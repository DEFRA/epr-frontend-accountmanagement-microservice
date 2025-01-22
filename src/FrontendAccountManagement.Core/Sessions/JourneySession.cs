namespace FrontendAccountManagement.Core.Sessions;

public class JourneySession 
{
    public AccountManagementSession AccountManagementSession { get; set; } = new();
    public PermissionManagementSession PermissionManagementSession { get; set; } = new();
    public EditCompanyDetailsSession EditCompanyDetailsSession { get; set; } = new();
    public bool IsComplianceScheme { get; set; }
}