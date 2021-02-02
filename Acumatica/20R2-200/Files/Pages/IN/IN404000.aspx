<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN404000.aspx.cs"
    Inherits="Page_IN404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter"
                     TypeName="PX.Objects.IN.InventoryTranDetEnq"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify"
        DataMember="Filter" DefaultControlID="edPeriodID" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" ID="chkByFinancialPeriod" runat="server" DataField="ByFinancialPeriod" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" AutoRefresh="True" DataField="LocationID" DataSourceID="ds" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" AutoRefresh="true" CommitChanges="true" AllowEdit="true" />
            <px:PXTextEdit CommitChanges="True" ID="edLotSerialNbr" runat="server" AllowNull="False" DataField="LotSerialNbr" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" AutoRefresh="True" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" StartGroup="True" GroupCaption="Date Range" />
            <px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" StartColumn="True" />
                <px:PXDateTimeEdit ID="edStartDate" runat="server" CommitChanges="True" DataField="StartDate" />
                <px:PXDateTimeEdit ID="edEndDate" runat="server" CommitChanges="True" DataField="EndDate" DisplayFormat="d" />
                <px:PXLayoutRule runat="server" StartColumn="True" />
                <px:PXDateTimeEdit ID="edPeriodStartDate" runat="server" DataField="PeriodStartDate" />
                <px:PXDateTimeEdit ID="edPeriodEndDate" runat="server" DataField="PeriodEndDateInclusive" />
            </px:PXPanel>
            <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" />
            <px:PXCheckBox CommitChanges="True" ID="chkSummaryByDay" runat="server" DataField="SummaryByDay" AlignLeft="True" />
            <px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" AlignLeft="True" />
            <px:PXLabel ID="lblNote1" runat="server" Height="18px">[*]  Estimated Costs</px:PXLabel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" BatchUpdate="True" Caption="Transaction Details" SkinID="PrimaryInquire"
        SyncPosition="true" FastFilterFields="RefNbr,INTran__SOOrderNbr,INTran__POReceiptNbr">
        <Levels>
            <px:PXGridLevel DataMember="ResultRecords">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr" />
                    <px:PXSelector ID="edINTran__SOOrderType" runat="server" DataField="INTran__SOOrderType" />
                    <px:PXSelector ID="edINTran__SOOrderNbr" runat="server" DataField="INTran__SOOrderNbr" AllowEdit="true" />
                    <px:PXSelector ID="edINTran__POReceiptNbr" runat="server" DataField="INTran__POReceiptNbr" AllowEdit="true" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="GridLineNbr" TextAlign="Right" />
                    <px:PXGridColumn DataField="INTran__InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="TranType" />
                    <px:PXGridColumn DataField="RefNbr">
                        <NavigateParams>
                            <px:PXControlParam ControlID="grid" Direction="Output" Name="DocType" PropertyName="DataValues[&quot;DocType&quot;]" Type="String" />
                            <px:PXControlParam ControlID="grid" Direction="Output" Name="RefNbr" PropertyName="DataValues[&quot;RefNbr&quot;]" Type="String" />
                        </NavigateParams>
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DocType" Visible="false" />
                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" />
                    <px:PXGridColumn DataField="INTran__SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="120px" />
                    <px:PXGridColumn DataField="INTran__FinPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn DataField="INTran__TranPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="INTran__ReleasedDateTime" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="BegQty" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyIn" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyOut" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="EndQty" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="BegBalance" TextAlign="Right" Width="120px" />
                    <px:PXGridColumn AllowNull="False" DataField="ExtCostIn" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="ExtCostOut" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="EndBalance" TextAlign="Right" Width="100px" />
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
