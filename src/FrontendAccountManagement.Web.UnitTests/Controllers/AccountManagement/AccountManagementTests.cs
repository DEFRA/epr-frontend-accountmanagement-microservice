using System.Net;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Http;
using Moq;
using FrontendAccountManagement.Core.Models;
using EPR.Common.Authorization.Models;
using System;
using FrontendAccountManagement.Core.Enums;
using System.Security.Claims;
using System.Text.Json;
using Organisation = EPR.Common.Authorization.Models.Organisation;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using AutoMapper;
using FrontendAccountManagement.Web.Constants.Enums;
using FrontendAccountManagement.Web.Controllers.AccountManagement;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class AccountManagementTests : AccountManagementTestBase
{
    private const string ViewName = "ManageAccount";
    private const string FirstName = "Test First Name";
    private const string LastName = "Test Last Name";
    private const string Telephone = "07545822431";

    private const string JobTitle = "Test Job Title";
    private const string OrganisationName = "Test Organization Name";
    private const string OrganisationType = "Companies House Company";

    private const string SubBuildingName = "Unit 5";
    private const string BuildingNumber = "10";
    private const string BuildingName = "Building";
    private const string Street = "A Street";
    private const string Town = "A Town";
    private const string County = "County";
    private const string Postcode = "AB1 1BA";

    private const string RoleInOrganisation = "Admin";
    private const int ServiceRoleId = 1;
    private const string ServiceRoleKey = "Approved.Admin";

    private const string AmendedUserDetailsKey = "AmendedUserDetails";


    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_WithNoJourneyPathSet_ThenShowManageAccountPage()
    {
        // Arrange
        SetupUserData(string.Empty);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenDeploymentRoleIsRegulator_WithRegulatorAdminServiceRole_ThenShowManageAccountPage()
    {
        // Arrange
        var mockUserData = new UserData
        {
            FirstName = FirstName,
            LastName = LastName,
            Telephone = Telephone,
            IsChangeRequestPending = true,
            ServiceRole = Core.Enums.ServiceRole.RegulatorAdmin.ToString(),
            ServiceRoleId = (int)Core.Enums.ServiceRole.RegulatorAdmin,
            RoleInOrganisation = PersonRole.Admin.ToString(),
            Organisations = new List<Organisation>
            {
                new Organisation()
            }
        };

        SetupBase(
            deploymentRole: DeploymentRoleOptions.RegulatorRoleValue,
            userServiceRoleId: (int)Core.Enums.ServiceRole.RegulatorAdmin,
            userData: mockUserData);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        result.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
        result.ViewData["CustomBackLinkToDisplay"].Should().Be("/back/to/home");
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenDeploymentRoleIsRegulator_WithInvalidServiceRole_ThenReturnForbidden()
    {
        // Arrange
        var mockUserData = new UserData
        {
            FirstName = FirstName,
            LastName = LastName,
            Telephone = Telephone,
            IsChangeRequestPending = true,
            ServiceRole = Core.Enums.ServiceRole.RegulatorBasic.ToString(),
            ServiceRoleId = (int)Core.Enums.ServiceRole.RegulatorBasic,
            RoleInOrganisation = PersonRole.Employee.ToString(),
            Organisations = new List<Organisation>
            {
                new Organisation()
            }
        };

        SetupBase(
            deploymentRole: DeploymentRoleOptions.RegulatorRoleValue,
            userServiceRoleId: (int)Core.Enums.ServiceRole.RegulatorBasic,
            userData: mockUserData);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel());

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenDeploymentRoleIsNotRegulator_WithRegulatorAdminServiceRole_ThenReturnForbidden()
    {
        // Arrange
        var mockUserData = new UserData
        {
            FirstName = FirstName,
            LastName = LastName,
            Telephone = Telephone,
            IsChangeRequestPending = true,
            ServiceRole = Core.Enums.ServiceRole.RegulatorBasic.ToString(),
            ServiceRoleId = (int)Core.Enums.ServiceRole.RegulatorBasic,
            RoleInOrganisation = PersonRole.Employee.ToString(),
            Organisations = new List<Organisation>
            {
                new Organisation()
            }
        };


        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession
            {
                UserData = mockUserData
            });

        SetupBase(
            deploymentRole: string.Empty,
            userServiceRoleId: (int)Core.Enums.ServiceRole.RegulatorAdmin,
            userData: mockUserData);

      

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel());

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_WithRemoveUserStatusNotNull_ThenShowManageAccountPage()
    {
        // Arrange
        SetupUserData(string.Empty);

        RemoveUserJourneyModel userDetails = new() { FirstName = "An", LastName = "Test" };
        AccountManagementSession removeUserAccount = new() { RemoveUserStatus = 0, RemoveUserJourney = userDetails };
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult(new JourneySession { AccountManagementSession = removeUserAccount }));

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_WithAddUserStatusNotNull_ThenShowManageAccountPage()
    {
        // Arrange
        SetupUserData(string.Empty);

        AddUserJourneyModel userDetails = new() { Email = "an.other@test.com", UserRole = "Test" };
        AccountManagementSession addUserAccount = new() { AddUserStatus = 0, AddUserJourney = userDetails };
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult(new JourneySession { AccountManagementSession = addUserAccount }));
        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenRemoveTeamMemberConfirmationCalled_ThenShowManageAccountPage()
    {
        // Arrange      
        RemoveTeamMemberConfirmationViewModel teamMember = new();
        var userData = new UserData()
        {
            FirstName = "FName",
            LastName = "LName",
            JobTitle = "Director",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            }
        };

        SetupBase(userData);

        SystemUnderTest.ModelState.AddModelError("key", "error message");

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation(teamMember) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(null);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_ThenManageAccountViewModelReturnedWithAdditionalFieldsToDisplay()
    {
        // Arrange
        var userData = SetupUserData(string.Empty);

        SetupBase(userData: userData);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;
        var model = result.Model as ManageAccountViewModel;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);

        Assert.AreEqual(string.Format("{0} {1}", FirstName, LastName), model.UserName);
        Assert.AreEqual(JobTitle, model.JobTitle);
        Assert.AreEqual(Telephone, model.Telephone);
        Assert.AreEqual(OrganisationName, model.CompanyName);
        Assert.AreEqual($"{SubBuildingName}, {BuildingNumber}, {BuildingName}, {Street}, {Town}, {County}, {Postcode}", model.OrganisationAddress);
        Assert.AreEqual(OrganisationType, model.OrganisationType);
        Assert.AreEqual(ServiceRoleKey, model.ServiceRoleKey);
    }

    [TestMethod]
    public async Task GivenOnEditUserDetailsPage_WhenRequested_TheShowUserDetails()
    {
        // Arrange
        var userData = new UserData()
        {
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            },
            RoleInOrganisation = PersonRole.Employee.ToString(),
            Telephone = "123456"
        };

        SetupBase(userData);

        // Act
        var result = await SystemUnderTest.EditUserDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(null);

        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        AutoMapperMock.Verify(m => m.Map<EditUserDetailsViewModel>(It.IsAny<UserData>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenTempDataHasSession_CheckYourOrganisationDetails_ShouldRemoveAnyTempData()
    {
        const string checkYourOrganisationDetailsKey = "CheckYourOrganisationDetails";
        // Arrange
        var checkYourOrganisationDetails = new CheckYourOrganisationDetailsViewModel
        {
            OrganisationId = Guid.NewGuid(),
            UkNation = UkNation.England,
        };

        var userData = SetupUserData(string.Empty);

        SetupBase(userData: userData);

        SystemUnderTest.TempData.Add(checkYourOrganisationDetailsKey, JsonSerializer.Serialize(checkYourOrganisationDetails));

        Assert.IsNotNull(SystemUnderTest.TempData[checkYourOrganisationDetailsKey]);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        Assert.IsNull(SystemUnderTest.TempData[checkYourOrganisationDetailsKey]);
    }


    [TestMethod]
    public async Task GivenOnEditUserDetailsPage_WhenPageSubbmitedWithValidData_RedirectsToNextPage()
    {
        // Arrange
        var editUserDetailsViewModel = new EditUserDetailsViewModel
        {
            FirstName = "FirstName",
            LastName = "LastName",
            OriginalJobTitle = "Job",
            JobTitle = "Job",
            OriginalTelephone = "Telephone",
            Telephone = "Telepohne"
        };

        // Act
        var result = await SystemUnderTest.EditUserDetails(editUserDetailsViewModel);

        // Assert
        var viewResult = result as RedirectToActionResult;
        Assert.IsNotNull(viewResult);
    }

    [TestMethod]
    [DataRow("JobTitle")]
    [DataRow("Telephone")]
    public async Task GivenOnEditUserDetailsPage_WhenPageSubbmitedWithInvalidData_ReturnsOriginalPage(string fieldValidationError)
    {
        // Arrange
        if (fieldValidationError == "JobTitle")
        {
            SystemUnderTest.ModelState.AddModelError("JobTitle", "JobTitle is missing");
        }
        else
        {
            SystemUnderTest.ModelState.AddModelError("Telephone", "JobTitle is missing");
        }

        var editUserDetailsViewModel = new EditUserDetailsViewModel
        {
            FirstName = "FirstName",
            LastName = "LastName",
            OriginalJobTitle = "Job",
            JobTitle = "Job",
            OriginalTelephone = "Telephone",
            Telephone = "Telepohne"
        };

        // Act
        var result = await SystemUnderTest.EditUserDetails(editUserDetailsViewModel) as ViewResult;

        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrWhiteSpace(result.ViewName));
        Assert.AreSame(editUserDetailsViewModel, result.Model);
    }

    [TestMethod]
    [DataRow("JobTitle")]
    [DataRow("Telephone")]
    public async Task GivenOnEditUserDetailsPage_WhenPageSubbmitedWithInvalidDataAndNoOriginalValues_RedirectsToNextPage(string fieldValidationError)
    {
        // Arrange
        if (fieldValidationError == "JobTitle")
        {
            SystemUnderTest.ModelState.AddModelError("JobTitle", "JobTitle is missing");
        }
        else
        {
            SystemUnderTest.ModelState.AddModelError("Telephone", "JobTitle is missing");
        }

        var editUserDetailsViewModel = new EditUserDetailsViewModel
        {
            FirstName = "FirstName",
            LastName = "LastName",
            JobTitle = "Job",
            Telephone = "Telepohne"
        };

        // Act
        var result = await SystemUnderTest.EditUserDetails(editUserDetailsViewModel);

        // assert
        var viewResult = result as RedirectToActionResult;
        Assert.IsNotNull(viewResult);
    }

    /// <summary>
    /// Test that if there are user details already stored in temp data (i.e. the user has entered new details, proceded to the next
    /// page, then clicked back to return to this one), the stored details are overwritten with whichever details
    /// they've changed this time.
    /// </summary>
    [TestMethod]
    public async Task EditUserDetailsPost_NewUserDetailsAlreadySet()
    {
        // Arrange
        var previousNewDetails = new EditUserDetailsViewModel
        {
            FirstName = "Previous first name",
            LastName = "Previous last name",
            JobTitle = "Previous job",
            Telephone = "Previous telephone number"
        };
        SystemUnderTest.TempData.Add("NewUserDetails", JsonSerializer.Serialize(previousNewDetails));

        var newNewDetails = new EditUserDetailsViewModel
        {
            FirstName = "New first name",
            LastName = "New last name",
            JobTitle = "New job",
            Telephone = "New telephone number"
        };

        // Check the initial state of the temp data is correct before we act.
        Assert.AreEqual(previousNewDetails, DeserialiseUserDetailsJson(SystemUnderTest.TempData["NewUserDetails"]));


        // Act
        await SystemUnderTest.EditUserDetails(newNewDetails);

        // Assert
        Assert.AreEqual(newNewDetails, DeserialiseUserDetailsJson(SystemUnderTest.TempData["NewUserDetails"]));
    }

    /// <summary>
    /// Parses the user details from the temp data back to an object.
    /// </summary>
    private EditUserDetailsViewModel DeserialiseUserDetailsJson(object json)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
        stream.Position = 0;
        return (EditUserDetailsViewModel)JsonSerializer.Deserialize(stream, typeof(EditUserDetailsViewModel));
    }

    [TestMethod]
    [DataRow("Approved Person")]
    [DataRow("Delegated Person")]
    public async Task GetCheckCompaniesHouseDetails_AllowedPerson_ReturnedPageAsExpected(string serviceRole)
    {
        // Arrange
        SetupUserData(serviceRole);

        var viewModel = new CheckYourOrganisationDetailsViewModel();

        TempDataDictionary["CheckYourOrganisationDetails"] =
            JsonSerializer.Serialize(viewModel);

        // Act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrWhiteSpace(result.ViewName));
        Assert.IsInstanceOfType<CheckYourOrganisationDetailsViewModel>(result.Model);
    }

    [TestMethod]
    public async Task GetCheckCompaniesHouseDetails_DisallowedPerson_ReturnedPageAsExpected()
    {
        // arrange
        SetupUserData(string.Empty);

        // act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails() as UnauthorizedResult;

        // assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [DataRow("Approved Person")]
    [DataRow("Delegated Person")]
    public async Task PostCheckCompaniesHouseDetails_ValidParameters_RequestsToSaveData(string serviceRole)
    {
        // Arrange
        var userData = SetupUserData(serviceRole);
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult(
                new JourneySession
                {
                    UserData = userData,
                    CompaniesHouseSession = new CompaniesHouseSession
                    {
                        CompaniesHouseData = new CompaniesHouseResponse
                        {
                            Organisation = new OrganisationDto
                            { }
                        }
                    }
                }));

        var organisationId = Guid.NewGuid();
        var viewModel = new CheckYourOrganisationDetailsViewModel
        {
            OrganisationId = organisationId,
            UkNation = UkNation.NorthernIreland
        };

        FacadeServiceMock.Setup(s => s.GetUserAccount()).ReturnsAsync(
            new UserAccountDto
            {
                User = new UserData()
            });


        // Act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails(viewModel) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);

        ClaimsExtensionsWrapperMock.Verify(s =>
            s.UpdateUserDataClaimsAndSignInAsync(
                It.IsAny<UserData>()),
            Times.Once);
        FacadeServiceMock.Verify(s =>
            s.UpdateOrganisationDetails(
                organisationId,
                It.IsAny<OrganisationUpdateDto>()),
            Times.Once);
    }

    [TestMethod]
    [DataRow("Approved Person")]
    [DataRow("Delegated Person")]
    public async Task PostCheckCompaniesHouseDetails_InvalidModelState_ReturnsOriginalView(string serviceRole)
    {
        // Arrange
        SetupUserData(serviceRole);

        SystemUnderTest.ModelState.AddModelError("Error", "Error");
        var viewModel = new CheckYourOrganisationDetailsViewModel();

        // Act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails(viewModel) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<CheckYourOrganisationDetailsViewModel>(result.Model);
    }

    [TestMethod]
    public async Task GivenOnCompanyDetailsHaveNotChangedPage_ForAnApprovedPerson_WhenRequested_ThenShowView()
    {
        // Arrange
        SetupUserData("Approved Person");

        // Act
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(null);

        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnCompanyDetailsHaveNotChangedPage_ForADelegatedPerson_WhenRequested_ThenShowView()
    {
        // Arrange
        SetupUserData("Delegated Person");

        // Act
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(null);

        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnCompanyDetailsHaveNotChangedPage_ForAnUnauthorisedPerson_WhenRequested_ReturnUnauthroised()
    {
        // Arrange
        SetupUserData("Unauthorised person");

        var companiesHouseData = new CompaniesHouseResponse();

        FacadeServiceMock.Setup(s => s.GetCompaniesHouseResponseAsync("123456")).ReturnsAsync(companiesHouseData);

        // Act
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task CompanyDetailsHaveNotChangedPage_ShouldDisplayPageNotFound_WhenUserIsBasicEmployee()
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
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task CompanyDetailsHaveNotChangedPage_ShouldDisplayPageNotFound_WhenUserIsBasicAdmin()
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
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task CheckCompaniesHouseDetails_ShouldDisplayPageNotFound_WhenUserIsBasicEmployee()
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
        var result = await SystemUnderTest.CheckCompaniesHouseDetails();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task CheckCompaniesHouseDetails_ShouldDisplayPageNotFound_WhenUserIsBasicAdmin()
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
        var result = await SystemUnderTest.CheckCompaniesHouseDetails();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task CheckCompaniesHouseDetails_ShouldThrowInvalidOperationException_WhenRoleInOrganisationIsNullOrEmpty()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = null,
        };

        Exception result = null;

        SetupBase(userData);

        // Act
        try
        {
            await SystemUnderTest.CheckCompaniesHouseDetails();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task CheckCompaniesHouseDetails_ShouldThrowInvalidOperationException_WhenServiceRoleIdIsDefault()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = default,
            RoleInOrganisation = Core.Enums.PersonRole.Admin.ToString(),
        };

        Exception result = null;

        SetupBase(userData);

        // Act
        try
        {
            await SystemUnderTest.CheckCompaniesHouseDetails();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task CompanyDetailsHaveNotChanged_ShouldThrowInvalidOperationException_WhenServiceRoleIdIsDefault()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = default,
            RoleInOrganisation = Core.Enums.PersonRole.Admin.ToString(),
        };

        Exception result = null;

        SetupBase(userData);

        // Act
        try
        {
            await SystemUnderTest.CompanyDetailsHaveNotChanged();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
        Assert.IsTrue(result.Message == "Unknown service role.");
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task CompanyDetailsHaveNotChanged_ShouldThrowInvalidOperationException_WhenRoleInOrganisationIsEmpty()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = string.Empty,
        };

        Exception result = null;

        SetupBase(userData);

        // Act
        try
        {
            await SystemUnderTest.CompanyDetailsHaveNotChanged();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
        Assert.IsTrue(result.Message == "Unknown role in organisation.");
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task CompanyDetailsHaveNotChanged_ShouldThrowInvalidOperationException_WhenRoleInOrganisationIsNull()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = null,
        };

        Exception result = null;

        SetupBase(userData);

        // Act
        try
        {
            await SystemUnderTest.CompanyDetailsHaveNotChanged();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
        Assert.IsTrue(result.Message == "Unknown role in organisation.");
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }


    private UserData SetupUserData(
        string serviceRole)
    {
        var userData = new UserData
        {
            FirstName = FirstName,
            LastName = LastName,
            JobTitle = JobTitle,
            Telephone = Telephone,
            ServiceRole = serviceRole,
            ServiceRoleId = ServiceRoleId,
            RoleInOrganisation = RoleInOrganisation,

            Organisations = new List<Organisation>
            {
                new Organisation
                {
                    Id = Guid.NewGuid(),
                    Name = OrganisationName,
                    OrganisationType = OrganisationType,
                    SubBuildingName = SubBuildingName,
                    BuildingNumber = BuildingNumber,
                    BuildingName = BuildingName,
                    Street = Street,
                    Town = Town,
                    County = County,
                    Postcode = Postcode,
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

        return userData;
    }

    [TestMethod]
    public async Task ChangeCompanyDetails_ReturnsViewResult()
    {
        // Arrange
        SetupBase();

        // Act
        var result = await SystemUnderTest.ChangeCompanyDetails();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public async Task CheckData_RedirectsToCompanyDetailsHaveNotChanged_WhenTheDataMatches()
    {
        // Arrange
        var mockUserData = new UserData
        {
            Organisations = new List<Organisation>
            {
                new Organisation
                {
                    Name = "Org Name",
                    TradingName = "Trading Name",
                    SubBuildingName = "Sub Building",
                    BuildingName = "Building Name",
                    BuildingNumber = "1",
                    Street = "Street",
                    Locality = "Locality",
                    DependentLocality = "Dependent Locality",
                    Town = "Town",
                    County = "County",
                    Country = "Country",
                    Postcode = "Postcode",
                    CompaniesHouseNumber = "12345678"
                }
            }
        };

        var companiesHouseResponse = new CompaniesHouseResponse
        {
            Organisation = new OrganisationDto
            {
                Name = "Org Name",
                TradingName = "Trading Name",
                RegisteredOffice = new AddressDto
                {
                    SubBuildingName = "Sub Building",
                    BuildingName = "Building Name",
                    BuildingNumber = "1",
                    Street = "Street",
                    Locality = "Locality",
                    DependentLocality = "Dependent Locality",
                    Town = "Town",
                    County = "County",
                    Country = new CountryDto { Name = "Country" },
                    Postcode = "Postcode"
                }
            }
        };

        SetupBase(mockUserData);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession { UserData = mockUserData });

        FacadeServiceMock.Setup(f => f.GetCompaniesHouseResponseAsync(It.IsAny<string?>()))
            .ReturnsAsync(companiesHouseResponse);

        // Act
        var result = await SystemUnderTest.CheckData() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(AccountManagementController.CompanyDetailsHaveNotChanged), result.ActionName);

        FacadeServiceMock.Verify(f => f.GetCompaniesHouseResponseAsync(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CheckData_RedirectsToConfirmCompanyDetails_WhenTheDataDoesNotMatch()
    {
        // Arrange
        var mockUserData = new UserData
        {
            Organisations = new List<Organisation>
            {
                new Organisation
                {
                    Name = "Org Name",
                    TradingName = "Trading Name",
                    SubBuildingName = "Sub Building",
                    BuildingName = "Building Name",
                    BuildingNumber = "1",
                    Street = "Street",
                    Locality = "Locality",
                    DependentLocality = "Dependent Locality",
                    Town = "Town",
                    County = "County",
                    Country = "Country",
                    Postcode = "Postcode",
                    CompaniesHouseNumber = "12345678"
                }
            }
        };

        var companiesHouseResponse = new CompaniesHouseResponse
        {
            Organisation = new OrganisationDto
            {
                Name = "Different Org Name", // Simulating mismatch
                TradingName = "Trading Name",
                RegisteredOffice = new AddressDto
                {
                    SubBuildingName = "Sub Building",
                    BuildingName = "Building Name",
                    BuildingNumber = "1",
                    Street = "Street",
                    Locality = "Locality",
                    DependentLocality = "Dependent Locality",
                    Town = "Town",
                    County = "County",
                    Country = new CountryDto { Name = "Country" },
                    Postcode = "Postcode"
                }
            }
        };

        SetupBase(mockUserData);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession { UserData = mockUserData });

        FacadeServiceMock.Setup(f => f.GetCompaniesHouseResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(companiesHouseResponse);

        // Act
        var result = await SystemUnderTest.CheckData() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(AccountManagementController.ConfirmCompanyDetails), result.ActionName);

        FacadeServiceMock.Verify(f => f.GetCompaniesHouseResponseAsync(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task CheckData_ThrowsInvalidOperationException_WhenThereIsNoOrganisationData()
    {
        // Arrange
        var mockUserData = new UserData
        {
            Organisations = new List<Organisation>()
        };

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession { UserData = mockUserData });

        SetupBase(mockUserData);

        // Act
        await SystemUnderTest.CheckData();

        FacadeServiceMock.Verify(f => f.GetCompaniesHouseResponseAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task EditUserDetails_ShouldReturnForbidden_WhenIsChangeRequestPendingIsTrue()
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
        var result = await SystemUnderTest.EditUserDetails();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task EditUserDetails_TempDataHasValidAmendedUserDetails_DeserializesAndReturnsViewWithModel()
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

        TempDataDictionary["AmendedUserDetails"] = serializedModel;

        AutoMapperMock.Setup(m =>
            m.Map<EditUserDetailsViewModel>(mockUserData))
            .Returns(expectedModel);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession
            {
                UserData = mockUserData
            });

        // Act
        var result = await SystemUnderTest.EditUserDetails();

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(expectedModel.FirstName, ((EditUserDetailsViewModel)viewResult.Model).FirstName);
    }

    [TestMethod]
    public async Task ConfirmDetailsOfTheCompany_ShouldRedirectToUkNation()
    {
        // Act
        var result = await SystemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        var redirectToActionResult = result as RedirectToActionResult;
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.IsNotNull(redirectToActionResult);
        Assert.AreEqual(nameof(SystemUnderTest.UkNation), redirectToActionResult.ActionName);
    }

    [TestMethod]
    public async Task UpdateDetailsConfirmation_ShouldReturnForbidden_WhenIsChangeRequestPendingIsTrue()
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

        SetupBase(mockUserData);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession
            {
                UserData = mockUserData
            });

        // Act
        var result = await SystemUnderTest.UpdateDetailsConfirmation();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }
}