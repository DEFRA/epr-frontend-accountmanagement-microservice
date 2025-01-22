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
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;

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
            _userData = _fixture.Build<UserData>()
                .With(x => x.RoleInOrganisation, PersonRole.Admin.ToString())
                .With(x => x.ServiceRoleId, (int)Core.Enums.ServiceRole.Approved)
                .Create();

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
            SetupUserData("Approved Person", 1, "Admin");

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
            SetupUserData("Approved Person", 1, "Admin");

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(new AccountManagementSession
                {
                    CompaniesHouseSession = new CompaniesHouseSession { CompaniesHouseData = _companiesHouseResponse }
                }));

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNull(viewResult.ViewName);
            Assert.AreEqual(_viewModel, viewResult.Model);
        }

        [TestMethod]
        public async Task ConfirmCompanyDetails_ShouldDisplayPageNotFound_WhenUserIsBasicEmployee()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = PersonRole.Employee.ToString(),
            };

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ConfirmCompanyDetails_ShouldDisplayPageNotFound_WhenUserIsBasicAdmin()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ConfirmCompanyDetails_ShouldDisplayErrorPage_WhenUserIsNotAnApprovedPerson()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.RegulatorAdmin.ToString(),
                ServiceRoleId = 4,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task CheckCompaniesHouseDetails_ShouldDisplayErrorPage_WhenUserIsNotAnApprovedPerson()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.RegulatorAdmin.ToString(),
                ServiceRoleId = 4,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            var mockViewModel = new CheckYourOrganisationDetailsViewModel();

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.CheckCompaniesHouseDetails(mockViewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        private void SetupUserData(string serviceRole,
            int? serviceRoleId = null,
            string roleInOrganisation = null)
        {
            var userData = new UserData
            {
                ServiceRole = serviceRole,
                ServiceRoleId = serviceRoleId != null ? serviceRoleId.Value : default,
                RoleInOrganisation = string.IsNullOrEmpty(roleInOrganisation) ? default : roleInOrganisation,
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
