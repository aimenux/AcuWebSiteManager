<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
ValidateRequest="false" CodeFile="AM208000.aspx.cs" Inherits="Page_AM208000"
Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" TypeName="PX.Objects.AM.BOMMaint" PrimaryView="Documents"
                     Visible="True" Width="100%" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="gridMatl" />
            <px:PXDSCallbackCommand Name="ViewCompBomID" DependOnGrid="gridMatl" Visible="False" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="BOM of Material" DataMember="Documents" DataSourceID="ds" 
                   NotifyIndicator="True" NoteIndicator="true" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edBOMID">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edBOMID" runat="server" AutoRefresh="True" DataField="BOMID" DataSourceID="ds" CommitChanges="true" FilterByAllFields="True" />
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" Width="50px" CommitChanges="True" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" LabelsWidth="S" ControlSize="L"/>
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="true" LabelsWidth="S" ControlSize="SM"/>
            <px:PXDateTimeEdit ID="edEffStartDate" runat="server" DataField="EffStartDate"/>
            <px:PXDateTimeEdit ID="edEffEndDate" runat="server" DataField="EffEndDate"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="600px" Panel1MinSize="100" Panel2MinSize="100">
<AutoSize Container="Window" Enabled="True" MinHeight="300" />
<Template1>
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="Details" Caption="Operations" AutoAdjustColumns="True" SyncPosition="true">
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
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode AllowUpload="True"/>
        <AutoSize Enabled="True" />
        <ActionBar ActionsText="False">
        </ActionBar>
        <AutoCallBack Command="Refresh" Target="gridmatl" ActiveBehavior="true" >
            <Behavior RepaintControlsIDs="gridmatl,gridStep,gridTool,gridOvhd,outsideProcessingform"  />
        </AutoCallBack>
    </px:PXGrid>
