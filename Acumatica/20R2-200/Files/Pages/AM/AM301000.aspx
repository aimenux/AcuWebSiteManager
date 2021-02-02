<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM301000.aspx.cs" Inherits="Page_AM301000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="batch" TypeName="PX.Objects.AM.LaborEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMMove_generateLotSerial" Visible="False"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMMove_binLotSerial" Visible="False" DependOnGrid="grid"/>
            <px:PXDSCallbackCommand Name="Release" StartNewGroup="True" />
        </CallbackCommands>
	</px:PXDataSource>
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
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXPanel ID="PXPanel1" runat="server" ContentLayout-Layout="Stack" RenderSimple="True" RenderStyle="Simple">
                <px:PXNumberEdit ID="edControlQty" runat="server" DataField="ControlQty"  />
                <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty" Enabled="False"  />
            </px:PXPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataMember="transactions" DataKeyNames="DocType,BatNbr,LineNbr">
                <RowTemplate>
                    <px:PXDropDown ID="edLaborType" runat="server" AllowNull="False" DataField="LaborType" CommitChanges="True" 
                        AllowEdit="True" AutoCallBack="True" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
                    <px:PXSelector ID="edProdOrdID" runat="server" AutoRefresh="True" DataField="ProdOrdID" AllowEdit="True" >
                        <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <AutoCallBack Command="Cancel" Target="ds" />
                    </px:PXSelector>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowAddNew="True" AllowEdit="True">
						<GridProperties>
							<PagerSettings Mode="NextPrevFirstLast" ></PagerSettings>
						</GridProperties>
					</px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" NullText="<SPLIT>">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
						</Parameters>
					</px:PXSegmentMask>
					<px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" DataSourceID="ds" AutoRefresh="True"  />
                    <px:PXSelector ID="edLaborCodeID" runat="server" DataField="LaborCodeID" AllowEdit="True" />
                    <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edStartTime" runat="server" DataField="StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True"/>
                    <px:PXDateTimeEdit ID="edEndTime" runat="server" DataField="EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True"/>
                    <px:PXTimeSpan ID="edLaborTime" TimeMode="True" runat="server" DataField="LaborTime" InputMask="hh:mm" CommitChanges="True" />
                    <px:PXCheckBox ID="chkIsScrap" runat="server" DataField="IsScrap" CommitChanges="True" />
					<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty"  />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" ></px:PXControlParam>
						</Parameters>
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" NullText="<SPLIT>">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" ></px:PXControlParam>
						</Parameters>
					</px:PXSegmentMask>
                    <px:PXSelector ID="edUOM1" runat="server" DataField="UOM" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
						</Parameters>
					</px:PXSelector>
					<px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" NullText="<SPLIT>" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMMTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXSyncGridParam ControlID="grid" Name="SyncGrid" ></px:PXSyncGridParam>
						</Parameters>
					</px:PXSelector>
                    <px:PXSelector Size="s" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="true">
                        <GridProperties>
						    <Columns>
							    <px:PXGridColumn DataField="ReceiptNbr"  Width="70px">
								    <Header Text="Receipt Nbr"/>
							    </px:PXGridColumn>
							    <px:PXGridColumn DataField="ReceiptDate" DataType="DateTime" Width="90px">
								    <Header Text="Receipt Date"/>
							    </px:PXGridColumn>
							    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyOnHand" DataType="Decimal"
								    Width="100px">
								    <Header Text="Quantity On Hand"/>
							    </px:PXGridColumn>
							    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TotalCost" DataType="Decimal"
								    Width="100px">
								    <Header Text="Total Cost"/>
							    </px:PXGridColumn>
						    </Columns>
						    <Layout ColumnsMenu="False" />
						    <PagerSettings Mode="NextPrevFirstLast" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="grid" Name="AMMTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            <px:PXControlParam ControlID="grid" Name="AMMTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                            <px:PXControlParam ControlID="grid" Name="AMMTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            <px:PXControlParam ControlID="grid" Name="AMMTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                    <px:PXNumberEdit ID="edQtyScrapped" runat="server" DataField="QtyScrapped" CommitChanges="True" />
                    <px:PXSelector CommitChanges="True" ID="edReasonCodeID" runat="server" DataField="ReasonCodeID" AllowEdit="True" />
                    <px:PXDropDown ID="edScrapAction" runat="server" DataField="ScrapAction" CommitChanges="True" />
                    <px:PXSelector ID="edGLBatNbr" runat="server" DataField="GLBatNbr" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edGLLineNbr" runat="server" DataField="GLLineNbr" />
                    <px:PXDropDown ID="edINDocType" runat="server" DataField="INDocType" />
                    <px:PXSelector ID="edINBatNbr" runat="server" DataField="INBatNbr" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edINLineNbr" runat="server" DataField="INLineNbr" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                    <px:PXGridColumn DataField="LaborType" AutoCallBack="True" />
                    <px:PXGridColumn DataField="OrderType" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProdOrdID" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="OperationID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="LaborCodeID" AutoCallBack="True" Width="100px" />
                    <px:PXGridColumn DataField="EmployeeID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ShiftID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="StartTime" Width="90px" TimeMode="true" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="EndTime" Width="90px" TimeMode="true" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="LaborTime" Width="60px" AutoCallBack="True" RenderEditorText="True"/>
                    <px:PXGridColumn AllowNull="False" DataField="LaborRate" TextAlign="Right" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn AllowNull="False" DataField="ExtCost" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="IsScrap" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Qty" Width="99px" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn DataField="UOM" />
                    <px:PXGridColumn DataField="SiteID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="LocationID" Width="81px" NullText="&lt;SPLIT&gt;" AutoCallBack="True" />
                    <px:PXGridColumn DataField="QtyScrapped" TextAlign="Right" Width="99px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ReasonCodeID" Width="90px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="ScrapAction" Width="80px" TextAlign="Left" AutoCallBack="True" />
                    <px:PXGridColumn DataField="GLBatNbr" Width="99px" />
                    <px:PXGridColumn DataField="GLLineNbr" Width="99px" />
                    <px:PXGridColumn DataField="INDocType" Width="99px" />
                    <px:PXGridColumn DataField="INBatNbr" Width="99px" />
                    <px:PXGridColumn DataField="INLineNbr" Width="99px" />
                    <px:PXGridColumn DataField="LotSerialNbr" Width="180px" AutoCallBack="true" NullText="<SPLIT>" />
					<px:PXGridColumn DataField="ExpireDate" Width="90px" />
                    <px:PXGridColumn DataField="ReceiptNbr" Width="81px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="TranDesc" Width="225px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowUpload="True" InitNewRow="True" />
		<AutoSize Container="Window" Enabled="True" MinHeight="250" />
		<ActionBar ActionsText="False">
		    <CustomItems>
		        <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSAMMove_binLotSerial" CommandSourceID="ds" DependOnGrid="grid" />
                <px:PXToolBarButton Text="Attributes" PopupPanel="AttributePanel" Enabled="True" />
            </CustomItems>
		</ActionBar>
	</px:PXGrid>
    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="764px" Caption="Allocations" DesignView="Content" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" Height="500px">

        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSAMMove_lotseropts" DataSourceID="ds"
            SkinID="Transparent" TabIndex="700">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty1" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSAMMove_generateLotSerial" CommandSourceID="ds">
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
    <px:PXSmartPanel ID="AttributePanel" runat="server" Width="720px" Height="360px" Caption="Production Attributes" CaptionVisible="True" 
        AutoCallBack-Command="Refresh" AutoCallBack-Target="AttributesGrid" DesignView="Content" >
        <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="Details" AutoAdjustColumns="True" SyncPosition="true"
            AdjustPageSize="Auto" AllowPaging="True" AllowSearch="true" Width="685px" Height="200px" >
            <Levels>
                <px:PXGridLevel DataKeyNames="DocType,BatNbr,TranLineNbr,LineNbr" DataMember="TransactionAttributes">
                    <Columns>
                        <px:PXGridColumn DataField="DocType" />
                        <px:PXGridColumn DataField="BatNbr" />
                        <px:PXGridColumn DataField="TranLineNbr"/>
                        <px:PXGridColumn DataField="LineNbr" />
                        <px:PXGridColumn DataField="AttributeID" />
                        <px:PXGridColumn DataField="Label" Width="120px" />
                        <px:PXGridColumn DataField="Descr" Width="200px" />
                        <px:PXGridColumn DataField="TransactionRequired" TextAlign="Center" Type="CheckBox" Width="85px" />
                        <px:PXGridColumn DataField="Value" Width="220px" MatrixMode="true" CommitChanges="true" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        <AutoSize Enabled="True" MinHeight="200"/>
        </px:PXGrid>
        <px:PXPanel ID="AttributeButtonPanel" runat="server" SkinID="Buttons">
            <px:PXButton ID="AttributeSaveButton" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="AttributeCancelButton" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>