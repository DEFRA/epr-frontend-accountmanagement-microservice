using FrontendAccountManagement.Web.Resources.Views.AccountManagement;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class EditCompanyRoleDetailsViewModel
    {
        [Required(ErrorMessageResourceName = "InvalidSelection", ErrorMessageResourceType = typeof(EditUserCompanyRoleDetails))]
        [RegularExpression("^(Director|Company Secretary|Partner|Member of a limited liability partnership)$", ErrorMessageResourceName = "InvalidSelection", ErrorMessageResourceType = typeof(EditUserCompanyRoleDetails))]
        public string SelectedCompaniesHouseRole { get ; set; }
    }
}
