namespace UserID;

using Npgsql;

//* utils
using DBConnection;

public static class User_ID
{
    public static async Task <int> UserID_DB(string token)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        string checkQuery = @"SELECT id FROM users WHERE token = @token AND token_created_at > NOW() - INTERVAL '168 hours';";
        using var checkCmd = new NpgsqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@token", token);

        var result = await checkCmd.ExecuteScalarAsync();
        int Valid_UserID = (int)result!;

        return Valid_UserID;
    }
}