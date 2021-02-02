using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Api;
using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.PM;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Api.ContractBased.Models;

namespace PX.Objects.AR.GraphExtensions
{
	public class GenerateIntercompanyBillExtension : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.interBranch>();

		public PXFilter<GenerateBillParameters> generateBillParameters;

		public override void Initialize()
		{
			base.Initialize();
			if (!typeof(SO.SOInvoiceEntry).IsAssignableFrom(Base.GetType()))
			{
				Base.ActionsMenuItem.AddMenuAction(generateOrViewIntercompanyBill);
			}
		}

		#region Actions
		public PXAction<GenerateBillParameters> generateOrViewIntercompanyBill;

		[PXUIField(DisplayName = "Generate/View AP Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable GenerateOrViewIntercompanyBill(PXAdapter adapter)
		{
			if (!generateOrViewIntercompanyBill.GetEnabled()) return adapter.Get();

			APSetup apsetup = SelectFrom<APSetup>.View.Select(Base);

			if(apsetup?.MigrationMode == true)
			{
				throw new PXException(AP.Messages.MigrationModeIsActivated);
			}

			Base.Save.Press();
			var parameters = generateBillParameters.Current;
			parameters.MassProcess = generateBillParameters.Current.MassProcess == true || adapter.MassProcess;

			ARInvoice ardoc = Base.CurrentDocument.Current;
			APInvoice apdoc = PXSelect<APInvoice, Where<APInvoice.intercompanyInvoiceNoteID, Equal<Required<APInvoice.intercompanyInvoiceNoteID>>>>
										.SelectSingleBound(Base, null, ardoc.NoteID);

			APInvoiceEntry apInvoiceEntryGraph = PXGraph.CreateInstance<APInvoiceEntry>();

			if (apdoc!=null)
			{
				apInvoiceEntryGraph.Document.Current = apdoc;
			}
			else 
			{
				if (parameters.MassProcess != true  && !Base.IsContractBasedAPI && ardoc.ProjectID > 0)
					if (generateBillParameters.View.Ask(AR.Messages.CopyProjectInformationToAPDocument, MessageButtons.YesNo) == WebDialogResult.Yes)
						parameters.CopyProjectInformation = true;

				apdoc = GenerateIntercompanyBill(apInvoiceEntryGraph, ardoc); // apInvoiceEntryGraph has created apdoc
			}
			if (parameters.MassProcess == true)
			{
				apInvoiceEntryGraph.Save.Press();
			}
			else 
			{
				PXRedirectHelper.TryRedirect(apInvoiceEntryGraph, PXRedirectHelper.WindowMode.Same);
			}
			return adapter.Get();
		}

		public virtual APInvoice GenerateIntercompanyBill(APInvoiceEntry apInvoiceEntryGraph, ARInvoice arInvoice, GenerateBillParameters parameters=null)
		{
			/* The user can specify the following parameter to manage the process of generation:
					* If the Create AP Documents in Specific Period checkbox is activated, then new AP documents will be created with the specified Financial Period.
					* If the Create AP Documents on Hold checkbox is checked, new AP documents are created in On Hold status; otherwise - in Balance or Pending Approval status depending on approval configuration in AP.
					* If the Copy Project Information To AP Document checkbox is activated, then the project data (project code, cost code, task id) will be copied from the AR document to the AP document. 
			*/
			apInvoiceEntryGraph.Clear();
			bool? storedRequireControlTotal = apInvoiceEntryGraph.APSetup.Current.RequireControlTotal;
			apInvoiceEntryGraph.APSetup.Current.RequireControlTotal = false;

			APInvoice apInvoice = new APInvoice();
			PXCache cacheAPInvoice = apInvoiceEntryGraph.Document.Cache;
			if(parameters==null)
				parameters = generateBillParameters.Current;

			#region Summary area
			#region Type
			switch (arInvoice.DocType)  // Type
			{
				case ARDocType.Invoice: // Bill in case of Invoice
					apInvoice.DocType = APDocType.Invoice;
					break;
				case ARDocType.DebitMemo: // Credit Adj. in case of Debit Memo
					apInvoice.DocType = APDocType.CreditAdj;
					break;
				case ARDocType.CreditMemo: // Debit Adj. in case of Credit Memo
					apInvoice.DocType = APDocType.DebitAdj;
					break;
				default:
					throw new NotImplementedException();
			}
			apInvoice = apInvoiceEntryGraph.Document.Insert(apInvoice);
			#endregion
			#region Vendor
			// AR Document's originating branch. If the originating branch is not extended to a Vendor, the error message should be shown: The originating branch is not extended to a vendor
			Vendor vendor = PXSelectJoin<Vendor,
									InnerJoin<Branch, On<Vendor.bAccountID, Equal<Branch.bAccountID>>>,
									Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
									.SelectSingleBound(Base, null, arInvoice.BranchID);

			if (vendor != null) 
			{
				cacheAPInvoice.SetValueExt<APInvoice.vendorID>(apInvoice, vendor.BAccountID);
			}

			if (apInvoice.VendorID == null)
			{
				Branch originatingBranch = SelectFrom<Branch>
					.Where<Branch.branchID.IsEqual<@P.AsInt>>
					.View
					.Select(Base, arInvoice.BranchID);
				throw new PXException(Messages.BranchIsNotExtendedToVendor, originatingBranch.BranchCD.Trim());
			}
			#endregion
			#region Hold checkbox
			if (parameters.MassProcess == true) // For the Generate Inter-Company AP Documents (AP503500)  form:
			{
				cacheAPInvoice.SetValueExt<APInvoice.hold>(apInvoice, parameters?.CreateOnHold == true); // * Activated if the Create AP Documents on Hold checkbox is checked
			}
			else  // For the Invoices and Memos (AR301000) form: Use standard logic of AP Documents creation depending on the Hold Documents on Entry parameter from the Accounts Payable Preferences (AP101000) form
			{
				cacheAPInvoice.SetValueExt<APInvoice.hold>(apInvoice, apInvoiceEntryGraph.APSetup.Current.HoldEntry ?? false);
			}
			#endregion
			BranchAlias branch = PXSelect<BranchAlias, Where<BranchAlias.bAccountID, Equal<Required<BranchAlias.bAccountID>>>>
				.SelectSingleBound(Base, null, arInvoice.CustomerID);
			cacheAPInvoice.SetValueExt<APInvoice.branchID>(apInvoice, branch.BranchID); // Branch AR Document's Customer
			cacheAPInvoice.SetValueExt<APInvoice.docDate>(apInvoice, arInvoice.DocDate);
			#region Post Period
			if (parameters.MassProcess == true
				&& parameters?.FinPeriodID != null)// For the Generate Inter-Company AP Documents(AP503500)  form:
			{
				cacheAPInvoice.SetValue<APInvoice.finPeriodID>(apInvoice, parameters?.FinPeriodID);
			}
			else
			{
				FinPeriodIDAttribute.SetPeriodsByMaster<APInvoice.finPeriodID>(apInvoiceEntryGraph.CurrentDocument.Cache, apInvoice, arInvoice.TranPeriodID);
			}
			cacheAPInvoice.Update(apInvoice);
			#endregion

			cacheAPInvoice.SetValueExt<APInvoice.invoiceNbr>(apInvoice, arInvoice.RefNbr); // AR Document's Reference Number

			#region Currency, Currency Rate Type, Currency Exchange Rate
			// AR Document's Currency
			// Currency Rate Type from the AR Document
			// The exchange rate from the AR Document

			CurrencyInfo arCuryInfo = Base.currencyinfo.Current = Base.currencyinfo.Select();
			apInvoiceEntryGraph.currencyinfo.Current = apInvoiceEntryGraph.currencyinfo.Select();
			CurrencyInfo apCuryInfo = PXCache<CurrencyInfo>.CreateCopy(arCuryInfo);			
			apCuryInfo.CuryInfoID = apInvoiceEntryGraph.currencyinfo.Current.CuryInfoID; // set CuryInfoID from new AP Document
			apInvoiceEntryGraph.currencyinfo.Update(apCuryInfo);
			cacheAPInvoice.SetValueExt<APInvoice.curyID>(apInvoice, arInvoice.CuryID);
			cacheAPInvoice.SetValueExt<APInvoice.curyInfoID>(apInvoice, apCuryInfo.CuryInfoID);
			#endregion
			//AR Document's Terms. If the specified Credit Terms are not visible for Vendors, ignore this restriction and copy the terms.
			cacheAPInvoice.SetValue<APInvoice.termsID>(apInvoice, arInvoice.TermsID);
			apInvoice = (APInvoice)cacheAPInvoice.Update(apInvoice); // throw Exception if Term of AR Document is absent in AP Document 
			cacheAPInvoice.SetValueExt<APInvoice.dueDate>(apInvoice, arInvoice.DueDate); // AR Document's Due Date
			cacheAPInvoice.SetValueExt<APInvoice.discDate>(apInvoice, arInvoice.DiscDate); // AR Document's Cash Discount Date
			cacheAPInvoice.SetValueExt<APInvoice.paymentsByLinesAllowed>(apInvoice, apInvoiceEntryGraph.vendor.Current.PaymentsByLinesAllowed); // Vendor's Pay by Line value
			cacheAPInvoice.SetValueExt<APInvoice.docDesc>(apInvoice, arInvoice.DocDesc); // AR Document's Description

			PXNoteAttribute.CopyNoteAndFiles(Base.Caches<ARInvoice>(), arInvoice, cacheAPInvoice, apInvoice);
			#endregion Summary area

			#region Financial Details tab
			cacheAPInvoice.SetValueExt<APInvoice.taxCalcMode>(apInvoice, arInvoice.TaxCalcMode); // AR Documen's Calculation Mode
			#endregion

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>
				.SelectSingleBound(Base, null, arInvoice.ProjectID);
			int nonProjectID = (int)ProjectDefaultAttribute.NonProject(); // projectID of Non-Project Code is 0
			apInvoice.ProjectID = parameters?.CopyProjectInformation == true
				? arInvoice.ProjectID
				: nonProjectID;

			#region Documents Details tab: each Bill Line corresponds to one Line from the AR Invoice
			PMSetup pmSetup = PXSelect<PMSetup>.Select(Base);
			PXCache cacheARTran = Base.Transactions.Cache;
			PXCache cacheAPTran = apInvoiceEntryGraph.Transactions.Cache;

			foreach (ARTran arTran in Base.Transactions.Select())
			{
				void APTranProjectIDFieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
				{
					e.NewValue = parameters?.CopyProjectInformation == true
						? arInvoice.ProjectID
						: nonProjectID;
					e.Cancel = true;
				}

				void APTranTaskIDFieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
				{
					if (parameters?.CopyProjectInformation == true)
					{
						e.NewValue = arTran.TaskID;
					}
					e.Cancel = true;
				}

				void APTranCostCodeIDFieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
				{
					if (parameters?.CopyProjectInformation == true)
					{
						e.NewValue = arTran.CostCodeID;
					}
					e.Cancel = true;
				}

				try
				{
					apInvoiceEntryGraph.FieldDefaulting.AddHandler<APTran.projectID>(APTranProjectIDFieldDefaulting);
					apInvoiceEntryGraph.FieldDefaulting.AddHandler<APTran.taskID>(APTranTaskIDFieldDefaulting);
					apInvoiceEntryGraph.FieldDefaulting.AddHandler<APTran.costCodeID>(APTranCostCodeIDFieldDefaulting);

					APTran apTran = apInvoiceEntryGraph.Transactions.Insert(new APTran());

					cacheAPTran.SetValueExt<APTran.inventoryID>(apTran, arTran.InventoryID);  // AR Document Line's Inventory ID
					cacheAPTran.SetValueExt<APTran.tranDesc>(apTran, arTran.TranDesc); // AR Document Line's Transaction Description
					cacheAPTran.SetValueExt<APTran.qty>(apTran, arTran.Qty); // AR Document Line's Quantity
					apTran.UOM = arTran.UOM;
					cacheAPTran.SetValueExt<APTran.curyUnitCost>(apTran, arTran.CuryUnitPrice); // AR Document Line's Unit Price
					cacheAPTran.SetValueExt<APTran.curyLineAmt>(apTran, arTran.CuryExtPrice); // AR Document Line's Ext. Price
					cacheAPTran.SetValueExt<APTran.curyDiscAmt>(apTran, arTran.CuryDiscAmt); // AR Document Line's Discount Amount
					cacheAPTran.SetValueExt<APTran.discPct>(apTran, arTran.DiscPct); // AR Document Line's Discount Percent
					cacheAPTran.SetValueExt<APTran.manualDisc>(apTran, true); // AR Document Line's Tax Category Activated
					cacheAPTran.SetValueExt<APTran.taxCategoryID>(apTran, arTran.TaxCategoryID); // AR Document Line's Tax Category

					apTran = apInvoiceEntryGraph.Transactions.Update(apTran);

					if (parameters?.CopyProjectInformation == true && project.VisibleInAP != true)
					{
						PXUIFieldAttribute.SetWarning<APTran.projectID>(cacheAPTran, apTran, PXMessages.LocalizeFormatNoPrefix(AR.Messages.ProjectCannotBeCopiedBecauseProjectVisibilitySettings, project.ContractCD));
						PXUIFieldAttribute.SetWarning<ARTran.projectID>(cacheARTran, arTran, PXMessages.LocalizeFormatNoPrefix(AR.Messages.ProjectCannotBeCopiedBecauseProjectVisibilitySettings, project.ContractCD));
					}
				}
				finally 
				{
					apInvoiceEntryGraph.FieldDefaulting.RemoveHandler<APTran.projectID>(APTranProjectIDFieldDefaulting);
					apInvoiceEntryGraph.FieldDefaulting.RemoveHandler<APTran.taskID>(APTranTaskIDFieldDefaulting);
					apInvoiceEntryGraph.FieldDefaulting.RemoveHandler<APTran.costCodeID>(APTranCostCodeIDFieldDefaulting);
				}
			}
			#endregion

			#region Discount Details tab: lines are created if the Vendor Discounts feature is activated.

			PXCache cacheAPInvoiceDiscountDetail = apInvoiceEntryGraph.DiscountDetails.Cache;

			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())   // If the Customer Discounts feature is activated, then each Discount Line in AP Document corresponds to one Discount Line from the AR Invoice:
			{
				foreach (ARInvoiceDiscountDetail arDiscount in Base.ARDiscountDetails.Select())
				{
					APInvoiceDiscountDetail apDiscount = apInvoiceEntryGraph.DiscountDetails.Insert(new APInvoiceDiscountDetail());

					#region Manual Discount checkbox
					cacheAPInvoiceDiscountDetail.SetValueExt<ARInvoiceDiscountDetail.isManual>(apDiscount, true); // Activated 
					#endregion
					#region Discount Amount
					cacheAPInvoiceDiscountDetail.SetValueExt<ARInvoiceDiscountDetail.curyDiscountAmt>(apDiscount, arDiscount.CuryDiscountAmt); // Discount Amount from AR Document's Discount Line
					#endregion
					#region Description
					cacheAPInvoiceDiscountDetail.SetValueExt<ARInvoiceDiscountDetail.description>(apDiscount, arDiscount.Description); // Description from AR Document's Discount Line
					#endregion

					apInvoiceEntryGraph.DiscountDetails.Update(apDiscount);
				}
			}
			else // If the Customer Discounts feature is not activated, then one Discount Line is created in the AP Document:
			{
				APInvoiceDiscountDetail apDiscount = apInvoiceEntryGraph.DiscountDetails.Insert(new APInvoiceDiscountDetail());
				cacheAPInvoiceDiscountDetail.SetValueExt<ARInvoiceDiscountDetail.isManual>(apDiscount, true);
				cacheAPInvoiceDiscountDetail.SetValueExt<ARInvoiceDiscountDetail.curyDiscountAmt>(apDiscount, arInvoice.CuryDiscTot); // Discount Total from the AR Document
				apInvoiceEntryGraph.DiscountDetails.Update(apDiscount);
			}
			#endregion

			cacheAPInvoice.SetValue<APInvoice.intercompanyInvoiceNoteID>(apInvoice, arInvoice.NoteID);
			if(arInvoice.CuryOrigDocAmt == apInvoice.CuryOrigDocAmt)
			{
				cacheAPInvoice.SetValue<APInvoice.curyOrigDiscAmt>(apInvoice, arInvoice.CuryOrigDiscAmt);
			}

			apInvoiceEntryGraph.APSetup.Current.RequireControlTotal = storedRequireControlTotal;
			apInvoice = apInvoiceEntryGraph.Document.Update(apInvoice);

			return apInvoice;
		}

		public ProcessingResult CheckGeneratedAPDocument(ARInvoice arInvoice, APInvoice apInvoice)
		{
			ProcessingResult result = ProcessingResult.CreateSuccess(arInvoice);

			if (Base.Transactions
				.Select()
				.RowCast<ARTran>()
				.Any(arTran => !string.IsNullOrEmpty(arTran.DeferredCode)))
			{
				result.AddMessage(PXErrorLevel.RowWarning, AP.Messages.CannotCopyDeferralCodeFromARDocument);
			}
			if (apInvoice.CuryOrigDocAmt != arInvoice.CuryOrigDocAmt)
			{
				result.AddMessage(PXErrorLevel.RowWarning, AP.Messages.IntercompanyRelatedDocumentTotalIsDiffer);
			}
			if (apInvoice.CuryTaxTotal != arInvoice.CuryTaxTotal)
			{
				result.AddMessage(PXErrorLevel.RowWarning, AP.Messages.IntercompanyRelatedTaxTotalIsDiffer);
			}

			return result;
		}
		#endregion

		#region Events
		public virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is ARInvoice arInvoice)) return;

			#region Get Document State
			ARInvoiceState state = new ARInvoiceState
			{
				IsDocumentReleased = arInvoice.Released == true,
				IsDocumentInvoice = arInvoice.DocType == ARDocType.Invoice,
				IsDocumentCreditMemo = arInvoice.DocType == ARDocType.CreditMemo,
				IsDocumentDebitMemo = arInvoice.DocType == ARDocType.DebitMemo,
				IsMigratedDocument = arInvoice.IsMigratedRecord == true,
				RetainageApply = arInvoice.RetainageApply == true,
				IsRetainageDocument = arInvoice.IsRetainageDocument == true
			};
			#endregion

			#region generateOrViewAPDoc Enable
			bool isHiddenInIntercompanySalesVisible =
					Base.customer.Current?.IsBranch == true //* The document's customer is extended from a company/branch
					&& (state.IsDocumentInvoice || state.IsDocumentCreditMemo || state.IsDocumentDebitMemo) //* The document's type = Invoice or Credit Memo or Debit Memo.
					&& !state.IsMigratedDocument  //* The document is not migrated (ARRegister.IsMigratedRecord = 0)
					&& (!state.IsRetainageDocument && !state.RetainageApply) //* The document is not a retainage invoice  and not an invoice with retainage (ARRegister.IsRetainageDocument = 0 AND ARRegister.RetainageApply = 0)
					&& arInvoice.MasterRefNbr == null && arInvoice.InstallmentNbr == null //* The document is not a child document for AR documents with multiple installment credit terms(ARInvoice.MasterRefNbr = NULL and ARInvoice.InstallmentNbr = NULL)
					&& !(state.IsDocumentCreditMemo && arInvoice.PendingPPD == true); //* The document is not a VAT Credit Memo(ARRegister.DocType = CRM AND ARRegister.HasPPDTaxes = 1)

			bool generateOrViewAPDocEnable =
				isHiddenInIntercompanySalesVisible
				&& state.IsDocumentReleased
				&& arInvoice.IsHiddenInIntercompanySales != true;

			APInvoice relatedBill = null;
			if(generateOrViewAPDocEnable != true)
			{
				relatedBill = SelectFrom<APInvoice>
					.Where<APInvoice.intercompanyInvoiceNoteID.IsEqual<@P.AsGuid>>
					.View
					.SelectSingleBound(Base, null, arInvoice.NoteID);
			}

			generateOrViewIntercompanyBill.SetEnabled(generateOrViewAPDocEnable || relatedBill != null);
			PXUIFieldAttribute.SetVisible<ARInvoice.isHiddenInIntercompanySales>(cache, arInvoice, isHiddenInIntercompanySalesVisible);
			#endregion
		}
		#endregion

		/// <summary>
		/// The user can specify the following parameter to manage the process of generation:
		/// * If the Create AP Documents in Specific Period checkbox is activated, then new AP documents will be created with the specified Financial Period.
		/// * If the Create AP Documents on Hold checkbox is checked, new AP documents are created in On Hold status; otherwise - in Balance or Pending Approval status depending on approval configuration in AP.
		/// * If the Copy Project Information To AP Document checkbox is activated, then the project data (project code, cost code, task id) will be copied from the AR document to the AP document. 
		/// </summary>
		[Serializable]
		[PXHidden]
		public class GenerateBillParameters : IBqlTable
		{
			#region FinPeriod
			/// <summary>
			/// If the Create AP Documents in Specific Period checkbox is activated, then new AP documents will be created with the specified Financial Period.
			/// </summary>
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			[PXString(6)]
			public virtual string FinPeriodID { get; set; }
			#endregion

			#region CreateOnHold
			/// <summary>
			/// If the Create AP Documents on Hold checkbox is checked, new AP documents are created in On Hold status; otherwise - in Balance or Pending Approval status depending on approval configuration in AP.
			/// </summary>
			public abstract class createOnHold : PX.Data.BQL.BqlBool.Field<createOnHold> { }
			[PXBool()]
			[PXUnboundDefault(true)]
			public virtual Boolean? CreateOnHold { get; set; }
			#endregion

			#region CopyProjectInformation
			/// <summary>
			/// If the Copy Project Information To AP Document checkbox is activated, then the project data (project code, cost code, task id) will be copied from the AR document to the AP document. 
			/// </summary>
			public abstract class copyProjectInformationto : PX.Data.BQL.BqlBool.Field<copyProjectInformationto> { }
			[PXBool()]
			[PXUnboundDefault(false)]
			public virtual Boolean? CopyProjectInformation { get; set; }
			#endregion

			#region MassProcess
			/// <summary>
			/// If the Copy Project Information To AP Document checkbox is activated, then the project data (project code, cost code, task id) will be copied from the AR document to the AP document. 
			/// </summary>
			public abstract class massProcess : PX.Data.BQL.BqlBool.Field<massProcess> { }
			[PXBool()]
			[PXUnboundDefault(false)]
			public virtual Boolean? MassProcess { get; set; }
			#endregion
			

		}
	}
}
