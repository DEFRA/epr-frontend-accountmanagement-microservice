using AutoFixture;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using FrontendAccountManagement.Core.Sessions;
using Microsoft.AspNetCore.Http;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class ConfirmCompanyDetailsTests : AccountManagementTestBase
    {
        private UserData _userData;
        private CompaniesHouseResponse _companiesHouseResponse;
        private ConfirmCompanyDetailsViewModel _viewModel;

        private Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _userData = _fixture.Create<UserData>();
            _companiesHouseResponse = _fixture.Create<CompaniesHouseResponse>();
            _viewModel = _fixture.Create<ConfirmCompanyDetailsViewModel>();

            SetupBase(_userData);

            FacadeServiceMock.Setup(x => x.GetCompaniesHouseResponseAsync(It.IsAny<string>()))
                .ReturnsAsync(_companiesHouseResponse);

            AutoMapperMock.Setup(x => x.Map<ConfirmCompanyDetailsViewModel>(_companiesHouseResponse))
                .Returns(_viewModel);
        }

        [TestMethod]
        public async Task ConfirmCompanyDetails_ShouldRedirectToErrorPage_WhenRegisteredOfficeIsNull()
        {
            // Arrange
            _companiesHouseResponse.Organisation.RegisteredOffice = null;
            SetupUserData("Approved Person");

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            var redirectToActionResult = result as RedirectToActionResult;

            Assert.AreEqual(PagePath.Error, redirectToActionResult.ControllerName.ToLower());
            Assert.AreEqual(nameof(ErrorController.Error).ToLower(), redirectToActionResult.ActionName);
            Assert.AreEqual((int)HttpStatusCode.NotFound, redirectToActionResult.RouteValues["statusCode"]);
        }

        [TestMethod]
        public async Task ConfirmCompanyDetails_ShouldReturnViewWithViewModel_WhenRegisteredOfficeIsNotNull()
        {
            // Arrange
            _companiesHouseResponse.Organisation.RegisteredOffice = _fixture.Create<AddressDto>();
            SetupUserData("Approved Person");

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(new JourneySession
                {
                    CompaniesHouseSession = new CompaniesHouseSession { CompaniesHouseResponse = _companiesHouseResponse }
                }));

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(nameof(AccountManagementController.ConfirmCompanyDetails), viewResult.ViewName);
            Assert.AreEqual(_viewModel, viewResult.Model);
        }

        private void SetupUserData(string serviceRole)
        {
            var userData = new UserData
            {
                ServiceRole = serviceRole,
                Organisations = new List<Organisation>
                {
                    new Organisation
                    {

                    }
                }
            };

            // Create a mock identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)),
            };

            var identity = new Mock<ClaimsIdentity>();
            identity.Setup(i => i.IsAuthenticated).Returns(true);
            identity.Setup(i => i.Claims).Returns(claims);

            // Create a mock ClaimsPrincipal
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(p => p.Identity).Returns(identity.Object);
            mockClaimsPrincipal.Setup(p => p.Claims).Returns(claims);

            // Use the mock ClaimsPrincipal in your tests
            var claimsPrincipal = mockClaimsPrincipal.Object;

            HttpContextMock.Setup(c => c.User).Returns(claimsPrincipal);
        }
    }
}
