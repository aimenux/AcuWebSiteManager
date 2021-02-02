using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.Extensions.CustomerCreditHold
{
	public class CreditVerificationResult
	{
		public bool Hold
		{
			get;
			set;
		}

		public bool Failed
		{
			get;
			set;
		}

		public bool Enforce
		{
			get;
			set;
		}

		public string ErrorMessage
		{
			get;
			set;
		}

		public string ErrorField
		{
			get;
			set;
		} = nameof(ARInvoice.customerID);
	}

	/// <summary>A generic graph extension that defines the credit helper functionality.</summary>
	/// <typeparam name="TGraph">A <see cref="PX.Data.PXGraph" /> type.</typeparam>
	public abstract class CustomerCreditExtension<
			TGraph,
			TDoc,
			TCustomerIDField,
			TCreditHoldField,
			TReleasedField,
			TStatusField
		> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TDoc : class, IBqlTable, new()
		where TCustomerIDField : IBqlField
		where TCreditHoldField : IBqlField
		where TReleasedField : IBqlField
		where TStatusField : IBqlField
	{
		protected bool _InternalCall = false;

		protected virtual void _(Events.RowPersisting<TDoc> e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
				Verify(e.Cache, e.Row, e.Args);
		}

		protected virtual void _(Events.RowUpdated<TDoc> e)
		{
			if (_InternalCall)
			{
				return;
			}

			try
			{
				_InternalCall = true;

				if (!e.Cache.ObjectsEqual<TCustomerIDField>(e.Row, e.OldRow))
				{
					e.Cache.RaiseExceptionHandling<TCustomerIDField>(e.Row, null, null);
					e.Cache.RaiseExceptionHandling<TCreditHoldField>(e.Row, null, null);
				}

				Verify(e.Cache, e.Row, e.Args);
			}
			finally
			{
				_InternalCall = false;
			}

		}

		protected virtual void _(Events.RowSelected<TDoc> e)
		{
			Verify(e.Cache, e.Row, e.Args);
		}

		protected virtual decimal? GetDocumentBalance(PXCache cache, TDoc Row)
		{
			ARBalances accumbal = cache.Current as ARBalances;
			if (accumbal != null && cache.GetStatus(accumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				return accumbal.CurrentBal + accumbal.UnreleasedBal + accumbal.TotalOpenOrders;
			}

			return 0m;
		}

		protected virtual void GetCustomerBalance(PXCache cache, Customer customer, out decimal? CustomerBal,
			out DateTime? OldInvoiceDate)
		{
			ARBalances summarybal;
			using (new PXConnectionScope())
			{
				summarybal =
					CustomerMaint.GetCustomerBalances<PX.Objects.AR.Override.Customer.sharedCreditCustomerID>(
						cache.Graph, customer.SharedCreditCustomerID);
			}

			CustomerBal = 0m;
			var curaccumbal = cache.Current as ARBalances;
			if (curaccumbal != null && cache.GetStatus(curaccumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				foreach (ARBalances accumbal in cache.Inserted)
				{
					if (accumbal.CustomerID != curaccumbal.CustomerID)
						continue;
					CustomerBal += accumbal.CurrentBal + accumbal.UnreleasedBal + accumbal.TotalOpenOrders;
				}
			}

			OldInvoiceDate = null;
			if (summarybal != null)
			{
				CustomerBal += (summarybal.CurrentBal ?? 0m);
				CustomerBal += (summarybal.UnreleasedBal ?? 0m);
				CustomerBal += (summarybal.TotalOpenOrders ?? 0m);
				CustomerBal += (summarybal.TotalShipped ?? 0m);
				CustomerBal -= (summarybal.TotalPrepayments ?? 0m);

				OldInvoiceDate = summarybal.OldInvoiceDate;
			}
		}

		protected virtual bool? GetHoldValue(PXCache sender, TDoc Row)
		{
			return (bool?)sender.GetValue<TCreditHoldField>(Row);
		}

		protected virtual bool? GetReleasedValue(PXCache sender, TDoc Row)
		{
			return (bool?)sender.GetValue<TReleasedField>(Row);
		}

		protected virtual bool? GetCreditCheckError(PXCache sender, TDoc Row)
		{
			ARSetup arsetup = GetARSetup();
			return arsetup?.CreditCheckError;
		}

		protected virtual bool? IsMigrationMode(PXCache sender)
		{
			ARSetup arsetup = GetARSetup();
			return arsetup?.MigrationMode;
		}

		protected virtual void PlaceOnHold(PXCache sender, TDoc Row, bool OnAdminHold)
		{
			sender.RaiseExceptionHandling<TStatusField>(Row, true,
				new PXSetPropertyException(AR.Messages.CreditHoldEntry, PXErrorLevel.Warning));
		}

		public virtual void Verify(PXCache sender, TDoc Row, EventArgs e)
		{
			PXCache arbalancescache = sender.Graph.Caches[typeof(ARBalances)];
			ARBalances arbalances = (ARBalances)arbalancescache.Current;

			Customer customer = EnsureCustomer(sender, Row);
			CustomerClass customerclass = EnsureCustomerClass(sender, customer);

			if (customer != null && IsMigrationMode(sender) != true)
			{
				if (arbalances != null && customer.BAccountID != arbalances.CustomerID)
				{
					arbalancescache.Current = null;
				}

				//On graph load Current for setups can be null
				if (customer == null || customerclass == null) return;

				CreditVerificationResult res = VerifyByCreditRules(sender, Row, customer, customerclass);

				ApplyCreditVerificationResult(sender, Row, e, customer, res);
			}
		}

		protected virtual CreditVerificationResult VerifyByCreditRules(
			PXCache sender, TDoc Row,
			Customer customer, CustomerClass customerclass)
		{
			var res = new CreditVerificationResult();

			res.Hold = GetHoldValue(sender, Row) == true || GetReleasedValue(sender, Row) == true;

			if (customer.CreditRule == CreditRuleTypes.CS_NO_CHECKING)
			{
				return res;
			}

			PXCache arbalancescache = sender.Graph.Caches[typeof(ARBalances)];
			decimal? CustomerBal;
			DateTime? OldInvoiceDate;
			GetCustomerBalance(arbalancescache, customer, out CustomerBal, out OldInvoiceDate);

			TimeSpan overdue = (DateTime)sender.Graph.Accessinfo.BusinessDate -
							   (DateTime)(OldInvoiceDate ?? sender.Graph.Accessinfo.BusinessDate);

			res.Failed = GetReleasedValue(sender, Row) ?? false;

			if (res.Failed == false && (customer.CreditRule == CreditRuleTypes.CS_BOTH ||
									customer.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE))
			{
				if (overdue.Days > customer.CreditDaysPastDue)
				{
					res.ErrorMessage = PX.Objects.AR.Messages.CreditDaysPastDueWereExceeded;
				}

				if (IsPutOnCreditHoldAllowed(arbalancescache, Row) && overdue.Days > customer.CreditDaysPastDue)
				{
					res.Failed = true;
				}
			}

			if (res.Failed == false && (customer.CreditRule == CreditRuleTypes.CS_BOTH ||
									customer.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT))
			{
				if (CustomerBal > customer.CreditLimit)
				{
					res.ErrorMessage = PX.Objects.AR.Messages.CreditLimitWasExceeded;
				}

				if (IsPutOnCreditHoldAllowed(arbalancescache, Row) && CustomerBal > customer.CreditLimit + customerclass.OverLimitAmount)
				{
					res.Failed = true;
				}
			}

			if (res.Failed == false && customer.Status == BAccount.status.CreditHold)
			{
				res.ErrorMessage = PX.Objects.AR.Messages.CustomerIsOnCreditHold;
				if (IsPutOnCreditHoldAllowed(arbalancescache, Row))
				{
					res.Enforce = true;
					res.Failed = true;
				}
			}

			if (res.Failed == false && customer.Status == BAccount.status.Hold)
			{
				res.ErrorMessage = PX.Objects.AR.Messages.CustomerIsOnHold;
				res.Failed = true;
				res.Enforce = true;
			}

			if (res.Failed == false && customer.Status == BAccount.status.Inactive)
			{
				res.ErrorMessage = PX.Objects.AR.Messages.CustomerIsInactive;
				res.Failed = true;
				res.Enforce = true;
			}

			return res;
		}

		public virtual void ApplyCreditVerificationResult(
			PXCache sender, TDoc Row, EventArgs e,
			Customer customer, CreditVerificationResult res)
		{
			PXCache arbalancescache = sender.Graph.Caches[typeof(ARBalances)];

			if (!string.IsNullOrEmpty(res.ErrorMessage))
			{
				string existingError = PXUIFieldAttribute.GetError(sender, Row, res.ErrorField);
				if (string.IsNullOrEmpty(existingError))
				{
					object value = sender.GetValue(Row, res.ErrorField);
					sender.RaiseExceptionHandling(res.ErrorField, Row, value,
						new PXSetPropertyException(res.ErrorMessage, PXErrorLevel.Warning));
				}
			}

			if (res.Failed && res.Hold == false)
			{
				if (e is PXRowUpdatedEventArgs && (res.Enforce || GetCreditCheckError(sender, Row) == true))
				{
					TDoc OldRow = (TDoc)sender.CreateCopy(Row);
					sender.SetValueExt<TCreditHoldField>(Row, true);
					UpdateARBalances(sender, Row, OldRow);

					decimal? DocumentBal = GetDocumentBalance(arbalancescache, Row);

					//this is a Credit Memo
					if (DocumentBal > 0m)
					{
						OldRow = (TDoc)sender.CreateCopy(Row);
						sender.SetValueExt<TCreditHoldField>(Row, false);
						UpdateARBalances(sender, Row, OldRow);
					}
					else
					{
						PlaceOnHold(sender, Row, res.Enforce);
					}
				}
				else if (e is PXRowPersistingEventArgs &&
						 ((((PXRowPersistingEventArgs)e).Operation & PXDBOperation.Command) !=
						  PXDBOperation.Delete) &&
						 (res.Enforce || GetCreditCheckError(sender, Row) == true))
				{
					if (string.IsNullOrEmpty(res.ErrorMessage) == false)
					{
						TDoc OldRow = (TDoc)sender.CreateCopy(Row);
						sender.SetValueExt<TCreditHoldField>(Row, true);
						UpdateARBalances(sender, Row, OldRow);

						decimal? DocumentBal = GetDocumentBalance(arbalancescache, Row);

						OldRow = (TDoc)sender.CreateCopy(Row);
						sender.SetValueExt<TCreditHoldField>(Row, false);
						UpdateARBalances(sender, Row, OldRow);

						//this is not a Credit Memo
						if (DocumentBal <= 0m)
						{
							throw new PXException(res.ErrorMessage);
						}
					}
				}
			}
		}

		protected virtual bool IsPutOnCreditHoldAllowed(PXCache cache, TDoc Row)
		{
			decimal? DocumentBal = GetDocumentBalance(cache, Row);
			return DocumentBal > 0m;
		}

		protected virtual Customer EnsureCustomer(PXCache sender, TDoc Row)
		{
			PXCache customercache = sender.Graph.Caches[typeof(Customer)];
			int? customerID = (int?)sender.GetValue<TCustomerIDField>(Row);
			Customer customer = (Customer)customercache.Current;
			if (object.Equals(customer?.BAccountID, customerID) == false)
			{
				customercache.Current = null;
				customer = PXSelect<Customer,
						Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.SelectSingleBound(sender.Graph, null, customerID);
			}
			return customer;
		}

		protected virtual CustomerClass EnsureCustomerClass(PXCache sender, Customer customer)
		{
			PXCache customerclasscache = sender.Graph.Caches[typeof(CustomerClass)];
			CustomerClass customerclass = (CustomerClass)customerclasscache.Current;
			if (customer != null && IsMigrationMode(sender) != true
				&& object.Equals(customerclass?.CustomerClassID, customer.CustomerClassID) == false)
			{
				customerclasscache.Current = null;
				customerclass = PXSelect<CustomerClass,
						Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>
					.SelectSingleBound(sender.Graph, null, customer.CustomerClassID);
			}
			return customerclass;
		}

		public abstract void UpdateARBalances(PXCache cache, TDoc newRow, TDoc oldRow);

		protected abstract ARSetup GetARSetup();
	}
}