using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class CompanyNameViewModel
    {
        [MaxLength(170, ErrorMessage = "CompanyName.LengthErrorMessage")]
        [Required(ErrorMessage = "CompanyName.ErrorMessage")]
        public string? CompanyName { get; set; }
    }
}
