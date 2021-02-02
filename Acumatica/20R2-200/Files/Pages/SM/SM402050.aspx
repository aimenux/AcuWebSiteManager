<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM402050.aspx.cs" Inherits="Page_SM402050" Title="Business Events" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" Runat="server" TypeName="PX.BusinessProcess.UI.BusinessProcessEventInq" PrimaryView="Events" Visible="True" Width="100%">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="createBusinessEvent" Visible="True" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="viewBusinessEvent" Visible="False" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:content ID="cont2" ContentPlaceHolderID="phL" Runat="Server">
	<px:PXGrid ID="grid" Runat="server" SkinID="Primary" Height="350" Width="100%" AdjustPageSize="Auto" AutoAdjustColumns="True"
        AllowPaging="True" AllowSearch="True">
        <Levels>
            <px:PXGridLevel DataMember="Events">
                <Columns>
                    <px:PXGridColumn DataField="Type" Width="200" />
					<px:PXGridColumn DataField="Name" Width="450" LinkCommand="ViewBusinessEvent" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar>
            <Actions>
                <AddNew ToolBarVisible="False" MenuVisible="False" />
                <Delete ToolBarVisible="False" MenuVisible="False" />
                <ExportExcel ToolBarVisible="False" MenuVisible="False" />
            </Actions>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        <AutoSize Container="Window" Enabled="True" MinHeight="250" />
    </px:PXGrid>
    <px:PXSmartPanel ID="pnlCreateBusinessEvent" runat="server" Key="NewEventData" Caption="Create Business Event" CaptionVisible="True"
        AutoReload="True" AutoRepaint="True" Width="500" Height="100">
	    <px:PXFormView ID="frmCreateBusinessEvent" runat="server" DataMember="NewEventData" DataSourceID="ds" Width="500" Height="100" SkinID="Transparent">
		    <Template>
			    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
			    <px:PXTextEdit runat="server" ID="txtName" DataField="Name" CommitChanges="True" />
		    </Template>
            <AutoSize Enabled="True" />
	    </px:PXFormView>
		<px:PXPanel ID="pnlButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:content>
