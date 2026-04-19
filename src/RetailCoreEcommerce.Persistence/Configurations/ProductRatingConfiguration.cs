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
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}