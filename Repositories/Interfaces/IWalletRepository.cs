using Astra.Models;

namespace Astra.Repositories.Interfaces
{
    public interface IWalletRepository
    {
        public Task<List<Wallet>> FindAll();
        public Task<Wallet?> Find(Guid uuid);
        public Task<Wallet?> FindByChainAndUserID(Guid chainID, string userID);
        public Task<List<Wallet>> FindAllByUserID(string userID);
        public Task Create(Wallet wallet);
        public Task Delete(Wallet wallet);
    }
}