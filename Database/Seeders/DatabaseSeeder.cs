using Astra.Models;
using Astra.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Astra.Database.Seeders {
    public class DatabaseSeeder {
        private readonly ILogger<DatabaseSeeder> _logger;
        public DatabaseSeeder(ILogger<DatabaseSeeder> logger) => _logger = logger; 

        public async Task Initialize(IServiceProvider provider) {
            using (var context = new AstraDbContext(
                provider.GetRequiredService<DbContextOptions<AstraDbContext>>())
            ) {

                await context.Database.EnsureCreatedAsync(); // ensure database is created

                await SeedRoles(provider, context); // seed roles
                await SeedChains(context); // seed chains

                _logger.LogInformation("save changes to database");
                await context.SaveChangesAsync();
            }
        }

        public async Task SeedChains(AstraDbContext context) {
            _logger.LogInformation("check if chains table already has data in it.");
            if (!await context.Chains.AnyAsync()) {
                _logger.LogInformation("seeding chains to database");
                var chains = new List<Chain> {
                    new() { UUID = Guid.NewGuid(), Name = "xion" },
                    new() { UUID = Guid.NewGuid(), Name = "solana" }
                };

                _logger.LogInformation("add chains to database");
                await context.Chains.AddRangeAsync(chains);
            }
        }

        public async Task SeedRoles(IServiceProvider provider, AstraDbContext context) {
           var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
           _logger.LogInformation("check if roles table is empty");
           if (!await context.Roles.AnyAsync()) {
                List<IdentityRole> roles = new List<IdentityRole> {
                    new IdentityRole {
                        Name = "Streamer",
                        NormalizedName = "STREAMER"
                    },
                    new IdentityRole {
                        Name = "Administrator",
                        NormalizedName = "ADMINISTRATOR"
                    },
                    new IdentityRole {
                        Name = "Artist",
                        NormalizedName = "ARTIST"
                    }
                };

                _logger.LogInformation("Insert roles to database");
                foreach (var role in roles) {
                    await roleManager.CreateAsync(role);
                }
           }
        }
    }
}