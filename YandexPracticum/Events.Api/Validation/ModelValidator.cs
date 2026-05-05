using Events.Api.Contracts;

namespace Events.Api.Validation;

internal static class ModelValidator
{
	public static void Validate(CreateEventRequest eventData) //TODO FluentValidator
	{
		if (eventData.StartAt == default)
		{
			throw new ArgumentNullException(nameof(eventData.StartAt),
				"Дата начала события обязательна для заполнения");
		}

		if (eventData.EndAt == default)
		{
			throw new ArgumentNullException(nameof(eventData.EndAt),
				"Дата окончания события обязательна для заполнения");
		}
	}
	
	public static void Validate(UpdateEventRequest eventData)
	{
		if (eventData.StartAt == default)
		{
			throw new ArgumentNullException(nameof(eventData.StartAt),
				"Дата начала события обязательна для заполнения");
		}

		if (eventData.EndAt == default)
		{
			throw new ArgumentNullException(nameof(eventData.EndAt),
				"Дата окончания события обязательна для заполнения");
		}
	}
}