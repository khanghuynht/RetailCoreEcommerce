using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Persistence.Configurations;

public class ProductRatingConfiguration : IEntityTypeConfiguration<ProductRating>
{
    public void Configure(EntityTypeBuilder<ProductRating> builder)
    {
        builder.ToTable(nameof(ProductRating));


        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Review).HasMaxLength(4000);


        builder.HasOne(x => x.Product)
            .WithMany(p => p.ProductRatings!)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(u => u.ProductRatings!)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}