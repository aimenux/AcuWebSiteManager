<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR505000.aspx.cs" Inherits="Page_AR505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARCreateWriteOff" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="showCustomer" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edWOType">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown CommitChanges="True" ID="edWOType" runat="server" DataField="WOType" SelectedIndex="-1" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edWODate" runat="server" DataField="WODate" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edWOFinPeriodID" runat="server" DataField="WOFinPeriodID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
			<px:PXSelector CommitChanges="True" ID="PXSelector1" runat="server" DataField="ReasonCode" AutoRefresh="True" />
			<px:PXNumberEdit CommitChanges="True" ID="edWOLimit" runat="server" DataField="WOLimit" />
			<px:PXNumberEdit ID="edSelTotal" runat="server" DataField="SelTotal" Enabled="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="Documents" AllowPaging="True" AdjustPageSize="Auto" SkinID="PrimaryInquire" TabIndex="3500"
		FastFilterFields="RefNbr, CustomerID, CustomerID_BAccountR_acctName" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="ARDocumentList">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="editDetail" />
					<px:PXGridColumn DataField="CustomerID" AllowUpdate="False" LinkCommand="editCustomer" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
					<px:PXGridColumn DataField="DocDate" />
					<px:PXGridColumn DataField="FinPeriodID" MaxLength="6" />
                    <px:PXGridColumn DataField="CuryID" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" AllowShowHide="Server" />
					<px:PXGridColumn AllowUpdate="False" DataField="DocBal" TextAlign="Right" />
					<px:PXGridColumn DataField="DocDesc" />
					<px:PXGridColumn DataField="ReasonCode" />
				</Columns>
				<Mode AllowAddNew="false" AllowDelete="false"></Mode>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
