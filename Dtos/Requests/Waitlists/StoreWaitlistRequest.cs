using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Waitlists {
    public sealed record StoreWaitlistRequest {
        [Required]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}