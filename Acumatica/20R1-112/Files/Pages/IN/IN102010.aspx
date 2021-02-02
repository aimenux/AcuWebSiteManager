<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN102010.aspx.cs"
    Inherits="Page_IN102010" Title="Warehouse Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INAccessDetail" PrimaryView="Site">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="SaveSite" />
            <px:PXDSCallbackCommand Name="FirstSite" StartNewGroup="True" HideText="True"/>
            <px:PXDSCallbackCommand Name="PrevSite" HideText="True"/>
            <px:PXDSCallbackCommand Name="NextSite" HideText="True"/>
            <px:PXDSCallbackCommand Name="LastSite" HideText="True"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Warehouse" DataMember="Site"
        DefaultControlID="edSiteCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask ID="edSiteCD" runat="server" DataField="SiteCD" AutoGenerateColumns="true">
                <AutoCallBack Command="CancelSite" Target="ds" />
            </px:PXSegmentMask>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Enabled="False" /></Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True"
        AdjustPageSize="Auto" Caption="Restriction Groups" AllowSearch="True" SkinID="Details">
        <Levels>
            <px:PXGridLevel DataMember="Groups">
                <Mode AllowAddNew="False" AllowDelete="False" />
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px" RenderEditorText="True"
                        AllowCheckAll="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="GroupName" Width="150px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Description" Width="200px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="40px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="GroupType" Label="Visible To Entities" RenderEditorText="True"
                        Width="171px" />
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
                    <px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
                    <px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType" Enabled="False" />
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" />
        <Mode AllowAddNew="False" AllowDelete="False" />
        <ActionBar>
            <Actions>
                <Delete Enabled="False" />
                <AddNew Enabled="False" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
