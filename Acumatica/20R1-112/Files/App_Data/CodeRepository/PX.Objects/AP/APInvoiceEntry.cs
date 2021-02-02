using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Discount;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.BQLConstants;
using PX.Objects.EP;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.DR;
using PX.Objects.AR;
using PX.TM;

using AP1099Hist = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Hist;
using AP1099Yr = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Yr;
using PX.Objects.GL.Reclassification.UI;
using Branch = PX.Objects.GL.Branch;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AP.BQL;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.Extensions.CostAccrual;

namespace PX.Objects.AP
{ 
	[Serializable]
	public class APInvoiceEntry : APDataEntryGraph<APInvoiceEntry, APInvoice>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
        #region Extensions

	    public class APInvoiceEntryDocumentExtension : InvoiceGraphExtension<APInvoiceEntry>
	    {
	        public override PXSelectBase<Location> Location => Base.location;

			public override void SuppressApproval()
			{
				Base.Approval.SuppressApproval = true;
			}

			public override PXSelectBase<CurrencyInfo> CurrencyInfo => Base.currencyinfo;

            public override void Initialize()
	        {
	            base.Initialize();

	            Documents = new PXSelectExtension<Invoice>(Base.Document);
	            Lines = new PXSelectExtension<DocumentLine>(Base.AllTransactions);
	            InvoiceTrans = new PXSelectExtension<InvoiceTran>(Base.Transactions);
	            TaxTrans = new PXSelectExtension<GenericTaxTran>(Base.Taxes);
	            LineTaxes = new PXSelectExtension<LineTax>(Base.Tax_Rows);
                AppliedAdjustments = new PXSelectExtension<Adjust2>(Base.Adjustments);
            }

            #region Mapping

            protected override InvoiceMapping GetDocumentMapping()
            {
                return new InvoiceMapping(typeof(APInvoice))
                {
                    HeaderTranPeriodID = typeof(APInvoice.tranPeriodID),
                    HeaderDocDate = typeof(APInvoice.docDate),
                    ContragentID = typeof(APInvoice.vendorID),
                    ContragentLocationID = typeof(APInvoice.vendorLocationID),
                };
            }

            protected override DocumentLineMapping GetDocumentLineMapping()
	        {
	            return new DocumentLineMapping(typeof(APTran));
	        }

	        protected override ContragentMapping GetContragentMapping()
	        {
	            return new ContragentMapping(typeof(Vendor));
	        }

            protected override InvoiceTranMapping GetInvoiceTranMapping()
	        {
	            return new InvoiceTranMapping(typeof(APTran));
	        }

	        protected override GenericTaxTranMapping GetGenericTaxTranMapping()
	        {
	            return new GenericTaxTranMapping(typeof(APTaxTran));
	        }

	        protected override LineTaxMapping GetLineTaxMapping()
	        {
	            return new LineTaxMapping(typeof(APTax));
	        }

            protected override Adjust2Mapping GetAdjust2Mapping()
            {
                return new Adjust2Mapping(typeof(APAdjust));
            }

            #endregion

            protected override bool ShouldUpdateAdjustmentsOnDocumentUpdated(Events.RowUpdated<Invoice> e)
	        {
	            return base.ShouldUpdateAdjustmentsOnDocumentUpdated(e)
	                   || !e.Cache.ObjectsEqual<Invoice.moduleAccountID, Invoice.moduleSubID, Invoice.branchID>(e.Row, e.OldRow);
            }
	    }

		public class CostAccrual : NonStockAccrualGraph<APInvoiceEntry, APInvoice>
		{
			[PXOverride]
			public virtual void SetExpenseAccount(PXCache sender, PXFieldDefaultingEventArgs e, Action<PXCache, PXFieldDefaultingEventArgs> baseMethod)
			{
				APTran row = (APTran)e.Row;

				if (row != null && row.AccrueCost == true)
				{
					SetExpenseAccountSub(sender, e, row.InventoryID, row.SiteID,
					GetAccountSubUsingPostingClass: (InventoryItem inItem, INSite inSite, INPostClass inPostClass) =>
					{
						return INReleaseProcess.GetAcctID<INPostClass.invtAcctID>(Base, inPostClass.InvtAcctDefault, inItem, inSite, inPostClass);
					},
					GetAccountSubFromItem: (InventoryItem inItem) =>
					{
						return inItem.InvtAcctID;
					});
				}
			}

			[PXOverride]
			public virtual object GetExpenseSub(PXCache sender, PXFieldDefaultingEventArgs e, Func<PXCache, PXFieldDefaultingEventArgs, object> baseMethod)
			{
				APTran row = (APTran)e.Row;

				object expenseAccountSub = null;

				if (row != null && row.AccrueCost == true)
				{
					expenseAccountSub = GetExpenseAccountSub(sender, e, row.InventoryID, row.SiteID,
					GetAccountSubUsingPostingClass: (InventoryItem inItem, INSite inSite, INPostClass inPostClass) =>
					{
						return INReleaseProcess.GetSubID<INPostClass.invtSubID>(Base, inPostClass.InvtAcctDefault, inPostClass.InvtSubMask, inItem, inSite, inPostClass);
					},
					GetAccountSubFromItem: (InventoryItem inItem) =>
					{
						return inItem.InvtSubID;
					});
				}

				return expenseAccountSub;
			}
		}

		#endregion

			public bool IsReverseContext = false;

		// used to not recalculate line amount in context of creating PPDDebitAdj
		// normally, APTran.curyLineAmt is recalculated on APTran_TaxCategoryID_FieldUpdated,
		// when Net-Gross feature is on and TaxCalcMode = Net and RequireControlTotal == true and APTran.Qty == 0
		internal bool IsPPDCreateContext = false;

		private DiscountEngine<APTran, APInvoiceDiscountDetail> _discountEngine => DiscountEngineProvider.GetEngineFor<APTran, APInvoiceDiscountDetail>();

		#region Internal Definitions + Cache Attached Events 
		#region InventoryItem
		#region COGSSubID
		[PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
		[SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
		public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion     
		#region APTran

		[Branch(typeof(APRegister.branchID), Enabled = false)]
		protected virtual void APTaxTran_BranchID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region APInvoice
        [PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[VendorActiveOrHoldPayments(
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName),
			CacheGlobal = true,
			Filterable = true)]
		[PXDefault]
		protected virtual void APInvoice_VendorID_CacheAttached(PXCache sender) { }


		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Original Document")]
		protected virtual void APInvoice_OrigRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXFormula(typeof(
			IIf<Where<Current<APInvoice.docType>, NotEqual<APDocType.debitAdj>,
					And<Current<APInvoice.docType>, NotEqual<APDocType.prepayment>>>,
				Selector<APInvoice.vendorID, Vendor.termsID>,
				Null>))]
		protected virtual void APInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDefault(typeof(Where2<FeatureInstalled<FeaturesSet.paymentsByLines>,
			And<APInvoice.docType, NotEqual<APDocType.prepayment>,
			And<APInvoice.origModule, NotEqual<BatchModule.moduleTX>,
			And<APInvoice.origModule, NotEqual<BatchModule.moduleEP>,
			And<APInvoice.isTaxDocument, NotEqual<True>,
			And<APInvoice.isMigratedRecord, NotEqual<True>,
			And<APInvoice.pendingPPD, NotEqual<True>,
			And<APInvoice.docType, NotEqual<APDocType.debitAdj>,
			And<Current<Vendor.paymentsByLinesAllowed>, Equal<True>>>>>>>>>>))]
		protected virtual void APInvoice_PaymentsByLinesAllowed_CacheAttached(PXCache sender) { }
		#endregion

		#endregion

