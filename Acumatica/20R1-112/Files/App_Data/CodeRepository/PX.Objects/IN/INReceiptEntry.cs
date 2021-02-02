using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System.Text;
using PX.Data.DependencyInjection;
using System.Linq;
using PX.LicensePolicy;

namespace PX.Objects.IN
{
	public class INReceiptEntry : INRegisterEntryBase, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
		public PXSelect<INRegister, Where<INRegister.docType, Equal<INDocType.receipt>>> receipt;
		public PXSelect<INRegister, Where<INRegister.docType, Equal<Current<INRegister.docType>>, And<INRegister.refNbr, Equal<Current<INRegister.refNbr>>>>> CurrentDocument; 
        [PXImport(typeof(INRegister))]
		public PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>> transactions;

		[PXCopyPasteHiddenView()]
		public PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Current<INTran.docType>>, And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>, And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>> splits; // using Current<> is valid, PXSyncGridParams used in aspx

		public LSINTran lsselect;
        public PXSelect<INCostSubItemXRef> costsubitemxref;
        public PXSelect<INItemSite> initemsite;

        #region CacheAttached
        #region INTran
        [PXDefault(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>), 
			SourceField = typeof(InventoryItem.purchaseUnit), CacheGlobal = true)]
        [INUnit(typeof(INTran.inventoryID))]
		public virtual void INTran_UOM_CacheAttached(PXCache sender)
		{ 
		}

		[PXString(2)]
		[PXFormula(typeof(Parent<INRegister.origModule>))]
		public virtual void INTran_OrigModule_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		public PXAction<INRegister> viewBatch;
		[PXUIField(DisplayName = "Review Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (receipt.Current != null && !String.IsNullOrEmpty(receipt.Current.BatchNbr))
			{
				GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(receipt.Current.BatchNbr, "IN");
				throw new PXRedirectRequiredException(graph, "Current batch record");
			}
			return adapter.Get();
		}

		public PXAction<INRegister> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = receipt.Cache;
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
			if (receipt.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["DocType"] = receipt.Current.DocType;
				parameters["RefNbr"] = receipt.Current.RefNbr;
				parameters["PeriodTo"] = null;
				parameters["PeriodFrom"] = null;
				throw new PXReportRequiredException(parameters, "IN611000", Messages.INEditDetails);
			}
			return adapter.Get();
		}

		public PXAction<INRegister> iNRegisterDetails;
		[PXUIField(DisplayName = Messages.INRegisterDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INRegisterDetails(PXAdapter adapter)
		{
			if (receipt.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["PeriodID"] = (string)receipt.GetValueExt<INRegister.finPeriodID>(receipt.Current);
				parameters["DocType"] = receipt.Current.DocType;
				parameters["RefNbr"] = receipt.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "IN614000", Messages.INRegisterDetails);
			}
			return adapter.Get();
		}

		public PXAction<INRegister> iNItemLabels;
		[PXUIField(DisplayName = Messages.INItemLabels, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INItemLabels(PXAdapter adapter)
		{
			if (receipt.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = receipt.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "IN619200", Messages.INItemLabels);
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

		protected virtual void INSiteStatusFilter_OnlyAvailable_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = false;
		}

		protected virtual void INSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			INSiteStatusFilter row = (INSiteStatusFilter)e.Row;
			if (row != null && receipt.Current != null)
				row.SiteID = receipt.Current.SiteID;
		}
		#endregion
		public INReceiptEntry()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<INTran.tranType>(transactions.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INTran.tranType>(transactions.Cache, null, false);
		}

	    public override void InitCacheMapping(Dictionary<Type, Type> map)
	    {
	        base.InitCacheMapping(map);
			// override INLotSerialStatus cache to allow INTransitLineLotSerialStatus with projection work currectly with LSSelect<TLSMaster, TLSDetail, Where>
			this.Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
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
							new PXDataFieldValue<INTran.docType>(PXDbType.Char, ((INReceiptEntry)graph).receipt.Current?.DocType),
							new PXDataFieldValue<INTran.refNbr>(((INReceiptEntry)graph).receipt.Current?.RefNbr),
							new PXDataFieldValue<INTran.createdByScreenID>(PXDbType.Char, "IN301000")
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(INTranSplit), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<INTranSplit.docType>(PXDbType.Char, ((INReceiptEntry)graph).receipt.Current?.DocType),
							new PXDataFieldValue<INTranSplit.refNbr>(((INReceiptEntry)graph).receipt.Current?.RefNbr),
							new PXDataFieldValue<INTranSplit.createdByScreenID>(PXDbType.Char, "IN301000")
						};
					}));
			}
		}

		protected virtual void INRegister_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Receipt;
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

        public virtual void INTran_BaseQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INTranBaseQtyFieldVerifying(sender, (INTran)e.Row, e.NewValue);
        }

        public virtual void INTranBaseQtyFieldVerifying(PXCache sender, INTran row, object value)
        {
            bool istransfer = CurrentDocument.Current != null && CurrentDocument.Current.TransferNbr != null;
            if (istransfer)
            {
                if ((decimal?)value > row.MaxTransferBaseQty && row.MaxTransferBaseQty.HasValue)
                {
                    sender.RaiseExceptionHandling<INTran.qty>(row, value, new PXSetPropertyException<INTran.qty>(CS.Messages.Entry_LE, INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, row, row.MaxTransferBaseQty.Value, INPrecision.QUANTITY)));
                }
            }
        }

        public virtual void INTranSplitBaseQtyFieldVerifying(PXCache sender, INTran row, object value)
        {
            bool istransfer = CurrentDocument.Current != null && CurrentDocument.Current.TransferNbr != null;
            if (istransfer)
            {
                if ((decimal?)value > row.MaxTransferBaseQty && row.MaxTransferBaseQty.HasValue)
                {
                    sender.RaiseExceptionHandling<INTranSplit.qty>(row, value, new PXSetPropertyException<INTran.qty>(CS.Messages.Entry_LE, INUnitAttribute.ConvertFromBase<INTranSplit.inventoryID, INTranSplit.uOM>(sender, row, row.MaxTransferBaseQty.Value, INPrecision.QUANTITY)));
                }
            }
        }

        public virtual void INTranSplit_BaseQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INTranSplitBaseQtyFieldVerifying(sender, (INTranSplit)e.Row, e.NewValue);
        }

		protected virtual void INRegister_TransferNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INTran newtran = null;
			int? prev_linenbr = null;
            INLocationStatus2 prev_stat = null;
            decimal newtranqty = 0m;
            string transferNbr = ((INRegister)e.Row).TransferNbr;
            decimal newtrancost = 0m;
            ParseSubItemSegKeys();

            using (new PXReadBranchRestrictedScope())
            {
                foreach (PXResult<INTransitLine, INLocationStatus2, INTransitLineLotSerialStatus, INSite, InventoryItem, INTran> res in
                    PXSelectJoin<INTransitLine,
                    InnerJoin<INLocationStatus2, On<INLocationStatus2.locationID, Equal<INTransitLine.costSiteID>>,
                    LeftJoin<INTransitLineLotSerialStatus,
                            On<INTransitLine.transferNbr, Equal<INTransitLineLotSerialStatus.transferNbr>, 
                            And<INTransitLine.transferLineNbr, Equal<INTransitLineLotSerialStatus.transferLineNbr>>>,
                    InnerJoin<INSite, On<INTransitLine.FK.ToSite>,
                    InnerJoin<InventoryItem, On<INLocationStatus2.FK.InventoryItem>,
                    InnerJoin<INTran, 
                        On<INTran.docType, Equal<INDocType.transfer>, 
                        And<INTran.refNbr, Equal<INTransitLine.transferNbr>,
                        And<INTran.lineNbr, Equal<INTransitLine.transferLineNbr>,
                        And<INTran.invtMult, Equal<shortMinus1>>>>>>>>>>,
                    Where<INTransitLine.transferNbr, Equal<Required<INTransitLine.transferNbr>>>,
                    OrderBy<Asc< INTransitLine.transferNbr, Asc< INTransitLine.transferLineNbr>>>>
                    .Select(this, transferNbr))
                {
                    INTransitLine transitline = res;
                    INLocationStatus2 stat = res;
                    INTransitLineLotSerialStatus lotstat = res;
                    INSite site = res;
                    InventoryItem item = res;
                    INTran tran = res;

                    if (stat.QtyOnHand == 0m || (lotstat!=null && lotstat.QtyOnHand == 0m))
                        continue;

                    if (prev_linenbr != transitline.TransferLineNbr)
                    {
                        UpdateTranCostQty(newtran, newtranqty, newtrancost);
                        newtrancost = 0m;
                        newtranqty = 0m;

                        if (!object.Equals(receipt.Current.BranchID, site.BranchID))
                        {
                            INRegister copy = PXCache<INRegister>.CreateCopy(receipt.Current);
                            copy.BranchID = site.BranchID;
                            receipt.Update(copy);
                        }
                        newtran = PXCache<INTran>.CreateCopy(tran);
						if (tran.DestBranchID == null)
						{
							// AC-119895: If two-step Transfer was released before upgrade we should follow the old behavior
							// where Interbranch transactions are placed in receipt part.
							newtran.OrigBranchID = newtran.BranchID;
						}
						newtran.OrigDocType = newtran.DocType;
						newtran.OrigTranType = newtran.TranType;
                        newtran.OrigRefNbr = transitline.TransferNbr;
                        newtran.OrigLineNbr = transitline.TransferLineNbr;
						if (tran.TranType == INTranType.Transfer)
						{
							newtran.OrigNoteID = ((INRegister)e.Row).NoteID;
							newtran.OrigToLocationID = tran.ToLocationID;
							INTranSplit split =
								PXSelectReadonly<INTranSplit,
								Where<INTranSplit.docType, Equal<Current<INTran.docType>>,
									And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>,
									And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>>
								.SelectSingleBound(this, new object[] {tran});
							newtran.OrigIsLotSerial = !string.IsNullOrEmpty(split?.LotSerialNbr);
						}
                        newtran.BranchID = site.BranchID;
                        newtran.DocType = ((INRegister)e.Row).DocType;
                        newtran.RefNbr = ((INRegister)e.Row).RefNbr;
                        newtran.LineNbr = (int)PXLineNbrAttribute.NewLineNbr<INTran.lineNbr>(transactions.Cache, e.Row);
                        newtran.InvtMult = (short)1;
                        newtran.SiteID = transitline.ToSiteID;
                        newtran.LocationID = transitline.ToLocationID;
                        newtran.ToSiteID = null;
                        newtran.ToLocationID = null;
                        newtran.BaseQty = 0m;
                        newtran.Qty = 0m;
                        newtran.UnitCost = 0m;
                        newtran.Released = false;
                        newtran.InvtAcctID = null;
                        newtran.InvtSubID = null;
                        newtran.ReasonCode = null;
                        newtran.ARDocType = null;
                        newtran.ARRefNbr = null;
                        newtran.ARLineNbr = null;
	                    newtran.ProjectID = null;
	                    newtran.TaskID = null;
						newtran.CostCodeID = null;
						newtran.TranCost = 0m;
	                    newtran.NoteID = null;

                        splits.Current = null;

                        newtran = transactions.Insert(newtran);

                        transactions.Current = newtran;

                        if (splits.Current != null)
                        {
                            splits.Delete(splits.Current);
                        }

                        
                        prev_linenbr = transitline.TransferLineNbr;
                    }

                    if (this.Caches[typeof(INLocationStatus2)].ObjectsEqual(prev_stat, stat) == false)
                    {
                        newtranqty += stat.QtyOnHand.Value;
                        prev_stat = stat;
                    }

                    decimal newsplitqty;
                    INTranSplit newsplit;
                    if(lotstat.QtyOnHand == null)
                    {
                        newsplit = new INTranSplit();
                        newsplit.InventoryID = stat.InventoryID;
                        newsplit.IsStockItem = true;
                        newsplit.FromSiteID = transitline.SiteID;
                        newsplit.SubItemID = stat.SubItemID;
                        newsplit.LotSerialNbr = null;
                        newsplitqty = stat.QtyOnHand.Value;
                    }
                    else
                    {
                        newsplit = new INTranSplit();
                        newsplit.InventoryID = lotstat.InventoryID;
                        newsplit.IsStockItem = true;
                        newsplit.FromSiteID = lotstat.FromSiteID;
                        newsplit.SubItemID = lotstat.SubItemID;
                        newsplit.LotSerialNbr = lotstat.LotSerialNbr;
                        newsplitqty = lotstat.QtyOnHand.Value;   
                    }

                    newsplit.DocType = ((INRegister)e.Row).DocType;
                    newsplit.RefNbr = ((INRegister)e.Row).RefNbr;
                    newsplit.LineNbr = newtran.LineNbr;
                    newsplit.SplitLineNbr = (int)PXLineNbrAttribute.NewLineNbr<INTranSplit.splitLineNbr>(splits.Cache, e.Row);

                    newsplit.UnitCost = 0m;
                    newsplit.InvtMult = (short)1;
                    newsplit.SiteID = transitline.ToSiteID;
                    newsplit.LocationID = lotstat.ToLocationID ?? transitline.ToLocationID;
                    newsplit.PlanID = null;
                    newsplit.Released = false;
	                newsplit.ProjectID = null;
	                newsplit.TaskID = null;

                    newsplit = splits.Insert(newsplit);

                    UpdateCostSubItemID(newsplit, item);
                    newsplit.MaxTransferBaseQty = newsplitqty;
                    newsplit.BaseQty = newsplitqty;
                    newsplit.Qty = newsplit.BaseQty.Value;

                    SetCostAttributes(newtran, newsplit, item, transferNbr);
                    newtrancost += newsplit.BaseQty.Value * newsplit.UnitCost.Value;
                    newsplit.UnitCost = PXCurrencyAttribute.BaseRound(this, newsplit.UnitCost);
                    splits.Update(newsplit);
                }
            }
            UpdateTranCostQty(newtran, newtranqty, newtrancost);
        }

        public virtual void UpdateTranCostQty(INTran newtran, decimal newtranqty, decimal newtrancost)
                    {
            lsselect.SuppressedMode = true;
            if (newtran != null)
            {
                newtran.BaseQty = newtranqty;
                newtran.Qty = INUnitAttribute.ConvertFromBase(transactions.Cache, newtran.InventoryID, newtran.UOM, newtran.BaseQty.Value, INPrecision.QUANTITY);
                newtran.MaxTransferBaseQty = newtranqty;
                newtran.UnitCost = PXCurrencyAttribute.BaseRound(this, newtrancost / newtran.Qty);
                newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtrancost);
                transactions.Update(newtran);    
                    }
            lsselect.SuppressedMode = false;
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
						sb.Append(segval.PadRight(seg.Length ?? 0));
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

        public virtual void UpdateCostSubItemID(INTranSplit split, InventoryItem item)
        {
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
        }

        public Int32? INTransitSiteID
        {
            get
            {
                if (insetup.Current.TransitSiteID == null)
                    throw new PXException("Please fill transite site id in inventory preferences.");
                return insetup.Current.TransitSiteID;
            }
        }

        public virtual PXView GetCostStatusCommand(INTranSplit split, InventoryItem item, string transferNbr, out object[] parameters)
        {
            BqlCommand cmd = null;

            int? costsiteid;
            costsiteid = INTransitSiteID;
            
            switch (item.ValMethod)
            {
                case INValMethod.Standard:
                case INValMethod.Average:
                case INValMethod.FIFO:

                    cmd = new Select<INCostStatus, 
                        Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>, 
                            And<INCostStatus.costSiteID, Equal<Required<INCostStatus.costSiteID>>, 
                            And<INCostStatus.costSubItemID, Equal<Required<INCostStatus.costSubItemID>>,
                            And<INCostStatus.layerType, Equal<INLayerType.normal>,
                            And<INCostStatus.receiptNbr, Equal<Required<INCostStatus.receiptNbr>>>>>>>, 
                        OrderBy<Asc<INCostStatus.receiptDate, Asc<INCostStatus.receiptNbr>>>>();
                    
                    parameters = new object[] { split.InventoryID, costsiteid, split.CostSubItemID, transferNbr };
                    break;
                case INValMethod.Specific:
                    cmd = new Select<INCostStatus, 
                        Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>, 
                        And<INCostStatus.costSiteID, Equal<Required<INCostStatus.costSiteID>>, 
                        And<INCostStatus.costSubItemID, Equal<Required<INCostStatus.costSubItemID>>, 
                        And<INCostStatus.lotSerialNbr, Equal<Required<INCostStatus.lotSerialNbr>>, 
                        And<INCostStatus.layerType, Equal<INLayerType.normal>,
                        And<INCostStatus.receiptNbr, Equal<Required<INCostStatus.receiptNbr>>>>>>>>>();
                    parameters = new object[] { split.InventoryID, costsiteid, split.CostSubItemID, split.LotSerialNbr, transferNbr };
                    break;
                default:
                    throw new PXException();
            }

            return new PXView(this, false, cmd);
        }

        public virtual void SetCostAttributes(INTran tran, INTranSplit split, InventoryItem item, string transferNbr)
        {
            if (split.BaseQty == 0m || split.BaseQty == null)
                return;

            object[] parameters;
            PXView cmd = GetCostStatusCommand(split, item, transferNbr, out parameters);
            INCostStatus layer = (INCostStatus)cmd.SelectSingle(parameters);
            tran.AcctID = layer.AccountID;
            tran.SubID = layer.SubID;
            split.UnitCost = layer.TotalCost.Value / layer.QtyOnHand.Value;
        }

        INRegister copy;

		protected virtual void INRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

            INTran tran = transactions.Current ?? transactions.SelectWindowed(0, 1);
            if (tran != null)
            {
                ((INRegister)e.Row).TransferNbr = tran.OrigRefNbr;
            }

            bool isTransfer = ((INRegister)e.Row).TransferNbr != null;
            bool isPOTransfer = isTransfer && ((INRegister)e.Row).OrigModule == GL.BatchModule.PO;

			release.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNEdit.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNRegisterDetails.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == true);
			iNItemLabels.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == true);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, ((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
			PXUIFieldAttribute.SetEnabled<INRegister.refNbr>(sender, e.Row, true);
			PXUIFieldAttribute.SetEnabled<INRegister.totalQty>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.totalCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.status>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = (((INRegister)e.Row).Released == false);
			sender.AllowDelete = (((INRegister)e.Row).Released == false && (((INRegister)e.Row).OrigModule == GL.BatchModule.IN));

			lsselect.AllowInsert = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN) && !isTransfer;
			lsselect.AllowUpdate = (((INRegister)e.Row).Released == false);
			lsselect.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN) && !isTransfer;

			addInvBySite.SetEnabled(lsselect.AllowInsert);

			PXUIFieldAttribute.SetVisible<INRegister.controlQty>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INRegister.controlCost>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);

            PXUIFieldAttribute.SetEnabled<INRegister.transferNbr>(sender, e.Row, sender.AllowUpdate && tran == null);
            PXUIFieldAttribute.SetEnabled<INRegister.branchID>(sender, e.Row, sender.AllowUpdate && !isTransfer);

            if (sender.Graph.IsImport == true && copy != null && sender.ObjectsEqual<INRegister.transferNbr, INRegister.released>(e.Row, copy))
            {
                return;
            }

			if (isTransfer && !isPOTransfer)
			{
				PXUIFieldAttribute.SetEnabled<INTran.qty>(transactions.Cache, null, true);
			}

			PXUIFieldAttribute.SetVisible<INTran.projectID>(transactions.Cache, null, IsPMVisible);
			PXUIFieldAttribute.SetVisible<INTran.taskID>(transactions.Cache, null, IsPMVisible);

            copy = PXCache<INRegister>.CreateCopy((INRegister)e.Row);
		}

        public class TransferNbrSelectorAttribute : PXSelectorAttribute
        {
            protected BqlCommand _RestrictedSelect;

            public TransferNbrSelectorAttribute(Type searchType)
                : base(searchType)
            { 
                _RestrictedSelect = BqlCommand.CreateInstance(typeof(Search2<INRegister.refNbr, InnerJoin<INSite, On<INRegister.FK.ToSite>>, Where<MatchWithBranch<INSite.branchID>>>));
            }

            public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
            {
                using (new PXReadBranchRestrictedScope())
                {
                    base.FieldVerifying(sender, e);
                }
            }

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				PXView outerview = new PXView(sender.Graph, true, _Select);
				PXView view = sender.Graph.Views[_ViewName] = new PXView(sender.Graph, true, _Select, (PXSelectDelegate)delegate()
				{
					int startRow = PXView.StartRow;
					int totalRows = 0;
					List<object> res;

					using (new PXReadBranchRestrictedScope())
					{
						res = outerview.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
						PXView.StartRow = 0;
					}

					PXCache cache = outerview.Graph.Caches[typeof(INSite)];

					return res.FindAll((item) =>
					{
						return _RestrictedSelect.Meet(cache, item is PXResult ? PXResult.Unwrap<INSite>(item) : item);
					});
				});

				if (_DirtyRead)
				{
					view.IsReadOnly = false;
				}

			}
		}


        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Transfer Nbr.")]
        [TransferNbrSelector(typeof(Search2<INRegister.refNbr, 
            InnerJoin<INSite, On<INRegister.FK.ToSite>, 
            InnerJoin<INTransferInTransit, On<INTransferInTransit.transferNbr, Equal<INRegister.refNbr>>,
            LeftJoin<INTran, On<INTran.origRefNbr, Equal<INTransferInTransit.transferNbr>,
                And<INTran.released, NotEqual<True>>>>>>, 
        Where<INRegister.docType, Equal<INDocType.transfer>, 
            And<INRegister.released, Equal<boolTrue>,
            And<INTran.refNbr, IsNull,   
            And<Match<INSite, Current<AccessInfo.userName>>>>>>>))]
		[PXRestrictor(typeof(Where<INRegister.origModule, Equal<GL.BatchModule.moduleIN>>), Messages.TransferShouldBeProcessedThroughPO, typeof(INRegister.refNbr))]
		protected virtual void INRegister_TransferNbr_CacheAttached(PXCache sender)
        { 
        }

		protected virtual void INRegister_TransferNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            INTran tran = transactions.SelectWindowed(0, 1);
			if (tran != null)
            {
				e.Cancel = true;
			}
		}

		protected virtual void INTran_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Receipt;
		}

        [PXDBString(3, IsFixed = true)]
        [PXDefault(INTranType.Receipt)]
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

		protected virtual void INTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row != null && (row.OrigModule == BatchModule.SO || row.OrigModule == BatchModule.PO))
			{
				INRegister doc = (INRegister)PXParentAttribute.SelectParent(sender, e.Row, typeof(INRegister));
				if (doc != null)
				{
					PXCache cache = receipt.Cache;
					object copy = cache.CreateCopy(doc);

					doc.SOShipmentType = row.SOShipmentType;
					doc.SOShipmentNbr = row.SOShipmentNbr;
					doc.SOOrderType = row.SOOrderType;
					doc.SOOrderNbr = row.SOOrderNbr;
					doc.POReceiptType = row.POReceiptType;
					doc.POReceiptNbr = row.POReceiptNbr;

					if (!object.Equals(doc, cache.Current))
					{
						cache.Update(doc);
					}
					else
					{
						if (cache.GetStatus(doc) == PXEntryStatus.Notchanged || cache.GetStatus(doc) == PXEntryStatus.Held)
						{
							cache.SetStatus(doc, PXEntryStatus.Updated);
						}
						cache.RaiseRowUpdated(doc, copy);
					}
				}
			}
		}

		public virtual void INTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            INTranSplit row = (INTranSplit)e.Row;

            if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                INTranSplitBaseQtyFieldVerifying(sender, row, row.BaseQty);   
            }
        }

		protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INTran row = (INTran)e.Row;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (!string.IsNullOrEmpty(row.POReceiptNbr))
				{
					if (PXDBQuantityAttribute.Round((decimal)(row.Qty + row.OrigQty)) > 0m)
					{
						sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_LE, -row.OrigQty));
					}
					else if (PXDBQuantityAttribute.Round((decimal)(row.Qty + row.OrigQty)) < 0m)
					{
						sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_GE, -row.OrigQty));
					}
				}
			}

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
                INTranBaseQtyFieldVerifying(sender, row, row.BaseQty);

				if (row.Qty == 0 && row.TranCost > 0)
                { 
                    if(row.POReceiptNbr!=null && row.POReceiptLineNbr != null && row.POReceiptType!=null)
                    {
                        PO.POReceiptLine poreceiptline = new PO.POReceiptLine();
                        poreceiptline.ReceiptType = row.POReceiptType;
                        poreceiptline.LineNbr = row.POReceiptLineNbr;
                        poreceiptline.ReceiptNbr = row.POReceiptNbr;
                        throw
                            new PXErrorContextProcessingException(this, poreceiptline, new PXSetPropertyException(Messages.ZeroQtyWhenNonZeroCost));
                    }                
                    sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty,
                            new PXSetPropertyException(Messages.ZeroQtyWhenNonZeroCost));
                }
                CheckForSingleLocation(sender, row);
				CheckSplitsForSameTask(sender, row);
				CheckLocationTaskRule(sender, row);
			}
		}

		protected virtual void INTran_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			if (PM.ProjectAttribute.IsPMVisible( BatchModule.IN))
			{
				if (row.LocationID != null)
				{
					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMProject, 
							On<PMProject.contractID, Equal<INLocation.projectID>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(sender.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMProject> res in result)
					{
						PMProject project = (PMProject)res;
						if (project != null && project.ContractCD != null && project.VisibleInIN == true)
						{
							e.NewValue = project.ContractCD;
							return;
						}
					}
				}
			}
		}

		protected virtual void INTran_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;
			
			if (PM.ProjectAttribute.IsPMVisible( BatchModule.IN))
			{
				if (row.LocationID != null )
				{
					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMTask, 
							On<PMTask.projectID, Equal<INLocation.projectID>, 
							And<PMTask.taskID, Equal<INLocation.taskID>>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(sender.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMTask> res in result)
					{
						PMTask task = (PMTask)res;
						if (task != null && task.TaskCD != null && task.VisibleInIN == true && task.IsActive == true)
						{
							e.NewValue = task.TaskCD;
							return;
						}
					}

				}
			}
		}

		protected virtual void INTran_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;
			
			sender.SetDefaultExt<INTran.projectID>(e.Row); //will set pending value for TaskID to null if project is changed. This is the desired behavior for all other screens.
			if (sender.GetValuePending<INTran.taskID>(e.Row) == null) //To redefault the TaskID in currecnt screen - set the Pending value from NULL to NOTSET
				sender.SetValuePending<INTran.taskID>(e.Row, PXCache.NotSetValue);
			sender.SetDefaultExt<INTran.taskID>(e.Row);
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INTranSplitPlanID(typeof(INRegister.noteID), typeof(INRegister.hold), typeof(INRegister.transferType))]
		protected virtual void INTranSplit_PlanID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitCost;
			sender.RaiseFieldDefaulting<INTran.unitCost>(e.Row, out UnitCost);

			if (UnitCost != null && (decimal)UnitCost != 0m)
			{
				decimal? unitcost = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitCost, INPrecision.UNITCOST);
				sender.SetValueExt<INTran.unitCost>(e.Row, unitcost);
			}
		}

		protected virtual bool IsPMVisible
		{
			get
			{
				PM.PMSetup setup = PXSelect<PM.PMSetup>.Select(this);
				if (setup == null)
				{
					return false;
				}
				else
				{
					if (setup.IsActive != true)
						return false;
					else
						return setup.VisibleInIN == true;
				}
			}
		}

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "transactions", StringComparison.OrdinalIgnoreCase) == 0)
			{
				CorrectKey("DocType", CurrentDocument.Current.DocType, keys, values);
				CorrectKey("RefNbr", CurrentDocument.Current.RefNbr, keys, values);
				INTran tran = PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>, OrderBy<Desc<INTran.lineNbr>>>.SelectSingleBound(this, new object[] { CurrentDocument.Current });
				CorrectKey("LineNbr", tran == null? 1 : tran.LineNbr + 1, keys, values);
			}
			return true;
		}

		protected static void CorrectKey(string name, object value, IDictionary keys, IDictionary values)
		{
			CorrectKey(name, value, keys);
			CorrectKey(name, value, values);
		}

		protected static void CorrectKey(string name, object value, IDictionary dict)
		{
			if (dict.Contains(name))
				dict[name] = value;
			else
				dict.Add(name, value);
		}

		public virtual bool RowImporting(string viewName, object row) { return row == null; }
		public virtual bool RowImported(string viewName, object row, object oldRow) { return oldRow == null; }
		public virtual void PrepareItems(string viewName, IEnumerable items){}

		protected virtual void CheckLocationTaskRule(PXCache sender, INTran row)
		{
			if (row.TaskID != null)
			{
				INLocation selectedLocation = INLocation.PK.Find(this, row.LocationID);

				if (selectedLocation != null && selectedLocation.TaskID != row.TaskID)
				{
					sender.RaiseExceptionHandling<INTran.locationID>(row, selectedLocation.LocationCD,
						new PXSetPropertyException(Messages.LocationIsMappedToAnotherTask, PXErrorLevel.Warning ));

				}
			}
		}

		protected virtual void CheckForSingleLocation(PXCache sender, INTran row)
		{
			InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
			if (item != null && item.StkItem == true && row.TaskID != null && row.LocationID == null)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(Messages.RequireSingleLocation));
			}
		}

		protected virtual void CheckSplitsForSameTask(PXCache sender, INTran row)
		{
			if (row.HasMixedProjectTasks == true)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(Messages.MixedProjectsInSplits));
			}
		}

		#region INRegisterBaseEntry implementation
		public override PXSelectBase<INRegister> INRegisterDataMember => receipt;
		public override PXSelectBase<INTran> INTranDataMember => transactions;
		public override LSINTran LSSelectDataMember => lsselect;
		public override PXSelectBase<INTranSplit> INTranSplitDataMember => splits;

		#endregion
	}
}
