using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Models.Enums;
using Npgsql;

namespace MediaRatingApp.Data.Repositories
{
    class FavoriteRepository : IFavoriteRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public FavoriteRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        private async Task<NpgsqlConnection> OpenConnectionAsync() =>
            await _dataSource.OpenConnectionAsync();

        #region Favorites

        public async Task<bool> AddFavoriteAsync(int userId, int mediaId) =>
            await SimpleInsertAsync("favorites", ("user_id", userId), ("media_id", mediaId));

        public async Task<bool> RemoveFavoriteAsync(int userId, int mediaId) =>
            await SimpleDeleteAsync("favorites", ("user_id", userId), ("media_id", mediaId));

        public async Task<bool> IsFavoriteAsync(int userId, int mediaId) =>
            await ExistsAsync("favorites", ("user_id", userId), ("media_id", mediaId));

        public async Task<List<Media>> GetUserFavoritesAsync(int userId) =>
            await GetUserMediaAsync("favorites", userId);

        #endregion

        #region Watchlist

        public async Task<bool> AddToWatchlistAsync(int userId, int mediaId) =>
            await SimpleInsertAsync("watchlists", ("user_id", userId), ("media_id", mediaId));

        public async Task<bool> RemoveFromWatchlistAsync(int userId, int mediaId) =>
            await SimpleDeleteAsync("watchlists", ("user_id", userId), ("media_id", mediaId));

        public async Task<bool> IsInWatchlistAsync(int userId, int mediaId) =>
            await ExistsAsync("watchlists", ("user_id", userId), ("media_id", mediaId));

        public async Task<List<Media>> GetUserWatchlistAsync(int userId) =>
            await GetUserMediaAsync("watchlists", userId);

        #endregion

        #region Helper Methods

        private async Task<bool> SimpleInsertAsync(string table, params (string, object)[] values)
        {
            var columns = string.Join(", ", values.Select(v => v.Item1));
            var parameters = string.Join(", ", values.Select(v => "@" + v.Item1));

            var sql = $"""
                INSERT INTO media_rating_app.{table} ({columns})
                VALUES ({parameters})
                ON CONFLICT DO NOTHING;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (name, value) in values)
                cmd.Parameters.AddWithValue(name, value);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        private async Task<bool> SimpleDeleteAsync(string table, params (string, object)[] keys)
        {
            var conditions = string.Join(" AND ", keys.Select(k => $"{k.Item1} = @{k.Item1}"));

            var sql = $"""
                DELETE FROM media_rating_app.{table}
                WHERE {conditions};
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (name, value) in keys)
                cmd.Parameters.AddWithValue(name, value);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        private async Task<bool> ExistsAsync(string table, params (string, object)[] keys)
        {
            var conditions = string.Join(" AND ", keys.Select(k => $"{k.Item1} = @{k.Item1}"));

            var sql = $"""
                SELECT 1
                FROM media_rating_app.{table}
                WHERE {conditions};
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (name, value) in keys)
                cmd.Parameters.AddWithValue(name, value);

            return await cmd.ExecuteScalarAsync() != null;
        }

        private async Task<List<Media>> GetUserMediaAsync(string table, int userId)
        {
            const string sqlTemplate = """
                SELECT m.id, m.creator_id, m.title, m.media_description, m.media_type,
                       m.release_date, m.age_restriction, m.artwork_url,
                       m.avg_rating, m.rating_count, m.created_at, m.updated_at
                FROM media_rating_app.{0} f
                JOIN media_rating_app.media_entries m ON m.id = f.media_id
                WHERE f.user_id = @user_id;
                """;

            var sql = string.Format(sqlTemplate, table);
            var list = new List<Media>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(
                    new Media
                    {
                        Id = reader.GetInt32(0),
                        CreatorId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        MediaDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                        MediaType = Enum.Parse<MediaType>(reader.GetString(4), ignoreCase: true),
                        ReleaseDate = reader.IsDBNull(5)
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(5)),
                        AgeRestriction = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        ArtworkUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                        AvgRating = reader.GetDecimal(8),
                        RatingCount = reader.GetInt32(9),
                        CreatedAt = reader.GetDateTime(10),
                        UpdatedAt = reader.GetDateTime(11),
                    }
                );
            }

            return list;
        }

        #endregion
    }
}
