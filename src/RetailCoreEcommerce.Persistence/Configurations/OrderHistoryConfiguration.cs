using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Persistence.Configurations;

public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
{
    public void Configure(EntityTypeBuilder<OrderHistory> builder)
    {
        builder.ToTable(nameof(OrderHistory));

        builder.Property(x => x.OldStatus).IsRequired().HasMaxLength(64);
        builder.Property(x => x.NewStatus).IsRequired().HasMaxLength(64);
    }
}