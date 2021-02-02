<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL401000.aspx.cs" Inherits="Page_GL401000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.AccountHistoryEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="PreviousPeriod" HideText="True" />
			<px:PXDSCallbackCommand Name="NextPeriod" HideText="True" />
			<px:PXDSCallbackCommand Name="ViewDetails" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand Name="AccountDetails" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="AccountBySub" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="AccountByPeriod" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Filter" Caption="Selection" AllowCollapse="false"
		DefaultControlID="edLedgerID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID"/>
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edAccountClassID" runat="server" DataField="AccountClassID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowCuryDetail" runat="server" DataField="ShowCuryDetail" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseMasterCalendar" runat="server" DataField="UseMasterCalendar" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" AllowSearch="True"
		BatchUpdate="True" AdjustPageSize="Auto" AllowPaging="True" Caption="Account Activity Summary"
		SkinID="PrimaryInquire" FastFilterFields="AccountID,Description" RestrictFields="True" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="EnqResult">
				<Columns>
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="AccountCD" LinkCommand="viewDetails" />
					<px:PXGridColumn DataField="Type" Type="DropDownList" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="LastActivityPeriod" />
					<px:PXGridColumn DataField="SignBegBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="PtdDebitTotal" TextAlign="Right" />
					<px:PXGridColumn DataField="PtdCreditTotal" TextAlign="Right" />
					<px:PXGridColumn DataField="SignEndBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryID" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryBegBalance" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdDebitTotal" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdCreditTotal" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryEndBalance" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="PtdSaldo" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdSaldo" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="ConsolAccountCD" />
					<px:PXGridColumn DataField="AccountClassID" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSelector ID="edAccountCD" runat="server" DataField="AccountCD" AutoGenerateColumns="True" />
					<px:PXDropDown ID="edType" runat="server" DataField="Type" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXLabel ID="lblSubIDH" runat="server"></px:PXLabel>
					<px:PXTextEdit ID="edLastActivityPeriod" runat="server" DataField="LastActivityPeriod" />
					<px:PXNumberEdit ID="edBegBalance" runat="server" DataField="BegBalance" />
					<px:PXNumberEdit ID="edPtdCreditTotal" runat="server" DataField="PtdCreditTotal" />
					<px:PXNumberEdit ID="edPtdDebitTotal" runat="server" DataField="PtdDebitTotal" />
					<px:PXNumberEdit ID="edEndBalance" runat="server" DataField="EndBalance" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar DefaultAction="viewDetails" />
	</px:PXGrid>
</asp:Content>
