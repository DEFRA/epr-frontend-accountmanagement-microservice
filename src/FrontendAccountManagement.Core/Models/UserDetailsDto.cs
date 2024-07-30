using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    /// <summary>
    /// User details that can be updated
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UserDetailsDto
    {
        /// <summary>
        /// User Firstname
        /// </summary>
        public string FirstName { get; init; }

        /// <summary>
        /// User Lastname
        /// </summary>
        public string LastName { get; init; }

        /// <summary>
        /// User Jobtitle in an organisation
        /// </summary>
        public string JobTitle { get; init; }

        /// <summary>
        /// User Telephone
        /// </summary>
        public string TelePhone { get; init; }
    }
}
