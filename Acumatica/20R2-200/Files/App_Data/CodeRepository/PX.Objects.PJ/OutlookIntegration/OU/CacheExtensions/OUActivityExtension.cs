using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.OutlookIntegration.OU.CacheExtensions
{
    public sealed class OuActivityExtension : PXCacheExtension<OUActivity>
    {
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project")]
        public bool? IsLinkProject
        {
            get => Base.Type == typeof(PMProject).FullName;
            set
            {
                if (value == true)
                {
                    Base.Type = typeof(PMProject).FullName;
                }
                else if (Base.IsLinkCase == true)
                {
                    Base.IsLinkContact = true;
                }
            }
        }

        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>>),
            DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible)]
        [PXUIVisible(typeof(isLinkProject.IsEqual<True>))]
        public int? ProjectId
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Request For Information")]
        public bool? IsLinkRequestForInformation
        {
            get => Base.Type == typeof(RequestForInformation).FullName;
            set
            {
                if (value == true)
                {
                    Base.Type = typeof(RequestForInformation).FullName;
                }
                else if (Base.IsLinkCase == true)
                {
                    Base.IsLinkContact = true;
                }
            }
        }

        [PXInt]
        [PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(RequestForInformation.requestForInformationId),
            SubstituteKey = typeof(RequestForInformation.requestForInformationCd),
            DescriptionField = typeof(RequestForInformation.summary))]
        [PXUIVisible(typeof(isLinkRequestForInformation.IsEqual<True>))]
        public int? RequestForInformationId
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Issue")]
        public bool? IsLinkProjectIssue
        {
            get => Base.Type == typeof(ProjectIssue).FullName;
            set
            {
                if (value == true)
                {
                    Base.Type = typeof(ProjectIssue).FullName;
                }
                else if (Base.IsLinkCase == true)
                {
                    Base.IsLinkContact = true;
                }
            }
        }

        [PXInt]
        [PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(ProjectIssue.projectIssueId),
            SubstituteKey = typeof(ProjectIssue.projectIssueCd),
            DescriptionField = typeof(ProjectIssue.summary))]
        [PXUIVisible(typeof(isLinkProjectIssue.IsEqual<True>))]
        public int? ProjectIssueId
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class isLinkProject : BqlBool.Field<isLinkProject>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class isLinkRequestForInformation : BqlBool.Field<isLinkRequestForInformation>
        {
        }

        public abstract class requestForInformationId : BqlInt.Field<requestForInformationId>
        {
        }

        public abstract class isLinkProjectIssue : BqlBool.Field<isLinkProjectIssue>
        {
        }

        public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }
    }
}
