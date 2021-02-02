<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL102000.aspx.cs" Inherits="Page_GL102000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="GLSetupRecord" TypeName="PX.Objects.GL.GLSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="GLSetupRecord" Caption="General Settings" AllowCollapse="False" DefaultControlID="edBatchNumberingID" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" StartColumn="True" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" StartColumn="True" />
			<px:PXSelector ID="edBatchNumberingID" runat="server" DataField="BatchNumberingID" AllowEdit="True" />
			<px:PXSelector ID="edTBImportNumberingID" runat="server" DataField="TBImportNumberingID" AllowEdit="True" />
			<px:PXSelector ID="edScheduleNumberingID" runat="server" DataField="ScheduleNumberingID" AllowEdit="True" />
			<px:PXSelector ID="edAllocationNumberingID" runat="server" DataField="AllocationNumberingID" AllowEdit="True" />
			<px:PXSelector ID="edDocBatchNumberingID" runat="server" DataField="DocBatchNumberingID" AllowEdit="True" />
            <px:PXCheckBox ID="edReuseRefNbrsInVouchers" runat="server" DataField="ReuseRefNbrsInVouchers" AlignLeft="true" />
			<px:PXLayoutRule runat="server" GroupCaption="Chart of Accounts Settings" StartGroup="True" />
			<px:PXSegmentMask OnValueChange="Commit" ID="edYtdNetIncAccountID" runat="server" DataField="YtdNetIncAccountID" />
			<px:PXSegmentMask OnValueChange="Commit" ID="edRetEarnAccountID" runat="server" DataField="RetEarnAccountID" />
			<px:PXDropDown ID="edTrialBalanceSign" runat="server" DataField="TrialBalanceSign" />
			<px:PXGroupBox ID="gbCOAOrder" runat="server" Caption="Chart of Accounts Order" DataField="COAOrder">
				<Template>
					<px:PXRadioButton ID="gbCOAOrder_op0" runat="server" Text="1:Assets 2:Liabilities 3:Income and Expenses" Value="0" GroupName="gbCOAOrder" />
					<px:PXRadioButton ID="gbCOAOrder_op1" runat="server" Text="1:Assets 2:Liabilities 3:Income 4:Expenses" Value="1" GroupName="gbCOAOrder" />
					<px:PXRadioButton ID="gbCOAOrder_op2" runat="server" Text="1:Income 2:Expenses 3:Assets 4:Liabilities" Value="2" GroupName="gbCOAOrder" />
					<px:PXRadioButton ID="gbCOAOrder_op3" runat="server" Text="1:Income and Expenses 2:Assets 3:Liabilities" Value="3" GroupName="gbCOAOrder" />
					<px:PXRadioButton ID="gb_COAOrder_op128" runat="server" Text="Custom Chart of Accounts Order" Value="128" AutoPostBack="True" GroupName="gbCOAOrder" />
				</Template>
				<ContentLayout Layout="Stack" />
			</px:PXGroupBox>
		    <px:PXLayoutRule runat="server" GroupCaption="Posting Settings" StartGroup="True" ControlSize="M" LabelsWidth="SM" StartColumn="True"/>
			<px:PXDropDown ID="edAutoRevOption" runat="server" DataField="AutoRevOption" />
			<px:PXCheckBox ID="chkAutoPostOption" runat="server" DataField="AutoPostOption" />
			<px:PXCheckBox ID="chkRestrictAccessToClosedPeriods" runat="server" DataField="RestrictAccessToClosedPeriods" />
            <px:PXCheckBox ID="chkConsolidatedPosting" runat="server" DataField="ConsolidatedPosting" />
			<px:PXCheckBox ID="chkAutoReleaseReclassBatch" runat="server" DataField="AutoReleaseReclassBatch" />
			<px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True" />
			<px:PXCheckBox ID="chkHoldEntry" runat="server" DataField="HoldEntry" />
			<px:PXCheckBox ID="chkVouchersHoldEntry" runat="server" DataField="VouchersHoldEntry" />
			<px:PXCheckBox ID="chkRequireControlTotal" runat="server" DataField="RequireControlTotal"/>
			<px:PXCheckBox ID="chkRequireRefNbrForTaxEntry" runat="server" DataField="RequireRefNbrForTaxEntry"/>
			<px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" />
			<px:PXFormView ID="basecurrency" runat="server" RenderStyle="Simple" DataMember="basecurrency" DataSourceID="ds" TabIndex="1100">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M"/>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" GroupCaption="Rounding Settings" />
					<px:PXSegmentMask CommitChanges="True" ID="edRoundingGainAcctID" runat="server" DataField="RoundingGainAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edRoundingGainSubID" runat="server" DataField="RoundingGainSubID" AutoRefresh="True" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edRoundingLossAcctID" runat="server" DataField="RoundingLossAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edRoundingLossSubID" runat="server" DataField="RoundingLossSubID" AutoRefresh="True" DataSourceID="ds" />
				    <px:PXLayoutRule runat="server" Merge="true" ControlSize="M" LabelsWidth="M"/>
				    <px:PXNumberEdit ID="edRoundingLimit" runat="server" DataField="GLSetupRecord.RoundingLimit" />
                    <px:PXTextEdit runat="server" ID="edCuryID" DataField="CuryID" Enabled ="false" SuppressLabel="true" Size="xs"/>
				</Template>
			</px:PXFormView>
        </Template>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXFormView>
</asp:Content>
