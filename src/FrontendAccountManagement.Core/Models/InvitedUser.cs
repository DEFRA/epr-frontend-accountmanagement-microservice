using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class InvitedUser
    {
        public string Email { get; set; }

        public string RoleKey { get; set; }

        public string OrganisationName { get; set; }

        public Guid? OrganisationId { get; set; }
    }
}