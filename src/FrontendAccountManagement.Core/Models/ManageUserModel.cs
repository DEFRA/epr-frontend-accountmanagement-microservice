using FrontendAccountManagement.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class ManageUserModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Guid PersonId { get; set; }
        public int PersonRoleId { get; set; }
        public int ServiceRoleId { get; set; }
        public string ServiceRoleKey { get; set; }
        public EnrolmentStatus EnrolmentStatus { get; set; }
        public bool IsRemoveable { get; set; }
        public Guid ConnectionId { get; set; }
    }
}