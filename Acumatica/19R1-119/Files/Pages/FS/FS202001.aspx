<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS202001.aspx.cs" Inherits="Page_FS202001" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.StaffContractScheduleEntry" 
        PrimaryView="StaffScheduleRecords">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="OpenStaffContractScheduleProcess" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" FilesIndicator="True"
		Width="100%" DataMember="StaffScheduleRecords" TabIndex="4500" MarkRequired="Dynamic" AllowCollapse="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" 
                LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXSelector runat="server" DataField="RefNbr" ID="edRefNbr">
            </px:PXSelector>
            <px:PXSegmentMask runat="server" DataField="EmployeeID" ID="edEmployeeID" CommitChanges="True">
            </px:PXSegmentMask>
            <px:PXSelector runat="server" DataField="BranchID" ID="edBranchID" AutoRefresh="True" CommitChanges="True">
            </px:PXSelector>
            <px:PXSelector runat="server" DataField="BranchLocationID" ID="edBranchLocationID">
            </px:PXSelector>
            <px:PXTextEdit ID="edStaffScheduleDescription" runat="server" DataField="StaffScheduleDescription">
            </px:PXTextEdit>
            <px:PXDateTimeEdit runat="server" DataField="StartDate_Date" 
                ID="edStartDate_Date" CommitChanges="True" Width="100px">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" Merge="True">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit runat="server" DataField="EndDate_Date" ID="edEndDate_Date" 
                CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXCheckBox ID="edEnableExpirationDate" runat="server" CommitChanges="True" 
                DataField="EnableExpirationDate">
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" GroupCaption="Scheduling Time">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit runat="server" TimeMode="True" DataField="StartTime_Time" 
                ID="edStartTime_Time" CommitChanges="True" Width="100px">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit runat="server" TimeMode="True" DataField="EndTime_Time" 
                ID="edEndTime_Time" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>            
            <px:PXGroupBox ID="ScheduleType" runat="server" Caption="Scheduling Settings"
            DataField="ScheduleType" Width="300px" CommitChanges="True">
            <Template>
                <px:PXRadioButton ID="ScheduleType_op0" runat="server" Text="Availability"
                Value="A" GroupName="ScheduleType" ></px:PXRadioButton>
                <px:PXRadioButton ID="ScheduleType_op1" runat="server" Text="Unavailability"
                Value="U" GroupName="ScheduleType" ></px:PXRadioButton>
            </Template>
            </px:PXGroupBox>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" DataMember="StaffScheduleSelected">
		<Items>
			<px:PXTabItem Text="Recurrence">
			    <Template>
                    <px:PXLayoutRule runat="server" StartRow = "True" StartGroup="True" 
                        ControlSize="XM" StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" Merge="True">
                    </px:PXLayoutRule>
                    <px:PXGroupBox ID="edFrequencyType" runat="server" Caption="Frequency Settings" 
                        CommitChanges="True" DataField="FrequencyType" Width="200px">
                        <Template>
                            <px:PXRadioButton ID="edFrequencyType_op0" runat="server" 
                                GroupName="edFrequencyType" Text="Daily" Value="D" />
                            <px:PXRadioButton ID="edFrequencyType_op1" runat="server" 
                                GroupName="edFrequencyType" Text="Weekly" Value="W" />
                            <px:PXRadioButton ID="edFrequencyType_op2" runat="server" 
                                GroupName="edFrequencyType" Text="Monthly" Value="M" />
                            <px:PXRadioButton ID="edFrequencyType_op3" runat="server" 
                                GroupName="edFrequencyType" Text="Yearly" Value="A" />
                        </Template>
                        <ContentLayout Layout="Stack" OuterSpacing="Horizontal" LabelsWidth="S" />
                    </px:PXGroupBox>
                    <px:PXTextEdit ID="edRecurrenceDescription" runat="server" CommitChanges="True" 
                        DataField="RecurrenceDescription" Enabled="False" Height="120px" 
                        LabelWidth="150px" SuppressLabel="True" TextAlign="Center" TextMode="MultiLine">
                    </px:PXTextEdit>
                    <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>                                         

                    <px:PXLayoutRule runat="server" StartRow = "True" StartGroup="True" StartColumn="True" GroupCaption="Yearly Settings"></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="76" ></px:PXLayoutRule>
                    <px:PXNumberEdit ID="edAnnualFrequency" runat="server" 
                        DataField="AnnualFrequency" Size="XXS" CommitChanges="True"></px:PXNumberEdit>
                    <px:PXTextEdit SuppressLabel="True" ID="edYears" DataField="Years" 
                        runat="server" SkinID="Label" Enabled="False" CommitChanges="True" Size="S"></px:PXTextEdit>                                              
                    <px:PXLayoutRule runat="server" Merge="True" ></px:PXLayoutRule>
                    <px:PXGroupBox ID="PXGroupBox1" runat="server" Caption="Yearly" 
                        CommitChanges="True" RenderStyle="Simple" DataField="Years"
                         Width="500px" Height="80px" RenderSimple="True">
                        <Template>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkAnnualOnJan" runat="server" DataField="AnnualOnJan" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXCheckBox ID="chkAnnualOnFeb" runat="server" DataField="AnnualOnFeb" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXCheckBox ID="chkAnnualOnMar" runat="server" DataField="AnnualOnMar"  
                                CommitChanges="True"></px:PXCheckBox> 
                            <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkAnnualOnApr" runat="server" DataField="AnnualOnApr" 
                                CommitChanges="True"></px:PXCheckBox> 
                            <px:PXCheckBox ID="chkAnnualOnMay" runat="server" DataField="AnnualOnMay" 
                                CommitChanges="True"></px:PXCheckBox>     
                            <px:PXCheckBox ID="chkAnnualOnJun" runat="server" DataField="AnnualOnJun" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkAnnualOnJul" runat="server" DataField="AnnualOnJul" 
                                CommitChanges="True"></px:PXCheckBox> 
                            <px:PXCheckBox ID="chkAnnualOnAug" runat="server" DataField="AnnualOnAug" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXCheckBox ID="chkAnnualOnSep" runat="server" DataField="AnnualOnSep" 
                                CommitChanges="True"></px:PXCheckBox> 
                            <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True"> </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkAnnualOnOct" runat="server" DataField="AnnualOnOct" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXCheckBox ID="chkAnnualOnNov" runat="server" DataField="AnnualOnNov" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXCheckBox ID="chkAnnualOnDec" runat="server" DataField="AnnualOnDec" 
                                CommitChanges="True"></px:PXCheckBox>
                            <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox> 
                    <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                    <px:PXGroupBox ID="edAnnually" runat="server" Caption="Schedule On" 
                        CommitChanges="True" DataField="AnnualRecurrenceType">
                        <Template>
                             <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                            <px:PXRadioButton ID="rbOnDay5" runat="server" GroupName="edAnnually" Value="D" Size="SM" >
                            </px:PXRadioButton>
                            <px:PXDropDown ID="edAnnualOnDay" runat="server" DataField="AnnualOnDay" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                            <px:PXRadioButton ID="rbOnDayOfWeek5" runat="server" GroupName="edAnnually" Value="W" Size="SM" >
                            </px:PXRadioButton>
                            <px:PXDropDown ID="edAnnualOnWeek" runat="server" DataField="AnnualOnWeek" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXDropDown ID="edAnnualOnDayOfWeek" runat="server" 
                                DataField="AnnualOnDayOfWeek" Size="S" SuppressLabel="True" AllowNull="False" 
                                CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>

                    <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" StartColumn="True" GroupCaption="Monthly Settings"></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS"></px:PXLayoutRule>
                    <px:PXDropDown ID="edMonthlyFrequency" runat="server" 
                        DataField="MonthlyFrequency" Size="XXS" AllowNull="False" CommitChanges="True">
                    </px:PXDropDown>
                    <px:PXTextEdit ID="edMonths1" runat="server" DataField="Months" 
                        SuppressLabel="True" SkinID="Label" Enabled="False" CommitChanges="True"></px:PXTextEdit>                                                            
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    <px:PXGroupBox ID="edMonthly1" runat="server" Caption="Schedule On" 
                        CommitChanges="True" DataField="MonthlyRecurrenceType1">
                        <Template>
                            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                            <px:PXRadioButton ID="rbOnDay1" runat="server" GroupName="edMonthly1" Value="D" Size="SM" >
                            </px:PXRadioButton>
                            <px:PXDropDown ID="edMonthlyOnDay1" runat="server" DataField="MonthlyOnDay1" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                            <px:PXRadioButton ID="rbOnDayOfWeek1" runat="server" GroupName="edMonthly1" Value="W" Size="SM" >
                            </px:PXRadioButton>
                            <px:PXDropDown ID="edMonthlyOnWeek1" runat="server" DataField="MonthlyOnWeek1" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXDropDown ID="edMonthlyOnDayOfWeek1" runat="server" 
                                DataField="MonthlyOnDayOfWeek1" Size="S" SuppressLabel="True" 
                                AllowNull="False" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>

                    <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" StartColumn="True" GroupCaption="Second Recurrence Monthly Settings"></px:PXLayoutRule>
                    <px:PXCheckBox ID="edMonthly2Selected" runat="server" DataField="Monthly2Selected" AlignLeft="True" CommitChanges="True">
                    </px:PXCheckBox>
                    <px:PXGroupBox ID="edMonthly2" runat="server" Caption="Schedule On" 
                        CommitChanges="True" DataField="MonthlyRecurrenceType2">
                        <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay2" runat="server" GroupName="edMonthly2" Value="D" Size="SM" >
                        </px:PXRadioButton>                    
                        <px:PXDropDown ID="edMonthlyOnDay2" runat="server" DataField="MonthlyOnDay2" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>                    
                        <px:PXRadioButton ID="rbOnDayOfWeek2" runat="server" GroupName="edMonthly2" Value="W" Size="SM" >
                        </px:PXRadioButton>                                    
                        <px:PXDropDown ID="edMonthlyOnWeek2" runat="server" DataField="MonthlyOnWeek2" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek2" runat="server" 
                                DataField="MonthlyOnDayOfWeek2" Size="S" SuppressLabel="True" 
                                AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>

                    <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" StartColumn="True" GroupCaption="Third Recurrence Monthly Settings"></px:PXLayoutRule>
                    <px:PXCheckBox ID="edMonthly3Selected" runat="server" DataField="Monthly3Selected" AlignLeft="True" CommitChanges="True">
                    </px:PXCheckBox>
                    <px:PXGroupBox ID="edMonthly3" runat="server" Caption="Schedule On" 
                        CommitChanges="True" DataField="MonthlyRecurrenceType3">
                        <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay3" runat="server" GroupName="edMonthly3" Value="D" Size="SM" >
                        </px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnDay3" runat="server" DataField="MonthlyOnDay3" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDayOfWeek3" runat="server" GroupName="edMonthly3" Value="W" Size="SM" >
                        </px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnWeek3" runat="server" DataField="MonthlyOnWeek3" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek3" runat="server" 
                                DataField="MonthlyOnDayOfWeek3" Size="S" SuppressLabel="True" 
                                AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>

                    <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" StartColumn="True" GroupCaption="Fourth Recurrence Monthly Settings"></px:PXLayoutRule>
                    <px:PXCheckBox ID="edMonthly4Selected" runat="server" DataField="Monthly4Selected" AlignLeft="True" CommitChanges="True">
                    </px:PXCheckBox>
                    <px:PXGroupBox ID="edMonthly4" runat="server" Caption="Schedule On" 
                        CommitChanges="True" DataField="MonthlyRecurrenceType4">
                        <Template>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDay4" runat="server" GroupName="edMonthly4" Value="D" Size="SM" >
                        </px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnDay4" runat="server" DataField="MonthlyOnDay4" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                        <px:PXRadioButton ID="rbOnDayOfWeek4" runat="server" GroupName="edMonthly4" Value="W" Size="SM" >
                        </px:PXRadioButton>
                        <px:PXDropDown ID="edMonthlyOnWeek4" runat="server" DataField="MonthlyOnWeek4" 
                                Size="XS" SuppressLabel="True" AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXDropDown ID="edMonthlyOnDayOfWeek4" runat="server" 
                                DataField="MonthlyOnDayOfWeek4" Size="S" SuppressLabel="True" 
                                AllowNull="False" CommitChanges="True">
                        </px:PXDropDown>
                        <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                
                    <px:PXLayoutRule runat="server" GroupCaption="Weekly Settings" StartGroup="True" ></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS" ></px:PXLayoutRule>
                    <px:PXNumberEdit ID="edWeeklyFrequency" runat="server" Size="XXS" 
                        DataField="WeeklyFrequency" CommitChanges="True" >
                    </px:PXNumberEdit>
                    <px:PXTextEdit ID="edWeeks" runat="server" DataField="Weeks" 
                        SuppressLabel="True" SkinID="Label" Enabled="False" CommitChanges="True" Size="S" >
                    </px:PXTextEdit>
                    <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                    <px:PXGroupBox ID="edWeekly" runat="server" Caption="Weekly" 
                        CommitChanges="True" RenderStyle="Simple" DataField="Weeks" Width="348px" 
                        RenderSimple="True">
                        <Template>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True">
                            </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkWeeklyOnSun" runat="server" DataField="WeeklyOnSun" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="chkWeeklyOnMon" runat="server" DataField="WeeklyOnMon" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="chkWeeklyOnTue" runat="server" DataField="WeeklyOnTue" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXLayoutRule runat="server">
                            </px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True">
                            </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkWeeklyOnWed" runat="server" DataField="WeeklyOnWed" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="chkWeeklyOnThu" runat="server" DataField="WeeklyOnThu" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="chkWeeklyOnFri" runat="server" DataField="WeeklyOnFri" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                            <px:PXLayoutRule runat="server">
                            </px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XS" StartColumn="True">
                            </px:PXLayoutRule>
                            <px:PXCheckBox ID="chkWeeklyOnSat" runat="server" DataField="WeeklyOnSat" 
                                CommitChanges="True">
                            </px:PXCheckBox>
                        </Template>
                    </px:PXGroupBox>  
                    <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>                

                    <px:PXLayoutRule runat="server" GroupCaption="Daily Settings" StartGroup="True" ></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="XS" ControlSize="XXS" ></px:PXLayoutRule>
                    <px:PXNumberEdit ID="edDailyFrequency" runat="server" 
                        DataField="DailyFrequency" CommitChanges="True" Size="XXS"></px:PXNumberEdit>
                    <px:PXTextEdit SuppressLabel="True" ID="edDays" DataField="Days" runat="server" 
                        SkinID="Label" Enabled="False" CommitChanges="True" Size="S"></px:PXTextEdit>
                    <px:PXLayoutRule runat="server" EndGroup="True" ></px:PXLayoutRule>  
                </Template>                                                                        
		</px:PXTabItem>			
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
