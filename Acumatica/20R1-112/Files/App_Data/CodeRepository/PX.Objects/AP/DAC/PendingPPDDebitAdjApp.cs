using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	[PXProjection(typeof(Select2<APAdjust,
	InnerJoin<AP.APInvoice, On<AP.APInvoice.docType, Equal<APAdjust.adjdDocType>, And<AP.APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>,
	Where<AP.APInvoice.released, Equal<True>,
		And<AP.APInvoice.pendingPPD, Equal<True>,
		And<AP.APInvoice.openDoc, Equal<True>,
		And<APAdjust.released, Equal<True>,
		And<APAdjust.voided, NotEqual<True>,
		And<APAdjust.pendingPPD, Equal<True>,
		And<APAdjust.pPDDebitAdjRefNbr, IsNull>>>>>>>>))]
	[Serializable]
	public partial class PendingPPDDebitAdjApp : APAdjust
	{
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		#endregion
		#region Index
		public abstract class index : PX.Data.BQL.BqlInt.Field<index> { }
		[PXInt]
		public virtual int Index
		{
			get; set;
		}
		#endregion

		#region APAdjust key fields

		#region PayDocType
		public abstract class payDocType : PX.Data.BQL.BqlString.Field<payDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(APAdjust.adjgDocType))]
		public virtual string PayDocType
		{
			get; set;
		}
		#endregion
		#region PayRefNbr
		public abstract class payRefNbr : PX.Data.BQL.BqlString.Field<payRefNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(APAdjust.adjgRefNbr))]
		public virtual string PayRefNbr
		{
			get; set;
		}
		#endregion
		#region InvDocType
		public abstract class invDocType : PX.Data.BQL.BqlString.Field<invDocType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(APAdjust.adjdDocType))]
		public virtual string InvDocType
		{
			get; set;
		}
		#endregion
		#region InvRefNbr
		public abstract class invRefNbr : PX.Data.BQL.BqlString.Field<invRefNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(APAdjust.adjdRefNbr))]
		public virtual string InvRefNbr
		{
			get; set;
		}
		#endregion
		#region PPDAdjNbr
		public abstract class ppdAdjNbr : PX.Data.BQL.BqlInt.Field<ppdAdjNbr> { }
		[PXDBInt(IsKey = true, BqlField = typeof(APAdjust.adjNbr))]
		public virtual Int32? PPDAdjNbr
		{
			get; set;
		}
		#endregion
		#endregion
		#region AP.APInvoice fields

		#region InvCuryID
		public abstract class invCuryID : PX.Data.BQL.BqlString.Field<invCuryID> { }
		protected string _InvCuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(AP.APInvoice.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string InvCuryID
		{
			get
			{
				return _InvCuryID;
			}
			set
			{
				_InvCuryID = value;
			}
		}
		#endregion

		#region InvCuryInfoID
		public abstract class invCuryInfoID : PX.Data.BQL.BqlLong.Field<invCuryInfoID> { }
		[PXDBLong(BqlField = typeof(AP.APInvoice.curyInfoID))]
		public virtual long? InvCuryInfoID { get; set; }
		#endregion

		#region InvVendorLocationID
		public abstract class invVendorLocationID : PX.Data.BQL.BqlInt.Field<invVendorLocationID> { }
		protected int? _InvVendorLocationID;
		[PXDBInt(BqlField = typeof(AP.APInvoice.vendorLocationID))]
		public virtual int? InvVendorLocationID
		{
			get
			{
				return _InvVendorLocationID;
			}
			set
			{
				_InvVendorLocationID = value;
			}
		}
		#endregion
		#region InvTaxZoneID
		public abstract class invTaxZoneID : PX.Data.BQL.BqlString.Field<invTaxZoneID> { }
		protected string _InvTaxZoneID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(AP.APInvoice.taxZoneID))]
		public virtual string InvTaxZoneID
		{
			get
			{
				return _InvTaxZoneID;
			}
			set
			{
				_InvTaxZoneID = value;
			}
		}
		#endregion
		#region InvTaxCalcMode
		public abstract class invTaxCalcMode : PX.Data.BQL.BqlString.Field<invTaxCalcMode> { }
		[PXDBString(10, IsUnicode = true, BqlField = typeof(AP.APInvoice.taxCalcMode))]
		public virtual string InvTaxCalcMode { get; set; }
		#endregion
		#region InvTermsID
		public abstract class invTermsID : PX.Data.BQL.BqlString.Field<invTermsID> { }
		protected string _InvTermsID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(AP.APInvoice.termsID))]
		[PXUIField(DisplayName = "Credit Terms", Visibility = PXUIVisibility.Visible)]
		public virtual string InvTermsID
		{
			get
			{
				return _InvTermsID;
			}
			set
			{
				_InvTermsID = value;
			}
		}
		#endregion
		#region InvCuryOrigDocAmt
		public abstract class invCuryOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<invCuryOrigDocAmt> { }
		protected decimal? _InvCuryOrigDocAmt;
		[PXDBCurrency(typeof(AP.APInvoice.curyInfoID), typeof(AP.APInvoice.origDocAmt), BqlField = typeof(AP.APInvoice.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InvCuryOrigDocAmt
		{
			get
			{
				return _InvCuryOrigDocAmt;
			}
			set
			{
				_InvCuryOrigDocAmt = value;
			}
		}
		#endregion
		#region InvCuryOrigDiscAmt
		public abstract class invCuryOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<invCuryOrigDiscAmt> { }
		protected decimal? _InvCuryOrigDiscAmt;
		[PXDBCurrency(typeof(AP.APInvoice.curyInfoID), typeof(AP.APInvoice.origDiscAmt), BqlField = typeof(AP.APInvoice.curyOrigDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? InvCuryOrigDiscAmt
		{
			get
			{
				return _InvCuryOrigDiscAmt;
			}
			set
			{
				_InvCuryOrigDiscAmt = value;
			}
		}
		#endregion
		#region InvCuryVatTaxableTotal
		public abstract class invCuryVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<invCuryVatTaxableTotal> { }
		protected decimal? _InvCuryVatTaxableTotal;
		[PXDBCurrency(typeof(AP.APInvoice.curyInfoID), typeof(AP.APInvoice.vatTaxableTotal), BqlField = typeof(AP.APInvoice.curyVatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InvCuryVatTaxableTotal
		{
			get
			{
				return _InvCuryVatTaxableTotal;
			}
			set
			{
				_InvCuryVatTaxableTotal = value;
			}
		}
		#endregion
		#region InvCuryTaxTotal
		public abstract class invCuryTaxTotal : PX.Data.BQL.BqlDecimal.Field<invCuryTaxTotal> { }
		protected decimal? _InvCuryTaxTotal;
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(AP.APInvoice.taxTotal), BqlField = typeof(AP.APInvoice.curyTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InvCuryTaxTotal
		{
			get
			{
				return _InvCuryTaxTotal;
			}
			set
			{
				_InvCuryTaxTotal = value;
			}
		}
		#endregion
		#region InvCuryDocBal
		public abstract class invCuryDocBal : PX.Data.BQL.BqlDecimal.Field<invCuryDocBal> { }
		protected decimal? _InvCuryDocBal;
		[PXDBCurrency(typeof(AP.APInvoice.curyInfoID), typeof(AP.APInvoice.docBal), BaseCalc = false, BqlField = typeof(AP.APInvoice.curyDocBal))]
		public virtual decimal? InvCuryDocBal
		{
			get
			{
				return _InvCuryDocBal;
			}
			set
			{
				_InvCuryDocBal = value;
			}
		}
		#endregion

		#endregion
	}
}
