<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS300100.aspx.cs" Inherits="Page_FS300100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ServiceOrderRecords" TypeName="PX.Objects.FS.ServiceOrderEntry">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ActionsMenu" CommitChanges="True">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ReportsMenu" CommitChanges="True">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ViewDirectionOnMap" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenSource" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="createNewCustomer" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenStaffSelectorFromServiceTab" Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="OpenStaffSelectorFromStaffTab" Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="OpenServiceSelector" Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="OpenAppointmentScreen" Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="SelectCurrentService" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenServiceOrderScreen" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenPostingDocument" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenInvoiceDocument" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenBatch" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="QuickProcessOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False"/>
            <px:PXDSCallbackCommand Name="QuickProcess" ShowProcessInfo="true" />
            <px:PXDSCallbackCommand Name="CreatePrepayment" Visible="False" CommitChanges="True"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="openRoomBoard" CommitChanges="True" Visible="False"/>
            <px:PXDSCallbackCommand Visible="false" Name="ViewPayment" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Visible="false" Name="FSSODetPartSplit$RefNoteID$Link" DependOnGrid="grid2" CommitChanges="True
            "/>
            <px:PXDSCallbackCommand Visible="false" Name="FSSODetServiceSplit$RefNoteID$Link" CommitChanges="True"/>

            <px:PXDSCallbackCommand Visible="False" Name="LSFSSODetPartLine_generateLotSerial" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSFSSODetPartLine_binLotSerial" DependOnGrid="PXGridParts" />

            <px:PXDSCallbackCommand Visible="False" Name="LSFSSODetServiceLine_generateLotSerial" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSFSSODetServiceLine_binLotSerial" DependOnGrid="PXGridServices"/>
            <px:PXDSCallbackCommand Visible="False" Name="OpenScheduleScreen"/>
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
    <px:PXSmartPanel
        ID="PXSmartPanelStaffSelector"
        runat="server"
        Caption="Staff Selector"
        CaptionVisible="True"
        Key="StaffSelectorFilter"
        AutoCallBack-Command="Refresh"
        AutoCallBack-Target="PXGridStaff"
        ShowAfterLoad="True"
        Width="1100px"
        Height="900px"
        CloseAfterAction ="True"
        AutoReload="True"
        LoadOnDemand="True"
        TabIndex="8500"
        AutoRepaint="true"
        CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" AllowResize="False">
        <px:PXLayoutRule runat="server" StartColumn="True">
        </px:PXLayoutRule>
        <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100px" DataMember="StaffSelectorFilter" TabIndex="700" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True"
                    LabelsWidth="SM" ControlSize="M">
                </px:PXLayoutRule>
                <px:PXSelector ID="edServiceLineRef2" runat="server" CommitChanges="True" DataField="ServiceLineRef" AutoRefresh="True" DisplayMode="Value">
                </px:PXSelector>
                <px:PXTextEdit ID="edPostalCode" runat="server" DataField="PostalCode"
                    Size="SM" SuppressLabel="False">
                </px:PXTextEdit>
                <px:PXSelector ID="edGeoZoneID" runat="server" DataField="GeoZoneID" CommitChanges = "True">
                </px:PXSelector>
                <px:PXTextEdit ID="edStaffConcat" runat="server" DataField="StaffConcat"
                    Size="L" SuppressLabel="False" Enabled="False">
                </px:PXTextEdit>

                <px:PXLayoutRule runat="server" StartRow="True"
                    LabelsWidth="SM" ControlSize="M">
                </px:PXLayoutRule>
                <px:PXLayoutRule runat="server" StartRow="True">
                </px:PXLayoutRule>
            </Template>
        </px:PXFormView>
                    <px:PXGrid ID="PXGridServiceSkills" runat="server" DataSourceID="ds"
                     Style="z-index: 100" Width="600px" SkinID="Inquire" TabIndex="900"
                     SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto"
                     Height="350px" AutoAdjustColumns="True">
                        <Levels>
                            <px:PXGridLevel DataMember="SkillGridFilter" DataKeyNames="SkillCD">
                                <Columns>
                                    <px:PXGridColumn AllowCheckAll="True" DataField="Mem_Selected" Width="90px" TextAlign="Center" Type="CheckBox" CommitChanges="true" >
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SkillCD" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Descr" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_ServicesList" Width="225px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="350" />
                        <ActionBar PagerVisible="Bottom" ActionsText="False">
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
                    <px:PXGrid ID="PXGridLicenseType" runat="server"
                    AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" Height="350px"
                    SkinID="Inquire" Style="z-index: 100" SyncPosition="True" TabIndex="910"
                    Width="600px" AutoAdjustColumns="True" FilesIndicator="false" NoteIndicator ="false">
                        <Levels>
                            <px:PXGridLevel DataMember="LicenseTypeGridFilter" DataKeyNames="LicenseTypeCD">
                                <Columns>
                                    <px:PXGridColumn AllowCheckAll="True" DataField="Mem_Selected" TextAlign="Center" Type="CheckBox"
                                      CommitChanges="true" Width="90px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LicenseTypeCD" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Descr" Width="200px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="350" />
                        <ActionBar ActionsText="False" PagerVisible="Bottom">
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
        <px:PXLayoutRule runat="server" StartColumn="True">
        </px:PXLayoutRule>
        <px:PXLabel runat="server" Height="66px" />
        <px:PXGrid ID="PXGridAvailableStaff" runat="server" DataSourceID="ds"
            Style="z-index: 100" Width="380px" SkinID="Inquire" TabIndex="500"
            SyncPosition = "True" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True"
            FilesIndicator="false" NoteIndicator ="false">
        <Levels>
          <px:PXGridLevel DataMember="StaffRecords" DataKeyNames="AcctCD">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox"
                                      CommitChanges="true" Width="90px">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="Type" Width="90px">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="AcctCD" Width="120px">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="AcctName" Width="200px">
                        </px:PXGridColumn>
                    </Columns>
          </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="704"/>
            <Mode AllowAddNew="False" AllowDelete="False"/>
      </px:PXGrid>
        <px:PXLayoutRule runat="server" StartRow="True">
        </px:PXLayoutRule>
        <px:PXButton ID="closeStaffSelector" runat="server" DialogResult="Cancel" Text="Close" AlignLeft="True">
        </px:PXButton>
    </px:PXSmartPanel>
    <%--/ Employee Selector --%>

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
        ShowAfterLoad="True"
        Width="400px"
        Height="100px">
        <px:PXLayoutRule runat="server" StartColumn="True">
        </px:PXLayoutRule>
        <px:PXFormView ID="DriverRouteForm" runat="server" DataMember="ServiceOrderTypeSelector"
            DataSourceID="ds" TabIndex="1600" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartRow="True" ControlSize="SM" LabelsWidth="SM"
                    StartColumn="True">
                </px:PXLayoutRule>
                <px:PXSelector ID="edSrvOrdType" runat="server"
                AutoRefresh="True" DataField="SrvOrdType" DataSourceID="ds" CommitChanges="True">
                </px:PXSelector>
            </Template>
        </px:PXFormView>
        <px:PXLayoutRule runat="server" StartRow="True" Merge="True">
        </px:PXLayoutRule>
        <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Proceed"
            AlignLeft="True" Width="125px">
        </px:PXButton>
        <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close"
            AlignLeft="True">
        </px:PXButton>
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
                CommitChanges="True" DataField="ProjectID" AllowEdit="True">
            </px:PXSegmentMask>
            <px:PXSelector ID="edDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID"
                DisplayMode="Value" AllowEdit = "True" AutoRefresh="True" CommitChanges="True">
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

                    <px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate" CommitChanges="True">
                    </px:PXDateTimeEdit>

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
                    <px:PXSelector ID="CauseID" runat="server" AllowEdit="True" DataField="CauseID"
                        DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXSelector ID="ResolutionID" runat="server" AllowEdit="True"
                        DataField="ResolutionID" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="ResolutionDate" runat="server"
                        DataField="ResolutionDate">
                    </px:PXDateTimeEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Services">
                <Template>
                    <px:PXGrid ID="PXGridServices" runat="server" DataSourceID="ds" TabIndex="-12436" SkinID="Details"
                        Width="100%" Height="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderDetServices">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True">
                                </px:PXLayoutRule>
                                    <px:PXSmartPanel
                                        ID="PXSmartPanelServiceSelector"
                                        runat="server"
                                        Caption="Service Selector"
                                        CaptionVisible="True"
                                        Key="ServiceSelectorFilter"
                                        AutoCallBack-Command="Refresh"
                                        AutoCallBack-Target="PXGridStaffSelected"
                                        ShowAfterLoad="True"
                                        Width="600px"
                                        ShowMaximizeButton="True"
                                        CloseAfterAction="True"
                                        AutoReload="True"
                                        HideAfterAction="False"
                                        LoadOnDemand="True"
                                        Height="600px" TabIndex="8500"
                                        AutoRepaint="True"
                                        >
                                        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM">
                                        </px:PXLayoutRule>
                                        <px:PXFormView ID="PXFormViewFilter" runat="server"  TabIndex="1600" SkinID="Transparent"
                                            DataMember="ServiceSelectorFilter" DataSourceID="ds">
                                            <Template>
                                                <px:PXSelector ID="edServiceClassID" runat="server" DataField="ServiceClassID"
                                                    DataSourceID="ds" Size="M" AutoRefresh="True" CommitChanges="True">
                                                </px:PXSelector>
                                                <px:PXLabel runat="server" />
                                                <px:PXGrid ID="PXGridSelectedEmployees" runat="server" DataSourceID="ds"
                                                    Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="800"
                                                    SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Height="200px">
                                                    <Levels>
                                                        <px:PXGridLevel DataMember="EmployeeGridFilter" DataKeyNames="EmployeeID">
                                                            <Columns>
                                                                <px:PXGridColumn
                                                                    AllowCheckAll="True" 
                                                                    DataField="Mem_Selected" TextAlign="Center" Type="CheckBox" Width="80px"
                                                                    CommitChanges="True">
                                                                </px:PXGridColumn>
                                                                <px:PXGridColumn DataField="EmployeeID" Width="120px">
                                                                </px:PXGridColumn>
                                                                <px:PXGridColumn DataField="EmployeeID_BAccountStaffMember_acctName" Width="200px">
                                                                </px:PXGridColumn>
                                                            </Columns>
                                                        </px:PXGridLevel>
                                                    </Levels>
                                                    <AutoSize Enabled="True" MinHeight="250" />
                                                    <ActionBar PagerVisible="Bottom" ActionsText="False">
                                                    </ActionBar>
                                                    <Mode AllowAddNew="False" AllowDelete="False" />
                                                </px:PXGrid>
                                            </Template>
                                        </px:PXFormView>
                                        <px:PXLayoutRule runat="server" StartRow="True">
                                        </px:PXLayoutRule>
                                        <px:PXGrid ID="PXGridAvailableServices" runat="server" DataSourceID="ds"
                                                Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="500"
                                                SyncPosition = "True" AllowPaging="True" AdjustPageSize="Auto" Height="200px">
                                            <Levels>
                                                <px:PXGridLevel DataMember="ServiceRecords" DataKeyNames="InventoryCD">
                                                    <Columns>
                                                        <px:PXGridColumn DataField="InventoryCD" Width="120px">
                                                        </px:PXGridColumn>
                                                        <px:PXGridColumn DataField="Descr" Width="200px">
                                                        </px:PXGridColumn>
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
                                        <px:PXButton ID="PXButtonCloseServiceSelector" runat="server" DialogResult="Cancel" Text="Close"
                                        AlignLeft="True"/>
                                    </px:PXSmartPanel>
                                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info"
                                        StartGroup="True" StartRow="True">
                                    </px:PXLayoutRule>
                                    <px:PXSegmentMask CommitChanges="True" ID="edServBranchID" runat="server" DataField="BranchID" />
                                    <px:PXTextEdit ID="edLineRef" runat="server" DataField="LineRef" NullText="<NEW>">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edScheduled" runat="server" DataField="Scheduled">
                                    </px:PXCheckBox>
                                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSSODetService.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXSegmentMask>
                                    <px:PXDropDown ID="edBillingRule" runat="server" DataField="BillingRule" Size="SM">
                                    </px:PXDropDown>
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="edSMEquipmentID" runat="server" DataField="SMEquipmentID" AutoRefresh="True" CommitChanges="True" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edNewTargetEquipmentLineNbr" runat="server" DataField="NewTargetEquipmentLineNbr" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edComponentID" runat="server" DataField="ComponentID" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edEquipmentLineRef" runat="server" DataField="EquipmentLineRef" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edStaffID" runat="server" DataField="StaffID" AutoRefresh="True" CommitChanges="True" NullText="<SPLIT>">
                                    </px:PXSegmentMask>
                                    <px:PXCheckBox ID="edWarranty" runat="server" DataField="Warranty">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="edIsPrepaid" runat="server" DataField="IsPrepaid">
                                    </px:PXCheckBox>
                                    <px:PXSegmentMask ID="edSiteIDServices" runat="server" DataField="SiteID" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSSODetService.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSSODetService.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                         </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSiteLocationIDServices" runat="server" DataField="SiteLocationID" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetService.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" CommitChanges = "True">
                                    </px:PXSelector>
