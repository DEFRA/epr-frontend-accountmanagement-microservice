using System;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models.CompaniesHouse
{
    [ExcludeFromCodeCoverage]
    public class CompaniesHouseResponse
    {
        public OrganisationDto? Organisation { get; set; }

        public bool AccountExists => AccountCreatedOn is not null;

        public DateTimeOffset? AccountCreatedOn { get; set; }
    }
}
