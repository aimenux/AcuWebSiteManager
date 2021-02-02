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
			<px:PXDSCallbackCommand Name="AddressLookupSelectAction" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="AddressLookup" SelectControlsIDs="frmHeader" RepaintControls="None" RepaintControlsIDs="ds,Address"   CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="RemitAddressLookup" SelectControlsIDs="frmHeader" RepaintControls="None" RepaintControlsIDs="ds,RemitAddress"  CommitChanges="true" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Width="100%" Caption="Vendor Location Summary" DataMember="Location" LabelsWidth="SM" NoteIndicator="True" FilesIndicator="True" LinkIndicator="true" BPEventsIndicator="true"
		EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edBAccountID">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
				<px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" />
				<px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" AutoRefresh="True" />
				<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="frmHeader" Name="Location.bAccountID" PropertyName="NewDataKey[&quot;BAccountID&quot;]" Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" LabelsWidth="SM" Height="540px" Style="z-index: 100" Width="100%" DataMember="LocationCurrent" MarkRequired="Dynamic">
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
								<px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Size="xs" Text="View On Map" />
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
						<px:PXSelector ID="edVBranchID" runat="server" DataField="VBranchID" AllowEdit="True" />
						<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXCheckBox ID="chkVPrintOrder" runat="server" DataField="VPrintOrder" AlignLeft="True" TabIndex="-1" Width="134px" />
							<px:PXCheckBox ID="chkVEmailOrder" runat="server" DataField="VEmailOrder" AlignLeft="True" TabIndex="-1" />
						<px:PXLayoutRule runat="server" />

				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Payment">
				<Template>

					<px:PXFormView ID="APPaymentInfoLocation" runat="server" DataMember="APPaymentInfoLocation" LabelsWidth="SM" RenderStyle="Simple">
						<Template>

							<%-- column 1 --%>

							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Remit-To Address" />
								<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOverrideRemitAddress" runat="server" DataField="OverrideRemitAddress" />
								<px:PXFormView ID="RemitAddress" runat="server" DataMember="RemitAddress" LabelsWidth="SM" SyncPosition="true" RenderStyle="Simple">
									<Template>
										<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
										<px:PXButton ID="btnRemitAddressLookup" runat="server" CommandName="RemitAddressLookup" CommandSourceID="ds" Size="xs" TabIndex="-1" />
										<px:PXButton ID="btnViewRemitOnMap" runat="server" CommandName="ViewRemitOnMap" CommandSourceID="ds" Size="xs" Text="View on Map" />
										<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
										<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
										<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
										<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowEdit="True" LabelsWidth="SM" />
										<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
										<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" CommitChanges="True" AutoRefresh="True" AllowEdit="True" LabelsWidth="SM" />
										<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
									</Template>
								</px:PXFormView>

							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Remit-To Info" />
								<px:PXCheckBox CommitChanges="True" ID="chkOverrideRemitContact" runat="server" DataField="OverrideRemitContact" SuppressLabel="True" />
								<px:PXFormView ID="RemitContact" runat="server" DataMember="RemitContact" LabelsWidth="SM" RenderStyle="Simple">
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

							<%-- column 2 --%>
							
							<px:PXLayoutRule runat="server" StartColumn="True" />
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Payment Settings" />

								<px:PXFormView ID="IsAPPaymentInfoSameAsMain" runat="server" DataMember="LocationCurrent" ControlSize="XM" LabelsWidth="SM" RenderStyle="Simple">
									<Template>
										<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
										<px:PXCheckBox ID="chkIsAPPaymentInfoSameAsMain" runat="server" CommitChanges="True" DataField="IsAPPaymentInfoSameAsMain" />
									</Template>
								</px:PXFormView>

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

			<px:PXTabItem Text="Purchase Settings">
				<Template>

					<%-- column 1 --%>
					
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Tax Settings" />
						<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
						<px:PXSelector ID="edTaxZoneID" runat="server" DataField="VTaxZoneID" AllowEdit="True" />
						<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="VTaxCalcMode" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Receipt Actions" />
						<px:PXCheckBox ID="chkVAllowAPBillBeforeReceipt" runat="server" DataField="VAllowAPBillBeforeReceipt" />
						<px:PXNumberEdit ID="edVRcptQtyMin" runat="server" DataField="VRcptQtyMin" />
						<px:PXNumberEdit ID="edVRcptQtyMax" runat="server" DataField="VRcptQtyMax" />
						<px:PXNumberEdit ID="edVRcptQtyThreshold" runat="server" DataField="VRcptQtyThreshold" />
						<px:PXDropDown ID="edVRcptQtyAction" runat="server" DataField="VRcptQtyAction" />

					<%-- column 2 --%>

					<px:PXLayoutRule runat="server" StartColumn="True" />
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Instructions" />
						<px:PXSegmentMask ID="edVSiteID" runat="server" DataField="VSiteID" AllowEdit="True" />
						<px:PXSelector ID="edVCarrierID" runat="server" DataField="VCarrierID" AllowEdit="True" />
						<px:PXSelector ID="edShipTermsID" runat="server" DataField="VShipTermsID" AllowEdit="True" />
						<px:PXSelector ID="edFOBPointID" runat="server" DataField="VFOBPointID" AllowEdit="True" />
						<px:PXNumberEdit ID="edLeadTime" runat="server" DataField="VLeadTime" />

				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="GL Accounts">
				<Template>

					<%-- column 1 --%>

					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsAPAccountSameAsMain" runat="server" DataField="IsAPAccountSameAsMain" />
					<px:PXFormView ID="APAccountSubLocation" runat="server" DataMember="APAccountSubLocation" LabelsWidth="SM" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
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
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
							<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="VRetainageAcctID" CommitChanges="True" />
							<px:PXSegmentMask ID="edRetainageSubID" runat="server" DataField="VRetainageSubID" CommitChanges="True" />
						</Template>
					</px:PXFormView>

                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" MinHeight="400" MinWidth="600" Enabled="True" />
	</px:PXTab>
	<!--#include file="~\Pages\Includes\AddressLookupPanel.inc"-->
</asp:Content>
