using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.MockedData;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;

namespace FrontendAccountManagement.Core.Services;

public class MockedFacadeService : IFacadeService
{
    public static async Task<string> GetTestMessageAsync()
    {
        return await Task.FromResult("Dummy test message from MockedFacadeService");
    }

    public Task<IEnumerable<Models.ServiceRole>?> GetAllServiceRolesAsync()
    {
        var serviceRoles = new List<Models.ServiceRole>
        {

        };

        var result = serviceRoles as IEnumerable<Models.ServiceRole>;
        return Task.FromResult(result);
    }

    public Task<EndpointResponseStatus> SendUserInvite(InviteUserRequest request)
    {
        return Task.FromResult(EndpointResponseStatus.Success);
    }

    public Task<ConnectionPerson?> GetPersonDetailsFromConnectionAsync(Guid organisationId, Guid connectionId, string serviceKey)
    {
        return Task.FromResult<ConnectionPerson?>(new ConnectionPerson { FirstName = "Dummy", LastName = "User" });
    }

    public async Task<(PermissionType? PermissionType, Guid? UserId)> GetPermissionTypeFromConnectionAsync(Guid organisationId, Guid connectionId, string serviceKey)
    {
        PermissionType currentUserAccess;

        switch (connectionId.ToString())
        {
            case "00000000-0000-0000-0000-000000000001":
                {
                    currentUserAccess = PermissionType.Delegated;
                    break;
                }
            case "00000000-0000-0000-0000-000000000002":
                {
                    currentUserAccess = PermissionType.Admin;
                    break;
                }
            default:
                {
                    currentUserAccess = PermissionType.Basic;
                    break;
                }
        }
        return await Task.FromResult((currentUserAccess, Guid.NewGuid()));
    }

    public Task UpdatePersonRoleAdminOrEmployee(Guid connectionId, Enums.PersonRole personRole, Guid organisationId, string serviceKey)
    {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ManageUserModel>?> GetUsersForOrganisationAsync(string organisationId, int serviceRoleId)
    {
        var users = MockedManageUserModels.GetMockedManageUserModels();
        return Task.FromResult(users);
    }

    public Task<EndpointResponseStatus> RemoveUserForOrganisation(string personExternalId, string organisationId, int serviceRoleId)
    {
        return Task.FromResult(EndpointResponseStatus.Success);
    }

    public Task<UserAccountDto?> GetUserAccount()
    {
        return Task.FromResult<UserAccountDto?>(null);
    }

    public Task<EnrolmentStatus?> GetEnrolmentStatus(Guid organisationId, Guid connectionId, string serviceKey, string serviceRoleKey)
    {
        throw new NotImplementedException();
    }
        
    public Task NominateToDelegatedPerson(Guid connectionId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest)
    {
        return Task.CompletedTask;
    }

    public Task<List<int>> GetNationIds(Guid organisationId)
    {
        return Task.FromResult(new List<int>{1,2});
    }
}
