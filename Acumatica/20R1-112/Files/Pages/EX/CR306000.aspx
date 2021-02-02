<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CR306000.aspx.cs" Inherits="Page_CR306000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CRCaseMaint" PrimaryView="Case">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Assign" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="SubmitToTfs" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="CaseRefs_CRCase_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="gridCaseReferencesDependsOn" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Case" Caption="Case Summary" NoteIndicator="True" FilesIndicator="True"
        LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edCaseCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edCaseCD" runat="server" DataField="CaseCD" FilterByAllFields="True" AutoRefresh="True" />
            <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" DisplayFormat="g" Enabled="False" Size="SM"/>
            <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastActivity" DisplayFormat="g" Enabled="False" Size="SM"/>
            <px:PXDateTimeEdit ID="edSLAETA" runat="server" DataField="SLAETA" DisplayFormat="g" Enabled="False" EditFormat="g" Size="SM"/>
            <px:PXDateTimeEdit ID="edResolutionDate" runat="server" DataField="ResolutionDate" Enabled="False" Size="SM"/>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="Subject" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edCaseClassID" runat="server" DataField="CaseClassID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint"
                AutoRefresh="True" />
            <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" DisplayMode="Text" TextMode="Search" TextField="displayName" AutoRefresh="True"
                AllowEdit="True" FilterByAllFields="True" />
            <px:PXSelector CommitChanges="True" ID="edContractID" runat="server" DataField="ContractID" DisplayMode="Hint" TextMode="Search" AutoRefresh="True" AllowEdit="True"
                        FilterByAllFields="True" />
            <px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="true" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" DataField="Status" AllowNull="False" />
            <px:PXDropDown CommitChanges="True" ID="edResolution" runat="server" DataField="Resolution" AllowNull="False" />
            <px:PXDropDown CommitChanges="True" ID="edSeverity" runat="server" DataField="Severity" SelectedIndex="-1" AllowNull="False" />
            <px:PXDropDown ID="edPriority" runat="server" DataField="Priority" SelectedIndex="-1" AllowNull="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="400px" DataSourceID="ds" DataMember="CaseCurrent">
        <Items>
            <px:PXTabItem Text="Details">
                <AutoCallBack Command="Refresh" Target="tab" ActiveBehavior="true">
                    <Behavior CommitChanges="true" PostData="Page" />
                </AutoCallBack>
                <Template>
                    <px:PXRichTextEdit ID="edDescription" runat="server" DataField="Description" Style="border-top: 0px; border-left: 0px; border-right: 0px; margin: 0px; padding: 0px;
                        width: 100%;">
                        <AutoSize Enabled="True" MinHeight="216" />
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
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Billing" />
                    <px:PXCheckBox CommitChanges="True" ID="chkIsBillable" runat="server" DataField="IsBillable" />
                    <px:PXCheckBox CommitChanges="True" ID="chkManualBillableTimes" runat="server" DataField="ManualBillableTimes" />
                    <px:PXMaskEdit ID="edTimeBillable" runat="server" DataField="TimeBillable" InputMask="### hrs ## mins" Size="SM" />
                    <px:PXMaskEdit ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="### hrs ## mins" Size="SM" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="XM" StartColumn="True" GroupCaption="Service Details" />
                    <px:PXSelector ID="WorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
                    <px:PXMaskEdit CommitChanges="True" ID="edTimeEstimated" runat="server" DataField="TimeEstimated" Size="S" InputMask="### hrs ## mins" />
                    <px:PXDateTimeEdit ID="PXDateTimeEdit1" runat="server" DataField="ETA" DisplayFormat="g" Enabled="False" EditFormat="g" />
                    <px:PXDateTimeEdit ID="PXDateTimeEdit2" runat="server" DataField="RemaininingDate" DisplayFormat="g" Enabled="False" EditFormat="g" />
                    <px:PXMaskEdit ID="edInitResponse" runat="server" DataField="InitResponse" InputMask="### hrs ## mins" />
                    <px:PXMaskEdit ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="### hrs ## mins" />
                    <px:PXMaskEdit ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="### hrs ## mins" />
                    <px:PXMaskEdit ID="edTimeToResolution" runat="server" DataField="TimeResolution" InputMask="### hrs ## mins" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" AllowResize="False" />
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
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activities" LoadOnDemand="true">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray" PreviewPanelStyle="z-index: 100; background-color: Window"
                        PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar ActionsText="true" DefaultAction="cmdViewActivity" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                    <ActionBar />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdViewActivity" Visible="false">
                                    <ActionBar MenuVisible="false" />
                                    <AutoCallBack Command="ViewActivity" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
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
                                    <px:PXGridColumn DataField="TimeSpent" />
                                    <px:PXGridColumn DataField="OvertimeSpent" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" />
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
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100%" SkinID="Label" >
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
                                    <px:PXGridColumn DataField="RelationType" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CRCase__Subject" Width="250px" />
                                    <px:PXGridColumn DataField="CRCase__Status" Width="100px" />
                                    <px:PXGridColumn DataField="CRCase__OwnerID" Width="150px" />
                                    <px:PXGridColumn DataField="CRCase__WorkgroupID" Width="150px" />
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
                              <px:PXGridColumn DataField="IsPrimary" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="TargetType" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="TargetNoteID" DisplayMode="Text"  LinkCommand="Relations_TargetDetails" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="EntityID" AutoCallBack="true" LinkCommand="Relations_EntityDetails" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="Name" ></px:PXGridColumn>
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
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
    </px:PXTab>
</asp:Content>
