using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Queries.Waitlists {
    public sealed record WaitlistQuery {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [AllowedValues("desc", "asc")]
        public string? Order { get; set; } = "asc"; 
    }
}