<%-- ESTIMATED FIELDS --%>                                    
                                    <px:PXMaskEdit ID="edEstimatedDuration" runat="server" DataField="EstimatedDuration" CommitChanges="True">
                                    </px:PXMaskEdit>
									<px:PXNumberEdit ID="edEstimatedQty" runat="server" DataField="EstimatedQty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryEstimatedTranAmt" runat="server" DataField="CuryEstimatedTranAmt">
                                    </px:PXNumberEdit>

<%-- CONTRACT RELATED FIELDS --%>
                                    <px:PXCheckBox ID="edContractRelated" runat="server" DataField="ContractRelated">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edCoveredQty" runat="server" DataField="CoveredQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edExtraUsageQty" runat="server" DataField="ExtraUsageQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryExtraUsageUnitPrice" runat="server" DataField="CuryExtraUsageUnitPrice">
                                    </px:PXNumberEdit>

<%-- APPOINTMENT SUM FIELDS --%>
                                    <px:PXNumberEdit ID="edApptDuration" runat="server" DataField="ApptDuration">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edApptQty" runat="server" DataField="apptQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryApptTranAmt" runat="server" DataField="CuryApptTranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edApptCount" runat="server" DataField="ApptNumber">
                                    </px:PXNumberEdit>

