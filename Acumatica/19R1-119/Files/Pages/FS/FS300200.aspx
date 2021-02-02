<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS300200.aspx.cs" Inherits="Page_FS300200" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" PrimaryView="AppointmentRecords" TypeName="PX.Objects.FS.AppointmentEntry" SuspendUnloading="False">
    <CallbackCommands>
      <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
      <px:PXDSCallbackCommand Name="CloneAppointment" CommitChanges="True" Visible="False"/>
      <px:PXDSCallbackCommand Name="MenuActions" CommitChanges="True"/>
      <px:PXDSCallbackCommand Name="ReportsMenu" CommitChanges="True"/>
      <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
      <px:PXDSCallbackCommand Name="Delete" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" />
      <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
      <px:PXDSCallbackCommand Name="Last" PostData="Self" />
      <px:PXDSCallbackCommand Name="ViewDirectionOnMap" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
      <px:PXDSCallbackCommand Name="ViewStartGPSOnMap" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
      <px:PXDSCallbackCommand Name="ViewCompleteGPSOnMap" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
      <px:PXDSCallbackCommand Name="ValidateAddress" Visible="False" />
      <px:PXDSCallbackCommand Name="openRoomBoard" CommitChanges="True" Visible="False"/>
      <px:PXDSCallbackCommand Name="openStaffSelectorFromStaffTab" Visible="False" RepaintControls="All" />
      <px:PXDSCallbackCommand Name="openStaffSelectorFromServiceTab" Visible="False" RepaintControls="All" />
      <px:PXDSCallbackCommand Name="OpenServiceSelector" Visible="False" RepaintControls="All" />
      <px:PXDSCallbackCommand Name="createNewCustomer" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
      <px:PXDSCallbackCommand Name="SelectCurrentService" Visible="False" />
      <px:PXDSCallbackCommand Name="SelectCurrentStaff" Visible="False" />
      <px:PXDSCallbackCommand Name="OpenPostingDocument" Visible="False" />
      <px:PXDSCallbackCommand Name="OpenInvoiceDocument" Visible="False"/>
      <px:PXDSCallbackCommand Name="OpenBatch" Visible="False"/>
			<px:PXDSCallbackCommand Name="startService" Visible="False" />
			<px:PXDSCallbackCommand Name="completeService" Visible="False" />
      <px:PXDSCallbackCommand Name="QuickProcessOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False"/>
      <px:PXDSCallbackCommand Name="QuickProcess" ShowProcessInfo="true" />
      <px:PXDSCallbackCommand Name="CreatePrepayment" Visible="False" CommitChanges="True"></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="QuickProcessMobile" Visible="False"></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Visible="false" Name="ViewPayment"/>
      <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
      <px:PXDSCallbackCommand Visible="False" Name="OpenScheduleScreen"/>
		</CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="TreeWFStages" TreeKeys="WFStageID" />
        </DataTrees>
  </px:PXDataSource>

  <%-- QuickProcess Smartpanel --%>
  <px:PXSmartPanel 
        ID="PXSmartPanelQuickProcess" 
        runat="server"
        Caption="Process Appointment"
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
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" StartGroup="True" GroupCaption="Appointment Actions" SuppressLabel="True" ColumnWidth="300" />
                <px:PXCheckBox ID="edCloseAppointment" runat="server" AlignLeft="True" DataField="CloseAppointment" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edEmailSignedAppointment" runat="server" AlignLeft="True" DataField="EmailSignedAppointment" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edGenerateInvoiceFromAppointment" runat="server" AlignLeft="True" DataField="GenerateInvoiceFromAppointment" CommitChanges="True">
                </px:PXCheckBox>
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
                      <px:PXGridColumn AllowCheckAll="True" DataField="Mem_Selected" Width="90px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
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
      <px:PXLabel runat="server" Height="66px"></px:PXLabel>
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
    <px:PXFormView ID="mainForm" runat="server" DataSourceID="ds" Style="z-index: 100" NotifyIndicator="True" FilesIndicator="True" ActivityIndicator="True" NoteIndicator="True" Width="100%" DataMember="AppointmentRecords" TabIndex="100" MarkRequired="Dynamic">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize="S" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXSelector ID="edSrvOrdType" runat="server"
                DataField="SrvOrdType" DataSourceID="ds"
                AllowEdit="True">
            </px:PXSelector>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
            </px:PXSelector>
            <px:PXSelector ID="edSORefNbr" runat="server" DataField="SORefNbr"
                DataSourceID="ds" NullText=" <NEW>" AutoRefresh="True" AllowEdit="True"
                CommitChanges="True">
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False">
            </px:PXDropDown>
            <px:PXTreeSelector CommitChanges="True" ID="edWFStageID" runat="server" DataField="WFStageID"
                TreeDataMember="TreeWFStages" TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
                <DataBindings>
                    <px:PXTreeItemBinding TextField="WFStageCD" ValueField="WFStageCD">
                    </px:PXTreeItemBinding>
                </DataBindings>
            </px:PXTreeSelector>
            <px:PXCheckBox ID="edHold" runat="server" CommitChanges="True" DataField="Hold">
            </px:PXCheckBox>
            <px:PXDateTimeEdit ID="ScheduledDateTimeBegin_Date" runat="server" CommitChanges="True"
                DataField="ScheduledDateTimeBegin_Date">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edExecutionDate" runat="server" CommitChanges="True" DataField="ExecutionDate">
            </px:PXDateTimeEdit>
            <px:PXCheckBox ID="edIsRouteAppoinment" runat="server"
                DataField="IsRouteAppoinment">
            </px:PXCheckBox>
            <px:PXCheckBox ID="edIsPrepaymentEnable" runat="server"
                DataField="IsPrepaymentEnable">
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM">
            </px:PXLayoutRule>
            <px:PXFormView ID="ServiceOrderHeader" runat="server" Caption="ServiceOrder Header" DataMember="ServiceOrderRelated" MarkRequired="Dynamic" DataSourceID="ds" RenderStyle="Simple" TabIndex="1500" DefaultControlID="edCustomerID">
                <Template>
                    <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" AllowEdit="True" CommitChanges="True" DataField="CustomerID" DataSourceID="ds">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" CommitChanges="True" AutoRefresh="True" AllowEdit="True">
                    </px:PXSegmentMask>
                    <pxa:PXCurrencyRate ID="edCury" runat="server" DataMember="_Currency_" DataField="AppointmentSelected.CuryID" RateTypeView="currencyinfo" DataSourceID="ds">
                    </pxa:PXCurrencyRate>
                    <px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" DataSourceID="ds" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edBillServiceContractID" runat="server" AutoRefresh="True"
                        CommitChanges="True" DataField="AppointmentSelected.BillServiceContractID" AllowEdit="True" FilterByAllFields="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edBillContractPeriodID" runat="server"  DataField="AppointmentSelected.BillContractPeriodID">
                    </px:PXSelector>
					         <px:PXSegmentMask ID="edProjectID" runat="server" AutoRefresh="True"
                        CommitChanges="True" DataField="ProjectID" AllowEdit="True">
                    </px:PXSegmentMask>
                </Template>
            </px:PXFormView>
            <px:PXSelector ID="edDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID" DisplayMode="Value" AllowEdit = "True" AutoRefresh="True" CommitChanges="True">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" ColumnSpan="2">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" CommitChanges="True">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXPanel ID="edThirdColumn" runat="server" RenderStyle="Simple">
              <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="S" />
              <px:PXMaskEdit ID="edEstimatedDurationTotal" runat="server" DataField="EstimatedDurationTotal" Enabled="False" TextAlign="Right">
              </px:PXMaskEdit>
              <px:PXMaskEdit ID="edActualDurationTotal" runat="server" DataField="ActualDurationTotal" Enabled="False" TextAlign="Right">
              </px:PXMaskEdit>
              <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal">
              </px:PXNumberEdit>
              <px:PXNumberEdit ID="edCuryDocTotal" runat="server" DataField="CuryDocTotal">
              </px:PXNumberEdit>
              <px:PXNumberEdit ID="edCuryCostTotal" runat="server" DataField="CuryCostTotal">
              </px:PXNumberEdit>
              <px:PXNumberEdit ID="edProfitPercent" runat="server" DataField="ProfitPercent">
              </px:PXNumberEdit>
              <px:PXCheckBox ID="edTimeRegistered" runat="server" CommitChanges="True" Enabled="False" DataField="TimeRegistered">
              </px:PXCheckBox>
              <px:PXCheckBox ID="edWaitingForParts" runat="server" DataField="WaitingForParts" Text="Waiting for Parts">
              </px:PXCheckBox>
            </px:PXPanel>
        </Template>
  </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" DataSourceID="ds" DataMember="AppointmentSelected">
    <Items>
      <px:PXTabItem Text="Settings">
          <Template>
          <px:PXFormView ID="edContactAddressForm" runat="server" DataMember="ServiceOrderRelated" DataSourceID="ds" RenderStyle="Simple">
              <Template>
                  <px:PXCheckBox ID="edAllowOverrideContactAddress" runat="server" DataField="AllowOverrideContactAddress" CommitChanges="true"/>
              </Template>
              <ContentStyle BackColor="Transparent"/>
          </px:PXFormView>   
          <px:PXFormView ID="edServiceOrder_Contact" runat="server" Caption="Contact" DataMember="ServiceOrder_Contact" DataSourceID="ds" RenderStyle="Fieldset">
              <Template>
                  <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM"/>
                  <px:PXFormView ID="edContactForm" runat="server" DataMember="ServiceOrderRelated" DataSourceID="ds" RenderStyle="Simple">
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
                  <px:PXFormView ID="edRoomForm" runat="server" DataMember="ServiceOrderRelated" DataSourceID="ds" RenderStyle="Simple">
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
                 <px:PXButton ID="btnViewDirectionOnMap" runat="server" CommandName="ViewDirectionOnMap" CommandSourceID="ds" Text="View On Map" />
                 <px:PXLayoutRule runat="server"/>
             </Template>
             <ContentStyle BackColor="Transparent" BorderStyle="None"/>
          </px:PXFormView>

          <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Scheduled Date And Time" StartGroup="True" ControlSize="XM" LabelsWidth="SM">
          </px:PXLayoutRule>
          <px:PXDateTimeEdit ID="ScheduledDateTimeBegin_Date" runat="server" CommitChanges="True"
              DataField="ScheduledDateTimeBegin_Date">
          </px:PXDateTimeEdit>
          <px:PXDateTimeEdit ID="ScheduledDateTimeBegin_Time" runat="server"
              CommitChanges="True" DataField="ScheduledDateTimeBegin_Time" TimeMode="True">
          </px:PXDateTimeEdit>
          <px:PXDateTimeEdit ID="ScheduledDateTimeEnd_Time" runat="server"
              CommitChanges="True" DataField="ScheduledDateTimeEnd_Time" TimeMode="True">
          </px:PXDateTimeEdit>
          <px:PXCheckBox ID="edHandleManuallyScheduleTime" runat="server"
              DataField="HandleManuallyScheduleTime" AlignLeft="False" CommitChanges="True">
          </px:PXCheckBox>
          <px:PXCheckBox ID="Confirmed" runat="server" CommitChanges="True"
              DataField="Confirmed" Text="Confirmed">
          </px:PXCheckBox>
          <px:PXCheckBox ID="edValidatedByDispatcher" runat="server" DataField="ValidatedByDispatcher">
          </px:PXCheckBox>
          <px:PXLayoutRule runat="server" GroupCaption="Actual Date And Time"
              StartGroup="True" ControlSize="XM" LabelsWidth="SM">
          </px:PXLayoutRule>
          <px:PXDateTimeEdit ID="edExecutionDate" runat="server" CommitChanges="True" 
              DataField="ExecutionDate">
          </px:PXDateTimeEdit>
          <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time" runat="server"
              CommitChanges="True" DataField="ActualDateTimeBegin_Time" TimeMode="True">
          </px:PXDateTimeEdit>
          <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time" runat="server"
              CommitChanges="True" DataField="ActualDateTimeEnd_Time" TimeMode="True">
          </px:PXDateTimeEdit>
          <px:PXCheckBox ID="edHandleManuallyActualTime" runat="server"
              DataField="HandleManuallyActualTime" AlignLeft="False" CommitChanges="True">
          </px:PXCheckBox>
          <px:PXCheckBox ID="Finished" runat="server" CommitChanges="True"
              DataField="Finished" Text="Finished">
          </px:PXCheckBox>
          <px:PXCheckBox ID="edUnreachedCustomer" runat="server" CommitChanges="True"
              DataField="UnreachedCustomer">
          </px:PXCheckBox>
	        <px:PXLayoutRule runat="server" GroupCaption="Service Order Settings" StartGroup="True"/>
	        <px:PXFormView ID="ServiceOrderSettings" runat="server"
	        Caption="Service Order Settings" DataMember="ServiceOrderRelated" MarkRequired="Dynamic"
	        DataSourceID="ds" RenderStyle="Simple" TabIndex="1500">
	          <Template>
	            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM"/>
	            <px:PXTextEdit ID="edCustPORefNbr" runat="server" DataField="CustPORefNbr" AutoRefresh="True" />
	            <px:PXTextEdit ID="edCustWorkOrderRefNbr" runat="server" DataField="CustWorkOrderRefNbr" AutoRefresh="True" />
	            <px:PXDropDown ID="edSeverity" runat="server" DataField="Severity" />
	            <px:PXDropDown ID="edPriority" runat="server" DataField="Priority" />
	            <px:PXSegmentMask ID="edAssignedEmpID" runat="server" DataField="AssignedEmpID" />
	            <px:PXSelector ID="ProblemID" runat="server" AllowEdit="True" DataField="ProblemID" DataSourceID="ds" />
	          </Template>
	        </px:PXFormView>
      </Template>
      </px:PXTabItem>
            <px:PXTabItem Text="Services">
                <Template>
                      <px:PXGrid ID="PXGridServices" runat="server" DataSourceID="ds" SkinID="DetailsInTab"
                        TabIndex="1700" Height="100%" Width="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel
                                DataMember="AppointmentDetServices" DataKeyNames="AppointmentID,AppDetID,SODetID">
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
                                                <px:PXLabel ID="PXLabel1" runat="server"></px:PXLabel>
                                                <px:PXGrid ID="PXGridSelectedEmployees" runat="server" DataSourceID="ds"
                                                    Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="800"
                                                    SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Height="200px">
                                                    <Levels>
                                                        <px:PXGridLevel
                                                            DataMember="EmployeeGridFilter" DataKeyNames="EmployeeID">
                                                            <Columns>
                                                                <px:PXGridColumn
                                                                    AllowCheckAll="True" 
                                                                    DataField="Mem_Selected" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="True">
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
                                            <ActionBar ActionsText="False" DefaultAction="SelectCurrentService" PagerVisible="False">
                                                <CustomItems>
                                                    <px:PXToolBarButton Key="SelectCurrentService">
                                              <AutoCallBack Target="ds" Command="SelectCurrentService" />
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
                                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info" StartGroup="True" StartRow="True">
                                    </px:PXLayoutRule>
                                    <px:PXSegmentMask CommitChanges="True" ID="edServBranchID" runat="server" DataField="BranchID" />
                                    <px:PXSelector ID="edSODetID" runat="server" DataField="SODetID"
                                        CommitChanges="True" NullText="<NEW>" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID"
                                            AllowEdit ="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSAppointmentDetService.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXSegmentMask>
                                    <px:PXDropDown ID="edBillingRule" runat="server" DataField="BillingRule" Size="SM">
                                    </px:PXDropDown>
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edEquipmentAction" runat="server" DataField="EquipmentAction">
                                    </px:PXDropDown>
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
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSAppointmentDetService.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSAppointmentDetService.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                         </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSiteLocationIDServices" runat="server" DataField="SiteLocationID" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSAppointmentDetService.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" CommitChanges="True">
                                    </px:PXSelector>

