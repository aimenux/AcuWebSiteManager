<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX505000.aspx.cs" Inherits="Page_TX505000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Items" TypeName="PX.Objects.TX.TaxImport" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewData" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowSearch="true" DataSourceID="ds" SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="StateCode" />
                    <px:PXGridColumn AllowUpdate="False" DataField="StateName" Label="State Name" Width="200px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID"  Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubID" Width="108px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton Text="View Data" Key="cmdViewData">
                    <AutoCallBack Command="ViewData" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
