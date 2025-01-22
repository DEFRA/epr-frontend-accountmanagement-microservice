using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Sessions.Interfaces;
using System;
using System.Collections.Generic;

namespace FrontendAccountManagement.Core.Sessions
{
    public class EditCompanyDetailsSession : IJourneySession
    {
        public List<string> Journey { get; set; } = new();
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
