using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(User));

        builder.Property(x => x.Username).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.Address).HasMaxLength(512);
        builder.Property(x => x.Province).HasMaxLength(128);
        builder.Property(x => x.Ward).HasMaxLength(128);

        builder.HasIndex(x => x.Username).IsUnique();
        
        // Convert enum to string
        builder.Property(x => x.Role).HasConversion(
            p => p.ToString(),
            p => Enum.Parse<UserRole>(p)
        );
    }
}