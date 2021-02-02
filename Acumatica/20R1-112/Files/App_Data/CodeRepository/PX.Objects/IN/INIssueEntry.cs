using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.IN
{
	public class INIssueEntry : INRegisterEntryBase, IGraphWithInitialization
	{
		public PXSelect<INRegister, Where<INRegister.docType, Equal<INDocType.issue>>> issue;
		public PXSelect<INRegister, Where<INRegister.docType, Equal<Current<INRegister.docType>>, And<INRegister.refNbr, Equal<Current<INRegister.refNbr>>>>> CurrentDocument;
        [PXImport(typeof(INRegister))]
		public PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>> transactions;

		[PXCopyPasteHiddenView()]
		public PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Current<INTran.docType>>, And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>, And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>> splits;

		public LSINTran lsselect;

		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>))]
		[INUnit(typeof(INTran.inventoryID))]
		public virtual void INTran_UOM_CacheAttached(PXCache sender)
		{
		}
        [PXString(2)]
        [PXFormula(typeof(Parent<INRegister.origModule>))]
        public virtual void INTran_OrigModule_CacheAttached(PXCache sender)
        {
        }
        [PXString(2)]
        [PXFormula(typeof(Parent<INRegister.origModule>))]
        public virtual void INTranSplit_OrigModule_CacheAttached(PXCache sender)
        {
        }
        [IN.LocationAvail(typeof(INTran.inventoryID),
            typeof(INTran.subItemID),
            typeof(INTran.siteID),
            typeof(Where<INTran.tranType, 
                            Equal<INTranType.invoice>, 
                        Or<INTran.tranType, 
                            Equal<INTranType.debitMemo>, 
                        Or<INTran.origModule, 
                            NotEqual<GL.BatchModule.modulePO>, 
                        And<INTran.tranType, 
                            Equal<INTranType.issue>>>>>),
            typeof(Where<INTran.tranType, 
                            Equal<INTranType.receipt>, 
                         Or<INTran.tranType, 
                            Equal<INTranType.return_>, 
                         Or<INTran.tranType, 
                            Equal<INTranType.creditMemo>, 
                         Or<INTran.origModule, 
                            Equal<GL.BatchModule.modulePO>, 
                         And<INTran.tranType, 
                            Equal<INTranType.issue>>>>>>),
            typeof(Where<INTran.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTran.invtMult, 
                            Equal<short1>, 
                         Or<INTran.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTran.invtMult, 
                            Equal<shortMinus1>>>>>))]
        public virtual void INTran_LocationID_CacheAttached(PXCache sender)
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

        [IN.LocationAvail(typeof(INTranSplit.inventoryID), 
            typeof(INTranSplit.subItemID),
            typeof(INTranSplit.siteID),
            typeof(Where<INTranSplit.tranType, 
                            Equal<INTranType.invoice>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.debitMemo>, 
                         Or<INTranSplit.origModule, 
                            NotEqual<GL.BatchModule.modulePO>, 
                         And<INTranSplit.tranType, 
                            Equal<INTranType.issue>>>>>),
            typeof(Where<INTranSplit.tranType, 
                            Equal<INTranType.receipt>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.return_>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.creditMemo>, 
                         Or<INTranSplit.origModule, 
                            Equal<GL.BatchModule.modulePO>, 
                         And<INTranSplit.tranType, 
                            Equal<INTranType.issue>>>>>>),
            typeof(Where<INTranSplit.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTranSplit.invtMult, 
                            Equal<short1>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTranSplit.invtMult,
                            Equal<shortMinus1>>>>>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
			if (issue.Current != null && !String.IsNullOrEmpty(issue.Current.BatchNbr))
			{
				GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(issue.Current.BatchNbr, "IN");
				throw new PXRedirectRequiredException(graph, "Current batch record");
			}
			return adapter.Get();
		}
		
		public PXAction<INRegister> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache =issue.Cache;
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
			PXLongOperation.StartOperation(this, delegate() { INDocumentRelease.ReleaseDoc(list, false, adapter.QuickProcessFlow); });
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
			if (issue.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["DocType"] = issue.Current.DocType;
				parameters["RefNbr"] = issue.Current.RefNbr;
				parameters["PeriodTo"] = null;
				parameters["PeriodFrom"] = null;
				throw new PXReportRequiredException(parameters, "IN611000", Messages.INEditDetails);
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

		public PXAction<INRegister> iNRegisterDetails;
		[PXUIField(DisplayName = Messages.INRegisterDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INRegisterDetails(PXAdapter adapter)
		{
			if (issue.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["PeriodID"] = (string)issue.GetValueExt<INRegister.finPeriodID>(issue.Current);
				parameters["DocType"] = issue.Current.DocType;
				parameters["RefNbr"] = issue.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "IN614000", Messages.INRegisterDetails);
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
			if (row != null && issue.Current != null)
				row.SiteID = issue.Current.SiteID;
		}
		#endregion

		public INIssueEntry()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup record = insetup.Current;
			}

			PXStringListAttribute.SetList<INTran.tranType>(transactions.Cache, null, new INTranType.IssueListAttribute().AllowedValues, new INTranType.IssueListAttribute().AllowedLabels);
			//PXDimensionSelectorAttribute.SetValidCombo<INTran.subItemID>(transactions.Cache, true);
			//PXDimensionSelectorAttribute.SetValidCombo<INTranSplit.subItemID>(splits.Cache, true);

            this.FieldDefaulting.AddHandler<IN.Overrides.INDocumentRelease.SiteStatus.negAvailQty>((sender, e) =>
            {
				if (!e.Cancel)
	                e.NewValue = true;
                e.Cancel = true;
            });
		}

	    public override void InitCacheMapping(Dictionary<Type, Type> map)
	    {
	        base.InitCacheMapping(map);
			// override INLotSerialStatus cache to allow INTransitLineLotSerialStatus with projection work currectly with LSSelect<TLSMaster, TLSDetail, Where>
			Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
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
							new PXDataFieldValue<INTran.docType>(PXDbType.Char, ((INIssueEntry)graph).issue.Current?.DocType),
							new PXDataFieldValue<INTran.refNbr>(((INIssueEntry)graph).issue.Current?.RefNbr),
							new PXDataFieldValue<INTran.createdByScreenID>(PXDbType.Char, "IN302000")
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(INTranSplit), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<INTranSplit.docType>(PXDbType.Char, ((INIssueEntry)graph).issue.Current?.DocType),
							new PXDataFieldValue<INTranSplit.refNbr>(((INIssueEntry)graph).issue.Current?.RefNbr),
							new PXDataFieldValue<INTranSplit.createdByScreenID>(PXDbType.Char, "IN302000")
						};
					}));
			}
		}

		protected virtual void INRegister_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Issue;
		}

		protected virtual void INRegister_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (((INRegister)e.Row).DocType == INDocType.Undefined)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (insetup.Current.RequireControlTotal == false)
			{
				if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalAmount) == false)
				{
					sender.SetValue<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).TotalAmount);
				}
				else
				{
					sender.SetValue<INRegister.controlAmount>(e.Row, 0m);
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
					if (((INRegister)e.Row).TotalAmount != ((INRegister)e.Row).ControlAmount)
					{
						sender.RaiseExceptionHandling<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).ControlAmount, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).ControlAmount, null);
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

			release.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNEdit.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNRegisterDetails.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == true);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, ((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
			PXUIFieldAttribute.SetEnabled<INRegister.refNbr>(sender, e.Row, true);
			PXUIFieldAttribute.SetEnabled<INRegister.totalQty>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.totalAmount>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<INRegister.totalCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.status>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = (((INRegister)e.Row).Released == false);
			sender.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

			lsselect.AllowInsert = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
			lsselect.AllowUpdate = (((INRegister)e.Row).Released == false);
			lsselect.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

			addInvBySite.SetEnabled(lsselect.AllowInsert);

			PXUIFieldAttribute.SetVisible<INRegister.controlQty>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INRegister.controlAmount>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INTran.projectID>(transactions.Cache, null, IsPMVisible);
			PXUIFieldAttribute.SetVisible<INTran.taskID>(transactions.Cache, null, IsPMVisible);
            PXUIFieldAttribute.SetVisible<INRegister.totalCost>(sender, e.Row, ((INRegister)e.Row).Released == true);

			/// added because IN Transfer is created via INIssueEntry in
			/// <see cref="SO.SOShipmentEntry.PostShipment(INIssueEntry, PXResult{SO.SOOrderShipment, SO.SOOrder}, DocumentList{INRegister}, AR.ARInvoice)"/>
			// TODO: move it to the Ctor or CacheAttached in 2019R1 after AC-118791
			switch (((INRegister)e.Row).DocType)
			{
				case INDocType.Issue:
					PXFormulaAttribute.SetAggregate<INTran.tranAmt>(transactions.Cache, typeof(SumCalc<INRegister.totalAmount>));
					break;
				default:
					PXFormulaAttribute.SetAggregate<INTran.tranAmt>(transactions.Cache, null);
					break;
			}
        }

		protected virtual void INTran_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Issue;
		}

		protected virtual void INTran_TranType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INTranType.Issue;
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
			DefaultUnitPrice(sender, e);
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitPrice(sender, e);
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SOOrderNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void INTran_SOShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void INTran_ReasonCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row != null)
			{
				ReasonCode reasoncd = ReasonCode.PK.Find(this, (string)e.NewValue);

				if (reasoncd != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject( row.ProjectID))
				{
					PX.Objects.GL.Account account = PXSelect<PX.Objects.GL.Account, Where<PX.Objects.GL.Account.accountID, Equal<Required<PX.Objects.GL.Account.accountID>>>>.Select(this, reasoncd.AccountID);
					if (account != null && account.AccountGroupID == null)
					{
						sender.RaiseExceptionHandling<INTran.reasonCode>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
					}
				}
				
				e.Cancel = (reasoncd != null) &&
					(row.TranType != INTranType.Issue && row.TranType != INTranType.Return && reasoncd.Usage == ReasonCodeUsages.Sales || reasoncd.Usage == row.DocType);

				
			}
		}

		protected virtual void INTran_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTranSplit_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTran_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTranSplit_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

        protected virtual void INTranSplit_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            //for cluster only. SelectQueries sometimes does not contain all the needed records after failed Save operation
            if (e.TranStatus == PXTranStatus.Aborted && PX.Common.WebConfig.IsClusterEnabled)
            {
                sender.ClearQueryCacheObsolete();
            }
        }
        
		protected virtual void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<INTran.unitCost>(sender, e.Row, ((INTran)e.Row).InvtMult == 1);
				PXUIFieldAttribute.SetEnabled<INTran.tranCost>(sender, e.Row, ((INTran)e.Row).InvtMult == 1);
            }
		}

		protected virtual void INTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row != null && (row.OrigModule == BatchModule.SO || row.OrigModule == BatchModule.PO))
			{
				INRegister doc = (INRegister)PXParentAttribute.SelectParent(sender, e.Row, typeof(INRegister));
				if (doc != null) 
				{
					PXCache cache = issue.Cache;
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

		protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INTran row = (INTran)e.Row;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (!string.IsNullOrEmpty(row.SOShipmentNbr))
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
						LeftJoin<PMProject, On<PMProject.contractID, Equal<INLocation.projectID>>>,
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
				if (row.LocationID != null)
				{
					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMTask, On<PMTask.projectID, Equal<INLocation.projectID>, And<PMTask.taskID, Equal<INLocation.taskID>>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(sender.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMTask> res in result)
					{
						PMTask task = (PMTask)res;
						if (task != null && task.TaskCD != null && task.VisibleInIN == true)
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

		protected virtual void DefaultUnitPrice(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitPrice;
			sender.RaiseFieldDefaulting<INTran.unitPrice>(e.Row, out UnitPrice);

			if (UnitPrice != null && (decimal)UnitPrice != 0m)
			{
				decimal? unitprice = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitPrice, INPrecision.UNITCOST);
				sender.SetValueExt<INTran.unitPrice>(e.Row, unitprice);
			}
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

		protected virtual void CheckLocationTaskRule(PXCache sender, INTran row)
		{
			if (row.TaskID != null)
			{
				INLocation selectedLocation = INLocation.PK.Find(sender.Graph, row.LocationID);

				if (selectedLocation != null && selectedLocation.TaskID != row.TaskID && (selectedLocation.TaskID != null || selectedLocation.ProjectID != null))
				{
					sender.RaiseExceptionHandling<INTran.locationID>(row, selectedLocation.LocationCD,
						new PXSetPropertyException(IN.Messages.LocationIsMappedToAnotherTask, PXErrorLevel.Warning));

				}
			}

		}

		protected virtual void CheckForSingleLocation(PXCache sender, INTran row)
		{
			if (row.TaskID != null && row.LocationID == null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				if (item != null && item.StkItem == true && row.LocationID == null)
				{
					sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(Messages.RequireSingleLocation));
				}
			}
		}

		protected virtual void CheckSplitsForSameTask(PXCache sender, INTran row)
		{
			if (row.HasMixedProjectTasks == true)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(Messages.MixedProjectsInSplits));
			}

		}

		#region INRegisterEntryBase members
		public override PXSelectBase<INRegister> INRegisterDataMember => issue;
		public override PXSelectBase<INTran> INTranDataMember => transactions;
		public override LSINTran LSSelectDataMember => lsselect;
		public override PXSelectBase<INTranSplit> INTranSplitDataMember => splits;

		#endregion
	}
}
