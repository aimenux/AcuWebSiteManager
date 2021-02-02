<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM308000.aspx.cs" Inherits="Page_AM308000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="batch" 
        TypeName="PX.Objects.AM.WIPAdjustmentEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
      <px:PXDSCallbackCommand Name="Release" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="batch" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" DefaultControlID="edBatNbr">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edBatNbr" runat="server" DataField="BatNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate"/>
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID"/>
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
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" TabIndex="300" SyncPosition="True" >
		<Levels>
			<px:PXGridLevel DataMember="transactions" DataKeyNames="DocType,BatNbr,LineNbr">
                <RowTemplate>
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
                    <px:PXSelector ID="edProdOrdID" runat="server" AutoRefresh="True" DataField="ProdOrdID" AllowEdit="True" CommitChanges="True">
                        <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <AutoCallBack Command="Cancel" Target="ds" />
                    </px:PXSelector>
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" AutoRefresh="True" />
                    <px:PXNumberEdit ID="edTranAmt" runat="server" DataField="TranAmt"/>
                    <px:PXSelector ID="edReasonCodeID" runat="server" DataField="ReasonCodeID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edAcctID" runat="server" DataField="AcctID"/>
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr"/>
                    <px:PXSelector ID="edGLBatNbr" runat="server" DataField="GLBatNbr" Enabled="False" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edGLLineNbr" runat="server" DataField="GLLineNbr" Enabled="False"/>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXDropDown ID="edINDocType" runat="server" DataField="INDocType" />
                    <px:PXSelector ID="edINBatNbr" runat="server" DataField="INBatNbr" AllowEdit="True" />
                    <px:PXNumberEdit ID="edINLineNbr" runat="server" DataField="INLineNbr" />
                    <px:PXNumberEdit ID="edQtyScrapped" runat="server" DataField="QtyScrapped" />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" />
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                    <px:PXDropDown ID="edTranType" runat="server" DataField="TranType" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="OrderType" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProdOrdID" AutoCallBack="True" Width="130px" />
                    <px:PXGridColumn DataField="OperationID" Width="85px" />
                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right" Width="120px"/>
                    <px:PXGridColumn DataField="ReasonCodeID" Width="110px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="AcctID" AutoCallBack="True" Width="80px" />
                    <px:PXGridColumn DataField="SubID" Width="130px" />
                    <px:PXGridColumn DataField="TranDesc" Width="225px"/>
                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right"/>
                    <px:PXGridColumn DataField="GLBatNbr" Width="120px" />
                    <px:PXGridColumn DataField="GLLineNbr" TextAlign="Right"/>
                    <px:PXGridColumn DataField="InventoryID" Width="120px" />
                    <px:PXGridColumn DataField="SubItemID" Width="85px" />
                    <px:PXGridColumn DataField="INDocType" Width="99px" />
                    <px:PXGridColumn DataField="INBatNbr" Width="99px" />
                    <px:PXGridColumn DataField="INLineNbr" Width="99px" />
                    <px:PXGridColumn DataField="QtyScrapped" TextAlign="Right" Width="99px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="LocationID" Width="120px" />
                    <px:PXGridColumn DataField="TranType" Width="120px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowUpload="True" InitNewRow="True" />
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
