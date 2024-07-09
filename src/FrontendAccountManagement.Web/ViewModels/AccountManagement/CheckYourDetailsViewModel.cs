using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class CheckYourDetailsViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }    
        public string JobTitle { get; set; }
        public string PhoneNumber { get; set; }
    }
}
