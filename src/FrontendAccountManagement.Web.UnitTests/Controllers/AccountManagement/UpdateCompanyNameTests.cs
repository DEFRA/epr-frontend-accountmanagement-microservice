﻿using AutoFixture;
using EPR.Common.Authorization.Models;
using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class UpdateCompanyNameTests : AccountManagementTestBase
    {
        private UserData _userData;
        private JourneySession _journeySession;
        private Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _userData = _fixture.Create<UserData>();
            _userData.IsChangeRequestPending = false;

            SetupBase(_userData);

            _journeySession = new JourneySession
            {
                UserData = _userData,
                AccountManagementSession = new AccountManagementSession() { Journey = new List<string>() }
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(_journeySession));
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldRedirectIfOrganisationIsCompaniesHouseCompany()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.CompaniesHouseCompany
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be("Error");
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldReturnViewWithModelWhenSessionIsNotNull()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = true  
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyNameViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyName.Should().Be(YesNoAnswer.Yes);  
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldReturnViewWithModelWhenIsUpdateCompanyNameIsFalse()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = false  
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyNameViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyName.Should().Be(YesNoAnswer.No);  
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldReturnViewWithModelWhenIsUpdateCompanyNameIsNull()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = null  
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyNameViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyName.Should().BeNull();  
            }
        }


        [TestMethod]
        public async Task Post_UpdateCompanyName_ShouldReturnRedirectWhenModelIsValid()
        {
            // Arrange
            var model = new UpdateCompanyNameViewModel { IsUpdateCompanyName = YesNoAnswer.Yes };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession()
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be(nameof(SystemUnderTest.CompanyName));
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyName_ShouldReturnRedirectWhenModelIsNotValid()
        {
            // Arrange
            var model = new UpdateCompanyNameViewModel();
            SystemUnderTest.ModelState.AddModelError("Error", "Some error message");

            // Act
            var result = await SystemUnderTest.UpdateCompanyName(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                viewResult.Model.Should().Be(model);
            }
        }
    }
}
