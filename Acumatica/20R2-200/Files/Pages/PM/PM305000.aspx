<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM305000.aspx.cs"
    Inherits="Page_PM305000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:pxdatasource id="ds" runat="server" visible="True" width="100%" typename="PX.Objects.CN.ProjectAccounting.CostProjectionEntry"
        primaryview="Document" borderstyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AppendSelectedCostBudget" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="AddCostBudget" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="ShowHistory" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="ViewCostCommitments" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="ViewCostTransactions" Visible="false" CommitChanges="true"/>
        </CallbackCommands>
    </px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:pxformview id="form" runat="server" datasourceid="ds" style="z-index: 100" width="100%" datamember="Document" caption="Selection"
        emailinggraph="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" AllowEdit="true"/>
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" Merge="true" LabelsWidth="SM" ControlSize="S" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="true" />
            <px:PXLayoutRule runat="server" Merge="false" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXSelector ID="edClassID" runat="server" DataField="ClassID" CommitChanges="True" AllowEdit="true" AllowAddNew="true"/>
            <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="true"/>
            
            <px:PXLayoutRule ID="PXLayoutRulea1" runat="server" GroupCaption="Budgeted" StartColumn="True" StartGroup="True" LabelsWidth="M" ControlSize="XS" />
            <px:PXNumberEdit ID="PXNumberEdit4" runat="server" DataField="TotalAmountToComplete" Enabled="False" />
            <px:PXNumberEdit ID="PXNumberEdit3" runat="server" DataField="TotalBudgetedAmount" Enabled="False" />
             
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Projected" StartColumn="True" StartGroup="True" />
            <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="TotalAmount" Enabled="False" SuppressLabel="True" Size="S"/>
            <px:PXNumberEdit ID="PXNumberEdit6" runat="server" DataField="TotalProjectedAmount" Enabled="False" SuppressLabel="True" Size="S"/>
        </Template>
    </px:pxformview>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:pxtab id="tab" runat="server" width="100%" height="511px" datasourceid="ds" datamember="DocumentSettings">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:pxgrid id="grid" runat="server" datasourceid="ds" style="z-index: 100" width="100%" height="150px" skinid="Details" syncposition="true" actionsposition="Top">
        <Levels>
            <px:PXGridLevel DataMember="Details">
                 <RowTemplate>
                     <px:PXSegmentMask ID="edTaskIDCost" runat="server" DataField="TaskID" AutoRefresh="true"/>
                     <px:PXSegmentMask ID="edCostCodeCost" runat="server" DataField="CostCodeID" AllowAddNew="true" AutoRefresh="true" />
                     <px:PXSegmentMask ID="edInventory" runat="server" DataField="InventoryID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="AccountGroupID"/>
                    <px:PXGridColumn DataField="TaskID"/>
                    <px:PXGridColumn DataField="CostCodeID"/>
                    <px:PXGridColumn DataField="InventoryID"/>
                    <px:PXGridColumn DataField="Description"/>
                    <px:PXGridColumn DataField="UOM"/>
                    <px:PXGridColumn DataField="BudgetedQuantity" TextAlign="Right"/>
                    <px:PXGridColumn DataField="BudgetedAmount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ActualQuantity" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ActualAmount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="UnbilledQuantity" TextAlign="Right"/>
                    <px:PXGridColumn DataField="UnbilledAmount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="CompletedQuantity" TextAlign="Right"/>
                    <px:PXGridColumn DataField="CompletedAmount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="QuantityToComplete" TextAlign="Right"/>
                    <px:PXGridColumn DataField="AmountToComplete" TextAlign="Right"/>
                    <px:PXGridColumn DataField="Quantity" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="ProjectedQuantity" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="ProjectedAmount" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="Mode" CommitChanges="true"/>
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
                <px:PXToolBarButton Key="cmdViewCostCommitments">
                    <AutoCallBack Command="ViewCostCommitments" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="View Transactions" Key="cmdViewCostTransactions">
                    <AutoCallBack Command="ViewCostTransactions" Target="ds" />
                    <PopupCommand Command="Refresh" Target="grid" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="History" Tooltip="Compare Revisions">
                    <AutoCallBack Command="ShowHistory" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="false" InitNewRow="true" AllowUpload="true"/>
    </px:pxgrid>
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
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:pxtab>
</asp:Content>
<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
    <px:pxsmartpanel id="AddCostBudgetPanel" runat="server" height="396px" width="850px" caption="Select Budget Lines" captionvisible="True" key="AvailableCostBudget" autocallback-command="Refresh"
        autocallback-enabled="True" autocallback-target="AvailableCostBudgetGrid" loadondemand="true" autorepaint="true">
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
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <AddNew MenuVisible="False" ToolBarVisible="False"/>
                    <Delete MenuVisible="False" ToolBarVisible="Top"/>
                    <NoteShow MenuVisible="False" ToolBarVisible="False"/>
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
        </px:PXGrid>
         <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonAdd" runat="server" Text="Add Lines" CommandName="AppendSelectedCostBudget"  CommandSourceID="ds"/>
            <px:PXButton ID="PXButtonAddClose" runat="server" Text="Add Lines & Close" DialogResult="OK"/>
            <px:PXButton ID="PXButtonClose" runat="server" DialogResult="Cancel" Text="Close"/>
        </px:PXPanel>
    </px:pxsmartpanel>

    <px:pxsmartpanel id="HistoryPanel" runat="server" height="396px" width="900px" caption="History" captionvisible="True" key="History" autocallback-command="Refresh"
        autocallback-enabled="True" autocallback-target="HistoryGrid" loadondemand="true" autorepaint="true">
        <px:PXGrid ID="HistoryGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="History">
                    <Columns>
                        <px:PXGridColumn DataField="PMCostProjection__RevisionID" />
                        <px:PXGridColumn DataField="PMCostProjection__Date" />
                        <px:PXGridColumn DataField="BudgetedQuantity" TextAlign="Right"/>
                        <px:PXGridColumn DataField="BudgetedAmount" TextAlign="Right"/>
                        <px:PXGridColumn DataField="ActualQuantity" TextAlign="Right"/>
                        <px:PXGridColumn DataField="ActualAmount" TextAlign="Right"/>
                        <px:PXGridColumn DataField="UnbilledQuantity" TextAlign="Right"/>
                        <px:PXGridColumn DataField="UnbilledAmount" TextAlign="Right"/>
                        <px:PXGridColumn DataField="CompletedQuantity" TextAlign="Right"/>
                        <px:PXGridColumn DataField="CompletedAmount" TextAlign="Right"/>
                        <px:PXGridColumn DataField="QuantityToComplete" TextAlign="Right"/>
                        <px:PXGridColumn DataField="AmountToComplete" TextAlign="Right"/>
                        <px:PXGridColumn DataField="Quantity" TextAlign="Right" CommitChanges="true"/>
                        <px:PXGridColumn DataField="Amount" TextAlign="Right" CommitChanges="true"/>
                        <px:PXGridColumn DataField="ProjectedQuantity" TextAlign="Right" CommitChanges="true"/>
                        <px:PXGridColumn DataField="ProjectedAmount" TextAlign="Right" CommitChanges="true"/>
                        <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" CommitChanges="true"/>
                        <px:PXGridColumn DataField="Mode" CommitChanges="true"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
    </px:pxsmartpanel>

    <px:pxsmartpanel id="PanelCopyRevision" runat="server" style="z-index: 108; position: absolute; left: 27px; top: 99px;" caption="Copy Revision"
        captionvisible="True" loadondemand="true" showafterload="true" key="CopyDialog" autocallback-enabled="true" autocallback-target="formNewRevision" autocallback-command="Refresh"
        callbackmode-commitchanges="True" callbackmode-postdata="Page" acceptbuttonid="PXButtonOK" cancelbuttonid="PXButtonCancel">
        <px:PXFormView ID="formNewRevision" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="New Revision" CaptionVisible="False" SkinID="Transparent"
            DataMember="CopyDialog">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit CommitChanges="True" ID="PXMaskEdit1" runat="server" DataField="RevisionID" />
                <px:PXCheckBox ID="chkRefreshBudget" runat="server" DataField="RefreshBudget" CommitChanges="True"/>
                <px:PXCheckBox ID="chkCopyNotes" runat="server" DataField="CopyNotes" CommitChanges="True"/>
                <px:PXCheckBox ID="chkCopyFiles" runat="server" DataField="CopyFiles" CommitChanges="True"/>
           </Template>
        </px:PXFormView>
        <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButtonOK" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>
    </px:pxsmartpanel>

    <px:pxsmartpanel id="panelReason" runat="server" caption="Enter Reason" captionvisible="true" loadondemand="true" key="ReasonApproveRejectParams"
        autocallback-enabled="true" autocallback-command="Refresh" callbackmode-commitchanges="True" width="600px"
        callbackmode-postdata="Page" acceptbuttonid="btnReasonOk" cancelbuttonid="btnReasonCancel" allowresize="False">
        <px:PXFormView ID="PXFormViewPanelReason" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="ReasonApproveRejectParams">
            <ContentStyle BackColor="Transparent" BorderStyle="None" Width="100%" Height="100%"  CssClass=""/>
            <Template>
                <px:PXLayoutRule ID="PXLayoutRulePanelReason" runat="server" StartColumn="True"/>
                <px:PXPanel ID="PXPanelReason" runat="server" RenderStyle="Simple">
                    <px:PXLayoutRule ID="PXLayoutRuleReason" runat="server" StartColumn="True" SuppressLabel="True"/>
                    <px:PXTextEdit ID="edReason" runat="server" DataField="Reason" TextMode="MultiLine" LabelWidth="0" Height="200px" Width="600px" CommitChanges="True"/>
                </px:PXPanel>
                <px:PXPanel ID="PXPanelReasonButtons" runat="server" SkinID="Buttons">
                    <px:PXButton ID="btnReasonOk" runat="server" Text="OK" DialogResult="Yes" CommandSourceID="ds"/>
                    <px:PXButton ID="btnReasonCancel" runat="server" Text="Cancel" DialogResult="No" CommandSourceID="ds"/>
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:pxsmartpanel>
</asp:Content>
