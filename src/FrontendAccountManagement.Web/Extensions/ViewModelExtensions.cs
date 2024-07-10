using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Extensions
{
    public static class ViewModelExtensions
    {
        public static bool PropertyExists(
            this EditUserDetailsViewModel editUserDetailsViewModel,
            Func<EditUserDetailsViewModel, string> propertySelector)
        {
            var propertyValue = propertySelector(editUserDetailsViewModel);
            return !string.IsNullOrWhiteSpace(propertyValue);
        }
    }
}
