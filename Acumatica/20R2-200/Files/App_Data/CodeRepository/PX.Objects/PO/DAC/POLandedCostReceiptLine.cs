using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.PO
{
    [Serializable]
	[PXCacheName(Messages.POLandedCostReceiptLine)]
	public class POLandedCostReceiptLine : PX.Data.IBqlTable, ISortOrder
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLandedCostReceiptLine>.By<docType, refNbr, lineNbr>
		{
			public static POLandedCostReceiptLine Find(PXGraph graph, string docType, string refNbr, int? lineNbr) => FindBy(graph, docType, refNbr, lineNbr);
		}
		public static class FK
		{
			public class LandedCostDoc : POLandedCostDoc.PK.ForeignKeyOf<POLandedCostReceiptLine>.By<docType, refNbr> { }
			public class LandedCostReceipt : POLandedCostReceipt.PK.ForeignKeyOf<POLandedCostReceiptLine>.By<docType, refNbr, pOReceiptType, pOReceiptNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POLandedCostReceiptLine>.By<inventoryID> { }
			public class Receipt : POReceipt.PK.ForeignKeyOf<POLandedCostReceiptLine>.By<pOReceiptNbr> { }
			public class ReceiptLine : POReceiptLine.PK.ForeignKeyOf<POLandedCostReceiptLine>.By<pOReceiptNbr, pOReceiptLineNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<POLandedCostReceiptLine>.By<siteID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Branch">Branch</see>, to which the transaction belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID">Branch.BranchID</see> field.
		/// </value>
		[Branch(typeof(POLandedCostDoc.branchID), Enabled = false)]
		public virtual Int32? BranchID
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
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POLandedCostDoc.docType))]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(POLandedCostDoc.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.LandedCostDoc))]
		[PXParent(typeof(FK.LandedCostReceipt), LeaveChildren = true, ParentCreate = true)]
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
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(POLandedCostDoc.lineCntr))]
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

		[PXDBInt]
		[PXUIField(DisplayName = POLandedCostReceiptLine.sortOrder.DispalyName, Visible = false, Enabled = false)]
		public virtual Int32? SortOrder
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
		[Inventory(Filterable = true, Enabled = false)]
		[PXForeignReference(typeof(FK.InventoryItem))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
				Where<InventoryItem.inventoryID, Equal<Current<POLandedCostReceiptLine.inventoryID>>,
					And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[SubItem(typeof(POLandedCostReceiptLine.inventoryID), Enabled = false)]
		public virtual Int32? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Description", Visibility = PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(POLandedCostDoc.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region POReceiptType
		public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "PO Receipt Type", Visible = false, Enabled = false)]
		public virtual String POReceiptType
		{
			get;
			set;
		}
		#endregion
		#region POReceiptNbr
		public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<pOReceiptType>>>>))]
		[PXFormula(null, typeof(CountCalc<POLandedCostReceipt.lineCntr>))]
		public virtual String POReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region POReceiptLineNbr
		public abstract class pOReceiptLineNbr : PX.Data.BQL.BqlInt.Field<pOReceiptLineNbr> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "PO Receipt Line Nbr.", Enabled = false)]
		public virtual Int32? POReceiptLineNbr
		{
			get;
			set;
		}
		#endregion
		#region POReceiptCuryID
		public abstract class pOReceiptBaseCuryID : PX.Data.BQL.BqlString.Field<pOReceiptBaseCuryID> { }

		/// <summary>
		/// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the line.
		/// </summary>
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String POReceiptBaseCuryID
		{
			get;
			set;
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		[POSiteAvail(typeof(inventoryID), typeof(subItemID), Enabled = false)]
		[PXDefault(typeof(Coalesce<Search<Location.vSiteID,
				Where<Location.locationID, Equal<Current<POLandedCostDoc.vendorLocationID>>,
					And<Location.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>>>>,
			Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.Site))]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }


		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(POLine.inventoryID), DisplayName = "UOM", Enabled = false)]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region ReceiptQty
		public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty> { }

		[PXDBQuantity(typeof(uOM), typeof(baseReceiptQty), HandleEmptyKey = true, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReceiptQty
		{
			get;
			set;
		}
		#endregion
		#region BaseReceiptQty
		public abstract class baseReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseReceiptQty> { }

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Receipt Qty.", Visible = false, Enabled = false)]
		public virtual Decimal? BaseReceiptQty
		{
			get;
			set;
		}
		#endregion
		#region UnitWeight
		public abstract class unitWeight : PX.Data.BQL.BqlDecimal.Field<unitWeight> { }

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseWeight, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>, And<InventoryItem.baseWeight, IsNotNull>>>))]
		[PXUIField(DisplayName = "Unit Weight")]
		public virtual Decimal? UnitWeight
		{
			get;
			set;
		}
		#endregion
		#region UnitVolume
		public abstract class unitVolume : PX.Data.BQL.BqlDecimal.Field<unitVolume> { }

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseVolume, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>, And<InventoryItem.baseVolume, IsNotNull>>>))]
		public virtual Decimal? UnitVolume
		{
			get;
			set;
		}
		#endregion
		#region ExtWeight
		public abstract class extWeight : PX.Data.BQL.BqlDecimal.Field<extWeight> { }

		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<Row<baseReceiptQty, receiptQty>, unitWeight>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtWeight
		{
			get;
			set;
		}
		#endregion
		#region ExtVolume
		public abstract class extVolume : PX.Data.BQL.BqlDecimal.Field<extVolume> { }

		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Volume", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<Row<baseReceiptQty, receiptQty>, unitVolume>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtVolume
		{
			get;
			set;
		}
		#endregion

		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }

		[PXDBBaseCury]
		[PXUIField(DisplayName = "Ext. Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryAllocatedLCAmt
		public abstract class curyAllocatedLCAmt : PX.Data.BQL.BqlDecimal.Field<curyAllocatedLCAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(allocatedLCAmt))]
		[PXUIField(DisplayName = "Allocated Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAllocatedLCAmt
		{
			get;
			set;
		}
		#endregion
		#region AllocatedLCAmt
		public abstract class allocatedLCAmt : PX.Data.BQL.BqlDecimal.Field<allocatedLCAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllocatedLCAmt
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
