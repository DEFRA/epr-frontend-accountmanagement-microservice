using FrontendAccountManagement;
using FrontendAccountManagement.Web;
using FrontendAccountManagement.Web.ViewModels;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.ReExAccountManagement;

[ExcludeFromCodeCoverage]
public class RemoveReExTeamMemberConfirmationViewModel : RemoveTeamMemberConfirmationViewModel
{
    public Guid OrganisationId { get; set; }
    public int EnrolmentId { get; set; }
}