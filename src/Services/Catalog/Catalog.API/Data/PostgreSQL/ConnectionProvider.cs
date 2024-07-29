using Npgsql;

namespace Catalog.API.Data.PostgreSQL
{
    public interface INpgsqlConnectionProvider
    {
        NpgsqlConnection GetConnection();
    }

    public class NpgsqlConnectionProvider(IConfiguration configuration) : INpgsqlConnectionProvider
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("Connection string not found");

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}