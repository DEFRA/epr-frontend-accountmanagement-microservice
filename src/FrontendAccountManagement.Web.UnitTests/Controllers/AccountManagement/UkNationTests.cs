﻿using AutoFixture;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants.Enums;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class UkNationTests : AccountManagementTestBase
{
    private UserData _userData;
    private CompaniesHouseResponse _companiesHouseResponse;
    private ConfirmCompanyDetailsViewModel _viewModel;
    private Mock<HttpContext>? _httpContextMock;

    private Fixture _fixture = new Fixture();

    private const string CheckYourOrganisationDetailsKey = "CheckYourOrganisationDetails";

    [TestInitialize]
    public void Setup()
    {
        _userData = _fixture.Create<UserData>();
        _companiesHouseResponse = _fixture.Create<CompaniesHouseResponse>();
        _viewModel = _fixture.Create<ConfirmCompanyDetailsViewModel>();
        _httpContextMock = new Mock<HttpContext>();

        SetupBase(_userData);

        FacadeServiceMock.Setup(x => x.GetCompaniesHouseResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(_companiesHouseResponse);

        AutoMapperMock.Setup(x => x.Map<ConfirmCompanyDetailsViewModel>(_companiesHouseResponse))
            .Returns(_viewModel);
    }


    [TestMethod]
    public async Task UkNation_ShouldReturnViewWithModel_WhenModelStateIsInvalid()
    {
        // Arrange
        var model = new UkNationViewModel { UkNation = null };
        SystemUnderTest.ModelState.AddModelError("UkNation", "Required");

        // Act
        var result = await SystemUnderTest.UkNation(model);

        // Assert
        var viewResult = result as ViewResult;

        Assert.AreEqual(model, viewResult.Model);
    }

    [TestMethod]
    public async Task UkNation_ShouldRedirectToUkNation_WhenModelStateIsValid()
    {
        // Arrange
        var model = new UkNationViewModel { UkNation = UkNation.England };
        var session = _fixture.Create<JourneySession>();

        session.CompaniesHouseSession = new CompaniesHouseSession
        {
            CompaniesHouseData = new CompaniesHouseResponse
            {
                Organisation = new OrganisationDto
                {
                    RegisteredOffice = _fixture.Create<AddressDto>()
                }
            }
        };

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        AutoMapperMock.Setup(m => m.Map<AddressViewModel>(It.IsAny<AddressDto>()))
            .Returns(_fixture.Create<AddressViewModel>());

        // Act
        var result = await SystemUnderTest.UkNation(model);

        // Assert
        var redirectToActionResult = result as RedirectToActionResult;
        Assert.AreEqual(nameof(SystemUnderTest.CheckCompaniesHouseDetails), redirectToActionResult.ActionName);
    }

    [TestMethod]
    public async Task UkNation_ShouldDisplayPageNotFound_WhenUserIsBasicEmployee()
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
        var result = await SystemUnderTest.UkNation();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_ShouldDisplayPageNotFound_WhenUserIsBasicAdmin()
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
        var result = await SystemUnderTest.UkNation();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_WhenTempDataHasValidData_ShouldReturnViewWithUkNation()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.RegulatorAdmin.ToString(),
            ServiceRoleId = 4,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        SetupBase(userData);

        var session = new JourneySession();
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var checkYourOrgDetails = new CheckYourOrganisationDetailsViewModel
        {
            UkNation = UkNation.England
        };

        TempDataDictionary[CheckYourOrganisationDetailsKey] = JsonSerializer.Serialize(checkYourOrgDetails);
        TempDataDictionary.Keep(CheckYourOrganisationDetailsKey);

        // Act
        var result = await SystemUnderTest.UkNation() as ViewResult;
        var viewModel = result?.Model as UkNationViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(UkNation.England, viewModel.UkNation);
    }
}