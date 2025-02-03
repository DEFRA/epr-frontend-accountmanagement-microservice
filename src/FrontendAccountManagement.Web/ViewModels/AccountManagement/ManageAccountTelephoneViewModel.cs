using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement;

public class ManageAccountTelephoneViewModel
{
    public string OriginalPhoneNumber { get; set; }

    //[Required(ErrorMessageResourceName = "TelehpneNumberMissing", ErrorMessageResourceType = typeof(TelehpneNumberChange))]
    public string NewPhoneNumber { get; set; }

    public bool IsPhoneChangeOnly { get; set; } = false;
}
