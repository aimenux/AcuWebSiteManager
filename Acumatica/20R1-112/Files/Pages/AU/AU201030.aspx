<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    CodeFile="AU201030.aspx.cs" Inherits="Page_AU201030"
    Title="Workflow" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <style type="text/css">


        .nodeHint {
            color: #aaa;
            font-style: italic;
            font-weight: normal !important;
            
        }

    </style>
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
    <pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUWorkflowMaint"
        PrimaryView="States" PageLoadBehavior="SearchSavedKeys">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="RemoveNode" Visible="False" RepaintControls="All" RepaintControlsIDs="tree,FormState" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="addState" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="addPredefinedState" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="addTransition" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="False" Name="refreshAll" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="False" Name="refreshActionProps" Visible="False" RepaintControls="None" RepaintControlsIDs="gridStateActionsFields,gridStateActionsParams" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="moveUp" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="moveDown" Visible="False" RepaintControls="All"  />
            <px:PXDSCallbackCommand CommitChanges="True" Name="comboBoxValues" Visible="False" DependOnGrid="gridStateProperties" StartNewGroup="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createAction" Visible="False" StartNewGroup="True" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createActionForState" Visible="False" StartNewGroup="True" RepaintControls="All" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="Items" TreeKeys="NodeID" />
        </DataTrees>
    </pxa:AUDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXTreeView ID="tree" runat="server" DataSourceID="ds" DataMember="Items" Height="180px" ShowLines="false" KeepPosition="True"
                AutoRepaint="True" SyncPosition="True" SyncPositionWithGraph="True" PreserveExpanded="True" ExpandDepth="1" 
                Caption="States and Transitions" ShowRootNode="False" SelectFirstNode="True" ShowImages="False" ShowDefaultImages="False" RenderHtmlText="True" >
                <Images>
                    <NodeImages Normal="tree@Folder" Selected="tree@FolderS"  ></NodeImages>
                    <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                    <LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
                </Images>
                <AutoCallBack Command="RefreshAll" Target="ds" />
                <CallBackMode RepaintControlsIDs="ds"></CallBackMode>
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="Items" TextField="DisplayName" ValueField="NodeID" />
                </DataBindings>
                <AutoSize Enabled="True" />
                <Styles>
                    <ParentNode CustomAttr="text-transform: capitalize;"></ParentNode>
                    
                </Styles>
                <ToolBarItems>
                    <px:PXToolBarButton CommandSourceID="ds" ImageKey="Refresh" CommandName="refreshAll" DisplayStyle="Image" />
                    <px:PXToolBarButton ImageKey="AddNew" DisplayStyle="Image" Tooltip="Add New">
                        <MenuItems>
                            <px:PXMenuItem>
                                <AutoCallBack Command="addState" Enabled="true" Target="ds" />
                            </px:PXMenuItem>
                            <px:PXMenuItem>
                                <AutoCallBack Command="addPredefinedState" Enabled="true" Target="ds" />
                            </px:PXMenuItem>
                            <px:PXMenuItem>
                                <AutoCallBack Command="addTransition" Enabled="true" Target="ds" />
                            </px:PXMenuItem>
                        </MenuItems>
                    </px:PXToolBarButton>
                    <px:PXToolBarButton CommandSourceID="ds" CommandName="RemoveNode" ImageKey="Remove" Enabled="true" DisplayStyle="Image"  />
                    <px:PXToolBarButton CommandSourceID="ds" CommandName="moveUp" ImageKey="ArrowUp" Enabled="False" DisplayStyle="Image" />
                    <px:PXToolBarButton CommandSourceID="ds" CommandName="moveDown" ImageKey="ArrowDown" Enabled="False" DisplayStyle="Image" />
                </ToolBarItems>
            </px:PXTreeView>
        </Template1>
        <Template2>
            <px:PXTab runat="server" ID="tab" Width="100%" RepaintOnDemand="false">
                <AutoSize Enabled="True"></AutoSize>
                <Items>
                    <px:PXTabItem Text="State Properties" RepaintOnDemand="false">
                        <Template>
                            <px:PXFormView ID="FormState" runat="server"
                                SkinID="Transparent"
                                DataMember="States"
                                DataSourceID="ds"
                                AutoRepaint="True">
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" LabelsWidth="SM" ControlSize="S" StartRow="True" Merge="True"></px:PXLayoutRule>
                                    <px:PXTextEdit runat="server" ID="edStateIdentifier" DataField="Identifier"></px:PXTextEdit>
                                    <%--<px:PXCheckBox runat="server" ID="PXCheckBox1" DataField="IsOverride"></px:PXCheckBox>--%>
                                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" ControlSize="XM"></px:PXLayoutRule>
                                    <px:PXTextEdit runat="server" ID="edStateDisplayName" DataField="DisplayName"></px:PXTextEdit>
                                   
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True"></px:PXLayoutRule>
                                    <px:PXCheckBox runat="server" ID="edStateIsActive" DataField="IsActive"  CommitChanges="True"></px:PXCheckBox>
                                    <px:PXCheckBox runat="server" ID="edStateIsInitial" DataField="IsInitial"></px:PXCheckBox>
                                </Template>
                            </px:PXFormView>
                            <px:PXGrid runat="server" ID="gridStateProperties" Caption="Fields" AutoAdjustColumns="True" Width="100%"  DataSourceID="ds" 
                                MatrixMode="True" SyncPosition="True" SkinID="DetailsInTab" AllowPaging="True" AdjustPageSize="Auto" OnEditorsCreated="grd_EditorsCreated_RelativeDates" >
                                <AutoCallBack Enabled="True" Command="refreshAll" Target="ds" SuppressOnReload="True" FromUIOnly="True"  ></AutoCallBack>
                                <AutoSize Enabled="True" MinHeight="150" />
                                <ActionBar ActionsVisible="True" >
                                    <Actions>
                                        <ExportExcel ToolBarVisible="False"></ExportExcel>
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton DisplayStyle="Text" Text="Combo Box Values" Visible="True" >
                                            <ActionBar ToolBarVisible="Top" Order="3" GroupIndex="2"></ActionBar>
                                            <AutoCallBack Command="comboBoxValues" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="StatePropertiesPerState" >
                                        <Mode InitNewRow="True" AllowRowSelect="True" />
                                        <Columns>
                                            <px:PXGridColumn DataField="IsActive" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                            <px:PXGridColumn DataField="ObjectName" Width="200px" CommitChanges="True" />
                                            <px:PXGridColumn DataField="FieldName" Width="200px" CommitChanges="True" />
                                            <px:PXGridColumn DataField="IsDisabled" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                            <px:PXGridColumn DataField="IsHide" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                            <px:PXGridColumn DataField="IsRequired" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                            <px:PXGridColumn DataField="DefaultValue" Width="200px"  MatrixMode="true" AllowStrings="True" DisplayMode="Value" CommitChanges="True" />
                                            <px:PXGridColumn DataField="Status" Width="50px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <CallbackCommands>
                                    <%--<Refresh CommitChanges="True" RepaintControls="All" />--%>
                                    <InitRow CommitChanges="False" />
                                </CallbackCommands>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Actions" RepaintOnDemand="False">
                        <Template>
                            <px:PXGrid runat="server" ID="gridStateActions" CaptionVisible="False" AutoAdjustColumns="True" Width="100%" DataSourceID="ds" 
                                MatrixMode="True" SyncPosition="True" SkinID="Details" AllowPaging="True" AdjustPageSize="Auto" >
                                <AutoSize Enabled="True" MinHeight="150" />
                                <AutoCallBack Enabled="True" Command="refreshActionProps" Target="ds" SuppressOnReload="True"></AutoCallBack>
                                <ActionBar ActionsVisible="True">
                                    <Actions>
                                        <ExportExcel ToolBarVisible="False"></ExportExcel>
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton DisplayStyle="Text" Text="CREATE ACTION" Visible="True">
                                            <ActionBar ToolBarVisible="Top" Order="3" GroupIndex="2"></ActionBar>
                                            <AutoCallBack Command="createActionForState" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="StateActionsPerState">
                                        <Mode InitNewRow="True" />
                                        <Columns>
                                            <px:PXGridColumn DataField="IsActive" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                            <px:PXGridColumn DataField="ActionName" Width="200px" CommitChanges="True" DisplayMode="Text" Type="DropDownList" />
                                            <px:PXGridColumn DataField="IsTopLevel" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                          <%--  <px:PXGridColumn DataField="IsDisabled" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                            <px:PXGridColumn DataField="IsHide" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />--%>
                                            <px:PXGridColumn DataField="AutoRun" Width="200px" CommitChanges="True" />
                                            <px:PXGridColumn DataField="Status" Width="50px" />
                                            <px:PXGridColumn DataField="FormName" Width="200px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <CallbackCommands>
                                    <%--<Refresh CommitChanges="True" RepaintControls="All" />--%>
                                    <InitRow CommitChanges="true" />
                                </CallbackCommands>
                            </px:PXGrid>
                            
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Transition Properties" RepaintOnDemand="False">
                        <Template>
                            <px:PXFormView ID="FormTransition" runat="server"
                                SkinID="Transparent"
                                DataMember="CurrentTransition"
                                DataSourceID="ds"
                                AutoRepaint="True">
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" LabelsWidth="SM" ControlSize="M" StartRow="True"></px:PXLayoutRule>
                                    <px:PXDropDown runat="server" ID="edTransitionFromState" DataField="FromStateName"></px:PXDropDown>
                                    <px:PXDropDown runat="server" ID="edTransitionActionName" DataField="ActionName" CommitChanges="True"></px:PXDropDown>
                                    <px:PXSelector ID="edTransitionConditionID" runat="server" CommitChanges="True"
                                        AllowNull="True" DataField="ConditionID" AutoGenerateColumns="False" DisplayMode="Text"
                                        AutoRefresh="True">
                                        <GridProperties>
                                            <Columns>
                                                <px:PXGridColumn DataField="ConditionName" Width="200px">
                                                </px:PXGridColumn>
                                            </Columns>

                                        </GridProperties>
                                    </px:PXSelector>
                                    <px:PXDropDown runat="server" ID="edTransitionTargetStateName" DataField="TargetStateName" CommitChanges="True"></px:PXDropDown>
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True"></px:PXLayoutRule>
                                    <px:PXCheckBox runat="server" ID="edTransitionIsActive" DataField="IsActive"  CommitChanges="True"></px:PXCheckBox>
                                 
                                </Template>
                            </px:PXFormView>
                            <px:PXGrid runat="server" ID="gridTransitionsFields" CaptionVisible="True" AutoAdjustColumns="True" Width="100%" Caption="Fields to Update After Transition"
                                MatrixMode="True" SyncPosition="True" SkinID="DetailsInTab" OnEditorsCreated="grd_EditorsCreated_RelativeDates">
                                <AutoSize Enabled="True" MinHeight="150" />
                                <ActionBar ActionsVisible="True">
                                    <Actions>
                                        <ExportExcel ToolBarVisible="False"></ExportExcel>
                                    </Actions>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="TransitionFieldsPerTransition">
                                        <Mode InitNewRow="True" />
                                        <RowTemplate>
                                            <pxa:PXFormulaCombo ID="edTFPTValue" runat="server" DataField="Value" EditButton="True"
                                                                FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields and Parameters" 
                                                                IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                                                SkinID="GI">
                                                <Parameters>
                                                    <px:PXParam Name="UseParentAction"></px:PXParam>
                                                </Parameters>
                                            </pxa:PXFormulaCombo>
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
        </Template2>
    </px:PXSplitContainer>
