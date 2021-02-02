<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM203520.aspx.cs" Inherits="Page_SM200575"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Companies"
		TypeName="PX.SM.CompanyMaint" PageLoadBehavior="SearchSavedKeys">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="saveCompanyCommand" CommitChanges="True" PopupVisible="True" HideText="True" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="insertCompanyCommand" PopupVisible="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="deleteCompanyCommand" PopupVisible="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="Previous" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="copyCompanyCommand" StartNewGroup="True" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="exportSnapshotCommand" StartNewGroup="True" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="importSnapshotCommand" DependOnGrid="gridSnapshots" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="downloadSnapshotCommand" DependOnGrid="gridSnapshots"	Visible="False" />
			<px:PXDSCallbackCommand Name="uploadSnapshotCommand" Visible="False" />
			<px:PXDSCallbackCommand Name="prepareSnapshotCommand" DependOnGrid="gridSnapshots" Visible="False" />
			<px:PXDSCallbackCommand Name="manageUsersCommand" DependOnGrid="gridUsers" Visible="False" />
			<px:PXDSCallbackCommand Name="changeVisibilityCommand" DependOnGrid="gridSnapshots"	Visible="False" />
			<px:PXDSCallbackCommand Name="deleteOrphanedRows" DependOnGrid="gridSnapshots" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="dismissedUnsafeSnapshotCommand" StartNewGroup="True" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="spaceUsageCommand" StartNewGroup="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<%--	<px:PXUploadFilePanel ID="dlgUploadFile" runat="server" IgnoreSize="True" OnUpload="OnFileUploadFinished"
		PanelID="pnlUploadFileSmart" CommandSourceID="ds" AllowedTypes=".zip" />--%>
	<px:PXUploadDialog ID="dlgUploadFile" runat="server" Key="UploadDialogPanel" Height="60px" Style="position: static" Width="450px" 
		Caption="Upload Snapshot Package" AutoSaveFile="false" SessionKey="UploadedSnapshotKey" AllowedTypes=".zip" IgnoreSize="true" RenderImportOptions="true"
		RenderCheckIn="false" RenderLinkTextBox="false" RenderLink="false" RenderComment="false" />
	<px:PXSmartPanel ID="pnlImportSnapshot" runat="server" CaptionVisible="True" Caption="Restore Snapshot"
		LoadOnDemand="true" Key="ImportSnapshotPanel" AutoCallBack-Enabled="True"
		AutoCallBack-Target="frmEditObjectCommand" 	AutoCallBack-Command="Refresh" AutoCallBack-ActiveBehavior="true"
		AutoCallBack-Behavior-RepaintControlsIDs="ftmImportSnapshot" Overflow="hidden">
		<px:PXFormView ID="ftmImportSnapshot" runat="server" SkinID="Transparent" DataMember="ImportSnapshotPanel"
			DataSourceID="ds">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXLabel ID="lblImportWarning" runat="server" Encode="True" Text ="WARNING: This action will overwrite all data in this company." />
				<px:PXTextEdit ID="edCompany" runat="server" DataField="Company" Enabled="False" />
				<px:PXTextEdit ID="edName" runat="server" DataField="Name" Enabled="False" />
				<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
