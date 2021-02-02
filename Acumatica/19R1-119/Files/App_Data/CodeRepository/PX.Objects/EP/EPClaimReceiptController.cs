using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    public static class EPClaimReceiptController
	{
		public static void SubmitReceipt(PXCache claimsCache, PXCache receiptsCache, EPExpenseClaim claim, EPExpenseClaimDetails receipt)
		{
			receipt = receiptsCache.CreateCopy(receipt) as EPExpenseClaimDetails;
			receipt.RefNbr = claim.RefNbr;
			RefNbrUpdated(receiptsCache, claim, receipt);
			receiptsCache.Update(receipt);
		}
		public static void RemoveReceipt(PXCache cache, EPExpenseClaimDetails receipt)
		{
			receipt = cache.CreateCopy(receipt) as EPExpenseClaimDetails;
			receipt.RefNbr = null;
			RefNbrUpdated(cache, null, receipt);
			cache.Update(receipt);
		}

		public static void RefNbrUpdated(PXCache receiptsCache, EPExpenseClaim claim, EPExpenseClaimDetails receipt)
		{
			if (claim != null)
			{
				receipt.ClaimCuryInfoID = claim?.CuryInfoID;
				receipt.SubmitedDate = DateTime.Now;        
                RecalcAmountInClaimCury(receiptsCache, receipt);
			}
			else
			{
				receipt.SubmitedDate = null;
				receipt.ClaimCuryInfoID = null;

				receipt.ClaimCuryTranAmt = 0;
				receipt.ClaimCuryTranAmtWithTaxes = 0;
				receipt.ClaimCuryTaxTotal = 0;
				receipt.ClaimCuryTaxRoundDiff = 0;
				receipt.ClaimCuryVatExemptTotal = 0;
				receipt.ClaimCuryVatTaxableTotal = 0;
			}
            foreach (EPTaxTran copy in PXSelect<EPTaxTran,
                     Where<EPTaxTran.claimDetailID, Equal<Required<EPExpenseClaimDetails.claimDetailID>>>>.Select(receiptsCache.Graph, receipt.ClaimDetailID))
            {
                copy.RefNbr = receipt.RefNbr;
                receiptsCache.Graph.Caches[typeof(EPTaxTran)].Update(copy);
            }
        }

		public static void RecalcAmountInClaimCury(PXCache receiptsCache, EPExpenseClaimDetails receipt)
		{
			if (receipt != null && receipt.TranAmt != null && receipt.TranAmtWithTaxes != null && receipt.RefNbr != null)
            {
				PXGraph graph = receiptsCache.Graph;
				PXCache taxTranCache = graph.Caches[typeof(EPTaxTran)];
				CurrencyInfo expenseCuriInfo = PXSelect<CurrencyInfo,
                    Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(graph, null, receipt.CuryInfoID);

                CurrencyInfo currencyinfo = PXSelect<CurrencyInfo,
                    Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(graph, null, receipt.ClaimCuryInfoID);
                decimal curyClaim = 0m;
                decimal curyClaimWithTaxes = 0m;
                decimal curyClaimTax = 0m;
                decimal curyTaxRoundDiff = 0m;
                decimal curyVatExemptTotal = 0m;
                decimal curyVatTaxableTotal = 0m;

                if (IsSameCury(receipt.CuryInfoID, receipt.ClaimCuryInfoID, expenseCuriInfo, currencyinfo))
                {
                    curyClaim = receipt.CuryTranAmt ?? 0m;
                    curyClaimWithTaxes = receipt.CuryTranAmtWithTaxes ?? 0m;
                    curyClaimTax = receipt.CuryTaxTotal ?? 0m;
                    curyTaxRoundDiff = receipt.CuryTaxRoundDiff ?? 0m;
                    curyVatExemptTotal = receipt.CuryVatExemptTotal ?? 0m;
                    curyVatTaxableTotal = receipt.CuryVatTaxableTotal ?? 0m;
                    foreach (EPTaxTran copy in PXSelect<EPTaxTran,
                        Where<EPTaxTran.claimDetailID, Equal<Required<EPExpenseClaimDetails.claimDetailID>>>>.Select(receiptsCache.Graph, receipt.ClaimDetailID))
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
                    PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, receipt.TranAmt ?? 0m, out curyClaim);
                    PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, receipt.TranAmtWithTaxes ?? 0m, out curyClaimWithTaxes);
                    PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, receipt.TaxTotal ?? 0m, out curyClaimTax);
                    PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, receipt.TaxRoundDiff ?? 0m, out curyTaxRoundDiff);
                    PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, receipt.VatExemptTotal ?? 0m, out curyVatExemptTotal);
                    PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, receipt.VatTaxableTotal ?? 0m, out curyVatTaxableTotal);
					foreach (EPTaxTran copy in PXSelect<EPTaxTran,
						Where<EPTaxTran.claimDetailID, Equal<Required<EPExpenseClaimDetails.claimDetailID>>>>.Select(receiptsCache.Graph, receipt.ClaimDetailID))
					{
						if (taxTranCache.GetStatus(copy) != PXEntryStatus.Inserted)
						{
							taxTranCache.SetStatus(copy, PXEntryStatus.Updated);
						}
						decimal newValue;
                        PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, copy.ExpenseAmt ?? 0m, out newValue);
						taxTranCache.SetValue<EPTaxTran.claimCuryExpenseAmt>(copy, newValue);

						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, copy.TaxableAmt ?? 0m, out newValue);
						taxTranCache.SetValue<EPTaxTran.claimCuryTaxableAmt>(copy, newValue);

						PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(receiptsCache, receipt, copy.TaxAmt ?? 0m, out newValue);
						taxTranCache.SetValue<EPTaxTran.claimCuryTaxAmt>(copy, newValue);
					}
                }
                receipt.ClaimCuryTranAmt = curyClaim;
                receipt.ClaimCuryTranAmtWithTaxes = curyClaimWithTaxes;
                receipt.ClaimCuryTaxTotal = curyClaimTax;
                receipt.ClaimCuryTaxRoundDiff = curyTaxRoundDiff;
                receipt.ClaimCuryVatExemptTotal = curyVatExemptTotal;
                receipt.ClaimCuryVatTaxableTotal = curyVatTaxableTotal;
            }
        }
        public static bool IsSameCury(PXGraph graph, long? curyInfoIDA, long? curyInfoIDB)
        {
            CurrencyInfo curyInfoA = PXSelect<CurrencyInfo,
                Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(graph, null, curyInfoIDA);
            CurrencyInfo curyInfoB = PXSelect<CurrencyInfo,
               Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(graph, null, curyInfoIDB);
            return IsSameCury(curyInfoIDA, curyInfoIDB, curyInfoA, curyInfoB);
        }
        public static bool IsSameCury(long? CuryInfoIDA, long? CuryInfoIDB, CurrencyInfo curyInfoA, CurrencyInfo curyInfoB)
        {
            return CuryInfoIDA == CuryInfoIDB || curyInfoA != null && curyInfoB != null && curyInfoA.CuryID == curyInfoB.CuryID;
        }

        public static void AmtFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (e.Row != null)
			{
				RecalcAmountInClaimCury(cache, row);
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
                    Where<Location.locationID, Equal<Required<EPEmployee.defLocationID>>>>.Select(graph, employee.DefLocationID);
                taxZoneID = location?.VTaxZoneID;
            }

            return taxZoneID;
        }

		public static void CheckAllowedUser(PXGraph graph)
		{
			EPEmployee employeeByUserID = PXSelect<EPEmployee, 
				Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(graph);

			if (employeeByUserID == null && System.Web.HttpContext.Current != null && 
				!graph.IsImport && !graph.IsExport && !graph.IsCopyPasteContext && !PXGraph.ProxyIsActive)
			{
				throw new PXException(Messages.MustBeEmployee);
			}
		}
	}
}
