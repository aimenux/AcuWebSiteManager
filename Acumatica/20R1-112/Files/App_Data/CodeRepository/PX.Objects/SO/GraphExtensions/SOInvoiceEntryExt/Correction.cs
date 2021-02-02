using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class Correction : PXGraphExtension<SOInvoiceEntry>
	{
		private bool CancellationInvoiceCreationOnRelease = false;

		public PXAction<ARInvoice> cancelInvoice;
		[PXUIField(DisplayName = Messages.CancelInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CancelInvoice(PXAdapter adapter)
		{
			if (Base.Document.Current == null)
			{
				return adapter.Get();
			}
			Base.Save.Press();

			EnsureCanCancel(Base.Document.Current, false);

			var reverseArgs = new ReverseInvoiceArgs { ApplyToOriginalDocument = true };
			if (this.CancellationInvoiceCreationOnRelease)
			{
				var existingCorrectionInvoiceSet = (PXResult<ARInvoice, CurrencyInfo>)
					PXSelectReadonly2<ARInvoice,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>,
					Where<ARInvoice.origDocType, Equal<Current<ARInvoice.docType>>,
						And<ARInvoice.origRefNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARInvoice.isCorrection, Equal<True>>>>>
					.Select(Base);
				ARInvoice existingCorrectionInvoice = existingCorrectionInvoiceSet;
				CurrencyInfo currencyInfo = existingCorrectionInvoiceSet;

				if (existingCorrectionInvoice == null)
					throw new RowNotFoundException(Base.Document.Cache, Base.Document.Current.DocType, Base.Document.Current.RefNbr);
				reverseArgs.DateOption = ReverseInvoiceArgs.CopyOption.Override;
				reverseArgs.DocumentDate = existingCorrectionInvoice.DocDate;
				reverseArgs.DocumentFinPeriodID = existingCorrectionInvoice.FinPeriodID;
				reverseArgs.CurrencyRateOption = ReverseInvoiceArgs.CopyOption.Override;
				reverseArgs.CurrencyRate = currencyInfo;
				reverseArgs.OverrideDocumentHold = false;
				using (new PXLocaleScope(Base.customer.Current.LocaleName))
				{
					reverseArgs.OverrideDocumentDescr = PXMessages.LocalizeFormatNoPrefixNLA(Messages.CorrectionOfInvoice, Base.Document.Current.RefNbr);
				}
			}

			return Base.ReverseDocumentAndApplyToReversalIfNeeded(adapter, reverseArgs);
		}

		public PXAction<ARInvoice> correctInvoice;
		[PXUIField(DisplayName = Messages.CorrectInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CorrectInvoice(PXAdapter adapter)
		{
			if (Base.Document.Current == null)
			{
				return adapter.Get();
			}
			Base.Save.Press();

			EnsureCanCancel(Base.Document.Current, true);

			return Base.ReverseDocumentAndApplyToReversalIfNeeded(adapter,
				new ReverseInvoiceArgs
				{
					ApplyToOriginalDocument = false,
					PreserveOriginalDocumentSign = true,
					DateOption = ReverseInvoiceArgs.CopyOption.SetDefault,
					CurrencyRateOption = ReverseInvoiceArgs.CopyOption.SetDefault
				});
		}

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			bool canCancelCorrect =
				e.Row?.DocType == ARDocType.Invoice
				&& e.Row.Released == true
				&& e.Row.IsUnderCorrection == false;

			cancelInvoice.SetEnabled(canCancelCorrect);
			correctInvoice.SetEnabled(canCancelCorrect);
		}

		protected virtual void _(Events.RowPersisted<ARInvoice> e)
		{
			bool insert = e.Operation == PXDBOperation.Insert;
			bool delete = e.Operation == PXDBOperation.Delete;
			if ((insert || delete) && e.TranStatus == PXTranStatus.Open
				&& (e.Row.IsCorrection == true || e.Row.IsCancellation == true) && e.Row.OrigDocType != null && e.Row.OrigRefNbr != null
				&& !this.CancellationInvoiceCreationOnRelease)
			{
				bool updatedOriginal = PXDatabase.Update<ARRegister>(
					new PXDataFieldAssign<ARRegister.isUnderCorrection>(PXDbType.Bit, insert),
					new PXDataFieldRestrict<ARRegister.docType>(PXDbType.Char, e.Row.OrigDocType),
					new PXDataFieldRestrict<ARRegister.refNbr>(PXDbType.NVarChar, e.Row.OrigRefNbr),
					new PXDataFieldRestrict<ARRegister.isUnderCorrection>(PXDbType.Bit, !insert));
				if (!updatedOriginal)
				{
					throw new PXLockViolationException(typeof(ARInvoice), PXDBOperation.Update, new[] { e.Row.OrigDocType, e.Row.OrigRefNbr });
				}
			}
		}

		public virtual void EnsureCanCancel(ARInvoice doc, bool isCorrection)
		{
			if (doc.DocType != ARDocType.Invoice)
			{
				throw new PXException(Messages.CantCancelDocType, doc.DocType);
			}

			if (doc.InstallmentCntr > 0)
			{
				throw new PXException(Messages.CantCancelMultipleInstallmentsInvoice);
			}

			var arAdjustGroups = PXSelectGroupBy<ARAdjust,
				Where<ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>, And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>>>,
				Aggregate<
					GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, GroupBy<ARAdjust.released,
					Sum<ARAdjust.curyAdjdAmt>>>>>>
				.SelectMultiBound(Base, new[] { doc })
				.RowCast<ARAdjust>().ToList();

			if (arAdjustGroups.Any(a => a.Released == false))
			{
				throw new PXException(Messages.CantCancelInvoiceWithUnreleasedApplications);
			}

			var nonReversedCreditMemo = arAdjustGroups.FirstOrDefault(a => a.CuryAdjdAmt != 0m && a.AdjgDocType == ARDocType.CreditMemo);
			if (nonReversedCreditMemo != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithCM, nonReversedCreditMemo.AdjdRefNbr, nonReversedCreditMemo.AdjgRefNbr);
			}

			var nonReversedApplication = arAdjustGroups.FirstOrDefault(a => a.CuryAdjdAmt != 0m);
			if (nonReversedApplication != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithPayment, nonReversedApplication.AdjdRefNbr, nonReversedApplication.AdjgRefNbr);
			}

			ARTran directSale = PXSelectReadonly<ARTran,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<ARTran.invtMult, NotEqual<short0>, And<ARTran.lineType, Equal<SOLineType.inventory>>>>>>
				.SelectSingleBound(Base, new[] { doc });
			if (directSale != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithDirectStockSales, doc.RefNbr);
			}

			SOOrderShipment notRequireShipment = PXSelectReadonly<SOOrderShipment,
				Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
					And<SOOrderShipment.shipmentNbr, Equal<Constants.noShipmentNbr>>>>>
				.SelectSingleBound(Base, new[] { doc });
			if (notRequireShipment != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithOrdersNotRequiringShipments, doc.RefNbr);
			}
		}

		[PXOverride]
		public virtual bool AskUserApprovalIfReversingDocumentAlreadyExists(ARInvoice origDoc, Func<ARInvoice, bool> baseImpl)
		{
			if (CancellationInvoiceCreationOnRelease)
				return true;
			return baseImpl(origDoc);
		}

		[PXOverride]
		public virtual ARInvoice CreateReversalARInvoice(ARInvoice doc, ReverseInvoiceArgs reverseArgs, Func<ARInvoice, ReverseInvoiceArgs, ARInvoice> baseMethod)
		{
			var result = baseMethod(doc, reverseArgs);

			if (reverseArgs.PreserveOriginalDocumentSign)
			{
				result.IsCorrection = true;
			}
			else
			{
				result.IsCancellation = true;
				result.DontPrint = true;
				result.DontEmail = true;
			}

			return result;
		}

		[PXOverride]
		public virtual ARTran CreateReversalARTran(ARTran srcTran, ReverseInvoiceArgs reverseArgs, Func<ARTran, ReverseInvoiceArgs, ARTran> baseMethod)
		{
			if (srcTran.LineType == SOLineType.Freight) return null;

			var ret = baseMethod(srcTran, reverseArgs);
			ret.OrigInvoiceType = srcTran.TranType;
			ret.OrigInvoiceNbr = srcTran.RefNbr;
			ret.OrigInvoiceLineNbr = srcTran.LineNbr;

			return ret;
		}

		[PXOverride]
		public virtual void ReverseInvoiceProc(ARRegister doc, ReverseInvoiceArgs reverseArgs, Action<ARRegister, ReverseInvoiceArgs> baseMethod)
		{
			baseMethod(doc, reverseArgs);

			foreach (SOFreightDetail freight in Base.FreightDetails.View.SelectMultiBound(new[] { (ARInvoice)doc }))
			{
				SOFreightDetail newfreight = PXCache<SOFreightDetail>.CreateCopy(freight);

				newfreight.DocType = null;
				newfreight.RefNbr = null;
				newfreight.CuryInfoID = null;
				newfreight.NoteID = null;

				Base.FreightDetails.Insert(newfreight);
			}

			if (Base.IsExternalTax(Base.Document.Current.TaxZoneID))
			{
				ARInvoice orgInvoice = (ARInvoice)doc;
				ARInvoice invoice = Base.Document.Current;

				invoice.CuryDocBal = orgInvoice.CuryDocBal;
				invoice.CuryOrigDocAmt = orgInvoice.CuryOrigDocAmt;
				invoice.CuryTaxTotal = orgInvoice.CuryTaxTotal;
				Base.Document.Update(invoice);
			}
		}

		[PXOverride]
		public virtual ARInvoiceState GetDocumentState(PXCache cache, ARInvoice doc, Func<PXCache, ARInvoice, ARInvoiceState> baseMethod)
		{
			ARInvoiceState state = baseMethod(cache, doc);

			state.IsCancellationDocument = doc.IsCancellation == true;
			state.IsCorrectionDocument = doc.IsCorrection == true;
			state.ShouldDisableHeader |= state.IsCancellationDocument;
			state.IsTaxZoneIDEnabled &= !state.IsCancellationDocument;
			state.IsAvalaraCustomerUsageTypeEnabled &= !state.IsCancellationDocument;
			state.IsAssignmentEnabled &= !state.IsCancellationDocument;
			state.AllowDeleteDocument |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.DocumentHoldEnabled |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.DocumentDateEnabled |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.DocumentDescrEnabled |= state.IsCancellationDocument && !state.IsDocumentReleased;

			state.BalanceBaseCalc |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.AllowDeleteTransactions &= !state.IsCancellationDocument && !state.IsCorrectionDocument;
			state.AllowUpdateTransactions &= !state.IsCancellationDocument;
			state.AllowInsertTransactions &= !state.IsCancellationDocument && !state.IsCorrectionDocument;
			state.AllowDeleteTaxes &= !state.IsCancellationDocument;
			state.AllowUpdateTaxes &= !state.IsCancellationDocument;
			state.AllowInsertTaxes &= !state.IsCancellationDocument;
			state.AllowDeleteDiscounts &= !state.IsCancellationDocument;
			state.AllowUpdateDiscounts &= !state.IsCancellationDocument;
			state.AllowInsertDiscounts &= !state.IsCancellationDocument;

			return state;
		}

		[PXOverride]
		public virtual void ReleaseInvoiceProc(List<ARRegister> list, bool isMassProcess, Action<List<ARRegister>, bool> baseMethod)
		{
			list = list
				.Select((doc, index) => CreateAndReleaseCancellationInvoice(doc, index, isMassProcess) ? doc : null)
				.ToList();

			baseMethod(list, isMassProcess);
		}

		protected virtual bool CreateAndReleaseCancellationInvoice(ARRegister doc, int index, bool isMassProcess)
		{
			if (doc.IsCorrection != true) return true;

			try
			{
				var invoiceGraph = PXGraph.CreateInstance<SOInvoiceEntry>();

				ARInvoice existingCancellationInvoice = PXSelect<ARInvoice,
					Where<ARInvoice.origDocType, Equal<Current<ARInvoice.origDocType>>,
						And<ARInvoice.origRefNbr, Equal<Current<ARInvoice.origRefNbr>>,
						And<ARInvoice.isCancellation, Equal<True>>>>>
					.SelectSingleBound(invoiceGraph, new[] { doc });
				if (existingCancellationInvoice != null)
				{
					if (existingCancellationInvoice.Released != true)
					{
						throw new PXException(Messages.CancellationInvoiceExists, existingCancellationInvoice.RefNbr, doc.RefNbr);
					}
					else
					{
						return true;
					}
				}

				invoiceGraph.GetExtension<Correction>().CancellationInvoiceCreationOnRelease = true;
				invoiceGraph.Document.Current = invoiceGraph.Document.Search<ARInvoice.refNbr>(doc.OrigRefNbr, doc.OrigDocType);
				invoiceGraph.Actions[nameof(cancelInvoice)].Press();

				using (var scope = new PXTransactionScope())
				{
					invoiceGraph.Save.Press();

					invoiceGraph.ReleaseInvoiceProc(
						new List<ARRegister> { invoiceGraph.Document.Current },
						isMassProcess: false);

					scope.Complete();
				}

				return true;
			}
			catch (PXException e) when (isMassProcess)
			{
				PXProcessing<ARRegister>.SetError(index, e);
				return false;
			}
		}
	}
}
