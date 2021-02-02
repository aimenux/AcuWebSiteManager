<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM507010.aspx.cs" Inherits="Page_SM507010" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.EmailSendReceiveMaint"
		PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="viewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Filter" Caption="Selection" DefaultControlID="edAction" 
		AllowCollapse="False">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" 
				ControlSize="M" />
			<px:PXDropDown CommitChanges="True" ID="edAction" runat="server" DataField="Operation" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" Caption="Email Accounts" SkinID="Inquire"
		NoteIndicator="True" FilesIndicator="True" FilesField="NoteFiles">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center"
						Type="CheckBox" Width="20px" />
					<px:PXGridColumn DataField="Description" Width="300px" LinkCommand="viewDetails"/> 
					<px:PXGridColumn DataField="Address" Width="200px"/>
					<px:PXGridColumn DataField="InboxCount" Width="60px" />
					<px:PXGridColumn DataField="LastReceiveDateTime" Width="110px" DisplayFormat="g" />
					<px:PXGridColumn DataField="OutboxCount" Width="60px" />
					<px:PXGridColumn DataField="LastSendDateTime" Width="110px" DisplayFormat="g" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Tooltip="Shows Account Details" Key="cmdViewDetails"
					Visible="false">
				    <AutoCallBack Command="viewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
					<ActionBar GroupIndex="0" Order="0"/>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
