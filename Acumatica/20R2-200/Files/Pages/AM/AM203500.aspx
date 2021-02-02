<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM203500.aspx.cs" Inherits="Page_AM203500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="Features" SuspendUnloading="False" TypeName="PX.Objects.AM.FeatureMaint">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Features">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="S" StartColumn="True"/>
		    <px:PXSelector ID="edFeatureID" runat="server" DataField="FeatureID"/>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="S" SuppressLabel="True"/>
            <px:PXCheckBox ID="edActiveFlg" runat="server" DataField="ActiveFlg" Text="Active"/>
            <px:PXCheckBox ID="edAllowNonInventoryOptions" runat="server" DataField="AllowNonInventoryOptions" Text="Allow Non-Inventory Options" CommitChanges="true"/>
            <px:PXCheckBox ID="edDisplayOptionAttributes" runat="server" DataField="DisplayOptionAttributes" Text="Display Option Attributes"/>
            <px:PXCheckBox ID="edPrintResults" runat="server" DataField="PrintResults"/>
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
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Options" LoadOnDemand="true" RepaintOnDemand="true">
			    <Template>
                    <px:PXGrid ID="PXGridFeatureOptions" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AutoAdjustColumns="true" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="FeatureID,LineNbr" DataMember="FeatureOptions">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edOLineNbr" runat="server" DataField="LineNbr"/>
                                    <px:PXSegmentMask ID="edOInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edOSubItemID" runat="server" DataField="SubItemID" />
                                    <px:PXTextEdit ID="edOLabel" runat="server" DataField="Label"/>
                                    <px:PXTextEdit ID="edODescr" runat="server" DataField="Descr"/>
                                    <px:PXCheckBox ID="edOFixedInclude" runat="server" DataField="FixedInclude"/>
                                    <px:PXCheckBox ID="edOQtyEnabled" runat="server" DataField="QtyEnabled"/>
                                    <pxa:PXFormulaCombo ID="edQtyRequired" runat="server" DataField="QtyRequired" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                        IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" />
                                    <px:PXTextEdit ID="edOUOM" runat="server" DataField="UOM"/>
                                    <pxa:PXFormulaCombo ID="edMinQty" runat="server" DataField="MinQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                        IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" />
                                    <pxa:PXFormulaCombo ID="edMaxQty" runat="server" DataField="MaxQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                        IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" />
                                    <pxa:PXFormulaCombo ID="edLotQty" runat="server" DataField="LotQty" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                        IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" />
                                    <pxa:PXFormulaCombo ID="edScrapFactor" runat="server" DataField="ScrapFactor" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                        IsExternalVisible="false" OnRootFieldsNeeded="edFormulaExpression"
                                        SkinID="GI" />
                                    <px:PXCheckBox ID="edBFlush" runat="server" DataField="BFlush"/>
                                    <px:PXDropDown ID="edOMaterialType" runat="server" DataField="MaterialType" CommitChanges="True" />
                                    <px:PXDropDown ID="edOPhantomRouting" runat="server" DataField="PhantomRouting"/>
                                    <px:PXTextEdit ID="edOPriceFactor" runat="server" DataField="PriceFactor"/>
                                    <px:PXCheckBox ID="edOResultsCopy" runat="server" DataField="ResultsCopy"/>
                                    <px:PXCheckBox ID="edQtyRoundUp" runat="server" DataField="QtyRoundUp"/>
                                    <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize"/>
                                    <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" CommitChanges="True" />
                                    <px:PXCheckBox ID="edOPrintResults" runat="server" DataField="PrintResults"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="FeatureID"/>
                                    <px:PXGridColumn DataField="LineNbr"/>
                                    <px:PXGridColumn DataField="Label" Width="120px"/>
                                    <px:PXGridColumn DataField="InventoryID" Width="100px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="SubItemID" />
                                    <px:PXGridColumn DataField="Descr" Width="200px"/>
                                    <px:PXGridColumn DataField="FixedInclude" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                    <px:PXGridColumn DataField="QtyEnabled" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                    <px:PXGridColumn DataField="QtyRequired" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM"/>
                                    <px:PXGridColumn DataField="MinQty"/>
                                    <px:PXGridColumn DataField="MaxQty"/>
                                    <px:PXGridColumn DataField="LotQty"/>
                                    <px:PXGridColumn DataField="ScrapFactor" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="70px"/>
                                    <px:PXGridColumn DataField="MaterialType" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="PhantomRouting" />
                                    <px:PXGridColumn DataField="PriceFactor" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="ResultsCopy" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                    <px:PXGridColumn DataField="QtyRoundUp" TextAlign="Center" Type="CheckBox" Width="85px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="BatchSize" Width="100px" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubcontractSource" Width="95px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="PrintResults" TextAlign="Center" Type="CheckBox" Width="70px"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowUpload="True"/>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
			    <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AutoAdjustColumns="true" SyncPosition="true">
                        <ClientEvents CellEditorCreated="gridCellEditorCreated" BeforeCellEdit="gridBeforeCellEdit" />
                        <Levels>
                            <px:PXGridLevel DataKeyNames="FeatureID,LineNbr" DataMember="FeatureAttributes">
                                <RowTemplate>
                                    <pxa:PXFormulaCombo ID="fAttributeValue" runat="server" DataField="Value" EditButton="True" SelectButton="False" FieldsAutoRefresh="True"
                                        FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
                                        IsExternalVisible="false" OnRootFieldsNeeded="edFormulaAttributeExpression"
                                        SkinID="GI" Enabled="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="FeatureID" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="AttributeID" CommitChanges="true"/>
	                                <px:PXGridColumn DataField="IsFormula" Width="1px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Label" Width="120px"/>
                                    <px:PXGridColumn DataField="Variable"/>
                                    <px:PXGridColumn DataField="Descr" Width="200px"/>
                                    <px:PXGridColumn DataField="Enabled" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                    <px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                    <px:PXGridColumn DataField="Visible" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                    <px:PXGridColumn DataField="Value" Width="200px" MatrixMode="True"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" AllowUpload="True" />
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
