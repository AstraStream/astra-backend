using Astra.Models;
using Astra.Database;
using Astra.Dtos.Responses; 
using Astra.Dtos.Queries.Chains;
using Astra.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Astra.Repositories {
    public class ChainRepository: IChainRepository {
        private readonly AstraDbContext _context;
        private readonly Pagination<Chain> _pagination; 

        public ChainRepository(
            AstraDbContext context, 
            Pagination<Chain> pagination
        ) {
            _context = context;
            _pagination = pagination;
        }

        public async Task Create(Chain chain) {
            await _context.Chains.AddAsync(chain);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<Chain>> FindAllPaginated(ChainQuery query) {
            // handle pagination and sorting
            var chains = _context.Chains.AsQueryable();
            string GenerateUrl(int p) => $"/chains?page={p}&pageSize={query.PageSize}";
            return await _pagination.GetPagedAsync(chains, query.Page, query.PageSize, GenerateUrl);
        }

        public async Task<List<Chain>> FindAll() {
            return await _context.Chains.ToListAsync();
        }

        public async Task<Chain?> Find(Guid uuid) {
            return await _context.Chains.FirstOrDefaultAsync(c => c.UUID == uuid);
        }

        public async Task Update(Chain chain, string name) {
            chain.Name = name;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Chain chain) {
            _context.Chains.Remove(chain);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(string name) {
            return await _context.Chains.AnyAsync(chain => chain.Name == name);
        }
    }
}