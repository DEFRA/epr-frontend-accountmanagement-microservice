using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class NameOfOrganisationViewModel
    {
        public Guid Id { get; set; }

        [MaxLength(160, ErrorMessage = "NameOfOrganisation.LengthErrorMessage")]
        [Required(ErrorMessage = "NameOfOrganisation.RequiredErrorMessage")]
        public string Name { get; set; } = default!;
    }
}