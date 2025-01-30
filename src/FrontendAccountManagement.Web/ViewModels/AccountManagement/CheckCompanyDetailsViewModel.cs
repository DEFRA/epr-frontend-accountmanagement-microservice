
namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class CheckCompanyDetailsViewModel
    {
        public bool ShowWarning { get; set; }
        public string Name { get; internal set; }
        public string Address { get; internal set; }
        public string UKNation { get; internal set; }
        public bool IsUpdateCompanyName { get; internal set; }
        public bool IsUpdateCompanyAddress { get; internal set; }
    }
}