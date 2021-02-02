<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM413000.aspx.cs" Inherits="Page_AM413000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.AM.MultiLevelBomInq" PrimaryView="Filter" >
        <CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="CostedReport" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" Caption="Selection" TabIndex="2100">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector ID="edBOMID" runat="server" AutoRefresh="True" DataField="BOMID" DataSourceID="ds" CommitChanges="True" >
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector> 
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" CommitChanges="True"  />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" CommitChanges="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edBOMDate" runat="server" DataField="BOMDate"/>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox ID="chkIgnoreReplenishmentSettings" runat="server" DataField="IgnoreReplenishmentSettings" CommitChanges="True" />
            <px:PXCheckBox ID="chkIncludeBomsOnHold" runat="server" DataField="IncludeBomsOnHold" CommitChanges="True" />
            <px:PXCheckBox ID="chkRollCosts" runat="server" DataField="RollCosts" CommitChanges="True" />
            <px:PXCheckBox ID="chkIgnoreMinMaxLotSizeValues" runat="server" DataField="IgnoreMinMaxLotSizeValues" Style="margin-left: 25px" CommitChanges="True" />
            <px:PXCheckBox ID="chkUseCurrentInventoryCost" runat="server" DataField="UseCurrentInventoryCost" CommitChanges="True" />
        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXFormView>
</asp:Content>