<%-- BILLABLE FIELDS --%>
                                    <px:PXCheckBox ID="edIsBillable" runat="server" DataField="IsBillable" CommitChanges="True">
                                    </px:PXCheckBox>                                    
                                    <px:PXNumberEdit ID="edBillableQty" runat="server" DataField="BillableQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryBillableTranAmt" runat="server" DataField="CuryBillableTranAmt">
                                    </px:PXNumberEdit>




                                    <px:PXSelector ID="edProjectTaskID" runat="server" DataField="ProjectTaskID"
                                        DisplayMode="Value" AllowEdit = "True" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edAcctID" runat="server" DataField="AcctID" CommitChanges="True" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edMem_LastReferencedBy" runat="server" AllowEdit="True"
                                    DataField="Mem_LastReferencedBy"  >
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edServEnablePO" runat="server" DataField="EnablePO">
                                    </px:PXCheckBox>
                                    <px:PXSegmentMask ID="edServPOVendorID" runat="server" DataField="POVendorID" AllowEdit="true" CommitChanges="true">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edServPOVendorLocationID" runat="server" DataField="POVendorLocationID" AllowEdit="true" AutoRefresh="true">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edServPONbr" runat="server" DataField="PONbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edServPOStatus" runat="server" DataField="POStatus" AllowEdit="True">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edServPOCompleted" runat="server" DataField="POCompleted" AllowEdit="True">
                                    </px:PXCheckBox>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="BranchID" Width="81px" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="LineRef" NullText="<NEW>" Width="85px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Scheduled" TextAlign="Center" Type="CheckBox" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn CommitChanges="True" DataField="LineType" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" Width="100px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubItemID" Width="60px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillingRule" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SMEquipmentID" Width="100px" AutoCallBack = "True" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" TextAlign="Right" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ComponentID" TextAlign="Right" CommitChanges="True" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EquipmentLineRef" TextAlign="Right" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="StaffID" Width="120px" CommitChanges="True" NullText="<SPLIT>">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Warranty" Width="70px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>

                                    <px:PXGridColumn DataField="IsPrepaid" Width="65px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteLocationID" AllowShowHide="Server" Width="81px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UOM">
                                    </px:PXGridColumn>
