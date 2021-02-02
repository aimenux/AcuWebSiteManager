using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.Objects.CS;
using PX.TaxProvider;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AP
{
    public class APInvoiceEntryExternalTax : ExternalTax<APInvoiceEntry, APInvoice>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
        }

        public override APInvoice CalculateExternalTax(APInvoice invoice)
        {
            if (invoice.InstallmentNbr != null)
            {
                //do not calculate tax for installments
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
                Base.Document.SetValueExt<APInvoice.nonTaxable>(invoice, false);
            }

            var service = TaxProviderFactory(Base, invoice.TaxZoneID);

            var taxRequest = BuildTaxRequest(invoice);

			if (taxRequest.CartItems.Count == 0)
			{
                ApplyTax(invoice, GetTaxResult.Empty); // Invoice without APTran. Clear APTax.
				invoice.IsTaxValid = true;
				invoice.IsTaxSaved = false;
				invoice = Base.Document.Update(invoice);

				SkipTaxCalcAndSave();

				return invoice;
			}

			var result = service.GetTax(taxRequest);
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

                var request = new PX.TaxProvider.PostTaxRequest();
                request.CompanyCode = taxRequest.CompanyCode;
                request.DocCode = taxRequest.DocCode;
                request.DocDate = taxRequest.DocDate;
                request.DocType = taxRequest.DocType;
                request.TotalAmount = result.TotalAmount;
                request.TotalTaxAmount = result.TotalTaxAmount;
                var postResult = service.PostTax(request);
                if (postResult.IsSuccess)
                {
                    invoice.IsTaxValid = true;
                    invoice = Base.Document.Update(invoice);
                    SkipTaxCalcAndSave();
                }

            }
            else
            {
                PXTrace.WriteError(String.Join(", ", result.Messages));

                throw new PXException(TX.Messages.FailedToGetTaxes);
            }

            return invoice;
        }

        [PXOverride]
        public virtual void Persist()
        {
            if (Base.Document.Current != null &&
                IsExternalTax(Base.Document.Current.TaxZoneID) &&
                Base.Document.Current.InstallmentNbr == null &&
                Base.Document.Current.IsTaxValid != true &&
                !skipExternalTaxCalcOnSave)
            {
                if (PXLongOperation.GetCurrentItem() == null)
                {
                    PXLongOperation.StartOperation(Base, delegate
                    {
                        APInvoiceEntry rg = PXGraph.CreateInstance<APInvoiceEntry>();
                        rg.Document.Current = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(rg, Base.Document.Current.DocType, Base.Document.Current.RefNbr);
                        rg.CalculateExternalTax(rg.Document.Current);
                    });
                }
                else
                {
                    Base.CalculateExternalTax(Base.Document.Current);
                }
            }
        }

        [PXOverride]
        public virtual APRegister OnBeforeRelease(APRegister doc)
        {
            skipExternalTaxCalcOnSave = true;

            return doc;
        }

        protected virtual void _(Events.RowSelected<APInvoice> e)
        {
            if (e.Row == null)
                return;

            if (IsExternalTax(e.Row.TaxZoneID) && e.Row.IsTaxValid != true)
                PXUIFieldAttribute.SetWarning<APInvoice.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
        }

        protected virtual void _(Events.RowPersisting<APInvoice> e)
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

        protected virtual void _(Events.RowUpdated<APTran> e)
        {
            //if any of the fields that was saved in External Tax Provider has changed mark doc as TaxInvalid.
            if (IsDocumentExtTaxValid(Base.Document.Current) && !e.Cache.ObjectsEqual<APTran.accountID, APTran.inventoryID, APTran.tranAmt, APTran.tranDate, APTran.taxCategoryID>(e.Row, e.OldRow))
            {
                Base.Document.Current.IsTaxValid = false;
                Base.Document.Update(Base.Document.Current);
            }
        }

        public virtual bool IsDocumentExtTaxValid(APInvoice doc)
        {
            return doc != null && IsExternalTax(doc.TaxZoneID) && doc.InstallmentNbr == null;
        }


        protected virtual void _(Events.RowDeleted<APTran> e)
        {
            Base.Document.Current.IsTaxValid = !IsDocumentExtTaxValid(Base.Document.Current);
        }

        protected virtual void _(Events.RowInserted<APTran> e)
        {
            if (IsDocumentExtTaxValid(Base.Document.Current))
            {
                Base.Document.Current.IsTaxValid = false;
                Base.Document.Cache.MarkUpdated(Base.Document.Current);
            }
        }

        protected virtual void _(Events.RowUpdated<APInvoice> e)
        {
            //Recalculate taxes when document date changed
            if (e.Row.Released != true)
            {
                if (IsDocumentExtTaxValid(e.Row) && !e.Cache.ObjectsEqual<APInvoice.curyDiscountedTaxableTotal, APInvoice.docDate>(e.Row, e.OldRow))
                {
                    e.Row.IsTaxValid = false;
                }
            }
        }

        protected virtual GetTaxRequest BuildTaxRequest(APInvoice invoice)
        {
            if (invoice == null) throw new PXArgumentException(nameof(invoice), ErrorMessages.ArgumentNullException);

            Vendor vend = (Vendor)Base.vendor.View.SelectSingleBound(new object[] { invoice });

            var request = new PX.TaxProvider.GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(invoice.TaxZoneID, invoice.BranchID);
            request.CurrencyCode = invoice.CuryID;
            request.CustomerCode = vend.AcctCD;
            IAddressBase fromAddress = GetFromAddress(invoice);
            IAddressBase toAddress = GetToAddress(invoice);

            if (fromAddress == null)
                throw new PXException(Messages.FailedGetFrom);

            if (toAddress == null)
                throw new PXException(Messages.FailedGetTo);

            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = $"AP.{invoice.DocType}.{invoice.RefNbr}";
            request.DocDate = invoice.DocDate.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(invoice);

            Location branchLoc = GetBranchLocation(invoice);

            if (branchLoc != null)
            {
                request.CustomerUsageType = branchLoc.CAvalaraCustomerUsageType;
                request.ExemptionNo = branchLoc.CAvalaraExemptionNumber;
            }

			request.DocType = GetTaxDocumentType(invoice);
			Sign sign = GetDocumentSign(invoice);

            PXSelectBase<APTran> select = new PXSelectJoin<APTran,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APTran.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<APTran.accountID>>>>,
                Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
                    And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
                    And<APTran.lineType, NotEqual<SOLineType.discount>>>>,
                OrderBy<Asc<APTran.tranType, Asc<APTran.refNbr, Asc<APTran.lineNbr>>>>>(Base);

            request.Discount = GetDocDiscount().GetValueOrDefault();
            foreach (PXResult<APTran, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { invoice }))
            {
                APTran tran = (APTran)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

                var line = new TaxCartItem();
                line.Index = tran.LineNbr.GetValueOrDefault();
				line.Amount = sign * tran.CuryTranAmt.GetValueOrDefault();
                line.Description = tran.TranDesc;
                line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(invoice, tran));
                line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(invoice, tran));
                line.ItemCode = item.InventoryCD;
                line.Quantity = Math.Abs(tran.Qty.GetValueOrDefault());
                line.Discounted = request.Discount > 0;
                line.RevAcct = salesAccount.AccountCD;

                line.TaxCode = tran.TaxCategoryID;

                request.CartItems.Add(line);
            }

			if (invoice.DocType == APDocType.DebitAdj && (invoice.OrigDocDate != null))
			{
				request.TaxOverride.Reason = Messages.DebitAdjustmentReason;
				request.TaxOverride.TaxDate = invoice.OrigDocDate.Value;
				request.TaxOverride.TaxOverrideType = PX.TaxProvider.TaxOverrideType.TaxDate;
			}

            return request;
		}

		public virtual TaxDocumentType GetTaxDocumentType(APInvoice invoice)
		{
			switch (invoice.DrCr)
			{
				case DrCr.Debit:
					return TaxDocumentType.PurchaseInvoice;
				case DrCr.Credit:
					return TaxDocumentType.ReturnInvoice;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}

		public virtual Sign GetDocumentSign(APInvoice invoice)
		{
			switch (invoice.DrCr)
			{
				case DrCr.Debit:
					return Sign.Plus;
				case DrCr.Credit:
					return Sign.Minus;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
        }

        protected virtual void ApplyTax(APInvoice invoice, GetTaxResult result)
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
                    throw new PXSetPropertyException(TX.Messages.TaxPayableAccountNotSpecified, vendor.AcctCD);

                if (vendor.SalesTaxSubID == null)
                    throw new PXException(TX.Messages.TaxPayableSubNotSpecified, vendor.AcctCD);
            }
            //Clear all existing Tax transactions:
            foreach (PXResult<APTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { invoice }))
            {
                APTaxTran taxTran = (APTaxTran)res;
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

                APTaxTran tax = new APTaxTran();
                tax.Module = BatchModule.AP;
                tax.TranType = invoice.DocType;
                tax.RefNbr = invoice.RefNbr;
                tax.TaxID = taxID;
                tax.CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount);
                tax.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount);
                tax.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100;
                tax.TaxType = "S";
                tax.TaxBucketID = 0;
                tax.AccountID = vendor.SalesTaxAcctID;
                tax.SubID = vendor.SalesTaxSubID;
                tax.JurisType = result.TaxSummary[i].JurisType;
                tax.JurisName = result.TaxSummary[i].JurisName;

                Base.Taxes.Insert(tax);
            }

            bool requireControlTotal = Base.APSetup.Current.RequireControlTotal == true;
            if (invoice.Hold != true)
                Base.APSetup.Current.RequireControlTotal = false;

            try
            {
                invoice.CuryTaxTotal = Math.Abs(result.TotalTaxAmount);
                Base.Document.Cache.SetValueExt<APInvoice.isTaxSaved>(invoice, true);
            }
            finally
            {
                Base.APSetup.Current.RequireControlTotal = requireControlTotal;
            }
        }

        protected virtual void CancelTax(APInvoice invoice, VoidReasonCode code)
        {
			string taxZoneID = APInvoice.PK.Find(Base, invoice)?.TaxZoneID ?? invoice.TaxZoneID;

            var request = new VoidTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(taxZoneID, invoice.BranchID);
            request.Code = code;
            request.DocCode = $"AP.{invoice.DocType}.{invoice.RefNbr}";
			request.DocType = GetTaxDocumentType(invoice);

            var service = TaxProviderFactory(Base, taxZoneID);
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

		protected override string GetExternalTaxProviderLocationCode(APInvoice invoice) => GetExternalTaxProviderLocationCode<APTran, APTran.FK.Invoice.SameAsCurrent, APTran.siteID>(invoice);

        protected virtual IAddressBase GetToAddress(APInvoice invoice)
        {
            return
                PXSelectJoin<Branch,
                InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
                    InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
                Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
                .Select(Base, invoice.BranchID)
                .RowCast<Address>()
                .FirstOrDefault();
        }

        protected virtual IAddressBase GetToAddress(APInvoice invoice, APTran tran)
        {
            var poLine =
                PXSelectJoin<Address,
                InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
                InnerJoin<POLine, On<POLine.siteID, Equal<INSite.siteID>>>>,
                Where<POLine.orderType, Equal<Current<APTran.pOOrderType>>,
                    And<POLine.orderNbr, Equal<Current<APTran.pONbr>>,
                    And<POLine.lineNbr, Equal<Current<APTran.pOLineNbr>>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .Cast<PXResult<Address, INSite, POLine>>()
                .FirstOrDefault();

            var poAddress =
                PXSelectJoin<POShipAddress,
                InnerJoin<POOrder, On<POShipAddress.addressID, Equal<POOrder.shipAddressID>>>,
                Where<POOrder.orderType, Equal<Current<APTran.pOOrderType>>,
                    And<POOrder.orderNbr, Equal<Current<APTran.pONbr>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .RowCast<POShipAddress>()
                .FirstOrDefault();

            if (poLine?.GetItem<POLine>()?.OrderType == POOrderType.DropShip)
                return poAddress;

            return
                PXSelectJoin<Address,
                InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
                InnerJoin<POReceiptLine, On<POReceiptLine.siteID, Equal<INSite.siteID>>>>,
                Where<POReceiptLine.receiptType, Equal<Current<APTran.receiptType>>,
                    And<POReceiptLine.receiptNbr, Equal<Current<APTran.receiptNbr>>,
                    And<POReceiptLine.lineNbr, Equal<Current<APTran.receiptLineNbr>>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .RowCast<Address>()
                .FirstOrDefault()
                ?? poLine?.GetItem<Address>()
                ?? poAddress
                ?? GetToAddress(invoice);
        }

        protected virtual Location GetBranchLocation(APInvoice invoice)
        {
            PXSelectBase<Branch> select = new PXSelectJoin
                <Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
                    InnerJoin<Location, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>>>,
                    Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

            foreach (PXResult<Branch, BAccountR, Location> res in select.Select(invoice.BranchID))
                return (Location)res;

            return null;
        }

        protected virtual IAddressBase GetFromAddress(APInvoice invoice)
        {
            return
                PXSelectJoin<Address,
                InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>>,
                Where<Location.locationID, Equal<Required<Location.locationID>>>>
                .SelectWindowed(Base, 0, 1, new object[]
                {
                    PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>()
                        ? invoice.SuppliedByVendorLocationID
                        : invoice.VendorLocationID
                })
                .RowCast<Address>()
                .FirstOrDefault();
        }

        protected virtual IAddressBase GetFromAddress(APInvoice invoice, APTran tran)
        {
            return (IAddressBase)
                PXSelectJoin<PORemitAddress,
                InnerJoin<POOrder, On<PORemitAddress.addressID, Equal<POOrder.remitAddressID>>>,
                Where<POOrder.orderType, Equal<Current<APTran.pOOrderType>>,
                    And<POOrder.orderNbr, Equal<Current<APTran.pONbr>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .RowCast<PORemitAddress>()
                .FirstOrDefault()
                ??
                PXSelectJoin<Address,
                InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>,
                InnerJoin<POReceipt, On<POReceipt.vendorLocationID, Equal<Location.locationID>>,
                InnerJoin<POReceiptLine, On<
                    POReceiptLine.receiptType, Equal<POReceipt.receiptType>,
                    And<POReceiptLine.receiptNbr, Equal<POReceipt.receiptNbr>>>>>>,
                Where<POReceiptLine.receiptType, Equal<Current<APTran.receiptType>>,
                    And<POReceiptLine.receiptNbr, Equal<Current<APTran.receiptNbr>>,
                    And<POReceiptLine.lineNbr, Equal<Current<APTran.receiptLineNbr>>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .RowCast<Address>()
                .FirstOrDefault()
                ??
                PXSelectJoin<Address,
                InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>>,
                Where<Location.locationID, Equal<Current<APInvoice.vendorLocationID>>>>
                .SelectSingleBound(Base, new[] { invoice })
                .RowCast<Address>()
                .FirstOrDefault(); ;
        }
    }
}
