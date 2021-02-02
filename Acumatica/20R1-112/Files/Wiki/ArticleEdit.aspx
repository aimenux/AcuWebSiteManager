<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="ArticleEdit.aspx.cs" Inherits="Page_ArticleEdit"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds1" runat="server" Visible="True" PrimaryView="Pages"
		TypeName="PX.SM.KBArticleMaint" style="float:left">
		<ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="processTag" Visible="False" DependOnGrid="gridTags" />
		    <px:PXDSCallbackCommand Name="Save" CommitChanges="True" Visible="False"/>
            <px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false"/>
			<px:PXDSCallbackCommand Name="moveUp" Visible="False" DependOnGrid="gridSub" />
			<px:PXDSCallbackCommand Name="moveDown" Visible="False" DependOnGrid="gridSub" />
			<px:PXDSCallbackCommand Name="viewRevision" Visible="False" DependOnGrid="gridHistory" />
			<px:PXDSCallbackCommand Name="publishRevision" Visible="False" DependOnGrid="gridHistory" />
			<px:PXDSCallbackCommand Name="revertToRevision" Visible="False" DependOnGrid="gridHistory" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="compare" Visible="False" />
			<px:PXDSCallbackCommand Name="editFile" Visible="False" DependOnGrid="gridAttachments" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />						
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />		
		</DataTrees>
	</px:PXDataSource>

	<px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" ImageSet="main">
		<Items>
			<px:PXToolBarButton Text="Attach" Tooltip="Attach File to Article" Key="attach" CommandName="AttachFile" CommandSourceID="edtWikiText">
				<Images Normal="main@Files" />
			</px:PXToolBarButton>
		</Items>
		<Layout ItemsAlign="Left" />
	</px:PXToolBar>
	<div style="clear:left" />

	<script type="text/javascript">
		px_callback.baseProcessRedirect = px_callback.processRedirect;
		px_callback.processRedirect = function (result, context)
		{
			var pageID = null;
			if (context == null)
			{
			    var prefix = "Refresh:";
				var split = result.indexOf("|");
				if (result.indexOf(prefix) == 0 && split > 0)
				{
					pageID = result.substring(prefix.length, split);
					result = "Redirect0:" + result.substring(split + 1, result.length);
					result = result.replace("~/", "");
					__refreshMainMenu(pageID);
				}
			}
			px_callback.baseProcessRedirect(result, context);
		}

		function tabChanged()
		{
			window.name = "main";
		}

		function commandResult(ds, context)
		{
			if (context.command == "Save" || context.command == "Delete")
			{
				__refreshMainMenu();
			}
		}
	</script>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlGetLink" runat="server" Caption="File Link" ForeColor="Black"
		Style="position: static" Position="UnderOwner" AutoCallBack-Enabled="True" AutoCallBack-Target="frmGetLink"
		AutoCallBack-Command="Refresh" Height="94px" Width="342px" ShowAfterLoad="true" DesignView="Content">
		<px:PXFormView ID="frmGetLink" runat="server" SkinID="Transparent" DataMember="Links"
			DataSourceID="ds1" Height="64px" Width="342px" DataKeyNames="FileID">
			<Template>
				<px:PXLabel ID="lblExternalLink" runat="server" Style="z-index: 100; left: 9px; position: absolute;
					top: 9px">External Link :</px:PXLabel>
				<px:PXTextEdit ID="edExternalLink" runat="server" DataField="ExternalLink" LabelID="lblExternalLink"
					Style="z-index: 101; left: 102px; position: absolute; top: 9px" TabIndex="10"
					Width="221px">
				</px:PXTextEdit>
				<px:PXLabel ID="lblWikiLink" runat="server" Style="z-index: 102; left: 9px; position: absolute;
					top: 36px">Wiki Link :</px:PXLabel>
				<px:PXTextEdit ID="edWikiLink" runat="server" DataField="WikiLink" LabelID="lblWikiLink"
					Style="z-index: 103; left: 103px; position: absolute; top: 36px" TabIndex="11"
					Width="220px">
				</px:PXTextEdit>
			</Template>
			<CallbackCommands>
				<Refresh RepaintControls="None" RepaintControlsIDs="frmGetLink" />
			</CallbackCommands>
		</px:PXFormView>
		<div style="text-align: right; padding-right: 12px">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="Cancel" Text="Close" Width="80px">
			</px:PXButton>
		</div>
	</px:PXSmartPanel>
	<px:PXTab ID="tab1" runat="server" DataSourceID="ds1" Height="378px" Style="z-index: 100"
		Width="100%" DataKeyNames="PageID" DataMember="Pages" FilesField="NoteFiles"
		NoteField="NoteText" OnDataBinding="tab_DataBinding" RepaintOnDemand="false" NoteIndicator="False" FilesIndicator="False">
		<Items>
			<px:PXTabItem Text="Content">
				<Template>
				    <px:PXPanel ID="panel" runat="server" RenderStyle="Simple" Style="margin-top: 9px; margin-left: 9px;"> 
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXTextEdit ID="edName" runat="server" DataField="Name"/>  
                    <px:PXTextEdit ID="edTitle" runat="server" DataField="Title"/> 
                    <px:PXDropDown ID="chkHtml" runat="server" DataField="VisibleisHtml" CommitChanges="True"/>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" SuppressLabel="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" ColumnSpan="2" />
					<px:PXTreeSelector ID="edParent" runat="server" DataField="ParentUID" PopulateOnDemand="True"
							ShowRootNode="False" TreeDataSourceID="ds1"	TreeDataMember="Folders" >
							<Images>
								<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
							</Images>
							<DataBindings>
								<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
							</DataBindings>
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
					</px:PXTreeSelector>
					<px:PXDropDown ID="edStatusID" runat="server" DataField="OldStatusID" ValueType="Int32" >
						<Items>
							<px:PXListItem Text="On Hold" Value="0" />
							<px:PXListItem Text="Pending" Value="1" />
							<px:PXListItem Text="Rejected" Value="2" />
							<px:PXListItem Text="Published" Value="3" />
							<px:PXListItem Text="Deleted" Value="4" />
						</Items>
					</px:PXDropDown>
					<px:PXDateTimeEdit ID="edPublishedDateTime" runat="server" DataField="PublishedDateTime" Enabled="False"/> 
					<px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved">
						<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
					</px:PXCheckBox>
					<px:PXCheckBox ID="chkRejected" runat="server" DataField="Rejected">
							<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
					</px:PXCheckBox>

					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" SuppressLabel="True" StartColumn="True" ControlSize="M"/>
					<px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" Text="Hold">
						<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
					</px:PXCheckBox>
                    <px:PXCheckBox ID="chkFolder" runat="server" DataField="Folder"/>
				    </px:PXPanel>
                    <px:PXWikiEdit DataSourceID="ds1" ID="edtWikiText" runat="server" DataField="Content" Height="140px" 
						Width="100%" OnFileUploaded="editor_FileUploaded" AttachFileEnabled="True">
						<AutoSize Enabled="True"/>
						<UploadCallBack Enabled="true" Target="gridAttachments" Command="Refresh" />
					</px:PXWikiEdit>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Properties" LoadOnDemand="true">
				<Template>	
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="margin-top: 9px;">
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L"/>
					<px:PXLayoutRule ID="PXLayoutRule8" runat="server" ColumnSpan="2"/>
					<px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" Height="120px" TextMode="MultiLine" />	
					<px:PXTextEdit ID="edKeywords" runat="server" DataField="Keywords" Height="120px" TextMode="MultiLine"/>
					<px:PXCheckBox ID="chkVersioned" runat="server" Checked="True" DataField="Versioned"/> 
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM"/>	
					<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataKeyNames="Username"
						DataMember="_Users_" DataSourceID="ds1" Enabled="False" TextField="Username">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px" Visible="false" AllowShowHide="False">
									<Header Text="Username">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px">
									<Header Text="Username">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="FirstName" MaxLength="255" Width="200px">
									<Header Text="First Name">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="LastName" MaxLength="255" Width="200px">
									<Header Text="Last Name">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="Comment" MaxLength="255" Width="200px">
									<Header Text="Comment">
									</Header>
								</px:PXGridColumn>
							</Columns>
							<Layout ColumnsMenu="False" />
						</GridProperties>
					</px:PXSelector> 
					<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" DisplayFormat="g" Enabled="False" Size="SM"/>
					<px:PXSelector ID="edLastModifiedByID" runat="server" DataField="LastModifiedByID"
						DataKeyNames="Username" DataMember="_Users_" DataSourceID="ds1" Enabled="False" TextField="Username">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px" Visible="false" AllowShowHide="False">
									<Header Text="Username">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px">
									<Header Text="Username">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="FirstName" MaxLength="255" Width="200px">
									<Header Text="First Name">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="LastName" MaxLength="255" Width="200px">
									<Header Text="Last Name">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="Comment" MaxLength="255" Width="200px">
									<Header Text="Comment">
									</Header>
								</px:PXGridColumn>
							</Columns>
							<Layout ColumnsMenu="False" />
						</GridProperties>
					</px:PXSelector>
					<px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime" DisplayFormat="g" Enabled="False" Size="SM"/>
					<px:PXTreeSelector ID="edApprovalGroupID" runat="server" DataField="ApprovalGroupID"
						TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds1" 					
						PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false">												
						<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						<DataBindings>							
							<px:PXTreeItemBinding TextField="Description" ValueField="Description" />														
						</DataBindings>										
					</px:PXTreeSelector>
					<px:PXSelector ID="edApprovalUserID" runat="server" AutoRefresh="True" DataField="ApprovalUserID"
						DataKeyNames="Username" DataMember="_Users_WikiPage.approvalGroupID_WikiPage.approvalGroupID_"
						DataSourceID="ds1" HintField="username" TextField="Username">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px" Visible="false" AllowShowHide="False">
									<Header Text="Username">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px">
									<Header Text="Username">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="FullName" MaxLength="255" Width="200px">
									<Header Text="Full Name">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="Comment" MaxLength="255" Width="200px">
									<Header Text="Comment">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" DataField="IsApproved" DataType="Boolean" Width="60px">
									<Header Text="Account activated">
									</Header>
								</px:PXGridColumn>
							</Columns>
						</GridProperties>
					</px:PXSelector>  
					</px:PXPanel>
					<px:PXPanel ID="panel1" runat="server" RenderStyle="Simple">
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"/>	
					<px:PXLabel ID="lblTags" runat="server" Style="margin-left: 9px; margin-top: 9px;" Width="115px">Version Tags:</px:PXLabel>
					<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"/>	
					<px:PXGrid ID="gridTags" runat="server" AdjustPageSize="Auto" 
						DataSourceID="ds1" Height="63px" Width="590px" AutoAdjustColumns="False" SkinID="ShortList" Style="margin-top: 9px;">
						<Levels>
							<px:PXGridLevel DataKeyNames="WikiID,PageID,Language,PageRevisionID,TagID" DataMember="RevisionTags">
								<Columns>
									<px:PXGridColumn DataField="TagID" Width="555px">
									<Header Text="Tag"/>
									</px:PXGridColumn>
								</Columns>	
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Process Tag">
									<Images Normal="main@Process" />
									<AutoCallBack Command="processTag" Enabled="True" Target="ds1">
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>  
					</px:PXPanel>
				</Template>
			</px:PXTabItem>
            
             <px:PXTabItem Text="Category">
				<Template>	
                    <px:PXGrid ID="gridCategory" runat="server" Height="162px" Style="z-index: 105;
						position: absolute; left: 0px; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataKeyNames="CategoryID" DataMember="Category">
								<Columns>
								    <px:PXGridColumn DataField="CategoryID" Width="300px"/>
                                </Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            
            <px:PXTabItem Text="Product" LoadOnDemand="true">
				<Template>	
                    <px:PXGrid ID="gridProduct" runat="server" Height="162px" Style="z-index: 105;
						position: absolute; left: 0px; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataKeyNames="ProductID" DataMember="Product">
								<Columns>
								    <px:PXGridColumn DataField="ProductID" Width="300px"/>
                                </Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Attachments" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridAttachments" runat="server" Height="162px" Style="z-index: 105;
						position: absolute; left: 0px; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0px" SkinID="Details">
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<Actions>
								<AddNew MenuVisible="False" ToolBarVisible="False" />
								<Delete MenuVisible="False" ToolBarVisible="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton PopupPanel="pnlGetLink" Text="Get Link" DisplayStyle="Text">
									<Images Normal="main@Link" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Edit" DisplayStyle="Text">
									<Images Normal="main@DataEntry" />
									<AutoCallBack Target="ds1" Command="editFile" Enabled="True" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataKeyNames="FileID" DataMember="Attachments">
								<Columns>
									<px:PXGridColumn DataField="Name" Width="300px">
										<Header Text="Name">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Comment" Width="200px">
										<Header Text="Comment">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="RevisionCreatedByID">
										<Header Text="Version by">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="RevisionCreatedDateTime" DataType="DateTime"
										Width="90px">
										<Header Text="Version Time">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Size" DataType="Int32" TextAlign="Right">
										<Header Text="File Size">
										</Header>
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Access Rights" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridAccessRights" runat="server" ActionsPosition="Top" DataSourceID="ds1"
						Width="100%" AdjustPageSize="Auto" Height="171px" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataKeyNames="PageID,ApplicationName,RoleName" DataMember="AccessRights">
								<Columns>
								    <px:PXGridColumn DataField="RoleName" Width="300px"/>
                                    <px:PXGridColumn AllowUpdate="False" DataField="Guest" Width="70px" Type="CheckBox" TextAlign="Center" />
								    <px:PXGridColumn AllowNull="False" DataField="AccessRights" DataType="Int16" DefValueText="0"
								                     RenderEditorText="True" Width="100px"/>
								    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ParentAccessRights"
								                     RenderEditorText="True" DataType="Int16" DefValueText="0" Width="100px"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subarticles" Key="Subarticles" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridSub" runat="server" Height="171px" Style="z-index: 100; left: 0px;
						position: static; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowFilter="False" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Subarticles" DataKeyNames="PageID">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="Folder" DataType="Boolean" DefValueText="False"
										TextAlign="Center" Type="CheckBox" Width="100px" AllowSort="False">
										<Header Text="Folder">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Name" AllowSort="False" Width="200px">
										<Header Text="Article ID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Title" AllowSort="False" Width="300px">
										<Header Text="Article Name">
										</Header>
									</px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Move Up">
									<AutoCallBack Command="moveUp" Enabled="True" Target="ds1">
									</AutoCallBack>
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Move Down">
									<AutoCallBack Command="moveDown" Enabled="True" Target="ds1">
									</AutoCallBack>
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<Search Enabled="False" />
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
								<FilterShow Enabled="False" />
								<FilterSet Enabled="False" />
								<ExportExcel Enabled="False" />
							</Actions>
						</ActionBar>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="History" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridHistory" runat="server" DataSourceID="ds1" Height="200px" Style="z-index: 100;
						left: 0px; position: absolute; top: 0px" Width="100%" ActionsPosition="Top" AllowPaging="True"
						AdjustPageSize="Auto" BorderWidth="0px" SkinID="Details" AutoAdjustColumns="true">
						<Levels>
							<px:PXGridLevel DataKeyNames="PageID,Language,PageRevisionID" DataMember="Revisions">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="Selected" DataType="Boolean" DefValueText="False"
										TextAlign="Center" Type="CheckBox" Width="50px">
										<Header Text="Source">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="SelectedDest" DataType="Boolean" DefValueText="False"
										TextAlign="Center" Type="CheckBox" Width="60px">
										<Header Text="Compare To">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="PageRevisionID" DataType="Int32" DefValueText="0"
										TextAlign="Right">
										<Header Text="Version ID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Width="150px">
										<Header Text="Created by">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" 
										DataType="DateTime" DisplayFormat="g" Width="100px">
										<Header Text="Creation Time">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="ApprovalByID" Width="150px">
										<Header Text="Approval By">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="ApprovalDateTime" DataType="DateTime" DisplayFormat="g"
										Width="110px">
										<Header Text="Approval Time">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Language" Visible="False">
										<Header Text="Language">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Published" DataType="Boolean" TextAlign="Center" Type="CheckBox"
										Width="60px">
										<Header Text="Published">
										</Header>
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Compare" Key="compare" DisplayStyle="Text" Enabled="False">
									<Images Normal="main@Compare" />
									<AutoCallBack Command="compare" Enabled="True" Target="ds1">
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="viewRevision" CommandSourceID="ds1" Text="View Version" DisplayStyle="Text">
									<Images Normal="main@Inquiry" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds1" Text="Publish" CommandName="publishRevision" DisplayStyle="Text">
									<Images Normal="main@SetDefault" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Revert to Version" CommandName="revertToRevision" CommandSourceID="ds1" DisplayStyle="Text">
									<Images Normal="main@Rollback" />
								</px:PXToolBarButton>
							</CustomItems>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

           <px:PXTabItem Text="Responses" Key="Responses" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="gridResp" runat="server" Height="171px" Style="z-index: 100; left: 0px;
						position: static; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowFilter="False" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Responses" DataKeyNames="ResponseID">
								<Columns>
									<px:PXGridColumn DataField="Users__Username" Width="120px"/>										
									<px:PXGridColumn DataField="Date" Width="120px"/>																
									<px:PXGridColumn DataField="RevisionID" Width="80px"/>										
									<px:PXGridColumn DataField="NewMark" Width="80px"/>										
									<px:PXGridColumn DataField="Summary" Width="500px"/>								
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar PagerVisible="False">
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
							<PagerSettings Mode="NextPrevFirstLast" />
						</ActionBar>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<ClientEvents TabChanged="tabChanged" />
		<Searches>
			<px:PXQueryStringParam Name="PageID" OnLoadOnly="True" QueryStringField="PageID"
				Type="String" />
			<px:PXControlParam ControlID="tab1" Name="Name" PropertyName="NewDataKey[&quot;Name&quot;]"
				Type="String" />
		</Searches>
		<AutoSize Container="Window" Enabled="true" />
	</px:PXTab>
</asp:Content>
