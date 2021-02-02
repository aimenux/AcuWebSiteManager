<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="RQ401000.aspx.cs" Inherits="Page_RQ401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.RQ.RQRequestEnq"
                     PrimaryView="Filter" BorderStyle="NotSet" Width="100%" PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edReqClassID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

			<px:PXSelector CommitChanges="True" ID="edReqClassID" runat="server" DataField="ReqClassID" />
			<px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" AutoRefresh="true" ValueField="AcctCD" />
			<px:PXSelector CommitChanges="True" ID="edDepartmentID" runat="server" DataField="DepartmentID" />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

			<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="true" AllowEdit="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" />
			<px:PXTextEdit CommitChanges="True" ID="edDescription" runat="server" DataField="Description"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" Caption="Requests" SkinID="PrimaryInquire" SyncPosition="true" FastFilterFields="edRQRequest__OrderNbr,InventoryID,Description,VendorID,VendorRefNbr" RestrictFields="True">
		<Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
		<Levels>
			<px:PXGridLevel DataMember="Records" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXLayoutRule runat="server" Merge="True" />

					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector Size="s" ID="edRQRequest__OrderNbr" runat="server" AllowEdit="True" DataField="RQRequest__OrderNbr"  />
					<px:PXLayoutRule runat="server" Merge="False" />

					<px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr"  />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
					<px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description"  />
					<px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" />
					<px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" />
					<px:PXTextEdit ID="edVendorName" runat="server" DataField="VendorName"  />
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXTextEdit ID="edVendorRefNbr" runat="server" DataField="VendorRefNbr"  />
					<px:PXTextEdit ID="edVendorDescription" runat="server" DataField="VendorDescription"  />
					<px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID"  />
					<px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
					<px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" />
					<px:PXNumberEdit ID="edOriginQty" runat="server" DataField="OriginQty" Enabled="False"  />
					<px:PXNumberEdit ID="edIssuedQty" runat="server" DataField="IssuedQty" Enabled="False"  />
					<px:PXNumberEdit ID="edOpenQty" runat="server" DataField="OpenQty" Enabled="False"  />
					<px:PXDropDown ID="edIssueStatus" runat="server" DataField="IssueStatus" Enabled="False"  />
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXNumberEdit ID="edCuryEstUnitCost" runat="server" DataField="CuryEstUnitCost"  />
					<px:PXNumberEdit ID="edCuryEstExtCost" runat="server" DataField="CuryEstExtCost"  />
					<px:PXSegmentMask ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" />
					<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" />
					<px:PXDateTimeEdit ID="edRequestedDate" runat="server" DataField="RequestedDate" />
					<px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate" />
					<px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" />
					<px:PXCheckBox ID="chkCancelled" runat="server" DataField="Cancelled" /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="RQRequest__OrderNbr"  />
					<px:PXGridColumn DataField="RQRequest__ReqClassID" DisplayFormat="&gt;aaaaaaaaaa" Width="100px" />
					<px:PXGridColumn DataField="RQRequest__OrderDate" Width="90px" />					
					<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" Width="81px" />
					<px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;A" Width="80px" />
					<px:PXGridColumn DataField="Description" Width="150px" />
					<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" />
					<px:PXGridColumn DataField="RQRequest__CuryID" DisplayFormat="&gt;LLLLL"  />
					<px:PXGridColumn DataField="CuryEstUnitCost" Width="81px" AllowNull="False" TextAlign="Right" />
					<px:PXGridColumn DataField="OrderQty" AllowNull="False" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryEstExtCost" Width="81px" AllowNull="False" TextAlign="Right" />
					<px:PXGridColumn AllowUpdate="False" DataField="OriginQty" TextAlign="Right" Width="81px" AllowNull="False" />
					<px:PXGridColumn AllowUpdate="False" DataField="IssuedQty" Width="81px" TextAlign="Right" AllowNull="False" />
					<px:PXGridColumn AllowUpdate="False" DataField="OpenQty" TextAlign="Right" Width="81px" AllowNull="False" />
					<px:PXGridColumn DataField="RQRequest__Status" AllowUpdate="False"  />
					<px:PXGridColumn AllowUpdate="False" DataField="IssueStatus" RenderEditorText="True" Width="117px" />
					<px:PXGridColumn DataField="RQRequest__DepartmentID" DisplayFormat="&gt;aaaaaaaaaa"  />
					<px:PXGridColumn DataField="RQRequest__EmployeeID" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn DataField="ExpenseAcctID" DisplayFormat="&gt;######" Width="54px" />
					<px:PXGridColumn DataField="ExpenseSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Width="108px" />
					<px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" />
					<px:PXGridColumn DataField="VendorLocationID" DisplayFormat="&gt;AAAA" Width="54px" />
					<px:PXGridColumn DataField="VendorName" Width="297px" />
					<px:PXGridColumn DataField="VendorRefNbr" Width="90px" />
					<px:PXGridColumn DataField="VendorDescription" Width="500px"  />
					<px:PXGridColumn DataField="AlternateID" Width="180px"  />					
					<px:PXGridColumn DataField="RequestedDate" Width="90px" />
					<px:PXGridColumn DataField="PromisedDate" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <ActionBar  DefaultAction="Details"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
