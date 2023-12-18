using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountManagement.Core.Sessions;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class RelationshipWithOrganisationViewModel
    {
        public Guid Id { get; set; }

        public Boolean IsComplianceScheme { get; set; }

        [Required(ErrorMessage = "RelationshipWithOrganisation.SelectHowTheyWork")]
        public RelationshipWithOrganisation SelectedRelationshipWithOrganisation { get; set; }

        public string? AdditionalRelationshipInformation { get; set; }
    }
}