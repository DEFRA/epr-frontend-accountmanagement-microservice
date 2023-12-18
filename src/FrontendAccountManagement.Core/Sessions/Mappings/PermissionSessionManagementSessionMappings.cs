using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Core.Sessions.Mappings
{
    public static class PermissionSessionManagementSessionMappings
    {
        public static DelegatedPersonNominationRequest MapToDelegatedPersonNominationRequest(PermissionManagementSessionItem currentSessionItem)
        {
            return new DelegatedPersonNominationRequest
            {
                ComplianceSchemeName = currentSessionItem.NameOfComplianceScheme,
                ConsultancyName = currentSessionItem.NameOfConsultancy,
                JobTitle = currentSessionItem.JobTitle,
                NominatorDeclaration = currentSessionItem.Fullname,
                OtherOrganisationName = currentSessionItem.NameOfOrganisation,
                OtherRelationshipDescription = currentSessionItem.AdditionalRelationshipInformation,
                RelationshipType = currentSessionItem.RelationshipWithOrganisation switch
                {
                    RelationshipWithOrganisation.Employee => RelationshipType.Employment,
                    RelationshipWithOrganisation.Consultant => RelationshipType.Consultancy,
                    RelationshipWithOrganisation.ConsultantFromComplianceScheme => RelationshipType.ComplianceScheme,
                    RelationshipWithOrganisation.SomethingElse => RelationshipType.Other,
                    _ => throw new ArgumentException("Not supported RelationshipWithOrganisation")
                }
            };
        }
    }
}
