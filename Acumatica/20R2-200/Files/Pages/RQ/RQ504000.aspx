<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="RQ504000.aspx.cs" Inherits="Page_RQ504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.RQ.RQRequestProcess"
                     PrimaryView="Filter" BorderStyle="NotSet" Width="100%" PageLoadBehavior="PopulateSavedValues"/>
	
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="chkMyOwner ">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />			
		    <px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector Size = "m" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID"  />			
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />				
			<px:PXLayoutRule runat="server" Merge="False" />			
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" Size="m" ID="edWorkGroupID" runat="server" DataField="WorkGroupID"  />			
			<px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />			
            <px:PXLayoutRule runat="server" Merge="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyEscalated" runat="server" DataField="MyEscalated" />			


			<px:PXSelector CommitChanges="True" ID="edReqClassID" runat="server" DataField="ReqClassID" />
			<px:PXLayoutRule runat="server" Merge="True" />

			<px:PXDropDown CommitChanges="True" Size="xs" ID="edSelectedPriority" runat="server" AllowNull="False" DataField="SelectedPriority"  />
			<px:PXCheckBox CommitChanges="True" ID="chkAddExists" runat="server" DataField="AddExists" />
			<px:PXLayoutRule runat="server" Merge="False" />

			<px:PXLayoutRule runat="server" ColumnSpan="2" />

			<px:PXTextEdit CommitChanges="True" ID="edDescription" runat="server" DataField="Description"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

			<px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edDepartmentID" runat="server" DataField="DepartmentID" />
			<px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID"  />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" Caption="Requests" SkinID="PrimaryInquire" SyncPosition="true" FastFilterFields="OrderNbr,InventoryID,DescriptionVendorRefNbr,VendorID,VendorName">
		<Levels>
			<px:PXGridLevel DataMember="Records" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" Enabled="False" AllowEdit="true" />
					<px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" Enabled="False" />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
					<px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" Enabled="False" />
					<px:PXLabel ID="lblSiteIDH" runat="server"></px:PXLabel>
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
					<px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" Enabled="False" />
					<px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" Enabled="False" />
					<px:PXTextEdit ID="edVendorName" runat="server" DataField="VendorName" Enabled="False"  />
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXTextEdit ID="edVendorRefNbr" runat="server" DataField="VendorRefNbr" Enabled="False"  />
					<px:PXTextEdit ID="edVendorDescription" runat="server" DataField="VendorDescription" Enabled="False"  />
					<px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID" Enabled="False"  />
					<px:PXSelector ID="edUOM" runat="server" DataField="UOM" Enabled="False" />
					<px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" Enabled="False" />
					<px:PXNumberEdit ID="edOriginQty" runat="server" DataField="OriginQty" Enabled="False"  />
					<px:PXNumberEdit ID="edIssuedQty" runat="server" DataField="IssuedQty" Enabled="False"  />
					<px:PXNumberEdit ID="edOpenQty" runat="server" DataField="OpenQty" Enabled="False"  />
					<px:PXDropDown ID="edIssueStatus" runat="server" DataField="IssueStatus" Enabled="False"  />
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXNumberEdit ID="edCuryEstUnitCost" runat="server" DataField="CuryEstUnitCost" Enabled="False"  />
					<px:PXNumberEdit ID="edCuryEstExtCost" runat="server" DataField="CuryEstExtCost" Enabled="False"  />
					<px:PXSegmentMask ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" Enabled="False" />
					<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" Enabled="False" />
					<px:PXDateTimeEdit ID="edRequestedDate" runat="server" DataField="RequestedDate" Enabled="False" />
					<px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate" Enabled="False" />
					<px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" Enabled="False" />
					<px:PXCheckBox ID="chkCancelled" runat="server" DataField="Cancelled" Enabled="False" /></RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
					<px:PXGridColumn DataField="Priority" AllowNull="False" RenderEditorText="True" />
					<px:PXGridColumn DataField="OrderNbr" AllowUpdate="False" />
					<px:PXGridColumn DataField="DepartmentID" AllowUpdate="False" />
					<px:PXGridColumn DataField="EmployeeID" AllowUpdate="False" />
					<px:PXGridColumn DataField="OrderDate" AllowUpdate="False" />
					<px:PXGridColumn DataField="ShipDestType" AllowUpdate="False" />
					<px:PXGridColumn DataField="LineNbr" AllowUpdate="False" TextAlign="Right" />					
					<px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="&gt;A" />
					<px:PXGridColumn DataField="Description" AllowUpdate="False" />
					<px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa"  />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OrderQty" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OpenQty" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryEstUnitCost" TextAlign="Right" />					
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryEstExtCost" TextAlign="Right" />
					<px:PXGridColumn AllowUpdate="False" DataField="RequestedDate" />
					<px:PXGridColumn AllowUpdate="False" DataField="PromisedDate" />
					<px:PXGridColumn DataField="VendorID" AllowUpdate="False" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn DataField="VendorName" AllowUpdate="False" />
					<px:PXGridColumn DataField="VendorLocationID" AllowUpdate="False" DisplayFormat="&gt;AAAAAA" />
					<px:PXGridColumn DataField="VendorRefNbr" AllowUpdate="False"  />
					<px:PXGridColumn AllowUpdate="False" DataField="VendorDescription" />
					<px:PXGridColumn AllowUpdate="False" DataField="AlternateID" />
					<px:PXGridColumn AllowUpdate="False" DataField="IssueStatus" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="ExpenseAcctID" DisplayFormat="&gt;######" />
					<px:PXGridColumn AllowUpdate="False" DataField="ExpenseSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <ActionBar DefaultAction="Details"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
