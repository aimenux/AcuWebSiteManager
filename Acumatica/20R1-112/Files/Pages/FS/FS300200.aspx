<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS300200.aspx.cs" Inherits="Page_FS300200" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" PrimaryView="AppointmentRecords" TypeName="PX.Objects.FS.AppointmentEntry" SuspendUnloading="False">
    <CallbackCommands>
      <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
      <px:PXDSCallbackCommand Name="CloneAppointment" CommitChanges="True" Visible="False" />
      <px:PXDSCallbackCommand Name="MenuActions" CommitChanges="True" />
      <px:PXDSCallbackCommand Name="ReportsMenu" CommitChanges="True" />
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
      <px:PXDSCallbackCommand Name="OpenINPostingDocument" Visible="False" />
      <px:PXDSCallbackCommand Name="OpenInvoiceDocument" Visible="False"/>
      <px:PXDSCallbackCommand Name="OpenBatch" Visible="False"/>
      <px:PXDSCallbackCommand Name="StartItemLine" Visible="False" />
      <px:PXDSCallbackCommand Name="CompleteItemLine" Visible="False" />
      <px:PXDSCallbackCommand Name="CancelItemLine" Visible="False" />
      <px:PXDSCallbackCommand Name="StartStaff" Visible="False" />
      <px:PXDSCallbackCommand Name="CompleteStaff" Visible="False" />

      <px:PXDSCallbackCommand Name="StartTravelMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="CompleteTravelMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="StartServiceMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="StartItemMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="StartForAssignedStaffMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="CompleteServiceMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="CompleteItemMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="StartStaffMobile" Visible="False" />

      <px:PXDSCallbackCommand Name="QuickProcessOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
      <px:PXDSCallbackCommand Name="QuickProcess" ShowProcessInfo="true" />
      <px:PXDSCallbackCommand Name="CreatePrepayment" Visible="False" CommitChanges="True" />
      <px:PXDSCallbackCommand Name="QuickProcessMobile" Visible="False" />
      <px:PXDSCallbackCommand Name="ViewPayment" Visible="false" />
      <px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
      <px:PXDSCallbackCommand Name="DetailsPasteLine" Visible="False" CommitChanges="True" DependOnGrid="PXGridDetails" />
      <px:PXDSCallbackCommand Name="DetailsResetOrder" Visible="False" CommitChanges="True" DependOnGrid="PXGridDetails" />
      <px:PXDSCallbackCommand Name="OpenSourceDocument" Visible="False"/>
      <px:PXDSCallbackCommand Name="OpenScheduleScreen" Visible="False"/>
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
  <px:PXSmartPanel ID="PXSmartPanelStaffSelector" runat="server" Caption="Add Staff" CaptionVisible="True" Key="StaffSelectorFilter"
      AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGridStaff" ShowAfterLoad="True" Width="1070px" Height="760px" CloseAfterAction ="True"
      AutoReload="True" LoadOnDemand="True" TabIndex="8500" AutoRepaint="true" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AllowResize="True">
      <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100px" DataMember="StaffSelectorFilter" TabIndex="700" SkinID="Transparent">
          <Template>
              <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>
              <px:PXSelector ID="edServiceLineRef2" runat="server" CommitChanges="True" DataField="ServiceLineRef" AutoRefresh="True" DisplayMode="Value"/>
              <px:PXTextEdit ID="edPostalCode" runat="server" DataField="PostalCode" SuppressLabel="False"/>
              <px:PXSelector ID="edGeoZoneID" runat="server" DataField="GeoZoneID" CommitChanges="True"/>
              <px:PXPanel ID="PXPanelGrids" runat="server" RenderStyle="Simple">
                  <px:PXGrid ID="PXGridServiceSkills" runat="server" DataSourceID="ds" Style="z-index: 100" Width="600px" SkinID="Inquire" TabIndex="900"
                  SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True">
                    <Levels>
                        <px:PXGridLevel DataMember="SkillGridFilter" DataKeyNames="SkillCD">
                            <Columns>
                                <px:PXGridColumn DataField="Mem_Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
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
                  <px:PXGrid ID="PXGridAvailableStaff" runat="server" DataSourceID="ds" Style="z-index: 100" Width="380px" SkinID="Inquire" TabIndex="920"
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
                    <AutoSize Enabled="True" MinHeight="602" />
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
      CloseAfterAction="True" AutoReload="True" HideAfterAction="False" LoadOnDemand="True" TabIndex="8500" AutoRepaint="True">
      <px:PXFormView ID="PXFormViewFilter" runat="server"  TabIndex="1600" SkinID="Transparent"
          DataMember="ServiceSelectorFilter" DataSourceID="ds">
          <Template>
              <px:PXSelector ID="edServiceClassID" runat="server" DataField="ServiceClassID"
                  DataSourceID="ds" Size="M" AutoRefresh="True" CommitChanges="True">
              </px:PXSelector>
              <px:PXLabel ID="PXLabel1" runat="server"></px:PXLabel>
              <px:PXGrid ID="PXGridSelectedEmployees" runat="server" DataSourceID="ds"
                  Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="800" AutoAdjustColumns="True"
                  SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Height="200px">
                  <Levels>
                      <px:PXGridLevel
                          DataMember="EmployeeGridFilter" DataKeyNames="EmployeeID">
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
      <px:PXPanel ID="pnlCloseButton" runat="server" SkinID="Buttons">
          <px:PXButton ID="PXButtonCloseServiceSelector" runat="server" DialogResult="Cancel" Text="OK"/>
      </px:PXPanel>
  </px:PXSmartPanel>
  <%--/ Service Selector --%>

  <%-- Log Action SmartPanel --%>
  <px:PXSmartPanel ID="PXLogActionSmartPanel" Key="LogActionFilter" runat="server" Caption="Perform Action" CaptionVisible="True" AutoCallBack-Target="LogActionFilter" AutoCallBack-Command="Refresh" Overflow="Hidden" Width="600px" AutoReload="True" AutoRepaint="True">
      <px:PXFormView ID="PXLogActionForm" runat="server"  TabIndex="1600" SkinID="Transparent"
          DataMember="LogActionFilter" DataSourceID="ds">
          <Template>
              <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize="M" LabelsWidth="S" />
              <px:PXDropDown ID="edActionLog" runat="server" DataField="Action" CommitChanges="True"/>
              <px:PXDropDown ID="edTypeLog" runat="server" DataField="Type" CommitChanges="True"/>
              <px:PXCheckBox ID="edMeLogAction" runat="server" CommitChanges="True" DataField="Me" />
              <px:PXDateTimeEdit ID="edLogTime" runat="server" CommitChanges="True" DataField="LogTime_Time" TimeMode="True"/>
              <px:PXSelector ID="edDetailsLineRef" runat="server" DataSourceID="ds" DataField="DetLineRef" CommitChanges="True" />
          </Template>
      </px:PXFormView>
      <px:PXGrid ID="PXLogTravelLinesGrid" runat="server" DataSourceID="ds"
          Style="z-index: 100" SkinID="Inquire" TabIndex="800"
          SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Width="100%" Height="400px" FilesIndicator="false" NoteIndicator ="false">
          <Levels>
              <px:PXGridLevel
                  DataMember="LogActionTravelDetails" DataKeyNames="LineRef">
                  <Columns>
                      <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                      <px:PXGridColumn DataField="BAccountID" DisplayMode="Hint"/>
                      <px:PXGridColumn DataField="LineRef"/>
                      <px:PXGridColumn DataField="InventoryID"/>
                      <px:PXGridColumn DataField="Descr"/>
                      <px:PXGridColumn DataField="Travel" TextAlign="Center" Type="CheckBox"/>
                      <px:PXGridColumn DataField="DateTimeBegin_Time" CommitChanges="True"/>
                  </Columns>
              </px:PXGridLevel>
          </Levels>
          <AutoSize Enabled="True" MinHeight="250" />
          <Mode AllowAddNew="False" AllowDelete="False" />
      </px:PXGrid>
      <px:PXGrid ID="PXLogServiceLinesGrid" runat="server" DataSourceID="ds"
          Style="z-index: 100" SkinID="Inquire" TabIndex="800"
          SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Width="100%" Height="400px" FilesIndicator="false" NoteIndicator ="false">
          <Levels>
              <px:PXGridLevel
                  DataMember="LogActionServiceDetails" DataKeyNames="LineRef">
                  <Columns>
                      <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                      <px:PXGridColumn DataField="BAccountID" DisplayMode="Hint"/>
                      <px:PXGridColumn DataField="LineRef"/>
                      <px:PXGridColumn DataField="InventoryID"/>
                      <px:PXGridColumn DataField="Descr"/>
                      <px:PXGridColumn DataField="Travel" TextAlign="Center" Type="CheckBox"/>
                      <px:PXGridColumn DataField="DateTimeBegin_Time" CommitChanges="True"/>
                  </Columns>
              </px:PXGridLevel>
          </Levels>
          <AutoSize Enabled="True" MinHeight="250" />
          <Mode AllowAddNew="False" AllowDelete="False" />
      </px:PXGrid>
      <px:PXGrid ID="PXServicesBasedOnGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="900"
        SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="ServicesLogAction" DataKeyNames="AppointmentID,AppDetID,SODetID">
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                    <px:PXGridColumn DataField="LineRef"/>
                    <px:PXGridColumn DataField="InventoryID"/>
                    <px:PXGridColumn DataField="Descr"/>
                    <px:PXGridColumn DataField="EstimatedDuration" CommitChanges="True"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="300" />
        <Mode AllowAddNew="False" AllowDelete="False" />
      </px:PXGrid>
      <px:PXGrid ID="PXLogStaffLinesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="900"
        SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="LogStaffActionDetails" DataKeyNames="AppointmentID,LineNbr">
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                    <px:PXGridColumn DataField="LineRef"/>
                    <px:PXGridColumn DataField="BAccountID" DisplayMode="Hint"/>
                    <px:PXGridColumn DataField="InventoryID"/>
                    <px:PXGridColumn DataField="Descr"/>
                    <px:PXGridColumn DataField="EstimatedDuration" CommitChanges="True"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="300" />
        <Mode AllowAddNew="False" AllowDelete="False" />
      </px:PXGrid>
      <px:PXGrid ID="PXLogStartActionStaffLinesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Inquire" TabIndex="900"
        SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True">
        <Levels>
            <px:PXGridLevel DataMember="StaffMemberLogStartAction" DataKeyNames="DocID, BAccountID">
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                    <px:PXGridColumn DataField="BAccountID" DisplayMode="Hint"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="300" />
        <Mode AllowAddNew="False" AllowDelete="False" />
      </px:PXGrid>

      <px:PXPanel ID="pnlCloseButtonLog" runat="server" SkinID="Buttons">
          <px:PXButton ID="startBtnLog" runat="server" DialogResult="OK" Text="OK" />
          <px:PXButton ID="cancelBtnLog" runat="server" DialogResult="Cancel" Text="Cancel"/>
      </px:PXPanel>
  </px:PXSmartPanel>
  <%--/ Log Action SmartPanel --%>
  
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
                        CommitChanges="True" DataField="ProjectID" AllowEdit="True" AllowAddNew="True">
                    </px:PXSegmentMask>
                </Template>
            </px:PXFormView>
            <px:PXSelector ID="edDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID" AllowEdit = "True" AutoRefresh="True" CommitChanges="True">
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
          <px:PXLayoutRule runat="server" Merge="True" />
          <px:PXDateTimeEdit ID="ScheduledDateTimeEnd_Time" runat="server"
              CommitChanges="True" DataField="ScheduledDateTimeEnd_Time" TimeMode="True">
          </px:PXDateTimeEdit>
          <px:PXCheckBox ID="edHandleManuallyScheduleTime" runat="server"
              DataField="HandleManuallyScheduleTime" AlignLeft="False" CommitChanges="True">
          </px:PXCheckBox>
          <px:PXLayoutRule runat="server" />
          <px:PXDropDown ID="edROOptimizationStatus" runat="server" DataField="ROOptimizationStatus" />    
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
          <px:PXLayoutRule runat="server" Merge="True" />
          <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time" runat="server"
              CommitChanges="True" DataField="ActualDateTimeEnd_Time" TimeMode="True">
          </px:PXDateTimeEdit>
          <px:PXCheckBox ID="edHandleManuallyActualTime" runat="server"
              DataField="HandleManuallyActualTime" AlignLeft="False" CommitChanges="True">
          </px:PXCheckBox>
          <px:PXLayoutRule runat="server" />
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
            <px:PXTabItem Text="Details">
        <Template>
          <px:PXGrid ID="PXGridDetails" runat="server" DataSourceID="ds" SkinID="DetailsInTab" TabIndex="1700" Height="100%" Width="100%" SyncPosition="True">
            <Levels>
              <px:PXGridLevel DataMember="AppointmentDetails" DataKeyNames="AppointmentID,AppDetID,SODetID">
                <RowTemplate>
                  <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Detail Info" StartGroup="True" StartRow="True" />
                  <px:PXSegmentMask ID="edUBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
                  <px:PXTextEdit ID="edULineRef" runat="server" DataField="LineRef" NullText="<NEW>" />
                  <px:PXDropDown ID="edUStatus" runat="server" DataField="Status" CommitChanges="True" />
                  <px:PXDropDown ID="edULineType" runat="server" DataField="LineType" AutoRefresh="True" CommitChanges="True" />
                  <px:PXSelector ID="edPickupDeliveryAppLineRef" runat="server" DataField="PickupDeliveryAppLineRef" CommitChanges="True" AutoRefresh="True" />
                  <px:PXSegmentMask ID="edUInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                    <Parameters>
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
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
                  <px:PXSegmentMask ID="edUSiteID" runat="server" DataField="SiteID" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
                    <Parameters>
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                    </Parameters>
                  </px:PXSegmentMask>
                  <px:PXSegmentMask ID="edUSiteLocationID" runat="server" DataField="SiteLocationID" AutoRefresh="True" CommitChanges="True">
                    <Parameters>
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                    </Parameters>
                  </px:PXSegmentMask>
                  <px:PXSelector ID="edULotSerialNbr" runat="server" DataField="LotSerialNbr" Size="XM" AllowNull="False" AutoRefresh="True" CommitChanges="True">
                    <Parameters>
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                      <px:PXControlParam ControlID="PXGridDetails" Name="FSAppointmentDet.siteLocationID" PropertyName="DataValues[&quot;SiteLocationID&quot;]" Type="String" />
                    </Parameters>
                  </px:PXSelector>
                  <px:PXSelector ID="edUUOM" runat="server" DataField="UOM" CommitChanges="True" />
                  <px:PXMaskEdit ID="edUEstimatedDuration" runat="server" DataField="EstimatedDuration" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUEstimatedQty" runat="server" DataField="EstimatedQty" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges="True" />
                  <px:PXCheckBox ID="edUManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUCuryEstimatedTranAmt" runat="server" DataField="CuryEstimatedTranAmt" />
                  <px:PXMaskEdit ID="edUActualDuration" runat="server" DataField="ActualDuration" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUQty" runat="server" DataField="Qty" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUCuryTranAmt" runat="server" DataField="CuryTranAmt" />
                  <px:PXCheckBox ID="edUIsBillable" runat="server" DataField="IsBillable" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUBillableQty" runat="server" DataField="BillableQty" />
                  <px:PXNumberEdit ID="edUCuryBillableExtPrice" runat="server" DataField="CuryBillableExtPrice" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUDiscPct" runat="server" DataField="DiscPct" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUCuryDiscAmt" runat="server" DataField="CuryDiscAmt" CommitChanges="True" />
                  <px:PXNumberEdit ID="edUCuryBillableTranAmt" runat="server" DataField="CuryBillableTranAmt" />
                  <px:PXCheckBox ID="edUFSSODet__EnablePO" runat="server" DataField="FSSODet__EnablePO" AllowEdit="False" />
                  <px:PXSelector ID="edUTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />  
                  <px:PXSelector ID="edUProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True" DisplayMode="Value" AllowEdit="True" />
                  <px:PXSegmentMask ID="edUCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                  <px:PXSegmentMask ID="edUAcctID" runat="server" CommitChanges="True" DataField="AcctID" AutoRefresh="True" />
                  <px:PXNumberEdit ID="edUCoveredQty" runat="server" DataField="CoveredQty" />
                  <px:PXNumberEdit ID="edUExtraUsageQty" runat="server" DataField="ExtraUsageQty" />
                  <px:PXNumberEdit ID="edUCuryExtraUsageUnitPrice" runat="server" DataField="CuryExtraUsageUnitPrice" />
                  <px:PXCheckBox ID="edUFSSODet__POCompleted" runat="server" DataField="FSSODet__POCompleted" AllowEdit="False" />
                  <px:PXSelector ID="edUFSSODet__PONbr" runat="server" DataField="FSSODet__PONbr" AllowEdit="True" />
                  <px:PXTextEdit ID="edUFSSODet__POStatus" runat="server" DataField="FSSODet__POStatus" AllowEdit="False" />
                  <px:PXCheckBox ID="edUIsPrepaid" runat="server" DataField="IsPrepaid" />
                  <px:PXSelector ID="edUSourceSalesOrderRefNbr" runat="server" AllowEdit="True" DataField="SourceSalesOrderRefNbr" />
                  <px:PXCheckBox ID="edUContractRelated" runat="server" DataField="ContractRelated" />
                  <px:PXSelector ID="edUSODetID" runat="server" DataField="SODetID" CommitChanges="True" AutoRefresh="True" />
                  <px:PXSegmentMask ID="edUSubID" runat="server" DataField="SubID" AutoRefresh="True" />
                  <px:PXDropDown ID="edServiceType" runat="server" DataField="ServiceType"/>
                  <px:PXSegmentMask ID="edPickupDeliveryServiceID" runat="server" DataField="PickupDeliveryServiceID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" />
                  <px:PXTextEdit ID="edUComment" runat="server" DataField="Comment"/>
                </RowTemplate>
                <Columns>
                  <px:PXGridColumn DataField="BranchID" RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                  <px:PXGridColumn DataField="LineRef" />
                  <px:PXGridColumn DataField="Status" CommitChanges="True" />
                  <px:PXGridColumn DataField="LineType" RenderEditorText="True" MatrixMode="True" CommitChanges="True" />
                  <px:PXGridColumn DataField="PickupDeliveryAppLineRef" CommitChanges="True"/>
                  <px:PXGridColumn DataField="InventoryID" CommitChanges="True" AllowDragDrop="True" />
                  <px:PXGridColumn DataField="SubItemID" NullText="<SPLIT>" CommitChanges="True" />
                  <px:PXGridColumn DataField="BillingRule" CommitChanges="True" />
                  <px:PXGridColumn DataField="TranDesc" CommitChanges="True" AllowDragDrop="True" />
                  <px:PXGridColumn DataField="EquipmentAction" CommitChanges="True" />
                  <px:PXGridColumn DataField="SMEquipmentID" AutoCallBack = "True" CommitChanges="True" />
                  <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="ComponentID" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="EquipmentLineRef" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="StaffID" AutoCallBack = "True"  NullText="<SPLIT>" />
                  <px:PXGridColumn DataField="Warranty" TextAlign="Center" Type="CheckBox" />
                  <px:PXGridColumn DataField="SiteID" CommitChanges="True" />
                  <px:PXGridColumn DataField="SiteLocationID" AllowShowHide="Server" NullText="<SPLIT>" CommitChanges="True" />
                  <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" NullText="&lt;SPLIT&gt;" CommitChanges="True"/> 
                  <px:PXGridColumn DataField="UOM" CommitChanges="True" AllowDragDrop="True" />
                  <px:PXGridColumn DataField="EstimatedDuration" CommitChanges="True" />
                  <px:PXGridColumn DataField="EstimatedQty" TextAlign="Right" CommitChanges="True" AllowDragDrop="True" />
                  <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="ManualPrice" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                  <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="CuryEstimatedTranAmt" TextAlign="Right" />
                  <px:PXGridColumn DataField="ActualDuration" CommitChanges="True" />
                  <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
                  <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                  <px:PXGridColumn DataField="BillableQty" AllowDragDrop="True" />
                  <px:PXGridColumn DataField="CuryBillableExtPrice" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="DiscPct" AllowNull="False" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" AllowNull="False" CommitChanges="True" />
                  <px:PXGridColumn DataField="CuryBillableTranAmt" TextAlign="Right" />
                  <px:PXGridColumn DataField="FSSODet__EnablePO" TextAlign="Center" Type="CheckBox" />
                  <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" CommitChanges="true"/>
                  <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" DisplayMode="Hint" />
                  <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                  <px:PXGridColumn DataField="AcctID" CommitChanges="true" />
                  <px:PXGridColumn DataField="RefNbr" />
                  <px:PXGridColumn DataField="CoveredQty" />
                  <px:PXGridColumn DataField="LineNbr" />
                  <px:PXGridColumn DataField="ExtraUsageQty" />
                  <px:PXGridColumn DataField="CuryExtraUsageUnitPrice" />
                  <px:PXGridColumn DataField="FSSODet__POCompleted" TextAlign="Center" Type="CheckBox" />
                  <px:PXGridColumn DataField="FSSODet__PONbr" TextAlign="Left" />
                  <px:PXGridColumn DataField="FSSODet__POStatus" TextAlign="Left" />
                  <px:PXGridColumn DataField="IsPrepaid" TextAlign="Center" Type="CheckBox" />
                  <px:PXGridColumn DataField="SourceSalesOrderRefNbr" />
                  <px:PXGridColumn DataField="ContractRelated" TextAlign="Center" Type="CheckBox" />
                  <px:PXGridColumn DataField="SrvOrdType" />
                  <px:PXGridColumn DataField="SODetID" CommitChanges="True"/>
                  <px:PXGridColumn DataField="SortOrder" />
                  <px:PXGridColumn DataField="SubID" />
                  <px:PXGridColumn DataField="ServiceType" />
                  <px:PXGridColumn DataField="PickupDeliveryServiceID" />
                  <px:PXGridColumn DataField="Comment" />
                  <px:PXGridColumn DataField="CanBeStarted" Type="CheckBox" />
                  <px:PXGridColumn DataField="CanBeCompleted" Type="CheckBox" />
                  <px:PXGridColumn DataField="CanBeCancelled" Type="CheckBox" />
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
                  <AutoCallBack Target="ds" Command="OpenServiceSelector" ></AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="cmdOpenStaffSelectorFromServiceTab">
                  <AutoCallBack Target="ds" Command="OpenStaffSelectorFromServiceTab" ></AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="btnStartItemLine" DependOnGrid="PXGridDetails" StateColumn="CanBeStarted">
                  <AutoCallBack Command="StartItemLine" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="btnCompleteItemLine" DependOnGrid="PXGridDetails" StateColumn="CanBeCompleted">
                  <AutoCallBack Command="CompleteItemLine" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="btnCancelItemLine" DependOnGrid="PXGridDetails" StateColumn="CanBeCancelled">
                  <AutoCallBack Command="CancelItemLine" Target="ds" />
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
             <px:PXTabItem Text="Staff">
                <Template>
                    <px:PXGrid ID="PXGridStaff" runat="server" DataSourceID="ds" SkinID="Details" SyncPosition="True" TabIndex="6500" Height="100%" Width="100%">
                        <Levels>
                            <px:PXGridLevel
                                DataMember="AppointmentServiceEmployees" DataKeyNames="AppointmentID, LineNbr">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edEmployeeLineRef" runat="server" DataField="LineRef">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="edStaffEmployeeID" runat="server" DataField="EmployeeID"
                                        AllowEdit="True"
                                        CommitChanges="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edServiceLineRef" runat="server" CommitChanges="True" DataField="ServiceLineRef" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edFSAppointmentServiceEmployee__InventoryID" runat="server" DataField="FSAppointmentServiceEmployee__InventoryID" Enabled="False" AutoRefresh="True" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edFSAppointmentServiceEmployee__TranDesc" runat="server" DataField="FSAppointmentServiceEmployee__TranDesc" Enabled="False">
                                    </px:PXTextEdit>
                                    <px:PXSegmentMask ID="edStaffDfltProjectID" runat="server" DataField="DfltProjectID" AllowEdit="True" CommitChanges="True" AutoRefresh="True"/>  
                                    <px:PXSelector ID="edStaffDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID" AllowEdit="True" AutoRefresh="True" CommitChanges="True"/>
                                    <px:PXSegmentMask ID="edCostCodeID4" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSelector ID="edEarningType" runat="server" AutoRefresh="True" CommitChanges="True" DataField="EarningType" DataSourceID="ds">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edLaborItemID" runat="server" DataField="LaborItemID" AllowEdit="True" CommitChanges="True" AutoRefresh="True">
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edPrimaryDriver" runat="server" DataField="PrimaryDriver"/>
                                    <px:PXCheckBox ID="edIsDriver" runat="server" DataField="IsDriver"
                                        Text="IsDriver" Width="80px">
                                    </px:PXCheckBox>                                    
                                    <px:PXTextEdit ID="edType" runat="server" DataField="Type">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" />
                                    <px:PXGridColumn DataField="LineRef" />
                                    <px:PXGridColumn DataField="EmployeeID"
                                        AllowShowHide="False" CommitChanges="True" DisplayMode="Hint">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PrimaryDriver" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="ServiceLineRef" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointmentServiceEmployee__InventoryID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointmentServiceEmployee__TranDesc">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TrackTime" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EarningType" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LaborItemID" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="DfltProjectID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DfltProjectTaskID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="IsDriver" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Type">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" />
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdOpenStaffSelectorFromStaffTab">
                                    <AutoCallBack Target="ds" Command="OpenStaffSelectorFromStaffTab" ></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdStartStaff">
                                    <AutoCallBack Target="ds" Command="StartStaff" ></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdCompleteStaff">
                                    <AutoCallBack Target="ds" Command="CompleteStaff" ></AutoCallBack>
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
            <px:PXTabItem Text="Log">
                <Template>
                    <px:PXGrid ID="PXGridLog" runat="server" DataSourceID="ds" SkinID="DetailsInTab" TabIndex="2150" Height="100%" Width="100%" SyncPosition="True" MarkRequired="Dynamic">
                        <Levels>
                            <px:PXGridLevel DataMember="LogRecords" DataKeyNames="DocID,LineNbr">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edLogLineRef" runat="server" DataField="LineRef"/>
                                    <px:PXSegmentMask ID="edLogBAccountID" runat="server" DataField="BAccountID"/>
                                    <px:PXDropDown ID="edLogStatus" runat="server" DataField="Status"/>
                                    <px:PXCheckBox ID="edLogTravel" runat="server" CommitChanges="True" DataField="Travel" AllowEdit="True"/>
                                    <px:PXSelector ID="edLogDetLineRef" runat="server" DataField="DetLineRef" AutoRefresh="True"/>
                                    <px:PXSegmentMask ID="edLogInventoryID" runat="server" DataField="FSAppointmentDet__InventoryID"/>
                                    <px:PXTextEdit ID="edLogDescr" runat="server" DataField="Descr"/>
                                    <px:PXDateTimeEdit ID="edLogDateTimeBegin_Time" runat="server" CommitChanges="True" DataField="DateTimeBegin_Time" TimeMode="True"/>
                                    <px:PXDateTimeEdit ID="edLogDateTimeEnd_Time" runat="server" CommitChanges="True" DataField="DateTimeEnd_Time" TimeMode="True"/>
                                    <px:PXMaskEdit ID="edLogTimeDuration" runat="server" DataField="TimeDuration" CommitChanges="True"/>
                                    <px:PXCheckBox ID="edLogTrackOnService" runat="server" CommitChanges="True" DataField="TrackOnService"/>
                                    <px:PXCheckBox ID="edLogTrackTime" runat="server" CommitChanges="True" DataField="TrackTime" AllowEdit="True"/>
                                    <px:PXCheckBox ID="edIsBillable" runat="server" CommitChanges="True" DataField="IsBillable" AllowEdit="True"/>
                                    <px:PXMaskEdit ID="edBillableTimeDuration" runat="server" DataField="BillableTimeDuration" CommitChanges="True"/>
                                    <px:PXNumberEdit ID="edCuryBillableTranAmount" runat="server" DataField="CuryBillableTranAmount" />
                                    <px:PXSelector ID="edLogProjectTaskID" runat="server" DataField="ProjectTaskID" AllowEdit="True" AutoRefresh="True" CommitChanges="True"/>
                                    <px:PXSegmentMask ID="edLogCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSelector ID="edLogEarningType" runat="server" DataField="EarningType" AutoRefresh="True" CommitChanges="True"/>
                                    <px:PXSegmentMask ID="edLogLaborItemID" runat="server" DataField="LaborItemID" AllowEdit="True" CommitChanges="True" AutoRefresh="True"/>
                                    <px:PXSelector ID="edLogTimeCardCD" runat="server" DataField="TimeCardCD" AllowEdit ="True"/>
                                    <px:PXCheckBox ID="edLogApprovedTime" runat="server" DataField="ApprovedTime" />
                                    <px:PXCheckBox ID="edLogKeepDateTimes" runat="server" DataField="KeepDateTimes" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" />
                                    <px:PXGridColumn DataField="DocRefNbr" />
                                    <px:PXGridColumn DataField="LineRef" />
                                    <px:PXGridColumn DataField="BAccountID" CommitChanges="True" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Travel" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="DetLineRef" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="FSAppointmentDet__InventoryID" />
                                    <px:PXGridColumn DataField="Descr" />
                                    <px:PXGridColumn DataField="DateTimeBegin_Time" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="DateTimeEnd_Time" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="TimeDuration" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="TrackOnService" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="TrackTime" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="BillableTimeDuration" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryBillableTranAmount" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="EarningType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LaborItemID" />
                                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeID" />
                                    <px:PXGridColumn DataField="TimeCardCD" />
                                    <px:PXGridColumn DataField="ApprovedTime" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="KeepDateTimes" TextAlign="Center" Type="CheckBox"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" AllowUpload="True"/>
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
                      <px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="AppointmentSelected.TaxCalcMode" CommitChanges="true"/>
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
                                    <px:PXGridColumn DataField="LineRef" />
                                    <px:PXGridColumn DataField="LineType" />
                                    <px:PXGridColumn DataField="ItemID" />
                                    <px:PXGridColumn DataField="Descr" />
                                    <px:PXGridColumn DataField="EmployeeID" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="UnitPrice" />
                                    <px:PXGridColumn DataField="EstimatedQty" />
                                    <px:PXGridColumn DataField="EstimatedAmount" />
                                    <px:PXGridColumn DataField="ActualDuration" />
                                    <px:PXGridColumn DataField="ActualQty" />
                                    <px:PXGridColumn DataField="ActualAmount" />
                                    <px:PXGridColumn DataField="BillableQty" />
                                    <px:PXGridColumn DataField="BillableAmount" />
                                    <px:PXGridColumn DataField="UnitCost" />
                                    <px:PXGridColumn DataField="CostTotal" />
                                    <px:PXGridColumn DataField="Profit" />
                                    <px:PXGridColumn DataField="ProfitPercent" />
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
              <px:PXFormView ID="formG" runat="server" DataSourceID="ds" DataMember="AppointmentSelected" RenderStyle="Simple">
                    <Template>
              <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Appointment Totals" />
              <px:PXNumberEdit ID="edCuryEstimatedLineTotal" runat="server" DataField="CuryEstimatedLineTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edCuryBillableLineTotal" runat="server" DataField="CuryBillableLineTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edCuryLogBillableTranAmountTotal" runat="server" DataField="CuryLogBillableTranAmountTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edGridCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edGridCuryDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edAppCompletedBillableTotal" runat="server" DataField="AppCompletedBillableTotal" Enabled="False" Size="Empty" />
                    </Template>
              </px:PXFormView>
              <px:PXFormView ID="ServiceOrderTotalsTab" runat="server" DataMember="ServiceOrderRelated" RenderStyle="Simple">
            <Template>
                        <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Service Order Totals" />
              <px:PXNumberEdit ID="edSOCuryDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" Size="Empty" />
              <px:PXNumberEdit ID="edSOCuryCompletedBillableTotal" runat="server" DataField="SOCuryCompletedBillableTotal" Enabled="False" Size="Empty" />
            </Template>
          </px:PXFormView>  
          <px:PXLayoutRule runat="server" StartColumn="True"/>
          <px:PXFormView ID="PrepaymentTotalsTab" runat="server" DataMember="ServiceOrderRelated" RenderStyle="Simple">
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
      </px:PXTabItem>
      <px:PXTabItem Text="Other Information" BindingContext="mainForm" LoadOnDemand="True" RepaintOnDemand="False" >
            <Template>
                <px:PXPanel ID="PXPanel5" runat="server" SkinID="transparent">
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Source Info" />
                    <px:PXFormView ID="ServiceOrderSourceInfo" runat="server" DataMember="ServiceOrderRelated" RenderStyle="Simple" >
              <Template>
                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXDropDown ID="edSourceType" runat="server" DataField="SourceType" Enabled="False">
                            </px:PXDropDown>
                            <px:PXTextEdit ID="edSourceReferenceNbr" runat="server" DataField="SourceReferenceNbr" Enabled="False">
                                <LinkCommand Target="ds" Command="OpenSourceDocument"></LinkCommand>
                            </px:PXTextEdit>
              </Template>
            </px:PXFormView> 
                    <px:PXFormView ID="edSourceInfoForm" runat="server" DataMember="AppointmentSelected" RenderStyle="Simple">
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
                    <px:PXFormView ID="edInvoiceInfoForm" runat="server" DataSourceID="ds" DataMember="AppointmentPostedIn" Caption="Billing Info" RenderStyle="Fieldset">
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
                            <px:PXTextEdit ID="edINPostDocReferenceNbr" runat="server" DataField="INPostDocReferenceNbr" Enabled="False">
                                <LinkCommand Target="ds" Command="OpenINPostingDocument"></LinkCommand>
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="edSignatureForm" runat="server" DataMember="AppointmentSelected" Caption="Signature" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM"/>
                            <px:PXTextEdit ID="edFullNameSignature" runat="server" DataField="FullNameSignature">
                            </px:PXTextEdit>
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