using Astra.Models;
using Astra.Dtos.Entities;
using Astra.Dtos.Responses;
using Astra.Dtos.Queries.Chains;
using Astra.Dtos.Requests.Chains;
using Astra.Dtos.Responses.Chains;

namespace Astra.Services.Interfaces {
    public interface IChainService {
        public Task<ServiceResult> Create(StoreChainRequest request);
        public Task<List<ChainResponse>> FindAll();
        public Task<PaginatedResult<ChainResponse>> FindAllPaginated(ChainQuery query);
        public Task<(ServiceResult, ChainResponse?)> Find(Guid uuid);
        public Task<ServiceResult> Update(Guid uuid, UpdateChainRequest request);
        public Task<ServiceResult> Delete(Guid uuid);
    }
}