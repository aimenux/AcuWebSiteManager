<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204015.aspx.cs" Inherits="Page_SM204015" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:DynamicDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Servers" TypeName="PX.SM.EMailSyncServerMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" PopupVisible="True" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</pxa:DynamicDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Servers" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" Caption="Server Summary" DefaultControlID="AccountCD">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" /> 
			<px:PXSelector ID="edAccountCD" runat="server" DataField="AccountCD" DisplayMode="Text" />		
			<px:PXTextEdit ID="edAddress" runat="server" DataField="Address"  />
			<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" />
			<px:PXDropDown ID="edLoggingLevel" runat="server" DataField="LoggingLevel"  />

			<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" /> 
			<px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" />
			<px:PXSelector ID="edDefaultPolicyName" runat="server" DataField="DefaultPolicyName" DisplayMode="Value" AutoRefresh="True"   />
			<px:PXTextEdit ID="edServerUrl" runat="server" DataField="ServerUrl"  />
			<px:PXDropDown ID="edConnectionMode" runat="server" DataField="ConnectionMode"  />
			
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" /> 
			<px:PXNumberEdit ID="edSyncProcBatch" runat="server" DataField="SyncProcBatch" AllowNull="true" />
			<px:PXNumberEdit ID="edSyncUpdateBatch" runat="server" DataField="SyncUpdateBatch" AllowNull="true" />
			<px:PXNumberEdit ID="edSyncSelectBatch" runat="server" DataField="SyncSelectBatch" AllowNull="true" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="true" /> 
			<px:PXNumberEdit ID="edSyncAttachmentSize" runat="server" DataField="SyncAttachmentSize" AllowNull="true" />
			<px:PXLabel ID="PXLabel1" runat="server">KB</px:PXLabel>
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" /> 
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds"  Style="z-index: 100" Width="100%" ActionsPosition="Top" Caption="Accounts" SkinID="Inquire" AdjustPageSize="Auto">
		<Mode AllowAddNew="True" AllowDelete="False" />
		<Levels>
			<px:PXGridLevel DataMember="SyncAccounts">
				<RowTemplate>
					<px:PXSelector ID="edEmployeeID" DataField="EmployeeID" runat="server" AllowEdit="true" />
					<px:PXSelector ID="edPolicyName" DataField="PolicyName" runat="server" AllowEdit="true" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="SyncAccount" AllowUpdate="False" Width="100px" Type="CheckBox" TextAlign="Center" />	
					<px:PXGridColumn DataField="EmployeeID" AllowUpdate="True" Width="100px" CommitChanges="true"/>
					<px:PXGridColumn DataField="EmployeeCD" AllowUpdate="False" Width="150px" />																	
					<px:PXGridColumn DataField="Address" AllowUpdate="False" Width="300px" />	
					<px:PXGridColumn DataField="PolicyName" AllowUpdate="False" Width="300px" DisplayMode="Value" />	
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<Actions>
				<Delete MenuVisible="false" Enabled="false" ToolBarVisible="false" />
				<AddNew MenuVisible="false" Enabled="false" ToolBarVisible="false" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
