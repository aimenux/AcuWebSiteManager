<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN207000.aspx.cs"
    Inherits="Page_IN207000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INLotSerClassMaint" PrimaryView="lotserclass">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="lotserclass" Caption="Lot/Serial Settings Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edLotSerClassID"
        DataKeyNames="LotSerClassID" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edLotSerClassID" runat="server" DataField="LotSerClassID" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXDropDown CommitChanges="True" ID="edLotSerTrack" runat="server" AllowNull="False" DataField="LotSerTrack" />
            <px:PXCheckBox ID="chkLotSerTrackExpiration" runat="server" DataField="LotSerTrackExpiration" />
            <px:PXCheckBox ID="chkRequiredForDropship" runat="server" DataField="RequiredForDropship" />
            <px:PXDropDown ID="edLotSerAssign" runat="server" AllowNull="False" DataField="LotSerAssign" CommitChanges="true" />
            <px:PXDropDown ID="edLotSerIssueMethod" runat="server" AllowNull="False" DataField="LotSerIssueMethod" CommitChanges="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkLotSerNumShared" runat="server" DataField="LotSerNumShared" />
            <px:PXMaskEdit ID="edLotSerNumVal" runat="server" DataField="LotSerNumVal" CommitChanges="true" />
            <px:PXCheckBox ID="chkAutoNextNbr" runat="server" DataField="AutoNextNbr" />
            <px:PXNumberEdit ID="edAutoSerialMaxCount" runat="server" DataField="AutoSerialMaxCount" />
            
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Numbering Settings"
        TabIndex="100" SkinID="Details">
        <Mode InitNewRow="True" />
        <Levels>
            <px:PXGridLevel DataMember="lotsersegments">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXNumberEdit ID="edSegmentID" runat="server" DataField="SegmentID" Enabled="False" />
                    <px:PXTextEdit ID="edSegmentValue" runat="server" DataField="SegmentValue" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowUpdate="False" DataField="SegmentID" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn AllowNull="False" DataField="SegmentType" Type="DropDownList" AutoCallBack="True" Width="99px" />
                    <px:PXGridColumn DataField="SegmentValue" Width="180px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
