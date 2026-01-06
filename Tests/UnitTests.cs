using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

using Hash_util;
using Token;

namespace MRP.Tests
{
    public class UtilityClassesUnitTests
    {
        //############################################# //
        // HASH-KLASSE                                  //
        //############################################# //
        
        // 1: Prüft, dass GenerateSalt() immer einen 16 Zeichen langen String zurückgibt.
        [Fact]
        public void Hash_GenerateSalt_ReturnsStringWithLength16()
        {
            string salt = Hash.GenerateSalt();
            
            Assert.Equal(24, salt.Length);
        }
        
        // 2: Prüft, dass der generierte Salt nur hexadezimale Kleinbuchstaben und Zahlen enthält.
        [Fact]
        public void Hash_GenerateSalt_ContainsOnlyHexadecimalCharacters()
        {
            string salt = Hash.GenerateSalt();
            
            Assert.Matches("^[A-Za-z0-9+/]+={0,2}$", salt);
        }
        
        // 3: Prüft die statistische Eindeutigkeit von generierten Salts.
        [Fact]
        public void Hash_GenerateSalt_GeneratesUniqueValues()
        {
            const int sampleSize = 1000;
            var salts = new HashSet<string>();
            
            for (int i = 0; i < sampleSize; i++)
            {
                salts.Add(Hash.GenerateSalt());
            }
            
            Assert.Equal(sampleSize, salts.Count);
        }
        
        // 4: Prüft den Determinismus der HashPassword-Funktion.
        [Fact]
        public void Hash_HashPassword_SameInputs_ProduceIdenticalHashes()
        {
            string password = "SecurePassword123";
            string salt = "dGVzdHNhbHQxMjM0NTY3ODkw";
            
            string hash1 = Hash.HashPassword(password, salt);
            string hash2 = Hash.HashPassword(password, salt);
            
            Assert.Equal(hash1, hash2);
        }
        
        // 5: Prüft, dass unterschiedliche Passwörter (bei gleichem Salt) unterschiedliche Hashes erzeugen.
        [Fact]
        public void Hash_HashPassword_DifferentPasswords_ProduceDifferentHashes()
        {
            string salt = "Y29tbW9uc2FsdDEyMzQ1Njc4";
            string password1 = "Password123";
            string password2 = "Different456";
            
            string hash1 = Hash.HashPassword(password1, salt);
            string hash2 = Hash.HashPassword(password2, salt);
            
            Assert.NotEqual(hash1, hash2);
        }
        
        // 6: Prüft, dass gleiche Passwörter mit unterschiedlichen Salts unterschiedliche Hashes erzeugen.
        [Fact]
        public void Hash_HashPassword_DifferentSalts_ProduceDifferentHashes()
        {
            string password = "SamePassword";
            string salt1 = "Zmlyc3RzYWx0MTIzNDU2Nzg=";
            string salt2 = "c2Vjb25kc2FsdDg3NjU0MzIx";
            
            string hash1 = Hash.HashPassword(password, salt1);
            string hash2 = Hash.HashPassword(password, salt2);
            
            Assert.NotEqual(hash1, hash2);
        }
        
        // 7: Prüft, dass HashPassword Base64-kodierte Strings zurückgibt.
        [Fact]
        public void Hash_HashPassword_ReturnsValidBase64String()
        {
            string password = "TestPassword";
            string salt = "dGVzdHNhbHQxMjM0NTY3ODkw";
            
            string hash = Hash.HashPassword(password, salt);
            
            Assert.Matches("^[A-Za-z0-9+/]+={0,2}$", hash);
        }
        
        // 8: Prüft, dass HashPassword case-sensitive ist.
        [Fact]
        public void Hash_HashPassword_IsCaseSensitive()
        {
            string salt = "dGVzdHNhbHQxMjM0NTY3ODkw";
            string lowerCase = "password";
            string upperCase = "PASSWORD";
            string mixedCase = "Password";
            
            string hashLower = Hash.HashPassword(lowerCase, salt);
            string hashUpper = Hash.HashPassword(upperCase, salt);
            string hashMixed = Hash.HashPassword(mixedCase, salt);
            
            Assert.NotEqual(hashLower, hashUpper);
            Assert.NotEqual(hashLower, hashMixed);
            Assert.NotEqual(hashUpper, hashMixed);
        }
        
        //#############################################//
        // TOKENHASH                                   //
        //#############################################//
        
        // 9: Prüft die Grundfunktionalität von HashToken.
        [Fact]
        public void TokenHash_HashToken_ReturnsNonNullHash()
        {
            string token = "sample.jwt.token.123";
            
            string hash = TokenHash.HashToken(token);
            
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }
        
        // 10: Prüft den Determinismus von HashToken.
        [Fact]
        public void TokenHash_HashToken_SameInput_ProducesIdenticalOutput()
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.xyz";
            
