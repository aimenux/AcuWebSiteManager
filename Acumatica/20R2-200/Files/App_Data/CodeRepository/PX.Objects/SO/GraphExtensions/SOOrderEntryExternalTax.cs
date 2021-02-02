using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;
using PX.Objects.Common.Extensions;
using PX.Api.Helpers;

namespace PX.Objects.SO
{
	public class SOOrderEntryExternalTax : ExternalTax<SOOrderEntry, SOOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public virtual bool CalculateTaxesUsingExternalProvider(string taxZoneID)
		{
			bool isImportedTaxes = Base.Document.Current != null && Base.Document.Current.ExternalTaxesImportInProgress == true;
			return IsExternalTax(Base, taxZoneID) && !isImportedTaxes;
		}

		public override SOOrder CalculateExternalTax(SOOrder order)
		{
			if (CalculateTaxesUsingExternalProvider(order.TaxZoneID))
				return CalculateExternalTax(order, false);

			return order;
		}

		public SOOrder CalculateExternalTax(SOOrder order, bool forceRecalculate)
		{
			var toAddress = GetToAddress(order);

			Stopwatch sw = new Stopwatch();
			sw.Start();
			Debug.Print("{0} Enter CalculateTax", DateTime.Now.TimeOfDay);
			Debug.Indent();

			var service = TaxProviderFactory(Base, order.TaxZoneID);

			GetTaxRequest getRequest = null;
			GetTaxRequest getRequestOpen = null;
			GetTaxRequest getRequestUnbilled = null;

			bool isValidByDefault = true;

			SOOrderType orderType = (SOOrderType)Base.soordertype.View.SelectSingleBound(new object[] { order });

			bool stopExternalTaxCalc = order.IsTaxValid == true && order.IsOpenTaxValid == true && order.IsUnbilledTaxValid == true
				&& !forceRecalculate && orderType.INDocType != INTranType.Transfer && !IsNonTaxable(toAddress);

			if (stopExternalTaxCalc)
				return order;

			if (orderType.INDocType != INTranType.Transfer && !IsNonTaxable(toAddress))
			{
				if (order.IsTaxValid != true || forceRecalculate)
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

				if (order.IsOpenTaxValid != true || forceRecalculate)
				{
					getRequestOpen = BuildGetTaxRequestOpen(order);
					if (getRequestOpen.CartItems.Count > 0)
					{
						isValidByDefault = false;
					}
					else
					{
						getRequestOpen = null;
					}
				}

				if (order.IsUnbilledTaxValid != true || forceRecalculate)
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
			}

			if (isValidByDefault)
			{
				order.CuryTaxTotal = 0;
				order.CuryOpenTaxTotal = 0;
				order.CuryUnbilledTaxTotal = 0;
				order.IsTaxValid = true;
				order.IsOpenTaxValid = true;
				order.IsUnbilledTaxValid = true;

				Base.Document.Update(order);

				foreach (SOTaxTran item in Base.Taxes.Select())
				{
					Base.Taxes.Delete(item);
				}

				using (var ts = new PXTransactionScope())
				{
					Base.Persist(typeof(SOTaxTran), PXDBOperation.Delete);
					Base.Persist(typeof(SOOrder), PXDBOperation.Update);
					PXTimeStampScope.PutPersisted(Base.Document.Cache, order, PXDatabase.SelectTimeStamp());
					ts.Complete();
				}
				return order;
			}

			GetTaxResult result = null;
			GetTaxResult resultOpen = null;
			GetTaxResult resultUnbilled = null;

			bool getTaxFailed = false;
			if (getRequest != null)
			{
				Stopwatch sw2 = new Stopwatch();
				sw2.Start();
				result = service.GetTax(getRequest);
				sw2.Stop();
				Debug.Print("{0} GetTax(request) in {1} millisec", DateTime.Now.TimeOfDay, sw2.ElapsedMilliseconds);

				if (!result.IsSuccess)
				{
					getTaxFailed = true;
				}
			}
			if (getRequestOpen != null)
			{
				if (getRequest != null && IsSame(getRequest, getRequestOpen))
				{
					resultOpen = result;
				}
				else
				{
					Stopwatch sw2 = new Stopwatch();
					sw2.Start();
					resultOpen = service.GetTax(getRequestOpen);
					sw2.Stop();
					Debug.Print("{0} GetTax(requestOpen) in {1} millisec", DateTime.Now.TimeOfDay, sw2.ElapsedMilliseconds);
					if (!resultOpen.IsSuccess)
					{
						getTaxFailed = true;
					}
				}
			}
			if (getRequestUnbilled != null)
			{
				if (getRequest != null && IsSame(getRequest, getRequestUnbilled))
				{
					resultUnbilled = result;
				}
				else
				{
					Stopwatch sw2 = new Stopwatch();
					sw2.Start();
					resultUnbilled = service.GetTax(getRequestUnbilled);
					sw2.Stop();
					Debug.Print("{0} GetTax(requestUnbilled) in {1} millisec", DateTime.Now.TimeOfDay, sw2.ElapsedMilliseconds);
					if (!resultUnbilled.IsSuccess)
					{
						getTaxFailed = true;
					}
				}
			}

			if (!getTaxFailed)
			{
				ApplyExternalTaxes(order, result, resultOpen, resultUnbilled);
			}
			else
			{
				ResultBase taxResult = result ?? resultOpen ?? resultUnbilled;
				if (taxResult != null)
					LogMessages(taxResult);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}

			sw.Stop();
			Debug.Unindent();
			Debug.Print("{0} Exit CalculateTax in {1} millisec", DateTime.Now.TimeOfDay, sw.ElapsedMilliseconds);

			return order;
		}

