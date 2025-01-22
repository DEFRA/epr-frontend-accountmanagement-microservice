﻿namespace FrontendAccountManagement.Core.Addresses
{
    public class Address
    {
        public Address()
        {
        }

        public Address(RegisteredOfficeAddress? office) : this()
        {
            SubBuildingName = office.SubBuildingName;
            BuildingName = office.BuildingName;
            BuildingNumber = office.BuildingNumber;
            Street = office.Street;
            Town = office.Town;
            County = office.County;
            Country = office.Country?.Name;
            Postcode = office.Postcode;
            Locality = office.Locality;
            DependentLocality = office.DependentLocality;
            IsManualAddress = false;
        }

        public string? AddressSingleLine { get; set; }

        public string? SubBuildingName { get; set; }

        public string? BuildingName { get; set; }

        public string? BuildingNumber { get; set; }

        public string? Street { get; set; }

        public string? Town { get; set; }

        public string? County { get; set; }

        public string? Country { get; set; }

        public string? Postcode { get; set; }

        public string? Locality { get; init; }

        public string? DependentLocality { get; init; }

        public bool IsManualAddress { get; set; }

        private string Separator => !string.IsNullOrEmpty(BuildingNumber) ? " " : "";

        private string BuildingNumberAndStreet => $"{BuildingNumber}{Separator}{Street}";

        public string?[] AddressFields => [SubBuildingName, BuildingName, BuildingNumberAndStreet, Town, County, Postcode];
    }

}
