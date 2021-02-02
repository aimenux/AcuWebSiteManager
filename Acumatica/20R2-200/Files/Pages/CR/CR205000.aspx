<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR205000.aspx.cs" Inherits="Page_CR205000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Class" TypeName="PX.Objects.CR.CRContactClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Contact Class Summary" DataMember="Class" FilesIndicator="True" NoteIndicator="True"
		ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edCRContactClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="sm" ControlSize="XM" />
			<px:PXLayoutRule runat="server" Merge="True"/>
			<px:PXSelector ID="edCRContactClassID" runat="server" DataField="ClassID" Size="SM" FilterByAllFields="True" />
            <px:PXCheckBox ID="chkInternal" runat="server" DataField="IsInternal"/>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Size="XL" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="198px" DataSourceID="ds" DataMember="ClassCurrent"
		LoadOnDemand="True">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="sm" ControlSize="XM"/>
					<px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True"/>
						<px:PXDropDown ID="edDefaultOwner" runat="server" DataField="DefaultOwner" CommitChanges="True"/>
						<px:PXSelector ID="edDefaultAssignmentMapID" runat="server" DataField="DefaultAssignmentMapID" AllowEdit="True" DisplayMode="Text"/>
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Conversion Settings" StartGroup="True"/>
						<px:PXSelector ID="edTargetLeadClassID" runat="server" DataField="TargetLeadClassID" AllowEdit="True"/>
						<px:PXSelector ID="edTargetBAccountClassID" runat="server" DataField="TargetBAccountClassID" AllowEdit="True"/>
						<px:PXSelector ID="edTargetOpportunityClassID" runat="server" DataField="TargetOpportunityClassID" CommitChanges="True" AllowEdit="True"/>
						<px:PXDropDown ID="edTargetOpportunityStage" runat="server" DataField="TargetOpportunityStage"/>
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Email Settings" StartGroup="True"/>
						<px:PXSelector ID="edDefaultEMailAccount" runat="server" DataField="DefaultEMailAccountID" DisplayMode="Text" SuppressLabel="False"/>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" ActionsPosition="Top" SkinID="Details" Caption="Contact Class Details" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AttributeID" Width="81px" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" Width="351px" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="81px" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="CSAttribute__ContainsPersonalData" TextAlign="Center" Type="CheckBox" Width="140px"/>
									<px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox"/>
									<px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" Width="81px" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" Width="100px" RenderEditorText="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
							</CustomItems>
						</ActionBar>
						<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
