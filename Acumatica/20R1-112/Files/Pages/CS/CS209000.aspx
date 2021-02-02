<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS209000.aspx.cs" Inherits="Page_CS209000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.CSCalendarMaint" PrimaryView="Calendar">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Calendar" Caption="Calendar Summary">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXDropDown ID="edTimeZone" runat="server" DataField="TimeZone" ValueField="Value" TextField="Text" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="250px" DataSourceID="ds" DataMember="CalendarDetails">
		<Items>
			<px:PXTabItem Text="Calendar Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XXS" ControlSize="XXS" />
					<px:PXLabel ID="LL1" runat="server"> Day of Week </px:PXLabel>
					<px:PXCheckBox CommitChanges="True" ID="chkSunWorkDay" runat="server" DataField="SunWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkMonWorkDay" runat="server" DataField="MonWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkTueWorkDay" runat="server" DataField="TueWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkWedWorkDay" runat="server" DataField="WedWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkThuWorkDay" runat="server" DataField="ThuWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkFriWorkDay" runat="server" DataField="FriWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkSatWorkDay" runat="server" DataField="SatWorkDay" AlignLeft="True" SuppressLabel="True" />
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
					<px:PXLabel ID="LL2" runat="server">Start time </px:PXLabel>
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edSunStartTime" runat="server" DataField="SunStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edMonStartTime" runat="server" DataField="MonStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edTueStartTime" runat="server" DataField="TueStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edWedStartTime" runat="server" DataField="WedStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edThuStartTime" runat="server" DataField="ThuStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edFriStartTime" runat="server" DataField="FriStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edSatStartTime" runat="server" DataField="SatStartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					
                    <px:PXLayoutRule ID="PXUnpaidTimeLayoutRule" runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
                    <px:PXLabel ID="PXUnpaidTimeLabel" runat="server">Unpaid Break Time </px:PXLabel>
                    <px:PXMaskEdit Size="s" ID="edSunUnpaidTime" runat="server" DataField="SunUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />
                    <px:PXMaskEdit Size="s" ID="edMonUnpaidTime" runat="server" DataField="MonUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />
                    <px:PXMaskEdit Size="s" ID="edTueUnpaidTime" runat="server" DataField="TueUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />
                    <px:PXMaskEdit Size="s" ID="edWedUnpaidTime" runat="server" DataField="WedUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />
                    <px:PXMaskEdit Size="s" ID="edThuUnpaidTime" runat="server" DataField="ThuUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />
                    <px:PXMaskEdit Size="s" ID="edFriUnpaidTime" runat="server" DataField="FriUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />
                    <px:PXMaskEdit Size="s" ID="edSatUnpaidTime" runat="server" DataField="SatUnpaidTime" SuppressLabel="True" Width="48" CommitChanges="true" />

                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
					<px:PXLabel ID="LL3" runat="server">End time </px:PXLabel>
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edSunEndTime" runat="server" DataField="SunEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edMonEndTime" runat="server" DataField="MonEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edTueEndTime" runat="server" DataField="TueEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edWedEndTime" runat="server" DataField="WedEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edThuEndTime" runat="server" DataField="ThuEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edFriEndTime" runat="server" DataField="FriEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
					<px:PXDateTimeEdit Size="s" CommitChanges="True" ID="edSatEndTime" runat="server" DataField="SatEndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
                    <px:PXLabel ID="PXLabel2" runat="server">Goods Are Moved </px:PXLabel>
					<px:PXCheckBox ID="chkSunGoodsMoves" runat="server" DataField="SunGoodsMoves" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox ID="chkMonGoodsMoves" runat="server" DataField="MonGoodsMoves" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox ID="chkTueGoodsMoves" runat="server" DataField="TueGoodsMoves" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox ID="chkWedGoodsMoves" runat="server" DataField="WedGoodsMoves" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox ID="chkThuGoodsMoves" runat="server" DataField="ThuGoodsMoves" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox ID="chkFriGoodsMoves" runat="server" DataField="FriGoodsMoves" AlignLeft="True" SuppressLabel="True" />
					<px:PXCheckBox ID="chkSatGoodsMoves" runat="server" DataField="SatGoodsMoves" AlignLeft="True" SuppressLabel="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Exceptions">
				<Template>
					<px:PXFormView ID="formExceptions" runat="server" DataMember="Filter" DataSourceID="ds" Width="100%" RenderStyle="Simple" TabIndex="8500">
						<ContentLayout OuterSpacing="Around" />
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSelector CommitChanges="True" ID="edYearID" runat="server" DataField="YearID" AutoRefresh="True" ValueField="YearID" DataSourceID="ds" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridExceptions" runat="server" DataSourceID="ds" Height="100px" Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True" SkinID="DetailsWithFilter">
						<Levels>
							<px:PXGridLevel DataMember="CSCalendarExceptions">
								<Mode AllowAddNew="True" AllowDelete="True" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" />
									<px:PXDropDown ID="edDayOfWeek" runat="server"  DataField="DayOfWeek" ValueField="Value" TextField="Text" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
									<px:PXCheckBox ID="chkWorkDay" runat="server" DataField="WorkDay" />
									<px:PXDateTimeEdit ID="edStartTime" runat="server" DataField="StartTime" DisplayFormat="t" EditFormat="t" TimeMode="true" />
									<px:PXMaskEdit ID="edUnpaidTime" runat="server" DataField="UnpaidTime" />
									<px:PXDateTimeEdit ID="edEndTime" runat="server" DataField="EndTime" DisplayFormat="t" EditFormat="t" TimeMode="true" />
									<px:PXCheckBox ID="chkGoodsMoved" runat="server" DataField="GoodsMoved" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Date" Width="90px" />
									<px:PXGridColumn DataField="DayOfWeek" TextAlign="Right" Width="74px" />
									<px:PXGridColumn DataField="Description" Width="151px" />
									<px:PXGridColumn DataField="WorkDay" TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn DataField="StartTime" Width="90px" TimeMode="true" />
									<px:PXGridColumn DataField="UnpaidTime" Width="90px" />
									<px:PXGridColumn DataField="EndTime" Width="90px" TimeMode="true" />
									<px:PXGridColumn DataField="GoodsMoved" TextAlign="Center" Type="CheckBox" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowDelete="True" />
					    <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" />
	</px:PXTab>
</asp:Content>
