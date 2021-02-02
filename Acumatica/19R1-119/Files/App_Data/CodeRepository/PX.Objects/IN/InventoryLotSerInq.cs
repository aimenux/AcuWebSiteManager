using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.RQ;
using System.Collections;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.SO;

namespace PX.Objects.IN
{
	[PX.Objects.GL.TableAndChartDashboardType]    
    [Serializable]
	public class InventoryLotSerInq : PXGraph<InventoryLotSerInq>
	{		
		public PXFilter<INLotSerFilter> Filter;
		public PXCancel<INLotSerFilter> Cancel;
		[PXFilterable]
		public PXSelectJoin<INTranSplit,
							InnerJoin<InventoryItem,
                                         On<InventoryItem.inventoryID, Equal<INTranSplit.tranInventoryID>,
										 And<Match<IN.InventoryItem,Current<AccessInfo.userName>>>>,
                            LeftJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<INTranSplit.pOReceiptNbr>,
                                And<POReceiptLine.lineNbr, Equal<INTranSplit.pOReceiptLineNbr>>>,
							LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POReceiptLine.vendorID>>,
                            LeftJoin<SOLine, On<SOLine.orderType, Equal<INTranSplit.sOOrderType>,
                                And<SOLine.orderNbr, Equal<INTranSplit.sOOrderNbr>,
                                And<SOLine.lineNbr, Equal<INTranSplit.sOOrderLineNbr>>>>,
							LeftJoin<Customer, On<Customer.bAccountID, Equal<SOLine.customerID>>,
							InnerJoin<INSite,
										 On2<INTranSplit.FK.Site,
										 And<Match<IN.INSite, Current<AccessInfo.userName>>>>>>>>>>,
							Where<Current<INLotSerFilter.lotSerialNbr>, IsNotNull,
							 And2<Where<Current<INLotSerFilter.startDate>, IsNull,
                                            Or<INTranSplit.tranTranDate, GreaterEqual<Current<INLotSerFilter.startDate>>>>,
								And<Where<Current<INLotSerFilter.endDate>, IsNull,
                                            Or<INTranSplit.tranTranDate, LessEqual<Current<INLotSerFilter.endDate>>>>>>>> Records;

		public InventoryLotSerInq()
		{
			Records.Cache.AllowInsert = false;
			Records.Cache.AllowUpdate = false;
			Records.Cache.AllowDelete = false;
			
			Records.WhereAndCurrent<INLotSerFilter>();
			PXUIFieldAttribute.SetVisible<INTranSplit.sOOrderType>(Records.Cache, null, true);
			PXUIFieldAttribute.SetVisible<INTranSplit.sOOrderNbr>(Records.Cache, null, true);
			PXCache receipt = this.Caches[typeof(POReceiptLine)];
			PXUIFieldAttribute.SetVisible<POReceiptLine.receiptType>(receipt, null, true);
			PXUIFieldAttribute.SetVisible<POReceiptLine.receiptNbr>(receipt, null, true);
			PXUIFieldAttribute.SetDisplayName<Customer.acctCD>(this.Caches[typeof(Customer)], Messages.CustomerID);
			PXUIFieldAttribute.SetDisplayName<POReceiptLine.receiptType>(receipt, Messages.ReceiptType);
			PXUIFieldAttribute.SetDisplayName<POReceiptLine.receiptNbr>(receipt, Messages.ReceiptNbr);
		}
		protected virtual void INLotSerFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerFilter filter = (INLotSerFilter)e.Row;
			if (filter != null)
			{
				PXUIFieldAttribute.SetVisible<INTranSplit.tranInventoryID>(Records.Cache, null, filter.InventoryID == null);
				PXUIFieldAttribute.SetVisible<INTranSplit.siteID>(Records.Cache, null, filter.SiteID == null);
				PXUIFieldAttribute.SetVisible<INTranSplit.locationID>(Records.Cache, null, filter.LocationID == null);
			}
		}

