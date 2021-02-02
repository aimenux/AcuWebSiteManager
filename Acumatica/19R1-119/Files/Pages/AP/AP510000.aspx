<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP510000.aspx.cs" Inherits="Page_AP510000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APRetainageRelease" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewDocument" DependOnGrid="grid" Visible="False"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edVendorID">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
		    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
		    <px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh ="True"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
			<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
			<px:PXSelector CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh ="True"/>
			<px:PXCheckBox CommitChanges="True" ID="chShowBillsWithOpenBalance" runat="server" DataField="ShowBillsWithOpenBalance" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="Documents" AllowPaging="True" AdjustPageSize="Auto" SkinID="PrimaryInquire" TabIndex="3500"
		SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="DocumentList">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="RefNbr" Width="100px"  LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="VendorID" Width="100px" />
					<px:PXGridColumn DataField="LineNbr" />
					<px:PXGridColumn DataField="RetainageReleasePct" Width="100px" CommitChanges="True" />
					<px:PXGridColumn DataField="CuryRetainageReleasedAmt" Width="100px" CommitChanges="True" />
					<px:PXGridColumn DataField="CuryRetainageUnreleasedCalcAmt" Width="100px" />
					<px:PXGridColumn DataField="RetainageVendorRef" Width="100px" />
					<px:PXGridColumn DataField="DocDate" Width="100px" />
					<px:PXGridColumn DataField="CuryOrigDocAmtWithRetainageTotal" Width="100px" />
					<px:PXGridColumn DataField="CuryID" TextAlign="Right" Width="80px" />
					<px:PXGridColumn DataField="DisplayProjectID" Width="100px" />
					<px:PXGridColumn DataField="DocDesc" Width="250px" />
					<px:PXGridColumn DataField="FinPeriodID" MaxLength="6" Width="81px" />
					<px:PXGridColumn DataField="InvoiceNbr" Width="90px" />
					<px:PXGridColumn DataField="APTranInventoryID"/>
					<px:PXGridColumn DataField="APTranTaskID" />
					<px:PXGridColumn DataField="APTranCostCodeID" />
					<px:PXGridColumn DataField="APTranAccountID" />
				</Columns>
				<Mode AllowAddNew="false" AllowDelete="false"></Mode>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
