using Astra.Models;
using Astra.Database;
using Astra.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Astra.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AstraDbContext _context;

        public WalletRepository(AstraDbContext context)
        {
            _context = context;
        }

        public async Task<List<Wallet>> FindAll()
        {
            return await _context.Wallets.Include(c => c.Chain).ToListAsync();
        }

        public async Task<List<Wallet>> FindAllByUserID(string userID) {
            return await _context.Wallets
                .Include(c => c.Chain)
                .Where(x => x.UserId == userID)
                .ToListAsync();
        }

        public async Task<Wallet?> Find(Guid uuid)
        {
            return await _context.Wallets
                .Include(c => c.Chain)
                .FirstOrDefaultAsync(x => x.UUID == uuid);
        }

        public async Task<Wallet?> FindByChainAndUserID(Guid chainID, string userID) {
            return await _context.Wallets
                .Join(
                    _context.Chains,
                    wallet => wallet.ChainId,
                    chain => chain.Id,
                    (wallet, chain) => new { Wallet = wallet, Chain = chain }
                )
                .Where(x => x.Wallet.UserId == userID && x.Chain.UUID == chainID)
                .Select(x => x.Wallet)
                .Include(c => c.Chain)
                .FirstOrDefaultAsync();
        }

        public async Task Create(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Wallet wallet)
        {
            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();
        }
    }
}