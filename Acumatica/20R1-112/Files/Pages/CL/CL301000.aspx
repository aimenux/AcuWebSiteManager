<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="CL301000.aspx.cs" Inherits="Page_CL301000" Title="Compliance Preferences" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="LienWaiverSetup" BorderStyle="NotSet"
        TypeName="PX.Objects.CN.Compliance.CL.Graphs.ComplianceDocumentSetupMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="341px" DataMember="LienWaiverSetup">
        <Items>
            <px:PXTabItem Text="Lien Waiver Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ColumnWidth="XXL" LabelsWidth="XXL" ControlSize="XXL" />
                    <px:PXLayoutRule runat="server" GroupCaption="Outstanding Lien Waivers" StartGroup="True" />
                    <px:PXCheckBox CommitChanges="True" ID="chkShouldWarnOnBillEntry" runat="server" DataField="ShouldWarnOnBillEntry" />
                    <px:PXCheckBox CommitChanges="True" ID="chkShouldWarnOnPayment" runat="server" DataField="ShouldWarnOnPayment" />
                    <px:PXCheckBox CommitChanges="True" ID="chkShouldStopPayments" runat="server" DataField="ShouldStopPayments" />
                    <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" />
                    <px:PXLayoutRule ColumnWidth="XL" LabelsWidth="M" ControlSize="M" GroupCaption="Conditional Lien Waivers"
                                     runat="server" StartGroup="True" />
                    <px:PXCheckBox AlignLeft="True" runat="server" ID="chkShouldGenerateConditional" DataField="ShouldGenerateConditional" 
                                   CommitChanges="True" />
                    <px:PXDropDown runat="server" ID="edGenerationEventConditional" DataField="GenerationEventConditional" />
                    <px:PXDropDown runat="server" ID="edThroughDateSourceConditional" DataField="ThroughDateSourceConditional" />
                    <px:PXDropDown runat="server" ID="edFinalAmountSourceConditional" DataField="FinalAmountSourceConditional" />
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXLayoutRule ControlSize="M" LabelsWidth="M" ColumnWidth="XL" runat="server" StartGroup="True"
                                     GroupCaption="Unconditional Lien Waivers" />
                    <px:PXCheckBox AlignLeft="True" runat="server" ID="chkShouldGenerateUnconditional" DataField="ShouldGenerateUnconditional" 
                                   CommitChanges="True" />
                    <px:PXDropDown runat="server" ID="edGenerationEventUnconditional" DataField="GenerationEventUnconditional" />
                    <px:PXDropDown runat="server" ID="edThroughDateSourceUnconditional" DataField="ThroughDateSourceUnconditional" />
                    <px:PXDropDown runat="server" ID="edFinalAmountSourceUnconditional" DataField="FinalAmountSourceUnconditional" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Lien Waiver Reporting Settings">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
                                AdjustPageSize="Auto" AllowPaging="True">
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="ComplianceNotifications" DataKeyNames="Module,SourceCD,NotificationCD,NBranchID">
                                        <RowTemplate>
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXSelector ID="edNBranchID" runat="server" DataField="NBranchID" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edDefPrinter" runat="server" DataField="DefaultPrinterID" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="NotificationCD" />
                                            <px:PXGridColumn DataField="NBranchID" />
                                            <px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="DefaultPrinterID" />
                                            <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" DataSourceID="ds" Width="100%"
                                Caption="Default Recipients" AdjustPageSize="Auto" AllowPaging="True"
                                Style="left: 0px; top: 0px">
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
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True"
                                                Width="100px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False"
                                                AllowShowHide="False" />
                                            <px:PXGridColumn DataField="ContactID">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR"
                                                        PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True"
                                                AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"
                                                Width="60px" />
                                            <px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center"
                                                Type="CheckBox" Width="60px" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M"
                                                ControlSize="XM" />
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID"
                                                AutoRefresh="True" ValueField="DisplayName" AllowEdit="True">
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
            <px:PXTabItem Text="Custom Attributes">
                <Template>
                    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
                        DataMember="Filter" Caption="Attribute Group" TabIndex="900">
                        <ContentStyle BorderStyle="None" />
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                            <px:PXSelector ID="edType" runat="server" DataField="Type" CommitChanges="True">
                            </px:PXSelector>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="gridCustom" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                        Width="100%" ActionsPosition="Top" SkinID="Details" Caption="Attributes" TabIndex="1100"
                        AllowPaging="True" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="Mapping" DataKeyNames="AttributeID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM"
                                        ControlSize="XM" />
                                    <px:PXTextEdit ID="edValue" runat="server" DataField="Value" Width="200px"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Value" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Common Attributes">
                <Template>
                    <px:PXGrid ID="gridCommon" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                        Width="100%" ActionsPosition="Top" SkinID="Details" Caption="Attributes" TabIndex="1100"
                        AllowPaging="True" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="MappingCommon">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSelector ID="edAttributeID" runat="server" DataField="AttributeID" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description"
                                        AlreadyLocalized="False" DefaultLocale="" />
                                    <px:PXCheckBox ID="edRequired" runat="server" AlreadyLocalized="False"
                                        DataField="Required" Text="Required" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
