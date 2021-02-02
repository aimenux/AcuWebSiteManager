<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA206000.aspx.cs"
	Inherits="Page_FA206000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.FABookYearSetupMaint" PrimaryView="FiscalYearSetup">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="autoFill" CommitChanges="True" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Calendar Summary" DataMember="FiscalYearSetup"
		NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXSelector runat="server" DataField="BookID" ID="edBookID" />
			<px:PXTextEdit runat="server" Enabled="False" DataField="FirstFinYear" AllowNull="False" ID="edFirstFinYear" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="BegFinYear" ID="edBegFinYear" />
			<px:PXCheckBox CommitChanges="True" runat="server" DataField="BelongsToNextYear" ID="chkBelongsToNextYear" />
			<px:PXDropDown CommitChanges="True" ID="edPeriodType" runat="server" AllowNull="False" DataField="PeriodType" />
			<px:PXDropDown OnValueChange="Commit" ID="edEndYearDayOfWeek" runat="server" DataField="EndYearDayOfWeek" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edPeriodsStartDate" runat="server" DataField="PeriodsStartDate" />
			<px:PXCheckBox CommitChanges="True" ID="chAdjustToStart" runat="server" DataField="AdjustToPeriodStart" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXNumberEdit CommitChanges="True" runat="server" DataField="FinPeriods" ID="edFinPeriods" />
			<px:PXNumberEdit ID="edPeriodLength" runat="server" DataField="PeriodLength" />
			<px:PXCheckBox CommitChanges="True" ID="chkHasAdjustmentPeriod" runat="server" DataField="HasAdjustmentPeriod" />
			<px:PXCheckBox CommitChanges="True" runat="server" DataField="UserDefined" ID="chkUserDefined" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Periods">
		<Levels>
			<px:PXGridLevel DataMember="Periods">
				<Columns>
					<px:PXGridColumn AllowUpdate="False" DataField="PeriodNbr" DisplayFormat="&gt;aa" Label="PeriodNbr" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="StartDateUI" Label="Start Date" />
					<px:PXGridColumn AllowUpdate="False" DataField="EndDateUI" Label="End Date" />
					<px:PXGridColumn DataField="Descr" Label="Description" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
