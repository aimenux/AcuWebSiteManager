<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN405000.aspx.cs"
    Inherits="Page_IN405000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter"
                     TypeName="PX.Objects.IN.InventoryTranHistEnq"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify"
        DataMember="Filter" DefaultControlID="edInventoryID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" AutoRefresh="true" AllowEdit="true" CommitChanges="true">
                <GridProperties>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" AutoRefresh="True" DataField="LocationID" />
            <px:PXTextEdit CommitChanges="True" ID="edLotSerialNbr" runat="server" AllowNull="False" DataField="LotSerialNbr" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" DisplayFormat="d" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" DisplayFormat="d" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkSummaryByDay" runat="server" DataField="SummaryByDay" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkShowAdjUnitCost" runat="server" DataField="ShowAdjUnitCost" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto"
        AllowPaging="True" AllowSearch="True" BatchUpdate="True" Caption="Transaction Details" SkinID="PrimaryInquire" SyncPosition="True"
        FastFilterFields="RefNbr,INTran__SOOrderNbr,INTran__POReceiptNbr">
        <Levels>
            <px:PXGridLevel DataMember="ResultRecords">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr" >
                    </px:PXSelector>
                    <px:PXSelector ID="edINTran__SOOrderType" runat="server" DataField="INTran__SOOrderType" />
                    <px:PXSelector ID="edINTran__SOOrderNbr" runat="server" DataField="INTran__SOOrderNbr" AllowEdit="true" />
                    <px:PXSelector ID="edINTran__POReceiptNbr" runat="server" DataField="INTran__POReceiptNbr" AllowEdit="true" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="GridLineNbr" />
                    <px:PXGridColumn DataField="INTran__InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="INTran__TranType" />
                    <px:PXGridColumn DataField="RefNbr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DocType" Visible="false" />
                    <px:PXGridColumn DataField="INTranSplit__SubItemID" DisplayFormat="&gt;AA-A" />
                    <px:PXGridColumn DataField="INTran__SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="INTranSplit__LocationID" AllowShowHide="Server" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="INTranSplit__LotSerialNbr" AllowShowHide="Server" Width="120px" />
                    <px:PXGridColumn DataField="INTran__FinPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn DataField="INTran__TranPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn DataField="INTran__Released" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="INTran__ReleasedDateTime" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="BegQty" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyIn" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyOut" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="EndQty" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="INTran__SOOrderType" DisplayFormat="&gt;aa" Label="SO Order Type" />
                    <px:PXGridColumn DataField="INTran__SOOrderNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="SO Order Nbr.">
                        <NavigateParams>
                            <px:PXControlParam Name="OrderType" ControlID="grid" PropertyName="DataValues[&quot;INTran__SOOrderType&quot;]" />
                            <px:PXControlParam Name="OrderNbr" ControlID="grid" PropertyName="DataValues[&quot;INTran__SOOrderNbr&quot;]" />
                        </NavigateParams>
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="INTran__POReceiptNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="PO Receipt Nbr." />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="ViewAllocDet"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
