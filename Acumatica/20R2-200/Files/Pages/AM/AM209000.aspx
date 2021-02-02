<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
ValidateRequest="false" CodeFile="AM209000.aspx.cs" Inherits="Page_AM209000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" TypeName="PX.Objects.AM.ProdDetail" PrimaryView="ProdItemRecords" Visible="True" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" Visible="False" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdItem_generateLotSerial" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdItem_binLotSerial" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdMatl_generateLotSerial" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdMatl_binLotSerial" Visible="False" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand Name="ViewCompBomID" DependOnGrid="gridMatl" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="AMProdMatlSplit$RefNoteID$Link" DependOnGrid="grid2" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CreatePurchaseOrder" Visible="False" />
            <px:PXDSCallbackCommand Name="POSupplyOK" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand Name="InventoryAllocationDetailInqMatl" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DefaultControlID="edProdOrdID" 
            Caption="Prod Order" DataKeyNames="ProdOrdID" DataMember="ProdItemRecords" FilesIndicator="True" ActivityIndicator="True" 
                   NotifyIndicator="True" ActivityField="NoteActivity" NoteIndicator="True">
        <Searches>
            <px:PXControlParam ControlID="form" Name="ProdOrdID" PropertyName="NewDataKey[&quot;ProdOrdID&quot;]" Type="String" />
        </Searches>
        <Template>
            <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
            <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AutoRefresh="True" DataSourceID="ds" AllowEdit="True" >
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
                <AutoCallBack Command="Cancel" Target="ds" />
            </px:PXSelector>
            <px:PXDateTimeEdit ID="edProdDate" runat="server" DataField="ProdDate" />
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
            <px:PXSegmentMask ID="edSiteId" runat="server" DataField="SiteId" AllowEdit="True" />
            <px:PXLayoutRule ID="PXLayoutRule33" runat="server" StartColumn="False" Merge="true" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown ID="edStatusID" runat="server" DataField="StatusID" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="true" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
<px:PXSplitContainer ID="SptCont1" runat="server" SkinID="Horizontal" SplitterPosition="300" Height="600px" Panel1MinSize="100" Panel2MinSize="100">
<AutoSize Container="Window" Enabled="true" MinHeight="300"/>
<Template1>
    <px:PXGrid ID="gridOperations" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="Details" Caption="Operations" SyncPosition="true" >
        <Levels>
            <px:PXGridLevel DataKeyNames="ProdOrdID,OperationID" DataMember="ProdOperRecords">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="M" ControlSize="XM" />
                    <px:PXTextEdit ID="edOperationCD" runat="server" DataField="OperationCD" />
                    <px:PXSelector ID="edWcID" runat="server" DataField="WcID" AllowEdit="True" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" MaxLength="60" />
                    <px:PXMaskEdit ID="edSetupTime" runat="server" DataField="SetupTime" CommitChanges="True" />
                    <px:PXNumberEdit ID="edRunUnits" runat="server" DataField="RunUnits" CommitChanges="True" />
                    <px:PXMaskEdit ID="edRunUnitTime" runat="server" DataField="RunUnitTime" CommitChanges="True" />
                    <px:PXNumberEdit ID="edMachineUnits" runat="server" DataField="MachineUnits" CommitChanges="True" />
                    <px:PXMaskEdit ID="edMachineUnitTime" runat="server" DataField="MachineUnitTime" CommitChanges="True" />
                    <px:PXMaskEdit ID="edQueueTime" runat="server" DataField="QueueTime" CommitChanges="True" />
                    <px:PXMaskEdit ID="edMoveTime" runat="server" DataField="MoveTime" CommitChanges="True" />
                    <px:PXDropDown ID="edGridStatusID" runat="server" DataField="StatusID" />
                    <px:PXNumberEdit ID="edQtytoProd" runat="server" DataField="QtytoProd"  />
                    <px:PXNumberEdit ID="edQtyComplete" runat="server" DataField="QtyComplete"  />
                    <px:PXNumberEdit ID="edQtyScrapped" runat="server" DataField="QtyScrapped"  />
                    <px:PXNumberEdit ID="edQtyRemainingOper" runat="server" DataField="QtyRemaining"  />
                    <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty"  />
                    <px:PXCheckBox ID="chkBFlush" runat="server" DataField="BFlush" />
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
                    <px:PXDateTimeEdit ID="edActStartDate" runat="server" DataField="ActStartDate" />
                    <px:PXDateTimeEdit ID="edActEndDate" runat="server" DataField="ActEndDate" />
                    <px:PXDropDown ID="edScrapAction" runat="server" DataField="ScrapAction" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="OperationID" Width="70px" />
                    <px:PXGridColumn DataField="OperationCD" Width="90px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="WcID" Width="90px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Descr" MaxLength="60" Width="150px" />
                    <px:PXGridColumn DataField="SetupTime" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="RunUnits" TextAlign="Right" Width="90px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="RunUnitTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="MachineUnits" TextAlign="Right" Width="90px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="MachineUnitTime" TextAlign="Right" Width="99px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="QueueTime" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="MoveTime" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="QtytoProd" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="QtyComplete" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="QtyScrapped" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="QtyRemaining" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="TotalQty" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="StatusID" MaxLength="1" RenderEditorText="True" Width="75px" />
                    <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="85px"  />
                    <px:PXGridColumn DataField="StartDate" Width="80px" />
                    <px:PXGridColumn DataField="EndDate" Width="80px" />
                    <px:PXGridColumn DataField="ActStartDate" Width="80px" />
                    <px:PXGridColumn DataField="ActEndDate" Width="80px" />
                    <px:PXGridColumn DataField="ScrapAction" Width="80px" MaxLength="1" RenderEditorText="True" TextAlign="Left" />
                    <px:PXGridColumn DataField="PhtmBOMID"/>
                    <px:PXGridColumn DataField="PhtmBOMRevisionID"/>
                    <px:PXGridColumn DataField="PhtmBOMOperationID"/>
                    <px:PXGridColumn DataField="PhtmBOMLineRef" TextAlign="Right"/>
                    <px:PXGridColumn DataField="PhtmLevel" TextAlign="Right"/>
                    <px:PXGridColumn DataField="PhtmMatlBOMID"/>
                    <px:PXGridColumn DataField="PhtmMatlRevisionID"/>
                    <px:PXGridColumn DataField="PhtmMatlOperationID"/>
                    <px:PXGridColumn DataField="PhtmMatlLineRef" TextAlign="Right"/>
                    <px:PXGridColumn DataField="PhtmPriorLevelQty"/>                         
                    <px:PXGridColumn DataField="ProdOrdID" MaxLength="15"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode AllowUpload="True"/>
        <AutoSize Enabled="True"/>
        <AutoCallBack Command="Refresh" Target="gridmatl" ActiveBehavior="true">
            <Behavior RepaintControlsIDs="gridmatl,gridStep,gridTool,gridOvhd,totalsform,outsideProcessingform" />    
        </AutoCallBack>
        <ActionBar ActionsText="False"/>
    </px:PXGrid>
