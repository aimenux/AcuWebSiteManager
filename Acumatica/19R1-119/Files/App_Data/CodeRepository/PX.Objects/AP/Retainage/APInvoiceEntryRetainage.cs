using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.TX;
using static PX.Objects.AP.APInvoiceEntry;

namespace PX.Objects.AP
{
	[Serializable]
	public class APInvoiceEntryRetainage : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		public override void Initialize()
		{
			base.Initialize();

			RetainageOptions releaseRetainageOptions = ReleaseRetainageOptions.Current;

			PXAction action = Base.Actions["action"];
			if (action != null)
			{
				action.AddMenuAction(releaseRetainage);
			}
		}

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[APRetainedTax(typeof(APRegister), typeof(APTax), typeof(APTaxTran), typeof(APInvoice.taxCalcMode), parentBranchIDField: typeof(APRegister.branchID))]
		protected virtual void APTran_TaxCategoryID_CacheAttached(PXCache sender) { }

		[DBRetainagePercent(
			typeof(APInvoice.retainageApply),
			typeof(APInvoice.defRetainagePct),
			typeof(Sub<Current<APTran.curyLineAmt>, Current<APTran.curyDiscAmt>>),
			typeof(APTran.curyRetainageAmt),
			typeof(APTran.retainagePct))]
		protected virtual void APTran_RetainagePct_CacheAttached(PXCache sender) { }

