using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class UserDetailsDto
    {
        public string FirstName { get; init; }

        public string LastName { get; init; }

        public string JobTitle { get; init; }

        public string TelePhone { get; init; }
    }
}
