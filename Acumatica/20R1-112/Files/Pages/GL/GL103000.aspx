<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL103000.aspx.cs" Inherits="Page_GL103000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLConsolSetupMaint"
		PrimaryView="GLSetupRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="GLSetupRecord" Caption="Consolidation Summary"
		TabIndex="100">
		<Template>
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='M' ControlSize="M" />
			<px:PXSelector ID="edConsolSegmentId" runat="server" AllowNull="True" DataField="ConsolSegmentId" CommitChanges="True">
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="378px" Style="z-index: 100; height: 378px;"
		Width="100%" Caption="Consolidation Details" AllowSearch="True" SkinID="Details"
		SyncPosition="True" TabIndex="200">
		<Mode AllowFormEdit="True" />
		<Levels>
			<px:PXGridLevel DataMember="ConsolSetupRecords">
				<Columns>
					<px:PXGridColumn DataField="Selected" DefValueText="False" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="IsActive" DefValueText="True" TextAlign="Center" Type="CheckBox">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="BranchID" AutoCallBack="true">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="LedgerId">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SegmentValue">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="PasteFlag" TextAlign="Center" Type="CheckBox">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Description">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Login">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Password">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Url">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SourceLedgerCD" AutoCallBack="true">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SourceBranchCD">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="StartPeriod">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="EndPeriod">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="LastPostPeriod">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="LastConsDate">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="BypassAccountSubValidation" DefValueText="False" TextAlign="Center" Type="CheckBox">
					</px:PXGridColumn>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='M' ControlSize="M" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected">
					</px:PXCheckBox>
					<px:PXCheckBox ID="IsActive" runat="server" DataField="IsActive" Text="Active">
					</px:PXCheckBox>
					<px:PXSegmentMask OnValueChange="Commit" ID="edBranchID" runat="server" DataField="BranchID">
					</px:PXSegmentMask>
					<px:PXSelector ID="edLedgerId" runat="server" DataField="LedgerId" AutoRefresh="True">
					</px:PXSelector>
					<px:PXSelector ID="edSegmentValue" runat="server" DataField="SegmentValue" AutoRefresh="True">
					</px:PXSelector>
					<px:PXCheckBox ID="chkPasteFlag" runat="server" DataField="PasteFlag">
					</px:PXCheckBox>
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
					</px:PXTextEdit>
					<px:PXTextEdit ID="edLogin" runat="server" DataField="Login">
					</px:PXTextEdit>
					<px:PXTextEdit ID="edPassword" runat="server" DataField="Password">
					</px:PXTextEdit>
					<px:PXTextEdit ID="edUrl" runat="server" DataField="Url">
					</px:PXTextEdit>
					<px:PXLayoutRule runat="server" LabelsWidth='M' ControlSize="M" StartColumn="True">
					</px:PXLayoutRule>
					<px:PXSelector ID="edSourceLedgerCD" runat="server" DataField="SourceLedgerCD" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam Name="GLConsolSetup.setupID" ControlID="grid" PropertyName="DataValues[&quot;SetupID&quot;]"
								Type="Int32" />
						</Parameters>
					</px:PXSelector>
					<px:PXSelector ID="edSourceBranchCD" runat="server" DataField="SourceBranchCD" AutoRefresh="True"
						Enabled="False">
					</px:PXSelector>
					<px:PXSelector Size="s" ID="edStartPeriod" runat="server" DataField="StartPeriod">
					</px:PXSelector>
					<px:PXSelector Size="s" ID="edEndPeriod" runat="server" DataField="EndPeriod">
					</px:PXSelector>
					<px:PXMaskEdit Size="s" ID="edLastPostPeriod" runat="server" DataField="LastPostPeriod"
						Enabled="False">
					</px:PXMaskEdit>
					<px:PXDateTimeEdit Size="s" ID="edLastConsDate" runat="server" DataField="LastConsDate"
						Enabled="False">
					</px:PXDateTimeEdit>
					<px:PXCheckBox ID="BypassAccountSubValidation" runat="server" DataField="BypassAccountSubValidation"
						Text="Bypass Account/Sub Validation">
					</px:PXCheckBox>
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Save PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
