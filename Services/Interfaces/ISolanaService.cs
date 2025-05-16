using Solnet.Wallet;

namespace Astra.Services.Interfaces
{
    public interface ISolanaService
    {
        public Wallet CreateWallet();
        public Task<ulong> GetBalance(string publicKey);
    }
}