		public void ApplyExternalTaxes(SOOrder order, GetTaxResult result, GetTaxResult resultOpen, GetTaxResult resultUnbilled)
		{
			try
			{
				ApplyTax(order, result, resultOpen, resultUnbilled);
				Stopwatch sw2 = new Stopwatch();
				sw2.Start();
				order.IsTaxValid = true;
				order.IsOpenTaxValid = true;
				order.IsUnbilledTaxValid = true;

				sw2.Stop();
				Debug.Print("{0} PXDatabase.Update<SOOrder> in {1} millisec", DateTime.Now.TimeOfDay, sw2.ElapsedMilliseconds);
				using (var ts = new PXTransactionScope())
				{
					Base.Document.Update(order);
					Base.Persist(typeof(SOOrder), PXDBOperation.Update);
					PXTimeStampScope.PutPersisted(Base.Document.Cache, order, PXDatabase.SelectTimeStamp());
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

		[PXOverride]
		public virtual void RecalculateExternalTaxes()
		{
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && !Base.IsTransferOrder &&
			    (Base.Document.Current.IsTaxValid != true || Base.Document.Current.IsOpenTaxValid != true || Base.Document.Current.IsUnbilledTaxValid != true)
			)
			{
				if (Base.RecalculateExternalTaxesSync)
				{
					// Need to calculate the external taxes in the context of the calling graph because the order can be not persisted yet
					CalculateExternalTax(Base.Document.Current);
				}
				else
				{
					Debug.Print("{0} SOExternalTaxCalc.Process(doc) Async", DateTime.Now.TimeOfDay);
					PXLongOperation.StartOperation(Base, delegate ()
					{
						Debug.Print("{0} Inside PXLongOperation.StartOperation", DateTime.Now.TimeOfDay);
						SOOrder doc = new SOOrder();
						doc.OrderType = Base.Document.Current.OrderType;
						doc.OrderNbr = Base.Document.Current.OrderNbr;
						SOExternalTaxCalc.Process(doc);

					});
				}
			}
		}

		public PXAction<SOOrder> recalcExternalTax;
		[PXUIField(DisplayName = "Recalculate External Tax", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable RecalcExternalTax(PXAdapter adapter)
		{
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				var order = Base.Document.Current;
				CalculateExternalTax(Base.Document.Current, true);

				Base.Clear(PXClearOption.ClearAll);
				Base.Document.Current = Base.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

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

		protected virtual void _(Events.RowSelected<SOOrder> e)
		{
			if (e.Row == null)
				return;

			var isExternalTax = CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID);

			if (isExternalTax == true && ((SOOrder)e.Row).IsTaxValid != true && !Base.IsTransferOrder)
				PXUIFieldAttribute.SetWarning<SOOrder.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);

			if (isExternalTax == true && ((SOOrder)e.Row).IsOpenTaxValid != true && !Base.IsTransferOrder)
				PXUIFieldAttribute.SetWarning<SOOrder.curyOpenTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);

			if (isExternalTax == true && ((SOOrder)e.Row).IsUnbilledTaxValid != true && !Base.IsTransferOrder)
			{
				PXUIFieldAttribute.SetWarning<SOOrder.curyUnbilledTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
				PXUIFieldAttribute.SetWarning<SOOrder.curyUnbilledOrderTotal>(e.Cache, e.Row, PX.Objects.SO.Messages.UnbilledBalanceWithoutTaxTaxIsNotUptodate);
			}

            bool taxEnabled = e.Row.Completed != true && e.Row.Cancelled != true && (e.Cache.Graph.IsContractBasedAPI || !isExternalTax);
            Base.Taxes.Cache.SetAllEditPermissions(taxEnabled);
        }

		protected virtual void _(Events.RowUpdated<SOOrder> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID))
			{
				if (!e.Cache.ObjectsEqual<
					SOOrder.avalaraCustomerUsageType, 
					SOOrder.orderDate, 
					SOOrder.taxZoneID, 
					SOOrder.customerID, 
					SOOrder.shipAddressID, 
					SOOrder.willCall, 
					SOOrder.curyFreightTot, 
					SOOrder.freightTaxCategoryID>(e.Row, e.OldRow))
				{
					e.Row.IsTaxValid = false;
					e.Row.IsOpenTaxValid = false;
					e.Row.IsUnbilledTaxValid = false;
				}
			}
		}

