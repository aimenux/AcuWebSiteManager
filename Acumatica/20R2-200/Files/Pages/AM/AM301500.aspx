<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM301500.aspx.cs" Inherits="Page_AM301500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="Document" TypeName="PX.Objects.AM.DisassemblyEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		    <px:PXDSCallbackCommand Name="CopyLine" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSAMDisassembleMaterialTran_generateLotSerial" />
		    <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSAMDisassembleMaterialTran_binLotSerial" DependOnGrid="gridMaterial" />
		    <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSAMDisassembleMasterTran_generateLotSerial" />
		    <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSAMDisassembleMasterTran_binLotSerial" />
            <px:PXDSCallbackCommand Name="Release" StartNewGroup="True" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Document" DefaultControlID="edBatchNbr" 
        FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" NoteIndicator="True" NotifyIndicator="true" >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" AutoRefresh="True" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDate" runat="server" DataField="Date"/>
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXDropDown ID="edBatchTranType" runat="server" DataField="TranType" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" CommitChanges="True" AllowEdit="True"  />
            <px:PXSelector ID="edProdOrdID" runat="server" AutoRefresh="True" DataField="ProdOrdID" CommitChanges="True" AllowEdit="True" >
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AutoCallBack="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" 
                AutoCallBack="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" AllowEdit="True"/>
            <px:PXSelector ID="edUnitDesc" CommitChanges="True" runat="server" DataField="UOM" />
            <px:PXNumberEdit ID="edBatchQty" runat="server" DataField="Qty" Width="150px" CommitChanges="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
