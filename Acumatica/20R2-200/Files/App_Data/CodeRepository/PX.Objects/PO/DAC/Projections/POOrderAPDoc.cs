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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select<APTran>), Persistent = false)]
	[Serializable]
	[PXHidden]
	public partial class APTranSigned : IBqlTable
	{
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType>
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(APTran.tranType))]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(APTran.refNbr))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		[PXDBInt(IsKey = true, BqlField = typeof(APTran.lineNbr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region POOrderType
		public abstract class pOOrderType : PX.Data.BQL.BqlString.Field<pOOrderType>
		{
		}
		[PXDBString(2, IsFixed = true, BqlField = typeof(APTran.pOOrderType))]
		public virtual string POOrderType
		{
			get;
			set;
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
		}
		[PXDBString(15, IsUnicode = true, BqlField = typeof(APTran.pONbr))]
		public virtual string PONbr
		{
			get;
			set;
		}
		#endregion
		#region SignedBaseQty
		public abstract class signedBaseQty : PX.Data.BQL.BqlDecimal.Field<signedBaseQty>
		{
		}
		[PXDBCalced(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.baseQty>, Minus<APTran.baseQty>>), typeof(decimal))]
		[PXQuantity]
		public virtual decimal? SignedBaseQty
		{
			get;
			set;
		}
		#endregion
		#region SignedCuryLineAmt
		[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R1)]
		public abstract class signedCuryLineAmt : PX.Data.BQL.BqlDecimal.Field<signedCuryLineAmt>
		{
		}
		[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R1)]
		[PXDBCalced(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.curyLineAmt>, Minus<APTran.curyLineAmt>>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? SignedCuryLineAmt
		{
			get;
			set;
		}
		#endregion
		#region SignedCuryTranAmt
		public abstract class signedCuryTranAmt : PX.Data.BQL.BqlDecimal.Field<signedCuryTranAmt>
		{
		}
		[PXDBCalced(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.curyTranAmt>, Minus<APTran.curyTranAmt>>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? SignedCuryTranAmt
		{
			get;
			set;
		}
		#region SignedCuryRetainageAmt
		public abstract class signedCuryRetainageAmt : PX.Data.BQL.BqlDecimal.Field<signedCuryRetainageAmt>
		{
		}
		[PXDBCalced(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.curyRetainageAmt>, Minus<APTran.curyRetainageAmt>>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? SignedCuryRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#endregion
		#region POPPVAmt
		public abstract class pOPPVAmt : PX.Data.BQL.BqlDecimal.Field<pOPPVAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(APTran.pOPPVAmt))]
		public virtual decimal? POPPVAmt
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(Select5<APTranSigned,
		InnerJoin<APInvoice, On<APInvoice.docType, Equal<APTranSigned.tranType>, And<APInvoice.refNbr, Equal<APTranSigned.refNbr>>>>,
		Where<APTranSigned.tranType, NotEqual<APDocType.prepayment>>,
		Aggregate<
			GroupBy<APTranSigned.pOOrderType,
			GroupBy<APTranSigned.pONbr,
			GroupBy<APInvoice.docType,
			GroupBy<APInvoice.refNbr,
			Sum<APTranSigned.signedBaseQty,
			Sum<APTranSigned.signedCuryTranAmt,
			Sum<APTranSigned.signedCuryRetainageAmt,
			Sum<APTranSigned.pOPPVAmt>>>>>>>>>>), Persistent = false)]
	[Serializable]
	[PXCacheName(Messages.POOrderAPDoc)]
	public partial class POOrderAPDoc : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
		{
		}

		/// <summary>
		/// [key] Type of the document.
		/// </summary>
		/// <value>
		/// Possible values are: "INV" - Invoice, "ACR" - Credit Adjustment, "ADR" - Debit Adjustment, 
		/// "CHK" - Check, "VCK" - Void Check, "PPM" - Prepayment, "REF" - Vendor Refund,
		/// "QCK" - Quick Check, "VQC" - Void Quick Check.
		/// </value>
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(APInvoice.docType))]
		[APDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}

		/// <summary>
		/// [key] Reference number of the document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(APInvoice.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Optional<docType>>>>), Filterable = true)]
		public virtual String RefNbr
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
		[PXDBDate(BqlField = typeof(APInvoice.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// Status of the document. The field is calculated based on the values of status flag. It can't be changed directly.
		/// The fields tht determine status of a document are: <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>, 
		/// <see cref="Scheduled"/>, <see cref="Prebooked"/>, <see cref="Printed"/>, <see cref="Approved"/>, <see cref="Rejected"/>.
		/// </summary>
		/// <value>
		/// Possible values are: 
		/// <c>"H"</c> - Hold, <c>"B"</c> - Balanced, <c>"V"</c> - Voided, <c>"S"</c> - Scheduled, 
		/// <c>"N"</c> - Open, <c>"C"</c> - Closed, <c>"P"</c> - Printed, <c>"K"</c> - Prebooked,
		/// <c>"E"</c> - Pending Approval, <c>"R"</c> - Rejected, <c>"Z"</c> - Reserved.
		/// Defaults to Hold.
		/// </value>
		[PXDBString(1, IsFixed = true, BqlField = typeof(APInvoice.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[APDocStatus.List]
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
		[PXDBQuantity(BqlField = typeof(APTranSigned.signedBaseQty))]
		[PXUIField(DisplayName = "Billed Qty.", Enabled = false)]
		public virtual Decimal? TotalQty
		{
			get;
			set;
		}
		#endregion

		#region TotalTranAmt
		public abstract class totalTranAmt : PX.Data.BQL.BqlDecimal.Field<totalTranAmt>
		{
		}

		[PXDBBaseCury(BqlField = typeof(APTranSigned.signedCuryTranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalTranAmt
		{
			get;
			set;
		}
		#endregion

		#region TotalRetainageAmt
		public abstract class totalRetainageAmt : PX.Data.BQL.BqlDecimal.Field<totalRetainageAmt>
		{
		}

		[PXDBBaseCury(BqlField = typeof(APTranSigned.signedCuryRetainageAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalRetainageAmt
		{
			get;
			set;
		}
		#endregion

		#region TotalAmt
		public abstract class totalAmt : PX.Data.BQL.BqlDecimal.Field<totalAmt>
		{
		}

		/// <summary>
		/// Billed Amount of the item or service associated with the line.
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXBaseCury]
		[PXFormula(typeof(Add<totalTranAmt, totalRetainageAmt>))]
		[PXUIField(DisplayName = "Billed Amt.")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? TotalAmt
		{
			get;
			set;
		}
		#endregion

		#region TotalPPVAmt
		public abstract class totalPPVAmt : PX.Data.BQL.BqlDecimal.Field<totalPPVAmt>
		{
		}

		/// <summary>
		/// Purchase price variance amount associated with the line.
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
		/// </summary>
		/// <seealso cref="PX.Objects.PO.POReceiptLineR1.BillPPVAmt"/>
		[PXDBBaseCury(BqlField = typeof(APTranSigned.pOPPVAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "PPV Amt")]
		public virtual Decimal? TotalPPVAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}
		protected String _CuryID;

		/// <summary>
		/// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(APInvoice.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion

		#region POOrderType
		public abstract class pOOrderType : PX.Data.BQL.BqlString.Field<pOOrderType>
		{
		}

		/// <summary>
		/// The type of the corresponding <see cref="PX.Objects.PO.POOrder">PO Order</see>.
		/// Together with <see cref="PONbr"/> and <see cref="POLineNbr"/> links APTrans to the PO Orders and their lines.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.PO.POOrder.OrderType">POOrder.OrderType</see> field.
		/// See its description for the list of allowed values.
		/// </value>
		[PXDBString(2, IsFixed = true, IsKey = true, BqlField = typeof(APTranSigned.pOOrderType))]
		[POOrderType.List()]
		[PXUIField(DisplayName = "PO Type", Enabled = false, IsReadOnly = true)]
		public virtual String POOrderType
		{
			get;
			set;
		}
		#endregion

		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
		}

		/// <summary>
		/// The reference number of the corresponding <see cref="PX.Objects.PO.POOrder">PO Order</see>.
		/// Together with <see cref="POOrderType"/> and <see cref="POLineNbr"/> links APTrans to the PO Orders and their lines.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.PO.POOrder.OrderNbr">POOrder.OrderNbr</see> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(APTranSigned.pONbr))]
		[PXUIField(DisplayName = "PO Number", Enabled = false, IsReadOnly = true)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Optional<pOOrderType>>>>))]
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
	}
}
