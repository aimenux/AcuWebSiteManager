using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.EP.DAC;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	/// <summary>
	/// Services only (and reduced handlers if needed).
	/// </summary>
	public class ExpenseClaimDetailGraphExtBase<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public virtual PXSelectBase<EPExpenseClaimDetails> Receipts { get; }

		public virtual PXSelectBase<EPExpenseClaim> Claim { get; }

		public virtual PXSelectBase<CurrencyInfo> CurrencyInfo { get; }
		public PXSetup<EPSetup> EPSetup;

		#region Repo Methods

		public virtual EPEmployeeCorpCardLink GetFirstCreditCardForEmployeeAlphabeticallySorted(int employeeID)
		{
			return (EPEmployeeCorpCardLink)PXSelectJoin<EPEmployeeCorpCardLink,
					InnerJoin<CACorpCard,
						On<CACorpCard.corpCardID, Equal<EPEmployeeCorpCardLink.corpCardID>>>,
					Where<EPEmployeeCorpCardLink.employeeID, Equal<Required<EPExpenseClaimDetails.employeeID>>,
						And<CACorpCard.isActive, Equal<True>>>,
					OrderBy<Asc<CACorpCard.name>>>
				.SelectSingleBound(this.Base, null, employeeID);
		}

		public virtual EPExpenseClaimDetails GetLastUsedCreditCardForEmployee(int employeeID)
		{
			return (EPExpenseClaimDetails)PXSelectJoin<EPExpenseClaimDetails,
					InnerJoin<EPEmployeeCorpCardLink,
						On<EPEmployeeCorpCardLink.employeeID, Equal<EPExpenseClaimDetails.employeeID>>,
						InnerJoin<CACorpCard,
							On<CACorpCard.corpCardID, Equal<EPEmployeeCorpCardLink.corpCardID>,
								And<CACorpCard.corpCardID, Equal<EPExpenseClaimDetails.corpCardID>>>>>,
					Where<CACorpCard.isActive, Equal<True>,
						And<EPEmployeeCorpCardLink.employeeID, Equal<Required<EPExpenseClaimDetails.employeeID>>>>,
					OrderBy<Desc<EPExpenseClaimDetails.lastModifiedDateTime>>>
				.SelectSingleBound(this.Base, null, employeeID);
		}

		#endregion


		protected virtual decimal? GetUnitCostByExpenseItem(PXCache cache, EPExpenseClaimDetails receipt)
		{
			var item = PXSelectorAttribute.Select<InventoryItem.inventoryID>(cache, receipt) as InventoryItem;
			decimal curyStdCost;
			if (item != null && CurrencyInfo.Current != null && CurrencyInfo.Current.CuryRate != null)
			{
				var cost = receipt.ExpenseDate >= item.StdCostDate ? item.StdCost.GetValueOrDefault() : item.LastStdCost.GetValueOrDefault();
				PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.curyInfoID>(cache, receipt, cost, out curyStdCost, true);
			}
			else
				curyStdCost = 0m;

			return curyStdCost;
		}
		public virtual void ClearFieldsIfNeeded(PXCache cache, EPExpenseClaimDetails row)
		{
			var corpCardIsEnabled = row.PaidWith != EPExpenseClaimDetails.paidWith.PersonalAccount;
			if (!corpCardIsEnabled)
			{
				cache.SetValueExt<EPExpenseClaimDetails.corpCardID>(row, null);
			}

			var taxIsEnabled = row.PaidWith != EPExpenseClaimDetails.paidWith.CardPersonalExpense;
			if (!taxIsEnabled)
			{
				cache.SetValueExt<EPExpenseClaimDetails.taxZoneID>(row, null);
				cache.SetValueExt<EPExpenseClaimDetails.taxCategoryID>(row, null);
			}

			if (row.PaidWith == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
			{
				PMProject prj = PXSelect<PMProject, Where<PMProject.nonProject, Equal<True>>>.SelectSingleBound(Base, null, null);

				cache.SetValueExt<EPExpenseClaimDetails.contractID>(row, prj.ContractCD);
				cache.SetValueExt<EPExpenseClaimDetails.taskID>(row, null);
				cache.SetValueExt<EPExpenseClaimDetails.costCodeID>(row, null);
				cache.SetValueExt<EPExpenseClaimDetails.curyTipAmt>(row, 0);
			}
		}

		public virtual void DefaultCardCurrencyInfo(PXCache cache, EPExpenseClaimDetails document)
		{
			if (document.CardCuryInfoID != null)
			{
				CurrencyInfo cardCuryInfo = CurrencyInfoAttribute.SetDefaults<EPExpenseClaimDetails.cardCuryInfoID>(cache, document);

				if (cardCuryInfo != null)
				{
					document.CardCuryID = cardCuryInfo.CuryID;
				}
			}
		}

		public virtual void SetCardCurrencyData(PXCache cache, EPExpenseClaimDetails document, int? corpCardID)
		{
			if (corpCardID != null)
			{
				CashAccount cashAccount = CACorpCardsMaint.GetCardCashAccount(Base, corpCardID);

				cache.SetValueExt<EPExpenseClaimDetails.cardCuryID>(document, cashAccount.CuryID);
			}
			else
			{
				DefaultCardCurrencyInfo(cache, document);
			}
		}

		public virtual void SetClaimCuryWhenNotInClaim(EPExpenseClaimDetails document, string claimRefNbr, int? corpCardID)
		{
			if (claimRefNbr == null)
			{
				document.ClaimCuryInfoID = corpCardID != null ? document.CardCuryInfoID : null;
			}
		}

		//TODO: remove in 2018R2
		internal static void DeleteLegacyTaxRows(PXGraph graph, string refNbr)
		{
			if (!String.IsNullOrEmpty(refNbr))
			{
				PXCache claimCache = graph.Caches[typeof(EPExpenseClaim)];
				PXCache taxTranCache = graph.Caches[typeof(EPTaxTran)];
				EPExpenseClaim claim = PXSelect<EPExpenseClaim,
					Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(graph, refNbr);
				bool taxTransDeleted = false;
				foreach (EPTaxAggregate taxTran in PXSelect<EPTaxAggregate,
					Where<EPTaxAggregate.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(graph, refNbr))
				{
					claim.TaxTotal -= taxTran.TaxAmt;
					claim.CuryTaxTotal -= taxTran.CuryTaxAmt;
					taxTransDeleted = true;
					graph.Caches[typeof(EPTaxAggregate)].Delete(taxTran);
				}
				if (taxTransDeleted)
				{
					claim.VatTaxableTotal = 0;
					claim.CuryVatTaxableTotal = 0;
					claim.CuryVatExemptTotal = 0;
					claim.VatExemptTotal = 0;
					claimCache.Update(claim);
				}

			}
		}

		public static string GetTaxZoneID(PXGraph graph, EPEmployee employee)
		{
			string taxZoneID = employee?.ReceiptAndClaimTaxZoneID;
			if (string.IsNullOrEmpty(taxZoneID))
			{
				Location location = PXSelect<Location,
					Where<Location.locationID, Equal<Required<EPEmployee.defLocationID>>>>.Select(graph, employee?.DefLocationID);
				taxZoneID = location?.VTaxZoneID;
			}

			return taxZoneID;
		}

		public static void CheckAllowedUser(PXGraph graph)
		{
			EPEmployee employeeByUserID = PXSelect<EPEmployee, 
				Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(graph);

            if (employeeByUserID == null && !PXGraph.ProxyIsActive)
            {
                if (graph.IsExport || graph.IsImport)
                {
                    throw new PXException(Messages.MustBeEmployee);
                }
                else
                {
                    Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
                }
            }
        }
		
		public virtual void SubmitReceiptExt(PXCache claimsCache, PXCache receiptsCache, EPExpenseClaim claim, EPExpenseClaimDetails receipt)
		{
			receipt = receiptsCache.CreateCopy(receipt) as EPExpenseClaimDetails;
			string oldRefNbr = receipt.RefNbr;
			receipt.RefNbr = claim.RefNbr;
			RefNbrUpdated(receiptsCache, claim, receipt, oldRefNbr);
			receiptsCache.Update(receipt);
		}

		public virtual void RemoveReceipt(PXCache cache, EPExpenseClaimDetails receipt, bool skipReceiptCacheUpdate = false)
		{
			receipt = cache.CreateCopy(receipt) as EPExpenseClaimDetails;
			string oldRefNbr = receipt.RefNbr;
			receipt.RefNbr = null;

			RefNbrUpdated(cache, null, receipt, oldRefNbr);

			if (!skipReceiptCacheUpdate)
			{
				cache.Update(receipt);
			}
		}

		public virtual void RefNbrUpdated(PXCache receiptsCache, EPExpenseClaim claim, EPExpenseClaimDetails receipt, string oldRefNbr)
		{
			SetClaimCuryWhenNotInClaim(receipt, claim?.RefNbr, receipt.CorpCardID);

			if (claim != null)
			{
				receipt.ClaimCuryInfoID = claim?.CuryInfoID;
				receipt.SubmitedDate = DateTime.Now;

				RecalcAmountInClaimCury(receipt);

				if (receipt.IsPaidWithCard && receipt.CorpCardID != null)
				{
					SumClaimValues(claim, receipt, null);
				}
			}
			else
			{
				EPExpenseClaimDetails oldReceipt = (EPExpenseClaimDetails)receiptsCache.CreateCopy(receipt);

				receipt.SubmitedDate = null;

				RecalcAmountInClaimCury(receipt);

				if (oldRefNbr != null)
				{
					EPExpenseClaim oldClaim = GetParentClaim(oldRefNbr);

					if (oldClaim != null)
					{
						SumClaimValues(oldClaim, null, oldReceipt);
					}
				}
			}
			foreach (EPTaxTran copy in PXSelect<EPTaxTran,
					 Where<EPTaxTran.claimDetailID, Equal<Required<EPExpenseClaimDetails.claimDetailID>>>>.Select(receiptsCache.Graph, receipt.ClaimDetailID))
			{
				copy.RefNbr = receipt.RefNbr;
				receiptsCache.Graph.Caches[typeof(EPTaxTran)].Update(copy);
			}
		}

		public virtual void RecalcAmountInClaimCury(EPExpenseClaimDetails receipt)
		{
			if (receipt != null && receipt.TranAmt != null && receipt.TranAmtWithTaxes != null)
			{
				var oldReceipt = RecalcAmountInClaimCuryForReceipt(receipt);
				if (Claim != null && receipt.RefNbr != null)
				{
					EPExpenseClaim claim = GetParentClaim(receipt.RefNbr);

					SumClaimValues(claim, receipt, oldReceipt);
				}
			}
		}

		public virtual EPExpenseClaimDetails RecalcAmountInClaimCuryForReceipt(EPExpenseClaimDetails receipt)
		{
			EPExpenseClaimDetails oldReceipt = null;
			if (receipt != null && receipt.TranAmt != null && receipt.TranAmtWithTaxes != null)
			{
				decimal curyClaim = 0m;
				decimal curyClaimWithTaxes = 0m;
				decimal curyClaimTax = 0m;
				decimal curyTaxRoundDiff = 0m;
				decimal curyVatExemptTotal = 0m;
				decimal curyVatTaxableTotal = 0m;

				if (!receipt.IsPaidWithCard || receipt.IsPaidWithCard && receipt.CorpCardID != null)
				{
					PXCache taxTranCache = Base.Caches[typeof(EPTaxTran)];
					CurrencyInfo expenseCuriInfo = PXSelect<CurrencyInfo,
						Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(Base, null, receipt.CuryInfoID);

					CurrencyInfo currencyinfo = PXSelect<CurrencyInfo,
						Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(Base, null, receipt.ClaimCuryInfoID);

					if (CurrencyHelper.IsSameCury(receipt.CuryInfoID, receipt.ClaimCuryInfoID, expenseCuriInfo, currencyinfo))
					{
						curyClaim = receipt.CuryTranAmt ?? 0m;
						curyClaimWithTaxes = receipt.CuryTranAmtWithTaxes ?? 0m;
						curyClaimTax = receipt.CuryTaxTotal ?? 0m;
						curyTaxRoundDiff = receipt.CuryTaxRoundDiff ?? 0m;
						curyVatExemptTotal = receipt.CuryVatExemptTotal ?? 0m;
						curyVatTaxableTotal = receipt.CuryVatTaxableTotal ?? 0m;
						foreach (EPTaxTran copy in PXSelect<EPTaxTran,
							Where<EPTaxTran.claimDetailID, Equal<Required<EPExpenseClaimDetails.claimDetailID>>>>.Select(Base, receipt.ClaimDetailID))
						{
							if (taxTranCache.GetStatus(copy) != PXEntryStatus.Inserted)
							{
								taxTranCache.SetStatus(copy, PXEntryStatus.Updated);
							}
							taxTranCache.SetValue<EPTaxTran.claimCuryExpenseAmt>(copy, copy.CuryExpenseAmt ?? 0m);
							taxTranCache.SetValue<EPTaxTran.claimCuryTaxableAmt>(copy, copy.CuryTaxableAmt ?? 0m);
							taxTranCache.SetValue<EPTaxTran.claimCuryTaxAmt>(copy, copy.CuryTaxAmt ?? 0m);

						}
					}
					else if (currencyinfo?.CuryRate != null)
					{
						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, receipt.TranAmt ?? 0m, out curyClaim);
						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, receipt.TranAmtWithTaxes ?? 0m, out curyClaimWithTaxes);
						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, receipt.TaxTotal ?? 0m, out curyClaimTax);
						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, receipt.TaxRoundDiff ?? 0m, out curyTaxRoundDiff);
						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, receipt.VatExemptTotal ?? 0m, out curyVatExemptTotal);
						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, receipt.VatTaxableTotal ?? 0m, out curyVatTaxableTotal);
						foreach (EPTaxTran copy in PXSelect<EPTaxTran,
							Where<EPTaxTran.claimDetailID, Equal<Required<EPExpenseClaimDetails.claimDetailID>>>>.Select(Base, receipt.ClaimDetailID))
						{
							if (taxTranCache.GetStatus(copy) != PXEntryStatus.Inserted)
							{
								taxTranCache.SetStatus(copy, PXEntryStatus.Updated);
							}
							decimal newValue;
							PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, copy.ExpenseAmt ?? 0m, out newValue);
							taxTranCache.SetValue<EPTaxTran.claimCuryExpenseAmt>(copy, newValue);

							PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, copy.TaxableAmt ?? 0m, out newValue);
							taxTranCache.SetValue<EPTaxTran.claimCuryTaxableAmt>(copy, newValue);

							PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(Receipts.Cache, receipt, copy.TaxAmt ?? 0m, out newValue);
							taxTranCache.SetValue<EPTaxTran.claimCuryTaxAmt>(copy, newValue);
						}
					}
				}

				oldReceipt = (EPExpenseClaimDetails)Receipts.Cache.CreateCopy(receipt);

				receipt.ClaimCuryTranAmt = curyClaim;
				receipt.ClaimCuryTranAmtWithTaxes = curyClaimWithTaxes;
				receipt.ClaimCuryTaxTotal = curyClaimTax;
				receipt.ClaimCuryTaxRoundDiff = curyTaxRoundDiff;
				receipt.ClaimCuryVatExemptTotal = curyVatExemptTotal;
				receipt.ClaimCuryVatTaxableTotal = curyVatTaxableTotal;
			}
			return oldReceipt;
		}

		public virtual void SumClaimValues(EPExpenseClaim claim, EPExpenseClaimDetails receipt, EPExpenseClaimDetails oldReceipt)
		{
			EPExpenseClaim newClaim = (EPExpenseClaim)Claim.Cache.CreateCopy(claim);

			SumCalc sumCalculator = new SumCalc();

			newClaim.CuryDocBal += (decimal?)sumCalculator.Calculate<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes>(Receipts.Cache, receipt, oldReceipt);
			newClaim.CuryTaxTotal += (decimal?)sumCalculator.Calculate<EPExpenseClaimDetails.claimCuryTaxTotal>(Receipts.Cache, receipt, oldReceipt);
			newClaim.CuryTaxRoundDiff += (decimal?)sumCalculator.Calculate<EPExpenseClaimDetails.claimCuryTaxRoundDiff>(Receipts.Cache, receipt, oldReceipt);
			newClaim.CuryVatExemptTotal += (decimal?)sumCalculator.Calculate<EPExpenseClaimDetails.claimCuryVatExemptTotal>(Receipts.Cache, receipt, oldReceipt);
			newClaim.CuryVatTaxableTotal += (decimal?)sumCalculator.Calculate<EPExpenseClaimDetails.claimCuryVatTaxableTotal>(Receipts.Cache, receipt, oldReceipt);

			Claim.Update(newClaim);
		}

		public virtual void AmtFieldUpdated(PXCache cache, Events.RowUpdated<EPExpenseClaimDetails> e)
		{
			if (e.Row != null)
			{
				RecalcAmountInClaimCury(e.Row);
			}
		}
		/// <summary>
		/// Method build sales or expense sub account by mask for receipt 
		/// </summary>
		/// <typeparam name="SubMaskField"><see=EPSetup.salesSubMask/> or <see=EPSetup.expenseSubMask/></typeparam>
		/// <typeparam name="CompanySubField">Field of receipt with company sub account (<see=Location.cMPSalesSubID/> or <see=Location.cMPExpenseSubID/>)</typeparam>
		/// <typeparam name="EmployeeSubField">Field of receipt with employee sub account (<see=EPEmployee.salesSubID/> or <see=EPEmployee.expenseSubID/>)</typeparam>
		/// <typeparam name="ItemSubField">Field of receipt with inventory item sub account (<see=InventoryItem.salesSubID/> or <see=InventoryItem.cOGSSubID/>)</typeparam>
		/// <param name="receiptCache">Cache of receipt</param>
		/// <param name="receipt">Receipt for which build sub account</param>
		/// <param name="subMask">Mask from <see=EPSetup/></param>
		/// <returns>Sub account by mask</returns>
		public static object MakeSubAccountByMaskForReceipt<SubMaskField, CompanySubField, EmployeeSubField, ItemSubField>(PXCache receiptCache, EPExpenseClaimDetails receipt, string subMask)
			where SubMaskField : IBqlField
			where CompanySubField : IBqlField
			where EmployeeSubField : IBqlField
			where ItemSubField : IBqlField
		{
			object value = null;
			InventoryItem item = (InventoryItem)PXSelect<InventoryItem,
													Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(receiptCache.Graph,
																																		receipt.InventoryID);
			Location companyloc = (Location)PXSelectJoin<Location,
												InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>,
																	And<Location.locationID, Equal<BAccountR.defLocationID>>>,
												InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>,
											Where<GL.Branch.branchID, Equal<Current<EPExpenseClaimDetails.branchID>>>>.Select(receiptCache.Graph,
																															receipt);
			Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(receiptCache.Graph,
																															receipt.ContractID);
			PMTask task = PXSelect<PMTask,
							Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
							And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(receiptCache.Graph,
																						receipt.ContractID,
																						receipt.TaskID);
			Location customerLocation = (Location)PXSelectorAttribute.Select<EPExpenseClaimDetails.customerLocationID>(receiptCache,
																														receipt);
			EPEmployee employee = (EPEmployee)PXSelect<EPEmployee>.Search<EPEmployee.bAccountID>(receiptCache.Graph,
																								receipt != null ? receipt.EmployeeID : null);

			int? employee_SubID = (int?)receiptCache.Graph.Caches[typeof(EPEmployee)].GetValue<EmployeeSubField>(employee);
			int? item_SubID = (int?)receiptCache.Graph.Caches[typeof(InventoryItem)].GetValue<ItemSubField>(item);
			int? company_SubID = (int?)receiptCache.Graph.Caches[typeof(Location)].GetValue<CompanySubField>(companyloc);
			int? project_SubID = (int?)receiptCache.Graph.Caches[typeof(Contract)].GetValue<Contract.defaultSubID>(contract);
			int? task_SubID = (int?)receiptCache.Graph.Caches[typeof(PMTask)].GetValue<PMTask.defaultSubID>(task);
			int? location_SubID = (int?)receiptCache.Graph.Caches[typeof(Location)].GetValue<Location.cSalesSubID>(customerLocation);

			value = SubAccountMaskAttribute.MakeSub<SubMaskField>(receiptCache.Graph, subMask,
				new object[] { employee_SubID, item_SubID, company_SubID, project_SubID, task_SubID, location_SubID },
				new Type[] { typeof(EmployeeSubField), typeof(ItemSubField), typeof(CompanySubField), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(Location.cSalesSubID) });

			return value;
		}
		public static void SalesSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, string salesSubMask)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (row.SalesAccountID != null)
			{
				object value = null;
				if (row.Billable == true)
				{
					value = MakeSubAccountByMaskForReceipt<EPSetup.salesSubMask,
																				Location.cMPSalesSubID,
																				EPEmployee.salesSubID,
																				InventoryItem.salesSubID>(sender, row, salesSubMask);
				}

				sender.RaiseFieldUpdating<EPExpenseClaimDetails.salesSubID>(row, ref value);

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		public static void ExpenseSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, string expenseSubMask)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (row.ExpenseAccountID != null)
			{
				object value = MakeSubAccountByMaskForReceipt<EPSetup.expenseSubMask,
																					Location.cMPExpenseSubID,
																					EPEmployee.expenseSubID,
																					InventoryItem.cOGSSubID>(sender, row, expenseSubMask);
				sender.RaiseFieldUpdating<EPExpenseClaimDetails.expenseSubID>(row, ref value);

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		public EPExpenseClaim GetParentClaim(string claimRefNbr)
		{
			EPExpenseClaimDetails receiptStub = new EPExpenseClaimDetails() { RefNbr = claimRefNbr };

			return PXParentAttribute.SelectParent<EPExpenseClaim>(Receipts.Cache, receiptStub);
		}

		public virtual void VerifyClaimAndCorpCardCurrencies(int? corpCardID, EPExpenseClaim claim, Action substituteNewValue = null)
		{
			if (corpCardID != null && claim != null)
			{
				CashAccount cashAccount = CACorpCardsMaint.GetCardCashAccount(Base, corpCardID);

				if (cashAccount.CuryID != claim.CuryID)
				{
					substituteNewValue?.Invoke();

					throw new PXSetPropertyException(Messages.CardCurrencyAndTheClaimCurrencyMustBeEqual);
				}
			}
		}

		public virtual void VerifyEmployeeAndClaimCurrenciesForCash(
			EPExpenseClaimDetails receipt,
			string receiptPaidWith,
			EPExpenseClaim claim,
			Action substituteNewValue = null)
		{
			if (claim == null)
				return;

			if (receiptPaidWith == EPExpenseClaimDetails.paidWith.PersonalAccount)
			{
				EPEmployee employee =
					PXSelect<EPEmployee,
							Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
						.Select(Base, receipt.EmployeeID);

				if (employee.AllowOverrideCury != true && employee.CuryID != null && employee.CuryID != claim.CuryID)
				{
					substituteNewValue?.Invoke();

					throw new PXSetPropertyException(Messages.ReceiptCannotBeAddedToClaimBecauseClaimCuryDiffersFromEmployeeCury);
				}
			}
		}

		public virtual void VerifyIsPositiveForCorpCardReceipt(string paidWith, decimal? amount)
		{
			if ((paidWith == EPExpenseClaimDetails.paidWith.CardCompanyExpense
				 || paidWith == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
				&& amount != null && amount < 0)
			{
				throw new PXSetPropertyException(Messages.AmountOfTheExpenseReceiptToBePaidWithTheCorporateCardCannotBeNegative);
			}
		}

		public virtual void VerifyEmployeePartIsZeroForCorpCardReceipt(string paidWith, decimal? amount)
		{
			if ((paidWith == EPExpenseClaimDetails.paidWith.CardCompanyExpense
				 || paidWith == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
				&& amount != null && amount != 0)
			{
				throw new PXSetPropertyException(Messages.EmployeePartMustBeZero);
			}
		}

		public virtual void VerifyExpenseRefNbrIsNotEmpty(EPExpenseClaimDetails receipt)
		{
			EPSetup setup = EPSetup.Select();

			if (setup.RequireRefNbrInExpenseReceipts == true)

			if (string.IsNullOrEmpty(receipt.ExpenseRefNbr))
			{
				var fieldName = typeof(EPExpenseClaimDetails.expenseRefNbr).Name;
				throw new PXRowPersistingException(fieldName, receipt.ExpenseRefNbr, ErrorMessages.FieldIsEmpty, fieldName);
			}
		}
	}

}
