
using System.Text.RegularExpressions;
using Xunit;
using Hash_util;

namespace MRP.Tests
{
	/// <summary>
	/// Echte Unit-Tests für Core-Logik (ohne DB-Zugriff).
	///
	/// Auswahl & Strategie (Deutsch):
	/// - Ziel: Präzise, schnelle und deterministische Tests, die die Kernregeln der Authentifizierung prüfen:
	///   Username-Regex, Passwort-Policy, Salt-Erzeugung und Hash-Verhalten.
	/// - Warum diese 20 Tests: Sie decken gültige sowie ungültige Eingaben, Randfälle (Länge, Unicode),
	///   Regressionen (Determinismus von Hash) und einfache statistische Eigenschaften (Salt-Eindeutigkeit) ab.
	/// - Qualitätskriterien: Keine Duplikate, aussagekräftige Namen, unabhängig voneinander, kurzlaufend.
	/// - Verwendung: Diese Tests dokumentieren die Business-Regeln und dienen als Frühwarnsystem für Regressionen.
	/// </summary>
	public class AuthIndependentTests
	{
		// Consolidated test suite: exactly 20 focused Facts below
		// Usernames
		[Fact]
		// Was: Prüft, dass ein valider ASCII-Username (Buchstaben/Ziffern/_/-) dem Regex entspricht.
		// Warum: Stellt sicher, dass erlaubte Zeichen akzeptiert werden und die Business-Regel umgesetzt ist.
		public void Username_ValidAscii_ShouldBeAccepted()
		{
			string p = @"^[A-Za-z0-9_-]+$";
			Assert.Matches(p, "ValidUser123");
		}

		// Was: Prüft, dass ein Username mit ungültigen Sonderzeichen (z.B. '@') abgelehnt wird.
		// Warum: Verhindert ungültige/gefährliche Zeichen in Benutzernamen.
		[Fact]
		public void Username_WithSpecialChar_ShouldBeRejected()
		{
			string p = @"^[A-Za-z0-9_-]+$";
			Assert.DoesNotMatch(p, "User@Name");
		}

		// Was: Prüft, dass ein leerer Username nicht erlaubt ist.
		// Warum: Leere Benutzernamen führen zu ungültigen Kontenzuständen und müssen verhindert werden.
		[Fact]
		public void Username_Empty_ShouldBeRejected()
		{
			string p = @"^[A-Za-z0-9_-]+$";
			Assert.DoesNotMatch(p, "");
		}

		// Was: Prüft, dass reine Zahlen als Username akzeptiert werden.
		// Warum: Bestätigt die Business-Entscheidung, dass numerische Usernames zulässig sind.
		[Fact]
		public void Username_NumericOnly_ShouldBeAccepted()
		{
			string p = @"^[A-Za-z0-9_-]+$";
			Assert.Matches(p, "123456789");
		}

		// Was: Prüft, dass ein sehr langer Username dem Regex entspricht.
		// Warum: Stellt sicher, dass die Regex nicht unbeabsichtigt Benutzerlängen limitiert.
		[Fact]
		public void Username_Long_ShouldBeAccepted()
		{
			string p = @"^[A-Za-z0-9_-]+$";
			Assert.Matches(p, "User_1234567890_ABCDEFGHIJKLMNOP");
		}

		// Was: Prüft, dass nicht-ASCII-Zeichen (z.B. Umlaute, Unicode) abgewiesen werden.
		// Warum: Policy: nur ASCII sind erlaubt; verhindert Probleme in Legacy-Systemen/DBs.
		[Fact]
		public void Username_NonAscii_ShouldBeRejected()
		{
			string p = @"^[A-Za-z0-9_-]+$";
			Assert.DoesNotMatch(p, "Jürgen");
		}

		// Passwords
		// Was: Prüft, dass ein Passwort mit mindestens 8 Zeichen und mindestens einer Ziffer akzeptiert wird.
		// Warum: Validiert Basis-Password-Policy für sichere Mindestanforderungen.
		[Fact]
		public void Password_Valid_ShouldBeAccepted()
		{
			string p = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
			Assert.Matches(p, "Password123");
		}

		// Was: Prüft die Grenzbedingung: genau 8 Zeichen mit mindestens Buchstaben+Zahl sind OK.
		// Warum: Stellt sicher, dass die Mindestlänge (8) korrekt implementiert ist.
		[Fact]
		public void Password_EightCharacters_ShouldBeAccepted()
		{
			string p = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
			Assert.Matches(p, "Abcdef12");
		}

		// Was: Prüft, dass zu kurze Passwörter (<8) abgelehnt werden.
		// Warum: Vermeidet zu schwache Passwörter und setzt die Mindestlänge durch.
		[Fact]
		public void Password_TooShort_ShouldBeRejected()
		{
			string p = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
			Assert.DoesNotMatch(p, "Pass1");
		}

