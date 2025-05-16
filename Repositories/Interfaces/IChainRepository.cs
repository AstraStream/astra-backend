using Astra.Models;
using Astra.Dtos.Responses; 
using Astra.Dtos.Queries.Chains;

namespace Astra.Repositories.Interfaces {
    public interface IChainRepository {
        public Task Create(Chain chain);
        public Task<List<Chain>> FindAll();
        public Task<PaginatedResult<Chain>> FindAllPaginated(ChainQuery query);
        public Task<Chain?> Find(Guid uuid);
        public Task Update(Chain chain, string name);
        public Task Delete(Chain chain);
        public Task<bool> Exists(string name);
    }
}