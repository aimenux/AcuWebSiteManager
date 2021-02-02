using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSContractPostRegister : PX.Data.IBqlTable
    {
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Contract Period ID")]
        public virtual int? ContractPeriodID { get; set; }
        #endregion
        #region ContractPostBatchID
        public abstract class contractPostBatchID : PX.Data.BQL.BqlInt.Field<contractPostBatchID> { }

        [PXDBInt]
        public virtual int? ContractPostBatchID { get; set; }
        #endregion
        #region PostDocType
        public abstract class postDocType : PX.Data.BQL.BqlString.Field<postDocType> { }

        [PXDBString(3, IsFixed = true, InputMask = ">aaa")]
        public virtual string PostDocType { get; set; }
        #endregion
        #region PostedTO
        public abstract class postedTO : PX.Data.BQL.BqlString.Field<postedTO> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        public virtual string PostedTO { get; set; }
        #endregion
        #region PostRefNbr
        public abstract class postRefNbr : PX.Data.BQL.BqlString.Field<postRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        public virtual string PostRefNbr { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Service Contract ID")]
        public virtual int? ServiceContractID { get; set; }
        #endregion
    }
}