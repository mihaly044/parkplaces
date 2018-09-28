using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace PPNetLib.Tcp
{
    public class Encrypter
    {
        private readonly byte[] _salt = new byte[] { 0x61, 0x2d, 0x46, 0x62,
                                                     0x60, 0x08, 0xe4, 0x49,
                                                     0x38, 0x21, 0x31, 0xb9,
                                                     0xa1, 0x7b, 0x05, 0x0c
                                                    };
        private readonly string _password;

        private byte[] _key;
        private byte[] _iv;

        public Encrypter(string password)
        {
            SetPassword(password);
        }

        public void SetPassword(string password)
        {
            using (var pdb = new Rfc2898DeriveBytes(password, _salt))
            {
                _key = pdb.GetBytes(32);
                _iv = pdb.GetBytes(16);
            }
        }

        public byte[] Encrypt(IEnumerable<byte> plain)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var rijndael = Rijndael.Create())
                {
                    rijndael.Key = _key;
                    rijndael.IV = _iv;
                    using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        foreach (var b in plain)
                            cryptoStream.WriteByte(b);

                        cryptoStream.Close();
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        public byte[] Decrypt(IEnumerable<byte> cipher)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var rijndael = Rijndael.Create())
                {
                    rijndael.Key = _key;
                    rijndael.IV = _iv;
                    using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        foreach (var b in cipher)
                            cryptoStream.WriteByte(b);

                        cryptoStream.Close();
                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}
