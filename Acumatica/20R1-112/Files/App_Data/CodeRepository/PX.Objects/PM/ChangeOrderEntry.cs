using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Reports.Parser;
using System.Diagnostics;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.CM;
using PX.Common;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.CA.Descriptor;
using CommonServiceLocator;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.PM
{
	public class ChangeOrderEntry : PXGraph<ChangeOrderEntry, PMChangeOrder>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
		public const string ChangeOrderReport = "PM643000";
		public const string ChangeOrderNotificationCD = "CHANGE ORDER";

		#region DAC Overrides

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMChangeOrder.date), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(PMChangeOrder.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMChangeOrder.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMChangeOrder.revenueTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMChangeOrder.revenueTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region PMChangeOrderRevenueBudget

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Quantity", Enabled = false)]
		protected virtual void PMChangeOrderRevenueBudget_PreviouslyApprovedQty_CacheAttached(PXCache sender){}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Amount", Enabled = false)]
		protected virtual void PMChangeOrderRevenueBudget_PreviouslyApprovedAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Quantity", Enabled = false)]
		protected virtual void PMChangeOrderRevenueBudget_CommittedCOQty_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Amount", Enabled = false)]
		protected virtual void PMChangeOrderRevenueBudget_CommittedCOAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Other Draft CO Amount", Enabled = false)]
		protected virtual void PMChangeOrderRevenueBudget_OtherDraftRevisedAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Total Potentially Revised Amount", Enabled = false)]
		protected virtual void PMChangeOrderRevenueBudget_TotalPotentialRevisedAmount_CacheAttached(PXCache sender) { }

		#endregion

		#region PMChangeOrderCostBudget

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Quantity", Enabled = false)]
		protected virtual void PMChangeOrderCostBudget_PreviouslyApprovedQty_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Amount", Enabled = false)]
		protected virtual void PMChangeOrderCostBudget_PreviouslyApprovedAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Quantity", Enabled = false)]
		protected virtual void PMChangeOrderCostBudget_CommittedCOQty_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Amount", Enabled = false)]
		protected virtual void PMChangeOrderCostBudget_CommittedCOAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Other Draft CO Amount", Enabled = false)]
		protected virtual void PMChangeOrderCostBudget_OtherDraftRevisedAmount_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Total Potentially Revised Amount", Enabled = false)]
		protected virtual void PMChangeOrderCostBudget_TotalPotentialRevisedAmount_CacheAttached(PXCache sender) { }

		#endregion

		#region PMChangeOrderLine

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Potentially Revised Quantity", Enabled = false)]
		protected virtual void PMChangeOrderLine_PotentialRevisedQty_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Potentially Revised Amount", Enabled = false)]
		protected virtual void PMChangeOrderLine_PotentialRevisedAmount_CacheAttached(PXCache sender) { }

		#endregion

		#region POLine

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.")]
		protected virtual void POLine_OrderNbr_CacheAttached(PXCache sender) { }
				
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.")]
		protected virtual void POLine_LineNbr_CacheAttached(PXCache sender) { }

		#endregion

		#region PMChangeOrder

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<PMChangeOrder.classID, PMChangeOrderClass.isRevenueBudgetEnabled>))]
		protected virtual void PMChangeOrder_IsRevenueVisible_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<PMChangeOrder.classID, PMChangeOrderClass.isCostBudgetEnabled>))]
		protected virtual void PMChangeOrder_IsCostVisible_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<PMChangeOrder.classID, PMChangeOrderClass.isPurchaseOrderEnabled>))]
		protected virtual void PMChangeOrder_IsDetailsVisible_CacheAttached(PXCache sender) { }

		#endregion

		#endregion

		[PXCopyPasteHiddenFields(typeof(PMChangeOrder.projectNbr))]
		[PXViewName(PM.Messages.ChangeOrder)]
		public PXSelect<PMChangeOrder> Document;

		public PXSelect<PMChangeOrder, Where<PMChangeOrder.refNbr, Equal<Current<PMChangeOrder.refNbr>>>> DocumentSettings;
		public PXSelect<PMChangeOrder, Where<PMChangeOrder.refNbr, Equal<Current<PMChangeOrder.refNbr>>>> VisibilitySettings;

		[PXCopyPasteHiddenView]
		[PXViewName(PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMChangeOrder.projectID.FromCurrent>> Project;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMChangeOrder.projectID>>>> ProjectProperties;

		[PXCopyPasteHiddenView]
		[PXViewName(AR.Messages.Customer)]
		public PXSetup<Customer>.Where<Customer.bAccountID.IsEqual<PMChangeOrder.customerID.AsOptional>> Customer;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		[PXImport(typeof(PMChangeOrder))]
		[PXFilterable]
		public PXSelectJoin<PMChangeOrderCostBudget, 
			LeftJoin<PMBudget, On<PMBudget.projectID, Equal<PMChangeOrderCostBudget.projectID>,
				And<PMBudget.projectTaskID, Equal<PMChangeOrderCostBudget.projectTaskID>,
				And<PMBudget.accountGroupID, Equal<PMChangeOrderCostBudget.accountGroupID>,
				And<PMBudget.inventoryID, Equal<PMChangeOrderCostBudget.inventoryID>,
				And<PMBudget.costCodeID, Equal<PMChangeOrderCostBudget.costCodeID>>>>>>>,
			Where<PMChangeOrderCostBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderCostBudget.type, Equal<GL.AccountType.expense>>>> CostBudget;
				
		public virtual IEnumerable costBudget()
		{
			List<PXResult<PMChangeOrderCostBudget, PMBudget>> result = new List<PXResult<PMChangeOrderCostBudget, PMBudget>>();
 
			var select = new PXSelect<PMChangeOrderCostBudget,
			Where<PMChangeOrderCostBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderCostBudget.type, Equal<GL.AccountType.expense>>>>(this);

			foreach (PMChangeOrderCostBudget record in select.Select())
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalCostBudget(BudgetKeyTuple.Create(record)) : null;
				if (budget == null) budget = new PMBudget();

				result.Add(new PXResult<PMChangeOrderCostBudget, PMBudget>(record, budget));
			}

			return result;
		}

		[PXImport(typeof(PMChangeOrder))]
		[PXFilterable]
		public PXSelectJoin<PMChangeOrderRevenueBudget,
			LeftJoin<PMRevenueBudget, On<PMRevenueBudget.projectID, Equal<PMChangeOrderRevenueBudget.projectID>,
				And<PMRevenueBudget.projectTaskID, Equal<PMChangeOrderRevenueBudget.projectTaskID>,
				And<PMRevenueBudget.accountGroupID, Equal<PMChangeOrderRevenueBudget.accountGroupID>,
				And<PMRevenueBudget.inventoryID, Equal<PMChangeOrderRevenueBudget.inventoryID>,
				And<PMRevenueBudget.costCodeID, Equal<PMChangeOrderRevenueBudget.costCodeID>>>>>>>,
			Where<PMChangeOrderRevenueBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderRevenueBudget.type, Equal<GL.AccountType.income>>>> RevenueBudget;

		public virtual IEnumerable revenueBudget()
		{
			List<PXResult<PMChangeOrderRevenueBudget, PMBudget>> result = new List<PXResult<PMChangeOrderRevenueBudget, PMBudget>>();

			var select = new PXSelect<PMChangeOrderRevenueBudget,
			Where<PMChangeOrderRevenueBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);

			foreach (PMChangeOrderRevenueBudget record in select.Select())
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(record)) : null;
				if (budget == null) budget = new PMBudget();

				result.Add(new PXResult<PMChangeOrderRevenueBudget, PMBudget>(record, budget));
			}

			return result;
		}

		[PXCopyPasteHiddenView]
		public PXSelect<PMCostBudget> AvailableCostBudget;
		public virtual IEnumerable availableCostBudget()
		{
			HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
			foreach (PXResult<PMChangeOrderCostBudget, PMBudget> res in costBudget())
			{
				existing.Add(BudgetKeyTuple.Create((PMChangeOrderCostBudget)res));
			}

			foreach (PMBudget budget in GetCostBudget() )
			{
				if (budget.Type != GL.AccountType.Expense)
					continue;

				if (existing.Contains(BudgetKeyTuple.Create(budget)))
					budget.Selected = true;

				yield return budget;
			}
		}

		[PXCopyPasteHiddenView]
		public PXSelect<PMRevenueBudget> AvailableRevenueBudget;
		public virtual IEnumerable availableRevenueBudget()
		{
			HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
			foreach (PXResult<PMChangeOrderRevenueBudget, PMBudget> res in revenueBudget())
			{
				existing.Add(BudgetKeyTuple.Create((PMChangeOrderRevenueBudget)res));
			}

			foreach (PMBudget budget in GetRevenueBudget())
			{
				if (budget.Type != GL.AccountType.Income)
					continue;

				if (existing.Contains(BudgetKeyTuple.Create(budget)))
					budget.Selected = true;

				yield return budget;
			}
		}

		[PXImport(typeof(PMChangeOrder))]
		[PXFilterable]
		public PXSelectJoin<PMChangeOrderLine,
			LeftJoin<POLinePM, On<POLinePM.orderType, Equal<PMChangeOrderLine.pOOrderType>, 
				And<POLinePM.orderNbr, Equal<PMChangeOrderLine.pOOrderNbr>, 
				And<POLinePM.lineNbr, Equal<PMChangeOrderLine.pOLineNbr>>>>>,
			Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>>> Details;

		protected Dictionary<POLineKey, POLinePM> polines;
		public IEnumerable details()
		{
			List<PXResult<PMChangeOrderLine, POLinePM>> result = new List<PXResult<PMChangeOrderLine, POLinePM>>(200);

			var select = new PXSelectJoin<PMChangeOrderLine,
				LeftJoin<POLinePM, On<POLinePM.orderType, Equal<PMChangeOrderLine.pOOrderType>,
				And<POLinePM.orderNbr, Equal<PMChangeOrderLine.pOOrderNbr>,
				And<POLinePM.lineNbr, Equal<PMChangeOrderLine.pOLineNbr>>>>>,
				Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>(this);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			if (polines == null || IsCacheUpdateRequired())
			{
				polines = new Dictionary<POLineKey, POLinePM>();
			}


			foreach (PXResult<PMChangeOrderLine, POLinePM> res in select.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				PMChangeOrderLine line = res;
				POLinePM poLine = res;
				if (IsValidKey(line) && poLine.LineNbr == null)
				{
					poLine = GetPOLine(line);
					if (poLine == null)
						poLine = res;
				}
				
				if (poLine.LineNbr != null)
				{
					POLineKey key = GetKey(poLine);
					if (!polines.ContainsKey(key))
					{
						polines.Add(key, poLine);
					}
				}

				result.Add(new PXResult<PMChangeOrderLine, POLinePM>(line, poLine));
			}
			PXView.StartRow = 0;

			return result;
		}

		public PXFilter<POLineFilter> AvailablePOLineFilter;

		[PXCopyPasteHiddenView]
		public PXSelect<POLinePM> AvailablePOLines;

		public IEnumerable availablePOLines()
		{
			List<POLinePM> result = new List<POLinePM>(200);

			HashSet<POLineKey> existing = new HashSet<POLineKey>();
			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (IsValidKey(line))
				{
					existing.Add(GetKey(line));
				}
			}

			var select = new PXSelect<POLinePM,
			Where2<Where<Current<POLineFilter.vendorID>, IsNull, Or<POLinePM.vendorID, Equal<Current<POLineFilter.vendorID>>>>,
			And2<Where<Current<POLineFilter.pOOrderNbr>, IsNull, Or<POLinePM.orderNbr, Equal<Current<POLineFilter.pOOrderNbr>>>>,
			And2<Where<Current<POLineFilter.projectTaskID>, IsNull, Or<POLinePM.taskID, Equal<Current<POLineFilter.projectTaskID>>>>,
			And2<Where<Current<POLineFilter.inventoryID>, IsNull, Or<POLinePM.inventoryID, Equal<Current<POLineFilter.inventoryID>>>>,
			And2<Where<Current<POLineFilter.costCodeFrom>, IsNull, Or<POLinePM.costCodeCD, GreaterEqual<Current<POLineFilter.costCodeFrom>>>>,
			And2<Where<Current<POLineFilter.costCodeTo>, IsNull, Or<POLinePM.costCodeCD, LessEqual<Current<POLineFilter.costCodeTo>>>>,
			And<POLinePM.projectID, Equal<Current<PMChangeOrder.projectID>>,
			And2<Where<POLinePM.cancelled, NotEqual<True>, Or<Current<POLineFilter.includeNonOpen>, Equal<True>>>, 
			And<Where<POLinePM.completed, NotEqual<True>, Or<Current<POLineFilter.includeNonOpen>, Equal<True>>>>>>>>>>>>>(this);
			
			int startRow = PXView.StartRow;
			int totalRows = 0;

			if (polines == null || IsCacheUpdateRequired())
			{
				polines = new Dictionary<POLineKey, POLinePM>();
			}

			foreach (POLinePM line in select.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				if (existing.Contains(GetKey(line)))
					line.Selected = true;
				
				result.Add(line);
			}
			PXView.StartRow = 0;

			return result;
		}
		[PXViewName(Messages.ChangeOrderClass)]
		public PXSetup<PMChangeOrderClass>.Where<PMChangeOrderClass.classID.IsEqual<PMChangeOrder.classID.AsOptional>> ChangeOrderClass;

		[PXViewName(CR.Messages.Answers)]
		public CRAttributeList<PMChangeOrder> Answers;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMChangeOrder.customerID>>>>))]
		[PMDefaultMailTo(typeof(Select2<Contact,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<Contact.bAccountID>, And<Customer.defContactID, Equal<Contact.contactID>>>>,
			Where<Customer.bAccountID, Equal<Current<PMChangeOrder.customerID>>>>))]
		public PMActivityList<PMChangeOrder> Activity;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Approval)]
		public EPApprovalList<PMChangeOrder, PMChangeOrder.approved, PMChangeOrder.rejected> Approval;
		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> Company;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMBudgetAccum> Budget;

		[PXHidden]
		public PXSelect<PMForecastHistoryAccum> ForecastHistory;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<POOrder> Order;


		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<POLine> dummyPOLine; //Added for the sake of Cache_Attached.

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Vendor> dummyVendor;

		public ChangeOrderEntry()
		{
			InitalizeActionsMenu();
			InitializeReportsMenu();
		}

		private IFinPeriodRepository finPeriodsRepo;
		public virtual IFinPeriodRepository FinPeriodRepository
		{
			get
			{
				if (finPeriodsRepo == null)
				{
					finPeriodsRepo = new FinPeriodRepository(this);
				}

				return finPeriodsRepo;
			}
		}

		protected virtual void BeforeCommitHandler(PXGraph e)
		{
			var check1 = _licenseLimits.GetCheckerDelegate<PMChangeOrder>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(PMChangeOrderBudget), (graph) =>
			{
				return new PXDataFieldValue[]
				{
							new PXDataFieldValue<PMChangeOrderBudget.refNbr>(((ChangeOrderEntry)graph).Document.Current?.RefNbr)
				};
			}));

			try
			{
				check1.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseCostBudgetAndRevenueBudget);
			}

			var check2 = _licenseLimits.GetCheckerDelegate<PMChangeOrder>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(PMChangeOrderLine), (graph) =>
			{
				return new PXDataFieldValue[]
				{
							new PXDataFieldValue<PMChangeOrderLine.refNbr>(((ChangeOrderEntry)graph).Document.Current?.RefNbr)
				};
			}));

			try
			{
				check2.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseCommitments);
			}
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += BeforeCommitHandler;
			}
			
		}

		protected ProjectBalance balanceCalculator;

		public virtual ProjectBalance BalanceCalculator
		{
			get
			{
				if (balanceCalculator == null)
				{
					balanceCalculator = new ProjectBalance(this);
				}

				return balanceCalculator;
			}
		}

		#region Actions
		public PXAction<PMChangeOrder> release;
		[PXUIField(DisplayName = GL.Messages.Release)]
		[PXProcessButton]
		public IEnumerable Release(PXAdapter adapter)
		{
			List<PMChangeOrder> list = new List<PMChangeOrder>();

			foreach (PMChangeOrder item in adapter.Get())
			{
				list.Add(item);
			}

			this.Save.Press();

			PXLongOperation.StartOperation(this, delegate () {

				ChangeOrderEntry graph = PXGraph.CreateInstance<ChangeOrderEntry>();
				graph.Document.Current = Document.Current;
				graph.ReleaseDocument(Document.Current);
				polines.Clear();

			});

			return list;
		}

		public PXAction<PMChangeOrder> reverse;
		[PXUIField(DisplayName = Messages.Reverse)]
		[PXProcessButton]
		public IEnumerable Reverse(PXAdapter adapter)
		{
			ReverseDocument();

			return new PMChangeOrder[] { Document.Current };
		}

		public PXAction<PMChangeOrder> approve;
		[PXUIField(DisplayName = "Approve")]
		[PXButton]
		public IEnumerable Approve(PXAdapter adapter)
		{
			List<PMChangeOrder> list = new List<PMChangeOrder>();

			foreach (PMChangeOrder item in adapter.Get())
			{
				list.Add(item);
			}

			if (IsDirty)
			Save.Press();

			foreach (PMChangeOrder item in list)
			{
				try
				{
				if (item.Approved == true)
					continue;

				if (Setup.Current.ChangeOrderApprovalMapID != null)
				{
					if (!Approval.Approve(item))
						throw new PXSetPropertyException(Common.Messages.NotApprover);
					item.Approved = Approval.IsApproved(item);
					if (item.Approved == true)
						item.Status = ChangeOrderStatus.Open;
				}
				else
				{
					item.Approved = true;
					item.Status = ChangeOrderStatus.Open;
				}

				Document.Update(item);

				Save.Press();
				}
				catch (ReasonRejectedException) { }

				yield return item;
			}
		}

		public PXAction<PMChangeOrder> reject;
		[PXUIField(DisplayName = "Reject")]
		[PXButton]
		public IEnumerable Reject(PXAdapter adapter)
		{
			List<PMChangeOrder> list = new List<PMChangeOrder>();

			foreach (PMChangeOrder item in adapter.Get())
			{
				list.Add(item);
			}

			if (IsDirty)
			Save.Press();

			foreach (PMChangeOrder item in list)
			{
				try
				{
				if (item.Approved == true || item.Rejected == true)
					continue;

				if (Setup.Current.ChangeOrderApprovalMapID != null)
				{
					if (!Approval.Reject(item))
						throw new PXSetPropertyException(Common.Messages.NotApprover);
					item.Rejected = true;
				}
				else
				{
					item.Rejected = true;
				}

				item.Status = ChangeOrderStatus.Rejected;
				Document.Update(item);

				Save.Press();
				}
				catch (ReasonRejectedException) { }

				yield return item;
			}
		}

		public PXAction<PMChangeOrder> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> coReport;
		[PXUIField(DisplayName = "Print Change Order", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable COReport(PXAdapter adapter)
		{
			OpenReport(ChangeOrderReport, Document.Current);

			return adapter.Get();
		}

		public virtual void OpenReport(string reportID, PMChangeOrder doc)
		{
			if (doc != null && Document.Cache.GetStatus(doc) != PXEntryStatus.Inserted)
			{
				var utility = new CR.NotificationUtility(this);
				string specificReportID = utility.SearchReport(PMNotificationSource.Project, Project.Current, reportID, Project.Current.DefaultBranchID);

				var parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = doc.RefNbr;
				throw new PXReportRequiredException(parameters, specificReportID, specificReportID);
			}
		}

		public PXAction<PMChangeOrder> send;
		[PXUIField(DisplayName = "Email Change Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Send(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				PXLongOperation.StartOperation(this, delegate () {
					SendReport(ChangeOrderNotificationCD, Document.Current);
				});
			}

			return adapter.Get();
		}

		public virtual void SendReport(string notificationCD, PMChangeOrder doc)
		{
			if (doc != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["RefNbr"] = Document.Current.RefNbr;

				using (var ts = new PXTransactionScope())
				{
					Activity.SendNotification(PMNotificationSource.Project, notificationCD, Project.Current.DefaultBranchID, mailParams);
					this.Save.Press();

					ts.Complete();
				}
			}
		}

		public PXAction<PMChangeOrder> addCostBudget;
		[PXUIField(DisplayName = "Select Budget Lines")]
		[PXButton]
		public IEnumerable AddCostBudget(PXAdapter adapter)
		{
			if (AvailableCostBudget.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedCostBudget();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> appendSelectedCostBudget;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedCostBudget(PXAdapter adapter)
		{
			AddSelectedCostBudget();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> addRevenueBudget;
		[PXUIField(DisplayName = "Select Budget Lines")]
		[PXButton]
		public IEnumerable AddRevenueBudget(PXAdapter adapter)
		{
			if (AvailableRevenueBudget.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedRevenueBudget();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> appendSelectedRevenueBudget;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedRevenueBudget(PXAdapter adapter)
		{
			AddSelectedRevenueBudget();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> addPOLines;
		[PXUIField(DisplayName = "Select Commitments")]
		[PXButton]
		public IEnumerable AddPOLines(PXAdapter adapter)
		{
			if (AvailablePOLines.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedPOLines();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> appendSelectedPOLines;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedPOLines(PXAdapter adapter)
		{
			AddSelectedPOLines();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCommitments;
		[PXUIField(DisplayName = Messages.ViewCommitments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public IEnumerable ViewCommitments(PXAdapter adapter)
		{
			if (Details.Current != null && !string.IsNullOrEmpty(Details.Current.POOrderNbr))
			{
				POOrderEntry target = PXGraph.CreateInstance<POOrderEntry>();
				target.Document.Current = PXSelect<POOrder, Where<POOrder.orderType, Equal<Current<PMChangeOrderLine.pOOrderType>>,
					And<POOrder.orderNbr, Equal<Current<PMChangeOrderLine.pOOrderNbr>>>>>.Select(this);

				throw new PXRedirectRequiredException(target, true, Messages.ViewCommitments) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewChangeOrder;
		[PXUIField(DisplayName = Messages.ViewChangeOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]

		public virtual IEnumerable ViewChangeOrder(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.OrigRefNbr != null)
			{
				
				ChangeOrderEntry target = PXGraph.CreateInstance<ChangeOrderEntry>();
				target.Clear(PXClearOption.ClearAll);
				target.SelectTimeStamp();
				target.Document.Current = PXSelect<PMChangeOrder, Where<PMChangeOrder.refNbr, Equal<Required<PMChangeOrder.origRefNbr>>>>.Select(this, Document.Current.OrigRefNbr);
				throw new PXRedirectRequiredException(target, true, "View Change Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewRevenueBudgetTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRevenueBudgetTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMChangeOrderRevenueBudget.projectTaskID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMChangeOrder> viewCostBudgetTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCostBudgetTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMChangeOrderCostBudget.projectTaskID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMChangeOrder> viewCommitmentTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCommitmentTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMChangeOrderLine.taskID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMChangeOrder> viewRevenueBudgetInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRevenueBudgetInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMChangeOrderRevenueBudget.inventoryID>>>>.Select(this);
			if (item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCostBudgetInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCostBudgetInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMChangeOrderCostBudget.inventoryID>>>>.Select(this);
			if (item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCommitmentInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCommitmentInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMChangeOrderLine.inventoryID>>>>.Select(this);
			if (item != null && item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		#region PMChangeOrder
		protected virtual void _(Events.RowSelected<PMChangeOrder> e)
		{
			//unconditional visibility for Copy-Paste to work properly:
			//The Copy-Paste export creates a new PMChangeOrder instance with default settings and extracts the UI containers from the Page.
			//Do not set PXUIVisibility.Invisible to columns that we intent to export via Copy-Paste:

			bool enforceVisibility = false;
			if (e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				if (e.Row.ProjectID == null && string.IsNullOrEmpty(e.Row.Description))
				{
					//there is very high probablity that this instance was created by the system to extract the containers on UI.
					enforceVisibility = true;
				}
			}

			Details.Cache.AllowSelect = Setup.Current.CostCommitmentTracking == true;

			approve.SetEnabled(e.Row?.Status == ChangeOrderStatus.PendingApproval);
			reject.SetEnabled(e.Row?.Status == ChangeOrderStatus.PendingApproval);
			release.SetEnabled(e.Row?.Approved == true && e.Row.Released != true);
			reverse.SetEnabled(e.Row?.Released == true);

			string budgetLevel = BudgetLevels.Task;
			if (Project.Current != null)
			{
				budgetLevel = Project.Current.BudgetLevel;
			}

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetVisible<PMChangeOrder.reverseStatus>(e.Cache, e.Row, e.Row.ReverseStatus != ChangeOrderReverseStatus.None);

				PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.inventoryID>(CostBudget.Cache, null, !CostCodeAttribute.UseCostCode() || budgetLevel == BudgetLevels.Item || budgetLevel == BudgetLevels.Detail);
				
				PXUIVisibility costCodeIDVisibility = CostCodeAttribute.UseCostCode() && (budgetLevel == BudgetLevels.CostCode || budgetLevel == BudgetLevels.Detail || IsCopyPasteContext || IsExport || IsImport || enforceVisibility) ? PXUIVisibility.Visible : PXUIVisibility.Invisible;

				PXUIFieldAttribute.SetVisible<PMChangeOrderRevenueBudget.inventoryID>(RevenueBudget.Cache, null, budgetLevel == BudgetLevels.Item || budgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetVisible<PMChangeOrderRevenueBudget.costCodeID>(RevenueBudget.Cache, null, CostCodeAttribute.UseCostCode() && (budgetLevel == BudgetLevels.CostCode || budgetLevel == BudgetLevels.Detail));
				PXUIFieldAttribute.SetVisibility<PMChangeOrderRevenueBudget.costCodeID>(RevenueBudget.Cache, null, costCodeIDVisibility);
				
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedInvoicedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedInvoicedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedOpenAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedOpenQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedReceivedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedCOQty> (Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedCOAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.committedCOQty>(CostBudget.Cache, null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.committedCOAmount>(CostBudget.Cache, null, Setup.Current.CostCommitmentTracking == true);

				PXUIFieldAttribute.SetVisibility<PMBudget.curyCommittedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.curyCommittedInvoicedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedInvoicedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.curyCommittedOpenAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedOpenQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedReceivedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

				bool isEditable = CanEditDocument(e.Row);

				Document.Cache.AllowDelete = isEditable;

				addRevenueBudget.SetEnabled(isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true);
				RevenueBudget.Cache.AllowInsert = isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true;
				RevenueBudget.Cache.AllowUpdate = isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true;
				RevenueBudget.Cache.AllowDelete = isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true;

				addCostBudget.SetEnabled(isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true);
				CostBudget.Cache.AllowInsert = isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true;
				CostBudget.Cache.AllowUpdate = isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true;
				CostBudget.Cache.AllowDelete = isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true;

				addPOLines.SetEnabled(isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true);
				Details.Cache.AllowInsert = isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true;
				Details.Cache.AllowUpdate = isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true;
				Details.Cache.AllowDelete = isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true;
				
				Answers.Cache.AllowInsert = isEditable;
				Answers.Cache.AllowUpdate = isEditable;
				Answers.Cache.AllowDelete = isEditable;

				PXUIFieldAttribute.SetVisible<PMChangeOrder.origRefNbr>(e.Cache, e.Row, e.Row.OrigRefNbr != null);

				PXUIFieldAttribute.SetEnabled<PMChangeOrder.hold>(e.Cache, e.Row, e.Row.Released != true);
				if (!this.IsContractBasedAPI)
				{
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.classID>(e.Cache, e.Row, isEditable);
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.projectID>(e.Cache, e.Row, isEditable && IsProjectEnabled());
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.description>(e.Cache, e.Row, isEditable);
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.completionDate>(e.Cache, e.Row, isEditable);
				}
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.extRefNbr>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.projectNbr>(e.Cache, e.Row, isEditable && e.Row.IsRevenueVisible == true && e.Row.ProjectNbr != Messages.NotAvailable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.date>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.delayDays>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.text>(e.Cache, e.Row, isEditable);

			}
		}

		protected virtual void _(Events.RowDeleted<PMChangeOrder> e)
		{
			PMChangeOrder deletedOrder = e.Row;

			if (Project.Current != null && Project.Current.LastChangeOrderNumber == deletedOrder.ProjectNbr)
			{
				Project.Current.LastChangeOrderNumber = DecNumber(Project.Current.LastChangeOrderNumber);
				ProjectProperties.Update(Project.Current);
			}

			UpdateOriginalReverseStatus(e.Cache, deletedOrder);
		}

		private void UpdateOriginalReverseStatus(PXCache cache, PMChangeOrder order)
		{
			if (order.ReverseStatus == ChangeOrderReverseStatus.Reversal)
			{
				PMChangeOrder originalOrder = SelectFrom<PMChangeOrder>
					.Where<PMChangeOrder.refNbr.IsEqual<PMChangeOrder.origRefNbr.FromCurrent>>
					.View
					.Select(this);

				// Has reversing orders, except current.
				bool originalHasReversing = SelectFrom<PMChangeOrder>
					.Where
						<PMChangeOrder.origRefNbr.IsEqual<@P.AsString>.And
						<PMChangeOrder.refNbr.IsNotEqual<@P.AsString>>>
					.View
					.Select(this, originalOrder.RefNbr, order.RefNbr)
					.Count > 0;
										
				if (originalHasReversing)
				{
					originalOrder.ReverseStatus = ChangeOrderReverseStatus.Reversed;
				}
				else if (!string.IsNullOrWhiteSpace(originalOrder.OrigRefNbr))
				{
					// If original order has his own original.
					originalOrder.ReverseStatus = ChangeOrderReverseStatus.Reversal;
				}
				else
				{
					originalOrder.ReverseStatus = ChangeOrderReverseStatus.None;
				}

				cache.Update(originalOrder);

				// Without this action, TimeStamp uses the value from the row that is being deleted,
				// which leads to the error "Data was updated by another operation".
				PXDBTimestampAttribute timestampAttribute = cache
					.GetAttributesOfType<PXDBTimestampAttribute>(null, nameof(PMChangeOrder.tstamp))
					.First();

				timestampAttribute.RecordComesFirst = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.hold> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Hold == true)
				{
					Approval.Reset(e.Row);
					e.Cache.SetValue<PMChangeOrder.approved>(e.Row, false);
					e.Cache.SetValue<PMChangeOrder.rejected>(e.Row, false);
					e.Cache.SetValue<PMChangeOrder.status>(e.Row, ProformaStatus.OnHold);
				}
				else
				{
					if (Setup.Current.ChangeOrderApprovalMapID != null)
					{

						Approval.Assign(e.Row, Setup.Current.ChangeOrderApprovalMapID, Setup.Current.ChangeOrderApprovalNotificationID);
						if (true == (bool?)e.Cache.GetValue<PMChangeOrder.approved>(e.Row))
						{
							e.Cache.SetValue<PMChangeOrder.status>(e.Row, ChangeOrderStatus.Open);
						}
						else
						{
							e.Cache.SetValue<PMChangeOrder.status>(e.Row, ChangeOrderStatus.PendingApproval);
						}
					}
					else
					{
						e.Cache.SetValue<PMChangeOrder.approved>(e.Row, true);
						e.Cache.SetValue<PMChangeOrder.status>(e.Row, ChangeOrderStatus.Open);
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrder, PMChangeOrder.projectNbr> e)
		{			
			PMChangeOrderClass orderClass = GetChangeOrderClass(e.Row.ClassID);
			if (orderClass != null && e.Row.ProjectID != null && orderClass.IncrementsProjectNumber == true)
			{
				PMProject project = GetProject(e.Row.ProjectID);
				string lastNumber = string.IsNullOrEmpty(project?.LastChangeOrderNumber) ? "0000" : project.LastChangeOrderNumber;

				if (!char.IsDigit(lastNumber[lastNumber.Length - 1]))
				{
					lastNumber = string.Format("{0}0000", lastNumber);
				}

				e.NewValue = ARDiscountSequenceMaint.IncNumber(lastNumber, 1);
			}
			else
			{
				e.NewValue = Messages.NotAvailable;
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.projectNbr> e)
		{
			string val = (string)e.NewValue;

			if (val.Equals(Messages.NotAvailable, StringComparison.InvariantCultureIgnoreCase))
				return;

			var selectDuplicate = new PXSelect<PMChangeOrder, Where<PMChangeOrder.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeOrder.projectNbr, Equal<Required<PMChangeOrder.projectNbr>>,
				And<PMChangeOrder.reverseStatus, NotEqual<ChangeOrderReverseStatus.reversed>>>>>(this);

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
			{
				selectDuplicate.WhereAnd<Where<PMChangeOrder.refNbr, NotEqual<Current<PMChangeOrder.refNbr>>>>();
			}

			PMChangeOrder duplicate = selectDuplicate.Select(e.NewValue);

			if (duplicate != null)
			{
				throw new PXSetPropertyException(Messages.DuplicateChangeOrderNumber, duplicate.RefNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.projectNbr> e)
		{
			PMProject project = GetProject(e.Row.ProjectID);
			if (project != null && e.Row.ProjectNbr != Messages.NotAvailable)
			{
				if (string.Compare(e.Row.ProjectNbr, project.LastChangeOrderNumber, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					project.LastChangeOrderNumber = e.Row.ProjectNbr;
					ProjectProperties.Update(project);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.projectID> e)
		{
			PMProject oldProject = GetProject((int?)e.OldValue);
			if (oldProject != null && oldProject.LastChangeOrderNumber == e.Row.ProjectNbr)
			{
				oldProject.LastChangeOrderNumber = DecNumber(oldProject.LastChangeOrderNumber);
				ProjectProperties.Update(oldProject);
			}
			e.Cache.SetDefaultExt<PMChangeOrder.projectNbr>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.classID> e)
		{
			PMProject project = GetProject(e.Row.ProjectID);

			PMChangeOrderClass oldClass = GetChangeOrderClass((string)e.OldValue);
			PMChangeOrderClass newClass = GetChangeOrderClass(e.Row.ClassID);

			if (oldClass != null && newClass != null && oldClass.IncrementsProjectNumber != newClass.IncrementsProjectNumber)
			{
				if (project != null && oldClass.IncrementsProjectNumber == true && project.LastChangeOrderNumber == e.Row.ProjectNbr)
				{
					project.LastChangeOrderNumber = DecNumber(Project.Current.LastChangeOrderNumber);
					ProjectProperties.Update(project);
				}

				e.Cache.SetDefaultExt<PMChangeOrder.projectNbr>(e.Row);
			}
			else if (oldClass == null && newClass != null)
			{
				e.Cache.SetDefaultExt<PMChangeOrder.projectNbr>(e.Row);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.classID> e)
		{
			if (!string.IsNullOrEmpty(e.Row.ClassID))
			{
				PMChangeOrderClass newClass = PXSelect<PMChangeOrderClass, Where<PMChangeOrderClass.classID, Equal<Required<PMChangeOrderClass.classID>>>>.Select(this, e.NewValue);
				if (newClass != null)
				{
					if (newClass.IsRevenueBudgetEnabled != true)
					{
						if (RevenueBudget.Select().Count > 0)
						{
							throw new PXSetPropertyException<PMChangeOrder.classID>(Messages.ChangeOrderContainsRevenueBudget);
						}
					}

					if (newClass.IsCostBudgetEnabled != true)
					{
						if (CostBudget.Select().Count > 0)
						{
							throw new PXSetPropertyException<PMChangeOrder.classID>(Messages.ChangeOrderContainsCostBudget);
						}
					}

					if (newClass.IsPurchaseOrderEnabled != true)
					{
						if (Details.Select().Count > 0)
						{
							throw new PXSetPropertyException<PMChangeOrder.classID>(Messages.ChangeOrderContainsDetails);
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.date> e)
		{
			if (e.NewValue != null)
			{
				Func<PXGraph, IFinPeriodRepository> factoryFunc = (Func<PXGraph, IFinPeriodRepository>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, IFinPeriodRepository>));
				IFinPeriodRepository service = factoryFunc(this);

				DateTime? date = (DateTime?)e.NewValue;
				string strDate = string.Empty;
				if (date != null)
				{
					strDate = date.Value.ToShortDateString();
				}
				if (service != null)
				{
					try
					{						
						var finperiod = service.GetFinPeriodByDate(date, PXAccess.GetParentOrganizationID(Accessinfo.BranchID));

						if (finperiod == null)
						{
							throw new PXSetPropertyException(Messages.ChnageOrderInvalidDate, strDate);
						}
					}
					catch (PXException ex)
					{
						throw new PXSetPropertyException(ex, PXErrorLevel.Error, Messages.ChnageOrderInvalidDate, strDate);
					}
				}
			}
		}
		#endregion

		#region PMChangeOrderCostBudget
		protected virtual void _(Events.RowSelected<PMChangeOrderCostBudget> e)
		{
			InitCostBudgetFields(e.Row);
			PMBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;

			PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.uOM>(e.Cache, e.Row, budget == null);
        }

		protected virtual void _(Events.RowInserting<PMChangeOrderCostBudget> e)
		{
			InitCostBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowInserted<PMChangeOrderCostBudget> e)
		{
			AddToDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}
				
		protected virtual void _(Events.RowUpdating<PMChangeOrderCostBudget> e)
		{
			InitCostBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowUpdated<PMChangeOrderCostBudget> e)
		{
			AddToDraftBucket(e.OldRow, -1);
			AddToDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.RowDeleted<PMChangeOrderCostBudget> e)
		{
			AddToDraftBucket(e.Row, -1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.costCodeID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.inventoryID> e)
		{
			e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.accountGroupID> e)
		{
			if (e.Row == null) return;
			if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = PXSelectorAttribute.Select<PMChangeOrderCostBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
				if (item != null)
				{
					Account account = PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Caches[typeof(InventoryItem)], item) as Account;
					if (account != null && account.AccountGroupID != null)
					{
						e.NewValue = account.AccountGroupID;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.projectTaskID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.accountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.uOM> e)
		{
			PMCostBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.UOM;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.rate> e)
		{
			PMCostBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.CuryUnitRate;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.description> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null)
				{
					PMCostBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;
					if (budget != null)
					{
						e.NewValue = budget.Description; 
					}
					else
					{
						PMCostCode costCode = GetCostCode(e.Row.CostCodeID);
						if (costCode != null)
						{
							e.NewValue = costCode.Description;
						}
					}
				}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = GetInventoryItem(e.Row.InventoryID);
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
		#endregion

		#region PMChangeOrderRevenueBudget
		protected virtual void _(Events.RowSelected<PMChangeOrderRevenueBudget> e)
		{
			InitRevenueBudgetFields(e.Row);
			PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
			PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.uOM>(e.Cache, e.Row, budget == null);
		}

		protected virtual void _(Events.RowInserting<PMChangeOrderRevenueBudget> e)
		{
			InitRevenueBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowInserted<PMChangeOrderRevenueBudget> e)
		{
			AddToDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}
		
		protected virtual void _(Events.RowUpdating<PMChangeOrderRevenueBudget> e)
		{
			InitRevenueBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowDeleted<PMChangeOrderRevenueBudget> e)
		{
			AddToDraftBucket(e.Row, -1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.RowUpdated<PMChangeOrderRevenueBudget> e)
		{
			AddToDraftBucket(e.OldRow, -1);
			AddToDraftBucket(e.Row, 1);
			RemoveObsoleteLines();

			if (IsValidKey(e.Row) && !IsValidKey(e.OldRow))
			{
				RevenueBudget.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.costCodeID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.inventoryID> e)
		{
			e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.accountGroupID> e)
		{
			if (e.Row == null) return;

			var select = new PXSelect<PMAccountGroup, Where<PMAccountGroup.type, Equal<GL.AccountType.income>>>(this);

			var resultset = select.SelectWindowed(0, 2);

			if (resultset.Count == 1)
			{
				e.NewValue = ((PMAccountGroup)resultset).GroupID;
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMChangeOrderRevenueBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						Account account = PXSelectorAttribute.Select<InventoryItem.salesAcctID>(Caches[typeof(InventoryItem)], item) as Account;
						if (account != null && account.AccountGroupID != null)
						{
							e.NewValue = account.AccountGroupID;
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.projectTaskID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.accountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.uOM> e)
		{
			PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.UOM;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.rate> e)
		{
			PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.CuryUnitRate;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.description> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null)
				{
					PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
					if (budget != null)
					{
						e.NewValue = budget.Description;
					}
					else
					{
						PMCostCode costCode = GetCostCode(e.Row.CostCodeID);
						if (costCode != null)
						{
							e.NewValue = costCode.Description;
						}
					}
				}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = GetInventoryItem(e.Row.InventoryID);
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
		#endregion

		#region PMChangeOrderLine
		protected virtual void _(Events.RowUpdated<PMChangeOrderLine> e)
		{
			bool isReopen = false;

			if (e.Row.POOrderNbr != e.OldRow.POOrderNbr)
			{
				if (!string.IsNullOrEmpty(e.OldRow.POOrderNbr))
				{
					// POOrderNbr was changed from one value to another
					e.Row.POLineNbr = null;
					e.Row.VendorID = null;
					e.Row.CuryID = null;
					e.Row.AccountID = null;
				}

				POOrderPM poOrder = PXSelect<POOrderPM, Where<POOrderPM.orderType, Equal<Required<POOrderPM.orderType>>,
					And<POOrderPM.orderNbr, Equal<Required<POOrderPM.orderNbr>>>>>.Select(this, e.Row.POOrderType, e.Row.POOrderNbr);

				if (poOrder != null)
				{
					e.Row.VendorID = poOrder.VendorID;
					e.Row.CuryID = poOrder.CuryID;
					e.Row.AccountID = DefaultAccountID(e.Row);
				}

			}
			else if (e.Row.POLineNbr != e.OldRow.POLineNbr)
			{
				if (IsValidKey(e.Row))
				{
					POLinePM poLine = GetPOLine(e.Row);
					if (poLine != null)
					{
						e.Row.TaskID = poLine.TaskID;
						e.Row.UOM = poLine.UOM;
						e.Row.VendorID = poLine.VendorID;
						e.Row.CostCodeID = poLine.CostCodeID;
						e.Row.InventoryID = poLine.InventoryID;
						e.Row.UnitCost = poLine.CuryUnitCost;
						e.Row.CuryID = poLine.CuryID;
						e.Row.AccountID = poLine.ExpenseAcctID;

						e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);

						isReopen = poLine.Completed == true || poLine.Cancelled == true;
					}
				}
			}

			if (IsValidKey(e.Row))
			{
				e.Row.LineType = isReopen ? ChangeOrderLineType.Reopen : ChangeOrderLineType.Update;
			}
			else if (!string.IsNullOrEmpty(e.Row.POOrderNbr))
			{
				e.Row.LineType = ChangeOrderLineType.NewLine;
			}
			else
			{
				e.Row.LineType = ChangeOrderLineType.NewDocument;
			}
		}

		protected virtual void _(Events.RowSelected<PMChangeOrderLine> e)
		{
			InitDetailLineFields(e.Row);

			if (e.Row != null)
			{
				bool referencesPOLine = e.Row.POLineNbr != null;
				bool referencesPOOrder = !string.IsNullOrEmpty(e.Row.POOrderNbr);

				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.taskID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.inventoryID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.costCodeID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.vendorID>(e.Cache, e.Row, !referencesPOOrder);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.curyID>(e.Cache, e.Row, !referencesPOOrder);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.uOM>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.accountID>(e.Cache, e.Row, !referencesPOLine);
			}
		}

		protected virtual void _(Events.RowInserting<PMChangeOrderLine> e)
		{
			InitDetailLineFields(e.Row);
		}

		protected virtual void _(Events.RowUpdating<PMChangeOrderLine> e)
		{
			InitDetailLineFields(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.accountID> e)
		{
			int? defaultValue = DefaultAccountID(e.Row);
			if (defaultValue != null)
			{
				e.NewValue = defaultValue;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.vendorID> e)
		{
			if (e.Row.AccountID == null)
			{
				e.Cache.SetDefaultExt<PMChangeOrderLine.accountID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderLine.accountID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.accountID> e)
		{
			if (e.Row.AccountID != null)
				e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.pOOrderNbr> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.pOLineNbr> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderLine, PMChangeOrderLine.qty> e)
		{
			decimal? newValue = (decimal?)e.NewValue;
			if (newValue < 0)
			{
				if (!IsValidKey(e.Row))
				{
					throw new PXSetPropertyException(Messages.NewCommitmentIsNegative);
				}
				else
				{
					POLinePM poLine = GetPOLine(e.Row);
					if (poLine != null && poLine.CalcOpenQty < Math.Abs(newValue.Value))
					{
						throw new PXSetPropertyException(Messages.CommitmentCannotbeDecreased);
					}

				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderLine, PMChangeOrderLine.amount> e)
		{
			decimal? newValue = (decimal?)e.NewValue;
			if (newValue < 0)
			{
				if (!IsValidKey(e.Row))
				{
					throw new PXSetPropertyException(Messages.NewCommitmentIsNegative);
				}
				else
				{
					POLinePM poLine = GetPOLine(e.Row);
					if (poLine != null && poLine.CuryUnbilledAmt.GetValueOrDefault() < Math.Abs(newValue.Value))
					{
						throw new PXSetPropertyException(Messages.CommitmentCannotbeDecreased);
					}

				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.amount> e)
		{
			e.Cache.SetValueExt<PMChangeOrderLine.amountInProjectCury>(e.Row, ConvertToProjectCury(e.Row));
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.curyID> e)
		{
			e.Cache.SetValueExt<PMChangeOrderLine.amountInProjectCury>(e.Row, ConvertToProjectCury(e.Row));
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.description> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null)
				{
					PMCostBudget budget = null;
					if (e.Row.TaskID != null && e.Row.AccountID != null)
					{
						BudgetKeyTuple key = new BudgetKeyTuple(e.Row.ProjectID.GetValueOrDefault(), e.Row.TaskID.GetValueOrDefault(), GetProjectedAccountGroup(e.Row).GetValueOrDefault(), e.Row.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID), e.Row.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
						budget = GetOriginalCostBudget(key);
					}

					if (budget != null)
					{
						e.NewValue = budget.Description;
					}
					else
					{
						PMCostCode costCode = GetCostCode(e.Row.CostCodeID);
						if (costCode != null)
						{
							e.NewValue = costCode.Description;
						}
					}
				}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = GetInventoryItem(e.Row.InventoryID);
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
		#endregion

		protected virtual void _(Events.FieldUpdated<POLineFilter, POLineFilter.vendorID> e)
		{
			if (e.Row.VendorID != null)
			{
				e.Row.POOrderNbr = null;
			}
		}

		#endregion

		public override void Persist()
		{
			var selectOrder = new PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
						And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>(this);

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.POLineNbr != null)
				{
					POOrder order = selectOrder.Select(line.POOrderType, line.POOrderNbr);
					if (order != null && order.Behavior == POBehavior.Standard)
					{
						order.Behavior = POBehavior.ChangeOrder;
						Order.Update(order);
					}
				}
			}

			//Clear lookup grids (updated only due to Selected field being changed); otherwise it will messup Budget Acumulator:
			AvailableCostBudget.Cache.Clear();
			AvailableRevenueBudget.Cache.Clear();

			base.Persist();
		}

		protected string draftChangeOrderBudgetStatsKey;
		protected Dictionary<BudgetKeyTuple, decimal> draftChangeOrderBudgetStats;
		public virtual Dictionary<BudgetKeyTuple, decimal> BuildBudgetStatsOnDraftChangeOrders()
		{
			Dictionary<BudgetKeyTuple, decimal> drafts = new Dictionary<BudgetKeyTuple, decimal>();

			var select = new PXSelectGroupBy<PMChangeOrderBudget, Where<PMChangeOrderBudget.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeOrderBudget.released, Equal<False>, 
				And<PMChangeOrderBudget.refNbr, NotEqual<Current<PMChangeOrder.refNbr>>>>>,
				Aggregate<GroupBy<PMChangeOrderBudget.projectID,
				GroupBy<PMChangeOrderBudget.projectTaskID,
				GroupBy<PMChangeOrderBudget.accountGroupID,
				GroupBy<PMChangeOrderBudget.inventoryID,
				GroupBy<PMChangeOrderBudget.costCodeID,
				Sum<PMChangeOrderBudget.amount>>>>>>>>(this);

			using (new PXFieldScope(select.View, typeof(PMChangeOrderBudget.projectID), typeof(PMChangeOrderBudget.projectTaskID)
				, typeof(PMChangeOrderBudget.accountGroupID), typeof(PMChangeOrderBudget.inventoryID), typeof(PMChangeOrderBudget.costCodeID)
				, typeof(PMChangeOrderBudget.amount), typeof(PMChangeOrderBudget.amount)))
			{
				foreach(PMChangeOrderBudget record in select.Select())
				{
					drafts.Add(BudgetKeyTuple.Create(record), record.Amount.GetValueOrDefault());
				}
			}

			return drafts;
		}

		public virtual decimal GetDraftChangeOrderBudgetAmount(PMChangeOrderBudget record)
		{
			if (!IsValidKey(record))
				return 0;


			if (draftChangeOrderBudgetStats == null || draftChangeOrderBudgetStatsKey != record.RefNbr)
			{
				draftChangeOrderBudgetStats = BuildBudgetStatsOnDraftChangeOrders();
				draftChangeOrderBudgetStatsKey = record.RefNbr;
			}
			
			decimal result = 0;
			draftChangeOrderBudgetStats.TryGetValue(BudgetKeyTuple.Create(record), out result);

			return result;
		}

		
		public virtual int? DefaultAccountID(PMChangeOrderLine line)
		{
			int? accountID = null;
			if (line.InventoryID != null && line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = PXSelectorAttribute.Select<PMChangeOrderLine.inventoryID>(Details.Cache, line) as InventoryItem;
				if (item != null )
				{
					Account expenseAccount = PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Caches[typeof(InventoryItem)], item) as Account;
					if (expenseAccount != null && expenseAccount.AccountGroupID != null)
					{
						accountID = expenseAccount.AccountID;
					}
				}
			}

			if (accountID == null && line.VendorID != null)
			{
				Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, line.VendorID);
				if (vendor != null)
				{
					Location location = PXSelect<Location, Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(this, vendor.DefLocationID);
					if (location != null)
					{
						Account expenseAccount = PXSelectorAttribute.Select<Location.vExpenseAcctID>(Caches[typeof(Location)], location) as Account;
						if (expenseAccount != null && expenseAccount.AccountGroupID != null)
						{
							accountID = expenseAccount.AccountID;
						}
					}
				}
			}

			return accountID;
		}

		private POLineKey GetKey(POLinePM record)
		{
			return new POLineKey(record.OrderType, record.OrderNbr, record.LineNbr.Value);
		}

		private POLineKey GetKey(PMChangeOrderLine record)
		{
			return new POLineKey(record.POOrderType, record.POOrderNbr, record.POLineNbr.Value);
		}

		private bool IsValidKey(PMChangeOrderBudget record)
		{
			if (record == null)
				return false;

			if (record.CostCodeID == null)
				return false;

			if (record.InventoryID == null)
				return false;

			if (record.AccountGroupID == null)
				return false;

			if (record.TaskID == null)
				return false;

			if (record.ProjectID == null)
				return false;

			return true;

		}

		private bool IsValidKey(PMChangeOrderLine record)
		{
			if(record == null)
				return false;

			if (record.POLineNbr == null)
				return false;

			if (string.IsNullOrEmpty(record.POOrderNbr))
				return false;

			if (string.IsNullOrEmpty(record.POOrderType))
				return false;

			return true;
		}
		
		protected Dictionary<BudgetKeyTuple, PMCostBudget> costBudgets;
		protected Dictionary<BudgetKeyTuple, PMChangeOrderBudget> previousTotals;
		public virtual Dictionary<BudgetKeyTuple, PMCostBudget> BuildCostBudgetLookup()
		{
			Dictionary<BudgetKeyTuple, PMCostBudget> result = new Dictionary<BudgetKeyTuple, PMCostBudget>();

			var select = new PXSelectReadonly<PMCostBudget, Where<PMCostBudget.projectID, Equal<Current<PMChangeOrder.projectID>>, And<PMCostBudget.type, Equal<GL.AccountType.expense>>>>(this);

			foreach(PMCostBudget record in select.Select())
			{
				result.Add(BudgetKeyTuple.Create(record), record);
			}

			return result;
		}

		protected Dictionary<BudgetKeyTuple, PMRevenueBudget> revenueBudgets;
		public virtual Dictionary<BudgetKeyTuple, PMRevenueBudget> BuildRevenueBudgetLookup()
		{
			Dictionary<BudgetKeyTuple, PMRevenueBudget> result = new Dictionary<BudgetKeyTuple, PMRevenueBudget>();

			var select = new PXSelectReadonly<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Current<PMChangeOrder.projectID>>, And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);

			foreach (PMRevenueBudget record in select.Select())
			{
				result.Add(BudgetKeyTuple.Create(record), record);
			}

			return result;
		}
				
		public virtual Dictionary<BudgetKeyTuple, PMChangeOrderBudget> BuildPreviousTotals()
		{
			Dictionary<BudgetKeyTuple, PMChangeOrderBudget> result = new Dictionary<BudgetKeyTuple, PMChangeOrderBudget>();

			var select = new PXSelectJoinGroupBy<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderBudget.refNbr>>>,
				Where<PMChangeOrderBudget.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeOrder.released, Equal<True>,
				And<PMChangeOrder.reverseStatus, Equal<ChangeOrderReverseStatus.none>>>>,
				Aggregate<GroupBy<PMChangeOrderBudget.projectID,
				GroupBy<PMChangeOrderBudget.projectTaskID,
				GroupBy<PMChangeOrderBudget.accountGroupID,
				GroupBy<PMChangeOrderBudget.inventoryID,
				GroupBy<PMChangeOrderBudget.costCodeID,
				Sum<PMChangeOrderBudget.qty,
				Sum<PMChangeOrderBudget.amount>>>>>>>>>(this);

			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
			{
				select.WhereAnd<Where<PMChangeOrderBudget.refNbr, Less<Current<PMChangeOrder.refNbr>>>>();
			}

			foreach (PMChangeOrderBudget record in select.Select())
			{
				result.Add(BudgetKeyTuple.Create(record), record);
			}

			return result;
		}

		
		public virtual POLinePM GetPOLine(PMChangeOrderLine line)
		{
			POLinePM result = null;

			if (IsValidKey(line))
			{
				POLineKey key = GetKey(line);

				if (polines != null)
				{
					polines.TryGetValue(key, out result);
				}
				else
				{
					polines = new Dictionary<POLineKey, POLinePM>();
				}

				if (result == null)
				{
					result = PXSelect<POLinePM, Where<POLinePM.orderType, Equal<Required<POLinePM.orderType>>,
						And<POLinePM.orderNbr, Equal<Required<POLinePM.orderNbr>>,
						And<POLinePM.lineNbr, Equal<Required<POLinePM.lineNbr>>>>>>.Select(this, key.OrderType, key.OrderNbr, key.LineNbr);

					if (result != null)
					{
						polines.Add(key, result);
					}
				}
			}			

			return result;
		}

		public virtual PMCostBudget GetOriginalCostBudget(BudgetKeyTuple record)
		{
			if (costBudgets == null || IsCacheUpdateRequired())
			{
				costBudgets = BuildCostBudgetLookup();
			}

			PMCostBudget result = null;
			costBudgets.TryGetValue(record, out result);

			return result;
		}

		public virtual PMRevenueBudget GetOriginalRevenueBudget(BudgetKeyTuple record)
		{
			if (revenueBudgets == null || IsCacheUpdateRequired())
			{
				revenueBudgets = BuildRevenueBudgetLookup();
			}

			PMRevenueBudget result = null;
			revenueBudgets.TryGetValue(record, out result);

			return result;
		}

		public virtual PMChangeOrderBudget GetPreviousTotals(BudgetKeyTuple record)
		{
			if (previousTotals == null || IsCacheUpdateRequired())
			{
				previousTotals = BuildPreviousTotals();
			}

			PMChangeOrderBudget result = null;
			previousTotals.TryGetValue(record, out result);

			return result;
		}

		public virtual ICollection<PMCostBudget> GetCostBudget()
		{
			if (costBudgets == null || IsCacheUpdateRequired())
			{
				costBudgets = BuildCostBudgetLookup();
			}

			return costBudgets.Values;
		}

		public virtual ICollection<PMRevenueBudget> GetRevenueBudget()
		{
			if (revenueBudgets == null || IsCacheUpdateRequired())
			{
				revenueBudgets = BuildRevenueBudgetLookup();
			}

			return revenueBudgets.Values;
		}

		public virtual void InitalizeActionsMenu()
		{
			action.MenuAutoOpen = true;
			action.AddMenuAction(approve);
			action.AddMenuAction(reject);
			action.AddMenuAction(send);
			action.AddMenuAction(reverse);
		}

		public virtual void InitializeReportsMenu()
		{
			report.MenuAutoOpen = true;
			report.AddMenuAction(coReport);
		}

		public virtual void ReleaseDocument(PMChangeOrder doc)
		{
			if (doc.Released == true)
				return;

			foreach (PMChangeOrderCostBudget costBudget in CostBudget.Select())
			{
				ApplyChangeOrderBudget(costBudget, doc);
				CostBudget.Cache.SetValue<PMChangeOrderCostBudget.released>(costBudget, true);
				CostBudget.Cache.MarkUpdated(costBudget);
			}

			foreach (PMChangeOrderRevenueBudget revenueBudget in RevenueBudget.Select())
			{
				ApplyChangeOrderBudget(revenueBudget, doc);
				RevenueBudget.Cache.SetValue<PMChangeOrderRevenueBudget.released>(revenueBudget, true);
				RevenueBudget.Cache.MarkUpdated(revenueBudget);
			}

			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = res;

				Details.Cache.SetValue<PMChangeOrderLine.released>(line, true);
				Details.Cache.MarkUpdated(line);
			}

			ReleaseLineChanges();

			doc.Released = true;
			doc.Status = ChangeOrderStatus.Closed;
			Document.Update(doc);
			Save.Press();
		}
				
		public virtual void ReverseDocument()
		{
			if (Document.Current == null)
				return;

			PMChangeOrder source = (PMChangeOrder) Document.Cache.CreateCopy(Document.Current);
			source.ReverseStatus = ChangeOrderReverseStatus.Reversed;
			Document.Update(source);
			
			List<PMChangeOrderRevenueBudget> revenueBudget = new List<PMChangeOrderRevenueBudget>();
			foreach(PMChangeOrderRevenueBudget budget in RevenueBudget.Select())
			{
				PMChangeOrderRevenueBudget sourceRevenueBudget = (PMChangeOrderRevenueBudget) RevenueBudget.Cache.CreateCopy(budget);
				revenueBudget.Add(sourceRevenueBudget);
			}

			List<PMChangeOrderCostBudget> costBudget = new List<PMChangeOrderCostBudget>();
			foreach (PMChangeOrderCostBudget budget in CostBudget.Select())
			{
				PMChangeOrderCostBudget sourceCostBudget = (PMChangeOrderCostBudget)CostBudget.Cache.CreateCopy(budget);
				costBudget.Add(sourceCostBudget);
			}

			List<PXResult<PMChangeOrderLine, POLinePM>> lines = new List<PXResult<PMChangeOrderLine, POLinePM>>();
			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = (PMChangeOrderLine)res;
				POLinePM poLine = (POLinePM)res;

				PMChangeOrderLine sourceLine = (PMChangeOrderLine)Details.Cache.CreateCopy(line);
				lines.Add(new PXResult<PMChangeOrderLine, POLinePM>(sourceLine, poLine));
			}

			source.OrigRefNbr = source.RefNbr;
			source.RefNbr = null;
			source.ExtRefNbr = null;
			source.ProjectNbr = Messages.NotAvailable; ;
			source.Released = false;
			source.ReverseStatus = ChangeOrderReverseStatus.Reversal;
			source.Hold = true;
			source.Approved = false;
			source.Status = ChangeOrderStatus.OnHold;
			source.LineCntr = 0;
			source.CommitmentTotal = 0;
			source.RevenueTotal = 0;
			source.CostTotal = 0;
			Guid? sourceNoteID = source.NoteID;
			source.NoteID = Guid.NewGuid();

			PMChangeOrder target = Document.Insert(source);

			foreach (CSAnswers answer in Answers.Select())
			{
				CSAnswers dstanswer =
					Answers.Insert(new CSAnswers()
					{
						RefNoteID = target.NoteID,
						AttributeID = answer.AttributeID
					});
				if (dstanswer != null)
					dstanswer.Value = answer.Value;
			}

			foreach (PMChangeOrderRevenueBudget budget in revenueBudget)
			{
				budget.RefNbr = target.RefNbr;
				budget.Released = false;
				budget.Amount = -budget.Amount.GetValueOrDefault();
				budget.Qty = -budget.Qty.GetValueOrDefault();
				budget.NoteID = null;
				RevenueBudget.Insert(budget);
			}

			foreach (PMChangeOrderCostBudget budget in costBudget)
			{
				budget.RefNbr = target.RefNbr;
				budget.Released = false;
				budget.Amount = -budget.Amount.GetValueOrDefault();
				budget.Qty = -budget.Qty.GetValueOrDefault();
				budget.NoteID = null;
				CostBudget.Insert(budget);
			}

			foreach (PXResult<PMChangeOrderLine, POLinePM> res in lines)
			{
				PMChangeOrderLine line = (PMChangeOrderLine)res;
				POLinePM poLine = (POLinePM)res;

				line.LineType = ChangeOrderLineType.Update;
				line.RefNbr = target.RefNbr;
				line.LineNbr = null;
				line.Released = false;
				line.Amount = -Math.Min( line.Amount.GetValueOrDefault(), poLine.CalcCuryOpenAmt.GetValueOrDefault() );
				line.Qty = - Math.Min( line.Qty.GetValueOrDefault(), poLine.CalcOpenQty.GetValueOrDefault() );
				line.AmountInProjectCury = null;
				line.NoteID = null;
				Details.Insert(line);
			}

		}
		
		public virtual void ReleaseLineChanges()
		{
			//Existing orders:
			Dictionary<string, POOrder> orders = new Dictionary<string, POOrder>();
			Dictionary<string, List<PMChangeOrderLine>> updated = new Dictionary<string, List<PMChangeOrderLine>>();
			Dictionary<string, List<PMChangeOrderLine>> added = new Dictionary<string, List<PMChangeOrderLine>>();

			//New orders:
			SortedList<string, POOrder> newOrders = new SortedList<string, POOrder>();
			SortedList<string, List<PMChangeOrderLine>> newLines = new SortedList<string, List<PMChangeOrderLine>>();
			
			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = res;
				POLinePM poLine = res;

				POOrder order = CreatePOOrderFromChangedLine(line);
				string key = CreatePOOrderKey(order);

				if (string.IsNullOrEmpty(order.OrderNbr))
				{					
					if (!newOrders.ContainsKey(key))
					{
						newOrders.Add(key, order);
						newLines.Add(key, new List<PMChangeOrderLine>());
					}

					newLines[key].Add(line);
				}
				else
				{
					if (!orders.ContainsKey(key))
					{
						orders.Add(key, order);
						updated.Add(key, new List<PMChangeOrderLine>());
						added.Add(key, new List<PMChangeOrderLine>());
					}

					if (poLine.LineNbr != null)
					{
						updated[key].Add(line);
					}
					else
					{
						added[key].Add(line);
					}
				}
			}

			foreach(KeyValuePair<string, POOrder> kv in orders)
			{
				ModifyExistingOrder(kv.Value, updated[kv.Key], added[kv.Key]);
			}

			foreach (KeyValuePair<string, POOrder> kv in newOrders)
			{
				POOrder newOrder = CreateNewOrder(kv.Value, newLines[kv.Key]);

				foreach (PMChangeOrderLine line in newLines[kv.Key])
				{
					SetReferences(line, newOrder);
				}
			}
		}
		
		public virtual void ModifyExistingOrder(POOrder order, List<PMChangeOrderLine> updated, List<PMChangeOrderLine> added)
		{
			POOrderEntry target = CreateTarget(order);
			target.Document.Current = target.Document.Search<POOrder.orderNbr>(order.OrderNbr, order.OrderType);
			target.GetExtension<POOrderEntryExt>().SkipProjectLockCommitmentsVerification = true;
			
			if (updated.Count > 0)
				target.Transactions.Select().Consume();

			target.Document.Current.LockCommitment = true;

			POSetup poSetup = target.POSetup.Current;
			poSetup.RequireOrderControlTotal = false;
			poSetup.RequireBlanketControlTotal = false;
			poSetup.RequireDropShipControlTotal = false;

			target.Document.Update(target.Document.Current);
			
			foreach (PMChangeOrderLine line in updated)
			{
				ModifyExistsingLineInOrder(target, line);
			}

			foreach (PMChangeOrderLine line in added)
			{
				AddNewLineToOrder(target, line);
			}

			target.Document.Current.Status = POOrderStatus.Hold;//AU will reset to Open/Pending Print/Pending Email.
			target.Document.Current.Approved = true;
			target.Document.Current.Printed = false;
			target.Document.Current.Emailed = false;
			target.Document.Update(target.Document.Current);
			target.Document.Search<POOrder.orderNbr>(order.OrderNbr, order.OrderType);


			target.Save.Press();
		}

		protected virtual void ModifyExistsingLineInOrder(POOrderEntry target, PMChangeOrderLine line)
		{
				POLine key = new POLine() { OrderType = line.POOrderType, OrderNbr = line.POOrderNbr, LineNbr = line.POLineNbr };
			POLine poLine = (POLine)target.Transactions.Cache.Locate(key);

				if (poLine.OrigExtCost == null)
				{
					poLine.OrigOrderQty = poLine.OrderQty;
					poLine.OrigExtCost = poLine.CuryLineAmt;
				}

			decimal curyLineAmt = poLine.CuryLineAmt.GetValueOrDefault() + line.Amount.GetValueOrDefault();

				poLine.ManualPrice = true;
				poLine.Cancelled = false;
				poLine.Completed = false;
				poLine = target.Transactions.Update(poLine);
				
				if (poLine.OrderQty + line.Qty.GetValueOrDefault() == 0 && poLine.OrderQty > 0)
				{
					poLine.Cancelled = true;
				}
				else
				{
					poLine.OrderQty += line.Qty.GetValueOrDefault();
				}
			poLine = target.Transactions.Update(poLine);
			poLine.CuryLineAmt = curyLineAmt;
			poLine = target.Transactions.Update(poLine);
			}

		protected virtual void AddNewLineToOrder(POOrderEntry target, PMChangeOrderLine line)
			{
			POLine poLine = CreatePOLineFromChangeOrderLine(line);
			decimal curyLineAmt = poLine.CuryLineAmt.GetValueOrDefault();
			poLine.CuryLineAmt = null;
			poLine = target.Transactions.Insert(poLine);

			Details.Cache.SetValue<PMChangeOrderLine.pOLineNbr>(line, poLine.LineNbr);
			poLine.CuryLineAmt = curyLineAmt;
			poLine = target.Transactions.Update(poLine);
		}

		public virtual POOrder CreateNewOrder(POOrder order, List<PMChangeOrderLine> added)
		{
			POOrderEntry target = CreateTarget(order);
			target.GetExtension<POOrderEntryExt>().SkipProjectLockCommitmentsVerification = true;
			target.Document.Insert(order);
			target.Document.SetValueExt<POOrder.curyID>(target.Document.Current, order.CuryID);

			foreach (PMChangeOrderLine line in added)
			{
				POLine source = CreatePOLineFromChangeOrderLine(line);
				source.CuryUnitCost = null;
				source.CuryLineAmt = null;

				POLine poline = target.Transactions.Insert(source);
				poline.CuryUnitCost = line.UnitCost;
				poline.CuryLineAmt = line.Amount;
				target.Transactions.Update(poline);

				Details.Cache.SetValue<PMChangeOrderLine.pOLineNbr>(line, poline.LineNbr);
			}

			target.Document.Current.Status = POOrderStatus.Hold;//AU will reset to Open/Pending Print/Pending Email.
			target.Document.Current.Hold = false;
			target.Document.Current.LockCommitment = true;
			target.Document.Current.Approved = true;
			target.Document.Current.Printed = false;
			target.Document.Current.Emailed = false;
			target.Document.Update(target.Document.Current);
			target.Document.Current.Approved = true;
			
			target.Document.Search<POOrder.orderNbr>(target.Document.Current.OrderNbr, target.Document.Current.OrderType);

			target.Save.Press();
			return target.Document.Current;
		}

		public virtual void SetReferences(PMChangeOrderLine line, POOrder newOrder)
		{
			Details.Cache.SetValue<PMChangeOrderLine.pOOrderType>(line, newOrder.OrderType);
			Details.Cache.SetValue<PMChangeOrderLine.pOOrderNbr>(line, newOrder.OrderNbr);
		}

		public virtual string CreatePOOrderKey(POOrder order)
		{
			return string.Format("{0}.{1}.{2}.{3}", order.VendorID, order.OrderType, order.OrderNbr, order.CuryID);
		}

		public virtual POOrder CreatePOOrderFromChangedLine(PMChangeOrderLine line)
		{
			POOrder order = new POOrder();
			order.OrderType = line.POOrderType;
			order.OrderNbr = line.POOrderNbr;
			order.VendorID = line.VendorID;
			order.PayToVendorID = line.VendorID;
			order.Behavior = POBehavior.ChangeOrder;
			order.OrderDesc = PXMessages.LocalizeFormatNoPrefix(Messages.ChangeOrderPrefix, line.RefNbr);
			order.ProjectID = line.ProjectID;
			order.CuryID = line.CuryID;

			return order;
		}

		public virtual POLine CreatePOLineFromChangeOrderLine(PMChangeOrderLine line)
		{
			POLine poLine = new POLine();
			poLine.InventoryID = line.InventoryID;
			poLine.SubItemID = line.SubItemID;
			poLine.TranDesc = line.Description;
			poLine.UOM = line.UOM;
			poLine.OrigOrderQty = 0m;
			poLine.OrigExtCost = 0m;
			poLine.OrderQty = line.Qty;
			poLine.CuryUnitCost = line.UnitCost;
			poLine.CuryLineAmt = line.Amount;
			poLine.ExpenseAcctID = line.AccountID;
			poLine.ProjectID = line.ProjectID;
			poLine.TaskID = line.TaskID;
			poLine.CostCodeID = line.CostCodeID;
			poLine.TranDesc = line.Description;
			poLine.ManualPrice = true;
			
			return poLine;
		}

		public virtual int? GetProjectedAccountGroup(PMChangeOrderLine line)
		{
			Account revenueAccount = PXSelectorAttribute.Select<PMChangeOrderLine.accountID>(Details.Cache, line, line.AccountID) as Account;
			if (revenueAccount != null)
			{
				return revenueAccount.AccountGroupID;
			}

			return null;
		}

		public virtual POOrderEntry CreateTarget(POOrder order)
		{
			return PXGraph.CreateInstance<POOrderEntry>();
		}
				
		public virtual void AddSelectedCostBudget()
		{
			foreach( PMCostBudget budget in AvailableCostBudget.Cache.Updated )
			{
				if (budget.Type != GL.AccountType.Expense || budget.Selected != true) continue;

				PMChangeOrderCostBudget key = new PMChangeOrderCostBudget() { ProjectID = budget.ProjectID, ProjectTaskID = budget.ProjectTaskID, AccountGroupID = budget.AccountGroupID, InventoryID = budget.InventoryID, CostCodeID = budget.CostCodeID };

				if (CostBudget.Locate(key) == null)
				{
					CostBudget.Insert(key);
				}
			}
		}

		public virtual void AddSelectedRevenueBudget()
		{
			foreach (PMRevenueBudget budget in AvailableRevenueBudget.Cache.Updated)
			{
				if (budget.Type != GL.AccountType.Income || budget.Selected != true) continue;

				PMChangeOrderRevenueBudget key = new PMChangeOrderRevenueBudget() { ProjectID = budget.ProjectID, ProjectTaskID = budget.ProjectTaskID, AccountGroupID = budget.AccountGroupID, InventoryID = budget.InventoryID, CostCodeID = budget.CostCodeID };

				if (RevenueBudget.Locate(key) == null)
				{
					RevenueBudget.Insert(key);
				}
			}
		}

		public virtual void AddSelectedPOLines()
		{
			HashSet<POLineKey> existing = new HashSet<POLineKey>();

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.POLineNbr != null)
				{
					existing.Add(new POLineKey(line.POOrderType, line.POOrderNbr, line.POLineNbr.Value));
				}
			}
						
			foreach (POLinePM selected in AvailablePOLines.Cache.Updated)
			{
				if (selected.Selected != true)
					continue;

				POLineKey key = new POLineKey(selected.OrderType, selected.OrderNbr, selected.LineNbr.Value);

				if (existing.Contains(key))
					continue;
				
				Details.Insert(CreateChangeOrderLine(selected));
			}
		}

		public virtual PMChangeOrderLine CreateChangeOrderLine(POLinePM poLine)
		{
			PMChangeOrderLine line = new PMChangeOrderLine();
			line.POOrderType = poLine.OrderType;
			line.POOrderNbr = poLine.OrderNbr;
			line.POLineNbr = poLine.LineNbr;
			line.TaskID = poLine.TaskID;
			line.UOM = poLine.UOM;
			line.UnitCost = poLine.CuryUnitCost;
			line.VendorID = poLine.VendorID;
			line.CostCodeID = poLine.CostCodeID;
			line.CuryID = poLine.CuryID;
			line.AccountID = poLine.ExpenseAcctID;
			line.LineType = poLine.Completed == true || poLine.Cancelled == true ? ChangeOrderLineType.Reopen : ChangeOrderLineType.Update;
			line.InventoryID = poLine.InventoryID;
			line.SubItemID = poLine.SubItemID;

			return line;
		}

		public virtual void InitCostBudgetFields(PMChangeOrderCostBudget record)
		{
			if (record != null)
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalCostBudget(BudgetKeyTuple.Create(record)) : null;
				InitBudgetFields(record, budget);
			}
		}

		public virtual void InitRevenueBudgetFields(PMChangeOrderRevenueBudget record)
		{
			if (record != null)
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(record)) : null;
				InitBudgetFields(record, budget);
			}
		}

		public virtual void InitBudgetFields(PMChangeOrderBudget record, PMBudget budget)
		{
			if (record != null)
			{
				var recordKey = BudgetKeyTuple.Create(record);

				record.OtherDraftRevisedAmount = GetDraftChangeOrderBudgetAmount(record);
				record.RevisedAmount = record.Amount.GetValueOrDefault();
				
				if (IsValidKey(record))
					record.CommittedCOAmount = GetCurrentCommittedCOAmount(recordKey);

				PMChangeOrderBudget previousTotals = GetPreviousTotals(recordKey);

				if (previousTotals != null)
					{
					record.PreviouslyApprovedAmount = previousTotals.Amount.GetValueOrDefault();
					record.PreviouslyApprovedQty = previousTotals.Qty.GetValueOrDefault();
					}					

				if (budget != null)
				{
					record.RevisedAmount = budget.CuryAmount.GetValueOrDefault() + record.PreviouslyApprovedAmount.GetValueOrDefault() + record.Amount.GetValueOrDefault();
					record.RevisedQty = budget.Qty.GetValueOrDefault() + record.PreviouslyApprovedQty.GetValueOrDefault() + record.Qty.GetValueOrDefault();

					if (IsValidKey(record))
						record.CommittedCOQty = GetCurrentCommittedCOQty(BudgetKeyTuple.Create(budget), budget);

					record.TotalPotentialRevisedAmount = budget.CuryRevisedAmount.GetValueOrDefault() + record.OtherDraftRevisedAmount.GetValueOrDefault();

					if (record.Released != true)
					{
						record.TotalPotentialRevisedAmount += record.Amount.GetValueOrDefault();
					}
					
				}
				else
				{
					if (!string.IsNullOrEmpty(record.UOM) )
					{
						record.RevisedQty = record.Qty;
						if (IsValidKey(record))
							record.CommittedCOQty = GetCurrentCommittedCOQty(recordKey, record);
					}
				}
			}
		}

		public virtual decimal GetCurrentCommittedCOAmount(BudgetKeyTuple key)
		{
			decimal amount = 0;

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()) == key.CostCodeID && line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) == key.InventoryID && line.TaskID == key.ProjectTaskID && GetProjectedAccountGroup(line) == key.AccountGroupID)
				{
					amount += line.AmountInProjectCury.GetValueOrDefault();
				}
			}

			return amount;
		}

		public virtual decimal GetCurrentCommittedCOQty(BudgetKeyTuple key, IQuantify budget)
		{
			decimal qty = 0;

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()) == key.CostCodeID && line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) == key.InventoryID && line.TaskID == key.ProjectTaskID && GetProjectedAccountGroup(line) == key.AccountGroupID)
				{
					var rollupQty = BalanceCalculator.CalculateRollupQty(line, budget);
					if (rollupQty != 0)
					{
						qty += rollupQty;
					}
				}
			}

			return qty;
		}
		
		public virtual void InitDetailLineFields(PMChangeOrderLine line)
		{
			if (line != null)
			{
				if (line.Released != true)
				{
					line.PotentialRevisedAmount = line.Amount.GetValueOrDefault();
					line.PotentialRevisedQty = line.Qty.GetValueOrDefault();
				}
				else
				{
					line.PotentialRevisedAmount = 0;
					line.PotentialRevisedQty = 0;
				}

				POLinePM poLine = GetPOLine(line);
				if (poLine != null)
				{
					line.PotentialRevisedAmount += poLine.CuryLineAmt.GetValueOrDefault();
					line.PotentialRevisedQty += poLine.OrderQty.GetValueOrDefault();

				}
			}
		}

		public virtual bool PrepaymentVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.PrepaymentEnabled == true;
			}

			return false;
		}

		public virtual bool LimitsVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.LimitsEnabled == true;
			}

			return false;
		}

		public virtual bool ProductivityVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.BudgetMetricsEnabled == true;
			}

			return false;
		}

		public virtual bool CanEditDocument(PMChangeOrder doc)
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
				return false;
			}
		}

		public virtual decimal? ConvertToProjectCury(PMChangeOrderLine line)
		{			
			if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
			{
				//Use Project RateTypeID 

				if (line.CuryID == Project.Current.CuryID)
				{
					return line.Amount;
				}
				else
				{
					string rateTypeID = Project.Current.RateTypeID;

					if (string.IsNullOrEmpty(rateTypeID))
					{
						CMSetup cmsetup = PXSelect<CMSetup>.Select(this);
						rateTypeID = cmsetup?.PMRateTypeDflt;
					}

					DateTime effectiveDate = Accessinfo.BusinessDate ?? DateTime.Now;
					if (Document.Current != null && Document.Current.Date != null)
						effectiveDate = Document.Current.Date.Value;

					PX.Objects.CM.Extensions.IPXCurrencyService currencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, PX.Objects.CM.Extensions.IPXCurrencyService>>()(this);

					var rate = currencyService.GetRate(line.CuryID, Project.Current.CuryID, rateTypeID, effectiveDate);
					if (rate == null)
					{
						throw new PXException(PM.Messages.FxTranToProjectNotFound, line.CuryID, Project.Current.CuryID, rateTypeID, effectiveDate);
					}
					else
					{
						return PMCommitmentAttribute.CuryConvCury(rate, line.Amount.GetValueOrDefault(), currencyService.CuryDecimalPlaces(Project.Current.CuryID));
					}
				}
			}
			else
			{
				//Classics ProjectCury is in Base; Use Vendor Rates

				return ConvertToBase(line);
			}
		}

		public virtual decimal? ConvertToBase(PMChangeOrderLine line)
		{
			decimal? result = null;
			if (!string.IsNullOrEmpty(line.CuryID) && line.CuryID == Accessinfo.BaseCuryID)
			{
				return line.Amount;
			}

			POLinePM poLine = GetPOLine(line);
			if (poLine != null && poLine.CuryInfoID != null)
			{
				if (poLine.CuryID == Accessinfo.BaseCuryID)
				{
					return line.Amount;
				}
				else
				{
					CurrencyInfo curyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, poLine.CuryInfoID);
					if (curyInfo != null)
					{
						decimal val;
						PXCurrencyAttribute.CuryConvBase(Details.Cache, curyInfo, line.Amount.GetValueOrDefault(), out val);
						result = val;
					}
				}
			}

			if (result == null)
			{
				string rateType = null;
				string currency = null;

				if (line.VendorID == null)
				{
					Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, line.VendorID);
					if (vendor != null)
					{
						rateType = vendor.CuryRateTypeID;
						currency = vendor.CuryID;
					}
				}

				if (string.IsNullOrEmpty(rateType))
				{
					CMSetup setup = PXSelect<CMSetup>.Select(this);
					if (setup != null)
					{
						rateType = setup.APRateTypeDflt;
					}
				}

				if (string.IsNullOrEmpty(rateType) || string.IsNullOrEmpty(currency))
				{
					return line.Amount;
				}

				DateTime effectiveDate = Accessinfo.BusinessDate ?? DateTime.Now;
				if (Document.Current != null && Document.Current.Date != null)
					effectiveDate = Document.Current.Date.Value;

				CurrencyInfo curyInfo = new CurrencyInfo();
				curyInfo.ModuleCode = GL.BatchModule.PM;
				curyInfo.CuryID = currency;
				curyInfo.CuryRateTypeID = rateType;

				try
				{
					curyInfo.SetCuryEffDate(Details.Cache, effectiveDate);

					decimal val;
					PXCurrencyAttribute.CuryConvBase(Details.Cache, curyInfo, line.Amount.GetValueOrDefault(), out val);

					result = val;
				}
				catch (PXRateNotFoundException ex)
				{
					PXTrace.WriteError(ex);
					result = line.Amount;
				}
				catch (PXRateIsNotDefinedForThisDateException ex)
				{
					PXTrace.WriteError(ex);
					result = line.Amount;
				}
			}

			return result;
		}


		public virtual string DecNumber(string lastChangeOrderNumber)
		{
			char[] lastNumber = lastChangeOrderNumber.ToCharArray();
			for (int i = lastNumber.Length - 1; i >= 0; i--)
			{
				if (char.IsDigit(lastNumber[i]))
				{
					int lastDigit = int.Parse(new string(lastNumber[i], 1));
					if (lastDigit != 0)
					{
						lastDigit--;
						lastNumber[i] = lastDigit.ToString().ToCharArray()[0];
						for (int j = i + 1; j < lastNumber.Length; j++)
						{
							lastNumber[j] = '9';
						}

						break;
					}
				}
				else
				{
					break;
				}
			}

			return new string(lastNumber);
		}

		public virtual bool IsProjectEnabled()
		{
			if (CostBudget.Cache.IsInsertedUpdatedDeleted)
				return false;
			if (RevenueBudget.Cache.IsInsertedUpdatedDeleted)
				return false;
			if (Details.Cache.IsInsertedUpdatedDeleted)
				return false;

			if (CostBudget.Select().Count > 0)
				return false;
			
			if (RevenueBudget.Select().Count > 0)
				return false;

			if (Details.Select().Count > 0)
				return false;

			return true;
		}

		public virtual void AddToDraftBucket(PMChangeOrderBudget row, int mult=1)
		{
			if (row.ProjectID == null) return;
			if (row.ProjectTaskID == null) return;
			if (row.AccountGroupID == null) return;
			if (row.InventoryID == null) return;
			if (row.CostCodeID == null) return;

			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, row.AccountGroupID);
			
			bool isExisting;
			BudgetService budgetService = new BudgetService(this);
			var target = budgetService.SelectProjectBalance(row, ag, Project.Current, out isExisting);

			PMBudgetAccum budget = new PMBudgetAccum();
			budget.ProjectID = row.ProjectID;
			budget.ProjectTaskID = row.ProjectTaskID;
			budget.AccountGroupID = row.AccountGroupID;
			budget.InventoryID = target.InventoryID;
			budget.CostCodeID = target.CostCodeID;
			budget.UOM = target.UOM;
			budget.Description = target.Description;
			budget.Type = target.Type;
						
			budget = Budget.Insert(budget);
			decimal rollupQty = BalanceCalculator.CalculateRollupQty(row, budget);

			budget.CuryDraftChangeOrderAmount += mult * row.Amount.GetValueOrDefault();
			budget.DraftChangeOrderQty += mult * rollupQty;

			if (Document.Current != null)
			{
				FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(Document.Current.Date, FinPeriod.organizationID.MasterValue);

				if (finPeriod != null)
				{
					PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
					forecast.ProjectID = target.ProjectID;
					forecast.ProjectTaskID = target.ProjectTaskID;
					forecast.AccountGroupID = target.AccountGroupID;
					forecast.InventoryID = target.InventoryID;
					forecast.CostCodeID = target.CostCodeID;
					forecast.PeriodID = finPeriod.FinPeriodID;

					forecast = ForecastHistory.Insert(forecast);

					forecast.DraftChangeOrderQty += mult * rollupQty;
					forecast.CuryDraftChangeOrderAmount += mult * row.Amount.GetValueOrDefault();
				}
			}
		}

		public virtual void RemoveObsoleteLines()
		{
			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				if (item.CuryDraftChangeOrderAmount.GetValueOrDefault() == 0 && item.DraftChangeOrderQty.GetValueOrDefault() == 0)
				{
					Budget.Cache.Remove(item);
				}
			}

			foreach (PMForecastHistoryAccum item in ForecastHistory.Cache.Inserted)
			{
				if (item.CuryDraftChangeOrderAmount.GetValueOrDefault() == 0 && item.DraftChangeOrderQty.GetValueOrDefault() == 0)
				{
					ForecastHistory.Cache.Remove(item);
				}
			}
		}

		public virtual void ApplyChangeOrderBudget(PMChangeOrderBudget row, PMChangeOrder order)
		{
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, row.AccountGroupID);
			
			bool isExisting;
			BudgetService budgetService = new BudgetService(this);
			var target = budgetService.SelectProjectBalance(row, ag, Project.Current, out isExisting);

			PMBudgetAccum budget = new PMBudgetAccum();
			budget.ProjectID = target.ProjectID;
			budget.ProjectTaskID = target.ProjectTaskID;
			budget.AccountGroupID = target.AccountGroupID;
			budget.InventoryID = target.InventoryID;
			budget.CostCodeID = target.CostCodeID;
			budget.UOM = target.UOM;
			budget.Description = target.Description;
			budget.Type = target.Type;

			decimal amountToInvoiceDelta = 0;
			if (!isExisting)
			{
				budget.CuryUnitRate = row.Rate;
				budget.UOM = row.UOM ?? target.UOM;
				budget.Description = row.Description ?? target.Description;
			}
			else
			{
				//The code block below is used to calculate/update the PendingInvoiceAmount based on 
				//current Completed % and CO. The following code works under assumption that there is no
				//concurrent modification of the underline budget line.
				//This pattern is to be reviewed

				if (budget.Type == GL.AccountType.Income)
				{
					PMRevenueBudget revenue = GetOriginalRevenueBudget(BudgetKeyTuple.Create(target));
					if (revenue != null)
					{
						decimal pendingCuryRevisedAmount = revenue.CuryRevisedAmount.GetValueOrDefault() + row.Amount.GetValueOrDefault();
						decimal invoicedOrPendingPrepayment = revenue.CuryActualAmount.GetValueOrDefault() + revenue.CuryInvoicedAmount.GetValueOrDefault() + (revenue.CuryPrepaymentAmount.GetValueOrDefault() - revenue.CuryPrepaymentInvoiced.GetValueOrDefault());
						decimal amountToInvoice = (pendingCuryRevisedAmount * revenue.CompletedPct.GetValueOrDefault() / 100m) - invoicedOrPendingPrepayment;

						amountToInvoiceDelta = amountToInvoice - revenue.CuryAmountToInvoice.GetValueOrDefault();
					}
				}
			}

			budget = Budget.Insert(budget);
			decimal rollupQty = BalanceCalculator.CalculateRollupQty(row, budget);
			
			budget.DraftChangeOrderQty -= rollupQty;
			budget.ChangeOrderQty += rollupQty;
			budget.RevisedQty += rollupQty;
			budget.CuryDraftChangeOrderAmount -= row.Amount.GetValueOrDefault();			
			budget.CuryChangeOrderAmount += row.Amount.GetValueOrDefault();			
			budget.CuryRevisedAmount += row.Amount.GetValueOrDefault();
			budget.CuryAmountToInvoice += amountToInvoiceDelta;

			FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(order.Date, FinPeriod.organizationID.MasterValue);

			if (finPeriod != null)
			{
				PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
				forecast.ProjectID = target.ProjectID;
				forecast.ProjectTaskID = target.ProjectTaskID;
				forecast.AccountGroupID = target.AccountGroupID;
				forecast.InventoryID = target.InventoryID;
				forecast.CostCodeID = target.CostCodeID;
				forecast.PeriodID = finPeriod.FinPeriodID;

				forecast = ForecastHistory.Insert(forecast);

				forecast.DraftChangeOrderQty -= rollupQty;
				forecast.CuryDraftChangeOrderAmount -= row.Amount.GetValueOrDefault();
				forecast.ChangeOrderQty += rollupQty;
				forecast.CuryChangeOrderAmount += row.Amount.GetValueOrDefault();
			}
		}


		private PMProject GetProject(int? projectID)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, projectID);
			return project;
		}

		private PMCostCode GetCostCode(int? costCodeID)
		{
			PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(this, costCodeID);

			return costCode;
		}

		private InventoryItem GetInventoryItem(int? inventoryID)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);

			return item;
		}

		private PMChangeOrderClass GetChangeOrderClass(string classID)
		{
			PMChangeOrderClass record = PXSelect<PMChangeOrderClass, Where<PMChangeOrderClass.classID, Equal<Required<PMChangeOrderClass.classID>>>>.Select(this, classID);

			return record;
		}

		private string LastRefNbr = string.Empty;
		/// <summary>
		/// You must invalidate local cache, when document key changes (next, previous, last, first actions).
		/// </summary>
		private bool IsCacheUpdateRequired()
		{
			if (Document.Current == null)
				return false;

			string currentRefNbr = Document.Current.RefNbr;
			bool isRequired = currentRefNbr != LastRefNbr;
			if (isRequired)
			{
				ClearLocalCache();
				LastRefNbr = currentRefNbr;				
			}
			return isRequired;
		}

		public override void Clear()
		{
			base.Clear();

			ClearLocalCache();
		}

		private void ClearLocalCache()
		{
			costBudgets = null;
			revenueBudgets = null;
			polines = null;
			previousTotals = null;
		}

		#region Local Types

		[PXHidden]
		[Serializable]
		public class POLineFilter : IBqlTable
		{
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(PMProject.contractID), AlwaysEnabled = true)]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			[POVendor()]
			public virtual Int32? VendorID
			{
				get;
				set;
			}
			#endregion
			#region VendorRefNbr
			public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
			
			[PXDBString(40)]
			[PXUIField(DisplayName = "Vendor Ref.", Enabled = false)]
			public virtual String VendorRefNbr
			{
				get;
				set;
			}
			#endregion
			#region POOrderNbr
			public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }

			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "PO Nbr.")]
			[PXSelector(typeof(Search4<POLine.orderNbr, Where<POLine.orderType, Equal<POOrderType.regularOrder>,
				And<POLine.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<POLine.cancelled, Equal<False>,
				And<POLine.completed, Equal<False>,
				And<Where<Current<vendorID>, IsNull, Or<POLine.vendorID, Equal<Current<vendorID>>>>>>>>>,
				Aggregate<GroupBy<POLine.orderType, GroupBy<POLine.orderNbr, GroupBy<POLine.vendorID>>>>>),
				typeof(POLine.orderType), typeof(POLine.orderNbr), typeof(POLine.vendorID))]
			public virtual String POOrderNbr
			{
				get;
				set;
			}
			#endregion

			#region CostCodeFrom
			public abstract class costCodeFrom : PX.Data.BQL.BqlString.Field<costCodeFrom> { }

			[PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(Search<PMCostCode.costCodeCD>))]
			[PXDBString(IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Cost Code From", FieldClass = CostCodeAttribute.COSTCODE)]
			public virtual String CostCodeFrom
			{
				get;
				set;
			}
			#endregion
			#region CostCodeTo
			public abstract class costCodeTo : PX.Data.BQL.BqlString.Field<costCodeTo> { }

			[PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(PMCostCode.costCodeCD))]
			[PXDBString(IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Cost Code To", FieldClass = CostCodeAttribute.COSTCODE)]
			public virtual String CostCodeTo
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[Inventory(Filterable = true)]
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

			#region Include Non-open Commitments 
			public abstract class includeNonOpen : PX.Data.BQL.BqlBool.Field<includeNonOpen> { }
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include Non-Open Commitments")]
			public virtual Boolean? IncludeNonOpen
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden]
		[Serializable]
		public class CostBudgetFilter : IBqlTable
		{
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(PMProject.contractID), AlwaysEnabled = true, DirtyRead = true)]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion

			#region GroupByTask
			public abstract class groupByTask : PX.Data.BQL.BqlBool.Field<groupByTask> { }
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Group by Task")]
			public virtual Boolean? GroupByTask
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden]
		[Serializable]
		public class RevenueBudgetFilter : IBqlTable
		{
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(PMProject.contractID), AlwaysEnabled = true, DirtyRead = true)]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion

			#region GroupByTask
			public abstract class groupByTask : PX.Data.BQL.BqlBool.Field<groupByTask> { }
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Group by Task")]
			public virtual Boolean? GroupByTask
			{
				get;
				set;
			}
			#endregion
		}

		public struct POLineKey
		{
			public readonly string OrderType;
			public readonly string OrderNbr;
			public readonly int LineNbr;
			
			public POLineKey(string orderType, string orderNbr, int lineNbr)
			{
				OrderType = orderType;
				OrderNbr = orderNbr;
				LineNbr = lineNbr;
			}

			public override int GetHashCode()
			{
				unchecked // Overflow is fine, just wrap
				{
					int hash = 17;
					hash = hash * 23 + OrderType.GetHashCode();
					hash = hash * 23 + OrderNbr.GetHashCode();
					hash = hash * 23 + LineNbr.GetHashCode();
					return hash;
				}
			}
			
		}

		#endregion

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == nameof(RevenueBudget))
			{
				string accountGroupCD = null;
				if (keys.Contains(nameof(PMRevenueBudget.AccountGroupID)))
				{
					//Import file could be missing the AccountGroupID field and hence the Default value could be set by the DefaultEventHandler

					object keyVal = keys[nameof(PMRevenueBudget.AccountGroupID)];

					if (keyVal is int)
					{
						PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, keyVal);
						if (accountGroup != null)
						{
							return accountGroup.Type == GL.AccountType.Income;
						}
					}
					else
					{
						accountGroupCD = (string)keys[nameof(PMRevenueBudget.AccountGroupID)];
					}
				}

				if (!string.IsNullOrEmpty(accountGroupCD))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>.Select(this, accountGroupCD);
					if (accountGroup != null)
					{
						return accountGroup.Type == GL.AccountType.Income;
					}
				}
				else
				{
					return true;
				}

				return false;

			}
			else if (viewName == nameof(CostBudget))
			{
				string accountGroupCD = null;
				if (keys.Contains(nameof(PMCostBudget.AccountGroupID)))
				{
					accountGroupCD = (string)keys[nameof(PMCostBudget.AccountGroupID)];
				}

				if (!string.IsNullOrEmpty(accountGroupCD))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>.Select(this, accountGroupCD);
					if (accountGroup != null)
					{
						return accountGroup.IsExpense == true;
					}
				}
				else
				{
					return true;
				}

				return false;
			}
			
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion
	}



}
