using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Descriptor.Attributes;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Abstractions;
using PX.Objects.CR;
using PX.Objects.CS;
using Messages = PX.Objects.AP.Messages;

namespace PX.Objects.CN.JointChecks.AP.Services
{
    public class JointChecksCreator
    {
        protected readonly APInvoiceEntry InvoiceEntry;
        protected APPaymentEntry PaymentEntry;
        protected ComplianceDocumentsService ComplianceDocumentsService;
        private APInvoiceJCExt invoiceExtension;
        private VendorBalancePerLineCalculationService vendorBalancePerLineCalculationService;

        public JointChecksCreator(APInvoiceEntry graph, PXSelect<JointPayeePayment> jointPayeePayments)
        {
            InvoiceEntry = graph;
            JointPayeePayments = jointPayeePayments;
            ComplianceDocumentsService = new ComplianceDocumentsService(InvoiceEntry,
                JointPayeePayments.Select().FirstTableItems);
        }

        public JointChecksCreator(APInvoiceEntry graph)
            : this(graph, graph.GetExtension<ApInvoiceEntryPayBillExtension>().JointPayeePayments)
        {
        }

        protected APInvoiceJCExt InvoiceExtension =>
            invoiceExtension ?? (invoiceExtension =
                PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(InvoiceEntry.Document.Current));

        protected PXSelect<JointPayeePayment> JointPayeePayments
        {
            get;
        }

        protected List<JointPayeePayment> InvoiceJointPayeePayments
        {
            get;
            set;
        }

        protected List<ComplianceDocument> ComplianceDocuments
        {
            get;
            set;
        }

        public virtual APPayment CreateJointChecks(List<JointPayeePayment> jointPayeePayments)
        {
            APPayment apPayment;
            InvoiceJointPayeePayments = jointPayeePayments;
            using (var scope = new PXTransactionScope())
            {
                ComplianceDocumentsService = new ComplianceDocumentsService(InvoiceEntry, InvoiceJointPayeePayments);
                apPayment = GenerateMultipleChecks();
                OpenPaymentCycleWorkflow();
                InvoiceEntry.Caches<ComplianceDocumentReference>().Persist(PXDBOperation.Delete);
				scope.Complete();
            }
            return apPayment;
        }

        protected APPayment GenerateVendorCheck()
        {
            if (InvoiceExtension.AmountToPay == 0)
            {
                return null;
            }
            var vendorCheck = CreateVendorCheck();
            ComplianceDocumentsService.UpdateComplianceDocumentsForVendorCheck(ComplianceDocuments, vendorCheck);
            CreateZeroAmountJointPayeePaymentsForVendorCheck(vendorCheck);
            MarkPaymentForLienWaiverGeneration(true);
            PaymentEntry.Persist();
            return vendorCheck;
        }

        protected IEnumerable<APPayment> GenerateJointChecks()
        {
            var checkGenerationModels = CreateCheckGenerationModels().ToList();
            foreach (var model in checkGenerationModels)
            {
                model.CreatedPayment = CreateJointCheck(model);
                UpdateComplianceDocumentForJointCheck(model);
            }
            
            return checkGenerationModels.Select(AddJointPayeePaymentsToCheck).ToList();
        }

        protected virtual IEnumerable<int?> GetBillLineNumbersForVendorCheck()
        {
            return IsPaymentsByLinesAllowed()
                ? InvoiceEntry.Transactions.SelectMain().Select(tran => tran.LineNbr)
                : ((int?) decimal.Zero).AsSingleEnumerable();
        }

        protected void OpenPaymentCycleWorkflow()
        {
            InvoiceExtension.IsPaymentCycleWorkflow = true;
            InvoiceEntry.Document.Cache.PersistUpdated(InvoiceEntry.Document.Current);
        }

        /// <summary>
        /// Lien Waivers should be created on payment persisting only after <see cref="JointPayeePayment"/> records are
        /// already inserted in order to retrieve correct amounts.
        /// </summary>
        protected void MarkPaymentForLienWaiverGeneration(bool shouldCreateLienWaivers)
        {
            var paymentExtension = PXCache<APPayment>.GetExtension<ApPaymentExt>(PaymentEntry.Document.Current);
            paymentExtension.ShouldCreateLienWaivers = shouldCreateLienWaivers;
            PaymentEntry.Document.UpdateCurrent();
        }

