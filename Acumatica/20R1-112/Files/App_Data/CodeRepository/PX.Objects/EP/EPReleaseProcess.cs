using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Objects.TX;
using static PX.Objects.AP.APReleaseProcess;

namespace PX.Objects.EP
{
	public class EPReleaseProcess : PXGraph<EPReleaseProcess>
	{
	    public PXSelect<CABankTran> CABankTrans;

	    public PXSelect<CABankTranMatch> CABankTranMatches;

		[InjectDependency]
        public IFinPeriodUtils FinPeriodUtils { get; set; }

		public virtual void ReleaseDocProc(EPExpenseClaim claim)
		{
			ExpenseClaimEntry expenseClaimGraph = PXGraph.CreateInstance<ExpenseClaimEntry>();

			EPExpenseClaim checkClaim = PXSelectReadonly<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(expenseClaimGraph, claim.RefNbr);

			if (checkClaim.Released == true)
			{
				throw new PXException(Messages.AlreadyReleased);
			}

            var receipts = PXSelect<EPExpenseClaimDetails,
						Where<EPExpenseClaimDetails.refNbr, Equal<Required<EPExpenseClaim.refNbr>>,
                                    And<EPExpenseClaimDetails.released, Equal<False>>>>
                                .Select(expenseClaimGraph, claim.RefNbr)
                                .RowCast<EPExpenseClaimDetails>()
                                .ToArray();

			IFinPeriodUtils finPeriodUtils = expenseClaimGraph.GetService<IFinPeriodUtils>();

			if (claim.FinPeriodID != null)
			{
                finPeriodUtils.ValidateFinPeriod<EPExpenseClaimDetails>(receipts, m => claim.FinPeriodID, m => m.BranchID.SingleToArray());
			}

	        List<APRegister> apDocs = new List<APRegister>();

			using (var ts = new PXTransactionScope())
			{
				if (receipts.Any())
		        {
			        var receiptsByPaidWithType = receipts.GroupBy(receipt => receipt.PaidWith);

	                foreach (var receiptGroup in receiptsByPaidWithType)
	                {
		                List<APRegister> res = null;

						if (receiptGroup.Key == EPExpenseClaimDetails.paidWith.PersonalAccount)
	                    {
							res = ReleaseClaimDetails<
								Invoice, InvoiceMapping, APInvoiceEntry, APInvoiceEntry.APInvoiceEntryDocumentExtension>(
								expenseClaimGraph, claim, receiptGroup,receiptGroup.Key);
			}
	                    else if (receiptGroup.Key == EPExpenseClaimDetails.paidWith.CardCompanyExpense)
	                    {
		                    res = ReleaseClaimDetails<
			                    PaidInvoice, PaidInvoiceMapping, APQuickCheckEntry, APQuickCheckEntry.APQuickCheckEntryDocumentExtension>(
			                    expenseClaimGraph, claim, receiptGroup, receiptGroup.Key);
	                    }
	                    else if (receiptGroup.Key == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
	                    {
		                    res = ReleaseClaimDetails<
			                    Invoice, InvoiceMapping, APInvoiceEntry, APInvoiceEntry.APInvoiceEntryDocumentExtension>(
			                    expenseClaimGraph, claim, receiptGroup, receiptGroup.Key);
	                    }
	                    else
	                    {
	                        throw new NotImplementedException();
	                    }

		                apDocs.AddRange(res);
					}
				}
		        else
		        {
			        apDocs = ReleaseClaimDetails<Invoice, InvoiceMapping, APInvoiceEntry, APInvoiceEntry.APInvoiceEntryDocumentExtension>(
				        expenseClaimGraph, claim, new EPExpenseClaimDetails[0], EPExpenseClaimDetails.paidWith.PersonalAccount);
				}

		        ts.Complete();
	        }

	        EPSetup epsetup = PXSelectReadonly<EPSetup>.Select(this);

			if (epsetup.AutomaticReleaseAP == true)
	        {
		        APDocumentRelease.ReleaseDoc(apDocs, false);
	        }
		}

        public virtual List<APRegister> ReleaseClaimDetails
            <TAPDocument, TInvoiceMapping, TGraph, TAPDocumentGraphExtension>
            (ExpenseClaimEntry expenseClaimGraph, EPExpenseClaim claim, IEnumerable<EPExpenseClaimDetails> receipts, string receiptGroupPaidWithType)
            where TGraph : PXGraph, new()
            where TAPDocument : InvoiceBase, new()
            where TInvoiceMapping : IBqlMapping
            where TAPDocumentGraphExtension : PX.Objects.Common.GraphExtensions.Abstract.InvoiceBaseGraphExtension<TGraph, TAPDocument, TInvoiceMapping>
        {
			#region prepare required variable

			var docgraph = PXGraph.CreateInstance<TGraph>();

	        EPSetup epsetup = PXSelectReadonly<EPSetup>.Select(docgraph);

			TAPDocumentGraphExtension apDocumentGraphExtension = docgraph.FindImplementation<TAPDocumentGraphExtension>();

            List<List<EPExpenseClaimDetails>> receiptsForDocument = new List<List<EPExpenseClaimDetails>>();

            if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.PersonalAccount)
            {
                receiptsForDocument = receipts.GroupBy(item => new {item.TaxZoneID, item.TaxCalcMode})
                                                .Select(group => group.ToList())
                                                .ToList();
            }
            else if(receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardCompanyExpense)
            {
	            if (epsetup.PostSummarizedCorpCardExpenseReceipts == true)
	            {
		            receiptsForDocument = receipts.GroupBy(item =>
				            new
				            {
					            item.TaxZoneID,
					            item.TaxCalcMode,
								item.CorpCardID,
								item.ExpenseDate,
								item.ExpenseRefNbr
				            })
			            .Select(group => group.ToList())
			            .ToList();
				}
	            else
	            {
		            receiptsForDocument = receipts.Select(receipt => receipt.SingleToList()).ToList();
				}
            }
            else if(receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
            {
                receiptsForDocument = new List<List<EPExpenseClaimDetails>>() {receipts.ToList()};
            }
            else
            {
                throw new NotImplementedException();
            }

	        if (!receiptsForDocument.Any())
	        {
		        receiptsForDocument.Add(new List<EPExpenseClaimDetails>());
			}

            APSetup apsetup = PXSelectReadonly<APSetup>.Select(docgraph);
			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPExpenseClaim.employeeID>>>>.Select(docgraph, claim.EmployeeID);
			Location emplocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<EPExpenseClaim.employeeID>>, And<Location.locationID, Equal<Required<EPExpenseClaim.locationID>>>>>.Select(docgraph, claim.EmployeeID, claim.LocationID);

			List<APRegister> doclist = new List<APRegister>();
			expenseClaimGraph.SelectTimeStamp();


			if (claim.FinPeriodID != null)
			{
                FinPeriodUtils.ValidateFinPeriod(claim.SingleToArray());
			}
			#endregion

	        foreach (var receiptGroup in receiptsForDocument)
				{
	            if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardCompanyExpense
	                && receiptGroup.Count > 1)
	            {
		            EPExpenseClaimDetails[] matchedReceipts = receiptGroup.Where(receipt => receipt.BankTranDate != null).Take(11).ToArray();

		            if (matchedReceipts.Any())
		            {
						PXResult<EPExpenseClaimDetails, CABankTranMatch, CABankTran>[] rows =
				            PXSelectJoin<EPExpenseClaimDetails,
						            InnerJoin<CABankTranMatch,
							            On<CABankTranMatch.docModule, Equal<BatchModule.moduleEP>,
								            And<CABankTranMatch.docType, Equal<EPExpenseClaimDetails.docType>,
									        And<CABankTranMatch.docRefNbr, Equal<EPExpenseClaimDetails.claimDetailCD>>>>,
									InnerJoin<CABankTran,
										On<CABankTran.tranID, Equal<CABankTranMatch.tranID>>>>,
						            Where<EPExpenseClaimDetails.claimDetailCD, In<Required<EPExpenseClaimDetails.claimDetailCD>>>>
					            .Select(expenseClaimGraph, matchedReceipts.Select(receipt => receipt.ClaimDetailCD).ToArray())
					            .Cast<PXResult<EPExpenseClaimDetails, CABankTranMatch, CABankTran>>()
								.ToArray();

						throw new PXException(Messages.ExpenseReceiptCannotBeSummarized,
							rows.Select(row => String.Concat(PXMessages.LocalizeNoPrefix(Messages.Receipt), 
															" ",
															((EPExpenseClaimDetails)row).ClaimDetailCD, 
															" - " ,
															((CABankTran)row).GetFriendlyKeyImage(Caches[typeof(CABankTran)])))
								.ToArray()
								.JoinIntoStringForMessageNoQuotes(maxCount: 10));
		            }
	            }

					docgraph.Clear(PXClearOption.ClearAll);
					docgraph.SelectTimeStamp();
                apDocumentGraphExtension.Contragent.Current = apDocumentGraphExtension.Contragent.Cache.GetExtension<Contragent>(employee);
                apDocumentGraphExtension.Location.Current = emplocation;

					CurrencyInfo infoOriginal = PXSelect<CurrencyInfo,
						Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaim.curyInfoID>>>>.Select(docgraph, claim.CuryInfoID);
					CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(infoOriginal);

					info.CuryInfoID = null;
                info = apDocumentGraphExtension.CurrencyInfo.Insert(info);

					#region CreateInvoiceHeader

                var invoice = new TAPDocument();

	            CABankTranMatch bankTranMatch = null;

				if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.PersonalAccount)
					{
                    invoice.DocType = receiptGroup.Sum(_ => _.ClaimCuryTranAmtWithTaxes) < 0
                        ? APInvoiceType.DebitAdj
                        : APInvoiceType.Invoice;
					}
                else if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardCompanyExpense)
					{
					EPExpenseClaimDetails receipt = receiptGroup.First();

					invoice.DocType = APDocType.QuickCheck;

                    CACorpCard card = CACorpCard.PKID.Find(this, receipt.CorpCardID);

                    PaymentMethodAccount paymentMethodAccount = PXSelect<PaymentMethodAccount,
                                                                            Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>>>
                                                                            .Select(this, card.CashAccountID);

                    invoice.CashAccountID = card.CashAccountID;
                    invoice.PaymentMethodID = paymentMethodAccount.PaymentMethodID;
					invoice.ExtRefNbr = receipt.ExpenseRefNbr;

					if (receiptGroup.Count == 1)
					{
						bankTranMatch =
							PXSelect<CABankTranMatch,
									Where<CABankTranMatch.docModule, Equal<BatchModule.moduleEP>,
										And<CABankTranMatch.docType, Equal<EPExpenseClaimDetails.docType>,
										And<CABankTranMatch.docRefNbr, Equal<Required<CABankTranMatch.docRefNbr>>>>>>
									.Select(expenseClaimGraph, receipt.ClaimDetailCD);

						if (bankTranMatch != null)
						{
							CABankTran bankTran = CABankTran.PK.Find(expenseClaimGraph, bankTranMatch.TranID);

							invoice.ClearDate = bankTran.ClearDate;
							invoice.Cleared = true;
						}
					}
				}
                else if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
					{
                    invoice.DocType = APDocType.DebitAdj;
					}
					else
					{
                    throw new NotImplementedException();
					}

