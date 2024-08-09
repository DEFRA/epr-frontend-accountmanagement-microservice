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
    /// Request the facade to update the user details for a give user ID
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="userDetailsDto">The details to update</param>
    /// <returns>An async task</returns>
    Task UpdateUserDetails(Guid? userId, UserDetailsUpdateModel userDetailsDto);

    Task<UpdateUserDetailsResponse> UpdatePersonalDetailsAsync(Guid userId, Guid organisationId, string serviceKey, UserDetailsUpdateModel userDetailsUpdateModelRequest);


}
