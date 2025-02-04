using FrontendAccountManagement.Web.Resources.Views.AccountManagement;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class ApprovedPersonNameChangeViewModel
    {
        [Required(ErrorMessageResourceName = "FirstNameMissing", ErrorMessageResourceType = typeof(ApprovedPersonNameChange))]
        [StringLength(50, ErrorMessageResourceName = "FirstNameMaxLength", ErrorMessageResourceType = typeof(ApprovedPersonNameChange))]
        public string FirstName { get; set; }
        [Required(ErrorMessageResourceName = "LastNameMissing", ErrorMessageResourceType = typeof(ApprovedPersonNameChange))]
        [StringLength(50, ErrorMessageResourceName = "LastNameMaxLength", ErrorMessageResourceType = typeof(ApprovedPersonNameChange))]
        public string LastName { get; set; }
    }
}