					invoice.CuryInfoID = info.CuryInfoID;

					invoice.Hold = true;
					invoice.Released = false;
					invoice.Printed = invoice.DocType == APDocType.QuickCheck;
					invoice.OpenDoc = true;

                invoice.HeaderDocDate = claim.DocDate;
						invoice.FinPeriodID = claim.FinPeriodID;
					invoice.InvoiceNbr = claim.RefNbr;
					invoice.DocDesc = claim.DocDesc;
                invoice.ContragentID = claim.EmployeeID;
					invoice.CuryID = info.CuryID;
                invoice.ContragentLocationID = claim.LocationID;
                invoice.ModuleAccountID = emplocation != null ? emplocation.APAccountID : null;
                invoice.ModuleSubID = emplocation != null ? emplocation.APSubID : null;
                invoice.TaxCalcMode = receiptGroup.Any() ? receiptGroup.First().TaxCalcMode: claim.TaxCalcMode;
					invoice.BranchID = claim.BranchID;
					invoice.OrigModule = BatchModule.EP;

	            if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardCompanyExpense
	                && receiptGroup.Count == 1)
	            {
		            invoice.OrigDocType = EPExpenseClaimDetails.DocType;
		            invoice.OrigRefNbr = receiptGroup.Single().ClaimDetailCD;
				}
	            else
	            {
		            invoice.OrigDocType = EPExpenseClaim.DocType;
					invoice.OrigRefNbr = claim.RefNbr;
				}              

