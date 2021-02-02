using System;
using PX.Data;
using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing MRP Detail Inquiry graph
    /// </summary>
    public class MRPDetail : PXGraph<MRPDetail>
    {
        public PXFilter<InvLookup> invlookup;

        public PXSelectReadonly<
            AMRPPlan,
            Where<AMRPPlan.inventoryID, Equal<Current<InvLookup.inventoryID>>,
                And<AMRPPlan.siteID, Equal<Current<InvLookup.siteID>>,
                And<AMRPPlan.subItemID, Equal<Current<InvLookup.subItemID>>>>>,  
            OrderBy<
                Asc<AMRPPlan.promiseDate, 
                Asc<AMRPPlan.refNoteID>>>> 
            MRPRecs;

        [PXHidden]
        public PXSelect<
            AMRPItemSite,
            Where<AMRPItemSite.inventoryID, Equal<Current<InvLookup.inventoryID>>,
                And2<
                    Where<AMRPItemSite.subItemID, Equal<Current<InvLookup.subItemID>>,
                        Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>,
                    And<AMRPItemSite.siteID, Equal<Current<InvLookup.siteID>>>>>> MRPInventory;

        [PXHidden]
        public PXSetup<AMRPSetup> amrpSetup;

        // For cache attached
        [PXHidden]
        public PXSelect<AMProdOper> ProdOper;

        #region CacheAttahed

        //Changing the production order keys for display of related document
        [OperationIDField(IsKey = false, Visible = false, Enabled = false)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationID> e) { }

        //Changing the production order keys for display of related document
        [OperationCDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationCD> e) { }

        #endregion

        /// <summary>
        /// Redirect to MRPDetail graph for given filter
        /// </summary>
        /// <param name="filter"></param>
        public static void Redirect(InvLookup filter)
        {
            if (filter?.InventoryID == null || filter.SiteID == null)
            {
                throw new PXArgumentException(nameof(filter));
            }

            var graph = CreateInstance<MRPDetail>();

            graph.invlookup.Cache.SetValueExt<InvLookup.inventoryID>(graph.invlookup.Current, filter.InventoryID);
            graph.invlookup.Cache.SetValueExt<InvLookup.siteID>(graph.invlookup.Current, filter.SiteID);

            if (AM.InventoryHelper.SubItemFeatureEnabled)
            {
                if (filter.SubItemID == null)
                {
                    throw new PXArgumentException(nameof(filter));
                }
                graph.invlookup.Cache.SetValueExt<InvLookup.subItemID>(graph.invlookup.Current, filter.SubItemID);
            }

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
        }

        /// <summary>
        /// Is the filter incomplete or empty (unable to process detail data)
        /// </summary>
        public bool EmptyFilter => invlookup?.Current?.InventoryID == null || invlookup.Current.SiteID == null || invlookup.Current.SubItemID == null && AM.InventoryHelper.SubItemFeatureEnabled;

        protected virtual void InvLookup_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var invLookup = (InvLookup)e.Row;
            if (invLookup == null)
            {
                return;
            }
            sender.SetDefaultExt<InvLookup.uOM>(e.Row);
            sender.SetDefaultExt<InvLookup.siteID>(e.Row);
            sender.SetDefaultExt<InvLookup.subItemID>(e.Row);
            KeyFilterFieldsChanged();
        }

        protected virtual void InvLookup_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            KeyFilterFieldsChanged();
        }

        protected virtual void InvLookup_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            KeyFilterFieldsChanged();
        }

        protected virtual void KeyFilterFieldsChanged()
        {
            MRPRecs.Cache.Clear();
            SetFilterAMRPItemSiteFields();
        }

        protected virtual void SetFilterAMRPItemSiteFields()
        {
            invlookup.Current.QtyOnHand = 0m;
            invlookup.Current.MinOrderQty = 0m;
            invlookup.Current.MaxOrderQty = 0m;
            invlookup.Current.LotQty = 0m;
            invlookup.Current.SafetyStock = 0m;

            MRPInventory.Current = MRPInventory.Select();

            if (MRPInventory.Current == null)
            {
                return;
            }

            invlookup.Current.QtyOnHand = MRPInventory.Current.QtyOnHand.GetValueOrDefault();
            invlookup.Current.MinOrderQty = MRPInventory.Current.MinOrdQty.GetValueOrDefault();
            invlookup.Current.MaxOrderQty = MRPInventory.Current.MaxOrdQty.GetValueOrDefault();
            invlookup.Current.LotQty = MRPInventory.Current.LotSize.GetValueOrDefault();
            invlookup.Current.SafetyStock = amrpSetup.Current.StockingMethod == AMRPSetup.MRPStockingMethod.SafetyStock
                ? MRPInventory.Current.SafetyStock.GetValueOrDefault() : MRPInventory.Current.ReorderPoint.GetValueOrDefault();
        }

        protected virtual IEnumerable mRPRecs()
        {
            if (invlookup.Current == null)
            {
                yield break;
            }

            if (EmptyFilter)
            {
                yield break;
            }

            var itVar1 = false;
            IEnumerator enumerator = this.MRPRecs.Cache.Inserted.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AMRPPlan itVar2 = (AMRPPlan)enumerator.Current;
                itVar1 = true;
                yield return itVar2;
            }

            if (!itVar1)
            {
                PXSelectBase<AMRPPlan> amrpPlans = new PXSelect<AMRPPlan,
                    Where<AMRPPlan.inventoryID, Equal<Current<InvLookup.inventoryID>>,
                        And<AMRPPlan.siteID, Equal<Current<InvLookup.siteID>>>>,
                    OrderBy<Asc<AMRPPlan.promiseDate,
                        Asc<AMRPPlan.refNoteID>>>>(this);

                if (AM.InventoryHelper.SubItemFeatureEnabled)
                {
                    amrpPlans.WhereAnd<Where<AMRPPlan.subItemID, Equal<Current<InvLookup.subItemID>>>>();
                }

                var qtytot = invlookup.Current.QtyOnHand.GetValueOrDefault();

                foreach (AMRPPlan amrpPlan in amrpPlans.Select())
                {
                    var row = amrpPlan;

                    qtytot += row.BaseQty.GetValueOrDefault();
                    row.QtyOnHand = qtytot;

                    yield return row;
                }
            }
        }
    }

    /// <summary>
    /// Filter dac
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.MRPDetailInventoryFilter)]
    public class InvLookup : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem]
        [PXDefault]
        public virtual Int32? InventoryID
        {
            get
            {
                return _InventoryID;
            }
            set
            {
                _InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(InvLookup.inventoryID), Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<InvLookup.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual Int32? SubItemID
        {
            get
            {
                return _SubItemID;
            }
            set
            {
                _SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected int? _SiteID;
        [Site]
        [PXDefault(typeof(Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<InvLookup.inventoryID>>>>))]
        public virtual int? SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                _SiteID = value;
            }
        }
        #endregion
        #region Quantity On Hand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected decimal? _QtyOnHand;
        [PXQuantity]
        [PXUIField(DisplayName = "Qty On Hand", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual decimal? QtyOnHand
        {
            get
            {
                return _QtyOnHand;
            }
            set
            {
                _QtyOnHand = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<InvLookup.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString]
        [PXUIField(DisplayName = "Base Unit", Enabled = false)]
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
        #region Safety Stock
        public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }

        protected Decimal? _SafetyStock;
        [PXQuantity]
        [PXUIField(DisplayName = "Safety Stock", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? SafetyStock
        {
            get
            {
                return this._SafetyStock;
            }
            set
            {
                this._SafetyStock = value;
            }
        }
        #endregion
        #region Min Order Qty
        public abstract class minOrderQty : PX.Data.BQL.BqlDecimal.Field<minOrderQty> { }

        protected Decimal? _MinOrderQty;
        [PXQuantity]
        [PXUIField(DisplayName = "Min. Order Qty", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? MinOrderQty
        {
            get
            {
                return this._MinOrderQty;
            }
            set
            {
                this._MinOrderQty = value;
            }
        }
        #endregion
        #region Max Order Qty
        public abstract class maxOrderQty : PX.Data.BQL.BqlDecimal.Field<maxOrderQty> { }

        protected Decimal? _MaxOrderQty;
        [PXQuantity]
        [PXUIField(DisplayName = "Max. Order Qty", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? MaxOrderQty
        {
            get
            {
                return this._MaxOrderQty;
            }
            set
            {
                this._MaxOrderQty = value;
            }
        }
        #endregion
        #region Lot Qty
        public abstract class lotQty : PX.Data.BQL.BqlDecimal.Field<lotQty> { }

        protected Decimal? _LotQty;
        [PXQuantity]
        [PXUIField(DisplayName = "Lot Qty", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? LotQty
        {
            get
            {
                return this._LotQty;
            }
            set
            {
                this._LotQty = value;
            }
        }
        #endregion
    }
}
