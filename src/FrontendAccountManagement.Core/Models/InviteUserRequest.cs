using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class InviteUserRequest
    {
        public InvitedUser InvitedUser { get; set; }

        public InvitingUser InvitingUser { get; set; }
    }
}