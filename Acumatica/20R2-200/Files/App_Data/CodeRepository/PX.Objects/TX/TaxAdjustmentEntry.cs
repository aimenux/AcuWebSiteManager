using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.Common.Tools;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.SM;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.TX
{
	public class TaxAdjustmentEntry : PXGraph<TaxAdjustmentEntry, TaxAdjustment>
	{
		#region Extensions

		public class TaxAdjustmentEntryDocumentExtension : DocumentWithLinesGraphExtension<TaxAdjustmentEntry>
		{
			#region Mapping

			public override void Initialize()
			{
				base.Initialize();

				Documents = new PXSelectExtension<Document>(Base.Document);
				Lines = new PXSelectExtension<DocumentLine>(Base.Transactions);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(TaxAdjustment))
				{
					HeaderTranPeriodID = typeof(TaxAdjustment.tranPeriodID),
					HeaderDocDate = typeof(TaxAdjustment.docDate)
				};
			}

			protected override DocumentLineMapping GetDocumentLineMapping()
			{
				return new DocumentLineMapping(typeof(TaxTran));
			}

			#endregion

			protected override bool ShouldUpdateLinesOnDocumentUpdated(Events.RowUpdated<Document> e)
			{
				return base.ShouldUpdateLinesOnDocumentUpdated(e)
				       || !e.Cache.ObjectsEqual<Document.headerDocDate>(e.Row, e.OldRow);
			}

			protected override void ProcessLineOnDocumentUpdated(Events.RowUpdated<Document> e,
			    DocumentLine line)
			{
				base.ProcessLineOnDocumentUpdated(e, line);

				if (!e.Cache.ObjectsEqual<Document.headerDocDate>(e.Row, e.OldRow))
				{
					Lines.Cache.SetDefaultExt<DocumentLine.tranDate>(line);
				}
			}
		}

		#endregion

		protected bool IsReversingInProgress { get; set; }

		public PXSelect<TaxAdjustment, 
			Where<TaxAdjustment.docType, Equal<Optional<TaxAdjustment.docType>>>> 
			Document;

		public PXSelect<TaxAdjustment, 
			Where<TaxAdjustment.docType, Equal<Current<TaxAdjustment.docType>>, 
				And<TaxAdjustment.refNbr, Equal<Current<TaxAdjustment.refNbr>>>>> 
			CurrentDocument;

		public PXSelect<TaxTran, 
			Where<TaxTran.tranType, Equal<Current<TaxAdjustment.docType>>, 
				And<TaxTran.refNbr, Equal<Current<TaxAdjustment.refNbr>>>>> 
			Transactions;

		public PXSelect<
			CurrencyInfo, 
			Where<CurrencyInfo.curyInfoID, Equal<Current<TaxAdjustment.curyInfoID>>>> 
			currencyinfo;

		public PXSetup<Vendor, 
			Where<Vendor.bAccountID, Equal<Optional<TaxAdjustment.vendorID>>>> 
			vendor;

		public PXSetup<Location, 
			Where<Location.bAccountID, Equal<Current<TaxAdjustment.vendorID>>, 
				And<Location.locationID, Equal<Optional<TaxAdjustment.vendorLocationID>>>>> 
			location;
		
		public PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>> SalesTax_Select;
		public PXSelect<TaxRev, 
			Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>, 
				And<TaxRev.taxType, Equal<Required<TaxRev.taxType>>, 
				And<TaxRev.outdated, Equal<False>, 
				And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>> 
			SalesTaxRev_Select;

		public ToggleCurrency<TaxAdjustment> CurrencyView;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public PXAction<TaxAdjustment> newVendor;

		[PXUIField(DisplayName = Messages.NewVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewVendor(PXAdapter adapter)
		{
			VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
			throw new PXRedirectRequiredException(graph, Messages.NewVendor);
		}

		public PXAction<TaxAdjustment> editVendor;
		[PXUIField(DisplayName = Messages.EditVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable EditVendor(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
				graph.BAccount.Current = (VendorR)vendor.Current;
				throw new PXRedirectRequiredException(graph, Messages.EditVendor);
			}
			return adapter.Get();
		}

		public PXAction<TaxAdjustment> viewBatch;

		[PXUIField(DisplayName = Messages.ReviewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (Document.Current != null && !String.IsNullOrEmpty(Document.Current.BatchNbr))
			{
				GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(Document.Current.BatchNbr, BatchModule.AP);
				throw new PXRedirectRequiredException(graph, Messages.CurrentBatchRecord);
			}
			return adapter.Get();
		}

		public PXAction<TaxAdjustment> viewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable ViewOriginalDocument(PXAdapter adapter)
		{
			TaxAdjustment currentTaxAdj = Document.Current;

			if (currentTaxAdj?.OrigRefNbr == null)
				return adapter.Get();

			TaxAdjustment originalTaxAdj =
				PXSelect<TaxAdjustment,
					Where<TaxAdjustment.docType, Equal<Required<TaxAdjustment.docType>>,
					And<TaxAdjustment.refNbr, Equal<Required<TaxAdjustment.refNbr>>>>>
				.SelectSingleBound(this, currents: null, pars: new[] { currentTaxAdj.DocType, currentTaxAdj.OrigRefNbr });

			if (originalTaxAdj != null)
			{
				Document.Cache.Current = originalTaxAdj;
			}

			return new List<TaxAdjustment> { originalTaxAdj };
		}

		public PXAction<TaxAdjustment> release;

		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]	
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<TaxAdjustment> list = new List<TaxAdjustment>();

			foreach (TaxAdjustment doc in adapter.Get())
			{
				if (doc.Hold != true && doc.Released != true)
				{
					cache.Update(doc);
					list.Add(doc);
				}
			}

			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}

			Save.Press();

			if (list.Count > 0)
			{
				PXLongOperation.StartOperation(this, () => ReportTaxReview.ReleaseDoc(list));
			}

			return list;
		}

		public PXAction<TaxAdjustment> reverseAdjustment;

		[PXUIField(DisplayName = Messages.Reverse, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ReverseAdjustment(PXAdapter adapter)
		{
			TaxAdjustment taxAdjToReverse = Document.Current;

			if (taxAdjToReverse?.Released != true || !AskUserApprovalIfReversingDocumentAlreadyExists(taxAdjToReverse))
			{
				return adapter.Get();
			}

			Save.Press();

			try
			{
				IsReversingInProgress = true;
				Clear(PXClearOption.PreserveTimeStamp);

				var reverseAdjustmentWithCuryInfo = CreateReversingTaxAdjustmentWithCuryInfo(taxAdjToReverse);
				TaxAdjustment reverseAdjustment = Document.Insert(reverseAdjustmentWithCuryInfo.Item1);

				UpdateCurrencyInfoForReversedTaxAdjustment(reverseAdjustmentWithCuryInfo.Item2);
				AddReversedTaxTransactionsToReversedTaxAdjustment(taxAdjToReverse, reverseAdjustment);

				Document.Cache.RaiseExceptionHandling<TaxAdjustment.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);
				PXTrace.WriteVerbose("Reverse Tax Adjustment for Tax Adjustment \"{0}\" was created", taxAdjToReverse.RefNbr);
				return new List<TaxAdjustment> { Document.Current };
			}
			catch (PXException e)
			{
				PXTrace.WriteError(e);
				Clear(PXClearOption.PreserveTimeStamp);
				Document.Current = taxAdjToReverse;				
				throw;
			}
			finally
			{
				IsReversingInProgress = false;
			}
		}

		/// <summary>
		/// Ask user for approval for creation of another reversal if reversing <see cref="TaxAdjustment"/> already exists.
		/// </summary>
		/// <param name="taxAdjToReverse">The tax adjustment to reverse.</param>
		/// <returns/>		
		private bool AskUserApprovalIfReversingDocumentAlreadyExists(TaxAdjustment taxAdjToReverse)
		{
			TaxAdjustment reversingTaxAdj = 
				PXSelect<TaxAdjustment,
					Where<TaxAdjustment.docType, Equal<Required<TaxAdjustment.docType>>,
						And<TaxAdjustment.origRefNbr, Equal<Required<TaxAdjustment.origRefNbr>>>>,
					OrderBy<
						Desc<TaxAdjustment.createdDateTime>>>
				.SelectSingleBound(this, currents: null, pars: new[] { taxAdjToReverse.DocType, taxAdjToReverse.RefNbr });

			if (reversingTaxAdj == null)
				return true;
	
			string docTypeDescr;
			TaxAdjustmentType.ListAttribute labelsListAttribute = new TaxAdjustmentType.ListAttribute();

			if (!labelsListAttribute.ValueLabelDic.TryGetValue(reversingTaxAdj.DocType, out docTypeDescr))
			{
				docTypeDescr = taxAdjToReverse.DocType;
				PXTrace.WriteWarning("Failed to retrieve tax adjustment type {0} description from {1} attribute.", reversingTaxAdj.DocType, 
									 nameof(TaxAdjustmentType.ListAttribute));
			}
		    
			string localizedMsg = PXMessages.LocalizeFormatNoPrefix(AR.Messages.ReversingDocumentExists, docTypeDescr, reversingTaxAdj.RefNbr);	
			return Document.Ask(Messages.Reverse, localizedMsg, MessageButtons.YesNo).IsPositive();
		}

		private Tuple<TaxAdjustment, CurrencyInfo> CreateReversingTaxAdjustmentWithCuryInfo(TaxAdjustment taxAdjToReverse)
		{
			CurrencyInfo originalCuryInfo =
				PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
				.SelectSingleBound(this, currents: null, pars: taxAdjToReverse.CuryInfoID);

			if (originalCuryInfo == null)
			{
				PXTrace.WriteError("The {0} object with the ID {1} is not found", nameof(CurrencyInfo), taxAdjToReverse.CuryInfoID);
				throw new PXException("The {0} object with the ID {1} is not found. The reverse tax adjustment can't be created.", 
									  nameof(CurrencyInfo), taxAdjToReverse.CuryInfoID);
			}

			CurrencyInfo reversedAdjCuryInfo = PXCache<CurrencyInfo>.CreateCopy(originalCuryInfo);
			reversedAdjCuryInfo.CuryInfoID = null;
			reversedAdjCuryInfo.IsReadOnly = false;
			reversedAdjCuryInfo = currencyinfo.Insert(reversedAdjCuryInfo);
			 
			TaxAdjustment reverseAdjustment = PXCache<TaxAdjustment>.CreateCopy(taxAdjToReverse);
			reverseAdjustment.CuryInfoID = reversedAdjCuryInfo.CuryInfoID;

			reverseAdjustment.RefNbr = null;
			reverseAdjustment.OrigRefNbr = taxAdjToReverse.RefNbr;
			reverseAdjustment.Released = false;
			reverseAdjustment.Hold = true;

			reverseAdjustment.BatchNbr = null;		
			reverseAdjustment.NoteID = null;

			reverseAdjustment.OrigDocAmt = -reverseAdjustment.OrigDocAmt;
			reverseAdjustment.CuryOrigDocAmt = -reverseAdjustment.CuryOrigDocAmt;

			//Adjustment doc balance is calculated via PXFormula attribute on TaxTan, therefore it's cleared here 
			reverseAdjustment.CuryDocBal = null;
			reverseAdjustment.DocBal = null;
			return Tuple.Create(reverseAdjustment, reversedAdjCuryInfo);
		}

		private void UpdateCurrencyInfoForReversedTaxAdjustment(CurrencyInfo reversedAdjCuryInfo)
		{
			if (reversedAdjCuryInfo == null)
				return;

			CurrencyInfo b_info = (CurrencyInfo)
				PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Current<TaxAdjustment.curyInfoID>>>>
				.Select(this, null);

			b_info.CuryID = reversedAdjCuryInfo.CuryID;
			b_info.CuryEffDate = reversedAdjCuryInfo.CuryEffDate;
			b_info.CuryRateTypeID = reversedAdjCuryInfo.CuryRateTypeID;
			b_info.CuryRate = reversedAdjCuryInfo.CuryRate;
			b_info.RecipRate = reversedAdjCuryInfo.RecipRate;
			b_info.CuryMultDiv = reversedAdjCuryInfo.CuryMultDiv;

			currencyinfo.Update(b_info);
		}

		private void AddReversedTaxTransactionsToReversedTaxAdjustment(TaxAdjustment originalTaxAdj, TaxAdjustment reversedTaxAdj)
		{
			if (reversedTaxAdj == null)
			{
				PXTrace.WriteError("The creation reversed Tax Adjustment for the Tax Adjustment with the ID {0} failed", originalTaxAdj.RefNbr);
				throw new PXException("The creation reversed Tax Adjustment for the Tax Adjustment with the ID {0} failed", originalTaxAdj.RefNbr);
			}

			var originalTransactions =
				PXSelect<TaxTran,
					Where<TaxTran.tranType, Equal<Required<TaxAdjustment.docType>>,
					And<TaxTran.refNbr, Equal<Required<TaxAdjustment.refNbr>>>>>
				.Select(this, originalTaxAdj.DocType, originalTaxAdj.RefNbr);

			foreach (TaxTran originalTaxTran in originalTransactions)
			{
				TaxTran reversingTran = PXCache<TaxTran>.CreateCopy(originalTaxTran);

				reversingTran.TranType = originalTaxTran.TranType;
				reversingTran.RefNbr = reversedTaxAdj.RefNbr;
				reversingTran.RecordID = null;

				reversingTran.Released = null;
				reversingTran.CuryInfoID = null;

				reversingTran.ExpenseAmt = -originalTaxTran.ExpenseAmt;
				reversingTran.CuryExpenseAmt = -originalTaxTran.CuryExpenseAmt;

				reversingTran.OrigTaxableAmt = -originalTaxTran.OrigTaxableAmt;           
				reversingTran.CuryOrigTaxableAmt = -originalTaxTran.CuryOrigTaxableAmt;   

				reversingTran.TaxAmt = -originalTaxTran.TaxAmt;
				reversingTran.CuryTaxAmt = -originalTaxTran.CuryTaxAmt;

				reversingTran.TaxableAmt = -originalTaxTran.TaxableAmt;
				reversingTran.CuryTaxableAmt = -originalTaxTran.CuryTaxableAmt;

				reversingTran.ReportTaxAmt = -originalTaxTran.ReportTaxAmt;
				reversingTran.ReportTaxableAmt = -originalTaxTran.ReportTaxableAmt;

				Transactions.Insert(reversingTran);
			}

			Transactions.View.RequestRefresh();
		}

		public PXSetup<APSetup> APSetup;

		public TaxAdjustmentEntry()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled<TaxAdjustment.curyID>(Document.Cache, null, false);
			PXUIFieldAttribute.SetVisible<TaxAdjustment.vendorLocationID>(Document.Cache, null, false);

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => 
			{
				if (e.Row != null)
					e.NewValue = BAccountType.VendorType;
			});
		}


		#region CurrencyInfo Events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
				{
					e.NewValue = vendor.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryRateTypeID))
				{
					e.NewValue = vendor.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((TaxAdjustment)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}
		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (vendor.Current != null && vendor.Current.AllowOverrideRate != true)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(sender, null, false);			
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion

		protected virtual void TaxAdjustment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TaxAdjustment doc = e.Row as TaxAdjustment;

			if (doc == null)
			{
				return;
			}

			bool isAdjustmentReleased = doc.Released == true;

			cache.AllowDelete = !isAdjustmentReleased;
			cache.AllowUpdate = !isAdjustmentReleased;

			Transactions.Cache.SetAllEditPermissions(!isAdjustmentReleased);
			PXUIFieldAttribute.SetEnabled(cache, doc, !isAdjustmentReleased);
			reverseAdjustment.SetEnabled(isAdjustmentReleased);

			if (isAdjustmentReleased)
			{						
				release.SetEnabled(false);
			}
			else
			{
				PXDBCurrencyAttribute.SetBaseCalc<TaxAdjustment.curyDocBal>(cache, null, true);
				
				PXUIFieldAttribute.SetEnabled<TaxAdjustment.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<TaxAdjustment.curyDocBal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<TaxAdjustment.batchNbr>(cache, doc, false);

				release.SetEnabled(doc.Hold == false);
				ValidateDocDate(cache, doc);
			}

			PXUIFieldAttribute.SetEnabled<TaxAdjustment.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<TaxAdjustment.refNbr>(cache, doc);
			PXUIFieldAttribute.SetEnabled<TaxAdjustment.curyID>(cache, doc, false);
			PXUIFieldAttribute.SetVisible<TaxAdjustment.curyID>(cache, doc, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			editVendor.SetEnabled(vendor.Current != null);
			bool tranExist = (TaxTran)this.Transactions.SelectWindowed(0, 1) != null;
			PXUIFieldAttribute.SetEnabled<TaxAdjustment.vendorID>(cache, doc, !tranExist);

			cache.RaiseExceptionHandling<TaxAdjustment.docDate>(doc, doc.DocDate, null);

			if (doc?.BranchID == null || doc.VendorID == null || doc.DocDate == null)
				return;

			TaxPeriod taxPeriod = GetTaxPeriodForTaxAdjustment(doc);

			if (taxPeriod == null)
			{
				cache.RaiseExceptionHandling<TaxAdjustment.docDate>(doc, doc.DocDate,
					new PXSetPropertyException(Messages.DateBelongsToNonExistingPeriod, PXErrorLevel.Warning));
			}
			else if (taxPeriod.Status == TaxPeriodStatus.Closed)
			{
				cache.RaiseExceptionHandling<TaxAdjustment.docDate>(doc, doc.DocDate,
					new PXSetPropertyException(Messages.DateBelongsToClosedPeriod, PXErrorLevel.Warning));
			}
		}

		protected virtual void TaxAdjustment_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxAdjustment doc = (TaxAdjustment)e.Row;
			if (doc == null) return;

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || sender.GetValuePending<TaxAdjustment.curyID>(doc) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<TaxAdjustment.curyInfoID>(sender, doc);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<TaxAdjustment.docDate>(doc, doc.DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						doc.CuryID = info.CuryID;
					}
				}
			}

			if (!this.IsCopyPasteContext)
			{
				sender.SetDefaultExt<TaxAdjustment.vendorLocationID>(e.Row);
				sender.SetDefaultExt<TaxAdjustment.taxPeriod>(e.Row);
				sender.SetDefaultExt<TaxAdjustment.adjAccountID>(e.Row);
				sender.SetDefaultExt<TaxAdjustment.adjSubID>(e.Row);
			}
		}

		protected virtual void TaxAdjustment_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<TaxAdjustment.docDate>(sender, e);

			foreach (TaxTran taxTran in Transactions.Select())
			{
				SetTaxTranTranDate(taxTran);

				_TaxRev = SalesTaxRev_Select.Select(taxTran.TaxID, taxTran.TaxType, taxTran.TranDate);

				Transactions.Cache.SetDefaultExt<TaxTran.taxBucketID>(taxTran);
				Transactions.Cache.SetDefaultExt<TaxTran.taxRate>(taxTran);
			}

			_TaxRev = null;

			if (!(e.Row is TaxAdjustment taxAdjustment) || taxAdjustment.TaxPeriod != null || taxAdjustment.DocDate == null)
				return;

			TaxPeriod taxPeriod = GetTaxPeriodForTaxAdjustment(taxAdjustment);

			if (taxPeriod == null)
				return;

			if (taxPeriod.Status == TaxPeriodStatus.Prepared)
			{
				sender.SetValue<TaxAdjustment.taxPeriod>(taxAdjustment, taxPeriod.TaxPeriodID);
			}
		}

		protected virtual void TaxAdjustment_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var doc = e.Row as TaxAdjustment;

			if (doc == null)
				return;

			if (doc.BranchID == null)
			{
				doc.TaxPeriod = null;
			}
			else
			{
				sender.SetDefaultExt<TaxAdjustment.taxPeriod>(doc);
			}

			MarkLinesUpdated();
		}

		protected virtual void TaxAdjustment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var doc = e.Row as TaxAdjustment;

			if (doc == null)
				return;

			if (doc.Hold != true && doc.Released != true)
			{
				sender.RaiseExceptionHandling<TaxAdjustment.curyOrigDocAmt>(e.Row, ((TaxAdjustment) e.Row).CuryOrigDocAmt,
					((TaxAdjustment) e.Row).CuryDocBal != ((TaxAdjustment) e.Row).CuryOrigDocAmt
						? new PXSetPropertyException(Messages.DocumentOutOfBalance)
						: null);
			}
		}

		protected virtual void TaxAdjustment_TaxPeriod_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var document = (TaxAdjustment)e.Row;

			MarkLinesUpdated();

			SetDocDateByPeriods(cache, document);
			ValidateDocDate(cache, document);
		}

		private void SetDocDateByPeriods(PXCache cache, TaxAdjustment document)
		{
			if (document.TaxPeriod == null || document.BranchID == null)
				return;

			var taxPeriod = TaxYearMaint.FindTaxPeriodByKey(this,
				PXAccess.GetParentOrganizationID(document.BranchID),
				document.VendorID,
				document.TaxPeriod);

			if (taxPeriod == null)
				return;

			DateTime? docDate;

			if (vendor.Current.TaxReportFinPeriod == true)
			{
				var finPeriod = FinPeriodRepository.FindMaxFinPeriodWithEndDataBelongToInterval(taxPeriod.StartDate,
																								taxPeriod.EndDate,
																								PXAccess.GetParentOrganizationID(document.BranchID));

				docDate = finPeriod != null
							? finPeriod.FinDate
							: Accessinfo.BusinessDate;
			}
			else
			{
				docDate = taxPeriod.EndDateUI;
			}

			cache.SetValueExt<TaxAdjustment.docDate>(document, docDate);
		}

		private void ValidateDocDate(PXCache cache, TaxAdjustment doc)
		{
			if (doc.DocDate == null || doc.TaxPeriod == null || doc.BranchID == null)
				return;
			
			var taxPeriod = TaxYearMaint.FindTaxPeriodByKey(this, 
															PXAccess.GetParentOrganizationID(doc.BranchID),
															doc.VendorID, 
															doc.TaxPeriod);

			if(taxPeriod == null)
				return;

			string errorMessage = null;

			if (vendor.Current.TaxReportFinPeriod == true)
			{
				var finPeriod = FinPeriodRepository.GetByID(doc.FinPeriodID, PXAccess.GetParentOrganizationID(doc.BranchID));

				if (finPeriod.FinDate >= taxPeriod.EndDate)
				{
					errorMessage = Messages.SelectedDateBelongsToTheTaxPeriodThatIsGreaterThanTheSpecifiedOne;
				}
			}
			else if (doc.DocDate >= taxPeriod.EndDate)
			{
				errorMessage = Messages.SelectedDateBelongsToTheTaxPeriodThatIsGreaterThanTheSpecifiedOne;
			}

			var ex = errorMessage != null
				? new PXSetPropertyException(errorMessage, PXErrorLevel.Warning)
				: null;

			cache.RaiseExceptionHandling<TaxAdjustment.docDate>(doc, doc.DocDate, ex);
		}

		protected Tax _Tax;
		protected TaxRev _TaxRev;

        protected virtual void TaxTranDefaulting(PXCache sender, TaxTran tran)
        {
            _Tax = SalesTax_Select.Select(tran.TaxID);

            sender.SetDefaultExt<TaxTran.accountID>(tran);
            sender.SetDefaultExt<TaxTran.subID>(tran);
            sender.SetDefaultExt<TaxTran.taxType>(tran);
	        SetTaxTranTranDate(tran);

			_TaxRev = SalesTaxRev_Select.Select(tran.TaxID, tran.TaxType, tran.TranDate);

            sender.SetDefaultExt<TaxTran.taxBucketID>(tran);
            sender.SetDefaultExt<TaxTran.taxRate>(tran);

            _Tax = null;
            _TaxRev = null;
        }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(TaxAdjustment.branchID))]
		protected virtual void TaxTran_BranchID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(TaxAdjustment.taxPeriod), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void TaxTran_TaxPeriodID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void TaxTran_TaxID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (object.Equals(e.OldValue, ((TaxTran)e.Row).TaxID) == false)
			{
                TaxTranDefaulting(sender, (TaxTran)e.Row);
			}
		}

        protected virtual void TaxTran_TaxID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            TaxTran tran = (TaxTran) e.Row;

            if (tran == null)
				return;

            TaxTran copy = (TaxTran)sender.CreateCopy(tran);
            copy.TaxID = (string)e.NewValue;
            TaxTranDefaulting(sender, copy);

            if (copy.TaxBucketID == null)
            {
                throw new PXSetPropertyException(
					Messages.EffectiveTaxNotFound, 
					copy.TaxID, 
					GetLabel.For<TaxType>(copy.TaxType));
            }
        }

	    protected virtual void TaxTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_Tax != null && Document.Current != null)
			{
				switch (Document.Current.DocType)
				{
					case TaxAdjustmentType.AdjustOutput:
						e.NewValue = SalesTax_Select.GetValueExt<Tax.salesTaxAcctID>(_Tax);
						break;
					case TaxAdjustmentType.AdjustInput:
						e.NewValue = SalesTax_Select.GetValueExt<Tax.purchTaxAcctID>(_Tax);
						break;
				}
			}
		}

		protected virtual void TaxTran_RevisionID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current.BranchID == null)
			{
				e.NewValue = null;
			}
			else
			{
				Organization organization = OrganizationMaint.FindOrganizationByID(this, PXAccess.GetParentOrganizationID(Document.Current.BranchID));
				int?[] branchID = organization.FileTaxesByBranches == true ? new[] {Document.Current.BranchID} : null;

				using (new PXReadBranchRestrictedScope(organization.OrganizationID.SingleToArray(), branchID, requireAccessForAllSpecified:true))
				{
					TaxHistory max =
						PXSelect<TaxHistory,
						Where<TaxHistory.vendorID, Equal<Current<TaxAdjustment.vendorID>>,
							And<TaxHistory.taxPeriodID, Equal<Current<TaxAdjustment.taxPeriod>>>>,
						OrderBy<
							Desc<TaxHistory.revisionID>>>
						.Select(this);

					e.NewValue = max != null ? max.RevisionID : 1;

				}
			}
		}

		protected virtual void TaxTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_Tax != null && Document.Current != null)
			{
				switch (Document.Current.DocType)
				{
					case TaxAdjustmentType.AdjustOutput:
						e.NewValue = SalesTax_Select.GetValueExt<Tax.salesTaxSubID>(_Tax);
						break;
					case TaxAdjustmentType.AdjustInput:
						e.NewValue = SalesTax_Select.GetValueExt<Tax.purchTaxSubID>(_Tax);
						break;
				}
			}
		}
		protected virtual void TaxTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			TaxTran row = e.Row as TaxTran;
			if (row == null) return;

			row.ReportTaxAmt = row.CuryTaxAmt;
			row.ReportTaxableAmt = row.CuryTaxableAmt;
		}
		protected virtual void TaxTran_TaxType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				switch (Document.Current.DocType)
				{
					case TaxAdjustmentType.AdjustOutput:
						e.NewValue = TaxType.Sales;
						break;
					case TaxAdjustmentType.AdjustInput:
						e.NewValue = TaxType.Purchase;
						break;
				}
			}
		}

		protected virtual void SetTaxTranTranDate(TaxTran tran)
		{
			if (Document.Current == null)
				return;

			tran.TranDate = Document.Current.DocDate;
		}

		protected virtual void TaxTran_TaxBucketID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		    TaxTran tran = (TaxTran) e.Row;
			if (_TaxRev != null)
			{
				e.NewValue = _TaxRev.TaxBucketID;
				sender.SetValue<TaxTran.taxRate>(e.Row, _TaxRev.TaxRate);
			}
		}

		protected virtual void TaxTran_TaxRate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_TaxRev != null)
			{
				e.NewValue = _TaxRev.TaxRate;
				e.Cancel = true;
			}
		}

		private void MarkLinesUpdated()
		{
			foreach (TaxTran taxTran in Transactions.Select())
			{
				Transactions.Cache.MarkUpdated(taxTran);
			}
		}

		private TaxPeriod GetTaxPeriodForTaxAdjustment(TaxAdjustment taxAdjustment)
		{
			if (taxAdjustment?.BranchID == null || taxAdjustment.VendorID == null || taxAdjustment.DocDate == null)
				return null;

			TaxPeriod taxPeriod = 
				PXSelectJoin<TaxPeriod,
				InnerJoin<GL.Branch,
					On<GL.Branch.organizationID, Equal<TaxPeriod.organizationID>>>,
				Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>,
					And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
					And<TaxPeriod.startDate, LessEqual<Required<TaxPeriod.startDate>>,
					And<TaxPeriod.endDate, Greater<Required<TaxPeriod.endDate>>>>>>>
				.SelectSingleBound(this, currents: null, pars: new object[] 
				{
					taxAdjustment.BranchID,
					taxAdjustment.VendorID,
					taxAdjustment.DocDate,
					taxAdjustment.DocDate
				});

			return taxPeriod;
		}
	}
}
