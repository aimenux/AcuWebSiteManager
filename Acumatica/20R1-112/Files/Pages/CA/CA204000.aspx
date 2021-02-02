<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA204000.aspx.cs" Inherits="Page_CA204000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="PaymentMethod" TypeName="PX.Objects.CA.PaymentMethodMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="PaymentMethod" Caption="Payment Method Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" DefaultControlID="edPaymentMethodID"
		ActivityField="NoteActivity">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" DataSourceID="ds" AutoRefresh="true" />
			<px:PXCheckBox CommitChanges="True" ID="chkARIsProcessingRequired" runat="server" DataField="ARIsProcessingRequired" Visible="False" />
			<px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />
			<px:PXCheckBox ID="chkContainsPersonalData" runat="server" DataField="ContainsPersonalData" />
			<px:PXDropDown CommitChanges="True" ID="edPaymentType" runat="server" DataField="PaymentType" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseForAP" runat="server" DataField="UseForAP" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseForAR" runat="server" DataField="UseForAR" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseForPR" runat="server" DataField="UseForPR" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseForCA" runat="server" DataField="UseForCA" SuppressLabel="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="300px" Width="100%" DataMember="PaymentMethodCurrent" MarkRequired="Dynamic">
		<Items>
			<px:PXTabItem Text="Allowed Cash Accounts" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdCashAccounts" runat="server" DataSourceID="ds" Height="150px" Width="100%" BorderWidth="0px" SkinID="Details" TabIndex="22500" NoteIndicator="true">
						<AutoCallBack Command="RepaintTab" Target="tab">
						</AutoCallBack>
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="CashAccounts" DataKeyNames="PaymentMethodID,CashAccountID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
									<px:PXSegmentMask ID="edCashAccountID" runat="server" DataField="CashAccountID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CashAccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="CashAccountID_CashAccount_Descr" />
									<px:PXGridColumn DataField="CashAccount__BranchID" />
									<px:PXGridColumn DataField="UseForAP" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="UseForPR" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="APIsDefault" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="APAutoNextNbr" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="APLastRefNbr" />
									<px:PXGridColumn DataField="APBatchLastRefNbr" />
									<px:PXGridColumn DataField="UseForAR" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="ARIsDefault" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="ARIsDefaultForRefund" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="ARAutoNextNbr" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="ARLastRefNbr" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Settings for Use in AR" LoadOnDemand="False" BindingContext="form" VisibleExp="DataControls[&quot;chkUseForAR&quot;].Value = 1">
				<Template>
					<px:PXPanel ID="pnlTop" runat="server" ContentLayout-OuterSpacing="Around" RenderSimple="True" RenderStyle="Simple">
						<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
						<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkARIsProcessingRequired1" runat="server" DataField="ARIsProcessingRequired" />
						<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsAccountNumberRequired0" runat="server" DataField="IsAccountNumberRequired" />
						<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" SuppressLabel="True" />
						<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkVoidOnDepositAccount" runat="server" DataField="ARVoidOnDepositAccount" />
						<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkARHasBillingInfo" runat="server" DataField="ARHasBillingInfo" />
						<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" SuppressLabel="True" />
						<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkDefaultVoidDateToDocDate" runat="server" DataField="ARDefaultVoidDateToDocumentDate" />
					</px:PXPanel>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsWithFilter" Caption="Payment Method Details">
						<Levels>
							<px:PXGridLevel DataMember="DetailsForReceivable" SortOrder="OrderIndex">
								<Columns>
									<px:PXGridColumn DataField="PaymentMethodID" AllowShowHide="Server" />
									<px:PXGridColumn DataField="DetailID" TextAlign="Right" TextCase="Upper" />
									<px:PXGridColumn DataField="Descr" />
									<px:PXGridColumn DataField="IsRequired" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsEncrypted" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn CommitChanges="True" DataField="IsIdentifier" TextAlign="Center" Type="CheckBox" AllowShowHide="Server" />
									<px:PXGridColumn DataField="IsExpirationDate" TextAlign="Center" Type="CheckBox" AllowShowHide="Server" CommitChanges="True" />
									<px:PXGridColumn DataField="IsCVV" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowShowHide="Server" CommitChanges="True" DataField="IsOwnerName" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsCCProcessingID" TextAlign="Center" Type="CheckBox" AllowShowHide="Server" CommitChanges="True" />
									<px:PXGridColumn DataField="OrderIndex" TextAlign="Right" />
									<px:PXGridColumn DataField="EntryMask" />
									<px:PXGridColumn DataField="ValidRegexp" />
									<px:PXGridColumn DataField="DisplayMask" AllowShowHide="Server" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
									<px:PXTextEdit ID="edDetailID" runat="server" DataField="DetailID" />
									<px:PXTextEdit ID="edEntryMask" runat="server" DataField="EntryMask" />
									<px:PXTextEdit ID="edValidRegexp" runat="server" DataField="ValidRegexp" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode InitNewRow="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Settings for Use in AP" VisibleExp="DataControls[&quot;chkUseForAP&quot;].Value = 1" BindingContext="form">
				<Template>
					<px:PXPanel ID="pnlPrintCheck" runat="server" RenderSimple="True" ContentLayout-OuterSpacing="Around" RenderStyle="Simple">
						<px:PXLayoutRule runat="server" StartColumn="true" ColumnWidth="L" />
						<px:PXGroupBox ID="gbAdditionalProcessing" runat="server" Caption="Additional Processing" DataField="APAdditionalProcessing"
							RenderStyle="Fieldset" CommitChanges="true">
							<Template>
								<px:PXRadioButton ID="gbAdditionalProcessing_opP" runat="server" Text="Print Checks"
									Value="P" GroupName="gbAdditionalProcessing" />
								<px:PXRadioButton ID="gbAdditionalProcessing_opB" runat="server" Text="Create BatchPayments"
									Value="B" GroupName="gbAdditionalProcessing" />
								<px:PXRadioButton ID="gbAdditionalProcessing_opN" runat="server" Text="Not Required"
									Value="N" GroupName="gbAdditionalProcessing" />
							</Template>
							<ContentLayout Layout="Stack" />
						</px:PXGroupBox>

						<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M" />

						<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Check Printing Settings" />
						<px:PXSelector CommitChanges="True" ID="edAPCheckReportID" runat="server" DataField="APCheckReportID" AutoGenerateColumns="True" MarkRequired="Dynamic" />
						<px:PXNumberEdit CommitChanges="True" ID="edStubLines" runat="server" DataField="APStubLines" Size="xxs" />
						<px:PXCheckBox CommitChanges="True" ID="chkPrintRemittanceReport" runat="server" DataField="APPrintRemittance" />
						<px:PXSelector CommitChanges="True" ID="edRemittanceReportID" runat="server" DataField="APRemittanceReportID" AutoGenerateColumns="True" ValueField="ScreenID" MarkRequired="Dynamic" />
						<px:PXLayoutRule runat="server" EndGroup="true" />

						<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Export Settings" />
						<px:PXSelector CommitChanges="True" ID="edAPBatchExportSYMappingID" runat="server" DataField="APBatchExportSYMappingID" MarkRequired="Dynamic" />
						<px:PXLayoutRule runat="server" EndGroup="true" />

						<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Payment Settings" />
						<px:PXCheckBox runat="server" DataField="APRequirePaymentRef" ID="edAPRequirePaymentRef" AlignLeft="true" Size="XL" CommitChanges="True" />
						<px:PXLayoutRule runat="server" EndGroup="true" />
					</px:PXPanel>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; margin-top: 18px" Width="100%" SkinID="DetailsWithFilter" Caption="Payment Method Details">
						<Levels>
							<px:PXGridLevel DataMember="DetailsForVendor">
								<Columns>
									<px:PXGridColumn DataField="DetailID" Label="ID" />
									<px:PXGridColumn DataField="Descr" />
									<px:PXGridColumn DataField="IsRequired" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="OrderIndex" TextAlign="Right" />
									<px:PXGridColumn DataField="EntryMask" />
									<px:PXGridColumn DataField="ValidRegexp" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode InitNewRow="True" />
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Settings for Use in PR" VisibleExp="DataControls[&quot;chkUseForPR&quot;].Value = 1" BindingContext="form">
				<Template>
					<px:PXPanel ID="pnlPRProcessing" runat="server" RenderSimple="True" ContentLayout-OuterSpacing="Around" RenderStyle="Simple">
						<px:PXLayoutRule runat="server" StartColumn="true" ColumnWidth="L" />
						<px:PXGroupBox ID="gbPRProcessing" runat="server" Caption="Processing" DataField="PRProcessing" RenderStyle="Fieldset" CommitChanges="true">
							<Template>
								<px:PXRadioButton ID="gbPRProcessingP" runat="server" Text="Print Checks" Value="P" GroupName="gbPRProcessing" />
								<px:PXRadioButton ID="gbPRProcessingB" runat="server" Text="Create Batch Payments" Value="B" GroupName="gbPRProcessing" />
							</Template>
							<ContentLayout Layout="Stack" />
						</px:PXGroupBox>
						<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M" />

						<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Print Settings" />
						<px:PXSelector CommitChanges="True" ID="edPRCheckReportID" runat="server" DataField="PRCheckReportID" AutoGenerateColumns="True" MarkRequired="Dynamic" />
						<px:PXLayoutRule runat="server" EndGroup="true" />

						<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Export Settings" />
						<px:PXSelector CommitChanges="True" ID="edPRBatchExportSYMappingID" runat="server" DataField="PRBatchExportSYMappingID" MarkRequired="Dynamic" />
						<px:PXLayoutRule runat="server" EndGroup="true" />
					</px:PXPanel>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Remittance Settings" BindingContext="form" VisibleExp="DataControls[&quot;chkUseForCA&quot;].Value = 1">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" SkinID="DetailsInTab" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="DetailsForCashAccount" DataKeyNames="PaymentMethodID,UseFor,DetailID">
								<Columns>
									<px:PXGridColumn DataField="DetailID" Label="ID" />
									<px:PXGridColumn DataField="Descr" />
									<px:PXGridColumn DataField="IsRequired" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="OrderIndex" TextAlign="Right" />
									<px:PXGridColumn DataField="EntryMask" />
									<px:PXGridColumn DataField="ValidRegexp" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem BindingContext="form" Text="Processing Centers" VisibleExp='DataControls["chkARIsProcessingRequired"].Value = 1'>
				<Template>
					<px:PXGrid ID="grdProcCenters" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="ProcessingCenters">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" />
									<px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />
									<px:PXCheckBox ID="chkIsDefault" runat="server" DataField="IsDefault" CommitChanges="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ProcessingCenterID" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AutoCallBack="True" DataField="IsDefault" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
