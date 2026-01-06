CREATE SCHEMA IF NOT EXISTS media_rating_app;

ALTER DATABASE postgres
SET search_path TO media_rating_app;

DROP SCHEMA IF EXISTS public;