using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable(nameof(ProductImage));

        builder.Property(x => x.Name).HasMaxLength(256);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2048);

        // Store Cloudinary public id for deletion
        builder.Property(x => x.PublicId).IsRequired().HasMaxLength(512);
    }
}