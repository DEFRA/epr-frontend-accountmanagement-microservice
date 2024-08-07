using FrontendAccountManagement.Core.Models.CompaniesHouse;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class AddressViewModel
    {
        public string? SubBuildingName { get; set; }

        public string? BuildingName { get; set; }

        public string? BuildingNumber { get; set; }

        public string? Street { get; set; }

        public string? Town { get; set; }

        public string? County { get; set; }

        public string? Postcode { get; set; }

        public string? Locality { get; set; }

        public string? DependentLocality { get; set; }

        public string? Country { get; set; }

        private string Separator => !string.IsNullOrWhiteSpace(BuildingNumber) ? " " : "";

        private string BuildingNumberAndStreet => $"{BuildingNumber}{Separator}{Street}";

        public string?[] AddressFields => new[]
            { SubBuildingName, BuildingName, BuildingNumberAndStreet, Town, County, Postcode };
    }
}
