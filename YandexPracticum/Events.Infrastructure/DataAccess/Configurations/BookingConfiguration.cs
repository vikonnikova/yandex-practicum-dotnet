using Events.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.DataAccess.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
	public void Configure(EntityTypeBuilder<Booking> builder)
	{
		builder.ToTable("bookings");
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedNever();

		builder.Property(b => b.EventId)
			.IsRequired();
		
		builder.Property(b => b.UserId)
			.IsRequired();

		builder.Property(e => e.CreatedAt)
			.HasColumnType("timestamp with time zone")
			.IsRequired();

		builder.Property(e => e.Status)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(b => b.ProcessedAt)
			.HasColumnType("timestamp with time zone")
			.IsRequired(false);

		builder.HasIndex(b => b.EventId);

		builder.HasOne(b => b.Event)
			.WithMany(e => e.Bookings)
			.HasForeignKey(b => b.EventId);

		builder.HasOne(b => b.User)
			.WithMany();
	}
}