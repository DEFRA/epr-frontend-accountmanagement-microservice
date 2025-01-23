using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class BusinessAddressPostcodeViewModel
    {
        [Required(ErrorMessage = "BusinessAddressPostcode.EnterPostcodeError")]
        [RegularExpression("^([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$",
            ErrorMessage = "BusinessAddressPostcode.EnterPostcodeError")]
        public string? Postcode { get; set; }
    }
}
