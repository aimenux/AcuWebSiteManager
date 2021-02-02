<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR101000.aspx.cs" Inherits="Page_CR101000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="CRSetupRecord"
		TypeName="PX.Objects.CR.CRSetupMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" ContainerID="tab" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Articles" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="643px" DataSourceID="ds" DataMember="CRSetupRecord"
		LoadOnDemand="True" EmailingGraph="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Setup Preferences">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" GroupCaption="Numbering Sequence" StartGroup="True" />
					<px:PXSelector ID="edOpportunityNumberingID" runat="server" DataField="OpportunityNumberingID"
						AllowEdit="True" />
					<px:PXSelector ID="edCaseNumberingID" runat="server" DataField="CaseNumberingID"
						AllowEdit="True" />
					<px:PXSelector ID="edMassMailNumberingID" runat="server" DataField="MassMailNumberingID"
						AllowEdit="True" />
					<px:PXSelector ID="edCampaignNumberingID" runat="server" DataField="CampaignNumberingID"
						AllowEdit="True" />	
					<px:PXLayoutRule runat="server" GroupCaption="Classes" StartGroup="True" />
					<px:PXSelector ID="edDefaultLeadClassID" runat="server" DataField="DefaultLeadClassID"
						AllowEdit="True" />
					<px:PXSelector ID="edDefaultCustomerClassID" runat="server" DataField="DefaultCustomerClassID"
						AllowEdit="True" />	
					<px:PXSelector ID="edDefaultOpportunityClassID" runat="server" DataField="DefaultOpportunityClassID"
						AllowEdit="True" />
					<px:PXSelector ID="edDefaultCaseClassID" runat="server" DataField="DefaultCaseClassID"
						AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False" />
						</GridProperties>
					</px:PXSelector>
					<px:PXLayoutRule runat="server" GroupCaption="Assignment Map" StartGroup="True" />
					<px:PXSelector ID="edLeadDefaultAssignmentMapID" runat="server" DataField="LeadDefaultAssignmentMapID"
						TextField="Name" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXSelector ID="edContactDefaultAssignmentMapID" runat="server" DataField="ContactDefaultAssignmentMapID"
						TextField="Name" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector Size="xm" ID="edDefaultBAccountAssignmentMapID" runat="server" DataField="DefaultBAccountAssignmentMapID"
						TextField="Name" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXCheckBox ID="chkCopyOwnershipFromLead" runat="server" DataField="BAccountCopyOwnership" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector Size="xm" ID="edDefaultOpportunityAssignmentMapID" runat="server"
						DataField="DefaultOpportunityAssignmentMapID" TextField="Name" AllowEdit="True" />
					<px:PXCheckBox ID="chkCopyOnwershipFromCustomer" runat="server" DataField="OpportunityCopyOwnership" />
					<px:PXLayoutRule runat="server" />
					<px:PXSelector ID="edDefaultCaseAssignmentMapID" runat="server" DataField="DefaultCaseAssignmentMapID"
						TextField="Name" AllowEdit="True" />
					
					<px:PXLayoutRule runat="server" GroupCaption="General" StartGroup="True" />
					<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
						<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="SM" StartColumn="True"/>  
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
						<px:PXCheckBox ID="chkVerifyActivitySource" runat="server" DataField="VerifyActivitySource"/>
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" /> 

						<px:PXLayoutRule ID="PXLayoutRule1" runat="server" SuppressLabel="True"/>
						<px:PXCheckBox ID="chkNotificationForNewCase" runat="server" CommitChanges="True" DataField="NotificationForNewCase"/>
						<px:PXTreeSelector ID="edCaseNotificationPageID" runat="server" CommitChanges="True"
							DataField="CaseNotificationPageID" InitialExpandLevel="0" PopulateOnDemand="True"
							ShowRootNode="False" TreeDataMember="Articles" TreeDataSourceID="ds" SuppressLabel="False" Size="XM">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Title" ValueField="PageID" />
							</DataBindings>
						</px:PXTreeSelector>
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
						
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" SuppressLabel="True" ControlSize="M"/>
						<px:PXCheckBox ID="lblShowEmailAuthors" runat="server" DataField="ShowAuthorForCaseEmail" Size="M"/>
						<px:PXCheckBox ID="chkCopyNotes" runat="server" Checked="True" DataField="CopyNotes" Size="M"/>
						<px:PXLayoutRule ID="PXLayoutRule9" runat="server" />
						
						<px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True" SuppressLabel="True" ControlSize="M"/>
						<px:PXCheckBox ID="edRestrictEmailRecipients" runat="server" DataField="RestrictEmailRecipients" Size="M"/>
						<px:PXCheckBox ID="chkCopyFiles" runat="server" Checked="True" DataField="CopyFiles" Size="M"/>	
						<px:PXLayoutRule ID="PXLayoutRule10" runat="server" />
						
						<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
						<px:PXSelector ID="edDefaultRateTypeID" runat="server" AllowEdit="True" DataField="DefaultRateTypeID" Size="sM">
							<GridProperties>
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSelector>
						<px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate">
						</px:PXCheckBox>
						<px:PXLayoutRule runat="server" />
						
						<px:PXLayoutRule runat="server" Merge="True" />
						<px:PXSelector ID="edDefaultCuryID" runat="server" AllowEdit="True" DataField="DefaultCuryID"
							Size="sM">
							<GridProperties>
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSelector>
						<px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury">
						</px:PXCheckBox>
						<px:PXLayoutRule runat="server" />
					</px:PXPanel>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Campaign Types">
				<Template>
					&nbsp;
					<px:PXGrid ID="gridCampaignType" SkinID="Details" runat="server" ActionsPosition="Top"
						AllowSearch="True" DataSourceID="ds" Height="100px" Style="z-index: 283; left: 0px;
						position: absolute; top: 0px" Width="100%" BorderWidth="0px">
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Levels>
							<px:PXGridLevel DataMember="CampaignType" DataKeyNames="TypeID">
								<Columns>
									<px:PXGridColumn DisplayFormat="&gt;aaaaaaaaaa" DataField="TypeID" />
									<px:PXGridColumn DataField="Description" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Opportunity Probabilities">
				<Template>
					&nbsp;&nbsp;
					<px:PXGrid ID="gridOppoturnityProbabilities" runat="server" SkinID="Details" ActionsPosition="Top"
						AllowSearch="True" DataSourceID="ds" Height="100px" Style="z-index: 100; left: 0px;
						position: absolute; top: 0px" Width="100%" BorderWidth="0px">
						<Levels>
							<px:PXGridLevel DataMember="OpportunityProbabilities" DataKeyNames="StageCode">
								<Columns>
									<px:PXGridColumn TextAlign="Left" DataField="StageCode" Type="DropDownList" />
									<px:PXGridColumn TextAlign="Right" DataField="Probability" AllowNull="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="TFS Integration">
				<Template>
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" GroupCaption="Connection" StartGroup="True" />
					    <px:PXTextEdit ID="edTfsUrl" runat="server" DataField="TfsUrl"/> 
					    <px:PXTextEdit ID="edTfsUsername" runat="server" DataField="TfsUsername"/>
                        <px:PXTextEdit ID="edTfsPassword" runat="server" DataField="TfsPassword" TextMode="Password"/>
                        <px:PXDateTimeEdit ID="edLastSyncTime" runat="server" DisplayFormat="g" DataField="LastSyncTime" Size="SM"/>
                        <px:PXSelector ID="edTfsAttributeID" runat="server" AllowEdit="True" DataField="TfsAttributeID" Size="SM"></px:PXSelector>
				</Template>
			</px:PXTabItem>
			<%--<px:PXTabItem Text="Shortcuts">
				<Template>
					<px:PXGrid ID="gridShortcuts" runat="server" SkinID="Details" ActionsPosition="Top"
						AllowSearch="True" DataSourceID="ds" Height="100px" Style="z-index: 100; left: 0px;
						position: absolute; top: 0px" Width="100%" BorderWidth="0px">
						<Levels>
							<px:PXGridLevel DataMember="Shortcuts" DataKeyNames="Command">
								<Columns>
									<px:PXGridColumn TextAlign="Left" DataField="DisplayName" AllowUpdate="False" />
									<px:PXGridColumn TextAlign="Left" DataField="KeysDescription" AllowNull="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>--%>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
