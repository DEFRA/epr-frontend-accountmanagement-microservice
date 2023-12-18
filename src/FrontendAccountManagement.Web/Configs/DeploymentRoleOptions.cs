namespace FrontendAccountManagement.Web.Configs;

public class DeploymentRoleOptions
{
    public const string ConfigSection = "DeploymentRole";

    public const string RegulatorRoleValue = "Regulator";
    public string? DeploymentRole { get; set; }

    public bool IsRegulator()
    {
        return DeploymentRole != null && DeploymentRole.Equals(RegulatorRoleValue, StringComparison.OrdinalIgnoreCase);
    }
}