using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;

namespace FrontendAccountManagement.Core.Sessions
{
    public class EditCompanyDetailsSession
    {
        public Nation? UkNation { get; set; }
        public string OrganisationName { get; set; }
        public string? OrganisationType { get; set; }
        public Guid? OrganisationId { get; set; }
        public Address? BusinessAddress { get; set; }
        public List<Address?> AddressesForPostcode { get; set; } = new();
        public bool IsUpdateCompanyAddress { get; set; }
        public bool IsUpdateCompanyName { get; set; }
    }
}