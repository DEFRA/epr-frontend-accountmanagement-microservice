using FrontendAccountManagement.Core.Models.CompaniesHouse;

namespace FrontendAccountManagement.Core.Sessions;

public record CompaniesHouseSession
{
    public CompaniesHouseResponse CompaniesHouseData { get; set; }
}