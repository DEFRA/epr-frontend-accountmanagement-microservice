using EPR.Common.Authorization.Interfaces;
using EPR.Common.Authorization.Models;

namespace FrontendAccountManagement.Core.Sessions;

public class JourneySession : IHasUserData
{
    public UserData UserData { get; set; } = new();
    public AccountManagementSession AccountManagementSession { get; set; } = new();
    public PermissionManagementSession PermissionManagementSession { get; set; } = new();
    public CompaniesHouseSession CompaniesHouseSession { get; set; } = new ();
    public bool IsComplianceScheme { get; set; }
    public Organisation? Organisation { get; set; }
    public bool isUpdateCompanyName { get; set; }
    public string CompanyName { get; set; }
    public bool isUpdateCompanyAddress { get; set; }
}