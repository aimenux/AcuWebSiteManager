using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.TM;
using Constants = PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Constants;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName(CacheNames.DrawingLog)]
    [PXPrimaryGraph(typeof(DrawingLogEntry))]
    public class DrawingLog : BaseCache, IBqlTable, IProjectManagementDocumentBase, IDocumentWithConfigurableStatus
    {
        [PXDBIdentity]
        [CascadeDelete(typeof(RequestForInformationDrawingLog), typeof(RequestForInformationDrawingLog.drawingLogId))]
        [CascadeDelete(typeof(ProjectIssueDrawingLog), typeof(ProjectIssueDrawingLog.drawingLogId))]
        [CascadeDelete(typeof(DrawingLogRevision), typeof(DrawingLogRevision.drawingLogId))]
        [CascadeDelete(typeof(DrawingLogRevision), typeof(DrawingLogRevision.drawingLogRevisionId))]
        [ValidateDrawingLogStatuses]
        [ValidateDrawingLogPersisting]
        public virtual int? DrawingLogId
        {
            get;
            set;
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = DrawingLogLabels.DrawingLogId, Required = true,
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<drawingLogCd>), Filterable = true)]
        [AutoNumber(typeof(DrawingLogSetup.drawingLogNumberingSequenceId), typeof(AccessInfo.businessDate))]
        public virtual string DrawingLogCd
        {
            get;
            set;
        }

        [PXDefault(typeof(Search<PMProject.contractID,
            Where<PMProject.contractID, Equal<Current<DrawingLogFilter.projectId>>>>))]
        [ActiveOrPlanningProject]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.taskID, Equal<Current<DrawingLogFilter.projectTaskId>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [ActiveOrPlanningProjectTaskWithType(typeof(projectId), DisplayName = "Project Task",
            AllowNull = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Validate<projectId>))]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(projectTaskId),
            Message = ProjectAccountingMessages.CostTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Revenue)]
        public virtual int? ProjectTaskId
        {
            get;
            set;
        }

        [Owner]
        [PXDefault(typeof(AccessInfo.contactID))]
        public virtual int? OwnerId
        {
            get;
            set;
        }

        [LinkRefNote(typeof(drawingLogCd))]
        [PXUIField(DisplayName = DrawingLogLabels.OriginalDrawingId, Enabled = false,
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(noteID), SubstituteKey = typeof(drawingLogCd))]
        public virtual Guid? OriginalDrawingId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<DrawingLogDiscipline.drawingLogDisciplineId,
            Where<DrawingLogDiscipline.isActive, Equal<True>>>),
            SubstituteKey = typeof(DrawingLogDiscipline.name))]
        [PXDefault(typeof(Search<DrawingLogDiscipline.drawingLogDisciplineId,
            Where<DrawingLogDiscipline.drawingLogDisciplineId, Equal<Current<DrawingLogFilter.disciplineId>>>>))]
        [PXUIField(DisplayName = DrawingLogLabels.Discipline,
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? DisciplineId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Drawing Number", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Number
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Revision
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Sketch", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Sketch
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Title", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Title
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Description
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault(typeof(Search<DrawingLogStatus.statusId,
            Where<DrawingLogStatus.isDefault, Equal<True>>>))]
        [PXSelector(typeof(DrawingLogStatus.statusId),
            typeof(DrawingLogStatus.name),
            typeof(DrawingLogStatus.description),
            SubstituteKey = typeof(DrawingLogStatus.name))]
        [PXUIField(DisplayName = DrawingLogLabels.Status)]
        public virtual int? StatusId
        {
            get;
            set;
        }

        [PXInt]
        [PXSelector(typeof(DrawingLogStatus.statusId), SubstituteKey = typeof(DrawingLogStatus.name))]
        [PXUIField(DisplayName = DrawingLogLabels.Status, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? SelectorStatusId => StatusId;

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Drawing Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DrawingDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Received Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ReceivedDate
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Current")]
        public virtual bool? IsCurrent
        {
            get;
            set;
        }

        [PXBool]
        [UiInformationField]
        public bool? Selected
        {
            get;
            set;
        }

        [CRAttributesField(typeof(usrDrawingLogClassId), typeof(noteID))]
        public virtual string[] UsrAttributes
        {
            get;
            set;
        }

        [PXString(20)]
        public virtual string UsrDrawingLogClassId => Constants.DrawingLogClassId;

        [PXNote]
        [DrawingLogSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class drawingLogId : BqlInt.Field<drawingLogId>
        {
        }

        public abstract class drawingLogCd : BqlString.Field<drawingLogCd>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class ownerId : BqlInt.Field<ownerId>
        {
        }

        public abstract class originalDrawingId : BqlGuid.Field<originalDrawingId>
        {
        }

        public abstract class disciplineId : BqlInt.Field<disciplineId>
        {
        }

        public abstract class number : BqlString.Field<number>
        {
        }

        public abstract class revision : BqlString.Field<revision>
        {
        }

        public abstract class sketch : BqlString.Field<sketch>
        {
        }

        public abstract class title : BqlString.Field<title>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class statusId : BqlInt.Field<statusId>
        {
        }

        public abstract class selectorStatusId : BqlInt.Field<selectorStatusId>
        {
        }

        public abstract class drawingDate : BqlDateTime.Field<drawingDate>
        {
        }

        public abstract class receivedDate : BqlDateTime.Field<receivedDate>
        {
        }

        public abstract class isCurrent : BqlBool.Field<isCurrent>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class usrDrawingLogClassId : BqlString.Field<usrDrawingLogClassId>
        {
        }

        public abstract class usrAttributes : IBqlField
        {
        }

        public class drawingLogClassId : BqlString.Constant<drawingLogClassId>
        {
            public drawingLogClassId()
                : base(Constants.DrawingLogClassId)
            {
            }
        }

        public class typeName : BqlString.Constant<typeName>
        {
            public typeName()
                : base(typeof(DrawingLog).FullName)
            {
            }
        }
    }
}