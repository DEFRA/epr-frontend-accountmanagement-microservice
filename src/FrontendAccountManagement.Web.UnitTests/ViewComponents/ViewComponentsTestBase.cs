using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using System.Security.Claims;

namespace FrontendAccountManagement.Web.UnitTests.ViewComponents;

public abstract class ViewComponentsTestBase
{
    protected readonly Mock<IAuthorizationService> AuthorizationServiceMock = new();
    protected readonly Mock<IFacadeService> FacadeService = new();
    protected readonly Mock<ISessionManager<JourneySession>> SessionManager = new();
    protected readonly Mock<ISession> Session = new();
    protected readonly Mock<HttpContext> HttpContextMock = new();
    protected readonly Mock<ClaimsPrincipal> UserMock = new();
    protected readonly ViewContext ViewContext = new();
    protected readonly ViewComponentContext ViewComponentContext = new();
    protected readonly Mock<IHttpContextAccessor> ViewComponentHttpContextAccessor = new();

    protected void SetViewComponentContext(string requestPath, ViewComponent component, UserData? userData)
    {
        var claims = new List<Claim>();
        if (userData != null)
        {
            claims.Add(new(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userData)));
        }
        
        UserMock.Setup(x => x.Claims).Returns(claims);

        var mockClaimsIdentity = new Mock<ClaimsIdentity>();
        UserMock.Setup(cp => cp.Identity).Returns(mockClaimsIdentity.Object);
        mockClaimsIdentity.Setup(ci => ci.Claims).Returns(claims);

        HttpContextMock.Setup(x => x.Request.PathBase).Returns( new PathString($"/manage-account"));
        HttpContextMock.Setup(x => x.Request.Path).Returns($"/{requestPath}");
        HttpContextMock.Setup(x => x.Session).Returns(Session.Object);
        HttpContextMock.Setup(x => x.User).Returns(UserMock.Object);
        ViewComponentHttpContextAccessor.Setup(x => x.HttpContext).Returns(HttpContextMock.Object);
        ViewContext.HttpContext = HttpContextMock.Object;
        ViewComponentContext.ViewContext = ViewContext;
        component.ViewComponentContext = ViewComponentContext;
    }
}