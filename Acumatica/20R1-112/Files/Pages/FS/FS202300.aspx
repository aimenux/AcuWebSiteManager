<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS202300.aspx.cs" Inherits="Page_FS202300" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="SvrOrdTypeRecords" TypeName="PX.Objects.FS.SvrOrdTypeMaint">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
                    <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
                    <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
                    <px:PXDSCallbackCommand Name="Last" PostData="Self" />
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="SvrOrdTypeRecords"
            TabIndex="900" DefaultControlID="edSrvOrdType" FilesIndicator="true">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" ControlSize="XM" LabelsWidth="SM" Merge="True">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edSrvOrdType" Size="XS" runat="server" AutoRefresh="True" DataField="SrvOrdType" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXCheckBox ID="edActive" runat="server" DataField="Active" Text="Active">
                    </px:PXCheckBox>
                    <px:PXLayoutRule runat="server" Merge="False">
                    </px:PXLayoutRule>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
                    </px:PXTextEdit>
                    <px:PXLayoutRule runat="server" ControlSize="SM" StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXCheckBox ID="chkShowQuickProcessTab" runat="server" DataField="ShowQuickProcessTab" Visible="False" Enabled="False"/>
                </Template>
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
            <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="CurrentSrvOrdTypeRecord" MarkRequired="Dynamic">
                <Items>
                    <px:PXTabItem Text="Preferences">
                        <Template>
                            <px:PXLayoutRule 
                                runat="server" 
                                StartColumn="True" 
                                GroupCaption="General Settings" 
                                LabelsWidth="M"
                                StartGroup="True"
                                ControlSize="XM">
                            </px:PXLayoutRule>
                            <%-- General Settings Fields--%>
                            <px:PXSelector ID="edSrvOrdNumberingID" runat="server" DataField="SrvOrdNumberingID" DataSourceID="ds" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXDropDown ID="edBehavior" runat="server" CommitChanges="True" DataField="Behavior">
                            </px:PXDropDown>
                            <px:PXCheckBox ID="edCompleteSrvOrdWhenSrvDone" runat="server" DataField="CompleteSrvOrdWhenSrvDone" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCloseSrvOrdWhenSrvDone" runat="server" DataField="CloseSrvOrdWhenSrvDone" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edRequireContact" runat="server" DataField="RequireContact" CommitChanges="True" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edRequireRoom" runat="server" DataField="RequireRoom" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edRequireCustomerSignature" runat="server" DataField="RequireCustomerSignature" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyNotesFromCustomer" runat="server" DataField="CopyNotesFromCustomer" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyAttachmentsFromCustomer" runat="server" DataField="CopyAttachmentsFromCustomer" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyNotesFromCustomerLocation" runat="server" DataField="CopyNotesFromCustomerLocation" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyAttachmentsFromCustomerLocation" runat="server" DataField="CopyAttachmentsFromCustomerLocation" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyNotesToAppoinment" runat="server" DataField="CopyNotesToAppoinment" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyAttachmentsToAppoinment" runat="server" DataField="CopyAttachmentsToAppoinment" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyLineNotesToInvoice" runat="server" DataField="CopyLineNotesToInvoice" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCopyLineAttachmentsToInvoice" runat="server" DataField="CopyLineAttachmentsToInvoice" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edOnTravelCompleteStartAppt" runat="server" DataField="OnTravelCompleteStartAppt" Size="XM" />
                            <%-- Default Settings Fields--%>
                            <px:PXLayoutRule runat="server" GroupCaption="Default Settings" ControlSize="XM" LabelsWidth="M" StartGroup="True">
                            </px:PXLayoutRule>
                            <px:PXDropDown ID="edAppAddressSource" runat="server" CommitChanges="True" DataField="AppAddressSource">
                            </px:PXDropDown>
                            <px:PXSegmentMask ID="edDfltCostCodeID" runat="server" DataField="DfltCostCodeID" AutoRefresh="true" AllowEdit="True"/>
                            <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" CommitChanges="True" AutoRefresh="True"></px:PXSegmentMask>
                            <px:PXCheckBox ID="edCommissionable" runat="server" CommitChanges="True" DataField="Commissionable"></px:PXCheckBox>
                            <px:PXSegmentMask ID="edDfltBillableTravelItem" runat="server" DataField="DfltBillableTravelItem" AllowEdit="True" CommitChanges="True" AutoRefresh="True" />
                            <%-- /Appointment Settings Fields--%>
                            <px:PXLayoutRule 
                                runat="server" 
                                StartColumn="True" 
                                GroupCaption="Billing Settings"
                                LabelsWidth="M"
                                ControlSize="XM">
                            </px:PXLayoutRule>
                            <%-- Posting Settings Fields--%>
                            <px:PXDropDown ID="edPostTo" runat="server" CommitChanges="True" DataField="PostTo">
                            </px:PXDropDown>
                            <px:PXCheckBox ID="edPostNegBalanceToAp" runat="server" DataField="PostNegBalanceToAp" CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edEnableINPosting" runat="server" DataField="EnableINPosting" CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowQuickProcess" runat="server" DataField="AllowQuickProcess" CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXSelector ID="edPostOrderType" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="PostOrderType" DataSourceID="ds">
                            </px:PXSelector>
                            <px:PXSelector ID="edPostOrderTypeNegativeBalance" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="PostOrderTypeNegativeBalance" DataSourceID="ds">
                            </px:PXSelector>
                            <px:PXSelector ID="edAllocationOrderType" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="AllocationOrderType" DataSourceID="ds">
                            </px:PXSelector>
                            <px:PXSelector ID="edDfltTermIDARSO" runat="server" AllowEdit="True" AutoRefresh="True" DataField="DfltTermIDARSO" DataSourceID="ds">
                            </px:PXSelector>
                            <px:PXSelector ID="edDfltTermIDAP" runat="server" AllowEdit="True" AutoRefresh="True" DataField="DfltTermIDAP" DataSourceID="ds">
                            </px:PXSelector>
                            <px:PXDropDown ID="edSalesAcctSource" runat="server" CommitChanges="True" DataField="SalesAcctSource">
                            </px:PXDropDown>
                            <px:PXSegmentMask ID="edCombineSubFrom" runat="server" CommitChanges="True" DataField="CombineSubFrom">
                            </px:PXSegmentMask>
                            <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID">
                            </px:PXSegmentMask>
                            <px:PXSegmentMask ID="edAccountGroupID" runat="server" DataField="AccountGroupID" CommitChanges="True" AllowEdit="True"></px:PXSegmentMask>
                            <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode" CommitChanges="True" AllowEdit="True"></px:PXSelector>
                            <px:PXDropDown ID="edBillingType" runat="server" CommitChanges="True" DataField="BillingType">
                            </px:PXDropDown>
                            <px:PXCheckBox ID="edReleaseProjectTransactionOnInvoice" runat="server" DataField="ReleaseProjectTransactionOnInvoice" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edReleaseIssueOnInvoice" runat="server" DataField="ReleaseIssueOnInvoice" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowInvoiceOnlyClosedAppointment" runat="server" DataField="AllowInvoiceOnlyClosedAppointment" Size="XM">
                            </px:PXCheckBox>
                            <px:PXLayoutRule runat="server" GroupCaption="Integrating with Time & Expenses" LabelsWidth="M" ControlSize="XM" StartGroup="True">
                            </px:PXLayoutRule>
                            <px:PXCheckBox ID="edRequireTimeApprovalToInvoice" runat="server" CommitChanges="True" DataField="RequireTimeApprovalToInvoice" Size="XM">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edCreateTimeActivitiesFromAppointment" runat="server" CommitChanges="True" DataField="CreateTimeActivitiesFromAppointment" Size="XM">
                            </px:PXCheckBox>
                            <px:PXSelector ID="edDfltEarningType" runat="server" AutoRefresh="True" CommitChanges="True" DataField="DfltEarningType" DataSourceID="ds">
                            </px:PXSelector>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Time Behavior">
                        <Template>
                            <px:PXLayoutRule 
                                runat="server" 
                                StartColumn="True" 
                                GroupCaption="Appointment Starting Settings" 
                                LabelsWidth="M"
                                StartGroup="True"
                                ControlSize="SM" />
                            <px:PXCheckBox ID="edOnStartApptSetStartTimeInHeader" runat="server" DataField="OnStartApptSetStartTimeInHeader" CommitChanges="True" AlignLeft="True" />
                            <px:PXCheckBox ID="edOnStartApptSetNotStartItemInProcess" runat="server" DataField="OnStartApptSetNotStartItemInProcess" CommitChanges="True" AlignLeft="True" />
                            <px:PXCheckBox ID="edOnStartApptStartUnassignedStaff" runat="server" DataField="OnStartApptStartUnassignedStaff" CommitChanges="True" AlignLeft="True" />
                            <px:PXCheckBox ID="edOnStartApptStartServiceAndStaff" runat="server" DataField="OnStartApptStartServiceAndStaff" CommitChanges="True" AlignLeft="True" />

                            <px:PXLayoutRule 
                                runat="server" 
                                GroupCaption="Appointment Completion Settings" 
                                LabelsWidth="M"
                                StartGroup="True"
                                ControlSize="SM" />
                            <px:PXCheckBox ID="edOnCompleteApptSetEndTimeInHeader" runat="server" DataField="OnCompleteApptSetEndTimeInHeader" CommitChanges="True" AlignLeft="True" />
                            <px:PXDropDown ID="edOnCompleteApptSetInProcessItemsAs" runat="server" DataField="OnCompleteApptSetInProcessItemsAs" />
                            <px:PXDropDown ID="edOnCompleteApptSetNotStartedItemsAs" runat="server" DataField="OnCompleteApptSetNotStartedItemsAs" />
                            <px:PXLayoutRule 
                                runat="server" 
                                GroupCaption="Other Settings" 
                                LabelsWidth="M"
                                StartGroup="True"
                                StartColumn="True"
                                ControlSize="XM"
                                ColumnWidth="400px"/>
                            <px:PXCheckBox ID="edOnStartTimeChangeUpdateLogStartTime" runat="server" DataField="OnStartTimeChangeUpdateLogStartTime" AlignLeft="True" CommitChanges="True" />
                            <px:PXCheckBox ID="edOnEndTimeChangeUpdateLogEndTime" runat="server" DataField="OnEndTimeChangeUpdateLogEndTime" AlignLeft="True" CommitChanges="True" />
                            <px:PXCheckBox ID="edAllowManualLogTimeEdition" runat="server" DataField="AllowManualLogTimeEdition" AlignLeft="True" />
                            <px:PXCheckBox ID="edSetTimeInHeaderBasedOnLog" runat="server" DataField="SetTimeInHeaderBasedOnLog" AlignLeft="True" CommitChanges="True"/>
                            <px:PXCheckBox ID="edOnCompleteApptRequireLog" runat="server" DataField="OnCompleteApptRequireLog" AlignLeft="True" />
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Quick Process Settings" VisibleExp="DataControls[&quot;chkShowQuickProcessTab&quot;].Value == 1" BindingContext="form" RepaintOnDemand="False">
                        <Template>
                            <px:PXPanel runat="server" ID="pnlQuickProcessSettings" SkinID="Transparent" DataMember="QuickProcessSettings">
                                <px:PXLayoutRule StartGroup="True" GroupCaption="Appointment Actions" runat="server">
                                </px:PXLayoutRule>
                                <px:PXCheckBox ID="edCloseAppointment" runat="server" AlignLeft="True" DataField="CloseAppointment" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXCheckBox ID="edEmailSignedAppointment" runat="server" AlignLeft="True" DataField="EmailSignedAppointment" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXCheckBox ID="edGenerateInvoiceFromAppointment" runat="server" AlignLeft="True" DataField="GenerateInvoiceFromAppointment" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXLayoutRule StartGroup="True" GroupCaption="Service Order Actions" runat="server">
                                </px:PXLayoutRule>
                                <px:PXCheckBox ID="edAllowInvoiceServiceOrder" runat="server" AlignLeft="True" DataField="AllowInvoiceServiceOrder" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXCheckBox ID="edCompleteServiceOrder" runat="server" AlignLeft="True" DataField="CompleteServiceOrder" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXCheckBox ID="edCloseServiceOrder" runat="server" AlignLeft="True" DataField="CloseServiceOrder" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXCheckBox ID="edGenerateInvoiceFromServiceOrder" runat="server" AlignLeft="True" DataField="GenerateInvoiceFromServiceOrder" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXLayoutRule StartGroup="True" GroupCaption="Sales Order Actions" runat="server">
                                </px:PXLayoutRule>
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
                                <px:PXLayoutRule StartGroup="True" GroupCaption="Bill Actions" runat="server">
                                </px:PXLayoutRule>
                                <px:PXCheckBox ID="edReleaseBill" runat="server" AlignLeft="True" DataField="ReleaseBill" CommitChanges="True">
                                </px:PXCheckBox>
                                <px:PXCheckBox ID="edPayBill" runat="server" AlignLeft="True" DataField="PayBill" CommitChanges="True">
                                </px:PXCheckBox>
                            </px:PXPanel>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Problem Codes">
                        <Template>
                            <px:PXGrid ID="PXGridProblems" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AllowPaging="True" AdjustPageSize="Auto"
                            Height="200px" TabIndex="11300" FilesIndicator="False" NoteIndicator="False">
                                <Levels>
                                    <px:PXGridLevel DataMember="SrvOrdTypeProblemRecords" DataKeyNames="SrvOrdType,ProblemID">
                                        <RowTemplate>
                                            <px:PXSelector ID="edProblemID" runat="server" DataField="ProblemID" AutoRefresh="True">
                                            </px:PXSelector>
                                            <px:PXTextEdit ID="edFSProblem__Descr" runat="server" DataField="FSProblem__Descr" Enabled="False">
                                            </px:PXTextEdit>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="ProblemID" CommitChanges="True">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="FSProblem__Descr">
                                            </px:PXGridColumn>
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Attributes">
                    <Template>
                        <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
                            border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
                            <Levels>
                                <px:PXGridLevel DataMember="Mapping">
                                    <RowTemplate>
                                        <px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn DataField="AttributeID" AutoCallBack="true" LinkCommand="CRAttribute_ViewDetails" />
                                        <px:PXGridColumn AllowNull="False" DataField="Description" />
                                        <px:PXGridColumn DataField="SortOrder" TextAlign="Right" SortDirection="Ascending" />
                                        <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                        <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                            <AutoSize Enabled="True" MinHeight="150" />
                        </px:PXGrid>
                    </Template>
                </px:PXTabItem>
                    <px:PXTabItem Text="Mailing Settings">
                        <Template>
                            <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px" SavePosition="True">
                                <AutoSize Enabled="True"></AutoSize>
                                <Template1>
                                    <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px" Caption="Mailings" AdjustPageSize="Auto"
                                    AllowPaging="True" DataSourceID="ds">
                                        <AutoCallBack Target="gridNR" Command="Refresh"></AutoCallBack>
                                        <AutoSize Enabled="True"></AutoSize>
                                        <Levels>
                                            <px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
                                                <RowTemplate>
                                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"></px:PXLayoutRule>
                                                    <px:PXDropDown ID="edFormat" runat="server" DataField="Format"></px:PXDropDown>
                                                    <px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID"></px:PXSegmentMask>
                                                    <px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active"></px:PXCheckBox>
                                                    <px:PXSelector ID="edSetupID" runat="server" DataField="SetupID"></px:PXSelector>
                                                    <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID"></px:PXSelector>
                                                    <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name"></px:PXSelector>
                                                    <px:PXSelector Size="s" ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text"></px:PXSelector>

                                                </RowTemplate>
                                                <Columns>
                                                    <px:PXGridColumn DataField="SetupID" AutoCallBack="True"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="ReportID" AutoCallBack="True"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="NotificationID" AutoCallBack="True"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>

                                                </Columns>
                                                <Layout FormViewHeight=""></Layout>
                                            </px:PXGridLevel>
                                        </Levels>
                                    </px:PXGrid>
                                </Template1>
                                <Template2>
                                    <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients" AdjustPageSize="Auto" AllowPaging="True"
                                    DataSourceID="ds">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="gridNS"></px:PXSyncGridParam>
                                        </Parameters>
                                        <CallbackCommands>
                                            <Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds"></Save>
                                            <FetchRow RepaintControls="None"></FetchRow>
                                        </CallbackCommands>
                                        <Levels>
                                            <px:PXGridLevel DataMember="NotificationRecipients" DataKeyNames="NotificationID">
                                                <Columns>
                                                    <px:PXGridColumn DataField="ContactType" AutoCallBack="True">
                                                    </px:PXGridColumn>
                                                    <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False"></px:PXGridColumn>
                                                    <px:PXGridColumn DataField="ContactID">
                                                        <NavigateParams>
                                                            <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]"></px:PXControlParam>
                                                        </NavigateParams>
                                                    </px:PXGridColumn>
                                                    <px:PXGridColumn DataField="Format" AutoCallBack="True">
                                                    </px:PXGridColumn>
                                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox">
                                                    </px:PXGridColumn>
                                                    <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox">
                                                    </px:PXGridColumn>
                                                </Columns>
                                                <RowTemplate>
                                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"></px:PXLayoutRule>
                                                    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName" AllowEdit="True">
                                                    </px:PXSelector>
                                                </RowTemplate>
                                                <Layout FormViewHeight=""></Layout>
                                            </px:PXGridLevel>
                                        </Levels>
                                        <AutoSize Enabled="True"></AutoSize>
                                    </px:PXGrid>
                                </Template2>
                            </px:PXSplitContainer>
                        </Template>
                    </px:PXTabItem>
                </Items>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
            </px:PXTab>
        </asp:Content>