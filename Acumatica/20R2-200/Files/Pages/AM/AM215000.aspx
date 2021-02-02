<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
ValidateRequest="false" CodeFile="AM215000.aspx.cs" Inherits="Page_AM215000"
Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" TypeName="PX.Objects.AM.ECOMaint" PrimaryView="Documents"
                     Visible="True" Width="100%" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="ECO" DataMember="Documents" DataSourceID="ds" 
                   NotifyIndicator="True" NoteIndicator="true" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edECOID">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edECOID" runat="server" DataField="ECOID" FilterByAllFields="True" />
            <px:PXTextEdit ID="edRevisionID" runat="server" DataField="RevisionID" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" />
            <px:PXTextEdit ID="edBOMID" runat="server" DataField="BOMID" AllowEdit="True" />
            <px:PXSelector ID="edBOMRevisionID" runat="server" DataField="BOMRevisionID" CommitChanges="True" AutoRefresh="True" AllowEdit="True" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" Width="50px" CommitChanges="True" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="true" LabelsWidth="S" ControlSize="L"/>
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edRequestDate" runat="server" DataField="RequestDate"/>
            <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" LabelsWidth="S" ControlSize="L"/>
            <px:PXSelector ID="edRequestor" runat="server" AutoRefresh="True" DataField="Requestor" FilterByAllFields="True" />
            <px:PXNumberEdit ID="edPriority" runat="server" AllowNull="False" DataField="Priority" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="600px" Panel1MinSize="100" Panel2MinSize="100">
<AutoSize Container="Window" Enabled="True" MinHeight="300" />
<Template1>
    <px:PXTab ID="opertab" runat="server" Width="100%" Height="100%" DataSourceID="ds" DataMember="CurrentDocument">
        <Items>
            <px:PXTabItem Text="Operations" LoadOnDemand="True" RepaintOnDemand="True">
                    <Template>
                        <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" AutoAdjustColumns="True" SyncPosition="true"
                            OnRowDataBound="AMBomOper_RowDataBound">
                            <Levels>
                                <px:PXGridLevel DataMember="BomOperRecords" >
                                    <RowTemplate>
                                        <px:PXMaskEdit ID="edOperationCD" runat="server" DataField="OperationCD" />
                                        <px:PXSelector ID="edWcID" runat="server" DataField="WcID" AllowEdit="True" />
                                        <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" MaxLength="60" />
                                        <px:PXMaskEdit ID="edSetupTime" runat="server" DataField="SetupTime" CommitChanges="True" />
                                        <px:PXNumberEdit ID="edRunUnits" runat="server" DataField="RunUnits" CommitChanges="True" />
                                        <px:PXMaskEdit ID="edRunUnitTime" runat="server" DataField="RunUnitTime" CommitChanges="True" />
                                        <px:PXNumberEdit ID="edMachineUnits" runat="server" DataField="MachineUnits" CommitChanges="True" />
                                        <px:PXMaskEdit ID="edMachineUnitTime" runat="server" DataField="MachineUnitTime" CommitChanges="True" />
                                        <px:PXMaskEdit ID="edQueueTime" runat="server" DataField="QueueTime" CommitChanges="True" />
                                        <px:PXDropDown ID="edScrapAction" runat="server" DataField="ScrapAction" />
                                        <px:PXDropDown ID="edOperRowStatus" runat="server" DataField="RowStatus" />
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn DataField="OperationID" Width="70px" />
                                        <px:PXGridColumn DataField="OperationCD" Width="70px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="WcID" Width="81px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="Descr" MaxLength="60" Width="250px" />
                                        <px:PXGridColumn DataField="SetupTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="RunUnits" TextAlign="Right" Width="90px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="RunUnitTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="MachineUnits" TextAlign="Right" Width="90px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="MachineUnitTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                                        <px:PXGridColumn DataField="QueueTime" TextAlign="Right" Width="99px" />
                                        <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn DataField="ScrapAction" Width="80px" MaxLength="1" RenderEditorText="True" TextAlign="Left" />
                                        <px:PXGridColumn DataField="RowStatus" />
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                            <Mode AllowUpload="True"/>
                            <AutoSize Enabled="True" />
                            <ActionBar ActionsText="False">
                            </ActionBar>
                            <AutoCallBack Command="Refresh" Target="gridmatl" ActiveBehavior="true">
                                <Behavior RepaintControlsIDs="gridmatl,gridStep,gridTool,gridOvhd,outsideProcessingform" />
                            </AutoCallBack>
                        </px:PXGrid>
                </Template>
                </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="DetailsInTab" AutoAdjustColumns="True" SyncPosition="true"
                    AllowPaging="True" Height="250px" Width="1070px" OnRowDataBound="AMBomAttribute_RowDataBound">
                    <Levels>
                        <px:PXGridLevel DataKeyNames="BOMID,RevisionID,LineNbr" DataMember="BomAttributes" >
                            <RowTemplate>
                                <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" CommitChanges="True" AutoRefresh="True" FilterByAllFields="True"/>
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="LineNbr"/>
                                <px:PXGridColumn DataField="Level" TextAlign="Left" Width="80px" />
                                <px:PXGridColumn DataField="AttributeID" Width="120px" CommitChanges="true" />
                                <px:PXGridColumn DataField="OperationID" Width="75px" CommitChanges="true"/>
                                <px:PXGridColumn DataField="Label" Width="120px" CommitChanges="true" />
                                <px:PXGridColumn DataField="Descr" Width="200px" />
                                <px:PXGridColumn DataField="Enabled" TextAlign="Center" Type="CheckBox" Width="75px" />
                                <px:PXGridColumn DataField="TransactionRequired" TextAlign="Center" Type="CheckBox" Width="85px" />
                                <px:PXGridColumn DataField="Value" Width="220px" MatrixMode="True" />
                                <px:PXGridColumn DataField="OrderFunction" Width="90px" />
                                <px:PXGridColumn DataField="RowStatus" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" Container="Parent" />
                    <mode AllowUpload="True" />
                </px:PXGrid>
                </Template>
            </px:PXTabItem>
                <px:PXTabItem Text="Approval Details">
                    <Template>
                      <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" Style="left: 0px;
                            top: 0px;">
                            <AutoSize Enabled="True" />
                            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                            <Levels>
                                <px:PXGridLevel DataMember="Approval">
                                    <Columns>
                                        <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                        <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                        <px:PXGridColumn DataField="WorkgroupID" />
                                        <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                        <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                        <px:PXGridColumn DataField="ApproveDate" />
                                        <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                        <px:PXGridColumn DataField="Reason" AllowUpdate="False" Width="160px" />
                                        <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" />
                                        <px:PXGridColumn DataField="RuleID" Visible="false" />
                                        <px:PXGridColumn DataField="StepID" Visible="false"/>
                                        <px:PXGridColumn DataField="CreatedDateTime" Visible="false"/>
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                        </px:PXGrid>
                    </Template>
                </px:PXTabItem>
            </Items>
        </px:PXTab>
