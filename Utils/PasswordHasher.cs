// Utils/PasswordHasher.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Inmobiliaria1.Utils
{

    public static class PasswordHasher
    {
        // Iteraciones y tamaño alineados PBKDF2-HMACSHA256
        private const int Iterations = 100_000;
        private const int SaltSize   = 16; // 128 bits
        private const int KeySize    = 32; // 256 bits

        public static string Hash(string password)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key  = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(key);
        }

        public static bool Verify(string password, string stored)
        {
            if (password is null || string.IsNullOrWhiteSpace(stored)) return false;
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var key  = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(parts[1]), key);
        }
    }
}
