using PX.Data;
using PX.Objects.CM;
using System;

namespace PX.Objects.FS
{
    [PXProjection(typeof(Select<FSTaxDetail,
                            Where<FSTaxDetail.entityType, Equal<FSTaxDetail.entityType.Service_Order>>>),
                    Persistent = true)]
    [Serializable]
    [PXBreakInheritance]
	public class FSServiceOrderTax : FSTaxDetail
	{
        #region EntityType
        public new abstract class entityType : ListField_PostDoc_EntityType
        {
        }
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(ID.PostDoc_EntityType.SERVICE_ORDER)]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "EntityType", Visibility = PXUIVisibility.Visible, Visible = true)]
        public override String EntityType { get; set; }
        #endregion

        #region EntityID
        public new abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

        [PXDBInt(IsKey = true)]
        [PXDBLiteDefault(typeof(FSServiceOrder.sOID))]
        [PXUIField(DisplayName = "Entity ID", Visibility = PXUIVisibility.Visible, Visible = true)]
        [PXParent(typeof(Select<FSServiceOrder,
                            Where<
                                FSServiceOrder.sOID, Equal<Current<FSServiceOrderTax.entityID>>,
                                And<FSServiceOrderTax.entityType.Service_Order, Equal<Current<FSServiceOrderTax.entityType>>>>>))]
        public override int? EntityID { get; set; }
        #endregion

        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(FSSODetUNION.lineNbr))]
        [PXUIField(DisplayName = "LineNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
        [PXParent(typeof(Select<FSSODetUNION,
                            Where<
                                FSSODetUNION.sOID, Equal<Current<FSServiceOrderTax.entityID>>,
                                And<FSSODetUNION.lineNbr, Equal<Current<FSServiceOrderTax.lineNbr>>,
                                And<FSServiceOrderTax.entityType.Service_Order, Equal<Current<FSServiceOrderTax.entityType>>>>>>))]
        public override Int32? LineNbr { get; set; }
        #endregion

        #region TaxID
        public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        #endregion

		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(FSServiceOrder.curyInfoID))]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
	}
}
