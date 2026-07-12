using Events.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.DataAccess.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
	public void Configure(EntityTypeBuilder<Event> builder)
	{
		builder.ToTable("events");

		builder.HasKey(e => e.Id);
		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedNever();

		builder.Property(e => e.Title)
			.HasMaxLength(200)
			.IsRequired();

		builder.Property(e => e.Description)
			.HasMaxLength(500)
			.IsRequired(false);

		builder.OwnsOne(e => e.Period, period =>
		{
			period.Property(p => p.StartAt)
				.HasColumnName("StartAt")
				.HasColumnType("timestamp with time zone")
				.IsRequired();

			period.Property(p => p.EndAt)
				.HasColumnName("EndAt")
				.HasColumnType("timestamp with time zone")
				.IsRequired();
		});

		builder.Property(e => e.TotalSeats)
			.IsRequired();

		builder.Property(e => e.AvailableSeats)
			.IsRequired();

		builder.HasMany(e => e.Bookings)
			.WithOne(b => b.Event)
			.HasForeignKey(b => b.EventId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}