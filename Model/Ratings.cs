using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class Rating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
    
    public bool CommentPublished { get; set; }
    
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class RatingHistoryDto
{
    public string MediaTitle { get; set; } = "";
    public string MediaType { get; set; } = "";
    public string MediaGenre { get; set; } = "";
    public string Username { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}