<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SF205040.aspx.cs"
    Inherits="Page_SF205040" Title="Real-Time Sync State" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Salesforce.SFRealtimeSyncStateMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="GoToSalesforce" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ShowEntity" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ResetSync" Visible="true" DependOnGrid="grid"></px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>


<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">

    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" TabIndex="900">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXDropDown ID="edEntity" runat="server" CommitChanges="True" DataField="Entity" Size="M" />
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Size="M" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartRow="True">
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edErrorsOnly" runat="server" DataField="ErrorsOnly" CommitChanges="True" AlreadyLocalized="False" />
        </Template>
    </px:PXFormView>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">

    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Height="150px" CaptionVisible="False"
        AllowPaging="True" AdjustPageSize="Auto" NoteIndicator="False" FilesIndicator="False"
        SkinID="Inquire" TabIndex="500" SyncPosition="True" AutoAdjustColumns="True" MatrixMode="True">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="30px" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="EntityType" Width="90px" DisplayMode="Text" TextAlign="Left" />
                    <px:PXGridColumn DataField="LocalID" Width="100px" DisplayMode="Text" TextAlign="Left" />
                    <px:PXGridColumn DataField="DisplayName" Width="100px" DisplayMode="Text" TextAlign="Left" LinkCommand="ShowEntity" />
                    <px:PXGridColumn DataField="LocalTS" Width="90px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="RemoteID" Width="130px" CommitChanges="True" LinkCommand="GoToSalesforce" />
                    <px:PXGridColumn DataField="RemoteTS" Width="90px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="Operation" Width="60px" />
                    <px:PXGridColumn DataField="Status" Width="90px" />
                    <px:PXGridColumn DataField="LastErrorMessage" Width="130" />
                    <px:PXGridColumn DataField="LastAttemptTS" Width="90px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="AttemptCount" Width="50px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar PagerVisible="False" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="false" />
    </px:PXGrid>
</asp:Content>
