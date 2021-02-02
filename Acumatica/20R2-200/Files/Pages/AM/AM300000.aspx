<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM300000.aspx.cs" Inherits="Page_AM300000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
        <px:pxdatasource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="batch" TypeName="PX.Objects.AM.MaterialEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="LSAMMaterial_generateLotSerial" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="LSAMMaterial_binLotSerial" CommitChanges="True" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="Release" StartNewGroup="True" />
        </CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
        <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="batch" Caption="Document Summary"
                       NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" DefaultControlID="edBatNbr">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edBatNbr" runat="server" DataField="BatNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate"/>
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID"  />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edOrigBatNbr" runat="server" DataField="OrigBatNbr" Enabled="False" AllowEdit="True" />
            <px:PXDropDown ID="edOrigDocType" runat="server" DataField="OrigDocType" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXPanel ID="PXPanel1" runat="server" ContentLayout-Layout="Stack" RenderSimple="True" RenderStyle="Simple">
                <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty" Enabled="False"  />
                <px:PXNumberEdit ID="edControlQty" runat="server" DataField="ControlQty" />
                <px:PXNumberEdit ID="edTotalAmount" runat="server" DataField="TotalAmount" Enabled="False"  />
                <px:PXNumberEdit ID="edControlAmount" runat="server" DataField="ControlAmount"  />
            </px:PXPanel>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
        <px:pxgrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" SyncPosition="True" TabIndex="3300" TemporaryFilterCaption="Filter Applied" StatusField="Availability">
		<Levels>
			<px:PXGridLevel DataMember="transactions" DataKeyNames="DocType,BatNbr,LineNbr">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />                   
                    <px:PXSelector ID="edProdOrdID" runat="server" AutoRefresh="True" DataField="ProdOrdID" AllowEdit="True" CommitChanges="true" >
                        <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <AutoCallBack Command="Cancel" Target="ds" />
                    </px:PXSelector>
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" AutoRefresh="True" CommitChanges="true" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSegmentMask ID="edInventoryID" Size="xs" runat="server" DataField="InventoryID" AutoRefresh="True" AllowEdit="True" CommitChanges="true" >
						<GridProperties>
							<PagerSettings Mode="NextPrevFirstLast" />
						</GridProperties>
					</px:PXSegmentMask>
                    <px:PXSegmentMask Size="xxs" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" CommitChanges="true" NullText="<SPLIT>">
					    <Parameters>
						    <px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
							    Type="String" />
					    </Parameters>
				    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" />
							<px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" />
						</Parameters>
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" NullText="<SPLIT>">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]"
								Type="String" />
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" />
							<px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" />
						</Parameters>
					</px:PXSegmentMask>
                    <px:PXNumberEdit Size="xs" ID="edQty1" runat="server" DataField="Qty" CommitChanges="true" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector ID="edUOM1" Size="xs" runat="server" DataField="UOM" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
						</Parameters>
					</px:PXSelector>
                    <px:PXLayoutRule runat="server" />
                    <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" NullText="<SPLIT>" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" />
							<px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" />
							<px:PXControlParam ControlID="grid" Name="AMMTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
								Type="String" />
							<px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
						</Parameters>
					</px:PXSelector>
                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                    <px:PXTextEdit ID="edLotSerFG" runat="server" AllowNull="False" DataField="LotSerFG"/>
                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" Enabled="False"  />
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXNumberEdit ID="edTranAmt" runat="server" DataField="TranAmt" Enabled="False"  />
                    <px:PXSelector ID="edGLBatNbr" runat="server" DataField="GLBatNbr" Enabled="False" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edGLLineNbr" runat="server" DataField="GLLineNbr" Enabled="False"/>
                    <px:PXDropDown ID="edINDocType" runat="server" DataField="INDocType" Enabled="False"/>
                    <px:PXSelector ID="edINBatNbr" runat="server" DataField="INBatNbr" Enabled="False" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edINLineNbr" runat="server" DataField="INLineNbr" Enabled="False"/>
                    <px:PXCheckBox ID="chkIsByproduct" runat="server" DataField="IsByproduct" />
                    <px:PXSelector ID="edMatlLineID" runat="server" DataField="MatlLineId" AutoRefresh="True" CommitChanges="true" />
                    <px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" Enabled="False"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="70px"/>
                    <px:PXGridColumn DataField="OrderType" Width="90px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="130px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="OperationID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SiteID" Width="130px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="LocationID" Width="130px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="108px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="UOM" Width="75px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="LotSerialNbr" NullText="&lt;SPLIT&gt;" Width="130px" />
                    <px:PXGridColumn DataField="ExpireDate" Width="90px" />
                    <px:PXGridColumn DataField="LotSerFG" Width="130px" />
                    <px:PXGridColumn DataField="UnitCost" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="TranAmt"  TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="GLBatNbr" Width="99px" />
                    <px:PXGridColumn DataField="GLLineNbr" Width="99px" />
                    <px:PXGridColumn DataField="INDocType" Width="99px" />
                    <px:PXGridColumn DataField="INBatNbr" Width="99px" />
                    <px:PXGridColumn DataField="INLineNbr" Width="99px" />
                    <px:PXGridColumn DataField="IsByproduct" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                    <px:PXGridColumn DataField="MatlLineId" Width="99px" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID_description" Width="200px"/>
                    <px:PXGridColumn DataField="TranDesc" Width="225px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowUpload="True" InitNewRow="True" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False">
            <CustomItems>
		        <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSAMMaterial_binLotSerial" CommandSourceID="ds" DependOnGrid="grid" />
            </CustomItems>
		</ActionBar>
	</px:pxgrid>
    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="764px" Caption="Allocations" DesignView="Content" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" Height="500px">

        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSAMMaterial_lotseropts" DataSourceID="ds"
            SkinID="Transparent" TabIndex="700">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSAMMaterial_generateLotSerial" CommandSourceID="ds">
                </px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" Height="192px" SkinID="Details">
            <Mode InitNewRow="True" />
            <AutoSize Enabled="true" />
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="splits">
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
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMMTranSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
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
