using Npgsql;

namespace MRP.Data
{
    internal class DatabaseConnection
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Username=postgres;Password=mysecretpassword;Database=mrp";

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}
