using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{

    public class BusinessAddressViewModel
    {
        [MaxLength(100, ErrorMessage = "BusinessAddress.SubBuildingNameLengthError")]
        public string? SubBuildingName { get; set; }

        [MaxLength(100, ErrorMessage = "BusinessAddress.BuildingNameLengthError")]
        public string? BuildingName { get; set; }

        [MaxLength(50, ErrorMessage = "BusinessAddress.BuildingNumberLengthError")]
        [Required(ErrorMessage = "BusinessAddress.BuildingNumberError")]
        public string? BuildingNumber { get; set; }

        [MaxLength(100, ErrorMessage = "BusinessAddress.StreetNameLengthError")]
        [Required(ErrorMessage = "BusinessAddress.StreetNameError")]
        public string? Street { get; set; }

        [MaxLength(70, ErrorMessage = "BusinessAddress.TownLengthError")]
        [Required(ErrorMessage = "BusinessAddress.TownError")]
        public string? Town { get; set; }

        [MaxLength(50, ErrorMessage = "BusinessAddress.CountyLengthError")]
        public string? County { get; set; }

        [Required(ErrorMessage = "BusinessAddress.PostcodeError")]
        [RegularExpression("^([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$",
            ErrorMessage = "BusinessAddress.PostcodeError")]
        public string? Postcode { get; set; }

        public bool ShowWarning { get; set; }
    }
}