<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS207600.aspx.cs" Inherits="Page_CS207600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Records" TypeName="PX.Objects.CS.CSBoxMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" TabIndex="800">
		<Levels>
			<px:PXGridLevel DataMember="Records">
			    <RowTemplate>
					<px:PXNumberEdit ID="edBoxWeight" runat="server" DataField="BoxWeight">
                    </px:PXNumberEdit>
					<px:PXNumberEdit ID="edMaxWeight" runat="server" DataField="MaxWeight" />
					<px:PXNumberEdit ID="edLength" runat="server" DataField="Length" />
                    <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="Width" />
                    <px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="Height" />
			        <px:PXNumberEdit ID="edMaxVolume" runat="server" DataField="MaxVolume">
                    </px:PXNumberEdit>
			    </RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="BoxID" />
					<px:PXGridColumn DataField="Description" Width="200px" />
					<px:PXGridColumn DataField="BoxWeight" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="MaxWeight" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CommonSetup__WeightUOM" />
					<px:PXGridColumn DataField="MaxVolume" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CommonSetup__VolumeUOM" />
					<px:PXGridColumn DataField="Length" TextAlign="Right" />
				    <px:PXGridColumn DataField="Width" TextAlign="Right" />
                    <px:PXGridColumn DataField="Height" TextAlign="Right" />
					<px:PXGridColumn DataField="CommonSetup__LinearUOM" />
				    <px:PXGridColumn DataField="CarrierBox" Width="90px" />
                    <px:PXGridColumn DataField="ActiveByDefault" TextAlign="Center" Type="CheckBox" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
