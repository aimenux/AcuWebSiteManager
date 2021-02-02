using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(ContractPostBatchMaint))]
    public class FSContractPostBatch : PX.Data.IBqlTable
    {
        #region ContractPostBatchID
        public abstract class contractPostBatchID : PX.Data.BQL.BqlInt.Field<contractPostBatchID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false, Visible = false, DisplayName = "Contract Post Batch ID")]
        public virtual int? ContractPostBatchID { get; set; }
        #endregion
        #region ContractPostBatchNbr
        public abstract class contractPostBatchNbr : PX.Data.BQL.BqlString.Field<contractPostBatchNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSContractPostBatch.contractPostBatchNbr>))]
        [AutoNumber(typeof(Search<FSSetup.postBatchNumberingID>),
                    typeof(AccessInfo.businessDate))]
        public virtual string ContractPostBatchNbr { get; set; }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [FinPeriodID(BqlField = typeof(FSContractPostBatch.finPeriodID))]
        [PXUIField(DisplayName = "Billing Period", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region InvoiceDate
        public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }

        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Billing Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? InvoiceDate { get; set; }
        #endregion
        #region PostTo
        public abstract class postTo : ListField_PostTo
        {
        }

        [PXDBString(2, IsFixed = true)]
        [postTo.ListAtrribute]
        [PXUIField(DisplayName = "Generated In", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string PostTo { get; set; }
        #endregion
        #region UpToDate
        public abstract class upToDate : PX.Data.BQL.BqlDateTime.Field<upToDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Up to Date")]
        public virtual DateTime? UpToDate { get; set; }
        #endregion        
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created By Screen ID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
        [PXUIField(DisplayName = "Last Modified By Screen ID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion
    }
}