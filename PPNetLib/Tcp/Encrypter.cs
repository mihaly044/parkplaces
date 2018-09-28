using PPNetLib.Prototypes;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace PPNetLib.Tcp
{
    public class Encrypter
    {

        private readonly byte[] _key;
        private readonly byte[] _iv;

        public Encrypter(string keyfile = "keyfile.ppsk")
        {
            var data = File.ReadAllBytes(keyfile);
            using (var stream = new MemoryStream(data))
            {
                var secret = Serializer.Deserialize<EncryptionKey>(stream);
                using (var pdb = new Rfc2898DeriveBytes(secret.Password, secret.Salt))
                {
                    _key = pdb.GetBytes(32);
                    _iv = pdb.GetBytes(16);
                }
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
