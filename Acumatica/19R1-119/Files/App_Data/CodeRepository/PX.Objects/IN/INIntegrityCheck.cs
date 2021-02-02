using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.CS;
using SOShipLineSplit = PX.Objects.SO.Table.SOShipLineSplit;
using POReceiptLineSplit = PX.Objects.PO.POReceiptLineSplit;
using INItemPlanDemand = PX.Objects.IN.INReleaseProcess.INItemPlanDemand;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.SO;

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
			Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>,
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

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
        public virtual bool PlanTypeCalc(INItemPlan plan, SOShipLineSplit sosplit, POReceiptLineSplit posplit, ref INPlanType plantype, out INPlanType locplantype)
        {
                    locplantype = plantype;
            return true;
        }

		private TNode UpdateAllocatedQuantities<TNode>(INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			INPlanType targettype = INItemPlanIDAttribute.GetTargetPlanTypeBase<TNode>(this, plan, plantype);
			return INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<TNode>(this, plan, targettype, InclQtyAvail);
		}

		protected HashSet<Guid?> _missingNoteIds;
		protected virtual HashSet<Guid?> GetMissingNoteIds()
		{
			if (_missingNoteIds != null)
				return _missingNoteIds;

			_missingNoteIds = new HashSet<Guid?>();

			_missingNoteIds.UnionWith(
				PXSelectJoin<INRegister,
					LeftJoin<Note, On<Note.noteID, Equal<INRegister.noteID>>>,
					Where<Note.noteID, IsNull>>
				.Select(this)
				.RowCast<INRegister>()
				.Select(r => r.NoteID));

			_missingNoteIds.UnionWith(
				PXSelectJoin<PO.POOrder,
					LeftJoin<Note, On<Note.noteID, Equal<PO.POOrder.noteID>>>,
					Where<Note.noteID, IsNull>>
				.Select(this)
				.RowCast<PO.POOrder>()
				.Select(r => r.NoteID));

			_missingNoteIds.UnionWith(
				PXSelectJoin<PO.POReceipt,
					LeftJoin<Note, On<Note.noteID, Equal<PO.POReceipt.noteID>>>,
					Where<Note.noteID, IsNull>>
				.Select(this)
				.RowCast<PO.POReceipt>()
				.Select(r => r.NoteID));

			_missingNoteIds.UnionWith(
				PXSelectJoin<SOOrder,
					LeftJoin<Note, On<Note.noteID, Equal<SOOrder.noteID>>>,
					Where<Note.noteID, IsNull>>
				.Select(this)
				.RowCast<SOOrder>()
				.Select(r => r.NoteID));

			_missingNoteIds.UnionWith(
				PXSelectJoin<SOShipment,
					LeftJoin<Note, On<Note.noteID, Equal<SOShipment.noteID>>>,
					Where<Note.noteID, IsNull>>
				.Select(this)
				.RowCast<SOShipment>()
				.Select(r => r.NoteID));

			return _missingNoteIds;
		}

        public virtual void IntegrityCheckProc(INItemSiteSummary itemsite, string minPeriod, bool replanBackorders)
        {
			var missingNoteIds = GetMissingNoteIds();
            using (PXConnectionScope cs = new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
						LeftJoin<Note, On<Note.noteID, Equal<INItemPlan.refNoteID>>>,
						Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
							And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
							And<Note.noteID, IsNull>>>>.SelectMultiBound(this, new object[] { itemsite }))
                    {
						// According to AC-130473 Note records can be removed when the related documents were not completely processed yet.
						if (missingNoteIds.Contains(p.RefNoteID))
							continue;

                        PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
                    }

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

					DeleteClosedPOPlans(itemsite);

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
                            Where <TransitLotSerialStatus.locationID, Equal<Current<INLocationStatus2.locationID>>,
                            And<TransitLotSerialStatus.inventoryID, Equal<Current<INLocationStatus2.inventoryID >>,
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
                        plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].Insert(plan);
                    }
                    transferGraph.Save.Press();

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

                    PXDatabase.Update<INSiteStatus>(
							new PXDataFieldRestrict<INSiteStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict<INSiteStatus.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldAssign<INSiteStatus.qtyAvail>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyHardAvail>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyActual>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyNotAvail>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyINIssues>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyINReceipts>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyInTransit>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyINAssemblySupply>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyINAssemblyDemand>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyInTransitToProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyProductionSupplyPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyProductionSupply>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyPOFixedProductionPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyPOFixedProductionOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyProductionDemandPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyProductionDemand>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyProductionAllocated>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtySOFixedProduction>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyProdFixedPurchase>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyProdFixedProduction>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyProdFixedProdOrdersPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyProdFixedProdOrders>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyProdFixedSalesOrdersPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyProdFixedSalesOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyINReplaned>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPOPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPOOrders>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPOReceipts>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INSiteStatus.qtyFSSrvOrdPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyFSSrvOrdBooked>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyFSSrvOrdAllocated>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INSiteStatus.qtySOPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtySOBooked>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtySOShipped>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtySOShipping>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtySOBackOrdered>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INSiteStatus.qtyFixedFSSrvOrd>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyPOFixedFSSrvOrd>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyPOFixedFSSrvOrdPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyPOFixedFSSrvOrdReceipts>(PXDbType.Decimal, 0m),


                            new PXDataFieldAssign<INSiteStatus.qtySOFixed>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPOFixedOrders>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPOFixedPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPOFixedReceipts>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtySODropShip>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPODropShipOrders>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPODropShipPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INSiteStatus.qtyPODropShipReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INSiteStatus.qtyInTransitToSO>(PXDbType.Decimal, 0m)
                        );

                    PXDatabase.Update<INLocationStatus>(
                            new PXDataFieldRestrict<INLocationStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict<INLocationStatus.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldAssign<INLocationStatus.qtyAvail>(PXDbType.DirectExpression, "QtyOnHand"),
                            new PXDataFieldAssign<INLocationStatus.qtyHardAvail>(PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INLocationStatus.qtyActual>(PXDbType.DirectExpression, "QtyOnHand"),
                            new PXDataFieldAssign<INLocationStatus.qtyINIssues>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyINReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyInTransit>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyINAssemblySupply>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyINAssemblyDemand>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyInTransitToProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProductionSupplyPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProductionSupply>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedProductionPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedProductionOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProductionDemandPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProductionDemand>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProductionAllocated>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtySOFixedProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProdFixedPurchase>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProdFixedProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProdFixedProdOrdersPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProdFixedProdOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProdFixedSalesOrdersPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyProdFixedSalesOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOReceipts>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLocationStatus.qtyFSSrvOrdPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyFSSrvOrdBooked>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyFSSrvOrdAllocated>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLocationStatus.qtySOPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INLocationStatus.qtySOBooked>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtySOShipped>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtySOShipping>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtySOBackOrdered>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLocationStatus.qtyFixedFSSrvOrd>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedFSSrvOrd>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedFSSrvOrdPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedFSSrvOrdReceipts>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLocationStatus.qtySOFixed>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPOFixedReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtySODropShip>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPODropShipOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPODropShipPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyPODropShipReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLocationStatus.qtyInTransitToSO>(PXDbType.Decimal, 0m)
                        );

                    PXDatabase.Update<INLotSerialStatus>(
                            new PXDataFieldRestrict<INLotSerialStatus.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict<INLotSerialStatus.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldAssign<INLotSerialStatus.qtyAvail>(PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INLotSerialStatus.qtyHardAvail>(PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INLotSerialStatus.qtyActual>(PXDbType.DirectExpression, "QtyOnHand"),
                            new PXDataFieldAssign<INLotSerialStatus.qtyINIssues>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyINReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyInTransit>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyINAssemblySupply>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyINAssemblyDemand>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyInTransitToProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProductionSupplyPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProductionSupply>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedProductionPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedProductionOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProductionDemandPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProductionDemand>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProductionAllocated>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtySOFixedProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProdFixedPurchase>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProdFixedProduction>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProdFixedProdOrdersPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProdFixedProdOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProdFixedSalesOrdersPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyProdFixedSalesOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOReceipts>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLotSerialStatus.qtyFSSrvOrdPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyFSSrvOrdBooked>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyFSSrvOrdAllocated>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLotSerialStatus.qtySOPrepared>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<INLotSerialStatus.qtySOBooked>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtySOShipped>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtySOShipping>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtySOBackOrdered>(PXDbType.Decimal, 0m),

                            new PXDataFieldAssign<INLotSerialStatus.qtyFixedFSSrvOrd>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedFSSrvOrd>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedFSSrvOrdPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedFSSrvOrdReceipts>(PXDbType.Decimal, 0m),


                            new PXDataFieldAssign<INLotSerialStatus.qtySOFixed>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPOFixedReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtySODropShip>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPODropShipOrders>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPODropShipPrepared>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyPODropShipReceipts>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign<INLotSerialStatus.qtyInTransitToSO>(PXDbType.Decimal, 0m)

                        );

                    PXDatabase.Update<INItemLotSerial>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldAssign("QtyAvail", PXDbType.DirectExpression, "QtyOnHand"),
                            new PXDataFieldAssign("QtyHardAvail", PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign("QtyActual", PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INItemLotSerial.qtyOnReceipt>(PXDbType.Decimal, 0m),
                            new PXDataFieldAssign("QtyINTransit", PXDbType.Decimal, 0m)
                        );

					PXDatabase.Update<INSiteLotSerial>(
							new PXDataFieldRestrict<INSiteLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict<INSiteLotSerial.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldAssign<INSiteLotSerial.qtyAvail>(PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INSiteLotSerial.qtyHardAvail>(PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INSiteLotSerial.qtyActual>(PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign<INSiteLotSerial.qtyInTransit>(PXDbType.Decimal, 0m)
						);


                    foreach (PXResult<ReadOnlyLocationStatus, INLocation> res in PXSelectJoinGroupBy<ReadOnlyLocationStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<ReadOnlyLocationStatus.locationID>>>, Where<ReadOnlyLocationStatus.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, And<ReadOnlyLocationStatus.siteID, Equal<Current<INItemSiteSummary.siteID>>>>, Aggregate<GroupBy<ReadOnlyLocationStatus.inventoryID, GroupBy<ReadOnlyLocationStatus.siteID, GroupBy<ReadOnlyLocationStatus.subItemID, GroupBy<INLocation.inclQtyAvail, Sum<ReadOnlyLocationStatus.qtyOnHand>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
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

					foreach (PXResult<INItemPlan, InventoryItem, SOShipLineSplit, POReceiptLineSplit> res in PXSelectJoin<INItemPlan,
						InnerJoin<InventoryItem, On<INItemPlan.FK.InventoryItem>,
                        LeftJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>,
						LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.planID, Equal<INItemPlan.planID>>>>>,
						Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, 
							And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
							And<InventoryItem.stkItem, Equal<boolTrue>>>>>
						.SelectMultiBound(this, new object[] { itemsite }))
                    {
                        INItemPlan plan = (INItemPlan)res;
                        INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);
                        INPlanType locplantype;
                        SOShipLineSplit sosplit = (SOShipLineSplit)res;
                        POReceiptLineSplit posplit = (POReceiptLineSplit)res;

                        if (plan.InventoryID != null &&
                                plan.SubItemID != null &&
                                plan.SiteID != null)
                        {
                            if (!PlanTypeCalc(plan, sosplit, posplit, ref plantype, out locplantype))
                                continue;

                            if (plan.LocationID != null)
                            {
								LocationStatus item = UpdateAllocatedQuantities<LocationStatus>(plan, locplantype, true);
								UpdateAllocatedQuantities<SiteStatus>(plan, plantype, (bool)item.InclQtyAvail);
                                if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                                {
									UpdateAllocatedQuantities<LotSerialStatus>(plan, locplantype, true);
									UpdateAllocatedQuantities<SiteLotSerial>(plan, locplantype, true);
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
                    foreach (PXResult<INItemPlan, SOShipLineSplit, POReceiptLineSplit> res in PXSelectJoin<INItemPlan,
                                    LeftJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>,
                                    LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.planID, Equal<INItemPlan.planID>>>>,
                            Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, 
                                And<INItemPlan.lotSerialNbr, NotEqual<StringEmpty>, 
                                And<INItemPlan.lotSerialNbr, IsNotNull>>>>
                            .SelectMultiBound(this, new object[] { itemsite }))
                    {
                        INItemPlan plan = (INItemPlan)res;
                        INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);
                        INPlanType locplantype;
                        SOShipLineSplit sosplit = (SOShipLineSplit)res;
                        POReceiptLineSplit posplit = (POReceiptLineSplit)res;

                        if (plan.InventoryID != null &&
                         plan.SubItemID != null &&
                         plan.SiteID != null)
                        {
                            if (!PlanTypeCalc(plan, sosplit, posplit, ref plantype, out locplantype))
                                continue;

                            if (plan.LocationID != null)
                            {
								UpdateAllocatedQuantities<ItemLotSerial>(plan, locplantype, true);
                            }
                            else
                            {
								UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
                            }
                        }
                    }

                    if (replanBackorders)
	                {
		                INReleaseProcess.ReplanBackOrders(this);
						initemplan.Cache.Persist(PXDBOperation.Insert);
						initemplan.Cache.Persist(PXDBOperation.Update);
	                }

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

                    if (minPeriod != null)
                    {
                        MasterFinPeriod period =
                            PXSelect<MasterFinPeriod,
                                Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>
                                .SelectWindowed(this, 0, 1, minPeriod);
                        if (period == null) return;
                        DateTime startDate = (DateTime)period.StartDate;

                        PXDatabase.Delete<INItemCostHist>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("CostSiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
                            );

                        PXDatabase.Delete<INItemSalesHistD>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("QtyPlanSales", PXDbType.Decimal, 0m),
                            new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)

                            );
                        PXDatabase.Delete<INItemCustSalesStats>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("LastDate", PXDbType.DateTime, 8, startDate, PXComp.GE));

                        PXDatabase.Update<INItemSalesHistD>(
                            new PXDataFieldAssign("QtyIssues", PXDbType.Decimal, 0m),
                            new PXDataFieldAssign("QtyExcluded", PXDbType.Decimal, 0m),
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)
                            );

                        foreach (INLocation loc in PXSelectReadonly2<INLocation, InnerJoin<INItemCostHist, On<INItemCostHist.costSiteID, Equal<INLocation.locationID>>>, Where<INLocation.siteID, Equal<Current<INItemSiteSummary.siteID>>, And<INItemCostHist.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>>>>.SelectMultiBound(this, new object[] { itemsite }))
                        {
                            PXDatabase.Delete<INItemCostHist>(
                                new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                                new PXDataFieldRestrict("CostSiteID", PXDbType.Int, 4, loc.LocationID, PXComp.EQ),
                                new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
                                );
                        }

                        PXDatabase.Delete<INItemSiteHist>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
                            );

                        PXDatabase.Delete<INItemSiteHistD>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)
                            );

	                    PXDatabase.Delete<INItemSiteHistDay>(
		                    new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
		                    new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
		                    new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)
	                    );

						INTran prev_tran = null;
                        foreach (PXResult<INTran, INTranSplit> res in PXSelectReadonly2<INTran, 
							InnerJoin<INTranSplit, 
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

                            if (split.BaseQty != 0m)
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
                    }

					DeleteZeroStatusRecords(itemsite);

                    ts.Complete();
                }

                sitestatus.Cache.Persisted(false);
                locationstatus.Cache.Persisted(false);
                lotserialstatus.Cache.Persisted(false);

                itemcosthist.Cache.Persisted(false);
                itemsitehist.Cache.Persisted(false);
                itemsitehistd.Cache.Persisted(false);
				itemsitehistday.Cache.Persisted(false);
            }
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
				locationstatus.Cache.Fields
				.Where(f => locationstatus.Cache.GetAttributesReadonly(f).OfType<PXDBQuantityAttribute>().Any())
				.Select(f => new PXDataFieldRestrict(f, PXDbType.Decimal, decimal.Zero)));
			PXDatabase.Delete<INLocationStatus>(restrictions.ToArray());
		}

		public virtual void DeleteClosedPOPlans(INItemSiteSummary itemsite)
		{
			// for reducing the size of INItemPlan table
			// decided to delete closed and old PO plans - related to the years before the previous year
			if (this.Accessinfo.BusinessDate == null) return;
			DateTime prevYearStartDate = new DateTime(this.Accessinfo.BusinessDate.Value.Year, 1, 1).AddYears(-1);

			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				InnerJoin<PO.POOrder, On<PO.POOrder.noteID, Equal<INItemPlan.refNoteID>>>,
				Where<PO.POOrder.status, In3<PO.POOrderStatus.closed, PO.POOrderStatus.cancelled>,
					And<INItemPlan.planQty, Equal<decimal0>,
					And<INItemPlan.planType, In3<INPlanConstants.plan70, INPlanConstants.plan74, INPlanConstants.plan76>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INItemPlan.planDate, Less<Required<INItemPlan.planDate>>>>>>>>>
				.SelectMultiBound(this, new object[] { itemsite }, prevYearStartDate))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}
		}
    }
}
