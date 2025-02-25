using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Services.Dto.CompaniesHouse
{
    [ExcludeFromCodeCoverage]
    public record Country
    {
        public string? Name { get; init; }

        public string? Iso { get; init; }
    }
}