<%@ Page Title="Dialog Boxes" Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" CodeFile="AU201040.aspx.cs" Inherits="Pages_AU_AU201040" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
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
    <pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUWorkflowFormsMaint"
        PrimaryView="ViewPageTitle" PageLoadBehavior="SearchSavedKeys">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="AddForm" RepaintControls="All" Visible="False"  />
            <px:PXDSCallbackCommand CommitChanges="True" Name="moveUp" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="moveDown" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="RefreshGrid" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewAction" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="RefreshFields" Visible="False" RepaintControlsIDs="grid" />
        </CallbackCommands>
    </pxa:AUDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXSplitContainer runat="server" ID="splitConditions" SplitterPosition="250" Style="border-top: solid 1px #BBBBBB;">
        <Template1>
            <px:PXGrid ID="GridForms" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" 
                       SyncPosition="True" KeepPosition="True" Width="100%" AdjustPageSize="Auto" SkinID="Details"  
                       AllowPaging="false" BorderWidth="0px" AllowSearch="True"  Caption="Dialog Boxes" CaptionVisible="true" AutoAdjustColumns="True">
                <Mode AutoInsert="False" AllowAddNew="False"></Mode>
                <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
                <ActionBar>
                    <Actions>
                        <AdjustColumns ToolBarVisible="false" />
                        <ExportExcel ToolBarVisible="false" />
                        <AddNew ToolBarVisible="False" ></AddNew>
                        
                    </Actions>
                    <CustomItems>
                        <px:PXToolBarButton>
                            <AutoCallBack Command="AddForm" Target="ds" />
                            <Images Normal="main@RecordAdd"></Images>
                            <ActionBar GroupIndex="0" Order="3"/>
                        </px:PXToolBarButton>
                       
                    </CustomItems>
                </ActionBar>
                <AutoCallBack Enabled="True" Command="ViewAction" Target="ds">
                   
                </AutoCallBack>
                <Levels>
                    <px:PXGridLevel DataMember="ViewForms">
                        <Columns>
                            <px:PXGridColumn DataField="FormName" Width="150" CommitChanges="True" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" MinHeight="150" />
            </px:PXGrid>
            </Template1>
        <Template2>
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%"  AutoRepaint="True"
                DataMember="ViewForm" >
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" CommitChanges="True" />
                    <px:PXTextEdit ID="edFormName" runat="server" DataField="FormName" Enabled="False" />
                    <px:PXTextEdit ID="Status" runat="server" DataField="Status" Enabled="False" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXNumberEdit ID="edColumns" runat="server" DataField="Columns" />
                    <px:PXTextEdit ID="edWorkflows" runat="server" DataField="Actions" Enabled="False" />
                   
                </Template>
            </px:PXFormView>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px"
                Width="100%" AdjustPageSize="Auto" SkinID="Details" Style="z-index: 100"
                Caption="Dialog Box Fields" CaptionVisible="True" SyncPosition="True" KeepPosition="True"
                AllowPaging="false" AutoAdjustColumns="true" AutoGenerateColumns="None" OnEditorsCreated="grd_EditorsCreated_RelativeDates">
                <AutoCallBack Enabled="True" Command="RefreshFields" Target="ds" SuppressOnReload="True" FromUIOnly="True" >
                </AutoCallBack>
                <ActionBar>
                    <Actions>
                        <AdjustColumns ToolBarVisible="false" />
                        <ExportExcel ToolBarVisible="false" />
                    </Actions>
                    <CustomItems>
                        <px:PXToolBarButton>
                            <AutoCallBack Command="moveUp" Target="ds" />
                            <Images Normal="main@ArrowUp" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton>
                            <AutoCallBack Command="moveDown" Target="ds" />
                            <Images Normal="main@ArrowDown" />                           
                        </px:PXToolBarButton>
                        <px:PXToolBarButton>
                            <AutoCallBack Command="comboBoxValues" Target="ds" />
                        </px:PXToolBarButton>
                    </CustomItems>
                </ActionBar>
                <Levels>
                    <px:PXGridLevel DataMember="ViewFieldsPerForm">
                        <Mode InitNewRow="True" />
                        <RowTemplate>
                            <pxa:PXFormulaCombo ID="edSAFValue" runat="server" DataField="Value" EditButton="True"
                                                FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields" 
                                                IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                                SkinID="GI"/>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn Type="CheckBox" DataField="IsActive" Width="60px"  />
                            <px:PXGridColumn DataField="SchemaField" CommitChanges="True" Width="200" />
                            <px:PXGridColumn DataField="FieldName"  CommitChanges="True" Width="200" />
                            <px:PXGridColumn DataField="DisplayName" Width="200" />
                            <px:PXGridColumn Type="CheckBox" DataField="FromScheme" CommitChanges="True" Width="60px"  />
                            <px:PXGridColumn DataField="DefaultValue" Width="200px" CommitChanges="True" Key="value" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value" />
                            <px:PXGridColumn Type="CheckBox" DataField="Required" Width="60px" />
                            <px:PXGridColumn DataField="ColumnSpan" Width="200" />
                            <px:PXGridColumn DataField="ControlSize" CommitChanges="True" Width="200" MatrixMode="true" AllowStrings="True" />
                            <px:PXGridColumn DataField="Status" Width="100" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" MinHeight="150" Container="Window" />
                <CallbackCommands>
                    <InitRow CommitChanges="true" />
                </CallbackCommands>
            </px:PXGrid>
        </Template2>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXSplitContainer>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phDialogs" runat="Server">
    <px:PXSmartPanel runat="server" ID="PanelDynamicForm" Key="FilterPreview"
        Caption="Preview Dialog Box" CaptionVisible="True"
        LoadOnDemand="True"
        AutoReload="True"
        AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel"
        AutoCallBack-Enabled="true" AutoCallBack-Target="FormPreview" AutoCallBack-Command="Refresh"
        ShowAfterLoad="True"
        Width="400">
        <px:PXFormView runat="server" ID="FormPreview" DataMember="FilterPreview" Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" />
                <%--                <px:PXTextEdit runat="server" ID="Subject" DataField="Subject"></px:PXTextEdit>--%>
            </Template>

        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>

    </px:PXSmartPanel>
    <px:PXSmartPanel runat="server" ID="PanelFilterNewForm" Key="FilterNewForm"
        Caption="New Dialog Box" CaptionVisible="True" AutoReload="True" AutoRepaint="True"
        AcceptButtonID="PXButtonOK2" CancelButtonID="PXButtonCancel2"
        Width="400">
        <px:PXFormView runat="server" ID="FormFilterNewForm" DataMember="FilterNewForm" Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" />
                <px:PXTextEdit runat="server" ID="FormName" DataField="FormName" />
            </Template>

        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK2" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>

    </px:PXSmartPanel>

    <%--                      AutoCallBack-Enabled="true"
                     AutoCallBack-Target="FormFilterNewForm"
                     AutoCallBack-Command="Refresh"
                     CallBackMode-CommitChanges="True"
                     CallBackMode-PostData="Page" --%>



    <px:PXSmartPanel ID="pnlCombos" runat="server" Style="z-index: 108; left: 351px; position: absolute; top: 99px"
        Width="550px" Caption="Combo Box Values" CaptionVisible="true" LoadOnDemand="true" Key="Combos"
        AutoCallBack-Enabled="true" AutoCallBack-Target="gridCombos" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <div style="padding: 5px">
            <px:PXGrid ID="gridCombos" runat="server" DataSourceID="ds" Style="z-index: 100" SkinID="Details" 
                Width="100%">
                <AutoSize Enabled="True" MinHeight="243"></AutoSize>
                <ActionBar>
                    <Actions>
                        <Refresh ToolBarVisible="False"></Refresh>
                        <ExportExcel ToolBarVisible="False"></ExportExcel>
                        <AdjustColumns ToolBarVisible="False"></AdjustColumns>
                    </Actions>
                </ActionBar>
                <Levels>
                    <px:PXGridLevel DataMember="Combos">
                        <Columns>
                            <px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
                                Width="60px" />
                            <px:PXGridColumn AllowNull="False" DataField="IsExplicit" TextAlign="Center" Type="CheckBox"
                                Width="60px" />
                            <px:PXGridColumn DataField="Value" />
                            <px:PXGridColumn DataField="Description" Width="200px" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <CallbackCommands>
                    <FetchRow RepaintControls="None" />
                    <Refresh CommitChanges="True" RepaintControls="All" />
                    <InitRow CommitChanges="true" />
                </CallbackCommands>
            </px:PXGrid>
        </div>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" CausesValidation="False" Text="OK" DialogResult="OK" />
            <px:PXButton ID="PXButton6" runat="server" CausesValidation="False" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    
    
    <px:PXSmartPanel ID="PanelViewChanges" runat="server" Caption="Changes" CaptionVisible="True" 
                     Width="800px" Height="600px" Key="Changes" AutoRepaint="True" >
        <px:PXFormView runat="server" ID="FormPanelViewChanges" DataMember="Changes" DataSourceID="ds" Height="100%"
                       Width="100%" AutoRepaint="True" >
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXHtmlView runat="server" ID="edMessages" DataField="Messages" TextMode="MultiLine"  Height="100%" Width="100%"
                               Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                    <AutoSize Enabled="True" MinHeight="350" Container="Parent" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="FormPanelViewChangesButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="FormPanelViewChangesButtonRevert" runat="server" Text="Revert all changes">
                <AutoCallBack Target="ds" Command="actionReset"/>
            </px:PXButton>
            <px:PXButton ID="FormPanelViewChangesButtonCancel" runat="server" DialogResult="Cancel" Text="Close">
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>
    
    <px:PXSmartPanel ID="ConfirmRevert" runat="server" Caption="Revert Changes" CaptionVisible="True" 
                     Width="600px" Height="300px" Key="ConfirmRevert" AutoRepaint="True" ClientEvents-AfterHide="OnConfirmRevertClosed" >
        <px:PXFormView runat="server" ID="PXFormView1" DataMember="ConfirmRevert" DataSourceID="ds" Height="100%"
                       Width="100%" AutoRepaint="True" >
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXHtmlView runat="server" ID="PXHtmlView1" DataField="Message" TextMode="MultiLine"  Height="100%" Width="100%"
                               Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                    <AutoSize Enabled="True" Container="Parent" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton3" runat="server" Text="OK" DialogResult="OK"/>
            <px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Close"/>
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