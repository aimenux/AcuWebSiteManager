using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.SO;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.Common.Discount;


namespace PX.Objects.RUTROT
{
	/// <summary>
	/// The collection of methods that are useful for the logic related to RUT and ROT deduction.
	/// </summary>
	public static class RUTROTHelper
	{
		public static bool IsRUTROTcompatibleType(string type)
		{
			return (type == ARDocType.Invoice) || (type == ARDocType.DebitMemo) || (type == ARDocType.CreditMemo) ||
					(type == SOOrderTypeConstants.QuoteOrder) || (type == SOOrderTypeConstants.SalesOrder);
		}

		public static bool CurrenciesMatch(BranchRUTROT branchRR, IRUTROTable document)
		{
			return branchRR?.RUTROTCuryID == document?.GetDocumentCuryID();
		}

		public static bool IsRUTROTAllowed(BranchRUTROT branchRR, IRUTROTable document)
		{
			return branchRR?.AllowsRUTROT == true && IsRUTROTcompatibleType(document?.GetDocumentType()) && CurrenciesMatch(branchRR, document);
		}

		public static bool IsItemMatchRUTROTType(string rutrotType, InventoryItem item, InventoryItemRUTROT itemRR, bool isRUTROTDeductible)
		{
			return !(item != null && !string.IsNullOrEmpty(itemRR.RUTROTType)
					&& !string.IsNullOrEmpty(rutrotType) && itemRR.RUTROTType != rutrotType
					&& isRUTROTDeductible);
		}
		/// <summary>
		/// Indicates whether given <see cref="RUTROTWorkType"/> can be used for RUT or ROT deduction 
		/// on the given date.
		/// </summary>
		/// <param name="workTypeID">The <see cref="RUTROTWorkType.workTypeID">identifier</see> 
		/// of the <see cref="RUTROTWorkType"/> to be verified.</param>
		/// <param name="graph">The graph object to be used to perform database select.</param>
		public static bool IsUpToDateWorkType(int? workTypeID, DateTime currentDate, PXGraph graph)
		{
			RUTROTWorkType workType = PXSelect<RUTROTWorkType,
				Where<RUTROTWorkType.workTypeID, Equal<Required<RUTROTWorkType.workTypeID>>>>.Select(graph, workTypeID);
			return IsUpToDateWorkType(workType, currentDate);
		}
		/// <summary>
		/// Indicates whether given <see cref="RUTROTWorkType"/> can be used for RUT or ROT deduction 
		/// on the given date.
		/// </summary>
		public static bool IsUpToDateWorkType(RUTROTWorkType workType, DateTime currentDate)
		{
			if (workType == null)
			{
				return false;
			}
			return (workType.EndDate > currentDate || workType.EndDate == null) && workType.StartDate <= currentDate;
		}
		/// <summary>
		/// Creates a copy of <see cref="RUTROT"/> and sets all calculated fields to initial state.
		/// </summary>
		/// <param name="cache">The cache object to be used to create a copy of the <see cref="RUTROT"/> object.</param>
		/// <param name="source">The source <see cref="RUTROT"/> object to be used as a template for a new one.</param>
		/// <param name="newDocType">The document type to be set for a new <see cref="RUTROT"/> object.</param>
		/// <param name="newRefNbr">The reference number to be set for a new <see cref="RUTROT"/> object.</param>
		/// <returns>A new <see cref="RUTROT"/> object.</returns>
		public static RUTROT CreateCopy(PXCache cache, RUTROT source, string newDocType, string newRefNbr)
		{
			if (source == null)
			{
				return null;
			}
			RUTROT rutrot = (RUTROT)cache.CreateCopy(source);
			rutrot.DocType = newDocType;
			rutrot.RefNbr = newRefNbr;
			rutrot.CuryDistributedAmt = 0m;
			rutrot.CuryUndistributedAmt = 0m;
			rutrot.CuryTotalAmt = 0m;
			rutrot.CuryOtherCost = 0m;
			rutrot.CuryMaterialCost = 0m;
			rutrot.CuryWorkPrice = 0m;
			rutrot.DistributedAmt = 0m;
			rutrot.UndistributedAmt = 0m;
			rutrot.TotalAmt = 0m;
			rutrot.OtherCost = 0m;
			rutrot.MaterialCost = 0m;
			rutrot.WorkPrice = 0m;
			rutrot.DistributionLineCntr = 0;
			rutrot.IsClaimed = false;
			rutrot.ClaimDate = null;
			rutrot.ClaimFileName = null;
			cache.SetDefaultExt<RUTROT.noteID>(rutrot);
			return rutrot;
		}

