using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Sessions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendAccountManagement.Web.Controllers.Base
{
    public class BaseController<TSession> : Controller where TSession : class
    {
        private readonly ISessionManager<TSession> _sessionManager;

        public BaseController(ISessionManager<TSession> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        protected void SetBackLink(TSession session, string currentPagePath)
        {
            if (session is IJourneySession journeySession)
            {
                ViewBag.BackLinkToDisplay = journeySession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
            }
        }

        protected async Task<RedirectToActionResult> SaveSessionAndRedirect(
            TSession session,
            string actionName,
            string currentPagePath,
            string? nextPagePath)
        {
            await SaveSessionAndJourney(session, currentPagePath, nextPagePath);
            return RedirectToAction(actionName);
        }

        protected async Task SaveSessionAndJourney(TSession session, string sourcePagePath, string? destinationPagePath)
        {
            if (session is IJourneySession journeySession)
            {
                ClearRestOfJourney(journeySession, sourcePagePath);
                journeySession.Journey.AddIfNotExists(destinationPagePath);
                await SaveSession(session);
            }
        }

        protected async Task SaveSession(TSession session)
        {
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        protected void ClearRestOfJourney(IJourneySession journeySession, string currentPagePath)
        {
            var index = journeySession.Journey.IndexOf(currentPagePath);
            journeySession.Journey = journeySession.Journey.Take(index + 1).ToList();
        }
    }
}
