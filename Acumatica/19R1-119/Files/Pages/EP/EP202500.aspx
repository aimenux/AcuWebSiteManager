<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP202500.aspx.cs" 
Inherits="Page_EP202500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="LoginType" TypeName="PX.Objects.EP.EPLoginTypeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="UpdateUsers"  CommitChanges="True" Visible="false"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="LoginType" Style="z-index: 100" 
		Width="100%" DefaultControlID="edLoginTypeName">
           <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edLoginTypeName" runat="server" DataField="LoginTypeName" DataSourceID="ds" AutoRefresh="True"/>
            <px:PXDropDown ID="edEntity" runat="server" DataField="Entity" CommitChanges="true" />
            <px:PXTextEdit ID="edLoginTypeDescription" runat="server" DataField="Description"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="518px" DataSourceID="ds" DataMember="CurrentLoginType" BorderStyle="None"
        AccessKey="T">
        <Items>
            <px:PXTabItem Text="Allowed Roles">
                <Template>
                    <px:PXGrid ID="allowsRolesGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" SyncPosition="True">
		                <Levels>
			                <px:PXGridLevel  DataMember="AllowedRoles">
                                <RowTemplate>
                                    <px:PXCheckBox ID="edIsDefault" runat="server" DataField="IsDefault" CommitChanges="true"/>
                                    <px:PXSelector ID="edRolename" runat="server" DataField="Rolename" AllowEdit="True" AutoRefresh="True"/>
                                    <px:PXTextEdit ID="edRoleDescription" runat="server" DataField="Roles__Descr" Enabled="False" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsDefault" Label="Default" Width="60px" Type="CheckBox" TextAlign="Center"/>
                                    <px:PXGridColumn DataField="Rolename" Label="Rolename" Width="150px" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="Roles__Descr" Label="Description" Width="150px" />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" />
		                <ActionBar ActionsText="False">
							<CustomItems>
								<px:PXToolBarButton>
									<AutoCallBack Command="UpdateUsers" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
		                </ActionBar>
	                </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Users">
                <Template>
                    <px:PXGrid ID="usersGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" AdjustPageSize="Auto" SyncPosition="True">
		                <Levels>
			                <px:PXGridLevel  DataMember="Users">
                                <RowTemplate>
                                    <px:PXSelector ID="edUsername" runat="server" DataField="Username" AllowEdit="True" AutoRefresh="True"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Username" Label="Username" Width="60px" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="State" Label="State" Width="150px"/>
                                    <px:PXGridColumn DataField="DisplayName" Label="Display Name" Width="150px" />
                                    <px:PXGridColumn DataField="Comment" Label="Comment" Width="150px" />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" />
		                <ActionBar ActionsText="False">
		                </ActionBar>
						<Mode AllowUpdate="false" AllowAddNew="false" />
	                </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Managed User Types" BindingContext="form" VisibleExp="DataControls[&quot;edEntity&quot;].Value == C;">
                <Template>
                    <px:PXGrid ID="managedLoginTypesGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" SyncPosition="True" >
		                <Levels>
			                <px:PXGridLevel  DataMember="ManagedLoginTypes">
                                <RowTemplate>
                                    <px:PXSelector ID="edManagedLoginTypeID" runat="server" DataField="LoginTypeID" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edManagedLoginTypeDescr" runat="server" DataField="EPLoginType__Description" Enabled="False" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LoginTypeID" Width="150px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="EPLoginType__Description" Width="400px" />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" />
		                <ActionBar ActionsText="False">
		                </ActionBar>
	                </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Login Creation Rules" >
                    <Template>
                        <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="M" />
                        <px:PXCheckBox ID="chkEmailAsLogin" runat="server" Checked="False" DataField="EmailAsLogin" CommitChanges="true"/>
                        <px:PXCheckBox ID="chkResetPasswd" runat="server" Checked="False" DataField="ResetPasswordOnLogin" />
                        <px:PXCheckBox ID="chkRequireActivation" runat="server" Checked="False" DataField="RequireLoginActivation" />
                    </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" MinHeight="538" Container="Window" />
    </px:PXTab>
</asp:Content>
