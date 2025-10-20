public class Media
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string MediaType { get; set; } = "";
    public int ReleaseYear { get; set; } = 0;
    public List<string> Genres { get; set; } = new List<string>();
    public int AgeRestriction { get; set; } = 0;
    public List<int> RatingCount { get; set; } = new List<int>();
    public double AverageScore
    {
        get
        {
            if (RatingCount == null || RatingCount.Count == 0) return 0;

            return RatingCount.Average();
        }
    }

    public List<string> Favorites { get; set; } = new List<string>();
}







// Test repository with some sample media data
public static class MediaRepository
{
    private static List<Media> _media = new List<Media>
    {
        new Media
        {
            Title = "1",
            Description = "Ein spannender Actionfilm.",
            MediaType = "movie",
            ReleaseYear = 2022,
            Genres = new List<string> { "Action", "Adventure" },
            AgeRestriction = 16,
            RatingCount = new List<int> {1},
            //AverageScore = 4.5,
            Favorites = new List<string> { "alice", "bob" }
        },
        new Media
        {
            Title = "2",
            Description = "Lustige Serie für die ganze Familie.",
            MediaType = "series",
            ReleaseYear = 2021,
            Genres = new List<string> { "Comedy", "Family" },
            AgeRestriction = 12,
            RatingCount = new List<int> {1, 2},
            //AverageScore = 4.0,
            Favorites = new List<string> { "charlie" }
        },
        new Media
        {
            Title = "3",
            Description = "Ein aufregendes Abenteuer-Spiel.",
            MediaType = "game",
            ReleaseYear = 2023,
            Genres = new List<string> { "Adventure", "Fantasy" },
            AgeRestriction = 16,
            RatingCount = new List<int> {1, 2, 3},
            //AverageScore = 4.7,
            Favorites = new List<string> { "alice" }
        },
        new Media
        {
            Title = "4",
            Description = "Ein packender Dramenfilm.",
            MediaType = "movie",
            ReleaseYear = 2020,
            Genres = new List<string> { "Drama" },
            AgeRestriction = 18,
            RatingCount = new List<int> {1, 2, 3, 4},
            //AverageScore = 4.2,
            Favorites = new List<string> { "bob", "charlie" }
        }
    };

    public static Media? Validate(string title, string description, string mediaType, int releaseYear, 
                                  List<string>? genres = null, int? ageRestriction = null)
    {
        return _media.FirstOrDefault(m => 
            m.Title == title &&
            m.Description == description &&
            m.MediaType == mediaType &&
            m.ReleaseYear == releaseYear &&
            (genres == null || m.Genres.SequenceEqual(genres)) &&
            (ageRestriction == null || m.AgeRestriction == ageRestriction)
        );
    }

    public static List<Media> GetAll()
    {
        return _media;
    }

}