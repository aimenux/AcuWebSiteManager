<%@ Page Title="Customization/Event Handlers" Language="C#"
    MasterPageFile="~/MasterPages/ListView.master"
    AutoEventWireup="true"
    ValidateRequest="false"
    CodeFile="AU201070.aspx.cs"
    Inherits="Pages_AU_AU201070" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXFormView runat="server" SkinID="transparent" ID="formTitle"
        DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
        <Template>
            <px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
                SkinID="Label" SuppressLabel="true"
                Width="90%"
                Style="padding: 10px">
                <font size="14pt" names="Arial,sans-serif;" />
            </px:PXTextEdit>
        </Template>
    </px:PXFormView>
    <pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUWorkflowEventHandlersMaint"
        PrimaryView="ViewHandlers">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="handlerNew" RepaintControls="All"  />
            <px:PXDSCallbackCommand CommitChanges="True" Name="actionViewChanges" Visible="True" RepaintControls="All" DependOnGrid="gridHandlers" StateColumn="ViewChangesAvailable"/>
        </CallbackCommands>
    </pxa:AUDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">

    <px:PXGrid ID="gridHandlers" runat="server" DataSourceID="ds"
        Width="100%" Height="400px"
        AdjustPageSize="Auto"
        BorderWidth="0px"
        AllowSearch="True"
        SkinID="Primary"
        SyncPositionWithGraph="True"
        SyncPosition="True"
        AllowPaging="false"
        AutoAdjustColumns="True">
        <Mode AllowFormEdit="False" InitNewRow="False" AllowAddNew="False" AllowRowSelect="True"  />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <Actions>
                <AdjustColumns ToolBarVisible="false" />
                <ExportExcel ToolBarVisible="false" />
                <AddNew ToolBarVisible="False" />
            </Actions>
        </ActionBar>
        <Levels>
            <px:PXGridLevel DataMember="ViewHandlers" SortOrder="Order">
                <Columns>
                    <px:PXGridColumn DataField="HandlerName" Width="100" LinkCommand="actionEdit" />
                    <px:PXGridColumn DataField="DisplayName" Width="100" />
                    <px:PXGridColumn DataField="EventContainerName" Width="100" />
                    <px:PXGridColumn DataField="EventName" Width="100" />
                    <px:PXGridColumn DataField="Status" Width="100" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>

