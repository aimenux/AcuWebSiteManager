<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM401200.aspx.cs" Inherits="Page_AM401200" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="BucketLookup" TypeName="PX.Objects.AM.MRPBucketInq">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Cancel" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="BucketLookup"> 
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edBucketID" runat="server" DataField="BucketID" AutoRefresh="True" DataSourceID="ds" AllowEdit="True" CommitChanges="True" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" CommitChanges="True" />
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" DisplayMode="Hint" CommitChanges="True" AllowEdit="True" />
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXNumberEdit ID="edQtyOnHand" runat="server" DataField="QtyOnHand" Width="200px" />
            <px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID" />
            <px:PXNumberEdit ID="edSafetyStock" runat="server" DataField="SafetyStock" Width="200px" />
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown ID="edReplenishmentSource" Size="m" runat="server" DataField="ReplenishmentSource"/>
            <px:PXSegmentMask ID="edPreferredVendorID" runat="server" DataField="PreferredVendorID" AllowEdit="True" />
            <px:PXNumberEdit ID="edLeadTime" runat="server" DataField="LeadTime" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" SyncPosition="True" TabIndex="300" >
		<Levels>
			<px:PXGridLevel DataMember="BucketDetailRecords" >
                <RowTemplate>
                    <px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" DisplayFormat="d" />

                    <px:PXDateTimeEdit ID="edToDate" runat="server" DataField="ToDate" DisplayFormat="d" />
                    <px:PXNumberEdit ID="edBeginQty" runat="server" DataField="BeginQty" />
                    <px:PXNumberEdit ID="edBucket" runat="server" DataField="Bucket" />
                    <px:PXNumberEdit ID="edActualSupply" runat="server" DataField="ActualSupply" />
                    <px:PXNumberEdit ID="edActualDemand" runat="server" DataField="ActualDemand" />
                    <px:PXNumberEdit ID="edNetQty" runat="server" DataField="NetQty" />
                    <px:PXNumberEdit ID="edPlannedSupply" runat="server" DataField="PlannedSupply" />
                    <px:PXNumberEdit ID="edPlannedDemand" runat="server" DataField="PlannedDemand" />
                    <px:PXNumberEdit ID="edEndQty" runat="server" DataField="EndQty" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="FromDate" Width="108px" />
                    <px:PXGridColumn DataField="Bucket" Width="150px" />
                    <px:PXGridColumn DataField="ToDate" Width="108px" />
                    <px:PXGridColumn DataField="BeginQty" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="ActualSupply" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="ActualDemand" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="NetQty" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="PlannedSupply" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="PlannedDemand" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="EndQty" Width="108px" TextAlign="Right" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