</Template1>
<Template2>
<px:PXTab ID="tab" runat="server" Width="100%">
<Items>
<px:PXTabItem Text="Materials" LoadOnDemand="True" RepaintOnDemand="True">
    <Template>
        <px:PXGrid ID="gridMatl" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="True" TabIndex="2600" StatusField="Availability" >
            <Levels>
                <px:PXGridLevel DataKeyNames="ProdOrdID,OperationID,LineID" DataMember="ProdMatlRecords">
                    <RowTemplate>
                        <px:PXMaskEdit ID="edMatlProdOrdID" runat="server" DataField="ProdOrdID"/>
                        <px:PXTextEdit ID="edMatlOperationID" runat="server" DataField="OperationID"/>
                        <px:PXSegmentMask ID="edMatlInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
                        <px:PXSegmentMask ID="edMatlSubItemID" runat="server" DataField="SubItemID"/>
                        <px:PXTextEdit ID="edMatlDescr" runat="server" DataField="Descr"/>
                        <px:PXNumberEdit ID="edMatlQtyReq" runat="server" DataField="QtyReq"/>
                        <px:PXNumberEdit ID="edMatlBatchSize" runat="server" DataField="BatchSize"/>
                        <px:PXSelector ID="edMatlUOM" runat="server" DataField="UOM" AutoRefresh="True"/>
                        <px:PXNumberEdit ID="edMatlUnitCost" runat="server" DataField="UnitCost"/>
                        <px:PXCheckBox ID="edMatlBFlush" runat="server" DataField="BFlush"/>
                        <px:PXCheckBox ID="edWarehouseOverride" runat="server" DataField="WarehouseOverride"/>
                        <px:PXSegmentMask ID="edMatlSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
                        <px:PXSelector ID="edMatlCompBOMID" runat="server" DataField="CompBOMID"/>
                        <px:PXSelector ID="edMatlCompBOMRevisionID" runat="server" DataField="CompBOMRevisionID" AllowEdit="True"/>
                        <px:PXSegmentMask ID="edMatlLocationID" runat="server" DataField="LocationID" AutoRefresh="True"/>
                        <px:PXNumberEdit ID="edMatlScrapFactor" runat="server" DataField="ScrapFactor"/>
                        <px:PXNumberEdit ID="edMatlTotalQtyRequired" runat="server" DataField="TotalQtyRequired"/>
                        <px:PXNumberEdit ID="edMatlPlanCost" runat="server" DataField="PlanCost"/>
                        <px:PXNumberEdit ID="edMatlQtyActual" runat="server" DataField="QtyActual"/>
                        <px:PXNumberEdit ID="edMatlQtyRemaining" runat="server" DataField="QtyRemaining"/>
                        <px:PXCheckBox ID="edMatlQtyRoundUp" runat="server" DataField="QtyRoundUp"/>
                        <px:PXNumberEdit ID="edMatlTotActCost" runat="server" DataField="TotActCost"/>
                        <px:PXDropDown ID="edMatlMaterialType" runat="server" DataField="MaterialType" CommitChanges="True" />
                        <px:PXTextEdit ID="edMatlPhtmBOMID" runat="server" DataField="PhtmBOMID"/>
                        <px:PXNumberEdit ID="edMatlPhtmBOMLineRef" runat="server" DataField="PhtmBOMLineRef"/>
                        <px:PXTextEdit ID="edMatlPhtmBOMOperationID" runat="server" DataField="PhtmBOMOperationID"/>
                        <px:PXNumberEdit ID="edMatlPhtmLevel" runat="server" DataField="PhtmLevel"/>
                        <px:PXNumberEdit ID="edMatlPhtmMatlLineRef" runat="server" DataField="PhtmMatlLineRef"/>
                        <px:PXTextEdit ID="edMatlPhtmMatlOperationID" runat="server" DataField="PhtmMatlOperationID"/>
                        <px:PXCheckBox ID="edMatlIsByproduct" runat="server" DataField="IsByproduct"/>
                        <px:PXCheckBox ID="edPOCreate" runat="server" DataField="POCreate"/>
                        <px:PXCheckBox ID="edProdCreate" runat="server" DataField="ProdCreate"/>
                        <px:PXNumberEdit ID="edMatlLineID" runat="server" DataField="LineID"/>
                        <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" CommitChanges="True" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="ProdOrdID" />
                        <px:PXGridColumn DataField="OperationID" />
                        <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" Width="120px"/>
                        <px:PXGridColumn DataField="SubItemID" Width="120px" />
                        <px:PXGridColumn DataField="Descr" Width="200px" />
                        <px:PXGridColumn DataField="QtyReq" Width="100px" TextAlign="Right" AutoCallBack="True" />
                        <px:PXGridColumn DataField="BatchSize" Width="100px" TextAlign="Right" AutoCallBack="True" />
                        <px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                        <px:PXGridColumn DataField="UnitCost" Width="100px" AutoCallBack="True" TextAlign="Right" />
                        <px:PXGridColumn DataField="BFlush" TextAlign="Center" Width="85px" Type="CheckBox" />
                        <px:PXGridColumn DataField="WarehouseOverride" TextAlign="Center" Width="90px" Type="CheckBox" AutoCallBack="True" />
                        <px:PXGridColumn DataField="SiteID" Width="120px" AutoCallBack="True"/>
                        <px:PXGridColumn DataField="CompBOMID" LinkCommand="ViewCompBomID" />
                        <px:PXGridColumn DataField="CompBOMRevisionID" />
                        <px:PXGridColumn DataField="LocationID" Width="120px" />
                        <px:PXGridColumn DataField="ScrapFactor" TextAlign="Right" Width="100px" AutoCallBack="True" /> 
                        <px:PXGridColumn DataField="TotalQtyRequired" TextAlign="Right" Width="100px" AutoCallBack="True" /> 
                        <px:PXGridColumn DataField="PlanCost" TextAlign="Right" Width="100px" AutoCallBack="True" /> 
                        <px:PXGridColumn DataField="QtyActual" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn DataField="QtyRemaining" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn DataField="QtyRoundUp" TextAlign="Center" Type="CheckBox" Width="85px" AutoCallBack="True" />
                        <px:PXGridColumn DataField="TotActCost" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn DataField="MaterialType" AutoCallBack="True" />
                        <px:PXGridColumn DataField="PhtmBOMID" />
                        <px:PXGridColumn DataField="PhtmBOMLineRef" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmBOMOperationID"/>
                        <px:PXGridColumn DataField="PhtmLevel" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmMatlLineRef" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmMatlOperationID"/>
                        <px:PXGridColumn DataField="IsByproduct" TextAlign="Center" Type="CheckBox" Width="65px"/>
                        <px:PXGridColumn DataField="LineID" TextAlign="Right"/>
                        <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="POCreate" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="ProdCreate" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn DataField="SubcontractSource" Width="95px" AutoCallBack="True" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowUpload="True" AllowDragRows="true"/>
            <AutoSize Enabled="True" MinHeight="200"/>
            <AutoCallBack ></AutoCallBack>
            <Parameters>
                <px:PXSyncGridParam ControlID="gridOperations" />
            </Parameters>
            <ActionBar ActionsText="False">
                <CustomItems>
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
                    <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSAMProdMatl_binLotSerial" CommandSourceID="ds" 
                        DependOnGrid="gridMatl">
                        <AutoCallBack>
                            <Behavior CommitChanges="True" PostData="Page" ></Behavior>
                        </AutoCallBack>
                    </px:PXToolBarButton>
                    <px:PXToolBarButton DependOnGrid="gridMatl" Key="cmdInventoryAllocationDetailInqMatl">
                        <AutoCallBack Command="InventoryAllocationDetailInqMatl" Target="ds" />
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="PO Link" DependOnGrid="gridMatl" StateColumn="POCreate">
                        <AutoCallBack Command="POSupplyOK" Target="ds" ></AutoCallBack>
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
            <CallbackCommands PasteCommand="PasteLine">
                <Save PostData="Container" />
            </CallbackCommands>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Steps" LoadOnDemand="True" RepaintOnDemand="True">
    <Template>
        <px:PXGrid id="gridStep" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="True" >
            <Levels>
                <px:PXGridLevel DataKeyNames="LineID,OperationID,ProdOrdID" DataMember="ProdStepRecords">
                    <RowTemplate>
                        <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" Merge="true" />
                        <px:PXTextEdit Size="xl" ID="edStepDescr" runat="server" DataField="Descr" />
                        <px:PXNumberEdit ID="edStepSortOrder" runat="server" DataField="SortOrder" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="Descr" Width="351px" />
                        <px:PXGridColumn DataField="PhtmBOMID"/>
                        <px:PXGridColumn DataField="PhtmBOMRevisionID"/>
                        <px:PXGridColumn DataField="PhtmBOMOperationID"/>
                        <px:PXGridColumn DataField="PhtmBOMLineRef" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmLevel" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmMatlBOMID"/>
                        <px:PXGridColumn DataField="PhtmMatlRevisionID"/>
                        <px:PXGridColumn DataField="PhtmMatlOperationID"/>
                        <px:PXGridColumn DataField="PhtmMatlLineRef" TextAlign="Right"/>
                        <px:PXGridColumn DataField="LineID" TextAlign="Right"/>
                        <px:PXGridColumn DataField="OperationID" MaxLength="10"/>
                        <px:PXGridColumn DataField="ProdOrdID"/>
                        <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="85px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode InitNewRow="True" AllowUpload="True" />
            <AutoSize Enabled="True" />
            <ActionBar ActionsText="False"/>
            <AutoCallBack/>
            <Parameters>
                <px:PXSyncGridParam ControlID="gridOperations" />
            </Parameters>
            <Mode InitNewRow="True" AllowFormEdit="True"/>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Tools" LoadOnDemand="True" RepaintOnDemand="True">
    <Template>
        <px:PXGrid ID="gridTool" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab"  SyncPosition="True" >
            <Levels>
                <px:PXGridLevel DataKeyNames="LineID,OperationID,ProdOrdID" DataMember="ProdToolRecords">
                    <RowTemplate>
                        <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                        <px:PXSelector ID="edToolID" runat="server"  DataField="ToolID" AllowEdit="True"/>
                        <px:PXLayoutRule ID="PXLayoutRule13" runat="server" Merge="True" />
                        <px:PXLabel Size="xs" ID="lblToolDescr" runat="server" Encode="True">Description:</px:PXLabel>
                        <px:PXTextEdit Size="xl" ID="edToolDescr" runat="server" DataField="Descr" MaxLength="60"  />
                        <px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />
                        <px:PXLabel Size="xs" ID="lblToolQtyReq" runat="server" Encode="True">Qty Required:</px:PXLabel>
                        <px:PXNumberEdit Size="s" ID="edToolQtyReq" runat="server" DataField="QtyReq"  />
                        <px:PXLayoutRule ID="PXLayoutRule16" runat="server" Merge="False" />
                        <px:PXLayoutRule ID="PXLayoutRule17" runat="server" Merge="True" />
                        <px:PXLabel Size="xs" ID="lblToolUnitCost" runat="server" Encode="True">Unit Cost:</px:PXLabel>
                        <px:PXNumberEdit Size="s" ID="edToolUnitCost" runat="server" DataField="UnitCost"  />
                        <px:PXNumberEdit Size="s" ID="edToolTotActUses" runat="server" DataField="TotActUses" />
                        <px:PXLayoutRule ID="PXLayoutRule19" runat="server" Merge="True" />
                        <px:PXLabel Size="xs" ID="lblToolTotActCost" runat="server" Encode="True">Total Actual Cost:</px:PXLabel>
                        <px:PXNumberEdit Size="s" ID="edToolTotActCost" runat="server" DataField="TotActCost"  />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="ProdOrdID" MaxLength="15" Visible="False" />
                        <px:PXGridColumn DataField="OperationID" MaxLength="10" Visible="False" />
                        <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                        <px:PXGridColumn DataField="ToolID" Width="81px" AutoCallBack="true" />
                        <px:PXGridColumn DataField="Descr" MaxLength="60" Width="351px" AutoCallBack="true"/>
                        <px:PXGridColumn DataField="QtyReq" TextAlign="Right" Width="117px" AutoCallBack="true"/>
                        <px:PXGridColumn DataField="UnitCost" TextAlign="Right" Width="117px" AutoCallBack="true"/>
                        <px:PXGridColumn DataField="TotActUses" TextAlign="Right" Width="117px" AutoCallBack="true" />
                        <px:PXGridColumn DataField="TotActCost" TextAlign="Right" Width="117px" AutoCallBack="true"/>
                        <px:PXGridColumn DataField="PhtmBOMID"/>
                        <px:PXGridColumn DataField="PhtmBOMRevisionID"/>
                        <px:PXGridColumn DataField="PhtmBOMOperationID"/>
                        <px:PXGridColumn DataField="PhtmBOMLineRef" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmLevel" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmMatlBOMID"/>
                        <px:PXGridColumn DataField="PhtmMatlRevisionID"/>
                        <px:PXGridColumn DataField="PhtmMatlOperationID"/>
                        <px:PXGridColumn DataField="PhtmMatlLineRef" TextAlign="Right"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowUpload="True"/>
            <AutoSize Enabled="True" />
            <ActionBar ActionsText="False"/>
            <AutoCallBack/>
            <Parameters>
                <px:PXSyncGridParam ControlID="gridOperations" />
            </Parameters>
            <Mode InitNewRow="True" AllowFormEdit="True"/>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Overhead" LoadOnDemand="True" RepaintOnDemand="True">
    <Template>
        <px:PXGrid ID="gridOvhd" runat="server" Width="100%" AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab" 
                   SyncPosition="True" TabIndex="-24636" >
            <Levels>
                <px:PXGridLevel DataKeyNames="ProdOrdID,OperationID,LineID" DataMember="ProdOvhdRecords">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                        <px:PXSelector ID="edOvhdID" runat="server" DataField="OvhdID" AllowEdit="True"/>
                        <px:PXTextEdit ID="edAMOverhead__Descr" runat="server" DataField="AMOverhead__Descr" MaxLength="60"  />
                        <px:PXDropDown ID="edAMOverhead__OvhdType" runat="server" DataField="AMOverhead__OvhdType" />
                        <px:PXNumberEdit ID="edOFactor" runat="server" DataField="OFactor"  />
                        <px:PXNumberEdit ID="edAMOverhead__CostRate" runat="server" DataField="AMOverhead__CostRate"  />
                        <px:PXCheckBox ID="edWCFlag" runat="server" DataField="WCFlag" Text="WC Flag"/>
                        <px:PXLayoutRule runat="server" Merge="True" />
                        <px:PXLabel Size="xs" ID="lblOvhdTotActCost" runat="server">Total Actual Cost:</px:PXLabel>
                        <px:PXNumberEdit Size="s" ID="edOvhdTotActCost" runat="server" DataField="TotActCost"  />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="ProdOrdID" MaxLength="10" Visible="False" />
                        <px:PXGridColumn DataField="OperationID" MaxLength="10" Visible="False" />
                        <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                        <px:PXGridColumn DataField="OvhdID" DisplayFormat="&gt;AAAAAAAAAA"  MaxLength="10" Width="81px" AutoCallBack="true" />
                        <px:PXGridColumn DataField="AMOverhead__Descr" MaxLength="60" Width="351px" />
                        <px:PXGridColumn DataField="AMOverhead__OvhdType" MaxLength="1" RenderEditorText="True" Width="198px" />
                        <px:PXGridColumn DataField="OFactor" TextAlign="Right" Width="117px" />
                        <px:PXGridColumn DataField="AMOverhead__CostRate" TextAlign="Right" Width="117px" />
                        <px:PXGridColumn DataField="TotActCost" TextAlign="Right" Width="117px" AllowUpdate="False" />
                        <px:PXGridColumn DataField="WCFlag" TextAlign="Center" Type="CheckBox" Width="60px"/>
                        <px:PXGridColumn DataField="PhtmBOMID"/>
                        <px:PXGridColumn DataField="PhtmBOMRevisionID"/>
                        <px:PXGridColumn DataField="PhtmBOMOperationID"/>
                        <px:PXGridColumn DataField="PhtmBOMLineRef" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmLevel" TextAlign="Right"/>
                        <px:PXGridColumn DataField="PhtmMatlBOMID"/>
                        <px:PXGridColumn DataField="PhtmMatlRevisionID"/>
                        <px:PXGridColumn DataField="PhtmMatlOperationID"/>
                        <px:PXGridColumn DataField="PhtmMatlLineRef" TextAlign="Right"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowUpload="True"/>
            <AutoSize Enabled="True" />
            <ActionBar ActionsText="False"></ActionBar>
            <AutoCallBack/>
            <Parameters>
                <px:PXSyncGridParam ControlID="gridOperations" />
            </Parameters>
            <Mode InitNewRow="True" AllowFormEdit="True"/>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Totals" LoadOnDemand="True" RepaintOnDemand="True">
    <Template >
        <px:PXFormView ID="totalsform" runat="server" CaptionVisible="False" DataMember="ProdOperSelected" DataSourceID="ds" SkinID="Transparent" Width="100%" >
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule21" runat="server" GroupCaption="Planned" StartColumn="True" LabelsWidth="125px" />
                <px:PXMaskEdit ID="edPlanLaborTime" runat="server" DataField="PlanLaborTime" Width="150px" />
                <px:PXNumberEdit ID="edPlanLabor" runat="server" DataField="PlanLabor" Width="150px" />
                <px:PXNumberEdit ID="edPlanMachine" runat="server" DataField="PlanMachine" Width="150px" />
                <px:PXNumberEdit ID="edPlanMaterial" runat="server" DataField="PlanMaterial" Width="150px"  />
                <px:PXNumberEdit ID="edPlanTool" runat="server" DataField="PlanTool" Width="150px" />
                <px:PXNumberEdit ID="edPlanFixedOverhead" runat="server" DataField="PlanFixedOverhead" Width="150px" />
                <px:PXNumberEdit ID="edPlanVariableOverhead" runat="server" DataField="PlanVariableOverhead" Width="150px" />
                <px:PXNumberEdit ID="edPlanSubcontract" runat="server" DataField="PlanSubcontract" Width="150px" />
                <px:PXNumberEdit ID="edPlanQtyToProduce" runat="server" DataField="PlanQtyToProduce" Width="150px"  />
                <px:PXNumberEdit ID="edPlanTotal" runat="server" DataField="PlanTotal" Width="150px"  />
                <px:PXDateTimeEdit ID="edPlanCostDate" runat="server" DataField="PlanCostDate" Width="150px" />
                <px:PXNumberEdit ID="edPlanReferenceMaterial" runat="server" DataField="PlanReferenceMaterial" Width="150px" />
                <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Actual" StartColumn="True" LabelsWidth="125px" />
                <px:PXMaskEdit ID="edActualLaborTime" runat="server" DataField="ActualLaborTime" Width="150px" />
                <px:PXNumberEdit ID="edActualLabor" runat="server" DataField="ActualLabor" Width="150px" />
                <px:PXNumberEdit ID="edActualMachine" runat="server" DataField="ActualMachine" Width="150px" />
                <px:PXNumberEdit ID="edActualMaterial" runat="server" DataField="ActualMaterial" Width="150px" />
                <px:PXNumberEdit ID="edActualTool" runat="server" DataField="ActualTool" Width="150px" />
                <px:PXNumberEdit ID="edActualFixedOverhead" runat="server" DataField="ActualFixedOverhead" Width="150px" />
                <px:PXNumberEdit ID="edActualVariableOverhead" runat="server" DataField="ActualVariableOverhead" Width="150px" />
                <px:PXNumberEdit ID="edActualSubcontract" runat="server" DataField="ActualSubcontract" Size="" Width="150px" />
                <px:PXNumberEdit ID="edQtyCompleteTotals" runat="server" DataField="QtyComplete" Width="150px" />
                <px:PXNumberEdit ID="edWIPAdjustment" runat="server" DataField="WIPAdjustment" Width="150px" />
                <px:PXNumberEdit ID="edScrapAmt" runat="server" DataField="ScrapAmount" Width="150px" />
                <px:PXNumberEdit ID="edWIPTotal" runat="server" DataField="WIPTotal" Width="150px" />
                <px:PXNumberEdit ID="edWIPComp" runat="server" DataField="WIPComp" Width="150px" />
                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Variance" StartColumn="True" LabelsWidth="125px" />
                <px:PXMaskEdit ID="edVarianceLaborTime" runat="server" DataField="VarianceLaborTime" Width="150px" />
                <px:PXNumberEdit ID="edVarianceLabor" runat="server" DataField="VarianceLabor" Width="150px" />
                <px:PXNumberEdit ID="edsVarianceMachine" runat="server" DataField="VarianceMachine" Width="150px" />
                <px:PXNumberEdit ID="edVarianceMaterial" runat="server" DataField="VarianceMaterial" Width="150px" />
                <px:PXNumberEdit ID="edVarianceTool" runat="server" DataField="VarianceTool" Width="150px" />
                <px:PXNumberEdit ID="edVarianceFixedOverhead" runat="server" DataField="VarianceFixedOverhead" Width="150px" />
                <px:PXNumberEdit ID="edVarianceVariableOverhead" runat="server" DataField="VarianceVariableOverhead" Width="150px" />
                <px:PXNumberEdit ID="edVarianceSubcontract" runat="server" DataField="VarianceSubcontract" Size="" Width="150px" />
                <px:PXNumberEdit ID="edTotalsQtyRemaining" runat="server" DataField="QtyRemaining" Width="150px" />
                <px:PXNumberEdit ID="edVarianceTotal" runat="server" DataField="VarianceTotal" Width="150px" />
                <px:PXNumberEdit ID="edWIPBalance" runat="server" DataField="WIPBalance" Width="150px" />
            </Template>
        </px:PXFormView>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Outside Process" LoadOnDemand="True" RepaintOnDemand="True" >
    <Template >
        <px:PXFormView ID="outsideProcessingform" runat="server" CaptionVisible="False" DataMember="OutsideProcessingOperationSelected" DataSourceID="ds" 
            SkinID="Transparent" Width="100%" SyncPosition="True" DataKeyNames="ProdOrdID,OperationID" >
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule25" runat="server" GroupCaption="General Settings" StartColumn="True" LabelsWidth="125px" />
                <px:PXCheckBox ID="edOutsideProcess" runat="server" DataField="OutsideProcess" AutoRefresh="True" CommitChanges="True" />
                <px:PXCheckBox ID="edDropShippedToVendor" runat="server" DataField="DropShippedToVendor" AutoRefresh="True" CommitChanges="True" />
                <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
                <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
                <px:PXLayoutRule ID="PXLayoutRule26" runat="server" GroupCaption="Purchase Order" StartColumn="False" LabelsWidth="125px" />
                <px:PXTextEdit ID="edPOOrderNbr" runat="server" DataField="POOrderNbr" />
                <px:PXNumberEdit ID="edPOLineNbr" runat="server" DataField="POLineNbr"/>
                <px:PXButton ID="btnCreatePurchaseOrder" runat="server" CommandName="CreatePurchaseOrder" CommandSourceID="ds" />
                <px:PXLayoutRule ID="PXLayoutRule27" runat="server" GroupCaption="Operation Quantity" StartColumn="True" LabelsWidth="125px" />
                <px:PXNumberEdit ID="edOPQtytoProd" runat="server" DataField="QtytoProd" Width="150px" />
                <px:PXNumberEdit ID="edOPShippedQuantity" runat="server" DataField="ShippedQuantity" Width="150px" />
                <px:PXNumberEdit ID="edOPShipRemainingQty" runat="server" DataField="ShipRemainingQty" Width="150px" />
                <px:PXNumberEdit ID="edOPAtVendorQuantity" runat="server" DataField="AtVendorQuantity" Width="150px" />
                <px:PXNumberEdit ID="edOPQtyComplete" runat="server" DataField="QtyComplete" Width="150px" />
            </Template>
        </px:PXFormView>
    </Template>
