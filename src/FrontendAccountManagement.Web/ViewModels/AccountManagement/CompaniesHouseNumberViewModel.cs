using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class CompaniesHouseNumberViewModel
    {
        [Required(ErrorMessage = "CompaniesHouseNumber.ErrorMessage")]
        [MaxLength(8, ErrorMessage = "CompaniesHouseNumber.LengthErrorMessage")]
        public string? CompaniesHouseNumber { get; set; }
    }
}
