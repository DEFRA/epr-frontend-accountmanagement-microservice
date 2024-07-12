using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Core.Constants;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using FrontendAccountManagement.Core.Models.CompaniesHouse;

namespace FrontendAccountManagement.Core.Services;

public class FacadeService : IFacadeService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string _baseAddress;
    private readonly string _serviceRolesPath;
    private readonly string _getUserAccountPath;
    private readonly string[] _scopes;

    public FacadeService(HttpClient httpClient, ITokenAcquisition tokenAcquisition, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _baseAddress = configuration["FacadeAPI:Address"];
        _serviceRolesPath = configuration["FacadeAPI:GetServiceRolesPath"];
        _getUserAccountPath = configuration["FacadeAPI:GetUserAccountPath"];
        _scopes = new[]
        {
            configuration["FacadeAPI:DownStreamScope"],
        };
    }

    public async Task<UserAccountDto?> GetUserAccount()
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync(_getUserAccountPath);

        if (response.StatusCode == HttpStatusCode.NotFound)
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

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return EndpointResponseStatus.UserExists;
        }

        throw new Exception(response.Content.ToString());
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
            !connectionWithEnrolments.Enrolments.Any() ||
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

        return connectionWithEnrolments?.Enrolments.FirstOrDefault(e => e.ServiceRoleKey.Equals(serviceRoleKey)).EnrolmentStatus;
    }

    public async Task UpdatePersonRoleAdminOrEmployee(Guid connectionId, PersonRole personRole, Guid organisationId, string serviceKey)
    {
        await PrepareAuthenticatedClient();

        var updatePersonRoleRequest = new UpdatePersonRoleRequest { PersonRole = personRole };

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        var uri = new Uri($"{_baseAddress}connections/{connectionId}/roles?personRole={personRole}&serviceKey={serviceKey}");

        var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(updatePersonRoleRequest, jsonSerializerOptions), Encoding.UTF8, "application/json"),
        };

        request.Headers.Add("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task NominateToDelegatedPerson(Guid connectionId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest)
    {
        await PrepareAuthenticatedClient();

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        var uri = new Uri($"{_baseAddress}connections/{connectionId}/delegated-person-nomination?serviceKey={serviceKey}");

        var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(nominationRequest, jsonSerializerOptions), Encoding.UTF8, "application/json"),
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

        response.EnsureSuccessStatusCode();

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<List<int>> GetNationIds(Guid organisationId)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"organisations/organisation-nation?organisationId={organisationId}");

        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<int>>() : new List<int> { 0 };
    }

    public async Task<CompaniesHouseResponse> GetCompaniesHouseResponseAsync(string companyHouseNumber)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"/api/companies-house?id={companyHouseNumber}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var companiesHouseData = await response.Content.ReadFromJsonAsync<CompaniesHouseResponse>();

        return companiesHouseData;

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