namespace FrontendAccountManagement.Core.Models.CompanyHouse
{
    public class Company
    {
        public Company()
        {
        }

        public Company(CompaniesHouseCompany? organisation) : this()
        {
            if (organisation == null)
            {
                throw new ArgumentException("Organisation cannot be null.");
            }

            CompaniesHouseNumber = organisation.Organisation.RegistrationNumber ?? string.Empty;
            Name = organisation.Organisation.Name ?? string.Empty;
            BusinessAddress = new Address(organisation.Organisation.RegisteredOffice);
            AccountCreatedOn = organisation.AccountCreatedOn;
        }

        public string Name { get; set; }

        public string CompaniesHouseNumber { get; set; }

        public string OrganisationId { get; set; }

        public Address BusinessAddress { get; set; }

        public bool AccountExists => AccountCreatedOn is not null;

        public DateTimeOffset? AccountCreatedOn { get; set; }
    }
}
