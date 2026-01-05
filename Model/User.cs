using System.Text.Json.Serialization;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class User
{
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("password")]
    public string Password { get; set; } = "";

    public string? Token { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int TotalRatings { get; set; } = 0;
    public double AverageScore { get; set; } = 0.0;
    public string? FavoriteGenre { get; set; }
}

public class UserRegisterDTO
{
    [JsonRequired]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonRequired]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class UserProfileDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime MemberSince { get; set; }
    
    public int TotalMediaEntries { get; set; } 
    public int TotalRatingsGiven { get; set; } 
    public double AvgRatingReceived { get; set; } 
    public string? FavoriteGenre { get; set; }
}