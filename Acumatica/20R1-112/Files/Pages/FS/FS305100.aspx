<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS305100.aspx.cs" Inherits="Page_FS305100" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.ServiceContractScheduleEntry" PrimaryView="ContractScheduleRecords">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="OpenServiceContractInq" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="True" DependOnGrid="PXGridDetails" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="True" DependOnGrid="PXGridDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ContractScheduleRecords" 
        TabIndex="5300" DefaultControlID="edSrvOrdType" NoteIndicator="True" NotifyIndicator="True" FilesIndicator="True" AllowCollapse="True">
		<Template>
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize = "M" LabelsWidth="SM">
                </px:PXLayoutRule>  
                <px:PXSelector ID="edEntityID" runat="server" DataField="EntityID" AutoRefresh="True" CommitChanges="True" FilterByAllFields="True">
                    <AutoCallBack Command="Cancel" Target="ds" />
                </px:PXSelector>
                <px:PXLayoutRule runat="server" Merge="True">
                </px:PXLayoutRule>
                <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" Width = "135px">
                    <AutoCallBack Command="Cancel" Target="ds" />
                </px:PXSelector>
                <px:PXCheckBox ID="edActive" runat="server" DataField="Active">
                </px:PXCheckBox>
                <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True">
                </px:PXSegmentMask>
                <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AllowEdit="True" CommitChanges="True">
                </px:PXSegmentMask>
                <px:PXTextEdit ID="edCustomerContractNbr" runat="server" DataField="CurrentServiceContract.CustomerContractNbr">
                </px:PXTextEdit>
                <px:PXSegmentMask ID="edProjectID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="ProjectID" AllowEdit="True">
                </px:PXSegmentMask>
                <px:PXSelector ID="edDfltProjectTaskID" runat="server" DataField="DfltProjectTaskID" AllowEdit = "True" AutoRefresh="True" CommitChanges="True" DisplayMode="Hint">
                </px:PXSelector>
                <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" CommitChanges="True" AllowEdit="True">
                </px:PXSelector>
                <px:PXDropDown ID="edScheduleGenType" runat="server" DataField="ScheduleGenType">
                </px:PXDropDown>
                <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" Visible="False">
                </px:PXSelector>
                <px:PXSelector ID="edBranchLocationID" runat="server" AllowEdit="True" 
                    DataField="BranchLocationID" AutoRefresh="True" CommitChanges="True" Visible="False">
                </px:PXSelector>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M">
                </px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edScheduleStartTime" runat="server" DataField="ScheduleStartTime_Time" TimeMode="true">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Size="S" CommitChanges="True">
                </px:PXDateTimeEdit>            
                <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" Size="S" CommitChanges ="True">
                </px:PXDateTimeEdit>
				<px:PXDateTimeEdit ID="edNextExecutionDate" runat="server" DataField="NextExecutionDate">
                </px:PXDateTimeEdit>
                <px:PXLayoutRule runat="server">
                </px:PXLayoutRule>    
                <px:PXDateTimeEdit ID="edLastGeneratedElementDate" runat="server" DataField="LastGeneratedElementDate" Enabled="False" Size="S" >
                </px:PXDateTimeEdit>
                <px:PXLayoutRule runat="server" StartGroup="True" ControlSize="M" GroupCaption="Additional Settings" StartRow="True" LabelsWidth="SM">
                </px:PXLayoutRule>
                <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID">
                </px:PXSegmentMask>
        </Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="550px" DataSourceID="ds" 
        DataMember="ContractScheduleSelected">
        <Items>
        <px:PXTabItem Text="Details">
            <Template>
                    <px:PXGrid ID="PXGridDetails" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100%" SkinID="DetailsInTab" TabIndex="2300" SyncPosition="True">
		                <Levels>
                        <px:PXGridLevel 
                            DataMember="ScheduleDetails" 
                            DataKeyNames="ScheduleID,LineNbr">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True">
                                </px:PXLayoutRule>                                  
                                <px:PXDropDown ID="edLineType" runat="server" DataField="LineType" CommitChanges="True" AutoRefresh="True">
                                </px:PXDropDown>                                                                      
                                <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" AllowEdit="True" CommitChanges="True">
                                </px:PXSegmentMask>
                                <px:PXDropDown ID="edBillingRule" runat="server" DataField="BillingRule" Size="SM">
                                </px:PXDropDown>
                                <px:PXSelector ID="edServiceTemplateID" runat="server" DataField="ServiceTemplateID" AllowEdit="True" CommitChanges="True">
                                </px:PXSelector>
                                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                                </px:PXNumberEdit>
                                <px:PXDropDown ID="edEquipmentAction" runat="server" DataField="EquipmentAction" CommitChanges="True">
                                </px:PXDropDown>
                                <px:PXSelector ID="edSMEquipmentID" runat="server" CommitChanges="true" DataField="SMEquipmentID" AllowEdit="True" AutoRefresh="True">
                                </px:PXSelector>
                                <px:PXSelector ID="edComponentID" runat="server" DataField="ComponentID" AutoRefresh="True" CommitChanges="True">
                                </px:PXSelector>
								<px:PXSelector ID="edEquipmentLineRef" runat="server" DataField="EquipmentLineRef" AutoRefresh="True" CommitChanges="True">
								</px:PXSelector>
                                <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc">
                                </px:PXTextEdit>
                                <px:PXSelector ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" CommitChanges="True">
                                </px:PXSelector>
                                <px:PXSelector ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="SortOrder" TextAlign="Right">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="LineNbr" TextAlign="Right">
                                </px:PXGridColumn>                                   
                                <px:PXGridColumn DataField="LineType" RenderEditorText="True" MatrixMode="True" CommitChanges="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="InventoryID" CommitChanges ="True" AllowDragDrop="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="BillingRule" CommitChanges ="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="ServiceTemplateID" CommitChanges="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="Qty" TextAlign="Right" AllowDragDrop="true">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="EquipmentAction" CommitChanges="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="SMEquipmentID" CommitChanges="true">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="ComponentID" TextAlign="Right" CommitChanges="True">
                                </px:PXGridColumn>
								<px:PXGridColumn DataField="EquipmentLineRef" CommitChanges="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="TranDesc">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="True" DisplayMode="Hint"/>
                                <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                            </Columns>
                        </px:PXGridLevel>
		                </Levels>
                        <CallbackCommands PasteCommand="PasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
                        <Mode AllowFormEdit="True" InitNewRow="True" AllowUpload="True" AllowDragRows="True"/>
		                <AutoSize Container="Window" Enabled="True" MinHeight="400" />
		                <ActionBar ActionsText="False">
		                </ActionBar>
	                </px:PXGrid>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="Recurrence">
		    <Template>
                <%-- Frequency Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" StartGroup="True" ControlSize="XM"></px:PXLayoutRule>
                <px:PXGroupBox ID="edFrequencyType" runat="server" Caption="Frequency Settings" CommitChanges="True" DataField="FrequencyType" Width="200px">
                    <Template>
                        <px:PXRadioButton ID="edFrequencyType_op0" runat="server" GroupName="edFrequencyType" Text="Daily" Value="D" />
                        <px:PXRadioButton ID="edFrequencyType_op1" runat="server" GroupName="edFrequencyType" Text="Weekly" Value="W" />
                        <px:PXRadioButton ID="edFrequencyType_op2" runat="server" GroupName="edFrequencyType" Text="Monthly" Value="M" />
                        <px:PXRadioButton ID="edFrequencyType_op3" runat="server" GroupName="edFrequencyType" Text="Yearly" Value="A" />
                    </Template>
                    <ContentLayout LabelsWidth="S" Layout="Stack" OuterSpacing="Horizontal" />
                </px:PXGroupBox>
                <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                <px:PXTextEdit ID="edRecurrenceDescription" runat="server" CommitChanges="True" DataField="RecurrenceDescription" Enabled="False" Height="100px" LabelWidth="150px" SuppressLabel="True" TextAlign="Center" TextMode="MultiLine"></px:PXTextEdit>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Frequency Settings Section --%>

                <%-- Seasons Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" GroupCaption="Season Settings"></px:PXLayoutRule>
                
                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" Merge="True"/>
                <px:PXCheckBox ID="edSeasonOnJan" runat="server" DataField="SeasonOnJan" Text="January" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnApr" runat="server" DataField="SeasonOnApr" Text="April" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnJul" runat="server" DataField="SeasonOnJul" Text="July" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnOct" runat="server" DataField="SeasonOnOct" Text="October" Size="S"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" Merge="False"/>
                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" Merge="True"/>
                <px:PXCheckBox ID="edSeasonOnFeb" runat="server" DataField="SeasonOnFeb" Text="February" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnMay" runat="server" DataField="SeasonOnMay" Text="May" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnAug" runat="server" DataField="SeasonOnAug" Text="August" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnNov" runat="server" DataField="SeasonOnNov" Text="November" Size="S"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" Merge="False"/>
                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" Merge="True"/>
                <px:PXCheckBox ID="edSeasonOnMar" runat="server" DataField="SeasonOnMar" Text="March" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnJun" runat="server" DataField="SeasonOnJun" Text="June" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnSep" runat="server" DataField="SeasonOnSep" Text="September" Size="S"></px:PXCheckBox>
                <px:PXCheckBox ID="edSeasonOnDec" runat="server" DataField="SeasonOnDec" Text="December" Size="S"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" Merge="False"/>

                <%--edCreatedByScreenID field is needed to manage properly the show/hide actions of the Season settings--%>
                <px:PXMaskEdit ID="edCreatedByScreenID" runat="server" DataField="CreatedByScreenID"></px:PXMaskEdit>

                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Seasons Settings Section --%>

                <%-- Yearly Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow ="True" StartGroup="True" GroupCaption="Yearly Settings"></px:PXLayoutRule>

                <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS"></px:PXLayoutRule>
                <px:PXNumberEdit ID="edAnnualFrequency" runat="server" DataField="AnnualFrequency" Size="XS" CommitChanges="True"></px:PXNumberEdit>
                <px:PXTextEdit ID="edYearlyLabel" DataField="YearlyLabel" runat="server" Size="S" SkinID="Label" Enabled="False" SuppressLabel="True"></px:PXTextEdit>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>

                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartRow="True" StartColumn="True"></px:PXLayoutRule>
                <px:PXCheckBox ID="chkAnnualOnJan" runat="server" DataField="AnnualOnJan" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkAnnualOnFeb" runat="server" DataField="AnnualOnFeb" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkAnnualOnMar" runat="server" DataField="AnnualOnMar" CommitChanges="True"></px:PXCheckBox> 
                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                <px:PXCheckBox ID="chkAnnualOnApr" runat="server" DataField="AnnualOnApr" CommitChanges="True"></px:PXCheckBox> 
                <px:PXCheckBox ID="chkAnnualOnMay" runat="server" DataField="AnnualOnMay" CommitChanges="True"></px:PXCheckBox>     
                <px:PXCheckBox ID="chkAnnualOnJun" runat="server" DataField="AnnualOnJun" CommitChanges="True"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                <px:PXCheckBox ID="chkAnnualOnJul" runat="server" DataField="AnnualOnJul" CommitChanges="True"></px:PXCheckBox> 
                <px:PXCheckBox ID="chkAnnualOnAug" runat="server" DataField="AnnualOnAug" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkAnnualOnSep" runat="server" DataField="AnnualOnSep" CommitChanges="True"></px:PXCheckBox> 
                <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                <px:PXCheckBox ID="chkAnnualOnOct" runat="server" DataField="AnnualOnOct" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkAnnualOnNov" runat="server" DataField="AnnualOnNov" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkAnnualOnDec" runat="server" DataField="AnnualOnDec" CommitChanges="True"></px:PXCheckBox>

                <px:PXLayoutRule runat="server" StartRow="True"></px:PXLayoutRule>
                <px:PXGroupBox ID="edAnnually" runat="server" Caption="Schedule On" CommitChanges="True" DataField="AnnualRecurrenceType">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay5" runat="server" GroupName="edAnnually" Value="D" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edAnnualOnDay" runat="server" DataField="AnnualOnDay" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDayOfWeek5" runat="server" GroupName="edAnnually" Value="W" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edAnnualOnWeek" runat="server" DataField="AnnualOnWeek" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXDropDown ID="edAnnualOnDayOfWeek" runat="server" DataField="AnnualOnDayOfWeek" Size="S" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    </Template>
                </px:PXGroupBox>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Yearly Settings Section --%>

                <%-- Monthly Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" StartColumn="True" GroupCaption="Monthly Settings"></px:PXLayoutRule>

                <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS"></px:PXLayoutRule>
                <px:PXDropDown ID="edMonthlyFrequency" runat="server" DataField="MonthlyFrequency" Size="XS" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                <px:PXTextEdit ID="edMonthlyLabel" runat="server" DataField="MonthlyLabel" SuppressLabel="True" SkinID="Label" Enabled="False" Size="S"></px:PXTextEdit>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>

                <px:PXGroupBox ID="edMonthly1" runat="server" Caption="Schedule On" CommitChanges="True" DataField="MonthlyRecurrenceType1">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay1" runat="server" GroupName="edMonthly1" Value="D" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnDay1" runat="server" DataField="MonthlyOnDay1" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDayOfWeek1" runat="server" GroupName="edMonthly1" Value="W" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnWeek1" runat="server" DataField="MonthlyOnWeek1" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek1" runat="server" DataField="MonthlyOnDayOfWeek1" Size="S" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>
                    </Template>
                </px:PXGroupBox>

                <%-- Second Recurrence Monthly Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" GroupCaption="Second Recurrence Monthly Settings"></px:PXLayoutRule>
                <px:PXCheckBox ID="edMonthly2Selected" runat="server" DataField="Monthly2Selected" AlignLeft="True" CommitChanges="True"></px:PXCheckBox>

                <px:PXGroupBox ID="edMonthly2" runat="server" Caption="Schedule On" CommitChanges="True" DataField="MonthlyRecurrenceType2">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay2" runat="server" GroupName="edMonthly2" Value="D" Size="SM"></px:PXRadioButton>                    
                        <px:PXDropDown ID="edMonthlyOnDay2" runat="server" DataField="MonthlyOnDay2" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>                    
                        <px:PXRadioButton ID="rbOnDayOfWeek2" runat="server" GroupName="edMonthly2" Value="W" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnWeek2" runat="server" DataField="MonthlyOnWeek2" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek2" runat="server" DataField="MonthlyOnDayOfWeek2" Size="S" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    </Template>
                </px:PXGroupBox>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Second Recurrence Monthly Settings Section --%>

                <%-- Third Recurrence Monthly Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" GroupCaption="Third Recurrence Monthly Settings"></px:PXLayoutRule>
                <px:PXCheckBox ID="edMonthly3Selected" runat="server" DataField="Monthly3Selected" AlignLeft="True" CommitChanges="True"></px:PXCheckBox>

                <px:PXGroupBox ID="edMonthly3" runat="server" Caption="Schedule On" CommitChanges="True" DataField="MonthlyRecurrenceType3">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay3" runat="server" GroupName="edMonthly3" Value="D" Size="SM"></px:PXRadioButton>                    
                        <px:PXDropDown ID="edMonthlyOnDay3" runat="server" DataField="MonthlyOnDay3" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>                    
                        <px:PXRadioButton ID="rbOnDayOfWeek3" runat="server" GroupName="edMonthly3" Value="W" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnWeek3" runat="server" DataField="MonthlyOnWeek3" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek3" runat="server" DataField="MonthlyOnDayOfWeek3" Size="S" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    </Template>
                </px:PXGroupBox>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Third Recurrence Monthly Settings Section --%>

                <%-- Fourth Recurrence Monthly Settings Section --%>
                <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" GroupCaption="Fourth Recurrence Monthly Settings"></px:PXLayoutRule>
                <px:PXCheckBox ID="edMonthly4Selected" runat="server" DataField="Monthly4Selected" AlignLeft="True" CommitChanges="True"></px:PXCheckBox>

                <px:PXGroupBox ID="edMonthly4" runat="server" Caption="Schedule On" CommitChanges="True" DataField="MonthlyRecurrenceType4">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay4" runat="server" GroupName="edMonthly4" Value="D" Size="SM"></px:PXRadioButton>                    
                        <px:PXDropDown ID="edMonthlyOnDay4" runat="server" DataField="MonthlyOnDay4" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>                    
                        <px:PXRadioButton ID="rbOnDayOfWeek4" runat="server" GroupName="edMonthly4" Value="W" Size="SM"></px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnWeek4" runat="server" DataField="MonthlyOnWeek4" Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek4" runat="server" DataField="MonthlyOnDayOfWeek4" Size="S" SuppressLabel="True" AllowNull="False" CommitChanges="True"></px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    </Template>
                </px:PXGroupBox>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Fourth Recurrence Monthly Settings Section --%>

                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Monthly Settings Section --%>
                
                <%-- Weekly Settings Section --%>
                <px:PXLayoutRule runat="server" StartGroup="True" StartRow="True" GroupCaption="Weekly Settings"></px:PXLayoutRule>

                <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS"></px:PXLayoutRule>
                <px:PXNumberEdit ID="edWeeklyFrequency" runat="server" Size="XS" DataField="WeeklyFrequency" CommitChanges="True"></px:PXNumberEdit>
                <px:PXTextEdit ID="edWeeklyLabel" runat="server" DataField="WeeklyLabel" Size="S" SuppressLabel="True" SkinID="Label" Enabled="False"></px:PXTextEdit>
                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>

                <px:PXLayoutRule runat="server" StartRow="True" SuppressLabel="True" ControlSize="XS"></px:PXLayoutRule>
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" SuppressLabel="True" ControlSize="XS"></px:PXLayoutRule>
                <px:PXCheckBox ID="chkWeeklyOnSun" runat="server" DataField="WeeklyOnSun" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkWeeklyOnMon" runat="server" DataField="WeeklyOnMon" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkWeeklyOnTue" runat="server" DataField="WeeklyOnTue" CommitChanges="True"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XS"></px:PXLayoutRule>
                <px:PXCheckBox ID="chkWeeklyOnWed" runat="server" DataField="WeeklyOnWed" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkWeeklyOnThu" runat="server" DataField="WeeklyOnThu" CommitChanges="True"></px:PXCheckBox>
                <px:PXCheckBox ID="chkWeeklyOnFri" runat="server" DataField="WeeklyOnFri" CommitChanges="True"></px:PXCheckBox>
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XS"></px:PXLayoutRule>
                <px:PXCheckBox ID="chkWeeklyOnSat" runat="server" DataField="WeeklyOnSat" CommitChanges="True"></px:PXCheckBox>

                <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                <%-- Weekly Settings Section --%>

                <%-- Daily Settings Section --%>
                <px:PXLayoutRule runat="server" GroupCaption="Daily Settings" StartGroup="True"></px:PXLayoutRule>
                <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS" ControlSize="XS"></px:PXLayoutRule>
                <px:PXNumberEdit ID="edDailyFrequency" runat="server" DataField="DailyFrequency" CommitChanges="True"></px:PXNumberEdit>
                <px:PXTextEdit SuppressLabel="True" ID="edDailyLabel" DataField="DailyLabel" runat="server" Size="S" SkinID="Label" Enabled="False"></px:PXTextEdit>
                <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>
                <%-- Daily Settings Section --%>
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
        <px:PXTabItem Text="Forecast">
            <Template>       
                <px:PXFormView ID="FromToFilterForm" runat="server" DataMember="FromToFilter" DataSourceID="ds" SkinID="Transparent" Width="100%" TabIndex="5400">
                    <Template>
                        <px:PXLayoutRule runat="server" ColumnWidth="XS" LabelsWidth="XXS" StartColumn="True" StartRow="True">
                        </px:PXLayoutRule>
                        <px:PXDateTimeEdit ID="edDateBegin" runat="server" CommitChanges="True" DataField="DateBegin">
                        </px:PXDateTimeEdit>
                        <px:PXLayoutRule runat="server" ColumnWidth="XS" LabelsWidth="XXS" StartColumn="True">
                        </px:PXLayoutRule>
                        <px:PXDateTimeEdit ID="edDateEnd" runat="server" CommitChanges="True" DataField="DateEnd">
                        </px:PXDateTimeEdit>
                    </Template>
                </px:PXFormView>                                 
                <px:PXGrid ID="ScheduleProjectionDates" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" 
                    AllowPaging="True" AdjustPageSize="Auto" Height="200px" TabIndex="11300" FilesIndicator="False" NoteIndicator="False">
                    <ActionBar>
                                <Actions>
                                    <AddNew Enabled="False" />
                                    <Delete Enabled="False" />
                                </Actions>
                    </ActionBar>
                    <Levels>
                        <px:PXGridLevel DataKeyNames="Date" DataMember="ScheduleProjectionRecords">
                            <RowTemplate>
                                <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="True">
                                </px:PXDateTimeEdit>
                                <px:PXTextEdit ID="edDayOfWeek" runat="server" DataField="DayOfWeek">
                                </px:PXTextEdit>
                                <px:PXTextEdit ID="edWeekOfYear" runat="server" DataField="WeekOfYear">
                                </px:PXTextEdit>
                                <px:PXDateTimeEdit ID="edBeginDateOfWeek2" runat="server" DataField="BeginDateOfWeek" Size="M">
                                </px:PXDateTimeEdit>
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="Date" CommitChanges="True">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="DayOfWeek">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="WeekOfYear">
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="BeginDateOfWeek">
                                </px:PXGridColumn>
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                <AutoSize Enabled="True" MinHeight="200" ></AutoSize>                    
                </px:PXGrid>                                        
            </Template>
        </px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True"/>
	</px:PXTab>
</asp:Content>