</Template1>
<Template2>
<px:PXTab ID="tab" runat="server" Width="100%" Height="100%">
<Items>
    <px:PXTabItem Text="Materials" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridMatl" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="True" OnRowDataBound="AMBomMatl_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomMatlRecords">
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                            <px:PXTextEdit ID="edDescrMat" runat="server" DataField="Descr" MaxLength="60" />
                            <px:PXNumberEdit ID="edQtyReq" runat="server" DataField="QtyReq" />
                            <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize" />
                            <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" />
                            <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
                            <px:PXNumberEdit ID="edMatlPlanCost" runat="server" DataField="PlanCost"/>
                            <px:PXDropDown ID="edMaterialType" runat="server" AllowNull="False" DataField="MaterialType" />
                            <px:PXDropDown ID="edPhtmRtngIorE" runat="server" AllowNull="False" DataField="PhantomRouting" />
                            <px:PXCheckBox ID="chkBFlush1" runat="server" DataField="BFlush" />
                            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" CommitChanges="true" />
                            <px:PXSelector ID="edCompBOMID" runat="server" DataField="CompBOMID" AutoRefresh="True" />
                            <px:PXSelector ID="edCompBOMRevisionID" runat="server" DataField="CompBOMRevisionID" AllowEdit="True" AutoRefresh="True" />
                            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                            <px:PXNumberEdit ID="edScrapFactor" runat="server" DataField="ScrapFactor" />
                            <px:PXTextEdit ID="edBubbleNbr" runat="server" DataField="BubbleNbr" />
                            <px:PXDateTimeEdit ID="edEffDate" runat="server" DataField="EffDate" />
                            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXDateTimeEdit ID="edExpDate" runat="server" DataField="ExpDate" />
                            <px:PXMaskEdit ID="edBOMIDmatl" runat="server" DataField="BOMID" />
                            <px:PXTextEdit ID="edOperationIDmatl" runat="server" DataField="OperationID" />
                            <px:PXNumberEdit ID="edLineIDmatl" runat="server" DataField="LineID" />
                            <px:PXDropDown ID="edMatlRowStatus" runat="server" DataField="RowStatus" />
                            <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" CommitChanges="True" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Width="54px" />
                            <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                            <px:PXGridColumn DataField="InventoryID" Width="130px" AutoCallBack="True" />
                            <px:PXGridColumn DataField="SubItemID" Width="81px" />
                            <px:PXGridColumn DataField="Descr" MaxLength="255" Width="200px" />
                            <px:PXGridColumn DataField="QtyReq" TextAlign="Right" Width="108px" AutoCallBack="True" />                            
                            <px:PXGridColumn DataField="BatchSize" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="UOM" Width="81px" AutoCallBack="True" />
                            <px:PXGridColumn DataField="UnitCost" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="PlanCost" TextAlign="Right" Width="100px" /> 
                            <px:PXGridColumn DataField="MaterialType" TextAlign="Left" Width="100px" CommitChanges="true" AutoCallBack="True" />
                            <px:PXGridColumn DataField="PhantomRouting" TextAlign="Left" Width="110px" />
                            <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="85px" />
                            <px:PXGridColumn DataField="SiteID" TextAlign="Left" Width="130px" CommitChanges="true" />
                            <px:PXGridColumn DataField="CompBOMID" LinkCommand="ViewCompBomID" CommitChanges="true" />
                            <px:PXGridColumn DataField="CompBOMRevisionID" Width="85px" />
                            <px:PXGridColumn DataField="LocationID" TextAlign="Right" Width="130px" />
                            <px:PXGridColumn DataField="ScrapFactor" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="BubbleNbr" Width="90px" />
                            <px:PXGridColumn DataField="EffDate" Width="85px" />
                            <px:PXGridColumn DataField="ExpDate" Width="85px" />
                            <px:PXGridColumn DataField="RowStatus" />
                            <px:PXGridColumn DataField="SubcontractSource" Width="95px" AutoCallBack="True" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowUpload="True" AllowDragRows="true"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                    <CustomItems>
                        <px:PXToolBarButton Text="Reference Designators" PopupPanel="PanelRef" Enabled="true">
                        </px:PXToolBarButton>
                        <px:PXToolBarButton DependOnGrid="gridMatl" Key="cmdResetOrder">
                            <AutoCallBack Command="ResetOrder" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
                            <AutoCallBack Target="gridMatl" Command="AddNew" Argument="1"></AutoCallBack>
                            <ActionBar ToolBarVisible="External" MenuVisible="true" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Cut Row" SyncText="false" ImageSet="main" ImageKey="Copy">
                            <AutoCallBack Target="gridMatl" Command="Copy"></AutoCallBack>
                            <ActionBar ToolBarVisible="External" MenuVisible="true" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Insert Cut Row" SyncText="false" ImageSet="main" ImageKey="Paste">
                            <AutoCallBack Target="gridMatl" Command="Paste"></AutoCallBack>
                            <ActionBar ToolBarVisible="External" MenuVisible="true" />
                        </px:PXToolBarButton>
                    </CustomItems>
                </ActionBar>
                <AutoCallBack Enabled="True" Command="Refresh" Target="gridref">
                </AutoCallBack>
                <Parameters>
					<px:PXSyncGridParam ControlID="gridMatl" />
				</Parameters>
                <ActionBar ActionsText="False"></ActionBar>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Steps" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridStep" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab" OnRowDataBound="AMBomStep_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomStepRecords" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" Merge="true" />
                            <px:PXLabel Size="xs" ID="lblStepDescr" runat="server" Encode="True">Descr:</px:PXLabel>
                            <px:PXTextEdit Size="xl" ID="edStepDescr" runat="server" AllowNull="False" DataField="Descr" MaxLength="60" />
                            <px:PXDropDown ID="edStepRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn AllowNull="False" DataField="Descr" MaxLength="60" Width="351px" />
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowUpload="True"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                </ActionBar>
                <AutoCallBack>
                </AutoCallBack>
                <Parameters>
                    <px:PXSyncGridParam ControlID="grid" />
                </Parameters>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Tools" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridTool" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab" OnRowDataBound="AMBomTool_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomToolRecords" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSelector ID="edToolID" runat="server" DataField="ToolID" DataKeyNames="ToolID" 
                                           DataSourceID="ds" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                            <px:PXTextEdit ID="edToolDescr" runat="server" DataField="AMToolMst__Descr" MaxLength="60" />
                            <px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
                            <px:PXLabel Size="xs" ID="lblToolQtyReq" runat="server" Encode="True">Qty Required:</px:PXLabel>
                            <px:PXNumberEdit Size="s" ID="edToolQtyReq" runat="server" DataField="QtyReq" />
                            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" />
                            <px:PXLabel Size="xs" ID="lblToolUnitCost" runat="server" Encode="True">Unit Cost:</px:PXLabel>
                            <px:PXNumberEdit Size="s" ID="edToolUnitCost" runat="server" DataField="UnitCost" />
                            <px:PXDropDown ID="edToolRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn AllowNull="False" DataField="ToolID" MaxLength="30" Width="180px" AutoCallBack="True" />
                            <px:PXGridColumn AllowNull="False" DataField="Descr" MaxLength="60" Width="351px" />
                            <px:PXGridColumn AllowNull="False" DataField="QtyReq" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowUpload="True"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                </ActionBar>
                <AutoCallBack>
                </AutoCallBack>
                <Parameters>
                    <px:PXSyncGridParam ControlID="grid" />
                </Parameters>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Overhead" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridOvhd" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab" OnRowDataBound="AMBomOvhd_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomOvhdRecords" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSelector ID="edOvhdID" runat="server" AllowNull="False" DataField="OvhdID" DataKeyNames="OvhdID" AllowEdit="True" />
                            <px:PXTextEdit ID="edAMOverhead__Descr" runat="server" AllowNull="False" DataField="AMOverhead__Descr" MaxLength="60" />
                            <px:PXDropDown ID="edAMOverhead__OvhdType" runat="server" AllowNull="False" DataField="AMOverhead__OvhdType" />
                            <px:PXNumberEdit ID="edOFactor" runat="server" DataField="OFactor" />
                            <px:PXDropDown ID="edOvhdRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn AllowNull="False" DataField="OvhdID" MaxLength="10" Width="81px" AutoCallBack="True" DisplayFormat="&gt;AAAAAAAAAA" />
                            <px:PXGridColumn DataField="AMOverhead__Descr" AllowNull="False" MaxLength="60" Width="351px" />
                            <px:PXGridColumn AllowNull="False" DataField="AMOverhead__OvhdType" Width="198px" MaxLength="1" RenderEditorText="True" />
                            <px:PXGridColumn DataField="OFactor" AllowNull="False" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowUpload="True"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                </ActionBar>
                <AutoCallBack>
                </AutoCallBack>
                <Parameters>
                    <px:PXSyncGridParam ControlID="grid" />
                </Parameters>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Outside Process" LoadOnDemand="True" RepaintOnDemand="True">
        <Template >
            <px:PXFormView ID="outsideProcessingform" runat="server" CaptionVisible="False" DataMember="OutsideProcessingOperationSelected" DataSourceID="ds" 
                Width="100%" SyncPosition="True" SkinID="Transparent">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule25" runat="server" GroupCaption="General Settings" StartColumn="True" LabelsWidth="125px" />
                    <px:PXCheckBox ID="edOutsideProcess" runat="server" DataField="OutsideProcess" CommitChanges="True" />
                    <px:PXCheckBox ID="edDropShippedToVendor" runat="server" DataField="DropShippedToVendor" CommitChanges="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
                </Template>
            </px:PXFormView>
        </Template>
    </px:PXTabItem>
