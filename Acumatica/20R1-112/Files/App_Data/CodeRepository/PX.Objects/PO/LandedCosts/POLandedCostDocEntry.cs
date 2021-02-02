using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.SM;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO.LandedCosts;
using PX.Objects.TX;
using Branch = PX.Objects.GL.Branch;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.PO
{
	[Serializable]
	public class POLandedCostDocEntry : PXGraph<POLandedCostDocEntry, POLandedCostDoc>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public POLandedCostDocEntry()
		{
			this.poReceiptSelection.Cache.AllowInsert = false;
			this.poReceiptSelection.Cache.AllowDelete = false;
			this.poReceiptLinesSelection.Cache.AllowInsert = false;
			this.poReceiptLinesSelection.Cache.AllowDelete = false;

			bool isInvoiceNbrRequired = (bool) apsetup.Current.RequireVendorRef;

			PXDefaultAttribute.SetPersistingCheck<POLandedCostDoc.vendorRefNbr>(this.Document.Cache, null,
				isInvoiceNbrRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<POReceipt.invoiceNbr>(this.Document.Cache, isInvoiceNbrRequired);

			PXUIFieldAttribute.SetEnabled(poReceiptSelection.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<POReceipt.selected>(poReceiptSelection.Cache, null, true);
			PXUIFieldAttribute.SetEnabled(poReceiptLinesSelection.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<POReceiptLineAdd.selected>(poReceiptLinesSelection.Cache, null, true);

			TaxAttribute.SetTaxCalc<POLandedCostDetail.taxCategoryID>(Details.Cache, null, TaxCalc.ManualLineCalc);

			PXAction action = Actions["action"];
			if (action != null)
			{
				action.MenuAutoOpen = true;
				action.AddMenuAction(createAPInvoice);
			}
		}

		public virtual LandedCostAPBillFactory GetApBillFactory()
		{
			return new LandedCostAPBillFactory(this);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		public virtual LandedCostINAdjustmentFactory GetInAdjustmentFactory()
		{
			return GetInAdjustmentFactory(this);
		}

		public virtual LandedCostINAdjustmentFactory GetInAdjustmentFactory(PXGraph graph)
		{
			return new LandedCostINAdjustmentFactory(graph);
		}

		#region Selectors

		[PXCopyPasteHiddenFields(typeof(POLandedCostDoc.hold))]
		[PXViewName(Messages.POLandedCostDoc)] public PXSelect<POLandedCostDoc> Document;

		public PXSetup<POSetup> posetup;
		public CMSetupSelect cmsetup;
		public PXSetup<APSetup> apsetup;

		public PXSetup<GL.Branch, Where<Branch.bAccountID, Equal<Optional<POLandedCostDoc.vendorID>>>> company;

		[PXViewName(AP.Messages.Vendor)]
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<POLandedCostDoc.vendorID>>>> vendor;

		[PXViewName(AP.Messages.VendorClass)]
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;

		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<POLandedCostDoc.taxZoneID>>>> taxzone;

		[PXViewName(AP.Messages.VendorLocation)]
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>, And<Location.locationID, Equal<Optional<POLandedCostDoc.vendorLocationID>>>>> location;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<POLandedCostDoc.curyInfoID>>>> currencyinfo;

		public PXSelect<POLandedCostDoc, Where<POLandedCostDoc.docType, Equal<Current<POLandedCostDoc.docType>>, And<POLandedCostDoc.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>> CurrentDocument;

		public PXSelect<POLandedCostReceipt, Where<POLandedCostReceipt.lCDocType, Equal<Current<POLandedCostDoc.docType>>, And<POLandedCostReceipt.lCRefNbr, Equal<Current<POLandedCostDoc.refNbr>>>>> Receipts;

		[PXCopyPasteHiddenView]
		public PXSelect<POLandedCostReceiptLine, 
				Where<
					POLandedCostReceiptLine.docType, Equal<Current<POLandedCostDoc.docType>>,
					And<POLandedCostReceiptLine.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>,
				OrderBy<Asc<POLandedCostReceiptLine.lineNbr>>>
			ReceiptLines;

		public PXSelect<
				POLandedCostDetail,
				Where<
					POLandedCostDetail.docType, Equal<Current<POLandedCostDoc.docType>>,
					And<POLandedCostDetail.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>,
				OrderBy<Asc<POLandedCostDetail.lineNbr>>>
			Details;

		[PXCopyPasteHiddenView]
		public PXSelect<
				POLandedCostSplit,
				Where<
					POLandedCostSplit.docType, Equal<Current<POLandedCostDoc.docType>>,
					And<POLandedCostSplit.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>>
			Splits;

		[PXCopyPasteHiddenView]
		public PXSelect<POLandedCostTax, Where<POLandedCostTax.docType, Equal<Current<POLandedCostDoc.docType>>, 
			And<POLandedCostTax.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>, OrderBy<Asc<POLandedCostTax.refNbr, Asc<POLandedCostTax.taxID>>>> Tax_Rows;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<POLandedCostTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<POLandedCostTaxTran.taxID>>>,
			Where<POLandedCostTaxTran.docType, Equal<Current<POLandedCostDoc.docType>>,
				And<POLandedCostTaxTran.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>> Taxes;

		public PXFilter<POReceiptFilter> filter;

		[PXCopyPasteHiddenView]
		[PXReadOnlyView]
		public PXSelectOrderBy<POReceipt, OrderBy<Desc<POReceipt.receiptDate, Desc<POReceipt.lastModifiedDateTime>>>> poReceiptSelection;

		[PXCopyPasteHiddenView]
		public PXSelect<POReceiptLineAdd, 
			Where<POReceiptLineAdd.receiptType, Equal<Current<POReceiptFilter.receiptType>>, 
				And<POReceiptLineAdd.lineType, NotEqual<POLineType.service>, 
				And<POReceiptLineAdd.lineType, NotEqual<POLineType.freight>, 
				And2<Where<Current<POReceiptFilter.receiptNbr>, IsNull, Or<POReceiptLineAdd.receiptNbr, Equal<Current<POReceiptFilter.receiptNbr>>>>, 
				And2<Where<Current<POReceiptFilter.orderType>, IsNull, Or<POReceiptLineAdd.pOType, Equal<Current<POReceiptFilter.orderType>>>>, 
				And2<Where<Current<POReceiptFilter.orderNbr>, IsNull, Or<POReceiptLineAdd.pONbr, Equal<Current<POReceiptFilter.orderNbr>>>>, 
				And2<Where<Current<POReceiptFilter.inventoryID>, IsNull, Or<POReceiptLineAdd.inventoryID, Equal<Current<POReceiptFilter.inventoryID>>>>, 
				And2<Where<Current<POReceiptFilter.vendorID>, IsNull, Or<POReceiptLineAdd.vendorID, Equal<Current<POReceiptFilter.vendorID>>>>, 
				And<Where<POReceiptLineAdd.released, Equal<True>>>>>>>>>>>
			,OrderBy<Desc<POReceiptLineAdd.receiptDate, Desc<POReceiptLineAdd.receiptLastModifiedDateTime, Desc<POReceiptLineAdd.receiptNbr, Desc<POReceiptLineAdd.lineNbr>>>>>>
			poReceiptLinesSelection;

		#endregion

		#region Events

		#region Currency Info

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Details.Cache);
				if (vendor.Current != null && !(bool) vendor.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo row = (CurrencyInfo) e.Row;
				POLandedCostDoc doc = Document.Current;
				if (row != null && doc != null && row.CuryInfoID == doc.CuryInfoID)
				{
					if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
					{
						e.NewValue = vendor.Current.CuryID;
						e.Cancel = true;
					}
				}
				else
				{
					/*LandedCostTran lcTran = this.landedCostTrans.Current;
					if (row != null && lcTran != null && row.CuryInfoID == lcTran.CuryInfoID)
					{
						e.NewValue = row.CuryID;
						e.Cancel = true;
					}*/
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
				else
				{
					e.NewValue = cmsetup.Current.APRateTypeDflt;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((POLandedCostDoc) Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		#endregion

		protected virtual void _(Events.RowSelected<POLandedCostDoc> e)
		{
			if (e.Row == null)
				return;

			var isReleased = e.Row.Released == true;
			bool hasVendor = (e.Row.VendorID != null && e.Row.VendorLocationID != null);

			bool requireControlTotal = posetup.Current.RequireLandedCostsControlTotal == true;
			bool curyenabled = (vendor.Current == null || vendor.Current.AllowOverrideCury == true) && !isReleased;

			bool hasTransactions = (this.Details.SelectWindowed(0, 1).Count > 0);

			e.Cache.AllowDelete = !isReleased;

			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.docType>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.hold>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.docDate>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.finPeriodID>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.vendorID>(e.Cache, e.Row, !isReleased && !hasTransactions);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.vendorLocationID>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.createBill>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.vendorRefNbr>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.curyControlTotal>(e.Cache, e.Row, !isReleased);

			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.branchID>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.termsID>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.billDate>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.dueDate>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.discDate>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.curyDiscAmt>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.taxZoneID>(e.Cache, e.Row, !isReleased);

			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.workgroupID>(e.Cache, e.Row, !isReleased);
			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.ownerID>(e.Cache, e.Row, !isReleased);

			PXUIFieldAttribute.SetEnabled<POLandedCostDoc.curyID>(e.Cache, e.Row, curyenabled);

			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<POLandedCostDoc.curyID>(e.Cache, e.Row, mcFeatureInstalled);

			ReceiptLines.Cache.AllowDelete = !isReleased && hasVendor;
			ReceiptLines.Cache.AllowUpdate = !isReleased && hasVendor;
			ReceiptLines.Cache.AllowInsert = !isReleased && hasVendor;

			addPOReceipt.SetEnabled(!isReleased && hasVendor);
			addPOReceiptLine.SetEnabled(!isReleased && hasVendor);

			Details.Cache.AllowDelete = !isReleased && hasVendor;
			Details.Cache.AllowUpdate = !isReleased && hasVendor;
			Details.Cache.AllowInsert = !isReleased && hasVendor;

			Taxes.Cache.AllowDelete = !isReleased && hasVendor;
			Taxes.Cache.AllowUpdate = !isReleased && hasVendor;
			Taxes.Cache.AllowInsert = !isReleased && hasVendor;

			PXUIFieldAttribute.SetVisible<POLandedCostDoc.curyControlTotal>(e.Cache, null, requireControlTotal);
			PXUIFieldAttribute.SetRequired<POLandedCostDoc.curyControlTotal>(e.Cache, requireControlTotal);

			release.SetEnabled(e.Row.Hold != true && e.Row.Released != true);

			var autoCreateInvoice = e.Row.CreateBill.Value;
			bool isInvoiceNbrRequired = autoCreateInvoice && (bool) apsetup.Current.RequireVendorRef;

			PXDefaultAttribute.SetPersistingCheck<POLandedCostDoc.vendorRefNbr>(e.Cache, e.Row,
				isInvoiceNbrRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<POLandedCostDoc.vendorRefNbr>(e.Cache, isInvoiceNbrRequired);

			var emptyDetails = Details.Select().RowCast<POLandedCostDetail>()
				.Where(t => String.IsNullOrEmpty(t.APDocType) && String.IsNullOrEmpty(t.APRefNbr)).ToList();

			createAPInvoice.SetEnabled(isReleased && emptyDetails.Any());
		}

		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(POLandedCostDoc.taxZoneID))]
		[PXUIField(DisplayName = "Vendor Tax Zone", Enabled = false)]
		protected virtual void POLandedCostTaxTran_TaxZoneID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void _(Events.FieldSelecting<POLandedCostReceiptLine.pOReceiptBaseCuryID> e)
		{
			if (currencyinfo.Current != null)
				e.ReturnValue = currencyinfo.Current.BaseCuryID;
			else if (Document.Current != null)
			{
				CurrencyInfo ci = currencyinfo.SelectWindowed(0, 1);
				if (ci != null)
					e.ReturnValue = ci.BaseCuryID;
			}
		}

		protected virtual void _(Events.RowSelected<POLandedCostDetail> e)
		{
			if (e.Row == null)
				return;

			if (!String.IsNullOrEmpty(e.Row.APRefNbr))
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
			}
		}

		protected virtual void _(Events.RowDeleting<POLandedCostDetail> e)
		{
			if (e.Row == null)
				return;

			if (!String.IsNullOrEmpty(e.Row.APRefNbr))
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.LandedCostsDetailCannotDelete);
			}
		}

		protected virtual void POLandedCostDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			TaxAttribute.Calculate<POLandedCostDetail.taxCategoryID>(sender, e);
		}

		protected virtual void _(Events.RowUpdated<POLandedCostDoc> e)
		{
			if (posetup.Current.RequireLandedCostsControlTotal != true)
			{
				e.Cache.SetValue<POLandedCostDoc.curyControlTotal>(e.Row, e.Row.CuryDocTotal);
			}
			else
			{
				if (e.Row.Hold != true)
				{
					if (e.Row.CuryControlTotal != e.Row.CuryDocTotal)
					{
						e.Cache.RaiseExceptionHandling<POLandedCostDoc.curyControlTotal>(e.Row, e.Row.CuryControlTotal,
							new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						e.Cache.RaiseExceptionHandling<POLandedCostDoc.curyControlTotal>(e.Row, e.Row.CuryControlTotal, null);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<POLandedCostDoc.vendorID> e)
		{
			POLandedCostDoc receipt = (POLandedCostDoc) e.Row;

			company.RaiseFieldUpdated(e.Cache, e.Row);
			vendor.RaiseFieldUpdated(e.Cache, e.Row);

			e.Cache.SetDefaultExt<POLandedCostDoc.createBill>(receipt);

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || e.Cache.GetValuePending<POLandedCostDoc.curyID>(e.Row) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<POLandedCostDoc.curyInfoID>(e.Cache, e.Row);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						e.Cache.RaiseExceptionHandling<POLandedCostDoc.docDate>(e.Row, ((POLandedCostDoc) e.Row).DocDate,
							new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						((POLandedCostDoc) e.Row).CuryID = info.CuryID;
					}
				}
			}

			e.Cache.SetDefaultExt<POLandedCostDoc.vendorLocationID>(e.Row);

			// Pay-to Vendor must be defaulted before terms defaulting
			if (receipt.VendorID != null)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
				{
					Vendor orderVendor =
						PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>>>.SelectSingleBound(this,
							new object[] {receipt});
					receipt.PayToVendorID = orderVendor?.PayToVendorID ?? receipt.VendorID;
				}
				else
				{
					receipt.PayToVendorID = receipt.VendorID;
				}
			}

			e.Cache.SetDefaultExt<POLandedCostDoc.termsID>(e.Row); // Defaulting of terms depends on pay-to vendor ID
			e.Cache.SetDefaultExt<POLandedCostDoc.taxZoneID>(e.Row);

			Validate.VerifyField<POLandedCostDoc.payToVendorID>(e.Cache, receipt);

		}

		protected virtual void _(Events.FieldVerifying<POLandedCostDoc.payToVendorID>  e)
		{
			var doc = e.Row as POLandedCostDoc;
			if (doc == null) return;

			Vendor payToVendor = PXSelectReadonly<Vendor, Where<Vendor.bAccountID, Equal<Required<POLandedCostDoc.payToVendorID>>>>.Select(this, e.NewValue);

			if (payToVendor?.CuryID != null && payToVendor.AllowOverrideCury != true && doc.CuryID != payToVendor.CuryID)
			{
				e.NewValue = payToVendor.AcctCD;
				throw new PXSetPropertyException(Messages.PayToVendorHasDifferentCury, payToVendor.AcctCD, payToVendor.CuryID, doc.CuryID);
			}
		}

		protected virtual void _(Events.FieldUpdated<POLandedCostDoc.payToVendorID> e)
		{
			e.Cache.SetDefaultExt<POLandedCostDoc.termsID>(e.Row); // Defaulting of terms depends on pay-to vendor ID
			e.Cache.SetDefaultExt<POLandedCostDoc.taxZoneID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<POLandedCostDetail.landedCostCodeID> e)
		{
			e.Cache.SetDefaultExt<POLandedCostDetail.descr>(e.Row);
			e.Cache.SetDefaultExt<POLandedCostDetail.taxCategoryID>(e.Row);
			e.Cache.SetDefaultExt<POLandedCostDetail.lCAccrualAcct>(e.Row);
			e.Cache.SetDefaultExt<POLandedCostDetail.lCAccrualSub>(e.Row);
		}

		protected virtual void POLandedCostDoc_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<POLandedCostDoc.docDate>(sender, e);
		}

		protected virtual void _(Events.FieldUpdated<POLandedCostDoc.vendorLocationID> e)
		{
			e.Cache.SetDefaultExt<POLandedCostDoc.branchID>(e.Row);
			e.Cache.SetDefaultExt<POLandedCostDoc.taxZoneID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<POLandedCostDetail.inventoryID> e)
		{

		}

		protected virtual void _(Events.RowPersisting<POLandedCostDoc> e)
		{
			var doc = e.Row;

			if (e.Operation != PXDBOperation.Delete && doc.CreateBill == true && String.IsNullOrEmpty(doc.VendorRefNbr))
			{
				e.Cache.RaiseExceptionHandling<POLandedCostDoc.vendorRefNbr>(doc, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo.curyID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo row = (CurrencyInfo) e.Row;
				var doc = Document.Current;
				if (row != null && doc != null && row.CuryInfoID == doc.CuryInfoID)
				{
					if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
					{
						e.NewValue = vendor.Current.CuryID;
						e.Cancel = true;
					}
				}
			}
		}


		protected virtual void _(Events.FieldDefaulting<CurrencyInfo.curyRateTypeID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryRateTypeID))
				{
					e.NewValue = vendor.Current.CuryRateTypeID;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = cmsetup.Current.APRateTypeDflt;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.RowPersisted<POLandedCostDetail> e)
		{
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual void _(Events.RowPersisting<POLandedCostDetail> e)
		{
		}

		#endregion

		public virtual IEnumerable PoReceiptSelection()
		{
			var cmd = new PXSelectJoinGroupBy<POReceipt,
				InnerJoin<POReceiptLine, On<POReceipt.receiptType, Equal<POReceiptLine.receiptType>, And<POReceipt.receiptNbr,
					Equal<POReceiptLine.receiptNbr>,
					And<POReceiptLine.lineType, NotEqual<POLineType.service>, And<POReceiptLine.lineType, NotEqual<POLineType.freight>>
					>>>>,
				Where<POReceipt.receiptType, Equal<Current<POReceiptFilter.receiptType>>,
					And2<Where<Current<POReceiptFilter.vendorID>, IsNull,
							Or<POReceipt.vendorID, Equal<Current<POReceiptFilter.vendorID>>>>,
						And2<Where<Current<POReceiptFilter.receiptNbr>, IsNull,
								Or<POReceipt.receiptNbr, Equal<Current<POReceiptFilter.receiptNbr>>>>,
							And<POReceipt.released, Equal<True>>>>>,
				Aggregate<GroupBy<POReceipt.receiptType, GroupBy<POReceipt.receiptNbr, Count<POReceiptLine.lineNbr>>>>,
				OrderBy<Desc<POReceipt.receiptDate, Desc<POReceipt.lastModifiedDateTime>>>>(this);

			var currentReceiptNbrs = ReceiptLines.Select().RowCast<POLandedCostReceiptLine>()
				.GroupBy(t => new
				{
					ReceiptType = t.POReceiptType,
					ReceiptNbr = t.POReceiptNbr
				})
				.Select(t => new
				{
					t.Key.ReceiptType,
					t.Key.ReceiptNbr,
					RowCount = t.Count()
				}).ToArray();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			int maximumRows = PXView.MaximumRows;

			if (currentReceiptNbrs.Any())
			{
				maximumRows += currentReceiptNbrs.Length;
			}

			var resultSet = cmd.View.Select(PXView.Currents, PXView.Parameters, new object[PXView.SortColumns.Length],
				PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, maximumRows, ref totalRows);
			PXView.StartRow = 0;

			var result = resultSet
				.Select(t => (PXResult<POReceipt, POReceiptLine>) t)
				.Where(t => t.RowCount > 0)
				.Where(t => !currentReceiptNbrs.Contains(new
				{
					ReceiptType = ((POReceipt) t).ReceiptType,
					ReceiptNbr = ((POReceipt) t).ReceiptNbr,
					RowCount = t.RowCount.Value
				})).RowCast<POReceipt>().ToList();

			return result;
		}

		public virtual IEnumerable PoReceiptLinesSelection()
		{
			PXSelectBase<POReceiptLineAdd> cmd = new PXSelect<POReceiptLineAdd,
				Where<POReceiptLineAdd.receiptType, Equal<Current<POReceiptFilter.receiptType>>, And<POReceiptLineAdd.lineType,
					NotEqual<POLineType.service>, And<POReceiptLineAdd.lineType, NotEqual<POLineType.freight>, And2<
						Where<Current<POReceiptFilter.receiptNbr>, IsNull,
							Or<POReceiptLineAdd.receiptNbr, Equal<Current<POReceiptFilter.receiptNbr>>>>, And2<
							Where<Current<POReceiptFilter.orderType>, IsNull,
								Or<POReceiptLineAdd.pOType, Equal<Current<POReceiptFilter.orderType>>>>, And2<
								Where<Current<POReceiptFilter.orderNbr>, IsNull,
									Or<POReceiptLineAdd.pONbr, Equal<Current<POReceiptFilter.orderNbr>>>>, And2<
									Where<Current<POReceiptFilter.inventoryID>, IsNull,
										Or<POReceiptLineAdd.inventoryID, Equal<Current<POReceiptFilter.inventoryID>>>>, And2<
										Where<Current<POReceiptFilter.vendorID>, IsNull,
											Or<POReceiptLineAdd.vendorID, Equal<Current<POReceiptFilter.vendorID>>>>,
										And<Where<POReceiptLineAdd.released, Equal<True>>>>>>>>>>>
				, OrderBy<Desc<POReceiptLineAdd.receiptDate, Desc<POReceiptLineAdd.receiptLastModifiedDateTime,
					Desc<POReceiptLineAdd.receiptNbr, Desc<POReceiptLineAdd.lineNbr>>>>>>(this);

			var currentReceiptLines = ReceiptLines.Select().RowCast<POLandedCostReceiptLine>()
				.Select(t => new {t.POReceiptType, t.POReceiptNbr, t.POReceiptLineNbr}).ToArray();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			int maximumRows = PXView.MaximumRows;

			if (currentReceiptLines.Any() && maximumRows > 0)
			{
				maximumRows += currentReceiptLines.Length;
			}

			var result = cmd.View.Select(PXView.Currents, PXView.Parameters, new object[PXView.SortColumns.Length],
				PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, maximumRows, ref totalRows).RowCast<POReceiptLineAdd>().ToList();
			PXView.StartRow = 0;

			result = result.Where(t =>
				!currentReceiptLines.Contains(new
				{
					POReceiptType = t.ReceiptType,
					POReceiptNbr = t.ReceiptNbr,
					POReceiptLineNbr = t.LineNbr
				})).ToList();

			return result;
		}

		public override void Persist()
		{
			ValidateDocument();

			AllocateLandedCosts();

			base.Persist();
		}

		protected virtual void ValidateDocument()
		{
			if (Document.Current != null)
			{
				var doc = Document.Current;
				var details = Details.Select().RowCast<POLandedCostDetail>().ToArray();
				var receiptLines = ReceiptLines.Select().RowCast<POLandedCostReceiptLine>().ToArray();

				if (receiptLines.Any())
				{
					bool hasErrors = false;
					var errorMessage = "";

					foreach (var detail in details)
					{
						string message;

						if (!LandedCostAllocationService.Instance.ValidateLCTran(this, doc, receiptLines, detail, out message))
						{
							if (this.Details.Cache.RaiseExceptionHandling<POLandedCostDetail.landedCostCodeID>(detail,
								detail.LandedCostCodeID, new PXSetPropertyException(message, PXErrorLevel.RowError)))
							{
								throw new PXRowPersistingException(typeof(POLandedCostDetail.landedCostCodeID).Name, detail.LandedCostCodeID,
									message);
							}
							else
							{
								hasErrors = true;
								errorMessage = message;
							}
						}
					}

					if (hasErrors)
						throw new PXException(errorMessage);
				}
				else if (doc.Hold == false)
				{
					throw new PXException(Messages.LandedCostsDetailsEmpty);
				}
			}
		}

		#region Buttons

		public PXAction<POLandedCostDoc> createAPInvoice;

		[PXUIField(DisplayName = Messages.EnterAPBill, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable CreateAPInvoice(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				var landedCostDoc = Document.Current;

				if (landedCostDoc.Released == true)
				{
					Document.Current = landedCostDoc;

					var details = Details.Select().RowCast<POLandedCostDetail>()
						.Where(t => String.IsNullOrEmpty(t.APDocType) && String.IsNullOrEmpty(t.APRefNbr)).ToList();

					var taxes = Taxes.Select().RowCast<POLandedCostTaxTran>().ToList();

					if (!details.Any())
						return adapter.Get();

					var factory = new LandedCostAPBillFactory(this);
					var bill = factory.CreateLandedCostBill(landedCostDoc, details, taxes);

					APInvoiceEntry apGraph = PXGraph.CreateInstance<APInvoiceEntry>();

					var curyInfo =
						(CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
							.SelectWindowed(this, 0, 1, bill.Document.CuryInfoID);

					var newCuryInfo = (CurrencyInfo)apGraph.currencyinfo.Cache.CreateCopy(curyInfo);
					newCuryInfo.CuryInfoID = null;
					newCuryInfo = apGraph.currencyinfo.Insert(newCuryInfo);
					bill.Document.CuryInfoID = newCuryInfo.CuryInfoID;

					apGraph.Document.Insert(bill.Document);

					TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(apGraph.Transactions.Cache, null, TaxCalc.ManualCalc);

					foreach (var inTran in bill.Transactions)
					{
						var ct = apGraph.Transactions.Insert(inTran);
						apGraph.LandedCostDetailSetLink(ct);
					}

					var aMap = apGraph.Approval.GetAssignedMaps(apGraph.Document.Current, apGraph.Document.Cache);
					if (aMap.Any())
					{
						apGraph.Approval.Assign(apGraph.Document.Current, aMap);
					}

					foreach (APTaxTran existingTaxTran in apGraph.Taxes.Cache.Cached.RowCast<APTaxTran>())
					{
						apGraph.Taxes.Delete(existingTaxTran);
					}

					foreach (var newTax in bill.Taxes)
					{
						newTax.CuryInfoID = newCuryInfo.CuryInfoID;
						apGraph.Taxes.Insert(newTax);
					}
					
					landedCostDoc.APDocCreated = true;
					apGraph.Caches<POLandedCostDoc>().SetStatus(landedCostDoc, PXEntryStatus.Updated);

					throw new PXRedirectRequiredException(apGraph, Messages.CreateAPInvoice);
				}
			}

			return adapter.Get();
		}

		public ToggleCurrency<POLandedCostDoc> CurrencyView;

		public PXAction<POLandedCostDoc> addPOReceipt;

		[PXUIField(DisplayName = Messages.AddPOReceipt, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceipt(PXAdapter adapter)
		{

			if (Document.Current != null &&
			    Document.Current.Released != true)
			{
				if (poReceiptSelection.AskExt((graph, view) =>
				{
					filter.Cache.ClearQueryCacheObsolete();
					filter.View.Clear();
					filter.Cache.Clear();

					poReceiptSelection.Cache.ClearQueryCacheObsolete();
					poReceiptSelection.View.Clear();
					poReceiptSelection.Cache.Clear();
				}, true) == WebDialogResult.OK)
				{
					return AddPOReceipt2(adapter);
				}
			}

			return adapter.Get();
		}

		protected virtual void _(Events.RowDeleted<POLandedCostTaxTran> e)
		{

		}

		protected virtual void _(Events.FieldVerifying<POLandedCostDoc.billDate> e)
		{
			var row = (POLandedCostDoc)e.Row;
			if (e.NewValue != null)
			{
				int? parentOrganizationID = PXAccess.GetParentOrganizationID(row.BranchID);
				FinPeriod fp = FinPeriodRepository.FindFinPeriodByDate((DateTime?)e.NewValue, parentOrganizationID);

				if (fp == null)
					throw new PXSetPropertyException<POLandedCostDoc.billDate>(GL.Messages.TranDateOutOfRange, e.NewValue, PXAccess.GetOrganizationCD(parentOrganizationID));

				ProcessingResult result = FinPeriodUtils.CanPostToPeriod(fp, typeof(FinPeriod.aPClosed));
				if (!result.IsSuccess)
					throw new PXSetPropertyException<POLandedCostDoc.billDate>(result.GetGeneralMessage());
			}
		}

		protected virtual void _(Events.FieldDefaulting<POReceiptFilter.receiptType> e)
		{
			var currentReceiptType = GetCurrentReceiptType();

			if (!String.IsNullOrEmpty(currentReceiptType))
				e.NewValue = currentReceiptType;
		}

		protected virtual void _(Events.RowSelected<POReceiptFilter> e)
		{
			var currentReceiptType = GetCurrentReceiptType();

			PXUIFieldAttribute.SetEnabled<POReceiptFilter.receiptType>(e.Cache, null, String.IsNullOrEmpty(currentReceiptType));
		}

		protected virtual string GetCurrentReceiptType()
		{
			var firstLine = ReceiptLines.SelectWindowed(0, 1).RowCast<POLandedCostReceiptLine>().FirstOrDefault();

			if (firstLine != null)
				return firstLine.POReceiptType;

			return null;
		}

		public PXAction<POLandedCostDoc> addPOReceipt2;

		[PXUIField(DisplayName = Messages.AddPOReceipt, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceipt2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
			    this.Document.Current.Released != true)
			{
				var receipts = poReceiptSelection.Cache.Updated.RowCast<POReceipt>()
					.Where(t => t.Selected == true && t.ReceiptType == filter.Current.ReceiptType).ToArray();

				AddPurchaseReceipts(receipts);

				receipts.ForEach(t => t.Selected = false);
			}

			return adapter.Get();
		}

		public PXAction<POLandedCostDoc> addPOReceiptLine;

		[PXUIField(DisplayName = Messages.AddPOReceiptLine, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptLine(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
			    this.Document.Current.Released != true)
			{
				if (poReceiptLinesSelection.AskExt((graph, view) =>
				{
					filter.Cache.ClearQueryCacheObsolete();
					filter.View.Clear();
					filter.Cache.Clear();

					filter.Current.ReceiptType = GetCurrentReceiptType();

					poReceiptLinesSelection.Cache.ClearQueryCacheObsolete();
					poReceiptLinesSelection.View.Clear();
					poReceiptLinesSelection.Cache.Clear();
				}, true) == WebDialogResult.OK)
				{
					return AddPOReceiptLine2(adapter);
				}
			}

			return adapter.Get();
		}

		public PXAction<POLandedCostDoc> addPOReceiptLine2;

		[PXUIField(DisplayName = Messages.AddPOReceiptLine, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptLine2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
			    this.Document.Current.Released != true)
			{
				var linesToAdd = poReceiptLinesSelection.Cache.Updated.RowCast<POReceiptLineAdd>().Where(t => t.Selected == true)
					.ToArray();

				AddPurchaseReceiptLines(linesToAdd);

				linesToAdd.ForEach(t => t.Selected = false);
			}

			return adapter.Get();
		}

		public PXAction<POLandedCostDoc> addLC;

		[PXUIField(DisplayName = Messages.AddLandedCosts, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddLC(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<POLandedCostDoc> release;

		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update,
			MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = this.Document.Cache;
			var list = new List<POLandedCostDoc>();
			foreach (POLandedCostDoc indoc in adapter.Get<POLandedCostDoc>())
			{
				if (indoc.Hold == false && indoc.Released == false)
				{
					cache.Update(indoc);
					list.Add(indoc);
				}
			}

			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, delegate() { ReleaseDoc(list); });
			return list;
		}

		public PXAction<POLandedCostDoc> action;

		[PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Action(PXAdapter adapter, [PXString()] string ActionName)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					List<object> result = new List<object>();
					foreach (object data in action.Press(adapter))
					{
						result.Add(data);
					}
					return result;
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Methods

		public virtual void AddPurchaseReceipts(IEnumerable<POReceipt> receipts)
		{
			var lines = new List<POReceiptLineAdd>();

			var currentFilter = filter.Current;

			foreach (var receipt in receipts)
			{
				filter.Cache.Remove(filter.Current);
				filter.Cache.Insert(new POReceiptFilter());
				filter.Current.ReceiptType = receipt.ReceiptType;
				filter.Current.ReceiptNbr = receipt.ReceiptNbr;

				var receiptLines = poReceiptLinesSelection.Select().RowCast<POReceiptLineAdd>().ToArray();

				lines.AddRange(receiptLines);
			}

			filter.Cache.Remove(filter.Current);
			filter.Cache.Insert(currentFilter);

			AddPurchaseReceiptLines(lines);
		}

		protected virtual POReceiptLineAdd[] GetLinesWithoutDuplicates(IEnumerable<POReceiptLineAdd> lines)
		{
			var existingReceiptLines = this.ReceiptLines.Select().RowCast<POLandedCostReceiptLine>();

			var asd = existingReceiptLines.Select(t =>
				new {ReceiptType = t.POReceiptType, ReceiptNbr = t.POReceiptNbr, ReceiptLineNbr = t.POReceiptLineNbr}).ToList();

			var result = lines.Where(t =>
				!asd.Contains(new {ReceiptType = t.ReceiptType, ReceiptNbr = t.ReceiptNbr, ReceiptLineNbr = t.LineNbr})).ToArray();

			return result;
		}

		public virtual void AddPurchaseReceiptLines(IEnumerable<POReceiptLineAdd> lines)
		{
			if (Document.Current == null || lines == null)
				return;

			var newLines = GetLinesWithoutDuplicates(lines);

			foreach (var line in newLines)
			{
				CurrencyInfo currencyInfo =
					PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.SelectWindowed(
						this, 0, 1, line.CuryInfoID);

				var landedCostReceiptLine = new POLandedCostReceiptLine
				{
					DocType = Document.Current.DocType,
					RefNbr = Document.Current.RefNbr,
					POReceiptType = line.ReceiptType,
					POReceiptNbr = line.ReceiptNbr,
					POReceiptLineNbr = line.LineNbr,
					BranchID = line.BranchID,
					SiteID = line.SiteID,
					InventoryID = line.InventoryID,
					SubItemID = line.SubItemID,
					UOM = line.UOM,
					BaseReceiptQty = line.BaseReceiptQty,
					ReceiptQty = line.ReceiptQty,
					POReceiptBaseCuryID = currencyInfo.BaseCuryID,
					LineAmt = line.TranCostFinal,
					AllocatedLCAmt = 0,
					CuryAllocatedLCAmt = 0,
					UnitWeight = line.UnitWeight,
					UnitVolume = line.UnitVolume,
				};

				ReceiptLines.Insert(landedCostReceiptLine);
			}
		}

		public virtual void AllocateLandedCosts()
		{
			if (Document.Current == null)
				return;

			var doc = Document.Current;
			var landedCostReceiptLines = this.ReceiptLines.Select().AsEnumerable().Select(t => (POLandedCostReceiptLine) t).ToList();
			var details = Details.Select().AsEnumerable().Select(t => (POLandedCostDetail) t).ToList();

			var taxesQuery = new PXSelectJoin<POLandedCostTax, InnerJoin<Tax, On<POLandedCostTax.taxID, Equal<Tax.taxID>>>,
				Where<POLandedCostTax.docType, Equal<Required<POLandedCostTax.docType>>, And<POLandedCostTax.refNbr, Equal<Required<POLandedCostTax.refNbr>>>>>(this);

			var taxes = taxesQuery.Select(doc.DocType, doc.RefNbr).AsEnumerable().Select(t=> (PXResult<POLandedCostTax, Tax>)t).ToList();

			var landedCostAdjustments = LandedCostAllocationService.Instance.Allocate(this, doc, landedCostReceiptLines, details, taxes);
			var landedCostSplits = LandedCostAllocationService.Instance.GetLandedCostSplits(doc, landedCostAdjustments);

			TrackLandedCostSplits(landedCostSplits);

			foreach (var landedCostReceiptLine in landedCostReceiptLines)
			{
				var receiptLineSplits = landedCostSplits.Where(t => t.ReceiptLineNbr == landedCostReceiptLine.LineNbr).ToList();

				landedCostReceiptLine.AllocatedLCAmt = receiptLineSplits.Sum(t => t.LineAmt);

				decimal curyAllocatedLCAmt;
				PXDBCurrencyAttribute.CuryConvCury<POLandedCostDoc.curyInfoID>(Document.Cache, Document.Current,
					landedCostReceiptLine.AllocatedLCAmt ?? 0, out curyAllocatedLCAmt);
				landedCostReceiptLine.CuryAllocatedLCAmt = curyAllocatedLCAmt;

				ReceiptLines.Cache.Update(landedCostReceiptLine);
			}

			decimal curyAllocatedTotal;
			doc.AllocatedTotal = landedCostReceiptLines.Sum(t => t.AllocatedLCAmt ?? 0);
			PXDBCurrencyAttribute.CuryConvCury<POLandedCostDoc.curyInfoID>(Document.Cache, Document.Current,
				doc.AllocatedTotal.Value, out curyAllocatedTotal);
			doc.CuryAllocatedTotal = curyAllocatedTotal;
			Document.Cache.Update(doc);
		}

		public virtual void ReleaseDoc(List<POLandedCostDoc> list)
		{
			foreach (var doc in list)
			{
				ReleaseDoc(doc);
			}
		}

		public virtual void ReleaseDoc(POLandedCostDoc doc)
		{
			if (doc == null)
				return;

			if (Document.Current == null || Document.Current.DocType != doc.DocType || Document.Current.RefNbr != doc.RefNbr)
			{
				Clear();
				Document.Current = Document.Search<POLandedCostDoc.refNbr>(doc.RefNbr, doc.DocType);
			}

			POSetup poSetupR = posetup.Select();
			var errorMessage = "";
			int errorCntr = 0;

			bool autoReleaseIN = poSetupR.AutoReleaseLCIN == true;
			bool autoReleaseAP = poSetupR.AutoReleaseAP == true;

			var receiptLines = ReceiptLines.View.SelectMultiBound(new object[] {doc}).RowCast<POLandedCostReceiptLine>()
				.ToList();
			var details = Details.View.SelectMultiBound(new object[] {doc}).RowCast<POLandedCostDetail>().ToList();
			var taxesQuery = new PXSelectJoin<POLandedCostTax, InnerJoin<Tax, On<POLandedCostTax.taxID, Equal<Tax.taxID>>>,
				Where<POLandedCostTax.docType, Equal<Required<POLandedCostTax.docType>>, And<POLandedCostTax.refNbr, Equal<Required<POLandedCostTax.refNbr>>>>>(this);

			var lcTaxes = taxesQuery.Select(doc.DocType, doc.RefNbr).Select(t => (PXResult<POLandedCostTax, Tax>)t).ToList();

			var taxes = Taxes.View.SelectMultiBound(new object[] {doc}).RowCast<POLandedCostTaxTran>().ToList();

			if (!receiptLines.Any() || !details.Any())
			{
				throw new PXException(Messages.LandedCostsCantReleaseWoDetails);
			}

			var notReleasedReceiptsQuery = new PXSelectJoin<POLandedCostReceipt,
				InnerJoin<POReceipt, On<POLandedCostReceipt.pOReceiptType, Equal<POReceipt.receiptType>,
					And<POLandedCostReceipt.pOReceiptNbr, Equal<POReceipt.receiptNbr>>>>,
				Where<POReceipt.released, Equal<False>, 
					And<POLandedCostReceipt.lCDocType, Equal<Required<POLandedCostReceipt.lCDocType>>,
					And<POLandedCostReceipt.lCRefNbr, Equal<Required<POLandedCostReceipt.lCRefNbr>>>>>>(this);

			var notReleasedReceipt = notReleasedReceiptsQuery.SelectWindowed(0, 1, doc.DocType, doc.RefNbr).RowCast<POReceipt>();
			if (notReleasedReceipt.Any())
			{
				throw new PXException(Messages.LandedCostsCannotReleaseWithUnreleasedReceipts);
			}

			var forReleaseIN = new List<INRegister>();
			var forReleaseAP = new List<APRegister>();

			var landedCostReceiptLineAdjustments = LandedCostAllocationService.Instance.Allocate(this, doc, receiptLines, details, lcTaxes);
			var splits = LandedCostAllocationService.Instance.GetLandedCostSplits(doc, landedCostReceiptLineAdjustments);

			using (var releaseTransactionScope = new PXTransactionScope())
			{
				try
				{
					forReleaseIN = CreateLandedCostAdjustment(doc, landedCostReceiptLineAdjustments);

					doc.Released = true;

					Document.Cache.Update(doc);
					Save.Press();

					releaseTransactionScope.Complete();
				}
				catch (Exception ex)
				{
					throw new PXException(ex, Messages.LandedCostsFailCreateAdj, ex.Message);
				}
			}

			using (var releaseTransactionScope = new PXTransactionScope())
			{
				try
				{
					if (doc.CreateBill == true && details.All(t => String.IsNullOrEmpty(t.APRefNbr)))
					{
						forReleaseAP = CreateLandedCostBill(doc, receiptLines, details, splits, taxes);
					}

					Document.Cache.Update(doc);
					Save.Press();

					releaseTransactionScope.Complete();
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
					errorCntr++;
					errorMessage = PXLocalizer.LocalizeFormat(Messages.LandedCostsFailCreateInvoice, ex.Message);
				}
			}

			if (autoReleaseIN)
			{
				try
				{
					if (forReleaseIN.Any())
						INDocumentRelease.ReleaseDoc(forReleaseIN, false);
				}
				catch(Exception ex)
				{
					PXTrace.WriteError(ex);
					errorCntr++;
					errorMessage = PXLocalizer.LocalizeFormat(Messages.LandedCostsFailReleaseAdj, ex.Message);
				}
			}

			if (autoReleaseAP)
			{
				try
				{
					if (forReleaseAP.Any())
						APDocumentRelease.ReleaseDoc(forReleaseAP, true);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
					errorCntr++;
					errorMessage = PXLocalizer.LocalizeFormat(Messages.LandedCostsFailReleaseInvoice, ex.Message);
				}
			}

			if (errorCntr == 1)
			{
				throw new PXException(errorMessage);
			} else if (errorCntr > 1)
			{
				throw new PXException(Messages.LandedCostsManyErrors);
			}
		}

		protected virtual List<INRegister> CreateLandedCostAdjustment(POLandedCostDoc doc,
			IEnumerable<LandedCostAllocationService.POLandedCostReceiptLineAdjustment> adjustments)
		{
			POSetup poSetupR = posetup.Select();

			INAdjustmentEntry inGraph = PXGraph.CreateInstance<INAdjustmentEntry>();
			var inAdjustmentFactory = GetInAdjustmentFactory(inGraph);
			var inAdjustmentsDictionary = inAdjustmentFactory.CreateLandedCostAdjustments(doc, adjustments);

			bool autoReleaseIN = poSetupR.AutoReleaseLCIN == true;
			var forReleaseIN = new List<INRegister>();

			using (var ts = new PXTransactionScope())
			{
				inGraph.FieldVerifying.AddHandler<INTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) =>
				{
					e.Cancel = true;
				});

				inGraph.FieldVerifying.AddHandler<INTran.origRefNbr>((PXCache sender, PXFieldVerifyingEventArgs e) =>
				{
					e.Cancel = true;
				});

				foreach (var inAdjustmentPair in inAdjustmentsDictionary)
				{
					inGraph.insetup.Current.RequireControlTotal = false;

					if (autoReleaseIN)
					{
						inGraph.insetup.Current.HoldEntry = false;
					}

					inGraph.adjustment.Insert(inAdjustmentPair.Value.Document);

					foreach (var inTran in inAdjustmentPair.Value.Transactions)
					{
						inGraph.transactions.Insert(inTran);
					}

					inGraph.Save.Press();

					var currentAdjustment = inGraph.adjustment.Current;

					inAdjustmentPair.Key.INDocType = currentAdjustment.DocType;
					inAdjustmentPair.Key.INRefNbr = currentAdjustment.RefNbr;

					Details.Cache.Update(inAdjustmentPair.Key);

					forReleaseIN.Add(currentAdjustment);

					inGraph.Clear(); // Clear cannot be in the beginning of the cycle because CreateLandedCostAdjustments method can add new Sub(s) to the cache.
				}

				doc.INDocCreated = true;
				Document.Cache.Update(doc);

				ts.Complete();
			}

			return forReleaseIN;
		}

		protected virtual List<APRegister> CreateLandedCostBill(POLandedCostDoc doc,
			IEnumerable<POLandedCostReceiptLine> receiptLines, IEnumerable<POLandedCostDetail> details,
			IEnumerable<POLandedCostSplit> splits, IEnumerable<POLandedCostTaxTran> taxes)
		{
			POSetup poSetupR = posetup.Select();

			var apBillFactory = GetApBillFactory();
			var bill = apBillFactory.CreateLandedCostBill(doc, details, taxes);

			bool autoReleaseAP = poSetupR.AutoReleaseAP == true;
			var forReleaseAP = new List<APRegister>();

			using (var ts = new PXTransactionScope())
			{
				APInvoiceEntry apGraph = PXGraph.CreateInstance<APInvoiceEntry>();

				if (autoReleaseAP)
				{
					apGraph.APSetup.Current.RequireControlTotal = false;
					apGraph.APSetup.Current.RequireControlTaxTotal = false;
					apGraph.APSetup.Current.HoldEntry = false;
				}

				var curyInfo =
					(CurrencyInfo) PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
						.SelectWindowed(this, 0, 1, bill.Document.CuryInfoID);

				var newCuryInfo = (CurrencyInfo) apGraph.currencyinfo.Cache.CreateCopy(curyInfo);
				newCuryInfo.CuryInfoID = null;
				newCuryInfo = apGraph.currencyinfo.Insert(newCuryInfo);
				bill.Document.CuryInfoID = newCuryInfo.CuryInfoID;

				apGraph.Document.Insert(bill.Document);
				var currentBill = apGraph.Document.Current;

				TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(apGraph.Transactions.Cache, null, TaxCalc.ManualCalc);

				foreach (var apTran in bill.Transactions)
				{
					apGraph.Transactions.Insert(apTran);
				}

				foreach (APTaxTran existingTaxTran in apGraph.Taxes.Cache.Cached.RowCast<APTaxTran>())
				{
					apGraph.Taxes.Delete(existingTaxTran);
				}

				foreach (var newTax in bill.Taxes)
				{
					newTax.CuryInfoID = newCuryInfo.CuryInfoID;
					apGraph.Taxes.Insert(newTax);
				}

				var aMap = apGraph.Approval.GetAssignedMaps(apGraph.Document.Current, apGraph.Document.Cache);
				if (aMap.Any())
				{
					apGraph.Approval.Assign(apGraph.Document.Current, aMap);
				}

				apGraph.Save.Press();

				foreach (var detail in details)
				{
					detail.APDocType = currentBill.DocType;
					detail.APRefNbr = currentBill.RefNbr;

					Details.Cache.Update(detail);
				}

				doc.APDocCreated = true;
				Document.Cache.Update(doc);

				forReleaseAP.Add(currentBill);

				ts.Complete();
			}

			return forReleaseAP;
		}

		protected virtual void TrackLandedCostSplits(IEnumerable<POLandedCostSplit> landedCostSplits)
		{
			var comparer = Splits.Cache.GetComparer();
			var currentSplitsDict = Splits.Select().RowCast<POLandedCostSplit>()
				.ToDictionary<POLandedCostSplit, POLandedCostSplit>(r => r, comparer);

			foreach (var landedCostSplit in landedCostSplits)
			{
				currentSplitsDict.TryGetValue(landedCostSplit, out POLandedCostSplit currentSplit);

				PXCurrencyAttribute.CuryConvCury<POLandedCostDoc.curyInfoID>(Document.Cache, Document.Cache.Current, landedCostSplit.LineAmt ?? 0m, out decimal curyLineAmt);
				if (currentSplit != null)
				{
					POLandedCostSplit copy = (POLandedCostSplit)Splits.Cache.CreateCopy(currentSplit);
					copy.LineAmt = landedCostSplit.LineAmt;
					copy.CuryLineAmt = curyLineAmt;

					Splits.Update(copy);
					currentSplitsDict.Remove(landedCostSplit);
				}
				else
				{
					landedCostSplit.CuryLineAmt = curyLineAmt;

					Splits.Insert(landedCostSplit);
				}
			}

			foreach (POLandedCostSplit obsoleteSplit in currentSplitsDict.Keys)
			{
				Splits.Delete(obsoleteSplit);
			}
		}

		#endregion
	}
}
