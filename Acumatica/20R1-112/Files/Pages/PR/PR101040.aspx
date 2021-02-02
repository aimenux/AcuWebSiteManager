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
			<px:PXDSCallbackCommand Name="ViewTaxList" Visible="False" />
			<px:PXDSCallbackCommand Name="GetTaxCodes" Visible="False" />
			<px:PXDSCallbackCommand Name="AddTaxCode" Visible="False" />
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
					<px:PXTextEdit ID="edAddress1" runat="server" DataField="AddressLine1" CommitChanges="true" />
					<px:PXTextEdit ID="edAddress2" runat="server" DataField="AddressLine2" CommitChanges="true" />
					<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true" />
					<px:PXSelector CommitChanges="True" ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" DataSourceID="ds" />
					<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowEdit="True" DataSourceID="ds" CommitChanges="true" >
						<CallBackMode PostData="Container" />
					</px:PXSelector>
					<px:PXLayoutRule runat="server" LabelsWidth="M" StartRow="True" Merge="true" ControlSize="M" />
					<px:PXTextEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
					<px:PXButton ID="PXButton1" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Text="View on Map" Width="100px" Height="22" />

					<px:PXLayoutRule runat="server" GroupCaption="Tax Location Info" LabelsWidth="M" ControlSize="M" StartRow="True" />
					<px:PXButton runat="server" TabIndex="100" ID="btnGetTaxEngineCodes" CommandName="GetTaxCodes" CommandSourceID="ds" />
					<px:PXTextEdit ID="edPrTaxLocationCode" runat="server" DataField="TaxLocationCode" CommitChanges="true" />
					<px:PXTextEdit ID="edPrTaxMunicipalCode" runat="server" DataField="TaxMunicipalCode" />
					<px:PXTextEdit ID="edPrTaxSchoolCode" runat="server" DataField="TaxSchoolCode" />
					<px:PXButton ID="btnViewTaxList" runat="server" TabIndex="140" CommandName="ViewTaxList" CommandSourceID="ds" Text="View Tax List" />
				</Template>
			</px:PXFormView>
		</Template>
		<AutoSize Container="Window" Enabled="true" />
	</px:PXFormView>
	<px:PXSmartPanel runat="server" ID="pnlTaxList" Caption="Possible Taxes for this Location" CaptionVisible="true" LoadOnDemand="true"
		Key="TaxesList" Width="1430px" Height="300px" AutoCallBack-Enabled="true" AutoCallBack-Target="taxListGrid" AutoCallBack-Command="Refresh">
		<px:PXGrid ID="taxListGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" SyncPosition="True">
			<Levels>
				<px:PXGridLevel DataMember="TaxesList">
					<Columns>
						<px:PXGridColumn DataField="State" Width="50px" />
						<px:PXGridColumn DataField="TaxLocationCode" Width="100px" />
						<px:PXGridColumn DataField="TaxTypeCode" Width="100px" />
						<px:PXGridColumn DataField="TypeDescription" Width="200px" />
						<px:PXGridColumn DataField="TaxDescription" Width="300px" />
						<px:PXGridColumn DataField="TaxJurisdiction" Width="100px" />
						<px:PXGridColumn DataField="TaxCategory" Width="150px" />
						<px:PXGridColumn DataField="TaxUniqueCode" Width="250px" />
						<px:PXGridColumn DataField="TaxCD" Width="100px" />
						<px:PXGridColumn DataField="TaxName" Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<ActionBar>
				<CustomItems>
					<px:PXToolBarButton CommandName="AddTaxCode" CommandSourceID="ds" Text="Add Tax Code" DependOnGrid="taxListGrid" />
				</CustomItems>
			</ActionBar>
			<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
			<Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
		</px:PXGrid>
	</px:PXSmartPanel>
</asp:Content>
