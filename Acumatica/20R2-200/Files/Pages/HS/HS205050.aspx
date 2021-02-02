<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="HS205050.aspx.cs"
	Inherits="Page_HS205050" Title="Real-Time Sync State" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.DataSync.HubSpot.HSMarketingListMembersMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="goToHubSpotList" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXSelector ID="edListID" AutoAdjustColumns="true" runat="server"
				CommitChanges="True"
				DisplayMode="Hint"
				DataField="MarketingListID"
				Size="L"
				AllowEdit="true" />
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXTextEdit ID="edHubSpotListName" runat="server" CommitChanges="true" DataField="HubSpotListName" Enabled="false">
				<LinkCommand Command="goToHubSpotList" Target="ds" />
			</px:PXTextEdit>
			<px:PXLayoutRule runat="server" StartRow="True" />
			<px:PXDropDown ID="edAction" runat="server" CommitChanges="True" DataField="Action" Size="L" />
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXCheckBox ID="edSubscribedOnly" runat="server" DataField="SubscribedOnly" CommitChanges="True" AlreadyLocalized="False" />
			<px:PXLayoutRule runat="server" StartRow="True" />
			<px:PXCheckBox ID="edDeleteInTarget" runat="server" DataField="DeleteInTarget" CommitChanges="True" AlreadyLocalized="False" />
		</Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Height="150px" CaptionVisible="False"
		AllowPaging="True" AdjustPageSize="Auto" NoteIndicator="False" FilesIndicator="False"
		SkinID="PrimaryInquire" TabIndex="500" SyncPosition="True" AutoAdjustColumns="True" MatrixMode="True" AllowSearch="true" FastFilterFields="DisplayName,RemoteName,LastErrorMessageSimplified">
		<Levels>
			<px:PXGridLevel DataMember="Records">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="MarketingListMemberID" Width="70px" Visible="false" SyncVisible="False" />
					<px:PXGridColumn DataField="IsSubscribed" Width="50px" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="HSSyncRecord__EntityType" Width="100px" DisplayMode="Text" TextAlign="Left" />
					<px:PXGridColumn DataField="LocalID" Width="200px" AutoCallBack="true" DisplayMode="Text" TextAlign="Left" LinkCommand="Contact_ViewDetails" />
					<px:PXGridColumn DataField="RemoteName" DisplayMode="Text" Width="200px" CommitChanges="True" LinkCommand="GoToHubSpotContact" />
					<px:PXGridColumn DataField="HSSyncRecord__SyncStatus" Width="130" />
					<px:PXGridColumn DataField="MembershipSyncStatus" DisplayMode="Text" Width="130" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar PagerVisible="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="false" />
	</px:PXGrid>
</asp:Content>
