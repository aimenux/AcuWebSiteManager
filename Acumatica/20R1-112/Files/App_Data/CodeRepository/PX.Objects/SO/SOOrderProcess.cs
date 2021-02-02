using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.RQ;
using PX.Objects.CS;
using PX.Objects.AR.MigrationMode;

namespace PX.Objects.SO
{
	public class SOOrderProcess : PXGraph<SOOrderProcess>
	{
		public PXCancel<SOProcessFilter> Cancel;
		public PXAction<SOProcessFilter> viewDocument;
		public PXFilter<SOProcessFilter> Filter;
		[PXFilterable]
		public SOEmailProcessing Records;

		public SOOrderProcess()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);
		}

		public virtual void SOProcessFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOProcessFilter filter = e.Row as SOProcessFilter;
			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				Records.SetProcessTarget(null, null, null, filter.Action, parameters);

				PXUIFieldAttribute.SetVisible<SOProcessFilter.printerID>(sender, filter,
					PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
					(filter != null && !String.IsNullOrEmpty(filter.Action) && SOReports.GetReportIDByName(Records, filter.Action) == SOReports.PrintSalesOrder));
				bool showPrintSettings = IsPrintingAllowed(filter);

				PXUIFieldAttribute.SetVisible<SOProcessFilter.printWithDeviceHub>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<SOProcessFilter.definePrinterManually>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<SOProcessFilter.printerID>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<SOProcessFilter.numberOfCopies>(sender, filter, showPrintSettings);

				PXUIFieldAttribute.SetEnabled<SOProcessFilter.definePrinterManually>(sender, filter, filter.PrintWithDeviceHub == true);
				PXUIFieldAttribute.SetEnabled<SOProcessFilter.numberOfCopies>(sender, filter, filter.PrintWithDeviceHub == true);
				PXUIFieldAttribute.SetEnabled<SOProcessFilter.printerID>(sender, filter, filter.PrintWithDeviceHub == true && filter.DefinePrinterManually == true);

				if (filter.PrintWithDeviceHub != true || filter.DefinePrinterManually != true)
				{
					filter.PrinterID = null;
				}
			}
		}

		public virtual bool IsPrintingAllowed(SOProcessFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				(filter != null && !String.IsNullOrEmpty(filter.Action) && SOReports.GetReportIDByName(Records, filter.Action) == SOReports.PrintSalesOrder);
		}
		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Records.Current != null)
			{
				SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
				docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(Records.Current.OrderNbr, Records.Current.OrderType);
				throw new PXRedirectRequiredException(docgraph, true, "Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public virtual void SOProcessFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if ((!sender.ObjectsEqual<SOProcessFilter.action>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOProcessFilter.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOProcessFilter.printWithDeviceHub>(e.Row, e.OldRow))
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true)
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintSalesOrder, Accessinfo.BranchID);
			}
		}

		public virtual void SOProcessFilter_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOProcessFilter row = (SOProcessFilter)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}
	}

	[Serializable()]
	public partial class SOProcessFilter : IBqlTable, PX.SM.IPrintable
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
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

		[PXDBGuid]
		[CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Assigned To")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		[PXDefault(false)]
		[PXDBBool]
        public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
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
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
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

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<boolTrue>>>))]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault("QT")]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer()]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[SOOrderStatus.List()]		
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[SalesPerson()]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion

		#region PrintWithDeviceHub
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		protected bool? _PrintWithDeviceHub;
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub
		{
			get
			{
				return _PrintWithDeviceHub;
			}
			set
			{
				_PrintWithDeviceHub = value;
			}
		}
		#endregion
		#region DefinePrinterManually
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		protected bool? _DefinePrinterManually = false;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually
		{
			get
			{
				return _DefinePrinterManually;
			}
			set
			{
				_DefinePrinterManually = value;
			}
		}
		#endregion
		#region PrinterID
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		protected Guid? _PrinterID;
		[PX.SM.PXPrinterSelector]
		public virtual Guid? PrinterID
		{
			get
			{
				return this._PrinterID;
			}
			set
			{
				this._PrinterID = value;
			}
		}
		#endregion
		#region NumberOfCopies
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		protected int? _NumberOfCopies;
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<SOProcessFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies
		{
			get
			{
				return this._NumberOfCopies;
			}
			set
			{
				this._NumberOfCopies = value;
			}
		}
		#endregion
	}

	[TM.OwnedFilter.Projection(typeof(SOProcessFilter), typeof(workgroupID), typeof(ownerID))]
    [Serializable]
	public partial class SOOrderProcessSelected : SOOrder
	{
		
	}

	public class SOEmailProcessing : PXFilteredProcessingJoin<SOOrderProcessSelected, SOProcessFilter,
				InnerJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>,
				Where<Current<SOProcessFilter.action>, NotEqual<PXAutomationMenuAttribute.undefinded>,
				 And<SOOrderProcessSelected.orderDate, LessEqual<Current<SOProcessFilter.endDate>>,
				   And2<Match<Customer, Current<AccessInfo.userName>>,
				 And<Where<Current<SOProcessFilter.startDate>, IsNull,
						Or<SOOrderProcessSelected.orderDate, GreaterEqual<Current<SOProcessFilter.startDate>>>>>>>>>
	{
		public SOEmailProcessing(PXGraph graph)
			: base(graph)
		{
			_OuterView.WhereAndCurrent<SOProcessFilter>(typeof(SOOrderProcessSelected.ownerID).Name);
		}
		public SOEmailProcessing(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			_OuterView.WhereAndCurrent<SOProcessFilter>(typeof(SOOrderProcessSelected.ownerID).Name);
		}
	}
}
