SET search_path TO media_rating_app;

INSERT INTO users (id, username, pw_hash)
VALUES (0, 'DELETED_USER', '!')
ON CONFLICT DO NOTHING;