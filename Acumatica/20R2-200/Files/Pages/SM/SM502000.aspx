<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM502000.aspx.cs" Inherits="Page_SM502000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.PushNotifications.UI.FailedToSendPushNotificationsProcess" PrimaryView="FailedToSend">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="SendPushNotification" CommitChanges="True" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="showPushNotification" CommitChanges="True"  Visible="True" RepaintControls="All" DependOnGrid="grid"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXSmartPanel ID="pnlViewNotification" runat="server" Height="600px" Width="800px" Caption="Notification Event"
		CaptionVisible="true" Key="PopupDefinition" AutoCallBack-Enabled="true"
		AutoCallBack-Command="Refresh" AutoCallBack-Target="frmViewNotification" AllowResize="false">
		<px:PXFormView ID="frmViewNotification" runat="server" DataSourceID="ds" Width="100%"
			CaptionVisible="False" DataMember="PopupDefinition">
			<ContentStyle BackColor="Transparent" BorderStyle="None">
			</ContentStyle>
			<Template>
				<px:PXTextEdit ID="edDetailsNotification" runat="server" DataField="NotificationBody" Height="550px"
					LabelID="" Style="z-index: 101; border-style: none;" TextMode="MultiLine"
					Width="100%" SelectOnFocus="false">
					
				</px:PXTextEdit>
			</Template>
			
		</px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons" >
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Close" Width="63px" Height="20px">
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" AutoAdjustColumns="true" 
        SyncPosition="true" KeepPosition="True" PreserveSortsAndFilters="True" PreservePageIndex="True">
		<Levels>
			<px:PXGridLevel DataMember="FailedToSend">
                <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="True" AllowRowSelect="True" />
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="True" AllowResize="false"/>
                    <px:PXGridColumn DataField="HookId" Width="100"/>
                    <px:PXGridColumn DataField="DateTimeStamp" Width="150"/>
                    <px:PXGridColumn DataField="Source" Width="375"/>
                    <px:PXGridColumn DataField="ErrorMessage" Width="300"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowAddNew="False" />
        <ActionBar PagerVisible="False" CustomItemsGroup="1">
            <Actions>
                <AddNew Enabled="False" />
                <Delete Enabled="False"/>
            </Actions>
        </ActionBar>
	</px:PXGrid>
</asp:Content>
