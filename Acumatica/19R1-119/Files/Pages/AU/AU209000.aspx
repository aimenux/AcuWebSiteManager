<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AU209000.aspx.cs" Inherits="Page_AU204000" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <label class="projectLink transparent border-box">Database Scripts</label>
    <px:PXDataSource ID="ds"
        runat="server"
        Visible="true"
        TypeName="PX.SM.ProjectScriptMaintenance"
        PrimaryView="CustObjects"
        SuspendUnloading="false">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="actionEdit" CommitChanges="true" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="actionDeleteCustomObject" CommitChanges="true" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="actionUpdateFromDatabase" CommitChanges="true" RepaintControls="All" />

            <px:PXDSCallbackCommand Name="actionAddSqlAttribute" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionAddNewCustomColumnToExistedTable" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionDeleteCustomColumnFromExistedTable" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionIncreaseColumnLengthToExistedTable" CommitChanges="true" Visible="false" />
        </CallbackCommands>        
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2"
    ContentPlaceHolderID="phL"
    runat="Server">
    <px:PXGrid ID="grid"
        runat="server"
        Height="400px"
        Width="100%"
        Style="z-index: 100"
        AllowPaging="True"
        AllowSearch="True"
        AdjustPageSize="Auto"
        DataSourceID="ds"
        SkinID="Primary"
        TabIndex="900"
        AutoAdjustColumns="True"
        KeepPosition="True"
        SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="CustObjects">
			    <Columns>
                    <px:PXGridColumn DataField="ShortName" Width="200px" LinkCommand="actionEdit" />
                    <px:PXGridColumn DataField="UserFriendlyType" Width="150px" />
                    <px:PXGridColumn DataField="Priority" Width="60px" MatrixMode="True" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	    <Mode AllowAddNew="False" />
        <ActionBar ActionsVisible="true" ActionsText="False">
            <Actions>
                <AddNew Enabled="false" MenuVisible="false" />
                <Delete Enabled="false" MenuVisible="false" />
            </Actions>
        </ActionBar>
	</px:PXGrid>

    <px:PXSmartPanel ID="PanelEditSql"
        runat="server"
        Key="SqlFilter"
        LoadOnDemand="true"
        Width="800px"
        Height="500px"
        Caption="Edit SQL Script"
        CaptionVisible="true"
        AutoRepaint="true"
        ShowMaximizeButton="true">
		<px:PXFormView runat="server"
            ID="FormEditSql"
            Width="100%"
			DataSourceID="ds" 
            DataMember="SqlFilter" 
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXPanel ID="PXPanelEditSql1" runat="server" RenderStyle="Simple" Style="padding-bottom:10px;">
                    <px:PXLayoutRule runat="server"/>
				    <px:PXTextEdit ID="edName"
                        runat="server" 
					    DataField="SqlName"
                        Size="XL"
                        TextAlign="Left"
                        CommitChanges="true"
                        SelectOnFocus="false"
                        DisableSpellcheck="True">
                    </px:PXTextEdit>
                    <px:PXNumberEdit ID="edPriority"
                        runat="server" 
					    DataField="Priority"
                        Size="XL"
                        TextAlign="Left"
                        CommitChanges="true"
                        SelectOnFocus="true"
                        DisableSpellcheck="True">
                    </px:PXNumberEdit>
                </px:PXPanel>
                <px:PXTextEdit ID="edContent"
                    runat="server"
                    DataField="SqlContent"
                    DisableSpellcheck="True"
                    Height="320px"
                    Width="100%"
                    TextAlign="Left"
                    TextMode="MultiLine"
                    SuppressLabel="True"
                    Wrap="False"
                    SelectOnFocus="false"
                    AllowNull="false">
                    <AutoSize Enabled="true"/>
                </px:PXTextEdit>
            </Template>
            <AutoSize Enabled="true"/>
		</px:PXFormView>
        <px:PXPanel ID="PXPanelEditSqlButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonAdd"
                runat="server"
                Text="SPECIFY DATABASE ENGINE"
                CommitChanges="true">
                <AutoCallBack Enabled="true"
                    Target="ds"
                    Command="actionAddSqlAttribute">
                    <Behavior RepaintControls="Bound" />
                </AutoCallBack>
            </px:PXButton>
			<px:PXButton ID="PXButtonOK"
                runat="server"
                DialogResult="OK"
                Text="OK"
                CommitChanges="true">
            </px:PXButton>
			<px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" CommitChanges="true" />
		</px:PXPanel>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelAddAttribute"
        runat="server"
        Key="AddAttributeFilter"
        LoadOnDemand="true"
        Caption="Specify Database Engine"
        CaptionVisible="true"
        AutoRepaint="true"
        AllowResize="false">
		<px:PXFormView runat="server"
            ID="FormAddAttribute"
            DataSourceID="ds" 
            DataMember="AddAttributeFilter" 
            Width="100%"
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="XS"/>
                <px:PXDropDown ID="edAddAttribute" runat="server" DataField="SqlAttribute" CommitChanges="true" />
            </Template>
            <AutoSize Enabled="false"/>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" CommitChanges="true" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" CommitChanges="true" />
		</px:PXPanel>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelAddCustomColumn"
        runat="server"
        Key="CustomColumnFilter"
        LoadOnDemand="true"
        Caption="Add Custom Column to Table"
        CaptionVisible="true"
        AutoRepaint="true"
        AllowResize="false">
		<px:PXFormView runat="server"
            ID="PXFormView1"
            DataSourceID="ds" 
            DataMember="CustomColumnFilter" 
            Width="100%"
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"/>
                <px:PXSelector ID="edTableName" runat="server" DataField="TableName" AutoRefresh="true"/>
                <px:PXTextEdit ID="edFieldName" runat="server" DataField="FieldName" DisableSpellcheck="true" SelectOnFocus="false"/>
                <px:PXDropDown ID="edDataType" runat="server" DataField="DataType" CommitChanges="true"/>
                <px:PXNumberEdit ID="edStringLength" runat="server" DataField="StringLength" SelectOnFocus="true" Width="100%"/>
                <px:PXNumberEdit ID="edDecimalPrecision" runat="server" DataField="DecimalPrecision" SelectOnFocus="true" Width="100%"/>
                <px:PXNumberEdit ID="edDecimalScale" runat="server" DataField="DecimalScale" SelectOnFocus="true" Width="100%"/>
            </Template>
            <AutoSize Enabled="false"/>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3"
                runat="server"
                DialogResult="OK"
                Text="OK"
                CommitChanges="true">
                <AutoCallBack Enabled="true"
                    Target="ds">
                    <Behavior RepaintControls="All"/>
                </AutoCallBack>
            </px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" CommitChanges="true"/>
		</px:PXPanel>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelEditCustomTable"
        runat="server"
        Key="ViewEditCustomTable"
        LoadOnDemand="true"
        Caption="Edit Table Columns"
        CaptionVisible="true"
        Width="640px"
        Height="480px"
        AutoRepaint="true"
        AllowResize="true"
        ShowMaximizeButton="true">
		<px:PXFormView runat="server"
            ID="PXFormView2"
            DataSourceID="ds" 
            DataMember="ViewEditCustomTable" 
            Width="100%"
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXGrid ID="GridEditCustomColumns"
                    runat="server"
                    Height="350"
                    Width="100%"
                    Style="z-index: 100"
                    AllowPaging="False"
                    AllowSearch="True"
                    AdjustPageSize="Auto"
                    DataSourceID="ds"
                    SkinID="Details"
                    TabIndex="900"
                    AutoAdjustColumns="True"
                    KeepPosition="True"
                    SyncPosition="True">
		            <Levels>
			            <px:PXGridLevel DataMember="ViewEditCustomTable">
			                <Columns>
                                <px:PXGridColumn DataField="FieldName" Width="200px"/>
                                <px:PXGridColumn DataField="ScriptType" Width="200px"/>
                                <px:PXGridColumn DataField="DataType" Width="200px"/>
                            </Columns>
			            </px:PXGridLevel>
		            </Levels>
		            <AutoSize Enabled="True"/>
                    <ActionBar ActionsVisible="true" ActionsText="False">
                        <Actions>
                            <Refresh Enabled="true" MenuVisible="true" GroupIndex="1" Order="1" ToolBarVisible="Top"/>
                            <AdjustColumns Enabled="true" MenuVisible="true" GroupIndex="1" Order="4" ToolBarVisible="Top"/>
                            <ExportExcel Enabled="true" MenuVisible="true" GroupIndex="1" Order="5" ToolBarVisible="Top"/>
                            <AddNew Enabled="false" MenuVisible="false" />
                            <Delete Enabled="false" MenuVisible="false" />
                        </Actions>
                        <CustomItems>
                            <px:PXToolBarButton Text="Add">
                                <ActionBar GroupIndex="1" Order="2" />                              
                                <MenuItems>
                                    <px:PXMenuItem Text="Add New Column">
                                            <AutoCallBack Command="actionAddNewCustomColumnToExistedTable" Enabled="true" Target="ds">
                                                <Behavior RepaintControls="Bound"/>
                                            </AutoCallBack>
                                    </px:PXMenuItem>
                                    <px:PXMenuItem Text="Column Length Increase">
                                       <AutoCallBack Command="actionIncreaseColumnLengthToExistedTable" Enabled="true" Target="ds">
                                                <Behavior RepaintControls="Bound"/>
                                            </AutoCallBack>
                                    </px:PXMenuItem>                                    
                                </MenuItems>                         
                            </px:PXToolBarButton>
                            <px:PXToolBarButton Text=""
                                ImageKey="RecordDel"
                                Tooltip="Delete Column"
                                CommandName="actionDeleteCustomColumnFromExistedTable"
                                CommandSourceID="ds"
                                DependOnGrid="GridEditCustomColumns">
                                <ActionBar GroupIndex="1" Order="3" />
                                <AutoCallBack Enabled="true"
                                    Target="ds">
                                    <Behavior RepaintControls="Bound"/>
                                </AutoCallBack>
                            </px:PXToolBarButton>
                        </CustomItems>
                    </ActionBar>
	            </px:PXGrid>
            </Template>
            <AutoSize Enabled="true"/>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton5"
                runat="server"
                DialogResult="OK"
                Text="Done"
                CommitChanges="true">
                <AutoCallBack Enabled="true"
                    Target="ds">
                    <Behavior RepaintControls="All"/>
                </AutoCallBack>
            </px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelAddNewTableSchema"
        runat="server"
        Key="CreateSchemaFilter"
        LoadOnDemand="true"
        Caption="Add Custom Table Schema"
        CaptionVisible="true"
        AutoRepaint="true"
        AllowResize="false">
		<px:PXFormView runat="server"
            ID="FormAddNewTableSchema"
            DataSourceID="ds" 
            DataMember="CreateSchemaFilter" 
            Width="100%"
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="XS"/>
                <px:PXSelector ID="edTableName" runat="server" DataField="TableName" CommitChanges="true" AutoRefresh="true" />
            </Template>
            <AutoSize Enabled="false"/>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton6"
                runat="server"
                DialogResult="OK"
                Text="OK"
                CommitChanges="true">
                <AutoCallBack Enabled="true"
                    Target="ds">
                    <Behavior RepaintControls="All"/>
                </AutoCallBack>
            </px:PXButton>
			<px:PXButton ID="PXButton7" runat="server" DialogResult="Cancel" Text="Cancel" CommitChanges="true" />
		</px:PXPanel>
	</px:PXSmartPanel>

     <px:PXSmartPanel ID="PanelIncreaseColumnLength"
        runat="server"
        Key="IncreaseColumnFilter"
        LoadOnDemand="true"
        Caption="Column Length Increase"
        CaptionVisible="true"
        AutoRepaint="true"
        AllowResize="false">
		<px:PXFormView runat="server"
            ID="PXFormView3"
            DataSourceID="ds" 
            DataMember="IncreaseColumnFilter" 
            Width="100%"
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"/>
                <px:PXSelector ID="edTableName" runat="server" DataField="TableName" AutoRefresh="true" CommitChanges="true"/>
                <px:PXDropDown ID="edFieldName" runat="server" DataField="FieldName" CommitChanges="true"/>            
                <px:PXTextEdit ID="edDataType" runat="server" DataField="DataType" CommitChanges="true"/>
                <px:PXTextEdit ID="edNewLength" runat="server" DataField="NewLength" CommitChanges="true"/>                  
            </Template>
            <AutoSize Enabled="false"/>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton8"
                runat="server"
                DialogResult="OK"
                Text="OK"
                CommitChanges="true">
                <AutoCallBack Enabled="true"
                    Target="ds">
                    <Behavior RepaintControls="All"/>
                </AutoCallBack>
            </px:PXButton>
			<px:PXButton ID="PXButton9" runat="server" DialogResult="Cancel" Text="Cancel" CommitChanges="true"/>
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>