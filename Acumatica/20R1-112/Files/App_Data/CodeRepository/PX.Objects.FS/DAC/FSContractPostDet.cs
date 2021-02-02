using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSContractPostDet : PX.Data.IBqlTable
    {
        #region ContractPostDetID
        public abstract class contractPostDetID : PX.Data.BQL.BqlInt.Field<contractPostDetID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? ContractPostDetID { get; set; }
        #endregion
        #region AppDetID
        public abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBInt]
        public virtual int? AppDetID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region ContractPeriodDetID
        public abstract class contractPeriodDetID : PX.Data.BQL.BqlInt.Field<contractPeriodDetID> { }

        [PXDBInt]
        public virtual int? ContractPeriodDetID { get; set; }
        #endregion
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract Period Nbr.")]
        public virtual int? ContractPeriodID { get; set; }
        #endregion
        #region ContractPostBatchID
        public abstract class contractPostBatchID : PX.Data.BQL.BqlInt.Field<contractPostBatchID> { }

        [PXDBInt]
        public virtual int? ContractPostBatchID { get; set; }
        #endregion
        #region ContractPostDocID
        public abstract class contractPostDocID : PX.Data.BQL.BqlInt.Field<contractPostDocID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract PostDoc Nbr.")]
        public virtual int? ContractPostDocID { get; set; }
        #endregion
        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt]
        public virtual int? SODetID { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        public virtual int? SOID { get; set; }
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
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region Tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}