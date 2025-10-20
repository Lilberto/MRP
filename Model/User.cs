public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Token { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Statistik laut MRP Specification
    public int TotalRatings { get; set; } = 0;
    public double AverageScore { get; set; } = 0.0;
    public string? FavoriteGenre { get; set; }
}
