using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class ConnectionPerson
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
}