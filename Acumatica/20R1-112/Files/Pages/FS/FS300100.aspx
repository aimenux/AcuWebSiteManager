<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS300100.aspx.cs" Inherits="Page_FS300100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ServiceOrderRecords" TypeName="PX.Objects.FS.ServiceOrderEntry">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ActionsMenu" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReportsMenu" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewDirectionOnMap" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenSource" Visible="False" />
            <px:PXDSCallbackCommand Name="createNewCustomer" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenStaffSelectorFromServiceTab" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="OpenStaffSelectorFromStaffTab" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="OpenServiceSelector" Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="OpenAppointmentScreen" Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="SelectCurrentService" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenServiceOrderScreen" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenPostingDocument" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenINPostingDocument" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenInvoiceDocument" Visible="False" />
            <px:PXDSCallbackCommand Name="OpenBatch" Visible="False" />
            <px:PXDSCallbackCommand Name="QuickProcessOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Name="QuickProcess" ShowProcessInfo="true" />
            <px:PXDSCallbackCommand Name="CreatePrepayment" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenRoomBoard" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewPayment" Visible="False" />
            <px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Name="FSSODetSplit$RefNoteID$Link" DependOnGrid="PXGridAllocations" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="LSFSSODetLine_generateLotSerial" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="LSFSSODetLine_binLotSerial" DependOnGrid="PXGridDetails" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="DetailsPasteLine" Visible="False" CommitChanges="True" DependOnGrid="PXGridDetails" />
            <px:PXDSCallbackCommand Name="DetailsResetOrder" Visible="False" CommitChanges="True" DependOnGrid="PXGridDetails" />
            <px:PXDSCallbackCommand Name="OpenScheduleScreen" Visible="False" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="TreeWFStages" TreeKeys="WFStageID"/>
        </DataTrees>
    </px:PXDataSource>

    <%-- QuickProcess Smartpanel --%>
    <px:PXSmartPanel 
        ID="PXSmartPanelQuickProcess" 
        runat="server"
        Caption="Process Service Order"
        ShowAfterLoad="true"
        CaptionVisible="true"
        DesignView="Hidden"
        Key="QuickProcessParameters"
        AutoCallBack-Enabled="true"
        AutoCallBack-Target="formQuickProcess"
        AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="formQuickProcess" runat="server" DataSourceID="ds" Style="z-index: 100" CaptionVisible="False" DataMember="QuickProcessParameters" AllowCollapse="False" Width="700px">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" StartGroup="True" GroupCaption="Service Order Actions" SuppressLabel="True" ColumnWidth="300" />
                <px:PXCheckBox ID="edAllowInvoiceServiceOrder" runat="server" DataField="AllowInvoiceServiceOrder" CommitChanges="True"/>
                <px:PXCheckBox ID="edCompleteServiceOrder" runat="server" DataField="CompleteServiceOrder" CommitChanges="True"/>
                <px:PXCheckBox ID="edCloseServiceOrder" runat="server" DataField="CloseServiceOrder" CommitChanges="True"/>
                <px:PXCheckBox ID="edGenerateInvoiceFromServiceOrder" runat="server" DataField="GenerateInvoiceFromServiceOrder" CommitChanges="True"/>
                <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Sales Order Actions" SuppressLabel="True" ColumnWidth="300" />
                <px:PXCheckBox ID="edPrepareInvoice" runat="server" AlignLeft="True" DataField="PrepareInvoice" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edSOQuickProcess" runat="server" AlignLeft="True" DataField="SOQuickProcess" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edEmailSalesOrder" runat="server" AlignLeft="True" DataField="EmailSalesOrder" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXLayoutRule StartGroup="True" GroupCaption="Invoice Actions" runat="server">
                </px:PXLayoutRule>
                <px:PXCheckBox ID="edReleaseInvoice" runat="server" AlignLeft="True" DataField="ReleaseInvoice" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edEmailInvoice" runat="server" AlignLeft="True" DataField="EmailInvoice" CommitChanges="True">
                </px:PXCheckBox>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel9" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" CommandName="QuickProcessOk" CommandSourceID="ds" SyncVisible="False"/>
        </px:PXPanel>
    </px:PXSmartPanel>
    <%--/ QuickProcess Smartpanel --%>

    <%-- Employee Selector --%>
    <px:PXSmartPanel ID="PXSmartPanelStaffSelector" runat="server" Caption="Add Staff" CaptionVisible="True" Key="StaffSelectorFilter"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGridStaff" ShowAfterLoad="True" Width="1070px" Height="760px" CloseAfterAction="True"
        AutoReload="True" LoadOnDemand="True" TabIndex="8500" AutoRepaint="true" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AllowResize="True">
        <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100px" DataMember="StaffSelectorFilter" TabIndex="700" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>
                <px:PXSelector ID="edServiceLineRef2" runat="server" CommitChanges="True" DataField="ServiceLineRef" AutoRefresh="True" DisplayMode="Value"/>
                <px:PXTextEdit ID="edPostalCode" runat="server" DataField="PostalCode" SuppressLabel="False"/>
                <px:PXSelector ID="edGeoZoneID" runat="server" DataField="GeoZoneID" CommitChanges = "True"/>
                <px:PXPanel ID="PXPanelGrids" runat="server" RenderStyle="Simple">
                    <px:PXGrid ID="PXGridServiceSkills" runat="server" DataSourceID="ds" Style="z-index: 100" Width="600px" SkinID="Inquire" TabIndex="900"
                     SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True">
                        <Levels>
                            <px:PXGridLevel DataMember="SkillGridFilter" DataKeyNames="SkillCD">
                                <Columns>
                                    <px:PXGridColumn DataField="Mem_Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
                                    <px:PXGridColumn DataField="SkillCD"/>
                                    <px:PXGridColumn DataField="Descr"/>
                                    <px:PXGridColumn DataField="Mem_ServicesList"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="300" />
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
                    <px:PXGrid ID="PXGridLicenseType" runat="server" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" 
                    SkinID="Inquire" Style="z-index: 100" SyncPosition="True" TabIndex="910" Width="600px" AutoAdjustColumns="True" FilesIndicator="false" NoteIndicator ="false">
                        <Levels>
                            <px:PXGridLevel DataMember="LicenseTypeGridFilter" DataKeyNames="LicenseTypeCD">
                                <Columns>
                                    <px:PXGridColumn DataField="Mem_Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="LicenseTypeCD"/>
                                    <px:PXGridColumn DataField="Descr"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="300" />
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartColumn="True"/>
                    <px:PXGrid ID="PXGridAvailableStaff" runat="server" DataSourceID="ds" Style="z-index: 100" Width="380px" SkinID="Inquire" TabIndex="500"
                        SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True" FilesIndicator="false" NoteIndicator ="false">
                        <Levels>
                          <px:PXGridLevel DataMember="StaffRecords" DataKeyNames="AcctCD">
                            <Columns>
                                <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="true"/>
                                <px:PXGridColumn DataField="Type"/>
                                <px:PXGridColumn DataField="AcctCD"/>
                                <px:PXGridColumn DataField="AcctName"/>
                            </Columns>
                          </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="602"/>
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartRow="True">
                    </px:PXLayoutRule>
                    <px:PXPanel ID="pnlCloseButton" runat="server" SkinID="Buttons">
                        <px:PXButton ID="closeStaffSelector" runat="server" DialogResult="Cancel" Text="OK"/>
                    </px:PXPanel>
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
    <%--/ Employee Selector --%>
    
    <%-- Service Selector --%>
    <px:PXSmartPanel ID="PXSmartPanelServiceSelector" runat="server" Caption="Add Services" CaptionVisible="True" Key="ServiceSelectorFilter"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGridStaffSelected" ShowAfterLoad="True" Width="600px" ShowMaximizeButton="True"
        CloseAfterAction="True" AutoReload="True" HideAfterAction="False" LoadOnDemand="True" TabIndex="8500" AutoRepaint="True" MinHeight="300" >
        <px:PXFormView ID="PXFormViewFilter" runat="server"  TabIndex="1600" SkinID="Transparent"
            DataMember="ServiceSelectorFilter" DataSourceID="ds">
            <Template>
                <px:PXSelector ID="edServiceClassID" runat="server" DataField="ServiceClassID"
                    DataSourceID="ds" Size="M" AutoRefresh="True" CommitChanges="True">
                </px:PXSelector>
                <px:PXLabel runat="server" />
                <px:PXGrid ID="PXGridSelectedEmployees" runat="server" DataSourceID="ds"
                    Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="800" AutoAdjustColumns="True"
                    SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Height="200px">
                    <Levels>
                        <px:PXGridLevel DataMember="EmployeeGridFilter" DataKeyNames="EmployeeID">
                            <Columns>
                                <px:PXGridColumn DataField="Mem_Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                <px:PXGridColumn DataField="EmployeeID"/>
                                <px:PXGridColumn DataField="EmployeeID_BAccountStaffMember_acctName"/>
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="250" />
                    <Mode AllowAddNew="False" AllowDelete="False" />
                </px:PXGrid>
            </Template>
        </px:PXFormView>
        <px:PXLayoutRule runat="server" StartRow="True">
        </px:PXLayoutRule>
        <px:PXGrid ID="PXGridAvailableServices" runat="server" DataSourceID="ds" AutoAdjustColumns="True"
                Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="500"
                SyncPosition = "True" AllowPaging="True" AdjustPageSize="Auto" Height="200px">
            <Levels>
                <px:PXGridLevel DataMember="ServiceRecords" DataKeyNames="InventoryCD">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryCD"/>
                        <px:PXGridColumn DataField="Descr"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="250"/>
            <ActionBar ActionsText="False" DefaultAction="SelectCurrentService"  PagerVisible="False">
                <CustomItems>
                    <px:PXToolBarButton Key="SelectCurrentService">
                        <AutoCallBack Target="ds" Command="SelectCurrentService"/>
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
        <px:PXLayoutRule runat="server" StartRow="True">
        </px:PXLayoutRule>
        <px:PXPanel ID="pnlCloseButton" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonCloseServiceSelector" runat="server" DialogResult="Cancel" Text="OK"/>
        </px:PXPanel>
    </px:PXSmartPanel>
    <%--/ Service Selector --%>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <%--Service Order Type Selector--%>
    <px:PXSmartPanel
        ID="ServiceOrderTypeSelector"
        runat="server"
        Caption="Select the new Service Order Type"
        CaptionVisible="True"
        Key="ServiceOrderTypeSelector"
        TabIndex="17900"
        ShowAfterLoad="True">
        <px:PXFormView ID="DriverRouteForm" runat="server" DataMember="ServiceOrderTypeSelector" DataSourceID="ds" TabIndex="1600" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartRow="True" ControlSize="SM" LabelsWidth="SM" />
                <px:PXSelector ID="edSrvOrdType" runat="server" DataSourceID="ds" DataField="SrvOrdType" AutoRefresh="True" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelCopyToServiceOrder" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Proceed" />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%--/Service Order Type Selector--%>
    <px:PXFormView ID="mainForm" runat="server" DataSourceID="ds" Width="100%" Caption="Service Order" MarkRequired="Dynamic" NotifyIndicator="True" FilesIndicator="True" NoteIndicator="True" ActivityIndicator="True" DataMember="ServiceOrderRecords" TabIndex="100" DefaultControlID="edCustomerID">
        <Template>
            <px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True" StartRow="True">
            </px:PXLayoutRule>
            <px:PXSelector ID="edSrvOrdType" runat="server" AllowEdit="True"
                AutoRefresh="True" DataField="SrvOrdType" DataSourceID="ds">
            </px:PXSelector>
            <px:PXSelector ID="edRefNbr" runat="server" AutoRefresh="True"
                DataField="RefNbr" DataSourceID="ds">
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False">
            </px:PXDropDown>
            <px:PXTreeSelector CommitChanges="True" ID="edWFStageID" runat="server" DataField="WFStageID"
                TreeDataMember="TreeWFStages" TreeDataSourceID="ds" PopulateOnDemand="True"
                InitialExpandLevel="0" ShowRootNode="False">
                <DataBindings>
                    <px:PXTreeItemBinding TextField="WFStageCD" ValueField="WFStageCD">
                    </px:PXTreeItemBinding>
                </DataBindings>
            </px:PXTreeSelector>
            <px:PXCheckBox ID="edHold" runat="server" DataField="Hold" Text="Hold" CommitChanges="True">
            </px:PXCheckBox>
            <px:PXDateTimeEdit ID="edOrderDate" runat="server" DataField="OrderDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXTextEdit ID="edCustPORefNbr" runat="server" DataField="CustPORefNbr" AutoRefresh="True">
            </px:PXTextEdit>
            <px:PXTextEdit ID="edCustWorkOrderRefNbr" runat="server" DataField="CustWorkOrderRefNbr" AutoRefresh="True">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM">
            </px:PXLayoutRule>
            <px:PXSegmentMask ID="edCustomerID" runat="server" AllowEdit="True"
                CommitChanges="True" DataField="CustomerID" DataSourceID="ds">
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edLocationID" runat="server" AllowEdit="True"
                AutoRefresh="True" CommitChanges="True" DataField="LocationID"
                DataSourceID="ds">
            </px:PXSegmentMask>
            <pxa:PXCurrencyRate ID="edCury" runat="server" DataMember="_Currency_" DataField="CuryID" RateTypeView="currencyinfo" DataSourceID="ds"></pxa:PXCurrencyRate>
            <px:PXSelector ID="edBranchLocationID" runat="server" AllowEdit="True"
                AutoRefresh="True" CommitChanges="True" DataField="BranchLocationID"
                DataSourceID="ds" >
            </px:PXSelector>
            <px:PXSelector ID="edBillServiceContractID" runat="server" AutoRefresh="True"
                CommitChanges="True" DataField="BillServiceContractID" AllowEdit="True" FilterByAllFields="True">
            </px:PXSelector>
            <px:PXSelector ID="edBillContractPeriodID" runat="server"  DataField="BillContractPeriodID">
            </px:PXSelector>
            <px:PXSegmentMask ID="edProjectID" runat="server" AutoRefresh="True"
                CommitChanges="True" DataField="ProjectID" AllowEdit="True" AllowAddNew="True" >
            </px:PXSegmentMask>
            <px:PXSelector ID="edDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID" AllowEdit = "True" AutoRefresh="True" CommitChanges="True">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" ColumnSpan="2">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXPanel ID="edThirdColumn" runat="server" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="S" />
                <px:PXMaskEdit ID="edApptDurationTotal" runat="server" DataField="ApptDurationTotal" Enabled="False" TextAlign="Right">
                </px:PXMaskEdit>
                <px:PXMaskEdit ID="edEstimatedDurationTotal" runat="server" DataField="EstimatedDurationTotal" Enabled="False" TextAlign="Right">
                </px:PXMaskEdit>
                <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edCuryDocTotal" runat="server" DataField="CuryDocTotal">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edSOCuryCompletedBillableTotal" runat="server" DataField="SOCuryCompletedBillableTotal">
                </px:PXNumberEdit>
                <px:PXCheckBox ID="WaitingForParts" runat="server" DataField="WaitingForParts" Text="Waiting for Parts">
                </px:PXCheckBox>
                <px:PXCheckBox ID="AppointmentsNeeded" runat="server" DataField="AppointmentsNeeded" Text="Appointments Needed">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edIsPrepaymentEnable" runat="server" DataField="IsPrepaymentEnable">
                </px:PXCheckBox>
            </px:PXPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" TabIndex="200" DataSourceID="ds" MarkRequired="Dynamic" DataMember="CurrentServiceOrder">
        <Items>
            <px:PXTabItem Text="Settings">
                <Template>
                    <px:PXFormView ID="edContactAddressForm" runat="server" DataMember="CurrentServiceOrder" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXCheckBox ID="edAllowOverrideContactAddress" runat="server" DataField="AllowOverrideContactAddress" CommitChanges="true"/>
                        </Template>
                        <ContentStyle BackColor="Transparent"/>
                    </px:PXFormView>   
                    <px:PXFormView ID="edServiceOrder_Contact" runat="server" Caption="Contact" DataMember="ServiceOrder_Contact" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM"/>
                            <px:PXFormView ID="edContactForm" runat="server" DataMember="CurrentServiceOrder" DataSourceID="ds" RenderStyle="Simple">
                                <Template>
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM"/>
                                    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                </Template>
                                <ContentStyle BackColor="Transparent"/>
                            </px:PXFormView>  
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" CommitChanges="true"/>
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True"/>
                            <px:PXDropDown ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True" CommitChanges="True" Width="134px"/>
                            <px:PXLabel ID="LPhone1" runat="server" SuppressLabel="true" Width="0px"/>
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" LabelWidth="0px" Size="XM" LabelID="LPhone1" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXMailEdit ID="edEMail" runat="server" CommandSourceID="ds" DataField="EMail" CommitChanges="True"/>
                       </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None"/>
                    </px:PXFormView>
                    <px:PXFormView ID="edServiceOrder_Address" runat="server" Caption="Address" DataMember="ServiceOrder_Address" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                          <px:PXFormView ID="edRoomForm" runat="server" DataMember="CurrentServiceOrder" DataSourceID="ds" RenderStyle="Simple">
                              <Template>
                                  <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM"/>
                                  <px:PXSelector ID="edRoomID" runat="server" AllowEdit="True" DataField="RoomID" AutoRefresh="True"/>
                              </Template>
                              <ContentStyle BackColor="Transparent"/>
                          </px:PXFormView>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM"/>
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="true"/>
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="true"/>
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true"/>
                            <px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
                                FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
                            <px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
                                           FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="SM"/>
                           <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" CommitChanges="True" />
                           <px:PXButton ID="btnViewDirectionOnMap" runat="server" CommandName="ViewDirectionOnMap" CommandSourceID="ds"  Text="View On Map" />
                           <px:PXLayoutRule runat="server"/>
                       </Template>
                       <ContentStyle BackColor="Transparent" BorderStyle="None"/>
                    </px:PXFormView>

                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Service Order Settings" StartGroup="True" ControlSize="XM" LabelsWidth="SM">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" Merge="True">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edSLAETA_Date" runat="server" DataField="SLAETA_Date">
                    </px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edSLAETA_Time" runat="server" DataField="SLAETA_Time" TimeMode="True" SuppressLabel="True" Width="134px" >
                    </px:PXDateTimeEdit>
                    <px:PXLayoutRule runat="server" />

                    <px:PXDropDown ID="edSeverity" runat="server" DataField="Severity">
                    </px:PXDropDown>
                    <px:PXDropDown ID="edPriority" runat="server" DataField="Priority">
                    </px:PXDropDown>
                    <px:PXSegmentMask ID="edAssignedEmpID" runat="server" DataField="AssignedEmpID">
                    </px:PXSegmentMask>
                    <px:PXSelector ID="ProblemID" runat="server" AllowEdit="True"
                        DataField="ProblemID" DataSourceID="ds">
                    </px:PXSelector>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXGrid ID="PXGridDetails" runat="server" DataSourceID="ds" TabIndex="-12436" SkinID="Details" StatusField="Availability"
                        Width="100%" Height="100%" SyncPosition="True" AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderDetails">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" StartGroup="True" GroupCaption="Detail Info" />
                                    <px:PXSegmentMask ID="edUBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
                                    <px:PXTextEdit ID="edULineRef" runat="server" DataField="LineRef" NullText="<NEW>" />
                                    <px:PXDropDown ID="edUStatus" runat="server" DataField="Status" CommitChanges="True" />
                                    <px:PXDropDown ID="edULineType" runat="server" DataField="LineType" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSegmentMask ID="edUInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.lineType" PropertyName="DataValues[&quot;LineType&quot;]" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edUSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>" CommitChanges="True" />
                                    <px:PXDropDown ID="edUBillingRule" runat="server" DataField="BillingRule" Size="SM" />
                                    <px:PXTextEdit ID="edUTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXDropDown ID="edUEquipmentAction" runat="server" DataField="EquipmentAction" CommitChanges="True" />
                                    <px:PXSelector ID="edUSMEquipmentID" runat="server" DataField="SMEquipmentID" AutoRefresh="True" CommitChanges="True" AllowEdit="True" />
                                    <px:PXSelector ID="edUNewTargetEquipmentLineNbr" runat="server" DataField="NewTargetEquipmentLineNbr" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSelector ID="edUComponentID" runat="server" DataField="ComponentID" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSelector ID="edUEquipmentLineRef" runat="server" DataField="EquipmentLineRef" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSegmentMask ID="edUStaffID" runat="server" DataField="StaffID" AutoRefresh="True" CommitChanges="True" NullText="<SPLIT>" />
                                    <px:PXCheckBox ID="edUWarranty" runat="server" DataField="Warranty" />
                                    <px:PXCheckBox ID="edUIsPrepaid" runat="server" DataField="IsPrepaid" />
                                    <px:PXSegmentMask ID="edUSiteID" runat="server" DataField="SiteID" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edUSiteLocationID" runat="server" DataField="SiteLocationID" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edULotSerialNbr" runat="server" Size="XM" AllowNull="False" DataField="LotSerialNbr" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.siteLocationID" PropertyName="DataValues[&quot;SiteLocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSelector ID="edUUOM" runat="server" DataField="UOM" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edUEstimatedDuration" runat="server" DataField="EstimatedDuration" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUEstimatedQty" runat="server" DataField="EstimatedQty" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="True" />
                                    <px:PXCheckBox ID="edUManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUCuryEstimatedTranAmt" runat="server" DataField="CuryEstimatedTranAmt" />
                                    <px:PXCheckBox ID="edUContractRelated" runat="server" DataField="ContractRelated" />
                                    <px:PXNumberEdit ID="edUCoveredQty" runat="server" DataField="CoveredQty" />
                                    <px:PXNumberEdit ID="edUExtraUsageQty" runat="server" DataField="ExtraUsageQty" />
                                    <px:PXNumberEdit ID="edUCuryExtraUsageUnitPrice" runat="server" DataField="CuryExtraUsageUnitPrice" />
                                    <px:PXNumberEdit ID="edUApptEstimatedDuration" runat="server" DataField="ApptEstimatedDuration" />
                                    <px:PXNumberEdit ID="edUApptDuration" runat="server" DataField="ApptDuration" />
                                    <px:PXNumberEdit ID="edUApptQty" runat="server" DataField="apptQty" />
                                    <px:PXNumberEdit ID="edUCuryApptTranAmt" runat="server" DataField="CuryApptTranAmt" />
                                    <px:PXNumberEdit ID="edUApptCntr" runat="server" DataField="ApptCntr" />
                                    <px:PXCheckBox ID="edUIsBillable" runat="server" DataField="IsBillable" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUBillableQty" runat="server" DataField="BillableQty" />
                                    <px:PXNumberEdit ID="edUCuryBillableExtPrice" runat="server" DataField="CuryBillableExtPrice" />
                                    <px:PXNumberEdit ID="edUDiscPct" runat="server" DataField="DiscPct" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUCuryDiscAmt" runat="server" DataField="CuryDiscAmt" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edUCuryBillableTranAmt" runat="server" DataField="CuryBillableTranAmt" />
                                    <px:PXSelector ID="edUProjectTaskID" runat="server" DataField="ProjectTaskID" DisplayMode="Value" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edUCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edUAcctID" runat="server" DataField="AcctID" CommitChanges="True" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edUSubID" runat="server" DataField="SubID" AutoRefresh="True" />
                                    <px:PXSelector ID="edUMem_LastReferencedBy" runat="server" AllowEdit="True" DataField="Mem_LastReferencedBy" />
                                    <px:PXCheckBox ID="edUEnablePO" runat="server" DataField="EnablePO" />
                                    <px:PXSegmentMask ID="edUPOVendorID" runat="server" DataField="POVendorID" AllowEdit="True" CommitChanges="True" />
                                    <px:PXSegmentMask ID="edUPOVendorLocationID" runat="server" DataField="POVendorLocationID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edUPONbr" runat="server" DataField="PONbr" AllowEdit="True" />
                                    <px:PXTextEdit ID="edUPOStatus" runat="server" DataField="POStatus" AllowEdit="True" />
                                    <px:PXCheckBox ID="edUPOCompleted" runat="server" DataField="POCompleted" AllowEdit="True" />
                                    <px:PXTextEdit ID="edUComment" runat="server" DataField="Comment" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Availability" />
                                    <px:PXGridColumn DataField="SrvOrdType" />
                                    <px:PXGridColumn DataField="BranchID" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RefNbr" />
                                    <px:PXGridColumn DataField="LineRef" NullText="<NEW>" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LineType" RenderEditorText="True" MatrixMode="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" AllowDragDrop="True" />
                                    <px:PXGridColumn DataField="SubItemID" NullText="<SPLIT>" CommitChanges="True" />
                                    <px:PXGridColumn DataField="BillingRule" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="EquipmentAction" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SMEquipmentID" AutoCallBack="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ComponentID" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EquipmentLineRef" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="StaffID" CommitChanges="True" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="Warranty" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="IsPrepaid" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="SiteID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SiteLocationID" AllowShowHide="Server" NullText="<SPLIT>" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" NullText="&lt;SPLIT&gt;" />
                                    <px:PXGridColumn DataField="UOM" AllowDragDrop="True" />
                                    <px:PXGridColumn DataField="EstimatedDuration" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EstimatedQty" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ManualPrice" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryEstimatedTranAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractRelated" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CoveredQty" />
                                    <px:PXGridColumn DataField="ExtraUsageQty" />
                                    <px:PXGridColumn DataField="CuryExtraUsageUnitPrice" />
                                    <px:PXGridColumn DataField="ApptEstimatedDuration" />
                                    <px:PXGridColumn DataField="ApptDuration" />
                                    <px:PXGridColumn DataField="ApptQty" />
                                    <px:PXGridColumn DataField="CuryApptTranAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ApptCntr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="BillableQty" AllowDragDrop="True" />
                                    <px:PXGridColumn DataField="CuryBillableExtPrice" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DiscPct" AllowNull="False" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" AllowNull="False" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="EnablePO" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="POVendorID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="POVendorLocationID" />
                                    <px:PXGridColumn DataField="PONbr" TextAlign="Left" />
                                    <px:PXGridColumn DataField="POStatus" TextAlign="Left" />
                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ProjectTaskID" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="AcctID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SubID" />
                                    <px:PXGridColumn DataField="Mem_LastReferencedBy" />
                                    <px:PXGridColumn DataField="POCompleted" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Comment" />
                                    <px:PXGridColumn DataField="LineNbr" />
                                    <px:PXGridColumn DataField="SortOrder" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands PasteCommand="DetailsPasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
                        <Mode AllowFormEdit="True" InitNewRow="True" AllowUpload="True" AllowDragRows="True" />
                        <AutoSize Enabled="True" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdOpenServiceSelector">
                                    <AutoCallBack Target="ds" Command="OpenServiceSelector" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdopenStaffSelectorFromServiceTab">
                                    <AutoCallBack Target="ds" Command="openStaffSelectorFromServiceTab" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSFSSODetLine_binLotSerial" CommandSourceID="ds" DependOnGrid="PXGridDetails">
                                    <AutoCallBack>
                                        <Behavior CommitChanges="True" PostData="Page" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Tax Details" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="gridTaxDetails" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details" ActionsPosition="Top" BorderWidth="0px">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" AllowUpdate="False" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Appointments" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid ID="PXGridAppointments" runat="server" DataSourceID="ds" SkinID="Inquire" TabIndex="2300" Height="100%"
                        Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderAppointments">
                                <RowTemplate>
                                    <px:PXSelector ID="RefNbr" runat="server" DataField="RefNbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="Confirmed" runat="server" DataField="Confirmed"
                                        Text="Confirmed">
                                    </px:PXCheckBox>
                                    <px:PXDropDown ID="Status" runat="server" DataField="Status" Enabled="False">
                                    </px:PXDropDown>
                                    <px:PXDateTimeEdit ID="ScheduledDateTimeBegin_Date" runat="server"
                                        DataField="ScheduledDateTimeBegin_Date">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="ScheduledDateTimeBegin_Time" runat="server"
                                        DataField="ScheduledDateTimeBegin_Time">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="ScheduledDateTimeEnd_Time" runat="server"
                                        DataField="ScheduledDateTimeEnd_Time">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Confirmed" TextAlign="Center"
                                        Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeBegin_Date">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeEnd_Time">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Financial Settings">
                <Template>
                    <px:PXLayoutRule runat="server" GroupCaption="Financial Information"
                        StartGroup="True" ControlSize="XM" LabelsWidth="SM">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edBranchID" runat="server" AllowEdit="True" CommitChanges="True"
                        DataField="BranchID">
                    </px:PXSelector>
                    <px:PXSegmentMask ID="edBillCustomerID" runat="server" AllowEdit="True"
                        DataField="BillCustomerID" DataSourceID="ds" CommitChanges="True">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edBillLocationID" runat="server"
                        DataField="BillLocationID" DataSourceID="ds" AutoRefresh="True" CommitChanges="true">
                    </px:PXSegmentMask>
                    <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" CommitChanges="true"/>
                    <px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" CommitChanges="true"/>
                    <px:PXFormView ID="BillingTabSummary" runat="server"
                        Caption="Billing Summary" DataMember="BillingCycleRelated" MarkRequired="Dynamic"
                        DataSourceID="ds" RenderStyle="Simple" TabIndex="1500">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM">
                            </px:PXLayoutRule>
                            <px:PXSelector ID="edBillingCycleCD" runat="server" AllowEdit="False" DataField="BillingCycleCD">
                            </px:PXSelector>
                            <px:PXDropDown ID="edBillingBy" runat="server" AllowEdit="False" DataField="BillingBy">
                            </px:PXDropDown>
                        </Template>
                    </px:PXFormView>
                    <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" CommitChanges="True" AutoRefresh="True"></px:PXSegmentMask>
                    <px:PXCheckBox ID="edCommissionable" runat="server" DataField="Commissionable">
                    </px:PXCheckBox>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Default Staff" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid ID="PXGridStaff" runat="server" DataSourceID="ds" FilesIndicator="False"
                        NoteIndicator="False" SkinID="DetailsInTab" TabIndex="2300" Height="100%"
                        Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderEmployees">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edEmployeeID2" runat="server" DataField="EmployeeID" AutoRefresh="True" CommitChanges="True" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edType" runat="server" DataField="Type">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="edServiceLineRef" runat="server" CommitChanges="True" DataField="ServiceLineRef" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edFSSODetEmployee__InventoryID" runat="server" DataField="FSSODetEmployee__InventoryID" Enabled="False" AutoRefresh="True" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edFSSODetEmployee__TranDesc" runat="server" DataField="FSSODetEmployee__TranDesc" Enabled="False">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edComment" runat="server" DataField="Comment">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" />
                                    <px:PXGridColumn DataField="RefNbr" />
                                    <px:PXGridColumn DataField="EmployeeID" CommitChanges="True" DisplayMode="Hint">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Type">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ServiceLineRef" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODetEmployee__InventoryID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODetEmployee__TranDesc">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdOpenStaffSelectorFromStaffTab">
                                    <AutoCallBack Target="ds" Command="OpenStaffSelectorFromStaffTab" ></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Default Resource Equipment" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid ID="PXGridResourceEquipment" runat="server" DataSourceID="ds" FilesIndicator="False"
                        NoteIndicator="False" SkinID="Details" TabIndex="2300" Height="100%"
                        Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderEquipment"  DataKeyNames="SOID,SMEquipmentID">
                                <RowTemplate>
                                    <px:PXSelector ID="edRSMEquipmentID" runat="server" DataField="SMEquipmentID"
                                        AllowEdit="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edRFSEquipment__Descr" runat="server"
                                        DataField="FSEquipment__Descr">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edCommentEquiment" runat="server" DataField="Comment">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" />
                                    <px:PXGridColumn DataField="RefNbr" />
                                    <px:PXGridColumn DataField="SMEquipmentID" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSEquipment__Descr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                        Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False"
                                        TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
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

            <px:PXTabItem Text="Related Service Orders" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid ID="PXGridRelatedServiceOrders" runat="server" DataSourceID="ds" FilesIndicator="False"
                        NoteIndicator="False" SkinID="DetailsInTab" TabIndex="2300" Height="100%" Width="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="RelatedServiceOrders">
                                <RowTemplate>
                                    <px:PXSelector ID="edRelatedSrvOrdSrvOrdType" runat="server" DataField="SrvOrdType" AllowEdit="True"></px:PXSelector>
                                    <px:PXSelector ID="edRelatedSrvOrdRefNbr" runat="server" DataField="RefNbr" AllowEdit="True"></px:PXSelector>
                                    <px:PXTextEdit ID="edRelatedSrvOrdDocDesc" runat="server" DataField="DocDesc">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edRelatedSrvOrdStatus" runat="server" DataField="Status" Enabled="False"></px:PXDropDown>
                                    <px:PXDateTimeEdit ID="edRelatedSrvOrdOrderDate" runat="server" DataField="OrderDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXSelector ID="edRelatedSrvOrdCuryID" runat="server" DataField="CuryID">
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                 <px:PXGridColumn DataField="SrvOrdType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenServiceOrderScreen">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="DocDesc">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="OrderDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryID">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar ActionsText="False" PagerVisible="False"></ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Prepayments" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False" VisibleExp="DataControls[&quot;edIsPrepaymentEnable&quot;].Value == true">
                <Template>
                    <px:PXPanel runat="server" ID="CstPanel43">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>
                        <px:PXNumberEdit ID="edSOPrepaymentReceived" runat="server" DataField="SOPrepaymentReceived" Enabled="False" Size="Empty"/>
                        <px:PXNumberEdit ID="edSOPrepaymentRemaining" runat="server" DataField="SOPrepaymentRemaining" Enabled="False" Size="Empty"/>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>
                        <px:PXNumberEdit ID="edSOCuryUnpaidBalanace" runat="server" DataField="SOCuryUnpaidBalanace" Enabled="False" Size="Empty"/>
                        <px:PXNumberEdit ID="edSOCuryBillableUnpaidBalanace" runat="server" DataField="SOCuryBillableUnpaidBalanace"  Enabled="False" Size="Empty"/>
                    </px:PXPanel>
                    <px:PXGrid ID="detgrid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 332px;" Width="100%"
                        BorderWidth="0px" SkinID="Details" Height="332px" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments">
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" Label="Type" />
                                    <px:PXGridColumn DataField="RefNbr" Label="Reference Nbr." LinkCommand="ViewPayment"/>
                                    <px:PXGridColumn DataField="Status" Label="Status" />
                                    <px:PXGridColumn DataField="AdjDate" Label="Application Date" />
                                    <px:PXGridColumn DataField="ExtRefNbr" Label="Payment Ref." />
                                    <px:PXGridColumn DataField="PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa" Label="ARPayment-Payment Method" />
                                    <px:PXGridColumn DataField="CashAccountID" DisplayFormat="&gt;######" Label="Cash Account" />
                                    <px:PXGridColumn DataField="CuryOrigDocAmt" Label="Orig. Amount" />
                                    <px:PXGridColumn DataField="CurySOApplAmt" Label="Applied to Orders" />
                                    <px:PXGridColumn DataField="CuryUnappliedBal" Label="Available Balance" />
                                    <px:PXGridColumn DataField="CuryID" Label="Currency ID" />
                                    <px:PXGridColumn DataField="FSAdjust__AdjdAppRefNbr" Label="Applied To Order"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Create Prepayment">
                                    <AutoCallBack Command="CreatePrepayment" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                    <PopupCommand Target="detgrid" Command="Refresh">
                                    </PopupCommand>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="View Payment" Tooltip="View Payment" CommandName="ViewPayment" CommandSourceID="ds">
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Totals">
                <Template>
                    <px:PXFormView ID="formG" runat="server" DataSourceID="ds" Style="z-index: 100; left: 18px; top: 36px;" Width="100%" DataMember="CurrentServiceOrder" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Service Order Totals" />
                            <px:PXNumberEdit ID="edCuryEstimatedOrderTotal" runat="server" DataField="CuryEstimatedOrderTotal" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryApptOrderTotal" runat="server" Enabled="False" DataField="CuryApptOrderTotal" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryBillableOrderTotal" runat="server" Enabled="False" DataField="CuryBillableOrderTotal" Size="Empty" />
                            <px:PXNumberEdit ID="edGridCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edGridCuryDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edGridSOCuryCompletedBillableTotal" runat="server" DataField="SOCuryCompletedBillableTotal" Enabled="False" Size="Empty" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Prepayment Totals" />
                            <px:PXNumberEdit ID="edSOPrepaymentReceivedT" runat="server" DataField="SOPrepaymentReceived" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edSOPrepaymentAppliedT" runat="server" DataField="SOPrepaymentApplied" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edSOPrepaymentRemainingT" runat="server" DataField="SOPrepaymentRemaining" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edSOCuryUnpaidBalanaceT" runat="server" DataField="SOCuryUnpaidBalanace" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edSOCuryBillableUnpaidBalanaceT" runat="server" DataField="SOCuryBillableUnpaidBalanace" Enabled="False" Size="Empty" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Other Information" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False" >
                <Template>
                    <px:PXPanel ID="PXPanel1" runat="server" SkinID="transparent">
                        <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ></px:PXLayoutRule>
                        <px:PXFormView ID="SourceInfoForm" runat="server" Caption="Source Info" DataMember="CurrentServiceOrder"
                        RenderStyle="Fieldset">
                            <Template>
                                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                                <px:PXDropDown ID="edSourceType" runat="server" DataField="SourceType" Enabled="False">
                                </px:PXDropDown>
                                <px:PXTextEdit ID="edSourceReferenceNbr" runat="server" DataField="SourceReferenceNbr" Enabled="False">
                                    <LinkCommand Target="ds" Command="OpenSource"></LinkCommand>
                                </px:PXTextEdit>
                                <px:PXSelector ID="edServiceContractID" runat="server" DataField="ServiceContractID" AllowEdit="True" DataSourceID="ds" Enabled="False">
                                </px:PXSelector>
                                <px:PXTextEdit ID="edScheduleID" runat="server" DataField="ScheduleID" Enabled="False" AllowEdit="True">
                                  <LinkCommand Target="ds" Command="OpenScheduleScreen" />
                                </px:PXTextEdit>
                                <px:PXTextEdit ID="edRecurrenceDescription" runat="server" DataField="ScheduleRecord.RecurrenceDescription" Enabled="false">
                                </px:PXTextEdit>
                            </Template>
                        </px:PXFormView>
                        <px:PXLayoutRule runat="server" StartColumn="True" />
                        <px:PXFormView ID="edInvoiceInfoForm1" runat="server" DataSourceID="ds" DataMember="ServiceOrderPostedIn" Caption="Billing Info" RenderStyle="Fieldset">
                            <Template>
                                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" />
                                <px:PXTextEdit ID="edFSPostBatch__BatchNbr" runat="server" DataField="FSPostBatch__BatchNbr" Enabled="False">
                                    <LinkCommand Target="ds" Command="OpenBatch"></LinkCommand>
                                </px:PXTextEdit>
                                <px:PXTextEdit ID="edPostDocType" runat="server" DataField="PostDocType">
                                </px:PXTextEdit>
                                <px:PXTextEdit ID="edPostDocReferenceNbr" runat="server" DataField="PostDocReferenceNbr" Enabled="False">
                                    <LinkCommand Target="ds" Command="OpenPostingDocument"></LinkCommand>
                                </px:PXTextEdit>
                                <px:PXTextEdit ID="edInvoiceReferenceNbr" runat="server" DataField="InvoiceReferenceNbr" Enabled="False">
                                    <LinkCommand Target="ds" Command="OpenInvoiceDocument"></LinkCommand>
                                </px:PXTextEdit>
                                <px:PXTextEdit ID="edINPostDocReferenceNbr" runat="server" DataField="INPostDocReferenceNbr" Enabled="False">
                                    <LinkCommand Target="ds" Command="OpenINPostingDocument"></LinkCommand>
                                </px:PXTextEdit>
                            </Template>
                        </px:PXFormView>
                        <px:PXFormView ID="edInvoiceInfoForm2" runat="server" DataMember="CurrentServiceOrder"
                        RenderStyle="Simple" SkinID="Transparent">
                            <Template>
                                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" />
                                <px:PXCheckBox ID="edAllowInvoice" runat="server" DataField="AllowInvoice" AlignLeft="False"/>
                                <px:PXCheckBox ID="edMem_Invoiced" runat="server" DataField="Mem_Invoiced" AlignLeft="False"/>
                            </Template>
                        </px:PXFormView>
                        <px:PXLayoutRule runat="server" StartRow="True"/>
                    </px:PXPanel>
                    <px:PXFormView ID="edCommentsForm" runat="server" DataMember="CurrentServiceOrder"
                    RenderStyle="Simple">
                        <Template>                
                            <px:PXRichTextEdit ID="edLongDescr" runat="server" DataField="LongDescr"
                                Style="width: 100%;height: 120px" AllowAttached="true" AllowSearch="true"
                                AllowMacros="true" AllowLoadTemplate="false" AllowSourceMode="true">
                                <AutoSize Enabled="True" MinHeight="216" />
                            </px:PXRichTextEdit>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>

    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="90%" Height="360px" Caption="Allocations" CaptionVisible="True" Key="lsFSSODetSelect"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="optform" DesignView="Content" TabIndex="3200">
        <px:PXFormView ID="optform" runat="server" CaptionVisible="False" DataMember="LSFSSODetLine_lotseropts" DataSourceID="ds" SkinID="Transparent"
            TabIndex="-3236" Width="100%">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" CommandName="LSFSSODetLine_generateLotSerial" CommandSourceID="ds" Height="20px"
                    Text="Generate" />
            </Template>
            <Parameters>
                <px:PXSyncGridParam ControlID="PXGridDetails" />
            </Parameters>
        </px:PXFormView>
        <px:PXGrid ID="PXGridAllocations" runat="server" AutoAdjustColumns="True" DataSourceID="ds" Height="192px" Style="height: 192px;" TabIndex="-3036" Width="100%" AllowFilter="true"
            SkinID="Inquire" SyncPosition="true">
            <Parameters>
                <px:PXSyncGridParam ControlID="PXGridDetails" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataKeyNames="SrvOrdType,RefNbr,LineNbr,SplitLineNbr" DataMember="Splits">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                        <px:PXSegmentMask ID="edSubItemID3" runat="server" AutoRefresh="True" DataField="SubItemID" />
                        <px:PXSegmentMask ID="edSiteID3" runat="server" AutoRefresh="True" DataField="SiteID" />
                        <px:PXSegmentMask ID="edLocationID3" runat="server" AutoRefresh="True" DataField="LocationID">
                            <Parameters>
                                <px:PXControlParam ControlID="PXGridAllocations" Name="FSSODetSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="PXGridAllocations" Name="FSSODetSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="PXGridAllocations" Name="FSSODetSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty3" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM3" runat="server" AutoRefresh="True" DataField="UOM">
                            <Parameters>
                                <px:PXControlParam ControlID="PXGridDetails" Name="FSSODet.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr3" runat="server" AutoRefresh="True" DataField="LotSerialNbr">
                            <Parameters>
                                <px:PXControlParam ControlID="PXGridAllocations" Name="FSSODetSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="PXGridAllocations" Name="FSSODetSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="PXGridAllocations" Name="FSSODetSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate3" runat="server" DataField="ExpireDate" />
                        <%--                        <px:PXCheckBox CommitChanges="True" ID="chkPOCreate2" runat="server" DataField="POCreate" />
                        <px:PXDropDown ID="edPOType2" runat="server" DataField="POType" Enabled="False" />
                        <px:PXSelector ID="edPONbr2" runat="server" DataField="PONbr" Enabled="False" AllowEdit="True" />--%>
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="SplitLineNbr" TextAlign="Right" />
                        <px:PXGridColumn DataField="ParentSplitLineNbr" TextAlign="Right" />
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="ShipDate" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="IsAllocated" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn DataField="SiteID" AllowShowHide="Server" AutoCallBack="True" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" DataField="Completed" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="LocationID" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="LotSerialNbr" AutoCallBack="True" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="ShippedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" />
                        <%--                        <px:PXGridColumn DataField="ShipmentNbr" />--%>
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="ExpireDate" />
                        <px:PXGridColumn AllowNull="False" DataField="POCreate" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="FSSODetSplit$RefNoteID$Link" />
                        <%--                        <px:PXGridColumn DataField="POType" Visible="False" RenderEditorText="True" />
                        <px:PXGridColumn DataField="PONbr" Visible="False" />--%>
                    </Columns>
                    <Layout FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
            <Mode InitNewRow="True" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>

