<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	CodeFile="SM201005.aspx.cs" Inherits="Page_SM202000" Title="Role Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script language="javascript" type="text/javascript">
		function membershipRowChanged(sender, arg)
		{
			if (sender.activeRow && sender.activeRow.getCell("Inherited").getValue() &&
			(String.compare(arg.command, PXGridCommand.Delete, true) ||
			String.compare(arg.command, PXGridCommand.NoteShow, true)))
			{
				arg.state.setEnabled(false);
			}
		} 
	</script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Roles" TypeName="PX.SM.RoleAccess"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" Visible="false" />
			<px:PXDSCallbackCommand Name="Save" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveRoles" />
			<px:PXDSCallbackCommand Name="InsertRoles" HideText="True"/>
			<px:PXDSCallbackCommand Name="DeleteRoles" HideText="True"/>
			<px:PXDSCallbackCommand Name="FirstRoles" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevRoles" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextRoles" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastRoles" HideText="True"/>
            <px:PXDSCallbackCommand Name="reloadADGroups" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Roles" Caption="Role Information" TemplateContainer="" DefaultControlID="edRolename">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" 
				LabelsWidth="SM" />
			<px:PXSelector ID="edRolename" runat="server" DataField="Rolename" AutoRefresh="True"
				DataSourceID="ds" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXCheckBox CommitChanges="True" ID="chkGuest" runat="server" Checked="True" DataField="Guest" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="255px">
		<Items>
			<px:PXTabItem Key="ms" Text="Membership">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AllowPaging="True" ActionsPosition="Top" SkinID="DetailsInTab" AdjustPageSize="Auto" AllowSearch="True"
                        FastFilterFields="Username,DisplayName,Comment">
						<Levels>
							<px:PXGridLevel DataMember="UsersByRole">
							    <RowTemplate>
							        <px:PXSelector runat="server" ID="edUsername" DataField="Username" AutoRefresh="True"></px:PXSelector>
							    </RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ApplicationName" Width="200px" Visible="False" AllowUpdate="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="Rolename" Width="200px" Visible="False" AllowUpdate="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="Username" Width="200px" AutoCallBack="True"  />
									<px:PXGridColumn DataField="DisplayName" Width="300px" />
                                    <px:PXGridColumn DataField="State" Width="60px"/>
									<px:PXGridColumn AllowUpdate="False" DataField="Domain" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Comment" Width="300px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Inherited" Width="60px" Type="CheckBox" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<FetchRow PostData="Page" />
							<Save PostData="Page" />
							<Refresh PostData="Page" />
						</CallbackCommands>
						<AutoSize Enabled="true" />
						<ActionBar PagerVisible="False">
							<PagerSettings Mode="NextPrevFirstLast" />
						</ActionBar>
						<ClientEvents CommandState="membershipRowChanged" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Key="ad" Text="Active Directory">
				<Template>
					<px:PXGrid ID="grdAD" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" >
						<Levels>
							<px:PXGridLevel DataMember="ActiveDirectoryMap">
							    <RowTemplate>
							        <px:PXSelector runat="server" ID="edGroupID" DataField="GroupID" TextField="Name" AutoRefresh="True" >
							            <GridProperties FastFilterFields="Name,Description"></GridProperties>
							        </px:PXSelector>
							    </RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="GroupID" TextField="GroupName" Width="200px" AutoCallBack="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="GroupDomain" Width="200px" />
									<px:PXGridColumn AllowUpdate="False" DataField="GroupDescription" Width="300px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<FetchRow PostData="Page" />
							<Save PostData="Page" />
							<Refresh PostData="Page" />
						</CallbackCommands>
						<AutoSize Enabled="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Key="ad" Text="Claims">
				<Template>
					<px:PXGrid ID="grdAD" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" >
						<Levels>
							<px:PXGridLevel DataMember="ClaimsMap">
								<Columns>
									<px:PXGridColumn DataField="GroupID" Width="200px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="true" Container="Window" MinHeight="255" />
	</px:PXTab>
</asp:Content>
