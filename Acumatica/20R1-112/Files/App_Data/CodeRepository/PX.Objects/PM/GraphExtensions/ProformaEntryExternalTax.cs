using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;

namespace PX.Objects.PM
{
	public class ProformaEntryExternalTax : ExternalTax<ProformaEntry, PMProforma>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public override PMProforma CalculateExternalTax(PMProforma doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
				return CalculateExternalTax(doc, false);

			return doc;
		}

		public virtual PMProforma CalculateExternalTax(PMProforma doc, bool forceRecalculate)
		{
			var toAddress = GetToAddress(doc);

			GetTaxRequest getRequest = null;
			bool isValidByDefault = true;
			if ((doc.IsTaxValid != true || forceRecalculate) && !IsNonTaxable(toAddress))
			{
				getRequest = BuildGetTaxRequest(doc);

				if (getRequest.CartItems.Count > 0)
				{
					isValidByDefault = false;
				}
				else
				{
					getRequest = null;
				}
			}

			if (isValidByDefault)
			{
				doc.CuryTaxTotal = 0;
				doc.IsTaxValid = true;
				Base.Document.Update(doc);

				foreach (PMTaxTran item in Base.Taxes.Select())
				{
					Base.Taxes.Delete(item);
				}

				using (var ts = new PXTransactionScope())
				{
					Base.Persist(typeof(PMTaxTran), PXDBOperation.Delete);
					Base.Persist(typeof(PMProforma), PXDBOperation.Update);
					PXTimeStampScope.PutPersisted(Base.Document.Cache, doc, PXDatabase.SelectTimeStamp());
					ts.Complete();
				}
				return doc;
			}

			GetTaxResult result = null;
			var service = TaxProviderFactory(Base, doc.TaxZoneID);
			bool getTaxFailed = false;
			if (getRequest != null)
			{
				result = service.GetTax(getRequest);
				if (!result.IsSuccess)
				{
					getTaxFailed = true;
				}
			}

			if (!getTaxFailed)
			{
				try
				{
					ApplyTax(doc, result);
					using (var ts = new PXTransactionScope())
					{
						doc.IsTaxValid = true;
						Base.Document.Update(doc);
						Base.Persist(typeof(PMProforma), PXDBOperation.Update);
						PXTimeStampScope.PutPersisted(Base.Document.Cache, doc, PXDatabase.SelectTimeStamp());
						ts.Complete();
					}
				}
				catch (PXOuterException ex)
				{
					string msg = TX.Messages.FailedToApplyTaxes;
					foreach (string err in ex.InnerMessages)
					{
						msg += Environment.NewLine + err;
					}

					throw new PXException(ex, msg);
				}
				catch (Exception ex)
				{
					string msg = TX.Messages.FailedToApplyTaxes;
					msg += Environment.NewLine + ex.Message;

					throw new PXException(ex, msg);
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}

			return doc;
		}

		public virtual void RecalculateExternalTaxes()
		{
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Document.Current.IsTaxValid != true)
			{
				if (Base.RecalculateExternalTaxesSync)
				{
					CalculateExternalTax(Base.Document.Current);
				}
				else
				{
					PXLongOperation.StartOperation(Base, delegate ()
					{
						PMProforma doc = new PMProforma();
						doc.RefNbr = Base.Document.Current.RefNbr;
						PMExternalTaxCalc.Process(doc);

					});
				}
			}
		}

		[PXOverride]
		public virtual void Persist(Action basePersist)
		{
			if (Base.RecalculateExternalTaxesSync)
			{
				RecalculateExternalTaxes();
				basePersist();
			}
			else
			{
				basePersist();
				RecalculateExternalTaxes();
			}
		}

