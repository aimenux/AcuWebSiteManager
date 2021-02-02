<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS500300.aspx.cs" Inherits="Page_FS500300" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="Filter" 
        TypeName="PX.Objects.FS.ServiceContractInq">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="FixSchedulesWithoutNextExecutionDate" CommitChanges="True"/>
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="gridServiceContracts" 
                Name="ServiceContractSchedules_ViewDetails" ></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenScheduleScreenBySchedules" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenScheduleScreenByGenerationLogError" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="RollBackRun" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ClearAll" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenServiceContractScreenBySchedules" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenServiceContractScreenByGenerationLogError" Visible="False">
            </px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" TabIndex="1300" DefaultControlID="edCustomerID">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" GroupCaption="Filtering Option" StartGroup="True">
            </px:PXLayoutRule>
            <px:PXSegmentMask ID="edCustomerID" runat="server" CommitChanges="True" 
                DataField="CustomerID">
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edCustomerLocationID" runat="server" CommitChanges="True" 
                DataField="CustomerLocationID">
            </px:PXSegmentMask>
            <px:PXSelector ID="edBranchID" runat="server" CommitChanges="True" 
                DataField="BranchID">
            </px:PXSelector>
            <px:PXSelector ID="edBranchLocationID" runat="server" CommitChanges="True" 
                DataField="BranchLocationID"  AutoRefresh="True">
            </px:PXSelector>
            <px:PXLayoutRule runat="server"
                StartColumn="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" GroupCaption="Generation options" 
                StartGroup="True" LabelsWidth="SM" ControlSize="XM">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit runat="server" DataField="ToDate" ID="edToDate" CommitChanges="True">
            </px:PXDateTimeEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" 
        DataMember="Filter">
        <Items>
            <px:PXTabItem Text="Schedules">
                <Template>
                    <px:PXGrid ID="gridServiceContracts" runat="server" AllowPaging="True" AllowSearch="true"
                        DataSourceID="ds" Style="z-index: 100" 
		                Width="100%" Height="100%" SkinID="Inquire" TabIndex="500" 
                        AdjustPageSize="Auto" SyncPosition="True" KeepPosition="True" BatchUpdate="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceContractSchedules" DataKeyNames="CustomerID,RefNbr">
                                <RowTemplate>
                                    <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" 
                                        Text="Selected">
                                    </px:PXCheckBox>
                                    <px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="CustomerID" 
                                        AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edFSServiceContract__RefNbr" runat="server" 
                                        DataField="FSServiceContract__RefNbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edFSServiceContract__CustomerContractNbr" runat="server" 
                                        DataField="FSServiceContract__CustomerContractNbr" AllowEdit="True">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edFSServiceContract__DocDesc" runat="server" 
                                        DataField="FSServiceContract__DocDesc">
                                    </px:PXTextEdit>
                                    <px:PXSegmentMask ID="edFSServiceContract__CustomerLocationID" runat="server" 
                                        DataField="FSServiceContract__CustomerLocationID">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edRecurrenceDescription" runat="server" DataField="RecurrenceDescription">
                                    </px:PXTextEdit>
                                    <px:PXSelector ID="PXSelector1" runat="server" 
                                        DataField="RefNbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edCustomerLocationID" runat="server" 
                                        DataField="CustomerLocationID" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="PXDateTimeEdit1" runat="server" DataField="StartDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="PXDateTimeEdit2" runat="server" DataField="EndDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edLastGeneratedElementDate" runat="server" 
                                        DataField="LastGeneratedElementDate">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__BranchID">
                                    </px:PXGridColumn>        
                                    <px:PXGridColumn DataField="FSServiceContract__BranchLocationID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CustomerID" TextField="CustomerID_description">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__CustomerLocationID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__RefNbr" LinkCommand="OpenServiceContractScreenBySchedules">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__CustomerContractNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__DocDesc">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenScheduleScreenBySchedules">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CustomerLocationID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RecurrenceDescription">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduleGenType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="StartDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EndDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastGeneratedElementDate">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="View Details" Key="cmdItemDetails" Visible="False">
                                    <Images Normal="main@DataEntry" ></Images>
                                    <AutoCallBack Command="ServiceContractSchedules_ViewDetails" Target="ds" ></AutoCallBack> 
                                    <ActionBar GroupIndex="0" ></ActionBar>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Run History">
                <Template>
                    <px:PXGrid ID="gridContractGenerationHistory" runat="server" DataSourceID="ds" SkinID="Inquire"
                        Width="100%" Height="100%" TabIndex="-15736">
                        <Levels>
                            <px:PXGridLevel DataMember="ContractHistoryRecords" DataKeyNames="ContractGenerationHistoryID">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edGenerationID" runat="server" DataField="GenerationID">
                                    </px:PXNumberEdit>
                                    <px:PXDateTimeEdit ID="edLastGeneratedElementDate2" runat="server" 
                                        DataField="LastGeneratedElementDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edLastProcessedDate" runat="server" 
                                        DataField="LastProcessedDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" 
                                        DataField="LastModifiedDateTime">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="GenerationID" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastProcessedDate" TextAlign="Left">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastModifiedDateTime" TextAlign="Left">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Generation Error Log">
                <Template>
                    <px:PXGrid ID="gridGenerationLogError" runat="server" DataSourceID="ds" SkinID="Inquire"
                        Width="100%" Height="100%" TabIndex="-15736" AutoAdjustColumns="True" SyncPosition="True" KeepPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ErrorMessageRecords" DataKeyNames="LogID">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edGenerationID2" runat="server" DataField="GenerationID">
                                    </px:PXNumberEdit>
                                    <px:PXSegmentMask ID="edFSServiceContract__CustomerID" runat="server" AllowEdit="True" DataField="FSServiceContract__CustomerID">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edFSServiceContract__RefNbr2" runat="server" AllowEdit="True" DataField="FSServiceContract__RefNbr">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edFSSchedule__RefNbr" runat="server" AllowEdit="True" DataField="FSSchedule__RefNbr">
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="edErrorDate" runat="server" DataField="ErrorDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXTextEdit ID="edErrorMessage" runat="server" DataField="ErrorMessage">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="GenerationID" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__CustomerID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__RefNbr" LinkCommand="OpenServiceContractScreenByGenerationLogError">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSchedule__RefNbr" LinkCommand="OpenScheduleScreenByGenerationLogError">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ErrorDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ErrorMessage">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                        <ActionBar ActionsText="False">
                        <CustomItems>
                            <px:PXToolBarSeperator></px:PXToolBarSeperator>
                            <px:PXToolBarButton Tooltip="Clear all errors">
                                <AutoCallBack Command="ClearAll" Target="ds">
                                </AutoCallBack>
                            </px:PXToolBarButton>
                        </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>

</asp:Content>
