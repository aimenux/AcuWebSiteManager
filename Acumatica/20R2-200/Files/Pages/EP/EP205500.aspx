<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP205500.aspx.cs"
	Inherits="Page_EP205500" Title="Untitled Page" %>
<%@ Register TagPrefix="px" Namespace="PX.Web.UI" Assembly="PX.Web.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=3b136cac2f602b8e" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		PrimaryView="Maps" TypeName="PX.Objects.EP.EPAssignmentAndApprovalMapEnq">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridMaps" Name="ViewDetails"/>
			<px:PXDSCallbackCommand DependOnGrid="gridMaps" Name="AddApprovalNew"/>
			<px:PXDSCallbackCommand DependOnGrid="gridMaps" Name="AddAssignmentNew"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	 <px:PXGrid ID="gridMaps" runat="server" DataSourceID="ds" ActionsPosition="Top"
        AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SyncPosition="True" NoteIndicator="true" FilesIndicator="true"
        SkinID="PrimaryInquire" Width="100%" MatrixMode="True" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="Maps">
                <Columns>
                    <px:PXGridColumn DataField="Name" LinkCommand="ViewDetails" />
					<px:PXGridColumn DataField="MapType" TextAlign="Left" />
                    <px:PXGridColumn DataField="EntityType" />
                </Columns>
            </px:PXGridLevel>
        </Levels>

        <ActionBar DefaultAction="ViewDetails" PagerVisible="False">
		    <Actions>
		        <AddNew Enabled="False"></AddNew>
		    </Actions>
        </ActionBar>
        <Mode AllowAddNew="False" AllowUpdate="False" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
