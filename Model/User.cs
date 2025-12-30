using System.Text.Json.Serialization;

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


public class UserProfileDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime MemberSince { get; set; }
    
    public int TotalMediaEntries { get; set; } // How many media entries the user has uploaded
    public int TotalRatingsGiven { get; set; } // How often has he rated
    public double AvgRatingReceived { get; set; } // How good are his uploads rated
    public string? FavoriteGenre { get; set; }
}