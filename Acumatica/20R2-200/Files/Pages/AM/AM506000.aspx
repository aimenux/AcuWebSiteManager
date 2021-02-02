<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM506000.aspx.cs" Inherits="Page_AM506000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="CompletedOrders" TypeName="PX.Objects.AM.CloseOrderProcess">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="FinancialPeriod" LinkPage="" DefaultControlID="edFinPeriodID">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edFinancialPeriodID" runat="server" DataField="FinancialPeriodID" DataKeyNames="FinancialPeriodID" 
                DataMember="_FinPeriod_" DataSourceID="ds" InputMask="##-####" MaxLength="6" CommitChanges="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" 
        AllowSearch="true" DataSourceID="ds" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
		<Levels>
			<px:PXGridLevel  DataMember="CompletedOrders" DataKeyNames="ProdOrdID">
			    <RowTemplate>
			        <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
			        <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="True"/>
			        <px:PXSegmentMask ID="eInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
			        <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
			        <px:PXSelector ID="edSiteId" runat="server" DataField="SiteId" AllowEdit="True"/>
			        <px:PXNumberEdit ID="edQtytoProd" runat="server" DataField="QtytoProd"/>
			        <px:PXNumberEdit ID="edQtyComplete" runat="server" DataField="QtyComplete"/>
			        <px:PXNumberEdit ID="edWIPBalance" runat="server" DataField="WIPBalance"/>
                    <px:PXSegmentMask ID="edWIPVarianceAcctID" runat="server" DataField="WIPVarianceAcctID" />
                    <px:PXSegmentMask ID="edWIPVarianceSubID" runat="server" DataField="WIPVarianceSubID" DataKeyNames="Value" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" Width="30px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="130px" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
			        <px:PXGridColumn DataField="SubItemID" Width="81px" />
                    <px:PXGridColumn DataField="SiteId" Width="130px" />
                    <px:PXGridColumn DataField="QtytoProd" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="QtyComplete" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="WIPBalance" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="WIPVarianceAcctID" TextAlign="Right" Width="130px" />
                    <px:PXGridColumn DataField="WIPVarianceSubID" TextAlign="Right" Width="130px" />
                    <px:PXGridColumn DataField="ProjectID" Width="90px"/>
                    <px:PXGridColumn DataField="TaskID" Width="90px"/>
                    <px:PXGridColumn DataField="CostCodeID" Width="90px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
