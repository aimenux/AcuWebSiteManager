<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL404000.aspx.cs" Inherits="Page_GL404000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.AccountByPeriodEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="previousperiod" HideText="True"/>
			<px:PXDSCallbackCommand CommitChanges="True" Name="nextperiod" HideText="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="Reclassify" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ReclassifyAll"/>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ReclassificationHistory" StateColumn="IncludedInReclassHistory"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"   Width="100%"
		Caption="Selection" DataMember="Filter" DefaultControlID="edLedgerID" DataSourceID="ds" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edOrganizationID" runat="server" DataField="OrganizationID"/>
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" Autorefresh="true"/>
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID" Autorefresh="true"/>
			<px:PXSelector CommitChanges="True" ID="edStartPeriodID" runat="server" DataField="StartPeriodID" AutoRefresh="True" />
			<px:PXSelector CommitChanges="True" ID="edEndPeriodID" runat="server" DataField="EndPeriodID" Autorefresh="True"/>
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID"/>
			<px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" SelectMode="Segment"  />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDateUI" runat="server" DataField="StartDateUI" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edPeriodStartDate" runat="server" DataField="PeriodStartDateUI" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDateUI" runat="server" DataField="EndDateUI" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edPeriodEndDateUI" runat="server" DataField="PeriodEndDateUI" />
			<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowSummary" runat="server" DataField="ShowSummary" />
			<px:PXCheckBox CommitChanges="True" ID="chkIncludeUnposted" runat="server" DataField="IncludeUnposted" />
			<px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" />
			<px:PXCheckBox CommitChanges="True" ID="chkIncludeReclassified" runat="server" DataField="IncludeReclassified" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowCuryDetail" runat="server" DataField="ShowCuryDetail" />
			<px:PXCheckBox CommitChanges="True" ID="chkUseMasterCalendar" runat="server" DataField="UseMasterCalendar" />
			
			<px:PXLayoutRule runat="server" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXNumberEdit ID="edBegBal" runat="server" DataField="BegBal">
			</px:PXNumberEdit>
			<px:PXNumberEdit ID="edTurnOver" runat="server" DataField="TurnOver">
			</px:PXNumberEdit>
			<px:PXNumberEdit ID="edEndBal" runat="server" DataField="EndBal">
			</px:PXNumberEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Height="150px"
		Width="100%" AllowPaging="True" AdjustPageSize="Auto" Caption="Summary By Period" SyncPosition ="True" FastFilterFields="TranDesc,RefNbr,"
		BatchUpdate="True" AllowSearch="True" SkinID="PrimaryInquire" RestrictFields="True" DataSourceID="ds" TabIndex="100" PreserveSortsAndFilters="False">
		<CallbackCommands>
			<Refresh RepaintControlsIDs="form"/>
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False"  />
		<Levels>
			<px:PXGridLevel DataMember="GLTranEnq">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" AllowCheckAll="True" AllowShowHide="Server" CommitChanges="True"/>
					<px:PXGridColumn DataField="Module" Width="100px" />
					<px:PXGridColumn DataField="BatchNbr" Width="100px" LinkCommand="ViewBatch" />
					<px:PXGridColumn DataField="TranDate" Width="100px" />
					<px:PXGridColumn DataField="FinPeriodID" Width="100px" />
					<px:PXGridColumn DataField="TranPeriodID" Width="100px" />
					<px:PXGridColumn DataField="TranDesc" Width="224px" />
					<px:PXGridColumn DataField="RefNbr" Width="100px" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="100px"  />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="AccountID" Width="108px" />
					<px:PXGridColumn DataField="SubID" Width="198px" />
					<px:PXGridColumn DataField="SignBegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DebitAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CreditAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignEndBalance" TextAlign="Right" MatrixMode="True" Width="100px" />
					<px:PXGridColumn DataField="CuryID"  AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryBegBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryEndBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="InventoryID" Width="120px" />
					<px:PXGridColumn DataField="ReferenceID" Width="120px" />
					<px:PXGridColumn DataField="ReferenceID_BaccountR_AcctName" Width="200px" />
					<px:PXGridColumn DataField="ReclassBatchNbr" TextAlign="Right" Width="120px" AllowShowHide="Server" LinkCommand="ViewReclassBatch" />
					<px:PXGridColumn DataField="IncludedInReclassHistory" AllowShowHide="False" Visible="false" SyncVisible="false" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar DefaultAction="DoubleClick" />
	</px:PXGrid>
</asp:Content>