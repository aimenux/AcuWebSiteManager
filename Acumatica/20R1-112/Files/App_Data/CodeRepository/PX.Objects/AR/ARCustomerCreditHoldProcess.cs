using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;
using System.Linq;

namespace PX.Objects.AR
{
	public class ARCustomerCreditHoldProcess : PXGraph<ARCustomerCreditHoldProcess>
	{
		#region Internal Types

		[System.SerializableAttribute()]
		public partial class CreditHoldParameters : IBqlTable
		{
			#region Action
			public abstract class action : PX.Data.BQL.BqlInt.Field<action> { }
			protected int? _Action;
			[PXDBInt]
			[PXDefault(0)]
			[PXUIField(DisplayName = "Action")]
			[PXIntList(new [] { ActionApplyCreditHold, ActionReleaseCreditHold }, 
				new [] { Messages.ApplyCreditHoldMsg, Messages.ReleaseCreditHoldMsg })]
			public virtual int? Action
			{
				get
				{
					return _Action;
				}
				set
				{
					_Action = value;
				}
			}

			public const int ActionApplyCreditHold = 0;
			public const int ActionReleaseCreditHold = 1;
			#endregion
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
			protected DateTime? _BeginDate;
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? BeginDate
			{
				get
				{
					return _BeginDate;
				}
				set
				{
					_BeginDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? EndDate
			{
				get
				{
					return _EndDate;
				}
				set
				{
					_EndDate = value;
				}
			}
			#endregion
			#region ShowAll
			public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
			protected bool? _ShowAll = false;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show All")]
			public virtual bool? ShowAll
			{
				get
				{
					return _ShowAll;
				}
				set
				{
					_ShowAll = value;
				}
			}
			#endregion

			public string IncludedCustomerStatus
			{
				get
				{
					switch(Action)
					{
						case ActionApplyCreditHold:
							return BAccount.status.Active;
						case ActionReleaseCreditHold:
							return BAccount.status.CreditHold;
						default:
							return null;
					}
				}
			}
		}

		[Serializable]
		public partial class DetailsResult : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion
			#region DunningLetterID
			public abstract class dunningLetterID : PX.Data.BQL.BqlInt.Field<dunningLetterID> { }
			protected int? _DunningLetterID;
            [PXInt]
			[PXUIField(Enabled = false)]
			public virtual int? DunningLetterID
			{
				get
				{
					return _DunningLetterID;
				}
				set
				{
					_DunningLetterID = value;
				}
			}
			#endregion
			#region CustomerId
			public abstract class customerId : PX.Data.BQL.BqlInt.Field<customerId> { }
			protected int? _CustomerId;
			[PXInt(IsKey = true)]
			[Customer(DescriptionField = typeof(Customer.acctName))]
			[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
			public virtual int? CustomerId
			{
				get
				{
					return _CustomerId;
				}
				set
				{
					_CustomerId = value;
				}
			}
			#endregion
			#region DunningLetterDate
			public abstract class dunningLetterDate : PX.Data.BQL.BqlDateTime.Field<dunningLetterDate> { }
			protected DateTime? _DunningLetterDate;
			[PXDBDate()]
			[PXDefault(TypeCode.DateTime, "01/01/1900")]
			[PXUIField(DisplayName = "Dunning Letter Date")]
			public virtual DateTime? DunningLetterDate
			{
				get
				{
					return _DunningLetterDate;
				}
				set
				{
					_DunningLetterDate = value;
				}
			}
			#endregion
			#region DocBal
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			protected decimal? _DocBal;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overdue Balance")]
			public virtual decimal? DocBal
			{
				get
				{
					return _DocBal;
				}
				set
				{
					_DocBal = value;
				}
			}
			#endregion
			#region InvBal
			public abstract class invBal : PX.Data.BQL.BqlDecimal.Field<invBal> { }
			protected decimal? _InvBal;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Customer Balance")]
			public virtual decimal? InvBal
			{
				get
				{
					return _InvBal;
				}
				set
				{
					_InvBal = value;
				}
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status>
			{
			}
			protected string _Status;
			[PXUIField(DisplayName = "Status")]
			[BAccount.status.List()]
			public virtual string Status
			{
				get
				{
					return _Status;
				}
				set
				{
					_Status = value;
				}
			}
			#endregion

			/// <summary>
			/// Copy field values from selection and aggregate details
			/// </summary>
			/// <param name="G">Graph</param>
			/// <param name="aSrc">Selected DunningLetter</param>
			/// <param name="cust">Selected Customer</param>
			public virtual void Copy(PXGraph G, ARDunningLetter aSrc, Customer cust)
			{
				CustomerId = cust.BAccountID;
				DunningLetterID = aSrc.DunningLetterID;
				DunningLetterDate = aSrc.DunningLetterDate;
				DocBal = 0;
				InvBal = 0;
				Status = cust.Status;

				foreach (PXResult<ARDunningLetterDetail> it in PXSelect<ARDunningLetterDetail,
					Where<ARDunningLetterDetail.dunningLetterID,
						Equal<Required<ARDunningLetterDetail.dunningLetterID>>>>
					.Select(G, aSrc.DunningLetterID))
				{
					ARDunningLetterDetail d = it;
					DocBal += d.OverdueBal;
				}
			}
		}

		#endregion

		#region Public members

		public ARCustomerCreditHoldProcess()
		{
			Details.Cache.AllowDelete = false;
			Details.Cache.AllowInsert = false;
			Details.SetSelected<ARInvoice.selected>();
			Details.SetProcessCaption(IN.Messages.Process);
			Details.SetProcessAllCaption(IN.Messages.ProcessAll);
		}

		public PXCancel<CreditHoldParameters> Cancel;
		public PXFilter<CreditHoldParameters> Filter;

        [PXFilterable]
		public PXFilteredProcessing<DetailsResult, CreditHoldParameters> Details;

		#endregion

		#region Delegates

		/// <summary>
		/// Generates a list of documents that meet the filter criteria.
		/// This list is used for display in the processing screen
		/// </summary>
		/// <returns>List of Customers with Dunning Letters</returns>
		protected virtual IEnumerable details()
		{
			CreditHoldParameters header = Filter.Current;
			List<DetailsResult> result = new List<DetailsResult>();
			if (header == null) yield break;

			foreach (PXResult<Customer, ARDunningLetter> record in GetCustomersToProcess(header))
			{
				ARDunningLetter dunningLetter = record;
				Customer customer = record;
				if (header.ShowAll == false && customer.Status != header.IncludedCustomerStatus) continue;

				DetailsResult res = new DetailsResult();
				res.Copy(this, dunningLetter, customer);

				ARBalances balances = CustomerMaint.GetCustomerBalances<Override.Customer.sharedCreditCustomerID>(this, customer.BAccountID);
				if (balances != null)
				{
					res.InvBal = balances.CurrentBal ?? 0.0m;
				}
				result.Add(res);
			}

			foreach (var item in result)
			{
				Details.Cache.SetStatus(item, PXEntryStatus.Held);
				yield return item;
			}
			Details.Cache.IsDirty = false;
		}

		protected PXResultset<Customer> GetCustomersToProcess(CreditHoldParameters header)
		{
			switch (header.Action)
			{
				case CreditHoldParameters.ActionApplyCreditHold:
					
					return PXSelectJoin<Customer,
						InnerJoin<ARDunningLetter, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>,
							And<ARDunningLetter.lastLevel, Equal<True>, 
							And<ARDunningLetter.released, Equal<True>,
							And<ARDunningLetter.voided, NotEqual<True>>>>>>,
						Where<ARDunningLetter.dunningLetterDate,
							Between<Required<ARDunningLetter.dunningLetterDate>, Required<ARDunningLetter.dunningLetterDate>>>,
						OrderBy<Asc<ARDunningLetter.bAccountID>>>.Select(this, header.BeginDate, header.EndDate);

				case CreditHoldParameters.ActionReleaseCreditHold:
					
					PXSelectBase<Customer> select = new PXSelectJoin<Customer,
						LeftJoin<ARDunningLetter, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>,
							And<ARDunningLetter.lastLevel, Equal<True>, 
							And<ARDunningLetter.released, Equal<True>, 
							And<ARDunningLetter.voided, NotEqual<True>>>>>>>(this);
					if (PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>())
					{
						select.WhereAnd<Where<Customer.bAccountID, Equal<Customer.sharedCreditCustomerID>>>();
					}
					return select.Select();

				default:
					return new PXResultset<Customer>();
			}
		}

		#endregion

		#region Filter Events

		protected virtual void CreditHoldParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CreditHoldParameters header = e.Row as CreditHoldParameters;
			if (header == null) 
				return;

			Details.SetProcessDelegate(delegate(CustomerMaint graph, DetailsResult res)
			{
				Customer customer = graph.BAccount.Search<Customer.bAccountID>(res.CustomerId);
				if (customer != null)
				{
					graph.BAccount.Current = customer;
					Customer copy = (Customer)graph.BAccount.Cache.CreateCopy(customer);
					copy.Status = header.Action == CreditHoldParameters.ActionApplyCreditHold
						? BAccount.status.CreditHold
						: BAccount.status.Active;
					graph.BAccount.Cache.Update(copy);
					graph.Actions.PressSave();
				}
			});

			bool allowDatesFiltering = header.Action == CreditHoldParameters.ActionApplyCreditHold;
			PXUIFieldAttribute.SetEnabled<CreditHoldParameters.beginDate>(sender, header, allowDatesFiltering);
			PXUIFieldAttribute.SetEnabled<CreditHoldParameters.endDate>(sender, header, allowDatesFiltering);
		}

		protected virtual void CreditHoldParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Details.Cache.Clear();
		}

		protected virtual void CreditHoldParameters_RowPersisting(PXCache sedner, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion
	}
}