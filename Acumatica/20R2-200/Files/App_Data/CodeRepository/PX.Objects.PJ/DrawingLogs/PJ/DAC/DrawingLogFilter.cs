using System;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName("Drawing Log Filter")]
    public class DrawingLogFilter : IBqlTable
    {
        [Project(typeof(Where<PMProject.nonProject, Equal<False>,
                And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = DrawingLogLabels.Project, WarnIfCompleted = false)]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [ActiveOrPlanningProjectTaskWithType(typeof(projectId), NeedsPrefilling = false,
            DisplayName = "Project Task", AlwaysEnabled = true)]
        public virtual int? ProjectTaskId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<DrawingLogDiscipline.drawingLogDisciplineId,
                Where<DrawingLogDiscipline.isActive, Equal<True>>>),
            SubstituteKey = typeof(DrawingLogDiscipline.name))]
        [PXUIField(DisplayName = DrawingLogLabels.Discipline)]
        public virtual int? DisciplineId
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Current Only")]
        public virtual bool? IsCurrentOnly
        {
            get;
            set;
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class disciplineId : BqlInt.Field<disciplineId>
        {
        }

        public abstract class isCurrentOnly : BqlBool.Field<isCurrentOnly>
        {
        }
    }
}