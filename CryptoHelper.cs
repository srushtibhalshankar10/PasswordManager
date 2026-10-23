using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager
{
    
public static class CryptoHelper
    {
        private const int Iterations = 100_000;

        public static byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(16);

        // Turns a password + salt into a 32-byte value (used as hash AND as AES key)
        public static byte[] DeriveBytes(string password, byte[] salt)
            => Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations,
                                         HashAlgorithmName.SHA256, 32);

        public static string HashMasterPassword(string password, byte[] salt)
            => Convert.ToBase64String(DeriveBytes(password, salt));

        public static bool VerifyMasterPassword(string password, string storedHash, byte[] salt)
        {
            byte[] attempt = DeriveBytes(password, salt);
            byte[] stored = Convert.FromBase64String(storedHash);
            return CryptographicOperations.FixedTimeEquals(attempt, stored);
        }

        public static (string cipherText, string iv) Encrypt(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();                       // new random IV for every entry

            using var encryptor = aes.CreateEncryptor();
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] cipher = encryptor.TransformFinalBlock(data, 0, data.Length);

            return (Convert.ToBase64String(cipher), Convert.ToBase64String(aes.IV));
        }

        public static string Decrypt(string cipherText, string iv, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = Convert.FromBase64String(iv);

            using var decryptor = aes.CreateDecryptor();
            byte[] cipher = Convert.FromBase64String(cipherText);
            byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plain);
        }
    }
}


