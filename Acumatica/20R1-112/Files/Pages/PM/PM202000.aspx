<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM202000.aspx.cs"
    Inherits="Page_PM202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ProjectAttributeGroupMaint" PrimaryView="Filter"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Attribute Group">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown CommitChanges="True" ID="edClassID" runat="server"  DataField="ClassID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        SkinID="Details" Caption="Attributes">
        <Levels>
            <px:PXGridLevel DataMember="Mapping">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector ID="edCRAttributeID" runat="server" DataField="AttributeID" AutoRefresh="true" AllowEdit="true" FilterByAllFields="True" />
                    <px:PXTextEdit ID="edDescription" runat="server"  DataField="Description" />
                    <px:PXCheckBox ID="chkRequired" runat="server" DataField="Required" />
                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
                    <px:PXDropDown ID="edControlType" runat="server"  DataField="ControlType" />
                    <px:PXDropDown ID="edType" runat="server"  DataField="Type" />
                </RowTemplate>
                <Columns>
					<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="AttributeID" AutoCallBack="True" />
                    <px:PXGridColumn  DataField="Description" />
                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                    <px:PXGridColumn  DataField="Required" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn  DataField="ControlType" Type="DropDownList" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
