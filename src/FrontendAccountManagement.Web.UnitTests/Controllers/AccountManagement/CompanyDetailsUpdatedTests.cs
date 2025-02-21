using System.Globalization;
using System.Threading.Tasks;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using FluentAssertions.Execution;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Sessions;
using AutoFixture;
using FrontendAccountManagement.Web.Controllers.Errors;
using System.Net;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class CompanyDetailsUpdatedTests : AccountManagementTestBase
    {
        private UserData _userData;
        private Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _userData = _fixture.Create<UserData>();
            _userData.Organisations[0].CompaniesHouseNumber = null;

            SetupBase(_userData);
        }

        [TestMethod]
        [DataRow("Bob", "McPlaceholder", "1066-02-03T10:53:52")]
        [DataRow("Sally", "FakeName", "3000-06-21T00:03:12")]
        [DataRow("Guy", "Incognito", "2014-09-11T17:42:30")]
        public async Task CompanyDetailsUpdatedGet(string firstName, string lastName, string changeDate)
        {
            // Arrange
            base.SetupBase(new UserData { FirstName = firstName, LastName = lastName});
            this.SystemUnderTest.TempData = new TempDataDictionary(new Mock<HttpContext>().Object, new Mock<ITempDataProvider>().Object);

            var parsedDate = DateTime.Parse(changeDate, CultureInfo.InvariantCulture);

            this.SystemUnderTest.TempData.Add("OrganisationDetailsUpdatedTime", parsedDate);

            // Act
            var result = await SystemUnderTest.CompanyDetailsUpdated();
            var viewResult = (ViewResult)result;
            var model = (CompanyDetailsUpdatedViewModel)viewResult.Model;

            // Assert

            Assert.AreEqual(nameof(AccountManagementController.CompanyDetailsUpdated), viewResult.ViewName);
            Assert.AreEqual($"{firstName} {lastName}", model.UserName);
            Assert.AreEqual(parsedDate.ToString("HH:mm"), model.ChangeDate.ToString("HH:mm"));
            Assert.AreEqual(parsedDate.ToString("dd MMMM yyyy"), model.ChangeDate.ToString("dd MMMM yyyy"));
        }


        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenCompanyDetailsUpdatedCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureName.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            // Act
            var result = await SystemUnderTest.CompanyDetailsUpdated();

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
