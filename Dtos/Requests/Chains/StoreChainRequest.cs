using System.ComponentModel.DataAnnotations;

namespace Astra.Dtos.Requests.Chains {
    public sealed record StoreChainRequest {
        [Required]
        [MaxLength(10)]
        public string Name { get; set; } = string.Empty;
    }
}