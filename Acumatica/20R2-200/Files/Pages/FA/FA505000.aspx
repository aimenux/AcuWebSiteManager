<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA505000.aspx.cs" Inherits="Page_FA505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.DisposalProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Caption="Options" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" MarkRequired="Dynamic" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="ParentAssetID" ID="edParentAssetID" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="BookID" ID="edBookID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DisposalDate" ID="edDisposalDate" />
			<px:PXSelector ID="edDisposalPeriodID" runat="server" DataField="DisposalPeriodID" CommitChanges="True" AutoCallBack="True" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="DisposalMethodID" ID="edDisposalMethodID" />
			<px:PXNumberEdit runat="server" DataField="DisposalAmt" ID="edDisposalAmt" />
			<px:PXDropDown CommitChanges="True" ID="edDisposalAmtMode" runat="server" AllowNull="False" DataField="DisposalAmtMode" SelectedIndex="-1" />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="DisposalAccountID" ID="edDisposalAccountID" />
			<px:PXSegmentMask runat="server" DataField="DisposalSubID" ID="edDisposalSubID" AutoRefresh="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkDeprBeforeDisposal" runat="server" DataField="DeprBeforeDisposal" />
			<px:PXTextEdit CommitChanges="True" ID="edReason" runat="server" DataField="Reason" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Assets to Dispose"
		Width="100%" Height="150px" SkinID="PrimaryInquire"
		AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
		FastFilterFields="AssetCD, Description" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataKeyNames="AssetCD" DataMember="Assets">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="BranchID" />
					<px:PXGridColumn AllowUpdate="False" DataField="ClassID" />
					<px:PXGridColumn AllowUpdate="False" DataField="AssetCD" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn AllowUpdate="False" DataField="ParentAssetID" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="FADetails__CurrentCost" TextAlign="Right" />
					<px:PXGridColumn DataField="DisposalAmt" TextAlign="Right" AutoCallBack="True" />
					<px:PXGridColumn DataField="FADetails__ReceiptDate" />
					<px:PXGridColumn AllowUpdate="False" DataField="UsefulLife" TextAlign="Right" />
					<px:PXGridColumn AllowUpdate="False" DataField="FAAccountID" />
					<px:PXGridColumn AllowUpdate="False" DataField="FASubID" />
					<px:PXGridColumn AllowUpdate="False" DataField="FADetails__TagNbr" />
					<px:PXGridColumn DataField="Account__AccountClassID" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar PagerVisible="False" />
	</px:PXGrid>
</asp:Content>