<%-- ESTIMATED FIELDS --%>    
                                    <px:PXMaskEdit ID="edEstimatedDuration" runat="server" DataField="EstimatedDuration" CommitChanges="True">
                                    </px:PXMaskEdit>
                                    <px:PXNumberEdit ID="edEstimatedQty" runat="server" DataField="EstimatedQty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryEstimatedTranAmt" runat="server" DataField="CuryEstimatedTranAmt">
                                    </px:PXNumberEdit>
<%-- ACTUAL FIELDS --%>
                                    <px:PXCheckBox ID="edKeepActualDateTimes" runat="server" DataField="KeepActualDateTimes">
                                    </px:PXCheckBox>
                                    <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time2" runat="server"
                                        DataField="ActualDateTimeBegin_Time" TimeMode="True" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time2" runat="server"
                                        DataField="ActualDateTimeEnd_Time" TimeMode="True" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edActualDuration" runat="server" DataField="ActualDuration" CommitChanges="True">
                                    </px:PXMaskEdit>
                                    <px:PXNumberEdit ID="Qty" runat="server" DataField="Qty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt">
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

<%-- BILLABLE FIELDS --%>                                    
                                    <px:PXCheckBox ID="edIsBillable" runat="server" DataField="IsBillable" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edBillableQty" runat="server" DataField="BillableQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryBillableTranAmt" runat="server" DataField="CuryBillableTranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXSelector ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True" DisplayMode="Value" AllowEdit = "True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edAcctID" runat="server" CommitChanges="True" DataField="AcctID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edSourceSalesOrderRefNbr" runat="server" AllowEdit="True" DataField="SourceSalesOrderRefNbr"  >
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edServEnablePO" runat="server" DataField="FSSODet__EnablePO" AllowEdit="False">
                                    </px:PXCheckBox>
                                    <px:PXSelector ID="edServPONbr" runat="server" DataField="FSSODet__PONbr" AllowEdit="False">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edServPOStatus" runat="server" DataField="FSSODet__POStatus" AllowEdit="False">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edServPOCompleted" runat="server" DataField="FSSODet__POCompleted" AllowEdit="False">
                                    </px:PXCheckBox>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="BranchID" Width="81px" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="SODetID" CommitChanges="True" NullText="<NEW>" Width="85px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LineType" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubItemID" Width="60px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillingRule" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SMEquipmentID" Width="100px" AutoCallBack = "True" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" TextAlign="Right" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ComponentID" TextAlign="Right" CommitChanges="True" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EquipmentLineRef" TextAlign="Right" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="StaffID" Width="120px" AutoCallBack = "True"  NullText="<SPLIT>">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Warranty" Width="70px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="IsPrepaid" TextAlign="Center" Type="CheckBox" Width="65px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteLocationID" AllowShowHide="Server" Width="81px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UOM" CommitChanges="True">
                                    </px:PXGridColumn>
