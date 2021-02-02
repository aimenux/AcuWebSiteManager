using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.TM;
using PX.Data.EP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using static PX.Objects.Common.UIState;

namespace PX.Objects.EP
{
	public class ExpenseClaimDetailMaint : PXGraph<ExpenseClaimDetailMaint>
	{
		public class ExpenseClaimDetailMaintReceiptExt : ExpenseClaimDetailGraphExtBase<ExpenseClaimDetailMaint>
		{
			public override PXSelectBase<EPExpenseClaimDetails> Receipts => Base.ClaimDetails;
		}

		[PXSelector(typeof(PX.SM.Users.pKID), SubstituteKey = typeof(PX.SM.Users.fullName), DescriptionField = typeof(PX.SM.Users.fullName), CacheGlobal = true)]
		[PXDBGuid(false)]
		[PXUIField(DisplayName = "Created by", Enabled = false)]
		protected virtual void EPExpenseClaimDetails_CreatedByID_CacheAttached(PXCache sender) { }

		[PXDBString(10, IsUnicode = true)]
		protected virtual void EPExpenseClaimDetails_TaxCategoryID_CacheAttached(PXCache sender) { }


		#region Action

		public PXCancel<ExpenseClaimDetailsFilter> Cancel;

		public PXAction<ExpenseClaimDetailsFilter> AddNew;
		[PXInsertButton]
		[PXUIField(DisplayName = "")]
		[PXEntryScreenRights(typeof(ExpenseClaimDetailsFilter), nameof(ExpenseClaimDetailEntry.Insert))]
		protected virtual void addNew()
		{
			using (new PXPreserveScope())
			{
				ExpenseClaimDetailEntry graph = (ExpenseClaimDetailEntry)PXGraph.CreateInstance(typeof(ExpenseClaimDetailEntry));
				graph.Clear(PXClearOption.ClearAll);
				EPExpenseClaimDetails claimDetails = (EPExpenseClaimDetails)graph.ClaimDetails.Cache.CreateInstance();
				graph.ClaimDetails.Insert(claimDetails);
				graph.ClaimDetails.Cache.IsDirty = false;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<ExpenseClaimDetailsFilter> EditDetail;
		[PXEditDetailButton]
		[PXUIField(DisplayName = "")]
		protected virtual void editDetail()
		{
			EPExpenseClaimDetails row = ClaimDetails.Current;
			if (row == null) return;
			PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<ExpenseClaimDetailsFilter> delete;
		[PXDeleteButton]
		[PXUIField(DisplayName = "")]
		[PXEntryScreenRights(typeof(ExpenseClaimDetailsFilter))]
		protected void Delete()
		{
			if (ClaimDetails.Current == null) return;

			if (ClaimDetails.Current.RefNbr != null)
				throw new PXException(Messages.ReceiptMayNotBeDeleted);

			ClaimDetails.Cache.Delete(ClaimDetails.Current);
			this.Persist();
		}

		public PXAction<ExpenseClaimDetailsFilter> viewClaim;
		[PXButton]
		protected virtual void ViewClaim()
		{
			if (ClaimDetails.Current != null && ClaimDetails.Current.RefNbr != null)
			{
				EPExpenseClaim claim = PXSelect<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaimDetails.refNbr>>>>.SelectSingleBound(this, null, ClaimDetails.Current.RefNbr);
				if (claim == null) return;
				PXRedirectHelper.TryRedirect(PXGraph.CreateInstance<ExpenseClaimEntry>(), claim, PXRedirectHelper.WindowMode.NewWindow);
			}
		}


		#endregion


		#region Select
		public PXFilter<ExpenseClaimDetailsFilter> Filter;
		//Not delete
		public PXSelect<CT.Contract> DummyContracts;
		[PXFilterable()]
		[PXViewName(Messages.ClaimDetailsView)]
		public PXFilteredProcessingJoin<
				EPExpenseClaimDetails,
				ExpenseClaimDetailsFilter,
				LeftJoin<EPExpenseClaim,
					  On<EPExpenseClaim.refNbr, Equal<EPExpenseClaimDetails.refNbr>>,
				LeftJoin<EPEmployee,
					  On<EPEmployee.bAccountID, Equal<EPExpenseClaimDetails.employeeID>>>>,
				Where<Current2<ExpenseClaimDetailsFilter.employeeID>, IsNotNull,
									And<EPExpenseClaimDetails.employeeID, Equal<Current2<ExpenseClaimDetailsFilter.employeeID>>,
									Or<Current2<ExpenseClaimDetailsFilter.employeeID>, IsNull,
									And<Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
									Or<EPExpenseClaimDetails.createdByID, Equal<Current<AccessInfo.userID>>,
									Or<EPExpenseClaimDetails.employeeID, WingmanUser<Current<AccessInfo.userID>>,
									Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
									Or<EPExpenseClaimDetails.noteID, Approver<Current<AccessInfo.userID>>,
									Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>>>>>>>>>>>,
				OrderBy<Desc<EPExpenseClaimDetails.claimDetailID>>> ClaimDetails;

		[PXHidden]
		public PXSelect<CR.BAccount> baccount;
		[PXHidden]
		public PXSelect<AP.Vendor> vendor;
		[PXHidden]
		public PXSelect<EPEmployee> employee;
		[PXHidden]
		public PXSelect<EPTax> TaxesRows;
		[PXHidden]
		public PXSelect<EPTaxTran> Taxes;
		#endregion

		public ExpenseClaimDetailMaint()
		{
			ClaimDetails.SetProcessDelegate(ClaimDetail);
			ClaimDetails.SetProcessCaption(Messages.Claim);
			ClaimDetails.SetProcessAllCaption(Messages.ClaimAll);
			ClaimDetails.SetSelected<EPExpenseClaimDetails.selected>();
			ClaimDetails.AllowDelete = false;
			ClaimDetails.AllowInsert = false;
			ClaimDetails.AllowUpdate = true;

			PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.branchID>(ClaimDetails.Cache, null, false);
		}


        #region Handler
        protected virtual void EPExpenseClaimDetails_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
            if (row != null)
            {
                bool legacy = row.LegacyReceipt == true && row.Released == false && !String.IsNullOrEmpty(row.TaxZoneID);
                EPEmployee employeeRow = PXSelect<EPEmployee, 
                    Where<EPEmployee.bAccountID, Equal<Required<EPExpenseClaimDetails.employeeID>>>>.Select(this, row.EmployeeID);
                string taxZoneID = ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.GetTaxZoneID(this, employeeRow);
                bool notMatchtaxZone = String.IsNullOrEmpty(row.TaxZoneID) && !String.IsNullOrEmpty(taxZoneID);

                RaiseOrHideError<EPExpenseClaimDetails.claimDetailID>(cache, 
                    row, 
                    legacy || notMatchtaxZone, 
                    notMatchtaxZone ? Messages.TaxZoneEmpty : Messages.LegacyReceipt, PXErrorLevel.RowWarning);
            }

        }


		protected virtual void EPExpenseClaimDetails_Selected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (row != null && row.RefNbr != null && (bool)e.NewValue == true)
				sender.RaiseExceptionHandling<EPExpenseClaimDetails.selected>(row, e.NewValue, new PXSetPropertyException(Messages.ReceiptIsClaimed, PXErrorLevel.RowWarning));
		}


		#endregion

		[CM.CurrencyInfo(ModuleCode = "EP", CuryIDField = "curyID", CuryDisplayName = "Currency", Enabled = false)]
		[PXDBLong()]
		protected virtual void EPExpenseClaimDetails_CuryInfoID_CacheAttached(PXCache cache)
		{
		}

		[PXDBLong()]
		protected virtual void EPExpenseClaimDetails_ClaimCuryInfoID_CacheAttached(PXCache cache)
		{
		}

		#region Function
		private class Receipts
		{
			public int? employee;
			public int? branch;
			public int? customer;
			public int? customerLocation;
			public string claimCuryID;
			public IEnumerable<EPExpenseClaimDetails> details;
		};

		private static void ClaimDetail(List<EPExpenseClaimDetails> details)
		{
			ClaimDetail(details, false, false);
		}

		private static void ClaimDetail(List<EPExpenseClaimDetails> details, bool isApiContext, bool singleOperation)
		{
			ExpenseClaimEntry expenseClaimEntry = CreateInstance<ExpenseClaimEntry>();
			PXSetup<EPSetup> epsetup = new PXSetup<EPSetup>(PXGraph.CreateInstance(typeof(ExpenseClaimDetailEntry)));
			bool enabledApprovalReceipt = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && epsetup.Current.ClaimDetailsAssignmentMapID != null;
			bool isError = false;
			bool notAllApproved = false;
		    Dictionary<string, EPExpenseClaim> result = new Dictionary<string, EPExpenseClaim>();

			IEnumerable<Receipts> List;

			if (epsetup.Current.AllowMixedTaxSettingInClaims == true)
			{
				List = details.Where(item => string.IsNullOrEmpty(item.RefNbr)).OrderBy(detail => detail.ClaimDetailID).GroupBy(
											item => new
											{
												item.EmployeeID,
												item.BranchID,
												item.CustomerID,
												item.CustomerLocationID,
												ClaimCuryID = GetClaimCuryID(expenseClaimEntry, item)
											},
											(key, item) => new Receipts
											{
												employee = key.EmployeeID,
												branch = key.BranchID,
												customer = key.CustomerID,
												customerLocation = key.CustomerLocationID,
												claimCuryID = key.ClaimCuryID,
												details = item
											}
											);
			}
			else
			{
				List = details.Where(item => string.IsNullOrEmpty(item.RefNbr)).OrderBy(detail => detail.ClaimDetailID).GroupBy(
											item => new
											{
												item.EmployeeID,
												item.BranchID,
												item.CustomerID,
												item.CustomerLocationID,
												item.TaxZoneID,
												item.TaxCalcMode,
												ClaimCuryID = GetClaimCuryID(expenseClaimEntry, item)
											},
											(key, item) => new Receipts
											{
												employee = key.EmployeeID,
												branch = key.BranchID,
												customer = key.CustomerID,
												customerLocation = key.CustomerLocationID,
												claimCuryID = key.ClaimCuryID,
												details = item
											}
											);
			}

			string errorMessage = null;

			foreach (Receipts item in List)
			{
				isError = false;
				notAllApproved = false;
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					expenseClaimEntry.Clear();
					expenseClaimEntry.SelectTimeStamp();
					EPExpenseClaim expenseClaim = (EPExpenseClaim)expenseClaimEntry.ExpenseClaim.Cache.CreateInstance();
					expenseClaim.EmployeeID = item.employee;
					expenseClaim.BranchID = item.branch;
					expenseClaim.CustomerID = item.customer;
					expenseClaim.DocDesc = EP.Messages.SubmittedReceipt;
					expenseClaim = expenseClaimEntry.ExpenseClaim.Update(expenseClaim);
					expenseClaim.CuryID = item.claimCuryID;
					expenseClaim = expenseClaimEntry.ExpenseClaim.Update(expenseClaim);
					expenseClaim.CustomerLocationID = item.customerLocation;
					expenseClaim.TaxCalcMode = item.details.First().TaxCalcMode;
					expenseClaim.TaxZoneID = item.details.First().TaxZoneID;

					foreach (EPExpenseClaimDetails detail in item.details)
					{
						PXProcessing<EPExpenseClaimDetails>.SetCurrentItem(detail);
						
						if (detail.Approved ?? false)
						{
							try
							{
								if (detail.IsPaidWithCard)
								{
									EPEmployee employee = 
										PXSelect<EPEmployee,
											Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
											.Select(expenseClaimEntry, item.employee);

									if (employee.AllowOverrideCury != true && detail.CardCuryID != employee.CuryID)
									{
										errorMessage = PXMessages.Localize(Messages.ClaimCannotBeCreatedForReceiptBecauseCuryCannotBeOverriden);

										isError = true;
									}
								}

								if (!isError && detail.TipAmt != 0 && epsetup.Current.NonTaxableTipItem == null)
								{
									errorMessage = Messages.TipItemIsNotDefined;
									isError = true;
								}

								if(!isError)
								{
									expenseClaimEntry.ReceiptEntryExt.SubmitReceiptExt(expenseClaimEntry.ExpenseClaim.Cache, 
										expenseClaimEntry.ExpenseClaimDetails.Cache, expenseClaimEntry.ExpenseClaim.Current, detail);

									expenseClaimEntry.Save.Press();
									if (!result.ContainsKey(expenseClaim.RefNbr))
									{
										result.Add(expenseClaim.RefNbr, expenseClaim);
									}
									detail.RefNbr = expenseClaim.RefNbr;

									PXProcessing<EPExpenseClaimDetails>.SetProcessed();
								}

							}
							catch (Exception ex)
							{
								errorMessage = ex.Message;
								isError = true;
							}
						}
						else
						{
							errorMessage = enabledApprovalReceipt
								? Messages.ReceiptNotApproved
								: Messages.ReceiptTakenOffHold;

							notAllApproved = true;
						}

						if (errorMessage != null)
						{
							PXProcessing<EPExpenseClaimDetails>.SetError(errorMessage);
						}
					}
					if (!isError)
					{
						ts.Complete();
					}
				}
			}

			if (!isError && !notAllApproved)
			{
				if (result.Count == 1 && isApiContext == false)
				{
					expenseClaimEntry = CreateInstance<ExpenseClaimEntry>();
					PXRedirectHelper.TryRedirect(expenseClaimEntry, result.First().Value, PXRedirectHelper.WindowMode.InlineWindow);
				}
			}
			else
			{
				PXProcessing<EPExpenseClaimDetails>.SetCurrentItem(null);
				if (singleOperation)
				{
					throw new PXException(errorMessage);
				}
				else
				{
					throw new PXException(Messages.ErrorProcessingReceipts);
				}
			}
		}

		public static string GetClaimCuryID(PXGraph graph, EPExpenseClaimDetails receipt)
		{
			if (receipt.CorpCardID != null)
			{
				return CACorpCardsMaint.GetCardCashAccount(graph, receipt.CorpCardID).CuryID;
			}
			else
			{
				EPEmployee employee =
					PXSelect<EPEmployee,
							Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
						.Select(graph, receipt.EmployeeID);

				return employee.CuryID ?? ((Company) PXSelect<Company>.Select(graph)).BaseCuryID;
			}
		}

		public static void ClaimSingleDetail(EPExpenseClaimDetails details, bool isApiContext = false)
		{
			ClaimDetail(new List<EPExpenseClaimDetails> { details }, isApiContext, true);
		}
		#endregion

		[Serializable]
		public partial class ExpenseClaimDetailsFilter : IBqlTable
		{
			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

			[PXDBInt]
			[PXUIField(DisplayName = "Employee")]
			[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXSubordinateAndWingmenSelector()]
			[PXFieldDescription]
			public virtual Int32? EmployeeID
			{
				get;
				set;
			}

			#endregion
		}
	}
}