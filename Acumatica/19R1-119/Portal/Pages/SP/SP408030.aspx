<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP408030.aspx.cs" Inherits="Page_SP408030" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.CR.ProductBusinessAccountMaint" PrimaryView="BAccount" PageLoadBehavior="SearchSavedKeys">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="First" StartNewGroup="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Last" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" Visible="False" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="Next" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" Visible="False" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grdContacts" Name="Contacts_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="AddContact" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="Locations_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="AddLocation" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefaultLocation" Visible="False" RepaintControlsIDs="grdLocations" />
            <px:PXDSCallbackCommand Name="AddOpportunity" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Opportunities_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
            <px:PXDSCallbackCommand Name="Opportunities_Contact_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
            <px:PXDSCallbackCommand Name="AddCase" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Cases_ViewDetails" Visible="False" DependOnGrid="gridCases" />
            <px:PXDSCallbackCommand DependOnGrid="grdContracts" Name="Contracts_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grdContracts" Name="Contracts_Location_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grdOrders" Name="Orders_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewVendor" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewCustomer" Visible="False" />
            <px:PXDSCallbackCommand Name="ConverToCustomer" Visible="False" />
            <px:PXDSCallbackCommand Name="ConverToVendor" Visible="False" />
			<px:PXDSCallbackCommand Name="MarkAsValidated" Visible="False" />
			<px:PXDSCallbackCommand Name="CheckForDuplicates" Visible="False" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" DependOnGrid="grdContacts" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="ViewDefLocationOnMap" CommitChanges="True" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewMainOnMap" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Members_CRCampaign_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="grdCampaignHistory" />
            <px:PXDSCallbackCommand Name="Subscriptions_CRMarketingList_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="grdMarketingLists" />
            <px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="ViewDuplicateAccount" Visible="False"/>            
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataMember="BAccount" DataSourceID="ds" NoteIndicator="False" FilesIndicator="False">
    	<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edAcctCD" runat="server" DataField="AcctCD" FilterByAllFields="True" CommitChanges="True" AutoRefresh="True"/>
		</Template>
    </px:PXFormView>
    <px:PXFormView ID="form1" runat="server" Width="100%" Caption="Account Summary" DataSourceID="ds" DataMember="CurrentBAccount">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Main Contact" StartGroup="True" />
            <px:PXFormView ID="frmDefContact" runat="server" CaptionVisible="False" DataMember="DefContact"
                DataSourceID="ds" SkinID="Transparent">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Main Contact" />
					<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" Enabled="False"/>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
                    <px:PXLabel ID="lblSalutation" runat="server">Attention:</px:PXLabel>
                    <px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="True" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" />
                    <px:PXTextEdit ID="edEMail" runat="server" DataField="EMail" CommandName="NewMailActivity"
                        CommandSourceID="ds" CommitChanges="True"/>
                    <px:PXTextEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                    <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                    <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
                    <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
                </Template>
                <ContentLayout OuterSpacing="None" />
                <ContentStyle BackColor="Transparent" BorderStyle="None">
                </ContentStyle>
            </px:PXFormView>
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" GroupCaption="Main Address" StartGroup="True" StartColumn="True" />
            <px:PXFormView ID="frmDefAddress" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefAddress" SkinID="Transparent">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Main Address" />
                    <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                    <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                    <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                    <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID"
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
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXFormView>
</asp:Content>



