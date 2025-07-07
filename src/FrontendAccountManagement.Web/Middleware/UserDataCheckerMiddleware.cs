using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FrontendAccountManagement.Web.Middleware;

public class UserDataCheckerMiddleware : IMiddleware
{
    private readonly IFacadeService _facadeService;
    private readonly IClaimsExtensionsWrapper _claimsExtensionsWrapper;
    private readonly ILogger<UserDataCheckerMiddleware> _logger;

    public UserDataCheckerMiddleware(
        IFacadeService facadeService,
        IClaimsExtensionsWrapper claimsExtensionsWrapper,
        ILogger<UserDataCheckerMiddleware> logger)
    {
        _facadeService = facadeService;
        _claimsExtensionsWrapper = claimsExtensionsWrapper;
        _logger = logger;
    }

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		var anonControllers = new List<string> { "Privacy", "Cookies", "Culture" };
		var controllerName = GetControllerName(context);

		var existingUserData = context.User.GetUserData();


		if (!anonControllers.Contains(controllerName) && context.User.Identity is { IsAuthenticated: true } && existingUserData is null)
		{
			var userAccount = await _facadeService.GetUserAccountWithEnrolments(string.Empty);
			if (userAccount is null)
			{
				_logger.LogInformation("User authenticated but account could not be found");
			}
			else
			{
				await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);
			}
		}

		await next(context);
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