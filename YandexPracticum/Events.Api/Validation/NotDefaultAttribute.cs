using System.ComponentModel.DataAnnotations;

namespace Events.Api.Validation;

internal class NotDefaultAttribute : ValidationAttribute
{
	public override bool IsValid(object? value)
	{
		if (value is DateTime date)
			return date != default;

		return true;
	}
}