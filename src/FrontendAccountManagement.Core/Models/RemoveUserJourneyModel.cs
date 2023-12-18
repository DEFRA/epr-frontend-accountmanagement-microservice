using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class RemoveUserJourneyModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid PersonId { get; set; }
    }
}