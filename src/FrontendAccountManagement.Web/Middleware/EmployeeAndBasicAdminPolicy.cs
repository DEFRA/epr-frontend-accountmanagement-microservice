using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace FrontendAccountManagement.Web.Middleware
{
    /// <summary>
    /// if user is basic and either admin or employee, deny access
    /// </summary>
    public class EmployeeOrBasicAdminHandler : AuthorizationHandler<EmployeeOrBasicAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EmployeeOrBasicAdminRequirement requirement)
        {
            var userData = context.User.GetUserData();

            if (requirement.ServiceRoleId == ParseEnum<ServiceRole>(userData.ServiceRoleId.ToString())
                && (userData.ServiceRole == PersonRole.Admin.ToString() 
                || requirement.RoleInOrganisationEmployee == ParseEnum<PersonRole>(userData.RoleInOrganisation)))
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

    public class EmployeeOrBasicAdminRequirement :IAuthorizationRequirement
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
