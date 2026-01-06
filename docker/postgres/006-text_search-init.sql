SET search_path TO media_rating_app;

ALTER TABLE media_entries
ADD COLUMN title_tsv tsvector
GENERATED ALWAYS AS (to_tsvector('english', title)) STORED;

CREATE INDEX idx_media_title_fts
ON media_entries
USING GIN(title_tsv);