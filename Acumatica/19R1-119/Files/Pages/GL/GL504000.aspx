<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL504000.aspx.cs" Inherits="Page_GL504000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter"
        TypeName="PX.Objects.GL.ScheduleRun">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="Filter" Caption="Schedule Date Range"
        DefaultControlID="edStartDate" DataSourceID="ds" TabIndex="100" MarkRequired="Dynamic">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="ExecutionDate" />
            <px:PXGroupBox ID="gbLimitType" runat="server" Caption="Stop" CommitChanges="True"
                DataField="LimitTypeSel" RenderSimple="True" RenderStyle="Simple">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXRadioButton ID="rbTillDate" runat="server" GroupName="gbLimitType" Value="D" />
                    <px:PXRadioButton ID="rbMultipleTimes" runat="server" GroupName="gbLimitType" Value="M" />
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXLabel runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXNumberEdit ID="edTimes" runat="server" DataField="RunLimit" SuppressLabel="True" />
                </Template>
                <ContentLayout LabelsWidth="S" />
            </px:PXGroupBox>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" AllowPaging="True" AdjustPageSize="Auto"
        Caption="Schedules" EditPageUrl="~/Pages/GL/GL203500.aspx" BatchUpdate="True"
        AllowSearch="true" SkinID="PrimaryInquire" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Schedule_List">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"
                        AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="ScheduleID" Width="100px" LinkCommand="EditDetail" />
                    <px:PXGridColumn DataField="ScheduleName" Width="200px" />
                    <px:PXGridColumn DataField="StartDate" Width="100px" />
                    <px:PXGridColumn DataField="EndDate" Width="90px" />
                    <px:PXGridColumn DataField="RunCntr" TextAlign="Right" Width="70px" />
                    <px:PXGridColumn DataField="RunLimit" TextAlign="Right" Width="70px" />
                    <px:PXGridColumn DataField="NextRunDate" Width="100px" />
                    <px:PXGridColumn DataField="LastRunDate" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode AllowUpdate="false" />
        <EditPageParams>
            <px:PXControlParam ControlID="grid" Name="ScheduleID" PropertyName="DataValues[&quot;ScheduleID&quot;]"
                Size="10" Type="String" />
        </EditPageParams>
    </px:PXGrid>
</asp:Content>