                bool reversedDocument = invoice.DocType == APInvoiceType.DebitAdj && receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.PersonalAccount;

                decimal signOperation = reversedDocument ? -1 : 1;

                invoice = apDocumentGraphExtension.Documents.Insert(invoice);
	            (apDocumentGraphExtension.Documents.Cache as PXModelExtension<TAPDocument>)?.UpdateExtensionMapping(invoice, MappingSyncDirection.BaseToExtension);

				invoice.TaxZoneID = receiptGroup.Any() ? receiptGroup.First().TaxZoneID : claim.TaxZoneID;

                invoice = apDocumentGraphExtension.Documents.Update(invoice);

					PXCache<CurrencyInfo>.RestoreCopy(info, infoOriginal);
					info.CuryInfoID = invoice.CuryInfoID;

					PXCache claimcache = docgraph.Caches[typeof(EPExpenseClaim)];
					PXCache claimdetailcache = docgraph.Caches[typeof(EPExpenseClaimDetails)];

                PXNoteAttribute.CopyNoteAndFiles(claimcache, claim, apDocumentGraphExtension.Documents.Cache, invoice, epsetup.GetCopyNoteSettings<PXModule.ap>());
					#endregion

                TaxAttribute.SetTaxCalc<InvoiceTran.taxCategoryID>(apDocumentGraphExtension.InvoiceTrans.Cache, null, TaxCalc.ManualCalc);

