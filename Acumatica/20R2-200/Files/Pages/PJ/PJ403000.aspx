<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ403000.aspx.cs" Inherits="Page_PJ403000" Title="Drawing Logs" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.DrawingLogs.PJ.Graphs.DrawingLogsMaint" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewEntity" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="InsertDrawingLogInGrid" Visible="false" />
        </CallbackCommands>
        <ClientEvents BeforeRedirect="updateIsDirtyField" />
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
    <script type="text/javascript">
        var CreateEntityActionNamePattern = "createIssue@";

        function updateIsDirtyField(dataSource, event) {
            if (event.context.command.startsWith(CreateEntityActionNamePattern)) {
                dataSource.isDirty = false;
            }
        }
    </script>
    <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
        Caption="Selection" DataMember="Filter">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edSelectionProjectId" runat="server" DataField="ProjectId" CommitChanges="True"
                AutoRefresh="True" />
            <px:PXSelector ID="edSelectionProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="True"
                AutoRefresh="True" />
            <px:PXSelector ID="edSelectionDisciplineId" runat="server" DataField="DisciplineId" CommitChanges="True"
                AutoRefresh="True" />
            <px:PXCheckBox ID="chkIsCurrentOnly" runat="server" DataField="IsCurrentOnly" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%"
        Caption="Drawing Logs" AdjustPageSize="Auto" SkinID="Details" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="DrawingLogs">
                <RowTemplate>
                    <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" Enabled="False" />
                    <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="True"
                        AutoRefresh="True" />
                    <px:PXSelector ID="edDisciplineId" runat="server" DataField="DisciplineId" CommitChanges="True"
                        AutoRefresh="True"  DisplayMode="Text"/>
                    <px:PXTextEdit ID="edNumber" runat="server" DataField="Number" />
                    <px:PXTextEdit ID="edRevision" runat="server" DataField="Revision" />
                    <px:PXTextEdit ID="edSketch" runat="server" DataField="Sketch" />
                    <px:PXTextEdit ID="edTitle" runat="server" DataField="Title" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXSelector ID="edStatusId" runat="server" DataField="StatusId" CommitChanges="True" AutoRefresh="True" />
                    <px:PXDateTimeEdit ID="edDrawingDate" runat="server" DataField="DrawingDate" AutoCallBack="True" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edReceivedDate" runat="server" DataField="ReceivedDate" AutoCallBack="True" CommitChanges="True" />
                    <px:PXSelector ID="edOwnerId" runat="server" DataField="OwnerId" CommitChanges="True" AutoRefresh="True" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                        TextAlign="Center" Type="CheckBox" Width="40px" CommitChanges="True" />
                    <px:PXGridColumn DataField="DrawingLogCd" AutoCallBack="True" LinkCommand="editDrawingLog" />
                    <px:PXGridColumn DataField="ProjectId" AutoCallBack="True" LinkCommand="ViewEntity" />
                    <px:PXGridColumn DataField="ProjectTaskId" AutoCallBack="True"
                        LinkCommand="ViewEntity" CommitChanges="True" />
                    <px:PXGridColumn DataField="DisciplineId" AutoCallBack="True" CommitChanges="True" Width="120px"  DisplayMode="Text"/>
                    <px:PXGridColumn DataField="Number" CommitChanges="True" />
                    <px:PXGridColumn DataField="Revision" CommitChanges="True" />
                    <px:PXGridColumn DataField="Sketch" CommitChanges="True" />
                    <px:PXGridColumn DataField="Title" CommitChanges="True" />
                    <px:PXGridColumn DataField="Description" CommitChanges="True" />
                    <px:PXGridColumn DataField="StatusId" CommitChanges="True" />
                    <px:PXGridColumn DataField="DrawingDate" AutoCallBack="True" CommitChanges="True" />
                    <px:PXGridColumn DataField="ReceivedDate" AutoCallBack="True" CommitChanges="True" />
                    <px:PXGridColumn DataField="OriginalDrawingId" AutoCallBack="True" LinkCommand="DrawingLog$OriginalDrawingId$Link" Width="120px"/>
                    <px:PXGridColumn DataField="OwnerId" CommitChanges="True" />
                    <px:PXGridColumn DataField="IsDrawingLogCurrentFile" Type="CheckBox" TextAlign="Center" CommitChanges="True" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode InitNewRow="True" AllowAddNew="False" AllowUpload="True" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton ImageKey="AddNew" CommandSourceID="ds" CommandName="InsertDrawingLogInGrid" DisplayStyle="Image" />
            </CustomItems>
            <Actions>
                <Delete GroupIndex="1" />
                <AddNew ToolBarVisible="False" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>