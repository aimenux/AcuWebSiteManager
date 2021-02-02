<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM206530.aspx.cs" Inherits="Page_SM206530" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource id="ds" width="100%" runat="server" TypeName="PX.SM.ScaleMaint" primaryview="Scale" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Cancel" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:content id="cont2" contentplaceholderid="phL" runat="Server">
	    <px:PXGrid id="grid" runat="server" height="400px" width="100%" allowpaging="True" adjustpagesize="Auto" allowsearch="True" skinid="Primary">
        <Levels>
            <px:PXGridLevel DataMember="Scale">
                <Columns>
                    <px:PXGridColumn DataField="DeviceHubID" />
					<px:PXGridColumn DataField="ScaleID" />
                    <px:PXGridColumn DataField="Descr" />
                    <px:PXGridColumn DataField="LastWeight" />
					<px:PXGridColumn DataField="LastModifiedDateTime" DisplayFormat="g"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" ></AutoSize>
    </px:PXGrid>
</asp:content>