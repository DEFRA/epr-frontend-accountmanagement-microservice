using FrontendAccountManagement.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class ConnectionWithEnrolments
    {
        public PersonRole PersonRole { get; set; } = default!;
        public Guid UserId { get; set; }
        public ICollection<EnrolmentsFromConnection> Enrolments { get; init; } = default!;
    }
    
    [ExcludeFromCodeCoverage]
    public class EnrolmentsFromConnection
    {
        public string ServiceRoleKey { get; set; } = default!;
        public EnrolmentStatus EnrolmentStatus { get; set; } = default!;
    }
}
