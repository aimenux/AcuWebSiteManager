<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ405000.aspx.cs" Inherits="Page_PJ405000" Title="Photo Logs" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.PhotoLogs.PJ.Graphs.PhotoLogMaint" PrimaryView="Filter" >
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="DownloadZip" />
            <px:PXDSCallbackCommand Name="EmailPhotoLog" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="PhotoLogsFilterContent" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="PhotoLogsFilter" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edProjectIdFilter" runat="server" DataField="ProjectId" CommitChanges="True"
                AutoRefresh="True" />
            <px:PXSelector ID="edProjectTaskIdFilter" runat="server" DataField="ProjectTaskId" CommitChanges="True"
                AutoRefresh="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit ID="edDateFromFilter" runat="server" DataField="DateFrom" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edDateToFilter" runat="server" DataField="DateTo" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="PhotoLogsContent" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" SplitterPosition="1100" Orientation="Vertical" Height="100%" Width="100%">
        <AutoSize Enabled="True" Container="Window" />
        <Template1>
            <px:PXGrid ID="gridPhotoLogs" runat="server" SkinID="Inquire" Width="100%" DataSourceID="ds"
                AdjustPageSize="Auto" AllowPaging="True" TabIndex="300" SyncPosition="True" AllowFilter="True"
                AllowSearch="True" FastFilterFields="PhotoLogCd, ProjectId, ProjectTaskId, CreatedById">
                <AutoCallBack Target="mainPhoto" Command="Refresh" ActiveBehavior="True">
                    <Behavior CommitChanges="True" RepaintControlsIDs="mainPhoto" />
                </AutoCallBack>
                <Levels>
                    <px:PXGridLevel DataMember="PhotoLogs">
                        <Columns>
                            <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                                TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                            <px:PXGridColumn DataField="PhotoLogCd" LinkCommand="editPhotoLog" />
                            <px:PXGridColumn DataField="StatusId" />
                            <px:PXGridColumn DataField="Date" />
                            <px:PXGridColumn DataField="ProjectId" LinkCommand="ViewEntity" />
                            <px:PXGridColumn DataField="ProjectTaskId" LinkCommand="ViewEntity" />
                            <px:PXGridColumn DataField="Description" />
                            <px:PXGridColumn DataField="CreatedById" LinkCommand="ViewEntity" />
                        </Columns>
                        <Mode AllowUpdate="False" AllowAddNew="False" />
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="true" />
            </px:PXGrid>
       </Template1>
       <Template2>
           <px:PXFormView ID="mainPhoto" runat="server" DataSourceID="ds" Width="100%" DataMember="MainPhoto">
               <AutoSize Container="Window" Enabled="True" />
               <Template>
                   <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"
                       GroupCaption="Main Photo Preview" />
                   <px:PXImageView ID="edImageUrl" runat="server" DataField="ImageUrl"
                       Style="position: absolute; max-height: 550px; max-width: 600px;" />
               </Template>
           </px:PXFormView>
       </Template2>
    </px:PXSplitContainer>
</asp:Content>