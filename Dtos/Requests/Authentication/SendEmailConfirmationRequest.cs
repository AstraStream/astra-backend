using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Authentication {
    public sealed record SendEmailConfirmationRequest {
        [Required]
        [EmailAddress]
        public string Email {get; set;} = string.Empty;
    }
}