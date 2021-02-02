using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CM;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select<POReceiptLine>), Persistent = false)]
	[Serializable]
	[PXHidden]
	public partial class POReceiptLineSigned : IBqlTable
	{
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
		}
		[PXDBString(POReceiptLine.receiptType.Length, IsFixed = true, BqlField = typeof(POReceiptLine.receiptType))]
		public virtual string ReceiptType
		{
			get;
			set;
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		public virtual string ReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType>
		{
		}
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.pOType))]
		public virtual string POType
		{
			get;
			set;
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
		}
		[PXDBString(POReceiptLine.pONbr.Length, IsUnicode = true, BqlField = typeof(POReceiptLine.pONbr))]
		public virtual string PONbr
		{
			get;
			set;
		}
		#endregion
		#region SignedBaseReceiptQty
		public abstract class signedBaseReceiptQty : PX.Data.BQL.BqlDecimal.Field<signedBaseReceiptQty>
		{
		}
		[PXQuantity]
		[PXDBCalced(typeof(Mult<POReceiptLine.baseReceiptQty, POReceiptLine.invtMult>), typeof(decimal))]
		public virtual decimal? SignedBaseReceiptQty
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(Select5<POReceipt,
		InnerJoin<POOrderReceipt, On<POReceipt.receiptNbr, Equal<POOrderReceipt.receiptNbr>>,
		LeftJoin<POReceiptLineSigned, On<POReceiptLineSigned.receiptType, Equal<POReceipt.receiptType>,
			And<POReceiptLineSigned.receiptNbr, Equal<POReceipt.receiptNbr>,
			And<POReceiptLineSigned.pOType, Equal<POOrderReceipt.pOType>,
			And<POReceiptLineSigned.pONbr, Equal<POOrderReceipt.pONbr>>>>>>>,
		Aggregate<GroupBy<POOrderReceipt.receiptNbr, GroupBy<POOrderReceipt.pOType, GroupBy<POOrderReceipt.pONbr, Sum<POReceiptLineSigned.signedBaseReceiptQty>>>>>>), Persistent = false)]
	[Serializable]
	[PXCacheName(Messages.POOrderPOReceipt)]
	public partial class POOrderPOReceipt : IBqlTable
	{
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
		}
		[PXDBString(2, IsFixed = true, InputMask = "", BqlField = typeof(POReceipt.receiptType))]
		[POReceiptType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual String ReceiptType
		{
			get;
			set;
		}
		#endregion

		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
			public const string DisplayName = "Receipt Nbr.";
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(POOrderReceipt.receiptNbr))]
		[PXUIField(DisplayName = receiptNbr.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Optional<receiptType>>>>), Filterable = true)]
		public virtual String ReceiptNbr
		{
			get;
			set;
		}
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate>
		{
		}

		/// <summary>
		/// Date of the document.
		/// </summary>
		[PXDBDate(BqlField = typeof(POReceipt.receiptDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true, BqlField = typeof(POReceipt.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POReceiptStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region TotalQty
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty>
		{
		}
		[PXDBQuantity(BqlField = typeof(POReceiptLineSigned.signedBaseReceiptQty))]
		[PXUIField(DisplayName = "Received Qty.", Enabled = false)]
		public virtual Decimal? TotalQty
		{
			get;
			set;
		}
		#endregion

		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType>
		{
		}

		[PXDBString(2, IsFixed = true, IsKey = true, BqlField = typeof(POOrderReceipt.pOType))]
		[POOrderType.List()]
		[PXUIField(DisplayName = "PO Type", Enabled = false, IsReadOnly = true)]
		public virtual String POType
		{
			get;
			set;
		}
		#endregion

		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(POOrderReceipt.pONbr))]
		[PXUIField(DisplayName = "PO Number", Enabled = false, IsReadOnly = true)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Optional<pOType>>>>))]
		public virtual String PONbr
		{
			get;
			set;
		}
		#endregion

		#region StatusText
		public abstract class statusText : PX.Data.BQL.BqlString.Field<statusText>
		{
		}
		[PXString]
		public virtual String StatusText
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		[ProjectionNote(typeof(POReceipt), BqlField = typeof(POReceipt.noteID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
	}
}
