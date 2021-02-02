using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;

namespace PX.Objects.PO
{
	public class POOrderEntryExternalTax : ExternalTax<POOrderEntry, POOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public override POOrder CalculateExternalTax(POOrder order)
		{
			var toAddress = GetToAddress(order);
			bool isNonTaxable = IsNonTaxable(toAddress);

			if (isNonTaxable)
			{
				order.IsTaxValid = true;
				order.IsUnbilledTaxValid = true;
				ApplyTax(order, GetTaxResult.Empty, GetTaxResult.Empty);

				return order;
			}

			var service = TaxProviderFactory(Base, order.TaxZoneID);

			GetTaxRequest getRequest = null;
			GetTaxRequest getRequestUnbilled = null;

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

			if (order.IsUnbilledTaxValid != true)
			{
				getRequestUnbilled = BuildGetTaxRequestUnbilled(order);
				if (getRequestUnbilled.CartItems.Count > 0)
				{
					isValidByDefault = false;
				}
				else
				{
					getRequestUnbilled = null;
				}
			}

			if (isValidByDefault)
			{
				PXDatabase.Update<POOrder>(
					new PXDataFieldAssign(nameof(POOrder.IsTaxValid), true),
					new PXDataFieldAssign(nameof(POOrder.IsUnbilledTaxValid), true),
					new PXDataFieldRestrict(nameof(POOrder.OrderType), PXDbType.VarChar, 2, order.OrderType, PXComp.EQ),
					new PXDataFieldRestrict(nameof(POOrder.OrderNbr), PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ)
					);
				order.IsTaxValid = true;
				order.IsUnbilledTaxValid = true;
				PXTimeStampScope.PutPersisted(Base.Document.Cache, order, PXDatabase.SelectTimeStamp());
				return order;
			}

			GetTaxResult result = null;
			GetTaxResult resultUnbilled = null;

			bool getTaxFailed = false;
			if (getRequest != null)
			{
				result = service.GetTax(getRequest);
				if (!result.IsSuccess)
				{
					getTaxFailed = true;
				}
			}
			if (getRequestUnbilled != null)
			{
				resultUnbilled = service.GetTax(getRequestUnbilled);
				if (!resultUnbilled.IsSuccess)
				{
					getTaxFailed = true;
				}
			}

			if (!getTaxFailed)
			{
				try
				{
					ApplyTax(order, result, resultUnbilled);
					PXDatabase.Update<POOrder>(
						new PXDataFieldAssign(nameof(POOrder.IsTaxValid), true),
						new PXDataFieldAssign(nameof(POOrder.IsUnbilledTaxValid), true),
						new PXDataFieldRestrict(nameof(POOrder.OrderType), PXDbType.VarChar, 2, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict(nameof(POOrder.OrderNbr), PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ)
						);
					order.IsTaxValid = true;
					order.IsUnbilledTaxValid = true;
					PXTimeStampScope.PutPersisted(Base.Document.Cache, order, PXDatabase.SelectTimeStamp());

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
				ResultBase taxResult = result ?? resultUnbilled;
				if (taxResult != null)
					LogMessages(taxResult);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}

			return order;
		}

		[PXOverride]
		public virtual void Persist(Action baseImpl)
		{
			if (Base.Document.Current?.OrderType == POOrderType.DropShip
				&& IsExternalTax(SO.SOOrder.PK.Find(Base, Base.Document.Current.SOOrderType, Base.Document.Current.SOOrderNbr)?.TaxZoneID))
				GetToAddress(Base.Document.Current).With(ValidAddressFrom<POOrder.shipAddressID>);

			baseImpl();

			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Document.Current.IsTaxValid != true)
			{
				PXLongOperation.StartOperation(Base, delegate ()
				{
					POOrder doc = new POOrder();
					doc.OrderType = Base.Document.Current.OrderType;
					doc.OrderNbr = Base.Document.Current.OrderNbr;
					POExternalTaxCalc.Process(doc);
				});
			}
		}

		protected virtual void _(Events.RowSelected<POOrder> e)
		{
			if (e.Row == null)
				return;

			if (IsExternalTax(e.Row.TaxZoneID) && e.Row.IsTaxValid != true)
			{
				PXUIFieldAttribute.SetWarning<POOrder.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
			}
		}

		protected virtual void _(Events.RowUpdated<POOrder> e)
		{
			if (e.Row == null)
				return;

			if (e.Cache.ObjectsEqual<POOrder.shipDestType, POOrder.shipToBAccountID, POOrder.shipToLocationID>(e.Row, e.OldRow) == false)
				InvalidateExternalTax(e.Row);
		}

