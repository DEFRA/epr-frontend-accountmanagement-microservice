using FrontendAccountManagement.Core.Constants;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.Json;
using EPR.Common.Authorization.Models;
using FluentAssertions;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using System.Net.Http.Json;
using FrontendAccountManagement.Core.Configuration;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Core.UnitTests.Services
{

    [TestClass]
    public class FacadeServiceTests
    {
        private Mock<HttpMessageHandler> _mockHandler = null!;
        private Mock<ITokenAcquisition> _tokenAcquisitionMock = null!;
        private HttpClient _httpClient = null!;
        private FacadeService _facadeService = null!;
        private Mock<IOptions<FacadeApiConfiguration>> _configuration;

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://example")
            };

            _httpClient.DefaultRequestHeaders.Add("X-EPR-Organisation", "Test");

            var inMemorySettings = new Dictionary<string, string> {
                {"TopLevelKey", "TopLevelValue"},
                {"SectionName:SomeKey", "SectionValue"},
                {"FacadeAPI:Address", "http://example/" },
                {"FacadeAPI:GetServiceRolesPath", "roles" },
                {"FacadeAPI:GetUserAccountPath", "user-accounts" },
				{"FacadeAPI:GetUserAccountV1Path", "v1/user-accounts" },
				{"FacadeAPI:DownStreamScope", "https://eprb2cdev.onmicrosoft.com/account-creation-facade/account-creation" }
            };

            _configuration = new Mock<IOptions<FacadeApiConfiguration>>();

            _configuration.Setup(c => c.Value).Returns(new FacadeApiConfiguration
            {

            });

            _facadeService = new FacadeService(
                _httpClient,
                _tokenAcquisitionMock.Object,
                _configuration.Object);
        }

        [TestMethod]
        public async Task CalledGetAllServiceRoles_WithAValidResult_ShouldReturnSingleServiceRoleAndRequestIsSuccessful()
        {
            // Arrange            
            var expectedResponse = new List<FrontendAccountManagement.Core.Models.ServiceRole>();
            var serviceRole = new FrontendAccountManagement.Core.Models.ServiceRole
            {
                PersonRoleId = 2,
                ServiceRoleId = 3,
                Key = "Testing",
                DescriptionKey = "This is a test"
            };
            expectedResponse.Add(serviceRole);

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetAllServiceRolesAsync();

            // Assert
            Assert.IsNotNull(response);
            response.Count().Should().Be(1);
            response.First().PersonRoleId.Should().Be(serviceRole.PersonRoleId);
            response.First().ServiceRoleId.Should().Be(serviceRole.ServiceRoleId);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Get
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();

        }

        [TestMethod]
        public async Task CalledGetAllServiceRoles_WithAnInvalidResult_ShouldReturnEmptyServiceRoleAndRequestIsSuccessful()
        {
            // Arrange            
            var expectedResponse = new List<FrontendAccountManagement.Core.Models.ServiceRole>();
            var serviceRole = new FrontendAccountManagement.Core.Models.ServiceRole();

            expectedResponse.Add(serviceRole);

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetAllServiceRolesAsync();

            // Assert
            Assert.IsNotNull(response);
            response.Count().Should().Be(1);
            response.First().PersonRoleId.Should().Be(0);
            response.First().ServiceRoleId.Should().Be(0);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Get &&
                    !req.RequestUri.AbsolutePath.Contains("check-organisation/TestNation")
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CalledSendUserInvite_WithAValidRequest_ShouldReturnASuccessResult()
        {
            // Arrange
            var invitedUser = new InvitedUser();

            var invitingUser = new InvitingUser();

            var inviteRequest = new InviteUserRequest
            {
                InvitedUser = invitedUser,
                InvitingUser = invitingUser
            };

            var expectedResponse = 0;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.SendUserInvite(inviteRequest);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response == EndpointResponseStatus.Success);

            httpTestHandler.Dispose();

        }

        [TestMethod]
        public async Task CalledSendUserInvite_WithErrorResponse_ShouldThrowException()
        {
            // Arrange
            var invitedUser = new InvitedUser();

            var invitingUser = new InvitingUser();

            var inviteRequest = new InviteUserRequest
            {
                InvitedUser = invitedUser,
                InvitingUser = invitingUser
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("InternalServerError"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            Assert.ThrowsExceptionAsync<Exception>(async () => await _facadeService.SendUserInvite(inviteRequest));

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CalledSendUserInvite_WithAnInValidRequest_ShouldReturnAUserAlreadyExistsResult()
        {
            // Arrange
            var invitedUser = new InvitedUser();

            var invitingUser = new InvitingUser();

            var inviteRequest = new InviteUserRequest
            {
                InvitedUser = invitedUser,
                InvitingUser = invitingUser
            };

            var expectedResponse = 2;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.SendUserInvite(inviteRequest);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response == EndpointResponseStatus.UserExists);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CalledGetPersonDetailsFromConnectionAsync_WithAValidRequest_ShouldReturnASuccessfulResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var serviceKey = "testing";

            var expectedResponse = new ConnectionPerson
            {
                FirstName = "An",
                LastName = "Other"
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.FirstName == expectedResponse.FirstName);
            Assert.IsTrue(response.LastName == expectedResponse.LastName);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task CalledGetPersonDetailsFromConnectionAsync_WithAnInValidRequest_ShouldReturnAnErrorResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var serviceKey = "testing";

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = null,
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNull(response);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(System.Text.Json.JsonException))]
        public async Task CalledGetPersonDetailsFromConnectionAsync_WithAnInValidRequest_ShouldReturnASuccessfulResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var serviceKey = "testing";

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = null,
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNull(response);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetUserAccount_IsSuccessful()
        {
            // Arrange
            var firstName = "First";
            var lastName = "Last";
            var expectedResponse = new UserAccountDto
            {
                User = new UserData
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName
                }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetUserAccount();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(firstName, response.User.FirstName);
            Assert.AreEqual(lastName, response.User.LastName);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetUserAccount_IsUnsuccessful()
        {
            // Arrange
            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetUserAccount();

            // Assert
            Assert.IsNull(response);
            httpTestHandler.Dispose();
        }


		[TestMethod]
		public async Task GetUserAccountWithEnrolmenmts_IsSuccessful()
		{
			// Arrange
			var firstName = "First";
			var lastName = "Last";
			var expectedResponse = new UserAccountDto
			{
				User = new UserData
				{
					Id = Guid.NewGuid(),
					FirstName = firstName,
					LastName = lastName,
                    Organisations =
					[
						new Organisation
                        {
                            Id = Guid.NewGuid(),
                            Name = "Test Organisation",
                            Enrolments =
							[
								new Enrolment
                                {
                                    ServiceRoleKey = ServiceRoles.ReprocessorExporter.ApprovedPerson
                                }
							]
						}
					]
				}
			};

			var httpTestHandler = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
			};

			_mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(httpTestHandler);

			// Act
			var response = await _facadeService.GetUserAccountWithEnrolments(It.IsAny<string>());

			// Assert
			Assert.IsNotNull(response);
			Assert.AreEqual(firstName, response.User.FirstName);
			Assert.AreEqual(lastName, response.User.LastName);
			Assert.AreEqual(expectedResponse.User.Organisations.Count, response.User.Organisations.Count);
            Assert.AreEqual(expectedResponse.User.Organisations[0].Enrolments.Count, response.User.Organisations[0].Enrolments.Count);
			httpTestHandler.Dispose();
		}

		[TestMethod]
		public async Task GetUserAccountWithEnrolments_IsUnsuccessful()
		{
			// Arrange
			var httpTestHandler = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.NotFound
			};

			_mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(httpTestHandler);

			// Act
			var response = await _facadeService.GetUserAccountWithEnrolments(It.IsAny<string>());

			// Assert
			Assert.IsNull(response);
			httpTestHandler.Dispose();
		}

		[TestMethod]
        public async Task GetPermissionTypeFromConnection_WithValidRequest_IsSuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Admin;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = EnrolmentStatus.NotSet
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            var result = ((PermissionType, Guid))response;
            Assert.AreEqual(expected: PermissionType.Admin, actual: result!.Item1);
            Assert.AreEqual(expected: userId, actual: result!.Item2);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetPermissionTypeFromConnection_WithValidRequestAndWithRoleEmployee_IsSuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Employee;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = EnrolmentStatus.NotSet
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            var result = ((PermissionType, Guid))response;
            Assert.AreEqual(expected: PermissionType.Basic, actual: result!.Item1);
            Assert.AreEqual(expected: userId, actual: result!.Item2);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetPermissionTypeFromConnection_WithInvitedStatus_IsUnsuccessfulReturnsNulls()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Admin;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = EnrolmentStatus.Invited
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNull(response.PermissionType);
            Assert.IsNull(response.UserId);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetPermissionTypeFromConnection_WithNoEnrolments_IsUnsuccessfulReturnsNulls()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Admin;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>()
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNull(response.PermissionType);
            Assert.IsNull(response.UserId);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetPermissionTypeFromConnection_WithValidRequestButNotFound_IsUnsuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Admin;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = EnrolmentStatus.Invited
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNull(response.PermissionType);
            Assert.IsNull(response.UserId);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetPermissionTypeFromConnection_WithValidRequestAndApprovedPerson_IsSuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Admin;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = ServiceRoles.Packaging.ApprovedPerson,
                            EnrolmentStatus = EnrolmentStatus.NotSet
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            var result = ((PermissionType, Guid))response;
            Assert.AreEqual(expected: PermissionType.Approved, actual: result!.Item1);
            Assert.AreEqual(expected: userId, actual: result!.Item2);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetPermissionTypeFromConnection_WithValidRequestAndDelegatedPerson_IsSuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var role = PersonRole.Admin;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = role,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = ServiceRoles.Packaging.DelegatedPerson,
                            EnrolmentStatus = EnrolmentStatus.NotSet
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId, connectionId, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            var result = ((PermissionType, Guid))response;
            Assert.AreEqual(expected: PermissionType.Delegated, actual: result!.Item1);
            Assert.AreEqual(expected: userId, actual: result!.Item2);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetEnrolmentStatus_WithValidRequest_IsSuccessfulReturnsEnrolmentStatus()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var enrolmentStatus = EnrolmentStatus.Invited;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = PersonRole.Admin,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = enrolmentStatus
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetEnrolmentStatus(organisationId, connectionId, serviceKey, serviceKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: enrolmentStatus, actual: response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetEnrolmentStatus_WithValidRequest_IsUnsuccessfulNotFoundReturnsNull()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var enrolmentStatus = EnrolmentStatus.Invited;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = PersonRole.Admin,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = enrolmentStatus
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetEnrolmentStatus(organisationId, connectionId, serviceKey, serviceKey);

            // Assert
            Assert.IsNull(response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetEnrolmentStatus_WithConnectionWithEnrolmentsNull_IsSuccessfulReturnsNull()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetEnrolmentStatus(organisationId, connectionId, serviceKey, ServiceRoles.Packaging.DelegatedPerson);

            // Assert
            Assert.IsNull(response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetEnrolmentStatus_WithEnrolmentsNull_IsSuccessfulReturnsNull()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = PersonRole.Admin,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection> { }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetEnrolmentStatus(organisationId, connectionId, serviceKey, serviceKey);

            // Assert
            Assert.IsNull(response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetEnrolmentStatus_WithServiceKeyNotPresentInEnrolments_IsSuccessfulReturnsNull()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var enrolmentStatus = EnrolmentStatus.Invited;

            var expectedResponse = new ConnectionWithEnrolments
            {
                PersonRole = PersonRole.Admin,
                UserId = userId,
                Enrolments = new Collection<EnrolmentsFromConnection>
                    {
                        new EnrolmentsFromConnection
                        {
                            ServiceRoleKey = serviceKey,
                            EnrolmentStatus = enrolmentStatus
                        }
                    }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.GetEnrolmentStatus(organisationId, connectionId, serviceKey, ServiceRoles.Packaging.DelegatedPerson);

            // Assert
            Assert.IsNull(response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetUsersForOrganisationAsync_WithValidRequest_IsSuccessfulReturnsVoid()
        {
            // Arrange
            var organisationId = Guid.NewGuid().ToString();
            var serviceRoleId = 2;

            var expectedResponse = new List<ManageUserModel>
            {
                new ManageUserModel
                {
                    FirstName ="f1",
                    LastName ="s1",
                    PersonId = Guid.NewGuid    (),
                    PersonRoleId =1,
                    ServiceRoleId =2,
                    EnrolmentStatus = EnrolmentStatus.NotSet,
                    IsRemoveable = false,
                    ConnectionId= Guid.NewGuid()
                },
                new ManageUserModel
                {
                    FirstName ="f2",
                    LastName ="s2",
                    PersonId = Guid.NewGuid    (),
                    PersonRoleId =3,
                    ServiceRoleId =0,
                    EnrolmentStatus = EnrolmentStatus.NotSet,
                    IsRemoveable = false,
                    ConnectionId= Guid.NewGuid()
                }
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            _facadeService.GetUsersForOrganisationAsync(organisationId, serviceRoleId);

            // Assert            
            Assert.IsNotNull(expectedResponse);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task RemoveUserForOrganisation_WithValidRequest_IsSuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var serviceRoleId = 2;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.RemoveUserForOrganisation(organisationId, userId, serviceRoleId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: EndpointResponseStatus.Success, response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task RemoveUserForOrganisation_WithValidRequest_IsUnsuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var serviceRoleId = 2;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.RemoveUserForOrganisation(organisationId, userId, serviceRoleId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: EndpointResponseStatus.Fail, response);
            httpTestHandler.Dispose();
        }
        
        [TestMethod]
        public async Task DeletePersonConnectionAndEnrolment_WithValidRequest_IsSuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var enrolementId = 2;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.DeletePersonConnectionAndEnrolment(organisationId, userId, enrolementId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: EndpointResponseStatus.Success, response);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task DeletePersonConnectionAndEnrolment_WithValidRequest_IsUnsuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var enrolementId = 2;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.DeletePersonConnectionAndEnrolment(organisationId, userId, enrolementId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: EndpointResponseStatus.Fail, response);
            httpTestHandler.Dispose();
        }
        
        [TestMethod]
        public async Task UpdatePersonRoleAdminOrEmployee_WithValidRequest_IsUnsuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var personRole = PersonRole.Admin;
            var connectionId = Guid.NewGuid();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            _facadeService.UpdatePersonRoleAdminOrEmployee(connectionId, personRole, organisationId, serviceKey);

            // Assert
            Assert.IsNotNull(connectionId);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task NominateToDelegatedPerson_WithValidRequest_IsUnsuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var serviceKey = ServiceRoles.Packaging.BasicUser;
            var connectionId = Guid.NewGuid();
            var delegated = new DelegatedPersonNominationRequest
            {
                RelationshipType = RelationshipType.NotSet
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            _facadeService.NominateToDelegatedPerson(connectionId, organisationId, serviceKey, delegated);

            // Assert
            Assert.IsNotNull(connectionId);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetNationId_WithValidRequest_IsUnsuccessfulReturnsResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var expectedResponse = new List<int> { 2 };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = _facadeService.GetNationIds(organisationId);

            // Assert
            Assert.IsNotNull(response);
            response.Result.Should().BeEquivalentTo(expectedResponse);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetNationId_WithValidRequest_IsUnsuccessful_ReturnsZero()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var expectedResponse = new List<int> { 0 };
            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = _facadeService.GetNationIds(organisationId);

            // Assert
            Assert.IsNotNull(response);
            response.Result.Should().BeEquivalentTo(expectedResponse);
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task UpdateNationIdByOrganisationId_CallsEndPoint()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var organisation = new OrganisationUpdateDto();

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse).Verifiable();

            // Act
            await _facadeService.UpdateOrganisationDetails(
                organisationId,
                organisation);

            // Assert
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [TestMethod]
        public async Task GetCompaniesHouseResponseAsync_NoContent_ReturnsNull()
        {
            // Arrange
            var companyHouseNumber = "12345678";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _facadeService.GetCompaniesHouseResponseAsync(companyHouseNumber);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetCompaniesHouseResponseAsync_Success_ReturnsCompaniesHouseResponse()
        {
            // Arrange
            var companyHouseNumber = "12345678";
            var expectedResponse = new CompaniesHouseResponse
            {
                Organisation = new OrganisationDto
                {
                    Name = "Test company name"
                }
            };
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _facadeService.GetCompaniesHouseResponseAsync(companyHouseNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test company name", result.Organisation.Name);
        }

        [TestMethod]
        public async Task GetCompaniesHouseResponseAsync_Error_ThrowsException()
        {
            // Arrange
            var companyHouseNumber = "12345678";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _facadeService.GetCompaniesHouseResponseAsync(companyHouseNumber));
        }
    }
}