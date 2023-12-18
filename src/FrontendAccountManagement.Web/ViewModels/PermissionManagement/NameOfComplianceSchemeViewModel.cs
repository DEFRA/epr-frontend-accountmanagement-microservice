using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class NameOfComplianceSchemeViewModel
    {
        public Guid Id { get; set; }

        [MaxLength(160, ErrorMessage = "NameOfComplianceScheme.LengthErrorMessage")]
        [Required(ErrorMessage = "NameOfComplianceScheme.RequiredErrorMessage")]
        public string Name { get; set; } = default!;
    }
}