		public static DocExt GetExtensionNullable<Doc, DocExt>(Doc doc)
			where Doc : class, IBqlTable, new()
			where DocExt : PXCacheExtension<Doc>
		{
			return doc == null ? null : PXCache<Doc>.GetExtension<DocExt>(doc);
		}

		public const string IsRUTROTDeductible = "IsRUTROTDeductible";

		public static BranchRUTROT GetBranchRUTROT(ClaimRUTROTProcess graph)
		{
			GL.Branch branch = PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(graph);
			return RUTROTHelper.GetExtensionNullable<GL.Branch, BranchRUTROT>(branch);
		}

        public static BranchRUTROT GetBranchRUTROT(ARInvoiceEntry graph)
        {
            GL.Branch branch = PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(graph);
            return RUTROTHelper.GetExtensionNullable<GL.Branch, BranchRUTROT>(branch);
        }

		public static BranchRUTROT GetBranchRUTROT(SOOrderEntry graph)
		{
			GL.Branch branch = PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(graph);
			return RUTROTHelper.GetExtensionNullable<GL.Branch, BranchRUTROT>(branch);
		}

		public static bool IsNeedBalancing(ARInvoiceEntry graph, ARInvoice invoice, string balanceOn)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
            {
                return false;
            }

            BranchRUTROT branchRUTROT = GetBranchRUTROT(graph);
			ValidateBranchRUTROTSettings(branchRUTROT);
            
            if (invoice == null)
            {
                return branchRUTROT?.BalanceOnProcess == balanceOn;
            }

            ARInvoiceRUTROT invoiceRUTROT = RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(invoice);

            return invoice.DocType == ARDocType.Invoice && invoiceRUTROT?.IsRUTROTDeductible == true &&
                   branchRUTROT?.BalanceOnProcess == balanceOn;
	}

        public static bool IsNeedBalancing(ARInvoiceEntry graph, string balanceOn) => IsNeedBalancing(graph, null, balanceOn);

		public static bool IsNeedBalancing(ClaimRUTROTProcess graph, string balanceOn)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
			{
				return false;
			}

			BranchRUTROT branchRUTROT = GetBranchRUTROT(graph);
			ValidateBranchRUTROTSettings(branchRUTROT);

