using Astra.Models;
using Astra.Configurations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Astra.Database
{
    public class AstraDbContext : IdentityDbContext<User>
    {
        public AstraDbContext(DbContextOptions options) : base(options) { }
        
        public DbSet<Chain> Chains { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<Waitlist> Waitlists { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ChainConfiguration());
            builder.ApplyConfiguration(new WalletConfiguration());
            builder.ApplyConfiguration(new RewardConfiguration());
        }
    }
}