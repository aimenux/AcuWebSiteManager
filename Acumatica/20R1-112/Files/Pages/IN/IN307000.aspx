<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN307000.aspx.cs"
    Inherits="Page_IN307000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.KitAssemblyEntry" PrimaryView="Document" EnableAttributes="true" >
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Release" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINComponentTran_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINComponentTran_binLotSerial" DependOnGrid="componentsGrid" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINComponentMasterTran_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINComponentMasterTran_binLotSerial" DependOnGrid="componentsGrid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Kit Summary" DataMember="Document"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="true" NotifyIndicator="true" ActivityIndicator="true" ActivityField="NoteActivity"
        EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edRefNbr">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" SelectedIndex="-1" />
            <px:PXSelector runat="server" DataField="RefNbr" ID="edRefNbr" AutoRefresh="true" />
            <px:PXDropDown runat="server" Enabled="False" DataField="Status" ID="edStatus" />
            <px:PXCheckBox CommitChanges="True" runat="server" DataField="Hold" ID="chkHold" />
            <px:PXDateTimeEdit runat="server" DataField="TranDate" ID="edTranDate" CommitChanges="true" />
            <px:PXSelector runat="server" DataField="FinPeriodID" ID="edFinPeriodID" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edKitInventoryID" runat="server" DataField="KitInventoryID" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="KitRevisionID" ID="edKitRevisionID" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="SubItemID" ID="edSubItemID" AutoRefresh="True" />
            <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode">
                <Parameters>
                    <px:PXControlParam ControlID="form" Name="INKitRegister.docType" PropertyName="NewDataKey[&quot;DocType&quot;]" Type="String" />
                </Parameters>
            </px:PXSelector>
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="2" />
            <px:PXTextEdit runat="server" DataField="TranTranDesc" ID="edTranDesc" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="SiteID" ID="edSiteID" AutoRefresh="true" />
            <px:PXSegmentMask runat="server" DataField="LocationID" ID="edLocationID" AutoRefresh="true" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="UOM" ID="edUOM" />
            <px:PXNumberEdit CommitChanges="True" runat="server" DataField="Qty" ID="edQty" /></Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="DocumentProperties">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Stock Components">
                <Template>
                    <px:PXGrid runat="server" ID="componentsGrid" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" 
                        StatusField="Availability" TabIndex="10300" SyncPosition="true">
                        <Mode InitNewRow="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSINComponentTran_binLotSerial" CommandSourceID="ds" DependOnGrid="componentsGrid">
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Components" DataKeyNames="DocType,RefNbr,LineNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AutoRefresh ="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="componentsGrid" Name="SyncGrid" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
                                    <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode" />
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXNumberEdit ID="edINKitSpecStkDet__DfltCompQty" runat="server" DataField="INKitSpecStkDet__DfltCompQty" />
                                    <px:PXSelector ID="edINKitSpecStkDet__UOM" runat="server" DataField="INKitSpecStkDet__UOM" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox ID="chkINKitSpecStkDet__AllowQtyVariation" runat="server" DataField="INKitSpecStkDet__AllowQtyVariation" />
                                    <px:PXNumberEdit ID="edINKitSpecStkDet__MinCompQty" runat="server" DataField="INKitSpecStkDet__MinCompQty" />
                                    <px:PXNumberEdit ID="edINKitSpecStkDet__MaxCompQty" runat="server" DataField="INKitSpecStkDet__MaxCompQty" />
                                    <px:PXNumberEdit ID="edINKitSpecStkDet__DisassemblyCoeff" runat="server" DataField="INKitSpecStkDet__DisassemblyCoeff" />
                                    <px:PXCheckBox ID="chkINKitSpecStkDet__AllowSubstitution" runat="server" DataField="INKitSpecStkDet__AllowSubstitution" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" Visible="False" AllowShowHide="False" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAA"
                                        AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-&gt;AA" />
                                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;CCCCCC-CCCC" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ReasonCode" DisplayFormat="&gt;aaaaaaaaaa" />
									<px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn AllowNull="False" DataField="INKitSpecStkDet__DfltCompQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="INKitSpecStkDet__UOM" DisplayFormat="&gt;aaaaaa" />
                                    <px:PXGridColumn AllowNull="False" DataField="INKitSpecStkDet__AllowQtyVariation" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="INKitSpecStkDet__MinCompQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="INKitSpecStkDet__MaxCompQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="INKitSpecStkDet__DisassemblyCoeff" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="INKitSpecStkDet__AllowSubstitution" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Non-Stock Components">
                <Template>
                    <px:PXGrid runat="server" ID="overheadGrid" DataSourceID="ds" BorderStyle="None" Width="100%" SkinID="Details">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="Overhead" DataKeyNames="DocType,RefNbr,LineNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" />
                                    <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                                    <px:PXSelector ID="edReasonCode2" runat="server" DataField="ReasonCode" />
									<px:PXTextEdit ID="edTranDesc2" runat="server" DataField="TranDesc" />
                                    <px:PXLayoutRule runat="server" Merge="True" />
                                    <px:PXSegmentMask Size="m" ID="edInventoryID2" runat="server" DataField="InventoryID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXLabel Size="xs" ID="lblInventoryIDH2" runat="server"></px:PXLabel>
                                    <px:PXLayoutRule runat="server" />
                                    <px:PXNumberEdit ID="edINKitSpecNonStkDet__DfltCompQty" runat="server" DataField="INKitSpecNonStkDet__DfltCompQty" />
                                    <px:PXSelector ID="edINKitSpecNonStkDet__UOM" runat="server" DataField="INKitSpecNonStkDet__UOM" />
                                    <px:PXCheckBox ID="chkINKitSpecNonStkDet__AllowQtyVariation" runat="server" DataField="INKitSpecNonStkDet__AllowQtyVariation" />
                                    <px:PXNumberEdit ID="edINKitSpecNonStkDet__MinCompQty" runat="server" DataField="INKitSpecNonStkDet__MinCompQty" />
                                    <px:PXNumberEdit ID="edINKitSpecNonStkDet__MaxCompQty" runat="server" DataField="INKitSpecNonStkDet__MaxCompQty" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAA" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ReasonCode" DisplayFormat="&gt;aaaaaaaaaa" />
									<px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn AllowNull="False" DataField="INKitSpecNonStkDet__DfltCompQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="INKitSpecNonStkDet__UOM" DisplayFormat="&gt;aaaaaa" />
                                    <px:PXGridColumn AllowNull="False" DataField="INKitSpecNonStkDet__AllowQtyVariation" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="INKitSpecNonStkDet__MinCompQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="INKitSpecNonStkDet__MaxCompQty" TextAlign="Right" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Kit Allocations">
                <Template>
                    <px:PXFormView ID="lsoptform" runat="server" Width="100%" DataMember="LSINComponentMasterTran_lotseropts"
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
                            <px:PXButton ID="btnGenerate2" runat="server" Text="Generate" CommandName="LSINComponentMasterTran_generateLotSerial"
                                CommandSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" ID="serialNumbersGrid" DataSourceID="ds" BorderStyle="None" Width="100%" SkinID="DetailsInTab"> 
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="MasterSplits" DataKeyNames="DocType,RefNbr,LineNbr,SplitLineNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                    <px:PXSegmentMask ID="edInventoryIDsn" runat="server" DataField="InventoryID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemIDsn" runat="server" DataField="SubItemID" />
                                    <px:PXSegmentMask ID="edLocationIDsn" runat="server" DataField="LocationID" />
                                    <px:PXSelector ID="edLotSerialNbrsn" runat="server" DataField="LotSerialNbr" />
                                    <px:PXSelector ID="edUOMsn" runat="server" DataField="UOM" Enabled="False" />
                                    <px:PXNumberEdit ID="edQtysn" runat="server" DataField="Qty" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-&gt;AA" />
                                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;CCCCCC-CCCC" />
                                    <px:PXGridColumn DataField="LotSerialNbr" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                                    <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ExpireDate" DisplayFormat="d" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108; left: 252px; position: absolute; top: 531px; height: 500px;"
        Width="764px" Caption="Allocations" DesignView="Content" CaptionVisible="True" Key="lsselect" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSINComponentTran_lotseropts"
            DataSourceID="ds" SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="componentsGrid" />
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
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSINComponentTran_generateLotSerial"
                    CommandSourceID="ds">
                </px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SyncPosition="true" SkinID="Details">
            <AutoSize Enabled="true" />
            <Mode InitNewRow="True" />
            <CallbackCommands>
                <InitRow RepaintControlsIDs="grid2" />
            </CallbackCommands>
            <Parameters>
                <px:PXSyncGridParam ControlID="componentsGrid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="ComponentSplits">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="LotSerialNbr" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ExpireDate" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule ID="PXLayoutRule20" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty3" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM3" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="componentsGrid" Name="INComponentTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                    Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INComponentTranSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
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
