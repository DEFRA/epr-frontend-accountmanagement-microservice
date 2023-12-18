using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class InvitationToChangeSentViewModel
    {
        public Guid Id { get; set; }
        public string UserDisplayName { get; set; } = default!;
    }
}