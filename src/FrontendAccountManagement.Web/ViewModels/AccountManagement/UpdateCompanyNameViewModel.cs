using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class UpdateCompanyNameViewModel
    {
        [Required(ErrorMessage = "UpdateCompanyNameErrorMessage")]
        public YesNoAnswer? isUpdateCompanyName { get; set; }
    }
}
