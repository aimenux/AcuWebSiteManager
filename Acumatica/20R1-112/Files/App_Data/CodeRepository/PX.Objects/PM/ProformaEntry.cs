using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;
using PX.Data.BQL.Fluent;
using System.Diagnostics;
using PX.Data.BQL;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	[Serializable]
	public class ProformaEntry : PXGraph<ProformaEntry, PMProforma>, IGraphWithInitialization
	{
		public const string ProformaInvoiceReport = "PM642000";
		public const string ProformaNotificationCD = "PROFORMA";

		#region DAC Overrides

		[PXDefault(BAccountType.CustomerType)]
		protected virtual void BAccountR_Type_CacheAttached(PXCache sender)
		{
		}

		[PXParent(typeof(Select<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMBillingRecord.proformaRefNbr>>>>))]
		[PXDBString(PMProforma.refNbr.Length, IsUnicode = true)]
		protected virtual void PMBillingRecord_ProformaRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Actual Amount", Enabled = false, Visible = false)]
		protected virtual void PMRevenueBudget_CuryActualAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Draft Invoices Amount", Enabled = false, Visible = false)]
		protected virtual void PMRevenueBudget_CuryInvoicedAmount_CacheAttached(PXCache sender) { }

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMProforma.invoiceDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(PMProforma.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMProforma.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}
		
		
		[PXDBLong]
		[CurrencyInfo(typeof(PMProforma.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMProforma.curyDocTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMProforma.docTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region PMTran
		[Account(null, typeof(Search2<Account.accountID,
			LeftJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Current<PMTran.accountGroupID>>>>,
			Where<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>, And<Account.accountGroupID, Equal<Current<PMTran.accountGroupID>>,
			Or<PMAccountGroup.type, Equal<PMAccountType.offBalance>,
			Or<PMAccountGroup.groupID, IsNull>>>>>), DisplayName = "Debit Account", Visible = false)]
		protected virtual void PMTran_AccountID_CacheAttached(PXCache sender) { }

		[SubAccount(typeof(PMTran.accountID), DisplayName = "Debit Subaccount", Visible = false)]
		protected virtual void PMTran_SubID_CacheAttached(PXCache sender) { }

		[Account(DisplayName = "Credit Account", Visible = false)]
		protected virtual void PMTran_OffsetAccountID_CacheAttached(PXCache sender) { }

		[SubAccount(typeof(PMTran.offsetAccountID), DisplayName = "Credit Subaccount", Visible = false)]
		protected virtual void PMTran_OffsetSubID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Amount")]
		protected virtual void PMTran_ProjectCuryAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Currency", FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		protected virtual void PMTran_ProjectCuryID_CacheAttached(PXCache sender) { }
		#endregion

		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(PMProforma.taxZoneID))]
		[PXUIFieldAttribute(DisplayName = "Customer Tax Zone", Enabled = false)]
		protected virtual void PMTaxTran_TaxZoneID_CacheAttached(PXCache sender) { }

		#endregion

		[PXViewName(Messages.Proforma)]
		public PXSelect<PMProforma> Document;
		public PXFilter<PMProformaOverflow> Overflow;
		public PXSelect<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMProforma.refNbr>>>> DocumentSettings;
		public ProgressLineSelect ProgressiveLines;
		public TransactLineSelect TransactionLines;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		public virtual IEnumerable transactionLines()
		{
			var select = new PXOrderedSelect<PMProforma, PMProformaTransactLine,
			Where<PMProformaTransactLine.refNbr, Equal<Current<PMProforma.refNbr>>,
			And<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>>>,
			OrderBy<Asc<PMProformaTransactLine.sortOrder, Asc<PMProformaTransactLine.lineNbr>>>>(this);

			if (!IsLimitsEnabled())
			{
				return select.Select();
			}
			else
			{
				decimal overflowTotal = 0;
				decimal overflowTotalInBase = 0;
				List<PMProformaTransactLine> result = new List<PMProformaTransactLine>();

				var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);
				var revenueBudget = selectRevenueBudget.Select(Project.Current.ContractID);

				Dictionary<BudgetKeyTuple, Tuple<decimal, decimal>> maxLimits = new Dictionary<BudgetKeyTuple, Tuple<decimal, decimal>>();
				Dictionary<BudgetKeyTuple, decimal> currentDocumentAmounts = new Dictionary<BudgetKeyTuple, decimal>();
				var resultset = select.Select();
				foreach (PMProformaTransactLine line in resultset)
				{
					if (line.IsPrepayment == true)
						continue;
					
					BudgetKeyTuple key = new BudgetKeyTuple(line.ProjectID.GetValueOrDefault(), line.TaskID.GetValueOrDefault(), GetProjectedAccountGroup(line).GetValueOrDefault(), 
						Project.Current.BudgetLevel == BudgetLevels.Item ? line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) : PMInventorySelectorAttribute.EmptyInventoryID,
						Project.Current.BudgetLevel == BudgetLevels.Task ? CostCodeAttribute.GetDefaultCostCode() : line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
					decimal invoicedAmount;
					if (currentDocumentAmounts.TryGetValue(key, out invoicedAmount))
					{
						currentDocumentAmounts[key] = invoicedAmount + GetLineTotalInProjectCurrency(line);
					}
					else
					{
						currentDocumentAmounts[key] = GetLineTotalInProjectCurrency(line);
					}
				}

				foreach (PMRevenueBudget budget in revenueBudget)
				{
					if (budget.LimitAmount != true)
						continue;

					BudgetKeyTuple key = BudgetKeyTuple.Create(budget);

					//Total invoiced amount including sum of all transactions in current document.
					//invoicedAmountIncludingCurrentDocument is in ProjectCurrency (i.e. = Base if MC is OFF; = Doc.CuryID if MC is ON)
					decimal invoicedAmountIncludingCurrentDocument = budget.CuryActualAmount.GetValueOrDefault() + budget.CuryInvoicedAmount.GetValueOrDefault() + CalculatePendingInvoicedAmount(budget.ProjectID, budget.ProjectTaskID, budget.AccountGroupID, budget.InventoryID, budget.CostCodeID);
					decimal invoicedAmountCurrentDoc = 0;
					currentDocumentAmounts.TryGetValue(key, out invoicedAmountCurrentDoc);

					decimal previouslyInvoicedTotal = invoicedAmountIncludingCurrentDocument - invoicedAmountCurrentDoc;
					
					Tuple<decimal, decimal> bucket = new Tuple<decimal, decimal>(budget.CuryMaxAmount.GetValueOrDefault(), Math.Max(0, budget.CuryMaxAmount.GetValueOrDefault() - previouslyInvoicedTotal));
					maxLimits.Add(key, bucket);
				}

				Dictionary<int, decimal> negativeAdjustmentPool = new Dictionary<int, decimal>();
				foreach (PMProformaTransactLine line in resultset)
				{
					if (line.IsPrepayment == true)
						continue;

					BudgetKeyTuple key = new BudgetKeyTuple(line.ProjectID.GetValueOrDefault(), line.TaskID.GetValueOrDefault(), GetProjectedAccountGroup(line).GetValueOrDefault(),
						(Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail) ? line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) : PMInventorySelectorAttribute.EmptyInventoryID,
						Project.Current.BudgetLevel == BudgetLevels.Task ? CostCodeAttribute.GetDefaultCostCode() : line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));

					Tuple<decimal, decimal> bucket;
					if (maxLimits.TryGetValue(key, out bucket))
					{
						line.MaxAmount = bucket.Item1;
						line.CuryMaxAmount = !UseBaseCurrency ? line.MaxAmount : ConvertToDocumentCurrency(line.MaxAmount);

						decimal adj = 0;
						negativeAdjustmentPool.TryGetValue(line.TaskID.Value, out adj);//In DocCury if MS; otherwise in Base

						line.CuryAvailableAmount = Math.Max(0, !UseBaseCurrency ? bucket.Item2 + adj : ConvertToDocumentCurrency(bucket.Item2 + adj));

						if (GetLineTotalInProjectCurrency(line) > 0)
						{
							decimal remainder = line.CuryAvailableAmount.GetValueOrDefault() - line.CuryLineTotal.GetValueOrDefault();
							decimal remainderInBase = line.AvailableAmount.GetValueOrDefault() - line.LineTotal.GetValueOrDefault();
							if (remainder >= adj)
							{
								line.CuryOverflowAmount = 0;
								line.OverflowAmount = 0;
								maxLimits[key] = new Tuple<decimal, decimal>(bucket.Item1, remainder - adj);
							}
							else
							{
                                line.CuryOverflowAmount = remainder > 0 ? 0 : -remainder;
								line.OverflowAmount = remainderInBase > 0 ? 0 : -remainderInBase;
								maxLimits[key] = new Tuple<decimal, decimal>(bucket.Item1, remainder - adj);
								overflowTotal += line.CuryOverflowAmount.Value;
								overflowTotalInBase += line.OverflowAmount.Value;
							}
						}
						else
						{
							if (negativeAdjustmentPool.ContainsKey(line.TaskID.Value))
							{

								negativeAdjustmentPool[line.TaskID.Value] += !UseBaseCurrency ? -(line.CuryLineTotal.GetValueOrDefault()) : -(line.LineTotal.GetValueOrDefault());
							}
							else
							{
								negativeAdjustmentPool[line.TaskID.Value] = !UseBaseCurrency ? -(line.CuryLineTotal.GetValueOrDefault()) : -(line.LineTotal.GetValueOrDefault());
							}
						}
					}

					result.Add(line);
				}

				Overflow.Current.CuryOverflowTotal = overflowTotal;
				Overflow.Current.OverflowTotal = overflowTotalInBase;
				Overflow.View.RequestRefresh();

				return result;
			}
		}

		public PXSelect<PMTran, Where<PMTran.proformaRefNbr, Equal<Current<PMProformaTransactLine.refNbr>>, And<PMTran.proformaLineNbr, Equal<Current<PMProformaTransactLine.lineNbr>>>>> Details;
       
        public PXSelect<PMTran, Where<PMTran.proformaRefNbr, Equal<Current<PMProformaTransactLine.refNbr>>, And<PMTran.proformaLineNbr, Equal<Current<PMProformaTransactLine.lineNbr>>>>> ReferencedTransactions;
		public PXSelect<PMTran, Where<PMTran.proformaRefNbr, Equal<Current<PMProformaTransactLine.refNbr>>>> AllReferencedTransactions;

		public PXSelect<PMTran> Unbilled;
		public virtual IEnumerable unbilled()
		{
			PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(engine, Document.Current.ProjectID);
			List<PMTran> billingBase = new List<PMTran>();
			if (project != null)
			{
				List<PMTask> tasks = engine.SelectBillableTasks(project);

				DateTime cuttoffDate = Document.Current.InvoiceDate.Value.AddDays(engine.IncludeTodaysTransactions ? 1 : 0);
				engine.PreSelectTasksTransactions(Document.Current.ProjectID, tasks, cuttoffDate); //billingRules dictionary also filled.

				
				foreach (PMTask task in tasks)
				{
					List<PMBillingRule> rulesList;
					if (engine.billingRules.TryGetValue(task.BillingID, out rulesList))
					{
						foreach (PMBillingRule rule in rulesList)
						{
							if (rule.Type == PMBillingType.Transaction)
							{
								billingBase.AddRange(engine.SelectBillingBase(task.ProjectID, task.TaskID, rule.AccountGroupID, rule.IncludeNonBillable == true));
							}
						}
					}
				}

				HashSet<long> unbilledBase = new HashSet<long>();
				foreach (PMTran tran in billingBase)
				{
					unbilledBase.Add(tran.TranID.Value);
					PMTran located = Unbilled.Locate(tran);
					if (located != null && (located.Billed == true || located.ExcludedFromBilling == true))
					{
						tran.Selected = true;
					}
				}

				foreach (PMTran tran in Unbilled.Cache.Updated)
				{
					if (tran.Billed != true && tran.ExcludedFromBilling != true && !unbilledBase.Contains(tran.TranID.Value))
					{
						billingBase.Add(tran);
					}
				}
			}
			return billingBase;
		}

		public PXSelect<PMBudgetAccum> Budget;
		public PXSelect<PMBillingRecord> BillingRecord;
		[PXViewName(PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMProforma.projectID.FromCurrent>> Project;
		[PXViewName(AR.Messages.Customer)]
		public PXSetup<Customer>.Where<Customer.bAccountID.IsEqual<PMProforma.customerID.AsOptional>> Customer;
		public PXSetup<Location>.Where<Location.bAccountID.IsEqual<PMProforma.customerID.FromCurrent>.And<Location.locationID.IsEqual<PMProforma.locationID.AsOptional>>> Location;

		public ToggleCurrency<PMProforma> CurrencyView;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PMProforma.curyInfoID>>>> currencyinfo;

		[PXViewName(Messages.Approval)]
		public EPApprovalList<PMProforma, PMProforma.approved, PMProforma.rejected> Approval;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMProforma.customerID>>>>))]
		[PMDefaultMailTo(typeof(Select2<Contact,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<Contact.bAccountID>, And<Customer.defContactID, Equal<Contact.contactID>>>>,
			Where<Customer.bAccountID, Equal<Current<PMProforma.customerID>>>>))]
		public PMActivityList<PMProforma> Activity;

		public PXSelect<PMAddress, Where<PMAddress.addressID, Equal<Current<PMProforma.billAddressID>>>> Billing_Address;
		public PXSelect<PMContact, Where<PMContact.contactID, Equal<Current<PMProforma.billContactID>>>> Billing_Contact;

		[PXViewName(Messages.PMAddress)]
		public PXSelect<PMShippingAddress, Where<PMShippingAddress.addressID, Equal<Current<PMProforma.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.PMContact)]
		public PXSelect<PMShippingContact, Where<PMShippingContact.contactID, Equal<Current<PMProforma.shipContactID>>>> Shipping_Contact;

		[PXCopyPasteHiddenView]
		public PXSelect<PMTax, Where<PMTax.refNbr, Equal<Current<PMProforma.refNbr>>>, OrderBy<Asc<PMTax.refNbr, Asc<PMTax.taxID>>>> Tax_Rows;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<PMTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<PMTaxTran.taxID>>>,
				Where<PMTaxTran.refNbr, Equal<Current<PMProforma.refNbr>>>> Taxes;

		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> Company;
		public PXSetup<ARSetup> ARSetup;
		public PXSetup<TaxZone>.Where<TaxZone.taxZoneID.IsEqual<PMProforma.taxZoneID.FromCurrent>> taxzone;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMUnbilledDailySummaryAccum> UnbilledSummary;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMRevenueBudget> dummyRevenueBudget; //for cache attached to rename fields for joinned table in ProgressiveLines view.

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<BAccountR> dummyAccountR;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;
		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}

		public Dictionary<int, List<PMTran>> cachedReferencedTransactions;

		public bool SuppressRowSeleted
		{
			get;
			set;
		}

		public ProformaEntry()
		{
			this.CopyPaste.SetVisible(false);
			InitalizeActionsMenu();
			InitializeReportsMenu();

			var pmAddressCache = Caches[typeof(PMAddress)];
			var pmContactCache = Caches[typeof(PMContact)];
			var pmShippingAddressCache = Caches[typeof(PMShippingAddress)];
			var pmShippingContactCache = Caches[typeof(PMShippingContact)];
		}

		protected virtual void BeforeCommitHandler(PXGraph e)
		{
			var check = _licenseLimits.GetCheckerDelegate<PMProforma>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(PMProformaLine), (graph) =>
			{
				return new PXDataFieldValue[]
				{
							new PXDataFieldValue<PMProformaLine.refNbr>(((ProformaEntry)graph).Document.Current?.RefNbr)

				};
			}));

			try
			{
				check.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseProgressBillingAndTimeAndMaterial);
			}

		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += BeforeCommitHandler;

			}
		}

		#region Actions
		public PXAction<PMProforma> release;
		[PXUIField(DisplayName = GL.Messages.Release)]
		[PXProcessButton]
		public IEnumerable Release(PXAdapter adapter)
		{
			RecalculateExternalTaxesSync = true;
			this.Save.Press();

			PXLongOperation.StartOperation(this, delegate () {

				ProformaEntry pe = PXGraph.CreateInstance<ProformaEntry>();
				pe.Document.Current = Document.Current;
				pe.RecalculateExternalTaxesSync = true;
				pe.ReleaseDocument(Document.Current);

			});
			return adapter.Get();
		}

		public PXAction<PMProforma> approve;
		[PXUIField(DisplayName = "Approve")]
		[PXButton]
		public IEnumerable Approve(PXAdapter adapter)
		{
			if (IsDirty)
			Save.Press();

			foreach (PMProforma item in adapter.Get<PMProforma>())
			{
				try
				{

				if (item.Approved == true)
					continue;

				if (Setup.Current.ProformaApprovalMapID != null)
				{
					if (!Approval.Approve(item))
						throw new PXSetPropertyException(Common.Messages.NotApprover);
					item.Approved = Approval.IsApproved(item);
					if (item.Approved == true)
						item.Status = ProformaStatus.Open;
				}
				else
				{
					item.Approved = true;
					item.Status = ProformaStatus.Open;
				}

				Document.Update(item);

				Save.Press();
				}
				catch (ReasonRejectedException) { }

				yield return item;
			}
		}

		public PXAction<PMProforma> reject;
		[PXUIField(DisplayName = "Reject")]
		[PXButton]
		public IEnumerable Reject(PXAdapter adapter)
		{
			if (IsDirty)
			Save.Press();

			foreach (PMProforma item in adapter.Get<PMProforma>())
			{
				try
				{
				if (item.Approved == true || item.Rejected == true)
					continue;

				if (Setup.Current.ProformaApprovalMapID != null)
				{
					if (!Approval.Reject(item))
						throw new PXSetPropertyException(Common.Messages.NotApprover);
					item.Rejected = true;
				}
				else
				{
					item.Rejected = true;
				}

				item.Status = ProformaStatus.Rejected;
				Document.Update(item);

				Save.Press();
				}
				catch (ReasonRejectedException) { }

				yield return item;
			}
		}

		public PXAction<PMProforma> action;
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PMProforma> report;
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PMProforma> proformaReport;
		[PXUIField(DisplayName = "Print Pro Forma Invoice", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable ProformaReport(PXAdapter adapter)
		{
			OpenReport(ProformaInvoiceReport, Document.Current);

			return adapter.Get();
		}

		public virtual void OpenReport(string reportID, PMProforma doc)
		{
			if (doc != null)
			{
				var utility = new CR.NotificationUtility(this);
				string specificReportID = utility.SearchReport(PMNotificationSource.Project, Project.Current, reportID, doc.BranchID);
				
				var parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = doc.RefNbr;
				throw new PXReportRequiredException(parameters, specificReportID, specificReportID);
			}
		}

		public PXAction<PMProforma> send;
		[PXUIField(DisplayName = "Email Pro Forma Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Send(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				PXLongOperation.StartOperation(this, delegate () {
					ProformaEntry pe = PXGraph.CreateInstance<ProformaEntry>();
					pe.Document.Current = Document.Current;
					pe.SendReport(ProformaNotificationCD, Document.Current);
				});
			}

			return adapter.Get();
		}

		public virtual void SendReport(string notificationCD, PMProforma doc)
		{
			if (doc != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["RefNbr"] = Document.Current.RefNbr;
				
				using (var ts = new PXTransactionScope())
				{
					Activity.SendNotification(PMNotificationSource.Project, notificationCD, doc.BranchID, mailParams);
					this.Save.Press();

					ts.Complete();
				}
			}
		}
				
		public PXAction<PMProforma> autoApplyPrepayments;
		[PXUIField(DisplayName = "Apply Available Prepaid Amounts")]
		[PXProcessButton]
		public IEnumerable AutoApplyPrepayments(PXAdapter adapter)
		{
			ApplyPrepayment(Document.Current);

			yield return Document.Current;
		}

		public PXAction<PMProforma> viewTranDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTranDocument(PXAdapter adapter)
		{
			RegisterEntry graph = CreateInstance<RegisterEntry>();
			graph.Document.Current = graph.Document.Search<PMRegister.refNbr>(Details.Current.RefNbr, Details.Current.TranType);
			throw new PXRedirectRequiredException(graph, "PMTransactions") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMProforma> uploadUnbilled;
		[PXUIField(DisplayName = "Upload Unbilled Transactions")]
		[PXButton]
		public IEnumerable UploadUnbilled(PXAdapter adapter)
		{			
			if(Unbilled.View.AskExt() == WebDialogResult.OK)
			{
				AppendUnbilled();
			}

			return adapter.Get();
		}

		public PXAction<PMProforma> appendSelected;
		[PXUIField(DisplayName = "Upload")]
		[PXButton]
		public IEnumerable AppendSelected(PXAdapter adapter)
		{
			AppendUnbilled();

			return adapter.Get();
		}

		public PXAction<PMProforma> viewProgressLineTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProgressLineTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMProformaProgressLine.taskID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMProforma> viewTransactLineTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTransactLineTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMProformaTransactLine.taskID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMProforma> viewProgressLineInventory;
		[PXUIField(DisplayName = "View Inventory Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProgressLineInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMProformaProgressLine.inventoryID>>>>.Select(this);
			if (item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMProforma> viewTransactLineInventory;
		[PXUIField(DisplayName = "View Inventory Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTransactLineInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMProformaTransactLine.inventoryID>>>>.Select(this);
			if (item != null && item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMProforma> viewVendor;
		[PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<PMProformaTransactLine.vendorID>>>>.Select(this);

			if (vendor != null)
			{
				VendorMaint graph = CreateInstance<VendorMaint>();
				graph.BAccount.Current = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<PMProformaTransactLine.vendorID>>>>.Select(this);
				throw new PXRedirectRequiredException(graph, true, "View Vendor") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void _(Events.RowInserted<PMProforma> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<ARInvoice.curyInfoID>(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<PMProforma> e)
		{
			foreach (PMTran tran in AllReferencedTransactions.View.SelectMultiBound(new[] { e.Row }).RowCast<PMTran>())
			{
				tran.Billed = false;
				tran.Reversed = false;
				tran.BilledDate = null;
				tran.BillingID = null;
				tran.ProformaRefNbr = null;
				tran.ProformaLineNbr = null;

				PX.Objects.PM.RegisterReleaseProcess.AddToUnbilledSummary(this, tran);
				Details.Update(tran);
			}
		}

		protected virtual void _(Events.RowInserted<PMProformaTransactLine> e)
		{
			Document.Cache.SetValue<PMProforma.enableTransactional>(Document.Current, true);

			AddToInvoiced(e.Row);
			SubtractPerpaymentRemainder(e.Row);
		}
				
		protected virtual void _(Events.RowInserted<PMProformaProgressLine> e)
		{
			Document.Cache.SetValue<PMProforma.enableProgressive>(Document.Current, true);
			AddToInvoiced(e.Row);
			SubtractPerpaymentRemainder(e.Row);
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.curyPrepaidAmount> e)
		{
			if (e.Row != null)
			{
				decimal? newAmount = (decimal?)e.NewValue;

				if (newAmount != null && e.Row.CuryPrepaidAmount > 0 && e.Row.CuryPrepaidAmount < newAmount)
				{
					e.NewValue = e.Row.CuryPrepaidAmount;
					e.Cache.RaiseExceptionHandling<PMProformaTransactLine.curyPrepaidAmount>(e.Row, e.Row.CuryPrepaidAmount, new PXSetPropertyException<PMProformaTransactLine.curyPrepaidAmount>(Messages.PrepaidAmountDecreased, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaProgressLine, PMProformaProgressLine.curyPrepaidAmount> e)
		{
			if (e.Row != null)
			{
				decimal? newAmount = (decimal?)e.NewValue;

				if (newAmount != null && e.Row.CuryPrepaidAmount > 0 && e.Row.CuryPrepaidAmount < newAmount)
				{
					e.NewValue = e.Row.CuryPrepaidAmount;
					e.Cache.RaiseExceptionHandling<PMProformaProgressLine.curyPrepaidAmount>(e.Row, e.Row.CuryPrepaidAmount, new PXSetPropertyException<PMProformaProgressLine.curyPrepaidAmount>(Messages.PrepaidAmountDecreased, PXErrorLevel.Warning));
				}
			}
		}

		
		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.curyLineTotal> e)
		{
			if (e.Row.CuryLineTotal + e.Row.CuryPrepaidAmount < e.Row.CuryBillableAmount)
			{
				if (e.Row.Option == PMProformaLine.option.BillNow && !IsAdjustment(e.Row))
				{
					e.Cache.SetValue<PMProformaTransactLine.option>(e.Row, null);
					e.Cache.SetValuePending<PMProformaTransactLine.option>(e.Row, null);
				}
			}
		}
				
		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.completedPct> e)
		{
			PMRevenueBudget budget = SelectRevenueBudget(e.Row);
			if (budget != null)
			{
				decimal pendingInvoiceAmount = !UseBaseCurrency ? CalculatePendingInvoicedAmount(e.Row) : ConvertToDocumentCurrency(CalculatePendingInvoicedAmount(e.Row));
				decimal billableAmount = budget.CuryRevisedAmount.GetValueOrDefault() * e.Row.CompletedPct.GetValueOrDefault() / 100m;
				decimal invoicedAmount = budget.CuryActualAmount.GetValueOrDefault() + budget.CuryInvoicedAmount.GetValueOrDefault() + pendingInvoiceAmount - e.Row.CuryLineTotal.GetValueOrDefault();
				decimal unbilledAmount = Math.Max(0, billableAmount - invoicedAmount);
				
				ProgressiveLines.SetValueExt<PMProformaProgressLine.curyAmount>(e.Row, unbilledAmount - e.Row.CuryMaterialStoredAmount.GetValueOrDefault());
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.curyRetainage> e)
		{
			if (e.Row.CuryLineTotal > 0)
				e.Row.RetainagePct = Math.Min(100m, 100m * e.Row.CuryRetainage.GetValueOrDefault() / e.Row.CuryLineTotal.GetValueOrDefault());
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.retainagePct> e)
		{
			PMProject proj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, e.Row.ProjectID);
			if (proj != null)
			{
				e.NewValue = proj.RetainagePct;
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaProgressLine, PMProformaProgressLine.retainagePct> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal percent = (decimal)e.NewValue;
				if (percent < 0 || percent > 100)
				{
					throw new PXSetPropertyException<PMProformaProgressLine.retainagePct>(IN.Messages.PercentageValueShouldBeBetween0And100);
				}

			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaProgressLine, PMProformaProgressLine.curyRetainage> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal val = (decimal)e.NewValue;
				if (val > e.Row.CuryLineTotal)
				{
					e.NewValue = e.Row.CuryLineTotal;
				}
				if (val <= 0)
				{
					e.NewValue = 0m;
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.curyRetainage> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal val = (decimal)e.NewValue;
				if (val > e.Row.CuryLineTotal)
				{
					e.NewValue = e.Row.CuryLineTotal;
				}
				if (val <= 0)
				{
					e.NewValue = 0m;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.curyRetainage> e)
		{
			if (e.Row.CuryLineTotal > 0)
				e.Row.RetainagePct = Math.Min(100m, 100m * e.Row.CuryRetainage.GetValueOrDefault() / e.Row.CuryLineTotal.Value);
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.retainagePct> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal percent = (decimal)e.NewValue;
				if (percent < 0 || percent > 100)
				{
					throw new PXSetPropertyException<PMProformaTransactLine.retainagePct>(IN.Messages.PercentageValueShouldBeBetween0And100);
				}

			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.currentInvoicedPct> e)
		{
			PMRevenueBudget budget = SelectRevenueBudget(e.Row);
			if (budget != null)
			{
				decimal unbilledAmount = budget.CuryRevisedAmount.GetValueOrDefault() * e.Row.CurrentInvoicedPct.GetValueOrDefault() / 100m;
				decimal amt = !UseBaseCurrency ? unbilledAmount : ConvertToDocumentCurrency(unbilledAmount);
				ProgressiveLines.SetValueExt<PMProformaProgressLine.curyAmount>(e.Row, amt - e.Row.CuryMaterialStoredAmount.GetValueOrDefault());
			}
		}

		protected virtual void _(Events.FieldSelecting<PMProformaProgressLine, PMProformaProgressLine.completedPct> e)
		{
			if (e.Row != null)
			{
				PMRevenueBudget budget = SelectRevenueBudget(e.Row);
				if (budget != null)
				{
					decimal timeAndMaterialTotal = 0;

					foreach(PMProformaTransactLine line in TransactionLines.Select())
					{
						if (line.TaskID == e.Row.TaskID && 
							line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) == e.Row.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) &&
							line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()) == e.Row.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()) &&
							GetProjectedAccountGroup(line) == e.Row.AccountGroupID)
						{
							timeAndMaterialTotal += GetLineTotalInProjectCurrency(line);
						}
					}

					decimal previouslyInvoiced = GetPreviouslyInvoicedAmount(e.Row);
					decimal invoicedAmount = previouslyInvoiced + GetLineTotalInProjectCurrency(e.Row) + timeAndMaterialTotal;
					decimal result = 0;
					if (budget.CuryRevisedAmount.GetValueOrDefault() != 0)
						result = 100m * invoicedAmount / budget.CuryRevisedAmount.Value;

					result = Math.Round(result, PMProformaProgressLine.completedPct.Precision);
	 
					PXFieldState fieldState = PXDecimalState.CreateInstance(result, PMProformaProgressLine.completedPct.Precision, nameof(PMProformaProgressLine.CompletedPct), false, 0, Decimal.MinValue, Decimal.MaxValue);
					e.ReturnState = fieldState;
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PMProformaProgressLine, PMProformaProgressLine.currentInvoicedPct> e)
		{
			if (e.Row != null)
			{
				PMRevenueBudget budget = SelectRevenueBudget(e.Row);
				if (budget != null)
				{
					decimal invoicedAmount = GetLineTotalInProjectCurrency(e.Row);
					decimal result = 0;
					
					if (budget.CuryRevisedAmount.GetValueOrDefault() != 0)
					{
						result = Math.Round(100m * invoicedAmount / budget.CuryRevisedAmount.Value, PMProformaProgressLine.completedPct.Precision);
					}
					PXFieldState fieldState = PXDecimalState.CreateInstance(result, PMProformaProgressLine.completedPct.Precision, nameof(PMProformaProgressLine.CurrentInvoicedPct), false, 0, Decimal.MinValue, Decimal.MaxValue);
					e.ReturnState = fieldState;
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PMProformaTransactLine, PMProformaTransactLine.option> e)
		{
			if (e.Row != null)
			{
				string status = (string)e.ReturnValue;

				KeyValuePair<List<string>, List<string>> result = new KeyValuePair<List<string>, List<string>>(new List<string>(), new List<string>());

				if (status == PMProformaLine.option.Writeoff || status == PMProformaLine.option.BillNow)
				{
					result.Key.Add(PMProformaLine.option.BillNow);
					result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_BillNow));
					result.Key.Add(PMProformaLine.option.Writeoff);
					result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_Writeoff));
				}
				else
				{
					if (e.Row.CuryLineTotal + e.Row.CuryPrepaidAmount < e.Row.CuryBillableAmount && e.Row.CuryLineTotal >= 0)
					{
						result.Key.Add(PMProformaLine.option.HoldRemainder);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_HoldRemainder));
						result.Key.Add(PMProformaLine.option.WriteOffRemainder);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_WriteOffRemainder));
						result.Key.Add(PMProformaLine.option.Writeoff);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_Writeoff));
					}
					else
					{
						result.Key.Add(PMProformaLine.option.BillNow);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_BillNow));
						result.Key.Add(PMProformaLine.option.Writeoff);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_Writeoff));
					}
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, typeof(PMProformaTransactLine.option).Name, false, 1, null, result.Key.ToArray(), result.Value.ToArray(), true, null);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.option> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				if (e.NewValue.ToString() == PMProformaLine.option.HoldRemainder)
				{
					Dictionary<int, List<PMTran>> pmtranByProformalLineNbr = GetReferencedTransactions();
					List<PMTran> list;
					if (pmtranByProformalLineNbr.TryGetValue(e.Row.LineNbr.Value, out list))
					{
						bool containAllocation = false;
						foreach (PMTran item in list)
						{
							containAllocation = !string.IsNullOrEmpty(item.AllocationID);

							if (containAllocation)
								break;
						}

						if (containAllocation && list.Count > 1)
						{
							throw new PXSetPropertyException(Messages.GroupedAllocationsBillLater);
						}
					}

				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.option> e)
		{
			if (e.Row.Option == PMProformaLine.option.Writeoff)
			{
				e.Cache.SetValueExt<PMProformaTransactLine.curyPrepaidAmount>(e.Row, 0m);
				e.Cache.SetValueExt<PMProformaTransactLine.curyLineTotal>(e.Row, 0m);
				e.Cache.SetValueExt<PMProformaTransactLine.qty>(e.Row, 0m);
			}
			else if (e.Row.Option == PMProformaLine.option.BillNow && GetLineTotalInProjectCurrency(e.Row) >= 0)
			{
				e.Cache.SetValueExt<PMProformaTransactLine.curyLineTotal>(e.Row, e.Row.CuryBillableAmount);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.accountID> e)
		{
			if (e.Row != null && (e.Row.InventoryID == null || e.Row.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID) && Location.Current != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(TransactionLines.Cache, e.Row, Location.Current.CSalesAcctID) as Account;
				if (revenueAccount != null && revenueAccount.AccountGroupID != null)
				{
					e.NewValue = Location.Current.CSalesAcctID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.inventoryID> e)
		{
			if (e.Row != null && e.Row.InventoryID == null && IsAdjustment(e.Row) )
			{
				e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.taxCategoryID> e)
		{
			if (Project.Current != null)
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMProformaTransactLine.inventoryID>(e.Cache, e.Row);
					if (item != null && item.TaxCategoryID != null)
					{
						e.NewValue = item.TaxCategoryID;
					}
				}

				if (e.NewValue == null)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, e.Row.TaskID);
					if (task != null && task.TaxCategoryID != null)
					{
						e.NewValue = task.TaxCategoryID;
					}
				}
			}
		}
		
		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.taskID> e)
		{
			if (IsAdjustment(e.Row))
			{
				PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, e.NewValue);
				if (task != null)
				{
					if (task.IsCompleted == true)
					{
						var ex = new PXTaskIsCompletedException(task.TaskID);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
				}
			}
		}


		protected virtual void _(Events.RowDeleted<PMProformaProgressLine> e)
		{
			SubtractFromInvoiced(e.Row);
			SubtractPerpaymentRemainder(e.Row, -1);

			if (e.Row.IsPrepayment != true)
				SubtractAmountToInvoice(e.Row, -e.Row.CuryBillableAmount, -e.Row.BillableAmount); //Restoring AmountToInvoice

			bool documentDeleted = false;
			PMProforma parent = (PMProforma)PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(PMProforma));
			if (parent != null && Document.Cache.GetStatus(parent) == PXEntryStatus.Deleted)
			{
				documentDeleted = true;
			}

			if (!documentDeleted && !RecalculatingContractRetainage)
			{
				RecalculateRetainageOnDocument(Project.Current);
			}
		}

		private ISet<int> GetUserBranches()
		{
			return new HashSet<int>(PXAccess
				.GetBranches(Accessinfo.UserName)
				.Select(branch => branch.Id));
		}

		protected virtual void OnProformaDeleting(Events.RowDeleting<PMProforma> e)
		{
			ISet<int> userBranches = GetUserBranches();

			using (new PXReadBranchRestrictedScope())
			{
				var lineBranches = new HashSet<int>(
					SelectFrom<PMProformaLine>
						.Where<PMProformaLine.refNbr.IsEqual<PMProforma.refNbr.FromCurrent>>
						.View
						.Select(this)
						.Select(line => ((PMProformaLine)line).BranchID)
						.Where(branchID => branchID.HasValue)
						.Select(branchID => branchID.Value));

				if (!userBranches.IsSupersetOf(lineBranches))
				{
					throw new PXOperationCompletedSingleErrorException(Messages.ProformaDeletingRestriction);
				}
			}
		}		

		protected virtual void OnProformaLineDeleting(Events.RowDeleting<PMProformaTransactLine> e)
		{
			ISet<int> userBranches = GetUserBranches();

			using (new PXReadBranchRestrictedScope())
			{
				PMProformaTransactLine proformaLine = e.Row;
				var transactionBranches = new HashSet<int>(
					SelectFrom<PMTran>
						.Where
							<PMTran.proformaRefNbr.IsEqual<@P.AsString>.And
							<PMTran.proformaLineNbr.IsEqual<@P.AsInt>>>
						.View
						.Select(this, proformaLine.RefNbr, proformaLine.LineNbr)
						.Select(line => ((PMTran)line).BranchID)
						.Where(branchID => branchID.HasValue)
						.Select(branchID => branchID.Value));

				if (!userBranches.IsSupersetOf(transactionBranches))
				{
					throw new PXOperationCompletedSingleErrorException(Messages.ProformaLineDeletingRestriction);
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMProformaTransactLine> e)
		{
			PMProforma parent = (PMProforma) PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(PMProforma));

			bool referencesAlreadyDeleted = false; //for the entire document.

			if (parent != null && Document.Cache.GetStatus(parent) == PXEntryStatus.Deleted)
			{
				referencesAlreadyDeleted = true;
			}

			if (!referencesAlreadyDeleted)
			{
			Dictionary<int, List<PMTran>> dict = GetReferencedTransactions();

			List<PMTran> list;
			if (dict.TryGetValue(e.Row.LineNbr.Value, out list))
			{
				foreach (PMTran tran in list)
				{
					tran.Billed = false;
					tran.BilledDate = null;
					tran.BillingID = null;
					tran.ProformaRefNbr = null;
					tran.ProformaLineNbr = null;
					tran.Selected = false;

					PM.RegisterReleaseProcess.AddToUnbilledSummary(this, tran);
					Details.Update(tran);
				}
				cachedReferencedTransactions = null;
			}
			}

			SubtractFromInvoiced(e.Row);
			SubtractPerpaymentRemainder(e.Row, -1);
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryID> e)
		{
			if (Project.Current != null)
				e.ReturnValue = Project.Current.CuryID;
		}

		protected virtual void _(Events.RowDeleted<PMTran> e)
		{
			PMProformaTransactLine key = new PMProformaTransactLine();
			key.RefNbr = e.Row.ProformaRefNbr;
			key.LineNbr = e.Row.ProformaLineNbr;

			PMProformaTransactLine line = TransactionLines.Locate(key);

			if (line != null)
			{
				if (e.Row.ProjectCuryID == Document.Current.CuryID)
				{
					line.CuryBillableAmount -= e.Row.ProjectCuryInvoicedAmount.GetValueOrDefault();
					line.CuryAmount -= e.Row.ProjectCuryInvoicedAmount.GetValueOrDefault();
				}
				else
				{
					line.CuryBillableAmount -= ConvertToDocumentCurrency(e.Row.InvoicedAmount.GetValueOrDefault());
					line.CuryAmount -= ConvertToDocumentCurrency(e.Row.InvoicedAmount.GetValueOrDefault());
				}
				
				line.BillableQty -= e.Row.InvoicedQty.GetValueOrDefault();
				line.Qty -= e.Row.InvoicedQty.GetValueOrDefault();

				TransactionLines.Update(line);
			}

			e.Cache.SetStatus(e.Row, PXEntryStatus.Updated);

			e.Row.Billed = false;
			e.Row.BilledDate = null;
			e.Row.BillingID = null;
			e.Row.ProformaRefNbr = null;
			e.Row.ProformaLineNbr = null;
		}

		#region CurrencyInfo events
		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (Project.Current != null && !string.IsNullOrEmpty(Project.Current.BillingCuryID))
				{
					e.NewValue = Project.Current.BillingCuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyRateTypeID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (Project.Current != null && !string.IsNullOrEmpty(Project.Current.RateTypeID))
				{
					e.NewValue = Project.Current.RateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyEffDate> e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((PMProforma)Document.Cache.Current).InvoiceDate;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowSelected<CurrencyInfo> e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (e.Row != null)
			{
				bool curyenabled = info.AllowUpdate(this.TransactionLines.Cache);

				if (Customer.Current != null && !(bool)Customer.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(e.Cache, e.Row, curyenabled);
			}
		}

		protected virtual void _(Events.RowUpdated<CurrencyInfo> e)
		{
			Action<PXCache, PMProformaLine> syncBudgets = (cache, tran) =>
			{
				decimal newLineTotal = 0;
				decimal newPrepaidAmount = 0;
				if (e.Row.CuryRate != null)
				{
					PXDBCurrencyAttribute.CuryConvBase(e.Cache, e.Row, tran.CuryLineTotal.GetValueOrDefault(), out newLineTotal);
					PXDBCurrencyAttribute.CuryConvBase(e.Cache, e.Row, tran.CuryPrepaidAmount.GetValueOrDefault(), out newPrepaidAmount);
				}
				var newTran = cache.CreateCopy(tran) as PMProformaLine;
				newTran.LineTotal = newLineTotal;
				newTran.PrepaidAmount = newPrepaidAmount;

				decimal oldLineTotal = 0;
				decimal oldPrepaidAmount = 0;
				if (e.OldRow.CuryRate != null)
				{
					PXDBCurrencyAttribute.CuryConvBase(e.Cache, e.OldRow, tran.CuryLineTotal.GetValueOrDefault(), out oldLineTotal);
					PXDBCurrencyAttribute.CuryConvBase(e.Cache, e.OldRow, tran.CuryPrepaidAmount.GetValueOrDefault(), out oldPrepaidAmount);
				}
				var oldTran = cache.CreateCopy(tran) as PMProformaLine;
				oldTran.LineTotal = oldLineTotal;
				oldTran.PrepaidAmount = oldPrepaidAmount;

				SyncBudgets(cache, newTran, oldTran);
			};

			foreach (PMProformaLine tran in TransactionLines.Select())
				syncBudgets(TransactionLines.Cache, tran);

			foreach (PMProformaLine tran in ProgressiveLines.Select())
				syncBudgets(ProgressiveLines.Cache, tran);
		}

		#endregion

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.taskID> e)
		{
			e.Cache.SetDefaultExt<PMProformaTransactLine.taxCategoryID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMProformaTransactLine.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMProformaTransactLine.taxCategoryID>(e.Row);
			e.Cache.SetDefaultExt<PMProformaTransactLine.accountID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.locationID> e)
		{
			e.Cache.SetDefaultExt<PMProforma.branchID>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.taxZoneID>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.workgroupID>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.ownerID>(e.Row);

			ARShippingAddressAttribute.DefaultRecord<PMProforma.shipAddressID>(e.Cache, e.Row);
			ARShippingContactAttribute.DefaultRecord<PMProforma.shipContactID>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.hold> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Hold == true)
				{
					Approval.Reset(e.Row);
					e.Cache.SetValue<PMProforma.approved>(e.Row, false);
					e.Cache.SetValue<PMProforma.rejected>(e.Row, false);
					e.Cache.SetValue<PMProforma.status>(e.Row, ProformaStatus.OnHold);
				}
				else
				{
					if (Setup.Current.ProformaApprovalMapID != null)
					{

						Approval.Assign(e.Row, Setup.Current.ProformaApprovalMapID, Setup.Current.ProformaApprovalNotificationID);
						if ( true == (bool?)e.Cache.GetValue<PMProforma.approved>(e.Row) )
						{
							e.Cache.SetValue<PMProforma.status>(e.Row, ProformaStatus.Open);
						}
						else
						{
							e.Cache.SetValue<PMProforma.status>(e.Row, ProformaStatus.PendingApproval);
						}
					}
					else
					{
						e.Cache.SetValue<PMProforma.approved>(e.Row, true);
						e.Cache.SetValue<PMProforma.status>(e.Row, ProformaStatus.Open);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProforma, PMProforma.curyID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BillingCuryID != (string) e.NewValue)
				{
					PXSetPropertyException ex = new PXSetPropertyException(Messages.BillingCurrencyCannotBeChanged, Project.Current.BillingCuryID);
					ex.ErrorValue = e.NewValue;

					throw ex;
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PMProforma> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Hold != true && (bool?)Document.Cache.GetValueOriginal<PMProforma.hold>(e.Row) == true)
				{
					ValidateLimitsOnUnhold(e.Row);
					ValidateRetainageOnUnhold(e.Row);
					ValidateInclusiveTaxesOnUnhold(e.Row);
				}
				if (e.Operation != PXDBOperation.Delete)
				{
					if (e.Row.FinPeriodID == null)
					{
						e.Cache.RaiseExceptionHandling<PMProforma.finPeriodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMProforma.finPeriodID).Name));
					}
			}
		}
		}

		protected virtual void _(Events.FieldDefaulting<PMProforma, PMProforma.taxZoneID> e)
		{
			if (e.Row != null)
			{
				Location customerLocation = Location.View.SelectSingleBound(new object[] { e.Row } ) as Location;
				if (customerLocation != null)
				{
					if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
					{
						e.NewValue = customerLocation.CTaxZoneID;
					}
					else
					{
						BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, e.Row.BranchID);
						if (companyAccount != null)
						{
							Location companyLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
							if (companyLocation != null)
								e.NewValue = companyLocation.VTaxZoneID;
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMProforma> e)
		{
			if (SuppressRowSeleted)
				return;

			if (Project.Current != null)
			{
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.costCodeID>(ProgressiveLines.Cache, null, Project.Current.BudgetLevel == BudgetLevels.CostCode);
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, !CostCodeAttribute.UseCostCode() && Project.Current.BudgetLevel != BudgetLevels.Task);
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.retainagePct>(ProgressiveLines.Cache, null, Project.Current.RetainageMode != RetainageModes.Contract);
				PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.retainagePct>(ProgressiveLines.Cache, null, Project.Current.RetainageMode != RetainageModes.Contract ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.curyRetainage>(ProgressiveLines.Cache, null, Project.Current.RetainageMode != RetainageModes.Contract);

				PXUIFieldAttribute.SetVisible<PMProformaLine.curyAllocatedRetainedAmount>(ProgressiveLines.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract);
				PXUIFieldAttribute.SetVisibility<PMProformaLine.curyAllocatedRetainedAmount>(ProgressiveLines.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

				PXUIFieldAttribute.SetVisible<PMProforma.curyAllocatedRetainedTotal>(e.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract);
				PXUIFieldAttribute.SetVisible<PMProforma.retainagePct>(e.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract);

			}
			else
			{
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, !CostCodeAttribute.UseCostCode());
			}

			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyMaxAmount>(TransactionLines.Cache, null, IsLimitsEnabled());
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyAvailableAmount>(TransactionLines.Cache, null, IsLimitsEnabled());
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyOverflowAmount>(TransactionLines.Cache, null, IsLimitsEnabled());
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyMaxAmount>(ProgressiveLines.Cache, null, IsLimitsEnabled() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyAvailableAmount>(ProgressiveLines.Cache, null, IsLimitsEnabled() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyOverflowAmount>(ProgressiveLines.Cache, null, IsLimitsEnabled() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			
			PXUIFieldAttribute.SetVisible<PMProforma.curyID>(e.Cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, !CostCodeAttribute.UseCostCode() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.curyPrepaidAmount>(ProgressiveLines.Cache, null, Project.Current?.PrepaymentEnabled == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyPrepaidAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<PMProformaProgressLine.curyPrepaidAmount>(ProgressiveLines.Cache, null, Project.Current?.PrepaymentEnabled == true);
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyPrepaidAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true);
			PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.curyAmount>(ProgressiveLines.Cache, null, Project.Current?.PrepaymentEnabled == true || CostCodeAttribute.UseCostCode() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<PMProformaProgressLine.curyAmount>(ProgressiveLines.Cache, null, Project.Current?.PrepaymentEnabled == true || CostCodeAttribute.UseCostCode());
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true);
			PXUIFieldAttribute.SetVisible<PMProforma.avalaraCustomerUsageType>(e.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>());

			autoApplyPrepayments.SetEnabled(Project.Current?.PrepaymentEnabled == true);

			if (e.Row != null)
			{
				approve.SetEnabled(e.Row.Status == ProformaStatus.PendingApproval);
				reject.SetEnabled(e.Row.Status == ProformaStatus.PendingApproval);
				release.SetEnabled(e.Row.Approved == true && e.Row.Released != true);
				uploadUnbilled.SetEnabled(e.Row.Hold == true);

				bool isEditable = CanEditDocument(e.Row);

				Document.Cache.AllowDelete = isEditable;
				ProgressiveLines.Cache.AllowUpdate = e.Row.Hold == true;
				ProgressiveLines.Cache.AllowDelete = e.Row.Hold == true;
				TransactionLines.Cache.AllowInsert = e.Row.Hold == true;
				TransactionLines.Cache.AllowUpdate = e.Row.Hold == true;
				TransactionLines.Cache.AllowDelete = e.Row.Hold == true;
				Details.Cache.AllowDelete = isEditable && e.Row.Hold == true;
				Billing_Address.Cache.AllowUpdate = isEditable;
				Billing_Contact.Cache.AllowUpdate = isEditable;
				Shipping_Address.Cache.AllowUpdate = isEditable;
				Shipping_Contact.Cache.AllowUpdate = isEditable;

				PXUIFieldAttribute.SetEnabled<PMProforma.hold>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMProforma.invoiceDate>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.finPeriodID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.description>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.curyID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.branchID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.taxZoneID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.termsID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.dueDate>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.discDate>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.avalaraCustomerUsageType>(e.Cache, e.Row, isEditable);

				PXUIFieldAttribute.SetEnabled<PMProforma.locationID>(e.Cache, e.Row, isEditable && e.Row.Hold == true);
			}
		}

		

		public virtual bool CanEditDocument(PMProforma doc)
		{
			if (doc == null)
				return true;

			if (doc.Released == true)
				return false;

			if (doc.Hold == true)
			{
				return true;
			}
			else
			{
				if (doc.Rejected == true)
					return false;

				if (doc.Approved == true)
				{
					//document is either approved or no approval is not required.
					if (Setup.Current.ProformaApprovalMapID != null)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				else 
				{
					return false;
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMProformaOverflow> e)
		{
			PXUIFieldAttribute.SetVisible<PMProformaOverflow.curyOverflowTotal>(e.Cache, null, IsLimitsEnabled());

			if (e.Row.CuryOverflowTotal > 0)
			{
				PXUIFieldAttribute.SetWarning<PMProformaOverflow.curyOverflowTotal>(e.Cache, e.Row, Messages.OverlimitHint);
			}
			else
			{
				PXUIFieldAttribute.SetError<PMProformaOverflow.curyOverflowTotal>(e.Cache, e.Row, null);
			}
		}

		protected virtual void _(Events.RowSelected<PMProformaProgressLine> e)
		{
			if (e.Row != null)
			{
				
				e.Row.CuryPreviouslyInvoiced = !UseBaseCurrency ? GetPreviouslyInvoicedAmount(e.Row) : ConvertToDocumentCurrency(GetPreviouslyInvoicedAmount(e.Row));
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.curyPrepaidAmount>(e.Cache, e.Row, IsPrepaidAmountEnabled(e.Row));
			}
		}

		protected virtual void _(Events.RowSelected<PMProformaTransactLine> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.curyPrepaidAmount>(e.Cache, e.Row, IsPrepaidAmountEnabled(e.Row));
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.option>(e.Cache, e.Row, e.Row.IsPrepayment != true);
				Details.AllowSelect = e.Row.IsPrepayment != true;

				bool adjustment = IsAdjustment(e.Row);

				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.taskID>(e.Cache, e.Row, adjustment);
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.inventoryID>(e.Cache, e.Row, adjustment);
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.option>(e.Cache, e.Row, !adjustment);

				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.curyLineTotal>(e.Cache, e.Row, e.Row.Option != PMProformaLine.option.Writeoff);
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.qty>(e.Cache, e.Row, e.Row.Option != PMProformaLine.option.Writeoff);

			}
		}

		protected virtual void _(Events.RowUpdated<PMProformaTransactLine> e)
		{
			SyncBudgets(e.Cache, e.Row, e.OldRow);
			
			if (e.Row.CuryMaxAmount != null && GetLineTotalInProjectCurrency(e.Row) != GetLineTotalInProjectCurrency(e.OldRow))
			{
				TransactionLines.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowUpdated<PMProformaProgressLine> e)
		{
			SyncBudgets(e.Cache, e.Row, e.OldRow);

			if (!RecalculatingContractRetainage && e.Row.CuryLineTotal != e.OldRow.CuryLineTotal)
				RecalculateRetainageOnDocument(Project.Current);
		}

		private void SyncBudgets(PXCache cache, PMProformaLine row, PMProformaLine oldRow)
		{
			Account revenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(cache, row, row.AccountID) as Account;
			Account oldRevenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(cache, oldRow, oldRow.AccountID) as Account;

			if (oldRevenueAccount != null)
			{
				SubtractFromInvoiced(oldRow, oldRevenueAccount.AccountGroupID);
			}

			if (revenueAccount != null)
			{
				AddToInvoiced(row, revenueAccount.AccountGroupID);
			}

			if (row.CuryPrepaidAmount != oldRow.CuryPrepaidAmount || row.PrepaidAmount != oldRow.PrepaidAmount)
			{
				SubtractPerpaymentRemainder(oldRow, -1);
				SubtractPerpaymentRemainder(row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.branchID> e)
		{
			e.Cache.SetDefaultExt<PMProforma.taxZoneID>(e.Row);

			foreach (PMTaxTran taxTran in Taxes.Select())
			{
				if (Taxes.Cache.GetStatus(taxTran) == PXEntryStatus.Notchanged)
				{
					Taxes.Cache.SetStatus(taxTran, PXEntryStatus.Updated);
				}
			}
		}
		
		#region PMTaxTran Events
		protected virtual void _(Events.FieldDefaulting<PMTaxTran, PMTaxTran.taxZoneID> e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowSelected<PMTaxTran> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxID>(e.Cache, e.Row, e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}

		protected virtual void _(Events.RowInserting<PMTaxTran> e)
		{
			PXParentAttribute.SetParent(e.Cache, e.Row, typeof(PMProforma), this.Document.Current);
		}

		protected virtual void _(Events.ExceptionHandling<PMTaxTran, PMTaxTran.taxZoneID> e)
		{
			Exception ex = e.Exception as PXSetPropertyException;
			if (ex != null)
			{
				Document.Cache.RaiseExceptionHandling<PMProforma.taxZoneID>(Document.Current, null, ex);
			}
		}
		#endregion

		protected virtual void _(Events.RowPersisted<PMBudgetAccum> e)
		{
			//to fix discrepancy on transactions persisting
			this.Caches[typeof(PMRevenueBudget)].Clear();
		}

		protected virtual void PMProforma_InvoiceDate_FieldUpdated(Events.FieldUpdated<PMProforma.invoiceDate> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMProforma.finPeriodID>(e.Row);
			}
		}

		protected virtual void PMProforma_BranchID_FieldUpdated(Events.FieldUpdated<PMProforma.branchID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMProforma.finPeriodID>(e.Row);
			}
		}

		#endregion

		

		public override void Clear()
		{
			cachedReferencedTransactions = null;
			base.Clear();
		}
		
		protected virtual void ValidateLimitsOnUnhold(PMProforma row)
		{
			if (Overflow.Current.CuryOverflowTotal > 0 && Setup.Current.OverLimitErrorLevel == OverLimitValidationOption.Error)
			{
				throw new PXRowPersistingException(typeof(PMProformaOverflow.overflowTotal).Name, null, Messages.Overlimit);
			}
		}

		protected virtual void ValidateInclusiveTaxesOnUnhold(PMProforma row)
		{
			foreach (PXResult<PMTaxTran, Tax> res in Taxes.Select())
			{
				Tax tax = (Tax) res;
				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
				{
					Document.Cache.RaiseExceptionHandling<PMProforma.curyTaxTotal>(row, row.CuryTaxTotal, new PXSetPropertyException(Messages.InclusiveTaxNotSupported));
					throw new PXRowPersistingException(typeof(PMProforma.curyTaxTotal).Name, null, Messages.InclusiveTaxNotSupported);
				}
			}
		}

		protected virtual void ValidateRetainageOnUnhold(PMProforma row)
		{
			if (row.DocTotal < 0m && row.RetainageTotal != 0m)
			{
				Document.Cache.RaiseExceptionHandling<PMProforma.curyRetainageTotal>(row, row.CuryRetainageTotal, new PXSetPropertyException(Messages.RetainageNotApplicable));
				throw new PXRowPersistingException(typeof(PMProforma.curyRetainageTotal).Name, null, Messages.RetainageNotApplicable);
			}
		}

		public virtual void InitalizeActionsMenu()
		{
			action.MenuAutoOpen = true;
			action.AddMenuAction(approve);
			action.AddMenuAction(reject);
			action.AddMenuAction(send);
			//action.AddMenuAction(autoApplyPrepayments);
		}

		public virtual void InitializeReportsMenu()
		{
			report.MenuAutoOpen = true;
			report.AddMenuAction(proformaReport);
		}

		public virtual void ReleaseDocument(PMProforma doc)
		{
			if (doc == null)
				throw new ArgumentNullException();

			if (doc.Released == true)
				throw new PXException(EP.Messages.AlreadyReleased);
									
			CheckMigrationMode();

			ValidatePrecedingBeforeRelease(doc);
			ValidatePrecedingInvoicesBeforeRelease(doc);

			PMProject project = (PMProject)Project.View.SelectSingleBound(new object[] { doc });
			PMRegister reversalDoc = null;
			ARInvoice invoice = null;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				RegisterEntry pmEntry = PXGraph.CreateInstance<RegisterEntry>();
				pmEntry.Clear();
				pmEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				pmEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				
				reversalDoc = (PMRegister)pmEntry.Document.Cache.Insert();
				reversalDoc.OrigDocType = PMOrigDocType.AllocationReversal;
				reversalDoc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversalOnARInvoiceGeneration);
				pmEntry.Document.Current = reversalDoc;

				PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();

				ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
				invoiceEntry.Clear();
				invoiceEntry.ARSetup.Current.RequireControlTotal = false;
				invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				invoiceEntry.FieldVerifying.AddHandler<ARTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				
				invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
				invoiceEntry.RowPersisted.AddHandler<ARInvoice>(delegate (PXCache sender, PXRowPersistedEventArgs e)
				{
					if (e.TranStatus == PXTranStatus.Open)
					{
						var row = (ARInvoice)e.Row;
						doc.ARInvoiceDocType = row.DocType;
						doc.ARInvoiceRefNbr = row.RefNbr;
						reversalDoc.OrigNoteID = row.NoteID;
					}
				});

				int mult = doc.DocTotal >= 0 ? 1 : -1;

				invoice.DocType = mult == 1 ? ARDocType.Invoice : ARDocType.CreditMemo;
				invoice.CustomerID = doc.CustomerID;
				invoice.DocDate = doc.InvoiceDate;
				invoice.DocDesc = doc.Description;
				invoice.DueDate = doc.DueDate;
				invoice.DiscDate = doc.DiscDate;
				invoice.TermsID = doc.TermsID;
				invoice.TaxZoneID = doc.TaxZoneID;
				invoice.CuryID = doc.CuryID;
				invoice.CuryInfoID = doc.CuryInfoID;
				invoice.CustomerLocationID = doc.LocationID;
				invoice.ProjectID = doc.ProjectID;
				invoice.FinPeriodID = doc.FinPeriodID;
				invoice.BranchID = doc.BranchID;
				invoice.ProformaExists = true;
				invoice.RetainageApply = doc.RetainageTotal > 0m;
				invoice.InvoiceNbr = doc.RefNbr;
				if (Project.Current.RetainageMode != RetainageModes.Normal)
				{
					invoice.PaymentsByLinesAllowed = true;
				}

				invoice = invoiceEntry.Document.Insert(invoice);
				if (!string.IsNullOrEmpty(doc.FinPeriodID))
				{
					invoiceEntry.Document.Cache.SetValue<ARInvoice.finPeriodID>(invoice, doc.FinPeriodID);
				}

				invoiceEntry.currencyinfo.Current = invoiceEntry.currencyinfo.Select();

				PMAddress billAddressPM = (PMAddress)PXSelect<PMAddress, Where<PMAddress.addressID, Equal<Required<PMProforma.billAddressID>>>>.Select(this, doc.BillAddressID);
				if (billAddressPM != null && billAddressPM.IsDefaultAddress != true)
				{
					ARAddress addressAR = invoiceEntry.Billing_Address.Current;
					CopyPMAddressToARInvoice(invoiceEntry, billAddressPM, addressAR);
				}

				PMShippingAddress shipAddressPM = (PMShippingAddress)PXSelect<PMShippingAddress, 
					Where<PMShippingAddress.addressID, Equal<Required<PMProforma.shipAddressID>>>>.Select(this, doc.ShipAddressID);
				if (shipAddressPM != null && shipAddressPM.IsDefaultAddress != true)
				{
					ARShippingAddress shipAddressAR = invoiceEntry.Shipping_Address.Current;
					CopyPMAddressToARInvoice(invoiceEntry, shipAddressPM, shipAddressAR);
				}

				PMContact billContactPM = (PMContact)PXSelect<PMContact, Where<PMContact.contactID, Equal<Required<PMProforma.billContactID>>>>.Select(this, doc.BillContactID);
				if (billContactPM != null && billContactPM.IsDefaultContact != true)
				{
					ARContact contactAR = invoiceEntry.Billing_Contact.Current;
					CopyPMContactToARInvoice(invoiceEntry, billContactPM, contactAR);
				}

				PMShippingContact shipContactPM = (PMShippingContact)PXSelect<PMShippingContact, 
					Where<PMShippingContact.contactID, Equal<Required<PMProforma.shipContactID>>>>.Select(this, doc.ShipContactID);
				if (shipContactPM != null && shipContactPM.IsDefaultContact != true)
				{
					ARShippingContact shipContactAR = invoiceEntry.Shipping_Contact.Current;
					CopyPMContactToARInvoice(invoiceEntry, shipContactPM, shipContactAR);
				}

				if (string.IsNullOrEmpty(doc.TaxZoneID))
				{
					TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(invoiceEntry.Transactions.Cache, null, PX.Objects.TX.TaxCalc.NoCalc);
				}
				else
				{
					if (!RecalculateTaxesOnRelease())
						TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(invoiceEntry.Transactions.Cache, null, PX.Objects.TX.TaxCalc.ManualCalc);
				}


				ARTran artran = null;
				List<PMProformaProgressLine> processedProgressiveLines = new List<PMProformaProgressLine>();
				foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.View.SelectMultiBound(new[] { doc }))
				{
					PMProformaProgressLine line = (PMProformaProgressLine)res;
					artran = InsertTransaction(invoiceEntry, line, mult);
					PXNoteAttribute.CopyNoteAndFiles(ProgressiveLines.Cache, line, invoiceEntry.Transactions.Cache, artran);
					line.CuryPreviouslyInvoiced = !UseBaseCurrency ? GetPreviouslyInvoicedAmount(line) : ConvertToDocumentCurrency(GetPreviouslyInvoicedAmount(line));
					line.ARInvoiceLineNbr = artran.LineNbr;
					ProgressiveLines.Update(line);
					processedProgressiveLines.Add(line);					
				}

				Dictionary<int, List<PMTran>> pmtranByProformalLineNbr = GetReferencedTransactions();
				
				List<PMProformaTransactLine> processedTransactionLines = new List<PMProformaTransactLine>();
				foreach (PMProformaTransactLine line in TransactionLines.View.SelectMultiBound(new[] { doc }).RowCast<PMProformaTransactLine>())
				{
					if (line.Option != PMProformaLine.option.Writeoff)
					{
						artran = InsertTransaction(invoiceEntry, line, mult);
						PXNoteAttribute.CopyNoteAndFiles(TransactionLines.Cache, line, invoiceEntry.Transactions.Cache, artran);

						TransactionLines.Cache.SetValue<PMProformaTransactLine.aRInvoiceLineNbr>(line, artran.LineNbr);
						TransactionLines.Cache.MarkUpdated(line);
						processedTransactionLines.Add(line);
					}
					else
					{
						List<PMTran> list;
						if (pmtranByProformalLineNbr.TryGetValue(line.LineNbr.Value, out list))
						{
							foreach (PMTran tran in list)
							{
								if (string.IsNullOrEmpty(tran.AllocationID))
								{
									//direct cost transaction
									PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, tran);
									AllReferencedTransactions.Update(tran);
								}
							}
						}
					}

					List<PMTran> list2;
					if (pmtranByProformalLineNbr.TryGetValue(line.LineNbr.Value, out list2))
					{
						foreach (PMTran original in list2)
						{
							if (original != null && original.Reverse == PMReverse.OnInvoiceGeneration)
							{
								foreach (PMTran tran in engine.ReverseTran(original))
								{
									tran.Date = doc.InvoiceDate;
									tran.FinPeriodID = null;
									pmEntry.Transactions.Insert(tran);
								}
							}
						}
					}
				}

				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(invoiceEntry.Transactions.Cache, null, PX.Objects.TX.TaxCalc.ManualCalc);
				if (artran != null)
					invoiceEntry.Transactions.Cache.RaiseRowUpdated(artran, artran);

				if (!RecalculateTaxesOnRelease() && !IsExternalTax(Document.Current.TaxZoneID))
				{
					List<Tuple<ARTaxTran, PMTaxTran>> manualTaxes = new List<Tuple<ARTaxTran, PMTaxTran>>();
					foreach (PMTaxTran tax in Taxes.Select())
					{
						ARTaxTran new_artax = new ARTaxTran { TaxID = tax.TaxID };
						new_artax = invoiceEntry.Taxes.Insert(new_artax);
						manualTaxes.Add(new Tuple<ARTaxTran, PMTaxTran>(new_artax, tax));
					}

					foreach(Tuple<ARTaxTran, PMTaxTran> manualTax in manualTaxes)
                    {
						if (manualTax.Item1 != null)
                        {
							manualTax.Item1.TaxRate = manualTax.Item2.TaxRate;
							manualTax.Item1.CuryTaxableAmt = mult * manualTax.Item2.CuryTaxableAmt;
							manualTax.Item1.CuryExemptedAmt = mult * manualTax.Item2.CuryExemptedAmt;
							manualTax.Item1.CuryTaxAmt = mult * manualTax.Item2.CuryTaxAmt;
							manualTax.Item1.CuryRetainedTaxableAmt = mult * manualTax.Item2.CuryRetainedTaxableAmt.GetValueOrDefault();
							manualTax.Item1.CuryRetainedTaxAmt = mult * manualTax.Item2.CuryRetainedTaxAmt.GetValueOrDefault();

							invoiceEntry.Taxes.Update(manualTax.Item1);
						}
                    }
				}
								

				invoice.Hold = ARSetup.Current.HoldEntry == true;
				invoice = invoiceEntry.Document.Update(invoice);

				invoiceEntry.Save.Press();
				doc.Released = true;
				doc.Status = ProformaStatus.Closed;
				Document.Update(doc);

				PMBillingRecord billingRecord = PXSelect<PMBillingRecord, Where<PMBillingRecord.proformaRefNbr, Equal<Required<PMProforma.refNbr>>>>.Select(this, doc.RefNbr);
				if (billingRecord != null)
				{
					billingRecord.ARDocType = doc.ARInvoiceDocType;
					billingRecord.ARRefNbr = doc.ARInvoiceRefNbr;

					BillingRecord.Update(billingRecord);
				}

				foreach (PMProformaProgressLine line in processedProgressiveLines)
				{
					ProgressiveLines.Cache.SetValue<PMProformaProgressLine.aRInvoiceDocType>(line, doc.ARInvoiceDocType);
					ProgressiveLines.Cache.SetValue<PMProformaProgressLine.aRInvoiceRefNbr>(line, doc.ARInvoiceRefNbr);
					ProgressiveLines.Cache.SetValue<PMProformaProgressLine.released>(line, true);
					ProgressiveLines.Cache.MarkUpdated(line);
				}

				foreach (PMProformaTransactLine line in processedTransactionLines)
				{
					TransactionLines.Cache.SetValue<PMProformaTransactLine.aRInvoiceDocType>(line, doc.ARInvoiceDocType);
					TransactionLines.Cache.SetValue<PMProformaTransactLine.aRInvoiceRefNbr>(line, doc.ARInvoiceRefNbr);
					TransactionLines.Cache.SetValue<PMProformaTransactLine.released>(line, true);
					TransactionLines.Cache.MarkUpdated(line);

					List<PMTran> list;
					if (pmtranByProformalLineNbr.TryGetValue(line.LineNbr.Value, out list))
					{
						foreach (PMTran tran in list)
						{
							tran.ARTranType = line.ARInvoiceDocType;
							tran.ARRefNbr = line.ARInvoiceRefNbr;
							tran.RefLineNbr = line.ARInvoiceLineNbr;
							AllReferencedTransactions.Update(tran);
						}
					}
				}
				
				if (pmEntry.Transactions.Cache.IsDirty)
					pmEntry.Save.Press();
						
				Save.Press();
				ts.Complete();
			}

			if (project.AutomaticReleaseAR == true )
			{
				try
				{
					List<ARRegister> list = new List<ARRegister>();
					list.Add(invoice);
					ARDocumentRelease.ReleaseDoc(list, false);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.AutoReleaseARFailed, ex);
				}
			}
		}

		protected virtual void CopyPMAddressToARInvoice(ARInvoiceEntry invoiceEntry, PMAddress addressPM, ARAddress addressAR)
		{
			addressAR.BAccountAddressID = addressPM.BAccountAddressID;
			addressAR.BAccountID = addressPM.BAccountID;
			addressAR.RevisionID = addressPM.RevisionID;
			addressAR.IsDefaultAddress = addressPM.IsDefaultAddress;
			addressAR.AddressLine1 = addressPM.AddressLine1;
			addressAR.AddressLine2 = addressPM.AddressLine2;
			addressAR.AddressLine3 = addressPM.AddressLine3;
			addressAR.City = addressPM.City;
			addressAR.State = addressPM.State;
			addressAR.PostalCode = addressPM.PostalCode;
			addressAR.CountryID = addressPM.CountryID;
			addressAR.IsValidated = addressPM.IsValidated;
		}

		protected virtual void CopyPMContactToARInvoice(ARInvoiceEntry invoiceEntry, PMContact contactPM, ARContact contactAR)
		{
			contactAR.BAccountContactID = contactPM.BAccountContactID;
			contactAR.BAccountID = contactPM.BAccountID;
			contactAR.RevisionID = contactPM.RevisionID;
			contactAR.IsDefaultContact = contactPM.IsDefaultContact;
			contactAR.FullName = contactPM.FullName;
			contactAR.Attention = contactPM.Attention;
			contactAR.Salutation = contactPM.Salutation;
			contactAR.Title = contactPM.Title;
			contactAR.Phone1 = contactPM.Phone1;
			contactAR.Phone1Type = contactPM.Phone1Type;
			contactAR.Phone2 = contactPM.Phone2;
			contactAR.Phone2Type = contactPM.Phone2Type;
			contactAR.Phone3 = contactPM.Phone3;
			contactAR.Phone3Type = contactPM.Phone3Type;
			contactAR.Fax = contactPM.Fax;
			contactAR.FaxType = contactPM.FaxType;
			contactAR.Email = contactPM.Email;
		}
	
		public virtual ARTran InsertTransaction(ARInvoiceEntry invoiceEntry, PMProformaLine line, int mult)
		{
			var tran = new ARTran();
			tran.InventoryID = line.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : line.InventoryID;
			tran.BranchID = line.BranchID;
			tran.TranDesc = line.Description;
			tran.ProjectID = line.ProjectID;
			tran.TaskID = line.TaskID;
			tran.CostCodeID = line.CostCodeID;
			tran.ExpenseDate = line.Date;
			tran.AccountID = line.AccountID;
			tran.SubID = line.SubID;
			tran.TaxCategoryID = line.TaxCategoryID;
			tran.UOM = line.UOM;
			tran.Qty = line.Qty * mult;
			tran.CuryUnitPrice = tran.Qty == 0 && line.Type == PMProformaLineType.Progressive ? 0 : line.CuryUnitPrice;
			tran.CuryExtPrice = line.CuryLineTotal * mult;
			tran.FreezeManualDisc = true;
			tran.ManualPrice = true;
			tran.PMDeltaOption = line.Option == PMProformaLine.option.HoldRemainder ? line.Option : PMProformaLine.option.WriteOffRemainder;
			tran.DeferredCode = line.DefCode;
			tran.RetainagePct = line.RetainagePct;
			tran.RetainageAmt = line.Retainage;
			tran.CuryRetainageAmt = line.CuryRetainage;
			
			tran = invoiceEntry.Transactions.Insert(tran);

			bool updateRequired = false;
			if (!string.IsNullOrEmpty(line.TaxCategoryID) && line.TaxCategoryID != tran.TaxCategoryID)
			{
				tran.TaxCategoryID = line.TaxCategoryID;
				updateRequired = true;
			}

			//if CuryExtPrice is passed as zero InvoiceEntry will recalculate it as Qty x UnitPrice.
			//so we need to correct it explicitly:
			if (line.CuryLineTotal == 0)
			{
				tran.CuryExtPrice = 0;
				updateRequired = true;				
			}

			if (updateRequired)
			{
				tran = invoiceEntry.Transactions.Update(tran);
			}

			return tran;
		}
		
		public virtual void AddToInvoiced(PMProformaLine line)
		{
			if (line.LineTotal == 0)
				return;

			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}
			
			AddToInvoiced(line, projectedRevenueAccountGroupID);
		}

		public virtual void SubtractFromInvoiced(PMProformaLine line)
		{
			if (line.LineTotal == 0)
				return;

			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}

			AddToInvoiced(line, projectedRevenueAccountGroupID, -1);
		}

		public virtual void SubtractFromInvoiced(PMProformaLine line, int? revenueAccountGroup)
		{
			AddToInvoiced(line, revenueAccountGroup, -1);
		}
		public virtual void AddToInvoiced(PMProformaLine line, int? revenueAccountGroup, int mult = 1)
		{
			if (revenueAccountGroup != null && line.ProjectID != null && line.TaskID != null )
			{
				PMBudgetAccum invoiced = GetTargetBudget(revenueAccountGroup, line);
				invoiced = Budget.Insert(invoiced);

				if (UseBaseCurrency)
				{
					invoiced.CuryInvoicedAmount += mult * line.LineTotal.GetValueOrDefault();
					invoiced.InvoicedAmount += mult * line.LineTotal.GetValueOrDefault();
					invoiced.CuryDraftRetainedAmount += mult * line.Retainage.GetValueOrDefault();
					invoiced.DraftRetainedAmount += mult * line.Retainage.GetValueOrDefault();
					invoiced.CuryTotalRetainedAmount += mult * line.Retainage.GetValueOrDefault();
					invoiced.TotalRetainedAmount += mult * line.Retainage.GetValueOrDefault();

					if (line.IsPrepayment == true)
					{
						invoiced.CuryPrepaymentInvoiced += mult * line.LineTotal.GetValueOrDefault();
						invoiced.PrepaymentInvoiced += mult * line.LineTotal.GetValueOrDefault();
					}
				}
				else
				{
					invoiced.CuryInvoicedAmount += mult * line.CuryLineTotal.GetValueOrDefault();
					invoiced.InvoicedAmount += mult * line.LineTotal.GetValueOrDefault();
					invoiced.CuryDraftRetainedAmount += mult * line.CuryRetainage.GetValueOrDefault();
					invoiced.DraftRetainedAmount += mult * line.Retainage.GetValueOrDefault();
					invoiced.CuryTotalRetainedAmount += mult * line.CuryRetainage.GetValueOrDefault();
					invoiced.TotalRetainedAmount += mult * line.Retainage.GetValueOrDefault();

					if (line.IsPrepayment == true)
					{
						invoiced.CuryPrepaymentInvoiced += mult * line.CuryLineTotal.GetValueOrDefault();
						invoiced.PrepaymentInvoiced += mult * line.LineTotal.GetValueOrDefault();
					}
				}
			}
		}

		protected virtual PMBudgetAccum GetTargetBudget(int? accountGroupID, PMProformaLine line)
		{
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, accountGroupID);
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, line.ProjectID);

			bool isExisting;
			BudgetService budgetService = new BudgetService(this);
			PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out isExisting);

			PMBudgetAccum target = new PMBudgetAccum();
			target.Type = budget.Type;
			target.ProjectID = budget.ProjectID;
			target.ProjectTaskID = budget.TaskID;
			target.AccountGroupID = budget.AccountGroupID;
			target.InventoryID = budget.InventoryID;
			target.CostCodeID = budget.CostCodeID;
			target.UOM = budget.UOM;
			target.Description = budget.Description;

			return target;
		}

		public virtual void SubtractPerpaymentRemainder(PMProformaLine line, int mult = 1)
		{
			int? projectedRevenueAccountGroupID = null;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}
			else
			{
				projectedRevenueAccountGroupID = line.AccountGroupID;
			}

			if (projectedRevenueAccountGroupID != null && line.ProjectID != null && line.TaskID != null)
			{
				PMBudgetAccum invoiced = GetTargetBudget(projectedRevenueAccountGroupID, line);
				invoiced = Budget.Insert(invoiced);
				
				if (UseBaseCurrency)
				{
					invoiced.CuryPrepaymentAvailable -= mult * line.PrepaidAmount.GetValueOrDefault();
					invoiced.PrepaymentAvailable -= mult * line.PrepaidAmount.GetValueOrDefault();
				}
				else
				{
					invoiced.CuryPrepaymentAvailable -= mult * line.CuryPrepaidAmount.GetValueOrDefault();
					invoiced.PrepaymentAvailable -= mult * line.PrepaidAmount.GetValueOrDefault();
				}
			}
		}

		public virtual void SubtractAmountToInvoice(PMProformaLine line, decimal? curyValue, decimal? value) 
		{
			int? projectedRevenueAccountGroupID = null;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}
			else
			{
				projectedRevenueAccountGroupID = line.AccountGroupID;
			}

			if (projectedRevenueAccountGroupID != null && line.ProjectID != null && line.TaskID != null)
			{
				PMBudgetAccum invoiced = GetTargetBudget(projectedRevenueAccountGroupID, line);
				invoiced = Budget.Insert(invoiced);

				if (UseBaseCurrency)
				{
					invoiced.CuryAmountToInvoice -= value.GetValueOrDefault();
					invoiced.AmountToInvoice -= value.GetValueOrDefault();
				}
				else
				{
					invoiced.CuryAmountToInvoice -= curyValue.GetValueOrDefault();
					invoiced.AmountToInvoice -= value.GetValueOrDefault();
				}
			}
		}

		public virtual bool IsLimitsEnabled()
		{
			if (Project.Current == null)
				return false;

			return Project.Current.LimitsEnabled == true;
		}

		public virtual bool IsAdjustment(PMProformaTransactLine line)
		{
			if (line == null)
				return false;

			if (line.LineNbr == null)
				return true;

			//billing process do not create adjustment lines.
			if (this.UnattendedMode == true)
				return false;

			var references = GetReferencedTransactions();
			return !references.ContainsKey(line.LineNbr.Value);
		}

		public virtual Dictionary<int, List<PMTran>> GetReferencedTransactions()
		{
			if (cachedReferencedTransactions == null)
			{
				cachedReferencedTransactions = new Dictionary<int, List<PMTran>>();

				foreach (PMTran tran in AllReferencedTransactions.View.SelectMultiBound(new[] { Document.Current }).RowCast<PMTran>())
				{
					List<PMTran> list;
					if (!cachedReferencedTransactions.TryGetValue(tran.ProformaLineNbr.Value, out list))
					{
						list = new List<PMTran>();
						cachedReferencedTransactions.Add(tran.ProformaLineNbr.Value, list);
					}

					list.Add(tran);
				}
			}

			return cachedReferencedTransactions;
		}

		public bool IsAllocated(PMProformaTransactLine row)
		{
			return false;
		}

		public virtual int? GetProjectedAccountGroup(PMProformaTransactLine line)
		{
			int? projectedRevenueAccountGroupID = null;
			int? projectedRevenueAccount = line.AccountID;
		
			if (projectedRevenueAccount != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(TransactionLines.Cache, line, projectedRevenueAccount) as Account;
				if (revenueAccount != null)
				{
					if (revenueAccount.AccountGroupID == null)
						throw new PXException(PM.Messages.RevenueAccountIsNotMappedToAccountGroup, revenueAccount.AccountCD);

					projectedRevenueAccountGroupID = revenueAccount.AccountGroupID;
				}
			}

			return projectedRevenueAccountGroupID;
		}

		public virtual PMRevenueBudget SelectRevenueBudget(PMProformaProgressLine row)
		{
			var select = new PXSelect<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
				And<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>,
				And<PMRevenueBudget.accountGroupID, Equal<Required<PMRevenueBudget.accountGroupID>>,
				And<PMRevenueBudget.inventoryID, Equal<Required<PMRevenueBudget.inventoryID>>,
				And<PMRevenueBudget.costCodeID, Equal<Required<PMRevenueBudget.costCodeID>>>>>>>>(this);

			PMRevenueBudget budget = select.Select(row.ProjectID, row.TaskID, row.AccountGroupID, row.InventoryID, row.CostCodeID);

			return budget;
		}

		/// <summary>
		/// Returns Pending InvoicedAmount calculated for current document in Project Currency.
		/// </summary>
		public virtual decimal CalculatePendingInvoicedAmount(PMProformaProgressLine row)
		{
			return CalculatePendingInvoicedAmount(row.ProjectID, row.TaskID, row.AccountGroupID, row.InventoryID, row.CostCodeID);
		}

		/// <summary>
		/// Returns Pending InvoicedAmount calculated for current document in Project Currency.
		/// </summary>
		public virtual decimal CalculatePendingInvoicedAmount(int? projectID, int? taskID, int? accountGroupID, int? inventoryID, int? costCodeID)
		{
			decimal result = 0;

			foreach (PMBudgetAccum accum in Budget.Cache.Inserted)
			{
				if (accum.ProjectID == projectID && accum.ProjectTaskID == taskID && accum.AccountGroupID == accountGroupID && accum.InventoryID == inventoryID && accum.CostCodeID == costCodeID)
				{
					result += accum.CuryInvoicedAmount.GetValueOrDefault();
				}
			}

			return result;
		}

		/// <summary>
		/// Returns Prevously invoiced Amount in Project Currency
		/// </summary>
		public virtual Dictionary<BudgetKeyTuple, decimal> CalculatePreviouslyInvoicedAmounts(PMProforma document)
		{
			Dictionary<BudgetKeyTuple, decimal> previouslyInvoiced = new Dictionary<BudgetKeyTuple, decimal>();
			
			var select = new PXSelect<PMProformaProgressLine,
					Where<PMProformaProgressLine.type, Equal<PMProformaLineType.progressive>,
					And<PMProformaProgressLine.projectID, Equal<Required<PMProforma.projectID>>,
					And<PMProformaProgressLine.refNbr, GreaterEqual<Required<PMProformaProgressLine.refNbr>>>>>>(this);

			var currentAndFutureProgressiveLines = select.Select(document.ProjectID, document.RefNbr);

			var select2 = new PXSelect<PMProformaTransactLine,
					Where<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>,
					And<PMProformaProgressLine.projectID, Equal<Required<PMProforma.projectID>>,
					And<PMProformaTransactLine.refNbr, GreaterEqual<Required<PMProformaTransactLine.refNbr>>>>>>(this);

			var currentAndFutureTransactionLines = select2.Select(document.ProjectID, document.RefNbr);

			var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);
			var revenueBudget = selectRevenueBudget.Select(document.ProjectID);

			foreach (PMRevenueBudget budget in revenueBudget)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(budget);
				if (previouslyInvoiced.ContainsKey(key))
					previouslyInvoiced[key] += budget.CuryActualAmount.GetValueOrDefault() + budget.CuryInvoicedAmount.GetValueOrDefault();
				else
					previouslyInvoiced[key] = budget.CuryActualAmount.GetValueOrDefault() + budget.CuryInvoicedAmount.GetValueOrDefault();
			}

			foreach (PMBudgetAccum accum in Budget.Cache.Inserted)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(accum);
				if (previouslyInvoiced.ContainsKey(key))
					previouslyInvoiced[key] += accum.CuryInvoicedAmount.GetValueOrDefault();
				else
					previouslyInvoiced[key] = accum.CuryInvoicedAmount.GetValueOrDefault();
			}

			foreach (PMProformaProgressLine line in currentAndFutureProgressiveLines)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(line);
				if (previouslyInvoiced.ContainsKey(key))
					previouslyInvoiced[key] -= GetLineTotalInProjectCurrency(line);
			}

			foreach (PMProformaTransactLine line in currentAndFutureTransactionLines)
			{
				BudgetKeyTuple key = GetBudgetKey(line);
				if (previouslyInvoiced.ContainsKey(key))
				{
					previouslyInvoiced[key] -= GetLineTotalInProjectCurrency(line);
				}
				else 
				{
					//Will be executed in case BudgetLevel=Item and given item is not budgeted.
					BudgetKeyTuple naKey = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
					if (previouslyInvoiced.ContainsKey(naKey))
					{
						previouslyInvoiced[naKey] -= GetLineTotalInProjectCurrency(line);
					}
				}
			}

			return previouslyInvoiced;
		}

		private decimal GetPrepaidAmountInProjectCurrency(PMProformaLine line)
		{
			return !UseBaseCurrency ? line.CuryPrepaidAmount.GetValueOrDefault() : line.PrepaidAmount.GetValueOrDefault();
		}

		private decimal GetLineTotalInProjectCurrency(PMProformaLine line)
		{
			return !UseBaseCurrency ? line.CuryLineTotal.GetValueOrDefault() : line.LineTotal.GetValueOrDefault();
		}

		public virtual bool IsPrepaidAmountEnabled(PMProformaLine line)
		{
			return line.IsPrepayment != true;
		}

		public Dictionary<BudgetKeyTuple, decimal> previouslyInvoicedAmounts;
		public string previouslyInvoicedAmountsKey = null;

		/// <summary>
		/// Returns Previously Invoiced Amount in Project Currency
		/// </summary>
		public virtual decimal GetPreviouslyInvoicedAmount(PMProformaProgressLine row)
		{
			decimal result = 0;
			if (row.Released == true)
			{
				result = UseBaseCurrency ? row.PreviouslyInvoiced.GetValueOrDefault() : row.CuryPreviouslyInvoiced.GetValueOrDefault();
			}
			else
			{
				if (previouslyInvoicedAmounts == null || Document.Current.RefNbr != previouslyInvoicedAmountsKey)
				{
					previouslyInvoicedAmounts = CalculatePreviouslyInvoicedAmounts(Document.Current);
					previouslyInvoicedAmountsKey = Document.Current.RefNbr;
				}
				BudgetKeyTuple key = BudgetKeyTuple.Create(row);
				
				previouslyInvoicedAmounts.TryGetValue(key, out result);
			}

			return result;
		}

		public virtual void ApplyPrepayment(PMProforma doc)
		{
			var select = new PXSelect<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
				And<PMRevenueBudget.curyPrepaymentAvailable, Greater<decimal0>>>>(this);

			Dictionary<BudgetKeyTuple, decimal> remainders = new Dictionary<BudgetKeyTuple, decimal>();

			foreach (PMRevenueBudget budget in select.Select(doc.ProjectID))
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(budget);
				if (remainders.ContainsKey(key))
					remainders[key] += budget.CuryPrepaymentAvailable.GetValueOrDefault();
				else
					remainders[key] = budget.CuryPrepaymentAvailable.GetValueOrDefault();
			}

			foreach (PMBudgetAccum accum in Budget.Cache.Inserted)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(accum);
				if (remainders.ContainsKey(key))
					remainders[key] += accum.CuryPrepaymentAvailable.GetValueOrDefault(); //not  saved -ve values.
			}


			foreach (PMProformaProgressLine line in ProgressiveLines.Select())
			{
				if (line.IsPrepayment == true)
					continue;

				BudgetKeyTuple key = BudgetKeyTuple.Create(line);
				if (Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.InventoryID, CostCodeAttribute.DefaultCostCode.GetValueOrDefault());
				}


				if ((Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail) && line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					if (!remainders.ContainsKey(key))
					{
						key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
					}
				}
				
				decimal remainder = 0;
				if (remainders.TryGetValue(key, out remainder) && remainder > 0 && line.CuryAmount > 0 && line.CuryPrepaidAmount == 0)
				{
					decimal curyRemainder = !UseBaseCurrency ? remainder : ConvertToDocumentCurrency(remainder);

					line.CuryPrepaidAmount = Math.Min(curyRemainder, line.CuryAmount.Value);
					ProgressiveLines.Update(line);

					remainders[key] -= GetPrepaidAmountInProjectCurrency(line);
				}
			}

			foreach (PMProformaTransactLine line in TransactionLines.Select())
			{
				if (line.IsPrepayment == true)
					continue;

				BudgetKeyTuple key = BudgetKeyTuple.Create(line);
				if (Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.InventoryID, CostCodeAttribute.DefaultCostCode.GetValueOrDefault());
				}

				if ((Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail) && line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					if (!remainders.ContainsKey(key))
					{
						key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
					}
				}
								
				decimal remainder = 0;
				if (remainders.TryGetValue(key, out remainder) && remainder > 0 && line.CuryAmount > 0 && line.CuryPrepaidAmount.GetValueOrDefault() == 0) 
				{
					decimal curyRemainder = !UseBaseCurrency ? remainder : ConvertToDocumentCurrency(remainder);
					line.CuryPrepaidAmount = Math.Min(curyRemainder, line.CuryAmount.GetValueOrDefault());
					TransactionLines.Update(line);

					remainders[key] -= GetPrepaidAmountInProjectCurrency(line);
				}
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public virtual void ValidatePrecedingBeforeRelease(PMProforma doc)
		{
			var selectUnreleased = new PXSelectJoin<PMBillingRecord,
				InnerJoin<PMBillingRecordEx, On<PMBillingRecord.projectID, Equal<PMBillingRecordEx.projectID>,
				And<PMBillingRecord.billingTag, Equal<PMBillingRecordEx.billingTag>,
				And<PMBillingRecord.proformaRefNbr, Greater<PMBillingRecordEx.proformaRefNbr>,
				And<PMBillingRecordEx.aRRefNbr, IsNull>>>>>,
				Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>,
				And<PMBillingRecord.proformaRefNbr, Equal<Required<PMBillingRecord.proformaRefNbr>>>>>(this);

			var resultset = selectUnreleased.Select(doc.ProjectID, doc.RefNbr);
			if (resultset.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				foreach (PXResult<PMBillingRecord, PMBillingRecordEx> res in resultset)
				{
					PMBillingRecordEx unreleased = (PMBillingRecordEx)res;
					sb.AppendFormat("{0},", unreleased.ProformaRefNbr);
				}

				string list = sb.ToString().TrimEnd(',');

				throw new PXException(Messages.UnreleasedProforma, list);
			}
		}

		public virtual void ValidatePrecedingInvoicesBeforeRelease(PMProforma doc)
		{
			var selectUnreleased = new PXSelectJoin<PMBillingRecord,
				InnerJoin<PMBillingRecordEx, On<PMBillingRecord.projectID, Equal<PMBillingRecordEx.projectID>,
				And<PMBillingRecord.billingTag, Equal<PMBillingRecordEx.billingTag>,
				And<PMBillingRecord.proformaRefNbr, Greater<PMBillingRecordEx.proformaRefNbr>,
				And<PMBillingRecordEx.aRRefNbr, IsNotNull>>>>,
				InnerJoin<ARRegister, On<ARRegister.docType, Equal<PMBillingRecordEx.aRDocType>, And<ARRegister.refNbr, Equal<PMBillingRecordEx.aRRefNbr>>>>>,
				Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>,
				And<PMBillingRecord.proformaRefNbr, Equal<Required<PMBillingRecord.proformaRefNbr>>>>,
				OrderBy<Desc<PMBillingRecordEx.recordID>>>(this);

			var resultset = selectUnreleased.SelectWindowed(0, 1, doc.ProjectID, doc.RefNbr);
			if (resultset.Count > 0)
			{
				ARRegister register = PXResult.Unwrap<ARRegister>(resultset[0]);
				if (register != null && register.Released != true)
					throw new PXException(Messages.UnreleasedPreviousInvoice, register.DocType, register.RefNbr);
			}
		}

		
		public virtual decimal ConvertToDocumentCurrency(decimal? value)
		{
			if (value.GetValueOrDefault() == 0)
				return 0;

			if (Document.Current == null)
				throw new InvalidOperationException("Failed to Convert to base currency. The Document.Current is null.");

			if (Document.Current.CuryID == Company.Current.BaseCuryID)
				return value.Value;

			if (currencyinfo.Current == null)
			{
				currencyinfo.Current = currencyinfo.Select();
			}

			decimal curyamount;
			PXDBCurrencyAttribute.CuryConvCury(currencyinfo.Cache, currencyinfo.Current, value.GetValueOrDefault(), out curyamount);

			return curyamount;
		}

		public virtual void AppendUnbilled()
		{
			if (Document.Current == null)
				return;

			PMBillingRecord billingRecord = PXSelect<PMBillingRecord, Where<PMBillingRecord.proformaRefNbr, Equal<Required<PMProforma.refNbr>>>>.Select(this, Document.Current.RefNbr);
			string tag = billingRecord?.BillingTag ?? "P";

			DateTime invoiceDate = Document.Current.InvoiceDate.Value;

			ProformaAppender engine = PXGraph.CreateInstance<ProformaAppender>();
			engine.SetProformaEntry(this);
			List<PMTask> tasks = engine.SelectBillableTasks(Project.Current);
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, Document.Current.CustomerID);
			
			DateTime cuttoffDate = invoiceDate.AddDays(engine.IncludeTodaysTransactions ? 1 : 0);
			engine.PreSelectTasksTransactions(Document.Current.ProjectID, tasks, cuttoffDate); //billingRules dictionary also filled.

			HashSet<string> distinctRateTables = new HashSet<string>();
			foreach (PMTask task in tasks)
			{
				if (!string.IsNullOrEmpty(task.RateTableID))
					distinctRateTables.Add(task.RateTableID);
			}
			HashSet<string> distinctRateTypes = new HashSet<string>();
			foreach (List<PMBillingRule> ruleList in engine.billingRules.Values)
			{
				foreach (PMBillingRule rule in ruleList)
				{
					if (!string.IsNullOrEmpty(rule.RateTypeID))
						distinctRateTypes.Add(rule.RateTypeID);
				}
			}

			engine.InitRateEngine(distinctRateTables.ToList(), distinctRateTypes.ToList());

			List<PMTran> billingBase = new List<PMTran>();
			List<PMBillEngine.BillingData> billingData = new List<PMBillEngine.BillingData>();
			Dictionary<int, decimal> availableQty = new Dictionary<int, decimal>();
			Dictionary<int, PMRecurringItem> billingItems = new Dictionary<int, PMRecurringItem>();

			foreach (PMTask task in tasks)
			{
				if (task.WipAccountGroupID != null)
					continue;
				
				List<PMBillingRule> rulesList;
				if (engine.billingRules.TryGetValue(task.BillingID, out rulesList))
				{
					foreach (PMBillingRule rule in rulesList)
					{
						if (rule.Type == PMBillingType.Transaction)
						{
							billingData.AddRange(engine.BillTask(Project.Current, customer, task, rule, invoiceDate, availableQty, billingItems, true));
						}
					}
				}
			}

			engine.InsertTransactionsInProforma(Project.Current, billingData);

			foreach (PMBillEngine.BillingData data in billingData)
			{
				foreach (PMTran orig in data.Transactions)
				{
					orig.Billed = true;
					orig.BilledDate = invoiceDate;
					Unbilled.Update(orig);
					PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, orig);
				}
			}
		}

		/// <summary>
		/// If false during the release of proforma documet taxes are copied as is; otherwise taxes are recalculated automaticaly on the ARInvoice.
		/// Default value is false.
		/// </summary>
		public virtual bool RecalculateTaxesOnRelease()
		{
			if (Document.Current != null && Customer.Current != null)
			{
				return Customer.Current.PaymentsByLinesAllowed == true;
			}

			return false;
		}

		public virtual BudgetKeyTuple GetBudgetKey(PMProformaTransactLine line)
		{
			int? accountGroupID = GetProjectedAccountGroup(line);
			int inventoryID = line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);

			if (Project.Current.BudgetLevel != BudgetLevels.Item)
				inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;

			BudgetKeyTuple defualtKey = BudgetKeyTuple.Create(line);
			BudgetKeyTuple key = new BudgetKeyTuple(defualtKey.ProjectID, defualtKey.ProjectTaskID, accountGroupID.GetValueOrDefault(), inventoryID, defualtKey.CostCodeID);

			return key;
		}

		#region Retainage

		protected virtual void RecalculateRetainageOnDocument(PMProject project)
		{
			if (project.RetainageMode == RetainageModes.Contract)
			{
				PMProjectRevenueTotal budget = PXSelectReadonly<PMProjectRevenueTotal,
				Where<PMProjectRevenueTotal.projectID, Equal<Required<PMProjectRevenueTotal.projectID>>>>.Select(this, project.ContractID);
				
				decimal totalRetainageOnInvoice = GetTotalRetainageOnInvoice(project.RetainagePct.GetValueOrDefault());
				decimal totalRetainageUptoDate = GetTotalRetainageUptoDate(budget);
				decimal contractAmount = GetContractAmount(project, budget);

				RecalculateContractRetainage(project, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice);

				decimal totalInvoiceUptoDate = GetTotalInvoiced(budget);
				ReAllocateContractRetainage(project, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice, totalInvoiceUptoDate);

				ProgressiveLines.View.RequestRefresh();
			}
		}

		/// <summary>
		/// Returns Total Retainage accumulated upto date excluding current document
		/// </summary>
		private decimal GetTotalRetainageUptoDate(PMProjectRevenueTotal budget)
		{
			decimal totalRetainageUptoDate = budget.CuryTotalRetainedAmount.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				totalRetainageUptoDate += item.CuryTotalRetainedAmount.GetValueOrDefault();
			}

			foreach (PMProformaProgressLine line in ProgressiveLines.Select())
			{
				totalRetainageUptoDate -= line.CuryRetainage.GetValueOrDefault();
			}

			return totalRetainageUptoDate;
		}
				
		public virtual void RecalculateRetainage()
		{
			if (Project.Current == null)
				return;
			if (Document.Current == null)
				return;
			if (Document.Current.Released == true)
				return;
					
			if (Project.Current.RetainageMode == RetainageModes.Contract)
			{
				PMProjectRevenueTotal budget = PXSelectReadonly<PMProjectRevenueTotal,
				Where<PMProjectRevenueTotal.projectID, Equal<Required<PMProjectRevenueTotal.projectID>>>>.Select(this, Project.Current.ContractID);

				RecalculateContractRetainage(Project.Current, budget);
				ReAllocateContractRetainage(Project.Current, budget);
			}
			else if (Project.Current.RetainageMode == RetainageModes.Line)
			{
				RecalculateLineRetainage(Project.Current);
			}
		}

		protected bool RecalculatingContractRetainage = false;
		protected virtual void RecalculateContractRetainage(PMProject project, PMProjectRevenueTotal budget)
		{
			
			decimal totalRetainageOnInvoice = GetTotalRetainageOnInvoice(project.RetainagePct.GetValueOrDefault());
			decimal totalRetainageUptoDate = budget.CuryTotalRetainedAmount.GetValueOrDefault();
			decimal contractAmount = GetContractAmount(project, budget);


			RecalculateContractRetainage(project, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice);
		}

		protected virtual void RecalculateContractRetainage(PMProject project, decimal totalRetainageUptoDate, decimal contractAmount, decimal totalRetainageOnInvoice)
		{
			RecalculatingContractRetainage = true;
			try
			{
				decimal roundingOverflow = 0;
				
				foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
				{
					PMProformaProgressLine line = (PMProformaProgressLine)res;
					line = (PMProformaProgressLine) ProgressiveLines.Cache.CreateCopy(line);//Use Copy instance since we are calling Update in the context on RowUpdated (outer caller).
					Tuple<decimal, decimal> retaiangeOnLine = CalculateContractRetainageOnLine(project, line, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice, roundingOverflow);
					line.CuryRetainage = retaiangeOnLine.Item1;
					roundingOverflow = retaiangeOnLine.Item2;
					line = ProgressiveLines.Update(line);
				}
			}
			finally
			{
				RecalculatingContractRetainage = false;
			}
		}

		protected virtual Tuple<decimal, decimal> CalculateContractRetainageOnLine(PMProject project, PMProformaProgressLine line, decimal totalRetainageUptoDate, decimal contractAmount, decimal totalRetainageOnInvoice, decimal roundingOverflow)
		{
			decimal result = 0;
			
			decimal totalRetainageCap = contractAmount * 0.01m * project.RetainagePct.GetValueOrDefault() * project.RetainageMaxPct.GetValueOrDefault() * 0.01m;

			decimal lineRetainage = line.CuryLineTotal.GetValueOrDefault() * project.RetainagePct.GetValueOrDefault() * 0.01m;
			if (project.BillingCuryID != project.CuryID)
			{
				lineRetainage = line.LineTotal.GetValueOrDefault() * project.RetainagePct.GetValueOrDefault() * 0.01m;
			}

			if (totalRetainageOnInvoice <= 0 || totalRetainageOnInvoice + totalRetainageUptoDate <= totalRetainageCap)
			{
				//within limits.
				result = lineRetainage;
			}
			else
			{
				decimal overLimitRetainage = totalRetainageOnInvoice + totalRetainageUptoDate - totalRetainageCap;
				decimal ratio = lineRetainage / totalRetainageOnInvoice;
				decimal decrease = overLimitRetainage * ratio;

				decimal effectiveRetainage = Math.Max(0, decimal.Round(lineRetainage - decrease + roundingOverflow, 2));
				roundingOverflow = (lineRetainage - decrease + roundingOverflow) - effectiveRetainage;
								
				if (project.CuryID != project.BillingCuryID)
				{
					PXDBCurrencyAttribute.CuryConvCury(currencyinfo.Cache, currencyinfo.Current, effectiveRetainage, out effectiveRetainage);
				}

				result = effectiveRetainage;
			}

			return new Tuple<decimal, decimal>(result, roundingOverflow);
		}

		private decimal GetTotalRetainageOnInvoice(decimal retainagePct)
		{
			decimal totalRetainageOnInvoice = 0;

			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
			{
				PMProformaProgressLine line = (PMProformaProgressLine)res;

				decimal lineRetainage = line.CuryLineTotal.GetValueOrDefault() * retainagePct * 0.01m;

				if (lineRetainage > 0)
				{
					totalRetainageOnInvoice += lineRetainage;
				}
			}

			return totalRetainageOnInvoice;
		}

		protected virtual void ReAllocateContractRetainage(PMProject project, PMProjectRevenueTotal budget)
		{
			decimal totalRetainageOnInvoice = GetTotalRetainageOnInvoice(project.RetainagePct.GetValueOrDefault());
			decimal totalRetainageUptoDate = budget.CuryTotalRetainedAmount.GetValueOrDefault();
			decimal contractAmount = GetContractAmount(project, budget);
			decimal totalInvoiceUptoDate = GetTotalInvoiced(budget);

			ReAllocateContractRetainage(project, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice, totalInvoiceUptoDate);
		}

		protected virtual void ReAllocateContractRetainage(PMProject project, decimal totalRetainageUptoDate, decimal contractAmount, decimal totalRetainageOnInvoice, decimal totalInvoiceUptoDate)
		{
			Debug.Print("ReAllocateContractRetainage totalRetainageUptoDate={0} contractAmount={1} totalRetainageOnInvoice={2} totalInvoiceUptoDate={3}", totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice, totalInvoiceUptoDate);
			Dictionary<PMProformaLine, decimal> allocations = new Dictionary<PMProformaLine, decimal>();
			allocations.Clear();

			decimal totalRetainageCap = contractAmount * 0.01m * project.RetainagePct.GetValueOrDefault() * project.RetainageMaxPct.GetValueOrDefault() * 0.01m;

			decimal totalRetainageToAllocate = Math.Min(totalRetainageCap, totalRetainageOnInvoice + totalRetainageUptoDate);

			if (totalRetainageUptoDate > totalRetainageCap)
				totalRetainageToAllocate = totalRetainageUptoDate;

			decimal unalloated = totalRetainageToAllocate;

			List<PXResult<PMProformaProgressLine, PMRevenueBudget>> available = new List<PXResult<PMProformaProgressLine, PMRevenueBudget>>();
			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
			{
				available.Add(res);
			}

			int cx = 0;
			while (totalRetainageToAllocate > 0.01m && available.Count > 1)
			{
				cx++;
				List<PXResult<PMProformaProgressLine, PMRevenueBudget>> toremove = new List<PXResult<PMProformaProgressLine, PMRevenueBudget>>();
				foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in available)
				{
					PMProformaProgressLine line = (PMProformaProgressLine)res;
					PMRevenueBudget revenue = (PMRevenueBudget)res;

					decimal invoicedTotal = GetInvoicedAmount(revenue);
					decimal capacity = GetRetainedTotal(revenue);

					decimal toAllocate = totalRetainageToAllocate;
					if (totalInvoiceUptoDate != 0)
					{
						toAllocate = decimal.Round(totalRetainageToAllocate * invoicedTotal / totalInvoiceUptoDate, 2);
					}

					if (!allocations.ContainsKey(line))
					{
						allocations.Add(line, 0);
					}

					Debug.Print("iteration:{0} invoiceTotal:{1} toAllocate:{2} capacity:{3} currentAllocations:{4}", cx, invoicedTotal, toAllocate, capacity, allocations[line]);

					decimal alreadyAllocatedSum = allocations[line];

					if (capacity > (toAllocate + alreadyAllocatedSum) && toAllocate > 0)
					{
						allocations[line] += toAllocate;
						unalloated -= toAllocate;
					}
					else
					{
						allocations[line] += (capacity - alreadyAllocatedSum);
						unalloated -= (capacity - alreadyAllocatedSum);
						toremove.Add(res);
					}
				}

				foreach (var line in toremove)
				{
					available.Remove(line);
				}

				totalRetainageToAllocate = unalloated;
				Debug.Print("iteration:{0} totalRetainageToAllocate:{1}", cx, totalRetainageToAllocate);
			}

			if (available.Count == 1)
			{
				if (!allocations.ContainsKey(available[0]))
				{
					allocations.Add(available[0], 0);
				}

				Debug.Print("last iteration:{0} totalRetainageToAllocate:{1} currentAllocations:{2}", cx, totalRetainageToAllocate, allocations[available[0]]);

				allocations[available[0]] += totalRetainageToAllocate;
			}

			Debug.Print("Iterations: {0}", cx);

			decimal allocatedTotal = 0;
			foreach (PMProformaProgressLine line in ProgressiveLines.Select())
			{
				decimal allocatedRetainage = 0;
				allocations.TryGetValue(line, out allocatedRetainage);

				if (project.BillingCuryID != project.CuryID)
				{
					PXDBCurrencyAttribute.CuryConvCury(currencyinfo.Cache, currencyinfo.Current, allocatedRetainage, out allocatedRetainage);
				}

				ProgressiveLines.Cache.SetValue<PMProformaLine.curyAllocatedRetainedAmount>(line, allocatedRetainage);
				allocatedTotal += allocatedRetainage;
			}
			Document.Cache.SetValue<PMProforma.curyAllocatedRetainedTotal>(Document.Current, allocatedTotal);
		}

		private decimal GetInvoicedAmount(PMRevenueBudget budget)
		{
			decimal invoicedTotal = budget.CuryInvoicedAmount.GetValueOrDefault()
							+ budget.CuryActualAmount.GetValueOrDefault()
							+ budget.CuryAmountToInvoice.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				if (item.ProjectTaskID == budget.ProjectTaskID &&
					item.AccountGroupID == budget.AccountGroupID &&
					item.InventoryID == budget.InventoryID &&
					item.CostCodeID == budget.CostCodeID)
				{
					invoicedTotal += item.CuryInvoicedAmount.GetValueOrDefault() + item.CuryAmountToInvoice.GetValueOrDefault();
				}
			}

			return invoicedTotal;
		}

		private decimal GetTotalInvoiced(PMProjectRevenueTotal budget)
		{
			decimal invoicedTotal = budget.CuryActualAmount.GetValueOrDefault() + budget.CuryInvoicedAmount.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				invoicedTotal += item.CuryInvoicedAmount.GetValueOrDefault();
			}

			return invoicedTotal;
		}

		private decimal GetRetainedTotal(PMRevenueBudget budget)
		{
			decimal total = budget.CuryDraftRetainedAmount.GetValueOrDefault() + budget.CuryRetainedAmount.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				if (item.ProjectTaskID == budget.ProjectTaskID &&
					item.AccountGroupID == budget.AccountGroupID &&
					item.InventoryID == budget.InventoryID &&
					item.CostCodeID == budget.CostCodeID)
				{
					total += item.CuryDraftRetainedAmount.GetValueOrDefault() + item.CuryRetainedAmount.GetValueOrDefault();
				}
			}

			return total;
		}

		protected virtual void RecalculateLineRetainage(PMProject project)
		{
			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
			{
				PMProformaProgressLine line = (PMProformaProgressLine)res;
				PMRevenueBudget revenue = (PMRevenueBudget)res;

				decimal lineRetainage = line.CuryLineTotal.GetValueOrDefault() * line.RetainagePct.GetValueOrDefault() * 0.01m;
				decimal maxLineRetainage = GetLineAmount(project, revenue) * revenue.RetainagePct.GetValueOrDefault() * 0.01m * revenue.RetainageMaxPct.GetValueOrDefault() * 0.01m;

				//TODO MC support

				if (lineRetainage + revenue.CuryTotalRetainedAmount.GetValueOrDefault() <= maxLineRetainage)
				{
					line.CuryRetainage = lineRetainage;
				}
				else
				{
					line.CuryRetainage = maxLineRetainage - revenue.CuryTotalRetainedAmount.GetValueOrDefault();
				}

				ProgressiveLines.Update(line);
			}
		}

		protected virtual decimal GetContractAmount(PMProject project, PMProjectRevenueTotal budget)
		{
			if (project.IncludeCO == true)
			{
				return budget.CuryRevisedAmount.GetValueOrDefault();
			}
			else
			{
				return budget.CuryAmount.GetValueOrDefault();
			}
		}

		protected virtual decimal GetLineAmount(PMProject project, PMBudget budget)
		{
			if (project.IncludeCO == true)
			{
				return budget.CuryRevisedAmount.GetValueOrDefault();
			}
			else
			{
				return budget.CuryAmount.GetValueOrDefault();
			}
		}

		#endregion

		private bool UseBaseCurrency
		{
			get
			{
				if (Document.Current != null && Document.Current.ProjectID != null)
				{
					if (Project.Current.CuryID != Project.Current.BillingCuryID)
					{
						return true;
					}
				}

				return false;
			}
		}

		public virtual void CheckMigrationMode()
		{
			if (ARSetup.Current.MigrationMode == true)
			{
				throw new PXException(Messages.ActiveMigrationMode);
			}
		}

		#region External Tax Provider


		public bool RecalculateExternalTaxesSync { get; set; }

		public virtual bool IsExternalTax(string taxZoneID)
		{
					return false;
			}

		public virtual PMProforma CalculateExternalTax(PMProforma doc)
		{
			return doc;
		}
		#endregion

		#region Local Types
		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class PMProformaOverflow : PX.Data.IBqlTable
		{			
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

			[PXDBLong()]
			[CurrencyInfo(typeof(PMProforma.curyInfoID))]
			public virtual Int64? CuryInfoID
			{
				get;
				set;
			}
			#endregion
			
			#region CuryOverflowTotal
			public abstract class curyOverflowTotal : PX.Data.BQL.BqlDecimal.Field<curyOverflowTotal> { }
			[PXCurrency(typeof(curyInfoID), typeof(overflowTotal), BaseCalc = false)]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Over-Limit Total", Enabled = false, Visible = false)]
			public virtual Decimal? CuryOverflowTotal
			{
				get; set;
			}
			#endregion
			#region OverflowTotal
			public abstract class overflowTotal : PX.Data.BQL.BqlDecimal.Field<overflowTotal> { }
			[PXBaseCury]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overflow Total in Base Currency", Enabled = false, Visible = false)]
			public virtual Decimal? OverflowTotal
			{
				get; set;
			}
			#endregion
		}

		#endregion

	}

	[PXDynamicButton(new string[] { ProgressivePasteLineCommand, ProgressiveResetOrderCommand },
					 new string[] { PX.Data.ActionsMessages.PasteLine, PX.Data.ActionsMessages.ResetOrder },
					 TranslationKeyType = typeof(PX.Objects.Common.Messages))]
	public class ProgressLineSelect : PXOrderedSelect<PMProforma, PMProformaProgressLine,
			InnerJoin<PMRevenueBudget, On<PMProformaProgressLine.projectID, Equal<PMRevenueBudget.projectID>,
				And<PMProformaProgressLine.taskID, Equal<PMRevenueBudget.projectTaskID>,
				And<PMProformaProgressLine.accountGroupID, Equal<PMRevenueBudget.accountGroupID>,
				And<PMProformaProgressLine.inventoryID, Equal<PMRevenueBudget.inventoryID>,
				And<PMProformaProgressLine.costCodeID, Equal<PMRevenueBudget.costCodeID>>>>>>>,
			Where<PMProformaProgressLine.refNbr, Equal<Current<PMProforma.refNbr>>, And<PMProformaProgressLine.type, Equal<PMProformaLineType.progressive>>>, OrderBy<Asc<PMProformaProgressLine.sortOrder, Asc<PMProformaProgressLine.lineNbr>>>>
	{
		public ProgressLineSelect(PXGraph graph) : base(graph) { }
		public ProgressLineSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public const string ProgressivePasteLineCommand = "ProgressPasteLine";
		public const string ProgressiveResetOrderCommand = "ProgressResetOrder";

		protected override void AddActions(PXGraph graph)
		{
			AddAction(graph, ProgressivePasteLineCommand, PX.Data.ActionsMessages.PasteLine, PasteLine);
			AddAction(graph, ProgressiveResetOrderCommand, PX.Data.ActionsMessages.ResetOrder, ResetOrder);
		}
	}

	[PXDynamicButton(new string[] { TransactPasteLineCommand, TransactResetOrderCommand },
					 new string[] { PX.Data.ActionsMessages.PasteLine, PX.Data.ActionsMessages.ResetOrder },
					 TranslationKeyType = typeof(PX.Objects.Common.Messages))]
	public class TransactLineSelect : PXOrderedSelect<PMProforma, PMProformaTransactLine,
			Where<PMProformaTransactLine.refNbr, Equal<Current<PMProforma.refNbr>>,
			And<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>>>,
			OrderBy<Asc<PMProformaTransactLine.sortOrder, Asc<PMProformaTransactLine.lineNbr>>>>
	{
		public TransactLineSelect(PXGraph graph) : base(graph) { }
		public TransactLineSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public const string TransactPasteLineCommand = "TransactPasteLine";
		public const string TransactResetOrderCommand = "TransactResetOrder";

		protected override void AddActions(PXGraph graph)
		{
			AddAction(graph, TransactPasteLineCommand, PX.Data.ActionsMessages.PasteLine, PasteLine);
			AddAction(graph, TransactResetOrderCommand, PX.Data.ActionsMessages.ResetOrder, ResetOrder);
		}
	}

	public class PMActivityList<TPrimaryView> : CRActivityList<TPrimaryView>
		where TPrimaryView : class, IBqlTable, new()
	{
		public PMActivityList(PXGraph graph):base(graph)
		{
		}
		
		protected override object GetSourceRow(string sourceType, CRPMTimeActivity activity)
		{
			object sourceRow = _Graph.Caches[typeof(TPrimaryView)].Current;

			if(sourceRow != null && sourceType == PMNotificationSource.Project)
			{
				int? projectID = (int?) _Graph.Caches[typeof(TPrimaryView)].GetValue(sourceRow, nameof(PMProforma.ProjectID));
				PMProject rec = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.SelectWindowed(_Graph, 0, 1, projectID);

				if (rec != null && rec.NonProject != true && rec.BaseType == CT.CTPRType.Project)
				{					
					return rec;
				}
			}

			return base.GetSourceRow(sourceType, activity);
		}

		public virtual bool IsProjectSourceActive(int? projectID, string notificationCD)
		{
			var select = new PXSelectJoin<NotificationSource,
				InnerJoin<NotificationSetup, On<NotificationSource.setupID, Equal<NotificationSetup.setupID>>,
				InnerJoin<PMProject, On<PMProject.noteID, Equal<NotificationSource.refNoteID>>>>,
				Where<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>,
				And<PMProject.contractID, Equal<Required<PMProject.contractID>>,
				And<NotificationSource.active, Equal<True>>>>>(_Graph);

			NotificationSource source = select.SelectSingle(notificationCD, projectID);

			return source != null;
		}

		public virtual string ProjectInvoiceReportActive(int? projectID)
		{
			var select = new PXSelectJoin<NotificationSource,
				InnerJoin<NotificationSetup, On<NotificationSource.setupID, Equal<NotificationSetup.setupID>>,
				InnerJoin<PMProject, On<PMProject.noteID, Equal<NotificationSource.refNoteID>>>>,
				Where<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>,
				And<PMProject.contractID, Equal<Required<PMProject.contractID>>,
				And<NotificationSource.active, Equal<True>>>>>(_Graph);

			NotificationSource source = select.SelectSingle("INVOICE", projectID);
			
			if (source != null)
			return source.ReportID;
			else
				return null;
		}

		public virtual string GetDefaultProjectInvoiceReport()
		{
			return "PM641000";
		}
	}

	public class PMDefaultMailToAttribute : CRDefaultMailToAttribute
	{
		public PMDefaultMailToAttribute()
			: base()
		{
		}

		public PMDefaultMailToAttribute(Type select)
			: base(select)
		{
		}

		protected override PXSelectBase GetSelectView(PXGraph graph)
		{
			var selectView = graph.GetType().GetField(_hostViewName).GetValue(graph);
			
			return (PXSelectBase)selectView;
		}
	}

	public class ProformaAppender : PMBillEngine
	{
		public void SetProformaEntry(ProformaEntry proformaEntry)
		{
			this.proformaEntry = proformaEntry;
		}

		public override List<PMTran> SelectBillingBase(int? projectID, int? taskID, int? accountGroupID, bool includeNonBillable)
		{
			List<PMTran> list = new List<PMTran>();
			
			foreach (PMTran tran in proformaEntry.Unbilled.Cache.Updated)
			{
				if (tran.Selected == true && tran.Billed != true && tran.ExcludedFromBilling != true && tran.TaskID == taskID && tran.AccountGroupID == accountGroupID)
				{
					list.Add(tran);
				}
			}

			return list;
		}

		public void InitRateEngine(IList<string> distinctRateTables, IList<string> distinctRateTypes)
		{
			rateEngine = CreateRateEngineV2(distinctRateTables, distinctRateTypes);
		}
	}
}