		[PXOverride]
		public virtual void InitalizeActionsMenu()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>())
			{
				Base.action.AddMenuAction(recalcExternalTax);
			}
		}

		public PXAction<PMProforma> recalcExternalTax;
		[PXUIField(DisplayName = "Recalculate External Tax", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable RecalcExternalTax(PXAdapter adapter)
		{
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID))
			{
				var proforma = Base.Document.Current;
				CalculateExternalTax(Base.Document.Current, true);

				Base.Clear(PXClearOption.ClearAll);
				Base.Document.Current = Base.Document.Search<PMProforma.refNbr>(proforma.RefNbr);

				yield return Base.Document.Current;
			}
			else
			{
				foreach (var res in adapter.Get())
				{
					yield return res;
				}
			}
		}

		protected virtual void _(Events.RowUpdated<PMProforma> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (IsExternalTax(e.Row.TaxZoneID))
			{
				if (e.Row.InvoiceDate != e.OldRow.InvoiceDate ||
				    e.Row.TaxZoneID != e.OldRow.TaxZoneID ||
				    e.Row.CustomerID != e.OldRow.CustomerID ||
				    e.Row.LocationID != e.OldRow.LocationID ||
				    e.Row.AvalaraCustomerUsageType != e.OldRow.AvalaraCustomerUsageType ||
					e.Row.ShipAddressID != e.OldRow.ShipAddressID)
				{
					e.Row.IsTaxValid = false;
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMProforma> e)
		{
			if (Base.Document.Current == null)
				return;

			bool isEditable = Base.CanEditDocument(e.Row);
			bool isExternalTax = IsExternalTax(Base.Document.Current.TaxZoneID);

			Base.Taxes.Cache.AllowInsert = isEditable && !isExternalTax && e.Row.Hold == true;
			Base.Taxes.Cache.AllowUpdate = isEditable && !isExternalTax && e.Row.Hold == true;
			Base.Taxes.Cache.AllowDelete = isEditable && !isExternalTax && e.Row.Hold == true;

			if (isExternalTax && e.Row.IsTaxValid != true)
			{
				PXUIFieldAttribute.SetWarning<PMProforma.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
			}
		}

		protected virtual void _(Events.RowUpdated<PMProformaTransactLine> e)
		{
			InvalidateTax(e.Row, e.OldRow);
		}

		protected virtual void _(Events.RowUpdated<PMProformaProgressLine> e)
		{
			InvalidateTax(e.Row, e.OldRow);
		}

		public virtual void InvalidateTax(PMProformaLine row, PMProformaLine oldRow)
		{
			if (Base.Document.Current != null)
			{
				if (row.AccountID != oldRow.AccountID ||
				    row.InventoryID != oldRow.InventoryID ||
				    row.LineTotal != oldRow.LineTotal ||
				    row.TaxCategoryID != oldRow.TaxCategoryID ||
				    row.Description != oldRow.Description)
				{
					InvalidateExternalTax(Base.Document.Current);
				}
			}
		}

		#region PMShippingAddress Events

		protected virtual void _(Events.RowUpdated<PMShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null
				&& e.Cache.ObjectsEqual<PMShippingAddress.postalCode, PMShippingAddress.countryID, PMShippingAddress.state>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowInserted<PMShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<PMShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.FieldUpdating<PMShippingAddress, PMShippingAddress.overrideAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#endregion

		private void InvalidateExternalTax(PMProforma doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
			{
				doc.IsTaxValid = false;
				Base.Document.Cache.MarkUpdated(doc);
			}
		}

		public virtual GetTaxRequest BuildGetTaxRequest(PMProforma doc)
		{
			if (doc == null)
				throw new PXArgumentException(nameof(doc));

			Customer cust = (Customer)Base.Customer.View.SelectSingleBound(new object[] { doc });
			Location loc = (Location)Base.Location.View.SelectSingleBound(new object[] { doc });

			IAddressBase fromAddress = GetFromAddress(doc);
			IAddressBase toAddress = GetToAddress(doc);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddress);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddress);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(doc.TaxZoneID, doc.BranchID);
			request.CurrencyCode = doc.CuryID;
			request.CustomerCode = cust.AcctCD;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = $"PM.{doc.RefNbr}";
			request.DocDate = doc.InvoiceDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(doc);
			request.DocType = TaxDocumentType.SalesOrder;
			request.CustomerUsageType = doc.AvalaraCustomerUsageType;

			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}

			foreach (PMProformaProgressLine tran in Base.ProgressiveLines.Select())
			{
				InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMProformaProgressLine.inventoryID>(Base.ProgressiveLines.Cache, tran);
				Account salesAccount = (Account)PXSelectorAttribute.Select<PMProformaProgressLine.accountID>(Base.ProgressiveLines.Cache, tran);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;

				line.Amount = tran.CuryLineTotal.GetValueOrDefault();

				line.Description = tran.Description;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				if (item != null)
					line.ItemCode = item.InventoryCD;
				line.Quantity = tran.Qty.GetValueOrDefault();
				line.Discounted = request.Discount > 0;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;

				request.CartItems.Add(line);
			}

			foreach (PMProformaTransactLine tran in Base.TransactionLines.Select())
			{
				InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMProformaTransactLine.inventoryID>(Base.TransactionLines.Cache, tran);
				Account salesAccount = (Account)PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(Base.TransactionLines.Cache, tran);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;

				line.Amount = tran.CuryLineTotal.GetValueOrDefault();

				line.Description = tran.Description;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				if (item != null)
					line.ItemCode = item.InventoryCD;
				line.Quantity = tran.Qty.GetValueOrDefault();
				line.Discounted = request.Discount > 0;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;

				request.CartItems.Add(line);
			}

			return request;
		}

		public virtual void ApplyTax(PMProforma doc, GetTaxResult result)
		{
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { doc });
			if (taxZone == null)
			{
				throw new PXException(SO.Messages.TaxZoneIsNotSet);
			}

			AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(Base, taxZone.TaxVendorID);

			if (vendor == null)
				throw new PXException(TX.Messages.ExternalTaxVendorNotFound);

			if (result != null)
			{
				//Clear all existing Tax transactions:
				PXSelectBase<PMTaxTran> TaxesSelect =
					new PXSelectJoin<PMTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<PMTaxTran.taxID>>>,
						Where<PMTaxTran.refNbr, Equal<Current<PMProforma.refNbr>>>>(Base);
				foreach (PXResult<PMTaxTran, Tax> res in TaxesSelect.View.SelectMultiBound(new object[] { doc }))
				{
					PMTaxTran taxTran = (PMTaxTran)res;
					Base.Taxes.Delete(taxTran);
				}

				Base.Views.Caches.Add(typeof(Tax));

				var taxDetails = new List<PX.TaxProvider.TaxDetail>();
				for (int i = 0; i < result.TaxSummary.Length; i++)
					taxDetails.Add(result.TaxSummary[i]);

				foreach (var taxDetail in taxDetails)
				{
					string taxID = taxDetail.TaxName;
					if (string.IsNullOrEmpty(taxID))
						taxID = taxDetail.JurisCode;

					if (string.IsNullOrEmpty(taxID))
					{
						PXTrace.WriteInformation(SO.Messages.EmptyValuesFromExternalTaxProvider);
						continue;
					}

                    CreateTax(Base, taxZone, vendor, taxDetail, taxID);

					PMTaxTran tax = new PMTaxTran();
					tax.RefNbr = doc.RefNbr;
					tax.TaxID = taxID;
					tax.CuryTaxAmt = taxDetail.TaxAmount;
					tax.CuryTaxableAmt = taxDetail.TaxableAmount;
					tax.TaxRate = Convert.ToDecimal(taxDetail.Rate) * 100;
					tax.JurisType = taxDetail.JurisType;
					tax.JurisName = taxDetail.JurisName;

					Base.Taxes.Insert(tax);

				}

				Base.Document.SetValueExt<PMProforma.curyTaxTotal>(doc, result.TotalTaxAmount);
			}

			Base.Document.Update(doc);
			SkipTaxCalcAndSave();
		}

		public virtual IAddressBase GetFromAddress(PMProforma doc)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccountR, Address> res in select.Select(doc.BranchID))
				return (Address)res;

			return null;
		}

		public virtual IAddressBase GetToAddress(PMProforma doc)
		{
			return (PMShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { doc });
		}

		public virtual bool IsSame(GetTaxRequest x, GetTaxRequest y)
		{
			if (x.CartItems.Count != y.CartItems.Count)
				return false;

			for (int i = 0; i < x.CartItems.Count; i++)
			{
				if (x.CartItems[i].Amount != y.CartItems[i].Amount)
					return false;
			}

			return true;

		}
	}
}
