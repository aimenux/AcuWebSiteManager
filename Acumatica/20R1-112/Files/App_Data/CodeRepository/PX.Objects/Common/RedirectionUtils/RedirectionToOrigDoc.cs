using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.EP;
using PX.Objects.GL;

using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;

namespace PX.Objects.Common
{
	/// <summary>
	/// A helper class that manages redirection to the original document in AR, AP, GL and CA.
	/// </summary>
	public static class RedirectionToOrigDoc
	{
        /// <summary>
        /// Tries to find and redirect to the original document using given original document type, refNbr and module.
        /// </summary>
        /// <param name="origDocType">Type of the original document.</param>
        /// <param name="origRefNbr">The original document reference number.</param>
        /// <param name="origModule">The original document module.</param>
		public static void TryRedirect(string origDocType, string origRefNbr, string origModule, bool preferPrimaryDocForm = false)
		{
			if (string.IsNullOrWhiteSpace(origRefNbr) || string.IsNullOrWhiteSpace(origModule))
				return;

			PXGraph destinationGraph = PrepareDestinationGraph(origDocType, origRefNbr, origModule, preferPrimaryDocForm);

			if (destinationGraph == null)
				return;
         
            PXRedirectHelper.TryRedirect(destinationGraph, PXRedirectHelper.WindowMode.NewWindow);         
		}

        private static PXGraph PrepareDestinationGraph(string origDocType, string origRefNbr, string origModule, bool preferPrimaryDocForm)
		{
            //For redirections to GL entries original module can be not equal to GL but 
            //origDocType GLTranType.GLEntry defines redirection to GL unambiguously 
            if (origDocType == GLTranType.GLEntry)
            {
                return PrepareDestinationGraphForGL(origModule, origRefNbr);
            }

            switch (origModule)
            {
                case BatchModule.AP:
                    return PrepareDestinationGraphForAP(origDocType, origRefNbr, preferPrimaryDocForm);
                  
                case BatchModule.AR:
                    return PrepareDestinationGraphForAR(origDocType, origRefNbr);

                case BatchModule.CA:
                    return PrepareDestinationGraphForCA(origDocType, origRefNbr);

				case BatchModule.EP:
					return PrepareDestinationGraphForEP(origDocType, origRefNbr);

				case BatchModule.SO:
					return MakeDestinationGraph<SO.SOInvoiceEntry, ARInvoice, ARInvoice.docType, ARInvoice.refNbr>(origDocType, origRefNbr);

				default:
                    return null;
            }                      
        }

        private static PXGraph PrepareDestinationGraphForGL(string origModule, string origRefNbr)
        {
            JournalEntry journalEntryGraph = PXGraph.CreateInstance<JournalEntry>();
            journalEntryGraph.BatchModule.Current =
                PXSelect<Batch,
                    Where<Batch.module, Equal<Required<Batch.module>>,
                      And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
                .Select(journalEntryGraph, origModule, origRefNbr);

            return journalEntryGraph;
        }

		private static PXGraph PrepareDestinationGraphForEP(string docType, string origRefNbr)
		{
			if (string.IsNullOrEmpty(docType) || docType == EPExpenseClaim.DocType)
			{
				EP.ExpenseClaimEntry expenseClaimGraph = PXGraph.CreateInstance<EP.ExpenseClaimEntry>();
				expenseClaimGraph.ExpenseClaim.Current =
					PXSelect<EP.EPExpenseClaim,
							Where<EP.EPExpenseClaim.refNbr, Equal<Required<EP.EPExpenseClaim.refNbr>>>>
						.Select(expenseClaimGraph, origRefNbr);

				return expenseClaimGraph;
			}
			else if (docType == EPExpenseClaimDetails.DocType)
			{
				ExpenseClaimDetailEntry expenseReceiptGraph = PXGraph.CreateInstance<EP.ExpenseClaimDetailEntry>();
				expenseReceiptGraph.ClaimDetails.Current =
					PXSelect<EP.EPExpenseClaimDetails,
							Where<EP.EPExpenseClaimDetails.claimDetailCD, Equal<Required<EP.EPExpenseClaimDetails.claimDetailCD>>>>
						.Select(expenseReceiptGraph, origRefNbr);

				return expenseReceiptGraph;
			}

			return null;
		}

		private static PXGraph PrepareDestinationGraphForAP(string origDocType, string origRefNbr, bool preferPrimaryDocForm)
        {
	        if (origDocType == APDocType.Invoice
	            || origDocType == APDocType.CreditAdj
	            || origDocType == APDocType.DebitAdj && preferPrimaryDocForm)
	        {
		        return MakeDestinationGraph<APInvoiceEntry, APInvoice, APInvoice.docType, APInvoice.refNbr>(origDocType, origRefNbr);
			}

            switch (origDocType)
            {     
                case APDocType.QuickCheck:
                case APDocType.VoidQuickCheck:
                    return MakeDestinationGraph<APQuickCheckEntry, APQuickCheck, APQuickCheck.docType, APQuickCheck.refNbr>(origDocType, origRefNbr);
                   
                case CATranType.CABatch:
                    CABatchEntry caBatchGraph = PXGraph.CreateInstance<CABatchEntry>();                  
                    caBatchGraph.Document.Current = 
                        PXSelect<CABatch, 
                            Where<CABatch.batchNbr, Equal<Required<CATran.origRefNbr>>>>
                        .Select(caBatchGraph, origRefNbr);

                    return caBatchGraph;

                case APDocType.Check:
                case APDocType.DebitAdj:
                case APDocType.Prepayment:
                case APDocType.Refund:
                case APDocType.VoidCheck:
                    return MakeDestinationGraph<APPaymentEntry, APPayment, APPayment.docType, APPayment.refNbr>(origDocType, origRefNbr);
                  
                default:
                    return null;                
            }         
        }

