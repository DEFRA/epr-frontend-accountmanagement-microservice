using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class DelegatedPersonNominationRequest
    {
        /// <summary>
        /// Relationship of the Nominee with the organisation.
        /// </summary>
        public RelationshipType RelationshipType { get; set; }

        /// <summary>
        /// JobTitle for RelationshipType = Employment
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Organisation name for RelationshipType = Consultancy.
        /// </summary>
        public string? ConsultancyName { get; set; }

        /// <summary>
        /// Organisation name for RelationshipType = ComplianceScheme.
        /// </summary>
        public string? ComplianceSchemeName { get; set; }

        /// <summary>
        /// Organisation name for RelationshipType = Other.
        /// </summary>
        public string? OtherOrganisationName { get; set; }

        /// <summary>
        /// Description explaining the relationship with the organisation when RelationshipType = Other.
        /// </summary>
        public string? OtherRelationshipDescription { get; set; }

        public string? NominatorDeclaration { get; set; }
    }
}
