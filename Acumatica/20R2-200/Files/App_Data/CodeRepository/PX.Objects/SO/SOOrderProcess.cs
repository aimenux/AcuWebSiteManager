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

		public virtual void _(Events.RowSelected<SOProcessFilter> e)
		{
			if (!string.IsNullOrEmpty(e.Row?.Action))
			{
				Records.SetProcessWorkflowAction(e.Row.Action, Filter.Cache.ToDictionary(e.Row));

				bool showPrintSettings = IsPrintingAllowed(e.Row);

				e.Cache.AdjustUI(e.Row)
					.For<SOProcessFilter.printWithDeviceHub>(ui => ui.Visible = showPrintSettings)
					.For<SOProcessFilter.definePrinterManually>(ui =>
					{
						ui.Visible = showPrintSettings;
						ui.Enabled = e.Row.PrintWithDeviceHub == true;
					})
					.SameFor<SOProcessFilter.numberOfCopies>()
					.For<SOProcessFilter.printerID>(ui =>
					{
						ui.Visible = showPrintSettings;
						ui.Enabled = e.Row.PrintWithDeviceHub == true && e.Row.DefinePrinterManually == true;
					});
			}
		}

		public virtual bool IsPrintingAllowed(SOProcessFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && filter.Action == WellKnownActions.SOOrderScreen.PrintSalesOrder;
		}
		
		[PXEditDetailButton, PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
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

		public virtual void _(Events.RowUpdated<SOProcessFilter> e)
		{
			if (!e.Cache.ObjectsEqual<SOProcessFilter.action, SOProcessFilter.definePrinterManually, SOProcessFilter.printWithDeviceHub>(e.Row, e.OldRow) &&
				PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				Filter.Current != null && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true)
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintSalesOrder, Accessinfo.BranchID);
			}
		}

		public virtual void _(Events.FieldVerifying<SOProcessFilter, SOProcessFilter.printerID> e)
		{
			if (e.Row != null && !IsPrintingAllowed(e.Row))
				e.NewValue = null;
		}

		public class WellKnownActions
		{
			public class SOOrderScreen
			{
				public const string ScreenID = "SO301000";

				public const string PrintSalesOrder
					= ScreenID + "$" + nameof(SOOrderEntry.printSalesOrder);

				public const string EmailSalesOrder
					= ScreenID + "$" + nameof(SOOrderEntry.emailSalesOrder);
			}
		}
	}

	[Serializable()]
	public partial class SOProcessFilter : IBqlTable, PX.SM.IPrintable
	{
		#region Action
		[PX.Data.Automation.PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action { get; set; }
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		#endregion
		#region CurrentOwnerID
		[PXDBInt]
		[CRCurrentOwnerID]
		public virtual int? CurrentOwnerID { get; set; }
		public abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }
		#endregion
		#region MyOwner
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner { get; set; }
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		#endregion
		#region OwnerID
		[PX.TM.SubordinateOwner(DisplayName = "Assigned To")]
		public virtual int? OwnerID
		{
			get => (MyOwner == true) ? CurrentOwnerID : _OwnerID;
			set => _OwnerID = value;
		}
		protected int? _OwnerID;
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		#endregion
		#region WorkGroupID
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get => (MyWorkGroup == true) ? null : _WorkGroupID;
			set => _WorkGroupID = value;
		}
		protected Int32? _WorkGroupID;
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		#endregion
		#region MyWorkGroup
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup { get; set; }
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		#endregion
		#region FilterSet
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? FilterSet => 
			this.OwnerID != null ||
			this.WorkGroupID != null ||
			this.MyWorkGroup == true;
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		#endregion
		#region StartDate
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion

		#region OrderType
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault("QT")]
		[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<True>>>))]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OrderType { get; set; }
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion
		#region CustomerID
		[Customer]
		public virtual Int32? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region Status
		[PXDBString(1, IsFixed = true)]
		[SOOrderStatus.List]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region SalesPersonID
		[SalesPerson]
		public virtual Int32? SalesPersonID { get; set; }
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		#endregion

		#region PrintWithDeviceHub
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub { get; set; }
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		#endregion
		#region DefinePrinterManually
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually { get; set; } = false;
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		#endregion
		#region PrinterID
		[PX.SM.PXPrinterSelector]
		[PXFormula(typeof(
			Null.When<
				printWithDeviceHub.IsNotEqual<True>.
				Or<definePrinterManually.IsNotEqual<True>>>.
			Else<printerID>))]
		public virtual Guid? PrinterID { get; set; }
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		#endregion
		#region NumberOfCopies
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<SOProcessFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies { get; set; }
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		#endregion
	}

	[TM.OwnedFilter.Projection(typeof(SOProcessFilter), typeof(workgroupID), typeof(ownerID))]
    [Serializable]
	public partial class SOOrderProcessSelected : SOOrder
	{
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
	}

	public class SOEmailProcessing : 
		PXFilteredProcessingJoin<SOOrderProcessSelected, SOProcessFilter,
		InnerJoinSingleTable<Customer, On<SOOrder.customerID.IsEqual<Customer.bAccountID>>>,
		Where<
			Match<Customer, AccessInfo.userName.FromCurrent>.
			And<SOOrderProcessSelected.orderDate.IsLessEqual<SOProcessFilter.endDate.FromCurrent>>.
			And<SOProcessFilter.startDate.FromCurrent.IsNull.
				Or<SOOrderProcessSelected.orderDate.IsGreaterEqual<SOProcessFilter.startDate.FromCurrent>>>.
			And<PX.SM.WhereWorkflowActionEnabled<SOOrderProcessSelected, SOProcessFilter.action>>>>
	{
		public SOEmailProcessing(PXGraph graph)
			: base(graph)
		{
			_OuterView.WhereAndCurrent<SOProcessFilter>(nameof(SOOrderProcessSelected.ownerID));
		}
		public SOEmailProcessing(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			_OuterView.WhereAndCurrent<SOProcessFilter>(nameof(SOOrderProcessSelected.ownerID));
		}
	}
}