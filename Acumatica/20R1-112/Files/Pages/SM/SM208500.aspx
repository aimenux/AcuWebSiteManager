<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM208500.aspx.cs" Inherits="Page_SM208500" Title="Lists as Entry Points" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        function commandResult(ds, context) {
            if (context.command == "Save" || context.command == "Delete") {
                var ds = px_all[context.id];
                var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
                if (isSitemapAltered) __refreshMainMenu();
            }
        }
	</script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Data.LEPMaint" PrimaryView="Items" Visible="True">
	    <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary"
        OnDataBound="grid_OnDataBound">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXCheckBox runat="server" DataField="IsActive" SuppressLabel="True"/>
                    <px:PXSelector ID="edEntryScreen" runat="server" DataField="EntryScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" TextField="Title" />
                    <px:PXSelector ID="edListScreen" runat="server" DataField="ListScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" TextField="Title" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="IsActive" Width="60" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="EntryScreenID" Width="275" CommitChanges="True" DisplayMode="Hint" />
					<px:PXGridColumn DataField="ListScreenID" Width="275" CommitChanges="True" DisplayMode="Hint"/>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
