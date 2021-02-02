using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.Descriptor.Attributes;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName(CacheNames.DrawingLogStatus)]
    public class DrawingLogStatus : IStatus, IBqlTable
    {
        [PXDBIdentity(IsKey = true, DatabaseFieldName = "DrawingLogStatusId")]
        public int? StatusId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Unique(ErrorMessage = ProjectManagementMessages.StatusNameUniqueConstraint)]
        [PXUIField(DisplayName = "Status")]
        public string Name
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public string Description
        {
            get;
            set;
        }

        [PXDBBool]
        public bool? IsDefault
        {
            get;
            set;
        }

        public abstract class statusId : BqlInt.Field<statusId>
        {
        }

        public abstract class name : BqlString.Field<name>
        {
        }

        public abstract class isDefault : BqlBool.Field<isDefault>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }
    }
}
