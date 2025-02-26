using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FrontendAccountManagement.Web.Middleware;
using System.Text.Json;
using FluentAssertions.Execution;

namespace FrontendAccountManagement.Web.UnitTests.Middleware
{
    [TestClass]
    public class AuthorizationPolicyTests
    {
        private const string FirstName = "Test First Name";
        private const string LastName = "Test Last Name";
        private const string Telephone = "07545822431";

        [TestMethod]
        public async Task GivenUserAccessingProtectedAccountManagerControllerAction_WhenUserHasServiceRoleBasic_ShouldFail()
        {
            // data
            var mockUserData = new UserData
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>(),
                ServiceRoleId = 3
            };

            var requirements = new List<IAuthorizationRequirement>
                                    {
                                        new EmployeeOrBasicAdminRequirement( ServiceRole.Basic,
                                            PersonRole.Employee,
                                            PersonRole.Admin
                                            ),
                                    };

            var claims = new List<Claim>
            {
                new(ClaimTypes.UserData, JsonSerializer.Serialize(mockUserData))
            };

            var user = new ClaimsPrincipal(
                   new ClaimsIdentity(
                       claims,
                       "Basic")
                   );

            var context = new AuthorizationHandlerContext(requirements, user, null);

            // test
            var handler = new EmployeeOrBasicAdminHandler();
            await handler.HandleAsync(context);

            using (new AssertionScope())
            {
                context.Should().NotBeNull();

                Assert.IsFalse(context.HasSucceeded);
            }
        }


        [TestMethod]
        public async Task GivenUserAccessingProtectedAccountManagerControllerAction_WhenUserHasServiceRoleRegulatorAdmin_ShouldFail()
        {
            // data
            var mockUserData = new UserData
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>(),
                ServiceRoleId = 4
            };

            var requirements = new List<IAuthorizationRequirement>
                                    {
                                        new EmployeeOrBasicAdminRequirement( ServiceRole.RegulatorAdmin,
                                            PersonRole.Employee,
                                            PersonRole.Admin
                                            ),
                                    };

            var claims = new List<Claim>
            {
                new(ClaimTypes.UserData, JsonSerializer.Serialize(mockUserData))
            };

            var user = new ClaimsPrincipal(
                   new ClaimsIdentity(
                       claims,
                       "Basic")
                   );

            var context = new AuthorizationHandlerContext(requirements, user, null);

            // test
            var handler = new EmployeeOrBasicAdminHandler();
            await handler.HandleAsync(context);

            using (new AssertionScope())
            {
                context.Should().NotBeNull();

                Assert.IsFalse(context.HasSucceeded);
            }
        }

        [TestMethod]
        public async Task GivenUserAccessingProtectedAccountManagerControllerAction_WhenUserHasServiceRoleApproved_ShouldSucceed()
        {
            // data
            var mockUserData = new UserData
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>(),
                ServiceRoleId = 1
            };

            var requirements = new List<IAuthorizationRequirement>
                                    {
                                        new EmployeeOrBasicAdminRequirement( ServiceRole.Approved,
                                            PersonRole.Employee,
                                            PersonRole.Admin
                                            ),
                                    };

            var claims = new List<Claim>
            {
                new(ClaimTypes.UserData, JsonSerializer.Serialize(mockUserData))
            };

            var user = new ClaimsPrincipal(
                   new ClaimsIdentity(
                       claims,
                       "Basic")
                   );

            var context = new AuthorizationHandlerContext(requirements, user, null);

            // test
            var handler = new EmployeeOrBasicAdminHandler();
            await handler.HandleAsync(context);

            using (new AssertionScope())
            {
                context.Should().NotBeNull();

                Assert.IsTrue(context.HasSucceeded);
            }
        }

        [TestMethod]
        public async Task GivenUserAccessingProtectedAccountManagerControllerAction_WhenUserHasServiceRoleDelegated_ShouldSucceed()
        {
            // data
            var mockUserData = new UserData
            {
                FirstName = FirstName,
                LastName = LastName,
                Telephone = Telephone,
                IsChangeRequestPending = true,
                Organisations = new List<Organisation>(),
                ServiceRoleId = 2
            };

            var requirements = new List<IAuthorizationRequirement>
                                    {
                                        new EmployeeOrBasicAdminRequirement( ServiceRole.Delegated,
                                            PersonRole.Employee,
                                            PersonRole.Admin
                                            ),
                                    };

            var claims = new List<Claim>
            {
                new(ClaimTypes.UserData, JsonSerializer.Serialize(mockUserData))
            };

            var user = new ClaimsPrincipal(
                   new ClaimsIdentity(
                       claims,
                       "Basic")
                   );

            var context = new AuthorizationHandlerContext(requirements, user, null);

            // test
            var handler = new EmployeeOrBasicAdminHandler();
            await handler.HandleAsync(context);

            using (new AssertionScope())
            {
                context.Should().NotBeNull();

                Assert.IsTrue(context.HasSucceeded);
            }
        }
    }
}
