namespace FrontendAccountManagement.Web.Sessions;

public interface ISessionManager<T> where T : class, new()
{
    Task<T?> GetSessionAsync(ISession session);

    Task SaveSessionAsync(ISession session, T sessionValue);

    void RemoveSession(ISession session);

    Task UpdateSessionAsync(ISession session, Action<T> updateFunc);
}
