using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Extensions
{
    public static class UpdateDetailsConfirmationViewModelExtension
    {
        public static string GetFormattedChangeMessage(this UpdateDetailsConfirmationViewModel model)
        {
            var timePart = model.UpdatedDatetime.ToString("hh:mmtt").ToLower();
            var datePart = $"{model.UpdatedDatetime:dd}{model.UpdatedDatetime.GetDateOrdinal()} {model.UpdatedDatetime:MMMM yyyy}";

            return $"Changed by {model.Username} at {timePart} on {datePart}";
        }
    }
}
