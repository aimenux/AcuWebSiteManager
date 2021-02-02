<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP504000.aspx.cs" Inherits="Page_AP504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.AP.APScheduleRun" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Schedule Date Range" DefaultControlID="edStartDate" MarkRequired="Dynamic">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="ExecutionDate" />
            <px:PXGroupBox ID="gbLimitType" runat="server" Caption="Stop" CommitChanges="True"
                DataField="LimitTypeSel" RenderSimple="True" RenderStyle="Simple">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" />
                    <px:PXRadioButton ID="rbTillDate" runat="server" GroupName="gbLimitType" Value="D" />
                    <px:PXRadioButton ID="rbMultipleTimes" runat="server" GroupName="gbLimitType" Value="M" />
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" />
                    <px:PXLabel runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
                    <px:PXNumberEdit ID="edTimes" runat="server" DataField="RunLimit" SuppressLabel="True" />
                </Template>
                <ContentLayout LabelsWidth="S" />
            </px:PXGroupBox>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto" DataSourceID="ds" Caption="Schedules" EditPageUrl="~/Pages/AP/AP203500.aspx"
        BatchUpdate="True" AllowSearch="true" SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="ScheduleID,ScheduleName">
        <Levels>
            <px:PXGridLevel DataMember="Schedule_List">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="ScheduleID" LinkCommand="ViewSchedule" />
                    <px:PXGridColumn DataField="ScheduleName" />
                    <px:PXGridColumn DataField="StartDate" />
                    <px:PXGridColumn DataField="EndDate" />
                    <px:PXGridColumn DataField="RunCntr" TextAlign="Right" />
                    <px:PXGridColumn DataField="RunLimit" TextAlign="Right" />
                    <px:PXGridColumn DataField="NextRunDate" />
                    <px:PXGridColumn DataField="LastRunDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <EditPageParams>
            <px:PXControlParam ControlID="grid" Name="ScheduleID" PropertyName="DataValues[&quot;ScheduleID&quot;]" Size="10" Type="String" />
        </EditPageParams>
    </px:PXGrid>
</asp:Content>
