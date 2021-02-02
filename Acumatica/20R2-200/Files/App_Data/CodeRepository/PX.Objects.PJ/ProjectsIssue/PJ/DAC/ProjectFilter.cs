using System;
using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectsIssue.PJ.DAC
{
    [Serializable]
    [PXCacheName("Project Filter")]
    public class ProjectFilter : IBqlTable
    {
        [Project(typeof(Where<PMProject.nonProject, Equal<False>,
                And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = "Project")]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        public abstract class projectId : IBqlField
        {
        }
    }
}
