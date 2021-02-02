<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SP404600.aspx.cs" Inherits="Page_SP404600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.SP.SPStatementForCustomer" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="PrintReport" Visible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" CommitChanges="True" DisplayFormat="d" />
			<px:PXDateTimeEdit ID="edTillDate" runat="server" DataField="TillDate" CommitChanges="True" DisplayFormat="d" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" 
        AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" Height="150px" 
        Style="z-index: 100" Width="100%" SkinID="PrimaryInquire" RestrictFields="True" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<Columns>
					<px:PXGridColumn DataField="StatementDate" Width="100px" />
					<px:PXGridColumn DataField="StatementBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OverdueBalance" TextAlign="Right" Width="200px" />					
					<px:PXGridColumn DataField="BranchID" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Text="Print Statement" Key="cmdPrintReport" Visible="False">
				    <AutoCallBack Command="PrintReport" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>

