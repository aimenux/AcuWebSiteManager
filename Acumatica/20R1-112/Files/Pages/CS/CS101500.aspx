<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS101500.aspx.cs" Inherits="Page_CS101500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">

	<script language="javascript" type="text/javascript">
		function commandResult(ds, context) {
			if (context.command == "Save" || context.command == "Delete") __refreshMainMenu();
		}
	</script>

	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.OrganizationMaint" PrimaryView="BAccount">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="True" CommitChanges="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefault" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="ViewLocation" Visible="False" />
			<px:PXDSCallbackCommand Name="NewLocation" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewMainOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDefLocationOnMap" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grdEmployees" Name="ViewContact" Visible="False" />
			<px:PXDSCallbackCommand Name="NewContact" Visible="False" />
		    <px:PXDSCallbackCommand Name="DeleteOrganizationLedgerLink" Visible="false"/>
		    <px:PXDSCallbackCommand Name="AddBranch" Visible="false"/>
		    <px:PXDSCallbackCommand Name="ViewBranch" Visible="false"/>
		    <px:PXDSCallbackCommand Name="AddLedger" Visible="false"/>
		    <px:PXDSCallbackCommand Name="ViewLedger" Visible="false"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="CreateLedger" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ChangeID" />
		</CallbackCommands>
		<ClientEvents CommandPerformed="commandResult" />
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="PXFormView1" runat="server" Width="100%" Caption="Company Summary" DataMember="BAccount" NoteIndicator="True" FilesIndicator="True" DefaultControlID="edAcctCD">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XL" />
			<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" />
			<px:PXTextEdit CommitChanges="True" ID="edAcctName" runat="server" DataField="AcctName" />
		    <px:PXDropDown CommitChanges="True" ID="edOrganizationType" runat="server" DataField="OrganizationView.OrganizationType" />
			<px:PXLayoutRule runat="server" StartColumn="True" ColumnWidth="XM" />
		    <px:PXCheckBox ID="chkActive" runat="server" DataField="OrganizationView.Active" CommitChanges="true" AlignLeft="True" />
		    <px:PXFormView ID="StateForm" runat="server" DataMember="StateView" DataSourceID="ds" Caption="Hidden Form needed for VisibleExp of TabItems" Visible="False">
		        <Template>
		            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
		            <px:PXCheckBox SuppressLabel="True" ID="chkIsBranchTabVisible" runat="server" DataField="IsBranchTabVisible" />
		            <px:PXCheckBox SuppressLabel="True" ID="chkIsDeliverySettingsTabVisible" runat="server" DataField="IsDeliverySettingsTabVisible" />
		            <px:PXCheckBox SuppressLabel="True" ID="chkIsGLAccountsTabVisible" runat="server" DataField="IsGLAccountsTabVisible" />
		            <px:PXCheckBox SuppressLabel="True" ID="chkIsRutRotTabVisible" runat="server" DataField="IsRutRotTabVisible" />
		        </Template>
		    </px:PXFormView>
		</Template>
	</px:PXFormView>
	<px:PXSmartPanel ID="pnlCreateLedger" runat="server" Caption="Create Ledger" CaptionVisible="True" Key="CreateLedgerView" LoadOnDemand="true"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="loform" AutoCallBack-Enabled="true"
        AcceptButtonID="cbOk" CancelButtonID="cbCancel">
		<px:PXFormView ID="loform" runat="server" DataMember="CreateLedgerView"  SkinID="Transparent">
			<ContentStyle BorderWidth="0px">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXMaskEdit  ID="edLedgerCD" runat="server" DataField="LedgerCD" CommitChanges="True"/>
				<px:PXTextEdit  ID="edLedgerDescr" runat="server" DataField="Descr" CommitChanges="True" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="cbOK" runat="server" Text="Create" CommandSourceID="ds" DialogResult="OK" />
			<px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
        <px:PXSmartPanel ID="pnlChangeID" runat="server"  Caption="Specify New ID"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog"  AutoCallBack-Enabled="true"
		AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh"
		AcceptButtonID="btnOK" CancelButtonID="btnCancel">
		<px:PXFormView ID="formChangeID" runat="server" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="ChangeIDDialog">
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXMaskEdit ID="edAcctCD" runat="server" DataField="CD" CommitChanges="True" />
                </Template>
            </px:PXFormView>
            <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" Text="OK" CommandSourceID="ds" DialogResult="OK" ></px:PXButton>
				<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />						
            </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="522px" Width="100%" DataMember="CurrentBAccount" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Company Details" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="DefContact" runat="server" Caption="Main Contact" DataMember="DefContact" RenderStyle="Fieldset" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True" />
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="DefAddress" runat="server" Caption="Main Address" DataMember="DefAddress" RenderStyle="Fieldset" DataSourceID="ds" SyncPosition="true">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" CommitChanges="True" DataField="CountryID" DataSourceID="ds" />
							<px:PXSelector ID="edState" runat="server" AllowEdit="True" AutoRefresh="True" DataField="State" CommitChanges="True" DataSourceID="ds" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" CommitChanges="True" />
							<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds" Size="xs" Text="View on Map" />
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" StartColumn="True" ControlSize="XM" />
				    <px:PXFormView ID="Company" runat="server" Caption="Base Currency Settings (Shared)" DataMember="Company" RenderStyle="Fieldset" DataSourceID="ds">
				        <Template>
				            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
				            <px:PXSelector CommitChanges="True" ID="edBaseCuryID" runat="server" DataField="BaseCuryID" DataSourceID="ds" />
				        </Template>
				    </px:PXFormView>
				    <px:PXFormView ID="CompanyCurrency" runat="server" DataMember="CompanyCurrency" RenderStyle="Simple" DataSourceID="ds">
				        <Template>
				            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
				            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
				            <px:PXTextEdit ID="edCurySymbol" runat="server" DataField="CurySymbol" />
				            <px:PXNumberEdit ID="edDecimalPlaces" runat="server" DataField="DecimalPlaces" CommitChanges="True" />
				        </Template>
				    </px:PXFormView>
					<px:PXLayoutRule runat="server" GroupCaption="Tax Registration Info" StartGroup="True" />
					<px:PXTextEdit CommitChanges="True" ID="edLegalName" runat="server" DataField="LegalName" />
					<px:PXTextEdit ID="TaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
					<px:PXFormView ID="PXFormViewLocation" runat="server" DataMember="IntLocations" RenderStyle="Simple" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="CAvalaraExemptionNumber" />
							<px:PXDropDown ID="edSalutation" runat="server" DataField="CAvalaraCustomerUsageType" />
						</Template>
					</px:PXFormView>
				    <px:PXCheckBox ID="edFileTaxesByBranches" runat="server" DataField="OrganizationView.FileTaxesByBranches" />
				    <px:PXCheckBox ID="edReporting1099ByBranches" runat="server" DataField="OrganizationView.Reporting1099ByBranches" CommitChanges="True" />
				    <px:PXCheckBox ID="edReporting1099" runat="server" DataField="OrganizationView.Reporting1099" CommitChanges="True" />
					<px:PXFormView ID="ConfigurationSettings" runat="server" DataMember="OrganizationView" RenderStyle="Simple" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Configuration Settings" ControlSize="XM" LabelsWidth="SM"/>
							<px:PXSelector ID="edRolename" runat="server" DataField="RoleName" CommitChanges="True"/>
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID" />
							<px:PXSelector ID="edDefaultPrinter" runat="server" DataField="DefaultPrinterID" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Miscellaneous Settings (Shared)" />
					<px:PXFormView ID="CommonSettings" runat="server" DataMember="commonsetup" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXNumberEdit ID="edDecPlQty" runat="server" DataField="DecPlQty" />
							<px:PXNumberEdit ID="edDecPlPrcCst" runat="server" DataField="DecPlPrcCst" />
							<px:PXSelector ID="edWeightUOM" runat="server" DataField="WeightUOM" AllowEdit="True" Size="S" />
							<px:PXSelector ID="edLinearUOM" runat="server" DataField="LinearUOM" AllowEdit="True" Size="S" />
							<px:PXSelector ID="edVolumeUOM" runat="server" DataField="VolumeUOM" AllowEdit="True" Size="S" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="MiscSettings" runat="server" DataMember="Company" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="PhoneMask" runat="server" DataField="PhoneMask" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		    <px:PXTabItem Text="Branches" BindingContext="StateForm" VisibleExp="DataControls[&quot;chkIsBranchTabVisible&quot;].Value == true">
		        <Template>
		            <px:PXGrid ID="grdBranches" runat="server" Height="300px" Width="100%" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds" SyncPosition="True">
		                <ActionBar>
		                    <Actions>
		                        <EditRecord ToolBarVisible="False" />
		                        <Delete ToolBarVisible="False"/>
		                        <AddNew ToolBarVisible="False"/>
		                    </Actions>
		                    <CustomItems>
		                        <px:PXToolBarButton Text="Add Branch">
		                            <AutoCallBack Command="AddBranch" Target="ds" />
		                        </px:PXToolBarButton>
		                    </CustomItems>
		                </ActionBar>
		                <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
		                <Levels>
		                    <px:PXGridLevel DataMember="BranchesView">
		                        <Columns>
		                            <px:PXGridColumn DataField="BranchCD" Width="150px" AutoCallBack="True" LinkCommand="ViewBranch"/>
		                            <px:PXGridColumn DataField="AcctName" Width="300px"/>
		                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60"/>
		                            <px:PXGridColumn DataField="RoleName" />
		                            <px:PXGridColumn DataField="Roles__Descr" Width="300px"/>
		                        </Columns>
		                    </px:PXGridLevel>
		                </Levels>
		            </px:PXGrid>
		        </Template>
		    </px:PXTabItem>
		    <px:PXTabItem Text="Delivery Settings" BindingContext="StateForm" VisibleExp="DataControls[&quot;chkIsDeliverySettingsTabVisible&quot;].Value == true">
		        <Template>
		            <px:PXFormView ID="frmDefLocation" runat="server" DataMember="DefLocation" Width="100%" CaptionVisible="False" TemplateContainer="" DataSourceID="ds" RenderStyle="Normal" SyncPosition="true">
		                <Template>
		                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Delivery Contact" StartGroup="True" />
		                    <px:PXCheckBox ID="chkIsContactSameAsMain" runat="server" CommitChanges="True" DataField="IsContactSameAsMain" SuppressLabel="True" />
		                    <px:PXFormView ID="DefLocationContact" runat="server" DataMember="DefLocationContact" RenderStyle="Simple" DataSourceID="ds">
		                        <Template>
		                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
		                            <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="FullName" />
		                            <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="Attention" />
		                            <px:PXMailEdit ID="PXMailEdit1" runat="server" DataField="EMail" CommitChanges="True" />
		                            <px:PXLinkEdit ID="PXLinkEdit1" runat="server" DataField="WebSite" CommitChanges="True" />
		                            <px:PXMaskEdit ID="PXMaskEdit1" runat="server" DataField="Phone1" />
		                            <px:PXMaskEdit ID="PXMaskEdit2" runat="server" DataField="Phone2" />
		                            <px:PXMaskEdit ID="PXMaskEdit3" runat="server" DataField="Fax" />
		                        </Template>
		                    </px:PXFormView>
		                    <px:PXLayoutRule runat="server" GroupCaption="Delivery Address" StartGroup="True" />
		                    <px:PXCheckBox ID="chkIsMain" runat="server" CommitChanges="True" DataField="IsAddressSameAsMain" SuppressLabel="True" />
		                    <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="IsValidated" Enabled="False" />
		                    <px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="AddressLine1" />
		                    <px:PXTextEdit ID="PXTextEdit4" runat="server" DataField="AddressLine2" />
		                    <px:PXTextEdit ID="PXTextEdit5" runat="server" DataField="City" />
		                    <px:PXSelector ID="PXSelector1" runat="server" AllowEdit="True" CommitChanges="True" DataField="CountryID" DataSourceID="ds" />
		                    <px:PXSelector ID="PXSelector2" runat="server" AllowEdit="True" AutoRefresh="True" DataField="State" DataSourceID="ds" />
		                    <px:PXLayoutRule runat="server" Merge="True" />
		                    <px:PXMaskEdit ID="PXMaskEdit4" runat="server" DataField="PostalCode" Size="s" CommitChanges="True" />
		                    <px:PXButton ID="btnViewDefLoactionOnMap" runat="server" CommandName="ViewDefLocationOnMap" CommandSourceID="ds" Size="xs" Text="View on Map" />
		                    <px:PXLayoutRule runat="server" />
		                    <px:PXLayoutRule runat="server" LabelsWidth="SM" StartColumn="True" ControlSize="XM" />
		                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Instructions" />
		                    <px:PXSelector ID="edVTaxZoneID" runat="server" DataField="VTaxZoneID" AllowEdit="True" DataSourceID="ds" />
		                    <px:PXDropDown ID="edCShipComplete" runat="server" DataField="CShipComplete" SelectedIndex="2" />
		                </Template>
		                <Activity HighlightColor="" SelectedColor="" />
		                <ContentStyle BackColor="Transparent" BorderStyle="None">
		                </ContentStyle>
		                <AutoSize Enabled="True" MinHeight="500" MinWidth="400" />
		            </px:PXFormView>
		        </Template>
		        <ContentLayout ColumnsWidth="XL" LabelsWidth="M" />
		    </px:PXTabItem>
		    <px:PXTabItem Text="Employees">
		        <Template>
		            <px:PXGrid ID="grdEmployees" runat="server" Height="150px" Width="100%" ActionsPosition="Top" AllowSearch="True" SkinID="Details" DataSourceID="ds" AdjustPageSize="Auto">
		                <Mode AllowColMoving="False" AllowAddNew="False" AllowDelete="False" />
		                <ActionBar DefaultAction="cmdViewContact">
		                    <Actions>
		                        <EditRecord ToolBarVisible="False" />
		                        <Delete ToolBarVisible="False"/>
		                        <AddNew ToolBarVisible="False"/>
		                    </Actions>
		                    <CustomItems>
		                        <px:PXToolBarButton Text="New Employee">
		                            <AutoCallBack Command="NewContact" Target="ds" />
		                        </px:PXToolBarButton>
		                    </CustomItems>
		                </ActionBar>
		                <Levels>
		                    <px:PXGridLevel DataMember="Employees" DataKeyNames="AcctCD">
		                        <RowTemplate>
		                        </RowTemplate>
		                        <Columns>
		                            <px:PXGridColumn DataField="BranchAlias__BranchCD" Width="80px"/>
		                            <px:PXGridColumn DataField="AcctCD" Width="80px" LinkCommand="ViewContact" />
		                            <px:PXGridColumn DataField="Contact__DisplayName" Width="150px" />
		                            <px:PXGridColumn DataField="DepartmentID" Width="80px" />
		                            <px:PXGridColumn DataField="Address__City" Width="80px" />
		                            <px:PXGridColumn DataField="Address__State" Width="80px" />
		                            <px:PXGridColumn DataField="Contact__Phone1" Width="120px" />
		                            <px:PXGridColumn DataField="Contact__EMail" Width="120px" />
		                            <px:PXGridColumn DataField="Status" />
		                        </Columns>
		                        <Layout FormViewHeight="" />
		                    </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" MinHeight="150" />
		            </px:PXGrid>
		        </Template>
		    </px:PXTabItem>
		    <px:PXTabItem Text="Ledgers">
		        <Template>
		            <px:PXGrid ID="grdLedgerLinks" runat="server" Height="300px" Width="100%" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds" SyncPosition="True" AdjustPageSize="Auto">
		                <ActionBar>
		                    <Actions>
		                        <EditRecord ToolBarVisible="False" />
		                        <Delete ToolBarVisible="False"/>
		                    </Actions>
		                    <CustomItems>
		                        <px:PXToolBarButton ImageKey="RecordDel" DisplayStyle="Image">
		                            <AutoCallBack Command="DeleteOrganizationLedgerLink" Target="ds" />
		                        </px:PXToolBarButton>
		                    </CustomItems>
		                </ActionBar>
		                <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
		                <Levels>
		                    <px:PXGridLevel DataMember="OrganizationLedgerLinkWithLedgerSelect">
		                        <RowTemplate>
		                            <px:PXSelector ID="edLedgerID" runat="server" DataField="LedgerID" CommitChanges="True"/>
		                        </RowTemplate>
		                        <Columns>
		                            <px:PXGridColumn DataField="LedgerID" Width="150px" AutoCallBack="True" LinkCommand="ViewLedger"/>
		                            <px:PXGridColumn DataField="Ledger__Descr" Width="300px"/>
		                            <px:PXGridColumn DataField="Ledger__BalanceType" Width="100"/>
		                            <px:PXGridColumn DataField="Ledger__BaseCuryID" />
		                            <px:PXGridColumn DataField="Ledger__ConsolAllowed" TextAlign="Center" Type="CheckBox" Width="110"/>
		                        </Columns>
		                    </px:PXGridLevel>
		                </Levels>
		            </px:PXGrid>
		        </Template>
		    </px:PXTabItem>
			<px:PXTabItem Text="GL Accounts" BindingContext="StateForm" VisibleExp="DataControls[&quot;chkIsGLAccountsTabVisible&quot;].Value == true">
				<Template>
					<px:PXFormView ID="frmDefLocationGL" runat="server" DataMember="DefLocation" Width="100%" CaptionVisible="False" TemplateContainer="" DataSourceID="ds" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSegmentMask ID="edCMPSalesSubID" runat="server" DataField="CMPSalesSubID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edCMPExpenseSubID" runat="server" DataField="CMPExpenseSubID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edCMPFreightSubID" runat="server" DataField="CMPFreightSubID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edCMPDiscountSubID" runat="server" DataField="CMPDiscountSubID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edCMPGainlossSubID" runat="server" DataField="CMPGainLossSubID" DataSourceID="ds" />
		                    <px:PXSegmentMask ID="edCMPPayrollSubID" runat="server" DataField="CMPPayrollSubID" DataSourceID="ds" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		    <px:PXTabItem Text="1099 Settings" BindingContext="PXFormView3" VisibleExp="DataControls[&quot;edReporting1099&quot;].Value == true" >
		        <Template>
		            <px:PXFormView ID="OrgPart10" runat="server" DataMember="OrganizationView" DataSourceID="ds" CaptionVisible="False" TemplateContainer="" SkinID="Transparent">
		                <Template>
		                    <px:PXLayoutRule runat="server" ID="Rule7" GroupCaption="E-Filing Settings" StartRow="True" LabelsWidth="M" ControlSize="XM" />
		                    <px:PXTextEdit runat="server" ID="edTCC1" DataField="TCC" />
		                    <px:PXCheckBox runat="server" ID="edCFSFiler2" DataField="CFSFiler" Text="CFSFiler" />
		                    <px:PXCheckBox runat="server" ID="edForeignEntity3" DataField="ForeignEntity" Text="ForeignEntity" />
		                    <px:PXTextEdit runat="server" ID="edContactName4" DataField="ContactName" />
		                    <px:PXTextEdit runat="server" ID="edCTelNumber5" DataField="CTelNumber" />
		                    <px:PXTextEdit runat="server" ID="edCEmail6" DataField="CEmail" />
		                    <px:PXTextEdit runat="server" ID="edNameControl1" DataField="NameControl" />
		                </Template>
		            </px:PXFormView>
		        </Template>
		    </px:PXTabItem>
			<px:PXTabItem Text="Visual Appearance" RepaintOnDemand="False">
				<Template>
					<px:PXFormView ID="frmLogo" runat="server" DataMember="OrganizationView" Width="100%" CaptionVisible="False" TemplateContainer="" DataSourceID="ds" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
						    
						    <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" GroupCaption="Site Logo" StartGroup="True" StartColumn="True"/>
						    <px:PXLabel ID="lblLogoImgUploader" runat="server">Recommended Size: Width 210px, Height 50px</px:PXLabel>
						    <px:PXImageUploader ID="logoImgUploader" DataMember="OrganizationView" runat="server" DataField="LogoName" Height="120px" Width="420px" AllowUpload="True" SuppressLabel="True"/>
						    <px:PXTextEdit ID="lblLogoFileName" runat="server" DataMember="OrganizationView" DataField="LogoNameGetter" Width="322px" Enabled="False"/>

						    <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" GroupCaption="Report Logo" StartGroup="True"  StartColumn="True"/>
						    <px:PXLabel ID="lblLogoReportImgUploader" runat="server">Recommended Size: Width 420px, Height 100px</px:PXLabel>
						    <px:PXImageUploader ID="reportLogoImgUploader" DataMember="OrganizationView" runat="server" DataField="LogoNameReport" Height="120px" Width="420px" AllowUpload="True" SuppressLabel="True" AllowNoImage="true"/>
						    <px:PXTextEdit ID="lblLogoFileNameReport" runat="server" DataMember="OrganizationView"  DataField="LogoNameReportGetter" Width="322px" Enabled="False"/>
                            
						    <px:PXLayoutRule runat="server" GroupCaption="Theme" StartGroup="True" StartRow="True"
						                     LabelsWidth="SM" ControlSize="XM" />
						    <px:PXCheckBox ID="chkOverrideThemeVariables" runat="server" DataField="OverrideThemeVariables" CommitChanges="True" />
						    <px:PXTextEdit ID="edPrimaryColor" runat="server" DataField="PrimaryColor" 
						                   CommitChanges="True" TextMode="Color" />
						    <px:PXTextEdit ID="PXBackgroundColor" runat="server" DataField="BackgroundColor" 
						                   CommitChanges="True" TextMode="Color" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="ROT & RUT Settings">
				<Template>
					<px:PXFormView runat="server" DataSourceID="ds" SkinID="Transparent" DataMember="OrganizationView" ID="RUTROTSettings" BindingContext="StateForm" VisibleExp="DataControls[&quot;chkIsRutRotTabVisible&quot;].Value == true">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXCheckBox runat="server" DataField="AllowsRUTROT" CommitChanges="True" ID="edRUTROTFlag" />
							<px:PXSelector runat="server" DataField="RUTROTCuryID" ID="edRUTROTCury" CommitChanges="true" Size="s" />
							<px:PXNumberEdit runat="server" DataField="RUTROTClaimNextRefNbr" ID="edRUTROTNextRef" CommitChanges="True" />
							<px:PXTextEdit runat="server" DataField="RUTROTOrgNbrValidRegEx" ID="edRUTROTOrgNbrRegEx" />
                            <px:PXDropDown runat="server" DataField="DefaultRUTROTType" ID="edDefaultRUTROTType" CommitChanges="true" />
                            <px:PXSelector runat="server" DataField="TaxAgencyAccountID" ID="edTaxAgencyAccountID" CommitChanges="true" />
                            <px:PXDropDown runat="server" DataField="BalanceOnProcess" ID="edBalanceOnProcess" CommitChanges="true" />

							<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" GroupCaption="ROT Settings" />
							<px:PXNumberEdit runat="server" DataField="ROTPersonalAllowanceLimit" ID="edROTAllowance" />
							<px:PXNumberEdit runat="server" DataField="ROTExtraAllowanceLimit" ID="edROTAllowanceExtra" />
							<px:PXNumberEdit runat="server" DataField="ROTDeductionPct" ID="edROTPercent" />

							<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" GroupCaption="RUT Settings" />
							<px:PXNumberEdit runat="server" DataField="RUTPersonalAllowanceLimit" ID="edRUTAllowance" />
							<px:PXNumberEdit runat="server" DataField="RUTExtraAllowanceLimit" ID="edRUTAllowanceExtra" />
							<px:PXNumberEdit runat="server" DataField="RUTDeductionPct" ID="edRUTPercent" />

						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="533" MinWidth="600" />
	</px:PXTab>
</asp:Content>
