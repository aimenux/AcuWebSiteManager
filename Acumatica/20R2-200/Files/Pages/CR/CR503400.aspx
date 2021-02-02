<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503400.aspx.cs" Inherits="Page_CR503400"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="PX.Objects.CR.CRGrammProcess" PrimaryView="Items" PageLoadBehavior="PopulateSavedValues">
	    <CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grdItems" Name="Items_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grdItems" Name="Items_BAccount_ViewDetails" />						
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Grams" AllowPaging="True" AdjustPageSize="auto"
		SkinID="PrimaryInquire">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>				    
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="BAccountID" LinkCommand="Items_BAccount_ViewDetails"/>
                    <px:PXGridColumn DataField="FullName" />				    
                    <px:PXGridColumn DataField="ContactType" />
                    <px:PXGridColumn DataField="DisplayName" LinkCommand="Items_ViewDetails"/>					
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
