<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS207035.aspx.cs" Inherits="Page_CS207035" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Location" TypeName="PX.Objects.CS.CompanyLocationMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="ViewOnMap" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Width="100%" Caption="Location Summary" DataMember="Location" DataSourceID="ds" NoteIndicator="True" FilesIndicator="True">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSegmentMask Size="m" ID="edLocationCD" runat="server" DataField="LocationCD" AutoRefresh="True" />
		    <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
		    <px:PXLayoutRule runat="server" Merge="False" />
			<px:PXTextEdit Size="l" ID="edDescr" runat="server" DataField="Descr" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="522px" Style="z-index: 100" Width="100%" DataMember="LocationCurrent">
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Info" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsContactSameAsMain" runat="server" DataField="IsContactSameAsMain" />
				    <px:PXLayoutRule runat="server" DataMember="Contact" />
					<px:PXTextEdit Size="l" ID="edFullName" runat="server" DataField="FullName" />
				    <px:PXTextEdit Size="l" ID="edSalutation" runat="server" DataField="Salutation" />
				    <px:PXMailEdit Size="l" ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
				    <px:PXLinkEdit Size="l" ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
				    <px:PXMaskEdit Size="m" ID="edPhone1" runat="server" DataField="Phone1" />
				    <px:PXMaskEdit Size="m" ID="edPhone2" runat="server" DataField="Phone2" />
				    <px:PXMaskEdit Size="m" ID="edFax" runat="server" DataField="Fax" />
				    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Address" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsAddressSameAsMain" runat="server" DataField="IsAddressSameAsMain" />
				    <px:PXLayoutRule runat="server" DataMember="Address" />
					<px:PXTextEdit Size="l" ID="edAddressLine1" runat="server" DataField="AddressLine1" />
				    <px:PXTextEdit Size="l" ID="edAddressLine2" runat="server" DataField="AddressLine2" />
				    <px:PXTextEdit Size="l" ID="edCity" runat="server" DataField="City" />
				    <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" />
				    <px:PXSelector ID="edState" runat="server" AutoRefresh="true" DataField="State" AllowEdit="True">
						<CallBackMode PostData="Container" />
						<Parameters>
							<px:PXControlParam ControlID="formA" Name="Address.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
						</Parameters>
					</px:PXSelector>
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" DataField="PostalCode" />
				    <px:PXButton Size="xs" ID="btnViewOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Text="View On Map" />
				    <px:PXLayoutRule runat="server" Merge="False" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Settings" />
					<px:PXTextEdit Size="l" ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
				    <px:PXSelector Size="xs" ID="edVTaxZoneID" runat="server" DataField="VTaxZoneID" AllowEdit="True" />
				    <px:PXSegmentMask ID="edCMPSiteID" runat="server" DataField="CMPSiteID" AllowEdit="True" />
				    <px:PXDropDown Size="m" ID="edCShipComplete" runat="server" AllowNull="False" DataField="CShipComplete" SelectedIndex="2" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" />
					<px:PXSegmentMask ID="edCMPSalesSubID" runat="server" DataField="CMPSalesSubID" />
				    <px:PXSegmentMask ID="edCMPExpenseSubID" runat="server" DataField="CMPExpenseSubID" />
				    <px:PXSegmentMask Size="s" ID="edCMPFreightSubID" runat="server" DataField="CMPFreightSubID" />
				    <px:PXSegmentMask Size="s" ID="edCMPDiscountSubID" runat="server" DataField="CMPDiscountSubID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Buildings">
				<Template>
					<px:PXGrid ID="gridBuildings" BorderWidth="0px" runat="server" Style="z-index: 100; height: 482px; left: 0px; top: 0px;" Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" DataSourceID="ds"
						SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="building">
								<Columns>
									<px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAA" Label="Location" Visible="False" />
								    <px:PXGridColumn DataField="BuildingCD" DisplayFormat="&gt;CCCCCCCCCC" Label="Building" />
								    <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" MinHeight="531" MinWidth="900" Enabled="True" />
	</px:PXTab>
</asp:Content>
