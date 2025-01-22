using AutoMapper;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.EditCompanyDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.EditCompanyDetails
{
    public abstract class EditCompanyDetailsTestBase
    {
        protected Mock<ISessionManager<EditCompanyDetailsSession>> SessionManagerMock = null!;
        protected Mock<ILogger<EditCompanyDetailsController>> LoggerMock = null!;
        protected Mock<IMapper> AutoMapperMock;
        protected EditCompanyDetailsController EditCompanyDetailsControllerTest;

        protected void SetupBase(EditCompanyDetailsSession editCompanyDetailsSession = null)
        {
            SessionManagerMock = new Mock<ISessionManager<EditCompanyDetailsSession>>();
            AutoMapperMock = new Mock<IMapper>();

            editCompanyDetailsSession ??= new EditCompanyDetailsSession();

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(editCompanyDetailsSession));

            LoggerMock = new Mock<ILogger<EditCompanyDetailsController>>();

            EditCompanyDetailsControllerTest = new EditCompanyDetailsController(
                SessionManagerMock.Object,
                LoggerMock.Object,
                AutoMapperMock.Object);
        }
    }
}
