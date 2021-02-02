<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR101040.aspx.cs" Inherits="Page_PR101040" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.WorkLocationsMaint" PrimaryView="Locations">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="CopyPaste" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="ViewOnMap" Visible="False" />
			<px:PXDSCallbackCommand Name="AddressLookupSelectAction" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="AddressLookup" SelectControlsIDs="form" RepaintControls="None" RepaintControlsIDs="ds,PXFormView2" CommitChanges="true" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="200px" Style="z-index: 100" Width="100%" DataMember="Locations" TabIndex="9100">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="M" ColumnWidth="L" ControlSize="M" StartColumn="True" StartRow="True" />
			<px:PXSelector ID="edLocationCD" runat="server" DataField="LocationCD" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" Text="Is Active" />
			<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
			<px:PXLayoutRule runat="server" GroupCaption="Main Address" StartRow="True" LabelsWidth="M" ColumnWidth="600px" ControlSize="L" StartGroup="True" />
			<px:PXFormView ID="PXFormView2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Address" TabIndex="9100" RenderStyle="Simple">
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" StartRow="True" />
					<px:PXButton ID="btnAddressLookup" runat="server" CommandName="AddressLookup" CommandSourceID="ds" Size="xs" TabIndex="-1" />
					<px:PXButton ID="PXButton1" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Text="View on Map" Width="100px" Height="22" TabIndex="-1" />
					<px:PXTextEdit ID="edAddress1" runat="server" DataField="AddressLine1" CommitChanges="true" />
					<px:PXTextEdit ID="edAddress2" runat="server" DataField="AddressLine2" CommitChanges="true" />
					<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true" />
					<px:PXSelector CommitChanges="True" ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" DataSourceID="ds" />
					<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowEdit="True" DataSourceID="ds" CommitChanges="true" >
						<CallBackMode PostData="Container" />
					</px:PXSelector>
					<px:PXTextEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
				</Template>
			</px:PXFormView>
		</Template>
		<AutoSize Container="Window" Enabled="true" />
	</px:PXFormView>
	<!--#include file="~\Pages\Includes\AddressLookupPanel.inc"-->
</asp:Content>
