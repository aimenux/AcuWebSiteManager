<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS305700.aspx.cs" Inherits="Page_FS305700" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ServiceContractRecords" 
        TypeName="PX.Objects.FS.ServiceContractEntry" PageLoadBehavior="InsertRecord"
         SuspendUnloading="False">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="MenuActions" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="Inquiry" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="AddSchedule" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenScheduleScreen" Visible="False" />
            <px:PXDSCallbackCommand Name="ActivatePeriod" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenScheduleScreenByScheduleDetService" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenScheduleScreenByScheduleDetPart" Visible="False" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">

    <%-- Activate Contract --%>
    <px:PXSmartPanel
        ID="PXSmartPanelActivationContract"
        runat="server"
        Caption="Activate Contract"
        CaptionVisible="True"
        Key="ActivationContractFilter"
        AutoCallBack-Command="Refresh"
        ShowAfterLoad="True"
        Width="860px"
        Height="450px"
        CloseAfterAction ="True"
        AutoReload="True"
        LoadOnDemand="True"
        TabIndex="8500"
        AutoRepaint="true"
        CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" 
        AllowResize="False">
        <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
        <px:PXFormView ID="PXFormActivateDate" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="40px" DataMember="ActivationContractFilter" TabIndex="700" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"></px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edActivationDate" runat="server" DataField="ActivationDate" CommitChanges="True"></px:PXDateTimeEdit>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="PXGridSchedulesActivation" runat="server" DataSourceID="ds" Style="z-index: 100" Width="800px" SkinID="Inquire" TabIndex="900"
            SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Height="300px" AutoAdjustColumns="True" NoteIndicator="False" FilesIndicator="False">
            <Levels>
                <px:PXGridLevel DataMember="ActiveScheduleRecords" DataKeyNames="RefNbr">
                    <RowTemplate>
                        <px:PXSelector ID="edSMAC_RefNbr" runat="server" AllowEdit="True" DataField="RefNbr"></px:PXSelector>
                        <px:PXTextEdit ID="edSMAC_RecurrenceDescription" runat="server" DataField="RecurrenceDescription"></px:PXTextEdit>
                        <px:PXCheckBox ID="edSMAC_ChangeRecurrence" runat="server" DataField="ChangeRecurrence" Text="Change Recurrence" CommitChanges="True"></px:PXCheckBox>
                        <px:PXDateTimeEdit ID="edSMAC_EffectiveRecurrenceStartDate" runat="server" DataField="EffectiveRecurrenceStartDate" CommitChanges="True"></px:PXDateTimeEdit>
                        <px:PXDateTimeEdit ID="edSMAC_NextExecution" runat="server" DataField="NextExecution"></px:PXDateTimeEdit>
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenScheduleScreen"></px:PXGridColumn>
                        <px:PXGridColumn DataField="RecurrenceDescription"></px:PXGridColumn>
                        <px:PXGridColumn DataField="ChangeRecurrence" TextAlign="Center" Type="CheckBox" CommitChanges="True"></px:PXGridColumn>
                        <px:PXGridColumn DataField="EffectiveRecurrenceStartDate" CommitChanges="True"></px:PXGridColumn>
                        <px:PXGridColumn DataField="NextExecution"></px:PXGridColumn>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="350" />
            <Mode AllowAddNew="False" AllowDelete="False" />
        </px:PXGrid>
        <px:PXFormView ID="PXFormView2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="40px" DataMember="ActivationContractFilter" TabIndex="700" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartRow="True" Merge="true"></px:PXLayoutRule>
                <px:PXButton ID="okActivationContract" runat="server" DialogResult="Ok" Text="Ok" AlignLeft="True"></px:PXButton>
                <px:PXButton ID="closeActivationContract" runat="server" DialogResult="Cancel" Text="Close" AlignLeft="True"></px:PXButton>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
    <%--/ Activate Contract --%>

    <%-- Terminate Contract --%>
    <px:PXSmartPanel
        ID="PXSmartPanelTerminateContract"
        runat="server"
        Caption="Terminate Contract"
        CaptionVisible="True"
        Key="TerminateContractFilter"
        AutoCallBack-Command="Refresh"
        ShowAfterLoad="True"
        Width="400px"
        Height="150px"
        CloseAfterAction ="True"
        AutoReload="True"
        LoadOnDemand="True"
        TabIndex="8500"
        AutoRepaint="true"
        CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" 
        AllowResize="False">
        <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
        <px:PXFormView ID="PXFormCancelationDate" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="80px" DataMember="TerminateContractFilter" TabIndex="700" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"></px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edCancelationDate" runat="server" DataField="CancelationDate" CommitChanges="True"></px:PXDateTimeEdit>
                <px:PXLabel Height="10px" runat="server"></px:PXLabel>
                <px:PXLabel Size="XXL" Height="30px" ID="lblMessage" runat="server">Canceling a contract will delete the associated appointments and service orders with a date later than or equal to the effective date.</px:PXLabel>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelTerminateContractButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="okTerminateContractButton" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
            <px:PXButton ID="cancelTerminateContractButton" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%--/ Terminate Contract --%>

    <%-- Suspend Contract --%>
    <px:PXSmartPanel
        ID="PXSmartPanelSuspendContract"
        runat="server"
        Caption="Suspend Contract"
        CaptionVisible="True"
        Key="SuspendContractFilter"
        AutoCallBack-Command="Refresh"
        ShowAfterLoad="True"
        Width="400px"
        Height="150px"
        CloseAfterAction ="True"
        AutoReload="True"
        LoadOnDemand="True"
        TabIndex="8500"
        AutoRepaint="true"
        CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" 
        AllowResize="False">
        <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
        <px:PXFormView ID="PXFormSuspensionDate" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="80px" DataMember="SuspendContractFilter" TabIndex="700" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"></px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edSuspensionDate" runat="server" DataField="SuspensionDate" CommitChanges="True"></px:PXDateTimeEdit>
                <px:PXLabel Height="10px" runat="server"></px:PXLabel>
                <px:PXLabel Size="XXL" Height="30px" ID="lblMessage2" runat="server">Suspending a contract will delete the associated appointments and service orders with a date later than or equal to the effective date.</px:PXLabel>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelSuspendContractButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="okSuspendContractButton" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
            <px:PXButton ID="cancelSuspendContractButton" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%--/ Suspend Contract --%>

    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Caption="Service Contract" NoteIndicator="True" NotifyIndicator="True" FilesIndicator="True"
        ActivityIndicator="True" DataMember="ServiceContractRecords" DefaultControlID="edCustomerID" AllowCollapse="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" 
                StartRow="True" LabelsWidth="SM">
            </px:PXLayoutRule>          
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" FilterByAllFields="True" AutoRefresh="true">
            </px:PXSelector>
            <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" 
                AllowEdit="True" CommitChanges="True">
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edCustomerLocationID" runat="server" 
                DataField="CustomerLocationID" CommitChanges="True">
            </px:PXSegmentMask>
            <px:PXSelector ID="edCustomerContractNbr" runat="server" DataField="CustomerContractNbr">
            </px:PXSelector>
            <px:PXSegmentMask ID="edProjectID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="ProjectID" AllowEdit="True">
            </px:PXSegmentMask>
            <px:PXSelector ID="edDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID" AllowEdit = "True" AutoRefresh="True" CommitChanges="True" DisplayMode="Hint">
            </px:PXSelector>
            <px:PXSelector ID="edMasterContractID" runat="server" AutoRefresh="true" DataField="MasterContractID">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S">
            </px:PXLayoutRule>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status">
            </px:PXDropDown>
            <px:PXDateTimeEdit ID="edStatusEffectiveFromDate" runat="server" DataField="StatusEffectiveFromDate">
            </px:PXDateTimeEdit>
            <px:PXDropDown ID="edUpcoming" runat="server" DataField="UpcomingStatus">
            </px:PXDropDown>
            <px:PXDateTimeEdit ID="edStatusEffectiveUntilDate" runat="server" DataField="StatusEffectiveUntilDate">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" >
            </px:PXTextEdit>
            <px:PXCheckBox ID="edShowPriceTab" runat="server" Visible="False"
                DataField="Mem_ShowPriceTab">
            </px:PXCheckBox>
            <px:PXCheckBox ID="edShowScheduleTab" runat="server" Visible="False"
                DataField="Mem_ShowScheduleTab">
            </px:PXCheckBox>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" 
        DataMember="ContractSchedules" Style="z-index: 100">
        <Items>
            <px:PXTabItem Text="Summary" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True">
                    </px:PXLayoutRule>
                    <px:PXFormView runat="server" Caption="Location"
                        DataMember="ServiceContractRecords" RenderStyle="Simple" DataSourceID="ds"
                        ID="ContractSummaryFormView">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                            </px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" GroupCaption="Contract Settings" StartGroup="True">
                            </px:PXLayoutRule>
                            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True">
                            </px:PXDateTimeEdit>
                            <px:PXDropDown ID="edExpirationType" runat="server" DataField="ExpirationType" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True">
                            </px:PXDateTimeEdit>
                            <px:PXDropDown ID="edScheduleGenType" runat="server" DataField="ScheduleGenType" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" AllowEdit="True">
                            </px:PXSegmentMask>
                            <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" CommitChanges="True" AllowEdit="True">
                            </px:PXSegmentMask>
                            <px:PXCheckBox ID="edCommissionable" runat="server" CommitChanges="True" DataField="Commissionable">
                            </px:PXCheckBox>
                            <px:PXLayoutRule runat="server" GroupCaption="Billing Settings" StartGroup="True">
                            </px:PXLayoutRule>
                            <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edBranchLocationID" runat="server" AllowEdit="True" 
                                DataField="BranchLocationID" AutoRefresh="True" CommitChanges="True">
                            </px:PXSelector>
                            <px:PXDropDown ID="edBillingType" runat="server" DataField="BillingType" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXDropDown ID="edBillTo" runat="server" DataField="BillTo" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXSegmentMask ID="edBillCustomerID" runat="server" DataField="BillCustomerID" AllowEdit="True" CommitChanges="True">
                            </px:PXSegmentMask>
                            <px:PXSegmentMask ID="edBillLocationID" runat="server" AllowEdit="True" DataField="BillLocationID" AutoRefresh="True">
                            </px:PXSegmentMask>
                            <px:PXSelector ID="edUsageBillingCycle" runat="server" DataField="UsageBillingCycleID">
                            </px:PXSelector>
                            <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Standardized Billing Settings" StartGroup="True" LabelsWidth="SM" ControlSize="XM">
                            </px:PXLayoutRule>
                            <px:PXDropDown ID="edBillingPeriod" runat="server" DataField="BillingPeriod" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXDateTimeEdit ID="edLastBillingInvoiceDate" runat="server" DataField="LastBillingInvoiceDate">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edNextBillingInvoiceDate" runat="server" DataField="NextBillingInvoiceDate">
                            </px:PXDateTimeEdit>
                            <px:PXLayoutRule runat="server" GroupCaption="As Performed Settings" StartGroup="True" LabelsWidth="SM" ControlSize="XM">
                            </px:PXLayoutRule>
                            <px:PXDropDown ID="edSourcePrice" runat="server" DataField="SourcePrice">
                            </px:PXDropDown>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Schedules" BindingContext="form" visibleExp="DataControls[&quot;edShowScheduleTab&quot;].Value == false">
                <Template>
                    <px:PXGrid ID="PXGridSchedule" runat="server" AutoAdjustColumns="True" 
                        DataSourceID="ds" Height="100%" SkinID="Inquire" TabIndex="4200" Width="100%" KeepPosition="True" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="RefNbr" DataMember="ContractSchedules">
                                <RowTemplate>
                                    <px:PXSelector ID="edRefNbrSchedule" runat="server" AllowEdit="True" DataField="RefNbr">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edSrvOrdType" runat="server" AllowEdit="True" 
                                        DataField="SrvOrdType">
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edActiveSchedule" runat="server" DataField="Active" Text="Active">
                                    </px:PXCheckBox>
                                    <px:PXTextEdit ID="edRecurrenceDescription" runat="server" DataField="RecurrenceDescription">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="edCustomerLocationID" runat="server" AllowEdit="True" 
                                        DataField="CustomerLocationID">
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand ="OpenScheduleScreen">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduleGenType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SrvOrdType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CustomerLocationID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RecurrenceDescription">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Schedule">
                                    <AutoCallBack Command="AddSchedule" Target="ds" ></AutoCallBack>
                                    <PopupCommand Command="Refresh" Target="PXGridSchedule" ></PopupCommand>                                                                        
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Services per Period" LoadOnDemand="True" RepaintOnDemand="True" BindingContext="form" VisibleExp="DataControls[&quot;edShowPriceTab&quot;].Value == false">
                <Template>
                    <px:PXFormView ID="SBP_form" runat="server" DataSourceID="ds" Style="z-index: 100"
                        Width="100%" Caption="Service Contract" DataMember="ContractPeriodFilter">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize = "M" StartRow="True" LabelsWidth="S">
                            </px:PXLayoutRule>  
                            <px:PXDropDown ID="edSBP_Actions" runat="server" DataField="Actions" CommitChanges="True" >
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize = "M" LabelsWidth="S">
                            </px:PXLayoutRule> 
                            <px:PXSelector ID="edSBP_ContractPeriodID" runat="server" DataField="ContractPeriodID" AutoRefresh="True" CommitChanges="True">
                            </px:PXSelector>
                            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize = "M" LabelsWidth="S">
                            </px:PXLayoutRule> 
                            <px:PXTextEdit ID="edSBP_PostDocRefNbr" runat="server" DataField="PostDocRefNbr">
                            </px:PXTextEdit>
                            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize = "S" LabelsWidth="M">
                            </px:PXLayoutRule> 
                            <px:PXNumberEdit  ID="edSBP_StandardizedBillingTotal" runat="server" DataField="StandardizedBillingTotal"></px:PXNumberEdit >
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" ID="gridServBillingPeriod" SkinID="DetailsInTab" Width="100%" AllowPaging="True" AdjustPageSize="Auto" 
                    FilesIndicator="False" NoteIndicator="False" Height="200px" TabIndex="11300" DataSourceID="ds" SyncPosition="True" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ContractPeriodDetRecords">
                                <Columns>
                                    <px:PXGridColumn DataField="LineType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SMequipmentID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="BillingRule" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Amount" CommitChanges="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Time" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Qty" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RegularPrice" />
                                    <px:PXGridColumn DataField="RecurringUnitPrice" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RecurringTotalPrice" CommitChanges="True" />
                                    <px:PXGridColumn DataField="OverageItemPrice" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RemainingAmount" CommitChanges="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RemainingTime" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RemainingQty" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UsedAmount" CommitChanges="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UsedTime" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UsedQty" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ScheduledAmount" CommitChanges="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ScheduledTime" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ScheduledQty" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" DisplayMode="Hint"/>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXDropDown runat="server" ID="edSBP_LineType" DataField="LineType" CommitChanges="True"/>
                                    <px:PXSegmentMask runat="server" ID="edSBP_InventoryID" DataField="InventoryID" CommitChanges="true" AllowEdit="True" AutoRefresh="True"/>
                                    <px:PXSelector runat="server" ID="edSBP_SMequipmentID" DataField="SMequipmentID" CommitChanges="true" AllowEdit="True" AutoRefresh="True"/>
                                    <px:PXDropDown runat="server" ID="edSBPBillingRule" DataField="BillingRule" />
                                    <px:PXTextEdit ID="edSBP_Amount" runat="server" DataField="Amount"></px:PXTextEdit>
                                    <px:PXMaskEdit ID="edSBP_Time" runat="server" DataField="Time"></px:PXMaskEdit>
                                    <px:PXTextEdit ID="edSBP_Qty" runat="server" DataField="Qty"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edSBP_RecurringUnitPrice" runat="server" DataField="RecurringUnitPrice"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edSBP_RecurringTotalPrice" runat="server" DataField="RecurringTotalPrice"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edSBP_OverageItemPrice" runat="server" DataField="OverageItemPrice"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edSBP_RemainingAmount" runat="server" DataField="RemainingAmount"></px:PXTextEdit>
                                    <px:PXMaskEdit ID="edSBP_RemainingTime" runat="server" DataField="RemainingTime"></px:PXMaskEdit>
                                    <px:PXTextEdit ID="edSBP_RemainingQty" runat="server" DataField="RemainingQty"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edSBP_UsedAmount" runat="server" DataField="UsedAmount"></px:PXTextEdit>
                                    <px:PXMaskEdit ID="edSBP_UsedTime" runat="server" DataField="UsedTime"></px:PXMaskEdit>
                                    <px:PXTextEdit ID="edSBP_UsedQty" runat="server" DataField="UsedQty"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edSBP_ScheduledAmount" runat="server" DataField="ScheduledAmount"></px:PXTextEdit>
                                    <px:PXMaskEdit ID="edSBP_ScheduledTime" runat="server" DataField="ScheduledTime"></px:PXMaskEdit>
                                    <px:PXNumberEdit ID="edSBP_ScheduledQty" runat="server" DataField="ScheduledQty"></px:PXNumberEdit>
                                    <px:PXSelector ID="edSBPProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True" DisplayMode="Hint">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edSBPCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="Activate Period">
                                    <AutoCallBack Command="ActivatePeriod" Target="ds" ></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Prices" BindingContext="form" VisibleExp="DataControls[&quot;edShowPriceTab&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="PXGridSalesPriceLines" runat="server" DataSourceID="ds" 
                        Height="100%" SkinID="Inquire" TabIndex="4200" AutoAdjustColumns="True"
                        Width="100%" NoteIndicator="False" FilesIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="SalesPriceID,InventoryItem__InventoryCD" DataMember="SalesPriceLines">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edInventoryItem__InventoryCD" runat="server" DataField="InventoryItem__InventoryCD" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edLineType2" runat="server" DataField="LineType">
                                    </px:PXTextEdit>
                                    <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="Mem_UnitPrice">
                                    </px:PXNumberEdit>
                                    <px:PXSelector ID="edUOMSrvContract" runat="server" DataField="UOM">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID">
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryItem__InventoryCD">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LineType">
                                    </px:PXGridColumn> 
                                    <px:PXGridColumn DataField="Mem_UnitPrice" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UOM">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryID">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Contract History" LoadOnDemand="True" RepaintOnDemand="False" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="PXGridContractHistory" SkinID="Inquire" Width="100%" AllowPaging="True" AdjustPageSize="Auto" FilesIndicator="False" NoteIndicator="False" TabIndex="11300" DataSourceID="ds" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ContractHistoryItems">
                                <Columns>
                                    <px:PXGridColumn DataField="Type" />
                                    <px:PXGridColumn DataField="Action" />
                                    <px:PXGridColumn DataField="ActionBusinessDate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ScheduleRefNbr" />
                                    <px:PXGridColumn DataField="ScheduleChangeRecurrence" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="ScheduleNextExecutionDate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ScheduleRecurrenceDescr" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXDropDown runat="server" ID="edCH_Type" DataField="Type" />
                                    <px:PXDropDown runat="server" ID="edCH_Action" DataField="Action" />
                                    <px:PXDateTimeEdit runat="server" ID="edCH_ActionBusinessDate" DataField="ActionBusinessDate" />
                                    <px:PXDateTimeEdit runat="server" ID="edCH_EffectiveDate" DataField="EffectiveDate" />
                                    <px:PXTextEdit runat="server" ID="edCH_ScheduleRefNbr" DataField="ScheduleRefNbr" />
                                    <px:PXCheckBox runat="server" ID="edCH_ScheduleChangeRecurrence" DataField="ScheduleChangeRecurrence"></px:PXCheckBox>
                                    <px:PXDateTimeEdit runat="server" ID="edCH_ScheduleNextExecutionDate" DataField="ScheduleNextExecutionDate" />
                                    <px:PXTextEdit runat="server" ID="edCH_ScheduleRecurrenceDescr" DataField="ScheduleRecurrenceDescription" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Value" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>