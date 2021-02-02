<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS304000.aspx.cs"
Inherits="Page_FS304000" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FS.RouteDocumentMaint"
            PrimaryView="RouteRecords" SuspendUnloading="False" PageLoadBehavior="InsertRecord">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="ActionsMenu" CommitChanges="True"/>
                    <px:PXDSCallbackCommand Name="DeleteRoute" HideText="True"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Delete" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenDriverSelector" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenVehicleSelector" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="UnassignAppointment" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="ReassignAppointment" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenRoutesOnMap" Visible="True"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenDriverCalendar" Visible="True"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="AddAppointment" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenCustomerLocation" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="SelectCurrentRoute" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenRouteSchedule" Visible="False"></px:PXDSCallbackCommand>                 
                    <px:PXDSCallbackCommand Name="Up" Visible="False" DependOnGrid="gridAppointmentsInRoute"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Down" Visible="False" DependOnGrid="gridAppointmentsInRoute"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenAppointment" Visible="False" DependOnGrid="gridAppointmentsInRoute"></px:PXDSCallbackCommand>
					<px:PXDSCallbackCommand Name="ViewStartGPSOnMap" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False"></px:PXDSCallbackCommand>
					<px:PXDSCallbackCommand Name="ViewCompleteGPSOnMap" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False"></px:PXDSCallbackCommand>
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
            <%--ReassignAppointment--%>
                <px:PXSmartPanel ID="PXRouteAppointmentAssigment" runat="server" AutoReload="True" AutoRepaint="True" Caption="Route Appointment Assignment" CaptionVisible="True" Height="700px" HideAfterAction="False" Key="RouteAppAssignmentFilter" LoadOnDemand="True"
                ShowMaximizeButton="True" ShowAfterLoad="True" Width="900px" AutoCallBack-Command="Refresh" AutoCallBack-Target="gridAppointmentsInRoute" CloseAfterAction="True" TabIndex="8500">
                    <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                    <px:PXFormView ID="PXFormViewRoute" runat="server" DataMember="RouteAppAssignmentFilter" DataSourceID="ds" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartRow="True"></px:PXLayoutRule>
                            <px:PXTextEdit ID="edAppRefNbr" runat="server" DataField="AppRefNbr"  Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edAppSrvOrdType" runat="server" DataField="AppSrvOrdType" Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edLocationID" runat="server" DataField="LocationID" Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edScheduledDateTimeBegin" runat="server" DataField="ScheduledDateTimeBegin" Enabled="false"></px:PXTextEdit>
                            <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" Enabled="false"></px:PXTextEdit>
                            <px:PXTextEdit ID="edState" runat="server" DataField="State" Enabled="false"></px:PXTextEdit>
                            <px:PXMaskEdit ID="edEstimatedDurationTotal" runat="server" DataField="EstimatedDurationTotal" Enabled="false"></px:PXMaskEdit>
                            <px:PXLayoutRule runat="server" StartRow="True" GroupCaption="Filter Options"></px:PXLayoutRule>
                            <px:PXDateTimeEdit ID="edRouteDate_Date" runat="server" DataField="RouteDate_Date" CommitChanges="True"></px:PXDateTimeEdit>
                            <px:PXSelector ID="edRouteID" runat="server" CommitChanges="True" DataField="RouteID" DataSourceID="ds" AutoRefresh="True" Width="200px"></px:PXSelector>
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartRow="True"></px:PXLayoutRule>
                    <px:PXGrid ID="PXGridRouteAvalable" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Inquire" SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Height="450px" CaptionVisible="true" Caption="Available Routes">
                        <Levels>
                            <px:PXGridLevel DataMember="RouteAppAssignmentRecords" DataKeyNames="RefNbr">
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="RouteID"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSRoute__RouteShort"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="Date"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="DriverID"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="VehicleID"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="TotalDuration"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="TotalNumAppointments" TextAlign="Right"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="TotalServices" TextAlign="Right"></px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="400" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartRow="True" Merge="True"></px:PXLayoutRule>
                    <px:PXButton ID="PXButtonRouteAppReassignButton" runat="server" AlignLeft="True" DialogResult="OK">
                        <AutoCallBack Target="ds" Command="SelectCurrentRoute" />
                        <PopupCommand Target="ds" Command="Cancel"></PopupCommand>
                    </px:PXButton>
                    <px:PXButton ID="PXButtonRouteAppReassignClose" runat="server" AlignLeft="True" DialogResult="OK" Text="Close">
                        <AutoCallBack Target="ds" Command="Cancel"></AutoCallBack>
                    </px:PXButton>
                </px:PXSmartPanel>
            <%--/ReassignAppointment--%>
            <%--Driver Selector--%>
                <px:PXSmartPanel ID="PXSmartPanel2" runat="server" Caption="Driver Selector" CaptionVisible="True" Key="DriverRouteSelected"
                TabIndex="17900" AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" AutoCallBack-Target="PXFormView3" ShowAfterLoad="True"
                Width="540px">
                    <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                    <px:PXFormView ID="PXFormView3" runat="server" DataMember="DriverRouteSelected" DataSourceID="ds" TabIndex="1600"
                    SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="S" StartColumn="True" LabelsWidth="S"></px:PXLayoutRule>
                            <px:PXSelector ID="edDriverRouteDocumentRefNbr" runat="server" DataField="RefNbr" Enabled="False"></px:PXSelector>
                            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="S" LabelsWidth="S"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" GroupCaption="Route Info" StartColumn="True" StartGroup="True" ControlSize="S"
                            LabelsWidth="S"></px:PXLayoutRule>
                            <px:PXSelector ID="edFSRoute__RouteCD" runat="server" DataField="FSRoute__RouteCD" DataSourceID="ds" Enabled="False"></px:PXSelector>
                            <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" Enabled="False"></px:PXDateTimeEdit>
                            <px:PXLayoutRule runat="server" GroupCaption="Current Selection" StartColumn="True" StartGroup="True"
                            ControlSize="SM" LabelsWidth="XS"></px:PXLayoutRule>
                            <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" DataSourceID="ds" Enabled="False"></px:PXSelector>
                            <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" DataSourceID="ds" Enabled="False"></px:PXSelector>
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="PXFormView4" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="DriverFilter"
                    TabIndex="2900" AllowCollapse="False">
                        <Template>
                            <px:PXLayoutRule runat="server" GroupCaption="Filter Options" StartGroup="True" StartRow="True"></px:PXLayoutRule>
                            <px:PXCheckBox ID="edShowUnassignedDrivers" runat="server" DataField="ShowUnassignedDrivers" CommitChanges="True"
                            Text="Show Available Drivers for this Route only" AlignLeft="True"></px:PXCheckBox>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" Style="z-index: 100" SkinID="Inquire" TabIndex="2500"
                    PageSize="10" SyncPosition="True" NoteIndicator="False" FilesIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AcctCD" DataMember="DriverRecords">
                                <RowTemplate>
                                    <px:PXSelector ID="edAcctCD" runat="server" DataField="AcctCD" AllowEdit="True"></px:PXSelector>
                                    <px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName"></px:PXTextEdit>
                                    <px:PXCheckBox ID="edMem_UnassignedDriver" runat="server" DataField="Mem_UnassignedDriver"></px:PXCheckBox>
                                    <px:PXNumberEdit ID="edPriorityPreference" runat="server" DataField="FSRouteEmployee__PriorityPreference"></px:PXNumberEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AcctCD" TextAlign="Left"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="AcctName" TextAlign="Left"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_UnassignedDriver" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSRouteEmployee__PriorityPreference" TextAlign="Center"></px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartRow="True" Merge="True"></px:PXLayoutRule>
                    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Select Driver" AlignLeft="True"></px:PXButton>
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" AlignLeft="True"></px:PXButton>
                </px:PXSmartPanel>
                <%--/Driver Selector--%>
                <%--Vehicle Selector--%>
                <px:PXSmartPanel ID="VehicleSelector" runat="server" Caption="Vehicle Selector" CaptionVisible="True" Key="VehicleRouteSelected"
                    TabIndex="17900" AutoCallBack-Command="Refresh" AutoCallBack-Target="VehicleRouteForm" ShowAfterLoad="True" Width="540px"
                    AllowResize="False" ShowMaximizeButton="True" CloseAfterAction="true">
                        <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                        <px:PXFormView ID="VehicleRouteForm" runat="server" DataMember="VehicleRouteSelected" DataSourceID="ds"
                        TabIndex="1600" SkinID="Transparent">
                            <Template>
                                <px:PXLayoutRule runat="server" StartRow="True" ControlSize="S" StartColumn="True"></px:PXLayoutRule>
                                <px:PXSelector ID="edVehicleRouteDocumentRefNbr" runat="server" DataField="RefNbr" Enabled="False"></px:PXSelector>
                                <px:PXLayoutRule runat="server" StartRow="True" ControlSize="S"></px:PXLayoutRule>
                                <px:PXLayoutRule runat="server" GroupCaption="Route Info" StartColumn="True" StartGroup="True" 
                                LabelsWidth="S"></px:PXLayoutRule>
                                <px:PXSelector ID="edFSRoute__RouteCD" runat="server" DataField="FSRoute__RouteCD" DataSourceID="ds" Enabled="False"></px:PXSelector>
                                <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" Enabled="False"></px:PXDateTimeEdit>
                                <px:PXLayoutRule runat="server" GroupCaption="Current Selection" StartColumn="True" StartGroup="True"
                                ControlSize="SM" LabelsWidth="XS"></px:PXLayoutRule>
                                <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" DataSourceID="ds" Enabled="False"></px:PXSelector>
                                <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" DataSourceID="ds" Enabled="False"></px:PXSelector>
                            </Template>
                        </px:PXFormView>
                        <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="VehicleFilter"
                        TabIndex="2900" AllowCollapse="False">
                            <Template>
                                <px:PXLayoutRule runat="server" GroupCaption="Filter Options" StartGroup="True" StartRow="True" SuppressLabel="True"></px:PXLayoutRule>
                                <px:PXCheckBox ID="edShowUnassignedVehicles" runat="server" CommitChanges="True" DataField="ShowUnassignedVehicles"
                                Text="Show Available Vehicles for this Route only"></px:PXCheckBox>
                            </Template>
                        </px:PXFormView>
                        <px:PXGrid ID="VehiclesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" SkinID="Inquire" TabIndex="2500" SyncPosition="True"
                        PageSize="10" AutoAdjustColumns="true">
                            <Levels>
                                <px:PXGridLevel DataMember="VehicleRecords" DataKeyNames="RefNbr">
                                    <RowTemplate>
                                        <px:PXSelector ID="edVehicleSelectorRefNbr" runat="server" DataField="RefNbr" Enabled="False" AllowEdit="True"></px:PXSelector>
                                        <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False"></px:PXTextEdit>
                                        <px:PXCheckBox ID="edFSMem_UnassignedVehicle" runat="server" DataField="Mem_UnassignedVehicle"></px:PXCheckBox>
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn DataField="RefNbr"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="Description" TextAlign="Left"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="Mem_UnassignedVehicle" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                            <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        </px:PXGrid>
                        <px:PXLayoutRule runat="server" StartRow="True" Merge="True"></px:PXLayoutRule>
                        <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Select Vehicle" AlignLeft="True" Width="140px"></px:PXButton>
                        <px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Close" AlignLeft="True"></px:PXButton>
                    </px:PXSmartPanel>
                    <%--/Vehicle Selector--%>
                        <%--Service Order Type Filter--%>
                            <px:PXSmartPanel ID="ServiceOrderTypeSelector" runat="server" Caption="Select the Service Order type for the new Appointment" CaptionVisible="True" Key="ServiceOrderTypeSelector"
                                TabIndex="17900" ShowAfterLoad="True" Width="400px" AutoCallBack-Command="Refresh" AutoCallBack-Target="ServiceOrderTypeSelectorForm" LoadOnDemand="true">
                            <px:PXLayoutRule runat="server" StartColumn="True">
                            </px:PXLayoutRule>
                            <px:PXFormView ID="ServiceOrderTypeSelectorForm" runat="server" DataMember="ServiceOrderTypeSelector" DataSourceID="ds" TabIndex="1600" SkinID="Transparent">
                                <Template>
                                    <px:PXLayoutRule runat="server" StartRow="True" ControlSize="SM" LabelsWidth="SM" StartColumn="True"></px:PXLayoutRule>
                                    <px:PXSelector ID="edSrvOrdType" runat="server" AutoRefresh="True" DataField="SrvOrdType" DataSourceID="ds" CommitChanges="True"></px:PXSelector>
                                </Template>
                            </px:PXFormView>
                            <px:PXLayoutRule runat="server" StartRow="True" Merge="True"></px:PXLayoutRule>
                            <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="Proceed" AlignLeft="True" Width="125px"></px:PXButton>
                            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Close"  AlignLeft="True"></px:PXButton>
                        </px:PXSmartPanel>
                        <%--/Service Order Type Filter--%>
                            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="RouteRecords"
                            TabIndex="3700" DefaultControlID="edRouteID" ActivityIndicator="True" NoteIndicator="True" NotifyIndicator="True" FilesIndicator="True">
                                <Template>
                                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize = "M" LabelsWidth="S"></px:PXLayoutRule>
                                    <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" ControlSize = "M" LabelsWidth="S"></px:PXLayoutRule>
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Size="SM" AutoRefresh="True"></px:PXSelector>
                                    <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" Size="SM" AutoRefresh="True"></px:PXSelector>
                                    <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" AllowEdit="True" CommitChanges="True"
                                    Size="SM"></px:PXSelector>
                                    <px:PXNumberEdit ID="edTripNbr" runat="server" DataField="TripNbr">
                                    </px:PXNumberEdit>
                                    <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="True"></px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edTimeBegin_Time" runat="server" DataField="TimeBegin_Time" TimeMode="True" CommitChanges="True"></px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edTimeEnd_Time" runat="server" DataField="TimeEnd_Time" TimeMode="True"></px:PXDateTimeEdit>
                                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Size="SM"></px:PXDropDown>
                                    <px:PXLayoutRule runat="server" Merge="True" ControlSize = "M" LabelsWidth="S"></px:PXLayoutRule>
                                    <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" AutoRefresh="True" CommitChanges="True"
                                    Size="SM"></px:PXSelector>
                                    <px:PXButton ID="btnAssignDriver" runat="server" Text="btnAssignDriver" Width="160px">
                                        <AutoCallBack Command="OpenDriverSelector" Target="ds"></AutoCallBack>
                                    </px:PXButton>
                                    <px:PXLayoutRule runat="server" EndGroup="True" ControlSize = "M" LabelsWidth="S"></px:PXLayoutRule>
                                    <px:PXSelector ID="edAdditionalDriverID" runat="server" DataField="AdditionalDriverID" AutoRefresh="True" Size="SM"></px:PXSelector>
                                    <px:PXLayoutRule runat="server" Merge="True" ControlSize = "M" LabelsWidth="S"></px:PXLayoutRule>
                                    <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" AutoRefresh="True" CommitChanges="True" Size="SM"></px:PXSelector>
                                    <px:PXButton ID="btnAssignVehicle" runat="server" Text="btnAssignVehicle" Width="160px">
                                        <AutoCallBack Command="OpenVehicleSelector" Target="ds"></AutoCallBack>
                                    </px:PXButton>
                                    <px:PXLayoutRule runat="server" EndGroup="True" ControlSize = "M" LabelsWidth="S"></px:PXLayoutRule>
                                    <px:PXSelector ID="edAdditionalVehicleID1" runat="server" DataField="AdditionalVehicleID1" AutoRefresh="True" CommitChanges="True" Size="SM"></px:PXSelector>
                                    <px:PXSelector ID="edAdditionalVehicleID2" runat="server" DataField="AdditionalVehicleID2" AutoRefresh="True" CommitChanges="True" Size="SM"></px:PXSelector>
                                    <px:PXLayoutRule runat="server" GroupCaption="Route Statistics" StartColumn="True" StartGroup="True" LabelsWidth="S" ControlSize="SM"></px:PXLayoutRule>
                                    <px:PXMaskEdit ID="edTotalNumAppointments" runat="server" DataField="TotalNumAppointments"></px:PXMaskEdit>
                                    <px:PXMaskEdit ID="edTotalServices" runat="server" DataField="TotalServices"></px:PXMaskEdit>
                                    <px:PXMaskEdit ID="edTotalDistanceFriendly" runat="server" nullText="unavailable" DataField="TotalDistanceFriendly"></px:PXMaskEdit>
                                    <px:PXMaskEdit ID="edTotalServicesDuration" runat="server" DataField="TotalServicesDuration" ></px:PXMaskEdit>
                                    <px:PXMaskEdit ID="edTotalDuration" runat="server" DataField="TotalDuration" nullText="unavailable" ></px:PXMaskEdit>
                                    <px:PXMaskEdit ID="edTotalTravelTime" runat="server" nullText="unavailable" DataField="TotalTravelTime"></px:PXMaskEdit> 
                                    <px:PXTextEdit ID="edApproximateValuesLabel" runat="server" DataField="ApproximateValuesLabel" SkinID="Label" Enabled="false" SuppressLabel="true" Size="XM" />
                                    <px:PXLayoutRule runat="server" EndGroup="True" StartColumn="True">
                                    </px:PXLayoutRule>
                                    <px:PXLayoutRule runat="server" GroupCaption="Actual Time" ControlSize="SM" LabelsWidth="S" StartGroup="True">
                                    </px:PXLayoutRule>
                                    <px:PXDateTimeEdit ID="edActualStartTime" runat="server" CommitChanges="True" DataField="ActualStartTime_Time" TimeMode="True">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edActualEndTime" runat="server" CommitChanges="True" DataField="ActualEndTime_Time" TimeMode="True">
                                    </px:PXDateTimeEdit>
                                    <px:PXMaskEdit ID="edActualDuration" runat="server" DataField="Mem_ActualDuration" Enabled="False">
                                    </px:PXMaskEdit>
                                </Template>
                            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" 
                DataMember="RouteSelected">
                <Items>
                    <px:PXTabItem Text="Appointments">
                        <Template>
                            <px:PXGrid ID="gridAppointmentsInRoute" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" TabIndex="3000" SyncPosition="True" KeepPosition="True">
                                <Levels>
                                    <px:PXGridLevel DataMember="AppointmentsInRoute" DataKeyNames="SrvOrdType, RefNbr">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                                            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType"></px:PXSelector>
                                            <px:PXSelector ID="edServiceContractID" runat="server" AllowEdit="True" DataField="ServiceContractID"></px:PXSelector>
                                            <px:PXTextEdit ID="edCustomerContractNbr" runat="server" DataField="CustomerContractNbr"></px:PXTextEdit>
                                            <px:PXSelector ID="edScheduleID" runat="server" AllowEdit="True" DataField="ScheduleID"></px:PXSelector>
                                            <px:PXSelector ID="edAppointmentRefNbr" runat="server" DataField="RefNbr"></px:PXSelector>
                                            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc"></px:PXTextEdit>
                                            <px:PXSegmentMask ID="edAppCustomerID" runat="server" DataField="CustomerID" AllowEdit="True"></px:PXSegmentMask>
                                            <px:PXSegmentMask ID="edAppLocationID" runat="server" DataField="LocationID" AllowEdit="True"></px:PXSegmentMask>
                                            <px:PXDropDown ID="edStatus" runat="server" DataField="Status"></px:PXDropDown>
                                            <px:PXDateTimeEdit ID="edScheduledDateTimeBegin_Date" runat="server" DataField="ScheduledDateTimeBegin_Date"></px:PXDateTimeEdit>
                                            <px:PXDateTimeEdit ID="edScheduledDateTimeBegin_Time" runat="server" DataField="ScheduledDateTimeBegin_Time"></px:PXDateTimeEdit>
                                            <px:PXDateTimeEdit ID="edScheduledDateTimeEnd_Time" runat="server" DataField="ScheduledDateTimeEnd_Time"></px:PXDateTimeEdit>
                                            <px:PXMaskEdit ID="edEstimatedDurationTotal" runat="server" DataField="EstimatedDurationTotal"></px:PXMaskEdit>
                                            <px:PXTextEdit ID="edAppAddressLine1" runat="server" DataField="AddressLine1"></px:PXTextEdit>
                                            <px:PXTextEdit ID="edAppAppAddressLine2" runat="server" DataField="AddressLine2"></px:PXTextEdit>
                                            <px:PXTextEdit ID="edAppPostalCode" runat="server" DataField="PostalCode"></px:PXTextEdit>
                                            <px:PXTextEdit ID="edAppCity" runat="server" DataField="City"></px:PXTextEdit>
                                            <px:PXSelector ID="edAppState" runat="server" DataField="State"></px:PXSelector>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="SrvOrdType"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="ServiceContractID"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="CustomerContractNbr"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="ScheduleID"  LinkCommand="OpenRouteSchedule"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenAppointment"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="DocDesc"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="CustomerID" DisplayMode="Hint"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="LocationID" LinkCommand="OpenCustomerLocation"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="Status"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="ScheduledDateTimeBegin_Date"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time" TimeMode="True"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="ScheduledDateTimeEnd_Time" TimeMode="True"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="EstimatedDurationTotal"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="AddressLine1"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="AddressLine2"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="City"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="State"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="PostalCode"></px:PXGridColumn>
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True"/>
                                <ActionBar ActionsText="False" PagerVisible="False">
                                    <CustomItems>
                                        <px:PXToolBarButton Tooltip="Create New Appointment" ImageSet="main" ImageKey="AddNew">
                                            <AutoCallBack Target="ds" Command="AddAppointment"></AutoCallBack>
                                            <PopupCommand Target="ds" Command="Cancel"></PopupCommand>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Tooltip="Delete Appointment" ImageSet="main" ImageKey="RecordDel">
                                            <AutoCallBack Target="ds" Command="UnassignAppointment"></AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Tooltip="Move Up" ImageSet="main" ImageKey="ArrowUp">
                                            <AutoCallBack Target="ds" Command="Up"></AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Tooltip="Move Down" ImageSet="main" ImageKey="ArrowDown">
                                            <AutoCallBack Target="ds" Command="Down"></AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarSeperator></px:PXToolBarSeperator>
                                        <px:PXToolBarSeperator></px:PXToolBarSeperator>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Target="ds" Command="ReassignAppointment"></AutoCallBack>
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Additional Info">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" LabelsWidth="S" ControlSize="M">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edMiles" runat="server" DataField="Miles" Size="SM">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edWeight" runat="server" DataField="Weight" Size="SM">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edFuelQty" runat="server" DataField="FuelQty" Size="SM">
                            </px:PXNumberEdit>
                            <px:PXDropDown ID="edFuelType" runat="server" DataField="FuelType" CommitChanges="True" Size="SM">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edOil" runat="server" DataField="Oil">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edAntiFreeze" runat="server" DataField="AntiFreeze">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edDEF" runat="server" DataField="DEF">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edPropane" runat="server" DataField="Propane">
                            </px:PXNumberEdit>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Attributes">
                        <Template>
                            <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                                Height="200px" MatrixMode="True">
                                <Levels>
                                    <px:PXGridLevel DataMember="Answers">
                                        <Columns>
                                            <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False"
                                                TextField="AttributeID_description" />
                                            <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False"/>
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="200" />
                                <ActionBar>
                                    <Actions>
                                        <Search Enabled="False" />
                                    </Actions>
                                </ActionBar>
                                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Location">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
                            </px:PXLayoutRule>
                            <px:PXFormView runat="server" Caption="Location"
                                DataMember="RouteSelected" RenderStyle="Simple" DataSourceID="ds"
                                ID="LocationForm">
                                <Template>
                                    <px:PXLayoutRule StartGroup="True" GroupCaption="Start Location" runat="server" LabelsWidth="S" ControlSize="M">
                                    </px:PXLayoutRule>
                                    <px:PXNumberEdit ID="edGPSLatitudeStart" runat="server" DataField="GPSLatitudeStart" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edGPSLongitudeStart" runat="server" DataField="GPSLongitudeStart" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXButton ID="btnViewStartGPSOnMap" runat="server" Height="21px"
                                        Text="View on Map">
                                        <AutoCallBack Command="ViewStartGPSOnMap" Target="ds">
                                        </AutoCallBack>
                                    </px:PXButton>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
                                    </px:PXLayoutRule>
                                    <px:PXLayoutRule StartGroup="True" GroupCaption="Complete Location" runat="server" LabelsWidth="S" ControlSize="M">
                                    </px:PXLayoutRule>
                                    <px:PXNumberEdit ID="edGPSLatitudeComplete" runat="server" DataField="GPSLatitudeComplete" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edGPSLongitudeComplete" runat="server" DataField="GPSLongitudeComplete" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXButton ID="btnViewCompleteGPSOnMap" runat="server" Height="21px"
                                        Text="View on Map">
                                        <AutoCallBack Command="ViewCompleteGPSOnMap" Target="ds">
                                        </AutoCallBack>
                                    </px:PXButton>
                                    <px:PXTextEdit ID="edGPSLatitudeLongitude" runat="server"
                                        DataField="GPSLatitudeLongitude" AlignLeft="True" Enabled = "False">
                                    </px:PXTextEdit>
                                </Template>
                            </px:PXFormView>
                        </Template>
                    </px:PXTabItem>
                </Items>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
            </px:PXTab>
        </asp:Content>
