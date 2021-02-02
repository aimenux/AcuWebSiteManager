<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM203010.aspx.cs" Inherits="Page_SM201020"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:DynamicDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True"
		Width="100%" PrimaryView="UserProfile" TypeName="PX.SM.MyProfileMaint,PX.SM.SMAccessPersonalMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveUsers" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="GetCalendarSyncUrl" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="resetTimeZone" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="manifestHelp" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="changePassword" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="changeEmail" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="changeSecretAnswer" Visible="false" />
		    <px:PXDSCallbackCommand Name="GenerateOneTimeCodes" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="associateUser" CommitChanges="True" Visible="false" DependOnGrid="gridIdentities" />
		</CallbackCommands>
	</pxa:DynamicDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="400px" Style="z-index: 100"
		Width="100%" DataMember="UserProfile" Caption="Edit your profile" OnDataBound="tab_DataBound"
		OnInit="tab_Init" RepaintOnDemand="False">
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="User Settings" ControlSize="XL" LabelsWidth="M" />

					<px:PXControlParam Type="String" PropertyName='NewDataKey["Username"]' Name="Username" ControlID="tab" />
					<px:PXTextEdit ID="edUsername" runat="server" DataField="Username" Enabled="false" />
					<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
					<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
					<px:PXMaskEdit ID="edPhone" runat="server" DataField="Phone" />
					<px:PXLayoutRule ID="PXLayoutRule13" runat="server" Merge="True" />
					<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" Enabled="False" />
					<px:PXButton ID="PXButton1" Width="215px"
						runat="server" Text="Change Email" CommandName="changeEmail" CommandSourceID="ds">
					</px:PXButton>
					<px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />
					<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" Enabled="False" TextMode="Password" />
					<px:PXButton ID="btnChangePassword" Width="215px"
						runat="server" Text="Change Password" CommandName="changePassword" CommandSourceID="ds">
					</px:PXButton>
					<px:PXLayoutRule ID="PXLayoutRule16" runat="server" Merge="True" />
					<px:PXTextEdit ID="edPasswordQuestion" runat="server" DataField="PasswordQuestion" />
					<px:PXButton ID="btnChangeAnswer" Width="215px"
						runat="server" Text="Change Answer" CommandName="changeSecretAnswer" CommandSourceID="ds">
					</px:PXButton>
					<px:PXLayoutRule ID="PXLayoutRule14" runat="server" />
					<px:PXTextEdit ID="edComment" runat="server" DataField="Comment" TextMode="MultiLine" />

					<px:PXFormView ID="formUserPrefs" runat="server" DataMember="UserPrefs" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Personal Settings" ControlSize="XL" LabelsWidth="M" />
							<px:PXSelector ID="edPdfCertificateName" runat="server" DataField="PdfCertificateName" />
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
							<px:PXDropDown ID="edTimeZone" runat="server" DataField="TimeZone" />
							<px:PXButton Size="m" ID="btnResetTimeZone" runat="server" Text="Reset"
								ToolTip="Reset To Calendar Time Zone" CommandSourceID="ds" CommandName="resetTimeZone">
							</px:PXButton>
							<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
							<px:PXSelector ID="edBranchCD" runat="server" DataField="DefBranchID" ValueField="BranchID"
								TextField="BranchCD" CommitChanges="True" TextMode="Search">
								<GridProperties FastFilterFields="BranchCD">
								</GridProperties>
							</px:PXSelector>
							<px:PXSelector runat="server" ID="edDfltBranchLocationID" DataField="DfltBranchLocationID" AutoRefresh="True" />
							<px:PXSelector ID="edDfltSrvOrdType" runat="server" DataField="DfltSrvOrdType" DataSourceID="ds" AutoRefresh="True" />
							<px:PXCheckBox ID="edAskForSrvOrdTypeInCalendars" runat="server" DataField="AskForSrvOrdTypeInCalendars" />
                            <px:PXSelector ID="edDefSite" runat="server" DataField="DefaultSite" />
                            <px:PXSelector ID="edDefaultScanner" runat="server" DataField="DefaultScannerID" CommitChanges="True" AutoRefresh="true"/>
							<px:PXSelector ID="edDefaultScales" runat="server" DataField="DefaultScalesID" CommitChanges="True" AutoRefresh="true"/>
							<px:PXSelector ID="edHomePage" runat="server" DataField="HomePage"  DisplayMode="Text" FilterByAllFields="true" />
							<px:PXCheckBox ID="chkSuggest" runat="server" DataField="DisableSuggest" />
							<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartGroup="True" GroupCaption="Default Automated Operations" ControlSize="XL" LabelsWidth="M" />
							<px:PXDropDown ID="edDefaultPPSMode" runat="server" DataField="PPSMode" CommitChanges="True" />
							<px:PXDropDown ID="edDefaultRPAMode" runat="server" DataField="RPAMode" CommitChanges="True" />
						</Template>
					</px:PXFormView>
					<px:PXSmartPanel ID="pnlChangePassword" runat="server" Caption="Change Password" CaptionVisible="True"
						LoadOnDemand="True" Width="400px"
						Key="Passwords" ShowAfterLoad="true"
						AutoCallBack-Enabled="true" AutoCallBack-Target="formPasswords" AutoCallBack-Command="Refresh"
						CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
						AcceptButtonID="btnOk" CancelButtonID="btnCancel" AutoReload="True">
						<px:PXFormView ID="formPasswords" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
							DataMember="Passwords">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
								</px:PXLayoutRule>
								<px:PXTextEdit ID="edOldPassword" runat="server" DataField="OldPassword" TextMode="Password" Required="True" />
								<px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
								<px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
							</Template>
						</px:PXFormView>
						<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
							<px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK" />
							<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
						</px:PXPanel>
					</px:PXSmartPanel>
					<px:PXSmartPanel ID="pnlChangeEmail" runat="server" Caption="Change Email" CaptionVisible="True"
						LoadOnDemand="True" Width="400px"
						Key="NewEmail" ShowAfterLoad="true"
						AutoCallBack-Enabled="true" AutoCallBack-Target="formNewEmail" AutoCallBack-Command="Refresh"
						CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
						AcceptButtonID="btnOk2" CancelButtonID="btnCancel2">
						<px:PXFormView ID="formNewEmail" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
							DataMember="NewEmail">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
								</px:PXLayoutRule>
								<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" Required="True" CommitChanges="True" />
								<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" Required="True" />
							</Template>
						</px:PXFormView>
						<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
							<px:PXButton ID="btnOk2" runat="server" DialogResult="OK" Text="OK" />
							<px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" />
						</px:PXPanel>
					</px:PXSmartPanel>
					<px:PXSmartPanel ID="pnlChangeAnswer" runat="server" Caption="Change Password Recovery Answer" CaptionVisible="True"
						LoadOnDemand="True" Width="400px"
						Key="NewAnswer" ShowAfterLoad="true"
						AutoCallBack-Enabled="true" AutoCallBack-Target="formNewAnswer" AutoCallBack-Command="Refresh"
						CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
						AcceptButtonID="btnOk3" CancelButtonID="btnCancel3">
						<px:PXFormView ID="formNewAnswer" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
							DataMember="NewAnswer">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
								</px:PXLayoutRule>
								<px:PXTextEdit ID="edAnswer" runat="server" DataField="PasswordAnswer" Required="True" />
								<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" Required="True" />
							</Template>
						</px:PXFormView>
						<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
							<px:PXButton ID="btnOk3" runat="server" DialogResult="OK" Text="OK" />
							<px:PXButton ID="btnCancel3" runat="server" DialogResult="Cancel" Text="Cancel" />
						</px:PXPanel>
					</px:PXSmartPanel>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Key="EmailSettings" Text="Email Settings">
				<Template>
					<px:PXFormView ID="form" runat="server" Width="100%" DataMember="UserPrefs" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSelector ID="edDefaultEMailAccountID" runat="server" DataField="DefaultEMailAccountID" DataSourceID="ds"
								ValueField="emailAccountID" TextField="Address" MaxLength="30" TextMode="Search"/>
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="form2" runat="server" Width="100%" DataMember="CalendarSettings" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXCheckBox ID="chbIsPublic" runat="server" DataField="IsPublic" />
							<px:PXButton ID="cmdCheckMailSettings" runat="server" Text="Synchronization URL"
								OnCallBack="cmdCheckMailSettings_CallBack">
								<AutoCallBack Command="X"></AutoCallBack>
							</px:PXButton>
							<px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="true" LabelsWidth="SM" ControlSize="M" />
							<asp:HyperLink CssClass="login_link"  Style="margin-left: 160px; margin-top:5px;" Height="15px" runat="server" ID="OutlookAddin" NavigateUrl="~/OutlookAddinManifest">Download Outlook Add-In Manifest</asp:HyperLink>
							<px:PXButton ID="btnHelp" runat="server" Text="Help" DisplayStyle="Image" CommandSourceID="ds" 
								CommandName="manifestHelp" Style="min-width:10px; width:20px; border-style: none;padding-left:0px;padding-right:0px;height:20px;padding-top:0px;background-color:Transparent;">
								<Images Normal="~/Wiki/image/ico4.png" />
							</px:PXButton>
                            </Template>
					</px:PXFormView>
                    <px:PXFormView ID="PXFormView2" runat="server" Width="100%" DataMember="UserPrefs" DataSourceID="ds" CaptionVisible="false" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartRow="true" GroupCaption="User Email Signature" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox ID="chkSignatureToNewEmail" runat="server" DataField="SignatureToNewEmail"/>
                            <px:PXCheckBox ID="chkSignatureToReplyAndForward" runat="server" DataField="SignatureToReplyAndForward"/>
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="PXFormView1" runat="server" Width="100%" DataMember="UserPrefs" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
						<Template>
							<px:PXRichTextEdit ID="wikiEdit" runat="server" DataField="MailSignature" AllowLinkEditor="true" AllowLoadTemplate="true" AllowImageEditor="true" FilesContainer="UserProfile" Width="100%" Style="z-index: 113; border-width: 0px;"
								AllowAttached="true" AllowSearch="true" AllowSourceMode="true"  >
								<LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M" />
								<AutoSize Enabled="True" />
							</px:PXRichTextEdit>
						</Template>
						<AutoSize Enabled="True" />
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Custom Locale Format">
				<Template>
					<px:PXFormView ID="formEditFormat" runat="server" DataSourceID="ds" Style="z-index: 100"
						Width="100%" DataMember="LocaleFormats" SkinID="Transparent" DataKeyNames="FormatID"
						Caption="Locale Preferences">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
								ColumnSpan="2" ColumnWidth="XXL" />
							<px:PXSelector CommitChanges="True" ID="edTemplateLocale" runat="server" DataField="TemplateLocale"
								DataSourceID="ds" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Date and Time Formats"
								ControlSize="M" LabelsWidth="SM" StartColumn="True" StartRow="True" />
							<px:PXSelector ID="edDateTimePattern" runat="server" DataField="DateTimePattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edTimeShortPattern" runat="server" DataField="TimeShortPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edTimeLongPattern" runat="server" DataField="TimeLongPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edDateShortPattern" runat="server" DataField="DateShortPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edDateLongPattern" runat="server" DataField="DateLongPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXTextEdit ID="edAMDesignator" runat="server" DataField="AMDesignator" />
							<px:PXTextEdit ID="edPMDesignator" runat="server" DataField="PMDesignator" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
								GroupCaption="Number Format" StartGroup="True" />
							<px:PXDropDown CommitChanges="True" ID="edNumberDecimalSeporator" runat="server"
								AllowEdit="True" DataField="NumberDecimalSeporator" />
							<px:PXDropDown CommitChanges="True" ID="edNumberGroupSeparator" runat="server" AllowEdit="True"
								DataField="NumberGroupSeparator" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="External Identities" Key="roles" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridIdentities" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" Heigth="100%" ActionsPosition="Top" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Identities">
								<Columns>
									<px:PXGridColumn DataField="ProviderName" Width="108px" />
									<px:PXGridColumn DataField="Active" Width="90px" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="UserKey" Width="250px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
						<Mode AllowAddNew="False" AllowDelete="False" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Associate" AutoPostBack="True" CommandName="associateUser" CommandSourceID="ds" />
							</CustomItems>
							<Actions>
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
								<AddNew MenuVisible="false" ToolBarVisible="False" />
								<Delete MenuVisible="false" ToolBarVisible="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Devices" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridDevices" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" Heigth="100%" ActionsPosition="Top" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="UserDevices">
								<Columns>
									<px:PXGridColumn DataField="Enabled" Width="160px" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="ApplicationInstanceID" Width="260px" />
                                    <px:PXGridColumn DataField="DeviceName" Width="200px" />
                                    <px:PXGridColumn DataField="DeviceModel" Width="200px" />
                                    <px:PXGridColumn DataField="DeviceOS" Width="200px" />
                                    <px:PXGridColumn DataField="ExpiredToken" Width="160px" TextAlign="Center" Type="CheckBox" />
								    <px:PXGridColumn DataField="IsConfirmation" Width="160px" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
						<Mode AllowAddNew="False" AllowDelete="True" />
						<ActionBar>
							<Actions>
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
								<AddNew MenuVisible="false" ToolBarVisible="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Printing Settings" LoadOnDemand="True" Key="printingSettings">
				<Template>
					<px:PXFormView ID="formUserPrefsDH" runat="server" DataMember="UserPrefs">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule51" runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="M" />
							<px:PXSelector ID="edDefprinterID" runat="server" DataField="DefaultPrinterID" CommitChanges="True" AutoRefresh="true"/>
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Printers by Report" AdjustPageSize="Auto" AllowPaging="True">
						<Levels>
							<px:PXGridLevel DataMember="Notifications">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edSetupID" runat="server" DataField="SetupID" CommitChanges="True" />
									<px:PXTextEdit ID="edReportID" runat="server" DataField="ReportID" />
									<px:PXSelector ID="edDefPrinterID" runat="server" DataField="DefaultPrinterID" />
									<px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia" />
									<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SetupID" CommitChanges="True" />
									<px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" AutoCallBack="True" />
									<px:PXGridColumn DataField="DefaultPrinterID" />
									<px:PXGridColumn DataField="ShipVia" />
									<px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
	</px:PXTab>
</asp:Content>