<asp:Content runat="server" ID="phDialogs" ContentPlaceHolderID="phDialogs">
    <px:PXSmartPanel runat="server" ID="PanelEditHandler" Key="FilterHandlerEdit"
        Caption="Event Handler Properties"
        CaptionVisible="True"
		AutoRepaint="true"
        AcceptButtonID="PXButtonOK"
        CancelButtonID="PXButtonCancel"
        ShowCloseButton="False"
        Width="900"
        Height="600px">
        <px:PXFormView runat="server" DataSourceID="ds" ID="FormHandlerEdit" DataMember="FilterHandlerEdit" Width="100%" SkinID="Transparent" AutoRepaint="True">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXTextEdit ID="HandlerName" runat="server" DataField="HandlerName" CommitChanges="True" />
                <%--<px:PXDropDown ID="GraphHandlerName" runat="server" DataField="GraphHandlerName" CommitChanges="True" />--%>
                <px:PXTextEdit ID="DisplayName" runat="server" DataField="DisplayName" />
                <px:PXDropDown ID="EventContainerName" runat="server" DataField="EventContainerName" CommitChanges="True"  />
                <px:PXDropDown ID="EventName" runat="server" DataField="EventName" CommitChanges="True" />
               
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" GroupCaption="Entity to apply workflow" />
                <px:PXCheckBox runat="server" ID="echbUseTargetAsPrimarySource" DataField="UseTargetAsPrimarySource" CommitChanges="True" AlignLeft="True"></px:PXCheckBox>
                <px:PXCheckBox runat="server" ID="echbUseParameterAsPrimarySource" DataField="UseParameterAsPrimarySource" CommitChanges="True" AlignLeft="True"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" Merge="True"  />
                <px:PXCheckBox runat="server" ID="echbuseViewAsPrimarySource" DataField="useViewAsPrimarySource" CommitChanges="True" AlignLeft="True"></px:PXCheckBox>
                <%--<px:PXDropDown ID="PXDropDown1" runat="server" DataField="SelectView" SuppressLabel="True" />
                <px:PXLayoutRule runat="server" />--%>
                <px:PXCheckBox runat="server" ID="PXCheckBox1" DataField="AllowMultipleSelect" CommitChanges="True"></px:PXCheckBox>
            </Template>
        </px:PXFormView>
        <px:PXTab runat="server" ID="tabhandlerDetails" Width="100%" >
            <AutoSize Enabled="True" MinHeight="150" />
            <Items>
                <px:PXTabItem Text="Field Update" RepaintOnDemand="False" >
                    <Template>
                         <px:PXGrid runat="server" ID="gridStatehandlersFields" CaptionVisible="False" AutoAdjustColumns="True" Width="100%" AllowPaging="False"
                            MatrixMode="True" SyncPosition="True" SkinID="Details"  OnEditorsCreated="grd_EditorsCreated_RelativeDates" >
                            <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="True">
                            </ActionBar>
                            <Levels>
                                <px:PXGridLevel DataMember="HandlerFieldsPerHandler">
                                    <Mode InitNewRow="True" />
                                    <RowTemplate>
                                        <pxa:PXFormulaCombo ID="edSAFValue" runat="server" DataField="Value" EditButton="True"
                                            FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields and Parameters"
                                            IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                            SkinID="GI" />
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn DataField="IsActive" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                        <px:PXGridColumn DataField="FieldName" Width="200px" CommitChanges="True" />
                                        <px:PXGridColumn DataField="IsFromScheme" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                        <px:PXGridColumn DataField="Value" Width="200px" CommitChanges="True" Key="value" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value" />
                                        <px:PXGridColumn DataField="Status" Width="50px" />
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                            <CallbackCommands>
                                <Refresh CommitChanges="True" PostData="Page" RepaintControls="OwnerContent" />
                                <InitRow CommitChanges="true" />
                            </CallbackCommands>
                    </px:PXGrid>
                    </Template>
                   
                </px:PXTabItem>
            </Items>
        </px:PXTab>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel">
                <AutoCallBack Enabled="True" Target="ds" Command="ActionClosePanel" />
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelViewChanges" runat="server" Caption="Changes" CaptionVisible="True"
        Width="800px" Height="600px" Key="Changes" AutoRepaint="True">
        <px:PXFormView runat="server" ID="FormPanelViewChanges" DataMember="Changes" DataSourceID="ds" Height="100%"
            Width="100%">
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXHtmlView runat="server" ID="edMessages" DataField="Messages" TextMode="MultiLine" Height="100%" Width="100%"
                    Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px;">
                    <AutoSize Enabled="True" MinHeight="350" Container="Parent" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="FormPanelViewChangesButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="FormPanelViewChangesButtonRevert" runat="server"  CommandName="actionReset" CommandSourceID="ds"  >
                
            </px:PXButton>
            <px:PXButton ID="FormPanelViewChangesButtonCancel" runat="server" DialogResult="Cancel" Text="Close">
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>

    <%--AutoRepaint="True"--%>

    <px:PXSmartPanel ID="ConfirmRevert" runat="server" Caption="Revert Changes" CaptionVisible="True"
        Width="600px" Height="300px" Key="ConfirmRevert" AutoRepaint="True" ClientEvents-AfterHide="OnConfirmRevertClosed">
        <px:PXFormView runat="server" ID="PXFormViewConfirmRevert1" DataMember="ConfirmRevert" DataSourceID="ds" Height="100%"
            Width="100%" AutoRepaint="True">
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXHtmlView runat="server" ID="PXHtmlViewConfirmRevert1" DataField="Message" TextMode="MultiLine" Height="100%" Width="100%"
                    Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px;">
                    <AutoSize Enabled="True" Container="Parent" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelConfirmRevert2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonConfirmRevert1" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="PXButtonConfirmRevert2" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
        <script type="text/javascript">
            function OnConfirmRevertClosed() {
                if (px_alls['ConfirmRevert'].dialogResult === PXDialogResult.OK) {
                    px_alls['PanelViewChanges'].hide();
                }
            }
        </script>
</asp:Content>
