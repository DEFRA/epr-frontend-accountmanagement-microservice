using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Services;
using EPR.Common.Authorization.Services.Interfaces;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.FeatureManagement;
using System.Security.Claims;

namespace FrontendAccountManagement.Web.Middleware;

public class UserDataCheckerMiddleware : IMiddleware
{
    private readonly IFacadeService _facadeService;
    private readonly IClaimsExtensionsWrapper _claimsExtensionsWrapper;
    private readonly IFeatureManager _featureManager;
    private readonly IGraphService _graphService;
    private readonly ILogger<UserDataCheckerMiddleware> _logger;

    public UserDataCheckerMiddleware(
        IFacadeService facadeService,
        IClaimsExtensionsWrapper claimsExtensionsWrapper,
        IFeatureManager featureManager,
        IGraphService graphService,
        ILogger<UserDataCheckerMiddleware> logger)
    {
        _facadeService = facadeService;
        _claimsExtensionsWrapper = claimsExtensionsWrapper;
        _featureManager = featureManager;
        _graphService = graphService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var anonControllers = new List<string> { "Privacy", "Cookies", "Culture" };
        var controllerName = GetControllerName(context);
        
        var existingUserData = context.User.GetUserData();

        if (!anonControllers.Contains(controllerName) && context.User.Identity is { IsAuthenticated: true } && existingUserData is null)
        {
            var userAccount = await _facadeService.GetUserAccount();

            if (userAccount is null)
            {
                _logger.LogInformation("User authenticated but account could not be found");
            }
            else
            {
                await UpdateOrganisationIdsClaim(userAccount.User);
                await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);
            }
        }

        await next(context);
    }

    private async Task UpdateOrganisationIdsClaim(UserData accountUser)
    {
        if (!await _featureManager.IsEnabledAsync(nameof(FeatureFlags.UseGraphApiForExtendedUserClaims)))
        {
            return;
        }

        var orgIdsClaim = await _claimsExtensionsWrapper.TryGetOrganisatonIds();

        if (orgIdsClaim is not null)
        {
            _logger.LogInformation("Found claim {Type} with value {Value}", ExtensionClaims.OrganisationIdsClaim, orgIdsClaim);
        }

        var organisationIds = string.Join(",", accountUser.Organisations.Select(o => o.OrganisationNumber));
        if (organisationIds != orgIdsClaim && _graphService is not NullGraphService)
        {
            await _graphService.PatchUserProperty(accountUser.Id.Value, ExtensionClaims.OrganisationIdsExtensionClaimName, organisationIds);
            _logger.LogInformation("Patched {Type} with value {Value}", ExtensionClaims.OrganisationIdsExtensionClaimName, organisationIds);
        }
    }

    private static string GetControllerName(HttpContext context)
    {
        var controllerName = string.Empty;
        var endpoint = context.GetEndpoint();

        if(endpoint != null)
        {
            controllerName = endpoint.Metadata?.GetMetadata<ControllerActionDescriptor>()?.ControllerName;
        }

        return controllerName;
    }
}