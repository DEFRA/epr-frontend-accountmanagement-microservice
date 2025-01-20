using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class UpdateCompanyAddressViewModel
    {
        [Required(ErrorMessage = "Select an option")]
        public YesNoAnswer? isUpdateCompanyAddress { get; set; }
    }
}
