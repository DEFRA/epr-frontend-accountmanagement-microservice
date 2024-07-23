using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement.Web.Constants.Enums;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

public class UkNationViewModel
{
    [Required]
    public UkNation? UkNation { get; set; }
}