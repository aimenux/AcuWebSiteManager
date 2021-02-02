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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AR
{
	public class ARInvoiceEntryExternalTax : ExternalTax<ARInvoiceEntry, ARInvoice>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public override ARInvoice CalculateExternalTax(ARInvoice invoice)
		{
			if (invoice.InstallmentNbr != null || invoice.IsChildRetainageDocument())
			{
				//do not calculate tax for installments and child retainage documents
				return invoice;
			}

			var toAddress = GetToAddress(invoice);
			bool isNonTaxable = IsNonTaxable(toAddress);

			if (isNonTaxable)
			{
				ApplyTax(invoice, GetTaxResult.Empty);
				invoice.IsTaxValid = true;
				invoice.NonTaxable = true;
				invoice.IsTaxSaved = false;
				invoice = Base.Document.Update(invoice);

				SkipTaxCalcAndSave();

				return invoice;
			}
			else if (invoice.NonTaxable == true)
			{
				Base.Document.SetValueExt<ARInvoice.nonTaxable>(invoice, false);
			}

			var service = TaxProviderFactory(Base, invoice.TaxZoneID);

			GetTaxRequest getRequest = BuildGetTaxRequest(invoice);

			if (getRequest.CartItems.Count == 0)
			{
				ApplyTax(invoice, GetTaxResult.Empty);
				invoice.IsTaxValid = true;
				invoice.IsTaxSaved = false;
				invoice = Base.Document.Update(invoice);

				SkipTaxCalcAndSave();

				return invoice;
			}

			GetTaxResult result = service.GetTax(getRequest);
			if (result.IsSuccess)
			{
				try
				{
					ApplyTax(invoice, result);
					SkipTaxCalcAndSave();
				}
				catch (PXOuterException ex)
				{
					try
					{
						CancelTax(invoice, VoidReasonCode.Unspecified);
					}
					catch (Exception)
					{
						throw new PXException(new PXException(ex, TX.Messages.FailedToApplyTaxes), TX.Messages.FailedToCancelTaxes);
					}

					string msg = TX.Messages.FailedToApplyTaxes;
					foreach (string err in ex.InnerMessages)
					{
						msg += Environment.NewLine + err;
					}

					throw new PXException(ex, msg);
				}
				catch (Exception ex)
				{
					try
					{
						CancelTax(invoice, VoidReasonCode.Unspecified);
					}
					catch (Exception)
					{
						throw new PXException(new PXException(ex, TX.Messages.FailedToApplyTaxes), TX.Messages.FailedToCancelTaxes);
					}

					string msg = TX.Messages.FailedToApplyTaxes;
					msg += Environment.NewLine + ex.Message;

					throw new PXException(ex, msg);
				}

				PostTaxRequest request = new PostTaxRequest();
				request.CompanyCode = getRequest.CompanyCode;
				request.DocCode = getRequest.DocCode;
				request.DocDate = getRequest.DocDate;
				request.DocType = getRequest.DocType;
				request.TotalAmount = result.TotalAmount;
				request.TotalTaxAmount = result.TotalTaxAmount;
				PostTaxResult postResult = service.PostTax(request);
				if (postResult.IsSuccess)
				{
					invoice.IsTaxValid = true;
					invoice = Base.Document.Update(invoice);
					SkipTaxCalcAndSave();
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}


			return invoice;
		}

		[PXOverride]
		public virtual void Persist(Action persist)
		{
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID) && Base.Document.Current.Released != true)
			{
				//Avalara - validate that valid ShipTo address can be returned - no mixed carriers, and no mixeds shipto address:
				GetToAddress(Base.Document.Current);

				if (Base.Document.Current.IsOriginalRetainageDocument() && Base.ARSetup.Current?.RetainTaxes == true)
				{
					throw new PXException(Messages.ExternalRetainedTaxesNotSupported);
				}
			}

			persist();

			if (IsDocumentExtTaxValid(Base.Document.Current) && Base.Document.Current.IsTaxValid != true && !skipExternalTaxCalcOnSave && Base.Document.Current.Released != true)
			{
				if (PXLongOperation.GetCurrentItem() == null)
				{
					PXLongOperation.StartOperation(Base, delegate
					{
						Base.RecalculateExternalTax(null, Base.Document.Current);
					});
					    }
					    else
					    {
					Base.RecalculateExternalTax(Base, Base.Document.Current);
				}
			}
		}

		[PXOverride]
		public virtual ARInvoice RecalculateExternalTax(ARInvoiceEntry taxCalculationEntry, ARInvoice invoice)
		{
			bool fromSOModule = invoice.OrigModule == BatchModule.SO;

			taxCalculationEntry = taxCalculationEntry ?? (fromSOModule
				? PXGraph.CreateInstance<SOInvoiceEntry>()
				: PXGraph.CreateInstance<ARInvoiceEntry>());

			if (taxCalculationEntry.Document.Current == null)
				{
				taxCalculationEntry.Document.Current = SelectFrom<ARInvoice>
						.Where<ARInvoice.docType.IsEqual<@P.AsString>
						.And<ARInvoice.refNbr.IsEqual<@P.AsString>>>
						.View.Select(taxCalculationEntry, invoice.DocType, invoice.RefNbr);

				if (fromSOModule)
				{
					taxCalculationEntry.Document.Current.ApplyPaymentWhenTaxAvailable = Base.Document.Current.ApplyPaymentWhenTaxAvailable;
					((SOInvoiceEntry)taxCalculationEntry).SODocument.Current = SelectFrom<SOInvoice>
						.Where<SOInvoice.docType.IsEqual<@P.AsString>
						.And<SOInvoice.refNbr.IsEqual<@P.AsString>>>
						.View.Select(taxCalculationEntry, invoice.DocType, invoice.RefNbr);
				}
			}

			invoice = taxCalculationEntry.CalculateExternalTax(taxCalculationEntry.Document.Current);

			Base.RecalcUnbilledTax();

			return invoice;
		}

		[PXOverride]
		public virtual ARRegister OnBeforeRelease(ARRegister doc)
		{
			skipExternalTaxCalcOnSave = true;

			return doc;
		}

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			if (e.Row == null)
				return;

			bool isRetainageDocument = e.Row.IsRetainageDocument == true;
			bool isRetainageCreditMemo = e.Row.DocType == ARDocType.CreditMemo && (e.Row.IsOriginalRetainageDocument() || e.Row.IsChildRetainageDocument());

			var isExternalTax = IsExternalTax(e.Row.TaxZoneID);
			Base.Taxes.Cache.AllowDelete = Base.Transactions.Cache.AllowDelete && !isExternalTax && e.Row.ProformaExists != true && !isRetainageCreditMemo && !isRetainageDocument;
			Base.Taxes.Cache.AllowInsert = Base.Transactions.Cache.AllowInsert && !isExternalTax && e.Row.ProformaExists != true && !isRetainageCreditMemo && !isRetainageDocument;
			Base.Taxes.Cache.AllowUpdate = Base.Taxes.Cache.AllowUpdate && !isExternalTax && e.Row.ProformaExists != true && !isRetainageCreditMemo && !isRetainageDocument;

			if (isExternalTax && e.Row.IsTaxValid != true)
				PXUIFieldAttribute.SetWarning<ARInvoice.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
		}

		protected virtual void _(Events.RowUpdated<ARInvoice> e)
		{
			if (IsDocumentExtTaxValid(e.Row) && e.Row.Released != true &&
				!e.Cache.ObjectsEqual<ARInvoice.avalaraCustomerUsageType, ARInvoice.curyDiscTot, ARInvoice.customerLocationID, ARInvoice.docDate, ARInvoice.taxZoneID>(e.Row, e.OldRow))
			{
				e.Row.IsTaxValid = false;
			}
		}

		public virtual bool IsDocumentExtTaxValid(ARInvoice doc)
		{
			return doc != null && IsExternalTax(doc.TaxZoneID) && doc.InstallmentNbr == null;
		}


		protected virtual void _(Events.RowInserted<ARTran> e)
		{
			if (IsDocumentExtTaxValid(Base.Document.Current))
			{
				InvalidateExternalTax(Base.Document.Current);
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowUpdated<ARTran> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (IsDocumentExtTaxValid(Base.Document.Current) &&
				!e.Cache.ObjectsEqual<ARTran.avalaraCustomerUsageType, ARTran.accountID, ARTran.inventoryID, ARTran.tranDesc, ARTran.tranAmt, ARTran.tranDate, ARTran.taxCategoryID>(e.Row, e.OldRow))
			{
				InvalidateExternalTax(Base.Document.Current);
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<ARTran> e)
		{
			if (IsDocumentExtTaxValid(Base.Document.Current))
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#region ARShippingAddress Events

		protected virtual void _(Events.RowUpdated<ARShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null
				&& e.Cache.ObjectsEqual<ARShippingAddress.postalCode, ARShippingAddress.countryID, ARShippingAddress.state>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowInserted<ARShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<ARShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}
		
		protected virtual void _(Events.FieldUpdating<ARShippingAddress, ARShippingAddress.overrideAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#endregion

		private void InvalidateExternalTax(ARInvoice doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
			{
				doc.IsTaxValid = false;
				Base.Document.Cache.MarkUpdated(doc);
			}
		}

		protected virtual void _(Events.RowPersisting<ARInvoice> e)
		{
			if (e.Row.IsTaxSaved != true || e.Row.Released == true)
				return;

			//Cancel tax if document is deleted
			if (e.Operation.Command() == PXDBOperation.Delete)
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}

			//Cancel tax if last line in the document is deleted
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update) && !Base.Transactions.Any())
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}

			//Cancel tax if IsExternalTax has changed to False (Document was changed from External Tax Provider to Acumatica Tax Engine) or address has become NonTaxable.
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update) && (!IsExternalTax(e.Row.TaxZoneID) || IsNonTaxable(GetToAddress(e.Row))))
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}
		}

		protected virtual GetTaxRequest BuildGetTaxRequest(ARInvoice invoice)
		{
			if (invoice == null) throw new PXArgumentException(nameof(invoice), ErrorMessages.ArgumentNullException);

			Customer cust = (Customer)Base.customer.View.SelectSingleBound(new object[] { invoice });
			CR.Location loc = (CR.Location)Base.location.View.SelectSingleBound(new object[] { invoice });

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(invoice.TaxZoneID, invoice.BranchID);
			request.CurrencyCode = invoice.CuryID;
			request.CustomerCode = cust.AcctCD;
			IAddressBase fromAddress = GetFromAddress(invoice);
			IAddressBase toAddress = GetToAddress(invoice);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFrom);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetTo);

			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = $"AR.{invoice.DocType}.{invoice.RefNbr}";
			request.DocDate = invoice.DocDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(invoice);
			request.CustomerUsageType = invoice.AvalaraCustomerUsageType;

			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}

			request.DocType = GetTaxDocumentType(invoice);
			Sign sign = GetDocumentSign(invoice);

			PXSelectBase<ARTran> select = new PXSelectJoin<ARTran,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARTran.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<ARTran.accountID>>>>,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
					And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<Where<ARTran.lineType, NotEqual<SOLineType.discount>, Or<ARTran.lineType, IsNull>>>>>,
				OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>(Base);

			request.Discount = GetDocDiscount().GetValueOrDefault();
			DateTime? taxDate = invoice.OrigDocDate;

			bool applyRetainage = Base.ARSetup.Current?.RetainTaxes != true && invoice.IsOriginalRetainageDocument();

			foreach (PXResult<ARTran, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { invoice }))
			{
				ARTran tran = (ARTran)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.Amount = sign * (tran.CuryTranAmt.GetValueOrDefault() + (applyRetainage ? tran.CuryRetainageAmt.GetValueOrDefault(): 0m));
				line.Description = tran.TranDesc;
				line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(invoice, tran));
				line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(invoice, tran));
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(tran.Qty.GetValueOrDefault());
				line.Discounted = tran.LineType != SOLineType.Freight && request.Discount > 0;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;
				line.CustomerUsageType = tran.AvalaraCustomerUsageType;

				if (tran.OrigInvoiceDate != null)
					taxDate = tran.OrigInvoiceDate;

				request.CartItems.Add(line);
			}

			if (applyRetainage)
			{
				var line = new TaxCartItem();
				line.Index = invoice.LineCntr.GetValueOrDefault() + 1;
				line.Amount = Sign.Minus * sign * invoice.CuryLineRetainageTotal.GetValueOrDefault();
				line.Description = PXMessages.LocalizeFormatNoPrefix(AP.Messages.RetainageForTransactionDescription, GetLabel.For<ARDocType>(invoice.DocType), invoice.RefNbr);
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = "Retainage";
				line.Discounted = false;
				line.NonTaxable = true;

				request.CartItems.Add(line);
			}

			if ((invoice.DocType == ARDocType.CreditMemo || invoice.DocType == ARDocType.CashReturn) && invoice.OrigDocDate != null)
			{
				request.TaxOverride.Reason = Messages.ReturnReason;
				request.TaxOverride.TaxDate = taxDate.Value;
				request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
				sign = Sign.Minus;
			}

			return request;
		}

		public virtual TaxDocumentType GetTaxDocumentType(ARInvoice invoice)
		{
			switch (invoice.DrCr)
			{
				case DrCr.Credit:
					return TaxDocumentType.SalesInvoice;
				case DrCr.Debit:
					return TaxDocumentType.ReturnInvoice;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}

		public virtual Sign GetDocumentSign(ARInvoice invoice)
		{
			switch (invoice.DrCr)
			{
				case DrCr.Credit:
					return Sign.Plus;
				case DrCr.Debit:
					return Sign.Minus;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}

		public virtual void ApplyTax(ARInvoice invoice, GetTaxResult result)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;
			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { invoice });
				vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(Base, taxZone.TaxVendorID);
				if (vendor == null)
					throw new PXException(TX.Messages.ExternalTaxVendorNotFound);

				if (vendor.SalesTaxAcctID == null)
					throw new PXException(TX.Messages.TaxPayableAccountNotSpecified, vendor.AcctCD);

				if (vendor.SalesTaxSubID == null)
					throw new PXException(TX.Messages.TaxPayableSubNotSpecified, vendor.AcctCD);
			}
			Sign sign = GetDocumentSign(invoice);

			//Clear all existing Tax transactions:
			foreach (PXResult<ARTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { invoice }))
			{
				ARTaxTran taxTran = res;
				Base.Taxes.Delete(taxTran);
			}

			Base.Views.Caches.Add(typeof(Tax));

			for (int i = 0; i < result.TaxSummary.Length; i++)
			{
				string taxID = result.TaxSummary[i].TaxName;
				if (string.IsNullOrEmpty(taxID))
					taxID = result.TaxSummary[i].JurisCode;

				if (string.IsNullOrEmpty(taxID))
				{
					PXTrace.WriteInformation(Messages.EmptyValuesFromExternalTaxProvider);
					continue;
				}

                CreateTax(Base, taxZone, vendor, result.TaxSummary[i], taxID);

				var tax = new ARTaxTran
						{
							Module = BatchModule.AR,
							TranType = invoice.DocType,
							RefNbr = invoice.RefNbr,
							TaxID = taxID,
							CuryTaxAmt = sign * result.TaxSummary[i].TaxAmount,
							CuryTaxableAmt = sign * result.TaxSummary[i].TaxableAmount,
							TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100,
							JurisType = result.TaxSummary[i].JurisType,
							JurisName = result.TaxSummary[i].JurisName,
							TaxType = CSTaxType.Sales,
							TaxBucketID = 0,
							AccountID = vendor.SalesTaxAcctID,
							SubID = vendor.SalesTaxSubID
						};

				Base.Taxes.Insert(tax);
			}

			bool requireControlTotal = Base.ARSetup.Current.RequireControlTotal == true;

			if (invoice.Hold != true)
				Base.ARSetup.Current.RequireControlTotal = false;

			try
			{
				invoice.CuryTaxTotal = sign * result.TotalTaxAmount;
				Base.Document.Cache.SetValueExt<ARInvoice.isTaxSaved>(invoice, true);
			}
			finally
			{
				Base.ARSetup.Current.RequireControlTotal = requireControlTotal;
			}

			if (invoice.ApplyPaymentWhenTaxAvailable == true)
			{
				PXSelectBase<ARAdjust2> select = new PXSelectJoin<ARAdjust2,
					InnerJoin<ARPayment, On<ARAdjust2.adjgDocType, Equal<ARPayment.docType>,
						And<ARAdjust2.adjgRefNbr, Equal<ARPayment.refNbr>>>>,
					Where<ARAdjust2.adjdDocType, Equal<Required<ARInvoice.docType>>,
						And<ARAdjust2.adjdRefNbr, Equal<Required<ARInvoice.refNbr>>>>>(Base);

				decimal amountApplied = 0m;
				foreach (PXResult<ARAdjust2, ARPayment> res in select.Select(invoice.DocType, invoice.RefNbr))
				{
					ARAdjust2 row = (ARAdjust2)res;
					ARPayment payment = (ARPayment)res;

					ARAdjust2 copy = PXCache<ARAdjust2>.CreateCopy(row);
					amountApplied += (copy.CuryAdjdAmt ?? 0m);

					if (amountApplied > (invoice.CuryDocBal ?? 0m))
					{
						decimal newAdjdAmt = (copy.CuryAdjdAmt ?? 0m) - (amountApplied - (invoice.CuryDocBal ?? 0m));
						copy.CuryAdjdAmt = newAdjdAmt > 0m ? newAdjdAmt : 0m;
					}
					Base.Adjustments.Update(copy);
				}
			}
		}

		public virtual void CancelTax(ARInvoice invoice, VoidReasonCode code)
		{
			string taxZoneID = ARInvoice.PK.Find(Base, invoice)?.TaxZoneID ?? invoice.TaxZoneID;

			var request = new VoidTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(taxZoneID, invoice.BranchID);
			request.Code = code;
			request.DocCode = $"AR.{invoice.DocType}.{invoice.RefNbr}";
			request.DocType = GetTaxDocumentType(invoice);

			var service = ExternalTax.TaxProviderFactory(Base, taxZoneID);
			if (service == null)
				return;

			var result = service.VoidTax(request);

			bool raiseError = false;
			if (!result.IsSuccess)
			{
				LogMessages(result);

				if (result.Messages.Any(t
					=> t.Contains("DocumentNotFoundError")
					|| t.Contains("The tax document could not be found.")))
				{
					//just ignore this error. There is no document to delete in avalara.
				}
				else
				{
					raiseError = true;
				}
			}

			if (raiseError)
			{
				throw new PXException(TX.Messages.FailedToDeleteFromExternalTaxProvider);
			}
			else
			{
				invoice.IsTaxSaved = false;
				invoice.IsTaxValid = false;
				if (Base.Document.Cache.GetStatus(invoice) == PXEntryStatus.Notchanged)
					Base.Document.Cache.SetStatus(invoice, PXEntryStatus.Updated);
			}
		}

		protected override string GetExternalTaxProviderLocationCode(ARInvoice invoice) => GetExternalTaxProviderLocationCode<ARTran, ARTran.FK.Invoice.SameAsCurrent, ARTran.siteID>(invoice);

		public virtual IAddressBase GetFromAddress(ARInvoice invoice)
		{
			var branch =
				PXSelectJoin<Branch,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
				InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
				Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
				.Select(Base, invoice.BranchID)
				.RowCast<Address>()
				.FirstOrDefault()
				.With(ValidAddressFrom<BAccountR.defAddressID>);

			return branch;
		}

		public virtual IAddressBase GetFromAddress(ARInvoice invoice, ARTran tran)
		{
			if (invoice.OrigModule == BatchModule.SO)
			{
				var soLine =
					PXSelectJoin<Address,
					InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
					InnerJoin<SOLine, On<SOLine.siteID, Equal<INSite.siteID>>>>,
					Where<SOLine.orderType, Equal<Current<ARTran.sOOrderType>>,
						And<SOLine.orderNbr, Equal<Current<ARTran.sOOrderNbr>>,
						And<SOLine.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>
					.SelectSingleBound(Base, new[] { tran }).AsEnumerable()
					.Cast<PXResult<Address, INSite, SOLine>>()
					.FirstOrDefault();

				return GetDropShipOriginAddress(tran, soLine)
					?? PXSelectJoin<Address,
						InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
						InnerJoin<SOShipLine, On<SOShipLine.siteID, Equal<INSite.siteID>,
							And<SOShipLine.shipmentType, Equal<Current<ARTran.sOShipmentType>>>>>>,
						Where<SOShipLine.shipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>,
							And<SOShipLine.lineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>>>>
						.SelectSingleBound(Base, new[] { tran }).AsEnumerable()
						.RowCast<Address>()
						.FirstOrDefault()
						.With(ValidAddressFrom<INSite.addressID>)
					?? (tran.LineType == SOLineType.Freight
						? PXSelectJoin<Address,
							InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
							InnerJoin<SOShipment, On<SOShipment.siteID, Equal<INSite.siteID>>>>,
							Where<SOShipment.shipmentType, Equal<Current<ARTran.sOShipmentType>>,
								And<SOShipment.shipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>>>>
							.SelectSingleBound(Base, new[] { tran })
							.RowCast<Address>()
							.FirstOrDefault()
							.With(ValidAddressFrom<INSite.addressID>)
						: null)
					?? ((Address)soLine).With(ValidAddressFrom<INSite.addressID>)
					?? GetFromAddress(invoice); // branch address
			}
			return GetFromAddress(invoice);
		}

		public virtual IAddressBase GetToAddress(ARInvoice invoice)
		{
			return ((ARShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { invoice }))
				.With(ValidAddressFrom<ARInvoice.shipAddressID>);
		}

		public virtual IAddressBase GetToAddress(ARInvoice invoice, ARTran tran)
		{
			if (invoice.OrigModule == BatchModule.SO)
			{
				var soLine =
					PXSelectJoin<SOAddress,
					InnerJoin<SOOrder, On<SOOrder.shipAddressID, Equal<SOAddress.addressID>>,
					LeftJoin<SOLine, On<SOLine.orderType, Equal<SOOrder.orderType>,
						And<SOLine.orderNbr, Equal<SOOrder.orderNbr>>>,
					LeftJoin<Carrier, On<Carrier.carrierID, Equal<SOOrder.shipVia>>>>>,
					Where<SOOrder.orderType, Equal<Current<ARTran.sOOrderType>>,
						And<SOOrder.orderNbr, Equal<Current<ARTran.sOOrderNbr>>,
						And<Where<Current2<ARTran.sOOrderLineNbr>, IsNull,
							Or<Current2<ARTran.sOOrderLineNbr>, Equal<SOLine.lineNbr>>>>>>>
					.SelectSingleBound(Base, new[] { tran }).AsEnumerable()
					.Cast<PXResult<SOAddress, SOOrder, SOLine, Carrier>>()
					.FirstOrDefault();

				var dropShipAddress = GetDropShipDestinationAddress(tran, soLine);
				if (dropShipAddress != null)
					return dropShipAddress;

				var shipLine =
					PXSelectJoin<SOAddress,
					InnerJoin<SOShipment, On<SOShipment.shipAddressID, Equal<SOAddress.addressID>>,
					LeftJoin<SOShipLine, On<SOShipLine.shipmentType, Equal<SOShipment.shipmentType>,
						And<SOShipLine.shipmentNbr, Equal<SOShipment.shipmentNbr>>>,
					LeftJoin<Carrier, On<Carrier.carrierID, Equal<SOShipment.shipVia>>>>>,
					Where<SOShipment.shipmentType, Equal<Current<ARTran.sOShipmentType>>,
						And<SOShipment.shipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>,
						And<Where<Current2<ARTran.sOShipmentLineNbr>, IsNull,
							Or<Current2<ARTran.sOShipmentLineNbr>, Equal<SOShipLine.lineNbr>>>>>>>
					.SelectSingleBound(Base, new[] { tran }).AsEnumerable()
					.Cast<PXResult<SOAddress, SOShipment, SOShipLine, Carrier>>()
					.FirstOrDefault();

				bool isCommonCarrier = shipLine == null
					? ((Carrier)soLine)?.IsCommonCarrier == true
					: ((Carrier)shipLine)?.IsCommonCarrier == true;
				if (tran.SiteID != null && isCommonCarrier == false)
					return GetFromAddress(invoice, tran);

				return (tran.LineType == SOLineType.Freight || tran.SOShipmentLineNbr != null ? (SOAddress)shipLine : null).With(ValidAddressFrom<SOShipment.shipAddressID>)
					?? (tran.LineType == SOLineType.Freight || tran.SOOrderLineNbr != null ? (SOAddress)soLine : null).With(ValidAddressFrom<SOOrder.shipAddressID>)
					?? GetToAddress(invoice);
			}

			return GetToAddress(invoice);
		}

		private IAddressBase GetDropShipOriginAddress(ARTran tran, SOLine soLine)
		{
			if (soLine != null && tran.SOOrderLineNbr != null)
			{
				Boolean isDropShip = soLine.POCreate == true && soLine.POSource == INReplenishmentSource.DropShipToOrder;
				Address vendorAddress = isDropShip
					? PXSelectJoin<Address,
						InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>,
						InnerJoin<PO.POReceipt, On<PO.POReceipt.vendorLocationID, Equal<Location.locationID>>>>,
						Where<PO.POReceipt.receiptNbr, Equal<Current<ARTran.sOShipmentNbr>>>>
						.SelectSingleBound(Base, new[] { tran })
						.RowCast<Address>()
						.FirstOrDefault()
					: null;

				if (vendorAddress != null)
					return vendorAddress.With(ValidAddressFrom<PO.POReceipt.vendorLocationID>);
			}
			return null;
		}

		private IAddressBase GetDropShipDestinationAddress(ARTran tran, SOLine soLine)
		{
			if (soLine != null && tran.SOOrderLineNbr != null)
			{
				Boolean isDropShip = soLine.POCreate == true && soLine.POSource == INReplenishmentSource.DropShipToOrder;
				IAddressBase dropShipAddress = isDropShip
					? PXSelectJoin<PO.POAddress,
						InnerJoin<PO.POOrder, On<PO.POOrder.shipAddressID, Equal<PO.POAddress.addressID>>,
						InnerJoin<PO.POReceiptLine, On<PO.POReceiptLine.pOType, Equal<PO.POOrder.orderType>,
							And<PO.POReceiptLine.pONbr, Equal<PO.POOrder.orderNbr>>>>>,
						Where<PO.POReceiptLine.receiptNbr, Equal<Current<ARTran.sOShipmentNbr>>,
							And<PO.POReceiptLine.lineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>>>>
						.SelectSingleBound(Base, new[] { tran })
						.RowCast<PO.POAddress>()
						.FirstOrDefault()
					: null;

				if (dropShipAddress != null)
					return dropShipAddress.With(ValidAddressFrom<PO.POOrder.shipAddressID>);
			}
			return null;
		}

		private IAddressBase ValidAddressFrom<TFieldSource>(IAddressBase address)
			where TFieldSource : IBqlField
		{
			if (!IsEmptyAddress(address)) return address;
			throw new PXException(PickAddressError<TFieldSource>(address));
		}

		private string PickAddressError<TFieldSource>(IAddressBase address)
			where TFieldSource : IBqlField
		{
			if (typeof(TFieldSource) == typeof(PO.POOrder.shipAddressID))
				return PXSelectReadonly<PO.POOrder, Where<PO.POOrder.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((PO.POAddress)address).AddressID).First().GetItem<PO.POOrder>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<PO.POOrder>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(PO.POReceipt.vendorLocationID))
				return PXSelectReadonly2<PO.POReceipt, InnerJoin<Location, On<Location.locationID, Equal<PO.POReceipt.vendorLocationID>>>, Where<Location.defAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<PO.POReceipt>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<PO.POReceipt>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(SOShipment.shipAddressID))
				return PXSelectReadonly<SOShipment, Where<SOShipment.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((SOAddress)address).AddressID).First().GetItem<SOShipment>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<SOShipment>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(SOOrder.shipAddressID))
				return PXSelectReadonly<SOOrder, Where<SOOrder.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((SOAddress)address).AddressID).First().GetItem<SOOrder>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<SOOrder>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(ARInvoice.shipAddressID))
				return PXSelect<ARInvoice, Where<ARInvoice.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((ARAddress)address).AddressID).First().GetItem<ARInvoice>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<ARInvoice>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(INSite.addressID))
				return PXSelectReadonly<INSite, Where<INSite.addressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<INSite>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<INSite>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(BAccountR.defAddressID))
				return PXSelectReadonly<BAccountR, Where<BAccountR.defAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<BAccountR>()
					.With(e => PXMessages.LocalizeFormat(Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<BAccountR>(), new EntityHelper(Base).GetRowID(e)));

			throw new ArgumentOutOfRangeException("Unknown address source used");
		}

		protected override decimal? GetDocDiscount()
		{
			return Base.Document.Current.CuryDiscTot;
		}
	}
} 