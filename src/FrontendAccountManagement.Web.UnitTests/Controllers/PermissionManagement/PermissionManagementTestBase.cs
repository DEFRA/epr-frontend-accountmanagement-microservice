using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text.Json;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

public abstract class PermissionManagementTestBase
{
    protected const string ModelErrorKey = "Error";
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    public Mock<HttpContext> _httpContextMock = default!;
    protected Mock<HttpRequest> _httpRequestMock = default!;
    protected Mock<ISessionManager<JourneySession>> _sessionManagerMock = default!;
    public PermissionManagementController _systemUnderTest = default!;
    public Mock<IFacadeService> _facadeServiceMock = default!;

    protected JourneySession _accountManagementSessionMock { get; set; } = default!;

    protected void SetupBase()
    {
        var userData = new UserData
        {
             Id= Guid.NewGuid(),
             Organisations = new List<Organisation> 
             { 
                 new Organisation { Id = Guid.NewGuid() } 
             }
        };

        var userDataString = JsonSerializer.Serialize(userData);
        var identity = new ClaimsIdentity();
        identity.AddClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.UserData, userDataString)
            });
        var user = new ClaimsPrincipal(identity);

        _httpContextMock = new Mock<HttpContext>();
        _httpRequestMock = new Mock<HttpRequest>();
        _httpRequestMock.Setup(x => x.Scheme).Returns("http");
        _httpRequestMock.Setup(x => x.Host).Returns(HostString.FromUriComponent("http://localhost:8080"));
        _httpRequestMock.Setup(x => x.PathBase).Returns(PathString.FromUriComponent("/manage-account"));
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
        _httpContextMock.Setup(x => x.User).Returns(user);
        _facadeServiceMock = new Mock<IFacadeService>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult(new JourneySession()));

        var serviceSettingsOptions = Options.Create<ServiceSettingsOptions>(new ServiceSettingsOptions { ServiceKey = "Packaging" });

        _systemUnderTest = new PermissionManagementController(_sessionManagerMock.Object, _facadeServiceMock.Object, serviceSettingsOptions);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}
