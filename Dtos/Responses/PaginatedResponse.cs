using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;
using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Responses {
    public class PaginatedResult<T> {
        public int TotalCount { get; set; }
        public required IEnumerable<T> Data { get; set; }
        public string NextLink { get; set; } = string.Empty;
        public string PreviousLink { get; set; } = string.Empty;
        public bool HasNext { get; set; } 
        public bool HasPrevious { get; set; } 
    }

    public class Pagination<T> {
        private readonly IFusionCache _cache;

        public Pagination(IFusionCache cache) {
            _cache = cache;
        }

        public async Task<PaginatedResult<T>> GetPagedAsync(IQueryable<T> queryable, int page, int pageSize, Func<int, string> urlGenerator) {
            if (queryable == null) {
                throw new ArgumentNullException(nameof(queryable), "The queryable cannot be null");
            }

            if (page < 1) {
                throw new ArgumentNullException(nameof(page), "Page number must be greater than or equal to 1.");
            }

            string cacheKey = $"pagination:{typeof(T).Name}:{page}:{pageSize}";
            var paginatedResult = await _cache.GetOrSetAsync(
                cacheKey,
                async _ => {
                    int total = await queryable.CountAsync();
                    var items = await queryable
                            .Skip((page - 1))
                            .Take(pageSize)
                            .ToListAsync();

                    string nextLink = "";
                    string previousLink = "";
                    bool hasNext = (page * pageSize) < total;
                    bool hasPrevious = page > 1;

                    if (hasNext) nextLink = urlGenerator(page + 1);
                    if (hasPrevious) previousLink = urlGenerator(page - 1);

                    var result = new PaginatedResult<T> {
                        Data = items,
                        TotalCount = total,
                        NextLink = nextLink,
                        PreviousLink = previousLink,
                        HasNext = hasNext,
                        HasPrevious = hasPrevious
                    };
                    return result;
                }
            );
            return paginatedResult!;
        }
    }
}