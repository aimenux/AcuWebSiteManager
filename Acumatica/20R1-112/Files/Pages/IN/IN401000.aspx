<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN401000.aspx.cs"
    Inherits="Page_IN401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.InventorySummaryEnq" PageLoadBehavior="PopulateSavedValues"
        PrimaryView="Filter" Visible="True" TabIndex="1">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        CaptionAlign="Justify" DefaultControlID="edInventoryID">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" AutoRefresh="true" CommitChanges="true" AllowEdit="true">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                    <PagerSettings Mode="NextPrevFirstLast" />
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
                <AutoCallBack Command="Save" Enabled="True" Target="form">
                </AutoCallBack>
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID">
                <AutoCallBack Command="Save" Enabled="True" Target="form" />
                <GridProperties FastFilterFields="Descr">
                    <Layout ColumnsMenu="False" />
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
                <Items>
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
                </Items>
            </px:PXSegmentMask>
            <px:PXCheckBox ID="chkExpandByLotSerialNbr" runat="server" DataField="ExpandByLotSerialNbr">
                <AutoCallBack Command="Save" Enabled="True" Target="form">
                </AutoCallBack>
            </px:PXCheckBox>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask ID="edSubItemCD" runat="server" DataField="SubItemCD">
                <AutoCallBack Command="Save" Enabled="True" Target="form">
                </AutoCallBack>
                <GridProperties FastFilterFields="Descr">
                    <Layout ColumnsMenu="False" />
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
                <Items>
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="1" Separator="-" TextCase="Upper" />
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="1" Separator="-" TextCase="Upper" />
                </Items>
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True">
                <AutoCallBack Command="Save" Enabled="True" Target="form" />
                <GridProperties FastFilterFields="Descr">
                    <Layout ColumnsMenu="False" />
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
                <Items>
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
                </Items>
            </px:PXSegmentMask>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" Caption="Inventory Summary" BatchUpdate="True" SkinID="PrimaryInquire"
        TabIndex="8" RestrictFields="True" SyncPosition="true" FastFilterFields="InventoryID,SiteID" OnRowDataBound="ISERecordsGrid_RowDataBound"
		MatrixMode="true">
        <Levels>
            <px:PXGridLevel DataMember="ISERecords">
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" LinkCommand="ViewAllocDet" />
                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LocationID" AllowNull="True" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyAvail" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyHardAvail" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyActual" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyNotAvail" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySOPrepared" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySOBooked" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySOShipping" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySOShipped" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySOBackOrdered" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyINIssues" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyINReceipts" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyInTransit" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyInTransitToSO" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyInAssemblyDemand" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyInAssemblySupply" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOPrepared" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOOrders" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOReceipts" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyExpired" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyOnHand" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySOFixed" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOFixedOrders" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOFixedPrepared" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOFixedReceipts" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySODropShip" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPODropShipOrders" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPODropShipPrepared" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPODropShipReceipts" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyFSSrvOrdPrepared" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyFSSrvOrdBooked" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyFSSrvOrdAllocated" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyFixedFSSrvOrd" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOFixedFSSrvOrd" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOFixedFSSrvOrdPrepared" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOFixedFSSrvOrdReceipts" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="BaseUnit" DisplayFormat="&gt;aaaaaa" />
                    <px:PXGridColumn DataField="UnitCost" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="TotalCost" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right"
                        Width="100px" />
                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="120px" />
                    <px:PXGridColumn DataField="ExpireDate" DataType="DateTime" Width="90px" AllowShowHide="Server" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        <ActionBar DefaultAction="ViewAllocDet"/>
    </px:PXGrid>
</asp:Content>
