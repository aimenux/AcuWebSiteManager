<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS209020.aspx.cs" Inherits="Page_CS209020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="PX.Objects.CS.DaylightShiftMaint" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="true" PostData="page" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Previous" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Next" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Period">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="Year" ValueField="nbr" DataSourceID="ds" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Style="z-index: 100;" Width="100%" Caption="Time Zones" AdjustPageSize="Auto" AllowFilter="False" SkinID="Inquire" Height="300px">
		<Levels>
			<px:PXGridLevel DataMember="Calendar">
				<Columns>
					<px:PXGridColumn DataField="TimeZoneDescription" Width="250px" />
					<px:PXGridColumn AllowShowHide="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
					<px:PXGridColumn DataField="FromDate_Date" Width="120px" AutoCallBack="True" />
					<px:PXGridColumn DataField="FromDate_Time" Width="120px" AutoCallBack="True" TimeMode="True" />
					<px:PXGridColumn DataField="ToDate_Date" Width="120px" AutoCallBack="True" />
					<px:PXGridColumn DataField="ToDate_Time" Width="120px" AutoCallBack="True" TimeMode="True" />
					<px:PXGridColumn DataField="Shift" Width="80px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar PagerVisible="False">
			<Actions>
				<FilterShow Enabled="False" />
				<FilterSet Enabled="False" />
				<Save Enabled="False" />
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<EditRecord Enabled="False" />
			</Actions>
		</ActionBar>
		<AutoSize Enabled="True" Container="Window" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" />
	</px:PXGrid>
</asp:Content>
