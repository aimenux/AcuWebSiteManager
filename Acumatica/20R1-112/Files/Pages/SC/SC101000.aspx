<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="SC101000.aspx.cs" Inherits="Page_SC101000" Title="Subcontracts Preferences" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Setup"
        TypeName="PX.Objects.CN.Subcontracts.SC.Graphs.SubcontractSetupMaint" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="487px" Style="z-index: 100"
        Width="100%" DataMember="Setup" Caption="General Settings"
        DefaultControlID="edSubcontractNumberingID">
        <Autosize Container="Window" Enabled="True" MinHeight="200" />
        <Activity Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edSubcontractNumberingID" runat="server" AllowNull="False"
                        DataField="SubcontractNumberingID" AllowEdit="True" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkRequireSubcontractControlTotal" runat="server"
                        DataField="RequireSubcontractControlTotal" />
                    <px:PXCheckBox ID="edSubcontractRequestApproval" runat="server" DataField="SubcontractRequestApproval" />
                    <px:PXFormView ID="edSetupApproval" runat="server" Caption="Approval" DataMember="SetupApproval"
                        RenderStyle="Fieldset" TabIndex="1100">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID"
                                TextField="Name" AllowEdit="True" />
                            <px:PXSelector ID="edAssignmentNotificationID" runat="server"
                                DataField="AssignmentNotificationID" AllowEdit="True" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailing Settings">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal"
                        Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds"
                                Height="150px" Caption="Default Sources"
                                       AdjustPageSize="Auto" AllowPaging="True">
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="Notifications"
                                        DataKeyNames="Module,SourceCD,NotificationCD">
                                        <RowTemplate>
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server"
                                                DataField="NotificationCD" />
                                            <px:PXSelector ID="edNotificationID" runat="server"
                                                DataField="NotificationID" ValueField="Name" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False"
                                                DataField="Format" SelectedIndex="3" />
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID"
                                                ValueField="ScreenID" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server"
                                                DataField="EMailAccountID" DisplayMode="Text" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="NotificationCD" />
                                            <px:PXGridColumn DataField="EMailAccountID"
                                                DisplayMode="Text" />
                                            <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC"
                                                Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID"
                                                AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Format"
                                                RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center"
                                                Type="CheckBox" Width="60px" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
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
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Attributes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSelector ID="edCRAttributeID" runat="server" DataField="AttributeID" AutoRefresh="true" AllowEdit="true" FilterByAllFields="True" />
                                    <px:PXTextEdit ID="edDescription" runat="server"  DataField="Description" />
                                    <px:PXCheckBox ID="chkRequired" runat="server" DataField="Required" />
                                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
                                    <px:PXDropDown ID="edControlType" runat="server"  DataField="ControlType" />
                                    <px:PXDropDown ID="edType" runat="server"  DataField="Type" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" AutoCallBack="True" />
                                    <px:PXGridColumn  DataField="Description" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                                    <px:PXGridColumn  DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn  DataField="ControlType" Type="DropDownList" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
