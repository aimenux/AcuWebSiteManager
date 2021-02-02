<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR303020.aspx.cs" Inherits="Page_AR303020"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Location"
		TypeName="PX.Objects.AR.CustomerLocationMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" CommitChanges="True" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ViewOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="AddressLookupSelectAction" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="AddressLookup" SelectControlsIDs="frmHeader" RepaintControls="None" RepaintControlsIDs="ds,Address" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="Opportunities_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="Opportunities_Contact_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="Cases_ViewDetails" Visible="False" DependOnGrid="gridCases" />
			<px:PXDSCallbackCommand Name="ValidateAddresses" StartNewGroup="True" Visible="True" CommitChanges="True" PopupVisible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Width="100%" Caption="Location Summary"
		DataMember="Location" DataSourceID="ds" NoteIndicator="True" FilesIndicator="True"
		LinkIndicator="true" BPEventsIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
		DefaultControlID="edBAccountID">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
				<px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="true" />
				<px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" AutoRefresh="True" />
				<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="frmHeader" Name="Location.bAccountID" PropertyName="NewDataKey[&quot;BAccountID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="513px" Style="z-index: 100" Width="100%" DataMember="LocationCurrent" MarkRequired="Dynamic">
		<Activity HighlightColor="" SelectedColor="" Width="" Height="" />
		<Items>

			<px:PXTabItem Text="General">
				<Template>

					<%-- column 1 --%>

					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" GroupCaption="Location Info" />
						<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" CommitChanges="True" TabIndex="100" />

					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Address" />
						<px:PXCheckBox ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" CommitChanges="True" />
						<px:PXFormView ID="Address" runat="server" DataMember="Address" RenderStyle="Simple" LabelsWidth="SM">
							<Template>
								<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
								<px:PXButton ID="btnAddressLookup" runat="server" CommandName="AddressLookup" CommandSourceID="ds" Size="xs" TabIndex="-1" />
								<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Size="xs" Text="View on Map" TabIndex="-1"/>
								<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
								<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
								<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
								<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" LabelsWidth="SM" DisplayMode="Hint" />
								<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="True" />
								<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" CommitChanges="True" LabelsWidth="SM" DisplayMode="Hint" />
								<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							</Template>
						</px:PXFormView>

					<%-- column 2 --%>

					<px:PXLayoutRule runat="server" StartColumn="True" />
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Additional Location Info" />
						<px:PXCheckBox ID="chkOverrideContact" runat="server" DataField="OverrideContact" CommitChanges="True" />
						<px:PXFormView ID="Contact" runat="server" DataMember="Contact" RenderStyle="Simple" LabelsWidth="SM">
							<Template>
								<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
								<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
								<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
								<px:PXLayoutRule runat="server" Merge="True"/>
								<px:PXDropDown ID="Phone1Type"	runat="server" DataField="Phone1Type" Size="S" SuppressLabel="True" TabIndex="-1" />
								<px:PXLabel ID="lblPhone1"		runat="server" Text=" " SuppressLabel="true" />
								<px:PXMaskEdit ID="PXMaskEdit1"	runat="server" DataField="Phone1" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule runat="server" Merge="True" />
								<px:PXDropDown ID="Phone2Type"	runat="server" DataField="Phone2Type" Size="S" SuppressLabel="True" TabIndex="-1" />
								<px:PXLabel ID="lblPhone2"		runat="server" Text=" " SuppressLabel="true" />
								<px:PXMaskEdit ID="PXMaskEdit2"	runat="server" DataField="Phone2" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule runat="server" Merge="True" />
								<px:PXDropDown ID="FaxType"		runat="server" DataField="FaxType" Size="S" SuppressLabel="True" TabIndex="-1" />
								<px:PXLabel ID="lblFax"			runat="server" Text=" " SuppressLabel="true" />
								<px:PXMaskEdit ID="PXMaskEdit3"	runat="server" DataField="Fax" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule runat="server" />
								<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
								<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
							</Template>
						</px:PXFormView>
					
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Other Settings" />
						<px:PXSelector ID="edCBranchID" runat="server" DataField="CBranchID" AutoRefresh="True"/>
						<px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID" />
						<px:PXSegmentMask ID="edCDefProjectID" runat="server" DataField="CDefProjectID" CommitChanges="True" />
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Shipping">
				<Template>

					<%-- column 1 --%>

					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Tax Settings" />
						<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
						<px:PXSelector ID="edCTaxZoneID" runat="server" DataField="CTaxZoneID" />
						<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="CTaxCalcMode" />
						<px:PXTextEdit ID="edCAvalaraExemptionNumber" runat="server" DataField="CAvalaraExemptionNumber" />
						<px:PXDropDown ID="edCAvalaraCustomerUsageType" runat="server" DataField="CAvalaraCustomerUsageType" />

					<%-- column 2 --%>

					<px:PXLayoutRule runat="server" StartColumn="True" />
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" GroupCaption="Shipping Instructions" />
						<px:PXSegmentMask ID="edCSiteID" runat="server" DataField="CSiteID" />
						<px:PXSelector CommitChanges="True" ID="edCarrierID" runat="server" DataField="CCarrierID" />
						<px:PXSelector ID="edShipTermsID" runat="server" DataField="CShipTermsID" />
						<px:PXSelector ID="edShipZoneID" runat="server" DataField="CShipZoneID" />
						<px:PXSelector ID="edFOBPointID" runat="server" DataField="CFOBPointID" />
						<px:PXCheckBox ID="chkResedential" runat="server" DataField="CResedential" />
						<px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="CSaturdayDelivery" />
						<px:PXCheckBox ID="chkInsurance" runat="server" DataField="CInsurance" />
						<px:PXCheckBox ID="chkGroundCollect" runat="server" DataField="CGroundCollect" />
						<px:PXDropDown ID="edCShipComplete" runat="server" DataField="CShipComplete" />
						<px:PXNumberEdit ID="edCOrderPriority" runat="server" DataField="COrderPriority" Size="XXS" />
						<px:PXNumberEdit ID="edLeadTime" runat="server" DataField="CLeadTime" Size="XXS" />
						<px:PXSelector ID="edCalendar" runat="server" DataField="CCalendarID" />

				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsARAccountSameAsMain" runat="server" DataField="IsARAccountSameAsMain" />
					<px:PXFormView ID="ARAccountSubLocation" runat="server" DataMember="ARAccountSubLocation" LabelsWidth="M" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXSegmentMask CommitChanges="True" ID="edCARAccountID" runat="server" DataField="CARAccountID" />
							<px:PXSegmentMask ID="edCARSubID" runat="server" DataField="CARSubID" />
						</Template>
					</px:PXFormView>
					<px:PXSegmentMask ID="edCSalesAcctID" runat="server" DataField="CSalesAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edCSalesSubID" runat="server" DataField="CSalesSubID" />
					<px:PXSegmentMask ID="edCDiscountAcctID" runat="server" DataField="CDiscountAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edCDiscountSubID" runat="server" DataField="CDiscountSubID" />
					<px:PXSegmentMask ID="edCFreightAcctID" runat="server" DataField="CFreightAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edCFreightSubID" runat="server" DataField="CFreightSubID" />
					<px:PXFormView ID="ARAccountSubLocation1" runat="server" DataMember="ARAccountSubLocation" LabelsWidth="SM" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="CRetainageAcctID" CommitChanges="True" />
							<px:PXSegmentMask ID="edRetainageSubID" runat="server" DataField="CRetainageSubID" CommitChanges="True" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>

		</Items>
		<AutoSize Container="Window" MinHeight="533" MinWidth="600" Enabled="True" />
	</px:PXTab>

	<!--#include file="~\Pages\Includes\AddressLookupPanel.inc"-->

</asp:Content>
