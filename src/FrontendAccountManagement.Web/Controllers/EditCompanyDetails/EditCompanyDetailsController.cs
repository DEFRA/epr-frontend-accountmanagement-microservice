using AutoMapper;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.Base;

namespace FrontendAccountManagement.Web.Controllers.EditCompanyDetails
{
    public class EditCompanyDetailsController : BaseController<EditCompanyDetailsSession>
    {
        private readonly ISessionManager<EditCompanyDetailsSession> _sessionManager;
        private readonly ILogger<EditCompanyDetailsController> _logger;
        private readonly IMapper _mapper;

        public EditCompanyDetailsController(
        ISessionManager<EditCompanyDetailsSession> sessionManager,
        ILogger<EditCompanyDetailsController> logger,
        IMapper mapper) : base(sessionManager)
        {
            _sessionManager = sessionManager;
            _logger = logger;
            _mapper = mapper;
        }
    }
}
