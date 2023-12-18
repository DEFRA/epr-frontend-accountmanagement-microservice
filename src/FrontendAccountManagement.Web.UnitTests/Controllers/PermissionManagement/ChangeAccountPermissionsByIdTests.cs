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
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Security.Claims;
using System.Text.Json;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http.Headers;
using static System.Net.WebRequestMethods;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using FrontendAccountManagement.Web.Constants;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement;

[TestClass]
public class ChangeAccountPermissionsByIdTests
{
    protected const string ModelErrorKey = "Error";
    private Mock<HttpContext> _httpContextMock;
    private Mock<HttpRequest> _httpRequestMock;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock = default!;
    private PermissionManagementController _systemUnderTest = default!;
    private Mock<IFacadeService> _facadeServiceMock = default!;
    private readonly Guid _id = new("aa5129bc-f86c-40fd-b595-328122c7e67d");
    private readonly Guid _userId = Guid.NewGuid();
    private IOptions<ServiceSettingsOptions> _serviceSettingsOptions;

    protected JourneySession AccountManagementSessionMock { get; set; } = default!;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpRequestMock = new Mock<HttpRequest>();
        _httpRequestMock.Setup(x => x.Scheme).Returns("http");
        _httpRequestMock.Setup(x => x.Host).Returns(HostString.FromUriComponent("http://localhost:8080"));
        _httpRequestMock.Setup(x => x.PathBase).Returns(PathString.FromUriComponent("/manage-account"));
        _httpRequestMock.Setup(x => x.Headers.Referer).Returns("");
        _httpContextMock.Setup(x => x.Request).Returns(new DefaultHttpContext().Request);
        _httpContextMock.Object.Request.GetTypedHeaders().Referer = new Uri("http://localhost:8080");

        _facadeServiceMock = new Mock<IFacadeService>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult(new JourneySession()));

        _serviceSettingsOptions = Options.Create<ServiceSettingsOptions>(new ServiceSettingsOptions() { ServiceKey = "Packaging" });
    }

    [DataRow(PermissionType.Basic)]
    [DataRow(PermissionType.Admin)]
    [DataRow(PermissionType.Delegated)]
    [DataRow(PermissionType.Basic)]
    [DataRow(PermissionType.Admin)]
    [DataRow(PermissionType.Approved)]
    [DataRow(null)]
    [TestMethod]
    public async Task ChangeAccountPermissionsId_PermissionDoesNotChange_RedirectsToManageAccount(PermissionType permissionType)
    {
        var userData = new UserData
        {
            Id = _userId,
            Organisations = new List<Organisation>
             {
                 new Organisation { Id = Guid.NewGuid() }
             }
        };
        CreateUserData(userData);

        _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: permissionType, UserId: _userId)));

        _systemUnderTest = new PermissionManagementController(_sessionManagerMock.Object, _facadeServiceMock.Object, _serviceSettingsOptions);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;

        var result = await _systemUnderTest.ChangeAccountPermissions(_id);

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task ChangeAccountPermissionsId_WhenUserDataIsNull_RedirectsToManageAccount()
    {
        _httpContextMock.Setup(c => c.User).Returns(new ClaimsPrincipal());

        _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: _userId)));

        _systemUnderTest = new PermissionManagementController(_sessionManagerMock.Object, _facadeServiceMock.Object, _serviceSettingsOptions);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;

        var result = await _systemUnderTest.ChangeAccountPermissions(_id);

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task ChangeAccountPermissionsId_WhenOrganisationIdNull_RedirectsToManageAccount()
    {
        var userData = new UserData
        {
            Id = _userId,
            Organisations = new List<Organisation>
             {
                 new Organisation { Id = null }
             }
        };
        CreateUserData(userData);

        _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: _userId)));

        _systemUnderTest = new PermissionManagementController(_sessionManagerMock.Object, _facadeServiceMock.Object, _serviceSettingsOptions);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;

        var result = await _systemUnderTest.ChangeAccountPermissions(_id);

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [DataRow("Approved Person", ServiceKey.Packaging, true)]
    [DataRow("Delegated Person", ServiceKey.Packaging, false)]
    [TestMethod]
    public async Task ChangeAccountPermissionsId_WhenUserIsApprovedAndHasPackagingServiceKey_ShowsDelegatedPersonContent(string serviceRole, string serviceKey, bool showDelegatedContent)
    {
        var userData = new UserData
        {
            Id = _userId,
            Organisations = new List<Organisation>
            {
                new Organisation { Id = Guid.NewGuid() }
            },
            ServiceRole = serviceRole
        };
        CreateUserData(userData);

        _serviceSettingsOptions = Options.Create<ServiceSettingsOptions>(new ServiceSettingsOptions() { ServiceKey = serviceKey });

        _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: Guid.NewGuid())));

        _systemUnderTest = new PermissionManagementController(_sessionManagerMock.Object, _facadeServiceMock.Object, _serviceSettingsOptions);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;

        var result = await _systemUnderTest.ChangeAccountPermissions(_id) as ViewResult;

        var model = (ChangeAccountPermissionViewModel)result.Model;
        result.Should().BeOfType<ViewResult>();
        model.Id.Should().Be(_id);
        model.PermissionType.Should().Be(PermissionType.Admin);
        model.ShowDelegatedContent.Should().Be(showDelegatedContent);

    }

    private void CreateUserData(UserData userData)
    {
        var userDataString = JsonSerializer.Serialize(userData);

        _httpContextMock.Setup(c => c.User).Returns(new ClaimsPrincipal());
        _httpContextMock.Object.User.AddIdentity(new ClaimsIdentity(new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.UserData, userDataString),
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", _userId.ToString())
        }));
    }

}