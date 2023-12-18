using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountManagement.Core.Sessions;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class CheckDetailsSendInviteViewModel
    {
        public Guid Id { get; set; }

        public PermissionType? PermissionType { get; set; }

        public RelationshipWithOrganisation SelectedRelationshipWithOrganisation { get; set; }

        public string? AdditionalRelationshipInformation { get; set; }

        public string? NameOfConsultancy { get; set; }

        public string? NameOfOrganisation { get; set; }

        public string? NameOfComplianceScheme { get; set; }

        public string? JobTitle { get; set; }

        [MaxLength(200, ErrorMessage = "CheckDetailsSendInvite.FullNameMaxLengthError")]
        [Required(ErrorMessage = "CheckDetailsSendInvite.FullNameError")]
        public string? Fullname { get; set; }

        public string? InviteeFullname { get; set; }

    }
}