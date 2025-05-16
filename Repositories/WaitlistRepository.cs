using Astra.Models;
using Astra.Database;
using Astra.Dtos.Responses;
using Astra.Dtos.Queries.Waitlists;
using Astra.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Astra.Repositories {
    public class WaitlistRepository: IWaitlistRepository {
        private readonly AstraDbContext _context;
        private readonly Pagination<Waitlist> _pagination; 

        public WaitlistRepository(
            AstraDbContext context, 
            Pagination<Waitlist> pagination
        ) {
            _context = context;
            _pagination = pagination;
        }

        public async Task Create(Waitlist waitlist) {
            await _context.Waitlists.AddAsync(waitlist);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<Waitlist>> FindAll(WaitlistQuery query) {
            // handle pagination and sorting
            var waitlists = _context.Waitlists.AsQueryable();
            string GenerateUrl(int p) => $"/api/waitlists?page={p}&pageSize={query.PageSize}";
            return await _pagination.GetPagedAsync(waitlists, query.Page, query.PageSize, GenerateUrl);
        }

        public async Task<bool> Exists(string email) {
            return await _context.Waitlists.AnyAsync(waitlist => waitlist.Email == email);
        }
    }
}