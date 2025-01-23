namespace FrontendAccountManagement.Core.Services.Dto
{
    public class AddressLookupResponseAddress
    {
        public string? AddressLine { get; set; }
        public string? SubBuildingName { get; set; }
        public string? BuildingName { get; set; }
        public string? BuildingNumber { get; set; }
        public string? Street { get; set; }
        public string? Locality { get; set; }
        public string? DependentLocality { get; init; }
        public string? Town { get; set; }
        public string? County { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
        public int? XCoordinate { get; set; }
        public int? YCoordinate { get; set; }
        public string? UPRN { get; set; }
        public string? Match { get; set; }
        public string? MatchDescription { get; set; }
        public string? Language { get; set; }
    }
}
