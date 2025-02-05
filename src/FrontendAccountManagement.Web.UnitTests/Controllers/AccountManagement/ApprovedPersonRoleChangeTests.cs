using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class ApprovedPersonRoleChangeTests: AccountManagementTestBase
    {
        private const string _jobTitle = "Director";
        private const string AmendedUserDetailsKey = "AmendedUserDetails";

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        [TestMethod]
        public async Task ShouldReturnViewWithCorrectModel()
        {
            // Arrange
            var mockUserData = new UserData
            {
                JobTitle = _jobTitle,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
                RoleInOrganisation = PersonRole.Admin.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                JobTitle = _jobTitle
            };

            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonRoleChange();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(expectedModel.JobTitle, ((ApprovedPersonRoleChangeViewModel)viewResult.Model).SelectedCompaniesHouseRole);
        }

        [TestMethod]
        public async Task ShouldReturnForbidden_WhenIsChangeRequestPendingIsTrue()
        {
            // Arrange
            var mockUserData = new UserData
            {
                JobTitle = _jobTitle,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>()
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                JobTitle = _jobTitle
            };

            AutoMapperMock.Setup(m =>
                m.Map<EditUserDetailsViewModel>(mockUserData))
                .Returns(expectedModel);

            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonRoleChange();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = (RedirectToActionResult)result;

            redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
            redirectResult.ActionName.Should().Be(PagePath.Error);
            redirectResult.RouteValues.Should().ContainKey("statusCode");
            redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task ShouldReturnForbidden_WhenUserIsNotApprovedCompany()
        {
            // Arrange
            var mockUserData = new UserData
            {
                JobTitle = _jobTitle,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>()
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                JobTitle = _jobTitle
            };

            AutoMapperMock.Setup(m =>
                m.Map<EditUserDetailsViewModel>(mockUserData))
                .Returns(expectedModel);

            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonRoleChange();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = (RedirectToActionResult)result;

            redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
            redirectResult.ActionName.Should().Be(PagePath.Error);
            redirectResult.RouteValues.Should().ContainKey("statusCode");
            redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task TempDataHasValidAmendedUserDetails_DeserializesAndReturnsViewWithModel()
        {
            // Arrange
            var mockUserData = new UserData
            {
                JobTitle = _jobTitle,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
                RoleInOrganisation = PersonRole.Admin.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                JobTitle = _jobTitle
            };

            var serializedModel = JsonSerializer.Serialize(expectedModel);

            SetupBase(mockUserData);

            TempDataDictionary[AmendedUserDetailsKey] = serializedModel;

            AutoMapperMock.Setup(m =>
                m.Map<EditUserDetailsViewModel>(mockUserData))
                .Returns(expectedModel);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonRoleChange();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(expectedModel.JobTitle, ((ApprovedPersonRoleChangeViewModel)viewResult.Model).SelectedCompaniesHouseRole);
        }

        [TestMethod]
        public async Task Should_Overwrite_ApprovedPersonRoleChange_TempData_When_Already_Set()
        {
            // Arrange
            var previousNewDetails = new ApprovedPersonRoleChangeViewModel
            {
                SelectedCompaniesHouseRole = "Previous Role Name"
            };
            SystemUnderTest.TempData.Add("ApprovedPersonRoleChange", JsonSerializer.Serialize(previousNewDetails));

            var newNewDetails = new ApprovedPersonRoleChangeViewModel
            {
                SelectedCompaniesHouseRole = "New Role Name"
            };

            var tempDataInitialState = DeserialiseUserDetailsJson(SystemUnderTest.TempData["ApprovedPersonRoleChange"]);
            
            tempDataInitialState.Should().BeEquivalentTo(previousNewDetails);

            // Act
            await SystemUnderTest.ApprovedPersonRoleChange(newNewDetails);

            var updatedTempDataInitialState = DeserialiseUserDetailsJson(SystemUnderTest.TempData["ApprovedPersonRoleChange"]);

            updatedTempDataInitialState.Should().BeEquivalentTo(newNewDetails);
        }

        /// <summary>
        /// Parses the user details from the temp data back to an object.
        /// </summary>
        private ApprovedPersonRoleChangeViewModel DeserialiseUserDetailsJson(object json)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return (ApprovedPersonRoleChangeViewModel)JsonSerializer.Deserialize(stream, typeof(ApprovedPersonRoleChangeViewModel));
        }
    }
}
