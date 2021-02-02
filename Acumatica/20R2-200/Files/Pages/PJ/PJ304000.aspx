<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ304000.aspx.cs" Inherits="Page_PJ304000" Title="DailyFieldReport" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" EnableAttributes="true" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.DailyFieldReports.PJ.Graphs.DailyFieldReportEntry" PrimaryView="DailyFieldReport">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="approve"/>
            <px:PXDSCallbackCommand Name="reject"/>
            <px:PXDSCallbackCommand Name="ViewTimeCard" Visible="False" DependOnGrid="gridEmployeeActivities" />
            <px:PXDSCallbackCommand Name="ViewChangeRequest" Visible="False" DependOnGrid="gridChangeRequests" />
            <px:PXDSCallbackCommand Name="ViewChangeOrder" Visible="False" DependOnGrid="gridChangeOrders" />
            <px:PXDSCallbackCommand Name="CreateChangeRequest" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateChangeOrder" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewProjectIssue" Visible="False" DependOnGrid="gridProjectIssues" />
            <px:PXDSCallbackCommand Name="ViewEquipmentTimeCard" Visible="False" DependOnGrid="gridEquipment" />
            <px:PXDSCallbackCommand Name="CreateProjectIssue" Visible="False" />
            <px:PXDSCallbackCommand Name="CreatePhotoLog" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewPhotoLog" Visible="False" DependOnGrid="gridPhotoLogs" />
            <px:PXDSCallbackCommand Name="ViewPurchaseReceipt" Visible="False" DependOnGrid="gridMaterials" />
            <px:PXDSCallbackCommand Name="CreateNewPurchaseReceipt" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateNewPurchaseReturn" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewExpenseClaim" Visible="False" DependOnGrid="gridEmployeeExpenses" />
            <px:PXDSCallbackCommand Name="ViewExpenseReceipt" Visible="False" DependOnGrid="gridEmployeeExpenses" />
            <px:PXDSCallbackCommand Name="CreateExpenseReceipt" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewAttachedReport" Visible="False" DependOnGrid="gridHistory" />
            <px:PXDSCallbackCommand Name="ViewAddressOnMap" Visible="False"  />
            <px:PXDSCallbackCommand Name="LoadWeatherConditions" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="DailyFieldReportContent" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="DailyFieldReport"
        Caption="Daily Field Report" NoteIndicator="True" FilesIndicator="True" DefaultControlID="edDailyFieldReportCd">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edDailyFieldReportCd" runat="server" DataField="DailyFieldReportCd"
                FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True" AllowNull="False"/>
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="True" SuppressLabel="True" />
            <px:PXDateTimeEdit ID="edDate" runat="server" AlreadyLocalized="False" DataField="Date" Size="S"/>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="True" />
            <px:PXSelector ID="edProjectManagerId" runat="server" DataField="ProjectManagerId" CommitChanges="True" />
            <px:PXSelector ID="edCreatedById" runat="server" DataField="CreatedById" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXTextEdit ID="edSiteAddress" runat="server" DataField="SiteAddress" CommitChanges="True"/>
            <px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="True" />
            <px:PXSelector ID="edCountryId" runat="server" DataField="CountryId" CommitChanges="True" />
            <px:PXSelector ID="edState" runat="server" DataField="State" CommitChanges="True" AutoRefresh="True" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXMaskEdit ID="edPostalCode" runat="server" Size="S" CommitChanges="True" DataField="PostalCode" />
            <px:PXButton ID="btnViewAddressOnMap" runat="server" Text="View on Map" CommandSourceID="ds" CommandName="ViewAddressOnMap" />
            <px:PXLayoutRule runat="server" />
            <px:PXNumberEdit ID="edLatitude" runat="server"  DataField="Latitude" CommitChanges="True" AllowNull="True"/>
            <px:PXNumberEdit ID="edLongitude" runat="server" DataField="Longitude" CommitChanges="True" AllowNull="True" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXNumberEdit ID="edTemperatureLevel" runat="server" DataField="TemperatureLevel" />
            <px:PXNumberEdit ID="edDFRHumidity" runat="server" DataField="Humidity" />
            <px:PXImageView ID="edIcon" runat="server" DataField="Icon" Style="position: absolute; height: 100px; max-width: 100px; 
                background-color: #FFFFFF" />
            <px:PXDateTimeEdit ID="edTime" runat="server" DataField="TimeObserved_Time" TimeMode="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="DailyFieldReportTabsContent" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%">
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Items>
             <px:PXTabItem Text="Labor Time And Activities" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridEmployeeActivities" runat="server" DataSourceID="ds" Style="z-index: 100" 
                        Width="100%" Height="150px" SkinID="DetailsInTab" TabIndex="700" NoteIndicator="True"
                               SyncPosition="True" AdjustPageSize="Auto" AllowPaging="True" >
                        <AutoSize Container="Window" Enabled="True" MinHeight="150"/>
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataKeyNames="NoteID" DataMember="EmployeeActivities">
                                <RowTemplate>
                                    <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="true" DisplayMode="Text"/>
                                    <px:PXSelector ID="edEarningTypeID" runat="server" DataField="EarningTypeID" AutoRefresh="true" />
                                    <px:PXDateTimeEdit ID="edDate_Date" runat="server" DataField="Date_Date" CommitChanges="True" AutoRefresh="true" />
                                    <px:PXDateTimeEdit ID="edDate_Time" TimeMode="True" runat="server" DataField="Date_Time" AutoRefresh="true" />
                                    <px:PXSelector ID="edParentTaskNoteID" runat="server" DataField="ParentTaskNoteID" AllowEdit="True"
                                        DisplayMode="Text" TextMode="Search" TextField="Subject" AutoRefresh="true" />
                                    <px:PXTimeSpan ID="edTimeSpent" TimeMode="True" runat="server" DataField="TimeSpent" InputMask="hh:mm" AutoRefresh="true" />
                                    <px:PXTimeSpan ID="edTimeBillable" TimeMode="True" runat="server" DataField="TimeBillable"
                                        InputMask="hh:mm" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" DataSourceID="ds" 
                                        CommitChanges="True" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edLabourItemID" runat="server" DataField="LabourItemID" AutoRefresh="true" />
                                    <px:PXSelector ID="edLastModifiedByIdEmployee" runat="server" DataField="LastModifiedById" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OwnerID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="OwnerID_description" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="EarningTypeID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="Date_Time" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TimeSpent" AutoCallBack="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Summary" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LastModifiedById" AutoCallBack="True" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="LastModifiedDateTime" AutoCallBack="True" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="TimeCardCD" LinkCommand="ViewTimeCard" />
                                    <px:PXGridColumn DataField="Hold" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ApprovalStatus" />
                                    <px:PXGridColumn DataField="Date_Date" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ParentTaskNoteID" AutoCallBack="True" TextField="Summary" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="CertifiedJob" Type="CheckBox" />
                                    <px:PXGridColumn DataField="UnionID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="LabourItemID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="WorkCodeID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ApproverID" />
                                    <px:PXGridColumn DataField="ContractID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="RefNoteID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Change Requests" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridChangeRequests" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" NoteIndicator="false" FilesIndicator="false" >
                        <AutoSize Container="Window" Enabled="True" MinHeight="150"/>
                        <Levels>
                            <px:PXGridLevel DataMember="ChangeRequests">
                                <RowTemplate>
                                    <px:PXSelector ID="edChangeRequestId" runat="server" DataField="ChangeRequestId" AutoRefresh="True" CommitChanges="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ChangeRequestId" TextField="PMChangeRequest__RefNbr" CommitChanges="True" LinkCommand="ViewChangeRequest"/>
                                    <px:PXGridColumn DataField="PMChangeRequest__Date" />
                                    <px:PXGridColumn DataField="PMChangeRequest__ExtRefNbr" />
                                    <px:PXGridColumn DataField="PMChangeRequest__Description" />
                                    <px:PXGridColumn DataField="PMChangeRequest__Status" />
                                    <px:PXGridColumn DataField="PMChangeRequest__CostTotal" />
                                    <px:PXGridColumn DataField="PMChangeRequest__LineTotal" />
                                    <px:PXGridColumn DataField="PMChangeRequest__MarkupTotal" />
                                    <px:PXGridColumn DataField="PMChangeRequest__PriceTotal" />
                                    <px:PXGridColumn DataField="PMChangeRequest__LastModifiedByID" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="PMChangeRequest__LastModifiedDateTime" DisplayFormat="g"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="CreateChangeRequest" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                            <Actions>
                                <Delete Tooltip="Unlink Change Request" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Change Orders" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridChangeOrders" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" NoteIndicator="false" FilesIndicator="false" >
                        <AutoSize Container="Window" Enabled="True" MinHeight="150"/>
                        <Levels>
                            <px:PXGridLevel DataMember="ChangeOrders">
                                <RowTemplate>
                                    <px:PXSelector ID="edChangeOrderId" runat="server" DataField="ChangeOrderId" AutoRefresh="True" CommitChanges="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ChangeOrderId" TextField="PMChangeOrder__RefNbr" CommitChanges="True" LinkCommand="ViewChangeOrder" />
                                    <px:PXGridColumn DataField="PMChangeOrder__ClassID" />
                                    <px:PXGridColumn DataField="PMChangeOrder__CustomerID" />
                                    <px:PXGridColumn DataField="Customer__AcctName" />
                                    <px:PXGridColumn DataField="PMChangeOrder__DelayDays" />
                                    <px:PXGridColumn DataField="PMChangeOrder__ProjectNbr" />
                                    <px:PXGridColumn DataField="PMChangeOrder__ExtRefNbr" />
                                    <px:PXGridColumn DataField="PMChangeOrder__Description" />
                                    <px:PXGridColumn DataField="PMChangeOrder__Status" />
                                    <px:PXGridColumn DataField="PMChangeOrder__RevenueTotal" />
                                    <px:PXGridColumn DataField="PMChangeOrder__CommitmentTotal" />
                                    <px:PXGridColumn DataField="PMChangeOrder__CostTotal" />
                                    <px:PXGridColumn DataField="PMChangeOrder__LastModifiedByID" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="PMChangeOrder__LastModifiedDateTime" DisplayFormat="g" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="CreateChangeOrder" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                            <Actions>
                                <Delete Tooltip="Unlink Change Order" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Subcontractors" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridSubcontractors" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" NoteIndicator="True" FilesIndicator="true">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Subcontractors">
                                <RowTemplate>
                                    <px:PXSelector ID="edVendorId" runat="server" DataField="VendorId" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSelector ID="edCostCodeId" runat="server" DataField="CostCodeId" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edTimeArrived_Time" runat="server" DataField="TimeArrived_Time" TimeMode="True" AutoRefresh="True" />
                                    <px:PXDateTimeEdit ID="edTimeDeparted_Time" runat="server" DataField="TimeDeparted_Time" TimeMode="True" AutoRefresh="True" />
                                    <px:PXTimeSpan ID="edWorkingTimeSpent" runat="server" DataField="WorkingTimeSpent" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="edTotalWorkingTimeSpent" runat="server" DataField="TotalWorkingTimeSpent" 
                                        InputMask="hh:mm" SummaryMode="true" />
                                    <px:PXSelector ID="edLastModifiedByIdSubcontractors" runat="server" DataField="LastModifiedById" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="VendorId" CommitChanges="True" LinkCommand="ViewEntity" />
                                    <px:PXGridColumn DataField="Vendor__AcctName" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AutoCallBack="true" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeId" AutoCallBack="true" CommitChanges="True" />
                                    <px:PXGridColumn DataField="NumberOfWorkers" AutoCallBack="true" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TimeArrived_Time" AutoCallBack="true" RenderEditorText="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TimeDeparted_Time" AutoCallBack="true" RenderEditorText="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="WorkingTimeSpent" AutoCallBack="true" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TotalWorkingTimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Description" AutoCallBack="true" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LastModifiedById" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="LastModifiedDateTime" DisplayFormat="g" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Project Issues" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridProjectIssues" runat="server" DataSourceID="ds" Width="100%" SkinID="Details"
                               NoteIndicator="false" FilesIndicator="false" SyncPosition="True">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="ProjectIssues">
                                <RowTemplate>
                                    <px:PXSelector ID="edProjectIssueId" runat="server" DataField="ProjectIssueId"
                                        AutoRefresh="True" DisplayMode="Text" CommitChanges="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ProjectIssueId" TextField="ProjectIssue__ProjectIssueCd"
                                        CommitChanges="True" LinkCommand="ViewProjectIssue" />
                                    <px:PXGridColumn DataField="ProjectIssue__Summary" />
                                    <px:PXGridColumn DataField="ProjectIssue__Status" />
                                    <px:PXGridColumn DataField="ProjectIssue__PriorityId" />
                                    <px:PXGridColumn DataField="ProjectIssue__ProjectTaskId" />
                                    <px:PXGridColumn DataField="ProjectIssue__ProjectIssueTypeId" />
                                    <px:PXGridColumn DataField="ProjectIssue__LastModifiedByID" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="ProjectIssue__LastModifiedDateTime" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="ProjectIssue__CreationDate" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectIssue__DueDate" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectIssue__OwnerID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectIssue__WorkgroupID" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="CreateProjectIssue" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                            <Actions>
                                <Delete Tooltip="Unlink Project Issue" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Photo Logs" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXSplitContainer runat="server" SplitterPosition="1100" Orientation="Vertical" Height="100%" Width="100%">
                        <AutoSize Enabled="True" Container="Window" />
                        <Template1>
                            <px:PXGrid ID="gridPhotoLogs" runat="server" DataSourceID="ds" Width="100%" SkinID="Details"
                                   NoteIndicator="false" FilesIndicator="false" SyncPosition="True">
                                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                                <AutoCallBack Target="mainPhoto" Command="Refresh" ActiveBehavior="True">
                                    <Behavior CommitChanges="True" RepaintControlsIDs="mainPhoto" />
                                </AutoCallBack>
                                <Levels>
                                    <px:PXGridLevel DataMember="PhotoLogs">
                                        <RowTemplate>
                                            <px:PXSelector ID="edPhotoLogId" runat="server" DataField="PhotoLogId"
                                                AutoRefresh="True" DisplayMode="Text" CommitChanges="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="PhotoLogId" TextField="PhotoLog__PhotoLogCd"
                                                CommitChanges="True" LinkCommand="ViewPhotoLog" />
                                            <px:PXGridColumn DataField="PhotoLog__StatusId" />
                                            <px:PXGridColumn DataField="PhotoLog__Date" />
                                            <px:PXGridColumn DataField="PhotoLog__ProjectTaskId" />
                                            <px:PXGridColumn DataField="PhotoLog__Description" />
                                            <px:PXGridColumn DataField="PhotoLog__CreatedById" DisplayMode="Hint" />
                                            <px:PXGridColumn DataField="PhotoLog__LastModifiedByID" DisplayMode="Hint" />
                                            <px:PXGridColumn DataField="PhotoLog__LastModifiedDateTime" DisplayFormat="g" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <ActionBar>
                                    <CustomItems>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="CreatePhotoLog" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                    <Actions>
                                        <Delete Tooltip="Unlink Photo Log" />
                                    </Actions>
                                </ActionBar>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                           <px:PXFormView ID="mainPhoto" runat="server" DataSourceID="ds" Width="100%" DataMember="MainPhoto">
                               <AutoSize Container="Window" Enabled="True" />
                               <Template>
                                   <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"
                                       GroupCaption="Main Photo Preview" />
                                   <px:PXImageView ID="edImageUrl" runat="server" DataField="ImageUrl"
                                       Style="position: absolute; max-height: 550px; max-width: 600px;" />
                               </Template>
                           </px:PXFormView>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Notes" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridNotes" runat="server" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" 
                               NoteIndicator="True" FilesIndicator="true">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Notes">
                                 <RowTemplate>
                                    <px:PXTimeSpan ID="edTime_Time" TimeMode="True" runat="server" DataField="Time" InputMask="hh:mm" AutoRefresh="true"/>
                                    <px:PXSelector ID="edLastModifiedByIdNote" runat="server" DataField="LastModifiedById" />
                                 </RowTemplate>
                                <Columns>
                                  <px:PXGridColumn DataField="Time_Time" AutoCallBack="True" RenderEditorText="True" />
                                  <px:PXGridColumn DataField="Description" />
                                  <px:PXGridColumn DataField="LastModifiedById" DisplayMode="Hint" />
                                  <px:PXGridColumn DataField="LastModifiedDateTime" DisplayFormat="g" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
             </px:PXTabItem>
             <px:PXTabItem Text="Materials" BindingContext="form" RepaintOnDemand="false">
                 <Template>
                     <px:PXGrid ID="gridMaterials" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" 
                                NoteIndicator="True" FilesIndicator="true" SyncPosition="True">
                         <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                         <Levels>
                             <px:PXGridLevel DataMember="PurchaseReceipts">
                                 <RowTemplate>
                                     <px:PXSelector ID="edPurchaseReceiptId" runat="server" DataField="PurchaseReceiptId" CommitChanges="True" AutoRefresh="True" />
                                 </RowTemplate>
                                 <Columns>
                                     <px:PXGridColumn DataField="PurchaseReceiptId" TextField="POReceipt__ReceiptNbr"
                                         CommitChanges="True" LinkCommand="ViewPurchaseReceipt" />
                                     <px:PXGridColumn DataField="POReceipt__ReceiptType" />
                                     <px:PXGridColumn DataField="POReceipt__Status" />
                                     <px:PXGridColumn DataField="POReceipt__VendorID" />
                                     <px:PXGridColumn DataField="Vendor__AcctName" />
                                     <px:PXGridColumn DataField="POReceipt__OrderQty" />
                                     <px:PXGridColumn DataField="POReceipt__LastModifiedByID" DisplayMode="Hint" />
                                     <px:PXGridColumn DataField="POReceipt__LastModifiedDateTime" DisplayFormat="g" />
                                 </Columns>
                             </px:PXGridLevel>
                         </Levels>
                         <ActionBar>
                             <CustomItems>
                                 <px:PXToolBarButton>
                                     <AutoCallBack Command="CreateNewPurchaseReceipt" Target="ds" />
                                 </px:PXToolBarButton>
                                 <px:PXToolBarButton>
                                     <AutoCallBack Command="CreateNewPurchaseReturn" Target="ds" />
                                 </px:PXToolBarButton>
                             </CustomItems>
                             <Actions>
                                 <Delete Tooltip="Unlink Purchase Receipt" />
                             </Actions>
                         </ActionBar>
                     </px:PXGrid>
                 </Template>
             </px:PXTabItem>
            <px:PXTabItem Text="Equipment" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridEquipment" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" 
                               NoteIndicator="True" FilesIndicator="true">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Equipment">
                                <RowTemplate>
                                    <px:PXSelector ID="edEquipmentId" runat="server" DataField="EquipmentId" CommitChanges="true" AutoCallBack="true"/>
                                    <px:PXTextEdit ID="edEquipmentDescription" runat="server" DataField="EquipmentDescription" CommitChanges="true" AutoCallBack="true"/>
                                    <px:PXSegmentMask ID="edEquipmentProjectTaskId" runat="server" DataField="ProjectTaskID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edEquipmentCostCodeId" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXTimeSpan ID="edSetupTime" runat="server" DataField="SetupTime" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="edRunTime" runat="server" DataField="RunTime" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="edSuspendTime" runat="server" DataField="SuspendTime" InputMask="hh:mm" />
                                    <px:PXSelector ID="edLastModifiedByIdEquipment" runat="server" DataField="LastModifiedByID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="EquipmentId" CommitChanges="true"  AutoCallBack="true"/>
                                    <px:PXGridColumn DataField="EquipmentDescription"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="SetupTime" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="RunTime" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="SuspendTime" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="IsBillable" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="LastModifiedByID" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="LastModifiedDateTime" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="EquipmentTimeCardCd" LinkCommand="ViewEquipmentTimeCard"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
             <px:PXTabItem Text="Weather" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridWeather" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" 
                               NoteIndicator="True" FilesIndicator="true" RepaintColumns="True">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Weather">
                                <RowTemplate>
                                    <px:PXDateTimeEdit ID="edTimeObserved_Time" runat="server" DataField="TimeObserved_Time"
                                        TimeMode="True" AutoRefresh="True" />
                                    <px:PXNumberEdit ID="edCloudiness" runat="server" DataField="Cloudiness" />
                                    <px:PXNumberEdit ID="edHumidity" runat="server" DataField="Humidity" />
                                    <px:PXNumberEdit ID="edPrecipitationAmount" runat="server" DataField="PrecipitationAmount" />
                                    <px:PXNumberEdit ID="edWindSpeed" runat="server" DataField="WindSpeed" />
                                    <px:PXSelector ID="edLastModifiedByIdWeather" runat="server" DataField="LastModifiedById" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TimeObserved_Time" AutoCallBack="true" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Cloudiness" />
                                    <px:PXGridColumn DataField="SkyState" />
                                    <px:PXGridColumn DataField="TemperatureLevel" />
                                    <px:PXGridColumn DataField="TemperatureLevelMobile" />
                                    <px:PXGridColumn DataField="Temperature" />
                                    <px:PXGridColumn DataField="Humidity" />
                                    <px:PXGridColumn DataField="PrecipitationAmount" />
                                    <px:PXGridColumn DataField="PrecipitationAmountMobile" />
                                    <px:PXGridColumn DataField="Precipitation" />
                                    <px:PXGridColumn DataField="WindSpeed" />
                                    <px:PXGridColumn DataField="WindSpeedMobile" />
                                    <px:PXGridColumn DataField="WindPower" />
                                    <px:PXGridColumn DataField="LocationCondition" />
                                    <px:PXGridColumn DataField="IsObservationDelayed" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="LastModifiedById" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="LastModifiedDateTime" DisplayFormat="g" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="LoadWeatherConditions" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Visitors" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridVisitors" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" 
                               NoteIndicator="True" FilesIndicator="true">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Visitors">
                                <RowTemplate>
                                    <px:PXSelector ID="edBusinessAccountId" runat="server" DataField="BusinessAccountId" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXDateTimeEdit ID="edTimeArrived" runat="server" DataField="TimeArrived_Time"
                                        TimeMode="True" AutoRefresh="True" />
                                    <px:PXDateTimeEdit ID="edTimeDeparted" runat="server" DataField="TimeDeparted_Time"
                                        TimeMode="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edLastModifiedByIdVisitors" runat="server" DataField="LastModifiedById" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="VisitorType" />
                                    <px:PXGridColumn DataField="VisitorName" />
                                    <px:PXGridColumn DataField="BusinessAccountId" LinkCommand="ViewEntity" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Company" />
                                    <px:PXGridColumn DataField="TimeArrived_Time" CommitChanges="True" AutoCallBack="true" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TimeDeparted_Time" CommitChanges="True" AutoCallBack="true" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="PurposeOfVisit" />
                                    <px:PXGridColumn DataField="AreaVisited" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="LastModifiedById" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="LastModifiedDateTime" DisplayFormat="g" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employee Expenses" RepaintOnDemand="False" BindingContext="form" >
                <Template>
                    <px:PXGrid FilesIndicator="False" NoteIndicator="False" DataSourceID="ds" Width="100%" SkinID="Details" runat="server" ID="gridEmployeeExpenses">
                        <AutoSize Container="Window" Enabled="True" MinHeight="150"/>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeExpenses" >
                                <RowTemplate>
                                    <px:PXSelector ID="edEmployeeExpenseId" runat="server" DataField="EmployeeExpenseId" AutoRefresh="True" CommitChanges="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="EmployeeExpenseId" TextField="EPExpenseClaimDetails__claimDetailCD" CommitChanges="True" LinkCommand="ViewExpenseReceipt"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__TaskID"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__CostCodeID"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__Status"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__TranDesc"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__ExpenseRefNbr"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__CuryTranAmtWithTaxes"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__CuryID"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__EmployeeID" TextField ="EPExpenseClaimDetails__EmployeeID_description"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__RefNbr" CommitChanges="True" LinkCommand="ViewExpenseClaim"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__LastModifiedByID" DisplayMode="Hint"/>
                                    <px:PXGridColumn DataField="EPExpenseClaimDetails__LastModifiedDateTime" DisplayFormat="g"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="CreateExpenseReceipt" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                            <Actions>
                                <Delete Tooltip="Unlink Expense Receipt" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approvals" BindingContext="form" RepaintOnDemand="false">
                    <Template>
                        <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                            <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                            <Levels>
                                <px:PXGridLevel DataMember="ApprovalHistory">
                                    <Columns>
                                        <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                        <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                        <px:PXGridColumn DataField="WorkgroupId" />
                                        <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                        <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                        <px:PXGridColumn DataField="ApproveDate" />
                                        <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                        <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                        <px:PXGridColumn DataField="AssignmentMapId"  Visible="false" SyncVisible="false" />
                                        <px:PXGridColumn DataField="RuleId" Visible="false" SyncVisible="false" />
                                        <px:PXGridColumn DataField="StepId" Visible="false" SyncVisible="false" />
                                        <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                    </Columns>
                                </px:PXGridLevel>
                            </Levels>
                        </px:PXGrid>
                    </Template>
                </px:PXTabItem>
                <px:PXTabItem BindingContext="form" RepaintOnDemand="False" Text="History" >
                    <Template>
                        <px:PXGrid Width="100%" runat="server" ID="gridHistory" DataSourceID="ds" SkinID ="Inquire" FilesIndicator="false">
                            <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                            <Levels>
                                <px:PXGridLevel DataMember="History" >
                                    <Columns>
                                        <px:PXGridColumn DataField="FileName" CommitChanges="True" LinkCommand="ViewAttachedReport"/>
                                        <px:PXGridColumn DataField="Comment" />
                                        <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g"/>
                                        <px:PXGridColumn DataField="CreatedById" DisplayMode="Hint"/>
                                    </Columns>
                                    <Mode AllowAddNew="False" AllowDelete ="False"/>
                                </px:PXGridLevel>
                            </Levels>
                        </px:PXGrid>
                    </Template>
                </px:PXTabItem>
        </Items>
        </px:PXTab>
</asp:Content>