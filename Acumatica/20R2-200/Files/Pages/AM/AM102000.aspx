<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM102000.aspx.cs" Inherits="Page_AM102000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="ProdSetupRecord" TypeName="PX.Objects.AM.ProdSetup">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="ProdSetupRecord" Visible="False" DefaultControlID="edBatchNumberingID">
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="ProdSetupRecord">
		<Items>
			<px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
                    <px:PXSelector ID="edMoveNumberingID" runat="server" DataField="MoveNumberingID" AllowEdit="True"/>
                    <px:PXSelector ID="edLaborNumberingID" runat="server" DataField="LaborNumberingID" AllowEdit="True"/>
                    <px:PXSelector ID="edMaterialNumberingID" runat="server" DataField="MaterialNumberingID" AllowEdit="True"/>
                    <px:PXSelector ID="edWipAdjustNumberingID" runat="server" DataField="WipAdjustNumberingID" AllowEdit="True"/>
                    <px:PXSelector ID="edProdCostNumberingID" runat="server" DataField="ProdCostNumberingID" AllowEdit="True"/>
                    <px:PXSelector ID="edDisassemblyNumberingID" runat="server" DataField="DisassemblyNumberingID" AllowEdit="True"/>
                    <px:PXSelector ID="edVendorShipmentNumberingID" runat="server" DataField="VendorShipmentNumberingID" AllowEdit="True"/>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Scheduling" />
                    <px:PXMaskEdit ID="edDefaultMoveTime" runat="server" DataField="DefaultMoveTime" />
                    <px:PXCheckBox ID="chkFMLTMRPOrdorOP" runat="server" DataField="FMLTMRPOrdorOP" />
                    <px:PXCheckBox CommitChanges="True" ID="chkFMLTime" runat="server" DataField="FMLTime" />
                    <px:PXCheckBox ID="edMachineScheduling" runat="server" DataField="MachineScheduling" />
                    <px:PXCheckBox ID="edToolScheduling" runat="server" DataField="ToolScheduling" />
                    <px:PXSelector ID="edFixMfgCalendarID" runat="server" DataField="FixMfgCalendarID" AllowEdit="True"/>
                    <px:PXDropDown ID="edFMLTimeUnits" runat="server" DataField="FMLTimeUnits"/>
                    <px:PXDropDown ID="edSchdBlockSize" runat="server" DataField="SchdBlockSize" CommitChanges="True"/>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Data Entry Settings" />
                    <px:PXDropDown ID="edDfltLbrRate" runat="server" AllowNull="False" DataField="DfltLbrRate"  />
                    <px:PXSelector ID="edDefaultOrderType" runat="server" DataField="DefaultOrderType" AllowEdit="True" />
                    <px:PXSelector ID="edDefaultDisassembleOrderType" runat="server" DataField="DefaultDisassembleOrderType" AllowEdit="True" />
                    <px:PXCheckBox ID="edInclScrap" runat="server" DataField="InclScrap" />
                    <px:PXCheckBox ID="chkSummPost" runat="server" DataField="SummPost" />
                    <px:PXCheckBox ID="edHoldEntry" runat="server"  DataField="HoldEntry" />
					<px:PXCheckBox ID="edRequireControlTotal" runat="server" DataField="RequireControlTotal" />
                    <px:PXCheckBox ID="edDefaultEmployee" runat="server" DataField="DefaultEmployee" />
                    <px:PXCheckBox ID="edRestrictClock" runat="server" DataField="RestrictClockCurrentUser" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Vendor Shipment Settings" />
                    <px:PXCheckBox ID="edHoldShipmentsOnEntry" runat="server" DataField="HoldShipmentsOnEntry" />
                    <px:PXCheckBox ID="edValidateShipmentTotalOnConfirm" runat="server" DataField="ValidateShipmentTotalOnConfirm" />
                    <px:PXLayoutRule runat="server" StartGroup="True" StartColumn="True" GroupCaption="Manufacturing Operations Settings" />
					<px:PXFormView ID="formScanSetup" runat="server" DataSourceID="ds" Width="100%" DataMember="ScanSetup" DefaultControlID="edUseDefaultQtyInMaterials" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartGroup="True" SuppressLabel="True" />
							<px:PXLabel ID="lblScanSetup" runat="server">These settings are specific to the current branch.</px:PXLabel>
							<px:PXCheckBox ID="edUseDefaultQtyInMaterials" runat="server" DataField="UseDefaultQtyInMaterials" CommitChanges="true" />
                            <px:PXCheckBox ID="edUseDefaultQtyInMove" runat="server" DataField="UseDefaultQtyInMove" CommitChanges="true" />
                            <px:PXCheckBox ID="edUseRemainingQtyInMaterials" runat="server" DataField="UseRemainingQtyInMaterials" CommitChanges="true" />
                            <px:PXCheckBox ID="edUseRemainingQtyInMove" runat="server" DataField="UseRemainingQtyInMove" CommitChanges="true" />
							<px:PXCheckBox ID="edUseDefaultOrderType" runat="server" DataField="UseDefaultOrderType" CommitChanges="true" />
							<px:PXCheckBox ID="edRequestLocationForEachItemInMaterials" runat="server" DataField="RequestLocationForEachItemInMaterials" CommitChanges="true" />
                            <px:PXCheckBox ID="edRequestLocationForEachItemInMove" runat="server" DataField="RequestLocationForEachItemInMove" CommitChanges="true" />
							<px:PXCheckBox ID="edExplicitLineConfirmation" runat="server" DataField="ExplicitLineConfirmation" />
							<px:PXCheckBox ID="edDefaultWarehouse" runat="server" DataField="DefaultWarehouse" CommitChanges="true" />
							<px:PXCheckBox ID="edDefaultLotSerialNumber" runat="server" DataField="DefaultLotSerialNumber" CommitChanges="true" />
							<px:PXCheckBox ID="edDefaultExpireDate" runat="server" DataField="DefaultExpireDate" CommitChanges="true" />
						</Template>
					</px:PXFormView>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>
