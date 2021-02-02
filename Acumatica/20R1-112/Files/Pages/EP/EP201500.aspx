<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP201500.aspx.cs"
    Inherits="Page_EP201500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.EP.DepartmentMaint" PrimaryView="EPDepartment" Visible="True" >
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" Visible="False" />
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Delete" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AutoAdjustColumns="False" AllowSearch="True" DataSourceID="ds" SkinID="Primary" AdjustPageSize="Auto" TabIndex="-18236">
        <Levels>
            <px:PXGridLevel DataMember="EPDepartment">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXMaskEdit ID="edDepartmentID" runat="server" DataField="DepartmentID" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXSegmentMask ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="DepartmentID" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="ExpenseAccountID" AutoCallBack="true" />
                    <px:PXGridColumn DataField="ExpenseSubID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXGrid>
</asp:Content>
