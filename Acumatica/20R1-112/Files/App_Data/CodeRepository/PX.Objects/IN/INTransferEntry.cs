using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Descriptor;

namespace PX.Objects.IN
{
    [Serializable]
	public class INTransferEntry : INRegisterEntryBase, IGraphWithInitialization
	{
		public PXSelect<INRegister, Where<INRegister.docType, Equal<INDocType.transfer>>> transfer;
		public PXSelect<INRegister, Where<INRegister.docType, Equal<Current<INRegister.docType>>, And<INRegister.refNbr, Equal<Current<INRegister.refNbr>>>>> CurrentDocument;
        [PXImport(typeof(INRegister))]
        [PXCopyPasteHiddenFields(typeof(INTran.iNTransitQty), typeof(INTran.receiptedQty), typeof(INTran.iNTransitBaseQty), typeof(INTran.receiptedBaseQty))]
        public PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>, And<INTran.invtMult, Equal<CS.shortMinus1>>>>> transactions;

		[PXCopyPasteHiddenView()]
		public PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Current<INTran.docType>>, And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>, And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>> splits;

		public PXSelect<INItemSite,
			Where<INItemSite.siteID, Equal<Required<INItemSite.siteID>>,
				And<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>>>> itemsite;

		public LSINTran lsselect;

	    #region INOpenPeriodTransfer

	    public class INOpenPeriodTransferAttribute : INOpenPeriodAttribute
	    {
			#region Types

		    protected class InTransferCalendarOrganizationIDProvider : CalendarOrganizationIDProvider
		    {
			    protected SourceSpecificationItem SiteSpecification { get; set; }

			    protected SourceSpecificationItem ToSiteSpecification { get; set; }

			    public InTransferCalendarOrganizationIDProvider()
			    {
				    SiteSpecification = new SourceSpecificationItem()
				    {
					    BranchSourceType = typeof(INRegister.siteID),
					    BranchSourceFormulaType = typeof(Selector<INRegister.siteID, INSite.branchID>),
					    IsMain = true,
				    }.Initialize();

				    ToSiteSpecification = new SourceSpecificationItem()
				    {
					    BranchSourceType = typeof(INRegister.toSiteID),
					    BranchSourceFormulaType = typeof(Selector<INRegister.toSiteID, INSite.branchID>),
				    }.Initialize();
			    }

			    public override SourcesSpecificationCollection GetSourcesSpecification(PXCache cache, object row)
			    {
				    var sourceSpecifications = new SourcesSpecificationCollection();
				    
					sourceSpecifications.SpecificationItems.Add(SiteSpecification);

				    sourceSpecifications.MainSpecificationItem = SiteSpecification;

					var register = (INRegister)row;

				    if (register == null
						|| register.TransferType == INTransferType.OneStep)
				    {
					    sourceSpecifications.SpecificationItems.Add(ToSiteSpecification);
				    }

				    sourceSpecifications.DependsOnFields.Add(typeof(INRegister.transferType));

					return sourceSpecifications;
			    }
		    }

			#endregion

			public INOpenPeriodTransferAttribute()
				: this(typeof(AccessInfo.branchID), null, typeof(INRegister.tranPeriodID))
			{
			}

			[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
			public INOpenPeriodTransferAttribute(Type branchSourceType = null, Type branchSourceFormulaType = null, Type masterFinPeriodIDType = null)
				: base(
					  sourceType: typeof(INRegister.tranDate),
					  branchSourceType: branchSourceType,
					  branchSourceFormulaType: branchSourceFormulaType,
					  masterFinPeriodIDType: masterFinPeriodIDType,
					  selectionModeWithRestrictions: SelectionModesWithRestrictions.All)
	        {
				if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() && PXAccess.FeatureInstalled<FeaturesSet.branch>())
				{
	            PeriodKeyProvider =
	            CalendarOrganizationIDProvider = new InTransferCalendarOrganizationIDProvider();
	        }
	    }
	    }

	    #endregion

