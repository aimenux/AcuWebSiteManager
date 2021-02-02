using System;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.ProjectsIssue.PJ.DAC
{
    [Serializable]
    [PXCacheName("Project Issue Drawing Log")]
    public class ProjectIssueDrawingLog : DrawingLogReferenceBase, IBqlTable
    {
        [PXDBInt(IsKey = true)]
        public virtual int? ProjectIssueId
        {
            get;
            set;
        }

        public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }

        public abstract class drawingLogId : BqlInt.Field<drawingLogId>
        {
        }
    }
}