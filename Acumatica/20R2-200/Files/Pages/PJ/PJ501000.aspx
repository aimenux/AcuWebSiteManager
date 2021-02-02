<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PJ501000.aspx.cs" Inherits="Page_PJ501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" AutoCallBack="True" Visible="True"
        IsDefaultDatasourceWidth="100%" TypeName="PX.Objects.PJ.RequestsForInformation.PJ.Graphs.AssignRequestForInformationMassProcess"
            PrimaryView="RequestsForInformation" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="RequestsForInformation_ViewDetails" Visible="false" DependOnGrid="gridItems" />
            <px:PXDSCallbackCommand Name="RequestsForInformation_EntityDetails" Visible="false" DependOnGrid="gridItems" />
            <px:PXDSCallbackCommand Name="RequestsForInformation_ContactDetails" Visible="false" DependOnGrid="gridItems" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Width="100%" Height="150px"
        SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Matching Records"
            RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="RequestsForInformation">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                        TextAlign="Center" Type="CheckBox" Width="40px" />
                    <px:PXGridColumn DataField="RequestForInformationCd" DisplayFormat="&gt;aaaaaaaaaa" Width="120px"
                        LinkCommand="RequestsForInformation_ViewDetails" />
                    <px:PXGridColumn DataField="Summary" Width="300px" />
                    <px:PXGridColumn AllowNull="False" DataField="Status" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="Reason" Width="90px" />
                    <px:PXGridColumn DataField="PriorityId" Width="90px" />
                    <px:PXGridColumn DataField="DueResponseDate" Width="90px" />
                    <px:PXGridColumn DataField="ClassId" Width="90px" />
                    <px:PXGridColumn DataField="BusinessAccountId" Width="120px"
                        LinkCommand="RequestsForInformation_EntityDetails" />
                    <px:PXGridColumn DataField="ContactId" Width="140px" DisplayMode="Text"
                        LinkCommand="RequestsForInformation_ContactDetails" />
                    <px:PXGridColumn DataField="OwnerId" Width="120px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="CreationDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar PagerVisible="False"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>