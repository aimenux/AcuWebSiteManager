using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSPostRegister : PX.Data.IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, IsKey = true, InputMask = ">AAAA")]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true)]
        public virtual string RefNbr { get; set; }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        [PXDBString(5, IsFixed = true, IsKey = true)]
        public virtual string Type { get; set; }
        #endregion
        #region EntityType
        public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        public virtual string EntityType { get; set; }
        #endregion
        #region BatchID
        public abstract class batchID : PX.Data.BQL.BqlInt.Field<batchID> { }

        [PXDBInt]
        public virtual int? BatchID { get; set; }
        #endregion
        #region ProcessID
        public abstract class processID : PX.Data.BQL.BqlGuid.Field<processID> { }

        [PXDBGuid]
        public virtual Guid? ProcessID { get; set; }
        #endregion
        #region PostedTO
        public abstract class postedTO : PX.Data.BQL.BqlString.Field<postedTO> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        public virtual string PostedTO { get; set; }
        #endregion
        #region PostDocType
        public abstract class postDocType : PX.Data.BQL.BqlString.Field<postDocType> { }

        [PXDBString(3, IsFixed = true, InputMask = ">aaa")]
        public virtual string PostDocType { get; set; }
        #endregion
        #region PostRefNbr
        public abstract class postRefNbr : PX.Data.BQL.BqlString.Field<postRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        public virtual string PostRefNbr { get; set; }
        #endregion
    }
}