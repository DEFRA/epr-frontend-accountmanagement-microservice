
    using System.ComponentModel.DataAnnotations;

    namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
    {
        public class UpdateCompanyAddressViewModel
        {
            [Required(ErrorMessage = "UpdateCompanyAddressErrorMessage")]
            public YesNoAnswer? isUpdateCompanyAddress { get; set; }
        }
    }

