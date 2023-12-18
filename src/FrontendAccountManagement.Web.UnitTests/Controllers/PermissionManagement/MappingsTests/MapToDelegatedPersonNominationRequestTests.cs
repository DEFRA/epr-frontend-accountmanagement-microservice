using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Core.Sessions.Mappings;
using System;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement.MappingsTests
{
    [TestClass]
    public class MapToDelegatedPersonNominationRequestTests
    {
        [TestMethod]
        [DataRow(Core.Sessions.RelationshipWithOrganisation.Consultant, RelationshipType.Consultancy)]
        [DataRow(Core.Sessions.RelationshipWithOrganisation.ConsultantFromComplianceScheme, RelationshipType.ComplianceScheme)]
        [DataRow(Core.Sessions.RelationshipWithOrganisation.Employee, RelationshipType.Employment)]
        [DataRow(Core.Sessions.RelationshipWithOrganisation.SomethingElse, RelationshipType.Other)]
        public void WhenPermissionManagementSessionItemIsMapped_ThenAllPropertiesAreCorrectlySet(
            Core.Sessions.RelationshipWithOrganisation relationshipWithOrganisation, 
            RelationshipType expectedRelationshipType)
        {
            var sessionItem = new Core.Sessions.PermissionManagementSessionItem
            {
                Id = Guid.NewGuid(),
                AdditionalRelationshipInformation = "Additional info",
                Fullname = "Full Name",
                JobTitle = "Job Title",
                NameOfComplianceScheme = "Name of Compliance Scheme",
                NameOfConsultancy = "Name of Consultancy",
                NameOfOrganisation = "Name of Organisation",
                PermissionType = Core.Sessions.PermissionType.Admin,
                RelationshipWithOrganisation = relationshipWithOrganisation
            };

            var nominationRequest = PermissionSessionManagementSessionMappings.MapToDelegatedPersonNominationRequest(sessionItem);

            nominationRequest.Should().NotBeNull();
            nominationRequest.OtherRelationshipDescription.Should().Be(sessionItem.AdditionalRelationshipInformation);
            nominationRequest.NominatorDeclaration.Should().Be(sessionItem.Fullname);
            nominationRequest.JobTitle.Should().Be(sessionItem.JobTitle);
            nominationRequest.ComplianceSchemeName.Should().Be(sessionItem.NameOfComplianceScheme);
            nominationRequest.ConsultancyName.Should().Be(sessionItem.NameOfConsultancy);
            nominationRequest.OtherOrganisationName.Should().Be(sessionItem.NameOfOrganisation);
            nominationRequest.OtherRelationshipDescription.Should().Be(sessionItem.AdditionalRelationshipInformation);
            nominationRequest.RelationshipType.Should().Be(expectedRelationshipType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenNotSupportedRelationshipWithOrganisationIsUsed_ThenThrowArgumentException()
        {
            var sessionItem = new Core.Sessions.PermissionManagementSessionItem
            {
                Id = Guid.NewGuid(),
                AdditionalRelationshipInformation = "Additional info",
                Fullname = "Full Name",
                JobTitle = "Job Title",
                NameOfComplianceScheme = "Name of Compliance Scheme",
                NameOfConsultancy = "Name of Consultancy",
                NameOfOrganisation = "Name of Organisation",
                PermissionType = Core.Sessions.PermissionType.Admin,
                RelationshipWithOrganisation = RelationshipWithOrganisation.NotSet
            };

            var nominationRequest = PermissionSessionManagementSessionMappings.MapToDelegatedPersonNominationRequest(sessionItem);

            Assert.IsNotNull(nominationRequest);    
        }
    }
}