</Items>
<AutoSize Enabled="True" />
</px:PXTab>
</Template2>
</px:PXSplitContainer>
<px:PXSmartPanel ID="PanelRef" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="gridref" Caption="Reference Designators" CaptionVisible="True"
		DesignView="Content" Height="350px" Width="600px" LoadOnDemand="true">
		<px:PXGrid ID="gridref" runat="server" DataSourceID="ds" SkinID="DetailsInTab" MatrixMode="true" Width="100%" Height="100%" OnRowDataBound="AMBomRef_RowDataBound">
			<Levels>
				<px:PXGridLevel DataKeyNames="BOMID,OperationID,MatlLineID,LineID,EffStartDate" DataMember="BomRefRecords">
					<RowTemplate>
						<px:PXTextEdit ID="edRefDes" runat="server" DataField="RefDes" CommitChanges="true" />
						<px:PXTextEdit ID="edDescrRef" runat="server" DataField="Descr" CommitChanges="true" />
					</RowTemplate>
					<Columns>
						<px:PXGridColumn DataField="RefDes" Width="120px" CommitChanges="true" />
                        <px:PXGridColumn DataField="Descr" Width="200px" CommitChanges="true" />
                        <px:PXGridColumn DataField="BOMID" />
                        <px:PXGridColumn DataField="OperationID" />
                        <px:PXGridColumn DataField="LineID" TextAlign="Right" />
                        <px:PXGridColumn DataField="MatlLineID" TextAlign="Right" />
                        <px:PXGridColumn DataField="RowStatus" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" />
			<ActionBar ActionsText="False">
			</ActionBar>
			<Parameters>
				<px:PXSyncGridParam ControlID="gridmatl" />
			</Parameters>
		</px:PXGrid>
		<px:PXPanel ID="PXPanelRefDesBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>