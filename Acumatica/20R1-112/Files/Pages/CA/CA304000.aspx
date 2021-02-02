<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA304000.aspx.cs" Inherits="Page_CA304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CAAdjRecords" TypeName="PX.Objects.CA.CATranEntry">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="true" Name="Release" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="Reverse" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="Action" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Hold" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
            <px:PXDSCallbackCommand Name="Flow" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="CAAdjRecords" Caption="Transaction Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edAdjRefNbr" MarkRequired="Dynamic">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="XM" />
            <px:PXDropDown ID="edAdjTranType" runat="server" DataField="AdjTranType" Enabled="False" Size="S"/>
            <px:PXSelector ID="edAdjRefNbr" runat="server" DataField="AdjRefNbr" Size="S"/>
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" Size="XM"/>
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_CAAdj_CurrencyInfo_" DataMember="_Currency_" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" Size="S"/>
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
                <AutoCallBack Command="Hold" Target="ds" />
            </px:PXCheckBox>
            <px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True" Size="S"/>
            <px:PXSelector CommitChanges="True" ID="edEntryTypeID" runat="server" DataField="EntryTypeID" AutoRefresh="True" />
            <px:PXDropDown ID="edDrCr" runat="server" DataField="DrCr" Enabled="False" />
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
            <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="s" />
            <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryTaxRoundDiff" runat="server" DataField="CuryTaxRoundDiff" Enabled="False" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryControlAmt" runat="server" DataField="CuryControlAmt" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
        </Template>
    </px:PXFormView>

    	<style type="text/css">
		.leftDocTemplateCol
		{
			width: 50%; float:left; min-width: 90px;
		}
		.rightDocTemplateCol
		{
			margin-left: 51%; min-width: 90px;
		}
	</style>
	<px:PXGrid ID="docsTemplate" runat="server" Visible="false">
		<Levels>
			<px:PXGridLevel>
				<Columns>
					<px:PXGridColumn Key="Template">
						<CellTemplate>
							<div id="ParentDiv1" class="leftDocTemplateCol">
                                <div id="div11" class="Field0"><%# ((PXGridCellContainer)Container).Text("adjRefNbr") %></div>								
								<div id="div12" class="Field1"><%# ((PXGridCellContainer)Container).Text("tranDate") %></div>
								<div id="div13" class="Field1"><%# ((PXGridCellContainer)Container).Text("cashAccountID") %></div>
							</div>
							<div id="ParentDiv2"  class="rightDocTemplateCol">
								<span id="span21" class="Field1"><%# ((PXGridCellContainer)Container).Text("curyTranAmt") %></span>                                
								<span id="span22" class="Field1"><%# ((PXGridCellContainer)Container).Text("curyID") %></span>
                                <div id="div21" class="Field1"><%# ((PXGridCellContainer)Container).Text("status") %></div>
                                <div id="div22" class="Field1"><%# ((PXGridCellContainer)Container).Text("entryTypeID") %></div>
							</div>
						</CellTemplate>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="180px" Width="100%" DataMember="CurrentDocument" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Transaction Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" SkinID="DetailsInTab" DataSourceID="ds" TabIndex="-14436" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="CASplitRecords" DataKeyNames="AdjRefNbr,AdjTranType,LineNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                                    <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True"/>
                                    <px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" />
                                    <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" />
                                    <px:PXSelector ID="edCashAccountID" runat="server" DataField="CashAccountID" CommitChanges="True" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="True" />
                                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True"/>
                                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                                    <px:PXGridColumn DataField="BranchID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UOM" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CashAccountID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="AccountID" CommitChanges="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID_description" />
                                    <px:PXGridColumn DataField="SubID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectID" />
                                    <px:PXGridColumn DataField="TaskID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="CostCodeID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="NonBillable" Label="Non Billable" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="TaxCategoryID" CommitChanges="True" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" AutoInsertField="CuryTranAmt" />
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details">
                <Template>
                    <px:PXGrid ID="grid1" runat="server" Height="150px" Width="100%" SkinID="DetailsInTab" DataSourceID="ds">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
                                    <px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" />
                                    <px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
                                    <px:PXNumberEdit ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" AllowUpdate="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExemptedAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Tax__TaxType" />
                                    <px:PXGridColumn DataField="Tax__PendingTax" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Tax__ReverseTax" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Tax__ExemptTax" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Tax__StatisticalTax" Type="CheckBox" TextAlign="Center" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" GroupCaption="Link to GL" StartGroup="True" />
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="TranID_CATran_batchNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                    <px:PXCheckBox CommitChanges="True" ID="chkCleared" runat="server" DataField="Cleared" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edClearDate" runat="server" DataField="ClearDate" />
                    <px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Tax Settings" LabelsWidth="S" StartColumn="True" StartGroup="True" />
                    <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
                    <px:PXCheckBox ID="chkUsesManualVAT" runat="server" DataField="UsesManualVAT" Enabled="false"/>
                    <px:PXDropDown CommitChanges="True" ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" />
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" GroupCaption="Voucher Details" />
                    <px:PXFormView ID="VoucherDetails" runat="server" RenderStyle="Simple"
                        DataMember="Voucher" DataSourceID="ds" TabIndex="1100">
                        <Template>
                            <px:PXTextEdit ID="linkGLVoucherBatch" runat="server" DataField="VoucherBatchNbr" Enabled="false">
                                <LinkCommand Target="ds" Command="ViewVoucherBatch"></LinkCommand>
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="linkGLWorkBook" runat="server" DataField="WorkBookID" Enabled="false">
                                <LinkCommand Target="ds" Command="ViewWorkBook"></LinkCommand>
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" DataSourceID="ds">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
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
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
