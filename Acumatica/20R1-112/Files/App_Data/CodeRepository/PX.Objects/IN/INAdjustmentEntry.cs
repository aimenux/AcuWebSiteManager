using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.IN
{
	public class INAdjustmentEntry : INRegisterEntryBase, IGraphWithInitialization
	{
		public PXSelect<INRegister, Where<INRegister.docType, Equal<INDocType.adjustment>>> adjustment;
		public PXSelect<INRegister, Where<INRegister.docType, Equal<Current<INRegister.docType>>, And<INRegister.refNbr, Equal<Current<INRegister.refNbr>>>>> CurrentDocument;
        [PXImport(typeof(INRegister))]
		public PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>> transactions;

		[PXCopyPasteHiddenView()]
		public PXSelect<INTranSplit, Where<INTranSplit.tranType, Equal<Argument<string>>, And<INTranSplit.refNbr, Equal<Argument<string>>, And<INTranSplit.lineNbr, Equal<Argument<Int16?>>>>>> splits;

		public LSINAdjustmentTran lsselect;

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXFormula(typeof(Mult<INTran.qty, INTran.unitCost>), typeof(SumCalc<INRegister.totalCost>))]
		public virtual void INTran_TranCost_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>))]
		[INUnit(typeof(INTran.inventoryID))]
		public virtual void INTran_UOM_CacheAttached(PXCache sender)
		{
		}

		[PXString(2)]
		[PXFormula(typeof(Parent<INRegister.origModule>))]
		public virtual void INTran_OrigModule_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXRestrictorAttribute))]
		[PXRestrictor(typeof(Where<ReasonCode.usage, Equal<Optional<INTran.docType>>,
			Or<ReasonCode.usage, Equal<ReasonCodeUsages.vendorReturn>, And<Optional<INTran.origModule>, Equal<BatchModule.modulePO>>>>),
			Messages.ReasonCodeDoesNotMatch)]
		public virtual void INTran_ReasonCode_CacheAttached(PXCache sender)
		{
		}

		[IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]
		public virtual void INTranSplit_LocationID_CacheAttached(PXCache sender)
		{ 
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INTranSplitPlanID(typeof(INRegister.noteID), typeof(INRegister.hold), typeof(INRegister.transferType))]
		protected virtual void INTranSplit_PlanID_CacheAttached(PXCache sender)
		{
		}

		public PXAction<INRegister> viewBatch;
		[PXUIField(DisplayName = "Review Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (adjustment.Current != null && !String.IsNullOrEmpty(adjustment.Current.BatchNbr))
			{
				JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<Batch.batchNbr>(adjustment.Current.BatchNbr, "IN");
				throw new PXRedirectRequiredException(graph, "Current batch record");
			}
			return adapter.Get();
		}

		public PXAction<INRegister> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = adjustment.Cache;
			List<INRegister> list = new List<INRegister>();
			foreach (INRegister indoc in adapter.Get<INRegister>())
			{
				if (indoc.Hold == false && indoc.Released == false)
				{
					cache.Update(indoc);
					list.Add(indoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, delegate() { INDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

        #region MyButtons (MMK)
        public PXAction<INRegister> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

		public PXAction<INRegister> iNEdit;
		[PXUIField(DisplayName = Messages.INEditDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INEdit(PXAdapter adapter)
		{
			if (adjustment.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["DocType"] = adjustment.Current.DocType;
				parameters["RefNbr"] = adjustment.Current.RefNbr;
				parameters["PeriodFrom"] = null;
				parameters["PeriodTo"] = null;
				throw new PXReportRequiredException(parameters, "IN611000", Messages.INEditDetails);
			}
			return adapter.Get();
		}

		public PXAction<INRegister> iNRegisterDetails;
		[PXUIField(DisplayName = Messages.INRegisterDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INRegisterDetails(PXAdapter adapter)
		{
			if (adjustment.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["PeriodID"] = (string)adjustment.GetValueExt<INRegister.finPeriodID>(adjustment.Current);
				parameters["DocType"] = adjustment.Current.DocType;
				parameters["RefNbr"] = adjustment.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "IN614000", Messages.INRegisterDetails);
			}
			return adapter.Get();
		}

		public PXAction<INRegister> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache		tCache = transactions.Cache;
			INTran		line   = transactions.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
			if (item != null && item.StkItem == true)
			{
				INSubItem	sbitem = (INSubItem) PXSelectorAttribute.Select<INTran.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID, 
										     ((sbitem != null) ? sbitem.SubItemCD : null), 
										     line.SiteID, 
										     line.LocationID);
			}
			return adapter.Get();
		}
		#region SiteStatus Lookup
		public PXFilter<INSiteStatusFilter> sitestatusfilter;
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public INSiteStatusLookup<INSiteStatusSelected, INSiteStatusFilter> sitestatus;

		public PXAction<INRegister> addInvBySite;
		[PXUIField(DisplayName = "Add Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddInvBySite(PXAdapter adapter)
		{
			sitestatusfilter.Cache.Clear();
			if (sitestatus.AskExt() == WebDialogResult.OK)
			{
				return AddInvSelBySite(adapter);
			}
			sitestatusfilter.Cache.Clear();
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<INRegister> addInvSelBySite;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
		{
			transactions.Cache.ForceExceptionHandling = true;

			foreach (INSiteStatusSelected line in sitestatus.Cache.Cached)
			{
				if (line.Selected == true && line.QtySelected > 0)
				{
					INTran newline = PXCache<INTran>.CreateCopy(this.transactions.Insert(new INTran()));
					newline.SiteID = line.SiteID;
					newline.InventoryID = line.InventoryID;
					newline.SubItemID = line.SubItemID;
					newline.UOM = line.BaseUnit;
					newline = PXCache<INTran>.CreateCopy(transactions.Update(newline));
					if (line.LocationID != null)
					{
					newline.LocationID = line.LocationID;
					newline = PXCache<INTran>.CreateCopy(transactions.Update(newline));
					}
					newline.Qty = line.QtySelected;
					transactions.Update(newline);
				}
			}
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		protected virtual void INSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			INSiteStatusFilter row = (INSiteStatusFilter)e.Row;
			if (row != null && adjustment.Current != null)
				row.SiteID = adjustment.Current.SiteID;
		}
		#endregion

		public INAdjustmentEntry()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<INTran.tranType>(transactions.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INTran.tranType>(transactions.Cache, null, false);

            //PXDimensionSelectorAttribute.SetValidCombo<INTran.subItemID>(transactions.Cache, true);
            //PXDimensionSelectorAttribute.SetValidCombo<INTranSplit.subItemID>(splits.Cache, true);

			PXVerifySelectorAttribute.SetVerifyField<INTran.origRefNbr>(transactions.Cache, null, true);
		}

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<INRegister>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(INTran), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<INTran.docType>(PXDbType.Char, ((INAdjustmentEntry)graph).adjustment.Current?.DocType),
							new PXDataFieldValue<INTran.refNbr>(((INAdjustmentEntry)graph).adjustment.Current?.RefNbr),
							new PXDataFieldValue<INTran.createdByScreenID>(PXDbType.Char, "IN303000")
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(INTranSplit), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<INTranSplit.docType>(PXDbType.Char, ((INAdjustmentEntry)graph).adjustment.Current?.DocType),
							new PXDataFieldValue<INTranSplit.refNbr>(((INAdjustmentEntry)graph).adjustment.Current?.RefNbr),
							new PXDataFieldValue<INTranSplit.createdByScreenID>(PXDbType.Char, "IN303000")
						};
					}));
			}
		}

		public virtual void Splits(
			[PXDBString(3, IsFixed = true)]
			ref string INTran_tranType,
			[PXDBString(10, IsUnicode = true)]
			ref string INTran_refNbr,
			[PXDBShort()]
			ref Int16? INTran_lineNbr)
		{
			transactions.Current = (INTran)PXSelect<INTran, Where<INTran.tranType, Equal<Required<INTran.tranType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>, And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>>>>>.Select(this, INTran_tranType, INTran_refNbr, INTran_lineNbr);
		}

		protected virtual void INRegister_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Adjustment;
		}

		protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (insetup.Current.RequireControlTotal == false)
			{
				if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalCost) == false)
				{
					sender.SetValue<INRegister.controlCost>(e.Row, ((INRegister)e.Row).TotalCost);
				}
				else
				{
					sender.SetValue<INRegister.controlCost>(e.Row, 0m);
				}

				if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalQty) == false)
				{
					sender.SetValue<INRegister.controlQty>(e.Row, ((INRegister)e.Row).TotalQty);
				}
				else
				{
					sender.SetValue<INRegister.controlQty>(e.Row, 0m);
				}
			}

			if (((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false)
			{
				if ((bool)insetup.Current.RequireControlTotal)
				{
					if (((INRegister)e.Row).TotalCost != ((INRegister)e.Row).ControlCost)
					{
						sender.RaiseExceptionHandling<INRegister.controlCost>(e.Row, ((INRegister)e.Row).ControlCost, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<INRegister.controlCost>(e.Row, ((INRegister)e.Row).ControlCost, null);
					}

					if (((INRegister)e.Row).TotalQty != ((INRegister)e.Row).ControlQty)
					{
						sender.RaiseExceptionHandling<INRegister.controlQty>(e.Row, ((INRegister)e.Row).ControlQty, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<INRegister.controlQty>(e.Row, ((INRegister)e.Row).ControlQty, null);
					}
				}
			}
		}

		protected virtual void INRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			bool allowUpdateDelete = ((((INRegister)e.Row).Released == false) && (((INRegister)e.Row).OrigModule != BatchModule.AP));
            //restriction added primary for Landed Cost
            bool allowInsertDeleteDetails = (((INRegister)e.Row).OrigModule != BatchModule.PO);
			bool isPIAdjustment = !string.IsNullOrEmpty(((INRegister)e.Row).PIID);

            release.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNEdit.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNRegisterDetails.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == true);
			addInvBySite.SetEnabled(e.Row != null && allowUpdateDelete && allowInsertDeleteDetails && !isPIAdjustment);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, ((INRegister)e.Row).Released == false);
			PXUIFieldAttribute.SetEnabled<INRegister.totalQty>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.totalCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.status>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = allowUpdateDelete;
			sender.AllowDelete = allowUpdateDelete && allowInsertDeleteDetails;

			transactions.Cache.AllowInsert = allowUpdateDelete && allowInsertDeleteDetails && !isPIAdjustment;
			transactions.Cache.AllowUpdate = allowUpdateDelete;
			transactions.Cache.AllowDelete = allowUpdateDelete && allowInsertDeleteDetails && !isPIAdjustment;

			splits.Cache.AllowInsert = allowUpdateDelete;
			splits.Cache.AllowUpdate = allowUpdateDelete;
			splits.Cache.AllowDelete = allowUpdateDelete;

			PXUIFieldAttribute.SetVisible<INRegister.controlQty>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INRegister.controlCost>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INRegister.pIID>(sender, e.Row, !string.IsNullOrEmpty(((INRegister)e.Row).PIID));
		}

		protected virtual void INRegister_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var adjustment = (INRegister)e.Row;
			if (e.Operation.Command() != PXDBOperation.Delete || string.IsNullOrEmpty(adjustment?.PIID))
				return;

			INPIController piReview = PXGraph.CreateInstance<INPIController>();
			piReview.ReopenPI(adjustment.PIID);
		}

		protected virtual void INTran_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Adjustment;
		}

        [PXDBString(3, IsFixed = true)]
        [PXDefault(INTranType.Adjustment)]
        [PXUIField(Enabled = false, Visible = false)]
        protected virtual void INTran_TranType_CacheAttached(PXCache sender)
        {
        }

		protected virtual void INTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INTranType.InvtMult(((INTran)e.Row).TranType);
		}

		protected virtual void INTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INTran.uOM>(e.Row);
			sender.SetDefaultExt<INTran.tranDesc>(e.Row);
		}

		protected virtual void INTran_UnitCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetupManualCostFlag(sender, (INTran)e.Row, e.ExternalCall);
		}

		protected virtual void INTran_TranCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetupManualCostFlag(sender, (INTran)e.Row, e.ExternalCall);
		}

		protected virtual void SetupManualCostFlag(PXCache inTranCache, INTran tran, bool externalCall)
		{
			if (string.IsNullOrEmpty(adjustment.Current?.PIID) || !externalCall)
				return;

			if (tran == null || tran.Qty <= 0 || tran.ManualCost == true)
				return;

			inTranCache.SetValueExt<INTran.manualCost>(tran, true);
		}

		protected virtual void INTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_LotSerialNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_OrigRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null && ((INTran)e.Row).TranType == INTranType.ReceiptCostAdjustment)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTran_OrigRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			InventoryItem item = InventoryItem.PK.Find(this, ((INTran)e.Row)?.InventoryID);
			bool noItemOrNonStandard = item == null || item.ValMethod != INValMethod.Standard;

			bool isPIAdjustment = !string.IsNullOrEmpty(adjustment.Current?.PIID);
			bool isFIFOCreditLineFromPI = isPIAdjustment && ((INTran)e.Row)?.Qty < 0 && item?.ValMethod == INValMethod.FIFO;
			bool isDebitLineFromPI = isPIAdjustment && ((INTran)e.Row)?.Qty > 0;

			PXUIFieldAttribute.SetEnabled<INTran.branchID>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.inventoryID>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.siteID>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.locationID>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.qty>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.uOM>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.unitCost>(sender, e.Row, !isPIAdjustment && noItemOrNonStandard || isDebitLineFromPI);
			PXUIFieldAttribute.SetEnabled<INTran.tranCost>(sender, e.Row, !isPIAdjustment || isDebitLineFromPI);
			PXUIFieldAttribute.SetEnabled<INTran.expireDate>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.origRefNbr>(sender, e.Row, !isPIAdjustment || isFIFOCreditLineFromPI);
			PXUIFieldAttribute.SetEnabled<INTran.reasonCode>(sender, e.Row, !isPIAdjustment);
			PXUIFieldAttribute.SetEnabled<INTran.tranDesc>(sender, e.Row, !isPIAdjustment);

			// To force field disabling as INLotSerialNbrAttribute overrides enable\disable logic of PXUIFieldAttribute
			sender.Adjust<INLotSerialNbrAttribute>(e.Row).For<INTran.lotSerialNbr>(a => a.ForceDisable = isPIAdjustment);

			PXUIFieldAttribute.SetVisible<INTran.manualCost>(sender, null, isPIAdjustment);
			PXUIFieldAttribute.SetVisible<INTran.pILineNbr>(sender, null, isPIAdjustment);
		}

		protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            if (adjustment.Current != null && adjustment.Current.OrigModule == INRegister.origModule.PI)
            {
                return;
            }

			object UnitCost = null;

			InventoryItem item = InventoryItem.PK.Find(this, ((INTran)e.Row)?.InventoryID);

			if (item.ValMethod == INValMethod.Specific && string.IsNullOrEmpty(((INTran)e.Row).LotSerialNbr) == false)
			{
				INCostStatus status = PXSelectJoin<INCostStatus, 
					LeftJoin<INLocation, On<INLocation.locationID,Equal<Current<INTran.locationID>>>, 
					InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>>, 
					Where<INCostStatus.inventoryID, Equal<Current<INTran.inventoryID>>, And2<Where<INLocation.isCosted, Equal<boolFalse>, And<INCostStatus.costSiteID, Equal<Current<INTran.siteID>>, Or<INCostStatus.costSiteID, Equal<Current<INTran.locationID>>>>>, And<INCostSubItemXRef.subItemID, Equal<Current<INTran.subItemID>>, And<INCostStatus.lotSerialNbr, Equal<Current<INTran.lotSerialNbr>>>>>>>.SelectSingleBound(this, new object[] { e.Row });
				if (status != null && status.QtyOnHand != 0m)
				{
					UnitCost = PXDBPriceCostAttribute.Round((decimal)(status.TotalCost / status.QtyOnHand));
				}
			} 
			else if (item.ValMethod == INValMethod.FIFO && string.IsNullOrEmpty(((INTran)e.Row).OrigRefNbr) == false)
			{
				INCostStatus status = PXSelectJoin<INCostStatus,
					LeftJoin<INLocation, On<INLocation.locationID, Equal<Current<INTran.locationID>>>, 
					InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>>, 
					Where<INCostStatus.inventoryID, Equal<Current<INTran.inventoryID>>, And2<Where<INLocation.isCosted, Equal<boolFalse>, And<INCostStatus.costSiteID, Equal<Current<INTran.siteID>>, Or<INCostStatus.costSiteID, Equal<Current<INTran.locationID>>>>>, And<INCostSubItemXRef.subItemID, Equal<Current<INTran.subItemID>>, And<INCostStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>>>>>>.SelectSingleBound(this, new object[] { e.Row });
				if (status != null && status.QtyOnHand != 0m)
				{
					UnitCost = PXDBPriceCostAttribute.Round((decimal)(status.TotalCost / status.QtyOnHand));
				}
			}
			else
			{
				if (item.ValMethod == INValMethod.Average)
				{
					sender.RaiseFieldDefaulting<INTran.avgCost>(e.Row, out UnitCost);
				}
				if (UnitCost == null || (decimal)UnitCost == 0m)
				{
					sender.RaiseFieldDefaulting<INTran.unitCost>(e.Row, out UnitCost);
				}
			}


			decimal? qty = (decimal?)sender.GetValue<INTran.qty>(e.Row);

			if (UnitCost != null && ((decimal)UnitCost != 0m || qty < 0m))
			{
                if ((decimal)UnitCost < 0m)
                    sender.RaiseFieldDefaulting<INTran.unitCost>(e.Row, out UnitCost);

                decimal? unitcost = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitCost, INPrecision.UNITCOST);

                //suppress trancost recalculation for cost only adjustments
                if (qty == 0m)
				{
					sender.SetValue<INTran.unitCost>(e.Row, unitcost);
				}
				else
				{
					sender.SetValueExt<INTran.unitCost>(e.Row, unitcost);
				}
			}
		}

		protected virtual void INTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			InventoryItem item = InventoryItem.PK.Find(this, ((INTran)e.Row)?.InventoryID);

			if (item != null && item.ValMethod == INValMethod.Standard && ((INTran)e.Row).TranType == INTranType.Adjustment && ((INTran)e.Row).InvtMult != 0 && ((INTran)e.Row).BaseQty == 0m && ((INTran)e.Row).TranCost != 0m)
			{
				sender.RaiseExceptionHandling<INTran.tranCost>(e.Row, ((INTran)e.Row).TranCost, new PXSetPropertyException(Messages.StandardCostNoCostOnlyAdjust));
			}
		}

		protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Delete)
				return;

			INTran row = (INTran)e.Row;

			InventoryItem item = InventoryItem.PK.Find(this, row?.InventoryID);
			INLotSerClass lotSerClass = INLotSerClass.PK.Find(this, item?.LotSerClassID);

			PXPersistingCheck check =
				((INTran)e.Row).InvtMult != 0 && (
				(item != null && item.ValMethod == INValMethod.Specific) ||
				(lotSerClass != null &&
				 lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered &&
				 lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived &&
				 ((INTran)e.Row).Qty != 0m))
				 ? PXPersistingCheck.NullOrBlank
				 : PXPersistingCheck.Nothing;


			PXDefaultAttribute.SetPersistingCheck<INTran.subID>(sender, e.Row, PXPersistingCheck.Null);
			PXDefaultAttribute.SetPersistingCheck<INTran.locationID>(sender, e.Row, PXPersistingCheck.Null);
			PXDefaultAttribute.SetPersistingCheck<INTran.lotSerialNbr>(sender, e.Row, check);

			if (adjustment.Current != null && adjustment.Current.OrigModule != INRegister.origModule.PI && item != null && item.ValMethod == INValMethod.FIFO && ((INTran)e.Row).OrigRefNbr == null)
			{
                bool dropShipPO = false;
                if (row != null && row.POReceiptNbr != null && row.POReceiptLineNbr != null)
                {
                    PO.POReceiptLine pOReceiptLine = PO.POReceiptLine.PK.Find(this, row.POReceiptNbr, row.POReceiptLineNbr);
                    dropShipPO = pOReceiptLine != null && (pOReceiptLine.LineType == PO.POLineType.GoodsForDropShip || pOReceiptLine.LineType == PO.POLineType.NonStockForDropShip);
                }
                if (!dropShipPO && sender.RaiseExceptionHandling<INTran.origRefNbr>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(INTran.origRefNbr).Name)))
				{
					throw new PXRowPersistingException(typeof(INTran.origRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(INTran.origRefNbr).Name);
				}
			}

			if (item != null && item.ValMethod == INValMethod.Standard && row.TranType == INTranType.Adjustment && row.InvtMult != 0 && row.BaseQty == 0m && row.TranCost != 0m)
			{
				if (sender.RaiseExceptionHandling<INTran.tranCost>(e.Row, row.TranCost, new PXSetPropertyException(Messages.StandardCostNoCostOnlyAdjust)))
				{
					throw new PXRowPersistingException(typeof(INTran.tranCost).Name, row.TranCost, Messages.StandardCostNoCostOnlyAdjust);
				}
			}
		}

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
            Search<INItemSite.tranUnitCost, Where<INItemSite.inventoryID, Equal<Current<INTran.inventoryID>>, And<INItemSite.siteID, Equal<Current<INTran.siteID>>>>>,
            Search<INItemCost.tranUnitCost, Where<INItemCost.inventoryID, Equal<Current<INTran.inventoryID>>>>>))]
        [PXUIField(DisplayName = "Unit Cost")]
        protected virtual void INTran_UnitCost_CacheAttached(PXCache sender)
        {
        }

		#region INRegisterBaseEntry implementation
		public override PXSelectBase<INRegister> INRegisterDataMember => adjustment;
		public override PXSelectBase<INTran> INTranDataMember => transactions;
		public override LSINTran LSSelectDataMember => lsselect;
		public override PXSelectBase<INTranSplit> INTranSplitDataMember => splits;
		
		#endregion
	}
}
