<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR308000.aspx.cs" Inherits="Page_CR308000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="MassMails"
		TypeName="PX.Objects.CR.CRMassMailMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PostData="Page" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="previewMail" CommitChanges="true" PostData="Page" />
			<px:PXDSCallbackCommand Name="send" CommitChanges="true" SelectControlsIDs="tab" />	
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="true" />		
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="Key" TreeView="EntityItems" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="MassMails" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="false"
		ActivityField="NoteActivity" LinkIndicator="true" NotifyIndicator="true" 
		DefaultControlID="edMassMailCD">
		<AutoSize Container="Window" Enabled="True" />
		<Items>
			<px:PXTabItem Text="Summary">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server">
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXSelector ID="edMassMailCD" runat="server" DataField="MassMailCD" />
						<px:PXSelector ID="edMailFrom" runat="server" DataField="MailAccountID" DisplayMode="Text" />
						<px:PXTreeSelector ID="edMailSubject" runat="server" DataField="MailSubject" TreeDataSourceID="ds"
							TreeDataMember="EntityItems" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false"
							MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true" AppendSelectedValue="true"
							AutoRefresh="true">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
						</px:PXTreeSelector>
						<px:PXTreeSelector ID="edMailTo" runat="server" DataField="MailTo" TreeDataSourceID="ds"
							TreeDataMember="EntityItems" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false"
							MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true" AppendSelectedValue="true"
							AutoRefresh="true">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
						</px:PXTreeSelector>
						<px:PXTreeSelector ID="edMailCc" runat="server" DataField="MailCc" TreeDataSourceID="ds"
							TreeDataMember="EntityItems" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false"
							MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true" AppendSelectedValue="true"
							AutoRefresh="true">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
						</px:PXTreeSelector>
						<px:PXTreeSelector ID="edMailBcc" runat="server" DataField="MailBcc" TreeDataSourceID="ds"
							TreeDataMember="EntityItems" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false"
							MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true" AppendSelectedValue="true"
							AutoRefresh="true">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
						</px:PXTreeSelector>
						<px:PXLayoutRule ID="PXLayoutRule25" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXDropDown CommitChanges="True" ID="edSource" runat="server" AllowNull="False"
							DataField="Source" />
						<px:PXDateTimeEdit ID="edPlannedDate" runat="server" DataField="PlannedDate" />
						<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status"
							Size="S" />
						<px:PXDateTimeEdit ID="edSentDateTime" runat="server" DataField="SentDateTime" DisplayFormat="g"
							EditFormat="g" Enabled="False" Size="M"/>
					</px:PXPanel>
					<px:PXRichTextEdit ID="wikiEdit" runat="server" Style="border-width: 0px; width: 100%;" DataField="MailContent" FilesContainer="message" AllowImageEditor="true"
						AllowLinkEditor="true" AllowLoadTemplate="false" AllowInsertParameter="true" AllowAttached="true" AllowSearch="true" AllowMacros="true" AllowSourceMode="true"
						DatafieldPreviewGraph="PX.Objects.CR.ContactMaint" DatafieldPreviewView="Contact" >
						<InsertDatafield DataSourceID="ds" DataMember="EntityItems" TextField="Name" ValueField="Path"
							ImageField="Icon" />
						<AutoSize Enabled="True" />
						<LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
					</px:PXRichTextEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Leads/Contacts/Employees" VisibleExp="DataControls[&quot;edSource&quot;].Value == 2"
				LoadOnDemand="true">
				<AutoCallBack Command="Refresh" Target="leads" />
				<Template>
					<px:PXFilterEditor ID="mainFilter" runat="server" SkinID="External" FilterView="Leads$FilterHeader" FilterRowsView="Leads$FilterRow"
						DataSourceID="ds" Width="600px" Style="position: relative" LinkedGridID="leads" ShowDefaultFilter="true">
					</px:PXFilterEditor>
					<px:PXGrid ID="leads" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
						position: relative; border-bottom: 0px; border-right: 0px; border-left: 0px;"
						Width="100%" ActionsPosition="Top" AllowPaging="True" SkinID="Inquire" ExternalFilter="true">
						<Levels>
							<px:PXGridLevel DataMember="Leads">
								<Columns>
									<px:PXGridColumn AllowCheckAll="True" AllowNull="False" AllowShowHide="False" DataField="Selected"
										TextAlign="Center" Type="CheckBox" Width="40px" />
									<px:PXGridColumn DataField="ContactType"/>
									<px:PXGridColumn AllowUpdate="False" DataField="DisplayName">
										<NavigateParams>
											<px:PXControlParam Name="ContactID" ControlID="leads" PropertyName="DataValues[&quot;ContactID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="FullName" />
									<px:PXGridColumn DataField="BAccount__ClassID" Visible="False" />
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" Visible="False" />
									<px:PXGridColumn AllowNull="False" DataField="ClassID" RenderEditorText="True" TextAlign="Center"
										Width="60px" Visible="False" />
									<px:PXGridColumn DataField="Source" Visible="False" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="Title" Visible="False" />
									<px:PXGridColumn DataField="Salutation" />
									<px:PXGridColumn AllowUpdate="False" DataField="ContactID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="EMail" />
									<px:PXGridColumn DataField="Address__AddressLine1" Visible="False" />
									<px:PXGridColumn DataField="Address__AddressLine2" Visible="False" />
									<px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" />
									<px:PXGridColumn DataField="Phone2" DisplayFormat="+#(###) ###-####" />
									<px:PXGridColumn DataField="Phone3" DisplayFormat="+#(###) ###-####" />
									<px:PXGridColumn DataField="Fax" DisplayFormat="+#(###) ###-####" />
									<px:PXGridColumn DataField="WebSite" />
									<px:PXGridColumn DataField="DateOfBirth" />
									<px:PXGridColumn DataField="CreatedByID_Creator_Username" />
									<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" />
									<px:PXGridColumn DataField="CreatedDateTime" />
									<px:PXGridColumn DataField="LastModifiedDateTime" />
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
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
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Campaigns" VisibleExp="DataControls[&quot;edSource&quot;].Value == 1"
				LoadOnDemand="true">
				<AutoCallBack Command="Refresh" Target="campaigns" />
				<Template>
					<px:PXGrid ID="campaigns" runat="server" DataSourceID="ds" Style="z-index: 105;"
						Width="100%" BorderStyle="None" AdjustPageSize="Auto" BorderWidth="0px" SkinID="Inquire">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="Campaigns">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox"
										Width="60px"/>
									<px:PXGridColumn AllowUpdate="False" DataField="CampaignID"/>
									<px:PXGridColumn AllowUpdate="False" DataField="CampaignName"/>
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CampaignType" />
									<px:PXGridColumn DataField="SendFilter" Type="DropDownList" />									
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Marketing List" VisibleExp="DataControls[&quot;edSource&quot;].Value == 0"
				LoadOnDemand="true">
				<AutoCallBack Command="Refresh" Target="mailList" />
				<Template>
					<px:PXGrid ID="mailList" runat="server" DataSourceID="ds" Style="z-index: 105;" Width="100%"
						BorderStyle="None" AdjustPageSize="Auto" BorderWidth="0px" SkinID="Inquire">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="MailLists">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox"
										Width="60px" AllowCheckAll="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="MailListCode" DisplayFormat="&gt;aaaaaaaaaa"
										Width="200px" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Name" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Messages" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="mailHistory" runat="server" DataSourceID="ds" Style="z-index: 105;"
						Width="100%" BorderStyle="None" AdjustPageSize="Auto" ActionsPosition="Top" BorderWidth="0px"
						SkinID="Details" SyncPosition="true">
						<AutoSize Enabled="True" />
						<ActionBar DefaultAction="cmdMessageDetails">							
							<Actions>
								<AddNew Enabled="false" />
								<EditRecord Enabled="false" />
								<Delete Enabled="false" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="History">
								<Columns>
									<px:PXGridColumn DataField="Subject" LinkCommand="History_ViewDetails"/>
									<px:PXGridColumn DataField="MailTo"/>
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g"/>
									<px:PXGridColumn DataField="MPStatus"/>
									<px:PXGridColumn DataField="Source" AutoCallBack="true" TextAlign="Left" DisplayMode="Text" LinkCommand="ViewEntity"/>
									<px:PXGridColumn DataField="DocumentSource" AutoCallBack="true" TextAlign="Left" DisplayMode="Text" LinkCommand="ViewDocument"/>
									<px:PXGridColumn DataField="ContactID" AutoCallBack="true" TextAlign="Left" DisplayMode="Text" LinkCommand="History_Contact_ViewDetails"/>
									<px:PXGridColumn DataField="BAccountID" AutoCallBack="true" TextAlign="Left" DisplayMode="Text" LinkCommand="History_BAccount_ViewDetails"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowDelete="false" AllowAddNew="false" AllowUpdate="false" />
						<AutoSize Enabled="true" />
					</px:PXGrid>
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
									<px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
									<px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox"  />
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
									<px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false"/>
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
		</Items>
	</px:PXTab>
	<px:PXSmartPanel ID="pnlEmailPreview" runat="server" Height="117px" Width="378px" Style="z-index: 108; left: 315px; position: absolute; top: 399px" Caption="Preview Message"
		CaptionVisible="true" DesignView="Content" LoadOnDemand="true" Key="Preview"
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmEmailPreview">
		<px:PXFormView ID="frmEmailPreview" runat="server" DataSourceID="ds" Style="z-index: 100;"
			Width="100%" SkinID="Transparent" DataMember="Preview">
			<Template>
				<px:PXLayoutRule ID="smLayoutRule" runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="XM"/>
				<px:PXSelector ID="edMailFrom" runat="server" DataField="MailAccountID" DisplayMode="Text" />
				<px:PXTextEdit ID="edMailTo" runat="server" DataField="MailTo" />
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons" Style="margin-right: 25px;">
			<px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK"  Width="63px" Height="20px"/>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel"  Width="63px" Height="20px" Style="margin-left: 5px"/>
        </px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
