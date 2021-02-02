<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM303000.aspx.cs" Inherits="Page_AM303000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="Documents" TypeName="PX.Objects.AM.EstimateMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewOperation" />
            <px:PXDSCallbackCommand Name="AddHistory" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CreateProdOrder" Visible="false" CommitChanges="True" />
		    <px:PXDSCallbackCommand Visible="false" Name="ViewQuote" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Documents" DefaultControlID="edEstimateID" 
        NoteIndicator="true" FilesIndicator="True" ActivityIndicator="True" NotifyIndicator="True" >
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" Merge="true"  LabelsWidth="SM" ControlSize="M" />
                <px:PXSelector ID="edEstimateID" runat="server" DataField="EstimateID" AutoRefresh="True" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" TextMode="Search"/>
                <px:PXCheckBox ID="chkIsNonInventory" runat="server" DataField="IsNonInventory" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXSelector ID="PXSelector1" runat="server" DataField="RevisionID" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" />
                <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="IsPrimary" />
            <px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartColumn="false" LabelsWidth="SM" ControlSize="M" />
                <px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" DataSourceID="ds" AutoRefresh="true" CommitChanges="True" FilterByAllFields="True" AllowEdit="True"/> 
                <px:PXLayoutRule ID="PXLayoutRule3" runat="server" ColumnSpan="2"/>
                <px:PXTextEdit ID="edItemDesc" runat="server" DataField="ItemDesc" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="false"  LabelsWidth="SM" ControlSize="M" />
                <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                <px:PXSelector ID="edEstimateClassID" runat="server" DataField="EstimateClassID" CommitChanges="True" AllowEdit="True" /> 
                <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" CommitChanges="True" AllowEdit="True" />
                <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" CommitChanges="True" AllowEdit="True" /> 
                <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" />
                <px:PXSelector ID="EngineerID" runat="server" DataField="EngineerID" />
                <px:PXDateTimeEdit ID="edRequestDate" runat="server" DataField="RequestDate" />
                <px:PXDateTimeEdit ID="edPromiseDate" runat="server" DataField="PromiseDate" />
            <px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="M"/>
                <px:PXNumberEdit ID="edLeadTime" runat="server" DataField="LeadTime" />
                <px:PXCheckBox ID="edLeadTimeOverride" runat="server" DataField="LeadTimeOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXDropDown ID="edEstimateStatus" runat="server" DataField="EstimateStatus" CommitChanges="True" />
                <px:PXDropDown ID="edQuoteSource" runat="server" DataField="QuoteSource" />
            <px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartColumn="false" Merge="false" LabelsWidth="SM" ControlSize="M" />
                <px:PXDateTimeEdit ID="edRevisionDate" runat="server" DataField="RevisionDate"/>
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM" />
                <px:PXNumberEdit ID="edFixedLaborCost" runat="server" DataField="FixedLaborCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edFixedLaborOverride" runat="server" DataField="FixedLaborOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edVariableLaborCost" runat="server" DataField="VariableLaborCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edVariableLaborOverride" runat="server" DataField="VariableLaborOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edMachineCost" runat="server" DataField="MachineCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edMachineOverride" runat="server" DataField="MachineOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edMaterialCost" runat="server" DataField="MaterialCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edMaterialOverride" runat="server" DataField="MaterialOverride" CommitChanges="True" /> 
            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edToolCost" runat="server" DataField="ToolCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edToolOverride" runat="server" DataField="ToolOverride" Width="100px" CommitChanges="True" /> 
            <px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edFixedOverheadCost" runat="server" DataField="FixedOverheadCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edFixedOverheadOverride" runat="server" DataField="FixedOverheadOverride" Width="100px" CommitChanges="True" />  
            <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edVariableOverheadCost" runat="server" DataField="VariableOverheadCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edVariableOverheadOverride" runat="server" DataField="VariableOverheadOverride" Width="100px" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartColumn="false" Merge="true" LabelsWidth="SM" ControlSize="XM"/>
                <px:PXNumberEdit ID="edSubcontractCost" runat="server" DataField="SubcontractCost" Width="200px" CommitChanges="True" />
                <px:PXCheckBox ID="edSubcontractOverride" runat="server" DataField="SubcontractOverride" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="False"  LabelsWidth="SM" ControlSize="XM" />
                <px:PXNumberEdit ID="edExtCostDisplay" runat="server" DataField="ExtCostDisplay" Width="200px" CommitChanges="True" />
                <px:PXNumberEdit ID="edReferenceMaterialCost" runat="server" DataField="ReferenceMaterialCost" Width="200px" CommitChanges="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataMember="EstimateRecordSelected" DataSourceID="ds" >
		<Items>
			<px:PXTabItem Text="Operations">
                <Template>
                    <px:PXGrid ID="gridOperations" runat="server" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
                        DataSourceID="ds" SkinID="DetailsInTab" width="100%" SyncPosition="True" >
                        <Mode InitNewRow="true" />
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="EstimateOperRecords" DataKeyNames="EstimateID,RevisionID,OperationID">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edOperationCD" runat="server" DataField="OperationCD" CommitChanges="True"/>
                                    <px:PXSelector ID="edWorkCenterID" runat="server" DataField="WorkCenterID" AllowEdit="True" />
                                    <px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" MaxLength="120" />
                                    <px:PXMaskEdit ID="edSetupTime" runat="server" DataField="SetupTime" Width="200px" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edRunUnits" runat="server" DataField="RunUnits" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edRunUnitTime" runat="server" DataField="RunUnitTime"  CommitChanges="True" />
                                    <px:PXNumberEdit ID="edMachineUnits" runat="server" DataField="MachineUnits" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edMachineUnitTime" runat="server" DataField="MachineUnitTime" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edQueueTime" runat="server" DataField="QueueTime" Width="200px" CommitChanges="True" />
                                    <px:PXCheckbox ID="edBackFlushLabor" runat="server" DataField="BackFlushLabor" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edoperFixedLaborCost" runat="server" DataField="FixedLaborCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperFixedLaborOverride" runat="server" DataField="FixedLaborOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edoperVariableLaborCost" runat="server" DataField="VariableLaborCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperVariableLaborOverride" runat="server" DataField="VariableLaborOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edoperMachineCost" runat="server" DataField="MachineCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperMachineOverride" runat="server" DataField="MachineOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edOperMaterialCost" runat="server" DataField="MaterialCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperMaterialOverride" runat="server" DataField="MaterialOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edOperToolCost" runat="server" DataField="ToolCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperToolOverride" runat="server" DataField="ToolOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edOperFixedOverheadCost" runat="server" DataField="FixedOverheadCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperFixedOverheadOverride" runat="server" DataField="FixedOverheadOverride" CommitChanges="True" /> 
                                    <px:PXNumberEdit ID="edOperVariableOverheadCost" runat="server" DataField="VariableOverheadCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperVariableOverheadOverride" runat="server" DataField="VariableOverheadOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edOperSubcontractCost" runat="server" DataField="SubcontractCost" CommitChanges="True" />
                                    <px:PXCheckbox ID="edOperSubcontractOverride" runat="server" DataField="SubcontractOverride" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edOperReferenceMaterialCost" runat="server" DataField="ReferenceMaterialCost" />
                                    <px:PXCheckbox ID="edOutsideProcess" runat="server" DataField="OutsideProcess" CommitChanges="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OperationCD" Width="100px" AutoCallBack="True" LinkCommand="ViewOperation"/>
                                    <px:PXGridColumn DataField="WorkCenterID" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" Width="120px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SetupTime" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="RunUnits" TextAlign="Right" Width="90px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="RunUnitTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MachineUnits" TextAlign="Right" Width="90px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MachineUnitTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="QueueTime" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="BackFlushLabor" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FixedLaborCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FixedLaborOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VariableLaborCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VariableLaborOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MachineCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MachineOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MaterialCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MaterialOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ToolCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ToolOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FixedOverheadCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FixedOverheadOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VariableOverheadCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VariableOverheadOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubcontractCost" Width="80px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubcontractOverride" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ReferenceMaterialCost" Width="80px" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="OutsideProcess" Width="80px" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowUpload="True" />
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="References" >
			    <Template>
			        <px:PXFormView ID="formreferences" runat="server" DataMember="EstimateReferenceRecord" DataSourceID="ds" SkinID="Transparent">
			            <Template>
						    <px:PXLayoutRule ID="PXLayoutRuleRef1" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
			                <px:PXSelector ID="edOpportunityID" runat="server" DataField="OpportunityID" Enabled="False" AllowEdit="True" />
                            <px:PXTextEdit ID="edQuoteType" runat="server" DataField="QuoteType" />
			                <px:PXTextEdit ID="edQuoteNbr" runat="server" DataField="QuoteNbr" AutoCallBack="True" CommitChanges="True" />
			                <px:PXTextEdit ID="edQuoteNbrLink" runat="server" DataField="QuoteNbrLink" AutoCallBack="True" Enabled="False" >
			                    <LinkCommand Command="ViewQuote" Target="ds" />
			                </px:PXTextEdit>
                            <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType"/>
			                <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" AllowEdit="True"/>
                            <px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" />
                            <px:PXSelector ID="edTaskID" runat="server" DataField="TaskID" CommitChanges="True" AutoRefresh="True" />
                            <px:PXSelector ID="edCostCodeID" runat="server" DataField="CostCodeID" CommitChanges="True" AutoRefresh="True" />
                            <px:PXLayoutRule ID="PXLayoutRuleRef2" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
                            <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
                            <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" CommitChanges="True" />
			                <px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
			                <px:PXTextEdit ID="edExternalRefNbr" runat="server" DataField="ExternalRefNbr" CommitChanges="True"/>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
		    <px:PXTabItem Text="Image">
                <Template>
                    <px:PXImageUploader Height="375px" Width="375px" ID="imgUploader" runat="server" DataField="ImageUrl" AllowUpload="true"
                                        ShowComment="true" DataMember="EstimateRecordSelected"/>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Description" LoadOnDemand="true" >
                <AutoCallBack Command="Refresh" Target="tab" ActiveBehavior="true">
                    <Behavior CommitChanges="true" PostData="Page" />
                </AutoCallBack>
                <Template>
                    <px:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="border-top: 0px; border-left: 0px; border-right: 0px; margin: 0px; padding: 0px;
                        width: 100%;" AllowSourceMode="true" AllowAttached="true" AllowSearch="true"  >
                        <AutoSize Enabled="True" MinHeight="216" />
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="History">
                <Template>
                    <px:PXGrid ID="PXGridHistory" runat="server" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
                        DataSourceID="ds" SkinID="Inquire" Width="100%" TabIndex="2100" SyncPosition="True">
                        <ActionBar ActionsText="True">
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Comment" CommandSourceID="ds" CommandName="AddHistory"/>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="EstimateHistoryRecords">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edHistoryRevisionID" runat="server" DataField="RevisionID" />
                                    <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" />
                                    <px:PXTextEdit ID="edCreatedByID" runat="server" DataField="CreatedByID" />
                                    <px:PXTextEdit ID="edHistoryDescription" runat="server" DataField="Description" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RevisionID" TextField="RevisionID" Width="70" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="150" />
                                    <px:PXGridColumn DataField="CreatedByID" Width="150" />
                                    <px:PXGridColumn DataField="Description" Width="450" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="300" />
                        <ActionBar ActionsText="False" >
                        </ActionBar>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Totals">
                <Template>
                    <px:PXFormView ID="formTotals" runat="server" DataMember="EstimateRecordSelected" DataSourceID="ds" DefaultControlID="edEstimateID" SkinID="Transparent">
                        <Template>
						    <px:PXLayoutRule ID="PXLayoutRuleTotals" runat="server" GroupCaption="Order Qty" StartColumn="True" LabelsWidth="SM" 
                                ControlSize="M" />
                            <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" CommitChanges="True" Width="200px"     />
                            <px:PXSelector ID="edUOM" runat="server" DataField="UOM" CommitChanges="True" />
                            <px:PXLayoutRule ID="PXLayoutRule17" runat="server" GroupCaption="Markup" StartColumn="False" LabelsWidth="SM" 
                                ControlSize="XM" />
                            <px:PXNumberEdit ID="edLaborMarkupPct" runat="server" DataField="LaborMarkupPct" CommitChanges="True" />
                            <px:PXNumberEdit ID="edMachineMarkupPct" runat="server" DataField="MachineMarkupPct" CommitChanges="True" />
                            <px:PXNumberEdit ID="edMaterialMarkupPct" runat="server" DataField="MaterialMarkupPct" CommitChanges="True" />
                            <px:PXNumberEdit ID="edToolMarkupPct" runat="server" DataField="ToolMarkupPct" CommitChanges="True" />
                            <px:PXNumberEdit ID="edOverheadMarkupPct" runat="server" DataField="OverheadMarkupPct" CommitChanges="True" />
                            <px:PXNumberEdit ID="edSubcontractMarkupPct" runat="server" DataField="SubcontractMarkupPct" CommitChanges="True" />
                            <px:PXNumberEdit ID="edOverallMarkupPct" runat="server" DataField="MarkupPct" />  
                            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" GroupCaption="Cost/Price" StartColumn="True" LabelsWidth="SM" 
                                ControlSize="XM" />
			                <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_AMEstimateItem_CurrencyInfo_"
			                                    DataMember="_Currency_"/>
                            <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="CuryUnitCost" Width="200px" CommitChanges="True" />
                            <px:PXNumberEdit ID="edExtCost" runat="server" DataField="CuryExtCost" Width="200px" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXNumberEdit ID="edUnitPrice" runat="server" DataField="CuryUnitPrice" Width="200px" CommitChanges="True" />
                            <px:PXCheckBox ID="edPriceOverride" runat="server" DataField="PriceOverride" Width="100px" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" Merge="false" />
                            <px:PXNumberEdit ID="edExtPrice" runat="server" DataField="CuryExtPrice" Width="200px" CommitChanges="True" />
                        </Template>
                    </px:PXFormView>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
    <px:PXSmartPanel ID="CreateProdOrderPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="FormCreateProdOrder" Caption="Create Production Order" CaptionVisible="True" Key="CreateProductionOrderFilter"
        DesignView="Content" Height="185px" Width="400px" LoadOnDemand="true" >
        <px:PXFormView ID="FormCreateProdOrder" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="CreateProductionOrderFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True"/>
                    <px:PXTextEdit CommitChanges="True" ID="edProdOrdID" runat="server" DataField="ProdOrdID" />
                    <px:PXSelector CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
                    <px:PXSelector CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="CreateProdOrderButtonPanel" runat="server" SkinID="Buttons" >
            <px:PXButton ID="CreateProdOrderButton1" runat="server" DialogResult="OK" Text="Create" />
            <px:PXButton ID="CreateProdOrderPXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="CreateBOMPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="FormCreateBOM" Caption="Create BOM" CaptionVisible="True" Key="CreateBomItemFilter"
        DesignView="Content" Height="145px" Width="375px" LoadOnDemand="true" >
        <px:PXFormView ID="FormCreateBOM" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="CreateBomItemFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="CreateBOMPanelRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                    <px:PXMaskEdit CommitChanges="True" ID="edCreateBOMPanelBomID" runat="server" DataField="BomID" />
                    <px:PXMaskEdit CommitChanges="True" ID="edCreateBOMPanelRevisionID" runat="server" DataField="RevisionID" />
                    <px:PXSelector CommitChanges="True" ID="edCreateBOMPanelSiteID" runat="server" DataField="SiteID" />
                </Template>
        </px:PXFormView>
        <px:PXPanel ID="CreateBOMPanelButtons" runat="server" SkinID="Buttons" >
            <px:PXButton ID="CreateBOMButton1" runat="server" DialogResult="OK" Text="Create" />
            <px:PXButton ID="CreateBOMButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="Add2OrderPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="Add2OrderForm" Caption="Add to Order" CaptionVisible="True" Key="Add2OrderFilter"
        DesignView="Content" Height="130px" Width="275px" LoadOnDemand="true">
        <px:PXFormView ID="Add2OrderForm" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="Add2OrderFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                    <px:PXSelector ID="edPanelOrderType" runat="server" AutoRefresh="True" DataField="OrderType" CommitChanges="True" />
                    <px:PXSelector ID="edPanelOrderNbr" runat="server" AutoRefresh="True" DataField="OrderNbr" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="Add2OrderButtons" runat="server" SkinID="Buttons" >
            <px:PXButton ID="Add2OrderAdd" runat="server" DialogResult="OK" Text="Add" />
            <px:PXButton ID="Add2OrderCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="HistoryPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="EstimateHistoryForm" Caption="Add Comment" CaptionVisible="True" Key="HistoryFilterRecord"
        DesignView="Content" Height="100px" Width="550px" LoadOnDemand="true">
        <px:PXFormView ID="EstimateHistoryForm" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="HistoryFilterRecord" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXTextEdit ID="edHistoryDescription" runat="server" CommitChanges="True" DataField="Description" 
                    SuppressLabel="True" Width="450px"/>    
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="HistoryButtonPanel" runat="server" SkinID="Buttons">
            <px:PXButton ID="HistoryOKButton" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="HistoryCancelButton" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel runat="server" ID="CopyFromPanel" LoadOnDemand="True" CaptionVisible="True" 
        Caption="Copy From" Key="CopyEstimateFromFilter" AllowResize="True" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="CopyFromForm" >
		<px:PXFormView runat="server" ID="CopyFromForm" SkinID="Transparent" CaptionVisible="False" Width="100%" DataSourceID="ds" 
            DataMember="CopyEstimateFromFilter" >
			<Template>
				<px:PXLayoutRule runat="server" ID="CopyFromPanelrule1" StartColumn="True" LabelsWidth="S" ControlSize="M"  />
				<px:PXDropDown runat="server" CommitChanges="True" DataField="CopyFrom" ID="CopyFromPanelCopyFrom" />
				<px:PXSelector runat="server" DataField="EstimateID" AutoRefresh="True" CommitChanges="True" ID="CopyFromPanelEstimateID" AutoCallBack="True" />
				<px:PXSelector runat="server" DataField="RevisionID" AutoRefresh="True" CommitChanges="True" ID="CopyFromPanelRevisionID" />
				<px:PXSelector runat="server" DataField="BOMID" AutoRefresh="True" CommitChanges="True" ID="CopyFromPanelBOMID" AutoCallBack="True" >
				    <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr">
				        <Layout ColumnsMenu="False" />
				    </GridProperties>
				</px:PXSelector>
				<px:PXSelector runat="server" DataField="BOMRevisionID" AutoRefresh="True" CommitChanges="True" ID="CopyFromPanelBOMRevisionID" AutoCallBack="True" />
                <px:PXSelector CommitChanges="True" ID="CopyFromPanelOrderType" runat="server" DataField="OrderType" AllowEdit="True" AutoCallBack="True" />
				<px:PXSelector runat="server" DataField="ProdOrdID" ID="CopyFromPanelProdOrdID" AutoRefresh="True" CommitChanges="True" />
                <px:PXLayoutRule runat="server" ID="CopyFromPanelRule2" GroupCaption="OPTIONS" LabelsWidth="S" ControlSize="M" />
                <px:PXCheckBox ID="edOverrideInventoryID" runat="server" DataField="OverrideInventoryID" CommitChanges="True" />
			    <px:PXLabel ID="WarningNotice" runat="server" Width="350px" Height="30px" Text="Note: This process will replace the current estimate details with the new source details." />
				</Template></px:PXFormView>
		<px:PXPanel runat="server" ID="CopyFromButtonPanel" SkinID="Buttons">
			<px:PXButton runat="server" ID="CopyFromButton1" Text="Copy" DialogResult="OK" CommandSourceID="ds" />
			<px:PXButton runat="server" ID="CopyFromButton2" Text="Cancel" DialogResult="Cancel" CommandSourceID="ds" /></px:PXPanel></px:PXSmartPanel>
</asp:Content>
