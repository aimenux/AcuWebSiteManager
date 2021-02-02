<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM207000.aspx.cs"
    Inherits="Page_PM207000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.BillingMaint" PrimaryView="Billing">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Rule Summary" DataMember="Billing"
        EmailingGraph="" LinkPage="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXSelector ID="edBillingID" runat="server" DataField="BillingID" DataSourceID="ds" DisplayMode="Value" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" Panel1MinSize="150" Panel2MinSize="700">
        <AutoSize Enabled="true" MinHeight="250" Container="Window" />
        <Template1>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AllowFilter="False" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
                <Levels>
                    <px:PXGridLevel DataMember="BillingRules">
                        <Columns>
                            <px:PXGridColumn DataField="IsActive" Type="CheckBox" />
                            <px:PXGridColumn DataField="StepID" SortDirection="Ascending" TextAlign="Right" CommitChanges="True" />
                            <px:PXGridColumn DataField="Description" CommitChanges="True" />
                            <px:PXGridColumn DataField="InvoiceGroup" CommitChanges="True" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" Container="Parent"></AutoSize>
                <ActionBar PagerVisible="False">
                </ActionBar>
                <Mode AllowColMoving="False" />
                <AutoCallBack Enabled="true" ActiveBehavior="True" Command="Refresh" Target="formRules">
                    <Behavior CommitChanges="True" RepaintControlsIDs="formRules,formSettings" />
                </AutoCallBack>
            </px:PXGrid>
        </Template1>
        <Template2>
            <px:PXFormView ID="formRules" runat="server" DataSourceID="ds" DataMember="BillingRule" Width="100%">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="SM" />
                    <px:PXDropDown ID="edType" runat="server" DataField="Type" CommitChanges="True" />

                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartRow="True" StartGroup="True" LabelsWidth="M" ControlSize="SM" GroupCaption="Transaction Selection Criteria" />
                    <px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="True" />

                    <px:PXSegmentMask ID="edAccountGroupID" runat="server" DataField="AccountGroupID" />

                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" ControlSize="SM" />
                    <px:PXSelector ID="edRateTypeID" runat="server" DataField="RateTypeID" />
                    <px:PXDropDown ID="edNoRateOption" runat="server" DataField="NoRateOption" />

                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartRow="True" StartGroup="True" LabelsWidth="M" GroupCaption="Invoice Settings" />
                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />
                    <pxa:PMFormulaEditor ID="edInvoiceFormula" runat="server" DataSourceID="ds" DataField="InvoiceFormula" />
                    <pxa:PMFormulaEditor ID="edQtyFormula" runat="server" DataSourceID="ds" DataField="QtyFormula" Parameters="@Rate,@Price" />
                    <pxa:PMFormulaEditor ID="edAmountFormula" runat="server" DataSourceID="ds" DataField="AmountFormula" Parameters="@Rate,@Price" />
                    <pxa:PMFormulaEditor ID="edDescriptionFormula" runat="server" DataSourceID="ds" DataField="DescriptionFormula" Parameters="@Rate,@Price" />

                    <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" ControlSize="SM" />
                    <px:PXDropDown ID="edBranchSource" runat="server" DataField="BranchSource" />
                    <px:PXDropDown ID="edBranchSourceBudget" runat="server" DataField="BranchSourceBudget" />
                    <px:PXSegmentMask ID="edBranchID" runat="server" DataField="TargetBranchID" />

                    <px:PXLayoutRule ID="PXLayoutRule13" runat="server" Merge="True" ControlSize="SM" />
                    <px:PXDropDown ID="PXDropDown1" runat="server" DataField="AccountSource" CommitChanges="true"/>
                    <px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="AccountID" CommitChanges="true" />

                    <px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" ControlSize="SM" />
                    <px:PXSegmentMask ID="edSubMask" runat="server" DataField="SubMask" DataMember="_PMBILL_Segments_" />
                    <px:PXSegmentMask ID="edSubMaskBudget" runat="server" DataField="SubMaskBudget" DataMember="_PMBILLBUDGET_Segments_" />
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />

                    <px:PXLayoutRule ID="PXLayoutRule88" runat="server" StartRow="True" StartGroup="True" GroupCaption="Billing Options" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" Merge="True" LabelsWidth="M" />
                    <px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="CopyNotes" Size="L"/>
                    <px:PXCheckBox ID="PXCheckBox3" runat="server" DataField="IncludeZeroAmount" />
					<px:PXCheckBox ID="PXCheckBox4" runat="server" DataField="IncludeZeroAmountAndQty" />

                    <px:PXLayoutRule ID="PXLayoutRule22" runat="server" Merge="True" ControlSize="SM" />
                    <px:PXCheckBox ID="PXCheckBox5" runat="server" DataField="IncludeNonBillable" Size="L"/>
                    <px:PXLayoutRule ID="PXLayoutRule23" runat="server" Merge="False" />

                    <px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartRow="True" StartGroup="True" LabelsWidth="M" GroupCaption="Aggregate Transactions By" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
                    <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="GroupByDate" Size="S" />
                    <px:PXCheckBox ID="PXCheckBox6" runat="server" DataField="GroupByEmployee" Size="S" />
                    <px:PXCheckBox ID="PXCheckBox7" runat="server" DataField="GroupByVendor" Size="S" />
                    <px:PXCheckBox ID="PXCheckBox8" runat="server" DataField="GroupByItem" Size="S" />
                </Template>
                <CallbackCommands>
                    <Save RepaintControls="None" RepaintControlsIDs="formSettings" CommitChanges="False" />
                </CallbackCommands>
                <AutoSize Enabled="True" Container="Parent"></AutoSize>
            </px:PXFormView>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
