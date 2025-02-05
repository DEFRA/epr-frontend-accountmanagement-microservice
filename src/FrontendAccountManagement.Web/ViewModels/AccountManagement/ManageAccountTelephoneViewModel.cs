using FrontendAccountManagement.Web.ViewModels.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

public class ManageAccountTelephoneViewModel
{
    public string OriginalPhoneNumber { get; set; }
     

    [TelephoneNumberValidation()]
    public string NewPhoneNumber { get; set; } 
}
