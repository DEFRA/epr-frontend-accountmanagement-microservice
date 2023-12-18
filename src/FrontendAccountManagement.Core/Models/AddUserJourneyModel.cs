using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class AddUserJourneyModel
    {
        public string Email { get; set; }

        public string UserRole { get; set; }
    }
}