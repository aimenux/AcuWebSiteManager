<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM208500.aspx.cs" Inherits="Page_AM208500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
        <px:pxdatasource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="Documents" TypeName="PX.Objects.AM.BOMAttributeMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false"  />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false" />
		</CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:pxformview ID="form" runat="server" DataSourceID="ds" DefaultControlID="edBOMID" Width="100%" DataMember="Documents">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edBOMID" runat="server" DataField="BOMID" CommitChanges="True" FilterByAllFields="True" AllowEdit="True" />
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" CommitChanges="True" AutoRefresh="True" AllowEdit="True" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" Width="50px" CommitChanges="True" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" LabelsWidth="S" ControlSize="L"/>
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" CommitChanges="True" />
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="true" LabelsWidth="S" ControlSize="SM"/>
            <px:PXDateTimeEdit ID="edEffStartDate" runat="server" DataField="EffStartDate" />
            <px:PXDateTimeEdit ID="edEffEndDate" runat="server" DataField="EffEndDate" />
        </Template>
	</px:pxformview>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="Details" AutoAdjustColumns="True" SyncPosition="true"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="true" Width="100%" >
        <Levels>
            <px:PXGridLevel DataKeyNames="BOMID,RevisionID,LineNbr" DataMember="BomAttributes">
                <RowTemplate>
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" CommitChanges="True" AutoRefresh="True" FilterByAllFields="True"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="LineNbr"/>
                    <px:PXGridColumn DataField="Level" TextAlign="Left" Width="80px" />
                    <px:PXGridColumn DataField="AttributeID" Width="120px" CommitChanges="true" />
                    <px:PXGridColumn DataField="OperationID" Width="75px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="Label" Width="120px" CommitChanges="true" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                    <px:PXGridColumn DataField="Enabled" TextAlign="Center" Type="CheckBox" Width="75px" />
                    <px:PXGridColumn DataField="TransactionRequired" TextAlign="Center" Type="CheckBox" Width="85px" />
                    <px:PXGridColumn DataField="Value" Width="220px" MatrixMode="True" />
                    <px:PXGridColumn DataField="OrderFunction" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" Container="Window" />
        <mode AllowUpload="True" />
    </px:PXGrid>
</asp:Content>

