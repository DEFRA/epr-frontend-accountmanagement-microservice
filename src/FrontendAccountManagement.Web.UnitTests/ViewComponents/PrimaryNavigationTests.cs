using FrontendAccountManagement.Web.ViewComponents;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.ViewComponents;

[TestClass]
public class PrimaryNavigationTests : ViewComponentsTestBase
{
    private const string _placeholderLinkText = "https://non-existent.com/landing-page";
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private PrimaryNavigationViewComponent _component;
    private NavigationModel _manageAccountLink;

    [TestInitialize]
    public void Setup()
    {
        _manageAccountLink = new NavigationModel
        {
            LocalizerKey = "PrimaryNavigation.ManageAccount",
            LinkValue = $"/manage-account/{PagePath.ManageAccount}"
        };

        var externalUrlsOptions = new ExternalUrlsOptions
        {
            LandingPageUrl = _placeholderLinkText
        };
        
        var options = Options.Create(externalUrlsOptions);
        _component = new PrimaryNavigationViewComponent(options, AuthorizationServiceMock.Object, _contextAccessorMock.Object);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(PagePath.ManageAccount)]
    public void Invoke_SetsModel_WhereManageAccountLinkIsActive(string pagePath)
    {
        // Arrange
        SetViewComponentContext(pagePath, _component, null);
        
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(_component.HttpContext);
        AuthorizationServiceMock.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(), 
                It.IsAny<object>(), 
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());
        
        // Act
        var model = _component.InvokeAsync().Result.ViewData.Model as PrimaryNavigationModel;
        var activeItems = model.Items.Where(x => x.IsActive);

        // Assert
        Assert.IsTrue(activeItems.Count().Equals(1));
        Assert.IsTrue(activeItems.FirstOrDefault().LinkValue == _manageAccountLink.LinkValue);
        Assert.IsTrue(model.Items.Any(x => x.LinkValue == _manageAccountLink.LinkValue));
        Assert.IsTrue(model.Items.Any(x => x.LinkValue == _placeholderLinkText));
    }
}