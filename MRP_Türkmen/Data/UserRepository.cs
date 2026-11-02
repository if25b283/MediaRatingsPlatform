using MRP.Domain;
using Npgsql;

namespace MRP.Data
{
    internal class UserRepository
    {
        private readonly DatabaseConnection database = new();

        public bool UserExists(User user)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql =
                "SELECT COUNT(*) FROM users WHERE username = @username";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("username", user.Username);

            long count = (long)command.ExecuteScalar()!;

            return count > 0;
        }

        public bool AddUser(User user)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql =
                "INSERT INTO users (username, password_hash) " +
                "VALUES (@username, @password)";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("username", user.Username);
            command.Parameters.AddWithValue(
                "password", BCrypt.Net.BCrypt.HashPassword(user.Password));

            return command.ExecuteNonQuery() == 1;
        }

        public string? GetPasswordHash(string username)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql =
                "SELECT password_hash FROM users WHERE username = @username";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("username", username);

            return command.ExecuteScalar() as string;
        }


        public int? GetUserId(string username)
        {
            using var connection = database.GetConnection();
            connection.Open();

            string sql = "SELECT id FROM users WHERE username = @username";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("username", username);

            object? result = command.ExecuteScalar();

            return result == null ? null : Convert.ToInt32(result);
        }
    }
}