</asp:Content>

<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="server">
    <px:PXSmartPanel ID="PanelAddPredefinedState" runat="server"
        Caption="Add Predefined State"
        CaptionVisible="True"
        AutoReload="True"
        LoadOnDemand="True" ShowAfterLoad="True" AutoRepaint="True"
        Key="AddPredefinedWorkflowState">
        <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="True" />
        <px:PXFormView ID="FormAddPredefinedState" runat="server"
            SkinID="Transparent"
            DataMember="AddPredefinedWorkflowState"
            DataSourceID="ds"
            AutoRepaint="True">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M"></px:PXLayoutRule>
                <px:PXDropDown ID="AddPredefinedWorkflowStateID" runat="server"
                    AllowNull="True" DataField="StateID">
                </px:PXDropDown>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
            <px:PXButton ID="PanelAddPredefinedStateButtonOk" runat="server"
                CausesValidation="False"
                Text="OK"
                DialogResult="OK" />
            <px:PXButton ID="PanelAddPredefinedStateButtonCancel" runat="server"
                CausesValidation="False" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PXSmartPanelAddState" runat="server"
        Caption="Add State"
        CaptionVisible="True"
        AutoReload="True"
        LoadOnDemand="True" ShowAfterLoad="True" AutoRepaint="True"
        Key="AddWorkflowState">
        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartRow="True" />
        <px:PXFormView ID="PXFormView1" runat="server"
            SkinID="Transparent"
            DataMember="AddWorkflowState"
            DataSourceID="ds"
            AutoRepaint="True">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M"></px:PXLayoutRule>
                <px:PXTextEdit runat="server" ID="edSIdentifier" DataField="Identifier"></px:PXTextEdit>
                <px:PXTextEdit runat="server" ID="edSDisplayName" DataField="DisplayName"></px:PXTextEdit>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server"
                CausesValidation="False"
                Text="OK"
                DialogResult="OK" />
            <px:PXButton ID="PXButton2" runat="server"
                CausesValidation="False" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PXSmartPanelAddTransition" runat="server" 
        Caption="Add Transition" 
        CaptionVisible="True"  
        AutoReload="True"
        LoadOnDemand="True" ShowAfterLoad="True" AutoRepaint="True"
        Key="AddWorkflowTransition">
        <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartRow="True" />
        <px:PXFormView ID="PXFormView2" runat="server"
            SkinID="Transparent"
            DataMember="AddWorkflowTransition"
            DataSourceID="ds"
                       
            AutoRepaint="True">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M"></px:PXLayoutRule>
                <px:PXDropDown runat="server" ID="edTIdentifier" DataField="FromStateName" Enabled="False"></px:PXDropDown>
                <px:PXLayoutRule ID="PXLayoutRule10" runat="server" ControlSize="M" Merge="True"></px:PXLayoutRule>
                <px:PXDropDown ID="edTActionName" runat="server" AllowNull="False" DataField="ActionName" CommitChanges="True" >
                </px:PXDropDown>
                <px:PXButton ID="PXButtonAddAction" runat="server" Text="Create" CommandName="createAction" CommandSourceID="ds"  >
                    
                </px:PXButton>
                <px:PXLayoutRule ID="PXLayoutRule11" runat="server" ControlSize="M" ></px:PXLayoutRule>
                <px:PXSelector ID="edTCondition" runat="server" CommitChanges="True"
                    AllowNull="True" DataField="ConditionID" AutoGenerateColumns="False"
                    AutoRefresh="True">
                    <GridProperties>
                        <Columns>
                            <px:PXGridColumn DataField="ConditionName" Width="200px">
                            </px:PXGridColumn>
                        </Columns>
                    </GridProperties>
                </px:PXSelector>
                <px:PXDropDown ID="edTTargetState" runat="server"
                    AllowNull="False" DataField="TargetStateName">
                </px:PXDropDown>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton3" runat="server"
                CausesValidation="False"
                Text="OK"
                DialogResult="OK" />

            <px:PXButton ID="PXButton4" runat="server" 
                CausesValidation="False" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlCombos" runat="server" Style="z-index: 108;left: 351px; position: absolute; top: 99px" 
                     Width="550px" Caption="Combo Box Values" CaptionVisible="true" LoadOnDemand="true" Key="StateProperties" 
                     AutoCallBack-Enabled="true" AutoCallBack-Target="gridCombos" AutoCallBack-Command="Refresh" 
                     CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <div style="padding: 5px">
            <px:PXGrid ID="gridCombos" runat="server" DataSourceID="ds" Style="z-index: 100" SkinID="Details" AutoAdjustColumns="True"
                       Width="100%">
                <AutoSize Enabled="True" MinHeight="243"></AutoSize>
                <ActionBar>
                    <Actions>
                        <Refresh ToolBarVisible="False"></Refresh>
                        <ExportExcel ToolBarVisible="False"></ExportExcel>
                        <AdjustColumns ToolBarVisible="False"></AdjustColumns>
                        <AddNew ToolBarVisible="False"></AddNew>
                        <Delete ToolBarVisible="False"></Delete>
                    </Actions>
                </ActionBar>
                <Levels>
                    <px:PXGridLevel  DataMember="Combos">
                        <Columns>
                            <px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
                            <px:PXGridColumn DataField="Value" CommitChanges="True" />
                            <px:PXGridColumn DataField="Description" Width="200px"  />
                      
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <CallbackCommands>
                    <FetchRow RepaintControls="None" />
                    <Refresh CommitChanges="True" RepaintControls="All" />
                </CallbackCommands>
            </px:PXGrid>
        </div>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" Text="OK" DialogResult="OK" CausesValidation="True" />
            <px:PXButton ID="PXButton6" runat="server" CausesValidation="False" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PXSmartPanelAddAction" runat="server" 
                     Caption="New Action"
                     CaptionVisible="True"
        AutoRepaint="True"
        Key="AddWorkflowAction">
        <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartRow="True" />
        <px:PXFormView ID="PXFormView3" runat="server"
            SkinID="Transparent"
            DataMember="AddWorkflowAction"
            DataSourceID="ds"
            AutoRepaint="True">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" ></px:PXLayoutRule>
                <px:PXDropDown runat="server" ID="edAActionType" DataField="ActionType" Enabled="False" ></px:PXDropDown>
                <px:PXTextEdit ID="edAActionName" runat="server" AllowNull="False" DataField="ActionName" CommitChanges="True">
                </px:PXTextEdit>
                <px:PXTextEdit ID="edADisplayName" runat="server" DataField="DisplayName">
                </px:PXTextEdit>
                <px:PXDropDown runat="server" ID="edAActionFormName" DataField="FormName"></px:PXDropDown>       
                <px:PXDropDown runat="server" ID="edAActionFolder" DataField="ActionFolder"></px:PXDropDown>                
                
                <px:PXCheckBox runat="server" ID="edATopLevel" DataField="IsTopLevel" CommitChanges="True"></px:PXCheckBox>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton7" runat="server"
                CausesValidation="False"
                Text="OK"
                DialogResult="OK" />

            <px:PXButton ID="PXButton8" runat="server"
                CausesValidation="False" DialogResult="Cancel" Text="Cancel" />
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
    <px:PXFormView runat="server" ID="PXFormView4" DataMember="ConfirmRevert" DataSourceID="ds" Height="100%"
                   Width="100%" AutoRepaint="True" >
        <AutoSize Enabled="True"></AutoSize>
        <Template>
            <px:PXHtmlView runat="server" ID="PXHtmlView1" DataField="Message" TextMode="MultiLine"  Height="100%" Width="100%"
                           Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                <AutoSize Enabled="True" Container="Parent" />
            </px:PXHtmlView>
        </Template>
    </px:PXFormView>
    <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButton9" runat="server" Text="OK" DialogResult="OK"/>
        <px:PXButton ID="PXButton10" runat="server" DialogResult="Cancel" Text="Close"/>
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
