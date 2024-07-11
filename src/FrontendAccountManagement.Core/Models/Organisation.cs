using FrontendAccountManagement.Core.Models.CompanyHouse;

namespace FrontendAccountManagement.Core.Models
{
    public class Organisation
    {
        public string? Name { get; init; }

        public string? RegistrationNumber { get; init; }

        public RegisteredOfficeAddress? RegisteredOffice { get; init; }

        public OrganisationData? OrganisationData { get; init; }
    }
}