		[GL.Branch(typeof(Search<INSite.branchID, Where<INSite.siteID, Equal<Current<INRegister.siteID>>>>), IsDetail = false, Enabled = false)]
		public virtual void INRegister_BranchID_CacheAttached(PXCache sender)
		{ 
		}

		[Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr))]
		[PXDefault()]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
		public virtual void INRegister_SiteID_CacheAttached(PXCache sender)
		{ 
		}

		[IN.ToSite(typeof(INRegister.transferType), DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr))]
		[PXDefault()]
		public virtual void INRegister_ToSiteID_CacheAttached(PXCache sender)
		{ 
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[INOpenPeriodTransfer(IsHeader = true)]
		public virtual void INRegister_FinPeriodID_CacheAttached(PXCache sender)
		{

		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void INRegister_POReceiptNbr_CacheAttached(PXCache sender) { }

		[GL.Branch(typeof(INRegister.branchID), Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		public virtual void INTran_BranchID_CacheAttached(PXCache sender)
		{
		}
        
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Line Number", Enabled = false, Visible = false)]
        public virtual void INTran_LineNbr_CacheAttached(PXCache sender)
        {
        }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<INTran.toSiteID, INSite.branchID>))]
		public virtual void INTran_DestBranchID_CacheAttached(PXCache sender)
		{
		}

		[IN.ToSite()]
		[PXDefault(typeof(INRegister.toSiteID))]
		public virtual void INTran_ToSiteID_CacheAttached(PXCache sender)
		{ 
		}

		[IN.LocationAvail(typeof(INTran.inventoryID), typeof(INTran.subItemID), typeof(INTran.toSiteID), false, false, true, DisplayName = "To Location ID", Visibility = PXUIVisibility.Service, Visible = false)]
		[PXDefault()]
		public virtual void INTran_ToLocationID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>))]
		[INUnit(typeof(INTran.inventoryID))]
		public virtual void INTran_UOM_CacheAttached(PXCache sender)
		{
		}

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBScalar(typeof(Search<INTransitLineStatus.qtyOnHand,
            Where<INTransitLineStatus.transferLineNbr, Equal<INTran.lineNbr>,
                And<INTransitLineStatus.transferNbr, Equal<INTran.refNbr>>>>))]
        public virtual void INTran_INTransitBaseQty_CacheAttached(PXCache sender)
        {
        }
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Sub<INTran.baseQty, INTran.iNTransitBaseQty>))]
        public virtual void INTran_ReceiptedBaseQty_CacheAttached(PXCache sender)
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
			if (transfer.Current != null && !String.IsNullOrEmpty(transfer.Current.BatchNbr))
			{
				GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(transfer.Current.BatchNbr, "IN");
				throw new PXRedirectRequiredException(graph, "Current batch record");
			}
			return adapter.Get();
		}

		public PXAction<INRegister> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = transfer.Cache;
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
			if (transfer.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["DocType"] = transfer.Current.DocType;
				parameters["RefNbr"] = transfer.Current.RefNbr;
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
			if (transfer.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["PeriodID"] = (string)transfer.GetValueExt<INRegister.finPeriodID>(transfer.Current);
				parameters["DocType"] = transfer.Current.DocType;
				parameters["RefNbr"] = transfer.Current.RefNbr;
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
		public PXFilter<INTransferStatusFilter> sitestatusfilter;
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public INSiteStatusLookup<INSiteStatusSelected, INTransferStatusFilter> sitestatus;

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
					newline.ToSiteID = this.transfer.Current.ToSiteID;
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

		protected virtual void INTransferStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			INTransferStatusFilter row = (INTransferStatusFilter)e.Row;
			if (row != null && transfer.Current != null)
				row.SiteID = transfer.Current.SiteID;
		}
		#endregion

		public INTransferEntry()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<INTran.tranType>(transactions.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INTran.tranType>(transactions.Cache, null, false);

            //PXDimensionSelectorAttribute.SetValidCombo<INTran.subItemID>(transactions.Cache, true);
            //PXDimensionSelectorAttribute.SetValidCombo<INTranSplit.subItemID>(splits.Cache, true);

			//PXSelectorAttribute.SetColumns<INTran.subItemID>(transactions.Cache, new Type[] { typeof(INSubItem.subItemCD), typeof(INSiteStatus.qtyOnHand), typeof(INSiteStatus.active) }, null);
			//PXSelectorAttribute.SetColumns<INTranSplit.subItemID>(splits.Cache, new Type[] { typeof(INSubItem.subItemCD), typeof(INSiteStatus.qtyOnHand), typeof(INSiteStatus.active) }, null);

            this.FieldDefaulting.AddHandler<IN.Overrides.INDocumentRelease.SiteStatus.negAvailQty>((sender, e) =>
            {
                e.NewValue = true;
                e.Cancel = true;
            });
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
							new PXDataFieldValue<INTran.docType>(PXDbType.Char, ((INTransferEntry) graph).transfer.Current?.DocType),
							new PXDataFieldValue<INTran.refNbr>(((INTransferEntry) graph).transfer.Current?.RefNbr),
							new PXDataFieldValue<INTran.createdByScreenID>(PXDbType.Char, "IN304000")
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(INTranSplit), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<INTranSplit.docType>(PXDbType.Char, ((INTransferEntry) graph).transfer.Current?.DocType),
							new PXDataFieldValue<INTranSplit.refNbr>(((INTransferEntry) graph).transfer.Current?.RefNbr),
							new PXDataFieldValue<INTranSplit.createdByScreenID>(PXDbType.Char, "IN304000")
						};
					}));
			}
		}

		protected virtual void INRegister_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Transfer;
		}

		protected virtual void Set1Step(INRegister row)
		{
			if (row.SiteID == row.ToSiteID)
			{
				row.TransferType = INTransferType.OneStep;
			} 
		}

		protected virtual void INRegister_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Set1Step((INRegister)e.Row);
			sender.SetDefaultExt<INRegister.branchID>(e.Row);
		}

		protected virtual void INRegister_ToSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if(e.Row != null)
			{
				foreach (INTran item in this.transactions.Select())
				{
					INTran updated = (INTran)this.transactions.Cache.CreateCopy(item);					
					updated.ToSiteID = ((INRegister)e.Row).ToSiteID;
					this.transactions.Cache.Update(updated);					
				}
			}
			Set1Step((INRegister)e.Row);
		}

		protected virtual void INRegister_TransferType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INRegister row = (INRegister)e.Row;
			{
				object toSiteID = row.ToSiteID;
				try
				{
					sender.RaiseFieldVerifying<INRegister.toSiteID>(row, ref toSiteID);
					sender.RaiseExceptionHandling<INRegister.toSiteID>(row, toSiteID, null);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<INRegister.toSiteID>(row, toSiteID, new PXSetPropertyException(ex, PXErrorLevel.Error, Messages.WarehouseNotAllowed, Messages.OneStep));
				}
			}
		}


		public virtual void INRegister_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

			PXDefaultAttribute.SetPersistingCheck<INTran.toLocationID>(this.transactions.Cache, null, ((INRegister)e.Row).TransferType == INTransferType.OneStep ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			object newValue = sender.GetValue<INRegister.toSiteID>(e.Row);
			try
			{
				sender.RaiseFieldVerifying<INRegister.toSiteID>(e.Row, ref newValue);
			}
			catch (PXSetPropertyException ex)
			{
				PXCache inRegisterCachce = sender.Graph.Caches[typeof(INRegister)];
				if ((String) inRegisterCachce.GetValue<INRegister.transferType>(inRegisterCachce.Current) == INTransferType.OneStep)
					sender.RaiseExceptionHandling<INRegister.toSiteID>(e.Row, newValue, new PXSetPropertyException(ex, PXErrorLevel.Error, Messages.WarehouseNotAllowed, Messages.OneStep));
			}

		}


	    protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (insetup.Current.RequireControlTotal == false)
			{
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

		public virtual void INTran_INTransitQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            INTran row = (INTran)e.Row;
            if (row == null)
                return;
            e.ReturnValue = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, row.INTransitBaseQty.GetValueOrDefault(), INPrecision.QUANTITY); ;
        }

        public virtual void INTran_ReceiptedQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            INTran row = (INTran)e.Row;
            if (row == null)
                return;
            e.ReturnValue = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, row.ReceiptedBaseQty.GetValueOrDefault(), INPrecision.QUANTITY); ;
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
			PXUIFieldAttribute.SetEnabled<INRegister.status>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.branchID>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = (((INRegister)e.Row).Released == false);
			sender.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

			lsselect.AllowInsert = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN && ((INRegister)e.Row).SiteID != null);
			lsselect.AllowUpdate = (((INRegister)e.Row).Released == false);
			lsselect.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

			PXUIFieldAttribute.SetVisible<INRegister.controlQty>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetEnabled<INRegister.siteID>(sender, e.Row, ((INRegister)e.Row).Released == false && (((INRegister)e.Row).SiteID == null || transactions.Select().Count == 0));

			PXUIFieldAttribute.SetEnabled<INRegister.transferType>(sender, e.Row, ((INRegister)e.Row).OrigModule == GL.BatchModule.IN && ((INRegister)e.Row).Released == false && (((INRegister)e.Row).SiteID == null || ((INRegister)e.Row).SiteID != ((INRegister)e.Row).ToSiteID));
			PXUIFieldAttribute.SetVisible<INTran.toLocationID>(this.transactions.Cache, null, ((INRegister)e.Row).TransferType == INTransferType.OneStep);
            PXUIFieldAttribute.SetVisible<INTran.receiptedQty>(this.transactions.Cache, null, ((INRegister)e.Row).TransferType != INTransferType.OneStep);
            PXUIFieldAttribute.SetVisible<INTran.iNTransitQty>(this.transactions.Cache, null, ((INRegister)e.Row).TransferType != INTransferType.OneStep);
            PXDefaultAttribute.SetPersistingCheck<INTran.toLocationID>(this.transactions.Cache, null, ((INRegister)e.Row).TransferType == INTransferType.OneStep ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			addInvBySite.SetEnabled(transactions.Cache.AllowInsert);
		}

		protected virtual void INTran_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Transfer;
		}

        [PXDBString(3, IsFixed = true)]
        [PXDefault(INTranType.Transfer)]
        [PXUIField(Enabled = false, Visible = false)]
        protected virtual void INTran_TranType_CacheAttached(PXCache sender)
        {
        }
	
		protected virtual void INTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INTranType.InvtMult(((INTran)e.Row).TranType);
		}

		protected virtual void INTran_ToLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INTran row = (INTran)e.Row;
			if(row == null) return;

			INItemSite itemSite = itemsite.SelectWindowed(0, 1, row.ToSiteID, row.InventoryID);
			if (itemSite != null)
			{
				e.NewValue = row.SiteID == row.ToSiteID ? itemSite.DfltShipLocationID : itemSite.DfltReceiptLocationID;
				e.Cancel = true;
			}
			else
			{
				INSite site = INSite.PK.Find(this, row.ToSiteID);
				if(site != null)
				{
					e.NewValue = row.SiteID == row.ToSiteID ? site.ShipLocationID : site.ReceiptLocationID;
					e.Cancel = true;
				}
			}
			
		}
		
		protected virtual void INTran_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INTran row = (INTran)e.Row;
			if (row == null) return;

			INItemSite itemSite = itemsite.SelectWindowed(0, 1, row.SiteID, row.InventoryID);
			if (itemSite != null)
			{
				e.NewValue = itemSite.DfltReceiptLocationID;
				e.Cancel = true;
			}
			else
			{
				INSite site = INSite.PK.Find(this, row.SiteID);
				if (site != null)
				{
					e.NewValue = site.ReceiptLocationID;
					e.Cancel = true;
				}
			}
		}	

		protected virtual void INTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INTran.uOM>(e.Row);
			sender.SetDefaultExt<INTran.tranDesc>(e.Row);
			sender.SetDefaultExt<INTran.toLocationID>(e.Row);
		}

		protected virtual void INTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (transfer.Current != null && transfer.Current.SiteID != null)
			{
				e.NewValue = transfer.Current.SiteID;
				e.Cancel = true;
			}
		}

        protected virtual void INTran_ToSiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (transfer.Current != null && transfer.Current.ToSiteID != null)
            {
                e.NewValue = transfer.Current.ToSiteID;
                e.Cancel = true;
            }
		}

		protected virtual void INTran_ToSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				foreach (INTranSplit item in this.splits.View.SelectMultiBound(new [] { e.Row }))
				{
					INTranSplit updated = (INTranSplit)this.splits.Cache.CreateCopy(item);
					updated.ToSiteID = ((INTran)e.Row).ToSiteID;
					this.splits.Cache.Update(updated);
				}
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

		protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				var tran = (INTran)e.Row;
				INRegister header = CurrentDocument.Current;

				if (header != null)
				{
					PXDefaultAttribute.SetPersistingCheck<INTran.toLocationID>(sender, tran, header.TransferType == INTransferType.OneStep ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

					if (header.SiteID != tran.SiteID || header.ToSiteID != tran.ToSiteID) // AC-139602. May occur during excel import.
					{
						if (sender.RaiseExceptionHandling<INTran.locationID>(tran, null, new PXSetPropertyException(Messages.TransferLineIsCorrupted, PXErrorLevel.RowError)))
							throw new PXRowPersistingException(nameof(INTran.locationID), null, Messages.TransferLineIsCorrupted);
					}
				}

				CheckSplitsForSameTask(sender, tran);
			}
		}

		protected virtual bool CheckSplitsForSameTask(PXCache sender, INTran row)
		{
			if (row.HasMixedProjectTasks == true)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(Messages.MixedProjectsInSplits));
				return false;
			}

			return true;
		}

		#region INRegisterEntryBase members
		public override PXSelectBase<INRegister> INRegisterDataMember => transfer;
		public override PXSelectBase<INTran> INTranDataMember => transactions;
		public override LSINTran LSSelectDataMember => lsselect;
		public override PXSelectBase<INTranSplit> INTranSplitDataMember => splits;

		#endregion

		[Serializable]
		public partial class INTransferStatusFilter : INSiteStatusFilter
		{
			#region SiteID
			public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }			
			[PXUIField(DisplayName = "Warehouse")]
			[SiteAttribute]
			[PXDefault(typeof(INRegister.siteID))]
			[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<INRegister.branchID>>>))]
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
			[IN.Location(typeof(INTransferStatusFilter.siteID))]
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

			#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
			protected String _ReceiptNbr;
			[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXSelector(typeof(Search2<PO.POReceipt.receiptNbr,
					InnerJoin<AP.Vendor, On<PO.POReceipt.vendorID, Equal<AP.Vendor.bAccountID>>>,
				Where<PO.POReceipt.receiptType, Equal<PO.POReceiptType.poreceipt>,
				And<Match<AP.Vendor, Current<AccessInfo.userName>>>>>), Filterable = true)]
			[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		}

		[System.SerializableAttribute()]
		[PXProjection(typeof(Select2<InventoryItem,
			LeftJoin<INLocationStatus,
							On<INLocationStatus.FK.InventoryItem>,
            LeftJoin<INLocation,
                            On<INLocationStatus.locationID, Equal<INLocation.locationID>>,
            LeftJoin<INSubItem,
							On<INLocationStatus.FK.SubItem>,
			LeftJoin<INSite,
							On<INLocationStatus.FK.Site>,
			LeftJoin<INItemXRef,
						On2<INItemXRef.FK.InventoryItem,
						And<INItemXRef.alternateType, Equal<INAlternateType.barcode>,
						And2<Where<INItemXRef.subItemID, Equal<INLocationStatus.subItemID>,
								Or<INLocationStatus.subItemID, IsNull>>,
						And<CurrentValue<INTransferStatusFilter.barCode>, IsNotNull>>>>,
			LeftJoin<INItemClass,
							On<InventoryItem.FK.ItemClass>,
			LeftJoin<INPriceClass,
							On<InventoryItem.FK.PriceClass>,
			LeftJoin<PO.POReceiptLine,
						 On<PO.POReceiptLine.receiptType, Equal<PO.POReceiptType.poreceipt>,
						 And<PO.POReceiptLine.receiptNbr, Equal<CurrentValue<INTransferStatusFilter.receiptNbr>>,
						 And<PO.POReceiptLine.siteID, Equal<CurrentValue<INRegister.siteID>>,
						 And<PO.POReceiptLine.inventoryID, Equal<InventoryItem.inventoryID>>>>>>>>>>>>>,

			Where2<CurrentMatch<InventoryItem, AccessInfo.userName>,
				And2<Where<INLocationStatus.siteID, IsNull, Or<INSite.branchID, IsNotNull, And2<CurrentMatch<INSite, AccessInfo.userName>,
					And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
						Or<SameOrganizationBranch<INSite.branchID, Current<INRegister.branchID>>>>>>>>,
				And2<Where<INLocationStatus.subItemID, IsNull,
								Or<CurrentMatch<INSubItem, AccessInfo.userName>>>,
				And2<Where<CurrentValue<INTransferStatusFilter.onlyAvailable>, Equal<CS.boolFalse>,
				   Or<INLocationStatus.qtyOnHand, Greater<CS.decimal0>>>,
				And2<Where<CurrentValue<INTransferStatusFilter.receiptNbr>, IsNull,
							 Or<PO.POReceiptLine.lineNbr, IsNotNull>>,
				 And<InventoryItem.stkItem, Equal<PX.Objects.CS.boolTrue>,
				 And<InventoryItem.isTemplate, Equal<False>,
				 And<InventoryItem.itemStatus, NotIn3<InventoryItemStatus.unknown, InventoryItemStatus.inactive, InventoryItemStatus.markedForDeletion>>>>>>>>>>),
			Persistent = false)]
		public partial class INSiteStatusSelected : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
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

			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[Inventory(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
			[PXDefault()]
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

			#region InventoryCD
			public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
			protected String _InventoryCD;
			[PXDefault()]
			[InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
			public virtual String InventoryCD
			{
				get
				{
					return this._InventoryCD;
				}
				set
				{
					this._InventoryCD = value;
				}
			}
			#endregion

			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			protected String _Descr;
			[PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion

			#region ItemClassID
			public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
			protected int? _ItemClassID;
			[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
			[PXUIField(DisplayName = "Item Class ID", Visible = false)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
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

			#region ItemClassCD
			public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
			protected string _ItemClassCD;
			[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
			public virtual string ItemClassCD
			{
				get
				{
					return this._ItemClassCD;
				}
				set
				{
					this._ItemClassCD = value;
				}
			}
			#endregion

			#region ItemClassDescription
			public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
			protected String _ItemClassDescription;
			[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
			[PXUIField(DisplayName = "Item Class Description", Visible = false)]
			public virtual String ItemClassDescription
			{
				get
				{
					return this._ItemClassDescription;
				}
				set
				{
					this._ItemClassDescription = value;
				}
			}
			#endregion

			#region PriceClassID
			public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
			protected String _PriceClassID;
			[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
			[PXUIField(DisplayName = "Price Class ID", Visible = false)]
			public virtual String PriceClassID
			{
				get
				{
					return this._PriceClassID;
				}
				set
				{
					this._PriceClassID = value;
				}
			}
			#endregion

			#region PriceClassDescription
			public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
			protected String _PriceClassDescription;
			[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
			[PXUIField(DisplayName = "Price Class Description", Visible = false)]
			public virtual String PriceClassDescription
			{
				get
				{
					return this._PriceClassDescription;
				}
				set
				{
					this._PriceClassDescription = value;
				}
			}
			#endregion		

			#region BarCode
			public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
			protected String _BarCode;
			[PXDBString(255, BqlField = typeof(INItemXRef.alternateID), IsUnicode = true)]
			//[PXUIField(DisplayName = "Barcode")]
			public virtual String BarCode
			{
				get
				{
					return this._BarCode;
				}
				set
				{
					this._BarCode = value;
				}
			}
			#endregion

			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected int? _SiteID;
			[PXUIField(DisplayName = "Warehouse")]
			[SiteAttribute(BqlField = typeof(INLocationStatus.siteID), IsKey = true)]
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

            #region SiteCD
            public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
            protected String _SiteCD;
            [PXDBString(IsUnicode = true, BqlField = typeof(INSite.siteCD))]
			[PXDimension(SiteAttribute.DimensionName)]
			public virtual String SiteCD
            {
                get
                {
                    return this._SiteCD;
                }
                set
                {
                    this._SiteCD = value;
                }
            }
            #endregion

            #region LocationID
            public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			protected Int32? _LocationID;
			[Location(typeof(INSiteStatusSelected.siteID), BqlField = typeof(INLocationStatus.locationID), IsKey = true)]
			[PXDefault()]
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

            #region LocationCD
            public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
            protected String _locationCD;
            [PXDBString(BqlField = typeof(INLocation.locationCD), IsUnicode = true)]
			[PXDimension(LocationAttribute.DimensionName)]
			[PXDefault()]
            public virtual String LocationCD
            {
                get
                {
                    return this._locationCD;
                }
                set
                {
                    this._locationCD = value;
                }
            }
            #endregion

            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			protected int? _SubItemID;
			[SubItem(typeof(INSiteStatusSelected.inventoryID), BqlField = typeof(INSubItem.subItemID), IsKey = true)]
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

			#region SubItemCD
			public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
			protected String _SubItemCD;
			[PXDBString(IsUnicode = true, BqlField = typeof(INSubItem.subItemCD))]
			[PXDimension(SubItemAttribute.DimensionName)]
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

			#region BaseUnit
			public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
			protected String _BaseUnit;
			[PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
			[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
			public virtual String BaseUnit
			{
				get
				{
					return this._BaseUnit;
				}
				set
				{
					this._BaseUnit = value;
				}
			}
			#endregion

			#region QtySelected
			public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
			protected Decimal? _QtySelected;
			[PXQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Qty. Selected")]
			public virtual Decimal? QtySelected
			{
				get
				{
					return this._QtySelected ?? 0m;
				}
				set
				{
					if (value != null && value != 0m)
						this._Selected = true;
					this._QtySelected = value;
				}
			}
			#endregion

			#region QtyOnHand
			public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
			protected Decimal? _QtyOnHand;
			[PXDBQuantity(BqlField = typeof(INLocationStatus.qtyOnHand))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Qty. On Hand")]
			public virtual Decimal? QtyOnHand
			{
				get
				{
					return this._QtyOnHand;
				}
				set
				{
					this._QtyOnHand = value;
				}
			}
			#endregion

			#region QtyAvail
			public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
			protected Decimal? _QtyAvail;
			[PXDBQuantity(BqlField = typeof(INLocationStatus.qtyAvail))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Qty. Available")]
			public virtual Decimal? QtyAvail
			{
				get
				{
					return this._QtyAvail;
				}
				set
				{
					this._QtyAvail = value;
				}
			}
			#endregion

			#region NoteID
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			protected Guid? _NoteID;
			[PXNote(BqlField = typeof(InventoryItem.noteID))]
			public virtual Guid? NoteID
			{
				get
				{
					return this._NoteID;
				}
				set
				{
					this._NoteID = value;
				}
			}
			#endregion
		}
	}
}

