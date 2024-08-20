
namespace FrontendAccountManagement.Core.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}