<px:PXTab ID="tab" runat="server" Width="100%" >
    <Items>
        <px:PXTabItem Text="Material">
            <Template>
                <px:PXGrid ID="gridMaterial" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" SyncPosition="True" 
                    StatusField="Availability" FilesIndicator="True" NoteIndicator="True" >
                    <Levels>
                        <px:PXGridLevel DataMember="MaterialTransactionRecords" DataKeyNames="DocType,BatNbr,LineNbr">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                                <px:PXCheckBox ID="chkTranOverride" runat="server" DataField="TranOverride" CommitChanges="True" />
                                <px:PXCheckBox ID="chkIsScrap" runat="server" DataField="IsScrap" CommitChanges="True" />
                                <px:PXDropDown ID="edLineTranType" runat="server" DataField="TranType" CommitChanges="True" />
                                <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" AutoRefresh="True"  />
                                <px:PXSegmentMask ID="edLineInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                <px:PXSegmentMask ID="edLineSubItemID" runat="server" DataField="SubItemID" />
                                <px:PXSegmentMask ID="edLineSiteID" runat="server" DataField="SiteID" AllowEdit="True" AutoRefresh="True" />
                                <px:PXSegmentMask ID="edLineLocationID" runat="server" AllowEdit="True" DataField="LocationID" AutoRefresh="True" />
                                <px:PXNumberEdit ID="edMatlQty" runat="server" DataField="Qty" />
                                <px:PXSelector ID="edLineUOM" runat="server" DataField="UOM" AutoRefresh="True"/>
                                <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" AutoRefresh="True" NullText="<SPLIT>" />
                                <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" />
                                <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
                                <px:PXNumberEdit ID="edTranAmt" runat="server" DataField="TranAmt" />
                                <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                <px:PXSelector ID="edReasonCodeID" runat="server" DataField="ReasonCodeID" CommitChanges="True" AllowEdit="True" />
                                <px:PXSelector ID="edMatlLineId" runat="server" DataField="MatlLineId" />
                                <px:PXTextEdit ID="edLotSerFG" runat="server" DataField="LotSerFG" />
                                <px:PXSelector ID="edGLBatNbr" runat="server" DataField="GLBatNbr" AllowEdit="True"/>
                                <px:PXNumberEdit ID="edGLLineNbr" runat="server" DataField="GLLineNbr" />
                                <px:PXDropDown ID="edINDocType" runat="server" DataField="INDocType" />
                                <px:PXSelector ID="edINBatNbr" runat="server" DataField="INBatNbr" AllowEdit="True" />
                                <px:PXNumberEdit ID="edINLineNbr" runat="server" DataField="INLineNbr" />
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="LineNbr" Width="81px" />
                                <px:PXGridColumn DataField="TranOverride" TextAlign="Center" Type="CheckBox" Width="90px" AutoCallBack="True" />
                                <px:PXGridColumn DataField="IsScrap" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                <px:PXGridColumn DataField="TranType" AutoCallBack="True" Width="100px" />
                                <px:PXGridColumn DataField="OperationID" AutoCallBack="True"  />
                                <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" Width="130px" />
                                <px:PXGridColumn DataField="SubItemID" AutoCallBack="True" Width="81px" />
                                <px:PXGridColumn DataField="SiteID" AutoCallBack="True" Width="130px" />
                                <px:PXGridColumn DataField="LocationID" NullText="&lt;SPLIT&gt;" AutoCallBack="True" Width="130px" />
                                <px:PXGridColumn DataField="Qty" AutoCallBack="True" Width="130px" />
                                <px:PXGridColumn DataField="UOM" AutoCallBack="True" Width="80px" />
                                <px:PXGridColumn DataField="LotSerialNbr" Width="180px" NullText="<SPLIT>" />
                                <px:PXGridColumn DataField="ExpireDate" Width="90px" />
                                <px:PXGridColumn DataField="UnitCost" Width="130px" AutoCallBack="true" />
                                <px:PXGridColumn DataField="TranAmt" Width="130px" />
                                <px:PXGridColumn DataField="TranDesc" Width="130px" />
                                <px:PXGridColumn DataField="ReasonCodeID" Width="90px" AutoCallBack="true" />
                                <px:PXGridColumn DataField="MatlLineId" AutoCallBack="True" Width="80px" />
                                <px:PXGridColumn DataField="LotSerFG" Width="180px" AutoCallBack="true" />
                                <px:PXGridColumn DataField="GLBatNbr" Width="99px" />
                                <px:PXGridColumn DataField="GLLineNbr" Width="99px" />
                                <px:PXGridColumn DataField="INDocType" Width="99px" />
                                <px:PXGridColumn DataField="INBatNbr" Width="99px" />
                                <px:PXGridColumn DataField="INLineNbr" Width="99px" />
                            </Columns>
			            </px:PXGridLevel>
		            </Levels>
                    <Mode AllowUpload="True" InitNewRow="True" />
                    <AutoSize Enabled="True" MinHeight="150" />
		            <ActionBar ActionsText="False">
		                <CustomItems>
		                    <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSAMDisassembleMaterialTran_binLotSerial" CommandSourceID="ds" DependOnGrid="gridMaterial" />
		                    <px:PXToolBarButton Text="CopyLine" CommandName="CopyLine" CommandSourceID="ds" DependOnGrid="gridMaterial" />
                        </CustomItems>
		            </ActionBar>
	            </px:PXGrid>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="Attributes">
            <Template>
                <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="DetailsInTab" SyncPosition="True" Width="100%">
                    <Levels>
                        <px:PXGridLevel DataKeyNames="OrderType,ProdOrdID,LineNbr" DataMember="TransactionAttributes">
                            <Columns>
                                <px:PXGridColumn DataField="OrderType"/>
                                <px:PXGridColumn DataField="ProdOrdID"/>
                                <px:PXGridColumn DataField="LineNbr"/>
                                <px:PXGridColumn DataField="AttributeID" Width="120px" />
                                <px:PXGridColumn DataField="Label" Width="120px" />
                                <px:PXGridColumn DataField="Descr" Width="200px" />
                                <px:PXGridColumn DataField="TransactionRequired" TextAlign="Center" Type="CheckBox" Width="85px" />
                                <px:PXGridColumn DataField="Value" Width="220px" MatrixMode="True" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="150" />
                </px:PXGrid>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="Allocations">
            <Template>
                <px:PXFormView ID="lsoptform" runat="server" Width="100%" DataMember="LSAMDisassembleMasterTran_lotseropts"
                    DataSourceID="ds" SkinID="Transparent">
                    <Template>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                        <px:PXNumberEdit ID="edUnassignedQty2" runat="server" DataField="UnassignedQty" Enabled="False" />
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty">
                            <AutoCallBack>
                                <Behavior CommitChanges="True" />
                            </AutoCallBack>
                        </px:PXNumberEdit>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                        <px:PXMaskEdit ID="edStartNumVal2" runat="server" DataField="StartNumVal" />
                        <px:PXButton ID="btnGenerate2" runat="server" Text="Generate" CommandName="LSAMDisassembleMasterTran_generateLotSerial"
                            CommandSourceID="ds" />
                    </Template>
                </px:PXFormView>
                <px:PXGrid runat="server" ID="serialNumbersGrid" DataSourceID="ds" BorderStyle="None" Width="100%" SkinID="DetailsInTab"> 
                    <Mode InitNewRow="True" />
                    <Levels>
                        <px:PXGridLevel DataMember="MasterSplits" DataKeyNames="DocType,BatNbr,LineNbr,SplitLineNbr">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                <px:PXDateTimeEdit ID="edExpireDateDsn" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                <px:PXSegmentMask ID="edInventoryIDsn" runat="server" DataField="InventoryID" AllowEdit="True">
                                </px:PXSegmentMask>
                                <px:PXSegmentMask ID="edSubItemIDsn" runat="server" DataField="SubItemID" />
                                <px:PXSegmentMask ID="edLocationIDsn" runat="server" DataField="LocationID" />
                                <px:PXSelector ID="edLotSerialNbrsn" runat="server" DataField="LotSerialNbr" />
                                <px:PXSelector ID="edUOMsn" runat="server" DataField="UOM" Enabled="False" />
                                <px:PXNumberEdit ID="edQtysn" runat="server" DataField="Qty" />

                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-&gt;AA" Width="81px" />
                                <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" DisplayFormat="&gt;CCCCCC-CCCC" Width="81px" />
                                <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="198px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" />
                                <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" Width="90px" />
                                <px:PXGridColumn DataField="ExpireDate" DisplayFormat="d" Width="90px" />
                            </Columns>
                            <Layout FormViewHeight="" />
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="150" />
                </px:PXGrid>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="References">
            <Template>
                <px:PXFormView ID="Referencesform" runat="server" Width="100%" DataSourceID="ds" DataMember="CurrentDocument" SkinID="Transparent">
                    <Template>
                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXSegmentMask ID="edRefBranchID" runat="server" DataField="BranchID" />
                        <px:PXDropDown ID="edRefINDocType" runat="server" DataField="INDocType" />
                        <px:PXSelector ID="edRefINBatNbr" runat="server" DataField="INBatNbr" Enabled="False" AllowEdit="True" />
                        <px:PXTextEdit ID="edRefTranDesc" runat="server" DataField="TranDesc" Width="400px" />
                    </Template>
                </px:PXFormView>
            </Template>
        </px:PXTabItem>
    </Items>
    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