</px:PXTabItem>
</Items>
<AutoSize Enabled="True" />
</px:PXTab>
</Template2>
</px:PXSplitContainer>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="90%" Height="360px" Caption="Allocations" CaptionVisible="True" Key="lsSelectMatl"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="optform" DesignView="Content" TabIndex="3200">
        <px:PXFormView ID="optform" runat="server" CaptionVisible="False" DataMember="LSAMProdMatl_lotseropts" DataSourceID="ds" SkinID="Transparent"
            TabIndex="-3236" Width="100%">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" CommandName="LSAMProdMatl_generateLotSerial" CommandSourceID="ds" Height="20px"
                    Text="Generate" />
            </Template>
            <Parameters>
                <px:PXSyncGridParam ControlID="gridMatl" />
            </Parameters>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" AutoAdjustColumns="True" DataSourceID="ds" Height="192px" Style="height: 192px;" TabIndex="-3036" Width="100%" AllowFilter="true"
            SkinID="Inquire" SyncPosition="true">
            <Parameters>
                <px:PXSyncGridParam ControlID="gridMatl" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataKeyNames="OrderType,ProdOrdID,OperationID,LineID,SplitLineNbr" DataMember="ProdMatlSplits">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                        <px:PXCheckBox ID="edIsAllocated" runat="server" DataField="IsAllocated"/>
                        <px:PXNumberEdit ID="edSplitLineNbr" runat="server" AutoRefresh="True" DataField="SplitLineNbr" />
                        <px:PXSegmentMask ID="edSiteID2" runat="server" AutoRefresh="True" DataField="SiteID" />
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" AutoRefresh="True" DataField="LotSerialNbr">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMProdMatlSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMProdMatlSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMProdMatlSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" AutoRefresh="True" DataField="UOM">
                            <Parameters>
                                <px:PXControlParam ControlID="gridMatl" Name="AMProdMatl.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXCheckBox ID="edPOCreateAllocation" runat="server" DataField="POCreate"/>
                        <px:PXCheckBox ID="edProdCreateAllocation" runat="server" DataField="ProdCreate"/>
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="IsAllocated" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn DataField="SplitLineNbr" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="SiteID" Width="90px" AllowShowHide="Server" AutoCallBack="True" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="LotSerialNbr" Width="90px" AutoCallBack="True" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="108px" />
                        <px:PXGridColumn DataField="QtyReceived" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" Width="90px" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="POCreate" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="ProdCreate" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="AMProdMatlSplit$RefNoteID$Link" Width="100px" ></px:PXGridColumn>
                    </Columns>
                    <Layout FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
            <Mode InitNewRow="True" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel4save" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelPOSupply" runat="server" Width="960px" Height="360px" Caption="Purchasing Details" CaptionVisible="True"
        LoadOnDemand="True" ShowAfterLoad="True" AutoCallBack-Target="formCurrentPOSupply" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" Key="currentposupply" TabIndex="3100">
        <px:PXFormView ID="formCurrentPOSupply" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="currentposupply"
            Caption="Purchasing Settings" CaptionVisible="False" SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="gridMatl" ></px:PXSyncGridParam>
            </Parameters>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" ></px:PXLayoutRule>
                <%--<px:PXDropDown CommitChanges="True" ID="edPOSource" runat="server" DataField="POSource" />--%>
                <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" />
                <px:PXSegmentMask CommitChanges="True" ID="edPurchaseSiteID" runat="server" DataField="SiteID" AutoRefresh="True" Enabled="false" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridPOSupply" runat="server" Height="360px" Width="1500px" DataSourceID="ds" AutoAdjustColumns="true">
            <Parameters>
                <px:PXSyncGridParam ControlID="gridMatl" ></px:PXSyncGridParam>
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="posupply">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" TextAlign="Center" />
                        <px:PXGridColumn DataField="OrderType" Width="90px"/>
                        <px:PXGridColumn DataField="OrderNbr" Width="108px" />
                        <px:PXGridColumn DataField="VendorRefNbr" Width="108px" />
                        <px:PXGridColumn AllowNull="False" DataField="LineType" Width="108px" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" Width="108px" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" Width="90px" />
                        <px:PXGridColumn DataField="VendorID" Width="108px" />
                        <px:PXGridColumn DataField="VendorID_Vendor_AcctName" Width="108px" />
                        <px:PXGridColumn DataField="PromisedDate" Width="90px" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" />
                        <px:PXGridColumn DataField="OrderQty" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="OpenQty" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="TranDesc" Width="108px" />
                    </Columns>
                    <RowTemplate>
                        <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" AllowEdit="True" ></px:PXSelector>
                    </RowTemplate>
                    <Layout ColumnsMenu="False" ></Layout>
                    <Mode AllowAddNew="false" AllowDelete="false" ></Mode>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" ></AutoSize>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="Save" ></px:PXButton>
            <px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" ></px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>