		[DBRetainageAmount(
			typeof(APTran.curyInfoID),
			typeof(Sub<APTran.curyLineAmt, APTran.curyDiscAmt>),
			typeof(APTran.curyRetainageAmt),
			typeof(APTran.retainageAmt),
			typeof(APTran.retainagePct))]
		protected virtual void APTran_CuryRetainageAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<APRegister.vendorID, Vendor.retainagePct>))]
		protected virtual void APInvoice_DefRetainagePct_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(APRegister.curyRetainageTotal))]
		protected virtual void APInvoice_CuryRetainageUnreleasedAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<Case<Where2<
			FeatureInstalled<FeaturesSet.retainage>,
				And<APRegister.retainageApply, Equal<True>,
				And<APRegister.released, NotEqual<True>>>>,
			APRegister.curyRetainageTotal>,
			APRegister.curyRetainageUnpaidTotal>))]
		protected virtual void APInvoice_CuryRetainageUnpaidTotal_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(
			IIf<Where<APRegister.docType, Equal<APInvoiceType.invoice>,
					And<APRegister.origModule, Equal<BatchModule.moduleAP>,
					And<Current<APSetup.migrationMode>, Equal<False>>>>,
				IsNull<Selector<APRegister.vendorID, Vendor.retainageApply>, False>,
				False>))]
		[PXUIVerify(
			typeof(Where<
			APRegister.retainageApply, NotEqual<True>,
			And<APRegister.isRetainageDocument, NotEqual<True>,
				Or<Selector<APInvoice.termsID, Terms.installmentType>, NotEqual<TermsInstallmentType.multiple>>>>),
			PXErrorLevel.Error,
			Messages.RetainageWithMultipleCreditTerms)]
		[PXUIVerify(
			typeof(Where<APRegister.retainageApply, NotEqual<True>,
				Or<APRegister.curyID, Equal<GetSetupValue<Company.baseCuryID>>>>),
			PXErrorLevel.Error,
			Messages.RetainageDocumentNotInBaseCurrency)]
		protected virtual void APInvoice_RetainageApply_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(
			typeof(Where<APRegister.curyRetainageTotal, GreaterEqual<decimal0>, And<APRegister.hold, NotEqual<True>,
				Or<APRegister.hold, Equal<True>>>>),
			PXErrorLevel.Error,
			Messages.IncorrectRetainageTotalAmount)]
		protected virtual void APInvoice_CuryRetainageTotal_CacheAttached(PXCache sender) { }

		#endregion

		#region APInvoice Events

		protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APInvoice document = e.Row as APInvoice;
			if (document == null) return;

			bool isDocumentReleasedOrPrebooked = document.Released == true || document.Prebooked == true;
			bool isDocumentVoided = document.Voided == true;
			bool isDocumentInvoice = document.DocType == APDocType.Invoice;
			bool retainageApply = document.RetainageApply == true;

			releaseRetainage.SetEnabled(false);

			if (isDocumentReleasedOrPrebooked || isDocumentVoided)
			{
				releaseRetainage.SetEnabled(isDocumentInvoice &&
					document.Released == true &&
					retainageApply &&
					document.CuryRetainageUnreleasedAmt > 0m);
			}
		}

		protected virtual void APInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			APInvoice document = e.Row as APInvoice;
			if (document == null) return;

			if (document.RetainageApply == true &&
				document.Released == true)
			{
				using (new PXConnectionScope())
				{
					APRetainageInvoice dummyInvoice = new APRetainageInvoice();
					dummyInvoice.CuryRetainageUnpaidTotal = 0m;
					dummyInvoice.CuryRetainagePaidTotal = 0m;

					foreach (APRetainageInvoice childRetainageBill in RetainageDocuments
						.Select(document.DocType, document.RefNbr)
						.RowCast<APRetainageInvoice>()
						.Where(res => res.Released == true))
					{
						dummyInvoice.DocType = childRetainageBill.DocType;
						dummyInvoice.RefNbr = childRetainageBill.RefNbr;
						dummyInvoice.CuryOrigDocAmt = childRetainageBill.CuryOrigDocAmt;
						dummyInvoice.CuryDocBal = childRetainageBill.CuryOrigDocAmt;

						foreach (APAdjust application in PXSelect<APAdjust,
							Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
								And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
								And<APAdjust.released, Equal<True>,
								And<APAdjust.voided, NotEqual<True>,
								And<APAdjust.adjgDocType, NotEqual<APDocType.debitAdj>>>>>>>
							.Select(Base, childRetainageBill.DocType, childRetainageBill.RefNbr))
						{
							dummyInvoice.AdjustBalance(application);
						}

						dummyInvoice.CuryRetainageUnpaidTotal += childRetainageBill.DocBal * childRetainageBill.SignAmount;
						dummyInvoice.CuryRetainagePaidTotal += (dummyInvoice.CuryOrigDocAmt - dummyInvoice.CuryDocBal) * dummyInvoice.SignAmount;
					}

					document.CuryRetainageUnpaidTotal = document.CuryRetainageUnreleasedAmt + dummyInvoice.CuryRetainageUnpaidTotal;
					document.CuryRetainagePaidTotal = dummyInvoice.CuryRetainagePaidTotal;
				}
			}
		}

		protected virtual void APInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;

			Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(Base.Document.Cache, doc);

			if (terms != null && doc.RetainageApply == true && terms.InstallmentType == CS.TermsInstallmentType.Multiple)
			{
				sender.RaiseExceptionHandling<APInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.RetainageWithMultipleCreditTerms));
			}

			bool disablePersistingCheckForRetainageAccountAndSub = doc.RetainageApply != true;
			PXDefaultAttribute.SetPersistingCheck<APRegister.retainageAcctID>(sender, doc, disablePersistingCheckForRetainageAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<APInvoice.retainageSubID>(sender, doc, disablePersistingCheckForRetainageAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
		}

		protected virtual void APInvoice_RetainageAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Location vendorLocation = Base.location.View.SelectSingleBound(new[] { e.Row }) as Location;
			if (vendorLocation != null)
			{
				e.NewValue = Base.GetAcctSub<Location.aPRetainageAcctID>(Base.location.Cache, vendorLocation);
			}
		}

		protected virtual void APInvoice_RetainageSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				Location vendorLocation = Base.location.View.SelectSingleBound(new[] { e.Row }) as Location;
				if (vendorLocation != null && e.Row != null)
				{
					e.NewValue = Base.GetAcctSub<Location.aPRetainageSubID>(Base.location.Cache, vendorLocation);
				}
			}
		}

		protected virtual void APInvoice_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.retainageAcctID>(e.Row);
			sender.SetDefaultExt<APInvoice.retainageSubID>(e.Row);
		}

		protected virtual void APInvoice_RetainageApply_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APInvoice document = (APInvoice)e.Row;
			bool? newValue = (bool?)e.NewValue;

			if (document == null) return;

			if (document.RetainageApply == true && newValue == false)
			{
				IEnumerable<APTran> trans = Base.Transactions.Select().AsEnumerable().Where(tran => ((APTran)tran).CuryRetainageAmt != 0 || ((APTran)tran).RetainagePct != 0).RowCast<APTran>();

				if (!trans.Any()) return;

				WebDialogResult wdr =
					Base.Document.Ask(
						Messages.Warning,
						Messages.UncheckApplyRetainage,
						MessageButtons.YesNo,
						MessageIcon.Warning);

				if (wdr == WebDialogResult.Yes)
				{
					foreach (APTran tran in trans)
					{
						tran.CuryRetainageAmt = 0m;
						tran.RetainagePct = 0m;
						Base.Transactions.Update(tran);
					}
				}
				else
				{
					e.Cancel = true;
					e.NewValue = true;
				}
			}
			else if (document.RetainageApply != true && newValue == true)
			{
				ClearCurrentDocumentDiscountDetails();
			}
		}

		protected virtual void ClearCurrentDocumentDiscountDetails()
		{
			Base.DiscountDetails
					.Select()
					.RowCast<APInvoiceDiscountDetail>()
					.ForEach(discountDetail => Base.DiscountDetails.Cache.Delete(discountDetail));

			Base.Discount_Row
				.Select()
				.RowCast<APTran>()
				.ForEach(tran => Base.Discount_Row.Cache.Delete(tran));
		}

		#endregion

		#region APInvoiceDiscountDetail events

		[PXOverride]
		public virtual void APInvoiceDiscountDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APInvoice invoice = Base.Document.Current;

			if (invoice?.RetainageApply == true ||
				invoice?.IsRetainageDocument == true)
			{
				e.Cancel = true;
			}
		}

		public delegate void AddDiscountDelegate(PXCache sender, APInvoice row);

		[PXOverride]
		public void AddDiscount(
			PXCache sender,
			APInvoice row,
			AddDiscountDelegate baseMethod)
		{
			bool isRetainage =
				row.RetainageApply == true ||
				row.IsRetainageDocument == true;

			if (!isRetainage)
			{
				baseMethod(sender, row);
			}
		}

		#endregion

		[PXReadOnlyView]
		[PXCopyPasteHiddenView]
		// APRetainageInvoice class is a APRegister class alias
		// because only APRegister part is affecting by the release process
		// and only this way we can get a proper behavior for the QueryCache mechanism.
		//
		public PXSelectJoin<APRetainageInvoice,
			InnerJoinSingleTable<APInvoice, On<APInvoice.docType, Equal<APRetainageInvoice.docType>,
				And<APInvoice.refNbr, Equal<APRetainageInvoice.refNbr>>>>,
			Where<APRetainageInvoice.isRetainageDocument, Equal<True>,
				And<APRetainageInvoice.origDocType, Equal<Optional<APInvoice.docType>>,
				And<APRetainageInvoice.origRefNbr, Equal<Optional<APInvoice.refNbr>>>>>> RetainageDocuments;

		[PXCopyPasteHiddenView]
		public PXFilter<RetainageOptions> ReleaseRetainageOptions;

		public PXAction<APInvoice> releaseRetainage;

		[PXUIField(
			DisplayName = "Release Retainage",
			MapEnableRights = PXCacheRights.Update,
			MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReleaseRetainage(PXAdapter adapter)
		{
			APInvoice doc = Base.Document.Current;

			if (doc != null &&
				doc.DocType == APDocType.Invoice &&
				doc.RetainageApply == true &&
				doc.CuryRetainageUnreleasedAmt > 0m)
			{
				APRegister reversingDoc;
				if (Base.CheckReversingRetainageDocumentAlreadyExists(Base.Document.Current, out reversingDoc))
				{
					throw new PXException(
						Messages.ReleaseRetainageReversingDocumentExists,
						PXMessages.LocalizeNoPrefix(APDocTypeDict[doc.DocType]),
						PXMessages.LocalizeNoPrefix(APDocTypeDict[reversingDoc.DocType]),
						reversingDoc.RefNbr);
				}

				Base.Save.Press();

				APRetainageRelease retainageGraph = PXGraph.CreateInstance<APRetainageRelease>();

				retainageGraph.Filter.Current.DocDate = doc.DocDate;
				retainageGraph.Filter.Current.FinPeriodID = doc.FinPeriodID;
				retainageGraph.Filter.Current.BranchID = doc.BranchID;
				retainageGraph.Filter.Current.VendorID = doc.VendorID;
				retainageGraph.Filter.Current.RefNbr = doc.RefNbr;
				retainageGraph.Filter.Current.ShowBillsWithOpenBalance = doc.OpenDoc == true;

				APInvoiceExt retainageDocToRelease = retainageGraph.DocumentList.SelectSingle();
				if (retainageDocToRelease == null)
				{
					APRetainageInvoice retainageDoc = RetainageDocuments
						.Select()
						.RowCast<APRetainageInvoice>()
						.FirstOrDefault(row => row.Released != true);

					throw new PXException(
						Messages.ReleaseRetainageNotReleasedDocument,
						PXMessages.LocalizeNoPrefix(APDocTypeDict[retainageDoc.DocType]),
						retainageDoc.RefNbr,
						PXMessages.LocalizeNoPrefix(APDocTypeDict[doc.DocType]));
				}

				throw new PXRedirectRequiredException(retainageGraph, nameof(ReleaseRetainage));
			}

			return adapter.Get();
		}

		public virtual void ReleaseRetainageProc(List<APInvoiceExt> list, RetainageOptions retainageOpts, bool isAutoRelease = false)
					{
			bool failed = false;
			List<APInvoice> result = new List<APInvoice>();

			foreach (var group in list.GroupBy(row => new { row.DocType, row.RefNbr }))
				{
				APInvoiceExt doc = group.First();
				PXProcessing<APInvoiceExt>.SetCurrentItem(doc);
				decimal curyRetainageSum = group.Sum(row => row.CuryRetainageReleasedAmt ?? 0m);

					try
					{
					Base.Clear(PXClearOption.ClearAll);
					PXUIFieldAttribute.SetError(Base.Document.Cache, null, null, null);

					APTran tranNew = null;
					decimal prevCuryTotal = 0m;
					TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null);

			Base.Clear(PXClearOption.PreserveTimeStamp);

					if (doc.CuryRetainageReleasedAmt <= 0 || doc.CuryRetainageReleasedAmt > doc.CuryRetainageBal)
			{
				throw new PXException(Messages.IncorrectRetainageAmount);
			}

			// Magic. We need to prevent rewriting of CurrencyInfo.IsReadOnly 
			// by true in CurrencyInfoView
			// 
			Base.CurrentDocument.Cache.AllowUpdate = true;

			PXResult<APInvoice, CurrencyInfo, Terms, Vendor> resultDoc =
				APInvoice_CurrencyInfo_Terms_Vendor
					.SelectSingleBound(Base, null, doc.DocType, doc.RefNbr, doc.VendorID).AsEnumerable()
					.Cast<PXResult<APInvoice, CurrencyInfo, Terms, Vendor>>()
					.First();

			CurrencyInfo info = resultDoc;
			APInvoice origInvoice = resultDoc;
			Vendor vendor = resultDoc;

			CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
			new_info.CuryInfoID = null;
			new_info.IsReadOnly = false;
			new_info = PXCache<CurrencyInfo>.CreateCopy(Base.currencyinfo.Insert(new_info));

			APInvoice invoice = PXCache<APInvoice>.CreateCopy(origInvoice);
			invoice.CuryInfoID = new_info.CuryInfoID;
			invoice.DocType = APDocType.Invoice;
			invoice.RefNbr = null;
			invoice.LineCntr = null;
					invoice.InvoiceNbr = doc.RetainageVendorRef;

			// Must be set for _RowSelected event handler
			// 
			invoice.OpenDoc = true;
			invoice.Released = false;

			Base.Document.Cache.SetDefaultExt<APInvoice.isMigratedRecord>(invoice);
			invoice.BatchNbr = null;
			invoice.PrebookBatchNbr = null;
			invoice.Prebooked = false;
			invoice.ScheduleID = null;
			invoice.Scheduled = false;
			invoice.NoteID = null;

			invoice.DueDate = null;
			invoice.DiscDate = null;
			invoice.CuryOrigDiscAmt = 0m;
			invoice.OrigDocType = origInvoice.DocType;
			invoice.OrigRefNbr = origInvoice.RefNbr;
			invoice.OrigDocDate = origInvoice.DocDate;

			invoice.PaySel = false;
			invoice.CuryLineTotal = 0m;
			invoice.IsTaxPosted = false;
			invoice.IsTaxValid = false;
			invoice.CuryVatTaxableTotal = 0m;
			invoice.CuryVatExemptTotal = 0m;

			invoice.CuryDocBal = 0m;
					invoice.CuryOrigDocAmt = curyRetainageSum;
			invoice.Hold = !isAutoRelease && Base.apsetup.Current.HoldEntry == true || Base.IsApprovalRequired(invoice, Base.Document.Cache);

			invoice.DocDate = retainageOpts.DocDate;
            FinPeriodIDAttribute.SetPeriodsByMaster<APInvoice.finPeriodID>(Base.Document.Cache, invoice, retainageOpts.MasterFinPeriodID);

			Base.ClearRetainageSummary(invoice);
			invoice.RetainageApply = false;
			invoice.IsRetainageDocument = true;

			invoice = Base.Document.Insert(invoice);

			if (new_info != null)
			{
				CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(Base);

				b_info.CuryID = new_info.CuryID;
				b_info.CuryEffDate = new_info.CuryEffDate;
				b_info.CuryRateTypeID = new_info.CuryRateTypeID;
				b_info.CuryRate = new_info.CuryRate;
				b_info.RecipRate = new_info.RecipRate;
				b_info.CuryMultDiv = new_info.CuryMultDiv;
				Base.currencyinfo.Update(b_info);
			}

					bool isRetainageByLines = doc.LineNbr != 0;

					foreach (APInvoiceExt docLine in group)
					{
						PXProcessing<APInvoiceExt>.SetCurrentItem(docLine);

						PXResultset<APTran> details = isRetainageByLines
							? PXSelect<APTran,
								Where<APTran.tranType, Equal<Required<APTran.tranType>>,
									And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
									And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>
								.SelectSingleBound(Base, null, docLine.DocType, docLine.RefNbr, docLine.LineNbr)
							: PXSelectGroupBy<APTran,
				Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.curyRetainageAmt, NotEqual<decimal0>>>>,
				Aggregate<
					GroupBy<APTran.taxCategoryID,
					Sum<APTran.curyRetainageAmt>>>>
								.Select(Base, docLine.DocType, docLine.RefNbr);

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

			foreach (APTran detail in details)
			{
				// Create APTran record for chosen retainage amount, 
				// clear all required fields to prevent tax calculation,
				// discount calculation and retainage calculation.
				// CuryUnitCost = 0m and CuryLineAmt = 0m here to prevent their 
				// FieldDefaulting events, because in our case default value 
				// should be equal to zero.
				//
				tranNew = Base.Transactions.Insert(new APTran
				{
					CuryUnitCost = 0m,
					CuryLineAmt = 0m
				});

				tranNew.BranchID = origInvoice.BranchID;
				tranNew.TaxCategoryID = detail.TaxCategoryID;
				tranNew.AccountID = origInvoice.RetainageAcctID;
				tranNew.SubID = origInvoice.RetainageSubID;
				tranNew.ProjectID = ProjectDefaultAttribute.NonProject();

				tranNew.Qty = 0m;
				tranNew.CuryUnitCost = 0m;
				tranNew.ManualDisc = true;
				tranNew.DiscPct = 0m;
				tranNew.CuryDiscAmt = 0m;
				tranNew.RetainagePct = 0m;
				tranNew.CuryRetainageAmt = 0m;
				tranNew.CuryTaxableAmt = 0m;
				tranNew.CuryTaxAmt = 0;
				tranNew.CuryExpenseAmt = 0m;
				tranNew.GroupDiscountRate = 1m;
				tranNew.DocumentDiscountRate = 1m;

							tranNew.OrigLineNbr = docLine.LineNbr;

				using (new PXLocaleScope(vendor.LocaleName))
				{
					tranNew.TranDesc = PXMessages.LocalizeFormatNoPrefix(
						Messages.RetainageForTransactionDescription,
						APDocTypeDict[origInvoice.DocType],
						origInvoice.RefNbr);
				}

							prevCuryTotal = curyRetainageSum - (invoice.CuryDocBal ?? 0m);
							decimal retainagePercent = (decimal)(docLine.CuryRetainageReleasedAmt /
								(isRetainageByLines ? detail.CuryOrigRetainageAmt : doc.CuryRetainageTotal));

				tranNew.CuryLineAmt = PXCurrencyAttribute.RoundCury(Base.Transactions.Cache, tranNew, (detail.CuryRetainageAmt ?? 0m) * retainagePercent);
				tranNew = Base.Transactions.Update(tranNew);
			}

						PXProcessing<APInvoiceExt>.SetProcessed();
					}

			ClearCurrentDocumentDiscountDetails();

			// We should copy all taxes from the original document
			// because it is possible to add or delete them.
			// 
			foreach (APTaxTran aptaxtran in PXSelect<APTaxTran,
				Where<APTaxTran.module, Equal<BatchModule.moduleAP>,
					And<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>,
					And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>>>>>
						.Select(Base, group.Key.DocType, group.Key.RefNbr)
				.RowCast<APTaxTran>()
				.Where(row => row.CuryRetainedTaxAmt != 0m))
			{
				APTaxTran new_aptaxtran = Base.Taxes.Insert(new APTaxTran
				{
					TaxID = aptaxtran.TaxID
				});

				if (new_aptaxtran != null)
				{
					new_aptaxtran = PXCache<APTaxTran>.CreateCopy(new_aptaxtran);
					new_aptaxtran.TaxRate = aptaxtran.TaxRate;
					new_aptaxtran = Base.Taxes.Update(new_aptaxtran);
				}
			}

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null, oldTaxCalc);
					decimal diff = curyRetainageSum - (invoice.CuryDocBal ?? 0m);

			if (tranNew != null && diff != 0m)
			{
				HashSet<string> taxList = PXSelectJoin<APTax,
					InnerJoin<Tax, On<Tax.taxID, Equal<APTax.taxID>>>,
					Where<APTax.tranType, Equal<Required<APTax.tranType>>,
						And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
						And<APTax.lineNbr, Equal<Required<APTax.lineNbr>>,
						And<Tax.taxType, NotEqual<CSTaxType.use>>>>>>
					.Select(Base, tranNew.TranType, tranNew.RefNbr, tranNew.LineNbr)
					.RowCast<APTax>()
					.Select(row => row.TaxID)
					.ToHashSet();

				// To guarantee correct document total amount 
				// we should calculate last line total, 
				// including its taxes.
				//
				TaxAttribute.CalcTaxable calcClass = new TaxAttribute.CalcTaxable(false, TaxAttribute.TaxCalcLevelEnforcing.None);
				decimal curyLineAmt = calcClass.CalcTaxableFromTotalAmount(
					Base.Transactions.Cache,
					tranNew,
					taxList,
					invoice.DocDate.Value,
					prevCuryTotal);

				tranNew.CuryLineAmt = curyLineAmt;
				tranNew = Base.Transactions.Update(tranNew);
			}

			APVendorRefNbrAttribute aPVendorRefNbrAttribute = Base.Document.Cache.GetAttributesReadonly<APInvoice.invoiceNbr>()
				.OfType<APVendorRefNbrAttribute>().FirstOrDefault();
			if (aPVendorRefNbrAttribute != null)
			{
				var args = new PXFieldVerifyingEventArgs(invoice, invoice.InvoiceNbr, true);
				aPVendorRefNbrAttribute.FieldVerifying(Base.Document.Cache, args);
			}

					Base.Save.Press();

					if (isAutoRelease && invoice.Hold != true)
					{
						using (new PXTimeStampScope(null))
						{
							APDocumentRelease.ReleaseDoc(new List<APRegister> { invoice }, false);
						}
					}
				}
				catch (PXException exc)
				{
					PXProcessing<APInvoiceExt>.SetError(exc);
					failed = true;
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
		}

		public PXAction<APInvoice> ViewRetainageDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewRetainageDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(RetainageDocuments.Current.DocType, RetainageDocuments.Current.RefNbr, RetainageDocuments.Current.OrigModule);
			return adapter.Get();
		}
	}
}