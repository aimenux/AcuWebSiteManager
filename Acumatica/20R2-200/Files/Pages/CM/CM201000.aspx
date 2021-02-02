<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM201000.aspx.cs" Inherits="Page_CM201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" AutoCallBack="True" PrimaryView="CuryRateTypeRecords" TypeName="PX.Objects.CM.CurrencyRateTypeMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100; left: 0px; top: 0px;" AllowPaging="True" ActionsPosition="Top" DataSourceID="ds" AdjustPageSize="Auto" AllowSearch="True"
		SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="CuryRateTypeRecords">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" />
					<px:PXMaskEdit ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
					</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="CuryRateTypeID"  />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn AllowNull="False" DataField="RateEffDays" TextAlign="Right" />
					<px:PXGridColumn DataField="RefreshOnline" Type="CheckBox" CommitChanges="True" TextAlign="Center"/>
					<px:PXGridColumn DataField="OnlineRateAdjustment" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" MinHeight="200" Enabled="True" />
		<Mode AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
