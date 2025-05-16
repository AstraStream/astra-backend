using System.Diagnostics.CodeAnalysis;

namespace Astra.Models {
    public class Reward {
        public Guid UUID { get; set; }
        
        [NotNull]
        public required string UserId { get; set; }
        public int Points { get; set; }
        public int TotalReedemed { get; set; }
        public DateTime LastEarnedAt { get; set; } 
    }
}