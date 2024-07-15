using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models.CompaniesHouse
{
    [ExcludeFromCodeCoverage]
    public class AddressDto
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

        public CountryDto? Country { get; set; }

        public bool IsUkAddress { get; set; }
    }

    public class CountryDto
    {
        public string? Name { get; set; }

        public string? Iso { get; set; }

    }
}