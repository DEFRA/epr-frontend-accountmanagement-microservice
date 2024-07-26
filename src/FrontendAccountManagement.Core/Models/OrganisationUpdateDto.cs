using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models;

/// <summary>
/// This dto is used for updating of an organisation details
/// </summary>
[ExcludeFromCodeCoverage]
public class OrganisationUpdateDto
{
    /// <summary>
    /// Gets or sets the name of the organisation
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the sub building name of the organisation
    /// </summary>
    public string SubBuildingName { get; init; }

    /// <summary>
    /// Gets or sets the building name of the organisation
    /// </summary>
    public string BuildingName { get; init; }

    /// <summary>
    /// gets or sets the building number of the organisation
    /// </summary>
    public string BuildingNumber { get; init; }

    /// <summary>
    /// Gets or sets the street name for the organisation
    /// </summary>
    public string Street { get; init; }

    /// <summary>
    /// Gets or sets the locality of the organisation
    /// </summary>
    public string Locality { get; init; }

    /// <summary>
    /// Gets or sets the dependant locality of the organisation
    /// </summary>
    public string DependentLocality { get; init; }

    /// <summary>
    /// Gets or sets the town of the organisation
    /// </summary>
    public string Town { get; init; }

    /// <summary>
    /// Gets or sets the county of the organisation
    /// </summary>
    public string County { get; init; }

    /// <summary>
    /// Gets or sets the country of the organisation
    /// </summary>
    public string Country { get; init; }

    /// <summary>
    /// Gets or sets the nation id for the organisation
    /// </summary>
    public int NationId { get; init; }
}
