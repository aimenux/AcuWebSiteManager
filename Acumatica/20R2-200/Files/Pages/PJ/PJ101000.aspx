<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PJ101000.aspx.cs" Inherits="Page_PJ101000" Title="Project Management Lite Preferences" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ProjectManagementSetup"
        TypeName="PX.Objects.PJ.ProjectManagement.PJ.Graphs.ProjectManagementSetupMaint" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="487px" Style="z-index: 100"
        Width="100%" DataMember="ProjectManagementSetup" Caption="General Settings"
        DefaultControlID="edAnswerDaysCalculationType">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Due Date Calculation" />
                    <px:PXDropDown ID="edAnswerDaysCalculationType" runat="server" DataField="AnswerDaysCalculationType" CommitChanges="True" />
                    <px:PXSelector runat="server" ID="edCalendarId" DataField="CalendarId" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Request For Information Settings" />
                    <px:PXSelector CommitChanges="True" ID="edRequestForInformationNumberingId" runat="server" AllowNull="False"
                        DataField="RequestForInformationNumberingId" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edDefaultEmailNotification" runat="server"
                        DataField="DefaultEmailNotification" />
                    <px:PXSelector AllowEdit="True" runat="server" ID="edRequestForInformationAssignmentMapId"
                        DataField="RequestForInformationAssignmentMapId" TextField="Name" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Daily Field Report Settings" />
                    <px:PXSelector CommitChanges="True" ID="edDailyFieldReportNumberingId" runat="server" AllowNull="False"
                        DataField="DailyFieldReportNumberingId" AllowEdit="True" />
                    <px:PXSelector AllowEdit="True" runat="server" ID="edDailyFieldReportApprovalMapId"
                        DataField="DailyFieldReportApprovalMapId" TextField="Name" />
                    <px:PXSelector AllowEdit="True" runat="server" ID="edPendingApprovalNotification"
                        DataField="PendingApprovalNotification" TextField="Name" />
                    <px:PXCheckBox runat="server" ID="chkHistoryLog" DataField="IsHistoryLogEnabled" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Project Issue Settings" />
                    <px:PXSelector CommitChanges="True" ID="edProjectIssueNumberingId" runat="server" AllowNull="False"
                        DataField="ProjectIssueNumberingId" AllowEdit="True" />
                    <px:PXSelector AllowEdit="True" runat="server" ID="edProjectIssueAssignmentMapId"
                        DataField="ProjectIssueAssignmentMapId" TextField="Name" />
                    <px:PXGrid ID="edProjectIssueType" runat="server" DataSourceID="ds" Height="140px" Width="400px" SkinID="ShortList">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ProjectIssueTypes">
                                <Columns>
                                    <px:PXGridColumn DataField="TypeName" />
                                    <px:PXGridColumn DataField="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Daily Field Report Copy Settings">
                <Template>
                    <px:PXFormView ID="DailyFieldReportCopyConfigurationForm" runat="server" DataSourceID="ds" Width="100%"
                        Style="margin: 9px" DataMember="DailyFieldReportCopyConfiguration" RenderStyle="Simple">
                        <ContentStyle BorderStyle="None" />
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ColumnWidth="M" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Common" />
                            <px:PXCheckBox ID="chkIsConfigurationEnabled" runat="server"
                                DataField="IsConfigurationEnabled" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartRow="True" ColumnWidth="M" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Daily Field Report Summary"
                                SuppressLabel="True" />
                            <px:PXCheckBox ID="chkNotes" runat="server" DataField="Notes" CommitChanges="True" />
                            <px:PXCheckBox ID="chkDate" runat="server" DataField="Date" CommitChanges="True" />
                            <px:PXCheckBox ID="chkProjectManager" runat="server" DataField="ProjectManager"
                                CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Labor Time And Activities"
                                SuppressLabel="True" ColumnWidth="M" />
                            <px:PXCheckBox ID="chkEmployee" runat="server" DataField="Employee" CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeEarningType" runat="server" DataField="EmployeeEarningType"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeProjectTask" runat="server" DataField="EmployeeProjectTask"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeCostCode" runat="server" DataField="EmployeeCostCode"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeTime" runat="server" DataField="EmployeeTime"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeTimeSpent" runat="server" DataField="EmployeeTimeSpent"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeIsBillable" runat="server" DataField="EmployeeIsBillable"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeBillableTime" runat="server" DataField="EmployeeBillableTime"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeDescription" runat="server" DataField="EmployeeDescription"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeTask" runat="server" DataField="EmployeeTask"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeCertifiedJob" runat="server" DataField="EmployeeCertifiedJob"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeUnionLocal" runat="server" DataField="EmployeeUnionLocal"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeLaborItem" runat="server" DataField="EmployeeLaborItem"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeWccCode" runat="server" DataField="EmployeeWccCode"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEmployeeContract" runat="server" DataField="EmployeeContract"
                                CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ColumnWidth="M" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Subcontractors" />
                            <px:PXCheckBox ID="chkSubcontractorVendorId" runat="server" DataField="SubcontractorVendorId"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkSubcontractorProjectTask" runat="server" DataField="SubcontractorProjectTask"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkSubcontractorCostCode" runat="server" DataField="SubcontractorCostCode"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkSubcontractorNumberOfWorkers" runat="server" DataField="SubcontractorNumberOfWorkers"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkSubcontractorTimeArrived" runat="server" DataField="SubcontractorTimeArrived"
                                CommitChanges="True"/>
                            <px:PXCheckBox ID="chkSubcontractorTimeDeparted" runat="server" DataField="SubcontractorTimeDeparted"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkSubcontractorWorkingHours" runat="server" DataField="SubcontractorWorkingHours"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkSubcontractorDescription" runat="server" DataField="SubcontractorDescription"
                                CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Equipment" StartColumn="True"
                                SuppressLabel="True" ColumnWidth="M" />
                            <px:PXCheckBox ID="chkEquipmentId" runat="server" DataField="EquipmentId"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentProjectTask" runat="server" DataField="EquipmentProjectTask"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentCostCode" runat="server" DataField="EquipmentCostCode"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentIsBillable" runat="server" DataField="EquipmentIsBillable"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentSetupTime" runat="server" DataField="EquipmentSetupTime"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentRunTime" runat="server" DataField="EquipmentRunTime"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentSuspendTime" runat="server" DataField="EquipmentSuspendTime"
                                CommitChanges="True" />
                            <px:PXCheckBox ID="chkEquipmentDescription" runat="server" DataField="EquipmentDescription"
                                CommitChanges="True" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Weather Service Integration Settings">
                <Template>
                    <px:PXFormView ID="WeatherIntegrationConfigurationForm" runat="server" DataSourceID="ds" Style="margin: 9px"
                        DataMember="WeatherIntegrationSetup" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ColumnWidth="XL" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="General Settings" />
                            <px:PXCheckBox ID="chkIsConfigurationWeatherServiceEnabled" runat="server" 
                                DataField="IsConfigurationEnabled" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ColumnWidth="XL" />
                            <px:PXLayoutRule runat="server" StartGroup="True" />
                            <px:PXDropDown ID="edWeatherApiService" runat="server" DataField="WeatherApiService" CommitChanges="True" />
                            <px:PXTextEdit ID="edWeatherApiKey" runat="server" DataField="WeatherApiKey" />
                            <px:PXDropDown ID="edUnitOfMeasureFormat" runat="server" DataField="UnitOfMeasureFormat" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ColumnWidth="XM" />
                            <px:PXDropDown ID="edRequestParametersType" runat="server" DataField="RequestParametersType" />
                            <px:PXButton ID="btnTestConnection" runat="server" Text="Test Connection" CommandSourceID="ds" CommandName="TestConnection" />
                            <px:PXLayoutRule runat="server" StartGroup="True" ColumnWidth="XM" />
                            <px:PXCheckBox ID="chkIsWeatherProcessingLogEnabled" AlignLeft="True" runat="server" DataField="IsWeatherProcessingLogEnabled" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartGroup="True" LabelsWidth="XM" />
                            <px:PXNumberEdit ID="edWeatherProcessingLogTerm" runat="server" DataField="WeatherProcessingLogTerm" CommitChanges="True" Width="40px" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Submittal Settings">
                <Template>
                    <px:PXFormView ID="SubmittalSettingsForm" runat="server" DataSourceID="ds" Style="margin: 9px"
                        DataMember="ProjectManagementSetup" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Submittal Settings" />
                            <px:PXSelector CommitChanges="True" ID="edSubmittalNumberingId" runat="server" AllowNull="False"
                                DataField="SubmittalNumberingId" AllowEdit="True" />
                            <px:PXGrid ID="edSubmittalType" runat="server" DataSourceID="ds" Height="140px" Width="400px" SkinID="ShortList" Caption="Submittal Types">
                                <Mode InitNewRow="True" />
                                <Levels>
                                    <px:PXGridLevel DataMember="SubmittalTypes">
                                        <Columns>
                                            <px:PXGridColumn DataField="TypeName" />
                                            <px:PXGridColumn DataField="Description" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>