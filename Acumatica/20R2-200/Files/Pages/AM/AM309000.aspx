<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM309000.aspx.cs" Inherits="Page_AM309000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="batch" 
        TypeName="PX.Objects.AM.ProductionCostEntry">
		<CallbackCommands>
		    <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:pxformview ID="form" runat="server" DataSourceID="ds" Width="100%" DataKeyNames="BatNbr" DataMember="batch" DefaultControlID="edBatNbr">
        <Activity HighlightColor="" SelectedColor="" Width="" Height="">
        </Activity>
        <Searches>
            <px:PXControlParam ControlID="form" Name="BatNbr" PropertyName="NewDataKey[&quot;BatNbr&quot;]"
                Type="String" />
        </Searches>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edBatNbr" runat="server" DataField="BatNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate"/>
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID"  />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edOrigBatNbr" runat="server" DataField="OrigBatNbr" Enabled="False" AllowEdit="True" />
            <px:PXDropDown ID="edOrigDocType" runat="server" DataField="OrigDocType" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXPanel ID="PXPanel1" runat="server" ContentLayout-Layout="Stack" RenderSimple="True" RenderStyle="Simple">
                <px:PXNumberEdit ID="edControlAmount" runat="server" DataField="ControlAmount"  />
                <px:PXNumberEdit ID="edTotalAmount" runat="server" DataField="TotalAmount" Enabled="False"  />
            </px:PXPanel>
        </Template>
	</px:pxformview>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" TabIndex="300" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="transactions" DataKeyNames="DocType,BatNbr,LineNbr">
                <RowTemplate>                    
                    <px:PXDropDown ID="edTranType" runat="server" DataField="TranType"/>
                    <px:PXNumberEdit ID="edTranAmt" runat="server" DataField="TranAmt"/>
                    <px:PXSegmentMask ID="edAcctID" runat="server" DataField="AcctID"/>
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                    <px:PXTextEdit ID="edTranDesc2" runat="server" DataField="TranDesc"/>
                    <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" />
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AutoRefresh="True" AllowEdit="True"/>
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID"/>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr"/>
                    <px:PXSelector ID="edGLBatNbr" runat="server" DataField="GLBatNbr" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edGLLineNbr" runat="server" DataField="GLLineNbr" />
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="TranType" Width="130" />
                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right" Width="108" />
                    <px:PXGridColumn DataField="AcctID" Width="130"/>
                    <px:PXGridColumn DataField="SubID" Width="130" />
                    <px:PXGridColumn DataField="TranDesc" Width="200" />
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="130" />
                    <px:PXGridColumn DataField="OperationID" Width="70" />                    
                    <px:PXGridColumn DataField="InventoryID" Width="130" />
                    <px:PXGridColumn DataField="LineNbr" Width="70"/>
                    <px:PXGridColumn DataField="GLBatNbr" Width="130" />
                    <px:PXGridColumn DataField="GLLineNbr" Width="130" />
                    <px:PXGridColumn DataField="Qty" Width="130px" />
                    <px:PXGridColumn DataField="ReferenceCostID" Width="130px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