<%-- ESTIMATED FIELDS --%>                                    
                                    <px:PXGridColumn DataField="EstimatedDuration" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
									<px:PXGridColumn DataField="EstimatedQty" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryUnitCost" Width="100px" TextAlign="Right" CommitChanges="True" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ManualPrice" Width="65px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryUnitPrice" Width="100px" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryEstimatedTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>

<%-- CONTRACT RELATED FIELDS --%>
                                    <px:PXGridColumn DataField="ContractRelated" Width="75px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CoveredQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ExtraUsageQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryExtraUsageUnitPrice" Width="100px">
                                    </px:PXGridColumn>
<%-- APPOINTMENT SUM FIELDS --%>
                                    <px:PXGridColumn DataField="ApptDuration" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ApptQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryApptTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ApptNumber" Width="95px">
                                    </px:PXGridColumn>



<%-- BILLABLE FIELDS --%>
                                    <px:PXGridColumn DataField="IsBillable" Width="65px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillableQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>


                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="AcctID" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubID" Width="180px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_LastReferencedBy" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EnablePO" TextAlign="Center" Type="CheckBox" Width="65px" CommitChanges="True" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POVendorID" Width="100px" CommitChanges="true" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POVendorLocationID" Width="100px" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PONbr" TextAlign="Left" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POStatus" TextAlign="Left" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POCompleted" TextAlign="Center" Type="CheckBox" Width="65px" Visible="false">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowFormEdit="True" InitNewRow="True"/>
                        <AutoSize Enabled="True" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdOpenServiceSelector">
                                    <AutoCallBack Target="ds" Command="OpenServiceSelector" ></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdopenStaffSelectorFromServiceTab">
                                    <AutoCallBack Target="ds" Command="openStaffSelectorFromServiceTab" ></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Inventory Items">
                <Template>
                    <px:PXGrid ID="PXGridParts" runat="server" DataSourceID="ds" TabIndex="-12436" SkinID="Details" StatusField="Availability"
                        Width="100%" Height="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderDetParts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info" StartGroup="True">
                                    </px:PXLayoutRule>
                                    <px:PXSegmentMask CommitChanges="True" ID="edInvBranchID" runat="server" DataField="BranchID" />
                                    <px:PXTextEdit ID="edLineRef2" runat="server" DataField="LineRef" NullText="<NEW>">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edStatus2" runat="server" DataField="Status" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXDropDown ID="edLineType2" runat="server" DataField="LineType"
                                        CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID2" runat="server" DataField="InventoryID"
                                        AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edTranDesc2" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edEquipmentAction2" runat="server" DataField="EquipmentAction" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSelector ID="edSMEquipmentID2" runat="server" DataField="SMEquipmentID" AutoRefresh="True" CommitChanges="True" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edNewTargetEquipmentLineNbr2" runat="server" DataField="NewTargetEquipmentLineNbr" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edComponentID2" runat="server" DataField="ComponentID" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edEquipmentLineRef2" runat="server" DataField="EquipmentLineRef" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edSuspendedTargetEquipmentID" runat="server" DataField="SuspendedTargetEquipmentID">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edWarranty2" runat="server" DataField="Warranty">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="edIsPrepaid2" runat="server" DataField="IsPrepaid">
                                    </px:PXCheckBox>
                                    <px:PXSegmentMask ID="edSiteID2" runat="server" DataField="SiteID" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                         </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSiteLocationID2" runat="server" DataField="SiteLocationID" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector Size="xm" ID="edLotSerialNbr" runat="server" AllowNull="False" DataField="LotSerialNbr" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.siteLocationID" PropertyName="DataValues[&quot;SiteLocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" CommitChanges = "True">
                                    </px:PXSelector>



