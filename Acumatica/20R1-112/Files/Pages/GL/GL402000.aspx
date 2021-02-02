<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL402000.aspx.cs" Inherits="Page_GL402000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.AccountHistoryByYearEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="previousperiod" HideText="True"/>
			<px:PXDSCallbackCommand CommitChanges="True" Name="nextperiod" HideText="True"/>
			<px:PXDSCallbackCommand Name="AccountDetails" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="AccountBySub" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  Width="100%" DataMember="Filter"
		Caption="Selection" DefaultControlID="edLedgerID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID"/>
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID" Autorefresh="true"/>
			<px:PXSelector CommitChanges="True" ID="edFinYear" runat="server" DataField="FinYear" Autorefresh="true"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowCuryDetail" runat="server" DataField="ShowCuryDetail" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseMasterCalendar" runat="server" DataField="UseMasterCalendar" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="153px"  Width="100%"
		Caption="Account Summary by Financial Periods" AllowSearch="True" AllowPaging="True"
		SkinID="PrimaryInquire" RestrictFields="True" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="EnqResult">
				<Columns>
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
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