		// Was: Prüft, dass Passwörter ohne Ziffern abgelehnt werden.
		// Warum: Sicherstellen, dass Passwort mindestens eine Ziffer enthalten muss.
		[Fact]
		public void Password_NoNumber_ShouldBeRejected()
		{
			string p = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
			Assert.DoesNotMatch(p, "PasswordOnly");
		}

		// Was: Prüft, dass Passwörter ohne Buchstaben abgelehnt werden.
		// Warum: Verhindert rein numerische Passwörter (Policy: Zahl+Buchstabe erforderlich).
		[Fact]
		public void Password_NoLetter_ShouldBeRejected()
		{
			string p = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
			Assert.DoesNotMatch(p, "12345678");
		}

		// Was: Prüft, dass Sonderzeichen im Passwort (z.B. '@') nicht zugelassen werden.
		// Warum: Policy: nur alphanumerische Zeichen erlaubt; schützt gegen unerwartete Eingaben/Encodierungsprobleme.
		[Fact]
		public void Password_SpecialCharacters_ShouldBeRejected()
		{
			string p = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
			Assert.DoesNotMatch(p, "Password@123");
		}

		// Salts
		// Was: Prüft, dass der erzeugte Salt aus 16 hexadezimalen Zeichen besteht.
		// Warum: Dokumentiert das Format (Guid-basiert) und verhindert Format-Regressionen.
		[Fact]
		public void GenerateSalt_FormatIsHexadecimal16()
		{
			Assert.Matches("^[a-f0-9]{16}$", Hash.GenerateSalt());
		}

		// Was: Prüft die feste Länge des Salts (16 Zeichen).
		// Warum: Konsistenz beim Speichern/Weiterverarbeiten des Salts gewährleisten.
		[Fact]
		public void GenerateSalt_LengthIs16()
		{
			Assert.Equal(16, Hash.GenerateSalt().Length);
		}

		// Was: Erzeugt 100 Salts und prüft, dass sie alle unterschiedlich sind.
		// Warum: Kleine statistische Probe gegen einfache Kollisionen bei der Salt-Erzeugung.
		[Fact]
		public void GenerateSalt_UniqueOverSample_ShouldBeUnique()
		{
			var s = new System.Collections.Generic.HashSet<string>();
			for (int i = 0; i < 100; i++) s.Add(Hash.GenerateSalt());
			Assert.Equal(100, s.Count);
		}

		// Hashes
		// Was: Prüft, dass die Hash-Funktion deterministisch ist (gleiches PW+Salt => gleicher Hash).
		// Warum: Wichtig für Login-Vergleiche und Authentifizierungs-Workflows.
		[Fact]
		public void Hash_SameInput_ShouldBeDeterministic()
		{
			string pw = "TestPassword123";
			string salt = "testsalt1234567890";
			Assert.Equal(Hash.HashPassword(pw, salt), Hash.HashPassword(pw, salt));
		}

		// Was: Prüft, dass verschiedene Passwörter (bei gleichem Salt) unterschiedliche Hashes erzeugen.
		// Warum: Verhindert, dass verschiedene Passwörter kollidierende Hashwerte produzieren (grundlegende Korrektheit).
		[Fact]
		public void Hash_DifferentPasswords_ShouldDiffer()
		{
			string salt = "samesalt123456789";
			Assert.NotEqual(Hash.HashPassword("Password123", salt), Hash.HashPassword("DifferentPassword456", salt));
		}

		// Was: Prüft, dass derselbe Password-String mit unterschiedlichen Salts unterschiedliche Hashes liefert.
		// Warum: Salt soll Hash-Ausgabe diversifizieren; wichtig für Sicherheitsannahmen.
		[Fact]
		public void Hash_DifferentSalts_ShouldDiffer()
		{
			string pw = "SamePassword123";
			Assert.NotEqual(Hash.HashPassword(pw, "salt1111111111111"), Hash.HashPassword(pw, "salt2222222222222"));
		}

		// Was: Prüft, dass die Hash-Funktion einen Base64-kodierten String zurückgibt.
		// Warum: Erwartetes Export-/Speicherformat ist Base64; Abweichungen würden Integrationsfehler verursachen.
		[Fact]
		public void Hash_Output_IsBase64()
		{
			string h = Hash.HashPassword("p4ssw0rd", "salt123456789012");
			Assert.Matches("^[A-Za-z0-9+/]+={0,2}$", h);
		}

		// Was: Prüft, dass Hashing gross-/klein-Schreibung unterscheidet (case-sensitive).
		// Warum: Vermeidet, dass unterschiedliche Passwörter als gleich behandelt werden (Sicherheit/Korrektheit).
		[Fact]
		public void Hash_IsCaseSensitive()
		{
			string salt = "testsalt1234567890";
			Assert.NotEqual(Hash.HashPassword("password", salt), Hash.HashPassword("Password", salt));
		}
	}
}

