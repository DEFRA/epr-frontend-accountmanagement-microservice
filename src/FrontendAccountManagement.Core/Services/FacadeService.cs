using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Configuration;
using FrontendAccountManagement.Core.Constants;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Core.Services.Dto.Address;
using FrontendAccountManagement.Core.Sessions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Net;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace FrontendAccountManagement.Core.Services;

public class FacadeService : IFacadeService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string _baseAddress;
    private readonly string _serviceRolesPath;
    private readonly string _getUserAccountPath;
    private readonly string _getCompanyFromCompaniesHousePath;

    private readonly string _putUserDetailsByUserIdPath;
    private readonly string _putUpdateOrganisationPath;
    private readonly string[] _scopes;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public FacadeService(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IOptions<FacadeApiConfiguration> options)
    {
        var config = options.Value;

        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _baseAddress = config.Address;
        _serviceRolesPath = config.GetServiceRolesPath;
        _getUserAccountPath = config.GetUserAccountPath;
        _getCompanyFromCompaniesHousePath = config.GetCompanyFromCompaniesHousePath;

        _putUserDetailsByUserIdPath = config.PutUserDetailsByUserIdPath;
        _putUpdateOrganisationPath = config.PutUpdateOrganisationPath;
        _scopes = new[]
        {
            config.DownStreamScope,
        };
        _jsonSerializerOptions = new JsonSerializerOptions();
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<UserAccountDto?> GetUserAccount()
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync(_getUserAccountPath);

        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var userAccountDto = await response.Content.ReadFromJsonAsync<UserAccountDto>();

        return userAccountDto;
    }

    public async Task<IEnumerable<Models.ServiceRole>?> GetAllServiceRolesAsync()
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync(_serviceRolesPath);

        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<IEnumerable<Models.ServiceRole>>();

        return roles;
    }

    public async Task<EndpointResponseStatus> SendUserInvite(InviteUserRequest request)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.PostAsJsonAsync("accounts-management/invite-user", request);

        if (response.IsSuccessStatusCode)
        {
            return EndpointResponseStatus.Success;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NoContent)
        {
            return EndpointResponseStatus.UserExists;
        }

        return EndpointResponseStatus.Fail;
    }

    public async Task<ConnectionPerson?> GetPersonDetailsFromConnectionAsync(Guid organisationId, Guid connectionId, string serviceKey)
    {
        await PrepareAuthenticatedClient();

        AddHttpClientHeader("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.GetAsync($"connections/{connectionId}/person?serviceKey={serviceKey}");

        response.EnsureSuccessStatusCode();

        var person = await response.Content.ReadFromJsonAsync<ConnectionPerson>();

        return person;
    }

    public async Task<(PermissionType? PermissionType, Guid? UserId)> GetPermissionTypeFromConnectionAsync(Guid organisationId, Guid connectionId, string serviceKey)
    {
        await PrepareAuthenticatedClient();

        AddHttpClientHeader("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.GetAsync($"connections/{connectionId}/roles?serviceKey={serviceKey}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (null, null);
        }

        response.EnsureSuccessStatusCode();

        var connectionWithEnrolments = await response.Content.ReadFromJsonWithEnumsAsync<ConnectionWithEnrolments>();

        if (connectionWithEnrolments == null ||
            connectionWithEnrolments.Enrolments.Count == 0 ||
            connectionWithEnrolments.Enrolments.Any(e => e.EnrolmentStatus == EnrolmentStatus.Invited))
        {
            return (null, null);
        }

        var currentPermissionType = PermissionType.NotSet;

        if (connectionWithEnrolments.Enrolments.Any(a => a.ServiceRoleKey == Constants.ServiceRoles.Packaging.ApprovedPerson))
        {
            currentPermissionType = PermissionType.Approved;
        }
        else if (connectionWithEnrolments.Enrolments.Any(a => a.ServiceRoleKey == Constants.ServiceRoles.Packaging.DelegatedPerson))
        {
            currentPermissionType = PermissionType.Delegated;
        }
        else
        {
            currentPermissionType = connectionWithEnrolments.PersonRole == PersonRole.Admin ? PermissionType.Admin : PermissionType.Basic;
        }

        return (currentPermissionType, connectionWithEnrolments.UserId);
    }

    public async Task<EnrolmentStatus?> GetEnrolmentStatus(Guid organisationId, Guid connectionId, string serviceKey, string serviceRoleKey)
    {
        await PrepareAuthenticatedClient();

        AddHttpClientHeader("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.GetAsync($"connections/{connectionId}/roles?serviceKey={serviceKey}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var connectionWithEnrolments = await response.Content.ReadFromJsonWithEnumsAsync<ConnectionWithEnrolments>();

        return connectionWithEnrolments?.Enrolments.FirstOrDefault(e => e.ServiceRoleKey.Equals(serviceRoleKey))?.EnrolmentStatus;
    }

    public async Task UpdatePersonRoleAdminOrEmployee(Guid connectionId, PersonRole personRole, Guid organisationId, string serviceKey)
    {
        await PrepareAuthenticatedClient();

        var updatePersonRoleRequest = new UpdatePersonRoleRequest { PersonRole = personRole };

        
        

        var uri = new Uri($"{_baseAddress}connections/{connectionId}/roles?personRole={personRole}&serviceKey={serviceKey}");

        var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(updatePersonRoleRequest, _jsonSerializerOptions), Encoding.UTF8, "application/json"),
        };

        request.Headers.Add("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task NominateToDelegatedPerson(Guid connectionId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest)
    {
        await PrepareAuthenticatedClient();

        var uri = new Uri($"{_baseAddress}connections/{connectionId}/delegated-person-nomination?serviceKey={serviceKey}");

        var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(nominationRequest, _jsonSerializerOptions), Encoding.UTF8, "application/json"),
        };

        request.Headers.Add("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<ManageUserModel>?> GetUsersForOrganisationAsync(string organisationId, int serviceRoleId)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"organisations/users?organisationId={organisationId}&serviceRoleId={serviceRoleId}");

        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonWithEnumsAsync<IEnumerable<ManageUserModel>>();

        return roles;
    }

    public async Task<EndpointResponseStatus> RemoveUserForOrganisation(
        string personExternalId,
        string organisationId,
        int serviceRoleId)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.DeleteAsync($"enrolments/{personExternalId}?organisationId={organisationId}&serviceRoleId={serviceRoleId}");

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<List<int>> GetNationIds(Guid organisationId)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"organisations/organisation-nation?organisationId={organisationId}");

        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<int>>() : new List<int> { 0 };
    }

    public async Task<Guid?> GetUserIdForPerson(Guid personExternalId)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"user-accounts/user-by-person-id?personId={personExternalId}");

        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Guid?>() : null;
    }

    public async Task<CompaniesHouseResponse> GetCompaniesHouseResponseAsync(string companyHouseNumber)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"{_getCompanyFromCompaniesHousePath}?id={companyHouseNumber}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var companiesHouseData = await response.Content.ReadFromJsonAsync<CompaniesHouseResponse>();

        return companiesHouseData;

    }

    /// <summary>
    /// Requests the facade to update the nation id for a
    /// given organisation id
    /// </summary>
    /// <param name="organisationId">The organisation id to update</param>
    /// <param name="nationId">The nation id to use</param>
    /// <returns>An async task</returns>
    public async Task UpdateOrganisationDetails(
        Guid organisationId,
        OrganisationUpdateDto organisation)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.PutAsJsonAsync($"{_putUpdateOrganisationPath}/{organisationId}", organisation);

        await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Update User Details
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="organisationId"></param>
    /// <param name="serviceKey"></param>
    /// <param name="userDetailsUpdateModelRequest"></param>
    /// <returns></returns>
    public async Task<UpdateUserDetailsResponse> UpdateUserDetailsAsync(
        Guid userId,
        Guid organisationId,
        string serviceKey,
        UpdateUserDetailsRequest userDetailsUpdateModelRequest)
    {
        await PrepareAuthenticatedClient();
        var uri = new Uri($"{_baseAddress}{_putUserDetailsByUserIdPath}?serviceKey={serviceKey}");
        var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(userDetailsUpdateModelRequest), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-EPR-Organisation", organisationId.ToString());
        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<UpdateUserDetailsResponse>();
        return responseData;
    }

    public async Task<AddressList?> GetAddressListByPostcodeAsync(string postcode)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"/api/address-lookup?postcode={postcode}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var addressResponse = await response.Content.ReadFromJsonAsync<AddressLookupResponse>();

        return new AddressList(addressResponse);
    }

    private async Task PrepareAuthenticatedClient()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_baseAddress);
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Microsoft.Identity.Web.Constants.Bearer, accessToken);
        }
    }

    private void AddHttpClientHeader(string key, string value)
    {
        if (_httpClient.DefaultRequestHeaders.Contains(key))
        {
            _httpClient.DefaultRequestHeaders.Remove(key);
        }
        _httpClient.DefaultRequestHeaders.Add(key, value);
    }
}