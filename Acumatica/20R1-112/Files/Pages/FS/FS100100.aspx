<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS100100.aspx.cs" Inherits="Page_FS100100" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="SetupRecord" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.SetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" ></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="348px" DataSourceID="ds" DataMember="SetupRecord"
        DefaultControlID="edPostBatchNumberingID">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
        <px:PXTabItem Text="General Settings">
		    <Template>
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="XM" ControlSize="M">
                </px:PXLayoutRule>                                                                 
                <px:PXLayoutRule runat="server" GroupCaption="Numbering Settings" StartGroup="True">
                </px:PXLayoutRule>                                    
                <px:PXSelector ID="edPostBatchNumberingID" runat="server" AllowEdit = "True"
                    DataField="PostBatchNumberingID">
                </px:PXSelector>
                <px:PXSelector ID="edEmpSchdlNumberingID" runat="server" AllowEdit = "True"
                    DataField="EmpSchdlNumberingID">
                </px:PXSelector>
                <px:PXSelector ID="edEquipmentNumberingID" runat="server" AllowEdit = "True"
                    DataField="EquipmentNumberingID">
                </px:PXSelector>
                <px:PXSelector ID="edLicenseNumberingID" runat="server" AllowEdit = "True"
                    DataField="LicenseNumberingID">
                </px:PXSelector>

                <px:PXLayoutRule runat="server" GroupCaption="General Settings" StartGroup="True">
                </px:PXLayoutRule>
                <px:PXCheckBox ID="edEnableEmpTimeCardIntegration" runat="server" AlignLeft="True" CommitChanges="True" DataField="EnableEmpTimeCardIntegration">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edManageRooms" runat="server" DataField="ManageRooms" AlignLeft="True" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edEnableDfltStaffOnServiceOrder" runat="server" DataField="EnableDfltStaffOnServiceOrder" AlignLeft="True" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edEnableDfltResEquipOnServiceOrder" runat="server" DataField="EnableDfltResEquipOnServiceOrder" AlignLeft="True" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edCustomerMultipleBillingOptions" runat="server" DataField="CustomerMultipleBillingOptions" AlignLeft="True" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edAlertBeforeCloseServiceOrder" runat="server" DataField="AlertBeforeCloseServiceOrder" AlignLeft="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edFilterInvoicingManually" runat="server" DataField="FilterInvoicingManually" AlignLeft="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edTrackAppointmentLocation" runat="server" AlignLeft="True" DataField="TrackAppointmentLocation">
                </px:PXCheckBox>
                <px:PXLayoutRule runat="server" GroupCaption="Default Settings" StartGroup="True">
                </px:PXLayoutRule>                                                                                    
                <px:PXSelector ID="edDfltSrvOrdType" runat="server" 
                    DataField="DfltSrvOrdType" DataSourceID="ds" AllowEdit = "True" 
                        AutoRefresh="True" >
                </px:PXSelector>
                <px:PXSelector ID="edDfltSOSrvOrdType" runat="server" 
                    DataField="DfltSOSrvOrdType" DataSourceID="ds" AllowEdit = "True" 
                        AutoRefresh="True" >
                </px:PXSelector>
                <px:PXSelector ID="edDfltCasesSrvOrdType" runat="server" 
                    DataField="DfltCasesSrvOrdType" DataSourceID="ds" AllowEdit = "True" 
                        AutoRefresh="True" >
                </px:PXSelector>
                <px:PXLayoutRule runat="server" EndGroup="True">
                </px:PXLayoutRule>
                <px:PXLabel runat="server" Height="10px"></px:PXLabel>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server" GroupCaption="Appointment Validation Settings" StartGroup="True">
                </px:PXLayoutRule>
                <px:PXDropDown ID="edDenyWarnBySkill" runat="server" DataField="DenyWarnBySkill">
                </px:PXDropDown>
                <px:PXDropDown ID="edDenyWarnByGeoZone" runat="server" DataField="DenyWarnByGeoZone">
                </px:PXDropDown>
                <px:PXDropDown ID="edDenyWarnByLicense" runat="server" DataField="DenyWarnByLicense">
                </px:PXDropDown>
                <px:PXDropDown ID="edDenyWarnByAppOverlap" runat="server" DataField="DenyWarnByAppOverlap">
                </px:PXDropDown>
                <px:PXLayoutRule runat="server" EndGroup="True">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server" GroupCaption="Schedule Optimization Settings" StartGroup="True"/>
                <px:PXTextEdit ID="edROWWApiEndPoint" runat="server" DataField="ROWWApiEndPoint"/>
                <px:PXTextEdit ID="edROWWLicensekey" runat="server" DataField="ROWWLicensekey"/>
                <px:PXLayoutRule runat="server" ControlSize="XS"/>
                <px:PXMaskEdit ID="edROLunchBreakDuration" runat="server" DataField="ROLunchBreakDuration" 
                    TextAlign="Right" CommitChanges="True"/>
                <px:PXDateTimeEdit ID="edROLunchBreakStartTimeFrame_Time" runat="server"
                    CommitChanges="True" DataField="ROLunchBreakStartTimeFrame_Time" TimeMode="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edROLunchBreakEndTimeFrame_Time" runat="server"
                    CommitChanges="True" DataField="ROLunchBreakEndTimeFrame_Time" TimeMode="True">
                </px:PXDateTimeEdit>
                <px:PXLayoutRule runat="server" EndGroup="True"/>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="Calendars and Maps">
            <Template>
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="XM" ControlSize="M">
                </px:PXLayoutRule>                                                                 
                <px:PXLayoutRule runat="server" GroupCaption="Calendar Settings" StartGroup="True">
                </px:PXLayoutRule>
                <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" 
                    DataSourceID="ds" CommitChanges="True" AllowEdit = "True" AutoRefresh="True">
                </px:PXSelector>
                <px:PXDropDown ID="edAppResizePrecision" runat="server" 
                    DataField="AppResizePrecision">
                </px:PXDropDown>
                <px:PXMaskEdit ID="edAppAutoConfirmGap" runat="server" 
                    DataField="AppAutoConfirmGap">
                </px:PXMaskEdit>
                <px:PXLayoutRule runat="server" Merge="True" ControlSize="XXS" LabelsWidth="XM" ></px:PXLayoutRule>
                <px:PXNumberEdit ID="edShowServiceOrderDaysGap" runat="server" 
                    DataField="ShowServiceOrderDaysGap" AllowNull="True">
                </px:PXNumberEdit>
                <px:PXLabel ID="PXLabel1" runat="server">Days</px:PXLabel>
                <px:PXLayoutRule runat="server">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server" GroupCaption="Map Settings" StartGroup="True">
                </px:PXLayoutRule>
                <px:PXTextEdit ID="edMapApiKey" runat="server" DataField="MapApiKey">
                </px:PXTextEdit>
                <px:PXLayoutRule runat="server" Merge="True" ControlSize="XXS" LabelsWidth="XM" ></px:PXLayoutRule>
                <px:PXNumberEdit ID="edGPSRefreshTrackingTime" runat="server" 
                    DataField="GPSRefreshTrackingTime" AllowNull="True">
                </px:PXNumberEdit>
                <px:PXLabel ID="PXLabel11" runat="server">Seconds</px:PXLabel>
                <px:PXLayoutRule runat="server" EndGroup="True">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server" Merge="True" ControlSize="XXS" LabelsWidth="XM" ></px:PXLayoutRule>
                <px:PXNumberEdit ID="edHistoryTimeAccuracy" runat="server" DataField="HistoryTimeAccuracy">
                </px:PXNumberEdit>
                <px:PXLabel ID="PXLabel12" runat="server">Minutes</px:PXLabel>
                <px:PXLayoutRule runat="server" EndGroup="True">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server" Merge="True" ControlSize="XXS" LabelsWidth="XM" ></px:PXLayoutRule>
                <px:PXNumberEdit ID="edHistoryDistanceAccuracy" runat="server" DataField="HistoryDistanceAccuracy">
                </px:PXNumberEdit>
                <px:PXLabel ID="PXLabel13" runat="server">Kilometers</px:PXLabel>
                <px:PXLayoutRule runat="server" EndGroup="True">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server"  LabelsWidth="L" ControlSize="M"></px:PXLayoutRule>
                <px:PXCheckBox ID="edEnableGPSTracking" runat="server" AlignLeft="True" DataField="EnableGPSTracking">
                </px:PXCheckBox>
                <px:PXLayoutRule runat="server" GroupCaption="Default Calendar Settings" StartGroup="True">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server"  LabelsWidth="XM" ControlSize="M"></px:PXLayoutRule>
                <px:PXDropDown ID="edDfltCalendarViewMode" runat="server" DataField="DfltCalendarViewMode">
                </px:PXDropDown>
                <px:PXDropDown ID="edTimeRange" runat="server" DataField="TimeRange">
                </px:PXDropDown>
                <px:PXDropDown ID="edTimeFilter" runat="server" DataField="TimeFilter">
                </px:PXDropDown>
                <px:PXDateTimeEdit runat="server" TimeMode="True" DataField="DfltCalendarStartTime_Time" 
                ID="edDfltCalendarStartTime_Time" CommitChanges="True" Size="M">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit runat="server" TimeMode="True" DataField="DfltCalendarEndTime_Time" 
                ID="edDfltCalendarEndTime_Time" CommitChanges="True" Size="M">
                </px:PXDateTimeEdit>
                <px:PXDropDown ID="edDayResolution" runat="server" DataField="DayResolution">
                </px:PXDropDown>
                <px:PXDropDown ID="edWeekResolution" runat="server" DataField="WeekResolution">
                </px:PXDropDown>
                <px:PXDropDown ID="edMonthResolution" runat="server" DataField="MonthResolution">
                </px:PXDropDown>
                <px:PXLayoutRule runat="server" EndGroup="True">
                </px:PXLayoutRule>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="Mailing Settings">
            <Template>
                <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" 
                    SkinID="Horizontal" Height="500px" SavePosition="True">
                    <AutoSize Enabled="True" ></AutoSize>
                    <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px"
                                Caption="Default Sources" AdjustPageSize="Auto" AllowPaging="True" TabIndex="300"
                                DataSourceID="ds">
                                <AutoCallBack Target="gridNR" Command="Refresh" ></AutoCallBack>
                                <Levels>
                                    <px:PXGridLevel DataMember="Notifications">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" ></px:PXLayoutRule>
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD"></px:PXMaskEdit>
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name"></px:PXSelector> 
                                            <px:PXDropDown ID="edFormat" runat="server" DataField="Format"></px:PXDropDown>  
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active"></px:PXCheckBox> 
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID"></px:PXSelector>
                                            <px:PXSelector ID="edEMailAccount" runat="server" DataField="EMailAccountID" DisplayMode="Text"></px:PXSelector>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="NotificationCD"></px:PXGridColumn>    
                                            <px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text"></px:PXGridColumn>    
                                            <px:PXGridColumn DataField="ReportID" AutoCallBack="True"></px:PXGridColumn> 
                                            <px:PXGridColumn DataField="NotificationID" AutoCallBack="True"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True"></px:PXGridColumn>
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>
                                                
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" ></AutoSize>
                            </px:PXGrid>
                    </Template1>
                    <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Default Recipients"
                                AdjustPageSize="Auto" AllowPaging="True" TabIndex="400" DataSourceID="ds">
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="gridNS" ></px:PXSyncGridParam>
                                </Parameters>
                                <CallbackCommands>
                                    <Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds"></Save>
                                    <FetchRow RepaintControls="None" ></FetchRow>
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="Recipients" DataKeyNames="RecipientID">
                                        <Columns>
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True" AutoCallBack="True">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" ></px:PXGridColumn>
                                            <px:PXGridColumn DataField="ContactID">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" ></px:PXControlParam>
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox">
                                            </px:PXGridColumn>
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" ></px:PXLayoutRule>
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True"
                                                ValueField="DisplayName" AllowEdit="True">
                                            </px:PXSelector>
                                        </RowTemplate>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" ></AutoSize>
                            </px:PXGrid>
                    </Template2>
                </px:PXSplitContainer>
            </Template>
        </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>
