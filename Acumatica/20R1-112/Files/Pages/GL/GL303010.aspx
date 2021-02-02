<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL303010.aspx.cs" Inherits="Page_GL303010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.JournalEntryImport" PrimaryView="Map">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" Visible="False" DependOnGrid="grid" RepaintControlsIDs="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" Visible="False" DependOnGrid="grid" RepaintControlsIDs="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Release" StartNewGroup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"   Width="100%" DataMember="Map" Caption="Import Summary" NoteIndicator="True" FilesIndicator="True"
		ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edImportNbr" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edImportNbr" runat="server" DataField="Number"  />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" SelectedIndex="-1" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="IsHold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDateEntered" runat="server" DataField="ImportDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID"  AutoRefresh="True"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID"  />
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID" AutoRefresh="True" />
			<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" AllowEdit="True"  Enabled="False" />
			<px:PXLabel ID="PXLabel2" runat="server"></px:PXLabel>
			<px:PXLabel ID="PXLabel1" runat="server"></px:PXLabel>
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit CommitChanges="True" ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit CommitChanges="True" ID="edDebitTotal" runat="server" DataField="DebitTotalBalance" Enabled="False" Width="150px" />
			<px:PXNumberEdit CommitChanges="True" ID="edCreditTotal" runat="server" DataField="CreditTotalBalance" Enabled="False" Width="150px" />
			<px:PXNumberEdit CommitChanges="True" ID="edTotal" runat="server" DataField="TotalBalance" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Width="100%">
		<Items>
			<px:PXTabItem Text="Transaction Details">
				<Template>
					<px:PXGrid ID="grid" runat="server"  Width="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" Height="274px">
						<Levels>
							<px:PXGridLevel DataMember="MapDetails" ImportDataMember="ImportTemplate">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
									<px:PXSegmentMask ID="edImportAccountCD" runat="server" DataField="ImportAccountCD" />
                                    <px:PXSegmentMask ID="edImportSubAccountCD" runat="server" DataField="ImportSubAccountCD" />
									<px:PXSegmentMask ID="edMapAccountID" runat="server" DataField="MapAccountID" Enabled="False" />
                                    <px:PXSegmentMask ID="edMapSubAccountID" runat="server" DataField="MapSubAccountID" Enabled="False" />
                                    <px:PXNumberEdit ID="edYtdBalance" runat="server"  DataField="YtdBalance" />
                                    <px:PXNumberEdit ID="edCuryYtdBalance" runat="server"  DataField="CuryYtdBalance" />
									<px:PXDropDown ID="edAccountType" runat="server" DataField="AccountType" Enabled="False" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="ImportAccountCD" CommitChanges="True" />
									<px:PXGridColumn DataField="MapAccountID" />
									<px:PXGridColumn DataField="ImportSubAccountCD" CommitChanges="True" />
									<px:PXGridColumn DataField="MapSubAccountID" />
									<px:PXGridColumn DataField="YtdBalance" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryYtdBalance" TextAlign="Right" AllowShowHide="Server" CommitChanges="True" />
									<px:PXGridColumn DataField="AccountType" />
									<px:PXGridColumn DataField="Description" />
								</Columns>
								<Mode AllowUpdate="True" />
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<pxa:PXGridProcessing ListItems="Validate,Merge Duplicates" DataMember="Operations" DataField="Action" ParameterName="action" TitleWidth="">
									<ActionBar Order="4" />
								</pxa:PXGridProcessing>
							</CustomItems>
						</ActionBar>
						<CallbackCommands>
							<Refresh CommitChanges="True" />
						</CallbackCommands>
						<AutoSize Enabled="True" />
						<Mode AllowFormEdit="True" AllowUpload="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Exceptions" LoadOnDemand="true" BindingContext="form" VisibleExp="DataControls[&quot;edStatus&quot;].Value != 2">
				<Template>
					<px:PXGrid ID="grid2" runat="server"  Height="100%" Width="100%" SkinID="DetailsInTab" AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="Exceptions">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AccountCD" />
									<px:PXGridColumn DataField="SubID" />
									<px:PXGridColumn DataField="Type" />
									<px:PXGridColumn DataField="Description" />
									<px:PXGridColumn DataField="LastActivityPeriod" />
									<px:PXGridColumn DataField="BegBalance" TextAlign="Right" />
									<px:PXGridColumn DataField="PtdDebitTotal" TextAlign="Right" />
									<px:PXGridColumn DataField="PtdCreditTotal" TextAlign="Right" />
									<px:PXGridColumn DataField="EndBalance" TextAlign="Right" />
								</Columns>
								<Mode  AllowAddNew="False" AllowDelete="False" />
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<ActionBar PagerVisible="False">
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
				
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
