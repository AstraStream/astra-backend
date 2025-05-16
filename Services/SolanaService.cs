using Astra.Services.Interfaces;
using Solnet.Rpc;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;

namespace Astra.Services
{
    public class SolanaService : ISolanaService
    {
        private readonly IRpcClient _client;

        public SolanaService(IRpcClient client)
        {
            _client = client;
        }

        public Wallet CreateWallet()
        {
            var mnemonic = new Mnemonic(WordList.English, WordCount.Twelve);
            return new Wallet(mnemonic);
        }

        public async Task<ulong> GetBalance(string publicKey)
        {
            var balanceResult = await _client.GetBalanceAsync(publicKey);

            if (balanceResult.WasSuccessful)
                return balanceResult.Result.Value / 1_000_000_000;

            throw new Exception($"Failed to get balance: {balanceResult.Reason}");
        }
    }
}