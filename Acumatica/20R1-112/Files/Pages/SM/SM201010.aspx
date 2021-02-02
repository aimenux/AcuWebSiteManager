<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	CodeFile="SM201010.aspx.cs" Inherits="Page_SM201010" Title="Untitled Page" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function onCallbackResult(ds, context)
		{
			if (context.command == "loginAsUser")
			{
				window.top.lastUrl = null;
				ds.isDirty = false;
				for (var name in px_all)
				{
					var item = px_all[name];
					if (item && item.getChanged)
					{
						if (item.checkChanges != null) item.checkChanges = false;
					}
				}
				var mainUrl = ((location.href.indexOf("HideScript") > 0) ? "" : "../../") + "Main";
				px.openUrl(mainUrl, "_top");
			}
        }
	</script>
	<pxa:DynamicDataSource ID="ds" runat="server" Visible="True" PrimaryView="UserList" 
		TypeName="PX.SM.AccessUsers,PX.SM.AccessWebUsers" >
		<ClientEvents CommandPerformed="onCallbackResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="InsertUsers" HideText="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveUsers" />
			<px:PXDSCallbackCommand Name="DeleteUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="FirstUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="PrevUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="NextUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="LastUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="ResetPasswordOK" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="addADUserOK" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="addADUser" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="reloadADUsers" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
		    <px:PXDSCallbackCommand Name="GenerateOneTimeCodes" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="gridDevices" Name="disableDevices" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridDevices" Name="enableDevices" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridDevices" Name="deleteDevices" Visible="False" />
		</CallbackCommands>
	</pxa:DynamicDataSource>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" 
		Width="100%" DataMember="UserList" Caption="User Information" ActivityIndicator="true">
		<Template>
		    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="SM"/>
			<px:PXSelector ID="edUsername" runat="server" DataField="Username" AutoRefresh="True" AutoComplete="False">
                <GridProperties FastFilterFields="DisplayName,Email" />
			    <AutoCallBack Command="CancelUsers" Target="ds"/>
			</px:PXSelector>
			<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password"/>
            <px:PXCheckBox ID="edGenerate" runat="server" DataField="GeneratePassword" CommitChanges="True"/>
            <px:PXCheckBox ID="edGuest" runat="server" DataField="Guest" CommitChanges="True"/>
		    <px:PXSelector ID="edLoginType" runat="server" DataField="LoginTypeID" CommitChanges="True"  AllowEdit="True"/>
		    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" CommitChanges="True" AutoRefresh="True" DisplayMode="Text" TextMode="Search" AllowEdit="True">
		         <GridProperties FastFilterFields="DisplayName,FullName,Email" />
            </px:PXSelector>

			<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
			<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
			<px:PXMailEdit CommitChanges="True" ID="edEmail" runat="server" DataField="Email" />
            <px:PXTextEdit ID="edComment" runat="server" DataField="Comment"
                TextMode="MultiLine" Height="45px" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" />
            <px:PXDropDown ID="edState" runat="server" DataField="State" Enabled="False" />
            <px:PXCheckBox ID="chkAllowPasswordRecovery" runat="server" DataField="AllowPasswordRecovery"/>
            <px:PXCheckBox ID="chkPasswordChangeable" runat="server" DataField="PasswordChangeable" />
            <px:PXCheckBox ID="chkPasswordNeverExpires" runat="server" DataField="PasswordNeverExpires" />
            <px:PXCheckBox ID="chkPasswordChangeOnNextLogin" runat="server" DataField="PasswordChangeOnNextLogin" />
            <px:PXNumberEdit ID="edAllowedSessions" runat="server" DataField="AllowedSessions" AllowNull="True"/>
            <px:PXCheckBox ID="chkOverride" runat="server" DataField="OverrideADRoles" CommitChanges="True"/>
            <px:PXDropDown ID="edSource" runat="server" DataField="Source" Visible="False" />
		    <px:PXLayoutRule ID="lrMultiFactor" runat="server" GroupCaption="Two-factor authentication" LabelsWidth="M"/>
		    <px:PXCheckBox ID="edMultiFactorOverride" runat="server" DataField="MultiFactorOverride" CommitChanges="True"/>
            <px:PXDropDown ID="edMultiFactorType" runat="server" DataField="MultiFactorType"/>
            <px:PXLayoutRule runat="server" />
            <px:PXSmartPanel ID="pnlResetPassword" runat="server" Caption="Change password"
                LoadOnDemand="True" Width="400px" Key="UserList" CommandName="ResetPasswordOK" CommandSourceID="ds" AcceptButtonID="btnOk" CancelButtonID="btnCancel" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmResetParams" AutoCallBack-Enabled="true" AutoReload="True">
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                <px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
                <px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK" />
                    <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
                </px:PXPanel>
            </px:PXSmartPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%"  OnCalculateVisibility="tab_DataBound"
		Height="200px" RepaintOnDemand="False" DataSourceID="ds" DataMember="UserListCurrent">
		<Items>
			<px:PXTabItem Key="membership" Text="Roles">
				<Template>
					<px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" SkinID="Inquire"
                        AllowSearch="True" FastFilterFields="Rolename,Rolename_Roles_descr">
						<Levels>
							<px:PXGridLevel DataMember="AllowedRoles">
								<Columns>
						            <px:PXGridColumn AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="80px" AutoCallBack="True"/>
									<px:PXGridColumn DataField="Rolename" Width="200px" AllowUpdate ="False"/>
									<px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" MinWidth="300" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Key="roles" Text="Roles">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" ActionsPosition="Top" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="RoleList">
								<Columns>
									<px:PXGridColumn DataField="Rolename" Width="200px" AutoCallBack="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" MinWidth="300" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Key="statistics" Text="Statistics">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XL"/>
					<px:PXDateTimeEdit ID="edCreationDate" runat="server" DataField="CreationDate" DisplayFormat="f"
						EditFormat="f" Size="SM"/>
					<px:PXTextEdit ID="edLastLoginDate" runat="server" DataField="LastLoginDate" Size="SM"/>
					<px:PXTextEdit ID="edLastLockedOutDate" runat="server" DataField="LastLockedOutDate" Size="SM"/>
					<px:PXTextEdit ID="edLastPasswordChangedDate" runat="server" DataField="LastPasswordChangedDate" Size="SM"/>
					<px:PXNumberEdit ID="edFailedPasswordAttemptCount" runat="server" DataField="FailedPasswordAttemptCount" Size="SM"/>
					<px:PXNumberEdit ID="edFailedPasswordAnswerAttemptCount" runat="server" DataField="FailedPasswordAnswerAttemptCount" Size="SM"/>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="IP filter">
				<Template>
					<px:PXGrid ID="gridFilterIP" runat="server" DataSourceID="ds" Style="z-index: 100"
						Height="150px" Width="100%" ActionsPosition="Top" Caption="Allowed IP Address Ranges"
						BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="UserFilters">
								<Columns>
									<px:PXGridColumn DataField="StartIPAddress" Width="200px" />
									<px:PXGridColumn DataField="EndIPAddress" Width="200px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXMaskEdit ID="edStartIPAddress" runat="server" DataField="StartIPAddress" EmptyChar="0" InputMask="###.###.###.###" />
									<px:PXMaskEdit ID="edEndIPAddress" runat="server" DataField="EndIPAddress" EmptyChar="0" InputMask="###.###.###.###" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" MinWidth="300" />
					</px:PXGrid>
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
									<px:PXGridColumn DataField="Active" Width="90px" TextAlign="Center" Type="CheckBox"  />
									<px:PXGridColumn DataField="UserKey" Width="250px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" MinWidth="300" />
						<Mode AllowAddNew="False" AllowDelete="False" />
						<ActionBar>
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
            <px:PXTabItem Text="Personal Settings">
                <Template>
                    <px:PXFormView runat="server" ID="PrefsForm" DataSourceID="ds" DataMember="UserPrefs" Width="100%" SkinID="Transparent">
                        <Template>
						    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="M"/>
							<px:PXSelector ID="edPdfCertificateName" runat="server" DataField="PdfCertificateName" />
							<px:PXDropDown ID="edTimeZone" runat="server" DataField="TimeZone" />
							<px:PXSelector ID="edBranchCD" runat="server" DataField="DefBranchID" ValueField="BranchID" TextField="BranchCD">
								<GridProperties FastFilterFields="BranchCD">
								</GridProperties>
							</px:PXSelector>
							<px:PXSelector ID="edHomePage" runat="server" DataField="HomePage"  DisplayMode="Text" FilterByAllFields="true" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Key="saleforceSyncStatus" Text="Sync Status">
				<Template>
					<px:PXGrid ID="syncGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="SyncRecs" DataKeyNames="SyncRecordID">
								<Columns>
									<px:PXGridColumn DataField="SYProvider__Name" Width="200px" />
									<px:PXGridColumn DataField="RemoteID" Width="200px" CommitChanges="True" LinkCommand="GoToSalesforce" />
									<px:PXGridColumn DataField="Status" Width="120px" />
									<px:PXGridColumn DataField="Operation" Width="80px" />
									<px:PXGridColumn DataField="SFEntitySetup__ImportScenario" Width="150" />
									<px:PXGridColumn DataField="SFEntitySetup__ExportScenario" Width="150" />
									<px:PXGridColumn DataField="LastErrorMessage" Width="150" />
									<px:PXGridColumn DataField="LastAttemptTS" Width="120px" DisplayFormat="g" />
									<px:PXGridColumn DataField="AttemptCount" Width="120px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Key="SyncSalesforce">
									<AutoCallBack Command="SyncSalesforce" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode InitNewRow="true" />
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Devices" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridDevices" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" AutoRefresh="True" AutoAdjustColumns="True"
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
								<EditRecord Enabled="True" />
								<NoteShow Enabled="False" />
								<AddNew MenuVisible="false" ToolBarVisible="False" />
							</Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="DELETE ALL"  CommandName="deleteDevices" CommandSourceID="ds"/>
                                <px:PXToolBarButton Text="DISABLE ALL" CommandName="disableDevices" CommandSourceID="ds"/>
                                <px:PXToolBarButton Text="ENABLE ALL"  CommandName="enableDevices" CommandSourceID="ds"/>
						    </CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Location Tracking" Key="tabLocationTracking" LoadOnDemand="True" BindingContext="form">
				<Template>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Tracking Settings" />
					<px:PXFormView ID="formLT" runat="server" Width="100%" DataMember="UserPrefs" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" />
						    <px:PXCheckBox CommitChanges="True" ID="edTrackLocation" runat="server" DataField="TrackLocation" />
						    <px:PXLayoutRule runat="server" Merge="True" />
							<px:PXNumberEdit ID="edInterval" runat="server" DataField="Interval" />
							<px:PXLabel Size="xs" ID="lblMinutes" runat="server"> Minutes</px:PXLabel>
							<px:PXLayoutRule runat="server" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXNumberEdit ID="edDistance" runat="server" DataField="Distance" />
							<px:PXLabel Size="xs" ID="lblMeters" runat="server"> Meters</px:PXLabel>
							<px:PXLayoutRule runat="server" />
                        </Template>
                    </px:PXFormView>
					<px:PXGrid ID="gridLT" runat="server" DataSourceID="ds" Height="200px" Width="350px" FilesIndicator="False" NoteIndicator="False" SkinID="ShortList">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="LocationTracking">
                                <RowTemplate>
                                    <px:PXDropDown ID="edWeekDay" runat="server" DataField="WeekDay" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edStartTime" runat="server" CommitChanges="True" DataField="StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" />
                                    <px:PXDateTimeEdit IID="edEndTime" runat="server" CommitChanges="True" DataField="EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="WeekDay" Width="50px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="StartTime" DisplayFormat="t" TimeMode="True" Width="50px" />
                                    <px:PXGridColumn DataField="EndTime" DisplayFormat="t" TimeMode="True" Width="50px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" MinWidth="300" />
	</px:PXTab>
    <px:PXSmartPanel ID="pnlAddADUser" runat="server" Key="ADUser" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmADUser" AutoCallBack-Enabled="true"
        LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Active Directory User" CaptionVisible="True" CommandName="addADUserOK" CommandSourceID="ds" >
        <px:PXFormView ID="frmADUser" runat="server" DataSourceID="ds" Style="z-index: 108" Width="100%" DataMember="ADUser"
            Caption="Active Directory User" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" />
                <px:PXSelector CommitChanges="True" ID="edADUsername" runat="server" DataField="Username"/>
            </Template>
        </px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">        
            <px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
