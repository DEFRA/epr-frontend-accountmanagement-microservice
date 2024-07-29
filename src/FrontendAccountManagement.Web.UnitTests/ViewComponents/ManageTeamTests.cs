using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.MockedData;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewComponents;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.ViewComponents;

[TestClass]
public class ManageTeamTests : ViewComponentsTestBase
{
    private readonly Guid _organisationId = Guid.NewGuid(); 
    private readonly Microsoft.Extensions.Logging.Abstractions.NullLogger<ManageTeamViewComponent> _logger = new ();
    private ManageTeamViewComponent _component;
    private UserData _userData;

    [TestInitialize]
    public void Setup()
    {
        _component = new ManageTeamViewComponent(FacadeService.Object, _logger, ViewComponentHttpContextAccessor.Object);
    }

    //[TestMethod]
    //[DataRow(1)]
    //public void Invoke_SetsModel_ForApprovedAdminUser_AndRemovableUsersAreSetCorrectly(int serviceRoleId)
    //{
    //    // Arrange
    //    _userData = new UserData
    //    {
    //        Organisations = new List<Organisation> {new() {Id = _organisationId}},
    //        RoleInOrganisation = "Admin",
    //        ServiceRoleId = serviceRoleId
    //    };        
    //    SetViewComponentContext(PagePath.ManageAccount, _component, _userData);
    //    FacadeService.Setup(x => x.GetUsersForOrganisationAsync(_organisationId.ToString(), _userData.ServiceRoleId))
    //        .ReturnsAsync(MockedManageUserModels.GetMockedManageUserModels());

    //    // Act
    //    var model = _component.InvokeAsync().Result.ViewData.Model as ManageTeamModel;
        
    //    // Assert
    //    Assert.IsTrue(model.Users.Any());
    //    Assert.AreEqual(false,
    //        model.Users.Find(x => x.Email == "peter@inviteduser.test").IsRemoveable);
    //    Assert.AreEqual(true, 
    //        model.Users.Find(x => x.Email == "chris@delegatedadmin.test").IsRemoveable);
    //    Assert.AreEqual(true, 
    //        model.Users.Find(x => x.Email == "donna@basicadmin.test").IsRemoveable);
    //    Assert.AreEqual(true, 
    //        model.Users.Find(x => x.Email == "albert@basicemployee.test").IsRemoveable);
    //}
    
    //[TestMethod]
    //[DataRow(2)]
    //[DataRow(3)]
    //public void Invoke_SetsModel_ForDelegatedOrBasicAdminUser_AndRemovableUsersAreSetCorrectly(int serviceRoleId)
    //{
    //    // Arrange
    //    _userData = new UserData
    //    {
    //        Organisations = new List<Organisation> {new() {Id = _organisationId}},
    //        RoleInOrganisation = "Admin",
    //        ServiceRoleId = serviceRoleId
    //    };        
    //    SetViewComponentContext(PagePath.ManageAccount, _component, _userData);
    //    FacadeService.Setup(x => x.GetUsersForOrganisationAsync(_organisationId.ToString(), _userData.ServiceRoleId))
    //        .ReturnsAsync(MockedManageUserModels.GetMockedManageUserModels());

    //    // Act
    //    var model = _component.InvokeAsync().Result.ViewData.Model as ManageTeamModel;
        
    //    // Assert
    //    Assert.IsTrue(model.Users.Any());
    //    Assert.AreEqual(false,
    //        model.Users.Find(x => x.Email == "peter@inviteduser.test").IsRemoveable);
    //    Assert.AreEqual(false, 
    //        model.Users.Find(x => x.Email == "chris@delegatedadmin.test").IsRemoveable);
    //    Assert.AreEqual(true, 
    //        model.Users.Find(x => x.Email == "donna@basicadmin.test").IsRemoveable);
    //    Assert.AreEqual(true, 
    //        model.Users.Find(x => x.Email == "albert@basicemployee.test").IsRemoveable);
    //}
}