</Template1>
<Template2>
<px:PXTab ID="tab" runat="server" Width="100%" Height="100%">
<Items>
    <px:PXTabItem Text="Materials" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridMatl" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="True">
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
                            <px:PXDropDown ID="edMaterialType" runat="server" AllowNull="False" DataField="MaterialType" CommitChanges="true" />
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
                            <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" CommitChanges="True" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Width="54px" />
                            <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                            <px:PXGridColumn DataField="InventoryID" Width="130px" CommitChanges="True" AllowDragDrop="True" />
                            <px:PXGridColumn DataField="SubItemID" Width="81px" />
                            <px:PXGridColumn DataField="Descr" MaxLength="255" Width="200px" />
                            <px:PXGridColumn DataField="QtyReq" TextAlign="Right" Width="108px" CommitChanges="True" AllowDragDrop="True" />                            
                            <px:PXGridColumn DataField="BatchSize" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="UOM" Width="81px" CommitChanges="True" />
                            <px:PXGridColumn DataField="UnitCost" TextAlign="Right" Width="108px" />
                            <px:PXGridColumn DataField="PlanCost" TextAlign="Right" Width="100px" /> 
                            <px:PXGridColumn DataField="MaterialType" TextAlign="Left" Width="100px" CommitChanges="True" />
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
                            <px:PXGridColumn DataField="SubcontractSource" Width="95px" CommitChanges="True" />
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
					<px:PXSyncGridParam ControlID="grid" />
				</Parameters>
                <ActionBar ActionsText="False"></ActionBar>
                <CallbackCommands PasteCommand="PasteLine">
                    <Save PostData="Container" />
                </CallbackCommands>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
    <px:PXTabItem Text="Steps" LoadOnDemand="True" RepaintOnDemand="True">
        <Template>
            <px:PXGrid ID="gridStep" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab">
                <Levels>
                    <px:PXGridLevel DataMember="BomStepRecords" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" Merge="true" />
                            <px:PXTextEdit Size="xl" ID="edStepDescr" runat="server" AllowNull="False" DataField="Descr" />
                            <px:PXNumberEdit ID="edStepSortOrder" runat="server" DataField="SortOrder" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn AllowNull="False" DataField="Descr" Width="351px" />
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="85px" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode InitNewRow="True" AllowUpload="True" />
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
            <px:PXGrid ID="gridTool" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab">
                <Levels>
                    <px:PXGridLevel DataMember="BomToolRecords" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSelector ID="edToolID" runat="server" DataField="ToolID" AllowEdit="True" />
                            <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                            <px:PXTextEdit ID="edToolDescr" runat="server" DataField="AMToolMst__Descr" MaxLength="60" />
                            <px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
                            <px:PXLabel Size="xs" ID="lblToolQtyReq" runat="server" Encode="True">Qty Required:</px:PXLabel>
                            <px:PXNumberEdit Size="s" ID="edToolQtyReq" runat="server" DataField="QtyReq" />
                            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" />
                            <px:PXLabel Size="xs" ID="lblToolUnitCost" runat="server" Encode="True">Unit Cost:</px:PXLabel>
                            <px:PXNumberEdit Size="s" ID="edToolUnitCost" runat="server" DataField="UnitCost" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn AllowNull="False" DataField="ToolID" MaxLength="30" Width="180px" AutoCallBack="True" />
                            <px:PXGridColumn AllowNull="False" DataField="Descr" MaxLength="60" Width="351px" />
                            <px:PXGridColumn AllowNull="False" DataField="QtyReq" TextAlign="Right" Width="117px" />
                            <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" Width="117px" />
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
            <px:PXGrid ID="gridOvhd" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab">
                <Levels>
                    <px:PXGridLevel DataMember="BomOvhdRecords" >
                        <RowTemplate>
                            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSelector ID="edOvhdID" runat="server" AllowNull="False" DataField="OvhdID" DataKeyNames="OvhdID" AllowEdit="True" />
                            <px:PXTextEdit ID="edAMOverhead__Descr" runat="server" AllowNull="False" DataField="AMOverhead__Descr" MaxLength="60" />
                            <px:PXDropDown ID="edAMOverhead__OvhdType" runat="server" AllowNull="False" DataField="AMOverhead__OvhdType" />
                            <px:PXNumberEdit ID="edOFactor" runat="server" DataField="OFactor" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
                            <px:PXGridColumn AllowNull="False" DataField="OvhdID" MaxLength="10" Width="81px" AutoCallBack="True" DisplayFormat="&gt;AAAAAAAAAA" />
                            <px:PXGridColumn DataField="AMOverhead__Descr" AllowNull="False" MaxLength="60" Width="351px" />
                            <px:PXGridColumn AllowNull="False" DataField="AMOverhead__OvhdType" Width="198px" MaxLength="1" RenderEditorText="True" />
                            <px:PXGridColumn DataField="OFactor" AllowNull="False" TextAlign="Right" Width="117px" />
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
		<px:PXGrid ID="gridref" runat="server" DataSourceID="ds" SkinID="DetailsInTab" MatrixMode="true" Width="100%" Height="100%">
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
	<px:PXSmartPanel ID="PanelCopyBom" runat="server" Caption="Copy BOM" CaptionVisible="True" LoadOnDemand="true" Key="copybomfilter" 
	    AutoCallBack-Enabled="True" AutoCallBack-Target="formCopyBom" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" >
		<px:PXFormView ID="formCopyBom" runat="server" DataSourceID="ds" CaptionVisible="False"
			DataMember="copybomfilter" SkinID="Transparent" Width="100%">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartGroup="True" GroupCaption="Copy From" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit ID="edFromBOMID" runat="server" DataField="FromBOMID" />
                <px:PXTextEdit ID="edFromRevisionID" runat="server" DataField="FromRevisionID" />
                <px:PXSegmentMask ID="edFromInvtID" runat="server" DataField="FromInventoryID" />
                <px:PXSegmentMask ID="edFromSubItemID" runat="server" DataField="FromSubItemID" />
                <px:PXSelector ID="edFromSiteID" runat="server" DataField="FromSiteID" />
				<px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartGroup="True" GroupCaption="Copy To" />
				<px:PXMaskEdit ID="edToBOMID" runat="server" DataField="ToBOMID" CommitChanges="True" />
                <px:PXTextEdit ID="edToRevisionID" runat="server" DataField="ToRevisionID" />
				<px:PXSegmentMask ID="edToInvtID" runat="server" DataField="ToInventoryID" CommitChanges="True" />
                <px:PXSegmentMask ID="edToSubItemCD" runat="server" DataField="ToSubItemCD" CommitChanges="True" />
				<px:PXSelector ID="edToSiteID" runat="server" DataField="ToSiteID" CommitChanges="True" />
                <px:PXCheckBox ID="edUpdateMaterialWarehouse" runat="server" DataField="UpdateMaterialWarehouse" />
			    <px:PXLayoutRule ID="PXLayoutRuleCopyNotes" runat="server" StartGroup="True" GroupCaption="Copy Notes" />
			    <px:PXGroupBox ID="copyNotesGroupBox" runat="server" RenderStyle="Simple" RenderSimple="True">
			        <Template>
			            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True"/>
			            <px:PXCheckBox ID="PXCopyNotesItem" runat="server" DataField="CopyNotesItem" AlignLeft="True"/>
			            <px:PXCheckBox ID="PXCopyNotesOper" runat="server" DataField="CopyNotesOper" AlignLeft="True"/>
			            <px:PXLayoutRule runat="server" StartColumn="True"/>
			            <px:PXCheckBox ID="PXCopyNotesMatl" runat="server" DataField="CopyNotesMatl" AlignLeft="True"/>
			            <px:PXCheckBox ID="PXCopyNotesStep" runat="server" DataField="CopyNotesStep" AlignLeft="True"/>
			            <px:PXLayoutRule runat="server" StartColumn="True"/>
			            <px:PXCheckBox ID="PXCopyNotesTool" runat="server" DataField="CopyNotesTool" AlignLeft="True"/>
			            <px:PXCheckBox ID="PXCopyNotesOvhd" runat="server" DataField="CopyNotesOvhd" AlignLeft="True"/>
			        </Template>
			    </px:PXGroupBox>
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanelCopyBomBtn" runat="server" SkinID="Buttons" Width="90%">
			<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Copy"></px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel"></px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
 <px:PXSmartPanel ID="PanelMakeDefault" runat="server" Caption="Default BOM Levels" CaptionVisible="True"
		DesignView="Content" LoadOnDemand="true" Key="DefaultBomLevelsFilter" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formDefaultBomLevels" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" Width="175px" Height="125px">
        <px:PXFormView ID="formDefaultBomLevels" runat="server" DataSourceID="ds" CaptionVisible="False"
			DataMember="DefaultBomLevelsFilter" SkinID="Transparent" Width="100%">
			<Template>
                <px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartGroup="True" GroupCaption="" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXCheckBox ID="edItem" runat="server" DataField="Item" AlignLeft="True" />
                <px:PXCheckBox ID="edWarehouse" runat="server" DataField="Warehouse" AlignLeft="True" />
                <px:PXCheckBox ID="edSubItem" runat="server" DataField="SubItem" AlignLeft="True" />
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanelMakeDefaultBtn" runat="server" SkinID="Transparent">
            <px:PXButton ID="PXButtonMakeDefaultOk" runat="server" DialogResult="OK" Text="Update" CommandSourceID="ds" />
            <px:PXButton ID="PXButtonMakeDefaultCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
