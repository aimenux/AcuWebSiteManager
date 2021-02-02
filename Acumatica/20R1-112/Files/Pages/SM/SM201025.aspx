<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	CodeFile="SM201025.aspx.cs" Inherits="Page_SM203000" Title="Role Access Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Roles" TypeName="PX.SiteMap.Graph.ModernAccess"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="EntCancel" PostData="Page" />
			<px:PXDSCallbackCommand Name="EntSave" PostData="Page" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="CopyRole" CommitChanges="True" StartNewGroup="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Entities" TreeKeys="NodeID,CacheName,MemberName" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlCopyRole" runat="server" Caption="New Role" CaptionVisible="True"
	Key="NewRole" CreateOnDemand="False" AutoCallBack-Target="formCopyRole" AutoCallBack-Command="Refresh"
	CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="btnOK">
	<px:PXFormView ID="formCopyRole" runat="server" DataSourceID="ds"	Width="100%" CaptionVisible="False" DataMember="NewRole">
		<ContentStyle BackColor="Transparent" BorderStyle="None" />
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="SM" />
			<px:PXTextEdit ID="edNewRoleName" runat="server" DataField="Rolename" />
		</Template>
	</px:PXFormView>
			<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="Copy">
				<AutoCallBack Target="formCopyRole" Command="Save" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Roles" Caption="Role Information" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edRolename" runat="server" DataField="Rolename" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
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
					MatrixMode="true" SkinID="Details" FastFilterFields="RoleDescr">
					<CallbackCommands>
						<Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
						<Save PostData="Page" />
					</CallbackCommands>
					<Levels>
						<px:PXGridLevel DataMember="RoleEntities">
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
								<px:PXGridColumn AllowNull="False" DataField="RoleRight" TextAlign="Left" Width="120px" AllowResize="False" >
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
								<px:PXGridColumn AllowNull="False" DataField="InheritedByChildren" TextAlign="Center" Type="CheckBox" Width="70px"  CommitChanges="true" />
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
</asp:Content>
