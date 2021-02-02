using System;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;
using OrderActions = PX.Objects.SO.SOOrderEntryActionsAttribute;
using ShipmentActions = PX.Objects.SO.SOShipmentEntryActionsAttribute;

namespace PX.Objects.SO
{
	using OrderTypeRequiresShipping = Where<SOOrderType.behavior.FromCurrent.IsEqual<SOOrderTypeConstants.salesOrder>>;
	using OrderTypeRequiresInvoicing = Where<SOOrderType.behavior.FromCurrent.IsIn<SOOrderTypeConstants.salesOrder, SOOrderTypeConstants.invoiceOrder, SOOrderTypeConstants.creditMemo>.
		And<SOOrderType.aRDocType.FromCurrent.IsNotEqual<AR.ARDocType.noUpdate>>>;


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
		[PXQuickProcess.Step.IsBoundTo(typeof(createShipment.Step), DisplayName = OrderActions.DisplayNames.CreateShipment)]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
		[PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
		public virtual bool? CreateShipment { get; set; }
		public abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOOrderEntry>
			{
				public Step() : base(g => g.createShipmentIssue) { }
				public override String OnSuccessMessage => OnCreateShipmentSuccess;
				public override String OnFailureMessage => OnCreateShipmentFailure;

				public const string OnCreateShipmentSuccess = "Shipment <*> is created";
				public const string OnCreateShipmentFailure = "Creating shipment";
			}
		}
		#region CreateShipment Parameters
		#region SiteID
		[PXInt]
		[PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
		[PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteID))]
		[OrderSiteSelector]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#endregion
		#endregion
		#region ConfirmShipment
		[PXQuickProcess.Step.IsBoundTo(typeof(confirmShipment.Step), DisplayName = ShipmentActions.Messages.ConfirmShipment)]
		[PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
		public virtual bool? ConfirmShipment { get; set; }
		public abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOShipmentEntry>
			{
				public Step() : base(g => g.confirmShipmentAction) { }
				public override String OnSuccessMessage => OnConfirmShipmentSuccess;
				public override String OnFailureMessage => OnConfirmShipmentFailure;

				public const string OnConfirmShipmentSuccess = "Shipment is confirmed";
				public const string OnConfirmShipmentFailure = "Confirming shipment";
			}
		}
		#endregion
		#region UpdateIN
		[PXQuickProcess.Step.IsBoundTo(typeof(updateIN.Step), DisplayName = ShipmentActions.Messages.PostInvoiceToIN)]
		[PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
		public virtual bool? UpdateIN { get; set; }
		public abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOShipmentEntry>
			{
				public Step() : base(g => g.UpdateIN) { }
				public override String OnSuccessMessage => OnUpdateINSuccess;
				public override String OnFailureMessage => OnUpdateINFailure;

				public const string OnUpdateINSuccess = "Inventory Document <*> is created";
				public const string OnUpdateINFailure = "Creating Inventory Document";
			}
		}
		#endregion
		#region PrepareInvoiceFromShipment
		[PXQuickProcess.Step.IsBoundTo(typeof(prepareInvoiceFromShipment.Step), DisplayName = ShipmentActions.Messages.CreateInvoice)]
		[PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
		public virtual bool? PrepareInvoiceFromShipment { get; set; }
		public abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOShipmentEntry>
			{
				public Step() : base(g => g.createInvoice) { }
				public override String OnSuccessMessage => OnPrepareInvoiceSuccess;
				public override String OnFailureMessage => OnPrepareInvoiceFailure;

				public const string OnPrepareInvoiceSuccess = "Invoice <*> is created";
				public const string OnPrepareInvoiceFailure = "Creating Invoice";
			}
		}
		#endregion
		#region PrepareInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(prepareInvoice.Step), DisplayName = OrderActions.DisplayNames.PrepareInvoice)]
		[PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
		[PXQuickProcess.Step.IsStartPoint(typeof(Where<Current<SOOrderType.behavior>, In3<SOOrderTypeConstants.invoiceOrder, SOOrderTypeConstants.creditMemo>>))]
		public virtual bool? PrepareInvoice { get; set; }
		public abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOOrderEntry>
			{
				public Step() : base(g => g.prepareInvoice) { }
				public override String OnSuccessMessage => OnPrepareInvoiceSuccess;
				public override String OnFailureMessage => OnPrepareInvoiceFailure;

				public const string OnPrepareInvoiceSuccess = "Invoice <*> is created";
				public const string OnPrepareInvoiceFailure = "Creating Invoice";
			}
		}
		#endregion
		#region EmailInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(emailInvoice.Step), DisplayName = "Email Invoice")]
		[PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresInvoicing>))]
		public virtual bool? EmailInvoice { get; set; }
		public abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOInvoiceEntry>
			{
				public Step() : base(g => g.emailInvoice) { }
				public override String OnSuccessMessage => OnEmailInvoiceSuccess;
				public override String OnFailureMessage => OnEmailInvoiceFailure;

				public const string OnEmailInvoiceSuccess = "Invoice is emailed";
				public const string OnEmailInvoiceFailure = "Emailing Invoice";
			}
		}
		#endregion
		#region ReleaseInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(releaseInvoice.Step), DisplayName = "Release Invoice")]
		[PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresInvoicing>))]
		public virtual bool? ReleaseInvoice { get; set; }
		public abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOInvoiceEntry>
			{
				public Step() : base(g => g.release) { }
				public override String OnSuccessMessage => OnReleaseInvoiceSuccess;
				public override String OnFailureMessage => OnReleaseInvoiceFailure;

				public const string OnReleaseInvoiceSuccess = "Invoice is released";
				public const string OnReleaseInvoiceFailure = "Releasing Invoice";
			}
		}
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
		[PXQuickProcess.Step.IsBoundTo(typeof(printPickList.Step), DisplayName = ShipmentActions.Messages.PrintPickList)]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.createShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.createShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresShipping))]
		public bool? PrintPickList { get; set; }
		public abstract class printPickList : PX.Data.BQL.BqlBool.Field<printPickList>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOShipmentEntry>
			{
				public Step() : base(g => g.printPickListAction) { }
				public override String OnSuccessMessage => OnPrintPickListSuccess;
				public override String OnFailureMessage => OnPrintPickListFailure;

				public const string OnPrintPickListSuccess = "<Pick List> is prepared";
				public const string OnPrintPickListFailure = "Preparing Pick List";
			}
		}
		#endregion
		#region PrintLabels
		[PXQuickProcess.Step.IsBoundTo(typeof(printLabels.Step), DisplayName = ShipmentActions.Messages.PrintLabels)]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresShipping))]
		public bool? PrintLabels { get; set; }
		public abstract class printLabels : PX.Data.BQL.BqlBool.Field<printLabels>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOShipmentEntry>
			{
				public Step() : base(g => g.printLabels) { }
				public override String OnSuccessMessage => OnPrintLabelsSuccess;
				public override String OnFailureMessage => OnPrintLabelsFailure;

				public const string OnPrintLabelsSuccess = "<Labels> are prepared";
				public const string OnPrintLabelsFailure = "Preparing Labels";
			}
		}
		#endregion
		#region PrintConfirmation
		[PXQuickProcess.Step.IsBoundTo(typeof(printConfirmation.Step), DisplayName = ShipmentActions.Messages.PrintShipmentConfirmation)]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.confirmShipment))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresShipping))]
		public bool? PrintConfirmation { get; set; }
		public abstract class printConfirmation : PX.Data.BQL.BqlBool.Field<printConfirmation>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOShipmentEntry>
			{
				public Step() : base(g => g.printShipmentConfirmation) { }
				public override String OnSuccessMessage => OnPrintShipmentSuccess;
				public override String OnFailureMessage => OnPrintShipmentFailure;

				public const string OnPrintShipmentSuccess = "<Shipment Confirmation> is prepared";
				public const string OnPrintShipmentFailure = "Preparing Shipment Confirmation";
			}
		}
		#endregion
		#region PrintInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(printInvoice.Step), DisplayName = "Print Invoice")]
		[PXQuickProcess.Step.RequiresSteps(typeof(SOQuickProcessParameters.prepareInvoice), typeof(SOQuickProcessParameters.prepareInvoiceFromShipment))]
		[PXQuickProcess.Step.IsInsertedJustAfter(typeof(SOQuickProcessParameters.prepareInvoice))]
		[PXQuickProcess.Step.IsApplicable(typeof(OrderTypeRequiresInvoicing))]
		public bool? PrintInvoice { get; set; }
		public abstract class printInvoice : PX.Data.BQL.BqlBool.Field<printInvoice>
		{
			[PXLocalizable]
			public class Step : PXQuickProcess.Step.Definition<SOInvoiceEntry>
			{
				public Step() : base(g => g.printInvoice) { }
				public override String OnSuccessMessage => OnPrintInvoiceSuccess;
				public override String OnFailureMessage => OnPrintInvoiceFailure;

				public const string OnPrintInvoiceSuccess = "<Invoice form> is prepared";
				public const string OnPrintInvoiceFailure = "Preparing Invoice form";
			}
		}
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