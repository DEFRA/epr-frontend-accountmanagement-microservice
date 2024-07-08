using FrontendAccountManagement.Web.Extensions;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class UpdateDetailsConfirmationViewModel
    {
        public string Username { get; set; }
        public DateTime UpdatedDatetime { get; set; }

        public string GetFormattedChangeMessage()
        {
            var timePart = UpdatedDatetime.ToString("hh:mmtt").ToLower();
            var datePart = $"{UpdatedDatetime:dd}{UpdatedDatetime.GetDateOrdinal()} {UpdatedDatetime:MMMM yyyy}";

            return $"Changed by {Username} at {timePart} on {datePart}";
        }
    }
}
