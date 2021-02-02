<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="FA207000.aspx.cs" Inherits="Page_FA207000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="AssetTypes"
		TypeName="PX.Objects.FA.AssetTypeMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" SyncPosition="true">
		<Levels>
			<px:PXGridLevel  DataMember="AssetTypes">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edAssetTypeID" runat="server" DataField="AssetTypeID" CommitChanges="True" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox ID="chkTangible" runat="server" DataField="Tangible" CommitChanges="True" />
                    <px:PXCheckBox ID="chkDepreciable" runat="server" DataField="Depreciable" CommitChanges="True" />
                </RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AssetTypeID" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="IsTangible" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn DataField="Depreciable" Type="CheckBox" TextAlign="Center" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
