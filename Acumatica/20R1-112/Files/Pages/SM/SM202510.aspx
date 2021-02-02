<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM202510.aspx.cs" Inherits="Page_SM210000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource SkinID="Transparent" ID="ds" runat="server" Visible="True"
        PrimaryView="Files" TypeName="PX.SM.WikiFileMaintenance" style="float: left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="checkOut" CommitChanges="True" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="viewRevision" Visible="False" DependOnGrid="gridRevisions" />
            <px:PXDSCallbackCommand Name="latestOnly" Visible="False" />
            <px:PXDSCallbackCommand Name="openArticle" Visible="False" DependOnGrid="gridArticles" />
            <px:PXDSCallbackCommand DependOnGrid="gridEntities" Name="viewEntity" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridScreens" Name="viewScreen" Visible="False" />
            <px:PXDSCallbackCommand Name="edit" Visible="False" />
            <px:PXDSCallbackCommand Name="downloadFile" Visible="false" />
            <px:PXDSCallbackCommand Name="uploadFile" Visible="false" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeKeys="PageID" TreeView="Pages" />
        </DataTrees>
    </px:PXDataSource>
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation"
        CommandSourceID="ds" CallbackUpdatable="True" BackColor="Transparent">
        <Items>
            <px:PXToolBarButton Text="Synchronization" Tooltip="Synchronization">
                <MenuItems>
                    <px:PXMenuItem CommandName="downloadFile" CommandSourceID="ds" Text="Export File" />
                    <px:PXMenuItem CommandName="uploadFile" CommandSourceID="ds" Text="Import File" />
                </MenuItems>
            </px:PXToolBarButton>
        </Items>
    </px:PXToolBar>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<script language="javascript" type="text/javascript">
		function isNullOrEmpty(str) {
			if (str == null || str == "")
				return true;
			return false;
		}

		function btnTlbClicked(sender, e) {
			if (e.button.autoCallBack.command == "latestrevisions")
				e.button.autoCallBack.command = "allrevisions";
			else
				e.button.autoCallBack.command = "latestrevisions";
		}

		function tabInitialized() {
			//var edUploaderID = pnlNewRevID + "_upl_upl";
			// pnlNewRevID variable is registered by server.
			px_all[pnlNewRevID].events.removeEventHandler("hideAfterUpload", refreshScreen);
			px_all[pnlNewRevID].events.addEventHandler("hideAfterUpload", refreshScreen);
		}

		function refreshScreen() {
			// dsID, gridRevisionsID variables are registered by server.		
			var ds = px_all[dsID];
			if (ds != null) ds.executeCallback("Cancel");
			px_all[pnlNewRevID].hide();
			px_all[gridRevisionsID].refresh();
		}
	
	</script>
	<px:PXUploadDialog ID="pnlNewRev" runat="server" DesignView="Hidden" Height="120px"
		Style="position: static" Width="560px" Caption="File Upload" AutoSaveFile="false"
		RenderCheckIn="true" Key="NewRevisionPanel" />
	<px:PXSmartPanel ID="pnlCheckOut" runat="server" Height="175px" Style="left: 0px;
		position: relative; top: 0px" Width="450px" Caption="Check Out File" CaptionVisible="True"
		Key="CheckoutComment">
		<px:PXFormView ID="formCheckOutComment" runat="server" DataSourceID="ds" Height="130px"
			Width="379px" Style="z-index: 100" SkinID="Transparent" DataMember="CheckoutComment">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
			    <px:PXTextEdit ID="edtCheckOutComment" runat="server" Height="81px" DataField="Comment" TextMode="MultiLine" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
				    <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Files" Caption="Choose file">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="M" StartColumn="True" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXTextEdit ID="edtFileID" runat="server" DataField="FileID" ReadOnly="True" ForeColor="Black" />
			<px:PXCheckBox ID="lblIsHidden" runat="server" DataField="IsHidden" />
			<px:PXLayoutRule runat="server" />
			<px:PXTextEdit ID="edCheckedOutBy" runat="server" DataField="CheckedOutBy" Enabled="False" />
			<px:PXTextEdit ID="edCheckedOutComment" runat="server" DataField="CheckedOutComment"
				Height="45px" TextMode="MultiLine" Enabled="False" BackColor="White" ReadOnly="True" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
				<px:PXTextEdit ID="edExternalLink" runat="server" DataMember="Files" DataField="ExternalLink" ReadOnly="True" />
				<px:PXTextEdit ID="edWikiLink" runat="server" DataField="WikiLink" ReadOnly="True" />
				<px:PXTextEdit ID="edWebDAVLink" runat="server" DataField="WebDAVLink" ReadOnly="True" />
        </Template>
		<Parameters>
			<px:PXQueryStringParam Name="name" QueryStringField="fileID" Type="String" OnLoadOnly="true" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="288px" LoadOnDemand="True"
		DataMember="CurrentFile" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Versions" Tooltip="Shows different versions of a file.">
				<Template>
					<px:PXGrid ID="gridRevisions" runat="server" DataSourceID="ds" Style="z-index: 100"
						Width="100%" ActionsPosition="Top" AllowSearch="True" AdjustPageSize="Auto" AllowPaging="True"
						BorderWidth="0px" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Revisions" DataKeyNames="FileID,FileRevisionID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXTextEdit ID="edComment" runat="server" DataField="Comment" />
									<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False"
										TextField="Username" />
									<px:PXNumberEdit ID="edFileRevisionID" runat="server" DataField="FileRevisionID" />
									<px:PXTextEdit ID="edReadableSize" runat="server" DataField="ReadableSize" />
									<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
										DisplayFormat="g" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="FileRevisionID" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
										Width="110px" />
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="120px" DisplayFormat="g" />
									<px:PXGridColumn DataField="ReadableSize" Width="110px" />
									<px:PXGridColumn DataField="Comment" Width="300px" />
									<px:PXGridColumn DataField="OriginalName" Width="250px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="View Selected Version" Tooltip="View selected file.">
								    <AutoCallBack Command="viewRevision" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Articles" Tooltip="Shows wiki articles which contain reference to this file.">
				<Template>
					<px:PXFormView ID="frmPrimaryPage" runat="server" DataSourceID="ds" DataMember="PrimaryArticle"
						CaptionVisible="False" Width="100%">
						<ContentStyle BorderStyle="None">
						</ContentStyle>
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" Enabled="False" ReadOnly="True" />
                        </Template>
					</px:PXFormView>
					<px:PXGrid ID="gridArticles" runat="server" DataSourceID="ds" Style="position: static"
						Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="Details"
						Caption="Articles Using This File">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="PagesWithFile" DataKeyNames="PageID,Language,PageRevisionID,FileID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXTextEdit ID="edWikiPagesName" runat="server" DataField="WikiPage__Name" />
									<px:PXTextEdit ID="edLanguage" runat="server" DataField="Language" />
									<px:PXTextEdit ID="edPageRevisionID" runat="server" DataField="PageRevisionID" />
									<px:PXTextEdit ID="edWikiPageTitle" runat="server" DataField="WikiPage__Title" />
							    </RowTemplate>
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="WikiPage__Name" Width="120px" />
									<px:PXGridColumn DataField="Language" />
									<px:PXGridColumn DataField="PageRevisionID" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="WikiPage__Title" Width="120px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Latest only" ToggleMode="True" Tooltip="When pushed shows latest article version which includes references to this file. Otherwise shows all article versions (if any) which include references to this file."
									Pushed="True" CommandName="latestOnly" CommandSourceID="ds" />
							    <px:PXToolBarButton Key="open" Text="Open" Tooltip="Navigate to selected article."
									CommandName="openArticle" CommandSourceID="ds" />
							</CustomItems>
						</ActionBar>
						<ClientEvents ToolsButtonClick="btnTlbClicked" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Entities" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridEntities" runat="server" DataSourceID="ds" Height="200px" Style="position: static"
						Width="100%" ActionsPosition="Top" AutoAdjustColumns="True" BorderWidth="0px"
						SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="EntitiesRecords" DataKeyNames="NoteID,FileID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXTextEdit ID="edEntityName" runat="server" DataField="EntityName" />
									<px:PXTextEdit ID="edEntityRowValues" runat="server" DataField="EntityRowValues" />
                                </RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="EntityName" Width="108px" />
									<px:PXGridColumn DataField="EntityRowValues" Width="108px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<AdjustColumns Enabled="False" />
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="View Entity">
								    <AutoCallBack Command="viewEntity" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Access Rights" RepaintOnDemand="false" Tooltip="Shows access rights for the file."
				VisibleExp="DataControls[&quot;edAccessRights&quot;].Value >= 4">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="SM" StartColumn="True" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" ColumnSpan="2" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsPublic" runat="server" DataField="IsPublic" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
					<px:PXSelector CommitChanges="True" ID="edWiki" runat="server" DataField="SelectedWikiID" />
					<px:PXTreeSelector CommitChanges="True" ID="edPrimaryPage" runat="server" DataField="SelectedPageID"
						PopulateOnDemand="True" ShowRootNode="False" TreeDataSourceID="ds" utoRefresh="True" InitialExpandLevel="0"
						MinDropWidth="393" TreeDataMember="Pages" >
						<DataBindings>
							<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
						</DataBindings>
					</px:PXTreeSelector>
                    <px:PXSelector ID="edScreenID" runat="server" DataField="PrimaryScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
					<px:PXLayoutRule runat="server" Merge="True">
					</px:PXLayoutRule>
					<asp:SiteMapDataSource ID="hds" runat="server" ShowStartingNode="False" />
					<px:PXLabel ID="PXHole" runat="server" Width="145px" CssClass="labelH note-m">Access Rights:</px:PXLabel>
					<px:PXGrid ID="gridRights" runat="server" Width="300px" Height="400px" DataSourceID="ds" 
						AutoAdjustColumns="True" SkinID="ShortList">
						<ActionBar ActionsVisible="false" />
						<Levels>
							<px:PXGridLevel DataMember="WikiAccessRightsRecs" 
								DataKeyNames="PageID,RoleName,ApplicationName">
								<Columns>
									<px:PXGridColumn DataField="RoleName" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="AccessRights" Width="130px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
					<px:PXLayoutRule runat="server">
					</px:PXLayoutRule>
					<px:PXTextEdit ID="edAccessRights" runat="server" DataField="AccessRights" Enabled="False" Visible="False" />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn ="True" />
					<px:PXLabel ID="PXHole0" runat="server" Width="145px" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Synchronization" Tooltip="Synchronization with different external sources">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkSynchronizable" runat="server"
						DataField="Synchronizable" ToolTip="Synchronize file with external source" />
					<px:PXDropDown CommitChanges="True" ID="edSourceType" runat="server" DataField="SourceType" />
					<px:PXTextEdit CommitChanges="True" ID="edSourceUri" runat="server" DataField="SourceUri" />
					<px:PXTextEdit ID="edSourceLogin" runat="server" DataField="SourceLogin" />
					<px:PXTextEdit ID="edSourcePassword" runat="server" DataField="SourcePassword" TextMode="Password" />
                    <px:PXSelector ID="edPSshCertificateName" runat="server" DataField="SshCertificateName" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkSourceIsFolder" runat="server"
						DataField="SourceIsFolder" />
					<px:PXTextEdit ID="edSourceMask" runat="server" DataField="SourceMask" />
					<px:PXDropDown ID="edSourceNamingFormat" runat="server" DataField="SourceNamingFormat" />
					<px:PXDateTimeEdit ID="edSourceLastImportDate" runat="server" DataField="SourceLastImportDate"
						DisplayFormat="g" Enabled="False" Size="XM" />
					<px:PXDateTimeEdit ID="edSourceLastExportDate" runat="server" DataField="SourceLastExportDate"
						DisplayFormat="g" Enabled="False" Size="XM" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" />
		<ClientEvents Initialize="tabInitialized" />
	</px:PXTab>
</asp:Content>
