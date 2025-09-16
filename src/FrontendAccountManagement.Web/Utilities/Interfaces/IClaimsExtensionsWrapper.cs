using EPR.Common.Authorization.Models;
using FrontendAccountManagement;
using FrontendAccountManagement.Web;
using FrontendAccountManagement.Web.Utilities;

namespace FrontendAccountManagement.Web.Utilities.Interfaces;

public interface IClaimsExtensionsWrapper
{
    Task UpdateUserDataClaimsAndSignInAsync(UserData userData);

    Task<string> TryGetOrganisatonIds();
}
