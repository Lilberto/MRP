public class Media
{
    // Client & Server
    public int id { get; set; }
    public int userid { get; set; }
    public string title { get; set; } = "";
    public string description { get; set; } = "";
    public string type { get; set; } = "";
    public int year { get; set; }
    public List<string> genres { get; set; } = new();
    public string agerating { get; set; } = "";
    public double score { get; set; }
    public string creator { get; set; } = "";
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