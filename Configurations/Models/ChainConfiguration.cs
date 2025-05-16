using Astra.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Astra.Configurations.Models {
    public class ChainConfiguration: IEntityTypeConfiguration<Chain> {
        public void Configure(EntityTypeBuilder<Chain> builder)
        {
            // make name column unique and define maximum length
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(10);

            // define chain and wallet relationship
            builder.HasMany(c => c.Wallets)
                .WithOne(c => c.Chain)
                .HasForeignKey(c => c.ChainId);
        }
    }
}