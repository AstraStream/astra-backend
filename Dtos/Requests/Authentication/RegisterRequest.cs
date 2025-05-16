using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Authentication {
    public sealed record RegisterRequest {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}