<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN201000.aspx.cs"
    Inherits="Page_IN201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" TypeName="PX.Objects.IN.INItemClassMaint"
		PrimaryView="itemclass" PageLoadBehavior="GoFirstRecord">
        <CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="GoToNodeSelectedInTree" Visible="False" CommitChanges="True" PostData="Self"/>
            <px:PXDSCallbackCommand Name="ViewRestrictionGroups" Visible="False" />
            <px:PXDSCallbackCommand Name="ResetGroup" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Action" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewGroupDetails" Visible="False" DependOnGrid="grid" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
			<px:PXTreeDataMember TreeView="ItemClassNodes" TreeKeys="ItemClassID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh"
		AcceptButtonID="btnOK" CancelButtonID="btnCancel">
		<px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
			DataMember="ChangeIDDialog">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXMaskEdit ID="edAcctCD" runat="server" DataField="CD" CommitChanges="True"/>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds"/>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlConfirmDirtyTreeNavigation" runat="server" Key="itemclass"
		CaptionVisible="True"
		DesignView="Hidden" CreateOnDemand="true" LoadOnDemand="true"
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" AutoCallBack-Target="formConfirmNavigation"	CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="btnYes" CancelButtonID="btnNo" CloseButtonDialogResult="Abort">
		<px:PXFormView ID="formConfirmNavigation" runat="server" DataSourceID="ds" SkinID="Transparent" Width="100%" >
			<Template>
				<px:PXLayoutRule ID="rl" runat="server" StartColumn="True" LabelsWidth="XL" />
				<px:PXLabel ID="lblConfirmNavigation" runat="server" Text="Any unsaved changes will be discarded."></px:PXLabel>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlConfirmDirtyTreeNavigationButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnYes" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="btnNo" runat="server" DialogResult="Abort" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="syncForm" runat="server" DataSourceID="ds" DataMember="TreeViewAndPrimaryViewSynchronizationHelper" Height="0" Width="100%" RenderStyle="Simple">
		<Template>
			<px:PXTextEdit ID="edDescrHelper" runat="server" DataField="Descr" Visible="False" Enabled="False" />
		</Template>
	</px:PXFormView>
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" DataMember="ItemClassNodes" Height="180px"
				AutoRepaint="True" SyncPosition="True" SyncPositionWithGraph="True" PreserveExpanded="True" ExpandDepth="0" PopulateOnDemand="True" 
				Caption="Item Class Tree" ShowRootNode="False" AllowCollapse="True">
				<CaptionStyle Height="17px" />
				<AutoCallBack Command="GoToNodeSelectedInTree" Target="ds"/>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="ItemClassNodes" TextField="SegmentedClassCD" ValueField="ItemClassID" DescriptionField="Descr" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXFormView ID="scrollForm" runat="server" Width="100%" RenderStyle="Simple">
				<Template>
			<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="itemclass" AllowCollapse="false"
        Caption="Item Class Summary" CaptionVisible="false" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity"
        DefaultControlID="edItemClassID">
				<ContentStyle BorderStyle="None"></ContentStyle>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
							<px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassCD" AutoRefresh="True" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
        </Template>
    </px:PXFormView>
			<px:PXTab ID="tab" runat="server" Width="100%" Height="530px" DataSourceID="ds" DataMember="itemclasssettings" MarkRequired="Dynamic">
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="General Settings" />
                    <px:PXCheckBox CommitChanges="True" ID="chkStkItem" runat="server" DataField="StkItem" />
                    <px:PXCheckBox ID="chkNegQty" runat="server" DataField="NegQty" />
	                <px:PXCheckBox ID="chkAccrueCost" runat="server" DataField="AccrueCost" CommitChanges="True" />
                    <px:PXDropDown ID="edItemType" runat="server" DataField="ItemType" CommitChanges="true" />
                    <px:PXDropDown ID="edValMethod" runat="server" AllowNull="False" DataField="ValMethod" CommitChanges="True" SelectedIndex="1" />
                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" AutoRefresh="True" />
                    <px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" />
                    <px:PXSelector ID="edPostClassID" runat="server" DataField="PostClassID" AllowEdit="True" />
                    <px:PXSelector ID="edLotSerClassID" runat="server" DataField="LotSerClassID" AllowEdit="True" />
                    <px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edDfltSiteID" runat="server" DataField="DfltSiteID" AllowEdit="True" />
                    <px:PXSelector ID="edAvailabilitySchemeID" runat="server" DataField="AvailabilitySchemeID" AllowEdit="True" />
                    <px:PXSelector runat="server" ID="edCountryOfOrigin" DataField="CountryOfOrigin" />
	                <px:PXLayoutRule runat="server" GroupCaption="International Shipping" StartGroup="True" />
                    <px:PXTextEdit runat="server" ID="edHSTariffCode" DataField="HSTariffCode" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Thresholds" />
					<px:PXNumberEdit ID="edUndershipThreshold" runat="server" DataField="UndershipThreshold" />
					<px:PXNumberEdit ID="edOvershipThreshold" runat="server" DataField="OvershipThreshold" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Unit of Measure" />
                    <px:PXPanel ID="PXPanel2" runat="server" RenderSimple="True" RenderStyle="Simple" Width="410px">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
						<px:PXLayoutRule runat="server" Merge="true"/>
                        <px:PXSelector CommitChanges="True" ID="edBaseUnit" runat="server" DataField="BaseUnit" AllowEdit="True" Width="100px" Style="margin-right:30px"/>
						<px:PXCheckBox ID="chkDecimalBaseUnit" runat="server" DataField="DecimalBaseUnit" CommitChanges="True" />
                        <px:PXLayoutRule runat="server" Merge="true"/>
						<px:PXSelector CommitChanges="True" ID="edSalesUnit" runat="server" DataField="SalesUnit" AllowEdit="True" Width="100px" Style="margin-right:30px"/>
						<px:PXCheckBox ID="chkDecimalSalesUnit" runat="server" DataField="DecimalSalesUnit" CommitChanges="True" />
						<px:PXLayoutRule runat="server" Merge="true"/>
                        <px:PXSelector CommitChanges="True" ID="edPurchaseUnit" runat="server" DataField="PurchaseUnit" AllowEdit="True" Width="100px" Style="margin-right:30px"/>
						<px:PXCheckBox ID="chkDecimalPurchaseUnit" runat="server" DataField="DecimalPurchaseUnit" CommitChanges="True" />
						<px:PXLayoutRule runat="server"/>
                        <px:PXGrid ID="gridUnits" runat="server" DataSourceID="ds" Height="103" SkinID="ShortList" Caption="Conversions" CaptionVisible="false">
                            <Mode InitNewRow="True" />
                            <Levels>
                                <px:PXGridLevel DataMember="classunits" DataKeyNames="UnitType,ItemClassID,InventoryID,ToUnit,FromUnit">
                                    <RowTemplate>
                                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                        <px:PXDropDown ID="edUnitType" runat="server" DataField="UnitType" SelectedIndex="2" />
                                        <px:PXNumberEdit ID="edItemClassID2" runat="server" DataField="ItemClassID" />
                                        <px:PXNumberEdit ID="edInventoryID" runat="server" DataField="InventoryID" />
                                        <px:PXMaskEdit ID="edFromUnit" runat="server" DataField="FromUnit" InputMask=">aaaaaa" />
                                        <px:PXMaskEdit ID="edSampleToUnit" runat="server" DataField="SampleToUnit" InputMask=">aaaaaa" />
                                        <px:PXDropDown ID="edUnitMultDiv" runat="server" DataField="UnitMultDiv" />
                                        <px:PXNumberEdit ID="edUnitRate" runat="server" DataField="UnitRate" />
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn AllowNull="False" DataField="UnitType" Width="99px" Visible="False" RenderEditorText="True" />
                                        <px:PXGridColumn AllowNull="False" DataField="ItemClassID" Width="36px" Visible="False" />
                                        <px:PXGridColumn AllowNull="False" DataField="InventoryID" Visible="False" TextAlign="Right" Width="54px" />
                                        <px:PXGridColumn DataField="FromUnit" Width="72px" CommitChanges="True"/>
                                        <px:PXGridColumn AllowNull="False" DataField="UnitMultDiv" Width="90px" RenderEditorText="True" />
                                        <px:PXGridColumn AllowNull="False" DataField="UnitRate" TextAlign="Right" Width="108px" />
                                        <px:PXGridColumn DataField="SampleToUnit" Width="72px" />
                                    </Columns>
                                    <Layout FormViewHeight="" />
                                </px:PXGridLevel>
                            </Levels>
                            <Layout ColumnsMenu="False" />
                        </px:PXGrid>
                    </px:PXPanel>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Price Management" LabelsWidth="S" ControlSize="XM" />
                    <px:PXTreeSelector ID="edPriceWorkgroupID" runat="server" DataField="PriceWorkgroupID" TreeDataMember="_EPCompanyTree_Tree_"
                        TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
                        <DataBindings>
                            <px:PXTreeItemBinding TextField="Description" ValueField="Description" />
                        </DataBindings>
                    </px:PXTreeSelector>
                    <px:PXSelector ID="edPriceManagerID" runat="server" DataField="PriceManagerID" AllowEdit="True" />
                    <px:PXNumberEdit ID="edMinGrossProfitPct" runat="server" DataField="MinGrossProfitPct" />
                    <px:PXNumberEdit ID="edMarkupPct" runat="server" DataField="MarkupPct" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Replenishment Settings" RepaintOnDemand="False" >
                <Template>
					 <px:PXFormView ID="PXFormView3" runat="server" DataMember="itemclasssettings" DataSourceID="ds" Style="z-index: 100" Width="100%">
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXDropDown ID="edDemandCalculation" runat="server" DataField="DemandCalculation" AllowNull="false" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="repGrid" runat="server" Height="250px" Width="100%" DataSourceID="ds" SkinID="DetailsInTab">
                        <Mode InitNewRow="true" />
                        <Levels>
                            <px:PXGridLevel DataMember="replenishment">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edReplenishmentClassID" runat="server" DataField="ReplenishmentClassID" AllowEdit="true" />
                                    <px:PXDropDown ID="edReplenishmentMethod" runat="server" AllowNull="False" DataField="ReplenishmentMethod" />
                                    <px:PXDropDown ID="edReplenishmentSource" runat="server" AllowNull="False" DataField="ReplenishmentSource" />
                                    <px:PXSegmentMask ID="edReplenishmentSourceSiteID" runat="server" DataField="ReplenishmentSourceSiteID" />									
                                    <px:PXSelector ID="edReplenishmentPolicyID" runat="server" DataField="ReplenishmentPolicyID" AllowEdit="true" />
									<px:PXNumberEdit ID="edTransferLeadTime" runat="server" AllowNull="false" Size="xxs" DataField="TransferLeadTime" />
									<px:PXNumberEdit ID="edTransferERQ" runat="server" AllowNull="false" Size="xxs" DataField="TransferERQ" />									
									<px:PXNumberEdit ID="edHistoryDepth" runat="server" AllowNull="false" Size="xxs" DataField="HistoryDepth" />
                                    <px:PXDateTimeEdit ID="edLaunchDate" runat="server" DataField="LaunchDate" />
                                    <px:PXDateTimeEdit ID="edTerminationDate" runat="server" DataField="TerminationDate" />
									<px:PXNumberEdit ID="edServiceLevelPct" runat="server" AllowNull="false" DataField="ServiceLevelPct"/>
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="ReplenishmentClassID" DisplayFormat="&gt;aaaaaaaaaa" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ReplenishmentPolicyID" DisplayFormat="&gt;aaaaaaaaaa" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="ReplenishmentSource" AutoCallBack="true" Width="140px" />
									<px:PXGridColumn AllowNull="False" DataField="ReplenishmentMethod" AutoCallBack="true" Width="140px" />
                                    <px:PXGridColumn DataField="ReplenishmentSourceSiteID" DisplayFormat="&gt;AAAAAAAAAA" Width="140px" />
									<px:PXGridColumn DataField="TransferLeadTime" Width="120px" />
									<px:PXGridColumn DataField="TransferERQ" Width="100px" />
									<px:PXGridColumn DataField="ForecastModelType" Width="140px"/>
									<px:PXGridColumn DataField="ForecastPeriodType" Width="50px"/>
									<px:PXGridColumn DataField="HistoryDepth" Width="50px" AllowNull = "False" />
                                    <px:PXGridColumn DataField="LaunchDate" Width="90px" />
                                    <px:PXGridColumn DataField="TerminationDate" Width="100px" />
									<px:PXGridColumn DataField="ServiceLevelPct" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="true" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Restriction Groups">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto"
                        AllowSearch="True" SkinID="Details" BorderWidth="0px">
                        <ActionBar>
                            <Actions>
                                <NoteShow Enabled="false" />
                            </Actions>
                            <CustomItems>
							    <px:PXToolBarButton Text="Group Details" CommandSourceID="ds" CommandName="ViewGroupDetails"/>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Groups">
                                <Mode AllowAddNew="False" AllowDelete="False" />
                                <Columns>
                                    <px:PXGridColumn AllowNull="False" DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px" RenderEditorText="True"
                                        AllowCheckAll="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="GroupName" Width="150px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="SpecificType" Width="150px" Type="DropDownList" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Description" Width="200px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="40px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="GroupType" Label="Visible To Entities" RenderEditorText="True"
                                        Width="171px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
                                    <px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                                    <px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
                                    <px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType" Enabled="False" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" SkinID="Details" ActionsPosition="Top" DataSourceID="ds" Width="100%" BorderWidth="0px"
                        Style="left: 0px; top: 0px; height: 13px">
                        <Levels>
                            <px:PXGridLevel DataMember="Mapping">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edCRAttributeID" runat="server" DataField="AttributeID" AutoRefresh="true" FilterByAllFields="True" />
                                    <px:PXTextEdit ID="edDescription2" runat="server" AllowNull="False" DataField="Description" />
                                    <px:PXCheckBox ID="chkRequired" runat="server" DataField="Required" />
                                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" AutoCallBack="True" LinkCommand = "CRAttribute_ViewDetails" />
                                    <px:PXGridColumn AllowNull="False" DataField="Description" Width="351px" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" Width="63px" />
									<px:PXGridColumn AllowNull="False" DataField="AttributeCategory" Type="DropDownList" CommitChanges="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>                      
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Service Management" LoadOnDemand="True" RepaintOnDemand="False" BindingContext="form" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1">
				<Template>
					<px:PXFormView runat="server" ID="formEquipment" DataMember="itemclasssettings" Style="z-index: 100" Width="100%">
                        <ContentStyle BackColor="Transparent" BorderStyle="None" >
                        </ContentStyle>
						<Template>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Service Management" LabelsWidth="SM" ControlSize="M" />
							<px:PXDropDown runat="server" ID="edDfltBillingRule" DataField="DfltBillingRule" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Route Management" />
							<px:PXCheckBox runat="server" ID="edRequireRoute" DataField="RequireRoute" AlignLeft="True" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Equipment Management" />
							<px:PXGroupBox runat="server" ID="edEquipmentItemClass" Caption="Equipment Class" DataField="EquipmentItemClass" CommitChanges="True">
								<Template>
									<px:PXRadioButton runat="server" ID="edEquipmentItemClass_op0" Value="OI" Text="Part or Other Inventory" />
									<px:PXRadioButton runat="server" ID="edEquipmentItemClass_op1" Value="ME" Text="Model Equipment" />
									<px:PXRadioButton runat="server" ID="edEquipmentItemClass_op2" Text="Component" Value="CT" />
									<px:PXRadioButton runat="server" ID="edEquipmentItemClass_op3" Text="Consumable" Value="CE" />
								</Template>
							</px:PXGroupBox>
							<px:PXCheckBox runat="server" ID="edMem_ShowComponent" DataField="Mem_ShowComponent" />
						</Template>
					</px:PXFormView>
					<px:PXGrid runat="server" ID="ModelTemplateComponents" SkinID="DetailsInTab" AutoGenerateColumns="None" AdjustPageSize="Auto" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="ModelTemplateComponentRecords">
								<Columns>
									<px:PXGridColumn DataField="ComponentCD" Width="120px" />
									<px:PXGridColumn DataField="Active" Width="70px" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Optional" Width="70" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Qty" Width="70" />
									<px:PXGridColumn DataField="Descr" Width="250px" />
									<px:PXGridColumn DataField="ClassID" Width="120px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
        </Items>
						<AutoSize Enabled="True" MinHeight="150" />
    </px:PXTab>
				</Template>
				<AutoSize Enabled="True" />
			</px:PXFormView>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
