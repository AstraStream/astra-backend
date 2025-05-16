using System.Diagnostics.CodeAnalysis;

namespace Astra.Models {
    public class Waitlist {
        public int Id { get; set; }
        public Guid UUID { get; set; }

        [NotNull]
        public required string FullName { get; set; }
        
        [NotNull]
        public required string Email { get; set; } 

        public DateTime createdAt { get; set; }
    }
}