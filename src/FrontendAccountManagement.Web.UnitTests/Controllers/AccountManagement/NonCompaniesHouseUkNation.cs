using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using EPR.Common.Authorization.Models;
using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Constants.Enums;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class NonCompaniesHouseUkNation : AccountManagementTestBase
    {
        private UserData _userData;
        private JourneySession _journeySession;

        private readonly Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _userData = _fixture.Create<UserData>();
            _userData.Organisations[0].CompaniesHouseNumber = null;

            SetupBase(_userData);

            _journeySession = new JourneySession
            {
                UserData = _userData,
                AccountManagementSession = new AccountManagementSession()
                {
                    Journey = 
                    [
                        PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress,PagePath.NonCompaniesHouseUkNation
                    ],
                    BusinessAddress = new Core.Addresses.Address()
                }
            };

            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_journeySession);
        }

        [TestMethod]
        public async Task NonCompaniesHouseUkNationGet_ShouldReturnView_WithCorrectUkNation()
        {
            //Arrange
            _journeySession.AccountManagementSession.UkNation = (Core.Enums.Nation?)UkNation.England;
            //Act
            var result = await SystemUnderTest.NonCompaniesHouseUkNation();

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();

                var viewResult = (ViewResult)result;
                viewResult.Model.Should().BeOfType<NonCompaniesHouseUkNationViewModel>();

                var model = (NonCompaniesHouseUkNationViewModel)viewResult.Model!;

                model.UkNation.Should().Be(UkNation.England);
            }     
        }

        [TestMethod]
        public async Task GivenValidUkNation_WhenNonCompaniesHouseUkNationsCalled_ThenRedirectToCheckCompanyDetailsPage_AndUpdateSession()
        {
            // Arrange
            var request = new NonCompaniesHouseUkNationViewModel { UkNation = Constants.Enums.UkNation.England };

            // Act
            var result = await SystemUnderTest.NonCompaniesHouseUkNation(request);
      
            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.CheckCompanyDetails));

            Assert.AreEqual(Core.Enums.Nation.England, _journeySession.AccountManagementSession.UkNation);
            SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }

        [TestMethod]
        public async Task GivenCompaniesHouseUser_WhenNonCompaniesHouseUkNationCalled_ThenRedirectToErrorPage_AndUpdateSession()
        {
            // Arrange
            _userData.Organisations[0].CompaniesHouseNumber = "123";
            SetupBase(_userData);

            // Act
            var result = await SystemUnderTest.NonCompaniesHouseUkNation();
            
            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            ((RedirectToActionResult)result).ControllerName.Should().Be(nameof(ErrorController.Error));
        }

        [TestMethod]
        public async Task NonCompaniesHouseUkNation_ShouldReturnViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new NonCompaniesHouseUkNationViewModel { UkNation = null };
            SystemUnderTest.ModelState.AddModelError("UkNation", "Required");

            // Act
            var result = await SystemUnderTest.NonCompaniesHouseUkNation(model);

            // Assert
            var viewResult = result as ViewResult;

            Assert.AreEqual(model, viewResult.Model);
        }

        [TestMethod]
        public async Task GivenUserWithNoOrganisation_WhenNonCompaniesHouseUkNatioCalled_ThrowInvalidOperationException()
        {
            // Arrange
            Exception result = null;

            _userData.Organisations[0] = null;

            SetupBase(_userData);

            // Act
            try
            {
                await SystemUnderTest.NonCompaniesHouseUkNation();
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            result.Should().BeOfType<InvalidOperationException>();
        }

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenNonCompaniesHouseUkNationCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            // Act
            var result = await SystemUnderTest.NonCompaniesHouseUkNation();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();

                var actionResult = result as RedirectToActionResult;
                actionResult.Should().NotBeNull();
                actionResult.ActionName.Should().Be(PagePath.Error);
                actionResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                actionResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenNonCompaniesHouseUkNationSubmitted_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            var model = new UkNationViewModel { UkNation = UkNation.England };

            // Act
            var result = await SystemUnderTest.NonCompaniesHouseUkNation(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();

                var actionResult = result as RedirectToActionResult;
                actionResult.Should().NotBeNull();
                actionResult.ActionName.Should().Be(PagePath.Error);
                actionResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                actionResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
            }
        }
    }
}