		#region EP Approval Defaulting
		[PXDBDate]
		[PXDefault(typeof(APInvoice.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(APInvoice.vendorID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(APInvoice.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(APInvoice.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(APInvoice.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(APInvoice.curyOrigDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(APInvoice.origDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		protected virtual void EPApproval_SourceItemType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = APDocTypeDict[Document.Current.DocType];

				e.Cancel = true;
			}		
		}
		#endregion

		#region EP Approval Actions

		public PXAction<APInvoice> approve;
		public PXAction<APInvoice> reject;

		[PXUIField(DisplayName = "Approve", Visible = false, MapEnableRights = PXCacheRights.Select)]
		public IEnumerable Approve(PXAdapter adapter)
		{
			IEnumerable<APInvoice> invoices = adapter.Get<APInvoice>().ToArray();

			if (IsDirty)
			Save.Press();

			foreach (APInvoice invoice in invoices)
			{
				invoice.Approved = true;
				Document.Update(invoice);

				Save.Press();

				yield return invoice;
			}
		}

		[PXUIField(DisplayName = "Reject", Visible = false, MapEnableRights = PXCacheRights.Select)]
		public IEnumerable Reject(PXAdapter adapter)
		{
			IEnumerable<APInvoice> invoices = adapter.Get<APInvoice>().ToArray();

			if (IsDirty)
			Save.Press();

			foreach (APInvoice invoice in invoices)
			{
				invoice.Rejected = true;
				Document.Update(invoice);

				Save.Press();

				yield return invoice;
			}
		}

		#endregion

		#region Buttons
		public ToggleCurrency<APInvoice> CurrencyView;

		public static void ViewScheduleForLine(PXGraph graph, APInvoice document, APTran tran)
		{
			PXSelectBase<DRSchedule> correspondingScheduleView = new PXSelect<
				DRSchedule,
				Where<
					DRSchedule.module, Equal<BatchModule.moduleAP>,
					And<DRSchedule.docType, Equal<Current<APTran.tranType>>,
					And<DRSchedule.refNbr, Equal<Current<APTran.refNbr>>,
					And<DRSchedule.lineNbr, Equal<Current<APTran.lineNbr>>>>>>>
				(graph);

			DRSchedule correspondingSchedule = correspondingScheduleView.Select();

			if (correspondingSchedule == null || correspondingSchedule.IsDraft == true)
			{
				var expensePostingAmount = APReleaseProcess.GetExpensePostingAmount(graph, tran);

				DRDeferredCode deferralCode = PXSelect<
					DRDeferredCode, 
					Where<
						DRDeferredCode.deferredCodeID, Equal<Current2<APTran.deferredCode>>>>
					.Select(graph);

				if (deferralCode != null)
				{
					DRProcess process = PXGraph.CreateInstance<DRProcess>();
					process.CreateSchedule(tran, deferralCode, document, expensePostingAmount.Base.Value, isDraft: true);
					process.Actions.PressSave();

					correspondingScheduleView.Cache.Clear();
					correspondingScheduleView.Cache.ClearQueryCacheObsolete();
					correspondingScheduleView.View.Clear();

					correspondingSchedule = correspondingScheduleView.Select();
					}
				}

			if (correspondingSchedule != null)
				{
					DraftScheduleMaint target = PXGraph.CreateInstance<DraftScheduleMaint>();
					target.Clear();
				target.Schedule.Current = correspondingSchedule;

				throw new PXRedirectRequiredException(target, true, nameof(ViewSchedule)) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
		}

		public PXAction<APInvoice> viewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Settings)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			APTran currentLine = Transactions.Current;

			if (currentLine != null && 
				Transactions.Cache.GetStatus(currentLine) == PXEntryStatus.Notchanged)
			{
				Save.Press();
				ViewScheduleForLine(this, Document.Current, Transactions.Current);
			}

			return adapter.Get();
		}


		public PXAction<APInvoice> newVendor;
		[PXUIField(DisplayName = "New Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewVendor(PXAdapter adapter)
		{
			VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
			throw new PXRedirectRequiredException(graph, "New Vendor");
		}

		public PXAction<APInvoice> editVendor;
		[PXUIField(DisplayName = "Edit Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable EditVendor(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
				graph.BAccount.Current = (VendorR)vendor.Current;
				throw new PXRedirectRequiredException(graph, "Edit Vendor");
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> vendorDocuments;
		[PXUIField(DisplayName = "Vendor Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable VendorDocuments(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				graph.Filter.Current.VendorID = vendor.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Vendor Details");
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false, 
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
		
			//TODO: This block should be removed after partially deductible taxes' support.
			if (Document.Current.DocType == APDocType.Invoice)
			{
				foreach (PXResult<APTaxTran, Tax> res in Taxes.Select())
				{
					Tax tax = res;
					APTaxTran apTaxTran = res;
					if (tax.DeductibleVAT == true && tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
					{
						throw new PXException(Messages.DeductiblePPDTaxProhibitedForReleasing);
					}
				}
			}
			foreach (APInvoice apdoc in adapter.Get<APInvoice>())
			{
				OnBeforeRelease(apdoc);

				if (!(bool)apdoc.Hold && !(bool)apdoc.Released)
				{
					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}

				Save.Press();
				PXLongOperation.StartOperation(this, delegate() { APDocumentRelease.ReleaseDoc(list, false); });

			return list;
				}

		public virtual APRegister OnBeforeRelease(APRegister doc)
					{
			return doc;
		}

		public PXAction<APInvoice> prebook;
		[PXUIField(DisplayName = "Pre-release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable Prebook(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();

			foreach (APInvoice apdoc in adapter.Get<APInvoice>())
			{
				if (!apdoc.Hold.Value && !apdoc.Released.Value && apdoc.Prebooked != true)
				{
					if (apdoc.PrebookAcctID == null)
					{
						cache.RaiseExceptionHandling<APInvoice.prebookAcctID>(apdoc, apdoc.PrebookAcctID, new PXSetPropertyException(Messages.PrebookingAccountIsRequiredForPrebooking)); 
						continue;
					}

					if (apdoc.PrebookSubID == null)
					{
						cache.RaiseExceptionHandling<APInvoice.prebookSubID>(apdoc, apdoc.PrebookSubID, new PXSetPropertyException(Messages.PrebookingAccountIsRequiredForPrebooking));
						continue;
					}						

					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}

			if (list.Count == 0)
			{
				throw new PXException(ErrorMessages.RecordRaisedErrors, Messages.Updating, this.Document.View.CacheGetItemType().Name);
			}

			Persist();
			PXLongOperation.StartOperation(this, delegate { APDocumentRelease.ReleaseDoc(list, false, true); });
			return list;
		}

		public PXAction<APInvoice> voidInvoice;
		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable VoidInvoice(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
			foreach (APInvoice apdoc in adapter.Get<APInvoice>())
			{
				if (apdoc.Released == true || apdoc.Prebooked == true)
				{
					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}

			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Persist();
			PXLongOperation.StartOperation(this, delegate() { APDocumentRelease.VoidDoc(list); });
			return list;
		}

		public static readonly Dictionary<string, string> APDocTypeDict = new APDocType.ListAttribute().ValueLabelDic;
		
		public virtual void ClearRetainageSummary(APInvoice document)
		{
			document.CuryLineRetainageTotal = 0m;
			document.CuryRetainageTotal = 0m;
			document.CuryRetainageUnreleasedAmt = 0m;
			document.CuryRetainedTaxTotal = 0m;
			document.CuryRetainedDiscTotal = 0m;
		}

		public PXAction<APInvoice> vendorRefund;
		[PXUIField(DisplayName = "Vendor Refund", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable VendorRefund(PXAdapter adapter)
		{
			if (Document.Current != null && (bool)Document.Current.Released && Document.Current.DocType == APDocType.DebitAdj && (bool)Document.Current.OpenDoc)
			{
				APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();

				APAdjust adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
					And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>, And<APAdjust.released, Equal<False>>>>>.Select(this);

				if (adj != null)
				{
					pe.Document.Current = pe.Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
				}
				else
				{
					pe.Clear();
					pe.CreatePayment(Document.Current);
				}
				throw new PXRedirectRequiredException(pe, "PayInvoice");
			}
			return adapter.Get();
		}

		/// <summary>
		/// Check if reversing retainage document already exists.
		/// </summary>
		/// <param name="origDoc">The original document.</param>
		/// <param name="errorMessage">Displayed message if reversing document already exists.</param>
		public virtual bool CheckReversingRetainageDocumentAlreadyExists(APInvoice origDoc, out APRegister reversingDoc)
		{
			reversingDoc = PXSelect<APRegister,
				Where<APRegister.docType, Equal<Required<APRegister.docType>>,
					And<APRegister.origDocType, Equal<Required<APRegister.origDocType>>,
					And<APRegister.origRefNbr, Equal<Required<APRegister.origRefNbr>>>>>,
				OrderBy<Desc<APRegister.createdDateTime>>>
				.SelectSingleBound(this, null, APDocType.DebitAdj, origDoc.DocType, origDoc.RefNbr);

			return 
				reversingDoc != null &&
				(reversingDoc.IsOriginalRetainageDocument() == origDoc.IsOriginalRetainageDocument() ||
					reversingDoc.IsChildRetainageDocument() == origDoc.IsChildRetainageDocument());
		}

		public PXAction<APInvoice> reverseInvoice;

		[PXUIField(DisplayName = Messages.APReverseBill, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReverseInvoice(PXAdapter adapter)
		{
			APInvoice origDoc = Document.Current;

			if (origDoc != null && (origDoc.DocType == APDocType.Invoice ||
				origDoc.DocType == APDocType.CreditAdj))
			{
				if (origDoc.InstallmentNbr != null && string.IsNullOrEmpty(origDoc.MasterRefNbr) == false)
				{
					throw new PXSetPropertyException(Messages.Multiply_Installments_Cannot_be_Reversed, origDoc.MasterRefNbr);
				}

				if (origDoc.IsOriginalRetainageDocument() ||
					origDoc.IsChildRetainageDocument())
			{
					// Verify the case when unreleased retainage
					// document exists.
					// 
					APRetainageInvoice retainageDoc = RetainageDocuments
						.Select()
						.RowCast<APRetainageInvoice>()
						.FirstOrDefault(row => row.Released != true);

					if (retainageDoc != null)
				{
						throw new PXException(
							Messages.ReverseRetainageNotReleasedDocument,
							PXMessages.LocalizeNoPrefix(APDocTypeDict[retainageDoc.DocType]),
							retainageDoc.RefNbr,
							PXMessages.LocalizeNoPrefix(APDocTypeDict[origDoc.DocType]));
					}

					// Verify the case when released retainage
					// document exists or payments applied.
					// 
					APAdjust adj =
						PXSelect<APAdjust,
						Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
							And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>,
							And<APAdjust.voided, Equal<False>>>>>
						.SelectSingleBound(this, null);

					bool hasPaymentsApplied = adj != null;

					if (origDoc.IsOriginalRetainageDocument() &&
						origDoc.CuryRetainageTotal != origDoc.CuryRetainageUnreleasedAmt ||
						hasPaymentsApplied)
					{
						throw new PXException(
							Messages.HasPaymentsOrDebAdjCannotBeReversed,
							PXMessages.LocalizeNoPrefix(APDocTypeDict[origDoc.DocType]),
							origDoc.RefNbr);
					}

					// Verify the case when reversing retainage
					// document exists.
					// 
					APRegister reversingDoc;
					if (CheckReversingRetainageDocumentAlreadyExists(origDoc, out reversingDoc))
					{
						throw new PXException(
							Messages.ReversingRetainageDocumentExists,
							PXMessages.LocalizeNoPrefix(APDocTypeDict[origDoc.DocType]),
							origDoc.RefNbr,
							PXMessages.LocalizeNoPrefix(APDocTypeDict[reversingDoc.DocType]), 
							reversingDoc.RefNbr);
					}
				}

				this.Save.Press();
				bool isPOReferenced = false;
				foreach (APTran itr in this.Transactions.Select())
				{
					if (!string.IsNullOrEmpty(itr.ReceiptNbr) || !string.IsNullOrEmpty(itr.PONbr))
					{
						isPOReferenced = true;
						break;
					}
				}
				if (isPOReferenced)
				{
					this.Document.Ask(Messages.Warning, Messages.DebitAdjustmentRowReferecesPOOrderOrPOReceipt, MessageButtons.OK, MessageIcon.Warning);
				}
				APInvoice doc = PXCache<APInvoice>.CreateCopy(Document.Current);
				_finPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<APInvoice.finPeriodID, APInvoice.branchID>(Document.Cache, doc, finperiod, typeof(OrganizationFinPeriod.aPClosed));

                try
				{
					IsReverseContext = true;
					
					this.ReverseInvoiceProc(doc);

					Document.Cache.RaiseExceptionHandling<APInvoice.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

                    return new List<APInvoice> { Document.Current };
				}
				catch (PXException)
				{
					this.Clear(PXClearOption.PreserveTimeStamp);
					Document.Current = doc;
					throw;
				}
				finally
				{
					IsReverseContext = false;
				}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> reclassifyBatch;
		[PXUIField(DisplayName = Messages.ReclassifyGLBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReclassifyBatch(PXAdapter adapter)
		{
			var document = Document.Current;

			if (document != null)
			{
				ReclassifyTransactionsProcess.TryOpenForReclassificationOfDocument(Document.View, 
					BatchModule.AP, document.BatchNbr, document.DocType, document.RefNbr);
			}

			return adapter.Get();
		}

		public PXAction<APInvoice> payInvoice;
		[PXUIField(DisplayName = Messages.APPayBill, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable PayInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.Released == true || Document.Current.Prebooked == true))
			{
				APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();
				if (Document.Current.OpenDoc == true && (Document.Current.Payable == true || Document.Current.DocType == APInvoiceType.Prepayment))
				{
					if (Document.Current.PendingPPD == true)
					{
						throw new PXSetPropertyException(Messages.PaidPPD);
					}

					string adjRefNbr = null;
					string adjDocType = null;

					APAdjust adj = PXSelect<APAdjust, 
						Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
							And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>>>,
						OrderBy<Asc<APAdjust.released>>>.Select(this);

					if (adj != null)
					{
						if (adj.Released == true)
						{
							if 
							(	adj.AdjdDocType == APDocType.Prepayment &&
								PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>,
									And<APPayment.docType, Equal<Required<APPayment.docType>>>>>.Select(this, Document.Current.RefNbr, Document.Current.DocType).Count > 0)
							{
								adjRefNbr = Document.Current.RefNbr;
								adjDocType = Document.Current.DocType;
							}
						}
						else
						{
							adjRefNbr = adj.AdjgRefNbr;
							adjDocType = adj.AdjgDocType;
						}
					}

					if (adjRefNbr != null && adjDocType != null)
					{
						pe.Document.Current = pe.Document.Search<APPayment.refNbr>(adjRefNbr, adjDocType);
					}
					else
					{
						pe.Clear();
						pe.CreatePayment(Document.Current);
					}
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
				else if (Document.Current.DocType == APDocType.DebitAdj)
				{
					pe.Document.Current = pe.Document.Search<APPayment.refNbr>(Document.Current.RefNbr, Document.Current.DocType);
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> createSchedule;
		[PXUIField(DisplayName = "Assign to Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ImageKey = PX.Web.UI.Sprite.Main.Shedule)]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CreateSchedule(PXAdapter adapter)
		{
			APInvoice currentDocument = Document.Current;

			if (currentDocument == null) return adapter.Get();

			Save.Press();

			IsSchedulable<APRegister>.Ensure(this, currentDocument);
			
			APScheduleMaint scheduleMaint = PXGraph.CreateInstance<APScheduleMaint>();

			if (currentDocument.Scheduled == true && currentDocument.ScheduleID != null)
			{
				scheduleMaint.Schedule_Header.Current = 
					scheduleMaint.Schedule_Header.Search<Schedule.scheduleID>(currentDocument.ScheduleID);
			}
			else
			{
				scheduleMaint.Schedule_Header.Cache.Insert();

				APRegister scheduledDocumentRecord = 
					scheduleMaint.Document_Detail.Cache.CreateInstance() as APRegister;

				PXCache<APRegister>.RestoreCopy(scheduledDocumentRecord, currentDocument);

				scheduledDocumentRecord = 
					scheduleMaint.Document_Detail.Cache.Update(scheduledDocumentRecord) as APRegister;
			}

			throw new PXRedirectRequiredException(scheduleMaint, "Create Schedule");
		}

		public virtual void AddLandedCosts(IEnumerable<POLandedCostDetailS> details)
		{
			var detailGroups = details.GroupBy(t => new { t.DocType, t.RefNbr });
			foreach (var detailGroup in detailGroups)
			{
				var landedCostDoc = PXSelect<POLandedCostDoc,
						Where<POLandedCostDoc.docType, Equal<Required<POLandedCostDoc.docType>>,
						And<POLandedCostDoc.refNbr, Equal<Required<POLandedCostDoc.refNbr>>>>>
						.Select(this, detailGroup.Key.DocType, detailGroup.Key.RefNbr);

				var landedCostDetails = PXSelect<POLandedCostDetail,
						Where<POLandedCostDetail.docType, Equal<Required<POLandedCostDetail.docType>>,
							And<POLandedCostDetail.refNbr, Equal<Required<POLandedCostDetail.refNbr>>,
							And<POLandedCostDetail.lineNbr, In<Required<POLandedCostDetail.lineNbr>>>>>>
					.Select(this, detailGroup.Key.DocType, detailGroup.Key.RefNbr, detailGroup.Select(t => t.LineNbr).ToArray())
					.RowCast<POLandedCostDetail>()
					.ToList();

				decimal mult = Document.Current.DrCr == GL.DrCr.Debit ? 1m : -1m;
				var landedCostApBillFactory = GetLandedCostApBillFactory();
				var transctions = landedCostApBillFactory.CreateTransactions(landedCostDoc, landedCostDetails, mult);

				transctions.ForEach(tran =>
				{
					var insertedTran = Transactions.Insert(tran);
					LandedCostDetailSetLink(insertedTran);
				});
			}
		}

		protected virtual POLandedCostDetail GetLandedCostDetail(string docType, string refNbr, int lineNbr)
		{
			var result =
				(POLandedCostDetail)PXSelect<POLandedCostDetail,
					Where<POLandedCostDetail.docType, Equal<Required<POLandedCostDetail.docType>>,
						And<POLandedCostDetail.refNbr, Equal<Required<POLandedCostDetail.refNbr>>,
						And<POLandedCostDetail.lineNbr, Equal<Required<POLandedCostDetail.lineNbr>>
						>>>>.Select(this, docType, refNbr, lineNbr);

			return result;
		}

		public virtual void LandedCostDetailSetLink(APTran tran)
		{
			if (tran != null && tran.LCDocType != null && tran.LCRefNbr != null && tran.LCLineNbr != null)
			{
				var landedCostDetail = GetLandedCostDetail(tran.LCDocType, tran.LCRefNbr, tran.LCLineNbr.Value);

				landedCostDetail.APDocType = tran.TranType;
				landedCostDetail.APRefNbr = tran.RefNbr;

				PXParentAttribute.SetParent(this.Caches<POLandedCostDetail>(), landedCostDetail, typeof(APInvoice), Document.Current);

				this.Caches<POLandedCostDetail>().SetStatus(landedCostDetail, PXEntryStatus.Updated);
					}
			}

		public virtual void LandedCostDetailClearLink(APTran tran)
		{
			if (Document.Current != null && tran != null && tran.LCDocType != null && tran.LCRefNbr != null && tran.LCLineNbr != null)
			{
				var landedCostDetail = GetLandedCostDetail(tran.LCDocType, tran.LCRefNbr, tran.LCLineNbr.Value);
				if (landedCostDetail.APDocType != Document.Current.DocType || landedCostDetail.APRefNbr != Document.Current.RefNbr)
					return;

				landedCostDetail.APDocType = null;
				landedCostDetail.APRefNbr = null;

				PXParentAttribute.SetParent(this.Caches<POLandedCostDetail>(), landedCostDetail, typeof(APInvoice), null);

				this.Caches<POLandedCostDetail>().SetStatus(landedCostDetail, PXEntryStatus.Updated);
				}
			}

		public virtual void LinkLandedCostDetailLine(APInvoice doc, APTran apTran, POLandedCostDetailS detail)
		{
			if (doc == null || apTran == null || detail == null)
				return;

			apTran.ReceiptType = null;
			apTran.ReceiptNbr = null;
			apTran.ReceiptLineNbr = null;
			apTran.POOrderType = null;
			apTran.PONbr = null;
			apTran.POLineNbr = null;
			apTran.AccountID = null;
			apTran.SubID = null;
			apTran.UOM = null;
			apTran.Qty = Decimal.One;
			apTran.CuryUnitCost = apTran.CuryLineAmt;
			apTran.LCDocType = null;
			apTran.LCRefNbr = null;
			apTran.LCLineNbr = null;
			apTran.LandedCostCodeID = null;

			LandedCostCode aLCCode = LandedCostCode.PK.Find(this, detail.LandedCostCodeID);

			apTran.AccountID = aLCCode.LCAccrualAcct;
			apTran.SubID = aLCCode.LCAccrualSub;

			apTran.TranDesc = detail.Descr;
			apTran.TaxCategoryID = detail.TaxCategoryID;
			apTran.LCDocType = detail.DocType;
			apTran.LCRefNbr = detail.RefNbr;
			apTran.LCLineNbr = detail.LineNbr;
			apTran.LandedCostCodeID = detail.LandedCostCodeID;

			Transactions.Cache.Update(apTran);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		public void checkTaxCalcMode()
		{
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		public void updateTaxCalcMode()
		{
		}


		public PXAction<APInvoice> viewPODocument;
		[PXUIField(DisplayName = PO.Messages.ViewPODocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ViewPODocument(PXAdapter adapter)
		{
			if (this.Transactions.Current != null)
			{
				APTran row = this.Transactions.Current;
				if (!String.IsNullOrEmpty(row.ReceiptNbr))
				{
					POReceiptEntry docGraph = PXGraph.CreateInstance<POReceiptEntry>();
					docGraph.Document.Current = docGraph.Document.Search<POReceipt.receiptNbr>(row.ReceiptNbr);
					if (docGraph.Document.Current != null)
						throw new PXRedirectRequiredException(docGraph, "View PO Receipt");
				}
				else
					if (!(String.IsNullOrEmpty(row.POOrderType) || String.IsNullOrEmpty(row.PONbr)))
					{
						POOrderEntry docGraph = PXGraph.CreateInstance<POOrderEntry>();
						docGraph.Document.Current = docGraph.Document.Search<POOrder.orderNbr>(row.PONbr, row.POOrderType);
						if (docGraph.Document.Current != null)
							throw new PXRedirectRequiredException(docGraph, "View PO Order");
					}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> ViewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Document.Current.OrigDocType, Document.Current.OrigRefNbr, Document.Current.OrigModule);
			return adapter.Get();
		}

		public PXAction<APInvoice> autoApply;
		[PXUIField(DisplayName = "Auto Apply", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AutoApply(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.DocType == APDocType.Invoice
				&& Document.Current.Released == false &&
				this.Document.Current.Prebooked == false)
			{
				foreach (PXResult<APAdjust, APPayment, Standalone.APRegisterAlias, CurrencyInfo> res in Adjustments_Raw.Select())
				{
					APAdjust adj = res;

					adj.CuryAdjdAmt = 0m;
					Adjustments_Raw.Cache.MarkUpdated(adj);
				}

				decimal? CuryApplAmt = Document.Current.CuryDocBal;

				foreach (PXResult<APAdjust, APPayment> res in Adjustments.Select())
				{
					APAdjust adj = (APAdjust)res;

					if (adj.CuryDocBal > 0m)
					{
						if (adj.CuryDocBal > CuryApplAmt)
						{
							adj.CuryAdjdAmt = CuryApplAmt;
							CuryApplAmt = 0m;
							Adjustments.Cache.RaiseFieldUpdated<APAdjust.curyAdjdAmt>(adj, 0m);
							Adjustments.Cache.MarkUpdated(adj);
							break;
						}
						else
						{
							adj.CuryAdjdAmt = adj.CuryDocBal;
							CuryApplAmt -= adj.CuryDocBal;
							Adjustments.Cache.RaiseFieldUpdated<APAdjust.curyAdjdAmt>(adj, 0m);
							Adjustments.Cache.MarkUpdated(adj);
						}
					}
				}
				Adjustments.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> voidDocument;
		[PXUIField(DisplayName = Messages.Void, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable VoidDocument(PXAdapter adapter)
		{
			APRegister doc = (APRegister)Document.Current;
			if (doc != null && doc.DocType == APInvoiceType.Prepayment)
			{
				APAdjust adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
													And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>>>>
							   .Select(this, doc.DocType, doc.RefNbr);

				if (adj != null)
					throw new PXException(
						adj.Released == true ?
							Messages.PrepaymentCannotBeVoidedDueToReleasedCheck :
							Messages.PrepaymentCannotBeVoidedDueToUnreleasedCheck,
						adj.AdjgRefNbr);

				this.VoidPrepayment(doc);

				List<APInvoice> rs = new List<APInvoice>();
				rs.Add(Document.Current);
				return rs;
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> viewPayment;
		[PXUIField(
			DisplayName = "View Document", 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewPayment(PXAdapter adapter)
		{
			if (Document.Current != null && Adjustments.Current != null)
			{
				switch(Adjustments.Current.AdjType)
				{
					case AR.ARAdjust.adjType.Adjusting:
					{
						APPaymentEntry pe = CreateInstance<APPaymentEntry>();
				pe.Document.Current = pe.Document.Search<APPayment.refNbr>(Adjustments.Current.AdjgRefNbr, Adjustments.Current.AdjgDocType);

						throw new PXRedirectRequiredException(pe, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
					case AR.ARAdjust.adjType.Adjusted:
					{
						APInvoiceEntry pe = CreateInstance<APInvoiceEntry>();
						pe.Document.Current = pe.Document.Search<APInvoice.refNbr>(Adjustments.Current.AdjdRefNbr, Adjustments.Current.AdjdDocType);

						throw new PXRedirectRequiredException(pe, true, "Invoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> viewItem;
		[PXUIField(
			DisplayName = "View Item",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewItem(PXAdapter adapter)
		{
			if (Transactions.Current != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID,
					Equal<Current<APTran.inventoryID>>>>.SelectSingleBound(this, null);
				if (item != null)
				{
					PXRedirectHelper.TryRedirect(Caches[typeof(InventoryItem)], item, "View Item", PXRedirectHelper.WindowMode.NewWindow);
				}
			}

			return adapter.Get();
		}

		public PXAction<APInvoice> recalculateDiscountsAction;
		[PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
		{
			if (adapter.MassProcess)
			{
				PXLongOperation.StartOperation(this, delegate ()
				{
					_discountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, DiscountDetails, Document.Current.VendorLocationID, Document.Current.DocDate, recalcdiscountsfilter.Current, DiscountEngine.DefaultAPDiscountCalculationParameters);
					this.Save.Press();
				});
			}
			else if (recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
			{
				_discountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, DiscountDetails, Document.Current.VendorLocationID, Document.Current.DocDate, recalcdiscountsfilter.Current, DiscountEngine.DefaultAPDiscountCalculationParameters);
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> recalcOk;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable RecalcOk(PXAdapter adapter)
		{
			return adapter.Get();
		}

		#endregion

		#region Selects
		public PXSelect<
			InventoryItem, 
			Where<
				InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>> 
			nonStockItem;

		[PXCopyPasteHiddenFields(typeof(APInvoice.invoiceNbr), FieldsToShowInSimpleImport = new[] { typeof(APInvoice.invoiceNbr) })]
		[PXViewName(Messages.APInvoice)]
		public PXSelectJoin<
			APInvoice, 
					LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APInvoice.vendorID>>>,
			Where<
				APInvoice.docType, Equal<Optional<APInvoice.docType>>,
				And2<Where<
					APInvoice.origModule, NotEqual<BatchModule.moduleTX>, 
					Or<APInvoice.released, Equal<True>>>,
				And<Where<
					Vendor.bAccountID, IsNull, 
					Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>> 
			Document;

		[PXCopyPasteHiddenFields(typeof(APInvoice.paySel), typeof(APInvoice.payDate))]
		public PXSelect<APInvoice, Where<APInvoice.docType, Equal<Current<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Current<APInvoice.refNbr>>>>> CurrentDocument;

		[PXCopyPasteHiddenView]
		public PXSelect<APTran,
			Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
				And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>> AllTransactions;

		[PXImport(typeof(APInvoice))]
		[PXCopyPasteHiddenFields(
			typeof(APTran.pOOrderType),
			typeof(APTran.pONbr),
			typeof(APTran.pOLineNbr),
			typeof(APTran.receiptNbr),
			typeof(APTran.receiptLineNbr),
			typeof(APTran.pPVDocType),
			typeof(APTran.pPVRefNbr))]
		[PXViewName(Messages.APTran)]
		public PXSelect<
			APTran,
			Where<
				APTran.tranType, Equal<Current<APInvoice.docType>>,
				And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
				And<APTran.lineType, NotEqual<SOLineType.discount>>>>,
			OrderBy<
				Asc<APTran.tranType,
				Asc<APTran.refNbr,
				Asc<APTran.lineNbr>>>>>
			Transactions;

		public PXSelectJoin<
			APTran, 
				LeftJoin<POLine, 
					On<POLine.orderType, Equal<APTran.pOOrderType>,
										And<POLine.orderNbr, Equal<APTran.pONbr>,
										And<POLine.lineNbr, Equal<APTran.pOLineNbr>>>>>,
			Where<
				APTran.tranType, Equal<Current<APInvoice.docType>>,
				And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>,
			OrderBy<
				Asc<APTran.tranType, 
				Asc<APTran.refNbr, 
				Asc<APTran.lineNbr>>>>>
		TransactionsPOLine;

		public PXSelect<
			APTran, 
			Where<
				APTran.tranType, Equal<Current<APInvoice.docType>>, 
				And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>, 
				And<APTran.lineType, Equal<SOLineType.discount>>>>, 
			OrderBy<
				Asc<APTran.tranType, 
				Asc<APTran.refNbr, 
				Asc<APTran.lineNbr>>>>> 
			Discount_Row;

		[PXCopyPasteHiddenView]
		public PXSelect<
			APTax, 
			Where<
				APTax.tranType, Equal<Current<APInvoice.docType>>, 
				And<APTax.refNbr, Equal<Current<APInvoice.refNbr>>>>, 
			OrderBy<
				Asc<APTax.tranType, 
				Asc<APTax.refNbr, Asc<APTax.taxID>>>>> 
			Tax_Rows;

		[PXCopyPasteHiddenView]
        public PXSelectJoin<APTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>,
			Where<APTaxTran.module, Equal<BatchModule.moduleAP>,
				And<APTaxTran.tranType, Equal<Current<APInvoice.docType>>,
				And<APTaxTran.refNbr, Equal<Current<APInvoice.refNbr>>>>>> Taxes;

		// We should use read only view here
		// to prevent cache merge because it
		// used only as a shared BQL query.
		// 
		[PXCopyPasteHiddenView]
		public PXSelectReadonly2< 
			APTaxTran, 
				LeftJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, 
			Where<
				APTaxTran.module, Equal<BatchModule.moduleAP>, 
				And<APTaxTran.tranType, Equal<Current<APInvoice.docType>>, 
				And<APTaxTran.refNbr, Equal<Current<APInvoice.refNbr>>, 
				And<Tax.taxType, Equal<CSTaxType.use>>>>>> 
			UseTaxes;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<
			APAdjust, 
				InnerJoin<APPayment, 
					On<APPayment.docType, Equal<APAdjust.adjgDocType>,
					And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>>> 
			Adjustments;

		public PXSelectJoin<
			APAdjust, 
				InnerJoinSingleTable<APPayment, 
					On<APPayment.docType, Equal<APAdjust.adjgDocType>, 
				And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>, 
				InnerJoin<Standalone.APRegisterAlias,
					On<Standalone.APRegisterAlias.docType, Equal<APAdjust.adjgDocType>,
					And<Standalone.APRegisterAlias.refNbr, Equal<APAdjust.adjgRefNbr>>>,
				InnerJoin<CurrencyInfo, 
					On<CurrencyInfo.curyInfoID, Equal<Standalone.APRegisterAlias.curyInfoID>>>>>,
			Where<
				APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>, 
				And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>,
				And2<Where<
					Current<APInvoice.released>, Equal<True>, 
					Or<Current<APInvoice.prebooked>, Equal<True>, 
					Or<APAdjust.released, Equal<Current<APInvoice.released>>>>>,
				And<APAdjust.isInitialApplication, NotEqual<True>>>>>> 
			Adjustments_Raw;

		public PXSelect<
			APInvoiceDiscountDetail, 
			Where<
				APInvoiceDiscountDetail.docType, Equal<Current<APInvoice.docType>>, 
				And<APInvoiceDiscountDetail.refNbr, Equal<Current<APInvoice.refNbr>>>>, 
			OrderBy<Asc<APInvoiceDiscountDetail.orderType,
					Asc<APInvoiceDiscountDetail.orderNbr,
					Asc<APInvoiceDiscountDetail.receiptType,
					Asc<APInvoiceDiscountDetail.receiptNbr,
				    Asc<APInvoiceDiscountDetail.lineNbr>>>>>>>
			DiscountDetails;

		public PXSelect<
			CurrencyInfo, 
			Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>> 
			currencyinfo;

		public PXSetup<APSetup> APSetup;
		[PXViewName(Messages.Vendor)]
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<APInvoice.vendorID>>>> vendor;
		public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<APInvoice.vendorID>>>> EmployeeByVendor;

		[PXViewName(EP.Messages.Employee)]
		public PXSetup<EPEmployee, Where<EPEmployee.userID, Equal<Current<APInvoice.employeeID>>>> employee;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;
		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<APInvoice.taxZoneID>>>> taxzone;

		public PXSetup<
			Location, 
			Where<
				Location.bAccountID, Equal<Current<APInvoice.vendorID>>, 
				And<Location.locationID, Equal<Optional<APInvoice.vendorLocationID>>>>> 
			location;

		public PXSetup<
			OrganizationFinPeriod, 
			Where<OrganizationFinPeriod.finPeriodID, Equal<Current<APInvoice.finPeriodID>>,
                And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<APInvoice.branchID>>>>> 
			finperiod;

		[PXCopyPasteHiddenView()]
		public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

		public PXSelect<AP1099Hist> ap1099hist;
		public PXSelect<AP1099Yr> ap1099year;

		public PXSetup<GLSetup> glsetup;
		public PXSetupOptional<INSetup> insetup;
		public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetup<POSetup> posetup;

		public PXSelect<DRSchedule> dummySchedule_forPXParent;
		public PXSelect<DRScheduleDetail> dummyScheduleDetail_forPXParent;
		public PXSelect<DRScheduleTran> dummyScheduleTran_forPXParent;

		public PXSelect<EPExpenseClaim, 
			Where<EPExpenseClaim.refNbr, Equal<Current<APInvoice.origRefNbr>>,
			And<Current<APInvoice.origModule>, Equal<BatchModule.moduleEP>>>> expenseclaim;

		public PXFilter<DuplicateFilter> duplicatefilter;
		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;

		public PXSelect<
			APTran, 
			Where<
				APTran.refNbr, Equal<Optional<APTran.refNbr>>, 
				And<APTran.tranType, Equal<Optional<APTran.tranType>>>>> 
			siblingTrans;

		[CRReference(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>>>))]
		public CRActivityList<APInvoice> Activity;

        [InjectDependency]
        public IFinPeriodRepository FinPeriodRepository { get; set; }

        [Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
        public virtual IEnumerable transactions()
		{
			return null;
		}

		public virtual IEnumerable taxes()
		{
			bool hasPPDTaxes = false;
			bool vatReportingInstalled = PXAccess.FeatureInstalled<FeaturesSet.vATReporting>();

			APTaxTran aptaxMax = null;
			decimal? discountedTaxableTotal = 0m;
			decimal? discountedPriceTotal = 0m;

			foreach (PXResult<APTaxTran, Tax> res in PXSelectJoin<APTaxTran,
				LeftJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>,
				Where<APTaxTran.module, Equal<BatchModule.moduleAP>,
					And<APTaxTran.tranType, Equal<Current<APInvoice.docType>>,
					And<APTaxTran.refNbr, Equal<Current<APInvoice.refNbr>>>>>>.Select(this))
			{
				if (vatReportingInstalled)
				{
					Tax tax = res;
					APTaxTran apTaxTran = res;
					hasPPDTaxes = tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment || hasPPDTaxes;

					if (hasPPDTaxes &&
						Document.Current != null &&
						Document.Current.CuryOrigDocAmt != null &&
						Document.Current.CuryOrigDocAmt != 0m &&
						Document.Current.CuryOrigDiscAmt != null)
					{
						decimal cashDiscPercent = (decimal)(Document.Current.CuryOrigDiscAmt / Document.Current.CuryOrigDocAmt);
						bool isTaxable = APPPDDebitAdjProcess.CalculateDiscountedTaxes(Taxes.Cache, apTaxTran, cashDiscPercent);
						decimal sign = tax.ReverseTax == true ? -1m : 1m;
						discountedPriceTotal += apTaxTran.CuryDiscountedPrice * sign;
						
						if (isTaxable)
						{
							if (tax.ReverseTax == false)
							{
								discountedTaxableTotal += apTaxTran.CuryDiscountedTaxableAmt;
							}

							if (aptaxMax == null || apTaxTran.CuryDiscountedTaxableAmt > aptaxMax.CuryDiscountedTaxableAmt)
							{
								aptaxMax = apTaxTran;
							}
						}
					}
				}

				yield return res;
			}

			if (vatReportingInstalled && Document.Current != null)
			{
				Document.Current.HasPPDTaxes = hasPPDTaxes;
				if (hasPPDTaxes)
				{
					decimal? discountedDocTotal = discountedTaxableTotal + discountedPriceTotal;
					Document.Current.CuryDiscountedDocTotal = Document.Current.CuryOrigDocAmt - Document.Current.CuryOrigDiscAmt;

					if (aptaxMax != null &&
						Document.Current.CuryVatTaxableTotal + Document.Current.CuryTaxTotal == Document.Current.CuryOrigDocAmt &&
						discountedDocTotal != Document.Current.CuryDiscountedDocTotal)
					{
						aptaxMax.CuryDiscountedTaxableAmt += Document.Current.CuryDiscountedDocTotal - discountedDocTotal;
						discountedTaxableTotal = Document.Current.CuryDiscountedDocTotal - discountedPriceTotal;
					}

					Document.Current.CuryDiscountedPrice = discountedPriceTotal;
					Document.Current.CuryDiscountedTaxableTotal = discountedTaxableTotal;
				}
			}
		}

		public PXSelect<APSetupApproval,
		Where<APSetupApproval.docType, Equal<Current<APInvoice.docType>>,
			Or<Where<Current<APInvoice.docType>, Equal<APDocType.prepayment>,
				And<APSetupApproval.docType, Equal<APDocType.prepaymentRequest>>>>>> SetupApproval;
		[PXViewName(EP.Messages.Approval)]
		public EPApprovalAutomationWithoutHoldDefaulting<APInvoice, APInvoice.approved, APInvoice.rejected, APInvoice.hold, APSetupApproval> Approval;
		
		#region Retainage part
		
		[PXReadOnlyView]
		[PXCopyPasteHiddenView]
		// APRetainageInvoice class is a APRegister class alias
		// because only APRegister part is affecting by the release process
		// and only this way we can get a proper behavior for the QueryCache mechanism.
		//
		public PXSelect<APRetainageInvoice,
					Where<True, Equal<False>>> RetainageDocuments;

		#endregion

		#endregion

		#region Function
		public virtual void VoidPrepayment(APRegister doc)
		{
			APInvoice invoice = PXCache<APInvoice>.CreateCopy((APInvoice)doc);
			invoice.OpenDoc = false;
			invoice.Voided = true;
			invoice.CuryDocBal = 0m;
			Document.Update(invoice);
			Save.Press();
		}
		public virtual void ReverseInvoiceProc(APRegister doc)
		{
			AR.DuplicateFilter dfilter = PXCache<AR.DuplicateFilter>.CreateCopy(duplicatefilter.Current);
			WebDialogResult dialogRes = duplicatefilter.View.Answer;

			this.Clear(PXClearOption.PreserveTimeStamp);

			//Magic. We need to prevent rewriting of CurrencyInfo.IsReadOnly by true in CurrencyInfoView
			CurrentDocument.Cache.AllowUpdate = true;

			Company company = PXSelect<Company>.Select(this);

			foreach (PXResult<APInvoice, CurrencyInfo, Terms, Vendor> res in APInvoice_CurrencyInfo_Terms_Vendor.Select(this, (object)doc.DocType, doc.RefNbr))
			{
				APInvoice origInvoice = res;
				bool IsTaxDocumentVendorNotBaseCuryID = origInvoice.IsTaxDocument == true													
													&& origInvoice.CuryID != company.BaseCuryID;

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info.BaseCalc = !IsTaxDocumentVendorNotBaseCuryID;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				APInvoice invoice = PXCache<APInvoice>.CreateCopy(origInvoice);
				invoice.CuryInfoID = info.CuryInfoID;
				invoice.DocType = APDocType.DebitAdj;
				invoice.RefNbr = null;

				//must set for _RowSelected
				invoice.OpenDoc = true;
				invoice.Released = false;

				Document.Cache.SetDefaultExt<APInvoice.isMigratedRecord>(invoice);
				invoice.BatchNbr = null;
				invoice.PrebookBatchNbr = null;
				invoice.Prebooked = false;
				invoice.ScheduleID = null;
				invoice.Scheduled = false;
				invoice.NoteID = null;

				invoice.TermsID = null;
				invoice.InstallmentCntr = null;
				invoice.InstallmentNbr = null;
				invoice.DueDate = null;
				invoice.DiscDate = null;
				invoice.CuryOrigDiscAmt = 0m;
				FinPeriodIDAttribute.SetPeriodsByMaster<APInvoice.finPeriodID>(CurrentDocument.Cache, invoice, doc.FinPeriodID);
                invoice.OrigDocDate = invoice.DocDate;

				if (doc.IsChildRetainageDocument())
				{
					invoice.OrigDocType = doc.OrigDocType;
					invoice.OrigRefNbr = doc.OrigRefNbr;
				}
				else
				{
				invoice.OrigDocType = doc.DocType;
				invoice.OrigRefNbr = doc.RefNbr;
				}

				invoice.PaySel = false;
				//PaySel does not affect these fields
				//invoice.PayTypeID = null;
				//invoice.PayDate = null;
				//invoice.PayAccountID = null;
				invoice.CuryDocBal = invoice.CuryOrigDocAmt;
				invoice.CuryLineTotal = 0m;
				invoice.IsTaxPosted = false;
				invoice.IsTaxValid = false;
				invoice.CuryVatTaxableTotal = 0m;
				invoice.CuryVatExemptTotal = 0m;
				invoice.PrebookAcctID = PXAccess.FeatureInstalled<FeaturesSet.prebooking>() ? origInvoice.PrebookAcctID : null;
				invoice.PrebookSubID = PXAccess.FeatureInstalled<FeaturesSet.prebooking>() ? origInvoice.PrebookSubID : null;
			    invoice.Hold = (apsetup.Current.HoldEntry ?? false) || IsApprovalRequired(invoice, Document.Cache);
				invoice.PendingPPD = false;
				invoice.TaxCostINAdjRefNbr = null;
				invoice.PaymentsByLinesAllowed = false;

				ClearRetainageSummary(invoice);
				using (var validationScope = new DisableSelectorValidationScope(this.Document.Cache, typeof(APRegister.employeeID)))
				{
					Document.Cache.SetDefaultExt<APInvoice.employeeID>(invoice);
					Document.Cache.SetDefaultExt<APInvoice.employeeWorkgroupID>(invoice);
				invoice = this.Document.Insert(invoice);

				FinPeriodIDAttribute.SetPeriodsByMaster<APInvoice.finPeriodID>(CurrentDocument.Cache, invoice, doc.TranPeriodID);

				}

				if (invoice.RefNbr == null)
				{
					//manual numbering, check for occasional duplicate
					APInvoice duplicate = PXSelect<APInvoice>.Search<APInvoice.docType, APInvoice.refNbr>(this, invoice.DocType, invoice.OrigRefNbr);
					if (duplicate != null)
					{
						PXCache<AR.DuplicateFilter>.RestoreCopy(duplicatefilter.Current, dfilter);
						duplicatefilter.View.Answer = dialogRes;
						if (duplicatefilter.AskExt() == WebDialogResult.OK)
						{
							duplicatefilter.Cache.Clear();
							if (duplicatefilter.Current.RefNbr == null)
								throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AR.DuplicateFilter.refNbr).Name);
							duplicate = PXSelect<APInvoice>.Search<APInvoice.docType, APInvoice.refNbr>(this, invoice.DocType, duplicatefilter.Current.RefNbr);
							if (duplicate != null)
						throw new PXException(ErrorMessages.RecordExists);
							invoice.RefNbr = duplicatefilter.Current.RefNbr;
						}
					}
					else
					invoice.RefNbr = invoice.OrigRefNbr;
					this.Document.Cache.Normalize();
					invoice = this.Document.Update(invoice);
				}

				if (info != null)
				{
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}
			}

			TaxAttribute.SetTaxCalc<APTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			var origLineNbrsDict = new Dictionary<int?, int?>();

			foreach (APTran srcTran in PXSelect<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>, And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				//TODO Create new APTran and explicitly fill the required fields
				APTran tran = PXCache<APTran>.CreateCopy(srcTran);
				tran.TranType = null;
				tran.RefNbr = null;
				tran.OrigLineNbr = doc.IsChildRetainageDocument()
					? srcTran.OrigLineNbr
					: srcTran.LineNbr;
				tran.TranID = null;
				string origDrCr = tran.DrCr;
				tran.DrCr = null;
				tran.Released = null;
				tran.CuryInfoID = null;
				tran.ManualDisc = true;

				tran.PPVDocType = null;
				tran.PPVRefNbr = null;
				tran.POPPVAmt = 0m;
				tran.NoteID = null;

				tran.ClearInvoiceDetailsBalance();

				if (!string.IsNullOrEmpty(tran.DeferredCode))
				{
					DRSchedule schedule = PXSelect<DRSchedule,
						Where<DRSchedule.module, Equal<moduleAP>,
						And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
						And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
						And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>.Select(this, doc.DocType, doc.RefNbr, tran.LineNbr);

					if (schedule != null)
					{
						tran.DefScheduleID = schedule.ScheduleID;
					}
				}

				Decimal? curyTranAmt = tran.CuryTranAmt;
				APTran tranNew = this.Transactions.Insert(tran);
				PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, srcTran, Transactions.Cache, tranNew);

				if (tranNew != null && tranNew.CuryTranAmt != curyTranAmt)
				{
					tranNew.CuryTranAmt = curyTranAmt;
					tranNew = (APTran)Transactions.Cache.Update(tranNew);
				}

				if (tranNew.LineType == SOLineType.Discount)
				{
					tranNew.DrCr = (origDrCr == DrCr.Debit) ? DrCr.Credit : DrCr.Debit;
					tranNew.FreezeManualDisc = true;
					tranNew.TaxCategoryID = null;
					this.Transactions.Update(tranNew);
				}

				origLineNbrsDict.Add(tranNew.LineNbr, srcTran.LineNbr);
			}

			foreach (APInvoiceDiscountDetail discountDetail in PXSelect<APInvoiceDiscountDetail, Where<APInvoiceDiscountDetail.docType, Equal<Required<APInvoice.docType>>, And<APInvoiceDiscountDetail.refNbr, Equal<Required<APInvoice.refNbr>>>>, OrderBy<Asc<APInvoiceDiscountDetail.docType, Asc<APInvoiceDiscountDetail.refNbr>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				APInvoiceDiscountDetail newDiscountDetail = PXCache<APInvoiceDiscountDetail>.CreateCopy(discountDetail);

				newDiscountDetail.DocType = Document.Current.DocType;
				newDiscountDetail.RefNbr = Document.Current.RefNbr;
				newDiscountDetail.IsManual = true;
				_discountEngine.UpdateDiscountDetail(DiscountDetails.Cache, DiscountDetails, newDiscountDetail);
			}    

			if (!IsExternalTax(Document.Current.TaxZoneID))
			{
				bool disableTaxCalculation = 
					doc.PendingPPD == true && doc.DocType == APDocType.DebitAdj || 
					doc.IsOriginalRetainageDocument() || 
					doc.IsChildRetainageDocument();

				PXResultset<APTaxTran> taxes = PXSelect<APTaxTran,
					Where<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>,
						And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>>>>
					.Select(this, doc.DocType, doc.RefNbr);

				// Insert taxes first and only after that copy 
				// all needed values to prevent tax recalculation
				// during the next tax insertion.
				// 
				Dictionary<string, APTaxTran> insertedTaxes = null;
				if (disableTaxCalculation)
				{
					insertedTaxes = new Dictionary<string, APTaxTran>();
					taxes.RowCast<APTaxTran>().ForEach(tax => insertedTaxes.Add(tax.TaxID, Taxes.Insert(new APTaxTran() { TaxID = tax.TaxID })));
				}

				foreach (APTaxTran tax in taxes)
				{
					APTaxTran new_aptax = disableTaxCalculation
						? insertedTaxes[tax.TaxID]
						: Taxes.Insert(new APTaxTran() { TaxID = tax.TaxID });

					if (new_aptax != null)
					{
						new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
						new_aptax.TaxRate = tax.TaxRate;
						new_aptax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_aptax.CuryTaxAmt = tax.CuryTaxAmt;
						new_aptax.CuryTaxAmtSumm = tax.CuryTaxAmtSumm;
						new_aptax.NonDeductibleTaxRate = tax.NonDeductibleTaxRate;
						new_aptax.CuryExpenseAmt = tax.CuryExpenseAmt;
						new_aptax.CuryRetainedTaxableAmt = tax.CuryRetainedTaxableAmt;
						new_aptax.CuryRetainedTaxAmt = tax.CuryRetainedTaxAmt;
						new_aptax.CuryRetainedTaxAmtSumm = tax.CuryRetainedTaxAmtSumm;
						new_aptax = Taxes.Update(new_aptax);
					}
				}

				// We should copy all calculated APTax records from the
				// Retainage Bill to keep consistent line balances.
				// For more detail see AC-137532 JIRA issue.
				// 
				if (doc.IsChildRetainageDocument() &&
					doc.PaymentsByLinesAllowed == true)
				{
					foreach (APTran newAPTran in Transactions.Select())
					{
						foreach (APTax newAPTax in PXSelect<APTax,
							Where<APTax.tranType, Equal<Required<APTax.tranType>>,
								And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
								And<APTax.lineNbr, Equal<Required<APTax.lineNbr>>>>>>
							.Select(this, newAPTran.TranType, newAPTran.RefNbr, newAPTran.LineNbr))
						{
							int? origLineNbr = origLineNbrsDict[newAPTran.LineNbr];

							APTax origAPtax = PXSelect<APTax,
								Where<APTax.tranType, Equal<Required<APTax.tranType>>,
									And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
									And<APTax.taxID, Equal<Required<APTax.taxID>>,
									And<APTax.lineNbr, Equal<Required<APTax.lineNbr>>>>>>>
								.SelectSingleBound(this, null, doc.DocType, doc.RefNbr, newAPTax.TaxID, origLineNbr);

							if (origAPtax != null)
							{
								APTax copyAPTax = PXCache<APTax>.CreateCopy(newAPTax);
								copyAPTax.CuryTaxableAmt = origAPtax.CuryTaxableAmt;
								copyAPTax.CuryTaxAmt = origAPtax.CuryTaxAmt;
								copyAPTax.NonDeductibleTaxRate = origAPtax.NonDeductibleTaxRate;
								copyAPTax.CuryExpenseAmt = origAPtax.CuryExpenseAmt;
								copyAPTax = Tax_Rows.Update(copyAPTax);
							}
						}
					}
				}
			}
		}

		private void PopulateBoxList()
		{
			List<int> AllowedValues = new List<int>();
			List<string> AllowedLabels = new List<string>();

			foreach (AP1099Box box in PXSelectReadonly<AP1099Box>.Select(this, null))
			{
				AllowedValues.Add((int)box.BoxNbr);
				StringBuilder bld = new StringBuilder(box.BoxNbr.ToString());
				bld.Append("-");
				bld.Append(box.Descr);
				AllowedLabels.Add(bld.ToString());
			}

			if (AllowedValues.Count > 0)
			{
				PXIntListAttribute.SetList<APTran.box1099>(Transactions.Cache, null, AllowedValues.ToArray(), AllowedLabels.ToArray());
			}
		}

		public object GetAcctSub<Field>(PXCache cache, object data) where Field : IBqlField
		{
			object NewValue = cache.GetValueExt<Field>(data);
			if (NewValue is PXFieldState)
			{
				return ((PXFieldState)NewValue).Value;
			}
			else
			{
				return NewValue;
			}
		}

		public virtual PO.LandedCosts.LandedCostAPBillFactory GetLandedCostApBillFactory()
		{
			return new PO.LandedCosts.LandedCostAPBillFactory(this);
		}
		
		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		public APInvoiceEntry()
		{
			APSetup setup = APSetup.Current;

			PopulateBoxList();

			RetainageDocuments.Cache.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.retainage>();
			RetainageDocuments.Cache.AllowDelete = false;
			RetainageDocuments.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(RetainageDocuments.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjdPPDAmt>(Adjustments.Cache, null, false);
			
			PXUIFieldAttribute.SetVisible<APTran.projectID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AP));
			PXUIFieldAttribute.SetVisible<APTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AP));
			PXUIFieldAttribute.SetVisible<APTran.nonBillable>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AP));

			TaxAttribute.SetTaxCalc<APTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });

			Actions.Move(nameof(release), nameof(prebook));
			PXAction action = Actions["action"];
			if (action != null)
			{
				action.AddMenuAction(voidInvoice);
			}

			APOpenPeriodAttribute.SetValidatePeriod<APInvoice.finPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<APInvoice>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(APTran), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<APTran.tranType>(PXDbType.Char, 3, ((APInvoiceEntry)graph).Document.Current?.DocType),
							new PXDataFieldValue<APTran.refNbr>(((APInvoiceEntry)graph).Document.Current?.RefNbr),
							new PXDataFieldValue<APTran.lineType>(PXDbType.Char, 2, SOLineType.Discount, PXComp.NEorISNULL)
						};
					}));
			}
		}

		public override void Persist()
		{
			if (Document.Current != null)
			{
				bool discountLineExists = Discount_Row.Any();
				if (!discountLineExists && Document.Current.CuryDiscTot != 0m)
				{
					AddDiscount(Document.Cache, Document.Current);
				}
				else if (discountLineExists && Document.Current.CuryDiscTot == 0m && !DiscountDetails.Any())
				{
					Discount_Row.Select().RowCast<APTran>().ForEach(discountLine => Discount_Row.Cache.Delete(discountLine));
				}
			}
			
			foreach (APAdjust adj in Adjustments.Cache.Inserted)
			{
				if (adj.CuryAdjdAmt == 0m || Document.Current?.Rejected == true)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.InsertedDeleted);
				}
			}

			foreach (APAdjust adj in Adjustments.Cache.Updated)
			{
				if (adj.CuryAdjdAmt == 0m || Document.Current?.Rejected == true)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.Deleted);
				}
			}

			Adjustments.Cache.ClearQueryCache();

			foreach (APInvoice apdoc in Document.Cache.Cached)
			{
				PXEntryStatus status = Document.Cache.GetStatus(apdoc);

				if (status == PXEntryStatus.Deleted && apdoc.PendingPPD == true && apdoc.DocType == APDocType.DebitAdj)
				{
					PXUpdate<Set<APAdjust.pPDDebitAdjRefNbr, Null>, APAdjust,
						Where<APAdjust.pendingPPD, Equal<True>,
							And<APAdjust.pPDDebitAdjRefNbr, Equal<Required<APAdjust.pPDDebitAdjRefNbr>>>>>
						.Update(this, apdoc.RefNbr);
				}

				if ((status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated) 
					&& apdoc.DocType == APDocType.Invoice 
					&& apdoc.Released != true && apdoc.Prebooked != true)
				{
					decimal? CuryApplAmt = 0m;

					foreach (APAdjust adj in Adjustments_Raw.View
						.SelectMultiBound(new object[] { apdoc })
						.RowCast<APAdjust>()
						.Where(adj => adj != null))
					{
						CuryApplAmt += adj.CuryAdjdAmt;

						if (apdoc.CuryDocBal - CuryApplAmt < 0m)
						{
								Adjustments.Cache.MarkUpdated(adj);
							Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(Messages.Application_Amount_Cannot_Exceed_Document_Amount));
							throw new PXException(Messages.Application_Amount_Cannot_Exceed_Document_Amount);
						}
					}
				}
			}

			_discountEngine.ValidateDiscountDetails(DiscountDetails);

			base.Persist();
		}

		public virtual IEnumerable adjustments()
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this);

			foreach (PXResult<APAdjust, APPayment, Standalone.APRegisterAlias, CurrencyInfo> res in Adjustments_Raw.Select())
			{
				APPayment payment = res;
				APAdjust adj = res;
				CurrencyInfo pay_info = res;

				Exception exception = null;

				PXCache<APRegister>.RestoreCopy(payment, (Standalone.APRegisterAlias)res);

				decimal CuryDocBal = 0m;
				try
				{
					CuryDocBal = BalanceCalculation.CalculateApplicationDocumentBalance(
						Adjustments.Cache, 
						pay_info, 
						inv_info, 
						payment.DocBal, 
						payment.CuryDocBal);
				}
				catch (Exception ex)
				{
					exception = ex;
				}

				if (adj != null)
				{
					if (adj.Released == false)
					{
						if (adj.CuryAdjdAmt > CuryDocBal)
						{
							//if reconsidered need to calc RGOL
							adj.CuryDocBal = CuryDocBal;
							adj.CuryAdjdAmt = 0m;
						}
						else
						{
							adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
						}
					}
					else
					{
						adj.CuryDocBal = CuryDocBal;
					}
					adj.AdjType = AR.ARAdjust.adjType.Adjusting;
					this.Caches<APAdjust>().RaiseFieldUpdated<APAdjust.adjType>(adj, null);
					if (exception != null)
					{
						this.Caches<APAdjust>().RaiseExceptionHandling<APAdjust.curyDocBal>(adj, 0m, exception);
					}
				}

				if (adj != null)
				{
					yield return new PXResult<APAdjust, APPayment, CurrencyInfo>(adj, payment, pay_info);
				}
			}

			if (Document.Current?.Released == true)
			{
				foreach (PXResult<APAdjust, Standalone.APRegisterAlias, APInvoice, CurrencyInfo> res in
					PXSelectJoin<
						APAdjust,
							InnerJoin<Standalone.APRegisterAlias,
								On<Standalone.APRegisterAlias.docType, Equal<APAdjust.adjdDocType>,
								And<Standalone.APRegisterAlias.refNbr, Equal<APAdjust.adjdRefNbr>>>,
							InnerJoinSingleTable<APInvoice,
								On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
								And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
							InnerJoin<CurrencyInfo,
								On<CurrencyInfo.curyInfoID, Equal<Standalone.APRegisterAlias.curyInfoID>>>>>,
						Where<
							APAdjust.adjgDocType, Equal<Current<APInvoice.docType>>,
							And<APAdjust.adjgRefNbr, Equal<Current<APInvoice.refNbr>>>>>
					.Select(this))
				{
					APAdjust adj = res;
					APInvoice invoice = res;
					CurrencyInfo adjd_info = res;
					PXCache<APRegister>.RestoreCopy(invoice, (Standalone.APRegisterAlias)res);

					adj.AdjType = ARAdjust.adjType.Adjusted;
					this.Caches<APAdjust>().RaiseFieldUpdated<APAdjust.adjType>(adj, null);

					try
					{
						adj.CuryDocBal = BalanceCalculation.CalculateApplicationDocumentBalance(
							Adjustments.Cache, 
							adjd_info, 
							inv_info, 
							invoice.DocBal, 
							invoice.CuryDocBal);
					}
					catch (Exception exception)
					{
						this.Caches<APAdjust>().RaiseExceptionHandling<APAdjust.curyDocBal>(adj, 0m, exception);
					}

                    yield return new PXResult<APAdjust, APInvoice, CurrencyInfo>(adj, invoice, adjd_info);
				}
			}

			if (Document.Current != null
				&& (Document.Current.DocType == APDocType.Invoice
					|| Document.Current.DocType == APDocType.CreditAdj)
				&& Document.Current.Released != true
				&& Document.Current.Prebooked != true
				&& Document.Current.Rejected != true
				&& Document.Current.Scheduled != true
				&& Document.Current.Voided != true
				&& !IsImport)
			{
				using (new ReadOnlyScope(Adjustments.Cache, Document.Cache))
				// The display fields are filled manually because
				// the selector + formulas severely affect performance 
				// in presence of many payment documents.
				// -
				using (var firstPerformanceScope = new DisableSelectorValidationScope(Adjustments.Cache))
				using (var secondPerformanceScope = new DisableFormulaCalculationScope(
					Adjustments.Cache,
					typeof(APAdjust.displayDocType),
					typeof(APAdjust.displayRefNbr),
					typeof(APAdjust.displayDocDate),
					typeof(APAdjust.displayDocDesc),
					typeof(APAdjust.displayCuryID),
					typeof(APAdjust.displayFinPeriodID),
					typeof(APAdjust.displayStatus)))
				{
					foreach (PXResult<Standalone.APRegisterAlias, CurrencyInfo, APAdjust, APPayment> res in 
						PXSelectJoin<
							Standalone.APRegisterAlias,
								InnerJoin<CurrencyInfo, 
									On<CurrencyInfo.curyInfoID, Equal<Standalone.APRegisterAlias.curyInfoID>>, 
								LeftJoin<APAdjust, 
									On<APAdjust.adjgDocType, Equal<Standalone.APRegisterAlias.docType>, 
									And<APAdjust.adjgRefNbr, Equal<Standalone.APRegisterAlias.refNbr>, 
									And<APAdjust.released, NotEqual<True>, 
									And<
										Where<APAdjust.adjdDocType, NotEqual<Current<APInvoice.docType>>, 
										Or<APAdjust.adjdRefNbr, NotEqual<Current<APInvoice.refNbr>>>>>>>>,
								InnerJoinSingleTable<APPayment,
									On<APPayment.docType, Equal<Standalone.APRegisterAlias.docType>,
									And<APPayment.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>>>>, 
							Where<
								Standalone.APRegisterAlias.vendorID, Equal<Current<APInvoice.vendorID>>, 
								And2<Where<
									Standalone.APRegisterAlias.docType, Equal<APDocType.prepayment>, 
									Or<Standalone.APRegisterAlias.docType, Equal<APDocType.debitAdj>>>, 
								And2<Where<
								Standalone.APRegisterAlias.docDate, LessEqual<Current<APInvoice.docDate>>, 
								And<Standalone.APRegisterAlias.tranPeriodID, LessEqual<Current<APRegister.tranPeriodID>>, 
								And<Standalone.APRegisterAlias.released, Equal<True>, 
								And<Standalone.APRegisterAlias.openDoc, Equal<True>, 
								And<APAdjust.adjdRefNbr, IsNull>>>>>,
								And<Standalone.APRegisterAlias.hold, NotEqual<True>,
								And<Not<HasUnreleasedVoidPayment<APPayment.docType, APPayment.refNbr>>>>>>>>
						.Select(this))
					{
						APPayment payment = res;
						APAdjust adj = new APAdjust();
						CurrencyInfo pay_info = res;

						PXCache<APRegister>.RestoreCopy(payment, (Standalone.APRegisterAlias)res);

						adj.VendorID = Document.Current.VendorID;
						adj.AdjdDocType = Document.Current.DocType;
						adj.AdjdRefNbr = Document.Current.RefNbr;
						adj.AdjdBranchID = Document.Current.BranchID;
						adj.AdjgDocType = payment.DocType;
						adj.AdjgRefNbr = payment.RefNbr;
						adj.AdjgBranchID = payment.BranchID;
						adj.AdjNbr = payment.AdjCntr;

						if (Adjustments.Cache.Locate(adj) == null)
						{
							adj.AdjgCuryInfoID = payment.CuryInfoID;
							adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
							//if LE constraint is removed from payment selection this must be reconsidered
							adj.AdjdCuryInfoID = Document.Current.CuryInfoID;
							adj.AdjType = AR.ARAdjust.adjType.Adjusting;
							Exception exception = null;
							try
							{
								adj.CuryDocBal = BalanceCalculation.CalculateApplicationDocumentBalance(
									Adjustments.Cache, 
									pay_info, 
									inv_info, 
									payment.DocBal, 
									payment.CuryDocBal);
							}
							catch (Exception ex)
							{
								exception = ex;
							}

							adj.DisplayDocType = payment.DocType;
							adj.DisplayRefNbr = payment.RefNbr;
							adj.DisplayDocDate = payment.DocDate;
							adj.DisplayDocDesc = payment.DocDesc;
							adj.DisplayCuryID = payment.CuryID;
							adj.DisplayFinPeriodID = payment.FinPeriodID;
                            adj.DisplayStatus = payment.Status;

							adj = Adjustments.Insert(adj);
							if (exception != null)
							{
								this.Caches<APAdjust>().RaiseExceptionHandling<APAdjust.curyDocBal>(adj, 0m, exception);
							}

							if (adj != null)
							{
								yield return new PXResult<APAdjust, APPayment, CurrencyInfo>(adj, payment, pay_info);
							}
						}
					}
				}
			}
		}

		#endregion

		#region APInvoice Events

		protected virtual void APInvoice_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = APDocType.Invoice; 
		}

		protected virtual void APInvoice_APAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		    Location vendorLocation = location.View.SelectSingleBound(new[] {e.Row}) as Location;
			if (vendorLocation != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APInvoice)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentAcctID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPAccountID>(location.Cache, vendorLocation);
				}
			}
		}

		protected virtual void APInvoice_APSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		    Location vendorLocation = location.View.SelectSingleBound(new[] { e.Row }) as Location;
			if (vendorLocation != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APInvoice)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentSubID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPSubID>(location.Cache, vendorLocation);
				}
			}
		}

		protected virtual void APInvoice_SuppliedByVendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.taxZoneID>(e.Row);
		}

		protected virtual void APInvoice_SuppliedByVendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.taxZoneID>(e.Row);
		}

		protected virtual void APInvoice_PayLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.separateCheck>(e.Row);
			sender.SetDefaultExt<APInvoice.payTypeID>(e.Row);

		}

		protected virtual void APInvoice_PaymentsByLinesAllowed_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APInvoice document = e.Row as APInvoice;
			if (document == null) return;

			if ((bool?)e.OldValue == true && document.PaymentsByLinesAllowed != true)
			{
				sender.RaiseExceptionHandling<APInvoice.curyDiscTot>(document, document.CuryDiscTot, null);
				sender.RaiseExceptionHandling<APInvoice.curyTaxTotal>(document, document.CuryTaxTotal, null);
				sender.RaiseExceptionHandling<APInvoice.curyOrigWhTaxAmt>(document, document.CuryOrigWhTaxAmt, null);
			}
		}

		protected virtual void APInvoice_CuryDocBal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var apdoc = e.Row as APInvoice;
			if (apdoc == null) return;

			if (!PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>()) return;

			PXEntryStatus status = Document.Cache.GetStatus(apdoc);

			if ((status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
				&& apdoc.DocType == APDocType.Invoice
				&& apdoc.Released != true && apdoc.Prebooked != true)
			{
				foreach (APAdjust adj in Adjustments.View
						.SelectMultiBound(new object[] { apdoc })
						.RowCast<APAdjust>()
						.Where(adj => adj != null && adj.CuryAdjdAmt > 0))
				{
					// use (CuryAdjdAmt + CuryDocBal) to be able descrease and then increase amount, when CuryDocBal become higher
					adj.CuryAdjdAmt = Math.Min((adj.CuryAdjdAmt ?? 0) + (adj.CuryDocBal ?? 0), apdoc.CuryDocBal ?? 0);
					Adjustments.Cache.MarkUpdated(adj);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<APInvoice, APInvoice.curyOrigDiscAmt> e)
		{
			if ((decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, 0.ToString());
			}
		}

		protected bool IsVendorIDUpdated = false;

		protected virtual void APInvoice_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			IsVendorIDUpdated = true;
			APInvoice invoice = (APInvoice)e.Row;

			vendor.RaiseFieldUpdated(sender, e.Row);

			Adjustments_Raw.Cache.Clear();
			Adjustments_Raw.Cache.ClearQueryCacheObsolete(); // remove this line after AC-62581 fix in appropriate versions

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || sender.GetValuePending<APInvoice.curyID>(e.Row) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<APInvoice.curyInfoID>(sender, invoice);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<APInvoice.docDate>(invoice, invoice.DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						invoice.CuryID = info.CuryID;
					}
				}
			}
		   
			SetDefaultsAfterVendorIDChanging(sender, e);

			// Delete all applications AC-97392
			PXSelect<APAdjust,
				Where<APAdjust.adjdDocType, Equal<Required<APInvoice.docType>>,
					And<APAdjust.adjdRefNbr, Equal<Required<APInvoice.refNbr>>>>>
				.Select(this, invoice.DocType, invoice.RefNbr)
				.RowCast<APAdjust>()
				.ForEach(application => Adjustments.Cache.Delete(application));
		}

		protected virtual void APInvoice_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

			try
			{
				// We should set defaults only when all preceding graph events 
				// have been executed, otherwise we may get an incorrect behavior
				// due to unfilled dependent fields. For more details see AC-102525.
				// TODO: refactor existing code inside the graph and DAC to implement
				// only declarative practice.
				//
				if (IsVendorIDUpdated || e.ExternalCall == true)
				{
					SetDefaultsAfterVendorIDChanging(sender, e);
				}
			}
			finally
			{
				IsVendorIDUpdated = false;
			}
		}

		private void SetDefaultsAfterVendorIDChanging(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.aPAccountID>(e.Row);
			sender.SetDefaultExt<APInvoice.aPSubID>(e.Row);
			sender.SetDefaultExt<APInvoice.branchID>(e.Row);
			sender.SetValue<APInvoice.payLocationID>(e.Row, sender.GetValue<APInvoice.vendorLocationID>(e.Row));
			sender.SetDefaultExt<APInvoice.payTypeID>(e.Row);
			sender.SetDefaultExt<APInvoice.separateCheck>(e.Row);
			sender.SetDefaultExt<APInvoice.taxCalcMode>(e.Row);
			sender.SetDefaultExt<APInvoice.taxZoneID>(e.Row);
			sender.SetDefaultExt<APInvoice.prebookAcctID>(e.Row);
			sender.SetDefaultExt<APInvoice.prebookSubID>(e.Row);
			sender.SetDefaultExt<APInvoice.paymentsByLinesAllowed>(e.Row);
		}

		protected virtual bool IsAllowWithholdingTax(PXCache sender, APInvoice doc)
		{
			if (doc.DocType == APDocType.Prepayment)
			{
				var withholdingTaxes = PXSelectJoin< APTaxTran,
								InnerJoin<Tax, 
									On<Tax.taxID, Equal<APTaxTran.taxID>>>,
								Where< APTaxTran.module, Equal<BatchModule.moduleAP>,
									And<APTaxTran.tranType, Equal<Current<APInvoice.docType>>,
									And<APTaxTran.refNbr, Equal<Current<APInvoice.refNbr>>,
									And<Tax.taxType, Equal<CSTaxType.withholding>>>>>>
								.Select(this);

				if(withholdingTaxes.Count>0)
				{
					return false;
				}
			}
			return true;
		}

		protected virtual void APInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;

			if(!IsAllowWithholdingTax(sender, doc))
			{
				throw new PXRowPersistingException(nameof(APInvoice), doc, Messages.WithholdingTaxesInPrepaymentError);
			}

			if (doc.DocType != APDocType.DebitAdj && doc.DocType != APDocType.Prepayment && string.IsNullOrEmpty(doc.TermsID))
			{
				sender.RaiseExceptionHandling<APInvoice.termsID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(Document.Cache, doc);

			if (vendor.Current != null && (bool)vendor.Current.Vendor1099 && terms != null)
			{

				if (terms.InstallmentType == CS.TermsInstallmentType.Multiple)
				{
					sender.RaiseExceptionHandling<APInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.AP1099_Vendor_Cannot_Have_Multiply_Installments));
				}
			}

			EPEmployee emp = EmployeeByVendor.Select();
			if (emp != null && terms != null)
			{
				if (PXCurrencyAttribute.IsNullOrEmpty(terms.DiscPercent) == false)
				{
					sender.RaiseExceptionHandling<APInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.Employee_Cannot_Have_Discounts));
				}

				if (terms.InstallmentType == TermsInstallmentType.Multiple)
				{
					sender.RaiseExceptionHandling<APInvoice.termsID>(e.Row, doc.TermsID, new PXSetPropertyException(Messages.Employee_Cannot_Have_Multiply_Installments));
				}
			}

			if (doc.DocType != APDocType.DebitAdj && doc.DueDate == null)
			{
				sender.RaiseExceptionHandling<APInvoice.dueDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayDate == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayDate != null && ((DateTime)doc.DocDate).CompareTo((DateTime)doc.PayDate) > 0)
			{
				sender.RaiseExceptionHandling<APInvoice.payDate>(e.Row, doc.PayDate, new PXSetPropertyException(Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayLocationID == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payLocationID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayAccountID == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payAccountID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayTypeID == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payTypeID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType == APDocType.Prepayment && doc.PaySel == true && doc.PayAccountID != null)
			{
				object PayAccountID = doc.PayAccountID;

				try
				{
					sender.RaiseFieldVerifying<APInvoice.payAccountID>(doc, ref PayAccountID);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<APInvoice.payAccountID>(doc, PayAccountID, ex);
				}
			}

			if (doc.DocType != APDocType.DebitAdj && doc.DocType != APDocType.Prepayment && doc.DiscDate == null)
			{
				sender.RaiseExceptionHandling<APInvoice.discDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (string.IsNullOrEmpty(doc.InvoiceNbr) && IsInvoiceNbrRequired(doc))
			{
				sender.RaiseExceptionHandling<APInvoice.invoiceNbr>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType == APDocType.Prepayment && doc.OpenDoc == true && doc.Voided == true)
			{
				doc.OpenDoc = false;
				doc.ClosedDate = doc.DocDate;
				doc.ClosedFinPeriodID = doc.FinPeriodID;
				doc.ClosedTranPeriodID = doc.TranPeriodID;
			}

			if (doc.CuryDiscTot > Math.Abs(doc.CuryLineTotal ?? 0m))
			{
				sender.RaiseExceptionHandling<APInvoice.curyDiscTot>(e.Row, doc.CuryDiscTot, new PXSetPropertyException(Messages.DiscountGreaterLineTotal, PXErrorLevel.Error));
			}

			ValidateAPAndReclassificationAccountsAndSubs(sender, doc);
			IsVendorIDUpdated = false;

			if (doc.CuryOrigDiscAmt != 0m && (doc.PaymentsByLinesAllowed == true || doc.RetainageApply == true))
			{
				foreach (PXResult<APTaxTran, Tax> res in Taxes.Select())
				{
					Tax tax = res;
					APTaxTran apTaxTran = res;
					if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
					{
						sender.RaiseExceptionHandling<APInvoice.curyOrigDiscAmt>(e.Row, doc.CuryOrigDiscAmt,
							new PXSetPropertyException(Messages.PaymentsByLinesOrApplyRetainagePPDTaxesNotSupported, PXErrorLevel.Error));

						break;
					}
				}
			}
		}

		protected virtual void APInvoice_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;
			if (doc != null && doc.DocType == APDocType.Prepayment && e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				APPayment duplicateCheck = PXSelect<APPayment,
					Where<APPayment.docType, Equal<APDocType.check>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
					.Select(this, doc.RefNbr);

				if (duplicateCheck != null)
				{
					// use the standard AutoNumberingAttribute functionality in case of autonumbering
					// throw primary key violation to make AutoNumbering re-create the next number
					// in case of manual numbering throws UI exception
					var numbering = (Numbering)PXSelectorAttribute.Select<APSetup.invoiceNumberingID>(this.Caches[typeof(APSetup)], this.Caches[typeof(APSetup)].Current);
					if (numbering?.UserNumbering == true)
						throw new PXRowPersistedException(typeof(APPayment.refNbr).Name, doc.RefNbr, Messages.SameRefNbr, Messages.Check, doc.RefNbr);
					else
						throw new PXLockViolationException(typeof(APInvoice), PXDBOperation.Insert, new object[] { doc.DocType, doc.RefNbr });
				}
			}
		}

		protected virtual bool IsInvoiceNbrRequired(APInvoice doc)
		{
			return APSetup.Current.RequireVendorRef == true
				&& doc.DocType != APDocType.DebitAdj
				&& doc.DocType != APDocType.CreditAdj
				&& doc.DocType != APDocType.Prepayment
				&& (vendor.Current == null
				|| vendor.Current.TaxAgency == false)
				&& (doc.OrigDocType == null 
					&& doc.OrigRefNbr == null
					|| doc.IsChildRetainageDocument());
		}

		protected virtual void APInvoice_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;

			CurrencyInfoAttribute.SetEffectiveDate<APInvoice.docDate>(sender, e);

			if (doc.DocType == APDocType.Prepayment)// && doc.DueDate != null && DateTime.Compare((DateTime)doc.DocDate, (DateTime)doc.DueDate) > 0)
			{
				sender.SetDefaultExt<APInvoice.dueDate>(doc);
			}
		}

		protected virtual void APInvoice_DueDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APInvoice invoice = (APInvoice)e.Row;
			if (invoice.DocType == APDocType.Prepayment)
			{
				e.NewValue = invoice.DocDate;
			}
		}

		protected virtual void APInvoice_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(sender, e.Row);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				foreach (APAdjust adj in Adjustments.Select())
				{
					Adjustments.Cache.Delete(adj);
				}
			}
		}

        public virtual APInvoiceState GetDocumentState(PXCache cache, APInvoice document)
        {
            if (cache == null) throw new PXArgumentException(nameof(cache));
            if (document == null) throw new PXArgumentException(nameof(document));

            var result = new APInvoiceState();

            result.IsFromExpenseClaims = document.OrigModule == BatchModule.EP;
            if (result.IsFromExpenseClaims 
				|| document.InstallmentNbr != null 
				|| (document.OrigRefNbr == null 
					&& document.IsTaxDocument == true))
            {
                result.DontApprove = true;
            }
            else
            {
                var isApprovalInstalled = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();
                var areMapsAssigned = Approval.GetAssignedMaps(document, cache).Any();
                result.DontApprove = !isApprovalInstalled || !areMapsAssigned;
            }

            result.HasPOLink = IsPOLinkedAPBill.Ensure(cache, document);

            result.IsDocumentPrepayment = document.DocType == APDocType.Prepayment;
            result.IsDocumentInvoice = document.DocType == APDocType.Invoice;
            result.IsDocumentDebitAdjustment = document.DocType == APDocType.DebitAdj;
            result.IsDocumentCreditAdjustment = document.DocType == APDocType.CreditAdj;

            result.IsDocumentOnHold = document.Hold == true;
            result.IsDocumentScheduled = document.Scheduled == true;
            result.IsDocumentPrebookedNotCompleted = document.Prebooked == true && document.Released == false &&
                                                   document.Voided == false;
            result.IsDocumentReleasedOrPrebooked = document.Released == true || document.Prebooked == true;
            result.IsDocumentVoided = document.Voided == true;
            result.IsDocumentRejected = document.Rejected == true;

            result.RetainageApply = document.RetainageApply == true;
            result.IsRetainageDocument = document.IsRetainageDocument == true;
            result.IsRetainageDebAdj = result.IsDocumentDebitAdjustment && (document.IsOriginalRetainageDocument() || document.IsChildRetainageDocument());

            result.IsDocumentRejectedOrPendingApproval =
                !result.IsDocumentOnHold &&
                !result.IsDocumentScheduled &&
                !result.IsDocumentReleasedOrPrebooked &&
                !result.IsDocumentVoided && (
                    result.IsDocumentRejected ||
                    document.Approved != true && document.DontApprove != true);

            result.IsDocumentApprovedBalanced =
                !result.IsDocumentOnHold &&
                !result.IsDocumentScheduled &&
                !result.IsDocumentReleasedOrPrebooked &&
                !result.IsDocumentVoided &&
                document.Approved == true &&
                document.DontApprove != true;

            result.LandedCostEnabled = false;

           
            if (document.VendorID.HasValue && document.VendorLocationID.HasValue &&
                (result.IsDocumentInvoice || result.IsDocumentDebitAdjustment))
            {
                if (this.vendor.Current != null)
                {
                    result.LandedCostEnabled = ((bool)this.vendor.Current.LandedCostVendor);

                    if (result.LandedCostEnabled == false && PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
                    {
                        var hasRelatedLandedCostVendor = PXSelect<Vendor, Where<Vendor.landedCostVendor, Equal<True>, And<Vendor.payToVendorID, Equal<Required<Vendor.payToVendorID>>>>>
                            .SelectWindowed(this, 0, 1, vendor.Current.BAccountID).Any();

                        result.LandedCostEnabled = hasRelatedLandedCostVendor;
                    }
                }
            }

            result.AllowAddPOByProject = (APSetup.Current.RequireSingleProjectPerDocument != true || document.ProjectID != null);

			result.IsCuryEnabled = (document.IsTaxDocument != true)
				&& (result.IsDocumentPrepayment ? !result.HasPOLink : vendor.Current?.AllowOverrideCury == true);

			result.IsFromPO = (document.OrigModule == BatchModule.PO);

            return result;
        }

		private Dictionary<Type, CachePermission> cachePermission = null;

        protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APInvoice document = e.Row as APInvoice;
			if (document == null) return;

			if (IsImport && APSetup.Current?.MigrationMode == true && document.IsMigratedRecord == true)
			{  // It needs because all caches are disabled in Migration Mode for not migrated documents. 
				// There is no code to Enable caches for new migrated document
				if (cachePermission!=null) 
				{
					this.LoadCachesPermissions(cachePermission);
					cachePermission = null;
				}
			}

            var invoiceState = GetDocumentState(cache, document);

			if (document.DontApprove != invoiceState.DontApprove)
			{
				cache.SetValueExt<APInvoice.dontApprove>(document, invoiceState.DontApprove);
			}

			// We need this for correct tabs repainting
			// in migration mode.
			// 
			Adjustments.Cache.AllowSelect = true;

			PXUIFieldAttribute.SetRequired<APInvoice.invoiceNbr>(cache, IsInvoiceNbrRequired(document));
			PXUIFieldAttribute.SetVisible<APInvoice.curyID>(cache, document, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			PXUIFieldAttribute.SetRequired<APInvoice.termsID>(cache, !invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentPrepayment);
			PXUIFieldAttribute.SetRequired<APInvoice.dueDate>(cache, !invoiceState.IsDocumentDebitAdjustment);
			PXUIFieldAttribute.SetRequired<APInvoice.discDate>(cache, !invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentPrepayment);

			this.viewPODocument.SetVisible(invoiceState.IsDocumentInvoice || invoiceState.IsDocumentDebitAdjustment);

            this.prebook.SetVisible(document.DocType != APDocType.Prepayment);
			this.voidInvoice.SetVisible(document.DocType != APDocType.Prepayment); 

			this.prebook.SetEnabled(false);
			this.voidInvoice.SetEnabled(false);

			bool vendor1099  = vendor.Current?.Vendor1099 == true;

			if (vendor.Current?.TaxAgency == true)
			{
				PXUIFieldAttribute.SetEnabled<APInvoice.taxZoneID>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APTran.taxCategoryID>(Transactions.Cache, null, false);
			}

			document.LCEnabled = invoiceState.LandedCostEnabled;

			cache.AllowDelete = true;
			cache.AllowUpdate = true;
			Transactions.Cache.AllowDelete = true;
			Transactions.Cache.AllowUpdate = true;
			Transactions.Cache.AllowInsert = (document.VendorID != null) && (document.VendorLocationID != null);

			if (invoiceState.IsDocumentReleasedOrPrebooked || invoiceState.IsDocumentVoided)
			{
				bool Enable1099 = (vendor.Current != null && vendor.Current.Vendor1099 == true && document.Voided == false);
				bool hasAdjustments = false;
				bool isUnreleasedPPD = document.Released != true && document.PendingPPD == true;

				if (isUnreleasedPPD)
				{
					recalculateDiscountsAction.SetEnabled(false);
				}

				foreach (APAdjust adj in Adjustments.Select())
				{
					string year1099 = ((DateTime)adj.AdjgDocDate).Year.ToString();

					AP1099Year year = PXSelect<AP1099Year,
											Where<AP1099Year.finYear, Equal<Required<AP1099Year.finYear>>,
													And<AP1099Year.organizationID, Equal<Required<AP1099Year.organizationID>>>>>
											.Select(this, year1099, PXAccess.GetParentOrganizationID(adj.AdjgBranchID));

					if (year != null && year.Status != AP1099Year.status.Open)
					{
						Enable1099 = false;
					}

					hasAdjustments = true;

					if (hasAdjustments & !Enable1099) break;
				}

				PXUIFieldAttribute.SetEnabled(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.dueDate>(cache, document,
					!invoiceState.IsDocumentDebitAdjustment && (bool) document.OpenDoc && document.PendingPPD != true);
				PXUIFieldAttribute.SetEnabled<APInvoice.paySel>(cache, document,
					!invoiceState.IsDocumentDebitAdjustment && (bool) document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payLocationID>(cache, document, (bool)document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payAccountID>(cache, document, (bool)document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payTypeID>(cache, document, (bool)document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payDate>(cache, document,
					!invoiceState.IsDocumentDebitAdjustment && (bool) document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.discDate>(cache, document,
					!invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentPrepayment && (bool) document.OpenDoc && document.PendingPPD != true);

				cache.AllowDelete = false;
				cache.AllowUpdate = (bool)document.OpenDoc || Enable1099 || invoiceState.IsDocumentPrebookedNotCompleted;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = Enable1099 || invoiceState.IsDocumentPrebookedNotCompleted;
				Transactions.Cache.AllowInsert = false;

				DiscountDetails.Cache.SetAllEditPermissions(allowEdit: false);

				Taxes.Cache.AllowUpdate = false;

				release.SetEnabled(invoiceState.IsDocumentPrebookedNotCompleted);

				bool hasPOLinks = !hasAdjustments && invoiceState.HasPOLink;

				if (this._allowToVoidReleased)
				{
					voidInvoice.SetEnabled((document.Released == true || document.Prebooked == true) && !hasPOLinks &&
					                       document.Voided == false && !hasAdjustments);
				}
				else
				{
					voidInvoice.SetEnabled(invoiceState.IsDocumentPrebookedNotCompleted && !hasPOLinks);
				}

				createSchedule.SetEnabled(false);
				payInvoice.SetEnabled((bool) document.OpenDoc &&
				                      ((bool) document.Payable || document.DocType == APInvoiceType.Prepayment));

				if (Enable1099 || invoiceState.IsDocumentPrebookedNotCompleted)
				{
					PXUIFieldAttribute.SetEnabled(Transactions.Cache, null, false);
					PXUIFieldAttribute.SetEnabled<APTran.box1099>(Transactions.Cache, null, Enable1099);

					PXUIFieldAttribute.SetEnabled<APTran.accountID>(Transactions.Cache, null, invoiceState.IsDocumentPrebookedNotCompleted);
					PXUIFieldAttribute.SetEnabled<APTran.subID>(Transactions.Cache, null, invoiceState.IsDocumentPrebookedNotCompleted);
					PXUIFieldAttribute.SetEnabled<APTran.branchID>(Transactions.Cache, null, invoiceState.IsDocumentPrebookedNotCompleted);
					PXUIFieldAttribute.SetEnabled<APTran.projectID>(Transactions.Cache, null, invoiceState.IsDocumentPrebookedNotCompleted);
					PXUIFieldAttribute.SetEnabled<APTran.taskID>(Transactions.Cache, null, invoiceState.IsDocumentPrebookedNotCompleted);
				}
			}
			else if (invoiceState.IsDocumentRejectedOrPendingApproval || invoiceState.IsDocumentApprovedBalanced)
			{
				PXUIFieldAttribute.SetEnabled(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APRegister.hold>(cache, document);

				// For non-rejected documents, default payment info should be editable.
				// -
				PXUIFieldAttribute.SetEnabled<APInvoice.separateCheck>(cache, document, !invoiceState.IsDocumentRejected);
				PXUIFieldAttribute.SetEnabled<APInvoice.paySel>(cache, document, !invoiceState.IsDocumentRejected);
				PXUIFieldAttribute.SetEnabled<APInvoice.payDate>(cache, document, !invoiceState.IsDocumentRejected);
				PXUIFieldAttribute.SetEnabled<APInvoice.payLocationID>(cache, document, !invoiceState.IsDocumentRejected);
				PXUIFieldAttribute.SetEnabled<APInvoice.payTypeID>(cache, document, !invoiceState.IsDocumentRejected);
				PXUIFieldAttribute.SetEnabled<APInvoice.payAccountID>(cache, document, !invoiceState.IsDocumentRejected);

				Transactions.Cache.SetAllEditPermissions(allowEdit: false);
				DiscountDetails.Cache.SetAllEditPermissions(allowEdit: false);
				Taxes.Cache.SetAllEditPermissions(allowEdit: false);

				release.SetEnabled(invoiceState.IsDocumentApprovedBalanced);
				prebook.SetEnabled(invoiceState.IsDocumentApprovedBalanced && !invoiceState.IsRetainageDocument && !invoiceState.RetainageApply);

				createSchedule.SetEnabled(false);
				payInvoice.SetEnabled(false);
				recalculateDiscountsAction.SetEnabled(false);
			}
			else if (invoiceState.IsRetainageDebAdj)
			{
				PXUIFieldAttribute.SetEnabled(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.docDesc>(cache, document, true);
				PXUIFieldAttribute.SetEnabled<APInvoice.hold>(cache, document, true);
				PXUIFieldAttribute.SetEnabled<APInvoice.docDate>(cache, document, true);
				PXUIFieldAttribute.SetEnabled<APInvoice.finPeriodID>(cache, document, true);
				release.SetEnabled(!invoiceState.IsDocumentOnHold && !invoiceState.IsDocumentScheduled);
				recalculateDiscountsAction.SetEnabled(false);
				DiscountDetails.Cache.SetAllEditPermissions(allowEdit: false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, document, true);
				PXUIFieldAttribute.SetEnabled<APInvoice.status>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyDocBal>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyLineTotal>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyTaxTotal>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyOrigWhTaxAmt>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyVatExemptTotal>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyVatTaxableTotal>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.batchNbr>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyID>(cache, document, invoiceState.IsCuryEnabled);
				PXUIFieldAttribute.SetEnabled<APInvoice.hold>(cache, document, (bool)document.Scheduled == false);

				PXUIFieldAttribute.SetEnabled<APInvoice.termsID>(cache, document,
					!invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentPrepayment);
				PXUIFieldAttribute.SetEnabled<APInvoice.dueDate>(cache, document, !invoiceState.IsDocumentDebitAdjustment);
				PXUIFieldAttribute.SetEnabled<APInvoice.discDate>(cache, document,
					!invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentPrepayment);

				Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(cache, document);
				bool termsMultiple = terms?.InstallmentType == TermsInstallmentType.Multiple;
				PXUIFieldAttribute.SetEnabled<APInvoice.curyOrigDiscAmt>(cache, document,
					(!invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentPrepayment && !termsMultiple));

				PXUIFieldAttribute.SetEnabled(Transactions.Cache, null, true);

				//PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(Transactions.Cache, null, (doc.DocType == APDocType.Invoice || doc.DocType == APDocType.CreditAdj));
				PXUIFieldAttribute.SetEnabled<APTran.defScheduleID>(Transactions.Cache, null,
					(document.DocType == APDocType.DebitAdj));
				PXUIFieldAttribute.SetEnabled<APTran.curyTranAmt>(Transactions.Cache, null, false);
				PXUIFieldAttribute.SetEnabled<APTran.discountSequenceID>(Transactions.Cache, null, false);
				PXUIFieldAttribute.SetEnabled<APTran.baseQty>(Transactions.Cache, null, false);

				//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
				PXDBCurrencyAttribute.SetBaseCalc<APInvoice.curyDocBal>(cache, null, true);
				PXDBCurrencyAttribute.SetBaseCalc<APInvoice.curyDiscBal>(cache, null, true);

				PXUIFieldAttribute.SetEnabled<APInvoice.payAccountID>(cache, document, (bool)document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payTypeID>(cache, document, (bool)document.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payDate>(cache, document, !invoiceState.IsDocumentDebitAdjustment);

				DiscountDetails.Cache.SetAllEditPermissions(!invoiceState.RetainageApply);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyDiscTot>(cache, document, !PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>());

				Vendor vendorRecord = this.vendor.Select();

				if (vendorRecord != null)
				{
					if (vendorRecord.Status == BAccount.status.Inactive)
					{
						cache.RaiseExceptionHandling<APInvoice.vendorID>(document, vendorRecord.AcctCD,
							new PXSetPropertyException(Messages.VendorIsInStatus, PXErrorLevel.Warning, CR.Messages.Inactive));
					}
				}

				Taxes.Cache.AllowUpdate = true;

				release.SetEnabled(!invoiceState.IsDocumentOnHold && !invoiceState.IsDocumentScheduled);
				prebook.SetEnabled(document.DocType != APDocType.Prepayment && !invoiceState.IsDocumentOnHold && !invoiceState.IsDocumentScheduled
                    && !invoiceState.IsRetainageDocument && !invoiceState.RetainageApply);
				createSchedule.SetEnabled(!invoiceState.IsDocumentOnHold && document.DocType == APDocType.Invoice);
				payInvoice.SetEnabled(false);

				PXUIFieldAttribute.SetEnabled<APInvoice.retainageApply>(cache, document, !invoiceState.IsDocumentDebitAdjustment && !invoiceState.IsDocumentCreditAdjustment);
				PXUIFieldAttribute.SetEnabled<APInvoice.projectID>(cache, document, !invoiceState.HasPOLink && !invoiceState.IsFromExpenseClaims && !invoiceState.IsRetainageDocument);
				PXUIFieldAttribute.SetEnabled<APInvoice.taxZoneID>(cache, document, !invoiceState.IsRetainageDocument);
				PXUIFieldAttribute.SetEnabled<APInvoice.taxCalcMode>(cache, document, !invoiceState.IsRetainageDocument);
				PXUIFieldAttribute.SetEnabled<APInvoice.branchID>(cache, document, !invoiceState.IsRetainageDocument);
				
				PXUIFieldAttribute.SetEnabled<APTran.curyRetainageAmt>(Transactions.Cache, null, invoiceState.RetainageApply);
				PXUIFieldAttribute.SetEnabled<APTran.retainagePct>(Transactions.Cache, null, invoiceState.RetainageApply);
			}
			PXUIFieldAttribute.SetEnabled<APTran.pOAccrualType>(Transactions.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<APInvoice.docType>(cache, document);
			PXUIFieldAttribute.SetEnabled<APInvoice.refNbr>(cache, document);

			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete && !invoiceState.IsRetainageDebAdj && !invoiceState.IsRetainageDocument;
			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert && !invoiceState.IsRetainageDebAdj && !invoiceState.IsRetainageDocument;
			Taxes.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && !invoiceState.IsRetainageDebAdj && !invoiceState.IsRetainageDocument;

			Adjustments.Cache.AllowInsert = false;
			Adjustments.Cache.AllowDelete = false;
			Adjustments.Cache.AllowUpdate = !invoiceState.IsRetainageDebAdj &&
                invoiceState.IsDocumentRejectedOrPendingApproval || invoiceState.IsDocumentApprovedBalanced
                    ? !invoiceState.IsDocumentRejected
                    : Transactions.Cache.AllowUpdate && !invoiceState.IsDocumentPrebookedNotCompleted;

			editVendor.SetEnabled(vendor?.Current != null && !invoiceState.IsRetainageDebAdj);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjgBranchID>(Adjustments.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APAdjust.displayDocType>(Adjustments.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APAdjust.displayRefNbr>(Adjustments.Cache, null, false);


			vendorRefund.SetEnabled(invoiceState.IsDocumentDebitAdjustment && invoiceState.IsDocumentReleasedOrPrebooked &&
                !invoiceState.IsDocumentPrepayment && !invoiceState.IsRetainageDebAdj);
			reclassifyBatch.SetEnabled(document.Released == true && !invoiceState.IsDocumentPrepayment && !invoiceState.IsRetainageDebAdj);

			if (Transactions.Any())
			{
				PXUIFieldAttribute.SetEnabled<APInvoice.vendorID>(cache, document, document.VendorID == null);
				PXUIFieldAttribute.SetEnabled<APInvoice.paymentsByLinesAllowed>(cache, document, false);
			}

			if (document.VendorLocationID != null && !invoiceState.IsRetainageDebAdj)
			{
				PXUIFieldAttribute.SetEnabled<APInvoice.vendorLocationID>(
					cache, 
					document, 
					!(
                        !invoiceState.IsDocumentEditable ||
						(bool)document.Voided
					));
			}
			PXUIFieldAttribute.SetVisible<APInvoice.curyOrigDocAmt>(cache, document,
				APSetup.Current.RequireControlTotal == true || invoiceState.IsDocumentReleasedOrPrebooked);
			PXUIFieldAttribute.SetVisible<APInvoice.curyTaxAmt>(cache, document,
				PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() &&
				(APSetup.Current.RequireControlTaxTotal == true));
			PXUIFieldAttribute.SetVisible<APTran.box1099>(Transactions.Cache, null, vendor1099);
			PXUIFieldAttribute.SetVisible<APInvoice.prebookBatchNbr>(cache, document, document.Prebooked == true);
			PXUIFieldAttribute.SetVisible<APInvoice.voidBatchNbr>(cache, document, document.Voided == true);
			PXUIFieldAttribute.SetVisible<APInvoice.batchNbr>(cache, document, document.Voided != true);

			bool showRoundingDiff = document.CuryRoundDiff != 0 || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>();
			PXUIFieldAttribute.SetVisible<APInvoice.curyRoundDiff>(cache, document, showRoundingDiff);

			PXResultset<APTaxTran> useTaxes = UseTaxes.Select();
			if (useTaxes.Count != 0 && !UnattendedMode)
			{
				cache.RaiseExceptionHandling<APInvoice.curyTaxTotal>(document, document.CuryTaxTotal,
					new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}

			cache.RaiseExceptionHandling<APInvoice.taxCalcMode>(document, document.TaxCalcMode, null);

			if (!UnattendedMode && document.Hold != true && document.Released != true && document.Prebooked != true)
			{
				POOrder poorder = FindPOOrderWithDifferentTaxCalcMode(document);
				
				if (poorder != null)
				{
					cache.RaiseExceptionHandling<APInvoice.taxCalcMode>(document, document.TaxCalcMode,
						new PXSetPropertyException(
							Messages.POWithDifferentTaxCalcModeFoundForTheBill,
							PXErrorLevel.Warning,
							TX.TaxCalculationMode.ListAttribute.GetLocalizedLabel<APInvoice.taxCalcMode>(cache, document),
							poorder.OrderNbr));
				}
			}

			PXUIFieldAttribute.SetVisible<APInvoice.usesManualVAT>(cache, document, document.UsesManualVAT == true);

			Taxes.Cache.AllowInsert = Taxes.Cache.AllowInsert && document.UsesManualVAT != true;
			Taxes.Cache.AllowDelete = Taxes.Cache.AllowDelete && document.UsesManualVAT != true;
			Taxes.Cache.AllowUpdate = Taxes.Cache.AllowUpdate && document.UsesManualVAT != true;

			Company company = PXSelect<Company>.Select(this);

			bool IsTaxDocumentVendorNotBaseCuryID = document.IsTaxDocument == true
													&& vendor.Current?.CuryID != null
													&& vendor.Current?.CuryID != company.BaseCuryID;

			Transactions.Cache.AllowDelete &= !invoiceState.IsRetainageDocument && !invoiceState.IsRetainageDebAdj && !IsTaxDocumentVendorNotBaseCuryID;
			Transactions.Cache.AllowInsert &= !invoiceState.IsRetainageDocument && !invoiceState.IsRetainageDebAdj && !IsTaxDocumentVendorNotBaseCuryID && !invoiceState.IsPrepaymentRequestFromPO;
			Transactions.Cache.AllowUpdate &= !invoiceState.IsRetainageDocument && !invoiceState.IsRetainageDebAdj && !IsTaxDocumentVendorNotBaseCuryID;

			bool isPayToVendor = PXSelect<Vendor, 
			    Where<Vendor.payToVendorID, Equal<Current<APInvoice.vendorID>>>, 
			    OrderBy<Asc<Vendor.bAccountID>>>
			    .SelectSingleBound(this, new object[] {document})
			    .AsEnumerable()
			    .Any();

			bool isSuppliedByVendorEnabled = !invoiceState.HasPOLink && document.Released != true && document.Prebooked != true && isPayToVendor && !invoiceState.IsRetainageDebAdj;
			PXUIFieldAttribute.SetEnabled<APInvoice.suppliedByVendorID>(cache, document, isSuppliedByVendorEnabled);
			PXUIFieldAttribute.SetVisible<APInvoice.suppliedByVendorID>(cache, document, document.DocType == APDocType.Invoice || document.DocType == APDocType.DebitAdj);

			PXUIFieldAttribute.SetEnabled<APInvoice.suppliedByVendorLocationID>(cache, document, isSuppliedByVendorEnabled && document.VendorID != document.SuppliedByVendorID);
			PXUIFieldAttribute.SetVisible<APInvoice.suppliedByVendorLocationID>(cache, document, document.DocType == APDocType.Invoice || document.DocType == APDocType.DebitAdj);

			#region Retainage

			bool isRetainageApplyInvoice =
                invoiceState.IsDocumentInvoice &&
				!invoiceState.IsFromExpenseClaims &&
				(!invoiceState.IsDocumentReleasedOrPrebooked &&
				document.IsRetainageDocument != true ||
                invoiceState.RetainageApply);

			bool isRetainageApplyDebitAdjustment =
                invoiceState.IsDocumentDebitAdjustment &&
				!invoiceState.IsFromExpenseClaims &&
                invoiceState.RetainageApply;

			bool visibleProjectID = (invoiceState.IsDocumentInvoice || invoiceState.IsDocumentDebitAdjustment || invoiceState.IsDocumentCreditAdjustment) && 
				APSetup.Current.RequireSingleProjectPerDocument == true;

			PXUIFieldAttribute.SetVisible<APInvoice.retainageAcctID>(cache, document, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetVisible<APInvoice.retainageSubID>(cache, document, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetVisible<APInvoice.retainageApply>(cache, document, isRetainageApplyInvoice || isRetainageApplyDebitAdjustment);
			PXUIFieldAttribute.SetVisible<APInvoice.isRetainageDocument>(cache, document, !isRetainageApplyInvoice && invoiceState.IsRetainageDocument);
			PXUIFieldAttribute.SetVisible<APInvoice.projectID>(cache, document, visibleProjectID);
			PXUIFieldAttribute.SetVisible<APTran.retainagePct>(Transactions.Cache, null, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetVisible<APTran.curyRetainageAmt>(Transactions.Cache, null, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetVisible<APTaxTran.curyRetainedTaxableAmt>(Taxes.Cache, null, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetVisible<APTaxTran.curyRetainedTaxAmt>(Taxes.Cache, null, invoiceState.RetainageApply);

			PXUIFieldAttribute.SetRequired<APInvoice.retainageAcctID>(cache, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetRequired<APInvoice.retainageSubID>(cache, invoiceState.RetainageApply);
			PXUIFieldAttribute.SetRequired<APInvoice.projectID>(cache, visibleProjectID && !invoiceState.HasPOLink && !invoiceState.IsFromExpenseClaims && invoiceState.RetainageApply);

			PXDefaultAttribute.SetPersistingCheck<APInvoice.projectID>(cache, document, visibleProjectID && 
                !invoiceState.HasPOLink && !invoiceState.IsFromExpenseClaims && invoiceState.RetainageApply
				? PXPersistingCheck.NullOrBlank
				: PXPersistingCheck.Nothing);

			#endregion

			#region VAT Recalculating

			bool showCashDiscountInfo = false;
			if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() &&
				document.CuryOrigDiscAmt > 0m &&
				document.DocType != APDocType.DebitAdj )
			{
				Taxes.Select();
				showCashDiscountInfo = document.HasPPDTaxes == true;
			}

			PXUIFieldAttribute.SetVisible<APInvoice.curyDiscountedDocTotal>(cache, e.Row, showCashDiscountInfo);
			PXUIFieldAttribute.SetVisible<APInvoice.curyDiscountedTaxableTotal>(cache, e.Row, showCashDiscountInfo);
			PXUIFieldAttribute.SetVisible<APInvoice.curyDiscountedPrice>(cache, e.Row, showCashDiscountInfo);

			PXUIVisibility visibility = showCashDiscountInfo ? PXUIVisibility.Visible : PXUIVisibility.Invisible;
			PXUIFieldAttribute.SetVisibility<APTaxTran.curyDiscountedPrice>(Taxes.Cache, null, visibility);
			PXUIFieldAttribute.SetVisibility<APTaxTran.curyDiscountedTaxableAmt>(Taxes.Cache, null, visibility);

			Taxes.View.RequestRefresh();

			if (PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				document.RetainageApply == true &&
				useTaxes.RowCast<APTaxTran>().Where(taxtran => taxtran.CuryRetainedTaxAmt != 0m).Any() &&
				!UnattendedMode)
			{
				cache.RaiseExceptionHandling<APInvoice.curyRetainedTaxTotal>(document, document.CuryRetainedTaxTotal,
					new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}

			#endregion

			#region Payments By Lines Settings

			bool isPaymentsByLinesAllowed =
				PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>() &&
				document.PaymentsByLinesAllowed == true;

			PXUIFieldAttribute.SetVisible<APTran.lineNbr>(Transactions.Cache, null, isPaymentsByLinesAllowed);

			if (isPaymentsByLinesAllowed)
			{
				autoApply.SetVisible(false);

				Adjustments.Cache.SetAllEditPermissions(false);
				DiscountDetails.Cache.SetAllEditPermissions(false);

				if (!invoiceState.IsDocumentReleasedOrPrebooked && !UnattendedMode)
				{
					cache.RaiseExceptionHandling<APInvoice.curyDiscTot>(document, document.CuryDiscTot,
						new PXSetPropertyException(Messages.PaymentsByLinesDiscountsNotSupported, PXErrorLevel.Warning));

					cache.RaiseExceptionHandling<APInvoice.curyOrigWhTaxAmt>(document, document.CuryOrigWhTaxAmt,
						new PXSetPropertyException(Messages.PaymentsByLinesWithholdingTaxesNotSupported, PXErrorLevel.Warning));
				}

				if (document.Released == true)
				{
					foreach (APAdjust adj in Adjustments.Select().RowCast<APAdjust>().Where(a => a.AdjdLineNbr == 0 && a.Released != true))
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.adjgRefNbr>(adj, adj.AdjgRefNbr,
							new PXSetPropertyException(Messages.NotDistributedApplicationCannotBeReleased, PXErrorLevel.RowWarning));
					}
				}
			}

			if (invoiceState.IsDocumentDebitAdjustment)
			{
				PXUIFieldAttribute.SetEnabled<APInvoice.paymentsByLinesAllowed>(cache, document, false);
			}

			if (invoiceState.IsDocumentPrepayment)
			{
				PXUIFieldAttribute.SetEnabled<APInvoice.paymentsByLinesAllowed>(cache, document, false);
			}

			PXUIFieldAttribute.SetEnabled<APTran.curyCashDiscBal>(Transactions.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APTran.curyRetainageBal>(Transactions.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APTran.curyTranBal>(Transactions.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APTran.curyOrigTaxAmt>(Transactions.Cache, null, false);

			PXUIFieldAttribute.SetVisible<APTran.curyCashDiscBal>(Transactions.Cache, null, 
				isPaymentsByLinesAllowed && 
				invoiceState.IsDocumentReleasedOrPrebooked && 
				document.CuryOrigDiscAmt != 0m);

			bool showRetainageLineBalance =
				isPaymentsByLinesAllowed &&
				invoiceState.IsDocumentReleasedOrPrebooked &&
				invoiceState.RetainageApply;
			PXUIFieldAttribute.SetVisible<APTran.curyRetainageBal>(Transactions.Cache, null, showRetainageLineBalance);
			PXUIFieldAttribute.SetVisible<APTran.curyRetainedTaxAmt>(Transactions.Cache, null, showRetainageLineBalance);

			bool showLineBalances =
				isPaymentsByLinesAllowed &&
				invoiceState.IsDocumentReleasedOrPrebooked;
			PXUIFieldAttribute.SetVisible<APTran.curyTranBal>(Transactions.Cache, null, showLineBalances);
			PXUIFieldAttribute.SetVisible<APTran.curyOrigTaxAmt>(Transactions.Cache, null, showLineBalances);

			#endregion

			#region Migration Mode Settings

			bool isMigratedDocument = document.IsMigratedRecord == true;
			bool isUnreleasedMigratedDocument = isMigratedDocument && document.Released != true;
			bool isReleasedMigratedDocument = isMigratedDocument && document.Released == true;
			bool isMigrationMode = APSetup.Current?.MigrationMode == true;

			PXUIFieldAttribute.SetVisible<APInvoice.curyDocBal>(cache, document, !isUnreleasedMigratedDocument);
			PXUIFieldAttribute.SetVisible<APInvoice.curyInitDocBal>(cache, document, isUnreleasedMigratedDocument);
			PXUIFieldAttribute.SetVisible<APInvoice.displayCuryInitDocBal>(cache, document, isReleasedMigratedDocument);

			if (isMigrationMode)
			{
				PXUIFieldAttribute.SetVisible<APInvoice.retainageApply>(cache, document, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.paymentsByLinesAllowed>(cache, document, false);
			}

			if (isUnreleasedMigratedDocument)
			{
				Adjustments.Cache.AllowSelect = false;
			}

			bool disableCaches = isMigrationMode
				? !isMigratedDocument
				: isUnreleasedMigratedDocument;
			if (disableCaches)
			{
				bool primaryCacheAllowInsert = Document.Cache.AllowInsert;
				bool primaryCacheAllowDelete = Document.Cache.AllowDelete;

				if (IsImport && cachePermission == null) 
				{
					cachePermission=this.SaveCachesPermissions(true);
				}
				this.DisableCaches();
				Document.Cache.AllowInsert = primaryCacheAllowInsert;
				Document.Cache.AllowDelete = primaryCacheAllowDelete;
			}

			// We should notify the user that initial balance can be entered,
			// if there are now any errors on this box.
			// 
			if (isUnreleasedMigratedDocument && 
				string.IsNullOrEmpty(PXUIFieldAttribute.GetError<APInvoice.curyInitDocBal>(cache, document)))
			{
				cache.RaiseExceptionHandling<APInvoice.curyInitDocBal>(document, document.CuryInitDocBal,
					new PXSetPropertyException(Messages.EnterInitialBalanceForUnreleasedMigratedDocument, PXErrorLevel.Warning));
			}
			#endregion

			Transactions.Cache.Adjust<PXUIFieldAttribute>()
				.For<APTran.prepaymentPct>(a =>
				{
					a.Enabled = invoiceState.IsDocumentPrepayment;
					a.Visible = invoiceState.IsDocumentPrepayment;
				})
				.For<APTran.curyPrepaymentAmt>(a =>
				{
					a.Enabled = false;
					a.Visible = invoiceState.IsDocumentPrepayment;
				});

			cache.RaiseExceptionHandling<APInvoice.curyRoundDiff>(document, null, null);

			bool checkControlTaxTotal = APSetup.Current.RequireControlTaxTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

			if (document.Hold != true && document.Released != true && document.Prebooked != true && document.RoundDiff != 0)
			{
				if (checkControlTaxTotal || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>() && document.TaxRoundDiff == 0)
				{
					if (Math.Abs(document.RoundDiff.Value) > Math.Abs(glsetup.Current.RoundingLimit.Value))
					{
						cache.RaiseExceptionHandling<APInvoice.curyRoundDiff>(document, document.CuryRoundDiff,
							new PXSetPropertyException(Messages.RoundingAmountTooBig, currencyinfo.Current.BaseCuryID, PXDBQuantityAttribute.Round(document.RoundDiff),
								PXDBQuantityAttribute.Round(glsetup.Current.RoundingLimit)));
					}
				}
				else
				{
					cache.RaiseExceptionHandling<APInvoice.curyRoundDiff>(document, document.CuryRoundDiff,
						PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>()
						? new PXSetPropertyException(Messages.CannotEditTaxAmtWOAPSetup, document.OrigModule == BatchModule.EP ? PXErrorLevel.Warning : PXErrorLevel.Error)
						: new PXSetPropertyException(Messages.CannotEditTaxAmtWOFeature));
				}
			}

			if (!invoiceState.IsDocumentReleasedOrPrebooked
				&& !UnattendedMode
				&& document.RoundDiff == 0
				&& PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>())
			{
				if (invoiceState.RetainageApply)
				{
					cache.RaiseExceptionHandling<APInvoice.curyRoundDiff>(document, document.CuryRoundDiff,
					new PXSetPropertyException(AP.Messages.RetainageInvoiceRoundingNotSupported, PXErrorLevel.Warning));
				}
				else if (isPaymentsByLinesAllowed)
				{
					cache.RaiseExceptionHandling<APInvoice.curyRoundDiff>(document, document.CuryRoundDiff,
						new PXSetPropertyException(Messages.PaymentsByLinesInvoiceRoundingNotSupported, PXErrorLevel.Warning));
				}
			}

			if (this.Document.Current != null && this.Document.Current.SetWarningOnDiscount == true)
			{
				this.Document.Cache.RaiseExceptionHandling<APInvoice.curyDiscTot>(this.Document.Current, this.Document.Current.CuryDiscTot,
								new PXSetPropertyException(Messages.DiscountInOriginalPOFoundTrace, PXErrorLevel.Warning));
			}

			PXUIFieldAttribute.SetVisible<APRegister.taxCostINAdjRefNbr>(cache, e.Row,
				document.DocType?.IsIn(APDocType.DebitAdj, APDocType.Invoice) == true);

		}

		private POOrder FindPOOrderWithDifferentTaxCalcMode(APInvoice doc)
		{
			var orders = PXSelectJoin<
							APTran,
							InnerJoin<POOrder, On<
								POOrder.orderType, Equal<APTran.pOOrderType>,
								And<POOrder.orderNbr, Equal<APTran.pONbr>>>>,
							Where<
								APTran.tranType, Equal<Current<APInvoice.docType>>,
								And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
								And<POOrder.taxCalcMode, NotEqual<Current<APInvoice.taxCalcMode>>>>>>
							.SelectSingleBound(this, new[] { doc })
							.AsEnumerable();

			if (orders.Any())
			{
				return PXResult.Unwrap<POOrder>(orders.First());
			}

			var receipts = PXSelectJoin<
							APTran,
							InnerJoin<POReceiptLine, On<
								POReceiptLine.receiptType, Equal<APTran.receiptType>,
								And<POReceiptLine.receiptNbr, Equal<APTran.receiptNbr>,
								And<POReceiptLine.lineNbr, Equal<APTran.receiptLineNbr>>>>,
							InnerJoin<POOrder, On<
								POOrder.orderType, Equal<POReceiptLine.pOType>,
								And<POOrder.orderNbr, Equal<POReceiptLine.pONbr>>>>>,
							Where<
								APTran.tranType, Equal<Current<APInvoice.docType>>,
								And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
								And<POOrder.taxCalcMode, NotEqual<Current<APInvoice.taxCalcMode>>>>>>
							.SelectSingleBound(this, new[] { doc })
							.AsEnumerable();

			if (receipts.Any())
			{
				return PXResult.Unwrap<POOrder>(receipts.First());
			}

			return null;
		}

		protected virtual void APInvoice_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.payAccountID>(e.Row);
		}

		protected virtual void APInvoice_PayAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;

			if (doc == null)
				return;

			CashAccount cashacct = PXSelectReadonly<CashAccount, 
				Where<CashAccount.cashAccountID, Equal<Required<APInvoice.payAccountID>>>>
				.Select(this, e.NewValue);
			// PXSelector.Select is semantically incorrect. Here, we want to retrieve an cash account by ID without additional conditions
			if (cashacct != null)
			{
				if (cashacct.RestrictVisibilityWithBranch == true && cashacct.BranchID != doc.BranchID)
				{
					e.NewValue = null; // TODO: Need to redesign and remove this string. FieldVerifying event must not modify the validating value
				}
			}
		}

		protected virtual void APInvoice_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.payAccountID>(e.Row);
		}

		protected virtual void APInvoice_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APInvoice row = e.Row as APInvoice;
			if (row == null) return;

			if (APSetup.Current.RequireSingleProjectPerDocument == true && !row.IsChildRetainageDocument())
			{
				foreach (APTran tran in Transactions.Select())
				{
					if (tran.ProjectID != row.ProjectID)
					{
						try
						{
							tran.ProjectID = row.ProjectID;
							Transactions.Update(tran);
						}
						catch (PXException ex)
						{
							PXFieldState projectIDState = (PXFieldState)sender.GetStateExt<APInvoice.projectID>(row);
							Transactions.Cache.RaiseExceptionHandling<APTran.projectID>(tran, projectIDState.Value, ex);
						}
					}
				}
			}
		}

		private void ValidateAPAndReclassificationAccountsAndSubs(PXCache sender, APInvoice invoice)
		{
			if(PXAccess.FeatureInstalled<FeaturesSet.prebooking>() == false)
			{
				return;
			}

			string errorMsg = null;

			var subsEnbaled = PXAccess.FeatureInstalled<FeaturesSet.subAccount>();

			var accountIdentical = invoice.PrebookAcctID == invoice.APAccountID;

			if (accountIdentical && !subsEnbaled)
			{
				errorMsg = Messages.APAndReclassAccountBoxesShouldNotHaveTheSameAccounts;
			}
			else if (accountIdentical && subsEnbaled && invoice.PrebookSubID == invoice.APSubID)
			{
				errorMsg = Messages.APAndReclassAccountSubaccountBoxesShouldNotHaveTheSameAccountSubaccountPairs;
			}

			if (errorMsg != null)
			{
				var errorEx = new PXSetPropertyException(errorMsg, PXErrorLevel.Error);

				var acctIDState = (PXFieldState) sender.GetStateExt<APInvoice.prebookAcctID>(invoice);
				sender.RaiseExceptionHandling<APInvoice.prebookAcctID>(invoice, acctIDState.Value, errorEx);

				var subIDState = (PXFieldState) sender.GetStateExt<APInvoice.prebookSubID>(invoice);
				sender.RaiseExceptionHandling<APInvoice.prebookSubID>(invoice, subIDState.Value, errorEx);
			}
		}

	    protected bool changedSuppliedByVendorLocation = false;
		protected virtual void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;
			if (doc == null) return;

			if (doc.Released != true)
			{
				if (e.ExternalCall && 
					((!sender.ObjectsEqual<APInvoice.docDate, APInvoice.retainageApply>(e.OldRow, e.Row) && doc.OrigDocType == null && doc.OrigRefNbr == null) 
                        || !sender.ObjectsEqual<APInvoice.vendorLocationID>(e.OldRow, e.Row)
                        || (changedSuppliedByVendorLocation = !sender.ObjectsEqual<APInvoice.suppliedByVendorLocationID>(e.OldRow, e.Row))))
				{
					_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails, doc.VendorLocationID, doc.DocDate, DiscountEngine.DefaultAPDiscountCalculationParameters);
				}

				if (sender.GetStatus(doc) != PXEntryStatus.Deleted && !sender.ObjectsEqual<APInvoice.curyDiscTot>(e.OldRow, e.Row))
				{
					if (!sender.Graph.IsImport)
					{
						try
						{
							AddDiscount(sender, doc);
						}
						catch (PXException ex)
						{
							sender.RaiseExceptionHandling<APInvoice.curyDiscTot>(doc, doc.CuryDiscTot, ex);
						}
					}

					if (!_discountEngine.IsInternalDiscountEngineCall && e.ExternalCall)
					{
						_discountEngine.SetTotalDocDiscount(Transactions.Cache, Transactions, DiscountDetails,
							Document.Current.CuryDiscTot, DiscountEngine.DefaultAPDiscountCalculationParameters);
						RecalculateTotalDiscount();
					}
				}

				if (doc.Released != true && doc.Prebooked != true)
				{
					if (APSetup.Current.RequireControlTotal != true)
					{
						if (doc.CuryDocBal != doc.CuryOrigDocAmt)
						{
							if (doc.CuryDocBal != null && doc.CuryDocBal != 0)
								sender.SetValueExt<APInvoice.curyOrigDocAmt>(doc, doc.CuryDocBal);
							else
								sender.SetValueExt<APInvoice.curyOrigDocAmt>(doc, 0m);
						}
					}

					if (doc.DocType == APDocType.Prepayment && doc.DueDate == null)
					{
						sender.SetValue<APInvoice.dueDate>(e.Row, this.Accessinfo.BusinessDate);
					}
				}

				if (doc.Hold != true && doc.Released != true && doc.Prebooked != true)
				{
					if (doc.CuryDocBal != doc.CuryOrigDocAmt)
					{
						sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (doc.CuryOrigDocAmt < 0m)
					{
						if (APSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
						else
						{
							sender.RaiseExceptionHandling<APInvoice.curyDocBal>(doc, doc.CuryDocBal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
					}
					else
					{
						if (APSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, null, null);
						}
						else
						{
							sender.RaiseExceptionHandling<APInvoice.curyDocBal>(doc, null, null);
						}
					}
				}
			}

			bool checkControlTaxTotal = APSetup.Current.RequireControlTaxTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

			if (doc.Hold != true && doc.Released != true && doc.Prebooked != true
				&& doc.CuryTaxTotal != doc.CuryTaxAmt && checkControlTaxTotal)
			{
				sender.RaiseExceptionHandling<APInvoice.curyTaxAmt>(doc, doc.CuryTaxAmt, new PXSetPropertyException(Messages.TaxTotalAmountDoesntMatch));
			}
			else
			{
				if (checkControlTaxTotal)
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>() && doc.CuryTaxAmt < 0)
					{
						sender.RaiseExceptionHandling<APInvoice.curyTaxAmt>(doc, doc.CuryTaxAmt, new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero));
					}
					else
					{
					sender.RaiseExceptionHandling<APInvoice.curyTaxAmt>(doc, null, null);
				}
				}
				else
				{
					sender.SetValueExt<APInvoice.curyTaxAmt>(doc, doc.CuryTaxTotal != null && doc.CuryTaxTotal != 0 ? doc.CuryTaxTotal : 0m);
				}
			}
		}

		protected virtual void APInvoice_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPExpenseClaim claim = expenseclaim.Select();
			if (claim != null)
			{
				throw new PXException(Messages.DocumentCannotBeDeleted);
				}
		}
		#endregion

		#region APTran Events

		/// <summary>
		/// Sets Expense Account for items with Accrue Cost = true. See implementation in CostAccrual extension.
		/// </summary>
		public virtual void SetExpenseAccount(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		}

		protected virtual void APTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;

			if (row != null && row.AccrueCost == true)
			{
				SetExpenseAccount(sender, e);
			}

			// We should allow entering an AccountID for stock inventory
			// item when migration mode is activated in AP module.
			// 
			if (APSetup.Current?.MigrationMode != true &&
				row?.InventoryID != null)
			{
				InventoryItem item = nonStockItem.Select(row.InventoryID);
				if (item?.StkItem == true)
				{
					e.NewValue = null;
					e.Cancel = true;
					return;
				}
			}
			
			if (vendor.Current == null || row == null || row.InventoryID != null || IsCopyPasteContext || IsReverseContext) return;
			
			switch (vendor.Current.Type)
			{
				case BAccountType.VendorType:
				case BAccountType.CombinedType:
					if (location.Current.VExpenseAcctID != null)
					{
						e.NewValue = location.Current.VExpenseAcctID;
					}
					break;
				case BAccountType.EmployeeType:
					EPEmployee employeeVendor = EmployeeByVendor.Select();
					e.NewValue = employeeVendor.ExpenseAcctID ?? e.NewValue;
					break;
			}

			e.Cancel = true;
		}

		protected virtual void APTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (vendor.Current != null && (bool)vendor.Current.Vendor1099)
			{
				sender.SetDefaultExt<APTran.box1099>(e.Row);
			}

			if (row.ProjectID == null || row.ProjectID == ProjectDefaultAttribute.NonProject())
			{
				sender.SetDefaultExt<APTran.projectID>(e.Row);
			}
		}

		/// <summary>
		/// Sets Expense Subaccount for items with Accrue Cost = true. See implementation in CostAccrual extension.
		/// </summary>
		public virtual object GetExpenseSub(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			return null;
		}

		protected virtual void APTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran documentLine = e.Row as APTran;
			if (documentLine == null) return;

			// Supress defaulting during paste
			if (IsCopyPasteContext)
			{
				e.NewValue = documentLine.SubID;
				e.Cancel = true;
				return;
			}

			// We should allow entering a SubID for stock inventory
			// item when migration mode is activated in AP module.
			// 
			InventoryItem item = nonStockItem.Select(documentLine.InventoryID);
			if (APSetup.Current?.MigrationMode != true && item?.StkItem == true)
			{
				e.NewValue = null;
				e.Cancel = true;
				return;
			}

			if (vendor.Current?.Type == null ||
				!string.IsNullOrEmpty(documentLine.PONbr) ||
				!string.IsNullOrEmpty(documentLine.ReceiptNbr) ||
				IsReverseContext)
			{
				return;
			}

			EPEmployee employeeByUser = PXSelect<
				EPEmployee,
				Where<
					EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>
				.Select(this, Document.Current?.EmployeeID ?? PXAccess.GetUserID());

			CRLocation companyLocation = PXSelectJoin<
				CRLocation,
				InnerJoin<BAccountR,
					On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
					And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
				InnerJoin<Branch,
					On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>,
				Where<
					Branch.branchID, Equal<Required<APTran.branchID>>>>
				.Select(this, documentLine.BranchID);

			Contract project = PXSelect<
				Contract,
				Where<
					Contract.contractID, Equal<Required<Contract.contractID>>>>
				.Select(this, documentLine.ProjectID);

			string expenseSubMask = APSetup.Current.ExpenseSubMask;

			int? projectTaskSubaccountID = null;

			if (project == null || project.BaseType == CTPRType.Contract)
			{
				project = PXSelect<CT.Contract, Where<CT.Contract.nonProject, Equal<True>>>.Select(this);
				expenseSubMask = expenseSubMask.Replace(APAcctSubDefault.MaskTask, APAcctSubDefault.MaskProject);
			}
			else
			{
				PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, documentLine.TaskID);
				if (task != null)
					projectTaskSubaccountID = task.DefaultSubID;
			}

			int? vendorSubaccountID = null;

			switch (vendor.Current.Type)
			{
				case BAccountType.VendorType:
				case BAccountType.CombinedType:
					vendorSubaccountID = (int?)Caches[typeof(Location)].GetValue<Location.vExpenseSubID>(location.Current);
					break;
				case BAccountType.EmployeeType:
					EPEmployee employeeVendor = EmployeeByVendor.Select();
					vendorSubaccountID = employeeVendor.ExpenseSubID ?? vendorSubaccountID;
					break;
			}

			int? itemSubaccountID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.cOGSSubID>(item);
			int? employeeByUserSubaccountID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(employeeByUser);
			int? companySubaccountID = (int?)Caches[typeof(CRLocation)].GetValue<CRLocation.cMPExpenseSubID>(companyLocation);
			int? projectSubaccountID = project.DefaultSubID;

			object subaccountValue;
			if (documentLine != null && documentLine.AccrueCost == true)
			{
				subaccountValue = GetExpenseSub(sender, e);
			}
			else
			{
				subaccountValue = SubAccountMaskAttribute.MakeSub<APSetup.expenseSubMask>(
					this,
					expenseSubMask,
					new object[]
					{
					vendorSubaccountID,
					itemSubaccountID,
					employeeByUserSubaccountID,
					companySubaccountID,
					projectSubaccountID,
					projectTaskSubaccountID
					},
				new []
					{
					typeof(Location.vExpenseSubID),
					typeof(InventoryItem.cOGSSubID),
					typeof(EPEmployee.expenseSubID),
					typeof(Location.cMPExpenseSubID),
					typeof(PMProject.defaultSubID),
					typeof(PMTask.defaultSubID)
					});
			}

			sender.RaiseFieldUpdating<APTran.subID>(documentLine, ref subaccountValue);

			e.NewValue = (int?)subaccountValue;
			e.Cancel = true;

		}

		protected virtual void APTran_LCTranID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[APTax(typeof(APRegister), typeof(APTax), typeof(APTaxTran), typeof(APInvoice.taxCalcMode), parentBranchIDField: typeof(APRegister.branchID),
			   //Per Unit Tax settings
			   Inventory = typeof(APTran.inventoryID), UOM = typeof(APTran.uOM), LineQty = typeof(APTran.qty))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void APTran_TaxCategoryID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void APTran_TaxCategoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null) return;

			APInvoice doc = Document.Current;

			bool isRetainageDocument = 
				doc.IsOriginalRetainageDocument() || 
				doc.IsChildRetainageDocument();

			if (!IsCopyPasteContext && 
				!IsReverseContext &&
				!isRetainageDocument && 
				!IsPPDCreateContext &&
				!UnattendedMode &&
				!IsImport &&
				!this.IsContractBasedAPI && 
				row.PONbr == null && // skip PO

				APSetup.Current.RequireControlTotal == true && 
				PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() &&
				doc.TaxCalcMode == TaxCalculationMode.Net &&

				row.Qty == 0 &&
				(row.CuryLineAmt ?? 0m) != 0m)
			{
				PXResultset<APTax> taxes = PXSelect<APTax, 
					Where<APTax.tranType, Equal<Required<APTax.tranType>>, 
						And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
						And<APTax.lineNbr, Equal<Required<APTax.lineNbr>>>>>>
					.Select(this, row.TranType, row.RefNbr, row.LineNbr);

				decimal curyTaxSum = 0;
				foreach (APTax tax in taxes)
				{
					curyTaxSum += tax.CuryTaxAmt.Value;
				}

				decimal? taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(sender, row, doc.TaxZoneID,
							row.TaxCategoryID, doc.DocDate.Value, row.CuryLineAmt.Value + curyTaxSum, false, GLTaxAttribute.TaxCalcLevelEnforcing.EnforceCalcOnItemAmount, doc.TaxCalcMode);
				sender.SetValueExt<APTran.curyLineAmt>(row, taxableAmount);
			}
		}

		protected virtual void APTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || row.InventoryID != null || vendor == null || vendor.Current == null || vendor.Current.TaxAgency == true) return;

			if (TaxAttribute.GetTaxCalc<APTran.taxCategoryID>(sender, row) == TaxCalc.Calc &&
			 taxzone.Current != null &&
			 !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID))
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || row.InventoryID != null) return;
			e.NewValue = 0m;
			e.Cancel = true;
		}

        protected virtual void APTran_CuryUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            APTran tran = (APTran)e.Row;

            if (APSetup.Current.RequireControlTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() && tran.Qty == 0)
            {
                e.NewValue = 0.0m;
                e.Cancel = true;
                return;
            }

            if (tran == null || tran.InventoryID == null || !string.IsNullOrEmpty(tran.PONbr))
            {
                e.NewValue = sender.GetValue<APTran.curyUnitCost>(e.Row);
                e.Cancel = e.NewValue != null;
                return;
            }

            APInvoice doc = this.Document.Current;

            if (doc != null && doc.VendorID != null && tran != null)
            {
                if (tran.ManualPrice != true || tran.CuryUnitCost == null)
                {
                    decimal? vendorUnitCost = null;

                    if (tran.InventoryID != null && tran.UOM != null)
                    {
                        DateTime date = Document.Current.DocDate.Value;

                        vendorUnitCost = APVendorPriceMaint.CalculateUnitCost(sender, tran.VendorID, doc.VendorLocationID, tran.InventoryID, tran.SiteID, currencyinfo.Select(), tran.UOM, tran.Qty, date, tran.CuryUnitCost);
                        e.NewValue = vendorUnitCost;
                    }

                    if (vendorUnitCost == null)
                    {
                        e.NewValue = POItemCostManager.Fetch<APTran.inventoryID, APTran.curyInfoID>(sender.Graph, tran,
                            doc.VendorID, doc.VendorLocationID, doc.DocDate, doc.CuryID, tran.InventoryID, null, null, tran.UOM);
                    }

                    APVendorPriceMaint.CheckNewUnitCost<APTran, APTran.curyUnitCost>(sender, tran, e.NewValue);
                }
                else
                    e.NewValue = tran.CuryUnitCost ?? 0m;
                e.Cancel = true;
            }
        }

		protected virtual void APTran_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null)
				sender.SetDefaultExt<APTran.curyUnitCost>(e.Row);
		}

		protected virtual void APTran_CuryLineAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APInvoice doc = Document.Current;
			APTran tran = e.Row as APTran;
			if (doc == null || tran == null) return;

			if (!IsImport && 
				APSetup.Current.RequireControlTotal == true && 
				PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() &&
				doc.RetainageApply != true)
			{
				decimal? newVal = 0;
				if (String.IsNullOrEmpty(tran.TaxCategoryID))
				{
					sender.SetDefaultExt<APTran.taxCategoryID>(tran);
				}

				newVal = TaxAttribute.CalcResidualAmt(sender, tran, doc.TaxZoneID, tran.TaxCategoryID, doc.DocDate.Value,
					doc.TaxCalcMode, doc.CuryOrigDocAmt.Value, doc.CuryLineTotal.Value, doc.CuryTaxTotal.Value);

				e.NewValue = newVal >= 0 ? newVal : 0;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_LCLineNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = (APTran)e.Row;
			if (APSetup.Current.RequireControlTotal != true || !PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() || tran.Qty != 0)
			{
				sender.SetDefaultExt<APTran.unitCost>(tran);
				sender.SetDefaultExt<APTran.curyUnitCost>(tran);
				sender.SetValue<APTran.unitCost>(tran, null);
			}
		}

		protected virtual void APTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = (APTran)e.Row;
			if (!string.IsNullOrEmpty(tran.PONbr) && tran.POAccrualType == POAccrualType.Order)
			{
				string oldUom = (string)e.OldValue;
				decimal qty = tran.Qty ?? 0m;
				decimal curyUnitCost = tran.CuryUnitCost ?? 0m;
				if (!string.IsNullOrEmpty(oldUom) && !string.IsNullOrEmpty(tran.UOM) && oldUom != tran.UOM)
				{
					if (qty != 0m)
					{
						qty = INUnitAttribute.ConvertToBase<APTran.inventoryID>(sender, e.Row, oldUom, qty, INPrecision.NOROUND);
						qty = INUnitAttribute.ConvertFromBase<APTran.inventoryID>(sender, e.Row, tran.UOM, qty, INPrecision.QUANTITY);
					}
					if (curyUnitCost != 0m)
					{
						curyUnitCost = INUnitAttribute.ConvertFromBase<APTran.inventoryID>(sender, e.Row, oldUom, curyUnitCost, INPrecision.NOROUND);
						curyUnitCost = INUnitAttribute.ConvertToBase<APTran.inventoryID>(sender, e.Row, tran.UOM, curyUnitCost, INPrecision.UNITCOST);
					}
					sender.SetValueExt<APTran.qty>(e.Row, qty);
					sender.SetValueExt<APTran.curyUnitCost>(e.Row, curyUnitCost);
				}
			}
			else if (APSetup.Current.RequireControlTotal != true || !PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() || tran.Qty != 0)
			{
				sender.SetDefaultExt<APTran.unitCost>(tran);
				sender.SetDefaultExt<APTran.curyUnitCost>(tran);
				sender.SetValue<APTran.unitCost>(tran, null);
			}
		}

		protected virtual void APTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null)
			{
				if (row.Qty == 0)
				{
					sender.SetValueExt<APTran.curyDiscAmt>(row, decimal.Zero);
					sender.SetValueExt<APTran.discPct>(row, decimal.Zero);
				}
				else
				{
					sender.SetDefaultExt<APTran.curyUnitCost>(e.Row);
				}
			}
		}

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PopupMessage]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>), PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		protected virtual void APTran_InventoryID_CacheAttached(PXCache sender)
        {
        }


        protected virtual void APTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = e.Row as APTran;
			if (tran != null)
			{
				if (String.IsNullOrEmpty(tran.ReceiptNbr) && string.IsNullOrEmpty(tran.PONbr))
				{
					sender.SetDefaultExt<APTran.accrueCost>(e.Row);
					sender.SetDefaultExt<APTran.accountID>(tran);
					sender.SetDefaultExt<APTran.subID>(tran);
					sender.SetDefaultExt<APTran.taxCategoryID>(tran);
					sender.SetDefaultExt<APTran.deferredCode>(tran);
					sender.SetDefaultExt<APTran.uOM>(tran);

					if (APSetup.Current.RequireControlTotal != true || !PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() || tran.Qty != 0)
					{
						if (e.ExternalCall && tran != null)
							tran.CuryUnitCost = 0m;
						sender.SetDefaultExt<APTran.unitCost>(tran);
						sender.SetDefaultExt<APTran.curyUnitCost>(tran);
						sender.SetValue<APTran.unitCost>(tran, null);
					}

					InventoryItem item = nonStockItem.Select(tran.InventoryID);

					if (item != null)
					{
						tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, "Descr", vendor.Current?.LocaleName);
					}
				}
			}
		}

		protected virtual void APTran_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			if (APSetup.Current.RequireSingleProjectPerDocument == true)
			{
				e.NewValue = Document.Current?.ProjectID;
				e.Cancel = true;
			}
		}
		
		protected virtual void APTran_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			if (APSetup.Current.ExpenseSubMask != null && APSetup.Current.ExpenseSubMask.Contains(APAcctSubDefault.MaskProject))
			{
				sender.SetDefaultExt<APTran.subID>(row);
			}

			foreach (APTran discTran in Discount_Row.Select())
			{
				SetProjectIDForDiscountTran(discTran);

				if (!PM.ProjectDefaultAttribute.IsNonProject( discTran.ProjectID))
				{
					try
					{
					SetTaskIDForDiscountTran(discTran);
				}
					catch (PXException ex)
					{
						PMProject project = (PMProject)PXSelectorAttribute.Select<APTran.projectID>(sender, row);
						sender.RaiseExceptionHandling<APTran.projectID>(row, project?.ContractCD, ex);
					}
				}
				else
				{
					discTran.TaskID = null;
				}
			}
		}

		protected virtual void APTran_TaskID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			if (APSetup.Current.ExpenseSubMask != null && APSetup.Current.ExpenseSubMask.Contains(APAcctSubDefault.MaskTask))
			{
				sender.SetDefaultExt<APTran.subID>(row);
		}
		}

		protected virtual void APTran_TaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;
			
			if (e.NewValue is Int32)
				CheckOrderTaskRule(sender, row, (int?)e.NewValue);
		}

		protected virtual void APTran_CuryLineAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			if (PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>() &&
				Document.Current?.PaymentsByLinesAllowed == true &&
				(decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(Messages.Entry_GE, 0m.ToString());
			}
		}

		protected virtual void APTran_DefScheduleID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, ((APTran)e.Row).DefScheduleID);
			if (sc != null)
			{
				APTran defertran = PXSelect<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>.Select(this, sc.DocType, sc.RefNbr, sc.LineNbr);

				if (defertran != null)
				{
					((APTran)e.Row).DeferredCode = defertran.DeferredCode;
				}
			}
		}

		protected virtual void APTran_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null && e.ExternalCall)
			{
				_discountEngine.UpdateManualLineDiscount(sender, Transactions, row, DiscountDetails, Document.Current.BranchID, Document.Current.VendorLocationID, Document.Current.DocDate, DiscountEngine.DefaultAPDiscountCalculationParameters);
			}
		}

		protected virtual void APTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			APInvoice doc = Document.Current;

			bool isPrebookedNotCompleted = (doc != null) && (doc.Prebooked == true && doc.Released == false && doc.Voided == false);
			bool isPOReceiptRelated = !string.IsNullOrEmpty(row.ReceiptNbr);
			bool isPOOrderRelated = !string.IsNullOrEmpty(row.PONbr);
			bool isPOOrderBasedBilling = isPOOrderRelated && (row.POAccrualType == POAccrualType.Order);
			bool isReverseTran = (row.OrigLineNbr != null);
			bool isLCBasedTranAP = false;
			bool is1099Enabled = vendor.Current?.Vendor1099 == true && doc != null && doc.Voided != true;
			bool isDocumentReleased = doc?.Released == true;
			bool isDocumentDebitAdjustment = doc.DocType == APDocType.DebitAdj;

			if (row.LCDocType != null && row.LCRefNbr != null)
			{
				isLCBasedTranAP = true;
			}

			bool isStockItem = false;

			// When migration mode is activated in AP module,
			// we should process stock items the same way as nonstock
			// items, because we should allow entering both types of
			// inventory items without any additional links to PO.
			// 
			if (APSetup.Current?.MigrationMode != true && row.InventoryID != null)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
				isStockItem = item?.StkItem == true;
			}

