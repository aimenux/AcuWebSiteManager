<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM206015.aspx.cs" Inherits="Page_SM206015"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script language="javascript" type="text/javascript">
		function adjustGridSizes()
		{
			//tabId, grLeft, grRight are registered by server
			var tab = px_all[tabId];
			var grid1 = px_all[grLeft];
			var grid2 = px_all[grRight];

			if (tab.element.clientHeight > 23 && grid1 != null && grid2 != null)
			{
				grid1.setHeight(tab.element.clientHeight - 23);
				grid1.setHeight(tab.element.clientHeight - 23);
			}
		}
		function refreshScreen()
		{
			px_all[gridRevisionsID].refresh();
			px_all[formID].refresh();
		}
		function tabInitialized()
		{
			px_alls["tlbDataView"].events.removeEventHandler("afterUpload", refreshScreen);
			px_alls["tlbDataView"].events.addEventHandler("afterUpload", refreshScreen);
		}
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Providers"
		TypeName="PX.Api.SYProviderMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand DependOnGrid="gridObjects" Name="Cancel" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="reloadParameters" Visible="False" />
			<px:PXDSCallbackCommand Name="fillSchemaObjects" Visible="False" />
			<px:PXDSCallbackCommand Name="fillSchemaFields" Visible="False" />
			<px:PXDSCallbackCommand Name="showObjectCommand" Visible="False" />
			<px:PXDSCallbackCommand Name="showFieldCommand" Visible="False" />
			<px:PXDSCallbackCommand Name="toggleFieldsActivation" Visible="False" DependOnGrid="gridFields" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlGetLink" runat="server" CaptionVisible="true" Caption="Attached File Link"
		ForeColor="Black" Style="position: static" AutoCallBack-Enabled="true" AutoCallBack-Target="ds"
		AutoCallBack-Command="getFileLink" Height="70px" Width="345px" ShowAfterLoad="true"
		Key="GetFileLinkFilter" Overflow="Hidden">
		<px:PXFormView ID="frmGetLink" runat="server" SkinID="Transparent" DataMember="GetFileLinkFilter"
			DataSourceID="ds" Width="342px">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S"
					ControlSize="M" />
				<px:PXTextEdit ID="edWikiLink" runat="server" DataField="WikiLink" ReadOnly="True" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="Cancel" Text="Close" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlEditObjectCommand" runat="server" CaptionVisible="True" Caption="Object Command Editor"
		ForeColor="Black" Style="position: static" Width="700px"
		LoadOnDemand="true" Key="CurrentObject" AutoCallBack-Enabled="True" AutoCallBack-Target="frmEditObjectCommand"
		AutoCallBack-Command="Refresh">
		<px:PXFormView ID="frmEditObjectCommand" runat="server" SkinID="Transparent" DataMember="CurrentObject"
			DataSourceID="ds" Width="700px">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S"
					ControlSize="XXL" />
				<px:PXTextEdit Height="228px" ID="edCommand" runat="server" DataField="Command" CommitChanges="True"
					TextMode="MultiLine" Width="550px" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlEditFieldCommand" runat="server" CaptionVisible="True" Caption="Field Command Editor"
		ForeColor="Black" Style="position: static" Width="700px"
		LoadOnDemand="true" Key="CurrentField" AutoCallBack-Enabled="True" AutoCallBack-Target="frmEditFieldCommand"
		AutoCallBack-Command="Refresh">
		<px:PXFormView ID="frmEditFieldCommand" runat="server" SkinID="Transparent" DataMember="CurrentField"
			DataSourceID="ds" Width="700px">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S"
					ControlSize="XXL" />
				<px:PXTextEdit Height="228px" ID="edCommand" runat="server" DataField="Command" CommitChanges="True"
					TextMode="MultiLine" Width="550px" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Close" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Provider Summary" DataMember="Providers" FilesIndicator="True" NoteIndicator="True">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXSelector runat="server" DataField="Name" ID="edName" AutoRefresh="True"
				MinDropWidth="440" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edProviderType" runat="server" DataField="ProviderType"
				TextField="Description" DataSourceID="ds" />
			<px:PXCheckBox runat="server" Checked="True" DataField="IsActive" ID="chkIsActive" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="270px" Style="z-index: 100" Width="100%">
		<AutoSize Enabled="true" Container="Window" />
		<Items>
			<px:PXTabItem Text="Parameters" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; height: 270px;"
						Width="100%" SkinID="DetailsInTab" AutoAdjustColumns="True" MatrixMode="True"
						OnRowDataBound="grid_RowDataBound" >
						<ActionBar>
							<Actions>
								<NoteShow MenuVisible="False" ToolBarVisible="False" />
								<ExportExcel MenuVisible="False" ToolBarVisible="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton CommandName="reloadParameters" CommandSourceID="ds" Text="Reload Parameters" />
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Parameters">
								<Columns>
									<px:PXGridColumn DataField="Name" Width="108px" />
									<px:PXGridColumn DataField="DisplayName" Width="208px" />
									<px:PXGridColumn DataField="Value" Width="408px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Schema" LoadOnDemand="True">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="450">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="gridObjects" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; height: 270px;"
								Width="100%" SkinID="DetailsInTab" AutoAdjustColumns="True"
								Caption="Source Objects" SyncPosition="True">
								<AutoCallBack Command="Refresh" Target="gridFields" ActiveBehavior="True">
									<Behavior BlockPage="True" CommitChanges="True" RepaintControlsIDs="gridFields" />
								</AutoCallBack>
								<ActionBar>
									<Actions>
										<NoteShow MenuVisible="False" ToolBarVisible="False" />
										<ExportExcel MenuVisible="False" ToolBarVisible="False" />
									</Actions>
									<CustomItems>
										<px:PXToolBarButton CommandName="fillSchemaObjects" CommandSourceID="ds" Text="Fill" />
										<px:PXToolBarButton CommandName="showObjectCommand" CommandSourceID="ds" Text="Edit Command" />
									</CustomItems>
								</ActionBar>
								<Levels>
									<px:PXGridLevel DataMember="Objects">
										<Columns>
											<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
												Width="60px" />
											<px:PXGridColumn DataField="Name" Width="200px" />
											<px:PXGridColumn DataField="Command" Width="150px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridFields" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; height: 270px;"
								Width="100%" SkinID="DetailsInTab" AutoAdjustColumns="True"
								Caption="Source Fields" SyncPosition="True">
								<ActionBar>
									<Actions>
										<NoteShow MenuVisible="False" ToolBarVisible="False" />
									</Actions>
									<CustomItems>
										<px:PXToolBarButton CommandName="fillSchemaFields" CommandSourceID="ds" Text="Fill" />
										<px:PXToolBarButton CommandName="showFieldCommand" CommandSourceID="ds" Text="Edit Command" />
										<px:PXToolBarButton CommandName="toggleFieldsActivation" CommandSourceID="ds" />
									</CustomItems>
								</ActionBar>
								<Parameters>
									<px:PXSyncGridParam ControlID="gridObjects" Name="SyncGrid" />
								</Parameters>
								<Mode InitNewRow="True" />
								<Levels>
									<px:PXGridLevel DataMember="Fields">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXDropDown ID="edDataType" runat="server" DataField="DataType" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
												Width="60px" />
											<px:PXGridColumn DataField="Name" Width="100px" />
											<px:PXGridColumn AllowNull="False" DataField="IsKey" TextAlign="Center" Type="CheckBox"
												Width="50px" />
											<px:PXGridColumn AllowUpdate="False" DataField="DisplayName" Width="200px" />
											<px:PXGridColumn AllowUpdate="False" DataField="DataType" RenderEditorText="true" />
											<px:PXGridColumn DataField="DataLength" TextAlign="Right" Width="54px" />
											<px:PXGridColumn DataField="Command" Width="150px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
		<ClientEvents Initialize="tabInitialized" />
	</px:PXTab>
</asp:Content>
