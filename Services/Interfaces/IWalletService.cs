using Astra.Models;
using System.Security.Claims;
using Astra.Dtos.Responses.Wallets;

namespace Astra.Services.Interfaces
{
    public interface IWalletService
    {
        public Task New(string id);
        public Task<List<WalletResponse>> FindAllByUser(ClaimsPrincipal principal);
        public Task<WalletResponse> FindByChainAndUser(ClaimsPrincipal principal, Guid ChainID);
        public Task<ulong> FetchSolanaBalance(string publicKey);
    }
}