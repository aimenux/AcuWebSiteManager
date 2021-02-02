<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205020.aspx.cs" Inherits="Page_SM205020"
	Title="Automation Schedule Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUScheduleMaint"
		PrimaryView="Schedule">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Refresh" Visible="false" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" ClosePopup="false" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Prev" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Next" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="ViewScreen" StartNewGroup="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Graphs" TreeKeys="GraphName,IsNamespace" />
		</DataTrees>
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formSchedule" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Caption="Automation Schedule" DataMember="Schedule" NoteIndicator="True"
		FilesIndicator="True" TemplateContainer="" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edScheduleID" runat="server" DataField="ScheduleID" AutoRefresh="True" TextField="Description" NullText="<NEW>" DataSourceID="ds">
				<AutoCallBack Command="Refresh" Target="ds" />
				<Parameters>
					<px:PXControlParam Name="AUSchedule.screenID" ControlID="formSchedule" PropertyName="DataControls[&quot;edScreenID&quot;].Value" Type="String" Size="8" />
				</Parameters>
			</px:PXSelector>
		    <px:PXLayoutRule runat="server" ColumnSpan="2"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" AlreadyLocalized="False" DefaultLocale=""  />
			<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
            </px:PXLayoutRule>
		    <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" Size="M" AlreadyLocalized="False" />
			<px:PXDropDown ID="edActionName" runat="server" DataField="ActionName" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="formSchedule" Name="AUSchedule.screenID" PropertyName="NewDataKey[&quot;ScreenID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100" Width="100%"
		DataSourceID="ds" DataMember="CurrentSchedule">
		<Items>
		    <px:PXTabItem Text="Details">
		        <Template>
		            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			
			        <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" AllowNull="False" Size="S" />
			
			        <px:PXLayoutRule runat="server" Merge="True" />
			        <px:PXDateTimeEdit Size="S" ID="edEndDate" runat="server" DataField="EndDate" />
			        <px:PXCheckBox CommitChanges="True" ID="chkNoEndDate" runat="server" Checked="True" DataField="NoEndDate" AlignLeft="True" SuppressLabel="True" />
			        <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
			        <px:PXNumberEdit Size="S" ID="edRunLimit" runat="server" DataField="RunLimit" />
			        <px:PXCheckBox CommitChanges="True" ID="chkNoRunLimit" runat="server" DataField="NoRunLimit" AlignLeft="True" SuppressLabel="True" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXNumberEdit ID="edHistoryRetainCount" runat="server" DataField="HistoryRetainCount"/>
                    <px:PXCheckBox CommitChanges="True" ID="chkKeepFullHistory" runat="server" DataField="KeepFullHistory" AlignLeft="True" SuppressLabel="True" />
			        <px:PXLayoutRule runat="server" />
                    <px:PXBranchSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="true" />
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			
			        <px:PXLayoutRule runat="server" Merge="True" />
			        <px:PXNumberEdit Size="S" ID="edRunCntr" runat="server" AllowNull="False" DataField="RunCntr" Enabled="False" />
			        <px:PXLabel Size="xxs" ID="lblTimes" style="margin-left:10px;" runat="server">Times</px:PXLabel>
			        <px:PXLayoutRule runat="server" />

			        <px:PXDateTimeEdit ID="edLastRunDate" runat="server" DataField="LastRunDate" Enabled="False" Size="SM" />
                    <px:PXDateTimeEdit ID="edNextRunDate1" runat="server" DataField="NextRunDateTime" Enabled="False" Size="SM" />

		            <px:PXDropDown ID="edTimeZoneID" runat="server" DataField="TimeZoneID" AllowNull="False" />
		        </Template>
		    </px:PXTabItem>
			<px:PXTabItem Text="Schedule">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" GroupCaption="Schedule Type" />
					<px:PXGroupBox CommitChanges="True" RenderStyle="Simple" ID="gbScheduleType" runat="server" AllowNull="False" Caption="ScheduleType" DataField="ScheduleType" RenderSimple="True">
					    <Template>
					        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" /> 
                            <px:PXRadioButton runat="server" ID="rbDaily" Size="M" GroupName="gbScheduleType" Text="Daily" Value="D"/>
                            <px:PXRadioButton runat="server" ID="rbWeekly" Size="M" GroupName="gbScheduleType" Text="Weekly" Value="W"/>
                            <px:PXRadioButton runat="server" ID="rbMonthly" Size="M" GroupName="gbScheduleType" Text="Monthly" Value="M"/>
                            <px:PXRadioButton runat="server" ID="rbPeriod" Size="M" GroupName="gbScheduleType" Text="By Financial Period" Value="P"/>
					    </Template>
					</px:PXGroupBox>
                    <px:PXLayoutRule runat="server" ControlSize="S" StartColumn="True" GroupCaption="Schedule Details" LabelsWidth="SM" StartGroup="True"/>
                    <px:PXDateTimeEdit ID="edNextRunDate" runat="server" DataField="NextRunDate" AllowNull="False" Size="M" />
					<px:PXLayoutRule runat="server" Merge="True"/>
					<px:PXNumberEdit ID="edPeriodFrequency" runat="server" DataField="PeriodFrequency"/>
                    <px:PXTextEdit ID="lblPeriodFrequencyDH" runat="server" DataField="PeriodLabel" Enabled="False" SuppressLabel="True"/>
					<px:PXLayoutRule runat="server"/>
					<px:PXGroupBox CommitChanges="True" RenderStyle="Simple" ID="gbPeriodically" runat="server" Caption="By Financial Period" DataField="PeriodDateSel" RenderSimple="True" >
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" /> 
							<px:PXRadioButton ID="rbEndOfPeriod" runat="server" Size="M" GroupName="gbPeriodically" Text="End of Financial Period" Value="E" />
							<px:PXRadioButton ID="rbStartOfPeriod" runat="server" Size="M" GroupName="gbPeriodically" Text="Start of Financial Period" Value="S" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXRadioButton ID="rbFixedDay" runat="server" Size="M" GroupName="gbPeriodically" Text="Fixed Day of the Period" Value="D"  />
							<px:PXNumberEdit ID="edPeriodFixedDay" runat="server" DataField="PeriodFixedDay" CommitChanges="True" Size="xxs" SuppressLabel="True" />
							<px:PXLayoutRule runat="server"/> 
						</Template>
					</px:PXGroupBox>
                    
					<px:PXLayoutRule runat="server" Merge="True"/>
					<px:PXDropDown ID="edMonthlyFrequency" runat="server" DataField="MonthlyFrequency" SelectedIndex="-1"/>
                    <px:PXTextEdit ID="lblMonthlyFrequencyDH" runat="server" DataField="MonthlyLabel" Enabled="False" SuppressLabel="True"/>
					<px:PXLayoutRule runat="server" />
					<px:PXGroupBox CommitChanges="True" RenderStyle="Simple" ID="gbMonthly" runat="server" DataField="MonthlyDaySel" Caption="Monthly" RenderSimple="True">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" /> 
							<px:PXLayoutRule runat="server" Merge="True"/>
							<px:PXRadioButton ID="rbOnDay" runat="server" Size="M" GroupName="gbMonthly" Text="On Day" Value="D" />
							<px:PXDropDown ID="edMonthlyOnDay" runat="server" DataField="MonthlyOnDay" SelectedIndex="-1" CommitChanges="True" Size="xxs" SuppressLabel="True" />
							<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM"/>
							<px:PXLayoutRule runat="server" Merge="True"/>
							<px:PXRadioButton ID="rbOnDayOfWeek" runat="server" Size="M" GroupName="gbMonthly" Text="On the" Value="W" />
							<px:PXDropDown ID="edMonthlyOnWeek" runat="server" CommitChanges="True" DataField="MonthlyOnWeek" SelectedIndex="-1" Size="xxs" SuppressLabel="True"/>
							<px:PXDropDown ID="edMonthlyOnDayOfWeek" runat="server" CommitChanges="True" DataField="MonthlyOnDayOfWeek" SelectedIndex="-1" Size="xs" SuppressLabel="True" />
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXGroupBox>
                    
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit ID="edWeeklyFrequency" runat="server" DataField="WeeklyFrequency" />
					<px:PXTextEdit ID="lblWeeklyFrequencyDH" runat="server" DataField="WeeklyLabel" Enabled="False" SuppressLabel="True"/>
					<px:PXLayoutRule runat="server" />
					<px:PXPanel ID="PXPanelWeek" runat="server" RenderSimple="True" RenderStyle="Simple">
						<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
					    <px:PXCheckBox ID="chkWeeklyOnDay1" runat="server" CommitChanges="True" DataField="WeeklyOnDay1" />
					    <px:PXCheckBox ID="chkWeeklyOnDay2" runat="server" CommitChanges="True" DataField="WeeklyOnDay2" />
					    <px:PXCheckBox ID="chkWeeklyOnDay3" runat="server" CommitChanges="True" DataField="WeeklyOnDay3" />
					    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
					    <px:PXCheckBox ID="chkWeeklyOnDay4" runat="server" CommitChanges="True" DataField="WeeklyOnDay4" />
					    <px:PXCheckBox ID="chkWeeklyOnDay5" runat="server" CommitChanges="True" DataField="WeeklyOnDay5" />
					    <px:PXCheckBox ID="chkWeeklyOnDay6" runat="server" CommitChanges="True" DataField="WeeklyOnDay6" />
					    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
					    <px:PXCheckBox ID="chkWeeklyOnDay7" runat="server" CommitChanges="True" DataField="WeeklyOnDay7" />
					</px:PXPanel>

					<px:PXLayoutRule runat="server" Merge="True"  />
					<px:PXNumberEdit ID="edDailyFrequency" runat="server" DataField="DailyFrequency" />
                    <px:PXTextEdit ID="lblDailyFrequencyDH" runat="server" DataField="DailyLabel" Enabled="False" SuppressLabel="True"/>
					<px:PXLayoutRule runat="server" StartRow="True" GroupCaption="Execution Time"/>
			        <px:PXPanel CommitChanges="True" RenderStyle="Simple" ID="gbHours" runat="server" Caption="Execution Time" RenderSimple="True">
					        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" />
					        <px:PXDateTimeEdit CommitChanges="True" ID="edStartTime" runat="server" DataField="StartTime" 
						        DisplayFormat="t" EditFormat="t" TimeMode="True" Size="M" />
					        <px:PXDateTimeEdit CommitChanges="True" ID="edEndTime" runat="server" DataField="EndTime"
						        DisplayFormat="t" EditFormat="t" TimeMode="True" Size="M" />
					        <px:PXMaskEdit ID="edInterval" runat="server" AllowNull="False" DataField="Interval"
						        InputMask="99:99" Text="1" Size="M" CommitChanges="true" />
					        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" />
					        <px:PXDateTimeEdit ID="edNextRunTime" runat="server" DataField="NextRunTime"
						        DisplayFormat="t" EditFormat="t" TimeMode="True" Size="M" />
					        <px:PXCheckBox ID="chkExactTime" runat="server" CommitChanges="True" DataField="ExactTime" />
                    </px:PXPanel>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Conditions">
				<Template>
					<px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" MatrixMode="true">
						<ActionBar>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Filters">
								<Mode InitNewRow="True" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
										AutoCallBack="true" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn DataField="Value2" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Filter Values" BindingContext="tab" VisibleExp="DataItem.FilterVisible = true">
				<Template>
					<px:PXGrid ID="gridFills" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" MatrixMode="true">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Fills">
								<Mode InitNewRow="true" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" AutoCallBack="true" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="IgnoreError" TextAlign="Center" Type="CheckBox"
										Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