		protected virtual void _(Events.RowInserted<SOLine> e)
		{
			if(e.Cache.Graph.IsCopyPasteContext)
			{
				//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowUpdated<SOLine> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				if (!e.Cache.ObjectsEqual<
						SOLine.avalaraCustomerUsageType,
						SOLine.salesAcctID,
						SOLine.inventoryID,
						SOLine.tranDesc,
						SOLine.lineAmt,
						SOLine.orderDate,
						SOLine.taxCategoryID,
						SOLine.siteID
					>(e.Row, e.OldRow) ||
					(e.Row.POSource == INReplenishmentSource.DropShipToOrder) != (e.OldRow.POSource == INReplenishmentSource.DropShipToOrder))
				{
					InvalidateExternalTax(Base.Document.Current);
				}

				if (!e.Cache.ObjectsEqual<SOLine.openAmt>(e.Row, e.OldRow))
				{
					Base.Document.Current.IsOpenTaxValid = false;
				}

				if (!e.Cache.ObjectsEqual<SOLine.unbilledAmt>(e.Row, e.OldRow))
				{
					Base.Document.Current.IsUnbilledTaxValid = false;
				}
			}
		}

		protected virtual void _(Events.RowDeleted<SOLine> e)
		{
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				InvalidateExternalTax(Base.Document.Current);
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		#region SOShippingAddress Events
		protected virtual void _(Events.RowUpdated<SOShippingAddress> e)
		{
			if (e.Row == null) return;
			if (e.Cache.ObjectsEqual<SOShippingAddress.postalCode, SOShippingAddress.countryID, SOShippingAddress.state>(e.Row, e.OldRow) == false)
				InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.RowInserted<SOShippingAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.RowDeleted<SOShippingAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.FieldUpdating<SOShippingAddress, SOShippingAddress.overrideAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}
		#endregion

		private GetTaxRequest BuildGetTaxRequest<TLineAmt, TLineQty, TDocDiscount>(SOOrder order, string docCode, string debugMethodName)
			where TLineAmt : IBqlField
			where TLineQty : IBqlField
			where TDocDiscount : IBqlField
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Debug.Indent();

			if (order == null)
				throw new PXArgumentException(nameof(order));

			Customer cust = (Customer)Base.customer.View.SelectSingleBound(new object[] { order });
			Location loc = (Location)Base.location.View.SelectSingleBound(new object[] { order });

			IAddressBase fromAddress = GetFromAddress(order);
			IAddressBase toAddress = GetToAddress(order);

			Debug.Print($"{DateTime.Now.TimeOfDay} Select Customer, Location, Addresses in {sw.ElapsedMilliseconds} millisec");

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddressSO);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddressSO);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = cust.AcctCD;
			request.TaxRegistrationID = loc?.TaxRegistrationID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = docCode;
			request.DocDate = order.OrderDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(order);

			Sign docSign = Sign.Plus;

			request.CustomerUsageType = order.AvalaraCustomerUsageType;
			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}

			SOOrderType orderType = (SOOrderType)Base.soordertype.View.SelectSingleBound(new object[] { order });

			if (orderType.DefaultOperation == SOOperation.Receipt)
			{
				request.DocType = TaxDocumentType.ReturnOrder;
				docSign = Sign.Minus;
			}
			else
			{
				request.DocType = TaxDocumentType.SalesOrder;
			}

			PXSelectBase<SOLine> select = new PXSelectJoin<SOLine,
				LeftJoin<InventoryItem, On<SOLine.FK.InventoryItem>,
					LeftJoin<Account, On<Account.accountID, Equal<SOLine.salesAcctID>>>>,
				Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
				OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>>(Base);

			PXCache documentCache = Base.Caches[typeof(SOOrder)];
			request.Discount = (documentCache.GetValue<TDocDiscount>(order) as decimal?) ?? 0m;

			Stopwatch sw2 = new Stopwatch();
			sw2.Start();

			if (order.CuryFreightTot > 0)
			{
				var line = new TaxCartItem();
				line.Index = short.MinValue;
				line.Quantity = 1;
				line.UOM = "EA";
				line.Amount = docSign * order.CuryFreightTot.GetValueOrDefault();
				line.Description = PXMessages.LocalizeNoPrefix(Messages.FreightDesc);
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = "N/A";
				line.Discounted = false;
				line.TaxCode = order.FreightTaxCategoryID;

				request.CartItems.Add(line);
			}

			PXCache lineCache = Base.Caches[typeof(SOLine)];

			foreach (PXResult<SOLine, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				SOLine tran = (SOLine)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;
				bool lineIsDiscounted = request.Discount  > 0m &&
					((tran.DocumentDiscountRate ?? 1m) != 1m || (tran.GroupDiscountRate ?? 1m) != 1m);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;

				decimal lineAmount = (lineCache.GetValue<TLineAmt>(tran) as decimal?) ?? 0m;
				decimal lineQty = (lineCache.GetValue<TLineQty>(tran) as decimal?) ?? 0m;

				line.Amount = orderType.DefaultOperation != tran.Operation
					? Sign.Minus * docSign * lineAmount
					: docSign * lineAmount;

				line.Description = tran.TranDesc;
				line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
				line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(lineQty);
				line.UOM = tran.UOM;
				line.Discounted = lineIsDiscounted;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;
				line.CustomerUsageType = tran.AvalaraCustomerUsageType;

				if (tran.Operation == SOOperation.Receipt && tran.InvoiceDate != null)
				{
					line.TaxOverride.Reason = Messages.ReturnReason;
					line.TaxOverride.TaxDate = tran.InvoiceDate.Value;
					line.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
				}

				request.CartItems.Add(line);
			}

			sw2.Stop();
			Debug.Print($"{DateTime.Now.TimeOfDay} Select detail lines in {sw2.ElapsedMilliseconds} millisec.");

			Debug.Unindent();
			sw.Stop();
			Debug.Print($"{DateTime.Now.TimeOfDay} {debugMethodName}() in {sw.ElapsedMilliseconds} millisec.");

			return request;
		}

		protected virtual GetTaxRequest BuildGetTaxRequest(SOOrder order) => 
			BuildGetTaxRequest<SOLine.curyLineAmt, SOLine.orderQty, SOOrder.curyDiscTot>(order, $"SO.{order.OrderType}.{order.OrderNbr}", nameof(BuildGetTaxRequest));

		protected virtual GetTaxRequest BuildGetTaxRequestOpen(SOOrder order) => 
			BuildGetTaxRequest<SOLine.curyOpenAmt, SOLine.openQty, SOOrder.curyOpenDiscTotal>(order, $"SO.{order.OrderType}.{order.OrderNbr}", nameof(BuildGetTaxRequestOpen));

		protected virtual GetTaxRequest BuildGetTaxRequestUnbilled(SOOrder order) =>
			BuildGetTaxRequest<SOLine.curyUnbilledAmt, SOLine.unbilledQty, SOOrder.curyUnbilledDiscTotal>(order, $"{order.OrderType}.{order.OrderNbr}.Open", nameof(BuildGetTaxRequestUnbilled));

		protected virtual void ApplyTax(SOOrder order, GetTaxResult result, GetTaxResult resultOpen, GetTaxResult resultUnbilled)
		{
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { order });
			if (taxZone == null)
			{
				throw new PXException(Messages.TaxZoneIsNotSet);
			}

			AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(Base, taxZone.TaxVendorID);

			if (vendor == null)
				throw new PXException(Messages.ExternalTaxVendorNotFound);

			var sign = ((SOOrderType)Base.soordertype.View.SelectSingleBound(new object[] { order })).DefaultOperation == SOOperation.Receipt
				? Sign.Minus
				: Sign.Plus;

			PXSelectBase<SOTaxTran> TaxesSelect = new PXSelectJoin<SOTaxTran,
				InnerJoin<Tax, On<Tax.taxID, Equal<SOTaxTran.taxID>>>,
				Where<SOTaxTran.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(Base);

			if (result != null)
			{
				ClearExistingTaxes(order);

				Base.Views.Caches.Add(typeof(Tax));

				bool requireControlTotal = Base.soordertype.Current.RequireControlTotal == true;

				if (order.Hold != true)
					Base.soordertype.Current.RequireControlTotal = false;

				TaxProvider.TaxDetail[] unbilledFreightTaxDetails = GetFreightTaxDetails(resultUnbilled);
				TaxProvider.TaxDetail[] unshippedFreightTaxDetails = GetFreightTaxDetails(resultOpen);

				try
				{
					foreach (var taxDetail in result.TaxSummary)
					{
						string taxID = GetSOTaxID(taxDetail);

						if (string.IsNullOrEmpty(taxID))
						{
							PXTrace.WriteInformation(Messages.EmptyValuesFromExternalTaxProvider);
							continue;
						}

						string taxUniqueID = GetTaxUniqueID(taxDetail);
						var unbilledDetail = GetTaxDetailWithoutFreight(taxUniqueID, resultUnbilled?.TaxSummary, unbilledFreightTaxDetails, taxDetail);
						var unshippedDetail = GetTaxDetailWithoutFreight(taxUniqueID, resultOpen?.TaxSummary, unshippedFreightTaxDetails, taxDetail);

						CreateTax(Base, taxZone, vendor, taxDetail, taxID);

						var tax = new SOTaxTran
						{
							OrderType = order.OrderType,
							OrderNbr = order.OrderNbr,
							TaxID = taxID,
							CuryTaxAmt = sign * taxDetail.TaxAmount,
							CuryTaxableAmt = sign * taxDetail.TaxableAmount,
							CuryUnshippedTaxAmt = sign * unshippedDetail.TaxAmount,
							CuryUnshippedTaxableAmt = sign * unshippedDetail.TaxableAmount,
							CuryUnbilledTaxAmt = sign * unbilledDetail.TaxAmount,
							CuryUnbilledTaxableAmt = sign * unbilledDetail.TaxableAmount,
							TaxRate = Convert.ToDecimal(taxDetail.Rate) * 100,
							JurisType = taxDetail.JurisType,
							JurisName = taxDetail.JurisName
						};
						Base.Taxes.Insert(tax);
					}

					Base.Document.SetValueExt<SOOrder.curyTaxTotal>(order, sign * result.TotalTaxAmount);
				}
				finally
				{
					Base.soordertype.Current.RequireControlTotal = requireControlTotal;
				}
			}

			if (result == null)
			{
				TaxProvider.TaxDetail[] unbilledFreightTaxDetails = GetFreightTaxDetails(resultUnbilled);
				TaxProvider.TaxDetail[] unshippedFreightTaxDetails = GetFreightTaxDetails(resultOpen);

				foreach (PXResult<SOTaxTran, Tax> res in TaxesSelect.View.SelectMultiBound(new object[] { order }))
				{
					SOTaxTran taxTran = res;
					string taxUniqueID = GetTaxUniqueID(taxTran);
					var unbilledDetail = GetTaxDetailWithoutFreight(taxUniqueID, resultUnbilled?.TaxSummary, unbilledFreightTaxDetails);
					if (unbilledDetail != null)
					{
						taxTran.CuryUnbilledTaxAmt = sign * unbilledDetail.TaxAmount;
						taxTran.CuryUnbilledTaxableAmt = sign * unbilledDetail.TaxableAmount;
					}

					var unshippedDetail = GetTaxDetailWithoutFreight(taxUniqueID, resultOpen?.TaxSummary, unshippedFreightTaxDetails);
					if (unshippedDetail != null)
					{
						taxTran.CuryUnshippedTaxAmt = sign * unshippedDetail.TaxAmount;
						taxTran.CuryUnshippedTaxableAmt = sign * unshippedDetail.TaxableAmount;
					}

					if (unbilledDetail != null || unshippedDetail != null)
					{
						Base.Taxes.Update(taxTran);
					}
				}
			}

			if (resultUnbilled != null)
			{
				decimal freightTax = resultUnbilled.TaxLines?.FirstOrDefault(taxLine => taxLine.Index == short.MinValue)?.TaxAmount ?? 0m;
				Base.Document.SetValueExt<SOOrder.curyUnbilledTaxTotal>(order, sign * (resultUnbilled.TotalTaxAmount - freightTax));
			}

			if (resultOpen != null)
			{
				decimal freightTax = resultOpen.TaxLines?.FirstOrDefault(taxLine => taxLine.Index == short.MinValue)?.TaxAmount ?? 0m;
				Base.Document.SetValueExt<SOOrder.curyOpenTaxTotal>(order, sign * (resultOpen.TotalTaxAmount - freightTax));
			}

			Base.Document.Update(order);

			SkipTaxCalcAndSave();
		}

		public virtual TaxProvider.TaxDetail[] GetFreightTaxDetails(TaxProvider.GetTaxResult result)
		{
			TaxProvider.TaxLine freightLine = result?.TaxLines?.FirstOrDefault(taxLine => taxLine.Index == short.MinValue);
			return freightLine?.TaxDetails ?? new TaxProvider.TaxDetail[0];
		}

		public virtual TaxProvider.TaxDetail GetTaxDetailWithoutFreight(string taxUniqueID, TaxProvider.TaxDetail[] summaryDetails,
			TaxProvider.TaxDetail[] freightParts, TaxProvider.TaxDetail fallbackSummaryDetail = null)
		{
			var summaryDetail = summaryDetails?.FirstOrDefault(d => GetTaxUniqueID(d) == taxUniqueID) ?? fallbackSummaryDetail;
			var freightTaxPart = freightParts.FirstOrDefault(d => GetTaxUniqueID(d) == taxUniqueID);

			if (summaryDetail == null || freightTaxPart == null)
				return summaryDetail;

			return new TaxProvider.TaxDetail
			{
				TaxName = summaryDetail.TaxName,
				JurisName = summaryDetail.JurisName,
				JurisType = summaryDetail.JurisType,
				JurisCode = summaryDetail.JurisCode,
				TaxAmount = summaryDetail.TaxAmount - freightTaxPart.TaxAmount,
				TaxableAmount = summaryDetail.TaxableAmount - freightTaxPart.TaxableAmount,
				Rate = summaryDetail.Rate,
				TaxCalculationLevel = summaryDetail.TaxCalculationLevel
			};
		}

		public virtual string GetSOTaxID(TaxProvider.TaxDetail taxDetail)
		{
			return !string.IsNullOrEmpty(taxDetail.TaxName) ? taxDetail.TaxName : taxDetail.JurisCode;
		}

		public virtual string GetTaxUniqueID(TaxProvider.TaxDetail taxDetail)
		{
			return String.Join("-", GetSOTaxID(taxDetail), taxDetail.JurisType ?? "", taxDetail.JurisName ?? "");
		}

		public virtual string GetTaxUniqueID(SOTaxTran taxDetail)
		{
			return String.Join("-", taxDetail.TaxID, taxDetail.JurisType ?? "", taxDetail.JurisName ?? "");
		}

		/// <summary>
		/// //Clears all existing Tax transactions
		/// </summary>
		/// <param name="order"></param>
		protected virtual void ClearExistingTaxes(SOOrder order)
		{
			PXSelectBase<SOTaxTran> TaxesSelect;

			if (order.ExternalTaxesImportInProgress == true && Base.IsContractBasedAPI)
			{
				TaxesSelect = new PXSelect<SOTaxTran, Where<SOTaxTran.orderType,
					   Equal<Current<SOOrder.orderType>>, And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(Base);

				foreach (SOTaxTran taxTran in TaxesSelect.View.SelectMultiBound(new object[] { order }))
				{
					Base.Taxes.Delete(taxTran);
				}
			}
			else
			{
				TaxesSelect = new PXSelectJoin<SOTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<SOTaxTran.taxID>>>, Where<SOTaxTran.orderType,
					Equal<Current<SOOrder.orderType>>, And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(Base);

				foreach (PXResult<SOTaxTran, Tax> res in TaxesSelect.View.SelectMultiBound(new object[] { order }))
				{
					SOTaxTran taxTran = res;
					Base.Taxes.Delete(taxTran);
				}
			}
		}

		protected virtual bool IsSame(GetTaxRequest x, GetTaxRequest y)
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

		protected override string GetExternalTaxProviderLocationCode(SOOrder order) => GetExternalTaxProviderLocationCode<SOLine, SOLine.FK.Order.SameAsCurrent, SOLine.siteID>(order);

        protected virtual IAddressBase GetFromAddress(SOOrder order)
		{
			var branch =
				PXSelectJoin<Branch,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
				InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
				Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
				.Select(Base, order.BranchID)
				.RowCast<Address>()
				.FirstOrDefault()
				.With(ValidAddressFrom<BAccountR.defAddressID>);

			return branch;
		}

        public virtual IAddressBase GetFromAddress(SOOrder order, SOLine line)
		{
			Boolean isDropShip = line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder;
			IAddressBase vendorAddress = isDropShip
				? PXSelectJoin<Address,
					InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>,
					InnerJoin<Vendor, On<Vendor.defLocationID, Equal<Location.locationID>>>>,
					Where<Vendor.bAccountID, Equal<Current<SOLine.vendorID>>>>
					.SelectSingleBound(Base, new[] { line })
					.RowCast<Address>()
					.FirstOrDefault()
					.With(ValidAddressFrom<Vendor.defLocationID>)
				: null;
			return vendorAddress
				?? PXSelectJoin<Address,
					InnerJoin<INSite, 
						On<INSite.FK.Address>>,
					Where<INSite.siteID, Equal<Current<SOLine.siteID>>>>
					.SelectSingleBound(Base, new[] { line })
					.RowCast<Address>()
					.FirstOrDefault()
					.With(ValidAddressFrom<INSite.addressID>)
				?? GetFromAddress(order);
		}

        protected virtual IAddressBase GetToAddress(SOOrder order)
		{
			if (order.WillCall == true)
				return GetFromAddress(order);
			else
				return ((SOShippingAddress)PXSelect<SOShippingAddress, Where<SOShippingAddress.addressID, Equal<Required<SOOrder.shipAddressID>>>>.Select(Base, order.ShipAddressID)).With(ValidAddressFrom<SOOrder.shipAddressID>);
		}

        public virtual IAddressBase GetToAddress(SOOrder order, SOLine line)
		{
			if (order.WillCall == true && line.SiteID != null && !(line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder))
				return GetFromAddress(order, line); // will call
			else
				return ((SOShippingAddress)PXSelect<SOShippingAddress, Where<SOShippingAddress.addressID, Equal<Required<SOOrder.shipAddressID>>>>.Select(Base, order.ShipAddressID)).With(ValidAddressFrom<SOOrder.shipAddressID>);
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
			if (typeof(TFieldSource) == typeof(SOOrder.shipAddressID))
				return PXSelect<SOOrder, Where<SOOrder.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((SOAddress)address).AddressID).First().GetItem<SOOrder>()
					.With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<SOOrder>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(Vendor.defLocationID))
				return PXSelectReadonly2<Vendor, InnerJoin<Location, On<Location.locationID, Equal<Vendor.defLocationID>>>, Where<Location.defAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<Vendor>()
					.With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<Vendor>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(INSite.addressID))
				return PXSelectReadonly<INSite, Where<INSite.addressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<INSite>()
					.With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<INSite>(), new EntityHelper(Base).GetRowID(e)));

			if (typeof(TFieldSource) == typeof(BAccountR.defAddressID))
				return PXSelectReadonly<BAccountR, Where<BAccountR.defAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<BAccountR>()
					.With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<BAccountR>(), new EntityHelper(Base).GetRowID(e)));

			throw new ArgumentOutOfRangeException("Unknown address source used");
		}

		protected virtual bool IsCommonCarrier(string carrierID)
		{
			if (string.IsNullOrEmpty(carrierID))
			{
				return false; //pickup;
			}
			else
			{
				Carrier carrier = Carrier.PK.Find(Base, carrierID);
				if (carrier == null)
				{
					return false;
				}
				else
				{
					return carrier.IsCommonCarrier == true;
				}
			}
		}

		private void InvalidateExternalTax(SOOrder order)
		{
			if (order == null || !CalculateTaxesUsingExternalProvider(order.TaxZoneID)) return;
			order.IsTaxValid = false;
			order.IsOpenTaxValid = false;
			order.IsUnbilledTaxValid = false;
		}

		public delegate IEnumerable ActionDelegate(PXAdapter adapter, int? actionID, DateTime? shipDate, int? siteID, string operation, string actionName);
		[PXOverride]
		public virtual IEnumerable Action(
			PXAdapter adapter, int? actionID, DateTime? shipDate, int? siteID, string operation, string actionName,
			ActionDelegate baseMethod)
		{
			bool reopenOrderAction = string.Equals(actionName, nameof(Base.reopenOrder), StringComparison.OrdinalIgnoreCase);
			bool skipExternalTaxCalcOnSaveOldValue = skipExternalTaxCalcOnSave;
			// skip calculation of external taxes on Re-open of Completed orders because PXLongOperation will cancel the update of SOOrder
			if (reopenOrderAction) skipExternalTaxCalcOnSave = true;
			try
			{
				return baseMethod(adapter, actionID, shipDate, siteID, operation, actionName);
			}
			finally
			{
				skipExternalTaxCalcOnSave = skipExternalTaxCalcOnSaveOldValue;
			}
		}
	}
}
