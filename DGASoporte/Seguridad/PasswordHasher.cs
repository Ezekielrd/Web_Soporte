using System.Security.Cryptography;
using System.Text;

namespace DGASoporte.Seguridad
{
    public static class PasswordHasher
    {
        private const int Iteraciones = 100_000;
        private const int SaltTamaño = 16;  // 128 bits
        private const int LlaveTamaño = 32; // 256 bits

        public static (string hash, string salt) Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltTamaño);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iteraciones, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(LlaveTamaño);

            return (Convert.ToBase64String(key), Convert.ToBase64String(salt));
        }

        // ✅ Orden estándar: (password, hash, salt)
        public static bool Verificar(string password, string expectedHashBase64, string saltBase64)
        {
            if (string.IsNullOrWhiteSpace(expectedHashBase64) || string.IsNullOrWhiteSpace(saltBase64))
                return false;

            byte[] salt = Convert.FromBase64String(saltBase64);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iteraciones, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(LlaveTamaño);

            var stored = Convert.FromBase64String(expectedHashBase64);
            return CryptographicOperations.FixedTimeEquals(stored, key);
        }
    }
}
