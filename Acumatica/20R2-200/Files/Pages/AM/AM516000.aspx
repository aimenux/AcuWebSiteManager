<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM516000.aspx.cs" Inherits="Page_AM516000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="UnapprovedTrans" TypeName="PX.Objects.AM.ClockApprovalProcess">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMClockTran_generateLotSerial" Visible="False"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMClockTran_binLotSerial" Visible="True" />   
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" LinkPage="" DefaultControlID="edProdOrdID">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edEmployee" runat="server" DataField="EmployeeID" AllowEdit="True" CommitChanges="True"/>
            <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" CommitChanges="True"/>
            <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="True" CommitChanges="True"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" 
        AllowSearch="true" DataSourceID="ds" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
		<Levels>
			<px:PXGridLevel  DataMember="UnapprovedTrans" DataKeyNames="EmployeeID, LineNbr">
			    <RowTemplate>
			        <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" />
                    <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
			        <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="True"/>
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" AllowEdit="True"/>
                    <px:PXSelector ID="edShiftID" runat="server" DataField="ShiftID" AllowEdit="True"/>
			        <px:PXSegmentMask ID="eInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
			        <px:PXNumberEdit ID="edQtytoProd" runat="server" DataField="Qty"/>
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate"/>
                    <px:PXDateTimeEdit ID="edStartTime" runat="server" DataField="StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True"/>
                    <px:PXDateTimeEdit ID="edEndTime" runat="server" DataField="EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True"/>
                    <px:PXTimeSpan ID="edLaborTime" TimeMode="True" runat="server" DataField="LaborTime" InputMask="hh:mm" CommitChanges="True" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" Width="30px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="EmployeeID" />
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="130px" />
                    <px:PXGridColumn DataField="OperationID" Width="90px" />
                    <px:PXGridColumn DataField="ShiftID" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="108px" />                  
                    <px:PXGridColumn DataField="TranDate" Width="90px" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="StartTime" Width="90px" TimeMode="true" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="EndTime" Width="90px" TimeMode="true" AutoCallBack="True"/>                    
                    <px:PXGridColumn DataField="LaborTime" Width="60px" AutoCallBack="True" RenderEditorText="True"/>  
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="764px" Caption="Allocations" DesignView="Content" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" Height="500px">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSAMClockTran_lotseropts" DataSourceID="ds"
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
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSAMClockTran_generateLotSerial" CommandSourceID="ds">
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
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="AMClockTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMClockTranSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
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
