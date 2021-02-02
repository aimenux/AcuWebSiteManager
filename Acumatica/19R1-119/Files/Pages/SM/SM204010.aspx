<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204010.aspx.cs" Inherits="Page_SM204010" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:DynamicDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="SyncPolicy" TypeName="PX.SM.EMailSyncPolicyMaint">
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
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="SyncPolicy" Caption="Synchronization Policy Summary" DefaultControlID="edPolicyName">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" /> 
			<px:PXSelector ID="edPolicyName" runat="server" DataField="PolicyName" DisplayMode="Text" AutoRefresh="True" FilterByAllFields="True" />		
			<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" /> 
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Size ="L" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="387px" Style="z-index: 100"
		Width="100%" DataMember="CurrentSyncPolicy" DefaultControlID="edDescription">
		<Items>
			<px:PXTabItem Text="Synchronization Settings">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Fieldset" Caption="General">
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
						<px:PXTextEdit ID="edCategory" runat="server" DataField="Category" CommitChanges="True"/>
						<px:PXTextEdit ID="edLinkTemplate" runat="server" DataField="LinkTemplate" />
						<px:PXDropDown ID="edPriority" runat="server" DataField="Priority" CommitChanges="true" />

						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M"/>
						<px:PXDropDown ID="edColor" runat="server" DataField="Color" CommitChanges="True"/>
                        <px:PXCheckBox ID="edSkipError" runat="server" DataField="SkipError" CommitChanges="true" AlignLeft="True"/>
					</px:PXPanel>

					<px:PXPanel ID="PXPanel2" runat="server" RenderStyle="Fieldset" Caption="Contacts">
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
						<px:PXCheckBox ID="edContactsSync" runat="server" DataField="ContactsSync" CommitChanges="true" Size="XXL" />
						<px:PXCheckBox ID="edContactsSeparated" runat="server" DataField="ContactsSeparated"  CommitChanges="true" />
						<px:PXCheckBox ID="edContactsMerge" runat="server" DataField="ContactsMerge" CommitChanges="true" />
						<px:PXCheckBox ID="edContactsSkipCategory" runat="server" DataField="ContactsSkipCategory" CommitChanges="true" />
						<px:PXCheckBox ID="edContactsGenerateLink" runat="server" DataField="ContactsGenerateLink" CommitChanges="true" />

						<px:PXLayoutRule ID="PXLayoutRule134" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>
						<px:PXDropDown ID="edContactsDirection" runat="server" DataField="ContactsDirection" CommitChanges="true" />
						<px:PXTextEdit ID="edContactsFolder" runat="server" DataField="ContactsFolder" />
						<px:PXDropDown ID="edContactsFilter" runat="server" DataField="ContactsFilter" CommitChanges="true" />
						<px:PXSelector ID="edContactsClass" runat="server" DataField="ContactsClass" AllowEdit="True" CommitChanges="true" />
					</px:PXPanel>

					<px:PXPanel ID="PXPanel3" runat="server" RenderStyle="Fieldset" Caption="Email">
						<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="S"/>
						<px:PXCheckBox ID="edEmailsSync" runat="server" DataField="EmailsSync"  CommitChanges="true" Size="XXL" />
						<px:PXCheckBox ID="edEmailsAttachments" runat="server" DataField="EmailsAttachments" />

						<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M"/>
						<px:PXTextEdit ID="edEmailsFolder" runat="server" DataField="EmailsFolder" />
					</px:PXPanel>

					<px:PXPanel ID="PXPanel4" runat="server" RenderStyle="Fieldset" Caption="Tasks">
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" />
						<px:PXCheckBox ID="edTasksSync" runat="server" DataField="TasksSync"  CommitChanges="true" Size="XXL" />
						<px:PXCheckBox ID="edTasksSeparated" runat="server" DataField="TasksSeparated"  CommitChanges="true" />
						<px:PXCheckBox ID="edTasksSkipCategory" runat="server" DataField="TasksSkipCategory" CommitChanges="true" />

						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M"/>
						<px:PXDropDown ID="edTasksDirection" runat="server" DataField="TasksDirection" />
						<px:PXTextEdit ID="edTasksFolder" runat="server" DataField="TasksFolder" />
					</px:PXPanel>

					<px:PXPanel ID="PXPanel5" runat="server" RenderStyle="Fieldset" Caption="Events">
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S" />
						<px:PXCheckBox ID="edEventsSync" runat="server" DataField="EventsSync"  CommitChanges="true" Size="XXL" />
						<px:PXCheckBox ID="edEventsSeparated" runat="server" DataField="EventsSeparated"  CommitChanges="true" />
						<px:PXCheckBox ID="edEventsSkipCategory" runat="server" DataField="EventsSkipCategory" CommitChanges="true" />

						<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>
						<px:PXDropDown ID="edEventsDirection" runat="server" DataField="EventsDirection" />	
						<px:PXTextEdit ID="edEventsFolder" runat="server" DataField="EventsFolder" />
					</px:PXPanel>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Users" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="gridEMailSyncAccount" runat="server" DataSourceID="ds" Style="position: static" Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab"
						NoteField="" FilesField="" NoteIndicator="False" FilesIndicator="False" ActivityIndicator="False">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="Preferences">
								<RowTemplate>
									<px:PXSelector ID="edEmployeeID" DataField="EmployeeID" runat="server" AllowEdit="true" AutoRefresh="true" >
										<Parameters>
											<px:PXControlParam ControlID="gridEMailSyncAccount" Name="EMailSyncAccountPreferences.employeeID" PropertyName="DataValues[&quot;EmployeeID&quot;]" Type="String" />
										</Parameters>
									</px:PXSelector>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="EmployeeID" AllowUpdate="True" Width="100px" CommitChanges="true"/>
									<px:PXGridColumn DataField="EmployeeCD" AllowUpdate="False" Width="150px" />																	
									<px:PXGridColumn DataField="Address" AllowUpdate="False" Width="300px" />	
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>	
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
