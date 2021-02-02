<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL101000.aspx.cs" Inherits="Page_GL101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:pxdatasource id="ds" runat="server" visible="true" typename="PX.Objects.GL.FiscalYearSetupMaint" primaryview="FiscalYearSetup">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" Visible="false" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="false" />
			<px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="false" />
			<px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="AutoFill" Visible="true" StartNewGroup="true" />
		</CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:pxformview id="form" runat="server" width="100%" datamember="FiscalYearSetup" caption="Financial Year Settings" defaultcontrolid="edFirstFinYear" tabindex="100">
		<Template>
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="S" />
			<px:PXTextEdit Size="s" ID="edFirstFinYear" runat="server" DataField="FirstFinYear" ReadOnly="True" />
			<px:PXDateTimeEdit OnValueChange="Commit" ID="edBegFinYear" runat="server" DataField="BegFinYear" />
			<px:PXCheckBox OnValueChange="Commit" ID="chkBelongsToNextYear" runat="server" DataField="BelongsToNextYear" />
			<px:PXDropDown OnValueChange="Commit" ID="edPeriodType" runat="server" DataField="PeriodType" />
			<px:PXDropDown OnValueChange="Commit" ID="edEndYearDayOfWeek" runat="server" DataField="EndYearDayOfWeek" />
            <px:PXDateTimeEdit OnValueChange="Commit" ID="edPeriodsStartDate" runat="server" DataField="PeriodsStartDate" />
			<px:PXCheckBox OnValueChange="Commit" ID="chAdjustToStart" runat="server" DataField="AdjustToPeriodStart" />
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='M' ControlSize="S" />
			<px:PXNumberEdit OnValueChange="Commit" ID="edFinPeriods" runat="server" DataField="FinPeriods" />
			<px:PXNumberEdit OnValueChange="Commit" ID="edPeriodLength" runat="server" DataField="PeriodLength" />
			<px:PXCheckBox OnValueChange="Commit" ID="chkUserDefined" runat="server" DataField="UserDefined" />
			<px:PXCheckBox OnValueChange="Commit" ID="chkHasAdjustmentPeriod" runat="server" DataField="HasAdjustmentPeriod" />
            <px:PXLayoutRule runat='server' LabelsWidth='M' ControlSize="L" />
            <px:PXDropDown OnValueChange="Commit" ID="edEndYearCalcMethod" runat="server" DataField="EndYearCalcMethod" />
            <px:PXDropDown OnValueChange="Commit" ID="edYearLastDayOfWeek" runat="server" DataField="YearLastDayOfWeek" Enabled="false" />
		</Template>
	</px:pxformview>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:pxgrid id="grid" runat="server" style="z-index: 100; height: 215px;" width="100%" caption="Financial Periods" skinid="Details" tabindex="200" height="215px">
		<Levels>
			<px:PXGridLevel DataMember="Periods">
				<Columns>
					<px:PXGridColumn DataField="PeriodNbr" />
					<px:PXGridColumn DataField="StartDateUI"  />
					<px:PXGridColumn DataField="EndDateUI" />
					<px:PXGridColumn DataField="Descr" />
				</Columns>
				<Mode AllowAddNew="True" AutoInsert="True" InitNewRow="True" />
				<RowTemplate>
					<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='M' ControlSize="S" />
					<px:PXTextEdit ID="edPeriodNbr" runat="server" DataField="PeriodNbr" />
					<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDateUI" DisplayFormat="d" />
					<px:PXDateTimeEdit ID="edEndDateUI" runat="server" DataField="EndDateUI" DisplayFormat="d" />
					<px:PXTextEdit ID="Descr" runat="server" DataField="Descr" Size="L" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Save PostData="Page" />
		</CallbackCommands>
	</px:pxgrid>
</asp:Content>
