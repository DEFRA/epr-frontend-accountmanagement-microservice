using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement;
using FrontendAccountManagement.Web.Resources.Enums;

namespace FrontendAccountManagement.Web.Constants.Enums;

public enum UkNation
{
    None = 0,

    [Display(Name = "England", ResourceType = typeof(UkNationResources))]
    England,

    [Display(Name = "Scotland", ResourceType = typeof(UkNationResources))]
    Scotland,

    [Display(Name = "Wales", ResourceType = typeof(UkNationResources))]
    Wales,

    [Display(Name = "NorthernIreland", ResourceType = typeof(UkNationResources))]
    NorthernIreland
}
