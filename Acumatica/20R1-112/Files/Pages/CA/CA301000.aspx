<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA301000.aspx.cs" Inherits="Page_CA301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Transfer" TypeName="PX.Objects.CA.CashTransferEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" StartNewGroup="True" Name="release" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewDoc" PopupCommand="Refresh" PopupCommandTarget="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Transfer" Caption="Transfer summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" 
		ActivityField="NoteActivity" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edTransferNbr" AllowCollapse="true" MarkRequired="Dynamic" >
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edTransferNbr" runat="server" DataField="TransferNbr" TabIndex="1"/>
			<px:PXDropDown Size="s" ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ControlSize="s" LabelsWidth="s" StartColumn="True" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Size="L" TabIndex="2"/>
			<px:PXNumberEdit ID="edRGOLAmt" runat="server" DataField="RGOLAmt" Enabled="False" />
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartRow="True" />
			<px:PXPanel ID="PXPanelDebit" runat="server" Caption="Source Account" Width="100%" RenderStyle="Fieldset">
				<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask CommitChanges="True" ID="edOutAccountID" runat="server" DataField="OutAccountID" TabIndex="3" />
				<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edOutDate" runat="server" DataField="OutDate" TabIndex="4"/>
				<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="OutPeriodID" TabIndex="5"/>
				<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkClearedOut" runat="server" DataField="ClearedOut" />
				<px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
				<px:PXTextEdit CommitChanges="True" ID="edOutExtRefNbr" runat="server" DataField="OutExtRefNbr" TabIndex="6"/>
				<pxa:PXCurrencyRate DataField="OutCuryID" RateField="OutCuryRate" ID="edCuryOut" runat="server" RateTypeView="CurrencyInfoOut" DataMember="_Currency_"></pxa:PXCurrencyRate>
				<px:PXNumberEdit CommitChanges="True" ID="edCuryTranOut" runat="server" DataField="CuryTranOut" TabIndex="7"/>
				<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
				<px:PXSelector ID="edBatchNbrOut" runat="server" DataField="TranIDOut_CATran_batchNbr" AllowEdit="True" Enabled="False" />
				<px:PXDateTimeEdit ID="edClearDateOut" runat="server" DataField="ClearDateOut" />
				<px:PXNumberEdit ID="edGLBalanceOut" runat="server" DataField="OutGLBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCashBalanceOut" runat="server" DataField="CashBalanceOut" Enabled="False" />
				<px:PXNumberEdit ID="edTranOut" runat="server" DataField="TranOut" Enabled="False" />
			</px:PXPanel>
			<px:PXPanel ID="PXPanelCredit" runat="server" Caption="Destination Account" Width="100%" RenderStyle="Fieldset">
				<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask CommitChanges="True" ID="edInAccountID" runat="server" DataField="InAccountID" TabIndex="8"/>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="True" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edInDate" runat="server" DataField="InDate" TabIndex="9"/>
                
            <px:PXSelector CommitChanges="True" ID="PXSelector1" runat="server" DataField="InPeriodID" />
				<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkClearedIn" runat="server" DataField="ClearedIn" />
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
				<px:PXTextEdit ID="edInExtRefNbr" runat="server" DataField="InExtRefNbr" TabIndex="10"/>
				<pxa:PXCurrencyRate DataField="InCuryID" RateField="CuryRate" ID="RateType2" runat="server" RateTypeView="CurrencyInfo" DataMember="_Currency_"></pxa:PXCurrencyRate>
				<px:PXNumberEdit CommitChanges="True" ID="edCuryTranIn" runat="server" DataField="CuryTranIn" TabIndex="11"/>
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
				<px:PXSelector ID="edBatchNbrIn" runat="server" DataField="TranIDIn_CATran_batchNbr" AllowEdit="True" Enabled="False" />
				<px:PXDateTimeEdit ID="PXDateTimeEdit1" runat="server" DataField="ClearDateIn" />
				<px:PXNumberEdit ID="edGLBalanceIn" runat="server" DataField="InGLBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCashBalanceIn" runat="server" DataField="CashBalanceIn" Enabled="False" />
				<px:PXNumberEdit ID="edTranIn" runat="server" DataField="TranIn" Enabled="False" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" ActionsPosition="Top" Caption="Additional Charges" SkinID="Details" MarkRequired="Dynamic" SyncPosition="true">
		<Mode InitNewRow="True" />
		<ActionBar DefaultAction="viewDoc">
		    <Actions>
		        <AddNew Tooltip="Add Expense" />
		    </Actions>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Expenses">
                <RowTemplate>>
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True"/>
                </RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="CashAccountID" CommitChanges="true"/>
					<px:PXGridColumn DataField="EntryTypeID" CommitChanges="true"/>
					<px:PXGridColumn DataField="TranDesc" />
					<px:PXGridColumn DataField="CuryTranAmt" CommitChanges="true"/>
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="AdjCuryRate" CommitChanges="true"/>
					<px:PXGridColumn DataField="ExtRefNbr" />
					<px:PXGridColumn DataField="AccountID" CommitChanges="true"/>
					<px:PXGridColumn DataField="SubID" CommitChanges="true"/>
					<px:PXGridColumn DataField="TranDate" CommitChanges="true"/>
					<px:PXGridColumn DataField="FinPeriodID" CommitChanges="true"/>
					<px:PXGridColumn DataField="Cleared" TextAlign="Center" Type="CheckBox" CommitChanges="true"/>
					<px:PXGridColumn DataField="batchNbr" LinkCommand="viewExpenseBatch" AllowShowHide="true"/>
					<px:PXGridColumn DataField="AdjRefNbr" LinkCommand="viewDoc" AllowShowHide="Server" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>