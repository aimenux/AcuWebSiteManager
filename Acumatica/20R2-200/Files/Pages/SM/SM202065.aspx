<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM202065.aspx.cs" Inherits="Page_SM202065" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.TokenLogin.SupportAccessMaint" PrimaryView="UserSelect" SuspendUnloading="False">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="RevokeAccess"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="UserSelect">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" />
			
			<px:PXDropDown ID="edState" runat="server" DataField="State" Enabled="False" />

			<px:PXDropDown ID="edAccessStatus" runat="server" DataField="FilterSelect.AccessStatus" CommitChanges="True" Enabled="False"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%">
		<Items>
			<px:PXTabItem Text="Roles Granted">
				<Template>
					<px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" SkinID="Inquire" Height="300px">
						<Levels>
							<px:PXGridLevel DataMember="AllowedRoles">
								<Columns>
									<px:PXGridColumn AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="80px" AutoCallBack="True"/>
									<px:PXGridColumn DataField="Rolename" Width="200px" AllowUpdate ="False"/>
									<px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True"/>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="History" LoadOnDemand="true" BindingContext="form">
				<Template>
					<px:PXGrid ID="gridHistory" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" SkinID="Inquire" Height="300px">
						<Levels>
							<px:PXGridLevel DataMember="GrantHistory">
								<Columns>
									<px:PXGridColumn DataField="Username" Width="180px"/>
									<px:PXGridColumn DataField="Type" Width="100px"/>
									<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="140px"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True"/>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="User Trace" LoadOnDemand="true" BindingContext="form">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
									 left: 0px; top: 0px;" Width="100%" ActionsPosition="Top" AllowPaging="True" Caption="Activity"
							   AllowSearch="True" AdjustPageSize="Auto" SkinID="Inquire">
						<Levels>
							<px:PXGridLevel DataMember="LoginTraces">
								<Columns>
									<px:PXGridColumn DataField="Date" Width="120px" DisplayFormat="g" />
									<px:PXGridColumn DataField="Username" Width="200px" />
									<px:PXGridColumn DataField="Operation" RenderEditorText="True" Width="100px" />
									<px:PXGridColumn DataField="Host" Width="200px"/>
									<px:PXGridColumn DataField="IPAddress" Width="150px" />
									<px:PXGridColumn DataField="ScreenID" Width="110px" />
									<px:PXGridColumn DataField="ScreenID_SiteMap_Title" Width="110px" />
									<px:PXGridColumn DataField="Comment" Width="200px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
									<px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" />
									<px:PXTextEdit ID="edUsername" runat="server" DataField="Username" />
									<px:PXDropDown ID="edOperation" runat="server" DataField="Operation" />
									<px:PXTextEdit ID="edHost" runat="server" DataField="Host"/>
									<px:PXTextEdit ID="edIPAddress" runat="server" DataField="IPAddress" />
								</RowTemplate>
								<Layout FormViewHeight=""></Layout>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
						<ActionBar DefaultAction="cmdAcctDetails" PagerVisible="False">
							<PagerSettings Mode="NextPrevFirstLast" />
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" />
	</px:PXTab>
</asp:Content>