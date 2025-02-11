using FrontendAccountManagement.Web.ViewModels.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

public class ManageAccountTelephoneViewModel
{

    [TelephoneNumberValidation()]
    public string? Telephone { get; set; }
}
