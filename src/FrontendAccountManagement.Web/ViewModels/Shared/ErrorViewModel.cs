using FrontendAccountManagement.Web.Extensions;

namespace FrontendAccountManagement.Web.ViewModels.Shared;

public class ErrorViewModel
{
    public string Timestamp { get; set; } = DateTime.UtcNow.UtcToGmt().ToString("dd/MM/yyyy HH:mm:ss");
}