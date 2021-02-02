<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP408040.aspx.cs" Inherits="Pages_SP408040"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="SP.Objects.SP.SPContactProductInquiry"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="ViewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" Caption="Selection" RenderStyle="Simple">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M"/>
            <px:PXTextEdit CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" Visible="False"/>
            <px:PXSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" Visible="False"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" 
        Caption="Contacts" AllowPaging="true" AdjustPageSize="auto"
		SkinID="PrimaryInquire" FastFilterFields="DisplayName,FullName" FilesIndicator="False" NoteIndicator="False" 
        RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn DataField="DisplayName" Width="130px" LinkCommand="ViewDetails" />
                    <px:PXGridColumn DataField="Salutation" Width="180px" />
					<px:PXGridColumn DataField="EMail" Width="190px" />
                    <px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
                    <px:PXGridColumn DataField="Users__UserName" Width="130px" />
                    <px:PXGridColumn DataField="EPLoginType__LoginTypeName" Width="150px" />
                    <px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox"/>
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
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>