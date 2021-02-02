<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM201000.aspx.cs" Inherits="Page_AM201000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="AMMPSRecords" TypeName="PX.Objects.AM.MPSMaint" BorderStyle="NotSet">
		<CallbackCommands>		
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SyncPosition="True" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="AMMPSRecords" DataKeyNames="MPSTypeID, MPSID">
                <RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
					<px:PXDateTimeEdit ID="edPlanDate" runat="server" DataField="PlanDate" />
                    <px:PXSelector ID="edMPSTypeID" runat="server" DataField="MPSTypeID" AutoCallBack="True" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXSelector ID="edBOMID" runat="server" DataField="BOMID" AutoRefresh="True" AllowEdit="True"/>
                    <px:PXCheckBox ID="chkActiveFlg" runat="server" DataField="ActiveFlg" />
                    <px:PXMaskEdit ID="edMPSID" runat="server" DataField="MPSID"/>
                    <px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" />
                    <px:PXTextEdit ID="edBOMID_description" runat="server" DataField="BOMID_description" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True"/>
                    <px:PXGridColumn DataField="SubItemID" CommitChanges="True" />
                    <px:PXGridColumn DataField="PlanDate" Width="90px" />
                    <px:PXGridColumn DataField="MPSTypeID" Width="125" />
                    <px:PXGridColumn DataField="SiteID" Width="125px" CommitChanges="True" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="110px" />
                    <px:PXGridColumn DataField="UOM"  Width="85px" />
                    <px:PXGridColumn DataField="BOMID" Width="125px" />
                    <px:PXGridColumn DataField="ActiveFlg"  TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="MPSID"/>
                    <px:PXGridColumn DataField="InventoryID_description" Width="280px"/>
                    <px:PXGridColumn DataField="BOMID_description" Width="280px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" />
	    <ActionBar ActionsText="False"/>
        <Mode InitNewRow="True" AllowUpload="True"/>
	</px:PXGrid>
</asp:Content>
