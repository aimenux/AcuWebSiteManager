<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM101000.aspx.cs" Inherits="Page_CM101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="cmsetup" TypeName="PX.Objects.CM.CMSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formSettings" runat="server" DataSourceID="ds" Width="100%" DataMember="cmsetup" Caption="CM Settings" TemplateContainer="" TabIndex="900">
		<AutoSize Container="Window" Enabled="True" />
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			<px:PXLayoutRule runat="server" GroupCaption="Setup and Numbering Settings" StartGroup="True" />
			<px:PXSelector ID="edBatchNumberingID" runat="server" DataField="BatchNumberingID" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edTranslNumberingID" runat="server" DataField="TranslNumberingID" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edExtRefNbrNumberingID" runat="server" AllowEdit="True" DataField="ExtRefNbrNumberingID" DataSourceID="ds" edit="1" />
			<px:PXSegmentMask ID="edGainLossSubMask" runat="server" DataField="GainLossSubMask" AllowEdit="True" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" GroupCaption="Posting Settings" StartGroup="True" />
			<px:PXCheckBox ID="chkAutoPostOption" runat="server" DataField="AutoPostOption" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Data Entry Settings" />
			<px:PXCheckBox ID="chkRateVarianceWarn" runat="server" DataField="RateVarianceWarn" />
			<px:PXNumberEdit ID="edRateVariance" runat="server" DataField="RateVariance" Size="XXS" />
			<px:PXSelector ID="edTranslDefId" runat="server" DataField="TranslDefId" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Rate Types" />
			<px:PXSelector ID="edGLRateTypeDflt" runat="server" DataField="GLRateTypeDflt" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edGLRateTypeReval" runat="server" DataField="GLRateTypeReval" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edCARateTypeDflt" runat="server" DataField="CARateTypeDflt" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edARRateTypeDflt" runat="server" DataField="ARRateTypeDflt" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edARRateTypeReval" runat="server" DataField="ARRateTypeReval" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edAPRateTypeDflt" runat="server" DataField="APRateTypeDflt" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edAPRateTypeReval" runat="server" DataField="APRateTypeReval" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXSelector ID="edPMRateTypeDflt" runat="server" DataField="PMRateTypeDflt" AllowEdit="True" DataSourceID="ds" edit="1" />
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			<px:PXFormView ID="basecurrency" runat="server" RenderStyle="Simple" DataMember="basecurrency" DataSourceID="ds" TabIndex="1100">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Realized Gain and Loss Settings" />
					<px:PXSegmentMask CommitChanges="True" ID="edRealGainAcctID" runat="server" DataField="RealGainAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edRealGainSubID" runat="server" DataField="RealGainSubID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edRealLossAcctID" runat="server" DataField="RealLossAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edRealLossSubID" runat="server" DataField="RealLossSubID" DataSourceID="ds" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Translation Gain and Loss Settings" />
					<px:PXSegmentMask CommitChanges="True" ID="edTranslationGainAcctID" runat="server" DataField="TranslationGainAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edTranslationGainSubID" runat="server" DataField="TranslationGainSubID" AutoRefresh="True" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edTranslationLossAcctID" runat="server" DataField="TranslationLossAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edTranslationLossSubID" runat="server" DataField="TranslationLossSubID" AutoRefresh="True" DataSourceID="ds" />
				</Template>
			</px:PXFormView>
		</Template>
	</px:PXFormView>
</asp:Content>

