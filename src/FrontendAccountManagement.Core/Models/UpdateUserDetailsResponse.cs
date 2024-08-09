using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    /// <summary>
    /// User details updated response
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateUserDetailsResponse
    {
        public bool HasTelephoneOnlyUpdated { get; set; } = false;
        public bool HasBasicUserDetailsUpdated { get; set; } = false;
        public bool HasApprovedOrDelegatedUserDetailsSentForApproval { get; set; } = false;
    }
}
