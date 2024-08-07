using PhoneNumbers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FrontendAccountManagement.Web.ViewModels.Attributes;

/// <summary>
/// Checks that phone numbers are in a valid format.
/// </summary>
public class TelephoneNumberValidationAttribute : ValidationAttribute
{
    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var phoneNumber = value?.ToString() ?? string.Empty;

        return IsValid(phoneNumber) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }

    private static bool IsValid(string telephoneNumber)
    {
        try
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var phoneNumber = phoneNumberUtil.Parse(telephoneNumber, "GB");
            bool isValidNumber = phoneNumberUtil.IsValidNumber(phoneNumber);

            // libphonenumber is rather lenient with it's matching, allowing non-numeric characters if it thinks
            // they were typed by mistake or are part of a vanity number.
            // We want to be stricter about it and only allow numbers and the "+" symbol.
            bool isValidRegexMatch = Regex.IsMatch(telephoneNumber, @"^[+ 0-9()]*$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

            return isValidNumber && isValidRegexMatch;
        }
        catch (Exception ex) when (ex is RegexMatchTimeoutException || ex is NumberParseException)
        {
            return false;
        }
    }
}