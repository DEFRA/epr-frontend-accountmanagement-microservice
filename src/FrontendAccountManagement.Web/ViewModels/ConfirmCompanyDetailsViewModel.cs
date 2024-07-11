using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Web.ViewModels
{
    public class ConfirmCompanyDetailsViewModel
    {
        public string CompanyName { get; set; }

        public string CompaniesHouseNumber { get; set; }

        public Address? BusinessAddress { get; set; }
    }
}
