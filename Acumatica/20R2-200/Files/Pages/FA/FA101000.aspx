<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA101000.aspx.cs"
    Inherits="Page_FA101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="FASetupRecord" TypeName="PX.Objects.FA.SetupMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="FASetupRecord" Caption="General Settings"
        AllowCollapse="False" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Account Settings" />
            <px:PXSegmentMask CommitChanges="True" ID="edFAAccrualAcctID" runat="server" DataField="FAAccrualAcctID" />
            <px:PXSegmentMask ID="edFAAccrualSubID" runat="server" DataField="FAAccrualSubID">
                <Parameters>
                    <px:PXControlParam ControlID="form" Name="FASetup.fAAccrualAcctID" PropertyName="DataControls[&quot;edFAAccrualAcctID&quot;].Value" />
                </Parameters>
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edProceedsAcctID" runat="server" DataField="ProceedsAcctID" CommitChanges="true" />
            <px:PXSegmentMask ID="edProceedsSubID" runat="server" DataField="ProceedsSubID" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Other" />
            <px:PXDropDown ID="edDeprHistoryView" runat="server" AllowNull="False" DataField="DeprHistoryView" SelectedIndex="1" />
            <px:PXCheckBox ID="chkDepreciateInDisposalPeriod" runat="server" DataField="DepreciateInDisposalPeriod" />
            <px:PXCheckBox ID="chkAccurateDepreciation" runat="server" DataField="AccurateDepreciation" />
            <px:PXCheckBox ID="chkReconcileBeforeDisposal" runat="server" DataField="ReconcileBeforeDisposal" />
            <px:PXCheckBox ID="chkAllowEditPredefinedDeprMethod" runat="server" DataField="AllowEditPredefinedDeprMethod" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
            <px:PXSelector ID="edRegisterNumberingID" runat="server" AllowNull="False" DataField="RegisterNumberingID" Text="FAREGISTER"
                AllowEdit="True" AutoRefresh="True" />
            <px:PXSelector ID="edAssetNumberingID" runat="server" AllowNull="False" DataField="AssetNumberingID" Text="FASSET" AllowEdit="True"
                AutoRefresh="True" />
            <px:PXSelector ID="edBatchNumberingID" runat="server" AllowEdit="True" AllowNull="False" DataField="BatchNumberingID" Text="BATCH" />
            <px:PXSelector ID="edTagNumberingID" runat="server" DataField="TagNumberingID" AllowEdit="True" />
            <px:PXCheckBox ID="chkCopyTagFromAssetID" runat="server" DataField="CopyTagFromAssetID" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Posting Settings" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoReleaseAsset" runat="server" DataField="AutoReleaseAsset" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoReleaseDepr" runat="server" DataField="AutoReleaseDepr" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoReleaseDisp" runat="server" DataField="AutoReleaseDisp" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoReleaseTransfer" runat="server" DataField="AutoReleaseTransfer" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoReleaseReversal" runat="server" DataField="AutoReleaseReversal" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoReleaseSplit" runat="server" DataField="AutoReleaseSplit" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkUpdateGL" runat="server" DataField="UpdateGL" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkAutoPost" runat="server" DataField="AutoPost" />
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkSummPost" runat="server" DataField="SummPost" CommitChanges="True"/>
            <px:PXCheckBox AlignLeft="True" SuppressLabel="True" ID="chkSummPostDepreciation" runat="server" DataField="SummPostDepreciation" />
        </Template>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXFormView>
</asp:Content>