</px:PXTab>
    <px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108; left: 252px; position: absolute; top: 531px; height: 360px;"
        Width="764px" Caption="Material Allocations" DesignView="Content" CaptionVisible="True" Key="lsselect" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSAMDisassembleMaterialTran_lotseropts"
            DataSourceID="ds" SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="gridMaterial" />
            </Parameters>
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSAMDisassembleMaterialTran_generateLotSerial"
                    CommandSourceID="ds">
                </px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" Style="border-width: 1px 0px;
            left: 0px; top: 0px; height: 192px;" SyncPosition="true">
            <AutoSize Enabled="true" />
            <Mode InitNewRow="True" />
            <CallbackCommands>
                <InitRow RepaintControlsIDs="grid2" />
            </CallbackCommands>
            <Parameters>
                <px:PXSyncGridParam ControlID="gridMaterial" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="MaterialSplits">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryID" Width="108px" />
                        <px:PXGridColumn DataField="SubItemID" Width="108px" />
                        <px:PXGridColumn DataField="LocationID" Width="108px" />
                        <px:PXGridColumn DataField="LotSerialNbr" Width="108px" />
                        <px:PXGridColumn DataField="Qty" Width="108px" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" Width="108px" />
                        <px:PXGridColumn DataField="ExpireDate" Width="90px" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule ID="PXLayoutRule20" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty3" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM3" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="componentsGrid" Name="AMDisassembleTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMDisassembleTranSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
