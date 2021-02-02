<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP408032.aspx.cs" Inherits="Page_SP408032"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
<%--<script type="text/javascript">
    function selInit(sel, arg) {
        sel.suppressEdit = true;
    }
	</script>--%>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.SP.PartnerBusinessAccountMaint"
		PrimaryView="BAccount">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False"/>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" Visible="False"/>
			
			<px:PXDSCallbackCommand Name="First" Visible="False" />
			<px:PXDSCallbackCommand Name="Previous" Visible="False" />
			<px:PXDSCallbackCommand Name="Next" Visible="False" />
			<px:PXDSCallbackCommand Name="Last" Visible="False" />

            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />	 
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
			
            <px:PXDSCallbackCommand Name="Merge" Visible="False" />
            <px:PXDSCallbackCommand Name="MarkAsValidated" Visible="False" />
            <px:PXDSCallbackCommand Name="CheckForDuplicates"  Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdContacts" Name="Contacts_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="AddContact" Visible="True" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="Locations_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="AddLocation" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefaultLocation" Visible="False" RepaintControlsIDs="grdLocations" />
			<px:PXDSCallbackCommand Name="AddOpportunity" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="Opportunities_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
            <px:PXDSCallbackCommand Name="Opportunities_BAccount_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="Opportunities_Contact_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="AddCase" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="Cases_ViewDetails" Visible="False" DependOnGrid="gridCases" />
			<px:PXDSCallbackCommand DependOnGrid="grdContracts" Name="Contracts_ViewDetails"
				Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdContracts" Name="Contracts_Location_ViewDetails"
				Visible="False" />
			<px:PXDSCallbackCommand Name="Contracts_BAccount_ViewDetails" Visible="False" DependOnGrid="grdContracts"/>
			<px:PXDSCallbackCommand DependOnGrid="grdOrders" Name="Orders_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewVendor" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="ConverToCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="ConverToVendor" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" DependOnGrid="grdContacts" Visible="False"
				CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False"
				CommitChanges="True" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True"
				DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="ViewDefLocationOnMap" CommitChanges="True" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewMainOnMap" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Members_CRCampaign_ViewDetails" Visible="False" CommitChanges="True"
				DependOnGrid="grdCampaignHistory" />
			<px:PXDSCallbackCommand Name="Subscriptions_CRMarketingList_ViewDetails" Visible="False"
				CommitChanges="True" DependOnGrid="grdMarketingLists" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True"
				DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True"
				DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True"
				DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="PXGridDuplicates" Name="Duplicates_Contact_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="PXGridDuplicates" Name="Duplicates_BAccount_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grdContacts" Name="ViewDetails" />
            <px:PXDSCallbackCommand Name="ViewDuplicateAccount" Visible="False"/> 

			<px:PXDSCallbackCommand Name="SyncSalesforce" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
   <px:PXFormView ID="form" runat="server" Width="100%" Caption="Account Summary"
		DataMember="BAccount" DataSourceID="ds" NoteIndicator="True" LinkIndicator="True"
		NotifyIndicator="True" FilesIndicator="True" DefaultControlID="edAcctCD">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
		    <px:PXSelector ID="edAcctCD" runat="server" DataField="AcctCD" CommitChanges="True" AutoRefresh="True" 
		                   FilterByAllFields="True" Enabled ="False"/>
			<px:PXTextEdit CommitChanges="True" ID="edAcctName" runat="server" DataField="AcctName"/>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="XM" />				
			<px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" Size="SM" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="500px" Width="100%" DataMember="CurrentBAccount">
		<Items>
			<px:PXTabItem Text="Details" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" ControlSize="XM"
						LabelsWidth="SM" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Main Contact" StartGroup="True" />
					<px:PXFormView ID="frmDefContact" runat="server" CaptionVisible="False" DataMember="DefContact"
						DataSourceID="ds" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM"
								ControlSize="XM" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXLabel ID="lblSalutation" runat="server">Attention:</px:PXLabel>
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="True" />
							<px:PXLayoutRule runat="server" />
							<px:PXTextEdit ID="edEMail" runat="server" DataField="EMail" CommandName="NewMailActivity"
								CommandSourceID="ds" CommitChanges="True"/>
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
                        </Template>
						<ContentLayout OuterSpacing="None" />
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
					</px:PXFormView>
					<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartGroup="True" GroupCaption="CRM"/>
					<px:PXSelector ID="ClassID" runat="server" CommitChanges="True" DataField="ClassID"
						FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" GroupCaption="Main Address" StartColumn="True" StartGroup="True" />
					<px:PXFormView ID="frmDefAddress" runat="server" CaptionVisible="False" DataSourceID="ds"
						DataMember="AddressCurrent" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="SM"
								ControlSize="XM" />
							<px:PXLayoutRule ID="PXLayoutRule18" runat="server" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
								FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
								FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
							<px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" />
							<px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds"
								Size="xs" Text="View On Map" Height="20" />
							<px:PXLayoutRule ID="PXLayoutRule9" runat="server" />
						</Template>
						<ContentLayout OuterSpacing="None" />
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
					</px:PXFormView>   										
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
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
									<px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
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
			<px:PXTabItem Text="Contacts" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="grdContacts" runat="server" DataSourceID="ds" Height="522px" Width="100%"
						SkinID="Inquire" AllowSearch="True">
						<Levels>
							<px:PXGridLevel DataMember="Contacts">
								<Columns>
									<px:PXGridColumn DataField="DisplayName" Width="280px" LinkCommand="ViewDetails" />
									<px:PXGridColumn DataField="Salutation" Width="160px" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="EMail" Width="200px" />
									<px:PXGridColumn DataField="Phone1" Width="140px" />
									<px:PXGridColumn DataField="Address__City" Width="180px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<ActionBar DefaultAction="cmdViewContact">
							<CustomItems>
								<px:PXToolBarButton ImageKey="AddNew" Tooltip="Add New Contact" DisplayStyle="Image">
									<AutoCallBack Command="AddContact" Target="ds">
									</AutoCallBack>
									<PopupCommand Command="Refresh" Target="grdContacts">
									</PopupCommand>
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Contact Details" Key="cmdViewContact" Visible="False">
									<AutoCallBack Command="ViewDetails" Target="ds">
									</AutoCallBack>
									<PopupCommand Command="Refresh" Target="grdContacts">
									</PopupCommand>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem> 		
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>	
</asp:Content>
