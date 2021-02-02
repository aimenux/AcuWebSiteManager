<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="NotificationEdit.aspx.cs" Inherits="Page_NotificationEdit"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds1" runat="server" Visible="True" PrimaryView="Pages"
		TypeName="PX.SM.WikiNotificationTemplateMaintenance" style="float:left">
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
			<px:PXDSCallbackCommand Name="compare" Visible="false" />
			<px:PXDSCallbackCommand Name="editFile" Visible="false" DependOnGrid="gridAttachments" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="Key" TreeView="EntityItems" />
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
			<px:PXTreeDataMember TreeKeys="Key" TreeView="GraphTree" />
			<px:PXTreeDataMember TreeKeys="Key" TreeView="CacheTree" />
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>

	<px:PXToolBar ID="PXToolBar1" runat="server" SkinID="Navigation" ImageSet="main">
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
			if(context.command == "Save" || context.command == "Delete")
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
	<px:PXTab ID="tab1" runat="server" DataSourceID="ds1" Height="400px" Style="z-index: 100"
		Width="100%" DataKeyNames="PageID" DataMember="Pages" FilesField="NoteFiles"
		NoteField="NoteText" OnDataBinding="tab_DataBinding" RepaintOnDemand="false">
		<Items>
			<px:PXTabItem Text="Content">
				<Template>
					<div style="position: relative; height: 261px; width: 100%; left: 0px; top: 0px;">
						<px:PXLabel ID="lblName" runat="server" Style="z-index: 100; left: 14px; position: absolute;
							top: 8px">Article ID :</px:PXLabel>
						<px:PXTextEdit ID="edName" runat="server" DataField="Name" LabelID="lblName" Style="z-index: 101;
							left: 95px; position: absolute; top: 8px" TabIndex="1" Width="207px">
						</px:PXTextEdit>
						<px:PXLabel ID="lblTitle" runat="server" Style="z-index: 102; left: 14px; position: absolute;
							top: 35px">Article Name :</px:PXLabel>
						<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" LabelID="lblTitle" Style="z-index: 103;
							left: 95px; position: absolute; top: 35px" TabIndex="3" Width="291px">
						</px:PXTextEdit>
						<px:PXCheckBox ID="chkFolder" runat="server" DataField="Folder" Style="z-index: 104;
							left: 320px; position: absolute; top: 8px" TabIndex="2" Text="Folder">
						</px:PXCheckBox>
						<px:PXLabel ID="lblCategoryID" runat="server" Style="z-index: 105; left: 14px; position: absolute;
							top: 62px">Category :</px:PXLabel>
						<px:PXSelector ID="edCategoryID" runat="server" DataField="CategoryID" DataKeyNames="WikiID,Description"
							DataMember="_WikiPageCategory_WikiPage.wikiID_" DataSourceID="ds1" LabelID="lblCategoryID"
							Style="z-index: 106; left: 95px; position: absolute; top: 62px" TabIndex="4"
							Width="297px" ValueField="Description">
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
						&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;
						<px:PXLabel ID="lblParentUID" runat="server" Style="z-index: 111; left: 401px; position: absolute;
							top: 8px" Width="81px">Parent Folder :</px:PXLabel>
						<px:PXTreeSelector ID="edParent" runat="server" DataField="ParentUID" PopulateOnDemand="True"
							ShowRootNode="False" Style="left: 491px; position: absolute; top: 8px" Width="189px"
							TabIndex="12" TreeDataSourceID="ds1" TreeDataMember="Folders" LabelID="lblParentUID">
							<Images>
								<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
							</Images>
							<DataBindings>
								<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
							</DataBindings>
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblPublishedDateTime" runat="server" Style="z-index: 113; left: 401px;
							position: absolute; top: 62px">Published Date :</px:PXLabel>
						<px:PXDateTimeEdit ID="edPublishedDateTime" runat="server" DataField="PublishedDateTime"
							Enabled="False" LabelID="lblPublishedDateTime" Style="z-index: 114; left: 491px;
							position: absolute; top: 62px" TabIndex="-1" Width="90px">
						</px:PXDateTimeEdit>
						<px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" Style="z-index: 115;
							left: 599px; position: absolute; top: 35px" TabIndex="13" Text="Hold">
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						</px:PXCheckBox>
						<px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" Style="z-index: 116;
							left: 599px; position: absolute; top: 62px" TabIndex="14" Text="Approved">
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						</px:PXCheckBox>
						<px:PXCheckBox ID="chkRejected" runat="server" DataField="Rejected" Style="z-index: 117;
							left: 671px; position: absolute; top: 62px" TabIndex="15" Text="Rejected">
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						</px:PXCheckBox>
						<px:PXLabel ID="lblEntityType" runat="server" Style="z-index: 100; left: 14px; position: absolute;
							top: 89px">Entity Type :</px:PXLabel>
						<px:PXTreeSelector ID="edEntityType" runat="server" DataField="EntityType" PopulateOnDemand="True"
							ShowRootNode="False" Style="z-index: 101; left: 95px; position: absolute; top: 89px"
							TabIndex="6" Width="297px" TreeDataSourceID="ds1" TreeDataMember="CacheTree"
							InitialExpandLevel="0" MinDropWidth="297" MaxDropWidth="500" LabelID="lblEntityType">
							 <Images>
                                    <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                                    <LeafImages Normal="main@Box" Selected="main@Box" />
                            </Images>
							<DataBindings>
								<px:PXTreeItemBinding DataMember="CacheTree" TextField="Name" ValueField="SubKey" />
							</DataBindings>
							<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblMailTo" runat="server" Style="z-index: 102; left: 14px; position: absolute;
							top: 116px">To :</px:PXLabel>
						<px:PXTreeSelector ID="edMailTo" runat="server" DataField="MailTo" LabelID="lblMailTo"
							MaxLength="255" Style="z-index: 103; left: 95px; position: absolute; top: 116px"
							TabIndex="8" Width="648px" TreeDataSourceID="ds1" PopulateOnDemand="True" InitialExpandLevel="0"
							ShowRootNode="false" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
							AppendSelectedValue="true" Height="16px" AutoRefresh="true" TreeDataMember="EntityItems">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
							<ButtonImages Normal="main@AddNew" Hover="main@AddNew" Pushed="main@AddNew" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblMailCc" runat="server" Style="z-index: 104; left: 14px; position: absolute;
							top: 143px; bottom: 105px;">CC :</px:PXLabel>
						<px:PXTreeSelector ID="edMailCc" runat="server" DataField="MailCc" LabelID="lblMailCc"
							MaxLength="255" Style="z-index: 105; left: 95px; position: absolute; top: 143px"
							TabIndex="9" Width="648px" TreeDataSourceID="ds1" PopulateOnDemand="True" InitialExpandLevel="0"
							ShowRootNode="false" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
							AppendSelectedValue="true" Height="16px" TreeDataMember="EntityItems" AutoRefresh="true" >
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
							<ButtonImages Normal="main@AddNew" Hover="main@AddNew" Pushed="main@AddNew" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblMailBcc" runat="server" Style="z-index: 104; left: 14px; position: absolute;
							top: 170px" Hidden="True">BCC :
						</px:PXLabel>
						<px:PXTreeSelector ID="edMailBcc" runat="server" DataField="MailBcc" LabelID="lblMailBcc"
							MaxLength="255" Style="z-index: 107; left: 95px; position: absolute; top: 170px"
							TabIndex="10" Width="648px" TreeDataSourceID="ds1" PopulateOnDemand="True" InitialExpandLevel="0"
							ShowRootNode="false" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
							AppendSelectedValue="true" Height="16px" TreeDataMember="EntityItems" AutoRefresh="true" >
							<DataBindings>
								<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
							</DataBindings>
							<ButtonImages Normal="main@AddNew" Hover="main@AddNew" Pushed="main@AddNew" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblOldStatusID" runat="server" Style="z-index: 100; left: 401px;
							position: absolute; top: 35px">Status :</px:PXLabel>
						<px:PXDropDown ID="edOldStatusID" runat="server" DataField="OldStatusID" Enabled="False"
							LabelID="lblOldStatusID" Style="z-index: 101; left: 491px; position: absolute;
							top: 35px" TabIndex="-1" ValueType="Int32" Width="90px">
							<Items>
								<px:PXListItem Text="Hold" Value="0" />
								<px:PXListItem Text="Pending" Value="1" />
								<px:PXListItem Text="Rejected" Value="2" />
								<px:PXListItem Text="Published" Value="3" />
								<px:PXListItem Text="Deleted" Value="4" />
							</Items>
							<AutoCallBack Enabled="true" Command="Save" Target="tab1" />
						</px:PXDropDown>
						<px:PXLabel ID="lblSubject" runat="server" Style="z-index: 100; left: 14px; position: absolute;
							top: 197px">Subject :</px:PXLabel>
						<px:PXTreeSelector ID="edSubject" runat="server" AllowEditValue="true" AppendSelectedValue="true"
							DataField="Subject" Height="16px" InitialExpandLevel="0" LabelID="lblSubject"
							MaxDropWidth="600" MaxLength="255" MinDropWidth="468" PopulateOnDemand="True"
							ShowRootNode="false" Style="z-index: 101; left: 95px; position: absolute; top: 197px"
							TabIndex="11" TreeDataMember="EntityItems" TreeDataSourceID="ds1" Width="648px" AutoRefresh="true" >
							<DataBindings>
								<px:PXTreeItemBinding ImageUrlField="Icon" TextField="Name" ValueField="Path" />
							</DataBindings>
							<ButtonImages Normal="main@AddNew" Hover="main@AddNew" Pushed="main@AddNew" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblGraph" runat="server" Style="z-index: 100; left: 401px; position: absolute;
							top: 89px">Graph :</px:PXLabel>
						<px:PXTreeSelector ID="edGraph" runat="server" DataField="GraphType" PopulateOnDemand="True"
							ShowRootNode="False" Style="z-index: 101; left: 491px; position: absolute; top: 89px"
							TabIndex="7" Width="252px" TreeDataSourceID="ds1" TreeDataMember="GraphTree"
							InitialExpandLevel="0" MinDropWidth="297" MaxDropWidth="500" AutoRefresh="true" LabelID="lblGraph">
							 <Images>
                                    <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                                    <LeafImages Normal="main@Box" Selected="main@Box" />
                            </Images>
							<DataBindings>
								<px:PXTreeItemBinding DataMember="GraphTree" TextField="Name" ValueField="SubKey" />
							</DataBindings>
							<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						</px:PXTreeSelector>
						<px:PXLabel ID="lblDefaultAccount" runat="server" Style="z-index: 100; left: 464px;
							position: absolute; top: 224px">Default Account :</px:PXLabel>
						<px:PXSelector ID="edDefaultAccount" runat="server" DataField="DefaultEMailAccount"
							DataKeyNames="EMailAccount" ValueField="Address" TextField="Address" DataSourceID="ds1" 
							LabelID="lblDefaultAccount"
							Style="z-index: 101; left: 563px; position: absolute; top: 224px" TabIndex="12"
							Width="180px">
							<GridProperties>
								<Columns>
									<px:PXGridColumn DataField="Address" Width="300px">
										<Header Text="Address">
										</Header>
									</px:PXGridColumn>
								</Columns>
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSelector>
					</div>
					<px:PXWikiEdit ID="edtWikiText" runat="server" DataField="Content" Height="140px"
						Width="100%" TabIndex="20" Style="outline-style: none" TemplateDataSourceID="ds1"
						TemplateDataDataMember="EntityItems">
						<TemplateDataBindings>
							<px:PXTreeItemBinding DataMember="EntityItems" TextField="Name" ValueField="Path"
								ImageUrlField="Icon" />
						</TemplateDataBindings>
						<AutoSize Enabled="True" />
						<UploadCallBack Enabled="true" Target="gridAttachments" Command="Refresh" />
					</px:PXWikiEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Properties" LoadOnDemand="true">
				<Template>
					<px:PXLabel ID="lblSummary" runat="server" Style="z-index: 100; left: 9px; position: absolute;
						top: 171px">Summary :</px:PXLabel>
					<px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" Height="81px" LabelID="lblSummary"
						Style="z-index: 101; left: 108px; position: absolute; top: 171px" TabIndex="70"
						TextMode="MultiLine" Width="576px">
					</px:PXTextEdit>
					<px:PXLabel ID="lblKeywords" runat="server" Style="z-index: 102; left: 9px; position: absolute;
						top: 9px">Keywords :</px:PXLabel>
					<px:PXTextEdit ID="edKeywords" runat="server" DataField="Keywords" LabelID="lblKeywords"
						Style="z-index: 103; left: 108px; position: absolute; top: 9px" TabIndex="10"
						Width="279px" Height="90px">
					</px:PXTextEdit>
					<px:PXCheckBox ID="chkVersioned" runat="server" Checked="True" DataField="Versioned"
						Style="z-index: 104; left: 108px; position: absolute; top: 117px" TabIndex="20"
						Text="Versioned">
					</px:PXCheckBox>
					<px:PXLabel ID="lblCreatedByID" runat="server" Style="z-index: 105; left: 423px;
						position: absolute; top: 9px">Created by :</px:PXLabel>
					<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataKeyNames="Username"
						DataMember="_Users_" DataSourceID="ds1" Enabled="False" LabelID="lblCreatedByID"
						Style="z-index: 106; left: 540px; position: absolute; top: 9px" TabIndex="-1"
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
					<px:PXLabel ID="lblCreatedDateTime" runat="server" Style="z-index: 107; left: 423px;
						position: absolute; top: 36px">Creation Time :</px:PXLabel>
					<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
						DisplayFormat="g" Enabled="False" LabelID="lblCreatedDateTime" Style="z-index: 108;
						left: 540px; position: absolute; top: 36px" TabIndex="-1" Width="153px">
					</px:PXDateTimeEdit>
					<px:PXLabel ID="lblApprovalGroupID" runat="server" Style="z-index: 109; left: 423px;
						position: absolute; top: 117px">Approval Group :</px:PXLabel>
					<px:PXTreeSelector ID="edApprovalGroupID" runat="server" DataField="ApprovalGroupID"
						TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds1" LabelID="lblApprovalGroupID"
						Style="z-index: 110; left: 540px; position: absolute; top: 117px" TabIndex="50"
						Width="153px" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false">
						<AutoCallBack Enabled="True" Command="Save" Target="tab1" />
						<DataBindings>
							<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
						</DataBindings>
					</px:PXTreeSelector>
					<px:PXLabel ID="lblLastModifiedByID" runat="server" Style="z-index: 100; left: 423px;
						position: absolute; top: 63px">LastModifiedByID :</px:PXLabel>
					<px:PXSelector ID="edLastModifiedByID" runat="server" DataField="LastModifiedByID"
						DataKeyNames="Username" DataMember="_Users_" DataSourceID="ds1" LabelID="lblLastModifiedByID"
						Style="z-index: 101; left: 540px; position: absolute; top: 63px" TabIndex="-1"
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
					<px:PXLabel ID="lblLastModifiedDateTime" runat="server" Style="z-index: 102; left: 423px;
						position: absolute; top: 90px">LastModifiedDateTime :</px:PXLabel>
					<px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime"
						DisplayFormat="g" LabelID="lblLastModifiedDateTime" Style="z-index: 103; left: 540px;
						position: absolute; top: 90px" TabIndex="-1" Width="153px" Enabled="False">
					</px:PXDateTimeEdit>
					<px:PXLabel ID="lblTags" runat="server" Style="z-index: 100; left: 9px; position: absolute;
						top: 270px">Version Tags :</px:PXLabel>
					<px:PXGrid ID="gridTags" runat="server" AdjustPageSize="Auto" ActionsPosition="Top"
						DataSourceID="ds1" Height="117px" Style="z-index: 105; left: 108px; position: absolute;
						top: 270px" Width="582px" AutoAdjustColumns="true" TabIndex="80">
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
					<px:PXLabel ID="lblApprovalUserID" runat="server" Style="z-index: 100; left: 423px;
						position: absolute; top: 144px">Approver ID :</px:PXLabel>
					<px:PXSelector ID="edApprovalUserID" runat="server" AutoRefresh="True" DataField="ApprovalUserID"
						DataKeyNames="Username" DataMember="_Users_WikiPage.approvalGroupID_WikiPage.approvalGroupID_"
						DataSourceID="ds1" HintField="username" HintLabelID="lblApprovalUserIDH" LabelID="lblApprovalUserID"
						Style="z-index: 101; left: 540px; position: absolute; top: 144px" TabIndex="60"
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
									<AutoCallBack Target="ds1" Command="editFile" Enabled="true" />
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
						<AutoSize Enabled="True" Container="Parent" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Access Rights" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridAccessRights" runat="server" ActionsPosition="Top" DataSourceID="ds1"
						Width="100%" AdjustPageSize="Auto" Height="171px">
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
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subarticles" Key="Subarticles" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="gridSub" runat="server" Height="171px" Style="z-index: 100; left: 0px;
						position: static; top: 0px" Width="100%" DataSourceID="ds1" ActionsPosition="Top"
						AllowFilter="False">
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
						<ActionBar ActionsVisible="true">
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
						<AutoSize Enabled="True" Container="Parent" />
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
