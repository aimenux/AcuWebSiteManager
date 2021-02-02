using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.IN.PhysicalInventory;
using PX.Objects.SO;
using PX.Objects.AP;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Objects.PO.LandedCosts;

namespace PX.Objects.IN
{
    public class PXQtyCostImbalanceException : PXException
    {
	    public PXQtyCostImbalanceException()
		    : base()
	    {
    }

	    public PXQtyCostImbalanceException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
    }

    public class PXNegativeQtyImbalanceException : PXException
    {
        public PXNegativeQtyImbalanceException()
            : base()
        {
        }

        public PXNegativeQtyImbalanceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    public delegate List<PXResult<INItemPlan, INPlanType>> OnBeforeSalesOrderProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr);

    [PX.Objects.GL.TableAndChartDashboardType]
    public class INDocumentRelease : PXGraph<INDocumentRelease>
    {
        public PXCancel<INRegister> Cancel;
        public PXAction<INRegister> viewDocument;
        [PXFilterable]
        public PXProcessing<INRegister, Where<INRegister.released, Equal<boolFalse>, And<INRegister.hold, Equal<boolFalse>>>> INDocumentList;
        public PXSetup<INSetup> insetup;

        public INDocumentRelease()
        {
            INSetup record = insetup.Current;
            INDocumentList.SetProcessDelegate(
                delegate (List<INRegister> list)
                {
                    ReleaseDoc(list, true);
                }
            );
			INDocumentList.SetProcessCaption(Messages.Release);
			INDocumentList.SetProcessAllCaption(Messages.ReleaseAll);
        }

	    public static void ReleaseDoc(List<INRegister> list, bool isMassProcess)
			=> ReleaseDoc(list, isMassProcess, PXQuickProcess.ActionFlow.NoFlow);

        public static void ReleaseDoc(List<INRegister> list, bool isMassProcess, PXQuickProcess.ActionFlow processFlow)
        {
            bool failed = false;

            INReleaseProcess rg = PXGraph.CreateInstance<INReleaseProcess>();
            JournalEntry je = rg.CreateJournalEntry();
            var pg = new Lazy<PostGraph>(() => rg.CreatePostGraph());
            Dictionary<int, int> batchbind = new Dictionary<int, int>();
	        var releasedDocs = new List<INRegister>();

            for (int i = 0; i < list.Count; i++)
            {
                INRegister doc = list[i];
                try
                {
                    rg.Clear();

                    rg.ReleaseDocProcR(je, doc);
                    int k;
                    if ((k = je.created.IndexOf(je.BatchModule.Current)) >= 0 && batchbind.ContainsKey(k) == false)
                    {
                        batchbind.Add(k, i);
                    }

                    if (isMassProcess)
                    {
                        PXProcessing<INRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
                    }

	                releasedDocs.Add(doc);
                }
                catch (Exception e)
                {
					je.Clear();
                    if (isMassProcess)
                    {
                        PXProcessing<INRegister>.SetError(i, e);
                        failed = true;
					}
                    else if (list.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                    else
                    {
						PXTrace.WriteError(e);
                        failed = true;
                    }
                }
            }

            for (int i = 0; i < je.created.Count; i++)
            {
                Batch batch = je.created[i];
                try
                {
                    if (rg.AutoPost)
                    {
                        pg.Value.Clear();
                        pg.Value.PostBatchProc(batch);
                    }
                }
                catch (Exception e)
                {
					if (isMassProcess)
                    {
                        failed = true;
                        PXProcessing<INRegister>.SetError(batchbind[i], e);
                    }
                    else if (list.Count == 1)
                    {
                        throw new PXMassProcessException(batchbind[i], e);
                    }
                    else
                    {
						PXTrace.WriteError(e);
                        failed = true;
                    }
                }
            }

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
			}
			else if (releasedDocs.Count == 1 && processFlow != PXQuickProcess.ActionFlow.NoFlow)
			{
				RedirectTo(releasedDocs[0]);
			}
        }

		[PXUIField(DisplayName = "")]
        [PXEditDetailButton]
        protected virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            if (this.INDocumentList.Current != null)
            {
                INRegister r = PXCache<INRegister>.CreateCopy(this.INDocumentList.Current);
	            RedirectTo(r, PXBaseRedirectException.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

		public static void RedirectTo(INRegister doc, PXBaseRedirectException.WindowMode windowMode = PXBaseRedirectException.WindowMode.Same)
		{
			switch (doc.DocType)
			{
				case INDocType.Issue:
				{
					INIssueEntry graph = PXGraph.CreateInstance<INIssueEntry>();
					graph.issue.Current = graph.issue.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Issue") {Mode = windowMode};
				}
				case INDocType.Receipt:
				{
					INReceiptEntry graph = PXGraph.CreateInstance<INReceiptEntry>();
					graph.receipt.Current = graph.receipt.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Receipt") {Mode = windowMode};
				}
				case INDocType.Transfer:
				{
					INTransferEntry graph = PXGraph.CreateInstance<INTransferEntry>();
					graph.transfer.Current = graph.transfer.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Transfer") {Mode = windowMode};
				}
				case INDocType.Adjustment:
				{
					INAdjustmentEntry graph = PXGraph.CreateInstance<INAdjustmentEntry>();
					graph.adjustment.Current = graph.adjustment.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Adjustment") {Mode = windowMode};
				}
				case INDocType.Production:
				case INDocType.Disassembly:
				{
					KitAssemblyEntry graph = PXGraph.CreateInstance<KitAssemblyEntry>();
					graph.Document.Current = graph.Document.Search<INKitRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Kit Assembly") {Mode = windowMode};
				}
				default:
					throw new PXException(Messages.UnknownDocumentType);
			}
		}
	}

    public class PXAccumSelect<Table> : PXSelect<Table>
    where Table : class, IBqlTable, new()
    {
        public override Table Insert(Table item)
        {
            Table ret = base.Insert(item);
            if (ret == null)
            {
                return base.Locate(item);
            }
            return ret;
        }

        public PXAccumSelect(PXGraph graph)
            : base(graph)
        {
        }

        public PXAccumSelect(PXGraph graph, Delegate handler)
            : base(graph, handler)
        {
        }
    }

    public class PXNoEventsCache<TNode> : PXCache<TNode>
        where TNode : class, IBqlTable, new()
    {
        public PXNoEventsCache(PXGraph graph)
            : base(graph.Caches[typeof(TNode)].Graph)
        {
            _EventsRowAttr.RowSelecting = null;
            _EventsRowAttr.RowSelected = null;
            _EventsRowAttr.RowInserting = null;
            _EventsRowAttr.RowInserted = null;
            _EventsRowAttr.RowUpdating = null;
            _EventsRowAttr.RowUpdated = null;
            _EventsRowAttr.RowDeleting = null;
            _EventsRowAttr.RowDeleted = null;
            _EventsRowAttr.RowPersisting = null;
            _EventsRowAttr.RowPersisted = null;
        }
    }

    public class INReleaseProcess : PXGraph<INReleaseProcess>
    {
        public OnBeforeSalesOrderProcessPOReceipt onBeforeSalesOrderProcessPOReceipt;

        public PXSelect<INCostSubItemXRef> costsubitemxref;
        public PXSelect<INItemSite> initemsite;

        public PXSelect<OversoldCostStatus> oversoldcoststatus;
        public PXSelect<UnmanagedCostStatus> unmanagedcoststatus;
        public PXSelect<FIFOCostStatus> fifocoststatus;
        public PXSelect<AverageCostStatus> averagecoststatus;
        public PXSelect<StandardCostStatus> standardcoststatus;
        public PXSelect<SpecificCostStatus> specificcoststatus;
        public PXSelect<SpecificTransitCostStatus> specifictransitcoststatus;
        public PXSelect<ReceiptStatus> receiptstatus;
        public PXSelect<ItemLotSerial> itemlotserial;
        public PXSelect<SiteLotSerial> sitelotserial;
        public PXSelect<LotSerialStatus> lotnumberedstatus;
        public PXSelect<LocationStatus> locationstatus;
        public PXSelect<SiteStatus> sitestatus;

		#region InTransit Statuses
        public PXSelect<INTransitLine> intransitline;
		public PXSelect<TransitSiteStatus> transitsitestatus;
		public PXSelect<TransitLocationStatus> transitlocationstatus;
		public PXSelect<TransitLotSerialStatus> transitlotnumberedstatus;
		#endregion

        public PXSelect<ItemStats> itemstats;

        public PXSelect<ItemSiteHist> itemsitehist;
        public PXSelect<ItemSiteHistD> itemsitehistd;
        public PXSelect<ItemSiteHistDay> itemsitehistday;
		public PXSelect<ItemCostHist> itemcosthist;
        public PXSelect<ItemSalesHist> itemsaleshist;
        public PXSelect<ItemCustSalesHist> itemcustsaleshist;
        public PXSelect<ItemCustSalesStats> itemcustsalesstats;
        public PXSelect<ItemCustDropShipStats> itemcustdropshipstats;
        public PXSelect<ItemSalesHistD> itemsaleshistd;


        public PXSelect<INRegister> inregister;
        public PXSelect<INTran> intranselect;
        public PXSelect<INTranSplit> intransplit;
        public PXSelect<INItemPlan> initemplan;
        public PXAccumSelect<INTranCost> intrancost;

        public PXSelect<SOShipLineUpdate> soshiplineupdate;
        public PXSelect<ARTranUpdate> artranupdate;
        public PXSelect<POReceiptLineUpdate> poreceiptlineupdate;
        public PXSelect<INTranUpdate> intranupdate;
        public PXSelect<INTranCostUpdate> intrancostupdate;
        public PXSelect<INTranSplitAdjustmentUpdate> intransplitadjustmentupdate;
        public PXSelect<INTranSplitUpdate> intransplitupdate;
        public PXSelect<SOLineSplit> solinesplit;
        public PXSelect<SOOrder> soorder;
        public PXSelect<INItemLotSerial> initemlotserialreadonly;

        public PXSetup<INSetup> insetup;
        public PXSetup<Company> companysetup;
        public PXSelect<INItemPlan,
                Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>> supplyplans;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public string BaseCuryID
        {
            get
            {
                return companysetup.Current.BaseCuryID;
            }
        }

        public bool AutoPost
        {
            get
            {
                return (bool)insetup.Current.AutoPost;
            }
        }

        public bool UpdateGL
        {
            get
            {
                return (bool)insetup.Current.UpdateGL;
            }
        }

        public bool SummPost
        {
            get
            {
                return (bool)insetup.Current.SummPost;
            }
        }

        protected ReasonCode _ReceiptReasonCode;
        public ReasonCode ReceiptReasonCode
        {
            get
            {
                if (this._ReceiptReasonCode == null)
                {
                    _ReceiptReasonCode = ReasonCode.PK.Find(this, insetup.Current.ReceiptReasonCode);
                }
                return _ReceiptReasonCode;
            }
        }

        protected ReasonCode _IssuesReasonCode;
        public ReasonCode IssuesReasonCode
        {
            get
            {
                if (this._IssuesReasonCode == null)
                {
                    _IssuesReasonCode = ReasonCode.PK.Find(this, insetup.Current.IssuesReasonCode);
                }
                return _IssuesReasonCode;
            }
        }

        protected ReasonCode _AdjustmentReasonCode;
        public ReasonCode AdjustmentReasonCode
        {
            get
            {
                if (this._AdjustmentReasonCode == null)
                {
                    _AdjustmentReasonCode = ReasonCode.PK.Find(this, insetup.Current.AdjustmentReasonCode);
                }
                return _AdjustmentReasonCode;
            }
        }

        public Int32? ARClearingAcctID
        {
            get
            {
                return insetup.Current.ARClearingAcctID;
            }
        }

        public Int32? ARClearingSubID
        {
            get
            {
                return insetup.Current.ARClearingSubID;
            }
        }

        public Int32? INTransitSiteID
        {
            get
            {
                return insetup.Current.TransitSiteID;
            }
        }

        public Int32? INTransitAcctID
        {
            get
            {
                return insetup.Current.INTransitAcctID;
            }
        }

        public Int32? INTransitSubID
        {
            get
            {
                return insetup.Current.INTransitSubID;
            }
        }

        public Int32? INProgressAcctID
        {
            get
            {
                return insetup.Current.INProgressAcctID;
            }
        }

        public Int32? INProgressSubID
        {
            get
            {
                return insetup.Current.INProgressSubID;
            }
        }

        protected PXCache<INTranCost> transfercosts;

        public override void Clear()
        {
            base.Clear();
			Clear(PXClearOption.ClearQueriesOnly);

            if (transfercosts != null)
            {
                transfercosts.Clear();
            }
            WIPCalculated = false;
            WIPVariance = 0m;
        }

        public INReleaseProcess()
        {
            INSetup setup = insetup.Current;

            transfercosts = new PXNoEventsCache<INTranCost>(this);

            PXDBDefaultAttribute.SetDefaultForInsert<INTran.docType>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTran.refNbr>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTran.tranDate>(intranselect.Cache, null, false);

            PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.refNbr>(intransplit.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.tranDate>(intransplit.Cache, null, false);

            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.docType>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(intranselect.Cache, null, false);

            intranselect.Cache.Adjust<FinPeriodIDAttribute>()
                .For<INTran.finPeriodID>(attr =>
                {
                    attr.HeaderFindingMode = FinPeriodIDAttribute.HeaderFindingModes.Parent;
                });

            PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.refNbr>(intransplit.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.tranDate>(intransplit.Cache, null, false);

            OpenPeriodAttribute.SetValidatePeriod<INRegister.finPeriodID>(inregister.Cache, null, PeriodValidation.Nothing);

            ParseSubItemSegKeys();

            PXDimensionSelectorAttribute.SetSuppressViewCreation(intranselect.Cache);
            PXDimensionSelectorAttribute.SetSuppressViewCreation(intrancost.Cache);

            PXFormulaAttribute.SetAggregate<INTran.qty>(intranselect.Cache, null);
            PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, null);
        }

		public virtual JournalEntry CreateJournalEntry()
		{
			var je = PXGraph.CreateInstance<JournalEntry>();

			//Field Verification can fail if GL module is not "Visible";therfore suppress it:
			je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			//Uncomment if posting of empty transactions for NS items is really needed.
			//je.RowInserting.AddHandler<GLTran>((sender, e) => { ((GLTran)e.Row).ZeroPost = ((GLTran)e.Row).ZeroPost ?? ("NP".IndexOf(((GLTran)e.Row).TranClass) >= 0 && ((GLTran)e.Row).AccountID != null && ((GLTran)e.Row).SubID != null); });

			if (UpdateGL)
			{
				PXCache subCache = Caches[typeof(Sub)];
				Caches[typeof(Sub)] = je.Caches[typeof(Sub)];

				je.RowPersisting.AddHandler<Sub>((cache, e) => 
					subCache.RaiseRowPersisting(e.Row, e.Operation));

				je.RowPersisted.AddHandler<Sub>((cache, e) =>
					subCache.RaiseRowPersisted(e.Row, e.Operation, e.TranStatus, e.Exception));
			}

			return je;
		}

		public virtual PostGraph CreatePostGraph()
		{
			return PXGraph.CreateInstance<PostGraph>();
		}

		protected virtual PILocksInspector CreateLocksInspector(int siteID)
		{
			return new PILocksInspector(siteID);
		}

        protected virtual void StandardCostStatus_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            StandardCostStatus tran = e.Row as StandardCostStatus;

            if (tran != null)
            {
                INItemSite itemsite = SelectItemSite(sender.Graph, tran.InventoryID, tran.CostSiteID);

                if (itemsite != null)
                {
                    e.NewValue = itemsite.StdCost;
                    e.Cancel = true;
                }
            }
        }

        //all descendants of INCostStatus should have this handler
        long _CostStatus_Identity = long.MinValue;
        protected virtual void StandardCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void AverageCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void FIFOCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void SpecificCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void OversoldCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        [IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]
        public virtual void INTranSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(IsKey = true, IsUnicode = true)]
        [PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLineSplit.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLineSplit.orderNbr>>>>>))]
        [PXDefault()]
        protected virtual void SOLineSplit_OrderNbr_CacheAttached(PXCache sender)
        {
        }

        [PXDBDate()]
        [PXDefault()]
        protected virtual void SOLineSplit_OrderDate_CacheAttached(PXCache sender)
        {
        }

        [PXDBLong()]
		[INItemPlanIDSimple()]
        protected virtual void SOLineSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void SOLineSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void SOLineSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INTranSplitPlanID(typeof(INRegister.noteID), typeof(INRegister.hold), typeof(INRegister.transferType))]
		protected virtual void INTranSplit_PlanID_CacheAttached(PXCache sender)
		{
		}

        protected virtual void INTran_UnitCost_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && (((INTran)e.Row).InvtMult == (short)0 || ((INTran)e.Row).InvtMult == (short)1 && ((INTran)e.Row).OrigLineNbr == null && ((INTran)e.Row).TranType != INTranType.Assembly && ((INTran)e.Row).TranType != INTranType.Disassembly && ((INTran)e.Row).DocType != INDocType.Disassembly))
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INTran_UnitPrice_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INRegister_TotalQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INRegister_TotalAmount_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INRegister_TotalCost_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && ((INRegister)e.Row).DocType.IsNotIn(INDocType.Issue, INDocType.Adjustment))
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        public virtual void INItemSite_InvtAcctID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (((INItemSite)e.Row).OverrideInvtAcctSub != true)
                {
                    e.ExcludeFromInsertUpdate();
                }
            }
        }

        public virtual void INItemSite_InvtSubID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (((INItemSite)e.Row).OverrideInvtAcctSub != true)
                {
                    e.ExcludeFromInsertUpdate();
                }
            }
        }

        protected virtual void INRegister_TransferNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        public virtual void UpdateSiteStatus(INTran tran, INTranSplit split, INLocation whseloc)
        {
            SiteStatus item = new SiteStatus();
            item.InventoryID = split.InventoryID;
            item.SubItemID = split.SubItemID;
            item.SiteID = split.SiteID;

            item = sitestatus.Insert(item);

            item.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
            item.QtyAvail += whseloc.InclQtyAvail == true ? (decimal)split.InvtMult * (decimal)split.BaseQty : 0m;
            item.QtyHardAvail += whseloc.InclQtyAvail == true ? (decimal)split.InvtMult * (decimal)split.BaseQty : 0m;
			item.QtyActual += whseloc.InclQtyAvail == true ? (decimal)split.InvtMult * (decimal)split.BaseQty : 0m;
            item.QtyNotAvail += whseloc.InclQtyAvail == true ? 0m : (decimal)split.InvtMult * (decimal)split.BaseQty;
            item.SkipQtyValidation = split.SkipQtyValidation;

            if (tran.TranType == INTranType.Transfer && !IsOneStepTransfer())
            {
                TransitSiteStatus tranitem = new TransitSiteStatus();

                tranitem.InventoryID = split.InventoryID;
                tranitem.SubItemID = split.SubItemID;
                tranitem.SiteID = this.INTransitSiteID;

                tranitem = (TransitSiteStatus)transitsitestatus.Cache.Insert(tranitem);

                tranitem.QtyOnHand -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                tranitem.QtyAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                tranitem.QtyHardAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
				tranitem.QtyActual -= (decimal)split.InvtMult * (decimal)split.BaseQty;
            }
        }

        public virtual void UpdateLocationStatus(INTran tran, INTranSplit split)
        {
			if (split.InvtMult == 0)
				return;
			if (this.CreateLocksInspector(split.SiteID.Value)
				.IsInventoryLocationLocked(split.InventoryID, split.LocationID, inregister.Current.PIID))
            {
                PXCache cache = this.Caches[typeof(INTranSplit)];
                throw new PXException(Messages.PICountInProgressDuringRelease,
                                      PXForeignSelectorAttribute.GetValueExt<INTranSplit.inventoryID>(cache, split),
                                      PXForeignSelectorAttribute.GetValueExt<INTranSplit.siteID>(cache, split),
                                      PXForeignSelectorAttribute.GetValueExt<INTranSplit.locationID>(cache, split));
            }

            LocationStatus item = new LocationStatus();
            item.InventoryID = split.InventoryID;
            item.SubItemID = split.SubItemID;
            item.SiteID = split.SiteID;
            item.LocationID = split.LocationID;
			item.RelatedPIID = inregister.Current.PIID;

            item = locationstatus.Insert(item);

            item.NegQty = (split.TranType == INTranType.Adjustment) ? false : item.NegQty;
            item.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
            item.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
            item.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
			item.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;
            item.SkipQtyValidation = split.SkipQtyValidation;

            if (tran.TranType == INTranType.Transfer && !IsOneStepTransfer())
            {
                TransitLocationStatus tranitem = new TransitLocationStatus();
                tranitem.InventoryID = split.InventoryID;
                tranitem.SubItemID = split.SubItemID;
                tranitem.SiteID = this.INTransitSiteID;

                if (IsIngoingTransfer(tran))
                {
                    var trl = new INTransitLine();
                    trl.TransferNbr = tran.OrigRefNbr;
                    trl.TransferLineNbr = tran.OrigLineNbr;
                    trl = intransitline.Locate(trl);
                    
                    tranitem.LocationID = trl.CostSiteID;
                }
                else
                {
                    INTransitLine ntl = GetTransitLine(tran);

                    tranitem.LocationID = ntl.CostSiteID;
                }

                tranitem = (TransitLocationStatus)transitlocationstatus.Cache.Insert(tranitem);

                tranitem.QtyOnHand -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                tranitem.QtyAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                tranitem.QtyHardAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
				tranitem.QtyActual -= (decimal)split.InvtMult * (decimal)split.BaseQty;
            }

        }

        public virtual INTransitLine GetTransitLine(INTran tran)
        {
            var ntl = new INTransitLine();
            ntl.TransferNbr = tran.RefNbr;
            ntl.TransferLineNbr = tran.LineNbr;
            var ntlloc = intransitline.Locate(ntl);
            if (ntlloc == null)
            {
                ntl.SOOrderType = tran.SOOrderType;
                ntl.SOOrderNbr = tran.SOOrderNbr;
                ntl.SOOrderLineNbr = tran.SOOrderLineNbr;
                ntl.SOShipmentType = tran.SOShipmentType;
                ntl.SOShipmentNbr = tran.SOShipmentNbr;
                ntl.SOShipmentLineNbr = tran.SOShipmentLineNbr;
                ntl.OrigModule = tran.SOOrderNbr == null ? GL.BatchModule.IN : GL.BatchModule.SO;
                ntl.SiteID = tran.SiteID;
                ntl.ToSiteID = tran.ToSiteID;
                ntl.RefNoteID = this.inregister.Current.NoteID;

                INTranSplit split = PXSelectReadonly<INTranSplit,
		            Where<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>,
			        And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>,
				    And<INTranSplit.docType, Equal<Current<INTran.docType>>>>>>.SelectSingleBound(this, new object[] { tran });

				INSite tosite = INSite.PK.Find(this, tran.ToSiteID);
				INItemSite itemsite = INItemSite.PK.Find(this, tran.InventoryID, tosite?.SiteID);

				ntl.IsLotSerial = !String.IsNullOrEmpty(split?.LotSerialNbr);
                ntl.ToLocationID = tran.ToLocationID ?? itemsite?.DfltReceiptLocationID ?? tosite.ReceiptLocationID;

                //generate noteid for plan linking
                ntl.NoteID = PXNoteAttribute.GetNoteID<INTransitLine.noteID>(intransitline.Cache, ntl).Value;
                foreach (Note note in this.Caches[typeof(Note)].Inserted)
                {
                    if (note.NoteID == ntl.NoteID)
                    {
                        note.GraphType = typeof(INTransferEntry).FullName;
                        break;
                    }
                }
                ntl = intransitline.Insert(ntl);
            }
            else
                ntl = ntlloc;
            return ntl;
        }

        public virtual LotSerialStatus AccumulatedLotSerialStatus(INTranSplit split, INLotSerClass lsclass)
        {
            LotSerialStatus ret = new LotSerialStatus();
            ret.InventoryID = split.InventoryID;
            ret.SubItemID = split.SubItemID;
            ret.SiteID = split.SiteID;
            ret.LocationID = split.LocationID;
            ret.LotSerialNbr = split.LotSerialNbr;
            ret = (LotSerialStatus)lotnumberedstatus.Cache.Insert(ret);
            if (ret.ExpireDate == null)
            {
                ret.ExpireDate = split.ExpireDate;
            }
            if (ret.ReceiptDate == null)
            {
                ret.ReceiptDate = split.TranDate;
            }
            ret.LotSerTrack = lsclass.LotSerTrack;

            return ret;
        }

        public virtual TransitLotSerialStatus AccumulatedTransitLotSerialStatus(INTranSplit split, INLotSerClass lsclass, INTransitLine tl)
        {
            TransitLotSerialStatus ret = new TransitLotSerialStatus();

            ret.InventoryID = split.InventoryID;
            ret.SubItemID = split.SubItemID;
            ret.SiteID = this.INTransitSiteID;
            ret.LocationID = tl.CostSiteID;
            ret.LotSerialNbr = split.LotSerialNbr;
            ret = (TransitLotSerialStatus)transitlotnumberedstatus.Cache.Insert(ret);
            if (ret.ExpireDate == null)
            {
                ret.ExpireDate = split.ExpireDate;
            }
            if (ret.ReceiptDate == null)
            {
                ret.ReceiptDate = split.TranDate;
            }
            ret.LotSerTrack = lsclass.LotSerTrack;

            return ret;
        }


        public virtual void ReceiveLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            LotSerialStatus lsitem;
            if (split.InvtMult == (short)1 && !IsOneStepTransfer())
            {
                if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
					(lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                )
                {
                    lsitem = AccumulatedLotSerialStatus(split, lsclass);

                    if (item.ValMethod == "S")
                    {
                        //for transit site we can identify cost layer by transfernbr and transferlinenbr
                        //for specific Items cost layer can be accurately identified by LotSerialNbr, Account/Sub is not taken into consideration
                        INCostStatus layer = AccumulatedCostStatus(tran, split, item);
                        lsitem.CostID = layer.CostID;
                    }

                    lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

                    return;
                }
            }
        }

        public virtual void IssueLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            LotSerialStatus lsitem;
            if (split.InvtMult == -1)
            {
                //for when used serial numbers numbers will mark processed numbers with trandate
				if (INLotSerialNbrAttribute.IsTrackSerial(lsclass, tran.TranType, tran.InvtMult) || 
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    lsitem = AccumulatedLotSerialStatus(split, lsclass);

                    if (lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                    {

                        if (item.ValMethod == "S")
                        {
                            //for transit site we can identify cost layer by transfernbr and transferlinenbr
                            //for specific Items cost layer can be accurately identified by LotSerialNbr, Account/Sub is not taken into consideration
                            INCostStatus layer = AccumulatedCostStatus(tran, split, item);
                            lsitem.CostID = layer.CostID;
                        }

                        lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                        lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                        lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
						lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    }

                    return;
                }
            }
        }

        public virtual void TransferLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            LotSerialStatus lsitem;
            if (IsOneStepTransfer() && split.InvtMult == (short)1 && tran.OrigLineNbr != null)
            {
				if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    lsitem = AccumulatedLotSerialStatus(split, lsclass);

                    PXResult res = PXSelectJoin<INTranSplit, 
						InnerJoin<ReadOnlyLotSerialStatus, 
							On<ReadOnlyLotSerialStatus.inventoryID, Equal<INTranSplit.inventoryID>, 
							And<ReadOnlyLotSerialStatus.siteID, Equal<INTranSplit.siteID>, 
							And<ReadOnlyLotSerialStatus.locationID, Equal<INTranSplit.locationID>, 
							And<ReadOnlyLotSerialStatus.lotSerialNbr, Equal<INTranSplit.lotSerialNbr>>>>>>, 
						Where<INTranSplit.docType, Equal<Current<INTran.origDocType>>, 
						And<INTranSplit.refNbr, Equal<Current<INTran.origRefNbr>>, 
						And<INTranSplit.lineNbr, Equal<Current<INTran.origLineNbr>>, 
						And<INTranSplit.lotSerialNbr, Equal<Current<INTranSplit.lotSerialNbr>>>>>>>.SelectSingleBound(this, new object[] { tran, split });

                    if (res != null)
                    {
                        lsitem.ReceiptDate = ((ReadOnlyLotSerialStatus)res[typeof(ReadOnlyLotSerialStatus)]).ReceiptDate;
                    }

                    if (item.ValMethod == "S")
                    {
                        //for specific Items cost layer can be accurately identified by LotSerialNbr, Account/Sub is not taken into consideration
                        INCostStatus layer = AccumulatedCostStatus(tran, split, item);
                        lsitem.CostID = layer.CostID;
                    }

                    lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

                    return;
                }
            }

            TransitLotSerialStatus lstritem;
            if (tran.TranType == INTranType.Transfer && !IsOneStepTransfer())
            {
                if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    if (IsIngoingTransfer(tran))
                    {
                        var trl = new INTransitLine();
                        trl.TransferNbr = tran.OrigRefNbr;
                        trl.TransferLineNbr = tran.OrigLineNbr;
                        trl = intransitline.Locate(trl);

                        lstritem = AccumulatedTransitLotSerialStatus(split, lsclass, trl);

                        ReadOnlyLotSerialStatus res = PXSelect<ReadOnlyLotSerialStatus, Where<ReadOnlyLotSerialStatus.inventoryID, Equal<Current<INTranSplit.inventoryID>>, And<ReadOnlyLotSerialStatus.siteID, Equal<SiteAttribute.transitSiteID>, And<ReadOnlyLotSerialStatus.locationID, Equal<Current<INTransitLine.costSiteID>>, And<ReadOnlyLotSerialStatus.lotSerialNbr, Equal<Current<INTranSplit.lotSerialNbr>>>>>>>.SelectSingleBound(this, new object[] { split, trl });

                        if (res != null && res.ReceiptDate != null)
                        {
                            LotSerialStatus complementlsitem = AccumulatedLotSerialStatus(split, lsclass);
                            complementlsitem.ReceiptDate = res.ReceiptDate;
                        }
                    }
                    else
                    {
                        INTransitLine ntl = GetTransitLine(tran);

                        lstritem = AccumulatedTransitLotSerialStatus(split, lsclass, ntl);

                        ReadOnlyLotSerialStatus res = PXSelect<ReadOnlyLotSerialStatus, Where<ReadOnlyLotSerialStatus.inventoryID, Equal<Current<INTranSplit.inventoryID>>, And<ReadOnlyLotSerialStatus.siteID, Equal<Current<INTranSplit.siteID>>, And<ReadOnlyLotSerialStatus.locationID, Equal<Current<INTranSplit.locationID>>, And<ReadOnlyLotSerialStatus.lotSerialNbr, Equal<Current<INTranSplit.lotSerialNbr>>>>>>>.SelectSingleBound(this, new object[] { split });

                        if (res != null && res.ReceiptDate != null)
                            lstritem.ReceiptDate = res.ReceiptDate;
                    }

                    lstritem.QtyOnHand -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lstritem.QtyAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lstritem.QtyHardAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
					lstritem.QtyActual -= (decimal)split.InvtMult * (decimal)split.BaseQty;
                }
            }
        }

        public virtual INItemLotSerial UpdateItemLotSerial(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
			if (IsOneStepTransferWithinSite()) return null;

            if (split.InvtMult == 1 &&
				!string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				(lsclass.LotSerAssign == INLotSerAssign.WhenReceived
				|| lsclass.LotSerAssign == INLotSerAssign.WhenUsed && split.TranType.IsIn(INTranType.CreditMemo, INTranType.Return)))
            {
                ItemLotSerial lsitem = AccumulatedItemLotSerial(split, lsclass);

                if (tran.OrigLineNbr == null && lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = true;

                if (split.TranType == INTranType.Adjustment && split.InvtMult == 1 && split.ExpireDate != null)
                {
                    lsitem.UpdateExpireDate = true;
                    lsitem.ExpireDate = split.ExpireDate;
                }

                lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

                return lsitem;
            }

            if (split.InvtMult == -1 &&
                    split.BaseQty != 0m &&
                    !string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered)
            {
                ItemLotSerial lsitem = AccumulatedItemLotSerial(split, lsclass);

                if (lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = lsclass.LotSerAssign == INLotSerAssign.WhenUsed;

                if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered ||
                        lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;
                }
                return lsitem;
            }

            return null;
        }

        public virtual INSiteLotSerial UpdateSiteLotSerial(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
			if (IsOneStepTransferWithinSite()) return null;

            if (split.InvtMult == 1 &&
					!string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
					(lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                )
            {
                SiteLotSerial lsitem = AccumulatedSiteLotSerial(split, lsclass);

                if (tran.OrigLineNbr == null && lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = true;

                if (split.TranType == INTranType.Adjustment && split.InvtMult == 1 && split.ExpireDate != null)
                {
                    lsitem.UpdateExpireDate = true;
                    lsitem.ExpireDate = split.ExpireDate;
                }

                lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

                return lsitem;
            }

            if (split.InvtMult == -1 &&
                    split.BaseQty != 0m &&
                    !string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered)
            {
                SiteLotSerial lsitem = AccumulatedSiteLotSerial(split, lsclass);

                if (lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = false;

                if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered ||
                        lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;
                }
                return lsitem;
            }

            return null;
        }

        public virtual void UpdateLotSerialStatus(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            ReceiveLot(tran, split, item, lsclass);
            IssueLot(tran, split, item, lsclass);
            TransferLot(tran, split, item, lsclass);
        }

        public virtual ItemLotSerial AccumulatedItemLotSerial(INTranSplit split, INLotSerClass lsclass)
        {
            ItemLotSerial lsitem = new ItemLotSerial();
            lsitem.InventoryID = split.InventoryID;
            lsitem.LotSerialNbr = split.LotSerialNbr;
            lsitem = (ItemLotSerial)itemlotserial.Insert(lsitem);
            if (lsitem.ExpireDate == null)
            {
                lsitem.ExpireDate = split.ExpireDate;
            }
            lsitem.LotSerTrack = lsclass.LotSerTrack;

            return lsitem;
        }

        public virtual SiteLotSerial AccumulatedSiteLotSerial(INTranSplit split, INLotSerClass lsclass)
        {
            SiteLotSerial lsitem = new SiteLotSerial();
            lsitem.InventoryID = split.InventoryID;
            lsitem.LotSerialNbr = split.LotSerialNbr;
            lsitem.SiteID = split.SiteID;
            lsitem = (SiteLotSerial)sitelotserial.Insert(lsitem);
            if (lsitem.ExpireDate == null)
            {
                lsitem.ExpireDate = split.ExpireDate;
            }
            lsitem.LotSerTrack = lsclass.LotSerTrack;

            return lsitem;
        }

        public virtual INCostStatus AccumulatedCostStatus(INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus layer = null;

            bool isTransit = split.CostSiteID == INTransitSiteID;

            switch (item.ValMethod)
            {
                case INValMethod.Standard:

                    if (tran.TranType == INTranType.NegativeCostAdjustment)
                    {
                        return AccumOversoldCostStatus(tran, split, item);
                    }
                    else
                    {
                        layer = new StandardCostStatus();
                        layer.AccountID = tran.InvtAcctID;
                        layer.SubID = tran.InvtSubID;
                        layer.InventoryID = tran.InventoryID;
                        layer.CostSiteID = split.CostSiteID;
                        layer.SiteID = split.SiteID;
                        layer.CostSubItemID = split.CostSubItemID;
                        layer.ReceiptNbr = INLayerRef.ZZZ;
                        layer.LayerType = INLayerType.Normal;

                        return (StandardCostStatus)standardcoststatus.Cache.Insert(layer);
                    }
                case INValMethod.Average:
                    layer = new AverageCostStatus();
                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.ReceiptNbr = INLayerRef.ZZZ;
                    layer.LayerType = INLayerType.Normal;

                    return (AverageCostStatus)averagecoststatus.Cache.Insert(layer);
                case INValMethod.FIFO:
                    layer = new FIFOCostStatus();
                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.ReceiptDate = tran.TranDate;
                    layer.ReceiptNbr = tran.OrigRefNbr ?? tran.RefNbr;
                    layer.LayerType = INLayerType.Normal;

                    return (FIFOCostStatus)fifocoststatus.Cache.Insert(layer);
                case INValMethod.Specific:
                    if (!isTransit)
                        layer = new SpecificCostStatus();
                    else
                        layer = new SpecificTransitCostStatus();

                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.LotSerialNbr = split.LotSerialNbr;
                    layer.ReceiptDate = tran.TranDate;
                    layer.ReceiptNbr = tran.RefNbr;
                    layer.LayerType = INLayerType.Normal;

                    if (tran.InvtMult == 0 && (tran.TranType == INTranType.Invoice || tran.TranType == INTranType.DebitMemo || tran.TranType == INTranType.CreditMemo))
                    {
                        layer.LotSerialNbr = string.Empty;
                    }

                    if (!isTransit)
                        return (SpecificCostStatus)specificcoststatus.Cache.Insert(layer);
                    else
                        return (SpecificTransitCostStatus)specifictransitcoststatus.Cache.Insert(layer);

                default:
                    throw new PXException();
            }
        }

        public virtual INCostStatus AccumOversoldCostStatus(INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus layer = null;

            if (item.NegQty == false && tran.TranType != INTranType.NegativeCostAdjustment)
            {
				INSite warehouse = INSite.PK.Find(this, tran.SiteID);
				INLocation location = INLocation.PK.Find(this, tran.LocationID);

				string siteCD = "";
                string locationCD = "";

                if (warehouse != null)
                    siteCD = warehouse.SiteCD;

                if (location != null)
                    locationCD = location.LocationCD;

                throw new PXException(Messages.Inventory_Negative, item.InventoryCD, siteCD, locationCD);
            }

            switch (item.ValMethod)
            {
                case INValMethod.Standard:
                case INValMethod.Average:
                case INValMethod.FIFO:
                    layer = new OversoldCostStatus();
                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.ReceiptDate = new DateTime(1900, 1, 1);
                    layer.ReceiptNbr = "OVERSOLD";
                    layer.LayerType = INLayerType.Oversold;
                    layer.ValMethod = item.ValMethod;

                    return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(layer);
                case INValMethod.Specific:
                    throw new PXException(Messages.Inventory_Negative);
                default:
                    throw new PXException();
            }
        }

        public virtual INCostStatus AccumUnmanagedCostStatus(INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus layer = null;

            layer = new UnmanagedCostStatus();
            layer.AccountID = tran.InvtAcctID;
            layer.SubID = tran.InvtSubID;
            layer.InventoryID = tran.InventoryID;
            layer.CostSiteID = split.CostSiteID;
	        layer.SiteID = split.SiteID;
            layer.CostSubItemID = split.CostSubItemID;
            layer.LayerType = INLayerType.Oversold;
            layer.ValMethod = item.ValMethod;

            return (UnmanagedCostStatus)unmanagedcoststatus.Cache.Insert(layer);
        }

        public virtual INCostStatus AccumOversoldCostStatus(INCostStatus layer)
        {
            INCostStatus ret = new OversoldCostStatus();

            PXCache<INCostStatus>.RestoreCopy(ret, layer);
            ret.QtyOnHand = 0m;
            ret.TotalCost = 0m;

            return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(ret);
        }

        public virtual PXView GetReceiptStatusView(InventoryItem item)
        {
            List<Type> bql = new List<Type>()
                {
                    typeof(Select2<,,>),
                    typeof(ReadOnlyCostStatus),
                    typeof(LeftJoin<ReceiptStatus, On<ReceiptStatus.inventoryID, Equal<ReadOnlyCostStatus.inventoryID>,
                                And<ReceiptStatus.costSubItemID, Equal<ReadOnlyCostStatus.costSubItemID>,
                                And<ReceiptStatus.costSiteID, Equal<ReadOnlyCostStatus.costSiteID>,
                                And<ReceiptStatus.accountID, Equal<ReadOnlyCostStatus.accountID>,
                                And<ReceiptStatus.subID, Equal<ReadOnlyCostStatus.subID>>>>>>>),
                    typeof(Where2<,>),
                    typeof(Where<ReadOnlyCostStatus.inventoryID, Equal<Current<INTranSplit.inventoryID>>,
                        And<ReadOnlyCostStatus.costSubItemID, Equal<Current<INTranSplit.costSubItemID>>,
                        And<ReadOnlyCostStatus.costSiteID, Equal<Current<INTranSplit.costSiteID>>,
                        And<ReadOnlyCostStatus.layerType, Equal<INLayerType.normal>,
                        And<ReadOnlyCostStatus.accountID, Equal<Required<INTran.invtAcctID>>,
                        And<ReadOnlyCostStatus.subID, Equal<Required<INTran.invtSubID>>>>>>>>)
                };

            switch (item.ValMethod)
            {
                case INValMethod.Standard:
                    bql.Add(typeof(And<Where<True, Equal<False>>>));
                    break;
                case INValMethod.FIFO:
                    bql.Add(typeof(And<Where<ReadOnlyCostStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>>>));
                    break;
                case INValMethod.Specific:
                    bql.Add(typeof(And<Where<ReadOnlyCostStatus.lotSerialNbr, Equal<ReceiptStatus.lotSerialNbr>, And<ReceiptStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>,
                        And<ReadOnlyCostStatus.lotSerialNbr, Equal<Current<INTran.lotSerialNbr>>>>>>));
                    break;
                case INValMethod.Average:
                    bql.Add(typeof(And<Where<ReadOnlyCostStatus.receiptNbr, Equal<INLayerRef.zzz>, And<ReceiptStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>>>>));
                    break;
            }
            return this.TypedViews.GetView(BqlCommand.CreateInstance(bql.ToArray()), false);
        }

        public virtual PXView GetReceiptStatusByKeysView(INCostStatus layer)
        {
            List<Type> bql = new List<Type>()
            {
                    typeof(Select<,>),
                    typeof(ReadOnlyReceiptStatus),
                    typeof(Where<,,>),
                    typeof(ReadOnlyReceiptStatus.inventoryID),
                    typeof(Equal<Required<INCostStatus.inventoryID>>),
                    typeof(And<,,>),
                    typeof(ReadOnlyReceiptStatus.costSiteID),
                    typeof(Equal<Required<INCostStatus.costSiteID>>),
                    typeof(And<,,>),
                    typeof(ReadOnlyReceiptStatus.costSubItemID),
                    typeof(Equal<Required<INCostStatus.costSubItemID>>),
                    typeof(And<,,>),
                    typeof(ReadOnlyReceiptStatus.accountID),
                    typeof(Equal<Required<INCostStatus.accountID>>)
            };


            if (layer.ValMethod == INValMethod.Specific)
            {
                bql.Add(typeof(And<,,>));
                bql.Add(typeof(ReadOnlyReceiptStatus.subID));
                bql.Add(typeof(Equal<Required<INCostStatus.subID>>));
                bql.Add(typeof(And<,>));
                bql.Add(typeof(ReadOnlyReceiptStatus.lotSerialNbr));
                bql.Add(typeof(Equal<Required<INCostStatus.lotSerialNbr>>));
            }
            else
            {
                bql.Add(typeof(And<,>));
                bql.Add(typeof(ReadOnlyReceiptStatus.subID));
                bql.Add(typeof(Equal<Required<INCostStatus.subID>>));
            }

            return this.TypedViews.GetView(BqlCommand.CreateInstance(bql.ToArray()), false);
        }

        public virtual PXView GetCostStatusCommand(INTran tran, INTranSplit split, InventoryItem item, out object[] parameters, bool correctImbalance, string fifoLayerNbr)
        {
            BqlCommand cmd = null;

            int? costsiteid;
            if (IsIngoingTransfer(tran))
            {
                costsiteid = INTransitSiteID;
            }
            else
            {
                costsiteid = split.CostSiteID;
            }

            if (correctImbalance || IsIngoingTransfer(tran))
            {
                fifoLayerNbr = tran.OrigRefNbr ?? string.Empty;
            }

            switch (item.ValMethod)
            {
                case INValMethod.Average:
                case INValMethod.Standard:
                case INValMethod.FIFO:

                    cmd = new Select<ReadOnlyCostStatus,
                        Where<ReadOnlyCostStatus.inventoryID, Equal<Required<ReadOnlyCostStatus.inventoryID>>,
                        And<ReadOnlyCostStatus.costSiteID, Equal<Required<ReadOnlyCostStatus.costSiteID>>,
                        And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>,
                        And<INCostStatus.layerType, Equal<INLayerType.normal>>>>>,
                        OrderBy<Asc<ReadOnlyCostStatus.receiptDate, Asc<ReadOnlyCostStatus.receiptNbr>>>>();
                    if (item.ValMethod == INValMethod.FIFO && fifoLayerNbr != null || IsIngoingTransfer(tran))
                    {
                        cmd = cmd.WhereAnd<Where<ReadOnlyCostStatus.receiptNbr, Equal<Required<ReadOnlyCostStatus.receiptNbr>>>>();
                    }
                    parameters = new object[] { split.InventoryID, costsiteid, split.CostSubItemID, fifoLayerNbr };
                    break;
                case INValMethod.Specific:
                    cmd = new Select<ReadOnlyCostStatus,
                        Where<ReadOnlyCostStatus.inventoryID, Equal<Required<ReadOnlyCostStatus.inventoryID>>,
                        And<ReadOnlyCostStatus.costSiteID, Equal<Required<ReadOnlyCostStatus.costSiteID>>,
                        And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>,
                        And<ReadOnlyCostStatus.lotSerialNbr, Equal<Required<ReadOnlyCostStatus.lotSerialNbr>>,
                        And<INCostStatus.layerType, Equal<INLayerType.normal>>>>>>>();
                    if (IsIngoingTransfer(tran))
                    {
                        cmd = cmd.WhereAnd<Where<ReadOnlyCostStatus.receiptNbr, Equal<Required<ReadOnlyCostStatus.receiptNbr>>>>();
                    }
                    parameters = new object[] { split.InventoryID, costsiteid, split.CostSubItemID, split.LotSerialNbr, tran.OrigRefNbr ?? string.Empty };
                    break;
                default:
                    throw new PXException();
            }
			return TypedViews.GetView(cmd, false);
        }

		/// <summary>
		/// Corrects the account in the cost layer (switches to another cost layer) if the target cost layer does not exist or empty
		/// </summary>
		public virtual void CorrectLayerAccountSub(INTranSplit split, InventoryItem item, INCostStatus layer, List<object> costStatuses, INTranCost costtran, bool updateAccountSub)
		{
			bool negQtyAdj = (split.InvtMult == (short)-1);
			bool costAdj = (split.TranType == INTranType.Adjustment && split.BaseQty == 0m);
			if (!negQtyAdj && !costAdj || !item.ValMethod.IsIn(INValMethod.Average, INValMethod.FIFO, INValMethod.Specific))
			{
				return;
			}

			var notEmptyCostStatuses = costStatuses.Cast<INCostStatus>()
				.Where(s => s.LayerType == INLayerType.Normal && s.QtyOnHand > 0m);

			var exactCostStatus = notEmptyCostStatuses
				.FirstOrDefault(s => s.AccountID == layer.AccountID && s.SubID == layer.SubID);
			if (exactCostStatus != null)
			{
				return;
			}

			var properCostStatus = notEmptyCostStatuses
				.OrderByDescending(s => negQtyAdj ? s.QtyOnHand : s.TotalCost)
				.FirstOrDefault();

			if (properCostStatus != null)
			{
				layer.AccountID = properCostStatus.AccountID;
				layer.SubID = properCostStatus.SubID;
				costtran.InvtAcctID = properCostStatus.AccountID;
				costtran.InvtSubID = properCostStatus.SubID;
			}
		}

        public virtual bool IsUnmanagedTran(INTran tran)
        {
            return tran.InvtMult == (short)0
                && (tran.IsCostUnmanaged ?? false);
        }


        public virtual void ReceiveCost(INTran tran, INTranSplit split, InventoryItem item, bool correctImbalance)
        {
            if (tran.InvtMult == (short)1 && tran.TranType != INTranType.Transfer
					&& (split.InvtMult == (short)1 || item.ValMethod != INValMethod.Standard && !correctImbalance || item.ValMethod == INValMethod.Standard && correctImbalance)
				|| (tran.InvtMult == (short)0 && !IsUnmanagedTran(tran))
				|| (tran.ExactCost == true && !correctImbalance))
            {
                //!!!SPECIFIC, add Account Sub population from existing layer.
                INCostStatus layer = AccumulatedCostStatus(tran, split, item);

                INTranCost costtran = new INTranCost();
                costtran.InvtAcctID = layer.AccountID;
                costtran.InvtSubID = layer.SubID;
                costtran.COGSAcctID = tran.COGSAcctID;
                costtran.COGSSubID = tran.COGSSubID;
                costtran.CostID = layer.CostID;
                costtran.InventoryID = layer.InventoryID;
                costtran.CostSiteID = layer.CostSiteID;
                costtran.CostSubItemID = layer.CostSubItemID;
                costtran.DocType = tran.DocType;
                costtran.TranType = tran.TranType;
                costtran.RefNbr = tran.RefNbr;
                costtran.LineNbr = tran.LineNbr;
                costtran.CostDocType = tran.DocType;
                costtran.CostRefNbr = tran.RefNbr;

                //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
                costtran.InvtMult = split.InvtMult;

                costtran.FinPeriodID = tran.FinPeriodID;
                costtran.TranPeriodID = tran.TranPeriodID;
                costtran.TranDate = tran.TranDate;
                costtran.TranAmt = 0m;

                PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
                INTranCost prev_tran = intrancost.Insert(costtran);
                costtran = PXCache<INTranCost>.CreateCopy(prev_tran);

                costtran.Qty += INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);

                //cost only adjustment
                if (split.BaseQty == 0m && tran.BaseQty == 0m)
                {
                    costtran.TranCost += tran.TranCost;
                }
                else if (item.ValMethod == INValMethod.Standard)
                {
                    //do not add cost, recalculate
                    costtran.TranCost = PXCurrencyAttribute.BaseRound(this, (decimal)layer.UnitCost * (decimal)costtran.Qty);
                }
                else
                {
                    costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)tran.UnitCost * (decimal)split.BaseQty);
                }

                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)split.BaseQty * (decimal)tran.UnitPrice);

                INCostStatus unmodifiedLayer = PXCache<INCostStatus>.CreateCopy(layer);

                layer.QtyOnHand += (decimal)costtran.InvtMult * (costtran.Qty - prev_tran.Qty);
                layer.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - prev_tran.TranCost);

                object[] parameters;
                List<object> costStatuses;
                PXView cmd = null;

                if (tran.TranType == INTranType.Receipt)
                {
                    costStatuses = new List<object>();
                }
                else
                {
                    cmd = GetCostStatusCommand(tran, split, item, out parameters, false, layer.ReceiptNbr);
                    costStatuses = cmd.SelectMulti(parameters);
                }

                //such empty layer could cause not intended switch of strategy (to issue), must be refactored
                CorrectLayerAccountSub(split, item, layer, costStatuses, costtran, false);

				//verify imbalance and accumulate changes for possible strategy change (receipt->issue)
                INCostStatus exactCostStatus = costStatuses.Cast<INCostStatus>().FirstOrDefault(s => s.AccountID == layer.AccountID && s.SubID == layer.SubID);
                Action<INCostStatus> restoreData =
                    backup =>
                {
                        PXCache<INCostStatus>.RestoreCopy(layer, unmodifiedLayer);
                        if (exactCostStatus != null && backup != null)
                            PXCache<INCostStatus>.RestoreCopy(exactCostStatus, backup);
                        intrancost.Delete(costtran);
                    };

                if (exactCostStatus != null)
                {
	                var backup = PXCache<INCostStatus>.CreateCopy(exactCostStatus);

                    exactCostStatus.QtyOnHand += (decimal)costtran.InvtMult * (costtran.Qty - prev_tran.Qty);
                    exactCostStatus.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - prev_tran.TranCost);
                    cmd.Cache.Hold(exactCostStatus);
                        //throw exception if not cost only adjustment and quantity to cost imbalance detected
                    if (split.BaseQty != 0m && tran.BaseQty != 0m && exactCostStatus.QtyOnHand < 0m)
                        {
                        restoreData(backup);
                            throw new PXNegativeQtyImbalanceException();
                        }
                    if (split.BaseQty != 0m && tran.BaseQty != 0m && exactCostStatus.QtyOnHand == 0m && exactCostStatus.TotalCost != 0m)
                            {
                        restoreData(backup);
                            throw new PXQtyCostImbalanceException();
                        }
                }
				else if (costtran.InvtMult == -1)
                {
                    restoreData(null);
                    throw new PXNegativeQtyImbalanceException();
                }
				else if (costtran.InvtMult == 1 && tran.TranType != INTranType.Receipt)
				{
					exactCostStatus = InsertArtificialCostLayer(cmd.Cache, layer);
				}

				decimal? pretotal = costtran.TranCost;

                if (split.BaseQty == 0m && tran.BaseQty == 0m)
                {
                    //avoid PXFormula
                    PXCache<INTranCost>.RestoreCopy(prev_tran, costtran);
                }
                else
                {
                    decimal diff;
                    if (item.ValMethod != INValMethod.Standard && (diff = PXCurrencyAttribute.BaseRound(this, (tran.CostedQty + costtran.Qty - prev_tran.Qty) * tran.UnitCost) - (tran.TranCost ?? 0m) - (costtran.TranCost ?? 0m) + (prev_tran.TranCost ?? 0m)) != 0m)
                    {
                        costtran.TranCost += diff;
                        layer.TotalCost += (decimal)costtran.InvtMult * diff;
					}

                    //update after, otherwise objects.Equal(costtran, prev_tran)
                    costtran = intrancost.Update(costtran);
                }

                //write-off cost remainder
                if (tran.BaseQty != 0m && tran.BaseQty == tran.CostedQty)
                {
                    if (item.ValMethod == INValMethod.Standard)
                    {
                        costtran.VarCost += (tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                    else
                    {
                        costtran.TranCost += (tran.OrigTranCost - tran.TranCost);
                        layer.TotalCost += costtran.InvtMult * (tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                }
                //negative cost adjustment 1:1 INTran:INTranSplit
                else if (tran.BaseQty != 0m && tran.BaseQty == -tran.CostedQty)
                {
                    if (item.ValMethod == INValMethod.Standard)
                    {
                        costtran.VarCost += (-1m * tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                    else
                    {
                        costtran.TranCost += (-1m * tran.OrigTranCost - tran.TranCost);
						layer.TotalCost += costtran.InvtMult * (-1m * tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                }

                //write-off price remainder
                if (tran.BaseQty != 0m && (tran.BaseQty == tran.CostedQty || tran.BaseQty == -tran.CostedQty))
                {
                    costtran.TranAmt += (tran.OrigTranAmt - tran.TranAmt);
                    tran.TranAmt = tran.OrigTranAmt;
                }

				//accumulate total differences
				if (exactCostStatus != null)
				{
					if (item.ValMethod == INValMethod.Specific ||
						item.ValMethod == INValMethod.Average || item.ValMethod == INValMethod.FIFO)
					{
						exactCostStatus.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - pretotal);
					}
				}
			}
        }

		protected virtual INCostStatus InsertArtificialCostLayer(PXCache cache, INCostStatus layer)
		{
			var newLayer = (INCostStatus)cache.CreateInstance();
			newLayer.AccountID = layer.AccountID;
			newLayer.SubID = layer.SubID;
			newLayer.InventoryID = layer.InventoryID;
			newLayer.CostSiteID = layer.CostSiteID;
			newLayer.SiteID = layer.SiteID;
			newLayer.CostSubItemID = layer.CostSubItemID;
			newLayer.LotSerialNbr = layer.LotSerialNbr;
			newLayer.ReceiptDate = layer.ReceiptDate;
			newLayer.ReceiptNbr = layer.ReceiptNbr;
			newLayer.LayerType = layer.LayerType;
			newLayer.ValMethod = layer.ValMethod;
			newLayer.QtyOnHand = layer.QtyOnHand;
			newLayer.UnitCost = layer.UnitCost;
			newLayer.TotalCost = layer.TotalCost;
			newLayer = (INCostStatus)cache.Insert(newLayer);
			return newLayer;
		}

        public virtual void DropshipCost(INTran tran, INTranSplit split, InventoryItem item)
        {
            if (!IsUnmanagedTran(tran))
                return;
            
            INCostStatus layer = AccumUnmanagedCostStatus(tran, split, item);

            INTranCost costtran = new INTranCost();
            costtran.InvtAcctID = layer.AccountID;
            costtran.InvtSubID = layer.SubID;
            costtran.COGSAcctID = tran.COGSAcctID;
            costtran.COGSSubID = tran.COGSSubID;
            costtran.CostID = layer.CostID;
            costtran.InventoryID = layer.InventoryID;
            costtran.CostSiteID = layer.CostSiteID;
            costtran.CostSubItemID = layer.CostSubItemID;
            costtran.DocType = tran.DocType;
            costtran.TranType = tran.TranType;
            costtran.RefNbr = tran.RefNbr;
            costtran.LineNbr = tran.LineNbr;
            costtran.CostDocType = tran.DocType;
            costtran.CostRefNbr = tran.RefNbr;
            costtran.InvtMult = split.InvtMult;
            costtran.FinPeriodID = tran.FinPeriodID;
            costtran.TranPeriodID = tran.TranPeriodID;
            costtran.TranDate = tran.TranDate;
            costtran.TranAmt = 0m;
            costtran.IsVirtual = true;

            INTranCost prev_tran = intrancost.Insert(costtran);
            costtran = PXCache<INTranCost>.CreateCopy(prev_tran);

            costtran.Qty += INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);

            if (split.Qty.GetValueOrDefault() != 0m)
            {
                costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)(tran.OrigTranCost / tran.BaseQty) * (decimal)split.BaseQty);
                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)split.BaseQty * (decimal)(tran.OrigTranAmt / tran.BaseQty));

				PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
				costtran = intrancost.Update(costtran);
			}
            else if (tran.TranType == INTranType.Adjustment)
            {
				// Landed cost and PPV for non-stock items (Drop Ships)
				costtran.TranCost = tran.TranCost;
				costtran.TranAmt = tran.TranAmt;

				PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), null);
				costtran = intrancost.Update(costtran);
				PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
			}

        }

        public class PXSelectOversold<InventoryID, CostSubItemID, CostSiteID> : 
            PXSelectReadonly2<INTranCost,
                InnerJoin<INTran, 
                    On2<INTranCost.FK.Tran,
                    And<INTran.docType, Equal<INTranCost.costDocType>, 
                    And<INTran.refNbr, Equal<INTranCost.costRefNbr>>>>,
                InnerJoin<ReadOnlyCostStatus, On<ReadOnlyCostStatus.costID, Equal<INTranCost.costID>, 
                    And<ReadOnlyCostStatus.layerType, Equal<INLayerType.oversold>>>>>,
                Where<INTranCost.inventoryID, Equal<Current<InventoryID>>, 
                    And<INTranCost.costSubItemID, Equal<Current<CostSubItemID>>, 
                    And<INTranCost.costSiteID, Equal<Current<CostSiteID>>, 
                    And<INTranCost.isOversold, Equal<True>,
                    And<INTranCost.oversoldQty, Greater<decimal0>>>>>>>
            where InventoryID : IBqlField
            where CostSubItemID : IBqlField
            where CostSiteID : IBqlField
        {
            public PXSelectOversold(PXGraph graph)
                : base(graph)
            {
            }
        }

        public void ReceiveOverSold<TLayer, InventoryID, CostSubItemID, CostSiteID>(INRegister doc)
            where TLayer : INCostStatus
            where InventoryID : IBqlField
            where CostSubItemID : IBqlField
            where CostSiteID : IBqlField
        {
            foreach (TLayer accumlayer in this.Caches[typeof(TLayer)].Inserted)
            {
                if (accumlayer.QtyOnHand > 0m)
                {
                    foreach (PXResult<INTranCost, INTran, ReadOnlyCostStatus> res in PXSelectOversold<InventoryID, CostSubItemID, CostSiteID>.SelectMultiBound(this, new object[] { accumlayer }))
                    {
                        INTranCost costtran = res;
                        INTran intran = res;

                        costtran = PXCache<INTranCost>.CreateCopy(costtran);
                        costtran.CostDocType = doc.DocType;
                        costtran.CostRefNbr = doc.RefNbr;

                        INTranCostUpdate oversoldtrancostupdate = new INTranCostUpdate
                        {
							DocType = costtran.DocType,
                            RefNbr = costtran.RefNbr,
                            LineNbr = costtran.LineNbr,
                            CostID = costtran.CostID,
                            CostDocType = intran.DocType,
                            CostRefNbr = intran.RefNbr
                        };

                        oversoldtrancostupdate = intrancostupdate.Insert(oversoldtrancostupdate);

                        costtran.OversoldQty += oversoldtrancostupdate.OversoldQty;
                        costtran.OversoldTranCost += oversoldtrancostupdate.OversoldTranCost;

                        if (costtran.OversoldQty <= 0m)
                            continue;

                        INCostStatus oversoldlayer = AccumOversoldCostStatus((ReadOnlyCostStatus)res);

                        if (accumlayer.QtyOnHand != 0m)
                        {
                            accumlayer.AvgCost = accumlayer.TotalCost / accumlayer.QtyOnHand;
                        }
                        if ((((ReadOnlyCostStatus)res).QtyOnHand + oversoldlayer.QtyOnHand) != 0m)
                        {
                            oversoldlayer.AvgCost = (((ReadOnlyCostStatus)res).TotalCost + oversoldlayer.TotalCost) / (((ReadOnlyCostStatus)res).QtyOnHand + oversoldlayer.QtyOnHand);
                        }

                        if (costtran.OversoldQty <= accumlayer.QtyOnHand)
                        {
                            {
                                //reverse original cost
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);

                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;
                                newtran.TranAmt = 0m;
                                newtran.Qty = 0m;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = 0m;
                                newtran.VarCost = 0m;

                                PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
                                //count for multiply layers adjusting single oversold transactions
                                INTranCost prev_tran = intrancost.Insert(newtran);
                                newtran = PXCache<INTranCost>.CreateCopy(prev_tran);

                                newtran.IsOversold = false;
                                newtran.Qty -= costtran.OversoldQty;
                                if (oversoldlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
                                    newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.UnitCost);
                                }
                                else
                                {
                                    newtran.TranCost -= costtran.OversoldTranCost;
                                }

                                decimal? oversoldcostmovement = -newtran.TranCost + prev_tran.TranCost;
                                decimal? oversoldqtymovement = -newtran.Qty + prev_tran.Qty;
                                oversoldlayer.TotalCost += oversoldcostmovement;
                                oversoldlayer.QtyOnHand += oversoldqtymovement;

                                oversoldtrancostupdate.OversoldQty -= oversoldqtymovement;
                                oversoldtrancostupdate.OversoldTranCost -= oversoldcostmovement;
                                oversoldtrancostupdate.ResetOversoldFlag = true;

                                intrancost.Update(newtran);


                            }
                            {
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
                                newtran.IsOversold = false;
                                newtran.CostID = accumlayer.CostID;
                                newtran.InvtAcctID = accumlayer.AccountID;
                                newtran.InvtSubID = accumlayer.SubID;
                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.Qty = costtran.OversoldQty;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.AvgCost);
                                if (accumlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.AvgCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.UnitCost);
                                }
                                newtran.TranAmt = 0m;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;

                                PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
                                intrancost.Cache.Insert(newtran);

                                accumlayer.TotalCost -= PXCurrencyAttribute.BaseRound(this, costtran.OversoldQty * accumlayer.AvgCost);
                                accumlayer.QtyOnHand -= costtran.OversoldQty;

                            }
                        }
                        else if (accumlayer.QtyOnHand > 0m)
                        {
                            {
                                //reverse original cost
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
                                newtran.IsOversold = true;
                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;
                                newtran.TranAmt = 0m;
                                newtran.Qty = 0m;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = 0m;
                                newtran.VarCost = 0m;

                                PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
                                //count for multiply layers adjusting single oversold transactions
                                INTranCost prev_tran = intrancost.Insert(newtran);
                                newtran = PXCache<INTranCost>.CreateCopy(prev_tran);

                                newtran.Qty -= accumlayer.QtyOnHand;
                                if (oversoldlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
                                    newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.UnitCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
                                }
                                else
                                {
                                    newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * costtran.OversoldTranCost / costtran.OversoldQty);
                                }

                                decimal? oversoldcostmovement = -newtran.TranCost + prev_tran.TranCost;
                                decimal? oversoldqtymovement = -newtran.Qty + prev_tran.Qty;
                                oversoldlayer.TotalCost += oversoldcostmovement;
                                oversoldlayer.QtyOnHand += oversoldqtymovement;

                                oversoldtrancostupdate.OversoldTranCost -= oversoldcostmovement;
                                oversoldtrancostupdate.OversoldQty -= oversoldqtymovement;
                                intrancost.Update(newtran);
                            }
                            {
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
                                newtran.IsOversold = false;
                                newtran.CostID = accumlayer.CostID;
                                newtran.InvtAcctID = accumlayer.AccountID;
                                newtran.InvtSubID = accumlayer.SubID;
                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.Qty = accumlayer.QtyOnHand;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = accumlayer.TotalCost;
                                if (accumlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.VarCost = -accumlayer.TotalCost + PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.UnitCost);
                                }
                                newtran.TranAmt = 0m;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;

                                PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
                                intrancost.Cache.Insert(newtran);

                                accumlayer.TotalCost = 0m;
                                accumlayer.QtyOnHand = 0m;
                            }
                        }
                    }
                }
            }
        }

        public virtual void ReceiveOversold(INRegister doc)
        {
            ReceiveOverSold<FIFOCostStatus, FIFOCostStatus.inventoryID, FIFOCostStatus.costSubItemID, FIFOCostStatus.costSiteID>(doc);
            ReceiveOverSold<AverageCostStatus, AverageCostStatus.inventoryID, AverageCostStatus.costSubItemID, AverageCostStatus.costSiteID>(doc);
            ReceiveOverSold<StandardCostStatus, StandardCostStatus.inventoryID, StandardCostStatus.costSubItemID, StandardCostStatus.costSiteID>(doc);
        }

        public virtual INCostStatus AccumulatedCostStatus(INCostStatus layer, InventoryItem item)
        {
            INCostStatus ret = null;

            if (layer.LayerType == INLayerType.Oversold)
            {
                ret = new OversoldCostStatus();
                ret.AccountID = layer.AccountID;
                ret.SubID = layer.SubID;
                ret.InventoryID = layer.InventoryID;
                ret.CostSiteID = layer.CostSiteID;
	            ret.SiteID = layer.SiteID;
                ret.CostSubItemID = layer.CostSubItemID;
                ret.ReceiptDate = layer.ReceiptDate;
                ret.ReceiptNbr = layer.ReceiptNbr;
                ret.LayerType = layer.LayerType;

                return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(ret);
            }

            switch (item.ValMethod)
            {
                case INValMethod.Average:
                    ret = new AverageCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = layer.LayerType;
                    ret.ReceiptNbr = layer.ReceiptNbr;

                    return (AverageCostStatus)averagecoststatus.Cache.Insert(ret);
                case INValMethod.Standard:
                    ret = new StandardCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = layer.LayerType;
                    ret.ReceiptNbr = layer.ReceiptNbr;

                    return (StandardCostStatus)standardcoststatus.Cache.Insert(ret);
                case INValMethod.FIFO:
                    ret = new FIFOCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.ReceiptDate = layer.ReceiptDate;
                    ret.ReceiptNbr = layer.ReceiptNbr;
                    ret.LayerType = layer.LayerType;

                    return (FIFOCostStatus)fifocoststatus.Cache.Insert(ret);
                case INValMethod.Specific:
                    if (layer.CostSiteID == INTransitSiteID)
                        ret = new SpecificTransitCostStatus();
                    else
                        ret = new SpecificCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LotSerialNbr = layer.LotSerialNbr;
                    ret.ReceiptDate = layer.ReceiptDate;
                    ret.ReceiptNbr = layer.ReceiptNbr;
                    ret.LayerType = layer.LayerType;

                    if (layer.CostSiteID == INTransitSiteID)
                        return (SpecificTransitCostStatus)specifictransitcoststatus.Cache.Insert(ret);
                    else
                        return (SpecificCostStatus)specificcoststatus.Cache.Insert(ret);
                default:
                    throw new PXException();
            }
        }

        public virtual INCostStatus AccumulatedTransferCostStatus(INCostStatus layer, INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus ret = null;
            bool transferModeEnabled = !IsIngoingTransfer(tran) && !IsOneStepTransfer();
            INCostStatus result;
            switch (item.ValMethod)
            {
                case INValMethod.Average:
                    ret = new AverageCostStatus();
                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? INTransitSiteID : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = INLayerType.Normal;
                    if (!transferModeEnabled)
                    {
                        ret.ReceiptNbr = INLayerRef.ZZZ;
                    }
                    else
                    {
                        ret.ReceiptNbr = tran.RefNbr;
                    }

                    result = (INCostStatus)averagecoststatus.Cache.Insert(ret);
                    return result;
                case INValMethod.Standard:
                    ret = new StandardCostStatus();
                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? INTransitSiteID : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = INLayerType.Normal;
                    if (!transferModeEnabled)
                    {
                        ret.ReceiptNbr = INLayerRef.ZZZ;
                    }
                    else
                    {
                        ret.ReceiptNbr = tran.RefNbr;
                    }

                    result = (INCostStatus)standardcoststatus.Cache.Insert(ret);
                    return result;
                case INValMethod.FIFO:
                    ret = new FIFOCostStatus();
                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? INTransitSiteID : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;

                    DateTime? fifoDate;
                    String fifoNbr;

                    if (SameWarehouseTransfer(tran, split))
                    {
                        fifoDate = layer.ReceiptDate;
                        fifoNbr = layer.ReceiptNbr;
                    }
                    else
                    {
                        fifoDate = tran.TranDate;
                        fifoNbr = tran.RefNbr;
                    }

                    ret.ReceiptDate = fifoDate;
                    ret.ReceiptNbr = fifoNbr;

                    ret.LayerType = INLayerType.Normal;

                    result = (INCostStatus)fifocoststatus.Cache.Insert(ret);
                    return result;
                case INValMethod.Specific:
                    if (transferModeEnabled)
                        ret = new SpecificTransitCostStatus();
                    else
                        ret = new SpecificCostStatus();

                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? INTransitSiteID : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LotSerialNbr = layer.LotSerialNbr;

                    if (SameWarehouseTransfer(tran, split))
                    {
                        ret.ReceiptNbr = layer.ReceiptNbr;
                        ret.ReceiptDate = layer.ReceiptDate;
                    }
                    else
                    {
                        ret.ReceiptNbr = tran.RefNbr;
                        ret.ReceiptDate = tran.TranDate;
                    }

                    ret.LayerType = INLayerType.Normal;

                    if (transferModeEnabled)
                    {
                        result = (INCostStatus)specifictransitcoststatus.Cache.Insert(ret);
                    }
                    else
                    {
                        result = (INCostStatus)specificcoststatus.Cache.Insert(ret);
                    }

                    return result;
                default:
                    throw new PXException();
            }
        }

        protected virtual bool SameWarehouseTransfer(INTran tran, INTranSplit split)
        {
            if (!IsOneStepTransfer())
                return false;

            if (split.FromSiteID == null && tran.OrigDocType != null && tran.OrigRefNbr != null && tran.OrigLineNbr != null)
            {
                INTran ortran = INTran.PK.Find(this, tran.OrigDocType, tran.OrigRefNbr, tran.OrigLineNbr);
                split.FromSiteID = ortran.SiteID;
            }
            return
                split.FromSiteID == tran.SiteID;
        }

        public virtual void IssueCost(INCostStatus layer, INTran tran, INTranSplit split, InventoryItem item, ref decimal QtyUnCosted)
        {
            INTranCost costtran = new INTranCost();
            costtran.InvtAcctID = layer.AccountID;
            costtran.InvtSubID = layer.SubID;
            costtran.COGSAcctID = tran.COGSAcctID;
            costtran.COGSSubID = tran.COGSSubID;
            costtran.CostID = layer.CostID;
            costtran.InventoryID = layer.InventoryID;
            costtran.CostSiteID = layer.CostSiteID;
            costtran.CostSubItemID = layer.CostSubItemID;
            costtran.IsOversold = (layer.LayerType == INLayerType.Oversold);
            costtran.DocType = tran.DocType;
            costtran.TranType = tran.TranType;
            costtran.RefNbr = tran.RefNbr;
            costtran.LineNbr = tran.LineNbr;
            costtran.CostDocType = tran.DocType;
            costtran.CostRefNbr = tran.RefNbr;
            costtran.IsVirtual = IsIngoingTransfer(tran);
            //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
            costtran.InvtMult = IsIngoingTransfer(tran) ? (short)-split.InvtMult : split.InvtMult;
            costtran.FinPeriodID = tran.FinPeriodID;
            costtran.TranPeriodID = tran.TranPeriodID;
            costtran.TranDate = tran.TranDate;
            costtran.TranAmt = 0m;

			if (tran.DocType == INDocType.Receipt && IsIngoingTransfer(tran))
			{// We should take InTransit account from existing intransit layer as it may be changed in setup.
				tran.AcctID = layer.AccountID;
				tran.SubID = layer.SubID;
			}

            if (costtran.IsVirtual != true)
                PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
            costtran = PXCache<INTranCost>.CreateCopy(intrancost.Insert(costtran));

            INCostStatus accumlayer = AccumulatedCostStatus(layer, item);

            //assigning currently accumulated qties
            decimal? issuedqty = costtran.Qty;
            decimal? issuedcost = costtran.TranCost;

            if (layer.QtyOnHand <= QtyUnCosted)
            {
                QtyUnCosted -= (decimal)layer.QtyOnHand;
                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)layer.QtyOnHand * (decimal)tran.UnitPrice);
                costtran.Qty += layer.QtyOnHand;
                costtran.TranCost += layer.TotalCost;
                if (accumlayer.ValMethod == INValMethod.Standard && (!IsIngoingTransfer(tran) && !IsOneStepTransfer()))
                {
                    costtran.VarCost += PXDBCurrencyAttribute.BaseRound(this, (decimal)layer.QtyOnHand * (decimal)layer.UnitCost) - layer.TotalCost;
                }
                layer.QtyOnHand = 0m;
                layer.TotalCost = 0m;
            }
            else
            {
                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)QtyUnCosted * (decimal)tran.UnitPrice);
                if (PXCurrencyAttribute.IsNullOrEmpty(layer.UnitCost))
                {
                    layer.UnitCost = (decimal)layer.TotalCost / (decimal)layer.QtyOnHand;
                }

                layer.QtyOnHand -= QtyUnCosted;
                layer.TotalCost += costtran.TranCost;
                layer.TotalCost -= PXCurrencyAttribute.BaseRound(this, (costtran.Qty + QtyUnCosted) * (decimal)layer.UnitCost);

                costtran.Qty += QtyUnCosted;
                costtran.TranCost = PXCurrencyAttribute.BaseRound(this, costtran.Qty * (decimal)layer.UnitCost);

                QtyUnCosted = 0m;
            }

            issuedqty -= costtran.Qty;
            issuedcost -= costtran.TranCost;

            accumlayer.QtyOnHand += issuedqty;
            accumlayer.TotalCost += issuedcost;

            TransferCost(tran, split, item, accumlayer, costtran, issuedqty.Value, issuedcost.Value);

            //Accumulate cost issued via PXFormula for Issues only
            costtran = intrancost.Update(costtran);

            //negative cost adjustment 1:1 INTran:INTranSplit
            if (tran.InvtMult == 1m && tran.BaseQty == -tran.CostedQty)
            {
                if (item.ValMethod != INValMethod.Specific)
                {
                    //reset variance to difference
                    costtran.VarCost = (-1m * tran.OrigTranCost - tran.TranCost);
                }
				if (item.ValMethod != INValMethod.Standard)
				{
					tran.TranCost = -tran.TranCost; // we should preserve the value that was actualy issued from cost layers.
				}
				else
				{
					tran.TranCost = tran.OrigTranCost; // we should preserve original value to be clear where does the std cost variance come from.
                }
            }

            //write-off price remainder
            if (tran.BaseQty != 0m && (tran.BaseQty == tran.CostedQty || tran.BaseQty == -tran.CostedQty))
            {
                costtran.TranAmt += (tran.OrigTranAmt - tran.TranAmt);
                tran.TranAmt = tran.OrigTranAmt;
            }

            //init OversoldQty
            if (costtran.IsOversold ?? false)
            {
                costtran.OversoldQty = costtran.Qty;
                costtran.OversoldTranCost = costtran.TranCost;
            }
        }

        public virtual void AssignQty(ReadOnlyReceiptStatus layer, ref decimal QtyUnAssigned)
        {
            ReceiptStatus accumreceiptstatus = new ReceiptStatus();
            accumreceiptstatus.ReceiptID = layer.ReceiptID;
            accumreceiptstatus.ReceiptNbr = layer.ReceiptNbr;
            accumreceiptstatus.SubID = layer.SubID;
            accumreceiptstatus.AccountID = layer.AccountID;
            accumreceiptstatus.CostSiteID = layer.CostSiteID;
            accumreceiptstatus.OrigQty = layer.OrigQty;
            accumreceiptstatus.ReceiptDate = layer.ReceiptDate;
            accumreceiptstatus.LotSerialNbr = layer.LotSerialNbr == null || layer.ValMethod != INValMethod.Specific ? String.Empty : layer.LotSerialNbr;
            accumreceiptstatus.LayerType = layer.LayerType;
            accumreceiptstatus.InventoryID = layer.InventoryID;
            accumreceiptstatus.CostSubItemID = layer.CostSubItemID;

            if (QtyUnAssigned < 0)
            {
                if (layer.QtyOnHand <= -QtyUnAssigned)
                {
                    QtyUnAssigned += (decimal)layer.QtyOnHand;
                    accumreceiptstatus.QtyOnHand = -layer.QtyOnHand;
                    layer.QtyOnHand = 0m;
                }
                else
                {
                    layer.QtyOnHand += QtyUnAssigned;
                    accumreceiptstatus.QtyOnHand = QtyUnAssigned;
                    QtyUnAssigned = 0m;
                }
            }
            else
            {
                accumreceiptstatus.QtyOnHand = QtyUnAssigned;
                layer.QtyOnHand += QtyUnAssigned;
            }
            receiptstatus.Insert(accumreceiptstatus);
        }

        public virtual void IssueQty(INCostStatus layer)
        {
            decimal QtyUnAssigned = layer.QtyOnHand ?? 0m;
            if (QtyUnAssigned >= 0m)
                return;
            PXView readonlyreceiptstatus = GetReceiptStatusByKeysView(layer);
            readonlyreceiptstatus.OrderByNew<OrderBy<Asc<ReadOnlyReceiptStatus.receiptDate, Asc<ReadOnlyReceiptStatus.receiptID>>>>();

            foreach (ReadOnlyReceiptStatus rsLayer in readonlyreceiptstatus.SelectMulti(layer.InventoryID, layer.CostSiteID, layer.CostSubItemID, layer.AccountID, layer.SubID, layer.LotSerialNbr))
            {
                if (rsLayer.QtyOnHand > 0m)
                {
                    AssignQty(rsLayer, ref QtyUnAssigned);

                    readonlyreceiptstatus.Cache.SetStatus(rsLayer, PXEntryStatus.Held);

                    if (QtyUnAssigned == 0m)
                    {
                        break;
                    }
                }
            }
            /* Commented out for legacy db support
            if (QtyUnAssigned != 0m)
            {
                INLocation location = 
                    PXSelectReadonly<INLocation, Where<INLocation.costSiteID, Equal<Required<INCostStatus.costSiteID>>>>
                    .SelectWindowed(this,0, 1, new object[] { layer.CostSiteID });
                int? warehouseid = location == null ? location.SiteID : layer.CostSiteID;
                throw new PXException(Messages.StatusCheck_QtyOnHandNegative,
                    PXForeignSelectorAttribute.GetValueExt<INCostStatus.inventoryID>(this.Caches[layer.GetType()], layer),
                    PXForeignSelectorAttribute.GetValueExt<INCostStatus.costSubItemID>(this.Caches[layer.GetType()], layer),
                    PXForeignSelectorAttribute.GetValueExt<INCostStatus.costSiteID>(this.Caches[layer.GetType()], layer));
            }*/
        }

        public virtual void IssueCost(INTran tran, INTranSplit split, InventoryItem item, bool correctImbalance)
        {
            //costing is done in parallel, i.e. if 2 splits are all the same as transaction, one accumulated cost tran(cost&qty summarized) will be added, if varied in cost key fields, then variance
            //will be via LayerID which will be different
            if ((tran.InvtMult == (short)-1 && (tran.ExactCost != true || correctImbalance))
				|| tran.InvtMult == (short)1 && split.InvtMult == (short)-1
					&& (item.ValMethod == INValMethod.Standard && !correctImbalance || item.ValMethod != INValMethod.Standard && correctImbalance)
				|| (tran.TranType == INTranType.Transfer && !(IsOneStepTransfer() && split.InvtMult != (short)-1)))
            {
                object[] parameters;
                PXView cmd = GetCostStatusCommand(tran, split, item, out parameters, correctImbalance, null);

                if (cmd != null)
                {
					INCostStatus lastLayer = null;
                    decimal QtyUnCosted = INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);
                    foreach (INCostStatus layer in cmd.SelectMulti(parameters))
                    {
						lastLayer = layer;
                        if (layer.QtyOnHand > 0m)
                        {
                            IssueCost(layer, tran, split, item, ref QtyUnCosted);

                            cmd.Cache.Hold(layer);

                            if (QtyUnCosted == 0m)
                            {
                                break;
                            }
                        }
                    }

                    //negative cost adjustment
                    if (tran.InvtMult == (short)1 && QtyUnCosted > 0m)
                    {
                        if (item.ValMethod == INValMethod.Standard)
                        {
                            throw new PXQtyCostImbalanceException();
                        }

                        ThrowNegativeQtyException(tran, split, lastLayer);
                    }

					if (tran.POReceiptType == PO.POReceiptType.POReturn && tran.ExactCost == true && QtyUnCosted > 0m)
					{
						ThrowNegativeQtyException(tran, split, lastLayer);
					}

                    if (QtyUnCosted > 0m && (!IsIngoingTransfer(tran) || IsOneStepTransfer()))
                    {
                        INCostStatus oversold = PXCache<INCostStatus>.CreateCopy(AccumOversoldCostStatus(tran, split, item));
                        //qty and cost in this dummy layer must be set explicitly, not added.
                        oversold.QtyOnHand = QtyUnCosted;
                        oversold.TotalCost = PXCurrencyAttribute.BaseRound(this, QtyUnCosted * (decimal)oversold.UnitCost);

                        IssueCost(oversold, tran, split, item, ref QtyUnCosted);
                    }

                    if (QtyUnCosted > 0m)
                    {
                        throw new PXException(Messages.InternalError, 500);
                    }
                }
            }
        }

		protected virtual void ThrowNegativeQtyException(INTran tran, INTranSplit split, INCostStatus lastLayer)
		{
                        object costSubItemID = split.CostSubItemID;
                        intranselect.Cache.RaiseFieldSelecting<INTran.subItemID>(tran, ref costSubItemID, true);
                        object inventoryCD = intranselect.Cache.GetValueExt<INTran.inventoryID>(tran);

                        if (IsIngoingTransfer(tran))
                        {
                            if (split.ValMethod == INValMethod.Specific)
                                throw new PXException(Messages.StatusCheck_QtyTransitLotSerialOnHandNegative, inventoryCD, costSubItemID, split.LotSerialNbr);
                            throw new PXException(Messages.StatusCheck_QtyTransitOnHandNegative, inventoryCD, costSubItemID);
                        }

			if (split.ValMethod == INValMethod.Specific && !string.IsNullOrEmpty(split.LotSerialNbr))
							throw new PXException(Messages.StatusCheck_QtyNegativeSPC, inventoryCD, costSubItemID, split.LotSerialNbr);

			if (split.ValMethod == INValMethod.FIFO && !string.IsNullOrEmpty(lastLayer?.ReceiptNbr))
			{
				if (tran.POReceiptType == PO.POReceiptType.POReturn && tran.ExactCost == true)
					throw new PXException(Messages.StatusCheck_QtyNegativeFifoExactCost, inventoryCD, costSubItemID, lastLayer.ReceiptNbr);

				throw new PXException(Messages.StatusCheck_QtyNegativeFifo, inventoryCD, costSubItemID, lastLayer.ReceiptNbr);
			}

                        INSite site = INSite.PK.Find(this, split.CostSiteID);
                        if (site != null)
                            throw new PXException(Messages.StatusCheck_QtyNegative, inventoryCD, costSubItemID, site.SiteCD);

                        var siteAndLocation = (PXResult<INSite, INLocation>)
                            PXSelectReadonly2<INSite,
                            InnerJoin<INLocation, On<INLocation.FK.Site>>,
                            Where<INLocation.locationID, Equal<Required<INLocation.locationID>>>>
                            .SelectWindowed(this, 0, 1, split.CostSiteID);
                        if (siteAndLocation != null)
                            throw new PXException(Messages.StatusCheck_QtyNegative1, inventoryCD, costSubItemID, siteAndLocation.GetItem<INLocation>().LocationCD, siteAndLocation.GetItem<INSite>().SiteCD);

                        throw new PXException(Messages.StatusCheck_QtyNegative, inventoryCD, costSubItemID, split.CostSiteID);
                    }

        public virtual void TransferCost(INTran tran, INTranSplit split, InventoryItem item, INCostStatus issueCost, INTranCost issueTranCost, decimal issuedQty, decimal issuedCost)
        {
            if (tran.TranType != INTranType.Transfer)
                return;

            if (IsOneStepTransfer())
            {
                foreach (INTran positivetran in intranselect.Cache.Cached)
                {
                    if (positivetran.OrigLineNbr == tran.LineNbr && positivetran.OrigRefNbr == tran.RefNbr && positivetran.OrigDocType == tran.DocType)
                    {
                        foreach (INTranSplit positivesplit in PXParentAttribute.SelectChildren(intransplit.Cache, positivetran, typeof(INTran)))
                        {
                            if (split.ToLocationID == positivesplit.LocationID || (split.ToLocationID == null && positivetran.LocationID == split.ToLocationID))
                            {
                                split = (INTranSplit)intransplit.Cache.CreateCopy(split);

                                split.FromSiteID = split.SiteID;
                                split.FromLocationID = split.LocationID;
                                split.SiteID = positivesplit.ToSiteID ?? tran.ToSiteID;
                                split.ToSiteID = split.SiteID;
                                split.LocationID = positivesplit.ToLocationID ?? tran.ToLocationID;
                                split.ToLocationID = split.LocationID;
                                split.CostSiteID = positivesplit.CostSiteID;
                                break;
                            }
                        }

                        tran = positivetran;
                        break;
                    }
                }
            }

            INCostStatus accumlayer = AccumulatedTransferCostStatus(issueCost, tran, split, item);

            accumlayer.QtyOnHand -= issuedQty;
            decimal costadded;
            if (accumlayer.ValMethod == INValMethod.Standard && accumlayer.CostSiteID != INTransitSiteID)
            {
                //do not add cost, recalculate
                costadded = -PXCurrencyAttribute.BaseRound(this, (decimal)accumlayer.UnitCost * (decimal)issuedQty);
            }
            else
            {
                costadded = -issuedCost;
            }

            accumlayer.TotalCost += costadded;

            var orig_trancost = issueTranCost;

            INTranCost costtran = new INTranCost();
            costtran.IsVirtual = !IsOneStepTransfer() && !IsIngoingTransfer(tran);
            costtran.InvtAcctID = accumlayer.AccountID;
            costtran.InvtSubID = accumlayer.SubID;
            costtran.InventoryID = accumlayer.InventoryID;
            costtran.CostSiteID = accumlayer.CostSiteID;
            costtran.CostSubItemID = accumlayer.CostSubItemID;
            costtran.COGSAcctID = tran.COGSAcctID;
            costtran.COGSSubID = tran.COGSSubID;
            costtran.FinPeriodID = tran.FinPeriodID;
            costtran.TranPeriodID = tran.TranPeriodID;
            costtran.TranDate = tran.TranDate;

            //init keys
            costtran.CostID = accumlayer.CostID;
            costtran.DocType = tran.DocType;
            costtran.TranType = tran.TranType;
            costtran.RefNbr = tran.RefNbr;
            costtran.LineNbr = tran.LineNbr;
            costtran.CostDocType = tran.DocType;
            costtran.CostRefNbr = tran.RefNbr;

            costtran.InvtMult = (short?)-orig_trancost.InvtMult;
            costtran = intrancost.Insert(costtran);

            if (!(costtran.IsVirtual ?? false))
                PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);

            costtran.QtyOnHand += -issuedQty;

            if (accumlayer.ValMethod == INValMethod.Standard)
            {
                costtran.TranCost += costadded;
                costtran.VarCost += -issuedCost - costadded;
            }
            else
            {
                costtran.TranCost += -issuedCost;
            }

            intrancost.Update(costtran);

            if (tran.BaseQty != 0m && tran.BaseQty == tran.CostedQty)
            {
                if (accumlayer.ValMethod == INValMethod.Standard)
                {
                    tran.TranCost = tran.OrigTranCost;
                }
            }
        }

        bool WIPCalculated = false;
        decimal? WIPVariance = 0m;

        public virtual void AssembleCost(INTran tran, INTranSplit split, InventoryItem item)
        {
            if ((tran.TranType == INTranType.Assembly || tran.TranType == INTranType.Disassembly) && tran.AssyType == INAssyType.KitTran && tran.InvtMult == (short)1)
            {
                tran.TranCost = 0m;

                //rollup stock components
                foreach (INTranCost costtran in intrancost.Cache.Inserted)
                {
                    if (string.Equals(costtran.CostDocType, tran.DocType) && string.Equals(costtran.CostRefNbr, tran.RefNbr) && costtran.InvtMult == (short)-1)
                    {
                        tran.TranCost += costtran.TranCost;
                    }
                }

                //rollup non-stock components
                foreach (INTran costtran in intranselect.Cache.Updated)
                {
                    if (string.Equals(costtran.DocType, tran.DocType) && string.Equals(costtran.RefNbr, tran.RefNbr) && costtran.AssyType == INAssyType.OverheadTran && costtran.InvtMult == (short)-1)
                    {
                        tran.TranCost += costtran.TranCost;
                    }
                }
            }

            if ((tran.TranType == INTranType.Assembly || tran.TranType == INTranType.Disassembly) && (tran.AssyType == INAssyType.CompTran || tran.AssyType == INAssyType.OverheadTran) && tran.InvtMult == (short)1)
            {
                if (WIPCalculated == false)
                {
                    //rollup kit disassembled
                    foreach (INTranCost costtran in intrancost.Cache.Inserted)
                    {
                        if (string.Equals(costtran.CostDocType, tran.DocType) && string.Equals(costtran.CostRefNbr, tran.RefNbr) && costtran.InvtMult == (short)-1)
                        {
                            WIPVariance += costtran.TranCost;
                        }
                    }
                    WIPCalculated = true;
                }
                WIPVariance -= tran.TranCost;
            }
        }

        protected virtual bool IsIngoingTransfer(INTran tran)
        {
            return
                tran.TranType == INTranType.Transfer && tran.InvtMult == 1m;
        }
        public virtual void UpdateCostStatus(INTran prev_tran, INTran tran, INTranSplit split, InventoryItem item)
        {
            if (object.Equals(prev_tran, tran) == false)
            {
                AssembleCost(tran, split, item);

                if (tran.BaseQty != 0m)
                {
                    tran.CostedQty = 0m;
                    tran.OrigTranCost = tran.TranCost;
                    tran.OrigTranAmt = tran.TranAmt;
                    tran.TranCost = 0m;
                    tran.TranAmt = 0m;

                    //CommandPreparing will prevent actual update
                    if (Math.Abs((decimal)tran.OrigTranCost - PXCurrencyAttribute.BaseRound(this, (decimal)tran.BaseQty * (decimal)tran.UnitCost)) > 0.00005m)
                        tran.UnitCost = PXPriceCostAttribute.Round((decimal)tran.OrigTranCost / (decimal)tran.BaseQty);
                    if (Math.Abs((decimal)tran.OrigTranAmt - PXCurrencyAttribute.BaseRound(this, (decimal)tran.BaseQty * (decimal)tran.UnitPrice)) > 0.00005m)
                        tran.UnitPrice = PXPriceCostAttribute.Round((decimal)tran.OrigTranAmt / (decimal)tran.BaseQty);
                }
                else
                {
                    //prevent SelectSiblings on null value.
                    tran.CostedQty = 0m;
                    tran.UnitCost = 0m;
                    tran.UnitPrice = 0m;
                }
            }

            DropshipCost(tran, split, item);

            try
            {
                ReceiveCost(tran, split, item, false);
                IssueCost(tran, split, item, false);
            }
            catch(PXNegativeQtyImbalanceException)
            {
                IssueCost(tran, split, item, true);
            }
            catch (PXQtyCostImbalanceException)
            {
				try
				{
                ReceiveCost(tran, split, item, true);
                IssueCost(tran, split, item, true);
            }
				catch (PXNegativeQtyImbalanceException)
				{
					ThrowNegativeQtyException(tran, split, null);
				}
            }
        }

        private void ProceedReceiveQtyForLayer(INCostStatus layer)
        {
            if (layer.LayerType != INLayerType.Normal)
                return;
            INRegister doc = inregister.Current;
            bool isqtyonhandcalcneeded = layer.ValMethod != INValMethod.FIFO;
            if (isqtyonhandcalcneeded && layer.QtyOnHand < 0m)
            {
                IssueQty(layer);
                return;
            }

			if (doc.DocType == INDocType.Transfer)
				return;

            bool makenew = (doc.DocType == INDocType.Receipt || doc.DocType == INDocType.Disassembly || doc.DocType == INDocType.Production) || !isqtyonhandcalcneeded;
            if (!makenew && layer.QtyOnHand > 0m)
            {
                PXView receiptview = GetReceiptStatusByKeysView(layer);
                receiptview.OrderByNew<OrderBy<Desc<ReadOnlyReceiptStatus.receiptDate, Desc<ReadOnlyReceiptStatus.receiptID>>>>();


                ReadOnlyReceiptStatus rs = (ReadOnlyReceiptStatus)receiptview.SelectSingle(layer.InventoryID, layer.CostSiteID, layer.CostSubItemID, layer.AccountID, layer.SubID, layer.LotSerialNbr);
                decimal QtyUnAssigned = layer.QtyOnHand ?? 0m;
                if (rs != null)
                    AssignQty(rs, ref QtyUnAssigned);
                else
                    makenew = true;
            }

            if (makenew && (layer.QtyOnHand > 0m || (doc.DocType == INDocType.Receipt && layer.QtyOnHand == 0m)))
            {
                ReceiptStatus receipt = new ReceiptStatus();
                receipt.InventoryID = layer.InventoryID;
                receipt.CostSiteID = layer.CostSiteID;
                receipt.CostSubItemID = layer.CostSubItemID;
                receipt.ReceiptNbr = doc.OrigRefNbr == null ? doc.RefNbr : doc.OrigRefNbr;
                receipt.ReceiptDate = doc.TranDate;
                receipt.OrigQty = layer.OrigQtyOnHand;
                receipt.ValMethod = layer.ValMethod;
                receipt.AccountID = layer.AccountID;
                receipt.SubID = layer.SubID;
                receipt.LotSerialNbr = layer.LotSerialNbr == null || layer.ValMethod != INValMethod.Specific ? String.Empty : layer.LotSerialNbr;
                receipt.QtyOnHand = layer.QtyOnHand;
                var prev_recstat = receiptstatus.Insert(receipt);
            }
        }

        private void ReceiveQty()
        {
            foreach (AverageCostStatus layer in averagecoststatus.Cache.Inserted)
                ProceedReceiveQtyForLayer(layer);

            foreach (StandardCostStatus layer in standardcoststatus.Cache.Inserted)
                ProceedReceiveQtyForLayer(layer);

            foreach (SpecificCostStatus layer in specificcoststatus.Cache.Inserted)
                ProceedReceiveQtyForLayer(layer);
        }

        public class INHistBucket
        {
            public decimal SignReceived = 0m;
            public decimal SignIssued = 0m;
            public decimal SignSales = 0m;
            public decimal SignCreditMemos = 0m;
            public decimal SignDropShip = 0m;
            public decimal SignTransferIn = 0m;
            public decimal SignTransferOut = 0m;
            public decimal SignAdjusted = 0m;
            public decimal SignAssemblyIn = 0m;
            public decimal SignAssemblyOut = 0m;
            public decimal SignYtd = 0m;

            public INHistBucket(INTran tran)
                : this(tran.TranType, tran.InvtMult)
            {
            }

            public INHistBucket(INTranCost costtran, INTran intran)
                : this(costtran.TranType, costtran.InvtMult)
            {
                if ((costtran.TranType == INTranType.Transfer || costtran.TranType == INTranType.Assembly || costtran.TranType == INTranType.Disassembly) && (costtran.CostDocType != intran.DocType || costtran.CostRefNbr != intran.RefNbr))
                {
                    this.SignTransferOut = 0m;
                    this.SignSales = 1m;
                }
            }

            public INHistBucket(INTranSplit tran)
                : this(tran.TranType, tran.InvtMult)
            {
            }

            public INHistBucket(string TranType, short? InvtMult)
            {
                SignYtd = (decimal)InvtMult;

                switch (TranType)
                {
                    case INTranType.Receipt:
                        SignReceived = 1m;
                        break;
                    case INTranType.Issue:
                        SignIssued = 1m;
                        break;
                    case INTranType.Return:
                        SignIssued = -1m;
                        break;
                    case INTranType.Invoice:
                    case INTranType.DebitMemo:
                        if (SignYtd == 0m)
                        {
                            SignDropShip = 1m;
                        }
                        else
                        {
                            SignSales = 1m;
                        }
                        break;
                    case INTranType.CreditMemo:
                        if (SignYtd == 0m)
                        {
                            SignDropShip = -1m;
                        }
                        else
                        {
                            SignCreditMemos = 1m;
                        }
                        break;
                    case INTranType.Transfer:
                        if (InvtMult == 1m)
                        {
                            SignTransferIn = 1m;
                        }
                        else
                        {
                            SignTransferOut = 1m;
                        }
                        break;
                    case INTranType.Adjustment:
                        if (InvtMult == 0m)
                        {
                            SignAdjusted = 1m;
                            SignSales = 1m;
                        }
                        else
                        {
                            SignAdjusted = (decimal)InvtMult;
                        }
                        break;
                    case INTranType.StandardCostAdjustment:
                    case INTranType.NegativeCostAdjustment:
                        SignAdjusted = 1m;
                        break;
                    case INTranType.Assembly:
                    case INTranType.Disassembly:
                        if (InvtMult == 1m)
                        {
                            SignAssemblyIn = 1m;
                        }
                        else
                        {
                            SignAssemblyOut = 1m;
                        }
                        break;
                    default:
                        throw new PXException();
                }
            }
        }

        protected static void UpdateHistoryField<FinHistoryField, TranHistoryField>(PXGraph graph, object data, decimal? value, bool IsFinField)
            where FinHistoryField : IBqlField
            where TranHistoryField : IBqlField
        {
            PXCache cache = graph.Caches[BqlCommand.GetItemType(typeof(FinHistoryField))];

            if (IsFinField)
            {
                decimal? oldvalue = (decimal?)cache.GetValue<FinHistoryField>(data);

                cache.SetValue<FinHistoryField>(data, (oldvalue ?? 0m) + (value ?? 0m));
            }
            else
            {
                decimal? oldvalue = (decimal?)cache.GetValue<TranHistoryField>(data);

                cache.SetValue<TranHistoryField>(data, (oldvalue ?? 0m) + (value ?? 0m));
            }
        }

        public static void UpdateCostHist(PXGraph graph, INHistBucket bucket, INTranCost tran, Int32? siteID, string PeriodID, bool FinFlag)
        {
            ItemCostHist hist = new ItemCostHist();
            hist.InventoryID = tran.InventoryID;
            hist.CostSiteID = tran.CostSiteID;
			hist.SiteID = siteID;
            hist.AccountID = tran.InvtAcctID;
            hist.SubID = tran.InvtSubID;
            hist.CostSubItemID = tran.CostSubItemID;
            hist.FinPeriodID = PeriodID;

            hist = (ItemCostHist)graph.Caches[typeof(ItemCostHist)].Insert(hist);

            UpdateHistoryField<ItemCostHist.finPtdCostReceived, ItemCostHist.tranPtdCostReceived>(graph, hist, tran.TranCost * bucket.SignReceived, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostIssued, ItemCostHist.tranPtdCostIssued>(graph, hist, tran.TranCost * bucket.SignIssued, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCOGS, ItemCostHist.tranPtdCOGS>(graph, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCOGSCredits, ItemCostHist.tranPtdCOGSCredits>(graph, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCOGSDropShips, ItemCostHist.tranPtdCOGSDropShips>(graph, hist, tran.TranCost * bucket.SignDropShip, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostTransferIn, ItemCostHist.tranPtdCostTransferIn>(graph, hist, tran.TranCost * bucket.SignTransferIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostTransferOut, ItemCostHist.tranPtdCostTransferOut>(graph, hist, tran.TranCost * bucket.SignTransferOut, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostAdjusted, ItemCostHist.tranPtdCostAdjusted>(graph, hist, tran.TranCost * bucket.SignAdjusted, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostAssemblyIn, ItemCostHist.tranPtdCostAssemblyIn>(graph, hist, tran.TranCost * bucket.SignAssemblyIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostAssemblyOut, ItemCostHist.tranPtdCostAssemblyOut>(graph, hist, tran.TranCost * bucket.SignAssemblyOut, FinFlag);

            UpdateHistoryField<ItemCostHist.finPtdQtyReceived, ItemCostHist.tranPtdQtyReceived>(graph, hist, tran.Qty * bucket.SignReceived, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyIssued, ItemCostHist.tranPtdQtyIssued>(graph, hist, tran.Qty * bucket.SignIssued, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtySales, ItemCostHist.tranPtdQtySales>(graph, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyCreditMemos, ItemCostHist.tranPtdQtyCreditMemos>(graph, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyDropShipSales, ItemCostHist.tranPtdQtyDropShipSales>(graph, hist, tran.Qty * bucket.SignDropShip, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyTransferIn, ItemCostHist.tranPtdQtyTransferIn>(graph, hist, tran.Qty * bucket.SignTransferIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyTransferOut, ItemCostHist.tranPtdQtyTransferOut>(graph, hist, tran.Qty * bucket.SignTransferOut, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyAdjusted, ItemCostHist.tranPtdQtyAdjusted>(graph, hist, tran.Qty * bucket.SignAdjusted, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyAssemblyIn, ItemCostHist.tranPtdQtyAssemblyIn>(graph, hist, tran.Qty * bucket.SignAssemblyIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyAssemblyOut, ItemCostHist.tranPtdQtyAssemblyOut>(graph, hist, tran.Qty * bucket.SignAssemblyOut, FinFlag);

            UpdateHistoryField<ItemCostHist.finPtdSales, ItemCostHist.tranPtdSales>(graph, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCreditMemos, ItemCostHist.tranPtdCreditMemos>(graph, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdDropShipSales, ItemCostHist.tranPtdDropShipSales>(graph, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCostHist.finYtdQty, ItemCostHist.tranYtdQty>(graph, hist, tran.Qty * bucket.SignYtd, FinFlag);
            UpdateHistoryField<ItemCostHist.finYtdCost, ItemCostHist.tranYtdCost>(graph, hist, tran.TranCost * bucket.SignYtd, FinFlag);
        }

        public static void UpdateCostHist(PXGraph graph, INTranCost costtran, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(costtran, intran);

			UpdateCostHist(graph, bucket, costtran, intran.SiteID, costtran.FinPeriodID, true);
            UpdateCostHist(graph, bucket, costtran, intran.SiteID, costtran.TranPeriodID, false);
        }

        protected virtual void UpdateCostHist(INTranCost costtran, INTran intran)
        {
            UpdateCostHist(this, costtran, intran);
        }

        protected virtual void UpdateSalesHist(INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag)
        {
            ItemSalesHist hist = new ItemSalesHist();
            hist.InventoryID = tran.InventoryID;
            hist.CostSiteID = tran.CostSiteID;
            hist.CostSubItemID = tran.CostSubItemID;
            hist.FinPeriodID = PeriodID;

            hist = itemsaleshist.Insert(hist);

            UpdateHistoryField<ItemSalesHist.finPtdCOGS, ItemSalesHist.tranPtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdCOGSCredits, ItemSalesHist.tranPtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdCOGSDropShips, ItemSalesHist.tranPtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finPtdQtySales, ItemSalesHist.tranPtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdQtyCreditMemos, ItemSalesHist.tranPtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdQtyDropShipSales, ItemSalesHist.tranPtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finPtdSales, ItemSalesHist.tranPtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdCreditMemos, ItemSalesHist.tranPtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdDropShipSales, ItemSalesHist.tranPtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finYtdCOGS, ItemSalesHist.tranYtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdCOGSCredits, ItemSalesHist.tranYtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdCOGSDropShips, ItemSalesHist.tranYtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finYtdQtySales, ItemSalesHist.tranYtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdQtyCreditMemos, ItemSalesHist.tranYtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdQtyDropShipSales, ItemSalesHist.tranYtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finYtdSales, ItemSalesHist.tranYtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdCreditMemos, ItemSalesHist.tranYtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdDropShipSales, ItemSalesHist.tranYtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);
        }

        protected virtual void UpdateSalesHist(INTranCost costtran, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(costtran, intran);

            UpdateSalesHist(bucket, costtran, costtran.FinPeriodID, true);
            UpdateSalesHist(bucket, costtran, costtran.TranPeriodID, false);
        }

        protected virtual void UpdateSalesHistD(INTran intran)
        {
            UpdateSalesHistD(this, intran);
        }
        public static void UpdateSalesHistD(PXGraph graph, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(intran);

            if (intran.TranDate == null || intran.BaseQty * bucket.SignSales <= 0 || intran.SubItemID == null) return;

            ItemSalesHistD hist = new ItemSalesHistD();
            hist.InventoryID = intran.InventoryID;
            hist.SiteID = intran.SiteID;
            hist.SubItemID = intran.SubItemID;
            hist.SDate = intran.TranDate;
            hist = (ItemSalesHistD)graph.Caches[typeof(ItemSalesHistD)].Insert(hist);

            DateTime date = (DateTime)intran.TranDate;
            hist.SYear = date.Year;
            hist.SMonth = date.Month;
            hist.SDay = date.Day;
            hist.SQuater = (date.Month + 2) / 3;
            hist.SDayOfWeek = (int)date.DayOfWeek;
            hist.QtyIssues += intran.BaseQty * bucket.SignSales;

            INItemSite itemsite = SelectItemSite(graph, intran.InventoryID, intran.SiteID);

            if (itemsite == null || itemsite.ReplenishmentPolicyID == null) return;

            INReplenishmentPolicy seasonality = INReplenishmentPolicy.PK.Find(graph, itemsite.ReplenishmentPolicyID);

            if (seasonality == null || seasonality.CalendarID == null) return;

            PXResult<CSCalendar, CSCalendarExceptions> result =
                (PXResult<CSCalendar, CSCalendarExceptions>)
                PXSelectJoin<CSCalendar,
                    LeftJoin<CSCalendarExceptions,
                    On<CSCalendarExceptions.calendarID, Equal<CSCalendar.calendarID>,
                    And<CSCalendarExceptions.date, Equal<Required<CSCalendarExceptions.date>>>>>,
                    Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>
                    .SelectWindowed(graph, 0, 1, date, seasonality.CalendarID);

            if (result != null)
            {
                CSCalendar calendar = result;
                CSCalendarExceptions exc = result;
                if (exc.Date != null)
                {
                    hist.DemandType1 = exc.WorkDay == true ? 1 : 0;
                    hist.DemandType2 = exc.WorkDay != true ? 1 : 0;
                }
                else
                {
                    hist.DemandType1 = calendar.IsWorkDay(date) ? 1 : 0;
                    hist.DemandType2 = calendar.IsWorkDay(date) ? 0 : 1;
                }
            }
        }

		protected virtual void UpdateCustSalesStats(INTran intran)
		{
			UpdateCustSalesStats(this, intran);
		}

		public static void UpdateCustSalesStats(PXGraph graph, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(intran);

			if (intran.TranDate == null || intran.BaseQty == 0 || intran.BAccountID == null || intran.SubItemID == null || intran.ARRefNbr == null)
				return;

			if (bucket.SignSales != 0)
				UpdateCustStats<ItemCustSalesStats,
					ItemCustSalesStats.lastDate, 
					ItemCustSalesStats.lastQty, 
					ItemCustSalesStats.lastUnitPrice>(graph, intran);
			else if(bucket.SignDropShip != 0)
				UpdateCustStats<ItemCustDropShipStats, 
					ItemCustDropShipStats.dropShipLastDate, 
					ItemCustDropShipStats.dropShipLastQty, 
					ItemCustDropShipStats.dropShipLastUnitPrice>(graph, intran);
		}

		private static void UpdateCustStats<TStatus, TLastDate, TLastQty, TLastUnitPrice>(PXGraph graph, INTran intran)
			where TStatus: INItemCustSalesStats, new()
			where TLastDate : IBqlField
			where TLastQty : IBqlField
			where TLastUnitPrice : IBqlField
		{
			TStatus stats = new TStatus();
			stats.InventoryID = intran.InventoryID;
			stats.SubItemID = intran.SubItemID;
			stats.SiteID = intran.SiteID;
			stats.BAccountID = intran.BAccountID;
			var cache = graph.Caches[typeof(TStatus)];
			stats = (TStatus)cache.Insert(stats);

			var lastDate = (DateTime?)cache.GetValue<TLastDate>(stats);
			if (lastDate == null || lastDate < intran.TranDate)
			{
				cache.SetValue<TLastDate>(stats, intran.TranDate);
				cache.SetValue<TLastQty>(stats, intran.BaseQty);
				//during release process intran.UnitPrice is recalculated for base uom and discarded after
				decimal? unitPrice = Math.Abs((decimal)intran.TranAmt - PXCurrencyAttribute.BaseRound(cache.Graph, (decimal)intran.BaseQty * (decimal)intran.UnitPrice)) < 0.00005m
					? intran.UnitPrice
					: (intran.TranAmt / intran.BaseQty);
				cache.SetValue<TLastUnitPrice>(stats, unitPrice);
			}
		}

		protected virtual void UpdateCustSalesHist(INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag, INTran intran)
        {
            if (intran.BAccountID == null) return;

            ItemCustSalesHist hist = new ItemCustSalesHist();
            hist.InventoryID = tran.InventoryID;
            hist.CostSiteID = tran.CostSiteID;
            hist.CostSubItemID = tran.CostSubItemID;
            hist.FinPeriodID = PeriodID;
            hist.BAccountID = intran.BAccountID;

            hist = itemcustsaleshist.Insert(hist);

            UpdateHistoryField<ItemCustSalesHist.finPtdCOGS, ItemCustSalesHist.tranPtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdCOGSCredits, ItemCustSalesHist.tranPtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdCOGSDropShips, ItemCustSalesHist.tranPtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finPtdQtySales, ItemCustSalesHist.tranPtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdQtyCreditMemos, ItemCustSalesHist.tranPtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdQtyDropShipSales, ItemCustSalesHist.tranPtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finPtdSales, ItemCustSalesHist.tranPtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdCreditMemos, ItemCustSalesHist.tranPtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdDropShipSales, ItemCustSalesHist.tranPtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finYtdCOGS, ItemCustSalesHist.tranYtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdCOGSCredits, ItemCustSalesHist.tranYtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdCOGSDropShips, ItemCustSalesHist.tranYtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finYtdQtySales, ItemCustSalesHist.tranYtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdQtyCreditMemos, ItemCustSalesHist.tranYtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdQtyDropShipSales, ItemCustSalesHist.tranYtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finYtdSales, ItemCustSalesHist.tranYtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdCreditMemos, ItemCustSalesHist.tranYtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdDropShipSales, ItemCustSalesHist.tranYtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);
        }

        protected virtual void UpdateCustSalesHist(INTranCost costtran, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(costtran, intran);
            UpdateCustSalesHist(bucket, costtran, costtran.FinPeriodID, true, intran);
            UpdateCustSalesHist(bucket, costtran, costtran.TranPeriodID, false, intran);
        }

        protected static void UpdateSiteHist(PXGraph graph, INHistBucket bucket, INTranSplit tran, string PeriodID, bool FinFlag)
        {
            ItemSiteHist hist = new ItemSiteHist();
            hist.InventoryID = tran.InventoryID;
            hist.SiteID = tran.SiteID;
            hist.SubItemID = tran.SubItemID;
            hist.LocationID = tran.LocationID;
            hist.FinPeriodID = PeriodID;

            hist = (ItemSiteHist)graph.Caches[typeof(ItemSiteHist)].Insert(hist);

            UpdateHistoryField<ItemSiteHist.finPtdQtyReceived, ItemSiteHist.tranPtdQtyReceived>(graph, hist, tran.BaseQty * bucket.SignReceived, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyIssued, ItemSiteHist.tranPtdQtyIssued>(graph, hist, tran.BaseQty * bucket.SignIssued, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtySales, ItemSiteHist.tranPtdQtySales>(graph, hist, tran.BaseQty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyCreditMemos, ItemSiteHist.tranPtdQtyCreditMemos>(graph, hist, tran.BaseQty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyDropShipSales, ItemSiteHist.tranPtdQtyDropShipSales>(graph, hist, tran.BaseQty * bucket.SignDropShip, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyTransferIn, ItemSiteHist.tranPtdQtyTransferIn>(graph, hist, tran.BaseQty * bucket.SignTransferIn, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyTransferOut, ItemSiteHist.tranPtdQtyTransferOut>(graph, hist, tran.BaseQty * bucket.SignTransferOut, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyAdjusted, ItemSiteHist.tranPtdQtyAdjusted>(graph, hist, tran.BaseQty * bucket.SignAdjusted, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyAssemblyIn, ItemSiteHist.tranPtdQtyAssemblyIn>(graph, hist, tran.BaseQty * bucket.SignAssemblyIn, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyAssemblyOut, ItemSiteHist.tranPtdQtyAssemblyOut>(graph, hist, tran.BaseQty * bucket.SignAssemblyOut, FinFlag);
            UpdateHistoryField<ItemSiteHist.finYtdQty, ItemSiteHist.tranYtdQty>(graph, hist, tran.BaseQty * bucket.SignYtd, FinFlag);
        }

	    protected virtual void UpdateSiteHistDay(INTran tran, INTranSplit split)
	    {
		    UpdateSiteHistDay(this, tran, split);

	    }

	    public static void UpdateSiteHistDay(PXGraph graph, INTran tran, INTranSplit split)
		{
			//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
			INHistBucket bucket = new INHistBucket(split);

			ItemSiteHistDay hist = new ItemSiteHistDay();
			hist.InventoryID = split.InventoryID;
			hist.SiteID = split.SiteID;
			hist.SubItemID = split.SubItemID;
			hist.LocationID = split.LocationID;
			DateTime date = (DateTime)split.TranDate;
			hist.SDate = date;

			hist = (ItemSiteHistDay)graph.Caches[typeof(ItemSiteHistDay)].Insert(hist);

			UpdateHistoryField<ItemSiteHistDay.qtyReceived, ItemSiteHistDay.qtyReceived>(graph, hist, split.BaseQty * bucket.SignReceived, true);
			UpdateHistoryField<ItemSiteHistDay.qtyIssued, ItemSiteHistDay.qtyIssued>(graph, hist, split.BaseQty * bucket.SignIssued, true);
			UpdateHistoryField<ItemSiteHistDay.qtySales, ItemSiteHistDay.qtySales>(graph, hist, split.BaseQty * bucket.SignSales, true);
			UpdateHistoryField<ItemSiteHistDay.qtyCreditMemos, ItemSiteHistDay.qtyCreditMemos>(graph, hist, split.BaseQty * bucket.SignCreditMemos, true);
			UpdateHistoryField<ItemSiteHistDay.qtyDropShipSales, ItemSiteHistDay.qtyDropShipSales>(graph, hist, split.BaseQty * bucket.SignDropShip, true);
			UpdateHistoryField<ItemSiteHistDay.qtyTransferIn, ItemSiteHistDay.qtyTransferIn>(graph, hist, split.BaseQty * bucket.SignTransferIn, true);
			UpdateHistoryField<ItemSiteHistDay.qtyTransferOut, ItemSiteHistDay.qtyTransferOut>(graph, hist, split.BaseQty * bucket.SignTransferOut, true);
			UpdateHistoryField<ItemSiteHistDay.qtyAdjusted, ItemSiteHistDay.qtyAdjusted>(graph, hist, split.BaseQty * bucket.SignAdjusted, true);
			UpdateHistoryField<ItemSiteHistDay.qtyAssemblyIn, ItemSiteHistDay.qtyAssemblyIn>(graph, hist, split.BaseQty * bucket.SignAssemblyIn, true);
			UpdateHistoryField<ItemSiteHistDay.qtyAssemblyOut, ItemSiteHistDay.qtyAssemblyOut>(graph, hist, split.BaseQty * bucket.SignAssemblyOut, true);
			UpdateHistoryField<ItemSiteHistDay.qtyDebit, ItemSiteHistDay.qtyDebit>(graph, hist, bucket.SignYtd > 0 ? split.BaseQty : 0m, true);
			UpdateHistoryField<ItemSiteHistDay.qtyCredit, ItemSiteHistDay.qtyCredit>(graph, hist, bucket.SignYtd < 0 ? split.BaseQty : 0m, true);

			UpdateHistoryField<ItemSiteHistDay.endQty, ItemSiteHistDay.endQty>(graph, hist, split.BaseQty * bucket.SignYtd, true);
		}

		public static void UpdateSiteHist(PXGraph graph, INTran tran, INTranSplit split)
        {
            //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
            INHistBucket bucket = new INHistBucket(split);

            UpdateSiteHist(graph, bucket, split, tran.FinPeriodID, true);
            UpdateSiteHist(graph, bucket, split, tran.TranPeriodID, false);
        }

        protected virtual void UpdateSiteHist(INTran tran, INTranSplit split)
        {
            UpdateSiteHist(this, tran, split);
        }

        public static void UpdateSiteHistD(PXGraph graph, INTranSplit tran)
        {
            //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
            INHistBucket bucket = new INHistBucket(tran);

            ItemSiteHistD hist = new ItemSiteHistD();
            DateTime date = (DateTime)tran.TranDate;
            hist.InventoryID = tran.InventoryID;
            hist.SiteID = tran.SiteID;
            hist.SubItemID = tran.SubItemID;
            hist.SDate = date;

            if (tran.TranType == INTranType.Transfer && tran.InvtMult == -1 && tran.SiteID == tran.ToSiteID)
            {
                bucket.SignTransferIn = -1;
                bucket.SignTransferOut = 0;
            }


            hist = (ItemSiteHistD)graph.Caches[typeof(ItemSiteHistD)].Insert(hist);

            hist.SYear = date.Year;
            hist.SMonth = date.Month;
            hist.SDay = date.Day;
            hist.SQuater = (date.Month + 2) / 3;
            hist.SDayOfWeek = (int)date.DayOfWeek;

            UpdateHistoryField<ItemSiteHistD.qtyReceived, ItemSiteHistD.qtyReceived>(graph, hist, tran.BaseQty * bucket.SignReceived, true);
            UpdateHistoryField<ItemSiteHistD.qtyIssued, ItemSiteHistD.qtyIssued>(graph, hist, tran.BaseQty * bucket.SignIssued, true);
            UpdateHistoryField<ItemSiteHistD.qtySales, ItemSiteHistD.qtySales>(graph, hist, tran.BaseQty * bucket.SignSales, true);
            UpdateHistoryField<ItemSiteHistD.qtyCreditMemos, ItemSiteHistD.qtyCreditMemos>(graph, hist, tran.BaseQty * bucket.SignCreditMemos, true);
            UpdateHistoryField<ItemSiteHistD.qtyDropShipSales, ItemSiteHistD.qtyDropShipSales>(graph, hist, tran.BaseQty * bucket.SignDropShip, true);
            UpdateHistoryField<ItemSiteHistD.qtyTransferIn, ItemSiteHistD.qtyTransferIn>(graph, hist, tran.BaseQty * bucket.SignTransferIn, true);
            UpdateHistoryField<ItemSiteHistD.qtyTransferOut, ItemSiteHistD.qtyTransferOut>(graph, hist, tran.BaseQty * bucket.SignTransferOut, true);
            UpdateHistoryField<ItemSiteHistD.qtyAdjusted, ItemSiteHistD.qtyAdjusted>(graph, hist, tran.BaseQty * bucket.SignAdjusted, true);
            UpdateHistoryField<ItemSiteHistD.qtyAssemblyIn, ItemSiteHistD.qtyAssemblyIn>(graph, hist, tran.BaseQty * bucket.SignAssemblyIn, true);
            UpdateHistoryField<ItemSiteHistD.qtyAssemblyOut, ItemSiteHistD.qtyAssemblyOut>(graph, hist, tran.BaseQty * bucket.SignAssemblyOut, true);

        }

        protected virtual void UpdateSiteHistD(INTranSplit tran)
        {
            UpdateSiteHistD(this, tran);
        }


        public int? GetAcctID<Field>(string AcctDefault, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetAcctID<Field>(this, AcctDefault, item, site, postclass);
        }

        public static int? GetAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            switch (AcctDefault)
            {
                case INAcctSubDefault.MaskItem:
                default:
                    {
                        PXCache cache = graph.Caches[typeof(InventoryItem)];
                        try
                        {
                            return (int)cache.GetValue<Field>(item);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<InventoryItem.inventoryCD>(item);
                            if (item.StkItem == true)
                            {
                                throw new PXMaskArgumentException(Messages.MaskItem, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                            }
                            throw new PXMaskArgumentException(Messages.MaskItem, GetSubstFieldDesr<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskSite:
                    {
                        PXCache cache = graph.Caches[typeof(INSite)];
                        try
                        {
                            return (int)cache.GetValue<Field>(site);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<INSite.siteCD>(site);
                            throw new PXMaskArgumentException(Messages.MaskSite, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskClass:
                    {
                        PXCache cache = graph.Caches[typeof(INPostClass)];
                        try
                        {
                            return (int)cache.GetValue<Field>(postclass);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<INPostClass.postClassID>(postclass);
                            throw new PXMaskArgumentException(Messages.MaskClass, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
            }
        }

        public static string GetSubstFieldDesr<Field>(PXCache cache)
            where Field : IBqlField
        {
            if (typeof(Field) == typeof(INPostClass.invtAcctID))
            {
                return PXUIFieldAttribute.GetDisplayName<NonStockItem.invtAcctID>(cache);
            }
            if (typeof(Field) == typeof(INPostClass.cOGSAcctID))
            {
                return PXUIFieldAttribute.GetDisplayName<NonStockItem.cOGSAcctID>(cache);
            }
            return PXUIFieldAttribute.GetDisplayName<Field>(cache);
        }

        public int? GetSubID<Field>(string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetSubID<Field>(this, AcctDefault, SubMask, item, site, postclass);
        }

        public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetSubID<Field>(graph, AcctDefault, SubMask, item, site, postclass, null);
        }

        public int? GetSubID<Field>(string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
            where Field : IBqlField
        {
            return GetSubID<Field>(this, AcctDefault, SubMask, item, site, postclass);
        }

        public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
            where Field : IBqlField
        {
            if (typeof(Field) == typeof(INPostClass.cOGSSubID) && tran != null && postclass.COGSSubFromSales == true)
            {
                PXCache cache = graph.Caches[typeof(INTran)];

                object tran_SubID = cache.GetValueExt<INTran.subID>(tran);
                object value = (tran_SubID is PXFieldState) ? ((PXFieldState)tran_SubID).Value : tran_SubID;

                cache.RaiseFieldUpdating<Field>(tran, ref value);
                return (int?)value;
            }
            else
            {
				int? item_SubID = null;
				int? site_SubID = null;
				int? class_SubID = null;

				if (typeof(Field) == typeof(INPostClass.cOGSSubID) && postclass.COGSSubFromSales == true)
				{
					item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue<InventoryItem.salesSubID>(item);
					site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue<INSite.salesSubID>(site);
					class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue<INPostClass.salesSubID>(postclass);
				}
				else
				{
					item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue<Field>(item);
					site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue<Field>(site);
					class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue<Field>(postclass);
				}

				object value = null;

                try
                {
                    if (item.StkItem == true && typeof(Field) == typeof(INPostClass.invtSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.invtSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.invtSubID), typeof(INSite.invtSubID), typeof(INPostClass.invtSubID) });
                    if (item.StkItem != true && typeof(Field) == typeof(INPostClass.invtSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.invtSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(NonStockItem.invtSubID), typeof(INSite.invtSubID), typeof(INPostClass.invtSubID) });
                    if (item.StkItem == true && typeof(Field) == typeof(INPostClass.cOGSSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.cOGSSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.cOGSSubID), typeof(INSite.cOGSSubID), typeof(INPostClass.cOGSSubID) });
                    if (item.StkItem != true && typeof(Field) == typeof(INPostClass.cOGSSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.cOGSSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(NonStockItem.cOGSSubID), typeof(INSite.cOGSSubID), typeof(INPostClass.cOGSSubID) });
                    if (typeof(Field) == typeof(INPostClass.salesSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.salesSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.salesSubID), typeof(INSite.salesSubID), typeof(INPostClass.salesSubID) });
                    if (typeof(Field) == typeof(INPostClass.stdCstVarSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.stdCstVarSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.stdCstVarSubID), typeof(INSite.stdCstVarSubID), typeof(INPostClass.stdCstVarSubID) });
                    if (typeof(Field) == typeof(INPostClass.stdCstRevSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.stdCstRevSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.stdCstRevSubID), typeof(INSite.stdCstRevSubID), typeof(INPostClass.stdCstRevSubID) });
                    if (typeof(Field) == typeof(INPostClass.pOAccrualSubID))
                        throw new NotImplementedException();
                    if (typeof(Field) == typeof(INPostClass.pPVSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.pPVSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.pPVSubID), typeof(INSite.pPVSubID), typeof(INPostClass.pPVSubID) });
                    if (typeof(Field) == typeof(INPostClass.lCVarianceSubID))
                        value = SubAccountMaskAttribute.MakeSub<INPostClass.lCVarianceSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.lCVarianceSubID), typeof(INSite.lCVarianceSubID), typeof(INPostClass.lCVarianceSubID) });
                }
                catch (PXMaskArgumentException ex)
                {
                    object keyval;
                    switch (ex.SourceIdx)
                    {
                        case 0:
                        default:
                            keyval = graph.Caches[typeof(InventoryItem)].GetStateExt<InventoryItem.inventoryCD>(item);
                            break;
                        case 1:
                            keyval = graph.Caches[typeof(INSite)].GetStateExt<INSite.siteCD>(site);
                            break;
                        case 2:
                            keyval = graph.Caches[typeof(INPostClass)].GetStateExt<INPostClass.postClassID>(postclass);
                            break;
                    }
                    throw new PXMaskArgumentException(ex, keyval);
                }

                switch (AcctDefault)
                {
                    case INAcctSubDefault.MaskItem:
                    default:
                        RaiseFieldUpdating<Field>(graph.Caches[typeof(InventoryItem)], item, ref value);
                        break;
                    case INAcctSubDefault.MaskSite:
                        RaiseFieldUpdating<Field>(graph.Caches[typeof(INSite)], site, ref value);
                        break;
                    case INAcctSubDefault.MaskClass:
                        RaiseFieldUpdating<Field>(graph.Caches[typeof(INPostClass)], postclass, ref value);
                        break;
                }
                return (int?)value;
            }
        }

        public static int? GetPOAccrualAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem item, INSite site, INPostClass postclass, Vendor vendor)
            where Field : IBqlField
        {
            switch (AcctDefault)
            {
                case INAcctSubDefault.MaskItem:
                default:
                    {
                        PXCache cache = graph.Caches[typeof(InventoryItem)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(item);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<InventoryItem.inventoryCD>(item);
                            throw new PXMaskArgumentException(Messages.MaskItem, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskSite:
                    {
                        PXCache cache = graph.Caches[typeof(INSite)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(site);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<INSite.siteCD>(site);
                            throw new PXMaskArgumentException(Messages.MaskSite, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskClass:
                    {
                        PXCache cache = graph.Caches[typeof(INPostClass)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(postclass);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<INPostClass.postClassID>(postclass);
                            throw new PXMaskArgumentException(Messages.MaskClass, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskVendor:
                    {
                        PXCache cache = graph.Caches[typeof(Vendor)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(vendor);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<Vendor.bAccountID>(vendor);
                            throw new PXMaskArgumentException(Messages.MaskVendor, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
            }
        }

        public static int? GetPOAccrualSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, Vendor vendor)
            where Field : IBqlField
        {
            int? item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue<Field>(item);
            int? site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue<Field>(site);
            int? class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue<Field>(postclass);
            int? vendor_SubID = (int?)graph.Caches[typeof(Vendor)].GetValue<Field>(vendor);

            object value = null;

            try
            {
                value = POAccrualSubAccountMaskAttribute.MakeSub<INPostClass.pOAccrualSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID, vendor_SubID }, new Type[] { typeof(InventoryItem.pOAccrualSubID), typeof(INSite.pOAccrualSubID), typeof(INPostClass.pOAccrualSubID), typeof(Vendor.pOAccrualSubID) });
            }
            catch (PXMaskArgumentException ex)
            {
                object keyval;
                switch (ex.SourceIdx)
                {
                    case 0:
                    default:
                        keyval = graph.Caches[typeof(InventoryItem)].GetStateExt<InventoryItem.inventoryCD>(item);
                        break;
                    case 1:
                        keyval = graph.Caches[typeof(INSite)].GetStateExt<INSite.siteCD>(site);
                        break;
                    case 2:
                        keyval = graph.Caches[typeof(INPostClass)].GetStateExt<INPostClass.postClassID>(postclass);
                        break;
                    case 3:
                        keyval = graph.Caches[typeof(Vendor)].GetStateExt<Vendor.bAccountID>(vendor);
                        break;
                }
                throw new PXMaskArgumentException(ex, keyval);
            }

            switch (AcctDefault)
            {
                case INAcctSubDefault.MaskItem:
                default:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(InventoryItem)], item, ref value);
                    break;
                case INAcctSubDefault.MaskSite:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(INSite)], site, ref value);
                    break;
                case INAcctSubDefault.MaskClass:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(INPostClass)], postclass, ref value);
                    break;
                case INAcctSubDefault.MaskVendor:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(Vendor)], vendor, ref value);
                    break;
            }
            return (int?)value;
        }

		public virtual int? GetReasonCodeSubID(ReasonCode tranreasoncode, ReasonCode defreasoncode, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
		{
			ReasonCode reasoncode = (tranreasoncode.AccountID == null) ? defreasoncode : tranreasoncode;
			return GetReasonCodeSubID(this, reasoncode, item, site, postclass, typeof(INPostClass.reasonCodeSubID));
		}

		public virtual int? GetReasonCodeSubID(ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
		{
			return GetReasonCodeSubID(this, reasoncode, item, site, postclass);
        }

		public static int? GetReasonCodeSubID(PXGraph graph, ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass)
		{
			return (reasoncode.AccountID == null) ? null
				: GetReasonCodeSubID(graph, reasoncode, item, site, postclass, typeof(INPostClass.reasonCodeSubID));
		}

		private static int? GetReasonCodeSubID(PXGraph graph, ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass, Type fieldType)
            {
                int? reasoncode_SubID = (int?)graph.Caches[typeof(ReasonCode)].GetValue<ReasonCode.subID>(reasoncode);
			int? item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue(item, fieldType.Name);
			int? site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue(site, fieldType.Name);
			int? class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue(postclass, fieldType.Name);

                object value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>(graph, reasoncode.SubMask,
                    new object[] { reasoncode_SubID, item_SubID, site_SubID, class_SubID },
                    new Type[] { typeof(ReasonCode.subID), typeof(InventoryItem.reasonCodeSubID), typeof(INSite.reasonCodeSubID), typeof(INPostClass.reasonCodeSubID) });

                RaiseFieldUpdating<ReasonCode.subID>(graph.Caches[typeof(ReasonCode)], reasoncode, ref value);
                return (int?)value;
            }

		public virtual int? GetCogsAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.cOGSAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetCogsSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.cOGSSubID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetInvtAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.invtAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetInvtSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.invtSubID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetSalesAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.salesAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetSalesSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.salesSubID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetStdCostVarAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.stdCstVarAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetStdCostVarSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.stdCstVarSubID>(this, item, site, postclass, useTran ? tran : null);
        }

        public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetAccountDefaults<Field>(graph, item, site, postclass, null);
        }

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass, INTran tran, out bool isControlAccount)
			where Field : IBqlField
		{
			isControlAccount = false;
			return GetAccountDefaults<Field>(graph, item, site, postclass, tran);
		}

		public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
			where Field : IBqlField
		{
            if (typeof(Field) == typeof(INPostClass.invtAcctID))
                return GetAcctID<Field>(graph, item.StkItem != true && postclass.InvtAcctDefault == INAcctSubDefault.MaskSite ? INAcctSubDefault.MaskItem : postclass.InvtAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.invtSubID))
                return GetSubID<Field>(graph, postclass.InvtAcctDefault, postclass.InvtSubMask, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.cOGSAcctID))
            {
                return GetAcctID<Field>(graph,
                    item.StkItem != true && postclass.COGSAcctDefault == INAcctSubDefault.MaskSite
                        ? INAcctSubDefault.MaskItem
                        : postclass.COGSAcctDefault, item, site, postclass);
            }
            if (typeof(Field) == typeof(INPostClass.cOGSSubID))
            {
                return GetSubID<Field>(graph, postclass.COGSAcctDefault, postclass.COGSSubMask, item, site, postclass,
                    tran);
            }
            if (typeof(Field) == typeof(INPostClass.salesAcctID))
                return GetAcctID<Field>(graph, postclass.SalesAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.salesSubID))
                return GetSubID<Field>(graph, postclass.SalesAcctDefault, postclass.SalesSubMask, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstVarAcctID))
                return GetAcctID<Field>(graph, postclass.StdCstVarAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstVarSubID))
                return GetSubID<Field>(graph, postclass.StdCstVarAcctDefault, postclass.StdCstVarSubMask, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstRevAcctID))
                return GetAcctID<Field>(graph, postclass.StdCstRevAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstRevSubID))
                return GetSubID<Field>(graph, postclass.StdCstRevAcctDefault, postclass.StdCstRevSubMask, item, site, postclass);

            throw new PXException();
        }

        public static INItemSite SelectItemSite(PXGraph graph, int? InventoryID, int? SiteID)
        {
            INItemSite itemsite = new INItemSite();
            itemsite.InventoryID = InventoryID;
            itemsite.SiteID = SiteID;
            itemsite = (INItemSite)graph.Caches<INItemSite>().Locate(itemsite);

            if (itemsite == null)
            {
                itemsite = INItemSite.PK.Find(graph, InventoryID, SiteID);
            }

            return itemsite;
        }

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
        public virtual void INItemPlan_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
        }

        public virtual void UpdateSOTransferPlans(long? oldPlanID, long? newPlandID)
        {
            foreach (INItemPlan itemPlan in supplyplans.Select(oldPlanID))
            {
                INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(itemPlan);
				INPlanType demand_plantype = INPlanType.PK.Find(this, demand_plan.PlanType);

                //avoid ReadItem()
                initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);

                demand_plan.SupplyPlanID = newPlandID;

                if (demand_plantype.ReplanOnEvent == INPlanConstants.Plan95)
                {
                    demand_plan.PlanType = demand_plantype.ReplanOnEvent;
                    initemplan.Cache.Update(demand_plan);
                }
                else
                {
                    initemplan.Cache.Update(demand_plan);
                }
            }
        }

        public virtual INTransitLine GetCachedTransitLine(int? costsiteid)
        {
            foreach (INTransitLine itertl in intransitline.Cache.Cached)
            {
                if (itertl.CostSiteID == costsiteid)
                {
                    return itertl;
                }
            }
            return null;
        }

        public virtual void UpdateTransitPlans()
        {
            //only for transfer receipts
            if (inregister.Current.DocType != INDocType.Receipt) 
                return;

            List<INTransitLine> processed = new List<INTransitLine>();
            List<INItemPlan> newplans = new List<INItemPlan>();

            foreach (TransitLotSerialStatus status in transitlotnumberedstatus.Cache.Inserted)
            {
                if (status.QtyOnHand == 0m)
                    continue;
                INTransitLine tl = processed.Find(x => x.CostSiteID == status.LocationID);

                if (tl == null)
                {
                    tl = GetCachedTransitLine(status.LocationID);
                    processed.Add(tl);
                }

                INItemPlan newplan = null;
				List<INItemPlan> oldPlans = PXSelect<INItemPlan,
                    Where<INItemPlan.refNoteID, Equal<Current<INTransitLine.noteID>>,
                        And<INItemPlan.inventoryID, Equal<Current<TransitLotSerialStatus.inventoryID>>,
                        And<INItemPlan.subItemID, Equal<Current<TransitLotSerialStatus.subItemID>>,
						And<INItemPlan.lotSerialNbr, Equal<Current<TransitLotSerialStatus.lotSerialNbr>>>>>>>
					.SelectMultiBound(this, new object[] { tl, status })
					.RowCast<INItemPlan>()
					.ToList();
				foreach (INItemPlan oldplan in oldPlans)
                {
                    //avoid ReadItem()
                    initemplan.Cache.SetStatus(oldplan, PXEntryStatus.Notchanged);

                    if (newplan == null)
                    {
                        newplan = (INItemPlan)initemplan.Cache.CreateCopy(oldplan);
                        initemplan.Delete(oldplan);
                        newplan.PlanQty += status.QtyOnHand;
                        newplan.PlanID = null;
                        newplan = initemplan.Insert(newplan);
                    }
                    else
                    {
                        //case when we are merging plans
                        newplan.PlanQty += oldplan.PlanQty;

                        initemplan.Delete(oldplan);

						newplan = initemplan.Update(newplan);
                    }
                    }

				if (newplan?.PlanQty <= 0m)
				{
                        initemplan.Delete(newplan);
					newplan = null;
				}

				foreach (INItemPlan oldPlan in oldPlans)
				{
					UpdateSOTransferPlans(oldPlan.PlanID, newplan?.PlanID);
                }
            }

            foreach (TransitLocationStatus status in transitlocationstatus.Cache.Inserted)
            {
                INTransitLine tl = processed.Find(x => x.CostSiteID == status.LocationID);
                if (tl!=null || status.QtyOnHand == 0m)
                    continue;

                tl = GetCachedTransitLine(status.LocationID);
                
                INItemPlan newplan = null;
				List<INItemPlan> oldPlans = PXSelect<INItemPlan,
                    Where<INItemPlan.refNoteID, Equal<Current<INTransitLine.noteID>>,
                        And<INItemPlan.inventoryID, Equal<Current<TransitLocationStatus.inventoryID>>,
						And<INItemPlan.subItemID, Equal<Current<TransitLocationStatus.subItemID>>>>>>
						.SelectMultiBound(this, new object[] { tl, status })
						.RowCast<INItemPlan>()
						.ToList();
				foreach (INItemPlan oldplan in oldPlans)
                {
                    if (newplan == null)
                    {
                        newplan = (INItemPlan)initemplan.Cache.CreateCopy(oldplan);
                        initemplan.Delete(oldplan);
                        newplan.PlanQty += status.QtyOnHand;
                        newplan.PlanID = null;
                        newplan = initemplan.Insert(newplan);
                    }
                    else
                    {
                        //case when we are merging plans
                        newplan.PlanQty += oldplan.PlanQty;

                        initemplan.Delete(oldplan);

						newplan = initemplan.Update(newplan);
                    }
                    }

				if (newplan?.PlanQty <= 0m)
				{
                        initemplan.Delete(newplan);
					newplan = null;
				}

				foreach (INItemPlan oldPlan in oldPlans)
				{
					UpdateSOTransferPlans(oldPlan.PlanID, newplan?.PlanID);
                }
            }
        }

        public virtual void UpdateItemSite(INTran tran, InventoryItem item, INSite site, ReasonCode reasoncode, INPostClass postclass)
        {
            if (item.StkItem == true)
            {
                INItemSite itemsite = SelectItemSite(this, tran.InventoryID, tran.SiteID);

                if (itemsite == null)
                {
                    itemsite = new INItemSite();
                    itemsite.InventoryID = tran.InventoryID;
                    itemsite.SiteID = tran.SiteID;
                    INItemSiteMaint.DefaultItemSiteByItem(this, itemsite, item, site, postclass);
                    itemsite = initemsite.Insert(itemsite);
                }

                if (itemsite.InvtAcctID == null)
                {
                    INItemSiteMaint.DefaultInvtAcctSub(this, itemsite, item, site, postclass);
                }

                if (tran.InvtAcctID == null)
                {
                    tran.InvtAcctID = itemsite.InvtAcctID;
                    tran.InvtSubID = itemsite.InvtSubID;
                }
            }
            else
            {
                switch (tran.TranType)
                {
                    case INTranType.Receipt:
                    case INTranType.Issue:
                        if (tran.InvtAcctID == null)
                        {
                            tran.InvtAcctID = GetCogsAcctID(item, null, postclass, tran, false);
                            tran.InvtSubID = GetCogsSubID(item, null, postclass, tran, false);
                        }
                        break;
                    case INTranType.Invoice:
                    case INTranType.DebitMemo:
                    case INTranType.CreditMemo:
                    case INTranType.Assembly:
                    case INTranType.Disassembly:
                        if (tran.InvtAcctID == null)
                        {
                            tran.InvtAcctID = GetInvtAcctID(item, null, postclass, tran, false);
                            tran.InvtSubID = GetInvtSubID(item, null, postclass, tran, false);
                        }
                        break;
					case INTranType.Adjustment:
						if (tran.InvtAcctID == null)
						{
							tran.InvtAcctID = GetInvtAcctID(item, null, postclass, tran, false);
							tran.InvtSubID = GetInvtSubID(item, null, postclass, tran, false);
						}
						break;
                    default:
                        throw new PXException(Messages.TranType_Invalid);
                }
            }

            switch (tran.TranType)
            {
                case INTranType.Receipt:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = reasoncode.AccountID ?? ReceiptReasonCode.AccountID;
                        tran.SubID = GetReasonCodeSubID(reasoncode, ReceiptReasonCode, item, site, postclass, tran);
                        tran.ReasonCode = tran.ReasonCode ?? ReceiptReasonCode.ReasonCodeID;
                    }
                    if (tran.COGSAcctID != null)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                case INTranType.Issue:
                case INTranType.Return:
					if (tran.POReceiptType == PO.POReceiptType.POReturn && reasoncode.Usage.IsIn(ReasonCodeUsages.VendorReturn, ReasonCodeUsages.Issue)
						&& (tran.AcctID != null || tran.COGSAcctID != null))
					{
						break;	// preserve PO Accrual Account for PO Returns
					}
                    if (tran.AcctID != null)
                    {
                        tran.AcctID = null;
                        tran.SubID = null;
                    }
                    if (tran.COGSAcctID == null)
                    {
                        //some crazy guys manage to setup ordertype so that it will create return in inventory and will specify non-inventory reason code
						if ((reasoncode.Usage == ReasonCodeUsages.Issue || (string.IsNullOrEmpty(reasoncode.Usage) && inregister.Current.OrigModule != GL.BatchModule.SO)))
                        {
                            tran.COGSAcctID = reasoncode.AccountID ?? IssuesReasonCode.AccountID;
                            tran.COGSSubID = GetReasonCodeSubID(reasoncode, IssuesReasonCode, item, site, postclass, tran);
                            tran.ReasonCode = tran.ReasonCode ?? IssuesReasonCode.ReasonCodeID;
                        }
                        else
                        {
                            tran.COGSAcctID = GetCogsAcctID(item, site, postclass, tran, false);
                            tran.COGSSubID = GetCogsSubID(item, site, postclass, tran, false);
                        }
                    }
                    break;
                case INTranType.Invoice:
                case INTranType.DebitMemo:
                case INTranType.CreditMemo:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = GetSalesAcctID(item, site, postclass, tran, false);
                        tran.SubID = GetSalesSubID(item, site, postclass, tran, false);
                    }
                    if (tran.COGSAcctID == null)
                    {
                        if (reasoncode.Usage == ReasonCodeUsages.Issue)
                        {
                            tran.COGSAcctID = reasoncode.AccountID;
                            tran.COGSSubID = GetReasonCodeSubID(reasoncode, item, site, postclass, tran);
                        }
                        else
                        {
                            tran.COGSAcctID = GetCogsAcctID(item, site, postclass, tran, true);
                            tran.COGSSubID = GetCogsSubID(item, site, postclass, tran, (tran.InvtMult != 0));
                        }
                    }
                    break;
                case INTranType.Adjustment:
                case INTranType.StandardCostAdjustment:
                case INTranType.NegativeCostAdjustment:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = reasoncode.AccountID ?? AdjustmentReasonCode.AccountID;
                        tran.SubID = GetReasonCodeSubID(reasoncode, AdjustmentReasonCode, item, site, postclass, tran);
                        tran.ReasonCode = tran.ReasonCode ?? AdjustmentReasonCode.ReasonCodeID;
                    }
                    if (tran.COGSAcctID == null && tran.InvtMult == (short)0)
                    {
                        if (item.ValMethod == INValMethod.Standard)
                        {
                            tran.COGSAcctID = GetStdCostVarAcctID(item, site, postclass, tran, false);
                            tran.COGSSubID = GetStdCostVarSubID(item, site, postclass, tran, false);
                        }
                        else
                        {
                            tran.COGSAcctID = GetCogsAcctID(item, site, postclass, tran, false);
                            tran.COGSSubID = GetCogsSubID(item, site, postclass, tran, true);
                        }
                    }
                    if (tran.COGSAcctID != null && tran.InvtMult == (short)1)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                case INTranType.Transfer:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = INTransitAcctID;
                        tran.SubID = INTransitSubID;
                        tran.ReclassificationProhibited = true;
                    }
                    if (tran.COGSAcctID != null)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                case INTranType.Assembly:
                case INTranType.Disassembly:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = INProgressAcctID;
                        tran.SubID = INProgressSubID;
                        tran.ReclassificationProhibited = true;
                    }
                    if (tran.COGSAcctID != null)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                default:
                    throw new PXException(Messages.TranType_Invalid);
            }
        }

        private void SegregateBatch(JournalEntry je, int? branchID, DateTime? docDate, string finPeriodID, string description)
        {
            je.created.Consolidate = je.glsetup.Current.ConsolidatedPosting ?? false;
            je.Segregate(BatchModule.IN, branchID, this.BaseCuryID, docDate, finPeriodID, description, null, null, null);
        }

        public virtual void WriteGLSales(JournalEntry je, INTran intran)
        {
            if (UpdateGL && intran.SalesMult != null && string.IsNullOrEmpty(intran.SOOrderNbr) && string.IsNullOrEmpty(intran.ARRefNbr))
            {
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = this.SummPost;
                    tran.BranchID = intran.BranchID;
                    tran.AccountID = ARClearingAcctID;
                    tran.SubID = ARClearingSubID;

                    tran.CuryDebitAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
                    tran.DebitAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
                    tran.CuryCreditAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
                    tran.CreditAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;

                    tran.TranType = intran.TranType;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.RefNbr = intran.RefNbr;
                    tran.InventoryID = intran.InventoryID;
                    tran.Qty = (intran.SalesMult == (short)1) ? intran.Qty : -intran.Qty;
                    tran.UOM = intran.UOM;
                    tran.TranDesc = intran.TranDesc;
                    tran.TranDate = intran.TranDate;
                    tran.TranPeriodID = intran.TranPeriodID;
                    tran.FinPeriodID = intran.FinPeriodID;
                    tran.ProjectID = intran.ProjectID;
                    tran.TaskID = intran.TaskID;
					tran.CostCodeID = intran.CostCodeID;
                    tran.Released = true;

                    je.GLTranModuleBatNbr.Insert(tran);
                }

                {
                    GLTran tran = new GLTran();
                    tran.SummPost = this.SummPost;
                    tran.BranchID = intran.BranchID;
                    tran.AccountID = intran.AcctID;
                    tran.SubID = GetValueInt<INTran.subID>(je, intran);

                    tran.CuryDebitAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
                    tran.DebitAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
                    tran.CuryCreditAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
                    tran.CreditAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;

                    tran.TranType = intran.TranType;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.RefNbr = intran.RefNbr;
                    tran.InventoryID = intran.InventoryID;
                    tran.Qty = (intran.SalesMult == (short)1) ? -intran.Qty : intran.Qty;
                    tran.UOM = intran.UOM;
                    tran.TranDesc = intran.TranDesc;
                    tran.TranDate = intran.TranDate;
                    tran.TranPeriodID = intran.TranPeriodID;
                    tran.FinPeriodID = intran.FinPeriodID;
                    tran.ProjectID = intran.ProjectID;
                    tran.TaskID = intran.TaskID;
					tran.CostCodeID = intran.CostCodeID;
                    tran.Released = true;

                    je.GLTranModuleBatNbr.Insert(tran);
                }
            }
        }

        public int? GetValueInt<SourceField>(PXGraph target, object item)
            where SourceField : IBqlField
        {
            PXCache source = this.Caches[BqlCommand.GetItemType(typeof(SourceField))];
            PXCache dest = target.Caches[BqlCommand.GetItemType(typeof(SourceField))];

            object value = source.GetValueExt<SourceField>(item);
            if (value is PXFieldState)
            {
                value = ((PXFieldState)value).Value;
            }

            if (value != null)
            {
                dest.RaiseFieldUpdating<SourceField>(item, ref value);
            }

            return (int?)value;
        }

        public static void RaiseFieldUpdating<Field>(PXCache cache, object item, ref object value)
            where Field : IBqlField
        {
            try
            {
                cache.RaiseFieldUpdating<Field>(item, ref value);
            }
            catch (PXSetPropertyException ex)
            {
                string fieldname = typeof(Field).Name;
                string itemname = PXUIFieldAttribute.GetItemName(cache);
                string dispname = PXUIFieldAttribute.GetDisplayName(cache, fieldname);
                string errortext = ex.Message;

                if (dispname != null && fieldname != dispname)
                {
                    int fid = errortext.IndexOf(fieldname, StringComparison.OrdinalIgnoreCase);
                    if (fid >= 0)
                    {
                        errortext = errortext.Remove(fid, fieldname.Length).Insert(fid, dispname);
                    }
                }
                else
                {
                    dispname = fieldname;
                }

                dispname = string.Format("{0} {1}", itemname, dispname);

                throw new PXSetPropertyException(ErrorMessages.ValueDoesntExist, dispname, value);
            }
        }

        public virtual void UpdateARTranCost(INTran tran)
        {
            UpdateARTranCost(tran, tran.TranCost);
        }

        public virtual void UpdateARTranCost(INTran tran, decimal? TranCost)
        {
            if (tran.ARRefNbr != null)
            {
                ARTranUpdate artran = new ARTranUpdate();
                artran.TranType = tran.ARDocType;
                artran.RefNbr = tran.ARRefNbr;
                artran.LineNbr = tran.ARLineNbr;

                artran = this.artranupdate.Insert(artran);

				if (tran.TranType == INTranType.TranTypeFromInvoiceType(tran.ARDocType, -1m))
				{
					// it means that ARTran has negative qty
					TranCost = -TranCost;
				}
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);

				if (!(sosetup != null && sosetup.SalesProfitabilityForNSKits == SalesProfitabilityNSKitMethod.NSKitStandardCostOnly && tran.IsComponentItem == true))
				{
					artran.TranCost += TranCost;
				}

                artran.IsTranCostFinal = true;
            }
        }

		public virtual POReceiptLineUpdate UpdatePOReceiptLineReleased(INTran tran)
		{
			if (string.IsNullOrEmpty(tran.POReceiptNbr) || tran.POReceiptLineNbr == null)
				return null;

			var row = new POReceiptLineUpdate
			{
				ReceiptNbr = tran.POReceiptNbr,
				LineNbr = tran.POReceiptLineNbr
			};
			row = this.poreceiptlineupdate.Insert(row);

			row.INReleased = tran.Released;

			return this.poreceiptlineupdate.Update(row);
		}

		public virtual POReceiptLineUpdate UpdatePOReceiptLineCost(INTran tran, INTranCost tranCost, InventoryItem item)
		{
			bool updatePOTranCostFinal =
				(tran.TranType == INTranType.Issue && (tran.ExactCost != true || item.ValMethod != INValMethod.Standard))
				&& tran.POReceiptType == PO.POReceiptType.POReturn && !string.IsNullOrEmpty(tran.POReceiptNbr) && tran.POReceiptLineNbr != null
				&& tran.Qty != 0m;
			if (!updatePOTranCostFinal)
				return null;

			var row = new POReceiptLineUpdate
			{
				ReceiptNbr = tran.POReceiptNbr,
				LineNbr = tran.POReceiptLineNbr
			};
			row = this.poreceiptlineupdate.Insert(row);

			row.UpdateTranCostFinal = true;
			row.TranCostFinal -= tranCost.InvtMult * tranCost.TranCost;

			return this.poreceiptlineupdate.Update(row);
		}

        public virtual void WriteGLNonStockCosts(JournalEntry je, INTran intran, InventoryItem item, INSite site)
        {
            if (item.StkItem == false && (intran.COGSAcctID != null || intran.AcctID != null))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = (intran.InvtMult == (short)0) ? intran.AcctID : intran.InvtAcctID;
                tran.SubID = (intran.InvtMult == (short)0) ? intran.SubID : GetValueInt<INTran.invtSubID>(je, intran);

                tran.CuryDebitAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;
                tran.DebitAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;
                tran.CuryCreditAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;
                tran.CreditAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.Qty = (intran.InvtMult == (short)1) ? intran.Qty : -intran.Qty;
                tran.UOM = intran.UOM;
                tran.TranDesc = intran.TranDesc;
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;

				bool accrueCost = intran.AccrueCost == true;
				tran.ProjectID = accrueCost ? ProjectDefaultAttribute.NonProject() : intran.ProjectID;
                tran.TaskID = accrueCost ? null : intran.TaskID;
				tran.CostCodeID = accrueCost ? null : intran.CostCodeID;

                tran.Released = true;

                je.GLTranModuleBatNbr.Insert(tran);
            }

            if (item.StkItem == false && (intran.COGSAcctID != null || intran.AcctID != null))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = (intran.COGSAcctID ?? intran.AcctID);
                tran.SubID = (GetValueInt<INTran.cOGSSubID>(je, intran) ?? GetValueInt<INTran.subID>(je, intran));

                tran.CuryDebitAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;
                tran.DebitAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;
                tran.CuryCreditAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;
                tran.CreditAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.Qty = (intran.InvtMult == (short)1) ? -intran.Qty : intran.Qty;
                tran.UOM = intran.UOM;
                tran.TranDesc = intran.TranDesc;
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = intran.ProjectID;
                tran.TaskID = intran.TaskID;
				tran.CostCodeID = intran.CostCodeID;
                tran.Released = true;

                je.GLTranModuleBatNbr.Insert(tran);
            }
        }

        public virtual void WriteGLCosts(JournalEntry je, INTranCost trancost, INTran intran, InventoryItem item, INSite site, INPostClass postclass, ReasonCode reasoncode, INLocation location)
        {
            bool isStdDropShip = intran != null && intran.SOShipmentType == SOShipmentType.DropShip && intran.POReceiptNbr != null && trancost.InvtMult == 0 && item.ValMethod == INValMethod.Standard;

            if (trancost.COGSAcctID != null || intran.AcctID != null)
            {
                GLTran tran = new GLTran();
                tran.SummPost = trancost.TranType == INTranType.Transfer && intran.DocType == trancost.CostDocType ? true : this.SummPost;
				tran.TranDate = trancost.TranDate;
				tran.TranPeriodID = trancost.TranPeriodID;
				tran.FinPeriodID = trancost.FinPeriodID;

				if (trancost.InvtMult == (short)0)
                {
                    tran.BranchID = intran.BranchID;
                    tran.AccountID = intran.AcctID;
                    tran.SubID = intran.SubID;
                    tran.ReclassificationProhibited = intran.ReclassificationProhibited;
                }
                else
                {
					tran.BranchID = site.BranchID;
                    tran.AccountID = trancost.InvtAcctID;
                    tran.SubID = GetValueInt<INTranCost.invtSubID>(je, trancost);
                    tran.ReclassificationProhibited = true;
					if (intran.BranchID != site.BranchID)
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, tran.TranPeriodID);
				}

                if (isStdDropShip)
                {
                    tran.CuryDebitAmt = 0m;
                    tran.DebitAmt = 0m;
                    tran.CuryCreditAmt = trancost.TranCost + trancost.VarCost;
                    tran.CreditAmt = trancost.TranCost + trancost.VarCost;
                }
                else
                {
                    tran.CuryDebitAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
                    tran.DebitAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
                    tran.CuryCreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;
                    tran.CreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;
                }

                tran.TranType = trancost.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.ZeroPost = trancost.CostDocType == intran.DocType && trancost.CostRefNbr == intran.RefNbr && tran.AccountID != null && tran.SubID != null;
                tran.RefNbr = trancost.RefNbr;
                tran.InventoryID = trancost.InventoryID;
                tran.Qty = (trancost.InvtMult == (short)1) ? trancost.Qty : -trancost.Qty;
                tran.UOM = item.BaseUnit;
                tran.TranDesc = intran.TranDesc;

                int? locProjectID;
                int? locTaskID = null;
                if (location != null && location.ProjectID != null)//can be null if Adjustment
                {
                    locProjectID = location.ProjectID;
                    locTaskID = location.TaskID;

                    if (locTaskID == null)//Location with ProjectTask WildCard
                    {
                        if (location.ProjectID == intran.ProjectID)
                        {
                            locTaskID = intran.TaskID;
                        }
                        else
                        {
                            //substitute with any task from the project.
                            PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
                                And<PMTask.visibleInIN, Equal<True>, And<PMTask.isActive, Equal<True>>>>>.Select(this, location.ProjectID);
                            if (task != null)
                            {
                                locTaskID = task.TaskID;
                            }
                        }
                    }

                }
                else
                {
                    locProjectID = PM.ProjectDefaultAttribute.NonProject();
                }

                if (trancost.TranType == INTranType.Adjustment || trancost.TranType == INTranType.Transfer)
                {
                    tran.ProjectID = locProjectID;
                    tran.TaskID = locTaskID;
                }
                else
                {
                    tran.ProjectID = intran.ProjectID ?? locProjectID;
                    tran.TaskID = intran.TaskID ?? locTaskID;
                }
				tran.CostCodeID = intran.CostCodeID;
                tran.Released = true;

                je.GLTranModuleBatNbr.Insert(tran);
            }

            if (item.ValMethod == INValMethod.Standard && (trancost.COGSAcctID != null || intran.AcctID != null))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = GetStdCostVarAcctID(item, site, postclass, intran, false);
                tran.SubID = GetStdCostVarSubID(item, site, postclass, intran, false);

                if (isStdDropShip)
                {
                    tran.CuryDebitAmt = trancost.VarCost;
                    tran.DebitAmt = trancost.VarCost;
                    tran.CuryCreditAmt = 0m;
                    tran.CreditAmt = 0m;
                }
                else
                {
                    tran.CuryDebitAmt = (trancost.InvtMult == (short)1) ? trancost.VarCost : 0m;
                    tran.DebitAmt = (trancost.InvtMult == (short)1) ? trancost.VarCost : 0m;
                    tran.CuryCreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.VarCost;
                    tran.CreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.VarCost;
                }

                tran.TranType = trancost.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.ZeroPost = trancost.CostDocType == intran.DocType && trancost.CostRefNbr == intran.RefNbr && tran.AccountID != null && tran.SubID != null;
                tran.RefNbr = trancost.RefNbr;
                tran.InventoryID = trancost.InventoryID;
                tran.Qty = (trancost.InvtMult == (short)1) ? trancost.Qty : -trancost.Qty;
                tran.UOM = item.BaseUnit;
                tran.TranDesc = intran.TranDesc;
				tran.TranDate = trancost.TranDate;
				tran.TranPeriodID = trancost.TranPeriodID;
				tran.FinPeriodID = trancost.FinPeriodID;
                tran.ProjectID = intran.ProjectID;
                tran.TaskID = intran.TaskID;
				tran.CostCodeID = intran.CostCodeID;
                tran.Released = true;

                je.GLTranModuleBatNbr.Insert(tran);
            }

            if (trancost.COGSAcctID != null || intran.AcctID != null)
            {
                //oversold transfers go to COGS instead of GIT
                if (trancost.TranType == INTranType.Transfer && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
                {
                    trancost.COGSAcctID = GetCogsAcctID(item, site, postclass, intran, false);
                    trancost.COGSSubID = GetCogsSubID(item, site, postclass, intran, false);
                }
                //oversold Assemblies go to Variance instead of WIP
                if ((trancost.TranType == INTranType.Assembly || trancost.TranType == INTranType.Disassembly) && reasoncode != null && reasoncode.AccountID != null && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
                {
                    trancost.COGSAcctID = reasoncode.AccountID;
                    trancost.COGSSubID = GetReasonCodeSubID(reasoncode, item, site, postclass, intran);
                }
                //oversold for issues with UpdateShippedNotInvoiced = true go to COGS
                if (intran != null && intran.UpdateShippedNotInvoiced == true && trancost.TranType == INTranType.Receipt && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
                {
                    trancost.COGSAcctID = GetCogsAcctID(item, site, postclass, intran, false);
                    trancost.COGSSubID = GetCogsSubID(item, site, postclass, intran, false);
                }

                GLTran tran = new GLTran();
                tran.SummPost = (trancost.TranType == INTranType.Transfer || trancost.TranType == INTranType.Assembly || trancost.TranType == INTranType.Disassembly) && intran.DocType == trancost.CostDocType ? true : this.SummPost;
                // AC-119895: If receipt was created before upgrade we should use old 'OrigBranchID' field.
				tran.BranchID = trancost.COGSAcctID == null ? intran.DestBranchID ?? intran.OrigBranchID ?? intran.BranchID : intran.BranchID;
                tran.AccountID = (trancost.COGSAcctID ?? intran.AcctID);
                tran.SubID = (GetValueInt<INTranCost.cOGSSubID>(je, trancost) ?? GetValueInt<INTran.subID>(je, intran));

                if (isStdDropShip)
                {
                    tran.CuryDebitAmt = trancost.TranCost;
                    tran.DebitAmt = trancost.TranCost;
                    tran.CuryCreditAmt = 0m;
                    tran.CreditAmt = 0m;
                }
                else
                {
                    tran.CuryDebitAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m);
                    tran.DebitAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m);
                    tran.CuryCreditAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m) : 0m;
                    tran.CreditAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m) : 0m;
                }

                tran.TranType = trancost.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.ZeroPost = trancost.CostDocType == intran.DocType && trancost.CostRefNbr == intran.RefNbr && tran.AccountID != null && tran.SubID != null;
                tran.RefNbr = trancost.RefNbr;
                tran.InventoryID = trancost.InventoryID;
                tran.Qty = (trancost.InvtMult == (short)1) ? -trancost.Qty : trancost.Qty;
                tran.UOM = item.BaseUnit;
                tran.TranDesc = intran.TranDesc;
				tran.TranDate = trancost.TranDate;
				tran.TranPeriodID = trancost.TranPeriodID;
				tran.FinPeriodID = trancost.FinPeriodID;

                if (trancost.TranType == INTranType.Adjustment)
                {
                    //Other Inventory Adjustments always goes to Non-Project
                    tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
                    tran.TaskID = null;
                }
                else if (trancost.TranType == INTranType.Transfer)
                {
                    //GIT always to Non-Project.
                    tran.ProjectID = trancost.COGSAcctID == null ? PM.ProjectDefaultAttribute.NonProject() : intran.ProjectID;
                    tran.TaskID = trancost.COGSAcctID == null ? null : intran.TaskID;
                }
                else
                {
                    tran.ProjectID = intran.ProjectID;
                    tran.TaskID = intran.TaskID;
                }
				tran.CostCodeID = intran.CostCodeID;
                tran.Released = true;
                tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

                je.GLTranModuleBatNbr.Insert(tran);
            }

            //Write off production variance from WIP
            if (WIPCalculated && intran.AssyType == INAssyType.KitTran && string.Equals(trancost.CostDocType, intran.DocType) && string.Equals(trancost.CostRefNbr, intran.RefNbr))
            {
                GLTran tran = new GLTran();
                tran.SummPost = true;
                tran.ZeroPost = false;
                tran.BranchID = intran.BranchID;
                tran.AccountID = INProgressAcctID;
                tran.SubID = INProgressSubID;
                tran.ReclassificationProhibited = true;

                tran.CuryDebitAmt = 0m;
                tran.DebitAmt = 0m;
                tran.CuryCreditAmt = WIPVariance;
                tran.CreditAmt = WIPVariance;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ProductionVarianceTranDesc);
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = intran.ProjectID;
                tran.TaskID = intran.TaskID;
				tran.CostCodeID = intran.CostCodeID;
                tran.Released = true;
                tran.TranLineNbr = null;

                je.GLTranModuleBatNbr.Insert(tran);
            }

            if (WIPCalculated && intran.AssyType == INAssyType.KitTran && string.Equals(trancost.CostDocType, intran.DocType) && string.Equals(trancost.CostRefNbr, intran.RefNbr))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = reasoncode.AccountID;
                tran.SubID = GetReasonCodeSubID(reasoncode, item, site, postclass, intran);

                tran.CuryDebitAmt = WIPVariance;
                tran.DebitAmt = WIPVariance;
                tran.CuryCreditAmt = 0m;
                tran.CreditAmt = 0m;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ProductionVarianceTranDesc);
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = intran.ProjectID;
                tran.TaskID = intran.TaskID;
				tran.CostCodeID = intran.CostCodeID;
                tran.Released = true;
                tran.TranLineNbr = null;

                je.GLTranModuleBatNbr.Insert(tran);

                WIPCalculated = false;
                WIPVariance = 0m;
            }
        }

        public object GetValueExt<Field>(PXCache cache, object data)
            where Field : class, IBqlField
        {
            object val = cache.GetValueExt<Field>(data);

            if (val is PXFieldState)
            {
                return ((PXFieldState)val).Value;
            }
            else
            {
                return val;
            }
        }

        List<Segment> _SubItemSeg = null;
        Dictionary<short?, string> _SubItemSegVal = null;

        public virtual void ParseSubItemSegKeys()
        {
            if (_SubItemSeg == null)
            {
                _SubItemSeg = new List<Segment>();

                foreach (Segment seg in PXSelect<Segment, Where<Segment.dimensionID, Equal<IN.SubItemAttribute.dimensionName>>>.Select(this))
                {
                    _SubItemSeg.Add(seg);
                }

                _SubItemSegVal = new Dictionary<short?, string>();

                foreach (SegmentValue val in PXSelectJoin<SegmentValue, InnerJoin<Segment, On<Segment.dimensionID, Equal<SegmentValue.dimensionID>, And<Segment.segmentID, Equal<SegmentValue.segmentID>>>>, Where<SegmentValue.dimensionID, Equal<IN.SubItemAttribute.dimensionName>, And<Segment.isCosted, Equal<boolFalse>, And<SegmentValue.isConsolidatedValue, Equal<boolTrue>>>>>.Select(this))
                {
                    try
                    {
                        _SubItemSegVal.Add((short)val.SegmentID, val.Value);
                    }
                    catch (Exception excep)
                    {
                        throw new PXException(excep, Messages.MultipleAggregateChecksEncountred, val.SegmentID, val.DimensionID);
                    }
                }
            }
        }

        public virtual string MakeCostSubItemCD(string SubItemCD)
        {
            StringBuilder sb = new StringBuilder();

            int offset = 0;

            foreach (Segment seg in _SubItemSeg)
            {
                string segval = SubItemCD.Substring(offset, (int)seg.Length);
                if (seg.IsCosted == true || segval.TrimEnd() == string.Empty)
                {
                    sb.Append(segval);
                }
                else
                {
                    if (_SubItemSegVal.TryGetValue(seg.SegmentID, out segval))
                    {
						segval = segval.PadRight((int)seg.Length);
						sb.Append(segval);
                    }
                    else
                    {
                        throw new PXException(Messages.SubItemSeg_Missing_ConsolidatedVal);
                    }
                }
                offset += (int)seg.Length;
            }

            return sb.ToString();
        }

        public virtual void UpdateCrossReference(INTranSplit split, InventoryItem item, INLocation whseloc)
        {
            if (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<INTran.locationID>(intranselect.Cache));
            }

            if (split.SubItemID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<INTran.subItemID>(intranselect.Cache));
            }

            INCostSubItemXRef xref = new INCostSubItemXRef();

            xref.SubItemID = split.SubItemID;
            xref.CostSubItemID = split.SubItemID;

            string SubItemCD = (string)this.GetValueExt<INCostSubItemXRef.costSubItemID>(costsubitemxref.Cache, xref);

            xref.CostSubItemID = null;

            string CostSubItemCD = PXAccess.FeatureInstalled<FeaturesSet.subItem>() ? MakeCostSubItemCD(SubItemCD) : SubItemCD;

            costsubitemxref.Cache.SetValueExt<INCostSubItemXRef.costSubItemID>(xref, CostSubItemCD);
            xref = costsubitemxref.Update(xref);

            if (costsubitemxref.Cache.GetStatus(xref) == PXEntryStatus.Updated)
            {
                costsubitemxref.Cache.SetStatus(xref, PXEntryStatus.Notchanged);
            }

            split.CostSubItemID = xref.CostSubItemID;
            //Standard & Specific Cost items will ignore per location costing, Standard can have null location
            split.CostSiteID = (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc.IsCosted == true ? whseloc.LocationID : split.SiteID);
        }

        public virtual void ReleaseDocProcR(JournalEntry je, INRegister doc)
        {
            int retryCnt = 5;
            while (true)
            {
                try
                {
                    ReleaseDocProc(je, doc);
                    return;
                }
                catch (PXRestartOperationException e)
                {
                    if (retryCnt-- < 0)
                    {
                        if (e.InnerException != null)
                            throw e.InnerException;
                        else
                            throw;
                    }
                    else
                    {
                        this.Clear();
                    }
                }
            }
        }

        public virtual void UpdateSplitDestinationLocation(INTranSplit split, int? value)
        {
            split.ToLocationID = value;

            if (IsOneStepTransferWithinSite())
            {
                INLocation originLocation = INTranSplit.FK.Location.FindParent(this, split);
                INLocation targetLocation = INTranSplit.FK.ToLocation.FindParent(this, split);

                split.SkipCostUpdate = (originLocation?.IsCosted) != true && (targetLocation?.IsCosted) != true;
                split.SkipQtyValidation = targetLocation?.IsSorting == true;
            }
            intransplit.Cache.MarkUpdated(split);
        }

        public virtual bool IsOneStepTransfer()
        {
            INRegister doc = inregister.Current;
            return doc.DocType == INDocType.Transfer && doc.TransferType == INTransferType.OneStep;
        }

		public virtual bool IsOneStepTransferWithinSite()
		{
			INRegister doc = inregister.Current;
			return doc.DocType == INDocType.Transfer && doc.TransferType == INTransferType.OneStep && doc.SiteID == doc.ToSiteID;
		}

		public virtual void ValidateTransferDocIntegrity(INRegister doc)
        {
			foreach (PXResult<INTranSplit, INItemPlan> res in PXSelectReadonly2<INTranSplit,
				LeftJoin<INItemPlan, 
					On<INTranSplit.FK.ItemPlan>>,
				Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>,
					And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
            {
				INTranSplit split = res;
				INItemPlan plan = res;

				if (split.TransferType != doc.TransferType
					|| split.TransferType == INTransferType.TwoStep
					&& INTranSplitPlanIDAttribute.IsTwoStepTransferPlanValid(Caches[typeof(INTranSplit)], split, plan) == false)
				{
					throw new PXException(Messages.TransferIsCorrupted, doc.RefNbr);
				}
			}
		}

		public virtual void ValidateOrigLCReceiptIsReleased(INRegister doc)
		{
			var unattachedLCtran = (INTran)PXSelectJoin<INTran,
				LeftJoin<INTran2,
					On<INTran2.docType, NotEqual<INDocType.adjustment>,
						And<INTran2.docType, NotEqual<INDocType.transfer>,
							And<INTran2.pOReceiptNbr, Equal<INTran.pOReceiptNbr>,
								And<INTran2.pOReceiptLineNbr, Equal<INTran.pOReceiptLineNbr>,
									And<INTran2.released, Equal<True>>>>>>>,
			Where<INTran.docType, Equal<Current<INRegister.docType>>,
				And<INTran.refNbr, Equal<Current<INRegister.refNbr>>,
						And<INTran.tranType, Equal<INTranType.receiptCostAdjustment>,
							And<INTran2.refNbr, IsNull>>>>>.SelectSingleBound(this, new[] { doc });

			if (unattachedLCtran != null)
			{
				var ortran = LandedCostAllocationService.Instance.GetOriginalInTran(this, unattachedLCtran.POReceiptNbr, unattachedLCtran.POReceiptLineNbr);

				string errorMessage = "";
				if (doc.IsPPVTran == true)
					errorMessage = AP.Messages.INReceiptMustBeReleasedBeforePPV;
				else if (doc.IsTaxAdjustmentTran == true)
					errorMessage = AP.Messages.INReceiptMustBeReleasedBeforeTaxAdjustment;
				else
					errorMessage = PO.Messages.INReceiptMustBeReleasedBeforeLCProcessing;

				throw new PXException(errorMessage, ortran == null ? String.Empty : ortran.RefNbr, unattachedLCtran.POReceiptNbr);
			}
		}

        public virtual void ReleaseDocProc(JournalEntry je, INRegister doc)
        {
            if ((bool)doc.Hold)
            {
                throw new PXException(Messages.Document_OnHold_CannotRelease);
            }

			if (doc.DocType == INDocType.Transfer)
			{
				ValidateTransferDocIntegrity(doc);
			}

			ValidateOrigLCReceiptIsReleased(doc);

            //planning requires document context.
            inregister.Current = doc;
            //mark as updated so that doc will not expire from cache, and totalcost will not be overwritten with old value
            inregister.Cache.SetStatus(doc, PXEntryStatus.Updated);

            INItemPlanIDAttribute.SetReleaseMode<INTranSplit.planID>(intransplit.Cache, true);

	        using (PXTransactionScope ts = new PXTransactionScope())
	        {
		        SegregateBatch(je, doc.BranchID, doc.TranDate, doc.FinPeriodID, doc.TranDesc);

		        INTran prev_tran = null;
		        int? prev_linenbr = null;

				if (IsOneStepTransfer())
				{
					foreach (PXResult<INTran, INTranSplit, INSite> res in PXSelectJoin<INTran,
							InnerJoin<INTranSplit, On<INTranSplit.FK.Tran>,
							InnerJoin<INSite, On<INTran.FK.ToSite>>>,
				        Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
					        And<INTran.docType, Equal<INDocType.transfer>, And<INTran.invtMult, Equal<shortMinus1>>>>>,
				        OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>
			        .Select(this, doc.DocType, doc.RefNbr))
		        {
			        INTran tran = res;
			        INTranSplit split = res;
			        INSite site = res;

			        UpdateSplitDestinationLocation(split, tran.ToLocationID);

                        if (object.Equals(prev_tran, tran) == false)
                        {
                            INTran newtran = PXCache<INTran>.CreateCopy(tran);
                            if (newtran.DestBranchID == null)
                            {
                                newtran.OrigBranchID = newtran.BranchID;
                            }
                            newtran.OrigDocType = newtran.DocType;
                            newtran.OrigTranType = newtran.TranType;
                            newtran.OrigRefNbr = newtran.RefNbr;
                            newtran.OrigLineNbr = newtran.LineNbr;
                            if (tran.TranType == INTranType.Transfer)
                            {
                                newtran.OrigNoteID = doc.NoteID;
                                newtran.OrigToLocationID = tran.ToLocationID;
                                newtran.OrigIsLotSerial = !(string.IsNullOrEmpty(split.LotSerialNbr));
                            }
                            newtran.BranchID = site.BranchID;
                            newtran.LineNbr = (int)PXLineNbrAttribute.NewLineNbr<INTran.lineNbr>(intranselect.Cache, doc);
                            newtran.InvtMult = (short)1;
                            newtran.SiteID = newtran.ToSiteID;
                            newtran.LocationID = newtran.ToLocationID;
                            newtran.DestBranchID = null;
                            newtran.ToSiteID = null;
                            newtran.ToLocationID = null;
                            newtran.InvtAcctID = null;
                            newtran.InvtSubID = null;
                            newtran.ARDocType = null;
                            newtran.ARRefNbr = null;
                            newtran.ARLineNbr = null;
	                        newtran.NoteID = null;

				        newtran = intranselect.Insert(newtran);
				        //persist now for join right part 
				        //intranselect.Cache.PersistInserted(newtran);

				        prev_tran = tran;
				        prev_linenbr = newtran.LineNbr;
			        }

			        INTranSplit newsplit = PXCache<INTranSplit>.CreateCopy(split);
			        newsplit.LineNbr = prev_linenbr;
			        newsplit.SplitLineNbr =
                        (int)PXLineNbrAttribute.NewLineNbr<INTranSplit.splitLineNbr>(intransplit.Cache, doc);
                    newsplit.InvtMult = (short)1;
			        newsplit.SiteID = tran.ToSiteID;
			        newsplit.LocationID = tran.ToLocationID;
			        newsplit.FromSiteID = split.SiteID;
			        newsplit.FromLocationID = split.LocationID;
			        newsplit.SkipCostUpdate = split.SkipCostUpdate;
			        newsplit.PlanID = null;

			        newsplit = intransplit.Insert(newsplit);
			        //persist now for join right part 
			        //intransplit.Cache.PersistInserted(newsplit);
		        }
				}

		        var originalintranlist = new PXResultset<INTran, INTranSplit, InventoryItem>();
		        foreach (PXResult<INTran, InventoryItem, INLocation, INLotSerClass> res in PXSelectJoin<INTran,
                    InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
					LeftJoin<INLocation, On<INTran.FK.Location>,
                    InnerJoin<INLotSerClass, On<InventoryItem.FK.LotSerClass>>>>,
				        Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
					        And<INTran.tranType, Equal<INTranType.receiptCostAdjustment>>>>,
				        OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>
			        .Select(this, doc.DocType, doc.RefNbr))
		        {
                    InventoryItem item = (InventoryItem)res;
                    INLocation whseloc = (INLocation)res;
                    INTran tran = (INTran)res;
                    INTranSplit split = (INTranSplit)tran;

			        UpdateCrossReference(split, item, whseloc);

			        originalintranlist.Add(new PXResult<INTran, INTranSplit, InventoryItem>(tran, split, item));
		        }

		        RegenerateInTranList(originalintranlist);

		        if (intranselect.Cache.IsDirty)
		        {
			        this.Persist(typeof(INTran), PXDBOperation.Insert);
			        this.Persist(typeof(INTranSplit), PXDBOperation.Insert);
			        this.Persist(typeof(INItemPlan), PXDBOperation.Insert);
			        byte[] timestamp = this.TimeStamp;
			        try
			        {
				        this.TimeStamp = PXDatabase.SelectTimeStamp();
				        intranselect.Cache.Persisted(false);
				        intransplit.Cache.Persisted(false);
				        initemplan.Cache.Persisted(false);
			        }
			        finally
			        {
				        this.TimeStamp = timestamp;
			        }
		        }

		        //caching transit lines
		        foreach (INTransitLine res in
			        PXSelectJoin<INTransitLine,
				        InnerJoin<INTran, On<INTran.origRefNbr, Equal<INTransitLine.transferNbr>,
					        And<INTran.origLineNbr, Equal<INTransitLine.transferLineNbr>>>>,
				        Where<INTran.docType, Equal<Current<INRegister.docType>>,
                            And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>>.SelectMultiBound(this, new object[] { doc }))
		        {
		        }

		        foreach (PXResult<INTranSplit, INTran, INItemPlan, INItemSite, INSite> res in PXSelectJoin<INTranSplit,
			        InnerJoin<INTran, 
						On<INTranSplit.FK.Tran>,
				    InnerJoin<INItemPlan, 
						On<INTranSplit.FK.ItemPlan>,
					LeftJoinSingleTable<INItemSite, 
						On<INItemSite.inventoryID, Equal<INTran.inventoryID>,
						And<INItemSite.siteID, Equal<INTran.toSiteID>>>,
                    LeftJoin<INSite, 
						On<INTran.FK.ToSite>>>>>,
			        Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>,
				        And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
		        {
			        INTranSplit split = res;
			        INTran tran = res;
			        INItemPlan plan = res;
			        INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);
			        INItemSite itemsite = res ?? new INItemSite();
			        INSite site = res ?? new INSite();

			        //avoid ReadItem()
			        initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);

			        bool replanToTransit = (!IsOneStepTransfer() && plantype.DeleteOnEvent != true && plantype.ReplanOnEvent == INPlanConstants.Plan42)
			                               || (plantype.PlanType == INPlanConstants.Plan62 && doc.OrigModule == BatchModule.SO &&
			                                   doc.DocType == INDocType.Transfer);
			        if (replanToTransit)
			        {
				        ReattachPlanToTransit(plan, tran, split, itemsite, site);
			        }
			        else
			        {
				        if (plantype.DeleteOnEvent == true)
				        {
					        initemplan.Delete(plan);
					        intransplit.Cache.SetStatus(split, PXEntryStatus.Updated);
                            split = (INTranSplit)intransplit.Cache.Locate(split);
					        if (split != null) split.PlanID = null;
				        }
				        else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
				        {
					        plan = PXCache<INItemPlan>.CreateCopy(plan);
					        plan.PlanType = plantype.ReplanOnEvent;
					        initemplan.Cache.Update(plan);
				        }
			        }
		        }

                ProcessLinkedAllocation(doc);

                    PXFormulaAttribute.SetAggregate<INTranCost.qty>(intrancost.Cache, typeof(SumCalc<INTran.costedQty>));
                    PXFormulaAttribute.SetAggregate<INTranCost.tranCost>(intrancost.Cache, typeof(SumCalc<INTran.tranCost>));
                    PXFormulaAttribute.SetAggregate<INTranCost.tranAmt>(intrancost.Cache, typeof(SumCalc<INTran.tranAmt>));

		        prev_tran = null;

				var mainselect = this.GetMainSelect(doc);

                var tranDataRows = mainselect.Select(doc.DocType, doc.RefNbr).AsEnumerable();

		        foreach (PXResult<INTran, InventoryItem, INSite, INPostClass, INLotSerClass, INTranSplit, ReasonCode,
			        INLocation> res in tranDataRows)
		        {
                    INTran tran = (INTran)res;
                    INSite site = (INSite)res;
                    INTranSplit resSplit = (INTranSplit)res;
                    InventoryItem item = (InventoryItem)res;
			        ValidateTran(doc, tran, resSplit, item, site);

			        tran = PXCache<INTran>.CreateCopy(tran);
			        tran.TranDate = doc.TranDate;
			        tran.TranPeriodID = doc.TranPeriodID;
					intranselect.Cache.SetDefaultExt<INTran.finPeriodID>(tran);
			        tran = intranselect.Update(tran);
		        }

		        ValidateFinPeriod(doc, tranDataRows.Cast<PXResult<INTran, InventoryItem, INSite>>());

					foreach (PXResult<INTran, InventoryItem, INSite, INPostClass, INLotSerClass, INTranSplit, ReasonCode, INLocation> res in tranDataRows)
                    {
                        INTran tran = (INTran)res;
						INTranSplit resSplit = (INTranSplit)res;
						INTranSplit split = (resSplit.RefNbr != null) ? resSplit : (INTranSplit)tran;
                        InventoryItem item = (InventoryItem)res;
                        INSite site = (INSite)res;
                        ReasonCode reasoncode = (ReasonCode)res;
                        INLocation whseloc = (((INLocation)res).LocationID != null) ? (INLocation)res : INLocation.PK.Find(this, tran.LocationID);
                        INPostClass postclass = (INPostClass)res;
                        INLotSerClass lotserclass = (INLotSerClass)res;

						PXParentAttribute.SetParent(intranselect.Cache, tran, typeof(INRegister), inregister.Current);

						InventoryItem.PK.StoreCached(this, item);
						INSite.PK.StoreCached(this, site);
						ReasonCode.PK.StoreCached(this, reasoncode);
						INLocation.PK.StoreCached(this, whseloc);
						INPostClass.PK.StoreCached(this, postclass);
						INLotSerClass.PK.StoreCached(this, lotserclass);

                        tran = PXCache<INTran>.CreateCopy(tran);
                        tran.Released = true;
                        tran = intranselect.Update(tran);

                        //zero quantity auto added splits will have it null
                        if (split.CreatedDateTime != null)
                        {
                            //locate split record not to erase PlanID
                            split = intransplit.Locate(split) ?? split;
                            split.TranDate = doc.TranDate;
                            split.Released = true;
                            split = intransplit.Update(split);
                        }

                        //ignore split processing for zero qty transactions
                        if (((tran.TranType == INTranType.Adjustment || tran.TranType == INTranType.NegativeCostAdjustment || tran.TranType == INTranType.StandardCostAdjustment || tran.TranType == INTranType.ReceiptCostAdjustment) || tran.Qty != 0m))
                        {
                            if (item.StkItem == true)
                            {
                                UpdateCrossReference(split, item, whseloc);
                                UpdateItemSite(tran, item, site, reasoncode, postclass);

                                if (split.BaseQty != 0m)
                                {
                                    UpdateSiteStatus(tran, split, whseloc);
                                    UpdateLocationStatus(tran, split);
                                    UpdateLotSerialStatus(tran, split, item, lotserclass);
                                    UpdateSiteHist(tran, split);
                                    UpdateSiteHistD(split);
									UpdateSiteHistDay(tran, split);
                                }
                                UpdateItemLotSerial(tran, split, item, lotserclass);
                                UpdateSiteLotSerial(tran, split, item, lotserclass);

                                if (!(split.SkipCostUpdate ?? false))
                                    UpdateCostStatus(prev_tran, tran, split, item);

                                prev_tran = tran;
                            }
                            else
                            {
                                if (tran.AssyType == INAssyType.KitTran || tran.AssyType == INAssyType.CompTran)
                                {
                                    throw new PXException(Messages.NonStockKitAssemblyNotAllowed);
                                }

                                UpdateItemSite(tran, item, site, reasoncode, postclass);
                                AssembleCost(tran, split, item);
                                WriteGLNonStockCosts(je, tran, item, site);
                                UpdateARTranCost(tran);
                            }
                        }
						UpdatePOReceiptLineReleased(tran);
                    }

                    UpdateTransitPlans();

                    if (this.insetup.Current.ReplanBackOrders == true)
                    {
                        ReplanBackOrders();
                    }

                    PXFormulaAttribute.SetAggregate<INTranCost.qty>(intrancost.Cache, null);
                    PXFormulaAttribute.SetAggregate<INTranCost.tranCost>(intrancost.Cache, null);
                    PXFormulaAttribute.SetAggregate<INTranCost.tranAmt>(intrancost.Cache, null);

					if (doc.DocType.IsIn(INDocType.Issue, INDocType.Adjustment))
                    {
                        PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, typeof(SumCalc<INRegister.totalCost>));
                        try
                        {
                            PXFormulaAttribute.CalcAggregate<INTran.tranCost>(intranselect.Cache, doc);
                        }
                        finally
                        {
                            PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, null);
                        }
                    }

                    SetOriginalQty();
                    ReceiveOversold(doc);
                    ReceiveQty();

                    var cosplits = new Dictionary<INTranSplit, List<INTranSplit>>(new INTranSplitCostComparer());
                    foreach (INTranSplit split in intransplit.Cache.Updated)
                    {
                        List<INTranSplit> list;
                        if (!cosplits.TryGetValue(split, out list))
                        {
                            cosplits[split] = list = new List<INTranSplit>();
                        }
                        list.Add(split);
                    }

                    foreach (INTranCost costtran in intrancost.Cache.Inserted)
                    {
                        if (costtran.IsVirtual ?? false)
                            continue;
                        INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
                        if (tran != null)
                        {
                            var ortran = LandedCostAllocationService.Instance.GetOriginalInTran(this, tran.POReceiptNbr, tran.POReceiptLineNbr);

                            UpdateAdditionalCost(ortran, costtran);

                            //specific items are handled only here since they do not have oversolds
                            if (costtran.CostDocType == tran.DocType && costtran.CostRefNbr == tran.RefNbr)
                            {
                                INTranSplit upd = new INTranSplit
                                {
									DocType = costtran.DocType,
                                    RefNbr = costtran.RefNbr,
                                    LineNbr = costtran.LineNbr,
                                    CostSiteID = costtran.CostSiteID,
                                    CostSubItemID = costtran.CostSubItemID,
                                    ValMethod = costtran.LotSerialNbr != null ? INValMethod.Specific : INValMethod.Average,
                                    LotSerialNbr = costtran.LotSerialNbr
                                };

                                List<INTranSplit> list;
                                if (cosplits.TryGetValue(upd, out list))
                                {
                                    foreach (INTranSplit split in list)
                                    {
                                        split.TotalQty += costtran.Qty;
                                        split.TotalCost += costtran.TranCost;
                                    }
                                }
                            }
                            else
                            {
                                INTranSplitUpdate upd = new INTranSplitUpdate
                                {
									DocType = costtran.DocType,
                                    RefNbr = costtran.RefNbr,
                                    LineNbr = costtran.LineNbr,
                                    CostSiteID = costtran.CostSiteID,
                                    CostSubItemID = costtran.CostSubItemID,
                                };

                                upd = intransplitupdate.Insert(upd);
                                upd.TotalQty += costtran.Qty;
                                upd.TotalCost += costtran.TotalCost;
                            }
                        }
                    }

                    foreach (INTranCost costtran in intrancost.Cache.Inserted)
                    {
                        if (costtran.IsVirtual ?? false)
                            continue;
                        INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
                        if (tran != null)
                        {
                            if (!(costtran.CostDocType == tran.DocType && costtran.CostRefNbr == tran.RefNbr))
                            {
                                INTranUpdate upd = new INTranUpdate
                                {
                                    DocType = tran.DocType,
                                    RefNbr = costtran.RefNbr,
                                    LineNbr = costtran.LineNbr
                                };

                                upd = intranupdate.Insert(upd);
                                upd.TranCost += costtran.TotalCost;
                            }
                        }
                    }

                    prev_tran = null;

					var piController = new Lazy<INPIController>(() => PXGraph.CreateInstance<INPIController>());
                    foreach (INTranCost costtran in intrancost.Cache.Inserted)
                    {
                        INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
                        if (tran != null)
                        {
							InventoryItem item = InventoryItem.PK.Find(this, costtran.InventoryID);
							INPostClass postClass = INPostClass.PK.Find(this, item?.PostClassID) ?? new INPostClass();
							INSite site = INSite.PK.Find(this, tran.SiteID);
                            ReasonCode reasoncode = ReasonCode.PK.Find(this, tran.ReasonCode);
                            INLocation location = INLocation.PK.Find(this, tran.LocationID);

                            UpdateCostHist(costtran, tran);
                            UpdateSalesHist(costtran, tran);
                            UpdateCustSalesHist(costtran, tran);

                            if (object.Equals(prev_tran, tran) == false)
                            {
                                if (tran.DocType == costtran.CostDocType && tran.RefNbr == costtran.CostRefNbr)
                                {
                                    UpdateSalesHistD(tran);
									UpdateCustSalesStats(tran);
                                    WriteGLSales(je, tran);
                                }

                                if ((tran.InvtMult == (short)-1 || tran.InvtMult == (short)1 && tran.OrigLineNbr != null) && tran.Qty != 0m &&
                                    Math.Abs((decimal)tran.TranCost - PXCurrencyAttribute.BaseRound(this, (decimal)tran.Qty * (decimal)tran.UnitCost)) > 0.00005m)
                                {
                                    tran.UnitCost = PXDBPriceCostAttribute.Round((decimal)tran.TranCost / (decimal)tran.Qty);
                                }
                            }

                            UpdateARTranCost(tran, costtran.TranCost);

							UpdatePOReceiptLineCost(tran, costtran, item);

							if (doc.DocType == INDocType.Adjustment && !string.IsNullOrEmpty(doc.PIID) && tran.PILineNbr != null)
							{
								piController.Value.AccumulateFinalCost(doc.PIID, (int)tran.PILineNbr, costtran.InvtMult * costtran.TranCost ?? 0m);
							}

							if (tran.SOShipmentNbr != null && tran.InvtMult != (short)0)
                            {
                                if (tran.DocType == costtran.CostDocType && tran.RefNbr == costtran.CostRefNbr)
                                {
                                    SOShipLineUpdate shipline = new SOShipLineUpdate();
                                    shipline.ShipmentType = tran.SOShipmentType;
                                    shipline.ShipmentNbr = tran.SOShipmentNbr;
                                    shipline.LineNbr = tran.SOShipmentLineNbr;

                                    shipline = this.soshiplineupdate.Insert(shipline);

                                    shipline.ExtCost += costtran.TranCost;
                                    shipline.UnitCost = PXDBPriceCostAttribute.Round((decimal)(shipline.ExtCost / (tran.Qty != 0m ? tran.Qty : null) ?? 0m));
                                }
                            }

                            prev_tran = tran;

                            WriteGLCosts(je, costtran, tran, item, site, postClass, reasoncode, location);
                        }
                    }

					if (doc.DocType == INDocType.Adjustment && !string.IsNullOrEmpty(doc.PIID))
					{
						piController.Value.ReleasePI(doc.PIID);
					}

                    foreach (ItemCostHist hist in itemcosthist.Cache.Inserted)
                    {
                        INSite insite = INSite.PK.Find(this, hist.CostSiteID);

                        if (insite != null)
                        {
                            ItemStats stats = new ItemStats();
                            stats.InventoryID = hist.InventoryID;
                            stats.SiteID = hist.CostSiteID;

                            stats = itemstats.Insert(stats);

                            stats.QtyOnHand += hist.FinYtdQty;
                            stats.TotalCost += hist.FinYtdCost;
                            stats.QtyReceived += hist.FinPtdQtyReceived + hist.FinPtdQtyTransferIn + hist.FinPtdQtyAssemblyIn;
                            stats.CostReceived += hist.FinPtdCostReceived + hist.FinPtdCostTransferIn + hist.FinPtdCostAssemblyIn;
                        }
                    }

                    foreach (ItemStats stats in itemstats.Cache.Cached)
                    {
                        if (itemstats.Cache.GetStatus(stats) != PXEntryStatus.Notchanged)
                        {
                            if (stats.QtyReceived != 0m && stats.QtyReceived != null && stats.CostReceived != null)
                            {
                                stats.LastCost = PXDBPriceCostAttribute.Round((decimal)(stats.CostReceived / stats.QtyReceived));
                                stats.LastCostDate = DateTime.Now;
                            }
                            else
                                stats.LastCost = 0m;

                            stats.MaxCost = stats.LastCost;
                            stats.MinCost = stats.LastCost;
                        }
                    }

                    if (UpdateGL && (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted || doc.SiteID != doc.ToSiteID || doc.DocType != INDocType.Transfer))
                    {
                        je.Save.Press();
                        doc.BatchNbr = je.BatchModule.Current.BatchNbr;
                    }

                    doc.Released = true;
					doc = inregister.Update(doc);

                    this.Actions.PressSave();

					UpdateINRegisterReleased(doc);

                    ts.Complete();
                }
            }

		public virtual void UpdateINRegisterReleased(INRegister doc)
		{
			// prevent double releasing by the fact that the Released flag can be set only one time
			bool setReleasedFlag = PXDatabase.Update<INRegister>(
				new PXDataFieldAssign<INRegister.released>(PXDbType.Bit, true),
				new PXDataFieldRestrict<INRegister.docType>(PXDbType.Char, doc.DocType),
				new PXDataFieldRestrict<INRegister.refNbr>(PXDbType.NVarChar, doc.RefNbr),
				new PXDataFieldRestrict<INRegister.released>(PXDbType.Bit, false));
			if (!setReleasedFlag)
			{
				throw new PXLockViolationException(typeof(INRegister), PXDBOperation.Update, new object[] { doc.DocType, doc.RefNbr });
			}
		}

		public virtual PXSelectBase<INTran> GetMainSelect(INRegister doc)
		{
			var mainSelect = new PXSelectJoin<INTran,
				InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
				InnerJoin<INSite, On<INTran.FK.Site>,
				LeftJoin<INPostClass, On<InventoryItem.FK.PostClass>,
				LeftJoin<INLotSerClass, On<InventoryItem.FK.LotSerClass>,
				LeftJoin<INTranSplit, On<INTranSplit.FK.Tran>,
				LeftJoin<ReasonCode, On<INTran.FK.ReasonCode>,
				LeftJoin<INLocation, On<INTranSplit.FK.Location>>>>>>>>,
				Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>>(this);

			this.OverrideMainSelectOrderBy(doc, mainSelect);

			return mainSelect;
		}

		public virtual void OverrideMainSelectOrderBy(INRegister doc, PXSelectBase<INTran> mainSelect)
		{
			switch (doc.DocType)
			{
				//For transfer its required to set up all the acceptor costsites and related info first
				case INDocType.Transfer:
					mainSelect.OrderByNew<OrderBy<Asc<INTran.docType, Asc<INTran.refNbr, Desc<INTran.invtMult, Asc<INTran.lineNbr>>>>>>();
					break;
				//for adjustments it's required to update all the acceptor cost layers first
				case INDocType.Adjustment:
					mainSelect.OrderByNew<OrderBy<Asc<INTran.docType, Asc<INTran.refNbr, Desc<INTranSplit.invtMult, Desc<INTran.tranCost, Asc<INTran.lineNbr>>>>>>>();
					break;
				//meanwhile ascending order is required for assembly
				default:
					mainSelect.OrderByNew<OrderBy<Asc<INTran.docType, Asc<INTran.refNbr, Asc<INTran.invtMult, Asc<INTran.lineNbr>>>>>>();
					break;
			}
		}

        protected virtual void ProcessLinkedAllocation(INRegister doc)
        {
            //replan fixed SO Demand
            //Multiple PO Receipts are never consolidated into single IN Receipt
            string POReceiptType = null;
            string POReceiptNbr = null;
            var planlist = new List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>>();

            foreach (PXResult<INItemPlan, INTranSplit, INTran, INItemPlanDemand> res in
                PXSelectJoin<INItemPlan,
                InnerJoin<INTranSplit, On<INTranSplit.planID, Equal<INItemPlan.supplyPlanID>>,
                InnerJoin<INTran, On<INTranSplit.FK.Tran>,
                LeftJoin<INItemPlanDemand, On<INItemPlan.FK.DemandItemPlan>>>>,
            Where<INTranSplit.docType, Equal<Required<INRegister.docType>>,
              And<INTranSplit.refNbr, Equal<Required<INRegister.refNbr>>>>>
                .Select(this, doc.DocType, doc.RefNbr))
            {
                INTran tran = res;
                INTranSplit split = res;
				INItemPlan plan = res;
                var plantype = INPlanType.PK.Find(this, plan.PlanType);

	            INLocation location = INLocation.PK.Find(this, split.LocationID);

                if (location != null && location.InclQtyAvail != true && plantype.ReplanOnEvent == INPlanConstants.Plan61)
                {
                    plantype = PXCache<INPlanType>.CreateCopy(plantype);
                    plantype.ReplanOnEvent = INPlanConstants.Plan60;
                }

                planlist.Add(new PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>(plan, res, res, plantype, res));

                if (string.IsNullOrEmpty(POReceiptNbr))
                {
                    POReceiptType = tran.POReceiptType;
                    POReceiptNbr = tran.POReceiptNbr;
                }
            }

            ProcessLinkedAllocation(planlist, POReceiptType, POReceiptNbr);
        }

        protected virtual void ProcessLinkedAllocation(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list, string poReceiptType, string poReceiptNbr)
        {
			var planlist = list?.ConvertAll(x => new PXResult<INItemPlan, INPlanType>(x, x));

			if (onBeforeSalesOrderProcessPOReceipt != null)
			{
				planlist = onBeforeSalesOrderProcessPOReceipt(this, planlist, poReceiptType, poReceiptNbr);
			}

			SOOrderEntry.ProcessPOReceipt(this, planlist, poReceiptType, poReceiptNbr);
        }

		private void ReattachPlanToTransit(INItemPlan plan, INTran tran, INTranSplit split, INItemSite itemsite, INSite site)
		{
			initemplan.Delete(plan);
			INTransitLine transitline = GetTransitLine(tran);
			plan = PXCache<INItemPlan>.CreateCopy(plan);
			plan.PlanType = INPlanConstants.Plan42;
			plan.PlanID = null;
			plan.SiteID = tran.ToSiteID;
			plan.RefNoteID = transitline.NoteID;
			plan.RefEntityType = typeof(INTransitLine).FullName;
			plan.LocationID = tran.ToLocationID ?? itemsite.DfltReceiptLocationID ?? site.ReceiptLocationID;
			plan = initemplan.Insert(plan);

			//should be refactored and removed - isfixedintransit should be moved to intransitline
			foreach (INItemPlan itemPlan in supplyplans.Select(split.PlanID))
			{
				INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(itemPlan);
				var demand_plantype = INPlanType.PK.Find(this, itemPlan.PlanType);

				//avoid ReadItem()
				initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);

				demand_plan.SupplyPlanID = plan.PlanID;

				if (demand_plantype.ReplanOnEvent == INPlanConstants.Plan95)
				{
					if (demand_plantype.PlanType == INPlanConstants.Plan93)
					{
						//Fixed Transfer for SO
						plan.PlanType = INPlanConstants.Plan44;
						plan = initemplan.Update(plan);

						split.IsFixedInTransit = true;
						transitline.IsFixedInTransit = true;
						intransitline.Update(transitline);
					}

					demand_plan.PlanType = demand_plantype.ReplanOnEvent;
					initemplan.Cache.Update(demand_plan);
				}
				else
				{
					initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Updated);
				}
			}
			split.PlanID = null;
			split.ToSiteID = tran.ToSiteID;
			UpdateSplitDestinationLocation(split, tran.ToLocationID ?? itemsite.DfltReceiptLocationID ?? site.ReceiptLocationID);

			intransplit.Cache.SetStatus(split, PXEntryStatus.Updated);
		}

        private void UpdateAdditionalCost(INTran ortran, INTranCost trancost)
        {
            if (ortran == null || (trancost.TranCost ?? 0m) == 0m)
                return;

            if (!(trancost.CostDocType == INDocType.Adjustment && trancost.Qty == 0m && trancost.InvtMult == 1))
                return;
            INTranSplitAdjustmentUpdate upd = new INTranSplitAdjustmentUpdate
            {
                DocType = ortran.DocType,
                RefNbr = ortran.RefNbr,
                LineNbr = ortran.LineNbr,
                CostSiteID = trancost.CostSiteID,
                LotSerialNbr = trancost.LotSerialNbr,
                CostSubItemID = trancost.CostSubItemID
            };

            upd = intransplitadjustmentupdate.Insert(upd);
            upd.AdditionalCost += trancost.TranCost;
        }

        public virtual INTran Copy(INTran tran, ReadOnlyCostStatus layer, InventoryItem item)
        {
            INTran newtran = new INTran();
            
            newtran.BranchID = tran.BranchID;
            newtran.DocType = tran.DocType;
            newtran.RefNbr = tran.RefNbr;
            newtran.TranType = INTranType.Adjustment;
            newtran.InventoryID = tran.InventoryID;
            newtran.SubItemID = tran.SubItemID;
            newtran.SiteID = tran.SiteID;
            newtran.LocationID = tran.LocationID;
            newtran.UOM = tran.UOM;
            newtran.Qty = 0m;
            newtran.AcctID = tran.AcctID;
            newtran.SubID = tran.SubID;
            newtran.COGSAcctID = tran.COGSAcctID;
            newtran.COGSSubID = tran.COGSSubID;
            if (layer != null)
            {
                newtran.InvtAcctID = layer.AccountID;
                newtran.InvtSubID = layer.SubID;
                newtran.OrigRefNbr = (item.ValMethod == INValMethod.FIFO || item.ValMethod == INValMethod.Specific) ? layer.ReceiptNbr : null;
                newtran.LotSerialNbr = (item.ValMethod == INValMethod.Specific) ? layer.LotSerialNbr : string.Empty;
            }
            else
            {
                newtran.InvtAcctID = null;
                newtran.InvtSubID = null;
                newtran.OrigRefNbr = null;
                newtran.LotSerialNbr = String.Empty;
            }
            newtran.POReceiptNbr = tran.POReceiptNbr;
            newtran.POReceiptLineNbr = tran.POReceiptLineNbr;
            newtran.ReasonCode = tran.ReasonCode;
			newtran.UnitCost = tran.UnitCost;
            return newtran;
        }

        public virtual void RegenerateInTranList(PXResultset<INTran, INTranSplit, InventoryItem> originalintranlist)
        {
            foreach (PXResult<INTran, INTranSplit, InventoryItem> res in originalintranlist)
            {
                INTran tran = res;
                INTranSplit split = res;
                InventoryItem item = res;

                decimal? accu_TranCost = 0m;
                decimal? accu_Qty = 0m;
                decimal? entiretrancost = tran.TranCost;

                ReadOnlyCostStatus prev_layer = null;

                INTran ortran = LandedCostAllocationService.Instance.GetOriginalInTran(this, tran.POReceiptNbr, tran.POReceiptLineNbr);

                if (ortran == null) continue;

                PXView costreceiptstatusview = GetReceiptStatusView(item);
				bool fifobreak = false; // used in case there is some old pre-valuation method change receipt statuses left for fifo item

                //there is no need of foreach anymore. It's one-to-one relation, left here for legacy.
                foreach (PXResult<ReadOnlyCostStatus, ReceiptStatus> layerres in
                            costreceiptstatusview.SelectMultiBound(new object[] { tran, split }, new object[] { ortran.InvtAcctID, ortran.InvtSubID }))
                {
                    ReadOnlyCostStatus layer = (ReadOnlyCostStatus)layerres;
                    ReceiptStatus receipt = (ReceiptStatus)layerres;

					if (fifobreak)
						break;

					if (item.ValMethod == INValMethod.FIFO && receipt != null && receipt.ReceiptNbr != null)
						fifobreak = true;

                    decimal? origqty = null;
                    decimal? qtyonhand = null;

                    switch (item.ValMethod)
                    {
                        case INValMethod.Average:
							if (layer.QtyOnHand == 0m && receipt.QtyOnHand != 0m)
							{
								origqty = layer.OrigQty;
								qtyonhand = layer.QtyOnHand;
							}
							else
							{
								origqty = receipt.OrigQty;
								qtyonhand = receipt.QtyOnHand;
							}
							break;
                        case INValMethod.Specific:
                            origqty = receipt.OrigQty;
                            qtyonhand = receipt.QtyOnHand;
                            break;
                        case INValMethod.FIFO:
                            origqty = layer.OrigQty;
                            qtyonhand = layer.QtyOnHand;
                            break;
                    }

                    if (qtyonhand > 0m)
                    {
                        prev_layer = layer;
                    }

					//PPV adjustment (Tax Adjustment) goes to expense in case resulting cost is negative
					bool isPPVTranOrTaxAdjustmentTran = inregister.Current != null && (inregister.Current.IsPPVTran == true || inregister.Current.IsTaxAdjustmentTran == true);
					if (isPPVTranOrTaxAdjustmentTran && entiretrancost < 0m &&
                        (layer.TotalCost + (qtyonhand == 0 ? 0 : (qtyonhand * entiretrancost / origqty))) < 0m)
                        break;

                    //inventory adjustment
                    if (qtyonhand != 0m)
                    {
                        INTran newtran = Copy(tran, layer, item);

                        newtran.InvtMult = (short)1;

                        decimal? newtranqty;
                        if (origqty != 0m)
                            newtranqty = qtyonhand < origqty ? qtyonhand : origqty;
                        else
                            newtranqty = qtyonhand;

                        newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(newtranqty * entiretrancost / origqty));

                        if (item.ValMethod == INValMethod.Specific)
                        {
                            newtran.TranCost += PXCurrencyAttribute.BaseRound(this, (accu_Qty + newtranqty) * entiretrancost / origqty - accu_TranCost - newtran.TranCost);
                        }
                        accu_TranCost += newtran.TranCost;
                        accu_Qty += newtranqty;

                        intranselect.Insert(newtran);
                    }

                    //cogs adjustment
                    if (qtyonhand < origqty && origqty != 0m)
                    {
                        INTran newtran = Copy(tran, layer, item);

                        newtran.InvtMult = (short)0;
                        decimal? newtranqty = origqty - qtyonhand;
                        newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(newtranqty * entiretrancost / origqty));

                        if (item.ValMethod == INValMethod.Specific)
                        {
                            newtran.TranCost += PXCurrencyAttribute.BaseRound(this, (accu_Qty + newtranqty) * entiretrancost / origqty - accu_TranCost - newtran.TranCost);
                        }
                        accu_TranCost += newtran.TranCost;
                        accu_Qty += newtranqty;

                        intranselect.Insert(newtran);
                    }
                }

                //Standard, Specific rounding and corrections for average-costed items with non-zero INReceiptStatus and zero INCostStatus
                if (entiretrancost - accu_TranCost != 0m)
                {
                    INTran newtran = Copy(tran, prev_layer, item);

                    newtran.InvtMult = (short)0;
                    newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(entiretrancost - accu_TranCost));

                    intranselect.Insert(newtran);
                }
                intranselect.Cache.SetStatus(tran, PXEntryStatus.Deleted);
            }
        }



        private void SetOriginalQty(INCostStatus layer)
        {
            if (layer.LayerType == INLayerType.Normal && layer.QtyOnHand > 0m)
            {
				layer.OrigQtyOnHand = layer.QtyOnHand;
				if (layer.ValMethod != INValMethod.Average)
				{
                layer.OrigQty = layer.QtyOnHand;
            }
        }
        }

        private void SetOriginalQty()
        {
            foreach (AverageCostStatus layer in averagecoststatus.Cache.Inserted)
                SetOriginalQty(layer);
            foreach (SpecificCostStatus layer in specificcoststatus.Cache.Inserted)
                SetOriginalQty(layer);
            foreach (SpecificTransitCostStatus layer in specifictransitcoststatus.Cache.Inserted)
                SetOriginalQty(layer);
            foreach (FIFOCostStatus layer in fifocoststatus.Cache.Inserted)
                SetOriginalQty(layer);
        }

        public virtual void ReplanBackOrders()
        {
            ReplanBackOrders(this, false);
        }

        public static void ReplanBackOrders(PXGraph graph)
        {
            ReplanBackOrders(graph, true);
        }

        public static void ReplanBackOrders(PXGraph graph, bool ForceReplan)
        {
            List<INItemPlan> replan = new List<INItemPlan>();
            foreach (SiteStatus layer in graph.Caches[typeof(SiteStatus)].Inserted)
            {
                //for inventory release the difference between QtyOnHand and QtyAvail is contributed by the locations excluded from Available Qty.
                //thus need to check both values
                if (layer.QtyOnHand <= 0m && layer.QtyAvail <= 0m && !ForceReplan)
                    continue;
	            SiteStatus dbLayer = SiteStatus.PK.Find(graph, layer.InventoryID, layer.SubItemID, layer.SiteID);

                decimal qtyAvail =
                    layer.QtyOnHand.Value
                    - layer.QtyNotAvail.Value
                    - layer.QtySOShipped.Value
                    - layer.QtySOShipping.Value
                    - layer.QtyINIssues.Value
                    - layer.QtyINAssemblyDemand.Value
                    - layer.QtyProductionAllocated.Value
                    - layer.QtyFSSrvOrdAllocated.Value
                    +
                    (
                        (dbLayer != null)
                            ? (dbLayer.QtyOnHand.Value
                               - dbLayer.QtyNotAvail.Value
                               - dbLayer.QtySOShipped.Value
                               - dbLayer.QtySOShipping.Value
                               - dbLayer.QtyINIssues.Value
                               - dbLayer.QtyINAssemblyDemand.Value
                               - dbLayer.QtyProductionAllocated.Value
                               - dbLayer.QtyFSSrvOrdAllocated.Value)
                            : 0m);

                if (qtyAvail > 0m)
                {
					foreach (PXResult<INItemPlan, SOOrder, SOOrderType, SOLineSplit> res in PXSelectJoin<INItemPlan,
                        InnerJoin<SOOrder, On<SOOrder.noteID, Equal<INItemPlan.refNoteID>>,
						InnerJoin<SOOrderType, On<SOOrder.FK.OrderType>,
						InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>>>>,
                        Where<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>,
                                And<INItemPlan.subItemID, Equal<Required<INItemPlan.subItemID>>,
                                    And<INItemPlan.siteID, Equal<Required<INItemPlan.siteID>>,
                                And<SOOrderType.requireAllocation, Equal<False>,
							And<INItemPlan.planType, In3<INPlanConstants.plan60, INPlanConstants.plan68>>>>>>,
                        OrderBy<Asc<INItemPlan.planDate, Asc<INItemPlan.planType, Desc<INItemPlan.planQty>>>>>
						.Select(graph, layer.InventoryID, layer.SubItemID, layer.SiteID))
                    {
						INItemPlan plan = res;
						SOLineSplit split = res;
						decimal plannedQty = split.BaseQty.Value - split.BaseShippedQty.Value;

                        if (plannedQty <= qtyAvail)
                        {
                            qtyAvail -= plannedQty;
                            if (plan.PlanType == INPlanConstants.Plan68)
                                replan.Add(plan);
                        }
                        else if (qtyAvail > 0)
                        {
                            if (plan.PlanType == INPlanConstants.Plan68)
                            {
                                SOLine soLine = PXSelectJoin<SOLine,
                                InnerJoin<SOLineSplit, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>>,
                                Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>>>.Select(graph, plan.PlanID);

                                if (soLine != null && soLine.ShipComplete != SOShipComplete.ShipComplete)
                                {
                                    replan.Add(plan);
                                    qtyAvail = 0m;
                                }
                            }
                            else
                            {
                                qtyAvail = 0m;
                            }
                        }

                        if (qtyAvail <= 0m)
                            break;
                    }
                }
            }
            PXCache plancache = graph.Caches[typeof(INItemPlan)];
            foreach (INItemPlan plan in replan)
            {
                INItemPlan upd = PXCache<INItemPlan>.CreateCopy(plan);
                upd.PlanType = INPlanConstants.Plan60;
                plancache.Update(upd);
            }
        }

		public virtual void ValidateTran(INRegister doc, INTran tran, INTranSplit split, InventoryItem item, INSite site)
		{
			if (site.Active != true)
				throw new PXException(Messages.InactiveWarehouse, site.SiteCD);

			ValidateTran(tran);

			if (split.LotSerialNbr == Messages.Unassigned)	// crutch for AC-97716
			{
				RaiseUnassignedQtyNotZeroException(tran);
			}

			if (doc.DocType == INDocType.Transfer && tran.InvtMult == -1
				&& (doc.SiteID != tran.SiteID || doc.ToSiteID != tran.ToSiteID
					|| doc.SiteID != split.SiteID || doc.ToSiteID != split.ToSiteID))
			{
				// AC-139602. May occur during excel import.
				throw new PXException(Messages.TransferDocumentIsCorrupted, tran.LineNbr);
			}

			if (split.RefNbr == null && tran.DocType != INDocType.Adjustment && tran.InvtMult != 0 && tran.Qty != 0m && item.StkItem == true)
			{
				throw new PXException(Messages.CannotReleaseAllocationsMissing,
					intranselect.Cache.GetValueExt<INTran.inventoryID>(tran),
					intranselect.Cache.GetValueExt<INTran.siteID>(tran),
					intranselect.Cache.GetValueExt<INTran.lineNbr>(tran));
			}
		}

        public virtual void ValidateTran(INTran tran)
        {
            if (tran.UnassignedQty != 0)
            {
                RaiseUnassignedQtyNotZeroException(tran);
            }
        }

        public virtual void RaiseUnassignedQtyNotZeroException(INTran tran)
        {
            InventoryItem item = InventoryItem.PK.Find(this, tran.InventoryID);

            throw new PXException(Messages.BinLotSerialNotAssignedWithItemCode, item?.InventoryCD);
        }

	    protected virtual void ValidateFinPeriod(INRegister doc, IEnumerable<PXResult<INTran, InventoryItem, INSite>> records)
	    {
		    Func<PXResult<INTran, InventoryItem, INSite>, int?[]> getBranchIDs = null;

			if (doc.DocType == INDocType.Transfer && doc.TransferType == INTransferType.OneStep)
			{
				INSite siteTo = INSite.PK.Find(this, doc.ToSiteID);

				getBranchIDs = row => new int?[]
				{
					((INTran) row).BranchID,
					((INSite) row).BranchID,
					siteTo.BranchID
				};
			}
			else
			{
				getBranchIDs = row => new int?[]
				{
					((INTran) row).BranchID,
					((INSite) row).BranchID,
				};
			}

			FinPeriodUtils.ValidateFinPeriod<PXResult<INTran, InventoryItem, INSite>>(
			    records,
			    row => ((INTran)row).FinPeriodID,
			    getBranchIDs,
			    typeof(OrganizationFinPeriod.iNClosed));
		}
	}
}
namespace PX.Objects.IN
{
    public interface ICostStatus
    {
        decimal? QtyOnHand { get; set; }
        decimal? TotalCost { get; set; }
    }

    public interface IStatus
    {
        decimal? QtyOnHand { get; set; }
        decimal? QtyAvail { get; set; }
        decimal? QtyNotAvail { get; set; }
        decimal? QtyExpired { get; set; }
        decimal? QtyHardAvail { get; set; }
		decimal? QtyActual { get; set; }
        decimal? QtyINIssues { get; set; }
        decimal? QtyINReceipts { get; set; }
        decimal? QtyInTransit { get; set; }
        decimal? QtyPOPrepared { get; set; }
        decimal? QtyPOOrders { get; set; }
        decimal? QtyPOReceipts { get; set; }

        decimal? QtyFSSrvOrdPrepared { get; set; }
        decimal? QtyFSSrvOrdBooked { get; set; }
        decimal? QtyFSSrvOrdAllocated { get; set; }


        decimal? QtySOBackOrdered { get; set; }
        decimal? QtySOPrepared { get; set; }
        decimal? QtySOBooked { get; set; }
        decimal? QtySOShipped { get; set; }
        decimal? QtySOShipping { get; set; }
        decimal? QtyINAssemblySupply { get; set; }
        decimal? QtyINAssemblyDemand { get; set; }
        decimal? QtyInTransitToProduction { get; set; }
        decimal? QtyProductionSupplyPrepared { get; set; }
        decimal? QtyProductionSupply { get; set; }
        decimal? QtyPOFixedProductionPrepared { get; set; }
        decimal? QtyPOFixedProductionOrders { get; set; }
        decimal? QtyProductionDemandPrepared { get; set; }
        decimal? QtyProductionDemand { get; set; }
        decimal? QtyProductionAllocated { get; set; }
        decimal? QtySOFixedProduction { get; set; }
        decimal? QtyProdFixedPurchase { get; set; }
        decimal? QtyProdFixedProduction { get; set; }
        decimal? QtyProdFixedProdOrdersPrepared { get; set; }
        decimal? QtyProdFixedProdOrders { get; set; }
        decimal? QtyProdFixedSalesOrdersPrepared { get; set; }
        decimal? QtyProdFixedSalesOrders { get; set; }

        decimal? QtyFixedFSSrvOrd { get; set; }
        decimal? QtyPOFixedFSSrvOrd { get; set; }
        decimal? QtyPOFixedFSSrvOrdPrepared { get; set; }
        decimal? QtyPOFixedFSSrvOrdReceipts { get; set; }

        decimal? QtySOFixed { get; set; }
        decimal? QtyPOFixedOrders { get; set; }
        decimal? QtyPOFixedPrepared { get; set; }
        decimal? QtyPOFixedReceipts { get; set; }
        decimal? QtySODropShip { get; set; }
        decimal? QtyPODropShipOrders { get; set; }
        decimal? QtyPODropShipPrepared { get; set; }
        decimal? QtyPODropShipReceipts { get; set; }
        decimal? QtyInTransitToSO { get; set; }
        decimal? QtyINReplaned { get; set; }
    }
}

namespace PX.Objects.IN.Overrides.INDocumentRelease
{
	public static class PXCacheExtensions
	{
		public static void PersistInserted(this PXCache cache)
		{
			foreach (object item in cache.Inserted)
			{
				try
				{
					cache.PersistInserted(item);
				}
				finally
				{
					cache.ResetPersisted(item);
					var interceptor = cache.Interceptor as StatusAccumulatorAttribute;
					if (interceptor != null)
					{
						interceptor.ResetPersisted(item);
					}
				}
			}
		}
	}

    public interface IQtyAllocated : IQtyAllocatedBase
    {
        decimal? QtyINIssues { get; set; }
        decimal? QtyINReceipts { get; set; }
        decimal? QtyPOPrepared { get; set; }
        decimal? QtyPOOrders { get; set; }
        decimal? QtyPOReceipts { get; set; }

        decimal? QtyFSSrvOrdPrepared { get; set; }
        decimal? QtyFSSrvOrdBooked { get; set; }
        decimal? QtyFSSrvOrdAllocated { get; set; }

        decimal? QtySOBackOrdered { get; set; }
        decimal? QtySOPrepared { get; set; }
        decimal? QtySOBooked { get; set; }
        decimal? QtySOShipped { get; set; }
        decimal? QtySOShipping { get; set; }
        decimal? QtyINAssemblySupply { get; set; }
        decimal? QtyINAssemblyDemand { get; set; }
        decimal? QtyInTransitToProduction { get; set; }
        decimal? QtyProductionSupplyPrepared { get; set; }
        decimal? QtyProductionSupply { get; set; }
        decimal? QtyPOFixedProductionPrepared { get; set; }
        decimal? QtyPOFixedProductionOrders { get; set; }
        decimal? QtyProductionDemandPrepared { get; set; }
        decimal? QtyProductionDemand { get; set; }
        decimal? QtyProductionAllocated { get; set; }
        decimal? QtySOFixedProduction { get; set; }
        decimal? QtyProdFixedPurchase { get; set; }
        decimal? QtyProdFixedProduction { get; set; }
        decimal? QtyProdFixedProdOrdersPrepared { get; set; }
        decimal? QtyProdFixedProdOrders { get; set; }
        decimal? QtyProdFixedSalesOrdersPrepared { get; set; }
        decimal? QtyProdFixedSalesOrders { get; set; }
        decimal? QtyINReplaned { get; set; }

        decimal? QtyFixedFSSrvOrd { get; set; }
        decimal? QtyPOFixedFSSrvOrd { get; set; }
        decimal? QtyPOFixedFSSrvOrdPrepared { get; set; }
        decimal? QtyPOFixedFSSrvOrdReceipts { get; set; }

        decimal? QtySOFixed { get; set; }
        decimal? QtyPOFixedOrders { get; set; }
        decimal? QtyPOFixedPrepared { get; set; }
        decimal? QtyPOFixedReceipts { get; set; }
        decimal? QtySODropShip { get; set; }
        decimal? QtyPODropShipOrders { get; set; }
        decimal? QtyPODropShipPrepared { get; set; }
        decimal? QtyPODropShipReceipts { get; set; }
        decimal? QtyInTransitToSO { get; set; }
    }

    public interface IQtyAllocatedBase
    {
        bool? NegQty { get; }
        bool? InclQtyAvail { get; }

        bool? InclQtyFSSrvOrdPrepared { get; }
        bool? InclQtyFSSrvOrdBooked { get; }
        bool? InclQtyFSSrvOrdAllocated { get; }

        bool? InclQtySOReverse { get; }
        bool? InclQtySOBackOrdered { get; }
        bool? InclQtySOPrepared { get; }
        bool? InclQtySOBooked { get; }
        bool? InclQtySOShipped { get; }
        bool? InclQtySOShipping { get; }
        bool? InclQtyPOPrepared { get; }
        bool? InclQtyPOOrders { get; }
        bool? InclQtyPOReceipts { get; }
        bool? InclQtyInTransit { get; }
        bool? InclQtyINIssues { get; }
        bool? InclQtyINReceipts { get; }
        bool? InclQtyPOFixedReceipt { get; }
        bool? InclQtyINAssemblySupply { get; }
        bool? InclQtyINAssemblyDemand { get; }
        bool? InclQtyProductionDemandPrepared { get; }
        bool? InclQtyProductionDemand { get; }
        bool? InclQtyProductionAllocated { get; }
        bool? InclQtyProductionSupplyPrepared { get; }
        bool? InclQtyProductionSupply { get; }
        decimal? QtyOnHand { get; set; }
        decimal? QtyAvail { get; set; }
        decimal? QtyHardAvail { get; set; }
		decimal? QtyActual { get; set; }
        decimal? QtyNotAvail { get; set; }
        decimal? QtyInTransit { get; set; }
    }

	public interface IQtyAllocatedSeparateReceipts : IQtyAllocatedBase
	{
		decimal? QtyOnReceipt { get; set; }
	}

    [ItemStatsAccumulator()]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class ItemStats : INItemStats
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
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Search<InventoryItem.valMethod, Where<InventoryItem.inventoryID, Equal<Current<ItemStats.inventoryID>>>>))]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
    }

    public class INTranAccumAttribute : PXAccumulatorAttribute
    {
        public INTranAccumAttribute()
        {
            this.SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.UpdateOnly = true;
            columns.Update<INTranUpdate.tranCost>(((INTranUpdate)row).TranCost, PXDataFieldAssign.AssignBehavior.Summarize);

            return true;
        }
    }

    [INTranAccum(BqlTable = typeof(INTran))]
    [Serializable]
    public partial class INTranUpdate : IBqlTable
    {
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        [PXDBString(1, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public virtual String DocType
        {
            get;
            set;
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public virtual String RefNbr
        {
            get;
            set;
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? LineNbr
        {
            get;
            set;
        }
        #endregion
        #region TranCost
        public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TranCost
        {
            get;
            set;
        }
		#endregion
		#region LastModifiedByID
	    [PXDBLastModifiedByID]
	    public virtual Guid? LastModifiedByID { get; set; }
	    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
	    #endregion
	    #region LastModifiedByScreenID
	    [PXDBLastModifiedByScreenID]
	    public virtual String LastModifiedByScreenID { get; set; }
	    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
	    [PXDBLastModifiedDateTime]
	    public virtual DateTime? LastModifiedDateTime { get; set; }
	    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
	}

	public class INTranSplitAdjustmentAccumAttribute : PXAccumulatorAttribute
    {
        public INTranSplitAdjustmentAccumAttribute()
        {
            this.SingleRecord = false;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }
            var crow = (INTranSplitAdjustmentUpdate)row;

            columns.UpdateOnly = true;
            columns.Update<INTranSplitAdjustmentUpdate.additionalCost>(crow.AdditionalCost, PXDataFieldAssign.AssignBehavior.Summarize);
            if (String.IsNullOrEmpty(crow.LotSerialNbr))
            {
                columns.Remove<INTranSplitAdjustmentUpdate.lotSerialNbr>();
            }
            return true;
        }
    }

    [INTranSplitAdjustmentAccum(BqlTable = typeof(INTranSplit))]
    [Serializable]
    public partial class INTranSplitAdjustmentUpdate : IBqlTable
    {
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        [PXDBString(1, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public virtual String DocType
        {
            get;
            set;
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public virtual String RefNbr
        {
            get;
            set;
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? LineNbr
        {
            get;
            set;
        }
        #endregion
        #region CostSiteID
        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? CostSiteID
        {
            get;
            set;
        }
        #endregion
        #region CostSubItemID
        public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? CostSubItemID
        {
            get;
            set;
        }
        #endregion
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsKey = true, IsUnicode = true)]
        public virtual String LotSerialNbr
        {
            get;
            set;
        }
        #endregion
        #region AdditionalCost
        public abstract class additionalCost : PX.Data.BQL.BqlDecimal.Field<additionalCost> { }
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? AdditionalCost
        {
            get;
            set;
        }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
	}

	public class INTranCostUpdateAccum : PXAccumulatorAttribute
    {
        public INTranCostUpdateAccum()
        {
            this.SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            var itcrow = (INTranCostUpdate)row;

            columns.UpdateOnly = true;
            if (itcrow.ResetOversoldFlag ?? false)
                columns.Update<INTranCostUpdate.isOversold>(false, PXDataFieldAssign.AssignBehavior.Replace);

            columns.Update<INTranCostUpdate.oversoldQty>(itcrow.OversoldQty, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<INTranCostUpdate.oversoldTranCost>(itcrow.OversoldTranCost, PXDataFieldAssign.AssignBehavior.Summarize);
            if (itcrow.ResetOversoldFlag ?? false)
                columns.Restrict<INTranCostUpdate.oversoldQty>(PXComp.EQ, -itcrow.OversoldQty);
            else
                columns.Restrict<INTranCostUpdate.oversoldQty>(PXComp.GE, -itcrow.OversoldQty);
            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
			}
            catch (PXLockViolationException)
            {

				var it = INTran.PK.Find(sender.Graph, ((INTranCost)row).DocType, ((INTranCost)row).RefNbr, ((INTranCost)row).LineNbr);
                
                throw new PXRestartOperationException(new PXException(String.Format(Messages.INTranCostOverReceipted
                    , sender.Graph.Caches[typeof(INTran)].GetValueExt<INTran.inventoryID>(it)
                    , sender.Graph.Caches[typeof(INTran)].GetValueExt<INTran.subItemID>(it))));
            }
        }
    }

    [INTranCostUpdateAccum(BqlTable = typeof(INTranCost))]
    [Serializable]
    public partial class INTranCostUpdate : IBqlTable
    {
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
		{
		}
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        protected String _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public virtual String RefNbr
        {
            get;
            set;
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? LineNbr
        {
            get;
            set;
        }
        #endregion
        #region CostDocType
        public abstract class costDocType : PX.Data.BQL.BqlString.Field<costDocType> { }
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault()]
        public virtual String CostDocType
        {
            get;
            set;
        }
        #endregion
        #region CostRefNbr
        public abstract class costRefNbr : PX.Data.BQL.BqlString.Field<costRefNbr> { }
        protected String _CostRefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public virtual String CostRefNbr
        {
            get;
            set;
        }
        #endregion
        #region CostID
        public abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        protected Int64? _CostID;
        [PXDBLong(IsKey = true)]
        [PXDefault()]
        public virtual Int64? CostID
        {
            get;
            set;
        }
        #endregion
        #region ResetOversoldFlag
        public abstract class resetOversoldFlag : PX.Data.BQL.BqlBool.Field<resetOversoldFlag> { }
        [PXBool()]
        [PXDefault(false)]
        public virtual bool? ResetOversoldFlag
        {
            get;
            set;
        }
        #endregion
        #region IsOversold
        public abstract class isOversold : PX.Data.BQL.BqlBool.Field<isOversold> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual bool? IsOversold
        {
            get;
            set;
        }
        #endregion
        #region OversoldQty
        public abstract class oversoldQty : PX.Data.BQL.BqlDecimal.Field<oversoldQty> { }
        [PXDBDecimal(6)]
        [PXDefault(typeof(decimal0), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? OversoldQty
        {
            get;
            set;
        }
        #endregion
        #region OversoldTranCost
        public abstract class oversoldTranCost : PX.Data.BQL.BqlDecimal.Field<oversoldTranCost> { }
        [PXDBBaseCury()]
        [PXDefault(typeof(decimal0), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? OversoldTranCost
        {
            get;
            set;
        }
		#endregion
		#region LastModifiedByID
	    [PXDBLastModifiedByID]
	    public virtual Guid? LastModifiedByID { get; set; }
	    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
	    #endregion
	    #region LastModifiedByScreenID
	    [PXDBLastModifiedByScreenID]
	    public virtual String LastModifiedByScreenID { get; set; }
	    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
	    #endregion
	    #region LastModifiedDateTime
	    [PXDBLastModifiedDateTime]
	    public virtual DateTime? LastModifiedDateTime { get; set; }
	    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
	    #endregion
	}

	public class INTranSplitAccumAttribute : PXAccumulatorAttribute
    {
        public INTranSplitAccumAttribute()
        {
            this.SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.UpdateOnly = true;
            columns.Update<INTranSplitUpdate.totalQty>(((INTranSplitUpdate)row).TotalQty, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<INTranSplitUpdate.totalCost>(((INTranSplitUpdate)row).TotalCost, PXDataFieldAssign.AssignBehavior.Summarize);
            //only valid for transfers, so planid must be null
			var upd = (INTranSplitUpdate)row;
			if (upd.ResetPlanID == true)
            columns.Update<INTranSplitUpdate.planID>(((INTranSplitUpdate)row).PlanID, PXDataFieldAssign.AssignBehavior.Nullout);

            return true;
        }
    }

    [INTranSplitAccum(BqlTable = typeof(INTranSplit))]
    [Serializable]
    public partial class INTranSplitUpdate : IBqlTable
    {
        #region TranType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        [PXDBString(1, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public virtual String DocType
        {
            get;
            set;
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public virtual String RefNbr
        {
            get;
            set;
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? LineNbr
        {
            get;
            set;
        }
        #endregion
        #region CostSiteID
        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? CostSiteID
        {
            get;
            set;
        }
        #endregion
        #region CostSubItemID
        public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? CostSubItemID
        {
            get;
            set;
        }
        #endregion
        #region TotalQty
        public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        public virtual Decimal? TotalCost
        {
            get;
            set;
        }
        #endregion
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
        protected Int64? _PlanID;
        [PXDBLong()]
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

		#region ResetPlanID
		public abstract class resetPlanID : PX.Data.BQL.BqlBool.Field<resetPlanID> { }
		protected bool? _ResetPlanID;
		[PXBool()]
		public virtual bool? ResetPlanID
		{
			get
			{
				return this._ResetPlanID;
			}
			set
			{
				this._ResetPlanID = value;
			}
		}
		#endregion
	    #region LastModifiedByID
	    [PXDBLastModifiedByID]
	    public virtual Guid? LastModifiedByID { get; set; }
	    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
	    #endregion
	    #region LastModifiedByScreenID
	    [PXDBLastModifiedByScreenID]
	    public virtual String LastModifiedByScreenID { get; set; }
	    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
	    #endregion
	    #region LastModifiedDateTime
	    [PXDBLastModifiedDateTime]
	    public virtual DateTime? LastModifiedDateTime { get; set; }
	    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
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

    public class ARTranAccum : PXAccumulatorAttribute
    {
        public ARTranAccum()
        {
            this.SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.UpdateOnly = true;
            columns.Update<ARTranUpdate.tranCost>(((ARTranUpdate)row).TranCost, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ARTranUpdate.isTranCostFinal>(((ARTranUpdate)row).IsTranCostFinal, PXDataFieldAssign.AssignBehavior.Replace);

            return true;
        }
    }

    [ARTranAccum(BqlTable = typeof(AR.ARTran))]
    [Serializable]
    public partial class ARTranUpdate : IBqlTable
    {
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        protected String _TranType;
        [PXDBString(3, IsKey = true, IsFixed = true)]
        [PXDefault()]
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
        [PXDefault()]
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
        [PXDefault()]
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
        #region TranCost
        public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
        protected Decimal? _TranCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TranCost
        {
            get
            {
                return this._TranCost;
            }
            set
            {
                this._TranCost = value;
            }
        }
        #endregion
        #region IsTranCostFinal
        public abstract class isTranCostFinal : PX.Data.BQL.BqlBool.Field<isTranCostFinal> { }
        protected Boolean? _IsTranCostFinal;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsTranCostFinal
        {
            get
            {
                return this._IsTranCostFinal;
            }
            set
            {
                this._IsTranCostFinal = value;
            }
        }
		#endregion
		#region LastModifiedByID
	    [PXDBLastModifiedByID]
	    public virtual Guid? LastModifiedByID { get; set; }
	    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
	    #endregion
	    #region LastModifiedByScreenID
	    [PXDBLastModifiedByScreenID]
	    public virtual String LastModifiedByScreenID { get; set; }
	    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
	    #endregion
	    #region LastModifiedDateTime
	    [PXDBLastModifiedDateTime]
	    public virtual DateTime? LastModifiedDateTime { get; set; }
	    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
	    #endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
    }

	[POReceiptLineUpdate.Accumulator(BqlTable = typeof(PO.POReceiptLine))]
	[PXHidden]
	[Serializable]
	public partial class POReceiptLineUpdate : IBqlTable
	{
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		public virtual string ReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region INReleased
		public abstract class iNReleased : PX.Data.BQL.BqlBool.Field<iNReleased>
        {
		}
		[PXDBBool, PXDefault(false)]
		public virtual Boolean? INReleased
		{
			get;
			set;
		}
		#endregion
		#region UpdateTranCostFinal
		public abstract class updateTranCostFinal : PX.Data.BQL.BqlBool.Field<updateTranCostFinal>
        {
		}
		[PXBool, PXDefault(false)]
		public virtual Boolean? UpdateTranCostFinal
		{
			get;
			set;
		}
		#endregion
		#region TranCostFinal
		public abstract class tranCostFinal : PX.Data.BQL.BqlDecimal.Field<tranCostFinal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranCostFinal
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				this.SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(sender, row, columns))
				{
					return false;
				}

				var returnRow = (POReceiptLineUpdate)row;

				columns.UpdateOnly = true;
				columns.Update<POReceiptLineUpdate.iNReleased>(returnRow.INReleased, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POReceiptLineUpdate.tranCostFinal>(returnRow.TranCostFinal,
					(returnRow.UpdateTranCostFinal == true) ? PXDataFieldAssign.AssignBehavior.Replace : PXDataFieldAssign.AssignBehavior.Initialize);
				columns.Update<POReceiptLineUpdate.lastModifiedByID>(returnRow.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POReceiptLineUpdate.lastModifiedDateTime>(returnRow.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POReceiptLineUpdate.lastModifiedByScreenID>(returnRow.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);

				return true;
			}
		}
	}

    public class SOShipLineAccum : PXAccumulatorAttribute
    {
        public SOShipLineAccum()
        {
            this.SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.UpdateOnly = true;
            columns.Update<SOShipLineUpdate.unitCost>(((SOShipLineUpdate)row).UnitCost, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<SOShipLineUpdate.extCost>(((SOShipLineUpdate)row).ExtCost, PXDataFieldAssign.AssignBehavior.Replace);

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                return false;
            }
        }
    }

    [SOShipLineAccum(BqlTable = typeof(SO.SOShipLine))]
    [Serializable]
    public partial class SOShipLineUpdate : IBqlTable
    {
        #region ShipmentNbr
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        protected String _ShipmentNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        public virtual String ShipmentNbr
        {
            get
            {
                return this._ShipmentNbr;
            }
            set
            {
                this._ShipmentNbr = value;
            }
        }
        #endregion
        #region ShipmentType
        public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
        protected String _ShipmentType;
        [PXDBString(1, IsFixed = true, IsKey = true)]
        public virtual String ShipmentType
        {
            get
            {
                return this._ShipmentType;
            }
            set
            {
                this._ShipmentType = value;
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
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        protected Decimal? _UnitCost;
        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnitCost
        {
            get
            {
                return this._UnitCost;
            }
            set
            {
                this._UnitCost = value;
            }
        }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }
        protected Decimal? _ExtCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
		#region LastModifiedByID
	    [PXDBLastModifiedByID]
	    public virtual Guid? LastModifiedByID { get; set; }
	    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
	    #endregion
	    #region LastModifiedByScreenID
	    [PXDBLastModifiedByScreenID]
	    public virtual String LastModifiedByScreenID { get; set; }
	    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
	    #endregion
	    #region LastModifiedDateTime
	    [PXDBLastModifiedDateTime]
	    public virtual DateTime? LastModifiedDateTime { get; set; }
	    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
	    #endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
    }

    [SiteStatusAccumulator]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class SiteStatus : INSiteStatus, IQtyAllocated
	{
		#region Keys
		public new class PK : PrimaryKeyOf<SiteStatus>.By<inventoryID, subItemID, siteID>
		{
			public static SiteStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID)
				=> FindBy(graph, inventoryID, subItemID, siteID);
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
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
		[SubItem(IsKey = true)]
		[PXForeignSelector(typeof(INTran.subItemID))]
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
		//[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.siteID))]
		[IN.Site(IsKey = true)]
		[PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
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
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID>
		{
		}
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<InventoryItem.lotSerClassID, Where<InventoryItem.inventoryID, Equal<Current<SiteStatus.inventoryID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string LotSerClassID
		{
			get;
			set;
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
		protected bool? _NegQty;
		[PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region NegAvailQty
		public abstract class negAvailQty : PX.Data.BQL.BqlBool.Field<negAvailQty> { }
		protected bool? _NegAvailQty;
		[PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), CacheGlobal = true, SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? NegAvailQty
        {
            get
            {
                return this._NegAvailQty;
            }
            set
            {
                this._NegAvailQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
		#endregion
		#region InitSiteStatus
		public abstract class initSiteStatus : PX.Data.BQL.BqlBool.Field<initSiteStatus> { }
		protected bool? _InitSiteStatus;
		[PXBool()]
		public virtual bool? InitSiteStatus
		{
			get
			{
				return this._InitSiteStatus;
			}
			set
			{
				this._InitSiteStatus = value;
			}
		}
		#endregion
		#region PersistEvenZero
		public abstract class persistEvenZero : PX.Data.IBqlField
		{
		}

		[PXBool()]
		public virtual bool? PersistEvenZero
		{
			get;
			set;
		}
        #endregion

        #region SkipQtyValidation
        [PXBool, PXUnboundDefault(false)]
        public virtual Boolean? SkipQtyValidation { get; set; }
        public abstract class skipQtyValidation : PX.Data.BQL.BqlBool.Field<skipQtyValidation> { }
        #endregion
    }


    [TransitSiteStatusAccumulator]
    [Serializable]
    public partial class TransitSiteStatus : INSiteStatus, IQtyAllocated
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<TransitSiteStatus>.By<inventoryID, subItemID, siteID>
	    {
		    public static TransitSiteStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID)
			    => FindBy(graph, inventoryID, subItemID, siteID);
	    }
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
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
        [SubItem(IsKey = true)]
        [PXForeignSelector(typeof(INTran.subItemID))]
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
        //[PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.siteID))]
        [IN.Site(true, IsKey = true)]
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
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region NegAvailQty
        public abstract class negAvailQty : PX.Data.BQL.BqlBool.Field<negAvailQty> { }
        protected bool? _NegAvailQty;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<TransitSiteStatus.itemClassID>>>>), CacheGlobal = true, SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? NegAvailQty
        {
            get
            {
                return this._NegAvailQty;
            }
            set
            {
                this._NegAvailQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<TransitSiteStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitSiteStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion
    }

    [TransitLocationStatusAccumulator]
    [Serializable]
    public partial class TransitLocationStatus : INLocationStatus, IQtyAllocated, ICostStatus
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<TransitLocationStatus>.By<inventoryID, subItemID, siteID, locationID>
	    {
		    public static TransitLocationStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID)
			    => FindBy(graph, inventoryID, subItemID, siteID, locationID);
	    }
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
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
        [SubItem(IsKey = true)]
        [PXForeignSelector(typeof(INTran.subItemID))]
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
        //[PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.siteID))]
        [IN.Site(true, IsKey = true)]
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
        [PXForeignSelector(typeof(INTran.locationID))]
		[PXDBDefault(typeof(INTransitLine.costSiteID))]
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
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
        protected Decimal? _TotalCost;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? TotalCost
        {
            get
            {
                return this._TotalCost;
            }
            set
            {
                this._TotalCost = value;
            }
        }
        #endregion
        #region QtyNotAvail
        public new abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(typeof(Search<INLocation.inclQtyAvail, Where<INLocation.locationID, Equal<Current<TransitLocationStatus.locationID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<TransitLocationStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<TransitLocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion
    }

    [LocationStatusAccumulator]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class LocationStatus : INLocationStatus, IQtyAllocated, ICostStatus
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<LocationStatus>.By<inventoryID, subItemID, siteID, locationID>
	    {
		    public static LocationStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID)
			    => FindBy(graph, inventoryID, subItemID, siteID, locationID);
	    }
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
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
        [SubItem(IsKey = true)]
        [PXForeignSelector(typeof(INTran.subItemID))]
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
        //[PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.siteID))]
        [IN.Site(IsKey = true)]
        [PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
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
        [IN.Location(IsKey = true, ValidComboRequired = false)]
        [PXForeignSelector(typeof(INTran.locationID))]
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
		#region ItemClassID
	    public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
	    [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
		{
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
        protected Decimal? _TotalCost;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? TotalCost
        {
            get
            {
                return this._TotalCost;
            }
            set
            {
                this._TotalCost = value;
            }
        }
        #endregion
        #region QtyNotAvail
        public new abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(typeof(Search<INLocation.inclQtyAvail, Where<INLocation.locationID, Equal<Current<LocationStatus.locationID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion
		#region RelatedPIID
		public abstract class relatedPIID : PX.Data.BQL.BqlString.Field<relatedPIID> { }
		[PXString(IsUnicode = true)]
		public virtual String RelatedPIID
		{
			get;
			set;
		}
		#endregion

        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LocationStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
		#region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion

        #region SkipQtyValidation
        [PXBool, PXUnboundDefault(false)]
        public virtual Boolean? SkipQtyValidation { get; set; }
        public abstract class skipQtyValidation : PX.Data.BQL.BqlBool.Field<skipQtyValidation> { }
        #endregion
    }

    [LotSerialStatusAccumulator]
    [Serializable]
    public partial class LotSerialStatus : INLotSerialStatus, IQtyAllocated
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<LotSerialStatus>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr>
	    {
		    public static LotSerialStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr)
			    => FindBy(graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr);
	    }
		public static new class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<LotSerialStatus>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<LotSerialStatus>.By<inventoryID, subItemID, siteID, locationID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<LotSerialStatus>.By<subItemID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<LotSerialStatus>.By<inventoryID> { }
			public class ItemLotSerial : INItemLotSerial.PK.ForeignKeyOf<LotSerialStatus>.By<inventoryID, lotSerialNbr> { }
		}
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
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
        [SubItem(IsKey = true)]
        [PXForeignSelector(typeof(INTran.subItemID))]
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
        //[PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.siteID))]
        [IN.Site(true, IsKey = true)]
        [PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
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
        [IN.Location(IsKey = true, ValidComboRequired = false)]
        [PXForeignSelector(typeof(INTran.locationID))]
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
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String LotSerialNbr
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
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [PXDBLong()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region LotSerClassID
        public abstract class lotSerClassID : IBqlField { }
        [PXString(10, IsUnicode = true)]
        [PXDefault(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<LotSerialStatus.inventoryID>>>>),
            CacheGlobal = true, SourceField = typeof(InventoryItem.lotSerClassID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string LotSerClassID
		{
			get;
			set;
		}
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region LotSerTrack
        public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Select<INLotSerClass,
            Where<INLotSerClass.lotSerClassID, Equal<Current<LotSerialStatus.lotSerClassID>>>>),
            SourceField = typeof(INLotSerClass.lotSerTrack), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        [PXDate()]
        public override DateTime? ExpireDate
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region QtyNotAvail
        public new abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion
    }



    [TransitLotSerialStatusAccumulator]
    [Serializable]
    public partial class TransitLotSerialStatus : INLotSerialStatus, IQtyAllocated
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<TransitLotSerialStatus>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr>
	    {
		    public static TransitLotSerialStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr)
			    => FindBy(graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr);
	    }
		public static new class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<TransitLotSerialStatus>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<TransitLotSerialStatus>.By<inventoryID, subItemID, siteID, locationID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<TransitLotSerialStatus>.By<subItemID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<TransitLotSerialStatus>.By<inventoryID> { }
			public class ItemLotSerial : INItemLotSerial.PK.ForeignKeyOf<TransitLotSerialStatus>.By<inventoryID, lotSerialNbr> { }
		}
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
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
        [SubItem(IsKey = true)]
        [PXForeignSelector(typeof(INTran.subItemID))]
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
        //[PXDBInt(IsKey = true)]
        [IN.Site(true, IsKey = true)]
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
		[PXDBDefault(typeof(INTransitLine.costSiteID))]
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
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String LotSerialNbr
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
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [PXDBLong]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region LotSerClassID
        public abstract class lotSerClassID : IBqlField { }
        [PXString(10, IsUnicode = true)]
        [PXDefault(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<TransitLotSerialStatus.inventoryID>>>>),
            SourceField = typeof(InventoryItem.lotSerClassID), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string LotSerClassID
		{
			get;
			set;
		}
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region LotSerTrack
        public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Select<INLotSerClass,
            Where<INLotSerClass.lotSerClassID, Equal<Current<TransitLotSerialStatus.lotSerClassID>>>>),
            SourceField = typeof(INLotSerClass.lotSerTrack), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        [PXDate()]
        public override DateTime? ExpireDate
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region QtyNotAvail
        public new abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<LotSerialStatus.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion
    }

    [ItemLotSerialAccumulator]
    [Serializable]
    public partial class ItemLotSerial : INItemLotSerial, IQtyAllocatedSeparateReceipts
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<ItemLotSerial>.By<inventoryID, lotSerialNbr>
	    {
		    public static ItemLotSerial Find(PXGraph graph, int? inventoryID, string lotSerialNbr) => FindBy(graph, inventoryID, lotSerialNbr);
	    }
		public static new class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<ItemLotSerial>.By<inventoryID> { }
		}
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
		//[SerialNumberedStatusCheck(typeof(ItemLotSerial.qtyAvail), typeof(ItemLotSerial.inventoryID), typeof(ItemLotSerial.lotSerialNbr), typeof(ItemLotSerial.lotSerTrack), typeof(ItemLotSerial.lotSerAssign), typeof(ItemLotSerial.refNoteID), typeof(ItemLotSerial.isDuplicated))]
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
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String LotSerialNbr
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
        #region RefNotID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        [PXGuid()]
        public virtual Guid? RefNoteID
        {
            get;
            set;
        }
        #endregion
        #region IsDuplicated
        public abstract class isDuplicated : PX.Data.BQL.BqlBool.Field<isDuplicated> { }
        [PXBool()]
        public virtual bool? IsDuplicated
        {
            get;
            set;
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region LotSerClassID
        public abstract class lotSerClassID : IBqlField { }
        [PXString(10, IsUnicode = true)]
        [PXDefault(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<ItemLotSerial.inventoryID>>>>),
            SourceField = typeof(InventoryItem.lotSerClassID), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string LotSerClassID
		{
			get;
			set;
		}
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region LotSerTrack
        public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Select<INLotSerClass,
            Where<INLotSerClass.lotSerClassID, Equal<Current<ItemLotSerial.lotSerClassID>>>>),
            SourceField = typeof(INLotSerClass.lotSerTrack), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region LotSerAssign
        public new abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Select<INLotSerClass,
            Where<INLotSerClass.lotSerClassID, Equal<Current<ItemLotSerial.lotSerClassID>>>>),
            SourceField = typeof(INLotSerClass.lotSerAssign), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerAssign
        {
            get
            {
                return this._LotSerAssign;
            }
            set
            {
                this._LotSerAssign = value;
            }
        }
        #endregion
        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        [PXDBDate()]
        public override DateTime? ExpireDate
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
        #region QtyNotAvail
        public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        protected Decimal? _QtyNotAvail;
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? QtyNotAvail
        {
            get
            {
                return this._QtyNotAvail;
            }
            set
            {
                this._QtyNotAvail = value;
            }
        }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(true)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion
    }

    [SiteLotSerialAccumulator]
    [Serializable]
    public partial class SiteLotSerial : INSiteLotSerial, IQtyAllocatedBase
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<SiteLotSerial>.By<inventoryID, siteID, lotSerialNbr>
	    {
		    public static SiteLotSerial Find(PXGraph graph, int? inventoryID, int? siteID, string lotSerialNbr)
			    => FindBy(graph, inventoryID, siteID, lotSerialNbr);
	    }
		public static new class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<SiteLotSerial>.By<siteID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SiteLotSerial>.By<inventoryID> { }
		}
	    #endregion
		#region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), CacheGlobal = true)]
		//[SerialNumberedStatusCheck(typeof(SiteLotSerial.qtyAvail), typeof(SiteLotSerial.inventoryID), typeof(SiteLotSerial.lotSerialNbr), typeof(SiteLotSerial.lotSerTrack), typeof(SiteLotSerial.lotSerAssign), typeof(SiteLotSerial.refNoteID), typeof(SiteLotSerial.isDuplicated))]
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
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[IN.Site(IsKey = true)]
		[PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
		[PXDefault]
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
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String LotSerialNbr
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
        #region RefNotID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        [PXGuid()]
        public virtual Guid? RefNoteID
        {
            get;
            set;
        }
        #endregion
        #region IsDuplicated
        public abstract class isDuplicated : PX.Data.BQL.BqlBool.Field<isDuplicated> { }
        [PXBool()]
        public virtual bool? IsDuplicated
        {
            get;
            set;
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXInt]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.itemClassID>))]
		[PXSelectorMarker(typeof(Search<INItemClass.itemClassID, Where<INItemClass.itemClassID, Equal<Current<itemClassID>>>>), CacheGlobal = true)]
		public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region LotSerClassID
        public abstract class lotSerClassID : IBqlField { }
        [PXString(10, IsUnicode = true)]
        [PXDefault(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<SiteLotSerial.inventoryID>>>>),
            SourceField = typeof(InventoryItem.lotSerClassID), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string LotSerClassID
		{
			get;
			set;
		}
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region LotSerTrack
        public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Select<INLotSerClass,
            Where<INLotSerClass.lotSerClassID, Equal<Current<SiteLotSerial.lotSerClassID>>>>),
            SourceField = typeof(INLotSerClass.lotSerTrack), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region LotSerAssign
        public new abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Select<INLotSerClass,
            Where<INLotSerClass.lotSerClassID, Equal<Current<SiteLotSerial.lotSerClassID>>>>),
            SourceField = typeof(INLotSerClass.lotSerAssign), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerAssign
        {
            get
            {
                return this._LotSerAssign;
            }
            set
            {
                this._LotSerAssign = value;
            }
        }
        #endregion
        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        [PXDBDate()]
        public override DateTime? ExpireDate
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
        #region QtyNotAvail
        public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        protected Decimal? _QtyNotAvail;
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? QtyNotAvail
        {
            get
            {
                return this._QtyNotAvail;
            }
            set
            {
                this._QtyNotAvail = value;
            }
        }
        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
        protected bool? _NegQty;
        [PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
        {
            get
            {
                return this._NegQty;
            }
            set
            {
                this._NegQty = value;
            }
        }
        #endregion
        #region InclQtyAvail
        public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
        protected Boolean? _InclQtyAvail;
        [PXBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyAvail
        {
            get
            {
                return this._InclQtyAvail;
            }
            set
            {
                this._InclQtyAvail = value;
            }
        }
        #endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteLotSerial.itemClassID>>>>),
			CacheGlobal = true, SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion

        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected Boolean? _InclQtyFSSrvOrdPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected Boolean? _InclQtyFSSrvOrdBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected Boolean? _InclQtyFSSrvOrdAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOPrepared
        public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
        protected Boolean? _InclQtySOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOPrepared
        {
            get
            {
                return this._InclQtySOPrepared;
            }
            set
            {
                this._InclQtySOPrepared = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
		[PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
			CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionAllocated), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        [PXBool()]
        [PXDefault(typeof(Select<INAvailabilityScheme, Where<INAvailabilityScheme.availabilitySchemeID, Equal<Current<SiteLotSerial.availabilitySchemeID>>>>),
            CacheGlobal = true, SourceField = typeof(INAvailabilityScheme.inclQtyProductionSupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion
    }

    [Serializable]
    [PXHidden]
    public partial class ReadOnlyLotSerialStatus : INLotSerialStatus
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
        [SubItem(IsKey = true)]
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
        [IN.Location(IsKey = true, ValidComboRequired = false)]
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
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String LotSerialNbr
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
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [PXDBLong()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region LotSerTrack
        public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        [PXDBString(1, IsFixed = true)]
        public override String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        protected new DateTime? _ExpireDate;
        [PXDBDate(BqlField = typeof(INItemLotSerial.expireDate))]
        [PXUIField(DisplayName = "Expiry Date")]
        public override DateTime? ExpireDate
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
    }

    [Serializable]
    [PXHidden]
    public partial class ReadOnlyCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [PXDBLongIdentityAttribute(IsKey = true)]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt()]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [PXDBInt()]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt()]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [PXDBInt()]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXDefault()]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault()]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true)]
        public override String LotSerialNbr
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

    [OversoldCostStatusAccumulator]
    [Serializable]
    public partial class OversoldCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault(INLayerType.Oversold)]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXDefault("OVERSOLD")]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true)]
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        [PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
			Search<INItemSiteSettings.negativeCost,
				Where<INItemSiteSettings.inventoryID, Equal<Current<OversoldCostStatus.inventoryID>>,
					And<Where<INItemSiteSettings.siteID, Equal<Current<OversoldCostStatus.siteID>>>>>>,
			Search<INItemCost.lastCost,
				Where<INItemCost.inventoryID, Equal<Current<OversoldCostStatus.inventoryID>>>>>))]
        public override Decimal? UnitCost
        {
            get
            {
                return this._UnitCost;
            }
            set
            {
                this._UnitCost = value;
            }
        }
        #endregion
    }

    [CostStatusAccumulator(typeof(UnmanagedCostStatus.qtyOnHand), typeof(UnmanagedCostStatus.totalCost), typeof(UnmanagedCostStatus.inventoryID), typeof(UnmanagedCostStatus.costSubItemID), typeof(UnmanagedCostStatus.costSiteID), typeof(UnmanagedCostStatus.layerType), typeof(UnmanagedCostStatus.receiptNbr))]
    [Serializable]
    public partial class UnmanagedCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt()]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount()]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault(INLayerType.Unmanaged)]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXDefault("UNMANAGED")]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true)]
       
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        [PXDBDecimal(6)]
        [PXDefault(typeof(decimal0))]
        public override Decimal? UnitCost
        {
            get
            {
                return this._UnitCost;
            }
            set
            {
                this._UnitCost = value;
            }
        }
        #endregion
    }

    [CostStatusAccumulator(typeof(AverageCostStatus.qtyOnHand), typeof(AverageCostStatus.totalCost), typeof(AverageCostStatus.inventoryID), typeof(AverageCostStatus.costSubItemID), typeof(AverageCostStatus.costSiteID), typeof(AverageCostStatus.layerType), typeof(AverageCostStatus.receiptNbr))]
    [Serializable]
    public partial class AverageCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault()]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.Average)]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault("ZZZ")]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true)]
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
    }

    [CostStatusAccumulator(typeof(StandardCostStatus.qtyOnHand), typeof(StandardCostStatus.totalCost), typeof(StandardCostStatus.inventoryID), typeof(StandardCostStatus.costSubItemID), typeof(StandardCostStatus.costSiteID), typeof(StandardCostStatus.costSiteID), typeof(StandardCostStatus.layerType), typeof(StandardCostStatus.receiptNbr))]
    [Serializable]
    public partial class StandardCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault()]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.Standard)]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault("ZZZ")]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true)]
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? UnitCost
        {
            get
            {
                return this._UnitCost;
            }
            set
            {
                this._UnitCost = value;
            }
        }
        #endregion
    }

    [CostStatusAccumulator(typeof(FIFOCostStatus.qtyOnHand), typeof(FIFOCostStatus.totalCost), typeof(FIFOCostStatus.inventoryID), typeof(FIFOCostStatus.costSubItemID), typeof(FIFOCostStatus.costSiteID), typeof(FIFOCostStatus.layerType), typeof(FIFOCostStatus.receiptNbr))]
    [Serializable]
    public partial class FIFOCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault()]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.FIFO)]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault()]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true)]
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
    }

    [CostStatusAccumulator(typeof(SpecificCostStatus.qtyOnHand), typeof(SpecificCostStatus.totalCost), typeof(SpecificCostStatus.inventoryID), typeof(SpecificCostStatus.costSubItemID), typeof(SpecificCostStatus.costSiteID), typeof(SpecificCostStatus.lotSerialNbr), typeof(SpecificCostStatus.layerType), typeof(SpecificCostStatus.receiptNbr))]
    [Serializable]
    public partial class SpecificCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID), typeof(LotSerialStatus.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.Specific)]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXDefault("ZZZ")]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Null)]
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
    }

    //receipt nbr is a key
    [CostStatusAccumulator(typeof(SpecificTransitCostStatus.qtyOnHand), typeof(SpecificTransitCostStatus.totalCost), typeof(SpecificTransitCostStatus.inventoryID), typeof(SpecificTransitCostStatus.costSubItemID), typeof(SpecificTransitCostStatus.costSiteID), typeof(SpecificTransitCostStatus.lotSerialNbr), typeof(SpecificTransitCostStatus.layerType), typeof(SpecificTransitCostStatus.receiptNbr))]
    [Serializable]
    public partial class SpecificTransitCostStatus : INCostStatus
    {
        #region CostID
        public new abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
        [CostIdentity(typeof(INTranCost.costID), typeof(LotSerialStatus.costID))]
        [PXDefault()]
        public override Int64? CostID
        {
            get
            {
                return this._CostID;
            }
            set
            {
                this._CostID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true)]
        [PXForeignSelector(typeof(INTran.inventoryID))]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [CostSiteID()]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt()]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount()]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.Specific)]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault("ZZZ")]
        public override String ReceiptNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Null)]
        public override String LotSerialNbr
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
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
    }

    [Serializable]
    [PXHidden]
    public partial class ReadOnlyReceiptStatus : INReceiptStatus
    {
        #region ReceiptID
        public new abstract class receiptID : PX.Data.BQL.BqlLong.Field<receiptID> { }
        [PXDBLong(IsKey = true)]
        [PXDefault()]
        public override Int64? ReceiptID
        {
            get
            {
                return this._ReceiptID;
            }
            set
            {
                this._ReceiptID = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [StockItem(IsKey = true)]
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        #endregion
        #region CostSiteID
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

        [Account(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [SubAccount(typeof(ReadOnlyReceiptStatus.accountID), IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String ReceiptNbr
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
        #region LotSerialNbr
        /*  public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
          protected String _LotSerialNbr;
          [PXDBString(100, IsUnicode = true)]
          [PXDefault("")]
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
          }*/
        #endregion
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        #endregion
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }

        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion

    }


    [ReceiptStatusAccumulator()]
    [Serializable]
    public partial class ReceiptStatus : INReceiptStatus
    {
        #region ReceiptID
        public new abstract class receiptID : PX.Data.BQL.BqlLong.Field<receiptID> { }
        [PXDBLongIdentity()]
        [PXDefault()]
        public override Int64? ReceiptID
        {
            get
            {
                return this._ReceiptID;
            }
            set
            {
                this._ReceiptID = value;
            }
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

        [Account(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [SubAccount(typeof(ReceiptStatus.accountID), IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region LayerType
        public new abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INLayerType.Normal)]
        public override String LayerType
        {
            get
            {
                return this._LayerType;
            }
            set
            {
                this._LayerType = value;
            }
        }
        #endregion
        #region ValMethod
        public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.FIFO)]
        public override String ValMethod
        {
            get
            {
                return this._ValMethod;
            }
            set
            {
                this._ValMethod = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        public override String ReceiptNbr
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
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true, IsKey = true)]
        [PXDefault("")]
        public override String LotSerialNbr
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
        #region ReceiptDate
        public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
        [PXDBDate()]
        [PXDefault()]
        public override DateTime? ReceiptDate
        {
            get
            {
                return this._ReceiptDate;
            }
            set
            {
                this._ReceiptDate = value;
            }
        }
        #endregion
        #region OrigQty
        public new abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
    }

    [PXAccumulator(new Type[] {
                typeof(ItemSiteHist.finYtdQty),
                typeof(ItemSiteHist.finYtdQty),

                typeof(ItemSiteHist.tranYtdQty),
                typeof(ItemSiteHist.tranYtdQty)

                },
                    new Type[] {
                typeof(ItemSiteHist.finBegQty),
                typeof(ItemSiteHist.finYtdQty),

                typeof(ItemSiteHist.tranBegQty),
                typeof(ItemSiteHist.tranYtdQty)

                }
            )]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class ItemSiteHist : INItemSiteHist
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
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public override String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
    }

	[PXAccumulator(new Type[] {
			typeof(ItemSiteHistDay.endQty),
			typeof(ItemSiteHistDay.endQty)
		}, new Type[] {
			typeof(ItemSiteHistDay.begQty),
			typeof(ItemSiteHistDay.endQty)
		})]
	[PXDisableCloneAttributes()]
	[Serializable]
	public partial class ItemSiteHistDay : INItemSiteHistDay
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
        {
		}
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
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID>
        {
		}
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
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
        {
		}
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
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID>
        {
		}
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
		#region SDate
		public new abstract class sDate : PX.Data.BQL.BqlDateTime.Field<sDate>
        {
		}
        [PXDBDate(IsKey = true)]
		public override DateTime? SDate
		{
			get
			{
				return this._SDate;
			}
			set
			{
				this._SDate = value;
			}
		}
		#endregion
	}


	public class ItemSiteHistDAccumAttribute : PXAccumulatorAttribute
    {
        public ItemSiteHistDAccumAttribute()
        {
            _SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemSiteHistD bal = (ItemSiteHistD)row;
            columns.Update<ItemSiteHistD.qtyReceived>(bal.QtyReceived, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyIssued>(bal.QtyIssued, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtySales>(bal.QtySales, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyCreditMemos>(bal.QtyCreditMemos, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyDropShipSales>(bal.QtyDropShipSales, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyTransferIn>(bal.QtyTransferIn, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyTransferOut>(bal.QtyTransferOut, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyAssemblyIn>(bal.QtyAssemblyIn, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyAssemblyOut>(bal.QtyAssemblyOut, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyAdjusted>(bal.QtyAdjusted, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.sDay>(bal.SDay, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sMonth>(bal.SMonth, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sYear>(bal.SYear, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sQuater>(bal.SQuater, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sDayOfWeek>(bal.SDayOfWeek, PXDataFieldAssign.AssignBehavior.Replace);

            return true;
        }
    }

    [ItemSiteHistDAccum(SingleRecord = true)]
    [Serializable]
    public partial class ItemSiteHistD : INItemSiteHistD
    {
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [Inventory(IsKey = true)]
        [PXDefault]
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
        [SubItem(IsKey = true)]
        [PXDefault]
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
        [Site(IsKey = true)]
        [PXDefault]
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
        #region SDate
        public new abstract class sDate : PX.Data.BQL.BqlDateTime.Field<sDate> { }
        [PXDBDate(IsKey = true)]
        public override DateTime? SDate
        {
            get
            {
                return this._SDate;
            }
            set
            {
                this._SDate = value;
            }
        }
        #endregion
    }

    [PXAccumulator(new Type[]
        {
            typeof(ItemCostHist.finYtdQty),
            typeof(ItemCostHist.finYtdQty),
            typeof(ItemCostHist.finYtdCost),
            typeof(ItemCostHist.finYtdCost),

            typeof(ItemCostHist.tranYtdQty),
            typeof(ItemCostHist.tranYtdQty),
            typeof(ItemCostHist.tranYtdCost),
            typeof(ItemCostHist.tranYtdCost)

        }, new Type[]
        {
            typeof(ItemCostHist.finBegQty),
            typeof(ItemCostHist.finYtdQty),
            typeof(ItemCostHist.finBegCost),
            typeof(ItemCostHist.finYtdCost),

            typeof(ItemCostHist.tranBegQty),
            typeof(ItemCostHist.tranYtdQty),
            typeof(ItemCostHist.tranBegCost),
            typeof(ItemCostHist.tranYtdCost)
        }
        )]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class ItemCostHist : INItemCostHist
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region AccountID
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        [SubAccount(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDBInt()]
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
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public override String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
    }

    public class ItemSalesHistAccumAttribute : PXAccumulatorAttribute
    {
        public ItemSalesHistAccumAttribute()
            : base(new Type[]
        {
            typeof(ItemSalesHist.finYtdSales),
            typeof(ItemSalesHist.finYtdCreditMemos),
            typeof(ItemSalesHist.finYtdDropShipSales),
            typeof(ItemSalesHist.finYtdCOGS),
            typeof(ItemSalesHist.finYtdCOGSCredits),
            typeof(ItemSalesHist.finYtdCOGSDropShips),
            typeof(ItemSalesHist.finYtdQtySales),
            typeof(ItemSalesHist.finYtdQtyCreditMemos),
            typeof(ItemSalesHist.finYtdQtyDropShipSales),

            typeof(ItemSalesHist.tranYtdSales),
            typeof(ItemSalesHist.tranYtdCreditMemos),
            typeof(ItemSalesHist.tranYtdDropShipSales),
            typeof(ItemSalesHist.tranYtdCOGS),
            typeof(ItemSalesHist.tranYtdCOGSCredits),
            typeof(ItemSalesHist.tranYtdCOGSDropShips),
            typeof(ItemSalesHist.tranYtdQtySales),
            typeof(ItemSalesHist.tranYtdQtyCreditMemos),
            typeof(ItemSalesHist.tranYtdQtyDropShipSales)
        }, new Type[]
        {
            typeof(ItemSalesHist.finYtdSales),
            typeof(ItemSalesHist.finYtdCreditMemos),
            typeof(ItemSalesHist.finYtdDropShipSales),
            typeof(ItemSalesHist.finYtdCOGS),
            typeof(ItemSalesHist.finYtdCOGSCredits),
            typeof(ItemSalesHist.finYtdCOGSDropShips),
            typeof(ItemSalesHist.finYtdQtySales),
            typeof(ItemSalesHist.finYtdQtyCreditMemos),
            typeof(ItemSalesHist.finYtdQtyDropShipSales),

            typeof(ItemSalesHist.tranYtdSales),
            typeof(ItemSalesHist.tranYtdCreditMemos),
            typeof(ItemSalesHist.tranYtdDropShipSales),
            typeof(ItemSalesHist.tranYtdCOGS),
            typeof(ItemSalesHist.tranYtdCOGSCredits),
            typeof(ItemSalesHist.tranYtdCOGSDropShips),
            typeof(ItemSalesHist.tranYtdQtySales),
            typeof(ItemSalesHist.tranYtdQtyCreditMemos),
            typeof(ItemSalesHist.tranYtdQtyDropShipSales)
        }
        )
        {
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemSalesHist hist = (ItemSalesHist)row;

            columns.RestrictPast<ItemSalesHist.finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
            columns.RestrictFuture<ItemSalesHist.finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");

            return true;
        }
    }

    [ItemSalesHistAccum()]
    [PXDisableCloneAttributes()]

    [Serializable]
    public partial class ItemSalesHist : INItemSalesHist
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public override String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
    }

    [ItemSalesHistDAccumAttribute(SingleRecord = true)]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class ItemSalesHistD : INItemSalesHistD
    {
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [Inventory(IsKey = true)]
        [PXDefault]
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
        [SubItem(IsKey = true)]
        [PXDefault]
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
        [Site(IsKey = true)]
        [PXDefault]
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
        #region SDate
        public new abstract class sDate : PX.Data.BQL.BqlDateTime.Field<sDate> { }
        [PXDBDate(IsKey = true)]
        public override DateTime? SDate
        {
            get
            {
                return this._SDate;
            }
            set
            {
                this._SDate = value;
            }
        }
        #endregion
        #region SYear
        public new abstract class sYear : PX.Data.BQL.BqlInt.Field<sYear> { }
        [PXDBInt(IsKey = true)]
        public override int? SYear
        {
            get
            {
                return this._SYear;
            }
            set
            {
                this._SYear = value;
            }
        }
        #endregion
        #region SMonth
        public new abstract class sMonth : PX.Data.BQL.BqlInt.Field<sMonth> { }
        [PXDBInt(IsKey = true)]
        public override int? SMonth
        {
            get
            {
                return this._SMonth;
            }
            set
            {
                this._SMonth = value;
            }
        }
        #endregion
        #region SDay
        public new abstract class sDay : PX.Data.BQL.BqlInt.Field<sDay> { }
        [PXDBInt(IsKey = true)]
        public override int? SDay
        {
            get
            {
                return this._SDay;
            }
            set
            {
                this._SDay = value;
            }
        }
        #endregion
    }
    public class ItemSalesHistDAccumAttribute : PXAccumulatorAttribute
    {
        public ItemSalesHistDAccumAttribute()
        {
            this._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemSalesHistD bal = (ItemSalesHistD)row;
            columns.Update<ItemSalesHistD.qtyExcluded>(bal.QtyExcluded, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSalesHistD.qtyIssues>(bal.QtyIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSalesHistD.qtyLostSales>(bal.QtyLostSales, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSalesHistD.qtyPlanSales>(bal.QtyPlanSales, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSalesHistD.demandType1>(bal.DemandType1, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSalesHistD.demandType2>(bal.DemandType2, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSalesHistD.sQuater>(bal.SQuater, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSalesHistD.sDayOfWeek>(bal.SDayOfWeek, PXDataFieldAssign.AssignBehavior.Replace);

            return true;
        }
    }
    public class ItemCustSalesHistAccumAttribute : PXAccumulatorAttribute
    {
        public ItemCustSalesHistAccumAttribute()
            : base(new Type[]
        {
            typeof(ItemCustSalesHist.finYtdSales),
            typeof(ItemCustSalesHist.finYtdCreditMemos),
            typeof(ItemCustSalesHist.finYtdDropShipSales),
            typeof(ItemCustSalesHist.finYtdCOGS),
            typeof(ItemCustSalesHist.finYtdCOGSCredits),
            typeof(ItemCustSalesHist.finYtdCOGSDropShips),
            typeof(ItemCustSalesHist.finYtdQtySales),
            typeof(ItemCustSalesHist.finYtdQtyCreditMemos),
            typeof(ItemCustSalesHist.finYtdQtyDropShipSales),

            typeof(ItemCustSalesHist.tranYtdSales),
            typeof(ItemCustSalesHist.tranYtdCreditMemos),
            typeof(ItemCustSalesHist.tranYtdDropShipSales),
            typeof(ItemCustSalesHist.tranYtdCOGS),
            typeof(ItemCustSalesHist.tranYtdCOGSCredits),
            typeof(ItemCustSalesHist.tranYtdCOGSDropShips),
            typeof(ItemCustSalesHist.tranYtdQtySales),
            typeof(ItemCustSalesHist.tranYtdQtyCreditMemos),
            typeof(ItemCustSalesHist.tranYtdQtyDropShipSales)
        }, new Type[]
        {
            typeof(ItemCustSalesHist.finYtdSales),
            typeof(ItemCustSalesHist.finYtdCreditMemos),
            typeof(ItemCustSalesHist.finYtdDropShipSales),
            typeof(ItemCustSalesHist.finYtdCOGS),
            typeof(ItemCustSalesHist.finYtdCOGSCredits),
            typeof(ItemCustSalesHist.finYtdCOGSDropShips),
            typeof(ItemCustSalesHist.finYtdQtySales),
            typeof(ItemCustSalesHist.finYtdQtyCreditMemos),
            typeof(ItemCustSalesHist.finYtdQtyDropShipSales),

            typeof(ItemCustSalesHist.tranYtdSales),
            typeof(ItemCustSalesHist.tranYtdCreditMemos),
            typeof(ItemCustSalesHist.tranYtdDropShipSales),
            typeof(ItemCustSalesHist.tranYtdCOGS),
            typeof(ItemCustSalesHist.tranYtdCOGSCredits),
            typeof(ItemCustSalesHist.tranYtdCOGSDropShips),
            typeof(ItemCustSalesHist.tranYtdQtySales),
            typeof(ItemCustSalesHist.tranYtdQtyCreditMemos),
            typeof(ItemCustSalesHist.tranYtdQtyDropShipSales)
        }
        )
        {
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemCustSalesHist hist = (ItemCustSalesHist)row;

            columns.RestrictPast<ItemCustSalesHist.finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
            columns.RestrictFuture<ItemCustSalesHist.finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");

            return true;
        }
    }

    [ItemCustSalesStatsAccum()]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class ItemCustSalesStats : INItemCustSalesStats
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
        [SubItem(IsKey = true)]
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
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected new Int32? _BAccountID;
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion
    }

	[ItemCustDropShipStatsAccum]
	[PXDisableCloneAttributes()]
	[Serializable]
	public partial class ItemCustDropShipStats : INItemCustSalesStats
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
		[SubItem(IsKey = true)]
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
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected new Int32? _BAccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
	}

	public abstract class INItemCustSalesStatsAccumAttribute: PXAccumulatorAttribute
	{
		public INItemCustSalesStatsAccumAttribute()
		{
			this._SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
				return false;

			PrepareInsertImpl((INItemCustSalesStats)row, columns);

			return true;
		}

		protected abstract void PrepareInsertImpl(INItemCustSalesStats stats, PXAccumulatorCollection columns);

		public override bool PersistInserted(PXCache sender, object row)
		{
			try
			{
				return base.PersistInserted(sender, row);
			}
			catch (PXLockViolationException)
			{
				return false;
			}
		}
	}

	public class ItemCustSalesStatsAccumAttribute : INItemCustSalesStatsAccumAttribute
	{        
		protected override void PrepareInsertImpl(INItemCustSalesStats stats, PXAccumulatorCollection columns)
		{
			columns.Update<INItemCustSalesStats.lastDate>(stats.LastDate, PXDataFieldAssign.AssignBehavior.Maximize);
			columns.Update<INItemCustSalesStats.lastUnitPrice>(stats.LastUnitPrice, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<INItemCustSalesStats.lastQty>(stats.LastQty, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Restrict<INItemCustSalesStats.lastDate>(PXComp.LEorISNULL, stats.LastDate);
		}
    }

	public class ItemCustDropShipStatsAccumAttribute : INItemCustSalesStatsAccumAttribute
	{
		protected override void PrepareInsertImpl(INItemCustSalesStats stats, PXAccumulatorCollection columns)
		{
			columns.Update<INItemCustSalesStats.dropShipLastDate>(stats.DropShipLastDate, PXDataFieldAssign.AssignBehavior.Maximize);
			columns.Update<INItemCustSalesStats.dropShipLastUnitPrice>(stats.DropShipLastUnitPrice, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<INItemCustSalesStats.dropShipLastQty>(stats.DropShipLastQty, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Restrict<INItemCustSalesStats.dropShipLastDate>(PXComp.LEorISNULL, stats.DropShipLastDate);
		}
	}

	[ItemCustSalesHistAccum()]
    [PXDisableCloneAttributes()]
    [Serializable]
    public partial class ItemCustSalesHist : INItemCustSalesHist
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
        #region CostSubItemID
        public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
        [SubItem(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSubItemID
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
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? CostSiteID
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
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected new Int32? _BAccountID;
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public override String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
    }

    public class StatusAccumulatorAttribute : PXAccumulatorAttribute
    {
        protected Dictionary<object, bool> _persisted;
        protected PXAccumulatorCollection _columns;
		protected bool _InternalCall = false;

        protected virtual object Aggregate(PXCache cache, object a, object b)
        {
            object ret = cache.CreateCopy(a);

            foreach (KeyValuePair<string, PXAccumulatorItem> column in _columns)
            {
                if (column.Value.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Summarize)
                {
                    object aVal = cache.GetValue(a, column.Key);
                    object bVal = cache.GetValue(b, column.Key);
                    object retVal = null;

                    if (aVal.GetType() == typeof(decimal))
                    {
                        retVal = (decimal)aVal + (decimal)bVal;
                    }

                    if (aVal.GetType() == typeof(double))
                    {
                        retVal = (double)aVal + (double)bVal;
                    }

                    if (aVal.GetType() == typeof(long))
                    {
                        retVal = (long)aVal + (long)bVal;
                    }

                    if (aVal.GetType() == typeof(int))
                    {
                        retVal = (int)aVal + (int)bVal;
                    }

                    if (aVal.GetType() == typeof(short))
                    {
                        retVal = (short)aVal + (short)bVal;
                    }

                    cache.SetValue(ret, column.Key, retVal);
                }
				else if (column.Value.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Replace)
				{
					object retVal = cache.GetValue(b, column.Key);
					cache.SetValue(ret, column.Key, retVal);
				}
				else if (column.Value.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Initialize)
				{
					object aVal = cache.GetValue(a, column.Key);
					object bVal = cache.GetValue(b, column.Key);
					object retVal = aVal ?? bVal;
					cache.SetValue(ret, column.Key, retVal);
				}
            }

            return ret;
        }

		public virtual void ResetPersisted(object row)
		{
			if (_persisted != null && _persisted.ContainsKey(row))
			{
				_persisted.Remove(row);
			}
		}

        public override object Insert(PXCache sender, object row)
        {
            object copy = sender.CreateCopy(row);

            PXAccumulatorCollection columns = new PXAccumulatorCollection();

			this._InternalCall = true;
            PrepareInsert(sender, row, columns);

            foreach (KeyValuePair<string, PXAccumulatorItem> column in columns)
            {
                if (column.Value.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Summarize)
                {
                    sender.SetValue(copy, column.Key, null);
                }
            }

            object item = base.Insert(sender, copy);

            if (item != null && _persisted.ContainsKey(item))
            {
                foreach (string field in sender.Fields)
                {
                    if (sender.GetValue(copy, field) == null)
                    {
                        object newvalue;
                        if (sender.RaiseFieldDefaulting(field, copy, out newvalue))
                        {
                            sender.RaiseFieldUpdating(field, copy, ref newvalue);
                        }
                        sender.SetValue(copy, field, newvalue);
                    }
                }
                return copy;
            }
            return item;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            _persisted = new Dictionary<object, bool>();
            sender.Graph.RowPersisted.AddHandler(sender.GetItemType(), RowPersisted);
            sender.Graph.OnClear += Graph_OnClear;
        }

        private void Graph_OnClear(PXGraph graph, PXClearOption option)
        {
            _persisted = new Dictionary<object, bool>();
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            _columns = columns;
            if (base.PrepareInsert(sender, row, columns))
            {
                foreach (string field in sender.Fields)
                {
                    if (sender.Keys.IndexOf(field) < 0 && field.StartsWith("Usr", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object val = sender.GetValue(row, field);
                        columns.Update(field, val, (val != null) ? PXDataFieldAssign.AssignBehavior.Replace : PXDataFieldAssign.AssignBehavior.Initialize);
                    }
                }
                return true;
            }
            return false;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
			this._InternalCall = false;
            if (base.PersistInserted(sender, row))
            {
                _persisted.Add(row, true);
                return true;
            }
            return false;
        }

        public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && (e.TranStatus == PXTranStatus.Completed || e.TranStatus == PXTranStatus.Aborted))
            {
                if (_persisted.ContainsKey(e.Row))
                {
                    _persisted.Remove(e.Row);
                }
            }
        }

		public virtual bool IsZero(IStatus a)
		{
			return a.IsZero();
		}
	}

    public class ItemStatsAccumulatorAttribute : PXAccumulatorAttribute
    {
        public ItemStatsAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemStats bal = (ItemStats)row;

            if (bal.LastCostDate == INItemStats.MinDate.get())
            {
				bal.LastCost = null;
				columns.Update<ItemStats.lastCost>(0m, PXDataFieldAssign.AssignBehavior.Initialize);
				// It is needed for correct work of DBLastChangedDateTime.
                columns.Update<ItemStats.lastCostDate>(bal.LastCostDate ?? new DateTime(1900, 1, 1), PXDataFieldAssign.AssignBehavior.Initialize);
            }
            else
            {
                columns.Update<ItemStats.lastCost>(bal.LastCost, PXDataFieldAssign.AssignBehavior.Replace);
                columns.Update<ItemStats.lastCostDate>(bal.LastCostDate, PXDataFieldAssign.AssignBehavior.Replace);
            }

            if (bal.MinCost == 0m)
            {
                columns.Update<ItemStats.minCost>(bal.MinCost, PXDataFieldAssign.AssignBehavior.Initialize);
            }
            else
            {
                columns.Update<ItemStats.minCost>(bal.MinCost, PXDataFieldAssign.AssignBehavior.Minimize);
            }

            columns.Update<ItemStats.maxCost>(bal.MaxCost, PXDataFieldAssign.AssignBehavior.Maximize);
            columns.Update<ItemStats.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemStats.totalCost>(bal.TotalCost, PXDataFieldAssign.AssignBehavior.Summarize);

            return true;
        }
    }

    public class SiteStatusAccumulatorAttribute : StatusAccumulatorAttribute
    {
        public SiteStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            SiteStatus bal = (SiteStatus)row;

            columns.Update<SiteStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<SiteStatus.qtyFSSrvOrdPrepared>(bal.QtyFSSrvOrdPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyFSSrvOrdBooked>(bal.QtyFSSrvOrdBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyFSSrvOrdAllocated>(bal.QtyFSSrvOrdAllocated, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<SiteStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtySOPrepared>(bal.QtySOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyInTransitToProduction>(bal.QtyInTransitToProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProductionSupplyPrepared>(bal.QtyProductionSupplyPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProductionSupply>(bal.QtyProductionSupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyPOFixedProductionPrepared>(bal.QtyPOFixedProductionPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyPOFixedProductionOrders>(bal.QtyPOFixedProductionOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProductionDemandPrepared>(bal.QtyProductionDemandPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProductionDemand>(bal.QtyProductionDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProductionAllocated>(bal.QtyProductionAllocated, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtySOFixedProduction>(bal.QtySOFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProdFixedPurchase>(bal.QtyProdFixedPurchase, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProdFixedProduction>(bal.QtyProdFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProdFixedProdOrdersPrepared>(bal.QtyProdFixedProdOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProdFixedProdOrders>(bal.QtyProdFixedProdOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProdFixedSalesOrdersPrepared>(bal.QtyProdFixedSalesOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteStatus.qtyProdFixedSalesOrders>(bal.QtyProdFixedSalesOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            //only in release process updates onhand.
            if (bal.NegQty == false && bal.SkipQtyValidation != true && bal.QtyOnHand < 0m)
            {
                columns.Restrict<SiteStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }
            else if (bal.NegAvailQty == false && bal.SkipQtyValidation != true && bal.QtyHardAvail < 0m)
            {
                columns.Restrict<SiteStatus.qtyHardAvail>(PXComp.GE, -bal.QtyHardAvail);
            }
            if (bal.NegQty == false && bal.SkipQtyValidation != true && bal.QtyActual < 0m)
            {
				columns.Restrict<SiteStatus.qtyActual>(PXComp.GE, -bal.QtyActual);
			}

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero(bal) && bal.PersistEvenZero != true)
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<SiteStatus.inventoryID>(row);
                object subItemID = sender.GetValue<SiteStatus.subItemID>(row);
                object siteID = sender.GetValue<SiteStatus.siteID>(row);

	            SiteStatus item = SiteStatus.PK.Find(sender.Graph, (int?)inventoryID, (int?)subItemID, (int?)siteID);

                item = (SiteStatus)this.Aggregate(sender, item, row);

                SiteStatus bal = (SiteStatus)row;
                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyOnHandNegative;
                    }
                }
                else if (bal.NegAvailQty == false && bal.QtyHardAvail < 0m)
                {
                    if (item.QtyHardAvail < 0)
                    {
                        message = Messages.StatusCheck_QtyAvailNegative;
                    }
                }
				if (bal.NegQty == false && bal.QtyActual < 0m)
				{
					if (item.QtyActual < 0)
					{
						message = Messages.StatusCheck_QtyActualNegative;
					}
				}

                if (message != null)
                {
                    throw new PXException(message,
                    PXForeignSelectorAttribute.GetValueExt<SiteStatus.inventoryID>(sender, row),
                    PXForeignSelectorAttribute.GetValueExt<SiteStatus.subItemID>(sender, row),
                    PXForeignSelectorAttribute.GetValueExt<SiteStatus.siteID>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                SiteStatus bal = (SiteStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyOnHandNegative;
                }

                if (bal.NegAvailQty == false && bal.QtyHardAvail < 0m)
                {
                    message = Messages.StatusCheck_QtyAvailNegative;
                }
                else if (bal.NegQty == false && bal.QtyINIssues < 0m && bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyAvailNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.siteID>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }
    }

    public class TransitSiteStatusAccumulatorAttribute : StatusAccumulatorAttribute
    {
        public TransitSiteStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            TransitSiteStatus bal = (TransitSiteStatus)row;

            columns.Update<TransitSiteStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<TransitSiteStatus.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<TransitSiteStatus.qtyFSSrvOrdPrepared>(bal.QtyFSSrvOrdPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyFSSrvOrdBooked>(bal.QtyFSSrvOrdBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyFSSrvOrdAllocated>(bal.QtyFSSrvOrdAllocated, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<TransitSiteStatus.qtySOPrepared>(bal.QtySOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyInTransitToProduction>(bal.QtyInTransitToProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProductionSupplyPrepared>(bal.QtyProductionSupplyPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProductionSupply>(bal.QtyProductionSupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyPOFixedProductionPrepared>(bal.QtyPOFixedProductionPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyPOFixedProductionOrders>(bal.QtyPOFixedProductionOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProductionDemandPrepared>(bal.QtyProductionDemandPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProductionDemand>(bal.QtyProductionDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProductionAllocated>(bal.QtyProductionAllocated, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtySOFixedProduction>(bal.QtySOFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProdFixedPurchase>(bal.QtyProdFixedPurchase, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProdFixedProduction>(bal.QtyProdFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProdFixedProdOrdersPrepared>(bal.QtyProdFixedProdOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProdFixedProdOrders>(bal.QtyProdFixedProdOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProdFixedSalesOrdersPrepared>(bal.QtyProdFixedSalesOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitSiteStatus.qtyProdFixedSalesOrders>(bal.QtyProdFixedSalesOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            //only in release process updates onhand.
            if (bal.QtyOnHand < 0m)
            {
                columns.Restrict<TransitSiteStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero((TransitSiteStatus)row))
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<TransitSiteStatus.inventoryID>(row);
                object subItemID = sender.GetValue<TransitSiteStatus.subItemID>(row);
                object siteID = sender.GetValue<TransitSiteStatus.siteID>(row);

                TransitSiteStatus item = TransitSiteStatus.PK.Find(sender.Graph, (int?)inventoryID, (int?)subItemID, (int?)siteID);

                item = (TransitSiteStatus)this.Aggregate(sender, item, row);

                TransitSiteStatus bal = (TransitSiteStatus)row;
                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyTransitOnHandNegative;
                    }
                }

                if (message != null)
                {
                    throw new PXException(message,
                    PXForeignSelectorAttribute.GetValueExt<SiteStatus.inventoryID>(sender, row),
                    PXForeignSelectorAttribute.GetValueExt<SiteStatus.subItemID>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                TransitSiteStatus bal = (TransitSiteStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyTransitOnHandNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.subItemID>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }
    }


    public class TransitLocationStatusAccumulatorAttribute : StatusAccumulatorAttribute
    {
        public TransitLocationStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            TransitLocationStatus bal = (TransitLocationStatus)row;

            columns.Update<TransitLocationStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<TransitLocationStatus.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<TransitLocationStatus.qtyFSSrvOrdPrepared>(bal.QtyFSSrvOrdPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyFSSrvOrdBooked>(bal.QtyFSSrvOrdBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyFSSrvOrdAllocated>(bal.QtyFSSrvOrdAllocated, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<TransitLocationStatus.qtySOPrepared>(bal.QtySOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyInTransitToProduction>(bal.QtyInTransitToProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProductionSupplyPrepared>(bal.QtyProductionSupplyPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProductionSupply>(bal.QtyProductionSupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyPOFixedProductionPrepared>(bal.QtyPOFixedProductionPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyPOFixedProductionOrders>(bal.QtyPOFixedProductionOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProductionDemandPrepared>(bal.QtyProductionDemandPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProductionDemand>(bal.QtyProductionDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProductionAllocated>(bal.QtyProductionAllocated, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtySOFixedProduction>(bal.QtySOFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProdFixedPurchase>(bal.QtyProdFixedPurchase, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProdFixedProduction>(bal.QtyProdFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProdFixedProdOrdersPrepared>(bal.QtyProdFixedProdOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProdFixedProdOrders>(bal.QtyProdFixedProdOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProdFixedSalesOrdersPrepared>(bal.QtyProdFixedSalesOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLocationStatus.qtyProdFixedSalesOrders>(bal.QtyProdFixedSalesOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            //only in release process updates onhand.
            if (bal.QtyOnHand < 0m)
            {
                columns.Restrict<TransitLocationStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero((TransitLocationStatus)row))
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<TransitLocationStatus.inventoryID>(row);
                object subItemID = sender.GetValue<TransitLocationStatus.subItemID>(row);
                object siteID = sender.GetValue<TransitLocationStatus.siteID>(row);
                object locationID = sender.GetValue<TransitLocationStatus.locationID>(row);

	            TransitLocationStatus item = TransitLocationStatus.PK.Find(sender.Graph, (int?)inventoryID, (int?)subItemID, (int?)siteID, (int?)locationID);

                item = (TransitLocationStatus)this.Aggregate(sender, item, row);

                TransitLocationStatus bal = (TransitLocationStatus)row;

                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyTransitOnHandNegative;
                    }
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<TransitLocationStatus.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<TransitLocationStatus.subItemID>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                TransitLocationStatus bal = (TransitLocationStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyTransitOnHandNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<TransitLocationStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<TransitLocationStatus.subItemID>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }
    }

    public class LocationStatusAccumulatorAttribute : StatusAccumulatorAttribute
    {
        public LocationStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            LocationStatus bal = (LocationStatus)row;

            columns.Update<LocationStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<LocationStatus.qtyFSSrvOrdPrepared>(bal.QtyFSSrvOrdPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyFSSrvOrdBooked>(bal.QtyFSSrvOrdBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyFSSrvOrdAllocated>(bal.QtyFSSrvOrdAllocated, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<LocationStatus.qtySOPrepared>(bal.QtySOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyInTransitToProduction>(bal.QtyInTransitToProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProductionSupplyPrepared>(bal.QtyProductionSupplyPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProductionSupply>(bal.QtyProductionSupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyPOFixedProductionPrepared>(bal.QtyPOFixedProductionPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyPOFixedProductionOrders>(bal.QtyPOFixedProductionOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProductionDemandPrepared>(bal.QtyProductionDemandPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProductionDemand>(bal.QtyProductionDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProductionAllocated>(bal.QtyProductionAllocated, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtySOFixedProduction>(bal.QtySOFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProdFixedPurchase>(bal.QtyProdFixedPurchase, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProdFixedProduction>(bal.QtyProdFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProdFixedProdOrdersPrepared>(bal.QtyProdFixedProdOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProdFixedProdOrders>(bal.QtyProdFixedProdOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProdFixedSalesOrdersPrepared>(bal.QtyProdFixedSalesOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LocationStatus.qtyProdFixedSalesOrders>(bal.QtyProdFixedSalesOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            if (bal.QtyOnHand >= 0m)
            {
                bal.NegQty = true;
            }

            //only in release process updates onhand.
            if (bal.NegQty == false && bal.SkipQtyValidation != true && bal.QtyOnHand < 0m)
            {
                columns.Restrict<LocationStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }

			if (!_InternalCall && 
				(bal.QtyOnHand < 0m || bal.QtySOShipped < 0m || 
				bal.QtyOnHand > 0m || bal.QtySOShipped > 0m))
			{
				if (this.CreateLocksInspector(bal.SiteID.Value)
					.IsInventoryLocationLocked(bal.InventoryID, bal.LocationID, bal.RelatedPIID))
				{
					throw new PXException(Messages.PICountInProgressDuringRelease,
										  PXForeignSelectorAttribute.GetValueExt<LocationStatus.inventoryID>(sender, bal),
										  PXForeignSelectorAttribute.GetValueExt<LocationStatus.siteID>(sender, bal),
										  PXForeignSelectorAttribute.GetValueExt<LocationStatus.locationID>(sender, bal));
				}
			}

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero((LocationStatus)row))
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<LocationStatus.inventoryID>(row);
                object subItemID = sender.GetValue<LocationStatus.subItemID>(row);
                object siteID = sender.GetValue<LocationStatus.siteID>(row);
                object locationID = sender.GetValue<LocationStatus.locationID>(row);

	            LocationStatus item = LocationStatus.PK.Find(sender.Graph, (int?)inventoryID, (int?)subItemID, (int?)siteID, (int?)locationID);

                item = (LocationStatus)this.Aggregate(sender, item, row);

                LocationStatus bal = (LocationStatus)row;

                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyLocationOnHandNegative;
                    }
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.subItemID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.siteID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.locationID>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                LocationStatus bal = (LocationStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyLocationOnHandNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.siteID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.locationID>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }

		protected virtual PILocksInspector CreateLocksInspector(int siteID)
		{
			return new PILocksInspector(siteID);
		}
    }

    public class ItemLotSerialAccumulatorAttribute : StatusAccumulatorAttribute
    {
        protected class AutoNumberedEntityHelper : EntityHelper
        {
            public AutoNumberedEntityHelper(PXGraph graph)
                : base(graph)
            {
            }

            public override string GetFieldString(object row, Type entityType, string fieldName, bool preferDescription)
            {
                PXCache cache = this.graph.Caches[entityType];

                if (cache.GetStatus(row) == PXEntryStatus.Inserted)
                {
                    object cached = cache.Locate(row);
                    string val = AutoNumberAttribute.GetKeyToAbort(cache, cached, fieldName);
                    if (val != null)
                    {
                        return val;
                    }
                }
                return base.GetFieldString(row, entityType, fieldName, preferDescription);
            }
        }

		protected bool forceValidateAvailQty;

		public virtual bool ValidateAvailQty(PXGraph graph)
		{
			return forceValidateAvailQty || !graph.UnattendedMode;
		}

		public static void ForceAvailQtyValidation(PXGraph graph, bool val)
		{
			graph.Caches.SubscribeCacheCreated<ItemLotSerial>(() =>
			{
				PXCache cache = graph.Caches[typeof(ItemLotSerial)];
				var attr = cache.Interceptor as ItemLotSerialAccumulatorAttribute;
				if (attr != null)
				{
					attr.forceValidateAvailQty = val;
				}
			});
		}

        public ItemLotSerialAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected virtual void PrepareSingleField<Field>(PXCache sender, object row, PXAccumulatorCollection columns)
           where Field : IBqlField
        {
            string lotSerTrack = (string)sender.GetValue<ItemLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<ItemLotSerial.lotSerAssign>(row);
            decimal? qty = (decimal?)sender.GetValue<Field>(row);

            if (lotSerTrack == INLotSerTrack.SerialNumbered &&
                qty != null &&
                qty != 0m &&
                qty != -1m &&
                qty != 1m)
                throw new PXException(Messages.SerialNumberDuplicated,
                    PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.inventoryID>(sender, row),
                    PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.lotSerialNbr>(sender, row));

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenReceived)
            {
                columns.Restrict<Field>(PXComp.LE, 1m - qty);
                columns.Restrict<Field>(PXComp.GE, 0m - qty);
            }

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenUsed)
            {
				columns.AppendException(string.Empty,
					new PXAccumulatorRestriction<Field>(PXComp.LE, 1m));
				columns.AppendException(string.Empty,
					new PXAccumulatorRestriction<Field>(PXComp.GE, -1m));

				columns.AppendException(string.Empty,
					new PXAccumulatorRestriction<ItemLotSerial.qtyOrig>(PXComp.ISNULL, null),
					new PXAccumulatorRestriction<ItemLotSerial.qtyOrig>(PXComp.NE, -1m),
					new PXAccumulatorRestriction<Field>(PXComp.LT, 1m));
				columns.AppendException(string.Empty,
					new PXAccumulatorRestriction<ItemLotSerial.qtyOrig>(PXComp.ISNULL, null),
					new PXAccumulatorRestriction<ItemLotSerial.qtyOrig>(PXComp.NE, 1m),
					new PXAccumulatorRestriction<Field>(PXComp.GT, -1m));
            }
        }

        protected virtual void ValidateSingleField<Field>(PXCache sender, object row, Guid? refNoteID, ref string message)
            where Field : IBqlField
        {
            string lotSerTrack = (string)sender.GetValue<ItemLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<ItemLotSerial.lotSerAssign>(row);
            decimal? qty = (decimal?)sender.GetValue<Field>(row);

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenReceived)
            {
                //consider reusing PXAccumulator rules
                if (qty < 0m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyIssued : Messages.SerialNumberAlreadyIssuedIn;
                }
                if (qty > 1m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyReceived : Messages.SerialNumberAlreadyReceivedIn;
                }
            }
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemLotSerial bal = (ItemLotSerial)row;

            columns.Update<ItemLotSerial.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemLotSerial.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemLotSerial.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemLotSerial.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemLotSerial.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.InitializeWith<ItemLotSerial.qtyOrig>(bal.QtyOrig);
            columns.Update<ItemLotSerial.lotSerTrack>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<ItemLotSerial.lotSerAssign>(bal.LotSerAssign, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<ItemLotSerial.expireDate>(bal.ExpireDate, bal.UpdateExpireDate == true ?
				PXDataFieldAssign.AssignBehavior.Replace : PXDataFieldAssign.AssignBehavior.Initialize);

			bool whenUsedSerialNumber = (bal.LotSerTrack == INLotSerTrack.SerialNumbered && bal.LotSerAssign == INLotSerAssign.WhenUsed);
			bool whenRcvdSerialNumber = (bal.LotSerTrack == INLotSerTrack.SerialNumbered && bal.LotSerAssign == INLotSerAssign.WhenReceived);
            //only in release process updates onhand.
            if (bal.QtyOnHand != 0m)
            {
				if (whenUsedSerialNumber && bal.QtyOnHand != null)
				{
					columns.Update<ItemLotSerial.qtyOrig>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Initialize);
				}
                PrepareSingleField<ItemLotSerial.qtyOnHand>(sender, row, columns);
            }
			else if (this.ValidateAvailQty(sender.Graph))
            {
                if (bal.QtyHardAvail < 0m)
                    PrepareSingleField<ItemLotSerial.qtyHardAvail>(sender, row, columns);

                if (bal.QtyAvail != 0m)
                    PrepareSingleField<ItemLotSerial.qtyAvail>(sender, row, columns);

				if (bal.QtyOnReceipt != 0m && whenRcvdSerialNumber)
				{
					PrepareSingleField<ItemLotSerial.qtyOnReceipt>(sender, row, columns);
					if (bal.QtyOnReceipt > 0m)
					{
						columns.Restrict<ItemLotSerial.qtyOnHand>(PXComp.LE, 1m - bal.QtyOnReceipt);
					}
				}
            }

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero((ItemLotSerial)row))
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}
			
            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
				int? inventoryID = (int?)sender.GetValue<ItemLotSerial.inventoryID>(row);
                string lotSerialNbr = (string)sender.GetValue<ItemLotSerial.lotSerialNbr>(row);
	            
				ItemLotSerial item = ItemLotSerial.PK.Find(sender.Graph, inventoryID, lotSerialNbr);

                item = (ItemLotSerial)this.Aggregate(sender, item, row);
                item.LotSerTrack = ((ItemLotSerial)row).LotSerTrack;
                item.LotSerAssign = ((ItemLotSerial)row).LotSerAssign;

				bool isDuplicated;
				string refRowID;
				Guid? refNoteID = LookupDocumentsByLotSerialNumber(sender, inventoryID, lotSerialNbr, out isDuplicated, out refRowID);

				string message = null;
				if (((ItemLotSerial)row).LotSerTrack == INLotSerTrack.SerialNumbered && isDuplicated)
				{
					message = Messages.SerialNumberDuplicated;
				}

				ItemLotSerial bal = (ItemLotSerial)row;

				//only in release process updates onhand.              
				if (bal.QtyOnHand != 0m)
                {
					ValidateSingleField<ItemLotSerial.qtyOnHand>(sender, item, null, ref message);
				}
				else if (this.ValidateAvailQty(sender.Graph))
				{
					ValidateSingleField<ItemLotSerial.qtyAvail>(sender, item, refNoteID, ref message);
					ValidateSingleField<ItemLotSerial.qtyHardAvail>(sender, item, refNoteID, ref message);
					bool whenRcvdSerialNumber = (bal.LotSerTrack == INLotSerTrack.SerialNumbered && bal.LotSerAssign == INLotSerAssign.WhenReceived);
					if (whenRcvdSerialNumber)
					{
						ValidateSingleField<ItemLotSerial.qtyOnReceipt>(sender, item, refNoteID, ref message);
						if (message == null && item.QtyOnHand + bal.QtyOnReceipt > 1m)
						{
							message = Messages.SerialNumberAlreadyReceived;
						}
					}
				}

				if (message != null)
				{
					throw new PXException(message,
						PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.inventoryID>(sender, row),
						PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.lotSerialNbr>(sender, row),
						refRowID);
				}

				throw;
			}
			catch (PXRestrictionViolationException exc)
			{
				int? inventoryID = (int?)sender.GetValue<ItemLotSerial.inventoryID>(row);
				string lotSerialNbr = (string)sender.GetValue<ItemLotSerial.lotSerialNbr>(row);
				bool isDuplicated;
				string refRowID;
				Guid? refNoteID = LookupDocumentsByLotSerialNumber(sender, inventoryID, lotSerialNbr, out isDuplicated, out refRowID);

				// even numbers are about already received, odd - issued
				bool alreadyReceived = (exc.Index % 2 == 0);
				string message = isDuplicated ? Messages.SerialNumberDuplicated
					: alreadyReceived
					? (refNoteID != null) ? Messages.SerialNumberAlreadyReceivedIn : Messages.SerialNumberAlreadyReceived
					: (refNoteID != null) ? Messages.SerialNumberAlreadyIssuedIn : Messages.SerialNumberAlreadyIssued;

				throw new PXException(message,
					PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.inventoryID>(sender, row),
					PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.lotSerialNbr>(sender, row),
					refRowID);
			}
		}

		protected virtual Guid? LookupDocumentsByLotSerialNumber(PXCache sender, int? inventoryID, string lotSerialNbr, out bool isDuplicated, out string refRowID)
		{
			isDuplicated = false;
			refRowID = null;
			PXResultset<INItemPlan> plans = PXSelect<INItemPlan,
				Where<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>, And<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>>
				.SelectWindowed(sender.Graph, 0, 10, inventoryID, lotSerialNbr);

                    if (plans.Count <= 1)
                    {
				return null;
                    }
                    else
                    {
				var refs = new List<Guid?>();
				var counts = new Dictionary<Guid?, int>();
                        PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                        for (int i = 0; i < plans.Count; i++)
                        {
					Guid? refNoteID = plans[i].GetItem<INItemPlan>().RefNoteID;
                            if (cache.GetStatus(plans[i].GetItem<INItemPlan>()) == PXEntryStatus.Notchanged)
                            {
                                refs.Insert(0, refNoteID);
                            }
                            else
                            {
                                refs.Add(refNoteID);
                            }

                            if (counts.ContainsKey(refNoteID))
                            {
                                counts[refNoteID]++;
                                isDuplicated = true;
                            }
                            else
                            {
                                counts[refNoteID] = 1;
                            }
                        }
				var hlp = new AutoNumberedEntityHelper(sender.Graph);
				refRowID = hlp.GetEntityRowID(refs[0]);

				return refs[0];
            }
        }

		public virtual bool IsZero(ItemLotSerial a)
		{
			return a.IsZero();
		}
    }

    public class SiteLotSerialAccumulatorAttribute : StatusAccumulatorAttribute
    {
        protected class AutoNumberedEntityHelper : EntityHelper
        {
            public AutoNumberedEntityHelper(PXGraph graph)
                : base(graph)
            {
            }

            public override string GetFieldString(object row, Type entityType, string fieldName, bool preferDescription)
            {
                PXCache cache = this.graph.Caches[entityType];

                if (cache.GetStatus(row) == PXEntryStatus.Inserted)
                {
                    object cached = cache.Locate(row);
                    string val = AutoNumberAttribute.GetKeyToAbort(cache, cached, fieldName);
                    if (val != null)
                    {
                        return val;
                    }
                }
                return base.GetFieldString(row, entityType, fieldName, preferDescription);
            }
        }

		protected bool forceValidateAvailQty;

		public virtual bool ValidateAvailQty(PXGraph graph)
		{
			return forceValidateAvailQty || !graph.UnattendedMode;
		}

		public static void ForceAvailQtyValidation(PXGraph graph, bool val)
		{
			graph.Caches.SubscribeCacheCreated<SiteLotSerial>(() =>
			{
				PXCache cache = graph.Caches[typeof(SiteLotSerial)];
				var attr = cache.Interceptor as SiteLotSerialAccumulatorAttribute;
				if (attr != null)
				{
					attr.forceValidateAvailQty = val;
				}
			});
		}

        public SiteLotSerialAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected virtual void PrepareSingleField<Field>(PXCache sender, object row, PXAccumulatorCollection columns)
           where Field : IBqlField
        {
            string lotSerTrack = (string)sender.GetValue<SiteLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<SiteLotSerial.lotSerAssign>(row);
            decimal? qty = (decimal?)sender.GetValue<Field>(row);

            if (lotSerTrack == INLotSerTrack.SerialNumbered &&
                qty != null &&
                qty != 0m &&
                qty != -1m &&
                qty != 1m)
                throw new PXException(Messages.SerialNumberDuplicated,
                    PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.inventoryID>(sender, row),
                    PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.lotSerialNbr>(sender, row));

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenReceived)
            {
                columns.Restrict<Field>(PXComp.LE, 1m - qty);
                columns.Restrict<Field>(PXComp.GE, 0m - qty);
            }
            }

        protected virtual void ValidateSingleField<Field>(PXCache sender, object row, Guid? refNoteID, ref string message)
            where Field : IBqlField
        {
            string lotSerTrack = (string)sender.GetValue<SiteLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<SiteLotSerial.lotSerAssign>(row);
            decimal? qty = (decimal?)sender.GetValue<Field>(row);

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenReceived)
            {
                //consider reusing PXAccumulator rules
                if (qty < 0m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyIssued : Messages.SerialNumberAlreadyIssuedIn;
                }
                if (qty > 1m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyReceived : Messages.SerialNumberAlreadyReceivedIn;
                }
            }
                }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            SiteLotSerial bal = (SiteLotSerial)row;

            string lotSerTrack = (string)sender.GetValue<SiteLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<SiteLotSerial.lotSerAssign>(row);
            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenUsed)
                return false;

            columns.Update<SiteLotSerial.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteLotSerial.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteLotSerial.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteLotSerial.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteLotSerial.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<SiteLotSerial.lotSerTrack>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<SiteLotSerial.lotSerAssign>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
            if (bal.UpdateExpireDate == true)
                columns.Update<SiteLotSerial.expireDate>(bal.ExpireDate, PXDataFieldAssign.AssignBehavior.Replace);
            else
                columns.Update<SiteLotSerial.expireDate>(bal.ExpireDate, PXDataFieldAssign.AssignBehavior.Initialize);

            //only in release process updates onhand.
            if (bal.QtyOnHand != 0m)
            {
                PrepareSingleField<SiteLotSerial.qtyOnHand>(sender, row, columns);
            }
			else if (this.ValidateAvailQty(sender.Graph))
            {
                if (bal.QtyHardAvail < 0m)
                    PrepareSingleField<SiteLotSerial.qtyHardAvail>(sender, row, columns);

                if (bal.QtyAvail != 0m)
                    PrepareSingleField<SiteLotSerial.qtyAvail>(sender, row, columns);

            }
			if (bal.QtyActual < 0m && bal.LotSerAssign == INLotSerAssign.WhenReceived)
			{
				columns.Restrict<SiteLotSerial.qtyActual>(PXComp.GE, -bal.QtyActual);
            }

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero((SiteLotSerial)row))
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                var inventoryID = (int?)sender.GetValue<SiteLotSerial.inventoryID>(row);
                var siteID = (int?)sender.GetValue<SiteLotSerial.siteID>(row);
                var lotSerialNbr = (string)sender.GetValue<SiteLotSerial.lotSerialNbr>(row);

	            SiteLotSerial item = SiteLotSerial.PK.Find(sender.Graph, inventoryID, siteID, lotSerialNbr);

                item = (SiteLotSerial)this.Aggregate(sender, item, row);
                item.LotSerTrack = ((SiteLotSerial)row).LotSerTrack;
                item.LotSerAssign = ((SiteLotSerial)row).LotSerAssign;

                bool isDuplicated = false;
                Guid? refNoteID = null;

                {
                    PXResultset<INItemPlan> plans = PXSelect<INItemPlan, Where<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>, And<INItemPlan.siteID, Equal<Required<INItemPlan.siteID>>, And<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>>>.SelectWindowed(sender.Graph, 0, 10, inventoryID, siteID, lotSerialNbr);

                    List<Guid?> refs = new List<Guid?>();

                    if (plans.Count <= 1)
                    {
                        refs.Add(null);
                    }
                    else
                    {
                        Dictionary<Guid?, int> counts = new Dictionary<Guid?, int>();
                        PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                        for (int i = 0; i < plans.Count; i++)
                        {
                            refNoteID = plans[i].GetItem<INItemPlan>().RefNoteID;
                            if (cache.GetStatus(plans[i].GetItem<INItemPlan>()) == PXEntryStatus.Notchanged)
                            {
                                refs.Insert(0, refNoteID);
                            }
                            else
                            {
                                refs.Add(refNoteID);
                            }

                            if (counts.ContainsKey(refNoteID))
                            {
                                counts[refNoteID]++;
                                isDuplicated = true;
                            }
                            else
                            {
                                counts[refNoteID] = 1;
                            }
                        }
                    }
                    refNoteID = refs[0];
                }

                string message = null;
                if (((SiteLotSerial)row).LotSerTrack == INLotSerTrack.SerialNumbered && isDuplicated)
                {
                    message = Messages.SerialNumberDuplicated;
                }

                SiteLotSerial bal = (SiteLotSerial)row;

                //only in release process updates onhand.
                if (bal.QtyOnHand != 0m)
                {
                    ValidateSingleField<SiteLotSerial.qtyOnHand>(sender, item, null, ref message);
                }
				else if (this.ValidateAvailQty(sender.Graph))
                {
                    ValidateSingleField<SiteLotSerial.qtyAvail>(sender, item, refNoteID, ref message);
                }
				if (bal.QtyActual < 0m && bal.LotSerAssign == INLotSerAssign.WhenReceived)
				{
					if (item.QtyActual < 0)
					{
						throw new PXException(Messages.StatusCheck_QtyLotSerialActualNegative,
							PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.inventoryID>(sender, row),
							PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.siteID>(sender, row),
							PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.lotSerialNbr>(sender, row));
					}
				}

                string refRowID = null;
                if (refNoteID != null)
                {
                    AutoNumberedEntityHelper hlp = new AutoNumberedEntityHelper(sender.Graph);
                    refRowID = hlp.GetEntityRowID(refNoteID);
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<SiteLotSerial.lotSerialNbr>(sender, row),
                        refRowID);
                }

                throw;
            }
        }

		public virtual bool IsZero(SiteLotSerial a)
		{
			return a.IsZero();
		}
    }

    public class LotSerialStatusAccumulatorAttribute : StatusAccumulatorAttribute
    {
        public LotSerialStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            LotSerialStatus bal = (LotSerialStatus)row;

            columns.Update<LotSerialStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<LotSerialStatus.qtyFSSrvOrdPrepared>(bal.QtyFSSrvOrdPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyFSSrvOrdBooked>(bal.QtyFSSrvOrdBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyFSSrvOrdAllocated>(bal.QtyFSSrvOrdAllocated, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<LotSerialStatus.qtySOPrepared>(bal.QtySOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.lotSerTrack>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<LotSerialStatus.costID>(bal.CostID, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<LotSerialStatus.receiptDate>(bal.ReceiptDate, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<LotSerialStatus.qtyInTransitToProduction>(bal.QtyInTransitToProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProductionSupplyPrepared>(bal.QtyProductionSupplyPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProductionSupply>(bal.QtyProductionSupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyPOFixedProductionPrepared>(bal.QtyPOFixedProductionPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyPOFixedProductionOrders>(bal.QtyPOFixedProductionOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProductionDemandPrepared>(bal.QtyProductionDemandPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProductionDemand>(bal.QtyProductionDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProductionAllocated>(bal.QtyProductionAllocated, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtySOFixedProduction>(bal.QtySOFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProdFixedPurchase>(bal.QtyProdFixedPurchase, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProdFixedProduction>(bal.QtyProdFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProdFixedProdOrdersPrepared>(bal.QtyProdFixedProdOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProdFixedProdOrders>(bal.QtyProdFixedProdOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProdFixedSalesOrdersPrepared>(bal.QtyProdFixedSalesOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<LotSerialStatus.qtyProdFixedSalesOrders>(bal.QtyProdFixedSalesOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            if (bal.CostID < 0)
            {
                throw new PXException(Messages.InternalError, Messages.LS);
            }

            //only in release process updates onhand.
            if (bal.QtyOnHand < 0m)
            {
                columns.Restrict<LotSerialStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && IsZero((LotSerialStatus)row))
			{
				sender.SetStatus(row, PXEntryStatus.InsertedDeleted);
				return false;
			}

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<LotSerialStatus.inventoryID>(row);
                object subItemID = sender.GetValue<LotSerialStatus.subItemID>(row);
                object siteID = sender.GetValue<LotSerialStatus.siteID>(row);
                object locationID = sender.GetValue<LotSerialStatus.locationID>(row);
                object lotSerialNbr = sender.GetValue<LotSerialStatus.lotSerialNbr>(row);

	            LotSerialStatus item = LotSerialStatus.PK.Find(sender.Graph, (int)inventoryID, (int?)subItemID, (int?)siteID, (int?)locationID, (string)lotSerialNbr);

                item = (LotSerialStatus)this.Aggregate(sender, item, row);

                LotSerialStatus bal = (LotSerialStatus)row;

                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyLotSerialOnHandNegative;
                    }
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.subItemID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.siteID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.locationID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.lotSerialNbr>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                LotSerialStatus bal = (LotSerialStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyLotSerialOnHandNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.siteID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.locationID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.lotSerialNbr>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }
    }


    public class TransitLotSerialStatusAccumulatorAttribute : StatusAccumulatorAttribute
    {
        public TransitLotSerialStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            TransitLotSerialStatus bal = (TransitLotSerialStatus)row;

            columns.Update<TransitLotSerialStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<TransitLotSerialStatus.qtyActual>(bal.QtyActual, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<TransitLotSerialStatus.qtyFSSrvOrdPrepared>(bal.QtyFSSrvOrdPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyFSSrvOrdBooked>(bal.QtyFSSrvOrdBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyFSSrvOrdAllocated>(bal.QtyFSSrvOrdAllocated, PXDataFieldAssign.AssignBehavior.Summarize);

            columns.Update<TransitLotSerialStatus.qtySOPrepared>(bal.QtySOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.lotSerTrack>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<TransitLotSerialStatus.costID>(bal.CostID, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<TransitLotSerialStatus.receiptDate>(bal.ReceiptDate, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<TransitLotSerialStatus.qtyInTransitToProduction>(bal.QtyInTransitToProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProductionSupplyPrepared>(bal.QtyProductionSupplyPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProductionSupply>(bal.QtyProductionSupply, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyPOFixedProductionPrepared>(bal.QtyPOFixedProductionPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyPOFixedProductionOrders>(bal.QtyPOFixedProductionOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProductionDemandPrepared>(bal.QtyProductionDemandPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProductionDemand>(bal.QtyProductionDemand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProductionAllocated>(bal.QtyProductionAllocated, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtySOFixedProduction>(bal.QtySOFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProdFixedPurchase>(bal.QtyProdFixedPurchase, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProdFixedProduction>(bal.QtyProdFixedProduction, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProdFixedProdOrdersPrepared>(bal.QtyProdFixedProdOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProdFixedProdOrders>(bal.QtyProdFixedProdOrders, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProdFixedSalesOrdersPrepared>(bal.QtyProdFixedSalesOrdersPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<TransitLotSerialStatus.qtyProdFixedSalesOrders>(bal.QtyProdFixedSalesOrders, PXDataFieldAssign.AssignBehavior.Summarize);

            if (bal.CostID < 0)
            {
                throw new PXException(Messages.InternalError, Messages.LS);
            }

            //only in release process updates onhand.
            if (bal.QtyOnHand < 0m)
            {
                columns.Restrict<TransitLotSerialStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<TransitLotSerialStatus.inventoryID>(row);
                object subItemID = sender.GetValue<TransitLotSerialStatus.subItemID>(row);
                object siteID = sender.GetValue<TransitLotSerialStatus.siteID>(row);
                object locationID = sender.GetValue<TransitLotSerialStatus.locationID>(row);
                object lotSerialNbr = sender.GetValue<TransitLotSerialStatus.lotSerialNbr>(row);

	            TransitLotSerialStatus item = TransitLotSerialStatus.PK.Find(sender.Graph, (int?)inventoryID, (int?)subItemID, (int?)siteID, (int?)locationID, (string)lotSerialNbr);

                item = (TransitLotSerialStatus)this.Aggregate(sender, item, row);

                TransitLotSerialStatus bal = (TransitLotSerialStatus)row;

                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyTransitLotSerialOnHandNegative;
                    }
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<TransitLotSerialStatus.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<TransitLotSerialStatus.subItemID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<TransitLotSerialStatus.lotSerialNbr>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                TransitLotSerialStatus bal = (TransitLotSerialStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyTransitLotSerialOnHandNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<TransitLotSerialStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<TransitLotSerialStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<TransitLotSerialStatus.lotSerialNbr>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }
    }

	public class OversoldCostStatusAccumulatorAttribute : CostStatusAccumulatorAttribute
	{
		public OversoldCostStatusAccumulatorAttribute()
			: base(
				  typeof(OversoldCostStatus.qtyOnHand),
				  typeof(OversoldCostStatus.totalCost),
				  typeof(OversoldCostStatus.inventoryID),
				  typeof(OversoldCostStatus.costSubItemID),
				  typeof(OversoldCostStatus.costSiteID),
				  typeof(OversoldCostStatus.layerType),
				  typeof(OversoldCostStatus.receiptNbr))
		{
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
				return false;

			INCostStatus layer = (INCostStatus)row;
			columns.Update<INCostStatus.valMethod>(layer.ValMethod, PXDataFieldAssign.AssignBehavior.Replace);

			return true;
		}
	}

    public class CostStatusAccumulatorAttribute : PXAccumulatorAttribute
    {
        protected Type _QuantityField;
        protected Type _CostField;
        protected Type _InventoryIDField;
        protected Type _SubItemIDField;
        protected Type _SiteIDField;
        protected Type _SpecificNumberField;
        protected Type _LayerTypeField;
        protected Type _ReceiptNbr;

        public CostStatusAccumulatorAttribute(Type quantityField, Type costField, Type inventoryIDField, Type subItemIDField, Type siteIDField, Type specificNumberField, Type layerTypeField, Type receiptNbr)
            : this()
        {
            _QuantityField = quantityField;
            _CostField = costField;
            _InventoryIDField = inventoryIDField;
            _SubItemIDField = subItemIDField;
            _SiteIDField = siteIDField;
            _SpecificNumberField = specificNumberField;
            _LayerTypeField = layerTypeField;
            _ReceiptNbr = receiptNbr;
        }
        public CostStatusAccumulatorAttribute(Type quantityField, Type costField, Type inventoryIDField, Type subItemIDField, Type siteIDField, Type layerTypeField, Type receiptNbr)
            : this(quantityField, costField, inventoryIDField, subItemIDField, siteIDField, null, layerTypeField, receiptNbr)
        {
        }
        public CostStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            INCostStatus bal = (INCostStatus)row;
            if (bal.LayerType != INLayerType.Unmanaged)
            {
                columns.AppendException(_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2,
                    new PXAccumulatorRestriction(_QuantityField.Name, PXComp.GE, 0m),
                    new PXAccumulatorRestriction(_LayerTypeField.Name, PXComp.EQ, INLayerType.Oversold));
                columns.AppendException(_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2,
                    new PXAccumulatorRestriction(_QuantityField.Name, PXComp.LE, 0m),
                    new PXAccumulatorRestriction(_LayerTypeField.Name, PXComp.EQ, INLayerType.Normal));
                columns.AppendException(Messages.StatusCheck_QtyCostImblance,
                    new PXAccumulatorRestriction(_QuantityField.Name, PXComp.NE, 0m),
                    new PXAccumulatorRestriction(_CostField.Name, PXComp.EQ, 0m));
                columns.AppendException(Messages.StatusCheck_QtyCostImblance,
                    new PXAccumulatorRestriction(_QuantityField.Name, PXComp.LE, 0m),
                    new PXAccumulatorRestriction(_CostField.Name, PXComp.GE, 0m));
                columns.AppendException(Messages.StatusCheck_QtyCostImblance,
                    new PXAccumulatorRestriction(_QuantityField.Name, PXComp.GE, 0m),
                    new PXAccumulatorRestriction(_CostField.Name, PXComp.LE, 0m));
            }
            else
            {
                columns.AppendException(Messages.StatusCheck_QtyCostImblance,
                    new PXAccumulatorRestriction(_QuantityField.Name, PXComp.NE, 0m));
                columns.AppendException(Messages.StatusCheck_QtyCostImblance,
                    new PXAccumulatorRestriction(_CostField.Name, PXComp.NE, 0m));
            }
            

            columns.Update<INCostStatus.unitCost>(bal.UnitCost, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<INCostStatus.origQty>(bal.OrigQty, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<INCostStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<INCostStatus.totalCost>(bal.TotalCost, PXDataFieldAssign.AssignBehavior.Summarize);

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXRestrictionViolationException e)
            {
                List<object> pars = new List<object>();
                if (sender.BqlKeys.Contains(_InventoryIDField))
                {
                    pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _InventoryIDField.Name));
                }
                if (sender.BqlKeys.Contains(_SubItemIDField))
                {
                    pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _SubItemIDField.Name));
                }
                if (sender.BqlKeys.Contains(_SiteIDField))
                {
                    pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _SiteIDField.Name));
                }
                if ((e.Index == 0 || e.Index == 1) && _SpecificNumberField != null && sender.BqlKeys.Contains(_SpecificNumberField))
                {
                    pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _SpecificNumberField.Name));
                }
                throw new PXException((e.Index == 0 || e.Index == 1) ? (_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2) : Messages.StatusCheck_QtyCostImblance, pars.ToArray());
            }
        }
    }

    public class ReceiptStatusAccumulatorAttribute : PXAccumulatorAttribute
    {
        public ReceiptStatusAccumulatorAttribute()
        {
            base._SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ReceiptStatus bal = (ReceiptStatus)row;

            columns.Update<ReceiptStatus.origQty>(bal.OrigQty, PXDataFieldAssign.AssignBehavior.Initialize);
            columns.Update<ReceiptStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Restrict<ReceiptStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);

            return true;
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException e)
            {
                ReceiptStatus newQty = (ReceiptStatus)row;
                ReceiptStatus oldQty;

                List<Type> bql = new List<Type>()
                {
                    typeof(Select<,>),
                    typeof(ReceiptStatus),
                    typeof(Where<,,>),
                    typeof(ReceiptStatus.inventoryID),
                    typeof(Equal<Current<ReceiptStatus.inventoryID>>),
                    typeof(And<,,>),
                    typeof(ReceiptStatus.costSiteID),
                    typeof(Equal<Current<ReceiptStatus.costSiteID>>),
                    typeof(And<,,>),
                    typeof(ReceiptStatus.costSubItemID),
                    typeof(Equal<Current<ReceiptStatus.costSubItemID>>),
                    typeof(And<,,>),
                    typeof(ReceiptStatus.accountID),
                    typeof(Equal<Current<ReceiptStatus.accountID>>) };


                if (newQty.ValMethod == INValMethod.Specific)
                {
                    bql.Add(typeof(And<,,>));
                    bql.Add(typeof(ReceiptStatus.subID));
                    bql.Add(typeof(Equal<Current<ReceiptStatus.subID>>));
                    bql.Add(typeof(And<,>));
                    bql.Add(typeof(ReceiptStatus.lotSerialNbr));
                    bql.Add(typeof(Equal<Current<ReceiptStatus.lotSerialNbr>>));
                }
                else
                {
                    bql.Add(typeof(And<,>));
                    bql.Add(typeof(ReceiptStatus.subID));
                    bql.Add(typeof(Equal<Current<ReceiptStatus.subID>>));
                }

                oldQty =
                    (ReceiptStatus)new PXView(sender.Graph, true, BqlCommand.CreateInstance(bql.ToArray()))
                        .SelectSingleBound(new object[] { newQty });

                if (((oldQty == null ? 0m : oldQty.QtyOnHand) + newQty.QtyOnHand) < 0m)
                {
                    throw new PXRestartOperationException(e);
                }
                throw;
            }
        }
    }



    public class CostSiteIDAttribute : PXForeignSelectorAttribute
    {
        public CostSiteIDAttribute()
            : base(typeof(INTran.locationID))
        {
        }

        protected override object GetValueExt(PXCache sender, object item)
        {
            object val = sender.GetValue(item, _FieldOrdinal);
            object copyval = val;
            string result = string.Empty;

			INLocation loc = INLocation.PK.Find(sender.Graph, (int?)val);
			INSite site;
			if (loc == null)
			{
				loc = PXSelectReadonly<INLocation, Where<INLocation.siteID, Equal<Required<INLocation.siteID>>>>.SelectWindowed(sender.Graph, 0, 1, (int?)val);
				if(loc == null)
				{
					var insetup = (INSetup)PXSetup<INSetup>.SelectWindowed(sender.Graph, 0, 1);
					if (insetup.TransitSiteID == (int?)val)
					{
						site = INSite.PK.Find(sender.Graph, (int?)val);
						return site.SiteCD;
					}
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, Messages.INLocation, nameof(INLocation.siteID) + " = " + val));
				}
				else
					site = INSite.PK.Find(sender.Graph, loc.SiteID);
			}
			else
				site = INSite.PK.Find(sender.Graph, loc.SiteID);

            val = site.SiteCD;
            sender.Graph.Caches[typeof(INSite)].RaiseFieldSelecting<INSite.siteCD>(loc, ref val, true);
            if (val is PXStringState && string.IsNullOrEmpty(((PXStringState)val).InputMask) == false)
            {
                result = PX.Common.Mask.Format(((PXStringState)val).InputMask, (string)((PXStringState)val).Value);
            }
            else if (val is PXFieldState && ((PXStringState)val).Value is string)
            {
                result = (string)((PXFieldState)val).Value;
            }

            if (loc.LocationID == (int?)copyval)
            {
                val = loc.LocationCD;
                sender.Graph.Caches[typeof(INLocation)].RaiseFieldSelecting<INLocation.locationCD>(loc, ref val, true);

                if (val is PXStringState && string.IsNullOrEmpty(((PXStringState)val).InputMask) == false)
                {
                    result += ' '.ToString();
                    result += PX.Common.Mask.Format(((PXStringState)val).InputMask, (string)((PXStringState)val).Value);
                }
                else if (val is PXFieldState && ((PXStringState)val).Value is string)
                {
                    result += ' '.ToString();
                    result += (string)((PXFieldState)val).Value;
                }
            }
            return result;
        }
    }

    public class CostIdentityAttribute : PXDBLongIdentityAttribute
    {
        #region State
        protected new long? _KeyToAbort = null;
        protected Type[] _ChildTypes = null;
        #endregion

        #region Ctor
        public CostIdentityAttribute(params Type[] ChildTypes)
            : base()
        {
            _ChildTypes = ChildTypes;
        }
        #endregion

        #region Implementation
        public long? SelectAccumIdentity(PXCache sender, object Row)
        {
            List<PXDataField> fields = new List<PXDataField>();

            fields.Add(new PXDataField(_FieldName));

            foreach (string key in sender.Keys)
            {
                fields.Add(new PXDataFieldValue(key, sender.GetValue(Row, key)));
            }

            using (PXDataRecord UpdatedRow = PXDatabase.SelectSingle(sender.BqlTable, fields.ToArray()))
            {
                if (UpdatedRow != null)
                {
                    return UpdatedRow.GetInt64(0);
                }
            }
            return null; ;
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (e.TranStatus == PXTranStatus.Open)
                {
                    this._KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);

                    base.RowPersisted(sender, e);

                    if (_KeyToAbort < 0)
                    {
                        long? _NewKey = SelectAccumIdentity(sender, e.Row);
                        for (int i = 0; i < _ChildTypes.Length; i++)
                        {
                            PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
                            foreach (object item in cache.Inserted)
                            {
                                if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _KeyToAbort)
                                {
                                    cache.SetValue(item, _ChildTypes[i].Name, _NewKey);
                                }
                            }
                        }
                    }
                }
                else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
                {
                    long? _NewKey = (long?)sender.GetValue(e.Row, _FieldOrdinal);
                    for (int i = 0; i < _ChildTypes.Length; i++)
                    {
                        PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
                        foreach (object item in cache.Inserted)
                        {
                            if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _NewKey)
                            {
                                cache.SetValue(item, _ChildTypes[i].Name, _KeyToAbort);
                            }
                        }
                    }
                    _KeyToAbort = null;
                    base.RowPersisted(sender, e);
                }
            }

            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
            {
                if (e.TranStatus == PXTranStatus.Open)
                {
                    this._KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);

                    base.RowPersisted(sender, e);

                    if (_KeyToAbort < 0)
                    {
                        long? _NewKey = Convert.ToInt64(PXDatabase.SelectIdentity());
                        for (int i = 0; i < _ChildTypes.Length; i++)
                        {
                            PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
                            foreach (object item in cache.Inserted)
                            {
                                if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _KeyToAbort)
                                {
                                    cache.SetValue(item, _ChildTypes[i].Name, _NewKey);
                                }
                            }
                        }
                    }
                }
                else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
                {
                    long? _NewKey = (long?)sender.GetValue(e.Row, _FieldOrdinal);
                    for (int i = 0; i < _ChildTypes.Length; i++)
                    {
                        PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
                        foreach (object item in cache.Inserted)
                        {
                            if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _NewKey)
                            {
                                cache.SetValue(item, _ChildTypes[i].Name, _KeyToAbort);
                            }
                        }
                    }
                    _KeyToAbort = null;
                    base.RowPersisted(sender, e);
                }
            }
        }
        #endregion
    }

    [PXHidden]
    public class NonStockItem : IBqlTable
    {
        #region InvtAcctID
        public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
        protected Int32? _InvtAcctID;
        [PXInt]
        [PXUIField(DisplayName = "Expense Accrual Account")]
        public virtual Int32? InvtAcctID
        {
            get
            {
                return this._InvtAcctID;
            }
            set
            {
                this._InvtAcctID = value;
            }
        }
        #endregion
        #region InvtSubID
        public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
        protected Int32? _InvtSubID;
        [PXInt]
        [PXUIField(DisplayName = "Expense Accrual Sub.")]
        public virtual Int32? InvtSubID
        {
            get
            {
                return this._InvtSubID;
            }
            set
            {
                this._InvtSubID = value;
            }
        }
        #endregion
        #region COGSAcctID
        public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
        protected Int32? _COGSAcctID;
        [PXInt]
        [PXUIField(DisplayName = "Expense Account")]
        public virtual Int32? COGSAcctID
        {
            get
            {
                return this._COGSAcctID;
            }
            set
            {
                this._COGSAcctID = value;
            }
        }
        #endregion
        #region COGSSubID
        public abstract class cOGSSubID : PX.Data.BQL.BqlInt.Field<cOGSSubID> { }
        protected Int32? _COGSSubID;
        [PXInt]
        [PXUIField(DisplayName = "Expense Sub.")]
        public virtual Int32? COGSSubID
        {
            get
            {
                return this._COGSSubID;
            }
            set
            {
                this._COGSSubID = value;
            }
        }
        #endregion
    }

}
