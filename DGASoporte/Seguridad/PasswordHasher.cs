using System.Security.Cryptography;

namespace DGASoporte.Seguridad
{
    public class PasswordHasher
    {
        // Parámetros recomendados
        private const int iteraciones = 100_000;
        private const int SaltTamaño = 16;   // 128 bits
        private const int LlaveTamaño = 32;   // 256 bits

        public static (string hash, string salt) Hash(string password)
        {
            // Generar sal
            byte[] salt = RandomNumberGenerator.GetBytes(SaltTamaño);

            // Derivar clave
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteraciones, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(LlaveTamaño);

            // Guardamos en Base64 para DB
            string hashB64 = Convert.ToBase64String(key);
            string saltB64 = Convert.ToBase64String(salt);

            return (hashB64, saltB64);
        }

        public static bool Verificar(string password, string saltBase64, string expectedHashBase64)
        {
            byte[] salt = Convert.FromBase64String(saltBase64);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteraciones, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(LlaveTamaño);

            string actualHashB64 = Convert.ToBase64String(key);
            // Comparación segura (tiempo constante)
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(actualHashB64),
                Convert.FromBase64String(expectedHashBase64)
            );
        }
    }
}
