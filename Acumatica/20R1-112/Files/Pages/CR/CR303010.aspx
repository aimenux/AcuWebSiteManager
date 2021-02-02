<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR303010.aspx.cs" Inherits="Page_CR303010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Location"
		TypeName="PX.Objects.CR.LocationMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Delete" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ViewOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="Opportunities_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="Opportunities_Contact_ViewDetails" Visible="False"
				DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="Cases_ViewDetails" Visible="False" DependOnGrid="gridCases" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="True"
				CommitChanges="True" PopupVisible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Width="100%" Caption="Location Summary"
		DataMember="Location" DataSourceID="ds" NoteIndicator="True" FilesIndicator="True"
		LinkIndicator="true" NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
		DefaultControlID="edBAccountID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID">
           </px:PXSegmentMask>
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" AutoRefresh="True">
            </px:PXSegmentMask>
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
			<px:PXLayoutRule runat="server" Merge="False" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" /></Template>
		<Parameters>
			<px:PXControlParam ControlID="frmHeader" Name="Location.bAccountID" PropertyName="NewDataKey[&quot;BAccountID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="513px" Style="z-index: 100"
		Width="100%" DataMember="LocationCurrent">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"
						StartGroup="True" GroupCaption="Location Contact" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsContactSameAsMain" runat="server" DataField="IsContactSameAsMain" TabIndex="10"/>
					<px:PXFormView ID="Contact" runat="server" DataMember="Contact" RenderStyle="Simple" TabIndex="20"
						LabelsWidth="SM">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
                            <px:PXLabel ID="LPhone1" runat="server" Size="SM" />
                            <px:PXDropDown ID="Phone1Type" runat="server" DataField="Phone1Type" Size="S" SuppressLabel="True" CommitChanges="True" />
					        <px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" Width="134px" LabelID="LPhone1" />
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
                            <px:PXLabel ID="LPhone2" runat="server" Size="SM" />
                            <px:PXDropDown ID="Phone2Type" runat="server" DataField="Phone2Type" Size="S" SuppressLabel="True" CommitChanges="True"/>
					        <px:PXMaskEdit ID="Phone2" runat="server" DataField="Phone2" Width="134px" LabelID="LPhone2" />
                            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
                            <px:PXLabel ID="LFax" runat="server" Size="SM" />
                            <px:PXDropDown ID="FaxType" runat="server" DataField="FaxType" Size="S" SuppressLabel="True" CommitChanges="True" />
					        <px:PXMaskEdit ID="Fax" runat="server" DataField="Fax" Width="134px" LabelID="LFax"/>
							<px:PXLayoutRule ID="PXLayoutRule23" runat="server" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Address" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsAddressSameAsMain" TabIndex="30"
						runat="server" DataField="IsAddressSameAsMain" />
					<px:PXFormView ID="Address" runat="server" DataMember="Address" RenderStyle="Simple" TabIndex="40"
						LabelsWidth="SM">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" CommitChanges="True"
								LabelsWidth="SM" DisplayMode="Hint" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" LabelsWidth="SM" DisplayMode="Hint" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="True" />
							<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" TabIndex="-1"
								Text="View on Map" Height="20"/>
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Location Settings" />
					<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" TabIndex="50" />
					<px:PXSelector ID="edCTaxZoneID" runat="server" DataField="CTaxZoneID" TabIndex="60" />
					<px:PXDropDown ID="edCTaxCalcMode" runat="server" DataField="CTaxCalcMode" TabIndex="65" />
					<px:PXTextEdit ID="edCAvalaraExemptionNumber" runat="server" DataField="CAvalaraExemptionNumber" TabIndex="70" />
					<px:PXDropDown ID="edCAvalaraCustomerUsageType" runat="server" DataField="CAvalaraCustomerUsageType" TabIndex="80" />
					<px:PXSelector ID="edCBranchID" runat="server" DataField="CBranchID" TabIndex="90" />
					<px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID" TabIndex="100" />
					<px:PXSegmentMask CommitChanges="True" ID="edCDefProjectID" runat="server" DataField="CDefProjectID" TabIndex="130" />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Shipping Instructions" />
					<px:PXSegmentMask ID="edCSiteID" runat="server" DataField="CSiteID" TabIndex="140" />
					<px:PXSelector CommitChanges="True" ID="edCarrierID" runat="server" DataField="CCarrierID" TabIndex="150" />
					<px:PXSelector ID="edShipTermsID" runat="server" DataField="CShipTermsID" TabIndex="160" />
					<px:PXSelector ID="edShipZoneID" runat="server" DataField="CShipZoneID" TabIndex="170" />
					<px:PXSelector ID="edFOBPointID" runat="server" DataField="CFOBPointID" TabIndex="180" />
                    <px:PXCheckBox ID="chkResedential" runat="server" DataField="CResedential" TabIndex="190" />
                    <px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="CSaturdayDelivery" TabIndex="200" />
					<px:PXCheckBox ID="chkInsurance" runat="server" DataField="CInsurance" TabIndex="210" />
                    <px:PXCheckBox ID="chkGroundCollect" runat="server" DataField="CGroundCollect" TabIndex="220" />
					<px:PXDropDown ID="edCShipComplete" runat="server" DataField="CShipComplete" TabIndex="230" />
					<px:PXNumberEdit ID="edCOrderPriority" runat="server" DataField="COrderPriority" TabIndex="240"
						Size="XXS" />
					<px:PXNumberEdit ID="edLeadTime" runat="server" DataField="CLeadTime" Size="XXS" TabIndex="250" />
					<px:PXSelector ID="edCalendar" runat="server" DataField="CCalendarID" TabIndex="260" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsARAccountSameAsMain" runat="server"
						DataField="IsARAccountSameAsMain" />
					<px:PXFormView ID="ARAccountSubLocation" runat="server" DataMember="ARAccountSubLocation"
						LabelsWidth="SM" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSegmentMask CommitChanges="True" ID="edCARAccountID" runat="server" DataField="CARAccountID" />
							<px:PXSegmentMask ID="edCARSubID" runat="server" DataField="CARSubID" />
						</Template>
					</px:PXFormView>
					<px:PXSegmentMask ID="edCSalesAcctID" runat="server" DataField="CSalesAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edCSalesSubID" runat="server" DataField="CSalesSubID" />
					<px:PXSegmentMask ID="edCDiscountAcctID" runat="server" DataField="CDiscountAcctID"
						CommitChanges="True" />
					<px:PXSegmentMask ID="edCDiscountSubID" runat="server" DataField="CDiscountSubID" />
					<px:PXSegmentMask ID="edCFreightAcctID" runat="server" DataField="CFreightAcctID"
						CommitChanges="True" />
					<px:PXSegmentMask ID="edCFreightSubID" runat="server" DataField="CFreightSubID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Opportunities" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridOpportunities" runat="server" DataSourceID="ds" Height="423px"
						Width="100%" AllowSearch="True" ActionsPosition="Top" SkinID="Inquire">
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Levels>
							<px:PXGridLevel DataMember="Opportunities">
								<Columns>
									<px:PXGridColumn DataField="OpportunityID" LinkCommand="Opportunities_ViewDetails" />
									<px:PXGridColumn DataField="Subject" />
									<px:PXGridColumn DataField="StageID" />
									<px:PXGridColumn DataField="CROpportunityProbability__Probability" TextAlign="Right" />
									<px:PXGridColumn DataField="Status" RenderEditorText="True" />
									<px:PXGridColumn DataField="CuryProductsAmount" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryID" />
									<px:PXGridColumn DataField="CloseDate" />
									<px:PXGridColumn DataField="Contact__DisplayName" LinkCommand="Opportunities_Contact_ViewDetails" />
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar DefaultAction="cmdOpportunityDetails">
							<CustomItems>
								<px:PXToolBarButton Text="Opportunity Details" Key="cmdOpportunityDetails">
									<AutoCallBack Command="Opportunities_ViewDetails" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Cases" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridCases" runat="server" DataSourceID="ds" Height="423px" Width="100%"
						AllowSearch="True" SkinID="Inquire" AllowPaging="true" AdjustPageSize="Auto"
						BorderWidth="0px">
						<ActionBar DefaultAction="cmdViewCaseDetails">
							<CustomItems>
								<px:PXToolBarButton Text="Case Details" Key="cmdViewCaseDetails" Visible="false">
									<ActionBar GroupIndex="0" />
									<AutoCallBack Command="Cases_ViewDetails" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Cases">
								<Columns>
									<px:PXGridColumn DataField="CaseCD" LinkCommand="Cases_ViewDetails" />
									<px:PXGridColumn DataField="Subject" />
									<px:PXGridColumn DataField="CaseClassID" />
									<px:PXGridColumn DataField="Severity" RenderEditorText="True" />
									<px:PXGridColumn DataField="Status" RenderEditorText="True" />
									<px:PXGridColumn DataField="Resolution" RenderEditorText="True" />
									<px:PXGridColumn DataField="CreatedDateTime" />
									<px:PXGridColumn DataField="InitResponse" DisplayFormat="###:##:##" />
									<px:PXGridColumn DataField="TimeEstimated" DisplayFormat="###:##:##" />
									<px:PXGridColumn DataField="ResolutionDate" />
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" MinHeight="533" MinWidth="600" Enabled="True" />
	</px:PXTab>
</asp:Content>