<px:PXSmartPanel ID="PanelMakePlanning" runat="server" Caption="Planning BOM Levels" CaptionVisible="True"
        DesignView="Content" LoadOnDemand="true" Key="PlanningBomLevelsFilter" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formPlanningBomLevels" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" Width="175px" Height="125px">
        <px:PXFormView ID="formPlanningBomLevels" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="PlanningBomLevelsFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartGroup="True" GroupCaption="" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXCheckBox ID="edItem" runat="server" DataField="Item" AlignLeft="True" />
                <px:PXCheckBox ID="edWarehouse" runat="server" DataField="Warehouse" AlignLeft="True" />
                <px:PXCheckBox ID="edSubItem" runat="server" DataField="SubItem" AlignLeft="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelMakePlanningBtn" runat="server" SkinID="Transparent">
            <px:PXButton ID="PXButtonMakePlanningOk" runat="server" DialogResult="OK" Text="Update" CommandSourceID="ds" />
            <px:PXButton ID="PXButtonMakePlanningCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
	</px:PXSmartPanel>
<px:PXSmartPanel ID="PanelBOMCostRoll" runat="server" Caption="BOM Cost Summary" CaptionVisible="True"
		DesignView="Content" LoadOnDemand="true" Key="bomcostrecs" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formCostSummary" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" Width="500px" Height="350px" CloseButtonDialogResult="Abort">
        <px:PXFormView ID="formCostSummary" runat="server" DataSourceID="ds" CaptionVisible="False"
			DataMember="bomcostrecs" SkinID="Transparent" Width="100%">
			<Template>
			    <px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartGroup="True" GroupCaption="" StartColumn="True" LabelsWidth="S" ControlSize="S" 
                    Merge="True" />
                <px:PXNumberEdit ID="edLotSize" runat="server" Enabled="False" DataField="LotSize" />
			    <px:PXCheckBox ID="edMultiLevelProcess" runat="server" DataField="MultiLevelProcess" />
				<px:PXLayoutRule ID="PXLayoutRule17" runat="server" StartColumn="True" GroupCaption="Labor" StartRow="True" LabelsWidth="S" ControlSize="S" 
                    Merge="True" />
                <px:PXNumberEdit ID="edFixedLabor" runat="server" DataField="FLaborCost" Enabled="False" />
                <px:PXNumberEdit ID="edVariableLabor" runat="server" DataField="VLaborCost" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartColumn="True" GroupCaption="Overhead" StartRow="True" LabelsWidth="S" ControlSize="S" 
                    Merge="True" />
                <px:PXNumberEdit ID="edFOvdCost" runat="server" DataField="FOvdCost" Enabled="False" />
                <px:PXNumberEdit ID="edVOvdCost" runat="server" DataField="VOvdCost" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartColumn="True" GroupCaption="Other Costs" StartRow="True" LabelsWidth="S" ControlSize="S" 
                    Merge="True" />
                <px:PXNumberEdit ID="edMachine" runat="server" DataField="MachCost" Enabled="False" />
                <px:PXNumberEdit ID="edTools" runat="server" DataField="ToolCost" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule20" runat="server" StartColumn="True" StartRow="True" LabelsWidth="S" ControlSize="S"  
                    Merge="True" />
                <px:PXNumberEdit ID="edMaterial" runat="server" DataField="MatlCost" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule21" runat="server" StartColumn="True" GroupCaption="Totals" StartRow="True" LabelsWidth="S" ControlSize="S" 
                    Merge="True" />
                <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" Enabled="False" />
                <px:PXNumberEdit ID="edTotalCost" runat="server" DataField="TotalCost" Enabled="False" />
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PanelCostSummary" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton5" runat="server" DialogResult="OK" CommandSourceID="ds" Text="OK"></px:PXButton>
        </px:PXPanel>
	</px:PXSmartPanel>
    <px:PXSmartPanel ID="BOMCostSettings" runat="server" Caption="BOM Cost Settings" CaptionVisible="True"
		DesignView="Content" LoadOnDemand="true" Key="rollsettings" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formCostSummary" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" Width="300px" Height="150px" CloseButtonDialogResult="Abort">
        <px:PXFormView ID="RollSettingsForm" runat="server" DataSourceID="ds" CaptionVisible="False"
			DataMember="rollsettings" SkinID="Transparent" Width="100%">
			<Template>
			    <px:PXLayoutRule ID="PXLayoutRule22" runat="server" StartGroup="True" GroupCaption="" StartColumn="True" LabelsWidth="S" 
                    ControlSize="S" Merge="True" />
                <px:PXNumberEdit ID="edLotSize" runat="server" Enabled="True" DataField="LotSize" CommitChanges="true" />
                <px:PXLayoutRule ID="PXLayoutRule23" runat="server" StartGroup="True" GroupCaption="" StartColumn="True" LabelsWidth="S" 
                    ControlSize="S" Merge="True" />
                <px:PXDropDown ID="edSnglMlti" runat="server" AllowNull="False" DataField="SnglMlti" CommitChanges="true" />
            </Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton7" runat="server" DialogResult="OK" CommandSourceID="ds" Text="OK"></px:PXButton>
            <px:PXButton ID="PXButton8" runat="server" DialogResult="Abort" CommandSourceID="ds" Text="Cancel"></px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
