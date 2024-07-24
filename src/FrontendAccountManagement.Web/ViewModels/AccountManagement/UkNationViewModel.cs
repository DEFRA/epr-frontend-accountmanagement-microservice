using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement.Web.Constants.Enums;
using ViewResources = FrontendAccountManagement.Web.Resources.Views;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

public class UkNationViewModel
{
    [Required(ErrorMessageResourceName = "UkNation_ErrorMessage", ErrorMessageResourceType = typeof(ViewResources.AccountManagement.UkNation))]
    public UkNation? UkNation { get; set; }
}