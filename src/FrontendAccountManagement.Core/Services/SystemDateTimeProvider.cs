

namespace FrontendAccountManagement.Core.Services;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}