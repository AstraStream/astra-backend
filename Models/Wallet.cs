using System.Diagnostics.CodeAnalysis;

namespace Astra.Models {
    public class Wallet {
        public int Id { get; set; }
        public Guid UUID { get; set; }

        [NotNull]
        public required string UserId { get; set; }

        [NotNull]
        public required int ChainId { get; set; }

        [NotNull]
        public required string PublicKey { get; set; } = string.Empty;
        public string? SecretKey { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public Chain? Chain { get; set; }
    }
}