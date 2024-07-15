using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class BaseConfirmationViewModel
    {
        public string Username { get; set; }
        public DateTime UpdatedDatetime { get; set; }
    }
}
