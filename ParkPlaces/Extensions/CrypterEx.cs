using CryptSharp;
using CryptSharp.Utility;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ParkPlaces.Extensions
{
    // Reference: https://gist.github.com/georgepowell/956a5be1c4cc337bc57e
    public static class CrypterEx
    {
        private static int _difficulty = 10;

        // Generate a 16 byte salt using cryptographic random number generator.
        private static byte[] GenerateSalt()
        {
            // To be completely secure we should generate a salt from a cryptographical random number source.
            var prov = new RNGCryptoServiceProvider();
            byte[] salt = new byte[16];
            prov.GetBytes(salt);
            return salt;
        }

        // Generates a salt and calculates the bcrypt hash for the given password. Outputs the results to the console.
        public static string CalculateBCrypt(string password)
        {
            var passwordBytes = Encoding.Unicode.GetBytes(password); // Convert passwords to bytes.
            var salt = GenerateSalt(); // Generate byte array 16 long from cryptographically random source.

            var hash = BlowfishCipher.BCrypt(passwordBytes, salt, _difficulty); // Perform bcrypt algorithm with set difficulty.
            var hashString = Convert.ToBase64String(hash); // Convert hash to string for storing/printing

            return hashString;
        }

        public static bool ValidatePassword(string password, string hash)
        {
            return Crypter.CheckPassword(password, hash);
        }
    }
}
