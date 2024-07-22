using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models.CompaniesHouse
{
    [ExcludeFromCodeCoverage]
    public class OrganisationDto
    {
        public string? Name { get; set; }

        public string? TradingName { get; set; }

        public string? RegistrationNumber { get; set; }

        public string? CompaniesHouseNumber { get; set; }

        public AddressDto? RegisteredOffice { get; set; }

        public OrganisationDataDto? OrganisationData { get; set; }
    }
}