<%-- ESTIMATED FIELDS --%>                                    
                                    <px:PXNumberEdit ID="EstimatedQty2" runat="server" DataField="EstimatedQty" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryUnitCost2" runat="server" DataField="CuryUnitCost" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edManualPrice2" runat="server" DataField="ManualPrice" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edCuryUnitPrice2" runat="server" DataField="CuryUnitPrice" CommitChanges = "True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryEstimatedTranAmt2" runat="server" DataField="CuryEstimatedTranAmt">
                                    </px:PXNumberEdit>



<%-- APPOINTMENT SUM FIELDS --%>
                                    <px:PXNumberEdit ID="edApptQty2" runat="server" DataField="ApptQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryApptTranAmt2" runat="server" DataField="CuryApptTranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edApptCount2" runat="server" DataField="ApptNumber">
                                    </px:PXNumberEdit>



<%-- BILLABLE FIELDS --%>
                                    <px:PXCheckBox ID="edIsBillable2" runat="server" DataField="IsBillable" CommitChanges="True">
                                    </px:PXCheckBox>                                    
                                    <px:PXNumberEdit ID="edBillableQty2" runat="server" DataField="BillableQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryBillableTranAmt2" runat="server" DataField="CuryBillableTranAmt">
                                    </px:PXNumberEdit>




                                    <px:PXSelector ID="ProjectTaskID2" runat="server" DataField="ProjectTaskID"
                                        AllowEdit = "True" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCostCodeID2" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edAcctID2" runat="server" CommitChanges="True" DataField="AcctID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubID2" runat="server" DataField="SubID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edMem_LastReferencedBy2" runat="server" AllowEdit="True"
                                    DataField="Mem_LastReferencedBy">
                                    </px:PXSelector>

