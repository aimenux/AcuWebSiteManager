<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR201000.aspx.cs" Inherits="Page_AP201000"
	Title="Untitled Page" EnableSessionState="True" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CustomerClassRecord"
		TypeName="PX.Objects.AR.CustomerClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="ResetGroup" StartNewGroup="true" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Customer Class Summary"
		NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		DataMember="CustomerClassRecord" DefaultControlID="edCustomerClassID" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edCustomerClassID" runat="server" DataField="CustomerClassID" AutoRefresh="True" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="600px" Width="100%" DataMember="CurCustomerClassRecord"
		ActivityField="NoteActivity" RepaintOnDemand="False" DataSourceID="ds" >
		<Items>
			<px:PXTabItem Text="General Settings" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXLayoutRule runat="server" GroupCaption="Default General Settings" StartGroup="True" />
					<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" />
					<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
					<px:PXCheckBox ID="chkRequireTaxZone" runat="server" DataField="RequireTaxZone" />
					<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" />
					<px:PXDropDown ID="edAvalaraCustomerUsageType" runat="server" DataField="AvalaraCustomerUsageType" />
					<px:PXCheckBox ID="chkAvalaraCustomerUsageType" runat="server" DataField="RequireAvalaraCustomerUsageType" />
					<px:PXCheckBox ID="chkDefaultLocationCDFromBranch" runat="server" DataField="DefaultLocationCDFromBranch" />
					<px:PXSelector runat="server" ID="edLocale" DataField="CustomerClassRecord.LocaleName" DisplayMode="Text" />
					<px:PXSelector ID="edGroupMask" runat="server" DataField="GroupMask" />
					<px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
					<px:PXLayoutRule runat="server" GroupCaption="Default Delivery Settings" StartGroup="True" />
					<px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia" />
					<px:PXSelector ID="edShipTermsID" runat="server" DataField="ShipTermsID" />
					<px:PXDropDown ID="edShipComplete" runat="server" DataField="ShipComplete" />
					<px:PXLayoutRule runat="server" GroupCaption="Default Credit Verification Settings"
						StartGroup="True" />
					<px:PXDropDown CommitChanges="True" ID="edCreditRule" runat="server" DataField="CreditRule" />
					<px:PXNumberEdit ID="edCreditLimit" runat="server" DataField="CreditLimit" />
					<px:PXNumberEdit ID="edOverLimitAmount" runat="server" DataField="OverLimitAmount" />
					<px:PXNumberEdit ID="edCreditDaysPastDue" runat="server" DataField="CreditDaysPastDue" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Service Management Settings" />
					<px:PXSelector runat="server" ID="edDfltBillingCycleID" DataField="DfltBillingCycleID" CommitChanges="True" />
					<px:PXDropDown runat="server" ID="edSendInvoicesTo" DataField="SendInvoicesTo" />
					<px:PXDropDown runat="server" ID="edBillShipmentSource" DataField="BillShipmentSource" />
					<px:PXDropDown runat="server" ID="edDefaultBillingCustomerSource" DataField="DefaultBillingCustomerSource" CommitChanges="True" />
					<px:PXSegmentMask runat="server" ID="edBillCustomerID" DataField="BillCustomerID" CommitChanges="True" AutoRefresh="True" />
					<px:PXSegmentMask runat="server" ID="edBillLocationID" DataField="BillLocationID" AutoRefresh="True" />
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
					<px:PXLayoutRule runat="server" GroupCaption="Default Financial Settings" StartGroup="True" />
					<px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" />
					<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" />
					<px:PXSelector ID="edDefPaymentMethodID" runat="server" DataField="DefPaymentMethodID" />
					<px:PXCheckBox ID="chkAutoApplyPayments" runat="server" DataField="AutoApplyPayments" />
					<px:PXCheckBox ID="chkFinChargeApply" runat="server" CommitChanges="True" DataField="FinChargeApply">
					</px:PXCheckBox>
					<px:PXSelector ID="edFinChargeID" runat="server" CommitChanges="True" DataField="FinChargeID">
					</px:PXSelector>
					<px:PXCheckBox ID="chkSmallBalanceAllow" runat="server" CommitChanges="True" DataField="SmallBalanceAllow">
					</px:PXCheckBox>
					<px:PXNumberEdit ID="edSmallBalanceLimit" runat="server" DataField="SmallBalanceLimit">
					</px:PXNumberEdit>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="True" />
					<px:PXSelector Size="xs" ID="edCuryID" runat="server" DataField="CuryID" />
					<px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
					<px:PXSelector Size="xs" ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" />
					<px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
					<px:PXNumberEdit ID="edDiscountLimit" runat="server" DataField="DiscountLimit" />
					<px:PXCheckBox ID="chkPaymentsByLinesAllowed" runat="server" DataField="PaymentsByLinesAllowed" />
					<px:PXCheckBox ID="chkRetainageApply" runat="server" DataField="RetainageApply" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />

					<px:PXLayoutRule runat="server" GroupCaption="Default Print and Email Settings" StartGroup="True" />
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" Merge="True" SuppressLabel="true" />
					<px:PXCheckBox ID="chkMailInvoices" runat="server" DataField="MailInvoices" Size="M"/>
					<px:PXCheckBox CommitChanges="True" ID="chkPrintInvoices" runat="server" DataField="PrintInvoices"  Size="M"/>
					<px:PXLayoutRule runat="server" Merge="False" />
					<px:PXLayoutRule runat="server" ControlSize="M"  LabelsWidth="M" Merge="True" SuppressLabel="true" />
					<px:PXCheckBox ID="chkMailDunningLetters" runat="server" DataField="MailDunningLetters"  Size="M"/>
					<px:PXCheckBox ID="chkPrintDunningLetters" runat="server" DataField="PrintDunningLetters" Size="M" />
					<px:PXLayoutRule runat="server" Merge="False" />
					<px:PXLayoutRule runat="server" ControlSize="M"  LabelsWidth="M" Merge="True" SuppressLabel="true" />
					<px:PXCheckBox ID="chkSendStatementByEmail" runat="server" DataField="SendStatementByEmail" Size="M" CommitChanges="true"/>
					<px:PXCheckBox ID="chkPrintStatements" runat="server" DataField="PrintStatements" Size="M" CommitChanges="true"/>
					<px:PXLayoutRule runat="server" Merge="False" />
					<px:PXDropDown CommitChanges="True" ID="edStatementType" runat="server" DataField="StatementType" />
					<px:PXCheckBox ID="chkPrintCuryStatements" runat="server" DataField="PrintCuryStatements" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSegmentMask ID="edARAcctID" runat="server" DataField="ARAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edARSubID" runat="server" DataField="ARSubID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.aRAcctID" PropertyName="DataControls[&quot;edARAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.salesAcctID" PropertyName="DataControls[&quot;edSalesAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>

					<px:PXSegmentMask ID="edDiscountAcctID" runat="server" DataField="DiscountAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edDiscountSubID" runat="server" DataField="DiscountSubID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.DiscountSubID" PropertyName="DataControls[&quot;edDiscountAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>

					<px:PXSegmentMask ID="edFreightAcctID" runat="server" DataField="FreightAcctID" CommitChanges="True">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edFreightSubID" runat="server" DataField="FreightSubID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.freightAcctID" PropertyName="DataControls[&quot;edFreightAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edDiscTakenAcctID" runat="server" DataField="DiscTakenAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edDiscTakenSubID" runat="server" DataField="DiscTakenSubID"
						AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.discTakenAcctID" PropertyName="DataControls[&quot;edDiscTakenAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID" CommitChanges="True">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID"
						AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.prepaymentAcctID" PropertyName="DataControls[&quot;edPrepaymentAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edCOGSAcctID" runat="server" DataField="COGSAcctID" CommitChanges="True">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edCOGSSubID" runat="server" DataField="COGSSubID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.cOGSAcctID" PropertyName="DataControls[&quot;edCOGSAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edMiscAcctID" runat="server" DataField="MiscAcctID" CommitChanges="True">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edMiscSubID" runat="server" DataField="MiscSubID" AutoRefresh="True">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.miscAcctID" PropertyName="DataControls[&quot;edMiscAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedGainAcctID" runat="server" DataField="UnrealizedGainAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedGainSubID" runat="server" DataField="UnrealizedGainSubID" DataSourceID="ds">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.unrealizedGainAcctID" PropertyName="DataControls[&quot;edUnrealizedGainAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedLossAcctID" runat="server" DataField="UnrealizedLossAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedLossSubID" runat="server" DataField="UnrealizedLossSubID" DataSourceID="ds">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="CustomerClass.unrealizedLossAcctID" PropertyName="DataControls[&quot;edUnrealizedLossAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
					</px:PXSegmentMask>

					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="RetainageAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edRetainageSubID" runat="server" DataField="RetainageSubID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
						border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AttributeID" AutoCallBack="true" LinkCommand="CRAttribute_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" SortDirection="Ascending" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
									<px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Mailing Settings">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
						<AutoSize Enabled="true" />
						<Template1>
								<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px"
									Caption="Mailings" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" >
									<AutoCallBack Target="gridNR" Command="Refresh" />
									<AutoSize Enabled="True" />
									<Levels>
										<px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
											<RowTemplate>
												<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
												<px:PXDropDown ID="edFormat" runat="server" DataField="Format"/>
												<px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID"/> 
												<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active"/>
												<px:PXSelector ID="edSetupID" runat="server" DataField="SetupID"/>
												<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID"/>
												<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name"/> 
												<px:PXSelector Size="s" ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text"/>
												
											</RowTemplate>
											<Columns>
												<px:PXGridColumn DataField="SetupID" AutoCallBack="True"/> 
												<px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch"/>
												<px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text"/>
												<px:PXGridColumn DataField="ReportID" AutoCallBack="True"/>
												<px:PXGridColumn DataField="NotificationID" AutoCallBack="True"/>	
												<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True"/> 
												<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"/>
												
											</Columns>
											<Layout FormViewHeight="" />
										</px:PXGridLevel>
									</Levels>
								</px:PXGrid>
						</Template1>
						<Template2>
								<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients"
									AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" >
									<Parameters>
										<px:PXSyncGridParam ControlID="gridNS" />
									</Parameters>
									<CallbackCommands>
										<Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds" />
										<FetchRow RepaintControls="None" />
									</CallbackCommands>
									<Levels>
										<px:PXGridLevel DataMember="NotificationRecipients" DataKeyNames="NotificationID">
											<Columns>
												<px:PXGridColumn DataField="ContactType" Width="100px" AutoCallBack="True">
												</px:PXGridColumn>
												<px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
												<px:PXGridColumn DataField="ContactID" Width="250px">
													<NavigateParams>
														<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
													</NavigateParams>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Format" Width="60px" AutoCallBack="True">
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px">
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px">
												</px:PXGridColumn>
											</Columns>
											<RowTemplate>
												<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
												<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True"
													ValueField="DisplayName" AllowEdit="True">
												</px:PXSelector>
											</RowTemplate>
											<Layout FormViewHeight="" />
										</px:PXGridLevel>
									</Levels>
									<AutoSize Enabled="True" />
								</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Service Billing" RepaintOnDemand="False" LoadOnDemand="True" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
				<Template>
					<px:PXGrid runat="server" ID="gridBillingCycles" SkinID="DetailsInTab" AutoGenerateColumns="None" DataSourceID="ds" KeepPosition="True" RepaintColumns="True" SyncPosition="True" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="BillingCycles">
								<Columns>
									<px:PXGridColumn DataField="SrvOrdType" CommitChanges="True" />
									<px:PXGridColumn DataField="BillingCycleID" CommitChanges="True" />
									<px:PXGridColumn DataField="SendInvoicesTo" CommitChanges="True" />
									<px:PXGridColumn DataField="BillShipmentSource" />
									<px:PXGridColumn DataField="FrequencyType" CommitChanges="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="600" MinWidth="100" />
	</px:PXTab>
</asp:Content>
