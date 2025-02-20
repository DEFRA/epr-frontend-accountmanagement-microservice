using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace FrontendAccountManagement.Web.Middleware
{
    /// <summary>
    /// if user is either (basic - service role and admin - person role) or (employee user - person role), deny access
    /// 
    /// ie: if user is not (basic - service role or admin - person role)  or  (not employee user - person role), allow access
    /// </summary>
    public sealed class EmployeeOrBasicAdminHandler : AuthorizationHandler<EmployeeOrBasicAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EmployeeOrBasicAdminRequirement requirement)
        {
            var userData = context.User.GetUserData();

            var userServiceRoleId = ParseEnum<ServiceRole>(userData.ServiceRoleId.ToString());

            if ((ServiceRole.Approved == userServiceRoleId 
                || (ServiceRole.Delegated == userServiceRoleId)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }

    public sealed class EmployeeOrBasicAdminRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// parm holder for build wire-up
        /// </summary>
        /// <param name="serviceRoleId" cref="ServiceRole">Basic</param>
        /// <param name="roleInOrganisationEmployee" cref="PersonRole">Employee</param>
        /// <param name="roleInOrganisationAdmin" cref="PersonRole">Admin</param>
        public EmployeeOrBasicAdminRequirement(
            ServiceRole serviceRoleId,
            PersonRole roleInOrganisationEmployee, 
            PersonRole roleInOrganisationAdmin
            )
        {
            RoleInOrganisationEmployee = roleInOrganisationEmployee;
            RoleInOrganisationAdmin = roleInOrganisationAdmin;
            ServiceRoleId = serviceRoleId;
        }

        public PersonRole RoleInOrganisationEmployee { get; }
        public PersonRole RoleInOrganisationAdmin { get; }
        public ServiceRole ServiceRoleId { get; }
     }
}
