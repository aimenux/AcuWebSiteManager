<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP203000.aspx.cs"
    Inherits="Page_EP203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Employee" TypeName="PX.Objects.EP.EmployeeMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewContact" Visible="False" />

            <px:PXDSCallbackCommand Name="GenerateTimeCards" Visible="False" CommitChanges="True"/>

            <px:PXDSCallbackCommand Name="ResetPassword" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ResetPasswordOK" Visible="False" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="ActivateLogin" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="EnableLogin" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="DisableLogin" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UnlockLogin" Visible="False" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="viewMap" Visible="False" CommitChanges="True" />            
            <px:PXDSCallbackCommand Name="CreateNewLicense" Visible="False" />
            <px:PXDSCallbackCommand Name="EmployeeSchedule" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="GridEmployeeLicenses" Name="EmployeeLicenses_ViewDetails" Visible="False" RepaintControls="All" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Employee" Caption="Employee Info"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edAcctCD"
        TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" DataSourceID="ds" FilterByAllFields="True" DisplayMode="Value"/>
            <px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="518px" DataSourceID="ds" DataMember="CurrentEmployee" BorderStyle="None"
        AccessKey="T">
        <Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
                    <px:PXFormView ID="ContactInfo" runat="server" Caption="Contact Info" DataMember="Contact" RenderStyle="Fieldset" DataSourceID="ds" TabIndex="200">
                        <Activity HighlightColor="" SelectedColor="" />
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" Enabled="False">
                                <LinkCommand Command="ViewContact" Target="ds" />
                            </px:PXTextEdit>
                            <px:PXDropDown ID="edTitle" runat="server" DataField="Title" AllowEdit="True" />
                            <px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
                            <px:PXTextEdit ID="edMidName" runat="server" DataField="MidName" />
                            <px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
                            <px:PXLayoutRule runat="server" Merge="True"/>
                            <px:PXLabel ID="LPhone1" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edPhone1" runat="server" DataField="Phone1"  LabelID="LPhone1"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="LPhone2" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edPhone2Type" runat="server" DataField="Phone2Type" SelectedIndex="1"  SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edPhone2" runat="server" DataField="Phone2" LabelID="LPhone2" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="LPhone3" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edPhone3Type" runat="server" DataField="Phone3Type" SelectedIndex="5" SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edPhone3" runat="server" DataField="Phone3" LabelID="LPhone3"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="LFax" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edFaxType" runat="server" DataField="FaxType" SelectedIndex="4"  SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edFax" runat="server" DataField="Fax" LabelID="LFax"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True" />
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="AddressInfo" runat="server" Caption="Address info" DataMember="Address" DataSourceID="ds" RenderStyle="FieldSet" TabIndex="300">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AllowAddNew="True" DataSourceID="ds" CommitChanges="true" AutoRefresh="True" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AllowAddNew="True" DataSourceID="ds" AutoRefresh="True" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                        </Template>
                    </px:PXFormView>

                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Employee Settings" />
                    <px:PXTextEdit ID="edAcctReferenceNbr" runat="server" DataField="AcctReferenceNbr" />
                    <px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" AllowEdit="True" />
                    <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" AllowEdit="True" />
                    <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True" />
                    <px:PXDropDown ID="edHoursValidation" runat="server" AllowNull="False" DataField="HoursValidation" />
                    <px:PXSegmentMask ID="edSupervisorID" runat="server" DataField="SupervisorID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" AutoRefresh="True" AllowEdit="True" />
                    <px:PXSelector ID="edUserID" runat="server" DataField="UserID" AllowEdit="True" Enabled="False" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryID" runat="server" DataField="CuryID" AllowAddNew="True" />
                    <px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" AllowAddNew="True" />
                    <px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXSegmentMask ID="edLabourItemID" runat="server" DataField="LabourItemID" />
                    <px:PXSelector ID="edUnion" runat="server" DataField="UnionID" />
                    <px:PXCheckBox SuppressLabel="True" ID="edRouteEmails" runat="server" DataField="RouteEmails" />
                    <px:PXCheckBox SuppressLabel="True" ID="edTimeCardRequired" runat="server" DataField="TimeCardRequired" />
                    <px:PXCheckBox SuppressLabel="True" runat="server" ID="edSDEnabled" DataField="SDEnabled" CommitChanges="True" />
                    <px:PXFormView ID="PersonalInfo" runat="server" Caption="Personal info" DataMember="Contact" DataSourceID="ds" RenderStyle="FieldSet">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXDateTimeEdit ID="edDateOfBirth" runat="server" DataField="DateOfBirth" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employment History">
                <Template>
                    <px:PXGrid runat="server" ID="gridPositions" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeePositions">
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="PositionID" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="StartDate" CommitChanges="True" />
                                    <px:PXGridColumn DataField="StartReason" />
                                    <px:PXGridColumn DataField="EndDate" CommitChanges="True" />
                                    <px:PXGridColumn DataField="IsTerminated" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TermReason" CommitChanges="True" />
                                    <px:PXGridColumn DataField="IsRehirable" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton CommandName="GenerateTimeCards" CommandSourceID="ds" />
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="GL Accounts" />
                    <px:PXFormView ID="frmPmtDefLocation" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple" TabIndex="400">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSegmentMask CommitChanges="True" ID="edVAPAccountID" runat="server" DataField="VAPAccountID" DataSourceID="ds" />
                            <px:PXSegmentMask ID="edVAPSubID" runat="server" DataField="VAPSubID" DataSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID" TabIndex="410"/>
                    <px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" TabIndex="420"/>
                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" TabIndex="430"/>
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" TabIndex="440"/>
                    <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" TabIndex="450"/>
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" TabIndex="460"/>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Financial Settings" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXFormView ID="PXFormView1" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple" TabIndex="470">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector ID="edVTaxZoneID" runat="server" DataField="VTaxZoneID" DataSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Payment Settings" />
                    <px:PXFormView ID="PXFormView3" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector CommitChanges="True" ID="edVPaymentMethodID" runat="server" DataField="VPaymentMethodID" AllowEdit="True"
                                DataSourceID="ds" />
                            <px:PXSegmentMask CommitChanges="True" ID="edVCashAccountID" runat="server" DataField="VCashAccountID" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
                            <px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" Caption="Payment Instructions" Width="400px" Height="160px" MatrixMode="True" SkinID="Attributes">
                                <Levels>
                                    <px:PXGridLevel DataMember="PaymentDetails" DataKeyNames="BAccountID,LocationID,PaymentMethodID,DetailID">
                                        <Columns>
                                            <px:PXGridColumn DataField="PaymentMethodDetail__descr" />
                                            <px:PXGridColumn DataField="DetailValue" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Layout HighlightMode="Cell" ColumnsMenu="False" HeaderVisible="False" />
                                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowSort="False" />
                                <AutoSize Enabled="False" />
                            </px:PXGrid>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                        Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False"
                                        TextField="AttributeID_description" />
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
            <px:PXTabItem Text="Activities" LoadOnDemand="True">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
                        AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
                        PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
                        BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar DefaultAction="cmdViewActivity" CustomItemsGroup="0" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False"
                                        ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False"
                                        ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False"
                                        ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
                                    <px:PXGridColumn DataField="TimeSpent" />
                                    <px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
                                        SyncVisible="False" SyncVisibility="False" Width="108px">
                                        <NavigateParams>
                                            <px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
                                        </NavigateParams>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine"
                                MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
                                      <AutoSize Container="Parent" Enabled="true" />
                                </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailings" LoadOnDemand="True">
                <Template>
                    <px:PXGrid runat="server" ID="gridNC" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" AdjustPageSize="Auto">
                        <Mode AllowAddNew="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="NWatchers" DataKeyNames="NotificationID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXTextEdit ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                    <px:PXDropDown ID="edFormat" runat="server" DataField="Format" SelectedIndex="3" />
                                    <px:PXTextEdit ID="edEntityDescription" runat="server" DataField="EntityDescription" Enabled="False" />
                                    <px:PXTextEdit ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="NotificationSetup__Module" />
                                    <px:PXGridColumn DataField="NotificationSetup__SourceCD" />
                                    <px:PXGridColumn DataField="NotificationSetup__NotificationCD" />
                                    <px:PXGridColumn DataField="ClassID" />
                                    <px:PXGridColumn DataField="EntityDescription" />
                                    <px:PXGridColumn DataField="ReportID" />
                                    <px:PXGridColumn DataField="TemplateID" />
                                    <px:PXGridColumn DataField="Format" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            
            <px:PXTabItem Text="Company Tree Info">
                <Template>
                    <px:PXGrid ID="companyTreeGrid" runat="server" DataSourceID="ds" Height="400px" 
                        Width="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="CompanyTree" DataKeyNames="WorkGroupID,UserID">                                
                                <Columns>
                                    <px:PXGridColumn DataField="WorkGroupID" Label="Workgroup ID" />
                                    <px:PXGridColumn DataField="IsOwner" Label="Owner" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Active" Label="Active" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Assignment and Approval Maps">
                <Template>
                    <px:PXGrid ID="gridAssignmentandApprovalMaps" runat="server" DataSourceID="ds" Height="400px" 
                        Width="100%" SkinID="Inquire" MatrixMode="True" SyncPosition="true" 
                        FilesIndicator="false" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="AssigmentAndApprovalMaps">                                
                                <Columns>
                                    <px:PXGridColumn DataField="EPAssignmentMap__Name" LinkCommand="ViewMap"/>
                                    <px:PXGridColumn DataField="StepName" />
                                    <px:PXGridColumn DataField="Name" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem> 

            <px:PXTabItem LoadOnDemand="True" Text="User Info">
                <Template>
                    <px:PXFormView ID="frmLogin" runat="server" DataMember="User" SkinID="Transparent" MarkRequired="Dynamic">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXDropDown ID="edState" runat="server" DataField="State" Enabled="False" />
                            <px:PXSelector ID="edLoginType" runat="server" DataField="LoginTypeID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" />
                            <px:PXMaskEdit ID="edUsername" runat="server" DataField="Username" CommitChanges="True" />
                            <px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" />
                            <px:PXCheckBox ID="edGenerate" runat="server" DataField="GeneratePassword" CommitChanges="True" />
                            <px:PXButton ID="btnResetPassword" runat="server" Text="Reset Password" CommandName="ResetPassword" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="SM" StartColumn="True" SuppressLabel="True" />
                            <px:PXButton ID="btnActivateLogin" runat="server" CommandName="ActivateLogin" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXButton ID="btnEnableLogin" runat="server" CommandName="EnableLogin" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXButton ID="btnDisableLogin" runat="server" CommandName="DisableLogin" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXButton ID="btnUnlockLogin" runat="server" CommandName="UnlockLogin" CommandSourceID="ds" Width="150" Height="20" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" Caption=" ">
                        <ActionBar>
                            <Actions>
                                <Save Enabled="False" />
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Roles">
                                <Columns>
                                    <px:PXGridColumn AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Rolename" AllowUpdate="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
                    </px:PXGrid>
                    <px:PXSmartPanel ID="pnlResetPassword" runat="server" Caption="Change password"
                        LoadOnDemand="True" Width="400px" Key="User" CommandName="ResetPasswordOK" 
                        CommandSourceID="ds" AcceptButtonID="btnOk" CancelButtonID="btnCancel" 
                        AutoCallBack-Command="Refresh" AutoCallBack-Target="frmResetParams" 
                        AutoCallBack-Enabled="true" AutoReload="True">
                        <px:PXFormView ID="frmResetParams" runat="server" DataSourceID="ds" Width="100%" DataMember="User"
                            Caption="Reset Password" SkinID="Transparent">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                                <px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
                                <px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
                            </Template>
                        </px:PXFormView>
                        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                            <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK" />
                            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
                        </px:PXPanel>
                    </px:PXSmartPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Corporate Cards" Visible="True">
                <Template>
                    <px:PXGrid runat="server" ID="gridCorpCards" DataSourceID="ds" SkinID="DetailsInTab" Height="400px" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeCorpCards">
                                <Columns>
                                    <px:PXGridColumn DataField="corpCardID" Width="140" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CACorpCard__Name" Width="220" />
                                    <px:PXGridColumn DataField="CACorpCard__CardNumber" Width="140" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Delegates">
                <Template>
                    <px:PXGrid ID="companyTreeGrid" runat="server" DataSourceID="ds" Height="400px" Width="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Wingman" DataKeyNames="RecordID">
                                <Columns>
                                    <px:PXGridColumn runat="server" DataField="WingmanId" CommitChanges="True" />
                                    <px:PXGridColumn runat="server" DataField="WingmanId_EPEmployee_acctName" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode InitNewRow="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Labor Item Overrides">
                <Template>
                    <px:PXGrid ID="LaborClassesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
                        DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="LaborMatrix">
                                <Columns>
                                    <px:PXGridColumn DataField="EarningType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EPEarningType__Description" />
                                    <px:PXGridColumn DataField="LabourItemID" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Skills" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridEmployeeSkills" SkinID="DetailsInTab" Style='height:400px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeSkills">
                                <Columns>
                                    <px:PXGridColumn DataField="SkillID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FSSkill__Descr" />
                                    <px:PXGridColumn DataField="FSSkill__IsDriverSkill" Type="CheckBox" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edSkillID" DataField="SkillID" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edFSSkill__Descr" DataField="FSSkill__Descr" />
                                    <px:PXCheckBox runat="server" ID="edFSSkill__IsDriverSkill" DataField="FSSkill__IsDriverSkill" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                    </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Service Areas" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridEmployeeGeoZones" SkinID="DetailsInTab" Style='height:400px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeGeoZones">
                                <Columns>
                                    <px:PXGridColumn DataField="GeoZoneID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FSGeoZone__Descr" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edGeoZoneID" DataField="GeoZoneID" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edFSGeoZone__Descr" DataField="FSGeoZone__Descr" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Licenses" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridEmployeeLicenses" SyncPosition="true" SkinID="DetailsInTab" DataSourceID="ds" Style='height:400px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeLicenses" DataKeyNames="RefNbr,LicenseTypeID">
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenLicenseDocument"/>
                                    <px:PXGridColumn DataField="LicenseTypeID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Descr" />
                                    <px:PXGridColumn DataField="IssueDate" />
                                    <px:PXGridColumn DataField="NeverExpires" Type="CheckBox" TextAlign="Center" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="ExpirationDate" />                                    
                                </Columns>
                                <RowTemplate>
                                    <px:PXTextEdit runat="server" ID="edRefNbr" DataField="RefNbr" />
                                    <px:PXSelector runat="server" ID="edLicenseTypeID" DataField="LicenseTypeID" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edDescr" DataField="Descr" />
                                    <px:PXDateTimeEdit runat="server" ID="edIssueDate" DataField="IssueDate" />
                                    <px:PXCheckBox runat="server" ID="edNeverExpires" DataField="NeverExpires"/>
                                    <px:PXDateTimeEdit runat="server" ID="edExpirationDate" DataField="ExpirationDate" />
                                </RowTemplate>
                                <Mode InitNewRow="True" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="viewDetail">
                            <Actions>
                                <AddNew ToolBarVisible="Top" />
                                <Delete ToolBarVisible="Top" />
                            </Actions>
                            <CustomItems />
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" MinHeight="538" Container="Window" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelGenerateTimeCards" runat="server" Caption="Generate Time Cards"
        CaptionVisible="True" Key="GenTimeCardFilter" LoadOnDemand="true" AutoCallBack-Command="gentcform" AutoCallBack-Enabled="True" CallBackMode-CommitChanges="True" AutoReload="True">
        <px:PXFormView ID="gentcform" runat="server" DataSourceID="ds" DataMember="GenTimeCardFilter" SkinID="Transparent" DefaultControlID="edGenerateUntil">
            <Template>
                <px:PXLayoutRule runat="server" ID="rule1" StartColumn="true" LabelsWidth="XS" ControlSize="M" />
                <px:PXDateTimeEdit ID="edLastDateGenerated" runat="server" DataField="LastDateGenerated" TimeMode="false" DisplayFormat="d" />
                <px:PXDateTimeEdit ID="edGenerateUntil" runat="server" DataField="GenerateUntil" TimeMode="false" DisplayFormat="d" EditFormat="d" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Generate" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlChangeID" runat="server"  Caption="Specify New ID"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
            <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                DataMember="ChangeIDDialog">
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
                </Template>
            </px:PXFormView>
            <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
                <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" >
                    <AutoCallBack Target="formChangeID" Command="Save" />
                </px:PXButton>
                <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />                       
            </px:PXPanel>
    </px:PXSmartPanel>    
</asp:Content>
