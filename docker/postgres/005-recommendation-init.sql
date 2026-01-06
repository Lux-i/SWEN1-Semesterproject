SET search_path TO media_rating_app;

CREATE EXTENSION IF NOT EXISTS vector;

-- ################
-- MEDIA VECTORS
-- ################
ALTER TABLE media_entries
ADD COLUMN IF NOT EXISTS embedding VECTOR(64);

-- ################
-- USER VECTORS
-- ################
CREATE TABLE IF NOT EXISTS user_embeddings (
    user_id INT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    embedding VECTOR(64) DEFAULT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ################
-- SIMILARITY SEARCH INDEX
-- ################
CREATE INDEX idx_media_embedding
ON media_entries
USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);

-- ################
-- TIME DECAY FUNCTION
-- ################
CREATE OR REPLACE FUNCTION time_decay(ts TIMESTAMPTZ)
RETURNS FLOAT AS $$
DECLARE
    age_days FLOAT; -- age in days of given timestamp
    wanted_half_life_days FLOAT := 30.0;
    -- decay-constant = 0.693 (ln(2)) / half-life
    lambda FLOAT := LN(2) / wanted_half_life_days;
BEGIN
    age_days := EXTRACT(EPOCH FROM (NOW() - ts)) / 86400;
    RETURN EXP(-lambda * age_days); -- exponential decay
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- ################
-- COMPUTE USER EMBEDDING FUNCTION
-- ################
CREATE OR REPLACE FUNCTION compute_user_embedding(p_user_id INT)
RETURNS VOID AS $$
DECLARE
    result VECTOR(64);
BEGIN
    -- Sum weighted embeddings from favorites, watchlists, and ratings
    -- An array is created out of the weight so it can be cast into vector
    -- As pgvector seems to not support direct scalar * vector multiplication
    -- Weight is calculated by a hardcoded per category factor multiplied by time decay
    SELECT COALESCE(SUM(me.embedding * array_fill(src.weight::REAL, ARRAY[64])::vector), NULL)
    INTO result
    FROM (
        -- Favorites
        SELECT f.media_id, 2.0 * time_decay(f.created_at) AS weight
        FROM favorites f
        WHERE f.user_id = p_user_id

        UNION ALL

        -- Watchlist
        SELECT w.media_id, 1.0 * time_decay(w.created_at) AS weight
        FROM watchlists w
        WHERE w.user_id = p_user_id

        UNION ALL

        -- Ratings
        SELECT r.media_id, (r.stars::FLOAT / 5.0) * 1.8 * time_decay(r.updated_at) AS weight
        FROM ratings r
        WHERE r.user_id = p_user_id
        AND r.stars IS NOT NULL
    ) AS src
    JOIN media_entries me ON me.id = src.media_id
    WHERE me.embedding IS NOT NULL;

    -- Insert or update user's embedding
    INSERT INTO user_embeddings (user_id, embedding, updated_at)
    VALUES (p_user_id, result, CURRENT_TIMESTAMP)
    ON CONFLICT (user_id)
    DO UPDATE SET
        embedding = EXCLUDED.embedding,
        updated_at = CURRENT_TIMESTAMP;
END;
$$ LANGUAGE plpgsql;

-- ################
-- UPDATE USER VECTOR TRIGGERS
-- ################
CREATE OR REPLACE FUNCTION update_recommendation_vector()
RETURNS TRIGGER AS $$
DECLARE
    user_id INT;
BEGIN
    IF TG_OP = 'DELETE' THEN
        user_id := OLD.user_id;
    ELSE
        user_id := NEW.user_id;
    END IF;

    PERFORM compute_user_embedding(user_id);
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_favorites_update_vector
AFTER INSERT OR DELETE ON favorites
FOR EACH ROW EXECUTE FUNCTION update_recommendation_vector();

CREATE TRIGGER trg_watchlists_update_vector
AFTER INSERT OR DELETE ON watchlists
FOR EACH ROW EXECUTE FUNCTION update_recommendation_vector();

CREATE TRIGGER trg_ratings_update_vector
AFTER INSERT OR UPDATE OR DELETE ON ratings
FOR EACH ROW EXECUTE FUNCTION update_recommendation_vector();

-- ################
-- MEDIA EMBEDDING UPDATE VECTOR TRIGGER
-- ################
CREATE OR REPLACE FUNCTION media_embedding_update_recommendation_vectors()
RETURNS TRIGGER AS $$
DECLARE
    affected_user_id INT;
