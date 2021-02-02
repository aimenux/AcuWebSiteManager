<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="IN203500.aspx.cs" Inherits="Page_IN203500"
    Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Header" TypeName="PX.Objects.IN.Matrix.Graphs.CreateMatrixItems">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="CreateMatrixItems" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CreateUpdate" CommitChanges="true" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Header">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXSelector runat="server" DataField="TemplateItemID" Size="M" CommitChanges="True" ID="edTemplateItemID" AllowEdit="True" edit="1">
            </px:PXSelector>
            <px:PXSelector runat="server" DataField="ColAttributeID" Size="M" CommitChanges="True" ID="edColAttributeID" AutoRefresh="True">
            </px:PXSelector>
            <px:PXSelector runat="server" DataField="RowAttributeID" Size="M" CommitChanges="True" ID="edRowAttributeID" AutoRefresh="True">
            </px:PXSelector>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <!--#include file="~\Pages\Includes\InventoryMatrixCreateItems.inc"-->
</asp:Content>