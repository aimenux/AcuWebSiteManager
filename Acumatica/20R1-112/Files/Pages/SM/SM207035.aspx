<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM207035.aspx.cs" Inherits="Page_SM207035"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script language="javascript" type="text/javascript">
		var isfirstshow = false;
		function panelPreparedShown(sender, e) {
			var grid = px_all[gridPreparedID]; //gridPreparedID is registered by server
			grid.adjustPage(true);
			isfirstshow = true;
			grid.events.addEventHandler("afterRefresh", onGridRefreshed);
		}

		function onGridRefreshed(sender, e) {
			setGridHeight(px_all[pnlPreparedDataID], sender); // pnlPreparedDataID is registered by server
			if (isfirstshow) {
				isfirstshow = false;
				sender.setPage(0);
			}
		}

		function setGridHeight(pnl, grid) {
			grid.setHeight(pnl.element.offsetHeight - 80);
		}
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Operation"
		TypeName="PX.Api.SYExportProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="viewHistory" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="true" Name="viewPreparedData" Visible="False"
				DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="savePreparedData" Visible="False" />
			<px:PXDSCallbackCommand Name="viewReplacement" Visible="False" DependOnGrid="gridPreparedData" />
            <px:PXDSCallbackCommand Name="replaceOneValue" Visible="False" />
            <px:PXDSCallbackCommand Name="replaceAllValues" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlHistory" runat="server" Style="left: 0px; position: relative;
		top: 0px; height: 480px;" Width="640px" Caption="History" CaptionVisible="True"
		Key="History" AutoReload="True" LoadOnDemand="True">
		<px:PXGrid ID="grHistory" runat="server" DataSourceID="ds" Height="460px" Style="z-index: 100"
			Width="100%" AutoAdjustColumns="True" AllowPaging="true" PageSize="100">
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
			<Parameters>
				<px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
			</Parameters>
			<Levels>
				<px:PXGridLevel DataMember="History">
					<RowTemplate>
						<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
						<px:PXDateTimeEdit ID="edStatusDate" runat="server" DataField="StatusDate" DisplayFormat="g"
							EditFormat="g" Enabled="False" />
						<px:PXDropDown ID="edStatusH" runat="server" DataField="Status" Enabled="False" />
						<px:PXNumberEdit ID="edNbrRecordsH" runat="server" DataField="NbrRecords" Enabled="False" /></RowTemplate>
					<Columns>
						<px:PXGridColumn AllowUpdate="False" DataField="StatusDate" DisplayFormat="g" Width="100px" />
						<px:PXGridColumn AllowUpdate="False" DataField="Status" RenderEditorText="True" Width="150px" />
						<px:PXGridColumn AllowUpdate="False" DataField="NbrRecords" TextAlign="Right" Width="100px" />
						<px:PXGridColumn AllowUpdate="False" DataField="ExportTimeStamp" TextAlign="Left"
							Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" MinHeight="150" />
            <ActionBar Position="Bottom" PagerGroup="1" PagerOrder="2" PagerVisible="False" ActionsText="False">
		        <BottomGroups>
			        <px:PXActionGroup Separator="Label" SeparatorWidth="100%" />
			        <px:PXActionGroup />
		        </BottomGroups>
                <Actions>
			        <PageFirst GroupIndex="1" Order="0" ToolBarVisible="Bottom" MenuVisible="False" />
			        <PagePrev GroupIndex="1" Order="1" ToolBarVisible="Bottom" MenuVisible="False" />
			        <PageNext GroupIndex="1" Order="3" ToolBarVisible="Bottom" MenuVisible="False" />
			        <PageLast GroupIndex="1" Order="4" ToolBarVisible="Bottom" MenuVisible="False" />
		        </Actions>
                <PagerStyles>
			        <Cell CssClass="GridPagerCell" />
			        <Link CssClass="GridPagerLink" />
		        </PagerStyles>
            </ActionBar>
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlPreparedData" runat="server" Style="left: 0px; position: relative;
		top: 0px;" Width="1024px" Caption="Prepared Data" CaptionVisible="True"
		Key="PreparedData" ClientEvents-AfterShow="panelPreparedShown">
		<px:PXGrid ID="gridPreparedData" runat="server" DataSourceID="ds" Height="411px"
			Style="z-index: 100" Width="100%" AutoGenerateColumns="AppendDynamic" AllowPaging="True"
			AdjustPageSize="Auto" SkinID="Inquire">
			<Mode AllowAddNew="False" AllowDelete="False" />
			<Parameters>
				<px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
			</Parameters>
			<Levels>
				<px:PXGridLevel DataMember="PreparedData">
					<RowTemplate>
						<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
						<px:PXLayoutRule runat="server" Merge="True" />
						<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
						<px:PXTextEdit Size="s" ID="edErrorMessage" runat="server" DataField="ErrorMessage" />
						<px:PXLayoutRule runat="server" Merge="False" />
						<px:PXCheckBox ID="chkIsProcessed" runat="server" DataField="IsProcessed" />
					</RowTemplate>
					<Columns>
						<px:PXGridColumn DataField="LineNbr" Width="50px" />
						<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
							Width="60px" />
						<px:PXGridColumn AllowNull="False" DataField="IsProcessed" TextAlign="Center" Type="CheckBox"
							Width="60px" />
						<px:PXGridColumn DataField="ErrorMessage" Width="100px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
		    <ActionBar PagerVisible="False"/>
			<AutoSize MinHeight="150" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnClose" runat="server" DialogResult="Cancel" Text="Close" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Parameters" DataMember="Operation" CheckChanges="False">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="M" />
			<px:PXDropDown CommitChanges="True" ID="edOperation" runat="server" DataField="Operation" />
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
			<px:PXCheckBox ID="chkSkipHeadersr" runat="server" DataField="SkipHeaders" AlignLeft="True" Size="M" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" AdjustPageSize="Auto" AllowPaging="True" Caption="Scenarios" SkinID="Inquire"
		BatchUpdate="True">
		<Levels>
			<px:PXGridLevel DataMember="Mappings">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector ID="edName" runat="server" DataField="Name" Enabled="False" AllowEdit="true" />
					<px:PXTextEdit ID="edScreenID" runat="server" DataField="ScreenID" Enabled="False" />
					<px:PXSelector ID="edProviderID" runat="server" DataField="ProviderID" Enabled="False" />
					<px:PXDropDown ID="edSyncType" runat="server" AllowNull="False" DataField="SyncType"
						Enabled="False" />
					<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status"
						Enabled="False" />
					<px:PXNumberEdit ID="edNbrRecords" runat="server" AllowNull="False" DataField="NbrRecords"
						Enabled="False" />
					<px:PXDateTimeEdit ID="edPreparedOn" runat="server" DataField="PreparedOn" DisplayFormat="g"
						Enabled="False" />
					<px:PXDateTimeEdit ID="edCompletedOn" runat="server" DataField="CompletedOn" DisplayFormat="g"
						Enabled="False" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center"
						Type="CheckBox" Width="30px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Name" Width="308px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ScreenDescription" Width="208px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ProviderID" Width="158px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="SyncType" RenderEditorText="True"
						Width="150px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True"
						Width="150px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="NbrRecords" TextAlign="Right"
						Width="74px" />
					<px:PXGridColumn AllowUpdate="False" DataField="PreparedOn" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CompletedOn" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton CommandName="viewHistory" CommandSourceID="ds" Text="View History"
					Tooltip="Shows history for the chosen item." />
			    <px:PXToolBarButton CommandName="viewPreparedData" CommandSourceID="ds" Text="Prepared Data"
					Tooltip="Shows prepared data for the chosen item." />
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
