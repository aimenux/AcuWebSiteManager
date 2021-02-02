using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using PX.SM;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.GL.Exceptions;
using PX.Objects.TX;
using PX.Common;
using PX.Objects.Common.Abstractions;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APPPDDebitAdjProcess : PXGraph<APPPDDebitAdjProcess>
	{
		public PXCancel<APPPDDebitAdjParameters> Cancel;
		public PXFilter<APPPDDebitAdjParameters> Filter;

		[PXFilterable]
		public PXFilteredProcessing<PendingPPDDebitAdjApp, APPPDDebitAdjParameters> Applications;
		public APSetupNoMigrationMode apsetup;

		public override bool IsDirty => false;

		public PXAction<APPPDDebitAdjParameters> viewPayment;

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewPayment(PXAdapter adapter)
		{
			PendingPPDDebitAdjApp adj = Applications.Current;
			if (adj != null)
			{
				APPayment payment = PXSelect<APPayment, Where< APPayment.refNbr, Equal< Current<PendingPPDDebitAdjApp.payRefNbr>>, 
					And<APPayment.docType, Equal< Current<PendingPPDDebitAdjApp.payDocType>>>>>
						.Select(this).First();
				if (payment != null)
				{
					PXGraph graph = PXGraph.CreateInstance<APPaymentEntry>();
					PXRedirectHelper.TryRedirect(graph, payment, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}

		public PXAction<APPPDDebitAdjParameters> viewInvoice;

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			PendingPPDDebitAdjApp adj = Applications.Current;
			if (adj != null)
			{
				APInvoice invoice = PXSelect<APInvoice, Where<APInvoice.refNbr, Equal<Current<PendingPPDDebitAdjApp.invRefNbr>>,
					And<APInvoice.docType, Equal<Current<PendingPPDDebitAdjApp.invDocType>>>>>
						.Select(this).First();
				if (invoice != null)
				{
					PXGraph graph = PXGraph.CreateInstance<APInvoiceEntry>();
					PXRedirectHelper.TryRedirect(graph, invoice, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}
		#region Cache Attached
		[Vendor]
		protected virtual void PendingPPDDebitAdjApp_VendorID_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APInvoiceType.RefNbr(typeof(Search2<
			Standalone.APRegisterAlias.refNbr,
				InnerJoinSingleTable<APInvoice,
					On<APInvoice.docType, Equal<Standalone.APRegisterAlias.docType>,
					And<APInvoice.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
				InnerJoinSingleTable<Vendor,
					On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<
				Standalone.APRegisterAlias.docType, Equal<Optional<PendingPPDDebitAdjApp.invDocType>>,
				And2<Where<
					Standalone.APRegisterAlias.origModule, Equal<BatchModule.moduleAP>,
					Or<Standalone.APRegisterAlias.released, Equal<True>>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>>))]
		[APInvoiceType.Numbering]
		//[APInvoiceNbr]
		[PXFieldDescription]
		protected virtual void PendingPPDDebitAdjApp_AdjdRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Doc. Date")]
		protected virtual void PendingPPDDebitAdjApp_AdjdDocDate_CacheAttached(PXCache sender) { }

		[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjPPDAmt))]
		[PXUIField(DisplayName = "Cash Discount")]
		protected virtual void PendingPPDDebitAdjApp_CuryAdjdPPDAmt_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Payment Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APPaymentType.RefNbr(typeof(Search2<
			Standalone.APRegisterAlias.refNbr,
				InnerJoinSingleTable<APPayment,
					On<APPayment.docType, Equal<Standalone.APRegisterAlias.docType>,
					And<APPayment.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
				InnerJoinSingleTable<Vendor,
					On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<
				Standalone.APRegisterAlias.docType, Equal<Current<PendingPPDDebitAdjApp.payDocType>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>>))]
		[APPaymentType.Numbering]
		[PXFieldDescription]
		protected virtual void PendingPPDDebitAdjApp_AdjgRefNbr_CacheAttached(PXCache sender) { }

		#endregion

		public APPPDDebitAdjProcess()
		{
			Applications.AllowDelete = true;
			Applications.AllowInsert = false;
			Applications.SetSelected<PendingPPDDebitAdjApp.selected>();
		}

		public virtual IEnumerable applications(PXAdapter adapter)
		{
			APPPDDebitAdjParameters filter = Filter.Current;
			if (filter == null || filter.ApplicationDate == null || filter.BranchID == null) yield break;

			PXSelectBase<PendingPPDDebitAdjApp> select = new PXSelect<PendingPPDDebitAdjApp,
				Where<PendingPPDDebitAdjApp.adjgDocDate, LessEqual<Current<APPPDDebitAdjParameters.applicationDate>>,
					And<PendingPPDDebitAdjApp.adjdBranchID, Equal<Current<APPPDDebitAdjParameters.branchID>>>>>(this);

			if (filter.VendorID != null)
			{
				select.WhereAnd<Where<PendingPPDDebitAdjApp.vendorID, Equal<Current<APPPDDebitAdjParameters.vendorID>>>>();
			}

			foreach (PendingPPDDebitAdjApp res in select.Select())
			{
				yield return res;
			}

			Filter.Cache.IsDirty = false;
		}

		protected virtual void APPPDDebitAdjParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APPPDDebitAdjParameters filter = (APPPDDebitAdjParameters)e.Row;
			if (filter == null) return;

			APSetup setup = apsetup.Current;
			Applications.SetProcessDelegate(list => CreatePPDDebitAdjs(sender, filter, setup, list));

			bool generateOnePerVendor = filter.GenerateOnePerVendor == true;
			PXUIFieldAttribute.SetEnabled<APPPDDebitAdjParameters.debitAdjDate>(sender, filter, generateOnePerVendor);
			PXUIFieldAttribute.SetEnabled<APPPDDebitAdjParameters.finPeriodID>(sender, filter, generateOnePerVendor);
			PXUIFieldAttribute.SetRequired<APPPDDebitAdjParameters.debitAdjDate>(sender, generateOnePerVendor);
			PXUIFieldAttribute.SetRequired<APPPDDebitAdjParameters.finPeriodID>(sender, generateOnePerVendor);
		}

		public virtual void APPPDDebitAdjParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APPPDDebitAdjParameters row = (APPPDDebitAdjParameters)e.Row;
			APPPDDebitAdjParameters oldRow = (APPPDDebitAdjParameters)e.OldRow;
			if (row == null || oldRow == null) return;

			if (!sender.ObjectsEqual<APPPDDebitAdjParameters.applicationDate, APPPDDebitAdjParameters.branchID, APPPDDebitAdjParameters.vendorID>(oldRow, row))
			{
				Applications.Cache.Clear();
				Applications.Cache.ClearQueryCacheObsolete();
			}
		}

		protected virtual void APPPDDebitAdjParameters_GenerateOnePerVendor_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPPDDebitAdjParameters filter = (APPPDDebitAdjParameters)e.Row;
			if (filter == null) return;

			if (filter.GenerateOnePerVendor != true && (bool?)e.OldValue == true)
			{
				filter.DebitAdjDate = null;
				filter.FinPeriodID = null;

				sender.SetValuePending<APPPDDebitAdjParameters.debitAdjDate>(filter, null);
				sender.SetValuePending<APPPDDebitAdjParameters.finPeriodID>(filter, null);
			}
		}

		public static void CreatePPDDebitAdjs(PXCache cache, APPPDDebitAdjParameters filter, APSetup setup, List<PendingPPDDebitAdjApp> docs)
		{
			int i = 0;
			bool failed = false;
			APInvoiceEntry ie = PXGraph.CreateInstance<APInvoiceEntry>();
			ie.APSetup.Current = setup;

			if (filter.GenerateOnePerVendor == true)
			{
				if (filter.DebitAdjDate == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<APPPDDebitAdjParameters.debitAdjDate>(cache));

				if (filter.FinPeriodID == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<APPPDDebitAdjParameters.finPeriodID>(cache));

				Dictionary<PPDApplicationKey, List<PendingPPDDebitAdjApp>> dict = new Dictionary<PPDApplicationKey, List<PendingPPDDebitAdjApp>>();
				foreach (PendingPPDDebitAdjApp pendingPPDDebitAdjApp in docs)
				{
					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(ie, pendingPPDDebitAdjApp.InvCuryInfoID);

					PPDApplicationKey key = new PPDApplicationKey();
					pendingPPDDebitAdjApp.Index = i++;
					key.BranchID = pendingPPDDebitAdjApp.AdjdBranchID;
					key.BAccountID = pendingPPDDebitAdjApp.VendorID;
					key.LocationID = pendingPPDDebitAdjApp.InvVendorLocationID;
					key.CuryID = info.CuryID;
					key.CuryRate = info.CuryRate;
					key.AccountID = pendingPPDDebitAdjApp.AdjdAPAcct;
					key.SubID = pendingPPDDebitAdjApp.AdjdAPSub;
					key.TaxZoneID = pendingPPDDebitAdjApp.InvTaxZoneID;

					List<PendingPPDDebitAdjApp> list;
					if (!dict.TryGetValue(key, out list))
					{
						dict[key] = list = new List<PendingPPDDebitAdjApp>();
					}

					list.Add(pendingPPDDebitAdjApp);
				}

				foreach (List<PendingPPDDebitAdjApp> list in dict.Values)
				{
					APInvoice invoice = CreateAndReleasePPDDebitAdj(ie, filter, list, AutoReleaseDebitAdjustments);

					if (invoice == null)
					{
						failed = true;
					}
				}
			}
			else foreach (PendingPPDDebitAdjApp pendingPPDDebitAdjApp in docs)
				{
					List<PendingPPDDebitAdjApp> list = new List<PendingPPDDebitAdjApp>(1);
					pendingPPDDebitAdjApp.Index = i++;
					list.Add(pendingPPDDebitAdjApp);

					APInvoice invoice = CreateAndReleasePPDDebitAdj(ie, filter, list, AutoReleaseDebitAdjustments);

					if (invoice == null)
					{
						failed = true;
					}
				}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}

		private const bool AutoReleaseDebitAdjustments = true; //TODO: now (26.10.2017) we can't apply nonreleased debit adjustment

		public static APInvoice CreateAndReleasePPDDebitAdj(APInvoiceEntry ie, APPPDDebitAdjParameters filter, List<PendingPPDDebitAdjApp> list, bool autoReleaseDebitAdjustments)
		{
			APInvoice invDebitAdj;

			try
			{
				ie.Clear(PXClearOption.ClearAll);
				PXUIFieldAttribute.SetError(ie.Document.Cache, null, null, null);

				using (var ts = new PXTransactionScope())
				{
					try
					{
						ie.IsPPDCreateContext = true;
						invDebitAdj = ie.CreatePPDDebitAdj(filter, list);
					}
					finally
					{
						ie.IsPPDCreateContext = false;
					}

					if (invDebitAdj != null)
					{
						if (autoReleaseDebitAdjustments == true)
						{
							using (new PXTimeStampScope(null))
							{
								APDocumentRelease.ReleaseDoc(new List<APRegister> {invDebitAdj}, false);
								APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
								APPayment debitAdj = PXSelect<APPayment,
										Where<APPayment.docType, Equal<Required<APPayment.docType>>,
											And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
									.Select(paymentEntry, invDebitAdj.DocType, invDebitAdj.RefNbr)
									.First();
								paymentEntry.Document.Current = debitAdj;
								paymentEntry.SelectTimeStamp();
								CurrencyInfoCache.StoreCached(paymentEntry.currencyinfo,
									CurrencyInfoCache.GetInfo(ie.currencyinfo, ie.Document.Current.CuryInfoID));
								CreatePPDApplications(paymentEntry, list, debitAdj);
								if (debitAdj.AdjFinPeriodID == null)
								{
									throw new FinancialPeriodNotDefinedForDateException(debitAdj.AdjDate);
								}
								paymentEntry.Persist();
								APDocumentRelease.ReleaseDoc(new List<APRegister> {invDebitAdj}, false); // It needs to release applications
							}
						}

						foreach (PendingPPDDebitAdjApp adj in list)
						{
							PXProcessing<PendingPPDDebitAdjApp>.SetInfo(adj.Index, ActionsMessages.RecordProcessed);
						}
					}
					ts.Complete();
				}
			}
			catch (Exception e)
			{
				foreach (PendingPPDDebitAdjApp adj in list)
				{
					PXProcessing<PendingPPDDebitAdjApp>.SetError(adj.Index, e);
				}

				invDebitAdj = null;
			}

			return invDebitAdj;
		}

		protected static void CreatePPDApplications(APPaymentEntry paymentEntry, List<PendingPPDDebitAdjApp> list, APPayment debitAdj)
		{
			foreach (PendingPPDDebitAdjApp doc in list)
			{
				var adj = new APAdjust();
				adj.AdjdDocType = doc.AdjdDocType;
				adj.AdjdRefNbr = doc.AdjdRefNbr;
				adj = paymentEntry.Adjustments_Raw.Insert(adj);

				adj.CuryAdjgAmt = doc.InvCuryDocBal;
				adj = paymentEntry.Adjustments_Raw.Update(adj);

				string refNbr = debitAdj.RefNbr;
				PXUpdate<Set<APAdjust.pPDDebitAdjRefNbr, Required<APAdjust.pPDDebitAdjRefNbr>>, APAdjust,
						Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
							And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
								And<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
									And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
										And<APAdjust.released, Equal<True>,
											And<APAdjust.voided, NotEqual<True>,
												And<APAdjust.pendingPPD, Equal<True>>>>>>>>>
					.Update(paymentEntry, refNbr, doc.AdjdDocType, doc.AdjdRefNbr, doc.AdjgDocType, doc.AdjgRefNbr);
			}
		}

		public static bool CalculateDiscountedTaxes(PXCache cache, APTaxTran aptaxTran, decimal cashDiscPercent)
		{
			bool? result = null;
			object value = null;

			IBqlCreator whereTaxable = (IBqlCreator)Activator.CreateInstance(typeof(WhereAPPPDTaxable<Required<APTaxTran.taxID>>));
			whereTaxable.Verify(cache, aptaxTran, new List<object> { aptaxTran.TaxID }, ref result, ref value);

			aptaxTran.CuryDiscountedTaxableAmt = cashDiscPercent == 0m
				? aptaxTran.CuryTaxableAmt
				: PXDBCurrencyAttribute.RoundCury(cache, aptaxTran,
					(decimal)(aptaxTran.CuryTaxableAmt * (1m - cashDiscPercent)));

			aptaxTran.CuryDiscountedPrice = cashDiscPercent == 0m
				? aptaxTran.CuryTaxAmt
				: PXDBCurrencyAttribute.RoundCury(cache, aptaxTran,
					(decimal)(aptaxTran.TaxRate / 100m * aptaxTran.CuryDiscountedTaxableAmt));

			return result == true;
		}


	}
}