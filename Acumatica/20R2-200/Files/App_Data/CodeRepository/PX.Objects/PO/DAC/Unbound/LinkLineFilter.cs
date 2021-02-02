using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
	public partial class LinkLineFilter : IBqlTable
	{
		#region POOrderNbr
			public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXSelector(typeof(Search5<POOrderRS.orderNbr,
			LeftJoin<LinkLineReceipt,
				On<POOrderRS.orderNbr, Equal<LinkLineReceipt.orderNbr>,
					And<POOrderRS.orderType, Equal<LinkLineReceipt.orderType>,
					And<Current<LinkLineFilter.selectedMode>, Equal<LinkLineFilter.selectedMode.receipt>>>>,
			LeftJoin<LinkLineOrder,
				On<POOrderRS.orderNbr, Equal<LinkLineOrder.orderNbr>,
					And<POOrderRS.orderType, Equal<LinkLineOrder.orderType>,
					And<Current<LinkLineFilter.selectedMode>, Equal<LinkLineFilter.selectedMode.order>>>>>>,
			Where2<
				Where<
					LinkLineReceipt.orderNbr, IsNotNull,
					Or<LinkLineOrder.orderType, IsNotNull>>,
				And<Where<
						POOrderRS.vendorID, Equal<Current<APInvoice.vendorID>>,
						And<POOrderRS.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>,
						And2<Not<FeatureInstalled<FeaturesSet.vendorRelations>>,
					Or2<FeatureInstalled<FeaturesSet.vendorRelations>,
						And<POOrderRS.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
						And<POOrderRS.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
						And<POOrderRS.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>>>>>>,
			Aggregate<
				GroupBy<POOrderRS.orderNbr,
				GroupBy<POOrderRS.orderType>>>>))]
		public virtual string POOrderNbr { get; set; }
		#endregion

		#region POReceiptNbr
			public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr>
		{
		}
		protected String _POReceiptNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Receipt Nbr.")]
		[PXSelector(typeof(Search<POReceipt.receiptNbr>))]
		public virtual String POReceiptNbr
		{
			get
			{
				return this._POReceiptNbr;
			}
			set
			{
				this._POReceiptNbr = value;
			}
		}
		#endregion

		#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
		{
		}
		protected Int32? _SiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", FieldClass = SiteAttribute.DimensionName)]
		[PXSelector(typeof(Search5<
			INSite.siteID
				, LeftJoin<LinkLineReceipt, On<INSite.siteID, Equal<LinkLineReceipt.receiptSiteID>, And<Current<LinkLineFilter.selectedMode>, Equal<LinkLineFilter.selectedMode.receipt>>>
					, LeftJoin<LinkLineOrder, On<INSite.siteID, Equal<LinkLineOrder.orderSiteID>, And<Current<LinkLineFilter.selectedMode>, Equal<LinkLineFilter.selectedMode.order>>>>
				>
			, Where<LinkLineReceipt.receiptSiteID, IsNotNull, Or<LinkLineOrder.orderSiteID, IsNotNull>>
			, Aggregate<GroupBy<INSite.siteID>>>), SubstituteKey = typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<LinkLineFilter.selectedMode>))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion

		#region selectedMode
			public abstract class selectedMode : PX.Data.BQL.BqlString.Field<selectedMode>
		{
			public const string Order = "O";
			public const string Receipt = "R";
			public const string LandedCost = "L";
				public class order : PX.Data.BQL.BqlString.Constant<order>
			{
				public order() : base(Order) { }
			}

				public class receipt : PX.Data.BQL.BqlString.Constant<receipt>
			{
				public receipt() : base(Receipt) { }
			}

				public class landedCost : PX.Data.BQL.BqlString.Constant<landedCost>
			{
				public landedCost() : base(LandedCost) { }
			}
		}
		protected String _SelectedMode;

		[PXDBString(1)]
		[PXFormula(typeof(Switch<Case<Where<Selector<inventoryID, InventoryItem.stkItem>, NotEqual<True>, And<Selector<inventoryID, InventoryItem.nonStockReceipt>, NotEqual<True>>>, selectedMode.order>, selectedMode.receipt>))]
		[PXUIField(DisplayName = "Selected Mode")]
		[PXStringList(new[] { selectedMode.Order, selectedMode.Receipt, selectedMode.LandedCost }, new[] { AP.Messages.POOrderMode, AP.Messages.POReceiptMode, AP.Messages.POLandedCostMode })]
		public virtual String SelectedMode
		{
			get
			{
				return this._SelectedMode;
			}
			set
			{
				this._SelectedMode = value;
			}
		}
		#endregion
		#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;

		[Inventory(Enabled = false)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		protected String _UOM;

		[INUnit(typeof(inventoryID), Enabled = false)]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
	}
}
