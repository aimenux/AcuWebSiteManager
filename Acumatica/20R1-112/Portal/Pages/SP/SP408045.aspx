﻿<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP408045.aspx.cs" Inherits="Page_SP408045"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Contact" PageLoadBehavior="SearchSavedKeys"
		TypeName="SP.Objects.CR.UserProfileContactMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" Visible="false"/>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self"/>
			<px:PXDSCallbackCommand Name="SaveClose" CommitChanges="True" Visible="false" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" Visible="false"/>
            <px:PXDSCallbackCommand Name="Last" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" Visible="False" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="Next" Visible="False" />
			<px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" Visible="false"/>
			<px:PXDSCallbackCommand Name="AddOpportunity" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="Opportunities_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="Opportunities_Contact_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
			<px:PXDSCallbackCommand Name="AddCase" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="Cases_ViewDetails" Visible="False" DependOnGrid="gridCases" />
			<px:PXDSCallbackCommand Name="copyBAccountContactInfo" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="ViewOnMap" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ValidateAddress" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Members_CRCampaign_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="grdCampaignHistory" />
			<px:PXDSCallbackCommand Name="Subscriptions_CRMarketingList_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="grdMarketingLists" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="ResetPassword" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ResetPasswordOK" Visible="False" CommitChanges="True" />

			<px:PXDSCallbackCommand Name="ActivateLogin" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="EnableLogin" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="DisableLogin" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="UnlockLogin" Visible="False" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">	
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="Contact Summary"
		DataMember="Contact" DataSourceID="ds" NoteIndicator="False" FilesIndicator="False" DefaultControlID="edContactID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" /> 		
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="500px" DataMember="ContactCurrent"
		Width="100%">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" ControlSize="XM" LabelsWidth="SM"
						StartColumn="True" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Summary" StartGroup="True" />
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
					<px:PXLabel ID="LFirstName" runat="server" Size="SM" />
					<px:PXDropDown ID="Title" runat="server" DataField="Title" Size="XS" SuppressLabel="True" CommitChanges="True" />
					<px:PXTextEdit ID="FirstName" runat="server" DataField="FirstName" Size="Empty" Width="164px" 
					LabelID="LFirstName" CommitChanges="True" />
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
					<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" SuppressLabel="False"
						CommitChanges="True" />
					<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="False" />
					
                    <px:PXLayoutRule ID="PXLayoutRule14" runat="server" ControlSize="XM" LabelsWidth="SM"/>
					<px:PXLayoutRule ID="PXLayoutRule15" runat="server" GroupCaption="Contact" StartGroup="True" />
					<px:PXMailEdit ID="EMail" runat="server" CommandName="NewMailActivity" CommandSourceID="ds"
						DataField="EMail" CommitChanges="True"/>
					<px:PXLinkEdit ID="WebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
					<px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" SuppressLabel="False" />
					<px:PXMaskEdit ID="Phone2" runat="server" DataField="Phone2" />
					<px:PXTextEdit ID="Phone3" runat="server" DataField="Phone3" />
					<px:PXMaskEdit ID="Fax" runat="server" DataField="Fax" />

                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True"/>
					<px:PXLayoutRule ID="PXLayoutRule16" runat="server" GroupCaption="Address" StartGroup="True" />
					<px:PXFormView ID="formA" runat="server" DataMember="AddressCurrent" DataSourceID="ds" SkinID="Transparent">
						<Template>
                        <px:PXLayoutRule ID="PXLayoutRule19" runat="server" />							
							<px:PXCheckBox ID="chkIsSameAsMaint" runat="server" DataField="ContactCurrent2.IsAddressSameAsMain"	CommitChanges="True" />			
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID"
								FilterByAllFields="True" TextMode="Search" CommitChanges="True" DataSourceID="ds"/>
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="True"
								FilterByAllFields="True" TextMode="Search" DataSourceID="ds" />
							<px:PXLayoutRule ID="PXLayoutRule20" runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" />
							<px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds"
								Size="xs" Text="View On Map" Height="20px" />
							<px:PXLayoutRule ID="PXLayoutRule21" runat="server" />
						</Template>
						<ContentLayout ControlSize="XM" LabelsWidth="SM" OuterSpacing="None" />
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem LoadOnDemand="True" Text="User Info" BindingContext="form">
                <Template>
                    <px:PXFormView ID="frmLogin" runat="server" DataMember="UserPrefs" SkinID="Transparent">
                        <Template>
							<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartGroup="True" GroupCaption="Personal Settings" ControlSize="XXXL" LabelsWidth="SM" />
							<px:PXDropDown ID="edTimeZone" runat="server" DataField="TimeZone" />

							<px:PXLayoutRule ID="PXLayoutRule9" runat="server" ControlSize="XM" LabelsWidth="SM" StartRow="true" GroupCaption="User Information"/>
							<px:PXFormView ID="formUser" runat="server" DataMember="User" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule29" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True"/>
									<px:PXDropDown ID="edState" runat="server" DataField="State" Enabled="False"/>
									<px:PXSelector ID="edLoginType" runat="server" DataField="LoginTypeID" CommitChanges="True"/>
									<px:PXTextEdit ID="edUsername" runat="server" DataField="Username" CommitChanges="True"/>
                					<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode ="Password"/>
									<px:PXCheckBox ID="edGenerate" runat="server" DataField="GeneratePassword" CommitChanges="True"/>
									<px:PXButton ID="btnResetPassword" runat="server" Text="Reset Password" CommandName="ResetPassword" CommandSourceID="ds" Width="150" Height="20"/>
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="SM" StartColumn="True" SuppressLabel="True"/>
									<px:PXButton ID="btnActivateLogin" runat="server" CommandName="ActivateLogin" CommandSourceID="ds" Width="150" Height="20"/>
									<px:PXButton ID="btnEnableLogin" runat="server" CommandName="EnableLogin" CommandSourceID="ds" Width="150" Height="20"/>
									<px:PXButton ID="btnDisableLogin" runat="server" CommandName="DisableLogin" CommandSourceID="ds" Width="150" Height="20"/>
									<px:PXButton ID="btnUnlockLogin" runat="server" CommandName="UnlockLogin" CommandSourceID="ds" Width="150" Height="20"/>
								</Template>
							</px:PXFormView>
						</Template>
                    </px:PXFormView>
                    <px:PXSmartPanel ID="pnlResetPassword" runat="server" Caption="Change password"
                    	LoadOnDemand="True" Width="400px" Key="User" CommandName="ResetPasswordOK" CommandSourceID="ds" AcceptButtonID="btnOk" CancelButtonID="btnCancel" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmResetParams" AutoCallBack-Enabled="true" AutoReload="True">
                        <px:PXFormView ID="frmResetParams" runat="server" DataSourceID="ds" Width="100%" DataMember="User"
                            Caption="Reset Password" SkinID="Transparent">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                                <px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
                                <px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
                            </Template>
                        </px:PXFormView>
                        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                            <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK"/>
                            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
                        </px:PXPanel>
                    </px:PXSmartPanel>
                </Template>
            </px:PXTabItem>
        </Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
