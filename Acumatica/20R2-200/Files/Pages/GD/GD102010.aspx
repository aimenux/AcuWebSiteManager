<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GD102010.aspx.cs" Inherits="Page_GD102010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.GDPR.GDPRPseudonymizeProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" />
			<px:PXDSCallbackCommand Name="OpenContact" CommitChanges="True" Visible="False"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	
	

	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Operation" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"/>
			<px:PXTextEdit ID="edSearch" runat="server" DataField="Search" CommitChanges="True"/>
			<px:PXDropDown ID="edMasterEntity" runat="server" AllowNull="False" DataField="MasterEntity" CommitChanges="True"/>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"/>
			<px:PXCheckBox ID="edNoConsent" runat="server" AlignLeft="true" DataField="NoConsent" CommitChanges="True"/>
			<px:PXCheckBox ID="edConsentExpired" runat="server" AlignLeft="true" DataField="ConsentExpired" CommitChanges="True"/>
		</Template>
	</px:PXFormView>
	
	

	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds"
		SkinID="Inquire" SyncPosition="True" AutoAdjustColumns="True" ActionsPosition="Top">
		<Levels>
			<px:PXGridLevel DataMember="SelectedItems">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="True"/>
					<px:PXGridColumn DataField="Deleted" TextAlign="Center" Type="CheckBox" Width="70px"/>
					<px:PXGridColumn DataField="ContactID" Width="80px" LinkCommand="OpenContact"/>
					<px:PXGridColumn DataField="ContactType" Width="150px"/>
					<px:PXGridColumn DataField="AcctCD" Width="200px"/>

					<px:PXGridColumn DataField="DisplayName" Width="150px"/>
					<px:PXGridColumn DataField="MidName" Width="150px"/>
					<px:PXGridColumn DataField="LastName" Width="150px"/>
					<px:PXGridColumn DataField="Salutation" Width="150px" SyncVisible="false" Visible="false"/>
					<px:PXGridColumn DataField="FullName" Width="150px"/>
					<px:PXGridColumn DataField="EMail" Width="150px"/>
					<px:PXGridColumn DataField="WebSite" Width="150px" SyncVisible="false" Visible="false"/>
					<px:PXGridColumn DataField="Fax" Width="150px" SyncVisible="false" Visible="false"/>
					<px:PXGridColumn DataField="Phone1" Width="150px"/>
					<px:PXGridColumn DataField="Phone2" Width="150px" SyncVisible="false" Visible="false"/>
					<px:PXGridColumn DataField="Phone3" Width="200px" SyncVisible="false" Visible="false"/>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
	
	

</asp:Content>
