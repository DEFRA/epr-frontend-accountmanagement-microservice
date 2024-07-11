namespace FrontendAccountManagement.Core.Models.CompanyHouse;

public class RegisteredOfficeAddress
{
    public string? SubBuildingName { get; init; }

    public string? BuildingName { get; init; }

    public string? BuildingNumber { get; init; }

    public string? Street { get; init; }

    public string? Town { get; init; }

    public string? County { get; init; }

    public string? Postcode { get; init; }

    public string? Locality { get; init; }

    public string? DependentLocality { get; init; }

    public Country? Country { get; init; }

    public bool? IsUkAddress { get; init; }
}
