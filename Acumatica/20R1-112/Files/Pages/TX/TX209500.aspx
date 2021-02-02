<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX209500.aspx.cs" Inherits="Page_TX209500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Data" TypeName="PX.Objects.TX.TaxImportZipDataMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="Data">
				<Columns>
					<px:PXGridColumn DataField="ZipCode" />
					<px:PXGridColumn DataField="StateCode" />
					<px:PXGridColumn DataField="CountyName" />
					<px:PXGridColumn DataField="Plus4PortionOfZipCode" TextAlign="Right" />
					<px:PXGridColumn DataField="Plus4PortionOfZipCode2" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
		</ActionBar>
	</px:PXGrid>
</asp:Content>
