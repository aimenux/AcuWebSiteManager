<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CR306000.aspx.cs" Inherits="Page_CR306000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CRCaseMaint" PrimaryView="Case">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true"/>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" PopupVisible="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Assign" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="ViewInvoice" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="CaseRefs_CRCase_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="gridCaseReferencesDependsOn" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="TakeCase" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
            <px:PXDSCallbackCommand Name="AddNewContact" Visible="false" /> 
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">

    <px:PXSmartPanel ID="panelCreateServiceOrder" runat="server" Style="z-index: 108; left: 27px; position: absolute; top: 99px;" Caption="Create Service Order" Width="390px" Height="180px" AutoRepaint="true"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateServiceOrderFilter" AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXFormView ID="formCreateServiceOrder" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="CreateServiceOrderFilter">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXSelector ID="edSrvOrdType" runat="server" AllowNull="False" DataField="SrvOrdType" DisplayMode="Text" CommitChanges="True" AutoRefresh="true" AllowEdit="True" />
                <px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" DisplayMode="Text" CommitChanges="True" AutoRefresh="true" AllowEdit="True"/>
                <px:PXSelector ID="edAssignedEmpID" runat="server" DataField="AssignedEmpID" DisplayMode="Text" CommitChanges="True" AutoRefresh="true" AllowEdit="True"/>
                <px:PXSelector ID="edProblemID" runat="server" DataField="ProblemID" DisplayMode="Text" CommitChanges="True" AutoRefresh="true" AllowEdit="True"/>
            </Template>
        </px:PXFormView>

        <div style="padding: 5px; text-align: right;">
                <px:PXButton ID="btnSave2" runat="server" CommitChanges="True" DialogResult="OK" Text="OK" Height="20"/>
                <px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
        </div>
    </px:PXSmartPanel>

    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Case" Caption="Case Summary" NoteIndicator="True" FilesIndicator="True"
        LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edCaseCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXSelector ID="edCaseCD" runat="server" DataField="CaseCD" FilterByAllFields="True" AutoRefresh="True" />
            <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" DisplayFormat="g" Enabled="False" Size="SM"/>
            <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastActivity" DisplayFormat="g" Enabled="False" Size="SM"/>
            <px:PXDateTimeEdit ID="edSLAETA" runat="server" DataField="SLAETA" DisplayFormat="g" Enabled="False" EditFormat="g" Size="SM"/>
            <px:PXDateTimeEdit ID="edResolutionDate" runat="server" DataField="ResolutionDate" Enabled="False" Size="SM"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edCaseClassID" runat="server" DataField="CaseClassID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint"
                AutoRefresh="True" />
            <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" DisplayMode="Text" TextMode="Search" TextField="displayName" AutoRefresh="True"
                AllowEdit="True" FilterByAllFields="True"  OnEditRecord="edContactID_EditRecord"/>
            <px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="true" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
            <px:PXButton runat="server" ID="edSDButton">
                <AutoCallBack Target="ds" Command="OpenAppointmentBoard" />
            </px:PXButton>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" DataField="Status" AllowNull="False" />
            <px:PXDropDown CommitChanges="True" ID="edResolution" runat="server" DataField="Resolution" AllowNull="False"/>
            <px:PXDropDown CommitChanges="True" ID="edSeverity" runat="server" DataField="Severity" SelectedIndex="-1" AllowNull="False" />
            <px:PXDropDown ID="edPriority" runat="server" DataField="Priority" SelectedIndex="-1" AllowNull="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="400px" DataSourceID="ds" DataMember="CaseCurrent" MarkRequired="Dynamic">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXRichTextEdit ID="edDescription" runat="server" Style="border-width: 0px; width: 100%;" DataField="Description" 
						AllowLoadTemplate="false"  AllowDatafields="false"  AllowMacros="true" AllowSourceMode="true" AllowAttached="true" AllowSearch="true">
						<AutoSize Enabled="True" />
						<LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
					</px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Additional Info">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Business Account Details" />
                    <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint"
                        AutoRefresh="True" Enabled="false" />
                    <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" AllowEdit="True" FilterByAllFields="True" TextMode="Search"
                        DisplayMode="Hint" />
                    <px:PXSelector CommitChanges="True" ID="edContractID" runat="server" DataField="ContractID" DisplayMode="Hint" TextMode="Search" AutoRefresh="True" AllowEdit="True"
                        FilterByAllFields="True" />
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Billing" />
                    <px:PXCheckBox CommitChanges="True" ID="chkIsBillable" runat="server" DataField="IsBillable" />
                    <px:PXCheckBox CommitChanges="True" ID="chkManualBillableTimes" runat="server" DataField="ManualBillableTimes" />
                    <px:PXTimeEdit ID="edTimeBillable" runat="server" DataField="TimeBillable"/>
                    <px:PXTimeEdit ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable"/>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" LabelsWidth="M" ControlSize="XM" StartColumn="True" GroupCaption="Service Details" />
                    <px:PXSelector ID="WorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
                    <px:PXMaskEdit ID="edInitResponse" runat="server" DataField="InitResponse" InputMask="### hrs ## mins" />
                    <px:PXMaskEdit ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="### hrs ## mins" />
                    <px:PXMaskEdit ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="### hrs ## mins" />
                    <px:PXMaskEdit ID="edTimeToResolution" runat="server" DataField="TimeResolution" InputMask="### hrs ## mins" />
                    <px:PXDateTimeEdit ID="edLastIncomingDate" runat="server" DataField="CaseActivityStatistics.LastIncomingActivityDate" Enabled="False" Size="SM"/>
                    <px:PXDateTimeEdit ID="edLastOutgoingDate" runat="server" DataField="CaseActivityStatistics.LastOutgoingActivityDate" Enabled="False" Size="SM"/>
                    <px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activities" LoadOnDemand="true">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" PreviewPanelStyle="z-index: 100; background-color: Window"
                        PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar ActionsText="true" DefaultAction="cmdViewActivity" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
			                    <RowTemplate>
					                <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
                                </RowTemplate>                                
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsBillable" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text"/>
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands>
                            <Refresh CommitChanges="True" PostData="Page" />
                        </CallbackCommands>
                        <GridMode AllowAddNew="False" AllowUpdate="False" />
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
                                      <AutoSize Container="Parent" Enabled="true" />
                                </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Related Cases" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="gridCaseReferencesDependsOn" runat="server" DataSourceID="ds" Height="162px" Style="z-index: 101; left: 0px; position: absolute; top: 0px;" AllowSearch="True"
                        ActionsPosition="Top" SkinID="Details" Width="100%" BorderWidth="0px">
                        <Levels>
                            <px:PXGridLevel DataMember="CaseRefs">
                                <Columns>
                                    <px:PXGridColumn DataField="ChildCaseCD" Width="100px" RenderEditorText="True" AutoCallBack="True" LinkCommand="CaseRefs_CRCase_ViewDetails" />
                                    <px:PXGridColumn DataField="RelationType" Width="100px" RenderEditorText="True" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="CRCaseRelated__Subject" Width="250px" />
                                    <px:PXGridColumn DataField="CRCaseRelated__Status" Width="100px" />
                                    <px:PXGridColumn DataField="CRCaseRelated__OwnerID" Width="150px" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="CRCaseRelated__WorkgroupID" Width="150px" />
                                </Columns>
                                <Mode InitNewRow="true" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="300" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Relations" LoadOnDemand="True">
                <Template>
					  <px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
                        <Levels>
                          <px:PXGridLevel DataMember="Relations">
                            <Columns>
                              <px:PXGridColumn DataField="Role" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="IsPrimary" Type="CheckBox" TextAlign="Center"  CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="TargetType" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="TargetNoteID" DisplayMode="Text"  LinkCommand="Relations_TargetDetails" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="EntityID" AutoCallBack="true" LinkCommand="Relations_EntityDetails" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="Name"></px:PXGridColumn>
                              <px:PXGridColumn DataField="ContactID" AutoCallBack="true" TextAlign="Left" TextField="ContactName" DisplayMode="Text" LinkCommand="Relations_ContactDetails" ></px:PXGridColumn>
                              <px:PXGridColumn DataField="Email" ></px:PXGridColumn>
                              <px:PXGridColumn DataField="AddToCC" Type="CheckBox" TextAlign="Center" ></px:PXGridColumn>
                            </Columns>
                            <RowTemplate>
                              <px:PXSelector ID="edTargetNoteID" runat="server" DataField="TargetNoteID" FilterByAllFields="True" AutoRefresh="True" />
                              <px:PXSelector ID="edRelEntityID" runat="server" DataField="EntityID" FilterByAllFields="True" AutoRefresh="True" />
                              <px:PXSelector ID="edRelContactID" runat="server" DataField="ContactID" FilterByAllFields="True" AutoRefresh="True" />
                            </RowTemplate>
                          </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" ></Mode>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" ></AutoSize>
                      </px:PXGrid>                 
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Sync Status">
		    <Template>
		        <px:PXGrid ID="syncGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire" SyncPosition="true">
		            <Levels>
		                <px:PXGridLevel DataMember="SyncRecs" DataKeyNames="SyncRecordID">
		                    <Columns>
		                        <px:PXGridColumn DataField="SYProvider__Name" />
		                        <px:PXGridColumn DataField="RemoteID" CommitChanges="True" LinkCommand="GoToSalesforce" />
		                        <px:PXGridColumn DataField="Status" />
		                        <px:PXGridColumn DataField="Operation" />
		                        <px:PXGridColumn DataField="SFEntitySetup__ImportScenario" />
		                        <px:PXGridColumn DataField="SFEntitySetup__ExportScenario" />
		                        <px:PXGridColumn DataField="LastErrorMessage" />
		                        <px:PXGridColumn DataField="LastAttemptTS" DisplayFormat="g" />
		                        <px:PXGridColumn DataField="AttemptCount" />
		                    </Columns>                               
		                    <Layout FormViewHeight="" />
		                </px:PXGridLevel>
		            </Levels>
		            <ActionBar>                        
		                <CustomItems>
		                    <px:PXToolBarButton Key="SyncSalesforce">
		                        <AutoCallBack Command="SyncSalesforce" Target="ds"/>
		                    </px:PXToolBarButton>
		                </CustomItems>
		            </ActionBar>
		            <Mode InitNewRow="true" />
		            <AutoSize Enabled="True" MinHeight="150" />
		        </px:PXGrid>
		    </Template>
		</px:PXTabItem>
			<px:PXTabItem Text="Owner User" Visible="False">
				<Template>
					<px:PXFormView ID="frmOwnerUser" runat="server" DataMember="OwnerUser" DataSourceID="ds">
						<Template>
							<px:PXTextEdit ID="edPKID" runat="server" DataField="PKID" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
    </px:PXTab>
</asp:Content>
