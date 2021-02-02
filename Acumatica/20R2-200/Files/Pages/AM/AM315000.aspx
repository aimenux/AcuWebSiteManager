<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM315000.aspx.cs" Inherits="Page_AM315000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="header" TypeName="PX.Objects.AM.ClockEntry">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="ClockInOut" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMClockItem_generateLotSerial" Visible="False"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMClockItem_binLotSerial" Visible="True" />  
            <px:PXDSCallbackCommand CommitChanges="True" Name="FillCurrentUser" Visible="false" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="header" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" DefaultControlID ="edEmployeeID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" CommitChanges="true" AutoRefresh="true"/>
            <px:PXButton ID="btnCurrent" runat="server" Text="Current User" Height="20px" CommandName="FillCurrentUser" CommandSourceID="ds"></px:PXButton>
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXSelector CommitChanges="True" ID="edProdOrdID" runat="server" AutoRefresh="True" DataField="ProdOrdID" AllowEdit="True" />
            <px:PXSelector CommitChanges="True" ID="edOperationID" runat="server" DataField="OperationID" DataSourceID="ds" AutoRefresh="True"  />
            <px:PXSelector ID="edShift" runat="server" DataField="ShiftID" AllowEdit="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" CommitChanges="true"/>
            <px:PXSelector ID="edUOM1" runat="server" DataField="UOM" AutoRefresh="True"></px:PXSelector>
            <px:PXDateTimeEdit ID="edStartTime" runat="server" DataField="StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True"/>
            <px:PXTimeSpan ID="edLaborTime" TimeMode="True" runat="server" DataField="LaborTime" InputMask="hh:mm" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataMember="transactions" DataKeyNames="EmployeeID, LineNbr">
                <RowTemplate>
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate"/>
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
                    <px:PXSelector ID="edProdOrdID" runat="server" AutoRefresh="True" DataField="ProdOrdID" AllowEdit="True" >
                        <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSelector>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowAddNew="True" AllowEdit="True">
						<GridProperties>
							<PagerSettings Mode="NextPrevFirstLast" ></PagerSettings>
						</GridProperties>
					</px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" NullText="<SPLIT>">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
						</Parameters>
					</px:PXSegmentMask>
					<px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" DataSourceID="ds" AutoRefresh="True"  />
                    <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edStartTimeTran" runat="server" DataField="StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True"/>
                    <px:PXDateTimeEdit ID="edEndTime" runat="server" DataField="EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True"/>
                    <px:PXTimeSpan ID="edLaborTime" TimeMode="True" runat="server" DataField="LaborTime" InputMask="hh:mm" CommitChanges="True" />
					<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty"  />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" ></px:PXControlParam>
						</Parameters>
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" NullText="<SPLIT>">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								Type="String" ></px:PXControlParam>
						</Parameters>
					</px:PXSegmentMask>
                    <px:PXSelector ID="edUOM1" runat="server" DataField="UOM" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="AMClockTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								Type="String" ></px:PXControlParam>
						</Parameters>
					</px:PXSelector> 
                    <px:PXCheckBox ID="chkCloseFlg" runat="server" DataField="CloseFlg" CommitChanges="True" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />    
                    <px:PXGridColumn DataField="TranDate" Width="90px" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="StartTime" Width="90px" TimeMode="true" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="EndTime" Width="90px" TimeMode="true" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="LaborTime" Width="60px" AutoCallBack="True" RenderEditorText="True"/>   
                    <px:PXGridColumn DataField="OrderType" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProdOrdID" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="OperationID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID" Width="90px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="EmployeeID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ShiftID" Width="81px" AutoCallBack="True" />                 
                    <px:PXGridColumn DataField="Qty" Width="99px" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn DataField="UOM" />
                    <px:PXGridColumn DataField="SiteID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="LocationID" Width="81px" NullText="&lt;SPLIT&gt;" AutoCallBack="True" />
                    <px:PXGridColumn DataField="CloseFlg" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowUpload="True" InitNewRow="True" />
		<AutoSize Container="Window" Enabled="True" MinHeight="250" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="764px" Caption="Allocations" DesignView="Content" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" Height="500px">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSAMClockItem_lotseropts" DataSourceID="ds"
            SkinID="Transparent" TabIndex="700">
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
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSAMClockItem_generateLotSerial" CommandSourceID="ds">
                </px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" Height="192px" SkinID="Details">
            <Mode InitNewRow="True" />
            <AutoSize Enabled="true" />
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
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="AMClockItem.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockItemSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
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