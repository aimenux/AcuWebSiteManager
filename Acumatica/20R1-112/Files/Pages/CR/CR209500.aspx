<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR209500.aspx.cs" Inherits="Page_CR209000" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.MergeMaint"
		PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveClose" Visible="false" PopupVisible="true"
				ClosePopup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Prepare" PopupVisible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="99px" Style="z-index: 100"
		Width="100%" DataMember="Document" NoteIndicator="True" FilesIndicator="True"
		LinkIndicator="True" NotifyIndicator="True" Caption="Merge Summary" DefaultControlID="edMergeID">
		<Template>
			<px:PXLayoutRule runat="server" ColumnWidth="M" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXLayoutRule runat="server" ColumnSpan="2">
			</px:PXLayoutRule>
			<px:PXSelector ID="edMergeCD" runat="server" DataField="MergeCD" DataSourceID="ds"
				AutoAdjustColumns="True" NullText="<NEW>">
				<AutoCallBack Enabled="true" Command="Cancel" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXSelector ID="edType" runat="server" DataField="EntityType" DataSourceID="ds"
				AutoAdjustColumns="True" DisplayMode="Text" AllowEdit="True">
				<AutoCallBack Enabled="true" Command="Save" Target="form">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXLayoutRule runat="server" ColumnSpan="3">
			</px:PXLayoutRule>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
			</px:PXTextEdit>
			<px:PXLayoutRule runat="server" ColumnWidth="XXS" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXLabel ID="lblFack" runat="server"></px:PXLabel>
			<px:PXLayoutRule runat="server" ColumnWidth="M" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXCheckBox ID="edRunOnSave" runat="server" DataField="RunOnSave">
			</px:PXCheckBox>
			<px:PXDateTimeEdit ID="edLastRun" runat="server" DataField="LastRun" Enabled="False">
			</px:PXDateTimeEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="420px">
		<Items>
			<px:PXTabItem Text="Criteria">
				<Template>
					<px:PXGrid ID="grdCriteria" runat="server" DataSourceID="ds" SkinID="DetailsInTab"
						Width="100%" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Criteria">
								<Columns>
									<px:PXGridColumn DataField="OpenBrackets">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="DataField" AutoCallBack="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Matching" AutoCallBack="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Value">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="CloseBrackets">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Operator">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode InitNewRow="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Methods">
				<Template>
					<px:PXGrid ID="grdFields" runat="server" DataSourceID="ds" SkinID="DetailsInTab"
						Width="100%" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataKeyNames="MergeID,DataField" DataMember="Methods">
								<Columns>
									<px:PXGridColumn DataField="DataField" AutoCallBack="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Method">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode InitNewRow="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="450" MinWidth="300" />
	</px:PXTab>
</asp:Content>
