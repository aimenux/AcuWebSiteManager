<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" 
    CodeFile="AM401500.aspx.cs" Inherits="Page_AM401500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.ProductionAttributesInq" PrimaryView="Filter" 
        BorderStyle="NotSet" >
		<CallbackCommands>
                
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" DefaultControlID="edProdOrdID">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
            <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AutoRefresh="True" DataSourceID="ds" CommitChanges="True" AllowEdit="True">
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXCheckBox ID="edShowTransactionAttributes" runat="server" CommitChanges="True" DataField="ShowTransactionAttributes" />
            <px:PXCheckBox ID="edShowOrderAttributes" runat="server"  CommitChanges="True" DataField="ShowOrderAttributes" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
                        DataSourceID="ds" SkinID="Inquire" Width="100%" TabIndex="2100" SyncPosition="True">
	    <ActionBar ActionsText="True">
        </ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="ProductionAttributes" DataKeyNames="LineNbr" >
			    <RowTemplate>
			        <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
			        <px:PXNumberEdit ID="edTranLineNbr" runat="server" DataField="TranLineNbr" />
			        <px:PXTextEdit ID="edgOrderType" runat="server" DataField="OrderType" />
			        <px:PXTextEdit ID="edgProdOrdID" runat="server" DataField="ProdOrdID" />
                    <px:PXNumberEdit ID="edLevel" runat="server" DataField="Level" />
                    <px:PXTextEdit ID="edOperationID" runat="server" DataField="OperationID" />
                    <px:PXNumberEdit ID="edSource" runat="server" DataField="Source" />
                    <px:PXSelector ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True"/>
                    <px:PXTextEdit ID="edLabel" runat="server" DataField="Label" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                    <px:PXCheckBox ID="chkEnabled" runat="server" DataField="Enabled" />
                    <px:PXCheckBox ID="chkTransactionRequired" runat="server" DataField="TransactionRequired" />
                    <px:PXTextEdit ID="edValue" runat="server" DataField="Value" />
                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
                    <px:PXSelector ID="edBatNbr" runat="server" DataField="BatNbr"  AutoRefresh="True" AllowEdit="True" />
                    <px:PXTextEdit ID="edTranOperationID" runat="server" DataField="TranOperationID" />
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" DisplayFormat="d" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataKeyNames="InventoryCD" 
                        DataMember="_InventoryItem_AccessInfo.userName_" DataSourceID="ds" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXTextEdit ID="edInventoryItemDescr" runat="server" DataField="InventoryItemDescr" />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                </RowTemplate>
			    <Columns>
			        <px:PXGridColumn DataField="LineNbr" />
                    <px:PXGridColumn DataField="TranLineNbr" />
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" />
                    <px:PXGridColumn DataField="Level" Width="75px" TextAlign="Left" />
                    <px:PXGridColumn DataField="OperationID" Width="75px" />
                    <px:PXGridColumn DataField="Source" Width="75px" TextAlign="Left" />
                    <px:PXGridColumn DataField="AttributeID" Width="100px" />
                    <px:PXGridColumn DataField="Label" Width="100px" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                    <px:PXGridColumn DataField="Enabled" TextAlign="Center" Type="CheckBox" Width="70px" />
                    <px:PXGridColumn DataField="TransactionRequired" TextAlign="Center" Type="CheckBox" Width="70px" />
                    <px:PXGridColumn DataField="Value" Width="220px" MatrixMode="True" />
                    <px:PXGridColumn DataField="DocType" Width="80px" />
                    <px:PXGridColumn DataField="BatNbr" Width="100px" />
                    <px:PXGridColumn DataField="TranOperationID" Width="80px" />
                    <px:PXGridColumn DataField="Qty" Width="100px" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranDate" Width="100px" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px" />
                    <px:PXGridColumn DataField="InventoryItemDescr" Width="200px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
