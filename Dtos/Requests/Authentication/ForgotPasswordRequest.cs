using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Authentication {
    public sealed record ForgotPasswordRequest {
        [Required]
        [EmailAddress]
        public string Email {get; set;} = string.Empty;
    }
}