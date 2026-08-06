using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealTest.Domain.Authentication;

namespace RealTest.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).HasMaxLength(256).IsRequired();
        builder.Property(t => t.CreatedByIpAddress).HasMaxLength(64).IsRequired();
        builder.Property(t => t.RevokedByIpAddress).HasMaxLength(64);

        // PostgreSQL has no SQL Server-style auto-updating rowversion column; the idiomatic
        // equivalent is the built-in "xmin" system column, mapped here as a shadow property
        // and used as an optimistic-concurrency token without adding any real schema.
        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasIndex(t => t.UserId).HasDatabaseName("IX_RefreshTokens_UserId");
        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("IX_RefreshTokens_TokenHash_Unique");
        builder.HasIndex(t => t.ExpiresAtUtc).HasDatabaseName("IX_RefreshTokens_ExpiresAtUtc");

        builder.HasOne<Domain.Users.ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