        protected void CreatePaymentEntry(APPaymentEntry paymentEntry = null)
        {
            PaymentEntry = paymentEntry ?? PXGraph.CreateInstance<APPaymentEntry>();
            var paymentEntryExtension = PaymentEntry.GetExtension<ApPaymentEntryExt>();
            PaymentEntry.RowPersisting.RemoveHandler<APAdjust>(paymentEntryExtension.APAdjust_RowPersisting);
        }

        private APPayment CreateVendorCheck()
        {
            GeneratePayment(InvoiceEntry.Document.Current);
            var billLineNumbers = GetBillLineNumbersForVendorCheck();
            InsertAdjustments(billLineNumbers);
            ResetCashDiscount();
            SetAdjustmentsAmountForVendorCheck(InvoiceExtension.AmountToPay);
            SetPaymentAmount(InvoiceExtension.AmountToPay);
            SavePayment();
            return PaymentEntry.Document.Current;
        }

        private APPayment GenerateMultipleChecks()
        {
            CreatePaymentEntry();
            JointPayeePayments.Cache.Clear();
            ComplianceDocuments = ComplianceDocumentsService.GetComplianceDocumentsToLink();
            var vendorCheck = GenerateVendorCheck();
            var jointChecks = GenerateJointChecks();
            ComplianceDocumentsService.ClearLinkToPaymentFlag(ComplianceDocuments);
            InvoiceEntry.Views.Caches.Add(typeof(JointPayeePayment));
            InvoiceEntry.Persist();
            MarkPaymentForLienWaiverGeneration(true);
            PaymentEntry.Persist();
            return vendorCheck ?? jointChecks.First();
        }

        private IEnumerable<JointCheckGenerationModel> CreateCheckGenerationModels()
        {
            var jointPayeePayments = InvoiceJointPayeePayments.Where(jpp => jpp.JointAmountToPay > 0).ToList();
            var jointPayeeGroups = GetJointPayeeGroups(jointPayeePayments);
            return jointPayeeGroups.Select(jp => JointCheckGenerationModel.Create(jointPayeePayments, jp))
                .OrderBy(gm => gm.InvoiceJointPayeePayments.Min(jpp => jpp.JointPayeeId));
        }

        private IEnumerable<IEnumerable<JointPayee>> GetJointPayeeGroups(
            IEnumerable<JointPayeePayment> jointPayeePayments)
        {
            var jointPayees = GetJointPayees(jointPayeePayments).ToList();
            var jointPayeesWithSameExternalName = jointPayees.Where(jp => jp.JointPayeeExternalName != null)
                .GroupBy(jp => jp.JointPayeeExternalName).Cast<IEnumerable<JointPayee>>();
            var jointPayeeWithSameId = jointPayees.Where(jp => jp.JointPayeeInternalId != null)
                .GroupBy(jp => jp.JointPayeeInternalId);
            return jointPayeesWithSameExternalName.Concat(jointPayeeWithSameId);
        }

        private void UpdateComplianceDocumentForJointCheck(JointCheckGenerationModel jointCheckGenerationModel)
        {
            var jointPayeeId = jointCheckGenerationModel.InvoiceJointPayeePayments.First().JointPayeeId;
            var jointPayee = GetJointPayee(jointPayeeId);
            foreach (var complianceDocument in ComplianceDocuments)
            {
                ComplianceDocumentsService.UpdateComplianceForJointCheck(
                    jointPayee, complianceDocument, jointCheckGenerationModel.CreatedPayment);
            }
        }

        private JointPayee GetJointPayee(int? jointPayeeId)
        {
            var query = new PXSelect<JointPayee,
                Where<JointPayee.jointPayeeId, Equal<Required<JointPayee.jointPayeeId>>>>(InvoiceEntry);
            return query.SelectSingle(jointPayeeId);
        }

        private IEnumerable<JointPayee> GetJointPayees(IEnumerable<JointPayeePayment> jointPayeePayments)
        {
            var jointPayeeIds = jointPayeePayments.Select(jpp => jpp.JointPayeeId);
            var query = new PXSelect<JointPayee,
                Where<JointPayee.jointPayeeId, In<Required<JointPayee.jointPayeeId>>>>(InvoiceEntry);
            return query.Select(jointPayeeIds.ToArray()).FirstTableItems;
        }

        private APPayment AddJointPayeePaymentsToCheck(JointCheckGenerationModel jointCheckGenerationModel)
        {
            CreateRelatedJointPayment(jointCheckGenerationModel);
            CreateZeroAmountJointPayeePaymentsForJointCheck(jointCheckGenerationModel);
            return jointCheckGenerationModel.CreatedPayment;
        }

