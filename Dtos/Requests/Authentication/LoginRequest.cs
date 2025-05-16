using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Authentication
{
    public sealed record LoginRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}