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

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class AccountManagementTelephoneChangeTests : AccountManagementTestBase
{
    private const string Telephone = "123456"; 
    private const string ManageAccountTelephoneChangeKey = "ManageAccountTelephoneChange"; 

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

        TempDataDictionary[ManageAccountTelephoneChangeKey] = serializedModel;

        AutoMapperMock.Setup(m =>
            m.Map<EditUserDetailsViewModel>(mockUserData))
            .Returns(expectedModel);

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
        var previousNewDetails = new ManageAccountTelephoneViewModel
        {
            Telephone = "12345",
        };
        SystemUnderTest.TempData.Add("ManageAccountTelephoneChange", JsonSerializer.Serialize(previousNewDetails));

        var newNewDetails = new ManageAccountTelephoneViewModel
        {
            Telephone = "9876543",
        };

        var tempDataInitialState = DeserialiseUserDetailsJson(SystemUnderTest.TempData["ManageAccountTelephoneChange"]);

        tempDataInitialState.Should().BeEquivalentTo(previousNewDetails);

        // Act
        await SystemUnderTest.ManageAccountTelephone(newNewDetails);

        var updatedTempDataInitialState = DeserialiseUserDetailsJson(SystemUnderTest.TempData["ManageAccountTelephoneChange"]);

        updatedTempDataInitialState.Should().BeEquivalentTo(newNewDetails);
    }


    /// <summary>
    /// Parses the user details from the temp data back to an object.
    /// </summary>
    private ManageAccountTelephoneViewModel DeserialiseUserDetailsJson(object json)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
        stream.Position = 0;
        return (ManageAccountTelephoneViewModel)JsonSerializer.Deserialize(stream, typeof(ManageAccountTelephoneViewModel));
    }
}
