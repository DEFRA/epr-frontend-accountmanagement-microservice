using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class ConfirmChangePermissionViewModel
    {
        public Guid Id { get; set; }
        public string? DisplayName { get; set; }
        public bool ApprovedByRegulator { get; set; }
        public YesNoAnswer? ConfirmAnswer { get; set; }
        public List<int>? NationIds { get; set; }
    }
}