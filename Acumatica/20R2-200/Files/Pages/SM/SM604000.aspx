<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM604000.aspx.cs" Inherits="Page_SM604000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds"
                     runat="server"
                     Visible="True"
                     Width="100%"
                     TypeName="PX.Data.Licensing.SM.SMLicenseManagment"
                     PrimaryView="MainInformationFilter">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Refresh" Visible="false" RepaintControls="Bound" />

            <px:PXDSCallbackCommand Name="actionRefreshManagementConsole" CommitChanges="True" RepaintControls="All" Visible="False" />
            <px:PXDSCallbackCommand Name="actionTransactionDetails" CommitChanges="True" RepaintControls="Bound" Visible="False" />
            <px:PXDSCallbackCommand Name="actionGenerateTestTrans" CommitChanges="True" RepaintControls="Bound" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>


<asp:Content ID="main" ContentPlaceHolderID="phf" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px">
        <Items>
            <px:PXTabItem Text="License" Visible ="True" RepaintOnDemand="False">
                <Template>
	                <px:PXFormView  ID="mainFormView"
                                    runat="server"
                                    DataSourceID="ds"
                                    DataMember="MainInformationFilter"
                                    Style="z-index: 100"
                                    Width="100%"
		                            Caption="License Monitoring Console"
                                    AllowCollapse="False">
		                <Template>
                            <px:PXCheckBox  ID="edBanner"
                                                runat="server"
                                                RenderStyle="Button"
                                                Enabled="False"
                                                ForceServerRendering="True"
                                                AlignLeft="True"
                                                SuppressLabel="True"
                                                DataField="Banner"
                                                Text="Limits of your license tier were exceeded. Contact your license provider to resolve the situation.">
                                <UncheckImages Normal="main@Fail" />
                            </px:PXCheckBox>
                            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XXL" ControlSize="S" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="L" ControlSize="M"/>
                            <px:PXTextEdit ID="edResourceLevel" runat="server" DataSourceID="ds" TextAlign="Right" DataField="ResourceLevel"/>

                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="License Details" />
                            <px:PXLayoutRule runat="server" LabelsWidth="XXL" ControlSize="S" />
                            <px:PXNumberEdit ID="edCommerceTransactionLimitMonthly" runat="server" DataSourceID="ds" DataField="CommerceTransactionLimitMonthly" />
                            <px:PXNumberEdit ID="edERPTransactionLimitMonthly" runat="server" DataSourceID="ds" DataField="ERPTransactionLimitMonthly" />
                            <px:PXNumberEdit ID="edDataIncluded" runat="server" DataSourceID="ds" DataField="DataIncluded" />

                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Recommended Maximums" />
                            <px:PXLayoutRule runat="server" LabelsWidth="XXL" ControlSize="S" />
                            <px:PXNumberEdit ID="edCommerceTransactionsLimitDaily" runat="server" DataSourceID="ds" DataField="CommerceTransactionsLimitDaily" />
                            <px:PXNumberEdit ID="edERPTransactionsLimitDaily" runat="server" DataSourceID="ds" DataField="ERPTransactionsLimitDaily" />
                            <px:PXNumberEdit ID="edmaximumRecommendedERPUsers" runat="server" DataSourceID="ds" DataField="maximumRecommendedERPUsers" />

                            <px:PXLayoutRule runat="server" StartColumn="True" />
                            <px:PXLabel runat="server" Text="" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="System Constraints" />
                            <px:PXLayoutRule runat="server" LabelsWidth="XXL" ControlSize="S" />
                            <px:PXNumberEdit ID="edWEBServicesAPIUsers" runat="server" DataSourceID="ds" DataField="WEBServicesAPIUsers" />
                            <px:PXNumberEdit ID="edWebServiceProcessingUnits" runat="server" DataSourceID="ds" DataField="WebServiceProcessingUnits" />
                            <px:PXNumberEdit ID="edWEBServicesAPIRequestsPerMinute" runat="server" DataSourceID="ds" DataField="WEBServicesAPIRequestsPerMinute"  />
                            <px:PXNumberEdit ID="edMaximumNumberOfFixedAssests" runat="server" DataSourceID="ds" DataField="MaximumNumberOfFixedAssests" />
                            <px:PXNumberEdit ID="edMaximumNumberOfInventoryItems" runat="server" DataSourceID="ds" DataField="MaximumNumberOfInventoryItems" />
                            <px:PXNumberEdit ID="edMaximumNumberOfBusinessAccounts" runat="server" DataSourceID="ds" DataField="MaximumNumberOfBusinessAccounts" />
                            <px:PXNumberEdit ID="edMaximumNumberOfLinesPerTransaction" runat="server" DataSourceID="ds" DataField="MaximumNumberOfLinesPerTransaction" />
                            <px:PXNumberEdit ID="edMaximumNumberOfSerialNumbersPerDocument" runat="server" DataSourceID="ds" DataField="MaximumNumberOfSerialNumbersPerDocument" />
							<px:PXNumberEdit ID="edMaximumNumberOfPayrollEmployees" runat="server" DataSourceID="ds" DataField="MaximumNumberOfPayrollEmployees" />
                            <px:PXNumberEdit ID="edmaximumNumberOfStaffMembersAndVehicles" runat="server" DataSourceID="ds" DataField="maximumNumberOfStaffMembersAndVehicles" />
                            <px:PXNumberEdit ID="edMaximumNumberOfAppointmentsPerMonth" runat="server" DataSourceID="ds" DataField="MaximumNumberOfAppointmentsPerMonth" />

		                </Template>
	                </px:PXFormView>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Statistics" Visible ="True">
                <Template>
                    <px:PXSplitContainer    runat="server"
                                            SplitterPosition="250"
						                    Orientation="Horizontal"
                                            Width="100%"
						                    SkinID="Horizontal">
                        <Template1>
                            <px:PXGrid ID="gridMonthly"
                                Caption="Monthly"
                                runat="server"
                                Height="150px"
                                Width="100%"
                                Style="z-index: 100"
                                AllowPaging="True"
                                AllowSearch="True"
                                AutoAdjustColumns="True"
                                AdjustPageSize="Auto"
                                DataSourceID="ds"
                                SkinID="Inquire"
                                SyncPosition="true">
		                        <Levels>
			                        <px:PXGridLevel DataMember="LicenseStatisticMonthly">
			                            <Columns>
                                            <px:PXGridColumn Width="100px" DataField="MonthYearUserFriendly" />
                                            <px:PXGridColumn Width="500px" DataField="CommerceTransacCountPercFromLimit" LinkCommand="actionMonthlyCommercialTransactionDetails"/>
                                            <px:PXGridColumn Width="300px" DataField="ERPTransacCountPercFromLimit" LinkCommand="actionMonthlyERPTransactionDetails" />
                                            <%--<px:PXGridColumn Width="300px" DataField="DaysInDataLoadMode" />--%>
                                        </Columns>
			                        </px:PXGridLevel>
		                        </Levels>
                                <AutoSize Enabled="True" MinHeight="200" />
                                <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpload="False" />
                                <ActionBar ActionsVisible="true" ActionsText="False">
                                    <Actions>
                                        <Refresh Enabled="false" MenuVisible="false"/>
                                        <AddNew Enabled="false" MenuVisible="false" />
                                        <Delete Enabled="false" MenuVisible="false" />
                                    </Actions>
                                </ActionBar>
                                <AutoCallBack   Enabled="true"
                                                Target="dailyGrid"
                                                Command="Refresh">
                                    <Behavior RepaintControlsIDs="gridDaily" />
                                </AutoCallBack>
	                        </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridDaily"
                                Caption="Daily"
                                runat="server"
                                Height="400px"
                                Width="100%"
                                Style="z-index: 100"
                                AllowPaging="True"
                                AllowSearch="True"
                                AutoAdjustColumns="True"
                                AdjustPageSize="Auto"
                                DataSourceID="ds"
                                AutoRepaint="True"
                                SkinID="Inquire"
                                SyncPosition="true">
		                        <Levels>
			                        <px:PXGridLevel DataMember="LicenseStatisticDaily">
			                            <Columns>
                                            <px:PXGridColumn Width="100px" DataField="Date" />
                                            <px:PXGridColumn Width="350px" DataField="CommerceTransacCountPercFromLimit" LinkCommand="actionDailyCommercialTransactionDetails" />
                                            <px:PXGridColumn Width="250px" DataField="ERPTransacCountPercFromLimit" LinkCommand="actionDailyERPTransactionDetails" />
                                            <px:PXGridColumn Width="200px" DataField="MaximumAPIRequestsPerMinute" />
                                            <px:PXGridColumn Width="200px" DataField="APIRequestsDeclinedStats" />
			                                <px:PXGridColumn Width="200px" DataField="APIRequestsThrottledStats" />
                                        </Columns>
			                        </px:PXGridLevel>
		                        </Levels>
		                        <AutoSize Enabled="True" MinHeight="200" />
                                <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpload="False" />
                                <ActionBar ActionsVisible="true" ActionsText="False">
                                    <Actions>
                                        <Refresh Enabled="false" MenuVisible="false"/>
										<AddNew Enabled="false" MenuVisible="false" />
                                        <Delete Enabled="false" MenuVisible="false" />
                                    </Actions>
                                </ActionBar>
	                        </px:PXGrid>
                        </Template2>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Warnings" Visible ="True">
                <Template>
                    <px:PXGrid  ID="violationHistory"
                                Caption="Violation History"
                                runat="server"
                                Height="400px"
                                Width="100%"
                                Style="z-index: 100"
                                AllowPaging="True"
                                AllowSearch="True"
                                AutoAdjustColumns="True"
                                AdjustPageSize="Auto"
                                DataSourceID="ds"
                                SkinID="Details">
		                <Levels>
			                <px:PXGridLevel DataMember="Violations">
			                    <Columns>
                                    <px:PXGridColumn Width="75px"   DataField="StatusUserFriendly" />
                                    <px:PXGridColumn Width="100px"  DataField="Date" />
                                    <px:PXGridColumn Width="150px"  DataField="LimitTypeUserFriendly" />
                                    <px:PXGridColumn Width="200px"  DataField="TranTypeUserFriendly" />
                                    <%--<px:PXGridColumn Width="150px"  DataField="TranCount" />--%>
                                    <px:PXGridColumn Width="150px"  DataField="Limit" />
                                    <%--<px:PXGridColumn Width="150px"  DataField="OverchargeRate" />--%>
                                    <px:PXGridColumn Width="100px"  DataField="CloseDate" />
                                    <px:PXGridColumn Width="400px"  DataField="Reason"/>
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" MinHeight="200" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpload="False" />
                        <ActionBar ActionsVisible="true" ActionsText="False">
                            <Actions>
                                <Refresh Enabled="false" MenuVisible="false"/>
                                <AddNew Enabled="false" MenuVisible="false" />
                                <Delete Enabled="false" MenuVisible="false" />
                            </Actions>
                        </ActionBar>
	                </px:PXGrid>
                </Template>
            </px:PXTabItem>

			<px:PXTabItem Text="Constraint History" Visible ="True" RepaintOnDemand="False">
                <Template>
					<px:PXGrid  ID="gridConstraintHistory"
								runat="server"
								Width="100%"
								Style="z-index: 100"
								AllowPaging="True"
								AllowSearch="True"
								AutoAdjustColumns="True"
								AdjustPageSize="Auto"
								DataSourceID="ds"
								SkinID="Details">
								<Levels>
									<px:PXGridLevel DataMember="ConstraintHistory">
										<Columns>
											<px:PXGridColumn Width="100px" DataField="Date" />
											<px:PXGridColumn Width="100px" DataField="CompanyID" />
											<px:PXGridColumn Width="200px" DataField="NbrOfFixedAssets" />
											<px:PXGridColumn Width="200px" DataField="NbrOfInventoryItems" />
											<px:PXGridColumn Width="200px" DataField="NbrOfBusinessAccounts" />
                                            <px:PXGridColumn Width="200px" DataField="NbrOfPayrollEmployees" />
                                            <px:PXGridColumn Width="200px" DataField="NbrOfStaffAndVehicles" />
											<px:PXGridColumn Width="200px" DataField="NbrOfAppointments" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinHeight="200" />
								<Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpload="False" />
								<ActionBar ActionsVisible="true" ActionsText="False">
									<Actions>
                                        <Refresh Enabled="false" MenuVisible="false"/>
										<AddNew Enabled="false" MenuVisible="false" />
										<Delete Enabled="false" MenuVisible="false" />
									</Actions>
								</ActionBar>
					</px:PXGrid>
				</Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXTab>


    <px:PXSmartPanel ID="panelTransactionDetails"
        runat="server"
        Key="TransactionDetailsFilter"
        LoadOnDemand="true"
        Caption="Transaction Details"
        CaptionVisible="true"
        AutoReload="True"
        AutoRepaint="true"
        AllowResize="false"
        Width="900px"
                     ShowCloseButton="False" 
        Height="500px">
		<px:PXFormView runat="server"
            ID="formTransactionDetails"
            DataSourceID="ds" 
            DataMember="TransactionDetailsFilter" 
            Width="100%"
			SkinID="Transparent"
            CommitChanges="true">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                <px:PXDropDown ID="PXDropDown1" runat="server" DataField="TransactionPeriodType" CommitChanges="True" />
                <px:PXDropDown ID="PXDropDown2" runat="server" DataField="TransactionType" CommitChanges="True" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S"/>
                <px:PXDateTimeEdit runat="server" ID="PXDateTimeEdit1" DataField="DateFrom" CommitChanges="True"></px:PXDateTimeEdit>
                <px:PXDateTimeEdit runat="server" ID="PXDateTimeEdit2" DataField="DateTo" CommitChanges="True"></px:PXDateTimeEdit>
            </Template>
           
            
		</px:PXFormView>
        <px:PXGrid  ID="gridCommerceTransactionDetails"
                    runat="server"
                    Width="100%"
                    Style="z-index: 100"
                    AllowPaging="False"
                    AllowSearch="True"
                    AutoAdjustColumns="True"
                    AdjustPageSize="Auto"
                    DataSourceID="ds"
                    SkinID="Details">
            <Levels>
                <px:PXGridLevel DataMember="TransactionDetails">
                    <Columns>
                        <px:PXGridColumn Width="200px" DataField="TransactionPeriod" />
                        <px:PXGridColumn Width="200px" DataField="TransactionScreen" DisplayMode="Text" />
                        <px:PXGridColumn Width="200px" DataField="TransactionSource" />
                        <px:PXGridColumn Width="150px" DataField="TransactionCount" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="200" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpload="False" />
            <ActionBar ActionsVisible="true" ActionsText="False">
                <Actions>
                    <AddNew Enabled="false" MenuVisible="false" />
                    <Delete Enabled="false" MenuVisible="false" />
                </Actions>
            </ActionBar>
            <AutoSize Enabled="True" MinHeight="200" />
        </px:PXGrid>
        
        <px:PXPanel ID="panelButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="buttonOK" runat="server" DialogResult="OK" Text="OK" CommitChanges="true" />
		</px:PXPanel>
	</px:PXSmartPanel>


</asp:Content>
