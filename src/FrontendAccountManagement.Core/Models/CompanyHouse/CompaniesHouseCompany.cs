namespace FrontendAccountManagement.Core.Models.CompanyHouse
{
    public class CompaniesHouseCompany
    {
        public Organisation? Organisation { get; init; }

        public bool AccountExists { get; set; }
        public DateTimeOffset? AccountCreatedOn { get; set; }
    }
}
