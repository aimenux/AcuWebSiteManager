<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR501000.aspx.cs" Inherits="Page_PR501000" Title="Process Payroll Documents" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.PR.PRDocumentProcess" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XS" ControlSize="M" />
				<px:PXDropDown ID="edAction" runat="server" DataField="Operation" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="grid1" runat="server" Width="100%" AllowPaging="True" AdjustPageSize="Auto" SkinID="Inquire" DataSourceID="ds" ExportNotes="False" NoteIndicator="False" FilesIndicator="False">
		<Levels>
			<px:PXGridLevel DataMember="DocumentList">
				<RowTemplate>
					<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="true" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
					<px:PXGridColumn DataField="RefNbr" Width="120px" />
					<px:PXGridColumn DataField="DocType" Width="120px" />
					<px:PXGridColumn DataField="Status" Width="150px" />
					<px:PXGridColumn DataField="EmployeeID" Width="150px" />
					<px:PXGridColumn DataField="EmployeeID_EPEmployee_acctName" Width="400px" />
					<px:PXGridColumn DataField="GrossAmount" Width="150px" />
					<px:PXGridColumn DataField="DedAmount" Width="150px" />
					<px:PXGridColumn DataField="TaxAmount" Width="150px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="300" />
	</px:PXGrid>
</asp:Content>
