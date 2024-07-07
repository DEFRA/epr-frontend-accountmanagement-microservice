namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class UpdateDetailsConfirmationViewModel
    {
        public string Username { get; set; }
        public DateTime UpdatedDatetime { get; set; }

        public string GetFormattedChangeMessage()
        {
            var timePart = UpdatedDatetime.ToString("hh:mmtt").ToLower();
            var datePart = $"{UpdatedDatetime:dd}{GetOrdinal(UpdatedDatetime.Day)} {UpdatedDatetime:MMMM yyyy}";

            return $"Changed by {Username} at {timePart} on {datePart}";
        }

        private string GetOrdinal(int day)
        {
            if (day is > 10 and < 20) return "th";
            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }
    }
}
