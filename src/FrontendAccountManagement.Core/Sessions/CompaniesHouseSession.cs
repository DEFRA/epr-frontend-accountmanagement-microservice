using FrontendAccountManagement.Core.Models.CompaniesHouse;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Sessions;

[ExcludeFromCodeCoverage]
public record CompaniesHouseSession
{
    public CompaniesHouseResponse CompaniesHouseData { get; set; }
}