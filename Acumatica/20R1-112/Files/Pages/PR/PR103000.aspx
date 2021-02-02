<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR103000.aspx.cs" Inherits="Page_PR103000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TaxCodes" TypeName="PX.Objects.PR.PRTaxCodeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="GetTaxCodes" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="TaxCodes" TabIndex="1000" AllowCollapse="True">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" StartGroup="True" ControlSize="XM" />
			<px:PXSelector ID="edTaxCD" runat="server" DataField="TaxCD" AutoRefresh="True" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXDropDown ID="edTaxCatType" runat="server" DataField="TaxCategory" CommitChanges="True" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXSelector ID="edGovtRefNbr" runat="server" DataField="GovtRefNbr" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXSelector ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="true" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXDropDown ID="edTaxInvDescrType" runat="server" DataField="TaxInvDescrType" CommitChanges="true" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXTextEdit runat="server" DataField="VndInvDescr" ID="edVndInvDescr" Size="XL" />

			<px:PXLayoutRule runat="server" GroupCaption="Tax Engine Info" StartGroup="True" StartRow="True" LabelsWidth="M" ControlSize="XM" Merge="true" />
			<px:PXSelector ID="edTypeName" runat="server" DataField="TypeName" AutoRefresh="True" CommitChanges="true" />
			<px:PXDropDown runat="server" DataField="JurisdictionLevel" ID="edJurisdictionLevel" LabelWidth="150px" Enabled="false" />

			<px:PXLayoutRule runat="server" Merge="true" LabelsWidth="M" ControlSize="XM" />
			<px:PXDropDown ID="edWageBaseType" runat="server" DataField="WageBaseType" AutoRefresh="True" CommitChanges="true" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="XM" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXSelector ID="edTaxState" runat="server" DataField="TaxState" CommitChanges="true" />

			<px:PXLayoutRule runat="server" StartRow="True" Merge="true" LabelsWidth="XM" ControlSize="XM" />
			<px:PXButton ID="PxButton1" runat="server" CommandName="GetTaxCodes" CommandSourceID="ds" Width="150px" />

			<px:PXLayoutRule runat="server" StartRow="True" Merge="true" LabelsWidth="M" ControlSize="XM" />
			<px:PXTextEdit ID="edTaxUniqueCode" runat="server" DataField="TaxUniqueCode" />
			<px:PXTextEdit ID="edTaxUniqueCodeDescription" runat="server" DataField="TaxUniqueCodeDescription" SuppressLabel="true" Width="350px" Enabled="false" />
		</Template>
	</px:PXFormView>
	<px:PXSmartPanel runat="server" ID="panelUniqueID" Caption="Select Tax" CaptionVisible="true" LoadOnDemand="true"
		Key="UniqueID" Width="240px" Height="100px" AutoCallBack-Enabled="true" AutoCallBack-Target="frmUniqueID"
		AutoCallBack-Command="Refresh" AutoCallBack-Behavior-PostData="Page" DesignView="Hidden" AcceptButtonID="OK">
		<px:PXFormView runat="server" ID="frmUniqueID" DataSourceID="ds" DataMember="UniqueID" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="L" />
				<px:PXSelector ID="edUniqueID" runat="server" DataField="TaxUniqueCode" DisplayMode="Text" CommitChanges="true" AutoRefresh="True" />
			</Template>
		</px:PXFormView>
		<px:PXLayoutRule runat="server" StartRow="true" Merge="true" SuppressLabel="true" />
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnUniqueIDSave" runat="server" DialogResult="OK" Text="OK" Width="120px" />
			<px:PXButton ID="btnUniqueIDCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CurrentTaxCode">
		<Items>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="130px" SkinID="Inquire" TabIndex="800" MatrixMode="true">
						<Levels>
							<px:PXGridLevel DataMember="Attributes">
								<Columns>
									<px:PXGridColumn DataField="Description" Width="400px" />
									<px:PXGridColumn DataField="AllowOverride" TextAlign="Center" Type="CheckBox" Width="150px" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="80px" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="120px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXFormView ID="frmGLAccounts" runat="server" DataMember="CurrentTaxCode" Width="100%" CaptionVisible="False" DataSourceID="ds" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSelector ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" />
							<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" />
							<px:PXSelector ID="edLiabilityAcctID" runat="server" DataField="LiabilityAcctID" />
							<px:PXSegmentMask ID="edLiabilitySubID" runat="server" DataField="LiabilitySubID" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
