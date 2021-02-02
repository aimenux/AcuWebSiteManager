<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP303010.aspx.cs" Inherits="Page_AP303010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Location" TypeName="PX.Objects.AP.VendorLocationMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="True" CommitChanges="True" />            
			<px:PXDSCallbackCommand Name="ViewOnMap" Visible="false"  />
            <px:PXDSCallbackCommand Name="ViewRemitOnMap" Visible="false"  />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Width="100%" Caption="Vendor Location Summary" DataMember="Location" LabelsWidth="SM" NoteIndicator="True" FilesIndicator="True" LinkIndicator="true" NotifyIndicator="true"
		EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edBAccountID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" AutoRefresh="True" />
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
			<px:PXLayoutRule runat="server" Merge="False" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="frmHeader" Name="Location.bAccountID" PropertyName="NewDataKey[&quot;BAccountID&quot;]" Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" LabelsWidth="SM" Height="540px" Style="z-index: 100" Width="100%" DataMember="LocationCurrent" MarkRequired="Dynamic">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Location Contact" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsContactSameAsMain" runat="server" DataField="IsContactSameAsMain" />
					<px:PXFormView ID="Contact" runat="server" DataMember="Contact" RenderStyle="Simple" LabelsWidth="SM">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Address" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsAddressSameAsMain" runat="server" DataField="IsAddressSameAsMain" />
					<px:PXFormView ID="Address" runat="server" DataMember="Address" SyncPosition="true" RenderStyle="Simple" LabelsWidth="SM">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False"/>
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" CommitChanges="True" AllowAddNew="True" LabelsWidth="SM" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" AllowAddNew="True" LabelsWidth="SM" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
							<px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Text="View On Map">
							</px:PXButton>
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Location Details" />
					<px:PXSelector ID="edTaxZoneID" runat="server" DataField="VTaxZoneID" AllowEdit="True" />
					<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="VTaxCalcMode" />
					<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
					<px:PXSelector ID="edVBranchID" runat="server" DataField="VBranchID" AllowEdit="True" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkVPrintOrder" runat="server" DataField="VPrintOrder" />
					<px:PXCheckBox ID="chkVEmailOrder" runat="server" DataField="VEmailOrder" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Instructions" />
					<px:PXSegmentMask ID="edVSiteID" runat="server" DataField="VSiteID" AllowEdit="True" />
					<px:PXSelector ID="edShipTermsID" runat="server" DataField="VShipTermsID" AllowEdit="True" />
					<px:PXSelector ID="edVCarrierID" runat="server" DataField="VCarrierID" AllowEdit="True" />
					<px:PXSelector ID="edFOBPointID" runat="server" DataField="VFOBPointID" AllowEdit="True" />
					<px:PXNumberEdit ID="edLeadTime" runat="server" DataField="VLeadTime" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Receipt Actions" />
					<px:PXCheckBox ID="chkVAllowAPBillBeforeReceipt" runat="server" DataField="VAllowAPBillBeforeReceipt" />
					<px:PXNumberEdit ID="edVRcptQtyMin" runat="server" DataField="VRcptQtyMin" />
					<px:PXNumberEdit ID="edVRcptQtyMax" runat="server" DataField="VRcptQtyMax" />
					<px:PXNumberEdit ID="edVRcptQtyThreshold" runat="server" DataField="VRcptQtyThreshold" />
					<px:PXDropDown ID="edVRcptQtyAction" runat="server" DataField="VRcptQtyAction" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Payment Settings">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
					<px:PXCheckBox ID="chkIsAPPaymentInfoSameAsMain" runat="server" CommitChanges="True" DataField="IsAPPaymentInfoSameAsMain">
					</px:PXCheckBox>
					<px:PXFormView ID="APPaymentInfoLocation" runat="server" DataMember="APPaymentInfoLocation" LabelsWidth="SM" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Remittance Info" />
							<px:PXCheckBox CommitChanges="True" ID="chkIsRemitContactSameAsMain" runat="server" DataField="IsRemitContactSameAsMain" SuppressLabel="True" />
							<px:PXFormView ID="RemitContact" runat="server" DataMember="RemitContact" LabelsWidth="SM" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
									<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
									<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
									<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
									<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
									<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
									<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Remittance Address" />
							<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsRemitAddressSameAsMain" runat="server" DataField="IsRemitAddressSameAsMain" />
							<px:PXFormView ID="RemitAddress" runat="server" DataMember="RemitAddress" LabelsWidth="SM" SyncPosition="true" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
									<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
									<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
									<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
									<px:PXSelector CommitChanges="True" ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" AllowEdit="True" LabelsWidth="SM" />
									<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowEdit="True" LabelsWidth="SM" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
									<px:PXButton Size="xs" ID="btnViewRemitOnMap" runat="server" CommandName="ViewRemitOnMap" CommandSourceID="ds" Text="View on Map" />
									<px:PXLayoutRule runat="server" />
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Default Payment Settings" />
							<px:PXSelector CommitChanges="True" ID="edVPaymentMethodID" runat="server" DataField="VPaymentMethodID" AutoRefresh="True" AllowEdit="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edVCashAccountID" runat="server" DataField="VCashAccountID" AllowEdit="True" AutoRefresh="True" />
							<px:PXDropDown ID="edVPaymentByType" runat="server" DataField="VPaymentByType" />
							<px:PXNumberEdit ID="edVPaymentLeadTime" runat="server" DataField="VPaymentLeadTime" />
							<px:PXCheckBox ID="chkVSeparateCheck" runat="server" DataField="VSeparateCheck" />
							<px:PXGrid ID="grdPaymentDetails" runat="server" SkinID="Attributes" Caption="Payment Instructions" MatrixMode="True" Width="400px" Height="150px">
								<Levels>
									<px:PXGridLevel DataMember="PaymentDetails">
										<Columns>
											<px:PXGridColumn AllowUpdate="False" DataField="PaymentMethodDetail__descr" />
											<px:PXGridColumn AllowShowHide="False" DataField="DetailValue" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowSort="False" />
							</px:PXGrid>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsAPAccountSameAsMain" runat="server" DataField="IsAPAccountSameAsMain" />
					<px:PXFormView ID="APAccountSubLocation" runat="server" DataMember="APAccountSubLocation" LabelsWidth="SM" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSegmentMask CommitChanges="True" ID="edVAPAccountID" runat="server" DataField="VAPAccountID" />
							<px:PXSegmentMask CommitChanges="True" ID="edVAPSubID" runat="server" DataField="VAPSubID" />
						</Template>
					</px:PXFormView>
					<px:PXSegmentMask CommitChanges="True" ID="edVExpenseAcctID" runat="server" DataField="VExpenseAcctID" />
					<px:PXSegmentMask ID="edVExpenseSubID" runat="server" DataField="VExpenseSubID" />
					<px:PXSegmentMask ID="edVDiscountAcctID" runat="server" DataField="VDiscountAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edVDiscountSubID" runat="server" DataField="VDiscountSubID" />
					<px:PXFormView ID="APAccountSubLocation1" runat="server" DataMember="APAccountSubLocation" LabelsWidth="SM" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="VRetainageAcctID" CommitChanges="True" />
							<px:PXSegmentMask ID="edRetainageSubID" runat="server" DataField="VRetainageSubID" CommitChanges="True" />
						</Template>
					</px:PXFormView>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" MinHeight="400" MinWidth="600" Enabled="True" />
	</px:PXTab>
</asp:Content>
