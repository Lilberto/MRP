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
    public string agerating { get; set; } = "FSK0";
    



    // Nur Server (Client ignoriert wenn 0/leer)
    public double score { get; set; }
    public string creator { get; set; } = "";
    public DateTime created { get; set; }
}