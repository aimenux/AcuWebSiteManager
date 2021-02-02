<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR202000.aspx.cs" Inherits="Page_CR202000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CampaignMaint"
		PrimaryView="Campaign">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewCampaignMemberActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="AddAction" Visible="False" />
			<px:PXDSCallbackCommand Name="DeleteAction" Visible="False" />
			<px:PXDSCallbackCommand Name="AddOpportunity" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="AddOpportunityForContact" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="AddContact" Visible="False" />
			<px:PXDSCallbackCommand Name="LinkToContact" Visible="False" />
			<px:PXDSCallbackCommand Name="LinkToBAccount" Visible="False" />
			<px:PXDSCallbackCommand Name="InnerProcess" Visible="false" />
			<px:PXDSCallbackCommand Name="InnerProcessAll" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Campaign Summary" DataMember="Campaign" FilesIndicator="True" NoteIndicator="True"
		ActivityIndicator="False" ActivityField="NoteActivity" DefaultControlID="edCampaignID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector ID="edCampaignID" runat="server" DataField="CampaignID" DataSourceID="ds" Size="SM" />
			<px:PXLayoutRule runat="server" />
			<px:PXSelector ID="edCampaignType" runat="server" AllowNull="False" DataField="CampaignType" AllowEdit="true" CommitChanges="true" Size="SM" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edCampaignName" runat="server" AllowNull="False" DataField="CampaignName" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="SM" />
			<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" /> 

			<px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="true" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="290px" DataSourceID="ds" DataMember="CampaignCurrent">
		<Items>
			<px:PXTabItem Text="Campaign Details">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="margin: 9px;">
                        
                        <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="SM"	ControlSize="SM" />	
                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Planning" StartGroup="True" ControlSize="SM" />
                        <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Size="SM" />
						<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" Size="SM" />
						<px:PXSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID"
							TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
                        <px:PXNumberEdit ID="edExpectedResponse" runat="server" DataField="ExpectedResponse" NullText="0.00" Size="SM" />
						<px:PXNumberEdit ID="edPlannedBudget" runat="server" DataField="PlannedBudget" NullText="0.00" Size="SM" />
                        <px:PXNumberEdit ID="edExpectedRevenue" runat="server" DataField="ExpectedRevenue" NullText="0.00" Size="SM" />
                        <px:PXTextEdit ID="edPromoCodeID" runat="server" AllowNull="False" DataField="PromoCodeID" />

						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="False" LabelsWidth="SM"	ControlSize="S" />	
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Project Accounting Integration" StartGroup="True" />
						<px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" CommitChanges="True" Size="SM" />
						<px:PXSelector ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" CommitChanges="True" Size="SM" OnEditRecord="edProjectTaskID_EditRecord" />
						<px:PXLayoutRule ID="PXLayoutRule9" runat="server" />

                        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM"	ControlSize="S" />	
                        <px:PXLayoutRule ID="PXLayoutRule5" runat="server" GroupCaption="Campaign Statistics" StartGroup="True" />
                        
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXNumberEdit ID="edContacts" runat="server" DataField="CalcCampaignCurrent.Contacts" Enabled="false"/>
						<px:PXNumberEdit ID="edMembersContacted" runat="server" DataField="CalcCampaignCurrent.MembersContacted" Enabled="false" />
						<px:PXNumberEdit ID="edMembersResponded" runat="server" DataField="CalcCampaignCurrent.MembersResponded" Enabled="false" />
					    <px:PXNumberEdit ID="edLeadsGenerated" runat="server" DataField="CalcCampaignCurrent.LeadsGenerated" Enabled="false" />
					    <px:PXNumberEdit ID="edLeadsConverted" runat="server" DataField="CalcCampaignCurrent.LeadsConverted" Enabled="false" />
						<px:PXNumberEdit ID="edOpportunities" runat="server" DataField="CalcCampaignCurrent.Opportunities" Enabled="false" />				
						<px:PXNumberEdit ID="edClosedOpportunities" runat="server" DataField="CalcCampaignCurrent.ClosedOpportunities" Enabled="false" />
						<px:PXNumberEdit ID="edOpportunitiesValue" runat="server" DataField="CalcCampaignCurrent.OpportunitiesValue" NullText="0.00" />
						<px:PXNumberEdit ID="edClosedOpportunitiesValue" runat="server" DataField="CalcCampaignCurrent.ClosedOpportunitiesValue" NullText="0.00" />
					</px:PXPanel>
					<px:PXRichTextEdit ID="edDescription" runat="server" Style="border-width: 0px; width: 100%;" DataField="Description" 
						AllowLoadTemplate="false"  AllowDatafields="false"  AllowMacros="true" AllowSourceMode="true" AllowAttached="true" AllowSearch="true">
						<AutoSize Enabled="True" />						
					</px:PXRichTextEdit>  
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Activities" LoadOnDemand="True">
				<Template>
					<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
						AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
						FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" 
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
									<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" Width="60px" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
									<px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox"  />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="80px" />
									<px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
										SyncVisible="False" SyncVisibility="False" Width="108px">
										<NavigateParams>
											<px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="WorkgroupID" Width="90px" />
									<px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text" />
								    <px:PXGridColumn DataField="BAccountID" Width="90px" LinkCommand="BAccount_ViewDetails"/>
								    <px:PXGridColumn DataField="ContactID" Width="80px"  DisplayMode="Text" LinkCommand="Contact_ViewDetails"/>
                                    <px:PXGridColumn DataField="ProjectID" Width="80px" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="80px" AllowShowHide="true" Visible="false" SyncVisible="false"/>
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

			<px:PXTabItem Text="Members">
				<Template>
					<px:PXGrid ID="grdCampaignMembers" runat="server" SkinID="DetailsInTab" Height="400px" NoteIndicator="false"
						Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top"
						AllowSearch="true" DataSourceID="ds" BorderWidth="0px" SyncPosition="true" MatrixMode="true" AllowFilter="true">
						<Levels>
							<px:PXGridLevel DataMember="CampaignMembers">
								<Columns>
								    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" AllowMove="False"
								                     AllowSort="False" TextAlign="Center" Type="CheckBox" Width="30px" />
									<px:PXGridColumn DataField="Contact__ContactType" />
									<px:PXGridColumn AllowNull="False" DataField="Contact__IsActive" TextAlign="Center"
										Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="CampaignID" Width="81px" Visible="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="ContactID" Width="250px" TextField="Contact__MemberName"
										AutoCallBack="true" DisplayMode="Text" CommitChanges="true" TextAlign="Left" LinkCommand="Contact_ViewDetails"/>
									<px:PXGridColumn DataField="Contact__Salutation" Width="150px" AllowUpdate="False" />
									<px:PXGridColumn DataField="Contact__EMail" Width="150px" AllowUpdate="False" />
									<px:PXGridColumn DataField="Contact__Phone1" Width="150px" AllowUpdate="False" DisplayFormat="+# (###) ###-#### Ext:####" />
									<px:PXGridColumn DataField="Contact__BAccountID" AllowUpdate="False" DisplayFormat="CCCCCCCCCC" Width="100px" DisplayMode="Value" LinkCommand="BAccount_ViewDetails"/>
									<px:PXGridColumn DataField="Contact__FullName" />
								    <px:PXGridColumn AllowNull="False" DataField="OpportunityCreatedCount" Width="72px"/>
									<px:PXGridColumn AllowNull="False" DataField="ActivitiesLogged" Width="72px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="EmailSendCount" Width="72px"/>
									<px:PXGridColumn DataField="Contact__Phone2" DisplayFormat="+#(###) ###-####" Width="140px"
										Visible="false" />
									<px:PXGridColumn DataField="Contact__Phone3" DisplayFormat="+#(###) ###-####" Width="140px"
										Visible="false" />
									<px:PXGridColumn DataField="Contact__Fax" DisplayFormat="+#(###) ###-####" Width="140px"
										Visible="false" />
									<px:PXGridColumn DataField="Contact__WebSite" Width="140px" Visible="false" />
									<px:PXGridColumn DataField="Contact__DateOfBirth" Width="90px" Visible="false" />
									<px:PXGridColumn DataField="Contact__CreatedByID" Width="108px" Visible="false" />
									<px:PXGridColumn DataField="Contact__LastModifiedByID" Width="108px" Visible="false" />
									<px:PXGridColumn DataField="Contact__CreatedDateTime" Width="90px" Visible="false" />
									<px:PXGridColumn DataField="Contact__LastModifiedDateTime" Width="90px" Visible="false" />
									<px:PXGridColumn DataField="Contact__WorkgroupID" Width="90px" Visible="false" />
									<px:PXGridColumn DataField="Contact__OwnerID" Width="90px" />
									<px:PXGridColumn AllowNull="False" DataField="Contact__ClassID" TextAlign="Center"
										Width="60px" Visible="false" />
									<px:PXGridColumn DataField="Contact__Source" Width="54px" Visible="false" />
									<px:PXGridColumn DataField="Contact__Title" Width="54px" Visible="false" />
									<px:PXGridColumn DataField="Contact__FirstName" Width="100px" />
									<px:PXGridColumn DataField="Contact__MidName" Width="100px" />
									<px:PXGridColumn DataField="Contact__LastName" Width="100px" />
									<px:PXGridColumn DataField="Address__AddressLine1" Width="90px" Visible="false" />
									<px:PXGridColumn DataField="Address__AddressLine2" Width="90px" Visible="false" />
									<px:PXGridColumn DataField="Contact__Status" Width="90px" />
									<px:PXGridColumn DataField="Contact__IsNotEmployee" Width="0px" AllowShowHide="Server" />
								    <px:PXGridColumn DataField="CreatedDateTime" AllowUpdate="False" DisplayFormat="g" Width="120px" Visible="false"/>
								</Columns>
                                <RowTemplate>
                                    <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" FilterByAllFields="true" />                                    
                                </RowTemplate>
							</px:PXGridLevel>
						</Levels>
                        <Mode AllowUpload="True"/>
						<ActionBar DefaultAction="cmdViewDoc">
							<Actions>
								<Delete Enabled = "false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Delete selected" Tooltip="Delete Selected Rows"  Key="cmdMultipleDelete" DisplayStyle="Image" ImageKey="RecordDel">
								    <AutoCallBack Command="DeleteAction" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add new members" Key="cmdMultipleInsert">
								    <AutoCallBack Command="AddAction" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddOpportunity" Tooltip="Add New Opportunity" DependOnGrid="grdCampaignMembers" StateColumn="Contact__IsNotEmployee">
									<AutoCallBack Command="AddOpportunityForContact" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddActivity" >
                                    <AutoCallBack Command="NewCampaignMemberActivity" Target="ds" ></AutoCallBack>
                                    <ActionBar />
                                </px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="200" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Generated Leads">
				<Template>
					<px:PXGrid ID="Leads" runat="server" DataSourceID="ds" Height="150px" Width="100%" 
						ActionsPosition="Top" AllowPaging="True" AutoGenerateColumns="AppendDynamic" SkinID="Inquire" ExternalFilter="true">
						<Levels>
							<px:PXGridLevel DataMember="Leads">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="ClassID" Width="100px" LinkCommand="Leads_CRContactClass_ViewDetails" />
									<px:PXGridColumn AllowUpdate="False" DataField="DisplayName" Width="280px" LinkCommand="Leads_ViewDetails">
										<NavigateParams>
											<px:PXControlParam Name="ContactID" ControlID="leads" PropertyName="DataValues[&quot;ContactID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
								    <px:PXGridColumn DataField="Status" Width="90px" />
								    <px:PXGridColumn DataField="Resolution" Width="90px" />
									<px:PXGridColumn DataField="Source" Width="100px" />
									<px:PXGridColumn DataField="FullName" Width="100px"/>
								    <px:PXGridColumn DataField="OwnerID" Width="90px" DisplayMode="Text" />
								    <px:PXGridColumn DataField="WorkgroupID" Width="90px" />

									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="100px" Visible="false" SyncVisible="false" />
									
									<px:PXGridColumn DataField="Title" Width="54px" Visible="False" />
									<px:PXGridColumn DataField="Salutation" Width="160px" />
									<px:PXGridColumn AllowUpdate="False" DataField="ContactID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="EMail" Width="200px" />
									<px:PXGridColumn DataField="Address__AddressLine1" Width="90px" Visible="False" />
									<px:PXGridColumn DataField="Address__AddressLine2" Width="90px" Visible="False" />
									<px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" Width="140px" />
									<px:PXGridColumn DataField="Phone2" DisplayFormat="+#(###) ###-####" Width="140px" />
									<px:PXGridColumn DataField="Phone3" DisplayFormat="+#(###) ###-####" Width="140px" />
									<px:PXGridColumn DataField="Fax" DisplayFormat="+#(###) ###-####" Width="140px" />
									<px:PXGridColumn DataField="WebSite" Width="140px" />
									<px:PXGridColumn DataField="DateOfBirth" Width="90px" />
									<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="108px" />
									<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="108px" />
									<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
									<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
									
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar PagerVisible="False">
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
								<FilterShow Enabled="False" />
								<FilterSet Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Key="cmdAddContact" ImageKey="AddNew" Tooltip="Add New Contact" DisplayStyle="Image">
									<AutoCallBack Command="AddContact" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

            <px:PXTabItem Text="Opportunities" LoadOnDemand="true">
			    <Template>
					<px:PXGrid ID="gridOpportunities" runat="server" AutoGenerateColumns="AppendDynamic" DataSourceID="ds" Height="423px"
						Width="100%" AllowSearch="True" ActionsPosition="Top" SkinID="Inquire">
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Levels>
							<px:PXGridLevel DataMember="Opportunities">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="ClassID" Width="80px" LinkCommand="Opportunities_CROpportunityClass_ViewDetails" />
									<px:PXGridColumn DataField="OpportunityID" Width="100px" LinkCommand="Opportunities_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Subject" Width="351px" />
									<px:PXGridColumn AllowNull="False" DataField="StageID" />
									<px:PXGridColumn AllowNull="False" DataField="CROpportunityProbability__Probability" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="Status" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="Resolution" Width="80px" />
									<px:PXGridColumn AllowNull="False" DataField="CuryProductsAmount" Width="80px" />
									<px:PXGridColumn AllowNull="False" DataField="CuryID" Width="80px" />
									<px:PXGridColumn AllowNull="False" DataField="CloseDate" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="Source" Width="90px" />
									<px:PXGridColumn AllowNull="False" DataField="OwnerID" Width="80px" />
									<px:PXGridColumn AllowNull="False" DataField="BAccountID" Width="100px" LinkCommand="Opportunities_BAccount_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="ContactID" DisplayMode="Text" Width="100px" LinkCommand="Opportunities_Contact_ViewDetails" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar ActionsText="False" DefaultAction="cmdOpportunityDetails" PagerVisible="False">
							<CustomItems>
								<px:PXToolBarButton Key="cmdAddOpportunity" ImageKey="AddNew" Tooltip="Add New Opportunity" DisplayStyle="Image">
									<AutoCallBack Command="AddOpportunity" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Attributes">
			    <Template>
					 <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" AllowResize="True" />
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False" TextField="AttributeID_description" />
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
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
	
    <px:PXSmartPanel ID="pnlCampaignMembers" runat="server" Key="CampaignMembers" LoadOnDemand="true" Width="720px" Height="500px"
        Caption="Add Members" CaptionVisible="true" AutoRepaint="true" DesignView="Content" ShowMaximizeButton="True">
	    
        <px:PXFormView ID="formAddItem" runat="server" CaptionVisible="False" DataMember="Operations" DataSourceID="ds"
            Width="100%" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
				<px:PXDropDown CommitChanges="True" ID="edDataSource" runat="server" DataField="DataSource" AllowNull="false"/>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
				<px:PXLayoutRule runat="server" Merge="True" />
				<px:PXSelector CommitChanges="True" ID="edContactGI" runat="server" DataField="ContactGI" AllowEdit="true" />			
				<px:PXSelector CommitChanges="True" ID="edMarketingListID" runat="server" DataField="SourceMarketingListID" AllowEdit="true" />
				<px:PXLabel ID="Fake" runat="server" Width="40px"/>
				<px:PXDropDown CommitChanges="True" ID="edSharedGIFilter" runat="server" DataField="SharedGIFilter" AutoRefresh="true" />
			</Template>
		</px:PXFormView>		

		<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Style="border-width: 1px 0px; top: 0px; left: 0px; height: 189px;"
            AutoAdjustColumns="true" Width="100%" SkinID="Inquire" AdjustPageSize="Auto" AllowSearch="True" MatrixMode="true" SyncPosition="true" SycPositionWithGraph="true">
			<Levels>
				<px:PXGridLevel DataMember="Items">
					<Columns>
                        <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="true"
                            AllowCheckAll="true" CommitChanges="true" />
						<px:PXGridColumn DataField="ContactType" Width="60px" />
						<px:PXGridColumn DataField="DisplayName" Width="130px" LinkCommand="LinkToContact" AutoCallBack="true" CommitChanges="true" />
						<px:PXGridColumn DataField="Title" Width="50px"  Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="FirstName" Width="100px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="MidName" Width="100px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="LastName" Width="100px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Salutation" Width="180px" />
						<px:PXGridColumn DataField="FullName" Width="200px" />
						<px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox" />
						<px:PXGridColumn DataField="EMail" Width="190px" />
						<px:PXGridColumn DataField="ClassID" Width="90px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Status" Width="90px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Source" Width="90px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
						<px:PXGridColumn DataField="BAccountID" DisplayMode="Text" AllowUpdate="False" DisplayFormat="CCCCCCCCCC" Width="100px" LinkCommand="LinkToBAccount"  />
						<px:PXGridColumn DataField="Phone2" DisplayFormat="+#(###) ###-####" Width="130px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone3" DisplayFormat="+#(###) ###-####" Width="130px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Fax" DisplayFormat="+#(###) ###-####" Width="130px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="WorkgroupID" Width="90px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="OwnerID" Width="90px" DisplayMode="Text" Visible="false" SyncVisible="false"/>
						<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="100px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="CreatedDateTime" Width="90px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="100px" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" Visible="false" SyncVisible="false" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
			<ActionBar PagerVisible="False">
                <PagerSettings Mode="NextPrevFirstLast" />
				<CustomItems>
					<px:PXToolBarButton Key="cmdShowDetails" Visible="false" DisplayStyle="Image">
						<AutoCallBack Target="ds" Command="LinkToContact" />
						<Images Normal="main@DataEntry" />
						<ActionBar GroupIndex="0" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Key="cmdShowBAccountDetails" Visible="false">
						<AutoCallBack Target="ds" Command="LinkToBAccount" />
						<Images Normal="main@DataEntry" />
						<ActionBar GroupIndex="0" />
					</px:PXToolBarButton>
				</CustomItems>
				<Actions>
					<FilterShow Enabled="False" />
					<FilterSet Enabled="False" />
				</Actions>
			</ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" />
			<CallbackCommands>
				<Save PostData="Page" />
			</CallbackCommands>
		</px:PXGrid>

		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" CommandName="InnerProcess" CommandSourceID="ds" DialogResult="OK" Text="Process" SyncVisible="false" />
            <px:PXButton ID="PXButton2" runat="server" CommandName="InnerProcessAll" CommandSourceID="ds" DialogResult="OK" Text="Process All" SyncVisible="false" />
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>

    </px:PXSmartPanel>

</asp:Content>


