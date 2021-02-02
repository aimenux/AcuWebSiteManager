<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM203535.aspx.cs" Inherits="Page_SM203535" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SmsProvider.SM.SmsPluginMaint" PrimaryView="Plugin" PageLoadBehavior="GoLastRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="SendTestMessage" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="pnlSendTestMessageDialogBox" runat="server"
        Style="z-index: 108;" Caption="Send Test Message" CaptionVisible="True"
        Key="SendTestMessageDialogView" LoadOnDemand="true" AutoCallBack-Command="Refresh"
        AutoCallBack-Target="formSendTestMessageDialogBox" AutoCallBack-Enabled="true"
        AcceptButtonID="cbOk" CancelButtonID="cbCancel">
        <px:PXFormView ID="formSendTestMessageDialogBox" runat="server" DataSourceID="ds" DataMember="SendTestMessageDialogView" SkinID="Transparent">
            <ContentStyle BorderWidth="0px"></ContentStyle>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit ID="to" runat="server" DataField="TO" CommitChanges="True"/>
                <px:PXTextEdit ID="body" runat="server" DataField="Body" CommitChanges="True" TextMode="MultiLine" Height="40" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="cbOK" runat="server" Text="OK" CommandSourceID="ds" DialogResult="OK" />
            <px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Voice Plug-in Summary" DataMember="Plugin" TabIndex="5100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XL" />
            <px:PXSelector runat="server" ID="edName" DataField="Name" DataSourceID="ds" AutoRefresh="True" />
            <px:PXSelector ID="edPluginTypeName" runat="server" DataField="PluginTypeName" DataSourceID="ds" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" SuppressLabel="true" />
            <px:PXCheckBox ID="chkIsDefault" runat="server" DataField="IsDefault" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="365px">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Parameters">
                <Template>
                    <px:PXGrid ID="PXGridSettings" runat="server" DataSourceID="ds" AllowFilter="False" Width="100%" SkinID="DetailsInTab"
                        Height="100%" MatrixMode="True" AutoAdjustColumns="True" OnRowDataBound="grid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="Details">
                                <Columns>
                                    <px:PXGridColumn DataField="Name" Width="90px" />
                                    <px:PXGridColumn DataField="Description" Width="300px" AllowNull="False" />
                                    <px:PXGridColumn DataField="Value" Width="300px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
</asp:Content>
