<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP201000.aspx.cs"
    Inherits="Page_AP201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.VendorClassMaint" PrimaryView="VendorClassRecord">
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
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="Vendor Class Summary" DataSourceID="ds" NoteIndicator="True"
        FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DataMember="VendorClassRecord" DefaultControlID="edVendorClassID"
        TemplateContainer="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edVendorClassID" runat="server" DataField="VendorClassID" DataSourceID="ds" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="559px" Style="z-index: 100" Width="100%" Caption="Vendor Class"
        DataMember="CurVendorClassRecord" ActivityField="NoteActivity">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings" RepaintOnDemand="false">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" GroupCaption="Default General Settings"
                        StartGroup="True" />
                    <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" />
                    <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" AllowEdit="True" />
                    <px:PXCheckBox ID="chkRequireTaxZone" runat="server" DataField="RequireTaxZone" />
                    <px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" />
                    <px:PXCheckBox ID="chkDefaultLocationCDFromBranch" runat="server" DataField="DefaultLocationCDFromBranch" />
                    <px:PXSelector runat="server" ID="edLocale" DataField="VendorClassRecord.LocaleName" DisplayMode="Text" />
                    <px:PXSelector ID="edGroupMask" runat="server" DataField="GroupMask">
                    </px:PXSelector>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Purchase Settings" />
                    <px:PXSelector ID="edShipTermsID" runat="server" DataField="ShipTermsID" />
                    <px:PXDropDown ID="edRcptQtyAction" runat="server" AllowNull="False" DataField="RcptQtyAction" SelectedIndex="1" />
                    <px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Default Financial Settings" LabelsWidth="SM" StartColumn="True"
                        StartGroup="True" />
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="True"
                        AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edCashAcctID" runat="server" DataField="CashAcctID" AllowEdit="True" AutoRefresh="True" />
                    <px:PXDropDown ID="edPaymentByType" runat="server" DataField="PaymentByType" />
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="True" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Size="XS" />
                    <px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
                    <px:PXSelector ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" Size="XS" />
                    <px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
                    <px:PXCheckBox ID="chkPaymentsByLinesAllowed" runat="server" DataField="PaymentsByLinesAllowed" />
                    <px:PXCheckBox ID="chkRetainageApply" runat="server" DataField="RetainageApply" />

                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
                    <px:PXLayoutRule runat="server" GroupCaption="Default Print and Email Settings" StartGroup="True" />
                    <px:PXCheckBox ID="chkPrintPO" runat="server" Checked="True" DataField="PrintPO" />
                    <px:PXCheckBox ID="chkEmailPO" runat="server" Checked="True" DataField="EmailPO" />
                    <px:PXLayoutRule runat="server" ID="PXLayoutRule7" StartGroup="True" GroupCaption="Default Lien Waiver Settings" />
	                <px:PXCheckBox runat="server" ID="chkShouldGenerateLienWaivers" DataField="ShouldGenerateLienWaivers" AlignLeft="True" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
                    <px:PXSegmentMask ID="edAPAcctID" runat="server" CommitChanges="True" DataField="APAcctID" />
                    <px:PXSegmentMask ID="edAPSubID" runat="server" CommitChanges="True" DataField="APSubID" />
                    <px:PXSegmentMask ID="edExpenseAcctID" runat="server" CommitChanges="True" DataField="ExpenseAcctID" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" CommitChanges="True" DataField="ExpenseSubID" />
                     <px:PXSegmentMask ID="edDiscountAcctID" runat="server" CommitChanges="true" DataField="DiscountAcctID">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edDiscountSubID" runat="server" DataField="DiscountSubID">
					</px:PXSegmentMask>
                    <px:PXSegmentMask ID="edDiscTakenAcctID" runat="server" CommitChanges="True" DataField="DiscTakenAcctID" />
                    <px:PXSegmentMask ID="edDiscTakenSubID" runat="server" CommitChanges="True" DataField="DiscTakenSubID" />
                    <px:PXSegmentMask ID="edPrepaymentAcctID" runat="server" CommitChanges="True" DataField="PrepaymentAcctID" />
                    <px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="VendorClass.PrepaymentAcctID" PropertyName="DataControls[&quot;edPrepaymentAcctID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
					<px:PXSegmentMask ID="edPrebookAcctID" runat="server" CommitChanges="True" DataField="PrebookAcctID">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edPrebookSubID" runat="server" DataField="PrebookSubID">
						<Parameters>
							<px:PXControlParam ControlID="tab" Name="VendorClass.PrebookAcctID" PropertyName="DataControls[&quot;edPrebookAcctID&quot;].Value" />
						</Parameters>
					</px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true">
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID">
					</px:PXSegmentMask>
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedGainAcctID" runat="server" DataField="UnrealizedGainAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedGainSubID" runat="server" DataField="UnrealizedGainSubID" DataSourceID="ds">
                        <Parameters>
							<px:PXControlParam ControlID="tab" Name="VendorClass.unrealizedGainAcctID" PropertyName="DataControls[&quot;edUnrealizedGainAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
                    </px:PXSegmentMask>
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedLossAcctID" runat="server" DataField="UnrealizedLossAcctID" DataSourceID="ds" />
					<px:PXSegmentMask CommitChanges="True" ID="edUnrealizedLossSubID" runat="server" DataField="UnrealizedLossSubID" DataSourceID="ds">
                        <Parameters>
							<px:PXControlParam ControlID="tab" Name="VendorClass.unrealizedLossAcctID" PropertyName="DataControls[&quot;edUnrealizedLossAcctID&quot;].Value" />
						</Parameters>
						<CallBackMode PostData="Container" ContainerID="tab" />
                    </px:PXSegmentMask>
					
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="RetainageAcctID" CommitChanges="true" />
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
                            <px:PXGrid ID="gridNS" runat="server" AdjustPageSize="Auto" AllowPaging="True" Caption="Mailings" DataSourceID="ds"
                                Height="150px" SkinID="DetailsInTab" Width="100%">
                                <Levels>
                                    <px:PXGridLevel DataKeyNames="SourceID,SetupID" DataMember="NotificationSources">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
                                            <px:PXLayoutRule runat="server" Merge="True" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" />
                                            <px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID"/>
                                            <px:PXLayoutRule runat="server" />
                                            <px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
                                            <px:PXSelector ID="edSetupID" runat="server" DataField="SetupID" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID"  DisplayMode="Text"/>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn AutoCallBack="True" DataField="SetupID" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="NBranchID" Label="Branch" />
                                            <px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="ReportID" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="NotificationID" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="Format" RenderEditorText="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <AutoCallBack Command="Refresh" Target="gridNR">
                                </AutoCallBack>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" AdjustPageSize="Auto" AllowPaging="True" Caption="Recipients" DataSourceID="ds" SkinID="DetailsInTab"
                                Width="100%">
                                <Levels>
                                    <px:PXGridLevel DataKeyNames="NotificationID" DataMember="NotificationRecipients">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
                                            <px:PXSelector ID="edContactID" runat="server" AllowEdit="True" AutoRefresh="True" DataField="ContactID" ValueField="DisplayName" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn AutoCallBack="True" DataField="ContactType" RenderEditorText="True" />
                                            <px:PXGridColumn AllowShowHide="False" DataField="OriginalContactID" Visible="False" />
                                            <px:PXGridColumn DataField="ContactID" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="Format" RenderEditorText="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <CallbackCommands>
                                    <Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="gridNS" />
                                </Parameters>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXTab>
    <px:PXSmartPanel runat="server" ID="LienWaiverRecipientProjectsDialog" Height="400px" Width="1170px" LoadOnDemand="True" AutoReload="True" CaptionVisible="True" Caption="Add Vendor Class to Project" Key="LienWaiverRecipientProjects" CommandSourceID="ds" AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGridProjects">
		<px:PXGrid runat="server" ID="PXGridProjects" Height="200px" SkinID="Inquire" TabIndex="17500" Width="100%" BatchUpdate="True" FilesIndicator="False" NoteIndicator="False" AllowSearch="False">
			<Mode AllowAddNew="False" />
			<Levels>
				<px:PXGridLevel DataMember="LienWaiverRecipientProjects">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" TextAlign="Center" AllowMove="False" AllowSort="False" AllowCheckAll="True" />
						<px:PXGridColumn DataField="PMProject__ContractCD" />
						<px:PXGridColumn DataField="PMProject__Status" />
						<px:PXGridColumn DataField="PMProject__CustomerID" />
						<px:PXGridColumn DataField="PMProject__Description" />
						<px:PXGridColumn DataField="PMProject__CustomerID_Customer_acctName" />
						<px:PXGridColumn DataField="PMProject__CuryID" />
						<px:PXGridColumn DataField="MinimumCommitmentAmount" />
					</Columns>
					<RowTemplate>
						<px:PXNumberEdit runat="server" ID="PXNumberEdit3" DataField="MinimumCommitmentAmount" />
					</RowTemplate>
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel runat="server" ID="PXPanelAddToProjects" SkinID="Buttons">
			<px:PXButton runat="server" ID="btnOK" Text="Add" DialogResult="OK">
				<AutoCallBack Command="Save" Target="PXGridProjects" />
			</px:PXButton>
			<px:PXButton runat="server" ID="btnCancel" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
