using System.Text.RegularExpressions;

namespace HexMaster.Trunq.Resolver.Validation;

/// <summary>
/// Centralized short code validation utilities for the Trunq application.
/// Ensures consistent validation rules across all components.
/// </summary>
public static partial class ShortCodeValidator
{
    /// <summary>
    /// The minimum allowed length for a short code.
    /// </summary>
    public const int MinLength = 4;

    /// <summary>
    /// The maximum allowed length for a short code.
    /// </summary>
    public const int MaxLength = 12;

    /// <summary>
    /// Regular expression pattern that matches valid short codes.
    /// Valid short codes are 4-12 characters long and contain only alphanumeric characters.
    /// </summary>
    public const string Pattern = @"^[a-zA-Z0-9]{4,12}$";

    /// <summary>
    /// Error message for invalid short codes.
    /// </summary>
    public const string ErrorMessage = "Short code must be 4-12 alphanumeric characters.";

    /// <summary>
    /// Compiled regex for efficient short code validation.
    /// </summary>
    [GeneratedRegex(Pattern, RegexOptions.Compiled)]
    private static partial Regex ShortCodeRegex();

    /// <summary>
    /// Validates whether a short code meets the required format.
    /// </summary>
    /// <param name="shortCode">The short code to validate.</param>
    /// <returns>True if the short code is valid, false otherwise.</returns>
    public static bool IsValid(string? shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            return false;

        return ShortCodeRegex().IsMatch(shortCode);
    }

    /// <summary>
    /// Validates a short code and throws an ArgumentException if invalid.
    /// </summary>
    /// <param name="shortCode">The short code to validate.</param>
    /// <param name="parameterName">The parameter name for the exception.</param>
    /// <exception cref="ArgumentException">Thrown when the short code is invalid.</exception>
    public static void ValidateAndThrow(string? shortCode, string parameterName = "shortCode")
    {
        if (!IsValid(shortCode))
        {
            throw new ArgumentException(ErrorMessage, parameterName);
        }
    }
}