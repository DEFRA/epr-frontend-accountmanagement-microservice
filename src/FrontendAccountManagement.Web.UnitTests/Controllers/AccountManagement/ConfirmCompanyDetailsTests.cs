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

            // Act
            var result = await SystemUnderTest.ConfirmCompanyDetails();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNull(viewResult.ViewName);
            Assert.AreEqual(_viewModel, viewResult.Model);
        }
    }
}
