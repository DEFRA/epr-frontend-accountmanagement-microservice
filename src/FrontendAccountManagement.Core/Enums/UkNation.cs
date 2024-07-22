using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.Constants.Enums;

public enum UkNation
{
    None = 0,
    [Display(Name = "England")]
    England,
    [Display(Name = "Scotland")]
    Scotland,
    [Display(Name = "Wales")]
    Wales,
    [Display(Name = "NorthernIreland")]
    NorthernIreland
}