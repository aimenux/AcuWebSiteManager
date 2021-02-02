<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA305000.aspx.cs" Inherits="Page_CA305000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds"  EnableAttributes="true" runat="server" Visible="True" Width="100%" PrimaryView="Document" TypeName="PX.Objects.CA.CADepositEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="true" Name="Release" />
			<px:PXDSCallbackCommand Name="VoidDocument" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="AddPayment" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ViewBatch" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDocument" DependOnGrid="grid"  Visible="false" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
		</CallbackCommands>
	</px:PXDataSource>

	<px:PXSmartPanel ID="PanelAddPayment" runat="server" Key="AvailablePayments" Caption="Add Payment to Deposit" CaptionVisible="True" LoadOnDemand="True" AutoCallBack-Command="Refresh" 
		AutoCallBack-Target="frmPaymentFilter1" Width = "900px" Height = "500px" DesignView="Content">
		<px:PXFormView ID="frmPaymentFilter1" runat="server" Caption="Payment Selection" CaptionVisible="False" DataMember="filter" DataSourceID="ds" SkinID="Transparent" TabIndex="2900">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule111" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
				<px:PXSegmentMask ID="edCashAccountID" runat="server" CommitChanges="True" DataField="CashAccountID" DataSourceID="ds" />
				<px:PXSelector ID="edPaymentMethodID" runat="server" CommitChanges="True" DataField="PaymentMethodID" DataSourceID="ds"/>
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="s" LabelsWidth="sm" StartColumn="True" />
				<px:PXDateTimeEdit ID="edStartDate" runat="server" CommitChanges="True" DataField="StartDate" />
				<px:PXDateTimeEdit ID="edEndDate" runat="server" CommitChanges="True" DataField="EndDate" />
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="gridOL1" runat="server" AllowPaging="True" BatchUpdate="True" DataSourceID="ds" SkinID="Details" TabIndex="3100" Width="100%">
			<Levels>
				<px:PXGridLevel DataKeyNames="DocType,RefNbr" DataMember="AvailablePayments">
					<Columns>
						<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
						<px:PXGridColumn DataField="Module"/>
						<px:PXGridColumn DataField="DocType" MatrixMode="True" />
						<px:PXGridColumn DataField="RefNbr" />
						<px:PXGridColumn DataField="BAccountID" />
						<px:PXGridColumn DataField="BAccountID_BAccountR_acctName" />
						<px:PXGridColumn DataField="LocationID" />
						<px:PXGridColumn DataField="ExtRefNbr" />
						<px:PXGridColumn AllowUpdate="False" DataField="DocDate" />
						<px:PXGridColumn AllowUpdate="False" DataField="DepositAfter" />
						<px:PXGridColumn DataField="CuryID" />
						<px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" />
						<px:PXGridColumn DataField="CashAccountID" />
						<px:PXGridColumn DataField="CashAccountID_CashAccount_Descr" />
						<px:PXGridColumn DataField="PaymentMethodID" TextAlign="Left" />
						<px:PXGridColumn DataField="PMInstanceID" TextAlign="Left" DisplayMode="Text"/>
						<px:PXGridColumn DataField="CuryChargeTotal" TextAlign="Right" />
						<px:PXGridColumn DataField="CuryGrossPaymentAmount" TextAlign="Right" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" MinHeight="300" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel111" runat="server" SkinID="Buttons">
			<px:PXButton ID="OK1" runat="server" DialogResult="OK" Text="Add & Close" />
			<px:PXButton ID="Cancel1" runat="server" DialogResult="No" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Document" Caption="Deposit Summary" NoteIndicator="True" FilesIndicator="True"
		LinkIndicator="True" NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="900"
		MarkRequired="Dynamic">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="s" ControlSize="XM" />
			<px:PXDropDown ID="edTranType" runat="server" AllowNull="False" DataField="TranType" Size="S"/>
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" DataSourceID="ds" Size="S"/>
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" DataSourceID="ds"/>
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_CADeposit_CurrencyInfo_" DataMember="_Currency_"></pxa:PXCurrencyRate>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" AllowNull="False" Size="S"/>
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" Size="S"/>
			<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate" Size="S"/>
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" AutoRefresh="True" Size="S"/>
			<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
			<px:PXSegmentMask CommitChanges="True" ID="edExtraCashAccountID" runat="server" DataField="ExtraCashAccountID" AutoRefresh="True" DataSourceID="ds" />
			<px:PXNumberEdit Size="S" CommitChanges="True" ID="edCuryExtraCashTotal" runat="server" DataField="CuryExtraCashTotal" />
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
			<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="s" ControlSize="s" />
			<px:PXNumberEdit ID="edCuryDetailTotal" runat="server" DataField="CuryDetailTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryChargeTotal" runat="server" DataField="CuryChargeTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryControlAmt" runat="server" DataField="CuryControlAmt" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="201px" Width="100%">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Payments">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" BorderWidth="0px"
						SkinID="Details" AddCommandName="AddPayment">
						<Levels>
							<px:PXGridLevel DataMember="DepositPayments">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
									<px:PXDropDown ID="edOrigDocType" runat="server" DataField="PaymentInfo__DocType" />
									<px:PXSegmentMask ID="edPaymentInfo__BAccountID" runat="server" DataField="PaymentInfo__BAccountID" />
									<px:PXSegmentMask ID="edPaymentInfo__LocationID" runat="server" DataField="PaymentInfo__LocationID" />
									<px:PXSegmentMask ID="edAccountID1" runat="server" DataField="AccountID" />
									<px:PXSelector ID="edOrigCuryID" runat="server" DataField="OrigCuryID" Enabled="False" />
									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" AllowNull="False" DataField="CuryTranAmt" />
									<px:PXTextEdit ID="edPaymentInfo__PaymentMethodID" runat="server" DataField="PaymentInfo__PaymentMethodID" Enabled="False" />
									<px:PXDropDown ID="edPaymentInfo__Status" runat="server" AllowNull="False" DataField="PaymentInfo__Status" Enabled="False" />
									<px:PXTextEdit ID="edPaymentInfo__ExtRefNbr" runat="server" DataField="PaymentInfo__ExtRefNbr" />
									<px:PXDateTimeEdit ID="edPaymentInfo__DocDate" runat="server" DataField="PaymentInfo__DocDate" Enabled="False" />
									<px:PXSelector ID="edChargeEntryTypeID" runat="server" DataField="ChargeEntryTypeID" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="OrigModule"/>
									<px:PXGridColumn DataField="PaymentInfo__DocType" MatrixMode="True"/>
									<px:PXGridColumn DataField="OrigRefNbr" Width="108px" LinkCommand="ViewDocument" >
										<NavigateParams>
											<px:PXControlParam ControlID="grid" Name="DocType" PropertyName="DataValues[&quot;OrigDocType&quot;]" Type="String" />
											<px:PXControlParam ControlID="grid" Name="RefNbr" PropertyName="DataValues[&quot;OrigRefNbr&quot;]" Type="String" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="PaymentInfo__BAccountID"/>
									<px:PXGridColumn DataField="PaymentInfo__BAccountID_Description" />
									<px:PXGridColumn DataField="PaymentInfo__LocationID" />
									<px:PXGridColumn DataField="AccountID"  />
									<px:PXGridColumn DataField="PaymentInfo__CuryID"  />
									<px:PXGridColumn DataField="CuryTranAmt" AllowNull="False" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryOrigAmt" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="PaymentInfo__PaymentMethodID" />
									<px:PXGridColumn AllowUpdate="False" DataField="PaymentInfo__Status" Type="DropDownList" />
									<px:PXGridColumn DataField="PaymentInfo__ExtRefNbr" />
									<px:PXGridColumn AllowUpdate="False" DataField="PaymentInfo__DocDate" />
									<px:PXGridColumn AllowUpdate="False" DataField="PaymentInfo__DepositAfter" />
									<px:PXGridColumn DataField="ChargeEntryTypeID"  />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode InitNewRow="True" AllowFormEdit="True" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Add AR Payment" Key="cmdPO" CommandSourceID="ds" CommandName="AddPayment">
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Charges">
				<Template>
					<px:PXGrid ID="gridCharges" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" BorderWidth="0px" SkinID="Details">
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
								<Save Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Charges">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
									<px:PXDropDown ID="edTranType" runat="server" DataField="TranType" Enabled="False" />
									<px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" />
									<px:PXSelector ID="edEntryTypeID" runat="server" DataField="EntryTypeID" CommitChanges="true"/>
									<px:PXSegmentMask ID="edDepositAcctID" runat="server" DataField="DepositAcctID" />
									<px:PXSelector ID="edPaymentMethodID" runat="server" AllowNull="False" DataField="PaymentMethodID" />
									<px:PXNumberEdit ID="edChargeRate" runat="server" AllowNull="False" DataField="ChargeRate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryChargeableAmt" runat="server" AllowNull="False" DataField="CuryChargeableAmt" />
									<px:PXNumberEdit ID="edCuryChargeAmt" runat="server" AllowNull="False" DataField="CuryChargeAmt" />
									<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
									<px:PXDropDown ID="edDrCr" runat="server" AllowNull="False" DataField="DrCr" SelectedIndex="1" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="EntryTypeID" AutoCallBack ="true" />
									<px:PXGridColumn DataField="PaymentMethodID" AutoCallBack ="true" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ChargeRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryChargeableAmt" AllowNull="False" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryChargeAmt" AllowNull="False" TextAlign="Right" />
									<px:PXGridColumn DataField="AccountID" CommitChanges="true" />
									<px:PXGridColumn DataField="SubID"  />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXFormView ID="form2" runat="server" DataSourceID="ds" Width="100%" DataMember="DocumentCurrent" CaptionVisible="False" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="m" ControlSize="M" />
							<px:PXSelector ID="edBatchNbr" runat="server" DataField="TranID_CATran_batchNbr" Enabled="False" AllowEdit="True" />
							<px:PXSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" />
							<px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" />
							<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
							<px:PXDateTimeEdit Size="s" ID="edClearDate" runat="server" DataField="ClearDate" />
							<px:PXCheckBox ID="chkCleared" SuppressLabel="true" runat="server" DataField="Cleared" />
							<px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="False" />
							<px:PXCheckBox CommitChanges="True" ID="chkChargesSeparate" runat="server" DataField="ChargesSeparate" />
						</Template>
						<AutoSize Enabled="True" />
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
</asp:Content>
