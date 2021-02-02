<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM201050.aspx.cs" Inherits="Page_SM101000"
	Title="SM Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.SMAccess"
		PrimaryView="Group">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formGroup" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Group" Caption="Restriction Group" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" DataSourceID="ds">
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" 
				DataField="GroupType" Size="M" />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Style="z-index: 100" Width="100%"
		SelectedIndex="1">
		<Items>
			<px:PXTabItem Text="Users">
				<Template>
					<px:PXGrid ID="gridUsers" BorderWidth="0px" runat="server" DataSourceID="ds" Height="150px"
						Style="z-index: 100" Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top"
						AllowSearch="True" EditPageUrl="~/Pages/SM/SM201010.aspx" SkinID="Details" FastFilterFields="Username,Comment">
						<Levels>
							<px:PXGridLevel DataMember="Users">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="Username" />
                                    <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
									<px:PXTextEdit ID="edComment" runat="server" DataField="Comment" />
									<px:PXCheckBox ID="chkUserIncluded" runat="server" DataField="Included" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px"
										AllowCheckAll="True" RenderEditorText="True" />
									<px:PXGridColumn DataField="Username" Width="300px" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="FullName" Width="200px" AllowUpdate="False" />
									<px:PXGridColumn DataField="Comment" Width="300px" AllowUpdate="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" AllowSort="True" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridUsers" Name="Username" PropertyName="DataValues[&quot;Username&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Email accounts">
				<Template>
					<px:PXGrid ID="gridEMail" BorderWidth="0px" runat="server" DataSourceID="ds" Height="150px"
						Style="z-index: 100" Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top"
						AllowSearch="True" EditPageUrl="~/Pages/SM/SM204002.aspx" SkinID="Details" FastFilterFields="Address,LoginName">
						<Levels>
							<px:PXGridLevel DataMember="Account">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSelector ID="edAddress" runat="server" DataField="Address" ValueField="Address" />
									<px:PXTextEdit ID="edLoginName" runat="server" DataField="LoginName" />
									<px:PXCheckBox ID="chkIncluded" runat="server" DataField="Included" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Address" Width="150px"  AutoCallBack="True"/>
									<px:PXGridColumn DataField="LoginName" Width="180px" />
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="100px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridEMail" Name="Address" PropertyName="DataValues[&quot;Address&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
