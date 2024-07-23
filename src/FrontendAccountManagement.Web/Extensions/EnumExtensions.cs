using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.Extensions
{
    /// <summary>
    /// Static class that contains extension methods for enums
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Displays the text represented in the Display attribute of the enum
        /// </summary>
        /// <param name="enumValue">the value of the enum that we want the resource text for</param>
        /// <returns>A resource accessed string for the relevant enum value</returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            var enumType = enumValue.GetType();
            var memberInfo = enumType.GetMember(enumValue.ToString());

            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DisplayAttribute)attrs[0]).GetName();
                }
            }

            return enumValue.ToString();
        }
    }
}
