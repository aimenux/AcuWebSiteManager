<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR202800.aspx.cs" Inherits="Page_AR202800" Title="AR Statement Cycle" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ARStatementCycleRecord" TypeName="PX.Objects.AR.ARStatementMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="recreateLast" CommitChanges="true" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="ARStatementCycleRecord" Caption="Statement Cycle" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" DefaultControlID="edStatementCycleId"
		TabIndex="100" DataSourceID="ds">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" ColumnSpan="5" GroupCaption="General Settings" StartGroup="True" />
			<px:PXSelector ID="edStatementCycleId" runat="server" DataField="StatementCycleId" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXDropDown CommitChanges="True" ID="edPrepareOn" runat="server" DataField="PrepareOn" SelectedIndex="1" />
			<px:PXNumberEdit ID="edDay00" runat="server" DataField="Day00" CommitChanges="true" />
			<px:PXNumberEdit ID="edDay01" runat="server" DataField="Day01"  CommitChanges="true" />
            <px:PXDropDown ID="edDayOfWeek" runat="server" DataField="DayOfWeek" CommitChanges="true" />
			<px:PXDateTimeEdit ID="edLastStmtDate" runat="server" DataField="LastStmtDate" Enabled="False" />
            <px:PXCheckBox ID="chkRequirePaymentApplication" runat="server" DataField="RequirePaymentApplication" TabIndex="103" />
            <px:PXCheckBox ID="chkPrintEmptyStatements" runat="server" DataField="PrintEmptyStatements" TabIndex="104" />
			<px:PXLayoutRule runat="server" GroupCaption="Aging Settings" StartGroup="True" />
            <px:PXCheckBox ID="edUseFinPeriodForAging" CommitChanges="true" runat="server" DataField="UseFinPeriodForAging" TabIndex="105" />
			<px:PXPanel ID="PXPanel1" runat="server" Caption="Aging Settings" ContentLayout-InnerSpacing="False" RenderSimple="True" ContentLayout-OuterSpacing="Horizontal" ContentLayout-SpacingSize="Small">
				<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XXS" ColumnSpan="3" />
				<px:PXLabel ID="RowNumberHeader" runat="server" Text="Aging Period (Days)" />
				<px:PXLayoutRule runat="server" SuppressLabel="true" />
                <px:PXLabel ID="RowNumber0" runat="server">Current</px:PXLabel>
				<px:PXNumberEdit ID="edBucket01LowerInclusiveBound" runat="server" DataField="Bucket01LowerInclusiveBound" Size="XXS" />
				<px:PXNumberEdit ID="edBucket02LowerInclusiveBound" runat="server" DataField="Bucket02LowerInclusiveBound" Size="XXS" />
				<px:PXNumberEdit ID="edBucket03LowerInclusiveBound" runat="server" DataField="Bucket03LowerInclusiveBound" Size="XXS" />
				<px:PXLabel ID="RowNumber4" runat="server">Over</px:PXLabel>
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XXS" LabelsWidth="XXS" />
                <px:PXLabel ID="DashHeader2" runat="server"/>
                <px:PXLabel ID="Dash1" runat="server">–</px:PXLabel>
                <px:PXLabel ID="Dash2" runat="server">–</px:PXLabel>
                <px:PXLabel ID="Dash3" runat="server">–</px:PXLabel>
                <px:PXLabel ID="DashFooter" runat="server"/>
				<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XXS" />
                <px:PXLabel ID="DaysCurrent" runat="server" Text="" />
				<px:PXNumberEdit ID="edAgeDays00" runat="server" DataField="AgeDays00" CommitChanges="true" TabIndex="107" Size="XXS"/>
				<px:PXNumberEdit ID="edAgeDays01" runat="server" DataField="AgeDays01" CommitChanges="true" TabIndex="109" Size="XXS"/>
				<px:PXNumberEdit ID="edAgeDays02" runat="server" DataField="AgeDays02" CommitChanges="true" TabIndex="111" Size="XXS"/>
				<px:PXNumberEdit ID="edBucket04LowerExclusiveBound" runat="server" DataField="Bucket04LowerExclusiveBound" Size="XXS"/>
				<px:PXLayoutRule runat="server" ControlSize="XM" StartColumn="True" SuppressLabel="True" />
				<px:PXLabel ID="MessageHeader" runat="server" Text="Description" />
				<px:PXTextEdit ID="edAgeMsgCurrent" runat="server" DataField="AgeMsgCurrent" SuppressLabel="True" TabIndex="106"/>
				<px:PXTextEdit ID="edAgeMsg00" runat="server" DataField="AgeMsg00" SuppressLabel="True" TabIndex="108"/>
				<px:PXTextEdit ID="edAgeMsg01" runat="server" DataField="AgeMsg01" SuppressLabel="True" TabIndex="110"/>
				<px:PXTextEdit ID="edAgeMsg02" runat="server" DataField="AgeMsg02" SuppressLabel="True" TabIndex="112"/>
				<px:PXTextEdit ID="edAgeMsg03" runat="server" DataField="AgeMsg03" SuppressLabel="True" TabIndex="113"/>
			</px:PXPanel>
            <px:PXDropDown ID="edAgeBasedOn" runat="server" DataField="AgeBasedOn" LabelWidth="137px" Width="250px" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Finance Charge Settings" ColumnSpan="5" ControlSize="XL" />
			<px:PXCheckBox CommitChanges="True" ID="chkFinChargeApply" runat="server" DataField="FinChargeApply" />
			<px:PXCheckBox ID="chkRequireFinChargeProcessing" runat="server" DataField="RequireFinChargeProcessing" />
			<px:PXSelector ID="edFinChargeID" runat="server" DataField="FinChargeID" DataSourceID="ds" />
		</Template>
		<AutoSize Enabled="True" Container="Window" MinHeight="300" MinWidth="400" />
	</px:PXFormView>
</asp:Content>
