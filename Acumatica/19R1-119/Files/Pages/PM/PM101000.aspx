<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM101000.aspx.cs"
    Inherits="Page_PM101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.SetupMaint" PrimaryView="Setup">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="500px" Style="z-index: 100" Width="100%" DataMember="Setup">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" GroupCaption="Numbering Sequence" StartGroup="True" StartColumn="True" LabelsWidth="L" ControlSize="L"/>
                    <px:PXSelector ID="edTranNumbering" runat="server" DataField="TranNumbering" Text="PMTRAN" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edBatchNumberingID" runat="server" DataField="BatchNumberingID" Text="BATCH" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edProformaNumbering" runat="server" DataField="ProformaNumbering" Text="PROFORMA" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edChangeOrderNumbering" runat="server" DataField="ChangeOrderNumbering" Text="CHANGEORD" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edQuoteNumberingID" runat="server" DataField="QuoteNumberingID" AllowEdit="True" Text="PMQUOTE" />

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="General Settings" />
                    <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
                    <px:PXTextEdit ID="edNonProjectCode" runat="server" DataField="NonProjectCode" Text="X" />
                    <px:PXTextEdit ID="edEmptyItemCode" runat="server" DataField="EmptyItemCode" Text="&tgN/A&lt" />
                    <px:PXSelector ID="edEmptyItemUOM" runat="server" DataField="EmptyItemUOM" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edDefaultChangeOrderClassID" runat="server" DataField="DefaultChangeOrderClassID" Text="DEFAULT" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edQuoteTemplateID" runat="server" DataField="QuoteTemplateID" AllowEdit="True" DataSourceID="ds" />
                                       
                    <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edProformaApprovalMapID" runat="server" DataField="ProformaApprovalMapID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edProformaApprovalNotificationID" runat="server" DataField="ProformaApprovalNotificationID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edProformaAssignmentMapID" runat="server" DataField="ProformaAssignmentMapID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edProformaAssignmentNotificationID" runat="server" DataField="ProformaAssignmentNotificationID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edChangeOrderApprovalMapID" runat="server" DataField="ChangeOrderApprovalMapID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edChangeOrderApprovalNotificationID" runat="server" DataField="ChangeOrderApprovalNotificationID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edQuoteApprovalMapID" runat="server" DataField="QuoteApprovalMapID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edQuoteApprovalNotificationID" runat="server" DataField="QuoteApprovalNotificationID" TextField="Name" AllowEdit="True"/>
                    <px:PXDropDown ID="edCutOffdate" runat="server" DataField="CutoffDate" />
                    <px:PXDropDown ID="edOverLimitErrorLevel" runat="server" DataField="OverLimitErrorLevel" />
                    <px:PXDropDown ID="edCostBudgetUpdateMode" runat="server" DataField="CostBudgetUpdateMode" />
                    <px:PXCheckBox ID="chkAutoPost" runat="server" DataField="AutoPost" />
                    <px:PXCheckBox ID="chkAutoReleaseAllocation" runat="server" DataField="AutoReleaseAllocation" />
                    <px:PXCheckBox ID="chkCostCommitmentTracking" runat="server" DataField="CostCommitmentTracking" />
                    <px:PXCheckBox ID="chkRevenueCommitmentTracking" runat="server" DataField="RevenueCommitmentTracking" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" StartColumn="True" LabelsWidth="L" ControlSize="L" GroupCaption="Visibility Settings" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInGL" runat="server" DataField="VisibleInGL" />
                    <px:PXCheckBox ID="chkVisibleInAP" runat="server" DataField="VisibleInAP" />
                    <px:PXCheckBox ID="chkVisibleInAR" runat="server" DataField="VisibleInAR" />
                    <px:PXCheckBox ID="chkVisibleInSO" runat="server" DataField="VisibleInSO" />
                    <px:PXCheckBox ID="chkVisibleInPO" runat="server" DataField="VisibleInPO" />
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInIN" runat="server" DataField="VisibleInIN" />
                    <px:PXCheckBox ID="chkVisibleInCA" runat="server" DataField="VisibleInCA" />
                    <px:PXCheckBox ID="chkVisibleInCR" runat="server" DataField="VisibleInCR" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInTA" runat="server" DataField="VisibleInTA" />
                    <px:PXCheckBox ID="chkVisibleInEA" runat="server" DataField="VisibleInEA" />
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                    <px:PXDropDown ID="edRestrictProjectSelect" runat="server" DataField="RestrictProjectSelect" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Account Settings" />
                    <px:PXDropDown ID="edExpenseAccountSource" runat="server" DataField="ExpenseAccountSource" SelectedIndex="-1" />
                    <px:PXSegmentMask ID="edExpenseSubMask" runat="server" DataField="ExpenseSubMask" DataMember="_PMSETUP_Segments_"
                        DataSourceID="ds" />
                    <px:PXDropDown ID="edExpenseAccrualAccountSource" runat="server" DataField="ExpenseAccrualAccountSource" SelectedIndex="-1" />
                    <px:PXSegmentMask ID="edExpenseAccrualSubMask" runat="server" DataField="ExpenseAccrualSubMask" DataMember="_PMSETUP_Segments_"
                        DataSourceID="ds" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailing Settings">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350"
                        SkinID="Horizontal" Height="500px" SavePosition="True">
                        <AutoSize Enabled="True" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
                                AdjustPageSize="Auto" AllowPaging="True">
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="Notifications" DataKeyNames="Module,NotificationCD">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="NotificationCD" Width="120px" />
                                            <px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="Details" DataSourceID="ds" Width="100%" Caption="Default Recipients" CaptionVisible="true">
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="gridNS" />
                                </Parameters>
                                <CallbackCommands>
                                    <Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="Recipients" DataKeyNames="RecipientID">
                                        <Columns>
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
                                            <px:PXGridColumn DataField="ContactID" Width="120px">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                            <px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName" AllowEdit="True">
                                                <Parameters>
                                                    <px:PXSyncGridParam ControlID="gridNR" />
                                                </Parameters>
                                            </px:PXSelector>
                                        </RowTemplate>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize MinHeight="480" Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>