<%-- ESTIMATED FIELDS --%>                                       
                                    <px:PXGridColumn DataField="EstimatedDuration" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EstimatedQty" TextAlign="Right" Width="75px" CommitChanges="True">
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

<%-- ACTUAL FIELDS --%>
                                    <px:PXGridColumn DataField="KeepActualDateTimes" TextAlign="Center" Type="CheckBox" Width="75px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDateTimeBegin_Time" CommitChanges="True" Width="80px" NullText="<SPLIT>">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDateTimeEnd_Time" CommitChanges="True" Width="80px" NullText="<SPLIT>">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDuration" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="70px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>

<%-- BILLABLE FIELDS --%>                                    
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="65px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillableQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>

                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" Width="108px" />
                                    <px:PXGridColumn DataField="AcctID" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubID" Width="180px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SourceSalesOrderRefNbr" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__EnablePO" TextAlign="Center" Type="CheckBox" Width="65px" Visible="False">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__PONbr" TextAlign="Left" Visible="False">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__POStatus" TextAlign="Left" Visible="False">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__POCompleted" TextAlign="Center" Type="CheckBox" Width="65px" Visible="False">
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
                                <px:PXToolBarButton Key="cmdOpenStaffSelectorFromServiceTab">
                                    <AutoCallBack Target="ds" Command="OpenStaffSelectorFromServiceTab" ></AutoCallBack>
                                </px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdStartService">
                                    <AutoCallBack Target="ds" Command="startService" ></AutoCallBack>
                                </px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdCompleteService">
                                    <AutoCallBack Target="ds" Command="completeService" ></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Inventory Items">
                <Template>
                      <px:PXGrid ID="PXGridParts" runat="server" DataSourceID="ds" SkinID="DetailsInTab"
                        TabIndex="1700" Height="100%" Width="100%" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel
                                DataMember="AppointmentDetParts" DataKeyNames="AppointmentID,AppDetID,SODetID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info" StartGroup="True">
                                    </px:PXLayoutRule>
                                    <px:PXSegmentMask CommitChanges="True" ID="edInvBranchID" runat="server" DataField="BranchID" />
                                    <px:PXSelector ID="edSODetID2" runat="server" DataField="SODetID"
                                        CommitChanges="True" NullText="<NEW>" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXDropDown ID="edStatus2" runat="server" DataField="Status" CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXDropDown ID="edLineType2" runat="server" DataField="LineType"
                                        CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID2" runat="server" DataField="InventoryID" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
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
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                         </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSiteLocationID" runat="server" DataField="SiteLocationID" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector Size="xm" ID="edLotSerialNbr" runat="server" AllowNull="False" DataField="LotSerialNbr" AutoRefresh="True" CommitChanges="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="PXGridParts" Name="FSAppointmentDetPart.siteLocationID" PropertyName="DataValues[&quot;SiteLocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" CommitChanges="True">
                                    </px:PXSelector>
