SET search_path TO media_rating_app;

-- ################
-- Indexes for faster frequently used searches
-- ################
CREATE INDEX idx_media_type ON media_entries(media_type);

CREATE INDEX idx_ratings_media ON ratings(media_id);
CREATE INDEX idx_ratings_user ON ratings(user_id);

CREATE INDEX idx_groups_name ON media_groups(group_name);
CREATE INDEX idx_group_links_media ON media_group_links(media_id);
CREATE INDEX idx_group_links_group ON media_group_links(group_id);