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


namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class AccountManagementTelephoneChangeTests : AccountManagementTestBase
{
    private const string Telephone = "123456"; 
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
            IsChangeRequestPending = false,
            Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
            RoleInOrganisation = PersonRole.Admin.ToString(),
            Telephone = Telephone,
            ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
        };

        var expectedModel = new EditUserDetailsViewModel
        {
            Telephone = Telephone
        };

        SetupBase(mockUserData);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession
            {
                UserData = mockUserData
            });

        // Act
        var result = await SystemUnderTest.ManageAccountTelephone();

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(expectedModel.Telephone, ((ManageAccountTelephoneViewModel)viewResult.Model).Telephone);
    }

    [TestMethod]
    public async Task ShouldReturnForbidden_WhenIsChangeRequestPendingIsTrue()
    {
        // Arrange
        var mockUserData = new UserData
        {
            IsChangeRequestPending = true,
            Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
            RoleInOrganisation = PersonRole.Admin.ToString(),
            Telephone = Telephone,
            ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
        };

        var expectedModel = new EditUserDetailsViewModel
        {
            Telephone = Telephone
        };

        SetupBase(mockUserData);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession
            {
                UserData = mockUserData
            });

        // Act
        var result = await SystemUnderTest.ManageAccountTelephone();

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
            Telephone = Telephone,
            IsChangeRequestPending = false,
            Organisations = new List<Organisation>()
        };

        var expectedModel = new EditUserDetailsViewModel
        {
            Telephone = Telephone
        };

        SetupBase(mockUserData);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession
            {
                UserData = mockUserData
            });

        // Act
        var result = await SystemUnderTest.ManageAccountTelephone();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task TempDataHasValidAmendedTelephone_DeserializesAndReturnsViewWithModel()
    {
        // Arrange
        var mockUserData = new UserData
        {
            Telephone = Telephone,
            IsChangeRequestPending = false,
            Organisations = new List<Organisation>() { new Organisation { Id = Guid.NewGuid(), OrganisationType = "Companies House Company" } },
            RoleInOrganisation = PersonRole.Admin.ToString(),
            ServiceRoleId = (int)Core.Enums.ServiceRole.Approved
        };

        var expectedModel = new EditUserDetailsViewModel
        {
            Telephone = Telephone
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
        var result = await SystemUnderTest.ManageAccountTelephone();

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(expectedModel.Telephone, ((ManageAccountTelephoneViewModel)viewResult.Model).Telephone);
    }

    [TestMethod]
    public async Task Should_Overwrite_ManageAccountTelephone_TempData_When_Already_Set()
    {
        // Arrange
        var mockUserData = new UserData
        {
            Telephone = Telephone,
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

        var previousEditDetails = new EditUserDetailsViewModel
        {
            Telephone = "12345",
        };
        SystemUnderTest.TempData.Add(AmendedUserDetailsKey, JsonSerializer.Serialize(previousEditDetails));
        var newNewDetails = new ManageAccountTelephoneViewModel
        {
            Telephone = "9876543",
        };

        var tempDataInitialState = DeserialiseEditUserDetailsJson(SystemUnderTest.TempData[AmendedUserDetailsKey]);
        tempDataInitialState.Should().BeEquivalentTo(previousEditDetails);
        // Act
        var result = await SystemUnderTest.ManageAccountTelephone(newNewDetails);
        var updatedTempDataInitialState = DeserialiseEditUserDetailsJson(SystemUnderTest.TempData[AmendedUserDetailsKey]);
        // Assert
        updatedTempDataInitialState.Telephone.Should().Be(newNewDetails.Telephone);
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(SystemUnderTest.CheckYourDetailsApprovedUserCompanyHouse));
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
