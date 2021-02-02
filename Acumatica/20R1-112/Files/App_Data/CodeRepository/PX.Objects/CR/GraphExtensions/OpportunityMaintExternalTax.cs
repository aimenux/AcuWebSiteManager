using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;

namespace PX.Objects.CR
{
	public class OpportunityMaintExternalTax : ExternalTaxBase<OpportunityMaint, CROpportunity>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		[PXOverride]
		public void Persist()
		{
			if (Base.Opportunity.Current != null && IsExternalTax(Base.Opportunity.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Opportunity.Current.IsTaxValid != true)
			{
				if (PXLongOperation.GetCurrentItem() == null)
				{
					PXLongOperation.StartOperation(Base, delegate ()
					{
						CalculateExternalTax(Base.Opportunity.Current);
					});
				}
				else
				{
					CalculateExternalTax(Base.Opportunity.Current);
				}
			}
		}

		public override void SkipTaxCalcAndSave()
		{
			try
			{
				skipExternalTaxCalcOnSave = true;
				Base.Save.Press();
			}
			finally
			{
				skipExternalTaxCalcOnSave = false;
			}
		}

		#region CROpportunity Events
		protected virtual void _(Events.RowSelected<CROpportunity> e)
		{
			if (e.Row == null)
				return;

			if (IsExternalTax(e.Row.TaxZoneID) && e.Row.IsTaxValid != true)
				PXUIFieldAttribute.SetWarning<CROpportunity.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
		}
		#endregion

		#region CROpportunityProducts Events
		protected virtual void _(Events.RowInserted<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}

		protected virtual void _(Events.RowDeleted<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		protected virtual void _(Events.RowUpdated<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}

		protected virtual void _(Events.RowUpdated<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		protected virtual void _(Events.RowDeleted<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		protected virtual void _(Events.RowInserted<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		#endregion

		#region CRShippingAddress Events

		protected virtual void _(Events.RowUpdated<CRShippingAddress> e)
		{
			if (e.Row != null
				&& e.Cache.ObjectsEqual<CRShippingAddress.postalCode, CRShippingAddress.countryID, CRShippingAddress.state>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		protected virtual void _(Events.RowInserted<CRShippingAddress> e)
		{
			if (e.Row != null && Base.Opportunity.Current != null)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<CRShippingAddress> e)
		{
			if (e.Row != null && Base.Opportunity.Current != null)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		#endregion

		public virtual void InvalidateExternalTax(CROpportunity doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
			{
				doc.IsTaxValid = false;
				Base.Opportunity.Cache.MarkUpdated(doc);
			}
		}

		public override CROpportunity CalculateExternalTax(CROpportunity order)
		{
			var toAddress = GetToAddress(order);
			bool isNonTaxable = IsNonTaxable(toAddress);

			if (isNonTaxable || order.BAccountID == null)
			{
				ApplyTax(order, GetTaxResult.Empty);
				order.IsTaxValid = true;
				order = Base.Opportunity.Update(order);
				
				SkipTaxCalcAndSave();

				return order;
			}

			var service = TaxProviderFactory(Base, order.TaxZoneID);

			GetTaxRequest getRequest = null;
			bool isValidByDefault = true;

			if (order.IsTaxValid != true)
			{
				getRequest = BuildGetTaxRequest(order);

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
				order.IsTaxValid = true;
				order = Base.Opportunity.Update(order);
				SkipTaxCalcAndSave();
				
				return order;
			}

			GetTaxResult result = service.GetTax(getRequest);
			if (result.IsSuccess)
			{
				try
				{
					ApplyTax(order, result);

					order.IsTaxValid = true;
					order = Base.Opportunity.Update(order);
					SkipTaxCalcAndSave();
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
					throw new PXException(ex, TX.Messages.FailedToApplyTaxes);
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}

			return order;
		}

		protected virtual GetTaxRequest BuildGetTaxRequest(CROpportunity order)
		{
			if (order == null)
				throw new PXArgumentException(nameof(order));

			BAccount cust = (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(Base, order.BAccountID);
			Location loc = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
				Select(Base, order.BAccountID, order.LocationID);

			IAddressBase addressFrom = GetFromAddress();
			IAddressBase addressTo = GetToAddress(order);

			if (addressFrom == null)
				throw new PXException(Messages.FailedGetFromAddressCR);

			if (addressTo == null)
				throw new PXException(Messages.FailedGetToAddressCR);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, Base.Accessinfo.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = cust?.AcctCD;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(addressFrom);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(addressTo);
			request.DocCode = $"CR.{order.OpportunityID}";
			request.DocDate = order.CloseDate.GetValueOrDefault();
			request.Discount = order.CuryLineDocDiscountTotal.GetValueOrDefault();

			int mult = 1;

			if (loc != null)
			{
				request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
			}
			if (!string.IsNullOrEmpty(loc?.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}

			request.DocType = TaxDocumentType.SalesOrder;

			PXSelectBase<CROpportunityProducts> select = new PXSelectJoin<CROpportunityProducts,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
				OrderBy<Asc<CROpportunityProducts.lineNbr>>>(Base);

			foreach (PXResult<CROpportunityProducts, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				CROpportunityProducts tran = (CROpportunityProducts)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.Amount = mult * tran.CuryAmount.GetValueOrDefault();
				line.Description = tran.Descr;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(tran.Qty.GetValueOrDefault());
				line.Discounted = request.Discount > 0;
				line.RevAcct = salesAccount.AccountCD;
				line.TaxCode = tran.TaxCategoryID;

				request.CartItems.Add(line);
			}

			return request;
		}

		protected virtual void CalcCuryProductsAmount(CROpportunity order, ref decimal? curyProductsAmount) { }

		protected virtual void ApplyTax(CROpportunity order, GetTaxResult result)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;

			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<CROpportunity.taxZoneID>>>>.Select(Base, order.TaxZoneID);
				vendor = (AP.Vendor)PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(Base, taxZone.TaxVendorID);

				if (vendor == null)
					throw new PXException(Messages.ExternalTaxVendorNotFound);
			}

			//Clear all existing Tax transactions:
			foreach (PXResult<CRTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { order }))
			{
				CRTaxTran taxTran = (CRTaxTran)res;
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

				CRTaxTran tax = new CRTaxTran();
				tax.QuoteID = order.QuoteNoteID;
				tax.TaxID = taxID;
				tax.CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount);
				tax.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount);
				tax.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100;

				Base.Taxes.Insert(tax);
			}

			Base.Opportunity.SetValueExt<CROpportunity.curyTaxTotal>(order, Math.Abs(result.TotalTaxAmount));

			decimal? СuryProductsAmount =
				order.ManualTotalEntry == true
				? order.CuryAmount - order.CuryDiscTot
				: order.CuryLineTotal - order.CuryDiscTot + order.CuryTaxTotal;

			CalcCuryProductsAmount(order, ref СuryProductsAmount);

			Base.Opportunity.SetValueExt<CROpportunity.curyProductsAmount>(order, СuryProductsAmount ?? 0m);
		}

		protected IAddressBase GetFromAddress()
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccount.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccount, Address> res in select.Select(Base.Accessinfo.BranchID))
				return (Address)res;

			return null;
		}

		protected IAddressBase GetToAddress(CROpportunity order)
		{
			var crShipAddress = (CRShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { order });

			if (crShipAddress != null)
				return crShipAddress;

			Address shipAddress = null;

			Location loc = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
				Select(Base, order.BAccountID, order.LocationID);
			if (loc != null)
			{
				shipAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, loc.DefAddressID);
			}

			return shipAddress;
		}
	}
}
