<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PJ502000.aspx.cs" Inherits="Page_PJ502000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" AutoCallBack="True" Visible="True"
        IsDefaultDatasourceWidth="100%" TypeName="PX.Objects.PJ.ProjectsIssue.PJ.Graphs.AssignProjectIssueMassProcess"
        PrimaryView="ProjectIssues" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="ProjectIssues_ViewDetails" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Width="100%" Height="150px"
        SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Matching Records" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="ProjectIssues">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                        TextAlign="Center" Type="CheckBox" Width="40px" />
                    <px:PXGridColumn DataField="ProjectIssueCd" DisplayFormat="&gt;aaaaaaaaaa" Width="120px"
                        LinkCommand="ProjectIssues_ViewDetails" />
                    <px:PXGridColumn DataField="Summary" Width="300px" />
                    <px:PXGridColumn AllowNull="False" DataField="Status" Width="90px" />
                    <px:PXGridColumn DataField="PriorityId" Width="90px" />
                    <px:PXGridColumn DataField="DueDate" Width="90px" />
                    <px:PXGridColumn DataField="ClassId" Width="90px" />
                    <px:PXGridColumn DataField="OwnerID" Width="120px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="WorkgroupID" Width="120px" />
                    <px:PXGridColumn DataField="CreationDate" Width="120px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar PagerVisible="False" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>