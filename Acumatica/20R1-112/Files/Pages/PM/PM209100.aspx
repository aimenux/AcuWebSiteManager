<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM209100.aspx.cs"
    Inherits="Page_PM209100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.TimeEntry" PrimaryView="Items">
	   <CallbackCommands>
		  <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
	   </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Items" NoteIndicator="True"
	   FilesIndicator="True" DefaultControlID="edSubject" Width="100%" AllowCollapse="false">
	   <Template>
		  <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
			 ControlSize="M" />
		  <px:PXSelector ID="edID" runat="server" DataField="NoteID" />
		  <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="ApprovalStatus" CommitChanges="True" />

		  <px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
		  <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
		  <px:PXDateTimeEdit ID="edDate_Date" runat="server" DataField="Date_Date"
			 CommitChanges="True" />
		  <px:PXDateTimeEdit ID="edDate_Time" runat="server" DataField="Date_Time"
			 TimeMode="true" SuppressLabel="true" Width="84" CommitChanges="True" />
		  <px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
		  <px:PXLayoutRule ID="PXLayoutRule9" runat="server" LabelsWidth="S" ControlSize="M" />
		  <px:PXSelector ID="edOwner" runat="server" DataField="OwnerID" CommitChanges="True" AutoRefresh="true" />
		  <px:PXSelector ID="edApprover" runat="server" DataField="ApproverID" Enabled="False" />
		  <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
		  <px:PXSegmentMask ID="edProject" runat="server" DataField="ProjectID" HintField="description" CommitChanges="True" />
		  <px:PXCheckBox ID="edCertifiedjob" runat="server" DataField="CertifiedJob" />
		  <px:PXLayoutRule ID="PXLayoutRule11" runat="server" />
		  <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" HintField="description" AutoRefresh="true" CommitChanges="True" />
		  <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="CostCodeID" AutoRefresh="true" CommitChanges="True" />
		  <px:PXSelector ID="edLaborItemID" runat="server" DataField="LabourItemID" CommitChanges="True" />
		  <px:PXSelector ID="edUnionID" runat="server" DataField="UnionID" />
		  <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
		  <px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" />
		  <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S"
			 ControlSize="M" />


		  <px:PXSelector ID="edEType" runat="server" DataField="EarningTypeID" AutoRefresh="true" CommitChanges="True" />
		  <px:PXSelector ID="edWorkCodeID" runat="server" DataField="WorkCodeID" />
		  <px:PXTimeSpan ID="edTimeSpent" TimeMode="True" runat="server" DataField="TimeSpent" CommitChanges="True" InputMask="hh:mm" Size="SM" />
		  <px:PXTimeSpan TimeMode="true" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" Enabled="False" Size="SM" InputMask="hh:mm" />
		  <px:PXCheckBox ID="chkIsBillable" runat="server" DataField="IsBillable" Text="Billable" CommitChanges="True" />
		  <px:PXCheckBox ID="chkReleased" runat="server" DataField="Released" Text="Released" />
		  <px:PXTimeSpan TimeMode="true" ID="edTimeBillable" runat="server" DataField="TimeBillable" CommitChanges="True" Size="SM" InputMask="hh:mm" />
		  <px:PXTimeSpan TimeMode="true" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" CommitChanges="True" Size="SM" InputMask="hh:mm" />
		  <px:PXTextEdit ID="edEmployeeRate" runat="server" DataField="EmployeeRate" />
		  <px:PXTextEdit ID="edNoteID" runat="server" DataField="NoteID" Visible="False">
			 <AutoCallBack Command="Cancel" Enabled="True" Target="form" />
		  </px:PXTextEdit>

	   </Template>
	   <AutoSize MinHeight="480" Container="Window" Enabled="True" />
    </px:PXFormView>
</asp:Content>
