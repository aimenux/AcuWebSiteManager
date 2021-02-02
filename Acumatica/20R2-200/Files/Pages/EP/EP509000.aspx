<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP509000.aspx.cs" Inherits="Page_EP509000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EquipmentTimeSheetRelease"
		PrimaryView="FilteredItems">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="viewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" Caption="Time Sheets" 
		SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems" >
				<Columns>
<%--
Warning: 	Invalid DataField 'EquipmentTimeSheetRelease::FilteredItems::DocDate' detected in Aspx: phF_grid_Levels#0_Columns#2
Warning: 	Invalid DataField 'EquipmentTimeSheetRelease::FilteredItems::EPApproval__ApproverID' detected in Aspx: phF_grid_Levels#0_Columns#4
Warning: 	Invalid DataField 'EquipmentTimeSheetRelease::FilteredItems::EPApproval__ApproveDate' detected in Aspx: phF_grid_Levels#0_Columns#5
--%>
					<px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="TimeSheetID" LinkCommand="viewDetails"  />
					<px:PXGridColumn DataField="Date" />
					<px:PXGridColumn AllowUpdate="False" DataField="EquipmentID" DisplayFormat="CCCCCCCCCC" />
					<px:PXGridColumn DataField="EPApproval__ApprovedByID" />
					<px:PXGridColumn DataField="EPApproval__ApproveDate" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Tooltip="Shows Time Sheet Details" Key="cmdViewDetails" Visible="false" >
				    <AutoCallBack Command="viewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
