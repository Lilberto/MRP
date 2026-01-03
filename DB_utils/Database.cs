namespace DBConnection;
using Npgsql;
using DotNetEnv;

public static class DbFactory
{
    static DbFactory()
    {
        Env.Load("DB_utils/DBCon.env");
    }

    private static string ConnectionString => 
        Environment.GetEnvironmentVariable("MRP_CONNECTION") 
        ?? throw new Exception("Connection String nicht gefunden!");

    public static NpgsqlConnection GetConnection() => new NpgsqlConnection(ConnectionString);
}