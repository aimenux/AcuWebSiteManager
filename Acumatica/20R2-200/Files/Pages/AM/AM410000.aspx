<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="AM410000.aspx.cs" Inherits="Page_AM410000" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AM.BOMCompareInq" PrimaryView="Filter">

        <DataTrees>
            <px:PXTreeDataMember TreeView="Tree1" TreeKeys="ParentID, LineNbr, CategoryNbr, DetailLineNbr" />
            <px:PXTreeDataMember TreeView="Tree2" TreeKeys="ParentID, LineNbr, CategoryNbr, DetailLineNbr" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
<style type="text/css">
        .HighlightText {
            background-color: yellow !important;
        }
    </style>   
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="BOM of Material" DataMember="Filter" DataSourceID="ds" 
                   NotifyIndicator="True" NoteIndicator="true" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edBOMID1" >
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" ColumnWidth="500px"/>
            <px:PXDropDown ID="edIDType1" runat="server" CommitChanges="true" DataField="IDType1" />
            <px:PXSelector ID="edBOMID1" runat="server" AutoRefresh="True" DataField="BOMID1" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            <px:PXSelector ID="edRevisionID1" runat="server" DataField="RevisionID1" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" AllowEdit="True" />
            <px:PXSelector ID="edECRID1" runat="server" AutoRefresh="True" DataField="ECRID1" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            <px:PXSelector ID="edECOID1" runat="server" AutoRefresh="True" DataField="ECOID1" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown ID="edIDType2" runat="server" CommitChanges="true" DataField="IDType2" />
            <px:PXSelector ID="edBOMID2" runat="server" AutoRefresh="True" DataField="BOMID2" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            <px:PXSelector ID="edRevisionID2" runat="server" DataField="RevisionID2" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" AllowEdit="True" />
            <px:PXSelector ID="edECRID2" runat="server" AutoRefresh="True" DataField="ECRID2" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            <px:PXSelector ID="edECOID2" runat="server" AutoRefresh="True" DataField="ECOID2" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            </Template>
    </px:PXFormView>
    </asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="400" SkinID="Horizontal">
    <AutoSize Enabled="true" Container="Window" />
    <Template1>
    <px:PXSplitContainer runat="server" ID="sp2" SplitterPosition="500">
    <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXTreeView ID="edTree1" runat="server" DataSourceID="ds" PopulateOnDemand="True" RootNodeText="BOM" ExpandDepth="4" SelectFirstNode="true" 
                ShowRootNode="false" Caption="Features" CaptionVisible="false" AllowCollapse="true"  DataMember="Tree1" AutoRepaint="true">
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="Tree1" TextField="Label" ValueField="DetailLineNbr" ImageUrlField="Icon" ToolTipField="ToolTip" />
                </DataBindings>
                <AutoCallBack Command="Refresh" Target="gridMatl" ActiveBehavior="true">
                    <Behavior RepaintControlsIDs="gridMatl,gridStep,gridTool,gridOvhd" />
                </AutoCallBack>
                <AutoSize Enabled="True" />
            </px:PXTreeView>
            </Template1>
        <Template2>
            <px:PXTreeView ID="edTree2" runat="server" DataSourceID="ds" PopulateOnDemand="True" RootNodeText="BOM" ExpandDepth="4" SelectFirstNode="true"
                ShowRootNode="false" Caption="Features" CaptionVisible="false" AllowCollapse="False"  DataMember="Tree2" AutoRepaint="true">
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="Tree2" TextField="Label" ValueField="DetailLineNbr" ImageUrlField="Icon" ToolTipField="ToolTip" />
                </DataBindings>
                <AutoCallBack Command="Refresh" Target="gridMatl" ActiveBehavior="true">
                    <Behavior RepaintControlsIDs="gridMatl,gridStep,gridTool,gridOvhd" />
                </AutoCallBack>
                <AutoSize Enabled="True" />
            </px:PXTreeView>
        </Template2>
        </px:PXSplitContainer>
    </Template1>
        <Template2>
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%">
<Items>
    <px:PXTabItem Text="Materials" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridMatl" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="True" OnRowDataBound="AMBomMatl_RowDataBound" >
                <Levels>
                    <px:PXGridLevel DataMember="BomMatlRecords" SortOrder="Selected">
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXTextEdit ID="edBOMIDmatl" runat="server" DataField="BOMID"  />
                            <px:PXTextEdit ID="edRevisionIDmatl" runat="server" DataField="RevisionID"  />
                            <px:PXTextEdit ID="edOperationIDmatl" runat="server" DataField="OperationID"  />
                            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID"  />
                            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                            <px:PXTextEdit ID="edDescrMat" runat="server" DataField="Descr" />
                            <px:PXNumberEdit ID="edQtyReq" runat="server" DataField="QtyReq" />
                            <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize" />
                            <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" />
                            <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
                            <px:PXNumberEdit ID="edMatlPlanCost" runat="server" DataField="PlanCost"/>
                            <px:PXDropDown ID="edMaterialType" runat="server" AllowNull="False" DataField="MaterialType" />
                            <px:PXDropDown ID="edPhtmRtngIorE" runat="server" AllowNull="False" DataField="PhantomRouting" />
                            <px:PXCheckBox ID="chkBFlush1" runat="server" DataField="BFlush" />
                            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" />
                            <px:PXTextEdit ID="edCompBOMID" runat="server" DataField="CompBOMID" AutoRefresh="True" />
                            <px:PXTextEdit ID="edCompBOMRevisionID" runat="server" DataField="CompBOMRevisionID" AutoRefresh="True" />
                            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                            <px:PXNumberEdit ID="edScrapFactor" runat="server" DataField="ScrapFactor" />
                            <px:PXTextEdit ID="edBubbleNbr" runat="server" DataField="BubbleNbr" />
                            <px:PXDateTimeEdit ID="edEffDate" runat="server" DataField="EffDate" />
                            <px:PXDateTimeEdit ID="edExpDate" runat="server" DataField="ExpDate" />
                            <px:PXNumberEdit ID="edLineIDmatl" runat="server" DataField="LineID" />
                            <px:PXDropDown ID="edMatlRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Width="54px" />
                            <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                            <px:PXGridColumn DataField="BOMID" Width="130px" />
                            <px:PXGridColumn DataField="RevisionID" Width="130px" />
                            <px:PXGridColumn DataField="InventoryID" Width="130px" />
                            <px:PXGridColumn DataField="OperationID" Width="81px" />
                            <px:PXGridColumn DataField="SubItemID" Width="81px" />
                            <px:PXGridColumn DataField="Descr" MaxLength="255" Width="200px" />
                            <px:PXGridColumn DataField="QtyReq" TextAlign="Right" Width="108px" />                            
                            <px:PXGridColumn DataField="BatchSize" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="UOM" Width="81px" AutoCallBack="True" />
                            <px:PXGridColumn DataField="UnitCost" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="PlanCost" TextAlign="Right" Width="100px" /> 
                            <px:PXGridColumn DataField="MaterialType" TextAlign="Left" Width="100px" />
                            <px:PXGridColumn DataField="PhantomRouting" TextAlign="Left" Width="110px" />
                            <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="85px" />
                            <px:PXGridColumn DataField="SiteID" TextAlign="Left" Width="130px" />
                            <px:PXGridColumn DataField="CompBOMID" />
                            <px:PXGridColumn DataField="CompBOMRevisionID" Width="85px" />
                            <px:PXGridColumn DataField="LocationID" TextAlign="Right" Width="130px" />
                            <px:PXGridColumn DataField="ScrapFactor" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="BubbleNbr" Width="90px" />
                            <px:PXGridColumn DataField="EffDate" Width="85px" />
                            <px:PXGridColumn DataField="ExpDate" Width="85px" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Parameters>
                    <px:PXControlParam ControlID="edTree1" Name="detailLineNbr1" PropertyName="SelectedValue" />
                    <px:PXControlParam ControlID="edTree2" Name="detailLineNbr2" PropertyName="SelectedValue" />
                </Parameters>
                <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False"></ActionBar>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Steps" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridStep" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab" OnRowDataBound="AMBomStep_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomStepRecords" SortOrder="Selected" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" Merge="true" />
                            <px:PXTextEdit ID="edsBOMID" runat="server" DataField="BOMID" />
                            <px:PXTextEdit ID="edsRevisionID" runat="server" DataField="RevisionID" />
                            <px:PXTextEdit ID="edsOperationID" runat="server" DataField="OperationID" />
                            <px:PXTextEdit Size="xl" ID="edStepDescr" runat="server" AllowNull="False" DataField="Descr" />
                            <px:PXDropDown ID="edStepRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="BOMID" Width="130px" />
                            <px:PXGridColumn DataField="RevisionID" Width="130px" />
                            <px:PXGridColumn DataField="OperationID" Width="81px" />
                            <px:PXGridColumn AllowNull="False" DataField="Descr" Width="351px" />
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Parameters>
                    <px:PXControlParam ControlID="edTree1" Name="detailLineNbr1" PropertyName="SelectedValue" />
                    <px:PXControlParam ControlID="edTree2" Name="detailLineNbr2" PropertyName="SelectedValue" />
                </Parameters>
                <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                </ActionBar>
                <AutoCallBack>
                </AutoCallBack>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Tools" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridTool" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab" OnRowDataBound="AMBomTool_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomToolRecords" SortOrder="Selected" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXTextEdit ID="edtBOMID" runat="server" DataField="BOMID"  />
                            <px:PXTextEdit ID="edtRevisionID" runat="server" DataField="RevisionID"  />
                            <px:PXTextEdit ID="edtOperationID" runat="server" DataField="OperationID"  />
                            <px:PXSelector ID="edToolID" runat="server" DataField="ToolID" DataKeyNames="ToolID" DataSourceID="ds" AllowEdit="True"/>
                            <px:PXTextEdit ID="edToolDescr" runat="server" DataField="AMToolMst__Descr" />
                            <px:PXNumberEdit Size="s" ID="edToolQtyReq" runat="server" DataField="QtyReq" />
                            <px:PXNumberEdit Size="s" ID="edToolUnitCost" runat="server" DataField="UnitCost" />
                            <px:PXDropDown ID="edToolRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="BOMID" Width="130px" />
                            <px:PXGridColumn DataField="RevisionID" Width="130px" />
                            <px:PXGridColumn DataField="OperationID" Width="81px" />
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn AllowNull="False" DataField="ToolID" Width="180px" />
                            <px:PXGridColumn AllowNull="False" DataField="Descr" Width="351px" />
                            <px:PXGridColumn AllowNull="False" DataField="QtyReq" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Parameters>
                    <px:PXControlParam ControlID="edTree1" Name="detailLineNbr1" PropertyName="SelectedValue" />
                    <px:PXControlParam ControlID="edTree2" Name="detailLineNbr2" PropertyName="SelectedValue" />
                </Parameters>
                <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                </ActionBar>
                <AutoCallBack>
                </AutoCallBack>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Overhead" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridOvhd" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab" OnRowDataBound="AMBomOvhd_RowDataBound">
                <Levels>
                    <px:PXGridLevel DataMember="BomOvhdRecords" SortOrder="Selected">
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXTextEdit ID="edoBOMID" runat="server" DataField="BOMID"  />
                            <px:PXTextEdit ID="edoRevisionID" runat="server" DataField="RevisionID"  />
                            <px:PXTextEdit ID="edoOperationID" runat="server" DataField="OperationID"  />
                            <px:PXSelector ID="edOvhdID" runat="server" AllowNull="False" DataField="OvhdID" DataKeyNames="OvhdID" AllowEdit="True" />
                            <px:PXTextEdit ID="edAMOverhead__Descr" runat="server" AllowNull="False" DataField="AMOverhead__Descr" />
                            <px:PXDropDown ID="edAMOverhead__OvhdType" runat="server" AllowNull="False" DataField="AMOverhead__OvhdType" />
                            <px:PXNumberEdit ID="edOFactor" runat="server" DataField="OFactor" />
                            <px:PXDropDown ID="edOvhdRowStatus" runat="server" DataField="RowStatus" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="BOMID" Width="130px" />
                            <px:PXGridColumn DataField="RevisionID" Width="130px" />
                            <px:PXGridColumn DataField="OperationID" Width="81px" />
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn DataField="OvhdID" Width="81px" />
                            <px:PXGridColumn DataField="AMOverhead__Descr" AllowNull="False" Width="351px" />
                            <px:PXGridColumn DataField="AMOverhead__OvhdType" Width="198px" RenderEditorText="True" />
                            <px:PXGridColumn DataField="OFactor" AllowNull="False" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn DataField="RowStatus" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Parameters>
                    <px:PXControlParam ControlID="edTree1" Name="detailLineNbr1" PropertyName="SelectedValue" />
                    <px:PXControlParam ControlID="edTree2" Name="detailLineNbr2" PropertyName="SelectedValue" />
                </Parameters>
                <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false"/>
                <AutoSize Enabled="True" />
                <ActionBar ActionsText="False">
                </ActionBar>
                <AutoCallBack>
                </AutoCallBack>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
</Items>
<AutoSize Enabled="True" />
</px:PXTab>
            </Template2>
        </px:PXSplitContainer>
</asp:Content>