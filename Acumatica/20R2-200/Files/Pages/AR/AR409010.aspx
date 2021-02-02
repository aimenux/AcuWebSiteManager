<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR409010.aspx.cs" 
	Inherits="Page_AR409010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="ReconciliationTools.ARGLDiscrepancyByAccountEnq" PrimaryView="Filter" 
		PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="False" Name="ViewDetails" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" 
		Caption="Selection" DefaultControlID="edAccountID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edPeriodFrom" runat="server" DataField="PeriodFrom" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edPeriodTo" runat="server" DataField="PeriodTo" AutoRefresh="true" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowOnlyWithDiscrepancy" runat="server" DataField="ShowOnlyWithDiscrepancy" 
				AlignLeft="True" TextAlign="Right" LabelWidth="m"/>

			<px:PXLayoutRule runat="server" StartColumn="True"/>
			<px:PXPanel runat="server" ID="pnlBalances" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
				<px:PXNumberEdit ID="edTotalGLAmount" runat="server" DataField="TotalGLAmount" Enabled="False" />
				<px:PXNumberEdit ID="edTotalXXAmount" runat="server" DataField="TotalXXAmount" Enabled="False" />
				<px:PXNumberEdit ID="edTotalDiscrepancy" runat="server" DataField="TotalDiscrepancy" Enabled="False" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" 
			AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" SkinID="PrimaryInquire" 
			TabIndex="9700" RestrictFields="True" SyncPosition="true" >
		<ActionBar DefaultAction="ViewDetails" />
		<Levels>
			<px:PXGridLevel DataMember="Rows">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AccountID" />
					<px:PXGridColumn DataField="SubID" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="GLTurnover" TextAlign="Right" />
					<px:PXGridColumn DataField="XXTurnover" TextAlign="Right" />
					<px:PXGridColumn DataField="NonXXTrans" TextAlign="Right" />
					<px:PXGridColumn DataField="Discrepancy" TextAlign="Right" LinkCommand="ViewDetails" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