        private void CreateRelatedJointPayment(JointCheckGenerationModel jointCheckGenerationModel)
        {
            var jointPayeePayments = jointCheckGenerationModel.InvoiceJointPayeePayments.Select(jpp =>
                CreateJointPayeePayment(jpp.JointPayeeId, jointCheckGenerationModel.CreatedPayment,
                    jpp.JointAmountToPay));
            JointPayeePayments.Cache.InsertAll(jointPayeePayments);
        }

        private void CreateZeroAmountJointPayeePaymentsForVendorCheck(IDocumentKey vendorCheck)
        {
            var jointPayeeIds = InvoiceJointPayeePayments.Select(jpp => jpp.JointPayeeId);
            CreateZeroAmountJointPayeePayments(vendorCheck, jointPayeeIds);
        }

        private void CreateZeroAmountJointPayeePaymentsForJointCheck(
            JointCheckGenerationModel jointCheckGenerationModel)
        {
            var invoiceJointPayeePayments = jointCheckGenerationModel.InvoiceJointPayeePayments.ToList();
            var jointPayeeIds = InvoiceJointPayeePayments
                .Where(jpp => !invoiceJointPayeePayments.Contains(jpp)
                    && jointCheckGenerationModel.BillLineNumbers.Contains(jpp.BillLineNumber))
                .Select(jpp => jpp.JointPayeeId);
            CreateZeroAmountJointPayeePayments(jointCheckGenerationModel.CreatedPayment, jointPayeeIds);
        }

        private void CreateZeroAmountJointPayeePayments(IDocumentKey payment, IEnumerable<int?> jointPayeeIds)
        {
            var jointPayeePayments = jointPayeeIds
                .Select(jointPayeeId => CreateJointPayeePayment(jointPayeeId, payment, 0));
            JointPayeePayments.Cache.InsertAll(jointPayeePayments);
        }

        private bool IsPaymentsByLinesAllowed()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>() &&
                InvoiceEntry.Document.Current.PaymentsByLinesAllowed == true;
        }

        private APPayment CreateJointCheck(JointCheckGenerationModel jointCheckGenerationModel)
        {
            GeneratePayment(InvoiceEntry.Document.Current);
            InsertAdjustments(jointCheckGenerationModel.BillLineNumbers);
            ResetCashDiscount();
            SetAdjustmentsAmountForJointCheck(jointCheckGenerationModel);
            SetPaymentAmount(jointCheckGenerationModel.JointAmountToPay);
            SavePayment();
            return PaymentEntry.Document.Current;
        }

        private JointPayeePayment CreateJointPayeePayment(int? payeeId, IDocumentKey payment, decimal? amount)
        {
            return new JointPayeePayment
            {
                JointPayeeId = payeeId,
                PaymentDocType = payment.DocType,
                PaymentRefNbr = payment.RefNbr,
                InvoiceDocType = InvoiceEntry.Document.Current.DocType,
                InvoiceRefNbr = InvoiceEntry.Document.Current.RefNbr,
                JointAmountToPay = amount,
                AdjustmentNumber = 0
            };
        }

        private void GeneratePayment(APInvoice invoice)
        {
            PaymentEntry.Clear();
            var location = TryGetInvoiceLocation(invoice);
            var payAccount = invoice.PayAccountID ?? location.CashAccountID;
            var cashAccount = TryGetVendorCashAccount(payAccount);
            var payment = CreateApPayment(invoice);
            UpdatePayment(invoice, payment, cashAccount, payAccount, location);
            SuppressDefaultEvents(invoice);
            PaymentEntry.Document.Current = PaymentEntry.Document.Update(payment);
        }

        private void SuppressDefaultEvents(IDocumentKey invoice)
        {
            PaymentEntry.FieldDefaulting.AddHandler<APPayment.cashAccountID>(
                (cache, args) => InsertCashAccountId(args, invoice));
            PaymentEntry.FieldDefaulting.AddHandler<CurrencyInfo.curyID>((cache, args) => InsertCurrencyId(args));
        }

        private void UpdatePayment(APInvoice invoice, APPayment payment, CashAccount cashAccount, int? payAccount,
            Location location)
        {
            payment.VendorID = invoice.VendorID;
            payment.VendorLocationID = invoice.PayLocationID;
            payment.AdjDate = GetPaymentApplicationDate(invoice);
            payment.CashAccountID = cashAccount.CuryID == invoice.CuryID
                ? payAccount
                : invoice.PayAccountID;
            payment.CuryID = invoice.CuryID;
            payment.PaymentMethodID = invoice.PayTypeID ?? location.PaymentMethodID;
            payment.DocDesc = invoice.DocDesc;
        }

