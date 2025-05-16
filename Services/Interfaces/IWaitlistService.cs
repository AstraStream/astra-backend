using Astra.Models;
using Astra.Dtos.Entities;
using Astra.Dtos.Responses;
using Astra.Dtos.Queries.Waitlists;
using Astra.Dtos.Requests.Waitlists;

namespace Astra.Services.Interfaces {
    public interface IWaitlistService {
        public Task<ServiceResult> Create(StoreWaitlistRequest request);
        public Task<PaginatedResult<Waitlist>> FindAll(WaitlistQuery query);
    }
}