		protected virtual void _(Events.RowUpdated<POLine> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID) &&
			    !e.Cache.ObjectsEqual<POLine.inventoryID, POLine.tranDesc, POLine.extCost, POLine.promisedDate, POLine.taxCategoryID>(e.Row, e.OldRow))
			{
				Base.Document.Current.IsTaxValid = false;
				Base.Document.Update(Base.Document.Current);
			}
		}

		#region POShipAddress handlers
		protected virtual void _(Events.RowUpdated<POShipAddress> e)
		{
			if (e.Row == null) return;
			if (e.Cache.ObjectsEqual<POShipAddress.postalCode, POShipAddress.countryID, POShipAddress.state>(e.Row, e.OldRow) == false)
				InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.RowInserted<POShipAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.RowDeleted<POShipAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.FieldUpdating<POShipAddress, POShipAddress.overrideAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}
		#endregion

		protected virtual GetTaxRequest BuildGetTaxRequest(POOrder order)
		{
			if (order == null)
				throw new PXArgumentException(nameof(order));

			Location loc = (Location)Base.location.View.SelectSingleBound(new object[] { order });
			Vendor vend = (Vendor)Base.vendor.View.SelectSingleBound(new object[] { order });

			IAddressBase fromAddress = GetFromAddress(order);
			IAddressBase toAddress = GetToAddress(order);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddressSO);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddressSO);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = vend.AcctCD;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = $"PO.{order.OrderType}.{order.OrderNbr}";
			request.DocDate = order.ExpectedDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(order);

			int mult = 1;

			request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}

			request.DocType = TaxDocumentType.PurchaseOrder;

			PXSelectBase<POLine> select = new PXSelectJoin<POLine,
				LeftJoin<InventoryItem, On<POLine.FK.InventoryItem>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<POLine.orderType, Equal<Current<POOrder.orderType>>, And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>,
				OrderBy<Asc<POLine.lineNbr>>>(Base);

			//request.Discount = order.CuryDiscTot.GetValueOrDefault();

			foreach (PXResult<POLine, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				POLine tran = (POLine)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.Amount = mult * tran.CuryExtCost.GetValueOrDefault();
				line.Description = tran.TranDesc;
				line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
				line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(tran.OrderQty.GetValueOrDefault());
				line.Discounted = request.Discount > 0;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;

				request.CartItems.Add(line);
			}

			return request;
		}

		protected virtual GetTaxRequest BuildGetTaxRequestUnbilled(POOrder order)
		{
			if (order == null)
				throw new PXArgumentException(nameof(order));

			Vendor vend = (Vendor)Base.vendor.View.SelectSingleBound(new object[] { order });
			Location loc = (Location)Base.location.View.SelectSingleBound(new object[] { order });

			IAddressBase fromAddress = GetFromAddress(order);
			IAddressBase toAddress = GetToAddress(order);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddressSO);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddressSO);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = vend.AcctCD;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = string.Format("PO.{0}.{1}", order.OrderType, order.OrderNbr);
			request.DocDate = order.OrderDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(order);

			int mult = 1;

			request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}


			request.DocType = TaxDocumentType.PurchaseOrder;

			PXSelectBase<POLine> select = new PXSelectJoin<POLine,
				LeftJoin<InventoryItem, On<POLine.FK.InventoryItem>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<POLine.orderType, Equal<Current<POOrder.orderType>>, And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>,
				OrderBy<Asc<POLine.lineNbr>>>(Base);


			foreach (PXResult<POLine, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				POLine tran = (POLine)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				if (tran.UnbilledAmt > 0)
				{
					var line = new TaxCartItem();
					line.Index = tran.LineNbr ?? 0;
					line.Amount = mult * tran.CuryUnbilledAmt.GetValueOrDefault();
					line.Description = tran.TranDesc;
					line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
					line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
					line.ItemCode = item.InventoryCD;
					line.Quantity = tran.BaseUnbilledQty.GetValueOrDefault();
					line.Discounted = request.Discount > 0;
					line.RevAcct = salesAccount.AccountCD;

					line.TaxCode = tran.TaxCategoryID;

					request.CartItems.Add(line);
				}
			}

			return request;
		}

		protected virtual void ApplyTax(POOrder order, GetTaxResult result, GetTaxResult resultUnbilled)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;
			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { order });
				vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(Base, taxZone.TaxVendorID);
				if (vendor == null)
					throw new PXException(Messages.ExternalTaxVendorNotFound);
			}
			//Clear all existing Tax transactions:
			foreach (PXResult<POTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { order }))
			{
				POTaxTran taxTran = (POTaxTran)res;
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

				POTaxTran tax = new POTaxTran();
				tax.OrderType = order.OrderType;
				tax.OrderNbr = order.OrderNbr;
				tax.TaxID = taxID;
				tax.CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount);
				tax.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount);
				tax.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100;
				tax.JurisType = result.TaxSummary[i].JurisType;
				tax.JurisName = result.TaxSummary[i].JurisName;

				Base.Taxes.Insert(tax);
			}

			bool requireBlanketControlTotal = Base.POSetup.Current.RequireBlanketControlTotal == true;
			bool requireDropShipControlTotal = Base.POSetup.Current.RequireDropShipControlTotal == true;
			bool requireOrderControlTotal = Base.POSetup.Current.RequireOrderControlTotal == true;

			if (order.Hold != true)
			{
				Base.POSetup.Current.RequireBlanketControlTotal = false;
				Base.POSetup.Current.RequireDropShipControlTotal = false;
				Base.POSetup.Current.RequireOrderControlTotal = false;
			}

			try
			{
				Base.Document.SetValueExt<POOrder.curyTaxTotal>(order, Math.Abs(result.TotalTaxAmount));
				if (resultUnbilled != null)
					Base.Document.SetValueExt<POOrder.curyUnbilledTaxTotal>(order, Math.Abs(resultUnbilled.TotalTaxAmount));
				Base.Document.Update(order);
			}
			finally
			{
				Base.POSetup.Current.RequireBlanketControlTotal = requireBlanketControlTotal;
				Base.POSetup.Current.RequireDropShipControlTotal = requireDropShipControlTotal;
				Base.POSetup.Current.RequireOrderControlTotal = requireOrderControlTotal;
			}

			SkipTaxCalcAndSave();
		}

		protected override string GetExternalTaxProviderLocationCode(POOrder order) => GetExternalTaxProviderLocationCode<POLine, POLine.FK.Order.SameAsCurrent, POLine.siteID>(order);

		protected virtual IAddressBase GetToAddress(POOrder order)
		{
			if (order.OrderType.IsIn(POOrderType.RegularOrder, POOrderType.Blanket, POOrderType.StandardBlanket, POOrderType.DropShip))
				return (POShipAddress)PXSelect<POShipAddress, Where<POShipAddress.addressID, Equal<Required<POOrder.shipAddressID>>>>.Select(Base, order.ShipAddressID);

			return
				PXSelectJoin<Branch,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
				Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
				.Select(Base, order.BranchID)
				.RowCast<Address>()
				.FirstOrDefault();
		}

		protected virtual IAddressBase GetToAddress(POOrder order, POLine line)
		{
			if (order.OrderType.IsIn(POOrderType.RegularOrder, POOrderType.Blanket, POOrderType.StandardBlanket))
			{
				return
					PXSelectJoin<Address,
					InnerJoin<INSite, On<INSite.FK.Address>>,
					Where<INSite.siteID, Equal<Current<POLine.siteID>>>>
					.SelectSingleBound(Base, new[] { line })
					.RowCast<Address>()
					.FirstOrDefault()
					?? GetToAddress(order);
			}

			return GetToAddress(order);
		}

		protected virtual CR.Standalone.Location GetBranchLocation(POOrder order)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<BAccountR.bAccountID>, And<CR.Standalone.Location.locationID, Equal<BAccountR.defLocationID>>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccountR, CR.Standalone.Location> res in select.Select(order.BranchID))
				return (CR.Standalone.Location)res;

			return null;
		}

		protected virtual IAddressBase GetFromAddress(POOrder order)
		{
			if (order.OrderType.IsIn(POOrderType.RegularOrder, POOrderType.Blanket, POOrderType.StandardBlanket, POOrderType.DropShip))
				return (PORemitAddress)PXSelect<PORemitAddress, Where<PORemitAddress.addressID, Equal<Required<POOrder.remitAddressID>>>>.Select(Base, order.RemitAddressID);

			return (Address)PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, Base.vendor.Current.DefAddressID);
		}

		protected virtual IAddressBase GetFromAddress(POOrder order, POLine line)
		{
			return GetFromAddress(order);
		}


		private void InvalidateExternalTax(POOrder order)
		{
			if (order == null || IsExternalTax(order.TaxZoneID) != true) return;
			order.IsTaxValid = false;
			order.IsUnbilledTaxValid = false;
		}

		private IAddressBase ValidAddressFrom<TFieldSource>(IAddressBase address)
			where TFieldSource : IBqlField
		{
			if (!string.IsNullOrEmpty(address.PostalCode)) return address;
			throw new PXException(PickAddressError<TFieldSource>(address));
		}

		private string PickAddressError<TFieldSource>(IAddressBase address)
			where TFieldSource : IBqlField
		{
			if (typeof(TFieldSource) == typeof(POOrder.shipAddressID))
				return PXSelect<POOrder, Where<POOrder.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((POAddress)address).AddressID).First().GetItem<POOrder>()
					.With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<POOrder>(), new EntityHelper(Base).GetRowID(e)));

			throw new ArgumentOutOfRangeException("Unknown address source used");
		}
	}
}