<%-- PURCHASE ORDER FIELDS --%>
                                    <px:PXCheckBox ID="edPartEnablePO" runat="server" DataField="EnablePO">
                                    </px:PXCheckBox>
                                    <px:PXSegmentMask ID="edPartPOVendorID" runat="server" DataField="POVendorID" AllowEdit="true" CommitChanges="true">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edPartPOVendorLocationID" runat="server" DataField="POVendorLocationID" AllowEdit="true" AutoRefresh="true">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edPartPONbr" runat="server" DataField="PONbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edPartPOStatus" runat="server" DataField="POStatus" AllowEdit="True">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edPartPOCompleted" runat="server" DataField="POCompleted" AllowEdit="True">
                                    </px:PXCheckBox>
                                    <px:PXTextEdit runat="server" ID="edComment2" DataField="Comment">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Availability" Width="1px" />
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="BranchID" Width="81px" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="LineRef" NullText="<NEW>" Width="85px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LineType" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" Width="100px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubItemID" Width="60px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EquipmentAction" Width="150px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SMEquipmentID" Width="140px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" TextAlign="Right" Width="140px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ComponentID" TextAlign="Right" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EquipmentLineRef" TextAlign="Right" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SuspendedTargetEquipmentID" TextAlign="Right" Width="140px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Warranty" Width="70px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="IsPrepaid" Width="65px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="SiteID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteLocationID" AllowShowHide="Server" Width="81px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="180px" NullText="&lt;SPLIT&gt;" /> 
                                    <px:PXGridColumn DataField="UOM">
                                    </px:PXGridColumn>



<%-- ESTIMATED FIELDS --%>                                    
                                    <px:PXGridColumn DataField="EstimatedQty" TextAlign="Right" Width="75px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryUnitCost" Width="100px" TextAlign="Right" CommitChanges="True" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ManualPrice" Width="65px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryUnitPrice" Width="100px" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryEstimatedTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>



<%-- APPOINTMENT SUM FIELDS --%>
                                    <px:PXGridColumn DataField="ApptQty" TextAlign="Right" Width="75px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryApptTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ApptNumber" TextAlign="Right" Width="75px">
                                    </px:PXGridColumn>



<%-- BILLABLE FIELDS --%>
                                    <px:PXGridColumn DataField="IsBillable" Width="65px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillableQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>



                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="AcctID" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubID" Width="180px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_LastReferencedBy" Width="120px">
                                    </px:PXGridColumn>



