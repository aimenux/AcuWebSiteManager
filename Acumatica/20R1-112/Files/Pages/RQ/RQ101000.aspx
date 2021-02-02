<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="RQ101000.aspx.cs" Inherits="Page_PO101000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Setup"
        TypeName="PX.Objects.RQ.RQSetupMaint" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="form" runat="server" DataSourceID="ds" Height="505px" Style="z-index: 100"
        Width="100%" DataMember="Setup" Caption="General Settings">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />

<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Request Settings" />

                            <px:PXSelector ID="edRequestNumberingID" runat="server" AllowNull="False" DataField="RequestNumberingID" Text="RQITEM" AllowEdit="True" />
                            <px:PXSelector ID="edRequestAssigmentMapID" runat="server" DataField="RequestAssignmentMapID" TextField="Name" AllowEdit="True" />
                            <px:PXNumberEdit ID="edMonthRetainRequest" runat="server" DataField="MonthRetainRequest" />
                            <px:PXCheckBox ID="chkRequestApproval" runat="server" Checked="True" DataField="RequestApproval" />                                                                
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Requisition Settings" />
                            <px:PXSelector ID="edRequisitionNumberingID" runat="server" AllowNull="False" DataField="RequisitionNumberingID" Text="RQRequisition" AllowEdit="True" />
                            <px:PXSelector ID="edRequisitionAssignmentMapID" runat="server" DataField="RequisitionAssignmentMapID" TextField="Name" AllowEdit="true" />
                            <px:PXNumberEdit ID="edMonthRetainRequisition" runat="server" DataField="MonthRetainRequisition" />
                            <px:PXCheckBox ID="chkPOHold" runat="server" Checked="True" DataField="POHold" />
                            <px:PXCheckBox ID="chkRequisitionApproval" runat="server" Checked="True" DataField="RequisitionApproval" />                            
                            <px:PXCheckBox ID="chkRequisitionMergeLines" runat="server" Checked="True" DataField="RequisitionMergeLines" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Other Settings" />

                            <px:PXSelector ID="edBudgetLedgerId" runat="server" DataField="BudgetLedgerId" AllowEdit="True" />
                            <px:PXDropDown ID="edBudgetCalculation" runat="server" AllowNull="False" DataField="BudgetCalculation"  />
                            <px:PXSelector ID="edDefaultReqClassID" runat="server" DataField="DefaultReqClassID" Text="DEFAULT" AllowEdit="True" /></Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" Style="position: absolute" Width="100%"
                        DataSourceID="ds" BorderWidth="0px" SkinID="Details">
                        <AutoSize Enabled="true" />
                        <Levels>
                            <px:PXGridLevel  DataMember="SetupApproval">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

                                    <px:PXDropDown ID="edType" runat="server" DataField="Type"  />
                                    <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" AutoRefresh="true" TextField="Name" AllowEdit="True">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="gridApproval" Name="SyncGrid" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSelector ID="edAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AllowEdit="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Type" RenderEditorText="True" Width="100px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="AssignmentMapID" Width="108px" TextField="AssignmentMapID_EPAssignmentMap_Name" />
                                    <px:PXGridColumn DataField="AssignmentNotificationID" Width="250px" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
      <px:PXTabItem Text="Reporting Settings">
          <Template>
              <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
                  <AutoSize Enabled="true" />
                  <Template1>
                      <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
                          AdjustPageSize="Auto" AllowPaging="True">
                          <AutoCallBack Target="gridNR" Command="Refresh" />
                          <Levels>
                              <px:PXGridLevel DataMember="Notifications">
                                  <RowTemplate>
                                      <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                      <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                      <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                      <px:PXSelector ID="edNBranchID" runat="server" DataField="NBranchID" />
                                      <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                      <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                      <px:PXSelector ID="edDefPrinterID" runat="server" DataField="DefaultPrinterID" />
                                      <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                      <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                      <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                  </RowTemplate>
                                  <Columns>
                                      <px:PXGridColumn DataField="NotificationCD" Width="120px" />
                                      <px:PXGridColumn DataField="NBranchID" Width="120px" />
                                      <px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
                                      <px:PXGridColumn DataField="DefaultPrinterID" Width="120px" />
                                      <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" Width="150px" AutoCallBack="true" />
                                      <px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="true" />
                                      <px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="true" />
                                      <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                  </Columns>
                              </px:PXGridLevel>
                          </Levels>
                          <AutoSize Enabled="true" />
                      </px:PXGrid>
                  </Template1>
                  <Template2>
                      <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" Caption="Default Recipients" AdjustPageSize="Auto"
                          AllowPaging="True" Style="left: 0px; top: 0px">
                          <Parameters>
                              <px:PXSyncGridParam ControlID="gridNS" />
                          </Parameters>
                          <CallbackCommands>
                              <Save CommitChanges="true" CommitChangesIDs="gridNR" RepaintControls="None" />
                              <FetchRow RepaintControls="None" />
                          </CallbackCommands>
                          <Levels>
                              <px:PXGridLevel DataMember="Recipients">
                                  <Columns>
                                      <px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="true" />
                                      <px:PXGridColumn DataField="OriginalContactID" Visible="false" AllowShowHide="false" />
                                      <px:PXGridColumn DataField="ContactID" Width="120px">
                                          <NavigateParams>
                                              <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                          </NavigateParams>
                                      </px:PXGridColumn>
                                      <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="true" />
                                      <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                      <px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
                                  </Columns>
                                  <RowTemplate>
                                      <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                      <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="true" ValueField="DisplayName" AllowEdit="True">
                                          <Parameters>
                                              <px:PXSyncGridParam ControlID="gridNR" />
                                          </Parameters>
                                      </px:PXSelector>
                                  </RowTemplate>
                              </px:PXGridLevel>
                          </Levels>
                          <AutoSize Enabled="true" MinHeight="150" />
                      </px:PXGrid>
                  </Template2>
              </px:PXSplitContainer>
          </Template>
	  </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
