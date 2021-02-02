using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.CT;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System.Linq;
using PX.Common;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.PR.Standalone;

namespace PX.Objects.PM
{
    [Serializable]
	public class RegisterEntry : PXGraph<RegisterEntry, PMRegister>, PXImportAttribute.IPXPrepareItems
	{
		#region DAC Attributes Override

		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<PMTran.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(PMTran.inventoryID))]
		protected virtual void PMTran_UOM_CacheAttached(PXCache sender) { }

		#endregion

		[PXHidden]
		public PXSelect<PMProject> Project;

		[PXHidden]
		public PXSetup<Company> Company;

		[PXHidden]
		public PXSelect<BAccount> dummy;

		[PXHidden]
		public PXSelect<Account> accountDummy;

		public PXSelect<PMRegister, Where<PMRegister.module, Equal<Optional<PMRegister.module>>>> Document;

		[PXImport(typeof(PMRegister))]
		public PXSelect<PMTran, 
			Where<PMTran.tranType, Equal<Current<PMRegister.module>>, 
			And<PMTran.refNbr, Equal<Current<PMRegister.refNbr>>>>> Transactions;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PMTran.projectCuryInfoID>>>> ProjectCuryInfo;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PMTran.baseCuryInfoID>>>> BaseCuryInfo;

		public PXSelect<CurrencyInfo> CuryInfo;

		public PXSelect<PMAllocationSourceTran, 
			Where<PMAllocationSourceTran.allocationID, Equal<Required<PMAllocationSourceTran.allocationID>>,
			And<PMAllocationSourceTran.tranID, Equal<Required<PMAllocationSourceTran.tranID>>>>> SourceTran;

		public PXSelect<PMAllocationAuditTran> AuditTrans;

		public PXSelect<PMRecurringItemAccum> RecurringItems;

		public PXSelect<PMTaskAllocTotalAccum> AllocationTotals;

		public PXSetup<PMSetup> Setup;

		public CMSetupSelect CMSetup;

		public PXSelect<PMTimeActivity> Activities;

		public PXSelect<ContractDetailAcum> ContractDetails;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public RegisterEntry()
        {

            if (PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>())
            {
				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if ( setup == null)
                throw new PXException(Messages.SetupNotConfigured);
            }
            else
            {
				ARSetup setup = PXSelect<ARSetup>.Select(this);
				if (setup == null)
					throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(ARSetup), typeof(ARSetup).Name);
				AutoNumberAttribute.SetNumberingId<PMRegister.refNbr>(Document.Cache, setup.UsageNumberingID);
            }

			selectBaseRate.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());
			selectProjectRate.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());
			curyToggle.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());
        }

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{		
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		/// <summary>
		/// Gets the source for the generated PMTran.AccountID
		/// </summary>
		public string ExpenseAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccountSource;
                }

                return result;
            }
        }

        public string ExpenseSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseSubMask))
                {
                    result = setup.ExpenseSubMask;
                }

                return result;
            }
        }

        public string ExpenseAccrualAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccrualAccountSource;
                }

                return result;
            }
        }

        public string ExpenseAccrualSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccrualSubMask))
                {
                    result = setup.ExpenseAccrualSubMask;
                }

                return result;
            }
        }

		public PXAction<PMRegister> curyToggle;
		[PXUIField(DisplayName = Messages.ViewBase)]
		[PXProcessButton]
		public IEnumerable CuryToggle(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				var wasDirty = Document.Cache.IsDirty;
				Document.Current.IsBaseCury = !Document.Current.IsBaseCury.GetValueOrDefault();
				Document.Update(Document.Current);
				if (!wasDirty && Document.Cache.IsDirty)
					Document.Cache.IsDirty = false;
			}
			return adapter.Get();
		}


        public PXAction<PMRegister> release;
        [PXUIField(DisplayName = GL.Messages.Release)]
        [PXProcessButton]
        public IEnumerable Release(PXAdapter adapter)
        {
			ReleaseDocument(Document.Current);

			yield return Document.Current;
        }

		public virtual void ReleaseDocument(PMRegister doc)
		{
			if (doc != null && doc.Released != true)
			{
				this.Save.Press();
				PXLongOperation.StartOperation(this, delegate()
				{
					RegisterRelease.Release(doc);
				});
			}
		}

		public PXAction<PMRegister> reverse;
		[PXUIField(DisplayName = Messages.ReverseAllocation)]
		[PXProcessButton(Tooltip = Messages.ReverseAllocationTip)]
		public void Reverse()
		{
			var reversalSelect = new PXSelect<PMRegister, Where<PMRegister.module, Equal<Current<PMRegister.module>>, 
				And<PMRegister.origNoteID, Equal<Current<PMRegister.noteID>>,
				And<PMRegister.origDocType, Equal<PMOrigDocType.reversal>>>>>(this);

			if (Document.Current != null && Document.Current.IsAllocation == true && Document.Current.Released == true)
			{
				PMRegister reversalDoc = reversalSelect.Select();

				if (reversalDoc != null)
				{
					throw new PXException(Messages.ReversalExists, reversalDoc.RefNbr);
				}

				RegisterEntry target = null;
				List<ProcessInfo<Batch>> infoList;
				using (new PXConnectionScope())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						target = PXGraph.CreateInstance<RegisterEntry>();
						target.FieldVerifying.AddHandler<PMTran.inventoryID>(SuppressFieldVerifying);
						PMRegister doc = (PMRegister)target.Document.Cache.Insert();
						doc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversal);
						doc.OrigDocType = PMOrigDocType.Reversal;
						doc.OrigNoteID = Document.Current.NoteID;

						PMBillEngine billEngine = PXGraph.CreateInstance<PMBillEngine>();

						foreach (PMTran tran in Transactions.Select())
						{
							var reversal = billEngine.ReverseTran(tran).First();
							target.Transactions.Insert(reversal);

							tran.ExcludedFromBilling = true;
							tran.ExcludedFromBillingReason = PXMessages.LocalizeNoPrefix(Messages.ExcludedFromBillingAsReversed);
							PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, tran);
							Transactions.Update(tran);
						}

						target.Save.Press();
											
						List<PMRegister> list = new List<PMRegister>();
						list.Add(doc);
						bool releaseSuccess = RegisterRelease.ReleaseWithoutPost(list, false, out infoList);
						if (!releaseSuccess)
						{
							throw new PXException(GL.Messages.DocumentsNotReleased);
						}
												
						Transactions.Cache.AllowUpdate = true;
						foreach (PMTran tran in Transactions.Select())
						{
							UnallocateTran(tran);
						}

						this.Save.Press();
						ts.Complete();
					}

					//Posting should always be performed outside of transaction
					bool postSuccess = RegisterRelease.Post(infoList, false);
					if (!postSuccess)
					{
						throw new PXException(GL.Messages.DocumentsNotPosted);
					}
				}
				
				if (!IsImport && !this.IsContractBasedAPI) //Using Import to mass reverse allocations.  
				{
					target.Document.Current = reversalSelect.Select();
					throw new PXRedirectRequiredException(target, "Open Reversal");
				}
			}
		}

		public PXAction<PMRegister> viewProject;
        [PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewProject(PXAdapter adapter)
        {
            if (Transactions.Current != null)
            {
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Transactions.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
            return adapter.Get();
        }

        public PXAction<PMRegister> viewTask;
        [PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewTask(PXAdapter adapter)
        {
            var graph = CreateInstance<ProjectTaskEntry>();
            graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMTran.taskID>>>>.Select(this);
            throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

		public PXAction<PMRegister> viewAllocationSorce;
		[PXUIField(DisplayName = Messages.ViewAllocationSource)]
		[PXButton]
		public IEnumerable ViewAllocationSorce(PXAdapter adapter)
		{
			if (Transactions.Current != null)
			{
				AllocationAudit graph = PXGraph.CreateInstance<AllocationAudit>();
				graph.Clear();
				graph.destantion.Current.TranID = Transactions.Current.TranID;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewAllocationSource) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<PMRegister> viewInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewInventory(PXAdapter adapter)
		{
			InventoryItem inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMTran.inventoryID>>>>.SelectSingleBound(this, new object[] { Transactions.Current });
			if (inv != null && inv.StkItem == true)
			{
				InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
				graph.Item.Current = inv;
				throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			else if (inv != null)
			{
				NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
				graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(inv.InventoryID);
				throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

        public PXAction<PMRegister> viewCustomer;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewCustomer(PXAdapter adapter)
        {
            BAccount account = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);

            if (account != null)
            {
                if (account.Type == BAccountType.CustomerType || account.Type == BAccountType.CombinedType)
                {
                    CustomerMaint graph = CreateInstance<CustomerMaint>();
                    graph.BAccount.Current = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else if (account.Type == BAccountType.VendorType)
                {
                    VendorMaint graph = CreateInstance<VendorMaint>();
                    graph.BAccount.Current = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else if (account.Type == BAccountType.EmployeeType || account.Type == BAccountType.EmpCombinedType)
                {
                    EmployeeMaint graph = CreateInstance<EmployeeMaint>();
                    graph.Employee.Current = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            return adapter.Get();
        }

        public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public PXAction<PMRegister> selectProjectRate;
		[PXUIField(DisplayName = "Select Project Currency Rate")]
		[PXButton]
		public IEnumerable SelectProjectRate(PXAdapter adapter)
		{
			if (Transactions.Cache.Cached.Count() > 0)
				ProjectCuryInfo.AskExt();
			return adapter.Get();
		}

		public PXAction<PMRegister> selectBaseRate;
		[PXUIField(DisplayName = "Select Base Currency Rate")]
		[PXButton]
		public IEnumerable SelectBaseRate(PXAdapter adapter)
		{
			if (Transactions.Cache.Cached.Count() > 0)
				BaseCuryInfo.AskExt();
			return adapter.Get();
		}

		#region Event Handlers


		#region PMRegister

		protected virtual void _(Events.FieldUpdated<PMRegister, PMRegister.isBaseCury> e)
		{
			if (e.Row != null)
				Accessinfo.CuryViewState = e.Row.IsBaseCury.GetValueOrDefault();
				}

		protected virtual void _(Events.FieldUpdated<PMRegister, PMRegister.hold> e)
		{
			if (e.Row.Released == true)
				return;

			if (e.Row.Hold == true)
			{
				e.Row.Status = PMRegister.status.Hold;
			}
			else
			{
				e.Row.Status = PMRegister.status.Balanced;
			}
		}

		protected virtual void _(Events.RowSelected<PMRegister> e)
		{
			if (e.Row != null)
			{
				curyToggle.SetCaption(e.Row.IsBaseCury == true ? Messages.ViewCury: Messages.ViewBase);

				PXUIFieldAttribute.SetEnabled<PMRegister.date>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.description>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.status>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.hold>(e.Cache, e.Row, e.Row.Released != true);

				Document.Cache.AllowUpdate = e.Row.Released != true && e.Row.Module == BatchModule.PM;
				Document.Cache.AllowDelete = e.Row.Released != true && e.Row.Module == BatchModule.PM;
				Insert.SetEnabled(e.Row.Module == BatchModule.PM);
				release.SetEnabled(e.Row.Released != true && e.Row.Hold != true);

				Transactions.Cache.AllowDelete = e.Row.Released != true && e.Row.IsAllocation != true;
				Transactions.Cache.AllowInsert = e.Row.Released != true && e.Row.IsAllocation != true && e.Row.Module == BatchModule.PM;
				Transactions.Cache.AllowUpdate = e.Row.Released != true;

				reverse.SetEnabled(e.Row.Released == true && e.Row.IsAllocation == true);
				viewAllocationSorce.SetEnabled(e.Row.OrigDocType == PMOrigDocType.Allocation);
				curyToggle.SetEnabled(true);
				selectProjectRate.SetEnabled(true);
				selectBaseRate.SetEnabled(true);

				PXUIFieldAttribute.SetVisible<PMRegister.origDocType>(e.Cache, e.Row, e.Row.Module == BatchModule.PM);
				PXUIFieldAttribute.SetVisible<PMRegister.origNoteID>(e.Cache, e.Row, e.Row.Module == BatchModule.PM);

				if (!this.IsImport && !this.IsContractBasedAPI)
				{
					decimal qty = 0, billableQty = 0, amount = 0;
					//no need to calculate when doing import. It will just slow down the import.

					foreach (PMTran tran in Transactions.Select())
					{
						qty += tran.Qty.GetValueOrDefault();
						billableQty += tran.BillableQty.GetValueOrDefault();
						amount += tran.Amount.GetValueOrDefault();
					}

					e.Row.QtyTotal = qty;
					e.Row.BillableQtyTotal = billableQty;
					e.Row.AmtTotal = amount;
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMRegister> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Released != true && e.Row.OrigDocType == PMOrigDocType.Timecard && e.Row.OrigNoteID != null)
				{
					EPTimeCard timeCard = PXSelect<EPTimeCard, Where<EPTimeCard.noteID, Equal<Required<EPTimeCard.noteID>>>>.Select(this, e.Row.OrigNoteID);
					if (timeCard != null)
					{
						Views.Caches.Add(typeof(EPTimeCard));
						UnreleaseTimeCard(timeCard);
					}
				}
			}
		}

		protected virtual void UnreleaseTimeCard(EPTimeCard timeCard)
		{
			timeCard.IsReleased = false;
			timeCard.Status = EPTimeCardStatusAttribute.ApprovedStatus;
			Caches[typeof(EPTimeCard)].Update(timeCard);
		}

		#endregion

		#region PMTran

		protected virtual void PMTran_BranchID_FieldUpdated(Events.FieldUpdated<PMTran, PMTran.branchID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.finPeriodID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.bAccountID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.locationID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.inventoryID> e)
		{
			if (e.Row != null && string.IsNullOrEmpty(e.Row.Description) && e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.Row.InventoryID);
				if (item != null)
				{
					e.Row.Description = item.Descr;

					PMProject project = PXSelect<PMProject,
						Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, e.Row.ProjectID);

					if (project != null && project.CustomerID != null)
					{
						Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);
						if (customer != null && !string.IsNullOrEmpty(customer.LocaleName))
						{
							e.Row.Description = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, nameof(InventoryItem.Descr), customer.LocaleName);
						}
					}
				}
			}

			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.uOM>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.qty> e)
		{
			if (e.Row != null && e.Row.Billable == true)
			{
				e.Cache.SetDefaultExt<PMTran.billableQty>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.billable> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Billable == true)
				{
					e.Cache.SetDefaultExt<PMTran.billableQty>(e.Row);
				}
				else
				{
					e.Cache.SetValueExt<PMTran.billableQty>(e.Row, 0m);
				}
			}
		}
				
		protected virtual void _(Events.FieldDefaulting<PMTran, PMTran.billableQty> e)
		{
			if (e.Row != null && e.Row.Billable == true)
			{
				e.NewValue = e.Row.Qty;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.billableQty> e)
		{
			if (e.Row != null && e.Row.BillableQty != 0)
			{
				SubtractUsage(e.Cache, e.Row, (decimal?)e.OldValue, e.Row.UOM);
				AddUsage(e.Cache, e.Row, e.Row.BillableQty, e.Row.UOM);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.uOM> e)
		{
			if (e.Row != null && e.Row.BillableQty != 0)
			{
				SubtractUsage(e.Cache, e.Row, e.Row.BillableQty, (string)e.OldValue);
				AddUsage(e.Cache, e.Row, e.Row.BillableQty, e.Row.UOM);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.date> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.finPeriodID>(e.Row);
				if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
				{
					InitializeRates(e.Row); //needed for excel

					CurrencyInfo baseCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(e.Row.BaseCuryInfoID);
					baseCuryInfo.CuryEffDate = e.Row.Date;
					CuryInfo.Update(baseCuryInfo);
					if (!IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(CuryInfo.Cache, baseCuryInfo)))
						e.Cache.RaiseExceptionHandling<PMTran.date>(e.Row, null, GetCurrencyRateError(baseCuryInfo));

					CurrencyInfo projectCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(e.Row.ProjectCuryInfoID);
					projectCuryInfo.CuryEffDate = e.Row.Date;
					CuryInfo.Update(projectCuryInfo);
					if (!IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(CuryInfo.Cache, projectCuryInfo)))
						e.Cache.RaiseExceptionHandling<PMTran.date>(e.Row, null, GetCurrencyRateError(projectCuryInfo));
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMTran, PMTran.resourceID> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTran.projectID>>>>.Select(this);
				if (project != null && project.RestrictToEmployeeList == true)
				{
					EPEmployeeContract rate = PXSelect<EPEmployeeContract, Where<EPEmployeeContract.contractID, Equal<Current<PMTran.projectID>>,
						And<EPEmployeeContract.employeeID, Equal<Required<EPEmployeeContract.employeeID>>>>>.Select(this, e.NewValue);
					if (rate == null)
					{
						EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, e.NewValue);
						if (emp != null)
							e.NewValue = emp.AcctCD;

						throw new PXSetPropertyException(Messages.EmployeeNotInProjectList);
					}
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryID> e)
		{
			if (e.Row != null)
			{
				CurrencyInfo projectCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(e.Row.ProjectCuryInfoID);
				e.ReturnValue = projectCuryInfo.BaseCuryID;
			}
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryRate> e)
		{
			if (e.Row != null)
			{
				CurrencyInfo projectCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(e.Row.ProjectCuryInfoID);
				e.ReturnValue = projectCuryInfo.SampleCuryRate;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMTran, PMTran.tranCuryID> e)
		{
			if (e.Row != null && e.Row.ProjectID != null && PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
			{
				PMProject project;
				if (ProjectDefaultAttribute.IsProject(this, e.Row.ProjectID, out project))
				{
				e.NewValue = project.CuryID;
			}
				else
				{
					e.NewValue = Company.Current.BaseCuryID;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.tranCuryID> e)
		{
			if (e.Row != null && PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
			{
				CurrencyInfo projectCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(e.Row.ProjectCuryInfoID);
				if (!IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(CuryInfo.Cache, projectCuryInfo)))
					e.Cache.RaiseExceptionHandling<PMTran.tranCuryID>(e.Row, null, GetCurrencyRateError(projectCuryInfo));

				CurrencyInfo baseCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(e.Row.BaseCuryInfoID);
				if (!IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(CuryInfo.Cache, baseCuryInfo)))
					e.Cache.RaiseExceptionHandling<PMTran.tranCuryID>(e.Row, null, GetCurrencyRateError(baseCuryInfo));
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.projectID> e)
		{
			if (e.Row != null && e.Row.ProjectID != null && PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
			{
				InitializeRates(e.Row); //needed for excel
				CalcCuryRatesForProject(e.Cache, e.Row);
			}
		}

		private void InitializeRates(PMTran tran)
		{
			if (tran.ProjectCuryInfoID == null)
			{
				CurrencyInfo projectCuryInfo = CuryInfo.Cache.Insert() as CurrencyInfo;
				projectCuryInfo.CuryRateTypeID = CMSetup.Current.PMRateTypeDflt;
				tran.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;
			}

			if (tran.BaseCuryInfoID == null)
			{
				CurrencyInfo baseCuryInfo = CuryInfo.Cache.Insert() as CurrencyInfo;
				baseCuryInfo.CuryRateTypeID = CMSetup.Current.PMRateTypeDflt;
				tran.BaseCuryInfoID = baseCuryInfo.CuryInfoID;
			}
		}

		private void CalcCuryRatesForProject(PXCache cache, PMTran tran)
		{
			PMProject project = Project.Search<PMProject.contractID>(tran.ProjectID);

			CurrencyInfo projectCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(tran.ProjectCuryInfoID);
			projectCuryInfo.BaseCuryID = project.NonProject == true ? Company.Current.BaseCuryID : project.CuryID;
			if (project.RateTypeID != null) projectCuryInfo.CuryRateTypeID = project.RateTypeID;
			//needed for CuryInfo recalculation with changed BaseCuryID
			projectCuryInfo.CuryEffDate = DateTime.MinValue;
			CuryInfo.Cache.Update(projectCuryInfo);
			projectCuryInfo.CuryEffDate = tran.Date;
			CuryInfo.Cache.Update(projectCuryInfo);
			if (projectCuryInfo.CuryRate == null && !IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(CuryInfo.Cache, projectCuryInfo)))
				cache.RaiseExceptionHandling<PMTran.projectID>(tran, null, GetCurrencyRateError(projectCuryInfo));

			CurrencyInfo baseCuryInfo = CuryInfo.Search<CurrencyInfo.curyInfoID>(tran.BaseCuryInfoID);
			if (project.RateTypeID != null) baseCuryInfo.CuryRateTypeID = project.RateTypeID;
			CuryInfo.Cache.Update(baseCuryInfo);
			CuryInfo.SetValueExt<CurrencyInfo.curyEffDate>(baseCuryInfo, tran.Date); //need for error appearing
			if (!IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(CuryInfo.Cache, baseCuryInfo)))
				cache.RaiseExceptionHandling<PMTran.projectID>(tran, null, GetCurrencyRateError(baseCuryInfo));
		}

		private PXSetPropertyException GetCurrencyRateError(CurrencyInfo info)
		{
			return new PXSetPropertyException(Messages.CurrencyRateIsNotDefined, PXErrorLevel.Warning,
				info.CuryID, info.BaseCuryID, info.CuryRateTypeID, info.CuryEffDate);
		}

		protected virtual void _(Events.RowSelected<PMTran> e)
		{
			if (e.Row != null)
			{
				var canBeAllocated = e.Row.Allocated != true && e.Row.ExcludedFromAllocation != true;
				PXUIFieldAttribute.SetEnabled<PMTran.billableQty>(e.Cache, e.Row, e.Row.Billable == true);
				PXUIFieldAttribute.SetEnabled<PMTran.projectID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.taskID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.accountGroupID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.accountID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.offsetAccountID>(e.Cache, e.Row, canBeAllocated);
			}
		}

		protected virtual void _(Events.RowInserted<PMTran> e)
		{
			if (e.Row != null)
			{
				AddAllocatedTotal(e.Row);

				if (e.Row.BillableQty != 0)
				{
					AddUsage(e.Cache, e.Row, e.Row.BillableQty, e.Row.UOM);
				}
			}
		}

		protected virtual void _(Events.RowUpdated<PMTran> e)
		{
			if (e.Row != null && e.OldRow != null && e.Row.Released != true &&
				e.Row.TranCuryAmount != e.OldRow.TranCuryAmount || e.Row.BillableQty != e.OldRow.BillableQty || e.Row.Qty != e.OldRow.Qty)
			{
				SubtractAllocatedTotal(e.OldRow);
				AddAllocatedTotal(e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<PMTran> e)
        {
			UnallocateTran(e.Row);
			UnreleaseActivity(e.Row);
        }

		protected virtual void UnallocateTran(PMTran row)
		{
			if (row != null)
			{
				PXSelectBase<PMAllocationAuditTran> select = new PXSelectJoin<PMAllocationAuditTran,
					InnerJoin<PMTran, On<PMTran.tranID, Equal<PMAllocationAuditTran.sourceTranID>>>,
					Where<PMAllocationAuditTran.tranID, Equal<Required<PMAllocationAuditTran.tranID>>>>(this);

				foreach (PXResult<PMAllocationAuditTran, PMTran> res in select.Select(row.TranID))
				{
					PMAllocationAuditTran aTran = (PMAllocationAuditTran) res;
					PMTran pmTran = (PMTran)res;

					if (!(pmTran.TranType == row.TranType && pmTran.RefNbr == row.RefNbr))
					{
						pmTran.Allocated = false;
						Transactions.Update(pmTran);
					}

					PMAllocationSourceTran ast = SourceTran.Select(aTran.AllocationID, aTran.SourceTranID);
					SourceTran.Delete(ast);
					AuditTrans.Delete(aTran);
				}

				SubtractAllocatedTotal(row);
			}
		}

        protected virtual void UnreleaseActivity(PMTran row)
        {
			if (row.OrigRefID != null && Document.Current != null && Document.Current.IsAllocation != true)
            {
                PMTimeActivity activity = PXSelect<PMTimeActivity, 
					Where<PMTimeActivity.noteID, Equal<Required<PMTimeActivity.noteID>>>>.Select(this, row.OrigRefID);

                if (activity != null)
                {
                    activity.Released = false;
                    activity.EmployeeRate = null;
                    Activities.Update(activity);
                }
            }
        }

		protected virtual void _(Events.RowPersisting<PMTran> e)
		{
			if (e.Row != null && e.Operation != PXDBOperation.Delete)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, e.Row.ProjectID);
				if (project != null && e.Row.AccountGroupID == null && project.BaseType == CT.CTPRType.Project && !ProjectDefaultAttribute.IsNonProject(project.ContractID))
				{
					e.Cache.RaiseExceptionHandling<PMTran.accountGroupID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMTran.accountGroupID).Name));
				}
			}
		}
				
		#endregion

		#region CurencyInfo

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyEffDate> e)
		{
			if (Transactions.Current != null && (e.Row.CuryInfoID == Transactions.Current.ProjectCuryInfoID || e.Row.CuryInfoID == Transactions.Current.BaseCuryInfoID))
			{
				e.NewValue = Transactions.Current.Date;
				e.Cancel = true;
			}
			//on import from excel there is no way to obtain tran date so it doesnt need to redefault it
			else if (e.Row.CuryEffDate != null)
			{
				e.NewValue = e.Row.CuryEffDate;
				e.Cancel = true;
			}
		}

		#endregion

		#endregion

		public virtual void ReverseCreditMemo(ARRegister arDoc, List<PXResult<ARTran, PMTran>> list)
		{
			var billEngine = PXGraph.CreateInstance<PMBillEngine>();

			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.CreditMemo;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.CreditMemo);

			foreach (PXResult<ARTran, PMTran> item in list)
			{
				ARTran ar = (ARTran)item;
				PMTran pm = (PMTran)item;

				PMTran newTran = PXCache<PMTran>.CreateCopy(pm);
				newTran.OrigTranID = pm.TranID;
				if (!IsFinPeriodValid(newTran))
				{
					newTran.FinPeriodID = ar.FinPeriodID;
				}
				newTran.TranID = null;
				newTran.TranType = null;
				newTran.RefNbr = null;
				newTran.RefLineNbr = null;
				newTran.ARRefNbr = null;
				newTran.ARTranType = null;
				newTran.ProformaRefNbr = null;
				newTran.ProformaLineNbr = null;
				newTran.BatchNbr = null;
				newTran.TranDate = null;
				newTran.TranPeriodID = null;
				newTran.BilledDate = null;
				newTran.NoteID = null;
				newTran.IsNonGL = true;
				newTran.Released = false;
				newTran.Billed = false;
				newTran.Allocated = false;
				newTran.ExcludedFromBilling = false;
				newTran = Transactions.Insert(newTran);

				string note = PXNoteAttribute.GetNote(Transactions.Cache, pm);
				if (note != null)
					PXNoteAttribute.SetNote(Transactions.Cache, newTran, note);

				Guid[] files = PXNoteAttribute.GetFileNotes(Transactions.Cache, pm);
				if (files != null && files.Length > 0)
					PXNoteAttribute.SetFileNotes(Transactions.Cache, newTran, files);

				if (pm.Reverse == PMReverse.Never)
				{
					var reversal = billEngine.ReverseTran(pm).First();
					if (!IsFinPeriodValid(reversal))
					{
						reversal.FinPeriodID = ar.FinPeriodID;
					}
					Transactions.Insert(reversal);
				}

				PMTran remainder = SelectFrom<PMTran>.Where<PMTran.remainderOfTranID.IsEqual<@P.AsInt>>.View.Select(this, pm.TranID);
				if (remainder != null && remainder.Billed != true)
				{
					remainder.ExcludedFromBilling = true;
					remainder.ExcludedFromBillingReason = PXMessages.LocalizeFormatNoPrefix(Messages.ExcludedFromBillingAsCreditMemoWrittenOff, arDoc.RefNbr);
					Transactions.Cache.MarkUpdated(remainder);
				}
			}
		}

		protected virtual bool IsFinPeriodValid(PMTran tran)
		{
			try
			{
				string newValue = tran.FinPeriodID;
				OpenPeriodAttribute attribute = new OpenPeriodAttribute();
				attribute.IsValidPeriod(Transactions.Cache, tran, newValue);
			}
			catch (PXSetPropertyException)
			{
				return false;
			}

			return true;
		}

		public virtual List<PMTran> BillLater(ARRegister arDoc, List<Tuple<PMProformaTransactLine, PMTran>> billLater)
		{
			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.UnbilledRemainder;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.UnbilledRemainder);

			var remainders = new List<PMTran>();
			foreach (var res in billLater)
			{
				var pfLine = res.Item1;
				var tran = res.Item2;
				PMProject project;
				if (ProjectDefaultAttribute.IsProject(this, tran.ProjectID, out project))
				{
					PMTran newTran = PXCache<PMTran>.CreateCopy(tran);
					newTran.RemainderOfTranID = tran.TranID;
					if (newTran.TranCuryID != arDoc.CuryID)
					{
						newTran.TranCuryID = arDoc.CuryID;
						newTran.BaseCuryInfoID = null;
						newTran.ProjectCuryInfoID = null;
					}

                    if (!IsFinPeriodValid(newTran))
                    {
                        newTran.FinPeriodID = arDoc.FinPeriodID;
                    }
					
					newTran.TranID = null;
					newTran.TranType = null;
					newTran.RefNbr = null;
					newTran.ARRefNbr = null;
					newTran.ARTranType = null;
					newTran.RefLineNbr = null;
					newTran.ProformaRefNbr = null;
					newTran.ProformaLineNbr = null;
					newTran.BatchNbr = null;
					newTran.TranDate = null;
					newTran.TranPeriodID = null;
					newTran.BilledDate = null;
					newTran.NoteID = null;
					newTran.AllocationID = null;

					newTran.Description = pfLine.Description;
					newTran.UOM = pfLine.UOM;
					newTran.Qty = Math.Max(0, pfLine.BillableQty.GetValueOrDefault() - pfLine.Qty.GetValueOrDefault());
					newTran.BillableQty = newTran.Qty;

					newTran.IsNonGL = true;
					newTran.Released = false;
					newTran.Billed = false;
					newTran.Allocated = false;
					newTran.ExcludedFromBilling = false;
					newTran.ExcludedFromAllocation = true;
					newTran.Reverse = PMReverse.Never;

					newTran = Transactions.Insert(newTran);

					if (arDoc.CuryID == project.CuryID)
					{
						newTran.Amount = Math.Max(0, pfLine.BillableAmount.GetValueOrDefault() - pfLine.LineTotal.GetValueOrDefault());
						newTran.TranCuryAmount = Math.Max(0, pfLine.CuryBillableAmount.GetValueOrDefault() - pfLine.CuryLineTotal.GetValueOrDefault());
						newTran.ProjectCuryAmount = Math.Max(0, pfLine.CuryBillableAmount.GetValueOrDefault() - pfLine.CuryLineTotal.GetValueOrDefault());
					}
					else
					{
						newTran.Amount = Math.Max(0, pfLine.BillableAmount.GetValueOrDefault() - pfLine.LineTotal.GetValueOrDefault());
						decimal val;
						PXCurrencyAttribute.CuryConvCury<PMTran.projectCuryInfoID>(Transactions.Cache, newTran, newTran.Amount.GetValueOrDefault(), out val);
						newTran.TranCuryAmount = val;
						newTran.ProjectCuryAmount = val;
					}
					Transactions.Update(newTran);
					remainders.Add(newTran);
				}
			}
			return remainders;
		}

		public void ReverseRemainders(ARRegister arDoc, List<PMTran> trans)
		{
			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.UnbilledRemainderReversal;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.UnbilledRemainderReversal);

			var billEngine = PXGraph.CreateInstance<PMBillEngine>();
			foreach (PMTran tran in trans)
			{
				PMTran reversal = billEngine.ReverseTran(tran).First();
				Transactions.Insert(reversal);
			}
		}

		public virtual void ReverseAllocations(ARRegister arDoc, List<PMTran> trans)
		{
			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.AllocationReversal;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversalOnARInvoiceRelease);

			PMBillEngine billEngine = PXGraph.CreateInstance<PMBillEngine>();
			foreach (PMTran tran in trans)
			{
				foreach (PMTran reverse in billEngine.ReverseTran(tran))
				{
					reverse.Date = arDoc.DocDate;
					reverse.FinPeriodID = arDoc.FinPeriodID;
					Transactions.Insert(reverse);
				}
			}
		}

		protected void SuppressFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

        private void AddUsage(PXCache sender, PMTran tran, decimal? used, string UOM)
        {
			//Only project is handled here. Contracts are handled explicitly in UsageMaint.cs
            if (tran.ProjectID != null && tran.TaskID != null && tran.InventoryID != null && tran.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
            {
				RecurringItemEx targetItem = PXSelect<RecurringItemEx,
					Where<RecurringItemEx.projectID, Equal<Required<RecurringItemEx.projectID>>,
					And<RecurringItemEx.taskID, Equal<Required<RecurringItemEx.taskID>>,
					And<RecurringItemEx.inventoryID, Equal<Required<RecurringItemEx.inventoryID>>>>>>.Select(this, tran.ProjectID, tran.TaskID, tran.InventoryID);

				if (targetItem != null)
				{
					decimal inTargetUnit = used ?? 0;
					if (!string.IsNullOrEmpty(UOM))
					{
						inTargetUnit = INUnitAttribute.ConvertToBase(sender, tran.InventoryID, UOM, used ?? 0, INPrecision.QUANTITY);
					}

					PMRecurringItemAccum item = new PMRecurringItemAccum();
					item.ProjectID = tran.ProjectID;
					item.TaskID = tran.TaskID;
					item.InventoryID = tran.InventoryID;

					item = RecurringItems.Insert(item);
					item.Used += inTargetUnit;
					item.UsedTotal += inTargetUnit;
				}
			}
        }

		private void SubtractUsage(PXCache sender, PMTran tran, decimal? used, string UOM)
		{
			if (used != 0)
				AddUsage(sender, tran, -used, UOM);
		}

		private void AddAllocatedTotal(PMTran tran)
		{
			if (tran.OrigProjectID != null && tran.OrigTaskID != null && tran.OrigAccountGroupID != null)
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
				tat.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

				tat = AllocationTotals.Insert(tat);
				tat.Amount += tran.Amount;
				tat.Quantity += (tran.Billable == true && tran.UseBillableQty == true) ? tran.BillableQty : tran.Qty;
			}
		}

		private void SubtractAllocatedTotal(PMTran tran)
		{
			if (tran.OrigProjectID != null && tran.OrigTaskID != null && tran.OrigAccountGroupID != null && tran.InventoryID != null)
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
				tat.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

				tat = AllocationTotals.Insert(tat);
				tat.Amount -= tran.Amount;
				tat.Quantity -= (tran.Billable == true && tran.UseBillableQty == true) ? tran.BillableQty : tran.Qty;
			}
		}

		public virtual PMTran CreateTransaction(PMTimeActivity timeActivity, int? employeeID, DateTime date, int? timeSpent, int? timeBillable, decimal? cost, decimal? overtimeMult)
		{
			if (timeActivity.ApprovalStatus == ActivityStatusAttribute.Canceled)
				return null;

			if (timeSpent.GetValueOrDefault() == 0 && timeBillable.GetValueOrDefault() == 0)
				return null;
            
            bool postToOffBalance = false;
			EPSetup epsetup = PXSelect<EPSetup>.Select(this);
			if (epsetup != null)
				postToOffBalance = EPSetupMaint.GetPostingOption(this, epsetup, employeeID) == EPPostOptions.PostToOffBalance;
			PMSetup pmsetup = PXSelect<PMSetup>.Select(this);
			if (pmsetup == null || pmsetup.IsActive != true || !PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
				postToOffBalance = true;

			InventoryItem laborItem = PXSelect<InventoryItem, Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>>.Select(this, timeActivity.LabourItemID);
			if (laborItem == null)
			{
				PXTrace.WriteError(EP.Messages.InventoryItemIsEmpty);
				throw new PXException(EP.Messages.InventoryItemIsEmpty);
			}

			if (!postToOffBalance && laborItem.InvtAcctID == null)
			{
				PXTrace.WriteError(EP.Messages.ExpenseAccrualIsRequired, laborItem.InventoryCD.Trim());
				throw new PXException(EP.Messages.ExpenseAccrualIsRequired, laborItem.InventoryCD.Trim());
			}

			if (!postToOffBalance && laborItem.InvtSubID == null)
			{
				PXTrace.WriteError(EP.Messages.ExpenseAccrualSubIsRequired, laborItem.InventoryCD.Trim());
				throw new PXException(EP.Messages.ExpenseAccrualSubIsRequired, laborItem.InventoryCD.Trim());
			}

			string ActivityTimeUnit = EPSetup.Minute;
			EPSetup epSetup = PXSelect<EPSetup>.Select(this);
			if (!string.IsNullOrEmpty(epSetup.ActivityTimeUnit))
			{
				ActivityTimeUnit = epSetup.ActivityTimeUnit;
			}

			if (timeActivity.ProjectID == null)
			{
				throw new PXException(Messages.ProjectIdIsNotSpecifiedForActivity, timeActivity.NoteID, timeActivity.Summary);
			}

			Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, timeActivity.ProjectID);

			decimal qty = timeSpent.GetValueOrDefault();
			if (qty > 0 && epSetup.MinBillableTime > qty)
				qty = (decimal)epSetup.MinBillableTime;
			try
			{
				qty = INUnitAttribute.ConvertGlobalUnits(this, ActivityTimeUnit, laborItem.BaseUnit, qty, INPrecision.QUANTITY);
			}
			catch (PXException ex)
			{
				PXTrace.WriteError(ex);
				throw ex;
			}

			decimal bilQty = timeBillable.GetValueOrDefault();
			if (bilQty > 0 && epSetup.MinBillableTime > bilQty)
				bilQty = (decimal)epSetup.MinBillableTime;
			try
			{ 
				bilQty = INUnitAttribute.ConvertGlobalUnits(this, ActivityTimeUnit, laborItem.BaseUnit, bilQty, INPrecision.QUANTITY);
			}
			catch (PXException ex)
			{
				PXTrace.WriteError(ex);
				throw ex;
			}
			int? accountID = laborItem.COGSAcctID;
            int? offsetaccountID = laborItem.InvtAcctID;
			int? accountGroupID = null;
			string subCD = null;
            string offsetSubCD = null;

			int? branchID = null;
			string tranCuryID = Accessinfo.BaseCuryID;
			bool isNonGL = false;
			EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, employeeID);
			if (emp != null)
			{
				Branch branch = PXSelect<Branch, Where<Branch.bAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>.Select(this, emp.ParentBAccountID);
				if (branch != null)
				{
					branchID = branch.BranchID;
					tranCuryID = branch.BaseCuryID;
				}

				if(PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>())
				{
					isNonGL = PXSelect<PREmployee, Where<PREmployee.bAccountID, Equal<Required<PREmployee.bAccountID>>>>.Select(this, employeeID).Count > 0;
				}
			}

			if (contract.BaseType == CT.CTPRType.Project && contract.NonProject != true && !postToOffBalance)//contract do not record money only usage.
			{
				if (contract.NonProject != true)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, timeActivity.ProjectID, timeActivity.ProjectTaskID);
					if ( task == null )
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.FailedSelectProjectTask, timeActivity.ProjectID, timeActivity.ProjectTaskID));
					if (task.IsActive != true)
					{
						PXTrace.WriteWarning(EP.Messages.ProjectTaskIsNotActive, contract.ContractCD.Trim(), task.TaskCD.Trim());
					}
					if (task.IsCompleted == true)
					{
						PXTrace.WriteWarning(EP.Messages.ProjectTaskIsCompleted, contract.ContractCD.Trim(), task.TaskCD.Trim());
					}
					if (task.IsCancelled == true)
					{
						PXTrace.WriteWarning(EP.Messages.ProjectTaskIsCancelled, contract.ContractCD.Trim(), task.TaskCD.Trim());
					}

					#region Combine Account and Subaccount

					if (ExpenseAccountSource == PMAccountSource.Project)
					{
						if (contract.DefaultAccountID != null)
						{
							accountID = contract.DefaultAccountID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account == null)
							{
								throw new PXException(EP.Messages.ProjectsDefaultAccountNotFound, accountID);
							}
							if (account.AccountGroupID == null)
							{
								throw new PXException(EP.Messages.NoAccountGroupOnProject, account.AccountCD.Trim(), contract.ContractCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnProject, contract.ContractCD.Trim());
						}
					}
					else if (ExpenseAccountSource == PMAccountSource.Task)
					{

						if (task.DefaultAccountID != null)
						{
							accountID = task.DefaultAccountID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account == null)
							{
								throw new PXException(EP.Messages.ProjectTasksDefaultAccountNotFound, accountID);
							}
							if (account.AccountGroupID == null)
							{
								throw new PXException(EP.Messages.NoAccountGroupOnTask, account.AccountCD.Trim(), contract.ContractCD.Trim(), task.TaskCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
					}
					else if (ExpenseAccountSource == PMAccountSource.Employee)
					{
						if (emp.ExpenseAcctID != null)
						{
							accountID = emp.ExpenseAcctID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account == null)
							{
								throw new PXException(EP.Messages.EmployeeExpenseAccountNotFound, accountID);
							}
							if (account.AccountGroupID == null)
							{
								throw new PXException(EP.Messages.NoAccountGroupOnEmployee, account.AccountCD, emp.AcctCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoExpenseAccountOnEmployee, emp.AcctCD.Trim());
						}
					}
					else
					{
						if (accountID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
						}

						//defaults to InventoryItem.COGSAcctID
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
						if (account == null)
						{
							throw new PXException(EP.Messages.ItemCogsAccountNotFound, accountID);
						}
						if (account.AccountGroupID == null)
						{
							PXTrace.WriteError(EP.Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), laborItem.InventoryCD.Trim());
						}
						accountGroupID = account.AccountGroupID;
					}


					if (accountGroupID == null)
					{
						//defaults to InventoryItem.COGSAcctID
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
						if (account == null)
						{
							throw new PXException(EP.Messages.ItemCogsAccountNotFound, accountID);
						}
						if (account.AccountGroupID == null)
						{
							PXTrace.WriteError(EP.Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
							throw new PXException(EP.Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
						}
						accountGroupID = account.AccountGroupID;
					}


					if (!string.IsNullOrEmpty(ExpenseSubMask))
					{
						if (ExpenseSubMask.Contains(PMAccountSource.InventoryItem) && laborItem.COGSSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnInventory, laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnInventory, laborItem.InventoryCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Project) && contract.DefaultSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnProject, contract.ContractCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnProject, contract.ContractCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Task) && task.DefaultSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Employee) && emp.ExpenseSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}


						subCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseSubMask>(this, ExpenseSubMask,
							new object[] { laborItem.COGSSubID, contract.DefaultSubID, task.DefaultSubID, emp.ExpenseSubID },
							new Type[] { typeof(InventoryItem.cOGSSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(EPEmployee.expenseSubID) });
					}

					#endregion

                    #region Combine Accrual Account and Subaccount

                    if (ExpenseAccrualAccountSource == PMAccountSource.Project)
                    {
                        if (contract.DefaultAccrualAccountID != null)
                        {
                            offsetaccountID = contract.DefaultAccrualAccountID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(EP.Messages.NoDefualtAccrualAccountOnProject, contract.ContractCD.Trim());
                        }
                    }
                    else if (ExpenseAccrualAccountSource == PMAccountSource.Task)
                    {
                        if (task.DefaultAccrualAccountID != null)
                        {
                            offsetaccountID = task.DefaultAccrualAccountID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                    }
                    else
                    {
                        if (offsetaccountID == null)
                        {
							PXTrace.WriteError(EP.Messages.NoAccrualExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
                            throw new PXException(EP.Messages.NoAccrualExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(ExpenseAccrualSubMask))
                    {
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.InventoryItem) && laborItem.InvtSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnInventory, laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoExpenseAccrualSubOnInventory, laborItem.InventoryCD.Trim());
						}
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Project) && contract.DefaultAccrualSubID == null)
                        {
							PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnProject, contract.ContractCD.Trim());
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnProject, contract.ContractCD.Trim());
                        }
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.Task) && task.DefaultAccrualSubID == null)
                        {
							PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Employee) && emp.ExpenseSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}
						
						offsetSubCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseAccrualSubMask>(this, ExpenseAccrualSubMask,
							new object[] { laborItem.InvtSubID, contract.DefaultAccrualSubID, task.DefaultAccrualSubID, emp.ExpenseSubID },
							new Type[] { typeof(InventoryItem.invtSubID), typeof(Contract.defaultAccrualSubID), typeof(PMTask.defaultAccrualSubID), typeof(EPEmployee.expenseSubID) });
                    }

                    #endregion
				}
				else
				{
					//defaults to InventoryItem.COGSAcctID
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
					if (account == null)
					{
						throw new PXException(EP.Messages.ItemCogsAccountNotFound, accountID);
					}
					if (account.AccountGroupID == null)
					{
						throw new PXException(EP.Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), laborItem.InventoryCD.Trim());
					}
					accountGroupID = account.AccountGroupID;
				}
			}

            int? subID = null;
            int? offsetSubID = null;

			if (postToOffBalance)
            {
                accountGroupID = epsetup.OffBalanceAccountGroupID;
                accountID = null;
                offsetaccountID = null;
                offsetSubID = null;
				offsetSubCD = null;
                subCD = null;
                subID = null;
            }
			else
			{
				subID = laborItem.COGSSubID;
				offsetSubID = laborItem.InvtSubID;
			}
			
            //verify that the InventoryItem will be accessable/visible in the selector:
            PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, accountGroupID);
            if (accountGroup != null && accountGroup.Type == AccountType.Income && laborItem.SalesAcctID == null)
            {
                PXTrace.WriteWarning(EP.Messages.NoSalesAccountOnInventory, laborItem.InventoryCD.Trim());
            }
			EmployeeCostEngine costEngine = new EmployeeCostEngine(this);
			//Verify that Project will be accessable/visible in the selector:
			if (contract.IsActive != true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectIsNotActive, contract.ContractCD.Trim());
			}
			if (contract.IsCompleted == true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectIsCompleted, contract.ContractCD.Trim());
			}
            PMTran tran = (PMTran)Transactions.Insert( new PMTran() { ProjectID = timeActivity.ProjectID });
			tran.BranchID = branchID;
			tran.AccountID = accountID;
			if (string.IsNullOrEmpty(subCD))
				tran.SubID = subID;
            if (string.IsNullOrEmpty(offsetSubCD))
                tran.OffsetSubID = offsetSubID;
            if (contract.BaseType == CT.CTPRType.Contract)
		    {
		        tran.BAccountID = contract.CustomerID;
		        tran.LocationID = contract.LocationID;
		    }
		    tran.AccountGroupID = accountGroupID;
			tran.ProjectID = timeActivity.ProjectID;
			tran.TaskID = timeActivity.ProjectTaskID;
			tran.CostCodeID = timeActivity.CostCodeID;
			tran.InventoryID = timeActivity.LabourItemID;
			tran.UnionID = timeActivity.UnionID;
			tran.WorkCodeID = timeActivity.WorkCodeID;
			tran.ResourceID = employeeID;
			tran.Date = date;
			tran.TranCuryID = tranCuryID;
			
			FinPeriod finPeriod = FinPeriodRepository.FindFinPeriodByDate(tran.Date.Value, PXAccess.GetParentOrganizationID(branchID));
			if (finPeriod == null)
			{
				throw new PXException(Messages.FinPeriodForDateNotFound);
			}

			tran.FinPeriodID = finPeriod.FinPeriodID;
			tran.Qty = decimal.Round(qty, 2);
			tran.Billable = timeActivity.IsBillable;
			tran.BillableQty = decimal.Round(bilQty, 2);
			tran.UOM = laborItem.BaseUnit;
			tran.TranCuryUnitRate = PXDBPriceCostAttribute.Round(cost.GetValueOrDefault());
            tran.OffsetAccountID = offsetaccountID;
            tran.IsQtyOnly = contract.BaseType == CT.CTPRType.Contract;
			tran.Description = timeActivity.Summary;
			tran.StartDate = timeActivity.Date;
			tran.EndDate = timeActivity.Date;
			tran.OrigRefID = timeActivity.NoteID;
			tran.EarningType = timeActivity.EarningTypeID;
			tran.OvertimeMultiplier = overtimeMult;
			tran.IsNonGL = isNonGL;
			if (timeActivity.RefNoteID != null)
			{
				Note note = PXSelectJoin<Note, 
					InnerJoin<CRActivityLink, 
						On<CRActivityLink.refNoteID, Equal<Note.noteID>>>,
					Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(this, timeActivity.RefNoteID);
				if (note != null && note.EntityType == typeof(CRCase).FullName)
				{
					CRCase crCase = PXSelectJoin<CRCase,
						InnerJoin<CRActivityLink,
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>, 
						Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(this, timeActivity.RefNoteID);

					if (crCase != null && crCase.IsBillable != true)
					{
						//Case is not billable, do not mark the cost transactions as Billed. User may configure Project and use Project Billing for these transactions.
					}
					else
					{
						//Activity associated with the case will be billed (or is already billed) by the Case Billing procedure. 
						tran.ExcludedFromAllocation = true;
						tran.ExcludedFromBilling = true;
						tran.ExcludedFromBillingReason = PXMessages.LocalizeFormatNoPrefix(Messages.ExcludedFromBillingAsBillableWithCase, crCase.CaseCD);
					}
				}
			}

			try
			{
				tran = Transactions.Update(tran);
				tran.BillableQty = decimal.Round(bilQty, 2);
				tran = Transactions.Update(tran);
			}
			catch(PXFieldValueProcessingException ex)
			{
				if (ex.InnerException is PXTaskIsCompletedException)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, ((PXTaskIsCompletedException)ex.InnerException).TaskID);
					if (task != null)
					{
						PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, task.ProjectID);
						if (project != null)
						{
							throw new PXException(Messages.ProjectTaskIsCompletedDetailed, project.ContractCD.Trim(), task.TaskCD.Trim());
						}
					}
				}

				throw ex;
			}
			catch(PXException ex)
			{
				throw ex;
			}

			if (!string.IsNullOrEmpty(subCD))
				Transactions.SetValueExt<PMTran.subID>(tran, subCD);
            
            if (!string.IsNullOrEmpty(offsetSubCD))
                Transactions.SetValueExt<PMTran.offsetSubID>(tran, offsetSubCD);

			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(PMTimeActivity)], timeActivity, Transactions.Cache, tran, epSetup.GetCopyNoteSettings<PXModule.pm>());
			return tran;
		}

        public virtual PMTran CreateContractUsage(PMTimeActivity timeActivity, int billableMinutes)
        {
            if (timeActivity.ApprovalStatus == ActivityStatusAttribute.Canceled)
                return null;

            if (timeActivity.RefNoteID == null)
                return null;

			if (timeActivity.IsBillable != true)
				return null;

	        CRCase refCase = PXSelectJoin<CRCase,
		        InnerJoin<CRActivityLink,
			        On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
		        Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(this, timeActivity.RefNoteID);
            
            if (refCase == null)
                throw new Exception(CR.Messages.CaseCannotBeFound);

            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, refCase.CaseClassID);

			if (caseClass.PerItemBilling != BillingTypeListAttribute.PerActivity)
                return null;//contract-usage will be created as a result of case release.

            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, refCase.ContractID);
            if (contract == null)
                return null;//activity has no contract and will be billed through Project using the cost-transaction. Contract-Usage is not created in this case. 

            int? laborItemID = CRCaseClassLaborMatrix.GetLaborClassID(this, caseClass.CaseClassID, timeActivity.EarningTypeID);

            if (laborItemID == null)
                laborItemID = EP.EPContractRate.GetContractLaborClassID(this, timeActivity);

            if (laborItemID == null)
            {
                EP.EPEmployee employeeSettings = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.userID, Equal<Required<EP.EPEmployee.userID>>>>.Select(this, timeActivity.OwnerID);
                if (employeeSettings != null)
                {
                    laborItemID = EP.EPEmployeeClassLaborMatrix.GetLaborClassID(this, employeeSettings.BAccountID, timeActivity.EarningTypeID) ??
                                  employeeSettings.LabourItemID;
                }
            }

            InventoryItem laborItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, laborItemID);

            if (laborItem == null)
            {
                throw new PXException(CR.Messages.LaborNotConfigured);
            }

			//save the sign of the value and do the rounding against absolute value.
			//reuse sign later when setting value to resulting transaction.
	        int sign = billableMinutes < 0 ? -1 : 1; 
	        billableMinutes = Math.Abs(billableMinutes);
			
			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity && caseClass.RoundingInMinutes > 1)
            {
				decimal fraction = Convert.ToDecimal(billableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                int points = Convert.ToInt32(Math.Ceiling(fraction));
				billableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
            }

			if (billableMinutes > 0 && caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity && caseClass.MinBillTimeInMinutes > 0)
            {
				billableMinutes = Math.Max(billableMinutes, (int)caseClass.MinBillTimeInMinutes);
            }
			
            if (billableMinutes > 0)
            {
				PMTran newLabourTran = new PMTran();
                newLabourTran.ProjectID = refCase.ContractID;
                newLabourTran.InventoryID = laborItem.InventoryID;
                newLabourTran.AccountGroupID = contract.ContractAccountGroup;
                newLabourTran.OrigRefID = timeActivity.NoteID;
                newLabourTran.BAccountID = refCase.CustomerID;
                newLabourTran.LocationID = refCase.LocationID;
                newLabourTran.Description = timeActivity.Summary;
                newLabourTran.StartDate = timeActivity.Date;
                newLabourTran.EndDate = timeActivity.Date;
                newLabourTran.Date = timeActivity.Date;
                newLabourTran.UOM = laborItem.SalesUnit;
                newLabourTran.Qty = sign * Convert.ToDecimal(TimeSpan.FromMinutes(billableMinutes).TotalHours);
                newLabourTran.BillableQty = newLabourTran.Qty;
                newLabourTran.Released = true;
                newLabourTran.ExcludedFromAllocation = true;
                newLabourTran.IsQtyOnly = true;
                newLabourTran.BillingID = contract.BillingID;
				newLabourTran.CaseCD = refCase.CaseCD;
                return this.Transactions.Insert(newLabourTran);
            }
            else
            {
                return null;
            }
        }

		public override void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers)
		{
			//move useBillableQty to prevent overriding amount by formula
			var useBillableQty = script.Where(_ => _.FieldName == nameof(PMTran.UseBillableQty)).SingleOrDefault();
			var tranCuryAmountIndex = script.FindIndex(_ => _.FieldName == nameof(PMTran.TranCuryAmount));
			if (useBillableQty != null && tranCuryAmountIndex >= 0)
			{
				script.Remove(useBillableQty);
				script.Insert(tranCuryAmountIndex, useBillableQty);
			}
		}

		[PXBreakInheritance]
        [Serializable]
        [PXHidden]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class RecurringItemEx : PMRecurringItem
		{
			#region ProjectID
			public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			[PXDBInt(IsKey = true)]
			public override Int32? ProjectID
			{
				get;
				set;
			}
			#endregion
			#region TaskID
			public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

			[PXDBInt(IsKey = true)]
			public override Int32? TaskID
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			[PXDBInt(IsKey = true)]
			public override Int32? InventoryID
			{
				get;
				set;
			}
			#endregion
		}
	}
}
