<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ404000.aspx.cs" Inherits="Page_PJ404000" Title="Daily Field Report Weather Processing Log" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.DailyFieldReports.PJ.Graphs.DailyFieldReportWeatherProcessingLogInquiry"
        PrimaryView="WeatherProcessingLogs" />
</asp:Content>
<asp:Content ID="FilterContent" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter"
        Caption="Selection" DefaultControlID="edProjectId">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="True" />
            <px:PXDropDown ID="edWeatherApiService" runat="server" DataField="WeatherApiService" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit ID="edRequestDateFrom" runat="server" DataField="RequestDateFrom" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edRequestDateTo" runat="server" DataField="RequestDateTo" CommitChanges="True" />
            <px:PXCheckBox ID="chkIsShowErrorsOnly" runat="server" DataField="IsShowErrorsOnly" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="DailyFieldReportTabsContent" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="WeatherProcessingLogsGrid" runat="server" DataSourceID="ds"
        Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" NoteIndicator="False"
        FastFilterFields="DailyFieldReport__DailyFieldReportCd,DailyFieldReport__Status,
        DailyFieldReport__ProjectId,DailyFieldReport__ProjectManagerId,DailyFieldReport__CreatedById,WeatherService"
        FilesIndicator="False" AdjustPageSize="Auto" AllowPaging="True" Caption="Daily Field Report Weather Processing Log">
        <Levels>
            <px:PXGridLevel DataMember="WeatherProcessingLogs">
                <Columns>
                    <px:PXGridColumn DataField="DailyFieldReport__DailyFieldReportCd" LinkCommand="ViewEntity" />
                    <px:PXGridColumn DataField="DailyFieldReport__Status" />
                    <px:PXGridColumn DataField="DailyFieldReport__Date" />
                    <px:PXGridColumn DataField="DailyFieldReport__ProjectId" LinkCommand="ViewEntity" />
                    <px:PXGridColumn DataField="DailyFieldReport__ProjectManagerId" LinkCommand="ViewEntity" />
                    <px:PXGridColumn DataField="DailyFieldReport__CreatedById" LinkCommand="ViewEntity" />
                    <px:PXGridColumn DataField="WeatherService" />
                    <px:PXGridColumn DataField="RequestTime" DisplayFormat="g" />
                    <px:PXGridColumn DataField="RequestBody" />
                    <px:PXGridColumn DataField="RequestStatusIcon" />
                    <px:PXGridColumn DataField="ResponseTime" DisplayFormat="g" />
                    <px:PXGridColumn DataField="ResponseBody" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>