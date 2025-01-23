using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class UpdateCompanyNameViewModel
    {
        [Required(ErrorMessage = "UpdateCompanyName.SelectionError")]
        public YesNoAnswer? IsUpdateCompanyName { get; set; }
    }
}
