<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204002.aspx.cs" Inherits="Page_SM204002" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <pxa:DynamicDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="EMailAccounts" TypeName="PX.SM.EMailAccountMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" PopupVisible="True" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="checkEMailAccount" PopupVisible="True" StartNewGroup="True"
				CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Action" PopupVisible="True" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="sendAll" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="receiveAll" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="sendReceiveAll" Visible="False" CommitChanges="True"/>
		</CallbackCommands>
	</pxa:DynamicDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="EMailAccounts" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True"
		NotifyIndicator="True" Caption="Email Account Summary" DefaultControlID="edEmailAccountID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" /> 
			<px:PXSelector ID="edEmailAccountID" runat="server" DataField="EmailAccountID" NullText="<NEW>" TextMode="Search" DisplayMode="Text" AutoRefresh="True" FilterByAllFields="True" />		
			<px:PXTextEdit ID="edDescriptionOld" runat="server" DataField="Description" />
			<px:PXTextEdit ID="edAddress" runat="server" DataField="Address" />
			<px:PXTextEdit ID="edReplyAddress" runat="server" DataField="ReplyAddress" />

			<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" /> 
			<px:PXDropDown ID="edEmailAccountType" runat="server" DataField="EmailAccountType" CommitChanges="True" AllowNull="False" /> 
			<%--<px:PXLabel ID="PXHole" runat="server" />--%>
			<px:PXDropDown ID="edSenderDisplayNameSource" runat="server" DataField="SenderDisplayNameSource" CommitChanges="true" /> 
			<px:PXTextEdit ID="edAccountDisplayName" runat="server" DataField="AccountDisplayName" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="387px" Style="z-index: 100"
		Width="100%" DataMember="CurrentEMailAccounts" DefaultControlID="edDescription">
		<Items>
			<px:PXTabItem Text="Servers" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Server Information" />
					<px:PXDropDown CommitChanges="True" ID="edIncomingHostProtocol" runat="server" DataField="IncomingHostProtocol"	AllowNull="False" />
					<px:PXTextEdit ID="edImapRootFolder" runat="server" DataField="ImapRootFolder" />
					<px:PXTextEdit ID="edIncomingHostName" runat="server" DataField="IncomingHostName" />
					<px:PXTextEdit ID="edOutcomingHostName" runat="server" DataField="OutcomingHostName" />
				    <px:PXTextEdit ID="edGroupMail" runat="server" DataField="SendGroupMails" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Logon Information" />
					<px:PXTextEdit ID="edLoginName" runat="server" DataField="LoginName" />
					<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password"/>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Advanced Settings" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" StartGroup="True" GroupCaption="Security" SuppressLabel="True"/>
					<px:PXCheckBox CommitChanges="True" ID="chkOutcomingAuthenticationRequest"
						runat="server" DataField="OutcomingAuthenticationRequest" />
					<px:PXCheckBox CommitChanges="True" ID="chkOutcomingAuthenticationDifferent"
						runat="server" DataField="OutcomingAuthenticationDifferent" />
					<px:PXTextEdit ID="edOutcomingLoginName" runat="server" DataField="OutcomingLoginName" SuppressLabel="False"/>
					<px:PXTextEdit ID="edOutcomingPassword" runat="server" DataField="OutcomingPassword" SuppressLabel="False" TextMode="Password"/>
					<px:PXCheckBox CommitChanges="True" ID="chkValidateFrom" runat="server" DataField="ValidateFrom" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Server Port Numbers" ControlSize="S" LabelsWidth="XM" SuppressLabel="True"/>
					<px:PXNumberEdit ID="edIncomingPort" runat="server" AllowNull="False" DataField="IncomingPort" SuppressLabel="False"/>
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIncomingSSLRequest"	runat="server" DataField="IncomingSSLRequest"/>
					<px:PXNumberEdit ID="edOutcomingPort" runat="server" AllowNull="False" DataField="OutcomingPort" SuppressLabel="False"/>
					<px:PXDropDown ID="chkOutcomingSSLRequest" runat="server" DataField="OutcomingSSLRequest"  Text="Outcoming SSL Request" NullText="None" SuppressLabel="False" CommitChanges="True"/>
					<px:PXDropDown ID="edTimeout" runat="server" DataField="Timeout"  NullText="None" SuppressLabel="False" CommitChanges="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Incoming Mail Processing">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" SuppressLabel="True" />
				    <px:PXCheckBox ID="chkIncomingProcessing" runat="server" DataField="IncomingProcessing">
						<AutoCallBack Enabled="true" Command="Save" Target="tab" />
					</px:PXCheckBox>  
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Initial Processing" ControlSize="M" LabelsWidth="SM" SuppressLabel="True"/>
					<px:PXCheckBox ID="chkConfirmReceipt" runat="server" DataField="ConfirmReceipt">
						<AutoCallBack Enabled="true" Command="Save" Target="tab" />
					</px:PXCheckBox>
					<px:PXSelector ID="edConfirmReceiptTemplate" runat="server" PopulateOnDemand="True"
						DataField="ConfirmReceiptNotificationID" SuppressLabel="False" Size="L" DisplayMode="Text">
						<AutoCallBack Enabled="True" Command="Save" Target="tab" />
					</px:PXSelector> 
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="True" GroupCaption="Main Processing" ControlSize="M" LabelsWidth="SM" SuppressLabel="True"/>
					<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="CreateCase" CommitChanges="true" />
                    <px:PXSelector ID="edCreateCaseClassID" runat="server" DataField="CreateCaseClassID" SuppressLabel="False" Size="M" DisplayMode="Hint" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False" />
						</GridProperties>
					</px:PXSelector>
					<px:PXCheckBox ID="edRouteEmployeeEmails" runat="server" DataField="RouteEmployeeEmails" />
					<px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="CreateActivity" />
					<px:PXCheckBox ID="PXCheckBox3" runat="server" DataField="CreateLead" CommitChanges="true" />
                    <px:PXSelector ID="edCreateLeadClassID" runat="server" DataField="CreateLeadClassID" AllowEdit="True" SuppressLabel="False" Size="M" DisplayMode="Hint"/>
					<px:PXCheckBox ID="chkProcessUnassigned" runat="server" DataField="ProcessUnassigned">
						<AutoCallBack Enabled="true" Command="Save" Target="tab" />
					</px:PXCheckBox>
					<px:PXSelector ID="edRespnseTemplate" runat="server" PopulateOnDemand="True"
						DataField="ResponseNotificationID" SuppressLabel="False" Size="L" DisplayMode="Text">
						<AutoCallBack Enabled="True" Command="Save" Target="tab" />
					</px:PXSelector>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" GroupCaption="Final Processing" ControlSize="M" LabelsWidth="SM" SuppressLabel="True" Merge="True"/>
					<px:PXCheckBox ID="chkDeleteUnProcessed" runat="server" DataField="DeleteUnProcessed" >
						<AutoCallBack Enabled="True" Command="Save" Target="tab" />
					</px:PXCheckBox>
					<px:PXDropDown ID="edTypeDelete" runat="server" DataField="TypeDelete" AllowNull="false"
						Size="XS" />
					<px:PXLabel ID="PXLabel3" runat="server" Text="processing" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" LabelsWidth="SM" ControlSize="M"
						SuppressLabel="True" />
					<px:PXCheckBox ID="chkAddUpInformation" runat="server" DataField="AddUpInformation" />
				</Template>
			</px:PXTabItem>	
			<px:PXTabItem Text="Content">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Content" ControlSize="S" LabelsWidth="SM" SuppressLabel="True"/>
					<px:PXLayoutRule runat="server"  ControlSize="S" LabelsWidth="M" SuppressLabel="True" />
					<px:PXCheckBox ID="chkIncomingDelSuccess" runat="server" DataField="IncomingDelSuccess" />
					<px:PXLabel ID="lblCommentAttach" runat="server" Text="Specify the extensions for allowed attachment types separating them with comma: .cvs, .jpg, .png, .xls"
						SuppressLabel="False"/>	 
					<px:PXTextEdit ID="edIncomingAttachmentType" runat="server" DataField="IncomingAttachmentType" Size="XXL" SuppressLabel="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Assignment Settings">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
					<px:PXSelector ID="edDefaultAssignmentMapID" runat="server" DataField="DefaultEmailAssignmentMapID"
						TextField="Name" AllowEdit="True" AutoRefresh="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXSelector ID="WorkgroupID" runat="server" DataField="DefaultWorkgroupID" CommitChanges="True"
						TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="True" />
					<px:PXSelector ID="OwnerID" runat="server" DataField="DefaultOwnerID" TextMode="Search" CommitChanges="True"
						DisplayMode="Text" FilterByAllFields="True" AutoRefresh="True" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