        private DateTime? GetPaymentApplicationDate(IRegister invoice)
        {
            var businessDate = PaymentEntry.Accessinfo.BusinessDate.GetValueOrDefault();
            var invoiceDate = invoice.DocDate.GetValueOrDefault();
            return businessDate < invoiceDate
                ? invoiceDate
                : businessDate;
        }

        private CashAccount TryGetVendorCashAccount(int? payAccount)
        {
            var cashAccount = GetVendorCashAccount(payAccount);
            return cashAccount ?? throw new PXException(Messages.VendorMissingCashAccount);
        }

        private CashAccount GetVendorCashAccount(int? payAccount)
        {
            return SelectFrom<CashAccount>
                .Where<CashAccount.cashAccountID.IsEqual<P.AsInt>>.View
                .Select(PaymentEntry, payAccount);
        }

        private Location TryGetInvoiceLocation(APInvoice invoice)
        {
            var location = GetInvoiceLocation(invoice);
            return location ?? throw new PXException(Messages.InternalError, 502);
        }

        private static void InsertCurrencyId(PXFieldDefaultingEventArgs args)
        {
            if (args.Row == null)
            {
                return;
            }
            args.NewValue = ((CurrencyInfo) args.Row).CuryID;
            args.Cancel = true;
        }

        private static void InsertCashAccountId(PXFieldDefaultingEventArgs args, IDocumentKey invoice)
        {
            if (invoice.DocType != APDocType.Prepayment)
            {
                return;
            }
            args.NewValue = null;
            args.Cancel = true;
        }

        private Location GetInvoiceLocation(APInvoice invoice)
        {
            return SelectFrom<Location>
                .Where<Location.bAccountID.IsEqual<P.AsInt>
                    .And<Location.locationID.IsEqual<P.AsInt>>>.View
                .Select(PaymentEntry, invoice.VendorID, invoice.PayLocationID);
        }

        private APPayment CreateApPayment(IDocumentKey invoice)
        {
            var documentType = invoice.DocType == APDocType.DebitAdj
                ? APDocType.Refund
                : APDocType.Check;
            var payment = new APPayment
            {
                DocType = documentType
            };
            return PaymentEntry.Document.Insert(payment);
        }

        private void InsertAdjustments(IEnumerable<int?> billLineNumbers)
        {
            DisableReferenceNumberAttributeValidation();
            billLineNumbers.ForEach(InsertAdjustment);
        }

        private void DisableReferenceNumberAttributeValidation()
        {
            PaymentEntry.Adjustments.Cache.GetAttributes<APAdjust.adjdRefNbr>()
                .OfType<AdjustmentReferenceNumberSelectorAttribute>()
                .ForEach(attribute => attribute.ValidateValue = false);
        }

        private void InsertAdjustment(int? billLineNumber)
        {
            var adjustment = CreateAdjustment(billLineNumber);
            try
            {
                PaymentEntry.Adjustments.Insert(adjustment);
            }
            catch (PXSetPropertyException)
            {
                throw new AdjustedNotFoundException();
            }
        }

        private APAdjust CreateAdjustment(int? billLineNumber)
        {
            return new APAdjust
            {
                AdjdDocType = InvoiceEntry.Document.Current.DocType,
                AdjdRefNbr = InvoiceEntry.Document.Current.RefNbr,
                AdjdLineNbr = billLineNumber
            };
        }

        private void SavePayment()
        {
            try
            {
                MarkPaymentForLienWaiverGeneration(false);
                PaymentEntry.Persist();
            }
            catch (PXOuterException exception)
            {
                HandlePaymentPersistException(exception);
            }
        }

        private void HandlePaymentPersistException(PXOuterException exception)
        {
            var message = $"{exception.Message} {exception.InnerMessages.First()}";
            InvoiceEntry.Document.Cache.RaiseException<APInvoiceJCExt.amountToPay>(InvoiceEntry.Document.Current,
                message, InvoiceExtension.AmountToPay);
            throw new PXException();
        }

        private void ResetCashDiscount()
        {
            PaymentEntry.Adjustments.SelectMain().ForEach(adj =>
                PaymentEntry.Adjustments.Cache.SetValueExt<APAdjust.curyAdjgPPDAmt>(adj, decimal.Zero));
        }

