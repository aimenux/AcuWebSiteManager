using PX.Data;
using PX.Objects.CM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POLineBillingRevision)]
	[Serializable]
	public class POLineBillingRevision : IBqlTable
	{
		#region APDocType
		public abstract class apDocType : PX.Data.BQL.BqlString.Field<apDocType> { }

		[PXDefault]
		[PXDBString(3, IsKey = true, IsFixed = true)]
		public virtual String APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class apRefNbr : PX.Data.BQL.BqlString.Field<apRefNbr> { }

		[PXDefault]
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		public virtual String APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXDefault]
		[PXDBString(2, IsKey = true, IsFixed = true)]
		public virtual String OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXDefault]
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		public virtual String OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region OrderLineNbr
		public abstract class orderLineNbr : PX.Data.BQL.BqlInt.Field<orderLineNbr> { }

		[PXDefault]
		[PXDBInt(IsKey = true)]
		public virtual Int32? OrderLineNbr
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDefault]
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		[PXDefault]
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

		[PXDefault]
		[PXDBDecimal(typeof(Search<CS.CommonSetup.decPlQty>))]
		public virtual Decimal? OrderQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }

		[PXDefault]
		[PXDBDecimal(typeof(Search<CS.CommonSetup.decPlQty>))]
		public virtual Decimal? BaseOrderQty
		{
			get;
			set;
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }

		[PXDefault]
		[PXDBDecimal(typeof(Search<CS.CommonSetup.decPlQty>))]
		public virtual Decimal? ReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }

		[PXDefault]
		[PXDBDecimal(typeof(Search<CS.CommonSetup.decPlQty>))]
		public virtual Decimal? BaseReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region RcptQtyMax
		public abstract class rcptQtyMax : PX.Data.BQL.BqlDecimal.Field<rcptQtyMax> { }

		[PXDefault]
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		public virtual Decimal? RcptQtyMax
		{
			get;
			set;
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }

		[PXDefault]
		[PXDBDecimal(typeof(Search<CS.CommonSetup.decPlQty>))]
		public virtual Decimal? UnbilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }

		[PXDefault]
		[PXDBDecimal(typeof(Search<CS.CommonSetup.decPlQty>))]
		public virtual Decimal? BaseUnbilledQty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }

		[PXDefault]
		[PXDBCury(typeof(POLineBillingRevision.curyID))]
		public virtual Decimal? CuryUnbilledAmt
		{
			get;
			set;
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }

		[PXDefault]
		[PXDBBaseCury]
		public virtual Decimal? UnbilledAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

		[PXDefault]
		[IN.PXDBPriceCost]
		public virtual Decimal? CuryUnitCost
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		[PXDefault]
		[IN.PXDBPriceCost]
		public virtual Decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(RecordComesFirst = true)]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
