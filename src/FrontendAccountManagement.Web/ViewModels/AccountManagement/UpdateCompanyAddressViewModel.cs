using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class UpdateCompanyAddressViewModel
    {
        [Required(ErrorMessage = "UpdateCompanyAddress.SelectionError")]
        public YesNoAnswer? IsUpdateCompanyAddress { get; set; }
    }
}
