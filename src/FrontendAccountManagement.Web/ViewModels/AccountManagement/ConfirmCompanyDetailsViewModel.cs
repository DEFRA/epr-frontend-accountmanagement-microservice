using FrontendAccountManagement.Core.Models.CompaniesHouse;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

[ExcludeFromCodeCoverage]
public class ConfirmCompanyDetailsViewModel
{
    public string CompanyName { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public AddressViewModel? BusinessAddress { get; set; }

    public string ExternalCompanyHouseChangeRequestLink { get; set; }
}