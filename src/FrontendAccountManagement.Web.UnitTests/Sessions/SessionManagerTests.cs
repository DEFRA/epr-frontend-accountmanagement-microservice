namespace FrontendAccountManagement.Web.UnitTests.Sessions;

using System.Text;
using System.Text.Json;
using EPR.Common.Authorization.Sessions;
using FluentAssertions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class SessionManagerTests
{
    private const string OrganisationName = "TestCo";
    private readonly JourneySession _testSession = new() { AccountManagementSession = new AccountManagementSession { OrganisationName = OrganisationName } };
    private readonly string _sessionKey = nameof(JourneySession);

    private string _serializedTestSession;
    private byte[] _sessionBytes;

    private Mock<ISession> _sessionMock;
    private JourneySessionManager _sessionManager;

    [TestInitialize]
    public void Setup()
    {
        _serializedTestSession = JsonSerializer.Serialize(_testSession);
        _sessionBytes = Encoding.UTF8.GetBytes(_serializedTestSession);

        _sessionMock = new Mock<ISession>();
        _sessionManager = new SessionManager<JourneySession>();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.TryGetValue(_sessionKey, out _sessionBytes)).Returns(true);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once());

        session.AccountManagementSession.OrganisationName.Should().Be(_testSession.AccountManagementSession.OrganisationName);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.TryGetValue(_sessionKey, out It.Ref<byte[]>.IsAny), Times.Never);

        session.AccountManagementSession.OrganisationName.Should().Be(_testSession.AccountManagementSession.OrganisationName);
    }

    [TestMethod]
    public async Task GivenNewSession_WhenSaveSessionAsyncCalled_ThenSessionSavedInStoreAndMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        // Act
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Once);

        savedSession.Should().NotBeNull();
        savedSession.AccountManagementSession.OrganisationName.Should().Be(_testSession.AccountManagementSession.OrganisationName);
    }

    [TestMethod]
    public async Task GivenSessionKey_WhenRemoveSessionCalled_ThenSessionRemovedFromMemoryAndSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        _sessionManager.RemoveSession(_sessionMock.Object);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.Remove(_sessionKey), Times.Once);

        savedSession.Should().BeNull();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.AccountManagementSession.OrganisationName = OrganisationName);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Once);

        savedSession.Should().NotBeNull();
        savedSession.AccountManagementSession.OrganisationName.Should().Be(OrganisationName);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.AccountManagementSession.OrganisationName = OrganisationName);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Exactly(2));

        savedSession.Should().NotBeNull();
        savedSession.AccountManagementSession.OrganisationName.Should().Be(OrganisationName);
    }
}
