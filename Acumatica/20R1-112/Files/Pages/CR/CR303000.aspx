<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="CR303000.aspx.cs" Inherits="Page_CR303000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        function refreshTasksAndEvents(ds, context) {
            if (context.command == "Cancel") {
                var top = window.top;
                if (top != window && top.MainFrame != null) top.MainFrame.refreshEventsInfo();
            }
        }
    </script>
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.BusinessAccountMaint"
        PrimaryView="BAccount">
        <ClientEvents CommandPerformed="refreshTasksAndEvents" />
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="ViewCustomer" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewVendor" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand DependOnGrid="grdContacts" Name="Contacts_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="AddContact" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="Locations_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="AddLocation" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefaultLocation" Visible="False" RepaintControlsIDs="grdLocations" />
            <px:PXDSCallbackCommand Name="AddOpportunity" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Opportunities_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
            <px:PXDSCallbackCommand Name="Opportunities_BAccount_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
            <px:PXDSCallbackCommand Name="Opportunities_Contact_ViewDetails" Visible="False" DependOnGrid="gridOpportunities" />
            <px:PXDSCallbackCommand Name="AddCase" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Cases_ViewDetails" Visible="False" DependOnGrid="gridCases" />
            <px:PXDSCallbackCommand DependOnGrid="grdContracts" Name="Contracts_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grdContracts" Name="Contracts_Location_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="Contracts_BAccount_ViewDetails" Visible="False" DependOnGrid="grdContracts" />
            <px:PXDSCallbackCommand DependOnGrid="grdOrders" Name="Orders_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="ConverToCustomer" Visible="False" />
            <px:PXDSCallbackCommand Name="ConverToVendor" Visible="False" />
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
            <px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            
            <px:PXDSCallbackCommand Name="CheckForDuplicates" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="DuplicateMerge" Visible="False" CommitChanges="True" DependOnGrid="PXGridDuplicates" />
            <px:PXDSCallbackCommand Name="DuplicateAttach" Visible="False" CommitChanges="True" DependOnGrid="PXGridDuplicates" />
            <px:PXDSCallbackCommand Name="ViewDuplicate" Visible="false" DependOnGrid="PXGridDuplicates" />
            <px:PXDSCallbackCommand Name="ViewDuplicateRefContact" Visible="false" DependOnGrid="PXGridDuplicates" />
            <px:PXDSCallbackCommand Name="MarkAsValidated" Visible="false" />
            <px:PXDSCallbackCommand Name="CloseAsDuplicate" Visible="false" />

            <px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
            <px:PXDSCallbackCommand Name="syncHubSpot" Visible="false" />
            <px:PXDSCallbackCommand Name="pullFromHubSpot" Visible="false" />
            <px:PXDSCallbackCommand Name="pushToHubSpot" Visible="false" />
            <px:PXDSCallbackCommand Name="CreateLead" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
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
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="Account Summary"
        DataMember="BAccount" DataSourceID="ds" NoteIndicator="True" LinkIndicator="True"
        NotifyIndicator="True" FilesIndicator="True" DefaultControlID="edAcctCD">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
                ControlSize="XM" />
            <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" AutoRefresh="True"
                FilterByAllFields="True" />
            <px:PXTextEdit CommitChanges="True" ID="edAcctName" runat="server" DataField="AcctName" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Size="S" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM"
                ControlSize="XM" />
            <px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search"
                DisplayMode="Text" FilterByAllFields="True" AutoRefresh="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID"
                TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
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
                    <px:PXFormView ID="frmDefContact" runat="server" CaptionVisible="False" DataMember="DefContact" TabIndex="10"
                        DataSourceID="ds" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM"
                                ControlSize="XM" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="PXLabel1" runat="server" Size="SM" Text="Attention"></px:PXLabel>
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" SuppressLabel="true" />
                            <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                            <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommandName="NewMailActivity"
                                CommandSourceID="ds" CommitChanges="True" />
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
                            <px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" />
                            <px:PXMaskEdit ID="Phone2" runat="server" DataField="Phone2" />
                            <px:PXMaskEdit ID="Fax" runat="server" DataField="Fax" />
                            <px:PXLayoutRule ID="PXLayoutRule23" runat="server" />
                            <px:PXCheckBox ID="edDuplicateFound" runat="server" DataField="DuplicateFound" Visible="False" />
                        </Template>
                        <ContentLayout OuterSpacing="None" />
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" GroupCaption="Main Address" StartGroup="True" />
                    <px:PXFormView ID="frmDefAddress" runat="server" CaptionVisible="False" DataSourceID="ds" TabIndex="20"
                        DataMember="AddressCurrent" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="SM"
                                ControlSize="XM" />
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXLayoutRule ID="PXLayoutRule18" runat="server" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
                                FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" CommitChanges="True" />
                            <px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds"
                                Size="xs" Text="View On Map" Height="20" TabIndex="-1" />
                            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" />
                            <px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
                                FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
                        </Template>
                        <ContentLayout OuterSpacing="None" />
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>

                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartGroup="True" GroupCaption="CRM" StartColumn="True" ControlSize="XM" LabelsWidth="M" />
                    <px:PXSelector ID="ClassID" runat="server" CommitChanges="True" DataField="ClassID" TabIndex="30"
                        FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                    <px:PXTextEdit ID="edAcctReferenceNbr" runat="server" DataField="AcctReferenceNbr" TabIndex="40"/>
                    <px:PXSegmentMask ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" TabIndex="50"
                        AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="true" />
                    <px:PXDropDown ID="edDuplicateStatus1" runat="server" DataField="DefContact.DuplicateStatus" Enabled="False" TabIndex="60"/>
                    <px:PXDateTimeEdit ID="edLastIncomingDate" runat="server" DataField="BAccountActivityStatistics.LastIncomingActivityDate" Enabled="False" Size="SM" TabIndex="70"/>
                    <px:PXDateTimeEdit ID="edLastOutgoingDate" runat="server" DataField="BAccountActivityStatistics.LastOutgoingActivityDate" Enabled="False" Size="SM" TabIndex="80"/>
                    <px:PXSelector ID="edCampaignSourceID" runat="server" DataField="CampaignSourceID" AllowEdit="True" TextMode="Search" DisplayMode="Hint" FilterByAllFields="True" TabIndex="90"/>
                    <px:PXSelector ID="edLanguageID" runat="server" AllowEdit="True" DataField="DefContact.LanguageID" TextMode="Search" DataSourceID="ds" DisplayMode="Hint" TabIndex="100"/>
                    
					<px:PXLayoutRule ID="PXLayoutRule25" runat="server" StartGroup="True" />
					<px:PXFormView ID="frmNoBother" runat="server" CaptionVisible="False" DataSourceID="ds"
                        DataMember="DefContact" SkinID="Transparent" TabIndex="110">
                        <Template>
							<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" LabelsWidth="M"/>
								<px:PXCheckBox ID="edNoCall" runat="server" DataField="NoCall" Size="S" SuppressLabel="True" Width="112"/>
								<px:PXCheckBox ID="edNoFax" runat="server" DataField="NoFax" Size="S" SuppressLabel="True"/>
							<px:PXLayoutRule ID="PXLayoutRule7" runat="server"/>
							<px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True"/>
								<px:PXCheckBox ID="edNoEMail" runat="server" DataField="NoEMail" Size="S" SuppressLabel="True" Width="112"/>
								<px:PXCheckBox ID="edNoMail" runat="server" DataField="NoMail" Size="S" SuppressLabel="True"/>
							<px:PXLayoutRule ID="PXLayoutRule9" runat="server"/>
							<px:PXLayoutRule ID="PXLayoutRule24" runat="server" Merge="True"/>
								<px:PXCheckBox ID="edNoMassMail" runat="server" DataField="NoMassMail" Size="S" Width="112"/>
								<px:PXCheckBox ID="edNoMarketingMaterials" runat="server" DataField="NoMarketing" Size="S"/>

							<px:PXLayoutRule ID="PXLayoutRule16" runat="server" GroupCaption="Personal Data Privacy" StartGroup="True" ControlSize="XM" LabelsWidth="M"/>
								<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="ConsentAgreement" AlignLeft="True" CommitChanges="True"/>
								<px:PXDateTimeEdit ID="edConsentDate" runat="server" DataField="ConsentDate" CommitChanges="True"/>
								<px:PXDateTimeEdit ID="edConsentExpirationDate" runat="server" DataField="ConsentExpirationDate" CommitChanges="True"/>
                        </Template>
                        <ContentLayout OuterSpacing="None" />
						<ContentStyle BackColor="Transparent" BorderStyle="None"/>
                    </px:PXFormView>

                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Duplicates" BindingContext="frmDefContact" VisibleExp="DataControls[&quot;edDuplicateFound&quot;].Value == true" LoadOnDemand="True">
                <Template>
                    <!--#include file="~\Pages\CR\Includes\CRDuplicateEntityTab.inc"-->
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                        Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False"
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
                                    <px:PXGridColumn DataField="ContactID" LinkCommand="OpenActivityContact" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
                                    <px:PXGridColumn DataField="TimeSpent" />
                                    <px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
                                        SyncVisible="False" SyncVisibility="False">
                                        <NavigateParams>
                                            <px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
                                        </NavigateParams>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="Source" AllowResize="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine"
                                MaxLength="50" Width="100%" Height="100px" SkinID="Label">
                                <AutoSize Container="Parent" Enabled="true" />
                            </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Contacts" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="grdContacts" runat="server" DataSourceID="ds" Height="522px" Width="100%"
                        SkinID="Inquire" AllowSearch="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Contacts">
                                <Columns>
                                    <px:PXGridColumn DataField="DisplayName" LinkCommand="Contacts_ViewDetails" />
                                    <px:PXGridColumn DataField="Salutation" />
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Address__City" />
                                    <px:PXGridColumn DataField="EMail" />
                                    <px:PXGridColumn DataField="Phone1" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ContactType" />
                                    <px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <ActionBar DefaultAction="cmdViewContact" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton ImageKey="AddNew" Tooltip="Add New Contact" DisplayStyle="Image">
                                    <AutoCallBack Command="AddContact" Target="ds">
                                    </AutoCallBack>
                                    <PopupCommand Command="Refresh" Target="grdContacts">
                                    </PopupCommand>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Contact Details" Key="cmdViewContact" Visible="False">
                                    <AutoCallBack Command="Contacts_ViewDetails" Target="ds">
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
            <px:PXTabItem Text="Delivery Settings" LoadOnDemand="true">
                <Template>
                    <px:PXFormView ID="frmDefLocation1" runat="server" DataMember="DefLocation" DataSourceID="ds"
                        Width="100%" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" LabelsWidth="SM"
                                ControlSize="XM" />
                            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartGroup="True" GroupCaption="Shipping Contact" />
                            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsContactSameAsMain" TabIndex="10"
                                runat="server" DataField="IsContactSameAsMain" />
                            <px:PXPanel ID="PXPanel3" runat="server" RenderStyle="Simple" RenderSimple="True">
                                <px:PXFormView ID="frmDefLocationContact" runat="server" DataMember="DefLocationContact"
                                    DataSourceID="ds" SkinID="Transparent">
                                    <Template>
                                        <px:PXLayoutRule ID="PXLayoutRule14" runat="server" LabelsWidth="SM" ControlSize="XM"
                                            StartColumn="True" />
                                        <px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True" />
                                        <px:PXLabel ID="lblAttention1" runat="server">Attention:</px:PXLabel>
                                        <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" SuppressLabel="True" TabIndex="20"/>
                                        <px:PXLayoutRule ID="PXLayoutRule22" runat="server" />
                                        <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" TabIndex="30"/>
                                        <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" TabIndex="40"/>
                                        <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" TabIndex="50"/>
                                        <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" TabIndex="60"/>
                                    </Template>
                                    <ContentLayout OuterSpacing="None" />
                                </px:PXFormView>
                            </px:PXPanel>
                            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" GroupCaption="Shipping Address"
                                StartGroup="True" />
                            <px:PXLayoutRule ID="PXLayoutRule16" runat="server" Merge="True" />
                            <px:PXPanel ID="PXPanel4" runat="server" RenderStyle="Simple" RenderSimple="True">
                                <px:PXFormView ID="frmDefLocationAddress" runat="server" DataMember="DefLocationAddress"
                                    DataSourceID="ds" SkinID="Transparent">
                                    <Template>
                                        <px:PXLayoutRule ID="PXLayoutRule14" runat="server" Merge="True" SuppressLabel="False" />
                                        <px:PXFormView ID="frmDefLocationCurrent" runat="server" DataMember="DefLocationCurrent"
                                            DataSourceID="ds" RenderStyle="Simple">
                                            <Template>
                                                <px:PXLayoutRule ID="PXLayoutRule14" runat="server" SuppressLabel="False" LabelsWidth="SM" />
                                                <px:PXCheckBox ID="chkIsSameAsMaint" runat="server" DataField="IsAddressSameAsMain" TabIndex="70"
                                                    CommitChanges="true">
                                                </px:PXCheckBox>
                                            </Template>
                                            <ContentStyle BackColor="Transparent" />
                                        </px:PXFormView>
                                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True" LabelsWidth="SM" ControlSize="XM" />
                                        <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" TabIndex="80" Enabled="False" />
                                        <px:PXLayoutRule ID="PXLayoutRule17" runat="server" LabelsWidth="SM" ControlSize="XM" />
                                        <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" TabIndex="90"/>
                                        <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" TabIndex="100"/>
                                        <px:PXTextEdit ID="edCity" runat="server" DataField="City" TabIndex="110"/>
                                        <px:PXSelector ID="edCountryID2" runat="server" AllowEdit="True" DataField="CountryID" TabIndex="120"
                                            FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
                                        <px:PXSelector ID="edState2" runat="server" AutoRefresh="True" DataField="State" TabIndex="130"
                                            CommitChanges="true" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                                        <px:PXLayoutRule ID="PXLayoutRule18" runat="server" Merge="True" LabelsWidth="SM" />
                                        <px:PXMaskEdit Size="S" ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" TabIndex="140"/>
                                        <px:PXButton ID="btnViewOnMap1" runat="server" CommandName="ViewDefLocationOnMap" TabIndex="-1"
                                            CommandSourceID="ds" Size="xs" Text="View On Map" Height="20">
                                        </px:PXButton>
                                    </Template>
                                    <ContentLayout OuterSpacing="None" />
                                </px:PXFormView>
                            </px:PXPanel>
                            <px:PXLayoutRule ID="PXLayoutRule19" runat="server" />
                            <px:PXLayoutRule ID="PXLayoutRule20" runat="server" ControlSize="XM" LabelsWidth="SM"
                                StartColumn="True" />
                            <px:PXLayoutRule ID="PXLayoutRule21" runat="server" StartGroup="True" GroupCaption="Default Location Settings" />
                            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" TabIndex="150"/>
                            <px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" TabIndex="160"/>
                            <px:PXSelector ID="edCTaxZoneID" runat="server" DataField="CTaxZoneID" AllowEdit="True" TabIndex="170"/>
                            <px:PXDropDown ID="exTaxCalcMode" runat="server" DataField="CTaxCalcMode" />
                            <px:PXSelector ID="edCBranchID" runat="server" DataField="CBranchID" AllowEdit="True" TabIndex="180"/>
                            <px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID" AllowEdit="True" TabIndex="190"/>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Shipping Instructions" />
                            <px:PXSegmentMask ID="edCSiteID" runat="server" DataField="CSiteID" AllowEdit="True" TabIndex="200"/>
                            <px:PXSelector CommitChanges="True" ID="edCarrierID" runat="server" DataField="CCarrierID" TabIndex="210"
                                AllowEdit="True" />
                            <px:PXSelector ID="edShipTermsID" runat="server" DataField="CShipTermsID" AllowEdit="True" TabIndex="220"/>
                            <px:PXSelector ID="edShipZoneID" runat="server" DataField="CShipZoneID" AllowEdit="True" TabIndex="230"/>
                            <px:PXSelector ID="edFOBPointID" runat="server" DataField="CFOBPointID" AllowEdit="True" TabIndex="240"/>
                            <px:PXCheckBox ID="chkResedential" runat="server" DataField="CResedential" TabIndex="250"/>
                            <px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="CSaturdayDelivery" TabIndex="260"/>
                            <px:PXCheckBox ID="chkInsurance" runat="server" DataField="CInsurance" TabIndex="270"/>
                            <px:PXDropDown ID="edCShipComplete" runat="server" DataField="CShipComplete" TabIndex="280"/>
                            <px:PXNumberEdit ID="edCOrderPriority" runat="server" DataField="COrderPriority" TabIndex="290"/>
                            <px:PXNumberEdit ID="edLeadTime" runat="server" DataField="CLeadTime" TabIndex="300"/>
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Locations" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="grdLocations" runat="server" DataSourceID="ds" Height="100%" Width="100%"
                        SkinID="Inquire" AllowSearch="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Locations">
                                <Columns>
                                    <px:PXGridColumn DataField="LocationCD" LinkCommand="Locations_ViewDetails" />
                                    <px:PXGridColumn DataField="Descr" />
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Address__City" />
                                    <px:PXGridColumn DataField="Address__CountryID" RenderEditorText="True" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Address__State" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CTaxZoneID" />
                                    <px:PXGridColumn DataField="CPriceClassID" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="CSalesAcctID" AutoCallBack="True" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="CSalesSubID" AllowShowHide="Server" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <LevelStyles>
                            <RowForm Height="400px" Width="800px">
                            </RowForm>
                        </LevelStyles>
                        <ActionBar DefaultAction="cmdViewLocation">
                            <CustomItems>
                                <px:PXToolBarButton ImageKey="AddNew" Tooltip="Add New Location" DisplayStyle="Image">
                                    <AutoCallBack Command="AddLocation" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="grdLocations" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Location Details" Key="cmdViewLocation" Visible="False">
                                    <AutoCallBack Command="Locations_ViewDetails" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="grdLocations" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Set as Default">
                                    <AutoCallBack Command="SetDefaultLocation" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Relations" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" SyncPositionWithGraph="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Relations">
                                <Columns>
                                    <px:PXGridColumn DataField="Role" CommitChanges="True"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="IsPrimary" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="TargetType" CommitChanges="True"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="TargetNoteID" DisplayMode="Text" LinkCommand="Relations_TargetDetails" CommitChanges="True"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="EntityID" AutoCallBack="true" LinkCommand="Relations_EntityDetails" CommitChanges="True"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="Name" ></px:PXGridColumn>
                                    <px:PXGridColumn DataField="ContactID" AutoCallBack="true" TextAlign="Left" TextField="ContactName" DisplayMode="Text" LinkCommand="Relations_ContactDetails"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="Email" ></px:PXGridColumn>
                                    <px:PXGridColumn DataField="AddToCC" Type="CheckBox" TextAlign="Center"></px:PXGridColumn>
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector ID="edTargetNoteID" runat="server" DataField="TargetNoteID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edRelEntityID" runat="server" DataField="EntityID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edRelContactID" runat="server" DataField="ContactID" FilterByAllFields="True" AutoRefresh="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True"></Mode>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100"></AutoSize>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Leads" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="grdLeads" runat="server" Height="400px" Width="100%" Style="z-index: 100"
                                AllowPaging="True" ActionsPosition="Top" DataSourceID="ds" SkinID="Details" SyncPosition="True"
                                AllowSearch="True" AllowFilter="True" FastFilterFields="DisplayName,Salutation,Address__City,EMail,Phone1,Source,CampaignID,Status,OwnerID,WorkgroupID">
                        <Levels>
                            <px:PXGridLevel DataMember="Leads">
                                <Columns>
									<px:PXGridColumn DataField="MemberName" LinkCommand="Leads_ViewDetails" AllowShowHide="False"/>
									<px:PXGridColumn DataField="Salutation" />
									<px:PXGridColumn DataField="CreatedDateTime" />
									<px:PXGridColumn DataField="Address__City" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="Address__State" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="Address__CountryID" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="EMail" />
									<px:PXGridColumn DataField="Phone1" />
									<px:PXGridColumn DataField="Phone2" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="Phone3" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="Source" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="CampaignID" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
									<px:PXGridColumn DataField="WorkgroupID" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="LastModifiedDateTime" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="LastModifiedByID" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar DefaultAction="cmdViewDoc">
                            <Actions>
                                <Delete Enabled = "false" />
                                <FilterBar ToolBarVisible="Top" Order="0" GroupIndex="3" />
                                <AddNew ToolBarVisible="False"/>
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddLead" ImageKey="AddNew" Tooltip="Add New Lead" DisplayStyle="Image">
                                    <AutoCallBack Command="CreateLead" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Opportunities" LoadOnDemand="true">
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
                                    <px:PXGridColumn DataField="BAccount__AcctCD" LinkCommand="Opportunities_BAccount_ViewDetails" />
                                    <px:PXGridColumn DataField="BAccount__AcctName" />
                                    <px:PXGridColumn DataField="Contact__DisplayName" LinkCommand="Opportunities_Contact_ViewDetails" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ClassID" LinkCommand="Opportunities_CLassID_ViewDetails"  AllowShowHide="true" Visible="false" SyncVisible="false"/>
                                    <px:PXGridColumn DataField="CROpportunityClass__Description" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar DefaultAction="cmdOpportunityDetails" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddOpportunity" ImageKey="AddNew" Tooltip="Add New Opportunity" DisplayStyle="Image">
                                    <AutoCallBack Command="AddOpportunity" Target="ds" />
                                    <ActionBar GroupIndex="0" Order="4" />
                                    <PopupCommand Command="Refresh" Target="gridOpportunities" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Opportunity Details" Key="cmdOpportunityDetails">
                                    <AutoCallBack Command="Opportunities_ViewDetails" Target="ds" />
                                    <ActionBar GroupIndex="2" />
                                    <PopupCommand Command="Refresh" Target="gridOpportunities" />
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
                        <ActionBar DefaultAction="cmdViewCaseDetails" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddCase" ImageKey="AddNew" Tooltip="Add New Case" DisplayStyle="Image">
                                    <AutoCallBack Command="AddCase" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="gridCases" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Case Details" Key="cmdViewCaseDetails" Visible="false">
                                    <ActionBar GroupIndex="0" />
                                    <AutoCallBack Command="Cases_ViewDetails" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="gridCases" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Cases">
                                <Columns>
                                    <px:PXGridColumn DataField="CaseCD" LinkCommand="Cases_ViewDetails" />
                                    <px:PXGridColumn DataField="Subject" />
                                    <px:PXGridColumn DataField="CaseClassID" />
                                    <px:PXGridColumn DataField="ContractID" />
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
            <px:PXTabItem Text="Contracts" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="grdContracts" runat="server" DataSourceID="ds" Height="522px" Width="100%"
                        SkinID="Inquire">
                        <Levels>
                            <px:PXGridLevel DataMember="Contracts">
                                <Columns>
                                    <px:PXGridColumn DataField="ContractCD" LinkCommand="Contracts_ViewDetails" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="LocationID" LinkCommand="Contracts_Location_ViewDetails" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <ActionBar DefaultAction="cmdViewContract" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="Contract Details" Key="cmdViewContract" Visible="False">
                                    <ActionBar GroupIndex="2" />
                                    <AutoCallBack Command="Contracts_ViewDetails" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="grdContracts" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Orders" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="grdOrders" runat="server" DataSourceID="ds" Height="522px" Width="100%"
                        SkinID="Inquire" AllowSearch="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Orders">
                                <Columns>
                                    <px:PXGridColumn DataField="OrderType" />
                                    <px:PXGridColumn DataField="OrderNbr" LinkCommand="Orders_ViewDetails" />
                                    <px:PXGridColumn DataField="OrderDesc" />
                                    <px:PXGridColumn DataField="CustomerOrderNbr" LinkCommand="Orders_ViewDetails" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="RequestDate" />
                                    <px:PXGridColumn DataField="ShipDate" />
                                    <px:PXGridColumn DataField="ShipVia" />
                                    <px:PXGridColumn DataField="ShipZoneID" />
                                    <px:PXGridColumn DataField="OrderWeight" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OrderVolume" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryID" />
                                    <px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <ActionBar DefaultAction="cmdViewOrder" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="Order Details" Key="cmdViewOrder" Visible="False">
                                    <ActionBar GroupIndex="2" />
                                    <AutoCallBack Command="Orders_ViewDetails" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="grdOrders" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Campaigns" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="grdCampaignHistory" runat="server" Height="400px" Width="100%" Style="z-index: 100"
                        AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds"
                        SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Members">
                                <Columns>
                                    <px:PXGridColumn DataField="CampaignID" AutoCallBack="true" LinkCommand="Members_CRCampaign_ViewDetails" />
                                    <px:PXGridColumn DataField="CRCampaign__CampaignName" />
                                    <px:PXGridColumn DataField="CRCampaign__Status" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Marketing Lists" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="grdMarketingLists" runat="server" Height="400px" Width="100%" SyncPosition="True"
                        AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds"
                        SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Subscriptions">
                                <Columns>
                                    <px:PXGridColumn DataField="IsSubscribed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="MarketingListID" AutoCallBack="true" LinkCommand="Subscriptions_CRMarketingList_ViewDetails"
                                        TextField="CRMarketingList__MailListCode" />
                                    <px:PXGridColumn DataField="CRMarketingList__Name" />
                                    <px:PXGridColumn DataField="Format" />
                                    <px:PXGridColumn DataField="CRMarketingList__IsDynamic" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Contact__ContactType" />
                                    <px:PXGridColumn DataField="CRMarketingListMember__ContactID" LinkCommand="Subscriptions_Contact_ViewDetails"
                                        TextField="Contact__MemberName" />
                                    <px:PXGridColumn DataField="Contact__Email" />
                                    <px:PXGridColumn DataField="Contact__Phone1" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector ID="edMarketingListID" runat="server" DataField="MarketingListID" AllowEdit="false" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Sync Status">
                <Template>
                    <px:PXGrid ID="syncGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="SyncRecs" DataKeyNames="SyncRecordID">
                                <Columns>
                                    <px:PXGridColumn DataField="SYProvider__Name" />
                                    <px:PXGridColumn DataField="RemoteID" CommitChanges="True" LinkCommand="GoToSalesforce" />
                                    <px:PXGridColumn DataField="Status" />
		                        <px:PXGridColumn DataField="Operation" />
		                        <px:PXGridColumn DataField="LastErrorMessage" />
                                    <px:PXGridColumn DataField="LastAttemptTS" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="AttemptCount" />
                                <px:PXGridColumn DataField="SFEntitySetup__ImportScenario" />
                                <px:PXGridColumn DataField="SFEntitySetup__ExportScenario" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="SyncSalesforce">
                                    <AutoCallBack Command="SyncSalesforce" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode InitNewRow="true" />
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <!--#include file="~\Pages\HS\HubSpotTab.inc"-->
            <px:PXTabItem Text="Mailing Settings" Overflow="Hidden" LoadOnDemand="True">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px" Caption="Mailings"
                                AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds">
                                <AutoSize Enabled="True" />
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
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
                            <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients" AdjustPageSize="Auto"
                                AllowPaging="True" DataSourceID="ds">
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
                                            <px:PXGridColumn DataField="ContactID">
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
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                            <px:PXDropDown ID="edContactType" runat="server" DataField="ContactType" />
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName"
                                                AllowEdit="True" />
                                        </RowTemplate>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
    <px:PXSmartPanel ID="pnlActivityContacts" runat="server" Height="81px" Style="z-index: 108; left: 352px; position: absolute; top: 99px"
        Width="376px" CaptionVisible="True"
        Caption="Select Contact" DesignView="Content" LoadOnDemand="True" Key="ActivityContacts"
        AutoCallBack-Target="frmActivityContacts" AutoCallBack-Command="Refresh">
        <div style="padding-top: 9px; padding-left: 9px; padding-right: 9px">
            <px:PXFormView ID="frmActivityContacts" runat="server" DataSourceID="ds"
                Width="100%" DataMember="ActivityContacts" SkinID="Transparent">
                <Template>
                    <table id="tbl" style="vertical-align: top; height: 20px;" cellspacing="0" cellpadding="0">
                        <tr align="left">
                            <td>
                                <px:PXLabel ID="lblContactID" runat="server">Contact :</px:PXLabel>
                            </td>
                            <td style="width: 9px;">&nbsp;
                            </td>
                            <td align="left">
                                <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID"
                                    LabelID="lblContactID" Width="261px" TextField="DisplayName" AutoRefresh="true"
                                    TextMode="Search" DisplayMode="Hint" FilterByAllFields="True" />
                            </td>
                        </tr>
                    </table>
                </Template>
            </px:PXFormView>
        </div>
        <div style="padding: 9px; text-align: right;">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" Width="63px"
                Height="20px" />
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Width="63px"
                Height="20px" Style="margin-left: 5px" />
        </div>
    </px:PXSmartPanel>

    <!--#include file="~\Pages\CR\Includes\CRDuplicateEntityMergePanel.inc"-->

</asp:Content>
