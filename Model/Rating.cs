public class Rating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaId { get; set; }
    public int Stars { get; set; } // 1–5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool Confirmed { get; set; } = false; 
    public List<int> LikedByUserIds { get; set; } = new List<int>();
}
