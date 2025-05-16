using Astra.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Astra.Configurations.Models {
    public class RewardConfiguration: IEntityTypeConfiguration<Reward> {
        public void Configure(EntityTypeBuilder<Reward> builder)
        {
            // make name column unique and define maximum length
            builder.Property(c => c.Points)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.TotalReedemed)
                .IsRequired()
                .HasMaxLength(10);

            // define chain and wallet relationship
            builder.HasKey(c => c.UserId);
        }
    }
}