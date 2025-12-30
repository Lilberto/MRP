namespace DBConnection;
using Npgsql;

public static class DbFactory
{
    private static string ConnectionString => 
        Environment.GetEnvironmentVariable("MRP_CONNECTION")!; // ?? "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;" local Fallback

    public static NpgsqlConnection GetConnection() => new NpgsqlConnection(ConnectionString);
}