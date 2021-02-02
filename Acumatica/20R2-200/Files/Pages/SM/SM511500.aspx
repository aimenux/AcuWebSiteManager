<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM511500.aspx.cs" Inherits="Page_SM511500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="TranslationSets" TypeName="PX.SM.TranslationSetProcessing"
		Visible="True">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="viewTranslationSet" Visible="False" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" SkinID="Inquire" Caption="Translation Sets" SyncPosition="True" DataSourceID="ds"
        BatchUpdate="true" AllowPaging="True">
		<Levels>
			<px:PXGridLevel DataMember="TranslationSets">
				<Columns>
				    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="40px" AllowCheckAll="True" />
					<px:PXGridColumn DataField="Description" Width="250px" />
                    <px:PXGridColumn DataField="IsCollected" TextAlign="Center" Type="CheckBox" Width="100px" />
                    <px:PXGridColumn DataField="SystemVersion" Width="100px" />
					<px:PXGridColumn DataField="SystemTime" DisplayFormat="g" Width="150px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowAddNew="False" />
        <ActionBar PagerVisible="False">
            <CustomItems>
                <px:PXToolBarButton>
                    <AutoCallBack Command="viewTranslationSet" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
	</px:PXGrid>
</asp:Content>
