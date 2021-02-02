<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SM200525.aspx.cs" Inherits="Page_SM200525"
    Title="Untitled Page" %>

<%@ Register TagPrefix="px" Namespace="PX.Web.Controls" Assembly="PX.Web.Controls" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.KBFeedbackExplore" PrimaryView="Responses">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Feedback" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top"
        AllowPaging="True" AdjustPageSize="Auto" SkinID="Details" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Responses">
                <Columns>
                    <px:PXGridColumn DataField="FeedbackID" Width="100" LinkCommand="Feedback" />
                    <px:PXGridColumn DataField="Users__Username" Width="120px"/>
                    <px:PXGridColumn DataField="Date" Width="60px"/>
                    <px:PXGridColumn DataField="IsFind" Width="100" />
                    <px:PXGridColumn DataField="Satisfaction" Width="150" />
                    <px:PXGridColumn DataField="Summary" Width="600" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <Actions>                
                <AddNew Enabled="False" />
                <Delete Enabled="False" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>

