using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.ReEx
{
    [TestClass]
    public class TeamMemberPermissionsTests : ReExAccountManagementTestBase
    {
        [TestMethod]
        public async Task Get_TeamMemberPermissions_ReturnsCorrectViewWithModel()
        {
            // Arrange
            var userData = new UserData
            {
                Organisations = new List<Organisation>
                {
                    new Organisation
                    {
                        Id = OrganisationId,
                        Enrolments = new List<Enrolment>
                        {
                            new Enrolment { ServiceRoleKey = "Re-Ex.BasicUser" }
                        }
                    }
                }
            };

            SetupBase(userData);

            var serviceRoles = new List<ServiceRole>
            {
                new ServiceRole { Key = "Re-Ex.ApprovedPerson" },
                new ServiceRole { Key = "Re-Ex.StandardUser" },
                new ServiceRole { Key = "Re-Ex.BasicUser" }
            };

            FacadeServiceMock.Setup(f => f.GetAllServiceRolesAsync()).ReturnsAsync(serviceRoles);

            // Act
            var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as TeamMemberPermissionsViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(3, model.ServiceRoles.Count);
            Assert.IsTrue(model.IsStandardUser);
        }

        [TestMethod]
        public async Task Post_TeamMemberPermissions_ModelStateInvalid_ReturnsViewWithModel()
        {
            // Arrange
            SetupBase();
            SystemUnderTest.ModelState.AddModelError("SelectedUserRole", "Required");

            var serviceRoles = new List<ServiceRole>
            {
                new ServiceRole { Key = "Re-Ex.ApprovedPerson" },
                new ServiceRole { Key = "Re-Ex.StandardUser" },
                new ServiceRole { Key = "Re-Ex.BasicUser" }
            };

            FacadeServiceMock.Setup(f => f.GetAllServiceRolesAsync()).ReturnsAsync(serviceRoles);

            var model = new TeamMemberPermissionsViewModel();

            // Act
            var result = await SystemUnderTest.TeamMemberPermissions(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TeamMemberPermissions", result.ViewName);
            var returnedModel = result.Model as TeamMemberPermissionsViewModel;
            Assert.IsNotNull(returnedModel);
            Assert.AreEqual(3, returnedModel.ServiceRoles.Count);
        }

        [TestMethod]
        public async Task Post_TeamMemberPermissions_Valid_RedirectsToViewDetails()
        {
            // Arrange
            var userData = new UserData();
            SetupBase(userData);

            var model = new TeamMemberPermissionsViewModel
            {
                SelectedUserRole = "Re-Ex.StandardUser"
            };

            // Act
            var result = await SystemUnderTest.TeamMemberPermissions(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ViewDetails", result.ActionName);
        }
    }
}