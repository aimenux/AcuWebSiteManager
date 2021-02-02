<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
CodeFile="PJ504010.aspx.cs" Inherits="Page_PJ504010" Title="Clear Daily Field Report Weather Processing Log" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
                     TypeName="PX.Objects.PJ.DailyFieldReports.PJ.Graphs.ClearWeatherProcessingLogProcess" 
                     PrimaryView="WeatherProcessingLogs">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="WeatherProcessingLogsGrid" runat="server" DataSourceID="ds"
               Width="100%" Height="150px" Style="z-index: 100" SkinID="PrimaryInquire" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="WeatherProcessingLogs">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                                     TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                    <px:PXGridColumn DataField="DailyFieldReport__DailyFieldReportCd" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="DailyFieldReport__Status"/>
                    <px:PXGridColumn DataField="DailyFieldReport__Date"/>
                    <px:PXGridColumn DataField="DailyFieldReport__ProjectId" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="DailyFieldReport__ProjectManagerId" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="DailyFieldReport__CreatedById" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="WeatherService"/>
                    <px:PXGridColumn DataField="RequestTime" DisplayFormat="g"/>
                    <px:PXGridColumn DataField="RequestBody"/>
                    <px:PXGridColumn DataField="RequestStatusIcon"/>
                    <px:PXGridColumn DataField="ResponseTime" DisplayFormat="g"/>
                    <px:PXGridColumn DataField="ResponseBody"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"/>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
    </px:PXGrid>
</asp:Content>