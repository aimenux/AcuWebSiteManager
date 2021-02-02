using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Helpers;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARExpiringCardsProcess: PXGraph<ARExpiringCardsProcess>
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
			[PXUIField(DisplayName = "Starting")]
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

			[PXDBInt(MinValue = 0, MaxValue = 10000)]
			[PXDefault(30)]
			[PXUIField(DisplayName = "Expire in (days)")]
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
			[PXUIField(DisplayName = "Active Only ")]
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
			#region DefaultOnly
			public abstract class defaultOnly : PX.Data.BQL.BqlBool.Field<defaultOnly> { }
			protected Boolean? _DefaultOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Default Payment Method Only")]
			public virtual Boolean? DefaultOnly
			{
				get
				{
					return this._DefaultOnly;
				}
				set
				{
					this._DefaultOnly = value;
				}
			}
			#endregion
			#region NoteLeftLimit
			public abstract class noteLeftLimit : PX.Data.BQL.BqlInt.Field<noteLeftLimit> { }

			protected Int32? _NoteLeftLimit;

			[PXDBInt()]
			[PXDefault(60)]
			[PXUIField(DisplayName = "X Days Before")]
			public virtual Int32? NoteLeftLimit
			{
				get
				{
					return this._NoteLeftLimit;
				}
				set
				{
					this._NoteLeftLimit = value;
				}
			}
			#endregion
			#region NoteRightLimit
			public abstract class noteRightLimit : PX.Data.BQL.BqlInt.Field<noteRightLimit> { }

			protected Int32? _NoteRightLimit;

			[PXDBInt()]
			[PXDefault(180)]
			[PXUIField(DisplayName = "X Days After")]
			public virtual Int32? NoteRightLimit
			{
				get
				{
					return this._NoteRightLimit;
				}
				set
				{
					this._NoteRightLimit = value;
				}
			}
			#endregion
			#region TokenizedPMs

			public abstract class tokenizedPMs : PX.Data.BQL.BqlString.Field<tokenizedPMs> { }
			protected string _TokenizedPMs;
			[PXString()]
			public virtual string TokenizedPMs
			{
				get
				{
					return _TokenizedPMs;
				}
				set
				{
					_TokenizedPMs = value;
				}
			}
			#endregion
		}


		#endregion
		#region Ctor + Member Declaration
		public PXFilter<ARExpiringCardFilter> Filter;
		public PXCancel<ARExpiringCardFilter> Cancel;
        [PXFilterable]
		[PXViewName(Messages.DetailsInputMode)]
        public PXFilteredProcessingJoin<CustomerPaymentMethod, ARExpiringCardFilter,
                                InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>,
                                      And<Customer.status, NotEqual<BAccount.status.inactive>>>,
                                LeftJoin<CR.Contact, On<CR.Contact.bAccountID, Equal<CustomerPaymentMethod.bAccountID>,
                                      And<Where2<Where<CustomerPaymentMethod.billContactID, IsNull,
                                            And<CR.Contact.contactID, Equal<Customer.defBillContactID>>>,
                                                Or<Where<CustomerPaymentMethod.billContactID, IsNotNull,
                                            And<CustomerPaymentMethod.billContactID, Equal<CR.Contact.contactID>>>>>>>>>> Cards;
        public PXAction<ARExpiringCardFilter> viewCustomer;
        public PXAction<ARExpiringCardFilter> viewPaymentMethod;

		public ARExpiringCardsProcess()
		{
			this.Cards.Cache.AllowInsert = false;
			this.Cards.Cache.AllowUpdate = true;
			this.Cards.Cache.AllowDelete = false;

			this.Cards.SetProcessDelegate<CustomerPaymentMethodMassProcess>(MailExpiringNotification);
			PXUIFieldAttribute.SetEnabled(this.Cards.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.selected>(this.Cards.Cache, null, true);
			this.Cards.SetSelected<CustomerPaymentMethod.selected>();
			this.Cards.SetProcessAllCaption(Messages.CCExpirationNotifyAll);
			this.Cards.SetProcessCaption(Messages.CCExpirationNotify);

			PXUIFieldAttribute.SetDisplayName(Caches[typeof(Contact)], typeof(Contact.salutation).Name, CR.Messages.Attention);
		}

        [PXViewName(CR.Messages.MainContact)]
        public PXSelect<Contact> DefaultCompanyContact;
        protected virtual IEnumerable defaultCompanyContact()
        {
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}
		#endregion
		#region Delegates
		public virtual IEnumerable cards()
		{
			ARExpiringCardFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<CustomerPaymentMethod> select = null;
				select = new PXSelectJoin<CustomerPaymentMethod,
								InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>,
								      And<Customer.status, NotEqual<BAccount.status.inactive>>>,
								LeftJoin<CR.Contact, On<CR.Contact.bAccountID, Equal<CustomerPaymentMethod.bAccountID>,
									   And<Where2<Where<CustomerPaymentMethod.billContactID, IsNull, 
                                                    And<CR.Contact.contactID, Equal<Customer.defBillContactID>>>,
                                                            Or<Where<CustomerPaymentMethod.billContactID, IsNotNull,
                                                        And<CustomerPaymentMethod.billContactID,Equal<CR.Contact.contactID>>>>>>>>>,
								Where<CustomerPaymentMethod.expirationDate, GreaterEqual<Required<CustomerPaymentMethod.expirationDate>>,
								And<CustomerPaymentMethod.expirationDate, LessEqual<Required<CustomerPaymentMethod.expirationDate>>>>>(this);
				
				if (!string.IsNullOrEmpty(filter.CustomerClassID))
				{
					select.WhereAnd<Where<Customer.customerClassID, Equal<Required<Customer.customerClassID>>>>();
				}

				if (filter.DefaultOnly == true)
				{
					select.WhereAnd<Where<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>();
				}
				if ((bool)filter.ActiveOnly)
				{
					select.WhereAnd<Where<CustomerPaymentMethod.isActive, Equal<BQLConstants.BitOn>>>();
				}
				foreach (PXResult<CustomerPaymentMethod, Customer, Contact> it in select.Select(filter.BeginDate, filter.EndDate, filter.CustomerClassID, filter.ActiveOnly))
				{
					CustomerPaymentMethod cpm = (CustomerPaymentMethod)it;
					if (cpm.LastNotificationDate.HasValue && cpm.ExpirationDate.HasValue)
					{
						TimeSpan diff = cpm.ExpirationDate.Value - cpm.LastNotificationDate.Value;
						if (diff.TotalDays > 0)
						{
							if (diff.TotalDays < filter.NoteLeftLimit) continue;
						}
						else
						{
							if (Math.Abs(diff.TotalDays) < filter.NoteRightLimit) continue;
						}
					}
					
					yield return it;
				}
			}
			yield break;

		} 
		#endregion

		#region Processing
		public static void MailExpiringNotification(CustomerPaymentMethodMassProcess aGraph, CustomerPaymentMethod aCard)
		{
			aGraph.MailExpirationNotification(aCard);
		} 
		#endregion
		#region Events

		#region Filter
		public virtual void ARExpiringCardFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
		}

		public virtual void ARExpiringCardFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{

			ARExpiringCardFilter row = e.Row as ARExpiringCardFilter;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.selected>(this.Cards.Cache, null, true);
			if (row.TokenizedPMs == null)
			{
				string pmString = CCProcessingHelper.GetTokenizedPMsString(this);
				sender.SetValue<ARExpiringCardFilter.tokenizedPMs>(row, pmString);
			}
			else
			{
				if (row.TokenizedPMs != String.Empty)
				{
					Filter.Cache.RaiseExceptionHandling<ARExpiringCardFilter.customerClassID>(e.Row, ((ARExpiringCardFilter)e.Row).CustomerClassID,
						new PXSetPropertyException(Messages.NotAllCardsShown,
								PXErrorLevel.Warning, row.TokenizedPMs));
				}
			}
		}

		public virtual void ARExpiringCardFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			this.Cards.Cache.Clear();
		}
		
		#endregion

		#region List
		public virtual void CustomerPaymentMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.selected>(this.Cards.Cache, e.Row, true);
		} 
		#endregion

		#endregion
		#region Sub-screen Navigation Button
		[PXUIField(
			DisplayName = Messages.ViewCustomer, 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewCustomer(PXAdapter adapter)
		{
			if (this.Cards.Current != null)
			{
				Customer customer = PXSelect<Customer, 
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, this.Cards.Current.BAccountID);
				CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
				graph.BAccount.Current = customer;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewCustomer) { Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

		[PXUIField(
			DisplayName = Messages.ViewPaymentMethod, 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton]
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

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARExpiredCardsProcess : PXGraph<ARExpiringCardsProcess>
	{
		#region Internal Types
		[Serializable]
		public partial class ARExpiredCardFilter : IBqlTable
		{
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }

			protected DateTime? _BeginDate;

			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Expiration Date")]
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
						return _BeginDate.Value.AddDays(-_ExpireXDays ?? 0);
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

			[PXDBInt(MinValue = 0, MaxValue = 10000)]
			[PXDefault(30)]
			[PXUIField(DisplayName = "Expired Within (Days)")]
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
			#region ExpiredOnly
			public abstract class expiredOnly : PX.Data.BQL.BqlBool.Field<expiredOnly> { }
			protected Boolean? _ExpiredOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Active Only ")]
			public virtual Boolean? ExpiredOnly
			{
				get
				{
					return this._ExpiredOnly;
				}
				set
				{
					this._ExpiredOnly = value;
				}
			}
			#endregion
			#region DefaultOnly
			public abstract class defaultOnly : PX.Data.BQL.BqlBool.Field<defaultOnly> { }
			protected Boolean? _DefaultOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Cards of Default Payment Method Only")]
			public virtual Boolean? DefaultOnly
			{
				get
				{
					return this._DefaultOnly;
				}
				set
				{
					this._DefaultOnly = value;
				}
			}
			#endregion
			#region NotificationSendOnly
			public abstract class notificationSendOnly : PX.Data.BQL.BqlBool.Field<notificationSendOnly> { }
			protected Boolean? _NotificationSendOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Cards of Notified Customers Only")]
			public virtual Boolean? NotificationSendOnly
			{
				get
				{
					return this._NotificationSendOnly;
				}
				set
				{
					this._NotificationSendOnly = value;
				}
			}
			#endregion
			#region TokenizedPMs
			public abstract class tokenizedPMs : PX.Data.BQL.BqlString.Field<tokenizedPMs> { }
			protected string _TokenizedPMs;
			[PXString()]
			public virtual string TokenizedPMs
			{
				get
				{
					return _TokenizedPMs;
				}
				set
				{
					_TokenizedPMs = value;
				}
		}

			#endregion
		}


		#endregion
		#region Member Decalaration
		public PXFilter<ARExpiredCardFilter> Filter;
		public PXCancel<ARExpiredCardFilter> Cancel;
        [PXFilterable]
        public PXFilteredProcessingJoin<CustomerPaymentMethod, ARExpiredCardFilter,
                   InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>,
                   And<Customer.status, NotEqual<BAccount.status.inactive>>>,
                   LeftJoin<CR.Contact, On<CR.Contact.bAccountID, Equal<Customer.bAccountID>,
                   And<CR.Contact.contactID, Equal<Customer.defBillContactID>>>>>> Cards;
        public PXAction<ARExpiredCardFilter> viewCustomer;
        public PXAction<ARExpiredCardFilter> viewPaymentMethod;

        #endregion

		public ARExpiredCardsProcess()
		{
			this.Cards.Cache.AllowInsert = false;
			this.Cards.Cache.AllowUpdate = true;
			this.Cards.Cache.AllowDelete = false;

			this.Cards.SetSelected<CustomerPaymentMethod.selected>();
			this.Cards.SetProcessDelegate<CustomerPaymentMethodMassProcess>(SetCardInactive);			
			PXUIFieldAttribute.SetEnabled(this.Cards.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.selected>(this.Cards.Cache, null, true);
			this.Cards.SetProcessAllCaption(Messages.CCDeactivateAll);
			this.Cards.SetProcessCaption(Messages.CCDeactivate);

			PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Caches[typeof(Contact)], CR.Messages.Attention);
		}

        public virtual IEnumerable cards()
		{
			ARExpiredCardFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<CustomerPaymentMethod> select = new PXSelectJoin<CustomerPaymentMethod,
									InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>,
									And<Customer.status, NotEqual<BAccount.status.inactive>>>,
									LeftJoin<CR.Contact, On<CR.Contact.bAccountID, Equal<Customer.bAccountID>,
									   And<CR.Contact.contactID, Equal<Customer.defBillContactID>>>>>,
									Where<CustomerPaymentMethod.isActive, Equal<True>,
									And<CustomerPaymentMethod.expirationDate, LessEqual<Required<CustomerPaymentMethod.expirationDate>>,
									And<CustomerPaymentMethod.expirationDate, GreaterEqual<Required<CustomerPaymentMethod.expirationDate>>>>>>(this);
				
				if (!string.IsNullOrEmpty(filter.CustomerClassID))
				{
					select.WhereAnd<Where<Customer.customerClassID, Equal<Required<Customer.customerClassID>>>>();
				}
				if (filter.DefaultOnly == true)
				{
					select.WhereAnd<Where<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>();
				}

				foreach (PXResult<CustomerPaymentMethod, Customer, Contact> it in select.Select(filter.BeginDate, filter.EndDate, filter.CustomerClassID))
				{
					CustomerPaymentMethod cpm = (CustomerPaymentMethod)it;

					if (filter.NotificationSendOnly.GetValueOrDefault() == true && !cpm.LastNotificationDate.HasValue)
					{
						continue;
					}
					yield return it;
				}
			}
			yield break;
		}

		public virtual void ARExpiredCardFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
		}
		
		public static void SetCardInactive(CustomerPaymentMethodMassProcess aGraph, CustomerPaymentMethod aCard)
		{
			aGraph.SetActive(aCard, false);
		}

	
		public virtual void ARExpiredCardFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARExpiredCardFilter row = e.Row as ARExpiredCardFilter;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.selected>(this.Cards.Cache, null, true);
			if (row.TokenizedPMs == null)
			{
				string pmString = CCProcessingHelper.GetTokenizedPMsString(this);
				sender.SetValue<ARExpiredCardFilter.tokenizedPMs>(row, pmString);
			}
			else
			{
				if (row.TokenizedPMs != String.Empty)
				{
					Filter.Cache.RaiseExceptionHandling<ARExpiredCardFilter.customerClassID>(e.Row, ((ARExpiredCardFilter)e.Row).CustomerClassID,
						new PXSetPropertyException(Messages.NotAllCardsShown,
								PXErrorLevel.Warning, row.TokenizedPMs));
				}
			}
		}

		public virtual void CustomerPaymentMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.selected>(this.Cards.Cache, e.Row, true);
		}

		#region Sub-screen Navigation Button
		[PXUIField(
			DisplayName = Messages.ViewCustomer, 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewCustomer(PXAdapter adapter)
		{
			if (this.Cards.Current != null)
			{
				Customer customer = PXSelect<Customer,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, this.Cards.Current.BAccountID);
				CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
				graph.BAccount.Current = customer;
				throw new PXRedirectRequiredException(graph, Messages.ViewCustomer) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		[PXUIField(
			DisplayName = Messages.ViewPaymentMethod, 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton]
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

	public class CustomerPaymentMethodMassProcess: PXGraph<CustomerPaymentMethodMassProcess>
	{
		#region Selects
		[PXViewNameAttribute(Messages.CustomerPaymentMethodView)]
		public PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>> cards;
		[PXViewNameAttribute(Messages.CustomerView)]
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>> customer;
		[PXViewNameAttribute(Messages.BillingContactView)]
		public PXSelectJoin<Contact, InnerJoin<Customer, On<Customer.bAccountID,Equal<Contact.bAccountID>,
														And<Customer.defBillContactID,Equal<Contact.contactID>>>>,
				                Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>> billContact;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>>))]
		[CRDefaultMailTo(typeof(Select<Contact, Where<Contact.contactID, Equal<Current<CustomerPaymentMethod.billContactID>>>>))]
		public CRActivityList<CustomerPaymentMethod>
			Activity;


		#endregion		

        public virtual IEnumerable billcontact() 
        {
            CustomerPaymentMethod cpm =  this.cards.Current;
            if (cpm == null)
                yield break;            
            if (cpm.BillContactID.HasValue)
            {
                foreach (Contact it in PXSelect<Contact, Where<Contact.contactID, Equal<Current<CustomerPaymentMethod.billContactID>>>>.Select(this))
                {
                    yield return it;
                }
            }
            else
            {
                foreach (Contact it in PXSelectJoin<Contact, InnerJoin<Customer, On<Customer.bAccountID, Equal<Contact.bAccountID>,
                                                         And<Customer.defBillContactID, Equal<Contact.contactID>>>>,
                                                    Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>>.Select(this))
                {
                    yield return it;
                }
            }            
        }

		public virtual void SetActive(CustomerPaymentMethod aCard, bool on)
		{
			this.Clear();
			CustomerPaymentMethod card = this.cards.Select(aCard.PMInstanceID);
			if (card != null && card.IsActive != on) 
			{
				card.IsActive = on;
				card = this.cards.Update(card);
			}
			this.Actions.PressSave();
		}

		public virtual void MailExpirationNotification(CustomerPaymentMethod aCard )
		{
			this.Clear();
			CustomerPaymentMethod doc = this.cards.Select(aCard.PMInstanceID);
			this.cards.Current = doc;
			Customer owner = this.customer.Select();
			this.customer.Current = owner;

			var parameters = new Dictionary<string, string>();
			parameters[FldName_CardType] = doc.PaymentMethodID;
			parameters[FldName_CardNumber] = doc.Descr;
			parameters[FldName_ExpirationDate] = doc.ExpirationDate.Value.ToString();
			try
			{
				Activity.SendNotification(ARNotificationSource.Customer, notificationCD, null,  parameters);
			}
			catch (PXException e)
			{
				throw new PXException(Messages.CreditCardExpirationNotificationException, e.Message);
			}
			doc.LastNotificationDate = DateTime.Now;
			cards.Update(doc);

			this.Actions.PressSave();
		}

		#region Constans
		public string notificationCD = "CCEXPIRENOTE";
		protected const string FldName_CardType = "CardType";
		protected const string FldName_CardNumber = "CardNumber";
		protected const string FldName_ExpirationDate = "ExpirationDate";
		#endregion	
	}
}
