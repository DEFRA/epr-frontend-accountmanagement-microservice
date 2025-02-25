using FrontendAccountManagement.Web.Constants.Enums;
using System.ComponentModel.DataAnnotations;
using ViewResources = FrontendAccountManagement.Web.Resources.Views;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class NonCompaniesHouseUkNationViewModel
    {
        [Required(ErrorMessage = "NonCompaniesHouseUkNation.ErrorMessage")]
        public UkNation? UkNation { get; set; }
    }
}
