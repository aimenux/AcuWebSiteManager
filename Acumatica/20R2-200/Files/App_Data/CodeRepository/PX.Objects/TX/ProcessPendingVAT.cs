using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.CS;
using PX.Objects.CA;
using System.Collections;
using PX.Data.EP;
using PX.Objects.CR;
using System.Linq;
using PX.Objects.Common.Extensions;
using PX.Objects.TX.Descriptor;
using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;

namespace PX.Objects.TX
{
	[TableAndChartDashboardType]
	public class ProcessSVATBase : PXGraph<ProcessSVATBase>
	{
		public PXCancel<SVATTaxFilter> Cancel;
		public PXFilter<SVATTaxFilter> Filter;
		public PXAction<SVATTaxFilter> viewDocument;

		[PXFilterable]
		public PXFilteredProcessingOrderBy<SVATConversionHistExt, SVATTaxFilter,
			OrderBy<Asc<SVATConversionHistExt.module,
				Asc<SVATConversionHistExt.adjdRefNbr,
				Asc<SVATConversionHistExt.adjdDocType,
				Asc<SVATConversionHistExt.adjdLineNbr,
				Asc<SVATConversionHistExt.adjgRefNbr,
				Asc<SVATConversionHistExt.adjgDocType>>>>>>>> SVATDocuments;

		Dictionary<object, object> _copies = new Dictionary<object, object>();

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public override void Clear()
		{
			Filter.Current.TotalTaxAmount = 0m;
			base.Clear();
		}

		public ProcessSVATBase()
		{
			PXUIFieldAttribute.SetEnabled<SVATConversionHistExt.taxInvoiceDate>(SVATDocuments.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<SVATConversionHistExt.taxInvoiceNbr>(SVATDocuments.Cache, null, true);
		}

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (!string.IsNullOrEmpty(SVATDocuments.Current?.AdjdDocType) &&
				!string.IsNullOrEmpty(SVATDocuments.Current?.AdjdRefNbr))
			{
				PXRedirectHelper.TryRedirect(SVATDocuments.Cache, SVATDocuments.Current, Messages.Document, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}

		public virtual IEnumerable sVATDocuments()
		{
			SVATTaxFilter filter = Filter.Current;
			if (filter != null)
			{
				PXSelectBase<SVATConversionHistExt> sel = new PXSelect<SVATConversionHistExt,
					Where<SVATConversionHistExt.processed, NotEqual<True>>>(this);

				if (filter.ReversalMethod != null)
				{
					switch (filter.ReversalMethod)
					{
						case SVATTaxReversalMethods.OnPayments:
							sel.WhereAnd<Where<SVATConversionHistExt.module, Equal<BatchModule.moduleCA>,
								Or<SVATConversionHistExt.adjdDocType, Equal<ARDocType.cashSale>,
								Or<SVATConversionHistExt.adjdDocType, Equal<ARDocType.cashReturn>,
								Or<SVATConversionHistExt.adjdDocType, Equal<APDocType.quickCheck>,
								Or<SVATConversionHistExt.adjdDocType, Equal<APDocType.voidQuickCheck>,
								Or<Where<SVATConversionHistExt.reversalMethod, Equal<SVATTaxReversalMethods.onPayments>,
									And<Where<SVATConversionHistExt.adjdDocType, NotEqual<SVATConversionHistExt.adjgDocType>,
										Or<SVATConversionHistExt.adjdRefNbr, NotEqual<SVATConversionHistExt.adjgRefNbr>>>>>>>>>>>>();
							break;

						case SVATTaxReversalMethods.OnDocuments:
							sel.WhereAnd<Where<SVATConversionHistExt.reversalMethod, Equal<SVATTaxReversalMethods.onDocuments>,
								And<Where<SVATConversionHistExt.adjdDocType, Equal<SVATConversionHistExt.adjgDocType>,
									And<SVATConversionHistExt.adjdRefNbr, Equal<SVATConversionHistExt.adjgRefNbr>>>>>>();
							break;
					}
				}

				if (filter.TaxAgencyID != null)
				{
					sel.WhereAnd<Where<SVATConversionHistExt.vendorID, Equal<Current<SVATTaxFilter.taxAgencyID>>>>();
				}
				else
				{
					sel.WhereAnd<Where<SVATConversionHistExt.vendorID, IsNull>>();
				}

				if (filter.Date != null)
				{
					sel.WhereAnd<Where<SVATConversionHistExt.adjdDocDate, LessEqual<Current<SVATTaxFilter.date>>>>();
				}

				if (filter.BranchID != null)
				{
					sel.WhereAnd<Where<SVATConversionHistExt.adjdBranchID, Equal<Current<SVATTaxFilter.branchID>>>>();
				}

				if (filter.TaxID != null)
				{
					sel.WhereAnd<Where<SVATConversionHistExt.taxID, Equal<Current<SVATTaxFilter.taxID>>>>();
				}

				FillSVATDocumentsQuery(sel);

				foreach (SVATConversionHistExt hist in sel.Select())
		{
					yield return hist;

					if (_copies.ContainsKey(hist))
					{
						_copies.Remove(hist);
					}
					_copies.Add(hist, PXCache<SVATConversionHistExt>.CreateCopy(hist));
				}

				SVATDocuments.Cache.IsDirty = false;
			}
		}

		public virtual void FillSVATDocumentsQuery(PXSelectBase<SVATConversionHistExt> sel)
		{
		}

		protected virtual void SVATTaxFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SVATTaxFilter filter = (SVATTaxFilter)e.Row;
			if (filter == null) return;

			SVATDocuments.SetProcessDelegate(list => ProcessPendingVATProc(list, filter));
		}

		protected virtual void SVATTaxFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Filter.Current.TotalTaxAmount = 0m;
			SVATDocuments.Cache.Clear();
			SVATDocuments.Cache.ClearQueryCacheObsolete();
		}

