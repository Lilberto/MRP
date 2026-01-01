using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Xunit;

// Re-use namespaces from the main project
using Hash_util;
using Token;

public class AuthIndependentTests
{
    // Test: Salt-Generator erzeugt genau 16 Zeichen.
    // Motivation: Die Salt-Länge ist Teil der Sicherheitsannahmen
    // und wird in anderen Teilen des Systems erwartet.
    [Fact]
    public void Hash_GenerateSalt_Length16()
    {
        var s = Hash.GenerateSalt();
        Assert.NotNull(s);
        Assert.Equal(16, s.Length);
    }

    // Test: Zwei aufeinanderfolgende Salts sollten unterschiedlich sein.
    // Motivation: Verhindert deterministische Hashes für gleiche Passwörter.
    [Fact]
    public void Hash_GenerateSalt_Unique()
    {
        var a = Hash.GenerateSalt();
        var b = Hash.GenerateSalt();
        Assert.NotEqual(a, b);
    }

    // Test: HashPassword gibt eine Base64-kodierte SHA256-Länge zurück.
    // Motivation: Sicherstellen, dass das Format (Base64) und die Länge
    // der Hash-Ausgabe wie erwartet sind (32 Bytes -> Base64 44 Zeichen).
    [Fact]
    public void Hash_HashPassword_Length44()
    {
        var hash = Hash.HashPassword("password123", "somesalt12345678");
        Assert.NotNull(hash);
        // SHA256 -> 32 bytes -> base64 length 44
        Assert.Equal(44, hash.Length);
    }

    // Test: Gleiches Passwort mit unterschiedlichem Salt -> unterschiedliche Hashes.
    // Motivation: Salt soll die Hash-Ausgabe diversifizieren, schützt gegen Rainbow-Tables.
    [Fact]
    public void Hash_HashPassword_DifferentSalt_ProducesDifferentHash()
    {
        var h1 = Hash.HashPassword("password123", "saltAAAA11111111");
        var h2 = Hash.HashPassword("password123", "saltBBBB22222222");
        Assert.NotEqual(h1, h2);
    }

    // Test: Unterschiedliche Passwörter mit gleichem Salt -> unterschiedliche Hashes.
    // Motivation: Gibt Sicherheit, dass Hash eindeutig auf Passwort reagiert.
    [Fact]
    public void Hash_HashPassword_DifferentPasswordDifferentHash()
    {
        var h1 = Hash.HashPassword("passwordA", "salt123456789012");
        var h2 = Hash.HashPassword("passwordB", "salt123456789012");
        Assert.NotEqual(h1, h2);
    }

    // Test: Token-Hash (SHA256 + Base64) hat erwartete Länge.
    // Motivation: Validiert das Format des Token-Hashes, wie er in DB gespeichert werden sollte.
    [Fact]
    public void TokenHash_HashToken_Length44()
    {
        var t = TokenHash.HashToken("mytoken-abc-123");
        Assert.NotNull(t);
        Assert.Equal(44, t.Length);
    }

    // Test: Hash-Funktion ist deterministisch für gleichen Input.
    // Motivation: Gleicher Token -> gleicher Hash (wichtige Eigenschaft für DB-Lookups).
    [Fact]
    public void TokenHash_HashToken_Deterministic()
    {
        var t1 = TokenHash.HashToken("same-token");
        var t2 = TokenHash.HashToken("same-token");
        Assert.Equal(t1, t2);
    }

    // Test: Der zurückgegebene Hash ist gültiges Base64.
    // Motivation: Verhindert Probleme beim Speichern/Übertragen von Token-Hashes.
    [Fact]
    public void TokenHash_Base64_CharactersValid()
    {
        var t = TokenHash.HashToken("another-token");
        // Should be valid base64
        Convert.FromBase64String(t); // will throw if invalid
    }

    // Test: Standard-Konstruktor von `Media` initialisiert `genres`.
    // Motivation: Vermeidet NullReferenceExceptions beim Zugriff auf `genres`.
    [Fact]
    public void Media_Defaults_GenresNotNull()
    {
        var m = new Media();
        Assert.NotNull(m.genres);
        Assert.Empty(m.genres);
    }

    // Test: JSON-Serialisierung/Deserialisierung für `MediaDto` erhält Felder.
    // Motivation: Verifiziert, dass DTOs korrekt serialisiert werden (z.B. API-Responses).
    [Fact]
    public void MediaDto_Serialization_RetainsFields()
    {
        var dto = new MediaDto
        {
            Id = 1,
            Title = "My Title",
            Description = "Desc",
            Type = "movie",
            Year = 2020,
            Genres = { "drama", "action" },
            AgeRating = "PG-13",
            Score = 4.2,
            CreatorId = 77
        };

        var json = JsonSerializer.Serialize(dto);
        var des = JsonSerializer.Deserialize<MediaDto>(json);
        Assert.NotNull(des);
        Assert.Equal(dto.Id, des.Id);
        Assert.Equal(dto.Title, des.Title);
        Assert.Equal(dto.Genres.Count, des.Genres.Count);
    }

