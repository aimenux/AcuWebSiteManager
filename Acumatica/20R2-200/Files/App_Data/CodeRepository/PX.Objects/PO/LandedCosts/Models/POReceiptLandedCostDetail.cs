using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO.LandedCosts.Attributes;
using APInvoice = PX.Objects.AP.Standalone.APInvoice;
using APPayment = PX.Objects.AP.Standalone.APPayment;
using APRegister = PX.Objects.AP.Standalone.APRegister;

namespace PX.Objects.PO.LandedCosts
{
	[PXProjection(typeof(Select2<POLandedCostReceipt,
		InnerJoin<POLandedCostDoc, On<POLandedCostDoc.docType, Equal<POLandedCostReceipt.lCDocType>, And<POLandedCostDoc.refNbr, Equal<POLandedCostReceipt.lCRefNbr>>>,
		InnerJoin<POLandedCostDetail, On<POLandedCostDetail.docType, Equal<POLandedCostReceipt.lCDocType>, And<POLandedCostDetail.refNbr, Equal<POLandedCostReceipt.lCRefNbr>>>>>>), new []{ typeof(POLandedCostReceipt)}, Persistent = true)]
	[PXBreakInheritance]
	[PXCacheName(Messages.POLandedCostReceipt)]
	[Serializable]
	public class POReceiptLandedCostDetail : POLandedCostReceipt
	{
		#region LCDocType
		public new abstract class lCDocType : PX.Data.BQL.BqlString.Field<lCDocType>
		{
		}

		[POLandedCostDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Landed Cost Type", Visible = false)]
		public override String LCDocType
		{
			get;
			set;
		}
		#endregion
		#region LCRefNbr
		public new abstract class lCRefNbr : PX.Data.BQL.BqlString.Field<lCRefNbr>
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Landed Cost Nbr.")]
		[PXSelector(typeof(Search<POLandedCostDoc.refNbr, Where<POLandedCostDoc.docType, Equal<Current<lCDocType>>>>))]
		public override String LCRefNbr
		{
			get;
			set;
		}
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true, BqlField = typeof(POLandedCostDetail.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true, BqlField = typeof(POLandedCostDoc.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POLandedCostDocStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		/// <summary>
		/// Date of the document.
		/// </summary>
		[PXDBDate(BqlField = typeof(POLandedCostDoc.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[Vendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, BqlField = typeof(POLandedCostDoc.vendorID))]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POLandedCostDoc.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong(BqlField = typeof(POLandedCostDetail.curyInfoID))]
		[CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region LandedCostCodeID
		public abstract class landedCostCodeID : PX.Data.BQL.BqlString.Field<landedCostCodeID> { }

		[PXDBString(15, IsUnicode = true, IsFixed = false, BqlField = typeof(POLandedCostDetail.landedCostCodeID))]
		[PXUIField(DisplayName = "Landed Cost Code")]
		[PXSelector(typeof(Search<LandedCostCode.landedCostCodeID>))]
		public virtual String LandedCostCodeID
		{
			get;
			set;
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(150, IsUnicode = true, BqlField = typeof(POLandedCostDetail.descr))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get;
			set;
		}
		#endregion

		#region AllocationMethod
		public abstract class allocationMethod : PX.Data.BQL.BqlString.Field<allocationMethod> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(POLandedCostDetail.allocationMethod))]
		[LandedCostAllocationMethod.List()]
		[PXUIField(DisplayName = "Allocation Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String AllocationMethod
		{
			get;
			set;
		}
		#endregion

		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineAmt), BqlField = typeof(POLandedCostDetail.curyLineAmt))]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? CuryLineAmt
		{
			get;
			set;
		}
		#endregion

		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }

		[PXDBBaseCury(BqlField = typeof(POLandedCostDetail.lineAmt))]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion

		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }

		[PXDBString(3, IsFixed = true, BqlField = typeof(POLandedCostDetail.aPDocType))]
		[PXUIField(DisplayName = "AP Doc. Type", Enabled = false)]
		[APDocType.List()]
		public virtual String APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }

		[PXDBString(15, IsUnicode = true, BqlField = typeof(POLandedCostDetail.aPRefNbr))]
		[PXUIField(DisplayName = "AP Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<AP.APInvoice.refNbr, Where<AP.APInvoice.docType, Equal<Current<aPDocType>>>>))]
		public virtual String APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region INDocType
		public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(POLandedCostDetail.iNDocType))]
		[PXUIField(DisplayName = "IN Doc. Type", Enabled = false)]
		[INDocType.List()]
		public virtual String INDocType
		{
			get;
			set;
		}
		#endregion
		#region INRefNbr
		public abstract class iNRefNbr : PX.Data.BQL.BqlString.Field<iNRefNbr> { }

		[PXDBString(15, IsUnicode = true, BqlField = typeof(POLandedCostDetail.iNRefNbr))]
		[PXUIField(DisplayName = "IN Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<iNDocType>>>>))]
		public virtual String INRefNbr
		{
			get;
			set;
		}
		#endregion
	}
}
