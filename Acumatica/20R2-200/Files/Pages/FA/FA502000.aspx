<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA502000.aspx.cs"
	Inherits="Page_FA502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.CalcDeprProcess" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewBook" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewClass" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" DefaultControlID="edPeriodID"
		Caption="Parameters" NoteField="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" MarkRequired="Dynamic" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="BookID" ID="edBookID" />
			<px:PXSelector CommitChanges="True" runat="server" InputMask="##-####" DataField="PeriodID" ID="edPeriodID" Size="S" AutoRefresh="true"/>
			<px:PXDropDown CommitChanges="True" runat="server" DataField="Action" ID="edAction" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID" />
			<px:PXSelector CommitChanges="True" ID="edParentAssetID" runat="server" DataField="ParentAssetID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" Height="150px"
		SkinID="PrimaryInquire" Caption="Assets to Process" AdjustPageSize="Auto" AllowPaging="True" FastFilterFields="AssetID, FixedAsset__Description" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Balances">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn DataField="FixedAsset__BranchID" />
					<px:PXGridColumn AllowUpdate="False" DataField="AssetID" LinkCommand="ViewAsset" />
					<px:PXGridColumn DataField="FixedAsset__Description" />
					<px:PXGridColumn AllowUpdate="False" DataField="ClassID" LinkCommand="ViewClass" />
					<px:PXGridColumn DataField="FixedAsset__ParentAssetID" />
					<px:PXGridColumn AllowUpdate="False" DataField="BookID" LinkCommand="ViewBook" />
					<px:PXGridColumn AllowUpdate="False" DataField="CurrDeprPeriod" DisplayFormat="##-####" />
					<px:PXGridColumn AllowNull="False" DataField="YtdDeprBase" TextAlign="Right" />
					<px:PXGridColumn DataField="FADetails__ReceiptDate" />
					<px:PXGridColumn DataField="FixedAsset__UsefulLife" TextAlign="Right" />
					<px:PXGridColumn DataField="FixedAsset__FAAccountID" />
					<px:PXGridColumn DataField="FixedAsset__FASubID" />
					<px:PXGridColumn DataField="FADetails__TagNbr" />
					<px:PXGridColumn DataField="Account__AccountClassID" Label="Account Class" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar PagerVisible="False" />
	</px:PXGrid>
</asp:Content>