<%-- ESTIMATED FIELDS --%>                                     
                                    <px:PXNumberEdit ID="edEstimatedQty2" runat="server" DataField="EstimatedQty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edManualPrice2" runat="server" DataField="ManualPrice" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edCuryUnitPrice2" runat="server" DataField="CuryUnitPrice" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryEstimatedTranAmt2" runat="server" DataField="CuryEstimatedTranAmt">
                                    </px:PXNumberEdit>
<%-- ACTUAL FIELDS --%>
                                    <px:PXNumberEdit ID="Qty2" runat="server" DataField="Qty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryTranAmt2" runat="server" DataField="CuryTranAmt">
                                    </px:PXNumberEdit>
<%-- BILLABLE FIELDS --%>
                                    <px:PXCheckBox ID="edIsBillable2" runat="server" DataField="IsBillable" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edBillableQty2" runat="server" DataField="BillableQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryBillableTranAmt2" runat="server" DataField="CuryBillableTranAmt">
                                    </px:PXNumberEdit>

                                    <px:PXSelector ID="edProjectTaskID2" runat="server"
                                        DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True"  AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCostCodeID2" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edAcctID2" runat="server" CommitChanges="True" DataField="AcctID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubID2" runat="server" DataField="SubID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edSourceSalesOrderRefNbr2" runat="server" AllowEdit="True" DataField="SourceSalesOrderRefNbr"  >
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edPartEnablePO" runat="server" DataField="FSSODet__EnablePO" AllowEdit="False">
                                    </px:PXCheckBox>
                                    <px:PXSelector ID="edPartPONbr" runat="server" DataField="FSSODet__PONbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edPartPOStatus" runat="server" DataField="FSSODet__POStatus" AllowEdit="False">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edPartPOCompleteds" runat="server" DataField="FSSODet__POCompleted" AllowEdit="False">
                                    </px:PXCheckBox>
                                    <px:PXTextEdit runat="server" ID="edComment2" DataField="Comment"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="BranchID" Width="81px" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="SODetID" CommitChanges="True" NullText="<NEW>" Width="85px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LineType" Width="100px" CommitChanges="True">
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
                                    <px:PXGridColumn DataField="IsPrepaid" TextAlign="Center" Type="CheckBox" Width="65px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteLocationID" AllowShowHide="Server" Width="81px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="180px" NullText="&lt;SPLIT&gt;" CommitChanges="True"/> 
                                    <px:PXGridColumn DataField="UOM" CommitChanges="True">
                                    </px:PXGridColumn>

