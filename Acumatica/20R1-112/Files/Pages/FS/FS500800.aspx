<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS500800.aspx.cs" Inherits="Page_FS500800" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FS.CloseRouteProcess"
    SuspendUnloading="False" PageLoadBehavior="InsertRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="gridRouteDocs" Name="OpenRoute" Visible="false" ></px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="3700">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" ControlSize="SM" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit ID="edDate" runat="server" CommitChanges="True"
                DataField="Date">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edShowCompletedRoutes" runat="server" DataField="ShowClosedRoutes" CommitChanges="True">
            </px:PXCheckBox>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="gridRouteDocs" runat="server" AllowPaging="True" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="500" SyncPosition="True" BatchUpdate="True" KeepPosition="True">
                <Levels>
                    <px:PXGridLevel DataMember="RouteDocs" DataKeyNames="RefNbr">
			            <RowTemplate>
                            <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" Text="Selected">
                            </px:PXCheckBox>
                            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True" Enabled="False">
                            </px:PXSelector>
                            <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" AllowEdit="True" 
                                 CommitChanges="true" AutoRefresh="true">
                            </px:PXSelector>
                            <px:PXSelector ID="edAdditionalDriverID" runat="server" DataField="AdditionalDriverID" 
                                AllowEdit="True" CommitChanges="true" AutoRefresh="true">
                            </px:PXSelector>
                            <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" 
                                AllowEdit="True" CommitChanges="true" AutoRefresh="true">
                            </px:PXSelector>
                            <px:PXSelector ID="edAdditionalVehicleID1" runat="server" DataField="AdditionalVehicleID1" 
                                AllowEdit="True" CommitChanges="true" AutoRefresh="true">
                            </px:PXSelector>
                            <px:PXSelector ID="edAdditionalVehicleID2" runat="server" DataField="AdditionalVehicleID2" 
                                AllowEdit="True" CommitChanges="true" AutoRefresh="true">
                            </px:PXSelector>
                            <px:PXSelector ID="edRouteID" runat="server" 
                                DataField="RouteID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edRouteShort" runat="server" 
                                DataField="FSRoute__RouteShort" AllowEdit="True">
                            </px:PXTextEdit>
                            <px:PXDropDown ID="edStatus" runat="server" DataField="Status">
                            </px:PXDropDown>
                            <px:PXDateTimeEdit ID="edTimeBegin_Time" runat="server" 
                                DataField="TimeBegin_Time">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edTimeEnd_Time" runat="server" DataField="TimeEnd_Time">
                            </px:PXDateTimeEdit>
                            <px:PXNumberEdit ID="edTotalNumAppointments" runat="server" 
                                DataField="TotalNumAppointments">
                            </px:PXNumberEdit>
                            <px:PXMaskEdit ID="edTotalDuration" runat="server" DataField="TotalDuration">
                            </px:PXMaskEdit>
                            <px:PXMaskEdit ID="edTotalTravelTime" runat="server" 
                                DataField="TotalTravelTime">
                            </px:PXMaskEdit>
                            <px:PXDateTimeEdit ID="edTimeBegin_Date" runat="server" 
                                DataField="TimeBegin_Date">
                            </px:PXDateTimeEdit>
                            <px:PXNumberEdit ID="edTotalServices" runat="server" DataField="TotalServices">
                            </px:PXNumberEdit>
                            <px:PXMaskEdit ID="edTotalServicesDuration" runat="server" 
                                DataField="TotalServicesDuration">
                            </px:PXMaskEdit>
                        </RowTemplate>
			            <Columns>
                            <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="RefNbr" CommitChanges="true" LinkCommand="OpenRoute">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="RouteID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSRoute__RouteShort">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Status">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Date">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TimeBegin_Time" TimeMode="True">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TimeEnd_Time" TimeMode="True">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="DriverID" DisplayMode="Hint">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AdditionalDriverID" DisplayMode="Hint">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="VehicleID" CommitChanges="true">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AdditionalVehicleID1" CommitChanges="true">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AdditionalVehicleID2" CommitChanges="true">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TotalNumAppointments" TextAlign="Right">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TotalServices" TextAlign="Right">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TotalDuration">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TotalServicesDuration">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TotalTravelTime">
                            </px:PXGridColumn>
                        </Columns>
			        </px:PXGridLevel>
                </Levels>
                <Mode AllowAddNew="False" AllowDelete="False" />
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
            </px:PXGrid>
</asp:Content>