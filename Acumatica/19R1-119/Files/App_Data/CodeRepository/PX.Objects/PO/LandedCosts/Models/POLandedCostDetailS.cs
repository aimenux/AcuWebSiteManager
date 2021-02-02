using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO.LandedCosts.Attributes;
using PX.Objects.SO;
using PX.Objects.TX;

namespace PX.Objects.PO
{
    [Serializable]
	[PXProjection(typeof(Select2<POLandedCostDetail, 
		InnerJoin<POLandedCostDoc, On<POLandedCostDetail.docType, Equal<POLandedCostDoc.docType>, And<POLandedCostDetail.refNbr, Equal<POLandedCostDoc.refNbr>>>>>), 
		Persistent = false)]
	/*[PXProjection(typeof(Select2<POLandedCostDetail, 
	InnerJoin<POLandedCostDoc, On<POLandedCostDetail.docType, Equal<POLandedCostDoc.docType>, And<POLandedCostDetail.refNbr, Equal<POLandedCostDoc.refNbr>>>, 
		LeftJoin<POLandedCostSplit, On<POLandedCostDetail.docType, Equal<POLandedCostSplit.docType>, 
			And<POLandedCostDetail.refNbr, Equal<POLandedCostSplit.refNbr>, And<POLandedCostDetail.lineNbr, Equal<POLandedCostSplit.detailLineNbr>>>>,
		LeftJoin<POLandedCostReceiptLine, On<POLandedCostSplit.docType, Equal<POLandedCostReceiptLine.docType>,
			And<POLandedCostSplit.refNbr, Equal<POLandedCostReceiptLine.refNbr>, And<POLandedCostSplit.receiptLineNbr, Equal<POLandedCostReceiptLine.lineNbr>>>>>>>>), 
	Persistent = false)]*/
	public class POLandedCostDetailS : PX.Data.IBqlTable, ISortOrder
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// [key] The type of the landed cost receipt line.
		/// </summary>
		/// <value>
		/// The field is determined by the type of the parent <see cref="POLandedCostDoc">document</see>.
		/// For the list of possible values see <see cref="POLandedCostDoc.DocType"/>.
		/// </value>
		[POLandedCostDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true, BqlField = typeof(POLandedCostDetail.docType))]
		[PXUIField(DisplayName = "Doc. Type")]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// [key] Reference number of the parent <see cref="POLandedCostDoc">document</see>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(POLandedCostDetail.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXParent(typeof(Select<POLandedCostDoc, Where<POLandedCostDoc.docType, Equal<Current<docType>>, And<POLandedCostDoc.refNbr, Equal<Current<refNbr>>>>>))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// [key] The number of the transaction line in the document.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the transactions belonging to a single document may include gaps.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(POLandedCostDetail.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.")]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
		{
			public const string DispalyName = "Line Order";
		}

		[PXDBInt(BqlField = typeof(POLandedCostDetail.sortOrder))]
		[PXUIField(DisplayName = sortOrder.DispalyName)]
		public virtual Int32? SortOrder
		{
			get;
			set;
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXUIField(DisplayName = "Currency")]
		[PXDBString(5, IsUnicode = true, BqlField = typeof(POLandedCostDoc.curyID))]
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
		[CurrencyInfo(CuryIDField = "CuryID")]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }

		[PXDBString(40, IsUnicode = true, BqlField = typeof(POLandedCostDoc.vendorRefNbr))]
		[PXUIField(DisplayName = "Vendor Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String VendorRefNbr
		{
			get;
			set;
		}
		#endregion

		#region LandedCostCodeID
		public abstract class landedCostCodeID : PX.Data.BQL.BqlString.Field<landedCostCodeID> { }

		/// <summary>
		/// The <see cref="PX.Objects.PO.LandedCostCode">landed cost code</see> used to describe the specific landed costs incurred for the line.
		/// This code is one of the codes associated with the vendor.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.PO.LandedCostCode.LandedCostCodeID">LandedCostCode.LandedCostCodeID</see> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, IsFixed = false, BqlField = typeof(POLandedCostDetail.landedCostCodeID))]
		[PXUIField(DisplayName = "Landed Cost Code")]
		[PXSelector(typeof(Search<Objects.PO.LandedCostCode.landedCostCodeID>))]
		public virtual String LandedCostCodeID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(150, IsUnicode = true, BqlField = typeof(POLandedCostDetail.descr))]
		[PXUIField(DisplayName = "Description")]
		[PXLocalizableDefault(typeof(Search<LandedCostCode.descr, Where<LandedCostCode.landedCostCodeID, Equal<Current<landedCostCodeID>>>>),
			typeof(Customer.localeName))]
		public virtual String Descr
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

		[PXDBDecimal(4, BqlField = typeof(POLandedCostDetail.lineAmt))]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

		/// <summary>
		/// Identifier of the <see cref="TaxCategory">tax category</see> associated with the line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// Defaults to the <see cref="InventoryItem.TaxCategoryID">tax category associated with the line item</see>.
		/// </value>
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLandedCostDetail.taxCategoryID))]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		public virtual String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.IN.InventoryItem">inventory item</see> associated with the transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.IN.InventoryItem.InventoryID">InventoryItem.InventoryID</see> field.
		/// </value>
		[APTranInventoryItem(Filterable = true, BqlField = typeof(POLandedCostDetail.inventoryID))]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		[SubItem(typeof(inventoryID), BqlField = typeof(POLandedCostDetail.subItemID))]
		public virtual Int32? SubItemID
		{
			get;
			set;
		}
		#endregion

		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }

		[PXDBString(3, IsFixed = true, BqlField = typeof(POLandedCostDetail.aPDocType))]
		[PXUIField(DisplayName = "AP Doc. Type")]
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
		[PXUIField(DisplayName = "AP Ref. Nbr.")]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Current<aPDocType>>>>))]

		public virtual String APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region INDocType
		public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(POLandedCostDetail.iNDocType))]
		[PXUIField(DisplayName = "IN Doc. Type")]
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
		[PXUIField(DisplayName = "IN Ref. Nbr.")]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<iNDocType>>>>))]
		public virtual String INRefNbr
		{
			get;
			set;
		}
		#endregion
	}
}
