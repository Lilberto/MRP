using Npgsql;

namespace UserID;

public static class User_ID
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int UserID_DB(string token)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        string checkQuery = @"SELECT id FROM users WHERE token = @token"; //  AND token_created_at > NOW() - INTERVAL '168 hours'
        using var checkCmd = new NpgsqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@token", token);

        var result = checkCmd.ExecuteScalar();
        int Valid_UserID = (int)result!;

        return Valid_UserID;
    }
}