<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS403900.aspx.cs" Inherits="Page_FS403900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.RouteWrkSheetInq" 
        PrimaryView="Filter" SuspendUnloading="False">
        <CallbackCommands>              
            <px:PXDSCallbackCommand Name="OpenDriverSelector" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenVehicleSelector" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenRouteDocument" Visible="False">
            </px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
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
        <px:PXGrid ID="VehiclesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Height="150px" SkinID="Inquire" TabIndex="2500" SyncPosition="True"
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
        <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Select Vehicle" AlignLeft="True" Width="140px"></px:PXButton>
        <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" AlignLeft="True"></px:PXButton>
    </px:PXSmartPanel>
    <%--/Vehicle Selector--%>
    <%--Driver Selector--%>
    <px:PXSmartPanel ID="PXSmartPanel4" runat="server" Caption="Driver Selector" CaptionVisible="True" Key="DriverRouteSelected"
    TabIndex="17900" AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" AutoCallBack-Target="grid" ShowAfterLoad="False"
    LoadOnDemand="True" AutoReload="True" Width="540px" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
        <px:PXFormView ID="PXFormView7" runat="server" DataMember="DriverRouteSelected" DataSourceID="ds" TabIndex="1600"
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
        <px:PXFormView ID="PXFormView8" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="DriverFilter"
        TabIndex="2900" AllowCollapse="False">
            <Template>
                <px:PXLayoutRule runat="server" GroupCaption="Filter Options" StartGroup="True" StartRow="True"></px:PXLayoutRule>
                <px:PXCheckBox ID="edShowUnassignedDrivers" runat="server" DataField="ShowUnassignedDrivers" CommitChanges="True"
                Text="Show Available Drivers for this Route only" AlignLeft="True"></px:PXCheckBox>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="PXGrid3" runat="server" DataSourceID="ds" Style="z-index: 100" Height="150px" SkinID="Inquire" TabIndex="2500"
        PageSize="10" SyncPosition="True" NoteIndicator="False" FilesIndicator="False">
            <Levels>
                <px:PXGridLevel DataKeyNames="AcctCD" DataMember="DriverRecords">
                    <RowTemplate>
                        <px:PXSelector ID="PXSelector7" runat="server" DataField="AcctCD" AllowEdit="True"></px:PXSelector>
                        <px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="AcctName"></px:PXTextEdit>
                        <px:PXCheckBox ID="PXCheckBox3" runat="server" DataField="Mem_UnassignedDriver"></px:PXCheckBox>
                        <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="FSRouteEmployee__PriorityPreference"></px:PXNumberEdit>
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
        <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Select Driver" AlignLeft="True"></px:PXButton>
        <px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Close" AlignLeft="True"></px:PXButton>
    </px:PXSmartPanel>
    <%--/Driver Selector--%>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Selection"
		Width="100%" DataMember="Filter" TabIndex="3700" DefaultControlID="edFromDate" AllowCollapse="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" ControlSize="SM" LabelsWidth="XS">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit runat="server" DataField="FromDate" ID="edFromDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit runat="server" DataField="ToDate" ID="edToDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" Merge="True" ></px:PXLayoutRule>
            <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" 
                CommitChanges="True" AutoRefresh="True">
            </px:PXSelector>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Inquire" TabIndex="3000" SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto"  KeepPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Routes" DataKeyNames="RefNbr">
			    <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" 
                        AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAdditionalDriverID" runat="server" DataField="AdditionalDriverID" 
                        AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" 
                        AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAdditionalVehicleID1" runat="server" DataField="AdditionalVehicleID1" 
                        AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAdditionalVehicleID2" runat="server" DataField="AdditionalVehicleID2" 
                        AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edRouteID" runat="server" 
                        DataField="RouteID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edRouteShort" runat="server" DataField="FSRoute__RouteShort">
                    </px:PXTextEdit>
                    <px:PXNumberEdit ID="edTribNbr" runat="server" DataField="TripNbr">
                    </px:PXNumberEdit>
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
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenRouteDocument">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RouteID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSRoute__RouteShort">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TripNbr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Status">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Date">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TimeBegin_Time" TimeMode="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TimeEnd_Time" TimeMode="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DriverID" CommitChanges="true" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalDriverID" CommitChanges="true" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="VehicleID" CommitChanges="true">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalVehicleID1" CommitChanges="true">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalVehicleID2" CommitChanges="True">
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
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar ActionsText="False">
            <CustomItems>
                <px:PXToolBarButton>
                    <AutoCallBack Target="ds" Command="OpenDriverSelector" ></AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarButton>
                    <AutoCallBack Target="ds" Command="OpenVehicleSelector" ></AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
