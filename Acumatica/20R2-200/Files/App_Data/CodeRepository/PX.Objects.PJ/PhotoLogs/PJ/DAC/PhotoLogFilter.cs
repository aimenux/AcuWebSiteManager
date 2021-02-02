using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName("Photo Log Filter")]
    public class PhotoLogFilter : IBqlTable
    {
        [Project(typeof(PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>),
            DisplayName = "Project", WarnIfCompleted = false)]
        public int? ProjectId
        {
            get;
            set;
        }

        [ProjectTask(typeof(projectId), DisplayName = "Project Task")]
        public int? ProjectTaskId
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Date From")]
        public DateTime? DateFrom
        {
            get;
            set;
        }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Date To")]
        public DateTime? DateTo
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

        public abstract class dateFrom : BqlDateTime.Field<dateFrom>
        {
        }

        public abstract class dateTo : BqlDateTime.Field<dateTo>
        {
        }
    }
}