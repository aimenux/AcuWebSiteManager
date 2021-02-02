using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TM;
using PX.Objects.PM;
using PX.Objects.Common.Extensions;
using static PX.Objects.Common.UIState;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.EP
{
	public class ExpenseClaimEntry : PXGraph<ExpenseClaimEntry, EPExpenseClaim>, PXImportAttribute.IPXPrepareItems
	{
		#region Extensions

		public class ExpenseClaimEntryReceiptExt : ExpenseClaimDetailEntryExt<ExpenseClaimEntry>
		{
			public override bool UseClaimStatus => true;

			public override PXSelectBase<EPExpenseClaimDetails> Receipts => Base.ExpenseClaimDetails;

			public override PXSelectBase<EPExpenseClaim> Claim => Base.ExpenseClaimCurrent;

			public override PXSelectBase<CurrencyInfo> CurrencyInfo => Base.currencyinfo;
		}

		#endregion

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		#region Internal classes
		public class ExpenceClaimApproval<SourceAssign> : EPApprovalAction<SourceAssign, EPExpenseClaim.approved, EPExpenseClaim.rejected>
			where SourceAssign : EPExpenseClaim, new()
		{
			public ExpenceClaimApproval(PXGraph graph, Delegate @delegate)
				: base(graph, @delegate)
			{
				Initialize();
			}

			public ExpenceClaimApproval(PXGraph graph)
				: base(graph)
			{
				Initialize();
			}

			private void Initialize()
			{
				if (!PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
				{
					this.Cache.Graph.FieldVerifying.AddHandler<EPApproval.ownerID>((sender, e) =>
					{
						e.Cancel = true;
					});
				}
			}
			protected override IEnumerable<ApproveInfo> GetApproversFromNextStep(SourceAssign source, EPAssignmentMap map, int? currentStepSequence)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
				{
					foreach (var approveInfo in base.GetApproversFromNextStep(source, map, currentStepSequence))
					{
						yield return approveInfo;
					}
					yield break;
				}

				PXCache cache = this._Graph.Caches[typeof(EPEmployee)];
                EPEmployee employee = (EPEmployee)cache.Current;
				if (employee.SupervisorID != null)
				{
					EPEmployee super = (EPEmployee)PXSelect<EPEmployee, 
                        Where<EPEmployee.bAccountID, Equal<Current<EPEmployee.supervisorID>>>>.Select(this._Graph, employee.BAccountID);
					if (super != null)
					{
						source.WorkgroupID = null;
						source.OwnerID = super.UserID;

						yield return new ApproveInfo
						{
							OwnerID = super.UserID,
							WorkgroupID = null
						};
					}
				}
			}
		}
		
		[Serializable]
        public class EPExpenseClaimDetailsForSubmit : EPExpenseClaimDetails
        {
	        public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			#region ClaimDetailID
			public new abstract class claimDetailID : PX.Data.BQL.BqlInt.Field<claimDetailID> { }
            [PXDBIdentity(IsKey = true)]
            public override Int32? ClaimDetailID
            {
                get;
                set;
            }
            #endregion

            #region RefNbr
            public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
            [PXDBString(15, IsUnicode = true)]
            public override String RefNbr
            {
                get;
                set;
            }
            #endregion
            #region TaxCategoryID

            public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

            [PXDBString(10, IsUnicode = true)]
            public override String TaxCategoryID
            {
                get;
                set;
            }
			#endregion
			#region CuryTipAmt
			public new abstract class curyTipAmt : PX.Data.BQL.BqlDecimal.Field<curyTipAmt> { }
			[PXDBCurrency(typeof(curyInfoID), typeof(tipAmt))]
			[PXUIField(DisplayName = "Tip Amount")]
			public override decimal? CuryTipAmt
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		[PXHidden]
		public partial class EPCustomerUpdateAsk : IBqlTable
		{

			#region PeriodDateSel
			public abstract class customerUpdateAnswer : PX.Data.BQL.BqlString.Field<customerUpdateAnswer> { }

			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Date Based On", Visibility = PXUIVisibility.Visible)]
			[PXDefault(SelectedCustomer)]
			[PXStringList(new string[] { SelectedCustomer, AllLines, Nothing },
				new string[] { Messages.SelectedCustomer, Messages.AllLines, Messages.Nothing })]
			public virtual string CustomerUpdateAnswer
			{
                get;
                set;
			}
			#endregion

			#region NewCustomerID
			public abstract class newCustomerID : PX.Data.BQL.BqlInt.Field<newCustomerID> { }

			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[AR.CustomerActive(DescriptionField = typeof(AR.Customer.acctName))]
			public virtual int? NewCustomerID
			{
				get;
				set;
			}
            #endregion

            #region OldCustomerID
            public abstract class oldCustomerID : PX.Data.BQL.BqlInt.Field<oldCustomerID> { }

			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[AR.CustomerActive(DescriptionField = typeof(AR.Customer.acctName))]
			public virtual int? OldCustomerID
			{
                get;
                set;
			}
			#endregion

			public const String SelectedCustomer = "S";
			public const String AllLines = "A";
			public const String Nothing = "N";

		}
		#endregion
		#region TaxZoneUpdateAsk
		[Serializable]
		[PXHidden]
		public partial class TaxZoneUpdateAsk : IBqlTable
		{
		}
		#endregion

		#region Buttons declaration

		public PXAction<EPExpenseClaim> action;
		[PXUIField(DisplayName = Messages.Actions)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			foreach (EPExpenseClaim claim in adapter.Get<EPExpenseClaim>())
			{
				ExpenseClaim.Search<EPExpenseClaim.refNbr>(((EPExpenseClaim)claim).RefNbr);
			}
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> ViewTaxes;
		[PXUIField(DisplayName = "View Taxes")]
		[PXButton]
		protected virtual IEnumerable viewTaxes(PXAdapter adapter)
		{
			Tax_Rows.AskExt(true);
			return adapter.Get();
		}

        public PXAction<EPExpenseClaim> CommitTaxes;
        [PXUIField(DisplayName = "Ok")]
        [PXButton]
        protected virtual IEnumerable commitTaxes(PXAdapter adapter)
        {
            ExpenseClaimDetails.Update(ExpenseClaimDetails.Current);
            return adapter.Get();
        }

        public PXAction<EPExpenseClaim> submit;
		[PXUIField(DisplayName = Messages.Submit)]
		[PXButton]
		protected virtual void Submit()
		{
			Save.Press();
			SubmitClaim(ExpenseClaim.Current);
		}

		protected virtual void SubmitClaim(EPExpenseClaim claim)
		{
			if (claim != null)
			{
				bool erroroccurred = false;
				foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
				{
					if (detail.Rejected == false && detail.Hold == false && detail.Approved == false)
					{
						erroroccurred = true;
						ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.tranDesc>(detail, detail.TranDesc, new PXSetPropertyException(Messages.ReceiptNotApproved, PXErrorLevel.RowError));
					}
					else if (detail.Rejected == true)
					{
						erroroccurred = true;
						ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.tranDesc>(detail, detail.TranDesc, new PXSetPropertyException(Messages.RemovedRejectedReceipt, PXErrorLevel.RowError));
					}
					else if (detail.Hold == true)
					{
						erroroccurred = true;
						ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.tranDesc>(detail, detail.TranDesc, new PXSetPropertyException(Messages.ReceiptTakenOffHold, PXErrorLevel.RowError));
					}
				}
				if (erroroccurred)
					throw new PXException(Messages.NotAllReceiptsOpenStatus);
				int? assignmentMap = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>()
					? epsetup.Current.ClaimAssignmentMapID
					: null;

				if (assignmentMap != null || !PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
				{
					Approval.Assign(claim, assignmentMap, epsetup.Current.ClaimAssignmentNotificationID);
				}
				else
				{
					claim.Approved = true;
				}
				claim.Hold = false;
				ExpenseClaim.Update(claim);
				ExpenseClaim.Search<EPExpenseClaim.refNbr>(((EPExpenseClaim)claim).RefNbr);
			}
		}

		public PXAction<EPExpenseClaim> edit;
		[PXUIField(DisplayName = Messages.PutOnHold)]
		[PXButton]
		protected virtual void Edit()
		{
			if (ExpenseClaim.Current != null)
			{
				ExpenseClaim.Current.Approved = false;
				ExpenseClaim.Current.Rejected = false;
				ExpenseClaim.Current.Hold = true;
				ExpenseClaim.Update(ExpenseClaim.Current);
				PXSelectBase<EPApproval> select = new PXSelect<EPApproval, Where<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>>>(this);
				foreach (EPApproval approval in select.Select((ExpenseClaim.Current.NoteID)))
				{
					this.Caches[typeof(EPApproval)].Delete(approval);
				}
			}
		}

		public PXAction<EPExpenseClaim> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		[PXActionRestriction(typeof(Where<Current<APSetup.migrationMode>, Equal<True>>), AP.Messages.MigrationModeIsActivated)]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			foreach (PXResult<EPExpenseClaim> rec in adapter.Get())
			{
				EPExpenseClaim claim = rec;
				if (claim.Approved == false || claim.Released == true)
				{
					throw new PXException(Messages.Document_Status_Invalid);
				}
				Save.Press();
				PXLongOperation.StartOperation(this, () => EPDocumentRelease.ReleaseDoc(claim));
			}
			return adapter.Get();
		}

		public ToggleCurrency<EPExpenseClaim> CurrencyView;

		public PXAction<EPExpenseClaim> expenseClaimPrint;
		[PXUIField(DisplayName = Messages.PrintExpenseClaim, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable ExpenseClaimPrint(PXAdapter adapter)
		{
			if (ExpenseClaim.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = ExpenseClaim.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "EP612000", Messages.PrintExpenseClaim);
			}

			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> showSubmitReceipt;
		[PXUIField(DisplayName = Messages.AddReceipts, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report, Tooltip = Messages.AddReceipts)]
		protected virtual IEnumerable ShowSubmitReceipt(PXAdapter adapter)
		{
			if (ReceiptsForSubmit.AskExt(true) == WebDialogResult.OK)
			{
				return SubmitReceipt(adapter);
			}
			ReceiptsForSubmit.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> submitReceipt;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable SubmitReceipt(PXAdapter adapter)
		{
			if (ExpenseClaim.Current != null)
			{
				foreach (EPExpenseClaimDetails item in ReceiptsForSubmit.Select())
				{
					if (item.Selected == true)
					{
						item.Selected = false;
						var key = new EPExpenseClaimDetails { ClaimDetailCD = item.ClaimDetailCD, ClaimDetailID = item.ClaimDetailID };
						EPExpenseClaimDetails origDetails = ExpenseClaimDetails.Locate(key) ?? item;
						EPExpenseClaimDetails details = (EPExpenseClaimDetails)ExpenseClaimDetails.Cache.CreateCopy(origDetails);
						FindImplementation<ExpenseClaimEntryReceiptExt>().SubmitReceiptExt(ExpenseClaim.Cache, ExpenseClaimDetails.Cache, ExpenseClaim.Current, details);
					}
				}
			}
			ReceiptsForSubmit.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> cancelSubmitReceipt;
		[PXUIField(DisplayName = Messages.Close, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable CancelSubmitReceipt(PXAdapter adapter)
		{
			ReceiptsForSubmit.Cache.Clear();
			return adapter.Get();
		}


		public PXAction<EPExpenseClaim> createNew;
		[PXUIField(DisplayName = Messages.AddNewReceipt)]
		[PXButton(Tooltip = Messages.AddReceiptToolTip,
			ConfirmationType = PXConfirmationType.IfDirty,
			ConfirmationMessage = ActionsMessages.ConfirmUnsavedChanges)]
		protected virtual void CreateNew()
		{
			ExpenseClaimDetailEntry graph = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
			graph.Clear(PXClearOption.ClearAll);
			EPExpenseClaimDetails claimDetails = (EPExpenseClaimDetails)graph.ClaimDetails.Cache.CreateInstance();
			EPExpenseClaim expenseClaim = ExpenseClaim.Current;
			if (expenseClaim != null && expenseClaim.EmployeeID != null)
			{
				claimDetails.RefNbr = expenseClaim.RefNbr;
				claimDetails.EmployeeID = expenseClaim.EmployeeID;
				claimDetails.BranchID = expenseClaim.BranchID;
				claimDetails.CustomerID = expenseClaim.CustomerID;
				claimDetails.CustomerLocationID = expenseClaim.CustomerLocationID;

				bool enabledApprovalReceipt = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && epsetup.Current.ClaimDetailsAssignmentMapID != null;
				claimDetails.Hold = enabledApprovalReceipt;
				claimDetails.Approved = !enabledApprovalReceipt;

				PXFieldVerifying skipVerifyingAction = (sender, args) =>
				{
					args.Cancel = true;
				};

				try
				{
					graph.FieldVerifying.AddHandler<EPExpenseClaimDetails.refNbr>(skipVerifyingAction);

					claimDetails = graph.ClaimDetails.Insert(claimDetails);
				}
				finally
				{
					graph.FieldVerifying.RemoveHandler<EPExpenseClaimDetails.refNbr>(skipVerifyingAction);
				}

				claimDetails.TaxZoneID = expenseClaim.TaxZoneID;
				claimDetails.TaxCalcMode = expenseClaim.TaxCalcMode;
				claimDetails = graph.ClaimDetails.Update(claimDetails);
				graph.ClaimDetails.SetValueExt<EPExpenseClaimDetails.refNbr>(claimDetails, ExpenseClaim.Current.RefNbr);
				claimDetails = graph.ClaimDetails.Update(claimDetails);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<EPExpenseClaim> editDetail;
		[PXUIField(DisplayName = ActionsMessages.Edit)]
		[PXEditDetailButton]
		protected virtual void EditDetail()
		{
			if (ExpenseClaim.Current != null && ExpenseClaimDetails.Current != null)
			{
				Save.Press();
				PXRedirectHelper.TryRedirect(ExpenseClaimDetails.Cache, ExpenseClaimDetails.Current, "Open receipt", PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<EPExpenseClaim> viewUnsubmitReceipt;
		[PXButton()]
		protected virtual void ViewUnsubmitReceipt()
		{
			if (ReceiptsForSubmit.Current != null)
			{
				ExpenseClaimDetailEntry graph = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
				PXRedirectHelper.TryRedirect(graph, ReceiptsForSubmit.Current, PXRedirectHelper.WindowMode.NewWindow);
			}
		}


		public PXAction<EPExpenseClaim> changeOk;
		[PXUIField(DisplayName = "Change")]
		[PXButton()]
		protected virtual IEnumerable ChangeOk(PXAdapter adapter)
		{
			if (CustomerUpdateAsk.Current.CustomerUpdateAnswer != EPCustomerUpdateAsk.Nothing)
			{
				var query = ExpenseClaimDetails.Select().AsEnumerable().Where(_ => ((EPExpenseClaimDetails)_).ContractID == null || ProjectDefaultAttribute.IsNonProject(((EPExpenseClaimDetails)_).ContractID));
				foreach (EPExpenseClaimDetails item in query)
				{
					if (CustomerUpdateAsk.Current.CustomerUpdateAnswer == EPCustomerUpdateAsk.AllLines ||
						item.CustomerID == CustomerUpdateAsk.Current.OldCustomerID)
					{
						ExpenseClaimDetails.Cache.SetValueExt<EPExpenseClaimDetails.customerID>(item, CustomerUpdateAsk.Current.NewCustomerID);
						ExpenseClaimDetails.Cache.Update(item);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> changeCancel;
		[PXUIField(DisplayName = "Cancel")]
		[PXButton()]
		protected virtual IEnumerable ChangeCancel(PXAdapter adapter)
		{
			ExpenseClaim.Cache.SetValueExt<EPExpenseClaim.customerID>(ExpenseClaim.Current, CustomerUpdateAsk.Current.OldCustomerID);
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> SaveTaxZone;
		[PXUIField(DisplayName = "Yes")]
		[PXButton()]
		protected virtual IEnumerable saveTaxZone(PXAdapter adapter)
		{
			EPEmployee.Cache.SetValue<EPEmployee.receiptAndClaimTaxZoneID>(EPEmployee.Current, ExpenseClaim.Current.TaxZoneID);
			EPEmployee.Update(EPEmployee.Current);
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProject(PXAdapter adapter)
		{
			if (ExpenseClaimDetails.Current != null)
			{
				if (ProjectDefaultAttribute.IsProject(this, ExpenseClaimDetails.Current.ContractID))
				{
					var graph = CreateInstance<ProjectAccountingService>();
					graph.NavigateToProjectScreen(ExpenseClaimDetails.Current.ContractID, PXRedirectHelper.WindowMode.NewWindow);
				}
				else
				{
					var graph = CreateInstance<ContractMaint>();
					graph.Contracts.Current = graph.Contracts.Search<Contract.contractID>(ExpenseClaimDetails.Current.ContractID);
					throw new PXRedirectRequiredException(graph, true, Messages.ViewProject) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<EPExpenseClaim> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (APDocuments.Current != null)
			{
				RedirectionToOrigDoc.TryRedirect(APDocuments.Current.DocType, APDocuments.Current.RefNbr, BatchModule.AP, preferPrimaryDocForm: true);
			}
			return adapter.Get();
		}
		#endregion

		#region Selects Declartion

		[PXHidden]
		public PXSelect<Contract> Dummy;

		[PXCopyPasteHiddenFields(typeof(EPExpenseClaim.approved), typeof(EPExpenseClaim.released), typeof(EPExpenseClaim.hold), typeof(EPExpenseClaim.rejected), typeof(EPExpenseClaim.status))]
		[PXViewName(Messages.ExpenseClaim)]
		public PXSelectJoin<EPExpenseClaim,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
			Where<EPExpenseClaim.createdByID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
				Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>,
				Or<EPExpenseClaim.employeeID, WingmanUser<Current<AccessInfo.userID>>>>>>>> ExpenseClaim;

		[PXCopyPasteHiddenFields(typeof(EPExpenseClaim.branchID), typeof(EPExpenseClaim.taxZoneID))]
		public PXSelect<EPExpenseClaim,
			Where<EPExpenseClaim.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>> ExpenseClaimCurrent;

		[PXCopyPasteHiddenView]
		public PXSelect<APInvoice> APDocuments;

		[PXImport(typeof(EPExpenseClaim))]
		[PXViewName(Messages.ExpenseClaimDetails)]
		[PXCopyPasteHiddenFields(typeof(EPExpenseClaimDetails.curyTaxTotal))]

		public PXSelect<EPExpenseClaimDetails,
			Where<EPExpenseClaimDetails.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>,
			OrderBy<Asc<EPExpenseClaimDetails.submitedDate, Asc<EPExpenseClaimDetails.createdDateTime, Asc<EPExpenseClaimDetails.claimDetailID>>>>> ExpenseClaimDetails;
		public PXSelect<EPExpenseClaimDetails,
			Where<EPExpenseClaimDetails.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>>> ExpenseClaimDetailsCurrent;
		public PXSelect<EPExpenseClaimDetails,
						Where<EPExpenseClaimDetails.refNbr, Equal<Current<EPExpenseClaim.refNbr>>,
							And<EPExpenseClaimDetails.paidWith, NotEqual<EPExpenseClaimDetails.paidWith.cash>>>>
						CardReceipts;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<EPExpenseClaim.curyInfoID>>>> currencyinfo;
		//Don't delete. It is for copy/paste functionality.
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<EPExpenseClaimDetails.curyInfoID>>>> CurrencyInfoReceipt;
		public PXSetup<EPSetup> epsetup;
		public PXSetup<APSetup> apsetup;
		[PXCopyPasteHiddenView]
		public PXSetup<GL.GLSetup> glsetup;
		[PXViewName(Messages.Employee)]
		public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Optional<EPExpenseClaim.employeeID>>>> EPEmployee;

        //this is required for approval
        public PXSetup<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaim.employeeID>>>> EPEmployeeSetup;

		[PXCopyPasteHiddenView]
        public PXSelect<EPTax> Tax_Rows_Internal;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<EPTaxTran,
			InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxTran.taxID>>>,
			Where<EPTaxTran.claimDetailID, Equal<Optional<EPExpenseClaimDetails.claimDetailID>>>> Tax_Rows;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<EPTaxAggregate,
			InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxAggregate.taxID>>>,
			Where<EPTaxAggregate.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>> Taxes;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<EPTaxTran,
						InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxTran.taxID>>>,
								Where<EPTaxTran.refNbr, Equal<Current<EPExpenseClaim.refNbr>>,
								And<Tax.taxType, Equal<CSTaxType.use>>>> UseTaxes;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Approval)]
		public ExpenceClaimApproval<EPExpenseClaim> Approval;

		[PXReadOnlyView]
		public PXSelect<EPExpenseClaimDetailsForSubmit, 
			Where<True, Equal<False>>, 
			OrderBy<Asc<EPExpenseClaimDetailsForSubmit.expenseDate>>> ReceiptsForSubmit;

		public PXFilter<EPCustomerUpdateAsk> CustomerUpdateAsk;
		public PXFilter<TaxZoneUpdateAsk> TaxZoneUpdateAskView;
		
		#endregion

		public ExpenseClaimEntryReceiptExt ReceiptEntryExt => FindImplementation<ExpenseClaimEntryReceiptExt>();

		#region Execute Select
		protected virtual IEnumerable receiptsforsubmit()
		{
			PXSelectBase<EPExpenseClaimDetailsForSubmit> receiptsForSubmit = new PXSelect<EPExpenseClaimDetailsForSubmit,
			 Where2<Where<EPExpenseClaimDetailsForSubmit.refNbr, IsNull,
						  Or<EPExpenseClaimDetailsForSubmit.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>>,
					And<Where<EPExpenseClaimDetailsForSubmit.rejected, NotEqual<True>,
					And<EPExpenseClaimDetailsForSubmit.employeeID, Equal<Current<EPExpenseClaim.employeeID>>>>>>>(this);

			if (epsetup.Current.AllowMixedTaxSettingInClaims != true)
			{
				receiptsForSubmit.WhereAnd<
					Where<EPExpenseClaimDetailsForSubmit.taxCalcMode, Equal<Current2<EPExpenseClaim.taxCalcMode>>,
					And<Where<EPExpenseClaimDetailsForSubmit.taxZoneID, Equal<Current2<EPExpenseClaim.taxZoneID>>, 
                        Or<Where<EPExpenseClaimDetailsForSubmit.taxZoneID, IsNull, 
                            And<Current2<EPExpenseClaim.taxZoneID>, IsNull>>>>>>>();
			}

			HashSet<Int32?> receiptsInClaim = new HashSet<Int32?>();
			foreach (EPExpenseClaimDetails receiptInClaim in ExpenseClaimDetails.Select())
			{
				receiptsInClaim.Add(receiptInClaim.ClaimDetailID);
			}
			foreach (EPExpenseClaimDetailsForSubmit receiptForSubmit in receiptsForSubmit.Select())
			{
				if (receiptsInClaim.Contains(receiptForSubmit.ClaimDetailID))
				{
					continue;
				}
				yield return receiptForSubmit;
			}
		}

		protected virtual IEnumerable apdocuments()
		{
			if (ExpenseClaimDetails.SelectSingle() != null)
			{
				return PXSelectJoinGroupBy<APInvoice,
								InnerJoin<EPExpenseClaimDetails,
									On<APInvoice.docType, Equal<EPExpenseClaimDetails.aPDocType>,
										And<APInvoice.refNbr, Equal<EPExpenseClaimDetails.aPRefNbr>>>>,
								Where<EPExpenseClaimDetails.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>,
								Aggregate<GroupBy<APInvoice.docType,
											GroupBy<APInvoice.refNbr>>>>
								.Select(this);
			}
			else
			{
				return PXSelectReadonly<APInvoice,
								Where<APInvoice.origModule, Equal<BatchModule.moduleEP>,
										And<APInvoice.origRefNbr, Equal<Current<EPExpenseClaim.refNbr>>,
										And<APInvoice.origDocType, Equal<EPExpenseClaim.docType>>>>>
								.Select(this);
			}
		}

        protected virtual IEnumerable taxes()
        {
			if (ExpenseClaim.Current == null)
				yield break;

			IEnumerable<PXResult<EPTaxTran, Tax>> taxTrans = PXSelectJoin<EPTaxTran,
						InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxTran.taxID>>>,
				Where<EPTaxTran.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(this, ExpenseClaim.Current.RefNbr).AsEnumerable().Select(_ => (PXResult<EPTaxTran, Tax>)_);
            var taxGroups = taxTrans.GroupBy(_ => ((EPTaxTran)_).TaxID);
            foreach (var items in taxGroups)
            {

				Tax tax = null;
                EPTaxAggregate result = new EPTaxAggregate
                {
                    RefNbr = ExpenseClaim.Current.RefNbr,
                    CuryTaxableAmt = 0m,
                    CuryTaxAmt = 0m,
                    CuryExpenseAmt = 0m,
                    TaxableAmt = 0m,
                    TaxAmt = 0m,
                    ExpenseAmt = 0m,
                    CuryInfoID = ExpenseClaim.Current.CuryInfoID,
                };
                foreach (PXResult<EPTaxTran, Tax> item in items)
                {
					EPTaxTran taxTran = item;
					tax = item;
                    result.TaxRate = taxTran.TaxRate;
                    result.TaxID = taxTran.TaxID;
                    result.CuryTaxableAmt += taxTran.ClaimCuryTaxableAmt;
                    result.CuryTaxAmt += taxTran.ClaimCuryTaxAmt;
                    result.CuryExpenseAmt += taxTran.ClaimCuryExpenseAmt;
                    result.TaxableAmt += taxTran.TaxableAmt;
                    result.TaxAmt += taxTran.TaxAmt;
                    result.ExpenseAmt += taxTran.ExpenseAmt;
                    result.NonDeductibleTaxRate = taxTran.NonDeductibleTaxRate;
                }
                if (Taxes.Locate(result) == null)
                {
                    Taxes.Cache.SetStatus(result, PXEntryStatus.Held);
                }
                yield return new PXResult<EPTaxAggregate, Tax>(result, tax);
            }
            foreach (PXResult< EPTaxAggregate,Tax> legacyRow in PXSelectReadonly2<EPTaxAggregate,
						InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxAggregate.taxID>>>,
				Where<EPTaxAggregate.refNbr, Equal<Required<EPExpenseClaimDetails.refNbr>>>>.Select(this, ExpenseClaim.Current.RefNbr))
            {
				EPTaxAggregate legacyTax = legacyRow;

				if (Taxes.Locate(legacyTax) == null)
                {
                    Taxes.Cache.SetStatus(legacyTax, PXEntryStatus.Held);
                }
                yield return legacyRow;
            }
        }
        #endregion

        #region Graph Methods
        public ExpenseClaimEntry()
		{
			PXUIFieldAttribute.SetVisible<APInvoice.taxZoneID>(APDocuments.Cache, null, epsetup.Current.AllowMixedTaxSettingInClaims == true);
			PXUIFieldAttribute.SetVisible<APInvoice.taxCalcMode>(APDocuments.Cache, null, epsetup.Current.AllowMixedTaxSettingInClaims == true);
			if (epsetup.Current == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), PXMessages.LocalizeNoPrefix(Messages.EPSetup));

			PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.contractID>(ExpenseClaimDetails.Cache, null, PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() || PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>());

			Taxes.Cache.SetAllEditPermissions(false);
			PXUIFieldAttribute.SetEnabled(APDocuments.Cache, null, false);

			Approval.StatusHandler =
				(item) =>
				{
					switch (Approval.GetResult(item))
					{
						case ApprovalResult.Approved:
							item.Status = EPExpenseClaimStatus.ApprovedStatus;
							break;

						case ApprovalResult.Rejected:
							item.Status = EPExpenseClaimStatus.RejectedStatus;
							break;

						case ApprovalResult.PendingApproval:
							item.Status = EPExpenseClaimStatus.OpenStatus;
							break;

						case ApprovalResult.Submitted:
							item.Status = EPExpenseClaimStatus.ReleasedStatus;
							break;
					}
				};
		}

		public override void Persist()
		{
			List<EPExpenseClaim> inserted = null;
			if (epsetup.Current.HoldEntry != true)
			{
				inserted = new List<EPExpenseClaim>();
				foreach (EPExpenseClaim item in this.ExpenseClaim.Cache.Inserted)
					inserted.Add(item);
			}
			base.Persist();
			if (inserted != null)
			{
				foreach (EPExpenseClaim item in inserted)
				{
					if (this.ExpenseClaim.Cache.GetStatus(item) != PXEntryStatus.Inserted)
					{
						SubmitClaim(item);
					}
				}
				base.Persist();
			}
		}
		#endregion

		#region Events
		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				EPEmployee employee = EPEmployee.Select();
				if (employee != null && !string.IsNullOrEmpty(employee.CuryID))
				{
					e.NewValue = employee.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				EPEmployee employee = EPEmployee.Select();
				if (employee != null && !string.IsNullOrEmpty(employee.CuryRateTypeID))
				{
					e.NewValue = employee.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (ExpenseClaim.Cache.Current != null)
			{
				e.NewValue = ((EPExpenseClaim)ExpenseClaim.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.ExpenseClaimDetails.Cache);
				EPEmployee employee = EPEmployee.Select();
				if (ExpenseClaim.Current != null && employee != null && !(bool)employee.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(cache, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(cache, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(cache, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(cache, info, curyenabled);
			}
		}

		protected virtual void CurrencyInfo_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (ExpenseClaim.Current.CuryInfoID == (e.Row as CurrencyInfo).CuryInfoID)
			{
				CurrencyInfo curyInfoClaim =
					PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaim.curyInfoID>>>>.Select(this,
						ExpenseClaim.Current.CuryInfoID);
				CurrencyInfo oldCuryInfoClaim = e.OldRow as CurrencyInfo;

			foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
			{
					CurrencyInfo curyInfoReceipt =
						PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.Select(
							this,
							detail.CuryInfoID);

					if (detail.CreatedFromClaim == true &&
						oldCuryInfoClaim.CuryID == curyInfoReceipt.CuryID &&
						oldCuryInfoClaim.CuryRate == curyInfoReceipt.CuryRate &&
						oldCuryInfoClaim.CuryEffDate == curyInfoReceipt.CuryEffDate &&
						oldCuryInfoClaim.CuryRateTypeID == curyInfoReceipt.CuryRateTypeID)
					{
						CurrencyInfo curyInfoReceiptNew = (CurrencyInfo) currencyinfo.Cache.CreateCopy(curyInfoClaim);
						curyInfoReceiptNew.CuryInfoID = curyInfoReceipt.CuryInfoID;
						currencyinfo.Cache.Update(curyInfoReceiptNew);

						CurrencyInfoAttribute.SetEffectiveDate<EPExpenseClaim.docDate>(ExpenseClaim.Cache, ExpenseClaim.Current, typeof(EPExpenseClaimDetails.cardCuryInfoID));
					}
					else
				{
					EPExpenseClaimDetails oldDetail = (EPExpenseClaimDetails)ExpenseClaimDetails.Cache.CreateCopy(detail);
					ReceiptEntryExt.RecalcAmountInClaimCury(detail);
					ExpenseClaimDetails.Cache.RaiseRowUpdated(detail, oldDetail);
				}
			}
		}
		}
        #endregion
        #region Expense Claim Events		
        protected virtual void EPExpenseClaim_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            EPExpenseClaim doc = e.Row as EPExpenseClaim;
            if (doc == null)
            {
                return;
            }

            ExpenseClaimDetails.Cache.SetAllEditPermissions(true);
            Tax_Rows.Cache.SetAllEditPermissions(true);

            PXUIFieldAttribute.SetVisible<EPExpenseClaim.curyID>(cache, doc, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			EPEmployee employee = EPEmployee.Select();

			if (doc.Released == true)
            {
                PXUIFieldAttribute.SetEnabled(cache, doc, false);
                cache.AllowDelete = false;
                cache.AllowUpdate = false;
                ExpenseClaimDetails.Cache.SetAllEditPermissions(false);
                Tax_Rows.Cache.SetAllEditPermissions(false);
                release.SetEnabled(false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.refNbr>(cache, doc, true);
                editDetail.SetEnabled(true);
            }
            else
            {
	            bool curyenabled = true;
	            
	            if (employee != null && (bool)(employee.AllowOverrideCury ?? false) == false)
	            {
		            curyenabled = false;
	            }
	            else
	            {
		            EPExpenseClaimDetails cardReceipt = CardReceipts.SelectSingle();

		            if (cardReceipt != null)
		            {
						curyenabled = false;
					}
				}

				PXUIFieldAttribute.SetEnabled(cache, doc, true);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.status>(cache, doc, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approverID>(cache, doc, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.workgroupID>(cache, doc, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyDocBal>(cache, doc, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyVatExemptTotal>(cache, doc, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyVatTaxableTotal>(cache, doc, false);

                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyID>(cache, doc, curyenabled);

                if (doc.Hold != true)
                {
                    PXUIFieldAttribute.SetEnabled(cache, doc, false);
                    PXUIFieldAttribute.SetEnabled<EPExpenseClaim.refNbr>(cache, doc, true);
                    PXUIFieldAttribute.SetEnabled<EPExpenseClaim.docDesc>(cache, doc, cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
                    ExpenseClaimDetails.Cache.AllowInsert = false;
                    ExpenseClaimDetails.Cache.AllowDelete = false;
                    ExpenseClaimDetails.Cache.AllowUpdate = ignoreDetailReadOnly;
                }

                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.hold>(cache, e.Row, doc.Released != true);
                bool isApprover = doc.Status == EPExpenseClaimStatus.OpenStatus && Approval.IsApprover(doc);

                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.finPeriodID>(cache, e.Row, isApprover || doc.Status == EPExpenseClaimStatus.ApprovedStatus);
                if ((isApprover || doc.Status == EPExpenseClaimStatus.ApprovedStatus) && doc.Released != true)
                    APOpenPeriodAttribute.SetValidatePeriod<EPExpenseClaim.finPeriodID>(cache, e.Row, PeriodValidation.DefaultSelectUpdate);
                else
                    APOpenPeriodAttribute.SetValidatePeriod<EPExpenseClaim.finPeriodID>(cache, e.Row, PeriodValidation.Nothing);

                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approverID>(cache, doc, isApprover && doc.Status == EPExpenseClaimStatus.OpenStatus);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.workgroupID>(cache, doc, isApprover && doc.Status == EPExpenseClaimStatus.OpenStatus);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approvedByID>(cache, doc, false);

                bool isNotExistDetail = null == ExpenseClaimDetails.SelectSingle();
                PXUIFieldAttribute.SetEnabled<EPExpenseClaim.employeeID>(cache, doc, isNotExistDetail);

                cache.AllowDelete = true;
                cache.AllowUpdate = true;
                release.SetEnabled(doc.Approved == true);

                if (doc.EmployeeID == null || doc.BranchID == null)
                {
                    ExpenseClaimDetails.Cache.SetAllEditPermissions(false);
                }

            }
            CurrencyInfo info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<EPExpenseClaim.curyInfoID>>>>.SelectSingleBound(this, new object[] { doc });
            string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, info);
            if (string.IsNullOrEmpty(message) && info != null && info.CuryRate == null)
                message = CM.Messages.RateNotFound;
            if (string.IsNullOrEmpty(message))
                cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyID>(e.Row, null, null);
            else
                cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyID>(e.Row, null, new PXSetPropertyException(message, PXErrorLevel.Warning));



            bool allowEdit = this.Accessinfo.UserID == doc.CreatedByID;

            if (employee != null)
            {
                if (!allowEdit && this.Accessinfo.UserID == employee.UserID)
                {
                    allowEdit = true;
                }

                if (!allowEdit)
                {
                    EPWingman wingMan = PXSelectJoin<EPWingman,
                                                    InnerJoin<EPEmployee, On<EPWingman.wingmanID, Equal<EPEmployee.bAccountID>>>,
                                                    Where<EPWingman.employeeID, Equal<Required<EPWingman.employeeID>>,
                                                      And<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>>.Select(this, doc.EmployeeID, Accessinfo.UserID);
                    if (wingMan != null)
                    {
                        allowEdit = true;
                    }
                }
            }

            bool legacyClaim = false;
            if (doc.TaxZoneID != null && doc.Released == false)
            {

                foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
                {
                    if (detail.LegacyReceipt == true)
                    {
                        legacyClaim = true;
                        break;
                    }
                }
                if (!legacyClaim)
                {
                    foreach (EPTaxAggregate detail in PXSelectReadonly<EPTaxAggregate,
                        Where<EPTaxAggregate.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(this, doc.RefNbr))
                    {
                        if (detail != null)
                        {
                            legacyClaim = true;
                            break;
                        }
                    }
                }
                if (legacyClaim)
                {
                    ExpenseClaimDetails.Cache.SetAllEditPermissions(false);
                    Tax_Rows.Cache.SetAllEditPermissions(false);
                    cache.AllowUpdate = false;
                }
            }
            RaiseOrHideError<EPExpenseClaim.refNbr>(cache, doc, legacyClaim, Messages.LegacyClaimHeader, PXErrorLevel.Warning);
			Numbering receiptNumbering = PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(this, epsetup.Current.ReceiptNumberingID);
			createNew.SetEnabled(ExpenseClaimDetails.Cache.AllowInsert && cache.GetStatus(doc) != PXEntryStatus.Inserted && receiptNumbering?.UserNumbering == false);
			PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.claimDetailCD>(Caches[typeof(EPExpenseClaimDetails)], null, receiptNumbering?.UserNumbering == true);
            editDetail.SetEnabled(cache.GetStatus(doc) != PXEntryStatus.Inserted);
            submit.SetEnabled(cache.GetStatus(doc) != PXEntryStatus.Inserted);
            submitReceipt.SetEnabled(ExpenseClaimDetails.Cache.AllowInsert);
            showSubmitReceipt.SetEnabled(ExpenseClaimDetails.Cache.AllowInsert);

            edit.SetEnabled(allowEdit);
            ViewTaxes.SetEnabled(true);

            PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.curyTipAmt>(Caches[typeof(EPExpenseClaimDetails)], null, epsetup.Current.NonTaxableTipItem.HasValue);

			if (UseTaxes.Select().Count != 0)
			{
				cache.RaiseExceptionHandling<EPExpenseClaim.curyTaxTotal>(doc, doc.CuryTaxTotal,
					new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<EPExpenseClaim.curyTaxTotal>(doc, doc.CuryTaxTotal, null);
			}
		}

        protected virtual void EPExpenseClaim_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			CurrencyInfoAttribute.SetDefaults<EPExpenseClaim.curyInfoID>(cache, e.Row);
		}
		protected virtual void EPExpenseClaim_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaim row = e.Row as EPExpenseClaim;

			if (row?.CustomerID == null)
				cache.SetValueExt<EPExpenseClaim.customerLocationID>(row, null);
		}

		protected virtual void EPExpenseClaim_LocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<EPExpenseClaim.taxZoneID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<EPExpenseClaim.approved> e)
		{
			var row = (EPExpenseClaim)e.Row;

			if (row == null)
				return;

			DateTime? approveDate = null;
			if (row.Approved == true)
				approveDate = PXTimeZoneInfo.Now;

			ExpenseClaim.Cache.SetValueExt<EPExpenseClaim.approveDate>(row, approveDate);
		}

        protected virtual void EPExpenseClaim_TaxZoneID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            EPExpenseClaim row = (EPExpenseClaim)e.Row;

            if (row != null)
            {
                foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
                {
                    if (detail.TaxZoneID != (string)e.NewValue && detail.CreatedFromClaim != true)
                    {
                        string message;
                        if (epsetup.Current.AllowMixedTaxSettingInClaims == true)
                        {
                            message = Messages.TaxZoneCalcModeChangeOnReceiptsForm;
                        }
                        else
                        {
                            message = Messages.CantChangeTaxZone;
                        }
                        cache.RaiseExceptionHandling<EPExpenseClaim.taxZoneID>(row,
                            row.TaxZoneID,
                            new PXSetPropertyException(message, detail.ClaimDetailID));
                        e.Cancel = true;
                        e.NewValue = row.TaxZoneID;
                    }
                }
            }
        }

		protected virtual void EPExpenseClaim_TaxCalcMode_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			EPExpenseClaim row = (EPExpenseClaim)e.Row;

			if (row != null)
			{
				foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
				{
					if (detail.TaxCalcMode != (string)e.NewValue && detail.CreatedFromClaim != true)
                    {
                        string message;
                        if (epsetup.Current.AllowMixedTaxSettingInClaims == true)
                        {
                            message = Messages.TaxZoneCalcModeChangeOnReceiptsForm;
                        }
                        else
                        {
                            message = Messages.CantChangeTaxCalcMode;
                        }
                        cache.RaiseExceptionHandling<EPExpenseClaim.taxCalcMode>(row,
						row.TaxCalcMode,
						new PXSetPropertyException(message, detail.ClaimDetailID));
						e.Cancel = true;
						e.NewValue = row.TaxCalcMode;
					}
				}
			}
		}

		public virtual void _(Events.FieldVerifying<EPExpenseClaimDetails.corpCardID> e)
		{
			EPExpenseClaimDetails receipt = (EPExpenseClaimDetails)e.Row;

			int? newCorpCardID = (e.NewValue is int?)
				? (int?) e.NewValue
				: CACorpCard.PKCD.Find(this, (string) e.NewValue)?.CorpCardID;

			ReceiptEntryExt.VerifyClaimAndCorpCardCurrencies(
				newCorpCardID,
				ExpenseClaim.Current,
				() => e.NewValue = (e.NewValue is int?)
					? CACorpCard.PKID.Find(this, (int?) e.NewValue)?.CorpCardCD
					: e.NewValue);
		}

		public virtual void _(Events.FieldVerifying<EPExpenseClaimDetails.paidWith> e)
		{
			EPExpenseClaimDetails receipt = (EPExpenseClaimDetails)e.Row;
			string newPaidWith = (string)e.NewValue;

			ReceiptEntryExt.VerifyEmployeeAndClaimCurrenciesForCash(receipt, newPaidWith, ExpenseClaim.Current);

			decimal? amount = (decimal?)BqlHelper.GetValuePendingOrRow<EPExpenseClaimDetails.curyExtCost>(e.Cache, receipt);

			ReceiptEntryExt.VerifyIsPositiveForCorpCardReceipt(newPaidWith, amount);

			decimal? curyEmployeePart = (decimal?)BqlHelper.GetValuePendingOrRow<EPExpenseClaimDetails.curyEmployeePart>(e.Cache, receipt);

			ReceiptEntryExt.VerifyEmployeePartIsZeroForCorpCardReceipt(newPaidWith, curyEmployeePart);
		}

		public virtual void _(Events.FieldVerifying<EPExpenseClaimDetailsForSubmit.selected> e)
		{
			EPExpenseClaimDetailsForSubmit receipt = (EPExpenseClaimDetailsForSubmit)e.Row;

			bool? newSelected = (bool?)e.NewValue;

			if (newSelected == true)
			{
				ReceiptEntryExt.VerifyClaimAndCorpCardCurrencies(receipt.CorpCardID, ExpenseClaim.Current);

				ReceiptEntryExt.VerifyEmployeeAndClaimCurrenciesForCash(receipt, receipt.PaidWith, ExpenseClaim.Current);
			}
		}

		protected virtual void EPExpenseClaim_DocDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<EPExpenseClaim.docDate>(cache, e);
		}
		
		protected virtual void EPExpenseClaim_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<EPExpenseClaim.curyInfoID>(cache, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					cache.RaiseExceptionHandling<EPExpenseClaim.docDate>(e.Row, ((EPExpenseClaim)e.Row).DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}
				if (info != null)
				{
					((EPExpenseClaim)e.Row).CuryID = info.CuryID;
				}
			}
			cache.SetDefaultExt<EPExpenseClaim.locationID>(e.Row);
			cache.SetDefaultExt<EPExpenseClaim.departmentID>(e.Row);
			cache.SetDefaultExt<EPExpenseClaim.branchID>(e.Row);
		}

		protected virtual void EPExpenseClaim_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as EPExpenseClaim;
			if (row != null && e.Operation != PXDBOperation.Delete && row.Hold == false)
			{
				if (glsetup.Current.RoundingLimit < Math.Abs(row.TaxRoundDiff ?? 0m))
				{
					throw new PXException(AP.Messages.RoundingAmountTooBig, currencyinfo.Current?.BaseCuryID, row.TaxRoundDiff,
						PXDBQuantityAttribute.Round(glsetup.Current.RoundingLimit));
				}
			}
		}

		protected virtual void EPExpenseClaim_CuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PXFormulaAttribute.CalcAggregate<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes>(ExpenseClaimDetails.Cache, e.Row);
		}

		protected virtual void EPExpenseClaim_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            EPExpenseClaim row = e.Row as EPExpenseClaim;
            EPExpenseClaim oldRow = e.OldRow as EPExpenseClaim;

            var receipts = ExpenseClaimDetails.Select().AsEnumerable();

            if (row == null || oldRow == null)
                return;

            if (row.CustomerID != oldRow.CustomerID)
            {
                var query = receipts.Where(_ => ((EPExpenseClaimDetails)_).ContractID == null || ProjectDefaultAttribute.IsNonProject(((EPExpenseClaimDetails)_).ContractID));
                if (query.Count() != 0)
                {
                    CustomerUpdateAsk.Current.NewCustomerID = row.CustomerID;
                    CustomerUpdateAsk.Current.OldCustomerID = oldRow.CustomerID;
                    CustomerUpdateAsk.AskExt();
                }
            }


            if (ExpenseClaim.Current != null && ExpenseClaim.Current.Released == true)
            {
                foreach (EPExpenseClaimDetails receipt in receipts)
                {
                    receipt.Status = EPExpenseClaimDetailsStatus.ReleasedStatus;
                    receipt.Released = true;
                }
            }
            if (row.TaxCalcMode != oldRow.TaxCalcMode)
            {
	            ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(this, row.RefNbr);
                foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
                {
					detail.TaxCalcMode = row.TaxCalcMode;
                    ExpenseClaimDetails.Update(detail);
                }
            }
            if (row.TaxZoneID != oldRow.TaxZoneID)
            {
	            ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(this, row.RefNbr);
                foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
                {
					if(detail.PaidWith != EPExpenseClaimDetails.paidWith.CardPersonalExpense)
					{
						detail.TaxZoneID = row.TaxZoneID;
					}

                    ExpenseClaimDetails.Update(detail);
                }
                if (e.ExternalCall && !this.IsMobile)
                {
                    if (!string.IsNullOrEmpty(row.TaxZoneID))
                    {
                        EPEmployee employee = EPEmployee.Select();

                        string taxZoneID = ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.GetTaxZoneID(this, employee);

                        if (row.TaxZoneID != taxZoneID)
                        {
                            EPEmployee.Current = employee;
                            TaxZoneUpdateAskView.View.AskExt();
                        }
                    }
                }
            }
        }

        protected virtual void EPExpenseClaim_Hold_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            EPExpenseClaim row = e.Row as EPExpenseClaim;
			if (row != null)
			{
                foreach (EPExpenseClaimDetails detail in ExpenseClaimDetails.Select())
				{
                    ExpenseClaimDetails.Cache.SetValueExt<EPExpenseClaimDetails.holdClaim>(detail, row.Hold);
				}
			}
        }

		public virtual void ValidateProjectAndProjectTask(EPExpenseClaimDetails info)
		{
			if (info != null)
			{
				string errProjectMsg = PXUIFieldAttribute.GetError<EPExpenseClaimDetails.contractID>(ExpenseClaimDetails.Cache, info);
				if (!string.IsNullOrEmpty(errProjectMsg) && errProjectMsg.Equals(PXLocalizer.Localize(PM.Messages.ProjectExpired)))
				{
					PXUIFieldAttribute.SetError<EPExpenseClaimDetails.contractID>(ExpenseClaimDetails.Cache, info, null);
				}

				if (info.ContractID != null)
				{
					PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<EPExpenseClaimDetails.contractID>>>>.SelectWindowed(this, 0, 1, info.ContractID);
					if (project != null && project.ExpireDate != null && info.ExpenseDate != null)
					{
						if (info.ExpenseDate > project.ExpireDate)
						{
							ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.contractID>(
									info, info.ContractID,
									new PXSetPropertyException(
									PM.Messages.ProjectExpired,
									PXErrorLevel.Warning));
						}
					}
				}

				string errProjTaskMsg = PXUIFieldAttribute.GetError<EPExpenseClaimDetails.taskID>(ExpenseClaimDetails.Cache, info);
				if (!string.IsNullOrEmpty(errProjTaskMsg) && (errProjTaskMsg.Equals(PXLocalizer.Localize(PM.Messages.ProjectTaskExpired))
														   || errProjTaskMsg.Equals(PXLocalizer.Localize(PM.Messages.TaskIsCompleted))))
				{
					PXUIFieldAttribute.SetError<EPExpenseClaimDetails.taskID>(ExpenseClaimDetails.Cache, info, null);
				}
				if (info.TaskID != null)
				{
					PMTask projectTask = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<EPExpenseClaimDetails.taskID>>>>.SelectWindowed(this, 0, 1, info.TaskID);
					if (projectTask != null && projectTask.EndDate != null && info.ExpenseDate != null)
					{
						
						if (info.ExpenseDate > projectTask.EndDate && projectTask.Status != ProjectTaskStatus.Completed)
						{
							ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(
									info, info.TaskID,
									new PXSetPropertyException(
									PM.Messages.ProjectTaskExpired,
									PXErrorLevel.Warning));
						}
						else if (projectTask.Status == ProjectTaskStatus.Completed)
						{
							ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(
									info, info.TaskID,
									new PXSetPropertyException(
									PM.Messages.TaskIsCompleted,
									PXErrorLevel.Warning));
						}
					}
				}
			}
		}

		#endregion
		#region ExpenseClaimDetails events
		protected virtual void EPExpenseClaimDetails_CuryTipAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (row != null)
			{
				if (row.CuryTipAmt != 0)
				{
					var item = (InventoryItem)PXSelect<InventoryItem,
												Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, epsetup.SelectSingle().NonTaxableTipItem);
					ExpenseClaimDetails.SetValueExt<EPExpenseClaimDetails.taxTipCategoryID>(row, item.TaxCategoryID);
				}
				else
				{
					ExpenseClaimDetails.SetValueExt<EPExpenseClaimDetails.taxTipCategoryID>(row, null);
				}
			}
		}

		protected virtual void EPExpenseClaimDetails_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (row != null)
			{
				if (IsImportFromExcel) //Calculate receipt CuryClaim fields without summing of ExpenseClaim totals
					ReceiptEntryExt.RecalcAmountInClaimCuryForReceipt(row);

				if (ExpenseClaim.Current != null)
			{
					CurrencyInfo curyInfoClaim =
						PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaim.curyInfoID>>>>.Select(this,
							ExpenseClaim.Current.CuryInfoID);
					CurrencyInfo curyInfoReceiptNew = (CurrencyInfo)currencyinfo.Cache.CreateCopy(curyInfoClaim);
					curyInfoReceiptNew.CuryInfoID = row.CuryInfoID;
					currencyinfo.Cache.Update(curyInfoReceiptNew);

				ReceiptEntryExt.SubmitReceiptExt(ExpenseClaim.Cache, cache, ExpenseClaim.Current, row);
			}
				ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(this, row.RefNbr);
        }
        }

        protected virtual void EPExpenseClaimDetails_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;

            EPExpenseClaimDetails oldrow = (EPExpenseClaimDetails)e.OldRow;

            if (row.RefNbr != oldrow.RefNbr || row.TaxCategoryID != oldrow.TaxCategoryID || row.TaxCalcMode != oldrow.TaxCalcMode || row.TaxZoneID != oldrow.TaxZoneID)
            {
	            ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(this, row.RefNbr);
            }

        }

		protected virtual void EPExpenseClaimDetails_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPExpenseClaimDetails detail = e.Row as EPExpenseClaimDetails;
			if (detail != null)
			{
				if (cache.GetStatus(detail) != PXEntryStatus.InsertedDeleted)
				{
					ReceiptEntryExt.RemoveReceipt(ExpenseClaimDetails.Cache, detail);
					e.Cancel = true;
					ExpenseClaimDetails.View.RequestRefresh();
				}
				else
				{
					ReceiptEntryExt.RemoveReceipt(ExpenseClaimDetails.Cache, detail, skipReceiptCacheUpdate: true);
				}
			}
		}

		protected virtual void EPExpenseClaimDetails_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
			if (row != null && e.Operation != PXDBOperation.Delete)
			{
				if (row.ContractID != null && (bool)row.Billable && row.TaskID != null)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID);
					if (task != null && !(bool)task.VisibleInAP)
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(e.Row, task.TaskCD,
							new PXSetPropertyException(PM.Messages.TaskInvisibleInModule, task.TaskCD, BatchModule.AP));
				}
				if ((row.CuryTipAmt ?? 0) != 0 && epsetup?.Current?.NonTaxableTipItem == null)
				{
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTipAmt>(row,
						row.CuryTipAmt,
						new PXRowPersistingException(nameof(EPExpenseClaim), null, Messages.TipItemIsNotDefined));
				}
				if (row.CuryTipAmt > 0 && row.CuryExtCost < 0 || row.CuryTipAmt < 0 && row.CuryExtCost > 0)
				{
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTipAmt>(row,
						row.CuryTipAmt,
						new PXSetPropertyException(Messages.TipSign));
				}
			}
		}

		protected virtual void EPExpenseClaimDetails_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (row != null)
			{
				bool enabledEditReceipt = (row.Hold == true || !enabledApprovalReceipt) && ((ExpenseClaimCurrent.Current?.Hold ?? false) == true);
				bool enabledFinancialDetails = (row.Rejected != true) && (row.Released != true) && (row.HoldClaim != false);

				if (!enabledEditReceipt)
				{
					PXUIFieldAttribute.SetEnabled(cache, row, false);
				}
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.expenseAccountID>(cache, row, enabledFinancialDetails);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.expenseSubID>(cache, row, enabledFinancialDetails);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesAccountID>(cache, row, enabledFinancialDetails && (row.Billable == true));
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesSubID>(cache, row, enabledFinancialDetails && (row.Billable == true));
				cache.Adjust<PXUIFieldAttribute>()
					.For<EPExpenseClaimDetails.taxCategoryID>(ui => ui.Enabled = enabledFinancialDetails
					                                                             && row.BankTranDate == null
					                                                             && row.PaidWith != EPExpenseClaimDetails.paidWith.CardPersonalExpense)
					.SameFor<EPExpenseClaimDetails.taxCalcMode>();
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.curyID>(cache, row, IsCopyPasteContext);
				Tax_Rows.Cache.SetAllEditPermissions(enabledEditReceipt && row.BankTranDate == null);

				Numbering receiptNumbering =PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(this, epsetup.Current.ReceiptNumberingID);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.claimDetailCD>(cache, row, receiptNumbering?.UserNumbering == true && cache.GetStatus(row) == PXEntryStatus.Inserted);
			}
        }

		protected virtual void EPExpenseClaimDetails_ExpenseSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.ExpenseSubID_FieldDefaulting(sender, e, epsetup.Current.ExpenseSubMask);
		}

		protected virtual void EPExpenseClaimDetails_SalesSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.SalesSubID_FieldDefaulting(sender, e, epsetup.Current.SalesSubMask);
		}

		protected virtual void EPExpenseClaimDetails_CuryEmployeePart_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Decimal? newVal = e.NewValue as Decimal?;
			if (newVal < 0)
				throw new PXSetPropertyException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<EPExpenseClaimDetails.curyEmployeePart>(cache));
		}
		protected virtual void EPExpenseClaimDetails_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = enabledApprovalReceipt;
			e.Cancel = true;
		}
		protected virtual void EPExpenseClaimDetails_Approved_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = !enabledApprovalReceipt;
			e.Cancel = true;
		}
		protected virtual void EPExpenseClaimDetails_Status_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = enabledApprovalReceipt ? EPExpenseClaimDetailsStatus.HoldStatus : EPExpenseClaimDetailsStatus.ApprovedStatus;
			e.Cancel = true;
		}

		protected virtual void EPExpenseClaimDetails_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;

			if (row?.CustomerID == null)
				cache.SetValueExt<EPExpenseClaimDetails.customerLocationID>(row, null);
		}

		protected virtual void EPExpenseClaimDetails_TaxZoneID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ExpenseClaimCurrent.Current.TaxZoneID;
		}

		protected virtual void EPExpenseClaimDetails_CustomerLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ExpenseClaimCurrent.Current.CustomerLocationID;
			e.Cancel = true;
		}

		protected virtual void EPExpenseClaimDetails_TaxCalcMode_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ExpenseClaimCurrent.Current.TaxCalcMode;
		}

		protected virtual void EPExpenseClaimDetails_TaxCategoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
			if (row != null)
			{
				row.CuryTaxableAmtFromTax = 0;
				row.TaxableAmtFromTax = 0;
				row.CuryTaxAmt = 0;
				row.TaxAmt = 0;
			}
		}
		#endregion

        #region EPTaxTran Events
		protected virtual void EPTaxTran_CuryTaxAmt_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			EPTaxTran row = (EPTaxTran)e.Row;
			EPExpenseClaimDetails doc = ExpenseClaimDetails.Current;
			if (e.NewValue != null)
			{
				decimal newValue = (decimal)e.NewValue;
				if (row != null && doc != null)
				{
					if (newValue > 0 && doc.CuryExtCost < 0 || newValue < 0 && doc.CuryExtCost > 0)
					{
						cache.RaiseExceptionHandling<EPTaxTran.curyTaxAmt>(row,
							row.CuryTaxAmt,
							new PXSetPropertyException(Messages.TaxSign));
						e.NewValue = row.CuryTaxAmt;
					}
				}
			}
		}
		protected virtual void EPTaxTran_CuryTaxableAmt_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			EPTaxTran row = (EPTaxTran)e.Row;
			EPExpenseClaimDetails doc = ExpenseClaimDetails.Current;
			if (e.NewValue != null)
			{
				decimal newValue = (decimal)e.NewValue;
				if (row != null && doc != null)
				{
					if (newValue > 0 && doc.CuryExtCost < 0 || newValue < 0 && doc.CuryExtCost > 0)
					{
						cache.RaiseExceptionHandling<EPTaxTran.curyTaxableAmt>(row,
							row.CuryTaxableAmt,
							new PXSetPropertyException(Messages.TaxableSign));
						e.NewValue = row.CuryTaxableAmt;
					}
				}
			}
		}
		protected virtual void EPTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<EPTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}
		#endregion
		#endregion

		#region CacheAttached
		#region EPExpenseClaimDetails Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBIdentity(IsKey = true)]
		protected virtual void EPExpenseClaimDetails_ClaimDetailID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[EPTax]
		protected virtual void EPExpenseClaimDetails_TaxCategoryID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Tax Amount", Enabled = false, Visible = true)]
		protected virtual void EPExpenseClaimDetails_CuryTaxTotal_CacheAttached(PXCache cache)
		{
		}


		[PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXUIField(DisplayName = "Currency")]
        protected virtual void EPExpenseClaimDetails_CuryID_CacheAttached(PXCache cache)
        {
        }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Tax Calculation Mode", Enabled = false, Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		protected virtual void EPExpenseClaimDetails_TaxCalcMode_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(true)]
		protected virtual void EPExpenseClaimDetails_CreatedFromClaim_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(EPExpenseClaim.taxZoneID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax Zone", Required = false, Enabled = false, Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		protected virtual void EPExpenseClaimDetails_TaxZoneID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Claimed by", Visible = false)]
		protected virtual void EPExpenseClaimDetails_EmployeeID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region EPApproval Cahce Attached
		[PXDBDate()]
		[PXDefault(typeof(EPExpenseClaim.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(EPExpenseClaim.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid]
		[PXDefault(typeof(Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<EPExpenseClaim.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(EPExpenseClaim.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(EPExpenseClaim.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(EPExpenseClaim.curyDocBal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(EPExpenseClaim.docBal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region APInvoice Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.taxZoneID), Filterable = true)]
		protected virtual void APInvoice_TaxZoneID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[APDocType.List()]
		protected virtual void _(Events.CacheAttached<APInvoice.docType> e)
		{
		}
		#endregion
		#endregion

		#region Function

		private bool ignoreDetailReadOnly = false;
		protected bool enabledApprovalReceipt => PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && epsetup.Current.ClaimDetailsAssignmentMapID != null;


		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public void CheckAllowedUser()
		{
			EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employeeByUserID == null && !ProxyIsActive)
			{
				if (IsExport || IsImport)
					throw new PXException(Messages.MustBeEmployee);
				else
					Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
			}
		}

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
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

		public void PrepareItems(string viewName, IEnumerable items) { }

		#endregion
	}
}