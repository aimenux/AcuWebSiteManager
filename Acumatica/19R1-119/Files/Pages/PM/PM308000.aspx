<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM308000.aspx.cs" Inherits="Page_PM308000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ChangeOrderEntry" PrimaryView="Document" BorderStyle="NotSet">
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
            
            <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date"/>
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

            <px:PXFormView ID="VisibilityForm" runat="server" DataMember="VisibilitySettings" DataSourceID="ds" Caption="Hidden Form needed for VisibleExp of TabItems. Tabs are Hidden based on the values of Combo"
                Visible="False" TabIndex="300">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkIsRevenueVisible" runat="server" DataField="IsRevenueVisible"/>
                    <px:PXCheckBox ID="chkIsCostVisible" runat="server" DataField="IsCostVisible"/>
                    <px:PXCheckBox ID="chkIsDetailsVisible" runat="server" DataField="IsDetailsVisible" />
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
                        <px:PXGridColumn DataField="Selected" Label="Selected" Width="63px" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                        <px:PXGridColumn DataField="Description" Width="180px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="UOM" Width="63px" />
                        <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" Width="99px" CommitChanges="true" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCommittedAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedReceivedQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedInvoicedQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedOpenQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCommittedOpenAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="ActualQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryActualAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="Performance" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn DataField="IsProduction" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="CuryLastCostToComplete" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCostToComplete" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="LastPercentCompleted" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="PercentCompleted" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryLastCostAtCompletion" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCostAtCompletion" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="RevenueTaskID" Width="100px" AutoCallBack="True" />
                        <px:PXGridColumn DataField="RevenueInventoryID" Width="100px" AutoCallBack="True" />
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
                        <px:PXGridColumn DataField="Selected" Label="Selected" Width="63px" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                        <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                        <px:PXGridColumn DataField="Description" Width="180px" />
                        <px:PXGridColumn DataField="Type" TextAlign="Right" Width="81px"/>
                        <px:PXGridColumn DataField="Qty" Label="Qty." TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn AutoCallBack="True" DataField="UOM" Label="UOM" Width="63px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryUnitRate" Label="Rate" TextAlign="Right" Width="99px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryAmount" Label="Amount" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="RevisedQty" Label="Revised Qty" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryRevisedAmount" Label="Revised Amount" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="LimitQty" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="LimitAmount" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedQty" Label="Committed Qty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCommittedAmount" Label="Committed Amount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedReceivedQty" Label="Committed Qty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedInvoicedQty" Label="Committed Qty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" Label="Committed Amount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CommittedOpenQty" Label="Committed Open Qty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryCommittedOpenAmount" Label="Committed Open Amount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryInvoicedAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="ActualQty" Label="Actual Qty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryActualAmount" Label="Actual Amount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" Width="100px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryPrepaymentAmount" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryPrepaymentInvoiced" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryPrepaymentAvailable" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" Width="100px" CommitChanges="true" />
                        <px:PXGridColumn DataField="CuryAmountToInvoice" TextAlign="Right" Width="81px" CommitChanges="true" />
                        <px:PXGridColumn DataField="Performance" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn DataField="TaxCategoryID" Width="100px" />
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
                        <px:PXGridColumn DataField="Selected" Label="Selected" Width="63px" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn DataField="TaskID" Width="108px" />
                        <px:PXGridColumn DataField="CostCodeID" Width="108px" />
                        <px:PXGridColumn DataField="InventoryID" Width="108px" />
                        <px:PXGridColumn DataField="SubItemID" Width="81px" />                       
                        <px:PXGridColumn DataField="VendorID" Width="108px" />
                        <px:PXGridColumn DataField="OrderNbr" Width="108px" />
                        <px:PXGridColumn DataField="OrderDate" Width="81px" />
                        <px:PXGridColumn DataField="CuryID" Width="81px" />
                        <px:PXGridColumn DataField="LineNbr" Width="108px" />
                        <px:PXGridColumn DataField="TranDesc" Width="180px" />
                        <px:PXGridColumn DataField="OrderQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="UOM" Width="63px" />
                        <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" Width="99px" />
                        <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" Width="81px" />
                        <px:PXGridColumn DataField="CuryExtCost" TextAlign="Right" Width="81px" />
                        
                        <px:PXGridColumn DataField="PromisedDate" Width="81px" />
                        <px:PXGridColumn DataField="VendorRefNbr" Width="108px" />
                        <px:PXGridColumn DataField="AlternateID" Width="108px" />
                        <px:PXGridColumn DataField="Cancelled" Width="63px" Type="CheckBox" />
                        <px:PXGridColumn DataField="Completed" Width="63px" Type="CheckBox"/>
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
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="DocumentSettings" >
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items >
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="108px" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="UOM" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Rate" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="PMBudget__Qty" TextAlign="Right" Width="81px"  />
                                    <px:PXGridColumn DataField="PMBudget__CuryAmount" TextAlign="Right" Width="81px"  />
                                    <px:PXGridColumn DataField="PreviouslyApprovedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PreviouslyApprovedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="RevisedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryInvoicedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__ActualQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryActualAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CompletedPct" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="OtherDraftRevisedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="TotalPotentialRevisedAmount" TextAlign="Right" Width="81px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="108px" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="UOM" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Rate" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="PMBudget__Qty" TextAlign="Right" Width="81px"  />
                                    <px:PXGridColumn DataField="PMBudget__CuryAmount" TextAlign="Right" Width="81px"  />
                                    <px:PXGridColumn DataField="PreviouslyApprovedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PreviouslyApprovedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="RevisedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedReceivedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedInvoicedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedInvoicedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedOpenQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedOpenAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CommittedCOQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryCommittedCOAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__ActualQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PMBudget__CuryActualAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedCOQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedCOAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="OtherDraftRevisedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="TotalPotentialRevisedAmount" TextAlign="Right" Width="81px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
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
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineType" Width="81px" DisplayMode="Text"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="TaskID" Width="108px" />
                                    <px:PXGridColumn CommitChanges="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn CommitChanges="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description" Width="108px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="Qty" TextAlign="Right" Width="81px" />                                    
                                    <px:PXGridColumn CommitChanges="True" DataField="UOM" Width="108px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="UnitCost" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="Amount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="AccountID" Width="81px" />
                                    <px:PXGridColumn CommitChanges="True" DataField="VendorID" Width="108px" />
                                    <px:PXGridColumn CommitChanges="True" DataField="POOrderNbr" Width="108px" LinkCommand="ViewCommitments" />
                                    <px:PXGridColumn DataField="POLinePM__OrderDate" Width="81px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="CuryID" Width="81px" />
                                    <px:PXGridColumn CommitChanges="True" DataField="POLineNbr" Width="108px" />
                                    <px:PXGridColumn DataField="POLinePM__TranDesc" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="POLinePM__OrderQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="POLinePM__CuryLineAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="POLinePM__CalcOpenQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="POLinePM__CalcCuryOpenAmt" TextAlign="Right" Width="81px" />                                   
                                    <px:PXGridColumn CommitChanges="true" DataField="AmountInProjectCury" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="PotentialRevisedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn CommitChanges="true" DataField="PotentialRevisedAmount" TextAlign="Right" Width="81px" />
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
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False" TextField="AttributeID_description" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="80px"/>
                                    <px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
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
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" Width="160px" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="WorkgroupID" Width="150px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" Width="100px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApproveDate" Width="90px" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" Width="160px" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false" Width="160px"/>
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" Width="160px" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" Width="160px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" Width="100px" />
                                </Columns>
                                <Layout FormViewHeight="" />
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
