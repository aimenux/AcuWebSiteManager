<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" 
    CodeFile="PJ504000.aspx.cs" Inherits="Page_PJ504000" Title="Load Daily Field Report Weather Conditions" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.DailyFieldReports.PJ.Graphs.DailyFieldReportWeatherProcess" PrimaryView="Filter">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="DailyFieldReportWeatherContent" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" Style="z-index: 100" Caption="Selection" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%">
        <Template>
            <px:PXLayoutRule runat="server" ID="PXLayoutRule1" StartRow="True" LabelsWidth="S" ControlSize="XM"/>
            <px:PXSelector CommitChanges="True" runat="server" ID="edProject" DataField="ProjectId" AutoRefresh="True"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="DailyFieldReportsGrid" runat="server" DataSourceID="ds" Width="100%" Height="150px" Style="z-index: 100" SkinID="PrimaryInquire" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="DailyFieldReports" >
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                                     TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                    <px:PXGridColumn DataField="DailyFieldReportCd" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="Status" />
                    <px:PXGridColumn DataField="Date" />
                    <px:PXGridColumn DataField="ProjectId" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="ProjectManagerId" LinkCommand="ViewEntity"/>
                    <px:PXGridColumn DataField="CreatedById" LinkCommand="ViewEntity"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>