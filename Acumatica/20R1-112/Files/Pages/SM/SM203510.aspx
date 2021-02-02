<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM203510.aspx.cs" Inherits="Page_SM200570"
	Title="Untitled Page" %>
<%@ Register TagPrefix="px" Namespace="PX.Web.UI" Assembly="PX.Web.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=3b136cac2f602b8e" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.UpdateMaint"
		PrimaryView="VersionRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="uploadVersionCommand" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="downloadVersionCommand" Visible="False" CommitChanges="True"
				DependOnGrid="gridAvailableVersions" />
			<px:PXDSCallbackCommand Name="applyVersionCommand" Visible="False" CommitChanges="True"
				DependOnGrid="gridAvailableVersions" />
            <px:PXDSCallbackCommand Name="validateCompatibility"  CommitChanges="True"
				DependOnGrid="gridAvailableVersions" Visible="False" />
			<px:PXDSCallbackCommand Name="skipErrorCommand" Visible="False" CommitChanges="True"
				DependOnGrid="gridUPErrors" />
			<px:PXDSCallbackCommand Name="showLogFileCommand" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="clearLogFileCommand" Visible="False" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<script language="javascript" type="text/javascript">
		function gridInitialized() {
			px_all[pnlNewRevID].events.removeEventHandler("afterHide", refreshScreen);
			px_all[pnlNewRevID].events.addEventHandler("afterHide", refreshScreen);
		}
		function refreshScreen() {
			px_all[pnlNewRevID].hide();
			px_all[gridRevisionsID].refresh();
		}
	</script>
	<px:PXUploadFilePanel ID="dlgUploadFile" runat="server" IgnoreSize="True" OnUpload="OnFileUploadFinished"
		PanelID="pnlUploadFileSmart" CommandSourceID="ds" AllowedTypes=".zip" />
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="VersionRecord" Caption="Current Version" TemplateContainer="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXMaskEdit ID="edCurrentVersion" runat="server" AllowNull="False" DataField="CurrentVersion"
				Enabled="False" />
			<px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
			<px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" DisplayFormat="g"
				EditFormat="g" Enabled="False" Size="SM" />
		</Template>
	</px:PXFormView>
	
	<px:PXSmartPanel ID="pnlLockout" runat="server" CaptionVisible="True" Caption="Schedule Lockout" LoadOnDemand="true" Key="LockoutFilterRecord">
		<px:PXFormView runat="server" ID="frmLockout" DataMember="LockoutFilterRecord" SkinID="Transparent" DataSourceID="ds">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartRow="True"/>
				<px:PXDateTimeEdit ID="edLockoutDate" runat="server" DataField="DateTime_Date" Size="S"/>
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"/>
				<px:PXDateTimeEdit ID="edLockoutTime" runat="server" DataField="DateTime_Time" TimeMode="true" SuppressLabel="True" Size="S" />
				<px:PXLayoutRule runat="server" StartRow="True" ColumnSpan="2" />
				<px:PXTextEdit runat="server" ID="edLockoutReason" DataField="Reason" TextMode="MultiLine" Size="XM"/>
				<px:PXCheckbox ID="chkLockoutAll" runat="server" DataField="LockoutAll" Size="M" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlLockoutButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnLockoutOk" runat="server" DialogResult="OK" Text="Ok">
				<AutoCallBack Command="Save" Target="frmLockout"/>
			</px:PXButton>
			<px:PXButton ID="btnLockoutCancel" runat="server" DialogResult="Cancel" Text="Cancel">
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSmartPanel ID="pnlShowLogFile" runat="server" CaptionVisible="True" Caption="Log"
		ForeColor="Black" Style="position: static" Width="700px"
		LoadOnDemand="true" Key="LogFileFilterRecord" AutoCallBack-Enabled="True" AutoCallBack-Target="frmEditObjectCommand"
		AutoCallBack-Command="Refresh">
		<px:PXFormView ID="frmShowLogFile" runat="server" SkinID="Transparent" DataMember="LogFileFilterRecord"
			DataSourceID="ds" Width="700px">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
					ControlSize="XXL" />
				<px:PXTextEdit Height="228px" ID="edText" runat="server" DataField="Text" CommitChanges="True"
					TextMode="MultiLine" Width="550px" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close">
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>

	<px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" DataMember="VersionRecord">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Updates">
				<Template>
					<px:PXFormView ID="frmPrimaryPage" runat="server" DataSourceID="ds" DataMember="VersionFilterRecord"
						CaptionVisible="False" Width="100%" TemplateContainer="">
						<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
						<ContentStyle BorderStyle="None">
						</ContentStyle>
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXDropDown CommitChanges="True" ID="edBranch" runat="server" DataField="Branch"
								MaxLength="10" Size="M" />
							<px:PXTextEdit CommitChanges="True" ID="edKey" runat="server" DataField="Key" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridAvailableVersions" runat="server" DataSourceID="ds" Style="position: static" 
						Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="Inquire"
						Caption="Available Updates">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="AvailableVersions">
								<Columns>
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" Width="100px" DataField="Version"
										Label="Version" />
									<px:PXGridColumn AllowUpdate="False" DataField="Date" DisplayFormat="g" Label="Published Date"
										Width="200px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Restricted" Label="Restricted" TextAlign="Center"
										Type="CheckBox" Width="60px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Uploaded" Label="Uploaded" TextAlign="Center"
										Type="CheckBox" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Upload Custom Version" Tooltip="Displays dialog to upload new version of the application.">
									<AutoCallBack Command="uploadVersionCommand" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Download Version" Tooltip="Download selected version.">
									<AutoCallBack Command="downloadVersionCommand" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Apply Version" Tooltip="Upgrade current site to selected version.">
									<AutoCallBack Command="applyVersionCommand" Target="ds" />
								</px:PXToolBarButton>
                                
								<px:PXToolBarButton >
									<AutoCallBack Command="validateCompatibility" Target="ds" />
								</px:PXToolBarButton>
                                

							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" />
						<ClientEvents Initialize="gridInitialized" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Update History" Overflow="Hidden">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="500" SkinID="Horizontal" Height="500px" Panel1MinSize="150" Panel2MinSize="150">
						<AutoSize Enabled="true" />
						<Template1>
							        <px:PXGrid ID="gridUPHistory" runat="server" AdjustPageSize="Auto" DataSourceID="ds" ActionsPosition="Top" AllowPaging="True"
								        SkinID="Inquire" Caption="Update History" Width="100%" Height="200px" FeedbackMode="DisableAll">
								        <Levels>
									<px:PXGridLevel DataMember="UpdateHistoryFullRecords">
										        <Columns>
											        <px:PXGridColumn DataField="UpdateID" Label="Maintenance ID" TextAlign="Right" Width="150px" />
                                            <px:PXGridColumn DataField="UPHistoryComponents__UpdateComponentID" Label="Maintenance Components ID" TextAlign="Right" Width="150px" />
											<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Host" Label="Host" Width="150px" />
                                            <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="UPHistoryComponents__ComponentName" Label="Component Name" Width="100px" />
											<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="UPHistoryComponents__ComponentType" Label="Component Type" Width="100px" />
											<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="UPHistoryComponents__FromVersion" Label="From Version" Width="150px" />
											<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="UPHistoryComponents__ToVersion" Label="To Version" Width="150px" />
											        <px:PXGridColumn AllowUpdate="False" DataField="Started" DisplayFormat="g" Label="Started On" Width="150px" />
											        <px:PXGridColumn AllowUpdate="False" DataField="Finished" DisplayFormat="g" Label="Finished On" Width="150px" />
										        </Columns>
									        </px:PXGridLevel>
								        </Levels>
								        <AutoSize Enabled="True" MinHeightLimit="150" MinHeight="150" />
								        <AutoCallBack Command="Refresh" Target="gridUPErrors" ActiveBehavior="True">
									        <Behavior RepaintControlsIDs="gridUPErrors" CommitChanges="True" />
								        </AutoCallBack>
								        <Mode AllowAddNew="False" AllowUpdate="False" />
							        </px:PXGrid>
                                </Template1>
                                <Template2>
							<px:PXGrid ID="gridUPErrors" runat="server" AdjustPageSize="Auto" DataSourceID="ds" ActionsPosition="Top" AllowPaging="True"
								SkinID="Details" Caption="Update Errors" Width="100%" Height="100px" FeedbackMode="DisableAll">
								<Levels>
									<px:PXGridLevel DataMember="UpdateErrorRecords">
										<RowTemplate>
											<%--<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" LabelsWidth="SM" />--%>
											<px:PXLayoutRule runat="server" />
											<px:PXNumberEdit ID="ErrorID" runat="server" DataField="ErrorID" />
											<px:PXTextEdit ID="Message" runat="server" DataField="Message" />
											<px:PXTextEdit ID="Details" runat="server" DataField="Details" TextMode="MultiLine" Width="600px" Height="200px" />
											<px:PXCheckBox ID="Skip" runat="server" DataField="Skip" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn AllowUpdate="False" DataField="ErrorID" Label="Error ID" TextAlign="Right" Width="50px" />
											<px:PXGridColumn AllowUpdate="False" DataField="Message" Label="Error" Width="400px" />
											<px:PXGridColumn AllowUpdate="False" DataField="Stack" Label="Stack Trace" Width="200px" />
											<px:PXGridColumn AllowUpdate="False" DataField="Script" Label="SQL Script" Width="200px" />
											<px:PXGridColumn AllowUpdate="False" DataField="Skip" Label="Skip Error" Width="70px" Type="CheckBox" TextAlign="Center" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinHeightLimit="150" MinHeight="150" />
								<Mode AllowAddNew="False" AllowUpdate="False" AllowFormEdit="True" />
								<Parameters>
									<px:PXSyncGridParam ControlID="gridUPHistory" Name="SyncGrid" />
								</Parameters>
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton Text="Skip Error" Tooltip="Mark this exception as skipped, so i will not appear on login screen.">
											<AutoCallBack Command="skipErrorCommand" Target="ds" />
										</px:PXToolBarButton>
										<px:PXToolBarButton Text="Show Log File" Tooltip="Show text with log of upgrade operations.">
											<AutoCallBack Command="showLogFileCommand" Target="ds" />
										</px:PXToolBarButton>
										<px:PXToolBarButton Text="Clear Log File" Tooltip="Clear log of upgrade operations.">
											<AutoCallBack Command="clearLogFileCommand" Target="ds" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
