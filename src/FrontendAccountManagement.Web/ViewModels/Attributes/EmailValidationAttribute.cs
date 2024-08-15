using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement.Web.UnitTests.Utilities;

namespace FrontendAccountManagement.Web.ViewModels.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EmailValidationAttribute : ValidationAttribute
{
    public EmailValidationAttribute(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null && !RegexUtilities.IsValidEmail(value.ToString() ?? string.Empty))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}