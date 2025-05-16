using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Astra.Models {
    public class Chain {
        public int Id { get; set; }

        [NotNull]
        public Guid UUID { get; set; }

        [NotNull]
        [MaxLength(20)]
        public string? Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<Wallet> Wallets { get; set; } = new();
    }
}