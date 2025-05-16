namespace Astra.Services.Interfaces {
    public interface IEncryptionService {
        public string Encrypt(string data);
        public string Decrypt(string cipher);
    }
}