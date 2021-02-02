<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM304000.aspx.cs" Inherits="Page_CM304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TranslHistRecords" TypeName="PX.Objects.CM.TranslationHistoryMaint" 
        PageLoadBehavior="GoLastRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="release" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="ViewBatch" Visible="False" />
            <px:PXDSCallbackCommand Name="TranslationDetailsReport" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="TranslHistRecords" Caption="Translation Summary" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True"
		ActivityIndicator="True" ActivityField="NoteActivity" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXSelector ID="edReferenceNbr" runat="server" DataField="ReferenceNbr" AutoRefresh="True" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXDateTimeEdit ID="edDateEntered" runat="server" DataField="DateEntered" Enabled="False" />
			<px:PXDateTimeEdit ID="edCuryEffDate" runat="server" DataField="CuryEffDate" Enabled="False" />
			<px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
            <px:PXLayoutRule ID="desrLayoutRule" runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXSelector ID="edTranslDefId" runat="server" DataField="TranslDefId" Enabled="False" />
            <px:PXSelector ID="edBranchId" runat="server" DataField="BranchID" Enabled="False"/>
			<px:PXSelector ID="edLedgerID" runat="server" DataField="LedgerID" Enabled="False" />
			<px:PXTextEdit ID="edDestCuryID" runat="server" DataField="DestCuryID" Enabled="False" />
			<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit ID="edDebitTot" runat="server" DataField="DebitTot" Enabled="False" />
			<px:PXNumberEdit ID="edCreditTot" runat="server" DataField="CreditTot" Enabled="False" />
			<px:PXNumberEdit ID="edControlTot" runat="server" DataField="ControlTot" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True" ActionsPosition="Top" Caption="Translation Details" AllowSearch="True" AdjustPageSize="Auto" SkinID="Details"
		FloatingEditor="False">
		<Levels>
			<px:PXGridLevel DataMember="TranslHistDetRecords">
				<Columns>
					<px:PXGridColumn DataField="BranchID"/>
					<px:PXGridColumn DataField="AccountID" />
					<px:PXGridColumn DataField="AccountID_Account_description" />
					<px:PXGridColumn DataField="SubID" />
					<px:PXGridColumn DataField="CalcMode" Type="DropDownList" />
					<px:PXGridColumn DataField="SourceAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="TranslatedAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="OrigTranslatedAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="RateTypeID" />
					<px:PXGridColumn DataField="CuryRate" TextAlign="Right" />
					<px:PXGridColumn DataField="DebitAmt" TextAlign="Right" AutoCallBack="True" />
					<px:PXGridColumn DataField="CreditAmt" TextAlign="Right" AutoCallBack="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="False" AllowUpdate="True" AllowDelete="true" />
	</px:PXGrid>
</asp:Content>
