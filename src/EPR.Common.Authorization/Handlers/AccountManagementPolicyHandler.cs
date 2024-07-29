namespace EPR.Common.Authorization.Handlers;

using System.Security.Claims;
using Config;
using Helpers;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Requirements;
using Sessions;

public sealed class AccountManagementPolicyHandler<TSessionType>
    : PolicyHandlerBase<AccountManagementPolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    public AccountManagementPolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<AccountManagementPolicyHandler<TSessionType>> logger)
        : base(sessionManager, httpClientFactory, options, logger)
    {
    }

    protected override string PolicyHandlerName => nameof(AccountManagementPolicyHandler<TSessionType>);
    protected override string PolicyDescription => "manage users";
    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
        ClaimsPrincipleHelper.IsEnrolledAdminOrBasic;
}