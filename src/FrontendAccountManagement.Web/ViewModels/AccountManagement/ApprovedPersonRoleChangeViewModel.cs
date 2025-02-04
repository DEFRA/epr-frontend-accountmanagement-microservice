using FrontendAccountManagement.Web.Resources.Views.AccountManagement;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class ApprovedPersonRoleChangeViewModel
    {
        [Required(ErrorMessageResourceName = "InvalidSelection", ErrorMessageResourceType = typeof(ApprovedPersonRoleChange))]
        [RegularExpression("^(Director|Company Secretary|Partner|Member of a limited liability partnership)$", ErrorMessageResourceName = "InvalidSelection", ErrorMessageResourceType = typeof(ApprovedPersonRoleChange))]
        public string SelectedCompaniesHouseRole { get ; set; }
    }
}
