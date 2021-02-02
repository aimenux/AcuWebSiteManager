<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA504500.aspx.cs"
	Inherits="Page_FA504500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.AssetGLTransactions">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Options" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="AccountID" ID="edAccountID" />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="SubID" ID="edSubID" />
			<px:PXCheckBox ID="chkShowReconciled" runat="server" DataField="ShowReconciled" CommitChanges="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="EmployeeID" ID="edEmployeeID" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="Department" ID="edDepartment" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="300px"
		Panel1MinSize="250" Panel2MinSize="250">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 188px;" Width="100%"
				SkinID="Inquire" Caption="GL Transactions" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True">
				<Levels>
					<px:PXGridLevel DataMember="GLTransactions">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXSelector ID="edClassID" runat="server" DataField="ClassID" />
							<px:PXLabel ID="lblLocationIDH" runat="server"></px:PXLabel>
                                <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" />
							<px:PXSelector ID="edDepartment" runat="server" DataField="Department" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px"
								AutoCallBack="True" />
							<px:PXGridColumn DataField="ClassID" AutoCallBack="True" />
							<px:PXGridColumn DataField="Reconciled" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
							<px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAA" AutoCallBack="True" />
							<px:PXGridColumn DataField="EmployeeID" Label="EmployeeID" AutoCallBack="True" />
							<px:PXGridColumn DataField="Department" AutoCallBack="True" />
							<px:PXGridColumn AllowUpdate="False" DataField="GLTranBranchID" DisplayFormat="&gt;AAAAAAAAAA" />
							<px:PXGridColumn AllowUpdate="False" DataField="GLTranInventoryID" DisplayFormat="&gt;AAAAAAAAAAAA" />
							<px:PXGridColumn DataField="GLTranUOM" DisplayFormat="&gt;aaaaaa" />
							<px:PXGridColumn AllowUpdate="False" DataField="SelectedQty" TextAlign="Right" />
							<px:PXGridColumn AllowUpdate="False" DataField="SelectedAmt" TextAlign="Right" />
							<px:PXGridColumn AllowUpdate="False" DataField="OpenQty" TextAlign="Right" />
							<px:PXGridColumn AllowUpdate="False" DataField="OpenAmt" TextAlign="Right" />
							<px:PXGridColumn AllowUpdate="False" DataField="GLTranQty" TextAlign="Right" />
							<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="UnitCost" TextAlign="Right" />
							<px:PXGridColumn AllowUpdate="False" DataField="GLTranAmt" TextAlign="Right" />
							<px:PXGridColumn AllowUpdate="False" DataField="GLTranDate" />
							<px:PXGridColumn DataField="GLTranModule" />
							<px:PXGridColumn DataField="GLTranBatchNbr" />
							<px:PXGridColumn AllowNull="False" DataField="GLTranRefNbr" />
							<px:PXGridColumn DataField="GLTranReferenceID" />
							<px:PXGridColumn DataField="GLTranDesc" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Enabled="True" MinHeight="200" />
				<AutoCallBack Command="Refresh" Target="gridSplit" />
				<Mode AllowAddNew="False" AllowDelete="False" />
				<ActionBar PagerVisible="False">
				</ActionBar>
			</px:PXGrid>
		</Template1>
		<Template2>
			<px:PXGrid ID="gridSplit" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 280px;" Width="100%"
				SkinID="Details" Caption="Transaction Split Details" AdjustPageSize="Auto" AllowPaging="True">
				<Levels>
					<px:PXGridLevel DataMember="FATransactions">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" />
							<px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
							<px:PXSelector ID="edBookID" runat="server" DataField="BookID" />
							<px:PXSelector CommitChanges="True" ID="edClassID1" runat="server" DataField="ClassID" />
							<px:PXSelector CommitChanges="True" ID="edTargetAssetID" runat="server" DataField="TargetAssetID" />
							<px:PXLabel ID="lblLocationIDH1" runat="server"></px:PXLabel>
                                <px:PXSelector ID="edEmployeeID1" runat="server" DataField="EmployeeID" />
							<px:PXSelector CommitChanges="True" ID="edDepartment1" runat="server" DataField="Department" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="NewAsset" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
							<px:PXGridColumn DataField="Component" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
							<px:PXGridColumn DataField="ClassID" AutoCallBack="True" />
							<px:PXGridColumn AutoCallBack="True" DataField="AssetCD" />
							<px:PXGridColumn DataField="Qty" TextAlign="Right" />
							<px:PXGridColumn DataField="TargetAssetID" AutoCallBack="True" />
							<px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAA" />
							<px:PXGridColumn DataField="EmployeeID" Label="EmployeeID" />
							<px:PXGridColumn DataField="Department" />
							<px:PXGridColumn DataField="TranType" RenderEditorText="True" />
							<px:PXGridColumn AutoCallBack="True" DataField="ReceiptDate" />
							<px:PXGridColumn DataField="DeprFromDate" />
							<px:PXGridColumn DataField="TranDate" AutoCallBack="True" />
							<px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" CommitChanges="True"/>
							<px:PXGridColumn DataField="TranAmt" TextAlign="Right" AutoCallBack="True" />
							<px:PXGridColumn DataField="TranDesc" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Enabled="True" MinHeight="200" />
				<Mode InitNewRow="True" AutoInsert="True" />
				<ActionBar PagerVisible="False" />
				<CallbackCommands>
					<Refresh CommitChanges="True" />
				</CallbackCommands>
				<Parameters>
					<px:PXSyncGridParam ControlID="grid" />
				</Parameters>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
