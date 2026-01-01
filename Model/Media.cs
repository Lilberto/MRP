public class Media
{
    public int id { get; set; }
    public int userid { get; set; }
    public string title { get; set; } = "";
    public string description { get; set; } = "";
    public string type { get; set; } = "";
    public int year { get; set; }
    public List<string> genres { get; set; } = new();
    public string agerating { get; set; } = "";
    public double score { get; set; }
    public DateTime created { get; set; }
}

public class MediaUpdateDto 
{
    public int id { get; set; }
    public int userid { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string type { get; set; }
    public int year { get; set; }
    public string agerating { get; set; }
    public List<string> genres { get; set; }
}

public class MediaSearchFilter
{
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public string? MediaType { get; set; }
    public int? ReleaseYear { get; set; }
    public string? AgeRestriction { get; set; }
    public double? MinRating { get; set; }
    public string? SortBy { get; set; }
}

public class MediaDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = ""; 
    public int Year { get; set; }
    public List<string> Genres { get; set; } = new();
    public string AgeRating { get; set; } = "";
    public double Score { get; set; }
    public int CreatorId { get; set; } 
}