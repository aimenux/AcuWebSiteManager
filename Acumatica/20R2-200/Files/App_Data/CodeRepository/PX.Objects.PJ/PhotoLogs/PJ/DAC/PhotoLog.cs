using System;
using PX.Objects.PJ.PhotoLogs.PJ.Attributes;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.DAC
{
    [Serializable]
    [PXPrimaryGraph(typeof(PhotoLogEntry))]
    [PXCacheName("Photo Log")]
    public class PhotoLog : BaseCache, IBqlTable, IDocumentWithConfigurableStatus, IProjectManagementDocumentBase
    {
        [PXBool]
        public bool? Selected
        {
            get;
            set;
        }

        [PXDBIdentity]
        public int? PhotoLogId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Photo Log ID", Visibility = PXUIVisibility.SelectorVisible)]
        [AutoNumber(typeof(PhotoLogSetup.photoLogNumberingId), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(photoLogCd),
            typeof(photoLogCd),
            typeof(date),
            typeof(projectId),
            typeof(projectTaskId),
            typeof(selectorStatusId),
            typeof(description))]
        public string PhotoLogCd
        {
            get;
            set;
        }

        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public DateTime? Date
        {
            get;
            set;
        }

        [PXDefault]
        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>
                .And<PMProject.status.IsEqual<ProjectStatus.active>>>),
            DisplayName = "Project", Visibility = PXUIVisibility.SelectorVisible)]
        public int? ProjectId
        {
            get;
            set;
        }

        [ProjectTask(typeof(projectId), AlwaysEnabled = true, AllowNull = true, DisplayName = "Project Task",
            Visibility = PXUIVisibility.SelectorVisible)]
        public int? ProjectTaskId
        {
            get;
            set;
        }

        [PXDBString(IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public string Description
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault(typeof(SearchFor<PhotoLogStatus.statusId>.
            Where<PhotoLogStatus.isDefault.IsEqual<True>>))]
        [PXSelector(typeof(PhotoLogStatus.statusId),
            typeof(PhotoLogStatus.name),
            typeof(PhotoLogStatus.description),
            SubstituteKey = typeof(PhotoLogStatus.name))]
        [PXUIField(DisplayName = "Status")]
        public int? StatusId
        {
            get;
            set;
        }

        [PXInt]
        [PXSelector(typeof(PhotoLogStatus.statusId), SubstituteKey = typeof(PhotoLogStatus.name))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        public int? SelectorStatusId => StatusId;

        [PXDBCreatedByID(DisplayName = "Created By")]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modification Date")]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        [PXNote]
        [PhotoLogSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXInt]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class photoLogId : BqlInt.Field<photoLogId>
        {
        }

        public abstract class photoLogCd : BqlString.Field<photoLogCd>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class date : BqlDateTime.Field<date>
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

        public abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }
    }
}