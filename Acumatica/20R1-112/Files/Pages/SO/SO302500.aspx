<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO302500.aspx.cs" Inherits="Page_SO302500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOPickingWorksheetReview" PrimaryView="worksheet" PageLoadBehavior="GoLastRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Save" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Cancel" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ShowSplits" CommitChanges="true" Visible="False" DependOnGrid="gridWorksheetLines" />
            <px:PXDSCallbackCommand Name="ShowShipments" CommitChanges="true" Visible="False" DependOnGrid="gridPickers" />
            <px:PXDSCallbackCommand Name="ShowPickers" CommitChanges="true" Visible="False" DependOnGrid="gridShipments" />
            <px:PXDSCallbackCommand Name="ShowPickList" CommitChanges="true" Visible="False" DependOnGrid="gridPickers" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="worksheet" Caption="Picking Worksheet Review" NoteIndicator="True" FilesIndicator="True" LinkIndicator="true" DefaultControlID="edWorksheetNbr" MarkRequired="Dynamic" SyncPosition="False">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edWorksheetNbr" runat="server" DataField="WorksheetNbr" AutoRefresh="true" CommitChanges="True"/>
            <px:PXDropDown ID="edWorksheetType" runat="server" DataField="WorksheetType" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edPickDate" runat="server" DataField="PickDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXNumberEdit ID="edShipmentQty" runat="server" DataField="Qty" Enabled="False" />
            <px:PXNumberEdit ID="edShipmentWeight" runat="server" DataField="ShipmentWeight" Enabled="False" />
            <px:PXNumberEdit ID="edShipmentVolume" runat="server" DataField="ShipmentVolume" Enabled="False" />
            <px:PXNumberEdit ID="edPackageCount" runat="server" DataField="PackageCount" Enabled="False" />
            <px:PXNumberEdit ID="edPackageWeight" runat="server" DataField="PackageWeight" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%">
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid ID="gridWorksheetLines" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="true">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" AllowFormEdit="False" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="ShowSplits" CommandName="ShowSplits" CommandSourceID="ds" DependOnGrid="gridWorksheetLines" />
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="worksheetLines" DataKeyNames="WorksheetNbr,LineNbr">
                                <Columns>
                                    <px:PXGridColumn DataField="WorksheetNbr" />
                                    <px:PXGridColumn DataField="LineNbr" />
                                    <px:PXGridColumn DataField="SiteID" />
                                    <px:PXGridColumn DataField="LocationID" NullText="<SPLIT>"/>
                                    <px:PXGridColumn DataField="InventoryID" />
                                    <px:PXGridColumn DataField="SubItemID" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="LotSerialNbr" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OrigOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OpenOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PickedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PackedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXTextEdit ID="edOrigOrderType" runat="server" DataField="OrigOrderType" />
                                    <px:PXTextEdit ID="edOrigOrderNbr" runat="server" DataField="OrigOrderNbr" />
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM"/>
                                    <px:PXNumberEdit ID="edShippedQty" runat="server" DataField="Qty" />
                                    <px:PXNumberEdit ID="edOrigOrderQty" runat="server" DataField="OrigOrderQty" />
                                    <px:PXLayoutRule runat="server" ColumnSpan="2" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
                                    <px:PXTextEdit ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" />
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipments">
                <Template>
                    <px:PXGrid ID="gridShipments" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="true" FilesIndicator="False" NoteIndicator="False">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" AllowFormEdit="False" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Pickers" Key="ShowPickers" CommandName="ShowPickers" CommandSourceID="ds" DependOnGrid="gridShipments" />
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="shipments">
                                <Columns>
                                    <px:PXGridColumn DataField="Picked" Type="CheckBox" />
                                    <px:PXGridColumn DataField="ShipmentNbr" />
                                    <px:PXGridColumn DataField="PickedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PackedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ShipmentQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ShipmentWeight" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ShipmentVolume" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Status" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" AllowEdit="True"/>
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Pickers">
                <Template>
                    <px:PXGrid ID="gridPickers" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="true">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" AllowFormEdit="False" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="ShowShipments" CommandName="ShowShipments" CommandSourceID="ds" DependOnGrid="gridPickers" />
                                <px:PXToolBarButton Key="ShowPickList" CommandName="ShowPickList" CommandSourceID="ds" DependOnGrid="gridPickers" />
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="pickers">
                                <Columns>
                                    <px:PXGridColumn DataField="Confirmed" Type="CheckBox" />
                                    <px:PXGridColumn DataField="PickerNbr" />
                                    <px:PXGridColumn DataField="UserID" />
                                    <px:PXGridColumn DataField="PathLength"/>
                                    <px:PXGridColumn DataField="CartID" />
                                    <px:PXGridColumn DataField="SortingLocationID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <%-- Allocations Dialog --%>
    <px:PXSmartPanel ID="PanelAllocations" runat="server" Caption="Allocations" CaptionVisible="True" Key="worksheetLineSplits" LoadOnDemand="True" AutoReload="True" AutoRepaint="True" >
        <px:PXGrid ID="gridWorksheetLineSplits" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="worksheetLineSplits">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="LotSerialNbr" />
                        <px:PXGridColumn DataField="PickedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ExpireDate" />
                        <px:PXGridColumn DataField="SortingLocationID" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSplitSubItemID" runat="server" DataField="SubItemID" />
                        <px:PXSegmentMask ID="edSplitLocationID" runat="server" DataField="LocationID" />
                        <px:PXNumberEdit ID="edSplitQty" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edSplitUOM" runat="server" DataField="UOM" />
                        <px:PXTextEdit ID="edSplitLotSerialNbr" runat="server" DataField="LotSerialNbr" />
                        <px:PXDateTimeEdit ID="edSplitExpireDate" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="300" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" AllowFormEdit="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
    <%-- Pickers of Shipment Dialog --%>
    <px:PXSmartPanel ID="ViewPickersDialog" runat="server" Caption="Pickers of a Shipment" CaptionVisible="True" Key="shipmentPickers" LoadOnDemand="True" AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridShipmentPickers" runat="server" DataSourceID="ds" SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="shipmentPickers">
                    <Columns>
                        <px:PXGridColumn DataField="Confirmed" Type="CheckBox" />
                        <px:PXGridColumn DataField="PickerNbr" />
                        <px:PXGridColumn DataField="UserID" />
                        <px:PXGridColumn DataField="CartID" />
                        <px:PXGridColumn DataField="SortingLocationID" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="300" />
            <Mode AllowUpload="False" AllowAddNew="False" AllowUpdate="False" AllowDelete="False"/>
        </px:PXGrid>
    </px:PXSmartPanel>
    <%-- Shipments of Picker Dialog --%>
    <px:PXSmartPanel ID="ViewTotesInCartDialog" runat="server" Caption="Assigned Shipments" CaptionVisible="True" Key="pickerShipments" LoadOnDemand="True" AutoReload="True" AutoRepaint="True" Height="500" Width="900" AllowResize="False">
        <px:PXSplitContainer ID="splitPickerShipments" runat="server" Orientation="Horizontal" Panel2Overflow="True" Panel1Overflow="True" Panel1MinSize="100" Panel2MinSize="100" SplitterPosition="240">
            <Template1>
                <px:PXGrid ID="gridPickerShipments" runat="server" DataSourceID="ds" SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="False" NoteIndicator="False" FilesIndicator="False" SyncPosition="True" SyncPositionWithGraph="True" Width="100%" Height="50px">
                    <Levels>
                        <px:PXGridLevel DataMember="pickerShipments">
                            <Columns>
                                <px:PXGridColumn DataField="ShipmentNbr"/>
                                <px:PXGridColumn DataField="ToteID"/>
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoCallBack Target="gridPickListByTote" Command="Refresh">
                        <Behavior CommitChanges="True" RepaintControlsIDs="gridPickListByTote"/>
                    </AutoCallBack>
                    <AutoSize Enabled="True" MinHeight="80" />
                    <Mode AllowUpload="False" AllowAddNew="False" AllowUpdate="False" AllowDelete="False"/>
                </px:PXGrid>
            </Template1>
            <Template2>
                <px:PXGrid ID="gridPickListByTote" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Width="100%" Caption="Content" AutoAdjustColumns="True">
                    <Levels>
                        <px:PXGridLevel DataMember="pickerListByShipment">
                            <Columns>
                                <px:PXGridColumn DataField="LocationID" />
                                <px:PXGridColumn DataField="InventoryID" />
                                <px:PXGridColumn DataField="SubItemID" />
                                <px:PXGridColumn DataField="LotSerialNbr" />
                                <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                                <px:PXGridColumn DataField="UOM" />
                                <px:PXGridColumn DataField="ExpireDate" />
                                <px:PXGridColumn DataField="PickedQty" TextAlign="Right" />
                            </Columns>
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                <px:PXSegmentMask ID="edPickTote_LocationID" runat="server" DataField="LocationID" />
                                <px:PXSegmentMask ID="edPickTote_SubItemID" runat="server" DataField="SubItemID" />
                                <px:PXNumberEdit ID="edPickTote_Qty" runat="server" DataField="Qty" />
                                <px:PXSelector ID="edPickTote_UOM" runat="server" DataField="UOM" />
                                <px:PXTextEdit ID="edPickTote_LotSerialNbr" runat="server" DataField="LotSerialNbr" />
                                <px:PXDateTimeEdit ID="edPickTote_ExpireDate" runat="server" DataField="ExpireDate" />
                                <px:PXNumberEdit ID="edPickTote_PickedQty" runat="server" DataField="PickedQty" />
                            </RowTemplate>
                            <Layout ColumnsMenu="False" />
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="100" />
                    <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" AllowFormEdit="False" />
                </px:PXGrid>
            </Template2>
        </px:PXSplitContainer>
    </px:PXSmartPanel>
    <%-- Pick List Dialog --%>
    <px:PXSmartPanel ID="PanelPickList" runat="server" Caption="Pick List" CaptionVisible="True" Key="pickerList" LoadOnDemand="True" AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridPickList" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="pickerList">
                    <Columns>
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="LotSerialNbr" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ExpireDate" />
                        <px:PXGridColumn DataField="PickedQty" TextAlign="Right" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edPick_LocationID" runat="server" DataField="LocationID" />
                        <px:PXSegmentMask ID="edPick_SubItemID" runat="server" DataField="SubItemID" />
                        <px:PXNumberEdit ID="edPick_Qty" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edPick_UOM" runat="server" DataField="UOM" />
                        <px:PXTextEdit ID="edPick_LotSerialNbr" runat="server" DataField="LotSerialNbr" />
                        <px:PXDateTimeEdit ID="edPick_ExpireDate" runat="server" DataField="ExpireDate" />
                        <px:PXNumberEdit ID="edPick_PickedQty" runat="server" DataField="PickedQty" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="300" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" AllowFormEdit="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
</asp:Content>