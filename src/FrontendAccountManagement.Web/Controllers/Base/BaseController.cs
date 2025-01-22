using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Sessions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrontendAccountManagement.Core.Sessions.Interfaces;

namespace FrontendAccountManagement.Web.Controllers.Base
{
    public class BaseController<T, TNested> : Controller
        where T : class  // JourneySession is the base class
        where TNested : class, IJourneySession // TNested is the type that implements IJourneySession
    {
        private readonly ISessionManager<JourneySession> _sessionManager;

        public BaseController(ISessionManager<JourneySession> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        // Set the back link dynamically based on journey type
        protected void SetBackLink(JourneySession session, string currentPagePath)
        {
            var journey = GetJourneyByType(session);
            ViewBag.BackLinkToDisplay = journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
        }

        // Save the session and redirect dynamically based on journey type
        protected async Task<RedirectToActionResult> SaveSessionAndRedirect(
            JourneySession session,
            string actionName,
            string currentPagePath,
            string? nextPagePath)
        {
            await SaveSessionAndJourney(session, currentPagePath, nextPagePath);
            return RedirectToAction(actionName);
        }

        // Save the session and journey dynamically based on journey type
        protected async Task SaveSessionAndJourney(JourneySession session, string sourcePagePath, string? destinationPagePath)
        {
            var journey = GetJourneyByType(session);
            ClearRestOfJourney(journey, sourcePagePath);
            journey.AddIfNotExists(destinationPagePath);
            await SaveSession(session);
        }

        // Save the journey session to the session store
        protected async Task SaveSession(JourneySession session)
        {
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        // Clear the rest of the journey list after the current page path
        protected void ClearRestOfJourney(List<string> journey, string currentPagePath)
        {
            var index = journey.IndexOf(currentPagePath);
            journey = journey.Take(index + 1).ToList();
        }

        // Get the appropriate journey list based on generic types
        private List<string> GetJourneyByType(JourneySession session)
        {
            // Retrieve the nested session of type TNested using reflection
            var nestedSession = typeof(T).GetProperty(typeof(TNested).Name)?.GetValue(session);
            if (nestedSession is TNested typedNestedSession && typedNestedSession is IJourneySession journeySession)
            {
                return journeySession.Journey;
            }

            throw new InvalidOperationException("No valid journey found in the session.");
        }
    }
}
