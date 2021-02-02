<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU201020.aspx.cs" Inherits="Page_AU201020"
	Title="Workflows" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXFormView runat="server" SkinID="transparent" ID="formTitle" 
                   DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
        <Template>
            <px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
                           SkinID="Label" SuppressLabel="true"
                           Width="90%"
                           style="padding: 10px">
                <font size="14pt" names="Arial,sans-serif;"/>
            </px:PXTextEdit>
        </Template>
    </px:PXFormView>
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUWorkflowDefinitionMaint"
		PrimaryView="Definition" PageLoadBehavior="SearchSavedKeys">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" /> 
            <px:PXDSCallbackCommand CommitChanges="True" Name="actionLoadWorkflows" RepaintControls="All" /> 
            <px:PXDSCallbackCommand CommitChanges="False" Name="refreshAll" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="actionViewChanges" Visible="True" RepaintControls="All" DependOnGrid="gridWorkflows" />
		</CallbackCommands>
	</pxa:AUDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" 
                   DataMember="Definition">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="M" />
            <px:PXDropDown ID="edStateField" runat="server" DataField="StateField" AllowNull="True" CommitChanges="True" />
            <px:PXDropDown ID="edFlowTypeField" runat="server" DataField="FlowTypeField" AllowNull="True" CommitChanges="True" />
            <px:PXCheckBox ID="chkEnableWorkflowIDField" runat="server" DataField="EnableWorkflowIDField" />
        </Template>
    </px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="gridWorkflows" runat="server" DataSourceID="ds" Height="150px"
               Width="100%" AdjustPageSize="Auto" SkinID="Details" SyncPosition="True" 
               MatrixMode="true" AllowPaging="false" AutoAdjustColumns="true" >		
        <Mode AllowAddNew="False"/>
        <AutoCallBack Enabled="True" Command="refreshAll" Target="ds" SuppressOnReload="True" FromUIOnly="True"  ></AutoCallBack>
        <ActionBar>
            <Actions>
                <AddNew ToolBarVisible="False" />
                <AdjustColumns ToolBarVisible = "false" />
                <ExportExcel ToolBarVisible ="false" />
            </Actions>
        </ActionBar> 
        <Levels>
            <px:PXGridLevel DataMember="Workflows" >
                <Mode InitNewRow="True" AutoInsert="True"  />
                <Columns>
                    <%--<px:PXGridColumn DataField="IsOverride" Type="CheckBox" Width="60px" TextAlign="Center" />--%>
                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                    <px:PXGridColumn DataField="IsSystem" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                    <px:PXGridColumn DataField="IsCustomized" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                    <px:PXGridColumn DataField="WorkflowID" Width="50px" CommitChanges="True" AllowNull="True" NullText="DEFAULT" />
                    <px:PXGridColumn DataField="Description" Width="200px" LinkCommand="navigateWorkflow" />
                    <px:PXGridColumn DataField="SysWorkflowID" Width="200px"  />
                    <px:PXGridColumn DataField="CalcStatus" Width="50px" />
                    <%--<px:PXGridColumn DataField="IsDefault" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />--%>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="150" Container="Window" />      
        <CallbackCommands>
            <InitRow CommitChanges="True"/>
            <Refresh CommitChanges="True"></Refresh>
        </CallbackCommands>
    </px:PXGrid>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phDialogs" runat="Server">
    
        
    <px:PXSmartPanel runat="server" ID="DlgExtendWorkflow" Key="DlgExtendWorkflow" 
                     Caption="Add Workflow" CaptionVisible="True" 
                     DesignView="Hidden"
                     LoadOnDemand="True"
                     AutoReload="True"
                     AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel"
                     AutoCallBack-Enabled="true" AutoCallBack-Target="FormPreview" AutoCallBack-Command="Refresh"
                     ShowAfterLoad="True"
                     Width="400">
        <px:PXFormView runat="server" ID="Form" DataMember="DlgExtendWorkflow" Width="100%" SkinID="Transparent" AutoRepaint="True">
            <Template>
                <px:PXLayoutRule runat="server"/>
                <px:PXDropDown runat="server" ID="Operation" DataField="Operation" CommitChanges="True"/>
                <px:PXDropDown runat="server" ID="SysWorkflowID" DataField="SysWorkflowID"/>
                <px:PXDropDown runat="server" ID="WorkflowToCopy" DataField="WorkflowToCopy"/>
                <px:PXDropDown runat="server" ID="WorkflowID" DataField="WorkflowID" CommitChanges="True"/>
               
                <px:PXTextEdit runat="server" ID="Description" DataField="Description"/>
            </Template>
           
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>

    </px:PXSmartPanel>
    
    <px:PXSmartPanel runat="server" ID="PXSmartPanelConflicts" Key="Conflicts" 
                     Caption="Upgrade conflicts" CaptionVisible="True" 
                     DesignView="Hidden"
                     LoadOnDemand="True"
                     AutoReload="True"
                     ShowCloseButton="False"
                     AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel"
                     AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh"
                     ShowAfterLoad="True"
                     Width="1400">
        <px:PXGrid ID="gridConflicts" runat="server" DataSourceID="ds" Style="z-index: 100" SkinID="Details" AutoAdjustColumns="True"
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
                <px:PXGridLevel  DataMember="Conflicts">
                    <Columns>
                        <px:PXGridColumn DataField="ObjectName" Width="300" />
                        <px:PXGridColumn DataField="Property" Width="100" />
                        <px:PXGridColumn DataField="SystemValue" Width="100" />
                        <px:PXGridColumn DataField="CustomizationValue" Width="100"   />
                        <px:PXGridColumn DataField="SystemAction" Width="50"  />
                        <px:PXGridColumn DataField="CustomizationAction" Width="50" />
                        <px:PXGridColumn DataField="Action" Width="200" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <CallbackCommands>
                <FetchRow RepaintControls="None" />
                <Refresh CommitChanges="True" RepaintControls="All" />
            </CallbackCommands>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel">
                <AutoCallBack Enabled="True" Target="ds" Command="ActionClosePanel"/>
            </px:PXButton>
        </px:PXPanel>

    </px:PXSmartPanel>
    
    <px:PXSmartPanel ID="PanelViewChanges" runat="server" Caption="Changes" CaptionVisible="True" 
                     Width="800px" Height="600px" Key="Changes" AutoRepaint="True" >
        <px:PXFormView runat="server" ID="FormPanelViewChanges" DataMember="Changes" DataSourceID="ds" Height="100%"
                       Width="100%"  >
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXHtmlView runat="server" ID="edMessages" DataField="Messages" TextMode="MultiLine"  Height="100%" Width="100%"
                               Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                    <AutoSize Enabled="True" MinHeight="350" Container="Parent" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="FormPanelViewChangesButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="FormPanelViewChangesButtonRevert" runat="server" Text="Revert all changes" CommandSourceID="ds" CommandName="actionReset" >
              
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
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
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
