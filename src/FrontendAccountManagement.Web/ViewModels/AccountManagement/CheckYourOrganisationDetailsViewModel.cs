using FrontendAccountManagement.Web.Constants.Enums;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

/// <summary>
/// View model for the action CheckCompaniesHouseDetails
/// </summary>
public record CheckYourOrganisationDetailsViewModel
{
    /// <summary>
    /// gets the OrganisationId for the organisation
    /// This is not displayed to the user, but required
    /// for updating the nation id
    /// </summary>
    public Guid OrganisationId { get; init; }

    /// <summary>
    /// gets the TradingName for the organisation
    /// </summary>
    public string TradingName { get; init; }

    /// <summary>
    /// Gets the Address for the organisation
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// Gets the uk nation for the organisation
    /// </summary>
    public UkNation UkNation { get; init; }
}
