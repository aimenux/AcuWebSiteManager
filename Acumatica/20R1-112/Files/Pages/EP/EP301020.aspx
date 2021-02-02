<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="EP301020.aspx.cs" Inherits="Page_EP301020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.ExpenseClaimDetailEntry" PrimaryView="ClaimDetails">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true"/>
            <px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="True" />
            <px:PXDSCallbackCommand Name="Action" Visible="True" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Submit" Visible="False" CommitChanges="true"/>
			<px:PXDSCallbackCommand Name="Claim" Visible="true" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="SaveTaxZone" Visible="False"/>
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ClaimDetails"
        CaptionVisible="False" TabIndex="100" ActivityIndicator="True" NoteIndicator="True" FilesIndicator="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXSelector ID="SelectorClaimDetailCD" runat="server" DataField="ClaimDetailCD" Size="s" TabIndex="101"/>
			<px:PXDateTimeEdit ID="edExpenseDate" runat="server" DataField="ExpenseDate" CommitChanges="True"
                TabIndex="102"/>
			<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" CommitChanges="True" AllowEdit="True"
                TabIndex="103"/>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
			<px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" DataSourceID="ds"
                TabIndex="104"/>
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds"
                TabIndex="105"/>
			<px:PXDropDown ID="PXDropDown1" runat="server" DataField="Status" Enabled="False" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="150px">
            </px:PXLayoutRule>
			<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmtWithTaxes" Enabled="False" />
			<px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="CuryTaxTotal" Enabled="False" />
            <px:PXLayoutRule runat="server" Merge="True"/>
            <px:PXNumberEdit ID="edClaimCuryTranAmtWithTaxes" runat="server" DataField="ClaimCuryTranAmtWithTaxes" Enabled="False" />
            <px:PXTextEdit ID="edCardCuryID" runat="server" DataField="CardCuryID" SkinID="Label" SuppressLabel="True" Enabled="False"/>
            <contentstyle borderstyle="None" />
        </Template>
    </px:PXFormView>
    
     <px:PXSmartPanel ID="TaxZoneUpdateAsk" runat="server" CaptionVisible="True" Key="TaxZoneUpdateAskView" CancelButtonID="CancelTaxZone" 
         AcceptButtonID="SaveTaxZone" >
          <px:PXFormView runat="server" DataMember="TaxZoneUpdateAskView" SkinID="Transparent" ID="PXFormView1">
                <Template>
                        <px:PXLayoutRule ID="PXLayoutRule232" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
					    <px:PXLabel ID="Label" runat="server" Text="Do you want to use the specified tax zone for expense receipts by default?" />
                    
                    </Template>
            </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="SaveTaxZone" runat="server" DialogResult="Yes" Text="Yes" CommandName="SaveTaxZone" CommandSourceID="ds" />
            <px:PXButton ID="CancelTaxZone" runat="server" DialogResult="No" Text="No"  />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="Tabs" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100%" DataMember="CurrentClaimDetails" DefaultControlID="ExpenseDate">
        <Items>
            <px:PXTabItem Text="Receipt Details">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Expense Details" />
					<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" CommitChanges="True" TextMode="MultiLine" Width="365px" Height="96" TabIndex ="106"/>

					<px:PXLayoutRule runat="server" Merge="True"/>
							<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" CommitChanges="True" TabIndex ="107"/>
							<px:PXSelector ID="edUOM" runat="server" DataField="UOM" CommitChanges="True" Size="S" TabIndex ="110"/>
						<px:PXLayoutRule runat="server" />
						<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="True" TabIndex ="108"/>
							<px:PXNumberEdit ID="edCuryEmployeePart" runat="server" DataField="CuryEmployeePart" CommitChanges="True" TabIndex ="111"/>
						<px:PXLayoutRule runat="server" />
						<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXNumberEdit ID="edCuryExtCost" runat="server" DataField="CuryExtCost" CommitChanges="True" TabIndex ="109"/>
							<px:PXNumberEdit ID="edCuryTipAmt" runat="server" DataField="CuryTipAmt" CommitChanges="True" TabIndex ="112"/>
					<px:PXLayoutRule runat="server" />
					
					<pxa:PXCurrencyRate DataField="CuryID" ID="edCuryID" runat="server" DataSourceID="ds" RateTypeView="_EPExpenseClaimDetails_CurrencyInfo_" DataMember="CurrencyList" />
					<px:PXTextEdit ID="edExpenseRefNbr" runat="server" DataField="ExpenseRefNbr" CommitChanges="True"/>
					<px:PXSegmentMask CommitChanges="True" ID="edContract" runat="server" DataField="ContractID" Size="XM" />
					<px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" Size="XM" AutoRefresh="True" />
                         <px:PXSegmentMask CommitChanges="True" ID="edCostCodeID" runat="server" DataField="CostCodeID" Size="XM" AutoRefresh="True" />
					<px:PXSelector CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" AllowEdit="True"/>
					<px:PXDropDown ID="PXStatusClaim" runat="server" DataField="StatusClaim" Enabled="False" />
                    <px:PXDropDown runat="server" ID="edPaidWith" DataField="PaidWith" CommitChanges="True" />
                    <px:PXSelector runat="server" ID="edCorpCardID" DataField="CorpCardID" CommitChanges="True"/>
                    <px:PXLayoutRule ID="PXLayoutInfo" runat="server" StartGroup="True" GroupCaption="Expense Classification" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="PXHold" runat="server" DataField="Hold" Enabled="False" />
                    <px:PXCheckBox ID="PXApproved" runat="server" DataField="Approved" Enabled="False" />
                    <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="Rejected" Enabled="False" />
                    <px:PXCheckBox ID="PXReleased" runat="server" DataField="Released" Enabled="False" />
                    <px:PXCheckBox ID="PXClaimHold" runat="server" DataField="HoldClaim" Enabled="False" />
                    
					<px:PXLayoutRule ID="PXLayoutTax" runat="server" StartGroup="True" GroupCaption="Tax Info" />
					<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" AutoRefresh="True" CommitChanges="True"/>
					<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode"  CommitChanges="True"/>
					<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" CommitChanges="True"/>

					<px:PXLayoutRule ID="PXLayoutFinencial" runat="server" StartGroup="True" GroupCaption="Financial Details" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>
					<px:PXCheckBox ID="chkBillable" runat="server" DataField="Billable" CommitChanges="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
					<px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edSalesAccountID" runat="server" DataField="SalesAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />

					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Image"/>
					<px:PXImageUploader Width="395px" Height="275" ID="imgUploader" runat="server" AllowUpload="True" ViewOnly="True" ArrowsOutside="True" LabelText="&nbsp;" SuppressLabel="True"/>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
                    <px:PXFormView runat="server" DataMember="CurrentClaimDetails" SkinID="Transparent" ID="TaxRoundDiffForm">
                        <Template>
                                <px:PXLayoutRule ID="PXLayoutRule232" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
					            <px:PXNumberEdit ID="edTaxDiff" runat="server" DataField="CuryTaxRoundDiff" Enabled="false" />
                         </Template>
                    </px:PXFormView>
					<px:PXGrid ID="grid1" runat="server" Height="100%"
						Width="100%" ActionsPosition="Top" SkinID="Details" DataSourceID="ds"
						TabIndex="3900">
						<AutoSize Enabled="True" MinHeight="150" />
						<LevelStyles>
							<RowForm Width="300px">
							</RowForm>
						</LevelStyles>
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
								<Save Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<Columns>
									<px:PXGridColumn DataField="TaxID" CommitChanges="true"  />
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="Tax__TaxType"/>
									<px:PXGridColumn DataField="Tax__PendingTax" TextAlign="Center" Type="CheckBox"/>
									<px:PXGridColumn DataField="Tax__ReverseTax" TextAlign="Center" Type="CheckBox"/>
									<px:PXGridColumn DataField="Tax__ExemptTax" TextAlign="Center" Type="CheckBox"/>
									<px:PXGridColumn DataField="Tax__StatisticalTax" TextAlign="Center" Type="CheckBox"/>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
                        <AutoSize Container="Window" Enabled="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Approval Details" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Style="left: 0px; top: 0px;">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false"/>
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
