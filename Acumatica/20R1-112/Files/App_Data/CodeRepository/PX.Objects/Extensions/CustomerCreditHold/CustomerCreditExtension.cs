using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.Extensions.CustomerCreditHold
{
	/// <summary>
	/// A mapped cache extension that represents a generic Document entity. This class should be mapped to an actual Document entity to implement customer credit verification.
	/// </summary>
	[PXHidden]
	public class Document : PXMappedCacheExtension
	{
		#region CustomerID

		/// <exclude/>
		public virtual Int32? CustomerID { get; set; }
		/// <summary>
		/// This field should be mapped to a field that contains CustomerID.
		/// </summary>
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		#endregion

		#region Hold

		/// <exclude/>
		public virtual Boolean? Hold { get; set; }
		/// <summary>
		/// This field should be mapped to a field that contains Hold or CreditHold value.
		/// </summary>
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		#endregion

		#region Released

		/// <exclude/>
		public virtual Boolean? Released { get; set; }
		/// <summary>
		/// This field should be mapped to a field that contains Released value.
		/// </summary>
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		#endregion
		#region Status

		/// <exclude/>
		public virtual Boolean? Status { get; set; }
		/// <summary>
		/// This field should be mapped to a field that contains Status (document status) value.
		/// </summary>
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		#endregion
	}

	/// <summary>A generic graph extension that defines the credit helper functionality.</summary>
	/// <typeparam name="TGraph">A <see cref="PX.Data.PXGraph" /> type.</typeparam>
	public abstract class CustomerCreditExtension<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		protected bool _InternalCall = false;

		public PXSelectExtension<Document> Document;

		protected abstract DocumentMapping GetDocumentMapping();

		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);
			protected Type _table;
			public Type Table => _table;

			public DocumentMapping(Type table)
			{
				_table = table;
			}
			public Type CustomerID = typeof(Document.customerID);
			public Type Hold = typeof(Document.hold);
			public Type Released = typeof(Document.released);
			public Type Status = typeof(Document.status);
		}

		protected virtual void _(Events.RowPersisting<Document> e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
				Verify(e.Cache, e.Row, e.Args);
		}

		protected virtual void _(Events.RowUpdated<Document> e)
		{
			if (_InternalCall)
			{
				return;
			}

			try
			{
				_InternalCall = true;

				if ((int?)e.Cache.GetValue<Document.customerID>(e.Row) !=
					(int?)e.Cache.GetValue<Document.customerID>(e.OldRow))
				{
					e.Cache.RaiseExceptionHandling<Document.customerID>(e.Row, null, null);
					e.Cache.RaiseExceptionHandling<Document.hold>(e.Row, null, null);
				}

				Verify(e.Cache, e.Row, e.Args);
			}
			finally
			{
				_InternalCall = false;
			}

		}

		protected virtual void _(Events.RowSelected<Document> e)
		{
			Verify(e.Cache, e.Row, e.Args);
		}

		protected virtual decimal? GetDocumentBalance(PXCache cache, object Row)
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

		protected virtual bool? GetHoldValue(PXCache sender, object Row)
		{
			return (bool?)sender.GetValue<Document.hold>(Row);
		}

		protected virtual bool? GetReleasedValue(PXCache sender, object Row)
		{
			return (bool?)sender.GetValue<Document.released>(Row);
		}

		protected virtual bool? GetCreditCheckError(PXCache sender, object Row)
		{
			ARSetup arsetup = PXSetup<ARSetup>.Select(sender.Graph);
			return arsetup?.CreditCheckError;
		}

		protected virtual bool? IsMigrationMode(PXCache sender)
		{
			ARSetup arsetup = PXSetup<ARSetup>.Select(sender.Graph);
			return arsetup?.MigrationMode;
		}

		protected virtual void PlaceOnHold(PXCache sender, object Row, bool OnAdminHold)
		{
			sender.RaiseExceptionHandling<Document.status>(Row, true,
				new PXSetPropertyException(PX.Objects.AR.Messages.CreditHoldEntry, PXErrorLevel.Warning));
		}

		public virtual void Verify(PXCache sender, object Row, EventArgs e)
		{
			PXCache customercache = sender.Graph.Caches[typeof(Customer)];
			PXCache customerclasscache = sender.Graph.Caches[typeof(CustomerClass)];
			PXCache arbalancescache = sender.Graph.Caches[typeof(ARBalances)];

			int? CustomerID = (int?)sender.GetValue<Document.customerID>(Row);
			bool? HoldValue = GetHoldValue(sender, Row);
			bool? ReleasedValue = GetReleasedValue(sender, Row);
			string errmsg = null;

			if (ReleasedValue == true)
			{
				HoldValue = true;
			}

			Customer customer = (Customer)customercache.Current;
			CustomerClass customerclass = (CustomerClass)customerclasscache.Current;
			ARBalances arbalances = (ARBalances)arbalancescache.Current;

			if (customer != null && object.Equals(customer.BAccountID, CustomerID) == false)
			{
				customercache.Current = null;
				customer = PXSelect<Customer,
						Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.SelectSingleBound(sender.Graph, null, CustomerID);
			}

			if (customer != null && customer.CreditRule != CreditRuleTypes.CS_NO_CHECKING &&
				IsMigrationMode(sender) != true)
			{
				if (arbalances != null && customer.BAccountID != arbalances.CustomerID)
				{
					arbalancescache.Current = null;
				}

				if (customerclass != null &&
					object.Equals(customerclass.CustomerClassID, customer.CustomerClassID) == false)
				{
					customerclasscache.Current = null;
				}

				decimal? DocumentBal = GetDocumentBalance(arbalancescache, Row);

				{
					decimal? CustomerBal;
					DateTime? OldInvoiceDate;
					GetCustomerBalance(arbalancescache, customer, out CustomerBal, out OldInvoiceDate);

					TimeSpan overdue = (DateTime)sender.Graph.Accessinfo.BusinessDate -
									   (DateTime)(OldInvoiceDate ?? sender.Graph.Accessinfo.BusinessDate);

					bool failed = (ReleasedValue ?? false);
					bool enforce = false;

					//On graph load Current for setups can be null
					if (customer == null || customerclass == null) return;
					if (failed == false && (customer.CreditRule == CreditRuleTypes.CS_BOTH ||
											customer.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE))
					{
						if (overdue.Days > customer.CreditDaysPastDue)
						{
							errmsg = PX.Objects.AR.Messages.CreditDaysPastDueWereExceeded;
						}

						if (DocumentBal > 0 && overdue.Days > customer.CreditDaysPastDue)
						{
							failed = true;
						}
					}

					if (failed == false && (customer.CreditRule == CreditRuleTypes.CS_BOTH ||
											customer.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT))
					{
						if (CustomerBal > customer.CreditLimit)
						{
							errmsg = PX.Objects.AR.Messages.CreditLimitWasExceeded;
						}

						if (DocumentBal > 0 && CustomerBal > customer.CreditLimit + customerclass.OverLimitAmount)
						{
							failed = true;
						}
					}

					if (failed == false && customer.Status == BAccount.status.CreditHold)
					{
						errmsg = PX.Objects.AR.Messages.CustomerIsOnCreditHold;
						if (DocumentBal > 0m)
						{
							enforce = true;
							failed = true;
						}
					}

					if (failed == false && customer.Status == BAccount.status.Hold)
					{
						errmsg = PX.Objects.AR.Messages.CustomerIsOnHold;
						failed = true;
						enforce = true;
					}

					if (failed == false && customer.Status == BAccount.status.Inactive)
					{
						errmsg = PX.Objects.AR.Messages.CustomerIsInactive;
						failed = true;
						enforce = true;
					}

					if (!string.IsNullOrEmpty(errmsg))
					{
						string existingError = PXUIFieldAttribute.GetError<Document.customerID>(sender, Row);
						if (string.IsNullOrEmpty(existingError))
							sender.RaiseExceptionHandling<Document.customerID>(Row, customer.AcctCD,
								new PXSetPropertyException(errmsg, PXErrorLevel.Warning));
					}

					if (failed && HoldValue == false)
					{
						if (e is PXRowUpdatedEventArgs && (enforce || GetCreditCheckError(sender, Row) == true))
						{
							object OldRow = sender.CreateCopy(Row);
							sender.SetValueExt<Document.hold>(Row, true);
							UpdateARBalances(sender, Row, OldRow);

							DocumentBal = GetDocumentBalance(arbalancescache, Row);

							//this is a Credit Memo
							if (DocumentBal > 0m)
							{
								OldRow = sender.CreateCopy(Row);
								sender.SetValueExt<Document.hold>(Row, false);
								UpdateARBalances(sender, Row, OldRow);
							}
							else
							{
								PlaceOnHold(sender, Row, enforce);
							}
						}
						else if (e is PXRowPersistingEventArgs &&
								 ((((PXRowPersistingEventArgs)e).Operation & PXDBOperation.Command) !=
								  PXDBOperation.Delete) &&
								 (enforce || GetCreditCheckError(sender, Row) == true))
						{
							if (string.IsNullOrEmpty(errmsg) == false)
							{
								object OldRow = sender.CreateCopy(Row);
								sender.SetValueExt<Document.hold>(Row, true);
								UpdateARBalances(sender, Row, OldRow);

								DocumentBal = GetDocumentBalance(arbalancescache, Row);

								OldRow = sender.CreateCopy(Row);
								sender.SetValueExt<Document.hold>(Row, false);
								UpdateARBalances(sender, Row, OldRow);

								//this is not a Credit Memo
								if (DocumentBal <= 0m)
								{
									throw new PXException(errmsg);
								}
							}
						}
					}
				}
			}
		}

		public abstract void UpdateARBalances(PXCache cache, object newRow, object oldRow);
	}
}