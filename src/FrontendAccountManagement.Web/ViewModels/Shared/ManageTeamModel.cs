using FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.Shared
{
    [ExcludeFromCodeCoverage]
    public class ManageTeamModel
    {
        public List<ManageUserModel> Users { get; set; }
    }
}