<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM308000.aspx.cs" Inherits="Page_PM308000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="True" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ChangeOrderEntry" PrimaryView="Document" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AppendSelectedCostBudget" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="AddCostBudget" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="AppendSelectedRevenueBudget" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="AddRevenueBudget" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="AppendSelectedPOLines" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="AddPOLines" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="ViewCommitments" Visible="false" CommitChanges="true" DependOnGrid="DetailsGrid"/>
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Send" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewChangeOrder" Visible="false"/>
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="ViewRevenueBudgetTask" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewCostBudgetTask" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewCommitmentTask" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewRevenueBudgetInventory" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewCostBudgetInventory" Visible="false" />
            <px:PXDSCallbackCommand Name="AddChangeRequests" Visible="false" />
            <px:PXDSCallbackCommand Name="AppendSelectedChangeRequests" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewChangeRequest" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewChangeRequestCostDetails" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewChangeRequestRevenueDetails" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewCommitmentInventory" Visible="false" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
	        <px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ChangeOrderNumber$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Change Order Summary" FilesIndicator="True"
        NoteIndicator="True" NotifyIndicator="true" ActivityIndicator="True" ActivityField="NoteActivity" LinkPage="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S"/>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="true" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="true" />
            
            <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="true"/>
            <px:PXDateTimeEdit ID="edCompletionDate" runat="server" DataField="CompletionDate"/>
            <px:PXNumberEdit ID="edDelayDays" runat="server" DataField="DelayDays" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>
            <px:PXSelector ID="edClassID" runat="server" DataField="ClassID" CommitChanges="true" AllowEdit="true" AllowAddNew="true"/>
            
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" AllowEdit="True"/>
            <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="true"/>
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
            <px:PXTextEdit ID="edProjectNumber" runat="server" DataField="ProjectNbr" CommitChanges="true"/>
            <px:PXLayoutRule runat="server" Merge="True" ControlSize="XS" />
            <px:PXDropDown ID="edReverseStatus" runat="server" DataField="ReverseStatus" TextAlign ="Left" />

             <px:PXTextEdit ID="linkOriginalRefNbr" runat="server" DataField="OrigRefNbr" Enabled="false" AlignLeft="True" TextAlign ="Left" Width="52px" LabelWidth="112px" >
								<LinkCommand Target="ds" Command="ViewChangeOrder" ></LinkCommand>
			</px:PXTextEdit>

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S"/>           
            <px:PXNumberEdit ID="edRevenueTotal" runat="server" DataField="RevenueTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCommitmentTotal" runat="server" DataField="CommitmentTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCostTotal" runat="server" DataField="CostTotal" Enabled="False" />
            <px:PXNumberEdit ID="edGrosMarginAmount" runat="server" DataField="GrossMarginAmount" Enabled="False" />
            <px:PXNumberEdit ID="edGrosMarginPct" runat="server" DataField="GrossMarginPct" Enabled="False" />
            <px:PXNumberEdit runat="server" ID="edChangeRequestCostTotal" DataField="ChangeRequestCostTotal" />
            <px:PXNumberEdit runat="server" ID="edChangeRequestLineTotal" DataField="ChangeRequestLineTotal" />
            <px:PXNumberEdit runat="server" ID="edChangeRequestMarkupTotal" DataField="ChangeRequestMarkupTotal" />
            <px:PXNumberEdit runat="server" ID="edChangeRequestPriceTotal" DataField="ChangeRequestPriceTotal" />
            <px:PXFormView ID="VisibilityForm" runat="server" DataMember="VisibilitySettings" DataSourceID="ds" Caption="Hidden Form needed for VisibleExp of TabItems. Tabs are Hidden based on the values of Combo"
                Visible="False" TabIndex="300">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkIsRevenueVisible" runat="server" DataField="IsRevenueVisible"/>
                    <px:PXCheckBox ID="chkIsCostVisible" runat="server" DataField="IsCostVisible"/>
                    <px:PXCheckBox ID="chkIsDetailsVisible" runat="server" DataField="IsDetailsVisible" />
                    <px:PXCheckBox runat="server" ID="chkIsChangeRequestVisible" DataField="IsChangeRequestVisible" />
                </Template>
            </px:PXFormView>
        </Template>
    </px:PXFormView>
    <px:PXSmartPanel ID="AddCostBudgetPanel" runat="server" Height="396px" Width="850px" Caption="Select Budget Lines" CaptionVisible="True" Key="AvailableCostBudget" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="AvailableCostBudgetGrid" LoadOnDemand="true" AutoRepaint="true">
        <px:PXGrid ID="AvailableCostBudgetGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details" SyncPosition="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="AvailableCostBudget">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Label="Selected" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" />
                        <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" />
                        <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                        <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn AutoCallBack="True" DataField="UOM" />
                        <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCommittedAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedReceivedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedInvoicedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedOpenQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCommittedOpenAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="ActualQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryActualAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="Performance" TextAlign="Right" />
                        <px:PXGridColumn DataField="IsProduction" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="CuryLastCostToComplete" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCostToComplete" TextAlign="Right" />
                        <px:PXGridColumn DataField="LastPercentCompleted" TextAlign="Right" />
                        <px:PXGridColumn DataField="PercentCompleted" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryLastCostAtCompletion" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCostAtCompletion" TextAlign="Right" />
                        <px:PXGridColumn DataField="RevenueTaskID" AutoCallBack="True" />
                        <px:PXGridColumn DataField="RevenueInventoryID" AutoCallBack="True" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <AddNew MenuVisible="False" ToolBarVisible="False" />
                    <Delete MenuVisible="False" ToolBarVisible="Top" />
                    <NoteShow MenuVisible="False" ToolBarVisible="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
         <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonAdd" runat="server" Text="Add Lines" CommandName="AppendSelectedCostBudget"  CommandSourceID="ds" />
            <px:PXButton ID="PXButtonAddClose" runat="server" Text="Add Lines & Close" DialogResult="OK"  />
            <px:PXButton ID="PXButtonClose" runat="server" DialogResult="Cancel" Text="Close" />      
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="AddRevenueBudgetPanel" runat="server" Height="396px" Width="850px" Caption="Select Budget Lines" CaptionVisible="True" Key="AvailableRevenueBudget" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="AvailableCostBudgetGrid" LoadOnDemand="true" AutoRepaint="true">
        <px:PXGrid ID="AvailableRevenueBudgetGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details" SyncPosition="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="AvailableRevenueBudget">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Label="Selected" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" />
                        <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" />
                        <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                        <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn DataField="Type" TextAlign="Right"/>
                        <px:PXGridColumn DataField="Qty" Label="Qty." TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn AutoCallBack="True" DataField="UOM" Label="UOM" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryUnitRate" Label="Rate" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryAmount" Label="Amount" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="RevisedQty" Label="Revised Qty" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryRevisedAmount" Label="Revised Amount" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="LimitQty" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="MaxQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="LimitAmount" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedQty" Label="Committed Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCommittedAmount" Label="Committed Amount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedReceivedQty" Label="Committed Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedInvoicedQty" Label="Committed Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" Label="Committed Amount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CommittedOpenQty" Label="Committed Open Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryCommittedOpenAmount" Label="Committed Open Amount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryInvoicedAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="ActualQty" Label="Actual Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryActualAmount" Label="Actual Amount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" />
                        <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryPrepaymentAmount" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryPrepaymentInvoiced" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryPrepaymentAvailable" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryAmountToInvoice" TextAlign="Right" CommitChanges="true" />
                        <px:PXGridColumn DataField="Performance" TextAlign="Right" />
                        <px:PXGridColumn DataField="TaxCategoryID" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <AddNew MenuVisible="False" ToolBarVisible="False" />
                    <Delete MenuVisible="False" ToolBarVisible="Top" />
                    <NoteShow MenuVisible="False" ToolBarVisible="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
         <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" Text="Add Lines" CommandName="AppendSelectedRevenueBudget"  CommandSourceID="ds" />
            <px:PXButton ID="PXButton2" runat="server" Text="Add Lines & Close" DialogResult="OK"  />
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Close" />      
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="AddPOLinesPanel" runat="server" Height="600px" Width="850px" Caption="Select Commitments" CaptionVisible="True" Key="AvailablePOLines" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="AvailablePOLinesGrid" LoadOnDemand="true" AutoRepaint="true">
        <px:PXFormView ID="AvailablePOLineFilter" runat="server" DataMember="AvailablePOLineFilter" RenderStyle="Normal">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
                <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="true" CommitChanges="true" />
                <px:PXSegmentMask ID="edCostCodeFrom" runat="server" DataField="CostCodeFrom" AutoRefresh="true" CommitChanges="true" />
                <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="true" CommitChanges="true" />
                <px:PXLayoutRule ID="PXLayoutRule10" runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
                <px:PXSelector ID="edPOOrderNbr" runat="server" DataField="POOrderNbr" AutoRefresh="true" CommitChanges="true" />                
                <px:PXSegmentMask ID="edCostCodeTo" runat="server" DataField="CostCodeTo" AutoRefresh="true" CommitChanges="true" />
                <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="true" CommitChanges="true" />
                 <px:PXCheckBox ID="chkIncludeNonOpen" runat="server" DataField="IncludeNonOpen"  CommitChanges="true"/>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="AvailablePOLinesGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details" SyncPosition="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="AvailablePOLines">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Label="Selected" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn DataField="TaskID" />
                        <px:PXGridColumn DataField="CostCodeID" />
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />                       
                        <px:PXGridColumn DataField="VendorID" />
                        <px:PXGridColumn DataField="CommitmentType" />
                        <px:PXGridColumn DataField="OrderNbr" />
                        <px:PXGridColumn DataField="OrderDate" />
                        <px:PXGridColumn DataField="CuryID" />
                        <px:PXGridColumn DataField="LineNbr" />
                        <px:PXGridColumn DataField="TranDesc" />
                        <px:PXGridColumn DataField="OrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" />
                        <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryExtCost" TextAlign="Right" />
                        
                        <px:PXGridColumn DataField="PromisedDate" />
                        <px:PXGridColumn DataField="VendorRefNbr" />
                        <px:PXGridColumn DataField="AlternateID" />
                        <px:PXGridColumn DataField="Cancelled" Type="CheckBox" />
                        <px:PXGridColumn DataField="Completed" Type="CheckBox"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <AddNew MenuVisible="False" ToolBarVisible="False" />
                    <Delete MenuVisible="False" ToolBarVisible="Top" />
                    <NoteShow MenuVisible="False" ToolBarVisible="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
         <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton4" runat="server" Text="Add Lines" CommandName="AppendSelectedPOLines"  CommandSourceID="ds" />
            <px:PXButton ID="PXButton5" runat="server" Text="Add Lines & Close" DialogResult="OK"  />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Close" />      
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel runat="server" Height="396px" Width="850px" ID="AddChangeRequestPanel" LoadOnDemand="true" AutoRepaint="true" CaptionVisible="True" Caption="Select Change Requests" Key="AvailableChangeRequests" AutoCallBack-Enabled="True" AutoCallBack-Target="AvailableChangeRequestGrid" AutoCallBack-Command="Refresh">
		<px:PXGrid runat="server" SyncPosition="true" Height="240px" SkinID="Details" Width="100%" ID="AvailableChangeRequestGrid" DataSourceID="ds">
			<AutoSize Enabled="true" />
			<ActionBar>
				<Actions>
					<AddNew ToolBarVisible="False" MenuVisible="False" />
					<Delete ToolBarVisible="Top" MenuVisible="False" />
					<NoteShow ToolBarVisible="False" MenuVisible="False" /></Actions></ActionBar>
			<Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
			<Levels>
				<px:PXGridLevel DataMember="AvailableChangeRequests">
					<Columns>
						<px:PXGridColumn Type="CheckBox" DataField="Selected" AllowCheckAll="true" Label="Selected" />
						<px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeRequest" />
						<px:PXGridColumn DataField="Date" />
						<px:PXGridColumn DataField="ExtRefNbr" />
						<px:PXGridColumn DataField="Description" />
						<px:PXGridColumn TextAlign="Right" DataField="CostTotal" />
						<px:PXGridColumn TextAlign="Right" DataField="LineTotal" />
						<px:PXGridColumn TextAlign="Right" DataField="MarkupTotal" />
						<px:PXGridColumn TextAlign="Right" DataField="PriceTotal" /></Columns></px:PXGridLevel></Levels></px:PXGrid>
		<px:PXPanel runat="server" SkinID="Buttons" ID="ChangeRequestPanelButton">
			<px:PXButton runat="server" Text="Add Change Requests" CommandSourceID="ds" CommandName="AppendSelectedChangeRequests" ID="PXButton7" />
			<px:PXButton runat="server" Text="Add Change Requests &amp; Close" DialogResult="OK" ID="PXButton8" />
			<px:PXButton runat="server" Text="Close" DialogResult="Cancel" ID="PXButton9" /></px:PXPanel></px:PXSmartPanel>
	<px:PXSmartPanel runat="server" Height="396px" Width="850px" ID="ViewChangeRequestCostPanel" LoadOnDemand="true" AutoRepaint="true" CaptionVisible="True" Caption="Change Request Details" Key="ChangeRequestCostDetails" AutoCallBack-Enabled="True" AutoCallBack-Target="ChangeRequestCostDetailsGrid" AutoCallBack-Command="Refresh">
		<px:PXGrid runat="server" SyncPosition="true" Height="240px" SkinID="Inquire" Width="100%" ID="ChangeRequestCostDetailsGrid" DataSourceID="ds">
			<AutoSize Enabled="true" />
			<ActionBar>
				<Actions>
					<AddNew ToolBarVisible="False" MenuVisible="False" />
					<Delete ToolBarVisible="Top" MenuVisible="False" />
					<NoteShow ToolBarVisible="False" MenuVisible="False" /></Actions></ActionBar>
			<Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
			<Levels>
				<px:PXGridLevel DataMember="ChangeRequestCostDetails">
					<Columns>
						<px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeRequest" />
						<px:PXGridColumn DataField="Description" />
						<px:PXGridColumn TextAlign="Right" DataField="Qty" />
						<px:PXGridColumn TextAlign="Right" DataField="UnitCost" />
						<px:PXGridColumn TextAlign="Right" DataField="ExtCost" /></Columns></px:PXGridLevel></Levels></px:PXGrid></px:PXSmartPanel>
    <px:PXSmartPanel runat="server" Height="520px" Width="850px" ID="ViewChangeRequestRevenuePanel" AutoRepaint="true" CaptionVisible="True" Caption="Change Request Details" Key="ChangeRequestRevenueDetails" AutoCallBack-Enabled="True" AutoCallBack-Target="ChangeRequestRevenueDetailsGrid" AutoCallBack-Command="Refresh">
        <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350"
            SkinID="Horizontal" Height="520px" SavePosition="True">
            <AutoSize Enabled="True" />
            <Template1>
                <px:PXGrid runat="server" SyncPosition="true" SkinID="Inquire" Width="100%" ID="ChangeRequestRevenueDetailsGrid" DataSourceID="ds">
                    <AutoSize Enabled="True"/>
                    <ActionBar>
                        <Actions>
                            <AddNew ToolBarVisible="False" MenuVisible="False" />
                            <Delete ToolBarVisible="Top" MenuVisible="False" />
                            <NoteShow ToolBarVisible="False" MenuVisible="False" />
                        </Actions>
                    </ActionBar>
                    <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
                    <Levels>
                        <px:PXGridLevel DataMember="ChangeRequestRevenueDetails">
                            <Columns>
                                <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeRequest" />
                                <px:PXGridColumn DataField="Description" />
                                <px:PXGridColumn TextAlign="Right" DataField="Qty" />
                                <px:PXGridColumn TextAlign="Right" DataField="UnitPrice" />
                                <px:PXGridColumn TextAlign="Right" DataField="ExtPrice" />
                                <px:PXGridColumn TextAlign="Right" DataField="LineAmount" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                </px:PXGrid>
            </Template1>
            <Template2>
                <px:PXGrid runat="server" SyncPosition="true" SkinID="Inquire" Width="100%" ID="ChangeRequestMarkupDetailsGrid" Caption="Markups" CaptionVisible="true" DataSourceID="ds">
                    <AutoSize Enabled="True" />
                    <ActionBar>
                        <Actions>
                            <AddNew ToolBarVisible="False" MenuVisible="False" />
                            <Delete ToolBarVisible="Top" MenuVisible="False" />
                            <NoteShow ToolBarVisible="False" MenuVisible="False" />
                        </Actions>
                    </ActionBar>
                    <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
                    <Levels>
                        <px:PXGridLevel DataMember="ChangeRequestMarkupDetails">
                            <Columns>
                                <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeRequest" />
                                <px:PXGridColumn DataField="Type" />
                                <px:PXGridColumn DataField="Description" />
                                <px:PXGridColumn TextAlign="Right" DataField="Value" />
                                <px:PXGridColumn TextAlign="Right" DataField="MarkupAmount" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                </px:PXGrid>
            </Template2>
        </px:PXSplitContainer>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="DocumentSettings" >
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Change Requests" VisibleExp="DataControls[&quot;chkIsChangeRequestVisible&quot;].Value == true" BindingContext="VisibilityForm">
                <Template>
                    <px:PXGrid runat="server" ID="gridChangeRequests" SkinID="DetailsInTab" Width="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ChangeRequests">
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeRequest" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="CostTotal" />
                                    <px:PXGridColumn DataField="LineTotal" />
                                    <px:PXGridColumn DataField="MarkupTotal" />
                                    <px:PXGridColumn DataField="PriceTotal" />
                                    <px:PXGridColumn DataField="DelayDays" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowUpdate="False" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Select Change Requests" Tooltip="Select approved change requests">
                                    <AutoCallBack Target="ds" Command="AddChangeRequests">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Revenue Budget" BindingContext="VisibilityForm" VisibleExp="DataControls[&quot;chkIsRevenueVisible&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="RevenueBudgetGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="RevenueBudget">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edProjectTaskIDRevenue" runat="server" DataField="ProjectTaskID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edCostCodeRevenue" runat="server" DataField="CostCodeID" AllowAddNew="true" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edInventoryIDRB" runat="server" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" LinkCommand="ViewRevenueBudgetTask"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" LinkCommand="ViewRevenueBudgetInventory"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ChangeRequestQty" />
                                    <px:PXGridColumn DataField="UOM" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Rate" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ChangeRequestAmount" />
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="PMBudget__Qty" TextAlign="Right"  />                                    
                                    <px:PXGridColumn DataField="PMBudget__CuryAmount" TextAlign="Right"  />
                                    <px:PXGridColumn DataField="PreviouslyApprovedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PreviouslyApprovedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevisedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryInvoicedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__ActualQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryActualAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CompletedPct" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OtherDraftRevisedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="TotalPotentialRevisedAmount" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Change Request Details" Tooltip="Change Request Details">
                                    <AutoCallBack Target="ds" Command="ViewChangeRequestRevenueDetails">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Select budget lines" Tooltip="Select lines from Project Budget">
                                    <AutoCallBack Command="AddRevenueBudget" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Cost Budget">
                <Template>
                     <px:PXGrid ID="CostBudgetGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="CostBudget">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edProjectTaskIDCost" runat="server" DataField="ProjectTaskID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edCostCodeCost" runat="server" DataField="CostCodeID" AllowAddNew="true" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edInventoryIDCB" runat="server" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" LinkCommand="ViewCostBudgetTask"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" LinkCommand="ViewCostBudgetInventory"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="ChangeRequestQty" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="UOM" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Rate" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ChangeRequestAmount" />
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="PMBudget__Qty" TextAlign="Right"  />
                                    <px:PXGridColumn DataField="PMBudget__CuryAmount" TextAlign="Right"  />
                                    <px:PXGridColumn DataField="PreviouslyApprovedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PreviouslyApprovedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevisedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedReceivedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedInvoicedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedInvoicedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedOpenQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedOpenAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedCOQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedCOAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__ActualQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PMBudget__CuryActualAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedCOQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedCOAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OtherDraftRevisedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="TotalPotentialRevisedAmount" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Change Request Details" Tooltip="Change Request Details">
                                    <AutoCallBack Target="ds" Command="ViewChangeRequestCostDetails">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Select budget lines" Tooltip="Select lines from Project Budget">
                                    <AutoCallBack Command="AddCostBudget" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Commitments" LoadOnDemand="True">
                <Template>
                     <px:PXGrid ID="DetailsGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Details">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edProjectTaskIDDetails" runat="server" DataField="TaskID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edCostCodeDetails" runat="server" DataField="CostCodeID" AllowAddNew="true" AutoRefresh="true"/>
                                     <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" 
                                        AutoRefresh="True" />
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType"/>
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edInventoryIDDet" runat="server" DataField="InventoryID" />
                                    <px:PXSelector runat="server" ID="edPOOrderNbr2" DataField="POOrderNbr" AutoRefresh="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ChangeRequestRefNbr" LinkCommand="ViewChangeRequest" />
                                    <px:PXGridColumn DataField="LineType" DisplayMode="Text"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="TaskID" LinkCommand="ViewCommitmentTask"/>
                                    <px:PXGridColumn CommitChanges="True" DataField="CostCodeID" />
                                    <px:PXGridColumn CommitChanges="True" DataField="InventoryID" LinkCommand="ViewCommitmentInventory"/>
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn CommitChanges="true" DataField="Qty" TextAlign="Right" />                                    
                                    <px:PXGridColumn CommitChanges="True" DataField="UOM" />
                                    <px:PXGridColumn CommitChanges="true" DataField="UnitCost" TextAlign="Right" />
                                    <px:PXGridColumn CommitChanges="true" DataField="Amount" TextAlign="Right" />
                                    <px:PXGridColumn CommitChanges="true" DataField="AccountID" />
                                    <px:PXGridColumn CommitChanges="True" DataField="VendorID" />
                                    <px:PXGridColumn DataField="CommitmentType" Type="DropDownList" CommitChanges="True" />
                                    <px:PXGridColumn CommitChanges="True" DataField="POOrderNbr" LinkCommand="ViewCommitments" />
                                    <px:PXGridColumn DataField="POLinePM__OrderDate" />
                                    <px:PXGridColumn CommitChanges="true" DataField="CuryID" />
                                    <px:PXGridColumn CommitChanges="True" DataField="POLineNbr" />
                                    <px:PXGridColumn DataField="POLinePM__TranDesc" TextAlign="Right" />
                                    <px:PXGridColumn DataField="POLinePM__OrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="POLinePM__CuryLineAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="POLinePM__CalcOpenQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="POLinePM__CalcCuryOpenAmt" TextAlign="Right" />                                   
                                    <px:PXGridColumn CommitChanges="true" DataField="AmountInProjectCury" TextAlign="Right" />
                                    <px:PXGridColumn CommitChanges="true" DataField="PotentialRevisedQty" TextAlign="Right" />
                                    <px:PXGridColumn CommitChanges="true" DataField="PotentialRevisedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" Type="CheckBox" />
                                    </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Select Commitments" Tooltip="Select lines from Purchase Orders">
                                    <AutoCallBack Command="AddPOLines" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Detailed Description" LoadOnDemand="True">
                <Template>
                    <px:PXRichTextEdit ID="edText" runat="server" Style="border-width: 0px; width: 100%;" DataField="Text" 
						AllowLoadTemplate="false"  AllowDatafields="false"  AllowMacros="true" AllowSourceMode="true" AllowAttached="true" AllowSearch="true">
						<AutoSize Enabled="True" />
						<LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
					</px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />

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
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" AutoGenerateColumns="Append" DataSourceID="ds" AllowPaging="True" PageSize="12">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                                    <px:PXSelector runat="server" ID="edDocumentTypeValue" DataField="DocumentTypeValue" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edBillID" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edInvoiceID" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edApCheckID" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edArPaymentID" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edProjectTransactionID" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edPurchaseOrder" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                                    <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                                    <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                                    <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                                    <px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ExpirationDate" TextAlign="Left" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CreationDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Required" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Received" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="ReceivedDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="IsProcessed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsVoided" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsCreatedAutomatically" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="SentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocuments_Task_ViewDetails" />
                                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocuments_Task_ViewDetails" />
                                    <px:PXGridColumn DataField="CostCodeID" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" />
                                    <px:PXGridColumn DataField="CustomerID" CommitChanges="True" LinkCommand="ComplianceDocuments_Customer_ViewDetails" />
                                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="VendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillID" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$BillID$Link" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ApCheckID" DisplayMode="Text" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocument$ApCheckID$Link" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentID" DisplayMode="Text" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocument$ArPaymentID$Link" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InvoiceID" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$InvoiceID$Link" />
                                    <px:PXGridColumn DataField="IsExpired" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsRequiredJointCheck" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="JointRelease" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointReleaseReceived" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="LastModifiedByID" />
                                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Limit" TextAlign="Right" />
                                    <px:PXGridColumn DataField="MethodSent" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentMethodID" />
                                    <px:PXGridColumn DataField="ApPaymentMethodID" />
                                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectTransactionID" DisplayMode="Text" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrder" LinkCommand="ComplianceDocument$PurchaseOrder$Link" DisplayMode="Text" CommitChanges="True" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" LinkCommand="ComplianceDocument$Subcontract$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