        private void SetAdjustmentsAmountForVendorCheck(decimal? amountToPay)
        {
            if (IsPaymentsByLinesAllowed())
            {
                SetAdjustmentAmountPerLine(amountToPay);
            }
            else
            {
                SetAdjustmentAmount(amountToPay);
            }
        }

        private void SetAdjustmentAmount(decimal? amountToPay)
        {
            PaymentEntry.Adjustments.Cache.SetValueExt<APAdjust.curyAdjgAmt>(PaymentEntry.Adjustments.Current,
                amountToPay);
        }

        private void SetAdjustmentAmountPerLine(decimal? amountToPay)
        {
            var jointPayees = GetJointPayees(InvoiceJointPayeePayments);
            vendorBalancePerLineCalculationService = new VendorBalancePerLineCalculationService(PaymentEntry, jointPayees);
            foreach (var adjustment in GetSortedAdjustments())
            {
                var adjustmentAmountPerLine = GetAdjustmentAmountPerLine(adjustment);
                adjustment.CuryAdjgAmt = amountToPay == decimal.Zero
                    ? decimal.Zero
                    : Math.Min(adjustmentAmountPerLine.GetValueOrDefault(), amountToPay.GetValueOrDefault());
                amountToPay = amountToPay - adjustment.CuryAdjgAmt;
                PaymentEntry.Adjustments.Cache.Update(adjustment);
            }
        }

        private decimal? GetAdjustmentAmountPerLine(APAdjust adjustment)
        {
            APInvoice invoice = InvoiceDataProvider.GetInvoice(InvoiceEntry, adjustment.AdjdDocType, adjustment.AdjdRefNbr);

            if (invoice.IsRetainageDocument == true)
            {
                var jointPayeePaymentsTotalAmountToPayForLine = InvoiceJointPayeePayments
                    .Where(jpp => jpp.BillLineNumber == adjustment.AdjdLineNbr)
                    .Sum(jpp => jpp.JointAmountToPay);
                var vendorBalancePerLine = vendorBalancePerLineCalculationService.GetVendorBalancePerLine(adjustment);
                var transactionBalance = TransactionDataProvider.GetTransaction(
                    PaymentEntry, adjustment.AdjdDocType, adjustment.AdjdRefNbr, adjustment.AdjdLineNbr).CuryTranBal;
                var minimumJointAmountToPay = transactionBalance - vendorBalancePerLine;
                return jointPayeePaymentsTotalAmountToPayForLine > minimumJointAmountToPay
                    ? transactionBalance - jointPayeePaymentsTotalAmountToPayForLine
                    : vendorBalancePerLine;
            }
            else
            {
                return vendorBalancePerLineCalculationService.GetVendorBalancePerLine(adjustment);
            }
        }

        private IEnumerable<APAdjust> GetSortedAdjustments()
        {
            var billLinesWithJointAmountsToPay = InvoiceJointPayeePayments
                .Where(jpp => jpp.JointAmountToPay.GetValueOrDefault() != 0)
                .Select(x => x.BillLineNumber).Distinct();
            var adjustments = PaymentEntry.Adjustments.SelectMain();
            var sortedAdjustments = billLinesWithJointAmountsToPay
                .Select(billLine => adjustments.FirstOrDefault(x => x.AdjdLineNbr == billLine)).ToList();
            sortedAdjustments.AddRange(adjustments.Where(adjustment => !sortedAdjustments.Contains(adjustment)));
            return sortedAdjustments;
        }

        private void SetAdjustmentsAmountForJointCheck(JointCheckGenerationModel jointCheckGenerationModel)
        {
            foreach (var adjustment in PaymentEntry.Adjustments.SelectMain())
            {
                var jointPayeesAmountToPaySumForLine = jointCheckGenerationModel.InvoiceJointPayeePayments
                    .Where(jpp => jpp.BillLineNumber == adjustment.AdjdLineNbr).Sum(jpp => jpp.JointAmountToPay);
                adjustment.CuryAdjgAmt = jointPayeesAmountToPaySumForLine;
                PaymentEntry.Adjustments.Update(adjustment);
            }
        }

        private void SetPaymentAmount(decimal? amountToPay)
        {
            var payment = PaymentEntry.Document.Current;
            payment.CuryApplAmt = amountToPay;
            payment.CuryOrigDocAmt = amountToPay;
        }
    }
}