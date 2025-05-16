using System.Text;
using Astra.Services.Interfaces;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Astra.Services {
    public class EncryptionService: IEncryptionService {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly IConfiguration _config;

        public EncryptionService(IConfiguration config) {
            _config = config;

            // Derive a 256-bit key from the provided string
            var encryptionKey = _config["Encryption:Key"]
                ?? throw new ArgumentNullException("Encryption key not configured");

            using (var deriveBytes = new Rfc2898DeriveBytes(
                encryptionKey,
                salt: Encoding.UTF8.GetBytes(config["Encryption:Salt"]!),
                iterations: 10000,
                HashAlgorithmName.SHA256
            )) {
                _key = deriveBytes.GetBytes(32);
                _iv = deriveBytes.GetBytes(16);
            }
        }
        
        public string Encrypt(string plainText) {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs)) {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            } catch (CryptographicException ex) {
                throw new CryptographicException("Encryption failed", ex);
            }
        }

        public string Decrypt(string cipherText) {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try {
                var cipherBytes = Convert.FromBase64String(cipherText);
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                using var ms = new MemoryStream(cipherBytes);
                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }  catch(CryptographicException ex) {
                throw new CryptographicException("Decryption failed", ex);
            }
        }
    }
}