using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<SOOrderType>))]
    #endregion
    public class SOOrderTypeQuickProcess : SOOrderType
    {
        #region OrderType
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region Behavior
        public new abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
        #endregion
        #region AllowQuickProcess
        public new abstract class allowQuickProcess : PX.Data.BQL.BqlBool.Field<allowQuickProcess> { }
        #endregion
    }
}
