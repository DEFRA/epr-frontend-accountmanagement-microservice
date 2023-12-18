using FrontendAccountManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class UpdatePersonRoleRequest
    {
        [Required]
        public PersonRole PersonRole { get; set; }
    }
}