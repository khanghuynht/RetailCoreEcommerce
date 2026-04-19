using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(nameof(OrderItem));
        
        builder.Property(x => x.ProductName).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ProductTitle).IsRequired().HasMaxLength(512);
        builder.Property(x => x.SKU).IsRequired().HasMaxLength(128);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(2048);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}