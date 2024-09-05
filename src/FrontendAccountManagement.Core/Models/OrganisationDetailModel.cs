namespace FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class OrganisationDetailModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string OrganisationRole { get; set; }
    
    public string OrganisationType { get; set; }
    
    public string OrganisationNumber { get; set; }

    public int? NationId { get; set; }

    public string? OrganisationAddress { get; set; }

    public string? JobTitle { get; set; }
}