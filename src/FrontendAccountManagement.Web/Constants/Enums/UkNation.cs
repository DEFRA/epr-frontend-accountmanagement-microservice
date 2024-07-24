using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement;
using FrontendAccountManagement.Web.Resources.Enums;

namespace FrontendAccountManagement.Web.Constants.Enums;

public enum UkNation
{
    None = 0,
    [Display(Name = "England", ResourceType = typeof(UkNationResource))]
    England,
    [Display(Name = "Scotland", ResourceType = typeof(UkNationResource))]
    Scotland,
    [Display(Name = "Wales", ResourceType = typeof(UkNationResource))]
    Wales,
    [Display(Name = "NorthernIreland", ResourceType = typeof(UkNationResource))]
    NorthernIreland
}