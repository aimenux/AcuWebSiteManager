<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="FS204800.aspx.cs" Inherits="Page_FS204800" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="ManufacturerModelRecords" 
        TypeName="PX.Objects.FS.ManufacturerModelMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Delete" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="130px" 
        Style="z-index: 100" Width="100%" DataMember="ManufacturerModelRecords" 
        TabIndex="11600" DefaultControlID="edManufacturerID">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXSelector ID="edManufacturerID" runat="server" AutoRefresh="True" 
                DataField="ManufacturerID" CommitChanges="True" AllowEdit="true">
            </px:PXSelector>
            <px:PXSelector ID="edManufacturerModelCD" runat="server" AutoRefresh="True" 
                DataField="ManufacturerModelCD" CommitChanges="True">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Size="XL">
            </px:PXTextEdit>
            <px:PXSelector ID="edEquipmentTypeID" runat="server" 
                DataField="EquipmentTypeID" AllowEdit="true">
            </px:PXSelector>
        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXFormView>
</asp:Content>

