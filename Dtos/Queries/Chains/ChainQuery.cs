using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Queries.Chains {
    public sealed record ChainQuery {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [AllowedValues("name")]
        public string? SortBy { get; set; } = "name";

        [AllowedValues("desc", "asc")]
        public string? Order { get; set; } = "asc"; 
    }
}