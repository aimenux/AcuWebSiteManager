using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.AR.MigrationMode;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARScheduleRun : ScheduleRunBase<ARScheduleRun, ARScheduleMaint, ARScheduleProcess>
	{
		public ARSetupNoMigrationMode ARSetup;

		protected override bool checkAnyScheduleDetails => false;

		public ARScheduleRun()
		{
			ARSetup setup = ARSetup.Current;

			Schedule_List.Join<
				LeftJoin<ARRegisterAccess,
					On<ARRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
					And<ARRegisterAccess.scheduled, Equal<True>,
					And<Not<Match<ARRegisterAccess, Current<AccessInfo.userName>>>>>>>>();

			Schedule_List.WhereAnd<Where<
				Schedule.module, Equal<BatchModule.moduleAR>,
				And<ARRegisterAccess.docType, IsNull>>>();

			Schedule_List.WhereAnd<Where<Exists<
				Select<ARRegister, 
				Where<ARRegister.scheduleID, Equal<Schedule.scheduleID>, 
					And<ARRegister.scheduled, Equal<True>>>>
			>>>();
		}

		public PXAction<ScheduleRun.Parameters> EditDetail;
		[PXUIField(DisplayName = Messages.ViewSchedule, Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXEditDetailButton]
        public virtual IEnumerable editDetail(PXAdapter adapter) => ViewScheduleAction(adapter);

		public PXAction<ScheduleRun.Parameters> NewSchedule;
		[PXUIField(DisplayName = Messages.NewSchedule, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable newSchedule(PXAdapter adapter)
		{
			ARScheduleMaint graph = CreateInstance<ARScheduleMaint>();

			graph.Schedule_Header.Insert(new Schedule());
			graph.Schedule_Header.Cache.IsDirty = false;

			throw new PXRedirectRequiredException(graph, true, Common.Messages.NewSchedule)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}
	}

	public class ARScheduleProcess : PXGraph<ARScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<ARTran> Tran_Created;
		public PXSelect<CurrencyInfo> CuryInfo_Created;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public virtual void GenerateProc(Schedule schedule)
        {
            GenerateProc(schedule, 1, Accessinfo.BusinessDate.Value);
        }

        public virtual void GenerateProc(Schedule schedule, short times, DateTime runDate)
        {
            IEnumerable<ScheduleDet> occurrences = new Scheduler(this).MakeSchedule(schedule, times, runDate);

			ARInvoiceEntry invoiceEntry = CreateGraph();

			using (PXTransactionScope transactionScope = new PXTransactionScope())
			{
				foreach (ScheduleDet occurrence in occurrences)
				{
					foreach (PXResult<ARInvoice, Customer, CurrencyInfo> scheduledInvoiceResult in PXSelectJoin<
						ARInvoice,
							InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>>,
						Where<
							ARInvoice.scheduleID, Equal<Required<ARInvoice.scheduleID>>,
							And<ARInvoice.scheduled, Equal<True>>>>
						.Select(this, schedule.ScheduleID))
					{
						invoiceEntry.Clear();

						invoiceEntry.customer.Current = scheduledInvoiceResult;
						ARInvoice scheduledInvoice = scheduledInvoiceResult;
						CurrencyInfo scheduledInvoiceCurrencyInfo = scheduledInvoiceResult;

						ARInvoice newInvoice = InsertDocument(
							invoiceEntry, 
							occurrence, 
							scheduledInvoiceResult, 
							scheduledInvoiceResult, 
							scheduledInvoiceCurrencyInfo);

						InsertDetails(invoiceEntry, scheduledInvoice, newInvoice);

						BalanceCalculation.ForceDocumentControlTotals(invoiceEntry, newInvoice);

						try
						{
							invoiceEntry.Save.Press();
						}
						catch
						{
							if (invoiceEntry.Document.Cache.IsInsertedUpdatedDeleted)
							{
								throw;
							}
						}
					}

					schedule.LastRunDate = occurrence.ScheduledDate;
					Running_Schedule.Cache.Update(schedule);
				}

				transactionScope.Complete(this);
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				Running_Schedule.Cache.Persist(PXDBOperation.Update);
				ts.Complete(this);
			}

			Running_Schedule.Cache.Persisted(false);
		}

		protected virtual ARInvoice InsertDocument(
			ARInvoiceEntry invoiceEntry, 
			ScheduleDet occurrence, 
			Customer customer, 
			ARInvoice scheduledInvoice, 
			CurrencyInfo scheduledInvoiceCurrencyInfo)
		{
			if (scheduledInvoice.Released == true)
			{
				throw new PXException(Messages.ScheduledDocumentAlreadyReleased);
			}

			// Cloning currency info is required because we want to preserve 
			// (and not default) the currency rate type of the template document.
			// - 
			CurrencyInfo newCurrencyInfo = PXCache<CurrencyInfo>.CreateCopy(scheduledInvoiceCurrencyInfo);

			newCurrencyInfo.CuryInfoID = null;
			newCurrencyInfo.CuryRate = null;
			newCurrencyInfo.CuryEffDate = occurrence.ScheduledDate;

			newCurrencyInfo = invoiceEntry.currencyinfo.Insert(newCurrencyInfo);

			ARInvoice newInvoice = PXCache<ARInvoice>.CreateCopy(scheduledInvoice);

			newInvoice.CuryInfoID = newCurrencyInfo.CuryInfoID;
			newInvoice.DocDate = occurrence.ScheduledDate;

			FinPeriod finPeriod = 
				FinPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(newInvoice.BranchID), occurrence.ScheduledPeriod)
				.GetValueOrRaiseError();
			newInvoice.FinPeriodID = finPeriod.FinPeriodID;

			newInvoice.TranPeriodID = null;
			newInvoice.DueDate = null;
			newInvoice.DiscDate = null;
			newInvoice.CuryOrigDiscAmt = null;
			newInvoice.OrigDiscAmt = null;
			newInvoice.RefNbr = null;
			newInvoice.Scheduled = false;
			newInvoice.StatementDate = null;
			newInvoice.CuryLineTotal = 0m;
			newInvoice.CuryVatTaxableTotal = 0m;
			newInvoice.CuryVatExemptTotal = 0m;
			newInvoice.NoteID = null;
			newInvoice.IsTaxValid = false;
			newInvoice.IsTaxPosted = false;
			newInvoice.IsTaxSaved = false;
			newInvoice.OrigDocType = scheduledInvoice.DocType;
			newInvoice.OrigRefNbr = scheduledInvoice.RefNbr;

			invoiceEntry.Document.Cache.SetDefaultExt<ARInvoice.printed>(newInvoice);
			invoiceEntry.Document.Cache.SetDefaultExt<ARInvoice.emailed>(newInvoice);

			bool forceClear = false;
			bool clearPaymentMethod = false;

			if (newInvoice.PMInstanceID.HasValue)
			{
				PXResult<CustomerPaymentMethod, PaymentMethod> paymentMethodResult = (PXResult<CustomerPaymentMethod, PaymentMethod>)
					PXSelectJoin<
					CustomerPaymentMethod,
						InnerJoin<PaymentMethod,
							On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>>,
					Where<
						CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
					.Select(invoiceEntry, newInvoice.PMInstanceID);

				if (paymentMethodResult != null)
				{
					CustomerPaymentMethod customerPaymentMethod = paymentMethodResult;
					PaymentMethod paymentMethod = paymentMethodResult;
					if (customerPaymentMethod == null || customerPaymentMethod.IsActive != true || paymentMethod.IsActive != true || paymentMethod.UseForAR != true)
					{
						clearPaymentMethod = true;
						forceClear = true;
					}
				}
				else
				{
					clearPaymentMethod = true;
					forceClear = true;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(newInvoice.PaymentMethodID))
				{
					PaymentMethod paymentMethod = PXSelect<
						PaymentMethod,
						Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>
						.Select(invoiceEntry, newInvoice.PaymentMethodID);

					if (paymentMethod == null || paymentMethod.IsActive != true || paymentMethod.UseForAR != true)
					{
						clearPaymentMethod = true;
						forceClear = true;
					}
				}
			}

			if (clearPaymentMethod)
			{
				newInvoice.PMInstanceID = null;
				newInvoice.PaymentMethodID = null;
				newInvoice.CashAccountID = null;
			}

			invoiceEntry.ClearRetainageSummary(newInvoice);

			newInvoice = invoiceEntry.Document.Insert(newInvoice);

			if (newInvoice.DontApprove != true)
			{
				// We always generate documents on hold
				// if approval process is enabled in AP.
				// -
				newInvoice.Hold = true;
			}

			newInvoice = invoiceEntry.Document.Update(newInvoice);

			// Force credit rule back
			// -
			invoiceEntry.customer.Current = customer;

			if (forceClear == true)
			{
				ARInvoice copy = PXCache<ARInvoice>.CreateCopy(newInvoice);
				copy.PMInstanceID = null;
				copy.PaymentMethodID = null;
				copy.CashAccountID = null;
				newInvoice = invoiceEntry.Document.Update(copy);
			}

			AddressAttribute.CopyRecord<ARInvoice.billAddressID>(invoiceEntry.Document.Cache, newInvoice, scheduledInvoice, false);
			ContactAttribute.CopyRecord<ARInvoice.billContactID>(invoiceEntry.Document.Cache, newInvoice, scheduledInvoice, false);

			CurrencyInfoAttribute.SetEffectiveDate<ARInvoice.docDate>(
				invoiceEntry.Document.Cache,
				new PXFieldUpdatedEventArgs(newInvoice, null, false));

			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(ARInvoice)], scheduledInvoice, invoiceEntry.Document.Cache, newInvoice);

			return newInvoice;
		}

		protected virtual void InsertDetails(ARInvoiceEntry invoiceEntry, ARInvoice scheduledInvoice, ARInvoice newInvoice)
		{
			foreach (ARTran originalLine in PXSelect<
				ARTran,
				Where<
					ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<Where<
						ARTran.lineType, IsNull,
						Or<ARTran.lineType, NotEqual<SOLineType.discount>>>>>>>
				.Select(invoiceEntry, scheduledInvoice.DocType, scheduledInvoice.RefNbr))
			{
				ARTran newLine = PXCache<ARTran>.CreateCopy(originalLine);

				newLine.FinPeriodID = null;
				newLine.TranPeriodID = null;
				newLine.RefNbr = null;
				newLine.CuryInfoID = null;
				newLine.ManualPrice = true;
				newLine.ManualDisc = true;
				newLine.NoteID = null;

				newLine = invoiceEntry.Transactions.Insert(newLine);

				PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(ARTran)], originalLine, invoiceEntry.Transactions.Cache, newLine);
			}

			foreach (ARInvoiceDiscountDetail originalDiscountDetail in PXSelect<
				ARInvoiceDiscountDetail,
				Where<
					ARInvoiceDiscountDetail.docType, Equal<Required<ARInvoiceDiscountDetail.docType>>,
					And<ARInvoiceDiscountDetail.refNbr, Equal<Required<ARInvoiceDiscountDetail.refNbr>>>>>
				.Select(invoiceEntry, scheduledInvoice.DocType, scheduledInvoice.RefNbr))
			{
				ARInvoiceDiscountDetail newDiscountDetail =
					PXCache<ARInvoiceDiscountDetail>.CreateCopy(originalDiscountDetail);

				newDiscountDetail.RefNbr = null;
				newDiscountDetail.CuryInfoID = null;
				newDiscountDetail.IsManual = true;

				DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>().InsertDiscountDetail(invoiceEntry.ARDiscountDetails.Cache, invoiceEntry.ARDiscountDetails, newDiscountDetail);
		}
		}

		public virtual ARInvoiceEntry CreateGraph()
		{
			return PXGraph.CreateInstance<ARInvoiceEntry>();
		}
	}
}