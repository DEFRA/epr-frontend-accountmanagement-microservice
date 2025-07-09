using FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.ReExAccountManagement;

[ExcludeFromCodeCoverage]
public class ViewDetailsViewModel
{
    public EndpointResponseStatus? UserRemovedStatus { get; set; }

    public string? Email { get; set; }

    public string? AccountRole { get; set; }

    public string? AddedBy { get; set; }

    public string? AccountPermissions { get; set; }
}