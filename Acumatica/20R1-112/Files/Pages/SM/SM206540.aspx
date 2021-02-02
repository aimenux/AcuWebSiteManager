<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM206540.aspx.cs" Inherits="Page_SM206540" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource id="ds" width="100%" runat="server" typename="PX.SM.SMScannerMaint" primaryview="Scanners" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="UpdateScannerList" Visible="True" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid id="grid" runat="server" height="400px" width="100%" allowpaging="True" adjustpagesize="Auto" allowsearch="True" 
               skinid="Primary" DataSourceID="ds" TabIndex="100" MatrixMode="true">
        <Levels>
            <px:PXGridLevel DataMember="Scanners">
                <Columns>
                    <px:PXGridColumn DataField="DeviceHubID" />
					<px:PXGridColumn DataField="ScannerName" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="PaperSourceDefValue" TextAlign="Left" />
                    <px:PXGridColumn DataField="PixelTypeDefValue" TextAlign="Left" />
                    <px:PXGridColumn DataField="ResolutionDefValue" TextAlign="Left" />
                    <px:PXGridColumn DataField="FileTypeDefValue" TextAlign="Left" />
					<px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" ></AutoSize>
    </px:PXGrid>
</asp:Content>