			return branchRUTROT?.BalanceOnProcess == balanceOn;
		}

		private static void ValidateBranchRUTROTSettings(BranchRUTROT branch)
		{
			if (branch.TaxAgencyAccountID == null)
			{
				throw new PXException(RUTROTMessages.TaxAgencyAccountWasNotFoundForThisBranch);
			}
		}

		#region Balance Processing
		public static void BalanceARInvoiceRUTROT(ARInvoiceEntry graph, ARInvoice invoice, RUTROT rutrot = null, bool OnRelease = false)
		{
			if (rutrot == null)
			{
				rutrot = PXSelect<RUTROT, Where<RUTROT.refNbr, Equal<Required<ARInvoice.refNbr>>,
						And<RUTROT.docType, Equal<Required<ARInvoice.docType>>>>>.Select(graph, invoice.RefNbr, invoice.DocType);
			}

			if (!string.IsNullOrEmpty(rutrot?.BalancingCreditMemoRefNbr) && !string.IsNullOrEmpty(rutrot?.BalancingDebitMemoRefNbr))
			{
				return;
			}

			var releaseList = new List<ARRegister>();

			CreateMemo(graph, invoice, rutrot, ARDocType.CreditMemo, OnRelease: OnRelease);
			releaseList.Add(graph.Document.Current);
			string balancingCreditMemoRefNbr = graph.Document.Current.RefNbr;
			string balancingCreditMemoDocType = graph.Document.Current.DocType;
			CreateMemo(graph, invoice, rutrot, ARDocType.DebitMemo);
			releaseList.Add(graph.Document.Current);
			string balancingDebitMemoRefNbr = graph.Document.Current.RefNbr;
			string balancingDebitMemoDocType = graph.Document.Current.DocType;

			graph.Document.Current = invoice;

			using (new PXTimeStampScope(null))
			{
				ARDocumentRelease.ReleaseDoc(releaseList, false);
			}

			rutrot.BalancingCreditMemoRefNbr = balancingCreditMemoRefNbr;
			rutrot.BalancingDebitMemoRefNbr = balancingDebitMemoRefNbr;

			rutrot.BalancingCreditMemoDocType = balancingCreditMemoDocType;
			rutrot.BalancingDebitMemoDocType = balancingDebitMemoDocType;

			graph.Rutrots.Update(rutrot);
			graph.Save.Press();
		}

		private static void CreateMemo(ARInvoiceEntry graph, ARRegister doc, RUTROT rutrot, string docType, bool OnRelease = false)
        {
            DuplicateFilter filter = PXCache<DuplicateFilter>.CreateCopy(graph.duplicatefilter.Current);

            foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res in ARInvoice_CurrencyInfo_Terms_Customer.Select(graph, (object)doc.DocType, doc.RefNbr, doc.CustomerID))
            {
                CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
                info.CuryInfoID = null;
                info.IsReadOnly = false;
                info = PXCache<CurrencyInfo>.CreateCopy(graph.currencyinfo.Insert(info));

                ARInvoice invoice = (ARInvoice)graph.Document.Cache.CreateInstance();

                if (docType == ARDocType.CreditMemo)
                {
                    invoice.DueDate = null;
                    invoice.DiscDate = null;
                    invoice.CustomerID = doc.CustomerID;
                    invoice.ARAccountID = doc.ARAccountID;
                    invoice.ARSubID = doc.ARSubID;
                }

                if (docType == ARInvoiceType.DebitMemo)
                {
                    invoice.DueDate = ((ARInvoice)res).DueDate;
                    invoice.DiscDate = ((ARInvoice)res).DiscDate;

                    BranchRUTROT branchRUTROT = GetBranchRUTROT(graph);

                    invoice.CustomerID = branchRUTROT.TaxAgencyAccountID;
                    invoice.ARAccountID = null;
                    invoice.ARSubID = null;
                }

                ARInvoiceRUTROT invoiceRUTROT = RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(invoice);
                invoiceRUTROT.IsRUTROTDeductible = false;

                invoice.CuryInfoID = info.CuryInfoID;
                invoice.DocType = docType;
                invoice.OrigModule = GL.BatchModule.AR;
                invoice.RefNbr = null;
                invoice.OrigModule = GL.BatchModule.AR;
                invoice.DocDesc = PXLocalizer.LocalizeFormat(RUTROTMessages.MemoDescription, doc.RefNbr);

                invoice.OpenDoc = true;
                invoice.Released = false;
                invoice.Hold = false;
                invoice.Printed = false;
                invoice.Emailed = false;
                invoice.BatchNbr = null;
                invoice.ScheduleID = null;
                invoice.Scheduled = false;
                invoice.NoteID = null;
                invoice.RefNoteID = null;

                invoice.TermsID = null;
                invoice.InstallmentCntr = null;
                invoice.InstallmentNbr = null;
                invoice.CuryOrigDiscAmt = 0m;
                invoice.FinPeriodID = doc.FinPeriodID;
                invoice.OrigDocDate = invoice.DocDate;
                invoice.CuryLineTotal = 0m;
                invoice.IsTaxPosted = false;
                invoice.IsTaxValid = false;
                invoice.CuryVatTaxableTotal = 0m;
                invoice.CuryVatExemptTotal = 0m;
                invoice.StatementDate = null;
                invoice.PendingPPD = false;
                invoice.CustomerLocationID = null;

                if (!string.IsNullOrEmpty(invoice.PaymentMethodID))
                {
                    CA.PaymentMethod pm = null;

                    if (invoice.CashAccountID.HasValue)
                    {
                        CA.PaymentMethodAccount pmAccount = null;
                        PXResult<CA.PaymentMethod, CA.PaymentMethodAccount> pmResult = (PXResult<CA.PaymentMethod, CA.PaymentMethodAccount>)
                                                                                        PXSelectJoin<CA.PaymentMethod,
                                                                                            LeftJoin<
                                                                                                     CA.PaymentMethodAccount, On<CA.PaymentMethod.paymentMethodID, Equal<CA.PaymentMethodAccount.paymentMethodID>>>,
                                                                                               Where<
                                                                                                     CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>,
                                                                                                     And<CA.PaymentMethodAccount.cashAccountID, Equal<Required<CA.PaymentMethodAccount.cashAccountID>>>>>.
                                                                                         Select(graph, invoice.PaymentMethodID, invoice.CashAccountID);
                        pm = pmResult;
                        pmAccount = pmResult;

                        if (pm == null || pm.UseForAR == false || pm.IsActive == false)
                        {
                            invoice.PaymentMethodID = null;
                            invoice.CashAccountID = null;
                        }
                        else if (pmAccount == null || pmAccount.CashAccountID == null || pmAccount.UseForAR != true)
                        {
                            invoice.CashAccountID = null;
                        }
                    }
                    else
                    {
                        pm = PXSelect<CA.PaymentMethod,
                                Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>
                             .Select(graph, invoice.PaymentMethodID);

                        if (pm == null || pm.UseForAR == false || pm.IsActive == false)
                        {
                            invoice.PaymentMethodID = null;
                            invoice.CashAccountID = null;
                            invoice.PMInstanceID = null;
                        }
                    }

                    if (invoice.PMInstanceID.HasValue)
                    {
                        CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod,
                                                       Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.
                                                       Select(graph, invoice.PMInstanceID);

                        if (string.IsNullOrEmpty(invoice.PaymentMethodID) || cpm == null || cpm.IsActive == false || cpm.PaymentMethodID != invoice.PaymentMethodID)
                        {
                            invoice.PMInstanceID = null;
                        }
                    }
                }
                else
                {
                    invoice.CashAccountID = null;
                    invoice.PMInstanceID = null;
                }

                SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARInvoice.salesPersonID>(graph.Document.Cache, invoice);

                if (sp == null || sp.IsActive == false)
                {
                    invoice.SalesPersonID = null;
                }
                
                invoice = graph.Document.Insert(invoice);
            }

            TX.TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(graph.Transactions.Cache, null, TX.TaxCalc.ManualCalc);

            graph.FieldDefaulting.AddHandler<ARTran.salesPersonID>((sender, e) =>
            {
                e.NewValue = null;
                e.Cancel = true;
            });

			decimal roundedTotalDistributedLinesAmt = 0m;

            foreach (ARTran srcTran in PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
                And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(graph, doc.DocType, doc.RefNbr))
            {
                ARTran tran = PXCache<ARTran>.CreateCopy(srcTran);
                ARTranRUTROT tranRR = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>(tran);

				if (tranRR.IsRUTROTDeductible != true)
				{
					continue;
				}

                tran.TranType = graph.Document.Current.DocType;
                tran.RefNbr = graph.Document.Current.RefNbr;
                string origDrCr = tran.DrCr;
                tran.DrCr = null;
                tran.Released = null;
                tran.CuryInfoID = null;
                tran.SOOrderNbr = null;
                tran.SOShipmentNbr = null;
                tran.OrigInvoiceDate = tran.TranDate;
                tran.NoteID = null;
                tran.ManualPrice = true;
				tran.CuryTranAmt = Math.Floor(tranRR.CuryRUTROTAvailableAmt ?? 0m);
				roundedTotalDistributedLinesAmt += tran.CuryTranAmt ?? 0m;
				tranRR.IsRUTROTDeductible = false;

				if (!string.IsNullOrEmpty(tran.DeferredCode))
                {
                    DRSchedule schedule = PXSelect<DRSchedule,
                                             Where<DRSchedule.module, Equal<BQLConstants.moduleAR>,
                                               And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
                                               And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
                                               And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>.
                                           Select(graph, doc.DocType, doc.RefNbr, tran.LineNbr);

                    if (schedule != null)
                    {
                        tran.DefScheduleID = schedule.ScheduleID;
                    }
                }

                SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARTran.salesPersonID>(graph.Transactions.Cache, tran);

                if (sp == null || sp.IsActive == false)
                    tran.SalesPersonID = null;

                ARTran insertedTran = graph.Transactions.Insert(tran);
                PXNoteAttribute.CopyNoteAndFiles(graph.Transactions.Cache, srcTran, graph.Transactions.Cache, insertedTran);

                insertedTran.ManualDisc = true;

                insertedTran.TaxCategoryID = null;
                graph.Transactions.Update(insertedTran);
            }

			decimal distributedFee = (rutrot.CuryDistributedAmt ?? 0m) - roundedTotalDistributedLinesAmt;

			if (distributedFee != 0m)
			{
				foreach (ARTran artran in graph.Transactions.Cache.Inserted)
				{
					if (Math.Round(distributedFee) == 0m)
					{
						break;
					}
					if (artran.CuryTranAmt != 0m)
					{
						artran.CuryTranAmt += Math.Sign(distributedFee);
						distributedFee -= Math.Sign(distributedFee);
					}

					graph.Transactions.Update(artran);
				}
			}

			graph.Document.Current.CuryOrigDocAmt = graph.Document.Current.CuryDocBal;
			graph.Document.Cache.Update(graph.Document.Current);

            graph.RowInserting.AddHandler<ARSalesPerTran>((sender, e) => { e.Cancel = true; });

            foreach (ARSalesPerTran salespertran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
                And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(graph, doc.DocType, doc.RefNbr))
            {
                ARSalesPerTran newtran = PXCache<ARSalesPerTran>.CreateCopy(salespertran);

                newtran.DocType = graph.Document.Current.DocType;
                newtran.RefNbr = graph.Document.Current.RefNbr;
                newtran.Released = false;
                newtran.CuryInfoID = null;
                newtran.CuryCommnblAmt *= -1m;
                newtran.CuryCommnAmt *= -1m;

                SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARSalesPerTran.salespersonID>(graph.salesPerTrans.Cache, newtran);

                if (!(sp == null || sp.IsActive == false))
                {
                    graph.salesPerTrans.Update(newtran);
                }
            }

            var discountDetailsSet = PXSelect<ARInvoiceDiscountDetail,
                Where<ARInvoiceDiscountDetail.docType, Equal<Required<ARInvoice.docType>>,
                    And<ARInvoiceDiscountDetail.refNbr, Equal<Required<ARInvoice.refNbr>>>>,
                OrderBy<Asc<ARInvoiceDiscountDetail.docType,
                    Asc<ARInvoiceDiscountDetail.refNbr>>>>
                .Select(graph, doc.DocType, doc.RefNbr);

            foreach (ARInvoiceDiscountDetail discountDetail in discountDetailsSet)
            {
                ARInvoiceDiscountDetail newDiscountDetail = PXCache<ARInvoiceDiscountDetail>.CreateCopy(discountDetail);

                newDiscountDetail.DocType = graph.Document.Current.DocType;
                newDiscountDetail.RefNbr = graph.Document.Current.RefNbr;
                newDiscountDetail.IsManual = true;
                DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>().UpdateDiscountDetail(graph.ARDiscountDetails.Cache, graph.ARDiscountDetails, newDiscountDetail);
            }

            graph.Save.Press();

            if (docType == ARDocType.CreditMemo && !OnRelease)
            {
                CreateAdjustment(graph, doc, graph.Document.Current);
            }
        }

        public static void CreateAdjustment(ARInvoiceEntry graph, ARInvoice invoice, RUTROT rutrot)
        {
            ARPayment creditMemo = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<RUTROT.docType>>,
            And<ARPayment.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(graph, rutrot.BalancingCreditMemoDocType, rutrot.BalancingCreditMemoRefNbr);

            ARAdjust2 applicationToCreditMemo = new ARAdjust2
            {
                AdjdDocType = invoice.DocType,
                AdjdRefNbr = invoice.RefNbr,
                AdjgDocType = creditMemo.DocType,
                AdjgRefNbr = creditMemo.RefNbr,
                AdjNbr = creditMemo.AdjCntr,
                CustomerID = creditMemo.CustomerID,
                AdjdCustomerID = invoice.CustomerID,
                AdjdBranchID = invoice.BranchID,
                AdjgBranchID = creditMemo.BranchID,
                AdjgCuryInfoID = creditMemo.CuryInfoID,
                AdjdOrigCuryInfoID = invoice.CuryInfoID,
                AdjdCuryInfoID = invoice.CuryInfoID,
                CuryAdjdAmt = creditMemo.CuryDocBal
            };

            graph.Adjustments.Insert(applicationToCreditMemo);
            graph.Save.Press();
        }

        private static void CreateAdjustment(ARInvoiceEntry graph, ARRegister invoice, ARInvoice creditMemo)
        {
            ARAdjust applicationToCreditMemo = new ARAdjust
            {
                AdjgDocType = creditMemo.DocType,
                AdjgRefNbr = creditMemo.RefNbr,
                AdjdDocType = invoice.DocType,
                AdjdRefNbr = invoice.RefNbr,
                CuryAdjgAmt = creditMemo.CuryDocBal
            };

            graph.Adjustments_1.Insert(applicationToCreditMemo);
            graph.Save.Press();
        }
        #endregion
    }
}
