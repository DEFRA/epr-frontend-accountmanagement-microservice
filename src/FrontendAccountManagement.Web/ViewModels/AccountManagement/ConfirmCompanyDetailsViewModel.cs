using System.Diagnostics.CodeAnalysis;
using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

[ExcludeFromCodeCoverage]
public class ConfirmCompanyDetailsViewModel
{
    public string CompanyName { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public Address? BusinessAddress { get; set; }

    public string ExternalCompanyHouseChangeRequestLink { get; set; }
}