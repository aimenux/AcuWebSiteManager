using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.CS;
using SOLineSplit3 = PX.Objects.PO.POOrderEntry.SOLineSplit3;
using SOLine5 = PX.Objects.PO.POOrderEntry.SOLine5;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.CM;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AP.MigrationMode;
using PX.Data.BQL;

namespace PX.Objects.PO
{
	[PX.Objects.GL.TableAndChartDashboardType]
    [Serializable]
	public class POCreate : PXGraph<POCreate>
	{

		public PXCancel<POCreateFilter> Cancel;
		public PXAction<POCreateFilter> viewDocument;
		public PXFilter<POCreateFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<POFixedDemand, POCreateFilter,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POFixedDemand.inventoryID>>,
			LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POFixedDemand.vendorID>>,
			LeftJoin<POVendorInventory,
			      On<POVendorInventory.recordID, Equal<POFixedDemand.recordID>>,
			LeftJoin<CRLocation, On<CRLocation.bAccountID, Equal<POFixedDemand.vendorID>, And<CRLocation.locationID, Equal<POFixedDemand.vendorLocationID>>>,
			LeftJoin<SOOrder, On<SOOrder.noteID, Equal<POFixedDemand.refNoteID>>,
			LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<POFixedDemand.planID>>,
			LeftJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
			LeftJoin<INItemClass, 
				On<InventoryItem.FK.ItemClass>>>>>>>>>,
			Where2<Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>, Or<Current<POCreateFilter.vendorID>, IsNull>>,
			And2<Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>, Or<Current<POCreateFilter.inventoryID>, IsNull>>,
			And2<Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>, Or<Current<POCreateFilter.siteID>, IsNull>>,
			And2<Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull, Or<SOOrder.orderNbr, IsNull>>>,
			And2<Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>, Or<Current<POCreateFilter.orderType>, IsNull>>,
			And2<Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>, Or<Current<POCreateFilter.orderNbr>, IsNull>>,
			And2<Where<POFixedDemand.planDate, LessEqual<Current<POCreateFilter.requestedOnDate>>, Or<Current<POCreateFilter.requestedOnDate>, IsNull>>,
			And<Where<INItemClass.itemClassCD, Like<Current<POCreateFilter.itemClassCDWildcard>>, Or<Current<POCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>>,
			OrderBy<Asc<POFixedDemand.inventoryID>>> FixedDemand;
		public POCreate()
		{
			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			PXUIFieldAttribute.SetEnabled<POFixedDemand.orderQty>(FixedDemand.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.fixedSource>(FixedDemand.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.sourceSiteID>(FixedDemand.Cache, null, true);						
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorID>(FixedDemand.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorLocationID>(FixedDemand.Cache, null, true);

			PXUIFieldAttribute.SetDisplayName<InventoryItem.descr>(this.Caches[typeof(InventoryItem)], Messages.InventoryItemDescr);
			PXUIFieldAttribute.SetDisplayName<INSite.descr>(this.Caches[typeof(INSite)], Messages.SiteDescr);
			PXUIFieldAttribute.SetDisplayName<Vendor.acctName>(this.Caches[typeof(Vendor)], Messages.VendorAcctName);
			PXUIFieldAttribute.SetDisplayName<Customer.acctName>(this.Caches[typeof(Customer)], Messages.CustomerAcctName);
			PXUIFieldAttribute.SetDisplayName<SOOrder.customerLocationID>(this.Caches[typeof(SOOrder)], Messages.CustomerLocationID);
			PXUIFieldAttribute.SetDisplayName<INPlanType.descr>(this.Caches[typeof(INPlanType)], Messages.PlanTypeDescr);

			PXUIFieldAttribute.SetDisplayName<SOLine.curyUnitPrice>(this.Caches[typeof(SOLine)], Messages.CustomerPrice);
			PXUIFieldAttribute.SetDisplayName<SOLine.unitPrice>(this.Caches[typeof(SOLine)], Messages.CustomerPrice);
			PXUIFieldAttribute.SetDisplayName<SOLine.uOM>(this.Caches[typeof(SOLine)], Messages.CustomerPriceUOM);
			PXUIFieldAttribute.SetRequired<SOLine.uOM>(this.Caches[typeof(SOLine)], false);

			PXUIFieldAttribute.SetDisplayName<POLine.orderNbr>(this.Caches[typeof(POLine)], Messages.POLineOrderNbr);

            PXProcessingStep[] targets = PXAutomation.GetProcessingSteps(this);

            if(targets.Length == 0)
                throw new PXScreenMisconfigurationException(SO.Messages.MissingMassProcessWorkFlow);
            else
            {
                List<PXFilterRow> filters = new List<PXFilterRow>
                {
                    new PXFilterRow("SOOrder__Behavior", PXCondition.ISNULL, null, null)
                };
                filters[0].OrOperator = true;

                foreach (PXProcessingStep step in targets)
                {
                    var newcondition = PXAutomation.GetFilters(this, new string[] { step.GraphName }, new string[] { step.Name });
                    if(newcondition != null && newcondition.Length>0)
                    {
                        if (newcondition.Length > 1)
                        {
                            newcondition[0].OpenBrackets++;
                            newcondition[newcondition.Length - 1].CloseBrackets++;
                        }
                        newcondition[newcondition.Length - 1].OrOperator = true;

                        foreach(PXFilterRow filter in newcondition)
                        {
                            filter.DataField = "SOOrder__" + filter.DataField;
                        }
                        filters.AddRange(newcondition);
                    }
                }
                FixedDemand.SetAdditionalFilters(filters.ToArray());
            }
            
		}

		protected IEnumerable filter()
		{
			POCreateFilter filter = this.Filter.Current;
			filter.OrderVolume = 0;
			filter.OrderWeight = 0;
			filter.OrderTotal = 0;
			foreach(POFixedDemand demand in this.FixedDemand.Cache.Updated)
				if(demand.Selected == true)
				{
					filter.OrderVolume += demand.ExtVolume ?? 0m;
					filter.OrderWeight += demand.ExtWeight ?? 0m;
					filter.OrderTotal  += demand.ExtCost ?? 0m;
				}
			yield return filter;
		}

		protected virtual IEnumerable fixedDemand()
		{
			var query = new PXSelectJoin<POFixedDemand,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<POFixedDemand.inventoryID>>,
				LeftJoin<Vendor,
					On<Vendor.bAccountID, Equal<POFixedDemand.vendorID>>,
				LeftJoin<POVendorInventory,
					  On<POVendorInventory.recordID, Equal<POFixedDemand.recordID>>,
				LeftJoin<CRLocation,
					On<CRLocation.bAccountID, Equal<POFixedDemand.vendorID>,
					And<CRLocation.locationID, Equal<POFixedDemand.vendorLocationID>>>,
				LeftJoin<SOOrder,
					On<SOOrder.noteID, Equal<POFixedDemand.refNoteID>>,
				LeftJoin<SOLineSplit,
					On<SOLineSplit.planID, Equal<POFixedDemand.planID>>,
				LeftJoin<SOLine,
					On<SOLine.orderType, Equal<SOLineSplit.orderType>,
					And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>,
					And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
				LeftJoin<INItemClass,
					On<InventoryItem.FK.ItemClass>>>>>>>>>,
				Where2<
					Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>,
						Or<Current<POCreateFilter.vendorID>, IsNull>>,
					And2<
						Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>,
							Or<Current<POCreateFilter.inventoryID>, IsNull>>,
						And2<
							Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>,
								Or<Current<POCreateFilter.siteID>, IsNull>>,
							And2<
								Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>,
									Or<Current<POCreateFilter.customerID>, IsNull>>,
								And2<
									Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>,
										Or<Current<POCreateFilter.orderType>, IsNull>>,
									And2<
										Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>,
											Or<Current<POCreateFilter.orderNbr>, IsNull>>,
										And2<
											Where<POFixedDemand.planDate, LessEqual<Current<POCreateFilter.requestedOnDate>>,
												Or<Current<POCreateFilter.requestedOnDate>, IsNull>>,
											And<Where<INItemClass.itemClassCD, Like<Current<POCreateFilter.itemClassCDWildcard>>,
												Or<Current<POCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>>>(this);

			var fixedDemands = new PXResultset<POFixedDemand, InventoryItem, Vendor, POVendorInventory>();
			var startRow = PXView.StartRow;
			var totalRows = 0;
			object[] parameters = null;

			if (PXView.MaximumRows == 1 
				&& PXView.Searches?.Length == FixedDemand.Cache.BqlKeys.Count
				&& PXView.Searches[0] != null && PXView.Searches[1] != null)
			{
				var inventoryCD = (string)PXView.Searches[0];
				var planID = Convert.ToInt64(PXView.Searches[1]);
				query.WhereAnd<Where<
					POFixedDemand.planID.IsEqual<@P.AsLong>
					.And<InventoryItem.inventoryCD.IsEqual<@P.AsString>>>>();
				parameters = new object[] { planID, inventoryCD };
			}

			foreach (PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory> demand in query.View.Select(PXView.Currents, parameters,
				PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
				ref startRow, PXView.MaximumRows, ref totalRows))
				fixedDemands.Add(demand);

			PXView.StartRow = 0;

			return EnumerateAndPrepareFixedDemands(fixedDemands);
		}

		/// <summary>
		/// Enumerates the and prepares fixed demands for the view delegate. This is an extension point used by Lexware PriceUnit customization.
		/// </summary>
		/// <param name="fixedDemands">The fixed demands.</param>
		/// <returns/>
		public virtual IEnumerable EnumerateAndPrepareFixedDemands(PXResultset<POFixedDemand> fixedDemands)
		{
			foreach (PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory> rec in fixedDemands)
			{
                var demand = (POFixedDemand)rec;
                var item = (InventoryItem)rec;
                var vendor = (Vendor)rec;
                var price = (POVendorInventory)rec;
                EnumerateAndPrepareFixedDemandRow(demand, item, vendor, price);

                yield return rec;
			}
		}

        public virtual void EnumerateAndPrepareFixedDemandRow(POFixedDemand demand, InventoryItem item, Vendor vendor, POVendorInventory price)
        {
                if (demand?.InventoryID != null && demand.UOM != null && demand.VendorID != null && vendor?.CuryID != null &&
                    Filter.Current.PurchDate != null && demand.EffPrice == null)
                {
                    demand.EffPrice = APVendorPriceMaint.CalculateCuryUnitCost(
                        sender: FixedDemand.Cache,
                        vendorID: demand.VendorID,
                        vendorLocationID: demand.VendorLocationID,
                        inventoryID: demand.InventoryID,
                        siteID: demand.SiteID,
                        curyID: vendor.CuryID,
                        UOM: demand.UOM,
                        quantity: demand.OrderQty,
                        date: Filter.Current.PurchDate.Value,
                        currentUnitCost: 0m);
                }

                if (demand.RecordID != null && demand.EffPrice == null)
                {
                    demand.EffPrice = price.LastPrice;
                    demand.AddLeadTimeDays = price.AddLeadTimeDays;
                }

                if (demand.EffPrice != null && demand.OrderQty != null && demand.ExtCost == null)
                    demand.ExtCost = demand.OrderQty * demand.EffPrice;
        }

        protected virtual void POCreateFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POCreateFilter filter = Filter.Current;

			if (filter == null) return;

			FixedDemand.SetProcessDelegate(delegate(List<POFixedDemand> list)
			{
				CreateProc(list, filter.PurchDate, filter.OrderNbr != null);
			});

			TimeSpan span;
			Exception message;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out span, out message);

			PXUIFieldAttribute.SetVisible<POLine.orderNbr>(Caches[typeof(POLine)], null, (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted));
			PXUIFieldAttribute.SetVisible<POCreateFilter.orderTotal>(sender, null, filter.VendorID != null);
		}

		protected virtual void POFixedDemand_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			POFixedDemand row = (POFixedDemand)e.Row;
			if(row != null && row.Selected != true 
				&& sender.ObjectsEqual<POFixedDemand.selected>(e.Row, e.OldRow))
			{
				row.Selected = true;
			}
		}
		protected virtual void POFixedDemand_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POFixedDemand row = e.Row as POFixedDemand;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<POFixedDemand.orderQty>(sender, row, row.PlanType == INPlanConstants.Plan90);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.fixedSource>(FixedDemand.Cache, row, row.PlanType == INPlanConstants.Plan90);
            PXUIFieldAttribute.SetEnabled<POFixedDemand.pOSiteID>(sender, row, row.FixedSource == INReplenishmentSource.Purchased);
            PXUIFieldAttribute.SetEnabled<POFixedDemand.sourceSiteID>(sender, row, row.FixedSource == INReplenishmentSource.Transfer);			
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorID>(sender, row, row.FixedSource == INReplenishmentSource.Purchased);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorLocationID>(sender, row, row.FixedSource == INReplenishmentSource.Purchased);			
		}

