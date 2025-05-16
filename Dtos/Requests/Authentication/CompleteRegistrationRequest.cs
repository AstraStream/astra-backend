using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Authentication {
    public sealed record CompleteRegistrationRequest {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;
    }
}