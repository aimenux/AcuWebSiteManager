<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM100000.aspx.cs" Inherits="Page_FormView" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>


<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="setup" TypeName="PX.Objects.AM.MRPSetupMaint" >
		<CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" 
        DataMember="setup" DefaultControlID="edExceptionDaysBefore" TabIndex="1700" >
		<AutoSize Container="Window" Enabled="True" MinHeight="200"/>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXLayoutRule runat="server" GroupCaption="Exceptions" StartGroup="True"/>
            <px:PXNumberEdit ID="edExceptionDaysBefore" runat="server" DataField="ExceptionDaysBefore"/>
            <px:PXNumberEdit ID="edExceptionDaysAfter" runat="server" DataField="ExceptionDaysAfter"/>
            <px:PXLayoutRule runat="server" GroupCaption="Forecast" StartGroup="True"/>
            <px:PXNumberEdit ID="edForecastPlanHorizon" runat="server" DataField="ForecastPlanHorizon"/>
            <px:PXSelector ID="edForecastNumberingID" runat="server" DataField="ForecastNumberingID" AllowEdit="True"/>
            <px:PXLayoutRule runat="server" GroupCaption="MPS" StartGroup="True"/>
            <px:PXNumberEdit ID="edMPSFence" runat="server" DataField="MPSFence"/>
            <px:PXSelector ID="edDefaultMPSTypeID" runat="server" DataField="DefaultMPSTypeID" AllowEdit="True"/>

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXLayoutRule runat="server" GroupCaption="General" StartGroup="True"/>
            <px:PXSelector ID="edPlanOrderType" runat="server" DataField="PlanOrderType" AllowEdit="True"/>
            <px:PXNumberEdit ID="edGracePeriod" runat="server" DataField="GracePeriod" />
            <px:PXDropDown ID="edStockingMethod" runat="server" DataField="StockingMethod" />
            <px:PXSelector ID="edPurchaseCalendarID" runat="server" DataField="PurchaseCalendarID" AllowEdit="True"/>
            <px:PXCheckBox ID="edIncludeOnHoldSalesOrder" runat="server" DataField="IncludeOnHoldSalesOrder" />
            <px:PXCheckBox ID="edIncludeOnHoldPurchaseOrder" runat="server" DataField="IncludeOnHoldPurchaseOrder"/>
            <px:PXCheckBox ID="edIncludeOnHoldProductionOrder" runat="server" DataField="IncludeOnHoldProductionOrder"/>
            <px:PXCheckBox ID="edUseFixMfgLeadTime" runat="server" DataField="UseFixMfgLeadTime"/>
        </Template>
	</px:PXFormView>
</asp:Content>
