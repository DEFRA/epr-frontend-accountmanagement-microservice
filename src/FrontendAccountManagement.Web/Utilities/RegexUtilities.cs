using System.Text.RegularExpressions;

namespace FrontendAccountManagement.Web.UnitTests.Utilities;

public static class RegexUtilities
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\.\s]{2,}$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}