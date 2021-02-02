<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS304010.aspx.cs"
Inherits="Page_FS304010" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FS.RouteClosingMaint"
            PrimaryView="RouteRecords" SuspendUnloading="False" PageLoadBehavior="GoFirstRecord">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="ActionsMenu" CommitChanges="True"/>
                    <px:PXDSCallbackCommand Name="Delete" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Insert" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="CopyPaste" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenAppointment" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenCustomerLocation" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenRouteSchedule" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenRouteContract" Visible="False"></px:PXDSCallbackCommand> 
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="RouteRecords" NoteIndicator="False" FilesIndicator="False" TabIndex="3700" DefaultControlID="edRefNbr" AllowCollapse="True">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True"></px:PXLayoutRule>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Size="SM" AutoRefresh="True"></px:PXSelector>
                    <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" Size="SM" AutoRefresh="True" Enabled="False"></px:PXSelector>
                    <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="True" Enabled="False"></px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edTimeBegin_Time" runat="server" DataField="TimeBegin_Time" TimeMode="True" CommitChanges="True" Enabled="False"></px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edTimeEnd_Time" runat="server" DataField="TimeEnd_Time" TimeMode="True" Enabled="False"></px:PXDateTimeEdit>
                    <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                    <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" AllowEdit="True" CommitChanges="True" Size="SM"></px:PXSelector>
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Size="SM"></px:PXDropDown>
                    <px:PXMaskEdit ID="edTripNbr" runat="server" CommitChanges="True" DataField="TripNbr" Enabled="False">
                    </px:PXMaskEdit>
                    <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" AutoRefresh="True" CommitChanges="True" Size="SM" Enabled="False"></px:PXSelector>
                    <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" AutoRefresh="True" CommitChanges="True" Size="SM" Enabled="False"></px:PXSelector>
                    <px:PXLayoutRule runat="server" StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" GroupCaption="Actual Time" StartGroup="True">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edActualStartTime" runat="server" DataField="ActualStartTime_Time" TimeMode="True" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edActualEndTime" runat="server" DataField="ActualEndTime_Time" TimeMode="True" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXMaskEdit ID="edActualDuration" runat="server" DataField="Mem_ActualDuration" Enabled="False">
                    </px:PXMaskEdit>
                </Template>
            </px:PXFormView>
        </asp:Content>

        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" 
                DataMember="RouteDocumentSelected" Style="z-index: 100">
                <Items>
                <px:PXTabItem Text="Appointments">
                    <Template>
                        <px:PXGrid ID="gridAppointmentsInRoute" runat="server" DataSourceID="ds" Height="530px" SkinID="Inquire" TabIndex="4200" Width="100%" SyncPosition="True">
                            <Levels>
                                <px:PXGridLevel DataMember="AppointmentsInRoute" DataKeyNames="SrvOrdType,RefNbr">
                                    <RowTemplate>
                                        <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                                        <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType"></px:PXSelector>
                                        <px:PXSelector ID="edServiceContractID" runat="server" AllowEdit="True" DataField="ServiceContractID" ></px:PXSelector>
                                        <px:PXSelector ID="edFSServiceContract__CustomerContractNbr" runat="server" DataField="FSServiceContract__CustomerContractNbr" ></px:PXSelector>
                                        <px:PXSelector ID="edScheduleID" runat="server" AllowEdit="True" DataField="ScheduleID" ></px:PXSelector>
                                        <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr"></px:PXSelector>
                                        <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc"></px:PXTextEdit>
                                        <px:PXSegmentMask ID="edFSServiceOrder__CustomerID" runat="server" DataField="FSServiceOrder__CustomerID" AllowEdit="True" ></px:PXSegmentMask>
                                        <px:PXSegmentMask ID="edFSServiceOrder__LocationID" runat="server" DataField="FSServiceOrder__LocationID" AllowEdit="True" ></px:PXSegmentMask>
                                        <px:PXMaskEdit ID="edEstimatedDurationTotal" runat="server" DataField="EstimatedDurationTotal"></px:PXMaskEdit>
                                        <px:PXDateTimeEdit ID="edScheduledDateTimeBegin_Time" runat="server" DataField="ScheduledDateTimeBegin_Time"></px:PXDateTimeEdit>
                                        <px:PXDateTimeEdit ID="edScheduledDateTimeEnd_Time" runat="server" DataField="ScheduledDateTimeEnd_Time"></px:PXDateTimeEdit>
                                        <px:PXTextEdit ID="edFSServiceOrder__AddressLine1" runat="server" DataField="FSServiceOrder__AddressLine1"></px:PXTextEdit>
                                        <px:PXTextEdit ID="edFSServiceOrder__AddressLine2" runat="server" DataField="FSServiceOrder__AddressLine2"></px:PXTextEdit>
                                        <px:PXTextEdit ID="edFSServiceOrder__PostalCode" runat="server" DataField="FSServiceOrder__PostalCode"></px:PXTextEdit>
                                        <px:PXDropDown ID="edStatus" runat="server" DataField="Status"></px:PXDropDown>
                                        <px:PXTextEdit ID="edFSServiceOrder__City" runat="server" DataField="FSServiceOrder__City"></px:PXTextEdit>
                                        <px:PXSelector ID="edFSServiceOrder__State" runat="server" DataField="FSServiceOrder__State"></px:PXSelector>
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn DataField="SrvOrdType"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="ServiceContractID" LinkCommand="OpenRouteContract"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceContract__CustomerContractNbr"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="ScheduleID"  LinkCommand="OpenRouteSchedule"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenAppointment"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="DocDesc"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__CustomerID" DisplayMode="Hint"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__LocationID" LinkCommand="OpenCustomerLocation"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="Status"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="ScheduledDateTimeBegin_Date"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time" TimeMode="True"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="ScheduledDateTimeEnd_Time" TimeMode="True"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="EstimatedDurationTotal"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__AddressLine1"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__AddressLine2"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__City"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__State"></px:PXGridColumn>
                                        <px:PXGridColumn DataField="FSServiceOrder__PostalCode"></px:PXGridColumn>
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                            <AutoSize Enabled="True" MinHeight="150" />
                        </px:PXGrid>
                    </Template>
                </px:PXTabItem>
                <px:PXTabItem Text="Additional Info">
                    <Template>
                        <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True">
                        </px:PXLayoutRule>
                        <px:PXNumberEdit ID="edMiles" runat="server" DataField="Miles" Size="SM">
                        </px:PXNumberEdit>
                        <px:PXNumberEdit ID="edWeight" runat="server" DataField="Weight" Size="SM">
                        </px:PXNumberEdit>
                        <px:PXNumberEdit ID="edFuelQty" runat="server" DataField="FuelQty" Size="SM">
                        </px:PXNumberEdit>
                        <px:PXDropDown ID="edFuelType" runat="server" DataField="FuelType" Size="SM">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" StartColumn="True">
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
		        </Items>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	        </px:PXTab>
        </asp:Content>
