using MRP.Domain;
using Npgsql;
using System.Text.Json;

namespace MRP.Data
{
    internal class MediaRepository
    {
        private readonly DatabaseConnection database = new();

        public List<Media> GetAll()
        {
            List<Media> media = new();

            using var connection = database.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM media ORDER BY id";

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                media.Add(ReadMedia(reader));
            }

            return media;
        }

        public Media? GetById(int id)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM media WHERE id = @id";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
                return ReadMedia(reader);

            return null;
        }

        public int AddMedia(Media media)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql = @"
                INSERT INTO media
                (title, description, type, release_year,
                 genres, age_restriction, creator_id)
                VALUES
                (@title, @description, @type, @year,
                 @genres, @age, @creator)
                RETURNING id";

            using var command = new NpgsqlCommand(sql, connection);

            AddParameters(command, media);

            return (int)command.ExecuteScalar()!;
        }

        public bool UpdateMedia(Media media)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE media SET
                    title = @title,
                    description = @description,
                    type = @type,
                    release_year = @year,
                    genres = @genres,
                    age_restriction = @age
                WHERE id = @id AND creator_id = @creator";

            using var command = new NpgsqlCommand(sql, connection);

            AddParameters(command, media);
            command.Parameters.AddWithValue("id", media.Id);

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteMedia(int id, int creatorId)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql =
                "DELETE FROM media WHERE id = @id AND creator_id = @creator";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("creator", creatorId);

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            NpgsqlCommand command, Media media)
        {
            command.Parameters.AddWithValue("title", media.Title);
            command.Parameters.AddWithValue("description", media.Description);
            command.Parameters.AddWithValue("type", media.Type.ToString());
            command.Parameters.AddWithValue("year", media.ReleaseYear);
            command.Parameters.AddWithValue(
                "genres", JsonSerializer.Serialize(media.Genres));
            command.Parameters.AddWithValue("age", media.AgeRestriction);
            command.Parameters.AddWithValue("creator", media.CreatorId);
        }

        private static Media ReadMedia(NpgsqlDataReader reader)
        {
            return new Media
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Type = Enum.Parse<MediaType>(
                    reader.GetString(reader.GetOrdinal("type"))),
                ReleaseYear = reader.GetInt32(
                    reader.GetOrdinal("release_year")),
                Genres = JsonSerializer.Deserialize<List<string>>(
                    reader.GetString(reader.GetOrdinal("genres"))) ?? new(),
                AgeRestriction = reader.GetInt32(
                    reader.GetOrdinal("age_restriction")),
                CreatorId = reader.GetInt32(
                    reader.GetOrdinal("creator_id"))
            };
        }
    }
}
