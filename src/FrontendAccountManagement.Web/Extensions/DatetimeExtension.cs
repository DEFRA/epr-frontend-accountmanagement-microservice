namespace FrontendAccountManagement.Web.Extensions
{
    public static class DatetimeExtension
    {
        public static string GetDateOrdinal(this DateTime dateTime)
        {
            var day = dateTime.Day;
            if (day is > 10 and < 20) return "th";
            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        /// <summary>Converts a date from UTC time to UK time.</summary>
        /// <param name="utcTime">The time in UTC.</param>
        /// <returns>The time in UK time.</returns>
        public static DateTime ToUkTime(this DateTime utcTime)
        {
            TimeZoneInfo gmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, gmtTimeZone);
        }
    }
}
