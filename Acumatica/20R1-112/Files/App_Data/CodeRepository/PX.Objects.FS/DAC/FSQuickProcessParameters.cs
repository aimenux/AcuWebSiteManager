using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    using TrueCondition = Where<True, Equal<True>>;
    using ServiceOrderCanBeCompleted = Where<Current<FSServiceOrder.status>, Equal<FSServiceOrder.status.Open>>;
    using ServiceOrderCanBeClosed = Where<Current<FSServiceOrder.status>, Equal<FSServiceOrder.status.Completed>, Or<Current<FSServiceOrder.status>, Equal<FSServiceOrder.status.Open>>>;
    using OrderTypeRequiresShipping = Where<Current<SOOrderTypeQuickProcess.behavior>, Equal<SOOrderTypeConstants.salesOrder>>;
    using OrderTypeRequiresInvoicing = Where<Current<SOOrderTypeQuickProcess.behavior>, In3<SOOrderTypeConstants.salesOrder, SOOrderTypeConstants.invoiceOrder, SOOrderTypeConstants.creditMemo>, And<Current<SOOrderTypeQuickProcess.aRDocType>, NotEqual<AR.ARDocType.noUpdate>>>;
    using OrderTypePostToSOInvoice = Where<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Invoice>>;
    using AppointmentCanBeClosed = Where<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>>;
    using AppointmentCanBeInvoiced = Where<Current<FSAppointment.status>, Equal<FSAppointment.status.Closed>>;

    public static class Path
    {
        [PXLocalizable]
        public static class Messages
        {
            public const string SUCCESS = "Success";
            public const string SUCCESS_DOCUMENT = "Document <*> is created.";
            public const string FAILURE = "Failure";
        }

        // Dummy actions
        public static class FS000000
        {
            public static readonly Type GroupGraph = typeof(SvrOrdTypeMaint);

            public static class FSOpen
            {
                public const String GroupStepID = "FS Open";

                public static class Action
                {
                    public const String GroupActionID = nameof(Action);

                    public class GenerateInvoiceFromServiceOrder : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Generate Invoice";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class GenerateInvoiceFromAppointment : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Generate Invoice";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class AllowInvoiceServiceOrder : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Allow Billing";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class CloseAppointment : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Close Appointment";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class CloseServiceOrder : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Close Order";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class CompleteServiceOrder : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Complete Order";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class EmailInvoice : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Email Invoice";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class EmailSalesOrder : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Email Sales Order/Quote";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class EmailSignedAppointment : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Send Email with Signed Appointment";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class PayBill : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Pay Bill";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class PrepareInvoice : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Prepare Invoice";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class ReleaseBill : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Release Bill";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class ReleaseInvoice : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Release Invoice";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    public class SOQuickProcess : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Use Sales Order Quick Processing";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }

                    // Dummy step action
                    public class GenerateInvoice : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => GroupGraph;
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => "Generate Invoice";
                        public String OnSuccessMessage => "";
                        public String OnFailureMessage => "";
                    }
                }
            }
        }

        public static class FS300100
        {
            public static readonly Type GroupGraph = typeof(ServiceOrderEntry);

            public static class SrvOrdOpen
            {
                public const String GroupStepID = "SrvOrdOpen";

                public class AllowBilling : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(AllowBilling);
                    public String MenuID => "Allow Billing";
                    public String OnSuccessMessage => Messages.SUCCESS;
                    public String OnFailureMessage => Messages.FAILURE;
                }

                public class CompleteOrder : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(CompleteOrder);
                    public String MenuID => "Complete Order";
                    public String OnSuccessMessage => Messages.SUCCESS;
                    public String OnFailureMessage => Messages.FAILURE;
                }

                public class CloseOrder : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(CloseOrder);
                    public String MenuID => "Close Order";
                    public String OnSuccessMessage => Messages.SUCCESS;
                    public String OnFailureMessage => Messages.FAILURE;
                }

                public class InvoiceOrder : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(InvoiceOrder);
                    public String MenuID => "Run Service Order Billing";
                    public String OnSuccessMessage => Messages.SUCCESS_DOCUMENT;
                    public String OnFailureMessage => Messages.FAILURE;
                }
            }

            // Sales Order Prepare Invoice
            public static class INOpen
            {
                public const String GroupStepID = "IN Open";
                public static class Action
                {
                    private const String GroupActionID = nameof(Action);
                    public class PrepareInvoice : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => typeof(SOOrderEntry);
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => nameof(PrepareInvoice);
                        public String OnSuccessMessage => SO.Path.Messages.OnPrepareInvoiceSuccess;
                        public String OnFailureMessage => SO.Path.Messages.OnPrepareInvoiceFailure;
                    }
                }
            }

            public static class SOOpen
            {
                public const String GroupStepID = "SO Open";

                public class QuickProcess : PXQuickProcess.Step.IDefinition
                {
                    private const String GroupActionID = nameof(QuickProcess);
                    public Type Graph => typeof(SOOrderEntry);
                    public String StepID => GroupStepID;
                    public String ActionID => GroupActionID;
                    public String MenuID => "QuickProcess";
                    public String OnSuccessMessage => SO.Path.Messages.OnPrepareInvoiceSuccess;
                    public String OnFailureMessage => SO.Path.Messages.OnPrepareInvoiceFailure;
                }

                public class RunSOQuickProcessWithNoDialog : PXQuickProcess.Step.IDefinition
                {
                    private const String GroupActionID = nameof(RunSOQuickProcessWithNoDialog);
                    public Type Graph => typeof(SOOrderEntry);
                    public String StepID => GroupStepID;
                    public String ActionID => GroupActionID;
                    public String MenuID => "RunSOQuickProcessWithNoDialog";
                    public String OnSuccessMessage => SO.Path.Messages.OnPrepareInvoiceSuccess;
                    public String OnFailureMessage => SO.Path.Messages.OnPrepareInvoiceFailure;
                }
            }
        }

        public static class FS300200
        {
            public static readonly Type GroupGraph = typeof(AppointmentEntry);

            public static class AppointmentOpen
            {
                public const String GroupStepID = "AppointmentOpen";

                public class CloseAppointment : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(CloseAppointment);
                    public String MenuID => "Close Appointment";
                    public String OnSuccessMessage => Messages.SUCCESS;
                    public String OnFailureMessage => Messages.FAILURE;
                }

                public class EmailSignedAppointment : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(EmailSignedAppointment);
                    public String MenuID => "Send Email with Signed Appointment";
                    public String OnSuccessMessage => Messages.SUCCESS;
                    public String OnFailureMessage => Messages.FAILURE;
                }

                public class InvoiceAppointment : PXQuickProcess.Step.IDefinition
                {
                    public Type Graph => GroupGraph;
                    public String StepID => GroupStepID;
                    public String ActionID => nameof(InvoiceAppointment);
                    public String MenuID => "Run Appointment Billing";
                    public String OnSuccessMessage => Messages.SUCCESS_DOCUMENT;
                    public String OnFailureMessage => Messages.FAILURE;
                }
            }

            // Sales Order Prepare Invoice
            public static class INOpen
            {
                public const String GroupStepID = "IN Open";

                public static class Action
                {
                    private const String GroupActionID = nameof(Action);

                    public class PrepareInvoice : PXQuickProcess.Step.IDefinition
                    {
                        public Type Graph => typeof(SOOrderEntry);
                        public String StepID => GroupStepID;
                        public String ActionID => GroupActionID;
                        public String MenuID => nameof(PrepareInvoice);
                        public String OnSuccessMessage => SO.Path.Messages.OnPrepareInvoiceSuccess;
                        public String OnFailureMessage => SO.Path.Messages.OnPrepareInvoiceFailure;
                    }
                }
            }

            public static class SOOpen
            {
                public const String GroupStepID = "SO Open";

                public class QuickProcess : PXQuickProcess.Step.IDefinition
                {
                    private const String GroupActionID = nameof(QuickProcess);
                    public Type Graph => typeof(SOOrderEntry);
                    public String StepID => GroupStepID;
                    public String ActionID => GroupActionID;
                    public String MenuID => "QuickProcess";
                    public String OnSuccessMessage => SO.Path.Messages.OnPrepareInvoiceSuccess;
                    public String OnFailureMessage => SO.Path.Messages.OnPrepareInvoiceFailure;
                }

                public class RunSOQuickProcessWithNoDialog : PXQuickProcess.Step.IDefinition
                {
                    private const String GroupActionID = nameof(RunSOQuickProcessWithNoDialog);
                    public Type Graph => typeof(SOOrderEntry);
                    public String StepID => GroupStepID;
                    public String ActionID => GroupActionID;
                    public String MenuID => "RunSOQuickProcessWithNoDialog";
                    public String OnSuccessMessage => SO.Path.Messages.OnPrepareInvoiceSuccess;
                    public String OnFailureMessage => SO.Path.Messages.OnPrepareInvoiceFailure;
                }
            }
        }
    }

    [Serializable]

    public class FSQuickProcessParameters : PX.Data.IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(FSSrvOrdType.srvOrdType))]
        [PXParent(typeof(Select<FSSrvOrdType, Where<FSSrvOrdType.srvOrdType, Equal<Current<srvOrdType>>>>))]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region AllowInvoiceServiceOrder
        public abstract class allowInvoiceServiceOrder : PX.Data.BQL.BqlBool.Field<allowInvoiceServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.AllowInvoiceServiceOrder))]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        public virtual bool? AllowInvoiceServiceOrder { get; set; }
        #endregion
        #region CompleteServiceOrder
        public abstract class completeServiceOrder : PX.Data.BQL.BqlBool.Field<completeServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.CompleteServiceOrder))]
        public virtual bool? CompleteServiceOrder { get; set; }
        #endregion
        #region CloseAppointment
        public abstract class closeAppointment : PX.Data.BQL.BqlBool.Field<closeAppointment> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.CloseAppointment))]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        public virtual bool? CloseAppointment { get; set; }
        #endregion
        #region CloseServiceOrder
        public abstract class closeServiceOrder : PX.Data.BQL.BqlBool.Field<closeServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.CloseServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(completeServiceOrder))]
        public virtual bool? CloseServiceOrder { get; set; }
        #endregion
        #region EmailInvoice
        public abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.EmailInvoice))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice))]
        public virtual bool? EmailInvoice { get; set; }
        #endregion
        #region EmailSalesOrder
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.EmailSalesOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoice))]
        public virtual bool? EmailSalesOrder { get; set; }
        #endregion
        #region EmailSignedAppointment
        public abstract class emailSignedAppointment : PX.Data.BQL.BqlBool.Field<emailSignedAppointment> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.EmailSignedAppointment))]
        public virtual bool? EmailSignedAppointment { get; set; }
        #endregion
        #region GenerateInvoiceFromAppointment
        public abstract class generateInvoiceFromAppointment : PX.Data.BQL.BqlBool.Field<generateInvoiceFromAppointment> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.GenerateInvoiceFromAppointment))]
        [PXQuickProcess.Step.RequiresSteps(typeof(closeAppointment))]
        [PXUIField(DisplayName = "Run Appointment Billing")]
        public virtual bool? GenerateInvoiceFromAppointment { get; set; }
        #endregion
        #region GenerateInvoiceFromServiceOrder
        public abstract class generateInvoiceFromServiceOrder : PX.Data.BQL.BqlBool.Field<generateInvoiceFromServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.GenerateInvoiceFromServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(allowInvoiceServiceOrder))]
        [PXUIField(DisplayName = "Run Service Order Billing")]
        public virtual bool? GenerateInvoiceFromServiceOrder { get; set; }
        #endregion
        #region PayBill
        public abstract class payBill : PX.Data.BQL.BqlBool.Field<payBill> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.PayBill))]
        [PXQuickProcess.Step.RequiresSteps(typeof(releaseBill))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        public virtual bool? PayBill { get; set; }
        #endregion
        #region PrepareInvoice
        public abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.PrepareInvoice))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoice))]
        public virtual bool? PrepareInvoice { get; set; }
        #endregion
        #region ReleaseBill
        public abstract class releaseBill : PX.Data.BQL.BqlBool.Field<releaseBill> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.ReleaseBill))]
        public virtual bool? ReleaseBill { get; set; }
        #endregion
        #region ReleaseInvoice
        public abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.ReleaseInvoice))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice))]
        public virtual bool? ReleaseInvoice { get; set; }
        #endregion
        #region SOQuickProcess
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.SOQuickProcess))]
        public virtual bool? SOQuickProcess { get; set; }
        #endregion

        // Dummy step action
        #region GenerateInvoice
        public abstract class generateInvoice : PX.Data.BQL.BqlBool.Field<generateInvoice> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS000000.FSOpen.Action.GenerateInvoice), nonDatabaseField: true)]
        public virtual bool? GenerateInvoice
        {
            get
            {
                return this.GenerateInvoiceFromAppointment != null ? (this.GenerateInvoiceFromAppointment.Value || this.GenerateInvoiceFromServiceOrder.Value) : false;
            }
        }
        #endregion
    }

    [Serializable]
    public class FSSrvOrdQuickProcessParams : SOQuickProcessParameters
    {
        #region SrvOrdType

        [PXString(4, IsFixed = true)]
        [PXDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }

        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region AllowInvoiceServiceOrder
        public abstract class allowInvoiceServiceOrder : PX.Data.BQL.BqlBool.Field<allowInvoiceServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300100.SrvOrdOpen.AllowBilling), true)]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        public bool? AllowInvoiceServiceOrder { get; set; }
        #endregion
        #region CompleteServiceOrder
        public abstract class completeServiceOrder : PX.Data.BQL.BqlBool.Field<completeServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300100.SrvOrdOpen.CompleteOrder), true)]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeCompleted))]
        [PXQuickProcess.Step.IsInsertedJustBefore(typeof(generateInvoiceFromServiceOrder))]
        public bool? CompleteServiceOrder { get; set; }
        #endregion
        #region CloseServiceOrder
        public abstract class closeServiceOrder : PX.Data.BQL.BqlBool.Field<closeServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300100.SrvOrdOpen.CloseOrder), true)]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeClosed))]
        [PXQuickProcess.Step.IsInsertedJustBefore(typeof(generateInvoiceFromServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(completeServiceOrder))]
        public bool? CloseServiceOrder { get; set; }
        #endregion
        #region GenerateInvoiceFromServiceOrder
        public abstract class generateInvoiceFromServiceOrder : PX.Data.BQL.BqlBool.Field<generateInvoiceFromServiceOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300100.SrvOrdOpen.InvoiceOrder), true)]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(allowInvoiceServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(allowInvoiceServiceOrder))]
        [PXUIField(DisplayName = "Run Service Order Billing")]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public bool? GenerateInvoiceFromServiceOrder { get; set; }
        #endregion
        #region SOQuickProcess
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }
        
        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        public bool? SOQuickProcess { get; set; }
        #endregion
        #region EmailSalesOrder
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO301000.SOOpen.Notification.EmailSalesOrder), true)]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromServiceOrder))]
        public virtual bool? EmailSalesOrder { get; set; }
        #endregion

        #region SOQuickProcessParameters
        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(SOOrderTypeQuickProcess.orderType))]
        public override String OrderType { get; set; }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region CreateShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO301000.SOOpen.Action.CreateShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        [PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? CreateShipment { get; set; }
        public new abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
        #region CreateShipment Parameters
        #region SiteID
        [PXInt]
        [PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
        [PXQuickProcess.Step.RelatedField(typeof(createShipment))]
        [FSOrderSiteSelector]
        public override Int32? SiteID { get; set; }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [PXString]
        [PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteCD))]
        public override string SiteCD { get; set; }
        public new abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
        #endregion
        #endregion
        #endregion
        #region ConfirmShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO302000.Open.Action.ConfirmShipment))]
        [PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? ConfirmShipment { get; set; }
        public new abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
        #endregion
        #region UpdateIN
        public bool? _updateIN;
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO302000.Confirmed.Action.UpdateIN), DisplayName = "Update IN")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        
        public override bool? UpdateIN
        {
            get
            {
                return _updateIN;
            }
            set
            {
                _updateIN = value;
            }
        }
        public new abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
        #endregion
        #region PrepareInvoiceFromShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO302000.Confirmed.Action.PrepareInvoice))]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
        public override bool? PrepareInvoiceFromShipment { get; set; }
        public new abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
        #endregion
        #region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO301000.INOpen.Action.PrepareInvoice), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromServiceOrder))]
        public override bool? PrepareInvoice { get; set; }
        public new abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
        #endregion
        #region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO303000.Balanced.Notification.EmailInvoice))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, Or<OrderTypePostToSOInvoice>>))]
        public override bool? EmailInvoice { get; set; }
        public new abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
        #endregion
        #region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO303000.Balanced.Action.Release), DisplayName = "Release Invoice")]
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
        #endregion
    }

    [Serializable]
    public class FSAppQuickProcessParams : SOQuickProcessParameters
    {
        #region SrvOrdType

        [PXString(4, IsFixed = true)]
        [PXDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }

        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region CloseAppointment
        public abstract class closeAppointment : PX.Data.BQL.BqlBool.Field<closeAppointment> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300200.AppointmentOpen.CloseAppointment), true)]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        [PXQuickProcess.Step.IsApplicable(typeof(AppointmentCanBeClosed))]
        public virtual bool? CloseAppointment { get; set; }
        #endregion
        #region EmailSignedAppointment
        public abstract class emailSignedAppointment : PX.Data.BQL.BqlBool.Field<emailSignedAppointment> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300200.AppointmentOpen.EmailSignedAppointment), true)]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(generateInvoiceFromAppointment))]
        public virtual bool? EmailSignedAppointment { get; set; }
        #endregion
        #region GenerateInvoiceFromAppointment
        public abstract class generateInvoiceFromAppointment : PX.Data.BQL.BqlBool.Field<generateInvoiceFromAppointment> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(Path.FS300200.AppointmentOpen.InvoiceAppointment), true)]
        [PXQuickProcess.Step.IsStartPoint(typeof(AppointmentCanBeInvoiced))]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(closeAppointment))]
        [PXQuickProcess.Step.RequiresSteps(typeof(closeAppointment))]
        [PXUIField(DisplayName = "Run Appointment Billing")]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public virtual bool? GenerateInvoiceFromAppointment { get; set; }
        #endregion
        #region SOQuickProcess
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }

        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        public bool? SOQuickProcess { get; set; }
        #endregion
        #region EmailSalesOrder
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder> { }

        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO301000.SOOpen.Notification.EmailSalesOrder), true)]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromAppointment))]
        public virtual bool? EmailSalesOrder { get; set; }
        #endregion

        #region SOQuickProcessParameters
        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(SOOrderTypeQuickProcess.orderType))]
        public override String OrderType { get; set; }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region CreateShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO301000.SOOpen.Action.CreateShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        [PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? CreateShipment { get; set; }
        public new abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
        #region CreateShipment Parameters
        #region SiteID
        [PXInt]
        [PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
        [PXQuickProcess.Step.RelatedField(typeof(createShipment))]
        [FSOrderSiteSelector]
        public override Int32? SiteID { get; set; }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [PXString]
        [PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteCD))]
        public override string SiteCD { get; set; }
        public new abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
        #endregion
        #endregion
        #endregion
        #region ConfirmShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO302000.Open.Action.ConfirmShipment))]
        [PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? ConfirmShipment { get; set; }
        public new abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
        #endregion
        #region UpdateIN
        public bool? _updateIN;
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO302000.Confirmed.Action.UpdateIN), DisplayName = "Update IN")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]

        public override bool? UpdateIN
        {
            get
            {
                return _updateIN;
            }
            set
            {
                _updateIN = value;
            }
        }
        public new abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
        #endregion
        #region PrepareInvoiceFromShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO302000.Confirmed.Action.PrepareInvoice))]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
        public override bool? PrepareInvoiceFromShipment { get; set; }
        public new abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
        #endregion
        #region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO301000.INOpen.Action.PrepareInvoice), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromAppointment))]
        public override bool? PrepareInvoice { get; set; }
        public new abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
        #endregion
        #region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO303000.Balanced.Notification.EmailInvoice))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, Or<OrderTypePostToSOInvoice>>))]
        public override bool? EmailInvoice { get; set; }
        public new abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
        #endregion
        #region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SO.Path.SO303000.Balanced.Action.Release), DisplayName = "Release Invoice")]
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
        #endregion
    }

    public class FSOrderSiteSelectorAttribute : PXSelectorAttribute
    {
        protected string _InputMask = null;

        public FSOrderSiteSelectorAttribute()
            : base(typeof(Search<INSite.siteID,
                Where<Match<INSite, Current<AccessInfo.userName>>>>),
                typeof(INSite.siteCD), typeof(INSite.descr), typeof(INSite.replenishmentClassID)
            )
        {
            this.DirtyRead = true;
            this.SubstituteKey = typeof(INSite.siteCD);
            this.DescriptionField = typeof(INSite.descr);
            this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            PXDimensionAttribute attr = new PXDimensionAttribute(SiteAttribute.DimensionName);
            attr.CacheAttached(sender);
            attr.FieldName = _FieldName;
            PXFieldSelectingEventArgs e = new PXFieldSelectingEventArgs(null, null, true, false);
            attr.FieldSelecting(sender, e);

            _InputMask = ((PXSegmentedState)e.ReturnState).InputMask;
        }

        public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            base.SubstituteKeyFieldSelecting(sender, e);
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, null, null, null, _InputMask, null, null, null, null);
            }
        }
    }
}