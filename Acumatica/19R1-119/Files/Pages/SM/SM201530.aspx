<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM201530.aspx.cs" Inherits="Pages_SM_SM201530"
	EnableViewState="False" EnableViewStateMac="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        var taskManagerReloaderIntervalId = 0;

        function updateDataCallback()
        {
            var ds = px_alls['ds'];
            ds.executeCallback("actionUpdateData");

            
        }

        function updateChart(pxChart) {
            pxChart.fillDataSource(pxChart.data);
            pxChart.chart.dataProvider = pxChart.dataProvider;
            pxChart.chart.validateData();
        }

        function prepareCPUData(data, currentValue, maxLength) {
            if (data == null) {
                data = [];
                for (var i = 0; i < maxLength; i++) {
                    data[i] = ['', 0, ''];
                }
            }

            data[maxLength - 1][0] = '';
            data.shift();
            data[0][0] = (maxLength * 2) + ' sec';
            data.push(['0', parseInt(currentValue.substring(0, currentValue.length - 1)) , currentValue + ' - ' + new Date().toLocaleTimeString()]);

            return data;
        }

        function prepareMemoryData(data, currentValue, maxLength) {
            if (data == null) {
                data = [];
                for (var i = 0; i < maxLength; i++) {
                    data[i] = ['', 0, ''];
                }
            }

            data[maxLength - 1][0] = '';
            data.shift();
            data[0][0] = (maxLength * 2) + ' sec';
            data.push(['0', currentValue , currentValue + ' Mb - ' + new Date().toLocaleTimeString()]);

            return data;
        }

        var taskManagerReloader = {
            onReLoad: function() {
                var pxChart = px_alls["SerialChartCPU"];
                var reloadData = false;
                if (pxChart) {
                    pxChart.data = prepareCPUData(null, '0%', 30);
                    updateChart(pxChart);
                    reloadData = true;
                }

                pxChart = px_alls["SerialChartWorkingSet"];
                if (pxChart) {
                    pxChart.data = prepareMemoryData(null, 0, 30);
                    updateChart(pxChart);
                    reloadData = true;
                }

                if (reloadData) {
                    __px_callback(window).addHandler(Function.createDelegate(window, this.handleError));
                    taskManagerReloaderIntervalId = setInterval(updateDataCallback, 2000);
                }
            },
            handleError: function (context, error) {
                if (error) {
                    clearInterval(taskManagerReloaderIntervalId);
                } else {
                    var cpuUtilization = px_alls["CurrentUtilization"].getValue();
                    var workingSet = px_alls["WorkingSet"].getValue();
           
                    var pxChart = px_alls["SerialChartCPU"];
                    pxChart.data = prepareCPUData(pxChart.data, cpuUtilization, 30);
                    updateChart(pxChart);

                    pxChart = px_alls["SerialChartWorkingSet"];
                    pxChart.data = prepareMemoryData(pxChart.data, workingSet, 30);
                    updateChart(pxChart);
                }
            }
        };

        __px_cm(window).registerRequiresOnReLoad(taskManagerReloader);
    </script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.SM.TaskManager" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="actionStop" BlockPage="true" DependOnGrid="grid" CommitChanges="False" RepaintControls="All" Visible="False" />
		    <px:PXDSCallbackCommand Name="actionShow" DependOnGrid="grid" Visible="False" />
		    <px:PXDSCallbackCommand Name="actionStackTrace" Visible="False" CommitChanges="True" RepaintControls="All" />
		    <px:PXDSCallbackCommand Name="actionViewUser" CommitChanges="True" DependOnGrid="ActiveUsersGrid" Visible="False" />
		    <px:PXDSCallbackCommand Name="actionGC" Visible="False" />
		    <px:PXDSCallbackCommand Name="actionUpdateData" Visible="False" RepaintControls="None" RepaintControlsIDs="ResourceUsageForm" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="MainTabControl" runat="server" Width="100%">
        <Items>
            <px:PXTabItem Text="RUNNING PROCESSES" Visible="True">
                <Template>
                    <px:PXFormView runat="server" ID="RunningProcessesForm" Width="100%" DataSourceID="ds" AllowCollapse="False" DataMember="Filter">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M">
                            </px:PXLayoutRule>
                            <px:PXCheckBox ID="ShowAllUsers" runat="server" AlignLeft="True" DataField="ShowAllUsers"
                                           SuppressLabel="True" Text="Show All Users" CommitChanges="True">
                            </px:PXCheckBox>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" ID="grid" Width="100%" Height="100%" DataSourceID="ds" AutoGenerateColumns="Recreate" SkinID="Details" Caption="Operations">
                                <AutoSize Enabled="true" Container="Window" />
                                <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False"/>
                                <ActionBar PagerVisible="False" DefaultAction="buttonStop">
                                    <Actions>
                                        <AddNew Enabled="false" MenuVisible="false" />
                                        <Delete Enabled="false" MenuVisible="false" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton Text="ABORT" Key="buttonStop">
                                            <AutoCallBack Target="ds" Command="actionStop"></AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Text="VIEW SCREEN" Key="buttonShow">
                                            <AutoCallBack Target="ds" Command="actionShow"></AutoCallBack>
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton Text="ACTIVE THREADS" CommandSourceID="ds" >
                                            <AutoCallBack Target="ds" Enabled="True" Command="actionStackTrace" >
                                                <Behavior RepaintControls="All"></Behavior>
                                            </AutoCallBack>
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="Items">
                                        <Columns>
                                            <px:PXGridColumn DataField="User" Width="200px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Screen">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Title" Width="150px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Processed" TextAlign="Right" Width="100px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Total" TextAlign="Right" Width="100px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Errors" TextAlign="Right" Width="200px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="WorkTime" Width="100px">
                                            </px:PXGridColumn>
                                        </Columns>
                                        <Layout FormViewHeight=""></Layout>
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="ACTIVE USERS" Visible="True">
                <Template>
                    <px:PXFormView runat="server" ID="ActiveUsersForm" Width="100%" DataSourceID="ds" AllowCollapse="False" DataMember="Filter">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M">
                            </px:PXLayoutRule>
                            <px:PXDropDown ID="LoginTypeDropDown" runat="server" DataField="LoginType" Text="Login Type" ControlSize="XL" CommitChanges="True"/>  
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" ID="ActiveUsersGrid" Width="100%" Height="100%" DataSourceID="ds" AutoGenerateColumns="None" 
                               SkinID="Details" Caption="Active Users" SyncPosition="True">
                                <AutoSize Enabled="true" Container="Window" />
                                <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False"/>
                                <ActionBar PagerVisible="False" DefaultAction="buttonViewUser">
                                    <Actions>
                                        <AddNew Enabled="false" MenuVisible="false" />
                                        <Delete Enabled="false" MenuVisible="false" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton Text="VIEW USER" Key="buttonViewUser">
                                            <AutoCallBack Target="ds" Command="actionViewUser"></AutoCallBack>
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="ActiveUsers">
                                        <Columns>
                                            <px:PXGridColumn DataField="User" Width="200px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Company"  Width="200px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="LoginType"  Width="200px">
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="LastActivity" Width="200px" >
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="LoginTimeSpan"  Width="200px">
                                            </px:PXGridColumn>
                                        </Columns>
                                        <Layout FormViewHeight=""></Layout>
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="RESOURCE USAGE" Visible="True" >
                <Template>
                    <px:PXFormView runat="server" ID="ResourceUsageForm" Width="100%" DataMember="Filter" CaptionVisible="False">
                        <Template>
                            <px:PXLayoutRule ID="Column0" runat="server" StartColumn="True" GroupCaption="MEMORY USAGE" LabelsWidth="M" />
                            <px:PXTextEdit runat="server" DataField="GCTotalMemory" ID="GCTotalMemory"  />
                            <px:PXTextEdit runat="server" DataField="WorkingSet" ID="WorkingSet" />
                            <px:PXTextEdit runat="server" DataField="GCCollection" ID="GCCollection" />
                            <px:PXButton runat="server" Text="COLLECT MEMORY" AlignLeft="True" >
                                <AutoCallBack Target="ds" Enabled="True" Command="actionGC"></AutoCallBack>
                            </px:PXButton>
                            
                            <px:PXLayoutRule ID="Column1" runat="server" StartColumn="True" GroupCaption="CPU USAGE" LabelsWidth="M"/>
                            <px:PXTextEdit runat="server" DataField="CurrentUtilization" ID="CurrentUtilization" />
                            <px:PXTextEdit runat="server" DataField="UpTime" ID="UpTime"  />
                            <px:PXTextEdit runat="server" DataField="ActiveRequests" ID="ActiveRequests" />
                            <px:PXTextEdit runat="server" DataField="RequestsSumLastMinute" ID="RequestsSumLastMinute" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSerialChart ID="SerialChartCPU" runat="server" Width="100%" SkinID="Chart1" Height="300px" LegendEnabled="False" OnLoad="SerialChartCPU_OnLoad">
                        <Graphs>
                            <px:PXChartGraph LineColor="Black" Title="CPU"/>
                        </Graphs>
                        <DataFields Category="Category" Value="Values" Description="Labels"></DataFields>    
                        <CategoryAxis ShowFirstLabel="True" ShowLastLabel="True" LabelRotation="0" StartOnAxis="True"></CategoryAxis>
                    </px:PXSerialChart>
                    <px:PXSerialChart ID="SerialChartWorkingSet" runat="server" Width="100%" SkinID="Chart1" Height="300px" LegendEnabled="False" OnLoad="SerialChartWorkingSet_OnLoad">
                        <Graphs>
                            <px:PXChartGraph LineColor="Black" Title="Working set"/>
                        </Graphs>
                        <DataFields Category="Category" Value="Values" Description="Labels"></DataFields>                      
                        <CategoryAxis ShowFirstLabel="True" ShowLastLabel="True" LabelRotation="0" StartOnAxis="True"></CategoryAxis>
                    </px:PXSerialChart>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
	
    <px:PXSmartPanel runat="server" ID="pnlCurrentThreads" Width="600px" Height="500px" CaptionVisible="True"
                     Caption="Active Threads" ShowMaximizeButton="True" Key="CurrentThreadsPanel" ShowAfterLoad="true" AutoRepaint="True">
        <px:PXFormView ID="frmCurrentThreads" runat="server" SkinID="Transparent" DataMember="CurrentThreadsPanel">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit ID="PXCurrentThreadsText" runat="server" DataField="CurrentThreads" TextMode="MultiLine" SuppressLabel="true" Width="540px" Height="440px" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelCurrentThreadsButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnCurrentThreadsCancel" runat="server" DialogResult="Cancel" Text="Ok" />
        </px:PXPanel>
    </px:PXSmartPanel>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	
</asp:Content>
