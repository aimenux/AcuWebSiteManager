<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR301000.aspx.cs" Inherits="Page_CR301000"
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
	<px:PXDataSource ID="ds" EnableAttributes="true" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.LeadMaint" PrimaryView="Lead">
		<ClientEvents CommandPerformed="refreshTasksAndEvents" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="True" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="ViewOnMap" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ValidateAddress" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Subscriptions_CRMarketingList_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="grdMarketingLists" />
			<px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
			
			<px:PXDSCallbackCommand Name="CreateContact" CommitChanges="True" Visible="True" />
			<px:PXDSCallbackCommand Name="CreateBAccount" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="CreateBothContactAndAccount" CommitChanges="True" Visible="True" />
			<px:PXDSCallbackCommand Name="ConvertToOpportunity" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="ConvertToOpportunityAll" CommitChanges="True" Visible="True" />

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
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%"
		DataMember="Lead" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True"
		NotifyIndicator="True" Caption="Lead Summary">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" NullText="<NEW>" DisplayMode="Text" TextMode="Search" FilterByAllFields="True" AutoRefresh="True" />
				<px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True" Size="SM" AllowNull="True" NullText="New" />
				<px:PXDropDown ID="edResolution" runat="server" DataField="Resolution" CommitChanges="True" Size="SM" AutoRefresh="True" AllowNull="False" />
				<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" SuppressLabel="False" TextMode="MultiLine" Height="80px" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXSelector ID="edRefContactID" runat="server" AllowEdit="True" DataField="RefContactID" CommitChanges="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="True" OnEditRecord="edRefContactID_EditRecord" />
				<px:PXSelector ID="edBAccountID" runat="server" AllowEdit="True" DataField="BAccountID" CommitChanges="True" FilterByAllFields="True" TextMode="Search" DataSourceID="ds" />
				<px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="true" CommitChanges="True" />
				<px:PXDropDown ID="edSource" runat="server" DataField="Source" SelectedIndex="2" />
				<px:PXSelector ID="edCampaignID" runat="server" DataField="CampaignID" CommitChanges="True" />
				<px:PXCheckBox ID="edDuplicateFound" runat="server" DataField="DuplicateFound" Visible="False" />
				<px:PXDropDown ID="edDuplicateStatus" runat="server" DataField="DuplicateStatus" CommitChanges="True" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="LeadCurrent">
		<Items>
			<px:PXTabItem Text="Contact Info" RepaintOnDemand="False">
				<Template>
					<px:PXFormView ID="edLeadCurrent" runat="server" DataMember="LeadCurrent" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXCheckBox ID="edOverrideRefContact" runat="server" DataField="OverrideRefContact" Size="SM" CommitChanges="true" AlignLeft="True" TabIndex="-1" />
						</Template>
						<ContentStyle BackColor="Transparent" />
					</px:PXFormView>

					<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartRow="True" />
					<px:PXFormView ID="edOpportunity_Contact" runat="server" DataMember="LeadCurrent" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule12" runat="server" GroupCaption="Contact" StartGroup="True" />
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True" ControlSize="XM" LabelsWidth="SM" />
								<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" CommitChanges="True" />
								<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" SuppressLabel="False" CommitChanges="True" />
								<px:PXTextEdit ID="edCompanyName" runat="server" DataField="FullName" CommitChanges="True" />
								<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="False" CommitChanges="True" />
								<px:PXMailEdit ID="EMail" runat="server" CommandName="NewMailActivity" CommandSourceID="ds" DataField="EMail" CommitChanges="True" />
								<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
									<px:PXDropDown ID="Phone1Type" runat="server" DataField="Phone1Type" Size="S" SuppressLabel="True" TabIndex="-1" />
									<px:PXLabel ID="lblPhone1" runat="server" Text=" " SuppressLabel="true" />
									<px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule ID="PXLayoutRule14" runat="server" Merge="True" />
									<px:PXDropDown ID="Phone2Type" runat="server" DataField="Phone2Type" Size="S" SuppressLabel="True" TabIndex="-1" />
									<px:PXLabel ID="lblPhone2" runat="server" Text=" " SuppressLabel="true" />
									<px:PXMaskEdit ID="Phone2" runat="server" DataField="Phone2" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />
									<px:PXDropDown ID="Phone3Type" runat="server" DataField="Phone3Type" Size="S" SuppressLabel="True" TabIndex="-1" />
									<px:PXLabel ID="lblPhone3" runat="server" Text=" " SuppressLabel="true" />
									<px:PXMaskEdit ID="Phone3" runat="server" DataField="Phone3" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
									<px:PXDropDown ID="FaxType" runat="server" DataField="FaxType" Size="S" SuppressLabel="True" TabIndex="-1" />
									<px:PXLabel ID="lblFax" runat="server" Text=" " SuppressLabel="true" />
									<px:PXMaskEdit ID="Fax" runat="server" DataField="Fax" SuppressLabel="True" LabelWidth="34px" />
								<px:PXLayoutRule runat="server" />
								<px:PXLinkEdit ID="WebSite" runat="server" DataField="WebSite" CommitChanges="True" />
						</Template>
						<ContentStyle BackColor="Transparent" />
					</px:PXFormView>

					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" />
					<px:PXFormView ID="formA" runat="server" DataMember="AddressCurrent" DataSourceID="ds" SkinID="Transparent" >
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule9" runat="server" GroupCaption="Address" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
								<px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" />
								<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="True" />
								<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="True" />
								<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="True" />
								<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="True" FilterByAllFields="True" TextMode="Search" DataSourceID="ds" />
								<px:PXLayoutRule runat="server" Merge="True" />
								<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" CommitChanges="true" />
								<px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Size="xs" Text="View On Map" Height="20px" TabIndex="-1" />
								<px:PXLayoutRule runat="server" />
								<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
									FilterByAllFields="True" TextMode="Search" CommitChanges="True" DataSourceID="ds" edit="1" />
						</Template>
						<ContentLayout ControlSize="XM" LabelsWidth="SM" OuterSpacing="None" />
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
					</px:PXFormView>
					<px:PXLayoutRule ID="PXLayoutRule16" runat="server" GroupCaption="Personal Data Privacy" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
						<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="ConsentAgreement" AlignLeft="True" CommitChanges="True" TabIndex="-1" />
						<px:PXDateTimeEdit ID="edConsentDate" runat="server" DataField="ConsentDate" CommitChanges="True" />
						<px:PXDateTimeEdit ID="edConsentExpirationDate" runat="server" DataField="ConsentExpirationDate" CommitChanges="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Activities" LoadOnDemand="True">
				<Template>
					<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
						AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
						FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
						PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
						BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
						<ActionBar DefaultAction="cmdViewActivity" CustomItemsGroup="0">
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
									<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
									<px:PXGridColumn DataField="UIStatus" />
									<px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
									<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
									<px:PXGridColumn DataField="TimeSpent" />
									<px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False">
										<NavigateParams>
											<px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
									<px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<PreviewPanelTemplate>
							<px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
								<AutoSize Container="Parent" Enabled="true" />
							</px:PXHtmlView>
						</PreviewPanelTemplate>
						<AutoSize Enabled="true" />
						<GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
					</pxa:PXGridWithPreview>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="CRM Info" RepaintOnDemand="False">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" GroupCaption="CRM" StartColumn="True" />
						<px:PXSelector ID="edClassID" runat="server" AllowEdit="True" DataField="ClassID" FilterByAllFields="True" TextMode="Search" CommitChanges="True" />
						<px:PXSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
						<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
						<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" CommitChanges="True">
							<AutoCallBack Enabled="true" ActiveBehavior="True">
								<Behavior CommitChanges="True" RepaintControls="All" RepaintControlsIDs="form,tab,frmLogin"/>
							</AutoCallBack>
						</px:PXCheckBox>

					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" GroupCaption="Activities"/>
						<px:PXDateTimeEdit ID="edLastIncomingDate" runat="server" DataField="LeadActivityStatistics.LastIncomingActivityDate" Enabled="False" Size="SM" />
						<px:PXDateTimeEdit ID="edLastOutgoingDate" runat="server" DataField="LeadActivityStatistics.LastOutgoingActivityDate" Enabled="False" Size="SM" />

					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" GroupCaption="Contact Preferences" StartColumn="True" />
						<px:PXDropDown ID="Method" runat="server" DataField="Method" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
						<px:PXCheckBox ID="edNoCall" runat="server" DataField="NoCall" Size="S" SuppressLabel="True" Width="112" />
						<px:PXCheckBox ID="edNoMarketingMaterials" runat="server" DataField="NoMarketing" Size="S" />
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True" />
						<px:PXCheckBox ID="edNoEMail" runat="server" DataField="NoEMail" Size="S" SuppressLabel="True" Width="112" />
						<px:PXCheckBox ID="edNoMassMail" runat="server" DataField="NoMassMail" Size="S" Width="112" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
						<px:PXSelector ID="edLanguageID" runat="server" AllowEdit="True" DataField="LanguageID" TextMode="Search" DataSourceID="ds" DisplayMode="Hint" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Duplicates" BindingContext="form" VisibleExp="DataControls[&quot;edDuplicateFound&quot;].Value == true" LoadOnDemand="True">
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
			<px:PXTabItem Text="Relations" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
						ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Relations">
								<Columns>
									<px:PXGridColumn DataField="Role" CommitChanges="True"></px:PXGridColumn>
									<px:PXGridColumn DataField="IsPrimary" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
									<px:PXGridColumn DataField="TargetType" CommitChanges="True"></px:PXGridColumn>
									<px:PXGridColumn DataField="TargetNoteID" DisplayMode="Text" LinkCommand="Relations_TargetDetails" CommitChanges="True"></px:PXGridColumn>
									<px:PXGridColumn DataField="EntityID" AutoCallBack="true" LinkCommand="Relations_EntityDetails" CommitChanges="True"></px:PXGridColumn>
									<px:PXGridColumn DataField="Name"></px:PXGridColumn>
									<px:PXGridColumn DataField="ContactID" AutoCallBack="true" TextAlign="Left" TextField="ContactName" DisplayMode="Text" LinkCommand="Relations_ContactDetails"></px:PXGridColumn>
									<px:PXGridColumn DataField="Email"></px:PXGridColumn>
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
			<px:PXTabItem Text="Campaigns" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdCampaignHistory" runat="server" Height="400px" Width="100%" Style="z-index: 100"
						AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds"
						SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Members">
								<Columns>
									<px:PXGridColumn DataField="CampaignID" AutoCallBack="true" DisplayMode="Value" TextField="CRCampaign__CampaignID" LinkCommand="Members_CRCampaign_ViewDetails" />
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
					<px:PXGrid ID="grdMarketingLists" runat="server" Height="400px" Width="100%"
						AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Subscriptions">
								<Columns>
									<px:PXGridColumn DataField="IsSubscribed" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="MarketingListID" AutoCallBack="true" DisplayMode="Text" TextField="CRMarketingList__MailListCode"
										LinkCommand="Subscriptions_CRMarketingList_ViewDetails" />
									<px:PXGridColumn DataField="CRMarketingList__Name" />
									<px:PXGridColumn DataField="Format" />
									<px:PXGridColumn DataField="CRMarketingList__IsDynamic" Type="CheckBox" TextAlign="Center" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
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
									<px:PXGridColumn DataField="Status" RenderEditorText="True" />
									<px:PXGridColumn DataField="CuryProductsAmount" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryID" />
									<px:PXGridColumn DataField="CloseDate" />
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
									<px:PXGridColumn DataField="ClassID" LinkCommand="Opportunities_CLassID_ViewDetails"  AllowShowHide="true" Visible="false" SyncVisible="false"/>
									<px:PXGridColumn DataField="CROpportunityClass__Description" AllowShowHide="true" Visible="false" SyncVisible="false" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Create Opportunity" Key="cmdOpportunityDetails">
									<AutoCallBack Command="Action@convertToOpportunityAll" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
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
									<px:PXGridColumn DataField="LastErrorMessage" Width="230" />
									<px:PXGridColumn DataField="LastAttemptTS" DisplayFormat="g" />
									<px:PXGridColumn DataField="AttemptCount" />
									<px:PXGridColumn DataField="SFEntitySetup__ImportScenario" Width="150" />
									<px:PXGridColumn DataField="SFEntitySetup__ExportScenario" Width="150" />
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

		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="450" MinWidth="300" />
	</px:PXTab>

	<!--#include file="~\Pages\CR\Includes\CRCreateContactPanel.inc"-->

	<!--#include file="~\Pages\CR\Includes\CRCreateBothContactAndAccountPanel.inc"-->

	<!--#include file="~\Pages\CR\Includes\CRCreateOpportunityAllPanel.inc"-->

	<!--#include file="~\Pages\CR\Includes\CRDuplicateEntityMergePanel.inc"-->

</asp:Content>