        protected virtual void INTranSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            INTranSplit row = (INTranSplit)e.Row;
            INLotSerFilter filter = Filter.Current;
            if (row == null || filter == null) return;

            if(filter.ShowAdjUnitCost ?? false)
            {
                row.TranUnitCost = row.TotalQty == null || row.TotalQty == 0m ? 0m : (row.TotalCost + row.AdditionalCost) / row.TotalQty;
            }
            else
            {
                row.TranUnitCost = row.TotalQty == null || row.TotalQty == 0m ? 0m : (row.TotalCost) / row.TotalQty;
            }
        }

        protected virtual void INLotSerFilter_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{			
			DateTime businessDate = (DateTime)this.Accessinfo.BusinessDate;
			e.NewValue = new DateTime(businessDate.Year, 1, 1);
			e.Cancel = true;			
		}

		#region View Actions		
		public PXAction<INLotSerFilter> viewSummary;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventorySummary)]
		protected virtual IEnumerable ViewSummary(PXAdapter a)
		{
			if (this.Records.Current != null)
			{
				PXSegmentedState subItem =
					this.Records.Cache.GetValueExt<INTranSplit.subItemID>
					(this.Records.Current) as PXSegmentedState;
				InventorySummaryEnq.Redirect(
					this.Records.Current.InventoryID,
					subItem != null ? (string)subItem.Value : null,
					this.Records.Current.SiteID,
					this.Records.Current.LocationID, false);
			}
			return a.Get();
		}

		public PXAction<INLotSerFilter> viewAllocDet;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventoryAllocDet)]
		protected virtual IEnumerable ViewAllocDet(PXAdapter a)
		{
			if (this.Records.Current != null)
			{
				PXSegmentedState subItem =
					this.Records.Cache.GetValueExt<INTranSplit.subItemID>
					(this.Records.Current) as PXSegmentedState;
				InventoryAllocDetEnq.Redirect(
					this.Records.Current.InventoryID,
					subItem != null ? (string)subItem.Value : null,
					this.Records.Current.LotSerialNbr,
					this.Records.Current.SiteID,
					this.Records.Current.LocationID);
			}
			return a.Get();
		}

		#endregion

        [Serializable]
		public partial class POReceiptLine : IBqlTable
		{
			#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
			protected String _ReceiptNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Receipt Nbr.", Visible = true)]
			[POReceiptType.RefNbr(typeof(Search2<POReceipt.receiptNbr,
				InnerJoinSingleTable<Vendor, On<POReceipt.vendorID, Equal<Vendor.bAccountID>>>,
			Where<POReceipt.receiptType, Equal<Optional<INTranSplit.pOReceiptType>>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<POReceipt.receiptNbr>>>), Filterable = true)]
			public virtual String ReceiptNbr
			{
				get
				{
					return this._ReceiptNbr;
				}
				set
				{
					this._ReceiptNbr = value;
				}
			}
			#endregion
			#region ReceiptType
			public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
			protected String _ReceiptType;
			[PXDBString(2, IsFixed = true)]
			[POReceiptType.List()]
			[PXDBDefault(typeof(POReceipt.receiptType))]
			[PXUIField(DisplayName = "Receipt Type", Visible = true)]
			public virtual String ReceiptType
			{
				get
				{
					return this._ReceiptType;
				}
				set
				{
					this._ReceiptType = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]			
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[Vendor()]
			[PXDBDefault(typeof(POReceipt.vendorID))]
			public virtual Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
		}
        [Serializable]
		public partial class SOLine : IBqlTable
		{
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			protected String _OrderType;
			[PXDBString(2, IsKey = true, IsFixed = true)]
			[PXDefault(typeof(SOOrder.orderType))]
			[PXUIField(DisplayName = "Order Type", Visible = true)]
			public virtual String OrderType
			{
				get
				{
					return this._OrderType;
				}
				set
				{
					this._OrderType = value;
				}
			}
			#endregion
			#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			protected String _OrderNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]			
			[PXUIField(DisplayName = "Order Nbr.", Visible = true)]
			[SO.SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
				InnerJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>,
				Where<SOOrder.orderType, Equal<Optional<INTranSplit.sOOrderType>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>, OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]
			public virtual String OrderNbr
			{
				get
				{
					return this._OrderNbr;
				}
				set
				{
					this._OrderNbr = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true)]
			[PXLineNbr(typeof(SOOrder.lineCntr))]
			[PXUIField(DisplayName = "Line Nbr.", Visible = true)]
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXDBInt()]
			[PXDefault(typeof(SOOrder.customerID))]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
				}
			}
			#endregion
		}
		[System.SerializableAttribute()]
        [PXProjection(typeof(Select2<INTranSplit, InnerJoin<INTran, On<INTranSplit.FK.Tran>>>), Persistent = false)]
		public partial class INTranSplit : PX.Data.IBqlTable, ILSDetail
		{
			#region Keys
			public class PK : PrimaryKeyOf<INTranSplit>.By<docType, refNbr, lineNbr, splitLineNbr>
			{
				public INTranSplit Find(PXGraph graph, string docType, string refNbr, int? lineNbr, int? splitLineNbr) 
					=> FindBy(graph, docType, refNbr, lineNbr, splitLineNbr);
			}
			public static class FK
			{
				public class Register : INRegister.PK.ForeignKeyOf<INTranSplit>.By<docType, refNbr> { }
				public class Tran : INTran.PK.ForeignKeyOf<INTranSplit>.By<docType, refNbr, lineNbr> { }
				public class Inventory : InventoryItem.PK.ForeignKeyOf<INTranSplit>.By<inventoryID> { }
				public class Site : INSite.PK.ForeignKeyOf<INTranSplit>.By<siteID> { }
			}
			#endregion
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool()]		
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			protected String _DocType;
			[PXDBString(1, IsFixed = true, IsKey = true)]			
			[PXUIField(DisplayName = "Doc. Type", Visible = false)]
			public virtual String DocType
			{
				get
				{
					return this._DocType;
				}
				set
				{
					this._DocType = value;
				}
			}
			#endregion
			#region TranType
			public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			protected String _TranType;
			[PXDBString(3, IsFixed = true)]
			[INTranType.List()]
			[PXUIField(DisplayName = "Tran. Type")]
			public virtual String TranType
			{
				get
				{
					return this._TranType;
				}
				set
				{
					this._TranType = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			protected String _RefNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDBDefault(typeof(INRegister.refNbr))]
			[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<docType>>>>))]
			[PXUIField(DisplayName = "Reference Nbr.")]
			public virtual String RefNbr
			{
				get
				{
					return this._RefNbr;
				}
				set
				{
					this._RefNbr = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true)]			
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region SplitLineNbr
			public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
			protected Int32? _SplitLineNbr;
			[PXDBInt(IsKey = true)]
			[PXLineNbr(typeof(INRegister.lineCntr))]
			public virtual Int32? SplitLineNbr
			{
				get
				{
					return this._SplitLineNbr;
				}
				set
				{
					this._SplitLineNbr = value;
				}
			}
			#endregion
			#region TranDate
			public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
			protected DateTime? _TranDate;
			[PXDBDate()]
			[PXDBDefault(typeof(INRegister.tranDate))]
			[PXUIField(DisplayName = "Tran. Date")]
			public virtual DateTime? TranDate
			{
				get
				{
					return this._TranDate;
				}
				set
				{
					this._TranDate = value;
				}
			}
			#endregion
			#region InvtMult
			public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
			protected Int16? _InvtMult;
			[PXDBShort()]
			public virtual Int16? InvtMult
			{
				get
				{
					return this._InvtMult;
				}
				set
				{
					this._InvtMult = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[StockItem(Visible = true, DisplayName="Inventory ID", BqlField = typeof(INTranSplit.inventoryID))]			
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
			#region IsStockItem
			public bool? IsStockItem
			{
				get { return true; }
				set { }
			}
			#endregion
			#region SubItemID
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			protected Int32? _SubItemID;
			[IN.SubItem(
				typeof(INTranSplit.inventoryID),
				typeof(LeftJoin<INSiteStatus,
					On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
					And<INSiteStatus.inventoryID, Equal<Optional<INTranSplit.inventoryID>>,
					And<INSiteStatus.siteID, Equal<Optional<INTranSplit.siteID>>>>>>), BqlField = typeof(INTranSplit.subItemID))]			
			public virtual Int32? SubItemID
			{
				get
				{
					return this._SubItemID;
				}
				set
				{
					this._SubItemID = value;
				}
			}
			#endregion
			#region CostSubItemID
			public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
			protected Int32? _CostSubItemID;
			[PXInt()]
			public virtual Int32? CostSubItemID
			{
				get
				{
					return this._CostSubItemID;
				}
				set
				{
					this._CostSubItemID = value;
				}
			}
			#endregion
			#region CostSiteID
			public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
			protected Int32? _CostSiteID;
			[PXInt()]
			public virtual Int32? CostSiteID
			{
				get
				{
					return this._CostSiteID;
				}
				set
				{
					this._CostSiteID = value;
				}
			}
			#endregion
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[IN.Site()]			
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
			#region LocationID
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			protected Int32? _LocationID;
			[IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]						
			public virtual Int32? LocationID
			{
				get
				{
					return this._LocationID;
				}
				set
				{
					this._LocationID = value;
				}
			}
			#endregion
			#region LotSerialNbr
			public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			protected String _LotSerialNbr;
			[PXDBString(100, IsUnicode = true)]
			public virtual String LotSerialNbr
			{
				get
				{
					return this._LotSerialNbr;
				}
				set
				{
					this._LotSerialNbr = value;
				}
			}
			#endregion
			#region LotSerClassID
			public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
			protected String _LotSerClassID;
			[PXString(10, IsUnicode = true)]
			public virtual String LotSerClassID
			{
				get
				{
					return this._LotSerClassID;
				}
				set
				{
					this._LotSerClassID = value;
				}
			}
			#endregion
			#region AssignedNbr
			public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
			protected String _AssignedNbr;
			[PXString(30, IsUnicode = true)]
			public virtual String AssignedNbr
			{
				get
				{
					return this._AssignedNbr;
				}
				set
				{
					this._AssignedNbr = value;
				}
			}
			#endregion
			#region ExpireDate
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			protected DateTime? _ExpireDate;
			[INExpireDate(typeof(INTranSplit.inventoryID))]
			public virtual DateTime? ExpireDate
			{
				get
				{
					return this._ExpireDate;
				}
				set
				{
					this._ExpireDate = value;
				}
			}
			#endregion
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			protected Boolean? _Released;
			[PXDBBool()]
			[PXUIField(DisplayName = "Released")]
			public virtual Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
			#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
			protected String _UOM;
			[INUnit(typeof(INTranSplit.inventoryID), DisplayName = "UOM", Enabled = false)]			
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
			#region Qty
			public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			protected Decimal? _Qty;
			[PXDBQuantity(typeof(INTranSplit.uOM), typeof(INTranSplit.baseQty))]						
			public virtual Decimal? Qty
			{
				get
				{
					return this._Qty;
				}
				set
				{
					this._Qty = value;
				}
			}
			#endregion
			#region BaseQty
			public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
			protected Decimal? _BaseQty;
			[PXDBQuantity()]
			public virtual Decimal? BaseQty
			{
				get
				{
					return this._BaseQty;
				}
				set
				{
					this._BaseQty = value;
				}
			}
			#endregion
			#region InvQty
			public abstract class invQty : PX.Data.BQL.BqlDecimal.Field<invQty> { }
			protected Decimal? _InvQty;
			[PXDBCalced(typeof(Mult<INTranSplit.qty, INTranSplit.invtMult>), typeof(decimal))]
			[PXQuantity()]
			[PXUIField(DisplayName = "Quantity")]			
			public virtual Decimal? InvQty
			{
				get
				{
					return this._InvQty;
				}
				set
				{
					this._InvQty = value;
				}
			}
			#endregion
			#region InvBaseQty
			public abstract class invBaseQty : PX.Data.BQL.BqlDecimal.Field<invBaseQty> { }
			protected Decimal? _InvBaseQty;
			[PXDBCalced(typeof(Mult<INTranSplit.baseQty, INTranSplit.invtMult>), typeof(decimal))]
			[PXQuantity()]
			[PXUIField(DisplayName = "Quantity")]			
			public virtual Decimal? InvBaseQty
			{
				get
				{
					return this._InvBaseQty;
				}
				set
				{
					this._InvBaseQty = value;
				}
			}
			#endregion
			#region PlanID
			public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
			protected Int64? _PlanID;
			[PXDBLong()]
			[INTranSplitPlanID(typeof(INRegister.noteID), typeof(INRegister.hold), typeof(INRegister.transferType))]
			public virtual Int64? PlanID
			{
				get
				{
					return this._PlanID;
				}
				set
				{
					this._PlanID = value;
				}
			}
			#endregion		
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
			[PXFormula(typeof(Selector<INTranSplit.locationID, INLocation.projectID>))]
			[PXInt]
			public virtual Int32? ProjectID
			{
				get
				{
					return this._ProjectID;
				}
				set
				{
					this._ProjectID = value;
				}
			}
			#endregion
			#region TaskID
			public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
			protected Int32? _TaskID;
			[PXFormula(typeof(Selector<INTranSplit.locationID, INLocation.taskID>))]
			[PXInt]
			public virtual Int32? TaskID
			{
				get
				{
					return this._TaskID;
				}
				set
				{
					this._TaskID = value;
				}
			}
            #endregion

            #region TotalQty
            public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
            [PXDBDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(Visible = false)]
            public virtual Decimal? TotalQty
            {
                get;
                set;
            }
            #endregion
            #region TotalCost
            public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
            [PXDBDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(Visible = false)]
            public virtual Decimal? TotalCost
            {
                get;
                set;
            }
            #endregion
            #region AdditionalCost
            public abstract class additionalCost : PX.Data.BQL.BqlDecimal.Field<additionalCost> { }
            protected Decimal? _AdditionalCost;
            [PXDBDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(Visible = false)]
            public virtual Decimal? AdditionalCost
            {
                get
                {
                    return this._AdditionalCost;
                }
                set
                {
                    this._AdditionalCost = value;
                }
            }
            #endregion

            #region TranUnitCost
            public abstract class tranUnitCost : PX.Data.BQL.BqlDecimal.Field<tranUnitCost> { }
            protected Decimal? _TranUnitCost;
            [CM.PXBaseCury(MinValue = 0)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Unit Cost")]
            public virtual Decimal? TranUnitCost
            {
                get
                {
                    return this._TranUnitCost;
                }
                set
                {
                    this._TranUnitCost = value;
                }
            }
            #endregion


            #region TranInventoryID
            public abstract class tranInventoryID : PX.Data.BQL.BqlInt.Field<tranInventoryID> { }
            protected Int32? _TranInventoryID;
            [PXDefault()]
            [StockItem(DisplayName = "Inventory ID", BqlField = typeof(INTran.inventoryID))]
            public virtual Int32? TranInventoryID
            {
                get
                {
                    return this._TranInventoryID;
                }
                set
                {
                    this._TranInventoryID = value;
                }
            }
            #endregion

            #region TranBaseQty
            public abstract class tranBaseQty : PX.Data.BQL.BqlDecimal.Field<tranBaseQty> { }
            protected Decimal? _TranBaseQty;
            [PXDBQuantity(BqlField = typeof(INTran.baseQty))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? TranBaseQty
            {
                get
                {
                    return this._TranBaseQty;
                }
                set
                {
                    this._TranBaseQty = value;
                }
            }
            #endregion
            #region SOOrderType
            public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
            protected String _SOOrderType;
            [PXDBString(2, IsFixed = true, BqlField = typeof(INTran.sOOrderType))]
            [PXUIField(DisplayName = "SO Order Type", Visible = false)]
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
            public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
            protected String _SOOrderNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(INTran.sOOrderNbr))]
			[PXUIField(DisplayName = "Order Nbr.", Visible = false, Enabled = false)]
			[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<sOOrderType>>>>))]
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
            #region SOOrderLineNbr
            public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
            protected Int32? _SOOrderLineNbr;
            [PXDBInt(BqlField = typeof(INTran.sOOrderLineNbr))]
            public virtual Int32? SOOrderLineNbr
            {
                get
                {
                    return this._SOOrderLineNbr;
                }
                set
                {
                    this._SOOrderLineNbr = value;
                }
            }
            #endregion

            #region POReceiptType
            public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }
            protected String _POReceiptType;
            [PXDBString(2, IsFixed = true, BqlField = typeof(INTran.pOReceiptType))]
            [PXUIField(DisplayName = "PO Receipt Type", Visible = false, Enabled = false)]
            public virtual String POReceiptType
            {
                get
                {
                    return this._POReceiptType;
                }
                set
                {
                    this._POReceiptType = value;
                }
            }
            #endregion
            #region POReceiptNbr
            public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }
            protected String _POReceiptNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(INTran.pOReceiptNbr))]
            [PXUIField(DisplayName = "PO Receipt Nbr.", Visible = false, Enabled = false)]
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
            #region POReceiptLineNbr
            public abstract class pOReceiptLineNbr : PX.Data.BQL.BqlInt.Field<pOReceiptLineNbr> { }
            protected Int32? _POReceiptLineNbr;
            [PXDBInt(BqlField = typeof(INTran.pOReceiptLineNbr))]
            public virtual Int32? POReceiptLineNbr
            {
                get
                {
                    return this._POReceiptLineNbr;
                }
                set
                {
                    this._POReceiptLineNbr = value;
                }
            }
            #endregion

            #region TranTranDate
            public abstract class tranTranDate : PX.Data.BQL.BqlDateTime.Field<tranTranDate> { }
            protected DateTime? _TranTranDate;
            [PXDBDate(BqlField = typeof(INTran.tranDate))]
            [PXDBDefault(typeof(INRegister.tranDate))]
            public virtual DateTime? TranTranDate
            {
                get
                {
                    return this._TranTranDate;
                }
                set
                {
                    this._TranTranDate = value;
                }
            }
            #endregion

           
        }


    }

    [Serializable]
	public partial class INLotSerFilter : IBqlTable
	{
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[LotSerialNbr()]
		[PXDefault()]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region LotSerialNbrWildcard
		public abstract class lotSerialNbrWildcard : PX.Data.BQL.BqlString.Field<lotSerialNbrWildcard> { };
		[PXDBString(100, IsUnicode = true)]
		public virtual String LotSerialNbrWildcard
		{
			get
			{
				return PXDatabase.Provider.SqlDialect.WildcardAnything + this._LotSerialNbr + PXDatabase.Provider.SqlDialect.WildcardAnything;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))] // ??? zzz stock / nonstock ?
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[SubItemRawExt(typeof(INLotSerFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion
		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(DescriptionField = typeof(INSite.descr), Required = false, DisplayName = "Warehouse")]
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

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INLotSerFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
        #endregion

        #region ShowAdjUnitCost
        public abstract class showAdjUnitCost : PX.Data.BQL.BqlBool.Field<showAdjUnitCost> { }
        protected bool? _ShowAdjUnitCost;
        [PXDBBool()]
        [PXDefault()]
        [PXUIField(DisplayName = "Include Landed Cost in Unit Cost", Visibility = PXUIVisibility.Visible)]
        public virtual bool? ShowAdjUnitCost
        {
            get
            {
                return this._ShowAdjUnitCost;
            }
            set
            {
                this._ShowAdjUnitCost = value;
            }
        }
        #endregion
    }

}
