<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN101000.aspx.cs" Inherits="Page_IN101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INSetupMaint" PrimaryView="setup">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="ViewRestrictionGroup" Visible="False" DependOnGrid="gridGroups" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="684px" Style="z-index: 100" Width="100%" DataMember="setup">
		<Activity HighlightColor="" SelectedColor="" Width="" Height="" />
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
					<px:PXSelector ID="edBatchNumberingID" runat="server" DataField="BatchNumberingID" Text="BATCH" AllowEdit="True" />
					<px:PXSelector ID="edReceiptNumberingID" runat="server" DataField="ReceiptNumberingID" Text="INRECEIPT" AllowEdit="True" />
					<px:PXSelector ID="edIssueNumberingID" runat="server" DataField="IssueNumberingID" Text="INISSUE" AllowEdit="True" />
					<px:PXSelector ID="edAdjustmentNumberingID" runat="server" DataField="AdjustmentNumberingID" Text="INADJUST" AllowEdit="True" />
					<px:PXSelector ID="edKitAssemblyNumberingID" runat="server" DataField="KitAssemblyNumberingID" Text="INKITASSY" AllowEdit="True" />
					<px:PXSelector ID="edPINumberingID" runat="server" AllowEdit="True" DataField="PINumberingID" Text="PIID" />
					<px:PXSelector ID="edReplenishmentNumberingID" runat="server" DataField="ReplenishmentNumberingID" Text="INREPL" AllowEdit="True" />
					<px:PXSelector ID="edServiceItemNumbering" runat="server" DataField="ServiceItemNumberingID" AllowEdit="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Inventory Options" />
                    <%--<px:PXSelector ID="edTransitSite" runat="server" DataField="TransitSiteID" AllowEdit="True" /> --%>
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkUseInventory" runat="server" Checked="True" DataField="UseInventorySubItem" />
					<px:PXCheckBox ID="chkReplanBackOrders" runat="server" DataField="ReplanBackOrders" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Account Settings" />
					<px:PXSegmentMask CommitChanges="True" ID="edARClearingAcctID" runat="server" DataField="ARClearingAcctID" />
					<px:PXSegmentMask ID="edARClearingSubID" runat="server" DataField="ARClearingSubID" AutoRefresh="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edTransitBranchID" runat="server" DataField="TransitBranchID" />
					<px:PXSegmentMask CommitChanges="True" ID="edINTransitAcctID" runat="server" DataField="INTransitAcctID" />
					<px:PXSegmentMask ID="edINTransitSubID" runat="server" DataField="INTransitSubID" AutoRefresh="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edINProgressAcctID" runat="server" DataField="INProgressAcctID" />
					<px:PXSegmentMask ID="edINProgressSubID" runat="server" DataField="INProgressSubID" AutoRefresh="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Posting Settings" />
					<px:PXCheckBox SuppressLabel="True" ID="chkUpdateGL" runat="server" DataField="UpdateGL" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSummPost" runat="server" DataField="SummPost" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoPost" runat="server" DataField="AutoPost" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Data Entry Settings" />
					<px:PXCheckBox SuppressLabel="True" ID="PXCheckBox4" runat="server" Checked="True" DataField="HoldEntry" />
					<px:PXCheckBox SuppressLabel="True" ID="PXCheckBox5" runat="server" Checked="True" DataField="RequireControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkByOne" runat="server" DataField="AddByOneBarcode" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoAdd" runat="server" DataField="AutoAddLineBarcode" />
					<px:PXSegmentMask CommitChanges="True" ID="edDfltStkItemClassID" runat="server" DataField="DfltStkItemClassID" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edDfltNonStkItemClassID" runat="server" DataField="DfltNonStkItemClassID" AllowEdit="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Reason Codes" />
					<px:PXSelector CommitChanges="True" ID="edReceiptReasonCode" runat="server" DataField="ReceiptReasonCode" AllowEdit="True" />
					<px:PXSelector CommitChanges="True" ID="edIssuesReasonCode" runat="server" DataField="IssuesReasonCode" AllowEdit="True" />
					<px:PXSelector CommitChanges="True" ID="edAdjustmentReasonCode" runat="server" DataField="AdjustmentReasonCode" AllowEdit="True" />
					<px:PXSelector CommitChanges="True" ID="edPIAdjReasonCode" runat="server" DataField="PIReasonCode" AllowEdit="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Physical Inventory Settings" />
					<px:PXCheckBox SuppressLabel="True" ID="chkPIUseTags" runat="server" DataField="PIUseTags" />
					<px:PXNumberEdit ID="edPILastTagNumber" runat="server" DataField="PILastTagNumber" />
					<px:PXNumberEdit CommitChanges="True" ID="edTurnoverPeriodsPerYear" runat="server" DataField="TurnoverPeriodsPerYear" />
					<px:PXCheckBox SuppressLabel="True" ID="autoReleasePIAdjustment" runat="server" DataField="AutoReleasePIAdjustment" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subitem/Restriction Groups" BindingContext="form" VisibleExp="DataControls[&quot;chkUseInventory&quot;].Value = 1">
				<Template>
					<px:PXGrid ID="gridGroups" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" BorderWidth="0px">
						<ActionBar>
							<Actions>
								<NoteShow Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Group Details" CommandSourceID="ds" CommandName="ViewRestrictionGroup" />
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Groups" DataKeyNames="GroupName">
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="GroupName" Width="150px" AutoCallBack="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="Description" Width="200px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn AllowUpdate="False" DataField="GroupType" Label="Visible To Entities" RenderEditorText="True" Width="171px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
									<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
									<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
									<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" Enabled="False" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Reporting Settings">
                <Template>
                    <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="100%" Caption="Default Sources"
                        AdjustPageSize="Auto" AllowPaging="True">
                        <AutoCallBack Target="gridNR" Command="Refresh" />
                        <Levels>
                            <px:PXGridLevel DataMember="Notifications" DataKeyNames="Module,NotificationCD">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                    <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                    <px:PXSelector ID="edNBranchID" runat="server" DataField="NBranchID" />
                                    <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                    <px:PXCheckBox ID="chkActiveNotification" runat="server" DataField="Active" />
                                    <px:PXSelector ID="edDefPrinterID" runat="server" DataField="DefaultPrinterID" />
                                    <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="NotificationCD" Width="120px" />
                                    <px:PXGridColumn DataField="NBranchID" Width="120px" />
                                    <px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="DefaultPrinterID" Width="120px" />
                                    <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" Width="150px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Warehouse Management">
				<Template>
					<px:PXFormView ID="formScanSetup" runat="server" DataSourceID="ds" Width="100%" DataMember="ScanSetup" DefaultControlID="edUseDefaultQtyInReceipt" RenderStyle="Simple">
						<Template>
							<px:PXLabel ID="lblScanSetup" runat="server" Height="30px">These settings are specific to the current branch.</px:PXLabel>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="General" />
							<px:PXCheckBox SuppressLabel="True" ID="edExplicitLineConfirmation" runat="server" DataField="ExplicitLineConfirmation" />
							<px:PXCheckBox SuppressLabel="True" ID="edDefaultWarehouse" runat="server" DataField="DefaultWarehouse" CommitChanges="true" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Receipt Settings" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultQtyInReceipt" runat="server" DataField="UseDefaultQtyInReceipt" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultReasonInReceipt" runat="server" DataField="UseDefaultReasonCodeInReceipt" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edRequestLocationForEachItemInReceipt" runat="server" DataField="RequestLocationForEachItemInReceipt" CommitChanges="true" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Issue Settings" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultQtyInIssue" runat="server" DataField="UseDefaultQtyInIssue" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultReasonInIssue" runat="server" DataField="UseDefaultReasonCodeInIssue" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edRequestLocationForEachItemInIssue" runat="server" DataField="RequestLocationForEachItemInIssue" CommitChanges="true" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Transfer Settings" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultQtyInTransfer" runat="server" DataField="UseDefaultQtyInTransfer" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultReasonInTransfer" runat="server" DataField="UseDefaultReasonCodeInTransfer" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edRequestLocationForEachItemInTransfer" runat="server" DataField="RequestLocationForEachItemInTransfer" CommitChanges="true" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseCartsForTransfers" runat="server" DataField="UseCartsForTransfers" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="PI Count Settings" />
							<px:PXCheckBox SuppressLabel="True" ID="edUseDefaultQtyInCount" runat="server" DataField="UseDefaultQtyInCount" CommitChanges="true" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GS1 Units">
				<Template>
					<px:PXFormView ID="formGS1Setup" runat="server" DataSourceID="ds" Width="100%" DataMember="gs1setup" DefaultControlID="edKilogram" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" StartGroup="True" GroupCaption="Weight" />
							<px:PXSelector ID="edKilogram" runat="server" DataField="Kilogram" AutoRefresh="true" />
							<px:PXSelector ID="edPound" runat="server" DataField="Pound" AutoRefresh="true" />
							<px:PXSelector ID="edOunce" runat="server" DataField="Ounce" AutoRefresh="true" />
							<px:PXSelector ID="edTroyOunce" runat="server" DataField="TroyOunce" AutoRefresh="true" />
							<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" GroupCaption="Pressure" />
							<px:PXSelector ID="edKilogramPerSqrMetre" runat="server" DataField="KilogramPerSqrMetre" AutoRefresh="true" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" StartGroup="True" GroupCaption="Linear Size" />
							<px:PXSelector ID="edMetre" runat="server" DataField="Metre" AutoRefresh="true" />
							<px:PXSelector ID="edInch" runat="server" DataField="Inch" AutoRefresh="true" />
							<px:PXSelector ID="edFoot" runat="server" DataField="Foot" AutoRefresh="true" />
							<px:PXSelector ID="edYard" runat="server" DataField="Yard" AutoRefresh="true" />
							<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" GroupCaption="Area" />
							<px:PXSelector ID="edSqrMetre" runat="server" DataField="SqrMetre" AutoRefresh="true" />
							<px:PXSelector ID="edSqrInch" runat="server" DataField="SqrInch" AutoRefresh="true" />
							<px:PXSelector ID="edSqrFoot" runat="server" DataField="SqrFoot" AutoRefresh="true" />
							<px:PXSelector ID="edSqrYard" runat="server" DataField="SqrYard" AutoRefresh="true" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" StartGroup="True" GroupCaption="Volume" />
							<px:PXSelector ID="edCubicMetre" runat="server" DataField="CubicMetre" AutoRefresh="true" />
							<px:PXSelector ID="edCubicInch" runat="server" DataField="CubicInch" AutoRefresh="true" />
							<px:PXSelector ID="edCubicFoot" runat="server" DataField="CubicFoot" AutoRefresh="true" />
							<px:PXSelector ID="edCubicYard" runat="server" DataField="CubicYard" AutoRefresh="true" />
							<px:PXSelector ID="edLitre" runat="server" DataField="Litre" AutoRefresh="true" />
							<px:PXSelector ID="edQuart" runat="server" DataField="Quart" AutoRefresh="true" />
							<px:PXSelector ID="edGallonUS" runat="server" DataField="GallonUS" AutoRefresh="true" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="true" />
	</px:PXTab>
</asp:Content>
