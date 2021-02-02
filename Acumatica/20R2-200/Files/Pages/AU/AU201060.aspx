<%@ Page Title="Field Automation" Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" CodeFile="AU201060.aspx.cs" Inherits="Pages_AU_AU201060" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
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

	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUWorkflowFieldsMaint"
	                  PrimaryView="ViewFields" >
		<CallbackCommands>

		</CallbackCommands>
	</pxa:AUDataSource>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phL" Runat="Server">
	                            <px:PXGrid runat="server" ID="gridStateProperties" AutoAdjustColumns="True" Width="100%"  DataSourceID="ds" 
                                           SyncPositionWithGraph="True" SkinID="Primary" AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" KeepPosition="True"  >
                                    <AutoCallBack Enabled="True" Command="refreshGrid" Target="ds" SuppressOnReload="True"></AutoCallBack>
                                <AutoSize Enabled="True" Container="Window" MinHeight="150" />
		                            <ActionBar>
			                            <Actions>
				                            <AdjustColumns  ToolBarVisible = "false" />
				                            <ExportExcel  ToolBarVisible ="false" />
				                            <AddNew ToolBarVisible="False"/>
			                            </Actions>
		                            </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="ViewFields" >
                                        <Mode InitNewRow="True" AllowRowSelect="True" />
                                        <Columns>
                                          
                                            <px:PXGridColumn DataField="ObjectName" Width="200px" CommitChanges="True" MatrixMode="True"  />
                                            <px:PXGridColumn DataField="FieldName" Width="200px" CommitChanges="True" MatrixMode="True" />
	                                        <px:PXGridColumn DataField="DisableCondition" Width="200px"/> 
	                                        <px:PXGridColumn DataField="HideCondition" Width="200px"/> 
	                                        <px:PXGridColumn DataField="RequiredCondition" Width="200px"/> 
	                                        <px:PXGridColumn DataField="DisplayName" Width="200px"/> 
                                            <px:PXGridColumn DataField="Status" Width="50px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <CallbackCommands>
                                    <%--<Refresh CommitChanges="True" RepaintControls="All" />--%>
                                    <InitRow CommitChanges="False" />
                                </CallbackCommands>
                            </px:PXGrid>
</asp:Content>


<asp:Content ID="Content3" ContentPlaceHolderID="phDialogs" Runat="Server">
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
    <px:PXSmartPanel runat="server" ID="PanelFilterNewField" Key="FilterNewField"
                     Caption="Add Field" 
                     CaptionVisible="True"
                     AutoRepaint="True"
                     AcceptButtonID="PXButtonOK"
                     CancelButtonID="PXButtonCancel"
                     ShowAfterLoad="True"
                     Width="1000px">
        <px:PXFormView runat="server" DataSourceID="ds"  ID="FormFilterNewField" DataMember="FilterNewField" SkinID="Transparent" AutoRepaint="True">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
                <px:PXDropDown ID="edContainer" runat="server" DataField="Container" CommitChanges="True"/>
                <px:PXDropDown ID="ObjectName" runat="server" DataField="ObjectName" CommitChanges="True"/>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
                <px:PXDropDown ID="FieldName" runat="server" DataField="FieldName" CommitChanges="True"/>
                <px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" CommitChanges="True"/>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="PXGridNewFields" runat="server" DataSourceID="ds" Width="100%" AutoAdjustColumns="True">
            <AutoSize Enabled="True" MinHeight="450"></AutoSize>
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
                <px:PXGridLevel DataMember="NewFields" >
                    <Columns>
                        <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="True" CommitChanges="True" />
                        <px:PXGridColumn DataField="Container" />
                        <px:PXGridColumn DataField="ObjectName"  />
                        <px:PXGridColumn DataField="FieldName" />
                        <px:PXGridColumn DataField="DisplayName" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <CallbackCommands>
                <FetchRow RepaintControls="None" />
                <Refresh CommitChanges="True" RepaintControls="All" />
            </CallbackCommands>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonAdd" runat="server" Text="Add">
                <AutoCallBack Target="ds"  Command="addFields"></AutoCallBack>
            </px:PXButton>
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="Add & Close">
                <AutoCallBack Target="ds"  Command="addFieldsAndClose"></AutoCallBack>
            </px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Close" >
                <AutoCallBack Target="ds"  Command="closeAddPanel"></AutoCallBack>
			</px:PXButton>
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
                <px:PXHtmlView runat="server" ID="edMessages" DataField="Message" TextMode="MultiLine"  Height="100%" Width="100%"
                               Style="font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                    <AutoSize Enabled="True" Container="Parent" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK"/>
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close"/>
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

