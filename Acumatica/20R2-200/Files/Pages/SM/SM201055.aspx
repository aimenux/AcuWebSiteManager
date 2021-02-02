<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	CodeFile="SM201055.aspx.cs" Inherits="Page_SM201055" Title="Rights by User"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="RightsUser" TypeName="PX.SiteMap.Graph.ModernUserRightsAccess"
		Visible="False">
		<DataTrees>
			<px:PXTreeDataMember TreeView="Entities" TreeKeys="NodeID,CacheName,MemberName" />
		</DataTrees>
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewRoles"/>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="RightsUser" Caption="User Name" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edUsername" runat="server" DataField="Username" DataSourceID="ds" CommitChanges="true" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
     <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
          <AutoSize Enabled="true" Container="Window" />
          <Template1>
				<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" PopulateOnDemand="True" ShowRootNode="false" ExpandDepth="1" AllowCollapse="False">
					<AutoCallBack Target="griddet" Command="Refresh" />
					<DataBindings>
						<px:PXTreeItemBinding DataMember="Entities" TextField="Text" ValueField="Path" ImageUrlField="Icon" ToolTipField="Description"/>
					</DataBindings>
                    <AutoSize Enabled="true" />
				</px:PXTreeView>
         </Template1>

          <Template2>

				<px:PXGrid ID="griddet" runat="server" Height="200px" Width="100%" Style="z-index: 100;
					position: relative;" DataSourceID="ds" AllowSearch="True" ActionsPosition="None"
					MatrixMode="true" SkinID="Details" SyncPosition="true" FastFilterFields="RoleDescr">
					<CallbackCommands>
						<Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
						<Save PostData="Page" />
					</CallbackCommands>
                    <ActionBar DefaultAction="ViewRoles">
                        <CustomItems>
                            <px:PXToolBarButton Text="View Roles" CommandSourceID="ds" CommandName="ViewRoles">
                                <ActionBar GroupIndex="2" Order="3"/> 
                            </px:PXToolBarButton>
                        </CustomItems>
                    </ActionBar>
					<Levels>
						<px:PXGridLevel DataMember="UserRoleEntities">
							<RowTemplate>
								<px:PXTextEdit SuppressLabel="True" Size="s" ID="edEntity" runat="server" DataField="RoleDescr"
									Enabled="False" />
								<px:PXDropDown SuppressLabel="True" Size="s" ID="edRoleRight" runat="server" DataField="RoleRight" />
							</RowTemplate>
							<Columns>
								<px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Visible="False" AllowShowHide="False" />
								<px:PXGridColumn AllowUpdate="False" DataField="CacheName" Visible="False" AllowShowHide="False" />
								<px:PXGridColumn AllowUpdate="False" DataField="MemberName" Visible="False" AllowShowHide="False" />
								<px:PXGridColumn AllowUpdate="False" DataField="RoleName" Visible="False" AllowShowHide="False" />
                                <px:PXGridColumn AllowUpdate="False" DataField="DesctiptionIcon" Width="25px" AllowResize="False" />
								<px:PXGridColumn AllowUpdate="False" DataField="RoleDescr" Width="400px" />
								<px:PXGridColumn AllowNull="False" DataField="RoleRight" TextAlign="Left" Width="120px" AllowResize="False">
									<ValueItems>
										<Items>
											<px:PXValueItem DisplayValue="Undefined" Value="-1" />
											<px:PXValueItem DisplayValue="Denied" Value="0" />
											<px:PXValueItem DisplayValue="Select" Value="1" />
											<px:PXValueItem DisplayValue="Update" Value="2" />
											<px:PXValueItem DisplayValue="Insert" Value="3" />
											<px:PXValueItem DisplayValue="Delete" Value="4" />
										</Items>
									</ValueItems>
								</px:PXGridColumn>
							</Columns>
							<Mode AllowAddNew="False" AllowDelete="False" />
						</px:PXGridLevel>
					</Levels>
					<Parameters>
						<px:PXControlParam ControlID="tree" Name="path" PropertyName="SelectedValue" Type="String" />
					</Parameters>
					<AutoSize Enabled="True" />
                    <Mode AllowAddNew="false" AllowDelete="false" />
				</px:PXGrid>
          </Template2>
    </px:PXSplitContainer>
    <px:PXSmartPanel ID="PanelViewRoles" runat="server" Key="ClarifiedRoles" LoadOnDemand="true" Width="600px" Height="500px"
        Caption="View Roles" CaptionVisible="true" AutoRepaint="true" DesignView="Content">
        <px:PXGrid ID="gripSiteStatus" runat="server" DataSourceID="ds" Style="height: 189px;"
            AutoAdjustColumns="true" Width="100%" SkinID="Inquire" AdjustPageSize="Auto" AllowSearch="True" BatchUpdate="true" MatrixMode="true">
            <CallbackCommands>
                <Refresh CommitChanges="true"></Refresh>
            </CallbackCommands>
            <Levels>
                <px:PXGridLevel DataMember="ClarifiedRoles">
                    <Mode AllowAddNew="false" AllowDelete="false" />
                    <Columns>
                        <px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Visible="False" AllowShowHide="False" />
                                <px:PXGridColumn AllowUpdate="False" DataField="DesctiptionIcon" Width="25px" AllowResize="False" Visible="False" AllowShowHide="False" />
								<px:PXGridColumn AllowUpdate="False" DataField="RoleDescr" Width="400px" Visible="False" AllowShowHide="False"/>
								<px:PXGridColumn AllowUpdate="False" DataField="RoleName" AllowShowHide="False" />
								<px:PXGridColumn AllowNull="False" DataField="InitialRoleRight" >
									<ValueItems>
										<Items>
											<px:PXValueItem DisplayValue="Undefined" Value="-1" />
											<px:PXValueItem DisplayValue="Denied" Value="0" />
											<px:PXValueItem DisplayValue="Select" Value="1" />
											<px:PXValueItem DisplayValue="Update" Value="2" />
											<px:PXValueItem DisplayValue="Insert" Value="3" />
											<px:PXValueItem DisplayValue="Delete" Value="4" />
										</Items>
									</ValueItems>
								</px:PXGridColumn>
                                <px:PXGridColumn AllowNull="False" DataField="RoleRight" >
									<ValueItems>
										<Items>
											<px:PXValueItem DisplayValue="Undefined" Value="-1" />
											<px:PXValueItem DisplayValue="Denied" Value="0" />
											<px:PXValueItem DisplayValue="Select" Value="1" />
											<px:PXValueItem DisplayValue="Update" Value="2" />
											<px:PXValueItem DisplayValue="Insert" Value="3" />
											<px:PXValueItem DisplayValue="Delete" Value="4" />
										</Items>
									</ValueItems>
								</px:PXGridColumn>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
            <Parameters>
				<px:PXControlParam ControlID="tree" Name="path" PropertyName="SelectedValue" Type="String" />
			</Parameters>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
