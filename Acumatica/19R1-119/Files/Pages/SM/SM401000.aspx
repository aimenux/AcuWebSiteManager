<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM401000.aspx.cs" Inherits="Page_SM401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SM.DatabaseSchemaInquiry" PageLoadBehavior="GoFirstRecord" PrimaryView="inspectingTables">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewParent" Visible="False" DependOnGrid="PXGridIn"/>
			<px:PXDSCallbackCommand Name="ViewChild" Visible="False" DependOnGrid="PXGridOut"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="inspectingTables" TabIndex="1600">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XXL" LabelsWidth="S" StartColumn="True" />
			<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
			<px:PXSelector runat="server" DataField="FullName" ID="edFullName" CommitChanges="True" FilterByAllFields="True"></px:PXSelector>
			<px:PXTextEdit runat="server" DataField="ClassName" ID="edClassName" ValidateRequestMode="Inherit"></px:PXTextEdit>
			<px:PXTextEdit runat="server" DataField="NamespaceName" ID="edNamespaceName" ValidateRequestMode="Inherit"></px:PXTextEdit>
			<px:PXTextEdit runat="server" DataField="ShortName" ID="edShortName" ValidateRequestMode="Inherit"></px:PXTextEdit>
			<px:PXSelector runat="server" DataField="BaseClassName" AllowEdit="True" Enabled="False" ID="edBaseClassName" ValidateRequestMode="Inherit"></px:PXSelector>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"></px:PXLayoutRule>
			<px:PXCheckBox runat="server" Text="Has Incoming" DataField="HasIncoming" ID="edHasIncoming" ValidateRequestMode="Inherit"></px:PXCheckBox>
			<px:PXCheckBox runat="server" Text="Has Outgoing" DataField="HasOutgoing" ID="edHasOutgoing" ValidateRequestMode="Inherit"></px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Incoming References">
				<Template>
					<px:PXGrid runat="server" SkinID="Details" Width="100%" TabIndex="2700" ID="PXGridOut" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="tableIncomingReferences">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="XL" StartColumn="True"></px:PXLayoutRule>
									<px:PXSelector runat="server" DataField="ChildFullName" ID="edChildFullNameOut" ValidateRequestMode="Inherit"></px:PXSelector>
									<px:PXTextEdit runat="server" DataField="ChildFields" ID="edChildFieldsIn" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXTextEdit runat="server" DataField="Behavior" ID="edBehaviorIn" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="ParentSelect" ID="edParentSelectIn" ValidateRequestMode="Inherit" Height="100px" TextMode="MultiLine"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="ChildSelect" ID="edChildSelectIn" ValidateRequestMode="Inherit" Height="100px" TextMode="MultiLine"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="OriginalSelect" ID="edOrigianlSelectIn" ValidateRequestMode="Inherit" Height="200px" TextMode="MultiLine"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XL"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="ParentFullName" ID="edParentDacIn" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXTextEdit runat="server" DataField="ParentFields" ID="edParentFieldsIn" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXTextEdit runat="server" DataField="AchievedBy" ID="edAchievedByIn" ValidateRequestMode="Inherit"></px:PXTextEdit>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ChildDac" LinkCommand="ViewChild" CommitChanges="True" Width="150px"></px:PXGridColumn>
									<px:PXGridColumn DataField="ChildFields" Width="190px"></px:PXGridColumn>
									<px:PXGridColumn DataField="ParentDac" Width="150px"></px:PXGridColumn>
									<px:PXGridColumn DataField="ParentFields" Width="190px"></px:PXGridColumn>
									<px:PXGridColumn DataField="AchievedBy" Width="150px"></px:PXGridColumn>
									<px:PXGridColumn DataField="Behavior" Width="100"></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>

						<AutoSize Enabled="True"></AutoSize>
						<Mode AllowFormEdit="True" AllowAddNew="False" AllowDelete="False" AllowUpdate="False"></Mode>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Outgoing References">
				<Template>
					<px:PXGrid runat="server" SkinID="Details" Width="100%" TabIndex="2800" ID="PXGridIn" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="tableOutgoingReferences">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="XL" StartColumn="True"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="ChildFullName" ID="edChildDacOut" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXTextEdit runat="server" DataField="ChildFields" ID="edChildFieldsOut" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXTextEdit runat="server" DataField="Behavior" ID="edBehaviorOut" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="ParentSelect" ID="edParentSelectOut" ValidateRequestMode="Inherit" Height="100px" TextAlign="NotSet" TextMode="MultiLine"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="ChildSelect" ID="edChildSelectOut" ValidateRequestMode="Inherit" Height="100px" TextMode="MultiLine"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
									<px:PXTextEdit runat="server" DataField="OriginalSelect" ID="edOrigianlSelectOut" ValidateRequestMode="Inherit" Height="200px" TextMode="MultiLine"></px:PXTextEdit>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XL"></px:PXLayoutRule>
									<px:PXSelector runat="server" DataField="ParentFullname" ID="edChildFullNameIn" ValidateRequestMode="Inherit"></px:PXSelector>
									<px:PXTextEdit runat="server" DataField="ParentFields" ID="edParentFieldsOut" ValidateRequestMode="Inherit"></px:PXTextEdit>
									<px:PXTextEdit runat="server" DataField="AchievedBy" ID="edAchievedByOut" ValidateRequestMode="Inherit"></px:PXTextEdit>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ChildDac" Width="150px"></px:PXGridColumn>
									<px:PXGridColumn DataField="ChildFields" Width="190px"></px:PXGridColumn>
									<px:PXGridColumn DataField="ParentDac" LinkCommand="ViewParent" CommitChanges="True" Width="150px"></px:PXGridColumn>
									<px:PXGridColumn DataField="ParentFields" Width="190px"></px:PXGridColumn>
									<px:PXGridColumn DataField="AchievedBy" Width="150px"></px:PXGridColumn>
									<px:PXGridColumn DataField="Behavior" Width="100"></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>

						<AutoSize Enabled="True"></AutoSize>
						<Mode AllowFormEdit="True" AllowAddNew="False" AllowDelete="False" AllowUpdate="False"></Mode>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window"/>
	</px:PXTab>
</asp:Content>
