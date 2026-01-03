using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } 

    public int MediaId { get; set; }
    public Media Media { get; set; }      

    [Range(1, 5)]
    public int Stars { get; set; } // 1–5
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
    public bool CommentPublished { get; set; } = false;

    public int LikesCount { get; set; } = 0;
    public bool LikedByMe { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


public class RatingHistoryDto
{
    public string MediaTitle { get; set; } = "";
    public string MediaType { get; set; } = "";
    public string MediaGenre { get; set; } = "";
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}