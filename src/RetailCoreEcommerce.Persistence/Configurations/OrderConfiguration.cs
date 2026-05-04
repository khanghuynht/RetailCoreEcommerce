using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(nameof(Order));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderCode).IsRequired().HasMaxLength(64);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.RecipientName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(32);
        builder.Property(x => x.StreetAddress).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Province).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Ward).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ShippingFee).HasPrecision(18, 2);
        builder.Property(x => x.Notes).IsRequired().HasMaxLength(2000);

        builder.HasIndex(x => x.OrderCode).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.Orders!)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Convert enum to string
        builder.Property(x => x.Status).HasConversion(
            p => p.ToString(),
            p => Enum.Parse<OrderStatus>(p));
        

        // Convert nullable enum to string
        builder.Property(x => x.PaymentStatus).HasConversion(
            p => p.HasValue ? p.Value.ToString() : null,
            p => p != null ? Enum.Parse<PaymentStatus>(p) : (PaymentStatus?)null);
    }
}