    // Test: `User.CreatedAt` wird beim Erzeugen gesetzt und ist aktuell.
    // Motivation: Hilft sicherzustellen, dass Timestamps gesetzt werden (z.B. für MemberSince).
    [Fact]
    public void User_Defaults_CreatedAtRecent()
    {
        var u = new User();
        var delta = DateTime.Now - u.CreatedAt;
        Assert.True(delta.TotalSeconds < 10, "CreatedAt should be set to now by default");
    }

    // Test: Standardwerte in `Rating` (Flags und Zähler).
    // Motivation: Stellt sicher, dass neue Ratings nicht sofort veröffentlicht sind und Likes bei 0 beginnen.
    [Fact]
    public void Rating_DefaultFlags()
    {
        var r = new Rating();
        Assert.False(r.CommentPublished);
        Assert.Equal(0, r.LikesCount);
    }

    // Test: Validierung schlägt fehl, wenn `Stars` unter 1 liegt.
    // Motivation: Attribute `Range(1,5)` muss enforced werden.
    [Fact]
    public void Rating_Stars_Validation_FailsBelowRange()
    {
        var r = new Rating { Stars = 0 };
        var results = new System.Collections.Generic.List<ValidationResult>();
        var ctx = new ValidationContext(r);
        var valid = Validator.TryValidateObject(r, ctx, results, true);
        Assert.False(valid);
    }

    // Test: Validierung schlägt fehl, wenn `Stars` über 5 liegt.
    // Motivation: Verhindert ungültige Bewertungsskalen.
    [Fact]
    public void Rating_Stars_Validation_FailsAboveRange()
    {
        var r = new Rating { Stars = 6 };
        var results = new System.Collections.Generic.List<ValidationResult>();
        var ctx = new ValidationContext(r);
        var valid = Validator.TryValidateObject(r, ctx, results, true);
        Assert.False(valid);
    }

    // Test: Kommentar-Längenvalidierung (MaxLength) wird angewendet.
    // Motivation: Verhindert zu lange Kommentare, die DB/Frontend-Probleme verursachen können.
    [Fact]
    public void Rating_Comment_MaxLength_Validation()
    {
        var r = new Rating { Stars = 3, Comment = new string('a', 2000) };
        var results = new System.Collections.Generic.List<ValidationResult>();
        var ctx = new ValidationContext(r);
        var valid = Validator.TryValidateObject(r, ctx, results, true);
        Assert.False(valid);
    }

    // Test: Getter/Setter von `MediaUpdateDto` funktionieren.
    // Motivation: Stelle sicher, dass die DTO-Felder korrekt belegt und abrufbar sind.
    [Fact]
    public void MediaUpdateDto_Properties_SetAndGet()
    {
        var u = new MediaUpdateDto
        {
            id = 5,
            userid = 10,
            title = "T",
            description = "D",
            type = "series",
            year = 1999,
            agerating = "18+",
            genres = new System.Collections.Generic.List<string> { "sci-fi" }
        };

        Assert.Equal(5, u.id);
        Assert.Equal(10, u.userid);
        Assert.Contains("sci-fi", u.genres);
    }

    // Test: MediaSearchFilter speichert Filter-Kriterien.
    // Motivation: Sicherstellen, dass Such-/Filter-DTOs korrekt belegt werden.
    [Fact]
    public void MediaSearchFilter_SetValues()
    {
        var f = new MediaSearchFilter { Title = "abc", Genre = "drama", MinRating = 3.5 };
        Assert.Equal("abc", f.Title);
        Assert.Equal("drama", f.Genre);
        Assert.Equal(3.5, f.MinRating);
    }

    // Test: Manuelles Mapping von `Media` zu `MediaDto` erhält Werte.
    // Motivation: Simuliert die einfache Mapping-Logik, die in Endpoints genutzt wird.
    [Fact]
    public void Media_Mapping_To_MediaDto()
    {
        var m = new Media
        {
            id = 12,
            userid = 7,
            title = "X",
            description = "Y",
            type = "movie",
            year = 2001,
            agerating = "PG",
            score = 3.3,
            created = DateTime.UtcNow
        };
        m.genres.Add("thriller");

        var dto = new MediaDto
        {
            Id = m.id,
            Title = m.title,
            Description = m.description,
            Type = m.type,
            Year = m.year,
            Genres = m.genres,
            AgeRating = m.agerating,
            Score = m.score,
            CreatorId = m.userid
        };

        Assert.Equal(m.id, dto.Id);
        Assert.Equal(m.title, dto.Title);
        Assert.Equal(m.genres.Count, dto.Genres.Count);
    }

    // Test: `UserProfileDTO` hat default-Werte, z.B. leere Username-String.
    // Motivation: Verhindert Null-Referenzen beim Rendern eines leeren Profils.
    [Fact]
    public void UserProfileDTO_Defaults()
    {
        var up = new UserProfileDTO();
        Assert.Equal("", up.Username);
    }

    // Test: `RatingHistoryDto` Standardwerte sind gesetzt.
    // Motivation: DTO-Defaults sind wichtig für UI-Rendering ohne Null-Checks.
    [Fact]
    public void RatingHistoryDto_DefaultValues()
    {
        var rh = new RatingHistoryDto();
        Assert.Equal("", rh.MediaTitle);
        Assert.Equal(0, rh.Stars);
    }
}