        private static PXGraph PrepareDestinationGraphForAR(string origDocType, string origRefNbr)
        {          
            switch (origDocType)
            {
                case ARDocType.Invoice:
                case ARDocType.DebitMemo:
                    return MakeDestinationGraph<ARInvoiceEntry, ARInvoice, ARInvoice.docType, ARInvoice.refNbr>(origDocType, origRefNbr);
                   
                case ARDocType.CashSale:
                case ARDocType.CashReturn:
                    ARCashSaleEntry arCashSalesGraph = PXGraph.CreateInstance<ARCashSaleEntry>();
                    ARCashSale origCashSale =
                        GetOriginalDocument<ARCashSale, ARCashSale.docType, ARCashSale.refNbr>(arCashSalesGraph, origDocType, origRefNbr);
                    
                    if (origCashSale?.OrigModule == BatchModule.SO && origCashSale.Released == false)
                    {
                        return MakeDestinationGraph<SO.SOInvoiceEntry, ARInvoice, ARInvoice.docType, ARInvoice.refNbr>(origDocType, origRefNbr);                       
                    }

                    arCashSalesGraph.Document.Current = origCashSale;                
                    return arCashSalesGraph;

                case ARDocType.Payment:
                case ARDocType.CreditMemo:
                case ARDocType.Prepayment:
                case ARDocType.Refund:
                case ARDocType.VoidRefund:
                case ARDocType.VoidPayment:
                case ARDocType.SmallBalanceWO:
                    return MakeDestinationGraph<ARPaymentEntry, ARPayment, ARPayment.docType, ARPayment.refNbr>(origDocType, origRefNbr);                  
                    
                default:
                    return null;
            }                         
        }

        private static PXGraph PrepareDestinationGraphForCA(string origDocType, string origRefNbr)
        {
            switch (origDocType)
            {
                case CATranType.CAAdjustment:
                case CATranType.CAAdjustmentRGOL:
                    CATranEntry caTranGraph = PXGraph.CreateInstance<CATranEntry>();  
                                                      
                    caTranGraph.CAAdjRecords.Current = origDocType == CATranType.CATransferExp
                        ? GetOriginalDocument<CAAdj, CAAdj.adjTranType, CAAdj.transferNbr>(caTranGraph, origDocType, origRefNbr) 
                        : GetOriginalDocument<CAAdj, CAAdj.adjTranType, CAAdj.adjRefNbr>(caTranGraph, origDocType, origRefNbr);

                    return caTranGraph;

                case CATranType.CADeposit:
                case CATranType.CAVoidDeposit:
                    return MakeDestinationGraph<CADepositEntry, CADeposit, CADeposit.tranType, CADeposit.refNbr>(origDocType, origRefNbr);
                   
                case CATranType.CATransferIn:
                case CATranType.CATransferOut:
				case CATranType.CATransferExp:
				case CATranType.CATransferRGOL:
                    CashTransferEntry cashTransferGraph = PXGraph.CreateInstance<CashTransferEntry>();                                   
                    cashTransferGraph.Transfer.Current =
                        PXSelect<CATransfer,
                            Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>
                        .Select(cashTransferGraph, origRefNbr);

                    return cashTransferGraph;

                default:
                    return null;
            }                                     
        }

        /// <summary>
        /// Makes the destination graph for most common redirection cases.
        /// </summary>
        /// <typeparam name="TGraph">The type of the destination graph.</typeparam>
        /// <typeparam name="TOrigDoc">The type of the original document.</typeparam>
        /// <typeparam name="TDocType">The type of the document type field.</typeparam>
        /// <typeparam name="TRefNbr">The type of the reference number field.</typeparam>
        /// <param name="origDocType">Type of the original document.</param>
        /// <param name="origRefNbr">The original document reference number.</param>
        /// <returns></returns>
        private static PXGraph MakeDestinationGraph<TGraph, TOrigDoc, TDocType, TRefNbr>(string origDocType, string origRefNbr)
        where TGraph : PXGraph, new()
        where TOrigDoc : class, IBqlTable, new()
        where TDocType : IBqlField
        where TRefNbr : IBqlField
        {
            TGraph destinationGraph = PXGraph.CreateInstance<TGraph>();
            TOrigDoc origDoc =
                GetOriginalDocument<TOrigDoc, TDocType, TRefNbr>(destinationGraph, origDocType, origRefNbr);

            Type mainDacType = origDoc?.GetType() ?? typeof(TOrigDoc);
            destinationGraph.Caches[mainDacType].Current = origDoc;            
            return destinationGraph;
        }

        private static TOrigDoc GetOriginalDocument<TOrigDoc, TDocType, TRefNbr>(PXGraph origDocGraph, string origDocType, string origRefNbr)
        where TOrigDoc : class, IBqlTable, new()
        where TDocType : IBqlField
        where TRefNbr : IBqlField
        {
            TOrigDoc origDoc = 
                PXSelect<TOrigDoc,
                    Where<TDocType, Equal<Required<TDocType>>,
                      And<TRefNbr, Equal<Required<TRefNbr>>>>>
               .Select(origDocGraph, origDocType, origRefNbr);

            return origDoc;
        }
    }
}
