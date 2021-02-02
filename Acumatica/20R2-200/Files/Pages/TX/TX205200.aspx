<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX205200.aspx.cs" Inherits="Page_TX205200" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" TypeName="PX.Objects.TX.TaxBucketMaint" PrimaryView="Bucket">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Bucket" Caption="Group Settings">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" />
			<px:PXSelector CommitChanges="True" ID="edBucketID" runat="server" AutoRefresh="True" DataField="BucketID" />
			<px:PXDropDown ID="edBucketType" runat="server" DataField="BucketType" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="top" Caption="Lines Update Rules" SkinID="Details">
		<ActionBar>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="BucketLine">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector ID="edLineNbr" runat="server" DataField="LineNbr" DisplayMode="Hint" AutoRefresh="true" />
					<px:PXDropDown ID="edLineType" runat="server" DataField="TaxReportLine__LineType" />
					<px:PXDropDown ID="edLineMult" runat="server" DataField="TaxReportLine__LineMult" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="LineNbr" DisplayMode="Hint" CommitChanges="True"/>
					<px:PXGridColumn DataField="TaxReportLine__LineType" RenderEditorText="True" />
					<px:PXGridColumn AllowNull="False" DataField="TaxReportLine__LineMult" RenderEditorText="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