BEGIN
    FOR affected_user_id IN
        SELECT DISTINCT user_id
        FROM (
            SELECT user_id
            FROM favorites
            WHERE media_id = NEW.id

            UNION

            SELECT user_id
            FROM watchlists
            WHERE media_id = NEW.id

            UNION

            SELECT user_id
            FROM ratings
            WHERE media_id = NEW.id
        ) AS affected_users
    LOOP
        PERFORM compute_user_embedding(affected_user_id);
    END LOOP;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_media_embedding_update_vectors
AFTER UPDATE OF embedding ON media_entries
FOR EACH ROW EXECUTE FUNCTION media_embedding_update_recommendation_vectors();

-- ################
-- COMPUTE MEDIA EMBEDDING FUNCTION
-- ################
CREATE OR REPLACE FUNCTION compute_media_embedding(
    p_media_type media_type,
    p_release_date DATE,
    p_age_restriction INT,
    p_media_id INT
)
RETURNS VECTOR(64) AS $$
DECLARE
    base_array FLOAT[] := ARRAY[
        0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
        0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
        0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
        0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
        0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
        0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
        0.0,0.0,0.0,0.0
    ]; -- initial zero vector (float array)
    genre_list TEXT[];
    genre_count INT;
    genre_dim INT := 20; -- how many vector slots are dedicated to genres
    i INT;
    g TEXT;
    media_type_val FLOAT;
    release_val FLOAT;
    age_val FLOAT;
BEGIN
    -- Get genres for this media entry (ordered)
    SELECT ARRAY_AGG(g.group_name ORDER BY g.group_name)
    INTO genre_list
    FROM media_group_links l
    JOIN media_groups g ON g.id = l.group_id
    WHERE l.media_id = p_media_id
      AND g.group_type = 'genre';

    IF genre_list IS NOT NULL THEN
        FOR i IN 1..LEAST(array_length(genre_list, 1), genre_dim) LOOP
            base_array[i] :=
                (abs(mod(hashtext(genre_list[i]), 1000))::float) / 1000;
        END LOOP;
    END IF;

    -- Encode media_type into next dimension
    media_type_val :=
    CASE p_media_type
        WHEN 'movie' THEN 1.0
        WHEN 'series' THEN 2.0
        WHEN 'game' THEN 3.0
        WHEN 'book' THEN 4.0
        ELSE 0.0
    END;

    base_array[genre_dim + 1] := media_type_val;

    -- Encode release_date as timestamp float in next dimension
    release_val := COALESCE(EXTRACT(EPOCH FROM p_release_date)::FLOAT / 1e9, 0.0);

    base_array[genre_dim + 2] := release_val;

    -- Encode age_restriction in next dimension
    age_val := COALESCE(p_age_restriction::FLOAT, 0.0);

    base_array[genre_dim + 3] := age_val;

    RETURN base_array::vector;

END;
$$ LANGUAGE plpgsql;

-- ################
-- UPDATE MEDIA VECTOR TRIGGERS
-- ################
CREATE OR REPLACE FUNCTION update_media_embedding_on_media_change()
RETURNS TRIGGER AS $$
DECLARE
    computed_embedding VECTOR(64);
BEGIN
    -- Prevent recursive trigger calls
    IF pg_trigger_depth() > 1 THEN
        RETURN NEW;
    END IF;

    computed_embedding := compute_media_embedding(
    NEW.media_type,
    NEW.release_date,
    NEW.age_restriction,
    NEW.id
);

    NEW.embedding := computed_embedding;
    NEW.updated_at := NOW();

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_media_entries_update_embedding
BEFORE INSERT OR UPDATE OF
    media_type,
    release_date,
    age_restriction
ON media_entries
FOR EACH ROW EXECUTE FUNCTION update_media_embedding_on_media_change();

CREATE OR REPLACE FUNCTION update_media_embedding_on_group_link_change()
RETURNS TRIGGER AS $$
DECLARE
    m_id INT;
    new_embedding VECTOR(64);
BEGIN
    m_id := COALESCE(NEW.media_id, OLD.media_id);

    SELECT compute_media_embedding(
        me.media_type,
        me.release_date,
        me.age_restriction,
        me.id
    )
    INTO new_embedding
    FROM media_entries me
    WHERE me.id = m_id;

    UPDATE media_entries
    SET embedding = new_embedding,
        updated_at = NOW()
    WHERE id = m_id;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_media_group_links_update_embedding
AFTER INSERT OR DELETE ON media_group_links
FOR EACH ROW EXECUTE FUNCTION update_media_embedding_on_group_link_change();