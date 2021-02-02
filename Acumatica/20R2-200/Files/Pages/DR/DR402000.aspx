<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR402000.aspx.cs"
	Inherits="Page_DR402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.DR.ScheduleTransInq"
		PrimaryView="Filter" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXDropDown CommitChanges="True" ID="edAccountType" runat="server" DataField="AccountType" />
			<px:PXSelector CommitChanges="True" ID="edDeferredCode" runat="server" DataField="DeferredCode" AutoRefresh="true" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" InitialExpandLevel="0" />
			<px:PXSelector CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXCheckBox DataField="UseMasterCalendar" ID="chkUseMasterCalendar" runat="server" AlignLeft="True" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Deferral Transactions"
		AllowPaging="True" RestrictFields="True" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Records">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector ID="edScheduleID" runat="server" DataField="ScheduleID" />
					<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
					<px:PXDateTimeEdit ID="edRecDate" runat="server" DataField="RecDate" />
					<px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" />
					<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
					<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
					<px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" InputMask="##-####" />
					<px:PXTextEdit ID="edDRScheduleDetail__DefCode" runat="server" DataField="DRScheduleDetail__DefCode" Enabled="False" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="ScheduleID" TextAlign="Right" LinkCommand ="ViewSchedule" />
					<px:PXGridColumn DataField="DRScheduleDetail__DefCode" />
					<px:PXGridColumn DataField="DRScheduleDetail__BranchID" />
					<px:PXGridColumn DataField="DRScheduleDetail__ComponentID" />
					<px:PXGridColumn DataField="RecDate" />
					<px:PXGridColumn DataField="TranDate" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="Amount" TextAlign="Right" />
					<px:PXGridColumn DataField="AccountID" />
					<px:PXGridColumn DataField="SubID" />
					<px:PXGridColumn DataField="DRScheduleDetail__DocType" />
					<px:PXGridColumn DataField="DRScheduleDetail__RefNbr" LinkCommand="ViewDoc" />
					<px:PXGridColumn DataField="DRScheduleDetail__LineNbr" />
					<px:PXGridColumn DataField="DRScheduleDetail__BAccountID" />
					<px:PXGridColumn DataField="BatchNbr" LinkCommand="ViewBatch" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
