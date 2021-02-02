using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.DAC;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.DAC
{
    [Serializable]
    [PXCacheName("Lien Waiver Recipient")]
    public class LienWaiverRecipient : BaseCache, IBqlTable
    {
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField]
        public virtual bool? Selected
        {
            get;
            set;
        }

        [PXDBIdentity]
        public virtual int? LienWaiverRecipientId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(PMProject.contractID))]
        [PXParent(typeof(SelectFrom<PMProject>.Where<PMProject.contractID.IsEqual<projectId>>))]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Vendor Class")]
        [PXSelector(typeof(SearchFor<VendorClass.vendorClassID>.In<SelectFrom<VendorClass>
            .LeftJoin<EPEmployeeClass>.On<EPEmployeeClass.vendorClassID.IsEqual<VendorClass.vendorClassID>>
            .Where<EPEmployeeClass.vendorClassID.IsNull>>))]
        [PXParent(typeof(SelectFrom<VendorClass>.Where<VendorClass.vendorClassID.IsEqual<vendorClassId>>))]
        public virtual string VendorClassId
        {
            get;
            set;
        }

        [PXDBDecimal(MinValue = 0)]
        [PXDefault]
        [PXUIField(DisplayName = "Minimum Commitment Amount", Required = true)]
        public virtual decimal? MinimumCommitmentAmount
        {
            get;
            set;
        }

        public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class lienWaiverRecipientId : BqlInt.Field<lienWaiverRecipientId>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class vendorClassId : BqlString.Field<vendorClassId>
        {
        }

        public abstract class minimumCommitmentAmount : BqlDecimal.Field<minimumCommitmentAmount>
        {
        }
    }
}
