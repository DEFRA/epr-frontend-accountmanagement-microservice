using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class NameOfConsultancyViewModel
    {
        public Guid Id { get; set; }

        [MaxLength(160, ErrorMessage = "NameOfConsultancy.LengthErrorMessage")]
        [Required(ErrorMessage = "NameOfConsultancy.RequiredErrorMessage")]
        public string Name { get; set; } = default!;
    }
}