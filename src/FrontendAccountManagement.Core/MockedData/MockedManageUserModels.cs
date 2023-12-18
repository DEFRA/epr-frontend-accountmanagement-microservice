using FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.MockedData
{
    [ExcludeFromCodeCoverage]
    public static class MockedManageUserModels
    {
        public static IEnumerable<ManageUserModel> GetMockedManageUserModels()
        {
            return new List<ManageUserModel>
            {
                new()
                {
                    FirstName = "Angela",
                    LastName = "Test",
                    Email = "angela@approvedadmin.test",
                    PersonId = Guid.NewGuid(),
                    PersonRoleId = 1,
                    ServiceRoleId = 3,
                    ServiceRoleKey = "Approved.Admin",
                    EnrolmentStatus = Enums.EnrolmentStatus.Enrolled,
                },
                new()
                {
                    FirstName = "Chris",
                    LastName = "Test",
                    Email = "chris@delegatedadmin.test",
                    PersonId = Guid.NewGuid(),
                    PersonRoleId = 1,
                    ServiceRoleId = 2,
                    ServiceRoleKey = "Delegated.Admin",
                    EnrolmentStatus = Enums.EnrolmentStatus.Enrolled,
                },
                new()
                {
                    FirstName = "Donna",
                    LastName = "Test",
                    Email = "donna@basicadmin.test",
                    PersonId = Guid.NewGuid(),
                    PersonRoleId = 1,
                    ServiceRoleId = 3,
                    ServiceRoleKey = "Basic.Admin",
                    EnrolmentStatus = Enums.EnrolmentStatus.Enrolled,
                },
                new()
                {
                    FirstName = "Albert",
                    LastName = "Test",
                    Email = "albert@basicemployee.test",
                    PersonId = Guid.NewGuid(),
                    PersonRoleId = 2,
                    ServiceRoleId = 3,
                    ServiceRoleKey = "Basic.Employee",
                    EnrolmentStatus = Enums.EnrolmentStatus.Enrolled,
                },
                new()
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = "peter@inviteduser.test",
                    PersonId = Guid.NewGuid(),
                    PersonRoleId = 2,
                    ServiceRoleId = 3,
                    ServiceRoleKey = "Basic.Employee",
                    EnrolmentStatus = Enums.EnrolmentStatus.NotSet,
                }
            };
        }
    }
}