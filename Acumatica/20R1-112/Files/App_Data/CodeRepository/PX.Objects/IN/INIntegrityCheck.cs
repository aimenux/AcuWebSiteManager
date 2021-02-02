using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.SO;
using POReceiptLineSplit = PX.Objects.PO.POReceiptLineSplit;
using SOShipLineSplit = PX.Objects.SO.Table.SOShipLineSplit;

namespace PX.Objects.IN
{
    [Serializable]
    [PXHidden]
	public partial class ReadOnlySiteStatus : INSiteStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
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
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
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
	}

    [Serializable]
    [PXHidden]
	public partial class ReadOnlyLocationStatus : INLocationStatus
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ReadOnlyLocationStatus>.By<inventoryID, subItemID, siteID, locationID>
		{
			public static ReadOnlyLocationStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID)
				=> FindBy(graph, inventoryID, subItemID, siteID, locationID);
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
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
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
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
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? LocationID
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
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
	}

	[Serializable]
	[PXHidden]
	public partial class ReadOnlyLotSerialStatus : INLotSerialStatus
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ReadOnlyLotSerialStatus>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr>
		{
			public static ReadOnlyLotSerialStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr)
				=> FindBy(graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr);
		}
		public static new class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<ReadOnlyLotSerialStatus>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<ReadOnlyLotSerialStatus>.By<inventoryID, subItemID, siteID, locationID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<ReadOnlyLotSerialStatus>.By<subItemID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<ReadOnlyLotSerialStatus>.By<inventoryID> { }
			public class ItemLotSerial : INItemLotSerial.PK.ForeignKeyOf<ReadOnlyLotSerialStatus>.By<inventoryID, lotSerialNbr> { }
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
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
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
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
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? LocationID
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
		public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		[PXDBString(IsKey = true)]
		[PXDefault()]
		public override string LotSerialNbr
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
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
	}

	[Serializable]
	[PXHidden]
    public partial class INItemSiteSummary : INItemSite 
    {
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
    }

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INIntegrityCheck : PXGraph<INIntegrityCheck>
	{
		public PXCancel<INSiteFilter> Cancel;
		public PXFilter<INSiteFilter> Filter;
		[PXFilterable]
        public PXFilteredProcessingJoin<InventoryItem,
			INSiteFilter,
            LeftJoin<INSiteStatusSummary, On<INSiteStatusSummary.inventoryID, Equal<InventoryItem.inventoryID>,
				And<INSiteStatusSummary.siteID, Equal<Current<INSiteFilter.siteID>>>>>,
			Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>,
            OrderBy<Asc<InventoryItem.inventoryCD>>>
			INItemList;
		public PXSetup<INSetup> insetup;
		public PXSelect<INItemSite> itemsite;
		public PXSelect<INSiteStatus> sitestatus_s;
		public PXSelect<SiteStatus> sitestatus;
		public PXSelect<LocationStatus> locationstatus;
		public PXSelect<LotSerialStatus> lotserialstatus;
        public PXSelect<ItemLotSerial> itemlotserial;
		public PXSelect<SiteLotSerial> sitelotserial;
		public PXSelect<INItemPlan> initemplan;

		public PXSelect<ItemSiteHist> itemsitehist;
        public PXSelect<ItemSiteHistD> itemsitehistd;
        public PXSelect<ItemSiteHistDay> itemsitehistday;
        public PXSelect<ItemCostHist> itemcosthist;
		public PXSelect<ItemSalesHistD> itemsalehistd;
		public PXSelect<ItemCustSalesStats> itemcustsalesstats;
		public PXSelect<ItemCustDropShipStats> itemcustdropshipstats;

		public INIntegrityCheck()
		{
			INSetup record = insetup.Current;

			INItemList.SetProcessCaption(Messages.Process);
			INItemList.SetProcessAllCaption(Messages.ProcessAll);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.refNbr>(this.Caches[typeof(INTranSplit)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.tranDate>(this.Caches[typeof(INTranSplit)], null, false);
		}

        protected IEnumerable initemlist()
        {
            if (Filter.Current.SiteID != null)
            {
                return null;
            }
            return new List<object>();
        }

		protected virtual void INSiteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            INSiteFilter filter = (INSiteFilter)e.Row;

            INItemList.SetProcessDelegate<INIntegrityCheck>(delegate(INIntegrityCheck graph, InventoryItem item)
			{
				graph.Clear(PXClearOption.PreserveTimeStamp);
                graph.IntegrityCheckProc(new INItemSiteSummary { InventoryID = item.InventoryID, SiteID = filter != null ? filter.SiteID : null }, filter != null && filter.RebuildHistory == true ? filter.FinPeriodID : null, filter.ReplanBackorders == true);
			});
			PXUIFieldAttribute.SetEnabled<INSiteFilter.finPeriodID>(sender, null, filter.RebuildHistory == true);
		}

		/// <summary>
		/// Is needed because of a restriciton for <see cref="SiteStatus.qtyActual"/> in <see cref="SiteStatusAccumulatorAttribute.PrepareInsert"/>
		/// </summary>
        protected virtual void SiteStatus_NegAvailQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = true;
            e.Cancel = true;
        }

		/// <summary>
		/// Is needed because of a restriciton for <see cref="SiteStatus.qtyActual"/> in <see cref="SiteStatusAccumulatorAttribute.PrepareInsert"/>
		/// </summary>
		protected virtual void SiteStatus_NegQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}

		/// <summary>
		/// Is needed because of a restriciton for <see cref="SiteLotSerial.qtyActual"/> in <see cref="SiteLotSerialAccumulatorAttribute.PrepareInsert"/>
		/// </summary>
		protected virtual void SiteLotSerial_NegQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}
        
		private TNode UpdateAllocatedQuantities<TNode>(INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			INPlanType targettype = INItemPlanIDAttribute.GetTargetPlanTypeBase<TNode>(this, plan, plantype);
			return INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<TNode>(this, plan, targettype, InclQtyAvail);
		}

		public virtual void IntegrityCheckProc(INItemSiteSummary itemsite, string minPeriod, bool replanBackorders)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
			using (var ts = new PXTransactionScope())
			{
				DeleteOrphanedItemPlans(itemsite);

				CreateItemPlansForTransit(itemsite);

				DeleteLotSerialStatusForNotTrackedItems(itemsite);

				ClearSiteStatusAllocatedQuantities(itemsite);
				ClearLocationStatusAllocatedQuantities(itemsite);
				ClearLotSerialStatusAllocatedQuantities(itemsite);

				PopulateSiteAvailQtyByLocationStatus(itemsite);

				UpdateAllocatedQuantitiesWithExistingPlans(itemsite);

				ReplanBackOrders(replanBackorders);

				Caches[typeof(INTranSplit)].Persist(PXDBOperation.Update);

				sitestatus.Cache.Persist(PXDBOperation.Insert);
				sitestatus.Cache.Persist(PXDBOperation.Update);

				locationstatus.Cache.Persist(PXDBOperation.Insert);
				locationstatus.Cache.Persist(PXDBOperation.Update);

				lotserialstatus.Cache.Persist(PXDBOperation.Insert);
				lotserialstatus.Cache.Persist(PXDBOperation.Update);

				itemlotserial.Cache.Persist(PXDBOperation.Insert);
				itemlotserial.Cache.Persist(PXDBOperation.Update);

				sitelotserial.Cache.Persist(PXDBOperation.Insert);
				sitelotserial.Cache.Persist(PXDBOperation.Update);

				RebuildItemHistory(minPeriod, itemsite);

				DeleteZeroStatusRecords(itemsite);

				ts.Complete();
			}

			initemplan.Cache.Persisted(false);
			Caches[typeof(INTranSplit)].Persisted(false);
			sitestatus.Cache.Persisted(false);
			locationstatus.Cache.Persisted(false);
			lotserialstatus.Cache.Persisted(false);
			itemlotserial.Cache.Persisted(false);
			sitelotserial.Cache.Persisted(false);

			itemcosthist.Cache.Persisted(false);
			itemsitehist.Cache.Persisted(false);
			itemsitehistd.Cache.Persisted(false);
			itemsitehistday.Cache.Persisted(false);
			itemsalehistd.Cache.Persisted(false);
			itemcustsalesstats.Cache.Persisted(false);
			itemcustdropshipstats.Cache.Persisted(false);
		}
		}

		protected virtual void DeleteOrphanedItemPlans(INItemSiteSummary itemsite)
		{
			DeleteItemPlansWithoutParentDocument(itemsite);

			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				InnerJoin<INRegister, On<INRegister.noteID, Equal<INItemPlan.refNoteID>, And<INRegister.siteID, Equal<INItemPlan.siteID>>>>,
				Where<INRegister.docType, Equal<INDocType.transfer>,
					And<INRegister.released, Equal<boolTrue>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d22.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
			InnerJoin<PO.POReceipt, On<PO.POReceipt.noteID, Equal<INItemPlan.refNoteID>>,
			LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.receiptNbr, Equal<PO.POReceipt.receiptNbr>
				, And<POReceiptLineSplit.receiptType, Equal<PO.POReceipt.receiptType>
				, And<POReceiptLineSplit.planID, Equal<INItemPlan.planID>>>>>>,
				Where<POReceiptLineSplit.receiptNbr, IsNull,
				And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
				And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d32.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
			InnerJoin<SOOrder, On<SOOrder.noteID, Equal<INItemPlan.refNoteID>>,
			LeftJoin<SOLineSplit, On<SOLineSplit.orderType, Equal<SOOrder.orderType>
				, And<SOLineSplit.orderNbr, Equal<SOOrder.orderNbr>
				, And<SOLineSplit.planID, Equal<INItemPlan.planID>>>>>>,
				Where<SOLineSplit.orderNbr, IsNull,
					And<INItemPlan.planType, NotEqual<INPlanConstants.plan64>,
				And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d33.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
			InnerJoin<SOShipment, On<SOShipment.noteID, Equal<INItemPlan.refNoteID>>,
			LeftJoin<SOShipLineSplit, On<SOShipLineSplit.shipmentNbr, Equal<SOShipment.shipmentNbr>
				, And<SOShipLineSplit.planID, Equal<INItemPlan.planID>>>>>,
				Where<SOShipLineSplit.shipmentNbr, IsNull,
				And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
				And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d128.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				LeftJoin<INItemPlanSupply, On<INItemPlanSupply.planID, Equal<INItemPlan.supplyPlanID>>>,
				Where<INItemPlanSupply.planID, IsNull,
					And<INItemPlan.supplyPlanID, IsNotNull,
					And<INItemPlan.planType, Equal<INPlanConstants.plan94>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}
		}

		protected virtual void DeleteItemPlansWithoutParentDocument(INItemSiteSummary itemsite)
		{
			Type[] knownNoteFields = GetParentDocumentsNoteFields();

			var docTypeNames = new List<string>();

			foreach (var noteField in knownNoteFields)
			{
				Type docType = BqlCommand.GetItemType(noteField);

				var command = BqlTemplate.OfCommand<Select2<INItemPlan,
					LeftJoin<BqlPlaceholder.E, On<BqlPlaceholder.N, Equal<INItemPlan.refNoteID>>>,
					Where<INItemPlan.refEntityType, Equal<Required<INItemPlan.refEntityType>>,
						And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
						And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
						And<BqlPlaceholder.N, IsNull>>>>>>
					.Replace<BqlPlaceholder.E>(docType)
					.Replace<BqlPlaceholder.N>(noteField)
					.ToCommand();

				var view = new PXView(this, true, command);

				string docTypeName = docType.FullName;

				foreach (PXResult<INItemPlan> row in view.SelectMultiBound(new object[] { itemsite }, new object[] { docTypeName }))
				{
					INItemPlan p = row;
					PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
				}

				docTypeNames.Add(docTypeName);
			}

			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				LeftJoin<Note, On<Note.noteID, Equal<INItemPlan.refNoteID>>>,
				Where<INItemPlan.refEntityType, NotIn<Required<INItemPlan.refEntityType>>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<Note.noteID, IsNull>>>>>
				.SelectMultiBound(this, new object[] { itemsite }, new object[] { docTypeNames.ToArray() }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}
		}

		public virtual Type[] GetParentDocumentsNoteFields()
		{
			List<Type> knownNoteFields = new List<Type>() {
				typeof(SOOrder.noteID),
				typeof(SOShipment.noteID),
				typeof(PO.POOrder.noteID),
				typeof(PO.POReceipt.noteID),
				typeof(INRegister.noteID),
				typeof(INTransitLine.noteID),
			};

			if (PXAccess.FeatureInstalled<FeaturesSet.replenishment>())
				knownNoteFields.Add(typeof(INReplenishmentOrder.noteID));

			if (PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>())
				knownNoteFields.Add(typeof(INKitRegister.noteID));

			return knownNoteFields.ToArray();
		}

		protected virtual void CreateItemPlansForTransit(INItemSiteSummary itemsite)
		{
			var transferGraph = CreateInstance<INTransferEntry>();
			foreach (PXResult<INLocationStatus2, INTransitLine> res in PXSelectJoin<INLocationStatus2,
					InnerJoin<INTransitLine, On<INTransitLine.costSiteID, Equal<INLocationStatus2.locationID>>,
					LeftJoin<INItemPlan, On<INItemPlan.refNoteID, Equal<INTransitLine.noteID>>>>,
					Where<INLocationStatus2.qtyOnHand, Greater<decimal0>,
						And<INLocationStatus2.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
						And<INTransitLine.toSiteID, Equal<Current<INItemSiteSummary.siteID>>,
						And<INItemPlan.planID, IsNull>>>>>.SelectMultiBound(transferGraph, new object[] { itemsite }))
			{
				INItemPlan plan;
				var locst = (INLocationStatus2)res;
				var tl = (INTransitLine)res;


				foreach (TransitLotSerialStatus tlss in
					PXSelect<TransitLotSerialStatus,
					Where<TransitLotSerialStatus.locationID, Equal<Current<INLocationStatus2.locationID>>,
					And<TransitLotSerialStatus.inventoryID, Equal<Current<INLocationStatus2.inventoryID>>,
					And<TransitLotSerialStatus.subItemID, Equal<Current<INLocationStatus2.subItemID>>,
					And<TransitLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>>.SelectMultiBound(transferGraph, new object[] { locst }))
				{
					plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].CreateInstance();
					plan.PlanType = tl.SOShipmentNbr == null ? INPlanConstants.Plan42 : INPlanConstants.Plan44;
					plan.InventoryID = tlss.InventoryID;
					plan.SubItemID = tlss.SubItemID ?? locst.SubItemID;
					plan.LotSerialNbr = tlss.LotSerialNbr;
					plan.SiteID = tl.ToSiteID;
					plan.LocationID = tl.ToLocationID;
					plan.FixedSource = INReplenishmentSource.Purchased;
					plan.PlanDate = tl.CreatedDateTime;
					plan.Reverse = false;
					plan.Hold = false;
					plan.PlanQty = tlss.QtyOnHand;
					locst.QtyOnHand -= tlss.QtyOnHand;
					plan.RefNoteID = tl.NoteID;
					plan.RefEntityType = typeof(INTransitLine).FullName;
					plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].Insert(plan);
				}

				if (locst.QtyOnHand <= 0m)
					continue;
				plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].CreateInstance();
				plan.PlanType = tl.SOShipmentNbr == null ? INPlanConstants.Plan42 : INPlanConstants.Plan44;
				plan.InventoryID = locst.InventoryID;
				plan.SubItemID = locst.SubItemID;
				plan.SiteID = tl.ToSiteID;
				plan.LocationID = tl.ToLocationID;
				plan.FixedSource = INReplenishmentSource.Purchased;
				plan.PlanDate = tl.CreatedDateTime;
				plan.Reverse = false;
				plan.Hold = false;
				plan.PlanQty = locst.QtyOnHand;
				plan.RefNoteID = tl.NoteID;
				plan.RefEntityType = typeof(INTransitLine).FullName;
				plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].Insert(plan);
			}
			transferGraph.Save.Press();
		}

		protected virtual void DeleteLotSerialStatusForNotTrackedItems(INItemSiteSummary itemsite)
		{
			//Deleting records from INLotSerialStatus, INItemLotSerial, INSiteLotSerial if item is not Lot/Serial tracked any more
			InventoryItem notTrackedItem = PXSelectReadonly2<InventoryItem,
				InnerJoin<INLotSerClass,
					On2<InventoryItem.FK.LotSerClass, And<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.notNumbered>>>,
				InnerJoin<INLotSerialStatus, On<INLotSerialStatus.FK.InventoryItem>>>,
				Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(this, 0, 1, itemsite.InventoryID);
			if (notTrackedItem != null && !InventoryItemMaint.IsQtyStillPresent(this, itemsite.InventoryID))
			{
				PXDatabase.Delete<INLotSerialStatus>(
					new PXDataFieldRestrict<INLotSerialStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
					new PXDataFieldRestrict<INLotSerialStatus.qtyOnHand>(PXDbType.Decimal, 4, 0m, PXComp.EQ)
				);

				PXDatabase.Delete<INItemLotSerial>(
					new PXDataFieldRestrict<INItemLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
					new PXDataFieldRestrict<INItemLotSerial.qtyOnHand>(PXDbType.Decimal, 4, 0m, PXComp.EQ)
				);

				PXDatabase.Delete<INSiteLotSerial>(
					new PXDataFieldRestrict<INSiteLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
					new PXDataFieldRestrict<INSiteLotSerial.qtyOnHand>(PXDbType.Decimal, 4, 0m, PXComp.EQ)
				);
			}
		}

		protected virtual void ClearSiteStatusAllocatedQuantities(INItemSiteSummary itemsite)
		{
			PXDatabase.Update<INSiteStatus>(
				AssignAllDBDecimalFieldsToZeroCommand(sitestatus.Cache,
					excludeFields: new string[]
					{
						nameof(INLocationStatus.qtyOnHand)
					})
				.Append(new PXDataFieldRestrict<INSiteStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INSiteStatus.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());
		}

		protected virtual void ClearLocationStatusAllocatedQuantities(INItemSiteSummary itemsite)
		{
			PXDatabase.Update<INLocationStatus>(
				AssignAllDBDecimalFieldsToZeroCommand(locationstatus.Cache,
					excludeFields: new string[]
					{
						nameof(INLocationStatus.qtyOnHand),
						nameof(INLocationStatus.qtyAvail),
						nameof(INLocationStatus.qtyHardAvail),
						nameof(INLocationStatus.qtyActual)
					})
				.Append(new PXDataFieldAssign<INLocationStatus.qtyAvail>(PXDbType.DirectExpression, nameof(INLocationStatus.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLocationStatus.qtyHardAvail>(PXDbType.DirectExpression, nameof(INLocationStatus.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLocationStatus.qtyActual>(PXDbType.DirectExpression, nameof(INLocationStatus.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INLocationStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INLocationStatus.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());
		}

		protected virtual void ClearLotSerialStatusAllocatedQuantities(INItemSiteSummary itemsite)
		{
			PXDatabase.Update<INLotSerialStatus>(
				AssignAllDBDecimalFieldsToZeroCommand(lotserialstatus.Cache,
					excludeFields: new string[]
					{
						nameof(INLotSerialStatus.qtyOnHand),
						nameof(INLotSerialStatus.qtyAvail),
						nameof(INLotSerialStatus.qtyHardAvail),
						nameof(INLotSerialStatus.qtyActual)
					})
				.Append(new PXDataFieldAssign<INLotSerialStatus.qtyAvail>(PXDbType.DirectExpression, nameof(INLotSerialStatus.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLotSerialStatus.qtyHardAvail>(PXDbType.DirectExpression, nameof(INLotSerialStatus.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLotSerialStatus.qtyActual>(PXDbType.DirectExpression, nameof(INLotSerialStatus.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INLotSerialStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INLotSerialStatus.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());

			PXDatabase.Update<INItemLotSerial>(
				AssignAllDBDecimalFieldsToZeroCommand(itemlotserial.Cache,
					excludeFields: new string[]
					{
						nameof(INItemLotSerial.qtyOnHand),
						nameof(INItemLotSerial.qtyAvail),
						nameof(INItemLotSerial.qtyHardAvail),
						nameof(INItemLotSerial.qtyActual),
						nameof(INItemLotSerial.qtyOrig)
					})
				.Append(new PXDataFieldAssign<INItemLotSerial.qtyAvail>(PXDbType.DirectExpression, nameof(INItemLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INItemLotSerial.qtyHardAvail>(PXDbType.DirectExpression, nameof(INItemLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INItemLotSerial.qtyActual>(PXDbType.DirectExpression, nameof(INItemLotSerial.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INItemLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.ToArray());

			PXDatabase.Update<INSiteLotSerial>(
				AssignAllDBDecimalFieldsToZeroCommand(sitelotserial.Cache,
					excludeFields: new string[]
					{
						nameof(INSiteLotSerial.qtyOnHand),
						nameof(INSiteLotSerial.qtyAvail),
						nameof(INSiteLotSerial.qtyHardAvail),
						nameof(INSiteLotSerial.qtyActual)
					})
				.Append(new PXDataFieldAssign<INSiteLotSerial.qtyAvail>(PXDbType.DirectExpression, nameof(INSiteLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INSiteLotSerial.qtyHardAvail>(PXDbType.DirectExpression, nameof(INSiteLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INSiteLotSerial.qtyActual>(PXDbType.DirectExpression, nameof(INSiteLotSerial.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INSiteLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INSiteLotSerial.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());
		}

		protected virtual void PopulateSiteAvailQtyByLocationStatus(INItemSiteSummary itemsite)
		{
			foreach (PXResult<ReadOnlyLocationStatus, INLocation> res in PXSelectJoinGroupBy<ReadOnlyLocationStatus,
				InnerJoin<INLocation, On<INLocation.locationID, Equal<ReadOnlyLocationStatus.locationID>>>,
				Where<ReadOnlyLocationStatus.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<ReadOnlyLocationStatus.siteID, Equal<Current<INItemSiteSummary.siteID>>>>,
				Aggregate<GroupBy<ReadOnlyLocationStatus.inventoryID,
					GroupBy<ReadOnlyLocationStatus.siteID,
					GroupBy<ReadOnlyLocationStatus.subItemID,
					GroupBy<INLocation.inclQtyAvail,
					Sum<ReadOnlyLocationStatus.qtyOnHand>>>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				SiteStatus status = new SiteStatus();
				status.InventoryID = ((ReadOnlyLocationStatus)res).InventoryID;
				status.SubItemID = ((ReadOnlyLocationStatus)res).SubItemID;
				status.SiteID = ((ReadOnlyLocationStatus)res).SiteID;
				status = (SiteStatus)sitestatus.Cache.Insert(status);

				if (((INLocation)res).InclQtyAvail == true)
				{
					status.QtyAvail += ((ReadOnlyLocationStatus)res).QtyOnHand;
					status.QtyHardAvail += ((ReadOnlyLocationStatus)res).QtyOnHand;
					status.QtyActual += ((ReadOnlyLocationStatus)res).QtyOnHand;
				}
				else
				{
					status.QtyNotAvail += ((ReadOnlyLocationStatus)res).QtyOnHand;
				}
			}
		}

		protected virtual void UpdateAllocatedQuantitiesWithExistingPlans(INItemSiteSummary itemsite)
		{
			foreach (PXResult<INItemPlan, InventoryItem> res in PXSelectJoin<INItemPlan,
				InnerJoin<InventoryItem, On<INItemPlan.FK.InventoryItem>>,
				Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<InventoryItem.stkItem, Equal<boolTrue>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				INItemPlan plan = (INItemPlan)res;
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);

				if (plan.InventoryID != null &&
					plan.SubItemID != null &&
					plan.SiteID != null)
				{
					if (plan.LocationID != null)
					{
						LocationStatus item = UpdateAllocatedQuantities<LocationStatus>(plan, plantype, true);
						UpdateAllocatedQuantities<SiteStatus>(plan, plantype, (bool)item.InclQtyAvail);
						if (!string.IsNullOrEmpty(plan.LotSerialNbr))
						{
							UpdateAllocatedQuantities<LotSerialStatus>(plan, plantype, true);
							UpdateAllocatedQuantities<SiteLotSerial>(plan, plantype, true);
						}
					}
					else
					{
						UpdateAllocatedQuantities<SiteStatus>(plan, plantype, true);
						if (!string.IsNullOrEmpty(plan.LotSerialNbr))
						{
							//TODO: check if LotSerialNbr was allocated on OrigPlanType
							UpdateAllocatedQuantities<SiteLotSerial>(plan, plantype, true);
						}
					}
				}
			}

			//Updating cross-site ItemLotSerial
			foreach (INItemPlan plan in PXSelect<INItemPlan,
					Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
						And<INItemPlan.lotSerialNbr, NotEqual<StringEmpty>,
						And<INItemPlan.lotSerialNbr, IsNotNull>>>>
					.SelectMultiBound(this, new object[] { itemsite }))
			{
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);

				if (plan.InventoryID != null &&
					plan.SubItemID != null &&
					plan.SiteID != null)
				{
					if (plan.LocationID != null)
					{
						UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
					}
					else
					{
						UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
					}
				}
			}
		}

		protected virtual void ReplanBackOrders(bool replanBackorders)
		{
			if (!replanBackorders) return;

			INReleaseProcess.ReplanBackOrders(this);
			initemplan.Cache.Persist(PXDBOperation.Insert);
			initemplan.Cache.Persist(PXDBOperation.Update);
		}

		protected virtual void RebuildItemHistory(string minPeriod, INItemSiteSummary itemsite)
		{
			if (minPeriod == null)
				return;

			MasterFinPeriod period =
				PXSelect<MasterFinPeriod,
					Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>
					.SelectWindowed(this, 0, 1, minPeriod);
			if (period == null) return;
			DateTime startDate = (DateTime)period.StartDate;

			PXDatabase.Delete<INItemCostHist>(
				new PXDataFieldRestrict<INItemCostHist.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCostHist.costSiteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCostHist.finPeriodID>(PXDbType.Char, 6, minPeriod, PXComp.GE));

			PXDatabase.Update<INItemSalesHistD>(
				AssignAllDBDecimalFieldsToZeroCommand(itemsalehistd.Cache,
					excludeFields: new string[]
					{
						nameof(INItemSalesHistD.qtyPlanSales),
						nameof(INItemSalesHistD.qtyLostSales)
					})
				.Append(new PXDataFieldRestrict<INItemSalesHistD.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSalesHistD.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSalesHistD.sDate>(PXDbType.DateTime, 8, startDate, PXComp.GE))
				.ToArray());

			PXDatabase.Update<INItemCustSalesStats>(
				new PXDataFieldAssign<INItemCustSalesStats.lastQty>(PXDbType.Decimal, null),
				new PXDataFieldAssign<INItemCustSalesStats.lastDate>(PXDbType.DateTime, null),
				new PXDataFieldAssign<INItemCustSalesStats.lastUnitPrice>(PXDbType.Decimal, null),
				new PXDataFieldRestrict<INItemCustSalesStats.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.lastDate>(PXDbType.DateTime, 8, startDate, PXComp.GE));

			PXDatabase.Update<INItemCustSalesStats>(
				new PXDataFieldAssign<INItemCustSalesStats.dropShipLastQty>(PXDbType.Decimal, null),
				new PXDataFieldAssign<INItemCustSalesStats.dropShipLastDate>(PXDbType.DateTime, null),
				new PXDataFieldAssign<INItemCustSalesStats.dropShipLastUnitPrice>(PXDbType.Decimal, null),
				new PXDataFieldRestrict<INItemCustSalesStats.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.dropShipLastDate>(PXDbType.DateTime, 8, startDate, PXComp.GE));

			PXDatabase.Delete<INItemCustSalesStats>(
				new PXDataFieldRestrict<INItemCustSalesStats.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.lastDate>(PXDbType.DateTime, 8, startDate, PXComp.ISNULL),
				new PXDataFieldRestrict<INItemCustSalesStats.dropShipLastDate>(PXDbType.DateTime, 8, startDate, PXComp.ISNULL));

			foreach (INLocation loc in PXSelectJoinGroupBy<INLocation,
				InnerJoin<INItemCostHist, On<INItemCostHist.costSiteID, Equal<INLocation.locationID>>>,
				Where<INLocation.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INItemCostHist.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>>>,
				Aggregate<GroupBy<INLocation.locationID>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemCostHist>(
					new PXDataFieldRestrict<INItemCostHist.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
					new PXDataFieldRestrict<INItemCostHist.costSiteID>(PXDbType.Int, 4, loc.LocationID, PXComp.EQ),
					new PXDataFieldRestrict<INItemCostHist.finPeriodID>(PXDbType.Char, 6, minPeriod, PXComp.GE));
			}

			PXDatabase.Delete<INItemSiteHist>(
				new PXDataFieldRestrict<INItemSiteHist.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHist.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHist.finPeriodID>(PXDbType.Char, 6, minPeriod, PXComp.GE));

			PXDatabase.Update<INItemSiteHistD>(
				AssignAllDBDecimalFieldsToZeroCommand(itemsitehistd.Cache)
				.Append(new PXDataFieldRestrict<INItemSiteHistD.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSiteHistD.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSiteHistD.sDate>(PXDbType.DateTime, 8, startDate, PXComp.GE))
				.ToArray());

			PXDatabase.Delete<INItemSiteHistDay>(
				new PXDataFieldRestrict<INItemSiteHistDay.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHistDay.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHistDay.sDate>(PXDbType.DateTime, 8, startDate, PXComp.GE));

			INTran prev_tran = null;
			foreach (PXResult<INTran, INTranSplit> res in PXSelectReadonly2<INTran,
				LeftJoin<INTranSplit,
					On<INTranSplit.FK.Tran>>,
				Where<INTran.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INTran.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INTran.finPeriodID, GreaterEqual<Required<INTran.finPeriodID>>,
					And<INTran.released, Equal<boolTrue>>>>>,
				OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>.SelectMultiBound(this, new object[] { itemsite }, minPeriod))
			{
				INTran tran = res;
				INTranSplit split = res;

				if (!Caches[typeof(INTran)].ObjectsEqual(prev_tran, tran))
				{
					INReleaseProcess.UpdateSalesHistD(this, tran);
					INReleaseProcess.UpdateCustSalesStats(this, tran);

					prev_tran = tran;
				}

				if ((split.BaseQty ?? 0) != 0m)
				{
					INReleaseProcess.UpdateSiteHist(this, res, split);
					INReleaseProcess.UpdateSiteHistD(this, split);
					INReleaseProcess.UpdateSiteHistDay(this, res, split);
				}
			}

			foreach (PXResult<INTran, INTranCost> res in
				PXSelectReadonly2<INTran,
				InnerJoin<INTranCost,
					On<INTranCost.FK.Tran>>,
				Where<INTran.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INTran.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INTranCost.finPeriodID, GreaterEqual<Required<INTran.finPeriodID>>,
					And<INTran.released, Equal<boolTrue>,
					And<INTranCost.costSiteID, NotEqual<SiteAttribute.transitSiteID>>>>>>>
					.SelectMultiBound(this, new object[] { itemsite }, minPeriod))
			{
				INReleaseProcess.UpdateCostHist(this, res, res);
			}

			itemcosthist.Cache.Persist(PXDBOperation.Insert);
			itemcosthist.Cache.Persist(PXDBOperation.Update);

			itemsitehist.Cache.Persist(PXDBOperation.Insert);
			itemsitehist.Cache.Persist(PXDBOperation.Update);

			itemsitehistd.Cache.Persist(PXDBOperation.Insert);
			itemsitehistd.Cache.Persist(PXDBOperation.Update);

			itemsitehistday.Cache.Persist(PXDBOperation.Insert);
			itemsitehistday.Cache.Persist(PXDBOperation.Update);

			itemsalehistd.Cache.Persist(PXDBOperation.Insert);
			itemsalehistd.Cache.Persist(PXDBOperation.Update);

			itemcustsalesstats.Cache.Persist(PXDBOperation.Insert);
			itemcustsalesstats.Cache.Persist(PXDBOperation.Update);

			itemcustdropshipstats.Cache.Persist(PXDBOperation.Insert);
			itemcustdropshipstats.Cache.Persist(PXDBOperation.Update);
		}

		protected virtual IEnumerable<PXDataFieldParam> AssignAllDBDecimalFieldsToZeroCommand(PXCache cache, params string[] excludeFields)
		{
			return cache.GetAllDBDecimalFields(excludeFields)
				.Select(f => new PXDataFieldAssign(f, PXDbType.Decimal, decimal.Zero));
		}

		public virtual void DeleteZeroStatusRecords(INItemSiteSummary itemsite)
		{
			var restrictions = new List<PXDataFieldRestrict>
			{
				new PXDataFieldRestrict(nameof(INLocationStatus.InventoryID), PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict(nameof(INLocationStatus.SiteID), PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				// just for reliability as it may cause very sensitive data loss
				new PXDataFieldRestrict(nameof(INLocationStatus.QtyOnHand), PXDbType.Decimal, decimal.Zero),
				new PXDataFieldRestrict(nameof(INLocationStatus.QtyAvail), PXDbType.Decimal, decimal.Zero),
				new PXDataFieldRestrict(nameof(INLocationStatus.QtyHardAvail), PXDbType.Decimal, decimal.Zero),
			};
			restrictions.AddRange(
				locationstatus.Cache.GetAllDBDecimalFields()
				.Select(f => new PXDataFieldRestrict(f, PXDbType.Decimal, decimal.Zero)));
			PXDatabase.Delete<INLocationStatus>(restrictions.ToArray());
		}
    }
}
