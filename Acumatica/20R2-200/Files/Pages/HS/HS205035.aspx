<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="HS205035.aspx.cs"
	Inherits="Page_HS205035" Title="Real-Time Sync" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.DataSync.HubSpot.HSReSyncMaint">
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">

	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" TabIndex="900">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="SM">
			</px:PXLayoutRule>
			<px:PXDropDown ID="edSyncMode" runat="server" DataField="SyncMode" Size="M" CommitChanges="True">
			</px:PXDropDown>
		</Template>
	</px:PXFormView>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">

	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Height="150px" CaptionVisible="False"
		AllowPaging="True" AdjustPageSize="Auto" NoteIndicator="False" FilesIndicator="False"
		SkinID="Inquire" TabIndex="500" SyncPosition="True" MatrixMode="True">
		<Levels>
			<px:PXGridLevel DataMember="RStoLSSyncProc">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="70px" AllowCheckAll="true" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="EntityType" DisplayMode="Text" TextAlign="Left" Width="120px" />
					<px:PXGridColumn DataField="MasterSource" Width="110px" />
					<px:PXGridColumn DataField="LastMissedSyncDateTime" Width="250px" DisplayFormat="g" />
					<px:PXGridColumn DataField="LastFullSyncDateTime" Width="250px" DisplayFormat="g" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar PagerVisible="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" />
	</px:PXGrid>
</asp:Content>
