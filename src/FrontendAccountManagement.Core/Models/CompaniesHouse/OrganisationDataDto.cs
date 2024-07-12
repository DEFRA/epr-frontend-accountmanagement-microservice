using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models.CompaniesHouse
{
    [ExcludeFromCodeCoverage]
    public class OrganisationDataDto
    {
        public DateTime DateOfCreation { get; set; }

        public string? Status { get; set; }

        public string? Type { get; set; }
    }
}
