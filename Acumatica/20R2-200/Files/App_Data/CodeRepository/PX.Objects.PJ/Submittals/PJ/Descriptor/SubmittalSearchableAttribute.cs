using PX.Data;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Descriptor;
using PX.Objects.PM;
using PX.Objects.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PJ.Submittals
{    
    public class SubmittalSearchableAttribute : PXSearchableAttribute
    {
         private const string FirstLineFormat = "{0}{1}";
        private const string SecondLineFormat = "{1}{2}";

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(PJSubmittal.status),
            typeof(PJSubmittal.projectId)
        };

        private static readonly Type[] FieldsForTheSecondLine =
        {
            typeof(PJSubmittal.currentWorkflowItemContactID), 
            typeof(PX.Objects.CR.Contact.displayName), 
            typeof(PJSubmittal.summary)
        };

        private static readonly Type[] TitleFields =
        {
            typeof(PJSubmittal.submittalID), 
            typeof(PJSubmittal.revisionID)
        };

        private static readonly Type[] IndexedFields =
       {
            typeof(PJSubmittal.summary),
            typeof(PJSubmittal.projectId),
            typeof(PMProject.contractCD),
            typeof(PMProject.description)
        };

        public SubmittalSearchableAttribute()
            : base(SearchCategory.PM, SubmittalMessage.SubmittleSearchTitle, TitleFields, IndexedFields)
        {
            NumberFields = TitleFields;
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = FieldsForTheSecondLine;
            SelectForFastIndexing = typeof(Select2<PJSubmittal, InnerJoin<PMProject, On<PJSubmittal.projectId, Equal<PMProject.contractID>>>>);
        }

        protected override string OverrideDisplayName(Type field, string displayName)
        {
           if (field == typeof(PX.Objects.CR.Contact.displayName))
            {
                return SubmittalMessage.BallInCourt;
            }

            return displayName;
        }
    }
}
