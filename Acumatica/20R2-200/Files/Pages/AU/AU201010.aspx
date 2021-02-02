<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU201010.aspx.cs" Inherits="Page_AU201010"
	Title="Conditions" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
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
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUScreenConditionMaint"
		PrimaryView="Conditions" >
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" /> 
		</CallbackCommands>
	</pxa:AUDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    
    <px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds"  
               Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Primary" SyncPosition="true" AllowPaging="false" >
        <Mode AllowFormEdit="False" InitNewRow="False" AllowAddNew="False"/>
        <%--<AutoCallBack Enabled="true" Command="Refresh" Target="gridFilters"/>--%>	
        <AutoSize Container="Window"  Enabled="True" MinHeight="150" />
        <ActionBar>
            <Actions>
                <AdjustColumns  ToolBarVisible = "false" />
                <ExportExcel  ToolBarVisible ="false" />
	            <AddNew ToolBarVisible="False"/>
            </Actions>
        </ActionBar>
        <Levels>
            <px:PXGridLevel DataMember="Conditions" SortOrder="Order">							        
                <Columns>									        
                   <%-- <px:PXGridColumn DataField="IsActive" Width="100" Type="CheckBox"/> --%>
                    <px:PXGridColumn DataField="ConditionName" Width="200px" LinkCommand="actionEditCondition"/>                                    
                    <%--<px:PXGridColumn DataField="Description" Width="200"/>--%> 
                    <px:PXGridColumn DataField="Expression" Width="400px"/>  
                    <px:PXGridColumn DataField="CalcStatus" Width="150px"/> 

                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="150" />
    </px:PXGrid>        

    		
</asp:Content>

<asp:Content runat="server" ID="phDialogs" ContentPlaceHolderID="phDialogs">
<%--	CommandName="ActionClosePanel"
	AutoReload="True"
	AutoCallBack-Enabled="true" AutoCallBack-Target="FormSelectedCondition" AutoCallBack-Command="Refresh"
	LoadOnDemand="True"
	CommandSourceID="ds"--%>
    <px:PXSmartPanel runat="server" ID="PanelEditCondition" Key="SelectedCondition"
                     Caption="Condition Properties" CaptionVisible="True"
                     AutoRepaint="True"
                     AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel"
                     ShowCloseButton="False"
                     ShowAfterLoad="True"
                     Width="900" Height="550px">

        <px:PXFormView runat="server" DataSourceID="ds"  ID="FormSelectedCondition" DataMember="SelectedCondition" Width="100%" SkinID="Transparent" AutoRepaint="True">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit ID="ConditionName" runat="server" DataField="ConditionName" CommitChanges="True"/>
                <%--<px:PXTextEdit runat="server" ID="Description" DataField="Description"/>--%>
                <px:PXCheckBox runat="server" ID="AppendSystemCondition" DataField="AppendSystemCondition" CommitChanges="True"/>
                <px:PXCheckBox runat="server" ID="InvertCondition" DataField="InvertCondition"/>

                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
              
                <px:PXSelector runat="server" ID="ParentCondition" DataField="ParentCondition"/>
                <px:PXDropDown runat="server" ID="JoinMethod" DataField="JoinMethod"/>
                       
            </Template>

        </px:PXFormView>
        

           <px:PXGrid ID="gridFilters" runat="server" DataSourceID="ds" Style="z-index: 100"
                           Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
                           MatrixMode="true" AllowPaging="false" AutoAdjustColumns="true" OnEditorsCreated="grd_EditorsCreated_RelativeDates">					
                    <ActionBar>
                        <Actions>
                            <AdjustColumns  ToolBarVisible = "false" />
                            <ExportExcel  ToolBarVisible ="false" />
                        </Actions>
                    </ActionBar> 
                    <Levels>
                        <px:PXGridLevel DataMember="Filters" >
                            <Mode InitNewRow="True"  />
                            <RowTemplate>
                                <pxa:PXFormulaCombo ID="edFilterValue" runat="server" DataField="Value" EditButton="True"
                                                    FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields and Parameters"
                                                    IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                                    SkinID="GI"/>
                                <pxa:PXFormulaCombo ID="edFilterValue2" runat="server" DataField="Value2" EditButton="True"
                                                    FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields and Parameters" 
                                                    IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                                    SkinID="GI"/>
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="IsActive" Type="CheckBox" Width="60px" TextAlign="Center"  />
                                <px:PXGridColumn DataField="OpenBrackets" Type="DropDownList" Width="50px" />
                                <px:PXGridColumn DataField="FieldName" Width="200px" CommitChanges="True" AutoCallBack="true" />
                                <px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" CommitChanges="True"/>
                                <px:PXGridColumn DataField="IsFromScheme" Type="CheckBox" Width="60px" TextAlign="Center" CommitChanges="True" />
                                <px:PXGridColumn DataField="Value" Width="200px" CommitChanges="True" Key="value" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value" />
                                <px:PXGridColumn DataField="Value2" Width="200px" CommitChanges="True" Key="value" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value" />
                                <px:PXGridColumn DataField="CloseBrackets" Type="DropDownList" Width="50px" />
                                <px:PXGridColumn DataField="Operator" Type="DropDownList" Width="50px" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="100" />      
                    <CallbackCommands>
                        <InitRow CommitChanges="true" />
                        <FetchRow CommitChanges="True"></FetchRow>
                    </CallbackCommands>
                </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" >
	            <AutoCallBack Enabled="True" Target="ds" Command="ActionClosePanel"/>

			</px:PXButton>
        </px:PXPanel>

    </px:PXSmartPanel>
</asp:Content>
