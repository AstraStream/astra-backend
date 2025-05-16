using Astra.Models;
using Astra.Dtos.Responses; 
using Astra.Dtos.Queries.Waitlists;
using Astra.Repositories.Interfaces;

namespace Astra.Repositories.Interfaces {
    public interface IWaitlistRepository {
        public Task Create(Waitlist waitlist);
        public Task<bool> Exists(string email);
        public Task<PaginatedResult<Waitlist>> FindAll(WaitlistQuery query);
    }
}