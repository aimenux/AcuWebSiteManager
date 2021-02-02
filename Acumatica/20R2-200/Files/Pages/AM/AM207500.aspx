<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM207500.aspx.cs" Inherits="Page_AM207500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AM.ConfigurationMaint" PrimaryView="Documents">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Documents" TabIndex="3700">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
            <px:PXSelector ID="edConfigurationID" runat="server" DataField="ConfigurationID" AutoRefresh="True" DataSourceID="ds" CommitChanges="true"/>
            <px:PXSelector ID="edRevision" runat="server" DataField="Revision" AutoRefresh="True" DataSourceID="ds" CommitChanges="true"/>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="true"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2"/>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True"/>
            <px:PXSelector ID="edBOMID" runat="server" DataField="BOMID" CommitChanges="True" >
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXSelector ID="edBOMRevisionID" runat="server" DataField="BOMRevisionID" AutoRefresh="True" AllowEdit="True" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
            <px:PXCheckBox ID="chkCompletionRequired" runat="server" DataField="IsCompletionRequired" AllowEdit="True"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <script type="text/javascript">
        function gridCellEditorCreated(n, c) {
            var activeRow = c.cell.row;
            var isFormulaCell = activeRow.getCell("IsFormula");
            var valueCell = activeRow.getCell("Value");
            if (valueCell == c.cell) {
                if (isFormulaCell.getValue()) {

                    //We specify the ID of the PXFormulaCombo so the
                    //framework knows to generate the PXFormulaCombo controls
                    valueCell.column.formEditorID = "fAttributeValue";

                    //Cannot call _getCustomEditor() because private.
                    //getSearchEditor will add the suffix "s" that will be append to 
                    //the key and thus, would not be found by the initial process
                    //if it's already in the cache.
                    valueCell.editor = valueCell.column.getSearchEditor(); 
                }
            }
        }

        function gridBeforeCellEdit(n, c) {
            var valueCell = c.cell.row.getCell("Value");
            if (valueCell == c.cell) {
                //We set to null the formEditorID so the 
                //framework never add our PXFormulaCombo 
                //to the private cache. It will be added
                //in the event gridCellEditorCreated 
                //(after it was initially created) with the
                //function "getSearchEditor" which will append
                //the suffix "s" and thus will not be found
                //in the cache in the first run.
                valueCell.column.formEditorID = null;
            }
        }
    </script>
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="SelectedConfiguration">
        <Items>
            <px:PXTabItem Text="Features" LoadOnDemand="True" RepaintOnDemand="True">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" Orientation="Horizontal" SplitterPosition="10" Panel1MinSize="250">
                        <AutoSize Enabled="True" />
                        <Template1>
                            <px:PXGrid AdjustPageSize="Auto" ID="gridFeature" runat="server" DataSourceID="ds" Width="100%" AutoAdjustColumns="True" SyncPosition="True" SkinID="Details" TabIndex="3100" TemporaryFilterCaption="Filter Applied">
                                <Levels>
                                    <%--<px:PXGridLevel DataKeyNames="ConfigurationID,Revision,LineNbr" DataMember="ConfigurationFeatures">--%>
                                    <px:PXGridLevel DataKeyNames="ConfigurationID,LineNbr" DataMember="ConfigurationFeatures">
                                         <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True"/>
                                            <px:PXTextEdit ID="edFConfigurationID" runat="server" DataField="ConfigurationID"/>
                                            <px:PXTextEdit ID="edFRevision" runat="server" DataField="Revision"/>
                                            <px:PXNumberEdit ID="edFLineNbr" runat="server" DataField="LineNbr"/>
                                            <px:PXSelector ID="edFFeatureID" runat="server" DataField="FeatureID"/>
                                            <px:PXTextEdit ID="edFLabel" runat="server" DataField="Label"/>
                                            <px:PXTextEdit ID="edFDescr" runat="server" DataField="Descr"/>
                                            <px:PXNumberEdit ID="edFSortOrder" runat="server" DataField="SortOrder"/>
                                            <px:PXCheckBox ID="edFVisible" runat="server" DataField="Visible" Text="Visible"/>
                                            <px:PXCheckBox ID="edFResultsCopy" runat="server" DataField="ResultsCopy" Text="Results Copy"/>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />

                                            <pxa:PXFormulaCombo ID="fMinSelection" runat="server" DataField="MinSelection" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" />
                                                             
                                            <pxa:PXFormulaCombo ID="fMaxSelection" runat="server" DataField="MaxSelection" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" />

                                            <pxa:PXFormulaCombo ID="fMinQty" runat="server" DataField="MinQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" />


                                            <pxa:PXFormulaCombo ID="fMaxQty" runat="server" DataField="MaxQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" />

                                            <pxa:PXFormulaCombo ID="fLotQty" runat="server" DataField="LotQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" />
                                            <px:PXCheckBox ID="edPrintResults" runat="server" DataField="PrintResults"/>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="ConfigurationID"/>
                                            <px:PXGridColumn DataField="Revision"/>
                                            <px:PXGridColumn DataField="LineNbr" TextAlign="Right"/>
                                            <px:PXGridColumn DataField="FeatureID" Width="120px" CommitChanges="True"/>
                                            <px:PXGridColumn DataField="Label" Width="120px"/>
                                            <px:PXGridColumn DataField="Descr" Width="200px"/>
                                            <px:PXGridColumn DataField="SortOrder" TextAlign="Right"/>
                                            <px:PXGridColumn DataField="MinSelection" Width="60px"/>
                                            <px:PXGridColumn DataField="MaxSelection" Width="60px"/>
                                            <px:PXGridColumn DataField="MinQty" Width="60px"/>
                                            <px:PXGridColumn DataField="MaxQty" Width="60px"/>
                                            <px:PXGridColumn DataField="LotQty"/>
                                            <px:PXGridColumn DataField="Visible" Width="60px" TextAlign="Center" Type="CheckBox"/>
                                            <px:PXGridColumn DataField="ResultsCopy" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                            <px:PXGridColumn DataField="PrintResults" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True" AllowUpload="True" />
                                <AutoCallBack Command="Refresh" Target="gridOption" ActiveBehavior="true">
                                    <Behavior RepaintControlsIDs="tabFeature,gridOption,gridFeatureRule" />
                                    <%--<Behavior RepaintControls="None" RepaintControlsIDs="OptionsGrid" />--%>
                                </AutoCallBack>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXTab ID="tabFeature" runat="server" Width="100%" Height="100%" DataSourceID="ds">
                            <%--<px:PXTab ID="tab" runat="server" Width="100%" Height="100%">--%>
                                <Items>
                                    <px:PXTabItem Text="Options" RepaintOnDemand="True">
                                        <AutoCallBack Command="Refresh" Target="gridOption" />
                                        <Template>
                                            <px:PXGrid ID="gridOption" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" SyncPosition="True" AutoAdjustColumns="True" TabIndex="8300" TemporaryFilterCaption="Filter Applied" >
                                                <Levels>
                                                    <%--<px:PXGridLevel DataKeyNames="ConfigurationID,Revision,ConfigFeatureLineNbr,LineNbr" DataMember="FeatureOptions">--%>
                                                    <px:PXGridLevel DataKeyNames="ConfigurationID,ConfigFeatureLineNbr,LineNbr" DataMember="FeatureOptions">
                                                        <RowTemplate>
                                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                                            <px:PXTextEdit ID="edOConfigurationID" runat="server" DataField="ConfigurationID"/>
                                                            <px:PXTextEdit ID="edORevision" runat="server" DataField="Revision"/>
                                                            <px:PXNumberEdit ID="edOConfigFeatureLineNbr" runat="server" DataField="ConfigFeatureLineNbr"/>
                                                            <px:PXNumberEdit ID="edOLineNbr" runat="server" DataField="LineNbr"/>
                                                            <px:PXSegmentMask ID="edOInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                                            <px:PXSegmentMask ID="edOSubItemID" runat="server" DataField="SubItemID" />
                                                            <px:PXTextEdit ID="edOLabel" runat="server" DataField="Label"/>
                                                            <px:PXTextEdit ID="edODescr" runat="server" DataField="Descr"/>
                                                            <px:PXCheckBox ID="edOFixedInclude" runat="server" DataField="FixedInclude"/>
                                                            <px:PXCheckBox ID="edOQtyEnabled" runat="server" DataField="QtyEnabled"/>

                                                            <pxa:PXFormulaCombo ID="edQtyRequired" runat="server" DataField="QtyRequired" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" LastNodeName="" />
                                                             
                                                            <px:PXTextEdit ID="edOUOM" runat="server" DataField="UOM"/>
                                                            <px:PXSelector ID="edOOperationID" runat="server" DataField="OperationID"/>

                                                            <pxa:PXFormulaCombo ID="edMinQty" runat="server" DataField="MinQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" LastNodeName="" />

                                                            <pxa:PXFormulaCombo ID="edMaxQty" runat="server" DataField="MaxQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" LastNodeName="" />

                                                            <pxa:PXFormulaCombo ID="edLotQty" runat="server" DataField="LotQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" LastNodeName="" />

                                                            <pxa:PXFormulaCombo ID="edScrapFactor" runat="server" DataField="ScrapFactor" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                                                IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                                                SkinID="GI" ExternalNodeName="" InternalNodeName="" LastNodeName="" />

                                                            <px:PXCheckBox ID="edBFlush" runat="server" DataField="BFlush"/>
                                                            <px:PXDropDown ID="edOMaterialType" runat="server" DataField="MaterialType" CommitChanges="True" />
                                                            <px:PXDropDown ID="edOPhantomRouting" runat="server" DataField="PhantomRouting"/>
                                                            <px:PXSegmentMask ID="edOptionSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
											                <px:PXSegmentMask ID="edOptionLocationID" runat="server" DataField="LocationID" AutoRefresh="True"/>
                                                            <px:PXTextEdit ID="edOPriceFactor" runat="server" DataField="PriceFactor"/>
                                                            <px:PXCheckBox ID="edOResultsCopy" runat="server" DataField="ResultsCopy"/>
                                                            <px:PXCheckBox ID="edQtyRoundUp" runat="server" DataField="QtyRoundUp"/>
                                                            <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize"/>
                                                            <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource"  CommitChanges="True" />
                                                            <px:PXCheckBox ID="edOPrintResults" runat="server" DataField="PrintResults"/>
                                                        </RowTemplate>
                                                        <Columns>
                                                            <px:PXGridColumn DataField="ConfigurationID"/>
                                                            <px:PXGridColumn DataField="Revision"/>
                                                            <px:PXGridColumn DataField="ConfigFeatureLineNbr" TextAlign="Right"/>
                                                            <px:PXGridColumn DataField="LineNbr" TextAlign="Right"/>
                                                            <px:PXGridColumn DataField="Label" Width="120px"/>
                                                            <px:PXGridColumn DataField="InventoryID" Width="100px" CommitChanges="true"/>
                                                            <px:PXGridColumn DataField="SubItemID" Width="100px"/>
                                                            <px:PXGridColumn DataField="Descr" Width="200px"/>
                                                            <px:PXGridColumn DataField="FixedInclude" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                                            <px:PXGridColumn DataField="QtyEnabled" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                                            <px:PXGridColumn DataField="QtyRequired" Width="150px" CommitChanges="True" AutoCallBack="True" />
                                                            <px:PXGridColumn DataField="UOM"/>
                                                            <px:PXGridColumn DataField="OperationID" CommitChanges="True"/>
                                                            <px:PXGridColumn DataField="MinQty"/>
                                                            <px:PXGridColumn DataField="MaxQty"/>
                                                            <px:PXGridColumn DataField="LotQty"/>
                                                            <px:PXGridColumn DataField="ScrapFactor" TextAlign="Right" AutoCallBack="True" />
                                                            <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="70px"/>
                                                            <px:PXGridColumn DataField="MaterialType" AutoCallBack="True"  />
                                                            <px:PXGridColumn DataField="PhantomRouting"/>
                                                            <px:PXGridColumn DataField="SiteID" CommitChanges="True"/>
                                                            <px:PXGridColumn DataField="LocationID"  CommitChanges="True"/>
                                                            <px:PXGridColumn DataField="SortOrder"/>
                                                            <px:PXGridColumn DataField="PriceFactor" TextAlign="Right"/>
                                                            <px:PXGridColumn DataField="ResultsCopy" TextAlign="Center" Type="CheckBox" Width="60px" />
                                                            <px:PXGridColumn DataField="QtyRoundUp" TextAlign="Center" Type="CheckBox" Width="85px" AutoCallBack="True" />
                                                            <px:PXGridColumn DataField="BatchSize" Width="100px" TextAlign="Right" AutoCallBack="True" />
                                                            <px:PXGridColumn DataField="SubcontractSource" Width="95px" AutoCallBack="True" />
                                                            <px:PXGridColumn DataField="PrintResults" TextAlign="Center" Type="CheckBox" Width="70px"/>
                                                        </Columns>
                                                    </px:PXGridLevel>
                                                </Levels>
                                                <AutoSize Enabled="True" />
                                                <Mode InitNewRow="True" AllowUpload="True" />
                                                <Parameters>
                                                    <px:PXSyncGridParam ControlID="gridFeature" />
                                                </Parameters>
                                            </px:PXGrid>
                                        </Template>
                                    </px:PXTabItem>
                                    <px:PXTabItem Text="Rules" RepaintOnDemand="True">
                                        <AutoCallBack Command="Refresh" Target="gridFeatureRule" />
                                        <Template>
                                            <px:PXGrid ID="gridFeatureRule" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" SyncPosition="True" AutoAdjustColumns="true">
                                                <Levels>
                                                    <px:PXGridLevel DataKeyNames="ConfigurationID,SourceLineNbr,LineNbr" DataMember="FeatureRules">
                                                        <RowTemplate>
                                                            <px:PXSelector ID="edSourceOptionLineNbr" runat="server" DataField="SourceOptionLineNbr" DataSourceID="ds">
                                                                <GridProperties FastFilterFields="Label,InventoryID"/>
                                                            </px:PXSelector>
                                                            <px:PXSelector ID="edTargetFeatureLineNbr" runat="server" DataField="TargetFeatureLineNbr" AutoRefresh="True" DataSourceID="ds" CommitChanges="true"/>
                                                            <px:PXSelector ID="edTargetOptionLineNbr" runat="server" DataField="TargetOptionLineNbr" AutoRefresh="True" DataSourceID="ds">
                                                                <GridProperties FastFilterFields="Label,InventoryID"/>
                                                            </px:PXSelector>
                                                        </RowTemplate>
                                                        <Columns>
                                                            <px:PXGridColumn DataField="RuleType" Width="120px" CommitChanges="true"/>
                                                            <px:PXGridColumn DataField="SourceOptionLineNbr" Width="200px"/>
                                                            <px:PXGridColumn DataField="TargetFeatureLineNbr" Width="200px" CommitChanges="true"/>
                                                            <px:PXGridColumn DataField="TargetOptionLineNbr" Width="200px"/>
                                                        </Columns>
                                                    </px:PXGridLevel>
                                                </Levels>
                                                <AutoSize Enabled="True" />
                                                <Mode InitNewRow="True" AllowUpload="True" />
                                                <Parameters>
                                                    <px:PXSyncGridParam ControlID="gridFeature" />
                                                </Parameters>
                                            </px:PXGrid>
                                        </Template>
                                    </px:PXTabItem>
                                </Items>
                                <AutoSize Enabled="True" />
                            </px:PXTab>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <AutoCallBack Command="Refresh" Target="AttributeRulesGrid"/>
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp2" SplitterPosition="200" SkinID="Horizontal" SavePosition="True">
                        <AutoSize Enabled="True" />
                        <Template1>
                            <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AutoAdjustColumns="True" SyncPosition="true">
                                <ClientEvents CellEditorCreated="gridCellEditorCreated" BeforeCellEdit="gridBeforeCellEdit" />
                                <Levels>
                                    <px:PXGridLevel DataKeyNames="ConfigurationID,LineNbr" DataMember="ConfigurationAttributes">
                                        <RowTemplate>
                                            <pxa:PXFormulaCombo ID="fAttributeValue" runat="server" DataField="Value" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                                IsExternalVisible="false" OnRootFieldsNeeded="edFormulaAttributeExpression"
                                                SkinID="GI" Enabled="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="ConfigurationID"/>
                                            <px:PXGridColumn DataField="Revision"/>
                                            <px:PXGridColumn DataField="LineNbr"/>
                                            <px:PXGridColumn DataField="AttributeID" CommitChanges="true"/>
	                                        <px:PXGridColumn DataField="IsFormula" Width="1px" RenderEditorText="True" />
                                            <px:PXGridColumn DataField="Label" Width="100px"/>
                                            <px:PXGridColumn DataField="Variable"/>
                                            <px:PXGridColumn DataField="Descr" Width="200px"/>
                                            <px:PXGridColumn DataField="Enabled" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                            <px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                            <px:PXGridColumn DataField="Visible" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                            <px:PXGridColumn DataField="Value" Width="200px" MatrixMode="true"/>
                                            <px:PXGridColumn DataField="SortOrder"/>
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True" AllowUpload="True" />
                                <AutoCallBack Command="Refresh" Target="AttributeRulesGrid" ActiveBehavior="true">
                                    <Behavior RepaintControlsIDs="tabAttr,AttributeRulesGrid" />
                                </AutoCallBack>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXTab ID="tabAttr" runat="server" Width="100%" Height="100%">
                                <Items>
                                    <px:PXTabItem Text="Rules" LoadOnDemand="True" RepaintOnDemand="True">
                                        <Template>
                                            <px:PXGrid ID="AttributeRulesGrid" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" SyncPosition="True" AutoAdjustColumns="true">
                                                <Levels>
                                                    <px:PXGridLevel DataKeyNames="ConfigurationID,SourceLineNbr,LineNbr" DataMember="AttributeRules">
                                                        <RowTemplate>    
                                                            <pxa:PXFormulaCombo ID="edValue1" runat="server" DataField="Value1" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                    FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                                                    IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                                                    SkinID="GI" />
                                                            <pxa:PXFormulaCombo ID="edValue2" runat="server" DataField="Value2" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                                                    FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                                                    IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                                                    SkinID="GI" />                                                                                                                   
                                                            <px:PXSelector ID="edAttTargetFeatureLineNbr" runat="server" DataField="TargetFeatureLineNbr" AutoRefresh="True" DataSourceID="ds" CommitChanges="true"/>
                                                            <px:PXSelector ID="edAttTargetOptionLineNbr" runat="server" DataField="TargetOptionLineNbr" AutoRefresh="True" DataSourceID="ds">
                                                                <GridProperties FastFilterFields="Label,InventoryID"/>
                                                            </px:PXSelector>                                                        
                                                        </RowTemplate>
                                                        <Columns>
                                                            <px:PXGridColumn DataField="RuleType" Width="120px" CommitChanges="True"/>
                                                            <px:PXGridColumn DataField="Condition" Width="120px" CommitChanges="true"/>
                                                            <px:PXGridColumn DataField="Value1" Width="200px"/>
                                                            <px:PXGridColumn DataField="Value2" Width="200px"/>
                                                            <px:PXGridColumn DataField="TargetFeatureLineNbr" Width="200px" CommitChanges="true"/>
                                                            <px:PXGridColumn DataField="TargetOptionLineNbr" Width="200px"/>
                                                        </Columns>
                                                    </px:PXGridLevel>
                                                </Levels>
                                                <AutoSize Enabled="True" />
                                                <Mode InitNewRow="True" AllowUpload="True" />
                                                <Parameters>
                                                    <px:PXSyncGridParam ControlID="AttributesGrid" />
                                                </Parameters>
                                            </px:PXGrid>
                                        </Template>
                                    </px:PXTabItem>
                                </Items>
                                <AutoSize Enabled="True" />
                            </px:PXTab>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Keys">
                <Template>
                    <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartRow="True" StartColumn="True"/>
                    <px:PXDropDown ID="edKeyFormat" runat="server" DataField="KeyFormat" CommitChanges="true"/>
                    <px:PXSelector ID="edKeyNumberingID" runat="server" DataField="KeyNumberingID"/>
                    <pxa:PXFormulaCombo ID="fKeyEquation" runat="server" DataField="KeyEquation" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                        IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" ExternalNodeName="" InternalNodeName=""/>
                    <px:PXLayoutRule runat="server" ControlSize="XXL" LabelsWidth="SM"/>
                    <pxa:PXFormulaCombo ID="fKeyDescription" runat="server" DataField="KeyDescription" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                        IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" ExternalNodeName="" InternalNodeName="" />
                    <pxa:PXFormulaCombo ID="fTranDescription" runat="server" DataField="TranDescription" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="True" PanelAutoRefresh="True" IsInternalVisible="False"
                                        IsExternalVisible="False" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" ExternalNodeName="" InternalNodeName="" />
                    <px:PXCheckBox ID="edOnTheFlySubItems" runat="server" DataField="OnTheFlySubItems"/>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Price">
                <Template>
                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartRow="True"/>
                    <px:PXDropDown ID="edPriceRollup" runat="server" DataField="PriceRollup"/>
                    <px:PXDropDown ID="edPriceCalc" runat="server" DataField="PriceCalc"/>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
        <px:PXSmartPanel ID="PanelMakeDefault" runat="server" Caption="Set as Default Level" CaptionVisible="True"
		DesignView="Content" LoadOnDemand="true" Key="ConfigurationIDUpdateFilter" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formDefaultLevels" CallBackMode-PostData="Page" Width="175px" Height="125px">
        <px:PXFormView ID="formDefaultLevels" runat="server" DataSourceID="ds" CaptionVisible="False"
			DataMember="ConfigurationIDUpdateFilter" SkinID="Transparent" Width="100%">
			<Template>
                <px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartGroup="True" GroupCaption="" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXCheckBox ID="edCfgIdItem" runat="server" DataField="Item" AlignLeft="True" CommitChanges="True"/>
                <px:PXCheckBox ID="edCfgIdWarehouse" runat="server" DataField="Warehouse" AlignLeft="True" CommitChanges="True" />
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanelMakeDefaultBtn" runat="server" SkinID="Transparent">
            <px:PXButton ID="PXButtonMakeDefaultOk" runat="server" DialogResult="OK" Text="Update" CommandSourceID="ds" />
            <px:PXButton ID="PXButtonMakeDefaultCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
