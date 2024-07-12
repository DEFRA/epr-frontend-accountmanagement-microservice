using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models;

[ExcludeFromCodeCoverage]
public class Country
{
    public string? Name { get; init; }

    public string? Iso { get; init; }
}