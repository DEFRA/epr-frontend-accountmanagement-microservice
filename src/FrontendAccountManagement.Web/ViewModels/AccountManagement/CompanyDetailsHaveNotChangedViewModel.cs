using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class CompanyDetailsHaveNotChangedViewModel
    {
        public Guid? Id { get; set; }

        public string CompanyName { get; set; }

        public string CompanyHouseNumber { get; set; }

        public string? BuildingName { get; set; }

        public string? SubBuildingName { get; set; }

        public string? BuildingNumber { get; set; }

        public string? Street { get; set; }

        public string? Locality { get; set; }

        public string? DependentLocality { get; set; }

        public string? Town { get; set; }

        public string? County { get; set; }

        public string? Country { get; set; }

        public string? Postcode { get; set; }

        public string CompaniesHouseChangeDetailsUrl { get; set; }
    }
}
