<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM205035.aspx.cs" Inherits="Page_SM205035" Title="Automation Schedule History" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:AUScheduleExecutionDataSource ID="ds" runat="server" TypeName="PX.SM.AUScheduleExecutionMaint" PrimaryView="Filter" Visible="True" Width="100%">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewScreen" DependOnGrid="grid" Visible="False"/>
            <px:PXDSCallbackCommand Name="ViewSchedule" DependOnGrid="grid" Visible="False"/>
            <px:PXDSCallbackCommand Name="ViewProcessed" DependOnGrid="grid" Visible="False"/>
            <px:PXDSCallbackCommand Name="ViewWarnings" DependOnGrid="grid" Visible="False"/>
            <px:PXDSCallbackCommand Name="ViewErrors" DependOnGrid="grid" Visible="False"/>
            <px:PXDSCallbackCommand Name="ViewTotal" DependOnGrid="grid" Visible="False"/>
            <px:PXDSCallbackCommand Name="ViewEntity" DependOnGrid="grid" Visible="False"/>
		</CallbackCommands>
	</px:AUScheduleExecutionDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Style="z-index: 100" Width="100%" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XS" ControlSize="XXL" StartColumn="True"/>
		    <px:PXSelector ID="edScreenID" runat="server" CommitChanges="True" DataField="ScreenID" AutoRefresh="True" DisplayMode="Text" FilterByAllFields="True"/>
            <px:PXSelector ID="edScheduleID" runat="server" CommitChanges="True" DataField="ScheduleID" AutoRefresh="True"/>

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XXS" ControlSize="SM"/>
            <px:PXDateTimeEdit ID="edDateFrom" runat="server" AlreadyLocalized="False" CommitChanges="True" DataField="DateFrom"/>
            <px:PXDateTimeEdit ID="edDateTo" runat="server" AlreadyLocalized="False" CommitChanges="True" DataField="DateTo"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SyncPosition="True" SkinID="PrimaryInquire" Style="z-index: 100" Width="100%" Height="150px" TabIndex="100" AllowFilter="True">
		<Levels>
			<px:PXGridLevel DataMember="Executions">
                <Mode AllowAddNew="False" AllowDelete="False" />
			    <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Width="30px" Type="CheckBox" AllowCheckAll="True" CommitChanges="True"/>
                    <px:PXGridColumn DataField="Status" TextAlign="Center" Width="50px" Type="Icon"/>
                    <px:PXGridColumn DataField="ScreenID" LinkCommand="ViewScreen" Width="96px"/>
                    <px:PXGridColumn DataField="ScheduleID" LinkCommand="ViewSchedule" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ExecutionDate" Width="90px"/>
                    <px:PXGridColumn DataField="TotalCount" LinkCommand="ViewTotal" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ProcessedCount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="WarningsCount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ErrorsCount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="Result"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar>
            <Actions>
                <AddNew ToolBarVisible="False" MenuVisible="False" />
                <Delete ToolBarVisible="False" MenuVisible="False" />
            </Actions>
        </ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>

    <px:PXSmartPanel ID="PanelHistories" runat="server" Width="900px" Height="550px" Key="HistoriesFilter" Caption="Processing Results" CaptionVisible="True"
        LoadOnDemand="True" AutoReload="True" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AutoCallBack-Command="Refresh" AutoCallBack-Target="form1">
        <px:PXFormView ID="form1" runat="server" DataSourceID="ds" DataMember="HistoriesFilter" Width="100%" Style="z-index: 100" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="S" StartColumn="True" />
                <px:PXNumberEdit runat="server" ID="edTabIndex" DataField="TabIndex" AllowNull="False" />
            </Template>
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
        </px:PXFormView>
        <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Width="100%" Style="height: 450px;" SkinID="PrimaryInquire" ActionsPosition="Top" AllowFilter="True"
            FilesIndicator="False" NoteIndicator="False" AllowPaging="True" AdjustPageSize="Auto" PreserveSortsAndFilters="False" SyncPosition="True" KeepPosition="True"
            AutoGenerateColumns="AppendDynamic" RepaintColumns="True" GenerateColumnsBeforeRepaint="True" MatrixMode="True">
            <Levels>
                <px:PXGridLevel DataMember="Histories">
                    <Columns>
                        <px:PXGridColumn DataField="ExecutionStatus" Width="70px" TextAlign="Center" Type="Icon"/>
                        <px:PXGridColumn DataField="ExecutionResult" Width="280px" />
                        <px:PXGridColumn DataField="_EntityLink_" LinkCommand="ViewEntity" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <Refresh ToolBarVisible="False" />
                    <FilterShow ToolBarVisible="False" />
                    <ExportExcel ToolBarVisible="False" />
                    <AdjustColumns ToolBarVisible="False" />
                </Actions>
            </ActionBar>
            <AutoSize Enabled="True" Container="Parent" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowRowSelect="True" />
        </px:PXGrid>
        <px:PXPanel ID="panel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="button1" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>

</asp:Content>