		protected virtual void SVATConversionHistExt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SVATTaxFilter filter = Filter.Current;
			SVATConversionHistExt hist = e.Row as SVATConversionHistExt;

			if (filter == null || hist == null)
				return;

			if (string.IsNullOrEmpty(hist.TaxInvoiceNbr))
			{
				switch (hist.DisplayTaxEntryRefNbr)
				{
					case VendorSVATTaxEntryRefNbr.DocumentRefNbr:
						hist.TaxInvoiceNbr = hist.AdjdRefNbr;
						break;

					case VendorSVATTaxEntryRefNbr.PaymentRefNbr:
						hist.TaxInvoiceNbr = hist.AdjgRefNbr;
						break;
				}
		}

			if (hist.TaxInvoiceDate == null)
		{
				hist.TaxInvoiceDate = filter.ReversalMethod == SVATTaxReversalMethods.OnDocuments ? filter.Date : hist.AdjdDocDate;
			}

			PXUIFieldAttribute.SetEnabled<SVATConversionHistExt.taxInvoiceNbr>(sender, null, hist.DisplayTaxEntryRefNbr != VendorSVATTaxEntryRefNbr.TaxInvoiceNbr);
		}

		protected virtual void SVATConversionHistExt_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SVATTaxFilter filter = Filter.Current;
			if (filter != null)
			{
				object OldRow = e.OldRow;
				if (object.ReferenceEquals(e.Row, e.OldRow) && !_copies.TryGetValue(e.Row, out OldRow))
				{
					decimal? val = 0m;
					foreach (SVATConversionHistExt res in SVATDocuments.Select())
					{
						if (res.Selected == true)
						{
							val += res.TaxAmt ?? 0m;
						}
					}

					filter.TotalTaxAmount = val;
				}
				else
				{
					SVATConversionHistExt old_row = OldRow as SVATConversionHistExt;
					SVATConversionHistExt new_row = e.Row as SVATConversionHistExt;

					filter.TotalTaxAmount -= old_row?.Selected == true ? old_row?.TaxAmt : 0m;
					filter.TotalTaxAmount += new_row?.Selected == true ? new_row?.TaxAmt : 0m;
				}
			}
		}

		public static void ProcessPendingVATProc(List<SVATConversionHistExt> list, SVATTaxFilter filter)
		{
			ProcessSVATBase svat = PXGraph.CreateInstance<ProcessSVATBase>();
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			je.Mode |= JournalEntry.Modes.RecognizingVAT;

			PXCache dummycache = je.Caches[typeof(TaxTran)];
			dummycache = je.Caches[typeof(SVATTaxTran)];
			je.Views.Caches.Add(typeof(SVATTaxTran));

			int index = 0;
			bool failed = false;
			Dictionary<AdjKey, string> taxInvoiceNbrs = new Dictionary<AdjKey, string>();
			Dictionary<string, Tuple<Batch, int>> batches = new Dictionary<string, Tuple<Batch, int>>();

			foreach (SVATConversionHistExt histSVAT in list)
			{
				AdjKey adjKey = new AdjKey
				{
					Module = histSVAT.Module,
					AdjdDocType = histSVAT.AdjdDocType,
					AdjdRefNbr = histSVAT.AdjdRefNbr,
					AdjdLineNbr = histSVAT.AdjdLineNbr ?? 0,
					AdjgDocType = histSVAT.AdjgDocType,
					AdjgRefNbr = histSVAT.AdjgRefNbr,
					AdjNbr = histSVAT.AdjNbr ?? 0
				};

				try
				{
					je.Clear(PXClearOption.ClearAll);

					using (PXTransactionScope ts = new PXTransactionScope())
					{
						PXProcessing<SVATConversionHistExt>.SetCurrentItem(histSVAT);

						Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID,
							Equal<Required<Vendor.bAccountID>>>>.SelectSingleBound(svat, null, histSVAT.VendorID);

						string TaxInvoiceNbr;
						if (taxInvoiceNbrs.TryGetValue(adjKey, out TaxInvoiceNbr))
						{
							histSVAT.TaxInvoiceNbr = TaxInvoiceNbr;
						}
						else
						{
							if (histSVAT.TaxType == TaxType.PendingSales &&
								vendor?.SVATOutputTaxEntryRefNbr == VendorSVATTaxEntryRefNbr.TaxInvoiceNbr)
							{
								histSVAT.TaxInvoiceNbr = AutoNumberAttribute.GetNextNumber(svat.SVATDocuments.Cache, null,
									vendor.SVATTaxInvoiceNumberingID, svat.Accessinfo.BusinessDate);
							}
							taxInvoiceNbrs.Add(adjKey, histSVAT.TaxInvoiceNbr);
						}

						if (string.IsNullOrEmpty(histSVAT.TaxInvoiceNbr) == true || histSVAT.TaxInvoiceDate == null)
						{
							PXProcessing<SVATConversionHistExt>.SetError(Messages.CannotProcessW);
							failed = true;
							index++;
							continue;
						}

						TaxTran prev_taxtran = null;
						FinPeriod finPeriod = svat.FinPeriodRepository.GetFinPeriodByDate(histSVAT.TaxInvoiceDate, PXAccess.GetParentOrganizationID(histSVAT.AdjdBranchID));

						string masterInvoiceNbr = string.Empty;
						if (histSVAT.Module == BatchModule.AR)
						{
							var invoiceSelect = new PXSelect<ARInvoice,
								Where<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>(je);
							ARInvoice arDoc = invoiceSelect.Select(histSVAT.AdjdRefNbr);
							ARInvoice masterInvoice = invoiceSelect.Select(arDoc?.MasterRefNbr);
							masterInvoiceNbr = masterInvoice?.RefNbr;
						}
						else if (histSVAT.Module == BatchModule.AP)
						{
							var invoiceSelect = new PXSelect<APInvoice,
								Where<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>(je);
							APInvoice apDoc = invoiceSelect.Select(histSVAT.AdjdRefNbr);
							APInvoice masterInvoice = invoiceSelect.Select(apDoc?.MasterRefNbr);
							masterInvoiceNbr = masterInvoice?.RefNbr;
						}

						var taxTranSelection = PXSelectJoin<SVATTaxTran,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SVATTaxTran.curyInfoID>>,
							InnerJoin<Tax, On<Tax.taxID, Equal<SVATTaxTran.taxID>>>>,
							Where<SVATTaxTran.module, Equal<Current<SVATConversionHistExt.module>>,
								And2<Where<SVATTaxTran.vendorID, Equal<Current<SVATConversionHistExt.vendorID>>,
									Or<SVATTaxTran.vendorID, IsNull,
									And<Current<SVATConversionHistExt.vendorID>, IsNull>>>,
								And<SVATTaxTran.tranType, Equal<Current<SVATConversionHistExt.adjdDocType>>,
								And2<Where<SVATTaxTran.refNbr, Equal<Current<SVATConversionHistExt.adjdRefNbr>>, Or<SVATTaxTran.refNbr, Equal<Required<SVATTaxTran.refNbr>>>>,
								And<SVATTaxTran.taxID, Equal<Current<SVATConversionHistExt.taxID>>>>>>>>
							.SelectSingleBound(je, new object[] { histSVAT }, masterInvoiceNbr).AsEnumerable();

						try
						{
							svat.FinPeriodUtils.ValidateFinPeriod<SVATTaxTran>(taxTranSelection.RowCast<SVATTaxTran>(),
								m => svat.FinPeriodRepository.GetFinPeriodByDate(histSVAT.TaxInvoiceDate, PXAccess.GetParentOrganizationID(m.BranchID)).FinPeriodID,
								m => m.BranchID.SingleToArray());
						}
						catch(PXException e)
						{
							PXProcessing<SVATConversionHistExt>.SetError(e);
							failed = true;
							index++;
							continue;
						}
						

						foreach (PXResult<SVATTaxTran, CurrencyInfo, Tax> res in taxTranSelection)
						{
							SVATTaxTran taxtran = res;
							CurrencyInfo info = res;
							Tax tax = res;

							je.SegregateBatch(BatchModule.GL, taxtran.BranchID, info.CuryID, histSVAT.TaxInvoiceDate,
								finPeriod.FinPeriodID, null, info, null);

							taxtran.TaxInvoiceNbr = histSVAT.TaxInvoiceNbr;
							taxtran.TaxInvoiceDate = histSVAT.TaxInvoiceDate;

							PXCache taxtranCache = je.Caches[typeof(SVATTaxTran)];
							taxtranCache.Update(taxtran);

							CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
							new_info.CuryInfoID = null;
							new_info.ModuleCode = BatchModule.GL;
							new_info.BaseCalc = false;
							new_info = je.currencyinfo.Insert(new_info) ?? new_info;

							bool drCr = (ReportTaxProcess.GetMult(taxtran) == 1m);
							decimal tranMult = ReportTaxProcess.GetMultByTranType(taxtran.Module, taxtran.TranType);

							decimal curyTaxableAmt = (histSVAT.CuryTaxableAmt ?? 0m) * tranMult;
							decimal taxableAmt = (histSVAT.TaxableAmt ?? 0m) * tranMult;
							decimal curyTaxAmt = (histSVAT.CuryTaxAmt ?? 0m) * tranMult;
							decimal taxAmt = (histSVAT.TaxAmt ?? 0m) * tranMult;

							#region reverse original transaction
							{
								GLTran tran = new GLTran();
								tran.BranchID = taxtran.BranchID;
								tran.AccountID = taxtran.AccountID;
								tran.SubID = taxtran.SubID;
								tran.CuryDebitAmt = drCr ? curyTaxAmt : 0m;
								tran.DebitAmt = drCr ? taxAmt : 0m;
								tran.CuryCreditAmt = drCr ? 0m : curyTaxAmt;
								tran.CreditAmt = drCr ? 0m : taxAmt;
								tran.TranType = taxtran.TranType;
								tran.TranClass = GLTran.tranClass.Normal;
								tran.RefNbr = taxtran.RefNbr;
								tran.TranDesc = taxtran.TaxInvoiceNbr;
								tran.TranPeriodID = finPeriod.FinPeriodID;
								tran.FinPeriodID = finPeriod.FinPeriodID;
								tran.TranDate = taxtran.TaxInvoiceDate;
								tran.CuryInfoID = new_info.CuryInfoID;
								tran.Released = true;

								je.GLTranModuleBatNbr.Insert(tran);

								SVATTaxTran newtaxtran = PXCache<SVATTaxTran>.CreateCopy(taxtran);
								newtaxtran.RecordID = null;
								newtaxtran.Module = BatchModule.GL;
								newtaxtran.TranType = (taxtran.TaxType == TaxType.PendingSales) ? TaxAdjustmentType.ReverseOutputVAT : TaxAdjustmentType.ReverseInputVAT;
								newtaxtran.RefNbr = newtaxtran.TaxInvoiceNbr;
								newtaxtran.TranDate = newtaxtran.TaxInvoiceDate;
								newtaxtran.FinPeriodID = finPeriod.FinPeriodID;
								newtaxtran.FinDate = null;
								

								decimal tranSign = (-1m) * ReportTaxProcess.GetMult(taxtran) * ReportTaxProcess.GetMult(newtaxtran);
								newtaxtran.CuryTaxableAmt = tranSign * curyTaxableAmt;
								newtaxtran.TaxableAmt = tranSign * taxableAmt;
								newtaxtran.CuryTaxAmt = tranSign * curyTaxAmt;
								newtaxtran.TaxAmt = tranSign * taxAmt;

								taxtranCache.Insert(newtaxtran);
							}
							#endregion

							#region reclassify to VAT account
							{
								GLTran tran = new GLTran();
								tran.BranchID = taxtran.BranchID;
								tran.AccountID = (taxtran.TaxType == TaxType.PendingSales) ? tax.SalesTaxAcctID : tax.PurchTaxAcctID;
								tran.SubID = (taxtran.TaxType == TaxType.PendingSales) ? tax.SalesTaxSubID : tax.PurchTaxSubID;
								tran.CuryDebitAmt = drCr ? 0m : curyTaxAmt;
								tran.DebitAmt = drCr ? 0m : taxAmt;
								tran.CuryCreditAmt = drCr ? curyTaxAmt : 0m;
								tran.CreditAmt = drCr ? taxAmt : 0m;
								tran.TranType = taxtran.TranType;
								tran.TranClass = GLTran.tranClass.Normal;
								tran.RefNbr = taxtran.RefNbr;
								tran.TranDesc = taxtran.TaxInvoiceNbr;
								tran.TranPeriodID = finPeriod.FinPeriodID;
								tran.FinPeriodID = finPeriod.FinPeriodID;
								tran.TranDate = taxtran.TaxInvoiceDate;
								tran.CuryInfoID = new_info.CuryInfoID;
								tran.Released = true;

								je.GLTranModuleBatNbr.Insert(tran);

								SVATTaxTran newtaxtran = PXCache<SVATTaxTran>.CreateCopy(taxtran);
								newtaxtran.RecordID = null;
								newtaxtran.Module = BatchModule.GL;
								newtaxtran.TranType = (taxtran.TaxType == TaxType.PendingSales) ? TaxAdjustmentType.OutputVAT : TaxAdjustmentType.InputVAT;
								newtaxtran.TaxType = (taxtran.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase;
								newtaxtran.AccountID = (taxtran.TaxType == TaxType.PendingSales) ? tax.SalesTaxAcctID : tax.PurchTaxAcctID;
								newtaxtran.SubID = (taxtran.TaxType == TaxType.PendingSales) ? tax.SalesTaxSubID : tax.PurchTaxSubID;
								newtaxtran.RefNbr = newtaxtran.TaxInvoiceNbr;
								newtaxtran.TranDate = newtaxtran.TaxInvoiceDate;
								newtaxtran.FinPeriodID = finPeriod.FinPeriodID;
								newtaxtran.FinDate = null;

								decimal tranSign = ReportTaxProcess.GetMult(taxtran) * ReportTaxProcess.GetMult(newtaxtran);
								newtaxtran.CuryTaxableAmt = tranSign * curyTaxableAmt;
								newtaxtran.TaxableAmt = tranSign * taxableAmt;
								newtaxtran.CuryTaxAmt = tranSign * curyTaxAmt;
								newtaxtran.TaxAmt = tranSign * taxAmt;

								prev_taxtran = (TaxTran)taxtranCache.Insert(newtaxtran);
							}
							#endregion
						}

						if (histSVAT.ReversalMethod == SVATTaxReversalMethods.OnPayments)
						{
							SVATConversionHist docSVAT = PXSelect<SVATConversionHist,
								Where<SVATConversionHist.module, Equal<Current<SVATConversionHist.module>>,
									And<SVATConversionHist.adjdDocType, Equal<Current<SVATConversionHist.adjdDocType>>,
									And<SVATConversionHist.adjdRefNbr, Equal<Current<SVATConversionHist.adjdRefNbr>>,
									And<SVATConversionHist.adjgDocType, Equal<SVATConversionHist.adjdDocType>,
									And<SVATConversionHist.adjgRefNbr, Equal<SVATConversionHist.adjdRefNbr>,
									And<SVATConversionHist.adjNbr, Equal<decimal_1>,
									And<SVATConversionHist.taxID, Equal<Current<SVATConversionHist.taxID>>>>>>>>>>
								.SelectSingleBound(svat, new object[] { histSVAT });

							docSVAT.CuryUnrecognizedTaxAmt -= histSVAT.CuryTaxAmt;
							docSVAT.UnrecognizedTaxAmt -= histSVAT.TaxAmt;

							if (docSVAT.CuryUnrecognizedTaxAmt == 0m)
							{
								docSVAT.Processed = true;
								docSVAT.AdjgFinPeriodID = finPeriod.FinPeriodID;
							}

							svat.SVATDocuments.Cache.Update(docSVAT);
						}

						je.Save.Press();

						histSVAT.Processed = true;
						histSVAT.CuryUnrecognizedTaxAmt = 0m;
						histSVAT.UnrecognizedTaxAmt = 0m;
						histSVAT.TaxRecordID = prev_taxtran?.RecordID;
						histSVAT.AdjBatchNbr = je.BatchModule.Current?.BatchNbr;
						histSVAT.AdjgFinPeriodID = finPeriod.FinPeriodID;
						svat.SVATDocuments.Cache.Update(histSVAT);

						svat.Persist();
						ts.Complete();
						
						if (je.BatchModule.Current != null && !batches.ContainsKey(histSVAT.AdjBatchNbr))
						{
							batches.Add(histSVAT.AdjBatchNbr, new Tuple<Batch, int>(je.BatchModule.Current, index));
						}
					}

					PXProcessing<SVATConversionHistExt>.SetInfo(index, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					PXProcessing<SVATConversionHistExt>.SetError(index, e);
					failed = true;
					taxInvoiceNbrs.Remove(adjKey);
				}

				index++;
			}

			if (je.glsetup.Current.AutoPostOption == true)
			{
				PostGraph pg = PXGraph.CreateInstance<PostGraph>();

				foreach (Tuple<Batch, int> tuple in batches.Values)
				{
					try
					{
						pg.Clear();
						pg.PostBatchProc(tuple.Item1);
					}						
					catch (Exception e)
					{
						PXProcessing<SVATConversionHistExt>.SetError(tuple.Item2, e);
						failed = true;
					}
				}
			}

			if (filter.ReversalMethod == SVATTaxReversalMethods.OnPayments)
			{
				foreach (AdjKey key in taxInvoiceNbrs.Keys)
				{
					string taxInvoiceNbr = taxInvoiceNbrs[key];
					switch (key.Module)
					{
						case BatchModule.AP:
							PXUpdate<Set<APAdjust.taxInvoiceNbr, Required<APAdjust.taxInvoiceNbr>>, APAdjust,
							Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
								And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
								And<APAdjust.adjdLineNbr, Equal<Required<APAdjust.adjdLineNbr>>,
								And<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
								And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
								And<APAdjust.adjNbr, Equal<Required<APAdjust.adjNbr>>>>>>>>>
								.Update(svat, taxInvoiceNbr, key.AdjdDocType, key.AdjdRefNbr, key.AdjdLineNbr, key.AdjgDocType, key.AdjgRefNbr, key.AdjNbr);
							break;

						case BatchModule.AR:
							PXUpdate<Set<ARAdjust.taxInvoiceNbr, Required<ARAdjust.taxInvoiceNbr>>, ARAdjust,
							Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
								And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
								And<ARAdjust.adjdLineNbr, Equal<Required<ARAdjust.adjdLineNbr>>,
								And<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
								And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
								And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>>>
								.Update(svat, taxInvoiceNbr, key.AdjdDocType, key.AdjdRefNbr, key.AdjdLineNbr, key.AdjgDocType, key.AdjgRefNbr, key.AdjNbr);
							break;
					}
				}
			}

			if (failed)
			{
				PXProcessing<APPayment>.SetCurrentItem(null);
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}
	}

	public class ProcessOutputSVAT : ProcessSVATBase
	{
		public ProcessOutputSVAT() { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Customer", Visible = false)]
		protected virtual void SVATConversionHistExt_DisplayCounterPartyID_CacheAttached(PXCache sender) { }

		public override void FillSVATDocumentsQuery(PXSelectBase<SVATConversionHistExt> sel)
		{
			sel.WhereAnd<Where<SVATConversionHistExt.taxType, Equal<TaxType.pendingSales>>>();
		}
	}

	public class ProcessInputSVAT : ProcessSVATBase
	{
		public ProcessInputSVAT()
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Vendor", Visible = false)]
		protected virtual void SVATConversionHistExt_DisplayCounterPartyID_CacheAttached(PXCache sender)
		{
		}

		public override void FillSVATDocumentsQuery(PXSelectBase<SVATConversionHistExt> sel)
		{
			sel.WhereAnd<Where<SVATConversionHistExt.taxType, Equal<TaxType.pendingPurchase>>>();
		}
	}

	public struct AdjKey
	{
		public string Module;
		public string AdjdDocType;
		public string AdjdRefNbr;
		public int AdjdLineNbr;
		public string AdjgDocType;
		public string AdjgRefNbr;
		public int AdjNbr;
	}

    [Serializable]
	public partial class SVATTaxFilter : IBqlTable
	{
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(Required = false)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region TaxAgencyID
		public abstract class taxAgencyID : PX.Data.BQL.BqlInt.Field<taxAgencyID> { }

		[TaxAgencyActive]
		public virtual int? TaxAgencyID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(Search<Tax.taxID, Where<Tax.isExternal, NotEqual<True>>>), DescriptionField = typeof(Tax.descr))]
		public virtual string TaxID
		{
			get;
			set;
		}
		#endregion
		#region ReversalMethod
		public abstract class reversalMethod : PX.Data.BQL.BqlString.Field<reversalMethod> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SVATTaxReversalMethods.OnDocuments)]
		[PXFormula(typeof(IsNull<Selector<SVATTaxFilter.taxAgencyID, Vendor.sVATReversalMethod>, SVATTaxReversalMethods.onDocuments>))]
		[SVATTaxReversalMethods.List]
		[PXUIField(DisplayName = "VAT Recognition Method")]
		public virtual string ReversalMethod
		{
			get;
			set;
		}
		#endregion
		#region TotalTaxAmount
		public abstract class totalTaxAmount : PX.Data.BQL.BqlDecimal.Field<totalTaxAmount> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Tax Amount", Enabled = false)]
		public virtual decimal? TotalTaxAmount
			{
			get;
			set;
			}
		#endregion
	}

	[Serializable]
	[PXHidden]
	public partial class SVATTaxTran : TaxTran
	{
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault]
		public override string Module
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		public override string TranType
			{
			get;
			set;
			}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public override string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		#region TaxPeriodID
		public new abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		[FinPeriodID]
		public override string TaxPeriodID
			{
			get;
			set;
		}
		#endregion
        #region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
	    [FinPeriodID(branchSourceType: typeof(SVATTaxTran.branchID))]
		[PXDefault]
	    public override string FinPeriodID { get; set; }
        #endregion
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public override string TaxID
			{
			get;
			set;
		}
		#endregion
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt]
		public override int? VendorID
			{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		public override string TaxZoneID
			{
			get;
			set;
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[PXDBInt]
		[PXDefault]
		public override int? AccountID
			{
			get;
			set;
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[PXDBInt]
		[PXDefault]
		public override int? SubID
			{
			get;
			set;
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate]
		[PXDefault]
		public override DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region TaxType
		public new abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		public override string TaxType
		{
			get;
			set;
		}
		#endregion
		#region TaxBucketID
		public new abstract class taxBucketID : PX.Data.BQL.BqlInt.Field<taxBucketID> { }
		[PXDBInt]
		[PXDefault]
		public override int? TaxBucketID
		{
			get;
			set;
		}
		#endregion
		#region TaxInvoiceNbr
		public new abstract class taxInvoiceNbr : PX.Data.BQL.BqlString.Field<taxInvoiceNbr> { }
		#endregion
		#region TaxInvoiceDate
		public new abstract class taxInvoiceDate : PX.Data.BQL.BqlDateTime.Field<taxInvoiceDate> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[PXDefault]
		public override long? CuryInfoID
			{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public new abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? CuryTaxableAmt
			{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public new abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		#endregion
		#region CuryTaxAmt
		public new abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxAmt
		public new abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		#endregion
		#region CuryTaxAmtSumm
		public new abstract class curyTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtSumm> { }
		[PXDBCurrency(typeof(SVATTaxTran.curyInfoID), typeof(SVATTaxTran.taxAmtSumm))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? CuryTaxAmtSumm { get; set; }
		#endregion
		#region TaxAmtSumm
		public new abstract class taxAmtSumm : PX.Data.BQL.BqlDecimal.Field<taxAmtSumm> { }
		#endregion
	}

	[Serializable]
	[PXPrimaryGraph(new Type[]
	{
		typeof(CATranEntry),
		typeof(APQuickCheckEntry),
		typeof(APInvoiceEntry),
		typeof(ARCashSaleEntry),
		typeof(ARInvoiceEntry)
	},
	new Type[]
		{
		typeof(Select<CAAdj, Where<Current<SVATConversionHistExt.module>, Equal<BatchModule.moduleCA>,
			And<CAAdj.adjTranType, Equal<Current<SVATConversionHistExt.adjdDocType>>,
			And<CAAdj.adjRefNbr, Equal<Current<SVATConversionHistExt.adjdRefNbr>>>>>>),
		typeof(Select<APQuickCheck, Where<Current<SVATConversionHistExt.module>, Equal<BatchModule.moduleAP>,
			And<APQuickCheck.docType, Equal<Current<SVATConversionHistExt.adjdDocType>>,
			And<APQuickCheck.refNbr, Equal<Current<SVATConversionHistExt.adjdRefNbr>>>>>>),
		typeof(Select<APInvoice, Where<Current<SVATConversionHistExt.module>, Equal<BatchModule.moduleAP>,
			And<APInvoice.docType, Equal<Current<SVATConversionHistExt.adjdDocType>>,
			And<APInvoice.refNbr, Equal<Current<SVATConversionHistExt.adjdRefNbr>>>>>>),
		typeof(Select<ARCashSale, Where<Current<SVATConversionHistExt.module>, Equal<BatchModule.moduleAR>,
			And<ARCashSale.docType, Equal<Current<SVATConversionHistExt.adjdDocType>>,
			And<ARCashSale.refNbr, Equal<Current<SVATConversionHistExt.adjdRefNbr>>>>>>),
		typeof(Select<ARInvoice, Where<Current<SVATConversionHistExt.module>, Equal<BatchModule.moduleAR>,
			And<ARInvoice.docType, Equal<Current<SVATConversionHistExt.adjdDocType>>,
			And<ARInvoice.refNbr, Equal<Current<SVATConversionHistExt.adjdRefNbr>>>>>>)
	})]
	[PXProjection(typeof(Select2<SVATConversionHist, 
		LeftJoin<Vendor, On<Vendor.bAccountID, Equal<SVATConversionHist.vendorID>>,
		LeftJoin<APInvoice, On<SVATConversionHist.module, Equal<BatchModule.moduleAP>,
			And<SVATConversionHist.adjdDocType, Equal<APInvoice.docType>,
			And<SVATConversionHist.adjdRefNbr, Equal<APInvoice.refNbr>>>>,
		LeftJoin<ARInvoice, On<SVATConversionHist.module, Equal<BatchModule.moduleAR>, 
			And<SVATConversionHist.adjdDocType, Equal<ARInvoice.docType>,
			And<SVATConversionHist.adjdRefNbr, Equal<ARInvoice.refNbr>>>>,
		LeftJoin<CAAdj, On<SVATConversionHist.module, Equal<BatchModule.moduleCA>, 
			And<SVATConversionHist.adjdDocType, Equal<CAAdj.adjTranType>,
			And<SVATConversionHist.adjdRefNbr, Equal<CAAdj.adjRefNbr>>>>>>>>>), Persistent = true)]
	public partial class SVATConversionHistExt : SVATConversionHist
		{
		#region SVATConversionHist keys

		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SVATConversionHist.module))]
		[PXDefault]
		[PXUIField(DisplayName = "Module")]
		[BatchModule.List]
		[PXFieldDescription]
		public override string Module
		{
			get;
			set;
		}
		#endregion
		#region AdjdDocType
		public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(SVATConversionHist.adjdDocType))]
		[PXDefault]
		[PXUIField(DisplayName = "Type")]
		public override string AdjdDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjdRefNbr
		public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(SVATConversionHist.adjdRefNbr))]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public override string AdjdRefNbr
			{
			get;
			set;
		}
		#endregion
		#region AdjdLineNbr
		public new abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }

		[PXDBInt(IsKey = true, BqlField = typeof(SVATConversionHist.adjdLineNbr))]
		[PXUIField(DisplayName = "Line Nbr.")]
		[PXDefault(0)]
		public override int? AdjdLineNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjgDocType
		public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(SVATConversionHist.adjgDocType))]
		[PXDefault(typeof(SVATConversionHist.adjdDocType))]
		[PXUIField(DisplayName = "AdjgDocType")]
		public override string AdjgDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjgRefNbr
		public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(SVATConversionHist.adjgRefNbr))]
		[PXDefault(typeof(SVATConversionHist.adjdRefNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr")]
		public override string AdjgRefNbr
			{
			get;
			set;
		}
		#endregion
		#region AdjNbr
		public new abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		[PXDBInt(IsKey = true, BqlField = typeof(SVATConversionHist.adjNbr))]
		[PXDefault(-1)]
		[PXUIField(DisplayName = "Adjustment Nbr.")]
		public override int? AdjNbr
		{
			get;
			set;
		}
		#endregion
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true, BqlField = typeof(SVATConversionHist.taxID))]
		[PXUIField(DisplayName = "Tax ID")]
		public override string TaxID
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXString(1, IsFixed = true)]
		[PXDBCalced(typeof(Switch<Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleCA>>, CAAdj.status,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAP>>, APInvoice.status,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAR>>, ARInvoice.status>>>>), typeof(string))]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region DisplayStatus
		public abstract class displayStatus : PX.Data.BQL.BqlString.Field<displayStatus> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Status")]
		[SVATHistStatus.List]
		public virtual string DisplayStatus
		{
			[PXDependsOnFields(typeof(SVATConversionHistExt.module),
				typeof(SVATConversionHistExt.status))]
			get
		{
				return this.Module + this.Status;
		}
			set
		{
			}
		}
		#endregion
		#region DisplayDocType
		public abstract class displayDocType : PX.Data.BQL.BqlString.Field<displayDocType> { }
		[PXString(5, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[SVATHistDocType.List]
		public virtual string DisplayDocType
		{
			[PXDependsOnFields(typeof(SVATConversionHistExt.module), 
				typeof(SVATConversionHistExt.adjdDocType))]
			get
		{
				return this.Module + this.AdjdDocType;
		}
			set
		{
			}
		}
		#endregion
		#region DisplayCounterPartyID
		public abstract class displayCounterPartyID : PX.Data.BQL.BqlInt.Field<displayCounterPartyID> { }
		[PXInt]
		[PXUIField(DisplayName = "Customer/Vendor", Visible = false)]
		[PXDBCalced(typeof(Switch<Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAP>>, APInvoice.vendorID,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAR>>, ARInvoice.customerID>>>), typeof(int))]
		[PXSelector(typeof(Search<BAccountR.bAccountID, Where<BAccountR.bAccountID, Equal<Current<SVATConversionHistExt.displayCounterPartyID>>>>),
			DescriptionField = typeof(BAccountR.acctName), SubstituteKey = typeof(BAccountR.acctCD))]
		public virtual int? DisplayCounterPartyID
		{
			get;
			set;
		}
		#endregion
		#region DisplayDescription
		public abstract class displayDescription : PX.Data.BQL.BqlString.Field<displayDescription> { }
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visible = false)]
		[PXDBCalced(typeof(Switch<Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleCA>>, CAAdj.tranDesc,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAP>>, APInvoice.docDesc,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAR>>, ARInvoice.docDesc>>>>), typeof(string))]
		public virtual string DisplayDescription
		{
			get;
			set;
		}
		#endregion
		#region DisplayDocRef
		public abstract class displayDocRef : PX.Data.BQL.BqlString.Field<displayDocRef> { }
		[PXString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Ref. / Customer Order Nbr.", Visible = false)]
		[PXDBCalced(typeof(Switch<Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleCA>>, CAAdj.extRefNbr,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAP>>, APInvoice.invoiceNbr,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAR>>, ARInvoice.invoiceNbr>>>>), typeof(string))]
		public virtual string DisplayDocRef
		{
			get;
			set;
		}
		#endregion
		#region DisplayTaxEntryRefNbr
		public abstract class displayTaxEntryRefNbr : PX.Data.BQL.BqlString.Field<displayTaxEntryRefNbr> { }
		[PXString(1, IsFixed = true)]
		[PXDBCalced(typeof(IsNull<
			Switch<Case<Where<SVATConversionHist.taxType, Equal<TaxType.pendingPurchase>>, Vendor.sVATInputTaxEntryRefNbr,
				Case<Where<SVATConversionHist.taxType, Equal<TaxType.pendingSales>>, Vendor.sVATOutputTaxEntryRefNbr>>>, 
			VendorSVATTaxEntryRefNbr.manuallyEntered>), typeof(string))]
		public virtual string DisplayTaxEntryRefNbr
		{
			get;
			set;
		}
		#endregion
		#region DisplayOrigDocAmt
		public abstract class displayOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<displayOrigDocAmt> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Amount")]
		[PXDBCalced(typeof(Switch<Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleCA>>, CAAdj.tranAmt,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAP>>, APInvoice.origDocAmt,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAR>>, ARInvoice.origDocAmt>>>>), typeof(decimal))]
		public virtual decimal? DisplayOrigDocAmt
			{
			get;
			set;
			}
		#endregion
		#region DisplayDocBal
		public abstract class displayDocBal : PX.Data.BQL.BqlDecimal.Field<displayDocBal> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Balance")]
		[PXDBCalced(typeof(Switch<Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleCA>>, decimal0,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAP>>, APInvoice.docBal,
			Case<Where<SVATConversionHist.module, Equal<BatchModule.moduleAR>>, ARInvoice.docBal>>>>), typeof(decimal))]
		public virtual decimal? DisplayDocBal
		{
			get;
			set;
		}
		#endregion
	}

	public class SVATHistStatus
		{
		public static readonly string[] Values = ARDocStatus.Values.Select(value => BatchModule.AR + value)
				.Concat(APDocStatus.Values.Select(value => BatchModule.AP + value))
				.Concat(CATransferStatus.Values.Select(value => BatchModule.CA + value)).ToArray();
		public static readonly string[] Labels = ARDocStatus.Labels.Concat(APDocStatus.Labels).Concat(CATransferStatus.Labels).ToArray();

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(Values, Labels)
			{
			}
		}
	}

	public class SVATHistDocType
			{
		public static readonly string[] Values = ARDocType.Values.Select(value => BatchModule.AR + value)
			.Concat(APDocType.Values.Select(value => BatchModule.AP + value))
			.Concat(CATranType.Values.Select(value => BatchModule.CA + value)).ToArray();
		public static readonly string[] Labels = ARDocType.Labels.Concat(APDocType.Labels).Concat(CATranType.Labels).ToArray();

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(Values, Labels)
			{
			}
			}
		}

	public class SVATTaxReversalMethods
		{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { OnPayments, OnDocuments },
				new string[] { Messages.OnPayments, Messages.OnDocuments })
		{
		}
		}

		public const string OnPayments = "P";
		public const string OnDocuments = "D";

		public class onPayments : PX.Data.BQL.BqlString.Constant<onPayments>
		{
			public onPayments() : base(OnPayments) { }
		}

		public class onDocuments : PX.Data.BQL.BqlString.Constant<onDocuments>
		{
			public onDocuments() : base(OnDocuments) { }
		}
	}
}