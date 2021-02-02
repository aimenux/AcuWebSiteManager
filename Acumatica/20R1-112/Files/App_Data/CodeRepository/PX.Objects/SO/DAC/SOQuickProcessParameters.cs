using System;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;

using OrderTypeRequiresShipping = PX.Data.Where<PX.Data.Current<PX.Objects.SO.SOOrderType.behavior>, PX.Data.Equal<PX.Objects.SO.SOOrderTypeConstants.salesOrder>>;
using OrderTypeRequiresInvoicing = PX.Data.Where<PX.Data.Current<PX.Objects.SO.SOOrderType.behavior>, PX.Data.In3<PX.Objects.SO.SOOrderTypeConstants.salesOrder, PX.Objects.SO.SOOrderTypeConstants.invoiceOrder, PX.Objects.SO.SOOrderTypeConstants.creditMemo>, PX.Data.And<PX.Data.Current<PX.Objects.SO.SOOrderType.aRDocType>, PX.Data.NotEqual<PX.Objects.AR.ARDocType.noUpdate>>>;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.SO
{	

	public static class Path
	{
		[PXLocalizable]
		public static class Messages
		{
			public const string OnCreateShipmentSuccess = "Shipment <*> is created";
			public const string OnCreateShipmentFailure = "Creating shipment";
			public const string OnPrintPickListSuccess = "<Pick List> is prepared";
			public const string OnPrintPickListFailure = "Preparing Pick List";
			public const string OnConfirmShipmentSuccess = "Shipment is confirmed";
			public const string OnConfirmShipmentFailure = "Confirming shipment";
			public const string OnPrintLabelsSuccess = "<Labels> are prepared";
			public const string OnPrintLabelsFailure = "Preparing Labels";
			public const string OnPrintShipmentSuccess = "<Shipment Confirmation> is prepared";
			public const string OnPrintShipmentFailure = "Preparing Shipment Confirmation";
			public const string OnUpdateINSuccess = "Inventory Document <*> is created";
			public const string OnUpdateINFailure = "Creating Inventory Document";
			public const string OnPrepareInvoiceSuccess = "Invoice <*> is created";
			public const string OnPrepareInvoiceFailure = "Creating Invoice";
			public const string OnPrintInvoiceSuccess = "<Invoice form> is prepared";
			public const string OnPrintInvoiceFailure = "Preparing Invoice form";
			public const string OnEmailInvoiceSuccess = "Invoice is emailed";
			public const string OnEmailInvoiceFailure = "Emailing Invoice";
            public const string OnEmailSalesOrderSuccess = "An email with the sales order has been sent.";
            public const string OnEmailSalesOrderFailure = "Sending the sales order by email.";
            public const string OnReleaseInvoiceSuccess = "Invoice is released";
			public const string OnReleaseInvoiceFailure = "Releasing Invoice";

		}

		public static class SO301000
		{
			public static readonly Type GroupGraph = typeof(SOOrderEntry);
			public static class SOOpen
			{
				public const String GroupStepID = "SO Open";
				public static class Action
				{
					public const String GroupActionID = nameof(Action);
					public class CreateShipment : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Create Shipment";
						public String OnSuccessMessage => Messages.OnCreateShipmentSuccess;
						public String OnFailureMessage => Messages.OnCreateShipmentFailure;
					}
                }

                public static class Notification
                {
                    public const String GroupActionID = nameof(Notification);

                    public class EmailSalesOrder : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Email Sales Order/Quote";
                        public String OnSuccessMessage => Messages.OnEmailSalesOrderSuccess;
                        public String OnFailureMessage => Messages.OnEmailSalesOrderFailure;
                    }
                }
            }
			public static class INOpen
			{
				public const String GroupStepID = "IN Open";
				public static class Action
				{
					private const String GroupActionID = nameof(Action);
					public class PrepareInvoice : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => nameof(PrepareInvoice);
						public String OnSuccessMessage => Messages.OnPrepareInvoiceSuccess;
						public String OnFailureMessage => Messages.OnPrepareInvoiceFailure;
					}
				}
			}
		}

		public class SO302000
		{
			public static readonly Type GroupGraph = typeof(SOShipmentEntry);
			public static class Open
			{
				public const String GroupStepID = nameof(Open);
				public static class Action
				{
					public const String GroupActionID = nameof(Action);
					public class ConfirmShipment : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Confirm Shipment";
						public String OnSuccessMessage => Messages.OnConfirmShipmentSuccess;
						public String OnFailureMessage => Messages.OnConfirmShipmentFailure;
					}
					public class PrintPickList : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Print Pick List";
						public String OnSuccessMessage => Messages.OnPrintPickListSuccess;
						public String OnFailureMessage => Messages.OnPrintPickListFailure;
					}
					public class PrintLabels : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Print Labels";
						public String OnSuccessMessage => Messages.OnPrintLabelsSuccess;
						public String OnFailureMessage => Messages.OnPrintLabelsFailure;
					}
				}
			}
			public static class Confirmed
			{
				public const String GroupStepID = "Confirmed";
				public static class Action
				{
					public const String GroupActionID = "Action";
					public class UpdateIN : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "UpdateIN";
						public String OnSuccessMessage => Messages.OnUpdateINSuccess;
						public String OnFailureMessage => Messages.OnUpdateINFailure;
					}
					public class PrepareInvoice : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Prepare Invoice";
						public String OnSuccessMessage => Messages.OnPrepareInvoiceSuccess;
						public String OnFailureMessage => Messages.OnPrepareInvoiceFailure;
					}
				}
				public static class Report
				{
					public const String GroupActionID = nameof(Report);
					public class PrintShipmentConfirmation : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Print Shipment Confirmation";
						public String OnSuccessMessage => Messages.OnPrintShipmentSuccess;
						public String OnFailureMessage => Messages.OnPrintShipmentFailure;
					}
				}
			}
		}

		public static class SO303000
		{
			public static readonly Type GroupGraph = typeof(SOInvoiceEntry);
			public static class Balanced
			{
				public const String GroupStepID = nameof(Balanced);
				public static class Action
				{
					public const String GroupActionID = nameof(Action);
					public class Release : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => nameof(Release);
						public String OnSuccessMessage => Messages.OnReleaseInvoiceSuccess;
						public String OnFailureMessage => Messages.OnReleaseInvoiceFailure;
					}
				}
				public static class Notification
				{
					public const String GroupActionID = nameof(Notification);
					public class EmailInvoice : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Email Invoice";
						public String OnSuccessMessage => Messages.OnEmailInvoiceSuccess;
						public String OnFailureMessage => Messages.OnEmailInvoiceFailure;
					}
				}
				public static class Report
				{
					public const String GroupActionID = nameof(Report);
					public class PrintInvoice : PXQuickProcess.Step.IDefinition
					{
						public Type Graph => GroupGraph;
						public String StepID => GroupStepID;
						public String ActionID => GroupActionID;
						public String MenuID => "Print Invoice";
						public String OnSuccessMessage => Messages.OnPrintInvoiceSuccess;
						public String OnFailureMessage => Messages.OnPrintInvoiceFailure;
					}
				}
			}
		}
	}

	[Serializable]
	public class SOQuickProcessParameters : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOQuickProcessParameters>.By<orderType>
		{
			public static SOQuickProcessParameters Find(PXGraph graph, string orderType) => FindBy(graph, orderType);
		}
		public static class FK
		{
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOQuickProcessParameters>.By<orderType> { }
		}
		#endregion
		#region OrderType
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrderType.orderType))]
		[PXParent(typeof(FK.OrderType))]
		public virtual String OrderType { get; set; }
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion
		#region CreateShipment
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO301000.SOOpen.Action.CreateShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
		[PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
		public virtual bool? CreateShipment { get; set; }
		public abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
		#region CreateShipment Parameters
		#region SiteID
		[PXInt]
		[PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
		[PXQuickProcess.Step.RelatedField(typeof(createShipment))]
		[OrderSiteSelector]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		[PXString]
		[PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteCD))]
		public virtual string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#endregion
		#endregion
		#region ConfirmShipment
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO302000.Open.Action.ConfirmShipment))]
		[PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
		public virtual bool? ConfirmShipment { get; set; }
		public abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
		#endregion
		#region UpdateIN
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO302000.Confirmed.Action.UpdateIN), DisplayName = "Update IN")]
		[PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
		public virtual bool? UpdateIN { get; set; }
		public abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
		#endregion
		#region PrepareInvoiceFromShipment
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO302000.Confirmed.Action.PrepareInvoice))]
		[PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
		public virtual bool? PrepareInvoiceFromShipment { get; set; }
		public abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
		#endregion
		#region PrepareInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO301000.INOpen.Action.PrepareInvoice), DisplayName = "Prepare Invoice")]
		[PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
		[PXQuickProcess.Step.IsStartPoint(typeof(Where<Current<SOOrderType.behavior>, In3<SOOrderTypeConstants.invoiceOrder, SOOrderTypeConstants.creditMemo>>))]
		public virtual bool? PrepareInvoice { get; set; }
		public abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
		#endregion
		#region EmailInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO303000.Balanced.Notification.EmailInvoice))]
		[PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresInvoicing>))]
		public virtual bool? EmailInvoice { get; set; }
		public abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
		#endregion
		#region ReleaseInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO303000.Balanced.Action.Release), DisplayName = "Release Invoice")]
		[PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresInvoicing>))]
		public virtual bool? ReleaseInvoice { get; set; }
		public abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }
		#endregion
		#region AutoRedirect
		[PXUIField(DisplayName = "Open All Created Documents in New Tabs")]
		[PXUIEnabled(typeof(Where<autoDownloadReports, Equal<False>>))]
		[PXQuickProcess.AutoRedirectOption]
		public virtual bool? AutoRedirect { get; set; }
		public abstract class autoRedirect : PX.Data.BQL.BqlBool.Field<autoRedirect> { }
		#endregion
		#region AutoDownloadReports
		[PXUIField(DisplayName = "Download All Created Print Forms")]
		[PXUIEnabled(typeof(Where<autoRedirect, Equal<True>>))]
		[PXQuickProcess.AutoDownloadReportsOption]
		public virtual bool? AutoDownloadReports { get; set; }
		public abstract class autoDownloadReports : PX.Data.BQL.BqlBool.Field<autoDownloadReports> { }
		#endregion
	}

	[Serializable]
	public sealed class SOQuickProcessParametersReportsExt : PXCacheExtension<SOQuickProcessParameters>
	{
		#region PrintPickList
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO302000.Open.Action.PrintPickList))]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.createShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.createShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresShipping))]
		public bool? PrintPickList { get; set; }
		public abstract class printPickList : PX.Data.BQL.BqlBool.Field<printPickList> { }
		#endregion
		#region PrintLabels
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO302000.Open.Action.PrintLabels))]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresShipping))]
		public bool? PrintLabels { get; set; }
		public abstract class printLabels : PX.Data.BQL.BqlBool.Field<printLabels> { }
		#endregion
		#region PrintConfirmation
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO302000.Confirmed.Report.PrintShipmentConfirmation))]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresShipping))]
		public bool? PrintConfirmation { get; set; }
		public abstract class printConfirmation : PX.Data.BQL.BqlBool.Field<printConfirmation> { }
		#endregion
		#region PrintInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(Path.SO303000.Balanced.Report.PrintInvoice))]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.prepareInvoice), typeof(SOQuickProcessParameters.prepareInvoiceFromShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.prepareInvoice))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresInvoicing))]
		public bool? PrintInvoice { get; set; }
		public abstract class printInvoice : PX.Data.BQL.BqlBool.Field<printInvoice> { }
		#endregion
	}

	public sealed class SOQuickProcessParametersAvailabilityExt : PXCacheExtension<SOQuickProcessParameters>
	{
		[PXLocalizable]
		public static class Msg
		{
			public const string CanShipAll = "All items available on the selected date.";
			public const string CanShipPartCancelRemainder = "Several items are not available for shipment on the selected date. The system will cancel items that are not currently available and complete the order.";
			public const string CanShipPartBackOrder = "Several items are not available for shipment on the selected date. The system will back order items that are not currently available and will not complete the order.";
			public const string NothingToShip = "There are no items to ship on the selected date in the warehouse. Try changing the date or the warehouse.";
			public const string NothingToShipAtAll = "There are no items to ship for this order.";
			public const string NoItemsAvailableToShip = "Several items are not available and partial shipment is not allowed for the order.";

			public class canShipAll : ConstantMessage<canShipAll> { public canShipAll() : base(CanShipAll) { } }
			public class canShipPartCancelRemainder : ConstantMessage<canShipPartCancelRemainder> { public canShipPartCancelRemainder() : base(CanShipPartCancelRemainder) { } }
			public class canShipPartBackOrder : ConstantMessage<canShipPartBackOrder> { public canShipPartBackOrder() : base(CanShipPartBackOrder) { } }
			public class nothingToShip : ConstantMessage<nothingToShip> { public nothingToShip() : base(NothingToShip) { } }
			public class nothingToShipAtAll : ConstantMessage<nothingToShipAtAll> { public nothingToShipAtAll() : base(NothingToShipAtAll) { } }
			public class noItemsAvailableToShip : ConstantMessage<noItemsAvailableToShip> { public noItemsAvailableToShip() : base(NoItemsAvailableToShip) { } }
		}

		#region AvailabilityStatus
		[PXString]
		public string AvailabilityStatus { get; set; }
		public abstract class availabilityStatus : PX.Data.BQL.BqlString.Field<availabilityStatus> { }
		#endregion
		#region SuccessIcon
		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		[PXUIVisible(typeof(Where<availabilityStatus, Equal<AvailabilityStatus.canShipAll>, And<skipByDateMsg, Equal<Empty>>>))]
		public bool? GreenStatus { get; set; }
		public abstract class greenStatus : PX.Data.BQL.BqlBool.Field<greenStatus> { }
		#endregion
		#region WarningIcon
		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		[PXUIVisible(typeof(Where<
			availabilityStatus, In3<AvailabilityStatus.canShipPartCancelRemainder, AvailabilityStatus.canShipPartBackOrder>, 
			Or<availabilityStatus, Equal<AvailabilityStatus.canShipAll>, And<skipByDateMsg, NotEqual<Empty>>>>))]
		public bool? YellowStatus { get; set; }
		public abstract class yellowStatus : PX.Data.BQL.BqlBool.Field<yellowStatus> { }
		#endregion
		#region ErrorIcon
		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		[PXUIVisible(typeof(Where<availabilityStatus, In3<AvailabilityStatus.nothingToShipAtAll, AvailabilityStatus.nothingToShip, AvailabilityStatus.noItemsAvailableToShip>>))]
		public bool? RedStatus { get; set; }
		public abstract class redStatus : PX.Data.BQL.BqlBool.Field<redStatus> { }
		#endregion
		#region Message
		[PXString(IsUnicode = true)]
		[PXUIField(IsReadOnly = true)]
		[PXUIVisible(typeof(Where<availabilityStatus, NotEqual<Empty>, And<availabilityStatus, IsNotNull>>))]
		[PXFormula(typeof(
			SmartJoin<
				Space,
				Switch<
					Case<Where<availabilityStatus, Equal<AvailabilityStatus.canShipAll>>, Msg.canShipAll,
					Case<Where<availabilityStatus, Equal<AvailabilityStatus.canShipPartCancelRemainder>>, Msg.canShipPartCancelRemainder,
					Case<Where<availabilityStatus, Equal<AvailabilityStatus.canShipPartBackOrder>>, Msg.canShipPartBackOrder,
					Case<Where<availabilityStatus, Equal<AvailabilityStatus.nothingToShip>>, Msg.nothingToShip,
					Case<Where<availabilityStatus, Equal<AvailabilityStatus.nothingToShipAtAll>>, Msg.nothingToShipAtAll,
					Case<Where<availabilityStatus, Equal<AvailabilityStatus.noItemsAvailableToShip>>, Msg.noItemsAvailableToShip
					>>>>>>, Empty>,
				skipByDateMsg>))]
		public string AvailabilityMessage { get; set; }
		public abstract class availabilityMessage : PX.Data.BQL.BqlString.Field<availabilityMessage> { }
		#endregion
		#region SkipByDateMsg
		[PXString, PXDefault(typeof(Empty), PersistingCheck = PXPersistingCheck.Nothing)]
		public string SkipByDateMsg { get; set; }
		public abstract class skipByDateMsg : PX.Data.BQL.BqlString.Field<skipByDateMsg> { }
		#endregion
	}

	public static class AvailabilityStatus
	{
		public const string CanShipAll = "AL";
		public const string CanShipPartCancelRemainder = "PC";
		public const string CanShipPartBackOrder = "PB";
		public const string NothingToShip = "NT";
		public const string NothingToShipAtAll = "NA";
		public const string NoItemsAvailableToShip = "NI";

		public class canShipAll : PX.Data.BQL.BqlString.Constant<canShipAll> { public canShipAll() :base(CanShipAll) { } }
		public class canShipPartCancelRemainder : PX.Data.BQL.BqlString.Constant<canShipPartCancelRemainder> { public canShipPartCancelRemainder() : base(CanShipPartCancelRemainder) { } }
		public class canShipPartBackOrder : PX.Data.BQL.BqlString.Constant<canShipPartBackOrder> { public canShipPartBackOrder() : base(CanShipPartBackOrder) { } }
		public class nothingToShip : PX.Data.BQL.BqlString.Constant<nothingToShip> { public nothingToShip() : base(NothingToShip) { } }
		public class nothingToShipAtAll : PX.Data.BQL.BqlString.Constant<nothingToShipAtAll> { public nothingToShipAtAll() : base(NothingToShipAtAll) { } }
		public class noItemsAvailableToShip : PX.Data.BQL.BqlString.Constant<noItemsAvailableToShip> { public noItemsAvailableToShip() : base(NoItemsAvailableToShip) { } }
	}

	public sealed class SOQuickProcessParametersShipDateExt : PXCacheExtension<SOQuickProcessParameters>
	{
		#region ShipDateMode
		[PXShort]
		[PXDefault(PX.Objects.SO.ShipDateMode.Today, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Shipment Date")]
		[PXQuickProcess.Step.RelatedField(typeof(SOQuickProcessParameters.createShipment))]
		public Int16? ShipDateMode { get; set; }
		public abstract class shipDateMode : PX.Data.BQL.BqlShort.Field<shipDateMode> { }
		#endregion
		#region ShipDate
		[PXDate]
		[PXUIField(DisplayName = "Custom Date")]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(IIf<Where<shipDateMode, Equal<ShipDateMode.tomorrow>>, tomorrow, today>))]
		[PXUIEnabled(typeof(Where<shipDateMode, Equal<ShipDateMode.custom>>))]
		[PXQuickProcess.Step.RelatedParameter(typeof(SOQuickProcessParameters.createShipment), nameof(shipDate))]
		public DateTime? ShipDate { get; set; }
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }

		#region Helpers
		[PXDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		public DateTime? Today { get; set; }
		public abstract class today : PX.Data.BQL.BqlDateTime.Field<today> { }
		[PXDate]
		public DateTime? Tomorrow { get { return Today.With(d => d.AddDays(1)); } set { } }
		public abstract class tomorrow : PX.Data.BQL.BqlDateTime.Field<tomorrow> { }

		public static void SetDate(PXCache cache, SOQuickProcessParameters row, DateTime date)
		{
			var ext = cache.GetExtension<SOQuickProcessParametersShipDateExt>(row);
			if (date == ext.Today)
				cache.SetValueExt<shipDateMode>(row, Objects.SO.ShipDateMode.Today);
			else if (date == ext.Tomorrow)
				cache.SetValueExt<shipDateMode>(row, Objects.SO.ShipDateMode.Tomorrow);
			else
			{
				cache.SetValueExt<shipDateMode>(row, Objects.SO.ShipDateMode.Custom);
				cache.SetValueExt<shipDate>(row, date);
			}
		}
		#endregion
		#endregion
	}

	public static class ShipDateMode
	{
		public const short Today = 0;
		public const short Tomorrow = 1;
		public const short Custom = 2;

		public class today : PX.Data.BQL.BqlShort.Constant<today> { public today() : base(Today) { } }
		public class tomorrow : PX.Data.BQL.BqlShort.Constant<tomorrow> { public tomorrow() : base(Tomorrow) { } }
		public class custom : PX.Data.BQL.BqlShort.Constant<custom> { public custom() : base(Custom) { } }
	}

	public sealed class SOQuickProcessParametersPrinterExt : PXCacheExtension<SOQuickProcessParameters>, PX.SM.IPrintable
	{
		#region HideWhenNothingToPrint
		[PXBool, PXDefault(true)]
		public bool? HideWhenNothingToPrint { get; set; } = true;
		public abstract class hideWhenNothingToPrint : PX.Data.BQL.BqlBool.Field<hideWhenNothingToPrint> { }
		#endregion
		#region PrintWithDeviceHub
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub", FieldClass = nameof(FeaturesSet.DeviceHub))]
		[ReportsPrintingSetting(nameof(PrintWithDeviceHub))]
		public bool? PrintWithDeviceHub { get; set; }
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		#endregion
		#region DefinePrinterManually
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Define Printers Manually", FieldClass = nameof(FeaturesSet.DeviceHub))]
		[PXUIEnabled(typeof(printWithDeviceHub))]
		[PXFormula(typeof(IIf<Where<printWithDeviceHub, Equal<False>>, False, definePrinterManually>))]
		[ReportsPrintingSetting(nameof(DefinePrinterManually))]
		public bool? DefinePrinterManually { get; set; } = false;
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		#endregion
		#region PrinterID
		[PX.SM.PXPrinterSelector]
		[PXUIEnabled(typeof(definePrinterManually))]
		[PXFormula(typeof(IIf<Where<printWithDeviceHub, Equal<False>, Or<definePrinterManually, Equal<False>>>, Null, printerID>))]
		[ReportsPrintingSetting(nameof(PrinterID))]
		public Guid? PrinterID { get; set; }
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		#endregion
		#region NumberOfCopies
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[ReportsPrintingSetting(nameof(NumberOfCopies))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public int? NumberOfCopies { get; set; }
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		#endregion

		public class ReportsPrintingSetting : PXAggregateAttribute
		{
			public ReportsPrintingSetting(string parameterName)
			{
				_Attributes.AddRange(
					new PXEventSubscriberAttribute[]
					{
						new PXUIVisibleAttribute(typeof(Where<hideWhenNothingToPrint, Equal<False>,
								Or<SOQuickProcessParametersReportsExt.printConfirmation, Equal<True>,
								Or<SOQuickProcessParametersReportsExt.printInvoice, Equal<True>,
								Or<SOQuickProcessParametersReportsExt.printLabels, Equal<True>,
								Or<SOQuickProcessParametersReportsExt.printPickList, Equal<True>
							>>>>>)),
						new PXQuickProcess.Step.RelatedParameterAttribute(typeof(SOQuickProcessParametersReportsExt.printConfirmation), parameterName) {SyncVisibilityWithRelatedStep = false},
						new PXQuickProcess.Step.RelatedParameterAttribute(typeof(SOQuickProcessParametersReportsExt.printInvoice), parameterName) {SyncVisibilityWithRelatedStep = false},
						new PXQuickProcess.Step.RelatedParameterAttribute(typeof(SOQuickProcessParametersReportsExt.printLabels), parameterName) {SyncVisibilityWithRelatedStep = false},
						new PXQuickProcess.Step.RelatedParameterAttribute(typeof(SOQuickProcessParametersReportsExt.printPickList), parameterName) {SyncVisibilityWithRelatedStep = false},
					});
			}
		}
	}
}