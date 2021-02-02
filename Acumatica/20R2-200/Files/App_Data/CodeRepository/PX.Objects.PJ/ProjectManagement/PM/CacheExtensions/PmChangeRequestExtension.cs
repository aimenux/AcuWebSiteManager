using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectManagement.PM.CacheExtensions
{
    public sealed class PmChangeRequestExtension : PXCacheExtension<PMChangeRequest>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();

		}

		public abstract class convertedFrom : BqlString.Field<convertedFrom> { }
	
		[PXDBString(50)]
        public string ConvertedFrom
        {
            get;
            set;
        }

        public abstract class rfiID : PX.Data.BQL.BqlInt.Field<rfiID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "RFI", FieldClass = nameof(FeaturesSet.ConstructionProjectManagement))]
        [PXSelector(typeof(SearchFor<RequestForInformation.requestForInformationId>
                .Where<RequestForInformation.projectId.IsEqual<PMChangeRequest.projectID.FromCurrent>>),
            SubstituteKey = typeof(RequestForInformation.requestForInformationCd))]
        [PXFormula(typeof(Default<PMChangeRequest.projectID>))]
        public int? RFIID
        {
            get;
            set;
        }
        
        public abstract class projectIssueID : PX.Data.BQL.BqlInt.Field<projectIssueID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Project Issue", FieldClass = nameof(FeaturesSet.ConstructionProjectManagement))]
        [PXSelector(typeof(SearchFor<ProjectIssue.projectIssueId>
                .Where<ProjectIssue.projectId.IsEqual<PMChangeRequest.projectID.FromCurrent>>),
            SubstituteKey = typeof(ProjectIssue.projectIssueCd))]
        [PXFormula(typeof(Default<PMChangeRequest.projectID>))]
        public int? ProjectIssueID
        {
            get;
            set;
        }
    }
}
