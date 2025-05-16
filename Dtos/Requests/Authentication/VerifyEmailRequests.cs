using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Authentication {
    public sealed record VerifyEmailRequest {
        [Required]
        [EmailAddress]
        public string Email {get; set;} = string.Empty;

        [Required]
        [StringLength(6)]
        public string Code {get; set;} = string.Empty;
    }
}