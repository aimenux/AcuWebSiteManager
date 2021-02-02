<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="IN208600.aspx.cs" Inherits="Page_IN208600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" 
		TypeName="PX.Objects.IN.INMovementClassMaint" PrimaryView="MovementClasses" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phL" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px"
		Style="z-index: 100; left: 0px; top: 0px;" Width="100%" ActionsPosition="Top" 
		AllowSearch="True" SkinID="Primary" TabIndex="500">
		<Mode InitNewRow="True" />
		<Levels>
			<px:PXGridLevel DataMember="MovementClasses" >
				<RowTemplate>
                    <px:PXNumberEdit ID="edMaxTurnoverPct" runat="server" 
                        DataField="MaxTurnoverPct">
                    </px:PXNumberEdit>
                </RowTemplate>
				<Columns>
                    <px:PXGridColumn DataField="MovementClassID" Required="True" TextCase="Upper" Width="100px" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                    <px:PXGridColumn DataField="CountsPerYear" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn DataField="MaxTurnoverPct" TextAlign="Right" Width="100px" 
                        AutoCallBack="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
