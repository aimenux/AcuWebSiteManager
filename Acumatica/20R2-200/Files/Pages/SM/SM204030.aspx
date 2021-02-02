<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204030.aspx.cs" Inherits="Page_SM204030"
	Title="Synchronization Processing" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CS.Email.EmailsSyncMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Status" CommitChanges="true" DependOnGrid="grid" RepaintControls="All" Visible="False" PostData="Page" />
			<px:PXDSCallbackCommand Name="ResetContacts" Visible="False"/>
			<px:PXDSCallbackCommand Name="ViewEmployee" Visible="false"/>
			<px:PXDSCallbackCommand Name="ResetTasks" Visible="False"/>
			<px:PXDSCallbackCommand Name="ResetEvents" Visible="False"/>
            <px:PXDSCallbackCommand Name="ResetEmails" Visible="False"/>
			<px:PXDSCallbackCommand Name="ClearLog" Visible="False"/>
			<px:PXDSCallbackCommand Name="ResetWarning" Visible="False"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="pnlStatus" runat="server" 
                     CaptionVisible="True" Caption="Synchronization Status" DesignView="Content" Key="CurrentItem" 
		AutoCallBack-ActiveBehavior="true" AutoCallBack-Behavior-RepaintControls="All" 
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AutoRepaint="true" Height="600px" Width="900px"
        AutoCallBack-Enabled="true" AutoCallBack-Target="frmStatus" AutoCallBack-Command="Refresh" LoadOnDemand="True" >
		<px:PXFormView ID="frmStatus" runat="server" SkinID="Transparent" DataMember="CurrentItem" DataSourceID="ds">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXSelector ID="edServer" runat="server" AllowNull="False" DataField="ServerID" Enabled="false"  />
				<px:PXTextEdit ID="edAddress" runat="server" AllowNull="False" DataField="Address" Enabled="false"  />
				<px:PXLayoutRule ID="PXLayoutRule10" runat="server"/>
				
				<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edContactsExportedDate_Date" runat="server" DataField="ContactsExportDate_Date" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edContactsExportedDate_Time" runat="server" DataField="ContactsExportDate_Time"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edContactsImportDate_Date" runat="server" DataField="ContactsImportDate_Date" Enabled="false" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edContactsImportDate_Time" runat="server" DataField="ContactsImportDate_Time" Enabled="false"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
				<px:PXButton ID="btnContactsReset" runat="server" DialogResult="Cancel" CommandName="ResetContacts" CommandSourceID="ds" Text="RESET" />
				<px:PXLayoutRule ID="PXLayoutRule7" runat="server"/>
				
				<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edTasksExportDate_Date" runat="server" DataField="TasksExportDate_Date" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edTasksExportDate_Time" runat="server" DataField="TasksExportDate_Time"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edTasksImportDate_Date" runat="server" DataField="TasksImportDate_Date" Enabled="false" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edTasksImportDate_Time" runat="server" DataField="TasksImportDate_Time" Enabled="false"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
				<px:PXButton ID="btnTasksReset" runat="server" DialogResult="Cancel" CommandName="ResetTasks" CommandSourceID="ds" Text="RESET" />
				<px:PXLayoutRule ID="PXLayoutRule8" runat="server"/>
				
				<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEventsExportDate_Date" runat="server" DataField="EventsExportDate_Date" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEventsExportDate_Time" runat="server" DataField="EventsExportDate_Time"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEventsImportDate_Date" runat="server" DataField="EventsImportDate_Date" Enabled="false" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEventsImportDate_Time" runat="server" DataField="EventsImportDate_Time" Enabled="false"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
				<px:PXButton ID="btnEventsReset" runat="server" DialogResult="Cancel" CommandName="ResetEvents" CommandSourceID="ds" Text="RESET" />
				<px:PXLayoutRule ID="PXLayoutRule9" runat="server"/>
				
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEmailsExportDate_Date" runat="server" DataField="EmailsExportDate_Date" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEmailsExportDate_Time" runat="server" DataField="EmailsExportDate_Time"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEmailsImportDate_Date" runat="server" DataField="EmailsImportDate_Date" Enabled="false" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edEmailsImportDate_Time" runat="server" DataField="EmailsImportDate_Time" Enabled="false"
                                   TimeMode="true" SuppressLabel="True" Width="84" />
                <px:PXButton ID="btnEmailsReset" runat="server" DialogResult="Cancel" CommandName="ResetEmails" CommandSourceID="ds" Text="RESET" />
				<px:PXLayoutRule ID="PXLayoutRule12" runat="server"/>
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="gridLog" runat="server" DataSourceID="ds" Height="200px" SkinID="Inquire" Caption="Log" Width="100%" AutoAdjustColumns="true" 
			AllowEdit="True" FastFilterFields="Message" AllowPaging="True" PageSize="200" >
			<ActionBar Position="TopAndBottom" CustomItemsGroup="1" PagerGroup="0" PagerOrder="10" PagerVisible="False" ActionsText="False">
				<Actions>
					<FilterSet GroupIndex="0" Order="0" ToolBarVisible="False" Enabled="False" />
					<FilterShow GroupIndex="0" Order="0" ToolBarVisible="False" Enabled="False" />
					<FilterBar ToolBarVisible="Top" GroupIndex="2" Order="10" Enabled="True" />
				</Actions>
				<CustomItems>
					<px:PXToolBarButton Text="Clear Log" >
						<AutoCallBack Command="ClearLog" Target="ds" />
					    <ActionBar GroupIndex="2" Order="9" />
					</px:PXToolBarButton>
                    <px:PXToolBarButton Text="Reset Warning" >
                        <AutoCallBack Command="ResetWarning" Target="ds" />
                        <ActionBar GroupIndex="2" Order="10" />
                    </px:PXToolBarButton>
				</CustomItems>
			</ActionBar>
			<Levels>
				<px:PXGridLevel DataMember="OperationLog" >
					<RowTemplate>
						<px:PXSelector ID="selServerID" runat="server" DataField="ServerID" />
					</RowTemplate>
					<Columns>
						<px:PXGridColumn AllowUpdate="False" DataField="ServerID" RenderEditorText="True" Width="100px" />
						<px:PXGridColumn AllowUpdate="False" DataField="Address" RenderEditorText="True" Width="150px" />
						<px:PXGridColumn AllowUpdate="False" DataField="Level" Width="100px" />
						<px:PXGridColumn AllowUpdate="False" DataField="Date" Width="150px" DisplayFormat="g" />
						<px:PXGridColumn AllowUpdate="False" DataField="Message" Width="500px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" MinHeight="150" />
		</px:PXGrid>
		<px:PXFormView ID="frmBtn" runat="server" SkinID="Transparent" DataMember="CurrentItem" DataSourceID="ds">
			<Template>
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnCopyCompanyCancel" runat="server" DialogResult="No" Text="Close" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Operation" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edServer" runat="server" AllowNull="True" DataField="ServerID" CommitChanges="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edPolicyName" runat="server" AllowNull="True" DataField="PolicyName" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; height: 283px;" Width="100%" SkinID="Details" Caption="Accounts" 
		AutoAdjustColumns="true" AllowEdit="True" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="SelectedItems" >
				<RowTemplate>
					<px:PXSelector ID="edServerID" runat="server" DataField="ServerID" AllowEdit="True" />
					<px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" AllowEdit="True" />
					<px:PXSelector ID="edEmailAccountID" runat="server" DataField="EmailAccountID" AllowEdit="True" />
					<px:PXSelector ID="edPolicyName" runat="server" DataField="PolicyName" AllowEdit="True"  />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center"	Type="CheckBox" Width="70" />
					<px:PXGridColumn AllowUpdate="False" DataField="ServerID" RenderEditorText="True" Width="150px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Address" RenderEditorText="True" Width="200px" />
					<px:PXGridColumn AllowUpdate="False" DataField="EmailAccountID" Width="200px" TextAlign="Right" DisplayMode="Text"  />
					<px:PXGridColumn AllowUpdate="False" DataField="EmployeeID" Width="100px" DisplayMode="Value" LinkCommand="ViewEmployee"  />
					<px:PXGridColumn AllowUpdate="False" DataField="EmployeeCD" Width="150px" />		
					<px:PXGridColumn AllowUpdate="False" DataField="PolicyName" Width="200px" DisplayMode="Value"  />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<Actions>
				<Delete MenuVisible="false" ToolBarVisible="false" />
				<AddNew MenuVisible="false" ToolBarVisible="false" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Text="Synchronization Status" Key="cmdStatus">
				    <AutoCallBack Command="Status" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
