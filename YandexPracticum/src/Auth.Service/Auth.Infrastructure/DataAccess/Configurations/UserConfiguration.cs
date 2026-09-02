using Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(b => b.Login)
            .IsRequired();
        builder.HasIndex(b => b.Login)
            .IsUnique();

        builder.Property(e => e.PasswordHash)
            .IsRequired();

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<string>();
    }
}