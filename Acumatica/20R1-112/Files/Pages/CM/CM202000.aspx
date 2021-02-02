<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM202000.aspx.cs" Inherits="Page_CM202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CuryListRecords" TypeName="PX.Objects.CM.CurrencyMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="newform" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CuryListRecords" Caption="General Info" NoteIndicator="False" FilesIndicator="False" ActivityIndicator="False" TemplateContainer="" TabIndex="4700">
		<AutoSize Container="Window" Enabled="True" />
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" ColumnSpan="2" GroupCaption="General Settings" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" AutoRefresh="True" DataSourceID="ds" Size="S"  />
            <px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" CommitChanges="True" />
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="False" />
			<px:PXTextEdit ID="edCurySymbol" runat="server" DataField="CurySymbol" Size="S" />
			<px:PXNumberEdit ID="edDecimalPlaces" runat="server" DataField="DecimalPlaces" MaxLength="1" Size="S" CommitChanges="True" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXCheckBox ID="edIsFinancial" runat="server" DataField="IsFinancial" CommitChanges="True" />
		</Template>
	</px:PXFormView>
    </asp:Content>
    <asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab1" runat="server" Width="100%" DataMember="CuryRecords"  Height="320px">
		<Items>
			<px:PXTabItem Text="GL Accounts">
				<Template>
	                <px:PXFormView ID="formGLAccounts" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CuryRecords" Caption="GL Accounts" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity">
		                <AutoSize Container="Window" Enabled="True" />
		                <Template>
			                <px:PXLayoutRule runat="server" GroupCaption="Realized Gain and Loss Accounts" StartGroup="True" StartRow="True" LabelsWidth="SM" ControlSize="M"/>
			                <px:PXSegmentMask CommitChanges="True" ID="edRealGainAcctID" runat="server" DataField="RealGainAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRealGainSubID" runat="server" DataField="RealGainSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRealLossAcctID" runat="server" DataField="RealLossAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRealLossSubID" runat="server" DataField="RealLossSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXLayoutRule runat="server" GroupCaption="Unrealized Gain and Loss Accounts" StartGroup="True" LabelsWidth="SM" ControlSize="M"/>
			                <px:PXSegmentMask CommitChanges="True" ID="edUnrealizedGainAcctID" runat="server" DataField="UnrealizedGainAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edUnrealizedGainSubID" runat="server" DataField="UnrealizedGainSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edUnrealizedLossAcctID" runat="server" DataField="UnrealizedLossAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edUnrealizedLossSubID" runat="server" DataField="UnrealizedLossSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXLayoutRule runat="server" GroupCaption="Unrealized Gain and Loss Provisioning Accounts" StartGroup="True" LabelsWidth="SM" ControlSize="M"/>
			                <px:PXSegmentMask CommitChanges="True" ID="edAPProvAcctID" runat="server" DataField="APProvAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edAPProvSubID" runat="server" DataField="APProvSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edARProvAcctID" runat="server" DataField="ARProvAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edARProvSubID" runat="server" DataField="ARProvSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			                <px:PXLayoutRule runat="server" GroupCaption="Revaluation Gain and Loss Accounts" StartGroup="True" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRevalGainAcctID" runat="server" DataField="RevalGainAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRevalGainSubID" runat="server" DataField="RevalGainSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRevalLossAcctID" runat="server" DataField="RevalLossAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRevalLossSubID" runat="server" DataField="RevalLossSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXLayoutRule runat="server" GroupCaption="Translation Gain and Loss Accounts" StartGroup="True" />
			                <px:PXSegmentMask CommitChanges="True" ID="edTranslationGainAcctID" runat="server" DataField="TranslationGainAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edTranslationGainSubID" runat="server" DataField="TranslationGainSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edTranslationLossAcctID" runat="server" DataField="TranslationLossAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edTranslationLossSubID" runat="server" DataField="TranslationLossSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXLayoutRule runat="server" GroupCaption="Rounding Gain and Loss Accounts" StartGroup="True" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRoundingGainAcctID" runat="server" DataField="RoundingGainAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRoundingGainSubID" runat="server" DataField="RoundingGainSubID" AutoRefresh="True" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRoundingLossAcctID" runat="server" DataField="RoundingLossAcctID" DataSourceID="ds" />
			                <px:PXSegmentMask CommitChanges="True" ID="edRoundingLossSubID" runat="server" DataField="RoundingLossSubID" AutoRefresh="True" DataSourceID="ds" />
		                </Template>
	                </px:PXFormView>
                    </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Rounding Settings">
				<Template>
	                <px:PXFormView ID="formRoundingSettings" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CuryRecords" Caption="Rounding Settings" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity">
		                <AutoSize Container="Window" Enabled="True" />
		                <Template>
			                <px:PXLayoutRule runat="server" GroupCaption="Accounts Receivable" StartGroup="True" StartRow="True" LabelsWidth="M" ControlSize="M"/>
			                <px:PXCheckBox ID="chkUseARPreferencesSettings" runat="server" DataField="UseARPreferencesSettings" CommitChanges="true" />
                            <px:PXDropDown ID="edARInvoiceRounding" runat="server" DataField="ARInvoiceRounding" CommitChanges="true" />
					        <px:PXDropDown ID="edARInvoicePrecision" runat="server" DataField="ARInvoicePrecision" />
			                <px:PXLayoutRule runat="server" GroupCaption="Accounts Payable" StartGroup="True" LabelsWidth="M" ControlSize="M"/>
			                <px:PXCheckBox ID="chkUseAPPreferencesSettings" runat="server" DataField="UseAPPreferencesSettings" CommitChanges="true" />
                            <px:PXDropDown ID="edAPInvoiceRounding" runat="server" DataField="APInvoiceRounding" CommitChanges="true" />
					        <px:PXDropDown ID="edAPInvoicePrecision" runat="server" DataField="APInvoicePrecision" />
		                </Template>
	                </px:PXFormView>
                    </Template>
			</px:PXTabItem>
            </Items>
		<AutoSize Enabled="true" Container="Window" MinHeight="320" />
	</px:PXTab>
</asp:Content>
