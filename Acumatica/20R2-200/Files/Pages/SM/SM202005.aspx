<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM202005.aspx.cs" Inherits="Page_WikiSet" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">	
	<pxa:DynamicDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.WikiSetupMaint,PX.SM.WikiMaintenance"
		PrimaryView="Wikis">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="processTag" Visible="False" DependOnGrid="gridTags" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			
			<px:PXDSCallbackCommand Name="DITA" PopupVisible="False" Visible="false"/>
			<px:PXDSCallbackCommand Name="importtoDITA" Visible="false"/>
			<px:PXDSCallbackCommand Name="exporttoDITA" Visible="false"/>
			<px:PXDSCallbackCommand Name="exportToDITAPublished" Visible="false"/> 
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
			<px:PXTreeDataMember TreeView="KBWikiPageTree" TreeKeys="PageID" />
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="AllPages" />  	
            <px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />	
		</DataTrees>
		<ClientEvents CommandPerformed="commandResult" />
	</pxa:DynamicDataSource>

	<px:PXUploadFilePanel ID="dlgUploadFile" runat="server" IgnoreSize="True" OnUpload="uploadPanel_Upload" 
		PanelID="pnlUploadFileSmart_DITA" CommandSourceID="ds" AllowedTypes=".zip" Caption="File Upload" CommandName="importToDITA" ViewStateMode="Enabled"/>
	<px:PXSmartPanel ID="pnlDate" runat="server" Style="position: static"
		Width="289px" Key="ClearingFilter" Caption="Choose date" CaptionVisible="True" 
        DesignView="Content">
		<px:PXFormView ID="frmDate" runat="server" CaptionVisible="False" Style="position: static;
			margin-top: 0px;" Width="100%" BorderStyle="None" DataMember="ClearingFilter"
			DataSourceID="ds" TemplateContainer="" SkinID="Transparent">
			<ContentStyle BorderStyle="None">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edTill" runat="server" DataField="Till"
					Size="SM" />
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
		    <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" />
	        <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Caption="Wiki" Width="100%" DataSourceID="ds"
		DataMember="Wikis" TemplateContainer="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" GroupCaption="General" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>
			<px:PXSelector ID="edName" runat="server" AutoRefresh="True" DataField="Name" DataSourceID="ds" />
			<px:PXTextEdit ID="edWikiTitle" runat="server" DataField="WikiTitle" />
			<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False"
			               TextField="Username" DataSourceID="ds" />
			<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" Enabled="False" DisplayFormat="g" Size="M" />			
			<px:PXLayoutRule runat="server" GroupCaption="Approval" />
			<px:PXCheckBox ID="chkHoldEntry" runat="server" DataField="HoldEntry" />
			<px:PXCheckBox CommitChanges="True" ID="chkRequestApproval" runat="server" DataField="RequestApproval" Checked="True" />
			<px:PXTreeSelector CommitChanges="True" Size="m" ID="edApprovalGroupID" runat="server"
				TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds" DataField="ApprovalGroupID"
				PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
				<DataBindings>
					<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
				</DataBindings>
			</px:PXTreeSelector>			
			<px:PXSelector Size="m" ID="edApprovalUserID" runat="server" DataField="ApprovalUserID"
				AutoRefresh="True" TextField="Username" DataSourceID="ds" ValueField="pkID"/>			

			<px:PXLayoutRule runat="server" GroupCaption="Modern UI" StartColumn="True" />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="IsActive" />
			<px:PXDropDown CommitChanges="True" ID="edCategory" runat="server" DataField="Category" SelectedIndex="-1" />			
			<px:PXTextEdit ID="edPosition" runat="server" DataField="Position" />						
			<px:PXTreeSelector ID="edDefaultUrl" runat="server" DataField="DefaultUrl"
			                   PopulateOnDemand="True" ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="AllPages">
				<DataBindings>
					<px:PXTreeItemBinding TextField="Title" ValueField="PageID" />
				</DataBindings>
			</px:PXTreeSelector>
			<px:PXLayoutRule runat="server" GroupCaption="Classic UI"  />			
			<px:PXTreeSelector ID="edScreen" runat="server" DataField="SitemapParent" PopulateOnDemand="True" 
			                   ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="SiteMapTree" MinDropWidth="350" CommitChanges="true">
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMapTree" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" />
				</DataBindings>
				<AutoCallBack Command="Save" Target="form">
				</AutoCallBack>
			</px:PXTreeSelector> 			
			<px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" />
			
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="211px" Style="z-index: 100" Width="100%" DataMember="CurrentWiki">		
		<AutoSize Enabled="True" Container="Window"></AutoSize>
		<Items>
				<px:PXTabItem Text="Wiki settings">
					<Template>
						<px:PXLayoutRule runat="server" GroupCaption="Look and Feel" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>						
						<px:PXSelector CommitChanges="True" ID="edCssID" runat="server" DataField="CssID" />
						<px:PXSelector ID="edCssPrintID" runat="server" DataField="CssPrintID"  />						
						<px:PXDropDown CommitChanges="True" ID="edSPWikiArticleType" runat="server" DataField="SPWikiArticleType"
						               AllowNull="False" SelectedIndex="-1" />
						<px:PXSelector ID="edRootPageID" runat="server" DataField="RootPageID" TextField="Title"
						               AutoRefresh="True" DataSourceID="ds" />
						<px:PXSelector ID="edRootPrintPageID" runat="server" DataField="RootPrintPageID"
						               TextField="Title" AutoRefresh="True" DataSourceID="ds" />
						<px:PXSelector ID="edHeaderPageID" runat="server" DataField="HeaderPageID" TextField="Title"
						               AutoRefresh="True" DataSourceID="ds" />
						<px:PXSelector ID="edFooterPageID" runat="server" DataField="FooterPageID" TextField="Title"
						               AutoRefresh="True" DataSourceID="ds" />

						<px:PXLayoutRule runat="server" GroupCaption="Miscellaneous"/>
						<px:PXSelector ID="edSiteMapTagID" runat="server" DataField="SiteMapTagID" AutoRefresh="True"  />
						<px:PXTextEdit ID="edPubVirtualPath" runat="server" DataField="PubVirtualPath"/>
						<px:PXLayoutRule runat="server" GroupCaption="Dashboard Description" StartColumn="true" ControlSize="L"/>
						<px:PXTextEdit ID="edDescription" runat="server" DataField="WikiDescription" TextMode="MultiLine" Height="75px" Width="315px" SuppressLabel="true" />
					</Template>
				</px:PXTabItem>
				<px:PXTabItem Text="Access Rights">
				<Template>
				    
                    <px:PXGrid ID="gridAccessRights" runat="server" Width="100%" Height="171px" DataSourceID="ds"
						AdjustPageSize="Auto" ActionsPosition="Top" BorderWidth="0px" SkinID="DetailsInTab" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="EntityRoles">
						<Columns>
							<px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="CacheName" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="MemberName" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="RoleName" Width="230px" />
							<px:PXGridColumn AllowUpdate="False" DataField="Guest" Width="70px" Type="CheckBox" TextAlign="Center" />
							<px:PXGridColumn AllowUpdate="False" DataField="RoleDescr" Width="300px" />
						    <px:PXGridColumn AllowNull="False" DataField="RoleRight"/>
						</Columns>
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
                    <%--<px:PXGrid ID="gridAccessRights" runat="server" Width="100%" Height="171px" DataSourceID="ds"
						AdjustPageSize="Auto" ActionsPosition="Top" BorderWidth="0px" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel  DataMember="EntityRoles">
							    <Columns>
								    <px:PXGridColumn DataField="RoleName" Width="108px"/>
								    <px:PXGridColumn AllowNull="False" DataField="AccessRights" DataType="Int16" DefValueText="0"
								                     Width="72px" Type="DropDownList"/>
								    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ParentAccessRights"
								                     DataType="Int16" DefValueText="0" Width="72px"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>--%>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tags">
				<Template>
					<px:PXGrid ID="gridTags" runat="server" Width="100%" DataSourceID="ds" AdjustPageSize="Auto"
						ActionsPosition="Top" BorderWidth="0px" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Tags">
								<Columns>
									<px:PXGridColumn DataField="Description" Width="300px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Process Tag">
									<AutoCallBack Command="processTag" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>			
			<px:PXTabItem Text="Locales" Tooltip="Locales which are available for users without edit rights.">
				<Template>
					<px:PXGrid ID="grdLocales" runat="server" DataSourceID="ds" Height="54px" Width="100%"
						AdjustPageSize="Auto" ActionsPosition="Top" BorderWidth="0px" SkinID="DetailsInTab">
						<Mode AllowAddNew="False" AllowDelete="False" />
						<Levels>
							<px:PXGridLevel DataMember="ReadLangs">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
									<px:PXTextEdit ID="edLanguage" runat="server" DataField="Language" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn DataField="Language" Width="300px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="File Paths" Tooltip="Defines how files attached to specific articles are mapped to web-site physical folders.">
				<Template>
					<px:PXGrid ID="grdFilePaths" runat="server" DataSourceID="ds" Height="54px" Width="100%"
						AdjustPageSize="Auto" ActionsPosition="Top" BorderWidth="0px" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="SitePaths">
								<Columns>
									<px:PXGridColumn DataField="PageName" Width="200px" />
									<px:PXGridColumn DataField="Path" Width="300px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXTreeSelector SuppressLabel="True" ID="edPageName" runat="server" DataField="PageName"
										PopulateOnDemand="True" ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="AllPages">
										<DataBindings>
											<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
										</DataBindings>
									</px:PXTreeSelector>
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" />
						<ActionBar>
							<Actions>
								<NoteShow MenuVisible="false" ToolBarVisible="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>