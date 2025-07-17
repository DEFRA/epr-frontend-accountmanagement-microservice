namespace FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class PersonDetailsDto
{
    public DateTimeOffset CreatedOn { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string TelephoneNumber { get; set; } = null!;

    public bool IsDeleted { get; set; }
}