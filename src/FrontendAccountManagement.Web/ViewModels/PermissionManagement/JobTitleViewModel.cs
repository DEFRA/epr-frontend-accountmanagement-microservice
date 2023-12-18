using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.PermissionManagement;

public class JobTitleViewModel
{
    public Guid Id { get; set; }

    [MaxLength(450, ErrorMessage = "JobTitle.LengthErrorMessage")]
    [Required(ErrorMessage = "JobTitle.RequiredErrorMessage")]
    public string JobTitle { get; set; } = default!;
}
