using System.ComponentModel.DataAnnotations;

public class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } 

    public int MediaId { get; set; }
    public Media Media { get; set; }      

    // rating
    public int Stars { get; set; } // 1–5
    public string? Comment { get; set; }
    public bool CommentPublished { get; set; } = false;

    // Likes
    public int LikesCount { get; set; } = 0;
    public bool LikedByMe { get; set; } = false;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}