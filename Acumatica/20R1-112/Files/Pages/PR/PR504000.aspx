<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR504000.aspx.cs" Inherits="Pages_PR_PR504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="server">
	<style type="text/css">
		div#aatrix-webform-overlay {
			z-index: unset !important;
		}
	</style>

	<script src="https://webforms.aatrix.com/public/external.js"></script>
	<script type="text/javascript">
		function runReportClickHandler(sender, context) {
			var ds = px_alls.ds;
			try {
				var reportName = px_alls.edHiddenFormName.getValue();
				if (!reportName) {
					ds.executeCallback("OnRunReportError", errSetup);
					return;
				}

				var reportingPeriod = px_alls.edHiddenReportingPeriod.getValue();
				if (!reportingPeriod) {
					ds.executeCallback("OnRunReportError", errSetup);
					return;
				}

				if (!validateSmartPanelInput(ds, reportingPeriod)) {
					return;
				}

				var ein = px_alls.edHiddenEin.getValue();
				if (!ein) {
					ds.executeCallback("OnRunReportError", errEinMissing);
					return;
				}

				var aatrixVendorID = px_alls.edHiddenAatrixVendorID.getValue();
				if (!aatrixVendorID) {
					ds.executeCallback("OnRunReportError", errAatrixVendorIDMissing);
					return;
				}

				Aatrix.WebForms.Ui.Integration.ClientSupport.ShowUiNew(
					ein,
					reportName,
					aatrixVendorID,
					onUploadPermissionGranted,
					onComplete);

				ds.executeCallback("StartAufGeneration");
				px_alls.pnlRunReport.hide();
			}
			catch (err) {
				ds.executeCallback("OnRunReportError", errException);
				return;
			}
		}

		function commandPerformed(ds, context) {
			if (context.command == "viewHistory") {
				try {
					var ein = px_alls.edHiddenEin.getValue();
					if (!ein) {
						ds.executeCallback("OnRunReportError", errEinMissing);
						return;
					}

					var aatrixVendorID = px_alls.edHiddenAatrixVendorID.getValue();
					if (!aatrixVendorID) {
						ds.executeCallback("OnRunReportError", errAatrixVendorIDMissing);
						return;
					}

					Aatrix.WebForms.Ui.Integration.ClientSupport.ShowUiExisting(
						ein,
						aatrixVendorID,
						onComplete);
				}
				catch (err) {
					ds.executeCallback("OnRunReportError", errException);
					return;
				}
			}
		}

		function validateSmartPanelInput(ds, reportingPeriod) {
			switch (reportingPeriod) {
				case reportingPeriodAnnual:
					if (px_alls.edYear.getValue() == null) {
						ds.executeCallback("OnRunReportError", errYearMissing);
						return false;
					}
					break;

				case reportingPeriodQuarterly:
					if (px_alls.edYear.getValue() == null) {
						ds.executeCallback("OnRunReportError", errYearMissing);
						return false;
					}
					if (px_alls.edQuarter.getValue() == null) {
						ds.executeCallback("OnRunReportError", errQuarterMissing);
						return false;
					}
					break;

				case reportingPeriodMonthly:
					if (px_alls.edYear.getValue() == null) {
						ds.executeCallback("OnRunReportError", errYearMissing);
						return false;
					}
					if (px_alls.edMonth.getValue() == null) {
						ds.executeCallback("OnRunReportError", errMonthMissing);
						return false;
					}
					break;

				case reportingPeriodDateRange:
					var dateFrom = px_alls.edDateFrom.getValue();
					var dateTo = px_alls.edDateTo.getValue();
					if (dateFrom == null) {
						ds.executeCallback("OnRunReportError", errDateFromMissing);
						return false;
					}
					if (dateTo == null) {
						ds.executeCallback("OnRunReportError", errDateToMissing);
						return false;
					}

					if (dateFrom > dateTo) {
						ds.executeCallback("OnRunReportError", errDateInconsistent);
						return false;
					}
					break;
			}

			return true;
		}

		function onUploadPermissionGranted(wfSessionId) {
			px_alls.ds.executeCallback("OnUploadPermissionGranted", wfSessionId + sessionIdSeparator + downloadAuf);
		}

		function onComplete(completeStatus, paymentResponse) {
		}

		window.onbeforeunload = function () {
			px_alls.ds.executeCallback("OnNavigateAway");
		}
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRGovernmentReportingProcess" PrimaryView="Filter">
		<ClientEvents CommandPerformed="commandPerformed" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="RunReport" Visible="False" DependOnGrid="formGrid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="server">
	<px:PXFormView ID="filterForm" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
				<px:PXBranchSelector ID="edBranchID" runat="server" DataField="OrgBAccountID" CommitChanges="True" />
				<px:PXCheckBox ID="edFederalOnly" runat="server" DataField="FederalOnly" CommitChanges="True" />
				<px:PXSelector ID="edState" runat="server" DataField="State" CommitChanges="True" />
				<px:PXDropDown ID="edReportingPeriod" runat="server" DataField="ReportingPeriod" CommitChanges="True" />
				<px:PXTextEdit ID="edHiddenEin" runat="server" DataField="Ein" Enabled="false" />
				<px:PXTextEdit ID="edHiddenAatrixVendorID" runat="server" DataField="AatrixVendorID" Enabled="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="formGrid" runat="server" Width="100%" AllowPaging="True" AdjustPageSize="Auto" SkinID="Inquire" DataSourceID="ds" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Reports">
				<Columns>
					<px:PXGridColumn DataField="FormDisplayName" Width="250px" LinkCommand="RunReport" />
					<px:PXGridColumn DataField="Description" Width="400px"/>
					<px:PXGridColumn DataField="State" Width="80px" />
					<px:PXGridColumn DataField="ReportingPeriod" Width="120px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="300" />
	</px:PXGrid>
	<px:PXSmartPanel runat="server" ID="pnlRunReport" Caption="Run Report" CaptionVisible="true" LoadOnDemand="true"
		Key="CurrentReport" Width="500px" Height="200px" ShowAfterLoad="True" AutoCallBack-Target="formRunReport" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
		<px:PXFormView ID="formRunReport" runat="server" DataMember="CurrentReport" DataSourceID="ds" Width="100%" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="XS" ControlSize="XM" />
					<px:PXTextEdit ID="edHiddenFormName" runat="server" DataField="FormName" Enabled="false" SelectOnFocus="false" />
					<px:PXTextEdit ID="edFormDisplayName" runat="server" DataField="FormDisplayName" Enabled="false" DisableSpellcheck="True" />
					<px:PXSelector ID="edYear" runat="server" DataField="Year" CommitChanges="true" />
					<px:PXDropDown ID="edQuarter" runat="server" DataField="Quarter" CommitChanges="true" />
					<px:PXDropDown ID="edMonth" runat="server" DataField="Month" CommitChanges="true" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="XS" ControlSize="XM" />
					<px:PXDateTimeEdit ID="edDateFrom" runat="server" DataField="DateFrom" CommitChanges="true" />
				<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="XS" ControlSize="XM" />
					<px:PXDateTimeEdit ID="edDateTo" runat="server" DataField="DateTo" CommitChanges="true" />					
			</Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
		<px:PXFormView ID="formRunReportReportingPeriod" runat="server" DataMember="CurrentReport" DataSourceID="ds" Hidden="True">
			<Template>
				<px:PXTextEdit ID="edHiddenReportingPeriod" runat="server" DataField="ReportingPeriod" Enabled="false" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="RunReportOkButton" runat="server" DialogResult="None" Text="Run Report">	
				<ClientEvents Click="runReportClickHandler" />
			</px:PXButton>
			<px:PXButton ID="RunReportCancelButton" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
