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
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="AppointmentSelected">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXGrid ID="PXGridDetails" runat="server" DataSourceID="ds" SkinID="DetailsInTab" TabIndex="1700" Height="100%" Width="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="AppointmentDetails" DataKeyNames="AppointmentID,AppDetID,SODetID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info" StartGroup="True" StartRow="True" />
                                    <px:PXSelector ID="edUSODetID" runat="server" DataField="SODetID" CommitChanges="True" NullText="<NEW>" AutoRefresh="True" />
                                    <%-- px:PXSegmentMask ID="edUPickupDeliveryServiceID" runat="server" DataField="PickupDeliveryServiceID" CommitChanges="True" AutoRefresh="True" AllowEdit="True" / --%>
                                    <px:PXDropDown ID="edUStatus" runat="server" DataField="Status" />
                                    <px:PXDropDown ID="edUServiceType" runat="server" DataField="ServiceType" Size="SM" />
                                    <px:PXDropDown ID="edULineType" runat="server" DataField="LineType" CommitChanges="True" />
                                    <px:PXSegmentMask ID="edUInventoryID" runat="server" DataField="InventoryID" AllowEdit ="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edUSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.InventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edUTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXSegmentMask ID="edUSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                                    <px:PXSelector ID="edUSMEquipmentID" runat="server" DataField="SMEquipmentID" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXCheckBox ID="edUIsBillable" runat="server" DataField="IsBillable" />
                                    <px:PXSelector ID="edUUOM" runat="server" AutoRefresh="True" DataField="UOM" />
                                    <px:PXNumberEdit ID="edUQty" runat="server" DataField="Qty" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUUnitPrice" runat="server" DataField="UnitPrice" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edUEstimatedDuration" runat="server" DataField="EstimatedDuration" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edUActualDateTimeBegin_Time" runat="server" DataField="ActualDateTimeBegin_Time" TimeMode="True" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edUActualDateTimeEnd_Time" runat="server" DataField="ActualDateTimeEnd_Time" TimeMode="True" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edUActualDuration" runat="server" DataField="ActualDuration" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUTranAmt" runat="server" DataField="TranAmt" />
                                    <px:PXSelector ID="edUProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True" DisplayMode="Value" AllowEdit = "True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SODetID" CommitChanges="True" NullText="<NEW>" />
                                    <%-- px:PXGridColumn DataField="PickupDeliveryServiceID" / --%>
                                    <px:PXGridColumn DataField="ServiceType" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="LineType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="SiteID" />
                                    <px:PXGridColumn DataField="SMEquipmentID" />
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="EstimatedDuration" />
                                    <px:PXGridColumn DataField="ActualDateTimeBegin_Time" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ActualDateTimeEnd_Time" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ActualDuration" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UnitPrice" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowFormEdit="True" InitNewRow="True"/>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
