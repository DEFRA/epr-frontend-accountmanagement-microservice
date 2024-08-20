using System.Globalization;
using System.Threading.Tasks;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class CompanyDetailsUpdatedTests : AccountManagementTestBase
    {
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
    }
}