					decimal? claimCuryTaxRoundDiff = 0m;
					decimal? claimTaxRoundDiff = 0m;
                foreach (EPExpenseClaimDetails claimdetail in receiptGroup)
					{
						#region AddDetails

						decimal tipQty;
						if (reversedDocument == claimdetail.ClaimCuryTranAmtWithTaxes < 0)
						{
							tipQty = 1;
						}
						else
						{
							tipQty = -1;
						}
						Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<EPExpenseClaimDetails.contractID>>>>.SelectSingleBound(docgraph, null, claimdetail.ContractID);

						if (claimdetail.TaskID != null)
						{
							PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(expenseClaimGraph, claimdetail.TaskID);
							if (task != null && !(bool)task.VisibleInAP)
								throw new PXException(PM.Messages.TaskInvisibleInModule, task.TaskCD, BatchModule.AP);
						}

                    InvoiceTran tran = new InvoiceTran();
						tran.InventoryID = claimdetail.InventoryID;
						tran.TranDesc = claimdetail.TranDesc;
						decimal unitCost;
						decimal amount;
						decimal taxableAmt;
						decimal taxAmt;
                    if (CurrencyHelper.IsSameCury(expenseClaimGraph, claimdetail.CuryInfoID, claimdetail.ClaimCuryInfoID))
						{
							unitCost = claimdetail.CuryUnitCost ?? 0m;
							amount = claimdetail.CuryTaxableAmt ?? 0m;
							taxableAmt = claimdetail.CuryTaxableAmtFromTax ?? 0m;
							taxAmt = claimdetail.CuryTaxAmt ?? 0m;
						}
						else
						{

							if (claimdetail.CuryUnitCost == null || claimdetail.CuryUnitCost == 0m)
							{
								unitCost = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.UnitCost, out unitCost);
							}
							if (claimdetail.CuryTaxableAmt == null || claimdetail.CuryTaxableAmt == 0m)
							{
								amount = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TaxableAmt, out amount);
							}
							if (claimdetail.CuryTaxableAmtFromTax == null || claimdetail.CuryTaxableAmtFromTax == 0m)
							{
								taxableAmt = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TaxableAmtFromTax, out taxableAmt);
							}
							if (claimdetail.CuryTaxAmt == null || claimdetail.CuryTaxAmt == 0m)
							{
								taxAmt = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TaxAmt, out taxAmt);
							}
						}

						tran.ManualPrice = true;
						tran.CuryUnitCost = unitCost;
						tran.Qty = claimdetail.Qty * signOperation;
						tran.UOM = claimdetail.UOM;
						tran.NonBillable = claimdetail.Billable != true;
						claimCuryTaxRoundDiff += (claimdetail.ClaimCuryTaxRoundDiff ?? 0m) * signOperation;
						claimTaxRoundDiff += (claimdetail.ClaimTaxRoundDiff ?? 0m) * signOperation;
						tran.Date = claimdetail.ExpenseDate;

						if (contract.BaseType == CT.CTPRType.Project)
						{
							tran.ProjectID = claimdetail.ContractID;
						}
						else
						{
							tran.ProjectID = ProjectDefaultAttribute.NonProject();
						}

						tran.TaskID = claimdetail.TaskID;
						tran.CostCodeID = claimdetail.CostCodeID;

                    if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
                    {
                        CACorpCard card = CACorpCard.PKID.Find(this, claimdetail.CorpCardID);
                        CashAccount cashAccount = CashAccount.PK.Find(this, card.CashAccountID);

                        tran.AccountID = cashAccount.AccountID;
                        tran.SubID = cashAccount.SubID;
                    }
                    else
                    {
						tran.AccountID = claimdetail.ExpenseAccountID;
						tran.SubID = claimdetail.ExpenseSubID;
                    }
                        
						tran.BranchID = claimdetail.BranchID;

                    
                    tran = InsertInvoiceTransaction(apDocumentGraphExtension.InvoiceTrans.Cache, tran,
                                      new InvoiceTranContext { EPClaim = claim, EPClaimDetails = claimdetail });

                    if (claimdetail.PaidWith == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
					{
						claimdetail.APLineNbr = tran.LineNbr;
					}

						tran.CuryLineAmt = amount * signOperation;
						tran.CuryTaxAmt = 0;
						tran.CuryTaxableAmt = taxableAmt * signOperation;
						tran.CuryTaxAmt = taxAmt * signOperation;
                    tran.TaxCategoryID = claimdetail.TaxCategoryID;
                   
                   
                    tran = UpdateInvoiceTransaction(apDocumentGraphExtension.InvoiceTrans.Cache, tran,
                                     new InvoiceTranContext { EPClaim = claim, EPClaimDetails = claimdetail });


                    if ((claimdetail.CuryTipAmt ?? 0) != 0)
						{
                        InvoiceTran tranTip = new InvoiceTran();
							if (epsetup.NonTaxableTipItem == null)
							{
								throw new PXException(Messages.TipItemIsNotDefined);
							}
							IN.InventoryItem tipItem = PXSelect<IN.InventoryItem,
								Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(docgraph, epsetup.NonTaxableTipItem);

							if (tipItem == null)
							{
								string fieldname = PXUIFieldAttribute.GetDisplayName<EPSetup.nonTaxableTipItem>(docgraph.Caches[typeof(EPSetup)]);
								throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, fieldname, epsetup.NonTaxableTipItem);
							}
							tranTip.InventoryID = epsetup.NonTaxableTipItem;
							tranTip.TranDesc = tipItem.Descr;
                        if (CurrencyHelper.IsSameCury(expenseClaimGraph, claimdetail.CuryInfoID, claimdetail.ClaimCuryInfoID))
							{
								tranTip.CuryUnitCost = Math.Abs(claimdetail.CuryTipAmt ?? 0m);
								tranTip.CuryTranAmt = claimdetail.CuryTipAmt * signOperation;
							}
							else
							{
								decimal tipAmt;
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TipAmt, out tipAmt);
								tranTip.CuryUnitCost = Math.Abs(tipAmt);
								tranTip.CuryTranAmt = tipAmt * signOperation;
							}
							tranTip.Qty = tipQty;
							tranTip.UOM = tipItem.BaseUnit;
							tranTip.NonBillable = claimdetail.Billable != true;
							tranTip.Date = claimdetail.ExpenseDate;

							tranTip.BranchID = claimdetail.BranchID;
                        
                       tranTip = InsertInvoiceTipTransaction(apDocumentGraphExtension.InvoiceTrans.Cache, tranTip,
                                     new InvoiceTranContext { EPClaim = claim, EPClaimDetails = claimdetail });


                        if (epsetup.UseReceiptAccountForTips == true)
							{
								tranTip.AccountID = claimdetail.ExpenseAccountID;
								tranTip.SubID = claimdetail.ExpenseSubID;
							}
							else
							{
								tranTip.AccountID = tipItem.COGSAcctID;
								Location companyloc = (Location)PXSelectJoin<Location,
																			InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>,
																								And<Location.locationID, Equal<BAccountR.defLocationID>>>,
																			InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>,
																Where<GL.Branch.branchID, Equal<Current<APInvoice.branchID>>>>.Select(docgraph);
								PMTask task = PXSelect<PMTask,
														Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
														And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(docgraph, claimdetail.ContractID, claimdetail.TaskID);

								Location customerLocation = (Location)PXSelectorAttribute.Select<EPExpenseClaimDetails.customerLocationID>(claimdetailcache, claimdetail);

								int? employee_SubID = (int?)docgraph.Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(employee);
								int? item_SubID = (int?)docgraph.Caches[typeof(IN.InventoryItem)].GetValue<IN.InventoryItem.cOGSSubID>(tipItem);
								int? company_SubID = (int?)docgraph.Caches[typeof(Location)].GetValue<Location.cMPExpenseSubID>(companyloc);
								int? project_SubID = (int?)docgraph.Caches[typeof(Contract)].GetValue<Contract.defaultSubID>(contract);
								int? task_SubID = (int?)docgraph.Caches[typeof(PMTask)].GetValue<PMTask.defaultSubID>(task);
								int? location_SubID = (int?)docgraph.Caches[typeof(Location)].GetValue<Location.cSalesSubID>(customerLocation);

								object value = SubAccountMaskAttribute.MakeSub<EPSetup.expenseSubMask>(docgraph, epsetup.ExpenseSubMask,
									new object[] { employee_SubID, item_SubID, company_SubID, project_SubID, task_SubID, location_SubID },
									new Type[] { typeof(EPEmployee.expenseSubID), typeof(IN.InventoryItem.cOGSSubID), typeof(Location.cMPExpenseSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(Location.cSalesSubID) });

								docgraph.Caches[typeof(APTran)].RaiseFieldUpdating<APTran.subID>(tranTip, ref value);
								tranTip.SubID = (int?)value;
							}
                        
                      tranTip = UpdateInvoiceTipTransactionAccounts(apDocumentGraphExtension.InvoiceTrans.Cache, tranTip,
                                     new InvoiceTranContext { EPClaim = claim, EPClaimDetails = claimdetail });

                        tranTip.TaxCategoryID = tipItem.TaxCategoryID;
						tranTip.ProjectID = tran.ProjectID;
						tranTip.TaskID = tran.TaskID;
						tranTip.CostCodeID = tran.CostCodeID;
						tranTip = AddTaxes<TAPDocument, TInvoiceMapping, TGraph, TAPDocumentGraphExtension>(apDocumentGraphExtension, docgraph, expenseClaimGraph, invoice, signOperation, claimdetail, tranTip, true);


                        tranTip = UpdateInvoiceTipTransactionTaxesAndProject(apDocumentGraphExtension.InvoiceTrans.Cache, tranTip,
                                  new InvoiceTranContext { EPClaim = claim, EPClaimDetails = claimdetail });                        
                    }

                    PXNoteAttribute.CopyNoteAndFiles(claimdetailcache, claimdetail, apDocumentGraphExtension.InvoiceTrans.Cache, tran, epsetup.GetCopyNoteSettings<PXModule.ap>());
						claimdetail.Released = true;
						expenseClaimGraph.ExpenseClaimDetails.Update(claimdetail);
						#endregion

                    if (receiptGroupPaidWithType != EPExpenseClaimDetails.paidWith.CardPersonalExpense)
                    {
                        tran = AddTaxes<TAPDocument, TInvoiceMapping, TGraph, TAPDocumentGraphExtension>(apDocumentGraphExtension, docgraph, expenseClaimGraph, invoice, signOperation, claimdetail, tran, false);
                    }
                                                        
                    
                    
            }

					#region legacy taxes
					foreach (EPTaxAggregate tax in PXSelectReadonly<EPTaxAggregate,
						Where<EPTaxAggregate.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(docgraph, claim.RefNbr))
					{
						#region Add taxes
                    GenericTaxTran new_aptax = apDocumentGraphExtension.TaxTrans.Search<GenericTaxTran.taxID>(tax.TaxID);

						if (new_aptax == null)
						{
                        new_aptax = new GenericTaxTran();
							new_aptax.TaxID = tax.TaxID;
                        new_aptax = apDocumentGraphExtension.TaxTrans.Insert(new_aptax);
							if (new_aptax != null)
							{
                            new_aptax = (GenericTaxTran)apDocumentGraphExtension.TaxTrans.Cache.CreateCopy(new_aptax);
								new_aptax.CuryTaxableAmt = 0m;
								new_aptax.CuryTaxAmt = 0m;
								new_aptax.CuryExpenseAmt = 0m;
                            new_aptax = apDocumentGraphExtension.TaxTrans.Update(new_aptax);
							}
						}

						if (new_aptax != null)
						{
                        new_aptax = (GenericTaxTran)apDocumentGraphExtension.TaxTrans.Cache.CreateCopy(new_aptax);
							new_aptax.TaxRate = tax.TaxRate;
							new_aptax.CuryTaxableAmt = (new_aptax.CuryTaxableAmt ?? 0m) + tax.CuryTaxableAmt * signOperation;
							new_aptax.CuryTaxAmt = (new_aptax.CuryTaxAmt ?? 0m) + tax.CuryTaxAmt * signOperation;
							new_aptax.CuryExpenseAmt = (new_aptax.CuryExpenseAmt ?? 0m) + tax.CuryExpenseAmt * signOperation;
                        new_aptax = apDocumentGraphExtension.TaxTrans.Update(new_aptax);
						}
						#endregion
					}
					#endregion
                    
					invoice.CuryOrigDocAmt = invoice.CuryDocBal;
					invoice.CuryTaxAmt = invoice.CuryTaxTotal;
					invoice.Hold = false;
					apDocumentGraphExtension.SuppressApproval();
                apDocumentGraphExtension.Documents.Update(invoice);

                if (receiptGroupPaidWithType != EPExpenseClaimDetails.paidWith.CardPersonalExpense)
                {
					invoice.CuryTaxRoundDiff = invoice.CuryRoundDiff = invoice.CuryRoundDiff = claimCuryTaxRoundDiff;
					invoice.TaxRoundDiff = invoice.RoundDiff = claimTaxRoundDiff;
					bool inclusive = PXSelectJoin<APTaxTran, InnerJoin<Tax,
							  On<APTaxTran.taxID, Equal<Tax.taxID>>>,
							  Where<APTaxTran.refNbr, Equal<Required<APInvoice.refNbr>>,
							  And<APTaxTran.tranType, Equal<Required<APInvoice.docType>>,
							  And<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>>>>>
							  .Select(docgraph, invoice.RefNbr, invoice.DocType).Count > 0;
					if ((invoice.TaxCalcMode == TaxCalculationMode.Gross
						&& PXSelectJoin<APTaxTran, InnerJoin<Tax,
							  On<APTaxTran.taxID, Equal<Tax.taxID>>>,
							  Where<APTaxTran.refNbr, Equal<Required<APInvoice.refNbr>>,
							  And<APTaxTran.tranType, Equal<Required<APInvoice.docType>>,
							  And<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.calcOnItemAmt>>>>>
							  .Select(docgraph, invoice.RefNbr, invoice.DocType).Count > 0)
							  || inclusive)
					{

						decimal curyAdditionalDiff = -(invoice.CuryTaxRoundDiff ?? 0m) + (invoice.CuryTaxAmt ?? 0m) - (invoice.CuryDocBal ?? 0m);
						decimal additionalDiff = -(invoice.TaxRoundDiff ?? 0m) + (invoice.TaxAmt ?? 0m) - (invoice.DocBal ?? 0m);
                        foreach (InvoiceTran line in apDocumentGraphExtension.InvoiceTrans.Select())
						{
							curyAdditionalDiff += (line.CuryTaxableAmt ?? 0m) == 0m ? (line.CuryTranAmt ?? 0m) : (line.CuryTaxableAmt ?? 0m);
							additionalDiff += (line.TaxableAmt ?? 0m) == 0m ? (line.TranAmt ?? 0m) : (line.TaxableAmt ?? 0m);
						}

						invoice.CuryTaxRoundDiff += curyAdditionalDiff;
						invoice.TaxRoundDiff += additionalDiff;

					}
                }

	            invoice = apDocumentGraphExtension.Documents.Update(invoice);
                docgraph.Actions.PressSave();

				if (receiptGroupPaidWithType == EPExpenseClaimDetails.paidWith.CardCompanyExpense
					&& receiptGroup.Count == 1
					&& bankTranMatch != null)
	            {
		            CABankTransactionsMaint.RematchFromExpenseReceipt(this, bankTranMatch, invoice.CATranID, invoice.ContragentID, receiptGroup.Single());
	            }

                foreach (EPExpenseClaimDetails claimdetail in receiptGroup)
					{
						claimdetail.APDocType = invoice.DocType;
						claimdetail.APRefNbr = invoice.RefNbr;
						expenseClaimGraph.ExpenseClaimDetails.Update(claimdetail);
					}

					claim.Status = EPExpenseClaimStatus.ReleasedStatus;
					claim.Released = true;

					expenseClaimGraph.ExpenseClaim.Update(claim);

                
                #region EP History Update
                EPHistory hist = new EPHistory();
                hist.EmployeeID = invoice.ContragentID;
					hist.FinPeriodID = invoice.FinPeriodID;
					hist = (EPHistory)expenseClaimGraph.Caches[typeof(EPHistory)].Insert(hist);

					hist.FinPtdClaimed += invoice.DocBal;
					hist.FinYtdClaimed += invoice.DocBal;
                if (invoice.FinPeriodID == invoice.HeaderTranPeriodID)
					{
						hist.TranPtdClaimed += invoice.DocBal;
						hist.TranYtdClaimed += invoice.DocBal;
					}
					else
					{
						EPHistory tranhist = new EPHistory();
                    tranhist.EmployeeID = invoice.ContragentID;
                    tranhist.FinPeriodID = invoice.HeaderTranPeriodID;
						tranhist = (EPHistory)expenseClaimGraph.Caches[typeof(EPHistory)].Insert(tranhist);
						tranhist.TranPtdClaimed += invoice.DocBal;
						tranhist.TranYtdClaimed += invoice.DocBal;
					}
					expenseClaimGraph.Views.Caches.Add(typeof(EPHistory));
					#endregion

					expenseClaimGraph.Save.Press();
				
				Actions.PressSave();

                doclist.Add((APRegister)apDocumentGraphExtension.Documents.Current.Base);
				}

	        return doclist;
			}
       
        private InvoiceTran AddTaxes<TAPDocument, TInvoiceMapping, TGraph, TAPDocumentGraphExtension>
        (TAPDocumentGraphExtension apDocumentGraphExtension,
            TGraph docgraph,
            ExpenseClaimEntry expenseClaimGraph, 
            TAPDocument invoice, 
            decimal signOperation, 
            EPExpenseClaimDetails claimdetail, 
            InvoiceTran tran, 
            bool isTipTran)
            where TGraph : PXGraph, new()
            where TAPDocument : InvoiceBase, new()
            where TInvoiceMapping : IBqlMapping
            where TAPDocumentGraphExtension : PX.Objects.Common.GraphExtensions.Abstract.InvoiceBaseGraphExtension<TGraph, TAPDocument, TInvoiceMapping>

		{
			var cmdEPTaxTran = new PXSelect<EPTaxTran,
				Where<EPTaxTran.claimDetailID, Equal<Required<EPTaxTran.claimDetailID>>>>(docgraph);

			var cmdEPTax = new PXSelect<EPTax,
								Where<EPTax.claimDetailID, Equal<Required<EPTax.claimDetailID>>,
								And<EPTax.taxID, Equal<Required<EPTax.taxID>>>>>(docgraph);
			if (isTipTran)
			{
				cmdEPTaxTran.WhereAnd<Where<EPTaxTran.isTipTax, Equal<True>>>();
				cmdEPTax.WhereAnd<Where<EPTax.isTipTax, Equal<True>>>();
			}
			else
			{
				cmdEPTaxTran.WhereAnd<Where<EPTaxTran.isTipTax, Equal<False>>>();
				cmdEPTax.WhereAnd<Where<EPTax.isTipTax, Equal<False>>>();
			}

			CurrencyInfo expenseCuriInfo = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(docgraph, null, claimdetail.CuryInfoID);

			CurrencyInfo currencyinfo = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(docgraph, null, claimdetail.ClaimCuryInfoID);

			foreach (EPTaxTran epTaxTran in cmdEPTaxTran.Select(claimdetail.ClaimDetailID))
			{
				#region Add taxes
                GenericTaxTran new_aptax = apDocumentGraphExtension.TaxTrans.Search<GenericTaxTran.taxID>(epTaxTran.TaxID);

				if (new_aptax == null)
				{
                    new_aptax = new GenericTaxTran();
					new_aptax.TaxID = epTaxTran.TaxID;
                    TaxAttribute.SetTaxCalc<InvoiceTran.taxCategoryID>(apDocumentGraphExtension.InvoiceTrans.Cache, null, TaxCalc.NoCalc);
                    new_aptax = apDocumentGraphExtension.TaxTrans.Insert(new_aptax);
					if (new_aptax != null)
					{
                        new_aptax = (GenericTaxTran)apDocumentGraphExtension.TaxTrans.Cache.CreateCopy(new_aptax);
						new_aptax.CuryTaxableAmt = 0m;
						new_aptax.CuryTaxAmt = 0m;
						new_aptax.CuryExpenseAmt = 0m;
                        new_aptax = apDocumentGraphExtension.TaxTrans.Update(new_aptax);
					}
				}

				if (new_aptax != null)
				{
					EPTax epTax = cmdEPTax.Select(claimdetail.ClaimDetailID, new_aptax.TaxID);
                    new_aptax = (GenericTaxTran)apDocumentGraphExtension.TaxTrans.Cache.CreateCopy(new_aptax);
					new_aptax.TaxRate = epTaxTran.TaxRate;
					new_aptax.CuryTaxableAmt = (new_aptax.CuryTaxableAmt ?? 0m) + epTaxTran.ClaimCuryTaxableAmt * signOperation;
					new_aptax.CuryTaxAmt = (new_aptax.CuryTaxAmt ?? 0m) + epTaxTran.ClaimCuryTaxAmt * signOperation;
					new_aptax.CuryTaxAmtSumm = new_aptax.CuryTaxAmt;
					new_aptax.CuryExpenseAmt = (new_aptax.CuryExpenseAmt ?? 0m) + epTaxTran.ClaimCuryExpenseAmt * signOperation;
					new_aptax.NonDeductibleTaxRate = epTaxTran.NonDeductibleTaxRate;
                    TaxAttribute.SetTaxCalc<InvoiceTran.taxCategoryID>(apDocumentGraphExtension.InvoiceTrans.Cache, null, TaxCalc.ManualCalc);
                    new_aptax = apDocumentGraphExtension.TaxTrans.Update(new_aptax);
					//On first inserting APTaxTran APTax line will be created automatically. 
					//However, new APTax will not be inserted on APTaxTran line update, even if we already have more lines.
					//So, we have to do it manually.
                    LineTax aptax = apDocumentGraphExtension.LineTaxes.Search<LineTax.lineNbr, LineTax.taxID>(tran.LineNbr, new_aptax.TaxID);
					if (aptax == null)
					{
						decimal ClaimCuryTaxableAmt = 0m;
						decimal ClaimCuryTaxAmt = 0m;
						decimal ClaimCuryExpenseAmt = 0m;

                        if (CurrencyHelper.IsSameCury(claimdetail.CuryInfoID, claimdetail.ClaimCuryInfoID, expenseCuriInfo, currencyinfo))
						{
							ClaimCuryTaxableAmt = epTax.CuryTaxableAmt ?? 0m;
							ClaimCuryTaxAmt = epTax.CuryTaxAmt ?? 0m;
							ClaimCuryExpenseAmt = epTax.CuryExpenseAmt ?? 0m;
						}
						else if (currencyinfo?.CuryRate != null)
						{
							PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, epTax.TaxableAmt ?? 0m, out ClaimCuryTaxableAmt);
							PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, epTax.TaxAmt ?? 0m, out ClaimCuryTaxAmt);
							PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, epTax.ExpenseAmt ?? 0m, out ClaimCuryExpenseAmt);
						}
                        aptax = apDocumentGraphExtension.LineTaxes.Insert(new LineTax()
						{
							LineNbr = tran.LineNbr,
							TaxID = new_aptax.TaxID,
							TaxRate = epTax.TaxRate,
							CuryTaxableAmt = ClaimCuryTaxableAmt * signOperation,
							CuryTaxAmt = ClaimCuryTaxAmt * signOperation,
							CuryExpenseAmt = ClaimCuryExpenseAmt * signOperation
						});
					}
					Tax taxRow = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(docgraph, new_aptax.TaxID);
					if ((taxRow.TaxCalcLevel == CSTaxCalcLevel.Inclusive ||
						(invoice.TaxCalcMode == TaxCalculationMode.Gross && taxRow.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt))
						&& (tran.CuryTaxableAmt == null || tran.CuryTaxableAmt == 0m))
					{
						tran.CuryTaxableAmt = epTaxTran.ClaimCuryTaxableAmt * signOperation;
						tran.CuryTaxAmt = epTaxTran.ClaimCuryTaxAmt * signOperation;
						tran = apDocumentGraphExtension.InvoiceTrans.Update(tran);
					}
				}
				#endregion
			}

			return tran;
		}

        public class InvoiceTranContext
        {            
            public virtual EPExpenseClaim EPClaim { get; set; }

            public virtual EPExpenseClaimDetails EPClaimDetails { get; set; }
         
        }

        /// <summary>
        /// The method to insert invoice EP transactions 
        /// for the <see cref="EPExpenseClaim"/> entity inside the
        /// <see cref="ReleaseClaimDetails"/>
        /// <see cref="InvoiceTranContext"/> class content:
        /// <see cref="InvoiceTranContext.EPClaim"/>.
        /// <see cref="InvoiceTranContext.EPClaimDetails"/>.
        /// </summary>    

        public virtual InvoiceTran InsertInvoiceTransaction
            (PXCache InvoiceTrans, InvoiceTran tran, InvoiceTranContext invoiceTranInsertionContext)

        {
            return (InvoiceTran)InvoiceTrans.Insert(tran);
        }
        /// <summary>
        /// The method to update invoice EP transactions 
        /// for the <see cref="EPExpenseClaim"/> entity inside the
        /// <see cref="ReleaseClaimDetails"/>
        /// <see cref="InvoiceTranContext"/> class content:
        /// <see cref="InvoiceTranContext.EPClaim"/>.
        /// <see cref="InvoiceTranContext.EPClaimDetails"/>.
        /// </summary>
        public virtual InvoiceTran UpdateInvoiceTransaction
            (PXCache InvoiceTrans, InvoiceTran tran, InvoiceTranContext invoiceTranUpdationContext)

        {
            return (InvoiceTran)InvoiceTrans.Update(tran);
        }

        /// <summary>
        /// The method to insert invoice EP transactions 
        /// for the <see cref="EPExpenseClaim"/> entity inside the
        /// <see cref="ReleaseClaimDetails"/>
        /// <see cref="InvoiceTranContext"/> class content:
        /// <see cref="InvoiceTranContext.EPClaim"/>.
        /// <see cref="InvoiceTranContext.EPClaimDetails"/>.
        /// </summary>

        public virtual InvoiceTran InsertInvoiceTipTransaction
             (PXCache InvoiceTrans, InvoiceTran tran, InvoiceTranContext invoiceTranTipInsertionContext)

        {
            return (InvoiceTran)InvoiceTrans.Insert(tran);
        }

        /// <summary>
        /// The method to update invoice EP transactions tip
        /// for the <see cref="EPExpenseClaim"/> entity inside the
        /// <see cref="ReleaseClaimDetails"/>
        /// <see cref="InvoiceTranContext"/> class content:
        /// <see cref="InvoiceTranContext.EPClaim"/>.
        /// <see cref="InvoiceTranContext.EPClaimDetails"/>.
        /// </summary>
        public virtual InvoiceTran UpdateInvoiceTipTransactionAccounts
           (PXCache InvoiceTrans, InvoiceTran tran, InvoiceTranContext invoiceTranTipUpdationContext)

        {
            return (InvoiceTran)InvoiceTrans.Update(tran);
        }
                
        /// <summary>
        /// The method to update invoice EP transactions tip for the particular Project ID
        /// for the <see cref="EPExpenseClaim"/> entity inside the
        /// <see cref="ReleaseClaimDetails"/>
        /// <see cref="InvoiceTranContext"/> class content:
        /// <see cref="InvoiceTranContext.EPClaim"/>.
        /// <see cref="InvoiceTranContext.EPClaimDetails"/>.
        /// </summary>
        public virtual InvoiceTran UpdateInvoiceTipTransactionTaxesAndProject
		   (PXCache InvoiceTrans, InvoiceTran tran, InvoiceTranContext invoiceTranTipUpdationContext)

        {
            return (InvoiceTran)InvoiceTrans.Update(tran);
        }

    }


}