<%-- PURCHASE ORDER FIELDS --%>
                                    <px:PXGridColumn DataField="EnablePO" TextAlign="Center" Type="CheckBox" Width="65px" CommitChanges="True" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POVendorID" Width="100px" CommitChanges="true" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POVendorLocationID" Width="100px" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PONbr" TextAlign="Left" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POStatus" TextAlign="Left" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="POCompleted" TextAlign="Center" Type="CheckBox" Width="65px" Visible="false">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment" Width="120"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowFormEdit="True" InitNewRow="True"/>
                        <AutoSize Enabled="True" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSFSSODetPartLine_binLotSerial" CommandSourceID="ds" DependOnGrid="PXGridParts">
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
                                    <px:PXGridColumn DataField="TaxID" Width="81px" AllowUpdate="False" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
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
                                    <px:PXGridColumn DataField="RefNbr" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Confirmed" Width="80px" TextAlign="Center"
                                        Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeBegin_Date" Width="90px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time" Width="90px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeEnd_Time" Width="90px">
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
                        DataField="BillLocationID" DataSourceID="ds" AutoRefresh="True">
                    </px:PXSegmentMask>
                    <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" CommitChanges="true"/>
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
            <px:PXTabItem Text="Staff" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
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
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="EmployeeID" Width="200px" CommitChanges="True" DisplayMode="Hint">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Type" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ServiceLineRef" Width="85px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODetEmployee__InventoryID" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODetEmployee__TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment" Width="200px">
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
            <px:PXTabItem Text="Resource Equipment" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
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
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="SMEquipmentID" Width="150px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSEquipment__Descr" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment" Width="200px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attendees" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid ID="PXGridAttendees" runat="server" DataSourceID="ds" FilesIndicator="False"
                        NoteIndicator="False" SkinID="DetailsInTab" TabIndex="2300" Height="100%"
                        Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceOrderAttendees">

                                <RowTemplate>
                                    <px:PXSegmentMask ID="edCustomerID2" runat="server" DataField="CustomerID" CommitChanges="True" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" CommitChanges="True" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edMem_CustomerContactName" runat="server" DataField="Mem_CustomerContactName">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edConfirmed" runat="server" DataField="Confirmed"
                                        Text="Confirmed">
                                    </px:PXCheckBox>
                                    <px:PXTextEdit ID="edMem_EMail" runat="server" DataField="Mem_EMail">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_Phone1" runat="server" DataField="Mem_Phone1">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edCommentAttendee" runat="server" DataField="Comment">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="CustomerID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ContactID" Width="200px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_CustomerContactName" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Confirmed" TextAlign="Center" Type="CheckBox"
                                        Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_EMail" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_Phone1" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment" Width="200px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="createNewCustomer" >
                                    <AutoCallBack Target="ds" Command="CreateNewCustomer" ></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
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
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
                                        TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
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
                                 <px:PXGridColumn DataField="SrvOrdType" Width="140px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RefNbr" Width="120px" LinkCommand="OpenServiceOrderScreen">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="DocDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="OrderDate" Width="120px">
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
                                <px:PXGridColumn DataField="DocType" Label="Type" Width="90px" />
                                <px:PXGridColumn DataField="RefNbr" Label="Reference Nbr." Width="110px" LinkCommand="ViewPayment"/>
                                <px:PXGridColumn DataField="Status" Label="Status" Width= "80px" />
                                <px:PXGridColumn DataField="AdjDate" Label="Application Date" Width="110px" />
                                <px:PXGridColumn DataField="ExtRefNbr" Label="Payment Ref." Width="110px" />
                                <px:PXGridColumn DataField="PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa" Label="ARPayment-Payment Method" Width="120px" />
                                <px:PXGridColumn DataField="CashAccountID" DisplayFormat="&gt;######" Label="Cash Account" Width="110px" />
                                <px:PXGridColumn DataField="CuryOrigDocAmt" Label="Orig. Amount" Width="130px" />
                                <px:PXGridColumn DataField="CurySOApplAmt" Label="Applied to Orders" Width="130px" />
                                <px:PXGridColumn DataField="CuryUnappliedBal" Label="Available Balance" Width="130px" />
                                <px:PXGridColumn DataField="CuryID" Label="Currency ID" Width="90px" />
                                <px:PXGridColumn DataField="FSAdjust__AdjdAppRefNbr" Label="Applied To Order" Width="130px"/>
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
                    <px:PXFormView ID="edInvoiceInfoForm1" runat="server" DataSourceID="ds" DataMember="ServiceOrderPostedIn" Caption="Invoice Info" RenderStyle="Fieldset">
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
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="90%" Height="360px" Caption="Allocations" CaptionVisible="True" Key="lsPartSelect"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="optform" DesignView="Content" TabIndex="3200">
        <px:PXFormView ID="optform" runat="server" CaptionVisible="False" DataMember="LSFSSODetPartLine_lotseropts" DataSourceID="ds" SkinID="Transparent"
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
                <px:PXButton ID="btnGenerate" runat="server" CommandName="LSFSSODetPartLine_generateLotSerial" CommandSourceID="ds" Height="20px"
                    Text="Generate" />
            </Template>
            <Parameters>
                <px:PXSyncGridParam ControlID="PXGridParts" />
            </Parameters>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" AutoAdjustColumns="True" DataSourceID="ds" Height="192px" Style="height: 192px;" TabIndex="-3036" Width="100%" AllowFilter="true"
            SkinID="Inquire" SyncPosition="true">
            <Parameters>
                <px:PXSyncGridParam ControlID="PXGridParts" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataKeyNames="SrvOrdType,RefNbr,LineNbr,SplitLineNbr" DataMember="partSplits">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                        <px:PXSegmentMask ID="edSubItemID3" runat="server" AutoRefresh="True" DataField="SubItemID" />
                        <px:PXSegmentMask ID="edSiteID3" runat="server" AutoRefresh="True" DataField="SiteID" />
                        <px:PXSegmentMask ID="edLocationID3" runat="server" AutoRefresh="True" DataField="LocationID">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="FSSODetPartSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="FSSODetPartSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="FSSODetPartSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty3" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM3" runat="server" AutoRefresh="True" DataField="UOM">
                            <Parameters>
                                <px:PXControlParam ControlID="PXGridParts" Name="FSSODetPart.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr3" runat="server" AutoRefresh="True" DataField="LotSerialNbr">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="FSSODetPartSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="FSSODetPartSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="FSSODetPartSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate3" runat="server" DataField="ExpireDate" />
                        <%--                        <px:PXCheckBox CommitChanges="True" ID="chkPOCreate2" runat="server" DataField="POCreate" />
                        <px:PXDropDown ID="edPOType2" runat="server" DataField="POType" Enabled="False" />
                        <px:PXSelector ID="edPONbr2" runat="server" DataField="PONbr" Enabled="False" AllowEdit="True" />--%>
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="SplitLineNbr" TextAlign="Right" Width="54px" />
                        <px:PXGridColumn DataField="ParentSplitLineNbr" TextAlign="Right" Width="54px" />
                        <px:PXGridColumn DataField="InventoryID" Width="108px" />
                        <px:PXGridColumn DataField="SubItemID" Width="108px" />
                        <px:PXGridColumn DataField="ShipDate" Width="90px" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="IsAllocated" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn DataField="SiteID" Width="108px" AllowShowHide="Server" AutoCallBack="True" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" DataField="Completed" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="LocationID" Width="108px" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="LotSerialNbr" Width="108px" AutoCallBack="True" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="108px" />
                        <px:PXGridColumn DataField="ShippedQty" TextAlign="Right" Width="108px" />
                        <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" Width="108px" />
                        <%--                        <px:PXGridColumn DataField="ShipmentNbr" />--%>
                        <px:PXGridColumn DataField="UOM" Width="108px" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="ExpireDate" Width="90px" />
                        <px:PXGridColumn AllowNull="False" DataField="POCreate" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="FSSODetPartSplit$RefNoteID$Link" Width="100px" />
                        <%--                        <px:PXGridColumn DataField="POType" Width="80px" Visible="False" RenderEditorText="True" />
                        <px:PXGridColumn DataField="PONbr" Width="80px" Visible="False" />--%>
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
