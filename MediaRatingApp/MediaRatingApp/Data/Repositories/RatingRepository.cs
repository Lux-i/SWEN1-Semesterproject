using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using Npgsql;

namespace MediaRatingApp.Data.Repositories
{
    class RatingRepository : IRatingRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public RatingRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        private async Task<NpgsqlConnection> OpenConnectionAsync() =>
            await _dataSource.OpenConnectionAsync();

        public async Task<Rating?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT id, user_id, media_id, stars,
                CASE WHEN is_confirmed THEN review ELSE NULL END AS review,
                is_confirmed, created_at, updated_at, like_count
                FROM media_rating_app.ratings
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return ReadRating(reader);
        }

        public async Task<Rating?> GetByUserAndMediaAsync(int userId, int mediaId)
        {
            const string sql = """
                SELECT id, user_id, media_id, stars,
                CASE WHEN is_confirmed THEN review ELSE NULL END AS review,
                is_confirmed, created_at, updated_at, like_count
                FROM media_rating_app.ratings
                WHERE user_id = @user_id AND media_id = @media_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("media_id", mediaId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return ReadRating(reader);
        }

        public async Task<List<Rating>> GetByMediaAsync(int mediaId)
        {
            const string sql = """
                SELECT id, user_id, media_id, stars,
                CASE WHEN is_confirmed THEN review ELSE NULL END AS review,
                is_confirmed, created_at, updated_at, like_count
                FROM media_rating_app.ratings
                WHERE media_id = @media_id;
                """;

            return await ReadManyAsync(sql, ("media_id", mediaId));
        }

        public async Task<List<Rating>> GetByUserAsync(int userId)
        {
            const string sql = """
                SELECT id, user_id, media_id, stars,
                CASE WHEN is_confirmed THEN review ELSE NULL END AS review,
                is_confirmed, created_at, updated_at, like_count
                FROM media_rating_app.ratings
                WHERE user_id = @user_id;
                """;

            return await ReadManyAsync(sql, ("user_id", userId));
        }

        public async Task<int> CreateAsync(Rating rating)
        {
            const string sql = """
                INSERT INTO media_rating_app.ratings
                    (user_id, media_id, stars, review)
                VALUES
                    (@user_id, @media_id, @stars, @review)
                RETURNING id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", rating.UserId);
            cmd.Parameters.AddWithValue("media_id", rating.MediaId);
            cmd.Parameters.AddWithValue("stars", (object?)rating.Stars ?? DBNull.Value);
            cmd.Parameters.AddWithValue("review", (object?)rating.Review ?? DBNull.Value);

            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<bool> UpdateAsync(Rating rating)
        {
            const string sql = """
                UPDATE media_rating_app.ratings
                SET stars = @stars,
                    review = @review
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", rating.Id);
            cmd.Parameters.AddWithValue("stars", (object?)rating.Stars ?? DBNull.Value);
            cmd.Parameters.AddWithValue("review", (object?)rating.Review ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> UpdateConfirmedAsync(int ratingId, bool confirmStatus, int userId)
        {
            const string sql = """
                UPDATE media_rating_app.ratings r
                SET is_confirmed = @is_confirmed
                FROM media_entries m
                WHERE r.id = @id AND r.media_id = m.id
                AND m.creator_id = @user_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", ratingId);
            cmd.Parameters.AddWithValue("is_confirmed", confirmStatus);
            cmd.Parameters.AddWithValue("user_id", userId);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM media_rating_app.ratings
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> LikeAsync(int userId, int ratingId)
        {
            const string sql = """
                INSERT INTO media_rating_app.rating_likes (user_id, rating_id)
                VALUES (@user_id, @rating_id)
                ON CONFLICT DO NOTHING;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("rating_id", ratingId);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> UnlikeAsync(int userId, int ratingId)
        {
            const string sql = """
                DELETE FROM media_rating_app.rating_likes
                WHERE user_id = @user_id AND rating_id = @rating_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("rating_id", ratingId);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> HasUserLikedAsync(int userId, int ratingId)
        {
            const string sql = """
                SELECT 1
                FROM media_rating_app.rating_likes
                WHERE user_id = @user_id AND rating_id = @rating_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("rating_id", ratingId);

            return await cmd.ExecuteScalarAsync() != null;
        }

        public async Task<int> GetLikeCountAsync(int ratingId)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM media_rating_app.rating_likes
                WHERE rating_id = @rating_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("rating_id", ratingId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        #region Helper Methods

        private static Rating ReadRating(NpgsqlDataReader reader)
        {
            return new Rating
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                MediaId = reader.GetInt32(2),
                Stars = reader.GetInt32(3),
                Review = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsConfirmed = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7),
                LikeCount = reader.GetInt32(8),
            };
        }

        private async Task<List<Rating>> ReadManyAsync(
            string sql,
            params (string, object)[] parameters
        )
        {
            var list = new List<Rating>();

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (name, value) in parameters)
                cmd.Parameters.AddWithValue(name, value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(ReadRating(reader));

            return list;
        }

        #endregion
    }
}
