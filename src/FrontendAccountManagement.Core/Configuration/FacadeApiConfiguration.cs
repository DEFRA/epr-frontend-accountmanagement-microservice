using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Configuration;

/// <summary>
/// This class contains data related to the configuration for the FacadeAPI section
/// </summary>
[ExcludeFromCodeCoverage]
public record FacadeApiConfiguration
{
    /// <summary>
    /// The section name to read config from
    /// </summary>
    public const string SectionName = "FacadeAPI";

    /// <summary>
    /// Gets or sets the base address of the facade API
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the downstream scope for the API
    /// </summary>
    public string DownStreamScope { get;set; }

    /// <summary>
    /// Gets or sets the path for the service roles
    /// </summary>
    public string ServiceRolesPath { get; set; }

    /// <summary>
    /// Gets or sets the path for getting the user accounts
    /// </summary>
    public string GetUserAccountPath { get; set; }

    /// <summary>
    /// Gets or sets the path for the service roles
    /// </summary>
    public string GetServiceRolesPath { get; set; }

    /// <summary>
    /// Gets or sets the path for companies house api
    /// </summary>
    public string GetCompanyFromCompaniesHousePath { get; set; }

    /// <summary>
    /// Gets or sets the path for updating the nation id of an organisation
    /// </summary>
    public string PutUpdateOrganisationPath { get; set; }

    /// <summary>
    /// Gets or sets the path for updating the user details
    /// </summary>
    public string PutUserDetailsByUserIdPath { get; set; }
}