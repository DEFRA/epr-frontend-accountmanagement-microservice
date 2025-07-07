using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.MockedData;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
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

	public Task<UserAccountDto?> GetUserAccountWithEnrolments(string serviceKey)
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
        return Task.FromResult(new List<int> { 1, 2 });
    }

    public Task<CompaniesHouseResponse> GetCompaniesHouseResponseAsync(string companyHouseNumber)
    {
        var stubResponse = new CompaniesHouseResponse
        {
            AccountCreatedOn = DateTimeOffset.UtcNow.AddDays(-15),
            Organisation = new OrganisationDto
            {
                Name = "Stub company name",
                RegistrationNumber = "AB122345",
                RegisteredOffice = new AddressDto
                {
                    Street = "Test street",
                    BuildingName = "Test Building name",
                    BuildingNumber = "11",
                    Country = new CountryDto
                    {
                        Iso = "123",
                        Name = "United Kingdom"
                    },
                    Town = "London",
                    SubBuildingName = "test stub building name",
                    Postcode = "wh1c 2wd"
                },
                OrganisationData = new OrganisationDataDto
                {
                    DateOfCreation = DateTime.UtcNow.AddMonths(-1),
                    Status = "mock company",
                    Type = "limited"
                }
            }
        };

        return Task.FromResult(stubResponse);
    }

    public async Task UpdateOrganisationDetails(Guid organisationId, OrganisationUpdateDto organisation)
    {
        await Task.CompletedTask;
    }

    public async Task UpdateUserDetails(Guid? userId, UpdateUserDetailsRequest userDetailsDto)
    {
        await Task.CompletedTask;
    }

    public async Task<UpdateUserDetailsResponse> UpdateUserDetailsAsync(Guid userId, Guid organisationId, string serviceKey, UpdateUserDetailsRequest userDetailsUpdateModelRequest)
    {
        var stubResponse = new UpdateUserDetailsResponse
        {
            HasApprovedOrDelegatedUserDetailsSentForApproval = false,
            HasBasicUserDetailsUpdated = true,
            HasTelephoneOnlyUpdated = false
        };
        return await Task.FromResult(stubResponse);
    }

    public Task<AddressList?> GetAddressListByPostcodeAsync(string postcode)
    {
        throw new NotImplementedException();  // PAUL TO DO
    }

    public async Task<PersonDetailsDto?> GetUserDetailsByIdAsync(Guid userId)
    {
        var stubResponse = new PersonDetailsDto
        {
            ContactEmail = "test@test.com"
        };
        return await Task.FromResult(stubResponse);
    }
}
