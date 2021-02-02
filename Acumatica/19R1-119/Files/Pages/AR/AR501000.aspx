<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR501000.aspx.cs" Inherits="Page_AR501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="ARDocumentList" TypeName="PX.Objects.AR.ARDocumentRelease">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="viewDocument" Visible="false" />
            <px:PXDSCallbackCommand Visible="False" Name="ARDocumentList_refNbr_ViewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds" BatchUpdate="True" SkinID="PrimaryInquire" Caption="AR Documents"  
        FastFilterFields="RefNbr, CustomerID, CustomerID_BAccountR_acctName" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="ARDocumentList">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="RefNbr" Width="100px" />
					<px:PXGridColumn DataField="PaymentMethodID" Width="100px" />
					<px:PXGridColumn DataField="CustomerID" Width="100px" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="150px" />
					<px:PXGridColumn DataField="CustomerRefNbr" Width="100px" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="FinPeriodID" Width="70px" />
					<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryID" Width="80px" />
					<px:PXGridColumn DataField="DocDesc" Width="250px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<Layout ShowRowStatus="False" />
	</px:PXGrid>
</asp:Content>
