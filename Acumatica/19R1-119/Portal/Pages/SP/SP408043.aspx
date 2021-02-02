<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP408043.aspx.cs" Inherits="Pages_SP408043"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="SP.Objects.SP.SPContactPartnerInquiry" 
        PrimaryView="FilteredItems" Visible="True">
		<CallbackCommands>
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="ViewBAccountDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
        Width="100%" ActionsPosition="Top" Caption="Contacts" AllowPaging="true" AdjustPageSize="auto"
		SkinID="PrimaryInquire" FastFilterFields="DisplayName,FullName" FilesIndicator="False" NoteIndicator="False">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn DataField="DisplayName" Width="130px" LinkCommand="ViewDetails" />
                    <px:PXGridColumn DataField="Salutation" Width="180px" />
					<px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox"/>
					<px:PXGridColumn DataField="BAccount__AcctCD" Width="190px" LinkCommand="ViewBAccountDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" Width="190px" />
					<px:PXGridColumn DataField="EMail" Width="190px" />
                    <px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Address__City" Width="100px" />
					<px:PXGridColumn DataField="Address__CountryID" Width="40px" />
                 </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Tooltip="View Details" Key="cmdItemDetails" Visible="false">
					<Images Normal="main@DataEntry" />
					<AutoCallBack Command="ViewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="View BAccount Details" Tooltip="View BAccount Details" Key="cmdItemBAccountDetails" Visible="false">
					<Images Normal="main@DataEntry" />
					<AutoCallBack Command="ViewBAccountDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
