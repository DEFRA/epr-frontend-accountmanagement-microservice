using EPR.Common.Authorization.Models;
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
    public class ApprovedPersonNameChangeTests: AccountManagementTestBase
    {
        private const string FirstName = "Test First Name";
        private const string LastName = "Test Last Name";
        private const string Telephone = "07545822431";
        private const string AmendedUserDetailsKey = "AmendedUserDetails";

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        [TestMethod]
        public async Task ShouldReturnForbidden_WhenIsChangeRequestPendingIsTrue()
        {
            // Arrange
            var mockUserData = new UserData
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>()
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone
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
                Telephone = Telephone,
                IsChangeRequestPending = false,
                Organisations = new List<Organisation>()
            };

            var expectedModel = new EditUserDetailsViewModel
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone
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
            var result = await SystemUnderTest.ApprovedPersonNameChange();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(expectedModel.FirstName, ((ApprovedPersonNameChangeViewModel)viewResult.Model).FirstName);
        }

        [TestMethod]
        public async Task Should_Overwrite_ApprovedPersonNameChange_When_TempData_Set()
        {
            // Arrange
            var previousNewDetails = new ApprovedPersonNameChangeViewModel
            {
                FirstName = "Previous first name",
                LastName = "Previous last name",
            };
            SystemUnderTest.TempData.Add("ApprovedPersonNameChange", JsonSerializer.Serialize(previousNewDetails));

            var newNewDetails = new ApprovedPersonNameChangeViewModel
            {
                FirstName = "New first name",
                LastName = "New last name",
            };

            var tempDataInitialState = DeserialiseUserDetailsJson(SystemUnderTest.TempData["ApprovedPersonNameChange"]);
            
            tempDataInitialState.Should().BeEquivalentTo(previousNewDetails);

            // Act
            await SystemUnderTest.ApprovedPersonNameChange(newNewDetails);

            var updatedTempDataInitialState = DeserialiseUserDetailsJson(SystemUnderTest.TempData["ApprovedPersonNameChange"]);

            updatedTempDataInitialState.Should().BeEquivalentTo(newNewDetails);
        }

        /// <summary>
        /// Parses the user details from the temp data back to an object.
        /// </summary>
        private ApprovedPersonNameChangeViewModel DeserialiseUserDetailsJson(object json)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return (ApprovedPersonNameChangeViewModel)JsonSerializer.Deserialize(stream, typeof(ApprovedPersonNameChangeViewModel));
        }
    }
}
