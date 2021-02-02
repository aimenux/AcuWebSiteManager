<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM304000.aspx.cs" Inherits="Page_AM304000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="EstimateOperationRecords" 
        TypeName="PX.Objects.AM.EstimateOperMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		    <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="gridMaterial" />
		    <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="gridMaterial" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="EstimateOperationRecords" DefaultControlID="edEstimateID" 
        NoteIndicator="true" FilesIndicator="True" SyncPosition="True" ActivityIndicator="True" NotifyIndicator="True"  >
        <CallbackCommands>
            <Save PostData="Self" />
            <Refresh RepaintControlsIDs="gridMaterial,gridSteps,gridTools,gridOverhead" />
        </CallbackCommands>
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
                <px:PXSelector ID="edEstimateID" runat="server" DataField="EstimateID" CommitChanges="True" AllowEdit="True" FilterByAllFields="True" TextMode="Search"/>
                <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" CommitChanges="True" AutoRefresh="True" />
                <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" CommitChanges="True" AutoRefresh="True" FilterByAllFields="True" TextMode="Search"/>
                <px:PXSelector ID="edWorkCenterID" runat="server" DataField="WorkCenterID" AutoCallBack="True" CommitChanges="True" MaxLength="10" AllowEdit="True" />
                <px:PXMaskEdit ID="edSetupTime" runat="server" DataField="SetupTime" CommitChanges="True" />
                <px:PXNumberEdit ID="edRunUnits" runat="server" DataField="RunUnits" Width="200px" CommitChanges="True" />
                <px:PXMaskEdit ID="edRunUnitTime" runat="server" DataField="RunUnitTime" CommitChanges="True" />
                <px:PXNumberEdit ID="edMachineUnits" runat="server" DataField="MachineUnits" Width="200px" CommitChanges="True" />
                <px:PXMaskEdit ID="edMachineUnitTime" runat="server" DataField="MachineUnitTime" CommitChanges="True" />
                <px:PXMaskEdit ID="edQueueTime" runat="server" DataField="QueueTime" CommitChanges="True" />
                <px:PXCheckBox ID="edBackflushLabor" runat="server" DataField="BackflushLabor" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" ColumnSpan="2" />
                <px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edFixedLaborCost" runat="server" DataField="FixedLaborCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edFixedLaborOverride" runat="server" DataField="FixedLaborOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edVariableLaborCost" runat="server" DataField="VariableLaborCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edVariableLaborOverride" runat="server" DataField="VariableLaborOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edMachineCost" runat="server" DataField="MachineCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edMachineOverride" runat="server" DataField="MachineOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edMaterialCost" runat="server" DataField="MaterialCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edMaterialOverride" runat="server" DataField="MaterialOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edToolCost" runat="server" DataField="ToolCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edToolOverride" runat="server" DataField="ToolOverride" CommitChanges="True" /> 
            <px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edFixedOverheadCost" runat="server" DataField="FixedOverheadCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edFixedOverheadOverride" runat="server" DataField="FixedOverheadOverride" CommitChanges="True" />  
            <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edVariableOverheadCost" runat="server" DataField="VariableOverheadCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edVariableOverheadOverride" runat="server" DataField="VariableOverheadOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edSubcontractCost" runat="server" DataField="SubcontractCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edSubcontractOverride" runat="server" DataField="SubcontractOverride" CommitChanges="True" />  
            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="False"  LabelsWidth="SM" ControlSize="M" />
                <px:PXNumberEdit ID="edExtCost" runat="server" DataField="ExtCost" Width="200px" CommitChanges="True" />
                <px:PXNumberEdit ID="edReferenceMaterialCost" runat="server" DataField="ReferenceMaterialCost" Width="200px"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="OutsideProcessingOperationSelected" DataKeyNames="EstimateID,RevisionID,OperationID" DynamicTabs="False" >
		<Items>
			<px:PXTabItem Text="Material" >
                <Template>
                    <px:PXGrid runat="server" ID="gridMaterial" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AdjustPageSize="Auto" 
                        AllowPaging="True" AllowSearch="true" SyncPosition="True" >
                        <Levels>
                            <px:PXGridLevel DataMember="EstimateOperMatlRecords" DataKeyNames="EstimateID,RevisionID,OperationID,LineID" >
                                <RowTemplate>
                                    <px:PXSelector ID="edInventoryCD" runat="server" DataField="InventoryCD" CommitChanges="True" AllowEdit="True"/>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                                    <px:PXTextEdit ID="edItemDesc" runat="server" AllowNull="False" DataField="ItemDesc" />
                                    <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edQtyReq" runat="server" DataField="QtyReq"  />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" /> 
                                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" CommitChanges="True" />
                                    <px:PXCheckBox ID="chkBackFlush" runat="server" DataField="BackFlush" />
                                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edScrapFactor" runat="server" DataField="ScrapFactor" />
                                    <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize"/>
                                    <px:PXCheckBox ID="edQtyRoundUp" runat="server" DataField="QtyRoundUp"/>
                                    <px:PXNumberEdit ID="edTotalQtyRequired" runat="server" DataField="TotalQtyRequired"/>
                                    <px:PXNumberEdit ID="edMatlPlanCost" runat="server" DataField="MaterialOperCost"/>
                                    <px:PXCheckbox ID="edIsNonInventory" runat="server" DataField="IsNonInventory" />
                                    <px:PXDropDown ID="edMaterialType" runat="server" DataField="MaterialType" CommitChanges="True" />
                                    <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" CommitChanges="True" />
                                    <px:PXDropDown ID="edPhantomRouting" runat="server" DataField="PhantomRouting" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryCD" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubItemID" Width="80px" />
                                    <px:PXGridColumn DataField="ItemDesc" Width="150px" />
                                    <px:PXGridColumn DataField="ItemClassID" Width="80px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="QtyReq" Width="100px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM" Width="80px" /> 
                                    <px:PXGridColumn DataField="UnitCost" Width="80px"  TextAlign="Right" AutoCallBack="True" /> 
                                    <px:PXGridColumn DataField="BackFlush" Width="80px" TextAlign="Center" Type="CheckBox" /> 
                                    <px:PXGridColumn DataField="SiteID" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ScrapFactor" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="BatchSize" Width="100px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="QtyRoundUp" TextAlign="Center" Type="CheckBox" Width="85px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TotalQtyRequired" TextAlign="Right" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MaterialOperCost" TextAlign="Right" Width="100px" AutoCallBack="True" /> 
                                    <px:PXGridColumn DataField="IsNonInventory" Width="80px" TextAlign="Center" Type="CheckBox"  />
                                    <px:PXGridColumn DataField="MaterialType" TextAlign="Left" Width="100px" AutoCallBack="True" />
								    <px:PXGridColumn DataField="PhantomRouting" TextAlign="Left" Width="110px" />
                                    <px:PXGridColumn DataField="LineID" TextAlign="Right" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                                    <px:PXGridColumn DataField="SubcontractSource" Width="95px" AutoCallBack="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowUpload="True" AllowDragRows="True" InitNewRow="True" />
                        <AutoSize Enabled="True"/>
                        <ActionBar ActionsText="False">
                            <CustomItems>
                                <px:PXToolBarButton DependOnGrid="gridMaterial" Key="cmdResetOrder">
                                    <AutoCallBack Command="ResetOrder" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
                                    <AutoCallBack Target="gridMaterial" Command="AddNew" Argument="1"></AutoCallBack>
                                    <ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Cut Row" SyncText="false" ImageSet="main" ImageKey="Copy">
                                    <AutoCallBack Target="gridMaterial" Command="Copy"></AutoCallBack>
                                    <ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Insert Cut Row" SyncText="false" ImageSet="main" ImageKey="Paste">
                                    <AutoCallBack Target="gridMaterial" Command="Paste"></AutoCallBack>
                                    <ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <CallbackCommands PasteCommand="PasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Steps" >
			    <Template>
			        <px:PXGrid ID="gridSteps" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AdjustPageSize="Auto" 
                        AllowPaging="True" AllowSearch="True" MatrixMode="True" SyncPosition="True" >
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="EstimateOperStepRecords" DataKeyNames="EstimateID,RevisionID,OperationID,LineID">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" Merge="true" />
                                    <px:PXTextEdit Size="xl" ID="edStepDescription" runat="server" DataField="Description" />
                                    <px:PXNumberEdit ID="edStepSortOrder" runat="server" DataField="SortOrder" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Description" Width="351px" />
                                    <px:PXGridColumn DataField="LineID" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="OperationID" MaxLength="10"/>
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="85px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowUpload="True" />
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Tools" >
			    <Template>
			        <px:PXGrid ID="gridTools" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AdjustPageSize="Auto" 
                        AllowPaging="True" AllowSearch="True" MatrixMode="True" SyncPosition="True" >
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="EstimateOperToolRecords" DataKeyNames="EstimateID,RevisionID,OperationID,LineID">
                                <RowTemplate>
                                    <px:PXSelector ID="edToolID" runat="server" DataField="ToolID" AllowEdit="True" CommitChanges="True" />
                                    <px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" />
                                    <px:PXNumberEdit ID="edToolQtyReq" runat="server" DataField="QtyReq"  />
                                    <px:PXNumberEdit ID="edToolUnitCost" runat="server" DataField="UnitCost"  />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ToolID" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" Width="200px" />
                                    <px:PXGridColumn DataField="QtyReq" Width="120px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UnitCost" Width="120px" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowUpload="True" />
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		    <px:PXTabItem Text="Overhead" >
                <Template>
                    <px:PXGrid ID="gridOverhead" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AdjustPageSize="Auto" 
                        AllowPaging="True" AllowSearch="True" MatrixMode="True" SyncPosition="True" >
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="EstimateOperOvhdRecords" DataKeyNames="EstimateID,RevisionID,OperationID,LineID">
                                <RowTemplate>
                                    <px:PXSelector ID="edOvhdID" runat="server" DataField="OvhdID" AllowEdit="True" />
                                    <px:PXTextEdit ID="edOvhdDescription" runat="server" DataField="Description" />
                                    <px:PXDropDown ID="edOvhdType" runat="server" DataField="OvhdType" />
                                    <px:PXNumberEdit ID="edOvhdCostRate" runat="server" DataField="OverheadCostRate" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edOFactor" runat="server" DataField="OFactor" CommitChanges="True" />
                                    <px:PXCheckbox ID="edWCFlag" runat="server" DataField="WCFlag" CommitChanges="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OvhdID" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" Width="200px" />
                                    <px:PXGridColumn DataField="OvhdType" Width="120px" />
                                    <px:PXGridColumn DataField="OverheadCostRate" Width="120px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OFactor" Width="120px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="WCFlag" Width="80px" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowUpload="True" />
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Outside Process" >
                <Template >
                    <px:PXLayoutRule ID="PXLayoutRule25" runat="server" GroupCaption="General Settings" StartColumn="True" LabelsWidth="125px" />
                    <px:PXCheckBox ID="edOutsideProcess" runat="server" DataField="OutsideProcess" CommitChanges="True" />
                    <px:PXCheckBox ID="edDropShippedToVendor" runat="server" DataField="DropShippedToVendor" CommitChanges="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
