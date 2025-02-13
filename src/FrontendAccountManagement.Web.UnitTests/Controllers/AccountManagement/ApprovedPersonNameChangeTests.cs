using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Text.Json;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class ApprovedPersonNameChangeTests: AccountManagementTestBase
    {
        private const string FirstName = "Test First Name";
        private const string LastName = "Test Last Name";
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
                FirstName = FirstName,
                LastName = LastName,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
                RoleInOrganisation = PersonRole.Admin.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                FirstName = FirstName,
                LastName = LastName
            };

            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonNameChange();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(expectedModel.FirstName, ((ApprovedPersonNameChangeViewModel)viewResult.Model).FirstName);
            Assert.AreEqual(expectedModel.LastName, ((ApprovedPersonNameChangeViewModel)viewResult.Model).LastName);
        }

        [TestMethod]
        public async Task ShouldReturnForbidden_WhenIsChangeRequestPendingIsTrue()
        {
            // Arrange
            var mockUserData = new UserData
            {
                FirstName = FirstName,
                LastName = LastName,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>()
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                FirstName = FirstName,
                LastName = LastName
            };

            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonNameChange();

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
                FirstName = FirstName,
                LastName = LastName,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>()
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                FirstName = FirstName,
                LastName = LastName
            };


            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonNameChange();

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
                FirstName = FirstName,
                LastName = LastName,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
                RoleInOrganisation = PersonRole.Admin.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                FirstName = FirstName,
                LastName = LastName
            };

            var serializedModel = JsonSerializer.Serialize(expectedModel);

            SetupBase(mockUserData);

            TempDataDictionary[AmendedUserDetailsKey] = serializedModel;

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            // Act
            var result = await SystemUnderTest.ApprovedPersonNameChange();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(expectedModel.FirstName, ((ApprovedPersonNameChangeViewModel)viewResult.Model).FirstName);
        }

        [TestMethod]
        public async Task Should_Overwrite_ApprovedPersonNameChange_TempData_When_Already_Set()
        {
            // Arrange
            var mockUserData = new UserData
            {
                Telephone = "020 665455",
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
                RoleInOrganisation = PersonRole.Admin.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
            };

            SetupBase(mockUserData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    UserData = mockUserData
                });

            var previousNewDetails = new EditUserDetailsViewModel()
            {
                FirstName = "Previous first name",
                LastName = "Previous last name",
            };

            SystemUnderTest.TempData.Add(AmendedUserDetailsKey, JsonSerializer.Serialize(previousNewDetails));
            
            var newNewDetails = new ApprovedPersonNameChangeViewModel()
            {
                FirstName = "New first name",
                LastName = "New last name",
            };

            var tempDataInitialState = DeserialiseEditUserDetailsJson(SystemUnderTest.TempData[AmendedUserDetailsKey]);
            tempDataInitialState.Should().BeEquivalentTo(previousNewDetails);

            // Act
            await SystemUnderTest.ApprovedPersonNameChange(newNewDetails);

            var updatedTempDataInitialState = DeserialiseEditUserDetailsJson(SystemUnderTest.TempData[AmendedUserDetailsKey]);


            updatedTempDataInitialState.FirstName.Should().BeEquivalentTo(newNewDetails.FirstName);
            updatedTempDataInitialState.LastName.Should().BeEquivalentTo(newNewDetails.LastName);
        }

        /// <summary>
        /// Parses the user details from the temp data back to an object.
        /// </summary>
        private EditUserDetailsViewModel DeserialiseEditUserDetailsJson(object json)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return (EditUserDetailsViewModel)JsonSerializer.Deserialize(stream, typeof(EditUserDetailsViewModel));
        }
    }
}
