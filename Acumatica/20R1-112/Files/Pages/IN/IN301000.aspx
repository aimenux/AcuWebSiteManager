<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN301000.aspx.cs"
    Inherits="Page_IN301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INReceiptEntry" PrimaryView="receipt">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINTran_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINTran_binLotSerial" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="True" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="False" Name="INEdit" />
            <px:PXDSCallbackCommand Visible="False" Name="INRegisterDetails" />
            <px:PXDSCallbackCommand Visible="False" Name="INItemLabels" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="receipt" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" DefaultControlID="edRefNbr">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" Visible="false"/>
            <px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edTransferNbr" runat="server" DataField="TransferNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXPanel ID="PXPanel1" runat="server" ContentLayout-Layout="Stack" RenderSimple="True" RenderStyle="Simple">
                <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty" Enabled="False" />
                <px:PXNumberEdit ID="edControlQty" runat="server" CommitChanges="True" DataField="ControlQty" />
                <px:PXNumberEdit ID="edTotalCost" runat="server" DataField="TotalCost" Enabled="False" />
                <px:PXNumberEdit ID="edControlCost" runat="server" CommitChanges="True" DataField="ControlCost" />
            </px:PXPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" style="z-index: 100;" width="100%">
			<Items>
				<px:PXTabItem Text="Transaction Details">
					<Template>
						<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 250px;"
							Width="100%" SkinID="DetailsInTab" StatusField="Availability" SyncPosition="True">
							<AutoSize Enabled="True" MinHeight="250" />
							<Mode InitNewRow="True" AllowUpload="True" />
							<ActionBar>
								<CustomItems>
									<px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSINTran_binLotSerial" CommandSourceID="ds"
										DependOnGrid="grid" />
								    <px:PXToolBarButton Text="Add Item" Key="cmdASI">
										<AutoCallBack Command="AddInvBySite" Target="ds">
											<Behavior PostData="Page" CommitChanges="True" />
										</AutoCallBack>
								    </px:PXToolBarButton>
								</CustomItems>
							</ActionBar>
							<Levels>
								<px:PXGridLevel DataMember="transactions">
									<RowTemplate>
										<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
										<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowAddNew="True" AllowEdit="True">
											<GridProperties>
												<PagerSettings Mode="NextPrevFirstLast" />
											</GridProperties>
										</px:PXSegmentMask>
										<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
										<px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>">
											<Parameters>
												<px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
													Type="String" />
											</Parameters>
										</px:PXSegmentMask>
										<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="true">
											<Parameters>
												<px:PXSyncGridParam ControlID="grid" />
											</Parameters>
										</px:PXSegmentMask>
                                                 <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="True" />
										<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True">
											<Parameters>
												<px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
													Type="String" />
												<px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
													Type="String" />
											</Parameters>
										</px:PXSegmentMask>
										<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" NullText="<SPLIT>">
											<Parameters>
												<px:PXControlParam ControlID="grid" Name="INTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]"
													Type="String" />
												<px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
													Type="String" />
												<px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
													Type="String" />
											</Parameters>
										</px:PXSegmentMask>
										<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
										<px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True">
											<Parameters>
												<px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
													Type="String" />
											</Parameters>
										</px:PXSelector>
										<px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
										<px:PXNumberEdit ID="edTranCost" runat="server" DataField="TranCost" />
										<px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" NullText="<SPLIT>" AutoRefresh="True">
											<Parameters>
												<px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
													Type="String" />
												<px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
													Type="String" />
												<px:PXControlParam ControlID="grid" Name="INTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
													Type="String" />
												<px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
											</Parameters>
										</px:PXSelector>
										<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
										<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
										<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
										<px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode">
											<Parameters>
												<px:PXControlParam ControlID="form" Name="INTran.docType" PropertyName="NewDataKey[&quot;DocType&quot;]"
													Type="String" />
											</Parameters>
										</px:PXSelector>
										<px:PXSelector ID="edPOReceiptNbr" runat="server" DataField="POReceiptNbr" AllowEdit="True" />
										<px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server" DataField="BranchID" /></RowTemplate>
									<Columns>
										<px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True"
											RenderEditorText="True" />
										<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" />
										<px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" NullText="<SPLIT>" AutoCallBack="true" />
										<px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" />
										<px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" NullText="<SPLIT>"
											AutoCallBack="true" />
										<px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" />
										<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
										<px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" />
										<px:PXGridColumn AllowNull="False" DataField="TranCost" TextAlign="Right" />
										<px:PXGridColumn DataField="LotSerialNbr" NullText="<SPLIT>" />
										<px:PXGridColumn DataField="ExpireDate" />
										<px:PXGridColumn DataField="ReasonCode" DisplayFormat="&gt;AAAAAAAAAA" />
										<px:PXGridColumn AutoCallBack="True" DataField="ProjectID" DisplayFormat="CCCCCCCCCC" Label="Project" />
										<px:PXGridColumn DataField="TaskID" DisplayFormat="CCCCCCCCCC" Label="Task" CommitChanges="true" />
										<px:PXGridColumn DataField="CostCodeID" CommitChanges="True"/>
										<px:PXGridColumn DataField="TranDesc" />
										<px:PXGridColumn DataField="POReceiptNbr" Visible="False" />
									</Columns>
								</px:PXGridLevel>
							</Levels>
						</px:PXGrid>
					</Template>
				</px:PXTabItem>
				<px:PXTabItem Text="Financial Details">
					<Template>
						<px:PXFormView ID="form2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" 
							DataMember="CurrentDocument" RenderStyle="Simple">
							<ContentLayout OuterSpacing="Around" />
							<Template>
								<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
								<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
								<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXTabItem>
			</Items>
			<AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="764px" Caption="Allocations" DesignView="Content" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" Height="500px">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSINTran_lotseropts" DataSourceID="ds"
            DefaultControlID="" SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
						<CallbackCommands>
							<Refresh RepaintControls="None" RepaintControlsIDs="grid2" />
						</CallbackCommands>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSINTran_generateLotSerial" CommandSourceID="ds">
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
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="splits">
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
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
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
    <script type="text/javascript">
        function UpdateItemSiteCell(n, c) {
            var activeRow = c.cell.row;
            var sCell = activeRow.getCell("Selected");
            var qCell = activeRow.getCell("QtySelected");
            if (sCell == c.cell) {
                if (sCell.getValue() == true)
                    qCell.setValue("1");
                else
                    qCell.setValue("0");
            }
            if (qCell == c.cell) {
                if (qCell.getValue() == "0")
                    sCell.setValue(false);
                else
                    sCell.setValue(true);
            }
        }
    </script>
    <px:PXSmartPanel id="PanelAddSiteStatus" runat="server" key="sitestatus" loadondemand="true" width="900px" height="500px"
        caption="Inventory Lookup" captionvisible="true" autocallback-command='Refresh' autocallback-enabled="True" autocallback-target="formSitesStatus"
        designview="Content">
		<px:PXFormView ID="formSitesStatus" runat="server" CaptionVisible="False" DataMember="sitestatusfilter"
			DataSourceID="ds" Width="100%" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
				<px:PXTextEdit ID="edInventory" runat="server" DataField="Inventory" />
				<px:PXTextEdit ID="edBarCode" runat="server" DataField="BarCode" CommitChanges="True"  />
                <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClass" DataSourceID="ds" CommitChanges="True"  />
			    <px:PXCheckBox ID="chkOnlyAvailable" runat="server" Checked="True" DataField="OnlyAvailable" CommitChanges="True"  />
				<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

				<px:PXSegmentMask ID="edSubItem" runat="server" DataField="SubItem" AutoRefresh="True" DataSourceID="ds" CommitChanges="True" />
                <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" DataSourceID="ds" CommitChanges="True" AutoRefresh="true" />
                <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" DataSourceID="ds" CommitChanges="True"  /></Template>
		</px:PXFormView>
		<px:PXGrid ID="gripSiteStatus" runat="server" DataSourceID="ds" Style="border-width: 1px 0px;
			top: 0px; left: 0px; height: 189px;" AutoAdjustColumns="true" Width="100%" SkinID="Details"
			AdjustPageSize="Auto" AllowSearch="True" FastFilterID="edInventory"
			FastFilterFields="InventoryCD,Descr" BatchUpdate="true" >
			<ClientEvents AfterCellUpdate="UpdateItemSiteCell" />			
			<ActionBar PagerVisible="False">
			    <PagerSettings Mode="NextPrevFirstLast"/>
			</ActionBar>
			<Levels>
				<px:PXGridLevel  
                    DataMember="siteStatus">
					<Mode AllowAddNew="false" AllowDelete="false" />
					<RowTemplate>
						<px:PXSegmentMask ID="editemClass" runat="server" DataField="ItemClassID" />
					</RowTemplate>
					<Columns>
						<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true" AllowCheckAll="true" />
						<px:PXGridColumn AllowNull="False" DataField="QtySelected" TextAlign="Right" />
						<px:PXGridColumn DataField="SiteID" />
						<px:PXGridColumn DataField="SiteCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
						<px:PXGridColumn DataField="LocationID" />
						<px:PXGridColumn DataField="LocationCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
						<px:PXGridColumn DataField="ItemClassID" />
						<px:PXGridColumn DataField="ItemClassDescription" />
						<px:PXGridColumn DataField="PriceClassID" />
						<px:PXGridColumn DataField="PriceClassDescription" />
						<px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
						<px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
						<px:PXGridColumn DataField="SubItemCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
						<px:PXGridColumn DataField="Descr" />
						<px:PXGridColumn DataField="BaseUnit" DisplayFormat="&gt;aaaaaa"  />
						<px:PXGridColumn AllowNull="False" DataField="QtyAvail" TextAlign="Right" />
						<px:PXGridColumn AllowNull="False" DataField="QtyOnHand" TextAlign="Right" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton5" runat="server" CommandName="AddInvSelBySite" CommandSourceID="ds" Text="Add" SyncVisible="false"/>
		    <px:PXButton ID="PXButton4" runat="server" Text="Add & Close" DialogResult="OK" />
		    <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