			if (isStockItem &&
				row.TranType != APDocType.Prepayment &&
				row.POAccrualType != POAccrualType.Order &&
				!isPOReceiptRelated &&
				doc.IsRetainageDocument != true)
			{
				cache.RaiseExceptionHandling<APTran.inventoryID>(row, row.InventoryID,
					new PXSetPropertyException(Messages.NoLinkedtoReceipt, PXErrorLevel.Warning));
			}

			if (!isLCBasedTranAP)
			{
				if (isPOOrderRelated || isPOReceiptRelated)
				{
					PXUIFieldAttribute.SetEnabled<APTran.inventoryID>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.uOM>(cache, row, isPOOrderBasedBilling && !isReverseTran);
					if (isPOOrderBasedBilling && isReverseTran && POLineType.UsePOAccrual(row.LineType))
					{
						// do not allow changing the original qty and amount because otherwise it won't be able to revert the PPV amount
						PXUIFieldAttribute.SetEnabled<APTran.qty>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.curyUnitCost>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.curyLineAmt>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.discPct>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.curyDiscAmt>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.manualDisc>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.discountID>(cache, row, false);
					}
				}

				bool allowEdit = (doc != null) && (doc.Prebooked == false && doc.Released == false && doc.Voided == false);
				PXUIFieldAttribute.SetEnabled<APTran.defScheduleID>(cache, row, allowEdit && row.TranType == APDocType.DebitAdj);
				PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(cache, row, allowEdit && row.DefScheduleID == null);

				if (doc.Released == false && doc.Prebooked == false)
				{
					bool currencyChanged = false;

					if (!string.IsNullOrEmpty(row.PONbr))
					{
						POOrder sourceDoc = PXSelect<POOrder,
							Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
												And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, row.POOrderType, row.PONbr);
						if (!string.IsNullOrEmpty(sourceDoc?.OrderNbr))
						{
							currencyChanged = (doc.CuryID != sourceDoc.CuryID);
						}
					}
					if (currencyChanged)
					{
						cache.RaiseExceptionHandling<APTran.curyLineAmt>(row, row.CuryLineAmt,
						new PXSetPropertyException(Messages.APDocumentCurrencyDiffersFromSourceDocument, PXErrorLevel.Warning));
						cache.RaiseExceptionHandling<APTran.curyUnitCost>(row, row.CuryUnitCost,
						new PXSetPropertyException(Messages.APDocumentCurrencyDiffersFromSourceDocument, PXErrorLevel.Warning));
					}
				}

