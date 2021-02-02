using System;
using PX.Data;
﻿
namespace PX.Objects.FS
{
	[System.SerializableAttribute]
    [PXCacheName(TX.TableName.CONTRACT_GENERATION_HISTORY)] 
    [PXPrimaryGraph(typeof(ContractGenerationHistoryMaint))]
	public class FSContractGenerationHistory : PX.Data.IBqlTable
    {
        #region ContractGenerationHistoryID
        public abstract class contractGenerationHistoryID : PX.Data.BQL.BqlInt.Field<contractGenerationHistoryID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false, Visible=false)]
        public virtual int? ContractGenerationHistoryID { get; set; }
		#endregion
        #region GenerationID
        public abstract class generationID : PX.Data.BQL.BqlInt.Field<generationID> { }
        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Generation ID")]
        public virtual int? GenerationID { get; set; }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "ScheduleID")]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region EntityType
        public abstract class entityType : ListField_Schedule_EntityType
        {
        }
        [PXDBString(1, IsFixed = true)]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "Entity Type")]
        public virtual string EntityType { get; set; }
        #endregion
        #region LastGeneratedElementDate
        public abstract class lastGeneratedElementDate : PX.Data.BQL.BqlDateTime.Field<lastGeneratedElementDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Last Generated Element")]
        public virtual DateTime? LastGeneratedElementDate { get; set; }
        #endregion
        #region LastProcessedDate
        public abstract class lastProcessedDate : PX.Data.BQL.BqlDateTime.Field<lastProcessedDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Up to Date")]
        public virtual DateTime? LastProcessedDate { get; set; }
        #endregion
        #region PreviousGeneratedElementDate
        public abstract class previousGeneratedElementDate : PX.Data.BQL.BqlDateTime.Field<previousGeneratedElementDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Previous Generated Element")]
        public virtual DateTime? PreviousGeneratedElementDate { get; set; }
        #endregion
        #region PreviousProcessedDate
        public abstract class previousProcessedDate : PX.Data.BQL.BqlDateTime.Field<previousProcessedDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Previous Last Processed")]
        public virtual DateTime? PreviousProcessedDate { get; set; }
        #endregion
        #region RecordType
        public abstract class recordType : ListField_RecordType_ContractSchedule
        {
        }
        [PXDBString(4, IsUnicode = true)]
        [PXDefault(ID.RecordType_ServiceContract.SERVICE_CONTRACT)]
        [recordType.ListAtrribute]
        public virtual string RecordType { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Generation Date")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}
