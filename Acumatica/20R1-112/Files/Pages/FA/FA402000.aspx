<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA402000.aspx.cs"
    Inherits="Page_FA402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.AssetSummary" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        TabIndex="1100" DefaultControlID="edClassID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="AssetTypeID" ID="edAssetTypeID" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="PropertyType" ID="edPropertyType" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="Condition" ID="edCondition" />
		    <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edAcqDateFrom" runat="server" DataField="AcqDateFrom" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edAcqDateTo" runat="server" DataField="AcqDateTo" LabelWidth="34px" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="BranchID" ID="edLocationID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="BuildingID" ID="edBuildingID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="S" ControlSize="XS" />
            <px:PXTextEdit CommitChanges="True" runat="server" DataField="Floor" ID="edFloor" />
            <px:PXTextEdit CommitChanges="True" runat="server" DataField="Room" ID="edRoom" LabelWidth="94px" />
            <px:PXLayoutRule runat="server" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="EmployeeID" ID="edEmployeeID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="Department" ID="edDepartment" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector CommitChanges="True" ID="edPONumber" runat="server" DataField="PONumber" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ReceiptNbr" ID="edReceiptNbr" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="BillNumber" ID="edBillNumber" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Fixed Assets Summary"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" FastFilterFields="AssetCD, Description" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="assets">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edAssetCD" runat="server" AllowEdit="True" DataField="AssetCD" />
                    <px:PXSelector ID="edParentAssetID" runat="server" AllowEdit="True" DataField="ParentAssetID" />
                    <px:PXSelector ID="edClassID" runat="server" AllowEdit="True" DataField="ClassID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="AssetCD" DisplayFormat="&gt;CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="ClassID" />
                    <px:PXGridColumn DataField="ParentAssetID" />
                    <px:PXGridColumn DataField="AssetTypeID" RenderEditorText="True" />
                    <px:PXGridColumn DataField="UsefulLife" TextAlign="Right" />
                    <px:PXGridColumn DataField="FADetails__DepreciateFromDate" />
                    <px:PXGridColumn AllowNull="False" DataField="FADetails__AcquisitionCost" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="FADetails__PropertyType" RenderEditorText="True" />
                    <px:PXGridColumn AllowNull="False" DataField="FADetails__Condition" RenderEditorText="True" />
                    <px:PXGridColumn DataField="FADetails__ReceiptNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="FADetails__PONumber" />
                    <px:PXGridColumn DataField="FADetails__BillNumber" />
                    <px:PXGridColumn DataField="FALocationHistory__LocationID" DisplayFormat="&gt;AAAAAA" />
                    <px:PXGridColumn DataField="FALocationHistory__BuildingID" />
                    <px:PXGridColumn DataField="FALocationHistory__Floor" />
                    <px:PXGridColumn DataField="FALocationHistory__Room" />
                    <px:PXGridColumn DataField="FALocationHistory__EmployeeID" />
                    <px:PXGridColumn DataField="FALocationHistory__Department" DisplayFormat="&gt;aaaaaaaaaa" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar PagerVisible="False" />
    </px:PXGrid>
    <px:PXSmartPanel ID="spDisposeParamDlg" runat="server" DesignView="Content" Key="DispParams" LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel"
        Caption="Disposal Parameters" CaptionVisible="True" HideAfterAction="False">
        <px:PXFormView ID="DisposePrm" runat="server" DataSourceID="ds" Style="z-index: 108" Width="100%" DataMember="DispParams"
            Caption="Dispose Parameters" SkinID="Transparent" TabIndex="2300">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edDisposalDate" runat="server" DataField="DisposalDate" />
                <px:PXSelector CommitChanges="True" ID="edDisposalPeriodID" runat="server" DataField="DisposalPeriodID" />
                <px:PXNumberEdit CommitChanges="True" ID="edDisposalAmt" runat="server" AllowNull="True" DataField="DisposalAmt" />
                <px:PXSelector CommitChanges="True" ID="edDisposalMethodID" runat="server" DataField="DisposalMethodID" />
                <px:PXSegmentMask CommitChanges="True" ID="edDisposalAccountID" runat="server" DataField="DisposalAccountID" />
                <px:PXSegmentMask CommitChanges="True" ID="edDisposalSubID" runat="server" DataField="DisposalSubID" AutoRefresh="True" />
                <px:PXCheckBox CommitChanges="True" ID="chkDeprBeforeDisposal" runat="server" DataField="DeprBeforeDisposal" />
                <px:PXTextEdit ID="edReason" runat="server" DataField="Reason" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
