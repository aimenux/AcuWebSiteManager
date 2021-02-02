<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR503500.aspx.cs" Inherits="Page_AR503500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARStatementPrint" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDropDown CommitChanges="True" ID="edAction" runat="server" DataField="Action" />
			<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStatementDate" runat="server" DataField="StatementDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXCheckBox CommitChanges="True" ID="chkCuryStatements" runat="server" DataField="CuryStatements" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowAll" runat="server" DataField="ShowAll" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterName" runat="server" DataField="PrinterName" />
            <px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="SM" ControlSize="XXL" />
            <px:PXTextEdit CommitChanges="True" ID="edStatementMessage" runat="server" DataField="StatementMessage" TextMode="MultiLine" Height="55px" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" Height="150px" Style="z-index: 100" Width="100%" 
        SkinID="PrimaryInquire" FastFilterFields="CustomerID, CustomerID_BAccountR_acctName" SyncPosition="true" >
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
					<px:PXGridColumn DataField="CustomerID" Width="100px" AllowUpdate="False" SortDirection="Ascending" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="250px" />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="StatementBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OverdueBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryID" Width="75px" />
					<px:PXGridColumn DataField="CuryStatementBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryOverdueBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="UseCurrency" TextAlign="Center" Type="CheckBox" Width="78px" />
					<px:PXGridColumn DataField="DontPrint" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="Printed" TextAlign="Center" Width="65px" Type="CheckBox" />
					<px:PXGridColumn DataField="DontEmail" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="Emailed" TextAlign="Center" Width="65px" Type="CheckBox" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
