using AutoMapper;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.Base;

namespace FrontendAccountManagement.Web.Controllers.EditCompanyDetails
{
    public class EditCompanyDetailsController : BaseController<JourneySession, EditCompanyDetailsSession>
    {
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly ILogger<EditCompanyDetailsController> _logger;
        private readonly IMapper _mapper;

        public EditCompanyDetailsController(
        ISessionManager<JourneySession> sessionManager,
        ILogger<EditCompanyDetailsController> logger,
        IMapper mapper) : base(sessionManager)
        {
            _sessionManager = sessionManager;
            _logger = logger;
            _mapper = mapper;
        }
    }
}
