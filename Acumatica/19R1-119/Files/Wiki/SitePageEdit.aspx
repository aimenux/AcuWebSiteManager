<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SitePageEdit.aspx.cs" Inherits="Page_SitePageEdit"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds1" runat="server" Visible="True" PrimaryView="Pages"
		TypeName="PX.SM.WikiSitePageMaintenance" style="float:left">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="processTag" Visible="False" DependOnGrid="gridTags" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false" />
			<px:PXDSCallbackCommand Name="moveUp" Visible="False" DependOnGrid="gridSub" />
			<px:PXDSCallbackCommand Name="moveDown" Visible="False" DependOnGrid="gridSub" />
			<px:PXDSCallbackCommand Name="viewRevision" Visible="False" DependOnGrid="gridHistory" />
			<px:PXDSCallbackCommand Name="publishRevision" Visible="False" DependOnGrid="gridHistory" />
			<px:PXDSCallbackCommand Name="revertToRevision" Visible="False" DependOnGrid="gridHistory" />
			<px:PXDSCallbackCommand Name="compare" Visible="False" />
			<px:PXDSCallbackCommand Name="editFile" Visible="False" DependOnGrid="gridAttachments" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>

	<px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation">
		<Items>
			<px:PXToolBarSeperator />
			<px:PXToolBarButton Text="Attach" Key="attach" CommandName="AttachFile" CommandSourceID="edtWikiText">
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
			if (context == null)
			{
				var prefix = "Refresh:";
				var split = result.indexOf("|");
				if (result.startsWith(prefix) && split > 0)
				{
					var pageID = result.substring(prefix.length, split - 1);
					result = "Redirect0:" + result.substring(split + 1, result.length);
					result = result.replace("~", "..");
					__refreshMainMenu();
					__syncTitleTree(pageID);
				}
			}
			px_callback.baseProcessRedirect(result, context);
		}
		/*
		PXBaseDataSource.prototype.callbackResult = function(context)
		{
			if (context.command == "Save" || context.command == "Delete")
			{
				__refreshMainMenu();
			}
		}*/
	</script>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlGetLink" runat="server" Caption="File Link" ForeColor="Black"
		Style="position: static" Position="UnderOwner" AutoCallBack-Enabled="True" AutoCallBack-Target="frmGetLink"
		AutoCallBack-Command="Refresh" Height="94px" Width="342px" ShowAfterLoad="true"
		DesignView="Content">
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
	<px:PXTab ID="tab1" runat="server" DataSourceID="ds1" Height="432px" Style="z-index: 100"
		Width="100%" DataKeyNames="PageID" DataMember="Pages" FilesField="NoteFiles"
		NoteField="NoteText" OnDataBinding="tab_DataBinding">
		<Items>
			<px:PXTabItem Text="Content">
				<Template>
					<div style="position: relative; height: 92px; width: 100%; left: 0px; top: 0px;">
						<px:PXLabel ID="lblName" runat="server" Style="z-index: 100; left: 9px; position: absolute;
							top: 9px">Article ID :</px:PXLabel>
						<px:PXTextEdit ID="edName" runat="server" DataField="Name" LabelID="lblName" Style="z-index: 101;
							left: 99px; position: absolute; top: 9px" TabIndex="1" Width="207px">
						</px:PXTextEdit>
						<px:PXLabel ID="lblTitle" runat="server" Style="z-index: 102; left: 9px; position: absolute;
							top: 63px">Article Name :</px:PXLabel>
						<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" LabelID="lblTitle" Style="z-index: 103;
							left: 99px; position: absolute; top: 63px" TabIndex="5" Width="333px">
						</px:PXTextEdit>
						<px:PXCheckBox ID="chkFolder" runat="server" DataField="Folder" Style="z-index: 104;
							left: 324px; position: absolute; top: 9px" TabIndex="2" Text="Folder">
						</px:PXCheckBox>
						<px:PXLabel ID="lblCategoryID" runat="server" Style="z-index: 105; left: 9px; position: absolute;
							top: 36px">Category :</px:PXLabel>
						<px:PXSelector ID="edCategoryID" runat="server" DataField="CategoryID" DataKeyNames="WikiID,Description"
							DataMember="_WikiPageCategory_WikiPage.wikiID_" DataSourceID="ds1" LabelID="lblCategoryID"
							Style="z-index: 106; left: 99px; position: absolute; top: 36px" TabIndex="3"
							Width="213px" ValueField="Description">
							<GridProperties>
								<Columns>
									<px:PXGridColumn DataField="Description" Width="300px">
										<Header Text="Description">
										</Header>
									</px:PXGridColumn>
								</Columns>
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSelector>
						&nbsp;&nbsp;
						<px:PXLabel ID="lblStatusID" runat="server" Style="z-index: 109; left: 477px; position: absolute;
							top: 36px">Status :</px:PXLabel>
						<px:PXDropDown ID="edStatusID" runat="server" DataField="OldStatusID" LabelID="lblStatusID"
							Style="z-index: 110; left: 567px; position: absolute; top: 36px" TabIndex="7"
							ValueType="Int32" Width="90px">
							<Items>
								<px:PXListItem Text="Hold" Value="0" />
								<px:PXListItem Text="Pending" Value="1" />
								<px:PXListItem Text="Rejected" Value="2" />
								<px:PXListItem Text="Published" Value="3" />
								<px:PXListItem Text="Deleted" Value="4" />
							</Items>
						</px:PXDropDown>
						<px:PXLabel ID="lblParentUID" runat="server" Style="z-index: 111; left: 470px; position: absolute;
							top: 9px" Width="81px">Parent Folder :</px:PXLabel>
						<px:PXLabel ID="lblPublishedDateTime" runat="server" Style="z-index: 113; left: 477px;
							position: absolute; top: 63px">Published Date :</px:PXLabel>
						<px:PXDateTimeEdit ID="edPublishedDateTime" runat="server" DataField="PublishedDateTime"
							Enabled="False" LabelID="lblPublishedDateTime" Style="z-index: 114; left: 567px;
							position: absolute; top: 63px" TabIndex="9" Width="90px">
						</px:PXDateTimeEdit>
						<px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" Style="z-index: 115;
							left: 666px; position: absolute; top: 36px" TabIndex="8" Text="Hold">
							<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
						</px:PXCheckBox>
						<px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" Style="z-index: 116;
							left: 666px; position: absolute; top: 63px" TabIndex="10" Text="Approved">
							<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
						</px:PXCheckBox>
						<px:PXCheckBox ID="chkRejected" runat="server" DataField="Rejected" Style="z-index: 117;
							left: 738px; position: absolute; top: 63px" TabIndex="11" Text="Rejected">
							<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
						</px:PXCheckBox>
						<px:PXTreeSelector ID="edParent" runat="server" DataField="ParentUID" PopulateOnDemand="True"
							ShowRootNode="False" Style="left: 567px; position: absolute; top: 8px" TreeDataSourceID="ds1"
							Width="189px" TabIndex="6" TreeDataMember="Folders" LabelID="lblParentUID">
							<Images>
								<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
							</Images>
							<DataBindings>
								<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
							</DataBindings>
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						</px:PXTreeSelector>
					</div>
					<px:PXWikiEdit ID="edtWikiText" runat="server" DataField="Content" Height="140px"
						Width="100%" TabIndex="20">
						<AutoSize Enabled="True" />
						<UploadCallBack Enabled="true" Target="gridAttachments" Command="Refresh" />
					</px:PXWikiEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Properties" LoadOnDemand="true">
				<Template>
					<px:PXLabel ID="lblSummary" runat="server" Style="z-index: 100; left: 9px; position: absolute;
						top: 171px">Summary :</px:PXLabel>
					<px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" Height="45px" LabelID="lblSummary"
						Style="z-index: 101; left: 108px; position: absolute; top: 171px" TabIndex="120"
						TextMode="MultiLine" Width="625px">
					</px:PXTextEdit>
					<px:PXLabel ID="lblDescription" runat="server" Style="z-index: 100; left: 9px; position: absolute;
						top: 234px">Description :</px:PXLabel>
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Height="45px"
						LabelID="lblDescription" Style="z-index: 101; left: 108px; position: absolute;
						top: 234px;" TabIndex="130" TextMode="MultiLine" Width="625px">
					</px:PXTextEdit>
					<px:PXLabel ID="lblKeywords" runat="server" Style="z-index: 102; left: 9px; position: absolute;
						top: 9px">Keywords :</px:PXLabel>
					<px:PXTextEdit ID="edKeywords" runat="server" DataField="Keywords" LabelID="lblKeywords"
						Style="z-index: 103; left: 108px; position: absolute; top: 9px" TabIndex="10"
						Width="337px" Height="63px" TextMode="MultiLine">
					</px:PXTextEdit>
					<px:PXLabel ID="lblFormUrl" runat="server" Style="z-index: 102; left: 9px; position: absolute;
						top: 117px">Form Url :</px:PXLabel>
					<px:PXTextEdit ID="edFormUrl" runat="server" DataField="FormUrl" LabelID="lblFormUrl"
						Style="z-index: 103; left: 108px; position: absolute; top: 117px" TabIndex="60"
						Width="126px">
					</px:PXTextEdit>
					<px:PXCheckBox ID="chkVersioned" runat="server" Checked="True" DataField="Versioned"
						Style="z-index: 104; left: 108px; position: absolute; top: 90px" TabIndex="20"
						Text="Versioned">
					</px:PXCheckBox>
					<px:PXCheckBox ID="chkIndexed" runat="server" Checked="True" DataField="Indexed"
						Style="z-index: 104; left: 189px; position: absolute; top: 90px" TabIndex="30"
						Text="Indexed">
					</px:PXCheckBox>
					<px:PXCheckBox ID="chkExpanded" runat="server" Checked="True" DataField="Expanded"
						Style="z-index: 104; left: 261px; position: absolute; top: 90px" TabIndex="40"
						Text="Expanded">
					</px:PXCheckBox>
					<px:PXCheckBox ID="chkHidden" runat="server" Checked="True" DataField="Hidden" Style="z-index: 104;
						left: 342px; position: absolute; top: 90px" TabIndex="50" Text="Hidden">
					</px:PXCheckBox>
					<px:PXLabel ID="lblCreatedByID" runat="server" Style="z-index: 105; left: 463px;
						position: absolute; top: 9px">Created by :</px:PXLabel>
					<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataKeyNames="Username"
						DataMember="_Users_" DataSourceID="ds1" Enabled="False" LabelID="lblCreatedByID"
						Style="z-index: 106; left: 580px; position: absolute; top: 9px" TabIndex="-1"
						Width="153px" TextField="Username">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px" Visible="false"
									AllowShowHide="False">
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
					<px:PXLabel ID="lblCreatedDateTime" runat="server" Style="z-index: 107; left: 463px;
						position: absolute; top: 36px">Creation Time :</px:PXLabel>
					<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
						DisplayFormat="g" Enabled="False" LabelID="lblCreatedDateTime" Style="z-index: 108;
						left: 580px; position: absolute; top: 36px" TabIndex="-1" Width="153px">
					</px:PXDateTimeEdit>
					<px:PXLabel ID="lblApprovalGroupID" runat="server" Style="z-index: 109; left: 463px;
						position: absolute; top: 117px">Approval Group :</px:PXLabel>
					<px:PXTreeSelector ID="edApprovalGroupID" runat="server" DataField="ApprovalGroupID"
						TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds1" LabelID="lblApprovalGroupID"
						Style="z-index: 110; left: 580px; position: absolute; top: 117px" TabIndex="100"
						Width="153px" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false">
						<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						<DataBindings>
							<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
						</DataBindings>
					</px:PXTreeSelector>
					<px:PXLabel ID="lblLastModifiedByID" runat="server" Style="z-index: 100; left: 463px;
						position: absolute; top: 63px">LastModifiedByID :</px:PXLabel>
					<px:PXSelector ID="edLastModifiedByID" runat="server" DataField="LastModifiedByID"
						DataKeyNames="Username" DataMember="_Users_" DataSourceID="ds1" LabelID="lblLastModifiedByID"
						Style="z-index: 101; left: 580px; position: absolute; top: 63px" TabIndex="-1"
						Width="153px" Enabled="False" TextField="Username">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px" Visible="false"
									AllowShowHide="False">
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
					<px:PXLabel ID="lblLastModifiedDateTime" runat="server" Style="z-index: 102; left: 463px;
						position: absolute; top: 90px">LastModifiedDateTime :</px:PXLabel>
					<px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime"
						DisplayFormat="g" LabelID="lblLastModifiedDateTime" Style="z-index: 103; left: 580px;
						position: absolute; top: 90px" TabIndex="-1" Width="153px" Enabled="False">
					</px:PXDateTimeEdit>
					<px:PXLabel ID="lblTags" runat="server" Style="z-index: 100; left: 9px; position: absolute;
						top: 297px">Version Tags :</px:PXLabel>
					<px:PXGrid ID="gridTags" runat="server" AdjustPageSize="Auto" DataSourceID="ds1"
						Height="99px" Style="z-index: 105; left: 108px; position: absolute; top: 297px;
						width: 630px;" AutoAdjustColumns="true" TabIndex="140">
						<Levels>
							<px:PXGridLevel DataKeyNames="WikiID,PageID,Language,PageRevisionID,TagID" DataMember="RevisionTags">
								<Columns>
									<px:PXGridColumn DataField="TagID" Width="200px">
										<Header Text="Tag">
										</Header>
									</px:PXGridColumn>
								</Columns>
								<RowTemplate>
									<px:PXLabel ID="lblTagID" runat="server" Style="z-index: 100; left: 9px; position: absolute;
										top: 9px">Tag :</px:PXLabel>
									<px:PXSelector ID="edTagID" runat="server" DataField="TagID" DataKeyNames="WikiID,Description"
										DataMember="_WikiTag_WikiRevisionTag.wikiID_" DataSourceID="ds1" LabelID="lblTagID"
										Style="z-index: 101; left: 126px; position: absolute; top: 9px" TabIndex="-1"
										Width="108px">
										<GridProperties>
											<Columns>
												<px:PXGridColumn DataField="Description">
													<Header Text="Description">
													</Header>
												</px:PXGridColumn>
											</Columns>
										</GridProperties>
									</px:PXSelector>
								</RowTemplate>
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
					<px:PXCheckBox ID="chkSecure" runat="server" DataField="Secure" Style="z-index: 100;
						left: 261px; position: absolute; top: 117px" TabIndex="70" Text="Secured">
					</px:PXCheckBox>
					<px:PXLabel ID="lblApprovalUserID" runat="server" Style="z-index: 100; left: 463px;
						position: absolute; top: 144px">Approver ID :</px:PXLabel>
					<px:PXSelector ID="edApprovalUserID" runat="server" DataField="ApprovalUserID" DataKeyNames="Username"
						DataMember="_Users_WikiPage.approvalGroupID_WikiPage.approvalGroupID_" DataSourceID="ds1"
						HintField="username" HintLabelID="lblApprovalUserIDH" LabelID="lblApprovalUserID"
						Style="z-index: 101; left: 580px; position: absolute; top: 144px" TabIndex="110"
						Width="153px" TextField="Username">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="Username" MaxLength="64" Width="200px" Visible="false"
									AllowShowHide="False">
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
					&nbsp;&nbsp;
					<px:PXCheckBox ID="chkHideMenu" runat="server" Checked="True" DataField="HideMenu"
						Style="z-index: 104; left: 342px; position: absolute; top: 117px" TabIndex="75"
						Text="Hide Menu">
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attachments" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridAttachments" runat="server" Height="162px" Style="z-index: 105;
						position: absolute; left: 0px; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowPaging="true" AdjustPageSize="Auto" SkinID="Details">
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<Actions>
								<AddNew MenuVisible="False" ToolBarVisible="False" />
								<Delete MenuVisible="False" ToolBarVisible="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton PopupPanel="pnlGetLink" Text="Get Link">
									<Images Normal="main@Link" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Edit">
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
						Width="100%" AdjustPageSize="Auto" Height="171px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataKeyNames="PageID,ApplicationName,RoleName" DataMember="AccessRights">
								<RowTemplate>
									<px:PXLabel ID="lblRoleName" runat="server" Style="z-index: 100; left: 9px; position: absolute;
										top: 9px">Role :</px:PXLabel>
									<px:PXSelector ID="edRoleName" runat="server" DataField="RoleName" DataKeyNames="ApplicationName,Rolename"
										DataMember="_Roles_" DataSourceID="ds1" LabelID="lblRoleName" Style="z-index: 101;
										left: 126px; position: absolute; top: 9px" TabIndex="-1" Width="108px">
										<GridProperties>
											<Columns>
												<px:PXGridColumn DataField="Rolename" MaxLength="255" Width="200px" SortDirection="Ascending">
													<Header Text="Role Name">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Descr" MaxLength="255" Width="300px">
													<Header Text="Role Description">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Guest" MaxLength="255" Width="100px" Type="CheckBox">
													<Header Text="Guest">
													</Header>
												</px:PXGridColumn>
											</Columns>
										</GridProperties>
									</px:PXSelector>
									<px:PXLabel ID="lblAccessRights" runat="server" Style="z-index: 102; left: 9px; position: absolute;
										top: 36px">Access Rights :</px:PXLabel>
									<px:PXDropDown ID="edAccessRights" runat="server" AllowNull="False" DataField="AccessRights"
										LabelID="lblAccessRights" SelectedIndex="1" Style="z-index: 103; left: 126px;
										position: absolute; top: 36px" TabIndex="-1" ValueType="Int16" Width="72px">
										<Items>
											<px:PXListItem Text="Inherit" Value="-1" />
											<px:PXListItem Text="Revoked" Value="0" />
											<px:PXListItem Text="View Only" Value="1" />
											<px:PXListItem Text="Edit" Value="2" />
											<px:PXListItem Text="Insert" Value="3" />
											<px:PXListItem Text="Delete" Value="4" />
										</Items>
									</px:PXDropDown>
									<px:PXLabel ID="lblParentAccessRights" runat="server" Style="z-index: 104; left: 9px;
										position: absolute; top: 63px">Parent Access Rights :</px:PXLabel>
									<px:PXDropDown ID="edParentAccessRights" runat="server" AllowNull="False" DataField="ParentAccessRights"
										Enabled="False" LabelID="lblParentAccessRights" SelectedIndex="1" Style="z-index: 105;
										left: 126px; position: absolute; top: 63px" TabIndex="-1" ValueType="Int16" Width="72px">
										<Items>
											<px:PXListItem Text="Not Set" Value="-1" />
											<px:PXListItem Text="Revoked" Value="0" />
											<px:PXListItem Text="View Only" Value="1" />
											<px:PXListItem Text="Edit" Value="2" />
											<px:PXListItem Text="Insert" Value="3" />
											<px:PXListItem Text="Delete" Value="4" />
										</Items>
									</px:PXDropDown>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="RoleName" Width="300px">
										<Header Text="Role">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="AccessRights" DataType="Int16" DefValueText="0"
										RenderEditorText="True" Width="100px">
										<Header Text="Access Rights">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ParentAccessRights"
										RenderEditorText="True" DataType="Int16" DefValueText="0" Width="100px">
										<Header Text="Parent Access Rights">
										</Header>
									</px:PXGridColumn>
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
						AllowFilter="False" SkinID="Details">
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
						AdjustPageSize="Auto" SkinID="Details" AutoAdjustColumns="true">
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
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
										Width="150px">
										<Header Text="Created by">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" DataType="DateTime"
										DisplayFormat="g" Width="100px">
										<Header Text="Creation Time">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="ApprovalByID_Users_Username" Width="150px">
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
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Compare" Key="compare">
									<Images Normal="main@Compare" />
									<AutoCallBack Command="compare" Enabled="True" Target="ds1">
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="viewRevision" CommandSourceID="ds1" Text="View Version">
									<Images Normal="main@Inquiry" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds1" Text="Publish" CommandName="publishRevision">
									<Images Normal="main@SetDefault" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Revert to Version" CommandName="revertToRevision" CommandSourceID="ds1">
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
			<px:PXTabItem Text="Meta">
				<Template>
					<px:PXGrid ID="gridMeta" runat="server" ActionsPosition="Top" DataSourceID="ds1"
						Width="100%" AdjustPageSize="Auto" Height="54px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataKeyNames="PageID,Name" DataMember="CustomMeta">
								<RowTemplate>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Name" Width="300px">
										<Header Text="Name">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Content" Width="400px">
										<Header Text="Content">
										</Header>
									</px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<Searches>
			<px:PXQueryStringParam Name="PageID" OnLoadOnly="True" QueryStringField="PageID"
				Type="String" />
			<px:PXControlParam ControlID="tab1" Name="PageID" PropertyName="NewDataKey[&quot;PageID&quot;]"
				Type="String" />
		</Searches>
		<AutoSize Container="Window" Enabled="true" />
	</px:PXTab>
</asp:Content>
