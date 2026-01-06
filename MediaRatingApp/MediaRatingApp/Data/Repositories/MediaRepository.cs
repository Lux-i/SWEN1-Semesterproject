using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Models.Enums;
using Npgsql;

namespace MediaRatingApp.Data.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public MediaRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<NpgsqlConnection> OpenConnectionAsync() =>
            await _dataSource.OpenConnectionAsync();

        public async Task<Media?> GetByIdAsync(int id)
        {
            var sql = """
                SELECT id, creator_id, title, media_description, media_type,
                       release_date, age_restriction, artwork_url,
                       avg_rating, rating_count, created_at, updated_at
                FROM media_entries
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapMedia(reader);
            }

            return null;
        }

        public async Task<List<Media>> GetAllAsync()
        {
            var sql = """
                SELECT id, creator_id, title, media_description, media_type,
                       release_date, age_restriction, artwork_url,
                       avg_rating, rating_count, created_at, updated_at
                FROM media_entries;
                """;

            var list = new List<Media>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMedia(reader));
            }

            return list;
        }

        public async Task<int> CreateAsync(Media media)
        {
            var sql = """
                INSERT INTO media_entries
                    (creator_id, title, media_description, media_type, release_date, age_restriction, artwork_url)
                VALUES
                    (@creator_id, @title, @media_description, @media_type::media_type, @release_date, @age_restriction, @artwork_url)
                RETURNING id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("creator_id", media.CreatorId);
            cmd.Parameters.AddWithValue("title", media.Title);
            cmd.Parameters.AddWithValue(
                "media_description",
                media.MediaDescription ?? (object)DBNull.Value
            );
            cmd.Parameters.AddWithValue("media_type", media.MediaType.ToString().ToLower());
            cmd.Parameters.AddWithValue(
                "release_date",
                media.ReleaseDate.HasValue ? media.ReleaseDate.Value : (object)DBNull.Value
            );
            cmd.Parameters.AddWithValue(
                "age_restriction",
                media.AgeRestriction ?? (object)DBNull.Value
            );
            cmd.Parameters.AddWithValue("artwork_url", media.ArtworkUrl ?? (object)DBNull.Value);

            int id = (int)await cmd.ExecuteScalarAsync();
            return id;
        }

        public async Task<bool> UpdateAsync(Media media)
        {
            var sql = """
                UPDATE media_entries
                SET title = @title,
                    media_description = @media_description,
                    media_type = @media_type::media_type,
                    release_date = @release_date,
                    age_restriction = @age_restriction,
                    artwork_url = @artwork_url
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", media.Id);
            cmd.Parameters.AddWithValue("title", media.Title);
            cmd.Parameters.AddWithValue(
                "media_description",
                media.MediaDescription ?? (object)DBNull.Value
            );
            cmd.Parameters.AddWithValue(
                "media_type",
                media.MediaType.ToString().ToLowerInvariant()
            );
            cmd.Parameters.AddWithValue(
                "release_date",
                media.ReleaseDate.HasValue ? media.ReleaseDate.Value : (object)DBNull.Value
            );
            cmd.Parameters.AddWithValue(
                "age_restriction",
                media.AgeRestriction ?? (object)DBNull.Value
            );
            cmd.Parameters.AddWithValue("artwork_url", media.ArtworkUrl ?? (object)DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = """
                DELETE FROM media_entries
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<Media>> GetByCreatorAsync(int creatorId)
        {
            var sql = """
                SELECT id, creator_id, title, media_description, media_type,
                       release_date, age_restriction, artwork_url,
                       avg_rating, rating_count, created_at, updated_at
                FROM media_entries
                WHERE creator_id = @creator_id;
                """;

            var list = new List<Media>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("creator_id", creatorId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMedia(reader));
            }

            return list;
        }

        public async Task<List<Media>> SearchByTitleAsync(string searchTerm, int limit = 50)
        {
            var sql = """
                SELECT id, creator_id, title, media_description, media_type,
                       release_date, age_restriction, artwork_url,
                       avg_rating, rating_count, created_at, updated_at
                FROM media_entries
                WHERE title_tsv @@ plainto_tsquery('english', @searchTerm)
                LIMIT @limit;
                """;

            var list = new List<Media>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("searchTerm", searchTerm);
            cmd.Parameters.AddWithValue("limit", limit);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMedia(reader));
            }

            return list;
        }

        public async Task<List<Media>> GetByGroupAsync(int groupId)
        {
            var sql = """
                SELECT me.id, me.creator_id, me.title, me.media_description, me.media_type,
                       me.release_date, me.age_restriction, me.artwork_url,
                       me.avg_rating, me.rating_count, me.created_at, me.updated_at
                FROM media_entries me
                JOIN media_group_links l ON me.id = l.media_id
                WHERE l.group_id = @group_id;
                """;

            var list = new List<Media>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("group_id", groupId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMedia(reader));
            }

            return list;
        }

        public async Task<List<Media>> GetWithFilterAsync(
            IEnumerable<KeyValuePair<string, object>> filter
        )
        {
            var sqlBuilder = new StringBuilder(
                """
                SELECT id, creator_id, title, media_description, media_type,
                       release_date, age_restriction, artwork_url,
                       avg_rating, rating_count, created_at, updated_at
                FROM media_entries
                """
            );

            if (filter.Any())
            {
                sqlBuilder.Append(" WHERE ");
                var conditions = new List<string>();
                int index = 0;
                foreach (var kvp in filter)
                {
                    string paramName = $"param{index++}";
                    conditions.Add($"{kvp.Key} = @{paramName}");
                }
                sqlBuilder.Append(string.Join(" AND ", conditions));
                sqlBuilder.Append(";");
            }

            var list = new List<Media>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sqlBuilder.ToString(), conn);
            int paramIndex = 0;
            foreach (var kvp in filter)
            {
                string paramName = $"param{paramIndex++}";
                cmd.Parameters.AddWithValue(paramName, kvp.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMedia(reader));
            }

            return list;
        }

        #region Helper Methods

        /// <summary>
        /// Maps a data reader row to a Media object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>A <see cref="Media"/> object</returns>
        private static Media MapMedia(NpgsqlDataReader reader)
        {
            return new Media
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                CreatorId = reader.GetInt32(reader.GetOrdinal("creator_id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                MediaDescription = reader.IsDBNull(reader.GetOrdinal("media_description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("media_description")),
                MediaType = Enum.Parse<MediaType>(
                    reader.GetString(reader.GetOrdinal("media_type")),
                    ignoreCase: true
                ),
                ReleaseDate = reader.IsDBNull(reader.GetOrdinal("release_date"))
                    ? null
                    : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("release_date"))),
                AgeRestriction = reader.IsDBNull(reader.GetOrdinal("age_restriction"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("age_restriction")),
                ArtworkUrl = reader.IsDBNull(reader.GetOrdinal("artwork_url"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("artwork_url")),
                AvgRating = reader.GetDecimal(reader.GetOrdinal("avg_rating")),
                RatingCount = reader.GetInt32(reader.GetOrdinal("rating_count")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            };
        }

        #endregion
    }

    public class FakeMediaRepository : IMediaRepository
    {
        private static ConcurrentDictionary<int, Media> _media = new();
        private static int _nextId = 1;

        public Task<Media?> GetByIdAsync(int id)
        {
            _media.TryGetValue(id, out var media);
            return Task.FromResult(media);
        }

        public Task<List<Media>> GetAllAsync()
        {
            return Task.FromResult(_media.Values.ToList());
        }

        public Task<int> CreateAsync(Media media)
        {
            int id = Interlocked.Increment(ref _nextId);
            media.Id = id;
            _media[id] = media;
            return Task.FromResult(id);
        }

        public Task<bool> UpdateAsync(Media media)
        {
            if (_media.ContainsKey(media.Id))
            {
                _media[media.Id] = media;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_media.TryRemove(id, out _));
        }

        public Task<List<Media>> GetByCreatorAsync(int creatorId)
        {
            var list = _media.Values.Where(m => m.CreatorId == creatorId).ToList();
            return Task.FromResult(list);
        }

        public Task<List<Media>> SearchByTitleAsync(string searchTerm, int limit = 50)
        {
            var list = _media
                .Values.Where(m => m.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();
            return Task.FromResult(list);
        }

        public Task<List<Media>> GetByGroupAsync(int groupId)
        {
            // Fake repository has no group link -> return empty list
            return Task.FromResult(new List<Media>());
        }

        public Task<List<Media>> GetWithFilterAsync(
            IEnumerable<KeyValuePair<string, object>> filter
        )
        {
            // Fake repository has no filtering logic -> return all
            return Task.FromResult(_media.Values.ToList());
        }
    }
}