<%--				<px:PXCheckBox ID="chkCustomization" runat="server" DataField="Customization" />--%>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnImportSnapshotOK" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Target="ftmImportSnapshot" Command="Save" />
			</px:PXButton>
			<px:PXButton ID="btnImportSnapshotCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlExportSnapshot" runat="server" CaptionVisible="True" Caption="Create Snapshot" LoadOnDemand="true"
		Key="ExportSnapshotPanel" AutoCallBack-Enabled="True" AutoCallBack-Target="frmEditObjectCommand" AutoCallBack-Command="Refresh" 
		AutoCallBack-ActiveBehavior="true" AutoCallBack-Behavior-RepaintControlsIDs="frmExportSnapshot" 
		DesignView="Content" Overflow="hidden">
		<px:PXFormView ID="frmExportSnapshot" runat="server" SkinID="Transparent" DataMember="ExportSnapshotPanel" DataSourceID="ds">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXTextEdit ID="edCompany" runat="server" DataField="Company" Enabled="False" />
				<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="True" />
				<px:PXDropDown ID="edExportMode" runat="server" AllowNull="False" DataField="ExportMode" SelectedIndex="1" />
				<px:PXCheckBox ID="chkPrepare" runat="server" DataField="Prepare" CommitChanges="True"/>
				<px:PXDropDown ID="edPrepareMode" runat="server" DataField="PrepareMode" SelectedIndex="1" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnExportSnapshotOK" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Target="frmExportSnapshot" Command="Save" />
			</px:PXButton>
			<px:PXButton ID="btnExportSnapshotCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlReloadSnapshot" runat="server" CaptionVisible="True" Caption="Reset Data"
		ForeColor="Black" Style="position: static" Height="95px" Width="375px" LoadOnDemand="True"
		Key="ReloadSnapshotPanel" AutoCallBack-Target="frmEditObjectCommand"
		AutoCallBack-Command="Refresh" DesignView="Content">
		<px:PXFormView ID="frmReloadSnapshot" runat="server" SkinID="Transparent" DataMember="ReloadSnapshotPanel"
			DataSourceID="ds" Width="350px" EmailingGraph="">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
				<PXLabel ID="lblImportWarning" runat="server" Encode="True" Text="WARNING: This action will overwrite all data in this company." />
				<px:PXDropDown ID="edDataType" runat="server" DataField="DataType" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnReloadSnapshotOK" runat="server" DialogResult="OK" Text="OK">
						<AutoCallBack Command="Save" Target="frmReloadSnapshot" />
					</px:PXButton>
					<px:PXButton ID="btnReloadSnapshotCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
			<Activity HighlightColor="" SelectedColor="" />
		</px:PXFormView>		
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlCopyCompany" runat="server" CaptionVisible="True" Caption="Copy Company"
		ForeColor="Black" Style="position: static" LoadOnDemand="True"
		Key="CopyCompanyPanel" AutoCallBack-Enabled="true" AutoCallBack-Target="frmCopyCompany"
		AutoCallBack-Command="Refresh" DesignView="Content">
		<px:PXFormView ID="frmCopyCompany" runat="server" SkinID="Transparent" DataMember="CopyCompanyPanel"
			DataSourceID="ds" EmailingGraph="">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" StartColumn="True">
				</px:PXLayoutRule>
				<px:PXLabel ID="lblImportWarning" runat="server" Encode="True">
					WARNING: This action will overwrite all data in the destination company.
				</px:PXLabel>
				<px:PXSelector ID="edCompanyID" runat="server" AutoRefresh="True" DataField="CompanyID" DataSourceID="ds" ReadOnly="True">
				</px:PXSelector>
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnCopyCompanyOK" runat="server" DialogResult="OK" Text="OK">
						<AutoCallBack Command="Save" Target="frmCopyCompany" />
					</px:PXButton>
					<px:PXButton ID="btnCopyCompanyCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
			<Activity HighlightColor="" SelectedColor="" />
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Companies" EmailingGraph="" Caption="Company Summary" TemplateContainer="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edCompanyID" runat="server" DataField="CompanyID" ReadOnly="True" AutoRefresh="True" DataSourceID="ds" />
			<px:PXTextEdit ID="edCompanyCD" runat="server" DataField="CompanyCD" />
			<px:PXTextEdit ID="edLoginName" runat="server" DataField="LoginName" /> 
			<px:PXLayoutRule runat="server" LabelsWidth="SM" StartColumn="True" ControlSize="L" />
			<px:PXTextEdit ID="edCloudTenantID" runat="server" DataField="CloudTenantID" Enabled="False" />
			<px:PXDropDown CommitChanges="True" ID="ddStatus" runat="server" DataField="Status" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CompanyCurrent"
		EmailingGraph="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Snapshots">
				<Template>
					<px:PXGrid ID="gridSnapshots" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
						Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab"
						AutoAdjustColumns="true" SyncPosition="true">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="Snapshots">
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="SnapshotID" Label="Snapshot ID" Width="250px" Visible="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="Name" Label="Name" Width="200px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Description" Label="Description" Width="300px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Prepared" Label="Prepared" Width="100px" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="SizePrepared" Label="Size (MB)" Width="145px" AllowShowHide="Server"/>
									<px:PXGridColumn AllowUpdate="False" DataField="Date" DisplayFormat="g" Label="Creation Date" Width="150px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Version" Label="Version" Width="110px" />
									<px:PXGridColumn AllowUpdate="False" DataField="ExportMode" Label="ExportMode" Width="150px" />
									<px:PXGridColumn DataField="SourceCompany" Label="Company ID" Width="200px" TextField="SourceCompany_UPCompany_description" />
									<px:PXGridColumn AllowUpdate="False" DataField="Customization" Label="Customization"
										Width="200px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="IsSafe" Label="IsSafe" Width="100px" Type="CheckBox" TextAlign="Center" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<FilterBar Enabled="False" />
								<FilterSet Enabled="False" />
								<FilterShow Enabled="False" />
								<AddNew Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Upload Data" Tooltip="Displays dialog to upload new data package.">
								    <AutoCallBack Command="uploadSnapshotCommand" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Prepare Snapshot" Tooltip="Prepare the Selected Snapshot for Exporting">
								    <AutoCallBack Command="prepareSnapshotCommand" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Download Data" Tooltip="Download selected data package.">
								    <AutoCallBack Command="downloadSnapshotCommand" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Change Visibility" Tooltip="Change current snapshot visibility.">
								    <AutoCallBack Command="changeVisibilityCommand" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Snapshot Restoration History">
				<Template>
					<px:PXGrid ID="gridHistory" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
						Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="SnapshotsHistory">
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="SnapshotID" Label="Snapshot ID" Width="250px" Visible="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="UPSnapshot__Name" Label="Description" Width="200px" />
									<px:PXGridColumn AllowUpdate="False" DataField="UPSnapshot__Description" Label="Description" Width="300px" />
									<px:PXGridColumn AllowUpdate="False" DataField="UserID" Label="User" Width="150px" Visible ="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" DisplayFormat="g" Label="Restoration Date" Width="150px" />
									<px:PXGridColumn AllowUpdate="False" DataField="UPSnapshot__Version" Label="Description" Width="150px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="IsSafe" Label="IsSafe" Width="100px" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Dismissed" Label="Dismissed" Width="100px" Type="CheckBox" TextAlign="Center" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<FilterBar Enabled="False" />
								<FilterSet Enabled="False" />
								<FilterShow Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Users">
				<Template>
					<px:PXGrid ID="gridUsers" runat="server" DataSourceID="ds" Style="position: static"
						Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="Users">
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="Username" Label="Username"
										Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="IsApproved" Label="Activate Account"
										TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="FirstName" Label="First Name" Width="150px" />
									<px:PXGridColumn DataField="LastName" Label="Last Name" Width="150px" />
									<px:PXGridColumn AllowNull="False" DataField="Email" Label="Email" Width="200px" />
									<px:PXGridColumn DataField="Phone" DisplayFormat="+# (###) ###-#### Ext:####" Label="Phone"
										Width="200px" />
									<px:PXGridColumn DataField="Comment" Label="Comment" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="IsOnLine" Label="IsOnLine" TextAlign="Center"
										Type="CheckBox" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="IsLockedOut" Label="Temporarily Lock Out Account"
										TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="PasswordChangeOnNextLogin" Label="Force User to Change Password on Next Login"
										TextAlign="Center" Type="CheckBox" Width="125px" />
									<px:PXGridColumn AllowNull="False" DataField="AllowPasswordRecovery" Label="Allow Password Recovery"
										TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="PasswordNeverExpires" Label="Password Never Expires"
										TextAlign="Center" Type="CheckBox" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<%--<CustomItems>
								<px:PXToolBarButton Text="User Managment">
								    <AutoCallBack Command="manageUsersCommand" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>--%>
							<Actions>
								<AddNew Enabled="false" MenuVisible="false" />
								<Delete Enabled="false" MenuVisible="false" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
    <px:PXSmartPanel ID="edUsageDetails" runat="server" Caption="Notification" LoadOnDemand="True"
        CaptionVisible="True" Key="Companies" CommandSourceID="ds" AllowResize="false">
        <px:PXLabel ID="edLabel" runat="server" Text="You can't create snapshot because the DB size limit is exceeded." />
        <px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="SM" ControlSize="M" />
        <px:PXLabel ID="PXLabel1" runat="server" Text="Too see detailed information about Database space usage please go to Space Usage form." />
        <px:PXPanel ID="edUsagePanel" runat="server" SkinID="Buttons" Width="100%">
            <px:PXButton ID="edUsageOk" runat="server" DialogResult="OK" Text="Go to Space Usage" />
			<px:PXButton ID="edUsageCnl" runat="server" DialogResult="Cancel" Text="Return to Tenants" />
		</px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