            string hash1 = TokenHash.HashToken(token);
            string hash2 = TokenHash.HashToken(token);
            
            Assert.Equal(hash1, hash2);
        }
        
        // 11: Prüft, dass HashToken Base64-kodierte Strings zurückgibt.
        [Fact]
        public void TokenHash_HashToken_ReturnsBase64EncodedString()
        {
            string token = "another.test.token";
            
            string hash = TokenHash.HashToken(token);
            
            Assert.Matches("^[A-Za-z0-9+/]+={0,2}$", hash);
        }
        
        // 12: Prüft, dass unterschiedliche Tokens unterschiedliche Hashes erzeugen.
        [Fact]
        public void TokenHash_HashToken_DifferentTokens_ProduceDifferentHashes()
        {
            string token1 = "token.for.user.123";
            string token2 = "token.for.user.456";
            
            string hash1 = TokenHash.HashToken(token1);
            string hash2 = TokenHash.HashToken(token2);
            
            Assert.NotEqual(hash1, hash2);
        }
        
        // 13: Prüft die Verarbeitung von Leerstrings durch HashToken.
        [Fact]
        public void TokenHash_HashToken_EmptyString_ProducesValidHash()
        {
            string emptyToken = "";
            
            string hash = TokenHash.HashToken(emptyToken);
            
            Assert.NotNull(hash);
            Assert.Matches("^[A-Za-z0-9+/]+={0,2}$", hash);
        }
        
        // 14: Prüft, dass HashToken mit sehr langen Tokens korrekt umgeht.
        [Fact]
        public void TokenHash_HashToken_VeryLongToken_ProcessesCorrectly()
        {
            string longToken = new string('x', 10000); 
            
            string hash = TokenHash.HashToken(longToken);
            
            Assert.NotNull(hash);
            Assert.Matches("^[A-Za-z0-9+/]+={0,2}$", hash);
        }
        
        //#############################################//
        // SECURERANDOM                                //
        //#############################################//
        
        // 15: Prüft, dass GenerateTokenPart die angeforderte Länge korrekt generiert.
        [Fact]
        public void SecureRandom_GenerateTokenPart_ReturnsCorrectLength()
        {
            int requestedLength = 25;
            
            string token = SecureRandom.GenerateTokenPart(requestedLength);
            
            Assert.Equal(requestedLength, token.Length);
        }
        
        // 16: Prüft, dass generierte Token-Teile nur erlaubte Zeichen enthalten.
        [Fact]
        public void SecureRandom_GenerateTokenPart_ContainsOnlyAllowedCharacters()
        {
            int length = 50;
            
            string token = SecureRandom.GenerateTokenPart(length);
            
            Assert.Matches("^[A-Za-z0-9]+$", token);
        }
        
        // 17: Prüft die statistische Verteilung der generierten Zeichen.
        [Fact]
        public void SecureRandom_GenerateTokenPart_CharactersAreRandomlyDistributed()
        {
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = 1000;
            
            string token = SecureRandom.GenerateTokenPart(length);
            
            foreach (char expectedChar in allowedChars)
            {
                Assert.Contains(expectedChar.ToString(), token);
            }
        }
        
        // 18: Prüft die Eindeutigkeit mehrerer generierter Token-Teile.
        [Fact]
        public void SecureRandom_GenerateTokenPart_GeneratesUniqueTokens()
        {
            const int numberOfTokens = 100;
            var tokens = new HashSet<string>();
            
            for (int i = 0; i < numberOfTokens; i++)
            {
                tokens.Add(SecureRandom.GenerateTokenPart(20));
            }
            
            Assert.Equal(numberOfTokens, tokens.Count);
        }
        
        // 19: Prüft verschiedene Längen-Parameter (Boundary Testing).
        [Theory]
        [InlineData(1)]   // Minimale Länge
        [InlineData(10)]  // Standard-Länge
        [InlineData(32)]  // Typische Token-Länge
        [InlineData(100)] // Große Länge
        public void SecureRandom_GenerateTokenPart_VariousLengths_WorkCorrectly(int length)
        {
            string token = SecureRandom.GenerateTokenPart(length);
            
            Assert.Equal(length, token.Length);
            Assert.Matches("^[A-Za-z0-9]+$", token);
        }
        
        // 20: Prüft, dass GenerateTokenPart mit Länge 0 einen leeren String zurückgibt.
        [Fact]
        public void SecureRandom_GenerateTokenPart_ZeroLength_ReturnsEmptyString()
        {
            int zeroLength = 0;
            
            string token = SecureRandom.GenerateTokenPart(zeroLength);
            
            Assert.Equal("", token);
            Assert.Equal(0, token.Length);
        }
    }
}