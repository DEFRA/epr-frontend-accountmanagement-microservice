using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FrontendAccountManagement.Web.Middleware;

public class UserDataCheckerMiddleware : IMiddleware
{
    private readonly IFacadeService _facadeService;
    private readonly ILogger<UserDataCheckerMiddleware> _logger;

    public UserDataCheckerMiddleware(
        IFacadeService facadeService,
        ILogger<UserDataCheckerMiddleware> logger)
    {
        _facadeService = facadeService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var anonControllers = new List<string> { "Privacy", "Cookies", "Culture" };
        var controllerName = GetControllerName(context);
        
        var existingUserData = context.User.TryGetUserData(_logger);

        if (!anonControllers.Contains(controllerName) && context.User.Identity is { IsAuthenticated: true } && existingUserData is null)
        {
            var userAccount = await _facadeService.GetUserAccount();

            if (userAccount is null)
            {
                _logger.LogInformation("User authenticated but account could not be found");
            }
            else
            {
                await ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync(context, userAccount.User);
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