		protected virtual void POFixedDemand_VendorLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POFixedDemand row = (POFixedDemand )e.Row;
			if(row != null)
			{
				e.NewValue =
					PX.Objects.PO.POItemCostManager.FetchLocation(
						this,
						row.VendorID,
						row.InventoryID,
						row.SubItemID,
						row.SiteID);
				e.Cancel = true;
			}
		}
		protected virtual void POFixedDemand_OrderQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POFixedDemand row = (POFixedDemand )e.Row;
			if (row != null && row.PlanUnitQty < (Decimal?)e.NewValue)
			{
				e.NewValue = row.PlanUnitQty;
				sender.RaiseExceptionHandling<POFixedDemand.orderQty>(row, null,
				                                                      new PXSetPropertyException<POFixedDemand.orderQty>(
				                                                      	Messages.POOrderQtyValidation, PXErrorLevel.Warning));
			}
		}
		protected virtual void POFixedDemand_RecordID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POFixedDemand row = (POFixedDemand)e.Row;
			POVendorInventory result = null;
			if (row == null) return;
			foreach (PXResult<POVendorInventory, BAccountR, InventoryItem> rec in 
				PXSelectJoin<POVendorInventory,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<POVendorInventory.vendorID>>,
				InnerJoin<InventoryItem, 
					On<POVendorInventory.FK.InventoryItem>>>,
				Where<POVendorInventory.vendorID, Equal<Current<POFixedDemand.vendorID>>,
				And<POVendorInventory.inventoryID, Equal<Current<POFixedDemand.inventoryID>>,
				And<POVendorInventory.active, Equal<boolTrue>,
				And2<Where<POVendorInventory.vendorLocationID, Equal<Current<POFixedDemand.vendorLocationID>>,
					    Or<POVendorInventory.vendorLocationID, IsNull>>,
					  And<Where<POVendorInventory.subItemID, Equal<Current<POFixedDemand.subItemID>>,
						     Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>>>>>>>>
				.SelectMultiBound(this, new object[] {e.Row}))
			{
				POVendorInventory price = rec;
				InventoryItem item = rec;
				if (price.VendorLocationID == row.VendorLocationID && 
					price.SubItemID == row.SubItemID)
				{
					result = price;
					break;
				}

				if (price.VendorLocationID == row.VendorLocationID)
					result = price;

				if (result != null && result.VendorLocationID != row.VendorLocationID &&
					price.SubItemID == row.SubItemID)
					result = price;

				if (result == null)
					result = price;
			}
			if(result != null)
			{
				e.NewValue = result.RecordID;
				e.Cancel = true;
			}
			

		}

		protected virtual void POFixedDemand_RecordID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            POFixedDemand demand = e.Row as POFixedDemand;

            if (demand == null || Filter.Current == null)
                return;

				decimal? vendorUnitCost = null;

            if (demand.InventoryID != null && demand.UOM != null && demand.VendorID != null && Filter.Current.PurchDate != null)
            {
                Vendor vendor = PXSelect<Vendor, 
                                   Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
                                .Select(this, demand.VendorID);

                if (vendor?.CuryID != null)
				{
                    vendorUnitCost = APVendorPriceMaint.CalculateCuryUnitCost(sender, demand.VendorID, demand.VendorLocationID, demand.InventoryID, 
																			  demand.SiteID, vendor.CuryID, demand.UOM, demand.OrderQty, 
																			  (DateTime)Filter.Current.PurchDate, 0m);
				}

                demand.EffPrice = vendorUnitCost;
			}

            POVendorInventory price = 
                PXSelect<POVendorInventory,
                   Where<POVendorInventory.recordID, Equal<Required<POVendorInventory.recordID>>>>
                .SelectSingleBound(this, null, demand.RecordID);

            if (vendorUnitCost == null)
            {
				demand.EffPrice = price?.LastPrice ?? 0m;
			}

            demand.AddLeadTimeDays = price?.AddLeadTimeDays;              
            FixedDemand.Cache.RaiseFieldUpdated<POFixedDemand.effPrice>(demand, null);
		}

        #region Actions
        public PXAction<POCreateFilter> inventorySummary;

        [PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable InventorySummary(PXAdapter adapter)
        {
            PXCache tCache = FixedDemand.Cache;
            POFixedDemand line = FixedDemand.Current;
            if (line == null) return adapter.Get();

            InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
            if (item != null && item.StkItem == true)
            {
                INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<POFixedDemand.subItemID>(tCache, line);
                InventorySummaryEnq.Redirect(item.InventoryID,
                                             ((sbitem != null) ? sbitem.SubItemCD : null),
                                             line.SiteID,
                                             line.LocationID);
            }
            return adapter.Get();
        }

        
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            POFixedDemand line = FixedDemand.Current;
            if (line == null || line.RefNoteID == null) return adapter.Get();

            SOOrder doc = PXSelect<SOOrder, Where<SOOrder.noteID, Equal<Required<POFixedDemand.refNoteID>>>>.Select(this, line.RefNoteID);

            if (doc != null)
            {
                SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();
                graph.Document.Current = doc;
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }
		#endregion

		public static void CreateProc(List<POFixedDemand> list, DateTime? orderDate, bool extSort)
		{
			PXRedirectRequiredException poredirect = CreatePOOrders(list, orderDate, extSort);
			//TODO: remove OrigPO related code
			//PXRedirectRequiredException soredirect = CreateSOOrders(list, orderDate);
			//if (poredirect != null && soredirect == null)
			//    throw poredirect;
			//if (poredirect == null && soredirect != null)
			//    throw soredirect;

			if (poredirect != null)
				throw poredirect;
		}

		public static PXRedirectRequiredException CreatePOOrders(List<POFixedDemand> list, DateTime? PurchDate, bool extSort)
		{
			POOrderEntry docgraph = PXGraph.CreateInstance<POOrderEntry>();
			docgraph.Views.Caches.Add(typeof(SOLineSplit3));
			POSetup setup = docgraph.POSetup.Current;

			DocumentList<POOrder> created = new DocumentList<POOrder>(docgraph);
            Dictionary<String, DocumentList<POLine>> orderedByPlantype = new Dictionary<String, DocumentList<POLine>>();
            DocumentList<POLine> ordered;

			list = docgraph.SortPOFixDemandList(list);

			POOrder order = null;
			bool hasErrors = false;

			foreach (POFixedDemand demand in list)
			{
				if (demand.FixedSource != INReplenishmentSource.Purchased)
                    continue;

				string OrderType =
					demand.PlanType == INPlanConstants.Plan6D ? POOrderType.DropShip :
					demand.PlanType == INPlanConstants.Plan6E ? POOrderType.DropShip :
					POOrderType.RegularOrder;
				string replanType = null;

				if (demand.VendorID == null || demand.VendorLocationID == null)
				{
					PXProcessing<POFixedDemand>.SetWarning(list.IndexOf(demand), Messages.MissingVendorOrLocation);
					continue;
				}			

				PXErrorLevel ErrorLevel = PXErrorLevel.RowInfo;
				string ErrorText = string.Empty;

				try
				{
					
					SOOrder soorder = PXSelect<SOOrder, Where<SOOrder.noteID, Equal<Required<SOOrder.noteID>>>>.Select(docgraph, demand.RefNoteID);
					SOLineSplit3 soline = PXSelect<SOLineSplit3, Where<SOLineSplit3.planID, Equal<Required<SOLineSplit3.planID>>>>.Select(docgraph, demand.PlanID);

					string BLType = null;
					string BLOrderNbr = null;

					if (demand.PlanType == INPlanConstants.Plan6B ||
							demand.PlanType == INPlanConstants.Plan6E)
					{
						BLType = soline.POType;
						BLOrderNbr = soline.PONbr;
					}

					var orderSearchValues = new List<FieldLookup>()
					{
						new FieldLookup<POOrder.orderType>(OrderType),
						new FieldLookup<POOrder.vendorID>(demand.VendorID),
						new FieldLookup<POOrder.vendorLocationID>(demand.VendorLocationID),
						new FieldLookup<POOrder.bLOrderNbr>(BLOrderNbr),
					};
					if (OrderType == POOrderType.RegularOrder)
					{
						bool requireSingleProject = (docgraph.apsetup.Current.RequireSingleProjectPerDocument == true);
						if (requireSingleProject)
						{
							int? project = demand.ProjectID ?? PM.ProjectDefaultAttribute.NonProject();
							orderSearchValues.Add(new FieldLookup<POOrder.projectID>(project));
						}

						if (order != null && order.ShipDestType == POShippingDestination.CompanyLocation && order.SiteID == null)
						{
							//When previous order was shipped to Company then we would never find it if we search by POSiteID 
						}
						else
						{
							orderSearchValues.Add(new FieldLookup<POOrder.siteID>(demand.POSiteID));
						}
					}
					else if (OrderType == POOrderType.DropShip)
					{
						orderSearchValues.Add(new FieldLookup<POOrder.sOOrderType>(soline.OrderType));
						orderSearchValues.Add(new FieldLookup<POOrder.sOOrderNbr>(soline.OrderNbr));
					}
					else
					{
						orderSearchValues.Add(new FieldLookup<POOrder.shipToBAccountID>(soorder.CustomerID));
						orderSearchValues.Add(new FieldLookup<POOrder.shipToLocationID>(soorder.CustomerLocationID));
						orderSearchValues.Add(new FieldLookup<POOrder.siteID>(demand.POSiteID));
					}
					order = created.Find(orderSearchValues.ToArray()) ?? new POOrder();

					if (order.OrderNbr == null)
					{
						docgraph.Clear();

						order.OrderType = OrderType;
						order = PXCache<POOrder>.CreateCopy(docgraph.Document.Insert(order));
						order.VendorID = demand.VendorID;
						order.VendorLocationID = demand.VendorLocationID;
						order.SiteID = demand.POSiteID;
						if (demand.ProjectID != null)
						{
							order.ProjectID = demand.ProjectID;
						}
						order.OrderDate = PurchDate;
						order.BLType = BLType;
						order.BLOrderNbr = BLOrderNbr;

						if (OrderType == POOrderType.DropShip || extSort)
						{
							order.SOOrderType = soline.OrderType;
							order.SOOrderNbr = soline.OrderNbr;
						}

						if (!string.IsNullOrEmpty(order.BLOrderNbr))
						{
							POOrder blanket = PXSelect<POOrder, Where<POOrder.orderType, Equal<Current<POOrder.bLType>>, And<POOrder.orderNbr, Equal<Current<POOrder.bLOrderNbr>>>>>.SelectSingleBound(docgraph, new object[] { order });
							if (blanket != null)
							{
								order.VendorRefNbr = blanket.VendorRefNbr;
							}
						}

						if (OrderType == POOrderType.DropShip)
						{
							order.ShipDestType = POShippingDestination.Customer;
							order.ShipToBAccountID = soorder.CustomerID;
							order.ShipToLocationID = soorder.CustomerLocationID;
						}
						else if (setup.ShipDestType == POShipDestType.Site)
						{
							order.ShipDestType = POShippingDestination.Site;
							order.SiteID = demand.POSiteID;
						}

						if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
						{
							//GetValuePending will fall in CurrencyInfo_CuryIdFieldSelecting()
							docgraph.currencyinfo.Current.CuryID = null;
						}

						order = docgraph.Document.Update(order);

                        if (OrderType == POOrderType.DropShip)
                        {
                            SOAddress soAddress = PXSelect<SOAddress, Where<SOAddress.addressID, Equal<Required<SOOrder.shipAddressID>>>>.Select(docgraph, soorder.ShipAddressID);

                            if (soAddress.IsDefaultAddress == false)
                            {
                                AddressAttribute.CopyRecord<POOrder.shipAddressID>(docgraph.Document.Cache, order, soAddress, true);
                            }
                            SOContact soContact = PXSelect<SOContact, Where<SOContact.contactID, Equal<Required<SOOrder.shipContactID>>>>.Select(docgraph, soorder.ShipContactID);

                            if (soContact.IsDefaultContact == false)
                            {
                                ContactAttribute.CopyRecord<POOrder.shipContactID>(docgraph.Document.Cache, order, soContact, true);
                            }

							if (order.ExpectedDate < soorder.RequestDate)
							{
								order = PXCache<POOrder>.CreateCopy(order);
								order.ExpectedDate = soorder.RequestDate;
								order = docgraph.Document.Update(order);
							}
                        }
                    }
					else if (docgraph.Document.Cache.ObjectsEqual(docgraph.Document.Current, order) == false)
					{
						docgraph.Document.Current = docgraph.Document.Search<POOrder.orderNbr>(order.OrderNbr, order.OrderType);
					}

					//we do not want vendor inventory updated in this case
					order.UpdateVendorCost = false;

					POLine line = null;
                    //Sales Orders to Blanket should not be grouped together
                    //Drop Ships to Blankets are not grouped either

				    if (!orderedByPlantype.TryGetValue(demand.PlanType, out ordered))
				    {
				        ordered = orderedByPlantype[demand.PlanType] = new DocumentList<POLine>(docgraph);
				    }

				    if (OrderType == POOrderType.RegularOrder && demand.PlanType != INPlanConstants.Plan6B)
					{
						var lineSearchValues = new List<FieldLookup>()
						{
							new FieldLookup<POLine.vendorID>(demand.VendorID),
							new FieldLookup<POLine.vendorLocationID>(demand.VendorLocationID),
							new FieldLookup<POLine.siteID>(demand.POSiteID),
							new FieldLookup<POLine.inventoryID>(demand.InventoryID),
							new FieldLookup<POLine.subItemID>(demand.SubItemID),
							new FieldLookup<POLine.requestedDate>(soline?.ShipDate),
							new FieldLookup<POLine.projectID>(soline?.ProjectID),
							new FieldLookup<POLine.taskID>(soline?.TaskID),
							new FieldLookup<POLine.costCodeID>(soline?.CostCodeID),
						};
						if (setup.CopyLineDescrSO == true && soline != null)
						{
							lineSearchValues.Add(new FieldLookup<POLine.tranDesc>(soline.TranDesc));
							line = ordered.Find(lineSearchValues.ToArray());
							if (line != null && setup.CopyLineNoteSO == true &&
								(PXNoteAttribute.GetNote(docgraph.Caches[typeof(POLine)], line) != null || PXNoteAttribute.GetNote(docgraph.Caches[typeof(SOLineSplit3)], soline) != null))
							{
								line = null;
							}
						}
						else
							line = ordered.Find(lineSearchValues.ToArray());
					}

					line = line ?? new POLine();

					if (line.OrderNbr == null)
					{
						docgraph.FillPOLineFromDemand(line, demand, OrderType, soline);

						line = docgraph.Transactions.Insert(line);

						if (setup.CopyLineNoteSO == true && soline != null)
						{
							PXNoteAttribute.SetNote(docgraph.Transactions.Cache, line,
								PXNoteAttribute.GetNote(docgraph.Caches[typeof(SOLineSplit3)], soline));
						}

                        if(docgraph.onCopyPOLineFields != null)
                        {
                            docgraph.onCopyPOLineFields(demand, line);
                        }

						line = PXCache<POLine>.CreateCopy(line);
                        ordered.Add(line);
					}
					else
					{
						line = (POLine)
                            PXSelect<POLine, 
                               Where<POLine.orderType, Equal<Current<POOrder.orderType>>, 
                               And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>, 
                               And<POLine.lineNbr, Equal<Current<POLine.lineNbr>>>>>>
                               .SelectSingleBound(docgraph, new object[] { line });

						line = PXCache<POLine>.CreateCopy(line);
						line.OrderQty += demand.OrderQty;
					}

					if (demand.PlanType == INPlanConstants.Plan6B ||
						demand.PlanType == INPlanConstants.Plan6E)
					{
						replanType =
							demand.PlanType == INPlanConstants.Plan6B
								? INPlanConstants.Plan66
								: INPlanConstants.Plan6D;
						demand.FixedSource = INReplenishmentSource.Purchased;

						line.POType = soline.POType;
						line.PONbr = soline.PONbr;
						line.POLineNbr = soline.POLineNbr;

						POLine blanket_line = 
                            PXSelect<POLine, 
                                Where<POLine.orderType, Equal<Current<POLine.pOType>>, 
                                  And<POLine.orderNbr, Equal<Current<POLine.pONbr>>, 
                                  And<POLine.lineNbr, Equal<Current<POLine.pOLineNbr>>>>>>
                             .SelectSingleBound(docgraph, new object[] { line });

						if (blanket_line != null)
						{
							//POOrderEntry() is persisted on each loop, BaseOpenQty will include everything in List<POLine> ordered
							if (demand.PlanQty > blanket_line.BaseOpenQty)
							{
								line.OrderQty -= demand.OrderQty;

								if (string.Equals(line.UOM, blanket_line.UOM))
								{
									line.OrderQty += blanket_line.OpenQty;
								}
								else
								{
									PXDBQuantityAttribute.CalcBaseQty<POLine.orderQty>(docgraph.Transactions.Cache, line);
									line.BaseOrderQty += blanket_line.BaseOpenQty;
									PXDBQuantityAttribute.CalcTranQty<POLine.orderQty>(docgraph.Transactions.Cache, line);
								}

								ErrorLevel = PXErrorLevel.RowWarning;
								ErrorText += PXMessages.LocalizeFormatNoPrefixNLA(Messages.QuantityReducedToBlanketOpen, line.PONbr);
							}

							line.CuryUnitCost = blanket_line.CuryUnitCost;
							line.UnitCost = blanket_line.UnitCost;
						}
					}

					line = docgraph.Transactions.Update(line);
					PXCache cache = docgraph.Caches[typeof(INItemPlan)];
					CreateSplitDemand(cache, demand);

					cache.SetStatus(demand, PXEntryStatus.Updated);
					demand.SupplyPlanID = line.PlanID;

					if (replanType != null)
					{
						cache.RaiseRowDeleted(demand);
						demand.PlanType = replanType;
						cache.RaiseRowInserted(demand);
					}

					if (soline != null)
					{
						if (demand.AlternateID != null && demand.InventoryID != null)
						{
							PXSelectBase<INItemXRef> xref = new PXSelect<INItemXRef,
								Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
								And<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>>>(docgraph);

							INItemXRef soXRef = xref.Select(demand.InventoryID, demand.AlternateID);

							if (soXRef != null && soXRef.AlternateType == INAlternateType.Global)
							{
								if (line.AlternateID != null && line.InventoryID != null)
								{
									INItemXRef poXRef = xref.Select(line.InventoryID, line.AlternateID);

									if (poXRef != null && poXRef.AlternateType == INAlternateType.Global)
									{
						line.AlternateID = demand.AlternateID;
									}
								}
								else
								{
									line.AlternateID = demand.AlternateID;
								}
							}
						}

						soline.POType = line.OrderType;
						soline.PONbr = line.OrderNbr;
						soline.POLineNbr = line.LineNbr;
                        soline.RefNoteID = docgraph.Document.Current.NoteID;

						docgraph.UpdateSOLine(soline, docgraph.Document.Current.VendorID, true);

						docgraph.FixedDemand.Cache.SetStatus(soline, PXEntryStatus.Updated);
					}

					if (docgraph.Transactions.Cache.IsInsertedUpdatedDeleted)
					{
						using (PXTransactionScope scope = new PXTransactionScope())
						{
							docgraph.Save.Press();

							if (demand.PlanType == INPlanConstants.Plan90)
							{
								docgraph.Replenihment.Current = docgraph.Replenihment.Search<INReplenishmentOrder.noteID>(demand.RefNoteID);
								if (docgraph.Replenihment.Current != null)
								{
									INReplenishmentLine rLine =
										PXCache<INReplenishmentLine>.CreateCopy(docgraph.ReplenishmentLines.Insert(new INReplenishmentLine()));

									rLine.InventoryID = line.InventoryID;
									rLine.SubItemID = line.SubItemID;
									rLine.UOM = line.UOM;
									rLine.VendorID = line.VendorID;
									rLine.VendorLocationID = line.VendorLocationID;
									rLine.Qty = line.OrderQty;
									rLine.POType = line.OrderType;
									rLine.PONbr = docgraph.Document.Current.OrderNbr;
									rLine.POLineNbr = line.LineNbr;
									rLine.SiteID = demand.POSiteID;
									rLine.PlanID = demand.PlanID;
									docgraph.ReplenishmentLines.Update(rLine);
									docgraph.Caches[typeof(INItemPlan)].Delete(demand);
									docgraph.Save.Press();
								}
							}
							scope.Complete();
						}

						if (ErrorLevel == PXErrorLevel.RowInfo)
						{
							PXProcessing<POFixedDemand>.SetInfo(list.IndexOf(demand), PXMessages.LocalizeFormatNoPrefixNLA(Messages.PurchaseOrderCreated, docgraph.Document.Current.OrderNbr) + "\r\n" + ErrorText);
						}
						else
						{
							PXProcessing<POFixedDemand>.SetWarning(list.IndexOf(demand), PXMessages.LocalizeFormatNoPrefixNLA(Messages.PurchaseOrderCreated, docgraph.Document.Current.OrderNbr) + "\r\n" + ErrorText);
						}

						if (created.Find(docgraph.Document.Current) == null)
						{
							created.Add(docgraph.Document.Current);
						}
					}
				}
				catch (Exception e)
				{
					PXProcessing<POFixedDemand>.SetError(list.IndexOf(demand), e);
					PXTrace.WriteError(e);
					hasErrors = true;
				}
			}

			if (!hasErrors && created.Count == 1)
			{
				using (new PXTimeStampScope(null))
				{
					docgraph.Clear();
					docgraph.Document.Current = docgraph.Document.Search<POOrder.orderNbr>(created[0].OrderNbr, created[0].OrderType);
					return new PXRedirectRequiredException(docgraph, Messages.POOrder);
				}
			}

			return null;
		}

		private static void CreateSplitDemand(PXCache cache, POFixedDemand demand)
		{
			if (demand.OrderQty != demand.PlanUnitQty)
			{
				INItemPlan orig_demand = PXSelectReadonly<INItemPlan,
					Where<INItemPlan.planID, Equal<Current<INItemPlan.planID>>>>
					.SelectSingleBound(cache.Graph, new object[] {demand});

				INItemPlan split = PXCache<INItemPlan>.CreateCopy(orig_demand);
				split.PlanID = null;
				split.PlanQty = demand.PlanUnitQty - demand.OrderQty;
				if (demand.UnitMultDiv == MultDiv.Multiply)
					split.PlanQty *= demand.UnitRate;
				else
					split.PlanQty /= demand.UnitRate;
				cache.Insert(split);
				cache.RaiseRowDeleted(demand);
				demand.PlanQty = orig_demand.PlanQty - split.PlanQty;
				cache.RaiseRowInserted(demand);
			}
		}


		[Serializable()]
		public partial class POCreateFilter : IBqlTable
		{
			#region CurrentOwnerID
			public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

			[PXDBGuid]
			[CRCurrentOwnerID]
			public virtual Guid? CurrentOwnerID { get; set; }
			#endregion
			#region MyOwner
			public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
			protected Boolean? _MyOwner;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Me")]
			public virtual Boolean? MyOwner
			{
				get
				{
					return _MyOwner;
				}
				set
				{
					_MyOwner = value;
				}
			}
			#endregion
			#region OwnerID
			public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
			protected Guid? _OwnerID;
			[PXDBGuid]
			[PXUIField(DisplayName = "Product Manager")]
			[PX.TM.PXSubordinateOwnerSelector]
			public virtual Guid? OwnerID
			{
				get
				{
					return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
				}
				set
				{
					_OwnerID = value;
				}
			}
			#endregion
			#region WorkGroupID
			public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
			protected Int32? _WorkGroupID;
			[PXDBInt]
			[PXUIField(DisplayName = "Product  Workgroup")]
			[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
				Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
			 SubstituteKey = typeof(EPCompanyTree.description))]
			public virtual Int32? WorkGroupID
			{
				get
				{
					return (_MyWorkGroup == true) ? null : _WorkGroupID;
				}
				set
				{
					_WorkGroupID = value;
				}
			}
			#endregion
			#region MyWorkGroup
			public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
			protected Boolean? _MyWorkGroup;
			[PXDefault(false)]
			[PXDBBool]
			[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? MyWorkGroup
			{
				get
				{
					return _MyWorkGroup;
				}
				set
				{
					_MyWorkGroup = value;
				}
			}
			#endregion
			#region FilterSet
			public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
			[PXDefault(false)]
			[PXDBBool]
            public virtual Boolean? FilterSet
			{
				get
				{
					return
						this.OwnerID != null ||
						this.WorkGroupID != null ||
						this.MyWorkGroup == true;
				}
			}
			#endregion			
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[Vendor(
				typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
				CacheGlobal = true,
				Filterable = true)]
			[VerndorNonEmployeeOrOrganizationRestrictor]
			[PXRestrictor(typeof(Where<Vendor.status, IsNull,
									Or<Vendor.status, Equal<BAccount.status.active>,
									Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(Vendor.status))]
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
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[IN.Site(DisplayName = "Warehouse ID")]
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
			#region SourceSiteID
			public abstract class sourceSiteID : PX.Data.BQL.BqlInt.Field<sourceSiteID> { }
			protected Int32? _SourceSiteID;
			[IN.Site(DisplayName = "Source Warehouse", DescriptionField = typeof(INSite.descr))]			
			public virtual Int32? SourceSiteID
			{
				get
				{
					return this._SourceSiteID;
				}
				set
				{
					this._SourceSiteID = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Date Promised")]
			[PXDefault(typeof(AccessInfo.businessDate))]
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
			#region PurchDate
			public abstract class purchDate : PX.Data.BQL.BqlDateTime.Field<purchDate> { }
			protected DateTime? _PurchDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Creation Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? PurchDate
			{
				get
				{
					return this._PurchDate;
				}
				set
				{
					this._PurchDate = value;
				}
			}
			#endregion
			#region RequestedOnDate
			public abstract class requestedOnDate : PX.Data.BQL.BqlDateTime.Field<requestedOnDate> { }
			protected DateTime? _RequestedOnDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Requested On")]
			public virtual DateTime? RequestedOnDate
			{
				get
				{
					return this._RequestedOnDate;
				}
				set
				{
					this._RequestedOnDate = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer()]
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
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[StockItem()]
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
			#region ItemClassCD
			public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
			protected string _ItemClassCD;

			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
			public virtual string ItemClassCD
				{
				get { return this._ItemClassCD; }
				set { this._ItemClassCD = value; }
				}
			#endregion
			#region ItemClassCDWildcard
			public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
			[PXString(IsUnicode = true)]
			[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
			[PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD))]
			public virtual string ItemClassCDWildcard
			{
				get { return ItemClassTree.MakeWildcard(ItemClassCD); }
				set { }
			}
			#endregion
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			protected String _OrderType;
			[PXDBString(2, IsFixed = true, InputMask = ">aa")]			
			[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<True>>>))]
			[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
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
			[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]			
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
			[SO.SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
				LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>,
						And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
				Where<SOOrder.orderType, Equal<Optional<POCreateFilter.orderType>>,
				And<Where<SOOrder.orderType, Equal<SOOrderTypeConstants.transferOrder>,
				 Or<Customer.bAccountID, IsNotNull>>>>,
				 OrderBy<Desc<SOOrder.orderNbr>>>))]
			[PXFormula(typeof(Default<POCreateFilter.orderType>))]
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
            #region OrderWeight
            public abstract class orderWeight : PX.Data.BQL.BqlDecimal.Field<orderWeight> { }
			protected Decimal? _OrderWeight;
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Weight", Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? OrderWeight
			{
				get
				{
					return this._OrderWeight;
				}
				set
				{
					this._OrderWeight = value;
				}
			}
			#endregion
			#region OrderVolume
			public abstract class orderVolume : PX.Data.BQL.BqlDecimal.Field<orderVolume> { }
			protected Decimal? _OrderVolume;
			[PXDBDecimal(6)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Volume", Enabled = false)]
			public virtual Decimal? OrderVolume
			{
				get
				{
					return this._OrderVolume;
				}
				set
				{
					this._OrderVolume = value;
				}
			}
			#endregion
			#region OrderTotal
			public abstract class orderTotal : PX.Data.BQL.BqlDecimal.Field<orderTotal> { }
			protected Decimal? _OrderTotal;
			[PXDBDecimal(typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<POCreateFilter.vendorID>>>>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total", Enabled = false)]
			public virtual Decimal? OrderTotal
			{
				get
				{
					return this._OrderTotal;
				}
				set
				{
					this._OrderTotal = value;
				}
			}
			#endregion
		
		}

        /// <summary>
        /// Specialized version of the Projection Attribute. Defines Projection as <br/>
        /// a select of INItemPlan Join INPlanType Join InventoryItem Join INUnit Left Join INItemSite <br/>
        /// filtered by InventoryItem.workgroupID and InventoryItem.productManagerID according to the values <br/>
        /// in the POCreateFilter: <br/>
        /// 1. POCreateFilter.ownerID is null or  POCreateFilter.ownerID = InventoryItem.productManagerID <br/>
        /// 2. POCreateFilter.workGroupID is null or  POCreateFilter.workGroupID = InventoryItem.productWorkgroupID <br\>
        /// 3. POCreateFilter.myWorkGroup = false or  InventoryItem.productWorkgroupID =InMember<POCreateFilter.currentOwnerID> <br/>
        /// 4. InventoryItem.productWorkgroupID is null or  InventoryItem.productWorkgroupID =Owened<POCreateFilter.currentOwnerID><br/>        
        /// </summary>
        public class POCreateProjectionAttribute : TM.OwnedFilter.ProjectionAttribute
		{
            /// <summary>
            /// Default ctor
            /// </summary>
			public POCreateProjectionAttribute()
				: base(typeof(POCreateFilter),
				BqlCommand.Compose(
			typeof(Select2<,,>),
				typeof(INItemPlan),
				typeof(InnerJoin<INPlanType, 
				              On<INItemPlan.FK.PlanType>,
				InnerJoin<InventoryItem, 
					On<INItemPlan.FK.InventoryItem>,
				InnerJoin<INUnit, 
					On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>, 
					And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>,
                LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>,
                LeftJoin<SOLine, On<SOLineSplit.orderType, Equal<SOLine.orderType>, And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>, And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>,
                LeftJoin<IN.S.INItemSite, On<IN.S.INItemSite.inventoryID, Equal<INItemPlan.inventoryID>, And<IN.S.INItemSite.siteID, Equal<INItemPlan.siteID>>>>>>>>>),
            typeof(Where2<,>),
			typeof(Where<INItemPlan.hold, Equal<False>,
					  And<INItemPlan.fixedSource, Equal<INReplenishmentSource.purchased>,	
					  And<INPlanType.isFixed, Equal<True>, And<INPlanType.isDemand, Equal<True>,
				      And<Where<INItemPlan.supplyPlanID, IsNull, 
				              Or<INItemPlan.planType, Equal<INPlanConstants.plan6B>,
				              Or<INItemPlan.planType, Equal<INPlanConstants.plan6E>>>>>>>>>),
			typeof(And<>),
			TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
			typeof(POCreateFilter),
			typeof(INItemSite.productWorkgroupID),
			typeof(INItemSite.productManagerID))))
			{
			}
		}
	
		
	}

    [POCreate.POCreateProjectionAttribute]
        [Serializable]
		public partial class POFixedDemand : INItemPlan
		{
			#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region SiteID
			public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region PlanDate
			public new abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }
			[PXDBDate()]
			[PXDefault()]
			[PXUIField(DisplayName = "Requested On")]
			public override DateTime? PlanDate
			{
				get
				{
					return this._PlanDate;
				}
				set
				{
					this._PlanDate = value;
				}
			}
			#endregion
			#region PlanID
			public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
			#endregion
            #region FixedSource
            public new abstract class fixedSource : PX.Data.BQL.BqlString.Field<fixedSource> { }
            [PXDBString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Fixed Source", Enabled = false)]
            [PXDefault(INReplenishmentSource.Purchased, PersistingCheck = PXPersistingCheck.Nothing)]
            [INReplenishmentSource.INPlanList]
            public override String FixedSource
            {
                get
                {
                    return this._FixedSource;
                }
                set
                {
                    this._FixedSource = value;
                }
            }
            #endregion
			#region PlanType
			public new abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
			[PXDBString(2, IsFixed = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Plan Type")]
			[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true, DescriptionField = typeof(INPlanType.descr))]
			public override String PlanType
			{
				get
				{
					return this._PlanType;
				}
				set
				{
					this._PlanType = value;
				}
			}
			#endregion			
			#region SubItemID
			public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region LocationID
			public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
			#region LotSerialNbr
			public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			#endregion
			#region SourceSiteID
			public new abstract class sourceSiteID : PX.Data.BQL.BqlInt.Field<sourceSiteID> { }
			[IN.Site(DisplayName = "Demand Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(INItemPlan.sourceSiteID))]
			[PXFormula(typeof(Default<POFixedDemand.fixedSource>))]
			[PXDefault(typeof(Search<INItemSiteSettings.replenishmentSourceSiteID,
				Where<INItemSiteSettings.inventoryID, Equal<Current<POFixedDemand.inventoryID>>,
				And<INItemSiteSettings.siteID, Equal<Current<POFixedDemand.siteID>>,
				And<Where<Current<POFixedDemand.fixedSource>, Equal<INReplenishmentSource.transfer>, 
                Or<Current<POFixedDemand.fixedSource>, Equal<INReplenishmentSource.purchased>>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
			public override Int32? SourceSiteID
			{
				get
				{
					return this._SourceSiteID;
				}
				set
				{
					this._SourceSiteID = value;
				}
			}
		#endregion
		#region SourceSiteDescr
		public abstract class sourceSiteDescr : PX.Data.BQL.BqlString.Field<sourceSiteDescr> { }
		protected String _SourceSiteDescr;
		[PXFormula(typeof(Selector<sourceSiteID, INSite.descr>))]
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Demand Warehouse Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String SourceSiteDescr
		{
			get
			{
				return this._SourceSiteDescr;
			}
			set
			{
				this._SourceSiteDescr = value;
			}
		}
		#endregion
		#region POSiteID
		public abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }
            protected Int32? _POSiteID;
            [PXDBCalced(typeof(IsNull<SOLineSplit.pOSiteID, INItemPlan.siteID>), typeof(int))]
            [PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
            [PXDimensionSelector(SiteAttribute.DimensionName, typeof(Search<INSite.siteID>), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr), CacheGlobal = true)]
            public virtual Int32? POSiteID
            {
                get
                {
                    return this._POSiteID;
                }
                set
                {
                    this._POSiteID = value;
                }
            }
            #endregion
			#region VendorID
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			[Vendor(typeof(Search<BAccountR.bAccountID,
				Where<Vendor.type, NotEqual<BAccountType.employeeType>>>))]
			[PXRestrictor(typeof(Where<Vendor.status, IsNull,
									Or<Vendor.status, Equal<BAccount.status.active>,
									Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(Vendor.status))]
			[PXFormula(typeof(Default<POFixedDemand.fixedSource>))]
            [PXDefault(typeof(Coalesce<                
                Search2<BAccountR.bAccountID,
				InnerJoin<INItemSiteSettings, On<INItemSiteSettings.inventoryID, Equal<Current<POFixedDemand.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<POFixedDemand.siteID>>>>,
				LeftJoin<INSite, On<INSite.siteID, Equal<INItemSiteSettings.replenishmentSourceSiteID>>,
				LeftJoin<GL.Branch, On<GL.Branch.branchID, Equal<INSite.branchID>>>>>,
				Where<INItemSiteSettings.preferredVendorID, Equal<BAccountR.bAccountID>, And<Current<POFixedDemand.fixedSource>, NotEqual<INReplenishmentSource.transfer>,
                        Or<GL.Branch.bAccountID, Equal<BAccountR.bAccountID>, And<Current<POFixedDemand.fixedSource>, Equal<INReplenishmentSource.transfer>>>>>>,
                Search<InventoryItem.preferredVendorID,
				Where<InventoryItem.inventoryID, Equal<Current<POFixedDemand.inventoryID>>>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
			public override Int32? VendorID
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
			#region VendorLocationID
			public new abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }						
			[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POFixedDemand.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
			[PXFormula(typeof(Default<POFixedDemand.vendorID>))]
			public override Int32? VendorLocationID
			{
				get
				{
					return this._VendorLocationID;
				}
				set
				{
					this._VendorLocationID = value;
				}
			}
			#endregion
			#region RecordID
			public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
			protected Int32? _RecordID;
			[PXDBScalar(typeof(Search2<POVendorInventory.recordID,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<POVendorInventory.vendorID>>,
				InnerJoin<InventoryItem, 
					On<POVendorInventory.FK.InventoryItem>>>,
				Where<POVendorInventory.vendorID, Equal<INItemPlan.vendorID>,
				  And<POVendorInventory.inventoryID, Equal<INItemPlan.inventoryID>,
					And<POVendorInventory.active, Equal<boolTrue>,
					And2<Where<POVendorInventory.vendorLocationID, Equal<INItemPlan.vendorLocationID>,
					        Or<POVendorInventory.vendorLocationID, IsNull>>,
					And<Where<POVendorInventory.subItemID, Equal<INItemPlan.subItemID>,
								 Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>>>>>>>,
				OrderBy<
				Asc<Switch<Case<Where<POVendorInventory.vendorLocationID, Equal<INItemPlan.vendorLocationID>>, boolFalse>, boolTrue>,
				Asc<Switch<Case<Where<POVendorInventory.subItemID, Equal<INItemPlan.subItemID>>, boolFalse>, boolTrue>>>>>))]
			[PXFormula(typeof(Default<POFixedDemand.vendorLocationID>))]
			public virtual Int32? RecordID
			{
				get
				{
					return this._RecordID;
				}
				set
				{
					this._RecordID = value;
				}
			}
			#endregion
			#region SupplyPlanID
			public new abstract class supplyPlanID : PX.Data.BQL.BqlLong.Field<supplyPlanID> { }
			#endregion
			#region PlanQty
			public new abstract class planQty : PX.Data.BQL.BqlDecimal.Field<planQty> { }
			[PXDBQuantity()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Requested Qty.")]
			public override Decimal? PlanQty
			{
				get
				{
					return this._PlanQty;
				}
				set
				{
					this._PlanQty = value;
				}
			}
			#endregion		
			#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
			protected String _UOM;
			[PXDBString(BqlField = typeof(INUnit.fromUnit))]
			[PXUIField(DisplayName = "UOM")]
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
			#region UnitMultDiv
			public abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }
			protected String _UnitMultDiv;
			[PXDBString(1, IsFixed = true, BqlField = typeof(INUnit.unitMultDiv))]
			public virtual String UnitMultDiv
			{
				get
				{
					return this._UnitMultDiv;
				}
				set
				{
					this._UnitMultDiv = value;
				}
			}
			#endregion
			#region UnitRate
			public abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
			protected Decimal? _UnitRate;
			[PXDBDecimal(6, BqlField = typeof(INUnit.unitRate))]
			public virtual Decimal? UnitRate
			{
				get
				{
					return this._UnitRate;
				}
				set
				{
					this._UnitRate = value;
				}
			}
			#endregion
			#region PlanUnitQty
			public abstract class planUnitQty : PX.Data.BQL.BqlDecimal.Field<planUnitQty> { }
			protected Decimal? _PlanUnitQty;
			[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>, Mult<INItemPlan.planQty, INUnit.unitRate>>, Div<INItemPlan.planQty, INUnit.unitRate>>), typeof(decimal))]
			[PXQuantity()]
			public virtual Decimal? PlanUnitQty
			{
				get
				{
					return this._PlanUnitQty;
				}
				set
				{
					this._PlanUnitQty = value;
				}
			}
			#endregion
			#region OrderQty
			public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
			protected Decimal? _OrderQty;
			[PXQuantity()]
			[PXUIField(DisplayName = "Quantity")]
			public virtual Decimal? OrderQty
			{
				[PXDependsOnFields(typeof(planUnitQty))]
				get
				{
					return this._OrderQty ?? this._PlanUnitQty;
				}
				set
				{
					this._OrderQty = value;
				}
			}
			#endregion
			#region RefNoteID
			public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			[PXRefNote()]
			[PXUIField(DisplayName = "Reference Nbr.")]
			public override Guid? RefNoteID
			{
				get
				{
					return this._RefNoteID;
				}
				set
				{
					this._RefNoteID = value;
				}
			}
			#endregion
			#region Hold
			public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
			#endregion			
			#region VendorID_Vendor_acctName
			public abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
			#endregion
			#region InventoryID_InventoryItem_descr
			public abstract class inventoryID_InventoryItem_descr : PX.Data.BQL.BqlString.Field<inventoryID_InventoryItem_descr> { }
			#endregion
			#region SiteID_INSite_descr
			public abstract class siteID_INSite_descr : PX.Data.BQL.BqlString.Field<siteID_INSite_descr> { }
			#endregion			
			#region AddLeadTimeDays
			public abstract class addLeadTimeDays : PX.Data.BQL.BqlShort.Field<addLeadTimeDays> { }
			protected Int16? _AddLeadTimeDays;
			[PXShort()]
			[PXUIField(DisplayName = "Add. Lead Time (Days)")]
			public virtual Int16? AddLeadTimeDays
			{
				get
				{
					return this._AddLeadTimeDays;
				}
				set
				{
					this._AddLeadTimeDays = value;
				}
			}
			#endregion		
			#region EffPrice
			public abstract class effPrice : PX.Data.BQL.BqlDecimal.Field<effPrice> { }
			protected Decimal? _EffPrice;
			[PXPriceCost()]
			[PXUIField(DisplayName = "Vendor Price", Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? EffPrice
			{
				get
				{
					return this._EffPrice;
				}
				set
				{
					this._EffPrice = value;
				}
			}
			#endregion
			#region ExtWeight
			public abstract class extWeight : PX.Data.BQL.BqlDecimal.Field<extWeight> { }
			protected Decimal? _ExtWeight;
			[PXDecimal(6)]
			[PXUIField(DisplayName = "Weight")]
            [PXFormula(typeof(Mult<POFixedDemand.planQty, Selector<POFixedDemand.inventoryID, InventoryItem.baseWeight>>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? ExtWeight
			{
				get
				{
					return this._ExtWeight;
				}
				set
				{
					this._ExtWeight = value;
				}
			}
			#endregion
			#region ExtVolume
			public abstract class extVolume : PX.Data.BQL.BqlDecimal.Field<extVolume> { }
			protected Decimal? _ExtVolume;
			[PXDecimal(6)]
			[PXUIField(DisplayName = "Volume")]
            [PXFormula(typeof(Mult<POFixedDemand.planQty, Selector<POFixedDemand.inventoryID, InventoryItem.baseVolume>>))]			
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? ExtVolume
			{
				get
				{
					return this._ExtVolume;
				}
				set
				{
					this._ExtVolume = value;
				}
			}
			#endregion	
			#region ExtCost
			public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }
			protected Decimal? _ExtCost;
		[PXDecimal(typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<POCreate.POCreateFilter.vendorID>>>>))]
			[PXUIField(DisplayName = "Extended Amt.", Enabled = false)]
			[PXFormula(typeof(Mult<POFixedDemand.orderQty, POFixedDemand.effPrice>))]						
			public virtual Decimal? ExtCost
			{
				get
				{
					return this._ExtCost;
				}
				set
				{
					this._ExtCost = value;
				}
			}
			#endregion
			#region AlternateID
			public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
			protected String _AlternateID;
			[PXUIField(DisplayName = "Alternate ID")]
			[PXDBString(50, IsUnicode = true, InputMask = "", BqlField = typeof(SOLine.alternateID))]
			public virtual String AlternateID
			{
				get
				{
					return this._AlternateID;
				}
				set
				{
					this._AlternateID = value;
				}
			}
			#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(BqlField = typeof(SOLine.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
        #endregion
    }
}
