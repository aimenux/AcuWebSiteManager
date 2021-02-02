using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POReceiptAPDoc)]
	public partial class POReceiptAPDoc : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXString(3, IsKey = true, IsFixed = true)]
		[APDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Optional<docType>>>>), Filterable = true)]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDate()]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[APDocStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region TotalQty
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
		[PXQuantity()]
		[PXUIField(DisplayName = "Billed Qty.", Enabled = false)]
		public virtual Decimal? TotalQty
		{
			get;
			set;
		}
		#endregion

		#region TotalAmt
		public abstract class totalAmt : PX.Data.BQL.BqlDecimal.Field<totalAmt> { }

		[PXBaseCury()]
		[PXUIField(DisplayName = "Billed Amt.", Enabled = false)]
		public virtual Decimal? TotalAmt
		{
			get;
			set;
		}
		#endregion

		#region AccruedQty
		public abstract class accruedQty : PX.Data.BQL.BqlDecimal.Field<accruedQty> { }

		[PXQuantity()]
		[PXUIField(DisplayName = "Accrued Qty.")]
		public virtual Decimal? AccruedQty
		{
			get;
			set;
		}
		#endregion

		#region AccruedAmt
		public abstract class accruedAmt : PX.Data.BQL.BqlDecimal.Field<accruedAmt> { }

		[PXBaseCury()]
		[PXUIField(DisplayName = "Accrued Amt.")]
		public virtual Decimal? AccruedAmt
		{
			get;
			set;
		}
		#endregion

		#region TotalPPVAmt
		public abstract class totalPPVAmt : PX.Data.BQL.BqlDecimal.Field<totalPPVAmt> { }

		[PXBaseCury()]
		[PXUIField(DisplayName = "PPV Amt")]
		public virtual Decimal? TotalPPVAmt
		{
			get;
			set;
		}
		#endregion

		#region StatusText
		public abstract class statusText : PX.Data.BQL.BqlString.Field<statusText> { }
		[PXString]
		public virtual String StatusText
		{
			get;
			set;
		}
		#endregion
	}
}
