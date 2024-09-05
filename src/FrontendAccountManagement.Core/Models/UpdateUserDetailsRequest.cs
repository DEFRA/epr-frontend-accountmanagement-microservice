using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    /// <summary>
    /// User details that can be updated
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateUserDetailsRequest
    {
        /// <summary>
        /// User Firstname
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User Lastname
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User Jobtitle in an organisation
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// User Telephone
        /// </summary>
        public string? Telephone { get; set; }
    }
}