				if (isPOOrderRelated || isPOReceiptRelated)
				{
					bool isPOPrepayment = (row.TranType == APDocType.Prepayment);
					bool allowChangingAccount = !isDocumentReleased && !isPrebookedNotCompleted
						&& (!POLineType.UsePOAccrual(row.LineType) || isPOPrepayment);
					PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, allowChangingAccount);
					PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, allowChangingAccount);
					PXUIFieldAttribute.SetEnabled<APTran.branchID>(cache, row, allowChangingAccount);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, !isStockItem && (!isDocumentReleased || !is1099Enabled));
					PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, !isStockItem && (!isDocumentReleased || !is1099Enabled));
				}
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<APTran.qty>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.uOM>(cache, row, false);

				if (isDocumentDebitAdjustment && row.OrigLineNbr.HasValue)
				{
					PXUIFieldAttribute.SetEnabled<APTran.curyUnitCost>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.curyLineAmt>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.discPct>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.curyDiscAmt>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.manualDisc>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.taxCategoryID>(cache, row, false);
				}
				//PXUIFieldAttribute.SetEnabled(cache, row, false);
			}

			bool isProjectEditable = !isPOOrderRelated;

				InventoryItem ns = (InventoryItem)PXSelectorAttribute.Select(cache, e.Row, cache.GetField(typeof (APTran.inventoryID)));
			if (ns != null && ns.StkItem != true && ns.NonStockReceipt != true)
			{
				isProjectEditable = true;
			}

			isProjectEditable = isProjectEditable && (!isDocumentReleased || !is1099Enabled);

			PXUIFieldAttribute.SetEnabled<APTran.projectID>(cache, row, isProjectEditable && APSetup.Current.RequireSingleProjectPerDocument != true);
			PXUIFieldAttribute.SetEnabled<APTran.taskID>(cache, row, isProjectEditable);

			PXUIFieldAttribute.SetEnabled<APTran.lCDocType>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<APTran.lCRefNbr>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<APTran.lCLineNbr>(cache, row, false);

			#region Migration Mode Settings

			if (doc != null &&
				doc.IsMigratedRecord == true &&
				doc.Released != true)
			{
				PXUIFieldAttribute.SetEnabled<APTran.defScheduleID>(Transactions.Cache, null, false);
				PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(Transactions.Cache, null, false);
			}

			#endregion
		}

		protected virtual void APTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APTran tran = e.Row as APTran;
			if (tran == null) return;

            if (tran.LineType == SOLineType.Discount)
            {
                if (tran.AccountID == null)
                {
                    throw new PXException(Messages.DiscountAccountNoSpecified);
                }

                if (tran.SubID == null)
                {
                    throw new PXException(Messages.DiscountSubaccountNoSpecified);
                }
            }

			APInvoice doc = Document.Current;

			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, tran.InventoryID);

			bool disablePersistingCheckForAccountAndSub = item?.StkItem == true && APSetup.Current?.MigrationMode != true;
			PXDefaultAttribute.SetPersistingCheck<APTran.accountID>(sender, tran, disablePersistingCheckForAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<APTran.subID>(sender, tran, disablePersistingCheckForAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				CheckOrderTaskRule(sender, tran, tran.TaskID);
				CheckProjectAccountRule(sender, tran);

				if (tran.Qty != 0 && string.IsNullOrEmpty(tran.UOM) && tran.TaskID != null && tran.InventoryID != null)
				{
					if (!sender.RaiseExceptionHandling(null, tran, null,
						new PXSetPropertyException<APTran.uOM>(Messages.NotZeroQtyRequireUOM, PXErrorLevel.Error)))
					{
						throw new PXSetPropertyException<APTran.uOM>(Messages.NotZeroQtyRequireUOM);
					}
				}

				if (tran.Released != true) CheckQtyFromPO(sender, tran);
			}

			ScheduleHelper.DeleteAssociatedScheduleIfDeferralCodeChanged(this, tran);

			if (doc != null
				&& !doc.IsChildRetainageDocument()
				&& APSetup.Current.RequireSingleProjectPerDocument == true 
				&& tran.ProjectID != doc.ProjectID
				&& doc.ProjectID != ProjectDefaultAttribute.NonProject())
			{
				tran.ProjectID = doc.ProjectID;
			}

			object projectID = tran.ProjectID;
			try
			{
				sender.RaiseFieldVerifying<APTran.projectID>(tran, ref projectID);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling<APTran.projectID>(tran, projectID, ex);
			}

			if (tran.LineType == SOLineType.Discount &&
				tran.TaskID == null && 
				!ProjectDefaultAttribute.IsNonProject(tran.ProjectID))
			{
				SetTaskIDForDiscountTran(tran);
			}
		}

		protected virtual void APTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row == null) return;
			TaxAttribute.Calculate<APTran.taxCategoryID>(sender, e);
		}

		protected virtual void APTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null)
			{
				//Validate that Expense Account <> Deferral Account:
				if (!sender.ObjectsEqual<APTran.accountID, APTran.deferredCode>(e.Row, e.OldRow))
				{
					if (!string.IsNullOrEmpty(row.DeferredCode))
					{
						DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, row.DeferredCode);
						if (defCode != null)
						{
							if (defCode.AccountID == row.AccountID)
							{
								sender.RaiseExceptionHandling<APTran.accountID>(e.Row, row.AccountID,
									new PXSetPropertyException(Messages.AccountIsSameAsDeferred, PXErrorLevel.Warning));
							}
						}
					}
				}
			}

			if(!sender.ObjectsEqual<APTran.lCDocType, APTran.lCRefNbr, APTran.lCLineNbr>(e.Row, e.OldRow))
			{
				LandedCostDetailClearLink(e.OldRow as APTran);
				LandedCostDetailSetLink(e.Row as APTran);
			}

			if ((!sender.ObjectsEqual<APTran.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.inventoryID>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<APTran.baseQty>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyUnitCost>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyTranAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<APTran.curyLineAmt>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyDiscAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<APTran.discPct>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.manualDisc>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<APTran.discountID>(e.Row, e.OldRow) || changedSuppliedByVendorLocation) 
					&& row.LineType != APLineType.Discount && row.LineType != APLineType.LandedCostAP)
				RecalculateDiscounts(sender, row);

			if ((e.ExternalCall || sender.Graph.IsImport)
				&& sender.ObjectsEqual<APTran.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<APTran.uOM>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<APTran.qty>(e.Row, e.OldRow) && sender.ObjectsEqual<APTran.branchID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<APTran.siteID>(e.Row, e.OldRow) && sender.ObjectsEqual<APTran.manualPrice>(e.Row, e.OldRow)
				&& (!sender.ObjectsEqual<APTran.curyUnitCost>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyLineAmt>(e.Row, e.OldRow)))
				row.ManualPrice = true;

			if (Document.Current.Released != true && Document.Current.Prebooked != true)
			{
				TaxAttribute.Calculate<APTran.taxCategoryID>(sender, e);
			}

			if (!sender.ObjectsEqual<APTran.box1099>(e.Row, e.OldRow))
			{
				foreach (APAdjust adj in PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>, And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>, And<APAdjust.released, Equal<True>>>>>.Select(this))
				{
					APReleaseProcess.Update1099Hist(this, -1m, adj, (APTran)e.OldRow, Document.Current);
					APReleaseProcess.Update1099Hist(this, 1m, adj, (APTran)e.Row, Document.Current);
				}
			}
		}

		protected virtual void APTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			APTran row = (APTran)e.Row;

			if (!String.IsNullOrEmpty(row.LCRefNbr))
			{
				LandedCostDetailClearLink(row);
			}

            if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted)
			{
				if (row.LineType != APLineType.Discount && row.LineType != APLineType.LandedCostAP)
				{
					_discountEngine.RecalculateGroupAndDocumentDiscounts(sender, Transactions, null, DiscountDetails, Document.Current.BranchID, Document.Current.VendorLocationID, Document.Current.DocDate, DiscountEngine.DefaultAPDiscountCalculationParameters);
				}
				RecalculateTotalDiscount();
			}
		}

		protected virtual void APTran_Box1099_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (vendor.Current == null || vendor.Current.Vendor1099 == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			object existing;
			if ((existing = sender.Locate(e.Row)) != null && sender.GetStatus(existing) == PXEntryStatus.Deleted)
			{
				sender.SetValue<APTran.tranID>(e.Row, sender.GetValue<APTran.tranID>(existing));
			}
		}

		protected virtual void AP1099Hist_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (((AP1099Hist)e.Row).BoxNbr == null)
			{
				e.Cancel = true;
			}
		}

		#endregion

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
				e.NewValue = ((APInvoice)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}
		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (vendor.Current != null && !(bool)vendor.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CurrencyInfo currencyInfo = e.Row as CurrencyInfo;

			if (currencyInfo?.CuryRate == null) return;

			if (Document.Current?.ReleasedOrPrebooked != true)
			{
				RecalculateApplicationsAmounts();
			}
		}

		#endregion

		#region APTaxTran Events
		protected virtual void APTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void APTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is APTaxTran apTaxTran))
				return;

			bool usesManualVAT = Document.Current?.UsesManualVAT == true;
			PXUIFieldAttribute.SetEnabled<APTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted && !usesManualVAT);
		}

		protected virtual void APTax_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APTax aptax = e.Row as APTax;
			if (aptax == null) return;

			if (Document.Current?.PaymentsByLinesAllowed == true)
			{
				Tax tax = (Tax)PXSelectorAttribute.Select<APTax.taxID>(sender, aptax);
				if (tax?.TaxType == CSTaxType.Withholding)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void APTaxTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APTaxTran aptaxtran = e.Row as APTaxTran;
			if (aptaxtran == null) return;

			PXParentAttribute.SetParent(sender, e.Row, typeof(APRegister), this.Document.Current);

			if (Document.Current?.PaymentsByLinesAllowed == true)
			{
				Tax tax = (Tax)PXSelectorAttribute.Select<APTaxTran.taxID>(sender, aptaxtran);
				if (tax?.TaxType == CSTaxType.Withholding)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void APTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				((APTaxTran)e.Row).TaxZoneID = Document.Current.TaxZoneID;
			}
		}

		protected virtual void APTaxTran_TaxZoneID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Exception is PXSetPropertyException)
			{
				Document.Cache.RaiseExceptionHandling<APInvoice.taxZoneID>(Document.Current, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
				e.Cancel = false;
			}
		}

		#endregion     

		#region APAdjust Events

		protected virtual void APAdjust_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<APInvoice.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.CuryDocBal == null)
			{
				PXResult<APPayment, CurrencyInfo> res = (PXResult<APPayment, CurrencyInfo>)PXSelectJoin<APPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>, Where<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);

				APPayment payment = res;
				CurrencyInfo pay_info = res;
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this);

				decimal CuryDocBal = BalanceCalculation.CalculateApplicationDocumentBalance(
					sender, 
					pay_info, 
					inv_info, 
					payment.DocBal, 
					payment.CuryDocBal);
				adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

		protected virtual void APAdjust_CuryAdjdAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust application = e.Row as APAdjust;

			if (application == null) return;

			RecalculateApplicationsAmounts();

			application.CuryDocBal = application.CuryDocBal + (decimal?)e.OldValue - application.CuryAdjdAmt;
		}

		protected virtual void APAdjust_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}
		protected virtual void APInvoice_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;
			if (PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
			{
				if (this.Approval.GetAssignedMaps(doc, sender).Any())
				{
					sender.SetValue<APInvoice.hold>(doc, true);
				}
			}
		}
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<
			Case<Where<APAdjust.adjType, Equal<ARAdjust.adjType.adjusted>>, APAdjust.adjdDocType,
			Case<Where<APAdjust.adjType, Equal<ARAdjust.adjType.adjusting>>, APAdjust.adjgDocType>>>))]
		protected virtual void APAdjust_DisplayDocType_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<
			Case<Where<APAdjust.adjType, Equal<ARAdjust.adjType.adjusted>>, APAdjust.adjdRefNbr,
			Case<Where<APAdjust.adjType, Equal<ARAdjust.adjType.adjusting>>, APAdjust.adjgRefNbr>>>))]
		protected virtual void APAdjust_DisplayRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<APAdjust.displayRefNbr, Standalone.APRegister.docDate>))]
		protected virtual void APAdjust_DisplayDocDate_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<APAdjust.displayRefNbr, Standalone.APRegister.docDesc>))]
		protected virtual void APAdjust_DisplayDocDesc_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<APAdjust.displayRefNbr, Standalone.APRegister.curyID>))]
		protected virtual void APAdjust_DisplayCuryID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(FormatPeriodID<FormatDirection.display, Selector<APAdjust.displayRefNbr, Standalone.APRegister.finPeriodID>>))]
		protected virtual void APAdjust_DisplayFinPeriodID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<APAdjust.displayRefNbr, Standalone.APRegister.status>))]
		protected virtual void APAdjust_DisplayStatus_CacheAttached(PXCache sender) { }

		#endregion

		#region APInvoiceDiscountDetail events

		protected virtual void APInvoiceDiscountDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (Document != null && Document.Current != null)
				Document.Cache.SetValueExt<APInvoice.curyDocDisc>(Document.Current, _discountEngine.GetTotalGroupAndDocumentDiscount(DiscountDetails, true));
		}

		protected virtual void APInvoiceDiscountDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>() &&
				Document.Current?.PaymentsByLinesAllowed == true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APInvoiceDiscountDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			APInvoiceDiscountDetail discountDetail = (APInvoiceDiscountDetail)e.Row;
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (discountDetail.DiscountID != null)
			{
					_discountEngine.InsertManualDocGroupDiscount(Transactions.Cache, Transactions,
						DiscountDetails, discountDetail, discountDetail.DiscountID, null, Document.Current.BranchID,
						Document.Current.VendorLocationID, Document.Current.DocDate,
						DiscountEngine.DefaultAPDiscountCalculationParameters);
					RecalculateTotalDiscount();
				}

				if (_discountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, null, DiscountEngine.DefaultAPDiscountCalculationParameters))
				RecalculateTotalDiscount();
			}
		}

		protected virtual void APInvoiceDiscountDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APInvoiceDiscountDetail discountDetail = (APInvoiceDiscountDetail)e.Row;
			APInvoiceDiscountDetail oldDiscountDetail = (APInvoiceDiscountDetail) e.OldRow;
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (!sender.ObjectsEqual<APInvoiceDiscountDetail.skipDiscount>(e.Row, e.OldRow))
				{
					_discountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, DiscountDetails, Document.Current.BranchID, Document.Current.VendorLocationID, Document.Current.DocDate, discountDetail.Type != DiscountType.Document, DiscountEngine.DefaultAPDiscountCalculationParameters);
				RecalculateTotalDiscount();
			}

				if (!sender.ObjectsEqual<APInvoiceDiscountDetail.discountID>(e.Row, e.OldRow) || !sender.ObjectsEqual<APInvoiceDiscountDetail.discountSequenceID>(e.Row, e.OldRow))
			{
					_discountEngine.UpdateManualDocGroupDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, discountDetail.DiscountID, sender.ObjectsEqual<APInvoiceDiscountDetail.discountID>(e.Row, e.OldRow) ? discountDetail.DiscountSequenceID : null, Document.Current.BranchID, Document.Current.VendorLocationID, Document.Current.DocDate, DiscountEngine.DefaultAPDiscountCalculationParameters);
				RecalculateTotalDiscount();
			}

				if (_discountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, oldDiscountDetail, DiscountEngine.DefaultAPDiscountCalculationParameters))
					RecalculateTotalDiscount();
		}
		}

		protected virtual void APInvoiceDiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			APInvoiceDiscountDetail discountDetail = (APInvoiceDiscountDetail)e.Row;
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				_discountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, DiscountDetails, Document.Current.BranchID, Document.Current.VendorLocationID, Document.Current.DocDate, (discountDetail.Type != null && discountDetail.Type != DiscountType.Document && discountDetail.Type != DiscountType.ExternalDocument), DiscountEngine.DefaultAPDiscountCalculationParameters);
			}
			RecalculateTotalDiscount();
		}
		
		protected virtual void APInvoiceDiscountDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APInvoiceDiscountDetail discountDetail = (APInvoiceDiscountDetail)e.Row;

			bool isExternalDiscount = discountDetail.Type == DiscountType.ExternalDocument;

			PXDefaultAttribute.SetPersistingCheck<APInvoiceDiscountDetail.discountID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<APInvoiceDiscountDetail.discountSequenceID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}
		
		#endregion

		protected virtual void APInvoiceDiscountDetail_DiscountSequenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}
		protected virtual void APInvoiceDiscountDetail_DiscountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}
		
    	#region Source-Specific  functions

		public virtual void InvoicePOReceipt(POReceipt receipt, DocumentList<APInvoice> list,
			bool saveAndAdd = false,
			bool usePOParemeters = true,
			bool keepOrderTaxes = false,
			bool errorIfUnreleasedAPExists = true)
		{
			if (errorIfUnreleasedAPExists)
			{
				var unreleasedAPTrans = PXSelectJoinGroupBy<POReceiptLine,
					InnerJoin<APTran,
						On<APTran.pOAccrualType, Equal<POReceiptLine.pOAccrualType>,
							And<APTran.pOAccrualRefNoteID, Equal<POReceiptLine.pOAccrualRefNoteID>,
							And<APTran.pOAccrualLineNbr, Equal<POReceiptLine.pOAccrualLineNbr>>>>>,
					Where<POReceiptLine.receiptNbr, Equal<Required<APTran.receiptNbr>>,
						And<APTran.released, Equal<False>>>,
					Aggregate<
						GroupBy<APTran.tranType,
						GroupBy<APTran.refNbr,
						GroupBy<APTran.pOAccrualType>>>>>
					.Select(this, receipt.ReceiptNbr)
					.RowCast<APTran>().ToList();
				if (unreleasedAPTrans.Any())
				{
					bool receiptBased = unreleasedAPTrans.Any(t => t.POAccrualType == POAccrualType.Receipt);
					throw receiptBased
						? new PXException(PO.Messages.UnreasedAPDocumentExistsForPOReceipt)
						: new PXException(PO.Messages.UnreleasedAPDocumentExistsForPOLines, unreleasedAPTrans.First().RefNbr);
				}
			}
				
			APInvoice newdoc;

			bool directReceipt = false;
			string taxZoneID = null;
			string taxCalcMode = null;
			int? payToVendorPO = null;
			string curyID = null;
			string termsID = null;
			string description = null;

			var receiptLinks = PXSelectGroupBy<POOrderReceiptLink,
				Where<POOrderReceiptLink.receiptNbr, Equal<Required<POReceipt.receiptNbr>>>, 
				Aggregate<
					GroupBy<POOrderReceiptLink.taxZoneID,
					GroupBy<POOrderReceiptLink.taxCalcMode,
					GroupBy<POOrderReceiptLink.payToVendorID,
					GroupBy<POOrderReceiptLink.curyID,
					GroupBy<POOrderReceiptLink.termsID>>>>>>>
				.Select(this, receipt.ReceiptNbr)
				.RowCast<POOrderReceiptLink>().ToList();

			if (receiptLinks.Any())
			{
				POOrderReceiptLink firstOrderReceipt = receiptLinks.First();
				if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
				{
					if (receiptLinks.Any(x => x.PayToVendorID != firstOrderReceipt.PayToVendorID))
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.POReceiptContainsSeveralPayToVendors, receipt.ReceiptNbr));
					else
						payToVendorPO = firstOrderReceipt.PayToVendorID;
				}
				else
				{
					payToVendorPO = receipt.VendorID;
				}

				if (usePOParemeters)
				{
					if (receiptLinks.Any(x => x.TaxZoneID != firstOrderReceipt.TaxZoneID) && list != null)
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.POReceiptContainsSeveralTaxZones, receipt.ReceiptNbr));
					else
						taxZoneID = firstOrderReceipt.TaxZoneID;

					if (list == null || !receiptLinks.Any(x => x.TaxCalcMode != firstOrderReceipt.TaxCalcMode))
						taxCalcMode = firstOrderReceipt.TaxCalcMode;

					if (receiptLinks.Any(x => x.CuryID != firstOrderReceipt.CuryID))
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.POReceiptContainsSeveralCurrencies, receipt.ReceiptNbr));
					else
						curyID = firstOrderReceipt.CuryID;

					if (receiptLinks.Count == 1 && receipt.ReceiptType != POReceiptType.POReturn)
						termsID = firstOrderReceipt.TermsID;

					if (receiptLinks.Count == 1)
						description = POOrder.PK.Find(this, firstOrderReceipt.POType, firstOrderReceipt.PONbr)?.OrderDesc;
				}
			}
			else
			{
				directReceipt = true;
				payToVendorPO = receipt.VendorID;
			}

			bool setControlTotal = false;

			if (list != null)
			{
				newdoc = list.Find<APInvoice.docType, APInvoice.branchID, APInvoice.vendorID,
						APInvoice.vendorLocationID, APInvoice.curyID, APInvoice.invoiceDate>((
							(POReceipt)receipt).GetAPDocType(), ((POReceipt)receipt).BranchID, ((POReceipt)receipt).VendorID,
								((POReceipt)receipt).VendorLocationID, ((POReceipt)receipt).CuryID, ((POReceipt)receipt).ReceiptDate)
					?? new APInvoice();

				if (newdoc.RefNbr != null)
				{
					Document.Current = Document.Search<APInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					newdoc.DocType = receipt.GetAPDocType();
					newdoc.DocDate = receipt.InvoiceDate;
					newdoc.BranchID = receipt.BranchID;

					if (DateTime.Compare((DateTime)receipt.ReceiptDate, (DateTime)receipt.InvoiceDate) == 0)
					{
						newdoc.FinPeriodID = receipt.FinPeriodID;
					}					

					if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
					{
						newdoc.VendorID = payToVendorPO;
						newdoc.VendorLocationID = receipt.VendorID == payToVendorPO ? receipt.VendorLocationID : null;
						newdoc.SuppliedByVendorID = receipt.VendorID;
						newdoc.SuppliedByVendorLocationID = receipt.VendorLocationID;
					}
					else
					{
						newdoc.VendorID =
						newdoc.SuppliedByVendorID = receipt.VendorID;
						newdoc.VendorLocationID =
						newdoc.SuppliedByVendorLocationID = receipt.VendorLocationID;
					}

					var cancel_defaulting = new PXFieldDefaulting((cache, e) => { e.NewValue = cache.GetValue<APInvoice.branchID>(e.Row); e.Cancel = true; });
					this.FieldDefaulting.AddHandler<APInvoice.branchID>(cancel_defaulting);

					try
					{
						newdoc = PXCache<APInvoice>.CreateCopy(Document.Insert(newdoc));
					}
					finally
					{
						this.FieldDefaulting.RemoveHandler<APInvoice.branchID>(cancel_defaulting);
					}

					if (receipt.AutoCreateInvoice == true)
					{
						newdoc.RefNoteID = receipt.NoteID;
					}

					newdoc.ProjectID = receipt.ProjectID;
					newdoc.InvoiceNbr = receipt.InvoiceNbr;

					if (termsID != null)
					{
						newdoc.TermsID = termsID;
					}

					if (taxZoneID != null)
					{
						newdoc.TaxZoneID = taxZoneID;
					}

					//needed for correct rate calculation
					string defaultedCuryID = newdoc.CuryID;
					string baseCuryID = currencyinfo.Current?.BaseCuryID ?? ((Company)PXSelect<Company>.Select(this)).BaseCuryID;
					Document.Cache.SetValueExt<APInvoice.curyID>(newdoc, baseCuryID);

					newdoc = Document.Update(newdoc);

					if (taxCalcMode != null)
						newdoc.TaxCalcMode = taxCalcMode;

					newdoc.CuryID = curyID != null ? curyID : defaultedCuryID;
					newdoc = Document.Update(newdoc);

					setControlTotal = true;
				}
			}
			else
			{
				newdoc = PXCache<APInvoice>.CreateCopy(Document.Current);
				if (newdoc.VendorID == null && newdoc.VendorLocationID == null)
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
					{
						newdoc.VendorID = payToVendorPO;
						newdoc.VendorLocationID = receipt.VendorID == payToVendorPO ? receipt.VendorLocationID : null;
						newdoc.SuppliedByVendorID = receipt.VendorID;
						newdoc.SuppliedByVendorLocationID = receipt.VendorLocationID;
					}
					else
					{
						newdoc.VendorID =
						newdoc.SuppliedByVendorID = receipt.VendorID;
						newdoc.VendorLocationID =
						newdoc.SuppliedByVendorLocationID = receipt.VendorLocationID;
					}

					newdoc.DocDate = receipt.InvoiceDate;

					if (string.IsNullOrEmpty(newdoc.InvoiceNbr))
					{
						newdoc.InvoiceNbr = receipt.InvoiceNbr;
					}

					if (string.IsNullOrEmpty(newdoc.TermsID))
					{
						newdoc.TermsID = termsID;
					}

					if (taxZoneID != null)
					{
						newdoc.TaxZoneID = taxZoneID;
					}

					if (curyID != null)
					{
						newdoc.CuryID = curyID;
					}

					newdoc = Document.Update(newdoc);

					if (taxCalcMode != null)
						newdoc.TaxCalcMode = taxCalcMode;

					newdoc = Document.Update(newdoc);
				}
				else if (newdoc.VendorID != payToVendorPO)
				{
					bool wrongVendor = true;
					if (directReceipt && PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
					{
						Vendor receiptVendor = vendor.Select(payToVendorPO);
						wrongVendor = (newdoc.VendorID != receiptVendor?.PayToVendorID);
					}
					if (wrongVendor)
				{
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.POReceiptBelongsAnotherVendor, receipt.ReceiptNbr));
					}
				}
			}
			newdoc = Document.Current;
			if (string.IsNullOrEmpty(newdoc.InvoiceNbr))
			{
				newdoc.InvoiceNbr = receipt.InvoiceNbr;
			}
			if (string.IsNullOrEmpty(newdoc.DocDesc) && !string.IsNullOrEmpty(description))
			{
				newdoc.DocDesc = description;
			}

			POAccrualSet duplicates = GetUsedPOAccrualSet();

			HashSet<string> ordersWithDiscounts = new HashSet<string>();
			foreach (POReceiptLineS orderline in
				PXSelectReadonly2<POReceiptLineS,
					LeftJoin<APTran, On<APTran.released, Equal<False>,
						And<APTran.pOAccrualType, Equal<POReceiptLineS.pOAccrualType>,
						And<APTran.pOAccrualRefNoteID, Equal<POReceiptLineS.pOAccrualRefNoteID>,
						And<APTran.pOAccrualLineNbr, Equal<POReceiptLineS.pOAccrualLineNbr>>>>>>,
					Where<POReceiptLineS.receiptNbr, Equal<Required<POReceiptLineS.receiptNbr>>,
						And2<Where<APTran.refNbr, IsNull, Or<APTran.refNbr, Equal<Required<APTran.refNbr>>, And<APTran.tranType, Equal<Required<APTran.tranType>>>>>,
						And<POReceiptLineS.unbilledQty, Greater<decimal0>>>>,
					OrderBy<Asc<POReceiptLineS.sortOrder>>>
				.Select(this, receipt.ReceiptNbr, newdoc.RefNbr, newdoc.DocType))
			{
				if ((orderline.RetainagePct ?? 0m) != 0m)
				{
					EnableRetainage();
				}

				if (keepOrderTaxes)
				{
					bool directReceiptLine = string.IsNullOrEmpty(orderline.POType) || string.IsNullOrEmpty(orderline.PONbr);
					bool keepOrderTaxesForLine = keepOrderTaxes && !directReceiptLine;
					TaxAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(this.Transactions.Cache, null,
						keepOrderTaxesForLine ? TaxCalc.ManualCalc : TaxCalc.ManualLineCalc);
				}
				PXRowInserting handler = (sender, e) =>
				{ 
					PXParentAttribute.SetParent(sender, e.Row, typeof(APRegister), Document.Current);
				};

				RowInserting.AddHandler<APTran>(handler);

				APTran tran = AddPOReceiptLine(orderline, duplicates);

				if (this.Document.Current != null && orderline.DocumentDiscountRate != null && (orderline.GroupDiscountRate != 1 || orderline.DocumentDiscountRate != 1))
				{
					this.Document.Current.SetWarningOnDiscount = true;
					ordersWithDiscounts.Add(orderline.PONbr);
				}

				RowInserting.RemoveHandler<APTran>(handler);
			}

			TaxAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc);
			AutoRecalculateDiscounts();

			if (keepOrderTaxes)
			{
				//This is required to transfer taxes from the original document(possibly modified there) instead of counting them by default rules
				TaxAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(this.Transactions.Cache, null, TaxCalc.ManualCalc);
				AddOrderTaxes(receipt);
			}

			if (setControlTotal)
			{
				newdoc.CuryOrigDocAmt = newdoc.CuryDocBal;
				Document.Update(newdoc);
			}

			if (list != null && saveAndAdd)
			{
				Save.Press();
				if (list.Find(Document.Current) == null)
				{
					list.Add(Document.Current);
				}
			}

			WritePODiscountWarningToTrace(this.Document.Current, ordersWithDiscounts);

			TaxAttribute.SetTaxCalc<APTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc);
		}

		public virtual void AddOrderTaxes(POOrder order)
		{
			if (Document.Current == null || IsExternalTax(Document.Current.TaxZoneID))
				return;

			AddOrderTaxLines(order.OrderType, order.OrderNbr);
		}

		public virtual void AddOrderTaxes(POReceipt receipt)
		{
			if (Document.Current == null || IsExternalTax(Document.Current.TaxZoneID))
				return;

			foreach (POOrderReceiptLink receiptLink in PXSelectGroupBy<POOrderReceiptLink,
				Where<POOrderReceiptLink.receiptNbr, Equal<Required<POReceipt.receiptNbr>>>,
				Aggregate<
					GroupBy<POOrderReceiptLink.pOType,
					GroupBy<POOrderReceiptLink.pONbr,
					GroupBy<POOrderReceiptLink.taxZoneID>>>>>
				.Select(this, receipt.ReceiptNbr))
			{
				// scope of the taxes recalculation is limited with the current POOrderReceiptLink
				// necessary to set proper current because the method APTaxAttribute.FilterParent depends on it
				PXCache orderReceiptCache = this.Caches[typeof(POOrderReceiptLink)];
				orderReceiptCache.Current = receiptLink;

				AddOrderTaxLines(receiptLink.POType, receiptLink.PONbr);
			}
		}

		public virtual void AddOrderTaxLines(string orderType, string orderNbr)
		{
			foreach (var tax in
				PXSelectJoin<POTaxTran,
				InnerJoin<Tax, On<POTaxTran.taxID, Equal<Tax.taxID>>>,
				Where<POTaxTran.orderType, Equal<Required<POTaxTran.orderType>>,
					And<POTaxTran.orderNbr, Equal<Required<POTaxTran.orderNbr>>>>>
				.Select(this, orderType, orderNbr).AsEnumerable()
				.Select(r => new
				{
					POTax = PXResult.Unwrap<POTaxTran>(r),
					Tax = PXResult.Unwrap<Tax>(r)
				})
				.OrderBy(r => r.Tax, TaxByCalculationLevelComparer.Instance)) // Order by value, not by label (BQL sorts such fields by corresponding labels)
			{
				APTaxTran newtax = new APTaxTran();
				newtax.Module = BatchModule.AP;
				Taxes.Cache.SetDefaultExt<APTaxTran.origTranType>(newtax);
				Taxes.Cache.SetDefaultExt<APTaxTran.origRefNbr>(newtax);
				Taxes.Cache.SetDefaultExt<APTaxTran.lineRefNbr>(newtax);
				newtax.TranType = Document.Current.DocType;
				newtax.RefNbr = Document.Current.RefNbr;
				newtax.TaxID = tax.POTax.TaxID;
				newtax.TaxRate = 0m;

				foreach (APTaxTran existingTaxTran in this.Taxes.Cache.Cached.RowCast<APTaxTran>().Where(a =>
					!this.Taxes.Cache.GetStatus(a).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)
					&& this.Taxes.Cache.ObjectsEqual<APTaxTran.module, APTaxTran.refNbr, APTaxTran.tranType, APTaxTran.taxID>(newtax, a)))
				{
					this.Taxes.Delete(existingTaxTran);
				}

				newtax = this.Taxes.Insert(newtax);
			}
		}

        public virtual bool EnableRetainage()
		{
			if (Document.Current.RetainageApply == true)
				return false;

			Document.Current.RetainageApply = true;
			Document.Cache.SetDefaultExt<APInvoice.defRetainagePct>(Document.Current);
			Document.Cache.RaiseExceptionHandling<APInvoice.retainageApply>(Document.Current, true,
				new PXSetPropertyException(PO.Messages.AutoAppliedRetainageFromOrder, PXErrorLevel.Warning));

			return true;
		}

		public virtual void AttachPrepayment()
		{
			AttachPrepayment(null);
		}

		public virtual void AttachPrepayment(List<POOrder> orders)
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this);
			if (Document.Current != null && (Document.Current.DocType == APDocType.Invoice || Document.Current.DocType == APDocType.CreditAdj) && Document.Current.Released == false && Document.Current.Prebooked == false)
			{
				var lazyTrans = new Lazy<APTran[]>(() => Transactions.SelectMain());
				using (ReadOnlyScope rs = new ReadOnlyScope(Adjustments.Cache, Document.Cache))
				{
					decimal? invoiceAmtToPay = Document.Current.CuryDocBal;
					foreach (PXResult<APPayment, CurrencyInfo, APAdjust> res in
						PXSelectJoin<APPayment,
						InnerJoin<CurrencyInfo,
							On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>,
						LeftJoin<APAdjust,
							On<APAdjust.adjgDocType, Equal<APPayment.docType>,
							And<APAdjust.adjgRefNbr, Equal<APPayment.refNbr>,
							And<APAdjust.released, NotEqual<True>,
							And<Where<
								APAdjust.adjdDocType, NotEqual<Current<APInvoice.docType>>,
								Or<APAdjust.adjdRefNbr, NotEqual<Current<APInvoice.refNbr>>>>>>>>>>,
						Where<
							APPayment.vendorID, Equal<Current<APInvoice.vendorID>>,
							And<APPayment.docType, Equal<APDocType.prepayment>,
							And<APPayment.docDate, LessEqual<Current<APInvoice.docDate>>,
							And<APPayment.tranPeriodID, LessEqual<Current<APInvoice.tranPeriodID>>,
							And<APPayment.released, Equal<True>,
							And<APPayment.openDoc, Equal<True>,
							And<APPayment.curyDocBal, Greater<decimal0>,
							And<APAdjust.adjdRefNbr, IsNull>>>>>>>>>.Select(this))
					{
						APPayment payment = (APPayment)res;
						CurrencyInfo pay_info = (CurrencyInfo)res;

						foreach (PXResult<POOrderPrepayment, CurrencyInfo> orderPrepayRes in
							PXSelectJoin<POOrderPrepayment,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<POOrderPrepayment.curyInfoID>>>,
							Where<POOrderPrepayment.aPDocType, Equal<Required<APPayment.docType>>,
								And<POOrderPrepayment.aPRefNbr, Equal<Required<APPayment.refNbr>>>>>
							.Select(this, payment.DocType, payment.RefNbr))
						{
							POOrderPrepayment orderPrepay = orderPrepayRes;
							CurrencyInfo orderInfo = orderPrepayRes;

							if ((orders == null || orders.Any(o => o.OrderType == orderPrepay.OrderType && o.OrderNbr == orderPrepay.OrderNbr))
								&& lazyTrans.Value.Any(t => t.POOrderType == orderPrepay.OrderType && t.PONbr == orderPrepay.OrderNbr))
							{
								APAdjust adj = new APAdjust
								{
									AdjdDocType = Document.Current.DocType,
									AdjdRefNbr = Document.Current.RefNbr,
									AdjdLineNbr = 0,
									AdjgDocType = payment.DocType,
									AdjgRefNbr = payment.RefNbr,
									AdjNbr = payment.AdjCntr
								};
								APAdjust existing;
								if ((existing = Adjustments.Locate(adj)) == null)
								{
									adj.VendorID = Document.Current.VendorID;
									adj.AdjdBranchID = Document.Current.BranchID;
									adj.AdjgBranchID = payment.BranchID;
									adj.AdjgCuryInfoID = payment.CuryInfoID;
									adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
									adj.AdjdCuryInfoID = Document.Current.CuryInfoID;
									adj.CuryDocBal = BalanceCalculation.CalculateApplicationDocumentBalance(
										Adjustments.Cache,
										pay_info,
										inv_info,
										payment.DocBal,
										payment.CuryDocBal);

									existing = Adjustments.Insert(adj);
								}
								decimal? orderAppliedAmt = BalanceCalculation.CalculateApplicationDocumentBalance(
									Adjustments.Cache,
									orderInfo,
									inv_info,
									orderPrepay.AppliedAmt,
									orderPrepay.CuryAppliedAmt);

								existing.CuryAdjdAmt = new[] { existing.CuryDocBal, orderAppliedAmt, invoiceAmtToPay }.Min();
								existing = Adjustments.Update(existing);
								invoiceAmtToPay -= existing.CuryAdjdAmt;
							}
						}
					}
				}
			}
		}

		public virtual void InvoicePOOrder(POOrder order, bool createNew, bool keepOrderTaxes = false)
		{
			APInvoice doc;
			if (createNew)
			{
				doc = new APInvoice
				{
					DocType = APDocType.Invoice,
					BranchID = order.BranchID,
					OrigModule = BatchModule.PO
				};
				doc = PXCache<APInvoice>.CreateCopy(Document.Insert(doc));
				if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
				{
					doc.VendorID = order.PayToVendorID;
					doc.VendorLocationID = order.VendorID == order.PayToVendorID ? order.VendorLocationID : null;
					doc.SuppliedByVendorID = order.VendorID;
					doc.SuppliedByVendorLocationID = order.VendorLocationID;
				}
				else
				{
					doc.VendorID = 
					doc.SuppliedByVendorID = order.VendorID;
					doc.VendorLocationID =
					doc.SuppliedByVendorLocationID = order.VendorLocationID;
				}
				doc.TermsID = order.TermsID;
				doc.InvoiceNbr = order.VendorRefNbr;
				doc.CuryID = order.CuryID;
				doc.DocDesc = order.OrderDesc;
				doc.ProjectID = order.ProjectID;
				doc.TaxZoneID = order.TaxZoneID;
				doc.TaxCalcMode = order.TaxCalcMode;
				doc.RetainageApply = order.RetainageApply;
				doc.DefRetainagePct = order.DefRetainagePct;

				var cancel_defaulting = new PXFieldDefaulting((cache, e) => { e.NewValue = cache.GetValue<APInvoice.branchID>(e.Row); e.Cancel = true; });
				this.FieldDefaulting.AddHandler<APInvoice.branchID>(cancel_defaulting);

				try
				{
					doc = Document.Update(doc);
				}
				finally
				{
					this.FieldDefaulting.RemoveHandler<APInvoice.branchID>(cancel_defaulting);
				}

				doc = Document.Update(doc);
			}
			else
			{
				doc = PXCache<APInvoice>.CreateCopy(Document.Current);
				if (string.IsNullOrEmpty(doc.DocDesc))
					doc.DocDesc = order.OrderDesc;
				if (string.IsNullOrEmpty(doc.InvoiceNbr))
					doc.InvoiceNbr = order.VendorRefNbr;
				doc = this.Document.Update(doc);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>() &&
				((doc.VendorID != null && doc.VendorID != order.VendorID)
					|| (doc.VendorLocationID != null && doc.VendorLocationID != order.VendorLocationID)))
			{
				throw new PXException(Messages.APBillHasDifferentVendorOrLocation, doc.RefNbr, order.OrderNbr);
			}

			if (doc.CuryID != order.CuryID)
			{
				throw new PXException(Messages.APBillHasDifferentCury, doc.RefNbr, doc.CuryID, order.OrderNbr, order.CuryID);
			}

			var duplicates = GetUsedPOAccrualSet();

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(Transactions.Cache, null, TaxCalc.ManualCalc);

			ProcessPOOrderLines(order, duplicates, addBilled: !createNew, keepOrderTaxes: keepOrderTaxes);

			if (keepOrderTaxes)
				AddOrderTaxes(order);

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

			if (!keepOrderTaxes)
			{
				object copy = PXCache<APInvoice>.CreateCopy(Document.Current);
				Document.Current.TaxZoneID = null;
				Document.Cache.Update(copy);
			}

			Document.Cache.RaiseFieldUpdated<APInvoice.curyOrigDocAmt>(doc, 0m);
		}

		public virtual void ProcessPOOrderLines(POOrder order, HashSet<APTran> duplicates, bool addBilled, bool keepOrderTaxes = false)
		{
			PXSelectBase<POLineS> cmd = new PXSelectReadonly<POLineS,
				Where<POLineS.orderType, Equal<Required<POLineS.orderType>>,
					And<POLineS.orderNbr, Equal<Required<POLineS.orderNbr>>,
					And<POLineS.cancelled, Equal<False>,
					And<POLineS.closed, Equal<False>,
					And<POLineS.pOAccrualType, Equal<POAccrualType.order>>>>>>,
				OrderBy<Asc<POLineS.sortOrder>>>(this);
			if (!addBilled)
			{
				cmd.Join<LeftJoin<APTran, On<APTran.pOAccrualRefNoteID, Equal<POLineS.orderNoteID>, And<APTran.pOAccrualLineNbr, Equal<POLineS.lineNbr>,
					And<APTran.released, Equal<False>>>>>>();
				cmd.WhereAnd<Where<APTran.refNbr, IsNull>>();
				cmd.WhereAnd<Where<POLineS.billed, Equal<False>>>();
			}

			ProcessPOOrderLines(
				cmd.Select(order.OrderType, order.OrderNbr).RowCast<POLineS>(),
				duplicates,
				keepOrderTaxes);
		}

		public virtual void ProcessPOOrderLines(IEnumerable<IAPTranSource> orderlines, HashSet<APTran> duplicates = null, bool keepOrderTaxes = false)
		{
			duplicates = duplicates ?? new POAccrualSet();

			bool failedToAddLine = false;
			HashSet<string> ordersWithDiscounts = new HashSet<string>();
			foreach (var orderline in orderlines)
			{
				TaxAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(this.Transactions.Cache, null,
						keepOrderTaxes ? TaxCalc.ManualCalc : TaxCalc.ManualLineCalc);

				PXRowInserting handler = (sender, e) =>
			{
					PXParentAttribute.SetParent(sender, e.Row, typeof(APRegister), Document.Current);
				};

				RowInserting.AddHandler<APTran>(handler);

				try
				{
					if ((orderline.RetainagePct ?? 0m) != 0m)
					{
						EnableRetainage();
					}

					APTran tran = AddPOReceiptLine(orderline, duplicates);
					
					if (tran?.PONbr != null
						&& this.Document.Current != null && orderline.DocumentDiscountRate != null && (orderline.GroupDiscountRate != 1 || orderline.DocumentDiscountRate != 1))
					{
						this.Document.Current.SetWarningOnDiscount = true;
						ordersWithDiscounts.Add(tran.PONbr);
					}
				}
				catch (PXException ex)
				{
					PXTrace.WriteError(ex);
					failedToAddLine = true;
				}
				finally
				{
					RowInserting.RemoveHandler<APTran>(handler);
				}
			}

			if (failedToAddLine)
			{
				throw new PXException(PO.Messages.FailedToAddLine);
			}

			AutoRecalculateDiscounts();

			WritePODiscountWarningToTrace(this.Document.Current, ordersWithDiscounts);
		}

		#endregion

		#region Utility Functions

		public virtual void WritePODiscountWarningToTrace(APInvoice document, HashSet<string> ordersWithDiscounts)
		{
			if (document.SetWarningOnDiscount == true && ordersWithDiscounts.Count > 0)
			{
				PXTrace.WriteWarning(Messages.DiscountInOriginalPOFoundNoTrace, string.Join(", ", ordersWithDiscounts.ToArray()));
			}
		}

        public bool IsApprovalRequired(APInvoice doc, PXCache cache)
	    {
	        var isApprovalInstalled = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();
	        var areMapsAssigned = Approval.GetAssignedMaps(doc, cache).Any();
	        return isApprovalInstalled && areMapsAssigned;
	    }

		public virtual APTran AddPOReceiptLine(IAPTranSource aLine, HashSet<APTran> checkForDuplicates)
		{
			APTran newtran = new APTran(),
				updTran = null;
			aLine.SetReferenceKeyTo(newtran);

			if (checkForDuplicates?.Contains(newtran) == true)
			{
				if (!aLine.AggregateWithExistingTran)
				{
					return null;
				}
				updTran = checkForDuplicates.First(t => checkForDuplicates.Comparer.Equals(t, newtran));
			}
			newtran.TranType = Document.Current.DocType;
			newtran.RefNbr = Document.Current.RefNbr;
			newtran.BranchID = aLine.BranchID;
			newtran.AccrueCost = aLine.AccrueCost;

			InventoryItem item = InventoryItemGetByID(aLine.InventoryID);
			if (aLine.AccrueCost == true
				&& !(PXAccess.FeatureInstalled<FeaturesSet.inventory>() && insetup != null && insetup.Current != null && insetup.Current.UpdateGL == true && item != null && item.NonStockReceipt == true))
			{
				newtran.AccountID = aLine.ExpenseAcctID;
				newtran.SubID = aLine.ExpenseSubID;
			}
			else
			{
				newtran.AccountID = aLine.POAccrualAcctID ?? aLine.ExpenseAcctID;
				newtran.SubID = aLine.POAccrualSubID ?? aLine.ExpenseSubID;
			}

			newtran.LineType = aLine.LineType;
			newtran.SiteID = aLine.SiteID;
			newtran.InventoryID = aLine.InventoryID;
			newtran.UOM = string.IsNullOrEmpty(aLine.UOM) ? null : aLine.UOM;

			bool keepAmounts = !aLine.IsPartiallyBilled || aLine.BaseOrigQty == 0m;
			decimal? billRatio = keepAmounts ? 1m
				: (!string.IsNullOrEmpty(aLine.UOM) && aLine.UOM == aLine.OrigUOM && aLine.BillQty != null) ? aLine.BillQty / aLine.OrigQty
				: aLine.BaseBillQty / aLine.BaseOrigQty;
			if (aLine.BillQty != null)
			{
				newtran.Qty = aLine.BillQty;
			}
			else
			{
				newtran.BaseQty = aLine.BaseBillQty;
				PXDBQuantityAttribute.CalcTranQty<APTran.qty>(this.Transactions.Cache, newtran);
			}
			decimal? amtToBill = billRatio * aLine.LineAmt;
			decimal? curyAmtToBill = billRatio * aLine.CuryLineAmt;
			decimal? discAmt = billRatio * (aLine.DiscAmt ?? 0m);
			decimal? curyDiscAmt = billRatio * (aLine.CuryDiscAmt ?? 0m);
			decimal? retainAmt = billRatio * (aLine.RetainageAmt ?? 0m);
			decimal? curyRetainAmt = billRatio * (aLine.CuryRetainageAmt ?? 0m);
			decimal? retainPct = aLine.RetainagePct ?? 0m;

			newtran.ManualPrice = true;
			newtran.ManualDisc = newtran.PONbr != null ? true : false;
			newtran.FreezeManualDisc = true;
			newtran.DiscountID = aLine.DiscountID;
			newtran.DiscountSequenceID = aLine.DiscountSequenceID;

			CurrencyInfo lineCuryInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, aLine.CuryInfoID);
			CurrencyInfo billCuryInfo = this.currencyinfo.Select();
			bool areCurrenciesSame = (billCuryInfo.CuryID == lineCuryInfo?.CuryID);
			if (areCurrenciesSame)
			{
				newtran.CuryUnitCost = aLine.CuryUnitCost;
				newtran.CuryLineAmt = keepAmounts ? curyAmtToBill : PXCurrencyAttribute.Round(Transactions.Cache, newtran, curyAmtToBill ?? 0m, CMPrecision.TRANCURY);
				newtran.CuryDiscAmt = keepAmounts ? curyDiscAmt : PXCurrencyAttribute.Round(Transactions.Cache, newtran, curyDiscAmt ?? 0m, CMPrecision.TRANCURY);
				newtran.CuryRetainageAmt = keepAmounts ? curyRetainAmt : 0m;
			}
			else
			{
				decimal curyUnitCost;
				decimal curyLineAmt;
				decimal curyDiscAmount;
				decimal curyRetainAmount;
				int costPrecision = this.commonsetup.Current.DecPlPrcCst.Value;
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(this.Document.Cache, this.Document.Current, aLine.UnitCost.Value, out curyUnitCost, costPrecision);
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(this.Document.Cache, this.Document.Current, amtToBill.Value, out curyLineAmt);
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(this.Document.Cache, this.Document.Current, discAmt.Value, out curyDiscAmount);
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(this.Document.Cache, this.Document.Current, retainAmt.Value, out curyRetainAmount);
				newtran.CuryUnitCost = curyUnitCost;
				newtran.CuryLineAmt = curyLineAmt;
				newtran.CuryDiscAmt = curyDiscAmount;
				newtran.CuryRetainageAmt = (keepAmounts ? curyRetainAmount : 0m);
			}

			if (!string.IsNullOrEmpty(newtran.UOM) && newtran.UOM != aLine.OrigUOM)
			{
				decimal curyUnitCost = newtran.CuryUnitCost ?? 0m;
				curyUnitCost = INUnitAttribute.ConvertFromBase<APTran.inventoryID>(this.Transactions.Cache, newtran, aLine.OrigUOM, curyUnitCost, INPrecision.UNITCOST);
				curyUnitCost = INUnitAttribute.ConvertToBase<APTran.inventoryID>(this.Transactions.Cache, newtran, newtran.UOM, curyUnitCost, INPrecision.UNITCOST);
				newtran.CuryUnitCost = curyUnitCost;
			}

            CopyCustomizationFieldsToAPTran(newtran, aLine, areCurrenciesSame);

            newtran.DiscPct = aLine.DiscPct;
			newtran.RetainagePct = (keepAmounts ? retainPct : 0m);
			newtran.TranDesc = aLine.TranDesc;
			newtran.TaxCategoryID = aLine.TaxCategoryID;
			newtran.TaxID = aLine.TaxID;
			newtran.ProjectID = aLine.ProjectID;
			newtran.TaskID = aLine.TaskID;
			newtran.CostCodeID = aLine.CostCodeID;
			
			if (updTran == null)
			{
				newtran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<APTran.lineNbr>(Transactions.Cache, Document.Current);
				newtran = this.Transactions.Insert(newtran);

				if (!keepAmounts && retainPct != 0m)
				{
					// we need this update because retainage amt should be calculated after disc amt
					// it does not work without update because ManualDiscountMode attribute sets the disc amt silently in RowInserted handler
					newtran.RetainagePct = retainPct;
					newtran = this.Transactions.Update(newtran);
			}

				checkForDuplicates.Add(newtran);
				return newtran;
			}
			else
			{
				if (updTran.UOM != newtran.UOM)
				{
					this.Transactions.Cache.RaiseExceptionHandling<APTran.qty>(updTran, updTran.Qty,
						new PXSetPropertyException(PO.Messages.SomeLinesSkippedBecauseUom, PXErrorLevel.Warning));
					return null;
				}

				updTran = (APTran)this.Transactions.Cache.CreateCopy(updTran);
				updTran.Qty += newtran.Qty;
				updTran.CuryLineAmt += newtran.CuryLineAmt;
				updTran.CuryDiscAmt += newtran.CuryDiscAmt;
				updTran = this.Transactions.Update(updTran);
				this.Transactions.Cache.RaiseExceptionHandling<APTran.qty>(updTran, updTran.Qty,
					new PXSetPropertyException(PO.Messages.SomeLinesAggregated, PXErrorLevel.Warning));

				checkForDuplicates.Remove(newtran);
				checkForDuplicates.Add(updTran);
				return updTran;
			}
		}

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        protected virtual void CopyCustomizationFieldsToAPTran(APTran apTranToFill, IAPTranSource poSourceLine, bool areCurrenciesSame)
        {
            //Extension point used in customizations to copy customization field. Used in LexWare PriceUnits customization
        }

		private InventoryItem InventoryItemGetByID(int? inventoryID)
		{
			return PX.Objects.IN.InventoryItem.PK.Find(this, inventoryID);
		}

		private DRDeferredCode DeferredCodeGetByID(string deferredCodeID)
		{
			return PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, deferredCodeID);
		}

		#endregion

		public virtual void CheckOrderTaskRule(PXCache sender, APTran row, int? newTaskID)
		{
			if (row.POOrderType != null && row.PONbr != null && row.POLineNbr != null && !POLineType.IsStock(row.LineType))
			{
				POLine poLine = PXSelectReadonly<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
								And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
								And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.Select(this, row.POOrderType, row.PONbr, row.POLineNbr);

				if (poLine != null && poLine.TaskID != null && poLine.TaskID != newTaskID)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID);
					string taskCd = task != null ? taskCd = task.TaskCD : null;

					if (posetup.Current.OrderRequestApproval == true)
					{
						sender.RaiseExceptionHandling<APTran.taskID>(row, taskCd,
							new PXSetPropertyException(PO.Messages.TaskDiffersError, PXErrorLevel.Error));
					}
					else
					{
						sender.RaiseExceptionHandling<APTran.taskID>(row, taskCd,
							new PXSetPropertyException(PO.Messages.TaskDiffersWarning, PXErrorLevel.Warning));
					}
				}
			}
		}

		public virtual void CheckProjectAccountRule(PXCache sender, APTran row)
		{
			if (row.ProjectID != ProjectDefaultAttribute.NonProject())
			{
				var select = new PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>(this);
				Account account = null;
				if (row.AccountID != null)
				{
					account = select.Select(row.AccountID);
				}

				if (account != null)
				{
					if (account.AccountGroupID == null)
					{
						sender.RaiseExceptionHandling<APTran.accountID>(row, account.AccountCD,
								new PXSetPropertyException(Messages.NoAccountGroup, PXErrorLevel.Error, account.AccountCD));
					}
				}
			}
		}

		protected virtual void CheckQtyFromPO(PXCache sender, APTran tran)
		{
			string lineType = null;
			decimal? aPTranQty = tran.Qty;
			
			switch (tran.POAccrualType)
			{
				case POAccrualType.Receipt:
					POReceiptLine rctLine = PXSelectReadonly<POReceiptLine,
						Where<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
							And<POReceiptLine.lineNbr, Equal<Required<POReceiptLine.lineNbr>>>>>
						.SelectWindowed(this, 0, 1, tran.ReceiptNbr, tran.ReceiptLineNbr);
					lineType = rctLine.LineType;
					
					decimal? pOReceiptLineUnbilledQty = rctLine.UnbilledQty;

					if (tran.UOM != rctLine.UOM)
					{
						aPTranQty = tran.BaseQty;
						pOReceiptLineUnbilledQty = rctLine.BaseUnbilledQty;
					}

					if (aPTranQty > pOReceiptLineUnbilledQty && tran.Sign == ((rctLine.InvtMult < 0) ? -1m : 1m))
					{
						if (!sender.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty,
							new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOReceiptQuantity, PXErrorLevel.Error)))
						{
							throw new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOReceiptQuantity);
						}
					}
					break;
				case POAccrualType.Order:
					POLine poLine = PXSelectReadonly<POLine,
						Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
							And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
						.SelectWindowed(this, 0, 1, tran.POOrderType, tran.PONbr, tran.POLineNbr);
					lineType = poLine.LineType;
					if (tran.Sign < 0) break;
					
					decimal? pOLineBilledQty = poLine.BilledQty;
					decimal? maxRcptQty = poLine.OrderQty * poLine.RcptQtyMax / 100m;
					
					if (tran.UOM != poLine.UOM)
					{
						aPTranQty = tran.BaseQty;
						pOLineBilledQty = poLine.BaseBilledQty;
						maxRcptQty = poLine.BaseOrderQty * poLine.RcptQtyMax / 100m;
					}

					if (poLine.RcptQtyAction.IsIn(POReceiptQtyAction.Reject, POReceiptQtyAction.AcceptButWarn)
						&& aPTranQty + pOLineBilledQty > maxRcptQty)
					{
						PXErrorLevel errorLevel = (poLine.RcptQtyAction == POReceiptQtyAction.Reject) ? PXErrorLevel.Error : PXErrorLevel.Warning;
						if (!sender.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty,
								new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOQuantity, errorLevel))
							&& errorLevel == PXErrorLevel.Error)
						{
							throw new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOQuantity);
						}
					}
					break;
			}

			if (tran.Qty == 0m && tran.CuryTranAmt != 0m && POLineType.UsePOAccrual(lineType))
			{
				sender.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty,
					new PXSetPropertyException(CS.Messages.Entry_NE, PXErrorLevel.Error, 0m));
			}
		}

		protected virtual void RecalculateDiscounts(PXCache sender, APTran line)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>() && line.CuryLineAmt != null)
			{
				if (line.Qty != null)
			{
					    bool enabledVendorRelations = PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>();
					    if (enabledVendorRelations)
					    {
					        line.SuppliedByVendorID = Document.Current.SuppliedByVendorID;
					    }

					DiscountEngine.DiscountCalculationOptions discountCalculationOptions = DiscountEngine.DefaultAPDiscountCalculationParameters;
					if (line.CalculateDiscountsOnImport == true)
						discountCalculationOptions = discountCalculationOptions | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport;

					_discountEngine.SetDiscounts(
							sender,
							Transactions,
							line,
							DiscountDetails,
							Document.Current.BranchID,
							enabledVendorRelations
                                ? Document.Current.SuppliedByVendorLocationID
								: Document.Current.VendorLocationID,
							Document.Current.CuryID,
							Document.Current.DocDate,
							recalcdiscountsfilter.Current,
							discountCalculationOptions);
				}
				RecalculateTotalDiscount();
			}
			else if (!PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>() && Document.Current != null)
			{
				_discountEngine.CalculateDocumentDiscountRate(Transactions.Cache, Transactions, line, DiscountDetails);
				}
			}

        public virtual void AutoRecalculateDiscounts()
		{
			bool enabledVendorRelations = PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>();

			_discountEngine.AutoRecalculatePricesAndDiscounts(
				Transactions.Cache, 
				Transactions, 
				null, 
				DiscountDetails, 
				enabledVendorRelations
				? Document.Current.SuppliedByVendorLocationID
				: Document.Current.VendorLocationID, 
				Document.Current.DocDate, 
				DiscountEngine.DefaultAPDiscountCalculationParameters);
			
				RecalculateTotalDiscount();
		}

		private void RecalculateApplicationsAmounts()
		{
			PXCache applicationCache = Caches[typeof(APAdjust)];
			APInvoice invoice = Document.Current;

			decimal curyInvoiceBalance = invoice.CuryDocBal ?? 0m;
			decimal baseInvoiceBalance = invoice.DocBal ?? 0m;

			foreach (APAdjust application in Adjustments.Select())
			{
				decimal curyAdjustingAmount;
			decimal baseAdjustedAmount;
			decimal baseAdjustingAmount;

			PXDBCurrencyAttribute.CuryConvBase<APAdjust.adjdCuryInfoID>(
				applicationCache, 
				application, 
				application.CuryAdjdAmt.Value, 
				out baseAdjustedAmount);

			CurrencyInfo applicationCurrencyInfo = PXSelect<
				CurrencyInfo, 
				Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjgCuryInfoID>>>>
				.SelectSingleBound(this, new object[] { application });

			CurrencyInfo documentCurrencyInfo = PXSelect<
				CurrencyInfo, 
				Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjdCuryInfoID>>>>
				.SelectSingleBound(this, new object[] { application });

			if (string.Equals(applicationCurrencyInfo.CuryID, documentCurrencyInfo.CuryID))
			{
					curyAdjustingAmount = (decimal)application.CuryAdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury<APAdjust.adjgCuryInfoID>(
					applicationCache, 
					application, 
					baseAdjustedAmount, 
						out curyAdjustingAmount);
			}

				if (Equals(applicationCurrencyInfo.CuryID, documentCurrencyInfo.CuryID) &&
					Equals(applicationCurrencyInfo.CuryRate, documentCurrencyInfo.CuryRate) &&
					Equals(applicationCurrencyInfo.CuryMultDiv, documentCurrencyInfo.CuryMultDiv))
			{
				baseAdjustingAmount = baseAdjustedAmount;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvBase<APAdjust.adjgCuryInfoID>(
					applicationCache, 
					application, 
						curyAdjustingAmount,
					out baseAdjustingAmount);
			}

				// We do not use this write-off, withholding tax and discount values because expect the zero amounts.
				application.CuryAdjgAmt = curyAdjustingAmount;
				application.AdjAmt = (curyInvoiceBalance == application.CuryAdjdAmt.Value ? baseInvoiceBalance : baseAdjustedAmount);
				application.RGOLAmt = baseAdjustingAmount - application.AdjAmt;

				curyInvoiceBalance -= curyAdjustingAmount;
				baseInvoiceBalance -= baseAdjustedAmount;
			}
		}

		private void RecalculateTotalDiscount()
		{
			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted)
			{
				APInvoice old_row = PXCache<APInvoice>.CreateCopy(Document.Current);
				Document.Cache.SetValueExt<APInvoice.curyDiscTot>(Document.Current, _discountEngine.GetTotalGroupAndDocumentDiscount(DiscountDetails));
				Document.Cache.RaiseRowUpdated(Document.Current, old_row);
			}
		}


		public virtual void SetProjectIDForDiscountTran(APTran apTran)
		{
			var docProjectIDs = Transactions.Select()
											.RowCast<APTran>()
											.Where(tran => tran.ProjectID != null)
											.Select(tran => tran.ProjectID)
											.Distinct()
											.ToArray();

			var projectID = docProjectIDs.Length == 1
								? docProjectIDs.Single()
								: ProjectDefaultAttribute.NonProject();

			apTran.ProjectID = projectID;
		}

		public virtual void SetTaskIDForDiscountTran(APTran apTran)
		{
			PM.PMProject project = PXSelect<PM.PMProject, 
											Where<PM.PMProject.contractID, Equal<Required<PM.PMProject.contractID>>>>
											.Select(this, apTran.ProjectID);

			if (project != null && project.BaseType != CT.CTPRType.Contract)
			{
				PM.PMAccountTask task = PXSelect<PM.PMAccountTask, 
													Where<PM.PMAccountTask.projectID, Equal<Required<PM.PMAccountTask.projectID>>, 
														And<PM.PMAccountTask.accountID, Equal<Required<PM.PMAccountTask.accountID>>>>>
													.Select(this, apTran.ProjectID, apTran.AccountID);
				if (task != null)
				{
					apTran.TaskID = task.TaskID;
					return;
				}

				Account ac = PXSelect<Account,
										Where<Account.accountID, Equal<Required<Account.accountID>>>>
										.Select(this, apTran.AccountID);

				throw new PXException(Messages.AccountMappingNotConfiguredForDiscount, project.ContractCD.Trim(), ac.AccountCD.Trim());
			}
		}

		public virtual void SetCostCodeForDiscountTran(APTran apTran)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				apTran.CostCodeID = CostCodeAttribute.DefaultCostCode;
			}
		}

		protected virtual void AddDiscount(PXCache sender, APInvoice row)
		{
			APTran discount = (APTran)Discount_Row.Cache.CreateInstance();
			discount.LineType = SOLineType.Discount;
			discount.DrCr = (Document.Current.DrCr == DrCr.Debit) ? DrCr.Credit : DrCr.Debit;
			discount = (APTran)Discount_Row.Select() ?? (APTran)Discount_Row.Cache.Insert(discount);

			APTran old_row = (APTran)Discount_Row.Cache.CreateCopy(discount);

			discount.CuryTranAmt = (decimal?)sender.GetValue<APInvoice.curyDiscTot>(row);
			discount.TaxCategoryID = null;
			using (new PXLocaleScope(vendor.Current?.LocaleName))
			{
			discount.TranDesc = PXMessages.LocalizeNoPrefix(Messages.DocDiscDescr);
			}

			DefaultDiscountAccountAndSubAccount(discount);

			//ToDo: create separate discount lines for different projects 
			if (discount.ProjectID == null)
			{
				SetProjectIDForDiscountTran(discount);
			}

			SetCostCodeForDiscountTran(discount);

			if (discount.ProjectID == null && old_row.ProjectID != null)
				discount.ProjectID = old_row.ProjectID;

			if (discount.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject( discount.ProjectID))
			{
				SetTaskIDForDiscountTran(discount);
			}

			Discount_Row.Cache.MarkUpdated(discount);

			discount.ManualDisc = true; //escape SOManualDiscMode.RowUpdated
			Discount_Row.Cache.RaiseRowUpdated(discount, old_row);

			decimal auotDocDisc = _discountEngine.GetTotalGroupAndDocumentDiscount(DiscountDetails);
			if (auotDocDisc == discount.CuryTranAmt)
			{
				discount.ManualDisc = false;
			}
		}

		

		protected object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return this.Caches[BqlCommand.GetItemType(typeof(Field))].GetValue(data, typeof(Field).Name);
		}

		private void DefaultDiscountAccountAndSubAccount(APTran tran)
		{
			Location vendorloc = location.Current;

			object vendor_LocationAcctID = GetValue<Location.vDiscountAcctID>(vendorloc);

			if (vendor_LocationAcctID != null)
			{
				tran.AccountID = (int?)vendor_LocationAcctID;
				Discount_Row.Cache.RaiseFieldUpdated<APTran.accountID>(tran, null);
			}

			if (tran.AccountID != null)
			{
				object vendor_LocationSubID = GetValue<Location.vDiscountSubID>(vendorloc);
				if (vendor_LocationSubID != null)
				{
					tran.SubID = (int?)vendor_LocationSubID;
					Discount_Row.Cache.RaiseFieldUpdated<APTran.subID>(tran, null);
				}
			}

		}

		#region Private Variables
		private bool _allowToVoidReleased = false;
		#endregion

		#region Internal Member Definitions


		#endregion

		#region APAdjust
		[Serializable]        
		public partial class APAdjust : PX.Objects.AP.APAdjust
		{
			#region VendorID
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
			{
			}
			[PXDBInt]
			[PXDBDefault(typeof(AP.APInvoice.vendorID))]
			[PXUIField(DisplayName = "Vendor ID", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
			#region AdjgDocType
			public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType>
			{
			}
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
			[APPaymentType.List()]
			[PXDefault()]
			[PXUIField(DisplayName = "Doc. Type", Enabled = false)]
			public override String AdjgDocType
			{
				get
				{
					return this._AdjgDocType;
				}
				set
				{
					this._AdjgDocType = value;
				}
			}
			#endregion
			#region AdjgRefNbr
			public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr>
			{
			}
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			[APPaymentType.AdjgRefNbr(typeof(Search<APPayment.refNbr, Where<APPayment.docType, Equal<Optional<APAdjust.adjgDocType>>>>), Filterable = true)]
			public override String AdjgRefNbr
			{
				get
				{
					return this._AdjgRefNbr;
				}
				set
				{
					this._AdjgRefNbr = value;
				}
			}
			#endregion
			#region AdjgBranchID
			public new abstract class adjgBranchID : PX.Data.BQL.BqlInt.Field<adjgBranchID>
			{
			}

			[Branch(useDefaulting: false)]
			public override Int32? AdjgBranchID
			{
				get
				{
					return this._AdjgBranchID;
				}
				set
				{
					this._AdjgBranchID = value;
				}
			}
			#endregion
			#region AdjdCuryInfoID
			public new abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID>
			{
			}
			[PXDBLong()]
			[CurrencyInfo(typeof(AP.APInvoice.curyInfoID), ModuleCode = BatchModule.AP, CuryIDField = "AdjdCuryID")]
			public override Int64? AdjdCuryInfoID
			{
				get
				{
					return this._AdjdCuryInfoID;
				}
				set
				{
					this._AdjdCuryInfoID = value;
				}
			}
			#endregion
			#region AdjdDocType
			public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType>
			{
			}
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
			[PXDBDefault(typeof(AP.APInvoice.docType))]
			[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public override String AdjdDocType
			{
				get
				{
					return this._AdjdDocType;
				}
				set
				{
					this._AdjdDocType = value;
				}
			}
			#endregion
			#region AdjdRefNbr
			public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr>
			{
			}
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDBDefault(typeof(AP.APInvoice.refNbr))]
			[PXParent(typeof(Select<AP.APInvoice, Where<AP.APInvoice.docType, Equal<Current<APAdjust.adjdDocType>>, And<AP.APInvoice.refNbr, Equal<Current<APAdjust.adjdRefNbr>>>>>))]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public override string AdjdRefNbr
				{
				get;
				set;
			}
			#endregion
			#region AdjdLineNbr
			public new abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }

			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.PaymentsByLines))]
			[PXDefault(typeof(Switch<Case<Where<Selector<APAdjust.adjdRefNbr, APInvoice.paymentsByLinesAllowed>, NotEqual<True>>, int0>, Null>))]
			[APInvoiceType.AdjdLineNbr]
			public override int? AdjdLineNbr
			{
				get;
				set;
			}
			#endregion
			#region AdjdBranchID
			public new abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID>
			{
			}
			[Branch(typeof(AP.APInvoice.branchID))]
			public override Int32? AdjdBranchID
			{
				get
				{
					return this._AdjdBranchID;
				}
				set
				{
					this._AdjdBranchID = value;
				}
			}
			#endregion
			#region AdjNbr
			public new abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr>
			{
			}
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
			[PXDefault()]
			public override Int32? AdjNbr
			{
				get
				{
					return this._AdjNbr;
				}
				set
				{
					this._AdjNbr = value;
				}
			}
			#endregion
			#region StubNbr
			public new abstract class stubNbr : PX.Data.BQL.BqlString.Field<stubNbr>
			{
			}
			[PXDBString(40, IsUnicode = true)]
			public override String StubNbr
			{
				get
				{
					return this._StubNbr;
				}
				set
				{
					this._StubNbr = value;
				}
			}
			#endregion
			#region AdjBatchNbr
			public new abstract class adjBatchNbr : PX.Data.BQL.BqlString.Field<adjBatchNbr>
			{
			}
			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
			public override String AdjBatchNbr
			{
				get
				{
					return this._AdjBatchNbr;
				}
				set
				{
					this._AdjBatchNbr = value;
				}
			}
			#endregion
			#region AdjdOrigCuryInfoID
			public new abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID>
			{
			}
			[PXDBLong()]
			[PXDBDefault(typeof(AP.APInvoice.curyInfoID))]
			public override Int64? AdjdOrigCuryInfoID
			{
				get
				{
					return this._AdjdOrigCuryInfoID;
				}
				set
				{
					this._AdjdOrigCuryInfoID = value;
				}
			}
			#endregion
			#region AdjgCuryInfoID
			public new abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID>
			{
			}
			[PXDBLong()]
			[CurrencyInfo(ModuleCode = BatchModule.AP, CuryIDField = "AdjgCuryID")]
			public override Int64? AdjgCuryInfoID
			{
				get
				{
					return this._AdjgCuryInfoID;
				}
				set
				{
					this._AdjgCuryInfoID = value;
				}
			}
			#endregion
			#region AdjgDocDate
			public new abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate>
			{
			}
			[PXDBDate()]
			[PXDBDefault(typeof(AP.APInvoice.docDate))]
			[PXUIField(DisplayName = "Transaction Date")]
			public override DateTime? AdjgDocDate
			{
				get
				{
					return this._AdjgDocDate;
				}
				set
				{
					this._AdjgDocDate = value;
				}
			}
			#endregion
			#region AdjgFinPeriodID
			public new abstract class adjgFinPeriodID : PX.Data.BQL.BqlString.Field<adjgFinPeriodID>
			{
			}
		    [FinPeriodID(
		        branchSourceType: typeof(APAdjust.adjgBranchID),
		        masterFinPeriodIDType: typeof(APAdjust.adjgTranPeriodID),
		        headerMasterFinPeriodIDType: typeof(AP.APInvoice.tranPeriodID))]
			[PXUIField(DisplayName = "Application Period")]
			public override String AdjgFinPeriodID
			{
				get
				{
					return this._AdjgFinPeriodID;
				}
				set
				{
					this._AdjgFinPeriodID = value;
				}
			}
			#endregion
			#region AdjgTranPeriodID
			public new abstract class adjgTranPeriodID : PX.Data.BQL.BqlString.Field<adjgTranPeriodID>
			{
			}
			[PeriodID]
			public override String AdjgTranPeriodID
			{
				get
				{
					return this._AdjgTranPeriodID;
				}
				set
				{
					this._AdjgTranPeriodID = value;
				}
			}
			#endregion
			#region AdjdDocDate
			public new abstract class adjdDocDate : PX.Data.BQL.BqlDateTime.Field<adjdDocDate>
			{
			}
			[PXDBDate()]
			[PXDBDefault(typeof(AP.APInvoice.docDate))]
			[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override DateTime? AdjdDocDate
			{
				get
				{
					return this._AdjdDocDate;
				}
				set
				{
					this._AdjdDocDate = value;
				}
			}
			#endregion
			#region AdjdFinPeriodID
			public new abstract class adjdFinPeriodID : PX.Data.BQL.BqlString.Field<adjdFinPeriodID>
			{
			}
			[FinPeriodID(
			    branchSourceType: typeof(APAdjust.adjdBranchID),
			    masterFinPeriodIDType: typeof(APAdjust.adjdTranPeriodID),
			    headerMasterFinPeriodIDType: typeof(AP.APInvoice.tranPeriodID))]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
			public override String AdjdFinPeriodID
			{
				get
				{
					return this._AdjdFinPeriodID;
				}
				set
				{
					this._AdjdFinPeriodID = value;
				}
			}
			#endregion
			#region AdjdTranPeriodID
			public new abstract class adjdTranPeriodID : PX.Data.BQL.BqlString.Field<adjdTranPeriodID>
			{
			}
			[PeriodID]
			public override String AdjdTranPeriodID
			{
				get
				{
					return this._AdjdTranPeriodID;
				}
				set
				{
					this._AdjdTranPeriodID = value;
				}
			}
			#endregion
			#region CuryAdjdDiscAmt
			public new abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt>
			{
			}
			[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjDiscAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjdDiscAmt
			{
				get
				{
					return this._CuryAdjdDiscAmt;
				}
				set
				{
					this._CuryAdjdDiscAmt = value;
				}
			}
			#endregion
			#region CuryAdjdAmt
			public new abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt>
			{
			}
			[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjAmt), BaseCalc = false, MinValue = 0)]
			[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjdAmt
			{
				get; set;
			}
			#endregion
			#region CuryAdjdWhTaxAmt
			public new abstract class curyAdjdWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdWhTaxAmt>
			{
			}
			[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjWhTaxAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjdWhTaxAmt
			{
				get
				{
					return this._CuryAdjdWhTaxAmt;
				}
				set
				{
					this._CuryAdjdWhTaxAmt = value;
				}
			}
			#endregion
			#region AdjAmt
			public new abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Amount")]
			public override Decimal? AdjAmt
			{
				get
				{
					return this._AdjAmt;
				}
				set
				{
					this._AdjAmt = value;
				}
			}
			#endregion
			#region AdjDiscAmt
			public new abstract class adjDiscAmt : PX.Data.BQL.BqlDecimal.Field<adjDiscAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Cash Discount Amount")]
			public override Decimal? AdjDiscAmt
			{
				get
				{
					return this._AdjDiscAmt;
				}
				set
				{
					this._AdjDiscAmt = value;
				}
			}
			#endregion
			#region AdjWhTaxAmt
			public new abstract class adjWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<adjWhTaxAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Withholding Tax Amount")]
			public override Decimal? AdjWhTaxAmt
			{
				get
				{
					return this._AdjWhTaxAmt;
				}
				set
				{
					this._AdjWhTaxAmt = value;
				}
			}
			#endregion
			#region CuryAdjgDiscAmt
			public new abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjgDiscAmt
			{
				get
				{
					return this._CuryAdjgDiscAmt;
				}
				set
				{
					this._CuryAdjgDiscAmt = value;
				}
			}
			#endregion
			#region CuryAdjgAmt
			public new abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjgAmt
			{
				get
				{
					return this._CuryAdjgAmt;
				}
				set
				{
					this._CuryAdjgAmt = value;
				}
			}
			#endregion
			#region CuryAdjgWhTaxAmt
			public new abstract class curyAdjgWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgWhTaxAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjgWhTaxAmt
			{
				get
				{
					return this._CuryAdjgWhTaxAmt;
				}
				set
				{
					this._CuryAdjgWhTaxAmt = value;
				}
			}
			#endregion
			#region RGOLAmt
			public new abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt>
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName="Realized Gain/Loss Amount")]
			public override Decimal? RGOLAmt
			{
				get
				{
					return this._RGOLAmt;
				}
				set
				{
					this._RGOLAmt = value;
				}
			}
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released>
			{
			}
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName="Released")]
			public override Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
			#region Voided
			public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided>
			{
			}
			[PXDBBool()]
			[PXDefault(false)]
			public override Boolean? Voided
			{
				get
				{
					return this._Voided;
				}
				set
				{
					this._Voided = value;
				}
			}
			#endregion
			#region VoidAdjNbr
			public new abstract class voidAdjNbr : PX.Data.BQL.BqlInt.Field<voidAdjNbr>
			{
			}
			[PXDBInt()]
			public override Int32? VoidAdjNbr
			{
				get
				{
					return this._VoidAdjNbr;
				}
				set
				{
					this._VoidAdjNbr = value;
				}
			}
			#endregion
			#region AdjdAPAcct
			public new abstract class adjdAPAcct : PX.Data.BQL.BqlInt.Field<adjdAPAcct>
			{
			}
			[Account()]
			[PXDBDefault(typeof(AP.APInvoice.aPAccountID))]
			public override Int32? AdjdAPAcct
			{
				get
				{
					return this._AdjdAPAcct;
				}
				set
				{
					this._AdjdAPAcct = value;
				}
			}
			#endregion
			#region AdjdAPSub
			public new abstract class adjdAPSub : PX.Data.BQL.BqlInt.Field<adjdAPSub>
			{
			}
			[SubAccount()]
			[PXDBDefault(typeof(AP.APInvoice.aPSubID))]
			public override Int32? AdjdAPSub
			{
				get
				{
					return this._AdjdAPSub;
				}
				set
				{
					this._AdjdAPSub = value;
				}
			}
			#endregion
			#region AdjdWhTaxAcctID
			public new abstract class adjdWhTaxAcctID : PX.Data.BQL.BqlInt.Field<adjdWhTaxAcctID>
			{
			}
			[Account()]
			[PXDefault(typeof(Search2<APTaxTran.accountID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			public override Int32? AdjdWhTaxAcctID
			{
				get
				{
					return this._AdjdWhTaxAcctID;
				}
				set
				{
					this._AdjdWhTaxAcctID = value;
				}
			}
			#endregion
			#region AdjdWhTaxSubID
			public new abstract class adjdWhTaxSubID : PX.Data.BQL.BqlInt.Field<adjdWhTaxSubID>
			{
			}
			[SubAccount()]
			[PXDefault(typeof(Search2<APTaxTran.subID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			public override Int32? AdjdWhTaxSubID
			{
				get
				{
					return this._AdjdWhTaxSubID;
				}
				set
				{
					this._AdjdWhTaxSubID = value;
				}
			}
			#endregion
			#region NoteID
			public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
			{
			}
			[PXNote()]
			public override Guid? NoteID
			{
				get
				{
					return this._NoteID;
				}
				set
				{
					this._NoteID = value;
				}
			}
			#endregion
			#region CuryDocBal
			public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal>
			{
			}
			[PXCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.docBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryDocBal
			{
				get
				{
					return this._CuryDocBal;
				}
				set
				{
					this._CuryDocBal = value;
				}
			}
			#endregion
			#region DocBal
			public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal>
			{
			}
			[PXDecimal(4)]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? DocBal
			{
				get
				{
					return this._DocBal;
				}
				set
				{
					this._DocBal = value;
				}
			}
			#endregion
			#region CuryDiscBal
			public new abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal>
			{
			}
			[PXCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.discBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryDiscBal
			{
				get
				{
					return this._CuryDiscBal;
				}
				set
				{
					this._CuryDiscBal = value;
				}
			}
			#endregion
			#region DiscBal
			public new abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal>
			{
			}
			[PXDecimal(4)]
			[PXUnboundDefault()]
			public override Decimal? DiscBal
			{
				get
				{
					return this._DiscBal;
				}
				set
				{
					this._DiscBal = value;
				}
			}
			#endregion
			#region CuryWhTaxBal
			public new abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal>
			{
			}
			[PXCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.whTaxBal), BaseCalc = false)]
			[PXUIField(DisplayName = "With. Tax Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryWhTaxBal
			{
				get
				{
					return this._CuryWhTaxBal;
				}
				set
				{
					this._CuryWhTaxBal = value;
				}
			}
			#endregion
			#region WhTaxBal
			public new abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal>
			{
			}
			[PXDecimal(4)]
			public override Decimal? WhTaxBal
			{
				get
				{
					return this._WhTaxBal;
				}
				set
				{
					this._WhTaxBal = value;
				}
			}
			#endregion
			#region VoidAppl
			public new abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl>
			{
			}
			[PXBool()]
			[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
			[PXDefault(false)]
			public override Boolean? VoidAppl
			{
				[PXDependsOnFields(typeof(adjgDocType))]
				get
				{
					return APPaymentType.VoidAppl(AdjgDocType);
				}
				set
				{
					if ((bool)value)
					{
						this._AdjgDocType = APPaymentType.GetVoidingAPDocType(AdjgDocType) ?? APDocType.VoidCheck;
						this.Voided = true;
					}
				}
			}
			#endregion
			#region ReverseGainLoss
			public new abstract class reverseGainLoss : PX.Data.BQL.BqlBool.Field<reverseGainLoss>
			{
			}
			[PXBool()]
			public override Boolean? ReverseGainLoss
			{
				[PXDependsOnFields(typeof(adjgDocType))]
				get
				{
					return (APPaymentType.DrCr(this._AdjgDocType) == DrCr.Debit);
				}
				set
				{
				}
			}
			#endregion
		}
		#endregion

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "Transactions", true) == 0)
			{
				if (values.Contains("tranType")) values["tranType"] = Document.Current.DocType;
				else values.Add("tranType", Document.Current.DocType);
				if (values.Contains("refNbr")) values["refNbr"] = Document.Current.RefNbr;
				else values.Add("refNbr", Document.Current.RefNbr);
			}
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		#region External Tax
		public virtual bool IsExternalTax(string taxZoneID)
			{
					return false;
		}

		public virtual APInvoice CalculateExternalTax(APInvoice invoice)
			{
				return invoice;
			}
		#endregion

		public virtual APInvoice CreatePPDDebitAdj(APPPDDebitAdjParameters filter, List<PendingPPDDebitAdjApp> list)

		{
			bool firstApp = true;
			APInvoice debitAdj = (APInvoice)Document.Cache.CreateInstance();

			foreach (PendingPPDDebitAdjApp doc in list)
			{
				if (firstApp)
				{
					firstApp = false;

					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, doc.InvCuryInfoID);
					info.CuryInfoID = null;
					info = currencyinfo.Insert(info);

					debitAdj.DocType = APDocType.DebitAdj;
					
                    debitAdj = PXCache<APInvoice>.CreateCopy(Document.Insert(debitAdj));

					debitAdj.VendorID = doc.VendorID;
					debitAdj.VendorLocationID = doc.InvVendorLocationID;
					debitAdj.CuryInfoID = info.CuryInfoID;
					debitAdj.CuryID = info.CuryID;
					Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
						.Select(this, doc.VendorID);
					debitAdj.DocDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(APSetup)], APSetup.Current, nameof(AP.APSetup.pPDDebitAdjustmentDescr), vendor?.LocaleName);
					debitAdj.BranchID = doc.AdjdBranchID;
				    debitAdj.DocDate = filter.GenerateOnePerVendor == true ? filter.DebitAdjDate : doc.AdjgDocDate;

				    string masterPeriodID = filter.GenerateOnePerVendor == true
				        ? FinPeriodRepository.GetByID(filter.FinPeriodID, PXAccess.GetParentOrganizationID(filter.BranchID)).MasterFinPeriodID
				        : doc.AdjgTranPeriodID;

				    FinPeriodIDAttribute.SetPeriodsByMaster<APInvoice.finPeriodID>(Document.Cache, debitAdj, masterPeriodID);

                    debitAdj.APAccountID = doc.AdjdAPAcct;
					debitAdj.APSubID = doc.AdjdAPSub;
					debitAdj.TaxZoneID = doc.InvTaxZoneID;
					debitAdj.PendingPPD = true;
					debitAdj.SuppliedByVendorLocationID = doc.InvVendorLocationID;
					debitAdj.Hold = false;
					debitAdj.TaxCalcMode = doc.InvTaxCalcMode;

					debitAdj = Document.Update(debitAdj);

					if (debitAdj.TaxCalcMode != doc.InvTaxCalcMode)
					{
						debitAdj.TaxCalcMode = doc.InvTaxCalcMode;
						debitAdj = Document.Update(debitAdj);
					}
				}

				AddTaxes(doc);
			}

			DiscountDetails.Select().RowCast<APInvoiceDiscountDetail>().ForEach(discountDetail => DiscountDetails.Cache.Delete(discountDetail));

			if (APSetup.Current.RequireControlTotal == true)
			{
				debitAdj.CuryOrigDocAmt = debitAdj.CuryDocBal;
				Document.Cache.Update(debitAdj);
			}

			if (APSetup.Current.RequireControlTaxTotal == true)
			{
				debitAdj.CuryTaxAmt = debitAdj.CuryTaxTotal;
				Document.Cache.Update(debitAdj);
			}

			Save.Press();

			return debitAdj;
		}


		public virtual void AddTaxes(PendingPPDDebitAdjApp doc)
		{
			APTaxTran taxMax = null;
			decimal? taxTotal = 0m;
			decimal? inclusiveTotal = 0m;
			decimal? discountedTaxableTotal = 0m;
			decimal? discountedPriceTotal = 0m;
			decimal cashDiscPercent = (decimal)(doc.CuryAdjdPPDAmt / doc.InvCuryOrigDocAmt);

			PXResultset<APTaxTran> taxes = PXSelectJoin<APTaxTran,
				InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>,
				Where<APTaxTran.module, Equal<BatchModule.moduleAP>,
					And<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>,
					And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>,
					And<Tax.taxApplyTermsDisc, Equal<CSTaxTermsDiscount.toPromtPayment>>>>>>.Select(this, doc.AdjdDocType, doc.AdjdRefNbr);

			//add taxes
			foreach (PXResult<APTaxTran, Tax> res in taxes)
			{
				Tax tax = res;
				APTaxTran taxTran = PXCache<APTaxTran>.CreateCopy(res);
				APTaxTran taxTranNew = Taxes.Search<APTaxTran.taxID>(taxTran.TaxID);

				if (taxTranNew == null)
				{
					taxTran.TranType = null;
					taxTran.RefNbr = null;
					taxTran.TaxPeriodID = null;
					taxTran.Released = false;
					taxTran.Voided = false;
				    taxTran.FinPeriodID = null;

                    taxTran.CuryInfoID = Document.Current.CuryInfoID;

					TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.NoCalc);
					taxTranNew = Taxes.Insert(taxTran);

					taxTranNew.CuryTaxableAmt = 0m;
					taxTranNew.CuryTaxAmt = 0m;
					taxTranNew.CuryTaxAmtSumm = 0m;
					taxTranNew.TaxRate = taxTran.TaxRate;
				}

				bool isTaxable = APPPDDebitAdjProcess.CalculateDiscountedTaxes(Taxes.Cache, taxTran, cashDiscPercent);
				decimal? curyTaxableAmt = taxTran.CuryTaxableAmt - taxTran.CuryDiscountedTaxableAmt;
				decimal? curyTaxAmt = taxTran.CuryTaxAmt - taxTran.CuryDiscountedPrice;

				decimal sign = tax.ReverseTax == true ? -1m : 1m;
				discountedPriceTotal += taxTran.CuryDiscountedPrice * sign;
				taxTranNew.CuryTaxableAmt += curyTaxableAmt;
				taxTranNew.CuryTaxAmt += curyTaxAmt;
				taxTranNew.CuryTaxAmtSumm += curyTaxAmt;

				TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualCalc);
				Taxes.Update(taxTranNew);

				if (isTaxable)
				{
					if (tax.ReverseTax != true)
					{
						discountedTaxableTotal += taxTran.CuryDiscountedTaxableAmt;
					}

					if (taxMax == null || taxTranNew.CuryTaxableAmt > taxMax.CuryTaxableAmt)
					{
						taxMax = taxTranNew;
					}
				}

				bool netGross = PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && (!netGross || Document.Current.TaxCalcMode != TaxCalculationMode.Net)
					|| netGross && Document.Current.TaxCalcMode == TaxCalculationMode.Gross)
				{
					inclusiveTotal += curyTaxAmt;
				}
				else
				{
					taxTotal += curyTaxAmt * sign;
				}
			}

			//adjust taxes according to parent APInvoice
			decimal? discountedInvTotal = doc.InvCuryOrigDocAmt - doc.InvCuryOrigDiscAmt;
			decimal? discountedDocTotal = discountedTaxableTotal + discountedPriceTotal;

			if (doc.InvCuryOrigDiscAmt == doc.CuryAdjdPPDAmt &&
				taxMax != null &&
				doc.InvCuryVatTaxableTotal + doc.InvCuryTaxTotal == doc.InvCuryOrigDocAmt &&
				discountedDocTotal != discountedInvTotal)
			{
				taxMax.CuryTaxableAmt += discountedDocTotal - discountedInvTotal;
				TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualCalc);
				Taxes.Update(taxMax);
			}

			//add document details
			AddPPDDebitAdjDetails(doc, taxTotal, inclusiveTotal, taxes);


		}

		private static readonly Dictionary<string, string> DocTypes = new APInvoiceType.AdjdListAttribute().ValueLabelDic;
        [InjectDependency]
	    protected IFinPeriodUtils _finPeriodUtils { get; set; }
	

	    public virtual void AddPPDDebitAdjDetails(PendingPPDDebitAdjApp doc, decimal? TaxTotal, decimal? InclusiveTotal, PXResultset<APTaxTran> taxes)
		{
			Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, doc.VendorID);
			APTran tranNew = Transactions.Insert();

			tranNew.BranchID = doc.AdjdBranchID;
			using (new PXLocaleScope(vendor.LocaleName))
			{
				tranNew.TranDesc = string.Format("{0} {1}, {2} {3}", PXMessages.LocalizeNoPrefix(DocTypes[doc.AdjdDocType]),
					doc.AdjdRefNbr, PXMessages.LocalizeNoPrefix(Messages.Check), doc.AdjgRefNbr);
			}
			tranNew.CuryLineAmt = doc.InvCuryDocBal - TaxTotal;
			tranNew.CuryTaxableAmt = doc.InvCuryDocBal - TaxTotal - InclusiveTotal;
			tranNew.CuryTaxAmt = TaxTotal + InclusiveTotal;
			tranNew.AccountID = vendor.DiscTakenAcctID;
			tranNew.SubID = vendor.DiscTakenSubID;
			tranNew.TaxCategoryID = null;
			tranNew.ManualDisc = true;
			tranNew.CuryDiscAmt = 0m;
			tranNew.DiscPct = 0m;
			tranNew.GroupDiscountRate = 1m;
			tranNew.DocumentDiscountRate = 1m;

			if (taxes.Count == 1)
			{
				APTaxTran taxTran = taxes[0];
				APTran aptran = PXSelectJoin<APTran,
					InnerJoin<APTax, On<APTax.tranType, Equal<APTran.tranType>,
						And<APTax.refNbr, Equal<APTran.refNbr>,
							And<APTax.lineNbr, Equal<APTran.lineNbr>>>>>,
					Where<APTax.tranType, Equal<Required<APTax.tranType>>,
						And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
							And<APTax.taxID, Equal<Required<APTax.taxID>>>>>,
					OrderBy<Asc<APTran.lineNbr>>>.SelectSingleBound(this, null, taxTran.TranType, taxTran.RefNbr, taxTran.TaxID);
				if (aptran != null)
				{
					tranNew.TaxCategoryID = aptran.TaxCategoryID;
				}
			}

			Transactions.Update(tranNew);
		}

		public virtual POAccrualSet GetUsedPOAccrualSet()
			=> new POAccrualSet(
				this.Transactions.Select().RowCast<APTran>(),
				this.Document.Current?.DocType == APDocType.Prepayment
					? (IEqualityComparer<APTran>)new POLineComparer()
					: new POAccrualComparer());
	}
		}
