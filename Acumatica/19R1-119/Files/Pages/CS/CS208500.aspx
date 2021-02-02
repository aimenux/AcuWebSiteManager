<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS208500.aspx.cs" Inherits="Page_CS208500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="FOBPoint" TypeName="PX.Objects.CS.FOBPointMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="170px" Width="100%" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="FOBPoint">
				<Columns>
					<px:PXGridColumn DataField="FOBPointID" Width="100px" />
					<px:PXGridColumn DataField="Description" Width="250px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXSelector ID="edFOBPointID" runat="server" DataField="FOBPointID" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
