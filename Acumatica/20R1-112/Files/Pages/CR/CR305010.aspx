<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR305010.aspx.cs" Inherits="Page_CR305010" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.MergeEntry"
		PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" PopupVisible="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Previous" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Next" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Last" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Prepare" PopupVisible="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" PopupVisible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Document" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True"
		NotifyIndicator="True" Caption="Merge Summary" DefaultControlID="edMergeID">
		<Template>
			<px:PXLayoutRule runat="server" ColumnWidth="L">
			</px:PXLayoutRule>
			<px:PXSelector ID="edMergeCD" runat="server" DataField="MergeCD" DataSourceID="ds"
				NullText="<SELECT>" AllowEdit="True" TextMode="Search">
				<AutoCallBack Enabled="True" Command="Cancel" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
				<px:PXGrid ID="grdGroups" runat="server" DataSourceID="ds" SyncPosition="True" AutoAdjustColumns="True"
					SkinID="Details" Width="100%" Caption="Groups">
					<Levels>
						<px:PXGridLevel DataMember="Groups">
							<Columns>
								<px:PXGridColumn DataField="Selected" AllowCheckAll="True" Type="CheckBox" TextAlign="Center"
									Width="10px" AllowShowHide="False" AllowResize="False" />
								<px:PXGridColumn DataField="EstCount">
								</px:PXGridColumn>
								<px:PXGridColumn DataField="ShortDescr">
								</px:PXGridColumn>
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<ActionBar PagerVisible="False">
						<Actions>
							<AddNew Enabled="false" />
							<Delete Enabled="false" />
							<EditRecord Enabled="false" />
						</Actions>
					</ActionBar>
					<AutoCallBack Target="grdItems" Command="Refresh" Enabled="True" />
					<Mode AllowAddNew="false" AllowDelete="false" />
					<AutoSize Enabled="True"></AutoSize>
				</px:PXGrid>
                        </Template1>
                        <Template2>
				<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="100%" Width="100%"
					AdjustPageSize="Auto" SkinID="Details" Caption="Items" DependsOnControlIDs="grdGroups"
					AutoGenerateColumns="AppendDynamic" RepaintColumns="True">
					<Levels>
						<px:PXGridLevel DataMember="Items">
							<Columns>
								<px:PXGridColumn DataField="Selected" AllowCheckAll="True" Type="CheckBox" TextAlign="Center"
									Width="20px" AllowShowHide="False" AllowResize="False" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<ActionBar PagerVisible="False">
						<Actions>
							<AddNew Enabled="false" />
							<Delete Enabled="false" />
							<EditRecord Enabled="false" />
						</Actions>
					</ActionBar>
					<Mode AllowAddNew="false" AllowDelete="false" />
					<AutoSize Enabled="True" />
				</px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
</asp:Content>
