using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARExpiringCardsEnq: PXGraph<ARExpiringCardsEnq>
	{
		#region Internal Types
		[Serializable]
		public partial class ARExpiringCardFilter: IBqlTable
		{
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }

			protected DateTime? _BeginDate;

			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? BeginDate
			{
				get
				{
					return this._BeginDate;
				}
				set
				{
					this._BeginDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? EndDate
			{
				get
				{
					if (this._BeginDate.HasValue)
						return _BeginDate.Value.AddDays(_ExpireXDays ?? 0);
					return this._BeginDate;
				}
				set
				{
				}
			}
			#endregion
			#region ExpireXDays
			public abstract class expireXDays : PX.Data.BQL.BqlInt.Field<expireXDays> { }

			protected Int32? _ExpireXDays;

			[PXDBInt()]
			[PXDefault(30)]
			[PXUIField(DisplayName = "Duration")]
			public virtual Int32? ExpireXDays
			{
				get
				{
					return this._ExpireXDays;
				}
				set
				{
					this._ExpireXDays = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual String CustomerClassID
			{
				get
				{
					return this._CustomerClassID;
				}
				set
				{
					this._CustomerClassID = value;
				}
			}
			#endregion			
			#region ActiveOnly
			public abstract class activeOnly : PX.Data.BQL.BqlBool.Field<activeOnly> { }
			protected Boolean? _ActiveOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Active Only ")]
			public virtual Boolean? ActiveOnly
			{
				get
				{
					return this._ActiveOnly;
				}
				set
				{
					this._ActiveOnly = value;					
				}
			}
			#endregion
		}
		#endregion
		#region Member Decalaration
		public PXFilter<ARExpiringCardFilter> Filter;
		public PXCancel<ARExpiringCardFilter> Cancel;
		#endregion

		public ARExpiringCardsEnq() 
		{
			this.Cards.Cache.AllowInsert = false;
			this.Cards.Cache.AllowUpdate = false;
			this.Cards.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetDisplayName(Caches[typeof(CR.Contact)], typeof(CR.Contact.salutation).Name, CR.Messages.Attention);
		}
		[PXFilterable]
		public PXSelectJoin<CustomerPaymentMethod,
							InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>,
							LeftJoin<CR.Contact, On<CR.Contact.bAccountID,Equal<Customer.bAccountID>,
									   And<CR.Contact.contactID,Equal<Customer.defBillContactID>>>>>> Cards; 
							//Where<CustomerPaymentMethod.expirationDate, GreaterEqual<Current<ARExpiringCardFilter.beginDate>>,
							//And<CustomerPaymentMethod.expirationDate, LessEqual<Current<ARExpiringCardFilter.endDate>>,							
							//And<CustomerPaymentMethod.isActive, Equal<BQLConstants.BitOn>,
							////Or<Current<ARExpiringCardFilter.activeOnly>,Equal<BQLConstants.BitOff>>>,
							//And<Where<Customer.customerClassID,Equal<Current<ARExpiringCardFilter.customerClassID>>,
							//Or<Current<ARExpiringCardFilter.customerClassID>, IsNull>>>>>>> Cards;

		public virtual IEnumerable cards() 
		{
			ARExpiringCardFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<CustomerPaymentMethod> select = new PXSelectJoin<CustomerPaymentMethod,
								InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>,
								LeftJoin<CR.Contact, On<CR.Contact.bAccountID,Equal<Customer.bAccountID>,
									   And<CR.Contact.contactID,Equal<Customer.defBillContactID>>>>>,
								Where<CustomerPaymentMethod.expirationDate, GreaterEqual<Required<CustomerPaymentMethod.expirationDate>>,
								And<CustomerPaymentMethod.expirationDate, LessEqual<Required<CustomerPaymentMethod.expirationDate>>>>>(this);

				if (!string.IsNullOrEmpty(filter.CustomerClassID)) 
				{
					select.WhereAnd<Where<Customer.customerClassID, Equal<Required<Customer.customerClassID>>>>();
				}

				if ((bool)filter.ActiveOnly) 
				{
					select.WhereAnd<Where<CustomerPaymentMethod.isActive, Equal<BQLConstants.BitOn>>>();
				}

				return select.Select(filter.BeginDate, filter.EndDate, filter.CustomerClassID);
			}
			return null;

		}

		public virtual void ARExpiringCardFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e) 
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
			
			/*CustomerPaymentMethod firstExpCard = PXSelect<CustomerPaymentMethod,
													Where<CustomerPaymentMethod.expirationDate, 
													GreaterEqual<Required<CustomerPaymentMethod.expirationDate>>>,
													OrderBy<Asc<CustomerPaymentMethod.expirationDate>>>.Select(this,Accessinfo.BusinessDate.Value);
			if (firstExpCard != null)
			{
				TimeSpan diff = firstExpCard.ExpirationDate.Value - Accessinfo.BusinessDate.Value;
			
				if (diff.Days > PeriodLength)
				{
					DateTime cardDate = firstExpCard.ExpirationDate.Value;
					e.NewValue = new DateTime(cardDate.Year,cardDate.Month,cardDate.Day);
					e.Cancel = true;
				}
			}*/
		}

		//private static int PeriodLength = 31;		

		#region Sub-screen Navigation Button
		public PXAction<ARExpiringCardFilter> viewCustomer;
		[PXUIField(DisplayName = Messages.ViewCustomer, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewCustomer(PXAdapter adapter)
		{
			if (this.Cards.Current != null)
			{
				Customer customer = PXSelect<Customer, 
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, this.Cards.Current.BAccountID);
				CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
				graph.BAccount.Current = customer;
				throw new PXRedirectRequiredException(graph, Messages.ViewCustomer);
			}
			return adapter.Get();
		}

		public PXAction<ARExpiringCardFilter> viewPaymentMethod;
		[PXUIField(DisplayName = Messages.ViewPaymentMethod, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewPaymentMethod(PXAdapter adapter)
		{
			if (this.Cards.Current != null)
			{
				CustomerPaymentMethod payMethod = PXSelect<CustomerPaymentMethod,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
					.Select(this, this.Cards.Current.PMInstanceID);
				CustomerPaymentMethodMaint graph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
				graph.CustomerPaymentMethod.Current = payMethod;
				throw new PXPopupRedirectException(graph, Messages.ViewPaymentMethod, true);
			}
			return adapter.Get();
		}
		#endregion
	}
}
