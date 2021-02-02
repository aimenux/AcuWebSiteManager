using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using static PX.Objects.PO.POCreate;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    [Serializable]
    [PXProjection(typeof(Select2<FSSODet,
                    LeftJoin<FSSODetSplit,
                        On<FSSODetSplit.srvOrdType, Equal<FSSODet.srvOrdType>,
                            And<FSSODetSplit.refNbr, Equal<FSSODet.refNbr>,
                            And<FSSODetSplit.lineNbr, Equal<FSSODet.lineNbr>>>>>>))]
    public class FSSODetFSSODetSplit : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, BqlField = typeof(FSSODet.srvOrdType))]
        [PXUIField(DisplayName = "Service Order Type")]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "", BqlField = typeof(FSSODet.refNbr))]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        public virtual string RefNbr { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true, BqlField = typeof(FSSODet.lineNbr))]
        [PXUIField(DisplayName = "Line Nbr.")]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDBInt(BqlField = typeof(FSSODetSplit.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        [PXDBLong(IsImmutable = true, BqlField = typeof(FSSODetSplit.planID))]
        [PXUIField(DisplayName = "Plan ID")]
        public virtual Int64? PlanID { get; set; }
        #endregion
        #region UnitPrice
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        [PXDBPriceCost(BqlField = typeof(FSSODet.unitPrice))]
        [PXUIField(DisplayName = "Base Unit Price")]
        public virtual Decimal? UnitPrice { get; set; }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        [PXDBString(BqlField = typeof(FSSODet.uOM))]
        [PXUIField(DisplayName = "UOM")]
        public virtual string UOM { get; set; }
        #endregion
    }

}
