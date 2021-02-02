using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    using TrueCondition = Where<True, Equal<True>>;
    using ServiceOrderRequiresAllowBilling = Where<Current<FSServiceOrder.allowInvoice>, Equal<False>>;
    using ServiceOrderCanBeCompleted = Where<Current<FSServiceOrder.status>, Equal<FSServiceOrder.status.Open>>;
    using ServiceOrderCanBeClosed = Where<Current<FSServiceOrder.status>, Equal<FSServiceOrder.status.Completed>, Or<Current<FSServiceOrder.status>, Equal<FSServiceOrder.status.Open>>>;
    using OrderTypeRequiresShipping = Where<Current<SOOrderTypeQuickProcess.behavior>, Equal<SOOrderTypeConstants.salesOrder>>;
    using OrderTypeRequiresInvoicing = Where<Current<SOOrderTypeQuickProcess.behavior>, In3<SOOrderTypeConstants.salesOrder, SOOrderTypeConstants.invoiceOrder, SOOrderTypeConstants.creditMemo>, And<Current<SOOrderTypeQuickProcess.aRDocType>, NotEqual<AR.ARDocType.noUpdate>>>;
    using OrderTypePostToSOInvoice = Where<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Invoice>>;
    using AppointmentCanBeClosed = Where<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>>;
    using AppointmentCanBeInvoiced = Where<Current<FSAppointment.status>, Equal<FSAppointment.status.Closed>>;

    [PXLocalizable]
    public static class QPMessages
    {
        public const string SUCCESS = "Success";
        public const string SUCCESS_DOCUMENT = "Document <*> is created.";
        public const string FAILURE = "Failure";
        public const string OnEmailSalesOrderSuccess = "An email with the sales order has been sent.";
        public const string OnEmailSalesOrderFailure = "Sending the sales order by email.";
    }

    [Serializable]
    public class FSQuickProcessParameters : PX.Data.IBqlTable
	{
		#region SrvOrdType
		[PXDBString(4, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(FSSrvOrdType.srvOrdType))]
        [PXParent(typeof(Select<FSSrvOrdType, Where<FSSrvOrdType.srvOrdType, Equal<Current<srvOrdType>>>>))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region AllowInvoiceServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(allowInvoiceServiceOrder.Step))]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderRequiresAllowBilling))]
        [PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderRequiresAllowBilling))]
        public virtual bool? AllowInvoiceServiceOrder { get; set; }
        public abstract class allowInvoiceServiceOrder : PX.Data.BQL.BqlBool.Field<allowInvoiceServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Allow Billing";
			}
		}
        #endregion
        #region CompleteServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(completeServiceOrder.Step))]
		[PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeCompleted))]
		[PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderCanBeCompleted))]
		public virtual bool? CompleteServiceOrder { get; set; }
        public abstract class completeServiceOrder : PX.Data.BQL.BqlBool.Field<completeServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Complete Order";
			}
		}
        #endregion
        #region CloseAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(closeAppointment.Step))]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        public virtual bool? CloseAppointment { get; set; }
		public abstract class closeAppointment : PX.Data.BQL.BqlBool.Field<closeAppointment>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Close Appointment";
			}
		}
		#endregion
		#region CloseServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(closeServiceOrder.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(completeServiceOrder))]
        public virtual bool? CloseServiceOrder { get; set; }
		public abstract class closeServiceOrder : PX.Data.BQL.BqlBool.Field<closeServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Close Order";
			}
		}
		#endregion
		#region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(emailInvoice.Step))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice))]
        public virtual bool? EmailInvoice { get; set; }
		public abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Email Invoice";
			}
		}
		#endregion
		#region EmailSalesOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSalesOrder.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoice))]
        public virtual bool? EmailSalesOrder { get; set; }
		public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Email Sales Order/Quote";
			}
		}
		#endregion
		#region EmailSignedAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSignedAppointment.Step))]
        public virtual bool? EmailSignedAppointment { get; set; }
		public abstract class emailSignedAppointment : PX.Data.BQL.BqlBool.Field<emailSignedAppointment>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Send Email with Signed Appointment";
			}
		}
		#endregion
		#region GenerateInvoiceFromAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromAppointment.Step), DisplayName = "Run Appointment Billing")]
        [PXQuickProcess.Step.RequiresSteps(typeof(closeAppointment))]
        public virtual bool? GenerateInvoiceFromAppointment { get; set; }
		public abstract class generateInvoiceFromAppointment : PX.Data.BQL.BqlBool.Field<generateInvoiceFromAppointment>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Generate Invoice From Appointment";
			}
		}
		#endregion
		#region GenerateInvoiceFromServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromServiceOrder.Step), DisplayName = "Run Service Order Billing")]
        [PXQuickProcess.Step.RequiresSteps(typeof(allowInvoiceServiceOrder))]
        public virtual bool? GenerateInvoiceFromServiceOrder { get; set; }
		public abstract class generateInvoiceFromServiceOrder : PX.Data.BQL.BqlBool.Field<generateInvoiceFromServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Generate Invoice From Service Order";
			}
		}
		#endregion
		#region PayBill
        [PXQuickProcess.Step.IsBoundTo(typeof(payBill.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(releaseBill))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        public virtual bool? PayBill { get; set; }
		public abstract class payBill : PX.Data.BQL.BqlBool.Field<payBill>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Pay Bill";
			}
		}
		#endregion
		#region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(prepareInvoice.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoice))]
        public virtual bool? PrepareInvoice { get; set; }
		public abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Prepare Invoice";
			}
		}
		#endregion
		#region ReleaseBill
        [PXQuickProcess.Step.IsBoundTo(typeof(releaseBill.Step))]
        public virtual bool? ReleaseBill { get; set; }
		public abstract class releaseBill : PX.Data.BQL.BqlBool.Field<releaseBill>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Release Bill";
			}
		}
		#endregion
		#region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(releaseInvoice.Step))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice))]
        public virtual bool? ReleaseInvoice { get; set; }
		public abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Release Invoice";
			}
		}
		#endregion
		#region SOQuickProcess
        [PXQuickProcess.Step.IsBoundTo(typeof(sOQuickProcess.Step))]
        public virtual bool? SOQuickProcess { get; set; }
		public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Use Sales Order Quick Processing";
			}
		}
		#endregion

		// Dummy step action
		#region GenerateInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(generateInvoice.Step), nonDatabaseField: true)]
		public virtual bool? GenerateInvoice => GenerateInvoiceFromAppointment != null ? (GenerateInvoiceFromAppointment.Value || GenerateInvoiceFromServiceOrder.Value) : false;
		public abstract class generateInvoice : PX.Data.BQL.BqlBool.Field<generateInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Generate Invoice";
			}
		}
		#endregion
		public abstract class FSQuickProcessDummyStepDefinition : PXQuickProcess.Step.IDefinition
		{
			public Type Graph => typeof(SvrOrdTypeMaint);
			public abstract String ActionName { get; }
			public String OnSuccessMessage => "";
			public String OnFailureMessage => "";
		}
	}

    [Serializable]
    public class FSSrvOrdQuickProcessParams : SOQuickProcessParameters
    {
        #region SrvOrdType
        [PXString(4, IsFixed = true)]
        [PXUnboundDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region AllowInvoiceServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(allowInvoiceServiceOrder.Step), true, DisplayName = "Allow Billing")]
		[PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderRequiresAllowBilling))]
		[PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderRequiresAllowBilling))]
		public bool? AllowInvoiceServiceOrder { get; set; }
        public abstract class allowInvoiceServiceOrder : PX.Data.BQL.BqlBool.Field<allowInvoiceServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.allowBilling) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region CompleteServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(completeServiceOrder.Step), true, DisplayName = "Complete Order")]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeCompleted))]
        [PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderCanBeCompleted))]
        [PXQuickProcess.Step.IsInsertedJustBefore(typeof(generateInvoiceFromServiceOrder))]
        public bool? CompleteServiceOrder { get; set; }
        public abstract class completeServiceOrder : PX.Data.BQL.BqlBool.Field<completeServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.completeOrder) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region CloseServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(closeServiceOrder.Step), true, DisplayName = "Close Order")]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeClosed))]
        [PXQuickProcess.Step.IsInsertedJustBefore(typeof(generateInvoiceFromServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(completeServiceOrder))]
        public bool? CloseServiceOrder { get; set; }
        public abstract class closeServiceOrder : PX.Data.BQL.BqlBool.Field<closeServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.closeOrder) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region GenerateInvoiceFromServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromServiceOrder.Step), true, DisplayName = "Run Service Order Billing")]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(allowInvoiceServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(allowInvoiceServiceOrder))]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public bool? GenerateInvoiceFromServiceOrder { get; set; }
        public abstract class generateInvoiceFromServiceOrder : PX.Data.BQL.BqlBool.Field<generateInvoiceFromServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.invoiceOrder) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS_DOCUMENT;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region SOQuickProcess
        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        public bool? SOQuickProcess { get; set; }
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }
        #endregion
        #region EmailSalesOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSalesOrder.Step), true, DisplayName = "Email Sales Order/Quote")]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromServiceOrder))]
        public virtual bool? EmailSalesOrder { get; set; }
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<SOOrderEntry>
            {
                public Step() : base(g => g.emailSalesOrder) { }
                public override string OnSuccessMessage => QPMessages.OnEmailSalesOrderSuccess;
                public override string OnFailureMessage => QPMessages.OnEmailSalesOrderFailure;
            }
        }
        #endregion

        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(SOOrderTypeQuickProcess.orderType))]
        public override String OrderType { get; set; }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region CreateShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.createShipment.Step), DisplayName = "Create Shipment")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        [PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? CreateShipment { get; set; }
        public new abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
        #region CreateShipment Parameters
        #region SiteID
        [PXInt]
        [PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
        [PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteID))]
        [OrderSiteSelector]
        public override Int32? SiteID { get; set; }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        #endregion
        #endregion
        #endregion
        #region ConfirmShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.confirmShipment.Step), DisplayName = "Confirm Shipment")]
        [PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? ConfirmShipment { get; set; }
        public new abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
        #endregion
        #region UpdateIN
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.updateIN.Step), DisplayName = "Update IN")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? UpdateIN { get; set; }
        public new abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
        #endregion
        #region PrepareInvoiceFromShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoiceFromShipment.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
        public override bool? PrepareInvoiceFromShipment { get; set; }
        public new abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
        #endregion
        #region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoice.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromServiceOrder))]
        public override bool? PrepareInvoice { get; set; }
        public new abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
        #endregion
        #region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.emailInvoice.Step), DisplayName = "Email Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, Or<OrderTypePostToSOInvoice>>))]
        public override bool? EmailInvoice { get; set; }
        public new abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
        #endregion
        #region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.releaseInvoice.Step), DisplayName = "Release Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment), typeof(generateInvoiceFromServiceOrder))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<True>))]
        public override bool? ReleaseInvoice { get; set; }
        public new abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }
        #endregion
        #region AutoRedirect
        [PXUIField(DisplayName = "Open All Created Documents in New Tabs")]
        [PXUIEnabled(typeof(Where<autoDownloadReports, Equal<False>>))]
        [PXQuickProcess.AutoRedirectOption]
        public override bool? AutoRedirect { get; set; }
        public new abstract class autoRedirect : PX.Data.BQL.BqlBool.Field<autoRedirect> { }
        #endregion
        #region AutoDownloadReports
        [PXUIField(DisplayName = "Download All Created Print Forms")]
        [PXUIEnabled(typeof(Where<autoRedirect, Equal<True>>))]
        [PXQuickProcess.AutoDownloadReportsOption]
        public override bool? AutoDownloadReports { get; set; }
        public new abstract class autoDownloadReports : PX.Data.BQL.BqlBool.Field<autoDownloadReports> { }
        #endregion
    }

    [Serializable]
    public class FSAppQuickProcessParams : SOQuickProcessParameters
    {
        #region SrvOrdType
        [PXString(4, IsFixed = true)]
        [PXUnboundDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region CloseAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(closeAppointment.Step), true, DisplayName = "Close Appointment")]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        [PXQuickProcess.Step.IsApplicable(typeof(AppointmentCanBeClosed))]
        public virtual bool? CloseAppointment { get; set; }
        public abstract class closeAppointment : PX.Data.BQL.BqlBool.Field<closeAppointment>
        {
            public class Step : PXQuickProcess.Step.Definition<AppointmentEntry>
            {
                public Step() : base(g => g.closeAppointment) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region EmailSignedAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSignedAppointment.Step), true, DisplayName = "Send Email with Signed Appointment")]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(generateInvoiceFromAppointment))]
        public virtual bool? EmailSignedAppointment { get; set; }
        public abstract class emailSignedAppointment : PX.Data.BQL.BqlBool.Field<emailSignedAppointment>
        {
            public class Step : PXQuickProcess.Step.Definition<AppointmentEntry>
            {
                public Step() : base(g => g.emailSignedAppointment) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region GenerateInvoiceFromAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromAppointment.Step), true, DisplayName = "Run Appointment Billing")]
        [PXQuickProcess.Step.IsStartPoint(typeof(AppointmentCanBeInvoiced))]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(closeAppointment))]
        [PXQuickProcess.Step.RequiresSteps(typeof(closeAppointment))]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public virtual bool? GenerateInvoiceFromAppointment { get; set; }
        public abstract class generateInvoiceFromAppointment : PX.Data.BQL.BqlBool.Field<generateInvoiceFromAppointment>
        {
            public class Step : PXQuickProcess.Step.Definition<AppointmentEntry>
            {
                public Step() : base(g => g.invoiceAppointment) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS_DOCUMENT;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region SOQuickProcess
        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        public bool? SOQuickProcess { get; set; }
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }
        #endregion
        #region EmailSalesOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSalesOrder.Step), true, DisplayName = "Email Sales Order/Quote")]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromAppointment))]
        public virtual bool? EmailSalesOrder { get; set; }
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<SOOrderEntry>
            {
                public Step() : base(g => g.emailSalesOrder) { }
                public override string OnSuccessMessage => QPMessages.OnEmailSalesOrderSuccess;
                public override string OnFailureMessage => QPMessages.OnEmailSalesOrderFailure;
            }
        }
        #endregion

        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(SOOrderTypeQuickProcess.orderType))]
        public override String OrderType { get; set; }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region CreateShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.createShipment.Step), DisplayName = "Create Shipment")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        [PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? CreateShipment { get; set; }
        public new abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
        #region CreateShipment Parameters
        #region SiteID
        [PXInt]
        [PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
        [PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteID))]
        [OrderSiteSelector]
        public override Int32? SiteID { get; set; }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        #endregion
        #endregion
        #endregion
        #region ConfirmShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.confirmShipment.Step), DisplayName = "Confirm Shipment")]
        [PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? ConfirmShipment { get; set; }
        public new abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
        #endregion
        #region UpdateIN
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.updateIN.Step), DisplayName = "Update IN")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? UpdateIN { get; set; }
        public new abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
        #endregion
        #region PrepareInvoiceFromShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoiceFromShipment.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
        public override bool? PrepareInvoiceFromShipment { get; set; }
        public new abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
        #endregion
        #region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoice.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromAppointment))]
        public override bool? PrepareInvoice { get; set; }
        public new abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
        #endregion
        #region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.emailInvoice.Step), DisplayName = "Email Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, Or<OrderTypePostToSOInvoice>>))]
        public override bool? EmailInvoice { get; set; }
        public new abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
        #endregion
        #region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.releaseInvoice.Step), DisplayName = "Release Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment), typeof(generateInvoiceFromAppointment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<True>))]
        public override bool? ReleaseInvoice { get; set; }
        public new abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }
        #endregion
        #region AutoRedirect
        [PXUIField(DisplayName = "Open All Created Documents in New Tabs")]
        [PXUIEnabled(typeof(Where<autoDownloadReports, Equal<False>>))]
        [PXQuickProcess.AutoRedirectOption]
        public override bool? AutoRedirect { get; set; }
        public new abstract class autoRedirect : PX.Data.BQL.BqlBool.Field<autoRedirect> { }
        #endregion
        #region AutoDownloadReports
        [PXUIField(DisplayName = "Download All Created Print Forms")]
        [PXUIEnabled(typeof(Where<autoRedirect, Equal<True>>))]
        [PXQuickProcess.AutoDownloadReportsOption]
        public override bool? AutoDownloadReports { get; set; }
        public new abstract class autoDownloadReports : PX.Data.BQL.BqlBool.Field<autoDownloadReports> { }
        #endregion
    }
}