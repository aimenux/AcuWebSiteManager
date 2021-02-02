<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO101000.aspx.cs"
    Inherits="Page_SO101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOSetupMaint" PrimaryView="sosetup">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="500px" Style="z-index: 100" Width="100%" DataMember="sosetup"
        DefaultControlID="edDefaultOrderType">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXPanel ID="pnlDataEntrySettings" runat="server" Caption="Data Entry Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXSelector ID="edDefaultOrderType" runat="server"
                            DataField="DefaultOrderType" DataSourceID="ds" />
                        <px:PXSelector ID="edTransferOrderType" runat="server"
                            DataField="TransferOrderType" DataSourceID="ds" />
                        <px:PXSelector ID="edShipmentNumberingID" runat="server" AllowNull="False"
                            DataField="ShipmentNumberingID" Text="SOSHIPMENT"
                            AllowEdit="True" DataSourceID="ds" edit="1" />
                        <px:PXSelector ID="edWorksheetNumberingID" runat="server" DataField="PickingWorksheetNumberingID" AllowNull="False" DataSourceID="ds" AllowEdit="True" />
                        <px:PXCheckBox ID="chkAdvancedAvailCheck" runat="server" DataField="AdvancedAvailCheck" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlPriceSettings" runat="server" Caption="Price Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXDropDown ID="edMinGrossProfitValidation" runat="server" AllowNull="False" DataField="MinGrossProfitValidation" SelectedIndex="1" />
                        <px:PXCheckBox ID="edUsePriceAdjustmentMultiplier" runat="server" DataField="UsePriceAdjustmentMultiplier" />
                        <px:PXLabel ID="lblIgnoreMinGrossProfitOptions" runat="server">Ignore Min. Markup Validation for Prices Specific To</px:PXLabel>
                        <px:PXCheckBox ID="edIgnoreMinGrossProfitCustomerPrice" runat="server" DataField="IgnoreMinGrossProfitCustomerPrice" />
                        <px:PXCheckBox ID="edIgnoreMinGrossProfitCustomerPriceClass" runat="server" DataField="IgnoreMinGrossProfitCustomerPriceClass" />
                        <px:PXCheckBox ID="edIgnoreMinGrossProfitPromotionalPrice" runat="server" DataField="IgnoreMinGrossProfitPromotionalPrice" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlFreightCalc" runat="server" Caption="Freight Calculation Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXDropDown ID="edFreightAllocation" runat="server" AllowNull="False" DataField="FreightAllocation" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlShipmentSettings" runat="server" Caption="Shipment Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXDropDown ID="edFreeItemShipping" runat="server" AllowNull="False" DataField="FreeItemShipping" SelectedIndex="-1" />
                        <px:PXCheckBox ID="chkHoldShipments" runat="server" Checked="True" DataField="HoldShipments" />
                        <px:PXCheckBox ID="chkRequireShipmentTotal" runat="server" Checked="True" DataField="RequireShipmentTotal" />
                        <px:PXCheckBox ID="chkAddAllToShipment" runat="server" DataField="AddAllToShipment" CommitChanges="true" />
                        <px:PXCheckBox ID="chkCreateZeroShipments" runat="server" DataField="CreateZeroShipments" CommitChanges="true" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlInvoiceSettings" runat="server" Caption="Invoice Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXCheckBox ID="chkCreditCheckError" runat="server" DataField="CreditCheckError" />
                        <px:PXCheckBox ID="chkUseShipDateForInvoiceDate" runat="server" DataField="UseShipDateForInvoiceDate" />
                    </px:PXPanel>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXPanel ID="pnlPostingSettings" runat="server" Caption="Posting Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXCheckBox ID="chkAutoReleaseIN" runat="server" DataField="AutoReleaseIN" />
                        <px:PXCheckBox ID="chkUseShippedNotInvoiced" runat="server" DataField="UseShippedNotInvoiced" CommitChanges="true" />
                        <px:PXSegmentMask ID="edShippedNotInvoicedAcctID" runat="server" DataField="ShippedNotInvoicedAcctID" AutoRefresh="True" CommitChanges="true" />
                        <px:PXSegmentMask ID="edShippedNotInvoicedSubID" runat="server" DataField="ShippedNotInvoicedSubID" AutoRefresh="True" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlSalesProfitability" runat="server" Caption="Sales Profitability Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXDropDown ID="edSalesProfitabilityForNSKits" runat="server" AllowNull="False" DataField="SalesProfitabilityForNSKits" />
                    </px:PXPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval">				
				<Template>
				    <px:PXPanel ID="PXPanel1" runat="server" DataMember="">
				        <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" />
				        <px:PXCheckBox ID="chkOrderRequestApproval" runat="server" AlignLeft="True" Checked="True" DataField="OrderRequestApproval" CommitChanges="True" />				        
                    </px:PXPanel>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" >
                        <AutoSize Enabled="True"/>
					    <Levels>
						    <px:PXGridLevel DataMember="SetupApproval">
							    <RowTemplate>
								    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
								    <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" CommitChanges="True"  />
								    <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" edit="1" CommitChanges="True" />
                                    <px:PXSelector ID="edAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AllowEdit="True" />
                                </RowTemplate>
							    <Columns>
								    <px:PXGridColumn DataField="OrderType" RenderEditorText="True" Width="100px" />
								    <px:PXGridColumn DataField="AssignmentMapID" Width="250px" RenderEditorText="True" TextField="AssignmentMapID_EPAssignmentMap_Name" />
                                    <px:PXGridColumn DataField="AssignmentNotificationID" Width="250px" RenderEditorText="True" />
							    </Columns>
						    </px:PXGridLevel>
					    </Levels>                        
					</px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Reporting Settings">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" 
                        SkinID="Horizontal" Height="500px" SavePosition="True">
                        <AutoSize Enabled="True" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
                                AdjustPageSize="Auto" AllowPaging="True">
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="Notifications" DataKeyNames="Module,NotificationCD">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXSelector ID="edNBranchID" runat="server" DataField="NBranchID" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edDefPrinterID" runat="server" DataField="DefaultPrinterID" />
                                            <px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="NotificationCD" Width="120px" />
                                            <px:PXGridColumn DataField="NBranchID" Width="120px" />
                                            <px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="DefaultPrinterID" Width="120px" />
                                            <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="ShipVia" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="Details" DataSourceID="ds" Width="100%" Caption="Default Recipients" CaptionVisible="true">
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="gridNS" />
                                </Parameters>
                                <CallbackCommands>
                                    <Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="Recipients" DataKeyNames="RecipientID">
                                        <Columns>
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
                                            <px:PXGridColumn DataField="ContactID" Width="120px">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                            <px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName" AllowEdit="True">
                                                <Parameters>
                                                    <px:PXSyncGridParam ControlID="gridNR" />
                                                </Parameters>
                                            </px:PXSelector>
                                        </RowTemplate>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Warehouse Management">
                <Template>
					<px:PXFormView ID="formScanSetup" runat="server" DataSourceID="ds" Width="100%" DataMember="PickPackShipSetup" DefaultControlID="edShowPickTab" RenderStyle="Simple">
						<Template>
							<px:PXLabel ID="lblScanSetup" runat="server" Height="30px">These settings are specific to the current branch.</px:PXLabel>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Fulfillment Workflow" LabelsWidth="M" />
							<px:PXCheckBox SuppressLabel="True" ID="edShowPickTab" runat="server" DataField="ShowPickTab" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edShowPackTab" runat="server" DataField="ShowPackTab" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edShowShipTab" runat="server" DataField="ShowShipTab" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edShowScanLogTab" runat="server" DataField="ShowScanLogTab" />

							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Fulfillment Settings" LabelsWidth="M" />
							<px:PXDropDown SuppressLabel="False" ID="edShortShipmentConfirmation" runat="server" DataField="ShortShipmentConfirmation" CommitChanges="True" />
							<px:PXDropDown SuppressLabel="False" ID="edShipmentLocationOrdering" runat="server" DataField="ShipmentLocationOrdering" CommitChanges="True" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultQty" runat="server" DataField="UseDefaultQty" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edExplicitLineConfirmation" runat="server" DataField="ExplicitLineConfirmation" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseCartsForPick" runat="server" DataField="UseCartsForPick" />
							<px:PXCheckBox SuppressLabel="True" ID="edDefaultLocation" runat="server" DataField="DefaultLocationFromShipment" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edDefaultLotSerial" runat="server" DataField="DefaultLotSerialFromShipment" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edEnterSizeForPackages" runat="server" DataField="EnterSizeForPackages" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edPrintShipmentConfirmation" runat="server" DataField="PrintShipmentConfirmation" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edPrintShipmentLabels" runat="server" DataField="PrintShipmentLabels" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edConfirmEachPackageWeight" runat="server" DataField="ConfirmEachPackageWeight" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edRequestLocationForEachItem" runat="server" DataField="RequestLocationForEachItem" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edConfirmToteForEachItem" runat="server" DataField="ConfirmToteForEachItem" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edPrintPickListsAndPackSlipsTogether" runat="server" DataField="PrintPickListsAndPackSlipsTogether" CommitChanges="true" />
						</Template>
					</px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize MinHeight="480" Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>
