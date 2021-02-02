<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS300210.aspx.cs" Inherits="Page_FS300210" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        BorderStyle="NotSet" PrimaryView="ClosingAppointmentRecords" 
        SuspendUnloading="False" 
        TypeName="PX.Objects.FS.AppointmentClosingMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Delete" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Insert" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="PostToInventory" CommitChanges="True"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="AppClosingMenuActions" CommitChanges="True"></px:PXDSCallbackCommand>
            
            <%-- These actions are inherited from AppointmentEntry. For this screen they must be not visible--%>
            <px:PXDSCallbackCommand Name="ViewDirectionOnMap" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ValidateAddress" Visible="False"/>
            <px:PXDSCallbackCommand Name="CloneAppointment" Visible="False"/>
            <px:PXDSCallbackCommand Name="OpenEmployeeSelector" Visible="False"/>
            <px:PXDSCallbackCommand Name="OpenServiceSelector" Visible="False"/>
            <px:PXDSCallbackCommand Name="CreateNewCustomer" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="SelectCurrentService" Visible="False"></px:PXDSCallbackCommand> 
            <px:PXDSCallbackCommand Name="SelectCurrentEmployee" Visible="False"></px:PXDSCallbackCommand>  
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Height="190px" DataMember="ClosingAppointmentRecords" TabIndex="7900" DefaultControlID="edActualDateTimeBegin_Time">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" ControlSize="SM">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr">
                <LinkCommand Command="OpenAppointment" Enabled="True" Target="ds" />
            </px:PXTextEdit>
            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" AllowEdit="True">
            </px:PXSelector>
            <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" AllowEdit="True">
            </px:PXSelector>
            <px:PXFormView ID="ServiceOrderHeader" runat="server" 
                Caption="ServiceOrder Header" DataMember="ServiceOrderRelated" 
                DataSourceID="ds" RenderStyle="Simple" TabIndex="1500">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM">
                    </px:PXLayoutRule>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" AllowEdit="True" CommitChanges="True" DataField="CustomerID" DataSourceID="ds">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" Enabled="False">
                    </px:PXSegmentMask>
                </Template>
            </px:PXFormView>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False">
            </px:PXDropDown>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" GroupCaption="Actual Date and Time" LabelsWidth="120px">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit ID="edExecutionDate" runat="server" CommitChanges="True" DataField="ExecutionDate" Enabled="False">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time" runat="server" CommitChanges="True" DataField="ActualDateTimeBegin_Time" TimeMode="True">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time" runat="server" CommitChanges="True" DataField="ActualDateTimeEnd_Time" TimeMode="True">
            </px:PXDateTimeEdit>
            <px:PXMaskEdit ID="edActualDurationTotal" runat="server" DataField="ActualDurationTotal" Size="S" Enabled="False">
            </px:PXMaskEdit>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" 
        DataMember="AppointmentSelected">
		<Items>
            <px:PXTabItem Text="Services">
                <Template>
                      <px:PXGrid ID="PXGridServices" runat="server" DataSourceID="ds" SkinID="DetailsInTab"
                        TabIndex="1700" Height="100%" Width="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel 
                                DataMember="AppointmentDetServices" DataKeyNames="AppointmentID,AppDetID,SODetID">
                                <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True">
                                </px:PXLayoutRule>                                                
                                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info" 
                                        StartGroup="True" StartRow="True">
                                    </px:PXLayoutRule>
                                    <px:PXSelector ID="edSODetID" runat="server" DataField="SODetID" 
                                        CommitChanges="True" NullText="<NEW>" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXDropDown ID="Status" runat="server" DataField="Status">
                                    </px:PXDropDown>                                    
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID"
                                            AllowEdit ="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSAppointmentDetService.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="edSMEquipmentID" runat="server" DataField="SMEquipmentID" AutoRefresh="True" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edIsBillable" runat="server" DataField="IsBillable">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="Qty" runat="server" DataField="Qty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edUnitPrice" runat="server" DataField="UnitPrice" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXMaskEdit ID="edEstimatedDuration" runat="server" 
                                        DataField="EstimatedDuration" CommitChanges="True">
                                    </px:PXMaskEdit>
                                    <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time2" runat="server" 
                                        DataField="ActualDateTimeBegin_Time" TimeMode="True" CommitChanges="True" />                                    
                                    <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time2" runat="server" 
                                        DataField="ActualDateTimeEnd_Time" TimeMode="True" CommitChanges="True" />                                    
                                    <px:PXMaskEdit ID="edActualDuration" runat="server" DataField="ActualDuration" CommitChanges="True">
                                    </px:PXMaskEdit>
                                    <px:PXNumberEdit ID="TranAmt" runat="server" DataField="TranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXSelector ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True" DisplayMode="Value" AllowEdit = "True">
                                    </px:PXSelector>                                    
                                    <px:PXCheckBox ID="edApprovedTime" runat="server" DataField="ApprovedTime" 
                                        Text="Approved Time">
                                    </px:PXCheckBox>
                                    <px:PXSelector ID="edTimeCardCD" runat="server" DataField="TimeCardCD">
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SODetID" CommitChanges="True" NullText="<NEW>" 
                                        Width="85px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="LineType" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SMEquipmentID" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" 
                                        Width="65px">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="EstimatedDuration" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDateTimeBegin_Time" CommitChanges="True" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDateTimeEnd_Time" CommitChanges="True" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDuration" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="60px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UnitPrice" Width="100px" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="ApprovedTime" Width="80px" TextAlign="Center" 
                                        Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TimeCardCD" Width="80px"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowFormEdit="True" InitNewRow="True"/>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Pickup/Delivery Items">
			    <Template>
                    <px:PXGrid ID="PXGridPickupDelivery" runat="server" TabIndex="4900" DataSourceID="ds" 
                        SkinID="DetailsInTab" Height="100%" Width="100%" SyncPosition="True" >
                        <Levels>
                            <px:PXGridLevel 
                                DataMember="PickupDeliveryItems" DataKeyNames="AppointmentID, SODetID, InventoryID">
                                <RowTemplate>
                                    <px:PXSelector ID="edSODetID2" runat="server" CommitChanges="True" DataField="SODetID" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edPickupDeliveryServiceID" runat="server" CommitChanges="True" AutoRefresh="True" DataField="PickupDeliveryServiceID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXDropDown ID="edServiceType" runat="server" DataField="ServiceType" Size="SM">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edPickupDeliveryInventoryID" runat="server" DataField="InventoryID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" Size="SM">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>">
                                            <Parameters>
                                                <px:PXControlParam ControlID="PXGridPickupDelivery" Name="FSAppointmentInventoryItem.InventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edTranDesc2" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXSegmentMask ID="edSiteID2" runat="server" DataField="SiteID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edUOM2" runat="server" AutoRefresh="True" DataField="UOM">
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edUnitPrice2" runat="server" DataField="UnitPrice" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edTranAmt2" runat="server" DataField="TranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXSelector ID="edProjectTaskID2" runat="server" DataField="ProjectTaskID" AllowEdit="True">
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn CommitChanges="True" DataField="SODetID" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PickupDeliveryServiceID" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ServiceType" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn CommitChanges="True" DataField="InventoryID" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" Width="45px" NullText="<SPLIT>" AutoCallBack="true" >
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteID" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UOM" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" Width="100px" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UnitPrice" TextAlign="Right" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right" Width="150px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="150px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
