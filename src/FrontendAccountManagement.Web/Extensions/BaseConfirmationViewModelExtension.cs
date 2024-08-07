using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Extensions
{
    public static class BaseConfirmationViewModelExtension
    {
        public static string GetFormattedChangeMessage(this BaseConfirmationViewModel model, string action)
        {
            var timePart = model.UpdatedDatetime.ToString("hh:mmtt").ToLower();
            var datePart = $"{model.UpdatedDatetime:dd}{model.UpdatedDatetime.GetDateOrdinal()} {model.UpdatedDatetime:MMMM yyyy}";

            return $"{action} by {model.Username} at {timePart} on {datePart}";
        }
    }
}
