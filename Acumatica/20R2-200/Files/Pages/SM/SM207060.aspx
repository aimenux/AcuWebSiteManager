<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM207060.aspx.cs" Inherits="Pages_SM_SM207060" Title="Web Service Endpoints" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Endpoint"
        TypeName="PX.Api.ContractBased.UI.EntityConfigurationMaint" PageLoadBehavior="GoFirstRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PopulateFields" Visible="False" />
            <px:PXDSCallbackCommand Name="ExtendEntity" Visible="False" />
            <px:PXDSCallbackCommand Name="PopulateParameters" Visible="False" />
            <px:PXDSCallbackCommand Name="SelectAll" Visible="False" />
            <px:PXDSCallbackCommand Name="ClearAll" Visible="False" />
            <px:PXDSCallbackCommand Name="EnablePopulate" Visible="False" />
            <px:PXDSCallbackCommand Name="validateEntity" Visible="False" />
            <px:PXDSCallbackCommand Name="ValidateAction" Visible="False" />

            <px:PXDSCallbackCommand Name="ViewEndpointService"/>
            <px:PXDSCallbackCommand Name="ViewMaintenanceService" />
            <px:PXDSCallbackCommand Name="ExtendEndpoint" />
            <px:PXDSCallbackCommand Name="ValidateEndpoint" />

            <px:PXDSCallbackCommand Name="InsertNew" Visible="False" HideText="True" />
            <px:PXDSCallbackCommand Name="DeleteNode" Visible="False" HideText="True" />
            <px:PXDSCallbackCommand Name="Delete" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="EntityTree" TreeKeys="Key" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Endpoint" TabIndex="100">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector runat="server" DataField="InterfaceName" ID="edInterfaceName" AutoRefresh="True" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector runat="server" DataField="GateVersion" ID="edGateVersion" AutoRefresh="True" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer ID="splitFields" runat="server" SplitterPosition="250">
        <AutoSize Enabled="True" Container="Window" />
        <Template1>
            <px:PXTreeView ID="entityTree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" ExpandDepth="1" ShowRootNode="False"
                DataMember="EntityTree" AllowCollapse="False" SelectFirstNode="True" SyncPosition="True" AllowDelete="true" AllowEdit="False" AllowAddNew="True">
                <ToolBarItems>
                    <px:PXToolBarButton CommandSourceID="ds" CommandName="InsertNew" ImageKey="AddNew" ImageSet="main" />
                    <px:PXToolBarButton CommandSourceID="ds" CommandName="DeleteNode" ImageKey="Remove" ImageSet="main" />
                </ToolBarItems>
                <AutoCallBack Target="ds" Command="EnablePopulate" Enabled="True" ActiveBehavior="True">
                    <Behavior RepaintControls="None" RepaintControlsIDs="entityPropertiesForm,actionPropertiesForm,fieldsGrid,actionsGrid,tab"></Behavior>
                </AutoCallBack>
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="EntityTree" TextField="Title" ValueField="Key" ImageUrlField="Icon" />
                </DataBindings>
                <AutoSize Enabled="True" Container="Parent" />
            </px:PXTreeView>
        </Template1>
        <Template2>
            <px:PXTab ID="tab" runat="server" TabIndex="200" Height="100%" Width="100%">
                <AutoSize Enabled="True" MinHeight="250" MinWidth="100" Container="Parent" />
                <Items>
                    <px:PXTabItem Text="Endpoint Properties" RepaintOnDemand="False">
                        <Template>
                            <px:PXFormView ID="endpointPropertiesForm" runat="server" DataSourceID="ds" Width="100%" DataMember="SelectedEndpoint" SkinID="Transparent">
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXTextEdit runat="server" DataField="InterfaceName" ID="edName" Enabled="False" AutoRefresh="True" />
                                    <px:PXTextEdit runat="server" DataField="GateVersion" ID="edVersion" Enabled="False" AutoRefresh="True" />
                                    <px:PXTextEdit runat="server" DataField="SystemContractVersion" ID="edSystemContractVersion" Enabled="False" AutoRefresh="True" />
                                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXTextEdit runat="server" DataField="ExtendsName" ID="edExtendsName" Enabled="False" AutoRefresh="True" />
                                    <px:PXTextEdit runat="server" DataField="ExtendsVersion" ID="edExtendsVersion" Enabled="False" AutoRefresh="True" />
                                </Template>
                                <Parameters>
                                    <px:PXControlParam ControlID="entityTree" Name="Key" PropertyName="SelectedValue" />
                                </Parameters>
                            </px:PXFormView>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Entity Properties" RepaintOnDemand="False">
                        <Template>
                            <px:PXFormView ID="entityPropertiesForm" runat="server" DataSourceID="ds" Width="100%" DataMember="SelectedEntity" SkinID="Transparent">
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXTextEdit runat="server" DataField="ObjectName" ID="edObjectName" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXDropDown runat="server" DataField="ObjectType" ID="edObjectType" CommitChanges="True" />
                                    <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID" TextField="Title" ValueField="ScreenID" DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
                                    <px:PXTextEdit runat="server" DataField="ScreenIDValue" ID="edScreenIDValue" AutoRefresh="True" Enabled="False" DisableSpellcheck="True" />
                                    <px:PXSelector runat="server" DataField="DefaultActionId" ID="edDefaultAction" CommitChanges="True" />
                                </Template>
                                <Parameters>
                                    <px:PXControlParam ControlID="entityTree" Name="Key" PropertyName="SelectedValue" />
                                </Parameters>
                            </px:PXFormView>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Action Properties" RepaintOnDemand="False">
                        <Template>
                            <px:PXFormView ID="actionPropertiesForm" runat="server" DataSourceID="ds" Width="100%" DataMember="SelectedAction" SkinID="Transparent">
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M" ColumnWidth="100%" />
                                    <px:PXTextEdit runat="server" DataField="ActionName" ID="edActionName" />
                                    <px:PXTextEdit runat="server" DataField="MappedAction" ID="edMappedAction" AutoRefresh="True" CommitChanges="True" Enabled="False" />
                                </Template>
                                <Parameters>
                                    <px:PXControlParam ControlID="entityTree" Name="Key" PropertyName="SelectedValue" />
                                </Parameters>
                            </px:PXFormView>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Fields" RepaintOnDemand="False">
                        <Template>
                            <px:PXGrid ID="fieldsGrid" runat="server" DataSourceID="ds" Height="150px" CaptionVisible="False" AllowPaging="False"
                                Width="100%" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto" SyncPosition="True" MatrixMode="true">
                                <AutoSize Enabled="True" />
                                <ActionBar>
                                    <Actions>
                                        <ExportExcel Enabled="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton Text="Populate" CommandName="PopulateFields" CommandSourceID="ds">
                                            <AutoCallBack>
                                                <Behavior CommitChanges="True" PostData="Content" />
                                            </AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Text="Extend Entity" CommandName="ExtendEntity" CommandSourceID="ds">
                                            <AutoCallBack>
                                                <Behavior CommitChanges="True" PostData="Content" />
                                            </AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Text="Validate Entity" CommandName="ValidateEntity" CommandSourceID="ds">
                                            <AutoCallBack>
                                                <Behavior CommitChanges="True" PostData="Content" />
                                            </AutoCallBack>
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="Fields">
                                        <RowTemplate>
                                            <px:PXTextEdit ID="edFieldName" runat="server" DataField="FieldName" CommitChanges="True" />
                                            <px:PXSelector ID="edMappedObject" runat="server" DataField="MappedObject" CommitChanges="True" AutoRefresh="True" />
                                            <px:PXSelector ID="edFieldType" runat="server" DataField="FieldType" CommitChanges="True" />
                                            <px:PXTextEdit ID="edMappingKey" runat="server" DataField="MappingKey" CommitChanges="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="FieldName" Label="Field Name" CommitChanges="True" />
                                            <px:PXGridColumn DataField="MappedObject" Label="Mapped Object" CommitChanges="True" />
                                            <px:PXGridColumn DataField="MappedField" Label="Mapped Field" CommitChanges="True" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="FieldType" Label="Field Type" CommitChanges="True" />
                                            <px:PXGridColumn DataField="MappingKey" CommitChanges="True" SyncVisibility="True" SyncVisible="True" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Parameters>
                                    <px:PXControlParam ControlID="entityTree" Name="Key" PropertyName="SelectedValue" />
                                </Parameters>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Parameters" RepaintOnDemand="False">
                        <Template>
                            <px:PXGrid ID="actionsGrid" runat="server" DataSourceID="ds" Height="150px" CaptionVisible="False"
                                Width="100%" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto" SyncPosition="True">
                                <AutoSize Enabled="True" />
                                <ActionBar>
                                    <Actions>
                                        <ExportExcel Enabled="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton Text="Populate" CommandName="PopulateParameters" CommandSourceID="ds">
                                            <AutoCallBack>
                                                <Behavior CommitChanges="True" PostData="Content" />
                                            </AutoCallBack>
                                        </px:PXToolBarButton>
                                         <px:PXToolBarButton Text="Validate Action" CommandName="validateAction" CommandSourceID="ds">
                                            <AutoCallBack>
                                                <Behavior CommitChanges="True" PostData="Content" />
                                            </AutoCallBack>
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="Parameters">
                                        <RowTemplate>
                                            <px:PXTextEdit ID="edParameterName" runat="server" DataField="ParameterName" CommitChanges="True" />
                                            <px:PXSelector ID="edParameterMappedObject" runat="server" DataField="MappedObject" CommitChanges="True" AutoRefresh="True" />
                                            <px:PXSelector ID="edParameterType" runat="server" DataField="ParameterType" CommitChanges="True" />
                                            <px:PXTextEdit ID="edParameterMappingKey" runat="server" DataField="MappingKey" CommitChanges="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="ParameterName" Label="Field Name" CommitChanges="True" />
                                            <px:PXGridColumn DataField="MappedObject" Label="Mapped Object" CommitChanges="True" />
                                            <px:PXGridColumn DataField="MappedField" Label="Mapped Field" CommitChanges="True" />
                                            <px:PXGridColumn DataField="ParameterType" Label="Field Type" CommitChanges="True" />
                                            <px:PXGridColumn DataField="MappingKey" CommitChanges="True" SyncVisibility="True" SyncVisible="True" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Parameters>
                                    <px:PXControlParam ControlID="entityTree" Name="Key" PropertyName="SelectedValue" />
                                </Parameters>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                </Items>
            </px:PXTab>
        </Template2>
    </px:PXSplitContainer>


    <px:PXSmartPanel ID="pnlCreateEntity" runat="server" Caption="Create Entity"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateEntityView" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formCreateEntity" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="formCreateEntity" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="CreateEntityView">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit runat="server" DataField="FieldName" ID="PXTextEdit1" AutoRefresh="True" CommitChanges="True" />
                <px:PXTextEdit runat="server" DataField="ParameterName" ID="PXTextEdit2" AutoRefresh="True" CommitChanges="True" />
                <px:PXCheckBox SuppressLabel="True" ID="chkUseExisting" runat="server" Checked="False" DataField="UseExisting" CommitChanges="True" />
                <px:PXTextEdit runat="server" DataField="ObjectName" ID="edObjectName" AutoRefresh="True" CommitChanges="True" />
                <px:PXDropDown runat="server" DataField="ObjectType" ID="edObjectType" CommitChanges="True" />
                <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID" TextField="Title" ValueField="ScreenID" DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
                <px:PXTextEdit runat="server" DataField="ScreenIDValue" ID="edScreenIDValue" AutoRefresh="True" Enabled="False" DisableSpellcheck="True" />
                <px:PXSelector runat="server" DataField="EntityType" ID="PXSelector1" AutoRefresh="True" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>

    <px:PXSmartPanel ID="pnlExtendEndpoint" runat="server" Caption="Extend Current Endpoint" CaptionVisible="True" DesignView="Hidden" LoadOnDemand="true" Key="ExtendEndpointView"
        CreateOnDemand="false" AutoCallBack-Enabled="true" AutoCallBack-Target="formExtendEndpoint" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnActionOK">
        <px:PXFormView ID="formExtendEndpoint" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="ExtendEndpointView">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit runat="server" DataField="ExtendedEndpointName" ID="PXTextEdit3" Enabled="False" AutoRefresh="True" CommitChanges="False" />
                <px:PXTextEdit runat="server" DataField="ExtendedEndpointVersion" ID="PXTextEdit4" Enabled="False" AutoRefresh="True" CommitChanges="False" />
                <px:PXTextEdit runat="server" DataField="EndpointName" ID="PXTextEdit1" AutoRefresh="True" CommitChanges="True" />
                <px:PXTextEdit runat="server" DataField="EndpointVersion" ID="PXTextEdit2" AutoRefresh="True" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnExtendOk" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="BtnExtendCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>

    <px:PXSmartPanel ID="pnlCreateAction" runat="server" Caption="Create Action"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateActionView" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formCreateAction" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnActionOK">
        <px:PXFormView ID="formCreateAction" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="CreateActionView">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXSelector runat="server" DataField="MappedAction" ID="PXTextEdit2" AutoRefresh="True" CommitChanges="True" />
                <px:PXTextEdit runat="server" DataField="ActionName" ID="PXTextEdit1" AutoRefresh="True" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnActionOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="btnActionCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>

    <px:PXSmartPanel ID="pnlPopulateFields" runat="server" Caption="Populate Fields"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="PopulateFilterView" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formPopulateFields" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnActionOK" Width="50%">
        <px:PXFormView ID="formPopulateFields" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="PopulateFilterView">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXSelector runat="server" DataField="Container" ID="PXTextEdit1" AutoRefresh="True" CommitChanges="True" />
                <px:PXCheckBox runat="server" DataField="ShowSelectorFields" ID="edShowSelectorFields" CommitChanges="True">
                </px:PXCheckBox>
            </Template>
        </px:PXFormView>

        <px:PXGrid ID="populateGrid" runat="server" DataSourceID="ds" CaptionVisible="False"
            Width="100%" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto" SyncPosition="True" Height="100%">
            <AutoSize Enabled="True" MinHeight="320" MinWidth="100" Container="Parent" />
            <CallbackCommands>
                <Refresh CommitChanges="true"></Refresh>
            </CallbackCommands>
            <ActionBar PagerVisible="False">
                <CustomItems>
                    <px:PXToolBarButton Text="Select All" CommandName="SelectAll" CommandSourceID="ds">
                        <AutoCallBack>
                            <Behavior CommitChanges="True" PostData="Content" />
                        </AutoCallBack>
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="Clear" CommandName="ClearAll" CommandSourceID="ds">
                        <AutoCallBack>
                            <Behavior CommitChanges="True" PostData="Content" />
                        </AutoCallBack>
                    </px:PXToolBarButton>
                </CustomItems>
                <PagerSettings Mode="NextPrevFirstLast" />
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="PopulatingFields">
                    <Mode AllowAddNew="false" AllowDelete="false" />
                    <Columns>
                        <px:PXGridColumn AllowNull="False" DataField="Populate" TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="true"
                            AllowCheckAll="true" />
                        <px:PXGridColumn DataField="Field" Label="Field" CommitChanges="True" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>

        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>

</asp:Content>
