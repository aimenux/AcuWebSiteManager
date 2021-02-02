using System;
using PX.Data;

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select4<INTransitLineStatusSO,
	  Where<INTransitLineStatusSO.qtyOnHand, Greater<Zero>>,
	  Aggregate<GroupBy<INTransitLineStatusSO.sOShipmentNbr,
		GroupBy<INTransitLineStatusSO.sOOrderType,
		  GroupBy<INTransitLineStatusSO.sOOrderNbr>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class INTransferInTransitSO : IBqlTable
	{
		#region SOShipmentNbr
		public abstract class sOShipmentNbr : PX.Data.IBqlField
		{
		}
		protected String _SOShipmentNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatusSO.sOShipmentNbr))]
		public virtual String SOShipmentNbr
		{
			get
			{
				return this._SOShipmentNbr;
			}
			set
			{
				this._SOShipmentNbr = value;
			}
		}
		#endregion

		#region SOOrderType
		public abstract class sOOrderType : PX.Data.IBqlField
		{
		}
		protected String _SOOrderType;
		[PXDBString(2, IsKey = true, BqlField = typeof(INTransitLineStatusSO.sOOrderType))]
		public virtual String SOOrderType
		{
			get
			{
				return this._SOOrderType;
			}
			set
			{
				this._SOOrderType = value;
			}
		}
		#endregion

		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.IBqlField
		{
		}
		protected String _SOOrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatusSO.sOOrderNbr))]
		public virtual String SOOrderNbr
		{
			get
			{
				return this._SOOrderNbr;
			}
			set
			{
				this._SOOrderNbr = value;
			}
		}
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Guid? _RefNoteID;
		[PXNote(BqlField = typeof(INTransitLineStatusSO.refNoteID))]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
	}
}
