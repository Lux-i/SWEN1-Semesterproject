SET search_path TO media_rating_app;

-- ################
-- ENFORCE MOVIE TYPE
-- ################
CREATE OR REPLACE FUNCTION enforce_movie_type()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM media_entries
        WHERE id = NEW.media_id AND media_type = 'movie'
    ) THEN
        RAISE EXCEPTION 'media_id % is not of type movie', NEW.media_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_enforce_movie_type
BEFORE INSERT OR UPDATE ON movie_details
FOR EACH ROW EXECUTE FUNCTION enforce_movie_type();

-- ################
-- ENFORCE SERIES TYPE
-- ################
CREATE OR REPLACE FUNCTION enforce_series_type()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM media_entries
        WHERE id = NEW.series_id AND media_type = 'series'
    ) THEN
        RAISE EXCEPTION 'series_id % is not of type series', NEW.series_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_enforce_series_type
BEFORE INSERT OR UPDATE ON seasons
FOR EACH ROW EXECUTE FUNCTION enforce_series_type();

-- ################
-- UPDATE MEDIA RATING STATS
-- ################
CREATE OR REPLACE FUNCTION update_media_rating_stats()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE media_entries
    SET rating_count = (
        SELECT COUNT(*)
        FROM ratings
        WHERE media_id = NEW.media_id
        AND stars IS NOT NULL
    ),
    avg_rating = (
        SELECT COALESCE(AVG(stars), 0)
        FROM ratings
        WHERE media_id = NEW.media_id
        AND stars IS NOT NULL
    )
    WHERE id = NEW.media_id;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_rating_change
AFTER INSERT OR UPDATE OR DELETE ON ratings
FOR EACH ROW EXECUTE FUNCTION update_media_rating_stats();

-- ################
-- GENERIC UPDATE updated_at TRIGGER
-- ################
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_media_updated_at
BEFORE UPDATE ON media_entries
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trg_ratings_updated_at
BEFORE UPDATE ON ratings
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ################
-- Update LikeCount on rating_likes change
-- ################
CREATE OR REPLACE FUNCTION update_rating_like_count()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE ratings
        SET like_count = like_count + 1
        WHERE id = NEW.rating_id;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE ratings
        SET like_count = GREATEST(like_count - 1, 0)
        WHERE id = OLD.rating_id;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_rating_likes_change
AFTER INSERT OR DELETE ON rating_likes
FOR EACH ROW EXECUTE FUNCTION update_rating_like_count();