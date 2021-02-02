<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM408000.aspx.cs" Inherits="Page_AM408000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AM.CostRollHistory"
        BorderStyle="NotSet" PrimaryView="CostRollHistoryRecords" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewBOM" Visible="False" DependOnGrid="grid" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True"  AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Inquire" AllowFilter ="true">
	    <Levels>
			<px:PXGridLevel DataMember="CostRollHistoryRecords" DataKeyNames="BOMID, RevisionID, StartDate">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXTextEdit ID="edBOMID" runat="server" DataField="BOMID" />
                    <px:PXTextEdit ID="RevisionID" runat="server" DataField="RevisionID" />
                    <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" />
                    <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
			        <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost"/>
                    <px:PXNumberEdit ID="edMatlManufacturedCost" runat="server" DataField="MatlManufacturedCost"/>
                    <px:PXNumberEdit ID="edMatlNonManufacturedCost" runat="server" DataField="MatlNonManufacturedCost"/>
			        <px:PXNumberEdit ID="edMatlCost" runat="server" DataField="MatlCost"/>
			        <px:PXNumberEdit ID="edFLaborCost" runat="server" DataField="FLaborCost"/>
                    <px:PXNumberEdit ID="edVLaborCost" runat="server" DataField="VLaborCost"/>
                    <px:PXNumberEdit ID="edFOvdCost" runat="server" DataField="FOvdCost"/>
			        <px:PXNumberEdit ID="edVOvdCost" runat="server" DataField="VOvdCost"/>
                    <px:PXNumberEdit ID="edToolCost" runat="server" DataField="ToolCost"/>
			        <px:PXNumberEdit ID="edMachCost" runat="server" DataField="MachCost"/>
                    <px:PXNumberEdit ID="edSubcontractMaterialCost" runat="server" DataField="SubcontractMaterialCost"/>
                    <px:PXNumberEdit ID="edReferenceMaterialCost" runat="server" DataField="ReferenceMaterialCost"/>
                    <px:PXNumberEdit ID="edLotSize" runat="server" DataField="LotSize"/>
			        <px:PXDropDown ID="edStatus" runat="server" DataField="AMBomItem__Status" />
			        <px:PXTextEdit ID="edBomRevDescr" runat="server" DataField="AMBomItem__Descr" />
                    <px:PXCheckBox ID="edMultiLevelProcess" runat="server" DataField="MultiLevelProcess" />
                    <px:PXNumberEdit ID="edLevel" runat="server" DataField="Level"/>
                    <px:PXCheckBox ID="edIsDefaultBom" runat="server" DataField="IsDefaultBom" />
                    <px:PXMaskEdit ID="edFixedLaborTime" runat="server" DataField="FixedLaborTime" Width="108px" />
                    <px:PXMaskEdit ID="edVariableLaborTime" runat="server" DataField="VariableLaborTime" Width="108px" />
                    <px:PXMaskEdit ID="edMachineTime" runat="server" DataField="MachineTime" Width="108px" />
                    <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
                    <px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost" Enabled="False" />
                    <px:PXNumberEdit ID="edPendingStdCost" runat="server" DataField="PendingStdCost" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="BOMID" Width="130px" LinkCommand="ViewBOM"/>
                    <px:PXGridColumn DataField="RevisionID" Width="99px" />
                    <px:PXGridColumn DataField="CreatedDateTime" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="LastModifiedDateTime" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="InventoryID"  Width="130px"/>
                    <px:PXGridColumn DataField="SubItemID"  Width="81px"/>
                    <px:PXGridColumn DataField="SiteID" Width="130px"/>
		            <px:PXGridColumn DataField="UnitCost" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="MatlManufacturedCost" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="MatlNonManufacturedCost" Width="108px" TextAlign="Right"/>
		            <px:PXGridColumn DataField="MatlCost" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="FLaborCost" TextAlign="Right" Width="108px"/>
		            <px:PXGridColumn DataField="VLaborCost" TextAlign="Right" Width="108px"/>
		            <px:PXGridColumn DataField="FOvdCost" TextAlign="Right" Width="108px"/>
                    <px:PXGridColumn DataField="VOvdCost" TextAlign="Right" Width="108px"/>
                    <px:PXGridColumn DataField="ToolCost" TextAlign="Right" Width="108px"/>
		            <px:PXGridColumn DataField="MachCost" TextAlign="Right" Width="108px"/>
                    <px:PXGridColumn DataField="SubcontractMaterialCost" TextAlign="Right" Width="120px"/>
                    <px:PXGridColumn DataField="ReferenceMaterialCost" TextAlign="Right" Width="120px"/>
                    <px:PXGridColumn DataField="LotSize" TextAlign="Right" Width="108px"/>
                    <px:PXGridColumn DataField="AMBomItem__Status"/>
		            <px:PXGridColumn DataField="AMBomItem__Descr" Width="175px"/>
                    <px:PXGridColumn DataField="MultiLevelProcess" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="Level" Width="75px"/>
                    <px:PXGridColumn DataField="IsDefaultBom" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="FixedLaborTime" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="VariableLaborTime" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="MachineTime" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ItemClassID" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="StdCost" Width="108px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="PendingStdCost" Width="108px" TextAlign="Right"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar ActionsText="False" >
        </ActionBar>
	</px:PXGrid>
</asp:Content>
