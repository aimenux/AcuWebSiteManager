<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AR203500.aspx.cs" Inherits="Page_AR203500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
        TypeName="PX.Objects.AR.ARScheduleMaint" PrimaryView="Schedule_Header">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="RunNow" />
            <px:PXDSCallbackCommand Name="ViewDocument" DependOnGrid="grid" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grid2" Name="ViewGenDocument" Visible="False" CommitChanges="True">
            </px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="Schedule_Header" AllowCollapse="true"
        Caption="Schedule Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
        ActivityField="NoteActivity" DefaultControlID="edScheduleID" DataSourceID="ds">
        <Parameters>
            <px:PXQueryStringParam Name="DocType" QueryStringField="DocType" Size="3" Type="String" />
            <px:PXQueryStringParam Name="RefNbr" QueryStringField="RefNbr" Size="15" Type="String" />
        </Parameters>
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
                ControlSize="S" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
            <px:PXSelector ID="PXSelector1" runat="server" DataField="ScheduleID"
                Size="S" DataSourceID="ds" />
            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" SuppressLabel="True" AlignLeft="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Size="S" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate"
                Size="S" />
            <px:PXCheckBox CommitChanges="True" ID="cbNoEndDate" runat="server" DataField="NoEndDate"
                FalseValue="False" TrueValue="True" SuppressLabel="True" AlignLeft="True" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
            <px:PXNumberEdit ID="edRunLimit" runat="server" DataField="RunLimit" Size="S" />
            <px:PXCheckBox CommitChanges="True" ID="chkNoRunLimit" runat="server" DataField="NoRunLimit"
                SuppressLabel="True" AlignLeft="True" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="True"
                GroupCaption="Schedule Type" />
            <px:PXGroupBox ID="gbScheduleType" runat="server" Caption="Schedule Type" CommitChanges="True"
                DataField="FormScheduleType" RenderSimple="True" RenderStyle="Simple" Height="101">
                <Template>
                    <px:PXRadioButton ID="rbDaily" runat="server" GroupName="gbScheduleType"
                        Text="Daily" Value="D" />
                    <px:PXRadioButton ID="rbWeekly" runat="server" GroupName="gbScheduleType"
                        Text="Weekly" Value="W" />
                    <px:PXRadioButton ID="rbMonthly" runat="server" GroupName="gbScheduleType"
                        Text="Monthly" Value="M" />
                    <px:PXRadioButton ID="rbPeriod" runat="server" GroupName="gbScheduleType"
                        Text="By Financial Period" Value="P" />
                </Template>
                <ContentLayout LabelsWidth="S" Layout="Stack" />
            </px:PXGroupBox>

            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="SM"
                ControlSize="M" />
            <px:PXTextEdit ID="edScheduleName" runat="server" DataField="ScheduleName" />
            <px:PXDateTimeEdit ID="edLastRunDate" runat="server" DataField="LastRunDate" Enabled="False"
                Size="S" />
            <px:PXDateTimeEdit ID="edNextRunDate" runat="server" DataField="NextRunDate" Enabled="False"
                Size="S" />
            <px:PXNumberEdit Size="S" ID="edRunCntr" runat="server" DataField="RunCntr" Enabled="False" />

            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" GroupCaption="By Financial Period" StartGroup="True" />
            <px:PXGroupBox ID="gbPeriodically" runat="server" Caption="By Financial Period" CommitChanges="True"
                DataField="PeriodDateSel" RenderStyle="Simple" Width="348">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True" LabelsWidth="XS" ControlSize="SM"  />
                    <px:PXNumberEdit ID="edPeriodFrequency" runat="server" DataField="PeriodFrequency"
                        Size="S">
                    </px:PXNumberEdit>

                    <px:PXTextEdit ID="edPeriods" runat="server" DataField="Periods" Enabled="False" SuppressLabel="True" SkinID="Label" />
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server">
                    </px:PXLayoutRule>
                    <px:PXRadioButton ID="rbStartOfPeriod" runat="server" GroupName="gbPeriodically"
                        Text="Start of Financial Period" Value="S" />
                    <px:PXRadioButton ID="rbEndOfPeriod" runat="server" GroupName="gbPeriodically" Text="End of Financial Period"
                        Value="E" />
                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True">
                    </px:PXLayoutRule>
                    <px:PXRadioButton ID="rbFixedDay" runat="server" GroupName="gbPeriodically" Text="Fixed Day of the Period"
                        Value="D" />
                    <px:PXNumberEdit ID="edPeriodFixedDay" runat="server" DataField="PeriodFixedDay"
                        Size="xxs" SuppressLabel="True">
                    </px:PXNumberEdit>
                    <px:PXLayoutRule ID="PXLayoutRule12" runat="server">
                    </px:PXLayoutRule>
                </Template>
                <ContentLayout LabelsWidth="S" />
            </px:PXGroupBox>
            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" EndGroup="True" />

            <px:PXLayoutRule ID="PXLayoutRule14" runat="server" GroupCaption="Monthly" StartGroup="True" />
            <px:PXGroupBox ID="gbMonthly" runat="server" Caption="Monthly" CommitChanges="True"
                DataField="MonthlyDaySel" RenderStyle="Simple" Width="348">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" LabelsWidth="76" />
                    <px:PXDropDown ID="edMonthlyFrequency" runat="server"
                        DataField="MonthlyFrequency" Size="XXS" AllowNull="False">
                    </px:PXDropDown>
                    <px:PXTextEdit ID="edMonths" runat="server" DataField="Months" SuppressLabel="True" SkinID="Label" Enabled="False" />
                    <px:PXLayoutRule ID="PXLayoutRule16" runat="server" Merge="True" />
                    <px:PXRadioButton ID="rbOnDay" runat="server" GroupName="gbMonthly" Value="D"
                        Size="XS" />
                    <px:PXDropDown ID="edMonthlyOnDay" runat="server" DataField="MonthlyOnDay" Size="XS"
                        SuppressLabel="True" AllowNull="False">
                    </px:PXDropDown>
                    <px:PXLayoutRule ID="PXLayoutRule17" runat="server" Merge="True" />
                    <px:PXRadioButton ID="rbOnDayOfWeek" runat="server" GroupName="gbMonthly"
                        Value="W" Size="XS" />
                    <px:PXDropDown ID="edMonthlyOnWeek" runat="server" DataField="MonthlyOnWeek" Size="XXS"
                        SuppressLabel="True" AllowNull="False">
                    </px:PXDropDown>
                    <px:PXDropDown ID="edMonthlyOnDayOfWeek" runat="server" DataField="MonthlyOnDayOfWeek"
                        Size="xs" SuppressLabel="True" AllowNull="False">
                    </px:PXDropDown>
                </Template>
            </px:PXGroupBox>
            <px:PXLayoutRule ID="PXLayoutRule18" runat="server" EndGroup="true" />

            <px:PXLayoutRule ID="PXLayoutRule19" runat="server" GroupCaption="Weekly" StartGroup="True" />
            <px:PXLayoutRule ID="PXLayoutRule20" runat="server" Merge="True" LabelsWidth="XS" />
            <px:PXNumberEdit Size="XXS" ID="edWeeklyFrequency" runat="server"
                DataField="WeeklyFrequency" />
            <px:PXTextEdit ID="edWeeks" runat="server" DataField="Weeks" SuppressLabel="True" SkinID="Label" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule21" runat="server" />
            <px:PXGroupBox ID="gbWeekly" runat="server" Caption="Weekly" CommitChanges="True" RenderStyle="Simple" DataField="Weeks" Width="348">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule22" runat="server" SuppressLabel="True" ControlSize="XS"
                        StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXCheckBox ID="chkWeeklyOnDay1" runat="server" DataField="WeeklyOnDay1">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="chkWeeklyOnDay2" runat="server" DataField="WeeklyOnDay2">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="chkWeeklyOnDay3" runat="server" DataField="WeeklyOnDay3">
                    </px:PXCheckBox>
                    <px:PXLayoutRule ID="PXLayoutRule23" runat="server">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule ID="PXLayoutRule24" runat="server" ControlSize="XS" StartColumn="True"
                        SuppressLabel="True">
                    </px:PXLayoutRule>
                    <px:PXCheckBox ID="chkWeeklyOnDay4" runat="server" DataField="WeeklyOnDay4">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="chkWeeklyOnDay5" runat="server" DataField="WeeklyOnDay5">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="chkWeeklyOnDay6" runat="server" DataField="WeeklyOnDay6">
                    </px:PXCheckBox>
                    <px:PXLayoutRule ID="PXLayoutRule25" runat="server">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule ID="PXLayoutRule26" runat="server" ControlSize="XS" StartColumn="True"
                        SuppressLabel="True">
                    </px:PXLayoutRule>
                    <px:PXCheckBox ID="chkWeeklyOnDay7" runat="server" DataField="WeeklyOnDay7">
                    </px:PXCheckBox>
                </Template>
            </px:PXGroupBox>
            <px:PXLayoutRule ID="PXLayoutRule27" runat="server" EndGroup="True" />

            <px:PXLayoutRule ID="PXLayoutRule28" runat="server" GroupCaption="Daily" StartGroup="True" />
            <px:PXLayoutRule ID="PXLayoutRule29" runat="server" Merge="True" LabelsWidth="XS" ControlSize="XS" />
            <px:PXNumberEdit Size="S" ID="edDailyFrequency" runat="server" DataField="DailyFrequency" />
            <px:PXTextEdit SuppressLabel="True" ID="edDays" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule30" runat="server" EndGroup="True" />

        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Width="100%">
        <Items>
            <px:PXTabItem Text="Document List">
                <Template>
                    <px:PXGrid ID="grid" runat="server" Height="100%" Width="100%" AllowSearch="true"
                        BorderWidth="0px" SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Document_Detail">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="true"
                                        AllowEdit="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="ARRegister.docType" PropertyName="DataValues[&quot;DocType&quot;]"
                                                Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" />
                                    <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" Enabled="False" />
                                    <px:PXTextEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
                                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
                                    <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" Type="DropDownList" />
                                    <px:PXGridColumn DataField="RefNbr" AutoCallBack="True" LinkCommand="ViewDocument" />
                                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
                                    <px:PXGridColumn DataField="DocDate" />
                                    <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" />
                                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" MatrixMode="True" />
                                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                                    <px:PXGridColumn DataField="DocDesc" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Generated Documents">
                <Template>
                    <px:PXGrid ID="grid2" runat="server" Height="100%" Width="100%" AllowSearch="true"
                        BorderWidth="0px" SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Document_History">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edRefNbr2" runat="server" DataField="RefNbr" Enabled="False" AutoRefresh="true"
                                        AllowEdit="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid2" Name="ARRegister.docType" PropertyName="DataValues[&quot;DocType&quot;]"
                                                Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edCustomerID2" runat="server" DataField="CustomerID" AllowEdit="True" />
                                    <px:PXDateTimeEdit ID="edDocDate2" runat="server" DataField="DocDate" Enabled="False" />
                                    <px:PXTextEdit ID="edFinPeriodID2" runat="server" DataField="FinPeriodID" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryOrigDocAmt2" runat="server" DataField="CuryOrigDocAmt" />
                                    <px:PXSelector ID="edCuryID2" runat="server" DataField="CuryID" />
                                    <px:PXTextEdit ID="edDocDesc2" runat="server" DataField="DocDesc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" Type="DropDownList" />
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewGenDocument" />
                                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
                                    <px:PXGridColumn DataField="Status" Type="DropDownList" />
                                    <px:PXGridColumn DataField="DocDate" />
                                    <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" />
                                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" MatrixMode="True" />
                                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                                    <px:PXGridColumn DataField="DocDesc" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="false" />
                                <Delete Enabled="false" />
                            </Actions>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" MinHeight="250" MinWidth="100" Container="Window" />
    </px:PXTab>
</asp:Content>
