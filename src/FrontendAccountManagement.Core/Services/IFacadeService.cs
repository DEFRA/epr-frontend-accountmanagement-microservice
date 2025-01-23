using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Core.Sessions;

namespace FrontendAccountManagement.Core.Services;

public interface IFacadeService
{
    Task<IEnumerable<Models.ServiceRole>?> GetAllServiceRolesAsync();

    Task<EndpointResponseStatus> SendUserInvite(InviteUserRequest request);

    Task<ConnectionPerson?> GetPersonDetailsFromConnectionAsync(Guid organisationId, Guid connectionId, string serviceKey);

    Task<(PermissionType? PermissionType, Guid? UserId)> GetPermissionTypeFromConnectionAsync(Guid organisationId, Guid connectionId, string serviceKey);

    Task<EnrolmentStatus?> GetEnrolmentStatus(Guid organisationId, Guid connectionId, string serviceKey, string serviceRoleKey);

    Task UpdatePersonRoleAdminOrEmployee(Guid connectionId, PersonRole personRole, Guid organisationId, string serviceKey);

    Task NominateToDelegatedPerson(Guid connectionId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest);

    Task<IEnumerable<ManageUserModel>?> GetUsersForOrganisationAsync(string organisationId, int serviceRoleId);

    Task<EndpointResponseStatus> RemoveUserForOrganisation(string personExternalId, string organisationId, int serviceRoleId);

    Task<UserAccountDto?> GetUserAccount();

    Task<List<int>> GetNationIds(Guid organisationId);

    Task<CompaniesHouseResponse> GetCompaniesHouseResponseAsync(string companyHouseNumber);

    /// <summary>
    /// Requests the facade to update the details of the organisation, including the
    /// nation id
    /// </summary>
    /// <returns>An async task</returns>
    Task UpdateOrganisationDetails(
        Guid organisationId,
        OrganisationUpdateDto organisation);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="organisationId"></param>
    /// <param name="serviceKey"></param>
    /// <param name="userDetailsUpdateModelRequest"></param>
    /// <returns></returns>
    Task<UpdateUserDetailsResponse> UpdateUserDetailsAsync(
        Guid userId,
        Guid organisationId,
        string serviceKey,
        UpdateUserDetailsRequest userDetailsUpdateModelRequest);

    Task<AddressList?> GetAddressListByPostcodeAsync(string postcode);
}
