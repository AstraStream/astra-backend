using Astra.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Astra.Configurations.Models {
    public class WalletConfiguration: IEntityTypeConfiguration<Wallet> {
        public void Configure(EntityTypeBuilder<Wallet> builder) {
            // define foreign keys
            builder.HasKey(k => new { k.UserId, k.ChainId });

             // make public key required
            builder.Property(k => k.PublicKey)
                    .IsRequired();

            // define user wallet relationship
            builder.HasOne(k => k.User)
                    .WithMany(k => k.Wallets)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

            // define chain wallet relationship
            builder.HasOne(k => k.Chain)
                    .WithMany(k => k.Wallets)
                    .HasForeignKey(k => k.ChainId);

            // Make the combination of ChainId and UserId unique
            builder.HasIndex(k => new {k.UserId, k.ChainId})
                    .IsUnique();
        }
    }
}