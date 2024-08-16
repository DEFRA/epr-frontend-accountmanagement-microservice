using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Extensions
{
    public static class BaseConfirmationViewModelExtension
    {
        public static string GetFormattedChangeMessage(this BaseConfirmationViewModel model, string action)
        {
            var locatDateTime = model.UpdatedDatetime.UtcToGmt();

            var timePart = locatDateTime.ToString("hh:mmtt").ToLower();

            var datePart = $"{locatDateTime:dd}{locatDateTime.GetDateOrdinal()} {locatDateTime:MMMM yyyy}";

            return $"{action} by {model.Username} at {timePart} on {datePart}";
        }
    }
}
