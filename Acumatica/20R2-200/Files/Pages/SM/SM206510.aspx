<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM206510.aspx.cs" Inherits="Page_SM206510" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:pxdatasource id="ds" width="100%" runat="server" typename="PX.SM.SMPrinterMaint" primaryview="Printers" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="UpdatePrinterList" Visible="True" CommitChanges="true" />
        </CallbackCommands>
    </px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid id="grid" runat="server" height="400px" width="100%" allowpaging="True" adjustpagesize="Auto" allowsearch="True" skinid="Primary">
        <Levels>
            <px:PXGridLevel DataMember="Printers">
                <Columns>
                    <px:PXGridColumn DataField="DeviceHubID" />
					<px:PXGridColumn DataField="PrinterName" />
                    <px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="DefaultNumberOfCopies" />
                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" ></AutoSize>
    </px:PXGrid>
</asp:Content>