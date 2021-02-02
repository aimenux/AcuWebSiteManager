<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO202000.aspx.cs"
    Inherits="Page_PO202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="LandedCostCode" TypeName="PX.Objects.PO.LandedCostCodeMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="LandedCostCode" Caption="Landed Cost Code"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edLandedCostCodeID">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector runat="server" DataField="LandedCostCodeID" ID="edLandedCostCodeID" AutoRefresh="true" />
            <px:PXTextEdit runat="server" DataField="Descr" ID="edDescr" />
            <px:PXDropDown runat="server" SelectedIndex="2" DataField="LCType" AllowNull="False" ID="edLCType" />
            <px:PXDropDown runat="server" DataField="AllocationMethod" AllowNull="False" ID="edAllocationMethod" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="VendorID" ID="edVendorID" AllowEdit="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID"
                AllowEdit="True">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSelector runat="server" DataField="TermsID" ID="edTermsID" AllowEdit="True" />
            <px:PXSelector runat="server" DataField="ReasonCode" ID="edReasonCode" AllowEdit="True" />
            <px:PXSegmentMask runat="server" DataField="LCAccrualAcct" ID="edLCAccrualAcct" CommitChanges="true" />
            <px:PXSegmentMask runat="server" DataField="LCAccrualSub" ID="edLCAccrualSub" AutoRefresh="true">
                <Parameters>
                    <px:PXControlParam ControlID="form" Name="LandedCostCode.lCAccrualAcct" PropertyName="DataControls[&quot;edLCAccrualAcct&quot;].Value"
                        Type="String" />
                </Parameters>
            </px:PXSegmentMask>
            <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" AutoRefresh="True"/>
            <px:PXSegmentMask runat="server" DataField="LCVarianceAcct" ID="edLCVarianceAcct" CommitChanges="true" />
            <px:PXSegmentMask runat="server" DataField="LCVarianceSub" ID="edLCVarianceSub" AutoRefresh="true">
                <Parameters>
                    <px:PXControlParam ControlID="form" Name="LandedCostCode.lCVarianceAcct" PropertyName="DataControls[&quot;edLCVarianceAcct&quot;].Value"
                        Type="String" />
                </Parameters>
            </px:PXSegmentMask>
        </Template>
    </px:PXFormView>
</asp:Content>
