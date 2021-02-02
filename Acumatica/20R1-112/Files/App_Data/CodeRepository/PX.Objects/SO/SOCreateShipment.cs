using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.AR.MigrationMode;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class SOCreateShipment : PXGraph<SOCreateShipment>
	{
		public PXCancel<SOOrderFilter> Cancel;
		public PXAction<SOOrderFilter> viewDocument;

		public PXFilter<SOOrderFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SOOrder, SOOrderFilter> Orders;

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Orders.Current != null)
			{
				SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
				docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(Orders.Current.OrderNbr, Orders.Current.OrderType);
				throw new PXRedirectRequiredException(docgraph, true, "Order"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

        public PXSelect<INSite> INSites;
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = Messages.SiteDescr, Visibility = PXUIVisibility.SelectorVisible)]    
        protected virtual void INSite_Descr_CacheAttached(PXCache sender)
        {
        }

        public PXSelect<Carrier> Carriers;
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = Messages.CarrierDescr, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void Carrier_Description_CacheAttached(PXCache sender)
        {
        }


        [PXInt]
        public virtual void SOOrder_EmployeeID_CacheAttached(PXCache sender)
        { }

        [PXDecimal]
        public virtual void SOOrder_CuryUnpaidBalance_CacheAttached(PXCache sender)
        { }

        [PXDecimal]
        public virtual void SOOrder_CuryDocBal_CacheAttached(PXCache sender)
        { }

        public SOCreateShipment()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Orders.SetSelected<SOOrder.selected>();
		}
		
		public virtual void SOOrderFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrderFilter filter = e.Row as SOOrderFilter;
            if (filter == null) return;
            string actionID = (string)Orders.GetTargetFill(null, null, null, filter.Action, "@actionID");
            PXUIFieldAttribute.SetVisible<SOOrderFilter.shipmentDate>(sender, null, actionID == "1");
            PXUIFieldAttribute.SetVisible<SOOrderFilter.siteID>(sender, null, actionID == "1");
            if (!String.IsNullOrEmpty(filter.Action))
			{
				string siteCD = Filter.GetValueExt<SOOrderFilter.siteID>(filter) as string;
                Orders.SetProcessTarget(null, null, null, filter.Action, null, actionID == "1" ? filter.ShipmentDate : filter.EndDate, siteCD);
			}
        }

		protected bool _ActionChanged = false;

		public virtual void SOOrderFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOOrderFilter.action>(e.Row, e.OldRow);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName != "Orders" )
				return base.ExecuteUpdate(viewName, keys, values, parameters);

			Dictionary<string, object> strippedValues = new Dictionary<string, object>(keys.Count + 1); 
			foreach (var key in values.Keys)
			{
				if (keys.Contains(key))
				{
					strippedValues.Add(key.ToString(), values[key] );
				}
				else if (string.Equals(key.ToString(), Orders.Cache.GetField(typeof(SOOrder.selected)), StringComparison.InvariantCultureIgnoreCase))
				{
					strippedValues.Add(key.ToString(), values[key]);
				}
			}

			return base.ExecuteUpdate(viewName, keys, strippedValues, parameters);
		}

		public virtual IEnumerable orders()
		{
			PXUIFieldAttribute.SetDisplayName<SOOrder.customerID>(Caches[typeof(SOOrder)], Messages.CustomerID);

			SOOrderFilter filter = PXCache<SOOrderFilter>.CreateCopy(Filter.Current);
			if (filter.Action == "<SELECT>")
			{
				yield break;
			}

			string actionID = (string)Orders.GetTargetFill(null, null, null, filter.Action, "@actionID");

			if (_ActionChanged)
			{
				Orders.Cache.Clear();
			}

			PXSelectBase<SOOrder> cmd;

			const string ActionIdCreateShipment = "1";

			switch (actionID)
			{
				case ActionIdCreateShipment:
					cmd = BuildCommandCreateShipment(filter);
					break;
				case var action when !string.IsNullOrEmpty(filter.Action) && filter.Action.StartsWith("PrepareInvoice", StringComparison.OrdinalIgnoreCase):
					cmd = BuildCommandPrepareInvoice();
					break;
				default:
					cmd = BuildCommandDefault();
					break;
			}

			AddCommonFilters(filter, cmd);

			PXFilterRow[] newFilters = AlterFilters();

			int startRow = PXView.StartRow;
			int totalRows = 0;

			foreach (PXResult<SOOrder, SOOrderType> res in cmd.View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				SOOrder order = res;
				SOOrder cached;

				order.Behavior = ((SOOrderType)res).Behavior;
				order.ARDocType = ((SOOrderType)res).ARDocType;
				order.DefaultOperation = ((SOOrderType)res).DefaultOperation;

				if ((cached = (SOOrder)Orders.Cache.Locate(order)) != null)
				{
					order.Selected = cached.Selected;
				}

				yield return order;
			}

			PXView.StartRow = 0;

			Orders.Cache.IsDirty = false;
		}

		protected virtual PXSelectBase<SOOrder> BuildCommandCreateShipment(SOOrderFilter filter)
		{
			PXSelectBase<SOOrder> cmd = new PXSelectJoinGroupBy<SOOrder,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
				LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
				InnerJoin<SOShipmentPlan,
					On<SOOrder.orderType, Equal<SOShipmentPlan.orderType>,
						And<SOOrder.orderNbr, Equal<SOShipmentPlan.orderNbr>>>,
				InnerJoin<INSite, On<INSite.siteID, Equal<SOShipmentPlan.siteID>>,
				LeftJoin<SOOrderShipment,
					On<SOOrderShipment.orderType, Equal<SOShipmentPlan.orderType>,
						And<SOOrderShipment.orderNbr, Equal<SOShipmentPlan.orderNbr>,
						And<SOOrderShipment.siteID, Equal<SOShipmentPlan.siteID>,
						And<SOOrderShipment.confirmed, Equal<boolFalse>>>>>,
				LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>>>>>>,
				Where<SOShipmentPlan.inclQtySOBackOrdered, Equal<short0>, And<SOOrderShipment.shipmentNbr, IsNull,
					And2<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>>,
				Aggregate<
					GroupBy<SOOrder.orderType,
					GroupBy<SOOrder.orderNbr,
					GroupBy<SOOrder.approved>>>>>(this);

			if (filter.SiteID != null)
				cmd.WhereAnd<Where<SOShipmentPlan.siteID, Equal<Current<SOOrderFilter.siteID>>>>();

			if (filter.DateSel == "S")
			{
				if (filter.EndDate != null)
					cmd.WhereAnd<Where<SOShipmentPlan.planDate, LessEqual<Current<SOOrderFilter.endDate>>>>();

				if (filter.StartDate != null)
				{
					cmd.WhereAnd<Where<SOShipmentPlan.planDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
				}

				filter.DateSel = string.Empty;
			}

			return cmd;
		}

		protected virtual PXSelectBase<SOOrder> BuildCommandPrepareInvoice()
		{
			var cmd =
				new PXSelectJoinGroupBy<SOOrder,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>, And<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>>>,
						LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
						LeftJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOOrder.orderType>, And<SOOrderShipment.orderNbr, Equal<SOOrder.orderNbr>>>,
						LeftJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>, And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>>>,
						LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>>>>>,
					Where<SOOrder.hold, Equal<boolFalse>, And<SOOrder.cancelled, Equal<boolFalse>,
						And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>,
					Aggregate<
						GroupBy<SOOrder.orderType,
						GroupBy<SOOrder.orderNbr,
						GroupBy<SOOrder.approved>>>>>(this);

			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				cmd.WhereAnd<
					Where<Sub<Sub<Sub<SOOrder.shipmentCntr,
																	SOOrder.openShipmentCntr>,
																	SOOrder.billedCntr>,
																	SOOrder.releasedCntr>, Greater<short0>,
						Or2<Where<SOOrder.orderQty, Equal<decimal0>,
									And<SOOrder.curyUnbilledMiscTot, Greater<decimal0>>>,
						Or<Where<SOOrderType.requireShipping, Equal<boolFalse>, And<ARInvoice.refNbr, IsNull>>>>>>();
			}
			else
			{
				cmd.WhereAnd<
					Where<SOOrder.curyUnbilledMiscTot, Greater<decimal0>, And<SOOrderShipment.shipmentNbr, IsNull,
						Or<Where<SOOrderType.requireShipping, Equal<boolFalse>, And<ARInvoice.refNbr, IsNull>>>>>>();
			}

			return cmd;
		}

		protected virtual PXSelectBase<SOOrder> BuildCommandDefault()
		{
			return new PXSelectJoin<SOOrder, InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
				LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
				LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>>>,
				Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>(this);
		}

		protected virtual void AddCommonFilters(SOOrderFilter filter, PXSelectBase<SOOrder> cmd)
		{
			if (filter.EndDate != null)
			{
				switch (filter.DateSel)
				{
					case "S":
						cmd.WhereAnd<Where<SOOrder.shipDate, LessEqual<Current<SOOrderFilter.endDate>>>>();
						break;
					case "C":
						cmd.WhereAnd<Where<SOOrder.cancelDate, LessEqual<Current<SOOrderFilter.endDate>>>>();
						break;
					case "O":
						cmd.WhereAnd<Where<SOOrder.orderDate, LessEqual<Current<SOOrderFilter.endDate>>>>();
						break;
				}
			}

			if (filter.StartDate != null)
			{
				switch (filter.DateSel)
				{
					case "S":
						cmd.WhereAnd<Where<SOOrder.shipDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						break;
					case "C":
						cmd.WhereAnd<Where<SOOrder.cancelDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						break;
					case "O":
						cmd.WhereAnd<Where<SOOrder.orderDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						break;
				}
			}

			if (!string.IsNullOrEmpty(filter.CarrierPluginID))
			{
				cmd.WhereAnd<Where<Carrier.carrierPluginID, Equal<Current<SOOrderFilter.carrierPluginID>>>>();
			}

			if (!string.IsNullOrEmpty(filter.ShipVia))
			{
				cmd.WhereAnd<Where<SOOrder.shipVia, Equal<Current<SOOrderFilter.shipVia>>>>();
			}

			if (filter.CustomerID != null)
			{
				cmd.WhereAnd<Where<SOOrder.customerID, Equal<Current<SOOrderFilter.customerID>>>>();
			}
		}

		protected virtual PXFilterRow[] AlterFilters()
		{
			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach (PXFilterRow f in PXView.Filters)
			{
				if (string.Compare(f.DataField, "Behavior", StringComparison.OrdinalIgnoreCase) == 0)
				{
					f.DataField = "SOOrderType__Behavior";
				}
				newFilters.Add(f);
			}

			return newFilters.ToArray();
		}
	}

	[Serializable]
	public partial class SOOrderFilter : IBqlTable
	{
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		protected string _Action;
		[PXAutomationMenu]
		public virtual string Action
		{
			get
			{
				return this._Action;
			}
			set
			{
				this._Action = value;
			}
		}
		#endregion
		#region DateSel
		public abstract class dateSel : PX.Data.BQL.BqlString.Field<dateSel>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					new[]
				{
						Pair(ShipDate, Messages.ShipDate),
						Pair(CancelBy, Messages.CancelBy),
						Pair(OrderDate, Messages.OrderDate),
					}) {}
				}

			public const string ShipDate = "S";
			public const string CancelBy = "C";
			public const string OrderDate = "O";

			public class shipDate : PX.Data.BQL.BqlString.Constant<shipDate> { public shipDate() : base(ShipDate) { } }
			public class cancelBy : PX.Data.BQL.BqlString.Constant<cancelBy> { public cancelBy() : base(CancelBy) { } }
			public class orderDate : PX.Data.BQL.BqlString.Constant<orderDate> { public orderDate() : base(OrderDate) { } }
		}
		protected string _DateSel;
		[PXDBString]
		[PXDefault(dateSel.ShipDate)]
		[PXUIField(DisplayName = "Select By")]
		[dateSel.List]
		public virtual string DateSel
		{
			get
			{
				return this._DateSel;
			}
			set
			{
				this._DateSel = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[PXDefault()]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(DisplayName = "Warehouse", DescriptionField = typeof(INSite.descr))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion	
		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		protected String _CarrierPluginID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Carrier", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
		public virtual String CarrierPluginID
		{
			get
			{
				return this._CarrierPluginID;
			}
			set
			{
				this._CarrierPluginID = value;
			}
		}
		#endregion
		#region ShipVia
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected String _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected int? _CustomerID;
		[PXUIField(DisplayName = "Customer")]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get
			{
				return _CustomerID;
			}
			set
			{
				_CustomerID = value;
			}
		}
		#endregion
        #region ShipmentDate
        public abstract class shipmentDate : PX.Data.BQL.BqlDateTime.Field<shipmentDate> { }
        protected DateTime? _ShipmentDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Shipment Date", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? ShipmentDate
        {
            get
            {
                return this._ShipmentDate;
            }
            set
            {
                this._ShipmentDate = value;
            }
        }
        #endregion

    }
}
