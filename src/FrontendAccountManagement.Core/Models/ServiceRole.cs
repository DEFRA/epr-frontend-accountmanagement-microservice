using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class ServiceRole
    {
        public int PersonRoleId { get; set; }
        public int ServiceRoleId { get; set; }
        public string Key { get; set; }
        public string DescriptionKey { get; set; }
    }
}