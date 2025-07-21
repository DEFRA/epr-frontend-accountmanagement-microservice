using FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.ReExAccountManagement;

[ExcludeFromCodeCoverage]
public class ViewDetailsViewModel
{
	public EndpointResponseStatus? UserRemovedStatus { get; set; }

	public Guid OrganisationId { get; set; }

	public Guid PersonId { get; set; }

	public int EnrolmentId { get; set; }

	public string? Email { get; set; }

	public string? AccountRole { get; set; }

	public string? AddedBy { get; set; }

	public bool CanRemoveUser { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }
}