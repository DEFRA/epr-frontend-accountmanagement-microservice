using FrontendAccountManagement.Core.Sessions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement
{
    [ExcludeFromCodeCoverage]
    public class ChangeAccountPermissionViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "ChangeAccountPermission.PermissionType.ErrorMessage")]
        public PermissionType PermissionType { get; set; }
        public bool ShowDelegatedContent { get; set; }
        public string ServiceKey { get; set; } = string.Empty;
    }
}