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

        public static string ToReadableDate(this DateTime dateTime) => dateTime.UtcToGmt().ToString("d MMMM yyyy");

        public static string ToReadableDateTime(this DateTime dateTime) => dateTime.UtcToGmt().ToString("d MMMM yyyy, hh:mm") + dateTime.UtcToGmt().ToString("tt").ToLower();

        public static string ToShortReadableDate(this DateTime dateTime) => dateTime.UtcToGmt().ToString("d MMM yyyy");

        public static string ToShortReadableWithShortYearDate(this DateTime dateTime) => dateTime.UtcToGmt().ToString("d MMM yy");

        public static string ToTimeHoursMinutes(this DateTime dateTime) => dateTime.UtcToGmt().ToString("h:mmtt").ToLower();

        public static DateTime UtcToGmt(this DateTime dateTime) => TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/London"));
    }
}
