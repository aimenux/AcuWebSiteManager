<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO302000.aspx.cs"
    Inherits="Page_PO302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" TypeName="PX.Objects.PO.POReceiptEntry" PrimaryView="Document" BorderStyle="NotSet" Width="100%">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="CreateReturn" CommitChanges="True" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Action" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" StartNewGroup="true" Name="AddPOOrder" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" StartNewGroup="true" Name="AddTransfer" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOOrderLine" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOOrderLine2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptReturn" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptReturn2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptLineReturn" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptLineReturn2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptLine" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptLine2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewINDocument" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Assign" Visible="false" RepaintControls="All" />
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="ViewPOOrder" />
            <px:PXDSCallbackCommand Name="CreateAPDocument" CommitChanges="True" Visible="false" />            
            <px:PXDSCallbackCommand Name="CreateLCDocument" CommitChanges="True" Visible="false" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSPOReceiptLine_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSPOReceiptLine_binLotSerial" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grid" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="true" NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
        ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edReceiptType">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown  ID="edReceiptType" runat="server" SelectedIndex="-1" DataField="ReceiptType" />
            <px:PXSelector ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="true">
                <GridProperties FastFilterFields="InvoiceNbr, VendorID, VendorID_Vendor_acctName">
                </GridProperties>
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edReceiptDate" runat="server" DataField="ReceiptDate" />
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AllowAddNew="True" AllowEdit="True">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSegmentMask>
			<px:PXSelector ID="edHeadProjectID" runat="server" DataField="ProjectID" AllowEdit="True" CommitChanges="True" />
            <pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" DataSourceID="ds" RateTypeView="_POReceipt_CurrencyInfo_"
                DataMember="_Currency_" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkAutoCreateInvoice" runat="server" Checked="True" DataField="AutoCreateInvoice" />
            <px:PXCheckBox ID="chkReturnOrigCost" runat="server" DataField="ReturnOrigCost" />
            <px:PXTextEdit CommitChanges="True" ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
	        <px:PXSelector ID="trsWorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" />
	        <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" />
            <px:PXNumberEdit ID="edControlQty" runat="server" DataField="ControlQty" />
            <px:PXNumberEdit ID="edUnbilledQty" runat="server" DataField="UnbilledQty" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="353px" Style="z-index: 100;" Width="100%" DataMember="CurrentDocument" DataSourceID="ds">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 455px;" Width="100%"
                        ActionsPosition="Top" SkinID="DetailsInTab" StatusField="Availability" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="transactions">
                                <Columns>
                                    <px:PXGridColumn DataField="Availability" />
                                    <px:PXGridColumn DataField="Selected" Type="CheckBox" CommitChanges="true" AllowCheckAll="True" TextAlign="Center" />
                                    <px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" Label="Inventory ID" AllowDragDrop="true"/>
                                    <px:PXGridColumn DataField="LineType" Type="DropDownList" AutoCallBack="True" Label="Line Type" AllowDragDrop="true"/>
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" NullText="&lt;SPLIT&gt;" AutoCallBack="True" Label="Subitem ID" />
                                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" Label="Warehouse ID" />
                                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" NullText="&lt;SPLIT&gt;" Label="Location ID" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="TranDesc" Label="Transaction Descr." />
                                    <px:PXGridColumn DataField="UOM" AutoCallBack="True" DisplayFormat="&gt;aaaaaa" Label="Unit of Measure" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrigOrderQty" Label="Ordered Qty." TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OpenOrderQty" Label="Open Qty." TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ReceiptQty" TextAlign="Right" AutoCallBack="True" Label="Order Qty" AllowDragDrop="true"/>
                                    <px:PXGridColumn AllowNull="False" DataField="BaseReceiptQty" TextAlign="Right" Label="Base Order Qty" />
                                    <px:PXGridColumn DataField="OrigReceiptNbr" />
                                    <px:PXGridColumn DataField="OrigReceiptLineNbr" />
                                    <px:PXGridColumn DataField="ExpenseAcctID" DisplayFormat="&gt;######" AutoCallBack="True" Label="Account" />
                                    <px:PXGridColumn DataField="ExpenseAcctID_Account_description" Label="Account Description" />
                                    <px:PXGridColumn DataField="ExpenseSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Label="Sub" />
									<px:PXGridColumn DataField="POAccrualAcctID" DisplayFormat="&gt;######" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="POAccrualSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectID" Label="Project" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="TaskID" DisplayFormat="&gt;AAAAAAAAAA" Label="Task" />
                                    <px:PXGridColumn DataField="ExpireDate" Label="Expire Date" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" Label="Line Nbr" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Visible="False" />
                                    <px:PXGridColumn DataField="LotSerialNbr" NullText="&lt;SPLIT&gt;" Label="Lot/Serial Nbr" />
                                    <px:PXGridColumn DataField="POType" RenderEditorText="True" Label="Order Type" />
                                    <px:PXGridColumn DataField="PONbr" Label="Order Nbr" />
                                    <px:PXGridColumn DataField="POLineNbr" TextAlign="Right" Label="PO Line Nbr" />
                                    <px:PXGridColumn DataField="SOOrderType" Label="Transfer Type" />
                                    <px:PXGridColumn DataField="SOOrderNbr" Label="Transfer Nbr" />
                                    <px:PXGridColumn DataField="SOOrderLineNbr" TextAlign="Right" Label="Transfer Line Nbr" />
                                    <px:PXGridColumn DataField="SOShipmentNbr" />
                                    <px:PXGridColumn AllowNull="False" DataField="AllowComplete" TextAlign="Center" Type="CheckBox" AutoCallBack="True"
                                        Label="Allow Complete Line" />
                                    <px:PXGridColumn AllowNull="False" DataField="AllowOpen" TextAlign="Center" Type="CheckBox" AutoCallBack="True"
                                        Label="Allow Open Line" />
                                    <px:PXGridColumn AllowNull="False" DataField="ReturnedQty" TextAlign="Right" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="ReasonCode" DisplayFormat="&gt;aaaaaaaaaa" Label="Reason Code" />
                                    <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" Label="Unit Cost" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="TranCost" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="TranCostFinal" TextAlign="Right" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXDropDown CommitChanges="True" ID="edLineType" runat="server" AllowNull="False" DataField="LineType" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="POReceiptLine.lineType" PropertyName="DataValues[&quot;LineType&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="POReceiptLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="POReceiptLine.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="POReceiptLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="POReceiptLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="POReceiptLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edOrigOrderQty" runat="server" DataField="OrigOrderQty" Enabled="False" />
                                    <px:PXNumberEdit ID="edOperOrderQty" runat="server" DataField="OpenOrderQty" Enabled="False" />
                                    <px:PXNumberEdit CommitChanges="True" ID="edReceiptQty" runat="server" DataField="ReceiptQty" />
                                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" CommitChanges="true" />
                                    <px:PXNumberEdit ID="edTranCost" runat="server" DataField="TranCost" CommitChanges="true" />
                                    <px:PXNumberEdit ID="edTranCostFinal" runat="server" DataField="TranCostFinal" />
                                    <px:PXDropDown ID="edPOType" runat="server" DataField="POType" />
                                    <px:PXSelector ID="edPONbr" runat="server" DataField="PONbr" AllowEdit="True" />
                                    <px:PXSelector ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" AllowEdit="True" />
                                    <px:PXSelector ID="edSOShipmentNbr" runat="server" DataField="SOShipmentNbr" AllowEdit="True" />
                                    <px:PXLayoutRule runat="server" Merge="True" />
                                    <px:PXNumberEdit Size="xxs" ID="edPOLineNbr" runat="server" DataField="POLineNbr" />
                                    <px:PXCheckBox ID="chkAllowComplete" runat="server" DataField="AllowComplete" />
                                    <px:PXLayoutRule runat="server" Merge="False" />
                                    <px:PXSelector ID="edOrigReceiptNbr" runat="server" DataField="OrigReceiptNbr" AllowEdit="True" />
                                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" ColumnSpan="2" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server" DataField="BranchID" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="grid" />
                                        </Parameters>
                                    </px:PXSegmentMask>
									<px:PXSegmentMask CommitChanges="True" ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" AutoRefresh="true" />
									<px:PXSegmentMask CommitChanges="True" ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="true" />
                                    <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                    <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" AutoRefresh="True" DataField="TaskID" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <CallbackCommands PasteCommand="PasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
                        <Mode InitNewRow="True" AllowFormEdit="True" AllowDragRows="true"/>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSPOReceiptLine_binLotSerial" CommandSourceID="ds" DependOnGrid="grid" />
                                <px:PXToolBarSeperator />
                                <px:PXToolBarButton Text="Add Line" Key="cmdAddReceiptLine" CommandSourceID="ds" CommandName="AddPOReceiptLine" />
                                <px:PXToolBarButton Text="Add Order" Key="cmdPO" CommandSourceID="ds" CommandName="AddPOOrder" />
                                <px:PXToolBarButton Text="Add Order Line" Key="cmdAddPOLine" CommandSourceID="ds" CommandName="AddPOOrderLine" />
                                <px:PXToolBarButton Key="cmdPOReceiptReturn" CommandSourceID="ds" CommandName="AddPOReceiptReturn" />
                                <px:PXToolBarButton Key="cmdPOReceiptLineReturn" CommandSourceID="ds" CommandName="AddPOReceiptLineReturn" />
                                <px:PXToolBarButton Text="Add Transfer" Key="cmdAddTransfer" CommandSourceID="ds" CommandName="AddTransfer" />
                                <px:PXToolBarSeperator />
                                <px:PXToolBarButton Text="View PO Order" Key="cmdViewPOOrder">
                                    <AutoCallBack Command="ViewPOOrder" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
																	<AutoCallBack Target="grid" Command="AddNew" Argument="1"></AutoCallBack>
																	<ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Cut Row" SyncText="false" ImageSet="main" ImageKey="Copy">
																	<AutoCallBack Target="grid" Command="Copy"></AutoCallBack>
																	<ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Insert Cut Row" SyncText="false" ImageSet="main" ImageKey="Paste">
																	<AutoCallBack Target="grid" Command="Paste"></AutoCallBack>
																	<ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Purchase Orders" VisibleExp="DataControls[&quot;edReceiptType&quot;].Value!=RX" BindingContext="form">
                <Template>
                    <px:PXGrid ID="formOrders" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="ReceiptOrdersLink" >
                                <RowTemplate>
                                 <px:PXSelector ID="edPONumber" runat="server" 
                                        DataField="PONbr" AutoRefresh="True"
                                        AllowEdit="True" edit="1" />
                                 <px:PXSelector ID="edPayToVendor" runat="server" 
                                        DataField="PayToVendorID" AutoRefresh="True"
                                        AllowEdit="True" edit="1" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="POType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="PONbr" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CuryID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TaxZoneID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TaxCalcMode" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="PayToVendorID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
                        </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Put Away History" VisibleExp="DataControls[&quot;edReceiptType&quot;].Value!=RX" BindingContext="form">
				<Template>
					<px:PXGrid ID="formTransfers" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="RelatedTransfers" >
								<RowTemplate>
									<px:PXSelector ID="edTransferRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" AllowEdit="True" edit="1" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="RefNbr" RenderEditorText="True" />
									<px:PXGridColumn DataField="Status" RenderEditorText="True" />
									<px:PXGridColumn DataField="TransferType" RenderEditorText="True" />
									<px:PXGridColumn DataField="TranDate" RenderEditorText="True" />
									<px:PXGridColumn DataField="FinPeriodID" RenderEditorText="True" />
									<px:PXGridColumn DataField="SiteID" RenderEditorText="True" />
									<px:PXGridColumn DataField="TotalQty" RenderEditorText="True" />
									<px:PXGridColumn DataField="BatchNbr" RenderEditorText="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
						</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="PR History" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="formPRHistory" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" BorderStyle="None" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="ReceiptHistory" >
                                <RowTemplate>
	                                <px:PXSelector SuppressLabel="True" Size="s" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="True" AllowEdit="True" edit="1" />
	                                <px:PXSelector SuppressLabel="True" Size="s" ID="edInvtRefNbr" runat="server" DataField="InvtRefNbr" AutoRefresh="True" AllowEdit="True" edit="1" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ReceiptType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="ReceiptNbr" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DocDate" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="FinPeriodID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TotalQty" RenderEditorText="True" />
	                                <px:PXGridColumn DataField="InvtDocType" RenderEditorText="True" />
	                                <px:PXGridColumn DataField="InvtRefNbr" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
                        </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Billing History" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="formBillingHistory" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" BorderStyle="None" StatusField="StatusText" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="ApDocs" >
                                <RowTemplate>
	                                <px:PXSelector SuppressLabel="True" Size="s" ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" AllowEdit="True" edit="1" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="RefNbr" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DocDate" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TotalQty" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TotalAmt" RenderEditorText="True" />
	                                <px:PXGridColumn DataField="AccruedQty" RenderEditorText="True" />
	                                <px:PXGridColumn DataField="AccruedAmt" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TotalPPVAmt" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
                        </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Landed Costs" VisibleExp="DataControls[&quot;edReceiptType&quot;].Value!=RN" BindingContext="form">
                <Template>
                    <px:PXGrid ID="gridLandedCosts" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="landedCosts" >
                                <RowTemplate>
                                </RowTemplate>
                                <Columns>
	                                <px:PXGridColumn DataField="LCDocType" RenderEditorText="True" />
	                                <px:PXGridColumn DataField="LCRefNbr" RenderEditorText="True" LinkCommand="ViewItem"/>
                                    <px:PXGridColumn DataField="DocDate" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="VendorID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="LandedCostCodeID" RenderEditorText="True" />
	                                <px:PXGridColumn DataField="CuryLineAmt" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CuryID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="APRefNbr" RenderEditorText="True" LinkCommand="ViewItem"/>
	                                <px:PXGridColumn DataField="INRefNbr" RenderEditorText="True" LinkCommand="ViewItem"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="True" AllowUpdate="False" />
                        <ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="True" />
							</Actions>
						</ActionBar>
                        </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Other Information">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
					<px:PXSelector ID="edInventoryRefNbr" runat="server" DataField="InventoryRefNbr" Enabled="False" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edInvoiceDate" runat="server" DataField="InvoiceDate" />
				</Template>
			</px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" Container="Window" MinHeight="180"></AutoSize>
        <CallbackCommands>
            <Refresh CommitChanges="True" PostData="Page" />
            <Search CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <script type="text/javascript">
        function PanelAdd_Load() {
            px_callback.addHandler(_receiptHhandleCallback);
            var barcode = px_alls["edBarCodePnl"];
            var subitem = px_alls["edSubItemIDPnl"];
            var lotSerial = px_alls["edLotSerialNbrPnl"];
            var location = px_alls["edLocationIDPnl"];
            barcode.focus();
            barcode.events.addEventHandler("keyPress", _keypress);
            subitem.events.addEventHandler("keyPress", _keypress);
            lotSerial.events.addEventHandler("keyPress", _keypress);
            location.events.addEventHandler("keyPress", _keypress);
        }

        function _keypress(ctrl, ev) {
            var me = this, timeout = this._enterTimeoutID;
            if (timeout) clearTimeout(timeout);
            this._enterTimeoutID = setTimeout(function () {
                var autoAdd = px_alls["chkAutoAddLine"];
                if (autoAdd != null && autoAdd.getValue() == true)
                    ctrl.updateValue();
            }, 500);
        }

        function _receiptHhandleCallback(context, error) {
            var barcode = px_alls["edBarCodePnl"];
            if (context != null && context.info != null && context.info.name == "AddPOReceiptLine2" && barcode != null) {
                barcode.focus();
                return;
            }

            if (context == null || context.info == null || context.info.name != "Save" || !context.controlID.endsWith("_frmReceipt"))
                return;

            var item = px_alls["edInventoryIDPnl"];
            var subitem = px_alls["edSubItemIDPnl"];
            var lotSerial = px_alls["edLotSerialNbrPnl"];
            var location = px_alls["edLocationIDPnl"];
            var description = px_alls["edDescriptionPnl"];

            if (description != null && description.getValue() != null)
                document.getElementById("audioDing").play();

            if (barcode != null && barcode.getValue() == null && barcode.getEnabled())
            { barcode.focus(); return; }

            if (item != null && item.getValue() == null && barcode != null)
            { barcode.elemText.select(); barcode.focus(); return; }

            if (subitem != null && subitem.getValue() == null && subitem.getEnabled())
            { subitem.focus(); return; }

            if (lotSerial != null && lotSerial.getValue() == null && lotSerial.getEnabled())
            { lotSerial.focus(); return; }

            if (location != null && location.getValue() == null && location.getEnabled())
            { location.focus(); return; }
        }

    </script>
	<audio id="audioDing" src="../../Sounds/Ding.wav" preload="auto" style="visibility: hidden"></audio>
    <px:PXSmartPanel ID="PanelAddRL" runat="server" Style="z-index: 108;" Width="708px" Key="addReceipt" Caption="Add Receipt Line"
        CaptionVisible="True" LoadOnDemand="true" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="frmReceipt" ClientEvents-AfterLoad="PanelAdd_Load">
        <px:PXFormView ID="frmReceipt" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="addReceipt"
            SkinID="Transparent" CaptionVisible="False">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="true" ControlSize="SM" />      
                <px:PXTextEdit CommitChanges="True" ID="edBarCodePnl" runat="server" DataField="BarCode" />
                <px:PXSegmentMask CommitChanges="True" ID="edInventoryIDPnl" runat="server" DataField="InventoryID" />
                <px:PXSegmentMask CommitChanges="True" ID="edSubItemIDPnl" runat="server" DataField="SubItemID" />

                <px:PXTextEdit CommitChanges="True" ID="edLotSerialNbrPnl" runat="server" DataField="LotSerialNbr"  />
                <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="true"  />
                <px:PXSegmentMask CommitChanges="True" ID="edLocationIDPnl" runat="server" DataField="LocationID" />
                
                <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                <px:PXNumberEdit CommitChanges="True" ID="edReceiptQty" runat="server" DataField="ReceiptQty" Size="XS" />
                <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" Size="XS" />

                <px:PXLayoutRule runat="server" StartColumn="true" ControlSize="S" />
                <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" Size="M" />
                <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" Size="SM" />
                <px:PXDropDown CommitChanges="True" ID="edPOType" runat="server" DataField="POType" />

                <px:PXSelector ID="edPONbr" runat="server" DataField="PONbr" Enabled="False" />
                <px:PXNumberEdit ID="edPOLineNbr" runat="server" DataField="POLineNbr" Enabled="False" />
                <px:PXSegmentMask ID="edShipFromSiteID" runat="server"  DataField="ShipFromSiteID" Enabled="False" />
                <px:PXSelector ID="edSOOrderType" runat="server" DataField="SOOrderType" Enabled="False" />
                <px:PXSelector ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" Enabled="False" />
                <px:PXNumberEdit ID="edSOOrderLineNbr" runat="server" DataField="SOOrderLineNbr" Enabled="False" />
                <px:PXSelector ID="edSOShipmentNbr" runat="server" DataField="SOShipmentNbr" Enabled="False" />
                <px:PXNumberEdit CommitChanges="True" ID="edUnitCost" runat="server" DataField="UnitCost" />       
                
                <px:PXCheckBox CommitChanges="True" ID="chkByOne" runat="server" Checked="True" DataField="ByOne" />
                <px:PXCheckBox CommitChanges="True" ID="chkAutoAddLine" runat="server" DataField="AutoAddLine" />
                <px:PXTextEdit ID="edDescriptionPnl" runat="server" DataField="Description" SkinID="Label" Size="M" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton7" runat="server" Text="Add" CommandSourceID="ds" CommandName="AddPOReceiptLine2" />
            <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="No" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108;" Width="764px" Height="500px" Caption="Allocations" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSPOReceiptLine_lotseropts" DataSourceID="ds"
            SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule26" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule ID="PXLayoutRule27" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSPOReceiptLine_generateLotSerial"
                    CommandSourceID="ds">
                </px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SyncPosition="true" SkinID="Details">
            <Mode InitNewRow="True" />
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="splits">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="LotSerialNbr" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ExpireDate" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule ID="PXLayoutRule28" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true" />
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="POReceiptLineSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="POReceiptLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="POReceiptLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="POReceiptLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="POReceiptLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="POReceiptLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="POReceiptLineSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddPOLine" runat="server" Height="396px" HideAfterAction="false" Style="z-index: 108;" Width="960px"
        Key="poLinesSelection" Caption="Add Purchase Order Line" CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true"
        AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="frmPOFilter">
        <px:PXFormView ID="frmPOFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="filter" Caption="PO Selection"
            SkinID="Transparent" CaptionVisible="false">
            <CallbackCommands>
                <Refresh RepaintControls="None" RepaintControlsIDs="gridOL" />
                <Save RepaintControls="None" RepaintControlsIDs="gridOL" />
            </CallbackCommands>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edOrderType" runat="server" AllowNull="False" DataField="OrderType" SelectedIndex="2" />
                <px:PXLayoutRule ID="PXLayoutRule30" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridOL" runat="server" Height="50px" Width="100%" DataSourceID="ds" AutoAdjustColumns="true" PageSize="200" SkinID="Inquire">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="poLinesSelection">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AutoCallBack="True" TextAlign="Center" />
                        <px:PXGridColumn AllowUpdate="False" DataField="OrderNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Order Nbr." />
                        <px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" Label="Vendor" />
                        <px:PXGridColumn AllowNull="False" DataField="LineType" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn DataField="OrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="LeftToReceiveQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="TranDesc" />
                        <px:PXGridColumn DataField="PromisedDate" TextAlign="Left" />
                        <px:PXGridColumn AllowNull="False" DataField="RcptQtyMin" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="RcptQtyMax" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="RcptQtyAction" Type="DropDownList" />
                        <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
            <CallbackCommands>
                <Save RepaintControls="None" />
            </CallbackCommands>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" Text="Add" SyncVisible="false">
                <AutoCallBack Command="AddPOOrderLine2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton9" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddPO" runat="server" Style="z-index: 108; height: 396px;" Width="960px" Caption="Add Purchase Order"
        CaptionVisible="True" LoadOnDemand="true" Key="openOrders" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="frmOrderFilter1" AutoRepaint="True">
        <px:PXFormView ID="frmOrderFilter1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="filter"
            Caption="PO Selection" SkinID="Transparent" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" SelectedIndex="2" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edShipToBAccountID" runat="server" DataField="ShipToBAccountID" AutoRefresh="true" />
                <px:PXSegmentMask CommitChanges="True" ID="edShipToLocationID" runat="server" DataField="ShipToLocationID" AutoRefresh="true" /></Template>
        </px:PXFormView>
        <px:PXGrid ID="grdOpenOrders" runat="server" Height="304px" Width="100%" DataSourceID="ds" Style="border-width: 1px 0px;" AutoAdjustColumns="true" PageSize="200" SkinID="Inquire">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="openOrders">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="OrderType" />
                        <px:PXGridColumn DataField="OrderNbr" />
                        <px:PXGridColumn DataField="OrderDate" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" />
                        <px:PXGridColumn DataField="CuryID" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrderTotal" TextAlign="Right" />
                        <px:PXGridColumn DataField="VendorRefNbr" />
                        <px:PXGridColumn DataField="TermsID" />
                        <px:PXGridColumn DataField="OrderDesc" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ReceivedQty" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="LeftToReceiveQty" TextAlign="Right" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton8" runat="server" Text="Add" SyncVisible="false">
                <AutoCallBack Command="AddPOOrder2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Add Return for PO Receipt --%>
    <px:PXSmartPanel ID="panelAddReceiptReturn" runat="server" Style="z-index: 108; height: 396px;" Width="960px" Caption="Add Receipt"
        CaptionVisible="True" LoadOnDemand="true" Key="poReceiptReturn" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="frmReceiptReturnFilter" AutoRepaint="True">
        <px:PXFormView ID="frmReceiptReturnFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="returnFilter"
            Caption="PO Receipt Selection" SkinID="Transparent" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
                <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="true" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="true" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grdReceiptReturn" runat="server" Height="304px" Width="100%" DataSourceID="ds" Style="border-width: 1px 0px;" AutoAdjustColumns="true" PageSize="200" SkinID="Inquire" >
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="poReceiptReturn" >
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="ReceiptNbr" />
                        <px:PXGridColumn DataField="ReceiptType" />
                        <px:PXGridColumn DataField="VendorID" />
                        <px:PXGridColumn DataField="VendorLocationID" />
                        <px:PXGridColumn DataField="ReceiptDate" />
                        <px:PXGridColumn DataField="OrderQty" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton14" runat="server" Text="Add" SyncVisible="false">
                <AutoCallBack Command="AddPOReceiptReturn2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton17" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton18" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Add Return Line for PO Receipt --%>
    <px:PXSmartPanel ID="panelAddReceiptLineReturn" runat="server" Style="z-index: 108; height: 396px;" Width="960px" Caption="Add Receipt Line"
        CaptionVisible="True" LoadOnDemand="true" Key="poReceiptLineReturn" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="frmReceiptReturnLineFilter" AutoRepaint="True">
        <px:PXFormView ID="frmReceiptReturnLineFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="returnFilter"
            Caption="PO Receipt Selection" SkinID="Transparent" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
                <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="true" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="true" />
                <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="true" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grdReceiptLineReturn" runat="server" Height="304px" Width="100%" DataSourceID="ds" Style="border-width: 1px 0px;" AutoAdjustColumns="true" PageSize="200" SkinID="Inquire">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="poReceiptLineReturn">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="PONbr" />
                        <px:PXGridColumn DataField="POType" />
                        <px:PXGridColumn DataField="ReceiptNbr" />
                        <px:PXGridColumn DataField="InvoiceNbr" />
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SiteID" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ReceiptQty" />
                        <px:PXGridColumn DataField="ReturnedQty" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel9" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton19" runat="server" Text="Add" SyncVisible="false">
                <AutoCallBack Command="AddPOReceiptLineReturn2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton20" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton21" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddTransfer" runat="server" Style="z-index: 108; height: 396px;" Width="960px" Caption="Add Transfer Order"
        CaptionVisible="True" LoadOnDemand="true" Key="openTransfers" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="formTransferFilter1" AutoRepaint="True">
        <px:PXFormView ID="formTransferFilter1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="filter"
            Caption="PO Selection" SkinID="Transparent" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edShipFromSiteID" runat="server" DataField="ShipFromSiteID" AutoRefresh="true" />
			</Template>
        </px:PXFormView>
        <px:PXGrid ID="gridOpenTransfers" runat="server" Height="304px" Width="100%" DataSourceID="ds" Style="border-width: 1px 0px;" AutoAdjustColumns="true" PageSize="200" SkinID="Inquire">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="openTransfers">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowResize="False" />
                        <px:PXGridColumn DataField="OrderType" />
                        <px:PXGridColumn DataField="OrderNbr" />
                        <px:PXGridColumn DataField="ShipmentNbr" />
						<px:PXGridColumn DataField="INRegister__SiteID" />
						<px:PXGridColumn DataField="INRegister__ToSiteID" />
						<px:PXGridColumn DataField="INRegister__TranDate" />
						<px:PXGridColumn DataField="INRegister__TranDesc" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton11" runat="server" Text="Add" >
                <AutoCallBack Command="AddTransfer2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton12" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton13" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddINTran" runat="server" Height="396px" HideAfterAction="false" Style="z-index: 108;" Width="960px"
        Key="intranSelection" Caption="Add Transfer Line" CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true"
        AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="frmTransferFilter">
        <px:PXFormView ID="frmTransferFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="filter" Caption="Transfer Selection"
            SkinID="Transparent" CaptionVisible="false">
            <CallbackCommands>
                <Refresh RepaintControls="None" RepaintControlsIDs="gridOL" />
                <Save RepaintControls="None" RepaintControlsIDs="gridOL" />
            </CallbackCommands>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edShipFromSiteID" runat="server"  AutoRefresh="True" DataField="ShipFromSiteID" SelectedIndex="2" />
                <px:PXLayoutRule ID="PXLayoutRule30" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" AutoRefresh="True" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="PXGrid1" runat="server" Height="240px" Width="100%" DataSourceID="ds" Style="border-width: 1px 0px" AutoAdjustColumns="true" PageSize="200" SkinID="Inquire">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="intranSelection">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AutoCallBack="True" TextAlign="Center" />
                        <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Order Nbr." />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="TranDesc" />
                        <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
            <CallbackCommands>
                <Save RepaintControls="None" />
            </CallbackCommands>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton15" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton16" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
