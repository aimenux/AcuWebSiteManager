<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP303000.aspx.cs" Inherits="Page_TabView" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" EnableAttributes="true" Visible="True" Width="100%" TypeName="PX.Objects.AP.VendorMaint" PrimaryView="BAccount">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="First" />
			<px:PXDSCallbackCommand DependOnGrid="grdContacts" Name="ViewContact" Visible="False" />
			<px:PXDSCallbackCommand Name="NewContact" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="ViewLocation" Visible="False" />
			<px:PXDSCallbackCommand Name="NewLocation" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefault" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewMainOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDefLocationOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewRestrictionGroups" Visible="False" />
			<px:PXDSCallbackCommand Name="ExtendToCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewBusnessAccount" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewBalanceDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewRemitOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="NewBillAdjustment" Visible="False" />
			<px:PXDSCallbackCommand Name="NewManualCheck" Visible="False" />
			<px:PXDSCallbackCommand Name="VendorDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="ApproveBillsForPayments" Visible="False" />
			<px:PXDSCallbackCommand Name="PayBills" Visible="False" />
			<px:PXDSCallbackCommand Name="BalanceByVendor" Visible="False" />
			<px:PXDSCallbackCommand Name="VendorHistory" Visible="False" />
			<px:PXDSCallbackCommand Name="APAgedPastDue" Visible="False" />
			<px:PXDSCallbackCommand Name="APAgedOutstanding" Visible="False" />
			<px:PXDSCallbackCommand Name="APDocumentRegister" Visible="False" />
			<px:PXDSCallbackCommand Name="RepVendorDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="VendorPrice" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" DependOnGrid="grdContacts" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlChangeID" runat="server"  Caption="Specify New ID"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="btnOK">
			<px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
				DataMember="ChangeIDDialog">
				<ContentStyle BackColor="Transparent" BorderStyle="None" />
				<Template>
					<px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
				</Template>
			</px:PXFormView>
			<px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
					<AutoCallBack Target="formChangeID" Command="Save" />
				</px:PXButton>
				<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />						
			</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="BAccount" runat="server" Width="100%" Caption="Vendor Summary" DataMember="BAccount" NoteIndicator="True" FilesIndicator="True" NotifyIndicator="True" ActivityIndicator="False" LinkIndicator="True"
		DefaultControlID="edAcctCD" DataSourceID="ds">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" DisplayMode="Value" DataSourceID="ds" FilterByAllFields="True" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit CommitChanges="True" ID="edAcctName" runat="server" DataField="AcctName" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXFormView ID="VendorBalance" runat="server" DataMember="VendorBalance" DataSourceID="ds" RenderStyle="Simple">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
					<px:PXNumberEdit ID="edBalance" runat="server" DataField="Balance" Enabled="False">
					</px:PXNumberEdit>
					<px:PXNumberEdit ID="edDepositsBalance" runat="server" DataField="DepositsBalance" Enabled="False">
					</px:PXNumberEdit>
					<px:PXNumberEdit ID="edRetainageBalance" runat="server" DataField="RetainageBalance" />
				</Template>
			</px:PXFormView>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="494px" Style="z-index: 100" Width="100%" DataMember="CurrentVendor" MarkRequired="Dynamic" >
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Info" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="DefContact" runat="server" Caption="Main Contact" DataMember="DefContact" RenderStyle="Fieldset" TabIndex="1100">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True" />
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
						</Template>
					</px:PXFormView>
					<px:PXTextEdit ID="edAcctReferenceNbr" runat="server" DataField="AcctReferenceNbr" />
					<px:PXSegmentMask ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" AllowEdit="True" />
					<px:PXSelector runat="server" ID="edLocale" DataField="BAccount.LocaleName" DisplayMode="Text" />
					<px:PXFormView ID="DefAddress" runat="server" Caption="Main Address" DataMember="DefAddress" SyncPosition="true" RenderStyle="Fieldset" TabIndex="1160">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="True" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="True" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="True" />
							<px:PXSelector ID="edCountryID" runat="server" CommitChanges="True" AllowAddNew="True" DataField="CountryID" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" AllowAddNew="True" CommitChanges="True" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" Size="s" />
							<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds" Text="View on Map" />
							<px:PXLayoutRule runat="server" />
                            <px:PXTextEdit runat="server" DataField="County" ID="edCounty" />
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXFormView>
                    <px:PXFormView runat="server" ID="VendorDefaults" Caption="Vendor Defaults" DataMember="CurrentVendor" TabIndex="1161" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ID="PXLayoutRule17" StartColumn="True" LabelsWidth="SM" />
                            <px:PXSelector runat="server" ID="edVendorDefaultCostCodeId" DataField="VendorDefaultCostCodeId" Size="XM" />
                            <px:PXSelector runat="server" ID="edVendorDefaultInventoryId" DataField="VendorDefaultInventoryId" Size="XM" />
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" ID="PXLayoutRule18" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" StartGroup="True" GroupCaption="Financial Settings" />
					<px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" AllowEdit="True" />
					<px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Size="S" />
					<px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
					<px:PXSelector ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" Size="S" />
					<px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" GroupCaption="Vendor Properties" />
					<px:PXCheckBox ID="chkLandedCostVendor" runat="server"  AllowEdit="True" DataField="LandedCostVendor" CommitChanges="True"/>
					<px:PXCheckBox CommitChanges="True" ID="chkTaxAgency" runat="server" DataField="TaxAgency" />
					<px:PXCheckBox ID="chkLaborUnion" runat="server"  AllowEdit="True" DataField="IsLaborUnion" CommitChanges="True"/>
					<px:PXCheckBox ID="chkVendor1099" runat="server" DataField="Vendor1099" CommitChanges="True" />
					<px:PXSelector ID="edBox1099" runat="server" DataField="Box1099" />
					<px:PXCheckBox runat="server" ID="edForeignEntity" DataField="ForeignEntity" Text="ForeignEntity" />
					<px:PXCheckBox ID="chkFATCA" runat="server" DataField="FATCA" />					
                    <px:PXCheckBox runat="server" ID="edSDEnabled" DataField="SDEnabled" CommitChanges="True" />

					<px:PXLayoutRule ID="PXLayoutRule16" runat="server" GroupCaption="Personal Data Privacy" StartGroup="True"/>
					<px:PXFormView ID="edContactGDPR" runat="server" DataMember="DefContact" DataSourceID="ds" RenderStyle="Simple" SkinID="Transparent" TabIndex="2750">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule16" runat="server" ControlSize="M" LabelsWidth="SM"/>
							<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="ConsentAgreement" AlignLeft="True" CommitChanges="True"/>
							<px:PXDateTimeEdit ID="edConsentDate" runat="server" DataField="ConsentDate" CommitChanges="True"/>
							<px:PXDateTimeEdit ID="edConsentExpirationDate" runat="server" DataField="ConsentExpirationDate" CommitChanges="True"/>
						</Template>
						<ContentLayout OuterSpacing="None"/>
						<ContentStyle BackColor="Transparent" BorderStyle="None"/>
					</px:PXFormView>

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Retainage Settings" />
					<px:PXCheckBox runat="server" ID="edRetainageApply" DataField="RetainageApply" CommitChanges="True" />
					<px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" ID="PXLayoutRule19" StartGroup="True" GroupCaption="Lien Waiver Settings" />
					<px:PXCheckBox runat="server" ID="chkShouldGenerateLienWaivers" DataField="ShouldGenerateLienWaivers" AlignLeft="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Payment Settings">
				<Template>
					<px:PXFormView ID="DefLocationPayment" runat="server" DataMember="DefLocation" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" StartGroup="True" GroupCaption="Remittance Contact" />
							<px:PXCheckBox CommitChanges="True" ID="chkIsRemitContactSameAsMain" runat="server" DataField="IsRemitContactSameAsMain" />
							<px:PXFormView ID="RemitContact" runat="server" DataMember="RemitContact" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                                    <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                                    <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                                    <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                                    <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
                                    <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
                                    <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True" />
									<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" GroupCaption="Remittance Address" StartGroup="True" />
							<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsRemitAddressSameAsMain" runat="server" DataField="IsRemitAddressSameAsMain" />
							<px:PXFormView ID="RemitAddress" runat="server" DataMember="RemitAddress" SyncPosition="true" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
									<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
									<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="True" />
									<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="True" />
									<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="True" />
									<px:PXSelector ID="edCountryID" runat="server" AllowAddNew="True" AutoRefresh="True" CommitChanges="True" DataField="CountryID" />
									<px:PXSelector ID="edState" runat="server" AllowAddNew="True" AutoRefresh="True" DataField="State" CommitChanges="True" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" Size="s" />
									<px:PXButton ID="btnViewRemitOnMap" runat="server" CommandName="ViewRemitOnMap" CommandSourceID="ds" Size="xs" Text="View on Map" />
									<px:PXLayoutRule runat="server" />
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Default Payment Settings" LabelsWidth="SM" StartColumn="True" StartGroup="True" />
							<px:PXSelector CommitChanges="True" ID="edVPaymentMethodID" runat="server" DataField="VPaymentMethodID" AutoRefresh="True"   AllowAddNew="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edVCashAccountID" runat="server" DataField="VCashAccountID" AllowAddNew="True" AutoRefresh="True" />
							<px:PXDropDown ID="edVPaymentByType" runat="server" DataField="VPaymentByType" />
							<px:PXNumberEdit ID="edVPaymentLeadTime" runat="server" DataField="VPaymentLeadTime" />
							<px:PXSegmentMask CommitChanges="True" ID="edPayToVendorID" runat="server" DataField="CurrentVendor.PayToVendorID" AllowEdit="True" AutoRefresh="True" />
							<px:PXCheckBox ID="chkVSeparateCheck" runat="server" DataField="VSeparateCheck" />
							<px:PXCheckBox ID="chkPaymentsByLinesAllowed" runat="server" DataField="CurrentVendor.PaymentsByLinesAllowed" />
							<px:PXNumberEdit ID="edVPrepaymentPct" runat="server" DataField="VPrepaymentPct" />
							<px:PXGrid ID="grdPaymentDetails" runat="server" Caption="Payment Instructions" SkinID="Attributes" MatrixMode="True" Height="160px" Width="400px">
								<Levels>
									<px:PXGridLevel DataMember="PaymentDetails" DataKeyNames="BAccountID,LocationID,PaymentMethodID,DetailID">
										<Columns>
											<px:PXGridColumn DataField="PaymentMethodDetail__descr" />
											<px:PXGridColumn DataField="DetailValue" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Purchase Settings" LoadOnDemand="True">
				<Template>
					<px:PXFormView ID="DefLocation" runat="server" CaptionVisible="False" DataMember="DefLocation" Width="100%" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartGroup="True" GroupCaption="Shipper's Contact" ControlSize="XM" LabelsWidth="SM" />
							<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsContactSameAsMain" runat="server" DataField="IsContactSameAsMain" />
							<px:PXFormView ID="DefLocationContact" runat="server" DataMember="DefLocationContact" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule10" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
									<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
									<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
									<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
									<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
									<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
									<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True" />
									<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule ID="PXLayoutRule11" runat="server" GroupCaption="Shipper's Address" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
							<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsMain" runat="server" DataField="IsAddressSameAsMain" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="True" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="True" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="True" />
							<px:PXSelector ID="edCountryID" runat="server" CommitChanges="True" DataField="CountryID" AllowAddNew="True" />
							<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowAddNew="True" CommitChanges="True" />
							<px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" />
							<px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
							<px:PXButton Size="xs" ID="btnViewDefLoactionOnMap" runat="server" CommandName="ViewDefLocationOnMap" CommandSourceID="ds" Text="View on Map" />
							<px:PXLayoutRule ID="PXLayoutRule13" runat="server" />
							<px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Default Location Settings" />
							<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
							<px:PXSelector ID="edVBranchID" runat="server" DataField="VBranchID" AllowEdit="True" />
							<px:PXSelector ID="edTaxZoneID" runat="server" DataField="VTaxZoneID" AllowEdit="True" />
							<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="VTaxCalcMode" />
							<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
							<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
							<px:PXCheckBox ID="chkVPrintOrder" runat="server" DataField="VPrintOrder" />
							<px:PXCheckBox ID="chkVEmailOrder" runat="server" DataField="VEmailOrder" />
							<px:PXLayoutRule ID="PXLayoutRule15" runat="server" />
							<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartGroup="True" GroupCaption="Shipping Instructions" />
							<px:PXSegmentMask ID="edVSiteID" runat="server" DataField="VSiteID" AllowEdit="True" CommitChanges="True" AutoRefresh="True" />
							<px:PXSelector ID="edShipTermsID" runat="server" DataField="VShipTermsID" AllowEdit="True" />
							<px:PXSelector ID="edVCarrierID" runat="server" DataField="VCarrierID" AllowEdit="True" />
							<px:PXSelector ID="edFOBPointID" runat="server" DataField="VFOBPointID" AllowEdit="True" />
							<px:PXNumberEdit ID="edLeadTime" runat="server" DataField="VLeadTime" />
							<px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartGroup="True" GroupCaption="Receipt Actions" />
							<px:PXCheckBox ID="chkVAllowAPBillBeforeReceipt" runat="server" DataField="VAllowAPBillBeforeReceipt" />
							<px:PXNumberEdit ID="edVRcptQtyMin" runat="server" DataField="VRcptQtyMin" />
							<px:PXNumberEdit ID="edVRcptQtyMax" runat="server" DataField="VRcptQtyMax" />
							<px:PXNumberEdit ID="edVRcptQtyThreshold" runat="server" DataField="VRcptQtyThreshold" />
							<px:PXDropDown ID="edVRcptQtyAction" runat="server" DataField="VRcptQtyAction" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Locations" LoadOnDemand="False">
				<Template>
					<px:PXGrid ID="grdLocations" runat="server" AllowSearch="True" Height="99%" SkinID="DetailsInTab" Style="z-index: 100;" Width="100%">
						<LevelStyles>
							<RowForm Height="400px" Width="800px">
							</RowForm>
						</LevelStyles>
						<Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="New Location">
									<AutoCallBack Command="NewLocation" Target="ds" />
									<PopupCommand Command="Refresh" Target="grdLocations" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewLocation" Text="Location Details">
									<AutoCallBack Command="ViewLocation" Target="ds" />
									<PopupCommand Command="Refresh" Target="grdLocations" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Set as Default">
									<AutoCallBack Command="SetDefault" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Locations">
								<RowTemplate>
									<px:PXSelector ID="edLocationCD" runat="server" DataField="LocationCD" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowShowHide="False" DataField="LocationBAccountID" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="LocationID" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="LocationCD" />
									<px:PXGridColumn DataField="Descr" TextCase="Upper" />
									<px:PXGridColumn DataField="City" />
									<px:PXGridColumn AutoCallBack="True" DataField="CountryID" RenderEditorText="True" />
									<px:PXGridColumn DataField="State" RenderEditorText="True" />
									<px:PXGridColumn DataField="VTaxZoneID" />
									<px:PXGridColumn DataField="VExpenseAcctID" AutoCallBack="True" />
									<px:PXGridColumn DataField="VExpenseSubID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowUpdate="False" AllowAddNew="False" AllowDelete="False"></Mode>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Contacts" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdContacts" runat="server" Height="100%" Width="100%" AllowSearch="True" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="ExtContacts">
								<RowTemplate>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ContactBAccountID" TextAlign="Right" Visible="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="ContactID" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Salutation" />
									<px:PXGridColumn DataField="ContactDisplayName" LinkCommand="ViewContact" />
									<px:PXGridColumn DataField="City" TextCase="Upper" />
									<px:PXGridColumn DataField="EMail" />
									<px:PXGridColumn DataField="Phone1" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="false" />
						<ActionBar DefaultAction="grdContacts">
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="New Contact">
									<AutoCallBack Command="NewContact" Target="ds" />
									<PopupCommand Command="Refresh" Target="grdContacts" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Contact Details">
									<AutoCallBack Command="ViewContact" Target="ds" />
									<PopupCommand Command="Refresh" Target="grdContacts" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
						Height="200px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Answers">
								<Columns>
									<px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
										TextField="AttributeID_description" />
									<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
							</Actions>
						</ActionBar>
						<Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			 <px:PXTabItem Text="Activities" LoadOnDemand="True">
				<Template>
					<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
						AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
						FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
						PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
						BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
						<ActionBar DefaultAction="cmdViewActivity" CustomItemsGroup="0" PagerVisible="False">
							<CustomItems>
								<px:PXToolBarButton Key="cmdAddTask">
									<AutoCallBack Command="NewTask" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddEvent">
									<AutoCallBack Command="NewEvent" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddEmail">
									<AutoCallBack Command="NewMailActivity" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddActivity">
									<AutoCallBack Command="NewActivity" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Activities">
								<Columns>
									<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
									<px:PXGridColumn DataField="UIStatus" />
									<px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
									<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
									<px:PXGridColumn DataField="TimeSpent" />
									<px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
										SyncVisible="False" SyncVisibility="False" Width="108px">
										<NavigateParams>
											<px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text"/>
									<px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false"/>
									<px:PXGridColumn DataField="Source" AllowResize="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<PreviewPanelTemplate>
							<px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine"
								MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
									  <AutoSize Container="Parent" Enabled="true" />
								</px:PXHtmlView>
						</PreviewPanelTemplate>
						<AutoSize Enabled="true" />
						<GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
					</pxa:PXGridWithPreview>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts" LoadOnDemand="False">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="DefLocationGLAccounts" runat="server" DataMember="DefLocation" RenderStyle="Simple" TabIndex="2100" MarkRequired="Dynamic">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXSegmentMask ID="edVAPAccountID" runat="server" CommitChanges="True" DataField="VAPAccountID" />
							<px:PXSegmentMask ID="edVAPSubID" runat="server" DataField="VAPSubID" />
							<px:PXSegmentMask ID="edVExpenseAcctID" runat="server" CommitChanges="True" DataField="VExpenseAcctID" />
							<px:PXSegmentMask ID="edVExpenseSubID" runat="server" DataField="VExpenseSubID" />
							<px:PXSegmentMask ID="edVDiscountAcctID" runat="server" CommitChanges="True" DataField="VDiscountAcctID" />
							<px:PXSegmentMask ID="edVDiscountSubID" runat="server" DataField="VDiscountSubID" />
						</Template>
					</px:PXFormView>
					<px:PXSegmentMask CommitChanges="True" ID="edDiscTakenAcctID" runat="server" DataField="DiscTakenAcctID" />
					<px:PXSegmentMask ID="edDiscTakenSubID" runat="server" DataField="DiscTakenSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID" />
					<px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" />
					<px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPrebookAcctID" runat="server" DataField="PrebookAcctID" />
					<px:PXSegmentMask ID="edPrebookSubID" runat="server" DataField="PrebookSubID" />
					<px:PXFormView ID="formRetainage" runat="server" DataMember="DefLocation" RenderStyle="Simple" >
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXSegmentMask ID="edVRetainageAcctID" runat="server" CommitChanges="True" DataField="VRetainageAcctID" />
							<px:PXSegmentMask ID="edVRetainageSubID" runat="server" DataField="VRetainageSubID" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Agency Settings" BindingContext="tab" VisibleExp="DataControls[&quot;chkTaxAgency&quot;].Value = 1">
				<Template>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Tax Accounts" 
						StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXSegmentMask CommitChanges="True" ID="edSalesTaxAcctID" runat="server" DataField="SalesTaxAcctID" />
					<px:PXSegmentMask CommitChanges="True" ID="edSalesTaxSubID" runat="server" DataField="SalesTaxSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPurchTaxAcctID" runat="server" DataField="PurchTaxAcctID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPurchTaxSubID" runat="server" DataField="PurchTaxSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edTaxExpenseAcctID" runat="server" DataField="TaxExpenseAcctID" />
					<px:PXSegmentMask CommitChanges="True" ID="edTaxExpenseSubID" runat="server" DataField="TaxExpenseSubID" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Pending VAT Settings" LabelsWidth="M" ControlSize="XM" />
					<px:PXDropDown ID="edSVATReversalMethod" runat="server" DataField="SVATReversalMethod" SelectedIndex="-1" CommitChanges="True" />
					<px:PXDropDown ID="edSVATInputTaxEntryRefNbr" runat="server" DataField="SVATInputTaxEntryRefNbr" SelectedIndex="-1" CommitChanges="True" />
					<px:PXDropDown ID="edSVATOutputTaxEntryRefNbr" runat="server" DataField="SVATOutputTaxEntryRefNbr" SelectedIndex="-1" CommitChanges="True" />
					<px:PXSelector ID="edSVATTaxInvoiceNumberingID" runat="server" DataField="SVATTaxInvoiceNumberingID" CommitChanges="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Tax Report Settings" 
						StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXDropDown ID="edTaxPeriodType" runat="server" DataField="TaxPeriodType" CommitChanges="true" />
					<px:PXCheckBox ID="chktaxReportFinPeriod" runat="server" DataField="TaxReportFinPeriod" />
					<px:PXCheckBox ID="chkUpdClosedTaxPeriods" runat="server" DataField="UpdClosedTaxPeriods" />
					<px:PXCheckBox ID="chkAutoGenerateTaxBill" runat="server" DataField="AutoGenerateTaxBill" />
					<px:PXDropDown ID="edTaxReportRounding" runat="server" DataField="TaxReportRounding" />					
					<px:PXLayoutRule runat="server" Merge="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXNumberEdit ID="edTaxReportPrecision" runat="server" DataField="TaxReportPrecision" Size="xxs" />
					<px:PXCheckBox ID="chkTaxUseVendorCurPrecision" runat="server" DataField="TaxUseVendorCurPrecision" CommitChanges="true" />
					<px:PXLayoutRule runat="server" Merge="False" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Mailing Settings">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="494px">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px" Caption="Mailings" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds">
								<AutoSize Enabled="True" />
								<AutoCallBack Target="gridNR" Command="Refresh" />
								<Levels>
									<px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
										<RowTemplate>
											<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXDropDown ID="edFormat" runat="server" DataField="Format" />
											<px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID" />
											<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
											<px:PXSelector ID="edSetupID" runat="server" DataField="SetupID" />
											<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
											<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
											<px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="SetupID" AutoCallBack="True" />
											<px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch" />
											<px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text" />
											<px:PXGridColumn DataField="ReportID" AutoCallBack="True" />
											<px:PXGridColumn DataField="NotificationID" AutoCallBack="True" />
											<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
											<px:PXGridColumn DataField="OverrideSource" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds">
								<AutoSize Enabled="True" />
								<Mode InitNewRow="True"></Mode>
								<Parameters>
									<px:PXSyncGridParam ControlID="gridNS" />
								</Parameters>
								<CallbackCommands>
									<Save RepaintControls="None" RepaintControlsIDs="ds" />
									<FetchRow RepaintControls="None" />
								</CallbackCommands>
								<Levels>
									<px:PXGridLevel DataMember="NotificationRecipients" DataKeyNames="NotificationID">
										<Mode InitNewRow="True"></Mode>
										<Columns>
											<px:PXGridColumn DataField="ContactType" RenderEditorText="True" AutoCallBack="True" />
											<px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
											<px:PXGridColumn DataField="ContactID" Width="200px">
												<NavigateParams>
													<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
												</NavigateParams>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Email" />
											<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
											<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" />
										</Columns>
										<RowTemplate>
											<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXDropDown ID="edContactType" runat="server" DataField="ContactType" />
											<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName" AllowEdit="True" />
										</RowTemplate>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Supplied-by Vendors" RepaintOnDemand="False">
				<Template>
					<px:PXGrid ID="PXGridSuppliedByVendors" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px">
						<Levels>
							<px:PXGridLevel DataMember="SuppliedByVendors">
								<Columns>
									<px:PXGridColumn DataField="AcctCD" LinkCommand="viewDetails" />
									<px:PXGridColumn DataField="AcctName" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" AutoGenerateColumns="Append" DataSourceID="ds" AllowPaging="True" PageSize="12">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
                                <Columns>
                                    <px:PXGridColumn DataField="ExpirationDate" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CreationDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn Type="CheckBox" DataField="Required" TextAlign="Center" />
                                    <px:PXGridColumn Type="CheckBox" DataField="Received" TextAlign="Center" />
                                    <px:PXGridColumn DataField="ReceivedDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="IsProcessed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsVoided" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsCreatedAutomatically" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="SentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeID" TextAlign="Left" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="VendorID" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillID" LinkCommand="ComplianceDocument$BillID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ApCheckID" LinkCommand="ComplianceDocument$ApCheckID$Link" CommitChanges="True" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentID" LinkCommand="ComplianceDocument$ArPaymentID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="CustomerID" LinkCommand="ComplianceDocuments_Customer_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InvoiceID" LinkCommand="ComplianceDocument$InvoiceID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="IsExpired" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsRequiredJointCheck" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="JointRelease" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointReleaseReceived" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="LastModifiedByID" />
                                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Limit" TextAlign="Right" />
                                    <px:PXGridColumn DataField="MethodSent" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentMethodID" />
                                    <px:PXGridColumn DataField="ApPaymentMethodID" />
                                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectTransactionID" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" CommitChanges="True" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PurchaseOrder" TextAlign="Left" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                                    <px:PXSelector runat="server" DataField="DocumentTypeValue" AutoRefresh="True" ID="edDocumentTypeValue" />
                                    <px:PXSelector runat="server" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" ID="edBillID" />
                                    <px:PXSelector runat="server" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" ID="edInvoiceID" />
                                    <px:PXSelector runat="server" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" ID="edApCheckID" />
                                    <px:PXSelector runat="server" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" ID="edArPaymentID" />
                                    <px:PXSelector runat="server" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" ID="edProjectTransactionID" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" ID="edPurchaseOrder" CommitChanges="True" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                                    <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                                    <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                                    <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                    <px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinWidth="300" />
	</px:PXTab>
</asp:Content>