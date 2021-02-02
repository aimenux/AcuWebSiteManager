using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(ServiceOrderEntry))]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<FSSODet>), Persistent = false)]
    public class FSSODetEmployee : FSSODet
    {
        public new abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        public new abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        public new abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }
    }
}