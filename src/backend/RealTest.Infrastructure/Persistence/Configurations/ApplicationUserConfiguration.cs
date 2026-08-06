using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealTest.Domain.Users;

namespace RealTest.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.UserType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(u => u.AccountStatus).HasConversion<string>().HasMaxLength(32).IsRequired();

        // Replaces Identity's default (non-unique) EmailIndex with a unique index — the
        // application requires one account per email address, enforced at the database level.
        builder.HasIndex(u => u.NormalizedEmail).IsUnique().HasDatabaseName("IX_AspNetUsers_NormalizedEmail_Unique");
    }
}
