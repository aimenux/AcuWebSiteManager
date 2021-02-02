<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA202000.aspx.cs" Inherits="Page_CA202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
				<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" SkinID="Transparent" BorderStyle="NotSet" Visible="True" Width="100%" PrimaryView="CashAccount" TypeName="PX.Objects.CA.CashAccountMaint">
					<CallbackCommands>
						<px:PXDSCallbackCommand Visible="false" Name="ViewPTInstance" DependOnGrid="grdPTInstances" StartNewGroup="true" />
						<px:PXDSCallbackCommand Visible="false" Name="AddPTInstance" />
					</CallbackCommands>
				</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="smpPTInstance" runat="server" Key="PTInstanceEditor" InnerPageUrl="~/Pages/CA/CA206000.aspx?PopupPanel=On" CaptionVisible="True" Caption="Card Definition" RenderIFrame="True" Visible="False">
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="btnOK">
		<px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
			DataMember="ChangeIDDialog">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Target="formChangeID" Command="Save" />
			</px:PXButton>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" Width="100%"
		Caption="Cash Account Summary" DataMember="CashAccount" NoteIndicator="True"
		FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		TemplateContainer="" TabIndex="1100" DataSourceID="ds">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			    <px:PXSegmentMask ID="CashAccountCD" runat="server" DataField="CashAccountCD" DisplayMode="Hint"
				    DataSourceID="ds"/>
                <px:PXCheckBox ID="chkActiveAccount" runat="server" Checked="True" DataField="Active" />
			    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID"
				    AutoRefresh="True" DisplayMode="Hint" DataSourceID="ds"  CommitChanges="True"/>
			    <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server"
				    DataField="SubID" AutoRefresh="True" DataSourceID="ds" />
			    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server"
				    DataField="BranchID" DataSourceID="ds" />
			    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False"
				    DataSourceID="ds" />
			    <px:PXSelector ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID"
				    AllowEdit="True" DataSourceID="ds" edit="1" />
			    <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr"/>

			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			    <px:PXCheckBox CommitChanges="True" ID="chkClearingAccount" runat="server" Checked="True" DataField="ClearingAccount" />
			    <px:PXCheckBox ID="chkReconcile" runat="server" Checked="True" DataField="Reconcile" CommitChanges="True" />
			    <px:PXCheckBox ID="chkRestrictVisibility" runat="server" Checked="True" DataField="RestrictVisibilityWithBranch" />
			    <px:PXCheckBox ID="chkMatchToBatch" runat="server" Checked="True" DataField="MatchToBatch" />
				<px:PXCheckBox runat="server" ID="CstPXCheckBox1" DataField="UseForCorpCard" />
			    <px:PXSelector ID="edReconNumberingID" runat="server"
				    DataField="ReconNumberingID" AllowEdit="True" DataSourceID="ds" edit="1" />
			    <px:PXSelector ID="edReferenceID" runat="server" DataField="ReferenceID"
				    AllowEdit="True" DataSourceID="ds" edit="1" />
			    <px:PXSelector ID="edStatementImportTypeName" runat="server"
				    DataField="StatementImportTypeName" DataSourceID="ds"/>
			    <px:PXCheckBox ID="chkAcctSettingsAllowed" runat="server" DataField="AcctSettingsAllowed" Enabled="False" />
			    <px:PXCheckBox ID="chkPTInstancesAllowed" runat="server" DataField="PTInstancesAllowed" Enabled="False" />

		</Template>
		<AutoSize MinWidth="200" />
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Width="100%" LoadOnDemand="True" DataMember="CurrentCashAccount" BorderStyle="None" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Payment Methods" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="grid" runat="server" Height="350px" Width="100%" AllowFilter="False" SkinID="DetailsInTab" DataSourceID="ds">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="Details" DataKeyNames="PaymentMethodID,CashAccountID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="PaymentMethodID" AutoCallBack="True" />
									<px:PXGridColumn AutoCallBack="True" DataField="UseForAP" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AutoCallBack="True" DataField="UseForPR" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="APIsDefault" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="APAutoNextNbr" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="APLastRefNbr" />
									<px:PXGridColumn DataField="APBatchLastRefNbr" />
									<px:PXGridColumn AutoCallBack="True" DataField="UseForAR" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="ARIsDefault" TextAlign="Center" Type="CheckBox"/>
									<px:PXGridColumn DataField="ARIsDefaultForRefund" TextAlign="Center" Type="CheckBox"/>
									<px:PXGridColumn DataField="ARAutoNextNbr" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="ARLastRefNbr" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Clearing Accounts" BindingContext="form" VisibleExp="DataControls[&quot;chkClearingAccount&quot;].Value == 0">
				<Template>
					<px:PXGrid ID="gridDepositAccount" runat="server" Height="300px" Width="100%" ActionsPosition="Top" AllowFilter="False" SkinID="DetailsInTab" DataSourceID="ds" TabIndex="14900">
						<Levels>
							<px:PXGridLevel DataMember="Deposits" DataKeyNames="AccountID,DepositAcctID,PaymentMethodID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSegmentMask ID="edDepositAcctID" runat="server" DataField="DepositAcctID" AutoRefresh="True" />
									<px:PXSelector ID="edPaymentMethodID1" runat="server" DataField="PaymentMethodID" />
									<px:PXSelector ID="edChargeEntryTypeID" runat="server" DataField="ChargeEntryTypeID" AutoRefresh="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="DepositAcctID" />
									<px:PXGridColumn DataField="PaymentMethodID" />
									<px:PXGridColumn DataField="ChargeEntryTypeID"  />
									<px:PXGridColumn DataField="ChargeRate" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Entry Types">
				<Template>
					<px:PXGrid ID="grid2" runat="server" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" ActionsPosition="Top" AllowFilter="False" BorderStyle="None" BorderWidth="0px"
						SkinID="Details" DataSourceID="ds">
						<Levels>
							<px:PXGridLevel DataMember="ETDetails" DataKeyNames="AccountID,EntryTypeID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSelector ID="edEntryTypeID" runat="server" DataField="EntryTypeID" AllowEdit="True" />
									<px:PXSegmentMask ID="edOffsetAccountID" runat="server" DataField="OffsetAccountID" />
									<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
									<px:PXDropDown ID="edCAEntryType__Module" runat="server" DataField="CAEntryType__Module" Enabled="False" />
									<px:PXSegmentMask ID="edOffsetSubID" runat="server" DataField="OffsetSubID" />
									<px:PXSegmentMask ID="edCAEntryType__AccountID" runat="server" DataField="CAEntryType__AccountID" Enabled="False" AllowEdit="False"/>
									<px:PXSegmentMask ID="edCAEntryType__SubID" runat="server" DataField="CAEntryType__SubID" Enabled="False"  AllowEdit="False"/>
									<px:PXSelector ID="edCAEntryType__ReferenceID" runat="server" DataField="CAEntryType__ReferenceID" Enabled="False" AllowEdit="False" />
									<px:PXDropDown ID="edCAEntryType__DrCr" runat="server" DataField="CAEntryType__DrCr" Enabled="False" AllowEdit="False"/>
									<px:PXTextEdit Size="xl" ID="edCAEntryType__Descr" runat="server" DataField="CAEntryType__Descr" Enabled="False" />
									<px:PXSelector ID="edOffsetCashAccountID" runat="server" DataField="OffsetCashAccountID"  AutoRefresh="true"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowShowHide="False" DataField="AccountID" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="EntryTypeID" AutoCallBack="True" />
									<px:PXGridColumn DataField="CAEntryType__DrCr" Type="DropDownList" />
									<px:PXGridColumn DataField="CAEntryType__Module" />
									<px:PXGridColumn DataField="CAEntryType__BranchID" />
									<px:PXGridColumn DataField="CAEntryType__AccountID" />
									<px:PXGridColumn DataField="CAEntryType__SubID" />
									<px:PXGridColumn DataField="CAEntryType__CashAccountID" />
									<px:PXGridColumn DataField="CAEntryType__ReferenceID" />
									<px:PXGridColumn DataField="CAEntryType__Descr" />
									<px:PXGridColumn DataField="CAEntryType__UseToReclassifyPayments" Type="CheckBox" TextAlign="Center"/>
									<px:PXGridColumn AutoCallBack="True" DataField="OffsetCashAccountID"  />
									<px:PXGridColumn DataField="OffsetBranchID" />
									<px:PXGridColumn DataField="OffsetAccountID" CommitChanges="True"/>
									<px:PXGridColumn DataField="OffsetSubID" />
									<px:PXGridColumn AutoCallBack="True" DataField="TaxZoneID" />
									<px:PXGridColumn DataField="TaxCalcMode" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
                        <Mode AllowUpload="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Remittance Settings" BindingContext="form" VisibleExp="DataControls[&quot;chkAcctSettingsAllowed&quot;].Value = 1">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" Height="500px">
						<AutoSize Enabled="True" />
						<Template1>
							<px:PXGrid ID="gridPaymentMethodForRemittance" runat="server" DataSourceID="ds" Width="100%" Caption="Payment Method"
								SkinID="DetailsWithFilter" Height="180px" SyncPosition="True" AutoAdjustColumns="true">
								<AutoCallBack Target="grdPaymentDetails" Command="Refresh" />
								<Levels>
									<px:PXGridLevel DataMember="PaymentMethodForRemittance" DataKeyNames="AccountID,PaymentMethodID">
										<Columns>
											<px:PXGridColumn DataField="PaymentMethodID" TextAlign="Left" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinWidth="120" />
								<ActionBar ActionsVisible="False">
									<Actions>
										<AddNew Enabled="False" />
										<Delete Enabled="False" />
									</Actions>
								</ActionBar>
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="grdPaymentDetails" runat="server" Caption="Remittance Details" MatrixMode="True" SkinID="DetailsWithFilter" DataSourceID="ds" Width="100%">
								<CallbackCommands>
									<Refresh SelectControlsIDs="gridPaymentMethodForRemittance" />
								</CallbackCommands>
								<Levels>
									<px:PXGridLevel DataMember="PaymentDetails" DataKeyNames="AccountID,PaymentMethodID,DetailID">
										<Columns>
											<px:PXGridColumn DataField="PaymentMethodID" />
											<px:PXGridColumn DataField="DetailID" />
											<px:PXGridColumn DataField="PaymentMethodDetail__descr" />
											<px:PXGridColumn AllowShowHide="False" DataField="DetailValue" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinHeight="100" />
								<ActionBar ActionsVisible="False">
									<Actions>
										<AddNew Enabled="False" />
										<Delete Enabled="False" />
									</Actions>
								</ActionBar>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem BindingContext="form" Text="Signature">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" LabelsWidth="S"/>
					<px:PXTextEdit ID="edSignatureDescr" runat="server" DataField="SignatureDescr"/>
					<px:PXImageUploader ID="edSignature" runat="server" DataField="Signature" Height="300px" Width="400px" AllowUpload="true" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>