<%-- ESTIMATED FIELDS --%> 
                                    <px:PXGridColumn DataField="EstimatedQty" TextAlign="Right" Width="75px" CommitChanges="true">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ManualPrice" Width="65px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" Width="100px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryEstimatedTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>
<%-- ACTUAL FIELDS --%>
                                    <px:PXGridColumn DataField="Qty" Width="75px" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>
<%-- BILLABLE FIELDS --%>
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="65px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillableQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>

                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" Width="108px" />
                                    <px:PXGridColumn DataField="AcctID" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubID" Width="180px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SourceSalesOrderRefNbr" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__EnablePO" TextAlign="Center" Type="CheckBox" Width="65px" Visible="False">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__PONbr" TextAlign="Left" Visible="False">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__POStatus" TextAlign="Left" Visible="False">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__POCompleted" TextAlign="Center" Type="CheckBox" Width="65px" Visible="False">
                                    </px:PXGridColumn>

                                    <px:PXGridColumn DataField="Comment" Width="120"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowFormEdit="True" InitNewRow="True"/>
                        <AutoSize Enabled="True" />
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
            <px:PXTabItem Text="Staff">
                <Template>
                    <px:PXGrid ID="PXGridStaff" runat="server" DataSourceID="ds" SkinID="Details" SyncPosition="True" TabIndex="6500" Height="100%" Width="100%">
                        <Levels>
                            <px:PXGridLevel
                                DataMember="AppointmentEmployees" DataKeyNames="AppointmentID, LineNbr">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edEmployeeLineRef" runat="server" DataField="LineRef">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="EmployeeID" runat="server" DataField="EmployeeID"
                                        AllowEdit="True"
                                        CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edType" runat="server" DataField="Type">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edIsDriver" runat="server" DataField="IsDriver"
                                        Text="IsDriver" Width="80px">
                                    </px:PXCheckBox>
                                    <px:PXSelector ID="edServiceLineRef" runat="server" CommitChanges="True" DataField="ServiceLineRef" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edFSAppointmentDetEmployee__InventoryID" runat="server" DataField="FSAppointmentDetEmployee__InventoryID" Enabled="False" AutoRefresh="True" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edFSAppointmentDetEmployee__TranDesc" runat="server" DataField="FSAppointmentDetEmployee__TranDesc" Enabled="False">
                                    </px:PXTextEdit>
                                    <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time3" runat="server"
                                        DataField="ActualDateTimeBegin_Time" TimeMode="True" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time3" runat="server"
                                        DataField="ActualDateTimeEnd_Time" TimeMode="True" CommitChanges="True" />
                                    <px:PXMaskEdit ID="edActualDuration3" runat="server" DataField="ActualDuration" CommitChanges="True">
                                    </px:PXMaskEdit>
                                    <px:PXSegmentMask ID="edCostCodeID4" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSelector ID="edEarningType" runat="server" AutoRefresh="True" CommitChanges="True" DataField="EarningType" DataSourceID="ds">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edLaborItemID" runat="server" DataField="LaborItemID" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="Comment" runat="server" DataField="Comment">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="edApprovedTime" runat="server" DataField="ApprovedTime"
                                        Text="Approved Time">
                                    </px:PXCheckBox>
                                    <px:PXSelector ID="edTimeCardCD" runat="server" DataField="TimeCardCD" AllowEdit ="True">
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="LineRef" Width="85px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EmployeeID" Width="200px"
                                        AllowShowHide="False" CommitChanges="True" DisplayMode="Hint">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Type" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="IsDriver" TextAlign="Center" Type="CheckBox" Width="75px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ServiceLineRef" Width="85px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointmentDetEmployee__InventoryID" Width="100px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointmentDetEmployee__TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="KeepActualDateTimes" TextAlign="Center" Type="CheckBox" Width="75px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDateTimeBegin_Time" CommitChanges="True" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDateTimeEnd_Time" CommitChanges="True" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDuration" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TrackTime" TextAlign="Center" Type="CheckBox" Width="75px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EarningType" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LaborItemID" Width="95px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment" Width="300px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ApprovedTime" Width="80px" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TimeCardCD" Width="80px"/>
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
            <px:PXTabItem Text="Resource Equipment">
                <Template>
                    <px:PXGrid ID="PXGridResourceEquipment" runat="server" DataSourceID="ds" SkinID="Details"
                        TabIndex="2100" Height="100%" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="AppointmentResources" DataKeyNames="AppointmentID,SMEquipmentID">
                                <RowTemplate>
                                    <px:PXSelector ID="edRSMEquipmentID" runat="server" DataField="SMEquipmentID"
                                        AllowEdit="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edFSEquipment__Descr" runat="server"
                                        DataField="FSEquipment__Descr">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="EComment" runat="server" DataField="Comment">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="SMEquipmentID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSEquipment__Descr" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Comment" Width="400px">
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
                  <px:PXLayoutRule runat="server" GroupCaption="Financial Information" StartGroup="True">
                  </px:PXLayoutRule>
                  <px:PXFormView ID="AppointmentBillingInfo" runat="server"
                  Caption="Appointment Billing Info" DataMember="ServiceOrderRelated" MarkRequired="Dynamic"
                  DataSourceID="ds" RenderStyle="Simple" TabIndex="1500">
                    <Template>
                      <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM">
                      </px:PXLayoutRule>
                      <px:PXSelector ID="edBranchID" runat="server" AllowEdit="True" DataField="BranchID" DataSourceID="ds" CommitChanges="True">
                      </px:PXSelector>
                      <px:PXSegmentMask ID="edBillCustomerID" runat="server" DataField="BillCustomerID" CommitChanges="True" AllowEdit="True">
                      </px:PXSegmentMask>
                      <px:PXSegmentMask ID="edBillLocationID" runat="server" DataField="BillLocationID" CommitChanges="true" AutoRefresh="True">
                      </px:PXSegmentMask>
                      <px:PXSelector ID="edTaxZoneID" runat="server" DataField="AppointmentSelected.TaxZoneID" CommitChanges="true"/>
                      <px:PXFormView ID="BillingTabSummary" runat="server"
                      Caption="Billing Summary" DataMember="BillingCycleRelated" MarkRequired="Dynamic" DataSourceID="ds" RenderStyle="Simple" TabIndex="1500">
                          <Template>
                              <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM">
                              </px:PXLayoutRule>
                              <px:PXSelector ID="edBillingCycleCD" runat="server" AllowEdit="False" DataField="BillingCycleCD">
                              </px:PXSelector>
                              <px:PXDropDown ID="edBillingBy" runat="server" AllowEdit="False" DataField="BillingBy">
                              </px:PXDropDown>
                          </Template>
                      </px:PXFormView>
                      <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" 
                      CommitChanges="True" AutoRefresh="True" >
                      </px:PXSegmentMask>
                      <px:PXCheckBox ID="edCommissionable" runat="server" CommitChanges="True" 
                          DataField="Commissionable" AllowEdit="True">
                      </px:PXCheckBox>
                    </Template>
                </px:PXFormView>
              </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Profitability">
                <Template>
                    <px:PXGrid ID="PXGridProfitability" runat="server" DataSourceID="ds" SkinID="Inquire" TabIndex="2100" Height="100%" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="ProfitabilityRecords" DataKeyNames="ItemID">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edProfLineRef" runat="server" DataField="LineRef">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edProfLineType" runat="server" DataField="LineType">
                                    </px:PXDropDown>
                                    <px:PXSelector ID="edProfItemID" runat="server" DataField="ItemID">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edProfDescr" runat="server" DataField="Descr">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="edProfEmployeeID" runat="server" DataField="EmployeeID">
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edProfUnitPrice" runat="server" DataField="UnitPrice">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfEstimatedQty" runat="server" DataField="EstimatedQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfEstimatedAmount" runat="server" DataField="EstimatedAmount">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfActualDuration" runat="server" DataField="ActualDuration">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfActualQty" runat="server" DataField="ActualQty">
                                    </px:PXNumberEdit>
                                    <px:PXMaskEdit ID="edProfActualAmount" runat="server" DataField="ActualAmount">
                                    </px:PXMaskEdit>
                                    <px:PXNumberEdit ID="edProfBillableQty" runat="server" DataField="BillableQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfBillableAmount" runat="server" DataField="BillableAmount">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfUnitCost" runat="server" DataField="UnitCost">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfCostTotal" runat="server" DataField="CostTotal">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfProfit" runat="server" DataField="Profit">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edProfProfitPercent" runat="server" DataField="ProfitPercent">
                                    </px:PXNumberEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineRef" Width="80px" />
                                    <px:PXGridColumn DataField="LineType" Width="120px" />
                                    <px:PXGridColumn DataField="ItemID" Width="120px" />
                                    <px:PXGridColumn DataField="Descr" Width="160px" />
                                    <px:PXGridColumn DataField="EmployeeID" Width="200px" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="UnitPrice" Width="90px" />
                                    <px:PXGridColumn DataField="EstimatedQty" Width="90px" />
                                    <px:PXGridColumn DataField="EstimatedAmount" Width="90px" />
                                    <px:PXGridColumn DataField="ActualDuration" Width="80px" />
                                    <px:PXGridColumn DataField="ActualQty" Width="90px" />
                                    <px:PXGridColumn DataField="ActualAmount" Width="90px" />
                                    <px:PXGridColumn DataField="BillableQty" Width="90px" />
                                    <px:PXGridColumn DataField="BillableAmount" Width="90px" />
                                    <px:PXGridColumn DataField="UnitCost" Width="90px" />
                                    <px:PXGridColumn DataField="CostTotal" Width="90px" />
                                    <px:PXGridColumn DataField="Profit" Width="90px" />
                                    <px:PXGridColumn DataField="ProfitPercent" Width="80px" />
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
                            <px:PXGridLevel DataMember="AppointmentAttendees" DataKeyNames="AppointmentID,AttendeeID">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID"
                                        CommitChanges="True"  AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID"
                                        CommitChanges="True" AutoRefresh="True">
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
                                    <px:PXTextEdit ID="edComment" runat="server" DataField="Comment">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SrvOrdType" Width="50px" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                                    <px:PXGridColumn DataField="CustomerID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ContactID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_CustomerContactName"  Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Confirmed" Width="60px" TextAlign="Center"
                                        Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_EMail" Width="150px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_Phone1" Width="120px">
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
            <px:PXTabItem Text="Pickup/Delivery Items" LoadOnDemand="True" RepaintOnDemand="False">
			    <Template>
                    <px:PXGrid ID="PXGridPickupDelivery" runat="server" TabIndex="4900" DataSourceID="ds"
                        SkinID="DetailsInTab" Height="100%" Width="100%" SyncPosition="True" >
                        <Levels>
                            <px:PXGridLevel
                                DataMember="PickupDeliveryItems" DataKeyNames="AppointmentID, SODetID, InventoryID">
                                <RowTemplate>
                                    <px:PXSegmentMask CommitChanges="True" ID="edPDBranchID" runat="server" DataField="BranchID" />
                                    <px:PXSelector ID="edSODetID3" runat="server" CommitChanges="True" DataField="SODetID" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edPickupDeliveryServiceID" runat="server" CommitChanges="True" AutoRefresh="True" DataField="PickupDeliveryServiceID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXDropDown ID="edServiceType" runat="server" DataField="ServiceType" Size="SM">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID3" runat="server" DataField="InventoryID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" Size="SM">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID3" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edTranDesc3" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXSegmentMask ID="edSiteID3" runat="server" DataField="SiteID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edUOM3" runat="server" AutoRefresh="True" DataField="UOM">
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edQty3" runat="server" DataField="Qty" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edManualPrice3" runat="server" DataField="ManualPrice" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edCuryUnitPrice3" runat="server" DataField="CuryUnitPrice" CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryTranAmt3" runat="server" DataField="CuryTranAmt">
                                    </px:PXNumberEdit>

                                    <px:PXCheckBox ID="edIsBillable3" runat="server" DataField="IsBillable" CommitChanges="True">
                                    </px:PXCheckBox>
                                    <px:PXNumberEdit ID="edBillableQty3" runat="server" DataField="BillableQty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edCuryBillableTranAmt3" runat="server" DataField="CuryBillableTranAmt">
                                    </px:PXNumberEdit>
  
                                    <px:PXSelector ID="edProjectTaskID3" runat="server" DataField="ProjectTaskID" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCostCodeID3" runat="server" DataField="CostCodeID" AutoRefresh="true" />


                                    <px:PXSegmentMask ID="edAcctID3" runat="server" CommitChanges="True" DataField="AcctID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubID3" runat="server" DataField="SubID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="BranchID" Width="81px" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SODetID" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PickupDeliveryServiceID" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ServiceType" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" Width="200px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubItemID" Width="60px" NullText="<SPLIT>" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteID" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UOM" Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" Width="100px" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ManualPrice" Width="65px" TextAlign="Center" Type="CheckBox" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" Width="120px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="150px">
                                    </px:PXGridColumn>

                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="65px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BillableQty" Width="95px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" Width="100px">
                                    </px:PXGridColumn>

                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="150px" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" Width="108px" />

                                    <px:PXGridColumn DataField="AcctID" Width="108px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SubID" Width="180px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True"/>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
      </px:PXTabItem>
            <px:PXTabItem Text="Delivery Notes" BindingContext="mainForm" VisibleExp="DataControls[&quot;edIsRouteAppoinment&quot;].Value == true">
                <Template>
                    <px:PXRichTextEdit ID="edDeliveryNotes" runat="server" DataField="DeliveryNotes"
                        Style="width: 100%;height: 120px" AllowAttached="true" AllowSearch="true"
                        AllowMacros="true" AllowLoadTemplate="false" AllowSourceMode="true">
                        <AutoSize Enabled="True" MinHeight="216" />
                    </px:PXRichTextEdit>
                </Template>
      </px:PXTabItem>
      <px:PXTabItem Text="Prepayments" BindingContext="mainForm" RepaintOnDemand="false" VisibleExp="DataControls[&quot;edIsPrepaymentEnable&quot;].Value == true">
            <Template>
                <px:PXFormView ID="PrepaymentTotals" runat="server" DataMember="ServiceOrderRelated" RenderStyle="Simple" SkinID="Transparent">
                  <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXNumberEdit ID="edSOPrepaymentReceived" runat="server" DataField="SOPrepaymentReceived" Enabled="False" Size="Empty" />
                    <px:PXNumberEdit ID="edSOPrepaymentRemaining" runat="server" DataField="SOPrepaymentRemaining" Enabled="False" Size="Empty" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXNumberEdit ID="edSOCuryUnpaidBalanace" runat="server" DataField="SOCuryUnpaidBalanace" Enabled="False" Size="Empty" />
                    <px:PXNumberEdit ID="edSOCuryBillableUnpaidBalanace" runat="server" DataField="SOCuryBillableUnpaidBalanace" Enabled="False" Size="Empty" />
                  </Template>
                </px:PXFormView>
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
              <px:PXFormView ID="formG" runat="server" DataSourceID="ds" Style="z-index: 100; left: 18px; top: 36px;" Width="100%" DataMember="AppointmentSelected" CaptionVisible="False" SkinID="Transparent">
                  <Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Appointment Totals" />
					<px:PXNumberEdit ID="edCuryEstimatedLineTotal" runat="server" DataField="CuryEstimatedLineTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edCuryBillableLineTotal" runat="server" DataField="CuryBillableLineTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edGridCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edGridCuryDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" Size="Empty" />
					<px:PXNumberEdit ID="edAppCompletedBillableTotal" runat="server" DataField="AppCompletedBillableTotal" Enabled="False" Size="Empty" />
					<px:PXFormView ID="ServiceOrderTotalsTab" runat="server" DataMember="ServiceOrderRelated" RenderStyle="Simple" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Service Order Totals" />
							<px:PXNumberEdit ID="edSOCuryDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" Size="Empty" />
							<px:PXNumberEdit ID="edSOCuryCompletedBillableTotal" runat="server" DataField="SOCuryCompletedBillableTotal" Enabled="False" Size="Empty" />
						</Template>
					</px:PXFormView>  
					<px:PXLayoutRule runat="server" StartColumn="True"/>
					<px:PXFormView ID="PrepaymentTotalsTab" runat="server" DataMember="ServiceOrderRelated" RenderStyle="Simple" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Prepayment Totals" />
							<px:PXNumberEdit ID="edSOPrepaymentReceivedT" runat="server" DataField="SOPrepaymentReceived" Enabled="False" Size="Empty" />
							<px:PXNumberEdit ID="edSOPrepaymentAppliedT" runat="server" DataField="SOPrepaymentApplied" Enabled="False" Size="Empty" />
							<px:PXNumberEdit ID="edSOPrepaymentRemainingT" runat="server" DataField="SOPrepaymentRemaining" Enabled="False" Size="Empty" />
							<px:PXNumberEdit ID="edSOCuryUnpaidBalanaceT" runat="server" DataField="SOCuryUnpaidBalanace" Enabled="False" Size="Empty" />
							<px:PXNumberEdit ID="edSOCuryBillableUnpaidBalanaceT" runat="server" DataField="SOCuryBillableUnpaidBalanace" Enabled="False" Size="Empty" />
						</Template>
					</px:PXFormView>
                  </Template>
              </px:PXFormView>
          </Template>
      </px:PXTabItem>
      <px:PXTabItem Text="Other Information" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False" >
            <Template>
                <px:PXPanel ID="PXPanel5" runat="server" SkinID="transparent">
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ></px:PXLayoutRule>
                    <px:PXFormView ID="edSourceInfoForm" runat="server" Caption="Source Info" DataMember="AppointmentSelected"
                    RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector ID="edServiceContractID" runat="server" DataField="ServiceContractID" AllowEdit="True" DataSourceID="ds" Enabled="False">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edScheduleID" runat="server" DataField="ScheduleID" Enabled="False" AllowEdit="True">
		                      <LinkCommand Target="ds" Command="OpenScheduleScreen" />
		                    </px:PXTextEdit>
                            <px:PXTextEdit ID="edRecurrenceDescription" runat="server" DataField="ScheduleRecord.RecurrenceDescription" Enabled="false">
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="edLocationForm" runat="server" Caption="Location" DataMember="AppointmentSelected"
                    RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="S" ControlSize="S">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edMapLatitude" runat="server" DataField="MapLatitude" CommitChanges = "True" Enabled="False" Size="S">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edMapLongitude" runat="server" DataField="MapLongitude" CommitChanges = "True" Enabled="False" SuppressLabel="True" Size="S">
                            </px:PXNumberEdit>
                            <px:PXButton ID="btnViewAppointmentLocationOnMap" runat="server" Height="24px"
                                Text="View on Map">
                                <AutoCallBack Command="ViewDirectionOnMap" Target="ds">
                                </AutoCallBack>
                            </px:PXButton>
                            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="S" ControlSize="S">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edGPSLatitudeStart" runat="server" DataField="GPSLatitudeStart" CommitChanges="True" Enabled="False" >
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edGPSLongitudeStart" runat="server" DataField="GPSLongitudeStart" CommitChanges="True" Enabled="False" SuppressLabel="True">
                            </px:PXNumberEdit>
                            <px:PXButton ID="btnViewStartGPSOnMap" runat="server" Height="24px"
                                Text="View on Map">
                                <AutoCallBack Command="ViewStartGPSOnMap" Target="ds">
                                </AutoCallBack>
                            </px:PXButton>
                            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="S" ControlSize="S">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edGPSLatitudeComplete" runat="server" DataField="GPSLatitudeComplete" CommitChanges = "True" Enabled="False">
                            </px:PXNumberEdit>
                            <px:PXNumberEdit ID="edGPSLongitudeComplete" runat="server" DataField="GPSLongitudeComplete" CommitChanges = "True" Enabled="False" SuppressLabel="True">
                            </px:PXNumberEdit>
                            <px:PXButton ID="btnViewCompleteGPSOnMap" runat="server" Height="24px"
                                Text="View on Map">
                                <AutoCallBack Command="ViewCompleteGPSOnMap" Target="ds">
                                </AutoCallBack>
                            </px:PXButton>
                            <px:PXTextEdit ID="edMem_GPSLatitudeLongitude" runat="server"
                                DataField="Mem_GPSLatitudeLongitude" AlignLeft="True" Enabled = "False" Visible="False">
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="edRouteInfoForm" runat="server" DataMember="AppointmentSelected" Caption="Route Info" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" AllowEdit="True" Enabled="false">
                            </px:PXSelector>
                            <px:PXSelector ID="edRouteDocumentID" runat="server" DataField="RouteDocumentID" Enabled="false" AllowEdit="true">
                            </px:PXSelector>
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXFormView ID="edInvoiceInfoForm" runat="server" DataSourceID="ds" DataMember="AppointmentPostedIn" Caption="Invoice Info" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" />
                            <px:PXTextEdit ID="edFSPostBatchBatchNbr" runat="server" DataField="BatchNbr" Enabled="False">
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
                    <px:PXFormView ID="edSignatureForm" runat="server" DataMember="AppointmentSelected" Caption="Signature" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM"/>
                            <px:PXTextEdit ID="edFullNameSignature" runat="server" DataField="FullNameSignature">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edAdditionalCommentsCustomer" runat="server" DataField="AdditionalCommentsCustomer">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edAdditionalCommentsStaff" runat="server" DataField="AdditionalCommentsStaff">
                            </px:PXTextEdit>
                            <px:PXCheckBox ID="edAgreementSignature" runat="server" AlignLeft="True" DataField="AgreementSignature" Size="XXL">
                            </px:PXCheckBox>
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartRow="True"/>
                </px:PXPanel>
                <px:PXFormView ID="edCommentsForm" runat="server" DataMember="AppointmentSelected"
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
</asp:Content>
