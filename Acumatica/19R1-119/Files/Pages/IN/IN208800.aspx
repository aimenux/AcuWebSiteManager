<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="IN208800.aspx.cs" Inherits="Page_IN208800" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INReplenishmentClassMaint" PrimaryView="Classes" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px"
		Style="z-index: 100" Width="100%" ActionsPosition="Top" AllowSearch="True" SkinID="Primary">
		<Mode InitNewRow="True" />
		<Levels>
			<px:PXGridLevel DataMember="Classes" >
				<Columns>
                    <px:PXGridColumn DataField="ReplenishmentClassID" Required="True" TextCase="Upper" Width="100px" />
                    <px:PXGridColumn DataField="Descr" Width="300px" />
				    <px:PXGridColumn AllowNull="False" DataField="ReplenishmentSource" Label="Replenishment Source" RenderEditorText="True" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
