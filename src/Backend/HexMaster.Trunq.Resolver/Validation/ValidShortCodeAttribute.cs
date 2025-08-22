using System.ComponentModel.DataAnnotations;

namespace HexMaster.Trunq.Resolver.Validation;

/// <summary>
/// Validation attribute for short codes that uses the centralized ShortCodeValidator.
/// </summary>
public class ValidShortCodeAttribute : ValidationAttribute
{
    public ValidShortCodeAttribute() : base(ShortCodeValidator.ErrorMessage)
    {
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
            return true; // Let [Required] handle null validation

        if (value is string shortCode)
            return ShortCodeValidator.IsValid(shortCode);

        return false;
    }
}