using FrontendAccountManagement.Core.Sessions;

namespace FrontendAccountManagement.Core.Models
{   
    public class CurrentUserAccess
    {
        public PermissionType PermissionType { get; set; } = PermissionType.NotSet;
        public bool IsApprovedPerson { get; set; }
    }
}