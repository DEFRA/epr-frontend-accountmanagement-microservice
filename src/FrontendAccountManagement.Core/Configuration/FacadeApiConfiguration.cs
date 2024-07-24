namespace FrontendAccountManagement.Core.Configuration
{
    public record FacadeApiConfiguration
    {
        public const string SectionName = "FacadeAPI";

        public string Address { get; set; }

        public string DownStreamScope { get;set; }

        public string ServiceRolesPath { get; set; }
        
        public string GetUserAccountPath { get; set; }

        public string GetServiceRolesPath { get; set; }

        public string GetCompanyFromCompaniesHousePath { get; set; }

        public string PutNationIdByOrganisationIdPath { get; set; }
    }
}
