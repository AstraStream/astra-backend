using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Astra.Models
{
    public class User : IdentityUser
    {  
        public string? Gender { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;
        public bool TotpTwoFactorEnabled { get; set; }
        public string? TotpSecret { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Wallet> Wallets { get; set; } = new();
    }
}