<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM403000.aspx.cs" Inherits="Page_SM403000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.FS.LocationTrackingInq" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
   <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="M" />
            <px:PXDateTimeEdit runat="server" DataSourceID="ds" ID="edDate" DataField="Date" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds" BatchUpdate="True"
        SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="ExecutionDate,Username,FullName">
        <Levels>
            <px:PXGridLevel DataMember="LocationTrackingRecords">
                <RowTemplate>
                    <px:PXDateTimeEdit runat="server" ID="edExecutionDate" DataField="ExecutionDate" />
                    <px:PXTextEdit runat="server" ID="edUsername" DataField="Username" />
                    <px:PXTextEdit runat="server" ID="edFullName" DataField="FullName" />
                    <px:PXNumberEdit runat="server" ID="edLatitude" DataField="Latitude" />
                    <px:PXNumberEdit runat="server" ID="edLongitude" DataField="Longitude" />
                    <px:PXNumberEdit runat="server" ID="edAltitude" DataField="Altitude" />
                    <px:PXNumberEdit runat="server" ID="edDistance" DataField="Distance" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="ExecutionDate" />
                    <px:PXGridColumn DataField="Username" />
                    <px:PXGridColumn DataField="FullName" />
                    <px:PXGridColumn DataField="Latitude" />
                    <px:PXGridColumn DataField="Longitude" />
                    <px:PXGridColumn DataField="Altitude" />
                    <px:PXGridColumn DataField="Distance" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Layout ShowRowStatus="False" />
    </px:PXGrid>
</asp:Content>