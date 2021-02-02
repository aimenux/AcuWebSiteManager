<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM206000.aspx.cs"
    Inherits="Page_PM206000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.RateMaint" PrimaryView="RateSequence">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="RateSequence" Caption="Selection"
        EmailingGraph="" >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edRateTableID" runat="server" DataField="RateTableID" DataSourceID="ds" AutoCallBack="true" />
            <px:PXSelector ID="edRateTypeID" runat="server" DataField="RateTypeID" DataSourceID="ds" AutoCallBack="true"/>
            <px:PXDropDown ID="edSequence" runat="server" DataField="Sequence" AutoCallBack="true"/>
            <px:PXSelector ID="edRateCodeID" runat="server" DataField="RateCodeID" DataSourceID="ds" AutoCallBack="true" AutoRefresh="True"/>
			
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True" StartColumn="True" LabelsWidth="SM" ControlSize="XXL" />
			<px:PXTextEdit ID="PXSelector1" runat="server" DataField="Description" DataSourceID="ds"/>

            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
            <px:PXFormView ID="HiddenForm" runat="server" DataSourceID="ds" Caption="Hidden Form needed for VisibleExp of TabItems."
                Visible="False" DataMember="RateDefinition">
                <Activity HighlightColor="" SelectedColor="" />
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkProject" runat="server" DataField="Project" />
                    <px:PXCheckBox ID="chkEmployee" runat="server" DataField="Employee" />
                    <px:PXCheckBox ID="chkAccountGroup" runat="server" DataField="AccountGroup" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkTask" runat="server" DataField="Task" />
                    <px:PXCheckBox ID="chkRateItem" runat="server" DataField="RateItem" />
                    <px:PXLayoutRule runat="server" />
                </Template>
            </px:PXFormView>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="Items">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Rate" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridRates" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
                        AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Rates" DataKeyNames="RateDefinitionID,LineNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edRate" runat="server" DataField="Rate" />
                                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
                                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="StartDate" Label="StartDate" />
                                    <px:PXGridColumn DataField="EndDate" Label="EndDate" />
                                    <px:PXGridColumn DataField="Rate" Label="Rate" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Projects" BindingContext="HiddenForm" LoadOnDemand="False" VisibleExp="DataControls[&quot;chkProject&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="gridProjects" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
                        AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Projects" DataKeyNames="RateDefinitionID,ProjectCD">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edProjectCD" runat="server" DataField="ProjectCD" Wildcard="?" SelectMode="Segment" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ProjectCD" Label="Project" />
                                    <px:PXGridColumn DataField="PMProject__Description" Label="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tasks" BindingContext="HiddenForm" LoadOnDemand="False" VisibleExp="DataControls[&quot;chkTask&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="gridTasks" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
                        AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Tasks" DataKeyNames="RateDefinitionID,TaskCD">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edTaskCD" runat="server" DataField="TaskCD" Wildcard="?" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaskCD" Label="Project Task" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Account Groups" BindingContext="HiddenForm" LoadOnDemand="False" VisibleExp="DataControls[&quot;chkAccountGroup&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="gridAccountGroups" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
                        AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="AccountGroups" DataKeyNames="RateDefinitionID,AccountGroupID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edAccountGroupID" runat="server" DataField="AccountGroupID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Label="Account Group" />
                                    <px:PXGridColumn DataField="PMAccountGroup__Description" Label="PMAccountGroup-Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Inventory" BindingContext="HiddenForm" LoadOnDemand="False" VisibleExp="DataControls[&quot;chkRateItem&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="gridItems" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
                        AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Items" DataKeyNames="RateDefinitionID,InventoryID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" Label="Inventory ID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="InventoryItem__Descr" Label="InventoryItem-Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employee" BindingContext="HiddenForm" LoadOnDemand="False" VisibleExp="DataControls[&quot;chkEmployee&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="gridEmployee" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
                        AdjustPageSize="Auto" DataSourceID="ds" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Employees" DataKeyNames="RateDefinitionID,EmployeeID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="EmployeeID" Label="Employee ID" />
                                    <px:PXGridColumn DataField="BAccount__AcctName" Label="Employee-Employee Name" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
